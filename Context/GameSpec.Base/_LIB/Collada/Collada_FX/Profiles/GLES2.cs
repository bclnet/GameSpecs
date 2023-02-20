using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "pass", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Pass_GLES2 : Grendgine_Collada_Pass
    {
        [XmlElement(ElementName = "program")] public Grendgine_Collada_Program_GLES2 Program;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "profile_GLES2", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Profile_GLES2 : Grendgine_Collada_Profile
    {
        [XmlAttribute("platform")] public string Platform;
        [XmlAttribute("language")] public string Language;
        [XmlElement(ElementName = "newparam")] public Grendgine_Collada_New_Param[] New_Param;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique_GLES2[] Technique;
        [XmlElement(ElementName = "code")] public Grendgine_Collada_Code[] Code;
        [XmlElement(ElementName = "include")] public Grendgine_Collada_Include[] Include;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "program", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Program_GLES2
    {
        [XmlElement(ElementName = "linker")] public Grendgine_Collada_Linker[] Linker;
        [XmlElement(ElementName = "shader")] public Grendgine_Collada_Shader_GLES2[] Shader;
        [XmlElement(ElementName = "bind_attribute")] public Grendgine_Collada_Bind_Attribute[] Bind_Attribute;
        [XmlElement(ElementName = "bind_uniform")] public Grendgine_Collada_Bind_Uniform[] Bind_Uniform;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shader", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Shader_GLES2 : Grendgine_Collada_Shader
    {
        [XmlElement(ElementName = "compiler")] public Grendgine_Collada_Compiler[] Compiler;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_GLES2 : Grendgine_Collada_Effect_Technique
    {
        [XmlElement(ElementName = "annotate")] public Grendgine_Collada_Annotate[] Annotate;
        [XmlElement(ElementName = "pass")] public Grendgine_Collada_Pass_GLES2[] Pass;
    }
}

