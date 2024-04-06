using GameX.Meta;
using GameX.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameX.WbB.Formats.Entity
{
    public class GameTime : IHaveMetaInfo
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
            YearSpec = r.ReadL16Encoding(Encoding.Default); r.Align();
            TimesOfDay = r.ReadL32FArray(x => new TimeOfDay(x));
            DaysOfTheWeek = r.ReadL32FArray(x => { var weekDay = r.ReadL16Encoding(); r.Align(); return weekDay; });
            Seasons = r.ReadL32FArray(x => new Season(x));
        }

        //: Entity.GameTime
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo($"ZeroTimeOfYear: {ZeroTimeOfYear}"),
                new MetaInfo($"ZeroYear: {ZeroYear}"),
                new MetaInfo($"DayLength: {DayLength}"),
                new MetaInfo($"DaysPerYear: {DaysPerYear}"),
                new MetaInfo($"YearSpec: {YearSpec}"),
                new MetaInfo("TimesOfDay", items: TimesOfDay.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[2].Name.Replace("Name: ", "");
                    items.RemoveAt(2);
                    return new MetaInfo(name, items: items);
                })),
                new MetaInfo("DaysOfWeek", items: DaysOfTheWeek.Select(x => new MetaInfo($"{x}"))),
                new MetaInfo("Seasons", items: Seasons.Select(x => {
                    var items = (x as IHaveMetaInfo).GetInfoNodes();
                    var name = items[1].Name.Replace("Name: ", "");
                    items.RemoveAt(1);
                    return new MetaInfo(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
