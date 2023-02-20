using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlRoot(ElementName = "bump")]
    public partial class Grendgine_Collada_BumpMap
    {
        [XmlElement(ElementName = "texture")] public Grendgine_Collada_Texture[] Textures { get; set; }
        public static implicit operator XmlElement(Grendgine_Collada_BumpMap bump)
        {
            var xs = new XmlSerializer(typeof(Grendgine_Collada_BumpMap));
            var ns = new XmlSerializerNamespaces();
            ns.Add(string.Empty, string.Empty);
            using var ms = new MemoryStream();
            xs.Serialize(ms, bump, ns);
            ms.Seek(0, SeekOrigin.Begin);
            var doc = new XmlDocument();
            doc.Load(ms);
            return doc.DocumentElement;
        }
        public static implicit operator Grendgine_Collada_BumpMap(XmlElement bump)
        {
            using var s = new MemoryStream(Encoding.UTF8.GetBytes(bump.OuterXml));
            return (Grendgine_Collada_BumpMap)new XmlSerializer(typeof(Grendgine_Collada_BumpMap)).Deserialize(s);
        }
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Extra
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("type")] public string Type;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique
    {
        [XmlAttribute("profile")] public string profile;
        [XmlAttribute("xmlns")] public string xmlns;
        [XmlAnyElement] public XmlElement[] Data;
        [XmlElement(ElementName = "bump")] public Grendgine_Collada_BumpMap[] Bump { get; set; }
        [XmlElement(ElementName = "user_properties")] public string UserProperties { get; set; }
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common
    {
    }
}
