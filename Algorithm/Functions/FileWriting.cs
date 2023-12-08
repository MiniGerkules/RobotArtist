using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Algorithm.Functions
{
    internal class FileWriting
    {
        /// <summary>
        /// The method converts a list of strokes to the plt-file 
        /// with commands for the robot using data parameters
        /// </summary>
        /// <param name="strokes"> List of strokes </param>
        /// <param name="m"> Height of the image </param>
        /// <param name="n"> Width of the image </param>
        /// <param name="canvasW_mm"> Canvas width in the millimeters </param>
        /// <param name="canvasH_mm"> Canvas height in the millimeters </param>
        /// <param name="brushSize_mm"> Brush size in the millimeters </param>
        /// <param name="filePath"> The path to the plt-file </param>
        /// <returns> Writes commands for the robot to the plt-file </returns>
        public static void SavePLT_8paints(List<Stroke> strokes, double m, double n, double canvasW_mm, double canvasH_mm, double brushSize_mm, string filePath)
        {
            if (filePath == "")
                return;
            try
            {
                StreamWriter sw = new StreamWriter(filePath);
                int scl = 40;
                sw.Write("IN;");
                double W = canvasW_mm * scl;
                double H = canvasH_mm * scl;
                double sfX = Math.Min(W / n, H / m); // #sfX = #sfY

                int nStrokes = strokes.Count;
                double[] col8paints = strokes[0].Col8paints; // Color in 8 paints, in motor ticks

                int commandCounter = 2; // #cmdctr
                sw.Write("PP");
                for (int k = 0; k < col8paints.Length - 1; k++)
                {
                    sw.Write(col8paints[k]);
                    sw.Write(",");
                }
                sw.Write(col8paints[col8paints.Length - 1]);
                sw.Write(";");
                double bs = (brushSize_mm * scl); // new brush size, scaled
                for (int i = 0; i < nStrokes; i++)
                {
                    double xo = (strokes[i].Points[0].Y + 1) * sfX;
                    double yo = (m - strokes[i].Points[0].X - 1) * sfX;
                    double[] col2 = strokes[i].Col8paints;
                    if (!Enumerable.SequenceEqual(col2, col8paints))
                    {
                        for (int k = 0; k < col8paints.Length; k++)
                            col8paints[k] = Math.Round(strokes[i].Col8paints[k]);
                        commandCounter++;
                        sw.Write("PP");
                        for (int k = 0; k < col8paints.Length - 1; k++)
                        {
                            sw.Write(col8paints[k]);
                            sw.Write(",");
                        }
                        sw.Write(col8paints[col8paints.Length - 1]);
                        sw.Write(";");
                        commandCounter++;
                        sw.Write("PU");
                        sw.Write(Math.Round(xo));
                        sw.Write(",");
                        sw.Write(Math.Round(yo));
                        sw.Write(";");
                    }
                    else
                    {
                        if (i > 0)
                        {
                            var yprev = (m - strokes[i - 1].Points.Last().X - 1) * sfX;
                            var xprev = (strokes[i - 1].Points.Last().Y + 1) * sfX;

                            if (Functions.ArraysManipulations.Distance(new double[] { xo, yo }, new double[] { xprev, yprev }) > bs * bs)
                            {
                                commandCounter++;
                                sw.Write("PU");
                                sw.Write(Math.Round(xo));
                                sw.Write(",");
                                sw.Write(Math.Round(yo));
                                sw.Write(";");
                                // PU only if strokes are merged in one...
                            }
                        }
                        else
                        {
                            commandCounter++;
                            sw.Write("PU");
                            sw.Write(Math.Round(xo));
                            sw.Write(",");
                            sw.Write(Math.Round(yo));
                            sw.Write(";");
                        }
                    }

                    for (int j = 0; j < strokes[i].Points.Count; j++)
                    {
                        xo = (strokes[i].Points[j].Y + 1) * sfX;
                        yo = (m - strokes[i].Points[j].X - 1) * sfX;
                        commandCounter++;
                        sw.Write("PD");
                        sw.Write(Math.Round(xo));
                        sw.Write(",");
                        sw.Write(Math.Round(yo));
                        sw.Write(";"); // not entirely correct, we may use only first PD before next commands, but this is simpler
                    }
                }
                sw.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }
    }
}
