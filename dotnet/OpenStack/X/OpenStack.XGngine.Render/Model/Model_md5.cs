using System;
using System.Diagnostics;
using System.NumericsX;
using System.NumericsX.OpenStack;
using static Gengine.Lib;
using static Gengine.Render.TR;
using static System.NumericsX.OpenStack.OpenStack;
using static System.NumericsX.Platform;

namespace Gengine.Render
{
    public unsafe class MD5Mesh
    {
        static int c_numVerts = 0;
        static int c_numWeights = 0;
        static int c_numWeightJoints = 0;

        struct VertexWeight
        {
            public int vert;
            public int joint;
            public Vector3 offset;
            public float jointWeight;
        }

        Vector2[] texCoords;           // texture coordinates
        int numWeights;         // number of weights
        Vector4[] scaledWeights;      // joint weights
        int[] weightIndex;       // pairs of: joint offset + bool true if next weight is for next vertex
        internal Material shader;               // material applied to mesh
        int numTris;            // number of triangles
        internal DeformInfo deformInfo;          // used to create srfTriangles_t from base frames and new vertexes
        internal int surfaceNum;         // number of the static surface created for this mesh

        public MD5Mesh()
        {
            scaledWeights = null;
            weightIndex = null;
            shader = null;
            numTris = 0;
            deformInfo = null;
            surfaceNum = 0;
        }

        public void ParseMesh(Lexer parser, int numJoints, JointMat[] joints)
        {
            int num, jointnum, i, j; int[] tris, firstWeightForVertex, numWeightsForVertex; VertexWeight[] tempWeights;

            parser.ExpectTokenString("{");

            // parse name
            if (parser.CheckTokenString("name")) parser.ReadToken(out var name);

            // parse shader
            parser.ExpectTokenString("shader"); parser.ReadToken(out var token);
            var shaderName = token;
            shader = declManager.FindMaterial(shaderName);

            // parse texture coordinates
            parser.ExpectTokenString("numverts"); var count = parser.ParseInt();
            if (count < 0) parser.Error($"Invalid size: {token}");

            texCoords = new Vector2[count];
            firstWeightForVertex = new int[count];
            numWeightsForVertex = new int[count];

            numWeights = 0;
            var maxweight = 0;
            for (i = 0; i < texCoords.Length; i++)
            {
                parser.ExpectTokenString("vert"); parser.ParseInt();

                fixed (float* textCoordsF = &texCoords[i].x) parser.Parse1DMatrix(2, textCoordsF);

                firstWeightForVertex[i] = parser.ParseInt();
                numWeightsForVertex[i] = parser.ParseInt();
                if (numWeightsForVertex[i] == 0) parser.Error("Vertex without any joint weights.");

                numWeights += numWeightsForVertex[i];
                if (numWeightsForVertex[i] + firstWeightForVertex[i] > maxweight) maxweight = numWeightsForVertex[i] + firstWeightForVertex[i];
            }

            // parse tris
            parser.ExpectTokenString("numtris");
            count = parser.ParseInt();
            if (count < 0) parser.Error($"Invalid size: {count}");

            tris = new int[count * 3];
            numTris = count;
            for (i = 0; i < count; i++)
            {
                parser.ExpectTokenString("tri"); parser.ParseInt();
                tris[i * 3 + 0] = parser.ParseInt();
                tris[i * 3 + 1] = parser.ParseInt();
                tris[i * 3 + 2] = parser.ParseInt();
            }

            // parse weights
            parser.ExpectTokenString("numweights"); count = parser.ParseInt();
            if (count < 0) parser.Error($"Invalid size: {count}");

            if (maxweight > count) parser.Warning($"Vertices reference out of range weights in model ({maxweight} of {count} weights).");

            tempWeights = new VertexWeight[count];
            for (i = 0; i < count; i++)
            {
                parser.ExpectTokenString("weight"); parser.ParseInt();

                jointnum = parser.ParseInt();
                if ((jointnum < 0) || (jointnum >= numJoints)) parser.Error($"Joint Index out of range({numJoints}): {jointnum}");
                tempWeights[i].joint = jointnum;
                tempWeights[i].jointWeight = parser.ParseFloat();

                fixed (float* offsetF = &tempWeights[i].offset.x) parser.Parse1DMatrix(3, offsetF);
            }

            // create pre-scaled weights and an index for the vertex/joint lookup
            scaledWeights = new Vector4[numWeights];
            weightIndex = new int[numWeights * 2];

            count = 0;
            for (i = 0; i < texCoords.Length; i++)
            {
                num = firstWeightForVertex[i];
                for (j = 0; j < numWeightsForVertex[i]; j++, num++, count++)
                {
                    scaledWeights[count].ToVec3() = tempWeights[num].offset * tempWeights[num].jointWeight;
                    scaledWeights[count].w = tempWeights[num].jointWeight;
                    weightIndex[count * 2 + 0] = tempWeights[num].joint * sizeof(JointMat);
                }
                weightIndex[count * 2 - 1] = 1;
            }

            tempWeights = null;
            numWeightsForVertex = null;
            firstWeightForVertex = null;

            parser.ExpectTokenString("}");

            // update counters
            c_numVerts += texCoords.Length;
            c_numWeights += numWeights;
            c_numWeightJoints++;
            for (i = 0; i < numWeights; i++) c_numWeightJoints += weightIndex[i * 2 + 1];

            // build the information that will be common to all animations of this mesh: silhouette edge connectivity and normal / tangent generation information
            //
            //DG: windows only has a 1MB stack and it could happen that we try to allocate >1MB here (in lost mission mod, game/le_hell map), causing a stack overflow to prevent that, use heap allocation if it's >600KB
            var verts = texCoords.Length * sizeof(DrawVert) < 600000
                ? stackalloc DrawVert[texCoords.Length + DrawVert.ALLOC16]
                : new DrawVert[texCoords.Length + DrawVert.ALLOC16];
            verts = Platform._alloca16(verts);

            for (i = 0; i < texCoords.Length; i++) { verts[i].Clear(); verts[i].st = texCoords[i]; }
            fixed (DrawVert* vertsD = verts)
            fixed (JointMat* jointsJ = joints)
                TransformVerts(vertsD, jointsJ);
            deformInfo = R_BuildDeformInfo(texCoords.Length, verts, tris.Length, tris, shader.UseUnsmoothedTangents);
        }

