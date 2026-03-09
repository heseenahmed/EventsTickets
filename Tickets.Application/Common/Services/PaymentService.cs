using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tickets.Application.Common.Interfaces;
using Tickets.Application.Common.Models;
using Tickets.Application.DTOs.Paymob;
using Tickets.Domain.Entity;
using Tickets.Domain.Enums;
using Tickets.Domain.IRepository;

namespace Tickets.Application.Common.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentAttemptRepository _attemptRepository;
        private readonly IPaymentWebhookLogRepository _webhookLogRepository;
        private readonly IBookingRepository _bookingRepository;
        private readonly ITicketRepository _ticketRepository;
        private readonly IPaymobClient _paymobClient;
        private readonly IPaymobWebhookVerifier _verifier;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PaymentService> _logger;
        private readonly PaymobOptions _options;

        public PaymentService(
            IOrderRepository orderRepository,
            IPaymentAttemptRepository attemptRepository,
            IPaymentWebhookLogRepository webhookLogRepository,
            IBookingRepository bookingRepository,
            ITicketRepository ticketRepository,
            IPaymobClient paymobClient,
            IPaymobWebhookVerifier verifier,
            IUnitOfWork uow,
            ILogger<PaymentService> logger,
            IOptions<PaymobOptions> options)
        {
            _orderRepository = orderRepository;
            _attemptRepository = attemptRepository;
            _webhookLogRepository = webhookLogRepository;
            _bookingRepository = bookingRepository;
            _ticketRepository = ticketRepository;
            _paymobClient = paymobClient;
            _verifier = verifier;
            _uow = uow;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<PaymentInitiationResult> InitiatePaymentAsync(Guid referenceId, string referenceType, decimal amount, string currency, string? userId)
        {
            // 1. Check if an order already exists for this reference
            var order = await _orderRepository.GetAllQueryable()
                .FirstOrDefaultAsync(o => o.ReferenceId == referenceId && o.ReferenceType == referenceType);

            if (order == null)
            {
                order = new Order
                {
                    Id = Guid.NewGuid(),
                    ReferenceId = referenceId,
                    ReferenceType = referenceType,
                    Amount = amount,
                    Currency = currency,
                    UserId = userId,
                    Status = PaymentStatus.Pending,
                    CreatedBy = userId ?? "System"
                };
                await _orderRepository.AddAsync(order);
            }
            else if (order.Status == PaymentStatus.Paid)
            {
                throw new Exception("Order is already paid.");
            }

            // 2. Create a new Payment Attempt
            var attempt = new PaymentAttempt
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Amount = amount,
                Currency = currency,
                Status = PaymentAttemptStatus.Initiated,
                IdempotencyKey = Guid.NewGuid().ToString(),
                CreatedBy = userId ?? "System"
            };
            await _attemptRepository.AddAsync(attempt);
            await _uow.CommitAsync();

            try
            {
                // 3. Call Paymob API
                var intentionRequest = new PaymobIntentionRequest
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = currency,
                    PaymentMethods = _options.IntegrationId > 0 ? new List<int> { _options.IntegrationId } : new List<int>(),
                    SpecialReference = attempt.Id.ToString(),
                    NotificationUrl = _options.WebhookUrl,
                    RedirectionUrl = _options.ReturnUrl,
                    BillingData = new PaymobBillingData
                    {
                        FirstName = "Customer",
                        LastName = "NA",
                        Email = "customer@example.com",
                        PhoneNumber = userId ?? "NA"
                    }
                };

                var intentionResponse = await _paymobClient.CreateIntentionAsync(intentionRequest);

                // 4. Update attempt with Paymob data
                attempt.PaymobIntentionId = intentionResponse.Id;
                attempt.Status = PaymentAttemptStatus.Pending;
                
                // Construct Checkout URL using Paymob Unified Checkout format
                attempt.CheckoutUrl = $"https://accept.paymob.com/unifiedcheckout/?publicKey={_options.PublicKey}&clientSecret={intentionResponse.ClientSecret}";

                await _attemptRepository.UpdateAsync(attempt);
                await _uow.CommitAsync();

                return new PaymentInitiationResult
                {
                    OrderId = order.Id,
                    PaymentAttemptId = attempt.Id,
                    CheckoutUrl = attempt.CheckoutUrl,
                    Status = order.Status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initiate Paymob payment for Order {OrderId}", order.Id);
                attempt.Status = PaymentAttemptStatus.Failed;
                attempt.FailureMessage = ex.Message;
                await _attemptRepository.UpdateAsync(attempt);
                await _uow.CommitAsync();
                throw;
            }
        }

        public async Task<bool> ProcessCallbackAsync(string hmac, string rawPayload)
        {
            // 1. Log webhook
            var log = new PaymentWebhookLog
            {
                Id = Guid.NewGuid(),
                Payload = rawPayload,
                SignatureOrHmac = hmac,
                CreatedBy = "PaymobWebhook"
            };
            await _webhookLogRepository.AddAsync(log);
            await _uow.CommitAsync();

            // 2. Verify HMAC
            if (!_verifier.VerifyHmac(rawPayload, hmac))
            {
                log.IsVerified = false;
                log.ProcessingStatus = "InvalidSignature";
                await _webhookLogRepository.UpdateAsync(log);
                await _uow.CommitAsync();
                return false;
            }

            log.IsVerified = true;

            try
            {
                // 3. Parse payload
                var root = JsonDocument.Parse(rawPayload).RootElement;
                var obj = root.GetProperty("obj");
                var success = obj.GetProperty("success").GetBoolean();
                var pending = obj.GetProperty("pending").GetBoolean();
                
                string? intentionId = null;
                if (obj.TryGetProperty("intention_id", out var intentionElement))
                {
                    intentionId = intentionElement.GetString();
                }

                if (string.IsNullOrEmpty(intentionId))
                {
                    log.ProcessingStatus = "IntentionIdMissing";
                    await _webhookLogRepository.UpdateAsync(log);
                    await _uow.CommitAsync();
                    return true;
                }
                
                // Find attempt
                var attempt = await _attemptRepository.GetAllQueryable()
                    .FirstOrDefaultAsync(a => a.PaymobIntentionId == intentionId);

                if (attempt == null)
                {
                    log.ProcessingStatus = "AttemptNotFound";
                    await _webhookLogRepository.UpdateAsync(log);
                    await _uow.CommitAsync();
                    return true;
                }

                if (attempt.IsFinal)
                {
                    log.ProcessingStatus = "AlreadyProcessed";
                    await _webhookLogRepository.UpdateAsync(log);
                    await _uow.CommitAsync();
                    return true;
                }

                // 4. Update statuses
                attempt.PaymobTransactionId = obj.GetProperty("id").GetRawText();
                attempt.CallbackReceivedAt = DateTime.UtcNow;
                attempt.RawWebhookJson = rawPayload;

                var order = await _orderRepository.GetByGuidAsync(attempt.OrderId);

                if (success)
                {
                    attempt.Status = PaymentAttemptStatus.Success;
                    attempt.IsFinal = true;
                    order.Status = PaymentStatus.Paid;
                    order.PaidAt = DateTime.UtcNow;

                    await FinalizeBusinessOrderAsync(order);
                }
                else if (!pending)
                {
                    attempt.Status = PaymentAttemptStatus.Failed;
                    attempt.IsFinal = true;
                }

                await _attemptRepository.UpdateAsync(attempt);
                await _orderRepository.UpdateAsync(order);
                
                log.ProcessingStatus = "Success";
                log.ProcessedAt = DateTime.UtcNow;
                await _webhookLogRepository.UpdateAsync(log);
                await _uow.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Paymob callback");
                log.ProcessingStatus = "Error";
                log.ProcessingError = ex.Message;
                await _webhookLogRepository.UpdateAsync(log);
                await _uow.CommitAsync();
                return false;
            }
        }

        public async Task<PaymentStatusResponse> GetPaymentStatusAsync(Guid referenceId, string referenceType)
        {
            var order = await _orderRepository.GetAllQueryable()
                .FirstOrDefaultAsync(o => o.ReferenceId == referenceId && o.ReferenceType == referenceType);

            if (order == null)
            {
                return new PaymentStatusResponse { Status = PaymentStatus.Unpaid, Message = "No order found." };
            }

            return new PaymentStatusResponse
            {
                OrderId = order.Id,
                Status = order.Status,
                Message = order.Status.ToString()
            };
        }

        private async Task FinalizeBusinessOrderAsync(Order order)
        {
            if (order.ReferenceType == "Booking")
            {
                var booking = await _bookingRepository.GetByGuidAsync(order.ReferenceId);
                if (booking != null)
                {
                    booking.IsPaid = true;
                    await _bookingRepository.UpdateAsync(booking);
                }
            }
            else if (order.ReferenceType == "Ticket")
            {
                var ticket = await _ticketRepository.GetByGuidAsync(order.ReferenceId);
                if (ticket != null)
                {
                    ticket.IsPaid = true;
                    await _ticketRepository.UpdateAsync(ticket);
                }
            }
        }
    }
}
