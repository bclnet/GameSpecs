using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "blinn", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Blinn
    {
        [XmlElement(ElementName = "emission")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Eission;
        [XmlElement(ElementName = "ambient")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Ambient;
        [XmlElement(ElementName = "diffuse")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Diffuse;
        [XmlElement(ElementName = "specular")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Specular;
        [XmlElement(ElementName = "transparent")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Transparent;
        [XmlElement(ElementName = "reflective")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Reflective;
        [XmlElement(ElementName = "shininess")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Shininess;
        [XmlElement(ElementName = "reflectivity")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Reflectivity;
        [XmlElement(ElementName = "transparency")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Transparency;
        [XmlElement(ElementName = "index_of_refraction")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "color_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Color_Clear : Grendgine_Collada_Float_Array_String
    {
        [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "color_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Color_Target
    {
        [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
        [XmlAttribute("slice"), DefaultValue(typeof(int), "0")] public int Slice;
        [XmlAttribute("mip"), DefaultValue(typeof(int), "0")] public int Mip;
        [XmlAttribute("face")] [DefaultValue(Grendgine_Collada_Face.POSITIVE_X)] public Grendgine_Collada_Face Face;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
        [XmlElement(ElementName = "instance_image")] public Grendgine_Collada_Instance_Image Instance_Image;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "constant", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Constant
    {
        [XmlElement(ElementName = "emission")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Eission;
        [XmlElement(ElementName = "reflective")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Reflective;
        [XmlElement(ElementName = "reflectivity")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Reflectivity;
        [XmlElement(ElementName = "transparent")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Transparent;
        [XmlElement(ElementName = "transparency")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Transparency;
        [XmlElement(ElementName = "index_of_refraction")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "depth_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Depth_Clear
    {
        [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
        [XmlText()] public float Value;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "depth_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Depth_Target
    {
        [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
        [XmlAttribute("slice"), DefaultValue(typeof(int), "0")] public int Slice;
        [XmlAttribute("mip"), DefaultValue(typeof(int), "0")] public int Mip;
        [XmlAttribute("face"), DefaultValue(Grendgine_Collada_Face.POSITIVE_X)] public Grendgine_Collada_Face Face;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
        [XmlElement(ElementName = "instance_image")] public Grendgine_Collada_Instance_Image Instance_Image;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "draw", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Draw
    {
        [XmlText()] public string Value;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "evaluate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Effect_Technique_Evaluate
    {
        [XmlElement(ElementName = "color_target")] public Grendgine_Collada_Color_Target Color_Target;
        [XmlElement(ElementName = "depth_target")] public Grendgine_Collada_Depth_Target Depth_Target;
        [XmlElement(ElementName = "stencil_target")] public Grendgine_Collada_Stencil_Target Stencil_Target;
        [XmlElement(ElementName = "color_clear")] public Grendgine_Collada_Color_Clear Color_Clear;
        [XmlElement(ElementName = "depth_clear")] public Grendgine_Collada_Depth_Clear Depth_Clear;
        [XmlElement(ElementName = "stencil_clear")] public Grendgine_Collada_Stencil_Clear Stencil_Clear;
        [XmlElement(ElementName = "draw")] public Grendgine_Collada_Draw Draw;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "fx_common_color_or_texture_type", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_FX_Common_Color_Or_Texture_Type
    {
        [XmlAttribute("opaque"), DefaultValue(Grendgine_Collada_FX_Opaque_Channel.A_ONE)] public Grendgine_Collada_FX_Opaque_Channel Opaque;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
        [XmlElement(ElementName = "color")] public Grendgine_Collada_Color Color;
        [XmlElement(ElementName = "texture")] public Grendgine_Collada_Texture Texture;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "fx_common_float_or_param_type", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_FX_Common_Float_Or_Param_Type
    {
        [XmlElement(ElementName = "float")] public Grendgine_Collada_SID_Float Float;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Material_Rendering
    {
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "technique_override")] public Grendgine_Collada_Technique_Override Technique_Override;
        [XmlElement(ElementName = "bind")] public Materials[] Bind;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "lambert", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Lambert
    {
        [XmlElement(ElementName = "emission")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Eission;
        [XmlElement(ElementName = "ambient")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Ambient;
        [XmlElement(ElementName = "diffuse")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Diffuse;
        [XmlElement(ElementName = "reflective")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Reflective;
        [XmlElement(ElementName = "transparent")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Transparent;
        [XmlElement(ElementName = "reflectivity")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Reflectivity;
        [XmlElement(ElementName = "transparency")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Transparency;
        [XmlElement(ElementName = "index_of_refraction")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Pass
    {
        [XmlAttribute("sid")] public string sID;
        [XmlElement(ElementName = "annotate")] public Grendgine_Collada_Annotate[] Annotate;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "states")] public Grendgine_Collada_States States;
        [XmlElement(ElementName = "evaluate")] public Grendgine_Collada_Effect_Technique_Evaluate Evaluate;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "phong", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Phong
    {
        [XmlElement(ElementName = "emission")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Emission;
        [XmlElement(ElementName = "ambient")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Ambient;
        [XmlElement(ElementName = "diffuse")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Diffuse;
        [XmlElement(ElementName = "specular")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Specular;
        [XmlElement(ElementName = "transparent")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Transparent;
        [XmlElement(ElementName = "reflective")] public Grendgine_Collada_FX_Common_Color_Or_Texture_Type Reflective;
        [XmlElement(ElementName = "shininess")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Shininess;
        [XmlElement(ElementName = "reflectivity")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Reflectivity;
        [XmlElement(ElementName = "transparency")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Transparency;
        [XmlElement(ElementName = "index_of_refraction")] public Grendgine_Collada_FX_Common_Float_Or_Param_Type Index_Of_Refraction;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "render", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Render
    {
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("sid")] public string sid;
        [XmlAttribute("camera_node")] public string Camera_Node;
        [XmlElement(ElementName = "layer")] public string[] Layer;
        [XmlElement(ElementName = "instance_material")] public Grendgine_Collada_Instance_Material_Rendering Instance_Material;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "states", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_States
    {
        [XmlAnyElement] public XmlElement[] Data;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "stencil_clear", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Stencil_Clear
    {
        [XmlAttribute("index"), DefaultValue(typeof(int), "0")] public int Index;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "stencil_target", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Stencil_Target
    {
        [XmlAttribute("index"), DefaultValue(typeof(int), "1")] public int Index;
        [XmlAttribute("slice"), DefaultValue(typeof(int), "0")] public int Slice;
        [XmlAttribute("mip"), DefaultValue(typeof(int), "0")] public int Mip;
        [XmlAttribute("face"), DefaultValue(Grendgine_Collada_Face.POSITIVE_X)] public Grendgine_Collada_Face Face;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
        [XmlElement(ElementName = "instance_image")] public Grendgine_Collada_Instance_Image Instance_Image;
    }
}

