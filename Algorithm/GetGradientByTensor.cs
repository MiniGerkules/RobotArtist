using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;



namespace Algorithm
{

    public struct Gradient
    {
        double[] U;
        double[] V;

        public static /*Gradient*/ void GetGradientByTensor(RGBLayers imgGray, uint brushThickness) // StrokeBrush.thickness
        {
            int g = (int)brushThickness * 4;
            double p1 = 0.183;
            double[,] Dx = {
            { 0.5 * p1, 0, -0.5 * p1 },
            { 0.5 - p1, 0, p1 - 0.5  },
            { 0.5 * p1, 0, -0.5 * p1 }
        };
            double[,] Dy = { // transposed Dx
            { 0.5 * p1, 0.5 - p1, 0.5 * p1 },
            { 0,        0,        0        },
            {-0.5 * p1, p1 - 0.5, -0.5 * p1}
        };

            //double[,] fx = filter2(Dx, imgGray); 
            //double[,] fy = filter2(Dy, imgGray);

            //double[,] E = fx * fx; // matrix multiplication
            //double[,] F = fx * fy;
            //double[,] G = fy * fy;

            //double[,] h = fspecial('gaussian', 6, 0.1); // gaussian filter

            //E = imfilter(E, h);
            //F = imfilter(F, h);
            //G = imfilter(G, h);

            //D = sqrt((E - G).^ 2 + 4 * F.^ 2); // каждый элемент матриц E - G и 4*F возводится в квадрат
            //lam1 = 0.5 * (E + G + D);
            // ....
        }
    }
    // MODIFIED GRADIENT WITH A TENSOR APPROACH
//function[U, V] = GetGradientByTensor(imggray, brushSize)

    // TODO: function filter2, matrix and their operations, fspecial, imfilter

//g = 4* brushSize; % filter parameter

//p1 = 0.183;
//Dx = 0.5*[p1, 0, -p1;
//    1 - 2* p1, 0, 2* p1 - 1;
//    p1, 0, -p1];

//Dy = transpose(Dx);
//    fx = filter2(Dx, imggray);
//    fy = filter2(Dy, imggray);

//    E = fx.* fx;
//    F = fx.* fy;
//    G = fy.* fy;

//% %smoothing
//h = fspecial('gaussian', 6, 0.1);
//%h = fspecial('average', g);
//    E  = imfilter(E, h);
//    F  = imfilter(F, h);
//    G  = imfilter(G, h);

//    D = sqrt((E - G).^2 + 4* F.^2);
//    lam1 = 0.5* (E + G + D);
//%major eigenvector = gradient.If 0 make direction 1
//U = F.* (F ~= 0) + (F == 0);
//V = lam1 - E;
//V = V.* (V ~= 0) + (V == 0);
//%now, U and V are normalized

//%filter U and V
//H = fspecial('average', g);
//%H = fspecial('gaussian', g,3);
//    U  = imfilter(U, H,'replicate');
//    V  = imfilter(V, H,'replicate');

//    scl = sqrt(U.^2 + V.^2); %scaling factor
//scl = (scl == 0) + scl;
//U = U./scl;
//V = V./scl;
//end
}