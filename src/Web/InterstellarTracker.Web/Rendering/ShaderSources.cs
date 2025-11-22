namespace InterstellarTracker.Web.Rendering;

/// <summary>
/// Contains GLSL shader source code for rendering.
/// </summary>
public static class ShaderSources
{
    /// <summary>
    /// Basic vertex shader with MVP transformation and Phong lighting setup.
    /// </summary>
    public const string VertexShader = @"
#version 330 core

layout (location = 0) in vec3 aPosition;
layout (location = 1) in vec3 aNormal;

out vec3 FragPos;
out vec3 Normal;

uniform mat4 uModel;
uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    FragPos = vec3(uModel * vec4(aPosition, 1.0));
    Normal = mat3(transpose(inverse(uModel))) * aNormal;
    
    gl_Position = uProjection * uView * vec4(FragPos, 1.0);
}
";

    /// <summary>
    /// Basic fragment shader with Phong lighting model.
    /// Supports ambient, diffuse, and specular lighting from a single directional light (Sun).
    /// </summary>
    public const string FragmentShader = @"
#version 330 core

in vec3 FragPos;
in vec3 Normal;

out vec4 FragColor;

uniform vec3 uObjectColor;
uniform vec3 uLightPos;      // Sun position
uniform vec3 uViewPos;       // Camera position
uniform float uEmissive;     // 1.0 for Sun (self-illuminating), 0.0 for others

void main()
{
    // If object is emissive (Sun), just use object color
    if (uEmissive > 0.5)
    {
        FragColor = vec4(uObjectColor, 1.0);
        return;
    }

    // Phong lighting for non-emissive objects
    vec3 norm = normalize(Normal);
    vec3 lightDir = normalize(uLightPos - FragPos);
    
    // Ambient
    float ambientStrength = 0.1;
    vec3 ambient = ambientStrength * vec3(1.0, 1.0, 1.0);
    
    // Diffuse
    float diff = max(dot(norm, lightDir), 0.0);
    vec3 diffuse = diff * vec3(1.0, 1.0, 1.0);
    
    // Specular
    float specularStrength = 0.5;
    vec3 viewDir = normalize(uViewPos - FragPos);
    vec3 reflectDir = reflect(-lightDir, norm);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), 32);
    vec3 specular = specularStrength * spec * vec3(1.0, 1.0, 1.0);
    
    vec3 result = (ambient + diffuse + specular) * uObjectColor;
    FragColor = vec4(result, 1.0);
}
";

    /// <summary>
    /// Simple line shader for drawing orbital paths.
    /// </summary>
    public const string LineVertexShader = @"
#version 330 core

layout (location = 0) in vec3 aPosition;

uniform mat4 uView;
uniform mat4 uProjection;

void main()
{
    gl_Position = uProjection * uView * vec4(aPosition, 1.0);
}
";

    /// <summary>
    /// Simple line fragment shader (solid color).
    /// </summary>
    public const string LineFragmentShader = @"
#version 330 core

out vec4 FragColor;

uniform vec3 uLineColor;

void main()
{
    FragColor = vec4(uLineColor, 0.8);
}
";
}
