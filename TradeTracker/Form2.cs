using System;
using System.Data;
using System.Windows.Forms;

namespace TradeTracker
{
    public partial class Form2 : Form
    {
        //DataGridView exportedLineTable = new DataGridView();
        //public DataGridView _form1Instance.Watch = new DataGridView();
        Form1 _form1Instance;
        bool populate = false;
        DataTable table = new DataTable();
        public Form2(Form1 form1Instance, bool autoPopulate)
        {
            this._form1Instance = form1Instance;
            //_form1Instance.Watch = _form1Instance.Watch;
            populate = autoPopulate;
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            table.Columns.Add("Symbol");
            table.Rows.Add();
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.DataSource = table;
            //table.NewRow();
            if (populate)
            {
                dataGridView1.Rows[0].Cells["Symbol"].Value = _form1Instance.Watch.Rows[_form1Instance.Watch.CurrentCell.RowIndex].Cells["Column1"].Value;
                textBox1.Text = _form1Instance.Watch.Rows[_form1Instance.Watch.CurrentCell.RowIndex].Cells["Notes"].Value == null ? "" : _form1Instance.Watch.Rows[_form1Instance.Watch.CurrentCell.RowIndex].Cells["Notes"].Value.ToString();
                dataGridView1.Rows[0].Cells["MajorLevels"].Value = _form1Instance.Watch.Rows[_form1Instance.Watch.CurrentCell.RowIndex].Cells["Column3"].Value;
                dataGridView1.Rows[0].Cells["Side"].Value = _form1Instance.Watch.Rows[_form1Instance.Watch.CurrentCell.RowIndex].Cells["PositionType"].Value;
                dataGridView1.Rows[0].Cells["Entry"].Value = _form1Instance.Watch.Rows[_form1Instance.Watch.CurrentCell.RowIndex].Cells["Actual"].Value;
                dataGridView1.Rows[0].Cells["Exit"].Value = _form1Instance.Watch.Rows[_form1Instance.Watch.CurrentCell.RowIndex].Cells["ExitPrice"].Value;
            }
            dataGridView1.AllowUserToAddRows = false;
        }

        private void button1_Click(object sender, EventArgs e) // Export Trade
        {
            //float p = float.TryParse(dataGridView1.Rows[0].Cells["Entry"].Value == null ? "0" : dataGridView1.Rows[0].Cells["Entry"].Value.ToString());
            //float ea = float.TryParse(dataGridView1.Rows[0].Cells["Exit"].Value == null ? "0" : dataGridView1.Rows[0].Cells["Exit"].Value.ToString());
            //float q = float.TryParse(dataGridView1.Rows[0].Cells["Quantity"].Value == null ? "0" : dataGridView1.Rows[0].Cells["Quantity"].Value.ToString());
            float p = 0, ea = 0, q = 0;
            if (dataGridView1.Rows[0].Cells["Entry"].Value != null && float.TryParse(dataGridView1.Rows[0].Cells["Entry"].Value.ToString(), out p))
            {
            }
            if (dataGridView1.Rows[0].Cells["Exit"].Value != null && float.TryParse(dataGridView1.Rows[0].Cells["Exit"].Value.ToString(), out ea))
            {
            }
            if (dataGridView1.Rows[0].Cells["Quantity"].Value != null && float.TryParse(dataGridView1.Rows[0].Cells["Quantity"].Value.ToString(), out q))
            {
                //p = float.Parse(dataGridView1.Rows[0].Cells["Entry"].Value.ToString());
                //ea = float.Parse(dataGridView1.Rows[0].Cells["Exit"].Value.ToString());
                //q = float.Parse(dataGridView1.Rows[0].Cells["Quantity"].Value.ToString());
            }
            float earnings = ea * q;
            if (Form1.perSale)
            {
                earnings -= _form1Instance.commissions[0];
            }
            if (Form1.perShare)
            {
                earnings -= q * _form1Instance.commissions[1];
            }
            if (Form1.perDollar)
            {
                earnings -= ea * _form1Instance.commissions[2];
            }
            _form1Instance.exportGrid.Rows.Add();
            _form1Instance.exportGrid.Rows[0].Cells["Symbol"].Value = dataGridView1.Rows[0].Cells["Symbol"].Value == null ? "" : dataGridView1.Rows[0].Cells["Symbol"].Value.ToString();
            _form1Instance.exportGrid.Rows[0].Cells["Position"].Value = dataGridView1.Rows[0].Cells["Side"].Value == null ? "" : dataGridView1.Rows[0].Cells["Side"].Value.ToString();
            _form1Instance.exportGrid.Rows[0].Cells["Quantity"].Value = dataGridView1.Rows[0].Cells["Quantity"].Value == null ? "" : dataGridView1.Rows[0].Cells["Quantity"].Value.ToString();
            _form1Instance.exportGrid.Rows[0].Cells["PricePerShare"].Value = dataGridView1.Rows[0].Cells["Entry"].Value == null ? "" : dataGridView1.Rows[0].Cells["Entry"].Value.ToString();
            _form1Instance.exportGrid.Rows[0].Cells["EarningsPerShare"].Value = dataGridView1.Rows[0].Cells["Exit"].Value == null ? "" : dataGridView1.Rows[0].Cells["Exit"].Value.ToString();
            _form1Instance.exportGrid.Rows[0].Cells["Price"].Value = (p * q != 0 ? (p * q).ToString() : "");
            _form1Instance.exportGrid.Rows[0].Cells["Earnings"].Value = earnings.ToString();
            _form1Instance.exportGrid.Rows[0].Cells["MajorLevels"].Value = dataGridView1.Rows[0].Cells["MajorLevels"].Value == null ? "" : dataGridView1.Rows[0].Cells["MajorLevels"].Value.ToString();
            _form1Instance.exportGrid.Rows[0].Cells["Date"].Value = dateTimePicker1.Value.ToString().Substring(0, dateTimePicker1.Value.ToString().Length - 6) + dateTimePicker1.Value.ToString().Substring(dateTimePicker1.Value.ToString().Length - 3, 3);
            _form1Instance.exportGrid.Rows[0].Cells["Strategy"].Value = _form1Instance.Watch.Rows[(_form1Instance.Watch.CurrentCell != null ? _form1Instance.Watch.CurrentCell.RowIndex : 0)].Cells["Column2"].Value == null ? "" : _form1Instance.Watch.Rows[(_form1Instance.Watch.CurrentCell != null ? _form1Instance.Watch.CurrentCell.RowIndex : 0)].Cells["Column2"].Value.ToString();
            _form1Instance.exportGrid.Rows[0].Cells["Strengths"].Value = textBox2.Text;
            _form1Instance.exportGrid.Rows[0].Cells["Weaknesses"].Value = textBox3.Text;
            _form1Instance.exportGrid.Rows[0].Cells["Notes"].Value = textBox1.Text;
            _form1Instance.populateGrid(_form1Instance.exportGrid);
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Enter the trade execution date either from the dropdown menu or using keyboard arrows, then use keyboard arrows to enter the trade execution time.", "Help");
        }

        private void dataGridView1_Validated(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow.Index > 1 && dataGridView1.RowCount > 1)
            {
                //DataGridViewRowCollection rows = new DataGridViewRowCollection
                dataGridView1.Rows[1].Cells["Symbol"].Value = "null";
                dataGridView1.Rows.Remove(dataGridView1.Rows[1]);
            }
        }
    }
}
