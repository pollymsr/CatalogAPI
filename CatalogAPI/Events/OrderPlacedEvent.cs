using System;

namespace FiapCloudGames.Events;

public record OrderPlacedEvent(Guid UserId, Guid GameId, decimal Price);
