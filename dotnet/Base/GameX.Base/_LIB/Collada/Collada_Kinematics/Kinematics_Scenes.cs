using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_joint_axis", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Bind_Joint_Axis
    {
        [XmlAttribute("target")] public string Target;
        [XmlElement(ElementName = "axis")] public Grendgine_Collada_Common_SIDREF_Or_Param_Type Axis;
        [XmlElement(ElementName = "value")] public Grendgine_Collada_Common_Float_Or_Param_Type Value;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Bind_Kinematics_Model
    {
        [XmlAttribute("node")] public string Node;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
        [XmlElement(ElementName = "SIDREF")] public string SIDREF;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Kinematics_Scene
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "setparam")] public Grendgine_Collada_Set_Param[] Set_Param;
        [XmlElement(ElementName = "bind_kinematics_model")] public Grendgine_Collada_Bind_Kinematics_Model[] Bind_Kenematics_Model;
        [XmlElement(ElementName = "bind_joint_axis")] public Grendgine_Collada_Bind_Joint_Axis[] Bind_Joint_Axis;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Kinematics_Scene
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "instance_kinematics_model")] public Grendgine_Collada_Instance_Kinematics_Model[] Instance_Kinematics_Model;
        [XmlElement(ElementName = "instance_articulated_system")] public Grendgine_Collada_Instance_Articulated_System[] Instance_Articulated_System;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_kinematics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Kinematics_Scene
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "kinematics_scene")] public Grendgine_Collada_Kinematics_Scene[] Kinematics_Scene;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

