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
    uniform highp mat4 u_viewMatrices [NUM_VIEWS];
};
layout(shared) uniform ProjectionMatrix
{
    uniform highp mat4 u_projectionMatrix;
};
uniform highp mat4 u_modelMatrix;
uniform mat4 u_fogMatrix;

// Out
// gl_Position
out vec2 var_TexFog;
out vec2 var_TexFogEnter;

void main()
{
    gl_Position = u_projectionMatrix * (u_viewMatrices[gl_ViewID_OVR] * (u_modelMatrix * attr_Vertex));

    // What will be computed:
    //
    // vec4 tc;
    // tc.x = dot( u_fogMatrix[0], attr_Vertex );
    // tc.y = dot( u_fogMatrix[1], attr_Vertex );
    // tc.z = 0.0;
    // tc.w = dot( u_fogMatrix[2], attr_Vertex );
    // var_TexFog.xy = tc.xy / tc.w;
    //
    // var_TexFogEnter.x = dot( u_fogMatrix[3], attr_Vertex );
    // var_TexFogEnter.y = 0.5;

    // Optimized version:
    //
    var_TexFog = vec2(dot(u_fogMatrix[0], attr_Vertex), dot(u_fogMatrix[1], attr_Vertex)) / dot(u_fogMatrix[2], attr_Vertex);
    var_TexFogEnter = vec2(dot(u_fogMatrix[3], attr_Vertex), 0.5);
}