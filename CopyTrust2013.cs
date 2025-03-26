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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CopyTrust2013 : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        public CopyTrust2013()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void CopyTrust2013_Load(object sender, EventArgs e)
        {
            btnWrite.Hide();
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = stop;
            this.dateTimePicker2.Value = stop;
        }
        /***********************************************************************************************/
        private void btnRead_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-01 00:00:00' ";
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + days.ToString("D2") + " 00:00:00 '";

            string cmd = "Select * from `trust2013r` where `payDate8` = '" + date1 + " ";
            string location = txtLocation.Text.Trim();
            if (!String.IsNullOrWhiteSpace(location))
                cmd += " and `locind` = '" + location + "' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            btnWrite.Show();
        }
        /***********************************************************************************************/
        private void btnWrite_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DateTime date = DateTime.Now;
            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            string address = "";
            string city = "";
            string state = "";
            string zip = "";
            string ssn = "";
            string serviceId = "";

            double beginningBalance = 0D;
            double currentPayments = 0D;
            double currentRemovals = 0D;
            double endingBalance = 0D;

            double interest = 0D;
            double ytdPrevious = 0D;
            double paymentCurrMonth = 0D;
            double deathRemYTDprevious = 0D;
            double deathRemCurrMonth = 0D;
            double refundRemYTDprevious = 0D;
            double refundRemCurrMonth = 0D;

            string location = "";
            string filename = "";
            string locind = "";
            string text2002 = "";

            string cmd = "";
            int lastRow = dt.Rows.Count;
            //lastRow = 1;
            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < lastRow; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                location = dt.Rows[i]["location"].ObjToString();
                locind = dt.Rows[i]["locind"].ObjToString();
                text2002 = dt.Rows[i]["Is2002"].ObjToString();

                beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                currentRemovals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();

                interest = dt.Rows[i]["interest"].ObjToDouble();
                ytdPrevious = dt.Rows[i]["ytdPrevious"].ObjToDouble();
                paymentCurrMonth = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                deathRemYTDprevious = dt.Rows[i]["deathRemYTDprevious"].ObjToDouble();
                deathRemCurrMonth = dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                refundRemYTDprevious = dt.Rows[i]["refundRemYTDprevious"].ObjToDouble();
                refundRemCurrMonth = dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();
                if (beginningBalance == 0D && currentPayments == 0D && currentRemovals == 0D && endingBalance == 0D && interest == 0D)
                {
                    if (ytdPrevious == 0D && paymentCurrMonth == 0D && deathRemYTDprevious == 0D && deathRemCurrMonth == 0D)
                    {
                        if (refundRemYTDprevious == 0D && refundRemCurrMonth == 0D)
                            continue;
                    }
                }

                // date = dt.Rows[i]["payDate8"].ObjToDateTime();
                date = this.dateTimePicker2.Value;
                cmd = "Select * from `Trust2013r` where `contractNumber` = '" + contractNumber + "' and `payDate8` = '" + date.ToString("yyyyMMdd") + "' ";
                cmd += " and `locind` = '" + locind + "' ";
                cmd += ";";
                DataTable dx = G1.get_db_data(cmd);
                string record = "";
                if (dx.Rows.Count > 0)
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    continue;
//                    dt.Rows[i]["dup"] = "Y";
                }
                else
                {
                    record = G1.create_record("Trust2013r", "firstName", "-1");
                    if (G1.BadRecord("Trust2013r", record))
                        break;
                }
                firstName = dt.Rows[i]["firstName"].ObjToString();
                lastName = dt.Rows[i]["lastName"].ObjToString();
                address = dt.Rows[i]["address2013"].ObjToString();
                city = dt.Rows[i]["city2013"].ObjToString();
                state = dt.Rows[i]["state2013"].ObjToString();
                zip = dt.Rows[i]["zip2013"].ObjToString();
                ssn = dt.Rows[i]["ssn2013"].ObjToString();
                serviceId = dt.Rows[i]["ServiceId"].ObjToString();

                beginningBalance = dt.Rows[i]["beginningBalance"].ObjToDouble();
                currentPayments = dt.Rows[i]["currentPayments"].ObjToDouble();
                currentRemovals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                endingBalance = dt.Rows[i]["endingBalance"].ObjToDouble();

                interest = dt.Rows[i]["interest"].ObjToDouble();
                ytdPrevious = dt.Rows[i]["ytdPrevious"].ObjToDouble();
                paymentCurrMonth = dt.Rows[i]["paymentCurrMonth"].ObjToDouble();
                deathRemYTDprevious = dt.Rows[i]["deathRemYTDprevious"].ObjToDouble();
                deathRemCurrMonth = dt.Rows[i]["deathRemCurrMonth"].ObjToDouble();
                refundRemYTDprevious = dt.Rows[i]["refundRemYTDprevious"].ObjToDouble();
                refundRemCurrMonth = dt.Rows[i]["refundRemCurrMonth"].ObjToDouble();

                filename = dt.Rows[i]["filename"].ObjToString();


                G1.update_db_table("Trust2013r", "record", record, new string[] { "contractNumber", contractNumber, "firstName", firstName, "lastName", lastName, "payDate8", date.ToString("yyyyMMdd"), "beginningBalance", beginningBalance.ToString(), "currentPayments", currentPayments.ToString(), "currentRemovals", currentRemovals.ToString(), "endingBalance", endingBalance.ToString(), "Is2002", text2002, "location", location, "filename", filename, "locind", locind });
                G1.update_db_table("Trust2013r", "record", record, new string[] { "interest", interest.ToString(), "ytdPrevious", ytdPrevious.ToString(), "paymentCurrMonth", paymentCurrMonth.ToString(), "deathRemYTDprevious", deathRemYTDprevious.ToString(), "deathRemCurrMonth", deathRemCurrMonth.ToString(), "refundRemYTDprevious", refundRemYTDprevious.ToString(), "refundRemCurrMonth", refundRemCurrMonth.ToString() });
                G1.update_db_table("Trust2013r", "record", record, new string[] { "address2013", address, "city2013", city, "state2013", state, "zip2013", zip, "ssn2013", ssn, "ServiceId", serviceId });
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}