using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Accessor
    {
        [XmlAttribute("count")] public uint Count;
        [XmlAttribute("offset")] public uint Offset;
        [XmlAttribute("source")] public string Source;
        [XmlAttribute("stride")] public uint Stride;
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param[] Param;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Bool_Array : Grendgine_Collada_Bool_Array_String
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("count")] public int Count;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Float_Array : Grendgine_Collada_Float_Array_String
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("count")] public int Count;
        [XmlAttribute("digits"), DefaultValue(typeof(int), "6")] public int Digits;
        [XmlAttribute("magnitude"), DefaultValue(typeof(int), "38")] public int Magnitude;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_IDREF_Array : Grendgine_Collada_String_Array_String
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("count")] public int Count;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Input_Shared : Grendgine_Collada_Input_Unshared
    {
        [XmlAttribute("offset")] public int Offset;
        [XmlAttribute("set")] public int Set;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Input_Unshared
    {
        [XmlAttribute("semantic")] public Grendgine_Collada_Input_Semantic Semantic; //: Commenting out default value as it won't write. [DefaultValue(Grendgine_Collada_Input_Semantic.NORMAL)]
        [XmlAttribute("source")] public string source;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Int_Array : Grendgine_Collada_Int_Array_String
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("count")] public int Count;
        [XmlAttribute("minInclusive"), DefaultValue(typeof(int), "-2147483648")] public int Min_Inclusive;
        [XmlAttribute("maxInclusive"), DefaultValue(typeof(int), "2147483647")] public int Max_Inclusive;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Name_Array : Grendgine_Collada_String_Array_String
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("count")] public int Count;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SIDREF_Array : Grendgine_Collada_String_Array_String
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("count")] public int Count;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Source
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlElement(ElementName = "bool_array")] public Grendgine_Collada_Bool_Array Bool_Array;
        [XmlElement(ElementName = "float_array")] public Grendgine_Collada_Float_Array Float_Array;
        [XmlElement(ElementName = "IDREF_array")] public Grendgine_Collada_IDREF_Array IDREF_Array;
        [XmlElement(ElementName = "int_array")] public Grendgine_Collada_Int_Array Int_Array;
        [XmlElement(ElementName = "Name_array")] public Grendgine_Collada_Name_Array Name_Array;
        [XmlElement(ElementName = "SIDREF_array")] public Grendgine_Collada_SIDREF_Array SIDREF_Array;
        [XmlElement(ElementName = "token_array")] public Grendgine_Collada_Token_Array Token_Array;
        [XmlElement(ElementName = "technique_common")] public Grendgine_Collada_Technique_Common_Source Technique_Common;
        [XmlElement(ElementName = "technique")] public Grendgine_Collada_Technique[] Technique;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;
        // ggerber 1.4.1 compatibilitiy
        [XmlAttribute("source")] public string Source;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Token_Array : Grendgine_Collada_String_Array_String
    {
        [XmlAttribute("id")] public string ID;
        [XmlAttribute("name")] public string Name;
        [XmlAttribute("count")] public int Count;
    }
}

