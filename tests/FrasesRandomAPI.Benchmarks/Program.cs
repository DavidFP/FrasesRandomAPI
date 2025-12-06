using BenchmarkDotNet.Running;

namespace FrasesRandomAPI.Benchmarks;

public static class BenchmarkHost
{
    public static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(BenchmarkHost).Assembly).Run(args);
    }
}
