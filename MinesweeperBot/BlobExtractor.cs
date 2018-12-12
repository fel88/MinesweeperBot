using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MinesweeperBot
{
    public class BlobExtractor
    {
        
        public BlobObject[] Output0;
        
        public BlobVerticalItem[] Output1;
        public List<BlobVerticalItem>[] clrvites = new List<BlobVerticalItem>[3000];
         
        public bool InsideInterval(int max, int min, int val, bool withEnds = true)
        {
            if (withEnds)
            {
                return val >= min && val <= max;
            }
            return val > min && val < max;
        }
        bool useParallel = false;

        public void Process()
        {
            DateTime dt = DateTime.Now;
            List<int> ddts = new List<int>();

            var inv = Inv;
            byte[,] net = Net;

            List<BlobVerticalItem>[] bbv = new List<BlobVerticalItem>[net.GetLength(0)+1];
            List<BlobVerticalItem> allv = new List<BlobVerticalItem>();
            int total = 0;
            



            #region  linear vesrion

            if (!useParallel)
            {
                for (short i = 0; i < net.GetLength(0); i++)
                {
                    int last = inv ? 255 : 0;
                    BlobVerticalItem b = new BlobVerticalItem() { X = i };
                    bool start = true;
                    for (short j = 0; j < net.GetLength(1); j++)
                    {
                        if (last != net[i, j])
                        {
                            if (start)
                            {
                                b.StartY = j;
                                start = false;
                            }
                            else
                            {
                                b.EndY = j;
                                start = true;
                                if (bbv[i] == null)
                                {
                                    bbv[i] = new List<BlobVerticalItem>();
                                }
                                bbv[i].Add(b);
                                allv.Add(b);
                                total++;
                                b = new BlobVerticalItem() { X = i };
                            }
                        }
                        last = net[i, j];
                    }
                    if (!start)
                    {
                        b.EndY = (short)(net.GetLength(1) - 1);

                        if (bbv[i] == null)
                        {
                            bbv[i] = new List<BlobVerticalItem>();
                        }
                        if (b.StartY != b.EndY)
                        {
                            bbv[i].Add(b);
                            allv.Add(b);
                            total++;
                        }

                    }
                }
            }



            #endregion

            if (useParallel)
            {
                int zlen = net.GetLength(0);
                Parallel.For(0, zlen, i =>
                {
                    int last = inv ? 255 : 0;
                    BlobVerticalItem b = new BlobVerticalItem() { X = (short)i };
                    bool start = true;
                    for (short j = 0; j < net.GetLength(1); j++)
                    {
                        if (last != net[i, j])
                        {
                            if (start)
                            {
                                b.StartY = j;
                                start = false;
                            }
                            else
                            {
                                b.EndY = j;
                                start = true;

                                if (bbv[i] == null)
                                {
                                    bbv[i] = new List<BlobVerticalItem>();
                                }

                                bbv[i].Add(b);

                                lock (allv)
                                {
                                    allv.Add(b);
                                }
                                total++;
                                b = new BlobVerticalItem() { X = (short)i };
                            }
                        }
                        last = net[i, j];
                    }
                    if (!start)
                    {
                        b.EndY = (short)(net.GetLength(1) - 1);

                        if (bbv[i] == null)
                        {
                            bbv[i] = new List<BlobVerticalItem>();
                        }
                        if (b.StartY != b.EndY)
                        {
                            bbv[i].Add(b);
                            lock (allv)
                            {
                                allv.Add(b);
                            }
                            total++;
                        }

                    }

                });

            }
            ddts.Add((int)DateTime.Now.Subtract(dt).TotalMilliseconds);
            dt = DateTime.Now;
            Output1 = allv.ToArray();
            
            short newClrIndx = 1;
            Dictionary<int, List<BlobVerticalItem>> vvd = new Dictionary<int, List<BlobVerticalItem>>();
            Dictionary<int, List<int>> smc = new Dictionary<int, List<int>>();
            

            #region parallel version

            int len = bbv.Length;
            Stopwatch sw = new Stopwatch();
            sw.Start();

            
            sw.Stop();
            var ms = sw.Elapsed.TotalMilliseconds;
            Parallel.For(0, len, i =>
            {
                if (bbv[i] == null || bbv[i + 1] == null) return;
                for (int j = 0; j < bbv[i].Count; j++)
                {
                    for (int k = 0; k < bbv[i + 1].Count; k++)
                    {
                        //check connection
                        var s2 = bbv[i + 1][k].StartY;
                        var e2 = bbv[i + 1][k].EndY;
                        var s1 = bbv[i][j].StartY;
                        var e1 = bbv[i][j].EndY;
                        

                        if (
                            InsideInterval(e2, s2, (e1 - s1) / 2 + s1, IntervalsWithEnds) ||//center point interval1
                            InsideInterval(e1, s1, (e2 - s2) / 2 + s2, IntervalsWithEnds) ||//center point interval 2
                            InsideInterval(e2, s2, s1, IntervalsWithEnds) || InsideInterval(e2, s2, e1, IntervalsWithEnds) || InsideInterval(e1, s1, e2, IntervalsWithEnds) || InsideInterval(e1, s1, s2, IntervalsWithEnds))
                        {
                            if (bbv[i][j].Color == null)
                            {
                                bbv[i][j].Color = newClrIndx++;

                            }
                            if (bbv[i + 1][k].Color == null)
                            {
                                bbv[i + 1][k].Color = bbv[i][j].Color;
                            }
                         
                            bbv[i][j].Childs.Add(bbv[i + 1][k]);
                            bbv[i + 1][k].Parents.Add(bbv[i][j]);

                        }
                    }
                }

            });
            #endregion
            List<BlobTempObject> tt = new List<BlobTempObject>();

            
            ddts.Add((int)DateTime.Now.Subtract(dt).TotalMilliseconds);
            dt = DateTime.Now;
            
            int unpcntr = allv.Count(z => !z.IsProcessed);
            int cntr = 0;


            
            List<BlobVerticalItem> lastv = new List<BlobVerticalItem>();
            lastv.AddRange(allv);
            while (unpcntr > 0)
            {
                Queue<BlobVerticalItem> qq = new Queue<BlobVerticalItem>();
                
                qq.Enqueue(lastv.First());
                tt.Add(new BlobTempObject());
              
                while (qq.Count > 0)
                {
                    var deq = qq.Dequeue();
                    if (deq.IsProcessed)
                    {
                        cntr++;
                        continue;
                    }
                    deq.IsProcessed = true;
                    lastv.Remove(deq);
                    unpcntr--;
                    tt.Last().Verticals.Add(deq);
                    
                    foreach (var blobVerticalItem in deq.Childs)
                    {
                        if (blobVerticalItem.IsProcessed) continue;
                        qq.Enqueue(blobVerticalItem);
                    }
                    foreach (var blobVerticalItem in deq.Parents)
                    {
                        if (blobVerticalItem.IsProcessed) continue;
                        qq.Enqueue(blobVerticalItem);
                    }
                }
            }
            ddts.Add((int)DateTime.Now.Subtract(dt).TotalMilliseconds);
            dt = DateTime.Now;

            
            var kx2 = CellSize;
            List<BlobObject> rett = new List<BlobObject>();
            BlobObject.NewId = 0;

            foreach (var blobTempObject in tt)
            {
                var yy = blobTempObject.Verticals.Sum(x => x.X) / blobTempObject.Verticals.Count;
                var maxy = blobTempObject.Verticals.Max(x => x.X);
                var miny = blobTempObject.Verticals.Min(x => x.X);
                var maxx = blobTempObject.Verticals.Max(x => x.EndY);
                var minx = blobTempObject.Verticals.Min(x => x.StartY);
                var xx = (int)blobTempObject.Verticals.Select(x => (x.EndY - x.StartY) / 2 + x.StartY).Average();

                rett.Add(new BlobObject()
                {
                    Id = BlobObject.NewId++,
                    Position = new Point(xx * kx2, yy * kx2),
                    Size = new float[] { Math.Abs(maxx - minx) * kx2, Math.Abs(maxy - miny) * kx2 },
                    Verticals = blobTempObject.Verticals.ToArray(),
                    Bound = new Rectangle(minx, miny, maxx - minx, maxy - miny)
                });

                rett.Last().H = (int)rett.Last().Size[1];
                rett.Last().W = (int)rett.Last().Size[0];

                rett.Last().X = (int)rett.Last().Position.X - rett.Last().W / 2;
                rett.Last().Y = (int)rett.Last().Position.Y - rett.Last().H / 2;

                rett.Last().CenterX = (int)rett.Last().Position.X;
                rett.Last().CenterY = (int)rett.Last().Position.Y;

                
            }
            Output0 = rett.Where(z => z.Size.All(u => u > 0)).ToArray();
            ddts.Add((int)DateTime.Now.Subtract(dt).TotalMilliseconds);
            dt = DateTime.Now;
        }        

        public byte[,] Net;
        public bool Inv;
        
        
        public bool IntervalsWithEnds { get; set; }
        public int DisPoint(Point p1, Point p2)
        {
            return Math.Abs(p1.X - p2.X) + Math.Abs(p1.Y - p2.Y);
        }


        public bool Invert = false;
        public int CellSize = 5;

    }

}
