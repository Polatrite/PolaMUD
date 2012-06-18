using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public class AreaLittleMan
    {
        public bool WeightOthers = true;
        public int WeightValue = 1;
        public int[,] Coordinates = new int[1000,1000];
        public List<int> IndexNumbers = new List<int>();

        public AreaLittleMan(int indexStart, int indexCount)
        {
            for (int i = indexStart; i < indexCount + indexStart; i++)
                IndexNumbers.Add(i);
        }

        public Area Generate()
        {
            Area area = new Area();
            int indexNumber;
            Room prevRoom = null;
            Room room = null;
            Exit exit;
            int i = 0;
            int x = 500;
            int y = 500;
            int dir = -1;
            int prevDir = -1;
            Directions[] dirNormal = new Directions[] { Directions.North, Directions.South, Directions.West, Directions.East };
            Directions[] dirOpposite = new Directions[] { Directions.South, Directions.North, Directions.East, Directions.West };
            Random rand = new Random();

            while (i < IndexNumbers.Count)
            {
                indexNumber = IndexNumbers[i];

                room = new Room(indexNumber);
                area.Rooms.Add(room);
                Coordinates[x, y] = indexNumber;

                if (prevRoom != null)
                {
                    while (true)
                    {
                        dir = rand.Next(0, 3);

                        if (dir != -1 && prevDir != -1 && dirNormal[dir] == dirOpposite[prevDir])
                            continue;

                        room.Exits[dirNormal[dir]] = new Exit(indexNumber, prevRoom.IndexNumber);
                        prevRoom.Exits[dirOpposite[dir]] = new Exit(prevRoom.IndexNumber, indexNumber);
                        break;
                    }

                    Global.Log("Created " + prevRoom.IndexNumber + " to " + indexNumber + " (" + dir.ToString() + ")");
                }

                prevDir = dir;
                prevRoom = room;
                i++;
            }

            return area;
        }
    }
}
