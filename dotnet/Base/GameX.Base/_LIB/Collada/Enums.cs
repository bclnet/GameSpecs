using System;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Alpha_Operator
    {
        REPLACE,
        MODULATE,
        ADD,
        ADD_SIGNED,
        INTERPOLATE,
        SUBTRACT
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Argument_Alpha_Operand
    {
        SRC_ALPHA,
        ONE_MINUS_SRC_ALPHA
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Argument_RGB_Operand
    {
        SRC_COLOR,
        ONE_MINUS_SRC_COLOR,
        SRC_ALPHA,
        ONE_MINUS_SRC_ALPHA
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Argument_Source
    {
        TEXTURE,
        CONSTANT,
        PRIMARY,
        PREVIOUS
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Face
    {
        POSITIVE_X,
        NEGATIVE_X,
        POSITIVE_Y,
        NEGATIVE_Y,
        POSITIVE_Z,
        NEGATIVE_Z
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Format_Hint_Channels
    {
        RGB,
        RGBA,
        RGBE,
        L,
        LA,
        D
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Format_Hint_Precision
    {
        DEFAULT,
        LOW,
        MID,
        HIGH,
        MAX
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Format_Hint_Range
    {
        SNORM,
        UNORM,
        SINT,
        UINT,
        FLOAT
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_FX_Opaque_Channel
    {
        A_ONE,
        RGB_ZERO,
        A_ZERO,
        RGB_ONE
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_FX_Sampler_Common_Filter_Type
    {
        NONE,
        NEAREST,
        LINEAR,
        ANISOTROPIC
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_FX_Sampler_Common_Wrap_Mode
    {
        WRAP,
        MIRROR,
        CLAMP,
        BORDER,
        MIRROR_ONCE,
        REPEAT,
        CLAMP_TO_EDGE,
        MIRRORED_REPEAT
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Geographic_Location_Altitude_Mode
    {
        absolute,
        relativeToGround
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Input_Semantic
    {
        BINORMAL,
        COLOR,
        CONTINUITY,
        IMAGE,
        INPUT,
        IN_TANGENT,
        INTERPOLATION,
        INV_BIND_MATRIX,
        JOINT,
        LINEAR_STEPS,
        MORPH_TARGET,
        MORPH_WEIGHT,
        NORMAL,
        OUTPUT,
        OUT_TANGENT,
        POSITION,
        TANGENT,
        TEXBINORMAL,
        TEXCOORD,
        TEXTANGENT,
        UV,
        VERTEX,
        WEIGHT,
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Modifier_Value
    {
        CONST,
        UNIFORM,
        VARYING,
        STATIC,
        VOLATILE,
        EXTERN,
        SHARED
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Node_Type
    {
        JOINT,
        NODE
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_RGB_Operator
    {
        REPLACE,
        MODULATE,
        ADD,
        ADD_SIGNED,
        INTERPOLATE,
        SUBTRACT,
        DOT3_RGB,
        DOT3_RGBA
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Sampler_Behavior
    {
        UNDEFINED,
        CONSTANT,
        GRADIENT,
        CYCLE,
        OSCILLATE,
        CYCLE_RELATIVE
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_Shader_Stage
    {
        TESSELATION,
        VERTEX,
        GEOMETRY,
        FRAGMENT
    }

    [Serializable, XmlType(Namespace = "http://www.collada.org/2005/11/COLLADASchema")]
    public enum Grendgine_Collada_TexEnv_Operator
    {
        REPLACE,
        MODULATE,
        DECAL,
        BLEND,
        ADD
    }
}

