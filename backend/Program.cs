using ChatGPTcorpus.Services;
using ChatGPTcorpus.Models;
using ChatGPTcorpus.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddNewtonsoftJson();
builder.Services.AddEndpointsApiExplorer();

// Add DbContext
builder.Services.AddDbContext<KorpusDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register custom services
builder.Services.AddSingleton<ZipService>();
builder.Services.AddScoped<ImportService>(sp => 
    new ImportService(
        System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "Data"),
        sp.GetRequiredService<KorpusDbContext>()
    ));
builder.Services.AddSingleton<SearchServiceProvider>(); // Helper for in-memory conversations

// Allow very large uploads
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = long.MaxValue;
});

// Add CORS configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
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
