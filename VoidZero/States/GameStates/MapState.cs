using ImGuiNET;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;
using System.Numerics;
using VoidZero.Core;
using VoidZero.Game;
using VoidZero.Game.Input;
using VoidZero.Graphics;
using VoidZero.States.GameStates;
using VoidZero.States.Graph;
using Vector2 = System.Numerics.Vector2;

namespace VoidZero.States
{
    /// <summary>
    /// Shown between stages. Renders the full world graph as an interactive overlay,
    /// lets the player pick their next node with the mouse, then transitions into a
    /// new <see cref="PlayState"/> for the chosen stage.
    ///
    /// Flow:
    ///   PlayState (boss killed) → MapState.Enter()
    ///   Player clicks an available node → MapState exits → new PlayState
    ///
    /// Coordinate system:
    ///   Graph nodes are laid out in a virtual canvas sized to the viewport.
    ///   Ranks go left→right, slots go top→bottom.
    ///   All drawing is done through ImGui's draw list so it sits above the
    ///   background but below the HUD font layer.
    /// </summary>
    public class MapState : GameState
    {
        // ── Dependencies ──────────────────────────────────────────────────────
        private readonly GameStateManager _gsm;
        private readonly GameWindow _window;
        private readonly InputManager _input;
        private readonly Background _background;
        private readonly GameManager _gm;
        private readonly WorldGraph _graph;

        // ── Base design resolution (all constants were authored at this size) ─
        private const float BaseW = 1600f;
        private const float BaseH = 900f;

        // ── Layout constants (at base resolution) ─────────────────────────────
        private const float BaseNodeRadius = 28f;
        private const float BaseHoveredExtraRadius = 6f;
        private const float BaseHRankSpacing = 160f;
        private const float BaseVSlotSpacing = 90f;
        private const float BaseEdgeThickness = 2f;
        private const float BaseAvailableEdgeThickness = 3f;
        private const float BaseTitleY = 24f;
        private const float BaseTooltipWidth = 160f;
        private const float BaseTooltipOffsetX = 16f;
        private const float BaseTooltipOffsetY = 8f;
        private const float BaseRingOffset = 3f;
        private const float BaseHoverBorderThickness = 2.5f;
        private const float BaseNormalBorderThickness = 1.5f;

        // ── Pixel grid size (at base resolution) ──────────────────────────────
        // All positions and shapes snap to this grid for the chunky pixel-art look.
        private const float BasePixelSize = 3f;

        // ── Colours ───────────────────────────────────────────────────────────
        private static readonly uint ColBackground = Rgba(15, 15, 25, 210);
        private static readonly uint ColEdge = Rgba(80, 80, 100, 180);
        private static readonly uint ColEdgeAvail = Rgba(255, 255, 255, 220);
        private static readonly uint ColNodeDefault = Rgba(50, 50, 70, 255);
        private static readonly uint ColNodeAvail = Rgba(40, 120, 80, 255);
        private static readonly uint ColNodeHoverAvail = Rgba(60, 200, 110, 255);
        private static readonly uint ColNodeDone = Rgba(60, 40, 120, 255);
        private static readonly uint ColNodeCurrent = Rgba(180, 140, 30, 255);
        private static readonly uint ColNodeLocked = Rgba(30, 30, 45, 255);
        private static readonly uint ColRing = Rgba(200, 200, 255, 255);
        private static readonly uint ColRingAvail = Rgba(100, 255, 160, 255);
        private static readonly uint ColRingDone = Rgba(140, 100, 255, 220);
        private static readonly uint ColRingLocked = Rgba(70, 70, 90, 255);
        private static readonly uint ColText = Rgba(230, 230, 255, 255);
        private static readonly uint ColTextDim = Rgba(140, 140, 160, 255);
        private static readonly uint ColTitle = Rgba(200, 200, 255, 255);

