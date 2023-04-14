using System;
using System.Linq;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

using GeneralComponents;
using GeneralComponents.PLT;
using GeneralComponents.Colors;

namespace Algorithm {
    public class Tracer {
        public StrokeBrush strokeBrush { get; private set; }
        public int MaxAmountOfIters { get; private set; }
        public BitmapImage Image { get; private set; }
        public Settings settings { get; private set; }
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

        Database database;

        public Tracer(Database database) {
            this.database = database;
            CMYBWColor.Database = database;
        }

        public PLTDecoderRes Trace(BitmapImage img, Settings settings) {
            this.settings = settings;

            Image = img;

            strokeBrush = new StrokeBrush(settings.guiTrace.brushWidthMM,
               Math.Min(settings.guiTrace.canvasWidthMM / Image.PixelWidth, // #sfX Image.PixelWidth should be greater than settings.guiTrace.canvasWidthMM
               settings.guiTrace.canvasHeightMM / Image.PixelHeight)); // #sfY
            MaxAmountOfIters = strokeBrush.thickness; // the max amount of attempts to find a new line section
            InitialImage = Functions.imageToMatrix3D(Image); // ##ok

            Ycell = database.GetHSV().Copy(); // ##ok
            Wcell = database.GetProportions().Copy(); // ##ok

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

            Gradients = new Gradient(GrayInitialImage, strokeBrush.thickness); // ##ok

            // ale iterations
            return doIterations();
        }

        public Tracer(BitmapImage img, Settings settings, Database database) // settings shouldn't be null! 
        {
            this.settings = settings;

            Image = img;

            strokeBrush = new StrokeBrush(settings.guiTrace.brushWidthMM,
               Math.Min(settings.guiTrace.canvasWidthMM / Image.PixelWidth, // #sfX Image.PixelWidth should be greater than settings.guiTrace.canvasWidthMM
               settings.guiTrace.canvasHeightMM / Image.PixelHeight)); // #sfY
            MaxAmountOfIters = strokeBrush.thickness; // the max amount of attempts to find a new line section
            InitialImage = Functions.imageToMatrix3D(Image); // ##ok

            this.database = database;
            Ycell = database.GetHSV().Copy(); // ##ok
            Wcell = database.GetProportions().Copy(); // ##ok

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

            Gradients = new Gradient(GrayInitialImage, strokeBrush.thickness); // ##ok

            // ale iterations
            doIterations();
        }

