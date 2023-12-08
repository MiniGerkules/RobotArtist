using GeneralComponents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Functions
{
    internal static class Clustering
    {
        /// <summary>
        /// The method returns a vector of indexes of the size = colArray.Count
        /// every double[] in list colArray is an rgb-pixel - Color dot 
        /// every Color dot goes to some of clustersAmount clusters
        /// result vector's i-coordinate means that i Color dot goes to vector[i] cluster (cluster number i)
        /// </summary>
        /// <param name="colArray"> List of rgb-pixels </param>
        /// <param name="clustersAmount"> Amount of clusters to split rgb-pixels into </param>
        /// <returns> Vector of integer numbers from 0 to (clustersAmount - 1) </returns>
        internal static List<int> KMeans(List<double[]> colArray, uint clustersAmount)
        {
            List<int> result = new List<int>(colArray.Count);

            // initial random choice of cluster's centroids by KMeans++ algorithm
            List<double[]> centers = Functions.RandomBased.KmeansPlus(colArray, clustersAmount);

            int iterationsLimit = 100;

            int currentIteration = 0;
            while (currentIteration < iterationsLimit)
            {
                currentIteration++;
                // now create a matrix where i row is about pixel, j column is about cluster
                // (cluster centroid). On i,j place will be the distance from pixel
                // to cluster's centroid
                Matrix2D distances = new Matrix2D(colArray.Count, (int)clustersAmount);

                for (int j = 0; j < clustersAmount; j++)
                {
                    double minDistance = 0;
                    double[] minPixel = centers[j]; // is a pixel nearest to it's cluster centroid
                    for (int i = 0; i < colArray.Count; i++)
                    {
                        distances[i, j] = ArraysManipulations.Distance(colArray[i], centers[j]);
                        if ((minDistance == 0) || (minDistance > distances[i, j]))
                        {
                            minDistance = distances[i, j];
                            minPixel = colArray[i];
                        }
                    }
                    centers[j] = ArraysManipulations.MiddleArray(centers[j], minPixel); // renew the center
                }
                List<int> currentResult = distances.GetIndexesOfMinElementsInRows().ToList();
                if (result.Count == 0)
                    result.AddRange(currentResult);
                else if (result.SequenceEqual(currentResult))
                    break;
            }
            return result;
        }
    }
}
