using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common_Util.Maths
{
    /// <summary>
    /// 尺寸计算工具
    /// </summary>
    public class SizeCalcUtil
    {

        /// <summary>
        /// 尺寸Zoom计算
        /// </summary>
        /// <param name="source"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static System.Drawing.Size SizeZoom(System.Drawing.Size source, System.Drawing.Size container)
        {
            int outputWidth;
            int outputHeight;

            // 数值有效化
            // 源
            if (source.Width <= 0)
            {
                source.Width = 1;
            }
            if (source.Height <= 0)
            {
                source.Height = 1;
            }
            // 容器
            if (container.Width <= 0)
            {
                container.Width = 1;
            }
            if (container.Height <= 0)
            {
                container.Height = 1;
            }

            float sourceWHScale = (float)source.Width / source.Height;
            float containerWHScale = (float)container.Width / container.Height;

            if (sourceWHScale < containerWHScale)
            {// 源比较窄
                outputHeight = container.Height;
                outputWidth = (int)(outputHeight * sourceWHScale);
            }
            else
            {// 源比较宽
                outputWidth = container.Width;
                outputHeight = (int)(outputWidth / sourceWHScale);
            }

            return new System.Drawing.Size(outputWidth, outputHeight);
        }
        /// <summary>
        /// 将短边设置为指定的长度
        /// </summary>
        /// <param name="source"></param>
        /// <param name="shortEdge"></param>
        /// <returns></returns>
        public static System.Drawing.Size ShortEdge(System.Drawing.Size source, int shortEdge)
        {
            int outputWidth;
            int outputHeight;

            // 数值有效化
            // 源
            if (source.Width <= 0)
            {
                source.Width = 1;
            }
            if (source.Height <= 0)
            {
                source.Height = 1;
            }
            // 输入
            shortEdge = shortEdge > 0 ? shortEdge : 1;

            float sourceWHScale = (float)source.Width / source.Height;

            if (source.Width > source.Height)
            {
                outputHeight = shortEdge;
                outputWidth = (int)(sourceWHScale * outputHeight);
            }
            else
            {
                outputWidth = shortEdge;
                outputHeight = (int)(outputWidth / sourceWHScale);
            }

            return new System.Drawing.Size(outputWidth, outputHeight);
        }

        /// <summary>
        /// 缩放, 使高度是指定值
        /// </summary>
        /// <param name="source"></param>
        /// <param name="targetHeight"></param>
        /// <returns></returns>
        public static System.Drawing.Size ZoomToHeight(System.Drawing.Size source, int targetHeight)
        {
            int outputWidth;
            int outputHeight;
            // 数值有效化
            // 源
            if (source.Width <= 0)
            {
                source.Width = 1;
            }
            if (source.Height <= 0)
            {
                source.Height = 1;
            }
            // 输入
            targetHeight = targetHeight > 0 ? targetHeight : 1;

            float sourceWHScale = (float)source.Width / source.Height;

            outputHeight = targetHeight;
            outputWidth = (int)(sourceWHScale * outputHeight);

            return new System.Drawing.Size(outputWidth, outputHeight);
        }

    }
}
