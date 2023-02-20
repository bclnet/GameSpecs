using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Constraint_Limit_Detail
    {
        [XmlElement(ElementName = "min")] public Grendgine_Collada_SID_Float_Array_String Min;
        [XmlElement(ElementName = "max")] public Grendgine_Collada_SID_Float_Array_String Max;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "limits", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Constraint_Limits
    {
        [XmlElement(ElementName = "swing_cone_and_twist")] public Grendgine_Collada_Constraint_Limit_Detail Swing_Cone_And_Twist;
        [XmlElement(ElementName = "linear")] public Grendgine_Collada_Constraint_Limit_Detail Linear;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "spring", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Constraint_Spring
    {
        [XmlElement(ElementName = "linear")] public Grendgine_Collada_Constraint_Spring_Type Linear;
        [XmlElement(ElementName = "angular")] public Grendgine_Collada_Constraint_Spring_Type Angular;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Constraint_Spring_Type
    {
        [XmlElement(ElementName = "stiffness")] public Grendgine_Collada_SID_Float Stiffness;
        [XmlElement(ElementName = "damping")] public Grendgine_Collada_SID_Float Damping;
        [XmlElement(ElementName = "target_value")] public Grendgine_Collada_SID_Float Target_Value;
    }

    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "mass_frame", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Mass_Frame
    {
        [XmlElement(ElementName = "rotate")] public Grendgine_Collada_Rotate[] Rotate;
        [XmlElement(ElementName = "translate")] public Grendgine_Collada_Translate[] Translate;
    }
}

