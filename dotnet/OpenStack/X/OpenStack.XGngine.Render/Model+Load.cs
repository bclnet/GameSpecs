using System.Collections.Generic;
using System.IO;
using System.NumericsX;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;
using static System.NumericsX.OpenStack.Gngine.Render.R;
using static System.NumericsX.OpenStack.Gngine.Gngine;

namespace System.NumericsX.OpenStack.Gngine.Render
{
    public unsafe class RenderModelStatic : IRenderModel
    {
        public bool LoadASE(string fileName)
        {
            var ase = ModelXAse.ASE_Load(fileName); if (ase == null) return false;
            ConvertASEToModelSurfaces(ase);
            ModelXAse.ASE_Free(ase);
            return true;
        }

        public bool LoadLWO(string fileName)
        {
            uint failID = 0; int failPos = 0;
            var lwo = ModelXLwo.lwGetObject(fileName, ref failID, ref failPos);
            if (lwo == null) return false;
            ConvertLWOToModelSurfaces(lwo);
            ModelXLwo.lwFreeObject(lwo);
            return true;
        }

        // USGS height map data for megaTexture experiments
        public bool LoadFLT(string fileName)
        {
            var len = fileSystem.ReadFile(fileName, out var data, out var _);
            if (len <= 0) return false;
            var size = (int)Math.Sqrt(len / 4f);
            fixed (byte* dataB = data)
            {
                var dataF = (float*)dataB;

                // bound the altitudes
                float min = 9999999f, max = -9999999f;
                for (var i = 0; i < len / 4; i++)
                {
                    dataF[i] = BigFloat(dataF[i]);
                    if (dataF[i] == -9999) dataF[i] = 0;        // unscanned areas

                    if (dataF[i] < min) min = dataF[i];
                    if (dataF[i] > max) max = dataF[i];
                }
#if true
                // write out a gray scale height map
                var image = (byte*)R_StaticAlloc(len);
                var image_p = image;
                for (var i = 0; i < len / 4; i++)
                {
                    var v = (data[i] - min) / (max - min);
                    image_p[0] = image_p[1] = image_p[2] = (byte)(v * 255); image_p[3] = 255;
                    image_p += 4;
                }
                var tgaName = $"{PathX.StripFileExtension(fileName)}.tga";
                R_WriteTGA(tgaName, image, size, size, false);
                R_StaticFree(image);
                //return false;
#endif

                // find the island above sea level
                int minX, maxX, minY, maxY;
                {
                    int i;
                    for (minX = 0; minX < size; minX++)
                    {
                        for (i = 0; i < size; i++) if (dataF[i * size + minX] > 1.0f) break;
                        if (i != size) break;
                    }
                    for (maxX = size - 1; maxX > 0; maxX--)
                    {
                        for (i = 0; i < size; i++) if (dataF[i * size + maxX] > 1f) break;
                        if (i != size) break;
                    }
                    for (minY = 0; minY < size; minY++)
                    {
                        for (i = 0; i < size; i++) if (dataF[minY * size + i] > 1f) break;
                        if (i != size) break;
                    }
                    for (maxY = size - 1; maxY < size; maxY--)
                    {
                        for (i = 0; i < size; i++) if (dataF[maxY * size + i] > 1f) break;
                        if (i != size) break;
                    }
                }

                var width = maxX - minX + 1; // width /= 2;
                var height = maxY - minY + 1;

                // allocate triangle surface
                var tri = R_AllocStaticTriSurf();
                tri.numVerts = width * height;
                tri.numIndexes = (width - 1) * (height - 1) * 6;

                fastLoad = true; // don't do all the sil processing

                R_AllocStaticTriSurfIndexes(tri, tri.numIndexes);
                R_AllocStaticTriSurfVerts(tri, tri.numVerts);

                for (var i = 0; i < height; i++)
                    for (var j = 0; j < width; j++)
                    {
                        var v = i * width + j;
                        tri.verts[v].Clear();
                        tri.verts[v].xyz.x = j * 10;  // each sample is 10 meters
                        tri.verts[v].xyz.y = -i * 10;
                        tri.verts[v].xyz.z = data[(minY + i) * size + minX + j];  // height is in meters
                        tri.verts[v].st.x = (float)j / (width - 1);
                        tri.verts[v].st.y = 1f - ((float)i / (height - 1));
                    }

                for (var i = 0; i < height - 1; i++)
                    for (var j = 0; j < width - 1; j++)
                    {
                        var v = (i * (width - 1) + j) * 6;
#if false
                        tri.indexes[v + 0] = i * width + j;
                        tri.indexes[v + 1] = (i + 1) * width + j;
                        tri.indexes[v + 2] = (i + 1) * width + j + 1;
                        tri.indexes[v + 3] = i * width + j;
                        tri.indexes[v + 4] = (i + 1) * width + j + 1;
                        tri.indexes[v + 5] = i * width + j + 1;
#else
                        tri.indexes[v + 0] = i * width + j;
                        tri.indexes[v + 1] = i * width + j + 1;
                        tri.indexes[v + 2] = (i + 1) * width + j + 1;
                        tri.indexes[v + 3] = i * width + j;
                        tri.indexes[v + 4] = (i + 1) * width + j + 1;
                        tri.indexes[v + 5] = (i + 1) * width + j;
#endif
                    }

                fileSystem.FreeFile(data);

                var surface = new ModelSurface
                {
                    geometry = tri,
                    id = 0,
                    shader = tr.defaultMaterial // declManager.FindMaterial("shaderDemos/megaTexture");
                };
                this.AddSurface(surface);
            }
            return true;
        }

        public bool LoadMA(string filename)
        {
            var ma = ModelXMa.MA_Load(filename);
            if (ma == null) return false;
            ConvertMAToModelSurfaces(ma);
            ModelXMa.MA_Free(ma);
            return true;
        }

