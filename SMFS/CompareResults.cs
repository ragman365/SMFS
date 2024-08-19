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
using System.IO;
using GeneralLib;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class CompareResults : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        /****************************************************************************************/
        public CompareResults( DataTable dt )
        {
            InitializeComponent();
            workDt = dt.Copy();
        }
        /****************************************************************************************/
        private void CompareResults_Load(object sender, EventArgs e)
        {
            if ( G1.get_column_number ( workDt,"found") < 0 )
                workDt.Columns.Add("found");
            if (G1.get_column_number(workDt, "foundPay") < 0)
                workDt.Columns.Add("foundPay");
            if (G1.get_column_number(workDt, "foundmonths") < 0)
                workDt.Columns.Add("foundmonths");

            dgv.DataSource = workDt;
        }
        /****************************************************************************************/
        private void btnCompare_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"C:\users\robby\downloads",
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "txt",
                Filter = "txt files (*.txt)|*.txt",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };

            if (openFileDialog1.ShowDialog() != DialogResult.OK)
                return;
            string filename = openFileDialog1.FileName;

            DataTable testDt = new DataTable();
            testDt.Columns.Add("group");
            testDt.Columns.Add("policy");
            testDt.Columns.Add("lastName");
            testDt.Columns.Add("firstName");
            testDt.Columns.Add("amountDue");
            testDt.Columns.Add("months");
            testDt.Columns.Add("paid");
            testDt.Columns.Add("FOUND");
            testDt.Columns.Add("foundmonths");

            string group = "";
            string policy = "";
            string lastName = "";
            string firstName = "";
            string amountDue = "";
            string months = "";
            string paid = "";
            string str = "";

            try
            {
                bool first = true;
                string payer = "";
                string line = "";
                int row = 0;
                str = "";

                string[] Compressed = new string[10];
                int compressedCount = 0;
                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader sr = new StreamReader(fs))

                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Application.DoEvents();
                        if (String.IsNullOrWhiteSpace(line))
                            continue;
                        G1.parse_answer_data(line, " "); // 4,3,12,23,29
                        if ( G1.of_ans_count > 0 )
                        {
                            compressedCount = compressData(Compressed);
                            if (compressedCount >= 7)
                            {

                                group = Compressed[0].Trim();
                                if (group == "GROUP#")
                                    continue;
                                policy = line.Substring(16, 10).Trim();
                                lastName = line.Substring(27, 15).Trim();
                                firstName = line.Substring(43, 10).Trim();
                                amountDue  = line.Substring(54, 7).Trim();
                                months = line.Substring(64, 4).Trim();
                                paid = line.Substring(69, 9).Trim();

                                DataRow dRow = testDt.NewRow();

                                dRow["group"] = group;
                                dRow["policy"] = policy;
                                dRow["lastName"] = lastName;
                                dRow["firstName"] = firstName;
                                dRow["amountDue"] = amountDue;
                                dRow["foundmonths"] = months;
                                dRow["paid"] = paid;
                                testDt.Rows.Add(dRow);
                            }
                        }
                        int count = G1.of_ans_count;
                        //if (G1.of_ans_count >= maxColumns)
                        //{
                        //}
                        row++;
                        //                        picLoader.Refresh();
                    }
                    sr.Close();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }

            bool gotit = false;
            for ( int i=0; i<testDt.Rows.Count; i++)
            {
                group = testDt.Rows[i]["group"].ObjToString();
                policy = testDt.Rows[i]["policy"].ObjToString();
                if ( policy == "90006907")
                {

                }
                lastName = testDt.Rows[i]["lastName"].ObjToString();
                firstName = testDt.Rows[i]["firstName"].ObjToString();
                amountDue = testDt.Rows[i]["amountDue"].ObjToString();
                months = testDt.Rows[i]["foundmonths"].ObjToString();
                paid = testDt.Rows[i]["paid"].ObjToString();
                gotit = FindData(policy, lastName, firstName, paid, months);
                if (gotit)
                    testDt.Rows[i]["FOUND"] = "FOUND";
            }

            for (int i = 0; i < testDt.Rows.Count; i++)
            {
                str = testDt.Rows[i]["FOUND"].ObjToString();
                if ( str != "FOUND")
                {
                    group = testDt.Rows[i]["group"].ObjToString();
                    policy = testDt.Rows[i]["policy"].ObjToString();
                    lastName = testDt.Rows[i]["lastName"].ObjToString();
                    firstName = testDt.Rows[i]["firstName"].ObjToString();
                    amountDue = testDt.Rows[i]["amountDue"].ObjToString();
                    months = testDt.Rows[i]["foundmonths"].ObjToString();
                    paid = testDt.Rows[i]["paid"].ObjToString();
                    paid = paid.Replace(",", "");
                    DataRow dRow = workDt.NewRow();
                    dRow["found"] = "BAD";
                    dRow["groupNumber"] = group;
                    dRow["policyNumber"] = policy;
                    dRow["policyName"] = lastName + ", " + firstName;
                    dRow["customer"] = lastName + ", " + firstName;
                    dRow["foundmonths"] = months;
                    dRow["paid"] = paid.ObjToDouble();
                    workDt.Rows.Add(dRow);
                }
            }
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private bool FindData ( string policy, string lastName, string firstName, string paid, string foundmonths )
        {
            bool found = false;
            DataTable dt = workDt;

            string rgroup = "";
            string rpolicy = "";
            string rlastName = "";
            string rfirstName = "";
            string ramountDue = "";
            string rmonths = "";
            string rpaid = "";

            string policyName = "";
            string name = lastName + ", " + firstName;
            double money = 0D;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                rpolicy = dt.Rows[i]["policyNumber"].ObjToString();
                rlastName = dt.Rows[i]["lastName"].ObjToString();
                rfirstName = dt.Rows[i]["firstName"].ObjToString();
                policyName = dt.Rows[i]["policyName"].ObjToString();
                rmonths = dt.Rows[i]["months"].ObjToString();
                rpaid = dt.Rows[i]["paid"].ObjToString();
                money = rpaid.ObjToDouble();
                rpaid = G1.ReformatMoney((money));
                if ( policyName.IndexOf (name) >= 0 )
                {
                    if (rpolicy == policy)
                    {
                        found = true;
                        dt.Rows[i]["found"] = "FOUND";
                        if (paid == rpaid)
                            dt.Rows[i]["foundPay"] = "FOUND";
                        else
                            dt.Rows[i]["foundPay"] = paid;
                        if (foundmonths == rmonths)
                            dt.Rows[i]["foundMonths"] = "FOUND";
                        else
                            dt.Rows[i]["foundmonths"] = foundmonths;
                        break;
                    }
                }
            }
            return found;
        }
        /****************************************************************************************/
        private int compressData ( string [] Compressed )
        {
            int compressedAnswers = 0;
            int count = G1.of_ans_count;

            for ( int i=0; i<count; i++)
            {
                if ( !String.IsNullOrWhiteSpace ( G1.of_answer[i]))
                {
                    Compressed[compressedAnswers] = G1.of_answer[i];
                    compressedAnswers++;
                    if (compressedAnswers >= 10)
                        break;
                }
            }
            return compressedAnswers;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string policy = dr["policyNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(policy))
                return;
            string cmd = "Select * from `policies` where `policyNumber` = '" + policy + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Bad Policy Number " + policy + "!");
                return;
            }
            string contractNumber = dt.Rows[0]["contractNumber"].ObjToString();
            Policies policyForm = new Policies(contractNumber);
            policyForm.Show();
        }
        /****************************************************************************************/
    }
}