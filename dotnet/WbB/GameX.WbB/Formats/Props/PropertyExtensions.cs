using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace GameX.WbB.Formats.Props
{
    public static partial class PropertyExtensions
    {
        public static string GetDescription(this Enum prop) => prop.GetAttributeOfType<DescriptionAttribute>()?.Description ?? prop.ToString();

        /// <summary>
        /// Will add a space infront of capital letter words in a string
        /// </summary>
        /// <param name="attribute2nd"></param>
        /// <returns>string with spaces infront of capital letters</returns>
        public static string ToSentence(this Enum prop) => new string(prop.ToString().Replace("Max", "Maximum").ToCharArray().SelectMany((c, i) => i > 0 && char.IsUpper(c) ? new char[] { ' ', c } : new char[] { c }).ToArray());
    }

    public class GenericPropertiesId<TAttribute> where TAttribute : Attribute
    {
        /// <summary>
        /// Method to return a list of enums by attribute type - in this case [AssessmentProperty] using generics to enhance code reuse.
        /// </summary>
        /// <typeparam name="T">Enum to list by [AssessmentProperty]</typeparam>
        /// <typeparam name="TResult">Type of the results</typeparam>
        static HashSet<TResult> GetValues<T, TResult>() =>
            new HashSet<TResult>(typeof(T).GetFields().Select(x => new
            {
                att = x.GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault(),
                member = x
            }).Where(x => x.att != null && !x.member.IsSpecialName).Select(x => (TResult)x.member.GetValue(null)));

        /// <summary>
        /// returns a list of values for PropertyInt that are [AssessmentProperty]
        /// </summary
        public static HashSet<ushort> PropertiesInt = GetValues<PropertyInt, ushort>();

        /// <summary>
        /// returns a list of values for PropertyInt that are [AssessmentProperty]
        /// </summary>
        public static HashSet<ushort> PropertiesInt64 = GetValues<PropertyInt64, ushort>();

        /// <summary>
        /// returns a list of values for PropertyInt that are [AssessmentProperty]
        /// </summary>
        public static HashSet<ushort> PropertiesBool = GetValues<PropertyBool, ushort>();

        /// <summary>
        /// returns a list of values for PropertyInt that are [AssessmentProperty]
        /// </summary>
        public static HashSet<ushort> PropertiesString = GetValues<PropertyString, ushort>();

        /// <summary>
        /// returns a list of values for PropertyInt that are [AssessmentProperty]
        /// </summary>
        public static HashSet<ushort> PropertiesDouble = GetValues<PropertyFloat, ushort>();

        /// <summary>
        /// returns a list of values for PropertyInt that are [AssessmentProperty]
        /// </summary>
        public static HashSet<ushort> PropertiesDataId = GetValues<PropertyDataId, ushort>();

        /// <summary>
        /// returns a list of values for PropertyInt that are [AssessmentProperty]
        /// </summary>
        public static HashSet<ushort> PropertiesInstanceId = GetValues<PropertyInstanceId, ushort>();
    }

    public class GenericProperties<TAttribute> where TAttribute : Attribute
    {
        /// <summary>
        /// Method to return a list of enums by attribute type - in this case [Ephemeral] using generics to enhance code reuse.
        /// </summary>
        /// <typeparam name="T">Enum to list by [Ephemeral]</typeparam>
        /// <typeparam name="TResult">Type of the results</typeparam>
        static HashSet<T> GetValues<T>() =>
           new HashSet<T>(typeof(T).GetFields().Select(x => new
           {
               att = x.GetCustomAttributes(false).OfType<TAttribute>().FirstOrDefault(),
               member = x
           }).Where(x => x.att != null && !x.member.IsSpecialName).Select(x => (T)x.member.GetValue(null)));

        /// <summary>
        /// returns a list of values for PropertyInt that are [Ephemeral]
        /// </summary
        public static HashSet<PropertyInt> PropertiesInt = GetValues<PropertyInt>();

        /// <summary>
        /// returns a list of values for PropertyInt64 that are [Ephemeral]
        /// </summary>
        public static HashSet<PropertyInt64> PropertiesInt64 = GetValues<PropertyInt64>();

        /// <summary>
        /// returns a list of values for PropertyBool that are [Ephemeral]
        /// </summary>
        public static HashSet<PropertyBool> PropertiesBool = GetValues<PropertyBool>();

        /// <summary>
        /// returns a list of values for PropertyString that are [Ephemeral]
        /// </summary>
        public static HashSet<PropertyString> PropertiesString = GetValues<PropertyString>();

        /// <summary>
        /// returns a list of values for PropertyFloat that are [Ephemeral]
        /// </summary>
        public static HashSet<PropertyFloat> PropertiesDouble = GetValues<PropertyFloat>();

        /// <summary>
        /// returns a list of values for PropertyDataId that are [Ephemeral]
        /// </summary>
        public static HashSet<PropertyDataId> PropertiesDataId = GetValues<PropertyDataId>();

        /// <summary>
        /// returns a list of values for PropertyInstanceId that are [Ephemeral]
        /// </summary>
        public static HashSet<PropertyInstanceId> PropertiesInstanceId = GetValues<PropertyInstanceId>();

        /// <summary>
        /// returns a list of values for PositionType that are [Ephemeral]
        /// </summary>
        public static HashSet<PositionType> PositionTypes = GetValues<PositionType>();
    }
}
