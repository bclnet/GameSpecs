using GameSpec.Metadata;
using GameSpec.Rsi.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Obj = System.Collections.Generic.Dictionary<string, object>;

namespace GameSpec.Rsi.Apps.DataForge
{
    public class Node
    {
        static readonly Dictionary<string, Node> Paths = new Dictionary<string, Node>();
        public string Name { get; private set; }
        public object Icon { get; private set; }
        public object Tag { get; }
        public List<Node> Items { get; } = new List<Node>();
        public List<Entity> Entities { get; } = new List<Entity>();

        public static void CreateNode(MetadataManager manager, List<Node> nodes, string k, List<Obj> v)
        {
            var parts = k.Split(".");
            var pathTake = 1; // parts.Length > 3 ? 3 : 1;
            var path = string.Join('/', parts.Take(pathTake));
            var icon = manager.FolderIcon;
            var node = Paths.TryGetValue(path, out var z) ? z : CreatePath(nodes, path, icon);
            node.Entities.Add(new Entity { Name = k, Value = v });
        }

        static Node CreatePath(List<Node> nodes, string path, object icon)
        {
            var lastIndex = path.LastIndexOf('/');
            if (lastIndex == -1)
            {
                var newNode2 = new Node { Name = path, Icon = icon };
                nodes.Add(newNode2);
                Paths.Add(path, newNode2);
                return newNode2;
            }
            string prevPath = path[..lastIndex], nextPath = path[(lastIndex + 1)..];
            var node = Paths.TryGetValue(prevPath, out var z) ? z : CreatePath(nodes, prevPath, icon);
            var newNode = new Node { Name = nextPath, Icon = icon };
            node.Items.Add(newNode);
            Paths.Add(path, newNode);
            return newNode;
        }
    }

    public class Entity
    {
        public string Name { get; set; }
        public List<Obj> Value { get; set; }
    }

    /// <summary>
    /// Database
    /// </summary>
    public class Database
    {
        Family family;
        PakFile pakFile;

        public List<Node> Nodes = new List<Node>();

        public async Task OpenAsync(MetadataManager manager)
        {
            family = FamilyManager.GetFamily("Rsi");
            pakFile = family.OpenPakFile(new Uri("game:/Data.p4k#StarCitizen"));
            var obj = await pakFile.LoadFileObjectAsync<BinaryDcb>($"Data/Game.dcb");
            var valueMap = obj.ValueMap;
            var structTypes = obj.StructTypes;
            //foreach (var (key, value) in obj.DataMap)
            //    Node.CreateNode(manager, Nodes, structTypes[key].GetName(valueMap), value);
            foreach (var (key, value) in obj.RecordMap)
                Node.CreateNode(manager, Nodes, structTypes[key].GetName(valueMap), value);
        }
    }
}