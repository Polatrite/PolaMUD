using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public static class InstanceManager
    {
        public static Dictionary<Player, Instance> Instances = new Dictionary<Player,Instance>();
        public static bool[] InstanceTable = new bool[100000];
        public static int FullInstanceIndexStart = 100000;

        /// <summary>
        /// Creates a new Instance belonging to the specified Player with the specified Quantity of indexes reserved.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static Instance NewInstance(Player player, int quantity)
        {
            if (Instances.ContainsKey(player))
            {
                Global.Error("Tried to assign a new instance to " + player.Name + " but one exists\n\r");
                return null;
            }

            int indexStart = 0;
            
            // TODO: check CPU usage
            int consecutiveCount = 0;
            for(int i = 0; i < FullInstanceIndexStart; i++)
            {
                if (InstanceTable[i] == false)
                {
                    if (indexStart == 0)
                        indexStart = i;

                    consecutiveCount++;
                }
                else
                {
                    consecutiveCount = 0;
                    indexStart = 0;
                }

                if (consecutiveCount >= quantity)
                    break;
            }

            int count = 0;
            for (int i = 0; i < quantity; i++)
            {
                InstanceTable[indexStart + i] = true;
                Global.Log("#" + (indexStart + i) + " marked, ");
                count++;
            }
            Global.Log("\n\r" + count + " total");

            Instance instance = new Instance(player, indexStart + 100000, indexStart + 100000 + quantity);
            Instances.Add(player, instance);
            return instance;
        }


        /// <summary>
        /// Removes the Instance belonging to Player, freeing all indexes belonging to the instance
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static bool RemoveInstance(Player player)
        {
            if (Instances.ContainsKey(player))
            {
                Instance instance = Instances[player];

                int count = 0;
                for (int i = instance.IndexStart - 100000; i <= instance.IndexEnd - instance.IndexStart; i++)
                {
                    InstanceTable[i] = false;
                    Global.Log("#" + i + " released, ");
                    count++;
                }
                Global.Log("\n\r" + count + " total");

                Instances.Remove(player);

                return true;
            }

            return false;
        }
    }

    public class Instance
    {
        public Player Owner;
        public Area Area;
        public int IndexStart;
        public int IndexEnd;

        public Instance(Player owner, Area area, int indexStart, int indexEnd)
        {
            Owner = owner;
            Area = area;
            IndexStart = indexStart;
            IndexEnd = indexEnd;
        }

        public Instance(Player owner, int indexStart, int indexEnd)
        {
            Owner = owner;
            IndexStart = indexStart;
            IndexEnd = indexEnd;
        }
    }
}
