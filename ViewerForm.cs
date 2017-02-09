using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace MandelbrotViewer
{
    public partial class ViewerForm : Form
    {
        private bool dragging;
        private int originX, originY;
        private int floatingX, floatingY;

        private double offsetX = 75, offsetY;

        //private double lastX, lastY, lastZ;

        private double zoom = 100;

        private readonly ImageRenderer worker0 = new ImageRenderer();
        private readonly ImageRenderer worker1 = new ImageRenderer();
        private readonly ImageRenderer worker2 = new ImageRenderer();
        private readonly ImageRenderer worker3 = new ImageRenderer();

        private readonly ArrayPool<byte> arrayPool = new ArrayPool<byte>(280000);

        private FastBitmap currentBitmap;

        private int correction;

        public ViewerForm()
        {
            this.InitializeComponent();

            this.pictureBox1.MouseDown += (sender, args) =>
            {
                this.dragging = true;

                this.originX = args.X;
                this.originY = args.Y;
            };

            this.pictureBox1.MouseMove += (sender, args) =>
            {
                if (this.dragging)
                {
                    this.floatingX = args.X - this.originX;
                    this.floatingY = args.Y - this.originY;
                }
            };

            this.pictureBox1.MouseUp += (sender, args) =>
            {
                this.dragging = false;

                this.offsetX += this.floatingX;
                this.offsetY += this.floatingY;

                this.floatingX = 0;
                this.floatingY = 0;
            };

            this.trackBar1.MouseWheel += (sender, args) =>
            {
                ((HandledMouseEventArgs)args).Handled = true;

                this.pictureBox1.Focus();
            };

            this.worker0.Start();
            this.worker1.Start();
            this.worker2.Start();
            this.worker3.Start();
        }
        

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (this.zoom < 0.01 * long.MaxValue)
                {
                    double baseOffsetX = this.offsetX / this.zoom;
                    double baseOffsetY = this.offsetY / this.zoom;

                    this.zoom *= 1.1;

                    this.offsetX = baseOffsetX * this.zoom - 17.5D * (e.X - 175) / 175;
                    this.offsetY = baseOffsetY * this.zoom - 10D * (e.Y - 100D) / 100;
                }

                //offsetY -= (int) (0.5D*20*(mouseEventArgs.Y - 100)/100);
                //offsetX -= (int) (0.5D*35*(mouseEventArgs.X - 175)/175);
            }
            else
            {
                if (this.zoom > 10)
                {
                    //offsetY += (int)(0.5D * 20 * (mouseEventArgs.Y - 100) / 100);
                    //offsetX += (int)(0.5D * 35 * (mouseEventArgs.X - 175) / 100);

                    double baseOffsetX = this.offsetX / this.zoom;
                    double baseOffsetY = this.offsetY / this.zoom;

                    this.zoom /= 1.1;

                    this.offsetX = baseOffsetX * this.zoom + 17.5D * (e.X - 175) / 175;
                    this.offsetY = baseOffsetY * this.zoom + 10D * (e.Y - 100D) / 100;
                }
            }
        }

        private void RedrawTimerTick(object sender, EventArgs e)
        {
            if (!this.backgroundWorker.IsBusy)
            {
                this.backgroundWorker.RunWorkerAsync(new Tuple<double, double, double, int>(this.offsetX + this.floatingX, this.offsetY + this.floatingY, this.zoom, this.correction));
            }
        }

        private void SetSpeedButtonClick(object sender, EventArgs e)
        {
            int value;

            if (!int.TryParse(this.speedTextBox.Text, out value))
            {
                this.speedTextBox.Text = "";
            }
            else
            {
                this.redrawTimer.Interval = value;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.offsetX = 75;
            this.offsetY = 0;
            this.zoom = 100;

            this.trackBar1.Value = 0;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            this.correction = this.trackBar1.Value;
        }

        private void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            Tuple<double, double, double, int> offsets = (Tuple<double, double, double, int>)e.Argument;

            //if (offsets.Item1 == this.lastX && offsets.Item2 == this.lastY && offsets.Item3 == this.lastZ)
            //{
            //    return; // no redraw required
            //}

            FastBitmap bitmap = new FastBitmap(350, 200, this.arrayPool);

            Stopwatch sw = Stopwatch.StartNew();

            RenderArguments renderTask = new RenderArguments { OffsetX = offsets.Item1, OffsetY = offsets.Item2, Zoom = offsets.Item3, Bitmap = bitmap, Correction = offsets.Item4};

            this.worker0.RunTask(renderTask.WithOffsets(0, 50));
            this.worker1.RunTask(renderTask.WithOffsets(50, 100));
            this.worker2.RunTask(renderTask.WithOffsets(100, 150));
            this.worker3.RunTask(renderTask.WithOffsets(150, 200));

            this.worker0.Join();
            this.worker1.Join();
            this.worker2.Join();
            this.worker3.Join();
            
            e.Result = new Tuple<FastBitmap, Stopwatch>(bitmap, sw);
        }

        private void BackgroundWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Tuple<FastBitmap, Stopwatch> result = (Tuple<FastBitmap, Stopwatch>)e.Result;

            if (result == null)
            {
                return;
            }

            this.label1.Text = $"{result.Item2.ElapsedMilliseconds} / {this.redrawTimer.Interval} ms";

            this.pictureBox1.Image = result.Item1.Bitmap;

            this.currentBitmap?.Dispose();
            this.currentBitmap = result.Item1;

            this.UpdateTextboxes();
        }

        private void UpdateTextboxes()
        {
            this.zoomTextBox.Text = $"{(long)this.zoom}%";
            this.xTextBox.Text = ((long)(0D - this.floatingX - this.offsetX)).ToString();
            this.yTextBox.Text = ((long)(this.floatingY + this.offsetY)).ToString();

            this.textBox1.Text = ((long)(3.5D * this.zoom)).ToString();
            this.textBox2.Text = ((long)(2D * this.zoom)).ToString();

            this.label8.Text = this.correction.ToString();
        }
    }
}
