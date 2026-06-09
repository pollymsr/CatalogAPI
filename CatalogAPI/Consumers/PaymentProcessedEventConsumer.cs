using System;
using System.Threading.Tasks;
using CatalogAPI.Data;
using CatalogAPI.Entities;
using FiapCloudGames.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace CatalogAPI.Consumers;

public class PaymentProcessedEventConsumer : IConsumer<PaymentProcessedEvent>
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<PaymentProcessedEventConsumer> _logger;

    public PaymentProcessedEventConsumer(CatalogDbContext context, ILogger<PaymentProcessedEventConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentProcessedEvent> context)
    {
        var message = context.Message;
        _logger.LogInformation("Recebido PaymentProcessedEvent para UserId: {UserId}, GameId: {GameId}, Status: {Status}", message.UserId, message.GameId, message.Status);

        if (message.Status == "Approved")
        {
            var alreadyOwned = await _context.UserGames
                .AnyAsync(ug => ug.UserId == message.UserId && ug.GameId == message.GameId);

            if (!alreadyOwned)
            {
                var userGame = new UserGame
                {
                    UserId = message.UserId,
                    GameId = message.GameId,
                    PurchaseDate = DateTime.UtcNow
                };

                _context.UserGames.Add(userGame);
                await _context.SaveChangesAsync();
                _logger.LogInformation("GameId {GameId} adicionado a biblioteca do UserId {UserId}.", message.GameId, message.UserId);
            }
            else
            {
                _logger.LogInformation("Usuário {UserId} já possui o GameId {GameId}.", message.UserId, message.GameId);
            }
        }
    }
}