        void TransformVerts(DrawVert* verts, JointMat* joints)
        {
            fixed (Vector4* scaledWeightsV = scaledWeights)
            fixed (int* weightIndexI = weightIndex)
                Simd.TransformVerts(verts, texCoords.Length, joints, scaledWeightsV, weightIndexI, numWeights);
        }

        // Special transform to make the mesh seem fat or skinny.  May be used for zombie deaths
        void TransformScaledVerts(DrawVert* verts, JointMat* joints, float scale)
        {
            var scaledWeights = stackalloc Vector4[numWeights + Vector4.ALLOC16]; scaledWeights = _alloca16(scaledWeights);
            Simd.Mul(&scaledWeights[0].x, scale, &scaledWeights[0].x, numWeights * 4);
            fixed (int* weightIndexI = weightIndex)
                Simd.TransformVerts(verts, texCoords.Length, joints, scaledWeights, weightIndexI, numWeights);
        }

        public void UpdateSurface(RenderEntity ent, JointMat[] joints, ModelSurface surf)
        {
            int i;

            tr.pc.c_deformedSurfaces++;
            tr.pc.c_deformedVerts += deformInfo.numOutputVerts;
            tr.pc.c_deformedIndexes += deformInfo.numIndexes;

            surf.shader = shader;

            if (surf.geometry != null)
            {
                // if the number of verts and indexes are the same we can re-use the triangle surface the number of indexes must be the same to assure the correct amount of memory is allocated for the facePlanes
                if (surf.geometry.numVerts == deformInfo.numOutputVerts && surf.geometry.numIndexes == deformInfo.numIndexes) R_FreeStaticTriSurfVertexCaches(surf.geometry);
                else { R_FreeStaticTriSurf(surf.geometry); surf.geometry = R_AllocStaticTriSurf(); }
            }
            else surf.geometry = R_AllocStaticTriSurf();

            var tri = surf.geometry;

            // note that some of the data is references, and should not be freed
            tri.deformedSurface = true;
            tri.tangentsCalculated = false;
            tri.facePlanesCalculated = false;

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

            if (tri.verts == null)
            {
                R_AllocStaticTriSurfVerts(tri, tri.numVerts);
                for (i = 0; i < deformInfo.numSourceVerts; i++) { tri.verts[i].Clear(); tri.verts[i].st = texCoords[i]; }
            }

            fixed (JointMat* jointsJ = joints)
                if (ent.shaderParms[IRenderWorld.SHADERPARM_MD5_SKINSCALE] != 0f) TransformScaledVerts(tri.verts, jointsJ, ent.shaderParms[IRenderWorld.SHADERPARM_MD5_SKINSCALE]);
                else TransformVerts(tri.verts, jointsJ);

            // replicate the mirror seam vertexes
            var base_ = deformInfo.numOutputVerts - deformInfo.numMirroredVerts;
            for (i = 0; i < deformInfo.numMirroredVerts; i++) tri.verts[base_ + i] = tri.verts[deformInfo.mirroredVerts[i]];

            R_BoundTriSurf(tri);

            // If a surface is going to be have a lighting interaction generated, it will also have to call R_DeriveTangents() to get normals, tangents, and face planes.  If it only
            // needs shadows generated, it will only have to generate face planes.  If it only has ambient drawing, or is culled, no additional work will be necessary
            if (!r_useDeferredTangents.Bool) R_DeriveTangents(tri); // set face planes, vertex normals, tangents
        }

