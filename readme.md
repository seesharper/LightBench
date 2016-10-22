## LightBench

A super simple benchmark tool for .Net. 

```csharp 
var result = Benchmark.Run(() => codeToBenchmark, 100, "Sample Benchmark");
Console.WriteLine(result);
```

Gives us the following result 

```console
****** Sample Benchmark ******
Number of runs: 100
Total time: 26,16 ms
Standard Deviation: 0,27 ms
Mean: 0,26 ms
Longest: 2,24 ms
Shortest: 0,19 ms
------ Memory ------
Memory (start): 1501400 bytes
Memory (end): 3197144 bytes
Memory (allocated) 1695744 bytes
Memory (after collect): 1501248 bytes

Percentage of the requests served within a certain time (ms)
50%		0,20
66%		0,20
75%		0,21
80%		0,22
90%		0,30
95%		0,43
98%		0,83
99%		1,86
100%	2,24
```