        // Type accent colours
        private static readonly Dictionary<WorldNodeType, uint> TypeColour = new()
        {
            [WorldNodeType.Battle] = Rgba(220, 80, 60, 255),
            [WorldNodeType.Event] = Rgba(80, 160, 220, 255),
            [WorldNodeType.Shop] = Rgba(220, 190, 60, 255),
            [WorldNodeType.Miniboss] = Rgba(200, 100, 220, 255),
            [WorldNodeType.Boss] = Rgba(220, 50, 50, 255),
            [WorldNodeType.Unassigned] = Rgba(100, 100, 100, 255),
        };

        private static readonly Dictionary<WorldNodeType, string> TypeLabel = new()
        {
            [WorldNodeType.Battle] = "BATTLE",
            [WorldNodeType.Event] = "EVENT",
            [WorldNodeType.Shop] = "SHOP",
            [WorldNodeType.Miniboss] = "MINI",
            [WorldNodeType.Boss] = "BOSS",
            [WorldNodeType.Unassigned] = "???",
        };

        // ── Runtime state ─────────────────────────────────────────────────────
        private Dictionary<WorldNode, Vector2> _nodePositions = new();
        private WorldNode _hoveredNode;
        private WorldNode _currentNode;
        private float _enterTimer = 0f;
        private const float FadeInDuration = 0.4f;
        private Vector2 _cursorPos;               // screen-space cursor position
        private WorldNode _selectedCursorNode;    // node cursor is currently on
        private bool _usingController = false;    // hides cursor when using mouse
        private float _stickCooldown = 0f;
        private const float StickCooldownDuration = 0.3f;  // seconds between stick moves
        private float _cursorTimer = 0f;

        // ── Constructor ───────────────────────────────────────────────────────

        public MapState(
            GameStateManager gsm,
            GameWindow window,
            InputManager input,
            Background background,
            GameManager gm,
            WorldGraph graph,
            WorldNode justCompleted = null)
        {
            _gsm = gsm;
            _window = window;
            _input = input;
            _background = background;
            _gm = gm;
            _graph = graph;
            _currentNode = justCompleted;
        }

        // ── GameState lifecycle ───────────────────────────────────────────────

        public override void Enter()
        {
            _enterTimer = 0f;
            _input.Reset();
            ComputeLayout();
            // Snap cursor to first available node
            var first = _graph.AllNodes.FirstOrDefault(n => n.IsAvailable);
            if (first != null)
            {
                _selectedCursorNode = first;
                _cursorPos = _nodePositions[first];
            }
        }

        public override void Exit()
        {
            _hoveredNode = null;
        }

        public override void Update(float dt)
        {
            _enterTimer += dt;
            _cursorTimer += dt;
            GameServices.Instance.ParticleSystem.Update(dt);

            _stickCooldown -= dt;

            Vector2 stick = (Vector2)_input.GetLeftStick();
            bool stickMoved = stick.Length() > 0.4f && _stickCooldown <= 0f;

            if (stickMoved && _selectedCursorNode != null)
            {
                _usingController = true;
                _stickCooldown = StickCooldownDuration;

                // Find the available node whose position is closest in the stick direction
                WorldNode best = null;
                float bestScore = float.MaxValue;

                var (_, _, vpW, vpH) = _gm.GetViewportRect();
                Vector2 currentPos = _nodePositions[_selectedCursorNode];

                foreach (var node in _graph.AllNodes)
                {
                    if (node == _selectedCursorNode) continue;
                    if (!node.IsAvailable && !node.IsCompleted) continue;
                    if (!_nodePositions.TryGetValue(node, out var pos)) continue;

                    Vector2 dir = pos - currentPos;
                    float dist = dir.Length();
                    if (dist < 1f) continue;

                    // Dot product tells us how well this node aligns with stick direction
                    Vector2 dirNorm = dir / dist;
                    float dot = dirNorm.X * stick.X + dirNorm.Y * stick.Y;

                    // Only consider nodes roughly in the stick direction
                    if (dot < 0.3f) continue;

                    // Score: prefer closer nodes that align well with direction
                    float score = dist / (dot * dot);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        best = node;
                    }
                }

                if (best != null)
                    _selectedCursorNode = best;

                _cursorPos = _nodePositions[_selectedCursorNode];
            }

