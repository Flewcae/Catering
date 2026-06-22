using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.UpdateEmploymentDetails;

public sealed record UpdateEmploymentDetailsCommand(
    Guid UserId,
    Guid DepartmentId,
    Guid PositionId,
    decimal? SalaryCeiling,
    bool HasDisability,
    string? DisabilityDescription,
    string? Notes) : ICommand;