        public Bounds CalcBounds(JointMat[] joints)
        {
            Bounds bounds = new();

            var verts = texCoords.Length * sizeof(DrawVert) < 600000
                ? stackalloc DrawVert[texCoords.Length + DrawVert.ALLOC16]
                : new DrawVert[texCoords.Length + DrawVert.ALLOC16];
            verts = _alloca16T(verts);

            fixed (DrawVert* vertsD = verts)
            fixed (JointMat* jointsJ = joints)
            {
                TransformVerts(vertsD, jointsJ);

                Simd.MinMaxd(out bounds[0], out bounds[1], vertsD, texCoords.Length);
            }

            return bounds;
        }

        public int NearestJoint(int a, int b, int c)
        {
            int i, bestJoint, vertNum, weightVertNum; float bestWeight;

            // duplicated vertices might not have weights
            if (a >= 0 && a < texCoords.Length) vertNum = a;
            else if (b >= 0 && b < texCoords.Length) vertNum = b;
            else if (c >= 0 && c < texCoords.Length) vertNum = c;
            else return 0; // all vertices are duplicates which shouldn't happen

            // find the first weight for this vertex
            weightVertNum = 0;
            for (i = 0; weightVertNum < vertNum; i++) weightVertNum += weightIndex[i * 2 + 1];

            // get the joint for the largest weight
            bestWeight = scaledWeights[i].w;
            bestJoint = weightIndex[i * 2 + 0] / sizeof(JointMat);
            for (; weightIndex[i * 2 + 1] == 0; i++) if (scaledWeights[i].w > bestWeight) { bestWeight = scaledWeights[i].w; bestJoint = weightIndex[i * 2 + 0] / sizeof(JointMat); }
            return bestJoint;
        }

        public int NumVerts
            => texCoords.Length;

        public int NumTris
            => numTris;

        public int NumWeights
            => numWeights;
    }

    public unsafe class RenderModelMD5 : RenderModelStatic
    {
        const string MD5_SnapshotName = "_MD5_Snapshot_";

