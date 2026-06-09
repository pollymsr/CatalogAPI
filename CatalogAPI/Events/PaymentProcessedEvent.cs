using System;

namespace FiapCloudGames.Events;

public record PaymentProcessedEvent(Guid UserId, Guid GameId, string Status);
