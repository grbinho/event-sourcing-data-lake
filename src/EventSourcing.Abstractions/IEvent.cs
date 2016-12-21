using System;
using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
	public interface IEvent<T>
    {
		/// <summary>
		/// Timestamp of the event. Utc Ticks.
		/// </summary>
		long Timestamp { get; set; }
		/// <summary>
		/// Id of the entity this event is related to.
		/// </summary>
		Guid EntityId { get; set; }
		/// <summary>
		/// Actual entity in the state that corresponds with this event.
		/// </summary>
		T Entity { get; set; }
		/// <summary>
		/// Mutation that lead to this event
		/// </summary>
		string Mutation { get; set; }
		/// <summary>
		/// Command that was supplied for the mutation
		/// </summary>
		ICommand Command { get; set; }
	}

	public static class EventExtensions
	{
		/// <summary>
		/// Replay supplied events to the entity <see cref="T"/>
		/// </summary>
		/// <typeparam name="T">Type of the entity</typeparam>
		/// <param name="events">Collection of events to replay</param>
		/// <returns></returns>
		public static T Replay<T>(this IEnumerable<IEvent<T>> events) where T: IEventSourced<T>, new()
		{
			var instance = new T();
			// Starting form beginning, there is either create or snapshot event that gives us initial object
			return instance.Replay(events);
		}
	}
}
