using Catering.BuildingBlocks.CQRS;

namespace Catering.UserService.Application.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string PhoneNumber,
    string? Address,
    DateOnly? BirthDate,
    string? ProfilePictureUrl) : ICommand;
