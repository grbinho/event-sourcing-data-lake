using EventSourcing.Abstractions;

namespace EventSourcing.EventStore.FileSystem.Tests
{
    class TestEvent: Event
    {
		public string AdditionalProperty { get; set; }
	}
}
