using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType=true), XmlRoot(ElementName="pass", Namespace="http://www.collada.org/2005/11/COLLADASchema", IsNullable=true)]
	public partial class Grendgine_Collada_Pass_GLES : Grendgine_Collada_Pass
	{
	}

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_GLES", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Profile_GLES : Grendgine_Collada_Profile
    {
        [XmlAttribute("platform")] public string Platform;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique_GLES[] Technique;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_GLES : Grendgine_Collada_Effect_Technique
    {
        [XmlElement(ElementName = "annotate")] public Grendgine_Collada_Annotate[] Annotate;
        [XmlElement(ElementName = "pass")] public Grendgine_Collada_Pass_GLES[] Pass;
    }
}

