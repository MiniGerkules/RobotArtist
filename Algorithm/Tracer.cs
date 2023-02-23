using System;
using GeneralComponents;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Windows;

namespace Algorithm {
    public class Tracer {
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

        public Tracer(BitmapImage img, Settings settings, Database database) // settings shouldn't be null! 
        {
            if (settings == null) // i would like to throw exception here or to make sure of parameters by default
                settings = new Settings(new GUITrace());

            this.settings = settings;

            Image = img;

            strokeBrush = new StrokeBrush(settings.guiTrace.brushWidthMM,
               Math.Min(settings.guiTrace.canvasWidthMM / Image.PixelWidth, // #sfX Image.PixelWidth should be greater than settings.guiTrace.canvasWidthMM
               settings.guiTrace.canvasHeightMM / Image.PixelHeight)); // #sfY
            MaxAmountOfIters = strokeBrush.thickness; // the max amount of attempts to find a new line section
            InitialImage = Functions.imageToMatrix3D(Image); // ##ok

            Ycell = database.GetHSV().Copy(); // ##NOTok
            Wcell = database.GetProportions().Copy(); // ##NOTok

            ColorClass = new Matrix2D(Image.PixelHeight, Image.PixelWidth); // #canvasgraymix 1 ColorMixType only // ##ok
            VolumeOfWhite = new Matrix2D(Image.PixelHeight, Image.PixelWidth); // #canvasgraymix 2 // ##ok
            ColoredCanvas = new Matrix3D(Image.PixelHeight, Image.PixelWidth, layers: 3, initVal: settings.canvasColor); // #canvas // ##ok
            Error = ColoredCanvas - InitialImage; // not doubled, for what? may be done later where it'll be needed in countings // ##ok

            UnblurredImage = InitialImage;

            if (settings.doBlur) {
                Matrix2D H = Functions.fspecial((int)strokeBrush.smallThickness);
                InitialImage = Functions.conv2(InitialImage, H, "replicate");
            }

            GrayInitialImage = InitialImage.rgb2gray(); // #imggray // ##ok

            Gradients = new Gradient(GrayInitialImage, strokeBrush.thickness); // ##NOTok

            // ale iterations
            doIterations();
        }