        private PLTDecoderRes doIterations() {
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

                int[] pvect = Functions.Randperm(mSize * nSize, mSize * nSize);
                //int[] pvect = new int[] {
                //    94,  44,  81,  61,  41,  65,  71,  28,  36,  69,
                //    96,  18,  91,  24,  16,  80,  11,  35,  17,  19,
                //    14,  54,  49,  92,  29,  66,  32,  42,  37,  12,
                //    21,  38,  78,  67,  5,   59,  2,   58,  30,  39,
                //    23,  20,  27,  50,  13,  100, 84,  47,  55,  95,
                //    73,  83,  99,  40,  53,  4,   52,  8,   90,  48,
                //    10,  31,  33,  86,  87,  9,   56,  1,   72,  57,
                //    68,  79,  34,  98,  75,  70,  46,  82,  74,  64,
                //    43,  89,  3,   15,  88,  7,   62,  93,  77,  76,
                //    85,  25,  63,  22,  45,  6,   51,  97,  26,  60
                //};

                for (int iCounter = 0; iCounter < mSize; iCounter++) { //# ictr
                    for (int jCounter = 0; jCounter < nSize; jCounter++) { // #jctr

                        int pCounter = (iCounter * nSize + jCounter); // #pctr
                        int pij = pvect[pCounter] - 1;
                        int j = pij % nSize; 
                        int i = (pij - j) / nSize; // (pij - j + 1) or (pij - j) ?

                        if ((Error[i, j, 0] > settings.pixTol) || (syntheticSmearMap[i, j, 0] == 0)) {
                            // if the deviation in pixel is large
                            int prevX = i; // #pX
                            int prevY = j; // #pY

                            //meancol is [r, g, b], col is the same but in shape of 3d array like (0,0,k) element

                            // find average color of the circle area with radius equals to brush radius
                            double[] meanColorPixel = Functions.getMeanColor( // #col? #meancol // ##ok
                                InitialImage, prevX, prevY, strokeBrush.smallThickness, 
                                strokeBrush.bsQuad, mSize, nSize);

                            double[] hsvMeanColorPixel = new double[meanColorPixel.Length];

                            for (int m = 0; m < meanColorPixel.Length; m++)
                                hsvMeanColorPixel[m] = meanColorPixel[m] / 255;

                            hsvMeanColorPixel = Functions.rgb2hsv(hsvMeanColorPixel); // ##ok

                            double[] proportions;
                            double[] hsvNewColor;
                            ColorMixType mixTypes;

                            // define proportions and type of the mixture in this area
                            Functions.PredictProportions(out proportions, out mixTypes, out hsvNewColor, 
                                hsvMeanColorPixel, database.GetHSV().Copy(), database.GetProportions().Copy(), Ycell.Count); // ##ok
                            // proportions of paints in real values
                            double[] col8paints = Functions.proportions2pp(proportions, mixTypes);
                            // the volume of white in the paint mixture for sorting smears by overlap
                            double volumeOfWhite = col8paints[4]; // ##ok

                            var col2 = Functions.hsv2rgb(hsvNewColor); // ##ok

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

                            Stroke stroke = new Stroke(new System.Windows.Point(prevX, prevY), meanColorPixel, col8paints, proportions, mixTypes);
                            // one stroke consists of several strokes of the same type but other directions

                            while (!isStrokeEnded)
                            {
                                // %find new direction
                                int counter = 1; // #ctr index
                                StrokeCandidate candidate = new StrokeCandidate(-1, -1, Double.MaxValue);
                                while (counter < MaxAmountOfIters)
                                {
                                    double cosA = 0, sinA = 0;
                                    Functions.getDirection(new Point(prevX, prevY), 
                                        stroke, Gradients, settings.goNormal, out cosA, out sinA);
                                    
                                    int r = MaxAmountOfIters - counter;

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

                                    counter++;

                                    if (isNewPieceAccepted) //%if error is small, accept the stroke immediately
                                        counter = MaxAmountOfIters;
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
                                    stroke.length = stroke.length + dlen;

                                    // %if length is too large
                                    int maxLen = strokeBrush.thickness * settings.maxLenFactor; // #maxLen - max length of the stroke
                                    if (stroke.length >= maxLen)
                                        isStrokeEnded = true;

                                    double vX = candidate.x;
                                    double vY = candidate.y;
                                    prevX = (int)candidate.x;
                                    prevY = (int)candidate.y;

                                    stroke.points.Add(new Point(vX, vY));
                                    
                                    nPoints++;
                                }
                                else
                                    isStrokeEnded = true;
                            }
                            if (nPoints > 1) // ##ok
                            { 
                                if (stroke.length < minLen)
                                {
                                    // if the stroke is too short, and the iteration number
                                    // is for long non-overlapping strokes
                                    // backup canvases
                                    ColoredCanvas = new Matrix3D(canvasCopy); // #canvas_copy
                                    ColorClass = new Matrix2D(canvasgraymixCopyColorClass); // #canvasgraymix_copy first layer
                                    VolumeOfWhite = new Matrix2D(canvasgraymixCopyVolumeOfWhite); // #canvasgraymix_copy first layer
                                    syntheticSmearMap = new Matrix3D(canvas2Copy); // #canvas2_copy
                                }
                                else
                                {
                                    // if the stroke is appropriate

                                    nStrokes++;
                                    // WHAT ARE NEXT TWO STRINGS FOR? #?
                                    //double[] dcol = stroke.color;
                                    //stroke.color = new double[] { dcol[0], dcol[1], dcol[2] };
                                    strokes.Add(stroke);
                                    // %new message, update message bar
                                }
                            }
                        }
                    }
                    // %after each row iteration, show canvas
                    // #? show #canvas and #canvas2
                }
                // #? draw somwthing
            }
            // %%%%%%%%%%%%%%%%%%%
            // % create array for each mix type
            // ##ok
            int amountOfMixGroups = Ycell.Count; // #nmixgroups
            List<List<double[]>> proportionsByMixGroups = new List<List<double[]>>(amountOfMixGroups); // #mixCell // % [props, cls]
            List<List<double[]>> colorByMixGroups = new List<List<double[]>>(amountOfMixGroups); // #colCell
            List<List<int>> mixGroupsId = new List<List<int>>(amountOfMixGroups); // #idcell

