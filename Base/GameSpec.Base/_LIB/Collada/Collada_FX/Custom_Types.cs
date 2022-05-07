using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Array_Length
    {
        [XmlAttribute("length")] public int Length;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Constant_Attribute
    {
        [XmlAttribute("value")] public string Value_As_String;
        [XmlAttribute("param")] public string Param_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Format_Hint
    {
        [XmlAttribute("channels")] public Grendgine_Collada_Format_Hint_Channels Channels;
        [XmlAttribute("range")] public Grendgine_Collada_Format_Hint_Range Range;
        [XmlAttribute("precision"), DefaultValue(Grendgine_Collada_Format_Hint_Precision.DEFAULT)] public Grendgine_Collada_Format_Hint_Precision Precision;
        [XmlAttribute("space")] public string Hint_Space;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Hex
    {
        [XmlAttribute("format")] public string Format;
        [XmlText()] public string Value;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Mips_Attribute
    {
        [XmlAttribute("levels")] public int Levels;
        [XmlAttribute("auto_generate")] public bool Auto_Generate;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Ref_String
    {
        [XmlAttribute("ref")] public string Ref;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Renderable_Share
    {
        [XmlAttribute("share")] public bool Share;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Size_2D
    {
        [XmlAttribute("width")] public int Width;
        [XmlAttribute("height")] public int Height;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "size", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Size_3D
    {
        [XmlAttribute("width")] public int Width;
        [XmlAttribute("height")] public int Height;
        [XmlAttribute("depth")] public int Depth;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "size_ratio", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Size_Ratio
    {
        [XmlAttribute("width")] public float Width;
        [XmlAttribute("height")] public float Height;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "size", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Size_Width_Only
    {
        [XmlAttribute("width")] public int Width;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_override", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Override
    {
        [XmlAttribute("ref")] public string Ref;
        [XmlAttribute("pass")] public string Pass;
    }


    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texcoord", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_TexCoord_Semantic
    {
        [XmlAttribute("semantic")] public string Semantic;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "texture", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Texture
    {
        [XmlAttribute("texture")] public string Texture;
        [XmlAttribute("texcoord")] public string TexCoord;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

