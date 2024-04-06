#version 300 es
precision mediump float;

// In
in vec2 var_TexDiffuse;
in vec2 var_TexClip;

// Uniforms
uniform sampler2D u_fragmentMap0;
uniform sampler2D u_fragmentMap1;
uniform lowp float u_alphaTest;
uniform lowp vec4 u_glColor;

// Out
layout(location = 0) out vec4 fragColor;

void main()
{
    if (u_alphaTest > (texture(u_fragmentMap0, var_TexDiffuse).a * texture(u_fragmentMap1, var_TexClip).a)) {
        discard;
    }

    fragColor = u_glColor;
}