            for (int i = 0; i < amountOfMixGroups; i++)
            {
                proportionsByMixGroups.Add(new List<double[]>());
                colorByMixGroups.Add(new List<double[]>());
                mixGroupsId.Add(new List<int>());
            }

            for (int i = 0; i < nStrokes; i++)
            {
                ColorMixType currentMixType = strokes[i].mixType;
                proportionsByMixGroups[(int)currentMixType].Add(strokes[i].proportions); // % proportions array
                colorByMixGroups[(int)currentMixType].Add(strokes[i].color); // % color array
                mixGroupsId[(int)currentMixType].Add(i); // %real order of stroke in cell array
            }
            // %now, rescale K for each mix group and perform internal k-means among
            // % proportions
            uint amountOfColors = 0; // #nColors2
            List<int> globalIndexes = new List<int>(); // #ids
            List<ColorMixType> colorMixTypes = new List<ColorMixType>(); // #cls
            List<double[]> mix8colorsValues = new List<double[]>(); // #mixvalues // %arrange col8 // double[8] - color
            List<double[]> finalColors = new List<double[]>(); // #colorsFinal // %arrange RGB colors // double[3] - color
            List<double[]> proportionValues = new List<double[]>(); // #propvalues // %arrange proportions // double[3] - proportions
            List<int> indexesToIndexes = new List<int>(); // #i2i
            int clustersCounter = 0; // #ctr

            // %cluster colors by mixture characteristics

