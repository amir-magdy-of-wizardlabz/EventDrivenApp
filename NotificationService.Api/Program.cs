using NotificationService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// This service has no API of its own: it only consumes events and sends email.
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
