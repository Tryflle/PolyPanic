#version 330 core

layout (location = 0) in vec3 aPos;
layout (location = 1) in vec3 aNormal;
layout (location = 2) in vec2 aTexCoord;

out vec3 fragPos;
out vec3 fragNormal;
out vec2 fragTexCoord;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
uniform mat4 normalMatrix;

void main()
{
    fragPos = vec3(model * vec4(aPos, 1.0));
    fragNormal = mat3(normalMatrix) * aNormal;
    fragTexCoord = aTexCoord;
    gl_Position = projection * view * vec4(fragPos, 1.0);
}