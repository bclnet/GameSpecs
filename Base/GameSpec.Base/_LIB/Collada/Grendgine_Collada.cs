using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using static OpenStack.Debug;

namespace grendgine_collada
{
    //[XmlRoot(ElementName = "COLLADA", Namespace = "https://www.khronos.org/files/collada_schema_1_5", IsNullable = false)]
    //[XmlRoot(ElementName = "COLLADA", Namespace = "http://www.khronos.org/files/collada_schema_1_4", IsNullable = false)]
    [Serializable, DebuggerStepThrough(), DesignerCategory("code"), XmlType(AnonymousType = true), XmlRoot(ElementName = "COLLADA", Namespace = "http://www.collada.org/2005/11/COLLADASchema", IsNullable = false)]
    public partial class Grendgine_Collada
    {
        [XmlAttribute("version")] public string Collada_Version;
        [XmlElement(ElementName = "asset")] public Grendgine_Collada_Asset Asset;

        // FX Elements
        [XmlElement(ElementName = "library_images")] public Grendgine_Collada_Library_Images Library_Images;
        [XmlElement(ElementName = "library_effects")] public Grendgine_Collada_Library_Effects Library_Effects;
        [XmlElement(ElementName = "library_materials")] public Grendgine_Collada_Library_Materials Library_Materials;

        // Core Elements
        [XmlElement(ElementName = "library_animations")] public Grendgine_Collada_Library_Animations Library_Animations;
        [XmlElement(ElementName = "library_animation_clips")] public Grendgine_Collada_Library_Animation_Clips Library_Animation_Clips;
        [XmlElement(ElementName = "library_cameras")] public Grendgine_Collada_Library_Cameras Library_Cameras;
        [XmlElement(ElementName = "library_controllers")] public Grendgine_Collada_Library_Controllers Library_Controllers;
        [XmlElement(ElementName = "library_formulas")] public Grendgine_Collada_Library_Formulas Library_Formulas;
        [XmlElement(ElementName = "library_geometries")] public Grendgine_Collada_Library_Geometries Library_Geometries;
        [XmlElement(ElementName = "library_lights")] public Grendgine_Collada_Library_Lights Library_Lights;
        [XmlElement(ElementName = "library_nodes")] public Grendgine_Collada_Library_Nodes Library_Nodes;
        [XmlElement(ElementName = "library_visual_scenes")] public Grendgine_Collada_Library_Visual_Scenes Library_Visual_Scene;

        // Physics Elements
        [XmlElement(ElementName = "library_force_fields")] public Grendgine_Collada_Library_Force_Fields Library_Force_Fields;
        [XmlElement(ElementName = "library_physics_materials")] public Grendgine_Collada_Library_Physics_Materials Library_Physics_Materials;
        [XmlElement(ElementName = "library_physics_models")] public Grendgine_Collada_Library_Physics_Models Library_Physics_Models;
        [XmlElement(ElementName = "library_physics_scenes")] public Grendgine_Collada_Library_Physics_Scenes Library_Physics_Scenes;

        // Kinematics
        [XmlElement(ElementName = "library_articulated_systems")] public Grendgine_Collada_Library_Articulated_Systems Library_Articulated_Systems;
        [XmlElement(ElementName = "library_joints")] public Grendgine_Collada_Library_Joints Library_Joints;
        [XmlElement(ElementName = "library_kinematics_models")] public Grendgine_Collada_Library_Kinematics_Models Library_Kinematics_Models;
        [XmlElement(ElementName = "library_kinematics_scenes")] public Grendgine_Collada_Library_Kinematics_Scene Library_Kinematics_Scene;
        [XmlElement(ElementName = "scene")] public Grendgine_Collada_Scene Scene;
        [XmlElement(ElementName = "extra")] public Grendgine_Collada_Extra[] Extra;

        public Grendgine_Collada()
        {
            Collada_Version = "1.5";
            Asset = new Grendgine_Collada_Asset
            {
                Title = "Test Engine 1"
            };
            Library_Visual_Scene = new Grendgine_Collada_Library_Visual_Scenes();
        }

        public static Grendgine_Collada Grendgine_Load_File(string file_name)
        {
            try
            {
                using (var tr = new StreamReader(file_name))
                    return (Grendgine_Collada)new XmlSerializer(typeof(Grendgine_Collada)).Deserialize(tr);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
                Console.ReadLine();
                return null;
            }
        }

        //public async static void Grendgine_Save_File(Grendgine_Collada data)
        //{
        //    var savePicker = new FileSavePicker
        //    {
        //        SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        //        SuggestedFileName = "Test",
        //    };
        //    savePicker.FileTypeChoices.Add("DAE", new List<string>() { ".dae" });
        //    savePicker.FileTypeChoices.Add("Xml", new List<string>() { ".xml" });
        //    var file = await savePicker.PickSaveFileAsync();
        //    if (file != null)
        //    {
        //        var sessionRandomAccess = await file.OpenAsync(FileAccessMode.ReadWrite);
        //        var sessionOutputStream = sessionRandomAccess.GetOutputStreamAt(0);
        //        try
        //        {
        //            var serializer = new XmlSerializer(typeof(Grendgine_Collada));
        //            serializer.Serialize(sessionOutputStream.AsStreamForWrite(), data);
        //        }
        //        catch (InvalidOperationException e) { Debug.WriteLine(e.InnerException); }
        //        sessionRandomAccess.Dispose();
        //        await sessionOutputStream.FlushAsync();
        //        sessionOutputStream.Dispose();
        //    }
        //}
    }
}