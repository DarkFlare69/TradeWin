using System;
using System.Windows.Forms;
using TradeTracker;

namespace TradeWin
{
    public partial class Form6 : Form
    {
        Form1 _form1Instance;
        public Form6(Form1 form1Instance)
        {
            this._form1Instance = form1Instance;
            InitializeComponent();
        }

        private void Form6_Load(object sender, EventArgs e)
        {
            textBox1.Text = _form1Instance.THistory.Rows[0].Cells["Earnings"].Value.ToString();
        }
    }
}
