using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_New_Param
    {
        [XmlAttribute("sid")] public string sID;
        [XmlElement(ElementName = "semantic")] public string Semantic;
        [XmlElement(ElementName = "modifier")] public string Modifier;
        [XmlElement("annotate")] public Grendgine_Collada_Annotate[] Annotate;
        [XmlAnyElement] public XmlElement[] Data;
        // ggerber 1.4.1 elements.  Surface and Sampler2D are single elements for textures.
        [XmlElement("surface")] public Grendgine_Collada_Surface Surface;
        [XmlElement("sampler2D")] public Grendgine_Collada_Sampler2D Sampler2D;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Param
    {
        [XmlAttribute("ref")] public string Ref;
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("semantic")] public string Semantic;
        [XmlAttribute("type")] public string Type;
        [XmlAnyElement] public XmlElement[] Data;
        //TODO: this is used in a few contexts
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Set_Param
    {
        [XmlAttribute("ref")] public string Ref;
        [XmlAnyElement] public XmlElement[] Data;
    }
}

