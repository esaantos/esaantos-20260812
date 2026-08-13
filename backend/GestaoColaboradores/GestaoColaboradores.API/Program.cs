using System.Text;
using GestaoColaboradores.API.Domain;
using GestaoColaboradores.API.Infra;
using GestaoColaboradores.API.Infra.Middleware;
using GestaoColaboradores.API.Infra.Repositories;
using GestaoColaboradores.API.Infra.Security;
using GestaoColaboradores.API.Services.Auth;
using GestaoColaboradores.API.Services.Colaboradores;
using GestaoColaboradores.API.Services.Common;
using GestaoColaboradores.API.Services.Unidades;
using GestaoColaboradores.API.Services.Usuarios;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

const string AngularDevCorsPolicy = "AngularDev";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Gestão de Colaboradores e Unidades",
        Version = "v1"
    });

    const string bearerScheme = "Bearer";
    options.AddSecurityDefinition(bearerScheme, new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe apenas o token JWT (sem o prefixo \"Bearer \")."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = bearerScheme }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IUnidadeRepository, UnidadeRepository>();
builder.Services.AddScoped<IUnidadeService, UnidadeService>();
builder.Services.AddScoped<IColaboradorRepository, ColaboradorRepository>();
builder.Services.AddScoped<IColaboradorService, ColaboradorService>();
builder.Services.AddScoped<ICodigoSequencialGenerator, CodigoSequencialGenerator>();

builder.Services.AddCors(options =>
{
    // Origem do Angular em desenvolvimento (`ng serve`, porta padrão do CLI).
    options.AddPolicy(AngularDevCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };

        options.Events = new JwtBearerEvents
        {
            OnChallenge = async context =>
            {
                context.HandleResponse();
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { error = "Token inválido ou ausente" });
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gestão de Colaboradores e Unidades v1");
    });
}

app.UseHttpsRedirection();

app.UseCors(AngularDevCorsPolicy);

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    // Sem endpoint de registro: semeia um usuário admin em banco vazio para permitir o
    // primeiro login local (dev only — nunca roda em produção).
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

    if (!await db.Usuarios.AnyAsync())
    {
        var codigo = await scope.ServiceProvider.GetRequiredService<ICodigoSequencialGenerator>()
            .ProximoAsync("usuario_codigo_seq", "USR", CancellationToken.None);

        var senhaHash = passwordHasher.Hash("Admin@123");
        db.Usuarios.Add(new Usuario("admin", senhaHash, codigo));

        await db.SaveChangesAsync();
    }
}

app.Run();
