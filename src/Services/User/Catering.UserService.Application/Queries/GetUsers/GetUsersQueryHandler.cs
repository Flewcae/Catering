using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Abstractions;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IUserRepository userRepository)
    : IQueryHandler<GetUsersQuery, List<UserDto>>
{
    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);

        return users.Select(user => user.ToDto()).ToList();
    }
}
