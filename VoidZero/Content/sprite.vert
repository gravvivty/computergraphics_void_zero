#version 330 core

layout (location = 0) in vec2 aPos;
layout (location = 1) in vec2 aTex;
layout (location = 2) in vec4 aColor;

out vec2 TexCoord;
out vec4 VertexColor;

uniform mat4 projection;

void main()
{
    TexCoord = aTex;
    VertexColor = aColor;
    gl_Position = projection * vec4(aPos, 0.0, 1.0);
}