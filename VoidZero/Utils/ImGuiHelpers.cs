using ImGuiNET;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidZero.Utils
{
    public static class ImGuiHelpers
    {
        public static void BeginCenteredBlock(float totalHeight)
        {
            float windowHeight = ImGui.GetWindowHeight();
            ImGui.SetCursorPosY((windowHeight - totalHeight) / 2f);
        }

        public static void CenterNextItem(float width)
        {
            float x =
                (ImGui.GetWindowSize().X - width) * 0.5f;

            ImGui.SetCursorPosX(x);
        }

        public static void CenterText(string text)
        {
            float textWidth = ImGui.CalcTextSize(text).X;

            float x =
                (ImGui.GetWindowSize().X - textWidth) * 0.5f;

            ImGui.SetCursorPosX(x);

            ImGui.Text(text);
        }

        public static void CenterColoredText(string text, Vector4 color)
        {
            float width = ImGui.CalcTextSize(text).X;
            CenterNextItem(width);
            ImGui.TextColored((System.Numerics.Vector4)color, text);
        }

        public static void CenterRainbowText(string text, float timer)
        {
            float hueStep = 1f / text.Length;

            float totalWidth = 0f;
            foreach (char c in text)
                totalWidth += ImGui.CalcTextSize(c.ToString()).X;

            float startX = (ImGui.GetWindowSize().X - totalWidth) * 0.5f + ImGui.GetWindowPos().X;
            float cursorY = ImGui.GetCursorScreenPos().Y;

            var drawList = ImGui.GetWindowDrawList();

            for (int i = 0; i < text.Length; i++)
            {
                float hue = (timer + i * hueStep) % 1f;
                ImGui.ColorConvertHSVtoRGB(hue, 1f, 1f, out float r, out float g, out float b);
                uint color = ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(r, g, b, 1f));

                drawList.AddText(
                    new System.Numerics.Vector2(startX, cursorY),
                    color,
                    text[i].ToString());

                startX += ImGui.CalcTextSize(text[i].ToString()).X;
            }

            ImGui.Dummy(new System.Numerics.Vector2(totalWidth, ImGui.GetTextLineHeight()));
        }
    }
}
