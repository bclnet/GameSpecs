using System.Collections.Generic;
using static System.NumericsX.OpenStack.OpenStack;

namespace System.NumericsX.OpenStack
{
    public class MapPrimitive
    {
        public enum TYPE
        {
            INVALID = -1, BRUSH,
            PATCH
        }

        protected TYPE type = TYPE.INVALID;
        public Dictionary<string, string> epairs = new();

        public TYPE Type => type;
    }

    public class MapBrushSide
    {
        protected internal string material;
        protected internal Plane plane;
        protected internal Vector3 texMat0;
        protected internal Vector3 texMat1;
        protected internal Vector3 origin;

        public MapBrushSide()
        {
            plane.Zero();
            texMat0.Zero();
            texMat1.Zero();
            origin.Zero();
        }

        public string Material
        {
            get => material;
            set => material = value;
        }

        public Plane Plane
        {
            get => plane;
            set => plane = value;
        }

        public void SetTextureMatrix(Vector3[] mat) { texMat0 = mat[0]; texMat1 = mat[1]; }
        public void GetTextureMatrix(Vector3 mat1, Vector3 mat2) { mat1 = texMat0; mat2 = texMat1; }
        public void GetTextureVectors(Vector4[] v)
        {
            Compute.ComputeAxisBase(plane.Normal, out var texX, out var texY);
            // unroll
            v[0].x = texX.x * texMat0.x + texY.x * texMat0.y;
            v[0].y = texX.y * texMat0.x + texY.y * texMat0.y;
            v[0].z = texX.z * texMat0.x + texY.z * texMat0.y;
            v[0].w = texMat0.z + (origin * v[0].ToVec3());
            //
            v[1].x = texX.x * texMat1.x + texY.x * texMat1.y;
            v[1].y = texX.y * texMat1.x + texY.y * texMat1.y;
            v[1].z = texX.z * texMat1.x + texY.z * texMat1.y;
            v[1].w = texMat1.z + (origin * v[1].ToVec3());
        }
    }

    public class MapBrush : MapPrimitive
    {
        protected int numSides;
        protected List<MapBrushSide> sides = new();

