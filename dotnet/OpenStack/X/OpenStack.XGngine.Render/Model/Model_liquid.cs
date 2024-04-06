using System;
using System.NumericsX;
using System.NumericsX.OpenStack;
using static Gengine.Lib;
using static Gengine.Render.TR;
using static System.NumericsX.OpenStack.OpenStack;

namespace Gengine.Render
{
    public unsafe class RenderModelLiquid : RenderModelStatic
    {
        const int LIQUID_MAX_SKIP_FRAMES = 5;
        const int LIQUID_MAX_TYPES = 3;

        int verts_x = 32;
        int verts_y = 32;
        float scale_x = 256f;
        float scale_y = 256f;
        int time = 0;
        int liquid_type = 0;
        int update_tics = 33;  // ~30 hz
        int seed = 0;

        RandomX random = new(0);

        Material shader = declManager.FindMaterial(null);
        DeformInfo deformInfo;        // used to create srfTriangles_t from base frames and new vertexes
        float density = 0.97f;
        float drop_height = 4;
        int drop_radius = 4;
        float drop_delay = 1000;

        float[] pages;
        int page1Start;
        int page2Start;

        DrawVert[] verts;

        int nextDropTime;

        public override void InitFromFile(string fileName)
        {
            int i, x, y; float size_x, size_y, rate;
            Parser parser = new(LEXFL.ALLOWPATHNAMES | LEXFL.NOSTRINGESCAPECHARS);
            int[] tris;

            name = fileName;

            if (!parser.LoadFile(fileName)) { MakeDefaultModel(); return; }

            size_x = scale_x * verts_x;
            size_y = scale_y * verts_y;

            while (parser.ReadToken(out var token))
            {
                if (string.Equals(token, "seed", StringComparison.OrdinalIgnoreCase)) seed = parser.ParseInt();
                else if (string.Equals(token, "size_x", StringComparison.OrdinalIgnoreCase)) size_x = parser.ParseFloat();
                else if (string.Equals(token, "size_y", StringComparison.OrdinalIgnoreCase)) size_y = parser.ParseFloat();
                else if (string.Equals(token, "verts_x", StringComparison.OrdinalIgnoreCase))
                {
                    verts_x = (int)parser.ParseFloat();
                    if (verts_x < 2) { parser.Warning("Invalid # of verts.  Using default model."); MakeDefaultModel(); return; }
                }
                else if (string.Equals(token, "verts_y", StringComparison.OrdinalIgnoreCase))
                {
                    verts_y = (int)parser.ParseFloat();
                    if (verts_y < 2) { parser.Warning("Invalid # of verts.  Using default model."); MakeDefaultModel(); return; }
                }
                else if (string.Equals(token, "liquid_type", StringComparison.OrdinalIgnoreCase))
                {
                    liquid_type = parser.ParseInt() - 1;
                    if (liquid_type < 0 || liquid_type >= LIQUID_MAX_TYPES) { parser.Warning("Invalid liquid_type.  Using default model."); MakeDefaultModel(); return; }
                }
                else if (string.Equals(token, "density", StringComparison.OrdinalIgnoreCase)) density = parser.ParseFloat();
                else if (string.Equals(token, "drop_height", StringComparison.OrdinalIgnoreCase)) drop_height = parser.ParseFloat();
                else if (string.Equals(token, "drop_radius", StringComparison.OrdinalIgnoreCase)) drop_radius = parser.ParseInt();
                else if (string.Equals(token, "drop_delay", StringComparison.OrdinalIgnoreCase)) drop_delay = MathX.SEC2MS(parser.ParseFloat());
                else if (string.Equals(token, "shader", StringComparison.OrdinalIgnoreCase)) { parser.ReadToken(out token); shader = declManager.FindMaterial(token); }
                else if (string.Equals(token, "update_rate", StringComparison.OrdinalIgnoreCase))
                {
                    rate = parser.ParseFloat();
                    if (rate <= 0f || rate > 60f) { parser.Warning("Invalid update_rate.  Must be between 0 and 60.  Using default model."); MakeDefaultModel(); return; }
                    update_tics = (int)(1000 / rate);
                }
                else { parser.Warning($"Unknown parameter '{token}'.  Using default model."); MakeDefaultModel(); return; }
            }

            scale_x = size_x / (verts_x - 1);
            scale_y = size_y / (verts_y - 1);

            pages = new float[2 * verts_x * verts_y];
            page1Start = 0;
            page2Start = verts_x * verts_y;

            verts = new DrawVert[verts_x * verts_y];
            for (i = 0, y = 0; y < verts_y; y++)
                for (x = 0; x < verts_x; x++, i++)
                {
                    //page1[i] = 0f; page2[i] = 0f;
                    verts[i].Clear();
                    verts[i].xyz.Set(x * scale_x, y * scale_y, 0f);
                    verts[i].st.Set((float)x / (float)(verts_x - 1), (float)-y / (float)(verts_y - 1));
                }

            tris = new int[(verts_x - 1) * (verts_y - 1) * 6];
            for (i = 0, y = 0; y < verts_y - 1; y++)
                for (x = 1; x < verts_x; x++, i += 6)
                {
                    tris[i + 0] = y * verts_x + x;
                    tris[i + 1] = y * verts_x + x - 1;
                    tris[i + 2] = (y + 1) * verts_x + x - 1;

                    tris[i + 3] = (y + 1) * verts_x + x - 1;
                    tris[i + 4] = (y + 1) * verts_x + x;
                    tris[i + 5] = y * verts_x + x;
                }

            // build the information that will be common to all animations of this mesh:
            // sil edge connectivity and normal / tangent generation information
            deformInfo = R_BuildDeformInfo(verts.Length, verts, tris.Length, tris, true);

            bounds.Clear();
            bounds.AddPoint(new Vector3(0f, 0f, drop_height * -10f));
            bounds.AddPoint(new Vector3((verts_x - 1) * scale_x, (verts_y - 1) * scale_y, drop_height * 10f));

            // set the timestamp for reloadmodels
            fileSystem.ReadFile(name, out timeStamp);

            Reset();
        }

