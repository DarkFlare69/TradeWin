using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TradeWin;

namespace TradeTracker
{
    public partial class Form1 : Form
    {
        public DataGridView exportGrid = new DataGridView();
        static DataTable watchList = new DataTable(), history = new DataTable();
        public static bool watchActive, autoSaveWatch, autoSaveTHistory, perSale = false, perShare = false, perDollar = false, loaded = false;
        static string versionString = "v1.0", watchPath, historyPath, strategyPath;
        public static string calenderPath;
        public float[] commissions = new float[4];
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //chart1.ChartType = SeriesChartType.Line;
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\dev");
            string settingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\settings.bin";
            watchPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin";
            historyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin";
            strategyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin";
            calenderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin";
            byte[] test = new byte[1];
            try
            {
                using (BinaryReader reader = new BinaryReader(new FileStream(settingPath, FileMode.Open)))
                {
                    reader.BaseStream.Seek(7, SeekOrigin.Begin);
                    reader.Read(test, 0, 1);
                    if (test[0] == 0)
                    {
                        watchPath += "\\watchlist.atw";
                        historyPath += "\\history.tw";
                        strategyPath += "\\strategy.tw";
                        calenderPath += "\\calender.txt";
                    }
                    else if (test[0] == 1) // use dev directory
                    {
                        watchPath += "\\dev\\watchlist.atw";
                        historyPath += "\\dev\\history.tw";
                        strategyPath += "\\dev\\strategy.tw";
                        calenderPath += "\\dev\\calender.txt";
                    }
                }
            }
            catch
            {
                watchPath += "\\watchlist.atw";
                historyPath += "\\history.tw";
                strategyPath += "\\strategy.tw";
                calenderPath += "\\calender.txt";
            }
            exportGrid.Columns.Add("Symbol", "Symbol");
            exportGrid.Columns.Add("Date", "Date");
            exportGrid.Columns.Add("Position", "Position");
            exportGrid.Columns.Add("Quantity", "Quantity");
            exportGrid.Columns.Add("PricePerShare", "Price Per Share");
            exportGrid.Columns.Add("EarningsPerShare", "Earnings Per Share");
            exportGrid.Columns.Add("Price", "Price");
            exportGrid.Columns.Add("Earnings", "Earnings");
            exportGrid.Columns.Add("Strategy", "Identified Strategy");
            exportGrid.Columns.Add("MajorLevels", "Identified Major Levels");
            exportGrid.Columns.Add("Strengths", "Strengths");
            exportGrid.Columns.Add("Weaknesses", "Weaknesses");
            exportGrid.Columns.Add("Notes", "Notes");
            exportGrid.Rows.Add();
            watchList.Columns.Add("RowCount");
            Watch.AutoGenerateColumns = false;
            Watch.DataSource = watchList;
            history.Columns.Add("RowCount");
            THistory.AutoGenerateColumns = false;
            THistory.DataSource = history;
            autoSaveWatch = true;
            autoSaveTHistory = true;
            watchActive = true;
            if (File.Exists(Process.GetCurrentProcess().MainModule.FileName + ".bak"))
            {
                File.Delete(Process.GetCurrentProcess().MainModule.FileName + ".bak");
            }
            if (!File.Exists(watchPath) || new FileInfo(watchPath).Length < 17)
            {
                File.WriteAllText(watchPath, ":AAPL,,,,,,,,,,,,,\n:QCOM,,,,,,,,,,,,,\n:MU,,,,,,,,,,,,,");
            }
            if (!File.Exists(historyPath))
            {
                File.WriteAllText(historyPath, ":,,,,,,,,,,,,,");
            }
            if (!File.Exists(strategyPath))
            {
                File.WriteAllText(strategyPath, ":,,,,,,,");
            }
            if (!File.Exists(settingPath))
            {
                try
                {
                    using (FileStream fs = File.Create(settingPath))
                    {
                        byte[] settings = { 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        fs.Write(settings, 0, 24);
                    }
                }
                catch { }
                MessageBox.Show("TradeWin is designed to be a free, open source trade logging application. This allows importing trade history from Fidelity (other brokers potentially coming), exporting/loading from TradeWin proprietary format (.tw) and more!\n\nTraders are intended to make trading decisions based on their own judgement. TradeWin is simply an additional performance monitoring tool for traders to log their trades on a free, offline platform.", "Welcome to TradeWin! - " + versionString);
            }
            if (File.Exists(calenderPath))
            {
                string line;
                StreamReader file = new StreamReader(calenderPath);
                string dailyMessage = "";
                string todaysMessage = "";
                string todaysDate = DateTime.Now.ToString("MM/dd/yyyy") + ",1,";
                if (todaysDate.StartsWith("0"))
                {
                    todaysDate = todaysDate.Substring(1, todaysDate.Length - 1);
                }
                if (todaysDate.Contains("/0"))
                {
                    todaysDate = todaysDate.Replace("/0", "/");
                }
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length < 12)
                        continue;
                    if (line.Contains("DailyMessage,1,"))
                    {
                        dailyMessage = line.Substring(15, line.Length - 16);
                    }
                    if (line.Contains(todaysDate))
                    {
                        todaysMessage = line.Substring(todaysDate.Length, line.Length - todaysDate.Length - 1);
                    }
                }
                if (todaysMessage != "")
                {
                    MessageBox.Show(todaysMessage.Replace("~~~", "\r\n"), "Todays Ideas");
                }
                if (dailyMessage != "")
                {
                    MessageBox.Show(dailyMessage.Replace("~~~", "\r\n"), "Daily Message");
                }
                file.Close();
            }
            if (File.Exists(strategyPath))
            {
                string line;
                int counter = 0;
                StreamReader file = new StreamReader(strategyPath);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Length < 11)
                        continue;
                    if (line.IndexOf(',') - line.IndexOf(':') > 1)
                    {
                        IdentifiedStratTable.Rows.Add();
                        IdentifiedStratTable.Rows[counter].Cells["Column5"].Value = line.Substring(line.IndexOf(':') + 1, line.IndexOf(',') - line.IndexOf(':') - 1);
                        IdentifiedStratTable.Rows[counter].Cells["Target"].Value = line.Substring(GetNth(line, ',', 1), GetNth(line, ',', 2) - GetNth(line, ',', 1) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                        IdentifiedStratTable.Rows[counter].Cells["AddNewTradesFrom"].Value = line.Substring(GetNth(line, ',', 2), GetNth(line, ',', 3) - GetNth(line, ',', 2) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                        IdentifiedStratTable.Rows[counter].Cells["Note"].Value = line.Substring(GetNth(line, ',', 3), GetNth(line, ',', 4) - GetNth(line, ',', 3) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                        IdentifiedStratTable.Rows[counter].Cells["Keywords"].Value = line.Substring(GetNth(line, ',', 4), GetNth(line, ',', 5) - GetNth(line, ',', 4) - 1).Replace(".", ","); // populate each cell in row here, THIS DOESNT WORK RN
                        IdentifiedStratTable.Rows[counter].Cells["Exclusions"].Value = line.Substring(GetNth(line, ',', 5), GetNth(line, ',', 6) - GetNth(line, ',', 5) - 1).Replace(".", ","); // populate each cell in row here, THIS DOESNT WORK RN
                        IdentifiedStratTable.Rows[counter].Cells["AvgWin"].Value = line.Substring(GetNth(line, ',', 6), GetNth(line, ',', 7) - GetNth(line, ',', 6) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                        IdentifiedStratTable.Rows[counter].Cells["AvgLoss2"].Value = line.Substring(GetNth(line, ',', 7), GetNth(line, ',', 8) - GetNth(line, ',', 7) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                        counter++;
                    }

                }
                File.Copy(strategyPath, strategyPath + ".init.bak", true);
                file.Close();
            }
            if (File.Exists(settingPath))
            {
                using (BinaryReader fileStream = new BinaryReader(File.Open(settingPath, FileMode.Open)))
                {
                    for (int i = 0; i < 12; i++)
                    {
                        if (i == 0 && fileStream.ReadByte() == 1) // Import Watchlist
                        {
                            if (File.Exists(watchPath))
                            {
                                string line;
                                int counter = 0;
                                StreamReader file = new StreamReader(watchPath);
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (line.Length < 16)
                                        continue;
                                    if (line.IndexOf(',') - line.IndexOf(':') > 1)
                                    {
                                        watchList.Rows.Add();
                                        Watch.Rows[counter].Cells["Column1"].Value = line.Substring(line.IndexOf(':') + 1, line.IndexOf(',') - line.IndexOf(':') - 1);
                                        Watch.Rows[counter].Cells["Column3"].Value = line.Substring(GetNth(line, ',', 1), GetNth(line, ',', 2) - GetNth(line, ',', 1) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                        Watch.Rows[counter].Cells["Column2"].Value = line.Substring(GetNth(line, ',', 2), GetNth(line, ',', 3) - GetNth(line, ',', 2) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                        Watch.Rows[counter].Cells["Entry"].Value = line.Substring(GetNth(line, ',', 3), GetNth(line, ',', 4) - GetNth(line, ',', 3) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                        Watch.Rows[counter].Cells["Notes"].Value = line.Substring(GetNth(line, ',', 4), GetNth(line, ',', 5) - GetNth(line, ',', 4) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                        Watch.Rows[counter].Cells["Actual"].Value = line.Substring(GetNth(line, ',', 5), GetNth(line, ',', 6) - GetNth(line, ',', 5) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                        Watch.Rows[counter].Cells["PositionType"].Value = line.Substring(GetNth(line, ',', 6), GetNth(line, ',', 7) - GetNth(line, ',', 6) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                        Watch.Rows[counter].Cells["ExitPrice"].Value = line.Substring(GetNth(line, ',', 7), GetNth(line, ',', 8) - GetNth(line, ',', 7) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                        counter++;
                                    }
                                }
                                File.Copy(watchPath, watchPath + ".init.bak", true);
                                file.Close();
                            }
                        }
                        if (i == 1 && fileStream.ReadByte() == 1) // Import Trade History
                        {
                            if (File.Exists(historyPath))
                            {
                                string line;
                                int counter = 0;
                                StreamReader file = new StreamReader(historyPath);
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (line.Length < 17 || !(line.IndexOf(',') - line.IndexOf(':') > 1))
                                        continue;
                                    history.Rows.Add();
                                    THistory.Rows[counter].Cells["Symbol"].Value = line.Substring(line.IndexOf(':') + 1, GetNth(line, ',', 1) - line.IndexOf(':') - 2); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Date"].Value = line.Substring(GetNth(line, ',', 1), GetNth(line, ',', 2) - GetNth(line, ',', 1) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Side"].Value = line.Substring(GetNth(line, ',', 2), GetNth(line, ',', 3) - GetNth(line, ',', 2) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Quantity"].Value = line.Substring(GetNth(line, ',', 3), GetNth(line, ',', 4) - GetNth(line, ',', 3) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Price"].Value = line.Substring(GetNth(line, ',', 4), GetNth(line, ',', 5) - GetNth(line, ',', 4) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Earnings"].Value = line.Substring(GetNth(line, ',', 5), GetNth(line, ',', 6) - GetNth(line, ',', 5) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Amount"].Value = line.Substring(GetNth(line, ',', 6), GetNth(line, ',', 7) - GetNth(line, ',', 6) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Earn"].Value = line.Substring(GetNth(line, ',', 7), GetNth(line, ',', 8) - GetNth(line, ',', 7) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["EPS"].Value = line.Substring(GetNth(line, ',', 8), GetNth(line, ',', 9) - GetNth(line, ',', 8) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["GainLoss"].Value = line.Substring(GetNth(line, ',', 9), GetNth(line, ',', 10) - GetNth(line, ',', 9) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Strategy"].Value = line.Substring(GetNth(line, ',', 10), GetNth(line, ',', 11) - GetNth(line, ',', 10) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["MajorLevels"].Value = line.Substring(GetNth(line, ',', 11), GetNth(line, ',', 12) - GetNth(line, ',', 11) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Strengths"].Value = line.Substring(GetNth(line, ',', 12), GetNth(line, ',', 13) - GetNth(line, ',', 12) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Weaknesses"].Value = line.Substring(GetNth(line, ',', 13), GetNth(line, ',', 14) - GetNth(line, ',', 13) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Notes2"].Value = line.Substring(GetNth(line, ',', 14), GetNth(line, ',', 15) - GetNth(line, ',', 14) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    counter++;
                                }
                                File.Copy(historyPath, historyPath + ".init.bak", true);
                                file.Close();
                            }
                        }
                        if (i == 2 && fileStream.ReadByte() == 0) // AutoSave Watchlist
                        {
                            autoSaveWatch = false;
                        }
                        if (i == 3 && fileStream.ReadByte() == 0) // AutoSave History
                        {
                            autoSaveTHistory = false;
                        }
                        if (i == 4 && fileStream.ReadByte() == 1)
                            perSale = true;
                        if (i == 5 && fileStream.ReadByte() == 1)
                            perShare = true;
                        if (i == 6 && fileStream.ReadByte() == 1)
                            perDollar = true;
                        if (i == 7 && fileStream.ReadByte() == 1) { }
                        if (i == 8)
                            commissions[0] = fileStream.ReadSingle();
                        if (i == 9)
                            commissions[1] = fileStream.ReadSingle();
                        if (i == 10)
                            commissions[2] = fileStream.ReadSingle();
                        if (i == 11)
                            commissions[3] = fileStream.ReadSingle();
                    }
                }
            }
            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\stratcalc.csv"))
            {
                string fileContent = "";
                using (StreamReader reader = new StreamReader(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\stratcalc.csv"))
                {
                    fileContent = reader.ReadToEnd();
                    MatchCollection match = Regex.Matches(fileContent, @",(.+?),");
                    if (match.Count == 5)
                    {
                        textBox1.Text = match[0].ToString() != null && match[0].ToString().Length > 2 ? match[0].ToString().Substring(1, match[0].Length - 2) : "";
                        textBox2.Text = match[1].ToString() != null && match[1].ToString().Length > 2 ? match[1].ToString().Substring(1, match[1].Length - 2) : "";
                        textBox3.Text = match[2].ToString() != null && match[2].ToString().Length > 2 ? match[2].ToString().Substring(1, match[2].Length - 2) : "";
                        textBox4.Text = match[3].ToString() != null && match[3].ToString().Length > 2 ? match[3].ToString().Substring(1, match[3].Length - 2) : "";
                        textBox5.Text = match[4].ToString() != null && match[4].ToString().Length > 2 ? match[4].ToString().Substring(1, match[4].Length - 2) : "";
                        File.Copy(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\stratcalc.csv", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\stratcalc.csv.init.bak", true);
                    }
                    reader.Close();
                }
            }
            loaded = true;
            updateStrategy(THistory, IdentifiedStratTable);
        }
        private void ImportTWFileToolStripMenuItem_Click(object sender, EventArgs e) // Import TW To History
        {
            string fileContent, filePath;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                openFileDialog.Filter = "TradeWin Trading History (*.tw)|*.tw|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                        MatchCollection match = Regex.Matches(fileContent, @":(.+?)\n"); // break file down into lines
                        //THistory.DataSource = null;
                        int counter;
                        counter = THistory.Rows.Count > 1 ? THistory.Rows.Count - 1 : 0;
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (match[i].ToString().Contains(",") && match[i].ToString().Length > 16) // All compatible lines
                            {
                                history.Rows.Add();
                                THistory.Rows[counter].Cells["Symbol"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ':', 1), GetNth(match[i].ToString(), ',', 1) - 1 - GetNth(match[i].ToString(), ':', 1));
                                THistory.Rows[counter].Cells["Date"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 1), GetNth(match[i].ToString(), ',', 2) - 1 - GetNth(match[i].ToString(), ',', 1));
                                THistory.Rows[counter].Cells["Side"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 2), GetNth(match[i].ToString(), ',', 3) - 1 - GetNth(match[i].ToString(), ',', 2));
                                THistory.Rows[counter].Cells["Quantity"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 3), GetNth(match[i].ToString(), ',', 4) - 1 - GetNth(match[i].ToString(), ',', 3));
                                THistory.Rows[counter].Cells["Price"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 4), GetNth(match[i].ToString(), ',', 5) - 1 - GetNth(match[i].ToString(), ',', 4));
                                THistory.Rows[counter].Cells["Earnings"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 5), GetNth(match[i].ToString(), ',', 6) - 1 - GetNth(match[i].ToString(), ',', 5));
                                THistory.Rows[counter].Cells["Amount"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 6), GetNth(match[i].ToString(), ',', 7) - 1 - GetNth(match[i].ToString(), ',', 6));
                                THistory.Rows[counter].Cells["Earn"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 7), GetNth(match[i].ToString(), ',', 8) - 1 - GetNth(match[i].ToString(), ',', 7));
                                THistory.Rows[counter].Cells["EPS"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 8), GetNth(match[i].ToString(), ',', 9) - 1 - GetNth(match[i].ToString(), ',', 8));
                                THistory.Rows[counter].Cells["GainLoss"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 9), GetNth(match[i].ToString(), ',', 10) - 1 - GetNth(match[i].ToString(), ',', 9));
                                THistory.Rows[counter].Cells["Strategy"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 10), GetNth(match[i].ToString(), ',', 11) - 1 - GetNth(match[i].ToString(), ',', 10));
                                THistory.Rows[counter].Cells["MajorLevels"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 11), GetNth(match[i].ToString(), ',', 12) - 1 - GetNth(match[i].ToString(), ',', 11));
                                THistory.Rows[counter].Cells["Strengths"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 12), GetNth(match[i].ToString(), ',', 13) - 1 - GetNth(match[i].ToString(), ',', 12));
                                THistory.Rows[counter].Cells["Weaknesses"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 13), GetNth(match[i].ToString(), ',', 14) - 1 - GetNth(match[i].ToString(), ',', 13));
                                THistory.Rows[counter].Cells["Notes2"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 14), GetNth(match[i].ToString(), ',', 15) - 1 - GetNth(match[i].ToString(), ',', 14));
                                counter++;
                            }
                        }
                        reader.Close();
                    }
                }
            }
        }

        private void fromTradeHistorycsvToolStripMenuItem_Click(object sender, EventArgs e) // Import TW To Watch
        {
            string fileContent, filePath;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                openFileDialog.Filter = "TradeWin Active Trades (*.atw)|*.atw|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                        MatchCollection match = Regex.Matches(fileContent, @":(.+?)\n"); // break file down into lines
                        //THistory.DataSource = null;
                        int counter;
                        counter = Watch.Rows.Count > 1 ? Watch.Rows.Count - 1 : 0;
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (match[i].ToString().Contains(",") && match[i].ToString().Length > 12) // All compatible lines
                            {
                                watchList.Rows.Add();
                                Watch.Rows[counter].Cells["Column1"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ':', 1), GetNth(match[i].ToString(), ',', 1) - 1 - GetNth(match[i].ToString(), ':', 1)).Replace("~~~", "\r\n");
                                Watch.Rows[counter].Cells["Column3"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 1), GetNth(match[i].ToString(), ',', 2) - 1 - GetNth(match[i].ToString(), ',', 1)).Replace("~~~", "\r\n");
                                Watch.Rows[counter].Cells["Column2"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 2), GetNth(match[i].ToString(), ',', 3) - 1 - GetNth(match[i].ToString(), ',', 2)).Replace("~~~", "\r\n");
                                Watch.Rows[counter].Cells["Entry"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 3), GetNth(match[i].ToString(), ',', 4) - 1 - GetNth(match[i].ToString(), ',', 3)).Replace("~~~", "\r\n");
                                Watch.Rows[counter].Cells["Notes"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 4), GetNth(match[i].ToString(), ',', 5) - 1 - GetNth(match[i].ToString(), ',', 4)).Replace("~~~", "\r\n");
                                Watch.Rows[counter].Cells["Actual"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 5), GetNth(match[i].ToString(), ',', 6) - 1 - GetNth(match[i].ToString(), ',', 5)).Replace("~~~", "\r\n");
                                Watch.Rows[counter].Cells["PositionType"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 6), GetNth(match[i].ToString(), ',', 7) - 1 - GetNth(match[i].ToString(), ',', 6)).Replace("~~~", "\r\n");
                                Watch.Rows[counter].Cells["ExitPrice"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 7), GetNth(match[i].ToString(), ',', 8) - 1 - GetNth(match[i].ToString(), ',', 7)).Replace("~~~", "\r\n");
                                counter++;
                            }
                        }
                        reader.Close();
                    }
                }
            }
        }

        private void fromWatchlistToolStripMenuItem_Click(object sender, EventArgs e) // Import .txt watchlist     DONE
        {
            string fileContent, filePath;

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();
                    int counter;
                    counter = Watch.Rows.Count > 1 ? Watch.Rows.Count - 1 : 0;
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                        MatchCollection match = Regex.Matches(fileContent, @":(.+?),");
                        for (int i = 0; i < match.Count; i++)
                        {
                            watchList.Rows.Add();
                            Watch.Rows[counter].Cells["Column1"].Value = match[i].ToString().Substring(1, match[i].Length - 2);
                            counter++;
                        }
                        reader.Close();
                    }
                }
            }
        }

        public int GetNth(string s, char t, int n)
        {
            int count = 0;
            for (int i = 0; i < s.Length; i++)
                if (s[i] == t)
                {
                    count++;
                    if (count == n)
                        return i + 1;
                }
            return -1;
        }
        private void tradeWinttwToolStripMenuItem_Click(object sender, EventArgs e) // Import Fidelity Trading history   DONE
        {
            string fileContent, filePath;
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                openFileDialog.Filter = "Fidelity Trade History (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        fileContent = reader.ReadToEnd();
                        MatchCollection match = Regex.Matches(fileContent, @"\n (.+?)\n"); // break file down into lines
                        //THistory.DataSource = null;
                        int counter;
                        counter = THistory.Rows.Count > 1 ? THistory.Rows.Count - 1 : 0;
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (match[i].ToString().Contains(", YOU ") && match[i].ToString().Contains(" (")) // All compatible lines
                            {
                                history.Rows.Add();
                                THistory.Rows[counter].Cells["Symbol"].Value = match[i].ToString().Substring(1 + GetNth(match[i].ToString(), ',', 2), GetNth(match[i].ToString(), ',', 3) - 2 - GetNth(match[i].ToString(), ',', 2));
                                THistory.Rows[counter].Cells["Date"].Value = match[i].ToString().Substring(2, GetNth(match[i].ToString(), ',', 1) - 3);
                                THistory.Rows[counter].Cells["Side"].Value = match[i].ToString().Contains(" SHORT COVER") ? "Buy Short Cover" : (match[i].ToString().Contains(" SHORT SALE") ? "Sell Short" : (match[i].ToString().Contains(" BOUGHT ") ? "Buy" : "Sell"));
                                THistory.Rows[counter].Cells["Quantity"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 5), GetNth(match[i].ToString(), ',', 6) - 1 - GetNth(match[i].ToString(), ',', 5));
                                THistory.Rows[counter].Cells["Price"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 6), GetNth(match[i].ToString(), ',', 7) - 1 - GetNth(match[i].ToString(), ',', 6));
                                THistory.Rows[counter].Cells["Amount"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 10), GetNth(match[i].ToString(), ',', 11) - 1 - GetNth(match[i].ToString(), ',', 10));
                                THistory.Rows[counter].Cells["Earn"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 7), GetNth(match[i].ToString(), ',', 8) - 1 - GetNth(match[i].ToString(), ',', 7));
                                THistory.Rows[counter].Cells["Notes2"].Value = match[i].ToString().Substring(1 + GetNth(match[i].ToString(), ',', 3), GetNth(match[i].ToString(), ',', 4) - 2 - GetNth(match[i].ToString(), ',', 3));
                                counter++;
                            }
                        }
                        reader.Close();
                    }
                }
            }
        }

        private void Grid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (Watch.SelectedCells[0].ColumnIndex == 8)
            {
                Form2 form2 = new Form2(this, true);
                _ = form2.ShowDialog();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form3 form3 = new Form3(this);
            _ = form3.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (watchList.Rows.Count != 0)
            {
                watchList.Rows.Clear();
                return;
            }
            watchList.Rows.Clear();
            if (Watch.RowCount != 1)
                Watch.Rows.Clear();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (history.Rows.Count != 0)
            {
                history.Rows.Clear();
                return;
            }
            history.Rows.Clear();
            if (THistory.RowCount != 1)
                THistory.Rows.Clear();
        }

        private void openDataFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin"))
                Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin");
        }

        public void populateGrid(DataGridView dataGrid)
        {
            if (THistory.Rows.Count >= 1 && exportGrid.Rows[0].Cells["Symbol"].Value.ToString() != null)
            {
                string[] lineArray = new string[13] { exportGrid.Rows[0].Cells["Symbol"].Value.ToString(), exportGrid.Rows[0].Cells["Date"].Value.ToString(), exportGrid.Rows[0].Cells["Position"].Value.ToString(), exportGrid.Rows[0].Cells["Quantity"].Value.ToString(), exportGrid.Rows[0].Cells["PricePerShare"].Value.ToString(), exportGrid.Rows[0].Cells["EarningsPerShare"].Value.ToString(), exportGrid.Rows[0].Cells["Strengths"].Value.ToString(), exportGrid.Rows[0].Cells["Weaknesses"].Value.ToString(), exportGrid.Rows[0].Cells["Notes"].Value.ToString(), exportGrid.Rows[0].Cells["MajorLevels"].Value.ToString(), exportGrid.Rows[0].Cells["Strategy"].Value.ToString(), exportGrid.Rows[0].Cells["Earnings"].Value.ToString(), exportGrid.Rows[0].Cells["Price"].Value.ToString() };
                DataRow row = history.NewRow();
                history.Rows.InsertAt(row, 0);
                THistory.Rows[0].Cells["Symbol"].Value = lineArray[0];
                THistory.Rows[0].Cells["Date"].Value = lineArray[1];
                THistory.Rows[0].Cells["Side"].Value = lineArray[2];
                THistory.Rows[0].Cells["Quantity"].Value = lineArray[3];
                THistory.Rows[0].Cells["Price"].Value = lineArray[4];
                THistory.Rows[0].Cells["Earnings"].Value = lineArray[5];
                THistory.Rows[0].Cells["Strengths"].Value = lineArray[6];
                THistory.Rows[0].Cells["Weaknesses"].Value = lineArray[7];
                THistory.Rows[0].Cells["Notes2"].Value = lineArray[8];
                THistory.Rows[0].Cells["MajorLevels"].Value = lineArray[9];
                THistory.Rows[0].Cells["Strategy"].Value = lineArray[10];
                THistory.Rows[0].Cells["Earn"].Value = lineArray[11];
                THistory.Rows[0].Cells["Amount"].Value = lineArray[12];
                watchActive = false;
            }
        }
        private void writeStr(bool backup, string path2, string str)
        {
            try
            {
                if (backup && File.Exists(path2))
                {
                    File.Copy(path2, path2 + ".bak", true);
                }
            }
            catch (Exception ex) { //MessageBox.Show("First: " + ex.ToString());
            }
            try
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin");
                if (str.Length > 4)
                    File.WriteAllText(path2, str);
            }
            catch (Exception ex) { //MessageBox.Show("Second: " + ex.ToString());
            }
        }

        private void exportTWFile(bool back, bool active, string path2, DataGridView grid)
        {
            if (!(loaded && active))
                return;
            string str = "", s;
            foreach (DataGridViewRow row in grid.Rows)
            {
                str += ":";
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (cell.Value != null)
                    {
                        if (cell.Value.ToString() != "")
                        {
                            s = cell.Value.ToString().Replace(",", ".").Replace("\r\n", "~~~");
                            str += s;
                        }
                    }
                    str += ',';
                }
                str += "\n";
            }
            writeStr(back, path2, str);
        }

        private void tradeWintwToolStripMenuItem_Click(object sender, EventArgs e) // Export THistory to File    DONE
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                saveFileDialog.Filter = "TradeWin Trading History File (*.tw)|*.tw|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    exportTWFile(false, true, saveFileDialog.FileName, THistory);
                }
            }
        }

        private void THistory_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            exportTWFile(true, autoSaveTHistory, historyPath, THistory);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2(this, false);
            form2.ShowDialog();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (Watch.CurrentRow != null && Watch.CurrentRow.Index >= 0 && watchActive)
            {
                Watch.Rows[Watch.CurrentRow.Index].Cells["Column1"].Value = "";
                Watch.Rows[Watch.CurrentRow.Index].Cells["Column2"].Value = "";
                Watch.Rows[Watch.CurrentRow.Index].Cells["Column3"].Value = "";
                Watch.Rows[Watch.CurrentRow.Index].Cells["Entry"].Value = "";
                Watch.Rows[Watch.CurrentRow.Index].Cells["PositionType"].Value = null;
                Watch.Rows[Watch.CurrentRow.Index].Cells["Actual"].Value = "";
                Watch.Rows[Watch.CurrentRow.Index].Cells["Notes"].Value = "";
                Watch.Rows[Watch.CurrentRow.Index].Cells["ExitPrice"].Value = "";
            }
            else if (THistory.CurrentRow != null && THistory.CurrentRow.Index >= 0 && !watchActive)
            {
                THistory.Rows[THistory.CurrentRow.Index].Cells["Symbol"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Date"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Side"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Quantity"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Price"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Earnings"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Amount"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Earn"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["EPS"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["GainLoss"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Strategy"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["MajorLevels"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Strengths"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Weaknesses"].Value = "";
                THistory.Rows[THistory.CurrentRow.Index].Cells["Notes2"].Value = "";
            }
        }

        private void Watch_Enter(object sender, EventArgs e)
        {
            watchActive = true;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(watchPath + ".bak"))
                {
                    string fileContent = "";
                    using (StreamReader reader = new StreamReader(watchPath + ".bak"))
                    {
                        fileContent = reader.ReadToEnd();
                        MatchCollection match = Regex.Matches(fileContent, @":(.+?),"); // break file down into lines
                        watchList.Rows.Clear();
                        int counter = 0;
                        //counter = Watch.Rows.Count > 1 ? Watch.Rows.Count - 1 : 0;
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (match[i].ToString().Length > 3) // All compatible lines
                            {
                                watchList.Rows.Add();
                                Watch.Rows[counter].Cells["Column1"].Value = match[i].ToString().Substring(1, match[i].ToString().Length - 2);
                                counter++;
                            }
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(historyPath + ".bak"))
                {
                    string fileContent = "";
                    using (StreamReader reader = new StreamReader(historyPath + ".bak"))
                    {
                        fileContent = reader.ReadToEnd();
                        MatchCollection match = Regex.Matches(fileContent, @":(.+?)\n");
                        history.Rows.Clear();
                        //history.Rows.Add();
                        int counter;
                        counter = THistory.Rows.Count > 1 ? THistory.Rows.Count - 1 : 0;
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (match[i].ToString().Contains(",") && match[i].ToString().Length > 16) // All compatible lines
                            {
                                history.Rows.Add();
                                THistory.Rows[counter].Cells["Symbol"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ':', 1), GetNth(match[i].ToString(), ',', 1) - 1 - GetNth(match[i].ToString(), ':', 1));
                                THistory.Rows[counter].Cells["Date"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 1), GetNth(match[i].ToString(), ',', 2) - 1 - GetNth(match[i].ToString(), ',', 1));
                                THistory.Rows[counter].Cells["Side"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 2), GetNth(match[i].ToString(), ',', 3) - 1 - GetNth(match[i].ToString(), ',', 2));
                                THistory.Rows[counter].Cells["Quantity"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 3), GetNth(match[i].ToString(), ',', 4) - 1 - GetNth(match[i].ToString(), ',', 3));
                                THistory.Rows[counter].Cells["Price"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 4), GetNth(match[i].ToString(), ',', 5) - 1 - GetNth(match[i].ToString(), ',', 4));
                                THistory.Rows[counter].Cells["Earnings"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 5), GetNth(match[i].ToString(), ',', 6) - 1 - GetNth(match[i].ToString(), ',', 5));
                                THistory.Rows[counter].Cells["Amount"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 6), GetNth(match[i].ToString(), ',', 7) - 1 - GetNth(match[i].ToString(), ',', 6));
                                THistory.Rows[counter].Cells["Earn"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 7), GetNth(match[i].ToString(), ',', 8) - 1 - GetNth(match[i].ToString(), ',', 7));
                                THistory.Rows[counter].Cells["EPS"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 8), GetNth(match[i].ToString(), ',', 9) - 1 - GetNth(match[i].ToString(), ',', 8));
                                THistory.Rows[counter].Cells["GainLoss"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 9), GetNth(match[i].ToString(), ',', 10) - 1 - GetNth(match[i].ToString(), ',', 9));
                                THistory.Rows[counter].Cells["Strategy"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 10), GetNth(match[i].ToString(), ',', 11) - 1 - GetNth(match[i].ToString(), ',', 10));
                                THistory.Rows[counter].Cells["MajorLevels"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 11), GetNth(match[i].ToString(), ',', 12) - 1 - GetNth(match[i].ToString(), ',', 11));
                                THistory.Rows[counter].Cells["Strengths"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 12), GetNth(match[i].ToString(), ',', 13) - 1 - GetNth(match[i].ToString(), ',', 12));
                                THistory.Rows[counter].Cells["Weaknesses"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 13), GetNth(match[i].ToString(), ',', 14) - 1 - GetNth(match[i].ToString(), ',', 13));
                                THistory.Rows[counter].Cells["Notes2"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 14), GetNth(match[i].ToString(), ',', 15) - 1 - GetNth(match[i].ToString(), ',', 14));
                                counter++;
                            }
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(watchPath + ".init.bak"))
                {
                    string fileContent = "";
                    using (StreamReader reader = new StreamReader(watchPath + ".init.bak"))
                    {
                        fileContent = reader.ReadToEnd();
                        MatchCollection match = Regex.Matches(fileContent, @":(.+?),"); // break file down into lines
                        watchList.Rows.Clear();
                        int counter = 0;
                        //counter = Watch.Rows.Count > 1 ? Watch.Rows.Count - 1 : 0;
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (match[i].ToString().Length > 3) // All compatible lines
                            {
                                watchList.Rows.Add();
                                Watch.Rows[counter].Cells["Column1"].Value = match[i].ToString().Substring(1, match[i].ToString().Length - 2);
                                counter++;
                            }
                        }
                        reader.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void aboutToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            aboutToolStripMenuItem.ForeColor = Color.White;
        }

        private void aboutToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            aboutToolStripMenuItem.ForeColor = Color.Black;
        }

        private void tradesToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            tradesToolStripMenuItem.ForeColor = Color.Black;
        }

        private void tradesToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            tradesToolStripMenuItem.ForeColor = Color.White;
        }

        private void stocksToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            stocksToolStripMenuItem.ForeColor = Color.Black;
        }

        private void watchlisttxtToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string entireList, filePath;
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                saveFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    filePath = saveFileDialog.FileName;
                    entireList = "";
                    for (int i = 0; i < Watch.RowCount - 1; i++)
                    {
                        entireList += ":" + Watch.Rows[i].Cells["Column1"].Value.ToString().Replace(",", ".") + ",\n";
                    }
                    File.WriteAllText(filePath, entireList);
                }
            }
        }

        private void activeTradestwToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                saveFileDialog.Filter = "TradeWin Active Trades File (*.atw)|*.atw|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    exportTWFile(false, true, saveFileDialog.FileName, Watch);
                }
            }
        }

        private void THistory_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (THistory.RowCount < 2)
                return;
            for (int i = 0; i < THistory.RowCount; i++)
            {
                float p = 0, ea = 0, q = 0;
                if (THistory.Rows[i].Cells[2].Value != null && THistory.Rows[i].Cells["Price"].Value != null && float.TryParse(THistory.Rows[i].Cells["Price"].Value.ToString(), out p) && THistory.Rows[i].Cells["Earnings"].Value != null && float.TryParse(THistory.Rows[i].Cells["Earnings"].Value.ToString(), out ea))
                {
                    if (p > ea)
                        THistory.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                    if (p < ea)
                        THistory.Rows[i].DefaultCellStyle.BackColor = Color.DarkSeaGreen;
                    if (p > ea && THistory.Rows[i].Cells[2].Value.ToString().Contains("Short"))
                        THistory.Rows[i].DefaultCellStyle.BackColor = Color.DarkSeaGreen;
                    if (p < ea && THistory.Rows[i].Cells[2].Value.ToString().Contains("Short"))
                        THistory.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                    THistory.Rows[i].Cells["EPS"].Value = Math.Round(Math.Abs(ea - p), 4);
                    if (THistory.Rows[i].Cells["Quantity"].Value != null && float.TryParse(THistory.Rows[i].Cells["Quantity"].Value.ToString(), out q))
                    {
                        THistory.Rows[i].Cells["Amount"].Value = (p * q).ToString();
                        THistory.Rows[i].Cells["Earn"].Value = (q * ea).ToString();
                        double GainLoss = Math.Abs((double)((p * q) - (ea * q)));
                        THistory.Rows[i].Cells["GainLoss"].Value = Math.Round(GainLoss, 2).ToString();
                    }
                    else
                        THistory.Rows[i].Cells["GainLoss"].Value = "0";
                }
                else
                    THistory.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
            }
            updateStrategy(THistory, IdentifiedStratTable);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (Watch.CurrentRow != null && Watch.CurrentRow.Index >= 0 && watchActive)
            {
                DataRow dr = watchList.NewRow();
                watchList.Rows.InsertAt(dr, Watch.CurrentRow.Index);
            }
            else if (THistory.CurrentRow != null && THistory.CurrentRow.Index >= 0 && !watchActive)
            {
                DataRow dr = history.NewRow();
                history.Rows.InsertAt(dr, THistory.CurrentRow.Index);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            if (Watch.CurrentRow != null && Watch.CurrentRow.Index >= 0 && watchActive)
            {
                DataRow dr = watchList.NewRow();
                watchList.Rows.InsertAt(dr, Watch.CurrentRow.Index + 1);
                for (int i = 0; i < Watch.Rows[Watch.CurrentRow.Index + 1].Cells.Count; i++)
                    Watch.Rows[Watch.CurrentRow.Index + 1].Cells[i].Value = Watch.Rows[Watch.CurrentRow.Index].Cells[i].Value;
            }
            else if (THistory.CurrentRow != null && THistory.CurrentRow.Index >= 0 && !watchActive)
            {
                DataRow dr = history.NewRow();
                history.Rows.InsertAt(dr, THistory.CurrentRow.Index + 1);
                for (int i = 0; i < THistory.Rows[THistory.CurrentRow.Index + 1].Cells.Count; i++)
                    THistory.Rows[THistory.CurrentRow.Index + 1].Cells[i].Value = THistory.Rows[THistory.CurrentRow.Index].Cells[i].Value;
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (Watch.CurrentRow != null && Watch.CurrentRow.Index >= 0 && watchActive)
                Watch.Rows.RemoveAt(Watch.CurrentRow.Index);
            else if (THistory.CurrentRow != null && THistory.CurrentRow.Index >= 0 && !watchActive)
                THistory.Rows.RemoveAt(THistory.CurrentRow.Index);
        }

        const int BYTES_TO_READ = sizeof(Int64);

        static bool FilesAreEqual(FileInfo first, FileInfo second)
        {
            if (first.Length != second.Length)
                return false;

            if (string.Equals(first.FullName, second.FullName, StringComparison.OrdinalIgnoreCase))
                return true;

            int iterations = (int)Math.Ceiling((double)first.Length / BYTES_TO_READ);

            using (FileStream fs1 = first.OpenRead())
            using (FileStream fs2 = second.OpenRead())
            {
                byte[] one = new byte[BYTES_TO_READ];
                byte[] two = new byte[BYTES_TO_READ];

                for (int i = 0; i < iterations; i++)
                {
                    fs1.Read(one, 0, BYTES_TO_READ);
                    fs2.Read(two, 0, BYTES_TO_READ);
                    if (BitConverter.ToInt64(one, 0) != BitConverter.ToInt64(two, 0))
                        return false;
                }
            }

            return true;
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var client = new WebClient())
            {
                try
                {
                    client.DownloadFile("https://github.com/DarkFlare69/TradeWin/raw/main/TradeTracker/bin/Debug/TradeWin.exe", "TradeWin-update.exe");
                }
                catch
                {
                    if (File.Exists(Directory.GetCurrentDirectory() + "\\TradeWin-update.exe")) // do this in Form1_Load too
                        File.Delete(Directory.GetCurrentDirectory() + "\\TradeWin-update.exe");
                    MessageBox.Show("Unable to retrieve update file! Please check your Internet connection.", "TradeWin Updater");
                    return;
                }
                if (File.Exists(Process.GetCurrentProcess().MainModule.FileName + ".bak")) // do this in Form1_Load too
                    File.Delete(Process.GetCurrentProcess().MainModule.FileName + ".bak");
                if (!FilesAreEqual(new FileInfo(Process.GetCurrentProcess().MainModule.FileName), new FileInfo(Directory.GetCurrentDirectory() + "\\TradeWin-update.exe")))
                {
                    File.Move(Process.GetCurrentProcess().MainModule.FileName, Process.GetCurrentProcess().MainModule.FileName + ".bak");
                    File.Move(Directory.GetCurrentDirectory() + "\\TradeWin-update.exe", Process.GetCurrentProcess().MainModule.FileName);
                    MessageBox.Show("The latest update has been downloaded! The application will restart now.", "TradeWin Updater");
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    Close();
                }
                else
                {
                    MessageBox.Show("You are currently using the last version of this software. Release: " + versionString, "TradeWin Updater");
                    File.Delete(Directory.GetCurrentDirectory() + "\\TradeWin-update.exe");
                }
            }
        }

        public int calculateStrat()
        {
            double quantity = 0, winRatio = 0, avgWin = 0, avgLoss = 0, tradeCount = 0, totalWins = 0, totalLosses = 0, endValue = 0;
            if (double.TryParse(textBox5.Text, out tradeCount))
            {
                if (double.TryParse(textBox1.Text, out quantity))
                {
                    textBox9.Text = (tradeCount * quantity).ToString();
                }
                if (double.TryParse(textBox2.Text, out winRatio))
                {
                    totalWins = Math.Floor(tradeCount / (1 + winRatio) * winRatio);
                    totalLosses = tradeCount - totalWins;
                    textBox7.Text = totalWins.ToString();
                    textBox6.Text = totalLosses.ToString();
                    if (double.TryParse(textBox3.Text, out avgWin))
                    {
                        if (double.TryParse(textBox4.Text, out avgLoss) && quantity > 0 && winRatio > 0 && avgWin > 0 && avgLoss > 0 && tradeCount > 0 && totalWins > 0 && totalLosses > 0)
                        {
                            endValue = totalWins * (avgWin * quantity);
                            endValue = endValue - (totalLosses * avgLoss * quantity);
                            textBox8.Text = "Net Earnings: $" + Math.Round(endValue, 2).ToString() + Environment.NewLine + "Average Earnings Per Trade: $" + Math.Round((endValue / tradeCount), 3).ToString();
                            return 2;
                        }
                    }
                }
            }
            return 0;
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }


        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (calculateStrat() == 2 && loaded)
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\stratcalc.csv", "," + textBox1.Text + ",," + textBox2.Text + ",," + textBox3.Text + ",," + textBox4.Text + ",," + textBox5.Text + ",");

        }

        private void button2_Click(object sender, EventArgs e)
        {
            IdentifiedStratTable.Rows.Clear();
        }

        private void button15_Click(object sender, EventArgs e)
        {
            StratLogPanel.Rows.Clear();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            IdentifiedStratTable.Rows.Clear();
            StratLogPanel.Rows.Clear();
            textBox1.Text = textBox2.Text = textBox3.Text = textBox4.Text = textBox5.Text = "";
        }

        private void IdentifiedStratTable_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (IdentifiedStratTable.CurrentCell != null && IdentifiedStratTable.CurrentCell.RowIndex >= 0 && IdentifiedStratTable.CurrentCell.ColumnIndex == 0)
            {
                Form4 form4 = new Form4(this);
                _ = form4.ShowDialog();
            }
        }

        private void IdentifiedStratTable_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            exportTWFile(true, true, strategyPath, IdentifiedStratTable); // exporting the file should work, now just import it on Form1 load before history
            if (loaded)
                updateStrategy(THistory, IdentifiedStratTable);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            IdentifiedStratTable.Rows.Add();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DataGridView dgv = IdentifiedStratTable;
            try
            {
                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                if (rowIndex == 0)
                    return;
                // get index of the column for the selected cell
                int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                DataGridViewRow selectedRow = dgv.Rows[rowIndex];
                dgv.Rows.Remove(selectedRow);
                dgv.Rows.Insert(rowIndex - 1, selectedRow);
                dgv.ClearSelection();
                dgv.Rows[rowIndex - 1].Cells[colIndex].Selected = true;
            }
            catch { }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            DataGridView dgv = IdentifiedStratTable;
            try
            {
                int totalRows = dgv.Rows.Count;
                // get index of the row for the selected cell
                int rowIndex = dgv.SelectedCells[0].OwningRow.Index;
                if (rowIndex == totalRows - 1)
                    return;
                // get index of the column for the selected cell
                int colIndex = dgv.SelectedCells[0].OwningColumn.Index;
                DataGridViewRow selectedRow = dgv.Rows[rowIndex];
                dgv.Rows.Remove(selectedRow);
                dgv.Rows.Insert(rowIndex + 1, selectedRow);
                dgv.ClearSelection();
                dgv.Rows[rowIndex + 1].Cells[colIndex].Selected = true;
            }
            catch { }
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            Form5 form5 = new Form5(this);
            _ = form5.ShowDialog();
        }

        private void button18_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Clear trading calender?", "Confirm", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                MessageBox.Show("Calender cleared!");
                if (File.Exists(calenderPath))
                {
                    File.Copy(calenderPath, calenderPath + ".bak", true);
                    File.Delete(calenderPath);
                }
            }
        }

        private void miscToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            miscToolStripMenuItem.ForeColor = Color.White;
        }

        private void miscToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            miscToolStripMenuItem.ForeColor = Color.Black;
        }

        private void button19_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Documents";
                saveFileDialog.Filter = "Text File (*.txt)|*.txt|All files (*.*)|*.*";
                saveFileDialog.RestoreDirectory = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //exportTWFile(false, true, saveFileDialog.FileName, THistory);
                    File.Copy(calenderPath, saveFileDialog.FileName);
                }
            }
        }

        private void backupCalenderToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button20_Click(object sender, EventArgs e)
        {
            Form6 form6 = new Form6(this);
            _ = form6.ShowDialog();
        }

        private void stocksToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            stocksToolStripMenuItem.ForeColor = Color.White;
        }

        private void fileToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            fileToolStripMenuItem.ForeColor = Color.Black;
        }

        private void fileToolStripMenuItem_DropDownClosed(object sender, EventArgs e)
        {
            fileToolStripMenuItem.ForeColor = Color.White;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(historyPath + ".init.bak"))
                {
                    string fileContent = "";
                    using (StreamReader reader = new StreamReader(historyPath + ".init.bak"))
                    {
                        fileContent = reader.ReadToEnd();
                        MatchCollection match = Regex.Matches(fileContent, @":(.+?),"); // break file down into lines
                        //THistory.DataSource = null;
                        int counter;
                        counter = THistory.Rows.Count > 1 ? THistory.Rows.Count - 1 : 0;
                        for (int i = 0; i < match.Count; i++)
                            if (match[i].ToString().Contains(",") && match[i].ToString().Length > 2) // All compatible lines
                            {
                                THistory.Rows.Add();
                                THistory.Rows[counter].Cells["Column1"].Value = match[i].ToString().Substring(1, match[i].ToString().Length - 2);
                                counter++;
                            }
                        reader.Close();
                    }
                }
            }
            catch { MessageBox.Show("Backup file missing at: " + historyPath + ".init.bak"); }
        }

        private void THistory_Enter(object sender, EventArgs e)
        {
            watchActive = false;
        }

        private void THistory_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            exportTWFile(true, autoSaveTHistory, historyPath, THistory);
            updateStrategy(THistory, IdentifiedStratTable);
        }
        private void Watch_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            exportTWFile(true, autoSaveWatch, watchPath, Watch);
        }
        private void Watch_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            exportTWFile(true, autoSaveWatch, watchPath, Watch);
        }

        public void updateStrategy(DataGridView historyGrid, DataGridView strategyGrid)
        {
            if (checkBox1.Checked && loaded)
            {
                StratLogPanel.Rows.Clear();
                int[] winCount = new int[strategyGrid.RowCount], lossCount = new int[strategyGrid.RowCount];
                double[] GainLOSS = new double[strategyGrid.RowCount], GainAmount = new double[strategyGrid.RowCount], LossAmount = new double[strategyGrid.RowCount];
                int stratCount = 0;
                string[] keywords = new string[strategyGrid.RowCount], exclusions = new string[strategyGrid.RowCount], positionType = new string[strategyGrid.RowCount];
                foreach (DataGridViewRow stratRow in strategyGrid.Rows) // loop through each strategy row
                {
                    if (stratRow.Cells["Column5"].Value != null) // make sure its a strategy
                    {
                        if (stratRow.Cells["Keywords"].Value != null) // check the keywords cell of the row
                        {
                            if (stratRow.Cells["Keywords"].Value.ToString() != "")
                            {
                                //MatchCollection keys = Regex.Matches(stratRow.Cells["Keywords"].Value.ToString(), @"(.+?),"); // break keywords down into comma sections
                                keywords[stratCount] = "," + stratRow.Cells["Keywords"].Value.ToString().Replace(",", ",,") + ","; // keywords[currentstrategy] will be all the keywords with commas
                            }
                        }
                        if (stratRow.Cells["Exclusions"].Value != null)
                        {
                            if (stratRow.Cells["Exclusions"].Value.ToString() != "")
                            {
                                //MatchCollection exclusions = Regex.Matches(stratRow.Cells["Exclusions"].Value.ToString(), @"(.+?),"); // break keywords down into comma sections
                                exclusions[stratCount] = "," + stratRow.Cells["Exclusions"].Value.ToString().Replace(",", ",,") + ",";
                                //exclusions[strat] = exclusions[strat];
                            }
                        }
                        if (stratRow.Cells["AddNewTradesFrom"].Value != null)
                        {
                            if (stratRow.Cells["AddNewTradesFrom"].Value.ToString() != "")
                            {
                                //MatchCollection exclusions = Regex.Matches(stratRow.Cells["Exclusions"].Value.ToString(), @"(.+?),"); // break keywords down into comma sections
                                positionType[stratCount] = stratRow.Cells["AddNewTradesFrom"].Value.ToString();
                            }
                        }
                        stratCount++;
                    }
                }
                foreach (DataGridViewRow historyrow in historyGrid.Rows) // loop through each history row
                {
                    string searchString = "";
                    bool exclusionFound = false;
                    if (historyrow.Cells["Symbol"].Value != null) // get the row data into searchString
                    {
                        searchString += historyrow.Cells["Strategy"].Value == null ? "" : historyrow.Cells["Strategy"].Value.ToString().ToLower();
                        searchString += historyrow.Cells["Notes2"].Value == null ? "" : historyrow.Cells["Notes2"].Value.ToString().ToLower();
                        searchString += historyrow.Cells["Side"].Value == null ? "" : historyrow.Cells["Side"].Value.ToString().ToLower();
                        searchString += historyrow.Cells["Strengths"].Value == null ? "" : historyrow.Cells["Strengths"].Value.ToString().ToLower();
                        searchString += historyrow.Cells["Weaknesses"].Value == null ? "" : historyrow.Cells["Weaknesses"].Value.ToString().ToLower();
                        searchString += historyrow.Cells["Symbol"].Value == null ? "" : historyrow.Cells["Symbol"].Value.ToString().ToLower();
                    }
                    for (int strat = 0; strat < stratCount; strat++) // loop through each strategy
                    {
                        MatchCollection exclusion = null;
                        MatchCollection key = null;
                        if (exclusions[strat] != ",," && exclusions[strat] != null)
                            exclusion = Regex.Matches(exclusions[strat], @",(.+?),"); // break keywords down into comma sections
                        if (keywords[strat] != ",," && keywords[strat] != null)
                            key = Regex.Matches(keywords[strat], @",(.+?),"); // break keywords down into comma sections
                        //string position = positionType[strat];
                        for (int k = 0; k < (key != null ? key.Count : 0); k++) // loop through each keyword
                        {
                            if (searchString.Contains(key[k].ToString().Substring(1, key[k].Length - 2))) // if one of the keywords is found in the row
                            {
                                for (int ex = 0; ex < (exclusion != null ? exclusion.Count : 0); ex++)
                                {
                                    if (searchString.Contains(exclusion[ex].ToString().Substring(1, exclusion[ex].Length - 2))) // if any of the exclusions are found, set the exclsuion to true
                                    {
                                        exclusionFound = true;
                                    }
                                }
                                if (!exclusionFound) // if no exclusions are found, check price and stuff
                                {
                                    if (historyrow.Cells["Price"].Value != null && historyrow.Cells["Earnings"].Value != null)
                                    {
                                        double tmp1, tmp2;
                                        if (double.TryParse(historyrow.Cells["Price"].Value.ToString(), out tmp1) && double.TryParse(historyrow.Cells["Earnings"].Value.ToString(), out tmp2))
                                        {
                                            if (historyrow.Cells["Side"].Value != null)
                                            {
                                                if (historyrow.Cells["Side"].Value.ToString().Contains("Short") && positionType[strat].Contains("Short"))
                                                {
                                                    if (tmp2 < tmp1)
                                                    {
                                                        GainLOSS[strat] += double.Parse(historyrow.Cells["GainLoss"].Value.ToString());
                                                        GainAmount[strat] += double.Parse(historyrow.Cells["GainLoss"].Value.ToString());
                                                        winCount[strat]++;
                                                    }
                                                    else if (tmp2 > tmp1)
                                                    {
                                                        GainLOSS[strat] -= double.Parse(historyrow.Cells["GainLoss"].Value.ToString());
                                                        LossAmount[strat] += double.Parse(historyrow.Cells["GainLoss"].Value.ToString());
                                                        lossCount[strat]++;
                                                    }
                                                    searchString = "1done";
                                                    continue;
                                                }
                                                else if (!historyrow.Cells["Side"].Value.ToString().Contains("Short") && positionType[strat].Contains("Long"))
                                                {
                                                    if (tmp2 > tmp1)
                                                    {
                                                        GainLOSS[strat] += double.Parse(historyrow.Cells["GainLoss"].Value.ToString());
                                                        GainAmount[strat] += double.Parse(historyrow.Cells["GainLoss"].Value.ToString());
                                                        winCount[strat]++;
                                                    }
                                                    else if (tmp2 < tmp1)
                                                    {
                                                        GainLOSS[strat] -= double.Parse(historyrow.Cells["GainLoss"].Value.ToString());
                                                        LossAmount[strat] += double.Parse(historyrow.Cells["GainLoss"].Value.ToString());
                                                        lossCount[strat]++;
                                                    }
                                                    searchString = "1done";
                                                    continue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (searchString == "1done")
                            break;
                    }
                }
                //StratLogPanel.Rows.Add();
                int curStrat = 0;
                for (int i = 0; i < stratCount; i++)
                {
                    double loss = 1, gain = 1, ratio = 0, actual = 0, rrTarget = 0, rrActual = 0;
                    
                    if (winCount[i] + lossCount[i] > 0)
                    {
                        StratLogPanel.Rows.Add();
                        StratLogPanel.Rows[curStrat].Cells["StratName"].Value = IdentifiedStratTable.Rows[i].Cells["Column5"].Value + "\r\nTrades: " + (winCount[i] + lossCount[i]);
                        StratLogPanel.Rows[curStrat].Cells["Wins"].Value = "Target: " + (double.TryParse(IdentifiedStratTable.Rows[i].Cells["Target"].Value != null ? IdentifiedStratTable.Rows[i].Cells["Target"].Value.ToString() : "0", out ratio) ? ratio.ToString() : "0") + "/1\r\nActual: " + (lossCount[i] > 0 ? actual = (double)winCount[i] / lossCount[i] : actual = winCount[i]) + "/1\r\nWins: " + winCount[i] + " Losses: " + lossCount[i];
                        double.TryParse(IdentifiedStratTable.Rows[i].Cells["AvgLoss2"].Value != null ? IdentifiedStratTable.Rows[i].Cells["AvgLoss2"].Value.ToString() : "1", out loss);
                        double.TryParse(IdentifiedStratTable.Rows[i].Cells["AvgWin"].Value != null ? IdentifiedStratTable.Rows[i].Cells["AvgWin"].Value.ToString() : "1", out gain);
                        StratLogPanel.Rows[curStrat].Cells["AvgLoss"].Value = "Target: " + (rrTarget = Math.Round((gain / loss), 3)) + "/1\r\nActual: " + (rrActual = Math.Round(GainAmount[i] / (LossAmount[i] != 0 ? LossAmount[i] : 1), 4)) + "/1";
                        StratLogPanel.Rows[curStrat].Cells["GainLosses"].Value = "Total G/L: $" + GainLOSS[i] + "\r\nGains: $" + GainAmount[i] + "\r\nLosses: $" + LossAmount[i] + "\r\nAvg G/L: $" + (GainLOSS[i] / ((winCount[i] + lossCount[i]) != 0 ? (winCount[i] + lossCount[i]) : 1));
                        if (rrActual > rrTarget)
                            StratLogPanel.Rows[curStrat].Cells["AvgLoss"].Style.BackColor = Color.DarkSeaGreen;
                        else if (rrActual < rrTarget)
                            StratLogPanel.Rows[curStrat].Cells["AvgLoss"].Style.BackColor = Color.LightCoral;
                        if (ratio > actual)
                            StratLogPanel.Rows[curStrat].Cells["Wins"].Style.BackColor = Color.LightCoral;
                        else if (ratio < actual)
                            StratLogPanel.Rows[curStrat].Cells["Wins"].Style.BackColor = Color.DarkSeaGreen;
                        if (GainAmount[i] > LossAmount[i])
                            StratLogPanel.Rows[curStrat].Cells["GainLosses"].Style.BackColor = Color.DarkSeaGreen;
                        else if (GainAmount[i] < LossAmount[i])
                            StratLogPanel.Rows[curStrat].Cells["GainLosses"].Style.BackColor = Color.LightCoral;
                        curStrat++;
                    }
                }
            }
        }
        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TradeWin is a free, simple daytrade tracking tool written in Visual C#. The goal of this program is to provide a free and robust platform for daytraders to prepare for setups, log their trades, and track performance statistics. Logging trades helps with performance monitoring, and this program aims to provide a platform to store trades in an offline file format. Users are expected to only use this tool for inputting and reading personal trade data, not for making trading decisions.\n\nTradeWin will always be provided 100% free. Other trading journals will charge you a monthly subscription fee, usually more than $20/m, and withhold important performance metrics from you on free plans. If you would like to see continuous improvements, please consider donating.\nPayPal: adamgames69@gmail.com\nCashApp: $Adam129111", "About TradeWin " + versionString);
        }
    }
}
