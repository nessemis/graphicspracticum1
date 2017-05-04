#version 330
in vec3 vPosition;
//in vec3 vNormal;
in vec3 vColor;
out vec4 color;
uniform mat4 M;
void main()
{
 gl_Position = M * vec4( vPosition, 1.0 );
 color = 2 * vec4(gl_Position);
}
