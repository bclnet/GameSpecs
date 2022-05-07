using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Controller
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "skin")] public Grendgine_Collada_Skin Skin;
        [XmlElement(ElementName = "morph")] public Grendgine_Collada_Morph Morph;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Instance_Controller
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "bind_material")] public Grendgine_Collada_Bind_Material[] Bind_Material;
        [XmlElement(ElementName = "skeleton")] public Grendgine_Collada_Skeleton[] Skeleton;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Joints
    {
        [XmlElement(ElementName = "input")] public Grendgine_Collada_Input_Unshared[] Input;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Controllers
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "controller")] public Grendgine_Collada_Controller[] Controller;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Morph
    {
        [XmlAttribute("source")] public string Source_Attribute;
        [XmlAttribute("method")] public string Method;
        [XmlArray("targets")] public Grendgine_Collada_Input_Shared[] Targets;
        [XmlElement(ElementName = "source")] public Grendgine_Collada_Source[] Source;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Skeleton
    {
        [XmlText()] public string Value;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Skin
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("source")] public string source;
        [XmlElement(ElementName = "bind_shape_matrix")] public Grendgine_Collada_Float_Array_String Bind_Shape_Matrix;
        [XmlElement(ElementName = "source")] public Grendgine_Collada_Source[] Source;
        [XmlElement(ElementName = "joints")] public Grendgine_Collada_Joints Joints;
        [XmlElement(ElementName = "vertex_weights")] public Grendgine_Collada_Vertex_Weights Vertex_Weights;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Targets
    {
        [XmlElement(ElementName = "input")] public Grendgine_Collada_Input_Unshared[] Input;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Vertex_Weights
    {
        [XmlAttribute("count")] public int Count;
        [XmlElement(ElementName = "input")] public Grendgine_Collada_Input_Shared[] Input;
        [XmlElement(ElementName = "vcount")] public Grendgine_Collada_Int_Array_String VCount;
        [XmlElement(ElementName = "v")] public Grendgine_Collada_Int_Array_String V;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

