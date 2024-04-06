using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using static GameX.Origin.Formats.UO.Binary_StringTable;
using static GameX.Util;
using static OpenStack.Debug;

namespace GameX.Origin.Structs.UO
{
    #region Direction

    [Flags]
    public enum Direction : byte
    {
        North = 0x0,
        Right = 0x1,
        East = 0x2,
        Down = 0x3,
        South = 0x4,
        Left = 0x5,
        West = 0x6,
        Up = 0x7,
        FacingMask = 0x7,
        Running = 0x80,
        ValueMask = 0x87,
        Nothing = 0xED
    }

    public static class DirectionHelper
    {
        public static Direction DirectionFromPoints(Vector2<int> from, Vector2<int> to) => DirectionFromVectors(new Vector2(from.X, from.Y), new Vector2(to.X, to.Y));
        public static Direction DirectionFromVectors(Vector2 fromPosition, Vector2 toPosition)
        {
            var angle = Math.Atan2(toPosition.Y - fromPosition.Y, toPosition.X - fromPosition.X);
            if (angle < 0) angle = Math.PI + (Math.PI + angle);
            var piPerSegment = (Math.PI * 2f) / 8f;
            var segmentValue = (Math.PI * 2f) / 16f;
            var direction = int.MaxValue;
            for (var i = 0; i < 8; i++)
            {
                if (angle >= segmentValue && angle <= (segmentValue + piPerSegment)) { direction = i + 1; break; }
                segmentValue += piPerSegment;
            }
            if (direction == int.MaxValue) direction = 0;
            direction = direction >= 7 ? direction - 7 : direction + 1;
            return (Direction)direction;
        }
        public static Direction GetCardinal(Direction inDirection) => inDirection & (Direction)0x6; // contains bitmasks for 0x0, 0x2, 0x4, and 0x6
        public static Direction Reverse(Direction inDirection) => (Direction)((int)inDirection + 0x04) & Direction.FacingMask;
    }

    #endregion

    #region Serial

    public struct Serial : IComparable, IComparable<Serial>
    {
        public readonly static Serial ProtectedAction = int.MinValue;
        public readonly static Serial Null = 0;
        public readonly static Serial World = unchecked((int)0xFFFFFFFF);
        static int _nextDynamicSerial = -1;

        readonly int _serial;
        Serial(int serial) => _serial = serial;
        public int Value => _serial;
        public bool IsMobile => _serial > 0 && _serial < 0x40000000;
        public bool IsItem => _serial >= 0x40000000;
        public bool IsValid => _serial > 0;
        public bool IsDynamic => _serial < 0;
        public static int NewDynamicSerial => _nextDynamicSerial--;
        public override int GetHashCode() => _serial;
        public int CompareTo(Serial other) => _serial.CompareTo(other._serial);
        public int CompareTo(object other) => other is Serial serial ? CompareTo(serial) : other == null ? -1 : throw new ArgumentException();
        public override bool Equals(object o) => o == null || !(o is Serial) ? false : ((Serial)o)._serial == _serial;
        public static bool operator ==(Serial l, Serial r) => l._serial == r._serial;
        public static bool operator !=(Serial l, Serial r) => l._serial != r._serial;
        public static bool operator >(Serial l, Serial r) => l._serial > r._serial;
        public static bool operator <(Serial l, Serial r) => l._serial < r._serial;
        public static bool operator >=(Serial l, Serial r) => l._serial >= r._serial;
        public static bool operator <=(Serial l, Serial r) => l._serial <= r._serial;
        public override string ToString() => $"0x{_serial:X8}";
        public static implicit operator int(Serial a) => a._serial;
        public static implicit operator Serial(int a) => new Serial(a);
    }

    #endregion

    #region AssistantFeatures

    /// <summary>
    /// These are the features that are made available by Razor. Servers can explicitly disallow enabling these
    /// features. However, because this client doesn't support Razor, for the most part, you can ignore these.
    /// </summary>
    [Flags]
    public enum AssistantFeatures : ulong
    {
        None = 0,
        FilterWeather = 1 << 0,  // Weather Filter
        FilterLight = 1 << 1,  // Light Filter
        SmartTarget = 1 << 2,  // Smart Last Target
        RangedTarget = 1 << 3,  // Range Check Last Target
        AutoOpenDoors = 1 << 4,  // Automatically Open Doors
        DequipOnCast = 1 << 5,  // Unequip Weapon on spell cast
        AutoPotionEquip = 1 << 6,  // Un/re-equip weapon on potion use
        PoisonedChecks = 1 << 7,  // Block heal If poisoned/Macro If Poisoned condition/Heal or Cure self
        LoopedMacros = 1 << 8,  // Disallow looping or recursive macros
        UseOnceAgent = 1 << 9,  // The use once agent
        RestockAgent = 1 << 10, // The restock agent
        SellAgent = 1 << 11, // The sell agent
        BuyAgent = 1 << 12, // The buy agent
        PotionHotkeys = 1 << 13, // All potion hotkeys
        RandomTargets = 1 << 14, // All random target hotkeys (not target next, last target, target self)
        ClosestTargets = 1 << 15, // All closest target hotkeys
        OverheadHealth = 1 << 16, // Health and Mana/Stam messages shown over player's heads
        AutolootAgent = 1 << 17, // The autoloot agent
        BoneCutterAgent = 1 << 18, // The bone cutter agent
        AdvancedMacros = 1 << 19, // Advanced macro engine
        AutoRemount = 1 << 20, // Auto remount after dismount
        AutoBandage = 1 << 21, // Auto bandage friends, self, last and mount option
        EnemyTargetShare = 1 << 22, // Enemy target share on guild, party or alliance chat
        FilterSeason = 1 << 23, // Season Filter
        SpellTargetShare = 1 << 24, // Spell target share on guild, party or alliance chat

        All = ulong.MaxValue
    }

    #endregion

    #region Body

    public enum BodyType : byte
    {
        Empty,
        Monster,
        Sea,
        Animal,
        Human,
        Equipment
    }

    public struct Body
    {
        internal static BodyType[] Types = new BodyType[0];
        public int BodyID;

        public Body(int bodyID) => BodyID = bodyID;

