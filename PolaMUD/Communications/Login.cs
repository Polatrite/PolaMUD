using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    public static partial class Parser
    {
        /// <summary>
        /// If our connection doesn't have a Player yet, everything is sent here to handle 
        /// login and Player creation.
        /// </summary>
        /// <param name="conn">The TelnetConnection the input is coming from</param>
        /// <param name="line">The input string</param>
        static public void LoginInterpret(TelnetConnection conn, string line)
        {
            Player player = new Player();
            player.MaxHealth = 200;
            player.Health = 200;
            player.PhysicalPower = 10;
            player.MagicPower = 10;
            player.Name = line;
            player.HandlingName = line;
            player.Connection = conn;

            foreach (Skill skill in Global.SkillTable.Values)
            {
                if (skill.Name == "Autoattack")
                    continue;
                player.Skills.Add(skill.Name, new SkillInstance(skill));
            }

            player.SkillSlots.Insert(0, player.Skills["Backstab"]);
            player.SkillSlots.Insert(1, player.Skills["Flurry"]);
            player.SkillSlots.Insert(2, player.Skills["Savage Strike"]);
            conn.SetPlayer(player);

            InputStringMenu strMenu = new InputStringMenu(player, "HandlePasswordInput");
            player.Menu = strMenu;
            player.SendMessage("Password: ");
        }
    }

    public partial class Player
    {
        /// <summary>
        /// Password input dialog displayed immediately on login.
        /// </summary>
        /// <param name="command"></param>
        public void HandlePasswordInput(string command)
        {
            Argument arg1 = Parser.GetArgument(command, 1);

            if (!Authenticate(arg1.Text))
            {
                // disconnect bad password
                SendMessage("That's a bad password young man!\n\r");
                return;
            }

            DynamicMenu nextMenu = new DynamicMenu(this, "HandleClientMenu", 
                "Please select your client type:\n\r- telnet\n\r- full\n\r- android\n\r");
            nextMenu.List.Add("telnet", new Command("telnet", "", false, "Sets your client type as a telnet connection (telnet, terminal, MUD client)"));
            nextMenu.List.Add("full", new Command("full", "", false, "Sets your client type as the official rich client"));
            nextMenu.List.Add("android", new Command("android", "", false, "Sets your client type as an Android phone"));
            Menu = nextMenu;
        }

        /// <summary>
        /// Client selection menu generally displayed on login. Designed to be automatically negotiated by clients.
        /// </summary>
        /// <param name="command"></param>
        public void HandleClientMenu(string command)
        {
            Argument arg1 = Parser.GetArgument(command, 1);
            Skill skill = null;
            switch (arg1.Text)
            {
                case "telnet":
                    ClientType = ClientType.Telnet;
                    CombatType = CombatType.Realtime;
                    break;
                case "full":
                    ClientType = ClientType.Full;
                    CombatType = CombatType.Realtime;
                    break;
                case "android":
                    ClientType = ClientType.Android;
                    CombatType = CombatType.TurnBased;
                    break;
            }

            Menu = null;

            SendMessage("Thanks, " + Name + ". Please enjoy your stay.\n\r\n\r");
            Room = Global.Limbo.Add(this, this.Name + " has arrived.\n\r");
        }

    }

}
