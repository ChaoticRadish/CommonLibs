using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows;
using Common_Wpf.Interfaces;

namespace Common_Wpf.Helpers
{
    /// <summary>
    /// <see cref="RichTextBox"/> 扩展依赖属性
    /// </summary>
    public static class RichTextBoxHelper
    {
        /// <summary>
        /// 附加属性: 绑定数据源
        /// </summary>
        public static readonly DependencyProperty DocumentSourceProperty =
            DependencyProperty.RegisterAttached(
                "DocumentSource",
                typeof(object),
                typeof(RichTextBoxHelper),
                new PropertyMetadata(OnDocumentSourceChanged));

        /// <summary>
        /// 附加属性: 绑定转换器
        /// </summary>
        public static readonly DependencyProperty DocumentConverterProperty =
            DependencyProperty.RegisterAttached(
                "DocumentConverter",
                typeof(IFlowDocumentConverter),
                typeof(RichTextBoxHelper),
                new PropertyMetadata(null));

        /// <summary>
        /// 内部附加属性: 取消令牌
        /// </summary>
        private static readonly DependencyProperty CtsProperty =
            DependencyProperty.RegisterAttached(
                "Cts",
                typeof(CancellationTokenSource),
                typeof(RichTextBoxHelper));

        public static object? GetDocumentSource(DependencyObject obj) => obj.GetValue(DocumentSourceProperty);
        public static void SetDocumentSource(DependencyObject obj, object? value) => obj.SetValue(DocumentSourceProperty, value);

        public static IFlowDocumentConverter GetDocumentConverter(DependencyObject obj) => (IFlowDocumentConverter)obj.GetValue(DocumentConverterProperty);
        public static void SetDocumentConverter(DependencyObject obj, IFlowDocumentConverter value) => obj.SetValue(DocumentConverterProperty, value);

        private static CancellationTokenSource? GetCts(DependencyObject obj) => (CancellationTokenSource?)obj.GetValue(CtsProperty);
        private static void SetCts(DependencyObject obj, CancellationTokenSource? value) => obj.SetValue(CtsProperty, value);


        private static async void OnDocumentSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RichTextBox richTextBox)
            {
                // 取消上一个任务
                var oldCts = GetCts(richTextBox);
                oldCts?.Cancel();
                oldCts?.Dispose();


                var source = e.NewValue;
                var converter = GetDocumentConverter(richTextBox);

                if (source is FlowDocument flowDocument)
                {
                    richTextBox.Document = flowDocument;
                    return;
                }
                if (source == null || converter == null)
                {
                    richTextBox.Document = new FlowDocument();
                    return;
                }

                var newCts = new CancellationTokenSource();
                SetCts(richTextBox, newCts);

                try
                {
                    FlowDocument newFlowDoc;

                    
                    if (converter is IAsyncFlowDocumentConverter asyncConverter)
                    {
                        newFlowDoc = await asyncConverter.ConvertAsync(source, newCts.Token);
                    }
                    else
                    {
                        newFlowDoc = await Task.Run(() => converter.Convert(source), newCts.Token);
                    }

                    if (!newCts.Token.IsCancellationRequested)
                    {
                        richTextBox.Document = newFlowDoc ?? new FlowDocument();
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    richTextBox.Document = new FlowDocument(new Paragraph(new Run($"数据源 ({source.GetType()}) 转换错误: {ex.Message}")));
                }
                finally
                {
                    if (GetCts(richTextBox) == newCts)
                    {
                        SetCts(richTextBox, null);
                    }
                    newCts.Dispose();
                }
            }
        }
    }
}
