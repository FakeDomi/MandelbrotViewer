using System;
using System.Threading;

namespace MandelbrotViewer
{
    public class ImageRenderer
    {
        private readonly AutoResetEvent requestResetEvent = new AutoResetEvent(false);
        private readonly AutoResetEvent completionResetEvent = new AutoResetEvent(false);

        private RenderArguments renderTask;
        private Thread worker;

        public void Start()
        {
            this.worker = new Thread(this.Run) { IsBackground = true };
            this.worker.Start();
        }

        public void RunTask(RenderArguments renderTask)
        {
            this.renderTask = renderTask;
            this.requestResetEvent.Set();
        }

        public void Join()
        {
            this.completionResetEvent.WaitOne();
        }

        private void Run()
        {
            while (true)
            {
                this.requestResetEvent.WaitOne();

                double offsetX = this.renderTask.OffsetX;
                double offsetY = this.renderTask.OffsetY;
                double zoom = this.renderTask.Zoom;

                byte[] bitmapData = this.renderTask.Bitmap.Bits;
                int pixelPos = this.renderTask.Bitmap.Width * 4 * this.renderTask.StartRow;

                int startRow = this.renderTask.StartRow;
                int endRow = this.renderTask.EndRow;

                for (int pY = startRow; pY < endRow; pY++)
                {
                    for (int pX = 0; pX < 350; pX++)
                    {
                        double x0 = (pX - 175 - offsetX) / zoom;
                        double y0 = (pY - 100 - offsetY) / zoom;

                        double x = 0D;
                        double y = 0D;

                        int iterations = 0 - this.renderTask.Correction;

                        while (x * x + y * y < 4 && iterations < 255)
                        {
                            double temp = x * x - y * y + x0;
                            y = 2 * x * y + y0;
                            x = temp;
                            iterations++;
                        }
                        
                        byte i = (byte)(Math.Max(0, iterations));
                        byte b = (byte)((i > 127 ? 0xFF : i * 2));

                        bitmapData[pixelPos] = b;
                        bitmapData[pixelPos + 1] = i;
                        bitmapData[pixelPos + 2] = i;
                        bitmapData[pixelPos + 3] = 0xFF;

                        pixelPos += 4;
                    }
                }

                this.renderTask = null;
                this.completionResetEvent.Set();
            }
        }
    }
}