        public override DynamicModel IsDynamicModel
            => DynamicModel.DM_CONTINUOUS;

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            RenderModelStatic staticModel; int frames, t; float lerp;

            if (cachedModel != null) cachedModel = null;

            if (deformInfo == null) return null;

            t = view == null ? 0 : view.renderView.time;

            // update the liquid model
            frames = (t - time) / update_tics;
            if (frames > LIQUID_MAX_SKIP_FRAMES)
            {
                // don't let time accumalate when skipping frames
                time += update_tics * (frames - LIQUID_MAX_SKIP_FRAMES);

                frames = LIQUID_MAX_SKIP_FRAMES;
            }

            while (frames > 0) { Update(); frames--; }

            // create the surface
            lerp = (t - time) / (float)update_tics;
            var surf = GenerateSurface(lerp);

            staticModel = new RenderModelStatic();
            staticModel.AddSurface(surf);
            staticModel.bounds = surf.geometry.bounds;

            return staticModel;
        }

        public override Bounds Bounds(RenderEntity ent)
            => bounds;

        public override void Reset()
        {
            int i, x, y;

            if (pages.Length < (2 * verts_x * verts_y)) return;

            nextDropTime = 0;
            time = 0;
            random.Seed = seed;

            page1Start = 0; page2Start = verts_x * verts_y;

            Span<float> page1 = pages.AsSpan(page1Start), page2 = pages.AsSpan(page2Start);
            for (i = 0, y = 0; y < verts_y; y++)
                for (x = 0; x < verts_x; x++, i++) { page1[i] = 0f; page2[i] = 0f; verts[i].xyz.z = 0f; }
        }

        public void IntersectBounds(Bounds bounds, float displacement)
        {
            int left = (int)(bounds[0].x / scale_x),
                right = (int)(bounds[1].x / scale_x),
                top = (int)(bounds[0].y / scale_y),
                bottom = (int)(bounds[1].y / scale_y);
            float down = bounds[0].z;

            if (right < 1 || left >= verts_x || bottom < 1 || top >= verts_x) return;

            // Perform edge clipping...
            if (left < 1) left = 1;
            if (right >= verts_x) right = verts_x - 1;
            if (top < 1) top = 1;
            if (bottom >= verts_y) bottom = verts_y - 1;

            Span<float> page1 = pages.AsSpan(page1Start);
            for (var cy = top; cy < bottom; cy++)
                for (var cx = left; cx < right; cx++) { ref float pos = ref page1[verts_x * cy + cx]; if (pos > down) pos = down; }
        }

        ModelSurface GenerateSurface(float lerp)
        {
            SrfTriangles tri; int i, base_; float inv_lerp;

            Span<float> page1 = pages.AsSpan(page1Start), page2 = pages.AsSpan(page2Start);
            inv_lerp = 1f - lerp;
            fixed (DrawVert* vertsV = verts)
            {
                DrawVert* vert = vertsV;
                for (i = 0; i < verts.Length; i++, vert++) vert->xyz.z = page1[i] * lerp + page2[i] * inv_lerp;

                tr.pc.c_deformedSurfaces++;
                tr.pc.c_deformedVerts += deformInfo.numOutputVerts;
                tr.pc.c_deformedIndexes += deformInfo.numIndexes;

                tri = R_AllocStaticTriSurf();

                // note that some of the data is references, and should not be freed
                tri.deformedSurface = true;

                tri.numIndexes = deformInfo.numIndexes;
                tri.indexes = deformInfo.indexes;
                tri.silIndexes = deformInfo.silIndexes;
                tri.numMirroredVerts = deformInfo.numMirroredVerts;
                tri.mirroredVerts = deformInfo.mirroredVerts;
                tri.numDupVerts = deformInfo.numDupVerts;
                tri.dupVerts = deformInfo.dupVerts;
                tri.numSilEdges = deformInfo.numSilEdges;
                tri.silEdges = deformInfo.silEdges;
                tri.dominantTris = deformInfo.dominantTris;

                tri.numVerts = deformInfo.numOutputVerts;
                R_AllocStaticTriSurfVerts(tri, tri.numVerts);
                Simd.Memcpy(tri.verts, vertsV, deformInfo.numSourceVerts * sizeof(DrawVert));

                // replicate the mirror seam vertexes
                base_ = deformInfo.numOutputVerts - deformInfo.numMirroredVerts;
                for (i = 0; i < deformInfo.numMirroredVerts; i++) tri.verts[base_ + i] = tri.verts[deformInfo.mirroredVerts[i]];

                R_BoundTriSurf(tri);

                // If a surface is going to be have a lighting interaction generated, it will also have to call R_DeriveTangents() to get normals, tangents, and face planes.  If it only
                // needs shadows generated, it will only have to generate face planes.  If it only has ambient drawing, or is culled, no additional work will be necessary
                if (!r_useDeferredTangents.Bool) R_DeriveTangents(tri); // set face planes, vertex normals, tangents
            }

            return new ModelSurface
            {
                geometry = tri,
                shader = shader
            };
        }

