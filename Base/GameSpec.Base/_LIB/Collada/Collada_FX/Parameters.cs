using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "array", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Array
    {
        [XmlAttribute("length")] public int Length;
        [XmlAttribute("resizable")] public bool Resizable;
        [XmlAnyElement] public XmlElement[] Data;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "modifier", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Modifier
    {
        [XmlText(), DefaultValue(Grendgine_Collada_Modifier_Value.CONST)] public Grendgine_Collada_Modifier_Value Value;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler_image", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Sampler_Image : Grendgine_Collada_Instance_Image
    {
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sampler_states", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Sampler_States : Grendgine_Collada_FX_Sampler_Common
    {
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "semantic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Semantic
    {
        [XmlText()] public string Value;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "usertype", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_UserType
    {
        [XmlAttribute("typename")] public string TypeName;
        [XmlAttribute("source")] public string Source;
        [XmlElement(ElementName = "setparam")] public Grendgine_Collada_Set_Param[] SetParam;
    }
}

