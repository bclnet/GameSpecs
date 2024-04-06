namespace GameX.Epic.Formats.Core
{
    public class UObject
    {
        // internal storage
        UPackage Package;
        string Name;
        UObject Outer;         // UObject containing this UObject (e.g. UAnimSet holding UAnimSequence). Not really used here.
        int PackageIndex;   // index in package export table; INDEX_NONE for non-packaged (transient) object
        int NetIndex;

        //public UObject(UPackage Ar)
        //{
        //    int index;
        //    if (Ar.Engine == UE2X && Ar.ArVer >= 145)
        //        *this << index;
        //    else
        //    if (Ar.Engine >= UE3)
        //        *this << index;
        //    else
        //        *this << AR_INDEX(index);

        //    if (index < 0)
        //    {
        //        //		const FObjectImport &Imp = GetImport(-index-1);
        //        //		appPrintf("PKG: Import[%s,%d] OBJ=%s CLS=%s\n", GetObjectName(Imp.PackageIndex), index, *Imp.ObjectName, *Imp.ClassName);
        //        Obj = Ar.CreateImport(-index - 1);
        //    }
        //    else if (index > 0)
        //    {
        //        //		const FObjectExport &Exp = GetExport(index-1);
        //        //		appPrintf("PKG: Export[%d] OBJ=%s CLS=%s\n", index, *Exp.ObjectName, GetClassNameFor(Exp));
        //        Obj = Ar.CreateExport(index - 1);
        //    }
        //    else // index == 0
        //    {
        //        Obj = null;
        //    }
        //}
    }
}