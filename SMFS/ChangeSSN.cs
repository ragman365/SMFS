using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using GeneralLib;
using Word = Microsoft.Office.Interop.Word;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ChangeSSN: DevExpress.XtraEditors.XtraForm
    {
        private string workContract = "";
        private string workSSN = "";
        private bool loading = true;
        /***********************************************************************************************/
        public ChangeSSN( string contractNumber, string oldSSN )
        {
            InitializeComponent();
            workContract = contractNumber;
            workSSN = oldSSN;
        }
        /***********************************************************************************************/
        private void ChangeSSN_Load(object sender, EventArgs e)
        {
            this.Text = "Change SSN for Contract " + workContract;
            oldSSN.Text = workSSN;
            oldSSN.Enabled = false;
            loading = false;
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string contract);
        public event d_void_eventdone_string SelectDone;
        protected void OnSelectDone( string contract )
        {
            SelectDone?.Invoke(contract);
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string ssn = oldSSN.Text.Trim();
            ssn = ssn.Replace("-", "");

            string ssnNew = newSSN.Text;

            ssnNew = ssnNew.Replace("-", "");

            string cmd = "Select * from `fcustomers` where `ssn` = '" + ssn + "';";
            DataTable dx = G1.get_db_data(cmd);

            int funCount = dx.Rows.Count;
            int preneedCount = 0;

            cmd = "Select * from `customers` where `ssn` = '" + ssn + "';";
            DataTable dt = G1.get_db_data(cmd);
            preneedCount = dx.Rows.Count;

            string funCountStr = " are ";
            if (funCount == 1)
                funCountStr = " is ";
            funCountStr += funCount.ToString();
            if (funCount == 1)
                funCountStr += " Funeral";
            else
                funCountStr += " Funerals";

            string preCountStr = preneedCount.ToString();
            if (preneedCount == 1)
                preCountStr += " Preneed Contract";
            else
                preCountStr += " Preneed Contracts";


            DialogResult result = MessageBox.Show("*** CONFIRM *** There" + funCountStr + " and " + preCountStr + " with the Old SSN!\nIs it okay to change these SSN's from " + oldSSN.Text + " to " + newSSN.Text + "?", "Change SSN Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;

            string record = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                record = dx.Rows[i]["record"].ObjToString();
                G1.update_db_table("fcustomers", "record", record, new string[] { "ssn", ssnNew });
            }
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                G1.update_db_table("customers", "record", record, new string[] { "ssn", ssnNew });
            }

            OnSelectDone( newSSN.Text );
            this.Close();
        }
        /***********************************************************************************************/
        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        public static bool CheckForOtherDeceased ( string contractNumber )
        {
            bool found = false;
            DateTime deceasedDate = DateTime.Now;
            string cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string workSSN = dt.Rows[0]["ssn"].ObjToString();
                workSSN = workSSN.Replace("-", "");
                if (!String.IsNullOrWhiteSpace(workSSN))
                {
                    string cNum = "";
                    DataTable multiSsnDt = Funerals.GetMultipleSSN(workSSN);
                    for (int i = 0; i < multiSsnDt.Rows.Count; i++)
                    {
                        cNum = multiSsnDt.Rows[i]["contractNumber"].ObjToString();
                        if (cNum == contractNumber)
                            continue;
                        cmd = "Select * from `customers` where `contractNumber` = '" + cNum + "';";
                        dt = G1.get_db_data(cmd);
                        if (dt.Rows.Count > 0)
                        {
                            deceasedDate = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                            if ( deceasedDate.Year > 100 )
                            {
                                found = true;
                                MessageBox.Show("***ERROR*** Contract (" + cNum + ") with same SSN (" + workSSN + ") is already deceased!\nYou must investigate!", "Deceased Contract Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                break;
                            }
                        }
                    }
                }
            }
            return found;
        }
        /***********************************************************************************************/
        private void newSSN_TextChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            string ssn = newSSN.Text;
            if (ssn.Length == 3)
            {
                ssn += "-";
                newSSN.Text = ssn;
            }
            else if (ssn.Length == 6)
            {
                ssn += "-";
                newSSN.Text = ssn;
            }
            newSSN.Refresh();
            newSSN.Select(newSSN.Text.Length, 0);

        }
        /***********************************************************************************************/
    }
}