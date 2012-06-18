using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace PolaMUD
{
	public class Area
	{
        public string Name;
        public List<Mob> Mobs = new List<Mob>();
        public List<Room> Rooms = new List<Room>();

		public Area()
		{
		}

		public bool Save()
		{
			return true;
		}

        public void ForceSpawn()
        {
            Spawn(true);
        }

        public void Spawn()
        {
            Spawn(false);
        }

        private void Spawn(bool force = false)
        {
            foreach (Room room in Rooms)
            {
                foreach (MobReset mobreset in room.MobResets)
                {
                    // if we haven't forced a respawn, lets check timers
                    if (!force)
                    {
                        if (mobreset.RespawnTrack < mobreset.Respawn)
                        {
                            mobreset.RespawnTrack++;
                            continue;
                        }
                    }

                    int count = 0;
                    foreach (Mob mob in mobreset.Room.Contents)
                    {
                        if (mob.IndexNumber == mobreset.IndexNumber)
                            count++;
                    }

                    while (count < mobreset.MaximumRoom)
                    {
                        mobreset.Spawn();
                        count++;
                    }

                    mobreset.RespawnTrack = 0;
                }
            }
        }

		public bool Load(string name)
		{
			XmlDocument document = new XmlDocument();
			XmlNode collection;
			XmlElement element;

			try
			{
				document.Load(name);
			}
			catch
			{
				Global.Error("ERROR: Area '" + name + "' not found.\n");
				return false;
			}

            element = document.DocumentElement;
            Name = element.GetAttribute("name");

            Global.Log("  " + Name + ":\n");

			collection = element["rooms"];
			Room newRoom;
			foreach (XmlNode node in collection.ChildNodes)
			{
				newRoom = new Room(Convert.ToInt32(node.Attributes["index"].Value));
				newRoom.Load(node, this);
                Rooms.Add(newRoom);
			}

			collection = element["mobs"];
			Mob newMob;
			foreach (XmlNode node in collection.ChildNodes)
			{
				newMob = new Mob(node);
			}

            Global.AreaTable.Add(Global.AreaTable.Count, this);
            Global.Log("  " + Name + " loaded, ");

            ForceSpawn();

            Global.Log("spawned\n");

			return true;
		}
	}
}
