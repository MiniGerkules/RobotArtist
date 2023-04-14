using System.Windows.Media;

namespace GeneralComponents.Colors {
    public interface IColor {
        Color GetRealColor();
        Color GetArtificialColor();
    }
}
