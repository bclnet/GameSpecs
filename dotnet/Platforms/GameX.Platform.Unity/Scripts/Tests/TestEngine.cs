using UnityEngine;
using SimpleEngine = System.Object;

namespace Tests
{
    public class TestEngine : AbstractTest
    {
        SimpleEngine Engine;
        GameObject PlayerPrefab;

        public TestEngine(UnityTest test) : base(test)
        {
            PlayerPrefab = GameObject.Find("Player00");
        }

        public override void Dispose()
        {
            base.Dispose();
            //Engine?.Dispose();
        }

        public override void Start()
        {
            //var assetUri = new Uri("http://192.168.1.3/ASSETS/Morrowind/Morrowind.bsa#Morrowind");
            //var dataUri = new Uri("http://192.168.1.3/ASSETS/Morrowind/Morrowind.esm#Morrowind");

            //var assetUri = new Uri("game:/Morrowind.bsa#Morrowind");
            //var dataUri = new Uri("game:/Morrowind.esm#Morrowind");

            ////var assetUri = new Uri("game:/Oblivion*#Oblivion");
            ////var dataUri = new Uri("game:/Oblivion.esm#Oblivion");

            //Engine = new SimpleEngine(TesEstateHandler.Handler, assetUri, dataUri);

            //// engine
            //Engine.SpawnPlayer(PlayerPrefab, new Vector3(-137.94f, 2.30f, -1037.6f)); // new Int3(-2, -9)

            // engine - oblivion
            //Engine.SpawnPlayer(PlayerPrefab, new Int3(0, 0, 60), new Vector3(0, 0, 0));
        }

        //public override void Update() => Engine?.Update();
    }
}