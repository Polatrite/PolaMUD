using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace PolaMUD
{
	public static partial class Parser
	{
        // More of this partial class exists at:
        // Communications/Login.cs
        // Communications/Logout.cs

        /// <summary>
        /// Retrieves tshe appropriate Argument from the given input string. 
        /// Argument's reference object is automatically retrieved based on the Type and 
        /// SearchLocations enumeration provided.
        /// </summary>
        /// <param name="line">The input string</param>
        /// <param name="index">The index of the desired argument (1 is always the command itself)</param>
        /// <param name="user">The user Mob, for retrieving references based on this Mob's state</param>
        /// <param name="reference">The Type to search for (Mob, Player, etc.)</param>
        /// <param name="searchLocation">A SearchLocations enumeration to use in finding the reference</param>
        /// <returns></returns>
        public static Argument GetArgument(string line, int index, Mob user, Type reference, int searchLocation)
        {
            Argument argument = GetArgument(line, index);

            argument.Text = argument.Text.ToLower();

            switch (searchLocation)
            {
                case SearchLocations.Room:
                    argument.Reference = user.Room.GetThing(reference, argument.Text);
                    break;
                default:
                    break;
            }

            return argument;
        }

		/// <summary>
		/// Retrieves the appropriate argument from the given input string, optionally retrieving 
        /// an object reference based on what you're looking for (Player, Room, etc.)
		/// </summary>
		/// <param name="line">The input string</param>
		/// <param name="index">The index of the desired argument (1 is always the command itself)</param>
		/// <returns></returns>
		static public Argument GetArgument(string line, int index)
		{
			string arg = GetArgumentString(line, index, false);
			Argument argObj = new Argument();
			argObj.Index = 1;
			argObj.Count = 1;
			argObj.Text = arg;
			int temp = -1;

			temp = arg.IndexOf('*');

			if (temp > 0)
			{
				try
				{
					argObj.Count = Convert.ToInt32(arg.Substring(0, temp));
					argObj.Text = arg.Substring(temp+1);
				}
				catch (Exception e)
				{
					return null;
				}
			}

			temp = arg.IndexOf('.');
			if (temp > 0)
			{
				try
				{
					argObj.Index = Convert.ToInt32(arg.Substring(0, temp));
					argObj.Text = arg.Substring(temp+1);
				}
				catch (Exception e)
				{
					return null;
				}
			}

			return argObj;
		}

		/// <summary>
		/// Retrieves a string argument from the given input string. This argument should always 
        /// be the last argument in the line, as this will retrieve the rest of the input string 
        /// after all previous arguments.
		/// </summary>
		/// <param name="line">The input string</param>
		/// <param name="index">The index of the desired argument (1 is always the command itself)</param>
		/// <returns></returns>
		static public Argument GetStringArgument(string line, int index)
		{
			Argument argument = new Argument();
			argument.Index = 1;
			argument.Count = 1;
			argument.Text = GetArgumentString(line, index, true);
			return argument;
		}

		/// <summary>
		/// This returns the string that composes an entire argument, including handling prefixes 
        /// such as 3. or 5*  This is for internal use. 
        /// External calls to the parser should use GetArgument.
		/// </summary>
		/// <param name="line">The input string</param>
		/// <param name="index">The index of the desired argument (1 is always the command itself)</param>
		/// <returns></returns>
		static string GetArgumentString(string line, int index, bool takeAll)
		{
			line = line.Trim();
			char[] cmd = line.ToCharArray();

			int argIndex = 0;
			int quoteIndexBegin = 0;
			int quoteIndexEnd = -1;
			bool quoted = false;
			for (int x = 0; x < cmd.Length; x++)
			{
				if (cmd[x] == '\'' || cmd[x] == '"')
				{
					if (quoted == false)
					{
						quoted = true;
						quoteIndexBegin = x + 1;
					}
					else
					{
						quoted = false;
						argIndex++;

						if (argIndex == index)
						{
							quoteIndexEnd = x;
							break;
						}
					}
				}
				else if (cmd[x] == ' ' && quoted == false)
				{
					if (cmd[x - 1] == '\'' || cmd[x - 1] == '"')
						argIndex--;

					argIndex++;
					if (argIndex == index)
					{
						quoteIndexEnd = x;
						break;
					}
					else
					{
						quoteIndexBegin = x + 1;
					}
				}

				//Console.WriteLine(cmd[x] + " : " + quoteIndexBegin + ", " + quoteIndexEnd + " - " + argIndex);
			}

			if (takeAll || (quoteIndexBegin >= 0 && quoteIndexEnd == -1))
				quoteIndexEnd = line.Length;

			if(quoteIndexEnd > 0)
				return line.Substring(quoteIndexBegin, quoteIndexEnd - quoteIndexBegin);
			else
				return "";
		}

        /// <summary>
        /// Thread-safe callback from each TelnetConnection, parsing input. 
        /// This function is timed internally by PulseTimer().
        /// </summary>
        /// <param name="conn">The TelnetConnection the input is coming from</param>
        /// <param name="line">The input string</param>
		static public void Interpret(TelnetConnection conn, string line)
		{
            // If the connection doesn't have a Player yet, it must be a new connection. 
            // Forward it to LoginInterpret().
			if (conn.GetPlayer() == null)
			{
                LoginInterpret(conn, line);
			}
			else
			{
                Player player = conn.GetPlayer();
				string commandText = GetArgument(line, 1).Text;

                if (commandText == "" || line == "")
                    return;

                if (player.Menu == null)
                {
                    // Mosey through all the commands
                    foreach (KeyValuePair<string, Command> pair in Global.Commands.List)
                    {
                        // Match the command, allowing shorthand abbreviation (e.g. south = so = s)
                        // Note: Commands are parsed in the order they are added to the list in 
                        //   Commands.cs, to give commands priority, add them to the top of the 
                        //   list. (e.g. s will always match south, because it always comes before 
                        //   score)
                        if (pair.Key.StartsWith(commandText.ToLower()))
                        {
                            Command command = pair.Value;

                            player.SendMessage("Command debug: " + GetArgument(line, 1).Text + "|" + GetArgument(line, 2).Text + "|" + GetArgument(line, 3).Text + "|" + GetArgument(line, 4).Text + "\n\r");
                            if (command != null)
                            {
                                // Voodoo reflection magic to execute the method that the command 
                                //   wants to execute. Passes the Player itself, as well as the 
                                //   input string to that method.
                                // TODO: This could be improved for effeciency by retrieving the 
                                //   method once on load and storing it as a reference - possibly 
                                //   a delegate? I'm not too sure. -DWE 2009/08/04
                                MethodInfo method = typeof(Commands).GetMethod(command.Method);
                                method.Invoke(Global.Commands, new object[] { player, pair.Value, line });
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (player.Menu is DynamicMenu)
                    {
                        // Mosey through all the commands
                        DynamicMenu menu = (DynamicMenu)player.Menu;
                        foreach (KeyValuePair<string, Command> pair in menu.List)
                        {
                            // Match the command, allowing shorthand abbreviation (e.g. south = so = s)
                            // Note: Commands are parsed in the order they are added to the list in 
                            //   Commands.cs, to give commands priority, add them to the top of the 
                            //   list. (e.g. s will always match south, because it always comes before 
                            //   score)
                            if (pair.Key.StartsWith(commandText.ToLower()))
                            {
                                Command command = pair.Value;
                                if (command != null)
                                {
                                    // Voodoo reflection magic to execute the method that the command 
                                    //   wants to execute. Passes the Player itself, as well as the 
                                    //   input string to that method.
                                    // TODO: This could be improved for effeciency by retrieving the 
                                    //   method once on load and storing it as a reference - possibly 
                                    //   a delegate? I'm not too sure. -DWE 2009/08/04
                                    Type type = player.Menu.Caller.GetType();
                                    MethodInfo method = type.GetMethod(player.Menu.CallbackMethod);
                                    method.Invoke(player.Menu.Caller, new object[] { line });
                                    return;
                                }
                            }
                        }
                    }
                    else if (player.Menu is InputNumberMenu)
                    {
                        double number;
                        bool valid = double.TryParse(commandText, out number);
                        if (valid)
                        {
                            Type type = player.Menu.Caller.GetType();
                            MethodInfo method = type.GetMethod(player.Menu.CallbackMethod);
                            method.Invoke(player.Menu.Caller, new object[] { line });
                            return;
                        }
                        else
                        {
                            conn.GetPlayer().SendMessage("That is not a valid number.\n\r");
                            return;
                        }


                    }
                    else if (player.Menu is InputStringMenu)
                    {
                        Type type = player.Menu.Caller.GetType();
                        MethodInfo method = type.GetMethod(player.Menu.CallbackMethod);
                        method.Invoke(player.Menu.Caller, new object[] { line });
                        return;
                    }


                    if (player.Menu.DisplayMessage != "" && player.Menu.Caller is Player)
                    {
                        ((Player)player.Menu.Caller).SendMessage(player.Menu.DisplayMessage);
                    }
                    return;
                }

				conn.GetPlayer().SendMessage("Huh? \"" + commandText + "\" isn't a command!\n\r");

				// Nifty argument debugger, uncomment to see arguments as they are being parsed 
                //  - SUPER spammy, sends globally for all input
				/* 
				for (int x = 1; x <= 10; x++)
				{
					Argument arg = GetArgument(line, x);

					if(arg != null)
						Global.Server.SendToAll(arg.Index + ".  " + arg.Count + "*  " + arg.Text);
					else
						Global.Server.SendToAll("Huh?");
				}
				*/
			}
        }

	}
}
