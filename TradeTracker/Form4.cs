using System;
using System.Windows.Forms;
using TradeTracker;

namespace TradeWin
{
    public partial class Form4 : Form
    {
        Form1 _form1Instance;
        public Form4(Form1 form1Instance)
        {
            this._form1Instance = form1Instance;
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            dataGridView1.Rows.Add();
            dataGridView1.Rows[0].Cells["StratName"].Value = _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Column5"].Value;
            if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Keywords"].Value != null)
                textBox1.Text = _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Keywords"].Value.ToString();
            else if (dataGridView1.Rows[0].Cells["StratName"].Value != null)
                textBox1.Text = dataGridView1.Rows[0].Cells["StratName"].Value.ToString() + ",";
            if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Note"].Value != null)
                textBox2.Text = _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Note"].Value.ToString();
            if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Exclusions"].Value != null)
                textBox3.Text = _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Exclusions"].Value.ToString();
            if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Target"].Value != null)
                textBox4.Text = _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Target"].Value.ToString();
            if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AvgWin"].Value != null)
                textBox5.Text = _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AvgWin"].Value.ToString();
            if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AvgLoss2"].Value != null)
                textBox6.Text = _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AvgLoss2"].Value.ToString();
            if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AddNewTradesFrom"].Value != null)
            {
                //int buttons = int.Parse(_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AddNewTradesFrom"].Value.ToString());
                if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AddNewTradesFrom"].Value.ToString() == "Long")
                {
                    radioButton1.Checked = true;
                }
                if (_form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AddNewTradesFrom"].Value.ToString() == "Short")
                {
                    radioButton2.Checked = true;
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The Strategy Builder allows you to define strategies and keywords for your strategies. This allows them to be properly added to the log automatically and allow you to track performance based on strategy.\n\nAt least ONE of the key phrases must appear in a given trade log and ALL exclusion phrases must be absent for a trade to be counted towards a strategy.\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double winRatio = 0, avgWin, avgLoss;
            if (radioButton1.Checked)
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AddNewTradesFrom"].Value = "Long";
            if (radioButton2.Checked)
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AddNewTradesFrom"].Value = "Short";
            if (radioButton3.Checked)
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AddNewTradesFrom"].Value = "Long Or Short";
            _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Exclusions"].Value = textBox3.Text;
            _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Note"].Value = textBox2.Text;
            _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Keywords"].Value = textBox1.Text;
            _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Column5"].Value = dataGridView1.Rows[0].Cells["StratName"].Value;

            if (double.TryParse(textBox4.Text, out winRatio) && winRatio > 0)
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Target"].Value = winRatio;
            else
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["Target"].Value = "";
            if (double.TryParse(textBox5.Text, out avgWin))
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AvgWin"].Value = avgWin;
            else
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AvgWin"].Value = "";
            if (double.TryParse(textBox6.Text, out avgLoss))
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AvgLoss2"].Value = avgLoss;
            else
                _form1Instance.IdentifiedStratTable.Rows[_form1Instance.IdentifiedStratTable.CurrentCell.RowIndex].Cells["AvgLoss2"].Value = "";
            _form1Instance.updateStrategy(_form1Instance.THistory, _form1Instance.IdentifiedStratTable);
            Close();
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            double risk, reward;
            if (double.TryParse(textBox5.Text, out reward) && double.TryParse(textBox6.Text, out risk))
                textBox7.Text = (reward / risk).ToString() + "/1";
            else
                textBox7.Text = "";
        }
    }
}
