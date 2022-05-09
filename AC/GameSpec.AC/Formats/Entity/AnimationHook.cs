using GameSpec.AC.Formats.Entity.AnimationHooks;
using GameSpec.AC.Formats.Props;
using GameSpec.Metadata;
using GameSpec.Formats;
using System;
using System.Collections.Generic;
using System.IO;

namespace GameSpec.AC.Formats.Entity
{
    public class AnimationHook : IGetMetadataInfo
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
        {
            switch (animationHook.HookType)
            {
                case AnimationHookType.AnimationDone: break;
                case AnimationHookType.Attack: return new AttackHook(animationHook);
                case AnimationHookType.CallPES: return new CallPESHook(animationHook);
                case AnimationHookType.CreateBlockingParticle: break;
                case AnimationHookType.CreateParticle: return new CreateParticleHook(animationHook);
                case AnimationHookType.DefaultScript: break;
                case AnimationHookType.DefaultScriptPart: return new DefaultScriptPartHook(animationHook);
                case AnimationHookType.DestroyParticle: return new DestroyParticleHook(animationHook);
                case AnimationHookType.Diffuse: return new DiffuseHook(animationHook);
                case AnimationHookType.DiffusePart: return new DiffusePartHook(animationHook);
                case AnimationHookType.Ethereal: return new EtherealHook(animationHook);
                case AnimationHookType.ForceAnimationHook32Bit: break;
                case AnimationHookType.Luminous: return new LuminousHook(animationHook);
                case AnimationHookType.LuminousPart: return new LuminousPartHook(animationHook);
                case AnimationHookType.NoDraw: return new NoDrawHook(animationHook);
                case AnimationHookType.NoOp: break;
                case AnimationHookType.ReplaceObject: return new ReplaceObjectHook(animationHook);
                case AnimationHookType.Scale: return new ScaleHook(animationHook);
                case AnimationHookType.SetLight: return new SetLightHook(animationHook);
                case AnimationHookType.SetOmega: return new SetOmegaHook(animationHook);
                case AnimationHookType.Sound: return new SoundHook(animationHook);
                case AnimationHookType.SoundTable: return new SoundTableHook(animationHook);
                case AnimationHookType.SoundTweaked: return new SoundTweakedHook(animationHook);
                case AnimationHookType.StopParticle: return new StopParticleHook(animationHook);
                case AnimationHookType.TextureVelocity: return new TextureVelocityHook(animationHook);
                case AnimationHookType.TextureVelocityPart: return new TextureVelocityPartHook(animationHook);
                case AnimationHookType.Transparent: return new TransparentHook(animationHook);
                case AnimationHookType.TransparentPart: return new TransparentPartHook(animationHook);
            }
            return new AnimationHook(animationHook);
        }

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
        public virtual List<MetadataInfo> GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"Dir: {Direction}"),
            };
            return nodes;
        }

        //: Entity.AnimationHook
        public override string ToString() => $"HookType: {HookType}, Dir: {Direction}";
    }
}