            uint nColors = settings.guiTrace.colorsAmount;
            for (int i = 0; i < amountOfMixGroups; i++)
            {
                indexesToIndexes.AddRange(mixGroupsId[i]);
                uint Ncl = (uint)proportionsByMixGroups[i].Count;
                List<double[]> colArray = colorByMixGroups[i];

                if (Ncl > 0)
                {
                    uint Ki = (uint)Math.Ceiling((Ncl / (double)nStrokes )* nColors); // % number of clusters for a given type of mixture, proportional to the number of strokes
                    List<int> indexes = new List<int>(); // #idsk
                    if (Ki >= Ncl)
                    {
                        Ki = Ncl;
                        for (int j = 0; j < Ncl; j++)
                            indexes.Add(j);
                    }
                    else
                    {
                        indexes = Functions.kmeans(colArray, Ki);
                    }

                    amountOfColors += Ki;
                    List<double[]> finalColor = new List<double[]>(); // #colorFinal
                    for (int j = 0; j < Ki; j++)
                        finalColor.Add(new double[3] {0, 0, 0} );
                    int[] nels = new int[Ki];
                    for (int j = 0; j < Ncl; j++)
                    {
                        for (int k = 0; k < colArray[j].Length; k++)
                            finalColor[indexes[j]][k] += colArray[j][k];
                        nels[indexes[j]]++;
                    }
                    // #ok

                    // %averaging proportions in each cluster

                    // %averaging colors in each cluster
                    for (int c = 0; c < Ki; c++)
                        for (int j = 0; j < finalColor[c].Length; j++)
                            finalColor[c][j] /= (nels[c] * 255); // #ok
                    // %transform proportions into colors
                    List<double[]> hsvColors = new List<double[]>();
                    for (int c = 0; c < finalColor.Count; c++)
                        hsvColors.Add(Functions.rgb2hsv(finalColor[c])); // #ok
                    List<double[]> propsFinal = new List<double[]>();
                    List<ColorMixType> mixType = new List<ColorMixType>(); // #mixtyps
                    double[] skip;
                    List<double[]> colorsFinali = new List<double[]>();
                    List<double[]> mixValuesi = new List<double[]>(); // #mixvaluesi
                    for (int c = 0; c < hsvColors.Count; c++)
                    {
                        double[] currentProps;
                        ColorMixType currentMixType;
                        Functions.PredictProportions(out currentProps, out currentMixType, out skip, hsvColors[c], Ycell, Wcell); // ##ok
                        propsFinal.Add(currentProps);
                        mixType.Add(currentMixType);
                        double[] colFinali = Functions.hsv2rgb(Functions.prop2hsv(currentProps, currentMixType, Wcell, Ycell));
                        for (int k = 0; k < colFinali.Length; k++)
                            colFinali[k] = Math.Round(colFinali[k] * 255);
                        colorsFinali.Add(colFinali);
                        
                        
                        colorMixTypes.Add(currentMixType);
                    }

                    for (int c = 0; c < Ncl; c++)
                    {
                        mixValuesi.Add(Functions.proportions2pp(propsFinal[c], (ColorMixType)i));
                        globalIndexes.Add(indexes[c] + clustersCounter); // % global indices
                    }
                    // ##ok

                    mix8colorsValues.AddRange(mixValuesi); //% 8 colors
                    finalColors.AddRange(colorsFinali); //% final colors
                    proportionValues.AddRange(propsFinal); // % proportions of the mixtures

                    clustersCounter += (int)Ki;
                }
            }
            nColors = amountOfColors;
            // %re-order ids
            List<int> reorderedIdexes = new List<int>(); // #idsnew
            reorderedIdexes.AddRange(globalIndexes);
            for (int i = 0; i < nStrokes; i++)
                reorderedIdexes[indexesToIndexes[i]] = globalIndexes[i];
            globalIndexes.Clear();
            globalIndexes.AddRange(reorderedIdexes);
            // ##ok

            //% create mix groups

            List<List<int>> listsByMixGroups = new List<List<int>>(amountOfMixGroups); // #iclscell
            for (int j = 0; j < amountOfMixGroups; j++)
                listsByMixGroups.Add(new List<int>());

            for (int j = 0; j < nColors; j++)
                listsByMixGroups[(int)colorMixTypes[j]].Add(j); // append new value of color mix to list of colorMixTypes[j]

            List<int> icls = new List<int>(); // %indices of values sorted by class
            // ## ok
            for (int j = 0; j < amountOfMixGroups; j++)
                icls.AddRange(listsByMixGroups[j]);

            // %sort by amount of white color - 5th row - in the overall mix
            // ##ok
            List<int> irow = new List<int>();
            List<double[]> mxall = new List<double[]>();
            int ctr = 0;
            for (int j = 0; j < amountOfMixGroups; j++)
            {
                if (listsByMixGroups[j].Count > 0)
                {
                    List<double> listToSort = new List<double>();
                    List<double[]> mxarray = new List<double[]>();
                    for (int index = 0; index < listsByMixGroups[j].Count; index++)
                    {
                        mxarray.Add(mix8colorsValues[(int)listsByMixGroups[j][index]]);
                        listToSort.Add(mix8colorsValues[(int)listsByMixGroups[j][index]][4]);
                    }
                    mxall.AddRange(mxarray);
                    // надо взять 4 столбец и его сортануть: (int)listsByMixGroups[все][4]
                    var sorted = listToSort // %iarray is array of indices how strokes are actually organized
                        .Select((elem, index) => new KeyValuePair<double, int>(elem, index)) // key = element, value = it's index
                        .OrderBy(elem => elem.Key)
                        .ToList();
                    List<int> iarray = sorted
                        .Select(pair => pair.Value)
                        .ToList();
                    foreach (int index in iarray)
                        irow.Add(icls[ctr + index]); // %irow is a sorting order of clusters

                    ctr += iarray.Count;
                }
            }

