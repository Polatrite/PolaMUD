using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public class MobReset : Reset
    {
        public MobReset(Room room, int indexNumber, int maximumRoom, int respawn)
        {
			Room = room;
            IndexNumber = indexNumber;
            MaximumRoom = maximumRoom;
            Respawn = respawn;
            Name = "MobReset of " + indexNumber + " in " + room.ToString() + " (limit " + maximumRoom + ")";

        }

		public override void Spawn()
		{
            Mob mob = new Mob(IndexNumber);
            mob.Move(Room);
		}
    }
}