        public BodyType Type
            => BodyID >= 0 && BodyID < Types.Length
            ? Types[BodyID]
            : BodyType.Empty;

        public bool IsHumanoid
            => BodyID >= 0
            && BodyID < Types.Length
            && Types[BodyID] == BodyType.Human
            && BodyID != 402
            && BodyID != 403
            && BodyID != 607
            && BodyID != 608
            && BodyID != 694
            && BodyID != 695
            && BodyID != 970;

        public bool IsGargoyle
            => BodyID == 666
            || BodyID == 667
            || BodyID == 694
            || BodyID == 695;

        public bool IsMale
            => BodyID == 183
            || BodyID == 185
            || BodyID == 400
            || BodyID == 402
            || BodyID == 605
            || BodyID == 607
            || BodyID == 666
            || BodyID == 694
            || BodyID == 750;

        public bool IsFemale
            => BodyID == 184
            || BodyID == 186
            || BodyID == 401
            || BodyID == 403
            || BodyID == 606
            || BodyID == 608
            || BodyID == 667
            || BodyID == 695
            || BodyID == 751;

        public bool IsGhost
            => BodyID == 402
            || BodyID == 403
            || BodyID == 607
            || BodyID == 608
            || BodyID == 694
            || BodyID == 695
            || BodyID == 970;

        public bool IsMonster
            => BodyID >= 0
            && BodyID < Types.Length
            && Types[BodyID] == BodyType.Monster;

        public bool IsAnimal
            => BodyID >= 0
            && BodyID < Types.Length
            && Types[BodyID] == BodyType.Animal;

        public bool IsEmpty
            => BodyID >= 0
            && BodyID < Types.Length
            && Types[BodyID] == BodyType.Empty;

        public bool IsSea
            => BodyID >= 0
            && BodyID < Types.Length
            && Types[BodyID] == BodyType.Sea;

        public bool IsEquipment
            => BodyID >= 0
            && BodyID < Types.Length
            && Types[BodyID] == BodyType.Equipment;

        public static implicit operator int(Body a) => a.BodyID;
        public static implicit operator Body(int a) => new Body(a);
        public override string ToString() => $"0x{BodyID:X}";
        public override int GetHashCode() => BodyID;
        public override bool Equals(object o) => o == null || !(o is Body) ? false : ((Body)o).BodyID == BodyID;
        public static bool operator ==(Body l, Body r) => l.BodyID == r.BodyID;
        public static bool operator !=(Body l, Body r) => l.BodyID != r.BodyID;
        public static bool operator >(Body l, Body r) => l.BodyID > r.BodyID;
        public static bool operator >=(Body l, Body r) => l.BodyID >= r.BodyID;
        public static bool operator <(Body l, Body r) => l.BodyID < r.BodyID;
        public static bool operator <=(Body l, Body r) => l.BodyID <= r.BodyID;
    }

    #endregion

    #region Books

    public class Books
    {
        //ushort[] _gumpBaseIDs =
        //{
        //    0x1F4, // Yellow Cornered Book
        //    0x1FE, // Regular Cornered Book
        //    0x898, // Funky Book?
        //    0x899, // Tan Book?
        //    0x89A, // Red Book?
        //    0x89B, // Blue Book?
        //    0x8AC, // SpellBook
        //    0x2B00, // Necromancy Book?
        //    0x2B01, // Ice Book?
        //    0x2B02, // Arms Book?
        //    0x2B06, // Bushido Book?
        //    0x2B07, // Another Crazy Kanji Thing
        //    0x2B2F // A Greenish Book
        //};
        static readonly ushort[] _bookItemIDs = {
            0xFEF, // Brown Book
            0xFF0, // Tan Book
            0xFF1, // Red Book
            0xFF2  // Blue Book
        };
        public static bool IsBookItem(ushort itemID) => _bookItemIDs.Contains(itemID);
    }

    #endregion

    #region Chairs

