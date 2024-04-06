#version 300 es

// Multiview
#define NUM_VIEWS 2
#extension GL_OVR_multiview2 : enable
layout(num_views = NUM_VIEWS) in;

precision mediump float;

// In
in highp vec4 attr_Vertex;      // input Vertex Coordinates

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
uniform mat4 u_fogMatrix;        // fogPlanes 0, 1, 3 (CATION: not 2!), 2

// Out
// gl_Position                   // output Vertex Coordinates
out vec2 var_TexFog;         // output Fog TexCoord
out vec2 var_TexFogEnter;    // output FogEnter TexCoord

void main()
{
    gl_Position = u_projectionMatrix * (u_viewMatrices[gl_ViewID_OVR] * (u_modelMatrix * attr_Vertex));

    // What will be computed:
    //
    // var_TexFog.x      = dot(u_fogMatrix[0], attr_Vertex);
    // var_TexFog.y      = dot(u_fogMatrix[1], attr_Vertex);
    // var_TexFogEnter.x = dot(u_fogMatrix[2], attr_Vertex);
    // var_TexFogEnter.y = dot(u_fogMatrix[3], attr_Vertex);

    // Optimized version:
    var_TexFog = vec2(dot(u_fogMatrix[0], attr_Vertex), dot(u_fogMatrix[1], attr_Vertex));
    var_TexFogEnter = vec2(dot(u_fogMatrix[2], attr_Vertex), dot(u_fogMatrix[3], attr_Vertex));
}