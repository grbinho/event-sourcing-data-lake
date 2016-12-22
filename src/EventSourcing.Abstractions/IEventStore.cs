using System;
using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
	/*
	 * Store implementation handles tenancy!
	 */

	public interface IEventStore
    {
		// If we return Tuple<Type,string> We need to handle deserialization, but that is probably the only way it can work
		IEnumerable<Tuple<Type, string>> GetEvents(Guid id);
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T">Actual event type</typeparam>
		/// <typeparam name="E">Entity type</typeparam>
		/// <typeparam name="C">Command type</typeparam>
		/// <param name="events"></param>
		void StreamEvents<T,E,C>(IEnumerable<T> events) where T: Event<E,C>;
		void StreamEvents<T,E,C>(T @event) where T: Event<E,C>;
	}

	/*
	 * Every write to store should check version information
	 * between current event and last stored event.
	 * Last event stored should have same version value as event comming in.
	 * Act of storing an event changes version value.
	 * To satisfy this, writes need to be atomic and serialized. 
	 */
}
