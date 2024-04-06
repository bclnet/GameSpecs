using GameX.Platforms;
using OpenStack.Graphics;
using System;
using System.Collections.Generic;
using UnityEngine;
using static OpenStack.Debug;

namespace GameX.Bethesda.Formats.Unity
{
    public class NifObjectBuilder
    {
        public enum MatTestMode { Always, Less, LEqual, Equal, GEqual, Greater, NotEqual, Never }

        class FixedMaterialInfo : IFixedMaterial
        {
            public string Name { get; set; }
            public string ShaderName { get; set; }
            public IDictionary<string, bool> GetShaderArgs() => null;
            public IDictionary<string, object> Data { get; set; }
            public string MainFilePath { get; set; }
            public string DarkFilePath { get; set; }
            public string DetailFilePath { get; set; }
            public string GlossFilePath { get; set; }
            public string GlowFilePath { get; set; }
            public string BumpFilePath { get; set; }
            public bool AlphaBlended { get; set; }
            public int SrcBlendMode { get; set; }
            public int DstBlendMode { get; set; }
            public bool AlphaTest { get; set; }
            public float AlphaCutoff { get; set; }
            public bool ZWrite { get; set; }
        }

        const bool KinematicRigidbodies = true;
        readonly NiFile _obj;
        readonly IMaterialManager<Material, Texture2D> _materialManager;
        readonly int _markerLayer;

        public NifObjectBuilder(NiFile obj, IMaterialManager<Material, Texture2D> materialManager, int markerLayer)
        {
            _obj = obj;
            _materialManager = materialManager;
            _markerLayer = markerLayer;
        }

        public GameObject BuildObject()
        {
            Assert(_obj.Name != null && _obj.Footer.Roots.Length > 0);

            // NIF files can have any number of root NiObjects.
            // If there is only one root, instantiate that directly.
            // If there are multiple roots, create a container GameObject and parent it to the roots.
            if (_obj.Footer.Roots.Length == 1)
            {
                var rootNiObject = _obj.Blocks[_obj.Footer.Roots[0]];
                var gameObject = InstantiateRootNiObject(rootNiObject);
                // If the file doesn't contain any NiObjects we are looking for, return an empty GameObject.
                if (gameObject == null)
                {
                    Log($"{_obj.Name} resulted in a null GameObject when instantiated.");
                    gameObject = new GameObject(_obj.Name);
                }
                // If gameObject != null and the root NiObject is an NiNode, discard any transformations (Morrowind apparently does).
                else if (rootNiObject is NiNode)
                {
                    gameObject.transform.position = Vector3.zero;
                    gameObject.transform.rotation = Quaternion.identity;
                    gameObject.transform.localScale = Vector3.one;
                }
                return gameObject;
            }
            else
            {
                Log($"{_obj.Name} has multiple roots.");
                var gameObject = new GameObject(_obj.Name);
                foreach (var rootRef in _obj.Footer.Roots)
                {
                    var child = InstantiateRootNiObject(_obj.Blocks[rootRef]);
                    if (child != null) child.transform.SetParent(gameObject.transform, false);
                }
                return gameObject;
            }
        }

        GameObject InstantiateRootNiObject(NiObject obj)
        {
            var gameObject = InstantiateNiObject(obj);
            ProcessExtraData(obj, out var shouldAddMissingColliders, out var isMarker);
            if (_obj.Name != null && IsMarkerFileName(_obj.Name))
            {
                shouldAddMissingColliders = false;
                isMarker = true;
            }
            // Add colliders to the object if it doesn't already contain one.
            if (shouldAddMissingColliders && gameObject.GetComponentInChildren<Collider>() == null) GameObjectUtils.AddMissingMeshCollidersRecursively(gameObject);
            if (isMarker) GameObjectUtils.SetLayerRecursively(gameObject, _markerLayer);
            return gameObject;
        }

