using System.Windows.Media.Imaging;

namespace GUI
{
    internal class RotatedBitmap
    {
        public RenderTargetBitmap Bitmap { get; private set; }
        public ushort Angle { get; private set; }

        public RotatedBitmap(RenderTargetBitmap bitmap)
        {
            Bitmap = bitmap;
            Angle = 0;
        }

        public void Rotate()
        {
            Angle = (ushort)((Angle + 90) % 360);
        }
    }
}
