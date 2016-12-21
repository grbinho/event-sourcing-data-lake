using EventSourcing.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Expenses.Api
{
	public class NullEventStore : IEventStore
	{
		public IEnumerable<IEvent<T>> GetEvents<T>(Guid id)
		{
			Console.WriteLine("Getting events for {0}", id);
			return new List<IEvent<T>>();
		}

		public void StreamEvents<T>(IEvent<T> @event)
		{

			Console.WriteLine("Streaming event: {0}", JsonConvert.SerializeObject(@event));
		}

		public void StreamEvents<T>(IEnumerable<IEvent<T>> events)
		{
			foreach(var @event in events)
			{
				StreamEvents<T>(@event);
			}
		}
	}
}
