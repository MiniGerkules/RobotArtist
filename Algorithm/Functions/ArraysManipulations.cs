using System;
using System.Collections.Generic;

namespace Algorithm.Functions
{
    internal static class ArraysManipulations
    {
        /// <summary>
        /// The method returns a distance between two vectors (double[])
        /// distance is calculated as the sum of squares of the coordinate differences
        /// </summary>
        /// <param name="first"> First vector, double[] </param>
        /// <param name="second"> Second vector, double[] </param>
        /// <returns> One number evaluating the distance between two data vectors </returns>
        public static double Distance(double[] first, double[] second) 
        {
            double distance = 0;
            int length = first.Length;
            if (length != second.Length)
                throw new ArgumentException("dimensions of vectors should match!");
            for (int i = 0; i < length; i++)
                distance += (first[i] - second[i]) * (first[i] - second[i]);
            return distance;
        }

        /// <summary>
        /// The method returns a list of distances between center vector and a list of vectors
        /// </summary>
        /// <param name="center"> First vector, double[] </param>
        /// <param name="pixels"> Second vector, double[] </param>
        /// <returns> List of numbers = distances between center vector and 
        /// a list of vectors </returns>
        public static List<double> Distances(double[] center, List<double[]> pixels)
        {
            List<double> distances = new List<double>(pixels.Count);
            for (int i = 0; i < pixels.Count; i++)
                distances.Add(Distance(center, pixels[i]));
            return distances;
        }

        /// <summary>
        /// The method returns an array based on data array where element is 0 or 1 if it's 
        /// less than 0 or greater than 1 respectively
        /// </summary>
        /// <param name="proportions"> Data double[] array </param>
        /// <returns> An array based on data array where every element is in renge [0; 1] </returns>
        public static double[] Saturation(double[] proportions)
        {
            double[] val = new double[proportions.Length];
            for (int i = 0; i < proportions.Length; i++)
                val[i] = Math.Min(Math.Max(proportions[i], 0), 1);
            return val;
        }

        /// <summary>
        /// The method returns an array with elements = sum of elements of data arrays divided by 2
        /// </summary>
        /// <param name="arr1"> First array </param>
        /// <param name="arr2"> Second array </param>
        /// <returns> An array with elements = sum of elements of data arrays divided by 2 </returns>
        public static double[] MiddleArray(double[] arr1, double[] arr2)
        {
            if (arr1.Length != arr2.Length)
                throw new ArgumentException("dimensions should match!");
            double[] answer = new double[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
                answer[i] = (arr1[i] + arr2[i]) / 2d;
            return answer;
        }
    }
}
