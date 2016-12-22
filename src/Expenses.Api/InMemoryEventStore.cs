using EventSourcing.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Expenses.Api
{
	public class InMemoryEventStore : IEventStore
	{
		IDictionary<Guid, List<Tuple<Type,string>>> _events = new Dictionary<Guid, List<Tuple<Type, string>>>();
				
		public IEnumerable<Tuple<Type,string>> GetEvents(Guid id)
		{
			var events = _events[id];
			foreach (var @event in events)
			{
				yield return @event;
			}
		}

		public void StreamEvents<T, E, C>(IEnumerable<T> events) where T : Event<E, C>
		{
			foreach (var @event in events)
			{
				StreamEvents<T,E,C>(@event);
			}
		}

		public void StreamEvents<T, E, C>(T @event) where T : Event<E, C>
		{
			if (_events.ContainsKey(@event.EntityId))
			{
				_events[@event.EntityId].Add(new Tuple<Type, string>(
					typeof(T),
					JsonConvert.SerializeObject(@event)
					));
			}
			else
			{
				_events.Add(@event.EntityId,
					new List<Tuple<Type, string>>
					{
						new Tuple<Type, string>(
							typeof(T),
							JsonConvert.SerializeObject(@event)
					)});			
			}

			Console.WriteLine("Streaming event for: {0}", @event.EntityId);
		}
	}
}
