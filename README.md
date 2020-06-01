# MSEngine ZA Ultra
#### A High Performance Zero Allocation Minesweeper Engine/Solver Built with c# and .NET Standard 2.1

---
[![Build status](https://ci.appveyor.com/api/projects/status/github/bradmarder/MSEngine?branch=master&svg=true)](https://ci.appveyor.com/project/bradmarder/msengine)
[![install from nuget](http://img.shields.io/nuget/v/MSEngine.Core.svg?style=flat-square)](https://www.nuget.org/packages/MSEngine.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

### The core API of MSEngine Ultra
```c#
void FillCustomBoard(Span<Node> nodes, int mineCount, byte columns);
void ComputeBoard(Matrix<Node> matrix, Turn turn);
```

### Who is this library for?
- Anyone who wants to build a Minesweeper game/UI without having to implement all the ugly/confusing internal logic
- Anyone planning on creating a Minesweeper solver bot
- Anyone interested in learning how to implement zero allocation c# code

### How do I use this library?
- See the example `RunSimulations()` inside the console app `src\MSEngine.ConsoleApp\` 
- This example uses the automated solver, but the selection of a turn may come from any UI/input

### Notes
- With just the initial board and a queue of turns, we can compute the expected state of any minesweeper game
- This approach allows for easy debugging, **replays** and **backwards time travel**
- Inspired by the Starcraft 2 replay engine and TAS (tool assisted speedruns)

### API
```c#
public interface IEngine
{
    void FillBeginnerBoard(Span<Node> nodes);
    void FillIntermediateBoard(Span<Node> nodes);
    void FillExpertBoard(Span<Node> nodes);
    void FillCustomBoard(Span<Node> nodes, int mineCount, byte columns);
    void FillCustomBoard(Span<Node> nodes, ReadOnlySpan<int> mines, byte columns);
}
public interface IBoardStateMachine
{
    void EnsureValidBoardConfiguration(Matrix<Node> matrix, Turn turn);
    void ComputeBoard(Matrix<Node> matrix, Turn turn);
}
```

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