    /// <summary>
    /// Contains a list of all chair objects, which are hardcoded in the legacy client.
    /// </summary>
    public static class Chairs
    {
        static Dictionary<int, ChairData> _chairs = new[]
        {
            // 0x0459 - 0x045C - marble benches
            new ChairData(0x0459, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x045A, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x045B, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x045C, Direction.East, ChairType.ReversibleFacing),
            // 0x0A2A - 0x0A2B - two stools
            new ChairData(0x0A2A, Direction.South, ChairType.AnyFacing),
            new ChairData(0x0A2B, Direction.South, ChairType.AnyFacing),
            //0x0B2C - 0x0B33 - chairs
            new ChairData(0x0B2C, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x0B2D, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x0B2E, Direction.South, ChairType.SingleFacing),
            new ChairData(0x0B2F, Direction.East, ChairType.SingleFacing),
            new ChairData(0x0B30, Direction.West, ChairType.SingleFacing),
            new ChairData(0x0B31, Direction.North, ChairType.SingleFacing),
            new ChairData(0x0B32, Direction.South, ChairType.SingleFacing),
            new ChairData(0x0B33, Direction.East, ChairType.SingleFacing),
            //0x0B4E - 0x0B6A - chairs, benches, one stool
            new ChairData(0x0B4E, Direction.East, ChairType.SingleFacing),
            new ChairData(0x0B4E, Direction.South, ChairType.SingleFacing),
            new ChairData(0x0B50, Direction.North, ChairType.SingleFacing),
            new ChairData(0x0B51, Direction.West, ChairType.SingleFacing),
            new ChairData(0x0B52, Direction.East, ChairType.SingleFacing),
            new ChairData(0x0B52, Direction.South, ChairType.SingleFacing),
            new ChairData(0x0B54, Direction.North, ChairType.SingleFacing),
            new ChairData(0x0B55, Direction.West, ChairType.SingleFacing),
            new ChairData(0x0B56, Direction.East, ChairType.SingleFacing),
            new ChairData(0x0B57, Direction.South, ChairType.SingleFacing),
            new ChairData(0x0B58, Direction.West, ChairType.SingleFacing),
            new ChairData(0x0B59, Direction.North, ChairType.SingleFacing),
            new ChairData(0x0B5A, Direction.East, ChairType.SingleFacing),
            new ChairData(0x0B5B, Direction.South, ChairType.SingleFacing),
            new ChairData(0x0B5C, Direction.North, ChairType.SingleFacing),
            new ChairData(0x0B5D, Direction.West, ChairType.SingleFacing),
            new ChairData(0x0B5E, Direction.South, ChairType.AnyFacing),
            new ChairData(0x0B5F, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x0B60, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x0B61, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x0B62, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x0B63, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x0B64, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x0B65, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x0B66, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x0B67, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x0B68, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x0B69, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x0B6A, Direction.South, ChairType.ReversibleFacing),
            // 0x0B91 - 0x0B4 - benches with high backs
            new ChairData(0x0B91, Direction.South, ChairType.SingleFacing),
            new ChairData(0x0B92, Direction.South, ChairType.SingleFacing),
            new ChairData(0x0B93, Direction.East, ChairType.SingleFacing),
            new ChairData(0x0B94, Direction.East, ChairType.SingleFacing),
            // 0x1049 - 0x104A - benches
            new ChairData(0x1049, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x104A,Direction.South, ChairType.ReversibleFacing),
            // 0x11FC - bamboo stool
            new ChairData(0x11FC,Direction.South, ChairType.AnyFacing),
            // 0x1207 - 0x120C - stone benches
            new ChairData(0x1207, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x1208, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x1209, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x120A, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x120B, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x120C, Direction.South, ChairType.ReversibleFacing),
            //0x1218 - 0x121B - stone chairs
            new ChairData(0x1218, Direction.South, ChairType.SingleFacing),
            new ChairData(0x1219, Direction.East, ChairType.SingleFacing),
            new ChairData(0x121A, Direction.North, ChairType.SingleFacing),
            new ChairData(0x121B, Direction.West, ChairType.SingleFacing),
            // 0x1DC7 - 0x1DD2 - long sandstone / marbe benches
            new ChairData(0x1DC7, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x1DC8, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x1DC9, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x1DCA, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x1DCB, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x1DCC, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x1DCD, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x1DCE, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x1DCF, Direction.East, ChairType.ReversibleFacing),
            new ChairData(0x1DD0, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x1DD1, Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x1DD2, Direction.South, ChairType.ReversibleFacing),
            // 0x2DE3 - 0x2DE6 - elven chairs 1
            new ChairData(0x2DE3,Direction.East, ChairType.SingleFacing),
            new ChairData(0x2DE4,Direction.South, ChairType.SingleFacing),
            new ChairData(0x2DE5,Direction.West, ChairType.SingleFacing),
            new ChairData(0x2DE6,Direction.North, ChairType.SingleFacing),
            // 0x2DEB - 0x2DEE - elven chairs 2
            new ChairData(0x2DEB,Direction.North, ChairType.SingleFacing),
            new ChairData(0x2DEC,Direction.South, ChairType.SingleFacing),
            new ChairData(0x2DED,Direction.East, ChairType.SingleFacing),
            new ChairData(0x2DEE,Direction.West, ChairType.SingleFacing),
            // 0x3DFF - 0x3E00 - dark stone benches
            new ChairData(0x3DFF,Direction.South, ChairType.ReversibleFacing),
            new ChairData(0x3E00,Direction.East, ChairType.ReversibleFacing)
        }.ToDictionary(x => x.ItemID);

        //public static void AddChairData(int itemID, Direction direction, ChairType chairType)
        //{
        //    if (_chairs.ContainsKey(itemID))
        //        _chairs.Remove(itemID);
        //    _chairs.Add(itemID, new ChairData(itemID, direction, chairType));
        //}

        //public static bool CheckItemAsChair(int itemID, out ChairData value)
        //{
        //    if (_chairs.TryGetValue(itemID, out value))
        //        return true;
        //    else
        //    {
        //        value = ChairData.Null;
        //        return false;
        //    }
        //}

        public class ChairData
        {
            public readonly int ItemID;
            public readonly Direction Facing;
            public readonly ChairType ChairType;
            public readonly int SittingPixelOffset;

            /*private Texture2DInfo _texture;
            public Texture2DInfo Texture
            {
                get
                {
                    if (_texture == null)
                    {
                        var provider = ServiceRegistry.GetService<IResourceProvider>();
                        var baseTexture = provider.GetItemTexture(ItemID);
                        _texture = provider.GetItemTexture(ItemID);
                    }
                    return _texture;
                }
            }*/

            public static ChairData Null = new ChairData(0, Direction.ValueMask, ChairType.AnyFacing);

            /// <summary>
            /// Creates a new chair data object.
            /// </summary>
            /// <param name="itemID">ItemID of the chair.</param>
            /// <param name="facing">The valid facing of the chair. Must be North, West, South, or East.</param>
            /// <param name="chairType">Whether the chair is a single facing (chair) reversible facing (bench) or any facing (stool) object.</param>
            public ChairData(int itemID, Direction facing, ChairType chairType)
            {
                ItemID = itemID;
                Facing = facing;
                ChairType = chairType;
                // SKY:TODO
                //SittingPixelOffset = TileData.ItemData[itemID].Unknown4;
                if (SittingPixelOffset > 32) SittingPixelOffset -= 32;
            }

            public Direction GetSittingFacing(Direction inFacing)
            {
                if (ChairType == ChairType.SingleFacing) return Facing;
                inFacing = DirectionHelper.GetCardinal(inFacing);
                if (inFacing == Facing) return Facing;
                else if (ChairType == ChairType.ReversibleFacing)
                {
                    if (DirectionHelper.Reverse(inFacing) == Facing) return inFacing;
                }
                else if (ChairType == ChairType.AnyFacing) return inFacing; // which has been made cardinal already, so this works.
                return Facing;
            }
        }

        public enum ChairType
        {
            /// <summary>
            /// The chair has only one valid facing. The mobile defaults to being drawn in the single default facing.
            /// </summary>
            SingleFacing = 0,
            /// <summary>
            /// The chair has two valid facings which are mirrored. 
            /// </summary>
            ReversibleFacing = 1,
            /// <summary>
            /// Mobiles can face any direction so long as it is NWS or E. The mobile defaults to being drawn in the default facing until it attempts to switch to another valid facing.
            /// </summary>
            AnyFacing = 2
        }
    }

