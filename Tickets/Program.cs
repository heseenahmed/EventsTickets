using Tickets.API;
using Tickets.API.Middlewares;
using Tickets.Application;
using Tickets.Domain.Entity;
using Tickets.Infra;
using Tickets.Infra.Data;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Layer registrations
builder.Services.AddAppServices();
builder.Services.AddInfraServices(builder.Configuration, builder.Environment);
builder.Services.AddAPIServices(builder.Configuration);

var app = builder.Build();

// Seed roles
try
{
    await DbSeeder.SeedRolesAsync(app.Services);
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during role seeding. The app will continue to start.");
}

// Enable Swagger in all environments (including Production)
app.UseSwagger();
app.UseSwaggerUI(c => 
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tickets API V1");
    c.RoutePrefix = string.Empty; // Serve Swagger UI at the root (/)
    c.DisplayRequestDuration();
});

// Fallback for root URL to ensure something is served
app.MapGet("/health", () => Results.Ok("Tickets API is running"));

FileHelperStartup.Configure(app); 

// Middleware (Exception first)
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();

// Static files
app.UseStaticFiles();

// Localization (MUST be before auth)
app.UseRequestLocalization(
    app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

// CORS
app.UseCors("MyPolicy");

// Security & pipeline
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
