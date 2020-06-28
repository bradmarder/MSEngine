# MSEngine ZA Ultra
#### A High Performance Zero Allocation Minesweeper Engine/Solver Built with c# and .NET Standard 2.1

---
[![Build status](https://ci.appveyor.com/api/projects/status/github/bradmarder/MSEngine?branch=master&svg=true)](https://ci.appveyor.com/project/bradmarder/msengine)
[![install from nuget](https://img.shields.io/nuget/v/MSEngine.Core.svg?style=flat-square)](https://www.nuget.org/packages/MSEngine.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## [Link to Series of Blog Posts](https://bradmarder.github.io/blog/)

### How Fast?
##### Measurement represents time to generate a random board and solve until guessing is required
``` ini

BenchmarkDotNet=v0.12.0, OS=Windows 10.0.18362
Intel Core i7-4770K CPU 3.50GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=3.1.301
  [Host]     : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT
  DefaultJob : .NET Core 3.1.5 (CoreCLR 4.700.20.26901, CoreFX 4.700.20.27001), X64 RyuJIT


```
|       Method |        Mean |     Error |    StdDev | Gen 0 | Gen 1 | Gen 2 | Allocated |
|------------- |------------:|----------:|----------:|------:|------:|------:|----------:|
|     Beginner |    53.93 us |  0.301 us |  0.281 us |     - |     - |     - |         - |
| Intermediate |   343.97 us |  2.637 us |  2.467 us |     - |     - |     - |         - |
|       Expert | 1,072.81 us | 21.002 us | 24.186 us |     - |     - |     - |         - |

### The core API of MSEngine ZA Ultra
```c#
public static class Engine
{
    static void FillBeginnerBoard(Span<Node> nodes);
    static void FillIntermediateBoard(Span<Node> nodes);
    static void FillExpertBoard(Span<Node> nodes);
    static void FillCustomBoard(Span<Node> nodes, int mineCount, byte columns);
    static void FillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns);
    static void EnsureValidBoardConfiguration(Matrix<Node> matrix, Turn turn);
    static void ComputeBoard(Matrix<Node> matrix, Turn turn);
}
```

### Who is this library for?
- Anyone who wants to build a Minesweeper game/UI without having to implement all the ugly/confusing internal logic
- Anyone creating a Minesweeper probability solver bot
- Anyone interested in learning how to implement zero allocation c# code

### How do I use this library?
- See the example `RunSimulations()` inside the console app `src\MSEngine.ConsoleApp\` 
- This example uses the automated solver, but the selection of a turn may come from any UI/input

### Notes
- With just the initial board and a queue of turns, we can compute the expected state of any minesweeper game
- This approach allows for easy debugging, **replays** and **backwards time travel**
- Inspired by the Starcraft 2 replay engine and TAS (tool assisted speedruns)

### Tests
To run tests, open a terminal and navigate to `src\MSEngine.Tests\` and execute `dotnet test`

### Benchmarks
To run benchmarks, open a terminal and navigate to `src\MSEngine.Benchmarks\` and execute `dotnet run`

### TODO / Future Goals
- ~~NuGet Package~~
- ~~Zero Allocations~~
- ~~Performance Enhancements~~
- ~~Extensive and deterministic test suite~~
- ~~Benchmarks~~
- ~~Matrix / Linear Algebra solver~~ *(Special thanks to [Robert Massaioli](https://massaioli.wordpress.com/2013/01/12/solving-minesweeper-with-matricies/) for providing a detailed implementation)*
- Probabilistic Solver
- Implement SIMD / Instrinsics
- Series of Blog Posts