    #endregion

    #region ChatMode

    public enum ChatMode
    {
        Default,
        Whisper,
        Emote,
        Party,
        PartyPrivate,
        Guild,
        Alliance
    }

    #endregion

    #region ClientVersion

    public static class ClientVersion
    {
        // NOTE FROM ZaneDubya: DO NOT change DefaultVersion from 6.0.6.2.
        // We are focusing our efforts on getting a specific version of the client working.
        // Once we have this version working, we will attempt to support additional versions.
        // We will not support any issues you experience after changing this value.
        public static readonly byte[] DefaultVersion = { 6, 0, 6, 2 };
        static readonly byte[] _extendedAddItemToContainer = { 6, 0, 1, 7 };
        static readonly byte[] _extendedFeaturesVersion = { 6, 0, 14, 2 };
        static readonly byte[] _convertedToUOPVersion = { 7, 0, 24, 0 };
        static byte[] _clientExeVersion;

        static byte[] FindClientVersion(string path)
        {
            //var p = FileManager.GetPath("client.exe");
            var s = File.Exists(path) ? FileVersionInfo.GetVersionInfo(path) : null;
            return s != null
                ? new byte[] { (byte)s.FileMajorPart, (byte)s.FileMinorPart, (byte)s.FileBuildPart, (byte)s.FilePrivatePart }
                : new byte[] { 0, 0, 0, 0 };
        }
        public static byte[] ClientExe => _clientExeVersion ??= FindClientVersion("path.exe");
        public static bool InstallationIsUopFormat => GreaterThanOrEqualTo(ClientExe, _convertedToUOPVersion);
        public static bool HasExtendedFeatures(byte[] version) => GreaterThanOrEqualTo(version, _extendedFeaturesVersion);
        public static bool HasExtendedAddItemPacket(byte[] version) => GreaterThanOrEqualTo(version, _extendedAddItemToContainer);
        public static bool EqualTo(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            var index = 0;
            while (index < a.Length)
            {
                if (a[index] != b[index]) return false;
                index++;
            }
            return true;
        }
        /// <summary> Compare two arrays of equal size. Returns true if first parameter array is greater than or equal to second. </summary>
        static bool GreaterThanOrEqualTo(byte[] a, byte[] b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;
            var index = 0;
            while (index < a.Length)
            {
                if (a[index] > b[index]) return true;
                if (a[index] < b[index]) return false;
                index++;
            }
            return true;
        }
    }

    #endregion

    #region ContextMenu

    public class ContextMenuData
    {
        readonly List<ContextMenuItem> _entries = new List<ContextMenuItem>();
        public readonly Serial Serial;
        public int Count => _entries.Count;

        public ContextMenuData(Serial serial) => Serial = serial;
        public ContextMenuItem this[int index] => index < 0 || index >= _entries.Count ? null : _entries[index];
        // Add a new context menu entry.
        internal void AddItem(int responseCode, int stringID, int flags, int hue)
            => _entries.Add(new ContextMenuItem(responseCode, stringID, flags, hue));
    }

    public class ContextMenuItem
    {
        public readonly int ResponseCode;
        public readonly string Caption;
        public ContextMenuItem(int responseCode, int stringID, int flags, int hue)
        {
            Caption = GetString(stringID);
            ResponseCode = responseCode;
        }
        public override string ToString() => $"{Caption} [{ResponseCode}]";
    }

    #endregion

    #region FeatureFlags

    [Flags]
    public enum FeatureFlags : uint
    {
        TheSecondAge = 0x1,
        Renaissance = 0x2,
        ThirdDawn = 0x4,
        LordBlackthornsRevenge = 0x8,
        AgeOfShadows = 0x10,
        CharacterSlot6 = 0x20,
        SameraiEmpire = 0x40,
        MondainsLegacy = 0x80,
        Splash8 = 0x100,
        Splash9 = 0x200,            // Ninth Age splash screen, crystal/shadow housing tiles
        TenthAge = 0x400,
        MoreStorage = 0x800,
        CharacterSlot7 = 0x1000,
        TenthAgeFaces = 0x2000,
        TrialAccount = 0x4000,
        EleventhAge = 0x8000,
        StygianAbyss = 0x10000,
        HighSeas = 0x20000,
        GothicHousing = 0x40000,
        RusticHousing = 0x80000
    }

    #endregion

    #region Genders

    public enum Genders
    {
        Male = 0,
        Female = 1
    }

    #endregion

    #region HairStyles

    public class HairStyles
    {
        static readonly int[] MaleStylesIDs = { 3000340, 3000341, 3000342, 3000343, 3000344, 3000345, 3000346, 3000347, 3000348, 3000349 };
        static string[] _maleHairNames;
        public static string[] MaleHairNames => _maleHairNames ??= MaleStylesIDs.Select(s => { var t = GetString(s); return t != "Pigtails" ? "2 Tails" : t; }).ToArray();
        //
        public readonly static int[] MaleIDs = { 0, 8251, 8252, 8253, 8260, 8261, 8266, 8263, 8264, 8265 };
        static readonly int[] _maleIDsForCreation = { 0, 1875, 1876, 1879, 1877, 1871, 1874, 1873, 1880, 1870 };
        public static int MaleGumpIDForCharacterCreationFromItemID(int id) { var i = Array.IndexOf(MaleIDs, id); return i >= 0 ? _maleIDsForCreation[i] : 0; }
        //
        static readonly int[] FacialStylesIDs = { 3000340, 3000351, 3000352, 3000353, 3000354, 1011060, 1011061, 3000357 };
        static string[] _facialHair;
        public static string[] FacialHair => _facialHair ??= FacialStylesIDs.Select(GetString).ToArray();
        //
        public static int[] FacialHairIDs = { 0, 8256, 8254, 8255, 8257, 8267, 8268, 8269 };
        static readonly int[] _facialGumpIDsForCreation = { 0, 1881, 1883, 1885, 1884, 1886, 1882, 1887 };
        public static int FacialHairGumpIDForCharacterCreationFromItemID(int id) { var i = Array.IndexOf(FacialHairIDs, id); return i >= 0 ? _facialGumpIDsForCreation[i] : 0; }
        //
        static readonly int[] FemaleStylesIDs = { 3000340, 3000341, 3000342, 3000343, 3000344, 3000345, 3000346, 3000347, 3000349, 3000350 };
        static string[] _femaleHairNames;
        public static string[] FemaleHairNames => _femaleHairNames ??= FemaleStylesIDs.Select(GetString).ToArray();
        //
        public static int[] FemaleIDs = { 0, 8251, 8252, 8253, 8260, 8261, 8266, 8263, 8265, 8262 };
        static readonly int[] _femaleIDsForCreation = { 0, 1847, 1842, 1845, 1843, 1844, 1840, 1839, 1836, 1841 };
        public static int FemaleGumpIDForCharacterCreationFromItemID(int id) { var i = Array.IndexOf(FemaleIDs, id); return i >= 0 ? _femaleIDsForCreation[i] : 0; }
    }

