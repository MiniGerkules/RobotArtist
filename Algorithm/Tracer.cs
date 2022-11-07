using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeneralComponents;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.ConstrainedExecution;
using System.Windows.Ink;

namespace Algorithm
{
    public class Tracer
    {
        public StrokeBrush strokeBrush { get; private set; }
        public int MaxAmountOfIters { get; private set; }
        public BitmapImage Image { get; private set; }
        public Settings settings { get; }
        public Matrix2D ColorClass { get; private set; } // #canvasgraymix first layer
        public Matrix2D VolumeOfWhite { get; private set; } // #canvasgraymix second layer
        public Matrix3D ColoredCanvas { get; private set; } // #canvas // array m x n x 3 of canvasColor
        public Matrix3D InitialImage { get; private set; } // #img // array m x n x 3 where m x n x 0 is a red layer, m x n x 1 is a green layer, m x n x 2 is a blue layer, ignore layer alpha
        public Matrix3D Error { get; private set; } // #err
        public Matrix3D UnblurredImage { get; private set; } // #img_unblurred
        public Matrix2D GrayInitialImage { get; private set; } // #imggray
        public List<List<List<double>>> Ycell { get; private set; }
        public List<List<List<double>>> Wcell { get; private set; }

        public Gradient Gradients; // stores Gradients (pairs U and V for every pixel) for all picture
        
        public Tracer(BitmapImage img, Settings settings) // settings shouldn't be null! 
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

            
            Database.LoadDatabase("C:\\Users\\varka\\Documents\\3course2sem\\NIR\\NIRgit\\GeneralComponents\\resources\\ModelTable600.xls");
            if (!Database.IsLoad())
            {
                throw new ArgumentException("Path to color model table is not valid");
            }

            List<List<List<double>>> ycell, wcell;

            Functions.getYcellWcell(Database.Data, out ycell, out wcell);

            Ycell = ycell;
            Wcell = wcell;

            ColorClass = new Matrix2D(Image.PixelHeight, Image.PixelWidth); // ColorMixType only
            VolumeOfWhite = new Matrix2D(Image.PixelHeight, Image.PixelWidth);
            ColoredCanvas = new Matrix3D(Image.PixelHeight, Image.PixelWidth, layers: 3, initVal: settings.canvasColor);
            Error = ColoredCanvas - InitialImage; // not doubled, for what? may be done later where it'll be needed in countings

            UnblurredImage = InitialImage;

            if (settings.doBlur)
            {
                double[,] H = Functions.fspecial((int)strokeBrush.smallThickness);
                InitialImage = Functions.conv2(InitialImage, H, "replicate");
            }

            GrayInitialImage = InitialImage.rgb2gray();
            
            Gradients = new Gradient(GrayInitialImage, strokeBrush.thickness);

            // ale iterations
            doIterations();
        }

