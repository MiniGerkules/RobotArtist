using System;
using GeneralComponents;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Algorithm {
    public class Tracer {
        public StrokeBrush strokeBrush { get; private set; }
        public uint MaxAmountOfIters { get; private set; }
        public BitmapImage Image { get; private set; }
        public Settings settings { get; }
        public Matrix2D ColorClass { get; private set; } // #canvasgraymix first layer
        public Matrix2D VolumeOfWhite { get; private set; } // #canvasgraymix second layer
        public Matrix3D ColoredCanvas { get; private set; } // #canvas // array m x n x 3 of canvasColor
        public Matrix3D InitialImage { get; private set; } // #img // array m x n x 3 where m x n x 0 is a red layer, m x n x 1 is a green layer, m x n x 2 is a blue layer, ignore layer alpha
        public Matrix3D Error { get; private set; } // #err
        public Matrix3D UnblurredImage { get; private set; } // #img_unblurred
        public Matrix2D GrayInitialImage { get; private set; }
        public List<List<List<double>>> Ycell { get; private set; }
        public List<List<List<double>>> Wcell { get; private set; }

        public Gradient Gradients; // stores Gradients (pairs U and V for every pixel) for all picture

        public Tracer(BitmapImage img, Settings settings, Database database) // settings shouldn't be null! 
        {
            if (settings == null) // i would like to throw exception here or to make sure of parameters by default
                settings = new Settings(new GUITrace());

            this.settings = settings;

            Image = img;

            strokeBrush = new StrokeBrush(settings.guiTrace.brushWidthMM,
               Math.Min(settings.guiTrace.canvasWidthMM / Image.PixelWidth,
               settings.guiTrace.canvasHeightMM / Image.PixelHeight));
            MaxAmountOfIters = strokeBrush.thickness;
            InitialImage = Functions.imageToMatrix3D(Image);

            Ycell = database.GetHSV().Copy();
            Wcell = database.GetProportions().Copy();

            ColorClass = new Matrix2D(Image.PixelHeight, Image.PixelWidth); // ColorMixType only
            VolumeOfWhite = new Matrix2D(Image.PixelHeight, Image.PixelWidth);
            ColoredCanvas = new Matrix3D(Image.PixelHeight, Image.PixelWidth, layers: 3, initVal: settings.canvasColor);
            Error = ColoredCanvas - InitialImage; // not doubled, for what? may be done later where it'll be needed in countings

            UnblurredImage = InitialImage;

            if (settings.doBlur) {
                double[,] H = Functions.fspecial((int)strokeBrush.smallThickness);
                InitialImage = Functions.conv2(InitialImage, H, "replicate");
            }

            GrayInitialImage = InitialImage.rgb2gray();

            Gradients = new Gradient(GrayInitialImage, strokeBrush.thickness);

            // ale iterations
            doIterations();
        }

        private void doIterations() {
            int nStrokes = 0;
            int accepted = 0;

            int mSize = InitialImage[0].Rows;
            int nSize = InitialImage[0].Columns;
            int kSize = InitialImage.Layers;

            Matrix3D syntheticSmearMap = new Matrix3D(mSize, nSize, kSize); // #canvas2

            for (int kk = 0; kk < settings.amountOfTotalIters; kk++) {
                double overlap = settings.minInitOverlapRatio;

                if (kk == 2)
                    InitialImage = UnblurredImage;

                if (kk > settings.minInitOverlapRatio)
                    overlap = settings.maxInitOverlapRatio;
                else
                    overlap = settings.minInitOverlapRatio;

                for (int i = 0; i < mSize; i++) {
                    for (int j = 0; j < nSize; j++) {
                        if ((Error[i, j, 0] > settings.pixTolAccept) || (syntheticSmearMap[i, j, 0] == 0)) {
                            int prevX = i; // pX
                            int prevY = j; // pY

                            //meancol is [r, g, b], col is the same but in shape of 3d array like (0,0,k) element
                            double[] meanColorPixel = Functions.getMeanColor( // col? meancol
                                InitialImage, prevX, prevY, strokeBrush.smallThickness,
                                strokeBrush.bsQuad, mSize, nSize);
                            double[] hsvMeanColorPixel = new double[meanColorPixel.Length];
                            for (int m = 0; m < meanColorPixel.Length; m++)
                                hsvMeanColorPixel[m] = meanColorPixel[m] / 255;
                            hsvMeanColorPixel = Functions.rgb2hsv(hsvMeanColorPixel);

                            double[] proportions;
                            ColorMixType mixTypes;

                            Functions.PredictProportions(out proportions, out mixTypes, hsvMeanColorPixel, Ycell, Wcell, Ycell.Count);

                        }
                    }
                }





            }

        }

    }
}