    #endregion

    #region HouseRevisionState

    public class HouseRevisionState
    {
        public Serial Serial;
        public int Hash;
        public HouseRevisionState(Serial serial, int revisionHash)
        {
            Serial = serial;
            Hash = revisionHash;
        }
    }

    #endregion

    #region Hues

    public static class Hues
    {
        public static int[] SkinTones => Enumerable.Range(0, 7 * 8).Select(i => i < 37 ? i + 1002 : i + 1003).ToArray();
        public static int[] HairTones => Enumerable.Range(0, 8 * 6).Select(i => i + 1102).ToArray();
        public static int[] TextTones => Enumerable.Range(0, 1024).Select(i => i + 2).ToArray();
    }

    #endregion

    #region ItemInContainer

    public class ItemInContainer
    {
        public readonly Serial Serial;
        public readonly int ItemID;
        public readonly int Amount;
        public readonly int X;
        public readonly int Y;
        public readonly int GridLocation;
        public readonly Serial ContainerSerial;
        public readonly int Hue;

        public ItemInContainer(Serial serial, int itemId, int amount, int x, int y, int gridLocation, int containerSerial, int hue)
        {
            Serial = serial;
            ItemID = itemId;
            Amount = amount;
            X = x;
            Y = y;
            GridLocation = gridLocation;
            ContainerSerial = containerSerial;
            Hue = hue;
        }
    }

    #endregion

    #region MessageTypes

    [Flags]
    public enum MessageTypes
    {
        Normal = 0x00,
        System = 0x01,
        Emote = 0x02,
        SpeechUnknown = 0x03,
        Information = 0x04,     // Overhead information messages
        Label = 0x06,
        Focus = 0x07,
        Whisper = 0x08,
        Yell = 0x09,
        Spell = 0x0A,
        Guild = 0x0D,
        Alliance = 0x0E,
        Command = 0x0F,
        /// <summary>
        /// This is used for display only. This is not in the UO protocol. Do not send msgs of this type to the server.
        /// </summary>
        PartyDisplayOnly = 0x10,
        EncodedTriggers = 0xC0 // 0x40 + 0x80
    }

    #endregion

    #region ParticleData

    public class ParticleData
    {
        static readonly ParticleData[] Data = new[] {
            new ParticleData("Explosion", 0x36B0, 0),           // 14000 explosion 1
            new ParticleData("Explosion", 0x36BD, 0),           // 14013 explosion 2
            new ParticleData("Explosion", 0x36CB, 0),           // 14027 explosion 3
            new ParticleData("Large Fireball", 0x36D4, 0),      // 14036 large fireball
            new ParticleData("Small Fireball", 0x36E4, 0),      // 14052 small fireball
            new ParticleData("Fire Snake", 0x36F4, 0),          // 14068 fire snake
            new ParticleData("Explosion Ball", 0x36FE, 0),      // 14078explosion ball
            new ParticleData("Fire Column", 0x3709, 0),         // 14089 fire column
                                                                // 14106 - display only the ending of fire column - is this actually used?
            new ParticleData("Smoke", 0x3728, 0),               // 14120 smoke
            new ParticleData("Fizzle", 0x3735, 0),              // 14133 fizzle
            new ParticleData("Sparkle Blue", 0x373A, 0),        // 14138 sparkle blue
            new ParticleData("Sparkle Red", 0x374A, 0),         // 14154 sparkle red
            new ParticleData("Sparkle Yellow", 0x375A, 0),      // 14170 sparkle yellow blue
            new ParticleData("Sparkle Surround", 0x376A, 0),    // 14186 sparkle surround
            new ParticleData("Sparkle Planar", 0x3779, 0),      // 14201 sparkle planar
            new ParticleData("Death Vortex", 0x3789, 0),        // 14217 death vortex (whirlpool on ground?)
            new ParticleData("Magic Arrow", 0x379E, 0),         // glowing arrow
            new ParticleData("Small Bolt", 0x379F, 0),          // small bolt
            new ParticleData("Field of Blades (Summon)", 0x37A0, 0),    // field of blades (summon?)
            new ParticleData("Glow", 0x37B9, 0),                // glow
            new ParticleData("Death Vortex", 0x37CC, 0),        // death vortex
            new ParticleData("Field of Blades (Folding)", 0x37EB, 0),   // field of blades (folding up)
            new ParticleData("Field of Blades (Unfolding)", 0x37F7, 0), // field of blades (unfolding)
            new ParticleData("Energy", 0x3818, 0),              // energy
            new ParticleData("Poison Wall (SW)", 0x3914, 0),    // field of poison (facing SW)
            new ParticleData("Poison Wall (SE)", 0x3920, 0),    // field of poison (facing SE)
            new ParticleData("Energy Wall (SW)", 0x3946, 0),    // field of energy (facing SW)
            new ParticleData("Energy Wall (SE)", 0x3956, 0),    // field of energy (facing SE)
            new ParticleData("Paralysis Wall (SW)", 0x3967, 0), // field of paralysis (facing SW, open and close?)
            new ParticleData("Paralysis Wall (SE)", 0x3979, 0), // field of paralysis (Facing SE, open and close?)
            new ParticleData("Fire Wall (SW)", 0x398C, 0),      // field of fire (facing SW)
            new ParticleData("Fire Wall (SE)", 0x3996, 0),      // field of fire (facing SE)
            new ParticleData("<null>", 0x39A0, 0)               // Used to determine the frame length of the preceding effect.
        };

