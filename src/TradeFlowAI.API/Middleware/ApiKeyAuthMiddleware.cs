using System.Security.Claims;

namespace TradeFlowAI.API.Middleware;

public sealed class ApiKeyAuthMiddleware(RequestDelegate next)
{
    private const string ApiKeyHeaderName = "X-API-KEY";

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
        {
            var keyString = extractedApiKey.ToString();
            // Validate prefix and format
            if (keyString.StartsWith("tfai_") && keyString.Length >= 20)
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "api-partner"),
                    new Claim(ClaimTypes.Role, "IntegrationPartner"),
                    new Claim("ApiKeyPrefix", keyString[..8])
                };

                var identity = new ClaimsIdentity(claims, "ApiKey");
                context.User = new ClaimsPrincipal(identity);
            }
        }

        await next(context);
    }
}
