using GameX.WbB.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameX.WbB
{
    [TestClass]
    public class BuilderTest
    {
        static readonly Family family = FamilyManager.GetFamily("WbB");

        [TestMethod]
        public void MapImageBuilder()
        {
            var output = @"C:\T_\GameEstate\ACMap.png";
            using var builder = new MapImageBuilder();
            Assert.IsNotNull(builder.MapImage, "Should be not null");
            builder.MapImage.Save(output);
        }
    }
}
