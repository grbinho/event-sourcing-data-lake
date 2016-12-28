using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.Classic;
using EventSourcing.Benchmarks;
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Running Benchmarks!");

		//var c = DefaultConfig.Instance.With(
		//		Job.Default.With(ClassicToolchain.Instance));

		//DefaultConfig.Instance.KeepBenchmarkFiles()

		var summary = BenchmarkRunner.Run<EventStoreFileSystem>();

		Console.WriteLine("Done!");
		Console.ReadKey();
	}
}