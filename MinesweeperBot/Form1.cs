using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MinesweeperBot
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Bot.Cols = 16;
            Bot.Rows = 30;
            checkBox5.Checked = Bot.UseSmooth;
            checkBox2.Checked = Bot.AllowMoves;
            checkBox4.Checked = Bot.AllowRandomMoves;
            cc.Clear();
            cc.Add(Color.FromArgb(-4144960));
            cc.Add(Color.FromArgb(-8355712));
            cc.Add(Color.FromArgb(-1));
            cc.Add(Color.FromArgb(-16776961));
            cc.Add(Color.FromArgb(-65536));
            cc.Add(Color.FromArgb(-16744448));
            cc.Add(Color.FromArgb(-16777088));
            cc.Add(Color.FromArgb(-8388608));
            cc.Add(Color.FromArgb(-16744320));
        }



        public void UpdateList()
        {
            listView1.Items.Clear();
            var wnds = User32.FindWindows(delegate (IntPtr wnd, IntPtr param)
            {
                return User32.GetWindowText(wnd).Contains(watermark1.Text);
                return true;
            });
            User32.EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                var txt = User32.GetWindowText(wnd);

                if (!string.IsNullOrEmpty(txt) && txt.ToUpper().Contains(watermark1.Text.ToUpper()))
                {
                    listView1.Items.Add(new ListViewItem(new string[] { txt, wnd.ToString() }) { Tag = wnd });
                }
                return true;
            }, IntPtr.Zero);
        }

        private void watermark1_TextChanged(object sender, EventArgs e)
        {
            UpdateList();
        }
        IntPtr hwn;
        Bitmap bmpScreenshot;
        Graphics gfxScreenshot;
        RECT rect;
        int cntr = 0;
        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {

                IntPtr wnd = (IntPtr)listView1.SelectedItems[0].Tag;
                if (pictureBox1.Image != null)
                {
                    pictureBox1.Image.Dispose();
                }

                pictureBox1.Image = User32.CaptureImage(wnd);

                hwn = wnd;
                label1.Text = "Handle: " + wnd.ToString();

                User32.GetWindowRect(hwn, out rect);

                if (bmpScreenshot != null)
                {
                    bmpScreenshot.Dispose();
                }
                if (gfxScreenshot != null)
                {
                    gfxScreenshot.Dispose();
                }
                bmpScreenshot = new Bitmap((rect.Width / 2 + 1) * 2, (rect.Height / 2 + 1) * 2, PixelFormat.Format24bppRgb);
                gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RECT _rect;
            User32.GetWindowRect(hwn, out _rect);
            label1.BackColor = Color.Green;
            label1.ForeColor = Color.White;
            if (!User32.IsWindow(hwn))
            {
                label1.Text = "Incorrect window";
                label1.BackColor = Color.Red;
                label1.ForeColor = Color.White;
                return;
            }
            if (_rect.Width <= 0 || _rect.Height <= 0)
            {
                return;
            }
            if (_rect.Width != rect.Width || _rect.Height != rect.Height)
            {
                //size changed
                bmpScreenshot.Dispose();
                gfxScreenshot.Dispose();

                rect = _rect;

                bmpScreenshot = new Bitmap((rect.Width / 2 + 1) * 2, (rect.Height / 2 + 1) * 2, PixelFormat.Format24bppRgb);
                gfxScreenshot = Graphics.FromImage(bmpScreenshot);

            }

            rect = _rect;


            gfxScreenshot.CopyFromScreen(rect.Left, rect.Top, 0, 0, rect.Size, CopyPixelOperation.SourceCopy);
            //User32.PrintWindow(hwn,)


            var pos = (Cursor.Position);
            int w = 6;
            pos.X -= rect.Left;
            pos.Y -= rect.Top;

            pictureBox1.Image = bmpScreenshot;

        }


        List<Color> cc = new List<Color>();
        ManualClusterization mc = new ManualClusterization();
        NativeMultipleBlobExtractor nm = new NativeMultipleBlobExtractor();

        Color? selectedColor = null;


        public void SetStatus(string text)
        {
            toolStripStatusLabel1.Text = text;
        }
        public Bot Bot = new Bot();
        public BlobFilter bf = new BlobFilter();
        private void button4_Click(object sender, EventArgs e)
        {
            var res = bf.Filter(nm.Output);
            var r = Bot.Extract(res, nm.Output);
            if (r != null)
            {
                pictureBox4.Image = Bot.DrawDesk(r);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {            
            ProcessVideoPipeline();
            Bot.MakeMove(Bot.Extracted, Bot.BBlobs);            
        }



        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            Bot.AllowMoves = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            Bot.UseSleep = checkBox3.Checked;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            Bot.StopEveryTurn = checkBox1.Checked;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            Bot.Sleep = int.Parse(textBox1.Text);
        }


        public void ProcessVideoPipeline()
        {
            Stopwatch sw = Stopwatch.StartNew();

            Bot.WindowPosition = rect.Location;
            var f = pictureBox1.Image;


            mc.Bitmap = f;

         
            mc.ColorsInput = cc.ToArray();
            mc.Process();


            nm.Colors = cc.ToArray();
            nm.Image = mc.Output0.Clone();

            nm.Process();


            var res = bf.Filter(nm.Output);
            var r = Bot.Extract(res, nm.Output);
            if (r != null)
            {
                pictureBox4.Image = Bot.DrawDesk(r);
            }

            sw.Stop();
            var ms = sw.ElapsedMilliseconds;

            SetStatus($"{nm.Output.Count() } extracted; Time: {ms} ms");
        }
        private void timer2_Tick(object sender, EventArgs e)
        {

            ProcessVideoPipeline();

            Bot.MakeMove(Bot.Extracted, Bot.BBlobs);
            if (Bot.StopEveryTurn)
            {
                timer2.Enabled = false;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            timer2.Enabled = !timer2.Enabled;
            label6.Text = timer2.Enabled ? "runned" : "stopped";
            label6.ForeColor = Color.White;
            label6.BackColor = timer2.Enabled ? Color.Green : Color.Orange;

            groupBox1.Enabled = !timer2.Enabled;

        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            Bot.AllowRandomMoves = checkBox4.Checked;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            Bot.Rows = int.Parse(textBox2.Text);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            Bot.Cols = int.Parse(textBox3.Text);
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            Bot.UseSmooth = checkBox5.Checked;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            Bot.SmoothSleep = int.Parse(textBox4.Text);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            Bot.Rows = 30;
            Bot.Cols = 16;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Bot.Rows = 16;
            Bot.Cols = 16;
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            Bot.Rows = 8;
            Bot.Cols = 8;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ProcessVideoPipeline();
        }
    }
}