        void WaterDrop(int x, int y, int pageStart)
        {
            int square, radsquare = drop_radius * drop_radius; float invlength = 1f / radsquare, dist;

            if (x < 0) x = 1 + drop_radius + random.RandomInt(verts_x - 2 * drop_radius - 1);
            if (y < 0) y = 1 + drop_radius + random.RandomInt(verts_y - 2 * drop_radius - 1);

            int left = -drop_radius,
                right = drop_radius,
                top = -drop_radius,
                bottom = drop_radius;

            // Perform edge clipping...
            if (x - drop_radius < 1) left -= (x - drop_radius - 1);
            if (y - drop_radius < 1) top -= (y - drop_radius - 1);
            if (x + drop_radius > verts_x - 1) right -= (x + drop_radius - verts_x + 1);
            if (y + drop_radius > verts_y - 1) bottom -= (y + drop_radius - verts_y + 1);

            var page = pages.AsSpan(pageStart);
            for (var cy = top; cy < bottom; cy++)
                for (var cx = left; cx < right; cx++)
                {
                    square = cy * cy + cx * cx;
                    if (square < radsquare)
                    {
                        dist = MathX.Sqrt(square * invlength);
                        page[verts_x * (cy + y) + cx + x] += MathX.Cos16((float)(dist * Math.PI * 0.5f)) * drop_height;
                    }
                }
        }

        void Update()
        {
            int x, y; float value;

            time += update_tics;

            UnsafeX.Swap(ref page1Start, ref page2Start);

            if (time > nextDropTime) { WaterDrop(-1, -1, page2Start); nextDropTime = (int)(time + drop_delay); }
            else if (time < nextDropTime - drop_delay) nextDropTime = (int)(time + drop_delay);

            fixed (float* pagesF = pages)
            {
                float* p1 = &pagesF[page1Start], p2 = &pagesF[page2Start];

                switch (liquid_type)
                {
                    case 0:
                        for (y = 1; y < verts_y - 1; y++)
                        {
                            p2 += verts_x;
                            p1 += verts_x;
                            for (x = 1; x < verts_x - 1; x++)
                            {
                                value =
                                    (p2[x + verts_x] +
                                      p2[x - verts_x] +
                                      p2[x + 1] +
                                      p2[x - 1] +
                                      p2[x - verts_x - 1] +
                                      p2[x - verts_x + 1] +
                                      p2[x + verts_x - 1] +
                                      p2[x + verts_x + 1] +
                                      p2[x]) * (2f / 9f) -
                                    p1[x];

                                p1[x] = value * density;
                            }
                        }
                        break;

                    case 1:
                        for (y = 1; y < verts_y - 1; y++)
                        {
                            p2 += verts_x;
                            p1 += verts_x;
                            for (x = 1; x < verts_x - 1; x++)
                            {
                                value =
                                    (p2[x + verts_x] +
                                      p2[x - verts_x] +
                                      p2[x + 1] +
                                      p2[x - 1] +
                                      p2[x - verts_x - 1] +
                                      p2[x - verts_x + 1] +
                                      p2[x + verts_x - 1] +
                                      p2[x + verts_x + 1]) * 0.25f -
                                    p1[x];

                                p1[x] = value * density;
                            }
                        }
                        break;

                    case 2:
                        for (y = 1; y < verts_y - 1; y++)
                        {
                            p2 += verts_x;
                            p1 += verts_x;
                            for (x = 1; x < verts_x - 1; x++)
                            {
                                value =
                                    (p2[x + verts_x] +
                                      p2[x - verts_x] +
                                      p2[x + 1] +
                                      p2[x - 1] +
                                      p2[x - verts_x - 1] +
                                      p2[x - verts_x + 1] +
                                      p2[x + verts_x - 1] +
                                      p2[x + verts_x + 1] +
                                      p2[x]) * (1f / 9f);

                                p1[x] = value * density;
                            }
                        }
                        break;
                }
            }
        }
    }
}
