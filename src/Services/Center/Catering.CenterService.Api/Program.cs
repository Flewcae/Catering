using Catering.BuildingBlocks.Authorization;
using Catering.BuildingBlocks.Messaging;
using Catering.CenterService.Application;
using Catering.CenterService.Infrastructure;
using Catering.CenterService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCenterServiceApplication();
builder.Services.AddCenterServiceInfrastructure(builder.Configuration);
builder.Services.AddKafkaEventBus(builder.Configuration);

builder.Services.AddSharedJwtBearerAuthentication(builder.Configuration);
builder.Services.AddDynamicPermissionPolicies();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CenterDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