        public MapBrush() { type = TYPE.BRUSH; sides.Resize(8, 4); }
        public unsafe static MapBrush Parse(Lexer src, Vector3 origin, bool newFormat = true, float version = MapFile.CURRENT_MAP_VERSION)
        {
            int i;
            Vector3 planepts0 = new();
            Vector3 planepts1 = new();
            Vector3 planepts2 = new();
            Token token;
            List<MapBrushSide> sides = new();
            MapBrushSide side;
            Dictionary<string, string> epairs = new();

            if (!src.ExpectTokenString("{")) return null;
            do
            {
                if (!src.ReadToken(out token)) { src.Error("MapBrush::Parse: unexpected EOF"); sides.Clear(); return null; }
                if (token == "}") break;

                // here we may have to jump over brush epairs ( only used in editor )
                do
                {
                    // if token is a brace
                    if (token == "(") break;
                    // the token should be a key string for a key/value pair
                    if (token.type != TT.STRING) { src.Error($"MapBrush::Parse: unexpected {token}, expected ( or epair key string"); sides.Clear(); return null; }

                    var key = (string)token;
                    if (!src.ReadTokenOnLine(out token) || token.type != TT.STRING) { src.Error("MapBrush::Parse: expected epair value string not found"); sides.Clear(); return null; }
                    epairs[key] = token;

                    // try to read the next key
                    if (!src.ReadToken(out token)) { src.Error("MapBrush::Parse: unexpected EOF"); sides.Clear(); return null; }
                } while (true);

                src.UnreadToken(token);

                side = new MapBrushSide();
                sides.Add(side);

                if (newFormat)
                {
                    fixed (float* _ = &side.plane.a)
                        if (!src.Parse1DMatrix(4, _))
                        {
                            src.Error("MapBrush::Parse: unable to read brush side plane definition");
                            sides.Clear();
                            return null;
                        }
                }
                else
                {
                    // read the three point plane definition
                    if (!src.Parse1DMatrix(3, &planepts0.x) || !src.Parse1DMatrix(3, &planepts1.x) || !src.Parse1DMatrix(3, &planepts2.x))
                    {
                        src.Error("MapBrush::Parse: unable to read brush side plane definition");
                        sides.Clear();
                        return null;
                    }

                    planepts0 -= origin;
                    planepts1 -= origin;
                    planepts2 -= origin;

                    side.plane.FromPoints(planepts0, planepts1, planepts2);
                }

                // read the texture matrix. this is odd, because the texmat is 2D relative to default planar texture axis
                fixed (float* _ = &side.texMat0.x)
                    if (!src.Parse2DMatrix(2, 3, _)) { src.Error("MapBrush::Parse: unable to read brush side texture matrix"); sides.Clear(); return null; }
                side.origin = origin;

                // read the material
                if (!src.ReadTokenOnLine(out token)) { src.Error("MapBrush::Parse: unable to read brush side material"); sides.Clear(); return null; }

                // we had an implicit 'textures/' in the old format...
                side.material = version < 2f ? "textures/" + token : token;

                // Q2 allowed override of default flags and values, but we don't any more
                if (src.ReadTokenOnLine(out token) && src.ReadTokenOnLine(out token) && src.ReadTokenOnLine(out token)) { }
            } while (true);

            if (!src.ExpectTokenString("}")) { sides.Clear(); return null; }

            var brush = new MapBrush();
            for (i = 0; i < sides.Count; i++)
                brush.AddSide(sides[i]);

            brush.epairs = epairs;

            return brush;
        }
        public unsafe static MapBrush ParseQ3(Lexer src, Vector3 origin)
        {
            int i;
            Vector3 planepts0 = new();
            Vector3 planepts1 = new();
            Vector3 planepts2 = new();
            Token token;
            List<MapBrushSide> sides = new();
            MapBrushSide side;
            Dictionary<string, string> epairs = new();

            do
            {
                if (src.CheckTokenString("}")) break;

                side = new MapBrushSide();
                sides.Add(side);

                // read the three point plane definition
                if (!src.Parse1DMatrix(3, &planepts0.x) || !src.Parse1DMatrix(3, &planepts1.x) || !src.Parse1DMatrix(3, &planepts2.x))
                {
                    src.Error("MapBrush::ParseQ3: unable to read brush side plane definition");
                    sides.Clear();
                    return null;
                }

                planepts0 -= origin;
                planepts1 -= origin;
                planepts2 -= origin;

                side.plane.FromPoints(planepts0, planepts1, planepts2);

                // read the material
                if (!src.ReadTokenOnLine(out token)) { src.Error("MapBrush::ParseQ3: unable to read brush side material"); sides.Clear(); return null; }

                // we have an implicit 'textures/' in the old format
                side.material = "textures/" + token;

                // skip the texture shift, rotate and scale
                src.ParseInt();
                src.ParseInt();
                src.ParseInt();
                src.ParseFloat();
                src.ParseFloat();
                side.texMat0 = new Vector3(0.03125f, 0.0f, 0.0f);
                side.texMat1 = new Vector3(0.0f, 0.03125f, 0.0f);
                side.origin = origin;

                // Q2 allowed override of default flags and values, but we don't any more
                if (src.ReadTokenOnLine(out token) && src.ReadTokenOnLine(out token) && src.ReadTokenOnLine(out token)) { }
            } while (true);

            var brush = new MapBrush();
            for (i = 0; i < sides.Count; i++) brush.AddSide(sides[i]);

            brush.epairs = epairs;

            return brush;
        }
        public bool Write(VFile fp, int primitiveNum, Vector3 origin)
        {
            int i;
            MapBrushSide side;

            fp.WriteFloatString($"// primitive {primitiveNum}\n{{\n brushDef3\n {{\n");

            // write brush epairs
            foreach (var epair in epairs) fp.WriteFloatString($"  \"{epair.Key}\" \"{epair.Value}\"\n");

            // write brush sides
            for (i = 0; i < NumSides; i++)
            {
                side = GetSide(i);
                fp.WriteFloatString($"  ( {side.plane.a} {side.plane.b} {side.plane.c} {side.plane.d} ) ");
                fp.WriteFloatString($"( ( {side.texMat0.x} {side.texMat0.y} {side.texMat0.z} ) ( {side.texMat1.x} {side.texMat1.y} {side.texMat1.z} ) ) \"{side.material}\" 0 0 0\n");
            }

            fp.WriteFloatString(" }\n}\n");

            return true;
        }
        public int NumSides => sides.Count;
        public int AddSide(MapBrushSide side) => sides.Add_(side);
        public MapBrushSide GetSide(int i) => sides[i];
        public uint GeometryCRC
        {
            get
            {
                var crc = 0U;
                for (var i = 0; i < NumSides; i++)
                {
                    var mapSide = GetSide(i);
                    for (var j = 0; j < 4; j++) crc ^= Compute.FloatCRC(mapSide.Plane[j]);
                    crc ^= Compute.StringCRC(mapSide.Material);
                }
                return crc;
            }
        }
    }

