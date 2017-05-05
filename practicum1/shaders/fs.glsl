//FRAGMENT SHADER
#version 330
in vec4 color;

//normal vector of this vertex.
in vec4 normal;
out vec4 outputColor;
void main()
{
 //light rays drop straight down onto the surface (hence vec4(0, 0, -1, 0)).
 outputColor = color * dot(normal, vec4(0, 0, -1, 0));
}