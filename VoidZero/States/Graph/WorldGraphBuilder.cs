using VoidZero.States.Stages;

namespace VoidZero.States.Graph
{
    public sealed class WorldGraphBuilder
    {
        // ── Stage pools ───────────────────────────────────────────────────────
        // Each entry is a factory so every call produces a fresh instance.
        // Extend each list to add more content; the builder shuffles and cycles automatically.

        private static readonly List<Func<IStageDefinition>> BattlePool = new()
        {
            () => new Stage2(),
            // () => new Stage4(),
        };

        private static readonly List<Func<IStageDefinition>> EventPool = new()
        {
            () => new Stage2(),
            // () => new EventStage1(),
        };

        private static readonly List<Func<IStageDefinition>> MinibossPool = new()
        {
            () => new Stage1(),
        };

        private static readonly List<Func<IStageDefinition>> BossPool = new()
        {
            () => new Stage3(),
        };

        // ── Public entry point ────────────────────────────────────────────────

        /// <summary>
        /// Generates a complete, randomised world graph.
        /// Call once per run at "New Game" time.
        /// </summary>
        public static WorldGraph Generate(Random rng = null)
        {
            rng ??= new Random();

            var nodes = BuildTopology();
            AssignTypes(nodes, rng);
            AssignStages(nodes, rng);

            nodes.Root.IsAvailable = true;
            return nodes;
        }

        // ── Step 1 : topology ─────────────────────────────────────────────────

        /// <summary>
        /// Creates every node with its allowed-type list, wires all edges, and
        /// returns the finished graph shell (all nodes still <c>Unassigned</c>).
        /// </summary>
        private static WorldGraph BuildTopology()
        {
            // ── Shorthand: B/E/S/M/Boss type sets ────────────────────────────
            static WorldNodeType[] BE() => [WorldNodeType.Battle, WorldNodeType.Event];
            static WorldNodeType[] BES() => [WorldNodeType.Battle, WorldNodeType.Event, WorldNodeType.Shop];

            // ── Rank 0 ────────────────────────────────────────────────────────
            var root = Node([WorldNodeType.Battle], 0, 0);

            // ── Rank 1 (4 nodes, all Battle/Event) ────────────────────────────
            var r1 = Rank(BE(), 1, 4);

            // ── Rank 2 (3 nodes) ──────────────────────────────────────────────
            //   slot 0 (outer top)  : B/E/S
            //   slot 1 (middle)     : B/E
            //   slot 2 (outer bot)  : B/E/S
            var r2 = new WorldNode[]
            {
                Node(BES(), 2, 0),   // outer top
                Node(BE(),  2, 1),   // middle
                Node(BES(), 2, 2),   // outer bot
            };

            // ── Rank 3 (single miniboss) ──────────────────────────────────────
            var miniboss = Node([WorldNodeType.Miniboss], 3, 0);

            // ── Rank 4 (2 nodes, all Battle/Event) ────────────────────────────
            var r4 = Rank(BE(), 4, 2);

            // ── Rank 5 (3 nodes, same outer/mid pattern as rank 2) ────────────
            var r5 = new WorldNode[]
            {
                Node(BES(), 5, 0),   // outer top
                Node(BE(),  5, 1),   // middle
            };

            // ── Rank 6 (three lanes) ──────────────────────────────────────────
            //   top lane  : two B/E nodes straight to BOSS
            //   mid lane  : B/E (hard path 1st node)
            //   bot lane  : two B/E nodes straight to BOSS
            var r6 = Rank(BE(), 6, 5);

            // ── Rank 7 (extra hard path node) ─────────────────────────────────────────────────
            var r7 = Node(BES(), 7, 0);

            // ── Rank 8 (boss) ─────────────────────────────────────────────────
            var boss = Node([WorldNodeType.Boss], 8, 0);

            // ── Wire edges ────────────────────────────────────────────────────

            // Root -> rank 1
            Connect(root, r1);

            // Rank 1 -> rank 2
            Connect(r1[0], r2[0]);
            Connect(r1[1], r2[0], r2[1]);
            Connect(r1[2], r2[1], r2[2]);
            Connect(r1[3], r2[2]);

            // Rank 2 -> miniboss
            Connect(r2[0], miniboss);
            Connect(r2[1], miniboss);
            Connect(r2[2], miniboss);

            // Miniboss -> rank 4
            Connect(miniboss, r4[0], r4[1]);

            // Rank 4 -> rank 5
            Connect(r4[0], r5[0]);
            Connect(r4[1], r5[1]);

            // Rank 5 -> rank 6
            Connect(r5[0], r6[0], r6[1], r6[2]);
            Connect(r5[1], r6[3], r6[4], r6[2]);

            // rank 6 -> end
            Connect(r6[0], boss);
            Connect(r6[1], boss);

            // hard path
            Connect(r6[2], r7);

            Connect(r6[3], boss);
            Connect(r6[4], boss);

            // hard path: rank 7 -> boss
            Connect(r7, boss);

            return new WorldGraph(root);
        }

