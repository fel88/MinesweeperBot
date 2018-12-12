using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using ColorConversion = OpenCvSharp.ColorConversionCodes;

namespace MinesweeperBot
{
    public class ManualClusterization
    {
        public List<Color> Colors = new List<Color>();

        public Mat Output0;
        public object ColorsInput;
        public object Bitmap;
        public void Process()
        {
            
            if (Bitmap == null) return;
            if (ColorsInput != null)
            {
                if (ColorsInput is Color[])
                {
                    var cols = ((Color[])ColorsInput).ToArray();
                    Colors.Clear();
                    Colors.AddRange(cols);
                }
                if (ColorsInput is object[])
                {
                    var cols = ((object[])ColorsInput).Where(z => z != null).Select(z => (Color)z).ToArray();
                    Colors.Clear();
                    Colors.AddRange(cols);
                }


            }
            if (Colors.Count == 0)
            {
                return;
            }
            Mat mat = new Mat();
            
            Stopwatch sw2 = new Stopwatch();
            sw2.Start();
            var bmp = Stuff.ToBitmapAnyWay(Bitmap);


            var bytes = BmpToBytes_Unsafe(bmp);
            int bitsPerPixel = ((int)bmp.PixelFormat & 0xff00) >> 8;
            int bytesPerPixel = (bitsPerPixel + 7) / 8;
            int stride = 4 * ((bmp.Width * bytesPerPixel + 3) / 4);


            #region parallel version

            int ww = bmp.Width;
            int hh = bmp.Height;
            Parallel.For(0, hh, i =>
            {
                for (int j = 0; j < ww; j++)
                {
                    Color? nearest = null;
                    float mindiff = 1e6f;
                    int index = i * stride + j * bytesPerPixel;

                    foreach (var color in Colors)
                    {
                        float diff = 0;
                        
                        diff += Math.Abs(color.R - bytes[index + 2]);
                        diff += Math.Abs(color.G - bytes[index + 1]);
                        diff += Math.Abs(color.B - bytes[index]);
                        if (diff < mindiff)
                        {
                            mindiff = diff;
                            nearest = color;
                        }


                    }


                    bytes[index] = nearest.Value.B;
                    bytes[index + 1] = nearest.Value.G;
                    bytes[index + 2] = nearest.Value.R;
                }

            });

            #endregion

            var retb = BytesToBmp(bytes, new Size(bmp.Width, bmp.Height), bmp.PixelFormat);

            var retm = Stuff.ToMatAnyWay(retb);
            if (retm.Channels() == 4)
            {
                retm = retm.CvtColor(ColorConversion.RGBA2RGB);
            }
            Output0 = retm;

            sw2.Stop();
            var wel1 = sw2.Elapsed;
            var welt1 = sw2.ElapsedTicks;


        }

        public static unsafe Bitmap BytesToBmp(byte[] bmpBytes, Size imageSize, PixelFormat format)
        {
            Bitmap bmp = new Bitmap(imageSize.Width, imageSize.Height);

            BitmapData bData = bmp.LockBits(new Rectangle(new Point(), bmp.Size),
                ImageLockMode.WriteOnly,                
                format);

            
            Marshal.Copy(bmpBytes, 0, bData.Scan0, bmpBytes.Length);
            bmp.UnlockBits(bData);
            return bmp;
        }

        public static byte[] BmpToBytes_Unsafe(Bitmap bmp)
        {
            BitmapData bData = bmp.LockBits(new Rectangle(new Point(), bmp.Size),
                ImageLockMode.ReadOnly,
                bmp.PixelFormat);            
            int byteCount = bData.Stride * bmp.Height;
            byte[] bmpBytes = new byte[byteCount];
            
            Marshal.Copy(bData.Scan0, bmpBytes, 0, byteCount);
                        
            bmp.UnlockBits(bData);

            return bmpBytes;
        }
    }
}
