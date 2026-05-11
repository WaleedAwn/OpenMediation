
namespace OpenMediation.Abstractions;

/// <summary>Marker interface for a request that produces a typed response.</summary>
/// <typeparam name="TResponse">The type of the response produced by this request.</typeparam>
#pragma warning disable S2326
public interface IRequest<TResponse>;
#pragma warning restore S2326