        static ParticleData()
        {
            for (var i = 0; i < Data.Length - 1; i++) Data[i].FrameLength = Data[i + 1].ItemID - Data[i].ItemID;
            DefaultEffect ??= Data[0];
        }

        public static ParticleData DefaultEffect;

        public static ParticleData RandomExplosion => (object)_randomValue(0, 2) switch
        {
            0 => Get(0x36B0),
            1 => Get(0x36BD),
            2 => Get(0x36CB),
            _ => Get(0x36B0),
        };

        public static ParticleData Get(int itemID)
        {
            if (itemID < Data[0].ItemID || itemID >= Data[^1].ItemID) return null;
            ParticleData data;
            for (var i = 1; i < Data.Length; i++)
                if (itemID < Data[i].ItemID)
                {
                    data = Data[i - 1];
                    if (itemID != data.ItemID) Log($"ERROR: Mismatch? Requested particle: {itemID}, returning particle: {data.ItemID}.");
                    return Data[i - 1];
                }
            Log($"ERROR: Unknown particle effect with ItemID: {itemID}");
            return null;
        }

        public int ItemID;
        public int FrameLength;
        public int SpeedOffset;
        public string Name;

        public ParticleData(string name, int itemID, int speed)
        {
            Name = name;
            ItemID = itemID;
            SpeedOffset = speed;
        }
    }

    #endregion

    #region Races

    public enum Races
    {
        Human = 1,
        Elf = 2
    }

    #endregion

    #region Reagents

    public enum Reagents
    {
        // britannia reagents
        BlackPearl,
        Bloodmoss,
        Garlic,
        Ginseng,
        MandrakeRoot,
        Nightshade,
        SulfurousAsh,
        SpidersSilk,
        // pagan reagents
        BatWing,
        GraveDust,
        DaemonBlood,
        NoxCrystal,
        PigIron
    }

    #endregion

    #region Seasons

    public enum Seasons
    {
        Spring = 0,
        Summer = 1,
        Fall = 2,
        Winter = 3,
        Desolation = 4
    }

    #endregion

    #region Spellbook

    public class SpellbookData
    {
        public readonly Serial Serial;
        public readonly ushort ItemID;
        public readonly SpellbookTypes BookType;
        public readonly ulong SpellsBitfield;

        public SpellbookData(Serial serial, ushort itemID, ushort bookTypePacketID, ulong spellBitFields)
        {
            Serial = serial;
            ItemID = itemID;
            SpellsBitfield = spellBitFields;
            switch (bookTypePacketID)
            {
                case 1: BookType = SpellbookTypes.Magic; break;
                case 101: BookType = SpellbookTypes.Necromancer; break;
                case 201: BookType = SpellbookTypes.Chivalry; break;
                case 401: BookType = SpellbookTypes.Bushido; break;
                case 501: BookType = SpellbookTypes.Ninjitsu; break;
                case 601: BookType = SpellbookTypes.Spellweaving; break;
                default: BookType = SpellbookTypes.Unknown; return;
            }
        }

        public static SpellbookTypes GetSpellBookTypeFromItemID(int itemID)
            => itemID switch
            {
                var x when x == 0x0E3B || x == 0x0EFA => SpellbookTypes.Magic,
                0x2252 => SpellbookTypes.Chivalry,  // paladin spellbook
                0x2253 => SpellbookTypes.Necromancer,  // necromancer book
                0x238C => SpellbookTypes.Bushido,  // book of bushido
                0x23A0 => SpellbookTypes.Ninjitsu,  // book of ninjitsu
                0x2D50 => SpellbookTypes.Chivalry,  // spell weaving book
                _ => SpellbookTypes.Unknown,
            };

        public static int GetOffsetFromSpellBookType(SpellbookTypes spellbooktype)
            => spellbooktype switch
            {
                SpellbookTypes.Magic => 1,
                SpellbookTypes.Necromancer => 101,
                SpellbookTypes.Chivalry => 201,
                SpellbookTypes.Bushido => 401,
                SpellbookTypes.Ninjitsu => 501,
                SpellbookTypes.Spellweaving => 601,
                _ => 1,
            };

        //public SpellbookData(ContainerItem spellbook, ContainerContentPacket contents)
        //{
        //    Serial = spellbook.Serial;
        //    ItemID = (ushort)spellbook.ItemID;
        //    BookType = GetSpellBookTypeFromItemID(spellbook.ItemID);
        //    if (BookType == SpellBookTypes.Unknown) return;
        //    var offset = GetOffsetFromSpellBookType(BookType);
        //    foreach (var i in contents.Items)
        //    {
        //        var index = ((i.Amount - offset) & 0x0000003F);
        //        var circle = (index / 8);
        //        index = index % 8;
        //        index = ((3 - circle % 4) + (circle / 4) * 4) * 8 + (index);
        //        var flag = ((ulong)1) << index;
        //        SpellsBitfield |= flag;
        //    }
        //}
    }

    public enum SpellbookTypes
    {
        Magic,
        Necromancer,
        Chivalry,
        Bushido,
        Ninjitsu,
        Spellweaving,
        Unknown
    }

    #endregion

    #region SpellDefinition

    public struct SpellDefinition
    {
        public static SpellDefinition EmptySpell = new SpellDefinition();

        public readonly string Name;
        public readonly int ID;
        public readonly int GumpIconID;
        public readonly int GumpIconSmallID;
        public readonly Reagents[] Regs;

        public SpellDefinition(string name, int index, int gumpIconID, params Reagents[] regs)
        {
            Name = name;
            ID = index;
            GumpIconID = gumpIconID;
            GumpIconSmallID = gumpIconID - 0x1298;
            Regs = regs;
        }

