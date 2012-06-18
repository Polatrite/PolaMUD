using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    /// <summary>
    /// Singleton class. An enumeration of different types of parameters that would be used for 
    /// matching arguments for commands. 
    /// For example, a "get" command may check for Objects in Room. A "guild kick" command may 
    /// check for Players in Global.
    /// </summary>
    public static class SearchLocations
    {
        public const int Inventory = 1;
        public const int Equipment = 2;
        public const int Room = 10;
        public const int Area = 11;
        public const int Global = 12;
        public const int Party = 20;
        public const int Guild = 21;
        public const int Quest = 30;
    }
}
