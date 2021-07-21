using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace TradeTracker
{
    public partial class Form3 : Form
    {
        Form1 _form1Instance;
        public Form3(Form1 form1Instance)
        {
            this._form1Instance = form1Instance;
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin");
            string basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\settings.bin";

            if (File.Exists(basePath))
            {
                /*using (FileStream fileStream = new FileStream(basePath, FileMode.Open))
                {
                    for (int i = 0; i < fileStream.Length; i++)
                    {
                        if (i == 0 && fileStream.ReadByte() == 0) // Import Watchlist
                            checkBox1.Checked = false;
                        if (i == 1 && fileStream.ReadByte() == 0) // Import Trade History
                            checkBox2.Checked = false;
                        if (i == 2 && fileStream.ReadByte() == 0) // AutoSave Watchlist
                            checkBox3.Checked = false;
                        if (i == 3 && fileStream.ReadByte() == 0) // AutoSave History
                            checkBox4.Checked = false;
                        if (i == 4 && fileStream.ReadByte() == 1)
                            checkBox5.Checked = true;
                        if (i == 5 && fileStream.ReadByte() == 1)
                            checkBox6.Checked = true;
                        if (i == 6 && fileStream.ReadByte() == 1)
                            checkBox7.Checked = true;
                    }
                }*/
                using (BinaryReader br = new BinaryReader(File.Open(basePath, FileMode.Open)))
                {
                    for (int i = 0; i < 11; i++)
                    {
                        //float[] floats = new float[3];
                        //byte[] floatConstructor = new byte[4];
                        if (i == 0 && br.ReadByte() == 0) // Import Watchlist
                            checkBox1.Checked = false;
                        if (i == 1 && br.ReadByte() == 0) // Import Trade History
                            checkBox2.Checked = false;
                        if (i == 2 && br.ReadByte() == 0) // AutoSave Watchlist
                            checkBox3.Checked = false;
                        if (i == 3 && br.ReadByte() == 0) // AutoSave History
                            checkBox4.Checked = false;
                        if (i == 4 && br.ReadByte() == 1) // Use Per Sale $
                            checkBox5.Checked = true;
                        if (i == 5 && br.ReadByte() == 1) // Use Per Share $
                            checkBox6.Checked = true;
                        if (i == 6 && br.ReadByte() == 1) // Use Per Dollar $
                            checkBox7.Checked = true;
                        if (i == 7 && br.ReadByte() == 1) // Use Dev Directory
                            checkBox9.Checked = true;
                        if (i == 8)
                            _form1Instance.commissions[0] = br.ReadSingle();
                        if (i == 9)
                            _form1Instance.commissions[1] = br.ReadSingle();
                        if (i == 10)
                            _form1Instance.commissions[2] = br.ReadSingle();
                    }
                    textBox1.Text = _form1Instance.commissions[0].ToString();
                    textBox4.Text = _form1Instance.commissions[1].ToString();
                    textBox3.Text = _form1Instance.commissions[2].ToString();
                    textBox5.Text = _form1Instance.commissions[3].ToString();
                    if (_form1Instance.commissions[3] > 0)
                    {
                        checkBox10.Checked = true;
                    }
                }
            }
        }
        public static UInt32 ReverseBytes(UInt32 value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin"))
                Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin", true);
        }

        public byte[] NewByteArray(byte[] arr1, byte[] arr2)
        {
            byte[] newArray = new byte[arr1.Length + arr2.Length];
            for (int i = 0; i < arr1.Length; i++) // add all of arr1 first
            {
                newArray[i] = arr1[i];
            }
            for (int j = 0; j < arr2.Length; j++)
            {
                newArray[arr1.Length + j] = arr2[j];
            }
            return newArray;
        }

        public void writeSettings()
        {
            try
            {
                using (FileStream fs = File.Create(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\TradeWin\\settings.bin"))
                {
                    Form1.autoSaveTHistory = checkBox3.Checked;
                    Form1.autoSaveWatch = checkBox4.Checked;
                    Form1.perSale = checkBox5.Checked;
                    Form1.perShare = checkBox6.Checked;
                    Form1.perDollar = checkBox7.Checked;
                    Form1.autoSaveWatch = checkBox4.Checked;
                    float[] floats = new float[4] { 0, 0, 0, 0 };
                    if (textBox1.Text != "" && float.TryParse(textBox1.Text, out floats[0]))
                    {
                        _form1Instance.commissions[0] = floats[0];
                    }
                    if (textBox4.Text != "" && float.TryParse(textBox4.Text, out floats[1]))
                    {
                        _form1Instance.commissions[1] = floats[1];
                    }
                    if (textBox3.Text != "" && float.TryParse(textBox3.Text, out floats[2]))
                    {
                        _form1Instance.commissions[2] = floats[2];
                    }
                    if (textBox5.Text != "" && float.TryParse(textBox5.Text, out floats[3]))
                    {
                        _form1Instance.commissions[3] = floats[3];
                    }
                    if (!checkBox10.Checked)
                    {
                        _form1Instance.commissions[3] = 0;
                    }
                    byte[] floatByteArr = new byte[floats.Length * 4];
                    Buffer.BlockCopy(floats, 0, floatByteArr, 0, floatByteArr.Length);
                    byte[] settings = new byte[8] { Convert.ToByte(checkBox1.Checked), Convert.ToByte(checkBox2.Checked), Convert.ToByte(checkBox3.Checked), Convert.ToByte(checkBox4.Checked), Convert.ToByte(checkBox5.Checked), Convert.ToByte(checkBox6.Checked), Convert.ToByte(checkBox7.Checked), Convert.ToByte(checkBox9.Checked) };
                    //settings[BitConverter.GetBytes(perSale);
                    byte[] newArr = new byte[24];
                    newArr = NewByteArray(settings, floatByteArr);
                    fs.Write(newArr, 0, 24);
                }
            }
            catch {}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = checkBox2.Checked = checkBox3.Checked = checkBox4.Checked = true;
            checkBox5.Checked = checkBox6.Checked = checkBox7.Checked = checkBox9.Checked =  false;
            textBox1.Text = textBox4.Text = textBox3.Text = "";
            writeSettings();
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            writeSettings();
        }

        private void checkBox9_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Restart the application to use changes.", "TradeWin Settings");
        }
    }
}
