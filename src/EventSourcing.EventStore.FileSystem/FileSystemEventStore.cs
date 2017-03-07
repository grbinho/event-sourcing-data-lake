using EventSourcing.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EventSourcing.EventStore.FileSystem
{
	public sealed class FileSystemEventStore : IEventStore, IDisposable
	{
		private readonly string _tenatId;
		private readonly string _rootPath;

		// TODO: Concurent dictionaries or some other form of sync for concurrency

		private readonly Dictionary<Guid, string> _pathCache = new Dictionary<Guid, string>();
		private readonly Dictionary<Guid, StreamWriter> _writerCache = new Dictionary<Guid, StreamWriter>();
		private readonly Dictionary<Guid, StreamReader> _readerCache = new Dictionary<Guid, StreamReader>();
		private readonly Dictionary<Guid, FileStream> _streamWriterCache = new Dictionary<Guid, FileStream>();
		private readonly Dictionary<Guid, FileStream> _streamReaderCache = new Dictionary<Guid, FileStream>();
		private readonly Stopwatch stopwatch = new Stopwatch();

		/// <summary>
		/// Creates new file system store.
		/// </summary>
		/// <param name="tenantId">Id of the tenant. Determines final path of the event storage.</param>
		/// <param name="storeRootPath">Root folder for event storage.</param>
		public FileSystemEventStore(string tenantId, string storeRootPath)
		{
			_tenatId = tenantId;
			_rootPath = storeRootPath;
		}

		public IEnumerable<Tuple<Type, string>> GetEvents(Guid id)
		{
			return ReadEvents(id);
		}

		public void StreamEvents<T>(T @event) where T : Event
		{
			stopwatch.Restart();
			AppendEventToFile<T>(@event);
			stopwatch.Stop();

			Console.WriteLine($"Event was written: {@event.EntityId}. Time took: {stopwatch.ElapsedMilliseconds} ms.");
		}

		public void StreamEvents<T>(IEnumerable<T> events) where T : Event
		{
			foreach (var @event in events)
				StreamEvents<T>(@event);
		}

		private IEnumerable<Tuple<Type, string>> ReadEvents(Guid entityId)
		{
			var fileContent = string.Empty;
			// TODO: This can also be cached, including the data. Then we just need to read new data.
			// Without Data caching, seek position needs to get reset every time
			var reader = GetReader(entityId, true);
			fileContent = reader.ReadToEnd();
			foreach (var line in fileContent.Split('\n'))
			{
				if(line.Length > 0)
				{
					// In case of reseting the reader, we get an empty character at the begining of first line.
					// TODO: Investigate why.
					var eventLine = JsonConvert.DeserializeObject<EventLine>(line.Trim());
					yield return new Tuple<Type, string>(Type.GetType(eventLine.Type), eventLine.Data);
				}
			}
		}

		private void AppendEventToFile<T>(T @event) where T : Event
		{
			var writer = GetWriter(@event.EntityId);
			//TODO: Check versioning
			var eventLine = new EventLine
			{
				Timestamp = DateTime.UtcNow.Ticks,
				Type = typeof(T).AssemblyQualifiedName,
				Data = JsonConvert.SerializeObject(@event)
			};

			var lineData = JsonConvert.SerializeObject(eventLine);

			//var eventData = JsonConvert.SerializeObject(@event);
			//TOOD: Escape AssemblyQualifiedName
			//writer.WriteLine($"{DateTime.UtcNow.Ticks},{EscapeCsv(typeof(T).AssemblyQualifiedName)},{EscapeCsv(eventData)}\n");
			writer.WriteLine(lineData);
			writer.Flush();
		}

		private StreamWriter GetWriter(Guid entityId)
		{
			FileStream eventsFile;
			StreamWriter eventsWriter;

			if (_streamWriterCache.ContainsKey(entityId))
				eventsFile = _streamWriterCache[entityId];
			else
			{
				eventsFile = File.Open(GetFilePath(entityId), FileMode.Append, FileAccess.Write, FileShare.Read);
				_streamWriterCache.Add(entityId, eventsFile);
			}

			if (_writerCache.ContainsKey(entityId))
				eventsWriter = _writerCache[entityId];
			else
			{
				eventsWriter = new StreamWriter(eventsFile, System.Text.Encoding.UTF8);
				_writerCache.Add(entityId, eventsWriter);
			}

			return eventsWriter;
		}

		private StreamReader GetReader(Guid entityId, bool reset = false)
		{
			FileStream eventsFile;
			StreamReader eventsReader;

			if (_streamReaderCache.ContainsKey(entityId))
				eventsFile = _streamReaderCache[entityId];
			else
			{
				eventsFile = File.Open(GetFilePath(entityId), FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				_streamReaderCache.Add(entityId, eventsFile);
			}

			if (_readerCache.ContainsKey(entityId) && !reset)
				eventsReader = _readerCache[entityId];
			else
			{
				eventsReader = new StreamReader(eventsFile, System.Text.Encoding.UTF8);
				_readerCache[entityId] = eventsReader;
			}

			return eventsReader;
		}

		public string GetFilePath(Guid entityId)
		{
			if(_pathCache.ContainsKey(entityId))
			{
				return _pathCache[entityId];
			}

			var path = Path.Combine(_rootPath, _tenatId, entityId.ToString(), "events.json");
			if(!File.Exists(path))
			{
				var di = Directory.CreateDirectory(Path.GetDirectoryName(path));
			}

			_pathCache[entityId] = path;
			return path;
		}

		//TODO: Build escape csv method!

		private string EscapeCsv(string value)
		{
			char[] csvTokens = new[] { '\"', ',', '\n', '\r' };

			// inside the loop
			if (value.IndexOfAny(csvTokens) >= 0)
			{
				var result = "\"" + value.Replace("\"", "\"\"") + "\"";
				return result;
			}

			return value;
		}

		private string UnescapeCsv(string value)
		{
			//If string starts with quotes, It has been escaped.
			if(value.StartsWith("\""))
			{
				var result = value.TrimStart('"').TrimEnd('"').Replace("\"\"", "\"");
				return result;
			}

			return value;
		}

		public void Dispose()
		{
			Dispose(true);
			// Loop through writers and streams and close them.
			GC.SuppressFinalize(this);
		}
		private void Dispose(bool disposing)
		{
			if(disposing)
			{
				foreach (var w in _writerCache.Values)
					if (w != null) w.Dispose();

				foreach (var s in _streamWriterCache.Values)
					if (s != null) s.Dispose();

				foreach (var r in _readerCache.Values)
					if (r != null) r.Dispose();

				foreach (var sr in _streamReaderCache.Values)
					if (sr != null) sr.Dispose();
			}
		}



		//private string[] GetColumnsFromCsvLine(string line)
		//{
		//	//If line starts with a quote, comma is last first apperance of single ",

		//}

		private class EventLine
		{
			public long Timestamp { get; set; }
			public string Type { get; set; }
			public string Data { get; set; }
		}

	}
}
