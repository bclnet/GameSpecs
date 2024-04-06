using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Pass_GLSL : Grendgine_Collada_Pass
    {
        [XmlElement(ElementName = "program")] public Grendgine_Collada_Program_GLSL Program;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_GLSL", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Profile_GLSL : Grendgine_Collada_Profile
    {
        [XmlAttribute("platform")] public string Platform;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique_GLSL[] Technique;
        [XmlElement(ElementName = "code")] public Grendgine_Collada_Code[] Code;
        [XmlElement(ElementName = "include")] public Grendgine_Collada_Include[] Include;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Program_GLSL
    {
        [XmlElement(ElementName = "shader")] public Grendgine_Collada_Shader_GLSL[] Shader;
        [XmlElement(ElementName = "bind_attribute")] public Grendgine_Collada_Bind_Attribute[] Bind_Attribute;
        [XmlElement(ElementName = "bind_uniform")] public Grendgine_Collada_Bind_Uniform[] Bind_Uniform;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Shader_GLSL : Grendgine_Collada_Shader
    {
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_GLSL : Grendgine_Collada_Effect_Technique
    {
        [XmlElement(ElementName = "annotate")] public Grendgine_Collada_Annotate[] Annotate;
        [XmlElement(ElementName = "pass")] public Grendgine_Collada_Pass_GLSL[] Pass;
    }
}

