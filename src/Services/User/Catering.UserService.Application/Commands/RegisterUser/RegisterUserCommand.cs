using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string TcIdentityNumber,
    string PhoneNumber,
    DateOnly? BirthDate,
    string? Address,
    Guid DepartmentId,
    Guid PositionId,
    DateOnly HireDate,
    bool HasDisability,
    string? DisabilityDescription,
    decimal? SalaryCeiling,
    string? Notes) : ICommand<Guid>;
