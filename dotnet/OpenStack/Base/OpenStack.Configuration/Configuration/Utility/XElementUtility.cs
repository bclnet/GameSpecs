using System;
using System.Linq;
using System.Xml.Linq;

namespace OpenStack.Configuration
{
    internal static class XElementUtility
    {
        internal static string GetOptionalAttributeValue(XElement element, string localName)
            => element.Attribute(localName)?.Value;

        internal static string GetOptionalAttributeValue(XElement element, string localName, string namespaceName)
            => element.Attribute(XName.Get(localName, namespaceName))?.Value;

        internal static void AddIndented(XContainer container, XNode content)
        {
            if (container != null && content != null)
            {
                var oneIndentLevel = ComputeOneLevelOfIndentation(container);

                var leadingText = container.PreviousNode as XText;
                var parentIndent = leadingText != null ? leadingText.Value : Environment.NewLine;

                IndentChildrenElements(content as XContainer, parentIndent + oneIndentLevel, oneIndentLevel);

                AddLeadingIndentation(container, parentIndent, oneIndentLevel);
                container.Add(content);
                AddTrailingIndentation(container, parentIndent);
            }
        }

        internal static void RemoveIndented(XNode element)
        {
            if (element != null)
            {
                var textBeforeOrNull = element.PreviousNode as XText;
                var textAfterOrNull = element.NextNode as XText;
                var oneIndentLevel = ComputeOneLevelOfIndentation(element);
                var isLastChild = !element.ElementsAfterSelf().Any();

                element.Remove();

                if (textAfterOrNull != null && IsWhiteSpace(textAfterOrNull))
                    textAfterOrNull.Remove();

                if (isLastChild && textBeforeOrNull != null && IsWhiteSpace(textAfterOrNull))
                    textBeforeOrNull.Value = textBeforeOrNull.Value.Substring(0, textBeforeOrNull.Value.Length - oneIndentLevel.Length);
            }
        }

        static string ComputeOneLevelOfIndentation(XNode node)
        {
            var depth = node.Ancestors().Count();
            var textBeforeOrNull = node.PreviousNode as XText;
            if (depth == 0 || textBeforeOrNull == null || !IsWhiteSpace(textBeforeOrNull))
                return "  ";

            var indentString = textBeforeOrNull.Value.Trim(Environment.NewLine.ToCharArray());
            var lastChar = indentString.LastOrDefault();
            var indentChar = (lastChar == '\t' ? '\t' : ' ');
            var indentLevel = Math.Max(1, indentString.Length / depth);
            return new string(indentChar, indentLevel);
        }

        static bool IsWhiteSpace(XText textNode)
            => string.IsNullOrWhiteSpace(textNode.Value);

        static void IndentChildrenElements(XContainer container, string containerIndent, string oneIndentLevel)
        {
            if (container != null)
            {
                var childIndent = containerIndent + oneIndentLevel;
                foreach (var element in container.Elements())
                {
                    element.AddBeforeSelf(new XText(childIndent));
                    IndentChildrenElements(element, childIndent + oneIndentLevel, oneIndentLevel);
                }

                if (container.Elements().Any())
                    container.Add(new XText(containerIndent));
            }
        }

        static void AddLeadingIndentation(XContainer container, string containerIndent, string oneIndentLevel)
        {
            var containerIsSelfClosed = !container.Nodes().Any();
            var lastChildText = container.LastNode as XText;
            if (containerIsSelfClosed || lastChildText == null)
                container.Add(new XText(containerIndent + oneIndentLevel));
            else
                lastChildText.Value += oneIndentLevel;
        }

        static void AddTrailingIndentation(XContainer container, string containerIndent)
            => container.Add(new XText(containerIndent));
    }
}
