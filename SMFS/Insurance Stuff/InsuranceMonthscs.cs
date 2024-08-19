using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using System.Globalization;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class InsuranceMonthscs : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        /***********************************************************************************************/
        public InsuranceMonthscs( DataTable dt )
        {
            InitializeComponent();
            workDt = dt;
        }
        /***********************************************************************************************/
        private void InsuranceMonthscs_Load(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("lastName");
            dt.Columns.Add("firstName");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("payer");
            dt.Columns.Add("numMonths");
            dt.Columns.Add("payDate8");
            dt.Columns.Add("found");

            labelMaximum.Show();
            labelMaximum.Text = "0";
            barImport.Show();
            lblTotal.Show();
            barImport.Minimum = 0;

            int lastrow = workDt.Rows.Count;
            //lastrow = 100;
            barImport.Maximum = lastrow;
            lblTotal.Text = "of " + lastrow.ToString();
            lblTotal.Refresh();

            string contractNumber = "";
            string lastName = "";
            string firstName = "";
            string payer = "";
            string numMonths = "";
            string oldMonths = "";
            string found = "";
            string payment = "";
            string dateKeyed = "";
            string depositNumber = "";
            int iDeposit = 0;
            string record = "";
            string details = "";

            int foundGood = 0;
            int foundBad = 0;

            string cmd = "";
            DateTime payDate8 = DateTime.Now;

            G1.CreateAudit("Import Ins Months");

            G1.WriteAudit("Last Row =" + lastrow.ToString());

            DataTable dx = null;
            DataTable dxx = null;
            DataRow dR = null;
            try
            {
                for (int i = 0; i < lastrow; i++)
                {
                    barImport.Value = i;
                    barImport.Refresh();
                    labelMaximum.Text = i.ToString();
                    labelMaximum.Refresh();

                    if ( i > 0 && (i%1000) == 0 )
                        G1.WriteAudit("Import Row =" + i.ToString() + " Good = " + foundGood.ToString() + " Bad = " + foundBad.ToString() );
                    try
                    {
                        payer = workDt.Rows[i]["payer#"].ObjToString();
                        payer = payer.TrimStart('0');
                        firstName = workDt.Rows[i]["Payer First Name"].ObjToString();
                        firstName = G1.protect_data(firstName);
                        lastName = workDt.Rows[i]["Payer Last Name"].ObjToString();
                        lastName = G1.protect_data(lastName);
                        numMonths = workDt.Rows[i]["total months"].ObjToString();
                        dateKeyed = workDt.Rows[i]["date keyed"].ObjToString();
                        payDate8 = dateKeyed.ObjToDateTime();
                        depositNumber = workDt.Rows[i]["deposit number"].ObjToString();
                        payment = workDt.Rows[i]["Amount keyed"].ObjToString();
                        contractNumber = "";
                        found = "NO";

                        dR = dt.NewRow();
                        dR["firstName"] = firstName;
                        dR["lastName"] = lastName;
                        dR["payer"] = payer;
                        dR["numMonths"] = numMonths;
                        dR["contractNumber"] = contractNumber;
                        dR["payDate8"] = dateKeyed;
                        dR["found"] = "NO";

                        cmd = "Select * from `icustomers` where `payer` = '" + payer + "' AND `firstName` = '" + firstName + "' AND `lastName` = '" + lastName + "' ORDER BY `contractNumber` DESC;";
                        dxx = G1.get_db_data(cmd);

                        if (dxx.Rows.Count <= 0)
                        {
                            foundBad++;
                            dt.Rows.Add(dR);
                            continue;
                        }

                        for (int j = 0; j < dxx.Rows.Count; j++)
                        {
                            contractNumber = dxx.Rows[j]["contractNumber"].ObjToString();

                            dateKeyed = payDate8.Year.ToString("D4") + "-" + payDate8.Month.ToString("D2") + "-" + payDate8.Day.ToString("D2");
                            if ( payDate8.Year > 100)
                            {
                                cmd = "Select * from `ipayments` where `contractNumber` = '" + contractNumber + "' AND `payDate8` = '" + dateKeyed + "';";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                {
                                    record = dx.Rows[0]["record"].ObjToString();
                                    oldMonths = dx.Rows[0]["numMonthPaid"].ObjToString();
                                    if (UpdateNumMonths(numMonths, oldMonths))
                                    {
                                        details = "numMonthPaid," + numMonths;
                                        G1.update_db_table("ipayments", "record", record, details);
                                    }
                                    found = "YES";
                                }
                                else if ( !String.IsNullOrWhiteSpace(depositNumber))
                                {
                                    iDeposit = Convert.ToInt32(depositNumber);
                                    if (iDeposit > 0)
                                    {
                                        depositNumber = iDeposit.ToString("D4");
                                        cmd = "Select * from `ipayments` where `contractNumber` = '" + contractNumber + "' AND `depositNumber` = '" + depositNumber + "';";
                                        dx = G1.get_db_data(cmd);
                                        if (dx.Rows.Count > 0)
                                        {
                                            record = dx.Rows[0]["record"].ObjToString();
                                            oldMonths = dx.Rows[0]["numMonthPaid"].ObjToString();
                                            if (UpdateNumMonths(numMonths, oldMonths))
                                            {
                                                details = "numMonthPaid," + numMonths;
                                                G1.update_db_table("ipayments", "record", record, details);
                                            }
                                            found = "YES";
                                        }
                                    }
                                }
                            }
                            else if(!String.IsNullOrWhiteSpace(depositNumber))
                            {
                                iDeposit = Convert.ToInt32(depositNumber);
                                if (iDeposit > 0)
                                {
                                    depositNumber = iDeposit.ToString("D4");
                                    cmd = "Select * from `ipayments` where `contractNumber` = '" + contractNumber + "' AND `depositNumber` = '" + depositNumber + "';";
                                    dx = G1.get_db_data(cmd);
                                    if (dx.Rows.Count > 0)
                                    {
                                        record = dx.Rows[0]["record"].ObjToString();
                                        oldMonths = dx.Rows[0]["numMonthPaid"].ObjToString();
                                        if (UpdateNumMonths(numMonths, oldMonths))
                                        {
                                            details = "numMonthPaid," + numMonths;
                                            G1.update_db_table("ipayments", "record", record, details);
                                        }
                                        found = "YES";
                                    }
                                }
                            }
                            dR["found"] = found;
                            if (found == "YES")
                                break;
                        }
                        if (found.ToUpper() != "YES")
                            foundBad++;
                        else
                            foundGood++;
                        dt.Rows.Add(dR);
                    }
                    catch ( Exception ex)
                    {
                    }
                }
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;
                G1.WriteAudit("Finished Import Good = " + foundGood.ToString() + " Bad = " + foundBad.ToString());
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private bool UpdateNumMonths ( string numMonths, string oldMonths )
        {
            bool doUpdate = true;
            int i_numMonths = Convert.ToInt32 ( numMonths);
            int i_oldMonths = Convert.ToInt32(oldMonths);
            if (i_numMonths == i_oldMonths)
                doUpdate = false;
            return doUpdate;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            string payer = dr["payer"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                DailyHistory dailyForm = new DailyHistory(contract, null, null);
                dailyForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
    }
}