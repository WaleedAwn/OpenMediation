namespace OpenMediation.Abstractions;

/// <summary>Defines a handler for a request of type <typeparamref name="TRequest"/>.</summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}