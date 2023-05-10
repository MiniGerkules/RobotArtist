using System;
using System.Collections.Generic;
using System.Linq;

namespace Algorithm.Functions
{
    internal static class RandomBased
    {
        /// <summary>
        /// The method returns a vector (int[]) of vectorSize of random integer numbers in range 1:limit
        /// </summary>
        /// <param name="limit"> Upper bound of the range </param>
        /// <param name="vectorSize"> The size of the generted vector </param>
        /// <returns> Vector of integer numbers </returns>
        public static int[] Randperm(int limit, int vectorSize)
        {
            if (vectorSize > limit)
                throw new ArgumentException("vectorSize must be <= limit");

            int[] answer = new int[vectorSize];
            var rand = new Random();
            for (int i = 0; i < vectorSize; i++)
                answer[i] = (i + 1 <= limit) ? (i + 1) : rand.Next(limit - 1) + 1;
            Shuffle<int>(rand, answer);
            return answer;
        }

        /// <summary>
        /// The method returns an shuffled array
        /// </summary>
        /// <param name="rng"> Random object to generate random integer numbers </param>
        /// <param name="array"> Array to shuffle elements in </param>
        /// <returns> Shuffled array </returns>
        public static void Shuffle<T>(this Random rng, T[] array)
        {
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }

        /// <summary>
        /// The method defines KMeans-plus algorithm
        /// </summary>
        /// <param name="colArray"> A list of arrays (every array represents a Color) </param>
        /// <param name="clustersAmount"> A number of clusters </param>
        /// <returns> List consists of clustersAmount clusters filled by data colors </returns>
        public static List<double[]> KmeansPlus(List<double[]> colArray, uint clustersAmount)
        {
            List<double[]> centers = new List<double[]>((int)clustersAmount);
            List<double[]> pixels = colArray.GetRange(0, colArray.Count);

            var rnd = new Random();
            int currentCenterIndex = rnd.Next(pixels.Count);

            centers.Add(pixels[currentCenterIndex]);
            pixels.RemoveAt(currentCenterIndex);

            // pixels, probabilities and distances match on index
            while (centers.Count < clustersAmount)
            {
                List<double> distances = Functions.ArraysManipulations.Distances(centers[0], pixels);

                List<double> probabilities = new List<double>(distances.Count);

                double distancesSum = distances.Sum();

                double currentSum = 0;

                for (int i = 0; i < distances.Count; i++)
                {
                    currentSum += distances[i];
                    probabilities.Add(currentSum / distancesSum);
                }

                double p = rnd.NextDouble();
                for (int i = 0; i < probabilities.Count; i++)
                {
                    if (p < probabilities[i])
                    {
                        centers.Add(pixels[i]);
                        pixels.RemoveAt(i);
                        break;
                    }
                }
            }
            return centers;
        }
    }
}
