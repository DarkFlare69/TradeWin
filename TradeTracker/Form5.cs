using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TradeTracker;

namespace TradeWin
{
    public partial class Form5 : Form
    {
        Form1 _form1Instance;
        string date;
        public Form5(Form1 form1Instance)
        {
            this._form1Instance = form1Instance;
            InitializeComponent();
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            string path = Form1.calenderPath;
            date = Regex.Match(_form1Instance.monthCalendar1.SelectionRange.ToString(), @"Start: (.+?) 12:00").ToString();
            date = date.Substring(7, date.Length - 13);
            groupBox2.Text = "Events For " + date;
            checkBox2.Text = "Show Notification On " + date;
            if (File.Exists(path))
            {
                string line;
                StreamReader file = new StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length < 11)
                        continue;
                    if (line.Contains("DailyMessage,"))
                    {
                        textBox2.Text = line.Substring(15, line.Length - 16).Replace("~~~", "\r\n");
                        if (line.Contains("DailyMessage,0,"))
                        {
                            checkBox1.Checked = false;
                        }
                    }
                    if (line.Contains(date))
                    {
                        textBox1.Text = line.Substring(date.Length + 3, line.Length - (date.Length + 4)).Replace("~~~", "\r\n");
                        if (line.Contains(date + ",0,"))
                        {
                            checkBox2.Checked = false;
                        }
                        break;
                    }
                }
                file.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = Form1.calenderPath;
            string date = Regex.Match(_form1Instance.monthCalendar1.SelectionRange.ToString(), @"Start: (.+?) 12:00").ToString();
            date = date.Substring(7, date.Length - 13) + ",";
            //long length = new System.IO.FileInfo(path).Length;
            if (File.Exists(path) && new System.IO.FileInfo(path).Length > 5)
            {
                string line, fileContents = "";
                StreamReader file = new StreamReader(path);
                while ((line = file.ReadLine()) != null)
                {
                    if (line != "")
                        fileContents += line + "\n";
                }
                file.Close();
                string newDailyMsg = "DailyMessage," + (checkBox1.Checked.ToString() == "True" ? "1," : "0,") + textBox2.Text.Replace("\r\n", "~~~") + ",\n";
                string newEntry = date + (checkBox2.Checked.ToString() == "True" ? "1," : "0,") + textBox1.Text.Replace("\r\n", "~~~") + ",\n";
                if (fileContents.Contains("DailyMessage,"))
                {
                    string dailyMessageToReplace = Regex.Match(fileContents, @"DailyMessage,(.+?),\n").ToString();
                    string entryToReplace = "";
                    fileContents = fileContents.Replace(dailyMessageToReplace, newDailyMsg);
                    if (fileContents.Contains(date))
                    {
                        entryToReplace = Regex.Match(fileContents, date + "(.+?),\n").ToString();
                        fileContents = fileContents.Replace(entryToReplace, newEntry);
                        File.WriteAllText(path, fileContents);
                    }
                    else
                        File.WriteAllText(path, fileContents + newEntry);
                }
                else
                    File.WriteAllText(path, newDailyMsg + newEntry);
            }
            else
                File.WriteAllText(path, "DailyMessage," + (checkBox1.Checked.ToString() == "True" ? "1," : "0,") + textBox2.Text + ",\n" + date + (checkBox2.Checked.ToString() == "True" ? "1," : "0,") + textBox1.Text + ",\n");
            Close();
        }
    }
}
