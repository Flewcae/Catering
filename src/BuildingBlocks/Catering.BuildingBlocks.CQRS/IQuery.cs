using MediatR;

namespace Catering.BuildingBlocks.CQRS;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