            // Mouse movement switches back to mouse mode
            if (_input.GamepadConnected == false || ImGui.GetIO().MouseDelta.Length() > 2f)
                _usingController = false;

            // Confirm selection
            if (_usingController && _selectedCursorNode != null
                && _selectedCursorNode.IsAvailable
                && _input.ConsumeConfirmPressed())
            {
                SelectNode(_selectedCursorNode);
            }

            // Cancel / back (optional)
            // if (_input.ConsumeCancelPressed()) { go back to menu }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _background.Draw(spriteBatch);
        }

        public override void DrawUI(SpriteBatch spriteBatch, float dt)
        {
            var io = ImGui.GetIO();
            var (vpX, vpY, vpW, vpH) = _gm.GetViewportRect();

            // ── Scale factor relative to base design resolution ───────────────
            float scale = MathF.Min(vpW / BaseW, vpH / BaseH);

            // ── Pixel grid size scales with viewport ──────────────────────────
            float px = MathF.Max(1f, MathF.Round(BasePixelSize * scale));

            // ── Scaled layout values ──────────────────────────────────────────
            float nodeRadius = BaseNodeRadius * scale;
            float hoveredRadius = (BaseNodeRadius + BaseHoveredExtraRadius) * scale;
            float edgeThickness = BaseEdgeThickness * scale;
            float availEdgeThickness = BaseAvailableEdgeThickness * scale;
            float ringOffset = BaseRingOffset * scale;
            float hoverBorderThickness = BaseHoverBorderThickness * scale;
            float normalBorderThickness = BaseNormalBorderThickness * scale;
            float titleY = BaseTitleY * scale;
            float tooltipWidth = BaseTooltipWidth * scale;
            float tooltipOffsetX = BaseTooltipOffsetX * scale;
            float tooltipOffsetY = BaseTooltipOffsetY * scale;

            // ── Full-screen transparent ImGui window ──────────────────────────
            ImGui.SetNextWindowPos(new Vector2(vpX, vpY));
            ImGui.SetNextWindowSize(new Vector2(vpW, vpH));
            ImGui.Begin("##MapOverlay",
                ImGuiWindowFlags.NoDecoration |
                ImGuiWindowFlags.NoMove |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse);

            var drawList = ImGui.GetWindowDrawList();
            float alpha = Math.Clamp(_enterTimer / FadeInDuration, 0f, 1f);
            alpha = alpha * alpha * (3f - 2f * alpha); // smooth-step

            // ── Dim overlay ───────────────────────────────────────────────────
            drawList.AddRectFilled(
                new Vector2(vpX, vpY),
                new Vector2(vpX + vpW, vpY + vpH),
                FadeAlpha(ColBackground, alpha));

            // ── Title ─────────────────────────────────────────────────────────
            string title = "SELECT YOUR NEXT NODE";
            float titleW = ImGui.CalcTextSize(title).X;
            drawList.AddText(
                new Vector2(vpX + (vpW - titleW) * 0.5f, vpY + titleY),
                FadeAlpha(ColTitle, alpha),
                title);

            // ── Mouse position ────────────────────────────────────────────────
            Vector2 mousePos = ImGui.GetMousePos();
            _hoveredNode = null;

            // ── Edges (pixelated) ─────────────────────────────────────────────
            foreach (var node in _graph.AllNodes)
            {
                if (!_nodePositions.TryGetValue(node, out var fromPos)) continue;

                foreach (var child in node.Children)
                {
                    if (!_nodePositions.TryGetValue(child, out var toPos)) continue;

                    bool isAvailablePath = node.IsCompleted && child.IsAvailable;
                    uint edgeCol = isAvailablePath ? ColEdgeAvail : ColEdge;
                    float thickness = isAvailablePath ? availEdgeThickness : edgeThickness;
                    Vector2 from = Snap(fromPos + new Vector2(vpX, vpY), px);
                    Vector2 to = Snap(toPos + new Vector2(vpX, vpY), px);

                    DrawPixelLine(drawList, from, to, FadeAlpha(edgeCol, alpha), px);
                }
            }

            // ── Nodes (pixelated diamonds) ────────────────────────────────────
            foreach (var node in _graph.AllNodes)
            {
                if (!_nodePositions.TryGetValue(node, out var localPos)) continue;

                // Snap node center to pixel grid
                Vector2 screenPos = Snap(localPos + new Vector2(vpX, vpY), px);

                bool isAvail = node.IsAvailable;
                bool isDone = node.IsCompleted;
                bool isCurrent = node == _currentNode;
                bool isLocked = !isAvail && !isDone && !isCurrent;
                bool isHovered = isAvail && Vector2.Distance(mousePos, screenPos) <= hoveredRadius;

                if (isHovered) _hoveredNode = node;

                float radius = isHovered ? hoveredRadius : nodeRadius;

                // Fill
                uint fillCol = (isDone, isCurrent, isAvail, isHovered) switch
                {
                    (true, true, _, _) => ColNodeCurrent,
                    (true, _, _, _) => ColNodeDone,
                    (_, _, true, true) => ColNodeHoverAvail,
                    (_, _, true, _) => ColNodeAvail,
                    _ => isLocked ? ColNodeLocked : ColNodeDefault,
                };

                DrawPixelDiamond(drawList, screenPos, radius, FadeAlpha(fillCol, alpha), px);

                // Ring — slightly larger, outline only
                uint ringCol = isLocked ? ColRingLocked :
                               isDone ? ColRingDone :
                               isAvail ? ColRingAvail : ColRing;

                if (TypeColour.TryGetValue(node.Type, out var typeAccent) && isAvail)
                    ringCol = typeAccent;

                DrawPixelDiamondOutline(drawList, screenPos, radius + ringOffset, FadeAlpha(ringCol, alpha), px);

                // Label inside node
                string typeStr = TypeLabel.GetValueOrDefault(node.Type, "?");
                float fontSize = ImGui.GetFontSize() * 0.8f;
                float labelW = ImGui.CalcTextSize(typeStr).X * 0.8f;
                uint textCol = isLocked ? FadeAlpha(ColTextDim, 0.3f) :
                                  (isDone && !isCurrent) ? ColTextDim : ColText;

                drawList.AddText(
                    ImGui.GetFont(),
                    fontSize,
                    screenPos - new Vector2(labelW * 0.5f, fontSize * 0.5f),
                    FadeAlpha(textCol, alpha),
                    typeStr);
            }

            // ── Controller cursor ─────────────────────────────────────────────────────
            if (_usingController && _selectedCursorNode != null)
            {
                if (_nodePositions.TryGetValue(_selectedCursorNode, out var cursorLocal))
                {
                    Vector2 cursorScreen = Snap(cursorLocal + new Vector2(vpX, vpY), px);
                    float pulseRadius = (hoveredRadius + ringOffset + 4f * scale)
                                           + MathF.Sin(_enterTimer * 6f) * 2f * scale;

                    DrawPixelDiamondOutline(
                        drawList,
                        cursorScreen,
                        pulseRadius,
                        FadeAlpha(Rgba(255, 255, 255, 200), alpha),
                        px);
                }
            }

            // ── Tooltip on hover ──────────────────────────────────────────────
            if (_hoveredNode != null)
            {
                ImGui.SetNextWindowPos(mousePos + new Vector2(tooltipOffsetX, tooltipOffsetY));
                ImGui.SetNextWindowSize(new Vector2(tooltipWidth, 0));
                ImGui.Begin("##NodeTooltip",
                    ImGuiWindowFlags.NoDecoration |
                    ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoResize |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.AlwaysAutoResize);

                ImGui.TextColored(ToVec4(TypeColour[_hoveredNode.Type]),
                    TypeLabel[_hoveredNode.Type]);
                ImGui.Separator();
                ImGui.Text("Travel here?");
                ImGui.End();
            }

            // ── Click handling ────────────────────────────────────────────────
            if (_hoveredNode != null && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                SelectNode(_hoveredNode);
            }

            // ── Custom pixel cursor diamond ───────────────────────────────────────────
            ImGui.SetMouseCursor(ImGuiMouseCursor.None); // hide OS cursor

            float pulse = MathF.Sin(_cursorTimer * 6f);          // -1..1
            float outerRadius = (8f + pulse * 2f) * scale;             // pulsing outer ring
            float innerRadius = 4f * scale;                             // solid inner dot
            float crossLen = (12f + pulse * 1.5f) * scale;          // crosshair arms

            uint colCursorFill = Rgba(255, 255, 255, 220);
            uint colCursorRing = _hoveredNode != null
                ? FadeAlpha(TypeColour[_hoveredNode.Type], alpha)       // type colour when hovering
                : Rgba(180, 180, 255, 200);
            uint colCursorCross = Rgba(255, 255, 255, 80);

            Vector2 cursor = Snap(mousePos, px);

            // Crosshair lines
            DrawPixelLine(drawList, cursor - new Vector2(crossLen, 0), cursor - new Vector2(outerRadius + 2f * scale, 0), FadeAlpha(colCursorCross, alpha), px);
            DrawPixelLine(drawList, cursor + new Vector2(outerRadius + 2f * scale, 0), cursor + new Vector2(crossLen, 0), FadeAlpha(colCursorCross, alpha), px);
            DrawPixelLine(drawList, cursor - new Vector2(0, crossLen), cursor - new Vector2(0, outerRadius + 2f * scale), FadeAlpha(colCursorCross, alpha), px);
            DrawPixelLine(drawList, cursor + new Vector2(0, outerRadius + 2f * scale), cursor + new Vector2(0, crossLen), FadeAlpha(colCursorCross, alpha), px);

            // Outer pulsing ring
            DrawPixelDiamondOutline(drawList, cursor, outerRadius, FadeAlpha(colCursorRing, alpha), px);

            // Inner solid dot
            DrawPixelDiamond(drawList, cursor, innerRadius, FadeAlpha(colCursorFill, alpha), px);

            ImGui.End();
        }

