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
                if (arr[i] != '0')
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

            public string Url { get; set; }
            public IList<Area> Childs { get; set; }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var time = this.textBox1.Text;
            var host = "http://www.stats.gov.cn/tjsj/tjbz/tjyqhdmhcxhfdm/" + time + "/";

            var txt = Net.GetRequest(host, null, Encoding.GetEncoding("gb2312"));
            if (!string.IsNullOrEmpty(txt))
            {
                var ms = Regex.Matches(txt, @"<a\s+href='(?<url>\w+)\.html'>(?<name>\w+)<");
                var list = new List<Area>();
                foreach (Match item in ms)
                {
                    list.Add(new Area()
                    {
                        Code = item.Groups["url"].Value,
                        Name = item.Groups["name"].Value
                    });
                }
                list.AsParallel().ForAll((c) =>
                {
                    InitUrlChild(host, c.Code + ".html", c);
                });

                var str = JsonConvert.SerializeObject(list);
                using (var sw = new StreamWriter(@"d:\\alljson.txt", true, Encoding.GetEncoding("gb2312")))
                {
                    sw.Write(str);
                }
                MessageBox.Show("Ok");

            }
        }

        private void InitUrlChild(string host, string url, Area top)
        {
            var rUrl = host + url;
            var arr = rUrl.ToArray();
            var index = 0;
            for (var i = arr.Length - 1; i >= 0; i--)
            {
                if (arr[i] == '/')
                {
                    index = i;
                    break;
                }
            }
            host = rUrl.Substring(0, index + 1);
            var txt = Net.GetRequest(rUrl, null, Encoding.GetEncoding("gb2312"), 10000);
            if (!string.IsNullOrEmpty(txt))
            {
                var ms = Regex.Matches(txt, @"<a\s+href='(?<url>[\d\/]+\.html)'>(?<code>\d+)</.*?href.*?>(?<name>\w+)<");
                var list = new List<Area>();
                if (ms.Count > 0)
                {
                    foreach (Match item in ms)
                    {
                        var r = new Area()
                        {
                            Code = item.Groups["code"].Value,
                            Name = item.Groups["name"].Value,
                            Url = item.Groups["url"].Value
                        };
                        list.Add(r);
                    }

                    list.AsParallel().ForAll((c) =>
                    {
                        InitUrlChild(host, c.Url, c);
                    });

                }
                else
                {
                    ms = Regex.Matches(txt, @"<tr.*?class='villagetr.*?>(?<code>\d+)<.*?<td>\d+</td><td>(?<name>\w+)<");

                    foreach (Match item in ms)
                    {
                        var r = new Area()
                        {
                            Code = item.Groups["code"].Value,
                            Name = item.Groups["name"].Value
                        };
                        list.Add(r);
                    }
                }

                top.Childs = list;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var rootUrl = txtRoot.Text.Trim();
            var rootRegex = txtRootRegex.Text.Trim();
            var host = txtHost.Text.Trim();
            var isHost = chHost.Checked;
            var contentRegex = txtContentRegex.Text.Trim();

            var encoding = Encoding.GetEncoding("gb2312");
            if (!this.radioButton1.Checked)
                encoding = Encoding.UTF8;
            var timeout = 100000;
            this.button4.Enabled = false;
            Task.Factory.StartNew(() =>
            {
                var rootContent = Net.GetRequest(rootUrl, null, encoding, timeout);
                using (var sw = new StreamWriter(@"d:\a.txt", false, encoding))
                {
                    if (!string.IsNullOrEmpty(rootContent))
                    {
                        var urlList = Regex.Matches(rootContent, rootRegex);
                        if (urlList.Count > 0)
                        {
                            foreach (Match m in urlList)
                            {
                                var url = m.Groups["url"].Value;
                                var title = m.Groups["title"].Value;
                                sw.WriteLine(title);
                                if (isHost)
                                    url = host + url;
                                var content = Net.GetRequest(url, null, encoding, timeout);
                                if (!string.IsNullOrEmpty(content))
                                {
                                    var txtMatch = Regex.Match(content, contentRegex);
                                    if (txtMatch.Success)
                                    {
                                        var txt = txtMatch.Groups["txt"].Value;
                                        txt = txt.Replace("&nbsp;", "");
                                        txt = Regex.Replace(txt, @"<([^>]*)>", new MatchEvaluator((mt) =>
                                        {
                                            return "";
                                        }));
                                        sw.WriteLine(txt);
                                    }
                                }
                            }
                        }

                    }

                }
                this.Invoke((Action)(() =>
                    {
                        MessageBox.Show("Ok");
                        button4.Enabled = true;
                    }));

            });

        }



        private void button5_Click(object sender, EventArgs e)
        {
            var encoding = Encoding.GetEncoding("gb2312");
            if (!this.radioButton1.Checked)
                encoding = Encoding.UTF8;
            var url = textBox2.Text;
            if (!string.IsNullOrEmpty(url))
            {
                var content = Net.GetRequest(url, null, encoding);
                Clipboard.SetText(content);
                MessageBox.Show("Ok");
            }
        }
    }
}
