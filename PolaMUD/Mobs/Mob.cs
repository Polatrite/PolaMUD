using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Runtime.Serialization;

namespace PolaMUD
{
    [Serializable]
	public partial class Mob : Thing, ISerializable
	{
		public Room Room;

		public int Level = 1;
		public decimal Experience = 0;
		public decimal ExperienceToLevel = 20;
        public decimal ExperienceReward = 10;

        public RaceType Race = RaceType.Humanoid;
        public string Description;
        public string RoomDescription;

		public decimal Coins = 0;

        public Gender Gender = Gender.Male;

		public int Health = 10;
		public int MaxHealth = 10;

        public int PhysicalPower = 0;
        public int PhysicalHit = 0;
        public int PhysicalCritical = 0;
        public int PhysicalCriticalDamage = 0;
        public int Leech = 0;
        public int Wounding = 0;

        public int MagicPower = 0;
        public int MagicHit = 0;
        public int MagicalCritical = 0;
        public int MagicalCriticalDamage = 0;
        public int Drain = 0;
        public int Tormenting = 0;

        public int Armor = 0;
        public int Resistance = 0;
        public int Evasion = 0;
        public int Reflection = 0;
        public int Mediation = 0;
        public int Persistence = 0;
        public int Aggression = 0;



        public int HealthRegen = 0;
        public int ManaRegen = 0;

        public Dictionary<string, SkillInstance> Skills = new Dictionary<string, SkillInstance>();
        public List<SkillInstance> SkillSlots = new List<SkillInstance>();
		public Dictionary<int, Quest> Quests = new Dictionary<int, Quest>();
		public List<Affect> Affects = new List<Affect>();

        [NonSerialized]
        public Mob TargetEnemy;
        [NonSerialized]
        public Mob TargetAlly;
        [NonSerialized]
        public CombatType CombatType = CombatType.Realtime;
        public Battle Battle;

        [NonSerialized]
        public int WaitPulses = 0;

        [NonSerialized]
        public ClientType ClientType = ClientType.Telnet;
        [NonSerialized]
        public Queue<string> IncomingBuffer = new Queue<string>();
        [NonSerialized]
        public Queue<string> OutgoingBuffer = new Queue<string>();
        [NonSerialized]
        public CommandMenu Menu;

		public Mob()
		{
		}

		/// <summary>
		/// Creates a new template for the specified mob and adds it to the reference table of every mob template. Use Loader.NewMob() to create a new instance of a mob.
		/// </summary>
		/// <param name="index"></param>
        public Mob(int index)
        {
			IndexNumber = index;
            Load(Global.MobLoadTable[index]);
    		Global.Mobs.Add(this);
            Skills.Add("Autoattack", new SkillInstance(Global.SkillTable["Autoattack"], new SkillAI()));
        }

        public Mob(XmlNode mob)
        {
            IndexNumber = Convert.ToInt32(mob.Attributes["index"].Value);
            Global.MobLoadTable.Add(IndexNumber, mob);
            Load(mob);
        }

        public void Load(XmlNode mob)
        {
            IndexNumber = Convert.ToInt32(mob.Attributes["index"].Value);

            if (mob.Attributes["hp"].Value != "")
                MaxHealth = Convert.ToInt32(mob.Attributes["hp"].Value);
            if (mob.Attributes["exp"].Value != "")
                ExperienceReward = Convert.ToInt32(mob.Attributes["exp"].Value);
            if (mob.Attributes["coins"].Value != "")
                Coins = Convert.ToInt32(mob.Attributes["coins"].Value);
            if (mob.Attributes["physicalpower"].Value != "")
                PhysicalPower = Convert.ToInt32(mob.Attributes["physicalpower"].Value);
            if (mob.Attributes["magicpower"].Value != "")
                MagicPower = Convert.ToInt32(mob.Attributes["magicpower"].Value);

            Name = mob["name"].InnerText;
            HandlingName = mob["handlingname"].InnerText;
            Description = mob["desc"].InnerText;
            RoomDescription = mob["roomdesc"].InnerText;

            Health = MaxHealth;

            foreach (XmlNode skillNode in mob["skills"].ChildNodes)
            {
                Skill skill = Global.SkillTable[skillNode.Attributes["name"].Value];
                SkillInstance skillInstance = new SkillInstance(skill, new SkillAI());
                XmlNode skillAInode = skillNode["skillai"];
                skillInstance.AI.RandomWeight = Convert.ToInt32(skillAInode.Attributes["randomweight"].Value);
                Skills.Add(skillInstance.Skill.Name, skillInstance);
                SkillSlots.Add(skillInstance);
            }

            Global.Log("    Mob: " + Name + " with ID #" + IndexNumber + "\n");
        }

