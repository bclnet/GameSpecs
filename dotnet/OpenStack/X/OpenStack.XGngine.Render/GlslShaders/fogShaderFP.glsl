#version 300 es
precision mediump float;

// In
in vec2 var_TexFog;            // input Fog TexCoord
in vec2 var_TexFogEnter;       // input FogEnter TexCoord

// Uniforms
uniform sampler2D u_fragmentMap0;   // Fog Image
uniform sampler2D u_fragmentMap1;   // Fog Enter Image
uniform lowp vec4 u_fogColor;       // Fog Color

// Out
layout(location = 0) out vec4 fragColor;

void main()
{
	fragColor = texture(u_fragmentMap0, var_TexFog) * texture(u_fragmentMap1, var_TexFogEnter) * vec4(u_fogColor.rgb, 1.0);
}