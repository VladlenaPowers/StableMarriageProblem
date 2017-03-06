﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PopularMatching
{

    public static class Program
    {

        //this algorithm generates all possible matchings
        private static IEnumerable<int[]> ValidMatchings(int[][] men, int[][] women)
        {
            var menSequence = Enumerable.Range(0, men.Length);

            //represents a three-dimensional jagged array
            //i0 = the number of unmatched men
            //i1 = possible set of unmatched men
            //i2 = unmatched man
            var unmatchedMen = Enumerable.Range(0, men.Length + 1).Select(unmatchedMenCount => {
                return Enumerable.Range(0, men.Length).Subset().Where(i => i.Count() == unmatchedMenCount).Select(i => i.ToArray()).ToArray();
            }).ToArray();
            
            foreach (var matchedWomen in Enumerable.Range(0, women.Length).OrderedSubset().Where(m => m.Count() <= men.Length))
            {
                int[] cpy = matchedWomen.ToArray();
                
                int unmatchedMenCount = men.Length - cpy.Length;
                int[][] possibleUnmatchedMen = unmatchedMen[unmatchedMenCount];
                for (int i = 0; i < possibleUnmatchedMen.Length; i++)
                {
                    int[] unmatchedMen2 = possibleUnmatchedMen[i];

                    int[] output = new int[men.Length];

                    int j = 0;
                    int k = 0;
                    for (int l = 0; l < output.Length; l++)
                    {
                        if (j < unmatchedMenCount && l == unmatchedMen2[j])
                        {
                            output[l] = -1;
                            j++;
                        }
                        else if(k < cpy.Length)
                        {
                            output[l] = cpy[k++];
                        }
                        else
                        {
                            throw new Exception("Not enough unmatched and matched men");
                        }
                    }

                    bool passes = true;
                    for (int l = 0; l < output.Length; l++)
                    {
                        int woman = output[l];
                        if (woman >= 0)
                        {
                            if (!(women[woman].Contains(l) && men[l].Contains(woman)))
                            {
                                passes = false;
                            }
                        }
                    }

                    if (passes)
                    {
                        yield return output;
                    }
                }
            }
        }

        //this algorithm generates a collection of matchings
        private static IEnumerable<int[]> BruteForceAlgorithm(int[][] men, int[][] women)
        {
            foreach (var matching in Utility.Permutation(Enumerable.Range(0, men.Length)))
            {
                int[] cpy = matching.ToArray();
                for (int i = 0; i < men.Length; i++)
                {
                    int woman = cpy[i];
                    if (woman >= 0 && (!women[woman].Contains(i) || !men[i].Contains(woman)))
                    {
                        cpy[i] = -1;
                    }
                }
                yield return cpy;
            }
        }

        //returns all of the popular matchings for a given set of matchings
        private static IEnumerable<int[]> PopularMatchings(this IEnumerable<int[]> matchings, int[][] men, int[][] women)
        {
            MatchingPopularityComparer comparer = new MatchingPopularityComparer(men, women);

            int[][] matchingsArray = matchings.ToArray();

            Console.WriteLine("Finding Popular matchings...");

            List<int[]> output = new List<int[]>();
            int[][] matchingsArr = matchings.ToArray();
            for (int i = 0; i < matchingsArr.Length; i++)
            {
                bool popular = true;
                for (int j = 0; j < matchingsArr.Length; j++)
                {
                    if (i != j)
                    {
                        if (comparer.Compare(matchingsArr[i], matchingsArr[j]) < 0)
                        {
                            popular = false;
                            break;
                        }
                    }
                }
                if (popular)
                {
                    output.Add(matchingsArr[i]);
                }
            }
            return output;

            //return matchingsArray.Where(matching => {
            //    return matchingsArray.All(curr => comparer.Compare(matching, curr) >= 0);
            //});
        }

        //private static IEnumerable<int[]> StableMatchings(this IEnumerable<int[]> popularMatchings)
        //{
        //    int[] min = popularMatchings.Aggregate((a, b) => (Matching.stableComparer.Compare(a, b) > 0) ? a : b);
        //    return popularMatchings.Where(popularMatching =>
        //    {
        //        return Matching.stableComparer.Compare(min, popularMatching) == 0;
        //    });
        //}

        static void Main(string[] args)
        {
            Console.SetWindowPosition(0, 0);
            Console.SetWindowSize(200, 64);

            int[][] men = new int[8][]
            {   new int[8] { 4,6,0,1,5,7,3,2 },
                new int[8] { 1,2,6,4,3,0,7,5 },
                new int[8] { 7,4,0,3,5,1,2,6 },
                new int[8] { 2,1,6,3,0,5,7,4 },
                new int[8] { 6,1,4,0,2,5,7,3 },
                new int[8] { 0,5,6,4,7,3,1,2 },
                new int[8] { 1,4,6,5,2,3,7,0 },
                new int[8] { 2,7,3,4,6,1,5,0 }
            };

            int[][] women = new int[8][]
                        {   new int[8] { 4,2,6,5,0,1,7,3 },
                new int[8] { 7,5,2,4,6,1,0,3 },
                new int[8] { 0,4,5,1,3,7,6,2 },
                new int[8] { 7,6,2,1,3,0,4,5 },
                new int[8] { 5,3,6,2,7,0,1,4 },
                new int[8] { 1,7,4,2,3,5,6,0 },
                new int[8] { 6,4,1,0,7,5,3,2 },
                new int[8] { 6,3,0,4,1,2,5,7 }
                        };

            const bool USE_DISCRETE = true;

            if (true)
            {
                var kavithaOutputs = new List<int[]>();
                var gKavithaOutputs = new List<int[]>();

                //Run Kavitha's Algorithm
                var menBefore = new Dictionary<int[], List<IEnumerable<int>>>(MatchingEqualityComparer.INSTANCE);
                var menAfter = new Dictionary<int[], List<IEnumerable<int>>>(MatchingEqualityComparer.INSTANCE);
                foreach (var prioritizedMen in Enumerable.Range(0, men.Length).Subset())
                {
                    int[] matching;
                    int[] prioritizedMenAfter;
                    if (USE_DISCRETE)
                    {
                        DiscreteKavitha.Output o = DiscreteKavitha.Run(men, women, prioritizedMen);
                        matching = o.matching;
                        prioritizedMenAfter = o.men0;
                    }
                    else
                    {
                        ContinuousKavitha.Output o = ContinuousKavitha.Run(men, women, prioritizedMen);
                        matching = o.matching;
                        prioritizedMenAfter = o.men0;
                    }
                    
                    if (menAfter.ContainsKey(matching))
                    {
                        menBefore[matching].Add(prioritizedMen);
                        menAfter[matching].Add(prioritizedMenAfter);
                    }
                    else
                    {
                        var temp = new List<IEnumerable<int>>();
                        var temp2 = new List<IEnumerable<int>>();
                        temp.Add(prioritizedMenAfter);
                        temp2.Add(prioritizedMen);
                        menAfter[matching] = temp;
                        menBefore[matching] = temp2;
                    }
                }



                Console.WriteLine();

                const string format = "{0,-32} :{1}";

                foreach (var kvPair in menAfter)
                {
                    Console.WriteLine();
                    Console.WriteLine("----------------------------- " + kvPair.Key.DefaultString() + " -------------------------------");
                    Console.WriteLine(Utility.CollectionToString(kvPair.Value.Zip(menBefore[kvPair.Key], (after, before) =>
                    {
                        return string.Format(format, before.DefaultString(), after.DefaultString());
                    }), "", "\n", ""));
                }

                Console.WriteLine();

                Console.WriteLine("Men:");
                Console.WriteLine(Utility.CollectionToString(men.Select((list, i) => i + ": " + list.DefaultString()), "", "\n", ""));
                Console.WriteLine("Women:");
                Console.WriteLine(Utility.CollectionToString(women.Select((list, i) => i + ": " + list.DefaultString()), "", "\n", ""));

                //Console.WriteLine();
                //Console.WriteLine("popularMatchings:");
                //var popularMatchings = ValidMatchings(men, women).Distinct(MatchingEqualityComparer.INSTANCE).PopularMatchings(men, women);
                //foreach (var popularMatching in popularMatchings)
                //{
                //    Console.WriteLine(popularMatching.DefaultString());
                //}

                //Console.WriteLine();
                //Console.Write("Filtering duplicate outputs ");
                //kavithaOutputs = kavithaOutputs.Distinct().ToList();
                //gKavithaOutputs = gKavithaOutputs.Distinct().ToList();
                //Console.WriteLine("done");

                //Console.WriteLine();
                //Console.WriteLine("original:");
                //Console.WriteLine(Utility.CollectionToString(kavithaOutputs.Select(list => list.DefaultString()), "", "\n", ""));
                //Console.WriteLine();
                //Console.WriteLine("general:");
                //Console.WriteLine(Utility.CollectionToString(gKavithaOutputs.Select(list => list.DefaultString()), "", "\n", ""));

                //Console.WriteLine();
                //bool popSubsetOfOriginal = popularMatchings.All(a => kavithaOutputs.Contains(a, MatchingEqualityComparer.INSTANCE));
                //Console.WriteLine("(popular matchings) ⊆(original Kavitha algorithm outputs)? " + ((popSubsetOfOriginal) ? "YES" : "NO"));
                //Console.WriteLine();

                //bool popSubsetOfGenKavitha = popularMatchings.All(a => gKavithaOutputs.Contains(a, MatchingEqualityComparer.INSTANCE));
                //Console.WriteLine("(popular matchings) ⊆(general Kavitha algorithm outputs)? " + ((popSubsetOfGenKavitha) ? "YES" : "NO"));
                //Console.WriteLine();
            }

            Console.Read();
        }
    }
}
