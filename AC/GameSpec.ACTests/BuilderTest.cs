using GameSpec.AC.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameSpec.AC
{
    [TestClass]
    public class BuilderTest
    {
        static readonly Family family = FamilyManager.GetFamily("AC");

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