        MD5Joint[] joints;
        JointQuat[] defaultPose;
        JointMat[] invertedDefaultPose;
        MD5Mesh[] meshes;

        void ParseJoint(Lexer parser, MD5Joint joint, JointQuat defaultPose)
        {
            // parse name
            parser.ReadToken(out var token); joint.name = token;

            // parse parent
            var num = parser.ParseInt();
            if (num < 0) joint.parent = null;
            else
            {
                if (num >= joints.Length - 1) parser.Error($"Invalid parent for joint '{joint.name}'");
                joint.parent = joints[num];
            }

            // parse default pose
            parser.Parse1DMatrix(3, &defaultPose.t.x);
            parser.Parse1DMatrix(3, &defaultPose.q.x);
            defaultPose.q.w = defaultPose.q.CalcW();
        }

        public override void InitFromFile(string fileName)
        {
            name = fileName;
            LoadModel();
        }

        // used for initial loads, reloadModel, and reloading the data of purged models Upon exit, the model will absolutely be valid, but possibly as a default model
        public override void LoadModel()
        {
            int version, i, num;
            Lexer parser = new(LEXFL.ALLOWPATHNAMES | LEXFL.NOSTRINGESCAPECHARS);

            if (!purged) PurgeModel(); purged = false;

            if (!parser.LoadFile(name)) { MakeDefaultModel(); return; }

            parser.ExpectTokenString(ModelX.MD5_VERSION_STRING); version = parser.ParseInt();
            if (version != ModelX.MD5_VERSION) parser.Error($"Invalid version {version}.  Should be version {ModelX.MD5_VERSION}\n");

            // skip commandline
            parser.ExpectTokenString("commandline"); parser.ReadToken(out var token);

            // parse num joints
            parser.ExpectTokenString("numJoints"); num = parser.ParseInt();
            joints = new MD5Joint[num];
            defaultPose = new JointQuat[num];
            var poseMat3 = stackalloc JointMat[num + JointMat.ALLOC16]; poseMat3 = _alloca16T(poseMat3);

            // parse num meshes
            parser.ExpectTokenString("numMeshes"); num = parser.ParseInt();
            if (num < 0) parser.Error($"Invalid size: {num}");
            meshes = new MD5Mesh[num];

            // parse joints
            parser.ExpectTokenString("joints"); parser.ExpectTokenString("{");

            for (i = 0; i < joints.Length; i++)
            {
                var pose = defaultPose[i];
                var joint = joints[i];
                ParseJoint(parser, joint, pose);
                poseMat3[i].SetRotation(pose.q.ToMat3());
                poseMat3[i].SetTranslation(pose.t);
                if (joint.parent != null)
                {
                    var parentNum = joint.parent - joints;
                    pose.q = (poseMat3[i].ToMat3() * poseMat3[parentNum].ToMat3().Transpose()).ToQuat();
                    pose.t = (poseMat3[i].ToVec3() - poseMat3[parentNum].ToVec3()) * poseMat3[parentNum].ToMat3().Transpose();
                }
            }
            parser.ExpectTokenString("}");

            //-----------------------------------------
            // create the inverse of the base pose joints to support tech6 style deformation of base pose vertexes, normals, and tangents.
            //
            // vertex * joints * inverseJoints == vertex when joints is the base pose When the joints are in another pose, it gives the animated vertex position
            //-----------------------------------------
            invertedDefaultPose = new JointMat[RenderWorldX.SIMD_ROUND_JOINTS(joints.Length)];
            for (i = 0; i < joints.Length; i++)
            {
                invertedDefaultPose[i] = poseMat3[i];
                invertedDefaultPose[i].Invert();
            }
            RenderWorldX.SIMD_INIT_LAST_JOINT(invertedDefaultPose, joints.Length);

            for (i = 0; i < meshes.Length; i++)
            {
                var isPDAmesh = false;
                parser.ExpectTokenString("mesh"); meshes[i].ParseMesh(parser, defaultPose.Length, poseMat3);

                // Koz begin: Remove hands from weapon & pda viewmodels if desired.
                var materialName = meshes[i].shader.Name;
                if (string.IsNullOrEmpty(materialName)) meshes[i].shader = null;
                // change material to _pdaImage instead of deault this allows rendering the PDA & swf menus to the model ingame. if we find this gui, we also need to add a surface to the model, so flag.
                else if (materialName == "textures/common/pda_gui" || materialName == "_pdaImage" || materialName == "_pdaimage") { meshes[i].shader = declManager.FindMaterial("_pdaImage"); isPDAmesh = true; }

                if (isPDAmesh)
                {
                    common.Printf("Load pda model\n");
                    for (var ti = 0; ti < meshes[i].NumVerts; ti++)
                        common.Printf($"Numverts {meshes[i].NumVerts} Vert {ti} {meshes[i].deformInfo.verts[ti].xyz.x} {meshes[i].deformInfo.verts[ti].xyz.y} {meshes[i].deformInfo.verts[ti].xyz.z} : {meshes[i].deformInfo.verts[ti].TexCoordS} {meshes[i].deformInfo.verts[ti].TexCoordT} {meshes[i].deformInfo.verts[ti].st[0]} {meshes[i].deformInfo.verts[ti].st[1]}\n");
                    common.Printf("PDA gui found, creating gui surface for hitscan.\n");

                    var pdasurface = new ModelSurface { id = 0, shader = declManager.FindMaterial("_pdaImage") };
                    var pdageometry = AllocSurfaceTriangles(meshes[i].NumVerts, meshes[i].deformInfo.numIndexes);
                    Debug.Assert(pdageometry != null);

                    // infinite bounds
                    pdageometry.bounds[0].x = pdageometry.bounds[0].y = pdageometry.bounds[0].z = -99999;
                    pdageometry.bounds[1].x = pdageometry.bounds[1].y = pdageometry.bounds[1].z = 99999;
                    pdageometry.numVerts = meshes[i].NumVerts;
                    pdageometry.numIndexes = meshes[i].deformInfo.numIndexes;

                    for (var zz = 0; zz < pdageometry.numIndexes; zz++) pdageometry.indexes[zz] = meshes[i].deformInfo.indexes[zz];
                    for (var zz = 0; zz < pdageometry.numVerts; zz++)
                    {
                        pdageometry.verts[zz].xyz = meshes[i].deformInfo.verts[zz].xyz;
                        //pdageometry.verts[zz].SetTexCoord( meshes[i].deformInfo.verts[zz].GetTexCoord() );
                        pdageometry.verts[zz].st = meshes[i].deformInfo.verts[zz].st;
                    }

                    common.Printf("verify pda model\n");
                    for (var ti = 0; ti < pdageometry.numVerts; ti++)
                        common.Printf($"Numverts {pdageometry.numVerts} Vert {ti} {pdageometry.verts[ti].xyz.x} {pdageometry.verts[ti].xyz.y} {pdageometry.verts[ti].xyz.z} : {pdageometry.verts[ti].TexCoordS} {pdageometry.verts[ti].TexCoordT} {pdageometry.verts[ti].st[0]} {pdageometry.verts[ti].st[1]}\n");
                    pdasurface.geometry = pdageometry;
                    AddSurface(pdasurface);
                }
            }

            // calculate the bounds of the model
            CalculateBounds(poseMat3);

            // set the timestamp for reloadmodels
            fileSystem.ReadFile(name, out var timeStamp);
        }

