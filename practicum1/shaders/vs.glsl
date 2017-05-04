#version 330
in vec3 vPosition;
in vec3 vNormal;
in vec3 vColor;
out vec4 color;
out vec4 normal;
uniform mat4 M;
uniform mat4 S;
void main()
{
 gl_Position = M * S * vec4( vPosition, 1.0 );
 color = S * vec4((vPosition.x + vPosition.y) / 2 - 16.0 * vPosition.z, (vPosition.x + vPosition.y) / 2 - 16.0 * vPosition.z, -8.0 * vPosition.z , 1.0 );
 normal = normal;
}
