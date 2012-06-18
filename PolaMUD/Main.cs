using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace PolaMUD
{
	public partial class Main : Form
	{
		Server server;

		public Main()
		{
			InitializeComponent();

			LogDelegateInstance = new LogDelegate(_Log);
			ErrorDelegateInstance = new ErrorDelegate(_Error);

			StartServer sserver = new StartServer(_StartServer);
			Thread serverConnection = new Thread(new ThreadStart(sserver));
			serverConnection.Start();

			//Thread.Sleep(5000);
		}

		public delegate void StartServer();
		public void _StartServer()
		{
			server = new Server(this);
			server.Start(4001);

		}

		public delegate void LogDelegate(string message);
		public LogDelegate LogDelegateInstance;
		public void Log(string message)
		{
			Invoke(LogDelegateInstance, new object[] { message });
		}
		public void _Log(string message)
		{
			txtOutput.Text += message;
		}

        public delegate void ErrorDelegate(string message);
        public ErrorDelegate ErrorDelegateInstance;
        public void Error(string message)
        {
			Invoke(ErrorDelegateInstance, new object[] { message });
        }
        public void _Error(string message)
        {
            txtErrorOutput.Text += message;
        }


		private void Main_FormClosed(object sender, FormClosedEventArgs e)
		{
		}

		private void Main_FormClosing(object sender, FormClosingEventArgs e)
		{
			server.Stop();
		}

	}
}
