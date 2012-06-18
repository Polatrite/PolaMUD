
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Xml.Serialization;
using System.Net.Sockets;
using System.Windows.Threading;
using System.Xml;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Windows;
using System.Windows.Documents;

namespace PolaMUD
{
    public class MUDServerConnection
    {
        //.NET's TCP Client connection, and a buffer to temporarily hold incoming/outgoing text
        private TcpClient connection = new TcpClient();
        private byte[] buffer = new byte[2500];
                       
        //disconnection callback and handler definition
        public event disconnectionEventHandler disconnected;
        public delegate void disconnectionEventHandler();
        
        //incoming message callback and handler definition
        public event serverMessageEventHandler serverMessage;
        public delegate void serverMessageEventHandler(List<MUDTextRun> runs);

        //incoming telnet control sequence callback and handler definition
        public event serverTelnetEventHandler telnetMessage;
        public delegate void serverTelnetEventHandler(string message);

        //a parser/decoder for ANSI control sequences, to give text color and potentially other styling
        ANSIColorParser ansiColorParser = new ANSIColorParser();

        //a parser for Telnet control sequences, which responds to any server messages as required by the Telnet protocol rules
        TelnetParser telnetParser;

        #region initialization, connection

        public MUDServerConnection(string address, int port)
        {
            //try to connect (may throw exceptions, to be handled by caller)
            this.connection.Connect(address, port);

            //if successful
            if (this.connection.Connected)
            {                
                //initialize the telnet parser
                this.telnetParser = new TelnetParser(this.connection);
                
                //start listening for new text
                connection.Client.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(this.handleServerMessage), null);
                
                //send a WILL NAWS (negotiate about window size)
                this.telnetParser.sendTelnetBytes((byte)Telnet.WILL, (byte)Telnet.NAWS);
            }
        }

        #endregion        

        #region incoming text handler

        //called when receiving any message
        void handleServerMessage(IAsyncResult result)
        {            
            //get length of data in buffer
            int receivedCount;
            try
            {
                receivedCount = connection.Client.EndReceive(result);
            }
            catch
            {
                //if there was any issue reading the server text, ignore the message (what else can we do?)
                return;
            }
            
            //0 bytes received means the server disconnected
            if (receivedCount == 0)
            {
                this.Disconnect();
                return;
            }

            //list of bytes which aren't telnet sequences
            //ultimately, this will be the original buffer minus any telnet messages from the server
            List<string> telnetMessages;
            List<byte> contentBytes = this.telnetParser.HandleAndRemoveTelnetBytes(this.buffer, receivedCount, out telnetMessages);

            //report any telnet sequences seen to the caller
            Application.Current.Dispatcher.BeginInvoke(new Action(delegate
            {
                foreach (string telnetMessage in telnetMessages)
                {
                    //fire the "received a server message" event
                    this.telnetMessage(telnetMessage);
                }
            }));

            //now we've filtered-out and responded accordingly to any telnet data.
            //next, convert the actual MUD content of the message from ASCII to Unicode
            string message = AsciiDecoder.AsciiToUnicode(contentBytes.ToArray(), contentBytes.Count);            
                        
            //run the following on the main thread so that calling code doesn't have to think about threading
            if (this.serverMessage != null)
            {
				Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    //pass the message to the mudTranslator to parse any ANSI control sequences (colors!)
                    List<MUDTextRun> runs = this.ansiColorParser.Translate(message);
                    
                    //fire the "received a server message" event with the runs to be displayed
                    this.serverMessage(runs);
                }));
            }

            //now that we're done with this message, listen for the next message
            connection.Client.BeginReceive(this.buffer, 0, this.buffer.Length, SocketFlags.None, new AsyncCallback(this.handleServerMessage), null);            
        }        

        #endregion
  
        #region outgoing text         

        public void SendText(string text)
        {
            //if not connected, do nothing
            if (!this.connection.Connected) return;

            //add carriage return and line feed
            text = text + "\r\n";

            //convert from Unicode to ASCII
            Encoder encoder = System.Text.Encoding.ASCII.GetEncoder();            
            char[] charArray = text.ToCharArray();
            int count = encoder.GetByteCount(charArray, 0, charArray.Length, true);
            byte[] outputBuffer = new byte[count];
            encoder.GetBytes(charArray, 0, charArray.Length, outputBuffer, 0, true);

            //send to server
            this.connection.Client.Send(outputBuffer);
        }

        #endregion                

        #region disconnect

        internal void Disconnect()
        {
            //if not connected, do nothing
            if (!this.connection.Connected) return;

            //close the connection
            this.connection.Close();

            //initialize a new object
            this.connection = new TcpClient();            
                
            //fire disconnection notification event on main UI thread
            if (this.disconnected != null)
            {
				Application.Current.Dispatcher.BeginInvoke(new Action(delegate
                {
                    this.disconnected.Invoke();
                }));
            }
        }

        #endregion        
    }    
}