        // ── Layout computation ────────────────────────────────────────────────

        private void ComputeLayout()
        {
            _nodePositions.Clear();

            var (_, _, vpW, vpH) = _gm.GetViewportRect();

            float scale = MathF.Min(vpW / BaseW, vpH / BaseH);
            float px = MathF.Max(1f, MathF.Round(BasePixelSize * scale));
            float hRankSpacing = BaseHRankSpacing * scale;
            float vSlotSpacing = BaseVSlotSpacing * scale;

            var byRank = _graph.AllNodes
                .GroupBy(n => n.Rank)
                .OrderBy(g => g.Key)
                .ToList();

            int maxRank = byRank.Max(g => g.Key);
            float totalW = maxRank * hRankSpacing;
            float startX = (vpW - totalW) * 0.5f;
            float midY = vpH * 0.52f;

            foreach (var rankGroup in byRank)
            {
                int rank = rankGroup.Key;
                var nodes = rankGroup.OrderBy(n => n.Slot).ToList();
                float x = startX + rank * hRankSpacing;

                int maxSlot = nodes.Max(n => n.Slot);
                float totalSlotHeight = maxSlot * vSlotSpacing;
                float colStartY = midY - totalSlotHeight * 0.5f;

                foreach (var node in nodes)
                {
                    float y = colStartY + node.Slot * vSlotSpacing;
                    // Snap layout positions to pixel grid at compute time
                    _nodePositions[node] = Snap(new Vector2(x, y), px);
                }
            }
        }

