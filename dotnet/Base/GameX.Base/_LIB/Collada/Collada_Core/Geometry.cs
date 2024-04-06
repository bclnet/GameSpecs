using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Control_Vertices
    {
        [XmlElement(ElementName = "input")] public Grendgine_Collada_Input_Unshared[] Input;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Geometry
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "brep")] public Grendgine_Collada_B_Rep B_Rep;
        [XmlElement(ElementName = "convex_mesh")] public Grendgine_Collada_Convex_Mesh Convex_Mesh;
        [XmlElement(ElementName = "spline")] public Grendgine_Collada_Spline Spline;
        [XmlElement(ElementName = "mesh")] public Grendgine_Collada_Mesh Mesh;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Geometry_Common_Fields
    {
        [XmlAttribute("count")] public int Count;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("material")] public string Material;
        [XmlElement(ElementName = "input")] public Grendgine_Collada_Input_Shared[] Input;
        [XmlElement(ElementName = "p")] public Grendgine_Collada_Int_Array_String P;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Instance_Geometry
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "bind_material")] public Grendgine_Collada_Bind_Material[] Bind_Material;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Geometries
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "geometry")] public Grendgine_Collada_Geometry[] Geometry;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Lines : Grendgine_Collada_Geometry_Common_Fields
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Linestrips : Grendgine_Collada_Geometry_Common_Fields
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Mesh
    {
        [XmlElement(ElementName = "source")] public Grendgine_Collada_Source[] Source;
        [XmlElement(ElementName = "vertices")] public Grendgine_Collada_Vertices Vertices;
        [XmlElement(ElementName = "lines")] public Grendgine_Collada_Lines[] Lines;
        [XmlElement(ElementName = "linestrips")] public Grendgine_Collada_Linestrips[] Linestrips;
        [XmlElement(ElementName = "polygons")] public Grendgine_Collada_Polygons[] Polygons;
        [XmlElement(ElementName = "polylist")] public Grendgine_Collada_Polylist[] Polylist;
        [XmlElement(ElementName = "triangles")] public Grendgine_Collada_Triangles[] Triangles;
        [XmlElement(ElementName = "trifans")] public Grendgine_Collada_Trifans[] Trifans;
        [XmlElement(ElementName = "tristrips")] public Grendgine_Collada_Tristrips[] Tristrips;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Polygons : Grendgine_Collada_Geometry_Common_Fields
    {
        [XmlElement(ElementName = "ph")] public Grendgine_Collada_Poly_PH[] PH;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Polylist : Grendgine_Collada_Geometry_Common_Fields
    {
        [XmlElement(ElementName = "vcount")] public Grendgine_Collada_Int_Array_String VCount;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Spline
    {
        [XmlAttribute("closed")] public bool Closed;
        [XmlElement(ElementName = "source")] public Grendgine_Collada_Source[] Source;
        [XmlElement(ElementName = "control_vertices")] public Grendgine_Collada_Control_Vertices Control_Vertices;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Triangles : Grendgine_Collada_Geometry_Common_Fields
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Trifans : Grendgine_Collada_Geometry_Common_Fields
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Tristrips : Grendgine_Collada_Geometry_Common_Fields
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Vertices
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "input")] public Grendgine_Collada_Input_Unshared[] Input;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

