using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using UepaMed.Application.Interfaces.Arquivos;
using UepaMed.Application.Interfaces.Artigos;
using UepaMed.Application.Interfaces.Convites;
using UepaMed.Application.Interfaces.Planilhas;
using UepaMed.Application.Interfaces.Revisoes;
using UepaMed.Application.Interfaces.Usuarios;
using UepaMed.Application.Interfaces.Votacoes;
using UepaMed.Application.Services;
using UepaMed.Infrastructure.Data;
using UepaMed.Infrastructure.Importers;
using UepaMed.Infrastructure.Repositories.Arquivos;
using UepaMed.Infrastructure.Repositories.Artigos;
using UepaMed.Infrastructure.Repositories.Planilhas;
using UepaMed.Infrastructure.Repositories.Revisoes;
using UepaMed.Infrastructure.Repositories.Usuarios;
using UepaMed.Infrastructure.Repositories.Votacoes;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<RevisaoService>();
builder.Services.AddScoped<IRevisaoRepository, RevisaoRepository>();

builder.Services.AddScoped<IImportadorArtigos, NbibImportador>();
builder.Services.AddScoped<IImportadorArtigos, RisImportador>();
builder.Services.AddScoped<IArtigoRepository, ArtigoRepository>();
builder.Services.AddScoped<ImportacaoArtigosService>();

builder.Services.AddScoped<
    IArquivoImportacaoRepository,
    ArquivoImportacaoRepository
>();

builder.Services.AddScoped<
    IRevisaoMembroRepository,
    RevisaoMembroRepository
>();

builder.Services.AddScoped<
    IConviteRevisaoRepository,
    ConviteRevisaoRepository
>();

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ConviteRevisaoService>();

builder.Services.AddScoped<IVotacaoRepository, VotacaoRepository>();
builder.Services.AddScoped<VotacaoService>();

builder.Services.AddScoped<
    IDuplicidadeRepository,
    DuplicidadeRepository
>();

builder.Services.AddScoped<DuplicidadeService>();

builder.Services.AddScoped<IPlanilhaRepository, PlanilhaRepository>();
builder.Services.AddScoped<PlanilhaService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:3001",
                "https://localhost:3000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()
        );
    });

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration["Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration["Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:Key"]!
                        )
                    ),

                ClockSkew = TimeSpan.Zero
            };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (
                    string.IsNullOrEmpty(context.Token) &&
                    context.Request.Cookies.TryGetValue(
                        "access_token",
                        out var accessToken
                    )
                )
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var error = context.Features
            .Get<IExceptionHandlerFeature>()?
            .Error;

        var (statusCode, message) = error switch
        {
            ArgumentException exception => (
                StatusCodes.Status400BadRequest,
                exception.Message
            ),

            UnauthorizedAccessException exception => (
                StatusCodes.Status401Unauthorized,
                exception.Message
            ),

            KeyNotFoundException exception => (
                StatusCodes.Status404NotFound,
                exception.Message
            ),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno. Tente novamente mais tarde."
            )
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            message
        });
    });
});

app.UseHttpsRedirection();

app.UseCors("Frontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();