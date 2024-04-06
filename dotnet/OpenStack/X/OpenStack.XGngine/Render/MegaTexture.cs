using System.Runtime.CompilerServices;
using WaveEngine.Bindings.OpenGLES;
using static System.NumericsX.OpenStack.Gngine.Gngine;
using static System.NumericsX.OpenStack.Gngine.Render.QGL;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public struct TextureTile
    {
        public int x, y;
    }

    public unsafe class TextureLevel
    {
        public const int TILE_PER_LEVEL = 4;
        public MegaTexture mega;

        public int tileOffset;
        public int tilesWide;
        public int tilesHigh;

        public Image image;
        public TextureTile[,] tileMap = new TextureTile[TILE_PER_LEVEL, TILE_PER_LEVEL];

        public float[] parms = new float[4];

        // Center is in the 0.0 to 1.0 range
        public void UpdateForCenter(float* center)
        {
            int* globalTileCorner = stackalloc int[2], localTileOffset = stackalloc int[2];

            if (tilesWide <= TILE_PER_LEVEL && tilesHigh <= TILE_PER_LEVEL)
            {
                globalTileCorner[0] = 0; globalTileCorner[1] = 0;
                localTileOffset[0] = 0; localTileOffset[1] = 0;
                // orient the mask so that it doesn't mask anything at all
                parms[0] = 0.25f; parms[1] = 0.25f; parms[3] = 0.25f;
            }
            else
            {
                var global = stackalloc float[2];
                for (var i = 0; i < 2; i++)
                {
                    // this value will be outside the 0.0 to 1.0 range unless we are in the corner of the megaTexture
                    global[i] = (center[i] * parms[3] - 0.5f) * TILE_PER_LEVEL;
                    globalTileCorner[i] = (int)(global[i] + 0.5f);
                    localTileOffset[i] = globalTileCorner[i] & (TILE_PER_LEVEL - 1);

                    // scaling for the mask texture to only allow the proper window of tiles to show through
                    parms[i] = -globalTileCorner[i] / (float)TILE_PER_LEVEL;
                }
            }

            image.Bind();

            var globalTile = stackalloc int[2];
            for (var x = 0; x < TILE_PER_LEVEL; x++)
                for (var y = 0; y < TILE_PER_LEVEL; y++)
                {
                    globalTile[0] = globalTileCorner[0] + ((x - localTileOffset[0]) & (TILE_PER_LEVEL - 1));
                    globalTile[1] = globalTileCorner[1] + ((y - localTileOffset[1]) & (TILE_PER_LEVEL - 1));

                    UpdateTile(x, y, globalTile[0], globalTile[1]);
                }
        }

        // A local tile will only be mapped to globalTile[ localTile + X * TILE_PER_LEVEL ] for some x
        public void UpdateTile(int localX, int localY, int globalX, int globalY)
        {
            ref TextureTile tile = ref tileMap[localX, localY];

            if (tile.x == globalX && tile.y == globalY) return;
            if ((globalX & (TILE_PER_LEVEL - 1)) != localX || (globalY & (TILE_PER_LEVEL - 1)) != localY) common.Error("TextureLevel::UpdateTile: bad coordinate mod");

            tile.x = globalX; tile.y = globalY;

            byte* data = stackalloc byte[MegaTexture.TILE_SIZE * MegaTexture.TILE_SIZE * 4];

            // off the map
            var tileSize = MegaTexture.TILE_SIZE * MegaTexture.TILE_SIZE * 4;
            if (globalX >= tilesWide || globalX < 0 || globalY >= tilesHigh || globalY < 0) Unsafe.InitBlock(data, 0, (uint)tileSize);
            else
            {
                // extract the data from the full image (FIXME: background load from disk)
                var tileNum = tileOffset + tile.y * tilesWide + tile.x;

                mega.fileHandle.Seek(tileNum * tileSize, FS_SEEK.SET);
                Unsafe.InitBlock(data, 128, (uint)tileSize);
                mega.fileHandle.Read(data, tileSize);
            }

            if (MegaTexture.r_showMegaTextureLabels.Bool)
            {
                // put a color marker in it. localX and localY are < TILE_PER_LEVEL => that fits perfectly into a byte.
                var color = stackalloc byte[4] { (byte)(255 * localX / TILE_PER_LEVEL), (byte)(255 * localY / TILE_PER_LEVEL), 0, 0 };
                for (var x = 0; x < 8; x++)
                    for (var y = 0; y < 8; y++)
                        *(int*)&data[((y + MegaTexture.TILE_SIZE / 2 - 4) * MegaTexture.TILE_SIZE + x + MegaTexture.TILE_SIZE / 2 - 4) * 4] = *(int*)color;
            }

            // upload all the mip-map levels
            int level = 0, size = MegaTexture.TILE_SIZE;
            while (true)
            {
                qglTexSubImage2D(TextureTarget.Texture2d, level, localX * size, localY * size, size, size, PixelFormat.Rgba, VertexAttribPointerType.UnsignedByte, data);
                level++; size >>= 1;

                if (size == 0) break;

                // mip-map in place
                for (var y = 0; y < size; y++)
                {
                    var in1 = data + y * size * 16;
                    var in2 = in1 + size * 8;
                    var out_ = data + y * size * 4;
                    for (var x = 0; x < size; x++)
                    {
                        out_[x * 4 + 0] = (byte)((in1[x * 8 + 0] + in1[x * 8 + 4 + 0] + in2[x * 8 + 0] + in2[x * 8 + 4 + 0]) >> 2);
                        out_[x * 4 + 1] = (byte)((in1[x * 8 + 1] + in1[x * 8 + 4 + 1] + in2[x * 8 + 1] + in2[x * 8 + 4 + 1]) >> 2);
                        out_[x * 4 + 2] = (byte)((in1[x * 8 + 2] + in1[x * 8 + 4 + 2] + in2[x * 8 + 2] + in2[x * 8 + 4 + 2]) >> 2);
                        out_[x * 4 + 3] = (byte)((in1[x * 8 + 3] + in1[x * 8 + 4 + 3] + in2[x * 8 + 3] + in2[x * 8 + 4 + 3]) >> 2);
                    }
                }
            }
        }

        public void Invalidate()
        {
            for (var x = 0; x < TILE_PER_LEVEL; x++)
                for (var y = 0; y < TILE_PER_LEVEL; y++)
                    tileMap[x, y].x = tileMap[x, y].y = -99999;
        }
    }

    public struct MegaTextureHeader
    {
        public int tileSize;
        public int tilesWide;
        public int tilesHigh;
    }

    public unsafe class MegaTexture
    {
        public const int MAX_MEGA_CHANNELS = 3;     // normal, diffuse, specular
        public const int MAX_LEVELS = 12;
        public const int MAX_LEVEL_WIDTH = 512;
        public const int TILE_SIZE = MAX_LEVEL_WIDTH / TextureLevel.TILE_PER_LEVEL;

        public static CVar r_megaTextureLevel = new("r_megaTextureLevel", "0", CVAR.RENDERER | CVAR.INTEGER, "draw only a specific level");
        public static CVar r_showMegaTexture = new("r_showMegaTexture", "0", CVAR.RENDERER | CVAR.BOOL, "display all the level images");
        public static CVar r_showMegaTextureLabels = new("r_showMegaTextureLabels", "0", CVAR.RENDERER | CVAR.BOOL, "draw colored blocks in each tile");
        public static CVar r_skipMegaTexture = new("r_skipMegaTexture", "0", CVAR.RENDERER | CVAR.INTEGER, "only use the lowest level image");
        public static CVar r_terrainScale = new("r_terrainScale", "3", CVAR.RENDERER | CVAR.INTEGER, "vertically scale USGS data");

        // allow sparse population of the upper detail tiles

        static int RoundDownToPowerOfTwo(int num)
        {
            int pot;
            for (pot = 1; (pot * 2) <= num; pot <<= 1) { }
            return pot;
        }

        static reinterpret.Color fillColor;

        static byte[,] colors = {
            { 0, 0, 0, 255 },
            { 255, 0, 0, 255 },
            { 0, 255, 0, 255 },
            { 255, 255, 0, 255 },
            { 0, 0, 255, 255 },
            { 255, 0, 255, 255 },
            { 0, 255, 255, 255 },
            { 255, 255, 255, 255 }
        };

        static void R_EmptyLevelImage(Image image)
        {
            int c = MAX_LEVEL_WIDTH * MAX_LEVEL_WIDTH;
            byte* data = stackalloc byte[c * 4];
            for (var i = 0; i < c; i++) ((int*)data)[i] = fillColor.intVal;

            // FIXME: this won't live past vid mode changes
            image.GenerateImage(data, MAX_LEVEL_WIDTH, MAX_LEVEL_WIDTH, Image.TF.DEFAULT, false, Image.TR.REPEAT, Image.TD.HIGH_QUALITY);
        }

        internal VFile fileHandle;

        SrfTriangles currentTriMapping;

        Vector3 currentViewOrigin;

        float[,] localViewToTextureCenter = new float[2, 4];

        int numLevels;
        TextureLevel[] levels = new TextureLevel[MAX_LEVELS];                // 0 is the highest resolution
        MegaTextureHeader header;

        public bool InitFromMegaFile(string fileBase)
        {
            var name = PathX.StripFileExtension($"megaTextures/{fileBase}") + ".mega";

            fileHandle = fileSystem.OpenFileRead(name);
            if (fileHandle == null) { common.Printf($"MegaTexture: failed to open {name}\n"); return false; }
            fixed (MegaTextureHeader* headerM = &header) fileHandle.Read((byte*)headerM, sizeof(MegaTextureHeader));
            if (header.tileSize < 64 || header.tilesWide < 1 || header.tilesHigh < 1) { common.Printf($"MegaTexture: bad header on {name}\n"); return false; }

            currentTriMapping = null;

            numLevels = 0;
            var width = header.tilesWide;
            var height = header.tilesHigh;
            var tileOffset = 1;                 // just past the header

            Array.Clear(levels, 0, levels.Length);
            while (true)
            {
                var level = levels[numLevels];
                level.mega = this;
                level.tileOffset = tileOffset;
                level.tilesWide = width; level.tilesHigh = height;
                // initially mask everything
                level.parms[0] = -1f; level.parms[1] = 0f; level.parms[2] = 0f; level.parms[3] = (float)width / TextureLevel.TILE_PER_LEVEL;
                level.Invalidate();

                tileOffset += level.tilesWide * level.tilesHigh;

                var str = $"MEGA_{fileBase}_{numLevels}";

                // give each level a default fill color
                for (var i = 0; i < 4; i++) fillColor.color[i] = colors[numLevels + 1, i];

                levels[numLevels].image = globalImages.ImageFromFunction(str, R_EmptyLevelImage);
                numLevels++;

                if (width <= TextureLevel.TILE_PER_LEVEL && height <= TextureLevel.TILE_PER_LEVEL) break;
                width = (width + 1) >> 1; height = (height + 1) >> 1;
            }

            // force first bind to load everything
            currentViewOrigin.x = -99999999.0f; currentViewOrigin.y = -99999999.0f; currentViewOrigin.z = -99999999.0f;

            return true;
        }

        // analyzes xyz and st to create a mapping. This is not very robust, but works for rectangular grids
        public void SetMappingForSurface(SrfTriangles tri)
        {
            if (tri == currentTriMapping) return;
            currentTriMapping = tri;

            if (tri.verts == null) return;

            DrawVert origin = default;
            var axis = stackalloc DrawVert[2];

            origin.st.x = 1f; origin.st.y = 1f;
            axis[0].st.x = 0; axis[0].st.y = 1;
            axis[1].st.x = 1; axis[1].st.y = 0;

            for (var i = 0; i < tri.numVerts; i++)
            {
                ref DrawVert v = ref tri.verts[i];
                if (v.st.x <= origin.st.x && v.st.y <= origin.st.y) origin = v;
                if (v.st.x >= axis[0].st.x && v.st.y <= axis[0].st.y) axis[0] = v;
                if (v.st.x <= axis[1].st.x && v.st.y >= axis[1].st.y) axis[1] = v;
            }

            for (var i = 0; i < 2; i++)
            {
                var dir = axis[i].xyz - origin.xyz;
                var texLen = axis[i].st[i] - origin.st[i];
                var spaceLen = (axis[i].xyz - origin.xyz).Length;

                var scale = texLen / (spaceLen * spaceLen);
                dir *= scale;

                var c = origin.xyz * dir - origin.st[i];

                localViewToTextureCenter[i, 0] = dir.x;
                localViewToTextureCenter[i, 1] = dir.y;
                localViewToTextureCenter[i, 2] = dir.z;
                localViewToTextureCenter[i, 3] = -c;
            }
        }

        // binds images and sets program parameters
        public void BindForViewOrigin(Vector3 viewOrigin)
        {
            SetViewOrigin(viewOrigin);

            // borderClamp image goes in texture 0
            GL_SelectTexture(0);
            globalImages.borderClampImage.Bind();

            // level images in higher textures, blurriest first
            for (var i = 0; i < 7; i++)
            {
                GL_SelectTexture(1 + i);

                if (i >= numLevels) globalImages.whiteImage.Bind();
                else
                {
                    var level = levels[numLevels - 1 - i];
                    if (r_showMegaTexture.Bool)
                    {
                        if ((i & 1) != 0) globalImages.blackImage.Bind();
                        else globalImages.whiteImage.Bind();
                    }
                    else level.image.Bind();
                }
            }
        }

        // removes texture bindings. This can go away once everything uses fragment programs so the enable states don't need tracking
        public void Unbind()
        {
            for (var i = 0; i < numLevels; i++)
            {
                GL_SelectTexture(1 + i);
                globalImages.BindNull();
            }
        }

        void SetViewOrigin(Vector3 viewOrigin)
        {
            if (r_showMegaTextureLabels.IsModified)
            {
                r_showMegaTextureLabels.ClearModified();
                currentViewOrigin.x = viewOrigin.x + 0.1f; // force a change
                for (var i = 0; i < numLevels; i++) levels[i].Invalidate();
            }

            if (viewOrigin == currentViewOrigin) return;
            if (r_skipMegaTexture.Bool) return;
            currentViewOrigin = viewOrigin;

            var texCenter = stackalloc float[2];

            // convert the viewOrigin to a texture center, which will be a different conversion for each megaTexture
            for (var i = 0; i < 2; i++)
                texCenter[i] =
                    viewOrigin.x * localViewToTextureCenter[i, 0] +
                    viewOrigin.y * localViewToTextureCenter[i, 1] +
                    viewOrigin.z * localViewToTextureCenter[i, 2] +
                    localViewToTextureCenter[i, 3];

            for (var i = 0; i < numLevels; i++)
                levels[i].UpdateForCenter(texCenter);
        }

        static void GenerateMegaMipMaps(MegaTextureHeader header, VFile outFile)
        {
            outFile.Flush();

            // out fileSystem doesn't allow read / write access...
            var inFile = fileSystem.OpenFileRead(outFile.Name);

            int tileOffset = 1, width = header.tilesWide, height = header.tilesHigh;

            int tileSize = header.tileSize * header.tileSize * 4;
            byte* oldBlock = stackalloc byte[tileSize], newBlock = stackalloc byte[tileSize];

            while (width > 1 || height > 1)
            {
                var newHeight = (height + 1) >> 1; if (newHeight < 1) newHeight = 1;
                var newWidth = (width + 1) >> 1; if (width < 1) width = 1;
                common.Printf($"generating {newWidth} x {newHeight} block mip level\n");

                int tileNum;
                for (var y = 0; y < newHeight; y++)
                {
                    common.Printf($"row {y}\n");
                    session.UpdateScreen();

                    // mip map four original blocks down into a single new block
                    for (var x = 0; x < newWidth; x++)
                        for (var yy = 0; yy < 2; yy++)
                            for (int xx = 0; xx < 2; xx++)
                            {
                                int tx = x * 2 + xx, ty = y * 2 + yy;

                                // off edge, zero fill
                                if (tx > width || ty > height) Unsafe.InitBlock(newBlock, 0, (uint)tileSize);
                                else
                                {
                                    tileNum = tileOffset + ty * width + tx;
                                    inFile.Seek(tileNum * tileSize, FS_SEEK.SET);
                                    inFile.Read(oldBlock, tileSize);
                                }
                                // mip map the new pixels
                                for (var yyy = 0; yyy < TILE_SIZE / 2; yyy++)
                                    for (var xxx = 0; xxx < TILE_SIZE / 2; xxx++)
                                    {
                                        byte* in1 = &oldBlock[(yyy * 2 * TILE_SIZE + xxx * 2) * 4];
                                        byte* out_ = &newBlock[(((TILE_SIZE / 2 * yy) + yyy) * TILE_SIZE + (TILE_SIZE / 2 * xx) + xxx) * 4];
                                        out_[0] = (byte)((in1[0] + in1[4] + in1[0 + TILE_SIZE * 4] + in1[4 + TILE_SIZE * 4]) >> 2);
                                        out_[1] = (byte)((in1[1] + in1[5] + in1[1 + TILE_SIZE * 4] + in1[5 + TILE_SIZE * 4]) >> 2);
                                        out_[2] = (byte)((in1[2] + in1[6] + in1[2 + TILE_SIZE * 4] + in1[6 + TILE_SIZE * 4]) >> 2);
                                        out_[3] = (byte)((in1[3] + in1[7] + in1[3 + TILE_SIZE * 4] + in1[7 + TILE_SIZE * 4]) >> 2);
                                    }

                                // write the block out
                                tileNum = tileOffset + width * height + y * newWidth + x;
                                outFile.Seek(tileNum * tileSize, FS_SEEK.SET);
                                outFile.Write(newBlock, tileSize);

                            }
                }
                tileOffset += width * height;
                width = newWidth;
                height = newHeight;
            }
        }

        // Make a 2k x 2k preview image for a mega texture that can be used in modeling programs
        static void GenerateMegaPreview(string fileName)
        {
            var fileHandle = fileSystem.OpenFileRead(fileName);
            if (fileHandle == null) { common.Printf($"MegaTexture: failed to open {fileName}\n"); return; }

            var outName = PathX.StripFileExtension(fileName) + "_preview.tga";
            common.Printf($"Creating {outName}.\n");

            MegaTextureHeader header = default;
            fileHandle.Read((byte*)&header, sizeof(MegaTextureHeader));
            if (header.tileSize < 64 || header.tilesWide < 1 || header.tilesHigh < 1) { common.Printf($"MegaTexture: bad header on {fileName}\n"); return; }

            int tileSize = header.tileSize,
                width = header.tilesWide,
                height = header.tilesHigh,
                tileOffset = 1,
                tileBytes = tileSize * tileSize * 4;
            // find the level that fits
            while (width * tileSize > 2048 || height * tileSize > 2048)
            {
                tileOffset += width * height;
                width >>= 1; if (width < 1) width = 1;
                height >>= 1; if (height < 1) height = 1;
            }

            byte* pic = (byte*)R_StaticAlloc(width * height * tileBytes);
            byte* oldBlock = stackalloc byte[tileBytes];
            for (var y = 0; y < height; y++)
                for (var x = 0; x < width; x++)
                {
                    var tileNum = tileOffset + y * width + x;
                    fileHandle.Seek(tileNum * tileBytes, FS_SEEK.SET);
                    fileHandle.Read(oldBlock, tileBytes);

                    for (var yy = 0; yy < tileSize; yy++) Unsafe.CopyBlock(pic + ((y * tileSize + yy) * width * tileSize + x * tileSize) * 4, oldBlock + yy * tileSize * 4, (uint)(tileSize * 4));
                }
            Image.R_WriteTGA(outName, pic, width * tileSize, height * tileSize, false);
            R_StaticFree(pic);
        }

        #region TargaHeader

        // Incrementally load a giant tga file and process into the mega texture block format
        /*

        struct TargaHeader
        {
            public byte id_length, colormap_type, image_type;
            public ushort colormap_index, colormap_length;
            public byte colormap_size;
            public ushort x_origin, y_origin, width, height;
            public byte pixel_size, attributes;

            //static byte ReadByte(VFile f) { byte b; f.Read(&b, 1); return b; }
            //static short ReadShort(VFile f) { byte* b = stackalloc byte[2]; f.Read(b, 2); return (short)(b[0] + (b[1] << 8)); }
        }

        static void MakeMegaTexture_f( CmdArgs args ) {
            int		columns, fileSize, numBytes;
            byte	*pixbuf;
            int		row, column;
            TargaHeader	targa_header;

            if ( args.Argc() != 2 ) {
                common.Printf( "USAGE: makeMegaTexture <filebase>\n" );
                return;
            }

            idStr	name_s = "megaTextures/";
            name_s += args.Argv(1);
            name_s.StripFileExtension();
            name_s += ".tga";

            const char	*name = name_s.c_str();

            //
            // open the file
            //
            common.Printf( "Opening %s.\n", name );
            fileSize = fileSystem.ReadFile( name, NULL, NULL );
            idFile	*file = fileSystem.OpenFileRead( name );

            if ( !file ) {
                common.Printf( "Couldn't open %s\n", name );
                return;
            }

            targa_header.id_length = ReadByte( file );
            targa_header.colormap_type = ReadByte( file );
            targa_header.image_type = ReadByte( file );

            targa_header.colormap_index = ReadShort( file );
            targa_header.colormap_length = ReadShort( file );
            targa_header.colormap_size = ReadByte( file );
            targa_header.x_origin = ReadShort( file );
            targa_header.y_origin = ReadShort( file );
            targa_header.width = ReadShort( file );
            targa_header.height = ReadShort( file );
            targa_header.pixel_size = ReadByte( file );
            targa_header.attributes = ReadByte( file );

            if ( targa_header.image_type != 2 && targa_header.image_type != 10 && targa_header.image_type != 3 ) {
                common.Error( "LoadTGA( %s ): Only type 2 (RGB), 3 (gray), and 10 (RGB) TGA images supported\n", name );
            }

            if ( targa_header.colormap_type != 0 ) {
                common.Error( "LoadTGA( %s ): colormaps not supported\n", name );
            }

            if ( ( targa_header.pixel_size != 32 && targa_header.pixel_size != 24 ) && targa_header.image_type != 3 ) {
                common.Error( "LoadTGA( %s ): Only 32 or 24 bit images supported (no colormaps)\n", name );
            }

            if ( targa_header.image_type == 2 || targa_header.image_type == 3 ) {
                numBytes = targa_header.width * targa_header.height * ( targa_header.pixel_size >> 3 );
                if ( numBytes > fileSize - 18 - targa_header.id_length ) {
                    common.Error( "LoadTGA( %s ): incomplete file\n", name );
                }
            }

            columns = targa_header.width;

            // skip TARGA image comment
            if ( targa_header.id_length != 0 ) {
                file.Seek( targa_header.id_length, FS_SEEK_CUR );
            }

            megaTextureHeader_t		mtHeader;

            mtHeader.tileSize = TILE_SIZE;
            mtHeader.tilesWide = RoundDownToPowerOfTwo( targa_header.width ) / TILE_SIZE;
            mtHeader.tilesHigh = RoundDownToPowerOfTwo( targa_header.height ) / TILE_SIZE;

            idStr	outName = name;
            outName.StripFileExtension();
            outName += ".mega";

            common.Printf( "Writing %i x %i size %i tiles to %s.\n",
                mtHeader.tilesWide, mtHeader.tilesHigh, mtHeader.tileSize, outName.c_str() );

            // open the output megatexture file
            idFile	*out = fileSystem.OpenFileWrite( outName.c_str() );

            out.Write( &mtHeader, sizeof( mtHeader ) );
            out.Seek( TILE_SIZE * TILE_SIZE * 4, FS_SEEK_SET );

            // we will process this one row of tiles at a time, since the entire thing
            // won't fit in memory
            byte	*targa_rgba = (byte *)R_StaticAlloc( TILE_SIZE * targa_header.width * 4 );

            int blockRowsRemaining = mtHeader.tilesHigh;
            while ( blockRowsRemaining-- ) {
                common.Printf( "%i blockRowsRemaining\n", blockRowsRemaining );
                session.UpdateScreen();

                if ( targa_header.image_type == 2 || targa_header.image_type == 3 )	{
                    // Uncompressed RGB or gray scale image
                    for( row = 0 ; row < TILE_SIZE ; row++ ) {
                        pixbuf = targa_rgba + row*columns*4;
                        for( column = 0; column < columns; column++) {
                            unsigned char red,green,blue,alphabyte;
                            switch( targa_header.pixel_size ) {
                            case 8:
                                blue = ReadByte( file );
                                green = blue;
                                red = blue;
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = 255;
                                break;

                            case 24:
                                blue = ReadByte( file );
                                green = ReadByte( file );
                                red = ReadByte( file );
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = 255;
                                break;
                            case 32:
                                blue = ReadByte( file );
                                green = ReadByte( file );
                                red = ReadByte( file );
                                alphabyte = ReadByte( file );
                                *pixbuf++ = red;
                                *pixbuf++ = green;
                                *pixbuf++ = blue;
                                *pixbuf++ = alphabyte;
                                break;
                            default:
                                common.Error( "LoadTGA( %s ): illegal pixel_size '%d'\n", name, targa_header.pixel_size );
                                break;
                            }
                        }
                    }
                } else if ( targa_header.image_type == 10 ) {   // Runlength encoded RGB images
                    unsigned char red,green,blue,alphabyte,packetHeader,packetSize,j;

                    red = 0;
                    green = 0;
                    blue = 0;
                    alphabyte = 0xff;

                    for( row = 0 ; row < TILE_SIZE ; row++ ) {
                        pixbuf = targa_rgba + row*columns*4;
                        for( column = 0; column < columns; ) {
                            packetHeader= ReadByte( file );
                            packetSize = 1 + (packetHeader & 0x7f);
                            if ( packetHeader & 0x80 ) {        // run-length packet
                                switch( targa_header.pixel_size ) {
                                    case 24:
                                            blue = ReadByte( file );
                                            green = ReadByte( file );
                                            red = ReadByte( file );
                                            alphabyte = 255;
                                            break;
                                    case 32:
                                            blue = ReadByte( file );
                                            green = ReadByte( file );
                                            red = ReadByte( file );
                                            alphabyte = ReadByte( file );
                                            break;
                                    default:
                                        common.Error( "LoadTGA( %s ): illegal pixel_size '%d'\n", name, targa_header.pixel_size );
                                        break;
                                }

                                for( j = 0; j < packetSize; j++ ) {
                                    *pixbuf++=red;
                                    *pixbuf++=green;
                                    *pixbuf++=blue;
                                    *pixbuf++=alphabyte;
                                    column++;
                                    if ( column == columns ) { // run spans across rows
                                        common.Error( "TGA had RLE across columns, probably breaks block" );
                                        column = 0;
                                        if ( row > 0) {
                                            row--;
                                        }
                                        else {
                                            goto breakOut;
                                        }
                                        pixbuf = targa_rgba + row*columns*4;
                                    }
                                }
                            } else {                            // non run-length packet
                                for( j = 0; j < packetSize; j++ ) {
                                    switch( targa_header.pixel_size ) {
                                        case 24:
                                                blue = ReadByte( file );
                                                green = ReadByte( file );
                                                red = ReadByte( file );
                                                *pixbuf++ = red;
                                                *pixbuf++ = green;
                                                *pixbuf++ = blue;
                                                *pixbuf++ = 255;
                                                break;
                                        case 32:
                                                blue = ReadByte( file );
                                                green = ReadByte( file );
                                                red = ReadByte( file );
                                                alphabyte = ReadByte( file );
                                                *pixbuf++ = red;
                                                *pixbuf++ = green;
                                                *pixbuf++ = blue;
                                                *pixbuf++ = alphabyte;
                                                break;
                                        default:
                                            common.Error( "LoadTGA( %s ): illegal pixel_size '%d'\n", name, targa_header.pixel_size );
                                            break;
                                    }
                                    column++;
                                    if ( column == columns ) { // pixel packet run spans across rows
                                        column = 0;
                                        if ( row > 0 ) {
                                            row--;
                                        }
                                        else {
                                            goto breakOut;
                                        }
                                        pixbuf = targa_rgba + row*columns*4;
                                    }
                                }
                            }
                        }
                        breakOut: ;
                    }
                }

                //
                // write out individual blocks from the full row block buffer
                //
                for ( int rowBlock = 0 ; rowBlock < mtHeader.tilesWide ; rowBlock++ ) {
                    for ( int y = 0 ; y < TILE_SIZE ; y++ ) {
                        out.Write( targa_rgba + ( y * targa_header.width + rowBlock * TILE_SIZE ) * 4, TILE_SIZE * 4 );
                    }
                }
            }

            R_StaticFree( targa_rgba );

            GenerateMegaMipMaps( &mtHeader, out );

            delete out;
            delete file;

            GenerateMegaPreview( outName.c_str() );
        #if false
            if ( (targa_header.attributes & (1<<5)) ) 			// image flp bit
                R_VerticalFlip( *pic, *width, *height );
        #endif
        }*/

        #endregion
    }
}