    public class MapPatch : MapPrimitive
    {
        public Surface_Patch Patch;

        protected string material;
        protected int horzSubdivisions;
        protected int vertSubdivisions;
        protected bool explicitSubdivisions;

        public MapPatch()
        {
            type = TYPE.PATCH;
            horzSubdivisions = vertSubdivisions = 0;
            explicitSubdivisions = false;
            Patch = new();
        }
        public MapPatch(int maxPatchWidth, int maxPatchHeight)
        {
            type = TYPE.PATCH;
            horzSubdivisions = vertSubdivisions = 0;
            explicitSubdivisions = false;
            Patch = new(maxPatchWidth, maxPatchHeight);
        }

        public static MapPatch Parse(Lexer src, Vector3 origin, bool patchDef3 = true, float version = MapFile.CURRENT_MAP_VERSION)
        {
            var info = new float[7];
            DrawVert vert;
            Token token;
            int i, j;

            if (!src.ExpectTokenString("{")) return null;

            // read the material (we had an implicit 'textures/' in the old format...)
            if (!src.ReadToken(out token)) { src.Error("MapPatch::Parse: unexpected EOF"); return null; }

            // Parse it
            if (patchDef3)
            {
                if (!src.Parse1DMatrix(7, info)) { src.Error("MapPatch::Parse: unable to Parse patchDef3 info"); return null; }
            }
            else
                if (!src.Parse1DMatrix(5, info)) { src.Error("MapPatch::Parse: unable to parse patchDef2 info"); return null; }

            var patch = new MapPatch((int)info[0], (int)info[1]);
            var patch2 = patch.Patch;
            patch2.SetSize((int)info[0], (int)info[1]);
            patch.Material = version < 2f ? $"textures/{token}" : token;

            if (patchDef3)
            {
                patch.HorzSubdivisions = (int)info[2];
                patch.VertSubdivisions = (int)info[3];
                patch.ExplicitlySubdivided = true;
            }

            if (patch2.Width < 0 || patch2.Height < 0) { src.Error("MapPatch::Parse: bad size"); return null; }

            // these were written out in the wrong order, IMHO
            if (!src.ExpectTokenString("(")) { src.Error("MapPatch::Parse: bad patch vertex data"); return null; }
            var v = new float[5];
            for (j = 0; j < patch2.Width; j++)
            {
                if (!src.ExpectTokenString("(")) { src.Error("MapPatch::Parse: bad vertex row data"); return null; }
                for (i = 0; i < patch2.Height; i++)
                {
                    if (!src.Parse1DMatrix(5, v)) { src.Error("MapPatch::Parse: bad vertex column data"); return null; }

                    vert = patch.Patch[i * patch2.Width + j];
                    vert.xyz.x = v[0] - origin.x;
                    vert.xyz.y = v[1] - origin.y;
                    vert.xyz.z = v[2] - origin.z;
                    vert.st.x = v[3];
                    vert.st.y = v[4];
                }
                if (!src.ExpectTokenString(")")) { src.Error("MapPatch::Parse: unable to parse patch control points"); return null; }
            }
            if (!src.ExpectTokenString(")")) { src.Error("MapPatch::Parse: unable to parse patch control points, no closure"); return null; }

            // read any key/value pairs
            while (src.ReadToken(out token))
            {
                if (token == "}") { src.ExpectTokenString("}"); break; }
                if (token.type == TT.STRING)
                {
                    var key = (string)token;
                    src.ExpectTokenType(TT.STRING, 0, out token);
                    patch.epairs[key] = token;
                }
            }

            return patch;
        }

