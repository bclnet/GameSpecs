using System;
using System.Xml;
using System.Xml.Serialization;

namespace grendgine_collada
{
    [Serializable, XmlType(AnonymousType = true), XmlRoot(ElementName = "technique_common", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = true)]
    public partial class Grendgine_Collada_Technique_Common_Instance_Rigid_Body : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "angular_velocity")] public Grendgine_Collada_Float_Array_String Angular_Velocity;
        [XmlElement(ElementName = "velocity")] public Grendgine_Collada_Float_Array_String Velocity;
        [XmlElement(ElementName = "dynamic")] public Grendgine_Collada_SID_Bool Dynamic;
        [XmlElement(ElementName = "mass")] public Grendgine_Collada_SID_Float Mass;
        [XmlElement(ElementName = "inertia")] public Grendgine_Collada_SID_Float_Array_String Inertia;
        [XmlElement(ElementName = "mass_frame")] public Grendgine_Collada_Mass_Frame Mass_Frame;
        [XmlElement(ElementName = "physics_material")] public Grendgine_Collada_Physics_Material Physics_Material;
        [XmlElement(ElementName = "instance_physics_material")] public Grendgine_Collada_Instance_Physics_Material Instance_Physics_Material;
        [XmlElement(ElementName = "shape")] public Grendgine_Collada_Shape[] Shape;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Physics_Material : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "dynamic_friction")] public Grendgine_Collada_SID_Float Dynamic_Friction;
        [XmlElement(ElementName = "restitution")] public Grendgine_Collada_SID_Float Restitution;
        [XmlElement(ElementName = "static_friction")] public Grendgine_Collada_SID_Float Static_Friction;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Physics_Scene : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "gravity")] public Grendgine_Collada_SID_Float_Array_String Gravity;
        [XmlElement(ElementName = "time_step")] public Grendgine_Collada_SID_Float Time_Step;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Rigid_Body : Grendgine_Collada_Technique_Common
    {
        [XmlElement(ElementName = "dynamic")] public Grendgine_Collada_SID_Bool Dynamic;
        [XmlElement(ElementName = "mass")] public Grendgine_Collada_SID_Float Mass;
        [XmlElement(ElementName = "inertia")] public Grendgine_Collada_SID_Float_Array_String Inertia;
        [XmlElement(ElementName = "mass_frame")] public Grendgine_Collada_Mass_Frame Mass_Frame;
        [XmlElement(ElementName = "physics_material")] public Grendgine_Collada_Physics_Material Physics_Material;
        [XmlElement(ElementName = "instance_physics_material")] public Grendgine_Collada_Instance_Physics_Material Instance_Physics_Material;
        [XmlElement(ElementName = "shape")] public Grendgine_Collada_Shape[] Shape;
    }

    [Serializable, XmlType(AnonymousType = true)]
    public partial class Grendgine_Collada_Technique_Common_Rigid_Constraint : Grendgine_Collada_Technique_Common
    {

        [XmlElement(ElementName = "enabled")] public Grendgine_Collada_SID_Bool Enabled;
        [XmlElement(ElementName = "interpenetrate")] public Grendgine_Collada_SID_Bool Interpenetrate;
        [XmlElement(ElementName = "limits")] public Grendgine_Collada_Constraint_Limits Limits;
        [XmlElement(ElementName = "spring")] public Grendgine_Collada_Constraint_Spring Spring;
    }
}

