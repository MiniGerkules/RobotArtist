using System;

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

        public FileName(FileName other) : this(other.FullName, other.ShortName)
        {
        }

        public static bool operator ==(FileName a, FileName b)
        {
            if (ReferenceEquals(a, b))
                return true;
            else if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return false;
            else
                return a.FullName == b.FullName && a.ShortName == b.ShortName;
        }

        public static bool operator !=(FileName a, FileName b)
        {
            if (ReferenceEquals(a, b))
                return false;
            else if (ReferenceEquals(a, null) || ReferenceEquals(b, null))
                return true;
            else
                return a.FullName != b.FullName || a.ShortName != b.ShortName;                
        }

        public override int GetHashCode()
        {
            return Tuple.Create(FullName, ShortName).GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj is FileName fileName)
                return this == fileName;
            else
                return false;
        }
    }
}
