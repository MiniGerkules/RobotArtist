using Microsoft.VisualStudio.TestTools.UnitTesting;
using Algorithm;
namespace TestMatlabFunctionsAndOperations
{
    [TestClass]
    public class TestMatlabFunctionsAndOperations
    {

        [TestMethod]
        public void TestFunction_hsv2rgb1()
        {
            double[] hsv = { 0.4122,   0.7720,   0.1339 };
            double[] rgb = { 0.030530,   0.133882,   0.079433 }; 
            double[] testRGB = Functions.hsv2rgb(hsv);

            for (int i = 0; i < rgb.Length; i++)
                Assert.AreEqual(Math.Round(rgb[i], 4), Math.Round(testRGB[i], 4), " wrong on " + i + " index");
        }

        [TestMethod]
        public void TestFunction_hsv2rgb2()
        {
            double[] hsv = { 0.1545,   0.7829,   0.9513 };
            double[] rgb = { 0.95130000000000003446132268436486, 0.89693158779000003999470891358214, 0.206527229999999978327096528119 }; // using vpa
            double[] testRGB = Functions.hsv2rgb(hsv);

            for (int i = 0; i < rgb.Length; i++)
                Assert.AreEqual(rgb[i], testRGB[i], " wrong on " + i + " index");
        }

        [TestMethod]
        public void TestFunction_hsv2rgb3()
        {
            double[] hsv = { 0.1841,   0.3554,   0.4611 };
            double[] rgb = { 0.4440,   0.4611,   0.2972 };
            double[] testRGB = Functions.hsv2rgb(hsv);

            for (int i = 0; i < rgb.Length; i++)
                Assert.AreEqual(Math.Round(rgb[i], 4), Math.Round(testRGB[i], 4), " wrong on " + i + " index");
        }

        [TestMethod]
        public void TestFunction_GaussSolution5()
        {
            double[,] matrix = {{ 16,  2,    3,   13 },
                                { 5,  11,   10,    8 },
                                { 9,   7,    6,   12 },
                                { 4,  14,   15,    1 } };

            double[] solution = { 0, 1, 1, 0 };

            double[] answer = Functions.GaussSolution(matrix, solution);
            double[] rightResult = { 1 / 34d - 1 / 6d, 19 / 34d - 0.5, 0.5 - 9 / 17d, 1 / 6d};
            //{ 1/34 - c, 19/34 - 3c, 3c - 9/17, c}; - общее решение, где с - любое число, мой алгоритм взял с = 1/6

            Assert.AreEqual(answer.GetLength(0), rightResult.GetLength(0), "amount of elements is wrong");

            for (int i = 0; i < rightResult.GetLength(0); i++)
            {
                Assert.AreEqual(Math.Round(rightResult[i], 15), Math.Round(answer[i], 15), "wrong result for " + i + " element");
            }
        }

        [TestMethod]
        public void TestFunction_GaussSolution4()
        {
            double[,] matrix = {{ 2,  5,  4,  1 },
                                { 1,  3,  2,  1 },
                                { 2, 10,  9,  7 },
                                { 3,  8,  9,  2 }};

            double[] solution = { 20, 11, 40, 37 };

            double[] answer = Functions.GaussSolution(matrix, solution); // { -215, 38, -21, 2 };
            double[] rightResult = { 1, 2, 2, 0 };

            Assert.AreEqual(answer.GetLength(0), rightResult.GetLength(0), "amount of elements is wrong");

            for (int i = 0; i < rightResult.GetLength(0); i++)
            {
                Assert.AreEqual(rightResult[i], answer[i], "wrong result for " + i + " element");
            }
        }

