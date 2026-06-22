using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Exceptions;

namespace Catering.UserService.Application.Commands.UpdateProfile;

public sealed class UpdateProfileCommandHandler(IUserRepository userRepository) : ICommandHandler<UpdateProfileCommand>
{
    public async Task Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"User '{request.UserId}' was not found.");

        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber, request.Address, request.BirthDate, request.ProfilePictureUrl);

        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
