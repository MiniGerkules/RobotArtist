using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using System.Drawing.Common;

namespace Algorithm
{
    public class Tracer
    {
        const uint depth = 3;
        public Settings settings { get; }
        public RGBLayers grayCanvas { get; private set; } // array m x n of canvasColor // is that ok
        public RGBLayers coloredCanvas { get; private set; } // array m x n x 3 of canvasColor
        public RGBLayers initialImage { get; private set; } // array m x n x 3 where m x n x 0 is a red layer, m x n x 1 is a green layer, m x n x 2 is a blue layer, ignore layer alpha
        public RGBLayers error { get; private set; }

        public RGBLayers grayInitialImage { get; private set; }
        
        public Tracer(Settings settings) 
        {
            // битмапы должны иметь размеры исходной картинки, один цвета canvasColor, другой цветной на таком же
            //canvasgray = (canvasColor + zeros(m, n, 1)); % gray version of canvas with tones
            //canvas = (canvasColor + zeros(m, n, 3)); % colored version
            initialImage = new RGBLayers(settings.image);
            grayCanvas = new RGBLayers(settings.canvasColor, 1);
            coloredCanvas = new RGBLayers(settings.canvasColor, 3);
            error = coloredCanvas - initialImage; // not doubled, for what? may be done later where it'll be needed in countings
            grayInitialImage = initialImage.rgb2gray();
            // strokes = cell(0);

        }
    }
}