        public string CreateReagentListString(string separator)
        {
            var b = new StringBuilder();
            for (var i = 0; i < Regs.Length; i++)
            {
                switch (Regs[i])
                {
                    // britanian reagents
                    case Reagents.BlackPearl: b.Append("Black Pearl"); break;
                    case Reagents.Bloodmoss: b.Append("Bloodmoss"); break;
                    case Reagents.Garlic: b.Append("Garlic"); break;
                    case Reagents.Ginseng: b.Append("Ginseng"); break;
                    case Reagents.MandrakeRoot: b.Append("Mandrake Root"); break;
                    case Reagents.Nightshade: b.Append("Nightshade"); break;
                    case Reagents.SulfurousAsh: b.Append("Sulfurous Ash"); break;
                    case Reagents.SpidersSilk: b.Append("Spiders' Silk"); break;
                    // pagan reagents
                    case Reagents.BatWing: b.Append("Bat Wing"); break;
                    case Reagents.GraveDust: b.Append("Grave Dust"); break;
                    case Reagents.DaemonBlood: b.Append("Daemon Blood"); break;
                    case Reagents.NoxCrystal: b.Append("Nox Crystal"); break;
                    case Reagents.PigIron: b.Append("Pig Iron"); break;
                    default: b.Append("Unknown reagent"); break;
                }
                if (i < Regs.Length - 1) b.Append(separator);
            }
            return b.ToString();
        }
    }

    #endregion

    #region SpellsMagery

    public static class SpellsMagery
    {
        static Dictionary<int, SpellDefinition> _spells;
        static ReadOnlyCollection<SpellDefinition> _readOnlySpells;

        public static ReadOnlyCollection<SpellDefinition> Spells
        {
            get
            {
                if (_readOnlySpells == null)
                {
                    var spells = new List<SpellDefinition>();
                    for (var i = 1; i <= 64; i++)
                        spells.Add(_spells[i]);
                    _readOnlySpells = new ReadOnlyCollection<SpellDefinition>(spells);
                }
                return _readOnlySpells;
            }
        }

        public static SpellDefinition GetSpell(int spellIndex)
        {
            SpellDefinition spell;
            if (_spells.TryGetValue(spellIndex, out spell))
                return spell;
            return SpellDefinition.EmptySpell;
        }

