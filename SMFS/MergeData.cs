using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GeneralLib;
/**************************************************************************************/
namespace SMFS
{
    /**************************************************************************************/
    public partial class MergeData : DevExpress.XtraEditors.XtraForm
    {
        /**************************************************************************************/
        public MergeData()
        {
            InitializeComponent();
        }
        /**************************************************************************************/
        private void MergeData_Load(object sender, EventArgs e)
        {
            dgv2.Dock = DockStyle.Fill;
            dgv3.Hide();
            dgv3.Dock = DockStyle.Fill;
        }
        /**************************************************************************************/
        private void btnSelectFile1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    DataTable dt = Import.ImportCSVfile(file);
                    dgv.DataSource = dt;
                    txtFile1.Text = file;
                }
            }
            return;
        }
        /**************************************************************************************/
        private void btnSelectFile2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    DataTable dt = Import.ImportCSVfile(file);
                    dgv2.DataSource = dt;
                    txtFile2.Text = file;
                }
            }
            return;
        }
        /**************************************************************************************/
        private void btnMerge_Click(object sender, EventArgs e)
        {
            DataTable mainDt = (DataTable)dgv.DataSource;
            DataTable secondDt = (DataTable)dgv2.DataSource;
            DataTable dt = mainDt.Copy();
            string firstName = "";
            string lastName = "";
            string madeIt = "1";
            string column = "";
            try
            {
                for (int i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    firstName = dt.Rows[i]["first"].ObjToString();
                    lastName = dt.Rows[i]["last"].ObjToString();
                    if (String.IsNullOrWhiteSpace(firstName) || String.IsNullOrWhiteSpace(lastName))
                        dt.Rows.RemoveAt(i);
                }
                madeIt = "2";
                for (int i = secondDt.Rows.Count - 1; i >= 0; i--)
                {
                    firstName = secondDt.Rows[i]["first"].ObjToString();
                    lastName = secondDt.Rows[i]["last"].ObjToString();
                    if (String.IsNullOrWhiteSpace(firstName) || String.IsNullOrWhiteSpace(lastName))
                        secondDt.Rows.RemoveAt(i);
                }
                madeIt = "3";
                for (int col = 0; col < secondDt.Columns.Count; col++)
                {
                    column = secondDt.Columns[col].ColumnName;
                    if (G1.get_column_number(mainDt, column) < 0)
                        dt.Columns.Add(column);
                }
            }
            catch ( Exception ex)
            {
            }
            string cmd = "";
            DataRow[] dRows = null;
            DataRow dR = null;
            madeIt = "4";
            try
            {
                for (int i = 0; i < secondDt.Rows.Count; i++)
                {
                    firstName = secondDt.Rows[i]["first"].ObjToString();
                    lastName = secondDt.Rows[i]["last"].ObjToString();
                    cmd = "first='" + firstName + "' AND last='" + lastName + "'";
                    dRows = mainDt.Select(cmd);
                    if (dRows.Length <= 0)
                    {
                        dR = dt.NewRow();
                        dR["first"] = firstName;
                        dR["last"] = lastName;
                        dt.Rows.Add(dR);
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            string data = "";
            string oldData = "";
            madeIt = "5";
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    firstName = dt.Rows[i]["first"].ObjToString();
                    lastName = dt.Rows[i]["last"].ObjToString();
                    cmd = "first='" + firstName + "' AND last='" + lastName + "'";
                    dRows = secondDt.Select(cmd);
                    if (dRows.Length > 0)
                    {
                        for (int col = 0; col < secondDt.Columns.Count; col++)
                        {
                            column = secondDt.Columns[col].ColumnName;
                            if (G1.get_column_number(mainDt, column) < 0)
                            {
                                data = dRows[0][column].ObjToString();
                                data = data.Replace("\\,", ",");
                                oldData = dt.Rows[i][column].ObjToString();
                                if ( String.IsNullOrWhiteSpace ( oldData) && !String.IsNullOrWhiteSpace ( data))
                                    dt.Rows[i][column] = data;
                            }
                        }
                    }

                }
            }
            catch ( Exception ex)
            {
            }
            DataView tempview = dt.DefaultView;
            tempview.Sort = "last ASC,first ASC";
            dt = tempview.ToTable();
            G1.NumberDataTable(dt);
            splitContainer1.Panel1Collapsed = true;
            dgv3.DataSource = dt;
            dgv2.Hide();
            dgv3.Show();
        }
        /**************************************************************************************/
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog ofdImage = new SaveFileDialog())
            {
                ofdImage.Filter = "All files (*.*)|*.*";

                if (ofdImage.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = ofdImage.FileName;
                    filename = filename.Replace('\\', '/');
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        DataTable dt = (DataTable)dgv3.DataSource;
                        if ( dt != null)
                        {
                            DataTable dx = dt.Copy();
                            if (G1.get_column_number(dx, "num") >= 0)
                                dx.Columns.Remove("num");
                            try
                            {
                                MySQL.CreateCSVfile(dx, filename, true, "~");
                            }
                            catch ( Exception ex)
                            {
                            }
                        }
                    }
                }
            }
        }
        /**************************************************************************************/
    }
}