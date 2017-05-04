#version 330
in vec4 color;
in vec4 normal;
out vec4 outputColor;
void main()
{
 outputColor = color * dot(normal, vec4(0, 0, -1, 0));
}