        // ── Selection ─────────────────────────────────────────────────────────

        private void SelectNode(WorldNode selected)
        {
            foreach (var node in _graph.AllNodes)
            {
                if (node != selected && node.IsAvailable && node.Rank == selected.Rank)
                    node.IsAvailable = false;
            }

            selected.IsAvailable = false;
            selected.IsCompleted = false;

            _currentNode = selected;
            _gm.EnterPlay();
            _input.Reset();

            _gsm.ChangeState(new PlayState(
                _gsm, _window, _input, _background, _gm,
                worldGraph: _graph,
                selectedNode: selected));
        }

        // ── Pixel drawing helpers ─────────────────────────────────────────────

        /// <summary>Snaps a position to the nearest pixel grid point.</summary>
        private static Vector2 Snap(Vector2 v, float grid) =>
            new Vector2(MathF.Round(v.X / grid) * grid, MathF.Round(v.Y / grid) * grid);

        /// <summary>
        /// Draws a filled pixel-art diamond using horizontal scanline rects,
        /// each rect being exactly <paramref name="px"/> tall and snapped to grid.
        /// </summary>
        private static void DrawPixelDiamond(ImDrawListPtr dl, Vector2 center, float radius, uint color, float px)
        {
            int steps = (int)(radius / px);
            for (int i = -steps; i <= steps; i++)
            {
                float y = MathF.Round(center.Y + i * px);
                float half = MathF.Round((1f - MathF.Abs(i / (float)steps)) * radius);
                // Snap half-width to px grid so edges are crisp
                half = MathF.Round(half / px) * px;
                float x0 = MathF.Round(center.X - half);
                float x1 = MathF.Round(center.X + half);
                dl.AddRectFilled(new Vector2(x0, y), new Vector2(x1, y + px), color);
            }
        }

