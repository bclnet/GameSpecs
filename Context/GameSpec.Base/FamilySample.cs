using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using static GameSpec.Util;

namespace GameSpec
{
    /// <summary>
    /// FamilySample
    /// </summary>
    public class FamilySample
    {
        public Dictionary<string, List<File>> Samples { get; } = new Dictionary<string, List<File>>();

        /// <summary>
        /// FamilySample
        /// </summary>
        /// <param name="elem"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FamilySample(JsonElement elem)
        {
            foreach (var s in elem.EnumerateObject())
            {
                var files = s.Value.GetProperty("files").EnumerateArray().Select(x => new File(x)).ToList();
                Samples.Add(s.Name, files);
            }
        }

        /// <summary>
        /// The sample file.
        /// </summary>
        public class File
        {
            /// <summary>
            /// The path
            /// </summary>
            public string Path { get; set; }
            /// <summary>
            /// The size
            /// </summary>
            public long Size { get; set; }
            /// <summary>
            /// The type
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// File
            /// </summary>
            /// <param name="elem"></param>
            /// <exception cref="ArgumentNullException"></exception>
            public File(JsonElement elem)
            {
                Path = _value(elem, "path");
                Size = _value(elem, "size", x => x.GetInt64(), 0L);
                Type = _value(elem, "type");
            }

            /// <summary>
            /// Converts to string.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this instance.
            /// </returns>
            public override string ToString() => Path;
        }
    }
}