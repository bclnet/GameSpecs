using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Bool_Array_String
    {
        [XmlText()] public string Value_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_Bool_Or_Param_Type : Grendgine_Collada_Common_Param_Type
    {
        [XmlElement(ElementName = "bool")] public bool Bool;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_Float_Or_Param_Type : Grendgine_Collada_Common_Param_Type
    {
        [XmlText()] public string Value_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_Float2_Or_Param_Type
    {
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
        [XmlText()] public string Value_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_Int_Or_Param_Type : Grendgine_Collada_Common_Param_Type
    {
        [XmlText()] public string Value_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_Param_Type
    {
        [XmlElement(ElementName = "param")] public Grendgine_Collada_Param Param;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Common_SIDREF_Or_Param_Type : Grendgine_Collada_Common_Param_Type
    {
        [XmlText()] public string Value;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Float_Array_String
    {
        [XmlText()] public string Value_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Int_Array_String
    {
        [XmlText()] public string Value_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Bool
    {
        [XmlAttribute("sid")] public string sID;
        [XmlText()] public bool Value;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Float
    {
        [XmlAttribute("sid")] public string sID;
        [XmlText()] public float Value;
    }


    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Float_Array_String
    {
        [XmlAttribute("sid")] public string sID;
        [XmlText()] public string Value_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Int_Array_String
    {
        [XmlAttribute("sid")] public string sID;
        [XmlText()] public string Value_As_String;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Name_Float
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlText()] public float Value;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_SID_Name_String
    {
        [XmlAttribute("sid")] public string sID;
        [XmlAttribute("name")] public string Name;
        [XmlText()] public string Value;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_String_Array_String
    {
        [XmlText()] public string Value_Pre_Parse;
    }
}


