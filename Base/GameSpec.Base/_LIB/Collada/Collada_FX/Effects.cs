using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "annotate", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Annotate
    {
        [XmlAttribute("name")] public string Name;
        [XmlAnyElement] public XmlElement[] Data;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_vertex_input", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Bind_Vertex_Input
    {
        [XmlAttribute("semantic")] public string Semantic;
        [XmlAttribute("imput_semantic")] public string Imput_Semantic;
        [XmlAttribute("input_set")] public int Input_Set;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "effect", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Effect
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "annotate")] public Grendgine_Collada_Annotate[] Annotate;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "profile_BRIDGE")] public Grendgine_Collada_Profile_BRIDGE[] Profile_BRIDGE;
        [XmlElement(ElementName = "profile_CG")] public Grendgine_Collada_Profile_CG[] Profile_CG;
        [XmlElement(ElementName = "profile_GLES")] public Grendgine_Collada_Profile_GLES[] Profile_GLES;
        [XmlElement(ElementName = "profile_GLES2")] public Grendgine_Collada_Profile_GLES2[] Profile_GLES2;
        [XmlElement(ElementName = "profile_GLSL")] public Grendgine_Collada_Profile_GLSL[] Profile_GLSL;
        [XmlElement(ElementName = "profile_COMMON")] public Grendgine_Collada_Profile_COMMON[] Profile_COMMON;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Effect_Technique
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("id")] public string id;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_effect", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Effect
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "setparam")] public Grendgine_Collada_Set_Param[] Set_Param;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "technique_hint")] public Grendgine_Collada_Technique_Hint[] Technique_Hint;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_effects", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Effects
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "effect")] public Grendgine_Collada_Effect[] Effect;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_hint", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Hint
    {
        [XmlAttribute("platform")] public string Platform;
        [XmlAttribute("ref")] public string Ref;
        [XmlAttribute("profile")] public string Profile;
    }
}

