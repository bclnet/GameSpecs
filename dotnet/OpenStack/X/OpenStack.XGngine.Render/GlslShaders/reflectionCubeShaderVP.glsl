#version 300 es

// Multiview
#define NUM_VIEWS 2
#if defined( GL_OVR_multiview2 )
#extension GL_OVR_multiview2 : enable
layout(num_views = NUM_VIEWS) in;
#define VIEW_ID gl_ViewID_OVR
#else
uniform lowp int ViewID;
#define VIEW_ID ViewID
#endif


precision mediump float;

// In
in highp vec4 attr_Vertex;
in lowp vec4 attr_Color;
in vec3 attr_TexCoord;

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
uniform mat4 u_modelViewMatrix;
uniform mat4 u_textureMatrix;
uniform lowp float u_colorAdd;
uniform lowp float u_colorModulate;

// Out
// gl_Position
out vec3 var_TexCoord;
out lowp vec4 var_Color;

void main()
{
    var_TexCoord = (u_textureMatrix * reflect(normalize(u_modelViewMatrix * attr_Vertex),
        // This suppose the modelView matrix is orthogonal
        // Otherwise, we should use the inverse transpose
        u_modelViewMatrix * vec4(attr_TexCoord, 0.0))).xyz;

    if (u_colorModulate == 0.0) {
        var_Color = vec4(u_colorAdd);
    }
    else {
        var_Color = (attr_Color * u_colorModulate) + vec4(u_colorAdd);
    }

    gl_Position = u_projectionMatrix * (u_viewMatrices[gl_ViewID_OVR] * (u_modelMatrix * attr_Vertex));
}