using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using CoworkingReservation.Infrastructure.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using CoworkingReservation.Infrastructure.UnitOfWork;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Features;
using CoworkingReservation.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using CoworkingReservation.Application.Jobs;
using CoworkingReservation.Application.Queries;
using CoworkingReservation.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.WriteIndented = true;
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Registrar el contexto de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar el repositorio genérico
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ICoworkingSpaceRepository, CoworkingSpaceRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<ICoworkingAvailabilityRepository, CoworkingAvailabilityRepository>();
builder.Services.AddScoped<ICoworkingAreaRepository, CoworkingAreaRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<ISpecialFeatureRepository, SpecialFeatureRepository>();
builder.Services.AddScoped<ISafetyElementRepository, SafetyElementRepository>();
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(GetCoworkingSpaceByIdHandler).Assembly);
});



// Registrar los servicios
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICoworkingSpaceService, CoworkingSpaceService>();
builder.Services.AddScoped<IBenefitService, BenefitService>();
builder.Services.AddScoped<IServiceOfferedService, ServiceOfferedService>();
builder.Services.AddScoped<ICoworkingAreaService, CoworkingAreaService>();
builder.Services.AddScoped<ISpecialFeatureService, SpecialFeatureService>();
builder.Services.AddScoped<ISafetyElementService, SafetyElementService>();

builder.Services.AddAutoMapper(typeof(CoworkingSpaceProfile).Assembly);

builder.Services.AddScoped<CoworkingApprovalJob>();
builder.Services.AddSingleton<IServiceScopeFactory>(sp => sp.GetRequiredService<IServiceScopeFactory>());

// Agregar configuración de JWT
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB
});
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),

        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ClientPolicy", policy =>
        policy.RequireRole("Client"));

    options.AddPolicy("HosterPolicy", policy =>
        policy.RequireRole("Hoster"));
});

var app = builder.Build();

// Ejecutar la Seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Usar la política de CORS
app.UseCors("AllowAngularApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
using (var scope = app.Services.CreateScope())
{
    var approvalJob = scope.ServiceProvider.GetRequiredService<CoworkingApprovalJob>();
    _ = Task.Run(async () => await approvalJob.Run());
}

app.Run();
