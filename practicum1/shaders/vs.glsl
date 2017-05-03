#version 330
in vec3 vPosition;
in vec3 vColor;
out vec4 color;
uniform mat4 M;
void main()
{
 gl_Position = M * vec4( vPosition, 1.0 );
 color = vec4( -vPosition.z/4.0f, -vPosition.z/2.0f, -vPosition.z, 1.0);
}
