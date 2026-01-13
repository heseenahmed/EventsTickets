using Tickets.API.Resources;
using Tickets.Application.Common.Localization;
using Tickets.Application.DTOs;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using System.Data.SqlClient;
using System.Net;
using System.Text.Json;

namespace Tickets.API.Middlewares;

public class GlobalExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;
    private readonly IStringLocalizer<Messages> _localizer;

    public GlobalExceptionHandlingMiddleware(
        ILogger<GlobalExceptionHandlingMiddleware> logger,
        IStringLocalizer<Messages> localizer)
    {
        _logger = logger;
        _localizer = localizer;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed.");
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, 
                _localizer[LocalizationMessages.ValidationError], 
                ex.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList());
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Key not found.");
            await WriteErrorResponseAsync(context, HttpStatusCode.NotFound, 
                _localizer[LocalizationMessages.NotFound], 
                new List<string> { ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid argument.");
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, 
                _localizer[LocalizationMessages.InvalidArgument], 
                new List<string> { ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized.");
            await WriteErrorResponseAsync(context, HttpStatusCode.Unauthorized, 
                _localizer[LocalizationMessages.Unauthorized], 
                new List<string> { ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation.");
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, 
                _localizer[LocalizationMessages.OperationFailed], 
                new List<string> { ex.Message });
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "Invalid operation.");
            await WriteErrorResponseAsync(context, HttpStatusCode.BadRequest, 
                _localizer[LocalizationMessages.OperationFailed], 
                new List<string> { ex.Message });
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error.");
            await WriteErrorResponseAsync(context, HttpStatusCode.Conflict, 
                _localizer[LocalizationMessages.DatabaseError], 
                new List<string>
                {
                    _localizer[LocalizationMessages.DatabaseErrorOccurred],
                    ex.InnerException?.Message ?? ex.Message
                });
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "SQL error occurred.");
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, 
                _localizer[LocalizationMessages.SqlError], 
                new List<string> { ex.Message });
        }
        catch (ApplicationException appEx)
        {
            _logger.LogError(appEx, "Application-level exception.");
            var errors = new List<string>();
            if (!string.IsNullOrEmpty(appEx.Message))
                errors.Add(appEx.Message);
            if (appEx.InnerException != null && !string.IsNullOrEmpty(appEx.InnerException.Message))
                errors.Add(appEx.InnerException.Message);
            await WriteErrorResponseAsync(
                context,
                HttpStatusCode.Conflict,
                _localizer[LocalizationMessages.ApplicationError],
                errors
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            await WriteErrorResponseAsync(context, HttpStatusCode.InternalServerError, 
                _localizer[LocalizationMessages.ServerError], 
                new List<string>
                {
                    _localizer[LocalizationMessages.UnexpectedErrorOccurred],
                    ex.Message
                });
        }
    }

    private static async Task WriteErrorResponseAsync(HttpContext context, HttpStatusCode statusCode, string msg, List<string> errors)
    {
        if (context.Response.HasStarted)
            return;
        var response = new APIResponse<string>
        {
            ApiStatusCode = (int)statusCode,
            Result = "Failed",
            Msg = msg,
            Errors = errors
        };
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });
        await context.Response.WriteAsync(json);
    }
}
