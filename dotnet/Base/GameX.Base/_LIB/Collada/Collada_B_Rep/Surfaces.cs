using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType=true)]
	public partial class Grendgine_Collada_Cone
	{
	    [XmlElement(ElementName = "radius")] public float Radius;
	    [XmlElement(ElementName = "angle")] public float Angle;
		[XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
	}

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Cylinder_B_Rep
    {
        [XmlElement(ElementName = "radius")] public Grendgine_Collada_Float_Array_String Radius;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Nurbs_Surface
    {
        [XmlAttribute("degree_u")] public int Degree_U;
        [XmlAttribute("closed_u")] public bool Closed_U;
        [XmlAttribute("degree_v")] public int Degree_V;
        [XmlAttribute("closed_v")] public bool Closed_V;
        [XmlElement(ElementName = "source")] public Grendgine_Collada_Source[] Source;
        [XmlElement(ElementName = "control_vertices")] public Grendgine_Collada_Control_Vertices Control_Vertices;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Surface
    {
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("sid")] public string sID;
        // ggerber 1.4.1 attribue
        [XmlAttribute("type")] public string Type;
        [XmlElement(ElementName = "cone")] public Grendgine_Collada_Cone Cone;
        [XmlElement(ElementName = "plane")] public Grendgine_Collada_Plane Plane;
        [XmlElement(ElementName = "cylinder")] public Grendgine_Collada_Cylinder_B_Rep Cylinder;
        [XmlElement(ElementName = "nurbs_surface")] public Grendgine_Collada_Nurbs_Surface Nurbs_Surface;
        [XmlElement(ElementName = "sphere")] public Grendgine_Collada_Sphere Sphere;
        [XmlElement(ElementName = "torus")] public Grendgine_Collada_Torus Torus;
        [XmlElement(ElementName = "swept_surface")] public Grendgine_Collada_Swept_Surface Swept_Surface;
        [XmlElement(ElementName = "orient")] public Grendgine_Collada_Orient[] Orient;
        [XmlElement(ElementName = "origin")] public Grendgine_Collada_Origin Origin;
        //ggerber 1.4.1
        [XmlElement(ElementName = "init_from")] public Grendgine_Collada_Init_From Init_From;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Surfaces
    {
        [XmlElement(ElementName = "surface")] public Grendgine_Collada_Surface[] Surface;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Swept_Surface
    {
        [XmlElement(ElementName = "curve")] public Grendgine_Collada_Curve Curve;
        [XmlElement(ElementName = "origin")] public Grendgine_Collada_Origin Origin;
        [XmlElement(ElementName = "direction")] public Grendgine_Collada_Float_Array_String Direction;
        [XmlElement(ElementName = "axis")] public Grendgine_Collada_Float_Array_String Axis;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Torus
    {
        [XmlElement(ElementName = "radius")] public Grendgine_Collada_Float_Array_String Radius;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

