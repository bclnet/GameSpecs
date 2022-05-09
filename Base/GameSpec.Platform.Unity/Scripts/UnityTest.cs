using Tests;
using UnityEngine;

public class UnityTest : UnityEngine.MonoBehaviour
{
    AbstractTest Test;

    [Header("Pak Settings")]
    public string Family = "Tes";
    public string PakUri = "game:/Morrowind.bsa#Morrowind";
    public string Pak2Uri;
    public string Pak3Uri;

    [Header("Test Params")]
    public UnityTestTest Type = UnityTestTest.Object;
    public string Param1 = "meshes/x/ex_common_balcony_01.nif";
    public string Param2;
    public string Param3;
    public string Param4;

    public void Awake()
    {
        switch (Type)
        {
            case UnityTestTest.Texture: Test = new TestTexture(this); break;
            case UnityTestTest.Object: Test = new TestObject(this); break;
            case UnityTestTest.Cell: Test = new TestCell(this); break;
            case UnityTestTest.Engine: Test = new TestEngine(this); break;
            default: Test = new TestObject(this); break;
        }
    }

    public void OnDestroy() => Test?.Dispose();

    public void Start() => Test?.Start();

    public void Update() => Test?.Update();
}