        private void doIterations()
        {
            int nStrokes = 0;
            bool isNewPieceAccepted = false; // #accepted

            int mSize = InitialImage[0].Rows;
            int nSize = InitialImage[0].Columns;
            int kSize = InitialImage.Layers;

            Matrix3D syntheticSmearMap = new Matrix3D(mSize, nSize, kSize); // #canvas2

            for (int kk = 0; kk < settings.amountOfTotalIters; kk++)
            {
                double overlap = settings.minInitOverlapRatio;

                if (kk == 2)
                    InitialImage = UnblurredImage;

                if (kk > settings.minInitOverlapRatio)
                    overlap = settings.maxInitOverlapRatio;
                else
                    overlap = settings.minInitOverlapRatio;

                for (int i = 0; i < mSize; i++) 
                {
                    for (int j = 0; j < nSize; j++)
                    {
                        if ((Error[i, j, 0] > settings.pixTolAccept) || (syntheticSmearMap[i, j, 0] == 0)) 
                        {
                            int prevX = i; // pX
                            int prevY = j; // pY

                            //meancol is [r, g, b], col is the same but in shape of 3d array like (0,0,k) element
                            double[] meanColorPixel = Functions.getMeanColor( // #col? #meancol
                                InitialImage, prevX, prevY, strokeBrush.smallThickness, 
                                strokeBrush.bsQuad, mSize, nSize);
                            double[] hsvMeanColorPixel = new double[meanColorPixel.Length];
                            for (int m = 0; m < meanColorPixel.Length; m++)
                                hsvMeanColorPixel[m] = meanColorPixel[m] / 255;
                            hsvMeanColorPixel = Functions.rgb2hsv(hsvMeanColorPixel);

                            double[] proportions;
                            ColorMixType mixTypes;

                            Functions.PredictProportions(out proportions, out mixTypes, hsvMeanColorPixel, Ycell, Wcell, Ycell.Count);

                            // proportions of paints in real values
                            double[] col8paints = Functions.proportions2pp(proportions, mixTypes);
                            // the volume of white in the paint mixture for sorting smears by overlap
                            double volumeOfWhite = col8paints[4];

                            // draw a synthetic map of smears with colour
                            // that fits the type of mixture

                            double[] col2 = new double[3];
                            var rand = new Random();
                            rand.NextDouble();

                            switch(mixTypes)
                            {
                                case ColorMixType.MagentaYellow1:
                                    col2 = new double[] {0.5 + 0.5 * rand.NextDouble(), 0.5 * rand.NextDouble(), 0.5 * rand.NextDouble() };
                                    break;
                                case ColorMixType.MagentaYellow2:
                                    col2 = new double[] { 0.5 + 0.5 * rand.NextDouble(), 0.5 + 0.5 * rand.NextDouble(), 0.5 * rand.NextDouble() };
                                    break;
                                case ColorMixType.YellowCyan:
                                    col2 = new double[] { 0.5 * rand.NextDouble(), 0.5 + 0.5 * rand.NextDouble(), 0.5 * rand.NextDouble() };
                                    break;
                                case ColorMixType.CyanMagenta:
                                    col2 = new double[] { 0, 0, 0.5 * rand.NextDouble() + 0.5 * rand.NextDouble() };
                                    break;
                            }

                            int nPoints = 1; // amount of points in the stroke #nPts
                            bool isStrokeEnded = false; // #endedStroke 

                            List<Stroke> strokes = new List<Stroke>();
                            strokes.Add(new Stroke(new System.Windows.Point(prevX, prevY), meanColorPixel, col8paints, proportions, mixTypes));

                            while (!isStrokeEnded)
                            {
                                // %find new direction
                                int counter = 0; // #ctr index
                                StrokeCandidate candidate = new StrokeCandidate(-1, -1, Double.MaxValue);
                                while (counter < MaxAmountOfIters)
                                {
                                    double cosA = 0, sinA = 0;
                                    Functions.getDirection(new System.Drawing.Point(prevX, prevY), strokes, Gradients, out cosA, out sinA);
                                    int r = MaxAmountOfIters - counter - 1;

                                    candidate.x = prevX + Math.Round(r * cosA); // %new X
                                    candidate.y = prevY + Math.Round(r * sinA); // %new Y 

                                    // next i should write TestNewPiece function
                                    // after that i think will be right to check all
                                    // and push to github
                                    // double error = 0;
                                    isNewPieceAccepted = Functions.testNewPieceAccepted( // #accepted
                                        startPoint: new System.Drawing.Point(prevX, prevY),
                                        img: InitialImage,
                                        canvasColor: settings.canvasColor,
                                        canvasEps: settings.canvasColorFault,
                                        canvas2: syntheticSmearMap,
                                        ColorClass: ColorClass,
                                        VolumeOfWhite: VolumeOfWhite,
                                        pixTol: settings.pixTol,
                                        pixTolAverage: settings.pixTolAverage,
                                        meanColorPixel: meanColorPixel,
                                        mSize: mSize,
                                        nSize: nSize,
                                        overlap: overlap,
                                        bs2: strokeBrush.smallThickness,
                                        bsQuad: strokeBrush.bsQuad,
                                        mixTypes: mixTypes,
                                        volumeOfWhite: volumeOfWhite,
                                        ref candidate
                                    );

                                    // #### to here all is OK ####

                                    if (isNewPieceAccepted) //%if error is small, accept the stroke immediately
                                        counter = MaxAmountOfIters;
                                    else
                                    {
                                        //% also try opposite direction
                                        candidate.x = prevX - Math.Round(r * cosA); // % new X
                                        candidate.y = prevY - Math.Round(r * sinA); // % new Y

                                        // % test new fragment of the stroke

                                        isNewPieceAccepted = Functions.testNewPieceAccepted( // #accepted
                                            startPoint: new System.Drawing.Point(prevX, prevY),
                                            img: InitialImage,
                                            canvasColor: settings.canvasColor,
                                            canvasEps: settings.canvasColorFault,
                                            canvas2: syntheticSmearMap,
                                            ColorClass: ColorClass,
                                            VolumeOfWhite: VolumeOfWhite,
                                            pixTol: settings.pixTol,
                                            pixTolAverage: settings.pixTolAverage,
                                            meanColorPixel: meanColorPixel,
                                            mSize: mSize,
                                            nSize: nSize,
                                            overlap: overlap,
                                            bs2: strokeBrush.smallThickness,
                                            bsQuad: strokeBrush.bsQuad,
                                            mixTypes: mixTypes,
                                            volumeOfWhite: volumeOfWhite,
                                            ref candidate
                                        );

                                        if (isNewPieceAccepted) //%if error is small, accept the stroke immediately
                                         counter = MaxAmountOfIters;
                                    }
                                    counter++;
                                }
                                // %draw the stroke fragment

                                if(isNewPieceAccepted)
                                {
                                    Matrix3D coloredCanvas = ColoredCanvas;
                                    Matrix2D colorClass = ColorClass;
                                    Matrix2D volOfWhite = VolumeOfWhite;

                                    Matrix2D err = Functions.drawPiece(
                                            startPoint: new System.Drawing.Point(prevX, prevY),
                                            candidate: candidate,
                                            bs2: strokeBrush.smallThickness,
                                            bsQuad: strokeBrush.bsQuad,
                                            canvas: ref coloredCanvas,
                                            canvas2: ref syntheticSmearMap,
                                            ColorClass: ref colorClass,
                                            VolumeOfWhite: ref volOfWhite,
                                            meanColorPixel: meanColorPixel,
                                            col2: col2,
                                            imggray: GrayInitialImage,
                                            mSize: mSize,
                                            nSize: nSize,
                                            mixTypes: mixTypes,
                                            volumeOfWhite: volumeOfWhite
                                        );

                                   // % determine new length
                                    var dlen = Math.Sqrt((prevX - candidate.x) * (prevX - candidate.x) + (prevY - candidate.y) * (prevY - candidate.y));
                                    strokes.Capacity = strokes.Count + (int)dlen;

                                    // %if length is too large
                                    if (strokes.Capacity >= strokeBrush.maxStrokeLength)
                                        isStrokeEnded = true;

                                    double vX = candidate.x;
                                    double vY = candidate.y;
                                    double pX = candidate.x; 
                                    double pY = candidate.y;

                                    // тут я чет не врубаюсь, что происходит
                                    //strokes.points = [strokes.x, vX];
                                    //strokes.y = [strokes.y, vY];
                                    //nPoints++;
                                }

                            }

                        }
                    }
                }





            }

        }

    }
}
