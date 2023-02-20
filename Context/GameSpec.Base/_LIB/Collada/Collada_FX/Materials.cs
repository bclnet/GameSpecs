using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Materials
    {
        [XmlAttribute("semantic")] public string Semantic;
        [XmlAttribute("target")] public string Target;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "bind_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Bind_Material
    {
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param[] Param;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Bind_Material Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "instance_material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Instance_Material_Geometry
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("target")] public string Target;
        [XmlAttribute("symbol")] public string Symbol;
        [XmlElement(ElementName = "bind")] public Materials[] Bind;
        [XmlElement(ElementName = "bind_vertex_input")] public Grendgine_Collada_Bind_Vertex_Input[] Bind_Vertex_Input;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "library_materials", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Library_Materials
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "material")] public Grendgine_Collada_Material[] Material;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "material", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Material
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "instance_effect")] public Grendgine_Collada_Instance_Effect Instance_Effect;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

