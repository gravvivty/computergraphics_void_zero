namespace VoidZero.States.Graph
{
    /// <summary>
    /// Pure data container for the world-map graph.
    /// Construction and randomisation live in <see cref="WorldGraphBuilder"/>.
    /// </summary>
    public sealed class WorldGraph
    {
        /// <summary>Entry-point node (rank 0).</summary>
        public WorldNode Root { get; }

        /// <summary>Every node in the graph, ordered breadth-first from Root.</summary>
        public IReadOnlyList<WorldNode> AllNodes { get; }

        internal WorldGraph(WorldNode root)
        {
            Root = root;
            AllNodes = BuildBfsOrder(root);
        }

        private static IReadOnlyList<WorldNode> BuildBfsOrder(WorldNode root)
        {
            var visited = new HashSet<WorldNode>();
            var queue = new Queue<WorldNode>();
            var result = new List<WorldNode>();

            queue.Enqueue(root);
            visited.Add(root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                result.Add(node);
                foreach (var child in node.Children)
                    if (visited.Add(child))
                        queue.Enqueue(child);
            }

            return result;
        }
    }
}