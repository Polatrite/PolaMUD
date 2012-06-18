using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    /// <summary>
    /// Singleton class. Contains the ANSI color codes for all supported colors. 
    /// Note to MUD administrators: only use blink when you're PUNISHING players, otherwise 
    /// NEVER USE BLINK. It's damn annoying. (Which is why our jail is a blinking deathtrap 
    /// you have to run frantically around until your sentence is up. >=)
    /// </summary>
	public static class Colors
	{
		public static string CLEAR = "[0m";
		public static string RED = "[0;31m";
		public static string GREEN = "[0;32m";
		public static string YELLOW = "[0;33m";
		public static string BLUE = "[0;34m";
		public static string MAGENTA = "[0;35m";
		public static string CYAN = "[0;36m";
		public static string WHITE = "[0;37m";
		public static string GRAY = "[1;30m";
		public static string BRIGHT_RED = "[1;31m";
		public static string BRIGHT_GREEN = "[1;32m";
		public static string BRIGHT_YELLOW = "[1;33m";
		public static string BRIGHT_BLUE = "[1;34m";
		public static string BRIGHT_MAGENTA = "[1;35m";
		public static string BRIGHT_CYAN = "[1;36m";
		public static string BRIGHT_WHITE = "[1;37m";

	}
}
