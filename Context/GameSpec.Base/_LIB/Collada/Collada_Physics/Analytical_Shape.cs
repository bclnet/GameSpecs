using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "box", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Box
    {
        [XmlElement(ElementName = "half_extents")] public Grendgine_Collada_Float_Array_String Half_Extents;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "capsule", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Capsule
    {
        [XmlElement(ElementName = "height")] public float Height;
        [XmlElement(ElementName = "radius")] public Grendgine_Collada_Float_Array_String Radius;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "convex_mesh", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Convex_Mesh
    {
        [XmlAttribute("convex_hull_of")] public string Convex_Hull_Of;
        [XmlElement(ElementName = "source")] public Grendgine_Collada_Source[] Source;
        [XmlElement(ElementName = "lines")] public Grendgine_Collada_Lines[] Lines;
        [XmlElement(ElementName = "linestrips")] public Grendgine_Collada_Linestrips[] Linestrips;
        [XmlElement(ElementName = "polygons")] public Grendgine_Collada_Polygons[] Polygons;
        [XmlElement(ElementName = "polylist")] public Grendgine_Collada_Polylist[] Polylist;
        [XmlElement(ElementName = "triangles")] public Grendgine_Collada_Triangles[] Triangles;
        [XmlElement(ElementName = "trifans")] public Grendgine_Collada_Trifans[] Trifans;
        [XmlElement(ElementName = "tristrips")] public Grendgine_Collada_Tristrips[] Tristrips;
        [XmlElement(ElementName = "vertices")] public Grendgine_Collada_Vertices Vertices;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "cylinder", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Cylinder
    {
        [XmlElement(ElementName = "height")] public float Height;
        [XmlElement(ElementName = "radius")] public Grendgine_Collada_Float_Array_String Radius;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "plane", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Plane
    {
        [XmlElement(ElementName = "equation")] public Grendgine_Collada_Float_Array_String Equation;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "shape", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Shape
    {
        [XmlElement(ElementName = "hollow")] public Grendgine_Collada_SID_Bool Hollow;
        [XmlElement(ElementName = "mass")] public Grendgine_Collada_SID_Float Mass;
        [XmlElement(ElementName = "density")] public Grendgine_Collada_SID_Float Density;
        [XmlElement(ElementName = "physics_material")] public Grendgine_Collada_Physics_Material Physics_Material;
        [XmlElement(ElementName = "instance_physics_material")] public Grendgine_Collada_Instance_Physics_Material Instance_Physics_Material;
        [XmlElement(ElementName = "instance_geometry")] public Grendgine_Collada_Instance_Geometry Instance_Geometry;
        [XmlElement(ElementName = "plane")] public Grendgine_Collada_Plane Plane;
        [XmlElement(ElementName = "box")] public Grendgine_Collada_Box Box;
        [XmlElement(ElementName = "sphere")] public Grendgine_Collada_Sphere Sphere;
        [XmlElement(ElementName = "cylinder")] public Grendgine_Collada_Cylinder Cylinder;
        [XmlElement(ElementName = "capsule")] public Grendgine_Collada_Capsule Capsule;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "sphere", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Sphere
    {
        [XmlElement(ElementName = "radius")] public float Radius;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

