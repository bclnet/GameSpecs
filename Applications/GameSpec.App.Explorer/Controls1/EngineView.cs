//using GameEstate.Graphics.Controls;

//namespace GameEstate.Explorer.Views
//{
//    public class EngineView : GLViewerControl
//    {
//        public Player Player;

//        public static EngineView Instance;
//        public static MainWindow Window => MainWindow.Instance;

//        //public static WorldViewer WorldViewer;
//        //public static MapViewer MapViewer;
//        //public static ModelViewer ModelViewer;
//        //public static TextureViewer TextureViewer;
//        //public static ParticleViewer ParticleViewer;

//        //private static ViewMode _viewMode;

//        //public static ViewMode ViewMode
//        //{
//        //    get => _viewMode;
//        //    set
//        //    {
//        //        if (_viewMode != value)
//        //        {
//        //            _viewMode = value;
//        //            if (_viewMode == ViewMode.Model)
//        //            {
//        //                Camera.Position = new Vector3(-10, -10, 10);
//        //                Camera.Dir = Vector3.Normalize(-Camera.Position);
//        //                Camera.Speed = Camera.Model_Speed;
//        //                Camera.SetNearPlane(Camera.NearPlane_Model);
//        //            }
//        //            if (_viewMode == ViewMode.Particle)
//        //            {
//        //                Camera.InitParticle();
//        //            }
//        //            if (_viewMode == ViewMode.World)
//        //            {
//        //                Camera.SetNearPlane(Camera.NearPlane_World);
//        //            }
//        //        }
//        //    }
//        //}

//        //public void PostInit()
//        //{
//        //    InitPlayer();

//        //    Render = new Render.Render();

//        //    WorldViewer = new WorldViewer();
//        //    MapViewer = new MapViewer();
//        //    ModelViewer = new ModelViewer();
//        //    TextureViewer = new TextureViewer();
//        //    ParticleViewer = new ParticleViewer();
//        //}

//        //public void InitPlayer()
//        //{
//        //    Player = new Player();
//        //}

//        //protected override void Update(GameTime time)
//        //{
//        //    base.Update(time);

//        //    //if (Player != null)
//        //    //    Player.Update(time);

//        //    //switch (ViewMode)
//        //    //{
//        //    //    case ViewMode.Texture:
//        //    //        TextureViewer.Update(time);
//        //    //        break;
//        //    //    case ViewMode.Model:
//        //    //        ModelViewer.Update(time);
//        //    //        break;
//        //    //    case ViewMode.World:
//        //    //        WorldViewer.Update(time);
//        //    //        break;
//        //    //    case ViewMode.Map:
//        //    //        MapViewer.Update(time);
//        //    //        break;
//        //    //    case ViewMode.Particle:
//        //    //        ParticleViewer.Update(time);
//        //    //        break;
//        //    //}
//        //}

//        //protected override void Draw(GameTime time)
//        //{
//        //    base.Draw(time);

//        //    //switch (ViewMode)
//        //    //{
//        //    //    case ViewMode.Texture:
//        //    //        TextureViewer.Draw(time);
//        //    //        break;
//        //    //    case ViewMode.Model:
//        //    //        ModelViewer.Draw(time);
//        //    //        break;
//        //    //    case ViewMode.World:
//        //    //        WorldViewer.Draw(time);
//        //    //        break;
//        //    //    case ViewMode.Map:
//        //    //        MapViewer.Draw(time);
//        //    //        break;
//        //    //    case ViewMode.Particle:
//        //    //        ParticleViewer.Draw(time);
//        //    //        break;
//        //    //}
//        //}
//    }
//}