        class MatchVert
        {
            public MatchVert next;
            public int v, tv;
            public byte[] color = new byte[4];
            public Vector3 normal;
        }

        static byte[] ConvertASEToModelSurfaces_identityColor = { 255, 255, 255, 255 };
        public bool ConvertASEToModelSurfaces(AseModel ase)
        {
            AseObject obj;
            AseMesh mesh;
            AseMaterial material;
            Material im1, im2;
            SrfTriangles tri;
            int objectNum;
            int i, j, k;
            int v, tv;
            int* vRemap;
            int* tvRemap;
            MatchVert mvTable;   // all of the match verts
            MatchVert mvHash;       // points inside mvTable for each xyz index
            MatchVert lastmv;
            MatchVert mv;
            Vector3 normal = new();
            float uOffset, vOffset, textureSin, textureCos;
            float uTiling, vTiling;
            byte* color;
            ModelSurface modelSurf;

            if (ase == null || ase.objects.Count < 1) return false;

            timeStamp = ase.timeStamp;

            // the modeling programs can save out multiple surfaces with a common material, but we would like to mege them together where possible
            // meaning that this.NumSurfaces() <= ase.objects.currentElements
            var mergeTo = stackalloc int[ase.objects.Count * sizeof(int)];
            ModelSurface surf = new();
            surf.geometry = null;
            // if we don't have any materials, dump everything into a single surface
            if (ase.materials.Count == 0)
            {
                surf.shader = tr.defaultMaterial;
                surf.id = 0;
                this.AddSurface(surf);
                for (i = 0; i < ase.objects.Count; i++) mergeTo[i] = 0;
            }
            // don't merge any
            else if (!r_mergeModelSurfaces.Bool)
                for (i = 0; i < ase.objects.Count; i++)
                {
                    mergeTo[i] = i;
                    obj = ase.objects[i];
                    material = ase.materials[obj.materialRef];
                    surf.shader = declManager.FindMaterial(material.name);
                    surf.id = this.NumSurfaces;
                    this.AddSurface(surf);
                }
            // search for material matches
            else
                for (i = 0; i < ase.objects.Count; i++)
                {
                    obj = ase.objects[i];
                    material = ase.materials[obj.materialRef];
                    im1 = declManager.FindMaterial(material.name);
                    if (im1.IsDiscrete) j = this.NumSurfaces; // flares, autosprites, etc
                    else
                        for (j = 0; j < this.NumSurfaces; j++)
                        {
                            modelSurf = this.surfaces[j];
                            im2 = modelSurf.shader;
                            if (im1 == im2) { mergeTo[i] = j; break; } // merge this
                        }
                    // didn't merge
                    if (j == this.NumSurfaces)
                    {
                        mergeTo[i] = j;
                        surf.shader = im1;
                        surf.id = this.NumSurfaces;
                        this.AddSurface(surf);
                    }
                }

            //VectorSubset < Vector3, 3 > vertexSubset;
            //VectorSubset < Vector2, 2 > texCoordSubset;

            // build the surfaces
            for (objectNum = 0; objectNum < ase.objects.Count; objectNum++)
            {
                obj = ase.objects[objectNum];
                mesh = obj.mesh;
                material = ase.materials[obj.materialRef];
                im1 = declManager.FindMaterial(material.name);

                var normalsParsed = mesh.normalsParsed;

                // completely ignore any explict normals on surfaces with a renderbump command which will guarantee the best contours and least vertexes.
                var rb = im1.RenderBump;
                if (rb != null && rb[0] != 0) normalsParsed = false;

                // It seems like the tools our artists are using often generate verts and texcoords slightly separated that should be merged
                // note that we really should combine the surfaces with common materials before doing this operation, because we can miss a slop combination if they are in different surfaces

                vRemap = (int*)R_StaticAlloc(mesh.numVertexes * sizeof(int));
                // renderbump doesn't care about vertex count
                if (fastLoad)
                    for (j = 0; j < mesh.numVertexes; j++) vRemap[j] = j;
                else
                {
                    float vertexEpsilon = r_slopVertex.Float, expand = 2 * 32 * vertexEpsilon; Vector3 mins, maxs;

                    fixed (Vector3* vertexesV = mesh.vertexes) Simd.MinMax3(out mins, out maxs, vertexesV, mesh.numVertexes);
                    mins -= new Vector3(expand, expand, expand);
                    maxs += new Vector3(expand, expand, expand);
                    vertexSubset.Init(mins, maxs, 32, 1024);
                    for (j = 0; j < mesh.numVertexes; j++) vRemap[j] = vertexSubset.FindVector(mesh.vertexes, j, vertexEpsilon);
                }

                tvRemap = (int*)R_StaticAlloc(mesh.numTVertexes * sizeof(int));
                // renderbump doesn't care about vertex count
                if (fastLoad)
                    for (j = 0; j < mesh.numTVertexes; j++) tvRemap[j] = j;
                else
                {
                    float texCoordEpsilon = r_slopTexCoord.Float, expand = 2 * 32 * texCoordEpsilon; Vector2 mins, maxs;

                    fixed (Vector2* tvertexesV = mesh.tvertexes) Simd.MinMax2(out mins, out maxs, tvertexesV, mesh.numTVertexes);
                    mins -= new Vector2(expand, expand);
                    maxs += new Vector2(expand, expand);
                    texCoordSubset.Init(mins, maxs, 32, 1024);
                    for (j = 0; j < mesh.numTVertexes; j++) tvRemap[j] = texCoordSubset.FindVector(mesh.tvertexes, j, texCoordEpsilon);
                }

                // we need to find out how many unique vertex / texcoord combinations there are, because ASE tracks them separately but we need them unified

                // the maximum possible number of combined vertexes is the number of indexes
                mvTable = (MatchVert*)R_ClearedStaticAlloc(mesh.numFaces * 3 * sizeof(MatchVert));

                // we will have a hash chain based on the xyz values
                mvHash = (MatchVert**)R_ClearedStaticAlloc(mesh.numVertexes * sizeof(MatchVert));

                // allocate triangle surface
                tri = R_AllocStaticTriSurf();
                tri.numVerts = 0;
                tri.numIndexes = 0;
                R_AllocStaticTriSurfIndexes(tri, mesh.numFaces * 3);
                tri.generateNormals = !normalsParsed;

                // init default normal, color and tex coord index
                normal.Zero();
                color = ConvertASEToModelSurfaces_identityColor;
                tv = 0;

                // find all the unique combinations
                var normalEpsilon = 1f - r_slopNormal.Float;
                for (j = 0; j < mesh.numFaces; j++)
                {
                    for (k = 0; k < 3; k++)
                    {
                        v = mesh.faces[j].vertexNum[k];
                        if (v < 0 || v >= mesh.numVertexes) common.Error($"ConvertASEToModelSurfaces: bad vertex index in ASE file {name}");

                        // collapse the position if it was slightly offset
                        v = vRemap[v];

                        // we may or may not have texcoords to compare
                        if (mesh.numTVFaces == mesh.numFaces && mesh.numTVertexes != 0)
                        {
                            tv = mesh.faces[j].tVertexNum[k];
                            if (tv < 0 || tv >= mesh.numTVertexes) common.Error($"ConvertASEToModelSurfaces: bad tex coord index in ASE file {name}");
                            // collapse the tex coord if it was slightly offset
                            tv = tvRemap[tv];
                        }

                        // we may or may not have normals to compare
                        if (normalsParsed) normal = mesh.faces[j].vertexNormals[k];

                        // we may or may not have colors to compare
                        if (mesh.colorsParsed) color = mesh.faces[j].vertexColors[k];

                        // find a matching vert
                        for (lastmv = null, mv = mvHash[v]; mv != null; lastmv = mv, mv = mv.next)
                        {
                            if (mv.tv != tv) continue;
                            if (*(uint*)mv.color != *(uint*)color) continue;
                            if (!normalsParsed) break; // if we are going to create the normals, just matching texcoords is enough
                            if (mv.normal * normal > normalEpsilon) break;      // we already have this one
                        }
                        if (mv == null)
                        {
                            // allocate a new match vert and link to hash chain
                            mv = mvTable[tri.numVerts];
                            mv.v = v;
                            mv.tv = tv;
                            mv.normal = normal;
                            *(uint*)mv.color = *(uint*)color;
                            mv.next = null;
                            if (lastmv != null) lastmv.next = mv;
                            else mvHash[v] = mv;
                            tri.numVerts++;
                        }

                        tri.indexes[tri.numIndexes] = mv - mvTable;
                        tri.numIndexes++;
                    }
                }

                // allocate space for the indexes and copy them
                if (tri.numIndexes > mesh.numFaces * 3) common.FatalError($"ConvertASEToModelSurfaces: index miscount in ASE file {name}");
                if (tri.numVerts > mesh.numFaces * 3) common.FatalError($"ConvertASEToModelSurfaces: vertex miscount in ASE file {name}");

                // an ASE allows the texture coordinates to be scaled, translated, and rotated
                if (ase.materials.Count == 0)
                {
                    uOffset = vOffset = 0f;
                    uTiling = vTiling = 1f;
                    textureSin = 0f;
                    textureCos = 1f;
                }
                else
                {
                    material = ase.materials[obj.materialRef];
                    uOffset = -material.uOffset;
                    vOffset = material.vOffset;
                    uTiling = material.uTiling;
                    vTiling = material.vTiling;
                    textureSin = MathX.Sin(material.angle);
                    textureCos = MathX.Cos(material.angle);
                }

                // now allocate and generate the combined vertexes
                R_AllocStaticTriSurfVerts(tri, tri.numVerts);
                for (j = 0; j < tri.numVerts; j++)
                {
                    mv = mvTable[j];
                    tri.verts[j].Clear();
                    tri.verts[j].xyz = mesh.vertexes[mv.v];
                    tri.verts[j].normal = mv.normal;
                    *(uint*)tri.verts[j].color = *(uint*)mv.color;
                    if (mesh.numTVFaces == mesh.numFaces && mesh.numTVertexes != 0)
                    {
                        var tv = mesh.tvertexes[mv.tv];
                        var u = tv.x * uTiling + uOffset;
                        var v = tv.y * vTiling + vOffset;
                        tri.verts[j].st[0] = u * textureCos + v * textureSin;
                        tri.verts[j].st[1] = u * -textureSin + v * textureCos;
                    }
                }

                R_StaticFree(ref mvTable);
                R_StaticFree(ref mvHash);
                R_StaticFree(ref tvRemap);
                R_StaticFree(ref vRemap);

                // see if we need to merge with a previous surface of the same material
                modelSurf = this.surfaces[mergeTo[objectNum]];
                var mergeTri = modelSurf.geometry;
                if (mergeTri == null) modelSurf.geometry = tri;
                else
                {
                    modelSurf.geometry = R_MergeTriangles(mergeTri, tri);
                    R_FreeStaticTriSurf(tri);
                    R_FreeStaticTriSurf(mergeTri);
                }
            }

            return true;
        }

