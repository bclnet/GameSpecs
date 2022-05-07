using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Camera
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "optics")] public Grendgine_Collada_Optics Optics;
        [XmlElement(ElementName = "imager")] public Grendgine_Collada_Imager Imager;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Imager
    {
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Instance_Camera
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Cameras
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "camera")] public Grendgine_Collada_Camera[] Camera;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Optics
    {
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Optics Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Orthographic
    {
        [XmlElement(ElementName = "xmag")] public Grendgine_Collada_SID_Float XMag;
        [XmlElement(ElementName = "ymag")] public Grendgine_Collada_SID_Float YMag;
        [XmlElement(ElementName = "aspect_ratio")] public Grendgine_Collada_SID_Float Aspect_Ratio;
        [XmlElement(ElementName = "znear")] public Grendgine_Collada_SID_Float ZNear;
        [XmlElement(ElementName = "zfar")] public Grendgine_Collada_SID_Float ZFar;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Perspective
    {
        [XmlElement(ElementName = "xfov")] public Grendgine_Collada_SID_Float XFov;
        [XmlElement(ElementName = "yfov")] public Grendgine_Collada_SID_Float YFov;
        [XmlElement(ElementName = "aspect_ratio")] public Grendgine_Collada_SID_Float Aspect_Ratio;
        [XmlElement(ElementName = "znear")] public Grendgine_Collada_SID_Float ZNear;
        [XmlElement(ElementName = "zfar")] public Grendgine_Collada_SID_Float ZFar;
    }
}

