#version 330 core
layout (location = 0) in vec3 aPos;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;

void main()
{
    // note: do not listen to the documentation about the order of multiplication. it breaks everything and they're wrong.
    gl_Position = projection * view * model * vec4(aPos, 1.0);
}