using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Point = System.Drawing.Point;

namespace Edu.CommonLibCore
{
    /// <summary>
    /// 图片操作工具类
    /// <para>author:wangyulong</para>
    /// </summary>
    public class ImageHelper
    {
        #region DllImport

        [System.Runtime.InteropServices.DllImport("gdi32")]
        public static extern bool DeleteObject(IntPtr hObject);

        #endregion

        #region 图片保存

        /// <summary>
        ///  按固定的格式保存图片
        /// </summary>
        /// <param name="img"></param>
        /// <param name="outPath"></param>
        /// <param name="quality">图片质量（0-100） 数值越高，压缩越小</param>
        /// <param name="codeInfo">图片采用的编码信息，因Bitmap不支持8位图的jpg，编码需特殊处理</param>
        public static void SaveImage(Image img, string outPath, int quality, ImageCodecInfo codeInfo = null)
        {
            if (File.Exists(outPath)) File.Delete(outPath);
            FileInfo info = new FileInfo(outPath);
            if (!Directory.Exists(info.Directory.FullName))
                Directory.CreateDirectory(info.Directory.FullName);

            //获取系统编码类型数组,包含了jpeg,bmp,png,gif,tiff
            if (codeInfo == null) codeInfo = GetCodeInfo(outPath);

            EncoderParameters ep = new EncoderParameters(1);
            ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
            img.Save(outPath, codeInfo, ep);
        }


        /// <summary>
        /// 根据扩展名获取图片编码格式
        /// </summary>
        /// <param name="extStr"></param>
        /// <returns></returns>
        public static ImageCodecInfo GetImgCodeInfoByExt(string extStr)
        {
            ImageCodecInfo[] icis = ImageCodecInfo.GetImageEncoders();
            ImageCodecInfo jpgInfo = null;
            foreach (ImageCodecInfo i in icis)
            {
                if (i.MimeType == "image/jpeg") jpgInfo = i;

                if ((".jpg".Equals(extStr) || ".jpeg".Equals(extStr)) && i.MimeType == "image/jpeg")
                    return i;
                if (extStr == ".png" && i.MimeType == "image/png")
                    return i;
                if (extStr == ".bmp" && i.MimeType == "image/bmp")
                    return i;
            }

            //非标准编码，使用Jpg编码
            return jpgInfo;
        }


