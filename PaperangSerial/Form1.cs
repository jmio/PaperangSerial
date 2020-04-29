using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace PaperangSerial
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// PAPERANG P1 PRINTER
        /// </summary>
        Paperang p;

        /// <summary>
        /// BTSerial Port Dictionary with description
        /// </summary>
        SortedDictionary<string, string> ComPortDict;

        /// <summary>
        /// Combobox Item
        /// </summary>
        class ComItems
        {
            public string comportname, description;

            public ComItems(KeyValuePair<string,string> k)
            {
                comportname = k.Key; description = k.Value;
            }

            /// <summary>
            /// Listbox Item Description
            /// </summary>
            /// <returns>Item String</returns>
            public override string ToString()
            {
                return comportname +" : ["+ description+"]";
            }
        }

        /// <summary> 
        /// Main Form
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            ComPortDict = EnumBluetoothSerial.EnumBTSerial();
            foreach (var k in ComPortDict) {
                comboBox1.Items.Add(new ComItems(k));
            }
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Fix Comport
        /// </summary>
        void FixComPort()
        {
            if (p == null)
            {
                ComItems i = comboBox1.SelectedItem as ComItems;
                if (i != null)
                {
                    p = new Paperang(i.comportname, logCallBack);
                    if (p.RegisterKey())
                    {
                        comboBox1.Enabled = false;
                    } 
                    else
                    {
                        p.Close();
                        p = null;
                    }
                }
            }
        }

        /// <summary>
        /// FEED Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            FixComPort();
            p?.LineFeed(44 + p.linepadding);
        }

        /// <summary>
        /// SEND Button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            FixComPort();
            p?.Print(textBox1.Text);
        }

        /// <summary>
        /// Log Callback
        /// </summary>
        /// <param name="msg"></param>
        public void logCallBack(string msg)
        {
            Invoke(new Action(() => {
                richTextBox1.Focus();
                richTextBox1.AppendText(msg + "\r\n");
            }));
        }

    }
}
