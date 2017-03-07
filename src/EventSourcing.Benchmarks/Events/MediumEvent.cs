using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcing.Benchmarks.Events
{
	/// <summary>
	/// Event with 20 fields, some of which are complex types
	/// </summary>
    class MediumEvent: Abstractions.Event
	{
		public int Count { get; set; }
		public double Amount { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
