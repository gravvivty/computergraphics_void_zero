#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTex;
layout (location = 2) in vec4 aColor;
layout (location = 3) in float aGlow;

out vec2 TexCoord;
out vec4 VertexColor;
out float GlowIntensity;

uniform mat4 projection;

void main()
{
    TexCoord = aTex;
    VertexColor = aColor;
    GlowIntensity = aGlow;
    gl_Position = projection * vec4(aPos, 0.0, 1.0);
}
