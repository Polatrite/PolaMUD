using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MUDClientEssentials
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //this "fancy" user interface actually communicates with servers through this object
        private MUDServerConnection serverConnection;

        #region startup, establishing a connection

        //obligatory constructor
        public MainWindow()
        {
            InitializeComponent();
        }

        //when the main window loads, prompt the user for connection info
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bool successfulConnection;
            do
            {
                //prompts
                string address = TextInputDialog.PromptUser("Server Address", "Input a MUD server address:");
                string portString = TextInputDialog.PromptUser("Server Port", "Enter the server's port number:");

                //convert port to int, default to port=23 in the event of any parsing issue
                int port;
                if (!int.TryParse(portString, out port))
                    port = 23;

                //attempt a connection
                successfulConnection = true;
                try
                {
                    //tell the user what we're doing first
                    this.appendText("Attempting a connection to " + address + ", port " + port.ToString() + "...");
                    
                    //then give it a shot
                    this.serverConnection = new MUDServerConnection(address, port);
                }

                //if there's any problem, start over with prompts again
                catch (Exception)
                {
                    this.appendText("Connection failed.  Please verify your internet connectivity and server information, then try again.");
                    successfulConnection = false;
                }
            }
            while (!successfulConnection);

            //now that we've connected, start listening for messages and disconnections 
            this.serverConnection.serverMessage += new MUDServerConnection.serverMessageEventHandler(serverConnection_serverMessage);
            this.serverConnection.disconnected += new MUDServerConnection.disconnectionEventHandler(serverConnection_disconnected);
            this.serverConnection.telnetMessage += new MUDServerConnection.serverTelnetEventHandler(serverConnection_telnetMessage);
        }

        #endregion

        #region receiving server text

        //when a telnet message arrives, display it in the special telnet output box
        void serverConnection_telnetMessage(string message)
        {
            //add the new message
            this.telnetOutputBox.AppendText(message + System.Environment.NewLine);

            //scroll down to ensure it's visible
            this.telnetOutputBox.ScrollToEnd();
        }

        //when a content message arrives, display it in the main output box
        void serverConnection_serverMessage(List<MUDTextRun> genericRuns)
        {
            //convert the generic "MUD Text Runs" to "WPF Runs" so that they can be displayed in the UI
            List<Run> wpfRuns = new List<Run>();
            {
                foreach(MUDTextRun genericRun in genericRuns)
                {
                    Run newRun = new Run(genericRun.Content);
                    newRun.Foreground = new SolidColorBrush(this.getColor(genericRun.ForegroundColor));
                    newRun.Background = new SolidColorBrush(this.getColor(genericRun.BackgroundColor));

                    wpfRuns.Add(newRun);
                }
            }
            
            //display them
            this.appendRuns(wpfRuns.ToArray());
        }

        //associates an actual color with each of the 15 color numbers used by servers
        //any modern client should make these user-customizable!
        //the color values used in this color theme come from the ANSI control sequence page on wikipedia. they're garish.
        private Color getColor(int colorNumber)
        {
            switch (colorNumber)
            {
                //colors 0 through 7 are basic colors
                case 0:
                    return Color.FromRgb(0, 0, 0);
                case 1:
                    return Color.FromRgb(128, 0, 0);
                case 2:
                    return Color.FromRgb(0, 128, 0);
                case 3:
                    return Color.FromRgb(128, 128, 0);
                case 4:
                    return Color.FromRgb(0, 0, 128);
                case 5:
                    return Color.FromRgb(128, 0, 128);
                case 6:
                    return Color.FromRgb(0, 128, 128);
                case 7:
                    return Color.FromRgb(192, 192, 192);

                //colors 8 through 15 are "intense" versions of the basic colors above
                //in this example, 7 is medium gray, and its corresponding "intense" version at 15 is bright white                
                case 8:
                    return Color.FromRgb(128, 128, 128);
                case 9:
                    return Color.FromRgb(255, 0, 0);
                case 10:
                    return Color.FromRgb(0, 255, 0);
                case 11:
                    return Color.FromRgb(255, 255, 0);
                case 12:
                    return Color.FromRgb(0, 0, 255);
                case 13:
                    return Color.FromRgb(255, 0, 255);
                case 14:
                    return Color.FromRgb(0, 255, 255);
                default: //case 15
                    return Color.FromRgb(255, 255, 255);                
            }
        }
                
        //displays plain text in the main output window (by turning it into a WPF run first)
        private void appendText(string message)
        {
            //add a line to the output box
            Run run = new Run(message);
            run.Foreground = new SolidColorBrush(Colors.White);
            run.Background = new SolidColorBrush(Colors.CornflowerBlue);
            this.appendRuns(run);
        }

        //displays rich text in the main output window
        private void appendRuns(params Run [] runs)
        {
            //create a new "paragraph" element, vertically separating this bunch of runs from the previous bunch
            Paragraph newParagraph = new Paragraph();
            
            //fill it with the provided runs
            newParagraph.Inlines.AddRange(runs);

            //add it to the document in the output box
            this.outputBox.Document.Blocks.Add(newParagraph);

            //automatically scroll to the bottom
            ScrollViewer descendantScrollViewer = findScrollViewerDescendant(this.outputBox);

            if (descendantScrollViewer != null)
                descendantScrollViewer.ScrollToEnd();
        }

        //helper for above, because FlowDocumentScrollViewer doesn't have a convenient ScrollToEnd() method
        //if this looks like black magic, that's because it is (this is not a fun area of WPF)
        private static ScrollViewer findScrollViewerDescendant(DependencyObject control)
        {
            if (control is ScrollViewer) return (control as ScrollViewer);

            int childCount = VisualTreeHelper.GetChildrenCount(control);
            for (int i = 0; i < childCount; i++)
            {
                ScrollViewer result = findScrollViewerDescendant(VisualTreeHelper.GetChild(control, i));
                if (result != null) return result;
            }

            return null;
        }

        #endregion

        #region sending text to the server

        //when user presses ENTER in the input box, send that text to the server
        private void inputBox_KeyDown(object sender, KeyEventArgs e)
        {
            //if the keystroke was ENTER
            if (e.Key == Key.Return)
            {
                //send text
                try
                {
                    this.serverConnection.SendText(this.inputBox.Text);
                }
                catch
                {
                    this.appendText("Failed to send the below command.  Your internet service may have been interrupted, or the server might have shut down.\r\n" + this.inputBox.Text);
                }

                //clear text for next command entry
                this.inputBox.Clear();
            }
        }

        #endregion

        #region server disconnection

        //when the server disconnects, notify the user via the output box
        void serverConnection_disconnected()
        {
            this.appendText("Disconnected.");
        }

        #endregion

        #region exiting the application

        //when the main window closes, make sure the connection is closed
        private void Window_Closed(object sender, EventArgs e)
        {            
            //make sure connection is closed
            this.serverConnection.Disconnect();
            
            //close the app
            Application.Current.Shutdown();
        }

        #endregion
    }
}
