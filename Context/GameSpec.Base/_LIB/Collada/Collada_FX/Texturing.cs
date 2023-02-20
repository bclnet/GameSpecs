using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "alpha", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Alpha
    {
        [XmlAttribute("operator"), DefaultValue(Grendgine_Collada_Alpha_Operator.ADD)] public Grendgine_Collada_Alpha_Operator Operator;
        [XmlAttribute("scale")] public float Scale;
        [XmlElement(ElementName = "argument")] public Grendgine_Collada_Argument_Alpha[] Argument;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "argument", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Argument_Alpha
    {
        [XmlAttribute("source")] public Grendgine_Collada_Argument_Source Source;
        [XmlAttribute("operand"), DefaultValue(Grendgine_Collada_Argument_Alpha_Operand.SRC_ALPHA)] public Grendgine_Collada_Argument_Alpha_Operand Operand;
        [XmlAttribute("sampler")] public string Sampler;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "argument", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Argument_RGB
    {
        [XmlAttribute("source")] public Grendgine_Collada_Argument_Source Source;
        [XmlAttribute("operand"), DefaultValue(Grendgine_Collada_Argument_RGB_Operand.SRC_COLOR)] public Grendgine_Collada_Argument_RGB_Operand Operand;
        [XmlAttribute("sampler")] public string Sampler;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "create_2d", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Create_2D
    {
        [XmlElement(ElementName = "size_exact")] public Grendgine_Collada_Size_2D Size_Exact;
        [XmlElement(ElementName = "size_ratio")] public Grendgine_Collada_Size_Ratio Size_Ratio;
        [XmlElement(ElementName = "mips")] public Grendgine_Collada_Mips_Attribute Mips;
        [XmlElement(ElementName = "unnormalized")] public XmlElement Unnormalized;
        [XmlElement(ElementName = "array")] public Grendgine_Collada_Array_Length Array_Length;
        [XmlElement(ElementName = "format")] public Grendgine_Collada_Format Format;
        [XmlElement(ElementName = "init_from")] public Grendgine_Collada_Init_From[] Init_From;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "create_3d", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Create_3D
    {
        [XmlElement(ElementName = "size")] public Grendgine_Collada_Size_3D Size;
        [XmlElement(ElementName = "mips")] public Grendgine_Collada_Mips_Attribute Mips;
        [XmlElement(ElementName = "array")] public Grendgine_Collada_Array_Length Array_Length;
        [XmlElement(ElementName = "format")] public Grendgine_Collada_Format Format;
        [XmlElement(ElementName = "init_from")] public Grendgine_Collada_Init_From[] Init_From;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "create_cube", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Create_Cube
    {
        [XmlElement(ElementName = "size")] public Grendgine_Collada_Size_Width_Only Size;
        [XmlElement(ElementName = "mips")] public Grendgine_Collada_Mips_Attribute Mips;
        [XmlElement(ElementName = "array")] public Grendgine_Collada_Array_Length Array_Length;
        [XmlElement(ElementName = "format")] public Grendgine_Collada_Format Format;
        [XmlElement(ElementName = "init_from")] public Grendgine_Collada_Init_From[] Init_From;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "format", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Format
    {
        [XmlElement(ElementName = "hint")] public Grendgine_Collada_Format_Hint Hint;
        [XmlElement(ElementName = "exact")] public string Exact;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "fx_sampler_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_FX_Sampler_Common
    {
        [XmlElement(ElementName = "texcoord")] public Grendgine_Collada_TexCoord_Semantic TexCoord_Semantic;
        [XmlElement(ElementName = "wrap_s")] public Grendgine_Collada_FX_Sampler_Common_Wrap_Mode Wrap_S; //: [DefaultValue(Grendgine_Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
        [XmlElement(ElementName = "wrap_t")] public Grendgine_Collada_FX_Sampler_Common_Wrap_Mode Wrap_T; //: [DefaultValue(Grendgine_Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
        [XmlElement(ElementName = "wrap_p")] public Grendgine_Collada_FX_Sampler_Common_Wrap_Mode Wrap_P; //: [DefaultValue(Grendgine_Collada_FX_Sampler_Common_Wrap_Mode.WRAP)]		
        [XmlElement(ElementName = "minfilter")] public Grendgine_Collada_FX_Sampler_Common_Filter_Type MinFilter; //: [DefaultValue(Grendgine_Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
        [XmlElement(ElementName = "magfilter")] public Grendgine_Collada_FX_Sampler_Common_Filter_Type MagFilter; //: [DefaultValue(Grendgine_Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
        [XmlElement(ElementName = "mipfilter")] public Grendgine_Collada_FX_Sampler_Common_Filter_Type MipFilter; //: [DefaultValue(Grendgine_Collada_FX_Sampler_Common_Filter_Type.LINEAR)]		
        [XmlElement(ElementName = "border_color")] public Grendgine_Collada_Float_Array_String Border_Color;
        [XmlElement(ElementName = "mip_max_level")] public byte Mip_Max_Level;
        [XmlElement(ElementName = "mip_min_level")] public byte Mip_Min_Level;
        [XmlElement(ElementName = "mip_bias")] public float Mip_Bias;
        [XmlElement(ElementName = "max_anisotropy")] public int Max_Anisotropy;
        [XmlElement(ElementName = "instance_image")] public Grendgine_Collada_Instance_Image Instance_Image;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Image
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "renderable")] public Grendgine_Collada_Renderable_Share Renderable_Share;
        [XmlElement(ElementName = "init_from")] public Grendgine_Collada_Init_From Init_From;
        [XmlElement(ElementName = "create_2d")] public Grendgine_Collada_Create_2D Create_2D;
        [XmlElement(ElementName = "create_3d")] public Grendgine_Collada_Create_3D Create_3D;
        [XmlElement(ElementName = "create_cube")] public Grendgine_Collada_Create_Cube Create_Cube;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "init_from", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Init_From
    {
        // Commented out parts are not recognized in Blender (and probably not part of Collada 1.4.1)
        //[XmlAttribute("mips_generate")] public bool Mips_Generate;
        //[XmlAttribute("array_index")] public int Array_Index;
        //[XmlAttribute("mip_index")] public int Mip_Index;
        // Uri added to support 1.4.1 formats
        [XmlText()] public string Uri;
        //[XmlAttribute("depth")] public int Depth;
        [XmlAttribute("face"), DefaultValue(Grendgine_Collada_Face.POSITIVE_X)] public Grendgine_Collada_Face Face;
        [XmlElement(ElementName = "ref")] public string Ref;
        [XmlElement(ElementName = "hex")] public Grendgine_Collada_Hex Hex;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Image
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_images", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Images
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "image")] public Grendgine_Collada_Image[] Image;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "annotate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_RGB
    {
        [XmlAttribute("operator"), DefaultValue(Grendgine_Collada_RGB_Operator.ADD)] public Grendgine_Collada_RGB_Operator Operator;
        [XmlAttribute("scale")] public float Scale;
        [XmlElement(ElementName = "argument")] public Grendgine_Collada_Argument_RGB[] Argument;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler1D", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Sampler1D : Grendgine_Collada_FX_Sampler_Common
    {
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler2D", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Sampler2D : Grendgine_Collada_FX_Sampler_Common
    {
        [XmlElement(ElementName = "source")] public string Source;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler3D", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Sampler3D : Grendgine_Collada_FX_Sampler_Common
    {
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "samplerCUBE", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_SamplerCUBE : Grendgine_Collada_FX_Sampler_Common
    {
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "samplerDEPTH", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_SamplerDEPTH : Grendgine_Collada_FX_Sampler_Common
    {
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "samplerRECT", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_SamplerRECT : Grendgine_Collada_FX_Sampler_Common
    {
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texcombiner", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_TexCombiner
    {
        [XmlElement(ElementName = "constant")] public Grendgine_Collada_Constant_Attribute Constant;
        [XmlElement(ElementName = "RGB")] public Grendgine_Collada_RGB RGB;
        [XmlElement(ElementName = "alpha")] public Grendgine_Collada_Alpha Alpha;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texenv", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_TexEnv
    {
        [XmlAttribute("operator")] public Grendgine_Collada_TexEnv_Operator Operator;
        [XmlAttribute("sampler")] public string Sampler;
        [XmlElement(ElementName = "constant")] public Grendgine_Collada_Constant_Attribute Constant;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texture_pipeline", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Texture_Pipeline
    {
        [XmlAttribute("sid")] public string sID;
        [XmlElement(ElementName = "texcombiner")] public Grendgine_Collada_TexCombiner[] TexCombiner;
        [XmlElement(ElementName = "texenv")] public Grendgine_Collada_TexEnv[] TexEnv;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

