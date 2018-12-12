using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MinesweeperBot
{
    public class Bot
    {
        public int Rows { get; set; }
        public int Cols { get; set; }

        public int ColorDist(Color color1, Color color2)
        {
            int dist = 0;
            dist += Math.Abs(color1.R - color2.R);
            dist += Math.Abs(color1.G - color2.G);
            dist += Math.Abs(color1.B - color2.B);
            return dist;
        }

        public BlobObject[,] BBlobs = null;
        public int[,] Extracted = null;
        public int[,] Extract(BlobObject[] blobs, BlobObject[] blobsAll)
        {

            if (blobs == null || blobs.Count() < (Rows * Cols))
            {
                return null;
            }
            int[,] bret = new int[Rows, Cols];

            List<BlobObject> exc = new List<BlobObject>();
            BlobObject fr = null;
            exc.AddRange(blobs);
            int yy = 0;
            BlobObject[,] bblobs = new BlobObject[Rows, Cols];
            while (exc.Count > 0)
            {
                fr = exc.OrderBy(z => z.Y).First();
                var row0 = exc.Where(z => Math.Abs(z.Y - fr.Y) < 5);
                if (row0.Count() < Rows)
                {
                    exc = exc.Except(row0).ToList();
                    continue;
                }
                exc = exc.Except(row0).ToList();

                int xx = 0;
                foreach (var blobObject in row0.OrderBy(z => z.X))
                {


                    var insiders = blobsAll.Where(z => blobObject.Bound.Value.Contains(z.Bound.Value)).ToArray();

                    var ps = insiders.Except(new BlobObject[] { blobObject }).OrderByDescending(z => z.PointSum).FirstOrDefault();



                    bblobs[xx, yy] = blobObject;

                    if (Math.Abs(blobObject.PointSum - 144) < 5)//unexplored
                    {
                        bret[xx, yy] = -1;
                    }
                    if (Math.Abs(blobObject.PointSum - 105) < 5)//mine flag
                    {
                        bret[xx, yy] = -2;
                    }
                    if (ps != null && Math.Abs(blobObject.PointSum - 186) < 3 && ColorDist(ps.Color, Color.Blue) < 35)//1
                    {
                        bret[xx, yy] = 1;
                    }
                    if (ps != null && Math.Abs(blobObject.PointSum - 161) < 3 && ColorDist(ps.Color, Color.Green) < 35)//2
                    {
                        bret[xx, yy] = 2;
                    }
                    if (ps != null && Math.Abs(blobObject.PointSum - 164) < 5 && ColorDist(ps.Color, Color.Red) < 35)//3
                    {
                        bret[xx, yy] = 3;
                    }

                    if (ps != null && ColorDist(ps.Color, Color.DarkBlue) < 35)//4
                    {
                        bret[xx, yy] = 4;
                    }
                    if (ps != null && ColorDist(ps.Color, Color.DarkRed) < 35)//5
                    {
                        bret[xx, yy] = 5;
                    }
                    if (ps != null && ColorDist(ps.Color, Color.DarkCyan) < 35)//6
                    {
                        bret[xx, yy] = 6;
                    }

                    if (Math.Abs(blobObject.PointSum - 225) < 5)//empty 0 mines around
                    {
                        bret[xx, yy] = 0;
                    }
                    xx++;

                }
                yy++;
            }
            Extracted = bret;
            BBlobs = bblobs;
            return bret;

        }

        private Random r = new Random();

        public bool wasAcceptation = true;
        public Point acceptPoint;
        public int accepState;
        public bool AllowMoves { get; set; } = true;
        public bool UseSleep { get; set; }
        public int Sleep { get; set; }
        public SimpleGUI Manager = new SimpleGUI();

        public bool AllowRandomMoves { get; set; } = true;
        public bool StopEveryTurn { get; set; }


        public Point WindowPosition;
        public void MakeMove(int[,] mines, BlobObject[,] bblobs)
        {

            ////stage 2 . solving
            if (mines == null) return;
            var mngr = Manager;

            Point cnt = new Point(100, 100);
            int[] pos = new int[] { WindowPosition.X, WindowPosition.Y };

            Point start = new Point(cnt.X + pos[0], cnt.Y + pos[1]);
            Point end = new Point(cnt.X + pos[0], cnt.Y + pos[1]);

            int[,] unexpa = new int[mines.GetLength(0), mines.GetLength(1)];

            List<MinesweeperConstraint>[,] constrs = new List<MinesweeperConstraint>[mines.GetLength(0), mines.GetLength(1)];

            //find moves
            List<MinesweeperMove> moves = new List<MinesweeperMove>();
            for (int i = 0; i < mines.GetLength(0); i++)
            {
                for (int j = 0; j < mines.GetLength(1); j++)
                {
                    if (mines[i, j] < 0) continue;

                    //get surrond

                    List<Point> cmines = new List<Point>();
                    List<Point> unexp = new List<Point>();
                    for (int k = -1; k <= 1; k++)
                    {
                        for (int l = -1; l <= 1; l++)
                        {
                            int xx = k + i;
                            int yy = l + j;
                            if (xx < 0 || xx > mines.GetUpperBound(0) || yy < 0 || yy > mines.GetUpperBound(1))
                            {
                                continue;
                            }
                            if (mines[xx, yy] == -1) { unexp.Add(new Point(xx, yy)); }
                            if (mines[xx, yy] == -2) { cmines.Add(new Point(xx, yy)); }
                        }
                    }

                    foreach (var point in unexp)
                    {
                        if (constrs[point.X, point.Y] == null)
                        {
                            constrs[point.X, point.Y] = new List<MinesweeperConstraint>();
                        }
                        constrs[point.X, point.Y].Add(new MinesweeperConstraint() { MinesCount = (mines[i, j] - cmines.Count), Points = unexp.ToArray() });

                    }


                    unexpa[i, j] = unexp.Count();

                    if ((mines[i, j] - cmines.Count) == unexp.Count)//mark all unexp as mines
                    {
                        foreach (var point in unexp)
                        {
                            var blob = bblobs[point.X, point.Y];
                            var v = blob.Bound.Value.Center();
                            start = new Point(v.X + pos[0], v.Y + pos[1]);

                            moves.Add(new MinesweeperMove()
                            {
                                CellPositions = new Point[] { point },
                                Positions = new Point[] { new Point(start.X, start.Y) },
                                Type = MinesweeperMoveTypeEnum.SetMineFlag
                            });

                        }
                    }

                    if (unexp.Count > 0 && (mines[i, j] == cmines.Count))//open all unexp 
                    {
                        var blob = bblobs[i, j];
                        var v = blob.Bound.Value.Center();
                        start = new Point(v.X + pos[0], v.Y + pos[1]);


                        moves.Add(new MinesweeperMove()
                        {
                            CellPositions = new Point[] { new Point(i, j) },
                            Positions = new Point[] { start },
                            Type = MinesweeperMoveTypeEnum.AreaDiscover
                        });
                    }
                }
            }

            for (int i = 0; i < mines.GetLength(0); i++)
            {
                for (int j = 0; j < mines.GetLength(1); j++)
                {
                    if (constrs[i, j] != null && constrs[i, j].Count > 1)
                    {
                        //full check  
                        var cc = constrs[i, j].ToArray();

                        var v = cc.OrderByDescending(z => z.Points.Count()).ToArray();
                        var cc2 = cc.Where(z => z.Points.Count() == v[0].Points.Count());
                        foreach (var minesweeperConstraint in cc2)
                        {
                            var tt = cc.Where(u => u.Points.Count() < minesweeperConstraint.Points.Count()).ToArray();
                            foreach (var constraint in tt)
                            {
                                if (minesweeperConstraint.MinesCount == constraint.MinesCount)
                                {
                                    int[,] map = new int[mines.GetLength(0), mines.GetLength(1)];
                                    foreach (var pn in minesweeperConstraint.Points)
                                    {
                                        map[pn.X, pn.Y] = 1;
                                    }

                                    if (constraint.Points.All(z => map[z.X, z.Y] == 1))
                                    {
                                        foreach (var pn in constraint.Points)
                                        {
                                            map[pn.X, pn.Y] = 2;
                                        }

                                        var cands = minesweeperConstraint.Points.Where(z => map[z.X, z.Y] == 1).ToArray();
                                        foreach (var point in cands)
                                        {
                                            var blob = bblobs[point.X, point.Y];
                                            var vv = blob.Bound.Value.Center();
                                            start = new Point(vv.X + pos[0], vv.Y + pos[1]);

                                            moves.Add(new MinesweeperMove()
                                            {
                                                Type = MinesweeperMoveTypeEnum.SingleCellDiscover,
                                                CellPositions = new Point[] { point },
                                                Positions = new Point[] { start }

                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!wasAcceptation)
            {
                if (accepState == 0)
                {
                    if (mines[acceptPoint.X, acceptPoint.Y] >= 0)
                    {
                        wasAcceptation = true;
                    }
                }
                if (accepState == 1)//area discover
                {
                    if (unexpa[acceptPoint.X, acceptPoint.Y] == 0)
                    {
                        wasAcceptation = true;
                    }
                }
                if (accepState == -2)
                {
                    if (mines[acceptPoint.X, acceptPoint.Y] == -2)
                    {
                        wasAcceptation = true;
                    }
                }

            }

            int unexpCnt = 0;
            for (int i = 0; i < mines.GetLength(0); i++)
            {
                for (int j = 0; j < mines.GetLength(1); j++)
                {
                    if (mines[i, j] == -1) unexpCnt++;
                }
            }
            if (unexpCnt == mines.GetLength(0) * mines.GetLength(1))
            {
                wasAcceptation = true;
                moves.Clear();

            }

            if (moves.Count == 0 && AllowRandomMoves)
            {

                List<Point> possible = new List<Point>();
                //random move
                int xx = 0;
                int yy = 0;
                for (int i = 0; i < mines.GetLength(0); i++)
                {
                    for (int j = 0; j < mines.GetLength(1); j++)
                    {
                        if (mines[i, j] == -1)
                        {
                            possible.Add(new Point(i, j));
                        }
                    }
                }
                if (possible.Count > 0)
                {
                    var move = possible[r.Next(possible.Count)];
                    var blob = bblobs[move.X, move.Y];
                    var v = blob.Bound.Value.Center();
                    start = new Point(v.X + pos[0], v.Y + pos[1]);

                    moves.Add(new MinesweeperMove()
                    {
                        CellPositions = new Point[] { new Point(move.X, move.Y) },
                        Positions = new Point[] { new Point(start.X, start.Y) },
                        Type = MinesweeperMoveTypeEnum.SingleCellDiscover
                    });
                }

            }



            int[,] hh = new int[mines.GetLength(0), mines.GetLength(1)];
            //make one move
            if (wasAcceptation)
            {

                while (moves.Count > 0)
                {
                    var move = moves.First();
                    if (AllowMoves)
                    {

                        switch (move.Type)
                        {
                            case MinesweeperMoveTypeEnum.AreaDiscover:
                                {
                                    var tempAcceptPoint = move.CellPositions[0];

                                    if (hh[tempAcceptPoint.X, tempAcceptPoint.Y] == 0)
                                    {
                                        MovePosition(mngr, move.Positions[0]);
                                        acceptPoint = move.CellPositions[0];
                                        hh[acceptPoint.X, acceptPoint.Y] = 1;
                                        mngr.LeftDown();
                                        mngr.RightDown();
                                        mngr.LeftUp();
                                        mngr.RightUp();
                                        accepState = 1; //area explored
                                    }
                                }
                                break;
                            case MinesweeperMoveTypeEnum.SingleCellDiscover:
                                {
                                    var tempAcceptPoint = move.CellPositions[0];

                                    if (hh[tempAcceptPoint.X, tempAcceptPoint.Y] == 0)
                                    {
                                        MovePosition(mngr, move.Positions[0]);
                                        acceptPoint = tempAcceptPoint;
                                        hh[acceptPoint.X, acceptPoint.Y] = 1;

                                        mngr.LeftClick();
                                        accepState = 0; //explored
                                    }
                                }
                                break;
                            case MinesweeperMoveTypeEnum.SetMineFlag:
                                {
                                    var tempAcceptPoint = move.CellPositions[0];

                                    if (hh[tempAcceptPoint.X, tempAcceptPoint.Y] == 0)
                                    {
                                        MovePosition(mngr, move.Positions[0]);
                                        mngr.RightClick();
                                        accepState = -2; //mine
                                        acceptPoint = tempAcceptPoint;
                                        hh[acceptPoint.X, acceptPoint.Y] = 1;

                                    }

                                }
                                break;
                        }

                        wasAcceptation = false;
                    }
                    moves.Remove(move);
                    if (UseSleep)
                    {
                        Thread.Sleep(Sleep);
                    }
                }

            }
        }

        public bool UseSmooth = true;
        public int SmoothSleep = 1;
        public void MovePosition(SimpleGUI gui, Point pos)
        {
            if (!UseSmooth)
            {
                gui.SetPosition(pos);
                return;
            }
            var x = gui.GetMouseX();
            var y = gui.GetMouseY();
            var dx = pos.X - x;
            var dy = pos.Y - y;
            float delta = 2;
            while ((Math.Abs(dx) > 0) || Math.Abs(dy) > 0)
            {
                var len = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                var dxx = dx / len;
                var dyy = dy / len;

                dxx *= delta;
                dyy *= delta;
                if (len > delta)
                {
                    var xnew = x + dxx;
                    var ynew = y + dyy;
                    gui.SetPosition(new Point((int)xnew, (int)ynew));
                }
                else
                {

                    gui.SetPosition(pos);
                    break;
                }
                Thread.Sleep(SmoothSleep);
                x = gui.GetMouseX();
                y = gui.GetMouseY();
                dx = pos.X - x;
                dy = pos.Y - y;


            }
        }

        public static Bitmap DrawDesk(int[,] objs)
        {
            int[,] board = (int[,])objs;

            int cw = 25;
            int ch = 25;
            Bitmap bmp = new Bitmap(board.GetLength(0) * cw, board.GetLength(1) * ch);

            var gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            gr.Clear(Color.LightGreen);
            var f = new Font("Consolas", 12);
            for (int i = 0; i < board.GetLength(0); i++)
            {
                for (int j = 0; j < board.GetLength(1); j++)
                {
                    int xx = i * cw;
                    int yy = j * ch;
                    var v = board[i, j];
                    if (v == -2)
                    {
                        gr.FillRectangle(Brushes.Red, xx, yy, cw, ch);
                    }
                    if (v == -1)
                    {
                        gr.FillRectangle(Brushes.DarkGray, xx, yy, cw, ch);
                    }
                    if (v > 0)
                    {
                        var ms = gr.MeasureString(v + "", f);
                        gr.DrawString(v + "", f, Brushes.Blue, xx+cw/2-ms.Width/2, yy+ch/2-ms.Height/2);
                    }
                    gr.DrawRectangle(Pens.Black, xx, yy, cw, ch);


                }

            }


            return bmp;

        }

    }



    public class MinesweeperMove
    {
        public Point[] Positions;
        public Point[] CellPositions;
        public MinesweeperMoveTypeEnum Type;
    }

    public class MinesweeperConstraint
    {
        public int MinesCount;
        public Point[] Points;
    }

    public enum MinesweeperMoveTypeEnum
    {
        AreaDiscover,
        SetMineFlag,
        SingleCellDiscover
    }

    public static class PointHelper
    {
        public static Point Center(this Rectangle rect)
        {
            return new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        }

    }
}
