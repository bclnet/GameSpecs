using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/PanoramaStyle
    public class DATAPanoramaStyle : DATAPanorama
    {
        DATABinaryKV3 SourceMap;

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            base.Read(parent, r);
            SourceMap = parent.GetBlockByType<SRMA>();
        }

        public override string ToString() => ToString(true);

        public string ToString(bool applySourceMapIfPresent)
            => (applySourceMapIfPresent && SourceMap != default && !(SourceMap.Data.Get<object>("DBITSLC") is null))
                ? Encoding.UTF8.GetString(PanoramaSourceMapDecode(Data, SourceMap.Data))
                : base.ToString();

        static byte[] PanoramaSourceMapDecode(byte[] data, IDictionary<string, object> sourceMap)
        {
            var mapping = sourceMap.GetArray("DBITSLC", kvArray => (kvArray.GetInt32("0"), kvArray.GetInt32("1"), kvArray.GetInt32("2")));
            var results = new List<IEnumerable<byte>>();
            var currentCol = 0;
            var currentLine = 1;
            for (var i = 0; i < mapping.Length - 1; i++)
            {
                var (startIndex, sourceLine, sourceColumn) = mapping[i];
                var (nextIndex, _, _) = mapping[i + 1];

                // Prepend newlines if they are in front of this chunk according to sourceLineByteIndices
                if (currentLine < sourceLine) { results.Add(Enumerable.Repeat(Encoding.UTF8.GetBytes("\n")[0], sourceLine - currentLine)); currentCol = 0; currentLine = sourceLine; }
                // Referring back to an object higher in hierarchy, also add newline here
                else if (sourceLine < currentLine) { results.Add(Enumerable.Repeat(Encoding.UTF8.GetBytes("\n")[0], 1)); currentCol = 0; currentLine++; }
                // Prepend spaces until we catch up to the index we need to be at
                if (currentCol < sourceColumn) { results.Add(Enumerable.Repeat(Encoding.UTF8.GetBytes(" ")[0], sourceColumn - currentCol)); currentCol = sourceColumn; }
                // Copy destination
                var length = nextIndex - startIndex;
                results.Add(data.Skip(startIndex).Take(length));
                currentCol += length;
            }
            results.Add(Enumerable.Repeat(Encoding.UTF8.GetBytes("\n")[0], 1));
            results.Add(data.Skip(mapping[^1].Item1));
            return results.SelectMany(_ => _).ToArray();
        }
    }
}
