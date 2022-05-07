using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Animation
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "animation")] public Grendgine_Collada_Animation[] Animation;
        [XmlElement(ElementName = "channel")] public Grendgine_Collada_Channel[] Channel;
        [XmlElement(ElementName = "source")] public Grendgine_Collada_Source[] Source;
        [XmlElement(ElementName = "sampler")] public Grendgine_Collada_Sampler[] Sampler;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Animation_Clip
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("start")] public double Start;
        [XmlAttribute("end")] public double End;
        [XmlElement(ElementName = "instance_animation")] public Grendgine_Collada_Instance_Animation[] Instance_Animation;
        [XmlElement(ElementName = "instance_formula")] public Grendgine_Collada_Instance_Formula[] Instance_Formula;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Channel
    {
        [XmlAttribute("source")] public string Source;
        [XmlAttribute("target")] public string Target;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Instance_Animation
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("url")] public string URL;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Animation_Clips
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "animation_clip")] public Grendgine_Collada_Animation_Clip[] Animation_Clip;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Library_Animations
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "animation")] public Grendgine_Collada_Animation[] Animation;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Sampler
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("pre_behavior"), DefaultValue(Grendgine_Collada_Sampler_Behavior.UNDEFINED)] public Grendgine_Collada_Sampler_Behavior Pre_Behavior;
        [XmlAttribute("post_behavior"), DefaultValue(Grendgine_Collada_Sampler_Behavior.UNDEFINED)] public Grendgine_Collada_Sampler_Behavior Post_Behavior;
        [XmlElement(ElementName = "input")] public Grendgine_Collada_Input_Unshared[] Input;
    }
}

