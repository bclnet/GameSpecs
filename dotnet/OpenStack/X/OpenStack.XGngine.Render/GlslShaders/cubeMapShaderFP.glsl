#version 300 es
precision mediump float;

// In
in vec3 var_TexCoord;
in lowp vec4 var_Color;

// Uniforms
uniform samplerCube u_fragmentCubeMap0;
uniform lowp vec4 u_glColor;

// Out
layout(location = 0) out vec4 fragColor;

void main()
{
	fragColor = texture(u_fragmentCubeMap0, var_TexCoord) * u_glColor * var_Color;
}