using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public class MovableThing : Thing
    {
        /*public delegate void PreMove(Thing targetLocation);
        public event PreMove PreMove;
        public delegate void OnMove(Thing targetLocation);
        public event OnMove OnMove;*/

        public bool CanMove(Thing targetThing)
        {
            return true;
        }

        public bool Move(Thing targetThing)
        {
            var currentLocation = Location;

            if (!CanMove(targetThing))
            {
                return false;
            }

            if (Location != null)
            {
                Location.Contents.Remove(this);
                Location = null;
            }

            targetThing.Contents.Add(this);
            Location = targetThing;

            return true;
        }

        public bool Move(Thing targetThing, string message)
        {
            var ret = Move(targetThing);
            return ret;
        }
    }
}
