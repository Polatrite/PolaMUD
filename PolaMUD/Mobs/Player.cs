using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
	public class Player : Mob
	{
		public TelnetConnection Connection;

		public Player()
		{
			Global.Players.Add(this);
            Skills.Add("Autoattack", new SkillInstance(Global.SkillTable["Autoattack"]));
		}

        ~Player()
        {
            Global.Players.Remove(this);
        }

        public void iPlayer()
        {
            Global.Players.Add(this);
            
        }

        /// <summary>
        /// Add a message to the outgoing buffer, sending it to the Player at the next opportunity.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public override bool SendMessage(string message)
        {
            return SendMessage(message, "");
        }
        public override bool SendMessage(string message, string mobileMessage)
        {
            if (ClientType == ClientType.Android)
            {
                if (mobileMessage.Length > 0)
                {
                    if(mobileMessage == "dupe")
                        OutgoingBuffer.Enqueue("t=output~txt=" + message);
                    else
                        OutgoingBuffer.Enqueue(mobileMessage);
                }
            }
            else
            {
                if (message.Length > 0)
                    OutgoingBuffer.Enqueue(message);
            }

            return true;
        }
        public bool SendMobileMessage(string mobileMessage)
        {
            return SendMessage("", mobileMessage);
        }

        /// <summary>
        /// Flushes the entire outbound buffer to the Player's connection. 
        /// If the Player is not connected, nothing is done.
        /// </summary>
        /// <returns></returns>
        public bool SendOutgoingBuffer()
        {
            if (OutgoingBuffer.Count == 0)
                return false;

            string message;

            while (true)
            {
                if (Connection == null)
                    return false;

                if (Connection.Writer == null)
                    return false;

                if (OutgoingBuffer.Count == 0)
                    break;

                message = OutgoingBuffer.Dequeue();

                try
                {
                    // save bandwidth by clearing color codes only on newlines
                    //TODO: check CPU usage
                    if (message.EndsWith("\n\r")) 
                        Connection.Writer.Write(message + Colors.CLEAR);
                    else
                        Connection.Writer.Write(message);
                }
                catch (Exception e)
                {
                    Connection.Writer = null;
                    return false;
                }
            }

            if (ClientType != ClientType.Android)
            {
                Connection.Writer.Write("\n\r");
                Connection.Writer.Write(GetPrompt());
            }

            return true;
        }

        public string GetPrompt()
        {
            string prompt = "(" + Colors.BRIGHT_RED + Health + "/" + MaxHealth + "hp " 
                + Colors.CLEAR + ") ";

            if (TargetEnemy != null)
            {
                prompt += " " + Colors.RED + TargetEnemy.Health + "/" + TargetEnemy.MaxHealth + "ehp" + Colors.CLEAR + ") ";
            }

            return prompt;
        }

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

        public void HandleTurnBattleMenu(string command)
        {
            if(TargetEnemy == null || Battle == null || !(Battle is TurnBattle))
            {
                SendMessage("You are not in a battle!\n\r");
                Menu = null;
                Battle = null;
                TargetEnemy = null;
                return;
            }

            if (((TurnBattle)Battle).CurrentTurn != this)
            {
                SendMessage("It's not yet your turn!\n\r", "dupe");
                return;
            }

            Argument arg1 = Parser.GetArgument(command, 1);
            SkillInstance skill = null;
            switch(arg1.Text)
            {
                case "skill1":
                    skill = SkillSlots[0];
                    break;
                case "skill2":
                    skill = SkillSlots[1];
                    break;
                case "skill3":
                    skill = SkillSlots[2];
                    break;
                case "skill4":
                    skill = SkillSlots[3];
                    break;
                case "skill5":
                    skill = SkillSlots[4];
                    break;
                case "skill6":
                    skill = SkillSlots[5];
                    break;
            }

            if (skill == null)
            {
                SendMessage("You don't have an ability in that slot!");
                return;
            }

            skill.Arguments = command;
            ((TurnBattle)Battle).HandleTurn(skill);

        }
	}
}