        public bool Write(VFile fp, int primitiveNum, Vector3 origin)
        {
            int i, j;
            DrawVert v;

            if (ExplicitlySubdivided)
            {
                fp.WriteFloatString($"// primitive {primitiveNum}\n{{\n patchDef3\n {{\n");
                fp.WriteFloatString($"  \"{Material}\"\n  ( {Patch.Width} {Patch.Height} {HorzSubdivisions} {VertSubdivisions} 0 0 0 )\n");
            }
            else
            {
                fp.WriteFloatString($"// primitive {primitiveNum}\n{{\n patchDef2\n {{\n");
                fp.WriteFloatString($"  \"{Material}\"\n  ( {Patch.Width} {Patch.Height} 0 0 0 )\n");
            }

            fp.WriteFloatString("  (\n");
            for (i = 0; i < Patch.Width; i++)
            {
                fp.WriteFloatString("   ( ");
                for (j = 0; j < Patch.Height; j++)
                {
                    v = Patch[j * Patch.Width + i];
                    fp.WriteFloatString($" ( {v.xyz.x + origin.x} {v.xyz.y + origin.y} {v.xyz.z + origin.y} {v.st.x} {v.st.y} )");
                }
                fp.WriteFloatString(" )\n");
            }
            fp.WriteFloatString("  )\n }\n}\n");

            return true;
        }

        public string Material
        {
            get => material;
            set => material = value;
        }
        public int HorzSubdivisions
        {
            get => horzSubdivisions;
            set => horzSubdivisions = value;
        }
        public int VertSubdivisions
        {
            get => vertSubdivisions;
            set => vertSubdivisions = value;
        }

        public bool ExplicitlySubdivided
        {
            get => explicitSubdivisions;
            set => explicitSubdivisions = value;
        }
        public uint GeometryCRC
        {
            get
            {
                var crc = (uint)(HorzSubdivisions ^ VertSubdivisions);
                for (var i = 0; i < Patch.Width; i++)
                    for (var j = 0; j < Patch.Height; j++)
                    {
                        crc ^= Compute.FloatCRC(Patch[j * Patch.Width + i].xyz.x);
                        crc ^= Compute.FloatCRC(Patch[j * Patch.Width + i].xyz.y);
                        crc ^= Compute.FloatCRC(Patch[j * Patch.Width + i].xyz.z);
                    }
                crc ^= Compute.StringCRC(Material);
                return crc;
            }
        }
    }

    public class MapEntity
    {
        protected internal List<MapPrimitive> primitives = new();

        public Dictionary<string, string> epairs = new();

