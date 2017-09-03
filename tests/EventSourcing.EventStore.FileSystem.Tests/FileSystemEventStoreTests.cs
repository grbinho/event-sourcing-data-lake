using System;
using Xunit;
using EventSourcing.Abstractions;
using EventSourcing.EventStore.FileSystem;
using System.Collections.Generic;
using System.IO;

namespace EventSourcing.EventStore.FileSystem.Tests
{
    public class FileSystemEventStoreTests: IDisposable
    {
        private FileSystemEventStore _eventStore;
        private const string _tenantId = "TestingTenant";
        private readonly string _rootPath;
        private readonly Guid _entityId;

        public FileSystemEventStoreTests()
        {
            _entityId = Guid.NewGuid();
            _rootPath = $@"C:\Temp2\TestStore_{Guid.NewGuid()}";
            _eventStore = new FileSystemEventStore(_tenantId, _rootPath);
        }

        [Fact]
        public void Given_Events_When_StreamEvents_Is_Called_Then_Values_Are_Persisted()
        {
            var events = GetTestEvents();
            _eventStore.StreamEvents<TestEvent>(events);

            var filePath = _eventStore.GetFilePath(_entityId);
            _eventStore.Dispose(); //Release file handles.

            AssertFileContains(filePath, "AdditionalValue1", "AdditionalValue2");
        }

        private List<TestEvent> GetTestEvents()
        {
            var events = new List<TestEvent>();
            events.Add(new TestEvent
            {
                EntityId = _entityId,
                Id = Guid.NewGuid(),
                Mutation = "SomeMutation",
                AdditionalProperty = "AdditionalValue1"
            });
            events.Add(new TestEvent
            {
                EntityId = _entityId,
                Id = Guid.NewGuid(),
                Mutation = "SomeOtherMutation",
                AdditionalProperty = "AdditionalValue2"
            });

            return events;
        }

        private void AssertFileContains(string filePath, params string[] values)
        {
            var seen = new bool[values.Length];

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("Invalid path", nameof(filePath));
            }

            using (var fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var sr = new StreamReader(fs))
            {
                while(!sr.EndOfStream)
                {
                    var line = sr.ReadLine();

                    for(int i=0; i< values.Length; i++)
                    {
                        seen[i] = seen[i] || line.Contains(values[i]);
                    }

                }
            }

            bool result = true;
            foreach(var item in seen)
            {
                result = result && item;
            }

            Assert.True(result);
        }

        public void Dispose()
        {
            Directory.Delete(_rootPath, true);
        }
    }
}
