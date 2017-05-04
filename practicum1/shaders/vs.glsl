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
 color = vec4(1.0, 1.0, 1.0 , 1.0);
 normal = normalize(vec4(vNormal.x, vNormal.y, vNormal.z, 0) * inverse(S));
}
