#version 330 core

in vec2 TexCoord;
in vec4 VertexColor;
in float GlowIntensity;

out vec4 FragColor;

uniform sampler2D texture0;
uniform float glowPower;

void main()
{
    vec4 color = texture(texture0, TexCoord) * VertexColor;
    color.rgb += color.rgb * GlowIntensity * glowPower;

    FragColor = color;
}
