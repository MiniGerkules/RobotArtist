namespace GUI
{
    internal class FileName
    {
        public string ShortName { get; private set; }
        public string FullName { get; private set; }

        public FileName(string fullName) : this(fullName, Helpers.GetFileName(fullName))
        {
        }

        public FileName(string fullName, string shortName)
        {
            FullName = fullName;
            ShortName = shortName;
        }
    }
}
