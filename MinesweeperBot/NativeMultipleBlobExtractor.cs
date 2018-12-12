using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;

namespace MinesweeperBot
{
    public class NativeMultipleBlobExtractor
    {


                

        public bool UseParallel { get; set; }
        public Color[] Colors;

        public bool Enabled = true;
        public static Bitmap GetColorSlice(object image, Color color, int treshold = 15)
        {
            Mat img = new Mat();
            img = Stuff.ToMatAnyWay(image);

            Cv2.InRange(img, new Scalar(color.B - treshold, color.G - treshold, color.R - treshold), new Scalar(color.B + treshold, color.G + treshold, color.R + treshold), img);

            var bm = BitmapConverter.ToBitmap(img);
            return bm;

        }
        public ImageZipQuantificator zip;

        public object Image;
        public BlobExtractor be;
        public void Process()
        {

            if (Colors == null) return;

            var colors = Colors;
            var mat = Stuff.ToMatAnyWay(Image, true);
            if (mat == null) return;
            List<BlobObject> ret = new List<BlobObject>();

            Stopwatch sw = new Stopwatch();
            if (UseParallel)
            {
                Parallel.ForEach(colors, color =>
                {

                    zip = new MinesweeperBot.ImageZipQuantificator();
                    zip.Image = GetColorSlice(mat.Clone(), color);
                    zip.CellSize = 1;
                    zip.Treshold = 0.5f;
                    zip.Process();


                    be = new MinesweeperBot.BlobExtractor();
                    
                    be.Net = zip.Output1;
                    be.CellSize = 1;
                    be.Inv = false;
                    be.Process();


                    var aa = (BlobObject[])be.Output0;
                    foreach (var blobObject in aa)
                    {                        
                        blobObject.Color = color;
                    }
                    lock (ret)
                    {
                        ret.AddRange(aa);
                    }

                });
            }
            else
            {
                foreach (var color in colors)
                {
                    zip = new MinesweeperBot.ImageZipQuantificator();
                    zip.IsBitmapOutputEnable = true;

                    zip.Image = GetColorSlice(mat.Clone(), color);
                    zip.CellSize = 1;
                    zip.Treshold = 0.5f;
                    zip.Process();


                    be = new BlobExtractor();
                    
                    be.Net = zip.Output1;
                    be.CellSize= 1;
                    be.Inv = false;

                    be.Process();

                    var aa = (BlobObject[])be.Output0;
                    foreach (var blobObject in aa)
                    {                        
                        blobObject.Color = color;
                    }
                    lock (ret)
                    {
                        ret.AddRange(aa);
                    }

                }
            }


            foreach (var blobObject in ret)
            {
                blobObject.Id = NewObjectId++;
            }

            Output = ret.ToArray();

        }
        public BlobObject[] Output;

        public static int NewObjectId;
    }



    public class BlobTempObject
    {        
        public List<BlobVerticalItem> Verticals = new List<BlobVerticalItem>();        
        public bool IsVerticalMode = true;
    }

    public class BlobVerticalItem : BlobVerticalItemLite
    {

        public bool IsProcessed;

        public List<BlobVerticalItem> Childs = new List<BlobVerticalItem>();
        public List<BlobVerticalItem> Parents = new List<BlobVerticalItem>();

        public short? Color;
    }

    public class BlobVerticalItemLite : IVertical
    {
        public uint Index { get; set; }
        public short X { get; set; }
        public short StartY { get; set; }
        public short EndY { get; set; }

    }

}
