using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Algorithm
{
    /**
     * RGBLayers is a class that stores a picture in RGB(A) model of layers
     * every layer is an array width x height of data picture
     * layer is an array of bytes, every bite is an integer from 0 to 255
     * first layer is R(red), second is G(green), third is B(blue), fourth is A(alpha) 
     * 4 layer is about opaque of the RGB-color
     * 
     **/
    public class RGBLayers // first use a bitmap image constructor!!!
    {
        static int bpp = 24; // bits per pixel
        static int width = 256; // width in pixels
        static int height = 256; // height in pixels
        public int layersAmount  { get; private set; }
    //public byte[] pixels { get; private set; }

        public double[,,] layers { get; private set; } // 0-R, 1-G, 2-B
        public RGBLayers(BitmapImage image)
        {
            FormatConvertedBitmap bla = new FormatConvertedBitmap(image, PixelFormats.Rgb24, BitmapPalettes.Halftone256, 0);
            bpp = 24;
           //bpp = image.Format.BitsPerPixel; // omg pictures opens in bgr32
            width = bla.PixelWidth;
            height = bla.PixelHeight;
            int depth = ((bpp + 7) / 8); // layers amount!!!! will be 3
            int stride = width * depth;
            int size = height * stride; // size in bytes
            byte[] pixels = new byte[size];
            bla.CopyPixels(pixels, stride, 0);

            layersAmount = 3;

            layers = ToRGB3DArray(pixels);

        }
        public RGBLayers(byte canvasColor, int pixelHeight, int pixelWidth, int layersAmount) 
        {
            // rgb is 3 layers
            //size /= 4; // argb is 4 layers, need one, argb is 32 bits per pixel: 8 for every layer
            //pixels = new byte[size]; 
            //for (int i = 0; i < size; i++)
            //    pixels[i] = canvasColor;
            this.layersAmount = layersAmount;

            layers = new double[pixelHeight, pixelWidth, layersAmount];
            
            for (int i = 0; i < pixelHeight; i++)
            {
                for (int j = 0; j < pixelWidth; j++)
                {
                    for (int k = 0; k < layersAmount; k++)
                    {
                        layers[i, j, k] = canvasColor;
                    }
                }
            }
        }

        public RGBLayers(int layersAmount, double[,,] layers)
        {
            this.layers = layers;
            this.layersAmount = layersAmount;
        }

        //byte[] get(int x, int y)
        //{
        //    byte[] rgb = new byte[layersAmount];
        //    int stride = width * ((bpp + 7) / 8);
        //    if (layersAmount == 1) // gray
        //    {
        //        rgb[0] = pixels[y * stride + bpp * x];
        //    }
        //    else // layersAmount = 3 RGB
        //    {
        //        int index = y * stride + bpp * x;
        //        byte red = pixels[index];
        //        byte green = pixels[index + 1];
        //        byte blue = pixels[index + 2];
        //        //byte alpha = pixels[index + 3];
        //        //Color color = Color.FromArgb(alpha, red, green, blue);

        //        rgb[0] = red;
        //        rgb[1] = green;
        //        rgb[2] = blue;
        //    }
        //    return rgb;
        //}

        public static RGBLayers operator-(RGBLayers first, RGBLayers second) // abs (a - b)
        {
            //int stride = width * ((bpp + 7) / 8);
            //int size = height * stride;
            //byte[] p = new byte[size];
            double[,,] p = new double[first.layers.GetLength(0), first.layers.GetLength(1), first.layers.GetLength(2)];
            if (first.layersAmount == second.layersAmount)
            {
                for (int k = 0; k < first.layersAmount; k++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            p[x,y,k] = Math.Abs(first.layers[x,y,k] - second.layers[x,y,k]);
                        }
                    }
                }
            }
            return new RGBLayers(first.layersAmount, p);
        } 

        public double[,] rgb2gray()
        {
            double[,] p = new double[width, height];
            if (layersAmount == 1)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        p[x, y] = layers[x, y, 0];
                    }
                }
            } 
            else
            {
                //int stride = width * ((bpp + 7) / 8);
                for (int k = 0; k < layersAmount; k++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            //int index = y * stride + bpp * x;
                            // count weighted sum: R * 0.2989 + G * 0.5870 + B * 0.1140
                            p[x, y] = (layers[x, y, 0] * 0.2989)
                                + (layers[x, y, 1] * 0.5870) + (layers[x, y, 2] * 0.1140);
                        }
                    }
                }
                
            }
            return p;
        }

        private double[,,] ToRGB3DArray(byte[] pixels)
        {
            double[,,] p = new double[width, height, layersAmount];
            int stride = width * ((bpp + 7) / 8);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int index = y * stride + ((bpp + 7) / 8) * x;
                    p[x, y, 0] = pixels[index]; // R
                    p[x, y, 1] = pixels[index + 1]; // G
                    p[x, y, 2] = pixels[index + 2]; // B
                }
            }
            return p;
        }

    }
}