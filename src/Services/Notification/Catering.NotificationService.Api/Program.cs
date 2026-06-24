using System.Text.Json.Serialization;
using Catering.BuildingBlocks.Authorization;
using Catering.BuildingBlocks.Messaging;
using Catering.NotificationService.Application;
using Catering.NotificationService.Infrastructure;
using Catering.NotificationService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

builder.Services.AddNotificationServiceApplication();
builder.Services.AddNotificationServiceInfrastructure(builder.Configuration);
builder.Services.AddKafkaEventBus(builder.Configuration);

builder.Services.AddSharedJwtBearerAuthentication(builder.Configuration);
builder.Services.AddDynamicPermissionPolicies();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
