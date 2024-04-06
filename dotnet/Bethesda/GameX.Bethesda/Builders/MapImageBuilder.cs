using OpenStack.Graphics.Imaging;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace GameX.Bethesda.Builders
{
    public class MapImageBuilder : IDisposable
    {
        class LandData
        {
            public ushort Type { get; set; }
            public int Z { get; set; }
            public bool Used { get; set; }
            public bool Blocked { get; set; }   // Can't walk on
        }

        const int LANDSIZE = 2041; // each landblock is 9x9 points, with the edge points being shared between neighbor landblocks. 255 * 8 + 1, the extra 1 is for the last edge.
        // The following constants change how the lighting works.  It is easy to wash out the bright whites of the snow, so be careful.
        const float COLORCORRECTION = 0.7f; // Increasing COLORCORRECTION makes the base color more prominant.
        const float LIGHTCORRECTION = 2.25f; // Increasing LIGHTCORRECTION increases the contrast between steep and flat slopes.
        const float AMBIENTLIGHT = 0.25f; // Increasing AMBIENTLIGHT makes everyting brighter.
        LandData[,] land { get; set; } = new LandData[LANDSIZE, LANDSIZE];

        public MapImageBuilder()
        {
            for (var x = 0; x < LANDSIZE; x++) for (var y = 0; y < LANDSIZE; y++) land[x, y] = new LandData();

            //var cell = DatabaseManager.Cell;
            //Parallel.For(0, 255 * 255, i =>
            //{
            //    int block_x = i / 255, block_y = i % 255;
            //    var key = (uint)(block_x << 24 | block_y << 16 | 0xFFFF);
            //    if (cell.Source.Contains((int)key)) // Ensures we either have a full cell, or prevents crashes
            //    {
            //        var landblock = cell.GetFile<Landblock>(key);
            //        int startX = block_x * 8, startY = LANDSIZE - block_y * 8 - 1;
            //        for (var x = 0; x < 9; x++)
            //        {
            //            for (var y = 0; y < 9; y++)
            //            {
            //                var type = landblock.Terrain[x * 9 + y];
            //                var newZ = landblock.Height[x * 9 + y];
            //                // Write new data point
            //                land[startY - y, startX + x].Type = type;
            //                land[startY - y, startX + x].Z = GetLandheight(newZ);
            //                land[startY - y, startX + x].Used = true;
            //                var itex = (uint)((type >> 2) & 0x3F);
            //                land[startY - y, startX + x].Blocked = !(itex < 16 || itex > 20);
            //            }
            //        }
            //    }
            //});

            CreateMap();
        }

        public void Dispose() => MapImage.Dispose();

        public DirectBitmap MapImage { get; set; }

        void CreateMap()
        {
            var emptyColor = Color.LimeGreen; // #32cd32
            var lightVector = new float[3] { -1.0f, -1.0f, 0.0f };
            var topo = new byte[LANDSIZE, LANDSIZE, 3];
            //var landColor = GetMapColors();

            //Parallel.For(0, LANDSIZE * LANDSIZE, i =>
            //{
            //    int x = i / LANDSIZE, y = i % LANDSIZE;
            //    var v = new float[3];
            //    if (land[y, x].Used)
            //    {
            //        // Calculate normal by using surrounding z values, if they exist
            //        if (x < LANDSIZE - 1 && y < LANDSIZE - 1)
            //        {
            //            if (land[y, x + 1].Used && land[y + 1, x].Used) { v[0] -= land[y, x + 1].Z - land[y, x].Z; v[1] -= land[y + 1, x].Z - land[y, x].Z; v[2] += 12.0f; }
            //        }
            //        if (x > 0 && y < LANDSIZE - 1)
            //        {
            //            if (land[y, x - 1].Used && land[y + 1, x].Used) { v[0] += land[y, x - 1].Z - land[y, x].Z; v[1] -= land[y + 1, x].Z - land[y, x].Z; v[2] += 12.0f; }
            //        }
            //        if (x > 0 && y > 0)
            //        {
            //            if (land[y, x - 1].Used && land[y - 1, x].Used) { v[0] += land[y, x - 1].Z - land[y, x].Z; v[1] += land[y - 1, x].Z - land[y, x].Z; v[2] += 12.0f; }
            //        }
            //        if (x < LANDSIZE - 1 && y > 0)
            //        {
            //            if (land[y, x + 1].Used && land[y - 1, x].Used) { v[0] -= land[y, x + 1].Z - land[y, x].Z; v[1] += land[y - 1, x].Z - land[y, x].Z; v[2] += 12.0f; }
            //        }

            //        // Check for road bit(s)
            //        var type = (land[y, x].Type & 0x0003) != 0 ? 32 : (ushort)((land[y, x].Type & 0xFF) >> 2);

            //        // Calculate lighting scalar
            //        float light = (((lightVector[0] * v[0] + lightVector[1] * v[1] + lightVector[2] * v[2]) /
            //            (float)Math.Sqrt((lightVector[0] * lightVector[0] + lightVector[1] * lightVector[1] + lightVector[2] * lightVector[2]) *
            //            (v[0] * v[0] + v[1] * v[1] + v[2] * v[2])) * 0.3f + 0.5f) * LIGHTCORRECTION + AMBIENTLIGHT) * COLORCORRECTION;

            //        // Apply lighting scalar to base colors
            //        float r = ColorCheck(landColor[type].R * light), g = ColorCheck(landColor[type].G * light), b = ColorCheck(landColor[type].B * light);
            //        topo[y, x, 0] = (byte)r; topo[y, x, 1] = (byte)g; topo[y, x, 2] = (byte)b;
            //    }
            //    // If data is not present for a point on the map, the resultant pixel is green
            //    else { topo[y, x, 0] = emptyColor.R; topo[y, x, 1] = emptyColor.G; topo[y, x, 2] = emptyColor.B; }
            //});

            MapImage = new DirectBitmap(LANDSIZE, LANDSIZE);
            for (var y = 0; y < LANDSIZE; y++) for (var x = 0; x < LANDSIZE; x++) MapImage.SetPixel(x, y, Color.FromArgb(topo[y, x, 0], topo[y, x, 1], topo[y, x, 2]));
        }

        ///// <summary>
        ///// Sanity check to make sure our colors are in-bounds.
        ///// </summary>
        //float ColorCheck(float color) => Math.Clamp(color, 0.0f, 255.0f);

        //Color[] GetMapColors()
        //{
        //    var portal = DatabaseManager.Portal;
        //    var RegionID = 0x13000000U;
        //    var Region = portal.GetFile<RegionDesc>(RegionID);
        //    var landColors = new Color[Region.TerrainInfo.LandSurfaces.TexMerge.TerrainDesc.Length];
        //    for (var i = 0; i < Region.TerrainInfo.LandSurfaces.TexMerge.TerrainDesc.Length; i++)
        //    {
        //        var t = Region.TerrainInfo.LandSurfaces.TexMerge.TerrainDesc[i];
        //        var st = portal.GetFile<SurfaceTexture>(t.TerrainTex.TexGID);
        //        var texture = portal.GetFile<Texture>(st.Textures[^1]);
        //        landColors[i] = GetAverageColor(texture);
        //    }
        //    return landColors.ToArray();
        //}

        //Color GetAverageColor(Texture image)
        //{
        //    if (image == null) return Color.FromArgb(0, 255, 0); // TRANSPARENT

        //    int total = 0, r = 0, g = 0, b = 0;
        //    for (var x = 0; x < image.Width; x++)
        //        for (var y = 0; y < image.Height; y++) { var clr = GetPixel(image, x, y); r += clr.B; g += clr.G; b += clr.R; total++; } // Is the A8R8G8B8 loading colors properly? (R=B,G=G,B=R)?

        //    // Calculate average
        //    r /= total; g /= total; b /= total;
        //    return Color.FromArgb(r, g, b);
        //}

        //Color GetPixel(Texture texture, int x, int y)
        //{
        //    var offset = (y * texture.Width + x) * 4;
        //    int r = texture.SourceData[offset + 2], g = texture.SourceData[offset + 1], b = texture.SourceData[offset];
        //    return Color.FromArgb(r, g, b);
        //}

        ///// <summary>
        ///// Functions like the Region.LandDefs.Land_Height_Table from (client_)portal.dat 0x13000000
        ///// </summary>
        //int GetLandheight(byte height)
        //{
        //    if (height <= 200) return height * 2;
        //    else if (height <= 240) return 400 + (height - 200) * 4;
        //    else if (height <= 250) return 560 + (height - 240) * 8;
        //    else return 640 + (height - 250) * 10;
        //}
    }
}