        void ProcessExtraData(NiObject obj, out bool shouldAddMissingColliders, out bool isMarker)
        {
            shouldAddMissingColliders = true;
            isMarker = false;
            if (obj is NiObjectNET objNET)
            {
                var extraData = objNET.ExtraData.Value >= 0 ? (NiExtraData)_obj.Blocks[objNET.ExtraData.Value] : null;
                while (extraData != null)
                {
                    if (extraData is NiStringExtraData strExtraData)
                    {
                        if (strExtraData.Str == "NCO" || strExtraData.Str == "NCC") shouldAddMissingColliders = false;
                        else if (strExtraData.Str == "MRK") { shouldAddMissingColliders = false; isMarker = true; }
                    }
                    // Move to the next NiExtraData.
                    extraData = extraData.NextExtraData.Value >= 0 ? (NiExtraData)_obj.Blocks[extraData.NextExtraData.Value] : null;
                }
            }
        }

        /// <summary>
        /// Creates a GameObject representation of an NiObject.
        /// </summary>
        /// <returns>Returns the created GameObject, or null if the NiObject does not need its own GameObject.</returns>
        GameObject InstantiateNiObject(NiObject obj)
        {
            if (obj.GetType() == typeof(NiNode)) return InstantiateNiNode((NiNode)obj);
            else if (obj.GetType() == typeof(NiBSAnimationNode)) return InstantiateNiNode((NiNode)obj);
            else if (obj.GetType() == typeof(NiTriShape)) return InstantiateNiTriShape((NiTriShape)obj, true, false);
            else if (obj.GetType() == typeof(RootCollisionNode)) return InstantiateRootCollisionNode((RootCollisionNode)obj);
            else if (obj.GetType() == typeof(NiTextureEffect)) return null;
            else if (obj.GetType() == typeof(NiBSAnimationNode)) return null;
            else if (obj.GetType() == typeof(NiBSParticleNode)) return null;
            else if (obj.GetType() == typeof(NiRotatingParticles)) return null;
            else if (obj.GetType() == typeof(NiAutoNormalParticles)) return null;
            else if (obj.GetType() == typeof(NiBillboardNode)) return null;
            else throw new NotImplementedException($"Tried to instantiate an unsupported NiObject ({obj.GetType().Name}).");
        }

        GameObject InstantiateNiNode(NiNode node)
        {
            var obj = new GameObject(node.Name);
            foreach (var childIndex in node.Children)
                // NiNodes can have child references < 0 meaning null.
                if (!childIndex.IsNull)
                {
                    var child = InstantiateNiObject(_obj.Blocks[childIndex.Value]);
                    if (child != null) child.transform.SetParent(obj.transform, false);
                }
            ApplyNiAVObject(node, obj);
            return obj;
        }

        GameObject InstantiateNiTriShape(NiTriShape triShape, bool visual, bool collidable)
        {
            Assert(visual || collidable);
            var mesh = NiTriShapeDataToMesh((NiTriShapeData)_obj.Blocks[triShape.Data.Value]);
            var obj = new GameObject(triShape.Name);
            if (visual)
            {
                obj.AddComponent<MeshFilter>().mesh = mesh;
                var materialProps = NiAVObjectPropertiesToMaterialProperties(triShape);
                var meshRenderer = obj.AddComponent<MeshRenderer>();
                meshRenderer.material = _materialManager.LoadMaterial(materialProps, out var _);
                if (triShape.Flags.HasFlag(NiAVObject.NiFlags.Hidden)) meshRenderer.enabled = false;
                obj.isStatic = true;
            }
            if (collidable)
            {
                obj.AddComponent<MeshCollider>().sharedMesh = mesh;
                if (KinematicRigidbodies) obj.AddComponent<Rigidbody>().isKinematic = true;
            }
            ApplyNiAVObject(triShape, obj);
            return obj;
        }

        GameObject InstantiateRootCollisionNode(RootCollisionNode collisionNode)
        {
            var obj = new GameObject("Root Collision Node");
            // NiNodes can have child references < 0 meaning null.
            foreach (var childIndex in collisionNode.Children) if (!childIndex.IsNull) AddColliderFromNiObject(_obj.Blocks[childIndex.Value], obj);
            ApplyNiAVObject(collisionNode, obj);
            return obj;
        }

