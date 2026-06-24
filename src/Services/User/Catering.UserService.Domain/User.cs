using Catering.BuildingBlocks.Domain;
using Catering.UserService.Domain.Enums;
using Catering.UserService.Domain.Events;

namespace Catering.UserService.Domain;

public sealed class User : AggregateRoot
{
    private const int MaxFailedLoginAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string TcIdentityNumber { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;

    public DateOnly? BirthDate { get; private set; }
    public string? Address { get; private set; }
    public string? ProfilePictureUrl { get; private set; }

    public Guid DepartmentId { get; private set; }
    public Department Department { get; private set; } = default!;

    public Guid PositionId { get; private set; }
    public Position Position { get; private set; } = default!;

    public Guid? CenterId { get; private set; }

    public DateOnly HireDate { get; private set; }
    public DateOnly? TerminationDate { get; private set; }

    public UserStatus Status { get; private set; }
    public SystemRole Role { get; private set; }

    public bool HasDisability { get; private set; }
    public string? DisabilityDescription { get; private set; }

    public decimal? SalaryCeiling { get; private set; }

    public string? Notes { get; private set; }

    public DateTimeOffset? LastLoginAt { get; private set; }
    public int FailedLoginAttempts { get; private set; }
    public DateTimeOffset? LockedUntil { get; private set; }

    public bool PasswordRegistered { get; private set; }

    private User()
    {
    }

    public static User Register(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        string tcIdentityNumber,
        string phoneNumber,
        DateOnly? birthDate,
        string? address,
        Guid departmentId,
        Guid positionId,
        DateOnly hireDate,
        bool hasDisability,
        string? disabilityDescription,
        decimal? salaryCeiling,
        string? notes,
        SystemRole role = SystemRole.Employee,
        bool passwordRegistered = false)
    {
        var user = new User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = firstName,
            LastName = lastName,
            TcIdentityNumber = tcIdentityNumber,
            PhoneNumber = phoneNumber,
            BirthDate = birthDate,
            Address = address,
            DepartmentId = departmentId,
            PositionId = positionId,
            HireDate = hireDate,
            Status = UserStatus.Active,
            Role = role,
            HasDisability = hasDisability,
            DisabilityDescription = disabilityDescription,
            SalaryCeiling = salaryCeiling,
            Notes = notes,
            PasswordRegistered = passwordRegistered,
        };

        user.AddDomainEvent(new UserCreatedDomainEvent(user.Id, firstName, lastName, email, phoneNumber, role));

        return user;
    }

    public bool IsLockedOut() => LockedUntil.HasValue && LockedUntil.Value > DateTimeOffset.UtcNow;

    public void RecordLoginSuccess()
    {
        LastLoginAt = DateTimeOffset.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        Touch();
    }

    public void RecordLoginFailure()
    {
        FailedLoginAttempts++;

        if (FailedLoginAttempts >= MaxFailedLoginAttempts)
        {
            LockedUntil = DateTimeOffset.UtcNow.Add(LockoutDuration);
        }

        Touch();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordRegistered = true;
        Touch();
    }

    public void UpdateProfile(string firstName, string lastName, string phoneNumber, string? address, DateOnly? birthDate, string? profilePictureUrl)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        Address = address;
        BirthDate = birthDate;
        ProfilePictureUrl = profilePictureUrl;
        Touch();
    }

    public void UpdateEmploymentDetails(Guid departmentId, Guid positionId, decimal? salaryCeiling, bool hasDisability, string? disabilityDescription, string? notes)
    {
        DepartmentId = departmentId;
        PositionId = positionId;
        SalaryCeiling = salaryCeiling;
        HasDisability = hasDisability;
        DisabilityDescription = disabilityDescription;
        Notes = notes;
        Touch();
    }

    public void ChangeStatus(UserStatus newStatus, DateOnly? terminationDate)
    {
        Status = newStatus;
        TerminationDate = newStatus == UserStatus.Terminated ? terminationDate ?? DateOnly.FromDateTime(DateTime.UtcNow) : null;
        Touch();
    }

    public void AssignCenter(Guid? centerId)
    {
        CenterId = centerId;
        Touch();
    }
}
