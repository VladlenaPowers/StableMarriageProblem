﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PopularMatching
{
    struct BeforeAfter
    {
        public IEnumerable<int> before;
        public IEnumerable<int> after;
    }

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

            int[][] men = new int[5][]
             {  new int[2] { 0,3 },
                new int[2] { 0,1 },
                new int[3] { 1,0,2 },
                new int[3] { 0,3,4 },
                new int[2] { 4,3 }
             };
            int[][] women = new int[5][]
            {   new int[4] { 2,3,0,1 },
                new int[2] { 1,2 },
                new int[1] { 2 },
                new int[3] { 4,0,3 },
                new int[2] { 3,4 }
            };

            const bool USE_DISCRETE = true;
            
            var kavithaOutputs = new List<int[]>();
            var gKavithaOutputs = new List<int[]>();

            //Run Continuous Kavitha's Algorithm
            var cResults = new Dictionary<int[], List<BeforeAfter>>(MatchingEqualityComparer.INSTANCE);
            foreach (var prioritizedMen in Enumerable.Range(0, men.Length).Subset())
            {
                ContinuousKavitha.Output o = ContinuousKavitha.Run(men, women, prioritizedMen);

                var result = new BeforeAfter()
                {
                    before = prioritizedMen,
                    after = o.men1
                };

                if (cResults.ContainsKey(o.matching))
                {
                    cResults[o.matching].Add(result);
                }
                else
                {
                    List<BeforeAfter> temp = new List<BeforeAfter>();
                    temp.Add(result);
                    cResults.Add(o.matching, temp);
                }
            }

            //Run Discrete Kavitha's Algorithm
            var dResults = new Dictionary<int[], List<BeforeAfter>>(MatchingEqualityComparer.INSTANCE);
            foreach (var prioritizedMen in Enumerable.Range(0, men.Length).Subset())
            {
                DiscreteKavitha.Output o = DiscreteKavitha.Run(men, women, prioritizedMen);

                var result = new BeforeAfter() {
                    before = prioritizedMen,
                    after = o.men1
                };

                if (dResults.ContainsKey(o.matching))
                {
                    dResults[o.matching].Add(result);
                }
                else
                {
                    List<BeforeAfter> temp = new List<BeforeAfter>();
                    temp.Add(result);
                    dResults.Add(o.matching, temp);
                }
            }


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Continuous Kavitha Algorithm:");

            const string format = "{0,-32} :{1}";
            foreach (var kvPair in cResults)
            {
                Console.WriteLine("----------------------------- " + kvPair.Key.DefaultString() + " -------------------------------");
                Console.WriteLine(Utility.CollectionToString(kvPair.Value.Select((result) =>
                {
                    return string.Format(format, result.before.DefaultString(), result.after.DefaultString());
                }), "", "\n", ""));
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("Discrete Kavitha Algorithm:");
            foreach (var kvPair in dResults)
            {
                Console.WriteLine("----------------------------- " + kvPair.Key.DefaultString() + " -------------------------------");
                Console.WriteLine(Utility.CollectionToString(kvPair.Value.Select((result) =>
                {
                    return string.Format(format, result.before.DefaultString(), result.after.DefaultString());
                }), "", "\n", ""));
            }
            
            Console.WriteLine();
            Console.WriteLine("Unique outputs:");
            Console.WriteLine();
            Console.WriteLine("Continuous Kavitha Algorithm:");
            Console.WriteLine(Utility.CollectionToString(cResults.Keys.Select(key => key.DefaultString()), "", "\n", ""));
            Console.WriteLine();
            Console.WriteLine("Discrete Kavitha Algorithm:");
            Console.WriteLine(Utility.CollectionToString(dResults.Keys.Select(key => key.DefaultString()), "", "\n", ""));
            
            Console.WriteLine();
            Console.WriteLine("Brute force popular matchings:");
            Console.WriteLine(Utility.CollectionToString(ValidMatchings(men, women).PopularMatchings(men, women).Select(m => m.DefaultString()), "", "\n", ""));


            Console.WriteLine();
            Console.WriteLine("men:");
            Console.WriteLine(Utility.CollectionToString(men.Select((pl, i) => i + ": " + pl.DefaultString()), "", "\n", ""));

            Console.WriteLine();
            Console.WriteLine("women:");
            Console.WriteLine(Utility.CollectionToString(men.Select((pl, i) => i + ": " + pl.DefaultString()), "", "\n", ""));

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

            Console.Read();
        }
    }
}
