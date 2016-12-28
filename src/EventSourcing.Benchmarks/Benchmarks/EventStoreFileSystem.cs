using BenchmarkDotNet.Attributes;
using System;
using System.IO;
using EventSourcing.EventStore.FileSystem;

namespace EventSourcing.Benchmarks
{
	/// <summary>
	/// This class holds benchmarks fro Event store using file system as storage
	/// </summary>
	[MemoryDiagnoser]
	public class EventStoreFileSystem
    {
		private const string RelativeRootPath = "./Benchmarks/FileSystemEventStoreRoot/";
		private readonly string _eventStorePath;
		private readonly string _tenantId;
		private readonly FileSystemEventStore _eventStore;
		private readonly Events.MediumEvent _mediumEvent;

		public EventStoreFileSystem()
		{
			_tenantId = Guid.NewGuid().ToString();
			_eventStorePath = Path.Combine(AppContext.BaseDirectory, RelativeRootPath);
			_eventStore = new FileSystemEventStore(_tenantId, _eventStorePath);
			_mediumEvent = CreateMediumEvent();
		}

		private Events.MediumEvent CreateMediumEvent()
		{
			return new Events.MediumEvent
			{
				Id = Guid.NewGuid(),
				EntityId = Guid.NewGuid(),
				Mutation = "Create",
				Timestamp = DateTime.UtcNow.Ticks,
				Amount = 234.41,
				Count = 5,
				Name = "Foo Bar",
				Description = "The quick brown fox jumps over the lazy dog"
			};
		}

		[Setup]
		public void Setup()
		{
			//Make sure root folder for our event store exists
			if(Directory.Exists(_eventStorePath))
				Cleanup();

			Directory.CreateDirectory(_eventStorePath);
		}

		[Cleanup]
		public void Cleanup()
		{
			_eventStore.Dispose();
			//Recursively delete root folder for our event store.
			Directory.Delete(_eventStorePath, true);
		}

		[Benchmark]
		public void CreateSingleEventOnNewEntity()
		{
			_eventStore.StreamEvents(_mediumEvent);
		}
    }
}
