using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MinesweeperBot
{
    [Serializable]
    public class BlobObject 
    {
        public BlobObject()
        {
            
            Bound = null;
        }


        public IVertical[] Verticals { get; set; }
         
  
        public int? _pointSum = null;
        public int PointSum
        {
            get
            {
                if (_pointSum != null)
                {
                    return _pointSum.Value;
                }

                _pointSum = (Verticals.Sum(z => z.EndY - z.StartY));
                return _pointSum.Value;

            }
        }
        public override string ToString()
        {
            return "ct:" + CheckText + "; text:" + Text;
        }
        public static int NewId { get; set; }
        public int Id { get; set; }
        public Point Position;
        public float[] Size;
        public int CenterX { get; set; }
        public int CenterY { get; set; }

        public int H { get; set; }
        public int W { get; set; }
        public int X { get; set; }
        public int Y { get; set; }



        public Color Color { get; set; }
        public string Text { get; set; }
        public object Tag { get; set; }
   
        public string CheckText { get; set; }
        public int Area
        {
            get { return W * H; }
        }
        
        
        public Rectangle? Bound { get; set; }
    }
    public interface IVertical
    {
        uint Index { get; set; }
        short X { get; set; }
        short StartY { get; set; }
        short EndY { get; set; }
    }
}
