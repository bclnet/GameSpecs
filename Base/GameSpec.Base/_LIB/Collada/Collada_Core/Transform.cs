using System;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Lookat : Grendgine_Collada_SID_Float_Array_String
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Matrix : Grendgine_Collada_SID_Float_Array_String
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Rotate : Grendgine_Collada_SID_Float_Array_String
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Scale : Grendgine_Collada_SID_Float_Array_String
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Skew : Grendgine_Collada_SID_Float_Array_String
    {
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Translate : Grendgine_Collada_SID_Float_Array_String
    {
    }
}

