using CoworkingReservation.Application.Services.Interfaces;
using CoworkingReservation.Application.Services;
using CoworkingReservation.Domain.IRepository;
using CoworkingReservation.Infrastructure.Data;
using CoworkingReservation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registrar el contexto de la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar el repositorio genérico
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Registrar el servicio de User
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Ejecutar la Seed
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await context.Database.MigrateAsync(); // Aplicar migraciones
    await DatabaseSeeder.SeedAsync(context); // Ejecutar la seed
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
