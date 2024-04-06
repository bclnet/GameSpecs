using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "attachment", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Attachment
    {
        [XmlAttribute("rigid_body")] public string Rigid_Body;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_physics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Physics_Model
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlAttribute("parent")] public string Parent;
        [XmlElement(ElementName = "instance_force_field")] public Grendgine_Collada_Instance_Force_Field[] Instance_Force_Field;
        [XmlElement(ElementName = "instance_rigid_body")] public Grendgine_Collada_Instance_Rigid_Body[] Instance_Rigid_Body;
        [XmlElement(ElementName = "instance_rigid_constraint")] public Grendgine_Collada_Instance_Rigid_Constraint[] Instance_Rigid_Constraint;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_rigid_body", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Rigid_Body
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("body")] public string Body;
        [XmlAttribute("target")] public string Target;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Instance_Rigid_Body Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_rigid_constraint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Rigid_Constraint
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("constraint")] public string Constraint;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_physics_models", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Physics_Models
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "physics_model")] public Grendgine_Collada_Physics_Model[] Physics_Model;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "physics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Physics_Model
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "rigid_body")] public Grendgine_Collada_Rigid_Body[] Rigid_Body;
        [XmlElement(ElementName = "rigid_constraint")] public Grendgine_Collada_Rigid_Constraint[] Rigid_Constraint;
        [XmlElement(ElementName = "instance_physics_model")] public Grendgine_Collada_Instance_Physics_Model[] Instance_Physics_Model;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "ref_attachment", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Ref_Attachment
    {
        [XmlAttribute("rigid_body")] public string Rigid_Body;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "rigid_body", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Rigid_Body
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Rigid_Body Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "rigid_constraint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Rigid_Constraint
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "ref_attachment")] public Grendgine_Collada_Ref_Attachment Ref_Attachment;
        [XmlElement(ElementName = "attachment")] public Grendgine_Collada_Attachment Attachment;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Rigid_Constraint Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

