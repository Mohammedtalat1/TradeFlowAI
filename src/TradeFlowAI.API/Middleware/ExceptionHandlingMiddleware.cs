using System.Net;
using System.Text.Json;
using FluentValidation;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Domain.Exceptions;

namespace TradeFlowAI.API.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        object response;

        switch (exception)
        {
            case ValidationException validationException:
                statusCode = HttpStatusCode.BadRequest;
                var errors = validationException.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                response = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "Validation Failed",
                    status = (int)statusCode,
                    errors
                };
                break;

            case NotFoundException notFoundException:
                statusCode = HttpStatusCode.NotFound;
                response = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                    title = "Resource Not Found",
                    status = (int)statusCode,
                    detail = notFoundException.Message
                };
                break;

            case DomainException domainException:
                statusCode = HttpStatusCode.UnprocessableEntity;
                response = new
                {
                    type = "https://tools.ietf.org/html/rfc4918#section-11.2",
                    title = "Domain Rule Violation",
                    status = (int)statusCode,
                    detail = domainException.Message
                };
                break;

            case UnauthorizedAccessException unauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                response = new
                {
                    type = "https://tools.ietf.org/html/rfc7235#section-3.1",
                    title = "Unauthorized",
                    status = (int)statusCode,
                    detail = unauthorizedAccessException.Message
                };
                break;

            default:
                response = new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                    title = "Internal Server Error",
                    status = (int)statusCode,
                    detail = "An unexpected error occurred while processing your request."
                };
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
