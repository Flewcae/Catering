using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.CreateUserAccount;

public sealed record CreateUserAccountCommand(
    string Email,
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
