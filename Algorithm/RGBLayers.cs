using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using Accord.Imaging.Filters;

namespace Algorithm
{
    public class RGBLayers // first use a bitmap image constructor!!!
    {
        static int bpp = 8; // bits per pixel for argb
        static int width = 256; // width in pixels
        static int height = 256; // height in pixels
        uint layersAmount = 3;
        private byte[] pixels;
        public RGBLayers(BitmapImage image)
        {
            bpp = image.Format.BitsPerPixel;
            width = image.PixelWidth;
            height = image.PixelHeight;
            int stride = width * ((bpp + 7) / 8);
            int size = height * stride; // size in bytes
            pixels = new byte[size];
            image.CopyPixels(pixels, stride, 0);
        }
        public RGBLayers(byte canvasColor, uint layersAmount)
        {
            this.layersAmount = layersAmount;
            int stride = width * ((bpp + 7) / 8);
            int size = height * stride; // size in bytes
            if (layersAmount == 1)
                size /= 4; // argb is 4 layers, need one
            pixels = new byte[size]; 
            for (int i = 0; i < size; i++)
                pixels[i] = canvasColor;
        }

        RGBLayers(uint layersAmount, byte[] pixels)
        {
            this.layersAmount = layersAmount;
            this.pixels = pixels;
        }

        byte[] get(int x, int y)
        {
            byte[] rgb = new byte[layersAmount];
            int stride = width * ((bpp + 7) / 8);
            if (layersAmount == 1) // gray
            {
                rgb[0] = pixels[y * stride + bpp * x];
            }
            else // layersAmount = 3 RGB
            {
                int index = y * stride + bpp * x;
                byte red = pixels[index];
                byte green = pixels[index + 1];
                byte blue = pixels[index + 2];
                //byte alpha = pixels[index + 3];
                //Color color = Color.FromArgb(alpha, red, green, blue);

                rgb[0] = red;
                rgb[1] = green;
                rgb[2] = blue;
            }
            return rgb;
        }

        public static RGBLayers operator-(RGBLayers first, RGBLayers second) // abs (a - b)
        {
            int stride = width * ((bpp + 7) / 8);
            int size = height * stride;
            byte[] p = new byte[size];
            if (first.layersAmount == second.layersAmount)
            {
                for (int i = 0; i < size; i++)
                {
                    p[i] = (byte)(Math.Abs(first.pixels[i] - second.pixels[i]));
                }
            }
            return new RGBLayers(first.layersAmount, p);
        } 

        public RGBLayers rgb2gray()
        {
            if (layersAmount == 1)
                return this;
            else
            {
                int stride = width * ((bpp + 7) / 8);
                int size = height * stride;
                byte[] p = new byte[size];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int index = y * stride + bpp * x;
                        p[index] = (byte)(pixels[index] * 0.2989); // R
                        p[index + 1] = (byte)(pixels[index + 1] * 0.5870); // G
                        p[index + 2] = (byte)(pixels[index + 2] * 0.1140); // B
                        p[index + 3] = 255; // alpha - not opaque at all
                    }
                }
                return new RGBLayers(layersAmount, p);
            }
        }

    }
}