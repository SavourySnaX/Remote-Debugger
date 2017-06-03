/*
 
The MIT License (MIT) 
 
Copyright (c) 2017 Savoury SnaX 
 
Permission is hereby granted, free of charge, to any person obtaining a copy 
of this software and associated documentation files (the "Software"), to deal 
in the Software without restriction, including without limitation the rights 
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
copies of the Software, and to permit persons to whom the Software is 
furnished to do so, subject to the following conditions: 
 
The above copyright notice and this permission notice shall be included in all 
copies or substantial portions of the Software. 
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE 
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE 
SOFTWARE. 

*/
using System;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RemoteDebugger
{
    static class Program
    {
        static readonly int BreakpointCount=10;

        public static TelNetSpec telnetConnection=new TelNetSpec();
        public static bool InStepMode = false;
        public static BindingList<BreakpointData> breakpointData;
        static Regex pcBreakRegex;

        static void InitialiseBreakpointData()
        {
            pcBreakRegex = new Regex(@"PC\s*=\s*([0-9]*[^a-fA-FHh]$|[0-9a-fA-F]*[Hh]$)");
            breakpointData = new BindingList<BreakpointData>();
            for (int a=0;a<BreakpointCount;a++)
            {
                breakpointData.Add(new BreakpointData() { IsEnabled=false, Condition = "" });
            }
        }

        public static int FindEmptyBreakpoint()
        {
            for (int a=0;a<BreakpointCount;a++)
            {
                if (breakpointData[a].Condition == "None")
                    return a;
            }
            return -1;
        }

        public static int FindBreakpoint(int address)
        {
            for (int a=0;a<BreakpointCount;a++)
            {
                if (breakpointData[a].IsEnabled)
                {
                    Match m = pcBreakRegex.Match(breakpointData[a].Condition);
                    if (m.Success)
                    {
                        string sAddress = m.Groups[1].Value;
                        if (sAddress.EndsWith("H") || sAddress.EndsWith("h"))
                        {
                            sAddress=sAddress.Remove(sAddress.Length-1);
                            int decAddress = Convert.ToInt32(sAddress, 16);
                            if (address == decAddress)
                                return a;
                        }
                        else
                        {
                            if (address == Convert.ToInt32(sAddress, 10))
                                return a;
                        }
                    }
                }
            }
            return -1;
        }

        public static void RemoveBreakpoint(int address)
        {
            int bp = FindBreakpoint(address);
            if (bp!=-1)
            {
                telnetConnection.SendCommand("set-breakpoint " + (bp + 1), null);
                telnetConnection.SendCommand("disable-breakpoint " + (bp + 1),null);
            }
        }

        public static void AddBreakpoint(int address)
        {
            int bp = FindEmptyBreakpoint();
            if (bp!=-1)
            {
                telnetConnection.SendCommand("set-breakpoint " + (bp + 1) + " PC=" + address, null);
            }
        }

        public static bool IsBreakpoint(int address)
        {
            return FindBreakpoint(address) != -1;
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            InitialiseBreakpointData();
            telnetConnection.UpdateSettings(Properties.Settings.Default.remoteAddress, Properties.Settings.Default.remotePort);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
