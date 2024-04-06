#version 300 es

// Multiview
#define NUM_VIEWS 2
#extension GL_OVR_multiview2 : enable
layout(num_views = NUM_VIEWS) in;

precision mediump float;

// In
in highp vec4 attr_Vertex;
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
uniform vec4 u_clipPlane;

// Out
// gl_Position
out vec2 var_TexDiffuse;
out vec2 var_TexClip;

void main()
{
    var_TexDiffuse = (u_textureMatrix * attr_TexCoord).xy;  // Homogeneous coordinates of textureMatrix supposed to be 1

    var_TexClip = vec2(dot(u_clipPlane, attr_Vertex), 0.5);

    gl_Position = u_projectionMatrix * (u_viewMatrices[gl_ViewID_OVR] * (u_modelMatrix * attr_Vertex));
}