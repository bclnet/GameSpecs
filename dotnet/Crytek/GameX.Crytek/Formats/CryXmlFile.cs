using GameX.Formats;
using GameX.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace GameX.Crytek.Formats
{
    public class CryXmlFile : XmlDocument, IHaveStream, IHaveMetaInfo
    {
        class Node
        {
            public int NodeId { get; set; }
            public int NodeNameOffset { get; set; }
            public int ItemType { get; set; }
            public short AttributeCount { get; set; }
            public short ChildCount { get; set; }
            public int ParentNodeId { get; set; }
            public int FirstAttributeIndex { get; set; }
            public int FirstChildIndex { get; set; }
            public int Reserved { get; set; }
        }

        public static Task<object> Factory(BinaryReader r, FileSource m, PakFile s)
            => Task.FromResult((object)new CryXmlFile(r, false));

        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag) => new List<MetaInfo> {
            new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = this }),
        };

        public CryXmlFile(string inFile, bool writeLog = false) : this(new BinaryReader(File.OpenRead(inFile)), writeLog) { }
        public CryXmlFile(byte[] bytes, bool writeLog = false) : this(new BinaryReader(new MemoryStream(bytes)), writeLog) { }
        public CryXmlFile(BinaryReader r, bool writeLog = false)
        {
            var startOffset = (int)r.BaseStream.Position;
            var peek = r.PeekChar();
            if (peek == '<') { Load(r.BaseStream); return; } // File is already XML, so return the XML.
            else if (peek != 'C') throw new Exception("Unknown File Format"); // Unknown file format

            var header = r.ReadFYString(7);
            if (header == "CryXml" || header == "CryXmlB") r.ReadCString();
            else if (header == "CRY3SDK") r.ReadBytes(2);
            else throw new FormatException("Unknown File Format");

            var headerLength = r.BaseStream.Position;
            var fileLength = r.ReadInt32();
            if (fileLength != r.BaseStream.Length) throw new FormatException("Invalid byte order");

            var nodeTableOffset = r.ReadInt32();
            var nodeTableCount = r.ReadInt32();
            var nodeTableSize = 28;

            var attributeTableOffset = r.ReadInt32();
            var attributeTableCount = r.ReadInt32();
            var attributeTableSize = 8;

            var childTableOffset = r.ReadInt32();
            var childTableCount = r.ReadInt32();
            var childTableSize = 4;

            var stringTableOffset = r.ReadInt32();
            var stringTableCount = r.ReadInt32();

            // NODE TABLE
            if (writeLog)
            {
                Console.WriteLine("Header");
                Console.WriteLine($"0x{0x00:X6}: {header}");
                Console.WriteLine($"0x{headerLength + 0x00:X6}: {{0:X8}} (Dec: {{0:D8}})", fileLength);
                Console.WriteLine($"0x{headerLength + 0x04:X6}: {{0:X8}} (Dec: {{0:D8}})", nodeTableOffset);
                Console.WriteLine($"0x{headerLength + 0x08:X6}: {{0:X8}} (Dec: {{0:D8}})", nodeTableCount);
                Console.WriteLine($"0x{headerLength + 0x12:X6}: {{0:X8}} (Dec: {{0:D8}})", attributeTableOffset);
                Console.WriteLine($"0x{headerLength + 0x16:X6}: {{0:X8}} (Dec: {{0:D8}})", attributeTableCount);
                Console.WriteLine($"0x{headerLength + 0x20:X6}: {{0:X8}} (Dec: {{0:D8}})", childTableOffset);
                Console.WriteLine($"0x{headerLength + 0x24:X6}: {{0:X8}} (Dec: {{0:D8}})", childTableCount);
                Console.WriteLine($"0x{headerLength + 0x28:X6}: {{0:X8}} (Dec: {{0:D8}})", stringTableOffset);
                Console.WriteLine($"0x{headerLength + 0x32:X6}: {{0:X8}} (Dec: {{0:D8}})", stringTableCount);
                Console.WriteLine("\nNode Table");
            }
            var nodeTable = new List<Node> { };
            r.BaseStream.Seek(nodeTableOffset, SeekOrigin.Begin);
            var nodeId = 0;
            while (r.BaseStream.Position < nodeTableOffset + nodeTableCount * nodeTableSize)
            {
                var position = r.BaseStream.Position;
                var value = new Node
                {
                    NodeId = nodeId++,
                    NodeNameOffset = r.ReadInt32(),
                    ItemType = r.ReadInt32(),
                    AttributeCount = r.ReadInt16(),
                    ChildCount = r.ReadInt16(),
                    ParentNodeId = r.ReadInt32(),
                    FirstAttributeIndex = r.ReadInt32(),
                    FirstChildIndex = r.ReadInt32(),
                    Reserved = r.ReadInt32(),
                };
                nodeTable.Add(value);
                if (writeLog) Console.WriteLine($"0x{position:X6}: {value.NodeNameOffset:X8} {value.ItemType:X8} {value.AttributeCount:X4} {value.ChildCount:X4} {value.ParentNodeId:X8} {value.FirstAttributeIndex:X8} {value.FirstChildIndex:X8} {value.Reserved:X8}");
            }

            // ATTRIBUTE TABLE
            if (writeLog) Console.WriteLine("\nAttribute Table");
            var attributeTable = new List<(int NameOffset, int ValueOffset)> { };
            r.BaseStream.Seek(attributeTableOffset, SeekOrigin.Begin);
            while (r.BaseStream.Position < attributeTableOffset + attributeTableCount * attributeTableSize)
            {
                var position = r.BaseStream.Position;
                var value = (NameOffset: r.ReadInt32(), ValueOffset: r.ReadInt32());
                attributeTable.Add(value);
                if (writeLog) Console.WriteLine($"0x{position:X6}: {value.NameOffset:X8} {value.ValueOffset:X8}");
            }

            // PARENT TABLE
            if (writeLog) Console.WriteLine("\nParent Table");
            var parentTable = new List<int> { };
            r.BaseStream.Seek(childTableOffset, SeekOrigin.Begin);
            while (r.BaseStream.Position < childTableOffset + childTableCount * childTableSize)
            {
                var position = r.BaseStream.Position;
                var value = r.ReadInt32();
                parentTable.Add(value);
                if (writeLog) Console.WriteLine($"0x{position:X6}: {value:X8}");
            }

            // STRING DICTIONARY
            if (writeLog) Console.WriteLine("\nString Dictionary");

            var dataTable = new List<(int Offset, string Value)> { };
            r.BaseStream.Seek(stringTableOffset, SeekOrigin.Begin);
            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                var position = r.BaseStream.Position;
                var value = (Offset: (int)position - stringTableOffset, Value: r.ReadCString());
                dataTable.Add(value);
                if (writeLog) Console.WriteLine($"0x{position:X6}: {value.Offset:X8} {value.Value}");
            }

            var dataMap = dataTable.ToDictionary(k => k.Offset, v => v.Value);
            var attributeIndex = 0;

            // DOCUMENT
            var xmlMap = new Dictionary<int, XmlElement> { };
            foreach (var node in nodeTable)
            {
                var element = CreateElement(dataMap[node.NodeNameOffset]);
                for (int i = 0, j = node.AttributeCount; i < j; i++)
                {
                    if (dataMap.ContainsKey(attributeTable[attributeIndex].ValueOffset)) element.SetAttribute(dataMap[attributeTable[attributeIndex].NameOffset], dataMap[attributeTable[attributeIndex].ValueOffset]);
                    else { element.SetAttribute(dataMap[attributeTable[attributeIndex].NameOffset], "BUGGED"); }
                    attributeIndex++;
                }
                xmlMap[node.NodeId] = element;
                if (xmlMap.ContainsKey(node.ParentNodeId)) xmlMap[node.ParentNodeId].AppendChild(element);
                else AppendChild(element);
            }
        }

        public override string ToString()
        {
            using var s = new MemoryStream();
            using var w = new XmlTextWriter(s, Encoding.Unicode) { Formatting = Formatting.Indented };
            WriteContentTo(w);
            w.Flush();
            s.Flush();
            s.Position = 0;
            return new StreamReader(s).ReadToEnd();
        }

        public static TObject Deserialize<TObject>(Stream inStream) where TObject : class
        {
            using var s = new MemoryStream();
            var xs = new XmlSerializer(typeof(TObject));
            var xmlDoc = new CryXmlFile(new BinaryReader(inStream));
            xmlDoc.Save(s);
            s.Seek(0, SeekOrigin.Begin);
            return xs.Deserialize(s) as TObject;
        }

        public static TObject Deserialize<TObject>(string inFile) where TObject : class
        {
            using var s = new MemoryStream();
            var xmlDoc = new CryXmlFile(inFile);
            xmlDoc.Save(s);
            s.Seek(0, SeekOrigin.Begin);
            var xs = new XmlSerializer(typeof(TObject));
            return xs.Deserialize(s) as TObject;
        }

        public Stream GetStream()
        {
            var s = new MemoryStream();
            Save(s);
            s.Seek(0, SeekOrigin.Begin);
            return s;
        }
    }
}
