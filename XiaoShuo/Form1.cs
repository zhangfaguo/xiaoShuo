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
using Newtonsoft.Json;

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

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var file = openFileDialog1.FileName;
                var txtContent = string.Empty;
                using (var sr = new StreamReader(file, Encoding.GetEncoding("gb2312")))
                {
                    txtContent = sr.ReadToEnd();
                }
                if (!string.IsNullOrEmpty(txtContent))
                {
                    var ms = Regex.Matches(txtContent, @"(?<code>\d+)\s+(?<name>\w+)");
                    if (ms.Count > 0)
                    {
                        var list = new List<Area>();

                        foreach (Match item in ms)
                        {
                            list.Add(new Area()
                            {
                                Code = item.Groups["code"].Value,
                                Name = item.Groups["name"].Value
                            });
                        }

                        if (list.Count > 0)
                        {
                            var jsonData = new List<Area>();
                            #region 获取区域
                            var topList = list.Where(c => c.Code.EndsWith("0000"));
                            var temp = new List<Area>();
                            jsonData.AddRange(topList);

                            foreach (var item in jsonData)
                            {
                                InitChild(item, list);
                            }
                            if (jsonData.Count > 0)
                            {
                                var str = JsonConvert.SerializeObject(jsonData);
                                using (var sw = new StreamWriter(@"d:\\json.txt", true, Encoding.Default))
                                {
                                    sw.Write(str);
                                }
                            }
                            #endregion
                        }
                    }
                }
            }
        }

        private void InitChild(Area top, List<Area> data)
        {
            var code = top.Code;
            var arr = code.ToArray();
            var index = 0;
            var len = arr.Count();
            for (var i = len - 1; i >= 0; i--)
            {
                if (arr[i] !='0')
                {
                    index = i;
                    break;
                }
            }
            var temp = "";
            for (var i = 0; i <= index; i++)
            {
                temp += arr[i];
            }
            temp += "(?<num>\\d{2})";

            for (var i = 5; i > index + 2; i--)
            {
                temp += "0";
            }

            if (index != 5)
            {
                var list = data.Where(c => Regex.Match(c.Code, temp).Success && c.Code != code).ToList();
                top.Childs = list;

                foreach (var item in top.Childs)
                {
                    InitChild(item, data);
                }
            }
        }

        private class Area 
        {
            public string Code { get; set; }

            public string Name { get; set; }
            public IList<Area> Childs { get; set; }
        }

     
    }
}
