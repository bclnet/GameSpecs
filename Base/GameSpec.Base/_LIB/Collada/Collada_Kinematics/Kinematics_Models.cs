using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "attachment_end", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Attachment_End
    {
        [XmlAttribute("joint")] public string Joint;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "attachment_full", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Attachment_Full
    {
        [XmlAttribute("joint")] public string Joint;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
        [XmlElement(ElementName = "link")] public Grendgine_Collada_Link Link;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "attachment_start", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Attachment_Start
    {
        [XmlAttribute("joint")] public string Joint;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_joint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Joint
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Kinematics_Model
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "bind")] public Grendgine_Collada_Bind[] Bind;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "setparam")] public Grendgine_Collada_Set_Param[] Set_Param;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "kinematics_model", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Kinematics_Model
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Kinematics_Model Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_kinematics_models", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Kinematics_Models
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "kinematics_model")] public Grendgine_Collada_Kinematics_Model[] Kinematics_Model;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "link", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Link
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
        [XmlElement(ElementName = "attachment_full")] public Grendgine_Collada_Attachment_Full Attachment_Full;
        [XmlElement(ElementName = "attachment_end")] public Grendgine_Collada_Attachment_End Attachment_End;
        [XmlElement(ElementName = "attachment_start")] public Grendgine_Collada_Attachment_Start Attachment_Start;
    }
}

