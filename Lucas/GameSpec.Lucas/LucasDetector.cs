using System.IO;
using System.Text;
using System.Text.Json;

namespace GameSpec.Lucas
{
    /// <summary>
    /// LucasPakFile
    /// </summary>
    /// <seealso cref="GameSpec.Formats.BinaryPakFile" />
    public class LucasDetector : Detector
    {
        public LucasDetector(FamilyGame game, JsonElement elem) : base(game, elem)
        {
            System.Console.WriteLine("HERE");
        }

        //public string GetSignature(string path)
        //{
        //    using (var md5 = System.Security.Cryptography.MD5.Create())
        //    using (var file = File.OpenRead(path))
        //    {
        //        var br = new BinaryReader(file);
        //        var data = br.ReadBytes(1024 * 1024);
        //        var md5Key = md5.ComputeHash(data, 0, data.Length);
        //        var b = new StringBuilder();
        //        for (var i = 0; i < 16; i++)
        //            b.AppendFormat("{0:x2}", md5Key[i]);
        //        return b.ToString();
        //    }
        //}
    }
}