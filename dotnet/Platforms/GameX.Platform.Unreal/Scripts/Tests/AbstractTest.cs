using GameX;
using GameX.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameSpecUnreal.Tests
{
    public abstract class AbstractTest : IDisposable
    {
        protected readonly UnrealTest Test;
        protected readonly Family Family;
        protected readonly List<PakFile> PakFiles = new List<PakFile>();
        protected readonly IUnrealGraphic Graphic;

        public AbstractTest(UnrealTest test)
        {
            Test = test;
            if (string.IsNullOrEmpty(test.Family)) return;
            Family = FamilyManager.GetFamily(test.Family);
            if (!string.IsNullOrEmpty(test.PakUri)) PakFiles.Add(Family.OpenPakFile(new Uri(test.PakUri)));
            if (!string.IsNullOrEmpty(test.Pak2Uri)) PakFiles.Add(Family.OpenPakFile(new Uri(test.Pak2Uri)));
            if (!string.IsNullOrEmpty(test.Pak3Uri)) PakFiles.Add(Family.OpenPakFile(new Uri(test.Pak3Uri)));
            var first = PakFiles.FirstOrDefault();
            Graphic = (IUnrealGraphic)first?.Graphic;
        }

        public virtual void Dispose()
        {
            foreach (var pakFile in PakFiles) pakFile.Dispose();
            PakFiles.Clear();
        }

        public abstract void Start();

        public virtual void Update(float deltaTime) { }
    }
}
