﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
	public partial class Commands
	{
		public bool CommandMoveNorth(Player user, Command command, string text)
		{
			if (((Room)user.Location).Exits.ContainsKey(Directions.North))
			{
                user.Move(Global.GetRoom(((Room)user.Location).Exits[Directions.North].ToRoom), "north");
			}
			else
			{
				user.SendMessage("You cannot go that way!\n\r", "t=err~id=100\n\r");
				return false;
			}

			return true;
		}

		public bool CommandMoveSouth(Player user, Command command, string text)
		{
			if (((Room)user.Location).Exits.ContainsKey(Directions.South))
			{
                user.Move(Global.GetRoom(((Room)user.Location).Exits[Directions.South].ToRoom), "south");
			}
			else
			{
				user.SendMessage("You cannot go that way!\n\r", "t=err~id=100\n\r");
				return false;
			}

			return true;
		}

		public bool CommandMoveEast(Player user, Command command, string text)
		{
			if (((Room)user.Location).Exits.ContainsKey(Directions.East))
			{
                user.Move(Global.GetRoom(((Room)user.Location).Exits[Directions.East].ToRoom), "east");
			}
			else
			{
				user.SendMessage("You cannot go that way!\n\r", "t=err~id=100\n\r");
				return false;
			}

			return true;
		}

		public bool CommandMoveWest(Player user, Command command, string text)
		{
			if (((Room)user.Location).Exits.ContainsKey(Directions.West))
			{
                user.Move(Global.GetRoom(((Room)user.Location).Exits[Directions.West].ToRoom), "west");
			}
			else
			{
				user.SendMessage("You cannot go that way!\n\r", "t=err~id=100\n\r");
				return false;
			}

			return true;
		}

		public bool CommandMoveUp(Player user, Command command, string text)
		{
			if (((Room)user.Location).Exits.ContainsKey(Directions.Up))
			{
                user.Move(Global.GetRoom(((Room)user.Location).Exits[Directions.Up].ToRoom), "up");
			}
			else
			{
				user.SendMessage("You cannot go that way!\n\r", "t=err~id=100\n\r");
				return false;
			}

			return true;
		}

		public bool CommandMoveDown(Player user, Command command, string text)
		{
			if (((Room)user.Location).Exits.ContainsKey(Directions.Down))
			{
                user.Move(Global.GetRoom(((Room)user.Location).Exits[Directions.Down].ToRoom), "down");
			}
			else
			{
				user.SendMessage("You cannot go that way!\n\r", "t=err~id=100\n\r");
				return false;
			}

			return true;
		}

	}
}
