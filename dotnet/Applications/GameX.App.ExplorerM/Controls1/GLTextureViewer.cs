//using OpenStack;
//using OpenStack.Graphics;
//using OpenStack.Graphics.OpenGL.Renderer1.Renderers;
//using OpenStack.Graphics.Renderer1;
//using System.ComponentModel;
//using System.Numerics;
//using System.Runtime.CompilerServices;

//namespace GameX.App.Explorer.Controls1
//{
//    //was:Renderer/GLTextureViewer
//    public class GLTextureViewer : OpenGLView
//    {
//        public GLTextureViewer()
//        {
//            OnDisplay += OnPaint;
//            //Unloaded += (s, e) => { GLPaint -= OnPaint; };
//        }

//        public event PropertyChangedEventHandler PropertyChanged;
//        void NotifyPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

//        public static readonly BindableProperty GraphicProperty = BindableProperty.Create(nameof(Graphic), typeof(object), typeof(GLTextureViewer),
//            propertyChanged: (d, e, n) => (d as GLTextureViewer).OnProperty());
        
//        public IOpenGraphic Graphic
//        {
//            get => GetValue(GraphicProperty) as IOpenGraphic;
//            set => SetValue(GraphicProperty, value);
//        }

//        public static readonly BindableProperty SourceProperty = BindableProperty.Create(nameof(Source), typeof(object), typeof(GLTextureViewer),
//            propertyChanged: (d, e, n) => (d as GLTextureViewer).OnProperty());

//        public object Source
//        {
//            get => GetValue(SourceProperty);
//            set => SetValue(SourceProperty, value);
//        }

//        void OnProperty()
//        {
//            if (Graphic == null || Source == null) return;
//            var graphic = Graphic as IOpenGLGraphic;
//            var source = Source is ITexture z ? z
//                : Source is IRedirected<ITexture> y ? y.Value
//                : null;
//            if (source == null) return;

//#if true
//            Camera.SetViewportSize(source.Width, source.Height);
//#endif
//            Camera.SetLocation(new Vector3(200));
//            Camera.LookAt(new Vector3(0));

//            var texture = graphic.TextureManager.LoadTexture(source, out _);
//            Renderers.Add(new TextureRenderer(graphic, texture));
//        }

//        readonly HashSet<TextureRenderer> Renderers = new();

//        void OnPaint(object sender, EventArgs e)
//        {
//            foreach (var renderer in Renderers) renderer.Render(e.Camera, RenderPass.Both);
//        }
//    }
//}
