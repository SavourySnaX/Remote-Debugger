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
    public partial class Disassembly : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        BindingList<DisassemblyData> disassemblyData;
        Regex disRegex;
        string viewName;

        public Disassembly(string name, string viewname)
        {
            viewName = viewname;
            InitializeComponent();
            disassemblyData = new BindingList<DisassemblyData>();
            disRegex = new Regex(@"([0-9a-fA-F]{4})\s*(.*)");
            for (int a=0;a<50;a++)
            {
                disassemblyData.Add(new DisassemblyData() { Address = "0000", Value = "" });
            }

            dataGridView1.DataSource = disassemblyData;

            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.Columns[1].ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
        }
        override protected string GetPersistString()
        {
            return viewName;
        }

        public void RequestUpdate(string address)
        {
            Program.t.SendCommand("d "+address+" 50", Callback);
        }

        void UIUpdate(string[] items)
        {
            bool updated = false;
            for (int a=0;a<items.Count() && a<disassemblyData.Count();a++)
            {
                Match m = disRegex.Match(items[a]);
                if (m.Success)
                {
                    disassemblyData[a].Address = m.Groups[1].Value;
                    disassemblyData[a].Value = m.Groups[2].Value;
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
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { UIUpdate(response); });
            }
            else
            {
                UIUpdate(response);
            }
        }
    }
    class DisassemblyData
    {
        public string Address { get; set; }
        public string Value { get; set; }
    }
}
