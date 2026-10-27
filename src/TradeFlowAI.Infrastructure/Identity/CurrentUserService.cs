using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Infrastructure.Identity;

public sealed class CurrentUserService(IHttpContextAccessor? httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor?.HttpContext?.User;

    public string? UserId => User?.FindFirstValue(ClaimTypes.NameIdentifier);
    public string? Email => User?.FindFirstValue(ClaimTypes.Email);
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