            // % then, obrain correct order of strokes by ind
            int[] ind = new int[nStrokes];
            ctr = 0; 
            for (int i = 0; i < nColors; i++)
            {
                int i_corr = irow[i];
                for (int j = 0; j < nStrokes; j++)
                {
                    if (globalIndexes[j] == i_corr)
                    {
                        ind[ctr] = j;
                        ctr++;
                    }
                }
            }

            int[,] strokes2 = new int[nColors, nStrokes];
            int[] strCount = new int[nColors];
            // map2 is unused wtf
            
            for (int i = 0; i < nStrokes; i++)
            {
                int i_cluster = globalIndexes[ind[i]];
                int i_sorted = ind[i];
                strokes[i_sorted].color = finalColors[i_cluster];
                strokes[i_sorted].col8paints = mix8colorsValues[i_cluster];
                strokes[i_sorted].proportions = proportionValues[i_cluster];

                strCount[i_cluster] += (strCount[i_cluster] == 0) ? 0 : 1;
                strokes2[i_cluster, strCount[i_cluster]] = i_sorted;
            }

            // %sort strokes by position in each cluster, to reduce the machine tool path
            int[,] strokes3 = strokes2.Clone() as int[,];
            for (int i = 0; i < nColors; i++) // % for each cluster
            {
                int curStrokeId = strokes2[i, 0]; // %index in strokes
                strokes2[i, 0] = 0; // %0, because the i-th stroke is already taken
                int j = 1;
                while (j < strCount[i]) // %for the strokes in a cluster
                {
                    // looking for pair to curStroke
                    Stroke curStroke = strokes[curStrokeId];
                    Point curPoint = curStroke.points.Last(); // #curX #curY
                    double distMin = double.MaxValue;
                    int nextStrokeId = curStrokeId;
                    int k = 0;
                    int nextNumber = 0;
                    while (k < strCount[i])
                    {
                        if (strokes2[i, k] != 0)
                        {
                            Stroke candidate = strokes[strokes2[i, k]];
                            double dist = Math.Sqrt(
                                (curPoint.X - candidate.points[0].X) * 
                                (curPoint.X - candidate.points[0].X) + 
                                (curPoint.Y - candidate.points[0].Y) * 
                                (curPoint.Y - candidate.points[0].Y));
                            if (dist < distMin)
                            {
                                distMin = dist;
                                nextStrokeId = strokes2[i, k];
                                nextNumber = k;
                            }
                        }
                        k++;
                    }
                    strokes3[i, j] = nextStrokeId;
                    curStrokeId = nextStrokeId;
                    strokes2[i, nextNumber] = 0; // %processed
                    j++;
                }
            }

            // %put them into the array again
            List<Stroke> map = new List<Stroke>(nStrokes);
            for (int i = 0; i < nColors; i++)
            {
                int iCol = irow[i]; // %correct order of clusters
                for (int j = 0; j <= strCount[iCol]; j++)
                    map.Add(strokes[strokes3[iCol, j]]);
            }
            // now in map we have the right ordered strokes we want to paint 
            //imshow(map2imgColorCanvas(brushSize, canvas, map, canvasColor));
            Matrix3D finalImage = Functions.strokesToImage(strokeBrush.thickness, ColoredCanvas, map, settings.canvasColor);
            
            string filename = @"C:\Users\varka\Documents\RobotArtist extra\commands.txt";
            Functions.SavePLT_8paints(map, InitialImage[0].Columns, InitialImage[0].Rows, settings.guiTrace.canvasWidthMM, settings.guiTrace.canvasHeightMM, settings.guiTrace.brushWidthMM, filename);
            
            return Convert(map);
        }

        private PLTDecoderRes Convert(List<Stroke> map) {
            throw new NotImplementedException();
        }
    }
}
