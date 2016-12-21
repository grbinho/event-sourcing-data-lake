using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
	public interface IEventSourced<T>
    {
		long Version { get; }
		T Replay(IEnumerable<IEvent<T>> events);
    }
}
