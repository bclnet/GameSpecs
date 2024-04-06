#version 300 es
precision mediump float;

// In
in vec2 var_TexCoord;
in lowp vec4 var_Color;

// Uniforms
uniform sampler2D u_fragmentMap0;
uniform lowp vec4 u_glColor;

// Out
layout(location = 0) out vec4 fragColor;

void main()
{
	fragColor = texture(u_fragmentMap0, var_TexCoord) * u_glColor * var_Color;
}