        static SpellsMagery()
        {
            _spells = new Dictionary<int, SpellDefinition>()
            {
                // first circle
                { 1, new SpellDefinition("Clumsy", 1, 0x1B58, Reagents.Bloodmoss, Reagents.Nightshade) },
                { 2, new SpellDefinition("Create Food", 2, 0x1B59, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot) },
                { 3, new SpellDefinition("Feeblemind", 3, 0x1B5A, Reagents.Nightshade, Reagents.Ginseng) },
                { 4, new SpellDefinition("Heal", 4, 0x1B5B, Reagents.Garlic, Reagents.Ginseng, Reagents.SpidersSilk) },
                { 5, new SpellDefinition("Magic Arrow", 5, 0x1B5C, Reagents.SulfurousAsh) },
                { 6, new SpellDefinition("Night Sight", 6, 0x1B5D, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 7, new SpellDefinition("Reactive Armor", 7, 0x1B5E, Reagents.Garlic, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 8, new SpellDefinition("Weaken", 8, 0x1B5F, Reagents.Garlic, Reagents.Nightshade) },
                // second circle
                { 9, new SpellDefinition("Agility", 9, 0x1B60, Reagents.Bloodmoss, Reagents.MandrakeRoot) },
                { 10, new SpellDefinition("Cunning", 10, 0x1B61, Reagents.Nightshade, Reagents.MandrakeRoot) },
                { 11, new SpellDefinition("Cure", 11, 0x1B62, Reagents.Garlic, Reagents.Ginseng) },
                { 12, new SpellDefinition("Harm", 12, 0x1B63, Reagents.Nightshade, Reagents.SpidersSilk) },
                { 13, new SpellDefinition("Magic Trap", 13, 0x1B64, Reagents.Garlic, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 14, new SpellDefinition("Magic Untrap", 14, 0x1B65, Reagents.Bloodmoss, Reagents.SulfurousAsh) },
                { 15, new SpellDefinition("Protection", 15, 0x1B66, Reagents.Garlic, Reagents.Ginseng, Reagents.SulfurousAsh) },
                { 16, new SpellDefinition("Strength", 16, 0x1B67, Reagents.MandrakeRoot, Reagents.Nightshade) },
                // third circle
                { 17, new SpellDefinition("Bless", 17, 0x1B68, Reagents.Garlic, Reagents.MandrakeRoot) },
                { 18, new SpellDefinition("Fireball", 18, 0x1B69, Reagents.BlackPearl) },
                { 19, new SpellDefinition("Magic Lock", 19, 0x1B6a, Reagents.Bloodmoss, Reagents.Garlic, Reagents.SulfurousAsh) },
                { 20, new SpellDefinition("Poison", 20, 0x1B6b, Reagents.Nightshade) },
                { 21, new SpellDefinition("Telekinesis", 21, 0x1B6c, Reagents.Bloodmoss, Reagents.MandrakeRoot) },
                { 22, new SpellDefinition("Teleport", 22, 0x1B6d, Reagents.Bloodmoss, Reagents.MandrakeRoot) },
                { 23, new SpellDefinition("Unlock", 23, 0x1B6e, Reagents.Bloodmoss, Reagents.SulfurousAsh) },
                { 24, new SpellDefinition("Wall of Stone", 24, 0x1B6f, Reagents.Bloodmoss, Reagents.Garlic) },
                // fourth circle
                { 25, new SpellDefinition("Arch Cure", 25, 0x1B70, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot) },
                { 26, new SpellDefinition("Arch Protection", 26, 0x1B71, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot, Reagents.SulfurousAsh) },
                { 27, new SpellDefinition("Curse", 27, 0x1B72, Reagents.Garlic, Reagents.Nightshade, Reagents.SulfurousAsh) },
                { 28, new SpellDefinition("Fire Field", 28, 0x1B73, Reagents.BlackPearl, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 29, new SpellDefinition("Greater Heal", 29, 0x1B74, Reagents.Garlic, Reagents.Ginseng, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                { 30, new SpellDefinition("Lightning", 30, 0x1B75, Reagents.MandrakeRoot, Reagents.SulfurousAsh) },
                { 31, new SpellDefinition("Mana Drain", 31, 0x1B76, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                { 32, new SpellDefinition("Recall", 32, 0x1B77, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot) },
                // fifth circle
                { 33, new SpellDefinition("Blade Spirits", 33, 0x1B78, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.Nightshade) },
                { 34, new SpellDefinition("Dispel Field", 34, 0x1B79, Reagents.BlackPearl, Reagents.Garlic, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 35, new SpellDefinition("Incognito", 35, 0x1B7a, Reagents.Bloodmoss, Reagents.Garlic, Reagents.Nightshade) },
                { 36, new SpellDefinition("Magic Reflection", 36, 0x1B7b, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                { 37, new SpellDefinition("Mind Blast", 37, 0x1B7c, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.Nightshade, Reagents.SulfurousAsh) },
                { 38, new SpellDefinition("Paralyze", 38, 0x1B7d, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                { 39, new SpellDefinition("Poison Field", 39, 0x1B7e, Reagents.BlackPearl, Reagents.Nightshade, Reagents.SpidersSilk) },
                { 40, new SpellDefinition("Summon Creature", 40, 0x1B7f, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                // sixth circle
                { 41, new SpellDefinition("Dispel", 41, 0x1B80, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SulfurousAsh) },
                { 42, new SpellDefinition("Energy Bolt", 42, 0x1B81, Reagents.BlackPearl, Reagents.Nightshade) },
                { 43, new SpellDefinition("Explosion", 43, 0x1B82, Reagents.Bloodmoss, Reagents.MandrakeRoot) },
                { 44, new SpellDefinition("Invisibility", 44, 0x1B83, Reagents.Bloodmoss, Reagents.Nightshade) },
                { 45, new SpellDefinition("Mark", 45, 0x1B84, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot) },
                { 46, new SpellDefinition("Mass Curse", 46, 0x1B85, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.Nightshade, Reagents.SulfurousAsh) },
                { 47, new SpellDefinition("Paralyze Field", 47, 0x1B86, Reagents.BlackPearl, Reagents.Ginseng, Reagents.SpidersSilk) },
                { 48, new SpellDefinition("Reveal", 48, 0x1B87, Reagents.Bloodmoss, Reagents.SulfurousAsh) },
                // seventh circle
                { 49, new SpellDefinition("Chain Lightning", 49, 0x1B88, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SulfurousAsh) },
                { 50, new SpellDefinition("Energy Field", 50, 0x1B89, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 51, new SpellDefinition("Flamestrike", 51, 0x1B8a, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 52, new SpellDefinition("Gate Travel", 52, 0x1B8b, Reagents.BlackPearl, Reagents.MandrakeRoot, Reagents.SulfurousAsh) },
                { 53, new SpellDefinition("Mana Vampire", 53, 0x1B8c, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                { 54, new SpellDefinition("Mass Dispel", 54, 0x1B8d, Reagents.BlackPearl, Reagents.Garlic, Reagents.MandrakeRoot, Reagents.SulfurousAsh) },
                { 55, new SpellDefinition("Meteor Swarm", 55, 0x1B8e, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 56, new SpellDefinition("Polymorph", 56, 0x1B8f, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                // eighth circle
                { 57, new SpellDefinition("Earthquake", 57, 0x1B90, Reagents.Bloodmoss, Reagents.Ginseng, Reagents.MandrakeRoot, Reagents.SulfurousAsh) },
                { 58, new SpellDefinition("Energy Vortex", 58, 0x1B91, Reagents.BlackPearl, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.Nightshade) },
                { 59, new SpellDefinition("Resurrection", 59, 0x1B92, Reagents.Bloodmoss, Reagents.Ginseng, Reagents.Garlic) },
                { 60, new SpellDefinition("Air Elemental", 60, 0x1B93, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                { 61, new SpellDefinition("Summon Daemon", 61, 0x1B94, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 62, new SpellDefinition("Earth Elemental", 62, 0x1B95, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk) },
                { 63, new SpellDefinition("Fire Elemental", 63, 0x1B96, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk, Reagents.SulfurousAsh) },
                { 64, new SpellDefinition("Water Elemental", 64, 0x1B97, Reagents.Bloodmoss, Reagents.MandrakeRoot, Reagents.SpidersSilk) }
            };
        }

        public static string[] CircleNames = {
            "First Circle", "Second Circle", "Third Circle", "Fourth Circle",
            "Fifth Circle", "Sixth Circle", "Seventh Circle", "Eighth Circle" };
    }

    #endregion

    #region StatLocks

    public class StatLocks
    {
        public int Strength;
        public int Dexterity;
        public int Intelligence;

        public StatLocks(int stren, int dexte, int intel)
        {
            Strength = stren;
            Dexterity = dexte;
            Intelligence = intel;
        }
    }

    #endregion

    #region TileFlag

    [Flags]
    public enum TileFlag
    {
        None = 0x00000000,
        Background = 0x00000001,
        Weapon = 0x00000002,
        Transparent = 0x00000004,
        Translucent = 0x00000008,
        Wall = 0x00000010,
        Damaging = 0x00000020,
        Impassable = 0x00000040,
        Wet = 0x00000080,
        Unknown1 = 0x00000100,
        Surface = 0x00000200,
        Bridge = 0x00000400,
        Generic = 0x00000800,
        Window = 0x00001000,
        NoShoot = 0x00002000,
        ArticleA = 0x00004000,
        ArticleAn = 0x00008000,
        Internal = 0x00010000,
        Foliage = 0x00020000,
        PartialHue = 0x00040000,
        Unknown2 = 0x00080000,
        Map = 0x00100000,
        Container = 0x00200000,
        Wearable = 0x00400000,
        LightSource = 0x00800000,
        Animation = 0x01000000,
        NoDiagonal = 0x02000000,
        Unknown3 = 0x04000000,
        Armor = 0x08000000,
        Roof = 0x10000000,
        Door = 0x20000000,
        StairBack = 0x40000000,
        StairRight = unchecked((int)0x80000000)
    }

    #endregion
}
