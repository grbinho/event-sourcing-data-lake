using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
	public interface IEventSourced<T>
    {
		T Apply(IEnumerable<IEvent<T>> events);
    }
}
