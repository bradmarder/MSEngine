# MSEngine
#### A Minesweeper Engine built using functional programming paradigms with c# and .NET Standard 2.0

---
[![Build status](https://ci.appveyor.com/api/projects/status/github/bradmarder/MSEngine?branch=master&svg=true)](https://ci.appveyor.com/project/bradmarder/msengine)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

### The core concept of MSEngine
```c#
static Board ComputeBoard(Board board, IEnumerable<Turn> turns) => turns.Aggregate(board, ComputeBoard);
static Board ComputeBoard(Board board, Turn turn);
```

### Who is this library for?
- Anyone who wants to build a Minesweeper game/UI without having to implement all the ugly/confusing internal logic
- Anyone planning on creating a Minesweeper solver bot
- Anyone interested in learning how to implement turn-based game logic using functional paradigms

### Notes
- With just the initial board and a queue of turns, we can compute the expected state of any minesweeper game
- This approach allows for **replays** and **backwards time travel**
- Inspired by the Starcraft 2 replay engine
- Also inspired by TAS (tool assisted speedruns)
- Everything immutable
- Enforce referential transparency
- The first turn must select a tile without a mine *and* it must have zero adjacent mines (this logic is the responsibility of the client, not the engine)
- The `System.Collections.Immutable` library has lesser performance relative to it's mutable counterparts

### API (all methods are thread safe)
```c#
static Board GenerateRandomBeginnerBoard();
static Board GenerateRandomIntermediateBoard();
static Board GenerateRandomExpertBoard();
static Board GenerateRandomBoard(byte columns, byte rows, byte mineCount);

[Pure]
static void EnsureValidBoardConfiguration(Board board, Turn turn);

[Pure]
static Board ComputeBoard(Board board, IEnumerable<Turn> turns);

[Pure]
static Board ComputeBoard(Board board, Turn turn);
```

### Tests
To run tests, open a terminal and navigate to `src\MSEngine.Tests\` and execute `dotnet test`

### TODO / Future Goals
- ~~NuGet Package~~
- Larger boards (current max is expert which is 30x16)
- Performance Enhancements (while balancing readability)
- ~~Extensive test suite~~
- Extra Z dimension
- ~~Automated Solvers~~ WIP