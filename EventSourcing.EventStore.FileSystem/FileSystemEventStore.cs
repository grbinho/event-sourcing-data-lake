﻿using EventSourcing.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace EventSourcing.EventStore.FileSystem
{
	public sealed class FileSystemEventStore : IEventStore
	{
		private readonly string _tenatId;
		private readonly string _rootPath;
		private readonly Dictionary<Guid, string> _pathCache = new Dictionary<Guid, string>();
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
			// TODO: Add more timings.
			var readFileStopwatch = new Stopwatch();
			var desearializeStopwatch = new Stopwatch();

			var fileContent = string.Empty;

			readFileStopwatch.Start();

			using (var eventsFile = File.Open(GetFilePath(entityId), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var reader = new StreamReader(eventsFile))
			{
				fileContent = reader.ReadToEnd();
			}

			readFileStopwatch.Stop();

			desearializeStopwatch.Start();

			foreach(var line in fileContent.Split('\n'))
			{
				if(line.Length > 0)
				{
					var eventLine = JsonConvert.DeserializeObject<EventLine>(line);
					yield return new Tuple<Type, string>(Type.GetType(eventLine.Type), eventLine.Data);
				}
			}

			desearializeStopwatch.Stop();

			Console.WriteLine($"Reading events duration. Reading file: {readFileStopwatch.ElapsedMilliseconds} ms\tDeserialization: {desearializeStopwatch.ElapsedMilliseconds} ms.");
		}

		private void AppendEventToFile<T>(T @event) where T : Event
		{
			var serializationStopwatch = new Stopwatch();
			var writingStopwatch = new Stopwatch();
			var writerClosing = new Stopwatch();
			var fileClosing = new Stopwatch();


			using (var eventsFile = File.Open(GetFilePath(@event.EntityId), FileMode.Append, FileAccess.Write, FileShare.Read))
			{


				using (var writer = new StreamWriter(eventsFile, System.Text.Encoding.UTF8))
				{
					serializationStopwatch.Start();
					//TODO: Check versioning
					var eventLine = new EventLine
					{
						Timestamp = DateTime.UtcNow.Ticks,
						Type = typeof(T).AssemblyQualifiedName,
						Data = JsonConvert.SerializeObject(@event)
					};

					var lineData = JsonConvert.SerializeObject(eventLine);

					serializationStopwatch.Stop();
					writingStopwatch.Start();
					//var eventData = JsonConvert.SerializeObject(@event);
					//TOOD: Escape AssemblyQualifiedName
					//writer.WriteLine($"{DateTime.UtcNow.Ticks},{EscapeCsv(typeof(T).AssemblyQualifiedName)},{EscapeCsv(eventData)}\n");
					writer.WriteLine(lineData);
					writer.Flush();
					writingStopwatch.Stop();
					writerClosing.Start();
				}
				writerClosing.Stop();
				fileClosing.Start();
			}

			fileClosing.Stop();

			Console.WriteLine($"Writing event duration. Serialization: {serializationStopwatch.ElapsedMilliseconds} ms, {serializationStopwatch.ElapsedTicks} ticks.\tWriting: {writingStopwatch.ElapsedMilliseconds} ms, {writingStopwatch.ElapsedTicks} ticks. Writer closing: {writerClosing.ElapsedMilliseconds} ms. File closing: {fileClosing.ElapsedTicks} ticks.");
		}

		private string GetFilePath(Guid entityId)
		{
			if(_pathCache.ContainsKey(entityId))
			{
				return _pathCache[entityId];
			}

			// Mozda je lakse sve u JSON

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
