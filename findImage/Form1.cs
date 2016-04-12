using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AForge.Imaging;
using AForge.Imaging.Filters;

namespace WindowsFormsApplication3
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public Bitmap ConvertToFormat(System.Drawing.Image image, System.Drawing.Imaging.PixelFormat format)
        {
            Bitmap copy = new Bitmap(image.Width, image.Height, format);
            using (Graphics gr = Graphics.FromImage(copy))
            {
                gr.DrawImage(image, new Rectangle(0, 0, copy.Width, copy.Height));
            }
            return copy;
        }

        int imageX = 0;
        int imageY = 0;

        public delegate void ControlStringConsumer(Control control, string text);  // defines a delegate type

        public void SetText(Control control, string text)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(new ControlStringConsumer(SetText), new object[] { control, text });  // invoking itself
            }
            else
            {
                control.Text = text; // the "functional part", executing only on the main thread
            }
        }

        void Contains(Bitmap template, Bitmap bmp)
        {
            const Int32 divisor = 4;
            const Int32 epsilon = 10;

            ExhaustiveTemplateMatching etm = new ExhaustiveTemplateMatching(0.925f); //98% threshold

            TemplateMatch[] tm = etm.ProcessImage(
                new ResizeNearestNeighbor(template.Width / divisor, template.Height / divisor).Apply(template),
                new ResizeNearestNeighbor(bmp.Width / divisor, bmp.Height / divisor).Apply(bmp)
            );
            label5.Text = tm.Length.ToString();
            if (tm.Length == 1)
            {
                Rectangle tempRect = tm[0].Rectangle;

                imageX = tempRect.Location.X * divisor;
                imageY = tempRect.Location.Y * divisor;
                
                SetText(imageXY, "X:" + (imageX).ToString() + " Y:" + (imageY).ToString());

                if (Math.Abs(bmp.Width / divisor - tempRect.Width) < epsilon && Math.Abs(bmp.Height / divisor - tempRect.Height) < epsilon)
                {
                    SetText(findImage, "True");
                }
            }
            else
            {
                SetText(findImage, "False");
                SetText(imageXY, "");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            cursorXY.Text = Cursor.Position.ToString();
        }

        private void MoveCursor(int x, int y)
        {
            Cursor.Position = new Point(x, y);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MoveCursor(imageX, imageY);
        }

        Bitmap find;
        Bitmap template2;

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int screenWidth = Screen.GetBounds(new Point(0, 0)).Width;
            int screenHeight = Screen.GetBounds(new Point(0, 0)).Height;
            Bitmap bmpScreenShot = new Bitmap(screenWidth, screenHeight);
            Bitmap findimg = new Bitmap(Application.StartupPath + @"\find.bmp");
            Graphics gfx = Graphics.FromImage(bmpScreenShot);
            gfx.CopyFromScreen(0, 0, 0, 0, new Size(screenWidth, screenHeight));
            //bmpScreenShot.Save("Screenshot.bmp", System.Drawing.Imaging.ImageFormat.Bmp); //DEBUG

            Bitmap template = ConvertToFormat(bmpScreenShot, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            find = ConvertToFormat(findimg, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            pictureBox1.Image = null;
            pictureBox1.Image = template;

            template2 = ConvertToFormat(bmpScreenShot, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            bmpScreenShot.Dispose();
            findimg.Dispose();
            gfx.Dispose();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle ee = new Rectangle(Convert.ToInt32(imageX / 4.99), Convert.ToInt32(imageY / 5.5), 30, 30);
            using (Pen pen = new Pen(Color.Red, 2))
            {
                e.Graphics.DrawRectangle(pen, ee);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Contains(template2, find);
            if (!backgroundWorker1.IsBusy)
                backgroundWorker1.RunWorkerAsync();
        }
    }
}
