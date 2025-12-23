using Common_Util.Data.Structure.Tree;
using Common_Util.Data.Structure.Tree.Extensions;
using Common_Util.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace Common_Wpf.Helpers.FlowDocuments
{
    /// <summary>
    /// <see cref="FlowDocument"/> 生成帮助类
    /// </summary>
    public static class FlowDocumentGenerator
    {
        public delegate ValueTask<FlowDocumentTreeNodeBase> AsyncGenerateFromTreeConverter<T>(T value, FlowDocumentTreeNodeBase? parent);
        public static async Task<FlowDocument> GenerateFromTree<TValue>(
            IMultiTree<TValue> tree, AsyncGenerateFromTreeConverter<TValue> nodeValueConverter)
        {
            FlowDocument document = new FlowDocument();
            if (tree.Root == null) return document;

            var list = new List();
            RootNode docRoot = new();
            List<TextElement> rootItems = [];
            foreach (var firstLayerNode in tree.Root.Childrens)
            {
                List<(FlowDocumentTreeNodeBase node, TextElement? element)> allSubNodes = [];
                TextElement? subRoot = null;
                foreach (var (index, node) in firstLayerNode.IndexPreorder().WithIndex())
                {
                    var parent = index == 0 ? docRoot : allSubNodes[node.ParentIndex].node;
                    var docNode = await nodeValueConverter(node.NodeValue, parent);
                    var element = docNode.ToElement();
                    allSubNodes.Add((docNode, element));
                    if (index == 0 && docNode is EmptyNode) break;
                    subRoot ??= element;
                    if (node.ParentIndex >= 0)
                    {
                        var parentElement = allSubNodes[node.ParentIndex].element;
                        if (element != null && parentElement != null)
                        {
                            AddToElement(element, parentElement);
                        }
                    }
                }
                if (allSubNodes.Count == 0) continue;
                if (subRoot == null) continue;

                rootItems.Add(subRoot);
            }

            // 全部都是内联元素时, 全部添加到一个段落内, 再添加到输出对象中
            if (rootItems.All(i => i is Inline))
            {
                Paragraph paragraph = new Paragraph();
                foreach (var item in rootItems.Cast<Inline>())
                {
                    paragraph.Inlines.Add(item);
                }
                document.Blocks.Add(paragraph);
            }
            // 其他情况下, 内联元素作为一个段落添加到输出对象
            else
            {
                foreach (var item in rootItems)
                {
                    if (item is Inline inline)
                    {
                        document.Blocks.Add(ToBlock(inline));
                    }
                    else if (item is Block block)
                    {
                        document.Blocks.Add(block);
                    }
                    else continue;
                }
            }
            return document;
        }

        private static Block ToBlock(Inline inline)
        {
            Paragraph paragraph = new Paragraph();
            paragraph.Inlines.Add(inline);
            return paragraph;
        }
        private static ListItem ToListItem(Block block)
        {
            var listItem = new ListItem();
            listItem.Blocks.Add(block);
            return listItem;
        }
        private static void AddToElement(TextElement source, TextElement dest)
        {
            // 块级类型的容器
            if (dest is Paragraph paragraphDest)
            {
                if (source is Inline inline)
                    paragraphDest.Inlines.Add(inline);
                else 
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            else if (dest is Section sectionDest)
            {
                if (source is Inline inline)
                    sectionDest.Blocks.Add(ToBlock(inline));
                else if (source is Block block)
                    sectionDest.Blocks.Add(block);
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            else if (dest is List listDest)
            {
                if (source is Inline inline)
                    listDest.ListItems.Add(ToListItem(ToBlock(inline)));
                else if (source is Block block)
                    listDest.ListItems.Add(ToListItem(block));
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            else if (dest is Table tableDest)
            {
                throw new NotImplementedException();
            }
            else if (dest is Figure figureDest)
            {
                if (source is Inline inline)
                    figureDest.Blocks.Add(ToBlock(inline));
                else if (source is Block block)
                    figureDest.Blocks.Add(block);
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            else if (dest is Floater floaterDest)
            {
                if (source is Inline inline)
                    floaterDest.Blocks.Add(ToBlock(inline));
                else if (source is Block block)
                    floaterDest.Blocks.Add(block);
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            // 内联类型的容器
            else if (dest is Span spanDest)
            {
                if (source is Inline inline)
                    spanDest.Inlines.Add(inline);
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            else if (dest is Bold boldDest)
            {
                if (source is Inline inline)
                    boldDest.Inlines.Add(inline);
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            else if (dest is Italic italicDest)
            {
                if (source is Inline inline)
                    italicDest.Inlines.Add(inline);
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            else if (dest is Underline underlineDest)
            {
                if (source is Inline inline)
                    underlineDest.Inlines.Add(inline);
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            else if (dest is Hyperlink hyperlinkDest)
            {
                if (source is Inline inline)
                    hyperlinkDest.Inlines.Add(inline);
                else
                    throw new InvalidOperationException($"添加元素到容器失败. source: {source}; dest: {dest}");
            }
            // 不可包含子元素的类型
            else if (dest is Run) return;
            else if (dest is LineBreak) return;
            // 不可变更包含内容的类型
            else if (dest is InlineUIContainer) return;
            else if (dest is BlockUIContainer) return;
        }
    }
}

