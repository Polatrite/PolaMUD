using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
	public class Exit : Thing
	{
		public int FromRoom;
		public int ToRoom;

		public bool Visible = true;

		public bool Door = false;
		public bool Closed = false;
		public bool Locked = false;
		public int LockDifficulty = 0;
		public int Key;

		public Exit()
		{
		}

		public Exit(int toRoom)
		{
			ToRoom = toRoom;
            Name = "Exit to " + toRoom;
		}

		public Exit(int fromRoom, int toRoom)
		{
			FromRoom = fromRoom;
			ToRoom = toRoom;
            Name = "Exit from " + fromRoom + " to " + toRoom;
        }
	}
}
