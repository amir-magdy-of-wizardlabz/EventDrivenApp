using UserService.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container using the extension method
builder.Services.AddUserInfrastructureServices(builder.Configuration);

// Register the core service
builder.Services.AddScoped<UserService.Core.Services.UserService>();

// Add services related to Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
