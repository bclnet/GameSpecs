using GameSpec.Explorer;
using GameSpec.Formats;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GameSpec.AC.Formats.Entity
{
    public class GameTime : IGetExplorerInfo
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
            YearSpec = r.ReadL16String(Encoding.Default); r.AlignBoundary();
            TimesOfDay = r.ReadL32Array(x => new TimeOfDay(x));
            DaysOfTheWeek = r.ReadL32Array(x => { var weekDay = r.ReadL16String(); r.AlignBoundary(); return weekDay; });
            Seasons = r.ReadL32Array(x => new Season(x));
        }

        //: Entity.GameTime
        List<ExplorerInfoNode> IGetExplorerInfo.GetInfoNodes(ExplorerManager resource, FileMetadata file, object tag)
        {
            var nodes = new List<ExplorerInfoNode> {
                new ExplorerInfoNode($"ZeroTimeOfYear: {ZeroTimeOfYear}"),
                new ExplorerInfoNode($"ZeroYear: {ZeroYear}"),
                new ExplorerInfoNode($"DayLength: {DayLength}"),
                new ExplorerInfoNode($"DaysPerYear: {DaysPerYear}"),
                new ExplorerInfoNode($"YearSpec: {YearSpec}"),
                new ExplorerInfoNode("TimesOfDay", items: TimesOfDay.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[2].Name.Replace("Name: ", "");
                    items.RemoveAt(2);
                    return new ExplorerInfoNode(name, items: items);
                })),
                new ExplorerInfoNode("DaysOfWeek", items: DaysOfTheWeek.Select(x => new ExplorerInfoNode($"{x}"))),
                new ExplorerInfoNode("Seasons", items: Seasons.Select(x => {
                    var items = (x as IGetExplorerInfo).GetInfoNodes();
                    var name = items[1].Name.Replace("Name: ", "");
                    items.RemoveAt(1);
                    return new ExplorerInfoNode(name, items: items);
                })),
            };
            return nodes;
        }
    }
}
