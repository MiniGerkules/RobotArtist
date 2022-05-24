using System.Windows.Media.Imaging;

namespace GUI
{
    internal class RotatedBitmap
    {
        public RenderTargetBitmap Bitmap { get; set; }
        public ushort Angle { get; private set; }

        public RotatedBitmap(RenderTargetBitmap bitmap, ushort angle = 0)
        {
            Bitmap = bitmap;
            Angle = angle;
        }

        public void Rotate()
        {
            Angle = (ushort)((Angle + 90) % 360);
        }
    }
}
