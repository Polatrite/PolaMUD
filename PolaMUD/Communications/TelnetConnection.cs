using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PolaMUD
{
	public class TelnetConnection
	{
		static object BigLock = new object();
		Socket socket;
		public StreamReader Reader;
		public StreamWriter Writer;
		public static ArrayList Connections = new ArrayList();
		bool running = false;

		Player player;

		public TelnetConnection(Socket socket)
		{
			this.socket = socket;
			Reader = new StreamReader(new NetworkStream(socket, false));
			Writer = new StreamWriter(new NetworkStream(socket, true));
			running = true;
            Writer.AutoFlush = true;
			new Thread(ClientLoop).Start();
		}

		void ClientLoop()
		{
			try
			{
				lock (BigLock)
				{
					OnConnect();
				}
				while (running == true)
				{
					Console.WriteLine("connection running");
					lock (BigLock)
					{
						foreach (TelnetConnection conn in Connections)
						{
							conn.Writer.Flush();
						}
					}
					string line = Reader.ReadLine();
					if (line == null)
					{
						break;
					}
					lock (BigLock)
					{
						ProcessLine(line);
					}
				}
			}
			catch
			{
			}
			finally
			{
				lock (BigLock)
				{
					socket.Close();
					OnDisconnect();
				}
			}
		}

		void OnConnect()
		{
			Writer.Write("Welcome to PolaMUD v1.0!\n\nWhat is your name? ");
			Connections.Add(this);
		}

		void OnDisconnect()
		{
			Connections.Remove(this);
		}

		public void Disconnect()
		{
			running = false;
		}

		public void ProcessLine(string line)
		{
            if (player != null)
            {
                player.IncomingBuffer.Enqueue(line);
            }
            else
            {
                Parser.Interpret(this, line);
            }
		}

		public Player GetPlayer()
		{
			return player;
		}

		public void SetPlayer(Player _player)
		{
			player = _player;
		}
	}
}
