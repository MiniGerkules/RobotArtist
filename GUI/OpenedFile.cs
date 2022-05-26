using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    internal class OpenedFile : Button
    {
        public FileName FileName { get; private set; }

        public OpenedFile(string fullName) : this(fullName, Helpers.GetFileName(fullName))
        {
        }

        public OpenedFile(string fullName, string shortName)
        {
            FileName = new(fullName, shortName);

            Content = shortName;
            Margin = new Thickness(5, 5, 5, 5);
            FontSize = 16;
        }

        public OpenedFile(FileName fileName) : this(fileName.FullName, fileName.ShortName)
        {
        }
    }
}
