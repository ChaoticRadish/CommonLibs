using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Common_Wpf.Helpers.FlowDocuments
{
    public abstract class FlowDocumentTreeNodeBase
    {
        public FontFamily? FontFamily { get; set; }
        public double? FontSize { get; set; }
        public FontStretch? FontStretch { get; set; }
        public FontStyle? FontStyle { get; set; }
        public FontWeight? FontWeight { get; set; }
        public Brush? Foreground { get; set; }
        public Brush? Background { get; set; }


        public abstract TextElement? ToElement();

        /// <summary>
        /// 设置通用属性
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        protected TElement SetOptions<TElement>(TElement element)
            where TElement : TextElement
        {
            if (FontFamily != null) element.FontFamily = FontFamily;
            if (FontSize != null) element.FontSize = FontSize.Value;
            if (FontStretch != null) element.FontStretch = FontStretch.Value;
            if (FontStyle != null) element.FontStyle = FontStyle.Value;
            if (FontWeight != null) element.FontWeight = FontWeight.Value;
            if (Foreground != null) element.Foreground = Foreground;
            if (Background != null) element.Background = Background;
            return element;
        }
    }

    public abstract class FlowDocumentTreeBlockNodeBase : FlowDocumentTreeNodeBase 
    {
        public override TextElement ToElement() => ToBlockElement();
        /// <summary>
        /// 转换为 UI 元素
        /// </summary>
        /// <returns></returns>
        public abstract Block ToBlockElement();
    }
    public abstract class FlowDocumentTreeInlineNodeBase : FlowDocumentTreeNodeBase
    {
        public TextDecorationCollection? TextDecorations { get; set; }

        public override TextElement ToElement() => ToInlineElement();
        /// <summary>
        /// 转换为 UI 元素
        /// </summary>
        /// <returns></returns>
        public abstract Inline ToInlineElement();

        /// <summary>
        /// 设置通用属性
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="element"></param>
        /// <returns></returns>
        protected TElement SetInlineOptions<TElement>(TElement element)
            where TElement : Inline
        {
            base.SetOptions(element);
            if (TextDecorations != null) element.TextDecorations = TextDecorations;
            return element;
        }
    }
    public class EmptyNode : FlowDocumentTreeNodeBase
    {
        public override TextElement? ToElement() => null;
    }

    public class RootNode : FlowDocumentTreeNodeBase
    {
        public override TextElement? ToElement() => null;
    }
    public class SectionNode : FlowDocumentTreeBlockNodeBase
    {
        public override Block ToBlockElement()
        {
            return new Section();
        }
    }
    public class ParagraphNode : FlowDocumentTreeBlockNodeBase
    {
        public override Block ToBlockElement()
        {
            return new Paragraph();
        }
    }


    public class CustomItemNode : FlowDocumentTreeInlineNodeBase
    {
        public UIElement? UIElement { get; set; }
        public override Inline ToInlineElement()
        {
            if (UIElement == null)
            {
                return new InlineUIContainer();
            }
            else
            {
                return new InlineUIContainer(UIElement);
            }
        }
    }
    public class TextNode : FlowDocumentTreeInlineNodeBase
    {
        public string Text { get; set; } = string.Empty;
        public override Inline ToInlineElement()
        {
            Run run = new Run(Text);
            SetInlineOptions(run);
            return run;
        }
    }
    public class LineBreakNode : FlowDocumentTreeInlineNodeBase
    {
        public override Inline ToInlineElement()
        {
            return new LineBreak();
        }
    }
    public class SpanNode : FlowDocumentTreeInlineNodeBase
    {
        public override Inline ToInlineElement()
        {
            return SetInlineOptions(new Span());
        }
    }
    public class HyperlinkNode : FlowDocumentTreeInlineNodeBase
    {
        public event RoutedEventHandler? Click;
        public override Inline ToInlineElement()
        {
            var link = new Hyperlink();
            if (Click != null) link.Click += Click;
            return link;
        }
    }
}
