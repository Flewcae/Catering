using Catering.BuildingBlocks.CQRS;
using Catering.UserService.Application.Dtos;

namespace Catering.UserService.Application.Queries.GetUserById;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<UserDto?>;
