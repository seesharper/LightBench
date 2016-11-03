## LightBench

A super simple benchmark tool for .Net. 

```csharp 
private static void SuperSimpleBenchmark()
{
    var report = Benchmark.Run(() => CodeToBenchMark(), 100, "Simple benchmark");
    Console.WriteLine(report.ToString());
}

private static void CodeToBenchMark()
{
    var random = new Random();
    Thread.Sleep(random.Next(1, 100));   
}
```

Gives us the following result 

```console
*********************
Simple benchmark
*********************
Started: 03.11.2016 17.27.11
Status: Completed (03.11.2016 17.27.15)
Number of runs: 100
Total time: 3661,00 ms
------ Memory ------
Memory (start):         98300 bytes
Memory (end):           131068 bytes
Memory (after collect): 99364 bytes
Memory (allocated)      32768 bytes

------ Frequency Distribution Table------
Mean: 36,61 ms +/- 32,25 ms
Min: 1,00 ms
(40)    **************************************************
(4)     *****
(0)
(7)     ********
(15)    ******************
(13)    ****************
(6)     *******
(0)
(5)     ******
(10)    ************
Max: 100,00 ms

Percentage of the requests served within a certain time (ms)
50%             40,00
66%             50,68
75%             58,00
80%             61,20
90%             90,10
95%             95,05
98%             98,04
99%             100,00
100%            100,00

Benchmark Completed!
```

## Benchmarking over time

To benchmark a piece of code over time, we can set this up with the **Monitor** method.

In this example we will execute the code for 20 seconds with a 500 ms "think time" between each execution.
```csharp
private static void MonitorSuperSimpleBenchmark()
{
    Benchmark.Monitor(
        () => CodeToBenchMark(), 
        TimeSpan.FromSeconds(20), 
        () => TimeSpan.FromMilliseconds(500), 
        () => "Monitor Simple benchmark",
        report =>
        {
            Console.Clear();
            Console.WriteLine(report.ToString());                    
        });
}
```

Notice that we specify both the "think time" and 
the description usng a function delegate. 
The reason for this is that it allows us to specify different values for each run.
Say for example that the benchmark to operate with a set of different "think times".
LightBench provides a simple extension method that allows to "randomly" pick an item from a list.

```csharp
private static void MonitorWithRandomThinkTimes()
{
    int[] thinkTimes = new[] {200, 300, 500, 600};
    Benchmark.Monitor(
        () => CodeToBenchMark(),
        TimeSpan.FromSeconds(20),
        () => TimeSpan.FromMilliseconds(thinkTimes.PickRandomItem()),
        () => "Monitor Simple benchmark",
        report =>
        {
            Console.Clear();
            Console.WriteLine(report.ToString());
        });
}
```



