using ImGuiNET;
using OpenTK.Mathematics;
using System.Numerics;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace VoidZero.UI
{
    public class DamageNumberManager
    {
        private readonly List<DamageNumber> _numbers = new();

        public void Spawn(Vector2 worldPos, float damage)
        {
            _numbers.Add(
                new DamageNumber(worldPos, damage)
            );
        }

        public void Update(float dt)
        {
            for (int i = _numbers.Count - 1; i >= 0; i--)
            {
                _numbers[i].Update(dt);

                if (_numbers[i].Dead)
                    _numbers.RemoveAt(i);
            }
        }

        public void Draw(Func<Vector2, System.Numerics.Vector2> worldToScreen)
        {
            var drawList = ImGui.GetForegroundDrawList();

            foreach (var n in _numbers)
            {
                string text = n.Damage.ToString("0.0");
                uint color = ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, n.Alpha));
                drawList.AddText(worldToScreen(n.Position), color, text);
            }
        }
    }
}