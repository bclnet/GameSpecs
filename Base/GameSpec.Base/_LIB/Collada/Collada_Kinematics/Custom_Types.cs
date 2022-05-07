using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Axis_Info_Kinematics : Grendgine_Collada_Axis_Info
    {
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "active")] public Grendgine_Collada_Common_Bool_Or_Param_Type Active;
        [XmlElement(ElementName = "locked")] public Grendgine_Collada_Common_Bool_Or_Param_Type Locked;
        [XmlElement(ElementName = "index")] public Grendgine_Collada_Kinematics_Axis_Info_Index[] Index;
        [XmlElement(ElementName = "limits")] public Grendgine_Collada_Kinematics_Axis_Info_Limits Limits;
        [XmlElement(ElementName = "formula")] public Grendgine_Collada_Formula[] Formula;
        [XmlElement(ElementName = "instance_formula")] public Grendgine_Collada_Instance_Formula[] Instance_Formula;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Axis_Info_Motion : Grendgine_Collada_Axis_Info
    {
        [XmlElement(ElementName = "bind")] public Grendgine_Collada_Bind[] Bind;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "setparam")] public Grendgine_Collada_New_Param[] Set_Param;
        [XmlElement(ElementName = "speed")] public Grendgine_Collada_Common_Float_Or_Param_Type Speed;
        [XmlElement(ElementName = "acceleration")] public Grendgine_Collada_Common_Float_Or_Param_Type Acceleration;
        [XmlElement(ElementName = "deceleration")] public Grendgine_Collada_Common_Float_Or_Param_Type Deceleration;
        [XmlElement(ElementName = "jerk")] public Grendgine_Collada_Common_Float_Or_Param_Type Jerk;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "index", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Kinematics_Axis_Info_Index : Grendgine_Collada_Common_Int_Or_Param_Type
    {
        [XmlAttribute("semantic")] public string Semantic;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Kinematics_Axis_Info_Limits
    {
        [XmlElement(ElementName = "min")] public Grendgine_Collada_Common_Float_Or_Param_Type Min;
        [XmlElement(ElementName = "max")] public Grendgine_Collada_Common_Float_Or_Param_Type Max;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "limits", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Kinematics_Limits
    {
        [XmlElement(ElementName = "min")] public Grendgine_Collada_SID_Name_Float Min;
        [XmlElement(ElementName = "max")] public Grendgine_Collada_SID_Name_Float Max;
    }
}

