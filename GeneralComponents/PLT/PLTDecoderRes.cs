using System.Collections.Generic;
using System.Collections.Immutable;

namespace GeneralComponents.PLT {
    public class PLTDecoderRes {
        public ImmutableList<Stroke> Strokes { get; private set; }
        public double Width { get; } = 0;
        public double Height { get; } = 0;

        public PLTDecoderRes(List<Stroke> strokes) {
            Strokes = strokes.ToImmutableList();

            foreach (var stroke in Strokes) {
                if (Width < stroke.End.X) Width = stroke.End.X;
                if (Height < stroke.End.Y) Height = stroke.End.Y;
            }
        }
    }
}
