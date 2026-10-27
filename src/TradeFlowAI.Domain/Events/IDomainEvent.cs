namespace TradeFlowAI.Domain.Events;

/// <summary>Marker interface for all domain events.</summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
