#version 300 es

// Multiview
#define NUM_VIEWS 2
#extension GL_OVR_multiview2 : enable
layout(num_views = NUM_VIEWS) in;

precision mediump float;

// In
in highp vec4 attr_Vertex;
in lowp vec4 attr_Color;

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
uniform vec4 u_viewOrigin;

// Out
// gl_Position
out vec3 var_TexCoord;
out lowp vec4 var_Color;

void main()
{
    var_TexCoord = (u_textureMatrix * (attr_Vertex - u_viewOrigin)).xyz;

    if (u_colorModulate == 0.0) {
        var_Color = vec4(u_colorAdd);
    }
    else {
        var_Color = (attr_Color * u_colorModulate) + vec4(u_colorAdd);
    }

    gl_Position = u_projectionMatrix * (u_viewMatrices[gl_ViewID_OVR] * (u_modelMatrix * attr_Vertex));
}