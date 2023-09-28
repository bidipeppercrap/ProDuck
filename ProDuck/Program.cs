using Microsoft.EntityFrameworkCore;
using ProDuck.Models;
using System.Text.Json.Serialization;
using AutoWrapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SigningKey"]!))
    };
});

var app = builder.Build();

app.UseCors();

app.UseAuthorization();

app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { UseCustomSchema = true });

app.MapControllers();

app.Run();
