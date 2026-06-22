using Catering.UserService.Domain;

namespace Catering.UserService.Application.Dtos;

public static class UserDtoExtensions
{
    public static UserDto ToDto(this User user) => new(
        user.Id,
        user.Email,
        user.FirstName,
        user.LastName,
        user.TcIdentityNumber,
        user.PhoneNumber,
        user.BirthDate,
        user.Address,
        user.ProfilePictureUrl,
        user.DepartmentId,
        user.Department.Name,
        user.PositionId,
        user.Position.Name,
        user.HireDate,
        user.TerminationDate,
        user.Status.ToString(),
        user.Role.ToString(),
        user.HasDisability,
        user.DisabilityDescription,
        user.SalaryCeiling,
        user.Notes,
        user.LastLoginAt,
        user.CreatedAt);
}
