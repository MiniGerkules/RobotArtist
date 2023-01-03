using GUI.PLT;
using GUI.Settings;
using System.Threading.Tasks;

namespace GUI {
    internal interface IImgFileContainer {
        bool Contains(string fileName);
        void Add(string fileName);
        void RemoveActive();

        PLTPicture? GetActive();
        string? GetPathToActive();

        Task ApplySettingsForActive(AlgorithmSettings newSettings);
    }
}
