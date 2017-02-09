using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace MandelbrotViewer
{
    public class FastBitmap
    {
        public Bitmap Bitmap { get; }
        public byte[] Bits { get; }
        public bool Disposed { get; private set; }
        public int Height { get; private set; }
        public int Width { get; private set; }

        protected GCHandle BitsHandle { get; }

        private readonly ArrayPool<byte> pool;

        public FastBitmap(int width, int height, ArrayPool<byte> pool)
        {
            this.Width = width;
            this.Height = height;
            this.Bits = pool.Get();
            this.pool = pool;
            this.BitsHandle = GCHandle.Alloc(this.Bits, GCHandleType.Pinned);
            this.Bitmap = new Bitmap(width, height, width * 4, PixelFormat.Format32bppPArgb, this.BitsHandle.AddrOfPinnedObject());
        }

        public void Dispose()
        {
            this.pool.Return(this.Bits);

            if (this.Disposed) return;
            this.Disposed = true;
            this.Bitmap.Dispose();
            this.BitsHandle.Free();
        }
    }
}