        /// <summary>
        /// Draws a pixel-art diamond outline (ring only) — one px thick border
        /// by drawing fill at radius and fill again at (radius - px) with background punch-through.
        /// We simply draw the outer scanlines only (top/bottom 1px rows of each half).
        /// </summary>
        private static void DrawPixelDiamondOutline(ImDrawListPtr dl, Vector2 center, float radius, uint color, float px)
        {
            int steps = (int)(radius / px);
            for (int i = -steps; i <= steps; i++)
            {
                float t = MathF.Abs(i / (float)steps);
                float y = MathF.Round(center.Y + i * px);
                float half = MathF.Round((1f - t) * radius);
                half = MathF.Round(half / px) * px;

                float halfInner = MathF.Round(half - px);
                halfInner = MathF.Max(0f, halfInner);

                float x0 = MathF.Round(center.X - half);
                float x1 = MathF.Round(center.X - halfInner);
                float x2 = MathF.Round(center.X + halfInner);
                float x3 = MathF.Round(center.X + half);

                // Left border strip
                if (x1 > x0)
                    dl.AddRectFilled(new Vector2(x0, y), new Vector2(x1, y + px), color);
                // Right border strip
                if (x3 > x2)
                    dl.AddRectFilled(new Vector2(x2, y), new Vector2(x3, y + px), color);
            }
        }

        /// <summary>
        /// Draws a line as a chain of pixel-sized squares snapped to the grid,
        /// giving it a chunky dot-matrix appearance instead of a smooth anti-aliased line.
        /// </summary>
        private static void DrawPixelLine(ImDrawListPtr dl, Vector2 from, Vector2 to, uint color, float px)
        {
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float dist = MathF.Sqrt(dx * dx + dy * dy);
            if (dist < 0.001f) return;

            int steps = (int)(dist / px);
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float x = MathF.Round((from.X + dx * t) / px) * px;
                float y = MathF.Round((from.Y + dy * t) / px) * px;
                dl.AddRectFilled(new Vector2(x, y), new Vector2(x + px, y + px), color);
            }
        }

        // ── Colour helpers ────────────────────────────────────────────────────

        private static uint Rgba(byte r, byte g, byte b, byte a) =>
            (uint)a << 24 | (uint)b << 16 | (uint)g << 8 | r;

        private static uint FadeAlpha(uint col, float alpha)
        {
            uint a = (col >> 24) & 0xFF;
            a = (uint)(a * alpha);
            return (col & 0x00FFFFFF) | (a << 24);
        }

        private static System.Numerics.Vector4 ToVec4(uint col)
        {
            float r = (col & 0xFF) / 255f;
            float g = ((col >> 8) & 0xFF) / 255f;
            float b = ((col >> 16) & 0xFF) / 255f;
            float a = ((col >> 24) & 0xFF) / 255f;
            return new System.Numerics.Vector4(r, g, b, a);
        }
    }
}