        public static MapEntity Parse(Lexer src, bool worldSpawn = false, float version = MapFile.CURRENT_MAP_VERSION)
        {
            MapPatch mapPatch; MapBrush mapBrush;

            if (!src.ReadToken(out var token)) return null;

            if (token != "{") { src.Error($"MapEntity::Parse: {{ not found, found {token}"); return null; }

            var mapEnt = new MapEntity();
            if (worldSpawn) mapEnt.primitives.Resize(1024, 256);

            var origin = new Vector3();
            var worldent = false;
            do
            {
                if (!src.ReadToken(out token)) { src.Error("MapEntity::Parse: EOF without closing brace"); return null; }
                if (token == "}") break;
                if (token == "{")
                {
                    // parse a brush or patch
                    if (!src.ReadToken(out token)) { src.Error("MapEntity::Parse: unexpected EOF"); return null; }
                    if (worldent) origin.Zero();
                    // if is it a brush: brush, brushDef, brushDef2, brushDef3
                    var tokenS = (string)token;
                    if (tokenS.StartsWith("brush", StringComparison.OrdinalIgnoreCase))
                    {
                        mapBrush = MapBrush.Parse(src, origin, (string.Equals(token, "brushDef2", StringComparison.OrdinalIgnoreCase) || string.Equals(token, "brushDef3", StringComparison.OrdinalIgnoreCase)), version);
                        if (mapBrush == null) return null;
                        mapEnt.AddPrimitive(mapBrush);
                    }
                    // if is it a patch: patchDef2, patchDef3
                    else if (tokenS.StartsWith("patch", StringComparison.OrdinalIgnoreCase))
                    {
                        mapPatch = MapPatch.Parse(src, origin, string.Equals(token, "patchDef3", StringComparison.OrdinalIgnoreCase), version);
                        if (mapPatch == null) return null;
                        mapEnt.AddPrimitive(mapPatch);
                    }
                    // assume it's a brush in Q3 or older style
                    else
                    {
                        src.UnreadToken(token);
                        mapBrush = MapBrush.ParseQ3(src, origin);
                        if (mapBrush == null) return null;
                        mapEnt.AddPrimitive(mapBrush);
                    }
                }
                else
                {
                    // parse a key / value pair
                    var key = (string)token;
                    src.ReadTokenOnLine(out token);
                    var value = (string)token;

                    // strip trailing spaces that sometimes get accidentally added in the editor
                    value = value.StripTrailingWhitespace();
                    key = key.StripTrailingWhitespace();

                    mapEnt.epairs[key] = value;

                    if (string.Equals(key, "origin", StringComparison.OrdinalIgnoreCase))
                    {
                        // scanf into doubles, then assign, so it is idVec size independent
                        TextScanFormatted.Scan(value, "%lf %lf %lf", out double v1, out double v2, out double v3);
                        origin.x = (float)v1;
                        origin.y = (float)v2;
                        origin.z = (float)v3;
                    }
                    else if (string.Equals(key, "classname", StringComparison.OrdinalIgnoreCase) && string.Equals(value, "worldspawn", StringComparison.OrdinalIgnoreCase)) worldent = true;
                }
            } while (true);

            return mapEnt;
        }
        public bool Write(VFile fp, int entityNum)
        {
            fp.WriteFloatString($"// entity {entityNum}\n{{\n");

            // write entity epairs
            foreach (var epair in epairs) fp.WriteFloatString($"\"{epair.Key}\" \"{epair.Value}\"\n");

            epairs.TryGetVector("origin", "0 0 0", out var origin);

            // write pritimives
            for (var i = 0; i < NumPrimitives; i++)
            {
                var mapPrim = GetPrimitive(i);
                switch (mapPrim.Type)
                {
                    case MapPrimitive.TYPE.BRUSH: ((MapBrush)mapPrim).Write(fp, i, origin); break;
                    case MapPrimitive.TYPE.PATCH: ((MapPatch)mapPrim).Write(fp, i, origin); break;
                }
            }

            fp.WriteFloatString("}\n");

            return true;
        }
        public int NumPrimitives => primitives.Count;
        public MapPrimitive GetPrimitive(int i) => primitives[i];
        public void AddPrimitive(MapPrimitive p) => primitives.Add_(p);
        public uint GeometryCRC
        {
            get
            {
                var crc = 0U;
                for (var i = 0; i < NumPrimitives; i++)
                {
                    var mapPrim = GetPrimitive(i);
                    switch (mapPrim.Type)
                    {
                        case MapPrimitive.TYPE.BRUSH: crc ^= ((MapBrush)mapPrim).GeometryCRC; break;
                        case MapPrimitive.TYPE.PATCH: crc ^= ((MapPatch)mapPrim).GeometryCRC; break;
                    }
                }
                return crc;
            }
        }
        public void RemovePrimitiveData()
            => primitives.Clear();
    }

    public class MapFile
    {
        internal const int OLD_MAP_VERSION = 1;
        internal const int CURRENT_MAP_VERSION = 2;
        internal const int DEFAULT_CURVE_SUBDIVISION = 4;
        internal const float DEFAULT_CURVE_MAX_ERROR = 4f;
        internal const float DEFAULT_CURVE_MAX_ERROR_CD = 24f;
        internal const float DEFAULT_CURVE_MAX_LENGTH = -1f;
        internal const float DEFAULT_CURVE_MAX_LENGTH_CD = -1f;

