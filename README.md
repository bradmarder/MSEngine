# MSEngine
#### A Minesweeper Engine built using functional programming paradigms with c# and .NET Standard 2.1

---
[![Build status](https://ci.appveyor.com/api/projects/status/github/bradmarder/MSEngine?branch=master&svg=true)](https://ci.appveyor.com/project/bradmarder/msengine)
[![install from nuget](http://img.shields.io/nuget/v/MSEngine.Core.svg?style=flat-square)](https://www.nuget.org/packages/MSEngine.Core)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

### The core concept of MSEngine
```c#
Board ComputeBoard(Board board, Turn turn);
Board ComputeBoard(Board board, IEnumerable<Turn> turns) => turns.Aggregate(board, ComputeBoard);
```

### Who is this library for?
- Anyone who wants to build a Minesweeper game/UI without having to implement all the ugly/confusing internal logic
- Anyone planning on creating a Minesweeper solver bot
- Anyone interested in learning how to implement turn-based game logic using functional paradigms

### How do I use this library?
- See the example `RunSimulations()` inside the console app `src\MSEngine.ConsoleApp\` 
- This example uses the automated solver, but the selection of a turn may come from any UI/input

### Notes
- With just the initial board and a queue of turns, we can compute the expected state of any minesweeper game
- This approach allows for easy debugging, **replays** and **backwards time travel**
- Inspired by the Starcraft 2 replay engine and TAS (tool assisted speedruns)
- Everything immutable, enforce referential transparency
- The first turn must select a tile without a mine *and* having zero adjacent mines (this logic is the responsibility of the client, not the engine)
- The `System.Collections.Immutable` library has lesser performance relative to it's mutable counterparts

### API (Instances are thread-safe)
```c#
public interface IEngine
{
    Board GenerateBeginnerBoard();
    Board GenerateIntermediateBoard();
    Board GenerateExpertBoard();
    Board GenerateCustomBoard(byte columns, byte rows, byte mineCount);
}
public interface IBoardStateMachine
{
    void EnsureValidBoardConfiguration(Board board, Turn turn);
    Board ComputeBoard(Board board, IEnumerable<Turn> turns);
    Board ComputeBoard(Board board, Turn turn);
}
```

### Tests
To run tests, open a terminal and navigate to `src\MSEngine.Tests\` and execute `dotnet test`

### Benchmarks
To run benchmarks, open a terminal and navigate to `src\MSEngine.Benchmarks\` and execute `dotnet run`

|                          Method |       Mean |     Error |    StdDev |     Median | Gen 0/1k Op | Gen 1/1k Op | Gen 2/1k Op | Allocated Memory/Op |
|-------------------------------- |-----------:|----------:|----------:|-----------:|------------:|------------:|------------:|--------------------:|
|           GenerateBeginnerBoard |   110.9 us |  2.201 us |  5.480 us |   108.2 us |     13.0615 |           - |           - |            53.81 KB |
|       GenerateIntermediateBoard | 1,441.4 us |  6.867 us |  6.424 us | 1,439.3 us |    132.8125 |           - |           - |           548.06 KB |
|             GenerateExpertBoard | 5,112.5 us | 33.829 us | 31.644 us | 5,104.7 us |    531.2500 |           - |           - |          2188.05 KB |

### TODO / Future Goals
- ~~NuGet Package~~
- ~~Performance Enhancements (while balancing readability)~~
- ~~Extensive and deterministic test suite~~
- ~~Benchmarks~~
- ~~Automated Solver~~ 
    - ~85% win ratio on beginner, ~80% on intermediate, ~29% on expert
	- need to verify logic on `PatternStrategy` and implement `EducatedGuessStrategy`
- Extra Z dimension
- Matrix based board/solver