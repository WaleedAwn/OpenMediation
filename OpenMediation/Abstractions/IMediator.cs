
namespace OpenMediation.Abstractions;
/// <summary>Defines the mediator that combines sending requests and publishing notifications.</summary>
public interface IMediator : ISender, IPublisher;
