using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinesweeperBot
{
    public class BlobFilter
    {

        public float FillKoefFilterMin = 0.5f;

        public BlobObject[] Filter(BlobObject[] output)
        {
            List<BlobObject> ret = new List<BlobObject>();
            //filters here

            foreach (var item in output)
            {
                ret.Add(item);
            }


   
            int minPointSum = 100;
            ret = ret.Where(z => z.PointSum >= minPointSum).ToList();
            ret = ret.Where(z => z.Bound.Value.Width <= 30).ToList();
            return ret.ToArray();
        }
    }
}