        protected float version;
        protected DateTime fileTime;
        protected uint geometryCRC;
        protected List<MapEntity> entities = new();
        protected string name;
        protected bool hasPrimitiveData;

        public MapFile()
        {
            version = CURRENT_MAP_VERSION;
            fileTime = DateTime.MinValue;
            geometryCRC = 0;
            entities.Resize(1024, 256);
            hasPrimitiveData = false;
        }

        // filename does not require an extension
        // normally this will use a .reg file instead of a .map file if it exists, which is what the game and dmap want, but the editor will want to always load a .map file
        public bool Parse(string filename, bool ignoreRegion = false, bool osPath = false)
        {
            // no string concatenation for epairs and allow path names for materials
            var src = new Lexer(LEXFL.NOSTRINGCONCAT | LEXFL.NOSTRINGESCAPECHARS | LEXFL.ALLOWPATHNAMES);
            Token token;
            MapEntity mapEnt;
            int i, j, k;

            var name = PathX.StripFileExtension(filename);
            var fullName = name;
            hasPrimitiveData = false;

            if (!ignoreRegion)
            {
                // try loading a .reg file first
                fullName = PathX.SetFileExtension(fullName, "reg");
                src.LoadFile(fullName, osPath);
            }

            if (!src.IsLoaded)
            {
                // now try a .map file
                fullName = PathX.SetFileExtension(fullName, "map");
                src.LoadFile(fullName, osPath);
                // didn't get anything at all
                if (!src.IsLoaded) return false;
            }

            version = OLD_MAP_VERSION;
            fileTime = src.FileTime;
            entities.Clear();

            if (src.CheckTokenString("Version"))
            {
                src.ReadTokenOnLine(out token);
                version = token.FloatValue;
            }

            while (true)
            {
                mapEnt = MapEntity.Parse(src, entities.Count == 0, version);
                if (mapEnt == null) break;
                entities.Add(mapEnt);
            }

            SetGeometryCRC();

            // if the map has a worldspawn
            if (entities.Count != 0)
            {
                // "removeEntities" "classname" can be set in the worldspawn to remove all entities with the given classname
                var removeEntities = entities[0].epairs.MatchPrefix("removeEntities", default);
                while (removeEntities.Key != null)
                {
                    RemoveEntities(removeEntities.Value);
                    removeEntities = entities[0].epairs.MatchPrefix("removeEntities", removeEntities);
                }

                // "overrideMaterial" "material" can be set in the worldspawn to reset all materials
                if (entities[0].epairs.TryGetString("overrideMaterial", "", out var material))
                {
                    for (i = 0; i < entities.Count; i++)
                    {
                        mapEnt = entities[i];
                        for (j = 0; j < mapEnt.NumPrimitives; j++)
                        {
                            var mapPrimitive = mapEnt.GetPrimitive(j);
                            switch (mapPrimitive.Type)
                            {
                                case MapPrimitive.TYPE.BRUSH: var mapBrush = (MapBrush)mapPrimitive; for (k = 0; k < mapBrush.NumSides; k++) mapBrush.GetSide(k).Material = material; break;
                                case MapPrimitive.TYPE.PATCH: ((MapPatch)mapPrimitive).Material = material; break;
                            }
                        }
                    }
                }

                // force all entities to have a name key/value pair
                if (entities[0].epairs.GetBool("forceEntityNames"))
                    for (i = 1; i < entities.Count; i++)
                    {
                        mapEnt = entities[i];
                        if (!mapEnt.epairs.ContainsKey("name")) mapEnt.epairs["name"] = $"{mapEnt.epairs.GetString("classname", "forcedName")}{i}";
                    }

                // move the primitives of any func_group entities to the worldspawn
                if (entities[0].epairs.GetBool("moveFuncGroups"))
                    for (i = 1; i < entities.Count; i++)
                    {
                        mapEnt = entities[i];
                        if (string.Equals(mapEnt.epairs.GetString("classname"), "func_group", StringComparison.OrdinalIgnoreCase))
                        {
                            entities[0].primitives.AddRange(mapEnt.primitives);
                            mapEnt.primitives.Clear();
                        }
                    }
            }

            hasPrimitiveData = true;
            return true;
        }

