using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Common_Wpf.Interfaces
{
    public interface IFlowDocumentConverter
    {
        /// <summary>
        /// 将原始数据转换为流式文档
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        FlowDocument Convert(object? source);
    }

    public interface IAsyncFlowDocumentConverter : IFlowDocumentConverter
    {
        /// <summary>
        /// 将原始数据异步转换为流式文档
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        ValueTask<FlowDocument> ConvertAsync(object? source, CancellationToken cancellationToken);
    }
}
