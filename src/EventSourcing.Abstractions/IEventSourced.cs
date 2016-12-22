using System;
using System.Collections.Generic;

namespace EventSourcing.Abstractions
{
	public interface IEventSourced<E>
    {
		long Version { get; }
		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="T">Actual Event type</typeparam>
		/// <typeparam name="C">Type of a command</typeparam>
		/// <param name="events"></param>
		/// <returns></returns>
		E Replay(IEnumerable<Tuple<Type, string>> events);
    }
}
