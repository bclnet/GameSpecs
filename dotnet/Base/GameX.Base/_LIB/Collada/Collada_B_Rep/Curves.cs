using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Circle
    {
        [XmlElement(ElementName = "radius")] public float Radius;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Curve
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "line")] public Grendgine_Collada_Line Line;
        [XmlElement(ElementName = "circle")] public Grendgine_Collada_Circle Circle;
        [XmlElement(ElementName = "ellipse")] public Grendgine_Collada_Ellipse Ellipse;
        [XmlElement(ElementName = "parabola")] public Grendgine_Collada_Parabola Parabola;
        [XmlElement(ElementName = "hyperbola")] public Grendgine_Collada_Hyperbola Hyperbola;
        [XmlElement(ElementName = "nurbs")] public Grendgine_Collada_Nurbs Nurbs;
        [XmlElement(ElementName = "orient")] public Grendgine_Collada_Orient[] Orient;
        [XmlElement(ElementName = "origin")] public Grendgine_Collada_Origin Origin;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Curves
    {
        [XmlElement(ElementName = "curve")] public Grendgine_Collada_Curve[] Curve;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Ellipse
    {
        [XmlElement(ElementName = "radius")] public Grendgine_Collada_Float_Array_String Radius;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Hyperbola
    {
        [XmlElement(ElementName = "radius")] public Grendgine_Collada_Float_Array_String Radius;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Line
    {
        [XmlElement(ElementName = "origin")] public Grendgine_Collada_Origin Origin;
        [XmlElement(ElementName = "direction")] public Grendgine_Collada_Float_Array_String Direction;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Nurbs
    {
        [XmlAttribute("degree")] public int Degree;
        [XmlAttribute("closed")] public bool Closed;
        [XmlElement(ElementName = "source")] public Grendgine_Collada_Source[] Source;
        [XmlElement(ElementName = "control_vertices")] public Grendgine_Collada_Control_Vertices Control_Vertices;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Parabola
    {
        [XmlElement(ElementName = "focal")] public float Focal;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Surface_Curves
    {
        [XmlElement(ElementName = "curve")] public Grendgine_Collada_Curve[] Curve;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }
}

