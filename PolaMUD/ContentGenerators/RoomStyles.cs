using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public static class RoomStyles
    {
        public static Dictionary<Terrain, List<RoomStyle>> Styles = new Dictionary<Terrain, List<RoomStyle>>();

        public static void Generate()
        {
            List<RoomStyle> styles;
            foreach(Terrain terrain in Global.TerrainTable.Values)
            {
                switch(terrain.Name)
                {
                    case "Grass":
                        styles = new List<RoomStyle>();
                        styles.Add(new RoomStyle("Open Plains", "", terrain));
                        styles.Add(new RoomStyle("The Grassy Plains", "", terrain));
                        styles.Add(new RoomStyle("Long Plains", "", terrain));
                        styles.Add(new RoomStyle("Open Meadow", "", terrain));
                        styles.Add(new RoomStyle("Worn Plains", "", terrain));
                        Styles.Add(terrain, styles);
                        break;
                }
            }
        }
    }

    public class RoomStyle
    {
        public string Name;
        public string Description;
        public Terrain Terrain;

        public RoomStyle(string name, string desc, Terrain terrain)
        {
            Name = name;
            Description = desc;
            Terrain = terrain;
        }
    }
}
