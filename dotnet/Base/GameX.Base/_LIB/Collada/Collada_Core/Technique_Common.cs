using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Formula : Grendgine_Collada_Technique_Common
    {
        [XmlAnyElement] public XmlElement[] Data;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Light : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "ambient")] public Grendgine_Collada_Ambient Ambient;
        [XmlElement(ElementName = "directional")] public Grendgine_Collada_Directional Directional;
        [XmlElement(ElementName = "point")] public Grendgine_Collada_Point Point;
        [XmlElement(ElementName = "spot")] public Grendgine_Collada_Spot Spot;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Optics : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "orthographic")] public Grendgine_Collada_Orthographic Orthographic;
        [XmlElement(ElementName = "perspective")] public Grendgine_Collada_Perspective Perspective;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Source : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "accessor")] public Grendgine_Collada_Accessor Accessor;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
    }
}