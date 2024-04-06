using GameX.WbB.Formats.Entity;
using GameX.WbB.Formats.Props;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace GameX.WbB.Formats.Models
{
    /// <summary>
    /// Only populated collections and dictionaries are initialized.
    /// We do this to conserve memory in ACE.Server
    /// Be sure to check for null first.
    /// </summary>
    public interface IWeenie
    {
        uint WeenieClassId { get; set; }
        WeenieType WeenieType { get; set; }

        IDictionary<PropertyBool, bool> PropertiesBool { get; set; }
        IDictionary<PropertyDataId, uint> PropertiesDID { get; set; }
        IDictionary<PropertyFloat, double> PropertiesFloat { get; set; }
        IDictionary<PropertyInstanceId, uint> PropertiesIID { get; set; }
        IDictionary<PropertyInt, int> PropertiesInt { get; set; }
        IDictionary<PropertyInt64, long> PropertiesInt64 { get; set; }
        IDictionary<PropertyString, string> PropertiesString { get; set; }

        IDictionary<PositionType, PropertiesPosition> PropertiesPosition { get; set; }

        IDictionary<int, float /* probability */> PropertiesSpellBook { get; set; }

        IList<PropertiesAnimPart> PropertiesAnimPart { get; set; }
        IList<PropertiesPalette> PropertiesPalette { get; set; }
        IList<PropertiesTextureMap> PropertiesTextureMap { get; set; }

        // Properties for all world objects that typically aren't modified over the original weenie
        ICollection<PropertiesCreateList> PropertiesCreateList { get; set; }
        ICollection<PropertiesEmote> PropertiesEmote { get; set; }
        HashSet<int> PropertiesEventFilter { get; set; }
        IList<PropertiesGenerator> PropertiesGenerator { get; set; } // Using a list per this: https://github.com/ACEmulator/ACE/pull/2616, however, no order is guaranteed for db records

        // Properties for creatures
        IDictionary<PropertyAttribute, PropertiesAttribute> PropertiesAttribute { get; set; }
        IDictionary<PropertyAttribute2nd, PropertiesAttribute2nd> PropertiesAttribute2nd { get; set; }
        IDictionary<CombatBodyPart, PropertiesBodyPart> PropertiesBodyPart { get; set; }
        IDictionary<Skill, PropertiesSkill> PropertiesSkill { get; set; }

        // Properties for books
        PropertiesBook PropertiesBook { get; set; }
        IList<PropertiesBookPageData> PropertiesBookPageData { get; set; }
    }

    /// <summary>
    /// Only populated collections and dictionaries are initialized.
    /// We do this to conserve memory in ACE.Server
    /// Be sure to check for null first.
    /// </summary>
    public class Weenie : IWeenie
    {
        public uint WeenieClassId { get; set; }
        public string ClassName { get; set; }
        public WeenieType WeenieType { get; set; }

        public IDictionary<PropertyBool, bool> PropertiesBool { get; set; }
        public IDictionary<PropertyDataId, uint> PropertiesDID { get; set; }
        public IDictionary<PropertyFloat, double> PropertiesFloat { get; set; }
        public IDictionary<PropertyInstanceId, uint> PropertiesIID { get; set; }
        public IDictionary<PropertyInt, int> PropertiesInt { get; set; }
        public IDictionary<PropertyInt64, long> PropertiesInt64 { get; set; }
        public IDictionary<PropertyString, string> PropertiesString { get; set; }

        public IDictionary<PositionType, PropertiesPosition> PropertiesPosition { get; set; }

        public IDictionary<int, float /* probability */> PropertiesSpellBook { get; set; }

        public IList<PropertiesAnimPart> PropertiesAnimPart { get; set; }
        public IList<PropertiesPalette> PropertiesPalette { get; set; }
        public IList<PropertiesTextureMap> PropertiesTextureMap { get; set; }

        // Properties for all world objects that typically aren't modified over the original weenie
        public ICollection<PropertiesCreateList> PropertiesCreateList { get; set; }
        public ICollection<PropertiesEmote> PropertiesEmote { get; set; }
        public HashSet<int> PropertiesEventFilter { get; set; }
        public IList<PropertiesGenerator> PropertiesGenerator { get; set; }

        // Properties for creatures
        public IDictionary<PropertyAttribute, PropertiesAttribute> PropertiesAttribute { get; set; }
        public IDictionary<PropertyAttribute2nd, PropertiesAttribute2nd> PropertiesAttribute2nd { get; set; }
        public IDictionary<CombatBodyPart, PropertiesBodyPart> PropertiesBodyPart { get; set; }
        public IDictionary<Skill, PropertiesSkill> PropertiesSkill { get; set; }

        // Properties for books
        public PropertiesBook PropertiesBook { get; set; }
        public IList<PropertiesBookPageData> PropertiesBookPageData { get; set; }
    }

    public static partial class WeenieExtensions
    {
        // =====================================
        // Get
        // Bool, DID, Float, IID, Int, Int64, String, Position
        // =====================================

        public static bool? GetProperty(this Weenie weenie, PropertyBool property)
            => weenie.PropertiesBool == null || !weenie.PropertiesBool.TryGetValue(property, out var value) ? null : (bool?)value;

        public static uint? GetProperty(this Weenie weenie, PropertyDataId property)
            => weenie.PropertiesDID == null || !weenie.PropertiesDID.TryGetValue(property, out var value) ? null : (uint?)value;

        public static double? GetProperty(this Weenie weenie, PropertyFloat property)
            => weenie.PropertiesFloat == null || !weenie.PropertiesFloat.TryGetValue(property, out var value) ? null : (double?)value;

        public static uint? GetProperty(this Weenie weenie, PropertyInstanceId property)
            => weenie.PropertiesIID == null || !weenie.PropertiesIID.TryGetValue(property, out var value) ? null : (uint?)value;

        public static int? GetProperty(this Weenie weenie, PropertyInt property)
            => weenie.PropertiesInt == null || !weenie.PropertiesInt.TryGetValue(property, out var value) ? null : (int?)value;

        public static long? GetProperty(this Weenie weenie, PropertyInt64 property)
            => weenie.PropertiesInt64 == null || !weenie.PropertiesInt64.TryGetValue(property, out var value) ? null : (long?)value;

        public static string GetProperty(this Weenie weenie, PropertyString property)
            => weenie.PropertiesString == null || !weenie.PropertiesString.TryGetValue(property, out var value) ? null : value;

        public static PropertiesPosition GetProperty(this Weenie weenie, PositionType property)
            => weenie.PropertiesPosition == null || !weenie.PropertiesPosition.TryGetValue(property, out var value) ? null : value;

        public static XPosition GetPosition(this Weenie weenie, PositionType property)
            => weenie.PropertiesPosition == null || !weenie.PropertiesPosition.TryGetValue(property, out var value) ? null : new XPosition(value.ObjCellId, value.PositionX, value.PositionY, value.PositionZ, value.RotationX, value.RotationY, value.RotationZ, value.RotationW);


        // =====================================
        // Utility
        // =====================================

        public static string GetName(this Weenie weenie)
            => weenie.GetProperty(PropertyString.Name);

        public static string GetPluralName(this Weenie weenie)
            => weenie.GetProperty(PropertyString.PluralName) ?? Grammar.Pluralize(weenie.GetProperty(PropertyString.Name));

        public static ItemType GetItemType(this Weenie weenie)
            => (ItemType)(weenie.GetProperty(PropertyInt.ItemType) ?? 0);

        public static int? GetValue(this Weenie weenie)
            => weenie.GetProperty(PropertyInt.Value);

        public static bool IsStackable(this Weenie weenie)
        {
            switch (weenie.WeenieType)
            {
                case WeenieType.Stackable:
                case WeenieType.Ammunition:
                case WeenieType.Coin:
                case WeenieType.CraftTool:
                case WeenieType.Food:
                case WeenieType.Gem:
                case WeenieType.Missile:
                case WeenieType.SpellComponent: return true;
            }
            return false;
        }

        public static bool IsStuck(this Weenie weenie)
            => weenie.GetProperty(PropertyBool.Stuck) ?? false;

        public static bool RequiresBackpackSlotOrIsContainer(this Weenie weenie)
            => (weenie.GetProperty(PropertyBool.RequiresBackpackSlot) ?? false) || weenie.WeenieType == WeenieType.Container;

        public static bool IsVendorService(this Weenie weenie)
            => weenie.GetProperty(PropertyBool.VendorService) ?? false;

        public static int GetStackUnitEncumbrance(this Weenie weenie)
        {
            if (weenie.IsStackable())
            {
                var stackUnitEncumbrance = weenie.GetProperty(PropertyInt.StackUnitEncumbrance);
                if (stackUnitEncumbrance != null) return stackUnitEncumbrance.Value;
            }
            return weenie.GetProperty(PropertyInt.EncumbranceVal) ?? 0;
        }

        public static int GetMaxStackSize(this Weenie weenie)
        {
            if (weenie.IsStackable())
            {
                var maxStackSize = weenie.GetProperty(PropertyInt.MaxStackSize);
                if (maxStackSize != null) return maxStackSize.Value;
            }
            return 1;
        }
    }

    partial class WeenieExtensions
    {
        public static Biota ConvertToBiota(this Weenie weenie, uint id, bool instantiateEmptyCollections = false, bool referenceWeenieCollectionsForCommonProperties = false)
        {
            var result = new Biota
            {
                Id = id,
                WeenieClassId = weenie.WeenieClassId,
                WeenieType = weenie.WeenieType
            };
            if (weenie.PropertiesBool != null && (instantiateEmptyCollections || weenie.PropertiesBool.Count > 0)) result.PropertiesBool = new Dictionary<PropertyBool, bool>(weenie.PropertiesBool);
            if (weenie.PropertiesDID != null && (instantiateEmptyCollections || weenie.PropertiesDID.Count > 0)) result.PropertiesDID = new Dictionary<PropertyDataId, uint>(weenie.PropertiesDID);
            if (weenie.PropertiesFloat != null && (instantiateEmptyCollections || weenie.PropertiesFloat.Count > 0)) result.PropertiesFloat = new Dictionary<PropertyFloat, double>(weenie.PropertiesFloat);
            if (weenie.PropertiesIID != null && (instantiateEmptyCollections || weenie.PropertiesIID.Count > 0)) result.PropertiesIID = new Dictionary<PropertyInstanceId, uint>(weenie.PropertiesIID);
            if (weenie.PropertiesInt != null && (instantiateEmptyCollections || weenie.PropertiesInt.Count > 0)) result.PropertiesInt = new Dictionary<PropertyInt, int>(weenie.PropertiesInt);
            if (weenie.PropertiesInt64 != null && (instantiateEmptyCollections || weenie.PropertiesInt64.Count > 0)) result.PropertiesInt64 = new Dictionary<PropertyInt64, long>(weenie.PropertiesInt64);
            if (weenie.PropertiesString != null && (instantiateEmptyCollections || weenie.PropertiesString.Count > 0)) result.PropertiesString = new Dictionary<PropertyString, string>(weenie.PropertiesString);
            if (weenie.PropertiesPosition != null && (instantiateEmptyCollections || weenie.PropertiesPosition.Count > 0))
            {
                result.PropertiesPosition = new Dictionary<PositionType, PropertiesPosition>(weenie.PropertiesPosition.Count);
                foreach (var kvp in weenie.PropertiesPosition) result.PropertiesPosition.Add(kvp.Key, kvp.Value.Clone());
            }
            if (weenie.PropertiesSpellBook != null && (instantiateEmptyCollections || weenie.PropertiesSpellBook.Count > 0)) result.PropertiesSpellBook = new Dictionary<int, float>(weenie.PropertiesSpellBook);
            if (weenie.PropertiesAnimPart != null && (instantiateEmptyCollections || weenie.PropertiesAnimPart.Count > 0))
            {
                result.PropertiesAnimPart = new List<PropertiesAnimPart>(weenie.PropertiesAnimPart.Count);
                foreach (var record in weenie.PropertiesAnimPart) result.PropertiesAnimPart.Add(record.Clone());
            }
            if (weenie.PropertiesPalette != null && (instantiateEmptyCollections || weenie.PropertiesPalette.Count > 0))
            {
                result.PropertiesPalette = new Collection<PropertiesPalette>();
                foreach (var record in weenie.PropertiesPalette) result.PropertiesPalette.Add(record.Clone());
            }
            if (weenie.PropertiesTextureMap != null && (instantiateEmptyCollections || weenie.PropertiesTextureMap.Count > 0))
            {
                result.PropertiesTextureMap = new List<PropertiesTextureMap>(weenie.PropertiesTextureMap.Count);
                foreach (var record in weenie.PropertiesTextureMap) result.PropertiesTextureMap.Add(record.Clone());
            }

            // Properties for all world objects that typically aren't modified over the original weenie

            if (referenceWeenieCollectionsForCommonProperties)
            {
                result.PropertiesCreateList = weenie.PropertiesCreateList;
                result.PropertiesEmote = weenie.PropertiesEmote;
                result.PropertiesEventFilter = weenie.PropertiesEventFilter;
                result.PropertiesGenerator = weenie.PropertiesGenerator;
            }
            else
            {
                if (weenie.PropertiesCreateList != null && (instantiateEmptyCollections || weenie.PropertiesCreateList.Count > 0))
                {
                    result.PropertiesCreateList = new Collection<PropertiesCreateList>();
                    foreach (var record in weenie.PropertiesCreateList) result.PropertiesCreateList.Add(record.Clone());
                }

                if (weenie.PropertiesEmote != null && (instantiateEmptyCollections || weenie.PropertiesEmote.Count > 0))
                {
                    result.PropertiesEmote = new Collection<PropertiesEmote>();
                    foreach (var record in weenie.PropertiesEmote) result.PropertiesEmote.Add(record.Clone());
                }

                if (weenie.PropertiesEventFilter != null && (instantiateEmptyCollections || weenie.PropertiesEventFilter.Count > 0)) result.PropertiesEventFilter = new HashSet<int>(weenie.PropertiesEventFilter);
                if (weenie.PropertiesGenerator != null && (instantiateEmptyCollections || weenie.PropertiesGenerator.Count > 0))
                {
                    result.PropertiesGenerator = new List<PropertiesGenerator>(weenie.PropertiesGenerator.Count);
                    foreach (var record in weenie.PropertiesGenerator) result.PropertiesGenerator.Add(record.Clone());
                }
            }

            // Properties for creatures

            if (weenie.PropertiesAttribute != null && (instantiateEmptyCollections || weenie.PropertiesAttribute.Count > 0))
            {
                result.PropertiesAttribute = new Dictionary<PropertyAttribute, PropertiesAttribute>(weenie.PropertiesAttribute.Count);
                foreach (var kvp in weenie.PropertiesAttribute) result.PropertiesAttribute.Add(kvp.Key, kvp.Value.Clone());
            }

            if (weenie.PropertiesAttribute2nd != null && (instantiateEmptyCollections || weenie.PropertiesAttribute2nd.Count > 0))
            {
                result.PropertiesAttribute2nd = new Dictionary<PropertyAttribute2nd, PropertiesAttribute2nd>(weenie.PropertiesAttribute2nd.Count);
                foreach (var kvp in weenie.PropertiesAttribute2nd) result.PropertiesAttribute2nd.Add(kvp.Key, kvp.Value.Clone());
            }

            if (referenceWeenieCollectionsForCommonProperties) result.PropertiesBodyPart = weenie.PropertiesBodyPart;
            else
            {
                if (weenie.PropertiesBodyPart != null && (instantiateEmptyCollections || weenie.PropertiesBodyPart.Count > 0))
                {
                    result.PropertiesBodyPart = new Dictionary<CombatBodyPart, PropertiesBodyPart>(weenie.PropertiesBodyPart.Count);
                    foreach (var kvp in weenie.PropertiesBodyPart) result.PropertiesBodyPart.Add(kvp.Key, kvp.Value.Clone());
                }
            }

            if (weenie.PropertiesSkill != null && (instantiateEmptyCollections || weenie.PropertiesSkill.Count > 0))
            {
                result.PropertiesSkill = new Dictionary<Skill, PropertiesSkill>(weenie.PropertiesSkill.Count);
                foreach (var kvp in weenie.PropertiesSkill) result.PropertiesSkill.Add(kvp.Key, kvp.Value.Clone());
            }

            // Properties for books

            if (weenie.PropertiesBook != null) result.PropertiesBook = weenie.PropertiesBook.Clone();
            if (weenie.PropertiesBookPageData != null && (instantiateEmptyCollections || weenie.PropertiesBookPageData.Count > 0)) result.PropertiesBookPageData = new List<PropertiesBookPageData>(weenie.PropertiesBookPageData);

            return result;
        }
    }
}
