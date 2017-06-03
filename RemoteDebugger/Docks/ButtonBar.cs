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
using System.IO;
using System.Windows.Forms;

namespace RemoteDebugger
{
    public partial class ButtonBar : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        string viewName;

        public ButtonBar(string viewname)
        {
            viewName = viewname;
            InitializeComponent();
        }

        override protected string GetPersistString()
        {
            return viewName;
        }

        void commandResponse(string[] s)
        {

        }

        bool LoadThingToAddress(int address,string filter)
        {
            Stream myStream = null;
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = "";
            openFileDialog1.Filter = filter;
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((myStream = openFileDialog1.OpenFile()) != null)
                    {
                        using (myStream)
                        {
                            // Insert code to read the stream here.
                            string toSend = "wmm " + address.ToString();
                            for (int a = 0; a < myStream.Length; a++)
                            {
                                toSend += " " + (int)myStream.ReadByte();
                            }
                            Program.telnetConnection.SendCommand(toSend, commandResponse);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. Original error: " + ex.Message);
                }
            }
            return false;
        }

        private void clickLoadScreen(object sender, EventArgs e)
        {
            LoadThingToAddress(16384, "scr files (*.scr)|*.scr|All files (*.*)|*.*");
        }

        private void clickStep(object sender, EventArgs e)
        {
            Program.telnetConnection.SendCommand("cpu-step", commandResponse);
        }

        private void clickPause(object sender, EventArgs e)
        {
            Program.telnetConnection.SendCommand("enter-cpu-step", commandResponse);
            Program.InStepMode = true;
        }

        private void ButtonBar_Load(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
        }

        private void clickStepOver(object sender, EventArgs e)
        {
            Program.telnetConnection.SendCommand("cpu-step-over", commandResponse);
        }

        private void clickRun(object sender, EventArgs e)
        {
            Program.telnetConnection.SendCommand("exit-cpu-step", commandResponse);
            Program.InStepMode = false;
        }

        private void clickLoadCode(object sender, EventArgs e)
        {
            LoadThingToAddress(32768, "bin files (*.bin)|*.bin|All files (*.*)|*.*");
            Program.InStepMode = true;
            Program.telnetConnection.SendCommand("enter-cpu-step", commandResponse);
            Program.telnetConnection.SendCommand("set-register pc=32768", commandResponse);
        }
    }
}
