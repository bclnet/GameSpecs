#version 300 es

// Multiview
#define NUM_VIEWS 2
#extension GL_OVR_multiview2 : enable
layout(num_views = NUM_VIEWS) in;


precision mediump float;

// In
in highp vec4 attr_Vertex;
in lowp vec4 attr_Color;
in vec4 attr_TexCoord;

// Uniforms
layout(shared) uniform ViewMatrices
{
    uniform highp mat4 u_viewMatrices[NUM_VIEWS];
};
layout(shared) uniform ProjectionMatrix
{
    uniform highp mat4 u_projectionMatrix;
};
uniform highp mat4 u_modelMatrix;
uniform mat4 u_textureMatrix;
uniform lowp float u_colorAdd;
uniform lowp float u_colorModulate;

// Out
// gl_Position
out vec2 var_TexCoord;
out lowp vec4 var_Color;

void main()
{
    var_TexCoord = (u_textureMatrix * attr_TexCoord).xy;  // Homogeneous coordinates of textureMatrix supposed to be 1

    if (u_colorModulate == 0.0) {
        var_Color = vec4(u_colorAdd);
    }
    else {
        var_Color = (attr_Color * u_colorModulate) + vec4(u_colorAdd);
    }

    gl_Position = u_projectionMatrix * (u_viewMatrices[gl_ViewID_OVR] * (u_modelMatrix * attr_Vertex));
}