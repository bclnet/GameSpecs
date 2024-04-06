using GameX.WbB.Formats.Props;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace GameX.WbB.Formats.Models
{
    public static partial class PropertiesExtensions { }

    public class PropertiesAllegiance
    {
        public bool Banned { get; set; }
        public bool ApprovedVassal { get; set; }
    }

    partial class PropertiesExtensions
    {
        public static Dictionary<uint, PropertiesAllegiance> GetApprovedVassals(this IDictionary<uint, PropertiesAllegiance> value, ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterReadLock();
            if (value == null) return new Dictionary<uint, PropertiesAllegiance>();
            try { return value.Where(i => i.Value.ApprovedVassal).ToDictionary(i => i.Key, i => i.Value); }
            finally { rwLock.ExitReadLock(); }
        }

        public static Dictionary<uint, PropertiesAllegiance> GetBanList(this IDictionary<uint, PropertiesAllegiance> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return new Dictionary<uint, PropertiesAllegiance>();
            rwLock.EnterReadLock();
            try { return value.Where(i => i.Value.Banned).ToDictionary(i => i.Key, i => i.Value); }
            finally { rwLock.ExitReadLock(); }
        }

        public static PropertiesAllegiance GetFirstOrDefaultByCharacterId(this IDictionary<uint, PropertiesAllegiance> value, uint characterId, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { value.TryGetValue(characterId, out var entity); return entity; }
            finally { rwLock.ExitReadLock(); }
        }

        public static void AddOrUpdateAllegiance(this IDictionary<uint, PropertiesAllegiance> value, uint characterId, bool isBanned, bool approvedVassal, ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterWriteLock();
            try
            {
                if (!value.TryGetValue(characterId, out var entity)) { entity = new PropertiesAllegiance { Banned = isBanned, ApprovedVassal = approvedVassal }; value.Add(characterId, entity); }
                entity.Banned = isBanned;
                entity.ApprovedVassal = approvedVassal;
            }
            finally { rwLock.ExitWriteLock(); }
        }

        public static bool TryRemoveAllegiance(this IDictionary<uint, PropertiesAllegiance> value, uint characterId, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return false;
            rwLock.EnterWriteLock();
            try { return value.Remove(characterId); }
            finally { rwLock.ExitWriteLock(); }
        }
    }

    public class PropertiesAnimPart
    {
        public byte Index { get; set; }
        public uint AnimationId { get; set; }

        public PropertiesAnimPart Clone() => new PropertiesAnimPart
        {
            Index = Index,
            AnimationId = AnimationId
        };
    }

    partial class PropertiesExtensions
    {
        public static int GetCount(this IList<PropertiesAnimPart> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return 0;
            rwLock.EnterReadLock();
            try { return value.Count; }
            finally { rwLock.ExitReadLock(); }
        }

        public static List<PropertiesAnimPart> Clone(this IList<PropertiesAnimPart> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { return new List<PropertiesAnimPart>(value); }
            finally { rwLock.ExitReadLock(); }
        }

        public static void CopyTo(this IList<PropertiesAnimPart> value, ICollection<PropertiesAnimPart> destination, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return;
            rwLock.EnterReadLock();
            try { foreach (var entry in value) destination.Add(entry); }
            finally { rwLock.ExitReadLock(); }
        }
    }

    public class PropertiesAttribute
    {
        public uint InitLevel { get; set; }
        public uint LevelFromCP { get; set; }
        public uint CPSpent { get; set; }

        public PropertiesAttribute Clone() => new PropertiesAttribute
        {
            InitLevel = InitLevel,
            LevelFromCP = LevelFromCP,
            CPSpent = CPSpent
        };
    }

    public class PropertiesAttribute2nd
    {
        public uint InitLevel { get; set; }
        public uint LevelFromCP { get; set; }
        public uint CPSpent { get; set; }
        public uint CurrentLevel { get; set; }

        public PropertiesAttribute2nd Clone() => new PropertiesAttribute2nd
        {
            InitLevel = InitLevel,
            LevelFromCP = LevelFromCP,
            CPSpent = CPSpent,
            CurrentLevel = CurrentLevel,
        };
    }

    public class PropertiesBodyPart
    {
        public DamageType DType { get; set; }
        public int DVal { get; set; }
        public float DVar { get; set; }
        public int BaseArmor { get; set; }
        public int ArmorVsSlash { get; set; }
        public int ArmorVsPierce { get; set; }
        public int ArmorVsBludgeon { get; set; }
        public int ArmorVsCold { get; set; }
        public int ArmorVsFire { get; set; }
        public int ArmorVsAcid { get; set; }
        public int ArmorVsElectric { get; set; }
        public int ArmorVsNether { get; set; }
        public int BH { get; set; }
        public float HLF { get; set; }
        public float MLF { get; set; }
        public float LLF { get; set; }
        public float HRF { get; set; }
        public float MRF { get; set; }
        public float LRF { get; set; }
        public float HLB { get; set; }
        public float MLB { get; set; }
        public float LLB { get; set; }
        public float HRB { get; set; }
        public float MRB { get; set; }
        public float LRB { get; set; }

        public PropertiesBodyPart Clone() => new PropertiesBodyPart
        {
            DType = DType,
            DVal = DVal,
            DVar = DVar,
            BaseArmor = BaseArmor,
            ArmorVsSlash = ArmorVsSlash,
            ArmorVsPierce = ArmorVsPierce,
            ArmorVsBludgeon = ArmorVsBludgeon,
            ArmorVsCold = ArmorVsCold,
            ArmorVsFire = ArmorVsFire,
            ArmorVsAcid = ArmorVsAcid,
            ArmorVsElectric = ArmorVsElectric,
            ArmorVsNether = ArmorVsNether,
            BH = BH,
            HLF = HLF,
            MLF = MLF,
            LLF = LLF,
            HRF = HRF,
            MRF = MRF,
            LRF = LRF,
            HLB = HLB,
            MLB = MLB,
            LLB = LLB,
            HRB = HRB,
            MRB = MRB,
            LRB = LRB,
        };
    }

    public class PropertiesBook
    {
        public int MaxNumPages { get; set; }
        public int MaxNumCharsPerPage { get; set; }

        public PropertiesBook Clone() => new PropertiesBook
        {
            MaxNumPages = MaxNumPages,
            MaxNumCharsPerPage = MaxNumCharsPerPage,
        };
    }

    public class PropertiesBookPageData
    {
        public uint AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string AuthorAccount { get; set; }
        public bool IgnoreAuthor { get; set; }
        public string PageText { get; set; }

        public PropertiesBookPageData Clone() => new PropertiesBookPageData
        {
            AuthorId = AuthorId,
            AuthorName = AuthorName,
            AuthorAccount = AuthorAccount,
            IgnoreAuthor = IgnoreAuthor,
            PageText = PageText,
        };
    }

    partial class PropertiesExtensions
    {
        public static int GetPageCount(this IList<PropertiesBookPageData> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return 0;
            rwLock.EnterReadLock();
            try { return value.Count; }
            finally { rwLock.ExitReadLock(); }
        }

        public static List<PropertiesBookPageData> Clone(this IList<PropertiesBookPageData> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { return new List<PropertiesBookPageData>(value); }
            finally { rwLock.ExitReadLock(); }
        }

        public static PropertiesBookPageData GetPage(this IList<PropertiesBookPageData> value, int index, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { return value.Count <= index ? null : value[index]; }
            finally { rwLock.ExitReadLock(); }
        }

        public static void AddPage(this IList<PropertiesBookPageData> value, PropertiesBookPageData page, out int index, ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterWriteLock();
            try { value.Add(page); index = value.Count; }
            finally { rwLock.ExitWriteLock(); }
        }

        public static bool RemovePage(this IList<PropertiesBookPageData> value, int index, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return false;
            rwLock.EnterWriteLock();
            try { if (value.Count <= index) return false; value.RemoveAt(index); return true; }
            finally { rwLock.ExitWriteLock(); }
        }
    }

    public class PropertiesCreateList
    {
        /// <summary>
        /// This is only used to tie this property back to a specific database row
        /// </summary>
        public uint DatabaseRecordId { get; set; }

        public DestinationType DestinationType { get; set; }
        public uint WeenieClassId { get; set; }
        public int StackSize { get; set; }
        public sbyte Palette { get; set; }
        public float Shade { get; set; }
        public bool TryToBond { get; set; }

        public PropertiesCreateList Clone() => new PropertiesCreateList
        {
            DestinationType = DestinationType,
            WeenieClassId = WeenieClassId,
            StackSize = StackSize,
            Palette = Palette,
            Shade = Shade,
            TryToBond = TryToBond,
        };
    }

    public class PropertiesEmote
    {
        /// <summary>
        /// This is only used to tie this property back to a specific database row
        /// </summary>
        public uint DatabaseRecordId { get; set; }

        public EmoteCategory Category { get; set; }
        public float Probability { get; set; }
        public uint? WeenieClassId { get; set; }
        public MotionStance? Style { get; set; }
        public MotionCommand? Substyle { get; set; }
        public string Quest { get; set; }
        public VendorType? VendorType { get; set; }
        public float? MinHealth { get; set; }
        public float? MaxHealth { get; set; }

        public Weenie Object { get; set; }
        public IList<PropertiesEmoteAction> PropertiesEmoteAction { get; set; } = new List<PropertiesEmoteAction>();

        public PropertiesEmote Clone()
        {
            var result = new PropertiesEmote
            {
                Category = Category,
                Probability = Probability,
                WeenieClassId = WeenieClassId,
                Style = Style,
                Substyle = Substyle,
                Quest = Quest,
                VendorType = VendorType,
                MinHealth = MinHealth,
                MaxHealth = MaxHealth,
            };
            foreach (var action in PropertiesEmoteAction) result.PropertiesEmoteAction.Add(action.Clone());
            return result;
        }
    }

    public class PropertiesEmoteAction
    {
        /// <summary>
        /// This is only used to tie this property back to a specific database row
        /// </summary>
        public uint DatabaseRecordId { get; set; }

        public uint Type { get; set; }
        public float Delay { get; set; }
        public float Extent { get; set; }
        public MotionCommand? Motion { get; set; }
        public string Message { get; set; }
        public string TestString { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
        public long? Min64 { get; set; }
        public long? Max64 { get; set; }
        public double? MinDbl { get; set; }
        public double? MaxDbl { get; set; }
        public int? Stat { get; set; }
        public bool? Display { get; set; }
        public int? Amount { get; set; }
        public long? Amount64 { get; set; }
        public long? HeroXP64 { get; set; }
        public double? Percent { get; set; }
        public int? SpellId { get; set; }
        public int? WealthRating { get; set; }
        public int? TreasureClass { get; set; }
        public int? TreasureType { get; set; }
        public PlayScript? PScript { get; set; }
        public Sound? Sound { get; set; }
        public sbyte? DestinationType { get; set; }
        public uint? WeenieClassId { get; set; }
        public int? StackSize { get; set; }
        public int? Palette { get; set; }
        public float? Shade { get; set; }
        public bool? TryToBond { get; set; }
        public uint? ObjCellId { get; set; }
        public float? OriginX { get; set; }
        public float? OriginY { get; set; }
        public float? OriginZ { get; set; }
        public float? AnglesW { get; set; }
        public float? AnglesX { get; set; }
        public float? AnglesY { get; set; }
        public float? AnglesZ { get; set; }

        public PropertiesEmoteAction Clone() => new PropertiesEmoteAction
        {
            Type = Type,
            Delay = Delay,
            Extent = Extent,
            Motion = Motion,
            Message = Message,
            TestString = TestString,
            Min = Min,
            Max = Max,
            Min64 = Min64,
            Max64 = Max64,
            MinDbl = MinDbl,
            MaxDbl = MaxDbl,
            Stat = Stat,
            Display = Display,
            Amount = Amount,
            Amount64 = Amount64,
            HeroXP64 = HeroXP64,
            Percent = Percent,
            SpellId = SpellId,
            WealthRating = WealthRating,
            TreasureClass = TreasureClass,
            TreasureType = TreasureType,
            PScript = PScript,
            Sound = Sound,
            DestinationType = DestinationType,
            WeenieClassId = WeenieClassId,
            StackSize = StackSize,
            Palette = Palette,
            Shade = Shade,
            TryToBond = TryToBond,
            ObjCellId = ObjCellId,
            OriginX = OriginX,
            OriginY = OriginY,
            OriginZ = OriginZ,
            AnglesW = AnglesW,
            AnglesX = AnglesX,
            AnglesY = AnglesY,
            AnglesZ = AnglesZ,
        };
    }

    public class PropertiesEnchantmentRegistry
    {
        public uint EnchantmentCategory { get; set; }
        public int SpellId { get; set; }
        public ushort LayerId { get; set; }
        public bool HasSpellSetId { get; set; }
        public SpellCategory SpellCategory { get; set; }
        public uint PowerLevel { get; set; }
        public double StartTime { get; set; }
        public double Duration { get; set; }
        public uint CasterObjectId { get; set; }
        public float DegradeModifier { get; set; }
        public float DegradeLimit { get; set; }
        public double LastTimeDegraded { get; set; }
        public EnchantmentTypeFlags StatModType { get; set; }
        public uint StatModKey { get; set; }
        public float StatModValue { get; set; }
        public EquipmentSet SpellSetId { get; set; }
    }

    partial class PropertiesExtensions
    {
        public static List<PropertiesEnchantmentRegistry> Clone(this ICollection<PropertiesEnchantmentRegistry> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;

            rwLock.EnterReadLock();
            try { return value.ToList(); }
            finally { rwLock.ExitReadLock(); }
        }

        public static bool HasEnchantments(this ICollection<PropertiesEnchantmentRegistry> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return false;
            rwLock.EnterReadLock();
            try { return value.Any(); }
            finally { rwLock.ExitReadLock(); }
        }

        public static bool HasEnchantment(this ICollection<PropertiesEnchantmentRegistry> value, uint spellId, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return false;
            rwLock.EnterReadLock();
            try { return value.Any(e => e.SpellId == spellId); }
            finally { rwLock.ExitReadLock(); }
        }

        public static PropertiesEnchantmentRegistry GetEnchantmentBySpell(this ICollection<PropertiesEnchantmentRegistry> value, int spellId, uint? casterGuid, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try
            {
                var results = value.Where(e => e.SpellId == spellId);
                if (casterGuid != null) results = results.Where(e => e.CasterObjectId == casterGuid);
                return results.FirstOrDefault();
            }
            finally { rwLock.ExitReadLock(); }
        }

        public static PropertiesEnchantmentRegistry GetEnchantmentBySpellSet(this ICollection<PropertiesEnchantmentRegistry> value, int spellId, EquipmentSet spellSetId, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { return value.FirstOrDefault(e => e.SpellId == spellId && e.SpellSetId == spellSetId); }
            finally { rwLock.ExitReadLock(); }
        }

        public static List<PropertiesEnchantmentRegistry> GetEnchantmentsByCategory(this ICollection<PropertiesEnchantmentRegistry> value, SpellCategory spellCategory, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { return value.Where(e => e.SpellCategory == spellCategory).ToList(); }
            finally { rwLock.ExitReadLock(); }
        }

        public static List<PropertiesEnchantmentRegistry> GetEnchantmentsByStatModType(this ICollection<PropertiesEnchantmentRegistry> value, EnchantmentTypeFlags statModType, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { return value.Where(e => (e.StatModType & statModType) == statModType).ToList(); }
            finally { rwLock.ExitReadLock(); }
        }

        // this ensures level 8 item self spells always take precedence over level 8 item other spells
        private static HashSet<int> Level8AuraSelfSpells = new HashSet<int>
        {
            (int)SpellId.BloodDrinkerSelf8,
            (int)SpellId.DefenderSelf8,
            (int)SpellId.HeartSeekerSelf8,
            (int)SpellId.SpiritDrinkerSelf8,
            (int)SpellId.SwiftKillerSelf8,
            (int)SpellId.HermeticLinkSelf8,
        };

        public static List<PropertiesEnchantmentRegistry> GetEnchantmentsTopLayer(this ICollection<PropertiesEnchantmentRegistry> value, ReaderWriterLockSlim rwLock, HashSet<int> setSpells)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try
            {
                var results =
                    from e in value
                    group e by e.SpellCategory
                    into categories
                    //select categories.OrderByDescending(c => c.LayerId).First();
                    select categories.OrderByDescending(c => c.PowerLevel)
                        .ThenByDescending(c => Level8AuraSelfSpells.Contains(c.SpellId))
                        .ThenByDescending(c => setSpells.Contains(c.SpellId) ? c.SpellId : c.StartTime).First();
                return results.ToList();
            }
            finally { rwLock.ExitReadLock(); }
        }

        /// <summary>
        /// Returns the top layers in each spell category for a StatMod type
        /// </summary>
        public static List<PropertiesEnchantmentRegistry> GetEnchantmentsTopLayerByStatModType(this ICollection<PropertiesEnchantmentRegistry> value, EnchantmentTypeFlags statModType, ReaderWriterLockSlim rwLock, HashSet<int> setSpells)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try
            {
                var valuesByStatModType = value.Where(e => (e.StatModType & statModType) == statModType);
                var results =
                    from e in valuesByStatModType
                    group e by e.SpellCategory
                    into categories
                    //select categories.OrderByDescending(c => c.LayerId).First();
                    select categories.OrderByDescending(c => c.PowerLevel)
                        .ThenByDescending(c => Level8AuraSelfSpells.Contains(c.SpellId))
                        .ThenByDescending(c => setSpells.Contains(c.SpellId) ? c.SpellId : c.StartTime).First();
                return results.ToList();
            }
            finally { rwLock.ExitReadLock(); }
        }

        /// <summary>
        /// Returns the top layers in each spell category for a StatMod type + key
        /// </summary>
        public static List<PropertiesEnchantmentRegistry> GetEnchantmentsTopLayerByStatModType(this ICollection<PropertiesEnchantmentRegistry> value, EnchantmentTypeFlags statModType, uint statModKey, ReaderWriterLockSlim rwLock, HashSet<int> setSpells, bool handleMultiple = false)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try
            {
                var multipleStat = EnchantmentTypeFlags.Undef;
                if (handleMultiple)
                {
                    // todo: this is starting to get a bit messy here, EnchantmentTypeFlags handling should be more adaptable
                    // perhaps the enchantment registry in acclient should be investigated for reference logic
                    multipleStat = statModType | EnchantmentTypeFlags.MultipleStat;
                    statModType |= EnchantmentTypeFlags.SingleStat;
                }
                var valuesByStatModTypeAndKey = value.Where(e => (e.StatModType & statModType) == statModType && e.StatModKey == statModKey || (handleMultiple && (e.StatModType & multipleStat) == multipleStat && (e.StatModType & EnchantmentTypeFlags.Vitae) == 0 && e.StatModKey == 0));
                // 3rd spell id sort added for Gauntlet Damage Boost I / Gauntlet Damage Boost II, which is contained in multiple sets, and can overlap
                // without this sorting criteria, it's already matched up to the client, but produces logically incorrect results for server spell stacking
                // confirmed this bug still exists in acclient Enchantment.Duel(), unknown if it existed in retail server
                var results =
                    from e in valuesByStatModTypeAndKey
                    group e by e.SpellCategory
                    into categories
                    //select categories.OrderByDescending(c => c.LayerId).First();
                    select categories.OrderByDescending(c => c.PowerLevel)
                        .ThenByDescending(c => Level8AuraSelfSpells.Contains(c.SpellId))
                        .ThenByDescending(c => setSpells.Contains(c.SpellId) ? c.SpellId : c.StartTime).First();
                return results.ToList();
            }
            finally { rwLock.ExitReadLock(); }
        }

        public static List<PropertiesEnchantmentRegistry> HeartBeatEnchantmentsAndReturnExpired(this ICollection<PropertiesEnchantmentRegistry> value, double heartbeatInterval, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try
            {
                var expired = new List<PropertiesEnchantmentRegistry>();
                foreach (var enchantment in value)
                {
                    enchantment.StartTime -= heartbeatInterval;
                    // StartTime ticks backwards to -Duration
                    if (enchantment.Duration >= 0 && enchantment.StartTime <= -enchantment.Duration) expired.Add(enchantment);
                }
                return expired;
            }
            finally { rwLock.ExitReadLock(); }
        }

        public static void AddEnchantment(this ICollection<PropertiesEnchantmentRegistry> value, PropertiesEnchantmentRegistry entity, ReaderWriterLockSlim rwLock)
        {
            rwLock.EnterWriteLock();
            try { value.Add(entity); }
            finally { rwLock.ExitWriteLock(); }
        }

        public static bool TryRemoveEnchantment(this ICollection<PropertiesEnchantmentRegistry> value, int spellId, uint casterObjectId, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return false;
            rwLock.EnterWriteLock();
            try
            {
                var entity = value.FirstOrDefault(x => x.SpellId == spellId && x.CasterObjectId == casterObjectId);
                if (entity != null) { value.Remove(entity); return true; }
                return false;
            }
            finally { rwLock.ExitWriteLock(); }
        }

        public static void RemoveAllEnchantments(this ICollection<PropertiesEnchantmentRegistry> value, IEnumerable<int> spellsToExclude, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return;
            rwLock.EnterWriteLock();
            try
            {
                var enchantments = value.Where(e => !spellsToExclude.Contains(e.SpellId)).ToList();
                foreach (var enchantment in enchantments) value.Remove(enchantment);
            }
            finally { rwLock.ExitWriteLock(); }
        }
    }

    public class PropertiesGenerator
    {
        /// <summary>
        /// This is only used to tie this property back to a specific database row
        /// </summary>
        public uint DatabaseRecordId { get; set; }

        public float Probability { get; set; }
        public uint WeenieClassId { get; set; }
        public float? Delay { get; set; }
        public int InitCreate { get; set; }
        public int MaxCreate { get; set; }
        public RegenerationType WhenCreate { get; set; }
        public RegenLocationType WhereCreate { get; set; }
        public int? StackSize { get; set; }
        public uint? PaletteId { get; set; }
        public float? Shade { get; set; }
        public uint? ObjCellId { get; set; }
        public float? OriginX { get; set; }
        public float? OriginY { get; set; }
        public float? OriginZ { get; set; }
        public float? AnglesW { get; set; }
        public float? AnglesX { get; set; }
        public float? AnglesY { get; set; }
        public float? AnglesZ { get; set; }

        public PropertiesGenerator Clone() => new PropertiesGenerator
        {
            Probability = Probability,
            WeenieClassId = WeenieClassId,
            Delay = Delay,
            InitCreate = InitCreate,
            MaxCreate = MaxCreate,
            WhenCreate = WhenCreate,
            WhereCreate = WhereCreate,
            StackSize = StackSize,
            PaletteId = PaletteId,
            Shade = Shade,
            ObjCellId = ObjCellId,
            OriginX = OriginX,
            OriginY = OriginY,
            OriginZ = OriginZ,
            AnglesW = AnglesW,
            AnglesX = AnglesX,
            AnglesY = AnglesY,
            AnglesZ = AnglesZ,
        };
    }

    public class PropertiesPalette
    {
        public uint SubPaletteId { get; set; }
        public ushort Offset { get; set; }
        public ushort Length { get; set; }

        public PropertiesPalette Clone() => new PropertiesPalette
        {
            SubPaletteId = SubPaletteId,
            Offset = Offset,
            Length = Length,
        };
    }

    partial class PropertiesExtensions
    {
        public static int GetCount(this IList<PropertiesPalette> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return 0;
            rwLock.EnterReadLock();
            try { return value.Count; }
            finally { rwLock.ExitReadLock(); }
        }

        public static List<PropertiesPalette> Clone(this IList<PropertiesPalette> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { return new List<PropertiesPalette>(value); }
            finally { rwLock.ExitReadLock(); }
        }

        public static void CopyTo(this IList<PropertiesPalette> value, ICollection<PropertiesPalette> destination, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return;
            rwLock.EnterReadLock();
            try { foreach (var entry in value) destination.Add(entry); }
            finally { rwLock.ExitReadLock(); }
        }
    }

    public class PropertiesPosition
    {
        public uint ObjCellId { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float RotationW { get; set; }
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }

        public PropertiesPosition Clone() => new PropertiesPosition
        {
            ObjCellId = ObjCellId,
            PositionX = PositionX,
            PositionY = PositionY,
            PositionZ = PositionZ,
            RotationW = RotationW,
            RotationX = RotationX,
            RotationY = RotationY,
            RotationZ = RotationZ,
        };
    }

    public class PropertiesSkill
    {
        public ushort LevelFromPP { get; set; }
        public SkillAdvancementClass SAC { get; set; }
        public uint PP { get; set; }
        public uint InitLevel { get; set; }
        public uint ResistanceAtLastCheck { get; set; }
        public double LastUsedTime { get; set; }

        public PropertiesSkill Clone() => new PropertiesSkill
        {
            LevelFromPP = LevelFromPP,
            SAC = SAC,
            PP = PP,
            InitLevel = InitLevel,
            ResistanceAtLastCheck = ResistanceAtLastCheck,
            LastUsedTime = LastUsedTime,
        };
    }

    public class PropertiesTextureMap
    {
        public byte PartIndex { get; set; }
        public uint OldTexture { get; set; }
        public uint NewTexture { get; set; }

        public PropertiesTextureMap Clone() => new PropertiesTextureMap
        {
            PartIndex = PartIndex,
            OldTexture = OldTexture,
            NewTexture = NewTexture,
        };
    }

    partial class PropertiesExtensions
    {
        public static int GetCount(this IList<PropertiesTextureMap> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return 0;
            rwLock.EnterReadLock();
            try { return value.Count; }
            finally { rwLock.ExitReadLock(); }
        }

        public static List<PropertiesTextureMap> Clone(this IList<PropertiesTextureMap> value, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return null;
            rwLock.EnterReadLock();
            try { return new List<PropertiesTextureMap>(value); }
            finally { rwLock.ExitReadLock(); }
        }

        public static void CopyTo(this IList<PropertiesTextureMap> value, ICollection<PropertiesTextureMap> destination, ReaderWriterLockSlim rwLock)
        {
            if (value == null) return;
            rwLock.EnterReadLock();
            try { foreach (var entry in value) destination.Add(entry); }
            finally { rwLock.ExitReadLock(); }
        }
    }
}
