using System.Windows.Media;

namespace GUI.Colors {
    public interface IColor {
        Color GetRealColor();
        Color GetArtificialColor();
    }
}
