using Microsoft.EntityFrameworkCore;
using UnistreamTest;
using UnistreamTest.Infrastructure.Database;
using UnistreamTest.RequestHandlers;
using UnistreamTest.RequestHandlers.Abstract;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddLogging();
// TODO: можно запрятать ошибки в Exception Handler
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.RegisterHandlers();

var redisHost = builder.Configuration["Redis:Host"] ?? throw new NullReferenceException("Redis:Host");
builder.Services.RegisterRedis(redisHost);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
