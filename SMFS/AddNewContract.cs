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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class AddNewContract : DevExpress.XtraEditors.XtraForm
    {
        private DateTime workDate;
        private string workContract = "";
        private DataRow workDr = null;
        private bool workAdding = false;
        private bool loading = true;
        private bool is2002 = false;
        /***********************************************************************************************/
        public AddNewContract( bool adding, DateTime dateIn, string contract, DataRow dr )
        {
            InitializeComponent();
            workDate = dateIn;
            workContract = contract;
            workDr = dr;
            workAdding = adding;
        }
        /***********************************************************************************************/
        private void AddNewContract_Load(object sender, EventArgs e)
        {
            is2002 = false;
            string str = workDr["Is2002"].ObjToString();
            if (str == "2002")
                is2002 = true;

            btnAddUpdate.Hide();
            btnDeathRemCurrMonth.Hide();
            btnDeathRemYTDprevious.Hide();
            btnRefundRemCurrMonth.Hide();
            btnRefundRemYTDprevious.Hide();

            if ( workAdding == true)
                btnAddUpdate.Text = "Add";

            txtWorkDate.Text = workDate.ToString("MM/dd/yyyy");

            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("description");
            dt.Columns.Add("data");
            dgv.DataSource = dt;

            LoadData(workDr);

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;
            loading = false;
        }
        /***************************************************************************************/
        private void LoadData(DataRow workDr)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = workDr["record"].ObjToString();
            string contractNumber = workDr["contractNumber"].ObjToString();
            string ssn = workDr["ssn2013"].ObjToString();
            string city = workDr["city2013"].ObjToString();
            string state = workDr["state2013"].ObjToString();
            string zip = workDr["zip2013"].ObjToString();
            double beginningBalance = workDr["beginningBalance"].ObjToDouble();
            double ytdPrevious = workDr["ytdPrevious"].ObjToDouble();
            double paymentCurrMonth = workDr["paymentCurrMonth"].ObjToDouble();
            double deathRemYTDprevious = workDr["deathRemYTDprevious"].ObjToDouble();
            double deathRemCurrMonth = workDr["deathRemCurrMonth"].ObjToDouble();
            double refundRemYTDprevious = workDr["refundRemYTDprevious"].ObjToDouble();
            double refundRemCurrMonth = workDr["refundRemCurrMonth"].ObjToDouble();
            double endingBalance = workDr["endingBalance"].ObjToDouble();
            double interest = workDr["interest"].ObjToDouble();
            string serviceId = workDr["serviceId"].ObjToString();
            string fName = workDr["firstName"].ObjToString();
            string lName = workDr["lastName"].ObjToString();
            string location = workDr["location"].ObjToString();
            string locind = workDr["locind"].ObjToString();
            string loc = workDr["loc"].ObjToString();
            if (String.IsNullOrWhiteSpace(location))
                location = loc;

            LoadDataRow(dt, "Contract Number", contractNumber);
            LoadDataRow(dt, "First Name", fName);
            LoadDataRow(dt, "Last Name", lName);
            LoadDataRow(dt, "SSN", ssn);
            LoadDataRow(dt, "City", city);
            LoadDataRow(dt, "State", state);
            LoadDataRow(dt, "Zip", zip);
            LoadDataRow(dt, "Service Id", serviceId);
            LoadDataRow(dt, "Beginning Balance", beginningBalance.ToString());
            LoadDataRow(dt, "Interest", interest.ToString());
            LoadDataRow(dt, "YTD Previous Periods", ytdPrevious.ToString());
            LoadDataRow(dt, "Payment Curr Month", paymentCurrMonth.ToString());
            LoadDataRow(dt, "Death Rem YTD Previous", deathRemYTDprevious.ToString());
            LoadDataRow(dt, "Death Rem Current Month", deathRemCurrMonth.ToString());
            LoadDataRow(dt, "Refund Rem YTD Previous", refundRemYTDprevious.ToString());
            LoadDataRow(dt, "Refund Rem Current Month", refundRemCurrMonth.ToString());
            LoadDataRow(dt, "Ending Balance", endingBalance.ToString());
            LoadDataRow(dt, "Location", location);
            LoadDataRow(dt, "LocInd", locind);

            G1.NumberDataTable(dt);

            dgv.DataSource = dt;
        }
        /***************************************************************************************/
        private string GetData ( DataTable dt, string name)
        {
            string data = "";
            int row = FindRow(name);
            if (row >= 0)
            {
                try
                {
                    data = dt.Rows[row]["data"].ObjToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Getting Field " + name + " " + ex.Message.ToString());
                }
            }
            return data;
        }
        /***************************************************************************************/
        private void UpdateDataRow ( DataTable dt, string name, string formalName)
        {
            int row = FindRow(name);
            if (row < 0)
                return;
            try
            {
                string data = dt.Rows[row]["data"].ObjToString();
                workDr[formalName] = data;
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Field " + name + " to " + formalName + " " + ex.Message.ToString());
            }
        }
        /***************************************************************************************/
        private void LoadDataRow(DataTable dt, string name, string answer = "")
        {
            int row = FindRow(name);
            if (row >= 0)
            {
                if (G1.validate_numeric(answer))
                {
                    if (name.ToUpper() != "SSN" && name.ToUpper() != "ZIP")
                    {
                        double dValue = answer.ObjToDouble();
                        answer = G1.ReformatMoney(dValue);
                    }
                }
                dt.Rows[row]["data"] = answer;
            }
            else
            {
                DataRow dRow = dt.NewRow();
                dRow["description"] = name;
                if (G1.validate_numeric(answer))
                {
                    if (name.ToUpper() != "SSN" && name.ToUpper() != "ZIP")
                    {
                        double dValue = answer.ObjToDouble();
                        answer = G1.ReformatMoney(dValue);
                    }
                }
                dRow["data"] = answer;
                dt.Rows.Add(dRow);
            }
        }
        /***************************************************************************************/
        private int FindRow(string desc)
        {
            int row = -1;
            if (dgv.DataSource == null)
                return row;
            string description = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                description = dt.Rows[i]["description"].ObjToString().ToUpper();
                if (description == desc.ToUpper())
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /***********************************************************************************************/
        private bool currentPaymentChanged = false;
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            string field = dr["description"].ObjToString();
            string data = e.Value.ObjToString();
            if (field == "Beginning Balance")
                dr["data"] = ConvertToDouble(data);
            else if (field == "YTD Previous Periods")
                dr["data"] = ConvertToDouble(data);
            else if (field == "Death Rem YTD Previous")
                dr["data"] = ConvertToDouble(data);
            else if (field == "Death Rem Current Month")
                dr["data"] = ConvertToDouble(data);
            else if (field == "Refund Rem YTD Previous")
                dr["data"] = ConvertToDouble(data);
            else if (field == "Refund Rem Current Month")
                dr["data"] = ConvertToDouble(data);
            else if (field == "Interest")
                dr["data"] = ConvertToDouble(data);
            else if (field == "Payment Curr Month")
            {
                dr["data"] = ConvertToDouble(data);
                currentPaymentChanged = true;
            }
            else if (field == "Contract Number")
                dr["data"] = data;
            else if (field == "LocInd")
                dr["data"] = data;
            else if (field == "Location")
                dr["data"] = data;

            //if ( field == "Location")
            //{
            //    string str = data;
            //    if (is2002)
            //    {
            //        if ( str.IndexOf ( "02") < 0 )
            //            str += "02";
            //    }
            //    workDr["locind"] = str;
            //}


            UpdateData();

            LoadData(workDr);

            btnAddUpdate.Show();
        }
        /***********************************************************************************************/
        private string ConvertToDouble ( string data)
        {
            double dValue = data.ObjToDouble();
            data = G1.ReformatMoney(dValue);
            return data;
        }
        /***********************************************************************************************/
        private void gridMain_ShowingEditor(object sender, CancelEventArgs e)
        {
            if (workAdding)
                return;
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string field = dr["description"].ObjToString();
            string data = dr["data"].ObjToString();

            //if (field == "Beginning Balance")
            //    e.Cancel = true;
            //else if (field == "YTD Previous Periods")
            //    e.Cancel = true;
            //if (field == "Death Rem YTD Previous")
            //    e.Cancel = true;
            //else if (field == "Death Rem Current Month")
            //    e.Cancel = true;
            //else if (field == "Refund Rem YTD Previous")
            //    e.Cancel = true;
            //else if (field == "Refund Rem Current Month")
            //    e.Cancel = true;
            //else if (field == "First Name")
            //    e.Cancel = true;
            //else if (field == "Last Name")
            //    e.Cancel = true;
            //else if (field == "Location")
            //    e.Cancel = true;
            if (field == "Contract Number")
                e.Cancel = true;
        }
        /***********************************************************************************************/
        private void btnMoveBalance_Click(object sender, EventArgs e)
        {
            if (btnMoveBalance.BackColor == Color.LightGreen)
            {
                btnMoveBalance.BackColor = Color.Transparent;
                btnDeathRemYTDprevious.Hide();
                btnDeathRemCurrMonth.Hide();
                btnRefundRemYTDprevious.Hide();
                btnRefundRemCurrMonth.Hide();
            }
            else
            {
                btnMoveBalance.BackColor = Color.LightGreen;
                //btnDeathRemYTDprevious.Show();
                btnDeathRemCurrMonth.Show();
                //btnRefundRemYTDprevious.Show();
                btnRefundRemCurrMonth.Show();
            }
        }
        /***********************************************************************************************/
        private void btnDeathRemYTDprevious_Click(object sender, EventArgs e)
        {
            btnMoveBalance.BackColor = Color.Transparent;
            btnDeathRemYTDprevious.Hide();
            btnDeathRemCurrMonth.Hide();
            btnRefundRemYTDprevious.Hide();
            btnRefundRemCurrMonth.Hide();

            DataRow dr = workDr;

            currentPaymentChanged = false;

            string record = dr["record"].ObjToString();
            double beginningBalance = dr["beginningBalance"].ObjToDouble();
            double ytdPrevious = dr["ytdPrevious"].ObjToDouble();
            double paymentCurrMonth = dr["paymentCurrMonth"].ObjToDouble();
            double deathRemYTDprevious = dr["deathRemYTDprevious"].ObjToDouble();
            double deathRemCurrMonth = dr["deathRemCurrMonth"].ObjToDouble();
            double refundRemYTDprevious = dr["refundRemYTDprevious"].ObjToDouble();
            double refundRemCurrMonth = dr["refundRemCurrMonth"].ObjToDouble();
            beginningBalance += ytdPrevious;
            if (ytdPrevious <= 0D)
                beginningBalance += paymentCurrMonth;
            if (beginningBalance > 0D)
            {
                dr["deathRemYTDprevious"] = beginningBalance;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (deathRemYTDprevious > 0D)
            {
                beginningBalance = deathRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = beginningBalance;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (deathRemCurrMonth > 0D)
            {
                beginningBalance = deathRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = beginningBalance;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (refundRemYTDprevious > 0D)
            {
                beginningBalance = refundRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = beginningBalance;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (refundRemCurrMonth > 0D)
            {
                beginningBalance = refundRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = beginningBalance;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else
                return;

            workDr = dr;
            LoadData(workDr);
            gridMain.RefreshData();
            dgv.Refresh();
            btnAddUpdate.Show();
        }
        /***********************************************************************************************/
        private void btnDeathRemCurrMonth_Click(object sender, EventArgs e)
        {
            btnMoveBalance.BackColor = Color.Transparent;
            btnDeathRemYTDprevious.Hide();
            btnDeathRemCurrMonth.Hide();
            btnRefundRemYTDprevious.Hide();
            btnRefundRemCurrMonth.Hide();

            DataRow dr = workDr;
            currentPaymentChanged = false;

            string record = dr["record"].ObjToString();
            double beginningBalance = dr["beginningBalance"].ObjToDouble();
            double ytdPrevious = dr["ytdPrevious"].ObjToDouble();
            double paymentCurrMonth = dr["paymentCurrMonth"].ObjToDouble();
            double deathRemYTDprevious = dr["deathRemYTDprevious"].ObjToDouble();
            double deathRemCurrMonth = dr["deathRemCurrMonth"].ObjToDouble();
            double refundRemYTDprevious = dr["refundRemYTDprevious"].ObjToDouble();
            double refundRemCurrMonth = dr["refundRemCurrMonth"].ObjToDouble();
            beginningBalance += ytdPrevious;
            if (ytdPrevious <= 0D)
                beginningBalance += paymentCurrMonth;
            if (ytdPrevious > 0)
                beginningBalance += paymentCurrMonth;
            if (beginningBalance > 0D)
            {
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = beginningBalance;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (deathRemYTDprevious > 0D)
            {
                beginningBalance = deathRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0;
                dr["deathRemCurrMonth"] = beginningBalance;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (deathRemCurrMonth > 0D)
            {
                beginningBalance = deathRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = beginningBalance;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (refundRemYTDprevious > 0D)
            {
                beginningBalance = refundRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = beginningBalance;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (refundRemCurrMonth > 0D)
            {
                beginningBalance = refundRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = beginningBalance;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else
                return;

            workDr = dr;
            LoadData(workDr);
            gridMain.RefreshData();
            dgv.Refresh();
            btnAddUpdate.Show();
        }
        /***********************************************************************************************/
        private void btnRefundRemYTDprevious_Click(object sender, EventArgs e)
        {
            btnMoveBalance.BackColor = Color.Transparent;
            btnDeathRemYTDprevious.Hide();
            btnDeathRemCurrMonth.Hide();
            btnRefundRemYTDprevious.Hide();
            btnRefundRemCurrMonth.Hide();

            DataRow dr = workDr;
            currentPaymentChanged = false;

            string record = dr["record"].ObjToString();
            double beginningBalance = dr["beginningBalance"].ObjToDouble();
            double ytdPrevious = dr["ytdPrevious"].ObjToDouble();
            double paymentCurrMonth = dr["paymentCurrMonth"].ObjToDouble();
            double deathRemYTDprevious = dr["deathRemYTDprevious"].ObjToDouble();
            double deathRemCurrMonth = dr["deathRemCurrMonth"].ObjToDouble();
            double refundRemYTDprevious = dr["refundRemYTDprevious"].ObjToDouble();
            double refundRemCurrMonth = dr["refundRemCurrMonth"].ObjToDouble();
            beginningBalance += ytdPrevious;
            if (ytdPrevious <= 0D)
                beginningBalance += paymentCurrMonth;
            if (beginningBalance > 0D)
            {
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = beginningBalance;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (deathRemYTDprevious > 0D)
            {
                beginningBalance = deathRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = beginningBalance;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (deathRemCurrMonth > 0D)
            {
                beginningBalance = deathRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = beginningBalance;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (refundRemYTDprevious > 0D)
            {
                beginningBalance = refundRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = beginningBalance;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (refundRemCurrMonth > 0D)
            {
                beginningBalance = refundRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = beginningBalance;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else
                return;

            workDr = dr;
            LoadData(workDr);
            gridMain.RefreshData();
            dgv.Refresh();
            btnAddUpdate.Show();
        }
        /***********************************************************************************************/
        private void btnRefundRemCurrMonth_Click(object sender, EventArgs e)
        {
            btnMoveBalance.BackColor = Color.Transparent;
            btnDeathRemYTDprevious.Hide();
            btnDeathRemCurrMonth.Hide();
            btnRefundRemYTDprevious.Hide();
            btnRefundRemCurrMonth.Hide();

            DataRow dr = workDr;
            currentPaymentChanged = false;

            string record = dr["record"].ObjToString();
            double beginningBalance = dr["beginningBalance"].ObjToDouble();
            double ytdPrevious = dr["ytdPrevious"].ObjToDouble();
            double paymentCurrMonth = dr["paymentCurrMonth"].ObjToDouble();
            double deathRemYTDprevious = dr["deathRemYTDprevious"].ObjToDouble();
            double deathRemCurrMonth = dr["deathRemCurrMonth"].ObjToDouble();
            double refundRemYTDprevious = dr["refundRemYTDprevious"].ObjToDouble();
            double refundRemCurrMonth = dr["refundRemCurrMonth"].ObjToDouble();
            beginningBalance += ytdPrevious;
            if (ytdPrevious <= 0D)
                beginningBalance += paymentCurrMonth;
            if (beginningBalance > 0D)
            {
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = beginningBalance;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (deathRemYTDprevious > 0D)
            {
                beginningBalance = deathRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = beginningBalance;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (deathRemCurrMonth > 0D)
            {
                beginningBalance = deathRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = beginningBalance;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (refundRemYTDprevious > 0D)
            {
                beginningBalance = refundRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = beginningBalance;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else if (refundRemCurrMonth > 0D)
            {
                beginningBalance = refundRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = beginningBalance;
                dr["currentRemovals"] = beginningBalance;
                dr["endingBalance"] = 0D;
            }
            else
                return;

            workDr = dr;
            LoadData(workDr);
            gridMain.RefreshData();
            dgv.Refresh();
            btnAddUpdate.Show();
        }
        /***********************************************************************************************/
        private void btnClearRemoval_Click(object sender, EventArgs e)
        {
            btnMoveBalance.BackColor = Color.Transparent;
            btnDeathRemYTDprevious.Hide();
            btnDeathRemCurrMonth.Hide();
            btnRefundRemYTDprevious.Hide();
            btnRefundRemCurrMonth.Hide();

            DataRow dr = workDr;

            string record = dr["record"].ObjToString();
            double beginningBalance = dr["beginningBalance"].ObjToDouble();
            double ytdPrevious = dr["ytdPrevious"].ObjToDouble();
            double paymentCurrMonth = dr["paymentCurrMonth"].ObjToDouble();
            double deathRemYTDprevious = dr["deathRemYTDprevious"].ObjToDouble();
            double deathRemCurrMonth = dr["deathRemCurrMonth"].ObjToDouble();
            double refundRemYTDprevious = dr["refundRemYTDprevious"].ObjToDouble();
            double refundRemCurrMonth = dr["refundRemCurrMonth"].ObjToDouble();
            beginningBalance += ytdPrevious;
            if (ytdPrevious <= 0D)
                beginningBalance += paymentCurrMonth;
            if (beginningBalance > 0D)
            {
                dr["deathRemYTDprevious"] = 0D;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = 0D;
                dr["endingBalance"] = beginningBalance;
            }
            else if (deathRemYTDprevious > 0D)
            {
                beginningBalance = deathRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = 0D;
                dr["endingBalance"] = beginningBalance;
            }
            else if (deathRemCurrMonth > 0D)
            {
                beginningBalance = deathRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = 0D;
                dr["endingBalance"] = beginningBalance;
            }
            else if (refundRemYTDprevious > 0D)
            {
                beginningBalance = refundRemYTDprevious;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = 0D;
                dr["endingBalance"] = beginningBalance;
            }
            else if (refundRemCurrMonth > 0D)
            {
                beginningBalance = refundRemCurrMonth;
                dr["beginningBalance"] = beginningBalance;
                dr["deathRemYTDprevious"] = 0;
                dr["deathRemCurrMonth"] = 0D;
                dr["refundRemYTDprevious"] = 0D;
                dr["refundRemCurrMonth"] = 0D;
                dr["currentRemovals"] = 0D;
                dr["endingBalance"] = beginningBalance;
            }
            else
                return;

            workDr = dr;
            LoadData(workDr);
            gridMain.RefreshData();
            dgv.Refresh();
            btnAddUpdate.Show();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt( DataRow dr);
        public event d_void_eventdone_dt Done;
        protected void OnDone(DataRow dr)
        {
            Done?.Invoke(dr);
        }
        /***********************************************************************************************/
        private void UpdateData ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            UpdateDataRow(dt, "Contract Number", "contractNumber");
            UpdateDataRow(dt, "First Name", "firstName");
            UpdateDataRow(dt, "Last Name", "lastName");
            UpdateDataRow(dt, "SSN", "ssn2013");
            UpdateDataRow(dt, "City", "city2013");
            UpdateDataRow(dt, "State", "state2013");
            UpdateDataRow(dt, "Zip", "zip2013");
            UpdateDataRow(dt, "Service Id", "ServiceId");
            UpdateDataRow(dt, "Beginning Balance", "beginningBalance");
            UpdateDataRow(dt, "Interest", "interest");

            UpdateDataRow(dt, "YTD Previous Periods", "ytdPrevious");
            UpdateDataRow(dt, "Payment Curr Month", "paymentCurrMonth");
            UpdateDataRow(dt, "Death Rem YTD Previous", "deathRemYTDprevious");
            UpdateDataRow(dt, "Death Rem Current Month", "deathRemCurrMonth");
            UpdateDataRow(dt, "Refund Rem YTD Previous", "refundRemYTDprevious");
            UpdateDataRow(dt, "Refund Rem Current Month", "refundRemCurrMonth");
            UpdateDataRow(dt, "Ending Balance", "endingBalance");

            UpdateDataRow(dt, "Location", "location");
            UpdateDataRow(dt, "LocInd", "LocInd");

            if (currentPaymentChanged)
            {
                double previous = GetData(dt, "YTD Previous Periods").ObjToDouble();
                double currentPayments = GetData(dt, "Payment Curr Month").ObjToDouble();
                double ytd = previous + currentPayments;
                workDr["currentPayments"] = ytd.ToString();
                double ending = GetData(dt, "Ending Balance").ObjToDouble();
                ending += currentPayments;
                DataRow dr = workDr;
                dr["endingBalance"] = ending;
                //UpdateDataRow(dt, "Ending Balance", "endingBalance");
            }
        }
        /***********************************************************************************************/
        private void btnAddUpdate_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if ( workAdding )
            {
                string contract = GetData(dt, "Contract Number");
                if ( String.IsNullOrWhiteSpace ( contract))
                {
                    MessageBox.Show("***ERROR*** You must supply a contract Number of you are adding a contract!");
                    return;
                }
                string cmd = "Select * from `trust2013r` where `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0 )
                {
                    MessageBox.Show("***ERROR*** Contract " + contract + " already exists!\nContract must be unique!");
                    dx.Dispose();
                    dx = null;
                    return;
                }
                dx.Dispose();
                dx = null;
            }

            string location = GetData(dt, "Location");
            if (String.IsNullOrWhiteSpace(location))
            {
                MessageBox.Show("***ERROR*** You must supply a location for the contract!");
                return;
            }

            UpdateData();

            OnDone(workDr);
            this.Close();
        }
        /***********************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                (dgv.FocusedView as ColumnView).FocusedRowHandle++;
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
    }
}