        public void iMob()
        {
            Global.Mobs.Add(this);
        }

        ~Mob()
        {
            Global.Mobs.Remove(this);
        }

        public Mob ShallowCopy()
        {
            return (Mob)this.MemberwiseClone();
        }

        /// <summary>
        /// Move the Mob from current Room to destination Room, sending no messages.
        /// </summary>
        /// <param name="destination">The destination Room</param>
        /// <returns></returns>
        public bool Move(Room destination)
        {
            return Move(destination, "");
        }

        /// <summary>
        /// Move the Mob from current room to destination room, sending direction-based messages 
        /// if provided.
        /// </summary>
        /// <param name="destination">The destination Room</param>
        /// <param name="direction">The direction the Mob is moving</param>
        /// <returns></returns>
        public bool Move(Room destination, string direction)
        {
            if (direction != "")
            {
                if (Room != null)
                    Room.Remove(this, Name + " leaves " + direction + ".\n\r");
                destination.Add(this, Name + " has arrived.\n\r");
            }
            else
            {
                if (Room != null)
                    Room.Remove(this);
                destination.Add(this);
            }

            Room = destination;

            return true;
        }

        /// <summary>
        /// Does nothing for Mobs.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual bool SendMessage(string message)
        {
            SendMessage(message, "");
            return false;
        }

        /// <summary>
        /// Does nothing for Mobs.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public virtual bool SendMessage(string message, string mobileMessage)
        {
            return false;
        }

        /// <summary>
        /// Reward the Mob with experience, potentially causing it to gain 
        /// (TODO: or lose) levels.
        /// </summary>
        /// <param name="reward"></param>
        public void RewardExperience(decimal reward)
        {
            Experience += reward;

            SendMessage(Colors.BRIGHT_CYAN + "You gain " + reward + " experience!\n\r", "dupe");

            while (Experience >= ExperienceToLevel)
            {
                LevelUp();
            }
        }

		/// <summary>
		/// Add coins to the Mob
		/// </summary>
		/// <param name="reward"></param>
		public void AddCoins(decimal reward)
		{
			if (reward < 1)
				return;

			Coins += reward;

			SendMessage(Colors.BRIGHT_YELLOW + "You earn " + reward + " " + ((reward == 1) ? "coin" : "coins") + "!\n\r");
		}

        public void Restore()
        {
            Health = MaxHealth;


            List<Affect> ToRemove = new List<Affect>();
            foreach (Affect affect in Affects)
            {
                switch (affect.AffectType)
                {
                    case AffectType.Debuff:
                    case AffectType.DOT:
                        ToRemove.Add(affect);
                        break;
                }
            }

            foreach (Affect affect in ToRemove)
            {
                Affects.Remove(affect);
            }
        }

		/// <summary>
        /// Cause the Mob to gain a level unconditionally.
        /// </summary>
        public void LevelUp()
        {
            Level++;

            MaxHealth += 50;

            PhysicalPower += 5;
            MagicPower += 5;

            Restore();

            Global.SendToAll(Colors.GREEN + "(INFO) " + Name + " has gained a level!\n\r", "");

            Experience = 0;
            ExperienceToLevel = 10 * Level;
			//ExperienceToLevel = ((50 * Level * Level * Level) - (150 * Level * Level + 250 * Level)) / 5;
        }

        /// <summary>
        /// Remove all references to this Mob, preparing it for garbage collection.
        /// </summary>
        public void Delete()
        {
            foreach(Affect affect in Affects)
                
            Affects = new List<Affect>();

            Room.Remove(this, "");

            Room = null;

            foreach (Mob mob in Global.Mobs)
            {
                if (mob.TargetEnemy == this)
                    mob.TargetEnemy = null;
                if (mob.TargetAlly == this)
                    mob.TargetAlly = null;
            }
        }




