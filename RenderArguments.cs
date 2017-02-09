namespace MandelbrotViewer
{
    public class RenderArguments
    {
        public double OffsetX { get; set; }

        public double OffsetY { get; set; }

        public double Zoom { get; set; }

        public int StartRow { get; set; }

        public int EndRow { get; set; }

        public FastBitmap Bitmap { get; set; }

        public int Correction { get; set; }

        public RenderArguments WithOffsets(int newStartRow, int newEndRow)
        {
            return new RenderArguments { OffsetX = this.OffsetX, OffsetY = this.OffsetY, Zoom = this.Zoom, Bitmap = this.Bitmap, StartRow = newStartRow, EndRow = newEndRow, Correction = this.Correction };
        }
    }
}
