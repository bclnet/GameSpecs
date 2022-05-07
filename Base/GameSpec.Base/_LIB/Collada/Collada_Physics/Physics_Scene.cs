using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "force_field", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Force_Field
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_force_field", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Force_Field
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_physics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Physics_Scene
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_force_fields", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Force_Fields
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "force_field")] public Grendgine_Collada_Force_Field[] Force_Field;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_physics_scenes", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Physics_Scenes
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "physics_scene")] public Grendgine_Collada_Physics_Scene[] Physics_Scene;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "physics_scene", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Physics_Scene
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "instance_force_field")] public Grendgine_Collada_Instance_Force_Field[] Instance_Force_Field;
        [XmlElement(ElementName = "instance_physics_model")] public Grendgine_Collada_Instance_Physics_Model[] Instance_Physics_Model;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Physics_Scene Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

