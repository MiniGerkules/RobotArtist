using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GeneralComponents {
    public static class ListExtensions {
        public static ImmutableList<ImmutableList<double>> ToImmutable(this List<List<double>> list) {
            return list.Select(elem => elem.ToImmutableList()).ToImmutableList();
        }

        public static ImmutableList<ImmutableList<ImmutableList<double>>> ToImmutable(this List<List<List<double>>> list) {
            return list.Select(elem2d =>
                                    elem2d.Select(elem1d =>
                                                        elem1d.ToImmutableList()).
                                    ToImmutableList()).
                    ToImmutableList();
        }

        public static List<List<List<double>>> Copy(this ImmutableList<ImmutableList<ImmutableList<double>>> list) {
            return list.Select(elem2d => elem2d.Select(elem1d => elem1d.ToList()).ToList()).ToList();
        }

        public static ImmutableList<ImmutableList<double>> GetByIndexes(this ImmutableList<ImmutableList<double>> getFrom,
                                                      int[] indexes) {
            List<List<double>> list = new(indexes.Length);

            foreach (int index in indexes)
                list.Add(getFrom[index].ToList());

            return list.ToImmutable();
        }

        public static ImmutableList<double[]> GetByIndexes(this ImmutableList<double[]> getFrom,
                                                      int[] indexes)
        {
            List<double[]> list = new(indexes.Length);

            foreach (int index in indexes)
                list.Add(getFrom[index]);

            return list.ToImmutableList<double[]>();
        }
    }
}
