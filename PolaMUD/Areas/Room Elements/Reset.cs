using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
	public abstract class Reset : Thing
	{
		public Room Room;
		public int Respawn = 5;
		public int RespawnTrack = 0;
		public int MaximumRoom = 0;

		public virtual void Spawn()
		{
		}
	}
}
