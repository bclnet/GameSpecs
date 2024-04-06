using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

namespace GameX.Valve.Formats.Blocks
{
    //was:Resource/ResourceTypes/PanoramaLayout
    public class DATAPanoramaLayout : DATAPanorama
    {
        DATABinaryKV3 _layoutContent;

        public override void Read(Binary_Pak parent, BinaryReader r)
        {
            base.Read(parent, r);
            _layoutContent = parent.GetBlockByType<LACO>();
        }

        public override string ToString() => _layoutContent == default
            ? base.ToString()
            : PanoramaLayoutPrinter.Print(_layoutContent.Data);

        static class PanoramaLayoutPrinter
        {
            public static string Print(IDictionary<string, object> layoutRoot)
            {
                using var w = new IndentedTextWriter();
                w.WriteLine("<!-- xml reconstructed by ValveResourceFormat: https://vrf.steamdb.info/ -->");
                var root = layoutRoot.GetSub("m_AST")?.GetSub("m_pRoot");
                if (root == default) throw new InvalidDataException("Unknown LaCo format, unable to format to XML");
                PrintNode(root, w);
                return w.ToString();
            }

            static void PrintNode(IDictionary<string, object> node, IndentedTextWriter writer)
            {
                var type = node.Get<string>("eType");
                switch (type)
                {
                    case "ROOT": PrintPanelBase("root", node, writer); break;
                    case "STYLES": PrintPanelBase("styles", node, writer); break;
                    case "INCLUDE": PrintInclude(node, writer); break;
                    case "PANEL": PrintPanel(node, writer); break;
                    case "SCRIPT_BODY": PrintScriptBody(node, writer); break;
                    case "SCRIPTS": PrintPanelBase("scripts", node, writer); break;
                    case "SNIPPET": PrintSnippet(node, writer); break;
                    case "SNIPPETS": PrintPanelBase("snippets", node, writer); break;
                    default: throw new ArgumentOutOfRangeException(nameof(type), $"Unknown node type {type}");
                };
            }

            static void PrintPanel(IDictionary<string, object> node, IndentedTextWriter w)
            {
                var name = node.Get<string>("name");
                PrintPanelBase(name, node, w);
            }

            static void PrintPanelBase(string name, IDictionary<string, object> node, IndentedTextWriter w)
            {
                var attributes = NodeAttributes(node);
                var nodeChildren = NodeChildren(node);
                if (!nodeChildren.Any()) { PrintOpenNode(name, attributes, " />", w); return; }
                PrintOpenNode(name, attributes, ">", w); w.Indent++;
                foreach (var child in nodeChildren) PrintNode(child, w);
                w.Indent--; w.WriteLine($"</{name}>");
            }

            static void PrintInclude(IDictionary<string, object> node, IndentedTextWriter w)
            {
                var reference = node.GetSub("child");
                w.Write($"<include src=");
                PrintAttributeOrReferenceValue(reference, w);
                w.WriteLine(" />");
            }

            static void PrintScriptBody(IDictionary<string, object> node, IndentedTextWriter w)
            {
                var content = node.Get<string>("name");
                w.Write("<script><![CDATA[");
                w.Write(content);
                w.WriteLine("]]></script>");
            }

            static void PrintSnippet(IDictionary<string, object> node, IndentedTextWriter w)
            {
                var nodeChildren = NodeChildren(node);
                var name = node.Get<string>("name");
                w.WriteLine($"<snippet name=\"{name}\">"); w.Indent++;
                foreach (var child in nodeChildren) PrintNode(child, w);
                w.Indent--; w.WriteLine("</snippet>");
            }

            static void PrintOpenNode(string name, IEnumerable<IDictionary<string, object>> attributes, string nodeEnding, IndentedTextWriter w)
            {
                w.Write($"<{name}");
                PrintAttributes(attributes, w);
                w.WriteLine(nodeEnding);
            }

            static void PrintAttributes(IEnumerable<IDictionary<string, object>> attributes, IndentedTextWriter w)
            {
                foreach (var attribute in attributes)
                {
                    var name = attribute.Get<string>("name");
                    var value = attribute.GetSub("child");
                    w.Write($" {name}=");
                    PrintAttributeOrReferenceValue(value, w);
                }
            }

            static void PrintAttributeOrReferenceValue(IDictionary<string, object> attributeValue, IndentedTextWriter w)
            {
                var value = attributeValue.Get<string>("name");
                var type = attributeValue.Get<string>("eType");
                value = type switch
                {
                    "REFERENCE_COMPILED" => "s2r://" + value,
                    "REFERENCE_PASSTHROUGH" => "file://" + value,
                    "PANEL_ATTRIBUTE_VALUE" => SecurityElement.Escape(value),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unknown node type {type}"),
                };
                w.Write($"\"{value}\"");
            }

            static bool IsAttribute(IDictionary<string, object> node) => node.Get<string>("eType") == "PANEL_ATTRIBUTE";
            static IEnumerable<IDictionary<string, object>> NodeAttributes(IDictionary<string, object> node) => SubNodes(node).Where(n => IsAttribute(n));
            static IEnumerable<IDictionary<string, object>> NodeChildren(IDictionary<string, object> node) => SubNodes(node).Where(n => !IsAttribute(n));
            static IEnumerable<IDictionary<string, object>> SubNodes(IDictionary<string, object> node)
                => node.ContainsKey("vecChildren") ? node.GetArray("vecChildren") : node.ContainsKey("child")
                    ? (new[] { node.GetSub("child") })
                    : (IEnumerable<IDictionary<string, object>>)Array.Empty<IDictionary<string, object>>();
        }
    }
}
