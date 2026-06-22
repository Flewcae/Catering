using Catering.BuildingBlocks.CQRS;
using Catering.BuildingBlocks.Messaging;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Exceptions;
using Catering.UserService.Application.IntegrationEvents;
using Catering.UserService.Domain;
using Catering.UserService.Domain.Common;

namespace Catering.UserService.Application.Commands.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IDepartmentRepository departmentRepository,
    IPositionRepository positionRepository,
    IPasswordHasher passwordHasher,
    IEventBus eventBus) : ICommandHandler<RegisterUserCommand, Guid>
{
    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!TcIdentityNumberValidator.IsValid(request.TcIdentityNumber))
        {
            throw new ValidationException("TC Identity Number is not valid.");
        }

        if (await userRepository.GetByEmailAsync(request.Email, cancellationToken) is not null)
        {
            throw new ConflictException($"A user with email '{request.Email}' already exists.");
        }

        if (await userRepository.GetByTcIdentityNumberAsync(request.TcIdentityNumber, cancellationToken) is not null)
        {
            throw new ConflictException("A user with this TC Identity Number already exists.");
        }

        if (await departmentRepository.GetByIdAsync(request.DepartmentId, cancellationToken) is null)
        {
            throw new NotFoundException($"Department '{request.DepartmentId}' was not found.");
        }

        if (await positionRepository.GetByIdAsync(request.PositionId, cancellationToken) is null)
        {
            throw new NotFoundException($"Position '{request.PositionId}' was not found.");
        }

        var passwordHash = passwordHasher.Hash(request.Password);

        var user = User.Register(
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.TcIdentityNumber,
            request.PhoneNumber,
            request.BirthDate,
            request.Address,
            request.DepartmentId,
            request.PositionId,
            request.HireDate,
            request.HasDisability,
            request.DisabilityDescription,
            request.SalaryCeiling,
            request.Notes);

        await userRepository.AddAsync(user, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);

        var integrationEvent = new UserCreatedIntegrationEvent(
            user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.Role.ToString());

        await eventBus.PublishAsync(integrationEvent, KafkaTopics.UserEvents, cancellationToken);

        return user.Id;
    }
}
