using System;
using System.Collections.Generic;

namespace GameX.Crytek.Formats.Models
{
    public class MaterialLibrary
    {
        public class Item
        {
            /// <summary>
            /// Unique identifier for this material.
            /// </summary>
            public Guid Guid { get; set; }

            /// <summary>
            /// Name of the material file this material was found.
            /// </summary>
            public string MaterialFileSource { get; set; }

            /// <summary>
            /// A material from MaterialFileSource
            /// </summary>
            public Core.Material Material { get; set; }
        }

        public string GameName { get; set; }
        public string BaseDirectory { get; set; }
        public List<Item> MaterialLibraryItems { get; set; } = new List<Item>();
    }
}
