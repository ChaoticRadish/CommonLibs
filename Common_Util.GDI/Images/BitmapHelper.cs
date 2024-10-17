using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Common_Util.GDI.Images
{
    /// <summary>
    /// 位图帮助类
    /// </summary>
    public static class BitmapHelper
    {
        /// <summary>
        /// 位图数据
        /// </summary>
        public struct BitmapDataInfo
        {
            /// <summary>
            /// 数据
            /// </summary>
            public byte[] Datas { get; set; }

            /// <summary>
            /// 位图信息数据
            /// </summary>
            public int Width { get; set; }
            /// <summary>
            /// 位图高度
            /// </summary>
            public int Height { get; set; }
            /// <summary>
            /// 是否无数据
            /// </summary>
            public bool IsNonData { get => Datas == null || Datas.Length == 0; }

            public SizeF Size { get => new SizeF(Width, Height); }

            public void Clear()
            {
                Datas = [];
                Width = 0;
                Height = 0;
            }
        }

        #region 像素格式
        /// <summary>
        /// 取得像素格式对应的字节大小 (部分未包含, 未包含对应值将返回-1)
        /// </summary>
        /// <param name="pixelFormat"></param>
        /// <returns></returns>
        public static int GetByteSize(PixelFormat pixelFormat)
        {
            return pixelFormat switch
            {
                PixelFormat.Canonical => 4,
                PixelFormat.Format16bppArgb1555 => 2,
                PixelFormat.Format16bppGrayScale => 2,
                PixelFormat.Format16bppRgb565 => 2,
                PixelFormat.Format24bppRgb => 3,
                PixelFormat.Format32bppArgb => 4,
                PixelFormat.Format32bppPArgb => 4,
                PixelFormat.Format32bppRgb => 4,
                PixelFormat.Format48bppRgb => 6,
                PixelFormat.Format64bppArgb => 8,
                PixelFormat.Format64bppPArgb => 8,
                PixelFormat.Format8bppIndexed => 1,
                _ => -1
            };
        }
        #endregion

        #region 文件压缩
        /// <summary>
        /// 压缩图片质量
        /// </summary>
        /// <param name="image"></param>
        /// <param name="quality">[0, 100]</param>
        /// <returns></returns>
        public static Image CompressionImage(Image image, long quality)
        {
            // 限制质量的数据范围
            if (quality < 0) quality = 0;
            if (quality > 100) quality = 100;

            using (Bitmap bitmap = new Bitmap(image))
            {
                ImageCodecInfo? CodecInfo = FindEncoder(image.RawFormat);
                if (CodecInfo == null)
                {
                    throw new Exception("未能找到图片原始格式对应的编码解码器! " + image.RawFormat.ToString());
                }
                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, quality);
                myEncoderParameters.Param[0] = myEncoderParameter;
                using (MemoryStream ms = new MemoryStream())
                {
                    bitmap.Save(ms, CodecInfo, myEncoderParameters);
                    myEncoderParameters.Dispose();
                    myEncoderParameter.Dispose();
                    return Image.FromStream(ms);
                }
            }
        }
        private static ImageCodecInfo? FindEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                { return codec; }
            }
            return null;
        }
        #endregion

        /// <summary>
        /// 将源读取为图片, 然后缩放到指定大小, 接着存放到指定容器中
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="saveFormat">保存格式, null时使用源格式</param>
        /// <returns></returns>
        public static Task Scale(Stream source, Stream dest, int width, int height,
            ImageFormat? saveFormat = null)
        {
            width = width > 0 ? width : 1;
            height = height > 0 ? height : 1;

            using Bitmap bitmap = new Bitmap(source);
            using Bitmap output = new Bitmap(width, height);

            Graphics graphics = Graphics.FromImage(output);
            graphics.DrawImage(
                bitmap,
                new Rectangle(0, 0, width, height),
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                GraphicsUnit.Pixel
                );

            output.Save(dest, saveFormat ?? bitmap.RawFormat);

            return Task.CompletedTask;
        }
        /// <summary>
        /// 将源读取为图片, 然后缩放到较长边不超过输入大小, 接着存放到指定容器中
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <param name="max"></param>
        /// <param name="saveFormat">保存格式, null时使用源格式</param>
        /// <returns></returns>
        public static Task ScaleSmallThen(Stream source, Stream dest, int max,
            ImageFormat? saveFormat = null)
        {
            max = max > 0 ? max : 1;

            using Bitmap bitmap = new Bitmap(source);

            int finalWidth; // 最终宽度
            int finalHeight;    // 最终高度
            if (bitmap.Width > bitmap.Height)
            {// 宽度较长
                finalWidth = max;
                finalHeight = (int)((float)max / bitmap.Width * bitmap.Height);
            }
            else
            {// 高度较长
                finalHeight = max;
                finalWidth = (int)((float)max / bitmap.Height * bitmap.Width);
            }

            using Bitmap output = new Bitmap(finalWidth, finalHeight);

            Graphics graphics = Graphics.FromImage(output);
            graphics.DrawImage(
                bitmap,
                new Rectangle(0, 0, finalWidth, finalHeight),
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                GraphicsUnit.Pixel
                );

            output.Save(dest, saveFormat ?? bitmap.RawFormat);

            return Task.CompletedTask;
        }

        /// <summary>
        /// 将图片缩放到指定的大小
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Bitmap Scale(Image image, int width, int height)
        {
            width = width > 0 ? width : 1;
            height = height > 0 ? height : 1;
            Bitmap output = new Bitmap(width, height);
            Graphics graphics = Graphics.FromImage(output);
            graphics.DrawImage(
                image,
                new Rectangle(0, 0, width, height),
                new Rectangle(0, 0, image.Width, image.Height),
                GraphicsUnit.Pixel
                );
            return output;
        }
        /// <summary>
        /// 将图片缩放到任意一边都不大于指定大小
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Bitmap ScaleSmallThen(Image image, int size)
        {
            size = size > 0 ? size : 1;
            if (image.Width <= size && image.Height <= size)
            {
                return new Bitmap(image);
            }
            int finalWidth; // 最终宽度
            int finalHeight;    // 最终高度
            if (image.Width > image.Height)
            {// 宽度较长
                finalWidth = size;
                finalHeight = (int)((float)size / image.Width * image.Height);
            }
            else
            {// 高度较长
                finalHeight = size;
                finalWidth = (int)((float)size / image.Height * image.Width);
            }
            return Scale(image, finalWidth, finalHeight);
        }
        /// <summary>
        /// 将图片缩放到较短边都不大于指定大小
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static Bitmap ScaleShorterSmallThen(Image image, int size)
        {
            size = size > 0 ? size : 1;
            if (image.Width <= size || image.Height <= size)
            {// 有一边比输入尺寸短, 说明较短边是小于指定尺寸的
                return new Bitmap(image);
            }
            int finalWidth; // 最终宽度
            int finalHeight;    // 最终高度
            if (image.Width < image.Height)
            {// 宽度较短
                finalWidth = size;
                finalHeight = (int)((float)size / image.Width * image.Height);
            }
            else
            {// 高度较短
                finalHeight = size;
                finalWidth = (int)((float)size / image.Height * image.Width);
            }
            return Scale(image, finalWidth, finalHeight);
        }
        /// <summary>
        /// 取得图片的数据长度
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static long GetImageLength(Image image)
        {
            try
            {
                using (MemoryStream stream = new MemoryStream())
                {
                    image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.Length;
                }
            }
            catch
            {
                Bitmap bitmap = new Bitmap(image);
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    return stream.Length;
                }
            }
        }
        /// <summary>
        /// 将位图转换为byte[]
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public static byte[] ImageToByteArray(System.Drawing.Image image)
        {
            byte[] data = [];
            MemoryStream stream = new MemoryStream();
            try
            {
                image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
            }
            catch
            {
                Bitmap bitmap = new Bitmap(image);
                bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                bitmap.Dispose();
                data = new byte[stream.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(data, 0, Convert.ToInt32(stream.Length));
            }
            finally
            {
                stream.Close();
                stream.Dispose();
            }
            return data;
        }
        /// <summary>
        /// 将位图转换为位图信息数据
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public static BitmapDataInfo ImageToBitmapDataInfo(System.Drawing.Image image)
        {
            BitmapDataInfo output = new BitmapDataInfo
            {
                Datas = ImageToByteArray(image),
                Width = image.Width,
                Height = image.Height
            };
            return output;
        }

        /// <summary>
        /// 将byte[]转换为位图
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap? ByteArrayCloneToBitmap(byte[] bytes)
        {
            try
            {
                byte[] bytesClone = new byte[bytes.Length];
                for (int i = 0; i < bytes.Length; i++)
                {
                    bytesClone[i] = bytes[i];
                }
                using (MemoryStream memoryStream = new MemoryStream(bytesClone))
                {
                    System.Drawing.Bitmap output = new System.Drawing.Bitmap(memoryStream);
                    return output;
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 将byte[]转换为位图
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static System.Drawing.Image? ByteArrayToImage(byte[] bytes)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(bytes))
                {
                    Image output = Image.FromStream(memoryStream);
                    // output.Tag = memoryStream;
                    return output;
                }
            }
            catch
            {
                return null;
            }
        }
        /// <summary>
        /// 尝试通过 <paramref name="bytes"/> 创建图片对象
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="image"></param>
        /// <param name="ex">创建过程中发生的异常</param>
        /// <returns></returns>
        public static bool TryCreateImageFrom(byte[] bytes, [NotNullWhen(true)] out System.Drawing.Image? image, [NotNullWhen(false)] out Exception? ex)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream(bytes);
                Image output = Image.FromStream(memoryStream);
                // output.Tag = memoryStream;
                image = output;
                ex = null;
                return true;
            }
            catch (Exception _ex)
            {
                image = null;
                ex = _ex;
                return false;
            }
        }
        /// <summary>
        /// 检查路径是否图片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool CheckBitmap(string path)
        {
            if (System.IO.File.Exists(path))
            {
                try
                {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    int fsLength = (int)fs.Length;
                    byte[] bytes = new byte[fsLength];
                    fs.Read(bytes, 0, fsLength);
                    fs.Close();
                    Image? image = ByteArrayToImage(bytes);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据路径获取图片
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Image? GetBitmap(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }
            if (System.IO.File.Exists(path))
            {
                try
                {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    int fsLength = (int)fs.Length;
                    byte[] bytes = new byte[fsLength];
                    fs.Read(bytes, 0, fsLength);
                    fs.Close();
                    Image? image = ByteArrayToImage(bytes);
                    return image;
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据路径获取图片数据
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BitmapDataInfo GetBitmapDataInfo(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return new BitmapDataInfo();
            }
            if (System.IO.File.Exists(path))
            {
                try
                {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                    int fsLength = (int)fs.Length;
                    byte[] bytes = new byte[fsLength];
                    fs.Read(bytes, 0, fsLength);
                    fs.Close();
                    Image? image = ByteArrayToImage(bytes);
                    if (image != null)
                    {
                        return ImageToBitmapDataInfo(image);
                    }
                }
                catch
                {
                    return new BitmapDataInfo();
                }
            }
            return new BitmapDataInfo();
        }


        /// <summary>
        /// 取得默认的位图文件名过滤器
        /// </summary>
        /// <returns></returns>
        public static string GetDefaultBitmapFilter()
        {
            return "PNG|*.png|JPG|*.jpg|BMP|*.bmp";
        }
        /// <summary>
        /// 保存图片, 格式由文件名后缀决定, 如果后缀不可用, 则保存png
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="fileName"></param>
        public static bool SaveBitmap(Image bitmap, string fileName)
        {
            if (bitmap == null || string.IsNullOrEmpty(fileName))
            {
                return false;
            }
            try
            {
                System.Drawing.Imaging.ImageFormat format;
                int index;
                if ((index = fileName.LastIndexOf('.')) >= 0)
                {
                    switch (fileName.Substring(index + 1).ToLower())
                    {
                        case "bitmap":
                        case "bmp":
                            format = System.Drawing.Imaging.ImageFormat.Bmp;
                            break;
                        case "jpg":
                            format = System.Drawing.Imaging.ImageFormat.Jpeg;
                            break;
                        case "png":
                        default:
                            format = System.Drawing.Imaging.ImageFormat.Png;
                            break;
                    }
                }
                else
                {
                    format = System.Drawing.Imaging.ImageFormat.Png;
                }
                bitmap.Save(fileName, format);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #region 图片处理
        /// <summary>
        /// 遍历位图像素点, 当不输出
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dealMethod"></param>
        public static void TraverseBitmapPixel(
            System.Drawing.Bitmap input,
            Action<Color> dealMethod)
        {
            if (input == null || input.Width == 0 || input.Height == 0)
            {
                return;
            }

            BitmapData inputData
                = input.LockBits(
                    new Rectangle(0, 0, input.Width, input.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    input.PixelFormat);
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    dealMethod.Invoke(GetPixelColor(x, y, inputData));
                }
            }
            input.UnlockBits(inputData);
        }
        /// <summary>
        /// 遍历位图像素点, 并输出
        /// </summary>
        /// <param name="input"></param>
        /// <param name="dealMethod"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap TraverseCopyBitmapPixel(
            System.Drawing.Bitmap input,
            Func<Color, Color> dealMethod)
        {
            if (input == null || input.Width == 0 || input.Height == 0)
            {
                return new Bitmap(1, 1);
            }


            Bitmap output = new Bitmap(input.Width, input.Height, input.PixelFormat);

            BitmapData inputData
                = input.LockBits(
                    new Rectangle(0, 0, input.Width, input.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    input.PixelFormat);
            BitmapData outputData
                = output.LockBits(
                    new Rectangle(0, 0, output.Width, output.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    input.PixelFormat);
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    Color oldColor = GetPixelColor(x, y, inputData);
                    Color newColor = dealMethod.Invoke(oldColor);
                    SetPixel(x, y, outputData, newColor);
                }
            }
            input.UnlockBits(inputData);
            output.UnlockBits(outputData);

            return output;
        }
        /// <summary>
        /// 遍历位图像素点, 并输出
        /// </summary>
        /// <param name="input"></param>
        /// <param name="samplingSize">采样原图时的采样尺寸大小, [0, 10]</param>
        /// <param name="dealMethod"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap TraverseCopyBitmapPixelGroup(
            System.Drawing.Bitmap input,
            int samplingSize,
            Func<Color[,], Color> dealMethod)
        {
            if (input == null || input.Width == 0 || input.Height == 0)
            {
                return new Bitmap(1, 1);
            }
            // 设置采样尺寸范围在 0~10
            if (samplingSize < 0) samplingSize = 0;
            if (samplingSize > 10) samplingSize = 10;

            Bitmap output = new Bitmap(input.Width, input.Height, input.PixelFormat);

            BitmapData inputData
                = input.LockBits(
                    new Rectangle(0, 0, input.Width, input.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    input.PixelFormat);
            BitmapData outputData
                = output.LockBits(
                    new Rectangle(0, 0, output.Width, output.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    input.PixelFormat);
            Color[,] oldColorGroup = new Color[1 + samplingSize * 2, 1 + samplingSize * 2];
            for (int x = 0; x < input.Width; x++)
            {
                for (int y = 0; y < input.Height; y++)
                {
                    for (int xi = x - samplingSize; xi <= x + samplingSize; xi++)
                    {
                        for (int yi = y - samplingSize; yi <= y + samplingSize; yi++)
                        {
                            oldColorGroup[xi - x + samplingSize, yi - y + samplingSize]
                                = GetPixelColor(xi, yi, inputData);
                        }
                    }
                    Color newColor = dealMethod.Invoke(oldColorGroup);
                    SetPixel(x, y, outputData, newColor);
                }
            }
            input.UnlockBits(inputData);
            output.UnlockBits(outputData);

            return output;
        }

        /// <summary>
        /// 去除位图的纯色边框
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static System.Drawing.Bitmap RemoveSolidColorBorder(
            System.Drawing.Bitmap input)
        {
            if (input == null || input.Width == 0 || input.Height == 0)
            {
                return new Bitmap(1, 1);
            }
            // 剪裁距离
            int left = 0;
            int right = 0;
            int top = 0;
            int bottom = 0;

            // 获取剪裁区域
            BitmapData inputData
                = input.LockBits(
                    new Rectangle(0, 0, input.Width, input.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    input.PixelFormat);
            Color firstColor = GetPixelColor(0, 0, inputData);
            // 左侧
            for (int x = 0; x < input.Width; x++)
            {
                bool haveDifferentColor = false;
                for (int y = 0; y < input.Height; y++)
                {
                    Color color = GetPixelColor(x, y, inputData);
                    if (color != firstColor)
                    {
                        haveDifferentColor = true;
                        break;
                    }
                }
                if (haveDifferentColor)
                {
                    break;
                }
                else
                {
                    left++;
                    if (left + right == input.Width)
                    {
                        input.UnlockBits(inputData);
                        return new Bitmap(1, 1);
                    }
                }
            }
            // 右侧
            for (int x = input.Width - 1; x >= 0; x--)
            {
                bool haveDifferentColor = false;
                for (int y = 0; y < input.Height; y++)
                {
                    Color color = GetPixelColor(x, y, inputData);
                    if (color != firstColor)
                    {
                        haveDifferentColor = true;
                        break;
                    }
                }
                if (haveDifferentColor)
                {
                    break;
                }
                else
                {
                    right++;
                    if (left + right == input.Width)
                    {
                        input.UnlockBits(inputData);
                        return new Bitmap(1, 1);
                    }
                }
            }
            // 上侧
            for (int y = 0; y < input.Height; y++)
            {
                bool haveDifferentColor = false;
                for (int x = 0; x < input.Width; x++)
                {
                    Color color = GetPixelColor(x, y, inputData);
                    if (color != firstColor)
                    {
                        haveDifferentColor = true;
                        break;
                    }
                }
                if (haveDifferentColor)
                {
                    break;
                }
                else
                {
                    top++;
                    if (top + bottom == input.Height)
                    {
                        input.UnlockBits(inputData);
                        return new Bitmap(1, 1);
                    }
                }
            }
            // 下侧
            for (int y = input.Height - 1; y >= 0; y--)
            {
                bool haveDifferentColor = false;
                for (int x = 0; x < input.Width; x++)
                {
                    Color color = GetPixelColor(x, y, inputData);
                    if (color != firstColor)
                    {
                        haveDifferentColor = true;
                        break;
                    }
                }
                if (haveDifferentColor)
                {
                    break;
                }
                else
                {
                    bottom++;
                    if (top + bottom == input.Height)
                    {
                        input.UnlockBits(inputData);
                        return new Bitmap(1, 1);
                    }
                }
            }

            Bitmap output = new Bitmap(
                input.Width - left - right, input.Height - top - bottom, input.PixelFormat);
            BitmapData outputData
                = output.LockBits(
                    new Rectangle(0, 0, output.Width, output.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadWrite,
                    input.PixelFormat);
            for (int x = 0; x < output.Width; x++)
            {
                for (int y = 0; y < output.Height; y++)
                {
                    SetPixel(x, y, outputData, GetPixelColor(left + x, top + y, inputData));
                }
            }

            input.UnlockBits(inputData);
            output.UnlockBits(outputData);

            return output;
        }

        #region 私有方法
        /// <summary>
        /// 取得像素点颜色
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        private static Color GetPixelColor(int x, int y, BitmapData data)
        {
            if (x < 0 || y < 0 || x >= data.Width || y >= data.Height)
            {
                return Color.FromArgb(255, 128, 128, 128);
            }
            if (data.PixelFormat == PixelFormat.Format32bppRgb ||
                data.PixelFormat == PixelFormat.Format32bppArgb)
            {
                byte[] bs = new byte[4];
                System.Runtime.InteropServices.Marshal.Copy(
                    data.Scan0 + y * data.Stride + x * 4,
                    bs, 0, 4);
                return Color.FromArgb(bs[3], bs[2], bs[1], bs[0]);
            }
            if (data.PixelFormat == PixelFormat.Format24bppRgb)
            {
                byte[] bs = new byte[3];
                System.Runtime.InteropServices.Marshal.Copy(
                    data.Scan0 + y * data.Stride + x * 3,
                    bs, 0, 3);
                return Color.FromArgb(bs[2], bs[1], bs[0]);
            }
            return Color.Empty;
        }
        /// <summary>
        /// 设置像素点颜色值
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="data"></param>
        /// <param name="color"></param>
        private static void SetPixel(int x, int y, BitmapData data, Color color)
        {
            if (data.PixelFormat == PixelFormat.Format32bppRgb ||
                data.PixelFormat == PixelFormat.Format32bppArgb)
            {
                byte[] bs = new byte[4];
                bs[3] = color.A;
                bs[2] = color.R;
                bs[1] = color.G;
                bs[0] = color.B;
                System.Runtime.InteropServices.Marshal.Copy(
                    bs, 0, data.Scan0 + y * data.Stride + x * 4, 4);
            }
            if (data.PixelFormat == PixelFormat.Format24bppRgb)
            {
                byte[] bs = new byte[3];
                bs[2] = color.R;
                bs[1] = color.G;
                bs[0] = color.B;
                System.Runtime.InteropServices.Marshal.Copy(
                    bs, 0, data.Scan0 + y * data.Stride + x * 3, 3);
            }
        }

        #endregion
        #endregion
    }

}
