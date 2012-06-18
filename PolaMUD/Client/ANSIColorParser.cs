using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace PolaMUD
{
    public struct MUDTextRun
    {
        public string Content;
        public int ForegroundColor;
        public int BackgroundColor;
    }
    
    //scans incoming text for ANSI control sequences to provide a stream of styled text for end-user viewing
    class ANSIColorParser
    {                        
        //regular expression to fit ANSI control sequences
        public readonly string ansiControlRegEx = (char)27 + @"\[" + "[^@-~]*" + "[@-~]";

        //current settings (with defaults)
        private int foregroundColor = 7;
        private int backgroundColor = 0;
        private bool brightColors = true;

        //scans incoming text for ANSI control sequences, parses them, and returns a list of styled text runs
        public List<MUDTextRun> Translate(string text)
        {                                    
            //start with an empty list of runs
            List<MUDTextRun> returnRuns = new List<MUDTextRun>();
            
            //argument validation
            if(string.IsNullOrEmpty(text))
                return returnRuns;

            //in simplest case, the generated run will represent the entire string
            int runStartIndex = 0;
            int runEndIndex = text.Length - 1;

            //find all the control sequences
            MatchCollection matches = Regex.Matches(text, this.ansiControlRegEx);

            //for each control sequence
            foreach (Match match in matches)
            {
                string matchValue = match.Value;

                //identify the operation by grabbing the last character
                char operationToken = matchValue[matchValue.Length - 1];

                //identify the arguments by splitting on semicolon, then converting to integers
                string argsString = matchValue.Substring(2, matchValue.Length - 3);
                string[] argsArray = argsString.Split(';');
                List<int> arguments = new List<int>();

                foreach (string argument in argsArray)
                {
                    try
                    {
                        arguments.Add(int.Parse(argument));
                    }
                    
                    //if we can't convert to an integer, it's a badly formed parameter and we'll ignore it
                    catch (FormatException) { }
                    catch (OverflowException) { }
                }

                //now apply whatever changes are necessary according to the operation and parameters
                
                //'m' is "select graphics rendition", and indicates a style change
                //we need a new run for each continuous string of same-style characters
                //so then, ending the current run and starting a new one
                if (operationToken.Equals('m'))
                {
                    //figure out start index, end index, and text of the completed run (remember runStartIndex initialized to zero)
                    //last run ends just before we encountered the ANSI control sequence
                    runEndIndex = match.Index - 1;
                    string runText = text.Substring(runStartIndex, runEndIndex - runStartIndex + 1);

                    //build a run out of the current color settings and text up until this point
                    MUDTextRun newRun;
                    newRun.Content = runText;
                    newRun.ForegroundColor = this.foregroundColor;
                    newRun.BackgroundColor = this.backgroundColor;
                    
                    returnRuns.Add(newRun);

                    //note the new start index of the next run, which will start after the end of the control sequence
                    runStartIndex = match.Index + match.Length;

                    //assume the next run will continue until the end of the string, until proven otherwise
                    runEndIndex = text.Length - 1;

                    //parameters will determine the style of the next run, and there may be several parameters
                    foreach (int param in arguments)
                    {
                        //reset to defaults
                        if (param == 0)
                        {
                            this.brightColors = true;
                            this.foregroundColor = 15;
                            this.backgroundColor = 0;
                        }

                        //bright colors on
                        if (param == 1)
                        {
                            this.brightColors = true;
                        }

                        //bright colors off
                        else if (param == 22)
                            this.brightColors = false;

                        //set foreground color
                        else if (param >= 30 && param <= 37)
                        {
                            this.foregroundColor = param - 30;
                            if (this.brightColors) this.foregroundColor += 8;
                        }

                        //set background color
                        else if (param >= 40 && param <= 47)
                        {
                            this.backgroundColor = param - 40;
                            if (this.brightColors) this.backgroundColor += 8;
                        }

                        //default background color
                        else if (param == 49)
                        {
                            this.backgroundColor = 0;
                            if (this.brightColors) this.backgroundColor += 8;
                        }

                        //default foreground color
                        else if (param == 39)
                        {
                            this.foregroundColor = 7;
                            if (this.brightColors) this.foregroundColor += 8;
                        }
                    }
                }

                //if the ansi control sequence has an unsupported operation code, show it in the UI highlighted in a bright color
                else
                {
                    MUDTextRun controlSequenceRun;
                    controlSequenceRun.Content = matchValue;
                    controlSequenceRun.BackgroundColor = 14;
                    controlSequenceRun.ForegroundColor = 0;
                    returnRuns.Add(controlSequenceRun);
                }
            }

            //now that we're done with the last control sequence, build a run from any remaining ("trailing") text
            
            //if there were no control sequences at all, the "trailing text" would be the entire string
            int trailingTextStartIndex = 0;
                        
            //if there were control sequences, the trailing text would start after the last control sequence ended
            if (matches.Count > 0)
            {
                //get the last control sequence
                Match lastMatch = matches[matches.Count - 1];
                
                //figure out where it ends
                trailingTextStartIndex = lastMatch.Index + lastMatch.Length;                
            }

            //if there's any trailing text, build a run from that text using current style
            if (trailingTextStartIndex < text.Length - 1)
            {
                MUDTextRun trailingRun;
                trailingRun.Content = text.Substring(trailingTextStartIndex);
                trailingRun.ForegroundColor = this.foregroundColor;
                trailingRun.BackgroundColor = this.backgroundColor;
                returnRuns.Add(trailingRun);
            }

            return returnRuns;
        }
    }
}

