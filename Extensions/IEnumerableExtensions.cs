using System.Xml;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CloudNFT.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };
            return sequences.Aggregate(
                emptyProduct,
                (accumulator, sequence) =>
                    from accseq in accumulator
                    from item in sequence
                    select accseq.Concat(new[] { item })
                );
        }

         public static IEnumerable<T> Randomize<T>(this IEnumerable<T> source)
        {
            var range= new Random();
            var output = source.Randomize(range);            
            return output;
        }

        private static IEnumerable<T> Randomize<T>(this IEnumerable<T> source, Random range)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (range== null) throw new ArgumentNullException("range");

            return source.RandomizeAlgorithm(range);
        }

        private static IEnumerable<T> RandomizeAlgorithm<T>(this IEnumerable<T> source, Random range)
        {
            var temp = source.ToList();

            for (int i = 0; i < temp.Count; i++)
            {
                int j = range.Next(i, temp.Count);
                yield return temp[j];

                temp[j] = temp[i];
            }
        }
    }
}