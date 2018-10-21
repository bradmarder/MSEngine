# MSEngine
### A Minesweeper Engine built using functional programming paradigms with c# and .NET Standard 2.0

### The Core of MSEngine
```csharp
public readonly struct GameState
{
    public Board Board { get; }
    public IImmutableQueue<Turn> Turns { get; }
}
```

- With just the initial board and a queue of turns, we can calculate the expected state of any minesweeper game
- This approach allows for **replays** and **backwards time travel**
- Inspired by the Starcraft 2 replay engine

### TODO / Future Goals
- NuGet Package
- Larger boards (current max is expert which is 30x16)
- Performance Enhancements
- More tests
- Extra Z dimension
- Automated Solvers