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

        public async Task<PaymentInitiationResult> InitiatePaymentAsync(Guid referenceId, string referenceType, decimal amount, string currency, string? userId, string? customerName = null, string? customerEmail = null, string? customerPhone = null)
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
                var nameParts = (customerName ?? "Customer").Split(' ', 2);
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
                        FirstName = nameParts[0],
                        LastName = nameParts.Length > 1 ? nameParts[1] : "NA",
                        Email = customerEmail ?? "NA",
                        PhoneNumber = customerPhone ?? "NA"
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
            var log = new PaymentWebhookLog
            {
                Id = Guid.NewGuid(),
                Payload = rawPayload,
                SignatureOrHmac = hmac,
                CreatedBy = "PaymobWebhook"
            };

            await _webhookLogRepository.AddAsync(log);
            await _uow.CommitAsync();

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
                var root = JsonDocument.Parse(rawPayload).RootElement;
                var obj = root.GetProperty("obj");

                var success = obj.GetProperty("success").GetBoolean();
                var pending = obj.GetProperty("pending").GetBoolean();

                string? intentionId = null;
                if (obj.TryGetProperty("intention_id", out var intentionElement))
                {
                    intentionId = intentionElement.ToString();
                }

                _logger.LogInformation("Paymob webhook parsed. Success={Success}, Pending={Pending}, IntentionId={IntentionId}",
                    success, pending, intentionId);

                if (string.IsNullOrWhiteSpace(intentionId))
                {
                    log.ProcessingStatus = "IntentionIdMissing";
                    await _webhookLogRepository.UpdateAsync(log);
                    await _uow.CommitAsync();
                    return true;
                }

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

                var order = await _orderRepository.GetByGuidAsync(attempt.OrderId);
                if (order == null)
                {
                    log.ProcessingStatus = "OrderNotFound";
                    await _webhookLogRepository.UpdateAsync(log);
                    await _uow.CommitAsync();
                    return true;
                }

                attempt.PaymobTransactionId = obj.TryGetProperty("id", out var txIdElement)
                    ? txIdElement.ToString()
                    : null;

                attempt.CallbackReceivedAt = DateTime.UtcNow;
                attempt.RawWebhookJson = rawPayload;

                var isFinalSuccess = success && !pending;
                var isFinalFailure = !success && !pending;

                if (isFinalSuccess)
                {
                    attempt.Status = PaymentAttemptStatus.Success;
                    attempt.IsFinal = true;
                    attempt.LastGatewayStatus = "Success";

                    order.Status = PaymentStatus.Paid;
                    order.PaidAt = DateTime.UtcNow;

                    _logger.LogInformation("Before finalizing order. OrderId={OrderId}, Status={Status}, ReferenceType={ReferenceType}, ReferenceId={ReferenceId}",
                        order.Id, order.Status, order.ReferenceType, order.ReferenceId);

                    await FinalizeBusinessOrderAsync(order);

                    await _orderRepository.UpdateAsync(order);

                    _logger.LogInformation("Order marked as paid. OrderId={OrderId}", order.Id);
                }
                else if (isFinalFailure)
                {
                    attempt.Status = PaymentAttemptStatus.Failed;
                    attempt.IsFinal = true;
                    attempt.LastGatewayStatus = "Failed";
                    attempt.FailureMessage = "Payment failed from Paymob callback";

                    order.Status = PaymentStatus.Failed;
                    await _orderRepository.UpdateAsync(order);

                    _logger.LogInformation("Order marked as failed. OrderId={OrderId}", order.Id);
                }
                else
                {
                    attempt.Status = PaymentAttemptStatus.Pending;
                    attempt.LastGatewayStatus = "Pending";

                    _logger.LogInformation("Payment still pending. OrderId={OrderId}", order.Id);
                }

                await _attemptRepository.UpdateAsync(attempt);

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
            _logger.LogInformation("FinalizeBusinessOrderAsync started. OrderId={OrderId}, ReferenceType={ReferenceType}, ReferenceId={ReferenceId}",
                order.Id, order.ReferenceType, order.ReferenceId);

            if (string.Equals(order.ReferenceType, "Booking", StringComparison.OrdinalIgnoreCase))
            {
                var booking = await _bookingRepository.GetByGuidAsync(order.ReferenceId);
                if (booking != null)
                {
                    booking.IsPaid = true;
                    await _bookingRepository.UpdateAsync(booking);
                    _logger.LogInformation("Booking marked as paid. BookingId={BookingId}", booking.Id);
                }
                else
                {
                    _logger.LogWarning("Booking not found. ReferenceId={ReferenceId}", order.ReferenceId);
                }
            }
            else if (string.Equals(order.ReferenceType, "Ticket", StringComparison.OrdinalIgnoreCase))
            {
                var ticket = await _ticketRepository.GetByGuidAsync(order.ReferenceId);
                if (ticket != null)
                {
                    ticket.IsPaid = true;
                    await _ticketRepository.UpdateAsync(ticket);
                    _logger.LogInformation("Ticket marked as paid. TicketId={TicketId}", ticket.Id);
                }
                else
                {
                    _logger.LogWarning("Ticket not found. ReferenceId={ReferenceId}", order.ReferenceId);
                }
            }
            else
            {
                _logger.LogWarning("Unknown ReferenceType={ReferenceType} for OrderId={OrderId}", order.ReferenceType, order.Id);
            }
        }
    }
}
