using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "articulated_system", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Articulated_System
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "kinematics")] public Grendgine_Collada_Kinematics Kinematics;
        [XmlElement(ElementName = "motion")] public Grendgine_Collada_Motion Motion;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "axis_info", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Axis_Info
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("axis")] public string Axis;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Bind
    {
        [XmlAttribute("symbol")] public string Symbol;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
        [XmlElement(ElementName = "float")] public float Float;
        [XmlElement(ElementName = "int")] public int Int;
        [XmlElement(ElementName = "bool")] public bool Bool;
        [XmlElement(ElementName = "SIDREF")] public string SIDREF;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "connect_param", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Connect_Param
    {
        [XmlAttribute("ref")] public string Ref;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "effector_info", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Effector_Info
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "bind")] public Grendgine_Collada_Bind[] Bind;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "setparam")] public Grendgine_Collada_Set_Param[] Set_Param;
        [XmlElement(ElementName = "speed")] public Grendgine_Collada_Common_Float2_Or_Param_Type Speed;
        [XmlElement(ElementName = "acceleration")] public Grendgine_Collada_Common_Float2_Or_Param_Type Acceleration;
        [XmlElement(ElementName = "deceleration")] public Grendgine_Collada_Common_Float2_Or_Param_Type Deceleration;
        [XmlElement(ElementName = "jerk")] public Grendgine_Collada_Common_Float2_Or_Param_Type Jerk;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "frame_object", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public class Grendgine_Collada_Frame_Object
    {
        [XmlAttribute("link")] public string Link;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "frame_origin", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public class Grendgine_Collada_Frame_Origin
    {
        [XmlAttribute("link")] public string Link;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "frame_tcp", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Frame_TCP
    {
        [XmlAttribute("link")] public string Link;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "frame_tip", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Frame_Tip
    {
        [XmlAttribute("link")] public string Link;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_articulated_system", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Articulated_System
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "bind")] public Grendgine_Collada_Bind[] Bind;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "setparam")] public Grendgine_Collada_Set_Param[] Set_Param;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "kinematics", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Kinematics
    {
        [XmlElement(ElementName = "instance_kinematics_model")] public Grendgine_Collada_Instance_Kinematics_Model[] Instance_Kinematics_Model;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Kinematics Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_articulated_systems", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Articulated_Systems
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "articulated_system")] public Grendgine_Collada_Articulated_System[] Articulated_System;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "motion", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Motion
    {
        [XmlElement(ElementName = "instance_articulated_system")] public Grendgine_Collada_Instance_Articulated_System Instance_Articulated_System;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Motion Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

