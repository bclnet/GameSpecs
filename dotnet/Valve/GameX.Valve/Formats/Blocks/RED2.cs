using System.Collections.Generic;
using System.IO;

namespace GameX.Valve.Formats.Blocks
{
    /// <summary>
    /// "RED2" block. CResourceEditInfo.
    /// </summary>
    public class RED2 : REDI
    {
        /// <summary>
        /// This is not a real Valve enum, it's just the order they appear in.
        /// </summary>
        public DATABinaryKV3 BackingData;

        public IDictionary<string, object> SearchableUserData { get; private set; }

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            var kv3 = new DATABinaryKV3
            {
                Offset = Offset,
                Size = Size,
            };
            kv3.Read(parent, r);
            BackingData = kv3;

            ConstructSpecialDependencies();
            ConstuctInputDependencies();

            SearchableUserData = kv3.Data.GetSub("m_SearchableUserData");
            //foreach (var kv in kv3.Data) { } //var structType = ConstructStruct(kv.Key);
        }

        public override void WriteText(IndentedTextWriter w)
           => BackingData.WriteText(w);

        void ConstructSpecialDependencies()
        {
            var specialDependenciesRedi = new REDISpecialDependencies();
            foreach (var specialDependency in BackingData.Data.GetArray("m_SpecialDependencies"))
                specialDependenciesRedi.List.Add(new REDISpecialDependencies.SpecialDependency
                {
                    String = specialDependency.Get<string>("m_String"),
                    CompilerIdentifier = specialDependency.Get<string>("m_CompilerIdentifier"),
                    Fingerprint = specialDependency.GetUInt32("m_nFingerprint"),
                    UserData = specialDependency.GetUInt32("m_nUserData"),
                });
            Structs.Add(REDIStruct.SpecialDependencies, specialDependenciesRedi);
        }

        void ConstuctInputDependencies()
        {
            var dependenciesRedi = new REDIInputDependencies();
            foreach (var dependency in BackingData.Data.GetArray("m_InputDependencies"))
                dependenciesRedi.List.Add(new REDIInputDependencies.InputDependency
                {
                    ContentRelativeFilename = dependency.Get<string>("m_RelativeFilename"),
                    ContentSearchPath = dependency.Get<string>("m_SearchPath"),
                    FileCRC = dependency.GetUInt32("m_nFileCRC"),
                });
            Structs.Add(REDIStruct.InputDependencies, dependenciesRedi);
        }
    }
}
