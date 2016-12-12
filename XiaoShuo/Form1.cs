using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
namespace XiaoShuo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var file = openFileDialog1.FileName;
                using (var sw = new StreamWriter("d:\\a.txt"))
                {
                    using (var sr = new StreamReader(file, Encoding.GetEncoding("gb2312")))
                    {
                        var txt = sr.ReadToEnd();
                        var ms = Regex.Matches(txt, @"第(?<name>.*?)章\s");
                        if (ms.Count > 0)
                        {
                            var key = "";
                            var index = 0;
                            foreach (Match item in ms)
                            {
                                if (key != item.Groups["name"].Value)
                                {
                                    key = item.Groups["name"].Value;
                                    var sub = txt.Substring(index, item.Index - index);
                                    index = item.Index;
                                    sw.Write(sub);
                                }
                                else
                                {
                                    index = item.Index;
                                }
                            }

                            sw.Write(txt.Substring(ms[ms.Count - 1].Index));
                        }

                    }
                }

            }
        }
    }
}