        public bool Write(string fileName, string ext, bool fromBasePath = true)
        {
            var qpath = PathX.SetFileExtension(fileName, ext);

            common.Printf("writing {qpath}...\n");

            var fp = fromBasePath
                ? fileSystem.OpenFileWrite(qpath, "fs_devpath")
                : fileSystem.OpenExplicitFileWrite(qpath);
            if (fp == null) { common.Warning($"Couldn't open {qpath}\n"); return false; }

            fp.WriteFloatString($"Version {(float)CURRENT_MAP_VERSION}\n");

            for (var i = 0; i < entities.Count; i++) entities[i].Write(fp, i);

            fileSystem.CloseFile(fp);

            return true;
        }

        void SetGeometryCRC()
        {
            var geometryCRC = 0U;
            for (var i = 0; i < entities.Count; i++) geometryCRC ^= entities[i].GeometryCRC;
        }

        // get the number of entities in the map
        public int NumEntities => entities.Count;
        // get the specified entity
        public MapEntity GetEntity(int i) => entities[i];
        // get the name without file extension
        public string Name => name;
        // get the file time
        public DateTime FileTime => fileTime;
        // get CRC for the map geometry. texture coordinates and entity key/value pairs are not taken into account
        public uint GeometryCRC => geometryCRC;
        // returns true if the file on disk changed
        public bool NeedsReload => name.Length == 0 || fileSystem.ReadFile(name, out var time) <= 0 || time > fileTime;

        public int AddEntity(MapEntity ent)
            => entities.Add_(ent);

        public MapEntity FindEntity(string name)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var ent = entities[i];
                if (string.Equals(ent.epairs.GetString("name"), name, StringComparison.OrdinalIgnoreCase)) return ent;
            }
            return null;
        }
        public void RemoveEntity(MapEntity mapEnt)
            => entities.Remove(mapEnt);
        public void RemoveEntities(string classname)
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var ent = entities[i];
                if (string.Equals(ent.epairs.GetString("classname"), classname, StringComparison.OrdinalIgnoreCase)) { entities.RemoveAt(i); i--; }
            }
        }
        public void RemoveAllEntities()
        {
            entities.Clear();
            hasPrimitiveData = false;
        }
        public void RemovePrimitiveData()
        {
            for (var i = 0; i < entities.Count; i++)
            {
                var ent = entities[i];
                ent.RemovePrimitiveData();
            }
            hasPrimitiveData = false;
        }
        public bool HasPrimitiveData => hasPrimitiveData;
    }

    static class Compute
    {
        public static unsafe uint FloatCRC(float f) => *(uint*)&f;

        public static uint StringCRC(string str)
        {
            var crc = 0U;
            for (var i = 0; i < str.Length; i++) crc ^= (uint)(str[i] << (i & 3));
            return crc;
        }

        // WARNING : special case behaviour of atan2(y,x) <. atan(y/x) might not be the same everywhere when x == 0 rotation by (0,RotY,RotZ) assigns X to normal
        public static void ComputeAxisBase(Vector3 normal, out Vector3 texS, out Vector3 texT)
        {
            // do some cleaning
            Vector3 n;
            n.x = (MathX.Fabs(normal.x) < 1e-6f) ? 0f : normal.x;
            n.y = (MathX.Fabs(normal.y) < 1e-6f) ? 0f : normal.y;
            n.z = (MathX.Fabs(normal.z) < 1e-6f) ? 0f : normal.z;

            var RotY = (float)-Math.Atan2(n.z, MathX.Sqrt(n.y * n.y + n.x * n.x));
            var RotZ = (float)-Math.Atan2(n.y, n.x);
            // rotate (0,1,0) and (0,0,1) to compute texS and texT
            texS.x = (float)-Math.Sin(RotZ);
            texS.y = (float)Math.Cos(RotZ);
            texS.z = 0;
            // the texT vector is along -Z ( T texture coorinates axis )
            texT.x = (float)-Math.Sin(RotY) * (float)Math.Cos(RotZ);
            texT.y = (float)-Math.Sin(RotY) * (float)Math.Sin(RotZ);
            texT.z = (float)-Math.Cos(RotY);
        }
    }
}