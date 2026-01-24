#version 330 core

in vec2 TexCoord;
in vec4 VertexColor;

out vec4 FragColor;

uniform sampler2D texture0;
uniform float grayscale;

void main()
{
    vec4 color = texture(texture0, TexCoord) * VertexColor;

    float gray = dot(color.rgb, vec3(0.2126, 0.7152, 0.0722));
    color.rgb = mix(color.rgb, vec3(gray), grayscale);

    FragColor = color;
}