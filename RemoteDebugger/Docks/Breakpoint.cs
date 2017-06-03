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

using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RemoteDebugger
{
    public partial class Breakpoint : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        Regex breakRegex;
        string viewName;

        public Breakpoint(string name, string viewname)
        {
            viewName = viewname;
            InitializeComponent();

            breakRegex = new Regex(@"(Enabled|Disabled)\s*[0-9]+:\s*(.*)");
            dataGridView1.DataSource = Program.breakpointData;

            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].ReadOnly = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            TabText = name;
        }
        override protected string GetPersistString()
        {
            return viewName;
        }

        public void RequestUpdate()
        {
            Program.telnetConnection.SendCommand("get-breakpoints", Callback);
        }

        void UIUpdate(string[] items)
        {
            bool updated = false;
            items = items.Skip(1).ToArray();    // Skip first line
            for (int a=0;a<items.Count() && a<Program.breakpointData.Count();a++)
            {
                Match m = breakRegex.Match(items[a]);
                if (m.Success)
                {
                    Program.breakpointData[a].IsEnabled = m.Groups[1].Value=="Enabled";
                    Program.breakpointData[a].Condition = m.Groups[2].Value;
                    updated = true;
                }
            }
            if (updated)
            {
                dataGridView1.Invalidate(true);
            }
        }

        void Callback(string[] response)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke((MethodInvoker)delegate { UIUpdate(response); });
                }
                else
                {
                    UIUpdate(response);
                }
            }
            catch
            {
                
            }
        }
    }
    class BreakpointData
    {
        public bool IsEnabled { get; set; }
        public string Condition { get; set; }
    }
}