        [TestMethod]
        public void TestFunction_GaussSolution3()
        {
            double[,] matrix = {{ 3,  2, -5 },
                                { 2, -1,  3 },
                                { 1,  2, -1 } };

            double[] solution = { -1, 13, 9 };

            double[] answer = Functions.GaussSolution(matrix, solution); // { -215, 38, -21, 2 };
            double[] rightResult = { 3, 5, 4 };

            Assert.AreEqual(answer.GetLength(0), rightResult.GetLength(0), "amount of elements is wrong");

            for (int i = 0; i < rightResult.GetLength(0); i++)
            {
                Assert.AreEqual(rightResult[i], answer[i], "wrong result for " + i + " element");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Solution of the system doesn't exist!")]
        public void TestFunction_GaussSolution2()
        {
            double[,] matrix = { { 1, 0, 0 },
                                 { 0, 1, 0 },
                                 { 0, 0, 0 } };

            double[] solution = { 1, 1, 1 };

            double[] answer = Functions.GaussSolution(matrix, solution);
            //double[] rightResult = { 1, 1, 0 };

            //Assert.ThrowsException<Exception>(() => Functions.GaussSolution(matrix, solution));
        }


        [TestMethod]
        public void TestFunction_GaussSolution1()
        {
            double[,] matrix = { { 1, 0, 0 },
                                 { 0, 1, 0 },
                                 { 0, 0, 1 } };

            double[] solution = { 1, 1, 1 };

            double[] answer = Functions.GaussSolution(matrix, solution);
            double[] rightResult = { 1, 1, 1 };

            Assert.AreEqual(answer.GetLength(0), rightResult.GetLength(0), "amount of elements is wrong");

            for (int i = 0; i < rightResult.GetLength(0); i++)
            {
                Assert.AreEqual(rightResult[i], answer[i], "wrong result for " + i + " element");
            }
        }

        [TestMethod]
        public void TestFunction_repmat()
        {
            int rowsRepeat = 3;
            int columnsRepeat = 1;
            double[] data = { 0, 1, 2 };
            
            double[,] test = Functions.repmat(data, rowsRepeat, columnsRepeat);

            double[,] right = { { 0, 1, 2 },
                                { 0, 1, 2 },
                                { 0, 1, 2 } };
            Assert.AreEqual(test.GetLength(0), right.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(test.GetLength(1), right.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < right.GetLength(0); i++)
            {
                for (int j = 0; j < right.GetLength(1); j++)
                {
                    Assert.AreEqual(right[i, j], test[i, j], "wrong result for " + i + ", " + j);
                }
            }
        }


        [TestMethod]
        public void TestFunction_rgb2hsv1()
        {
            double[] rgb = { 82, 0, 87 };
            double[] hsv = { 0.82375478927203060752759711249382, 1.0000,   87.0000 }; // using vpa in matlab answer
            
            double[] testHSV = Functions.rgb2hsv(rgb);

            for (int i = 0; i < rgb.Length; i++)
                Assert.AreEqual(hsv[i], testHSV[i], " wrong on " + i + " index");
        }

        [TestMethod]
        public void TestFunction_rgb2hsv2()
        {
            double[] rgb = { 1, 0, 0 };
            double[] hsv = { 0, 1, 1 };

            double[] testHSV = Functions.rgb2hsv(rgb);

            for (int i = 0; i < rgb.Length; i++)
                Assert.AreEqual(hsv[i], testHSV[i], " wrong on " + i + " index");
        }

        [TestMethod]
        public void TestFunction_linspace_0_0_1()
        {
            double start = 0;
            double end = 0;
            int amount = 1;
            double[] answer = { 0.0 };
            Assert.AreEqual(answer[0], Functions.linspace(start, end, amount)[0], "wrong resual for amount = 1");
        }

        [TestMethod]
        public void TestFunction_linspace_0_1_1()
        {
            double start = 0;
            double end = 1;
            int amount = 1;
            double[] answer = { 0.5 };
            Assert.AreEqual(answer[0], Functions.linspace(start, end, amount)[0], "wrong resual for amount = 1");
        }

        [TestMethod]
        public void TestFunction_linspace_minus2_2_4()
        {
            double start = -2;
            double end = 2;
            int amount = 5;
            double[] answer = { -2, -1, 0, 1, 2 };
            for (int i = 0; i < amount; i++)
            {
                Assert.AreEqual(answer[i], Functions.linspace(start, end, amount)[i], "wrong resual for amount = 1");
            }
        }

        [TestMethod]
        public void TestFunction_fspecial_0()
        {
            int squareSize = 1;
            double[,] actual = Functions.fspecial(0);
            double[,] expected = { { 1 } };
            Assert.AreEqual(squareSize, actual.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(squareSize, actual.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    Assert.AreEqual(expected[i, j], actual[i, j], "wrong result for amount = 1");
                }
            }
        }

        [TestMethod]
        public void TestFunction_fspecial_1()
        {
            int radius = 1;
            int squareSize = radius * 2 + 1;
            double[,] actual = Functions.fspecial(radius);
            double[,] expected = {
                { 0.025079,   0.145344,   0.025079 },
                { 0.145344,   0.318310,   0.145344 },
                { 0.025079,   0.145344,   0.025079 }
            };
            Assert.AreEqual(squareSize, actual.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(squareSize, actual.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    Assert.AreEqual(expected[i, j], Math.Round(actual[i, j], 6), "wrong result for radius = 1");
                }
            }
        }

        [TestMethod]
        public void TestFunction_fspecial_2()
        {
            int radius = 2;
            int squareSize = radius * 2 + 1;
            double[,] actual = Functions.fspecial(radius);
            double[,] expected = {
                            { 0,          0.017016,   0.038115,   0.017016,          0 },
                            { 0.017016,   0.078381,   0.079577,   0.078381,   0.017016 },
                            { 0.038115,   0.079577,   0.079577,   0.079577,   0.038115 },
                            { 0.017016,   0.078381,   0.079577,   0.078381,   0.017016 },
                            { 0,          0.017016,   0.038115,   0.017016,          0 }
            };
            Assert.AreEqual(squareSize, actual.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(squareSize, actual.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    Assert.AreEqual(expected[i, j], Math.Round(actual[i, j], 6), "wrong result for radius = 2");
                }
            }
        }

        [TestMethod]
        public void TestFunction_fspecial_3()
        {
            int radius = 3;
            int squareSize = radius * 2 + 1;
            double[,] actual = Functions.fspecial(radius);
            double[,] expected = {
            { 0,          0.000281,   0.011025,   0.017191,   0.011025,   0.000281,          0 },
            { 0.000281,   0.024517,   0.035368,   0.035368,   0.035368,   0.024517,   0.000281 },
            { 0.011025,   0.035368,   0.035368,   0.035368,   0.035368,   0.035368,   0.011025 },
            { 0.017191,   0.035368,   0.035368,   0.035368,   0.035368,   0.035368,   0.017191 },
            { 0.011025,   0.035368,   0.035368,   0.035368,   0.035368,   0.035368,   0.011025 },
            { 0.000281,   0.024517,   0.035368,   0.035368,   0.035368,   0.024517,   0.000281 },
            { 0,          0.000281,   0.011025,   0.017191,   0.011025,   0.000281,          0 },
            };
            Assert.AreEqual(squareSize, actual.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(squareSize, actual.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    Assert.AreEqual(expected[i, j], Math.Round(actual[i, j], 6), "wrong result for radius = 2");
                }
            }
        }

        [TestMethod]
        public void TestFunction_fspecial_4()
        {
            int radius = 4;
            int squareSize = radius * 2 + 1;
            double[,] actual = Functions.fspecial(radius);
            double[,] expected = {
                    { 0,           0,           0.000950,   0.007191,   0.009739,   0.007191,   0.000950,           0,          0 },
                    { 0,           0.004138,    0.017908,   0.019894,   0.019894,   0.019894,   0.017908,    0.004138,          0 },
                    { 0.000950,    0.017908,    0.019894,   0.019894,   0.019894,   0.019894,   0.019894,    0.017908,   0.000950 },
                    { 0.007191,    0.019894,    0.019894,   0.019894,   0.019894,   0.019894,   0.019894,    0.019894,   0.007191 },
                    { 0.009739,    0.019894,    0.019894,   0.019894,   0.019894,   0.019894,   0.019894,    0.019894,   0.009739 },
                    { 0.007191,    0.019894,    0.019894,   0.019894,   0.019894,   0.019894,   0.019894,    0.019894,   0.007191 },
                    { 0.000950,    0.017908,    0.019894,   0.019894,   0.019894,   0.019894,   0.019894,    0.017908,   0.000950 },
                    { 0,           0.004138,    0.017908,   0.019894,   0.019894,   0.019894,   0.017908,    0.004138,          0 },
                    { 0,           0,           0.000950,   0.007191,   0.009739,   0.007191,   0.000950,           0,          0 },
            };
            Assert.AreEqual(squareSize, actual.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(squareSize, actual.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    Assert.AreEqual(expected[i, j], Math.Round(actual[i, j], 6), "wrong result for radius = 2");
                }
            }
        }

        [TestMethod]
        public void TestFunction_fspecial_5()
        {
            int radius = 5;
            int squareSize = radius * 2 + 1;
            double[,] actual = Functions.fspecial(radius);
            double[,] expected = {
   {        0,          0,          0,   0.001250,   0.004967,   0.006260,   0.004967,   0.001250,          0,          0,          0 },
   {        0,   0.000032,   0.006157,   0.012396,   0.012732,   0.012732,   0.012732,   0.012396,   0.006157,   0.000032,          0 },
   {        0,   0.006157,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.006157,          0 },
   { 0.001250,   0.012396,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012396,   0.001250 },
   { 0.004967,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.004967 },
   { 0.006260,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.006260 },
   { 0.004967,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.004967 },
   { 0.001250,   0.012396,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012396,   0.001250 },
   {        0,   0.006157,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.012732,   0.006157,          0 },
   {        0,   0.000032,   0.006157,   0.012396,   0.012732,   0.012732,   0.012732,   0.012396,   0.006157,   0.000032,          0 },
   {        0,          0,          0,   0.001250,   0.004967,   0.006260,   0.004967,   0.001250,          0,          0,          0 }
            };
            Assert.AreEqual(squareSize, actual.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(squareSize, actual.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < squareSize; i++)
            {
                for (int j = 0; j < squareSize; j++)
                {
                    Assert.AreEqual(expected[i, j], Math.Round(actual[i, j], 6), "wrong result for radius = 2");
                }
            }
        }

        [TestMethod]
        public void TestFunction_conv2_full()
        {
            double[,] A = {

                { 0.4732,   0.2214,   0.3632 },
                { 0.2649,   0.8177,   0.4528 },
                { 0.8845,   0.6648,   0.1139 }
            };
            double[,] B = {
                { 0.467242,   0.929308,   0.066337,   0.714990 },
                { 0.708175,   0.802291,   0.163014,   0.766517 },
                { 0.278067,   0.386775,   0.811355,   0.046331 },
                { 0.803728,   0.821413,   0.885128,   0.431040 }
            };
            double[,] fullResult = {
                { 0.221109,   0.543197,   0.406804,   0.690551,   0.182364,   0.259680 },
                { 0.458913,   1.164694,   1.500999,   1.354708,   0.843568,   0.602174 },
                { 0.732493,   2.168847,   2.320200,   1.824030,   1.488447,   0.445353 },
                { 1.080404,   2.076939,   2.307969,   2.426839,   1.350370,   0.264811 },
                { 0.458893,   1.401811,   2.276601,   1.834387,   0.876474,   0.200470 },
                { 0.710912,   1.260911,   1.420542,   1.063268,   0.387361,   0.049080 },
            };
            double[,] conv2result = Functions.conv2(A, B, "full");
            int sizeX = A.GetLength(0) + B.GetLength(0) - 1;
            int sizeY = A.GetLength(1) + B.GetLength(1) - 1;
            Assert.AreEqual(sizeX, fullResult.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(sizeY, fullResult.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    Assert.AreEqual(Math.Round(fullResult[i, j], 3), Math.Round(conv2result[i, j], 3), "result matrix is wrong: " + i + " " + j + " doesnt't match");
                }
            }
        }

        [TestMethod]
        public void TestFunction_conv2_same()
        {
            double[,] A = {

                { 0.4732,   0.2214,   0.3632 },
                { 0.2649,   0.8177,   0.4528 },
                { 0.8845,   0.6648,   0.1139 }
            };
            double[,] B = {
                { 0.467242,   0.929308,   0.066337,   0.714990 },
                { 0.708175,   0.802291,   0.163014,   0.766517 },
                { 0.278067,   0.386775,   0.811355,   0.046331 },
                { 0.803728,   0.821413,   0.885128,   0.431040 }
            };
            double[,] sameResult = { // when using vpa in matlab
                { 2.8450830818999999216600826912327,   2.687038589599999749424341644044,   1.4785664572000001282248149436782 },
                { 2.2937906491000004116642685403349,   1.963874567399999770955787425919,   1.6122367208000001337353523922502 },
                { 1.5294693090999997853174363626749,   2.0920160898999999865566223888891,  1.3650301306999998995905798437889 } // first is 1.5295 because of rounds
            };
            double[,] conv2result = Functions.conv2(A, B, "same");
            int sizeX = A.GetLength(0);
            int sizeY = A.GetLength(1);
            Assert.AreEqual(sizeX, sameResult.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(sizeY, sameResult.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    Assert.AreEqual(sameResult[i, j], conv2result[i, j], 3, "result matrix is wrong: " + i + " " + j + " doesnt't match");
                }
            }
        }

        [TestMethod]
        public void TestFunction_conv2_replicate_squares()
        {
            double[,] A = {

                   { 17,   24,    1,    8,   15 },
                   { 23,    5,    7,   14,   16 },
                   {  4,    6,   13,   20,   22 },
                   { 10,   12,   19,   21,    3 },
                   { 11,   18,   25,    2,    9 }
            };
            double[,] B = {
                {  1,   2,  -1 },
                {  0,   2,   3 },
                { -2,   4,  -1 }
            };
            double[,] replicateResult = {
                { 174,   82,    48,    89,   118 },
                { 90,    98,    94,   110,   129 },
                { 98,    86,   122,   168,   107 },
                { 66,   109,   175,    31,    80 },
                { 98,   151,   147,    38,    92 }
            };
            double[,] conv2result = Functions.conv2(A, B, "replicate");
            int sizeX = A.GetLength(0);
            int sizeY = A.GetLength(1);
            Assert.AreEqual(sizeX, replicateResult.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(sizeY, replicateResult.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    Assert.AreEqual(Math.Round(replicateResult[i, j]), Math.Round(conv2result[i, j]), "result matrix is wrong: " + i + " " + j + " doesnt't match");
                }
            }
        }

        [TestMethod]
        public void TestFunction_conv2_same_int()
        {
            double[,] A = {
                   { 4,  8,  3,  1,  1 },
                   { 7,  2,  8,  6,  9 },
                   { 1,  5,  1,  5,  4 },
                   { 3,  8,  8,  2,  5 }
            };
            double[,] B = {
                  { 9,  6 },
                  { 8,  1 },
                  { 0,  5 }
            };
            double[,] sameResult = {
                  {  50,  107,   55,   54,   8 }, 
                  { 167,   119,  128,   92,  81 }, // incorrect all
                  {  128,  147,   131,  177,  113 }, // incorrect all
                  {  71,   123,   105,   90,  76 }
            };
            double[,] conv2result = Functions.conv2(A, B, "same");
            int sizeX = A.GetLength(0);
            int sizeY = A.GetLength(1);
            Assert.AreEqual(sizeX, sameResult.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(sizeY, sameResult.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    Assert.AreEqual(sameResult[i, j], conv2result[i, j], "result matrix is wrong: " + i + " " + j + " doesnt't match");
                }
            }
        }

        [TestMethod]
        public void TestFunction_conv2_replicate_int()
        {
            double[,] A = {
                   { 4,  8,  3,  1,  1 },
                   { 7,  2,  8,  6,  9 },
                   { 1,  5,  1,  5,  4 },
                   { 3,  8,  8,  2,  5 }
            };
            double[,] B = {
                  { 9,  6 },
                  { 8,  1 },
                  { 0,  5 }
            };
            double[,] replicateResult = {
                  { 134,  197,   88,   69,   69 },
                  { 167,  119,  128,   92,  116 },
                  { 128,  147,  131,  177,  196 },
                  { 111,  163,  115,  115,  130 }
            };
            double[,] conv2result = Functions.conv2(A, B, "replicate");
            int sizeX = A.GetLength(0);
            int sizeY = A.GetLength(1);
            Assert.AreEqual(sizeX, replicateResult.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(sizeY, replicateResult.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    Assert.AreEqual(Math.Round(replicateResult[i, j]), Math.Round(conv2result[i, j]), "result matrix is wrong: " + i + " " + j + " doesnt't match");
                }
            }
        }


        [TestMethod]
        public void TestFunction_conv2_replicate_double() // don't pass with big round because rounds are not equal
        {
            double[,] A = {

                      { 0.629613,   0.754613,   0.782659,   0.725452,   0.297485,   0.594475,   0.183090 },
                      { 0.736023,   0.792248,   0.026958,   0.275014,   0.796442,   0.694579,   0.177453 },
                      { 0.560366,   0.569297,   0.311960,   0.428138,   0.646303,   0.470123,   0.096446 },
                      { 0.320281,   0.311513,   0.073803,   0.944514,   0.457346,   0.663887,   0.128626 },
                      { 0.579592,   0.412875,   0.521244,   0.060756,   0.133461,   0.744121,   0.086037 },
                      { 0.369368,   0.944330,   0.071442,   0.040557,   0.931072,   0.788090,   0.152904 }
            };
            double[,] B = {
                { 0.2373,   0.7980 },
                { 0.5794,   0.8573 },
                { 0.9508,   0.2305 }
            };
            double[,] replicateResult = {
                   { 2.6457,   2.6713,   1.9290,   1.5299,   2.1443,   1.4899,   0.6622 },
                   { 2.5212,   1.8989,   1.4113,   1.8077,   2.3247,   1.3110,   0.5584 },
                   { 1.9959,   1.1200,   1.0615,   2.5064,   2.1086,   1.3224,   0.4742 },
                   { 1.6861,   1.1405,   1.7777,   1.6452,   1.6610,   1.4108,   0.3863 },
                   { 1.5832,   1.7332,   1.2026,   0.9918,   2.4204,   1.5497,   0.4374 },
                   { 2.0594,   2.0367,   0.3256,   1.1957,   2.9074,   1.6175,   0.4894 }
            };
            double[,] conv2result = Functions.conv2(A, B, "replicate");
            int sizeX = A.GetLength(0);
            int sizeY = A.GetLength(1);
            Assert.AreEqual(sizeX, replicateResult.GetLength(0), "amount of rows is wrong");
            Assert.AreEqual(sizeY, replicateResult.GetLength(1), "amount of colomns is wrong");
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    Assert.AreEqual(Math.Round(replicateResult[i, j], 2), Math.Round(conv2result[i, j], 2), "result matrix is wrong: " + i + " " + j + " doesnt't match");
                }
            }
        }

    }
}