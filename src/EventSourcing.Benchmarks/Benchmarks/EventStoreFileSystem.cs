using BenchmarkDotNet.Attributes;

namespace EventSourcing.Benchmarks
{
	/// <summary>
	/// This class holds benchmarks fro Event store using file system as storage
	/// </summary>
	[MemoryDiagnoser]
	public class EventStoreFileSystem
    {
		[Benchmark]
		public void CreateSingleEventOnNewEntity()
		{

		}
    }
}
