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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace RemoteDebugger
{
    public partial class MainForm : Form
    {
        DockPanel dockPanel;
        public static ButtonBar myButtonBar;
        public static List<LogView> myLogs;
        Registers myNewRegisters;
        Disassembly myDisassembly;
        SpectrumScreen myScreen;
        Breakpoint myBreakpoints;
        public static List<SpriteView> mySpriteViews;

        bool refreshScreen;

        public MainForm()
        {
            myLogs = new List<LogView>();
            mySpriteViews = new List<SpriteView>();
            InitializeComponent();

            this.IsMdiContainer = true;
            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Controls.Add(this.dockPanel);

            UpdateStatus();

            if (File.Exists("layout.xml"))
            {
                dockPanel.LoadFromXml("layout.xml",DelegateHandler);
            }
            else
            {
                myButtonBar = new ButtonBar("Control");
                myLogs.Add(new LogView("Log", "Log"));
                myNewRegisters = new Registers("Registers", "Registers");
                myDisassembly = new Disassembly("Disassembly", "Disassembly");
                myScreen = new SpectrumScreen("Screen", "Screen");
                myBreakpoints = new Breakpoint("Breakpoints", "Breakpoints");
                mySpriteViews.Add(new SpriteView("Sprite Patterns", "SpritePatterns"));

                myButtonBar.Show(this.dockPanel, DockState.DockTop);
                myNewRegisters.Show(this.dockPanel, DockState.DockLeft);
                myDisassembly.Show(this.dockPanel, DockState.DockRight);
                myLogs[0].Show(this.dockPanel, DockState.DockBottom);
                myBreakpoints.Show(this.dockPanel, DockState.DockLeft);
                myScreen.Show(this.dockPanel, DockState.DockRight);
                mySpriteViews[0].Show(this.dockPanel, DockState.Float);
            }

            Program.telnetConnection.SendCommand("help", null);
            refreshScreen = false;

            Invalidate();
        }

        public IDockContent DelegateHandler(string name)
        {
            string[] split = name.Split(':');
            switch (split[0])
            {
                case "Control":
                    if (myButtonBar==null)
                    {
                        myButtonBar = new ButtonBar("Control");
                    }
                    return myButtonBar;
                case "Log":
                    {
                        foreach (LogView lv in myLogs)
                        {
                            if (lv.viewName == name)
                                return lv;
                        }
                        LogView t = new LogView("Log", "Log");
                        myLogs.Add(t);
                        return t;
                    }
                case "Registers":
                    if (myNewRegisters==null)
                    {
                        myNewRegisters = new Registers("Registers", "Registers");
                    }
                    return myNewRegisters;
                case "Disassembly":
                    if (myDisassembly == null)
                    {
                        myDisassembly = new Disassembly("Disassembly", "Disassembly");
                    }
                    return myDisassembly;
                case "Screen":
                    if (myScreen == null)
                    {
                        myScreen = new SpectrumScreen("Screen", "Screen");
                    }
                    return myScreen;
                case "Breakpoints":
                    if (myBreakpoints == null)
                    {
                        myBreakpoints = new Breakpoint("Breakpoints", "Breakpoints");
                    }
                    return myBreakpoints;
                case "SpritePatterns":
                    {
                        foreach (SpriteView sv in mySpriteViews)
                        {
                            if (sv.viewName == name)
                                return sv;
                        }
                        SpriteView t = new SpriteView("Sprite Patterns", "SpritePatterns");
                        mySpriteViews.Add(t);
                        return t;
                    }
                default:
                    break;
            }
            return null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            dockPanel.SaveAsXml("layout.xml");
            Program.telnetConnection.CloseConnection();
        }

        private void newRegisterViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (myNewRegisters == null)
            {
                myNewRegisters = new Registers("Registers", "Registers");
                myNewRegisters.Show(dockPanel, DockState.Float);
            }
        }

        private void UpdateStatus()
        {
            if (Program.telnetConnection.connected)
            {
                toolStripStatusLabel1.Text = "Connected";
            }
            else
            {
                toolStripStatusLabel1.Text = "Please launch ZEsarUX with --enable-remoteprotocol  (also confirm remote Address and remote Port in settings)";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            UpdateStatus();
            if (!Program.telnetConnection.connected)
                return;

            if (Program.telnetConnection.IsQueueEmpty())
            {
                if (myNewRegisters != null)
                {
                    myNewRegisters.RequestUpdate();
                }
                if (myBreakpoints != null)
                {
                    myBreakpoints.RequestUpdate();
                }
                if (myDisassembly != null)
                {
                    string address = "0000H";
                    if (myNewRegisters != null)
                    {
                        address = myNewRegisters.GetRegisterValue("PC") + "H";
                    }
                    myDisassembly.RequestUpdate(address);
                }
                foreach (SpriteView sv in mySpriteViews)
                {
                    sv.RequestUpdate();
                }
                if (myScreen!=null)
                {
                    if (refreshScreen)
                    {
                        refreshScreen = false;
                        myScreen.RequestUpdate();
                    }
                }
            }
        }

        private void newLogViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (myLogs.Count==0)
            {
                LogView t = new LogView("Log", "Log");
                myLogs.Add(t);
                t.Show(dockPanel, DockState.Float);
            }
        }

        private void newDisassemblyViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (myDisassembly == null)
            {
                myDisassembly = new Disassembly("Disassembly", "Disassembly");
                myDisassembly.Show(dockPanel, DockState.Float);
            }

        }

        private void newScreenViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (myScreen == null)
            {
                myScreen = new SpectrumScreen("Screen", "Screen");
                myScreen.Show(dockPanel, DockState.Float);
            }

        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            refreshScreen = true;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (AboutBox box = new AboutBox())
            {
                box.ShowDialog(this);
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Settings settings = new Settings())
            {
                settings.ShowDialog(this);
            }
        }

        private void newBreakpointsViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (myBreakpoints == null)
            {
                myBreakpoints = new Breakpoint("Breakpoints", "Breakpoints");
                myBreakpoints.Show(dockPanel, DockState.Float);
            }
        }

        private void newSpriteViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpriteView t = new SpriteView("Sprite Patterns", "SpritePatterns");
            t.Show(dockPanel, DockState.Float);
            mySpriteViews.Add(t);
        }
    }
}
