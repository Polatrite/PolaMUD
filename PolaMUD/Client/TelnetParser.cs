using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolaMUD
{
    enum Telnet : byte
    {
        //escape
        InterpretAsCommand = 255,

        //commands
        SubnegotiationEnd = 240,
        NoOperation = 241,
        DataMark = 242,
        Break = 243,
        InterruptProcess = 244,
        AbortOutput = 245,
        AreYouThere = 246,
        EraseCharacter = 247,
        EraseLine = 248,
        GoAhead = 249,
        SubnegotiationBegin = 250,

        //negotiation
        WILL = 251,
        WONT = 252,
        DO = 253,
        DONT = 254,

        //options (common)
        SuppressGoAhead = 3,
        Status = 5,
        Echo = 1,
        TimingMark = 6,
        TerminalType = 24,
        TerminalSpeed = 32,
        RemoteFlowControl = 33,
        LineMode = 34,
        EnvironmentVariables = 36,
        NAWS = 31,

        //options (MUD-specific)
        MSDP = 69,
        MXP = 91,
        MCCP1 = 85,
        MCCP2 = 86,
        MSP = 90
    };
    
    class TelnetParser
    {
        private System.Net.Sockets.TcpClient tcpClient;

        public TelnetParser(System.Net.Sockets.TcpClient tcpClient)
        {            
            this.tcpClient = tcpClient;
        }        

        public List<byte> HandleAndRemoveTelnetBytes(byte[] buffer, int receivedCount, out List<string> telnetMessages)
        {            
            //list to hold a report of any telnet control sequences received or sent
            telnetMessages = new List<string>();
            
            //list to hold any bytes which aren't telnet bytes (which will be most of the bytes)
            List<byte> contentBytes = new List<byte>();

            //we'll scan for telnet control sequences.  anything NOT a telnet control sequence will be added to the contentBytes list for later processing.
            int currentIndex = 0;
            while (currentIndex < receivedCount)
            {
                //search for an IAC, which may signal the beginning of a telnet message
                while (currentIndex < receivedCount && buffer[currentIndex] != (byte)Telnet.InterpretAsCommand)
                {
                    contentBytes.Add(buffer[currentIndex]);
                    currentIndex++;
                }

                //if at the end of the data, stop.  otherwise we've encountered an IAC and there should be at least one more byte here
                if (++currentIndex == receivedCount) break;

                //read the next byte
                byte secondByte = buffer[currentIndex];

                //if another IAC, then this was just sequence IAC IAC, which is the escape sequence to represent byte value 255 (=IAC) in the content stream
                if (secondByte == (byte)Telnet.InterpretAsCommand)
                {
                    //write byte value 255 to the content stream and move on
                    contentBytes.Add(secondByte);
                }

                //otherwise we have a "real" telnet sequence, where the second byte is a command or negotiation
                else
                {
                    //start building a string representation of this message, to be reported to the caller
                    //caller might want to show this info to the user always, or optionally for debugging purposes
                    StringBuilder stringVersionOfMessage = new StringBuilder();

                    //also build a string version of the response (if any)
                    StringBuilder stringVersionOfResponse = new StringBuilder();

                    //DO
                    if (secondByte == (byte)Telnet.DO)
                    {
                        stringVersionOfMessage.Append("DO ");

                        //what are we being told to do?
                        currentIndex++;
                        if (currentIndex == receivedCount) break;
                        byte thirdByte = buffer[currentIndex];

                        stringVersionOfMessage.Append(interpretByteAsTelnet(thirdByte));

                        //if NAWS (negotiate about window size)
                        if (thirdByte == (byte)Telnet.NAWS)
                        {
                            //on connection, we offered to negotiate about window size.  so this is a "go ahead and negotiate" response.
                            //so then, send information about client window size per the NAWS protocol
                            //we're lieing to server by telling it a ridiculously large size, so that it won't do line breaking or paging for us (annoying!)
                            stringVersionOfResponse.Append(this.sendTelnetBytes(
                                (byte)Telnet.SubnegotiationBegin, (byte)31,
                                254, 254, 254, 254,
                                (byte)Telnet.InterpretAsCommand, (byte)Telnet.SubnegotiationEnd));
                        }

                        //everything else the server might ask us to do is unsupported by us
                        else
                        {
                            stringVersionOfMessage.Append(interpretByteAsTelnet(thirdByte));

                            //sorry, i won't do whatever "that thing you said to do" was
                            stringVersionOfResponse.Append(this.sendTelnetBytes((byte)Telnet.InterpretAsCommand, (byte)Telnet.WONT, thirdByte));
                        }
                    }

                    //DONT
                    else if (secondByte == (byte)Telnet.DONT)
                    {
                        stringVersionOfMessage.Append("DONT ");

                        currentIndex++;
                        if (currentIndex == receivedCount) break;
                        byte thirdByte = buffer[currentIndex];

                        stringVersionOfMessage.Append(interpretByteAsTelnet(thirdByte));

                        //whatever you want me to stop doing, that's no problem because i wasn't going to do it anyway                    
                        stringVersionOfResponse.Append(this.sendTelnetBytes((byte)Telnet.WONT, thirdByte));
                    }

                    //WILL
                    else if (secondByte == (byte)Telnet.WILL)
                    {
                        stringVersionOfMessage.Append("WILL ");

                        //find out what the server is willing to do
                        currentIndex++;
                        if (currentIndex == receivedCount) break;
                        byte thirdByte = buffer[currentIndex];
                        stringVersionOfMessage.Append(interpretByteAsTelnet(thirdByte));

                        //anything the server offers to do for us, we'll tell it not to because we don't know what it is
                        stringVersionOfResponse.Append((this.sendTelnetBytes((byte)Telnet.DONT, thirdByte)));
                    }

                    //WONT
                    else if (secondByte == (byte)Telnet.WONT)
                    {
                        stringVersionOfMessage.Append("WONT ");

                        //find out what the server is NOT willing to do
                        currentIndex++;
                        if (currentIndex == receivedCount) break;
                        byte thirdByte = buffer[currentIndex];

                        stringVersionOfMessage.Append(interpretByteAsTelnet(thirdByte));

                        //because we haven't asked the server to DO anything, should not expect to receive any WONT
                        //however if we do receive a WONT, respond with a DONT to confirm that the server can go ahead and NOT do that thing it doesn't want to do
                        stringVersionOfResponse.Append(this.sendTelnetBytes((byte)Telnet.DONT, thirdByte));
                    }

                    //subnegotiations
                    else if (secondByte == (byte)Telnet.SubnegotiationBegin)
                    {
                        stringVersionOfMessage.Append("SB ");
                        List<byte> subnegotiationBytes = new List<byte>();

                        //read until an IAC followed by an SE
                        while (currentIndex < receivedCount - 1 &&
                            !(buffer[currentIndex] == (byte)Telnet.InterpretAsCommand && buffer[currentIndex] == (byte)Telnet.SubnegotiationEnd))
                        {
                            subnegotiationBytes.Add(buffer[currentIndex]);
                            currentIndex++;
                        }

                        byte[] subnegotiationBytesArray = subnegotiationBytes.ToArray();

                        //append the content of the subnegotiation to the incoming message report string
                        stringVersionOfMessage.Append(AsciiDecoder.AsciiToUnicode(subnegotiationBytesArray, subnegotiationBytes.Count));

                        //append the subnegotiation end
                        stringVersionOfMessage.Append(" SE");
                    }

                    //any other telnet message
                    else
                    {
                        //try to convert it to a known message via the enum defined above
                        stringVersionOfMessage.Append(interpretByteAsTelnet(secondByte));
                    }

                    //report the control sequence we found, if any
                    string messageToReport = stringVersionOfMessage.ToString();
                    if (!string.IsNullOrEmpty(messageToReport))
                    {
                        telnetMessages.Add("RECV: " + messageToReport.ToString());                                               
                    }

                    //report the response message sent, if any
                    string responseToReport = stringVersionOfResponse.ToString();
                    if (!string.IsNullOrEmpty(responseToReport))
                    {
                        telnetMessages.Add("SEND: " + stringVersionOfResponse.ToString());                                               
                    }
                }
                //move up to the next byte in the data
                currentIndex++;
            }

            return contentBytes;
        }

        #region "friendly" text for telnet sequences

        private string interpretByteAsTelnet(byte thisByte)
        {
            //try to convert the byte value to a string representation based on the Telnet enumeration
            string friendlyName = Enum.GetName(typeof(Telnet), thisByte);

            //if failed, just show the byte's numerical value in brackets, like [254]
            if (string.IsNullOrEmpty(friendlyName))
            {
                friendlyName = '[' + thisByte.ToString() + ']';
            }

            return friendlyName;
        }

        #endregion
        
        public string sendTelnetBytes(params byte[] bytes)
        {
            //if not connected, do nothing
            if (!this.tcpClient.Connected) return "";

            //send IAC
            this.tcpClient.Client.Send(new byte[] { (byte)Telnet.InterpretAsCommand });

            //send the specified bytes
            this.tcpClient.Client.Send(bytes);

            //start building a string to report to the caller
            StringBuilder reportMessage = new StringBuilder();

            for (int i = 0; i < bytes.Length; i++)
            {
                //convert the byte value to something readable, and append it to the report string
                reportMessage.Append(this.interpretByteAsTelnet(bytes[i]) + " ");
            }

            return reportMessage.ToString();
        }
    }
}
