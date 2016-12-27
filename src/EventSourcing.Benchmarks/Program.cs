using BenchmarkDotNet.Running;
using EventSourcing.Benchmarks;
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Running Benchmarks!");

		var summary = BenchmarkRunner.Run<EventStoreFileSystem>();

		Console.WriteLine("Done!");
		Console.ReadKey();
	}
}