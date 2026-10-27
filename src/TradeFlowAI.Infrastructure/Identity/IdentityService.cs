using Microsoft.AspNetCore.Identity;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService)
    : IIdentityService
{
    public async Task<(bool Success, string UserId, string? ErrorMessage)> RegisterAsync(
        string email, string password, string fullName, CancellationToken ct = default)
    {
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
            return (false, string.Empty, "User with this email already exists.");

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return (false, string.Empty, errors);
        }

        return (true, user.Id, null);
    }

    public async Task<(bool Success, AuthResponseDto? Response, string? ErrorMessage)> LoginAsync(
        string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return (false, null, "Invalid credentials.");

        var valid = await userManager.CheckPasswordAsync(user, password);
        if (!valid)
            return (false, null, "Invalid credentials.");

        var roles = await userManager.GetRolesAsync(user);
        var (accessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user.Id, user.Email!, user.FullName, roles);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var response = new AuthResponseDto(
            accessToken,
            refreshToken,
            expiresAt,
            user.Id,
            user.Email!,
            user.FullName);

        return (true, response, null);
    }

    public async Task<(bool Success, AuthResponseDto? Response, string? ErrorMessage)> RefreshTokenAsync(
        string token, string refreshToken, CancellationToken ct = default)
    {
        var principal = jwtTokenService.GetPrincipalFromExpiredToken(token);
        if (principal is null)
            return (false, null, "Invalid access token.");

        var email = principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(email))
            return (false, null, "Invalid token claims.");

        var user = await userManager.FindByEmailAsync(email);
        if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return (false, null, "Invalid or expired refresh token.");

        var roles = await userManager.GetRolesAsync(user);
        var (newAccessToken, expiresAt) = jwtTokenService.GenerateAccessToken(user.Id, user.Email!, user.FullName, roles);
        var newRefreshToken = jwtTokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var response = new AuthResponseDto(
            newAccessToken,
            newRefreshToken,
            expiresAt,
            user.Id,
            user.Email!,
            user.FullName);

        return (true, response, null);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        // Find user by refresh token and clear it
        var users = userManager.Users.Where(u => u.RefreshToken == refreshToken).ToList();
        foreach (var user in users)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await userManager.UpdateAsync(user);
        }
    }
}
