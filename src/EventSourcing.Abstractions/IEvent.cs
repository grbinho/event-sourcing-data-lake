using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
	public interface IEvent<T>
    {
		//Version
		//Timestamp
		string Mutation { get; set; }
		ICommand Command { get; set; }
		T Entity { get; set; }
    }

	public static class EventExtensions
	{
		public static T Apply<T>(this IEnumerable<IEvent<T>> events) where T: IEventSourced<T>, new()
		{
			var instance = new T();
			// Starting form beginning, there is either create or snapshot event that gives us initial object
			return instance.Apply(events);
		}
	}
}
