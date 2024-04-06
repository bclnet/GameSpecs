using GameX.Meta;
using GameX.Cig.Formats;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameX.Cig.Apps.DataForge
{
    public class Node
    {
        static readonly Dictionary<string, Node> Paths = new Dictionary<string, Node>();
        public string Name { get; private set; }
        public object Icon { get; private set; }
        public object Tag { get; }
        public List<Node> Items { get; } = new List<Node>();
        public List<Entity> Entities { get; } = new List<Entity>();

        public static void CreateNode(MetaManager manager, List<Node> nodes, Binary_Dcb.Record v)
        {
            var path = v.FileName?[21..] ?? v.Name;
            var icon = manager.FolderIcon;
            var node = Paths.TryGetValue(path, out var z) ? z : CreatePath(nodes, path, icon);
            node.Entities.Add(new Entity { Name = v.Name, Value = v });
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
        public Binary_Dcb.Record Value { get; set; }
    }

    /// <summary>
    /// Database
    /// </summary>
    public class Database
    {
        Family family;
        PakFile pakFile;

        public List<Node> Nodes = new List<Node>();

        public async Task OpenAsync(MetaManager manager)
        {
            family = FamilyManager.GetFamily("Cig");
            pakFile = family.OpenPakFile(new Uri("game:/#StarCitizen"));
            var obj = await pakFile.LoadFileObject<Binary_Dcb>($"Data/Game.dcb");
            foreach (var value in obj.RecordTable)
                Node.CreateNode(manager, Nodes, value);
        }
    }
}