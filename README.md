# 🎮 CatalogAPI

Este microsserviço é o núcleo de e-commerce da Fiap Cloud Games (FCG). Ele gerencia o **Catálogo de Jogos, Promoções, a Biblioteca do Usuário e o Carrinho/Checkout**.

Foi isolado no Tech Challenge (Fase 2) para garantir maior disponibilidade e independência na jornada de compra dos usuários.

## Funcionalidades
- **Gestão de Jogos e Promoções:** Listagem e associação.
- **Checkout Assíncrono:** Ao "comprar" um jogo, o serviço publica o evento `OrderPlacedEvent` no RabbitMQ, em vez de processar o pagamento diretamente.
- **Consumo de Pagamentos:** Consome ativamente o evento `PaymentProcessedEvent`. Se o pagamento for aprovado, o jogo é permanentemente atribuído à biblioteca do usuário.

## Tecnologias Utilizadas
- C# .NET 8 Web API
- Entity Framework Core (SQLite)
- MassTransit + RabbitMQ (Producer e Consumer)
- Docker (Multi-stage build)

## Como Executar
1. Certifique-se de que o RabbitMQ está rodando (necessário para os fluxos de Checkout).
2. Execute o projeto localmente:
```bash
dotnet build
dotnet run
```
