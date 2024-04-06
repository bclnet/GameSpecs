using GameX.Bethesda.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameX.Bethesda
{
    [TestClass]
    public class BuilderTest
    {
        static readonly Family family = FamilyManager.GetFamily("Bethesda");

        [TestMethod]
        public void MapImageBuilder()
        {
            var output = @"C:\T_\GameX\BethesdaMap.png";
            using var builder = new MapImageBuilder();
            Assert.IsNotNull(builder.MapImage, "Should be not null");
            builder.MapImage.Save(output);
        }
    }
}
