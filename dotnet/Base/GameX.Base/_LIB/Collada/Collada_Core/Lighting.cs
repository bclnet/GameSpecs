using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Ambient
    {
        [XmlElement(ElementName = "color")] public Grendgine_Collada_Color Color;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Color : Grendgine_Collada_SID_Float_Array_String
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Directional
    {
        [XmlElement(ElementName = "color")] public Grendgine_Collada_Color Color;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Instance_Light
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Lights
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "light")] public Grendgine_Collada_Light[] Light;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Light
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Light Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Point
    {
        [XmlElement(ElementName = "color")] public Grendgine_Collada_Color Color;
        [XmlElement(ElementName = "constant_attenuation"), DefaultValue(typeof(float), "1.0")] public Grendgine_Collada_SID_Float Constant_Attenuation;
        [XmlElement(ElementName = "linear_attenuation"), DefaultValue(typeof(float), "0.0")] public Grendgine_Collada_SID_Float Linear_Attenuation;
        [XmlElement(ElementName = "quadratic_attenuation")] public Grendgine_Collada_SID_Float Quadratic_Attenuation;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Spot
    {
        [XmlElement(ElementName = "color")] public Grendgine_Collada_Color Color;
        [XmlElement(ElementName = "constant_attenuation"), DefaultValue(typeof(float), "1.0")] public Grendgine_Collada_SID_Float Constant_Attenuation;
        [XmlElement(ElementName = "linear_attenuation"), DefaultValue(typeof(float), "0.0")] public Grendgine_Collada_SID_Float Linear_Attenuation;
        [XmlElement(ElementName = "quadratic_attenuation"), DefaultValue(typeof(float), "0.0")] public Grendgine_Collada_SID_Float Quadratic_Attenuation;
        [XmlElement(ElementName = "falloff_angle"), DefaultValue(typeof(float), "180.0")] public Grendgine_Collada_SID_Float Falloff_Angle;
        [XmlElement(ElementName = "falloff_exponent"), DefaultValue(typeof(float), "0.0")] public Grendgine_Collada_SID_Float Falloff_Exponent;
    }
}

