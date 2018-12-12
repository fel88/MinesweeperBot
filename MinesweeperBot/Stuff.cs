using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using OpenCvSharp.Extensions;

namespace MinesweeperBot
{
    public static class Stuff
    {
        public static Bitmap ToBitmapAnyWay(object obj, bool nullable = false)
        {
            if (obj is Bitmap)
            {
                return obj as Bitmap;
            }
            if (obj is Mat)
            {
                return BitmapConverter.ToBitmap(obj as Mat);
            }
            if (nullable)
            {
                return null;
            }
            throw new ArgumentException("unable to cast " + obj.GetType().Name + " to bitmap");
        }
        public static Mat ToMatAnyWay(object obj, bool nullable = false)
        {
            if (obj is Mat)
            {
                return obj as Mat;
            }
            if (obj is Bitmap)
            {
                return BitmapConverter.ToMat((Bitmap)(obj as Bitmap).Clone());
            }
            if (nullable)
            {
                return null;
            }
            throw new ArgumentException("unable to cast " + obj.GetType().Name + " to mat");
        }

    }
}