        public override void Print()
        {
            common.Printf($"{name}\n");
            common.Printf("Dynamic model.\n");
            common.Printf("Generated smooth normals.\n");
            common.Printf("    verts  tris weights material\n");
            int totalVerts = 0, totalTris = 0, totalWeights = 0;
            for (var i = 0; i < meshes.Length; i++)
            {
                var mesh = meshes[i];
                totalVerts += mesh.NumVerts;
                totalTris += mesh.NumTris;
                totalWeights += mesh.NumWeights;
                common.Printf($"{i,2}: {mesh.NumVerts,5} {mesh.NumTris,5} {mesh.NumWeights,7} {mesh.shader.Name}\n");
            }
            common.Printf("-----\n");
            common.Printf($"{totalVerts,4} verts.\n");
            common.Printf($"{totalTris,4} tris.\n");
            common.Printf($"{totalWeights,4} weights.\n");
            common.Printf($"{joints.Length,4} joints.\n");
        }

        public override void List()
        {
            int totalTris = 0, totalVerts = 0;
            foreach (var mesh in meshes) { totalTris += mesh.NumTris; totalVerts += mesh.NumVerts; }
            common.Printf($" {Memory / 1024,4}k {meshes.Length,3} {totalVerts,4} {totalTris,4} {name}(MD5)");
            if (defaulted) common.Printf(" (DEFAULTED)");
            common.Printf("\n");
        }

