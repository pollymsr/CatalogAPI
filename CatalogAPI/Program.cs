using CatalogAPI.Consumers;
using CatalogAPI.Data;
using CatalogAPI.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlite("Data Source=catalog.db"));

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPromotionService, PromotionService>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PaymentProcessedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration["RabbitMQ:Host"] ?? "localhost";
        cfg.Host(rabbitHost, "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
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
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = "FiapCloudGames",
        ValidateAudience = true,
        ValidAudience = "FiapCloudGamesUsers"
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

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.EnsureCreated();

    if (!db.Games.Any())
    {
        db.Games.AddRange(
            new CatalogAPI.Entities.Game { Id = Guid.NewGuid(), Title = "Elden Ring: Shadow of the Erdtree", Description = "A épica expansão...", Price = 199.90m, Genre = "Action RPG", ReleaseDate = new DateTime(2024, 6, 21).ToUniversalTime() },
            new CatalogAPI.Entities.Game { Id = Guid.NewGuid(), Title = "Black Myth: Wukong", Description = "RPG de ação focado em mitologia...", Price = 249.99m, Genre = "Action RPG", ReleaseDate = new DateTime(2024, 8, 20).ToUniversalTime() },
            new CatalogAPI.Entities.Game { Id = Guid.NewGuid(), Title = "Senua's Saga: Hellblade II", Description = "Jornada brutal de sobrevivência...", Price = 229.00m, Genre = "Action Adventure", ReleaseDate = new DateTime(2024, 5, 21).ToUniversalTime() }
        );
        db.SaveChanges();
    }
}

app.Run();
