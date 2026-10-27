using MediatR;
using TradeFlowAI.Application.Common.Interfaces;

namespace TradeFlowAI.Application.Features.Auth.Queries;

public sealed record UserProfileDto(
    string UserId,
    string Email,
    bool IsAuthenticated);

public sealed record GetCurrentUserQuery : IRequest<UserProfileDto>;

public sealed class GetCurrentUserHandler(ICurrentUserService currentUserService)
    : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    public Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        return Task.FromResult(new UserProfileDto(
            currentUserService.UserId ?? "anonymous",
            currentUserService.Email ?? "anonymous@tradeflow.ai",
            currentUserService.IsAuthenticated));
    }
}
