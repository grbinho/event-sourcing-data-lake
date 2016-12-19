using System;
using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
	public interface IEventStore
    {
		IEnumerable<IEvent<T>> GetEvents<T>(Guid id);
		void StreamEvents<T>(IEnumerable<IEvent<T>> events);
		void StreamEvents<T>(IEvent<T> @event);
	}

	/*
	 * Every write to store should check version information
	 * between current event and last stored event.
	 * Last event stored should have same version value as event comming in.
	 * Act of storing an event changes version value.
	 * To satisfy this, writes need to be atomic and serialized. 
	 */
}