        public Mob(SerializationInfo info, StreamingContext ctxt)
        {
            Type mobtype = this.GetType();

            FieldInfo[] fields = mobtype.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].IsNotSerialized)
                    fields[i].SetValue(this, info.GetValue(fields[i].Name, fields[i].FieldType));
            }

            PropertyInfo[] props = mobtype.GetProperties();
            PropertyAttributes pattr;
            for (int i = 0; i < props.Length; i++)
            {
                pattr = props[i].Attributes;
                if (!pattr.ToString().Contains("NotSerialized"))
                    props[i].SetValue(this, info.GetValue(props[i].Name, props[i].PropertyType), null);
            }

            iMob();

            /*
            if (props[i].CanWrite && props[i].Attributes !propertiesToIgnore.Contains(props[i].Name))
            {
                props[i].SetValue(this, props[i].GetValue(from, null), null);
            }
        }

        Level = (int)info.GetValue("Level", typeof(int));
        (int)info.GetValue("Experience", typeof(int));
        (int)info.GetValue("ExperienceToLevel", typeof(int));
        (int)info.GetValue("ExperienceReward", typeof(int));
        (int)info.GetValue("Description", Description);
        (int)info.GetValue("RoomDescription", RoomDescription);
        (int)info.GetValue("Coins", typeof(int));
        (int)info.GetValue("Gender", Gender);
        (int)info.GetValue("Health", typeof(int));
        (int)info.GetValue("MaxHealth", typeof(int));
        (int)info.GetValue("Mana", typeof(int));
        (int)info.GetValue("MaxMana", typeof(int));
        (int)info.GetValue("Strength", typeof(int));
        (int)info.GetValue("PhysicalPower", typeof(int));
        (int)info.GetValue("WieldWeight", typeof(int));
        (int)info.GetValue("CarryWeight", typeof(int));
        (int)info.GetValue("Intelligence", typeof(int));
        (int)info.GetValue("MagicPower", typeof(int));
        (int)info.GetValue("ManaRegen", typeof(int));
        (int)info.GetValue("MagicalCritical", typeof(int));
        (int)info.GetValue("MagicHit", typeof(int));
        (int)info.GetValue("Vitality", typeof(int));
        (int)info.GetValue("HealthRegen", typeof(int));
        (int)info.GetValue("PhysicalDamageReduction", typeof(int));
        (int)info.GetValue("MagicalDamageReduction", typeof(int));
        (int)info.GetValue("Dexterity", typeof(int));
        (int)info.GetValue("PhysicalHit", typeof(int));
        (int)info.GetValue("PhysicalCritical", typeof(int));
        (int)info.GetValue("Dodge", typeof(int));
        (int)info.GetValue("Skills", Skills);
        (int)info.GetValue("Quests", Quests);
        (int)info.GetValue("Affects", Affects);*/
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            Type mobtype = this.GetType();

            FieldInfo[] fields = mobtype.GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                if (!fields[i].IsNotSerialized)
                {
                    info.AddValue(fields[i].Name, fields[i].FieldType);
                    Global.Log(fields[i].Name);
                }
            }

            PropertyInfo[] props = mobtype.GetProperties();
            PropertyAttributes pattr;
            for (int i = 0; i < props.Length; i++)
            {
                pattr = props[i].Attributes;
                if (!pattr.ToString().Contains("NotSerialized"))
                {
                    info.AddValue(props[i].Name, props[i].PropertyType);
                    Global.Log(props[i].Name);
                }
            }



            /*info.AddValue("Level", Level);
            info.AddValue("Experience", Experience);
            info.AddValue("ExperienceToLevel", ExperienceToLevel);
            info.AddValue("ExperienceReward", ExperienceReward);
            info.AddValue("Description", Description);
            info.AddValue("RoomDescription", RoomDescription);
            info.AddValue("Coins", Coins);
            info.AddValue("Gender", Gender);
            info.AddValue("Health", Health);
            info.AddValue("MaxHealth", MaxHealth);
            info.AddValue("Mana", Mana);
            info.AddValue("MaxMana", MaxMana);
            info.AddValue("Strength", Strength);
            info.AddValue("PhysicalPower", PhysicalPower);
            info.AddValue("WieldWeight", WieldWeight);
            info.AddValue("CarryWeight", CarryWeight);
            info.AddValue("Intelligence", Intelligence);
            info.AddValue("MagicPower", MagicPower);
            info.AddValue("ManaRegen", ManaRegen);
            info.AddValue("MagicalCritical", MagicalCritical);
            info.AddValue("MagicHit", MagicHit);
            info.AddValue("Vitality", Vitality);
            info.AddValue("HealthRegen", HealthRegen);
            info.AddValue("PhysicalDamageReduction", PhysicalDamageReduction);
            info.AddValue("MagicalDamageReduction", MagicalDamageReduction);
            info.AddValue("Dexterity", Dexterity);
            info.AddValue("PhysicalHit", PhysicalHit);
            info.AddValue("PhysicalCritical", PhysicalCritical);
            info.AddValue("Dodge", Dodge);
            info.AddValue("Skills", Skills);
            info.AddValue("Quests", Quests);
            info.AddValue("Affects", Affects);*/
        }


    }
}
