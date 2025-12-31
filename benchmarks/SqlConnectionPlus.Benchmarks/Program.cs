using BenchmarkDotNet.Running;

namespace RentADeveloper.SqlConnectionPlus.Benchmarks;

public class Program
{
    public static void Main(String[] args) =>
        BenchmarkSwitcher
            .FromAssembly(typeof(Program).Assembly)
            .Run(args);
}