        private void doIterations() {
            int nStrokes = 0; // #nStrokes
            List<Stroke> strokes = new List<Stroke>(); // #strokes
            bool isNewPieceAccepted = false; // #accepted

            int mSize = InitialImage[0].Rows;
            int nSize = InitialImage[0].Columns;
            int kSize = InitialImage.Layers;

            Matrix3D syntheticSmearMap = new Matrix3D(mSize, nSize, kSize); // #canvas2

            for (int kk = 0; kk < settings.amountOfTotalIters; kk++) {
                double overlap = settings.minInitOverlapRatio; // coefficient of overlap of the stroke
                int minLen = strokeBrush.thickness * settings.minLenFactor[kk]; // min length of the stroke - used only on 1 iteration
                if (kk == 2)
                    InitialImage = UnblurredImage;

                if (kk > settings.minInitOverlapRatio)
                    overlap = settings.maxInitOverlapRatio;
                else
                    overlap = settings.minInitOverlapRatio;

                int[] pvect = Functions.Randperm(mSize, nSize);

                for (int iCounter = 0; iCounter < mSize; iCounter++) { //# ictr
                    for (int jCounter = 0; jCounter < nSize; jCounter++) { // #jctr

                        int pCounter = ((iCounter - 1) * nSize + jCounter); // #pctr
                        int pij = pvect[pCounter] - 1;
                        int j = pij % nSize; 
                        int i = (pij - j) / nSize; // (pij - j + 1) or (pij - j) ?

                        if ((Error[i, j, 0] > settings.pixTol) || (syntheticSmearMap[i, j, 0] == 0)) {
                            // if the deviation in pixel is large
                            int prevX = i; // pX
                            int prevY = j; // pY

                            //meancol is [r, g, b], col is the same but in shape of 3d array like (0,0,k) element

                            // find average color of the circle area with radius equals to brush radius
                            double[] meanColorPixel = Functions.getMeanColor( // #col? #meancol
                                InitialImage, prevX, prevY, strokeBrush.smallThickness, 
                                strokeBrush.bsQuad, mSize, nSize);

                            double[] hsvMeanColorPixel = new double[meanColorPixel.Length];

                            for (int m = 0; m < meanColorPixel.Length; m++)
                                hsvMeanColorPixel[m] = meanColorPixel[m] / 255;

                            hsvMeanColorPixel = Functions.rgb2hsv(hsvMeanColorPixel);

                            double[] proportions;
                            ColorMixType mixTypes;

                            // define proportions and type of the mixture in this area
                            Functions.PredictProportions(out proportions, out mixTypes, hsvMeanColorPixel, Ycell, Wcell, Ycell.Count);
                            // СТОИТ ПРОВЕРИТЬ ФУНКЦИЮ PredictProportions на изменения 
                            // proportions of paints in real values
                            double[] col8paints = Functions.proportions2pp(proportions, mixTypes);
                            // the volume of white in the paint mixture for sorting smears by overlap
                            double volumeOfWhite = col8paints[5];

                            var col2 = Functions.hsv2rgb(hsvMeanColorPixel);

                            // draw a synthetic map of smears with colour
                            // that fits the type of mixture

                            //double[] col2 = new double[3];
                            //var rand = new Random();
                            //rand.NextDouble();

                            //switch(mixTypes)
                            //{
                            //    case ColorMixType.MagentaYellow1:
                            //        col2 = new double[] {0.5 + 0.5 * rand.NextDouble(), 0.5 * rand.NextDouble(), 0.5 * rand.NextDouble() };
                            //        break;
                            //    case ColorMixType.MagentaYellow2:
                            //        col2 = new double[] { 0.5 + 0.5 * rand.NextDouble(), 0.5 + 0.5 * rand.NextDouble(), 0.5 * rand.NextDouble() };
                            //        break;
                            //    case ColorMixType.YellowCyan:
                            //        col2 = new double[] { 0.5 * rand.NextDouble(), 0.5 + 0.5 * rand.NextDouble(), 0.5 * rand.NextDouble() };
                            //        break;
                            //    case ColorMixType.CyanMagenta:
                            //        col2 = new double[] { 0, 0, 0.5 * rand.NextDouble() + 0.5 * rand.NextDouble() };
                            //        break;
                            //}

                            int nPoints = 1; // amount of points in the stroke #nPts
                            bool isStrokeEnded = false; // #endedStroke 

                            // copy canvases - to backup if the stroke is too short
                            Matrix3D canvasCopy = new Matrix3D(ColoredCanvas); // #canvas_copy
                            Matrix2D canvasgraymixCopyColorClass = new Matrix2D(ColorClass); // #canvasgraymix_copy first layer
                            Matrix2D canvasgraymixCopyVolumeOfWhite = new Matrix2D(VolumeOfWhite); // #canvasgraymix_copy first layer
                            Matrix3D canvas2Copy = new Matrix3D(syntheticSmearMap); // #canvas2_copy
                            // НАДО ПРОВЕРИТЬ ЧТО МАТРИЦЫ КОПИРУЮТСЯ

                            Stroke stroke = new Stroke(new System.Windows.Point(prevX, prevY), meanColorPixel, col8paints, proportions, mixTypes);
                            // one stroke consists of several strokes of the same type but other directions

                            while (!isStrokeEnded)
                            {
                                // %find new direction
                                int counter = 0; // #ctr index
                                StrokeCandidate candidate = new StrokeCandidate(-1, -1, Double.MaxValue);
                                while (counter < MaxAmountOfIters)
                                {
                                    double cosA = 0, sinA = 0;
                                    Functions.getDirection(new Point(prevX, prevY), 
                                        stroke, Gradients, settings.goNormal, out cosA, out sinA);
                                    
                                    int r = MaxAmountOfIters - counter - 1;

                                    candidate.x = prevX + Math.Round(r * cosA); // %new X
                                    candidate.y = prevY + Math.Round(r * sinA); // %new Y 

                                    // test new fragment of the stroke
                                    isNewPieceAccepted = Functions.testNewPieceAccepted( // #accepted
                                        startPoint: new Point(prevX, prevY),
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
                                   // else
                                //    {
                                //        //% also try opposite direction
                                //        candidate.x = prevX - Math.Round(r * cosA); // % new X
                                //        candidate.y = prevY - Math.Round(r * sinA); // % new Y

                                //        // % test new fragment of the stroke

                                //        isNewPieceAccepted = Functions.testNewPieceAccepted( // #accepted
                                //            startPoint: new System.Drawing.Point(prevX, prevY),
                                //            img: InitialImage,
                                //            canvasColor: settings.canvasColor,
                                //            canvasEps: settings.canvasColorFault,
                                //            canvas2: syntheticSmearMap,
                                //            ColorClass: ColorClass,
                                //            VolumeOfWhite: VolumeOfWhite,
                                //            pixTol: settings.pixTol,
                                //            pixTolAverage: settings.pixTolAverage,
                                //            meanColorPixel: meanColorPixel,
                                //            mSize: mSize,
                                //            nSize: nSize,
                                //            overlap: overlap,
                                //            bs2: strokeBrush.smallThickness,
                                //            bsQuad: strokeBrush.bsQuad,
                                //            mixTypes: mixTypes,
                                //            volumeOfWhite: volumeOfWhite,
                                //            ref candidate
                                //        );

                                //        if (isNewPieceAccepted) //%if error is small, accept the stroke immediately
                                //         counter = MaxAmountOfIters;
                                //    }
                                    counter++;
                                }
                                // %draw the stroke fragment

                                if (isNewPieceAccepted)
                                {
                                    Matrix3D coloredCanvas = ColoredCanvas;
                                    Matrix2D colorClass = ColorClass;
                                    Matrix2D volOfWhite = VolumeOfWhite;

                                    Matrix2D err = Functions.drawPiece(
                                            startPoint: new Point(prevX, prevY),
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
                                    stroke.length = stroke.length + (int)dlen;

                                    // %if length is too large
                                    int maxLen = strokeBrush.thickness * settings.maxLenFactor; // #maxLen - max length of the stroke
                                    if (stroke.length >= maxLen)
                                        isStrokeEnded = true;

                                    double vX = candidate.x;
                                    double vY = candidate.y;
                                    double pX = candidate.x;
                                    double pY = candidate.y;

                                    stroke.points.Add(new Point(vX, vY));
                                    
                                    nPoints++;
                                }
                                else
                                    isStrokeEnded = true;
                            }
                            if (nPoints > 1)
                            { 
                                if (stroke.length < minLen)
                                {
                                    // if the stroke is too short, and the iteration number
                                    // is for long non-overlapping strokes
                                    // backup canvases
                                    canvasCopy = new Matrix3D(ColoredCanvas); // #canvas_copy
                                    canvasgraymixCopyColorClass = new Matrix2D(ColorClass); // #canvasgraymix_copy first layer
                                    canvasgraymixCopyVolumeOfWhite = new Matrix2D(VolumeOfWhite); // #canvasgraymix_copy first layer
                                    canvas2Copy = new Matrix3D(syntheticSmearMap); // #canvas2_copy
                                }
                                else
                                {
                                    // if the stroke is appropriate

                                    nStrokes++;
                                    // WHAT ARE NEXT TWO STRINGS FOR? #?
                                    //double[] dcol = stroke.color;
                                    //stroke.color = new double[] { dcol[0], dcol[1], dcol[2] };
                                    strokes.Add(stroke);
                                }
                            }
                        }
                    }
                    // after each row iteration, show canvas
                    // #? show #canvas and #canvas2
                }
                // #? draw somwthing
            }
            // %%%%%%%%%%%%%%%%%%%
            // % create array for each mix type




        }
    }
}