        public bool ConvertLWOToModelSurfaces(lwObject lwo)
        {
            Material im1, im2;
            SrfTriangles tri;
            lwSurface lwoSurf;
            int numTVertexes;
            int i, j, k;
            int v, tv;
            Vector3* vList;
            int* vRemap;
            Vector2[] tvList;
            int* tvRemap;
            MatchVert mvTable;   // all of the match verts
            MatchVert[] mvHash;  // points inside mvTable for each xyz index
            MatchVert lastmv;
            MatchVert mv;
            Vector3 normal;
            byte[] color = new byte[4];
            ModelSurface modelSurf;

            if (lwo == null || lwo.surf == null) return false;

            timeStamp = lwo.timeStamp;

            // count the number of surfaces
            i = 0;
            for (lwoSurf = lwo.surf; lwoSurf != null; lwoSurf = (lwSurface)lwoSurf.next) i++;

            // the modeling programs can save out multiple surfaces with a common material, but we would like to merge them together where possible
            var mergeTo = stackalloc int[i];
            var surf = new ModelSurface();

            // don't merge any
            if (!r_mergeModelSurfaces.Bool)
                for (lwoSurf = lwo.surf, i = 0; lwoSurf != null; lwoSurf = (lwSurface)lwoSurf.next, i++)
                {
                    mergeTo[i] = i;
                    surf.shader = declManager.FindMaterial(lwoSurf.name);
                    surf.id = this.NumSurfaces;
                    this.AddSurface(surf);
                }
            // search for material matches
            else
                for (lwoSurf = lwo.surf, i = 0; lwoSurf != null; lwoSurf = (lwSurface)lwoSurf.next, i++)
                {
                    im1 = declManager.FindMaterial(lwoSurf.name);
                    if (im1.IsDiscrete) j = this.NumSurfaces; // flares, autosprites, etc
                    else
                        for (j = 0; j < this.NumSurfaces; j++)
                        {
                            modelSurf = this.surfaces[j];
                            im2 = modelSurf.shader;
                            if (im1 == im2) { mergeTo[i] = j; break; } // merge this
                        }
                    // didn't merge
                    if (j == this.NumSurfaces)
                    {
                        mergeTo[i] = j;
                        surf.shader = im1;
                        surf.id = this.NumSurfaces;
                        this.AddSurface(surf);
                    }
                }

            //VectorSubset < idVec3, 3 > vertexSubset;
            //VectorSubset < idVec2, 2 > texCoordSubset;

            // we only ever use the first layer
            var layer = (lwLayer)lwo.layer;

            // vertex positions
            if (layer.point.count <= 0) { common.Warning($"ConvertLWOToModelSurfaces: model \'{name}\' has bad or missing vertex data"); return false; }

            vList = (Vector3**)R_StaticAlloc(layer.point.count * sizeof(Vector3));
            for (j = 0; j < layer.point.count; j++)
            {
                vList[j].x = layer.point.pt[j].pos[0];
                vList[j].y = layer.point.pt[j].pos[2];
                vList[j].z = layer.point.pt[j].pos[1];
            }

            // vertex texture coords
            numTVertexes = 0;

            if (layer.nvmaps != 0)
                for (var vm = layer.vmap; vm != null; vm = (lwVMap)vm.next) if (vm.type == ('T' << 24 | 'X' << 16 | 'U' << 8 | 'V')) numTVertexes += vm.nverts;

            if (numTVertexes != 0)
            {
                tvList = new Vector2[numTVertexes];
                var offset = 0;
                for (var vm = layer.vmap; vm != null; vm = (lwVMap)vm.next)
                    if (vm.type == ('T' << 24 | 'X' << 16 | 'U' << 8 | 'V'))
                    {
                        vm.offset = offset;
                        for (k = 0; k < vm.nverts; k++)
                        {
                            tvList[k + offset].x = vm.val[k][0];
                            tvList[k + offset].y = 1f - vm.val[k][1];    // invert the t
                        }
                        offset += vm.nverts;
                    }
            }
            else
            {
                common.Warning($"ConvertLWOToModelSurfaces: model \'{name}\' has bad or missing uv data");
                numTVertexes = 1;
                tvList = new Vector2[numTVertexes];
            }

            // It seems like the tools our artists are using often generate verts and texcoords slightly separated that should be merged
            // note that we really should combine the surfaces with common materials before doing this operation, because we can miss a slop combination if they are in different surfaces

            vRemap = (int*)R_StaticAlloc(layer.point.count * sizeof(int));

            // renderbump doesn't care about vertex count
            if (fastLoad)
                for (j = 0; j < layer.point.count; j++) vRemap[j] = j;
            else
            {
                float vertexEpsilon = r_slopVertex.Float, expand = 2 * 32 * vertexEpsilon; Vector3 mins, maxs;
                Simd.MinMax3(out mins, out maxs, vList, layer.point.count);
                mins -= new Vector3(expand, expand, expand);
                maxs += new Vector3(expand, expand, expand);
                vertexSubset.Init(mins, maxs, 32, 1024);
                for (j = 0; j < layer.point.count; j++) vRemap[j] = vertexSubset.FindVector(vList, j, vertexEpsilon);
            }

            tvRemap = (int*)R_StaticAlloc(numTVertexes * sizeof(int));
            // renderbump doesn't care about vertex count
            if (fastLoad)
                for (j = 0; j < numTVertexes; j++) tvRemap[j] = j;
            else
            {
                float texCoordEpsilon = r_slopTexCoord.Float, expand = 2 * 32 * texCoordEpsilon; Vector2 mins, maxs;
                fixed (Vector2* tvListV = tvList) Simd.MinMax2(out mins, out maxs, tvListV, numTVertexes);
                mins -= new Vector2(expand, expand);
                maxs += new Vector2(expand, expand);
                texCoordSubset.Init(mins, maxs, 32, 1024);
                for (j = 0; j < numTVertexes; j++) tvRemap[j] = texCoordSubset.FindVector(tvList, j, texCoordEpsilon);
            }

            // build the surfaces
            for (lwoSurf = lwo.surf, i = 0; lwoSurf != null; lwoSurf = (lwSurface)lwoSurf.next, i++)
            {
                im1 = declManager.FindMaterial(lwoSurf.name);

                var normalsParsed = true;

                // completely ignore any explict normals on surfaces with a renderbump command which will guarantee the best contours and least vertexes.
                var rb = im1.RenderBump;
                if (rb != null && rb[0] != 0) normalsParsed = false;

                // we need to find out how many unique vertex / texcoord combinations there are

                // the maximum possible number of combined vertexes is the number of indexes
                mvTable = new MatchVert[layer.polygon.count * 3];

                // we will have a hash chain based on the xyz values
                mvHash = new MatchVert[layer.point.count];

                // allocate triangle surface
                tri = R_AllocStaticTriSurf();
                tri.numVerts = 0;
                tri.numIndexes = 0;
                R_AllocStaticTriSurfIndexes(tri, layer.polygon.count * 3);
                tri.generateNormals = !normalsParsed;

                // find all the unique combinations
                var normalEpsilon = fastLoad ? 1f : 1f - r_slopNormal.Float;    // don't merge unless completely exact
                for (j = 0; j < layer.polygon.count; j++)
                {
                    var poly = layer.polygon.pol[j];
                    if (poly.surf != lwoSurf) continue;
                    if (poly.nverts != 3) { common.Warning($"ConvertLWOToModelSurfaces: model {name} has too many verts for a poly! Make sure you triplet it down"); continue; }

                    for (k = 0; k < 3; k++)
                    {
                        v = vRemap[poly.v[k].index];

                        normal.x = poly.v[k].norm[0];
                        normal.y = poly.v[k].norm[2];
                        normal.z = poly.v[k].norm[1];

                        // LWO models aren't all that pretty when it comes down to the floating point values they store
                        normal.FixDegenerateNormal();

                        tv = 0;

                        color[0] = (byte)(lwoSurf.color.rgb[0] * 255);
                        color[1] = (byte)(lwoSurf.color.rgb[1] * 255);
                        color[2] = (byte)(lwoSurf.color.rgb[2] * 255);
                        color[3] = (byte)255;

                        // first set attributes from the vertex
                        var pt = layer.point.pt[poly.v[k].index];
                        int nvm;
                        for (nvm = 0; nvm < pt.nvmaps; nvm++)
                        {
                            var vm = pt.vm[nvm];
                            if (vm.vmap.type == ('T' << 24 | 'X' << 16 | 'U' << 8 | 'V')) tv = tvRemap[vm.index + vm.vmap.offset];
                            if (vm.vmap.type == ('R' << 24 | 'G' << 16 | 'B' << 8 | 'A')) for (var chan = 0; chan < 4; chan++) color[chan] = (byte)(255 * vm.vmap.val[vm.index][chan]);
                        }

                        // then override with polygon attributes
                        for (nvm = 0; nvm < poly.v[k].nvmaps; nvm++)
                        {
                            var vm = poly.v[k].vm[nvm];
                            if (vm.vmap.type == ('T' << 24 | 'X' << 16 | 'U' << 8 | 'V')) tv = tvRemap[vm.index + vm.vmap.offset];
                            if (vm.vmap.type == ('R' << 24 | 'G' << 16 | 'B' << 8 | 'A')) for (var chan = 0; chan < 4; chan++) color[chan] = (byte)(255 * vm.vmap.val[vm.index][chan]);
                        }

                        // find a matching vert
                        for (lastmv = null, mv = mvHash[v]; mv != null; lastmv = mv, mv = mv.next)
                        {
                            if (mv.tv != tv) continue;
                            if (*(uint*)mv.color != *(uint*)color) continue;
                            if (!normalsParsed) break; // if we are going to create the normals, just matching texcoords is enough
                            if (mv.normal * normal > normalEpsilon) break; // we already have this one
                        }
                        // allocate a new match vert and link to hash chain
                        if (mv == null)
                        {
                            mv = mvTable[tri.numVerts];
                            mv.v = v;
                            mv.tv = tv;
                            mv.normal = normal;
                            *(uint*)mv.color = *(uint*)color;
                            mv.next = null;
                            if (lastmv != null) lastmv.next = mv;
                            else mvHash[v] = mv;
                            tri.numVerts++;
                        }

                        tri.indexes[tri.numIndexes] = mv - mvTable;
                        tri.numIndexes++;
                    }
                }

                // allocate space for the indexes and copy them
                if (tri.numIndexes > layer.polygon.count * 3) common.FatalError($"ConvertLWOToModelSurfaces: index miscount in LWO file {name}");
                if (tri.numVerts > layer.polygon.count * 3) common.FatalError($"ConvertLWOToModelSurfaces: vertex miscount in LWO file {name}");

                // now allocate and generate the combined vertexes
                R_AllocStaticTriSurfVerts(tri, tri.numVerts);
                for (j = 0; j < tri.numVerts; j++)
                {
                    mv = mvTable[j];
                    tri.verts[j].Clear();
                    tri.verts[j].xyz = vList[mv.v];
                    tri.verts[j].st = tvList[mv.tv];
                    tri.verts[j].normal = mv.normal;
                    *(uint*)tri.verts[j].color = *(uint*)mv.color;
                }

                R_StaticFree(mvTable);
                R_StaticFree(mvHash);

                // see if we need to merge with a previous surface of the same material
                modelSurf = this.surfaces[mergeTo[i]];
                var mergeTri = modelSurf.geometry;
                if (mergeTri == null) modelSurf.geometry = tri;
                else
                {
                    modelSurf.geometry = R_MergeTriangles(mergeTri, tri);
                    R_FreeStaticTriSurf(tri);
                    R_FreeStaticTriSurf(mergeTri);
                }
            }

            R_StaticFree(ref tvRemap);
            R_StaticFree(ref vRemap);
            R_StaticFree(ref tvList);
            R_StaticFree(ref vList);

            return true;
        }

