using FluentValidation;
using MediatR;
using TradeFlowAI.Application.Common.Exceptions;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Application.Features.Auth.Commands;

public sealed record RegisterCommand(string Email, string Password, string FullName) : IRequest<string>;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
    }
}

public sealed class RegisterHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, string>
{
    public async Task<string> Handle(RegisterCommand request, CancellationToken ct)
    {
        var (success, userId, error) = await identityService.RegisterAsync(request.Email, request.Password, request.FullName, ct);
        if (!success)
            throw new InvalidOperationException(error ?? "Registration failed.");

        return userId;
    }
}

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResponseDto>;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginHandler(IIdentityService identityService)
    : IRequestHandler<LoginCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken ct)
    {
        var (success, response, error) = await identityService.LoginAsync(request.Email, request.Password, ct);
        if (!success || response is null)
            throw new UnauthorizedAccessException(error ?? "Invalid credentials.");

        return response;
    }
}

public sealed record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<AuthResponseDto>;

public sealed class RefreshTokenHandler(IIdentityService identityService)
    : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var (success, response, error) = await identityService.RefreshTokenAsync(request.AccessToken, request.RefreshToken, ct);
        if (!success || response is null)
            throw new UnauthorizedAccessException(error ?? "Invalid refresh token.");

        return response;
    }
}

public sealed record LogoutCommand(string RefreshToken) : IRequest<bool>;

public sealed class LogoutHandler(IIdentityService identityService)
    : IRequestHandler<LogoutCommand, bool>
{
    public async Task<bool> Handle(LogoutCommand request, CancellationToken ct)
    {
        await identityService.RevokeRefreshTokenAsync(request.RefreshToken, ct);
        return true;
    }
}
