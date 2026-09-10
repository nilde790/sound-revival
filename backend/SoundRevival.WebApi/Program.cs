
using Microsoft.EntityFrameworkCore;
using SoundRevival.Repository;
using SoundRevival.Repository.Interfaces;
using SoundRevival.Repository.Repository;
using SoundRevival.Repository.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
// Add services to the container.

builder.Services.AddScoped<IAuthService,AuthService>();
builder.Services.AddScoped<IUserRepository,UserRepository>();   

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
