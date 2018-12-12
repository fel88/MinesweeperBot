using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MinesweeperBot
{
    public class ImageZipQuantificator
    {




        public Bitmap Output0;

               

        
        public bool IsBitmapOutputEnable { get; set; }
      
        public unsafe static byte[,,] BitmapToByteRgb(Bitmap bmp)
        {
            int width = bmp.Width,
                height = bmp.Height;
            byte[,,] res = new byte[3, height, width];
            BitmapData bd = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly,
                PixelFormat.Format24bppRgb);
            try
            {
                byte* curpos;
                for (int h = 0; h < height; h++)
                {
                    curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                    for (int w = 0; w < width; w++)
                    {
                        res[2, h, w] = *(curpos++);
                        res[1, h, w] = *(curpos++);
                        res[0, h, w] = *(curpos++);                      
                    }
                }
            }
            finally
            {
                bmp.UnlockBits(bd);
            }
            return res;
        }

        public byte[,] Output1;
        public object Image;
        public  void Process()
        {
            
            
            var csize = CellSize;
            var treshold = Treshold;

            Mat mat = null;
            
            Bitmap bmp = null;

            if (Image is Bitmap)
            {

                bmp = (Bitmap)Image;

            }
            if (Image is Mat)
            {
                bmp = BitmapConverter.ToBitmap(Image as Mat);
            }
            if (bmp == null)
            {
                Output0 = null;
                return;
            }
            
            var bytes = BitmapToByteRgb(bmp);
            int kx = CellSize;

            byte[,] net = new byte[bmp.Height, bmp.Width];
            for (int i = 0; i < bmp.Width - kx; i += kx)
            {
                for (int j = 0; j < bmp.Height - kx; j += kx)
                {
                    int cnt = 0;
                    for (int k = 0; k < kx; k++)
                    {
                        for (int l = 0; l < kx; l++)
                        {
                            int xx = j + l;
                            int yy = i + k;
                            var px = bytes[0, j + l, i + k];
                            
                            if (px == 255)
                            {
                                cnt++;
                            }
                        }
                    }
                    if (cnt > (treshold * kx * kx))
                    {
                        net[j / kx, i / kx] = 255;
                    }
                }
            }

        

            Output1 = net;
            if (!IsBitmapOutputEnable) return;

            Bitmap bb = new Bitmap(bmp.Width, bmp.Height);
            if (Use1KxToOutputImage)
            {
                bb = new Bitmap(bmp.Width / kx, bmp.Height / kx);
            }
            var gr = Graphics.FromImage(bb);
            gr.Clear(Color.Black);

            for (int i = 0; i < net.GetLength(0); i++)
            {
                for (int j = 0; j < net.GetLength(1); j++)
                {
                    if (net[i, j] == 255)
                    {
                        if (Use1KxToOutputImage)
                        {
                            gr.FillRectangle(Brushes.White, j, i, 1, 1);

                        }
                        else
                        {
                            gr.FillRectangle(Brushes.White, j * kx, i * kx, kx, kx);

                        }
                    }
                }
            }

            Output0 = bb;

        }

        public bool Use1KxToOutputImage { get; set; }

        public int DisPoint(System.Drawing.Point p1, System.Drawing.Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);            
        }

        public float Treshold = 0.5f;
        public int CellSize = 1;
       
    }
}