        // ── Step 2 : type assignment ──────────────────────────────────────────

        /// <summary>
        /// Walks the graph in BFS order and resolves each node's <see cref="WorldNode.Type"/>
        /// from its <see cref="WorldNode.AllowedTypes"/> list.
        ///
        /// Shop constraint: at most one Shop per rank, and only in an outer-perimeter slot.
        /// Fixed nodes (Miniboss, Boss) are resolved without touching the RNG.
        /// </summary>
        private static void AssignTypes(WorldGraph graph, Random rng)
        {
            // Track which ranks have already received a shop this run.
            var shopUsedInRank = new HashSet<int>();

            foreach (var node in graph.AllNodes)
            {
                var allowed = node.AllowedTypes;

                // Fixed single-type nodes resolve immediately.
                if (allowed.Count == 1)
                {
                    node.Type = allowed[0];
                    continue;
                }

                // Build the candidate list, respecting the shop constraint.
                var candidates = allowed
                    .Where(t => t != WorldNodeType.Shop || !shopUsedInRank.Contains(node.Rank))
                    .ToList();

                // Safety: fall back to non-shop types if somehow the list is empty.
                if (candidates.Count == 0)
                    candidates = allowed.Where(t => t != WorldNodeType.Shop).ToList();

                var chosen = candidates[rng.Next(candidates.Count)];
                node.Type = chosen;

                if (chosen == WorldNodeType.Shop)
                    shopUsedInRank.Add(node.Rank);
            }
        }

        // ── Step 3 : stage assignment ─────────────────────────────────────────

        /// <summary>
        /// Assigns concrete <see cref="IStageDefinition"/> instances to every node
        /// that needs one, drawing from the appropriate shuffled pool.
        /// </summary>
        private static void AssignStages(WorldGraph graph, Random rng)
        {
            var battles = new PoolDispenser<IStageDefinition>(BattlePool, rng);
            var events = new PoolDispenser<IStageDefinition>(EventPool, rng);
            var minibosses = new PoolDispenser<IStageDefinition>(MinibossPool, rng);
            var bosses = new PoolDispenser<IStageDefinition>(BossPool, rng);

            foreach (var node in graph.AllNodes)
            {
                node.StageDefinition = node.Type switch
                {
                    WorldNodeType.Battle => battles.Next(),
                    WorldNodeType.Event => events.Next(),
                    WorldNodeType.Shop => null,   // shops have no stage definition
                    WorldNodeType.Miniboss => minibosses.Next(),
                    WorldNodeType.Boss => bosses.Next(),
                    _ => null,
                };
            }
        }

        // ── Node / edge helpers ───────────────────────────────────────────────

        private static WorldNode Node(IEnumerable<WorldNodeType> allowedTypes, int rank, int slot)
            => new(allowedTypes, rank, slot);

        private static List<WorldNode> Rank(WorldNodeType[] allowedTypes, int rank, int count)
        {
            var list = new List<WorldNode>(count);
            for (int i = 0; i < count; i++)
                list.Add(Node(allowedTypes, rank, i));
            return list;
        }

        private static void Connect(WorldNode from, WorldNode to)
            => from.Children.Add(to);

        private static void Connect(WorldNode from, params WorldNode[] to)
        {
            foreach (var n in to) from.Children.Add(n);
        }

        private static void Connect(WorldNode from, List<WorldNode> to)
        {
            foreach (var n in to) from.Children.Add(n);
        }
    }
}