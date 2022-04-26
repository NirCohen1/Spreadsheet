using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class SpreadsheetApp : Form
    {
        static SharableSpreadsheet spreadsheeta;
        public SpreadsheetApp()
        {
            spreadsheeta = new SharableSpreadsheet(10, 10);
            InitializeComponent();

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string path = textBox3.Text;
            spreadsheeta.load(path);

            MessageBox.Show("Spreadsheet loaded");
            
        }

        private void SpreadsheetApp_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            int row = Convert.ToInt32(textBox4.Text),
                col = Convert.ToInt32(textBox5.Text);

            string ret = spreadsheeta.getCell(row-1, col-1);
            if(ret!= null)
                MessageBox.Show("string " + ret + " found in cell [" + row.ToString() + "," + col.ToString() + "].");
            else
                MessageBox.Show("null");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int x=0, y=0;
            spreadsheeta.getSize(ref x, ref y);
            MessageBox.Show("Rows = " + x.ToString() + ", Columns = " + y.ToString());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //set cell
            int row = Convert.ToInt32(textBox1.Text),
                col = Convert.ToInt32(textBox2.Text);
            string input = textBox3.Text;

            bool ret = spreadsheeta.setCell(row-1,col-1,input);
            if(ret)
                MessageBox.Show("cell settled");
            else
                MessageBox.Show("Error");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            int row = Convert.ToInt32(textBox7.Text);
            string input = textBox8.Text;
            int col = 0;

            bool ret = spreadsheeta.searchInRow(row - 1, input, ref col);
            if (ret)
                MessageBox.Show("string " + input + " found in cell [" + row.ToString() + "," + (col+1).ToString() + "].");
            else
                MessageBox.Show("Not founded");
            
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {

        }

        private void button5_Click(object sender, EventArgs e)
        {
            int col = Convert.ToInt32(textBox10.Text);
            string input = textBox9.Text;
            int row = 0;

            bool ret = spreadsheeta.searchInRow(col - 1, input, ref row);
            if (ret)
                MessageBox.Show("string " + input + " found in cell [" + (row+1).ToString() + "," + col.ToString() + "].");
            else
                MessageBox.Show("Not founded");
        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void SpreadsheetApp_Load_1(object sender, EventArgs e)
        {

        }

        private void SpreadsheetApp_Load_2(object sender, EventArgs e)
        {

        }
    }
}
