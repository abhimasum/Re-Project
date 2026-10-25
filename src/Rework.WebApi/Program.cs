using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Rework.Application;
using Rework.Domain;
using Rework.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("ReworkDb")
    ?? throw new InvalidOperationException("ConnectionStrings:ReworkDb is not configured.");
builder.Services.AddReworkInfrastructure(connectionString);

builder.Services.AddControllers().AddJsonOptions(o =>
    o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks().AddDbContextCheck<ReworkDbContext>("database");

// The Settings Angular app is the only browser client that calls the API directly.
// The MAUI desktop app calls the API from C#, so it needs no CORS entry.
var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(o => o.AddPolicy("SettingsUi", p =>
    p.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Schema changes come from migrations in the release pipeline, not from startup code.
if (app.Environment.IsDevelopment() && builder.Configuration.GetValue<bool>("Database:EnsureCreated"))
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<ReworkDbContext>().Database.EnsureCreatedAsync();
}

app.UseExceptionHandler(errors => errors.Run(async ctx =>
{
    var ex = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, title) = ex switch
    {
        DomainRuleException => (StatusCodes.Status409Conflict, "Business rule violated"),
        NotFoundException => (StatusCodes.Status404NotFound, "Not found"),
        DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Changed by someone else. Reload and retry."),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected error"),
    };
    ctx.Response.StatusCode = status;
    await ctx.Response.WriteAsJsonAsync(new ProblemDetails
    {
        Status = status,
        Title = title,
        Detail = status == StatusCodes.Status500InternalServerError ? null : ex?.Message,
    });
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("SettingsUi");
app.MapControllers();
app.MapHealthChecks("/health/ready");

app.Run();
