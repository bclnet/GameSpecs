using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Pass_CG : Grendgine_Collada_Pass
    {
        [XmlElement(ElementName = "program")] public Grendgine_Collada_Program_CG Program;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_CG", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Profile_CG : Grendgine_Collada_Profile
    {
        [XmlAttribute("platform")] public string Platform;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique_CG[] Technique;
        [XmlElement(ElementName = "code")] public Grendgine_Collada_Code[] Code;
        [XmlElement(ElementName = "include")] public Grendgine_Collada_Include[] Include;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Program_CG
    {
        [XmlElement(ElementName = "shader")] public Grendgine_Collada_Shader_CG[] Shader;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Shader_CG : Grendgine_Collada_Shader
    {
        [XmlElement(ElementName = "bind_uniform")] public Grendgine_Collada_Bind_Uniform[] Bind_Uniform;
        [XmlElement(ElementName = "compiler")] public Grendgine_Collada_Compiler[] Compiler;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_CG : Grendgine_Collada_Effect_Technique
    {
        [XmlElement(ElementName = "annotate")] public Grendgine_Collada_Annotate[] Annotate;
        [XmlElement(ElementName = "pass")] public Grendgine_Collada_Pass_CG[] Pass;
    }
}

