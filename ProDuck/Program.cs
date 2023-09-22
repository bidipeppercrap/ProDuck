using Microsoft.EntityFrameworkCore;
using ProDuck.Models;
using System.Text.Json.Serialization;
using AutoWrapper;

var builder = WebApplication.CreateBuilder(args);

string connectionString = builder.Configuration["Database:ConnectionString"]!;
var serverVersion = new MariaDbServerVersion(new Version(10, 6, 14));

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<ProDuckContext>(opt =>
    opt.UseMySql(connectionString, serverVersion));

var app = builder.Build();

app.UseCors();

app.UseAuthorization();

app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseCustomSchema = true });

app.MapControllers();

app.Run();
