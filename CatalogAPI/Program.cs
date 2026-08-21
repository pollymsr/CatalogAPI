using CatalogAPI.Consumers;
using CatalogAPI.Data;
using CatalogAPI.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

using MongoDB.Driver;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenTelemetry()
    .WithMetrics(builder =>
    {
        builder.AddPrometheusExporter();
        builder.AddMeter("Microsoft.AspNetCore.Hosting", "Microsoft.AspNetCore.Server.Kestrel");
        builder.AddView("http.server.request.duration",
            new ExplicitBucketHistogramConfiguration
            {
                Boundaries = new double[] { 0, 0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10 }
            });
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {seu_token}"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddSingleton<CatalogMongoContext>();

var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConn;
});

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentProcessedEventConsumer>();

    x.UsingAmazonSqs((context, cfg) =>
    {
        cfg.Host("us-east-1", h =>
        {
            // Pega as credenciais automaticamente do perfil AWS local
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var jwtKey = builder.Configuration["JWT_SECRET_KEY"] ?? "c6b5cbdc128daa0d2cd2726eacaae1266f8c8ee24fffd3e3f2bac1302b55069f";
var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = false,
        RequireSignedTokens = false,
        SignatureValidator = (string token, TokenValidationParameters validationParameters) => 
            new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(token)
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();
app.MapControllers();

// Seeder MongoDB
using (var scope = app.Services.CreateScope())
{
    var mongoContext = scope.ServiceProvider.GetRequiredService<CatalogMongoContext>();
    var gamesCount = mongoContext.Games.CountDocuments(FilterDefinition<CatalogAPI.Entities.Game>.Empty);
    if (gamesCount == 0)
    {
        var seedGames = new List<CatalogAPI.Entities.Game>
        {
            new CatalogAPI.Entities.Game { Id = Guid.NewGuid(), Title = "Elden Ring: Shadow of the Erdtree", Description = "A épica expansão...", Price = 199.90m, Genre = "Action RPG", ReleaseDate = new DateTime(2024, 6, 21).ToUniversalTime() },
            new CatalogAPI.Entities.Game { Id = Guid.NewGuid(), Title = "Black Myth: Wukong", Description = "RPG de ação focado em mitologia...", Price = 249.99m, Genre = "Action RPG", ReleaseDate = new DateTime(2024, 8, 20).ToUniversalTime() },
            new CatalogAPI.Entities.Game { Id = Guid.NewGuid(), Title = "Senua's Saga: Hellblade II", Description = "Jornada brutal de sobrevivência...", Price = 229.00m, Genre = "Action Adventure", ReleaseDate = new DateTime(2024, 5, 21).ToUniversalTime() }
        };
        mongoContext.Games.InsertMany(seedGames);
    }
}

app.Run();
