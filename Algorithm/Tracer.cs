using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Drawing.Common;

namespace Algorithm
{
    public class Tracer
    {
        const uint depth = 3;

        public StrokeBrush strokeBrush { get; private set; }
        public uint maxAmountOfIters { get; private set; }
        public BitmapImage image { get; private set; }
        public Settings settings { get; }
        public RGBLayers grayCanvas { get; private set; } // array m x n of canvasColor // is that ok
        public RGBLayers coloredCanvas { get; private set; } // array m x n x 3 of canvasColor
        public RGBLayers initialImage { get; private set; } // array m x n x 3 where m x n x 0 is a red layer, m x n x 1 is a green layer, m x n x 2 is a blue layer, ignore layer alpha
        public RGBLayers error { get; private set; }
        public RGBLayers unblurredImage { get; private set; }

        public double[,] grayInitialImage { get; private set; }

        public Gradient gradients; // stores gradients (pairs U and V for every pixel) for all picture
        
        public Tracer(BitmapImage img, Settings settings) // settings shouldn't be null! 
        {
            // битмапы должны иметь размеры исходной картинки, один цвета canvasColor, другой цветной на таком же
            //canvasgray = (canvasColor + zeros(m, n, 1)); % gray version of canvas with tones
            //canvas = (canvasColor + zeros(m, n, 3)); % colored version

            if (settings == null) // i would like to throw exception here or to make sure of parameters by default
                settings = new Settings(new GUITrace());

            this.settings = settings;

            image = img;

            strokeBrush = new StrokeBrush(settings.guiTrace.brushWidthMM,
               Math.Min(settings.guiTrace.canvasWidthMM / image.PixelWidth,
               settings.guiTrace.canvasHeightMM / image.PixelHeight));
            maxAmountOfIters = strokeBrush.thickness;
            initialImage = new RGBLayers(image);
            grayCanvas = new RGBLayers(settings.canvasColor, image.PixelHeight, image.PixelWidth, 1);
            coloredCanvas = new RGBLayers(settings.canvasColor, image.PixelHeight, image.PixelWidth, 3);
            error = coloredCanvas - initialImage; // not doubled, for what? may be done later where it'll be needed in countings

            unblurredImage = initialImage;

            if (settings.doBlur)
            {
                double[,] H = Functions.fspecial((int)strokeBrush.smallThickness);
                initialImage = Functions.conv2(initialImage, H, "replicate");
            }

            grayInitialImage = initialImage.rgb2gray();
            // strokes = cell(0);
            gradients = new Gradient(grayInitialImage, strokeBrush.thickness);



            // ale iterations
            doIterations();
        }

        private void doIterations()
        {
            int nStrokes = 0;
            int accepted = 0;

            int mSize = initialImage.layers.GetLength(0);
            int nSize = initialImage.layers.GetLength(1);
            int kSize = initialImage.layersAmount;

            double[,,] syntheticSmearMap = new double[mSize, nSize, kSize]; // canvas2

            for (int kk = 0; kk < settings.amountOfTotalIters; kk++)
            {
                double overlap = settings.minInitOverlapRatio;

                if (kk == 2)
                    initialImage = unblurredImage;

                if (kk > settings.minInitOverlapRatio)
                    overlap = settings.maxInitOverlapRatio;
                else
                    overlap = settings.minInitOverlapRatio;

                // int j = 0; // wtf
                for (int i = 0; i < mSize; i++)
                {
                    for (int j = 0; j < nSize; j++)
                    {
                        if ((error.layers[i, j, 0] > settings.pixTolAccept) || (syntheticSmearMap[i, j, 0] == 0)) 
                        {
                            int prevX = i; // pX
                            int prevY = j; // pY

                            //meancol is [r, g, b], col is the same but in shape of 3d array like (0,0,k) element
                            double[] meanColorPixel = Functions.getMeanColor( // col? meancol
                                initialImage, prevX, prevY, strokeBrush.smallThickness, 
                                strokeBrush.bsQuad, mSize, nSize);


                        }
                    }
                }





            }

        }

    }
}
