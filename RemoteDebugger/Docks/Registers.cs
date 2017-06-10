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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RemoteDebugger
{
    public partial class Registers : WeifenLuo.WinFormsUI.Docking.DockContent
    {
        BindingList<RegisterItem> registerData;
        Dictionary<string, int> nameMap;
        List<Regex> regexList;

        string viewName;

        void InitialiseRegister(string name, string value,string regex)
        {
            registerData.Add(new RegisterItem() { Register = name, Value = value });
            regexList.Add(new Regex(regex));
            nameMap.Add(name, registerData.Count - 1);
        }
        public Registers(string name, string viewname)
        {
            viewName = viewname;
            InitializeComponent();
            dataGridView1.CausesValidation = false;
            registerData = new BindingList<RegisterItem>();
            regexList = new List<Regex>();
            nameMap = new Dictionary<string, int>();

            InitialiseRegister("A", "00", @"A\s*=\s*([a-fA-F0-9]{2})\s+");
            InitialiseRegister("F", "00", @"F\s*=\s*([SZ5P3HNC ]+)\s+");
            InitialiseRegister("BC", "0000", @"BC\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("DE", "0000", @"DE\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("HL", "0000", @"HL\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("IX", "0000", @"IX\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("IY", "0000", @"IY\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("SP", "0000", @"SP\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("PC", "0000", @"PC\s*=\s*([a-fA-F0-9]{4})\s+");

            InitialiseRegister("I", "00", @"I\s*=\s*([a-fA-F0-9]{2})\s+");
            InitialiseRegister("R", "00", @"R\s*=\s*([a-fA-F0-9]{2})\s+");

            InitialiseRegister("MEMPTR", "0000",@"MEMPTR\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("INT", "DI", @"\s+(DI|EI)\s+");
            InitialiseRegister("IMode", "IM1", @"\s+(IM0|IM1|IM2)\s+");
            InitialiseRegister("VPS", "0", @"VPS\s*:\s*(\w)");

            InitialiseRegister("A'", "00", @"A\'\s*=\s*([a-fA-F0-9]{2})\s+");
            InitialiseRegister("F'", "00", @"F\'\s*=\s*([SZ5P3HNC ]+)\s+");
            InitialiseRegister("BC'", "0000", @"BC\'\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("DE'", "0000", @"DE\'\s*=\s*([a-fA-F0-9]{4})\s+");
            InitialiseRegister("HL'", "0000", @"HL\'\s*=\s*([a-fA-F0-9]{4})\s+");

            // Add registers
            dataGridView1.DataSource = registerData;

            dataGridView1.Columns[0].ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.CausesValidation = true;
        }
        override protected string GetPersistString()
        {
            return viewName;
        }

        public void RequestUpdate()
        {
            Program.telnetConnection.SendCommand("get-registers", Callback);
        }

        public string GetRegisterValue(string reg)
        {
            return registerData[nameMap[reg]].Value;
        }
        void UIUpdate(string[] items)
        {
            if (items.Count() != 1)
                return;
            bool updated = false;
            dataGridView1.CausesValidation = false;
            for (int r = 0; r < regexList.Count; r++)
            {
                Match m = regexList[r].Match(items[0]);
                if (m.Success)
                {
                    string newValue = m.Groups[1].Value;
                    if (registerData[r].Value != newValue)
                    {
                        registerData[r].Value = newValue;
                        updated = true;
                    }
                }
            }
            if (updated)
            {
                dataGridView1.Invalidate(true);
            }
            dataGridView1.CausesValidation = true;
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

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 1)
            {
                dataGridView1.Rows[e.RowIndex].ErrorText = "";
                string regName = dataGridView1.Rows[e.RowIndex].Cells[0].Value as string;
                string cValue = dataGridView1.Rows[e.RowIndex].Cells[1].Value as string;
                if (cValue != e.FormattedValue.ToString())
                {
                    Program.telnetConnection.SendCommand("set-register " + regName + "=" + e.FormattedValue.ToString(), null);
                }
            }
        }
    }

    class RegisterItem
    {
        public string Register { get; set; }
        public string Value { get; set; }
    }

}
