using ChatGPTcorpus.Services;
using ChatGPTcorpus.Models;
using ChatGPTcorpus.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();

// Add DbContext
var connectionString = configuration.GetConnectionString("Default");

if (string.IsNullOrWhiteSpace(connectionString))
{
    var dbHost = configuration["DB_HOST"] ?? configuration["POSTGRES_HOST"] ?? "db";
    var dbPort = configuration["DB_PORT"] ?? "5432";
    var dbName = configuration["DB_NAME"] ?? configuration["POSTGRES_DB"] ?? "app";
    var dbUser = configuration["DB_USER"] ?? configuration["POSTGRES_USER"] ?? "app";
    var dbPassword = configuration["DB_PASSWORD"] ?? configuration["POSTGRES_PASSWORD"] ?? "app";

    connectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUser};Password={dbPassword}";
}

builder.Services.AddDbContext<KorpusDbContext>(options =>
    options.UseNpgsql(connectionString));

// Register custom services
builder.Services.AddSingleton<ZipService>();
builder.Services.AddScoped<ImportService>(sp => 
    new ImportService(
        System.IO.Path.Combine(builder.Environment.ContentRootPath, "Data"),
        sp.GetRequiredService<KorpusDbContext>()
    ));
builder.Services.AddSingleton<SearchServiceProvider>(); // Helper for in-memory conversations

// Allow very large uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

// Add CORS configuration
var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

if (configuredOrigins.Length == 0)
{
    var originsFromEnv = configuration["CORS_ORIGINS"];
    if (!string.IsNullOrWhiteSpace(originsFromEnv))
    {
        configuredOrigins = originsFromEnv
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(origin => origin.Trim())
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToArray();
    }
}

if (configuredOrigins.Length == 0)
{
    configuredOrigins = new[] { "http://localhost:5173", "http://localhost:5174" };
}

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(configuredOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // app.UseSwagger();
    // app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Add CORS middleware before routing
app.UseCors();

app.MapControllers();

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<KorpusDbContext>();
    dbContext.Database.Migrate();
}

app.Run();

// Helper for in-memory conversations
public class SearchServiceProvider
{
    private List<Conversation> _conversations = new List<Conversation>();
    public List<Conversation> Conversations => _conversations;
    public void SetConversations(List<Conversation> conversations) => _conversations = conversations;
}

public partial class Program { }
