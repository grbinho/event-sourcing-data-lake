using System;
using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
	public class Event<TEntity, TCommand>
    {
		/// <summary>
		/// Timestamp of the event. Utc Ticks.
		/// </summary>
		public long Timestamp { get; set; }
		/// <summary>
		/// Id of the entity this event is related to.
		/// </summary>
		public Guid EntityId { get; set; }
		/// <summary>
		/// Actual entity in the state that corresponds with this event.
		/// </summary>
		public TEntity Entity { get; set; }
		/// <summary>
		/// Mutation that lead to this event
		/// </summary>
		public string Mutation { get; set; }
		/// <summary>
		/// Command that was supplied for the mutation
		/// </summary>
		public TCommand Command { get; set; }
	}

	public static class EventExtensions
	{
		/// <summary>
		/// Replay supplied events to the entity <see cref="T"/>
		/// </summary>
		/// <typeparam name="T">Type of event</typeparam>
		/// <param name="events">Collection of events to replay</param>
		/// <returns></returns>
		public static E Replay<E>(this IEnumerable<Tuple<Type, string>> events) where E : IEventSourced<E>, new()
		{
			var instance = new E();
			// Starting form beginning, there is either create or snapshot event that gives us initial object
			return instance.Replay(events);
		}
	}
}
