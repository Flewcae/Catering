using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Queries.GetUsers;

public sealed record GetUsersQuery : IQuery<List<UserDto>>;
