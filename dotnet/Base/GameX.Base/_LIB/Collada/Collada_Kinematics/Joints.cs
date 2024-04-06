using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "joint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Joint
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("sid")] public string sID;
        [XmlElement(ElementName = "prismatic")] public Grendgine_Collada_Prismatic[] Prismatic;
        [XmlElement(ElementName = "revolute")] public Grendgine_Collada_Revolute[] Revolute;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_joints", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Joints
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "joint")] public Grendgine_Collada_Joint[] Joint;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "prismatic", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Prismatic
    {
        [XmlAttribute("sid")] public string sID;
        [XmlElement(ElementName = "axis")] public Grendgine_Collada_SID_Float_Array_String Axis;
        [XmlElement(ElementName = "limits")] public Grendgine_Collada_Kinematics_Limits Limits;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "revolute", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Revolute
    {
        [XmlAttribute("sid")] public string sID;
        [XmlElement(ElementName = "axis")] public Grendgine_Collada_SID_Float_Array_String Axis;
        [XmlElement(ElementName = "limits")] public Grendgine_Collada_Kinematics_Limits Limits;
    }
}

