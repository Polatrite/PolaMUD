using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PolaMUD
{
    public class Command
    {
        public string Text;
        public string Description;	// optional
        public string Method;
        public bool Administrative;
        public int ClientRequirement;

        public Command(string text, string method, bool administrative, string description)
        {
            Text = text;
            Description = description;
            Administrative = administrative;
            Method = method;
            ClientRequirement = 0;
        }

        public Command(string text, string method, bool administrative, string description, int clientRequirement)
        {
            Text = text;
            Description = description;
            Administrative = administrative;
            Method = method;
            ClientRequirement = clientRequirement;
        }
    }



    public partial class Commands : CommandMenu
    {
        public new Dictionary<string, Command> List = new Dictionary<string, Command>();

        /// <summary>
        /// Singleton class. Creating this loads all commands into the parser. 
        /// TODO: Safe-lock this so that this class can be disposed and re-created at runtime 
        /// to "reload" commands... for the ability to load commands from XML later, instead of hardcoded.)
        /// </summary>
        public Commands()
        {
            // Note: Commands are parsed in the order they are added to the list, to give 
            //   commands priority, add them to the top of the list. (e.g. s will always match 
            //   south, because it always comes before score)
            // NOTE: KEY values for commands must always be LOWER CASE.
            List.Add("north", new Command("north", "CommandMoveNorth", false, "Moves the character north"));
            List.Add("south", new Command("south", "CommandMoveSouth", false, "Moves the character south"));
            List.Add("east", new Command("east", "CommandMoveEast", false, "Moves the character east"));
            List.Add("west", new Command("west", "CommandMoveWest", false, "Moves the character west"));
            List.Add("up", new Command("up", "CommandMoveUp", false, "Moves the character up"));
            List.Add("down", new Command("down", "CommandMoveDown", false, "Moves the character down"));
            List.Add("debug", new Command("debug", "CommandDb", false, "DEBUG COMMAND", 20));
            List.Add("kill", new Command("kill", "CommandKill", false, "Attacks another mob", 20));
            List.Add("battle", new Command("battle", "CommandBattle", false, "Begins a turn-based battle with another mob"));
            List.Add("skill", new Command("skill", "CommandSkill", false, "Perform a certain skill action", 20));
            List.Add("who", new Command("who", "CommandWho", false, "Displays who is online"));
            List.Add("say", new Command("say", "CommandSay", false, "Says something to your current room"));
            List.Add("look", new Command("look", "CommandLook", false, "Look at your room, or another mob or object"));
            List.Add("score", new Command("score", "CommandScore", false, "Check your general stats"));
            List.Add("screen", new Command("screen", "CommandScreen", false, "Retrieves screen output information for Android"));
            List.Add("testact", new Command("testAct", "CommandTestAct", false, "Test the NarrateAction method.  REMOVE THIS COMMAND.", 20));
            List.Add("aload", new Command("aload", "CommandAload", false, "Load an area from a file", 20));
            List.Add("backstab", new Command("backstab", "CommandSkill", false, "Perform a certain skill action", 20));
            List.Add("flurry", new Command("flurry", "CommandSkill", false, "Perform a certain skill action", 20));
            List.Add("savage", new Command("savage", "CommandSkill", false, "Perform a certain skill action", 20));
        }

        public bool CommandDb(Player user, Command command, string text)
        {
            if (Parser.GetArgument(text, 2).Text == "entities")
            {
                if (Parser.GetArgument(text, 3).Text == "list")
                {
                    int count = 1;
                    foreach (Thing thing in Global.Things)
                    {
                        user.SendMessage(count + ".  " + "[" + thing.Name + "] #" + thing.IndexNumber + "; type: " + thing.GetType() + "\n\r");
                        count++;
                    }
                }
            }
            if (Parser.GetArgument(text, 2).Text == "menu")
            {
                DynamicMenu menu = new DynamicMenu(user, "HandleMenu");
                menu.List.Add("waffle", new Command("waffle", "", false, "Menu item 1"));
                menu.List.Add("carrot", new Command("carrot", "", false, "Menu item 2"));
                menu.List.Add("apple", new Command("apple", "", false, "Menu item 3"));
                user.Menu = menu;
                user.SendMessage("Please select from the following: waffle, carrot, apple\n\r");
            }
            if (Parser.GetArgument(text, 2).Text == "littleman")
            {
                AreaLittleMan littleMan = new AreaLittleMan(300, 7);
                Area area = littleMan.Generate();
                Room room = area.Rooms[0];
                Instance instance = InstanceManager.NewInstance(user, Convert.ToInt32(Parser.GetArgument(text, 3).Text));
                instance.Area = area;
                user.Move(room);
            }
            if (Parser.GetArgument(text, 2).Text == "map")
            {
                Area area = user.Room.Area;

            }
            if (Parser.GetArgument(text, 2).Text == "instance")
            {
                InstanceManager.NewInstance(user, Convert.ToInt32(Parser.GetArgument(text, 3).Text));


            }
            if (Parser.GetArgument(text, 2).Text == "killinstance")
            {
                InstanceManager.RemoveInstance(user);
            }
            if (Parser.GetArgument(text, 2).Text == "bat")
            {
                Mob mob = new Mob();
                mob.Name = "test mob #" + Combat.Random.Next(0, 9);
                mob.Skills.Add("Autoattack", new SkillInstance(Global.SkillTable["Autoattack"], new SkillAI()));
            }

            return true;
        }

        public bool CommandWho(Player user, Command command, string text)
        {
            int count = 0;
            foreach (Player player in Global.Players)
            {
                user.SendMessage("[" + String.Format("{0:00}", player.Level) + "] " + player.Name + " has no title!\n\r", "dupe");
                count++;
            }
            user.SendMessage("\n\r");
            user.SendMessage("Total players: " + count + "\n\r", "dupe");

            return true;
        }

        public bool CommandSay(Player user, Command command, string text)
        {
            text = Parser.GetStringArgument(text, 2).Text;

            foreach (Player player in user.Room.Contents)
            {
                player.SendMessage(Colors.GREEN + user.Name + " says '" + text + "'\n\r", "dupe");
            }

            return true;
        }

        public bool CommandLook(Player user, Command command, string text)
        {
            if (Parser.GetArgument(text, 2).Text != "")
            {
                // do some crap for arguments here
            }
            user.Room.Display(user);

            return true;
        }

        public bool CommandKill(Player user, Command command, string text)
        {
            Type type = typeof(Mob);
            Argument arg1 = Parser.GetArgument(text, 2, user, type, SearchLocations.Room);
            if (arg1.Reference == null)
            {
                user.SendMessage("They aren't here!\n\r");
                return false;
            }

            Mob target = (Mob)arg1.Reference;

            user.WaitPulses += Global.RoundDuration;

            if (user.TargetEnemy == null)
                user.TargetEnemy = target;

            Combat.OneHit(user, target);

            return true;
        }

        public bool CommandBattle(Player user, Command command, string text)
        {
            if (user.ClientType != ClientType.Android)
                user.SendMessage("Hey, you're not supposed to be using this combat system!\n\rI'll let it slide, this time...\n\r", "dupe");
            Type type = typeof(Mob);
            Argument arg1 = Parser.GetArgument(text, 2, user, type, SearchLocations.Room);
            if (arg1.Reference == null)
            {
                user.SendMessage("They aren't here!\n\r");
                return false;
            }

            Mob target = (Mob)arg1.Reference;

            Combat.StartTurnCombat(user, target);

            DynamicMenu menu = new DynamicMenu(user, "HandleTurnBattleMenu");
            menu.List.Add("skill1", new Command("skill1", "", false, "Use skill slot #"));
            menu.List.Add("skill2", new Command("skill2", "", false, "Use skill slot #"));
            menu.List.Add("skill3", new Command("skill3", "", false, "Use skill slot #"));
            menu.List.Add("skill4", new Command("skill4", "", false, "Use skill slot #"));
            menu.List.Add("skill5", new Command("skill5", "", false, "Use skill slot #"));
            menu.List.Add("skill6", new Command("skill6", "", false, "Use skill slot #"));
            menu.List.Add("flee", new Command("flee", "", false, "Run away from the fight"));
            user.Menu = menu;

            return true;
        }

        public bool CommandScore(Player user, Command command, string text)
        {
            user.SendMessage("   " + user.Name + "\n\r");
            user.SendMessage(Colors.BRIGHT_RED + "Health: " + user.Health + "/" + user.MaxHealth + "\n\r");
            //user.SendMessage(Colors.BRIGHT_BLUE + "Mana:   " + user.Mana + "/" + user.MaxMana + "\n\r");
            user.SendMessage("Experience:   " + user.Experience + "/" + user.ExperienceToLevel + "\n\r");
            user.SendMessage("Physical:     " + user.PhysicalPower + "\n\r");
            user.SendMessage("Magical:      " + user.MagicPower + "\n\r");
            //user.SendMessage("Vitality:     " + user.Vitality + "\n\r");
            //user.SendMessage("Dexterity:    " + user.Dexterity + "\n\r");

            user.SendMobileMessage("t=stats~name=" + user.Name + "~hp=" + user.Health + "~mhp=" + user.MaxHealth +
                "~exp=" + user.Experience + "~exptnl=" + user.ExperienceToLevel + /*"~str=" + user.Strength +
                "~int=" + user.Intelligence + "~vit=" + user.Vitality + "~dex=" + user.Dexterity +*/ "\n\r");

            return true;
        }

        public bool CommandScreen(Player user, Command command, string text)
        {
            if (Parser.GetArgument(text, 2).Text == "turnbattle")
            {
                if (user.Battle != null && user.CombatType == CombatType.Realtime)
                {
                    Mob mob = null;
                    foreach (Mob batmob in user.Battle.Participants)
                    {
                        if (!(batmob is Player))
                        {
                            mob = batmob;
                        }
                    }

                    user.SendMobileMessage(
                        "t=screen" +
                        "~ename=" + mob.Name +
                        "~erace=" + mob.Race.ToString() +
                        "~ehp=" + mob.Health +
                        "~emhp=" + mob.MaxHealth +
                        "~name=" + user.Name +
                        "~race=" + user.Race.ToString() +
                        "~hp=" + user.Health +
                        "~mhp=" + user.MaxHealth +
                        "\n\r");
                }
            }


            return true;
        }

        public bool CommandTestAct(Player user, Command command, string text)
        {
            Type type = typeof(Mob);
            Argument arg1 = Parser.GetArgument(text, 2, user, type, SearchLocations.Room);
            if (arg1.Reference == null)
            {
                //user.SendMessage("They aren't here!\n\r");
                type = typeof(Player);
                arg1 = Parser.GetArgument(text, 2, user, type, SearchLocations.Room);
                if (arg1.Reference == null)
                {
                    user.SendMessage("They aren't here!\n\r");
                    return false;
                }
            }

            Mob target = (Mob)arg1.Reference;

            Communications.DeliverMessage
                (user, target, MessageVector.Character, "You test the NarrateAction method!  $N picks $S nose!\n\r", "");
            Communications.DeliverMessage
                (user, target, MessageVector.Target, "$n tests the NarrateAction method!  You pick your nose!\n\r", "");
            Communications.DeliverMessage
                (user, target, MessageVector.ThirdParty, "$n tests the NarrateAction method!  $N picks $S nose!\n\r", "");

            return true;
        }

		public bool CommandAload(Player user, Command command, string text)
		{
			text = Parser.GetStringArgument(text, 2).Text;

			Area area = new Area();
			area.Load(text);

			return true;
		}

        public bool CommandSkill(Player user, Command command, string text)
        {
            string skill = command.Text;

            //TODO: Make this execute skills with the generic "skill" command so that we don't have to have a verb for
            //      every skill for debugging purposes
            if (skill == "skill")
            {
                skill = Parser.GetArgument(text, 2).Text;
				text = text.Substring(6);
            }

            bool found = false;
            string literalKey = "";

            foreach (string key in user.Skills.Keys)
            {
                if (key.ToLower().StartsWith(skill))
                {
                    found = true;
                    literalKey = key;
                    break;
                }
            }
            if (found)
            {
                user.Skills[literalKey].Skill.Action(user, text);
            }
            else
            {
                user.SendMessage("You don't know that skill!\n\r");
            }
            return true;
        }


    }
}
