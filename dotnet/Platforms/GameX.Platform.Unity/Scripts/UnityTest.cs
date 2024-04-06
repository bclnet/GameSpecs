using Tests;
using UnityEngine;

public class UnityTest : UnityEngine.MonoBehaviour
{
    public enum TestTest
    {
        Texture,
        Object,
        Cell,
        Engine,
    }

    AbstractTest Test;

    [Header("Pak Settings")]
    public string Family = "Tes";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";
    public string Pak2Uri;
    public string Pak3Uri;

    [Header("Test Params")]
    public TestTest Type = TestTest.Texture;
    public string Param1 = "bookart/boethiah_256.dds";
    //public string Param1 = "meshes/x/ex_common_balcony_01.nif";
    public string Param2;
    public string Param3;
    public string Param4;

    public void Awake()
    {
        Test = Type switch
        {
            TestTest.Texture => new TestTexture(this),
            TestTest.Object => new TestObject(this),
            TestTest.Cell => new TestCell(this),
            TestTest.Engine => new TestEngine(this),
            _ => new TestObject(this),
        };
    }

    public void OnDestroy() => Test?.Dispose();

    public void Start() => Test?.Start();

    public void Update() => Test?.Update();
}