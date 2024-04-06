using System.Collections.Generic;
using System.NumericsX.OpenStack.Gngine.Render;

namespace System.NumericsX.OpenStack.Gngine.Framework
{
    public class RefSound
    {
        public ISoundEmitter referenceSound; // this is the interface to the sound system, created with SoundWorld::AllocSoundEmitter() when needed
        public Vector3 origin;
        public int listenerId;              // SSF_PRIVATE_SOUND only plays if == listenerId from PlaceListener no spatialization will be performed if == listenerID
        public ISoundShader shader;         // this really shouldn't be here, it is a holdover from single channel behavior
        public float diversity;             // 0.0 to 1.0 value used to select which samples in a multi-sample list from the shader are used
        public bool waitfortrigger;         // don't start it at spawn time
        public SoundShaderParms parms;      // override volume, flags, etc
    }

    enum TEST_PARTICLE
    {
        MODEL = 0,
        IMPACT,
        MUZZLE,
        FLIGHT,
        SELECTED
    }

    public interface IGameEdit
    {
        // These are the canonical idDict to parameter parsing routines used by both the game and tools.
        // These are the canonical idDict to parameter parsing routines used by both the game and tools.
        void ParseSpawnArgsToRenderLight(Dictionary<string, string> args, RenderLight renderLight);
        void ParseSpawnArgsToRenderEntity(Dictionary<string, string> args, RenderEntity renderEntity);
        void ParseSpawnArgsToRefSound(Dictionary<string, string> args, RefSound refSound);

        // Animation system calls for non-game based skeletal rendering.
        IRenderModel ANIM_GetModelFromEntityDef(string classname);
        Vector3 ANIM_GetModelOffsetFromEntityDef(string classname);
        IRenderModel ANIM_GetModelFromEntityDef(Dictionary<string, string> args);
        IRenderModel ANIM_GetModelFromName(string modelName);
        IMD5Anim ANIM_GetAnimFromEntityDef(string classname, string animname);
        int ANIM_GetNumAnimsFromEntityDef(Dictionary<string, string> args);
        string ANIM_GetAnimNameFromEntityDef(Dictionary<string, string> args, int animNum);
        IMD5Anim ANIM_GetAnim(string fileName);
        int ANIM_GetLength(IMD5Anim anim);
        int ANIM_GetNumFrames(IMD5Anim anim);
        void ANIM_CreateAnimFrame(IRenderModel model, IMD5Anim anim, int numJoints, JointMat[] frame, int time, in Vector3 offset, bool remove_origin_offset);
        IRenderModel ANIM_CreateMeshForAnim(IRenderModel model, string classname, string animname, int frame, bool remove_origin_offset);

        // Articulated Figure calls for AF editor and Radiant.
        bool AF_SpawnEntity(string fileName);
        void AF_UpdateEntities(string fileName);
        void AF_UndoChanges();
        IRenderModel AF_CreateMesh(Dictionary<string, string> args, in Vector3 meshOrigin, in Matrix3x3 meshAxis, out bool poseIsSet);

        // Entity selection.
        void ClearEntitySelection();
        int GetSelectedEntities(IEntity[] list, int max);
        void AddSelectedEntity(IEntity ent);

        // Selection methods
        void TriggerSelected();

        // Entity defs and spawning.
        Dictionary<string, string> FindEntityDefDict(string name, bool makeDefault = true);
        void SpawnEntityDef(Dictionary<string, string> args, IEntity[] ent);
        IEntity FindEntity(string name);
        string GetUniqueEntityName(string classname);

        // Entity methods.
        void EntityGetOrigin(IEntity ent, in Vector3 org);
        void EntityGetAxis(IEntity ent, in Matrix3x3 axis);
        void EntitySetOrigin(IEntity ent, in Vector3 org);
        void EntitySetAxis(IEntity ent, in Matrix3x3 axis);
        void EntityTranslate(IEntity ent, in Vector3 org);
        Dictionary<string, string> EntityGetSpawnArgs(IEntity ent);
        void EntityUpdateChangeableSpawnArgs(IEntity ent, Dictionary<string, string> dict);
        void EntityChangeSpawnArgs(IEntity ent, Dictionary<string, string> newArgs);
        void EntityUpdateVisuals(IEntity ent);
        void EntitySetModel(IEntity ent, string val);
        void EntityStopSound(IEntity ent);
        void EntityDelete(IEntity ent);
        void EntitySetColor(IEntity ent, in Vector3 color);

        // Player methods.
        bool PlayerIsValid();
        void PlayerGetOrigin(in Vector3 org);
        void PlayerGetAxis(in Matrix3x3 axis);
        void PlayerGetViewAngles(in Angles angles);
        void PlayerGetEyePosition(in Vector3 org);

        // In game map editing support.
        Dictionary<string, string> MapGetEntityDict(string name);
        void MapSave(string path = null);
        void MapSetEntityKeyVal(string name, string key, string val);
        void MapCopyDictToEntity(string name, Dictionary<string, string> dict);
        int MapGetUniqueMatchingKeyVals(string key, string[] list, int max);
        void MapAddEntity(Dictionary<string, string> dict);
        int MapGetEntitiesMatchingClassWithString(string classname, string match, string[] list, int max);
        void MapRemoveEntity(string name);
        void MapEntityTranslate(string name, in Vector3 v);
    }
}