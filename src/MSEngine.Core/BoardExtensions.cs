using System;

namespace MSEngine.Core
{
    public static class BoardExtensions
    {
        public static BoardStatus Status(this Span<Node> nodes) => Status((ReadOnlySpan<Node>)nodes);
        public static int FlagsAvailable(this Span<Node> nodes) => FlagsAvailable((ReadOnlySpan<Node>)nodes);
        public static int MineCount(this Span<Node> nodes) => MineCount((ReadOnlySpan<Node>)nodes);
        public static int FlaggedNodesCount(this Span<Node> nodes) => FlaggedNodesCount((ReadOnlySpan<Node>)nodes);
        public static bool AllMinesFlagged(this Span<Node> nodes) => AllMinesFlagged((ReadOnlySpan<Node>)nodes);

        public static BoardStatus Status(this ReadOnlySpan<Node> nodes)
        {
            var complete = true;
            foreach (var node in nodes)
            {
                if (node.HasMineExploded)
                {
                    return BoardStatus.Failed;
                }
                if (!node.SatisfiesWinningCriteria)
                {
                    complete = false;
                }
            }
            return complete ? BoardStatus.Completed : BoardStatus.Pending;
        }
        public static int FlagsAvailable(this ReadOnlySpan<Node> nodes) => nodes.MineCount() - nodes.FlaggedNodesCount();
        public static int MineCount(this ReadOnlySpan<Node> nodes)
        {
            var n = 0;
            foreach (var node in nodes)
            {
                if (node.HasMine)
                {
                    n++;
                }
            }
            return n;
        }
        public static int FlaggedNodesCount(this ReadOnlySpan<Node> nodes)
        {
            var n = 0;
            foreach (var node in nodes)
            {
                if (node.State == NodeState.Flagged)
                {
                    n++;
                }
            }
            return n;
        }
        public static bool AllMinesFlagged(this ReadOnlySpan<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.HasMine && node.State != NodeState.Flagged)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
