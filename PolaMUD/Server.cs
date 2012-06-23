using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;

namespace PolaMUD
{
	/// <summary>
	/// This class is responsible for booting up the connection manager, and subsequently the world.
	/// Once the Server is running, players will be able to connect on the specified port. Each
	/// player will execute on their own thread, which will then be managed under thread-safe
	/// standards - the rest of the server operations will run as a single thread game loop.
	/// </summary>
	public class Server
	{
		int PortNumber = 0;
		const int BacklogSize = 20;
		bool running = false;

		Socket server;

		public delegate void Log();

		public List<TelnetConnection> Connections = new List<TelnetConnection>();

		public Main Application;

		public Server(Main application)
		{
			Application = application;
			Global.Server = this;

			Global.Log("Booting up PolaMUD!\n\r");

            Global.Log("Loading terrain types... ");
            Global.TerrainTable.Add("Grass", new Terrain() { Name = "Grass" });
            Global.TerrainTable.Add("Forest", new Terrain() { Name = "Forest" });
            Global.TerrainTable.Add("Desert", new Terrain() { Name = "Desert" });
            RoomStyles.Generate();
            Global.Log("done!\n");

            Global.Log("Loading skills... ");
            List<Type> types = Assembly.GetCallingAssembly().GetTypes().Where(type => type.IsSubclassOf(typeof(Skill))).ToList();
            foreach (Type type in types)
            {
                if (type == typeof(Skill))
                    continue;
                Skill skill = (Skill)Activator.CreateInstance(type);
                Global.SkillTable.Add(skill.Name, skill);
                Global.Log(skill.Name + ", ");
            }
            Global.Log("done!\n");
            
            Global.Log("Loading areas...\n");
            Area area = new Area();
            area.Load("00_TestData\\limbo.xml");
            new Areas.TestArea();
			Global.Log("done!\n");
            
            Global.Log("Initializing game loop... ");
			Global.GameLoop = new GameLoop();
            Global.Log("done!\n");
		}

		public void Start(int portNumber)
		{
			PortNumber = portNumber;
			server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			server.Bind(new IPEndPoint(IPAddress.Any, PortNumber));
			server.Listen(BacklogSize);
			running = true;
			Global.Log("PolaMUD is up and running on port " + portNumber);
			while (running == true)
			{
				try
				{
					Socket conn = server.Accept();
					new TelnetConnection(conn);
				}
				catch { }
			}
		}

		public void Stop()
		{
			running = false;

			server.Close();

			foreach (TelnetConnection connection in TelnetConnection.Connections)
			{
				connection.Disconnect();
			}
		}

	}

}
