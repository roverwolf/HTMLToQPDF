using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using HTMLQuestPDF.Extensions;
using HTMLQuestPDF.Utils;
using HTMLToQPDF.Components;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace HTMLQuestPDF.Components
{
    internal class ParagraphComponent : IComponent
    {
        private readonly List<HtmlNode> lineNodes;
        private readonly Dictionary<string, TextStyle> textStyles;

        public ParagraphComponent(List<HtmlNode> lineNodes, HTMLComponentsArgs args)
        {
            this.lineNodes = lineNodes;
            this.textStyles = args.TextStyles;
        }

        private HtmlNode? GetParrentBlock(HtmlNode node)
        {
            if (node == null) return null;
            return node.IsBlockNode() ? node : GetParrentBlock(node.ParentNode);
        }

        private HtmlNode? GetListItemNode(HtmlNode node)
        {
            if (node == null || node.IsList()) return null;
            return node.IsListItem() ? node : GetListItemNode(node.ParentNode);
        }

        public void Compose(IContainer container)
        {
            var listItemNode = GetListItemNode(lineNodes.First()) ?? GetParrentBlock(lineNodes.First());
            if (listItemNode == null) return;

            var numberInList = listItemNode.GetNumberInList();
            var alignment = GetAlignmentAttribute(listItemNode);

            if (numberInList != -1 || listItemNode.GetListNode() != null)
            {
                container.Row(row =>
                {
                    var listPrefix = numberInList == -1 ? "" : numberInList == 0 ? "•  " : $"{numberInList}. ";
                    row.AutoItem().MinWidth(26).AlignCenter().Text(listPrefix);
                    container = row.RelativeItem();
                });
            }

            var first = lineNodes.First();
            var last = lineNodes.First();

            first.InnerHtml = first.InnerHtml.TrimStart();
            last.InnerHtml = last.InnerHtml.TrimEnd();

            container = container.Align(alignment);
            container.Text(GetAction(lineNodes, alignment));
        }

        private Action<TextDescriptor> GetAction(List<HtmlNode> nodes, string alignment)
        {
            return text =>
            {
                lineNodes.ForEach(node => GetAction(node, alignment).Invoke(text));
            };
        }

        private Action<TextDescriptor> GetAction(HtmlNode node, string alignment)
        {
            return text =>
            {
                if (node.NodeType == HtmlNodeType.Text)
                {
                    var span = text.Span(node.InnerText);
                    GetTextSpanAction(node).Invoke(span);
                    if (alignment == "ql-align-justify" || alignment == "justify")
                    {
                        text.Justify();
                    }
                }
                else if (node.IsBr())
                {
                    var span = text.Span("\n");
                    GetTextSpanAction(node).Invoke(span);
                }
                else
                {
                    foreach (var item in node.ChildNodes)
                    {
                        var action = GetAction(item, alignment);
                        action(text);
                    }
                }
            };
        }

        private TextSpanAction GetTextSpanAction(HtmlNode node)
        {
            return spanAction =>
            {
                var action = GetTextStyles(node);
                action(spanAction);
                if (node.ParentNode != null)
                {
                    var parrentAction = GetTextSpanAction(node.ParentNode);
                    parrentAction(spanAction);
                }
            };
        }

        public TextSpanAction GetTextStyles(HtmlNode element)
        {
            return (span) => span.Style(GetTextStyle(element));
        }

        public TextStyle GetTextStyle(HtmlNode element)
        {
            // Start with tag-based style or default
            var style = textStyles.TryGetValue(element.Name.ToLower(), out TextStyle? tagStyle)
                ? tagStyle
                : TextStyle.Default;

            // Apply inline CSS styles if present
            var styleAttr = element.GetAttributeValue("style", string.Empty);
            if (!string.IsNullOrEmpty(styleAttr))
            {
                var cssStyles = CssParser.ParseStyleAttribute(styleAttr);
                style = CssParser.ApplyTextStyles(style, cssStyles);
            }

            return style;
        }

        private static string GetAlignmentAttribute(HtmlNode element)
        {
            foreach (var attr in element.Attributes)
            {
                if (attr.Name == "class")
                {
                    foreach (var className in attr.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (className == "ql-align-right" ||
                            className == "ql-align-center" ||
                            className == "ql-align-justify")
                        {
                            return className;
                        }
                    }
                }
                else if (attr.Name == "style")
                {
                    foreach (var declaration in attr.Value.Split(';', StringSplitOptions.RemoveEmptyEntries))
                    {
                        var colonIndex = declaration.IndexOf(':');
                        if (colonIndex <= 0)
                        {
                            continue;
                        }

                        var property = declaration[..colonIndex].Trim();
                        if (property == "text-align")
                        {
                            return declaration[(colonIndex + 1)..].Trim();
                        }
                    }
                }
            }

            return string.Empty;
        }
    }
}