using GeneralComponents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Functions
{
    internal class ProportionsProcessing
    {
        /// <summary>
        /// Predicts possible proportions, mixtype and hsv-color for data hsvColor
        /// </summary>
        /// <returns> Returns renewed error matrix </returns>
        internal static void PredictProportions(out List<double[]> proportions, out List<ColorMixType> mixTypes,
            out List<double[]> hsvNewColor, List<double[]> hsvColor, List<List<List<double>>> Ycell,
            List<List<List<double>>> Wcell, int sheetsAmount = Constants.ClustersAmount)
        {
            int K = 25;
            double tolerance = 0.2; // #tol - tolerance of Color Error in rgb

            List<List<double>> Tbl = new List<List<double>>();
            List<int> Clss = new List<int>(); // one big column of indicies

            for (int i = 0; i < sheetsAmount; i++)
            {
                int Nset = Ycell[i].Count;
                Tbl.AddRange(Ycell[i]);
                Clss.AddRange(Enumerable.Repeat(i, Nset));
            }

            int N = Tbl.Count;
            int Nhsv = hsvColor.Count; // #Nhsv

            //now, create classification table and class list
            hsvNewColor = new List<double[]>(); // #hsvnew
            hsvNewColor.AddRange(hsvColor);
            proportions = new List<double[]>(Nhsv);
            mixTypes = new List<ColorMixType>(Nhsv); // #cls
            List<double[]> hsvArray = new List<double[]>(Nhsv);

            for (int i = 0; i < Nhsv; i++)
            {
                proportions.Add(new double[Functions.Constants.ColorLength]);
                mixTypes.Add((ColorMixType)0);
                hsvArray.Add(new double[Functions.Constants.ColorLength]);
            }

            Matrix2D M = (new Matrix2D(new List<double> { 1d, 1d / 16, 1d })).MakeDiag();
            double[] hsvColorCurrent = new double[Functions.Constants.ColorLength];
            for (int i = 0; i < Nhsv; i++)
            {
                hsvNewColor[i] = (double[])hsvColor[i].Clone();
                Array.Copy(hsvColor[i], hsvColorCurrent, Constants.ColorLength);

                // make prediction from the closest point
                int Ks = 10;
                Matrix2D dst = new Matrix2D(N, 1); // distances - taken as maximum range

                for (int n = 0; n < N; n++)
                    dst[n] = ArraysManipulations.Distance(hsvColorCurrent, Tbl[n].ToArray()); // #ok

                int[] I = dst.GetIndexesForSorted();
                int[] classes = { 0, 0, 0, 0 };

                for (int k = 0; k < Ks; k++)
                    classes[Clss[I[k]]]++;

                int clsvect = classes.ToList().IndexOf(classes.Max());
                mixTypes[i] = (ColorMixType)clsvect; // #cls(i) (i = 1)
                                                     // then, find second possible class
                int ctr = 0; // index
                int ctri = 0; // index
                Matrix2D class2vect = new Matrix2D(K, 1);
                while (ctr < Clss.Count && ctri < K)
                {
                    if ((ColorMixType)Clss[I[ctr]] != mixTypes[i])
                    {
                        class2vect[ctri] = Clss[I[ctr]];
                        ctri++;
                    }
                    ctr++;
                }

                classes = Enumerable.Repeat(0, Constants.ClustersAmount).ToArray();

                for (int k = 0; k < ctri; k++)
                    classes[(int)class2vect[k]]++;

                int class2 = classes.ToList().IndexOf(classes.Max()); // ##ok
                bool flag = true;
                ctr = 1; // counter
                double[] props0 = { 0, 0, 0 }; // initial Proportions
                double[] propscur = { 0, 0, 0 }; // current Proportions
                double err0 = 0;

                double[] hsvinv = { 0, 0, 0 };
                Matrix2D h;

                while (flag)
                {
                    List<List<double>> Y = Ycell[(int)mixTypes[i]];
                    List<List<double>> W = Wcell[(int)mixTypes[i]];
                    int NY = Y.Count; // amount of rows in Y
                    Matrix2D dist = new Matrix2D(NY, 1);
                    for (int n = 0; n < NY; n++)
                    {
                        Matrix2D hsvColorCurrentMatrix = new Matrix2D(hsvColorCurrent.ToList());
                        Matrix2D Yn = new Matrix2D(Y[n]);
                        Matrix2D temp = (hsvColorCurrentMatrix - Yn);
                        dist[n, 0] = (double)(temp * M * temp.Transpose());
                    }
                    int[] indexes = dist.GetIndexesForSorted();

                    Matrix2D d = new Matrix2D(1, K);
                    Matrix2D ds = new Matrix2D(1, K); // transposed

                    for (int p = 0; p < K; p++)
                    {
                        d[0, p] = 1d / dist[indexes[p]];
                        ds[0, p] = Math.Sqrt(d[p]);
                    }

                    Matrix2D D = d.MakeDiag(); // ##ok
                    // hsvpossible = ds'*Y(I(1:K),:)/sum(ds); 
                    List<List<double>> Y_IK = new List<List<double>>();
                    for (int k = 0; k < K; k++)
                        Y_IK.Add(Y[indexes[k]]);
                    Matrix2D hsvpossible = ds * (new Matrix2D(Y_IK)) / ds.GetSum();
                    double al = 0.7;
                    for (int k = 0; k < hsvNewColor[i].Length; k++)
                        hsvNewColor[i][k] = hsvpossible[k] * (1 - al) + hsvColorCurrent[k] * al; //#ok

                    hsvColorCurrent = (double[])hsvNewColor[i].Clone(); // ##ok

                    // take first K Points, make linear regression
                    for (int j = 0; j < Constants.ColorLength; j++)
                    {
                        Matrix2D X = new Matrix2D(Y_IK);

                        Matrix2D E = new Matrix2D(K, Constants.ClustersAmount); // evaluated polynomial
                        List<List<double>> T = new List<List<double>> {
                            new List<double> { 0, 0, 0 },
                            new List<double> { 1, 0, 0 },
                            new List<double> { 0, 1, 0 },
                            new List<double> { 0, 0, 1 }
                        }; // monomial orders
                        h = Matrix2D.Eye(4);

                        for (int k = 0; k < 4; k++)
                        {
                            Matrix2D product = Matrix2D.MulByRows(X.Pow(new Matrix2D(T[k])).RepeatRows(K));
                            for (int r = 0; r < K; r++)
                            {
                                for (int c = 0; c < E.Columns; c++)
                                {
                                    E[r, c] += product[r] * h[k, c];
                                }
                            }
                        }

                        double delt = 1e-14; // for Tikhonov regularization

                        Matrix2D Wj = new Matrix2D(K, 1);

                        for (int k = 0; k < K; k++)
                            Wj[k] = W[indexes[k]][j];

                        // h = (E'*D*E)\(E'*D*W(I(1:K),j)); // WLS
                        Matrix2D Etransposed = E.Transpose();

                        Matrix2D answers = Etransposed * D * Wj;

                        Matrix2D coefs = Etransposed * D * E + (Matrix2D.Eye(4)) * delt;

                        h = GausMethod.Solve(coefs, answers); // answers don't match, but they pretty close

                        // predict proportion
                        double p = 0;

                        for (int k = 0; k < 4; k++)
                            p += h[k] * ((new Vector(hsvColorCurrent)).Pow(new Vector(T[k])).Product());
                        propscur[j] = p;
                    }

                    // get into ranges [0,1]
                    propscur = Functions.ArraysManipulations.Saturation(propscur); // ##ok
                                                                                   // test inversion
                    List<double[]> propscurList = new List<double[]> { propscur };
                    hsvinv = Functions.Converters.Proportions2hsv(propscurList, mixTypes, Wcell, Ycell)[0];
                    // here a section about h_alt, remake
                    propscur = propscurList[0];
                    double[] hsvinv2 = (double[])hsvinv.Clone();
                    hsvinv2[0] = (Math.Abs(hsvinv[0] - hsvColorCurrent[0]) < Math.Abs(hsvinv[0] - 1 - hsvColorCurrent[0])) ? hsvinv[0] : (hsvinv[0] - 1);

                    double err = Math.Sqrt(Functions.ArraysManipulations.Distance(hsvColor[i], hsvinv2));

                    if (ctr < 2)
                    {
                        // if the values are calculated for the first time
                        if (err > tolerance)
                        {
                            ctr++;
                            mixTypes[i] = (ColorMixType)class2;
                            propscur.CopyTo(props0, 0);
                            err0 = err;
                        }
                        else
                            flag = false; // go out of the loop
                    }
                    else
                    {
                        // if the values are calculated for the second time
                        if (err > err0) // if Error is greater, return to first variant
                        {
                            props0.CopyTo(propscur, 0);
                            mixTypes[i] = (ColorMixType)clsvect;
                        }
                        flag = false; // anyway, go out of the loop
                    }
                }
                proportions[i] = propscur;
                hsvArray[i] = (double[])hsvNewColor[i].Clone();
            }
            hsvNewColor = hsvArray;
        }
    }
}
