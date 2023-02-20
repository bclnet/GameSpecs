using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Asset
    {
        [XmlElement(ElementName = "created")] public DateTime Created;
        [XmlElement(ElementName = "modified")] public DateTime Modified;
        [XmlElement(ElementName = "unit")] public Grendgine_Collada_Asset_Unit Unit;
        [XmlElement(ElementName = "up_axis"), DefaultValue("Y_UP")] public string Up_Axis;
        [XmlElement(ElementName = "contributor")] public Grendgine_Collada_Asset_Contributor[] Contributor;
        [XmlElement(ElementName = "keywords")] public string Keywords;
        [XmlElement(ElementName = "revision")] public string Revision;
        [XmlElement(ElementName = "subject")] public string Subject;
        [XmlElement(ElementName = "title")] public string Title;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
        [XmlElement(ElementName = "coverage")] public Grendgine_Collada_Asset_Coverage Coverage;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Asset_Contributor
    {
        [XmlElement(ElementName = "author")] public string Author;
        [XmlElement(ElementName = "author_email")] public string Author_Email;
        [XmlElement(ElementName = "author_website")] public string Author_Website;
        [XmlElement(ElementName = "authoring_tool")] public string Authoring_Tool;
        [XmlElement(ElementName = "comments")] public string Comments;
        [XmlElement(ElementName = "copyright")] public string Copyright;
        [XmlElement(ElementName = "source_data")] public string Source_Data;
    }

#pragma warning disable 0169
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Asset_Coverage
    {
        [XmlElement(ElementName = "geographic_location")] Grendgine_Collada_Geographic_Location Geographic_Location;
    }
#pragma warning restore 0169

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Asset_Unit
    {
        [XmlAttribute("meter")] public double Meter; //: [DefaultValue(1.0)] // Commented out to force it to write these values.
        [XmlAttribute("name")] public string Name; //: [DefaultValue("meter")]
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Geographic_Location
    {
        [XmlElement(ElementName = "longitude")] public float Longitude;
        [XmlElement(ElementName = "latitude")] public float Latitude;
        [XmlElement(ElementName = "altitude")] public Grendgine_Collada_Geographic_Location_Altitude Altitude;
    }
}

