using GameX.WbB.Formats.Entity.AnimationHooks;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameX.WbB.Formats.Entity
{
    public class AnimationHook : IHaveMetaInfo
    {
        public static readonly AnimationHook AnimDoneHook = new AnimationHook();
        protected readonly AnimationHook Base;
        public readonly AnimationHookType HookType;
        public readonly AnimationHookDir Direction;

        AnimationHook() => HookType = AnimationHookType.AnimationDone;
        protected AnimationHook(AnimationHook @base) => Base = @base;
        /// <summary>
        /// WARNING: If you're reading a hook from the dat, you should use AnimationHook.ReadHook(reader).
        /// If you read a hook from the dat using this function, it is likely you will not read all the data correctly.
        /// </summary>
        public AnimationHook(BinaryReader r)
        {
            HookType = (AnimationHookType)r.ReadUInt32();
            Direction = (AnimationHookDir)r.ReadInt32();
        }

        public static AnimationHook Factory(AnimationHook animationHook)
            => animationHook.HookType switch
            {
                AnimationHookType.AnimationDone => new AnimationHook(animationHook),
                AnimationHookType.Attack => new AttackHook(animationHook),
                AnimationHookType.CallPES => new CallPESHook(animationHook),
                AnimationHookType.CreateBlockingParticle => new AnimationHook(animationHook),
                AnimationHookType.CreateParticle => new CreateParticleHook(animationHook),
                AnimationHookType.DefaultScript => new AnimationHook(animationHook),
                AnimationHookType.DefaultScriptPart => new DefaultScriptPartHook(animationHook),
                AnimationHookType.DestroyParticle => new DestroyParticleHook(animationHook),
                AnimationHookType.Diffuse => new DiffuseHook(animationHook),
                AnimationHookType.DiffusePart => new DiffusePartHook(animationHook),
                AnimationHookType.Ethereal => new EtherealHook(animationHook),
                AnimationHookType.ForceAnimationHook32Bit => new AnimationHook(animationHook),
                AnimationHookType.Luminous => new LuminousHook(animationHook),
                AnimationHookType.LuminousPart => new LuminousPartHook(animationHook),
                AnimationHookType.NoDraw => new NoDrawHook(animationHook),
                AnimationHookType.NoOp => new AnimationHook(animationHook),
                AnimationHookType.ReplaceObject => new ReplaceObjectHook(animationHook),
                AnimationHookType.Scale => new ScaleHook(animationHook),
                AnimationHookType.SetLight => new SetLightHook(animationHook),
                AnimationHookType.SetOmega => new SetOmegaHook(animationHook),
                AnimationHookType.Sound => new SoundHook(animationHook),
                AnimationHookType.SoundTable => new SoundTableHook(animationHook),
                AnimationHookType.SoundTweaked => new SoundTweakedHook(animationHook),
                AnimationHookType.StopParticle => new StopParticleHook(animationHook),
                AnimationHookType.TextureVelocity => new TextureVelocityHook(animationHook),
                AnimationHookType.TextureVelocityPart => new TextureVelocityPartHook(animationHook),
                AnimationHookType.Transparent => new TransparentHook(animationHook),
                AnimationHookType.TransparentPart => new TransparentPartHook(animationHook),
                _ => new AnimationHook(animationHook),
            };

        public static AnimationHook Factory(BinaryReader r)
        {
            // We peek forward to get the hook type, then revert our position.
            var hookType = (AnimationHookType)r.ReadUInt32();
            r.Skip(-4);
            return hookType switch
            {
                AnimationHookType.Sound => new SoundHook(r),
                AnimationHookType.SoundTable => new SoundTableHook(r),
                AnimationHookType.Attack => new AttackHook(r),
                AnimationHookType.ReplaceObject => new ReplaceObjectHook(r),
                AnimationHookType.Ethereal => new EtherealHook(r),
                AnimationHookType.TransparentPart => new TransparentPartHook(r),
                AnimationHookType.Luminous => new LuminousHook(r),
                AnimationHookType.LuminousPart => new LuminousPartHook(r),
                AnimationHookType.Diffuse => new DiffuseHook(r),
                AnimationHookType.DiffusePart => new DiffusePartHook(r),
                AnimationHookType.Scale => new ScaleHook(r),
                AnimationHookType.CreateParticle => new CreateParticleHook(r),
                AnimationHookType.DestroyParticle => new DestroyParticleHook(r),
                AnimationHookType.StopParticle => new StopParticleHook(r),
                AnimationHookType.NoDraw => new NoDrawHook(r),
                AnimationHookType.DefaultScriptPart => new DefaultScriptPartHook(r),
                AnimationHookType.CallPES => new CallPESHook(r),
                AnimationHookType.Transparent => new TransparentHook(r),
                AnimationHookType.SoundTweaked => new SoundTweakedHook(r),
                AnimationHookType.SetOmega => new SetOmegaHook(r),
                AnimationHookType.TextureVelocity => new TextureVelocityHook(r),
                AnimationHookType.TextureVelocityPart => new TextureVelocityPartHook(r),
                AnimationHookType.SetLight => new SetLightHook(r),
                AnimationHookType.CreateBlockingParticle => new CreateBlockingParticle(r),
                // The following HookTypes have no additional properties:
                AnimationHookType.AnimationDone => new AnimationHook(r),
                AnimationHookType.DefaultScript => new AnimationHook(r),
                _ => throw new FormatException($"Not Implemented Hook type encountered: {hookType}"),
            };
        }

        //: Entity.AnimationHook
        public virtual List<MetaInfo> GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"Dir: {Direction}"),
            };
            return nodes;
        }

        //: Entity.AnimationHook
        public override string ToString() => $"HookType: {HookType}, Dir: {Direction}";
    }
}