        void CalculateBounds(JointMat[] joints)
        {
            bounds.Clear();
            foreach (var mesh in meshes) bounds.AddBounds(mesh.CalcBounds(joints));
        }

        // This calculates a rough bounds by using the joint radii without transforming all the points
        public override Bounds Bounds(RenderEntity ent)
            => ent == null
            ? bounds // this is the bounds for the reference pose
            : ent.bounds;

        //void GetFrameBounds(RenderEntity ent, out Bounds bounds);

        void DrawJoints(RenderEntity ent, ViewDef view)
        {
            int i; Vector3 pos;

            var num = ent.numJoints;
            for (i = 0; i < num; i++)
            {
                var joint = ent.joints[i];
                var md5Joint = joints[i];
                pos = ent.origin + joint.ToVec3() * ent.axis;
                if (md5Joint.parent != null)
                {
                    var parentNum = md5Joint.parent - joints;
                    session.rw.DebugLine(colorWhite, ent.origin + ent.joints[parentNum].ToVec3() * ent.axis, pos);
                }

                session.rw.DebugLine(colorRed, pos, pos + joint.ToMat3()[0] * 2f * ent.axis);
                session.rw.DebugLine(colorGreen, pos, pos + joint.ToMat3()[1] * 2f * ent.axis);
                session.rw.DebugLine(colorBlue, pos, pos + joint.ToMat3()[2] * 2f * ent.axis);
            }

            Bounds bounds = new();
            bounds.FromTransformedBounds(ent.bounds, Vector3.origin, ent.axis);
            session.rw.DebugBounds(colorMagenta, bounds, ent.origin);

            if (r_jointNameScale.Float != 0f && bounds.Expand(128f).ContainsPoint(view.renderView.vieworg - ent.origin))
            {
                Vector3 offset = new(0f, 0f, r_jointNameOffset.Float);
                var scale = r_jointNameScale.Float;
                num = ent.numJoints;
                for (i = 0; i < num; i++)
                {
                    var joint = ent.joints[i];
                    pos = ent.origin + joint.ToVec3() * ent.axis;
                    session.rw.DrawText(joints[i].name, pos + offset, scale, colorWhite, view.renderView.viewaxis, 1);
                }
            }
        }

