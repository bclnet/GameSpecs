using GameSpec.Metadata;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class GameTime : IGetMetadataInfo
    {
        public double ZeroTimeOfYear;
        public uint ZeroYear; // Year "0" is really "P.Y. 10" in the calendar.
        public float DayLength;
        public uint DaysPerYear; // 360. Likely for easier math so each month is same length
        public string YearSpec; // "P.Y."
        public TimeOfDay[] TimesOfDay;
        public string[] DaysOfTheWeek;
        public Season[] Seasons;

        public GameTime(BinaryReader r)
        {
            ZeroTimeOfYear = r.ReadDouble();
            ZeroYear = r.ReadUInt32();
            DayLength = r.ReadSingle();
            DaysPerYear = r.ReadUInt32();
            YearSpec = r.ReadL16String(Encoding.Default); r.Align();
            TimesOfDay = r.ReadL32Array(x => new TimeOfDay(x));
            DaysOfTheWeek = r.ReadL32Array(x => { var weekDay = r.ReadL16String(); r.Align(); return weekDay; });
            Seasons = r.ReadL32Array(x => new Season(x));
        }

        //: Entity.GameTime
        List<MetadataInfo> IGetMetadataInfo.GetInfoNodes(MetadataManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<MetadataInfo> {
                new MetadataInfo($"ZeroTimeOfYear: {ZeroTimeOfYear}"),
                new MetadataInfo($"ZeroYear: {ZeroYear}"),
                new MetadataInfo($"DayLength: {DayLength}"),
                new MetadataInfo($"DaysPerYear: {DaysPerYear}"),
                new MetadataInfo($"YearSpec: {YearSpec}"),
                new MetadataInfo("TimesOfDay", items: TimesOfDay.Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[2].Name.Replace("Name: ", "");
                    items.RemoveAt(2);
                    return new MetadataInfo(name, items: items);
                })),
                new MetadataInfo("DaysOfWeek", items: DaysOfTheWeek.Select(x => new MetadataInfo($"{x}"))),
                new MetadataInfo("Seasons", items: Seasons.Select(x => {
                    var items = (x as IGetMetadataInfo).GetInfoNodes();
                    var name = items[1].Name.Replace("Name: ", "");
                    items.RemoveAt(1);
                    return new MetadataInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
