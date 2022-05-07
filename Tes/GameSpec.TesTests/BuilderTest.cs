using GameSpec.Tes.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameSpec.Tes
{
    [TestClass]
    public class BuilderTest
    {
        static readonly Family family = FamilyManager.GetFamily("Tes");

        [TestMethod]
        public void MapImageBuilder()
        {
            var output = @"C:\T_\GameSpec\TesMap.png";
            using var builder = new MapImageBuilder();
            Assert.IsNotNull(builder.MapImage, "Should be not null");
            builder.MapImage.Save(output);
        }
    }
}
