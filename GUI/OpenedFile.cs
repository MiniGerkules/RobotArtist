using System.Windows;
using System.Windows.Controls;

namespace GUI
{
    internal class OpenedFile : Button
    {
        private string fullName;
        public string FullName
        {
            get => fullName;
            set
            {
                fullName = value;
                ShortName = Helpers.GetFileName(value);
                Content = ShortName;
            }
        }
        public string ShortName { get; private set; }

        public OpenedFile(string fullName) : this(fullName, Helpers.GetFileName(fullName))
        {
        }

        public OpenedFile(string fullName, string shortName)
        {
            FullName = fullName;
            Content = shortName;

            Margin = new Thickness(5, 5, 5, 5);
            FontSize = 16;
        }
    }
}
