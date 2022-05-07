using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "binary", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Binary
    {
        [XmlElement(ElementName = "ref")] public string Ref;
        [XmlElement(ElementName = "hex")] public Grendgine_Collada_Hex Hex;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_attribute", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Bind_Attribute
    {
        [XmlAttribute("symbol")] public string Symbol;
        [XmlElement(ElementName = "semantic")] public Grendgine_Collada_Semantic Semantic;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_uniform", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Bind_Uniform
    {
        [XmlAttribute("symbol")] public string Symbol;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
        [XmlAnyElement] public XmlElement[] Data;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "code", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Code
    {
        [XmlAttribute("sid")] public string sID;
        [XmlText()] public string Value;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "compiler", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Compiler
    {
        [XmlAttribute("platform")] public string Platform;
        [XmlAttribute("target")] public string Target;
        [XmlAttribute("options")] public string Options;
        [XmlElement(ElementName = "binary")] public Grendgine_Collada_Binary Binary;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "include", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Include
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("url")] public string URL;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "linker", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Linker
    {
        [XmlAttribute("platform")] public string Platform;
        [XmlAttribute("target")] public string Target;
        [XmlAttribute("options")] public string Options;
        [XmlElement(ElementName = "binary")] public Grendgine_Collada_Binary[] Binary;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Shader
    {
        [XmlAttribute("stage"), DefaultValue(Grendgine_Collada_Shader_Stage.VERTEX)] public Grendgine_Collada_Shader_Stage Stage;
        [XmlElement(ElementName = "sources")] public Grendgine_Collada_Shader_Sources Sources;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sources", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Shader_Sources
    {
        [XmlAttribute("entry")] public string Entry;
        [XmlElement(ElementName = "inline")] public string[] Inline;
        [XmlElement(ElementName = "import")] public Grendgine_Collada_Ref_String[] Import;
    }
}

