using OpenTK.Mathematics;
using VoidZero.Game;
using VoidZero.Game.Entities;

namespace VoidZero.States.Stages
{
    /// <summary>
    /// Maps a 7×7 logical grid to world positions.
    ///
    /// Column/Row indices:
    ///   0           → one cell outside the left/top edge  (spawn ring)
    ///   1 – 5       → the inner 5×5 play area
    ///   6           → one cell outside the right/bottom edge (spawn ring)
    ///
    /// Example grid (C = col, R = row):
    ///
    ///   C: 0   1   2   3   4   5   6
    /// R:0  ·   ·   ·   ·   ·   ·   ·   ← spawn ring (above screen)
    ///   1  ·   +   +   +   +   +   ·<
    ///   2  ·   +   +   +   +   +   ·   ← play area (5×5)
    ///   3  ·   +   +   +   +   +   ·
    ///   4  ·   +   +   +   +   +   ·
    ///   5  ·   +   +   +   +   +   ·
    ///   6  ·   ·   ·   ·   ·   ·   ·   ← spawn ring (below screen)
    ///      ↑                       ↑
    ///   left ring               right ring
    /// </summary>
    public static class StageGrid
    {
        public static float Norm(int index) => _norm[index];
        public static Vector2 GridNorm(int col, int row) => new Vector2(_norm[col], _norm[row]);

        private static readonly float[] _norm = new float[13]
        {
            -0.2f,
            0.05f,
            0.1f,
            0.2f,
            0.3f,
            0.4f,
            0.5f,
            0.6f,
            0.7f,
            0.8f,
            0.9f,
            0.95f,
            1.2f,
        };

        /// <summary>
        /// Returns the top-left world position for an entity placed at grid (col, row),
        /// centred on the cell and adjusted for the entity's dimensions.
        /// </summary>
        public static Vector2 CellPosition(int col, int row, Entity entity)
        {
            float w = GameServices.Instance.Settings.WorldWidth;
            float h = GameServices.Instance.Settings.WorldHeight;

            return new Vector2(
                _norm[col] * w - entity.Width / 2f,
                _norm[row] * h - entity.Height / 2f
            );
        }

        /// <summary>
        /// True when the column is in the outer spawn ring (col 0 or col 6).
        /// </summary>
        public static bool IsOuterCol(int col) => col == 0 || col == 6;

        /// <summary>
        /// True when the row is in the outer spawn ring (row 0 or row 6).
        /// </summary>
        public static bool IsOuterRow(int row) => row == 0 || row == 6;
    }
}