        public AseModel ConvertLWOToASE(lwObject obj, string fileName)
        {
            int j, k;

            if (obj == null) return null;

            var ase = new AseModel { timeStamp = obj.timeStamp };
            ase.objects.Resize(obj.nlayers, obj.nlayers);

            var materialRef = 0;
            for (var surf = obj.surf; surf != null; surf = (lwSurface)surf.next)
            {
                var mat = new AseMaterial { name = surf.name, uTiling = 1, vTiling = 1, angle = 0, uOffset = 0, vOffset = 0 };
                ase.materials.Add(mat);

                var layer = obj.layer;

                var obj2 = new AseObject { materialRef = materialRef++ };
                var mesh = obj2.mesh;
                ase.objects.Add(obj2);

                mesh.numFaces = layer.polygon.count;
                mesh.numTVFaces = mesh.numFaces;
                mesh.faces = new AseFace[mesh.numFaces];
                mesh.numVertexes = layer.point.count;
                mesh.vertexes = new Vector3[mesh.numVertexes];

                // vertex positions
                if (layer.point.count <= 0) common.Warning($"ConvertLWOToASE: model \'{name}\' has bad or missing vertex data");

                for (j = 0; j < layer.point.count; j++)
                {
                    mesh.vertexes[j].x = layer.point.pt[j].pos[0];
                    mesh.vertexes[j].y = layer.point.pt[j].pos[2];
                    mesh.vertexes[j].z = layer.point.pt[j].pos[1];
                }

                // vertex texture coords
                mesh.numTVertexes = 0;

                if (layer.nvmaps != 0) for (var vm = layer.vmap; vm != null; vm = (lwVMap)vm.next) if (vm.type == ('T' << 24 | 'X' << 16 | 'U' << 8 | 'V')) mesh.numTVertexes += vm.nverts;

                if (mesh.numTVertexes != 0)
                {
                    mesh.tvertexes = new Vector2[mesh.numTVertexes];
                    var offset = 0;
                    for (var vm = layer.vmap; vm != null; vm = (lwVMap)vm.next)
                        if (vm.type == ('T' << 24 | 'X' << 16 | 'U' << 8 | 'V'))
                        {
                            vm.offset = offset;
                            for (k = 0; k < vm.nverts; k++)
                            {
                                mesh.tvertexes[k + offset].x = vm.val[k][0];
                                mesh.tvertexes[k + offset].y = 1f - vm.val[k][1];   // invert the t
                            }
                            offset += vm.nverts;
                        }
                }
                else
                {
                    common.Warning($"ConvertLWOToASE: model \'{fileName}\' has bad or missing uv data");
                    mesh.numTVertexes = 1;
                    mesh.tvertexes = new Vector2[mesh.numTVertexes];
                }

                mesh.normalsParsed = true;
                mesh.colorsParsed = true;  // because we are falling back to the surface color

                // triangles
                var faceIndex = 0;
                for (j = 0; j < layer.polygon.count; j++)
                {
                    var poly = layer.polygon.pol[j];
                    if (poly.surf != surf) continue;
                    if (poly.nverts != 3) { common.Warning($"ConvertLWOToASE: model {fileName} has too many verts for a poly! Make sure you triplet it down"); continue; }

                    mesh.faces[faceIndex].faceNormal.x = poly.norm[0];
                    mesh.faces[faceIndex].faceNormal.y = poly.norm[2];
                    mesh.faces[faceIndex].faceNormal.z = poly.norm[1];

                    for (k = 0; k < 3; k++)
                    {
                        mesh.faces[faceIndex].vertexNum[k] = poly.v[k].index;

                        mesh.faces[faceIndex].vertexNormals[k].x = poly.v[k].norm[0];
                        mesh.faces[faceIndex].vertexNormals[k].y = poly.v[k].norm[2];
                        mesh.faces[faceIndex].vertexNormals[k].z = poly.v[k].norm[1];

                        // complete fallbacks
                        mesh.faces[faceIndex].tVertexNum[k] = 0;

                        mesh.faces[faceIndex].vertexColors[k][0] = (byte)(surf.color.rgb[0] * 255);
                        mesh.faces[faceIndex].vertexColors[k][1] = (byte)(surf.color.rgb[1] * 255);
                        mesh.faces[faceIndex].vertexColors[k][2] = (byte)(surf.color.rgb[2] * 255);
                        mesh.faces[faceIndex].vertexColors[k][3] = 255;

                        // first set attributes from the vertex
                        var pt = layer.point.pt[poly.v[k].index];
                        int nvm;
                        for (nvm = 0; nvm < pt.nvmaps; nvm++)
                        {
                            var vm = pt.vm[nvm];
                            if (vm.vmap.type == ('T' << 24 | 'X' << 16 | 'U' << 8 | 'V')) mesh.faces[faceIndex].tVertexNum[k] = vm.index + vm.vmap.offset;
                            if (vm.vmap.type == ('R' << 24 | 'G' << 16 | 'B' << 8 | 'A')) for (var chan = 0; chan < 4; chan++) mesh.faces[faceIndex].vertexColors[k][chan] = (byte)(255 * vm.vmap.val[vm.index][chan]);
                        }

                        // then override with polygon attributes
                        for (nvm = 0; nvm < poly.v[k].nvmaps; nvm++)
                        {
                            var vm = poly.v[k].vm[nvm];
                            if (vm.vmap.type == ('T' << 24 | 'X' << 16 | 'U' << 8 | 'V')) mesh.faces[faceIndex].tVertexNum[k] = vm.index + vm.vmap.offset;
                            if (vm.vmap.type == ('R' << 24 | 'G' << 16 | 'B' << 8 | 'A')) for (var chan = 0; chan < 4; chan++) mesh.faces[faceIndex].vertexColors[k][chan] = (byte)(255 * vm.vmap.val[vm.index][chan]);
                        }
                    }

                    faceIndex++;
                }

                mesh.numFaces = faceIndex;
                mesh.numTVFaces = faceIndex;

                Array.Resize(ref mesh.faces, mesh.numFaces);
            }

            return ase;
        }

