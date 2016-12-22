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
			return events;
		}

		public void StreamEvents<T>(IEnumerable<T> events) where T: Event
		{
			foreach (var @event in events)
			{
				StreamEvents<T>(@event);
			}
		}

		public void StreamEvents<T>(T @event) where T: Event
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