        public override IRenderModel InstantiateDynamicModel(RenderEntity ent, ViewDef view, IRenderModel cachedModel)
        {
            int i, surfaceNum; MD5Mesh mesh; RenderModelStatic staticModel;

            if (cachedModel != null && !r_useCachedDynamicModels.Bool)
            {
                cachedModel.Dispose();
                cachedModel = null;
            }

            if (purged) { common.DWarning($"model {Name} instantiated while purged"); LoadModel(); }

            if (ent.joints == null)
            {
                common.Printf($"RenderModelMD5::InstantiateDynamicModel: null joints on renderEntity for '{Name}'\n");
                cachedModel.Dispose();
                return null;
            }
            else if (ent.numJoints != joints.Length)
            {
                common.Printf($"RenderModelMD5::InstantiateDynamicModel: renderEntity has different number of joints than model for '{Name}'\n");
                cachedModel.Dispose();
                return null;
            }

            tr.pc.c_generateMd5++;

            if (cachedModel != null)
            {
                Debug.Assert(cachedModel is RenderModelStatic);
                Debug.Assert(string.Equals(cachedModel.Name, MD5_SnapshotName, StringComparison.OrdinalIgnoreCase));
                staticModel = (RenderModelStatic)cachedModel;
            }
            else
            {
                staticModel = new RenderModelStatic();
                staticModel.InitEmpty(MD5_SnapshotName);
            }

            staticModel.bounds.Clear();

            if (r_showSkel.Integer != 0)
            {
                if (view != null && (!r_skipSuppress.Bool || ent.suppressSurfaceInViewID == 0 || ent.suppressSurfaceInViewID != view.renderView.viewID)) DrawJoints(ent, view); // only draw the skeleton
                if (r_showSkel.Integer > 1) { staticModel.InitEmpty(MD5_SnapshotName); return staticModel; } // turn off the model when showing the skeleton
            }

            // create all the surfaces
            for (i = 0; i < meshes.Length; i++)
            {
                mesh = meshes[i];

                // avoid deforming the surface if it will be a nodraw due to a skin remapping. FIXME: may have to still deform clipping hulls
                var shader = mesh.shader;

                shader = R_RemapShaderBySkin(shader, ent.customSkin, ent.customShader);

                if (shader == null || (!shader.IsDrawn && !shader.SurfaceCastsShadow)) { staticModel.DeleteSurfaceWithId(i); mesh.surfaceNum = -1; continue; }

                ModelSurface surf;
                if (staticModel.FindSurfaceWithId(i, out surfaceNum))
                {
                    mesh.surfaceNum = surfaceNum;
                    surf = staticModel.surfaces[surfaceNum];
                }
                else
                {
                    // Remove Overlays before adding new surfaces
                    RenderModelOverlay.RemoveOverlaySurfacesFromModel(staticModel);

                    mesh.surfaceNum = staticModel.NumSurfaces;
                    surf = staticModel.surfaces.Alloc();
                    surf.geometry = null;
                    surf.shader = null;
                    surf.id = i;
                }

                mesh.UpdateSurface(ent, ent.joints, surf);

                staticModel.bounds.AddPoint(surf.geometry.bounds[0]);
                staticModel.bounds.AddPoint(surf.geometry.bounds[1]);
            }

            return staticModel;
        }

        public override DynamicModel IsDynamicModel
            => DynamicModel.DM_CACHED;

        public override int NumJoints
            => joints.Length;

        public override MD5Joint[] Joints
            => joints;

        public override JointQuat[] DefaultPose
            => defaultPose;

        public override JointHandle GetJointHandle(string name)
        {
            for (var i = 0; i < joints.Length; i++) if (string.Equals(joints[i].name, name, StringComparison.OrdinalIgnoreCase)) return (JointHandle)i;
            return JointHandle.INVALID_JOINT;
        }

        public override string GetJointName(JointHandle handle)
            => handle < 0 || (int)handle >= joints.Length
                ? "<invalid joint>"
                : joints[(int)handle].name;

        public override int NearestJoint(int surfaceNum, int a, int b, int c)
        {
            if (surfaceNum > meshes.Length) common.Error("RenderModelMD5::NearestJoint: surfaceNum > meshes.Length");
            foreach (var mesh in meshes) if (mesh.surfaceNum == surfaceNum) return mesh.NearestJoint(a, b, c);
            return 0;
        }

        // models that are already loaded at level start time will still touch their materials to make sure they are kept loaded
        public override void TouchData()
        {
            foreach (var mesh in meshes)
                declManager.FindMaterial(mesh.shader.Name);
        }

        // frees all the data, but leaves the class around for dangling references, which can regenerate the data with LoadModel()
        public override void PurgeModel()
        {
            purged = true;
            joints = null;
            defaultPose = null;
            meshes = null;
        }

        public override int Memory
            => 0;
    }
}