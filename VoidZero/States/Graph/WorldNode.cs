using VoidZero.States.Stages;

namespace VoidZero.States.Graph
{
    /// <summary>
    /// A single node in the world-map graph.
    /// </summary>
    public class WorldNode
    {
        // ── Identity ─────────────────────────────────────────────────────────

        /// <summary>Horizontal rank (column) this node occupies.</summary>
        public int Rank { get; }

        /// <summary>Vertical slot index within the rank (0 = top).</summary>
        public int Slot { get; }

        // ── Type ─────────────────────────────────────────────────────────────

        /// <summary>
        /// The set of node types this slot is allowed to become.
        /// Populated at graph-construction time; the final <see cref="Type"/> is
        /// assigned in a separate pass after the topology is fixed.
        /// </summary>
        public IReadOnlyList<WorldNodeType> AllowedTypes { get; }

        /// <summary>The resolved type for this run. Set by <see cref="WorldGraphBuilder"/>.</summary>
        public WorldNodeType Type { get; internal set; }

        // ── Stage ─────────────────────────────────────────────────────────────

        /// <summary>Stage definition assigned after type resolution.</summary>
        public IStageDefinition StageDefinition { get; internal set; }

        // ── Graph ─────────────────────────────────────────────────────────────

        public List<WorldNode> Children { get; } = new();

        // ── Run state ─────────────────────────────────────────────────────────

        /// <summary>
        /// True when the player can click this node on the map screen.
        /// Set to true on the root at generation time, then propagated
        /// to children when their parent's stage is completed.
        /// </summary>
        public bool IsAvailable { get; set; }

        /// <summary>
        /// True once the player has cleared this node's stage.
        /// Used by MapState to visually distinguish visited nodes
        /// and to know which edges to highlight.
        /// </summary>
        public bool IsCompleted { get; set; }

        // ── Ctor ──────────────────────────────────────────────────────────────

        public WorldNode(IEnumerable<WorldNodeType> allowedTypes, int rank, int slot)
        {
            AllowedTypes = allowedTypes.ToList();
            Rank = rank;
            Slot = slot;
            Type = WorldNodeType.Unassigned;
        }

        /// <summary>
        /// Marks this node complete and makes all its children available.
        /// Call this when the stage attached to this node is cleared.
        /// </summary>
        public void Complete()
        {
            IsCompleted = true;
            IsAvailable = false;

            foreach (var child in Children)
                child.IsAvailable = true;
        }

        public override string ToString() =>
            $"[R{Rank}S{Slot} {Type} ({string.Join('/', AllowedTypes)})]";
    }
}