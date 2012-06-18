using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace PolaMUD
{
	public class Room : Thing
	{
		public List<object> Contents = new List<object>();
		public Area Area;
		public string Description;
		public Terrain Terrain;
        public List<MobReset> MobResets = new List<MobReset>();

		public Dictionary<Directions, Exit> Exits = new Dictionary<Directions, Exit>();

        /// <summary>
        /// Creates a new room with the specified IndexNumber and adds it to Global.RoomTable.
        /// </summary>
        /// <param name="number"></param>
		public Room(int number)
		{
			IndexNumber = number;
			try
			{
				Global.RoomTable.Add(IndexNumber, this);
			}
			catch
			{
				Global.Log("ERROR: Room #" + IndexNumber + " already exists.\n");
			}
		}

        /// <summary>
        /// Displays the short descriptive string of the room.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name + " #" + IndexNumber + " (" + (Area != null ? Area.Name : "null") + ")";
        }

        /// <summary>
        /// Loads a new room from the provided XmlNode.
        /// </summary>
        /// <param name="room"></param>
		public void Load(XmlNode room)
		{
			Name = room.Attributes["name"].Value;
			Description = room["desc"].InnerText;

			foreach (XmlNode exit in room["exits"].ChildNodes)
			{
				Directions dir = Directions.North;
				switch (exit.Attributes["name"].Value)
				{
					case "north":
						dir = Directions.North;
						break;
					case "south":
						dir = Directions.South;
						break;
					case "west":
						dir = Directions.West;
						break;
					case "east":
						dir = Directions.East;
						break;
					case "up":
						dir = Directions.Up;
						break;
					case "down":
						dir = Directions.Down;
						break;
				}
				Exits[dir] = new Exit(IndexNumber, Convert.ToInt32(exit.Attributes["index"].Value));
			}

			if (room["mobresets"] != null)
			{
				foreach (XmlNode mob in room["mobresets"].ChildNodes)
				{
					MobReset mobreset = new MobReset(this, Convert.ToInt32(mob.Attributes["index"].Value), Convert.ToInt32(mob.Attributes["maxroom"].Value), Convert.ToInt32(mob.Attributes["respawn"].Value));
					MobResets.Add(mobreset);
					Global.Log("    Added ID #" + mobreset.IndexNumber);
				}
			}

			Global.Log("    Room: " + Name + " with ID #" + IndexNumber + "\n");
		}

        /// <summary>
        /// Loads a new room from the provided XmlNode while automatically setting the area that the room belongs to.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="area"></param>
        public void Load(XmlNode room, Area area)
        {
            Area = area;
            Load(room);
        }

        /// <summary>
        /// Add a Thing to the Room, sending no messages.
        /// </summary>
        /// <param name="thing">Thing to be added</param>
        /// <returns></returns>
        public Room Add(Thing thing)
        {
            return Add(thing, "");
        }

        /// <summary>
        /// Add a Thing to the Room, sending the given entrance message.
        /// </summary>
        /// <param name="thing">Thing to be added</param>
        /// <param name="message">Message to the Room</param>
        /// <returns></returns>
		public Room Add(Thing thing, string message)
		{
			if(message != null)
			{
                SendMessage(message);
            }

			if (thing is Player)
				Display((Player)thing);

			Contents.Add(thing);
			return this;
		}

        /// <summary>
        /// Remove a Thing from the Room, sending no messages.
        /// </summary>
        /// <param name="thing">Thing to be removed</param>
        /// <returns></returns>
        public Room Remove(Thing thing)
        {
            return Remove(thing, "");
        }

        /// <summary>
        /// Remove a Thing from the Room, sending the given exit message.
        /// </summary>
        /// <param name="thing">Thing to be removed</param>
        /// <param name="message">Message to the room</param>
        /// <returns></returns>
        public Room Remove(Thing thing, string message)
		{
			if (message != null)
			{
                SendMessage(message);
			}

			Contents.Remove(thing);
			return this;
		}

        /// <summary>
        /// Get a Thing from the room with the provided Type (Mob, Player, etc.) that matches Name.
        /// </summary>
        /// <param name="reference">The Type to search for (Mob, Player, etc.)</param>
        /// <param name="name">Name to match</param>
        /// <returns></returns>
        public Thing GetThing(Type reference, string name)
        {
            foreach (Thing thing in Contents)
            {
                if (thing.GetType() == reference || thing.GetType().BaseType == reference)
                {
                    //If name is empty string, it should be Not Found.
                    if (name.Length > 0 && thing.HandlingName.IndexOf(name) >= 0)
                    {
                        return thing;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Show the Room to Player, including all contents.
        /// </summary>
        /// <param name="player"></param>
		public void Display(Player player)
		{
			player.SendMessage(Colors.BRIGHT_WHITE + Name + " [Room " + IndexNumber + "]\n\r");
			player.SendMessage("  " + Description + "\n\r\n\r");

			player.SendMessage("[Exits:");

            player.SendMobileMessage("t=room~name=" + Name + "~id=" + IndexNumber + "~exit=");

            if (Exits.ContainsKey(Directions.North))
            {
                player.SendMessage(" north");
                player.SendMobileMessage("n");
            }
            if (Exits.ContainsKey(Directions.South))
            {
                player.SendMessage(" south");
                player.SendMobileMessage("s");
            }
            if (Exits.ContainsKey(Directions.West))
            {
                player.SendMessage(" west");
                player.SendMobileMessage("w");
            }
            if (Exits.ContainsKey(Directions.East))
            {
                player.SendMessage(" east");
                player.SendMobileMessage("e");
            }
            if (Exits.ContainsKey(Directions.Up))
            {
                player.SendMessage(" up");
                player.SendMobileMessage("u");
            }
            if (Exits.ContainsKey(Directions.Down))
            {
                player.SendMessage(" down");
                player.SendMobileMessage("d");
            }

            /*foreach (KeyValuePair<Directions, Exit> exit in Exits)
			{
				switch (exit.Key)
				{
					case Directions.North:
						player.SendMessage(" north");
                        player.SendMobileMessage("n");
						break;
					case Directions.South:
						player.SendMessage(" south");
                        player.SendMobileMessage("s");
						break;
					case Directions.West:
						player.SendMessage(" west");
                        player.SendMobileMessage("w");
						break;
					case Directions.East:
						player.SendMessage(" east");
                        player.SendMobileMessage("e");
						break;
					case Directions.Up:
						player.SendMessage(" up");
                        player.SendMobileMessage("u");
						break;
					case Directions.Down:
						player.SendMessage(" down");
                        player.SendMobileMessage("d");
						break;
				}
			}*/

			player.SendMessage("]\n\r\n\r");
            player.SendMobileMessage("~\n\r");

            player.SendMobileMessage("t=roomcontents=");
			foreach (Thing thing in Contents)
			{
                if (thing is Mob)
                {
                    Mob mob = (Mob)thing;
                    player.SendMessage(mob.Name + " is here.\n\r");
                    player.SendMobileMessage(mob.Name + "|");
                }
                else if (thing is Player)
                {
                }
			}
            player.SendMobileMessage("~\n\r");
		}

        /// <summary>
        /// Send a message to all valid Mobs in the Room.
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(string message, string mobileMessage)
        {
            foreach (Mob mob in Contents)
            {
                mob.SendMessage(message, mobileMessage);
            }
        }
        public void SendMessage(string message)
        {
            SendMessage(message, "");
        }
        public void SendMobileMessage(string mobileMessage)
        {
            SendMessage("", mobileMessage);
        }
	}
}
