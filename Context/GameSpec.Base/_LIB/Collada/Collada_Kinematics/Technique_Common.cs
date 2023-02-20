using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Common_Kinematics : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "axis_info")] public Grendgine_Collada_Axis_Info_Kinematics[] Axis_Info;
        [XmlElement(ElementName = "frame_origin")] public Grendgine_Collada_Frame_Origin Frame_Origin;
        [XmlElement(ElementName = "frame_tip")] public Grendgine_Collada_Frame_Tip Frame_Tip;
        [XmlElement(ElementName = "frame_tcp")] public Grendgine_Collada_Frame_TCP Frame_TCP;
        [XmlElement(ElementName = "frame_object")] public Grendgine_Collada_Frame_Object Frame_Object;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Common_Kinematics_Model : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "joint")] public Grendgine_Collada_Joint[] Joint;
        [XmlElement(ElementName = "instance_joint")] public Grendgine_Collada_Instance_Joint[] Instance_Joint;
        [XmlElement(ElementName = "link")] public Grendgine_Collada_Link[] Link;
        [XmlElement(ElementName = "formula")] public Grendgine_Collada_Formula[] Formula;
        [XmlElement(ElementName = "instance_formula")] public Grendgine_Collada_Instance_Formula[] Instance_Formula;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Common_Motion : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "axis_info")] public Grendgine_Collada_Axis_Info_Motion[] Axis_Info;
        [XmlElement(ElementName = "effector_info")] public Grendgine_Collada_Effector_Info Effector_Info;
    }
}

