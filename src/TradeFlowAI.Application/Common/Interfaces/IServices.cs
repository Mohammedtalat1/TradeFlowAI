namespace TradeFlowAI.Application.Common.Interfaces;

// ── External service contracts (Application defines, Infrastructure implements) ──

public interface ICurrentUserService
{
    string? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}

public interface INotificationService
{
    Task NotifyAsync(string userId, string title, string message, CancellationToken ct = default);
}

public interface IWebhookDispatcher
{
    Task DispatchAsync(string eventType, object payload, Guid? accountId = null, CancellationToken ct = default);
}

public interface IIdentityService
{
    Task<(bool Success, string UserId, string? ErrorMessage)> RegisterAsync(string email, string password, string fullName, CancellationToken ct = default);
    Task<(bool Success, AuthResponseDto? Response, string? ErrorMessage)> LoginAsync(string email, string password, CancellationToken ct = default);
    Task<(bool Success, AuthResponseDto? Response, string? ErrorMessage)> RefreshTokenAsync(string token, string refreshToken, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}

public sealed record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    string UserId,
    string Email,
    string FullName);
