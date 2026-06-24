using System.Text.Json.Serialization;
using Catering.BuildingBlocks.Authorization;
using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Api.ExceptionHandling;
using Catering.UserService.Api.Security;
using Catering.UserService.Application;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Infrastructure;
using Catering.UserService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

builder.Services.AddUserServiceApplication();
builder.Services.AddUserServiceInfrastructure(builder.Configuration);
builder.Services.AddKafkaEventBus(builder.Configuration);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSharedJwtBearerAuthentication(builder.Configuration);
builder.Services.AddDynamicPermissionPolicies();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(dbContext, passwordHasher);
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
