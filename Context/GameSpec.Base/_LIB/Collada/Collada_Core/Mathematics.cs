using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Formula
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("sid")] public string sID;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Formula Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "target")] public Grendgine_Collada_Common_Float_Or_Param_Type Target;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Instance_Formula
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "setparam")] public Grendgine_Collada_Set_Param[] Set_Param;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Formulas
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "formula")] public Grendgine_Collada_Formula[] Formula;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