        static byte[] ConvertMAToModelSurfaces_identityColor = { 255, 255, 255, 255 };
        public bool ConvertMAToModelSurfaces(MaModel ma)
        {
            MaObject obj;
            MaMesh mesh;
            MaMaterial material;

            Material im1, im2;
            SrfTriangles tri;
            int objectNum;
            int i, j, k;
            int v, tv;
            int* vRemap;
            int* tvRemap;
            MatchVert mvTable;   // all of the match verts
            MatchVert[] mvHash;       // points inside mvTable for each xyz index
            MatchVert lastmv;
            MatchVert mv;
            Vector3 normal;
            float uOffset, vOffset, textureSin, textureCos;
            float uTiling, vTiling;
            byte* color;

            ModelSurface modelSurf;

            if (ma == null || ma.objects.Count < 1) return false;

            timeStamp = ma.timeStamp;

            // the modeling programs can save out multiple surfaces with a common material, but we would like to mege them together where possible meaning that this.NumSurfaces() <= ma.objects.currentElements
            var mergeTo = stackalloc int[ma.objects.Count];

            var surf = new ModelSurface();
            surf.geometry = null;
            // if we don't have any materials, dump everything into a single surface
            if (ma.materials.Count == 0)
            {
                surf.shader = tr.defaultMaterial;
                surf.id = 0;
                this.AddSurface(surf);
                for (i = 0; i < ma.objects.Count; i++) mergeTo[i] = 0;
            }
            // don't merge any
            else if (!r_mergeModelSurfaces.Bool)
                for (i = 0; i < ma.objects.Count; i++)
                {
                    mergeTo[i] = i;
                    obj = ma.objects[i];
                    if (obj.materialRef >= 0) { material = ma.materials[obj.materialRef]; surf.shader = declManager.FindMaterial(material.name); }
                    else surf.shader = tr.defaultMaterial;
                    surf.id = this.NumSurfaces;
                    this.AddSurface(surf);
                }
            // search for material matches
            else
            {
                for (i = 0; i < ma.objects.Count; i++)
                {
                    obj = ma.objects[i];
                    if (obj.materialRef >= 0) { material = ma.materials[obj.materialRef]; im1 = declManager.FindMaterial(material.name); }
                    else im1 = tr.defaultMaterial;
                    if (im1.IsDiscrete) j = this.NumSurfaces; // flares, autosprites, etc
                    else
                        for (j = 0; j < this.NumSurfaces; j++)
                        {
                            modelSurf = this.surfaces[j];
                            im2 = modelSurf.shader;
                            if (im1 == im2) { mergeTo[i] = j; break; } // merge this
                        }
                    // didn't merge
                    if (j == this.NumSurfaces)
                    {
                        mergeTo[i] = j;
                        surf.shader = im1;
                        surf.id = this.NumSurfaces;
                        this.AddSurface(surf);
                    }
                }
            }

            VectorSubset<Vector3> vertexSubset = new(3);
            VectorSubset<Vector2> texCoordSubset = new(2);

            // build the surfaces
            for (objectNum = 0; objectNum < ma.objects.Count; objectNum++)
            {
                obj = ma.objects[objectNum];
                mesh = obj.mesh;
                if (obj.materialRef >= 0) { material = ma.materials[obj.materialRef]; im1 = declManager.FindMaterial(material.name); }
                else im1 = tr.defaultMaterial;

                var normalsParsed = mesh.normalsParsed;

                // completely ignore any explict normals on surfaces with a renderbump command which will guarantee the best contours and least vertexes.
                var rb = im1.RenderBump;
                if (rb != null && rb[0] != 0) normalsParsed = false;

                // It seems like the tools our artists are using often generate verts and texcoords slightly separated that should be merged
                // note that we really should combine the surfaces with common materials before doing this operation, because we can miss a slop combination if they are in different surfaces

                vRemap = (int*)R_StaticAlloc(mesh.numVertexes * sizeof(vRemap[0]));

                // renderbump doesn't care about vertex count
                if (fastLoad)
                    for (j = 0; j < mesh.numVertexes; j++) vRemap[j] = j;
                else
                {
                    float vertexEpsilon = r_slopVertex.Float, expand = 2 * 32 * vertexEpsilon; Vector3 mins, maxs;
                    fixed (Vector3* vertexesF = mesh.vertexes) Simd.MinMax3(out mins, out maxs, vertexesF, mesh.numVertexes);
                    mins -= new Vector3(expand, expand, expand);
                    maxs += new Vector3(expand, expand, expand);
                    vertexSubset.Init(mins, maxs, 32, 1024);
                    for (j = 0; j < mesh.numVertexes; j++) vRemap[j] = vertexSubset.FindVector(mesh.vertexes, j, vertexEpsilon);
                }

                tvRemap = (int*)R_StaticAlloc(mesh.numTVertexes * sizeof(tvRemap[0]));
                // renderbump doesn't care about vertex count
                if (fastLoad)
                    for (j = 0; j < mesh.numTVertexes; j++) tvRemap[j] = j;
                else
                {
                    float texCoordEpsilon = r_slopTexCoord.Float, expand = 2 * 32 * texCoordEpsilon; Vector2 mins, maxs;

                    fixed (Vector2* tvertexesF = mesh.tvertexes) Simd.MinMax2(out mins, out maxs, tvertexesF, mesh.numTVertexes);
                    mins -= new Vector2(expand, expand);
                    maxs += new Vector2(expand, expand);
                    texCoordSubset.Init(mins, maxs, 32, 1024);
                    for (j = 0; j < mesh.numTVertexes; j++) tvRemap[j] = texCoordSubset.FindVector(mesh.tvertexes, j, texCoordEpsilon);
                }

                // we need to find out how many unique vertex / texcoord / color combinations there are, because MA tracks them separately but we need them unified

                // the maximum possible number of combined vertexes is the number of indexes
                mvTable = (MatchVert)R_ClearedStaticAlloc(mesh.numFaces * 3 * sizeof(MatchVert));

                // we will have a hash chain based on the xyz values
                mvHash = (MatchVert[])R_ClearedStaticAlloc(mesh.numVertexes * sizeof(MatchVert[]));

                // allocate triangle surface
                tri = R_AllocStaticTriSurf();
                tri.numVerts = 0;
                tri.numIndexes = 0;
                R_AllocStaticTriSurfIndexes(tri, mesh.numFaces * 3);
                tri.generateNormals = !normalsParsed;

                // init default normal, color and tex coord index
                normal.Zero();
                color = ConvertMAToModelSurfaces_identityColor;
                tv = 0;

                // find all the unique combinations
                var normalEpsilon = 1f - r_slopNormal.Float;
                for (j = 0; j < mesh.numFaces; j++)
                {
                    for (k = 0; k < 3; k++)
                    {
                        v = mesh.faces[j].vertexNum[k];

                        if (v < 0 || v >= mesh.numVertexes) common.Error($"ConvertMAToModelSurfaces: bad vertex index in MA file {name}");

                        // collapse the position if it was slightly offset
                        v = vRemap[v];

                        // we may or may not have texcoords to compare
                        if (mesh.numTVertexes != 0)
                        {
                            tv = mesh.faces[j].tVertexNum[k];
                            if (tv < 0 || tv >= mesh.numTVertexes) common.Error($"ConvertMAToModelSurfaces: bad tex coord index in MA file {name}");
                            // collapse the tex coord if it was slightly offset
                            tv = tvRemap[tv];
                        }

                        // we may or may not have normals to compare
                        if (normalsParsed) normal = mesh.faces[j].vertexNormals[k];

                        // BSM: Todo: Fix the vertex colors
                        // we may or may not have colors to compare
                        if (mesh.faces[j].vertexColors[k] != -1 && mesh.faces[j].vertexColors[k] != -999) color = mesh.colors[mesh.faces[j].vertexColors[k] * 4];

                        // find a matching vert
                        for (lastmv = null, mv = mvHash[v]; mv != null; lastmv = mv, mv = mv.next)
                        {
                            if (mv.tv != tv) continue;
                            if (*(uint*)mv.color != *(uint*)color) continue;
                            if (!normalsParsed) break; // if we are going to create the normals, just matching texcoords is enough
                            if (mv.normal * normal > normalEpsilon) break;      // we already have this one
                        }
                        if (mv == null)
                        {
                            // allocate a new match vert and link to hash chain
                            mv = mvTable[tri.numVerts];
                            mv.v = v;
                            mv.tv = tv;
                            mv.normal = normal;
                            *(uint*)mv.color = *(uint*)color;
                            mv.next = null;
                            if (lastmv != null) lastmv.next = mv;
                            else mvHash[v] = mv;
                            tri.numVerts++;
                        }

                        tri.indexes[tri.numIndexes] = mv - mvTable;
                        tri.numIndexes++;
                    }
                }

                // allocate space for the indexes and copy them
                if (tri.numIndexes > mesh.numFaces * 3) common.FatalError($"ConvertMAToModelSurfaces: index miscount in MA file {name}");
                if (tri.numVerts > mesh.numFaces * 3) common.FatalError($"ConvertMAToModelSurfaces: vertex miscount in MA file {name}");

                // an MA allows the texture coordinates to be scaled, translated, and rotated
                //BSM: Todo: Does Maya support this and if so how
                //if (ase.materials.Num() == 0 ) {
                uOffset = vOffset = 0f;
                uTiling = vTiling = 1f;
                textureSin = 0f;
                textureCos = 1f;
                //} else {
                //	material = ase.materials[object.materialRef];
                //	uOffset = -material.uOffset;
                //	vOffset = material.vOffset;
                //	uTiling = material.uTiling;
                //	vTiling = material.vTiling;
                //	textureSin = idMath::Sin( material.angle );
                //	textureCos = idMath::Cos( material.angle );
                //}

                // now allocate and generate the combined vertexes
                R_AllocStaticTriSurfVerts(tri, tri.numVerts);
                for (j = 0; j < tri.numVerts; j++)
                {
                    mv = mvTable[j];
                    tri.verts[j].Clear();
                    tri.verts[j].xyz = mesh.vertexes[mv.v];
                    tri.verts[j].normal = mv.normal;
                    *(uint*)tri.verts[j].color = *(uint*)mv.color;
                    if (mesh.numTVertexes != 0)
                    {
                        var tv = mesh.tvertexes[mv.tv];
                        var u = tv.x * uTiling + uOffset;
                        var v = tv.y * vTiling + vOffset;
                        tri.verts[j].st[0] = u * textureCos + v * textureSin;
                        tri.verts[j].st[1] = u * -textureSin + v * textureCos;
                    }
                }

                R_StaticFree(ref mvTable);
                R_StaticFree(ref mvHash);
                R_StaticFree(ref tvRemap);
                R_StaticFree(ref vRemap);

                // see if we need to merge with a previous surface of the same material
                modelSurf = this.surfaces[mergeTo[objectNum]];
                var mergeTri = modelSurf.geometry;
                if (mergeTri == null) modelSurf.geometry = tri;
                else
                {
                    modelSurf.geometry = R_MergeTriangles(mergeTri, tri);
                    R_FreeStaticTriSurf(tri);
                    R_FreeStaticTriSurf(mergeTri);
                }
            }

            return true;
        }
    }
}