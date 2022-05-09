//using GameEstate.Graphics;
//using System;
//using UnityEngine;

//namespace GameEstate.Estates.Tes.Components
//{
//    public static class TestAsset
//    {
//        static Estate Estate = EstateManager.GetEstate("Tes");
//        static UnityPakFile PakFile = new UnityPakFile(Estate.OpenPakFile(new Uri("game:/Morrowind.bsa#Morrowind")));
//        //static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("http://192.168.1.3/ASSETS/Morrowind/Morrowind.bsa#Morrowind")));
//        //static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("game:/Skyrim*#SkyrimVR")));
//        //static TesUnityPakFile PakFile = new TesUnityPakFile(Estate.OpenPakFile(new Uri("game:/Fallout4*#Fallout4VR")));

//        public static void Awake() { }
//        public static void Start()
//        {
//            // Morrowind
//            //MakeObject("meshes/i/in_dae_room_l_floor_01.nif");
//            //MakeObject("meshes/w/w_arrow01.nif");

//            //MakeObject("meshes/x/ex_common_balcony_01.nif");
//            //MakeTexture("");

//            // Skyrim
//            //var nifFileLoadingTask = await Asset.LoadObjectInfoAsync("meshes/actors/alduin/alduin.nif");
//            //MakeObject("meshes/markerx.nif");
//            //MakeObject("meshes/w/w_arrow01.nif");
//            //MakeObject("meshes/x/ex_common_balcony_01.nif");
//        }
//        public static void OnDestroy() => PakFile.Dispose();
//        public static void Update() { }

//        static GameObject MakeObject(string path) => PakFile.CreateObject(path);
//        static GameObject MakeTexture(string path)
//        {
//            var materialManager = PakFile.MaterialManager;
//            var obj = GameObject.CreatePrimitive(PrimitiveType.Cube); // GameObject.Find("Cube"); // CreatePrimitive(PrimitiveType.Cube);
//            var meshRenderer = obj.GetComponent<MeshRenderer>();
//            var materialProps = new MaterialProps
//            {
//                Textures = new MaterialTextures { MainFilePath = path },
//            };
//            meshRenderer.material = materialManager.BuildMaterialFromProperties(materialProps);
//            return obj;
//        }
//        static void MakeCursor(string path) => Cursor.SetCursor(PakFile.LoadTexture(path), Vector2.zero, CursorMode.Auto);
//    }
//}
