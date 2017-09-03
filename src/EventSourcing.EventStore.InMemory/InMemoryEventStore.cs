using EventSourcing.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EventSourcing.EventStore.InMemory
{
	public class InMemoryEventStore : IEventStore
	{
        IDictionary<Guid, List<(Type type,string data)>> _events = new Dictionary<Guid, List<(Type, string)>>();

        public IEnumerable<(Type type,string data)> GetEvents(Guid id)
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
				_events[@event.EntityId].Add((
					typeof(T),
					JsonConvert.SerializeObject(@event)
					));
			}
			else
			{
				_events.Add(@event.EntityId, 
                    new List<(Type, string)> {
						(typeof(T), JsonConvert.SerializeObject(@event))
                    });
			}

			Console.WriteLine("Streaming event for: {0}", @event.EntityId);
		}
	}
}
