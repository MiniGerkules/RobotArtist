using System.Linq;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace GeneralComponents {
    public class Database {
        private readonly List<List<List<double>>> data;

        public Database(List<List<List<double>>> data) {
            this.data = data;
        }

        public ImmutableList<ImmutableList<ImmutableList<double>>> GetHSV() {
            List<List<List<double>>> hsv = Enumerable.Repeat(new List<List<double>>(), data.Count).ToList();

            for (int i = 0; i < data.Count; ++i)
                hsv[i].AddRange(data[i].Select(elem => elem.GetRange(0, 3)));

            return hsv.ToImmutable();
        }

        public ImmutableList<ImmutableList<ImmutableList<double>>> GetProportions() {
            List<List<List<double>>> props = Enumerable.Repeat(new List<List<double>>(), data.Count).ToList();

            for (int i = 0; i < data.Count; ++i)
                props[i].AddRange(data[i].Select(elem => elem.GetRange(3, 3)));

            return props.ToImmutable();
        }

        public ImmutableList<ImmutableList<double>> GetHSV(ColorMixType mixType) {
            List<List<double>> hsv = new();

            if (mixType == ColorMixType.MagentaYellow1 || mixType == ColorMixType.MagentaYellow2) {
                hsv = new(data[0].Count + data[1].Count);
                hsv.AddRange(data[0].Select(elem => elem.GetRange(0, 3)));
                hsv.ForEach(elem => elem[0] -= 1);
                hsv.AddRange(data[1].Select(elem => elem.GetRange(0, 3)));
            } else {
                hsv = new(data[(int)mixType].Count);
                hsv.AddRange(data[(int)mixType].Select(elem => elem.GetRange(0, 3)));
            }

            return hsv.ToImmutable();
        }

        public ImmutableList<ImmutableList<double>> GetProportions(ColorMixType mixType) {
            List<List<double>> proportions = new();

            if (mixType == ColorMixType.MagentaYellow1 || mixType == ColorMixType.MagentaYellow2) {
                proportions = new(data[0].Count + data[1].Count);
                proportions.AddRange(data[0].Select(elem => elem.GetRange(3, 3)));
                proportions.AddRange(data[1].Select(elem => elem.GetRange(3, 3)));
            } else {
                proportions = new(data[(int)mixType].Count);
                proportions.AddRange(data[(int)mixType].Select(elem => elem.GetRange(3, 3)));
            }

            return proportions.ToImmutable();
        }
    }
}
