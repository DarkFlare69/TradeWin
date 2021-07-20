using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TradeTracker
{
    public partial class Form1 : Form
    {
        public DataGridView exportGrid = new DataGridView();
        static DataTable watchList = new DataTable(), history = new DataTable();
        public static bool watchActive, autoSaveWatch, autoSaveTHistory, perSale = false, perShare = false, perDollar = false, loaded = false;
        static string versionString = "v1.0", watchPath, historyPath;
        public float[] commissions = new float[3];
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\dev");
            string settingPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\settings.bin";
            watchPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin";
            historyPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin";
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

                    }
                    else if (test[0] == 1) // use dev directory
                    {
                        watchPath += "\\dev\\watchlist.atw";
                        historyPath += "\\dev\\history.tw";
                    }
                }
            }
            catch (Exception ex)
            {
                watchPath += "\\watchlist.atw";
                historyPath += "\\history.tw";
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
            if (!File.Exists(settingPath))
            {
                try
                {
                    using (FileStream fs = File.Create(settingPath))
                    {
                        byte[] settings = { 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                        fs.Write(settings, 0, 20);
                    }
                }
                catch (Exception ex){}
                MessageBox.Show("TradeWin is designed to be a free, open source trade logging application. This allows importing trade history from Fidelity, exporting/loading from TradeWin proprietary format (.tw) and more!\n\nTraders are intended to make trading decisions based on their own sources. TradeWin is simply an additional tool to keep for traders to keep an eye on their daytrades and log performance on a free, offline platform.\n\nNotable features include auto-saving and auto-loading the watchlist/history (configurable in settings), and importing/exporting to multiple file formats. Most other necessary basic features are available.", "Welcome to TradeWin! - " + versionString);
            }
            if (File.Exists(settingPath))
            {
                using (BinaryReader fileStream = new BinaryReader(File.Open(settingPath, FileMode.Open)))
                {
                    for (int i = 0; i < 11; i++)
                    {
                        if (i == 0 && fileStream.ReadByte() == 1) // Import Watchlist
                        {
                            if (File.Exists(watchPath))
                            {
                                string line;
                                int counter = 0;
                                StreamReader file = new System.IO.StreamReader(watchPath);
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (line.Length < 16)
                                        continue;
                                    if (line.IndexOf(',') - line.IndexOf(':') > 1)
                                    {
                                        //DataRow dr = watchList.NewRow();
                                        //dr["RowCount"] = "";
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
                                StreamReader file = new System.IO.StreamReader(historyPath);
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (line.Length < 16 || !(line.IndexOf(',') - line.IndexOf(':') > 1))
                                        continue;
                                    //DataRow dr = history.NewRow();
                                    //dr["RowCount"] = "";
                                    history.Rows.Add();
                                    THistory.Rows[counter].Cells["Symbol"].Value = line.Substring(line.IndexOf(':') + 1, GetNth(line, ',', 1) - line.IndexOf(':') - 2); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Date"].Value = line.Substring(GetNth(line, ',', 1), GetNth(line, ',', 2) - GetNth(line, ',', 1) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Side"].Value = line.Substring(GetNth(line, ',', 2), GetNth(line, ',', 3) - GetNth(line, ',', 2) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Quantity"].Value = line.Substring(GetNth(line, ',', 3), GetNth(line, ',', 4) - GetNth(line, ',', 3) - 1); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Price"].Value = line.Substring(GetNth(line, ',', 4), GetNth(line, ',', 5) - GetNth(line, ',', 4) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Earnings"].Value = line.Substring(GetNth(line, ',', 5), GetNth(line, ',', 6) - GetNth(line, ',', 5) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Amount"].Value = line.Substring(GetNth(line, ',', 6), GetNth(line, ',', 7) - GetNth(line, ',', 6) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Earn"].Value = line.Substring(GetNth(line, ',', 7), GetNth(line, ',', 8) - GetNth(line, ',', 7) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["GainLoss"].Value = line.Substring(GetNth(line, ',', 8), GetNth(line, ',', 9) - GetNth(line, ',', 8) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Strategy"].Value = line.Substring(GetNth(line, ',', 9), GetNth(line, ',', 10) - GetNth(line, ',', 9) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["MajorLevels"].Value = line.Substring(GetNth(line, ',', 10), GetNth(line, ',', 11) - GetNth(line, ',', 10) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Strengths"].Value = line.Substring(GetNth(line, ',', 11), GetNth(line, ',', 12) - GetNth(line, ',', 11) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Weaknesses"].Value = line.Substring(GetNth(line, ',', 12), GetNth(line, ',', 13) - GetNth(line, ',', 12) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    THistory.Rows[counter].Cells["Notes2"].Value = line.Substring(GetNth(line, ',', 13), GetNth(line, ',', 14) - GetNth(line, ',', 13) - 1).Replace("~~~", "\r\n"); // populate each cell in row here, THIS DOESNT WORK RN
                                    counter++;
                                    //this.CoWaitForMultipleHandles();
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
                        if (i == 7 && fileStream.ReadByte() == 1) {}
                        if (i == 8)
                            commissions[0] = fileStream.ReadSingle();
                        if (i == 9)
                            commissions[1] = fileStream.ReadSingle();
                        if (i == 10)
                            commissions[2] = fileStream.ReadSingle();
                    }
                }
            }
            loaded = true;
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

                    //Read the contents of the file into a stream
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
                            if (match[i].ToString().Contains(",") && match[i].ToString().Length > 15) // All compatible lines
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
                                THistory.Rows[counter].Cells["GainLoss"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 8), GetNth(match[i].ToString(), ',', 9) - 1 - GetNth(match[i].ToString(), ',', 8));
                                THistory.Rows[counter].Cells["Strategy"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 9), GetNth(match[i].ToString(), ',', 10) - 1 - GetNth(match[i].ToString(), ',', 9));
                                THistory.Rows[counter].Cells["MajorLevels"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 10), GetNth(match[i].ToString(), ',', 11) - 1 - GetNth(match[i].ToString(), ',', 10));
                                THistory.Rows[counter].Cells["Strengths"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 11), GetNth(match[i].ToString(), ',', 12) - 1 - GetNth(match[i].ToString(), ',', 11));
                                THistory.Rows[counter].Cells["Weaknesses"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 12), GetNth(match[i].ToString(), ',', 13) - 1 - GetNth(match[i].ToString(), ',', 12));
                                THistory.Rows[counter].Cells["Notes2"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 13), GetNth(match[i].ToString(), ',', 14) - 1 - GetNth(match[i].ToString(), ',', 13));
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

                    //Read the contents of the file into a stream
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

                    //Read the contents of the file into a stream
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
            {
                Watch.Rows.Clear();
            }
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
            {
                THistory.Rows.Clear();
            }
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
                //THistory.Rows.Insert(0, new string[13]);
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
            catch (Exception ex)
            {
                //MessageBox.Show("First try: " + ex.ToString());
            }
            try
            {
                Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin");
                if (str.Length > 4)
                    File.WriteAllText(path2, str);
            }
            catch (Exception ex)
            {
                //MessageBox.Show("Second try: " + ex.ToString());
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
                                                                                        //THistory.DataSource = null;
                        watchList.Rows.Clear();
                        //watchList.Rows.Add();
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
                            if (match[i].ToString().Contains(",") && match[i].ToString().Length > 15) // All compatible lines
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
                                THistory.Rows[counter].Cells["GainLoss"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 8), GetNth(match[i].ToString(), ',', 9) - 1 - GetNth(match[i].ToString(), ',', 8));
                                THistory.Rows[counter].Cells["Strategy"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 9), GetNth(match[i].ToString(), ',', 10) - 1 - GetNth(match[i].ToString(), ',', 9));
                                THistory.Rows[counter].Cells["MajorLevels"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 10), GetNth(match[i].ToString(), ',', 11) - 1 - GetNth(match[i].ToString(), ',', 10));
                                THistory.Rows[counter].Cells["Strengths"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 11), GetNth(match[i].ToString(), ',', 12) - 1 - GetNth(match[i].ToString(), ',', 11));
                                THistory.Rows[counter].Cells["Weaknesses"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 12), GetNth(match[i].ToString(), ',', 13) - 1 - GetNth(match[i].ToString(), ',', 12));
                                THistory.Rows[counter].Cells["Notes2"].Value = match[i].ToString().Substring(GetNth(match[i].ToString(), ',', 13), GetNth(match[i].ToString(), ',', 14) - 1 - GetNth(match[i].ToString(), ',', 13));
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
                                                                                        //THistory.DataSource = null;
                        watchList.Rows.Clear();
                        //watchList.Rows.Add();
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
                    if (p < ea && !THistory.Rows[i].Cells[2].Value.ToString().Contains("Short"))
                    {
                        THistory.Rows[i].DefaultCellStyle.BackColor = Color.DarkSeaGreen;
                    }
                    else if (p < ea && THistory.Rows[i].Cells[2].Value.ToString().Contains("Short"))
                    {
                        THistory.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                    if (p > ea && THistory.Rows[i].Cells[2].Value.ToString().Contains("Short"))
                    {
                        THistory.Rows[i].DefaultCellStyle.BackColor = Color.DarkSeaGreen;
                    }
                    else if (p > ea && !THistory.Rows[i].Cells[2].Value.ToString().Contains("Short"))
                    {
                        THistory.Rows[i].DefaultCellStyle.BackColor = Color.LightCoral;
                    }
                    //MessageBox.Show(THistory.Rows[i].Cells[2].Value.ToString());
                    if (THistory.Rows[i].Cells["Quantity"].Value != null && float.TryParse(THistory.Rows[i].Cells["Quantity"].Value.ToString(), out q))
                    {
                        THistory.Rows[i].Cells["Amount"].Value = (p * q).ToString();
                        THistory.Rows[i].Cells["Earn"].Value = (q * ea).ToString();
                        double GainLoss = -(double)((p * q) - (ea * q));
                        if (THistory.Rows[i].Cells[2].Value.ToString().Contains("Short"))
                        {
                            GainLoss = -GainLoss;
                        }
                        THistory.Rows[i].Cells["GainLoss"].Value = Math.Round(GainLoss, 2).ToString();

                    }
                    else
                    {
                        THistory.Rows[i].Cells["GainLoss"].Value = "0";
                    }
                }
                else
                {
                    THistory.Rows[i].DefaultCellStyle.BackColor = Color.LightGray;
                }
            }
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
                {
                    Watch.Rows[Watch.CurrentRow.Index + 1].Cells[i].Value = Watch.Rows[Watch.CurrentRow.Index].Cells[i].Value;
                }
            }
            else if (THistory.CurrentRow != null && THistory.CurrentRow.Index >= 0 && !watchActive)
            {
                DataRow dr = history.NewRow();
                history.Rows.InsertAt(dr, THistory.CurrentRow.Index + 1);
                for (int i = 0; i < THistory.Rows[THistory.CurrentRow.Index + 1].Cells.Count; i++)
                {
                    THistory.Rows[THistory.CurrentRow.Index + 1].Cells[i].Value = THistory.Rows[THistory.CurrentRow.Index].Cells[i].Value;
                }
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (Watch.CurrentRow != null && Watch.CurrentRow.Index >= 0 && watchActive)
            {
                Watch.Rows.RemoveAt(Watch.CurrentRow.Index);
            }
            else if (THistory.CurrentRow != null && THistory.CurrentRow.Index >= 0 && !watchActive)
            {
                THistory.Rows.RemoveAt(THistory.CurrentRow.Index);
            }
        }

        private void checkForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var client = new WebClient())
            {
                client.DownloadFile("https://github.com/DarkFlare69/Gateway-To-NTR-Converter/raw/master/Gateway%20To%20NTR%20Converter/bin/Debug/Gateway%20To%20NTR%20Converter.exe", "TradeWin-update.exe");
                if (File.Exists(Process.GetCurrentProcess().MainModule.FileName + ".bak")) // do this in Form1_Load too
                {
                    File.Delete(Process.GetCurrentProcess().MainModule.FileName + ".bak");
                }
                if (new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Length != new FileInfo(Directory.GetCurrentDirectory() + "\\TradeWin-update.exe").Length)
                {
                    File.Move(Process.GetCurrentProcess().MainModule.FileName, Process.GetCurrentProcess().MainModule.FileName + ".bak");
                    File.Move(Directory.GetCurrentDirectory() + "\\TradeWin-update.exe", Process.GetCurrentProcess().MainModule.FileName);
                    MessageBox.Show("The latest update has been downloaded! The application will restart now.", "TradeWin Updater");
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    Close();
                }
                else if (new FileInfo(Process.GetCurrentProcess().MainModule.FileName).Length == new FileInfo(Directory.GetCurrentDirectory() + "\\TradeWin-update.exe").Length)
                {
                    MessageBox.Show("You are currently using the latest version of this software. Release: " + versionString, "TradeWin Updater");
                }
            }
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
                        counter = Watch.Rows.Count > 1 ? Watch.Rows.Count - 1 : 0;
                        for (int i = 0; i < match.Count; i++)
                        {
                            if (match[i].ToString().Contains(",") && match[i].ToString().Length > 2) // All compatible lines
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
                MessageBox.Show("Backup file missing at: " + historyPath + ".init.bak");
            }
        }

        private void THistory_Enter(object sender, EventArgs e)
        {
            watchActive = false;
        }

        private void THistory_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            exportTWFile(true, autoSaveTHistory, historyPath, THistory);
        }
        private void Watch_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            exportTWFile(true, autoSaveWatch, watchPath, Watch);
        }
        private void Watch_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            exportTWFile(true, autoSaveWatch, watchPath, Watch);
        }

        private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("TradeWin is a free, simple daytrade tracking tool written in Visual C#. The goal of this program is to provide a platform for daytraders to prepare for setups, log their trades, and import trading history. Logging trades helps with performance monitoring, and this program aims to provide a platform to store trades in an offline file format. Users are expected to only use this tool for inputting and reading personal trade data, not for making trading decisions.", "About TradeWin " + versionString);
        }
    }
}
