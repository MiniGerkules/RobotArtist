using System.Collections.Generic;
using System.Collections.Immutable;

namespace GUI.PLT {
    public class PLTDecoderRes {
        public ImmutableList<Stroke> Strokes { get; private set; }
        public double Width { get; }
        public double Height { get; }

        public PLTDecoderRes(List<Stroke> strokes) {
            Strokes = strokes.ToImmutableList();

            Width = 0; Height = 0;
            foreach (var stroke in Strokes) {
                if (Width < stroke.End.X) Width = stroke.End.X;
                if (Height < stroke.End.Y) Height = stroke.End.Y;
            }
        }
    }
}
