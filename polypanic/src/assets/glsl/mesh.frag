#version 330 core

in vec3 fragPos;
in vec3 fragNormal;
in vec2 fragTexCoord;

out vec4 fragColor;

uniform vec3 objectColor;
uniform vec3 lightColor;
uniform vec3 lightPos;
uniform vec3 viewPos;
uniform sampler2D texture_diffuse1;
uniform int hasTexture;

void main()
{
    vec3 norm = normalize(fragNormal);

    float ambientStrength = 1.0;
    vec3 ambient = ambientStrength * lightColor;

    vec3 lightDir = normalize(lightPos - fragPos);
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * lightColor;
    
    float specularStrength = 0.5;
    vec3 viewDir = normalize(viewPos - fragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * lightColor;
    
    vec3 lighting = ambient + diffuse + specular;

    vec3 baseColor;
    if (hasTexture == 1) {
        baseColor = texture(texture_diffuse1, fragTexCoord).rgb;
    } else {
        baseColor = objectColor;
    }
    
    vec3 result = lighting * baseColor;
    fragColor = vec4(result, 1.0);
}