        void ApplyNiAVObject(NiAVObject niAVObject, GameObject obj)
        {
            obj.transform.position = niAVObject.Translation.ToUnity(ConvertUtils.MeterInUnits);
            obj.transform.rotation = niAVObject.Rotation.ToUnityQuaternionAsRotationMatrix();
            obj.transform.localScale = niAVObject.Scale * Vector3.one;
        }

        Mesh NiTriShapeDataToMesh(NiTriShapeData data)
        {
            // vertex positions
            var vertices = new Vector3[data.Vertices.Length];
            for (var i = 0; i < vertices.Length; i++) vertices[i] = data.Vertices[i].ToUnity(ConvertUtils.MeterInUnits);
            // vertex normals
            Vector3[] normals = null;
            if (data.HasNormals)
            {
                normals = new Vector3[vertices.Length];
                for (var i = 0; i < normals.Length; i++) normals[i] = data.Normals[i].ToUnity();
            }
            // vertex UV coordinates
            Vector2[] UVs = null;
            if (data.HasUV)
            {
                UVs = new Vector2[vertices.Length];
                for (var i = 0; i < UVs.Length; i++) { var NiTexCoord = data.UVSets[0, i]; UVs[i] = new Vector2(NiTexCoord.u, NiTexCoord.v); }
            }
            // triangle vertex indices
            var triangles = new int[data.NumTrianglePoints];
            for (var i = 0; i < data.Triangles.Length; i++)
            {
                var baseI = 3 * i;
                // Reverse triangle winding order.
                triangles[baseI] = data.Triangles[i].v1;
                triangles[baseI + 1] = data.Triangles[i].v3;
                triangles[baseI + 2] = data.Triangles[i].v2;
            }

            // Create the mesh.
            var mesh = new Mesh
            {
                vertices = vertices,
                normals = normals,
                uv = UVs,
                triangles = triangles
            };
            if (!data.HasNormals) mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        IMaterial NiAVObjectPropertiesToMaterialProperties(NiAVObject obj)
        {
            // Find relevant properties.
            NiTexturingProperty texturingProperty = null;
            //NiMaterialProperty materialProperty = null;
            NiAlphaProperty alphaProperty = null;
            foreach (var propRef in obj.Properties)
            {
                var prop = _obj.Blocks[propRef.Value];
                if (prop is NiTexturingProperty) texturingProperty = (NiTexturingProperty)prop;
                //else if (prop is NiMaterialProperty) materialProperty = (NiMaterialProperty)prop;
                else if (prop is NiAlphaProperty) alphaProperty = (NiAlphaProperty)prop;
            }

            // Create the material properties.
            var mp = new FixedMaterialInfo();

            if (alphaProperty != null)
            {
                #region AlphaProperty Cheat Sheet
                /*
                14 bits used:

                1 bit for alpha blend bool
                4 bits for src blend mode
                4 bits for dest blend mode
                1 bit for alpha test bool
                3 bits for alpha test mode
                1 bit for zwrite bool ( opposite value )

                Bit 0 : alpha blending enable
                Bits 1-4 : source blend mode 
                Bits 5-8 : destination blend mode
                Bit 9 : alpha test enable
                Bit 10-12 : alpha test mode
                Bit 13 : no sorter flag ( disables triangle sorting ) ( Unity ZWrite )

                blend modes (glBlendFunc):
                0000 GL_ONE
                0001 GL_ZERO
                0010 GL_SRC_COLOR
                0011 GL_ONE_MINUS_SRC_COLOR
                0100 GL_DST_COLOR
                0101 GL_ONE_MINUS_DST_COLOR
                0110 GL_SRC_ALPHA
                0111 GL_ONE_MINUS_SRC_ALPHA
                1000 GL_DST_ALPHA
                1001 GL_ONE_MINUS_DST_ALPHA
                1010 GL_SRC_ALPHA_SATURATE

                test modes (glAlphaFunc):
                000 GL_ALWAYS
                001 GL_LESS
                010 GL_EQUAL
                011 GL_LEQUAL
                100 GL_GREATER
                101 GL_NOTEQUAL
                110 GL_GEQUAL
                111 GL_NEVER
                */
                #endregion
                var flags = alphaProperty.Flags;
                var oldflags = flags;
                var srcbm = (byte)(BitConverter.GetBytes(flags >> 1)[0] & 15);
                var dstbm = (byte)(BitConverter.GetBytes(flags >> 5)[0] & 15);
                mp.ZWrite = BitConverter.GetBytes(flags >> 15)[0] == 1; // smush
                if ((flags & 0x01) == 0x01) // if flags contain the alpha blend flag at bit 0 in byte 0
                {
                    mp.AlphaBlended = true;
                    mp.SrcBlendMode = (int)FigureBlendMode(srcbm);
                    mp.DstBlendMode = (int)FigureBlendMode(dstbm);
                }
                else if ((flags & 0x100) == 0x100) // if flags contain the alpha test flag
                {
                    mp.AlphaTest = true;
                    mp.AlphaCutoff = (float)alphaProperty.Threshold / 255;
                }
            }
            else
            {
                mp.AlphaBlended = false;
                mp.AlphaTest = false;
            }
            // Apply textures.
            if (texturingProperty != null) ConfigureTextureProperties(mp, texturingProperty);
            return mp;
        }

        void ConfigureTextureProperties(FixedMaterialInfo info, NiTexturingProperty ntp)
        {
            if (ntp.TextureCount < 1) return;
            if (ntp.BaseTexture != null) { var src = (NiSourceTexture)_obj.Blocks[ntp.BaseTexture.source.Value]; info.MainFilePath = src.FileName; }
            if (ntp.DarkTexture != null) { var src = (NiSourceTexture)_obj.Blocks[ntp.DarkTexture.source.Value]; info.DarkFilePath = src.FileName; }
            if (ntp.DetailTexture != null) { var src = (NiSourceTexture)_obj.Blocks[ntp.DetailTexture.source.Value]; info.DetailFilePath = src.FileName; }
            if (ntp.GlossTexture != null) { var src = (NiSourceTexture)_obj.Blocks[ntp.GlossTexture.source.Value]; info.GlossFilePath = src.FileName; }
            if (ntp.GlowTexture != null) { var src = (NiSourceTexture)_obj.Blocks[ntp.GlowTexture.source.Value]; info.GlowFilePath = src.FileName; }
            if (ntp.BumpMapTexture != null) { var src = (NiSourceTexture)_obj.Blocks[ntp.BumpMapTexture.source.Value]; info.BumpFilePath = src.FileName; }
        }

        UnityEngine.Rendering.BlendMode FigureBlendMode(byte b) => (UnityEngine.Rendering.BlendMode)Mathf.Min(b, 10);

        MatTestMode FigureTestMode(byte b) => (MatTestMode)Mathf.Min(b, 7);

        void AddColliderFromNiObject(NiObject niObject, GameObject gameObject)
        {
            if (niObject.GetType() == typeof(NiTriShape)) { var colliderObj = InstantiateNiTriShape((NiTriShape)niObject, false, true); colliderObj.transform.SetParent(gameObject.transform, false); }
            else if (niObject.GetType() == typeof(AvoidNode)) { }
            else Log("Unsupported collider NiObject: " + niObject.GetType().Name);
        }

        bool IsMarkerFileName(string name)
        {
            var lowerName = name.ToLower();
            return lowerName == "marker_light" ||
                lowerName == "marker_north" ||
                lowerName == "marker_error" ||
                lowerName == "marker_arrow" ||
                lowerName == "editormarker" ||
                lowerName == "marker_creature" ||
                lowerName == "marker_travel" ||
                lowerName == "marker_temple" ||
                lowerName == "marker_prison" ||
                lowerName == "marker_radius" ||
                lowerName == "marker_divine" ||
                lowerName == "editormarker_box_01";
        }
    }
}