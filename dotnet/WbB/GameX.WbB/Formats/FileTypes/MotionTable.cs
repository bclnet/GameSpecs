using GameX.WbB.Formats.Entity;
using GameX.WbB.Formats.Entity.AnimationHooks;
using GameX.WbB.Formats.Props;
using GameX.Meta;
using GameX.Formats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GameX.WbB.Formats.FileTypes
{
    [PakFileType(PakFileType.MotionTable)]
    public class MotionTable : FileType, IHaveMetaInfo
    {
        public static Dictionary<ushort, MotionCommand> RawToInterpreted = Enum.GetValues(typeof(MotionCommand)).Cast<object>().ToDictionary(x => (ushort)(uint)x, x => (MotionCommand)x);
        public readonly uint DefaultStyle;
        public readonly IDictionary<uint, uint> StyleDefaults;
        public readonly IDictionary<uint, MotionData> Cycles;
        public readonly IDictionary<uint, MotionData> Modifiers;
        public readonly IDictionary<uint, IDictionary<uint, MotionData>> Links;

        public MotionTable(BinaryReader r)
        {
            Id = r.ReadUInt32();
            DefaultStyle = r.ReadUInt32();
            StyleDefaults = r.ReadL32TMany<uint, uint>(sizeof(uint), x => x.ReadUInt32());
            Cycles = r.ReadL32TMany<uint, MotionData>(sizeof(uint), x => new MotionData(x));
            Modifiers = r.ReadL32TMany<uint, MotionData>(sizeof(uint), x => new MotionData(x));
            Links = r.ReadL32TMany<uint, IDictionary<uint, MotionData>>(sizeof(uint), x => x.ReadL32TMany<uint, MotionData>(sizeof(uint), y => new MotionData(y)));
        }

        //: FileTypes.MotionTable
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            static string GetLabel(uint combined)
            {
                var stanceKey = (ushort)(combined >> 16);
                var motionKey = (ushort)combined;
                if (RawToInterpreted.TryGetValue(stanceKey, out var stance) && RawToInterpreted.TryGetValue(motionKey, out var motion)) return $"{stance} - {motion}";
                else if (Enum.IsDefined(typeof(MotionCommand), combined)) return $"{(MotionCommand)combined}";
                else return $"{combined:X8}";
            }

            var nodes = new List<MetaInfo> {
                new MetaInfo($"{nameof(MotionTable)}: {Id:X8}", items: new List<MetaInfo> {
                    new MetaInfo($"Default style: {(MotionCommand)DefaultStyle}"),
                    new MetaInfo("Style defaults", items: StyleDefaults.OrderBy(i => i.Key).Select(x => new MetaInfo($"{(MotionCommand)x.Key}: {(MotionCommand)x.Value}"))),
                    new MetaInfo("Cycles", items: Cycles.OrderBy(i => i.Key).Select(x => new MetaInfo(GetLabel(x.Key), items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                    new MetaInfo("Modifiers", items: Modifiers.OrderBy(i => i.Key).Select(x => new MetaInfo(GetLabel(x.Key), items: (x.Value as IHaveMetaInfo).GetInfoNodes()))),
                    new MetaInfo("Links", items: Links.OrderBy(i => i.Key).Select(x => new MetaInfo(GetLabel(x.Key), items: x.Value.OrderBy(i => i.Key).Select(y => new MetaInfo(GetLabel(y.Key), items: (y.Value as IHaveMetaInfo).GetInfoNodes()))))),
                })
            };
            return nodes;
        }


        /// <summary>
        /// Gets the default style for the requested MotionStance
        /// </summary>
        /// <returns>The default style or MotionCommand.Invalid if not found</returns>
        MotionCommand GetDefaultMotion(MotionStance style) => StyleDefaults.TryGetValue((uint)style, out var z) ? (MotionCommand)z : MotionCommand.Invalid;
        public float GetAnimationLength(MotionCommand motion) => GetAnimationLength((MotionStance)DefaultStyle, motion, GetDefaultMotion((MotionStance)DefaultStyle));
        public float GetAnimationLength(MotionStance stance, MotionCommand motion, MotionCommand? currentMotion = null) => GetAnimationLength(stance, motion, currentMotion ?? GetDefaultMotion(stance));

        public float GetCycleLength(MotionStance stance, MotionCommand motion)
        {
            var key = (uint)stance << 16 | (uint)motion & 0xFFFFF;
            if (!Cycles.TryGetValue(key, out var motionData) || motionData == null) return 0.0f;

            var length = 0.0f;
            foreach (var anim in motionData.Anims) length += GetAnimationLength(anim);
            return length;
        }

        static readonly ConcurrentDictionary<AttackFrameParams, List<(float time, AttackHook attackHook)>> attackFrameCache = new ConcurrentDictionary<AttackFrameParams, List<(float time, AttackHook attackHook)>>();

        public List<(float time, AttackHook attackHook)> GetAttackFrames(uint motionTableId, MotionStance stance, MotionCommand motion)
        {
            // could also do uint, and then a packed ulong, but would be more complicated maybe?
            var attackFrameParams = new AttackFrameParams(motionTableId, stance, motion);
            if (attackFrameCache.TryGetValue(attackFrameParams, out var attackFrames))
                return attackFrames;

            var motionTable = DatabaseManager.Portal.GetFile<MotionTable>(motionTableId);

            var animData = GetAnimData(stance, motion, GetDefaultMotion(stance));

            var frameNums = new List<int>();
            var attackHooks = new List<AttackHook>();
            var totalFrames = 0;
            foreach (var anim in animData)
            {
                var animation = DatabaseManager.Portal.GetFile<Animation>(anim.AnimId);
                foreach (var frame in animation.PartFrames)
                {
                    foreach (var hook in frame.Hooks) if (hook is AttackHook attackHook) { frameNums.Add(totalFrames); attackHooks.Add(attackHook); }
                    totalFrames++;
                }
            }

            attackFrames = new List<(float time, AttackHook attackHook)>();
            for (var i = 0; i < frameNums.Count; i++) attackFrames.Add(((float)frameNums[i] / totalFrames, attackHooks[i]));

            attackFrameCache.TryAdd(attackFrameParams, attackFrames);

            return attackFrames;
        }

        public AnimData[] GetAnimData(MotionStance stance, MotionCommand motion, MotionCommand currentMotion)
        {
            var animData = new AnimData[0];
            var motionKey = (uint)stance << 16 | (uint)currentMotion & 0xFFFFF;
            if (!Links.TryGetValue(motionKey, out var link) || link == null) return animData;
            if (!link.TryGetValue((uint)motion, out var motionData) || motionData == null)
            {
                motionKey = (uint)stance << 16;
                if (!Links.TryGetValue(motionKey, out link) || link == null) return animData;
                if (!link.TryGetValue((uint)motion, out motionData) || motionData == null) return animData;
            }
            return motionData.Anims;
        }

        public float GetAnimationLength(MotionStance stance, MotionCommand motion, MotionCommand currentMotion)
        {
            var animData = GetAnimData(stance, motion, currentMotion);
            var length = 0.0f;
            foreach (var anim in animData) length += GetAnimationLength(anim);
            return length;
        }

        public float GetAnimationLength(AnimData anim)
        {
            var highFrame = anim.HighFrame;
            // get the maximum # of animation frames
            var animation = DatabaseManager.Portal.GetFile<Animation>(anim.AnimId);
            if (anim.HighFrame == -1) highFrame = (int)animation.NumFrames;
            if (highFrame > animation.NumFrames)
            {
                // magic windup for level 6 spells appears to be the only animation w/ bugged data
                //Console.WriteLine($"MotionTable.GetAnimationLength({anim}): highFrame({highFrame}) > animation.NumFrames({animation.NumFrames})");
                highFrame = (int)animation.NumFrames;
            }
            var numFrames = highFrame - anim.LowFrame;
            return numFrames / Math.Abs(anim.Framerate); // framerates can be negative, which tells the client to play in reverse
        }

        public XPosition GetAnimationFinalPositionFromStart(XPosition position, float objScale, MotionCommand motion)
        {
            var defaultStyle = (MotionStance)DefaultStyle;
            var defaultMotion = GetDefaultMotion(defaultStyle); // get the default motion for the default
            return GetAnimationFinalPositionFromStart(position, objScale, defaultMotion, defaultStyle, motion);
        }

        public XPosition GetAnimationFinalPositionFromStart(XPosition position, float objScale, MotionCommand currentMotionState, MotionStance style, MotionCommand motion)
        {
            var length = 0F; // init our length var...will return as 0 if not found
            var finalPosition = new XPosition();
            var motionHash = ((uint)currentMotionState & 0xFFFFFF) | ((uint)style << 16);

            if (Links.ContainsKey(motionHash))
            {
                var links = Links[motionHash];
                if (links.ContainsKey((uint)motion))
                {
                    // loop through all that animations to get our total count
                    for (var i = 0; i < links[(uint)motion].Anims.Length; i++)
                    {
                        var anim = links[(uint)motion].Anims[i];
                        uint numFrames;
                        // check if the animation is set to play the whole thing, in which case we need to get the numbers of frames in the raw animation
                        if ((anim.LowFrame == 0) && (anim.HighFrame == -1))
                        {
                            var animation = DatabaseManager.Portal.GetFile<Animation>(anim.AnimId);
                            numFrames = animation.NumFrames;
                            if (animation.PosFrames.Length > 0)
                            {
                                finalPosition = position;
                                var origin = new Vector3(position.PositionX, position.PositionY, position.PositionZ);
                                var orientation = new Quaternion(position.RotationX, position.RotationY, position.RotationZ, position.RotationW);
                                foreach (var posFrame in animation.PosFrames)
                                {
                                    origin += Vector3.Transform(posFrame.Origin, orientation) * objScale;

                                    orientation *= posFrame.Orientation;
                                    orientation = Quaternion.Normalize(orientation);
                                }

                                finalPosition.PositionX = origin.X;
                                finalPosition.PositionY = origin.Y;
                                finalPosition.PositionZ = origin.Z;

                                finalPosition.RotationW = orientation.W;
                                finalPosition.RotationX = orientation.X;
                                finalPosition.RotationY = orientation.Y;
                                finalPosition.RotationZ = orientation.Z;
                            }
                            else return position;
                        }
                        else numFrames = (uint)(anim.HighFrame - anim.LowFrame);

                        length += numFrames / Math.Abs(anim.Framerate); // Framerates can be negative, which tells the client to play in reverse
                    }
                }
            }

            return finalPosition;
        }

        public MotionStance[] GetStances()
        {
            var stances = new HashSet<MotionStance>();
            foreach (var cycle in Cycles.Keys)
            {
                var stance = (MotionStance)(0x80000000 | cycle >> 16);
                if (!stances.Contains(stance)) stances.Add(stance);
            }
            if (stances.Count > 0 && !stances.Contains(MotionStance.Invalid)) stances.Add(MotionStance.Invalid);
            return stances.ToArray();
        }

        public MotionCommand[] GetMotionCommands(MotionStance stance = MotionStance.Invalid)
        {
            var commands = new HashSet<MotionCommand>();
            foreach (var cycle in Cycles.Keys)
            {
                if ((cycle >> 16) != ((uint)stance & 0xFFFF)) continue;
                var rawCommand = (ushort)(cycle & 0xFFFF);
                var motionCommand = RawToInterpreted[rawCommand];
                if (!commands.Contains(motionCommand)) commands.Add(motionCommand);
            }
            foreach (var kvp in Links)
            {
                var stanceMotion = kvp.Key;
                var links = kvp.Value;
                if ((stanceMotion >> 16) != ((uint)stance & 0xFFFF)) continue;
                foreach (var link in links.Keys)
                {
                    var rawCommand = (ushort)(link & 0xFFFF);
                    var motionCommand = RawToInterpreted[rawCommand];
                    if (!commands.Contains(motionCommand)) commands.Add(motionCommand);
                }
            }
            return commands.ToArray();
        }
    }
}
