using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Geographic_Location_Altitude
    {
        [XmlText()] public float Altitude;
        [XmlAttribute("mode"), DefaultValue(Grendgine_Collada_Geographic_Location_Altitude_Mode.relativeToGround)] public Grendgine_Collada_Geographic_Location_Altitude_Mode Mode;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Poly_PH
    {
        [XmlElement(ElementName = "p")] public Grendgine_Collada_Int_Array_String P;
        [XmlElement(ElementName = "h")] public Grendgine_Collada_Int_Array_String[] H;
    }
}

