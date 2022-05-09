namespace Tests
{
    public class TestObject : AbstractTest
    {
        public TestObject(UnityTest test) : base(test) { }

        public override void Start()
        {
            if (!string.IsNullOrEmpty(Test.Param1)) MakeObject(Test.Param1);
        }

        void MakeObject(string path)
            => Graphic.CreateObject(path, out var _);
    }
}
