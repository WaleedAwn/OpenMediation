
namespace OpenMediation.Abstractions;

/// <summary>Sends a request through the mediator pipeline to a single handler.</summary>
public interface ISender
{
    /// <summary>
    /// Sends the specified request through the pipeline and returns the handler response.
    /// The dispatcher resolves the matching request handler and all registered pipeline behaviors
    /// for the concrete runtime type of the request.
    /// </summary>
    /// <typeparam name="TResponse">The type of response returned by the request handler.</typeparam>
    /// <param name="request">The request instance to dispatch.</param>
    /// <param name="cancellationToken">A token used to propagate cancellation.</param>
    /// <returns>The response returned by the request handler.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="request"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when no handler or more than one handler is registered for the request type.
    /// </exception>
    Task<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
}



