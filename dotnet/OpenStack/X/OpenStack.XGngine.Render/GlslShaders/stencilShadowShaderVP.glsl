#version 300 es

// Multiview
#define NUM_VIEWS 2
#extension GL_OVR_multiview2 : enable
layout(num_views = NUM_VIEWS) in;

precision mediump float;

// In
in highp vec4 attr_Vertex;

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
uniform vec4 u_lightOrigin;

// Out
// gl_Position

void main()
{
    gl_Position = u_projectionMatrix * (u_viewMatrices[gl_ViewID_OVR] * (u_modelMatrix * (attr_Vertex.w * u_lightOrigin + attr_Vertex - u_lightOrigin)));
}