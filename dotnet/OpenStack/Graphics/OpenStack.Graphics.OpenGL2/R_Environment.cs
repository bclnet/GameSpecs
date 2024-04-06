using System.Collections.Generic;

namespace OpenStack.Graphics.OpenGL2
{
    public class R_Environment
    {
        public ACE.DatLoader.FileTypes.Environment _env { get; set; }

        public Dictionary<uint, R_CellStruct> R_CellStructs { get; set; }

        public R_Environment(uint envID)
        {
            // caching?
            _env = DatManager.PortalDat.ReadFromDat<ACE.DatLoader.FileTypes.Environment>(envID);

            BuildEnv();
        }

        public void BuildEnv()
        {
            R_CellStructs = new Dictionary<uint, R_CellStruct>();

            foreach (var kvp in _env.Cells)
                R_CellStructs.Add(kvp.Key, new R_CellStruct(kvp.Value));
        }

        public void Draw(uint? cellStructId = null, List<Texture2D> textures = null)
        {
            if (cellStructId != null)
            {
                // draw EnvCell
                if (R_CellStructs.TryGetValue(cellStructId.Value, out var cellStruct))
                    cellStruct.Draw(textures);
            }
            else
            {
                // draw all the possible cell structs
                foreach (var cellStruct in R_CellStructs.Values)
                    cellStruct.Draw(textures);
            }
        }
    }
}
