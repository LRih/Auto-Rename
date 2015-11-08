using System;

namespace AutoRename
{
    public struct RegexPair
    {
        //===================================================================== VARIABLES
        public readonly string Name;
        public readonly string OldFormat;
        public readonly string NewFormat;

        //===================================================================== INITIALIZE
        public RegexPair(string name, string oldFormat, string newFormat)
        {
            Name = name;
            OldFormat = oldFormat;
            NewFormat = newFormat;
        }
        public RegexPair(string[] data)
        {
            Name = data[0];
            OldFormat = data[1];
            NewFormat = data[2];
        }
    }
}