        /// <summary>
        /// 提取图片的缩略图
        /// </summary>
        /// <param name="newFilePath"></param>
        /// <param name="thumbPath"></param>
        /// <param name="maxWidth"></param>
        /// <param name="maxHeight"></param>
        /// <returns></returns>
        public static bool SaveThumbImg(string newFilePath, string thumbPath, int maxWidth, int maxHeight)
        {
            try
            {
                using (Image di = Image.FromFile(newFilePath))
                {
                    if (di.Width > maxWidth || di.Height > maxHeight)
                    {
                        var width = 0; var height = 0;
                        if (di.Width >= di.Height)
                        {
                            width = maxWidth; height = (int)((width + 0.01) / di.Width * di.Height);
                        }
                        else
                        {
                            height = maxHeight; width = (int)((height + 0.01) / di.Height * di.Width);
                        }
                        Bitmap thumbImage = new Bitmap(width, height);
                        Graphics g = Graphics.FromImage(thumbImage);
                        g.DrawImage(di, new Rectangle(0, 0, width, height));
                        ImageHelper.SaveImage(thumbImage, thumbPath, 100);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// 获取支持的图片扩展名
        /// </summary>
        /// <returns></returns>
        public static List<string> GetCommImgExts()
        {
            return new List<string>() { ".jpg", ".png", ".bmp", ".jpeg" };
        }

        /// <summary>
        /// 根据本地文件创建Bitmap
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Bitmap CreateBitmap(string path)
        {
            try
            {
                Bitmap bp = null;
                if (GetCommImgExts().Contains(Path.GetExtension(path)))
                {
                    bp = new Bitmap(path);
                }
                else
                {
                    /*  using (MagickImage image = new MagickImage(path))
                      {
                          IExifProfile profile = image.GetExifProfile();
                          MemoryStream stream = new MemoryStream();
                          image.Write(stream, MagickFormat.Bmp);
                          bp = new Bitmap(stream);
                          stream.Close();
                      }*/
                }
                return bp;
            }
            catch (Exception ex)
            {
                Bitmap bp = null;
                /* using (MagickImage image = new MagickImage(path))
                 {
                     IExifProfile profile = image.GetExifProfile();
                     MemoryStream stream = new MemoryStream();
                     image.Write(stream, MagickFormat.Bmp);
                     bp = new Bitmap(stream);
                     stream.Close();
                 }*/

                return bp;
            }

            return null;
        }


        #region 图片切割


        /// <summary>
        /// 切割图片
        /// </summary>
        /// <param name="imgPath">原始图片</param>
        /// <param name="startX">切割起始X</param>
        /// <param name="startY">切割起始Y</param>
        /// <param name="width">切割图片宽度</param>
        /// <param name="height">切割图片高度</param>
        /// <param name="outPath"></param>
        /// <returns></returns>
        public static bool SplitPic(string imgPath, int startX, int startY, int width, int height, string outPath)
        {
            Bitmap initImage = CreateBitmap(imgPath);

            if (startX >= initImage.Width || startY >= initImage.Height)
                return false;

            //裁剪对象
            Image pickedImage = null;
            Graphics g = null;

            //定位
            Rectangle fromR = new Rectangle(0, 0, 0, 0);//原图裁剪定位
            Rectangle toR = new Rectangle(0, 0, 0, 0);//目标定位

            //裁剪对象实例化
            pickedImage = new Bitmap(width, height);
            g = Graphics.FromImage(pickedImage);

            //裁剪源定位
            fromR.X = startX;
            fromR.Y = startY;
            fromR.Width = width;
            fromR.Height = height;

            //裁剪目标定位
            toR.X = 0;
            toR.Y = 0;
            toR.Width = width;
            toR.Height = height;

            //设置质量
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            //裁剪
            g.DrawImage(initImage, toR, fromR, GraphicsUnit.Pixel);

            pickedImage.Save(outPath);
            g.Dispose(); //Graphics对象必须释放，否则会有内存泄漏
            initImage.Dispose();
            pickedImage.Dispose();

            return true;
        }

        #endregion

        #region BitmapSource构建


        /// <summary>
        /// 转换本地图片到ImageSource
        /// </summary>
        /// <param name="headUrl"></param>
        /// <returns></returns>
        public static BitmapSource ConvertLocalFileToBitmapSource(string localImgPath)
        {
            /*   if (!File.Exists(localImgPath)) return null;
               Bitmap bitMap = ImageHelper.CreateBitmap(localImgPath);
               IntPtr ip = bitMap.GetHbitmap();
               BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bitMap.Width, bitMap.Height));
               DeleteObject(ip);
               bitMap.Dispose();
               return bitmapSource;*/

            return null;
        }

        #endregion


        #region 屏幕截图

        /// <summary>
        /// 截取全屏幕图像
        /// </summary>
        /// <returns>屏幕位图</returns>
        public static Bitmap GetFullScreen()
        {
            Bitmap mimage = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height);
            Graphics gp = Graphics.FromImage(mimage);
            gp.CopyFromScreen(new Point(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y), new Point(0, 0), mimage.Size, CopyPixelOperation.SourceCopy);
            gp.Dispose();
            return mimage;
        }


        /// <summary>
        /// 截取全屏幕图像
        /// </summary>
        /// <returns>屏幕位图</returns>
        public static Bitmap GetFullScreenWithBar()
        {
            Bitmap mimage = new Bitmap(System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height);
            Graphics gp = Graphics.FromImage(mimage);
            gp.CopyFromScreen(new Point(System.Windows.Forms.Screen.PrimaryScreen.Bounds.X, System.Windows.Forms.Screen.PrimaryScreen.Bounds.Y), new Point(0, 0), mimage.Size, CopyPixelOperation.SourceCopy);
            gp.Dispose();
            return mimage;
        }

        #endregion

        #region 辅助处理

        //获取图形编码
        static ImageCodecInfo GetCodeInfo(string filePath)
        {
            string fileType = Path.GetExtension(filePath).ToLower();
            return GetImgCodeInfoByExt(fileType);
        }

        #endregion
    }
}
