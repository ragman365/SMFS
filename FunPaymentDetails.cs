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
using System.IO;
using System.Text.RegularExpressions;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraGrid.Views.Grid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.Utils;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraPrinting;
using OfficeOpenXml.FormulaParsing.LexicalAnalysis.TokenSeparatorHandlers;
using DevExpress.XtraReports.UI;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.ViewInfo;
using System.Globalization;
//using DevExpress.XtraGrid.Views.Grid;
//using DevExpress.XtraGrid.Views.Base;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class FunPaymentDetails : DevExpress.XtraEditors.XtraForm
    {
        private string workContract = "";
        private string workServiceId = "";
        private string workType = "";
        private bool funModified = false;
        private bool loading = true;
        private Color serviceColor = Color.Transparent;
        private bool showServices = true;
        private bool showMerchandise = false;
        private bool showCashAdvanced = false;
        private DataTable workDt = null;
        private string workPaidRecord = "";
        private double workAmountDue = 0D;
        private string workNames = "";
        private bool workNoEdit = false;
        /****************************************************************************************/
        public double amountFiled = 0D;
        public double amountReceived = 0D;
        public double amountDiscount = 0D;
        public double amountGrowth = 0D;
        public double amountDebit = 0D;
        public string depositNumbers = "";
        public string paymentStatus = "";
        private bool dataSaved = false;
        private DataRow workDR = null;
        private string workStatus = "";
        private double trustTotal = 0D;
        /****************************************************************************************/
        public FunPaymentDetails(string contract, string paidRecord, double amountDue, DataRow dr, bool noEdit = false )
        {
            InitializeComponent();
            workContract = contract;
            workPaidRecord = paidRecord;
            workAmountDue = amountDue;
            workDR = dr;
            workNoEdit = noEdit;
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupToolTips()
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.pictureBox12, "Add New Detail");
            tt.SetToolTip(this.pictureBox11, "Cancel Detail");
        }
        /****************************************************************************************/
        private string customerName = "";
        private string serviceId = "";
        private void LoadupTitle ()
        {
            this.Text = "Payment Details for Contract " + workContract;
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string name = dt.Rows[0]["lastName"].ObjToString() + ", " + dt.Rows[0]["firstName"].ObjToString();

            customerName = name;
            serviceId = dt.Rows[0]["serviceId"].ObjToString();
            workServiceId = serviceId;

            workNames = workDR["names"].ObjToString();

            string type = workDR["type"].ObjToString();
            DateTime dateEntered = workDR["dateEntered"].ObjToDateTime();
            double payment = workDR["payment"].ObjToDouble();

            string title = "Payment Details for (" + workContract + ") " + name;
            if (!String.IsNullOrWhiteSpace(type))
            {
                title += " for " + type;
                workType = type;
            }
            title += " entered on " + dateEntered.ToString("MM/dd/yyyy");
            title += " for " + G1.ReformatMoney(payment);
            if (type.ToUpper() != "TRUST")
            {
                gridMain.Columns["trustAmtFiled"].OptionsColumn.AllowEdit = true;
                if ( type.ToUpper() != "INSURANCE DIRECT" && type.ToUpper() != "3RD PARTY")
                {
                    gridMain.Columns["dcRequired"].Visible = false;
                    gridMain.Columns["dateDCFiled"].Visible = false;
                }
                if ( type.ToUpper() != "INSURANCE UNITY")
                    gridMain.Columns["dateReceivedFromFH"].Visible = false;
                lbltbb.Hide();
                tbb.Hide();
            }
            else
            {
                gridMain.Columns["dateReceivedFromFH"].Visible = false;
                gridMain.Columns["dcRequired"].Visible = false;
                gridMain.Columns["dateDCFiled"].Visible = false;
                gridMain.Columns["paid"].OptionsColumn.AllowEdit = false;
                gridMain.Columns["trustAmtFiled"].OptionsColumn.AllowEdit = false;
            }

            this.Text = title;

            workStatus = workDR["status"].ObjToString();
        }
        RepositoryItemSpinEdit spinEdit = new RepositoryItemSpinEdit();
        RepositoryItemCalcEdit calcEdit = new RepositoryItemCalcEdit();
        /****************************************************************************************/
        private void FunPaymentDetails_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            LoadupTitle();

            if ( G1.isHomeOffice() || G1.isAdminOrSuper())
            {
                gridMain.Columns["dateReceived"].OptionsColumn.AllowEdit = true;
                gridMain.Columns["dateFiled"].OptionsColumn.AllowEdit = true;
            }

            amountFiled = 0D;
            amountReceived = 0D;
            amountDiscount = 0D;
            amountGrowth = 0D;
            paymentStatus = "";

            lblTotalReceived.Hide();
            SetupToolTips();
            gridMain.OptionsView.AllowHtmlDrawGroups = true;
            SetupSave();
            loading = true;

            //if ( G1.isField() )
            //{
            //    gridMain.Columns["depositNumber"].OptionsColumn.ReadOnly = true;
            //    gridMain.Columns["depositNumber"].OptionsColumn.AllowEdit = false;
            //}

            LoadData();

            SetupTrustPaidCombo();

            string data = G1.ReformatMoney(workAmountDue);
            txtTotalDue.Text = "$" + data;
            txtTotalDue.Refresh();

            CalculateAmountDue();

            //dgv.RepositoryItems.AddRange(new RepositoryItem[] { spinEdit, calcEdit });
            //gridMain.Columns["checklist"].ShowButtonMode = ShowButtonModeEnum.ShowAlways;


            loading = false;

            if ( workNoEdit )
            {
                this.pictureBox11.Hide();
                this.pictureBox12.Hide();
                btnSavePayments.Hide();
            }

            if (G1.isAdminOrSuper() || G1.isHomeOffice())
            {
                gridMain.Columns["dateReceived"].OptionsColumn.AllowEdit = true;
                gridMain.Columns["trustAmtFiled"].OptionsColumn.AllowEdit = true;
            }


            DataTable dt = (DataTable)dgv.DataSource;
            if ( dt != null )
            {
                string bankAccount = dt.Rows[0]["bankAccount"].ObjToString();
                string type = dt.Rows[0]["type"].ObjToString();
                if (String.IsNullOrWhiteSpace(bankAccount))
                {
                    InitialBank(dt, type, 0);
                    dgv.Refresh();
                }
            }

            RecalcDiscountOrGrowth();

            this.Cursor = Cursors.Arrow;
        }
        /****************************************************************************************/
        private void CalculateTBB ( DataTable dt )
        {
            string status = "";
            string type = "";
            string cmd = "";
            DataTable dbrDt = null;
            double total = 0D;
            double amtFiled = 0D;

            double totalDBR = 0D;
            double dbr = 0D;
            double dbDBR = 0D;
            double oldDBR = 0D;
            double refund = 0D;

            DataRow[] dRows = null;

            DateTime deceasedDate = DateTime.MinValue;
            DateTime dateFiled = DateTime.MinValue;

            cmd = "Select * from `customers` WHERE `contractNumber` = '" + workContract + "'";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
                deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();


            string contract = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if (type == "TRUST")
                {
                    status = dt.Rows[i]["status"].ObjToString();
                    //if (status.ToUpper() == "DEPOSITED" || status.ToUpper() == "FILED" )
                    //{
                        amtFiled = dt.Rows[i]["trustAmtFiled"].ObjToDouble();
                        total += amtFiled;
                    //}
                    oldDBR = dt.Rows[i]["dbr"].ObjToDouble();
                    if ( oldDBR > 0D )
                    {
                        dRows = dt.Select("Status='CANCELLED'");
                        if (dRows.Length > 0)
                            continue;
                    }
                    dbr = dt.Rows[i]["dbr"].ObjToDouble();
                    if ( deceasedDate.Year > 1000 )
                    {
                        contract = dt.Rows[i]["contractNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(workDR["trust_policy"].ObjToString()))
                            contract = workDR["trust_policy"].ObjToString();
                        dateFiled = dt.Rows[i]["dateFiled"].ObjToDateTime();
                        if ( dateFiled.Year > 1000 )
                        {
                            double jDBR = PaymentsReport.isDBR(contract, dateFiled );
                            if (jDBR > 0D)
                                dbr = jDBR;
                        }
                    }
                    if (oldDBR != dbr)
                        dt.Rows[i]["mod"] = "Y";
                    totalDBR += dbr;

                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    refund += getPossibleRefund(contract);
                    if (!String.IsNullOrWhiteSpace(contract))
                    {
                        cmd = "Select * from `dbrs` where contractNumber = '" + contract + "';";
                        dbrDt = G1.get_db_data(cmd);
                        if ( dbrDt.Rows.Count > 0 )
                        {
                            dbDBR += dbrDt.Rows[0]["dbr"].ObjToDouble();
                        }
                    }
                }
            }
            //if ( total > 0D)
            //{
            //    tbb.Text = G1.ReformatMoney(total);
            //    tbb.Refresh();
            //}

            dbDBR = 0D;
            contract = workDR["trust_policy"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                cmd = "Select * from `dbrs` where contractNumber = '" + contract + "';";
                dbrDt = G1.get_db_data(cmd);
                if (dbrDt.Rows.Count > 0)
                {
                    for ( int i=0; i<dbrDt.Rows.Count; i++)
                        dbDBR += dbrDt.Rows[i]["dbr"].ObjToDouble();
                }

                double xDBR = PaymentsReport.isDBR(contract);
            }

            //if (dbDBR > 0D)
                totalDBR = dbDBR;

            txtDBR.Text = G1.ReformatMoney(totalDBR);
            txtDBR.Refresh();

            //contract = G1.ReformatMoney(refund);
            //txtRefund.Text = contract;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("paid", null);
            AddSummaryColumn("growth", null);
            AddSummaryColumn("discount", null);
            AddSummaryColumn("amtActuallyReceived", null);
            AddSummaryColumn("trustAmtFiled", null);
            //AddSummaryColumn("currentprice", null);
            //AddSummaryColumn("difference", null);

            gridMain.Columns["paid"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["paid"].SummaryItem.DisplayFormat = "{0:C2}";
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + workContract + "' and `paymentRecord` = '" + workPaidRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("initialized");
            dt.Columns.Add("names");

            AddMod(dt, gridMain);

            GetInitialMoney(dt);

            SetupSelection(dt, repositoryItemCheckEdit1, "discresionaryACH");
            SetupSelection(dt, repositoryItemCheckEdit2, "pmtInTransition");

            bool modified = LoadCurrentPaymentMaybe(dt);

            CalculateTBB(dt);
            
            if ( G1.isAdminOrSuper() || G1.isHomeOffice() )
            {
                gridMain.Columns["trustAmtFiled"].OptionsColumn.AllowEdit = true;
                gridMain.Columns["paid"].OptionsColumn.AllowEdit = true;
            }


            dgv.DataSource = dt;

            if (workType.ToUpper() == "TRUST" && dt.Rows.Count == 1 )
                gridMain.Columns["trustAmtFiled"].OptionsColumn.AllowEdit = false;
            else
                gridMain.Columns["trustAmtFiled"].OptionsColumn.AllowEdit = true;

            if (workType.ToUpper() != "TRUST")
                gridMain.Columns["paidFrom"].OptionsColumn.AllowEdit = false;


            funModified = false;
            if ( modified && !workNoEdit )
            {
                funModified = true;
                btnSavePayments.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private bool LoadCurrentPaymentMaybe(DataTable dt)
        {
            bool found = false;
            bool modified = false;
            string status = "";
            string record = "";
            string locind = "";
            double dbr = 0D;
            double oldDBR = 0D;
            bool gotDBR = false;
            string str = "";

            double existingTrust = 0D;
            if ( G1.get_column_number ( dt, "trustAmtFiled") < 0 )
                dt.Columns.Add ("trustAmtFiled", Type.GetType("System.Double"));

            DataRow[] dRows = null;
            string refundTrust = "";
            double dValue = 0D;
            double refund = 0D;
            double unityRefund = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["names"] = workNames;
                record = dt.Rows[i]["paymentRecord"].ObjToString();
                if (record == workPaidRecord)
                {
                    found = true;

                    if (workDR["type"].ObjToString().ToUpper() == "TRUST")
                    {
                        double beginningBalance = 0D;
                        double endingBalance = 0D;
                        double trust85Pending = 0D;
                        if (G1.get_column_number(dt, "trust_policy") >= 0)
                        {
                            string contract = dt.Rows[i]["trust_policy"].ObjToString();
                            contract = contract.Replace("DBR", "");
                            contract = contract.Replace("-", "");
                            contract = contract.Replace("(", "");
                            contract = contract.Replace(")", "").Trim();
                            //                        CalcTrust2013(workContract, ref endingBalance, ref trust85Pending);
                            unityRefund = getPossibleRefund(contract);
                            CalcTrust2013(contract, ref endingBalance, ref trust85Pending, ref beginningBalance, ref locind );
                            if (endingBalance == 0D)
                                endingBalance = beginningBalance;
                            double totalTrust = endingBalance + trust85Pending;
                            totalTrust = G1.RoundValue(totalTrust);
                            existingTrust = dt.Rows[i]["trustAmtFiled"].ObjToDouble();
                            if ( existingTrust == 0D )
                                dt.Rows[i]["trustAmtFiled"] = totalTrust; // Dont do Unity Refund at this time. Need Confirmation from Cliff/Charlotte
                                //dt.Rows[i]["trustAmtFiled"] = totalTrust - unityRefund;
                            str = G1.ReformatMoney(totalTrust);
                            tbb.Text = str;

                            double newTBB = TrustDeceased.GetCummulativeTBB(contract);
                            tbb.Text = G1.ReformatMoney(newTBB);

                            trustTotal = totalTrust;

                            double paid = dt.Rows[i]["paid"].ObjToDouble();
                            if (paid <= 0D)
                                dt.Rows[i]["paid"] = workDR["payment"].ObjToDouble();
                            oldDBR = dt.Rows[i]["dbr"].ObjToDouble();
                            dbr = DailyHistory.GetPossibleDBR(contract);

                            dRows = dt.Select("status='Cancelled'");
                            if (dRows.Length <= 0)
                            {
                                dt.Rows[i]["dbr"] = dbr;
                                gotDBR = true;
                                if (oldDBR != dbr)
                                    dt.Rows[i]["mod"] = "Y";
                            }
                            //refund += getPossibleRefund(contract);
                            refund += unityRefund;
                        }
                        else
                        {
                            string contract = workDR["trust_policy"].ObjToString();
                            contract = contract.Replace("DBR", "");
                            contract = contract.Replace("-", "");
                            contract = contract.Replace("(", "");
                            contract = contract.Replace(")", "").Trim();
                            CalcTrust2013Actual(contract, ref endingBalance, ref trust85Pending, ref beginningBalance, ref locind );
                            if (endingBalance == 0D)
                                endingBalance = beginningBalance;
                            double totalTrust = endingBalance + trust85Pending;
                            totalTrust = G1.RoundValue(totalTrust);
                            existingTrust = dt.Rows[i]["trustAmtFiled"].ObjToDouble();
                            if (existingTrust == 0D)
                                dt.Rows[i]["trustAmtFiled"] = totalTrust;
                            str = G1.ReformatMoney(totalTrust);
                            tbb.Text = str;

                            double newTBB = TrustDeceased.GetCummulativeTBB(contract);
                            tbb.Text = G1.ReformatMoney ( newTBB );

                            double paid = dt.Rows[i]["paid"].ObjToDouble();
                            if (paid <= 0D)
                                dt.Rows[i]["paid"] = workDR["payment"].ObjToDouble();
                            oldDBR = dt.Rows[i]["dbr"].ObjToDouble();
                            dbr = DailyHistory.GetPossibleDBR(contract);
                            dt.Rows[i]["dbr"] = dbr;
                            gotDBR = true;
                            if (oldDBR != dbr)
                                dt.Rows[i]["mod"] = "Y";
                            refund += getPossibleRefund(contract);
                        }
                    }
                    else if (workDR["type"].ObjToString().ToUpper() == "INSURANCE UNITY")
                    {
                        dt.Rows[i]["paidFrom"] = "UNITY";
                    }
                    else if (workDR["type"].ObjToString().ToUpper() == "INSURANCE DIRECT")
                    {
                        dt.Rows[i]["paidFrom"] = "FILED DIRECT";
                    }

                    status = workDR["status"].ObjToString();
                    paymentStatus = status;
                    if (status.ToUpper() == "DEPOSITED")
                        status = "Deposited";
                    if (dt.Rows[i]["status"].ObjToString().ToUpper() != status.ToUpper() )
                    {
                        dt.Rows[i]["status"] = status;
                        dt.Rows[i]["mod"] = "Y";
                        modified = true;
                        if ( !workNoEdit)
                            btnSavePayments.Show();
                    }
                    break;
                }
            }
            if ( paymentStatus.ToUpper() == "CANCELLED")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["status"] = paymentStatus;
                    dt.Rows[i]["mod"] = "Y";
                    modified = true;
                    if (!workNoEdit)
                        btnSavePayments.Show();
                }
            }
            //if (found == true)
            //{
            //    string contract = workDR["trust_policy"].ObjToString();
            //    LoadDBR_Detail( contract, dt);

            //    return modified;
            //}
            string type = "";
            status = "";
            if (!found)
            {
                DataRow dr = dt.NewRow();
                if (!String.IsNullOrWhiteSpace(workPaidRecord))
                    dr["paymentRecord"] = workPaidRecord;
                if (workDR != null)
                {
                    status = workDR["status"].ObjToString();
                    if (status.ToUpper() == "DEPOSITED" || status.ToUpper() == "ACCEPT")
                        status = "Deposited";
                    type = workDR["type"].ObjToString();
                    if (type.ToUpper() == "CHECK")
                        type = "CHECK-LOCAL";
                    dr["names"] = workNames;
                    dr["status"] = status;
                    dr["type"] = type;
                    dr["localDescription"] = workDR["localDescription"].ObjToString();
                    dr["bankaccount"] = workDR["bankAccount"].ObjToString();
                    dr["discresionaryACH"] = "0";
                    dr["pmtInTransition"] = "0";
                    dr["paid"] = workDR["payment"].ObjToDecimal();
                    dr["dateModified"] = G1.DTtoMySQLDT(workDR["dateEntered"].ObjToDateTime());
                    dr["lastUser"] = LoginForm.username;
                    if (type.ToUpper() == "CASH")
                    {
                        dr["dateFiled"] = G1.DTtoMySQLDT(workDR["dateEntered"].ObjToDateTime());
                        dr["dateReceived"] = G1.DTtoMySQLDT(workDR["dateEntered"].ObjToDateTime());
                    }
                    if (type.ToUpper() == "TRUST")
                    {
                        double beginningBalance = 0D;
                        double endingBalance = 0D;
                        double trust85Pending = 0D;
                        string contract = workDR["trust_policy"].ObjToString();
                        if (String.IsNullOrWhiteSpace(contract))
                            contract = workContract;
                        //                    CalcTrust2013(workContract, ref endingBalance, ref trust85Pending);
                        unityRefund = getPossibleRefund(contract);
                        CalcTrust2013(contract, ref endingBalance, ref trust85Pending, ref beginningBalance, ref locind);
                        if (endingBalance == 0D)
                            endingBalance = beginningBalance;
                        double totalTrust = endingBalance + trust85Pending;
                        DateTime dateEntered = workDR["dateEntered"].ObjToDateTime();
                        dbr = PaymentsReport.isDBR(contract, dateEntered );
                        if (dbr > 0D)
                            totalTrust = totalTrust - dbr;
                        totalTrust = G1.RoundValue(totalTrust);
                        str = G1.ReformatMoney(totalTrust);
                        tbb.Text = str;

                        dr["trustAmtFiled"] = totalTrust; // Dont do this yet/ Need Confirmation from Cliff/Charlotte
                        //dr["trustAmtFiled"] = totalTrust - unityRefund;

                        //dbr = DailyHistory.GetPossibleDBR(contract);
                        dr["dbr"] = dbr;
                        dr["mod"] = "Y";
                        gotDBR = true;

                        refund += unityRefund;
                    }
                    else if (type.ToUpper() == "INSURANCE UNITY")
                    {
                        dr["paidFrom"] = "UNITY";
                    }
                    else if (type.ToUpper() == "INSURANCE DIRECT")
                    {
                        dr["paidFrom"] = "FILED DIRECT";
                    }

                }
                dt.Rows.Add(dr);
            }

            if (gotDBR)
            {
                string contract = workDR["trust_policy"].ObjToString();
                LoadDBR_Detail(contract, dt);

                str = G1.ReformatMoney(refund);
                txtRefund.Text = str;

                return modified;
            }

            if (type.ToUpper() == "TRUST")
                CalculateTBB(dt);

            str = G1.ReformatMoney(refund);
            txtRefund.Text = str;

            SetupSave();
            return funModified;
        }
        /***********************************************************************************************/
        public static double getPossibleRefund ( string contract )
        {
            double refund = 0D;

            if ( contract == "N16007UI")
            {
            }

            bool doit = false;
            if (contract.ToUpper().EndsWith("UI"))
                doit = true;
            else if (contract.ToUpper().EndsWith("U"))
                doit = true;
            if (!doit)
                return refund;

            string cmd = "Select * from `unityrefunds` WHERE `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                double dValue = 0D;
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    dValue = dt.Rows[i]["unityRefund"].ObjToDouble();
                    refund += dValue;
                }
            }
            return refund;
        }
        /***********************************************************************************************/
        private void LoadDBR_Detail( string contractNumber, DataTable dt )
        {
            string cmd = "";
            double DBR = 0D;
            DataTable dx = null;
            DataRow dR = null;
            DataRow[] dRows = null;
            DataTable ddt = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DBR = dt.Rows[i]["dbr"].ObjToDouble();
                if (DBR == 0D)
                    continue;

                dRows = dt.Select("status='CANCELLED'");
                if (dRows.Length > 0)
                    continue;

                cmd = "Select * from `customers` a JOIN `contracts` b on a.`contractNumber` = b.`contractNumber` where a.`contractNumber` = '" + contractNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;

                string firstName = dx.Rows[0]["firstName"].ObjToString();
                string lastName = dx.Rows[0]["lastName"].ObjToString();

                DateTime deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();

                int deceasedYear = deceasedDate.Year;
                if (deceasedYear <= 1000)
                    continue;
                int deceasedMonth = deceasedDate.Month;

                DateTime startDate = new DateTime(deceasedYear, deceasedMonth, 1);

                cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "'  AND `payDate8` >= '" + startDate.ToString("yyyy-MM-dd") + "' ORDER BY `payDate8` DESC, `tmstamp` DESC;";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;

                double dbr = 0D;
                double downPayment = 0D;
                string bank = "";

                string[] Lines = null;
                string routingNumber = "";
                string bankAccount = "";
                string depositNumber = "";
                string location = "";

                DataTable bankDt = null;
                DataTable ddx = null;

                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    dbr = dx.Rows[j]["trust85P"].ObjToDouble();
                    downPayment = DailyHistory.getDownPayment(dx, j);
                    if ( dbr > 0D )
                    {
                        dR = ddt.NewRow();
                        dR["type"] = "DBR";
                        dR["amtActuallyReceived"] = dbr;
                        dR["dateReceived"] = dx.Rows[j]["payDate8"];
                        dR["discresionaryACH"] = "0";
                        dR["pmtInTransition"] = "0";
                        dR["status"] = "DBR";
                        depositNumber = dx.Rows[j]["depositNumber"].ObjToString();
                        dR["depositNumber"] = depositNumber;
                        location = dx.Rows[j]["location"].ObjToString();

                        cmd = "Select * from `downpayments` WHERE `lastName` = '" + lastName + "' and `depositNumber` = '" + depositNumber + "';";
                        ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count <= 0)
                            location = DailyHistory.DetermineBox(dx, j, 4);
                        else
                            location = "TD";

                        dR["type"] = location;

                        bank = dx.Rows[j]["bank_account"].ObjToString();

                        Lines = bank.Split('~');

                        if (Lines.Length >= 3)
                        {
                            routingNumber = Lines[1].Trim();
                            bankAccount = Lines[2].Trim();

                            cmd = "Select * from `bank_accounts` WHERE `general_ledger_no` = '" + routingNumber + "' AND `account_no` = '" + bankAccount + "';";
                            bankDt = G1.get_db_data(cmd);
                            if ( bankDt.Rows.Count > 0 )
                            {
                                dR["bankAccount"] = bankAccount;
                                dR["localDescription"] = bankDt.Rows[0]["localDescription"].ObjToString();
                            }
                        }


                        ddt.Rows.Add(dR);
                    }
                }
            }

            for (int i = 0; i < ddt.Rows.Count; i++)
                dt.ImportRow(ddt.Rows[i]);

            return;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew, string column)
        {
            bool saveLoad = loading;
            loading = true;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            string set = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                set = dt.Rows[i][column].ObjToString();
                if (set != "1")
                    dt.Rows[i][column] = "0";
            }
            loading = saveLoad;
        }
        /***********************************************************************************************/
        private void CalculateAmountDue()
        {
            double totalPaid = 0D;
            string status = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString().ToUpper();
                if (status == "DEPOSITED")
                    totalPaid += dt.Rows[i]["paid"].ObjToDouble();
            }

            if (totalPaid >= workAmountDue)
                lblTotalReceived.Show();
            else
                lblTotalReceived.Hide();
            lblTotalReceived.Refresh();

            CalculateTBB(dt);
        }
        /***********************************************************************************************/
        private double ResolveImportedData()
        {
            double goods = 0D;
            string cmd = "Select * from `customers` where `contractNumber` = '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return goods;

            string group = EditCustomer.activeFuneralHomeCasketGroup;
            if (String.IsNullOrWhiteSpace(group))
                group = "Casket Group 3.3";

            string casketCode = dx.Rows[0]["extraItemAmtMI1"].ObjToString();
            string vaultCode = dx.Rows[0]["extraItemAmtMI2"].ObjToString();
            double casketPrice = dx.Rows[0]["extraItemAmtMR1"].ObjToDouble();
            double vaultPrice = dx.Rows[0]["extraItemAmtMR2"].ObjToDouble();
            goods += casketPrice + vaultPrice;
            return goods;
        }
        ///****************************************************************************************/
        //private DataTable saveDt = null;
        //private void pictureBox3_Click(object sender, EventArgs e)
        //{
        //}
        /***************************************************************************************/
        public DataTable FireEventFunPaymentsReturn()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            return dt;
        }
        /***************************************************************************************/
        public bool FireEventFunServicesModified()
        {
            if (funModified)
                return true;
            return false;
        }
        /***************************************************************************************/
        public void FireEventSaveFunServices(bool save = false)
        {
            if (save && funModified)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                SaveCustomerPayments(dt);
            }
            this.Close();
        }
        /****************************************************************************************/
        private void panelClaimTop_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelClaimTop.Bounds;
            Graphics g = panelClaimTop.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width - 1;
            int high = rect.Height - 1;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /****************************************************************************************/
        private void panelBottom_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = panelBottom.Bounds;
            Graphics g = panelBottom.CreateGraphics();
            Pen pen = new Pen(Brushes.Black);
            int left = rect.Left;
            int top = rect.Top;
            int width = rect.Width - 1;
            int high = rect.Height - 1;
            g.DrawRectangle(pen, left, top, width, high);
        }
        /***********************************************************************************************/
        private void CheckForSaving()
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            if (!funModified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nPayments have been modified!\nWould you like to SAVE your Payments?", "Payments Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (loading)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string type = dt.Rows[row]["type"].ObjToString().ToUpper();
            string status = dt.Rows[row]["status"].ObjToString().ToUpper();
            if ( LoginForm.classification.ToUpper() == "FIELD")
            {
                if ( status == "ADDED")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if ( status == "DEBIT")
            {
                string mod = dt.Rows[row]["mod"].ObjToString();
                if ( mod.ToUpper() == "D")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /****************************************************************************************/
        private void btnSavePayments_Click(object sender, EventArgs e)
        {
            if (validateClosing())
            {
                DataTable dt = (DataTable)dgv.DataSource;
                SaveCustomerPayments(dt);
                btnSavePayments.Hide();
            }
        }
        /***********************************************************************************************/
        private void SaveCustomerPayments(DataTable dt)
        {
            string desc = "";
            string price = "";
            string type = "";
            string status = "";
            string record = "";
            string assignmentFrom = "";
            string bankAccount = "";
            string localDescription = "";
            string discresionaryACH = "";
            string pmtInTransition = "";
            string depositNumber = "";
            string notes = "";
            string dcRequired = "";
            string mod = "";
            DateTime dateDCFiled = DateTime.Now;
            DateTime dateReceivedFromFH = DateTime.Now;
            double paid = 0D;
            double trustAmtFiled = 0D;
            double amtActuallyReceived = 0D;
            double growth = 0D;
            double discount = 0D;
            double dbr = 0D;
            DateTime dateFiled = DateTime.Now;
            DateTime dateReceived = DateTime.Now;
            DateTime dateModified = DateTime.Now;
            string lastUser = "";
            amountFiled = 0D;
            amountReceived = 0D;
            amountDiscount = 0D;
            amountGrowth = 0D;
            amountDebit = 0D;
            depositNumber = "";
            string cmd = "";
            DataRow[] dRows = null;
            //string cmd = "Delete from `cust_payments` where `contractNumber` = '" + workContract + "';";
            //G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    status = dt.Rows[i]["status"].ObjToString();
                    if (status.ToUpper() == "DBR")
                        continue;
                    record = dt.Rows[i]["record"].ObjToString();
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod.ToUpper() == "D")
                    {
                        if (record == "0" || String.IsNullOrWhiteSpace(record))
                            continue;
                        G1.delete_db_table("cust_payment_details", "record", record);
                        dt.Rows[i]["record"] = 0;
                        continue;
                    }
                    if (String.IsNullOrWhiteSpace(record))
                    {
                        record = G1.create_record("cust_payment_details", "comment", "-1");
                        dt.Rows[i]["mod"] = "Y";
                    }
                    if (G1.BadRecord("cust_payment_details", record))
                        break;
                    dt.Rows[i]["record"] = record;
                    desc = dt.Rows[i]["comment"].ObjToString();
                    status = dt.Rows[i]["status"].ObjToString();
                    if (i == 0)
                        paymentStatus = status;
                    price = dt.Rows[i]["paid"].ObjToString();
                    trustAmtFiled = dt.Rows[i]["trustAmtFiled"].ObjToDouble();
                    type = dt.Rows[i]["type"].ObjToString();
                    assignmentFrom = dt.Rows[i]["paidFrom"].ObjToString();
                    dateFiled = dt.Rows[i]["dateFiled"].ObjToDateTime();
                    dateReceived = dt.Rows[i]["dateReceived"].ObjToDateTime();
                    discresionaryACH = dt.Rows[i]["discresionaryACH"].ObjToString();
                    pmtInTransition = dt.Rows[i]["pmtInTransition"].ObjToString();
                    bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                    localDescription = dt.Rows[i]["localDescription"].ObjToString();
                    lastUser = dt.Rows[i]["lastUser"].ObjToString();
                    dateModified = dt.Rows[i]["dateModified"].ObjToDateTime();
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    amtActuallyReceived = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    growth = dt.Rows[i]["growth"].ObjToDouble();
                    discount = dt.Rows[i]["discount"].ObjToDouble();
                    notes = dt.Rows[i]["notes"].ObjToString();
                    dbr = dt.Rows[i]["dbr"].ObjToDouble();
                    if ( dbr > 0D )
                    {
                        dRows = dt.Select("Status='CANCELLED'");
                        if (dRows.Length > 0)
                        {
                            dbr = 0D;
                            dt.Rows[i]["mod"] = "Y";
                        }
                    }

                    if (dt.Rows[i]["mod"].ObjToString().ToUpper() == "Y")
                    {
                        G1.update_db_table("cust_payment_details", "record", record, new string[] { "comment", desc, "type", type, "contractNumber", workContract, "paid", price, "status", status, "paidFrom", assignmentFrom, "dateFiled", dateFiled.ToString("yyyy-MM-dd"), "dateReceived", dateReceived.ToString("yyyy-MM-dd"), "paymentRecord", workPaidRecord, "pmtInTransition", discresionaryACH, "bankAccount", bankAccount, "localDescription", localDescription, "lastUser", lastUser, "dateModified", dateModified.ToString("yyyy-MM-dd"), "depositNumber", depositNumber, "trustAmtFiled", trustAmtFiled.ToString(), "amtActuallyReceived", amtActuallyReceived.ToString(), "notes", notes, "discount", growth.ToString(), "discount", discount.ToString(), "dbr", dbr.ToString(), "pmtInTransition", pmtInTransition });

                        if (workType.ToUpper() == "INSURANCE DIRECT" || workType.ToUpper() == "3RD PARTY")
                        {
                            dcRequired = dt.Rows[i]["dcRequired"].ObjToString();
                            dateDCFiled = dt.Rows[i]["dateDCFiled"].ObjToDateTime();
                            G1.update_db_table("cust_payment_details", "record", record, new string[] { "dcRequired", dcRequired, "dateDCFiled", dateDCFiled.ToString("yyyy-MM-dd") });
                        }

                        if (workType.ToUpper() == "INSURANCE UNITY")
                        {
                            dateReceivedFromFH = dt.Rows[i]["dateReceivedFromFH"].ObjToDateTime();
                            G1.update_db_table("cust_payment_details", "record", record, new string[] { "dateReceivedFromFH", dateReceivedFromFH.ToString("yyyy-MM-dd") });
                        }
                    }

                    paid = price.ObjToDouble();
                    if (status.ToUpper() == "FILED")
                        amountFiled += trustAmtFiled;
                    else if (status.ToUpper() == "DEPOSITED")
                    {
                        amountFiled += trustAmtFiled;
                        amountReceived += amtActuallyReceived;
                    }
                    else if (status.ToUpper() == "DEBIT")
                        amountDebit += paid;
                    if (type.ToUpper() == "DISCOUNT" && status.ToUpper() == "ADDED")
                        amountDiscount += paid;
                    //if (type.ToUpper() == "GROWTH" && status.ToUpper() == "ADDED")
                    amountGrowth += growth;

                    if (!String.IsNullOrWhiteSpace(depositNumbers))
                        depositNumbers += ", ";

                    depositNumbers += depositNumber;
                }
                catch (Exception ex)
                {
                }
            }

            G1.update_db_table("cust_payments", "record", workPaidRecord, new string[] { "amountFiled", amountFiled.ToString(), "amountReceived", amountReceived.ToString() });
            dataSaved = true;

            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            funModified = false;
            //NotifyContract(dt);
        }
        /***********************************************************************************************/
        private void NotifyContract(DataTable dt)
        {
            for (int i = 0; i < Application.OpenForms.Count; i++)
            {
                var form = Application.OpenForms[i];
                if (form.Visible)
                {
                    string text = form.Name.ObjToString();
                    if (text.ToUpper().IndexOf("CONTRACT1") >= 0)
                    {
                        text = form.Text;
                        Contract1 editForm = (Contract1)form;
                        string contract = editForm.myWorkContract;
                        if (contract == workContract)
                        {
                            editForm.FireEventFunPaymentsChanged(contract, dt);
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        public void FireEventFunServicesChanged(string contract, DataTable dt)
        {
            if (contract == workContract)
            {
                LoadData();
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
                return;
            }
            int rowHandle = e.RowHandle;
            if (rowHandle < 0)
                return;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dgv.DataSource == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;

            bool protectField = false;
            string type = dt.Rows[row]["type"].ObjToString().ToUpper();
        }
        /****************************************************************************************/
        private void btnShowPDF_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (e == null)
                return;
            bool doBanks = false;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            dr["mod"] = 'Y';
            if (e.Column.FieldName.Trim().ToUpper() == "STATUS")
            {
                string status = dr["status"].ObjToString();
                CalculateAmountDue();
                if (status.ToUpper() == "DEPOSITED")
                    doBanks = true;
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "LOCALDESCRIPTION")
            {
                string status = dr["status"].ObjToString();
                if (status.ToUpper() == "DEPOSITED")
                    doBanks = true;
            }

            if (doBanks)
            {
                string set = dr["discresionaryACH"].ObjToString();
                if (set != "1")
                {
                    string bankAccount = GetDepositBankAccount("discresionaryACH");
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        dr["bankAccount"] = bankAccount;
                        dt.Rows[row]["bankAccount"] = bankAccount;
                    }
                }
                else
                {
                    string bankAccount = GetDepositBankAccount("trustDeathClaims");
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        dr["bankAccount"] = bankAccount;
                        dt.Rows[row]["bankAccount"] = bankAccount;
                    }
                }
            }

            CheckForDiscountOrGrowth();

            CalculateAmountDue();

            SetupSave();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void SetupSave()
        {
            if (workNoEdit)
                return;
            funModified = true;
            btnSavePayments.Show();
            btnSavePayments.Refresh();
        }
        /****************************************************************************************/
        private void RecalcDiscountOrGrowth ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            for (int i = 0; i < dt.Rows.Count; i++)
                CheckForDiscountOrGrowth(i);
        }
        /****************************************************************************************/
        private void CheckForDiscountOrGrowth( int doRow = -1 )
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = null;
            int row = doRow;
            if (row < 0)
            {
                int rowHandle = gridMain.FocusedRowHandle;
                row = gridMain.GetDataSourceRowIndex(rowHandle);
                dr = gridMain.GetFocusedDataRow();
            }
            else
                dr = dt.Rows[row];
            if (dr == null)
                return;
            string status = dr["status"].ObjToString().ToUpper();
            if ( status == "DBR")
            {
                dr["discount"] = 0D;
                dt.Rows[row]["discount"] = 0D;
                dr["growth"] = 0D;
                dt.Rows[row]["growth"] = 0D;
                dt.AcceptChanges();
                return;
            }
            //if (type != "INSURANCE UNITY" && type != "INSURANCE DIRECT" && type != "TRUST")
            //    return;
            status = dr["status"].ObjToString().ToUpper();

            string type = dr["type"].ObjToString().ToUpper();
            string bankType = G1.force_lower_line(type);

            double trustAmtFiled = dr["trustAmtFiled"].ObjToDouble();
            double amtActuallyReceived = dr["amtActuallyReceived"].ObjToDouble();
            double growth = trustAmtFiled - amtActuallyReceived;
            if (amtActuallyReceived != 0D)
            {
                if (growth > 0D)
                {
                    dr["discount"] = growth;
                    dt.Rows[row]["discount"] = growth;
                    dr["growth"] = 0D;
                    dt.Rows[row]["growth"] = 0D;
                }
                else
                {
                    growth = Math.Abs(growth);
                    dr["growth"] = growth;
                    dt.Rows[row]["growth"] = growth;
                    dr["discount"] = 0D;
                    dt.Rows[row]["discount"] = 0D;
                }
            }
            dt.AcceptChanges();
        }
        /****************************************************************************************/
        private string GetDepositBankAccount(string paymentType)
        {
            string location = EditCustomer.activeFuneralHomeName;
            string bankAccount = "";
            if (!String.IsNullOrWhiteSpace(location))
            {
                string cmd = "Select * from `funeralhomes` where `locationCode` = '" + location + "';";
                if (DailyHistory.gotCemetery(location))
                    cmd = "Select * from `cemeteries` where `loc` = '" + location + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    if (paymentType.ToUpper() == "TRUST")
                        paymentType = "trustDeathClaims";
                    if (String.IsNullOrWhiteSpace(paymentType))
                        paymentType = "CASH";
                    if (paymentType.ToUpper() == "TRUSTDEATHCLAIMS")
                        bankAccount = SetupMainBanks("trustDeathClaims");
                    else if (paymentType.ToUpper() == "DISCRESIONARYACH")
                        bankAccount = SetupMainBanks("discresionaryACH");
                    else if (paymentType.ToUpper() == "DISCRETIONARY ACH")
                        bankAccount = SetupMainBanks("discresionaryACH");
                    else if (paymentType.ToUpper() == "CASH")
                        bankAccount = SetupBanks(dt, "cashLocal");
                    else if (paymentType.ToUpper() == "CHECK-LOCAL")
                        bankAccount = SetupBanks(dt, "checkLocal");
                    else if (paymentType.ToUpper() == "INSURANCE UNITY")
                        bankAccount = SetupMainBanks("insUnity");
                    //else if (paymentType.ToUpper() == "INSURANCE DIRECT")
                    //    bankAccount = SetupMainBanks("insUnity");
                    else if (paymentType.ToUpper() == "CHECK-REMOTE")
                        bankAccount = SetupMainBanks("checkRemote");
                    else if (paymentType.ToUpper() == "CREDIT CARD")
                        bankAccount = SetupMainBanks("funeral");
                    else
                    {
                        gridMain.Columns["localDescription"].ColumnEdit = null;
                        int focusedRow = gridMain.FocusedRowHandle;

                        int row = gridMain.GetDataSourceRowIndex(focusedRow);
                        gridMain.SelectRow(focusedRow);

                        DataRow dr = gridMain.GetFocusedDataRow();
                        dr["bankAccount"] = "";
                        dr["localDescription"] = "";

                        dt = (DataTable)dgv.DataSource;
                        dt.Rows[row]["bankAccount"] = "";
                        dt.Rows[row]["localDescription"] = "";
                        gridMain.RefreshData();
                        gridMain.RefreshEditor(true);
                    }

                }
            }
            return bankAccount;
        }
        /***********************************************************************************************/
        private string SetupBanks(DataTable dt, string what)
        {
            string bankAccount = "";
            string bankList = "";
            string cmd = "";
            string[] account = null;
            string str = dt.Rows[0][what].ObjToString();
            str = str.TrimEnd('~');
            if (!String.IsNullOrWhiteSpace(str))
            {
                DataTable dx = null;
                string record = "";
                string general_ledger_no = "";
                string account_no = "";
                string[] Lines = str.Split('~');
                for (int i = 0; i < Lines.Length; i++)
                {
                    record = Lines[i].Trim();
                    account = record.Split('/');
                    if (account.Length < 2)
                        continue;
                    general_ledger_no = account[0].Trim();
                    account_no = account[1].Trim();
                    //cmd = "Select * from `bank_accounts` where `record` = '" + record + "';";
                    cmd = "Select * from `bank_accounts` where `general_ledger_no` = '" + general_ledger_no + "' and `account_no` = '" + account_no + "';";
                    //cmd = "Select * from `bank_accounts` where `account_no` = '" + account_no + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        bankList += dx.Rows[0]["localDescription"].ObjToString() + "~";
                        if (String.IsNullOrWhiteSpace(bankAccount))
                            bankAccount = dx.Rows[0]["account_no"].ObjToString();
                    }
                }
                FixBank(bankList);
            }
            return bankAccount;
        }
        /***********************************************************************************************/
        private string GetBankDiscountGrowth(string what )
        {
            string bankAccount = "";
            string bankList = "";
            string localDescription = "";
            if (bankDt == null)
            {
                bankDt = G1.get_db_data("Select * from `bank_accounts`;");
                for (int i = 0; i < bankDt.Rows.Count; i++)
                {
                    localDescription = bankDt.Rows[i]["localDescription"].ObjToString();
                    if (String.IsNullOrWhiteSpace(localDescription))
                        bankDt.Rows[i]["localDescription"] = bankDt.Rows[i]["account_title"].ObjToString();
                }
            }
            if (bankDt.Rows.Count <= 0)
                return bankAccount;

            DataRow[] dRows = bankDt.Select("location='" + what + "'");
            if (dRows.Length > 0)
                bankAccount = dRows[0]["account_no"].ObjToString();

            return bankAccount;
        }
        /***********************************************************************************************/
        private DataTable bankDt = null;
        private string SetupMainBanks(string what)
        {
            string bankAccount = "";
            string bankList = "";
            string localDescription = "";
            if (bankDt == null)
            {
                bankDt = G1.get_db_data("Select * from `bank_accounts` ORDER BY `order`;");
                for (int i = 0; i < bankDt.Rows.Count; i++)
                {
                    localDescription = bankDt.Rows[i]["localDescription"].ObjToString();
                    if (String.IsNullOrWhiteSpace(localDescription))
                        bankDt.Rows[i]["localDescription"] = bankDt.Rows[i]["account_title"].ObjToString();
                }
            }
            if (bankDt.Rows.Count <= 0)
                return bankAccount;
            DataRow[] dRows = bankDt.Select(what + "='1'");
            for (int i = 0; i < dRows.Length; i++)
            {
                bankList += dRows[i]["localDescription"].ObjToString() + "~";
                if (String.IsNullOrWhiteSpace(bankAccount))
                    bankAccount = dRows[i]["account_no"].ObjToString();
            }

            if (!String.IsNullOrWhiteSpace(bankList))
                FixBank(bankList, what );

            return bankAccount;
        }
        /***********************************************************************************************/
        RepositoryItemComboBox ciLookup2 = null;
        private void FixBank(string bankList, string paymentType = "")
        {
            bankList = bankList.TrimEnd('~');
            string[] Lines = bankList.Split('~');

            if (ciLookup2 == null)
            {
                ciLookup2 = new RepositoryItemComboBox();
                ciLookup2.SelectedIndexChanged += repositoryItemComboBox4_SelectedIndexChanged;
            }

            int focusedRow = gridMain.FocusedRowHandle;

            int row = gridMain.GetDataSourceRowIndex(focusedRow);
            gridMain.SelectRow(focusedRow);

            DataTable dt = (DataTable)dgv.DataSource;
            string saveLocation = dt.Rows[row]["localDescription"].ObjToString();
            dt.Rows[row]["initialized"] = "1";

            ciLookup2.Items.Clear();
            string localDescription = "";
            string saveDescription = "";
            bool first = true;
            for (int i = 0; i < Lines.Length; i++)
            {
                localDescription = Lines[i].Trim();
                if (!String.IsNullOrWhiteSpace(localDescription))
                {
                    ciLookup2.Items.Add(localDescription);
                    if (first)
                    {
                        dt.Rows[row]["localDescription"] = localDescription;
                        saveDescription = localDescription;
                    }
                    first = false;
                }
            }
            gridMain.Columns["localDescription"].ColumnEdit = ciLookup2;

            //if (!String.IsNullOrWhiteSpace(paymentType))
            //{
            //    string location = EditCustomer.activeFuneralHomeName;
            //    if (!String.IsNullOrWhiteSpace(location))
            //    {
            //        string bankAccount = GetDepositBankAccount(paymentType);
            //        dt.Rows[row]["localDescription"] = saveLocation;
            //        if (!String.IsNullOrWhiteSpace(saveDescription))
            //        {
            //            string cmd = "Select * from `bank_accounts` where `localDescription` = '" + saveDescription + "';";
            //            DataTable dx = G1.get_db_data(cmd);
            //            if (dx.Rows.Count <= 0)
            //            {
            //                cmd = "Select * from `bank_accounts` where `account_title` = '" + saveDescription + "';";
            //                dx = G1.get_db_data(cmd);
            //            }
            //            if (dx.Rows.Count > 0)
            //            {
            //                bankAccount = dx.Rows[0]["account_no"].ObjToString();
            //                dt.Rows[row]["bankAccount"] = bankAccount;
            //                gridMain.RefreshData();
            //            }
            //        }
            //    }
            //}
        }
        /***********************************************************************************************/
        public static bool Mod10Check(string creditCardNumber)
        { //// check whether input string is null or empty 
            if (string.IsNullOrEmpty(creditCardNumber)) { return false; }
            //// 1. Starting with the check digit double the value of every other digit 
            //// 2. If doubling of a number results in a two digits number, add up 
            /// the digits to get a single digit number. This will results in eight single digit numbers 
            /// //// 3. Get the sum of the digits 
            int sumOfDigits = creditCardNumber.Where((e) => e >= '0' && e <= '9').Reverse().Select((e, i) => ((int)e - 48) * (i % 2 == 0 ? 1 : 2)).Sum((e) => e / 10 + e % 10);
            //// If the final sum is divisible by 10, then the credit card number 
            // is valid. If it is not divisible by 10, the number is invalid. 
            return sumOfDigits % 10 == 0;
        }
        /****************************************************************************************/
        public enum CardType
        {
            Unknown = 0,
            MasterCard = 1,
            VISA = 2,
            Amex = 3,
            Discover = 4,
            DinersClub = 5,
            JCB = 6,
            enRoute = 7
        }
        /****************************************************************************************/
        // Class to hold credit card type information
        private class CardTypeInfo
        {
            public CardTypeInfo(string regEx, int length, CardType type)
            {
                RegEx = regEx;
                Length = length;
                Type = type;
            }

            public string RegEx { get; set; }
            public int Length { get; set; }
            public CardType Type { get; set; }
        }
        /****************************************************************************************/
        // Array of CardTypeInfo objects.
        // Used by GetCardType() to identify credit card types.
        private static CardTypeInfo[] _cardTypeInfo =
        {
            new CardTypeInfo("^(51|52|53|54|55)", 16, CardType.MasterCard),
            new CardTypeInfo("^(4)", 16, CardType.VISA),
            new CardTypeInfo("^(4)", 13, CardType.VISA),
            new CardTypeInfo("^(34|37)", 15, CardType.Amex),
            new CardTypeInfo("^(6011)", 16, CardType.Discover),
            new CardTypeInfo("^(300|301|302|303|304|305|36|38)",14, CardType.DinersClub),
            new CardTypeInfo("^(3)", 16, CardType.JCB),
            new CardTypeInfo("^(2131|1800)", 15, CardType.JCB),
            new CardTypeInfo("^(2014|2149)", 15, CardType.enRoute),
        };
        /****************************************************************************************/
        public static CardType GetCardType(string cardNumber)
        {
            foreach (CardTypeInfo info in _cardTypeInfo)
            {
                if (cardNumber.Length == info.Length &&
                    Regex.IsMatch(cardNumber, info.RegEx))
                    return info.Type;
            }
            return CardType.Unknown;
        }
        /****************************************************************************************/
        private void btnValidateCard_Click(object sender, EventArgs e)
        {
            //string creditCard = txtCC.Text.Trim();
            //if (String.IsNullOrWhiteSpace(creditCard))
            //{
            //    MessageBox.Show("***ERROR*** Credit Card Field is Empty!");
            //    return;
            //}
            //bool valid = Mod10Check(creditCard);
            //if (valid)
            //{
            //    CardType cardType = GetCardType(creditCard);
            //    txtCCType.Text = cardType.ObjToString();
            //}
            //else
            //    txtCCType.Text = "INVALID CARD";
        }
        /****************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        { // Remove Existing Check
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int row = gridMain.FocusedRowHandle;
            row = gridMain.GetDataSourceRowIndex(row);
            string contract = dr["contractNumber"].ObjToString();
            string type = dt.Rows[row]["type"].ObjToString();
            string status = dt.Rows[row]["status"].ObjToString();
            if (G1.isAdminOrSuper() || G1.isHomeOffice())
            {
                if (status.ToUpper() == "DEBIT")
                {
                    dr["mod"] = "D";
                    dt.Rows[row]["mod"] = "D";
                }
                else
                {
                    dr["status"] = "Cancelled";
                    dt.Rows[row]["status"] = "Cancelled";
                }
            }
            else
            {
                dr["status"] = "Cancelled";
                dt.Rows[row]["status"] = "Cancelled";
            }
            funModified = true;
            dgv.RefreshDataSource();
            dgv.Refresh();
            this.Refresh();
            CalculateAmountDue();
        }
        /***********************************************************************************************/
        private void AddMod(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");
        }
        /****************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        { // Add New Payment
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            DateTime now = DateTime.Now;
            //dRow["dateFiled"] = G1.DTtoMySQLDT(now);
            //dRow["dateReceived"] = G1.DTtoMySQLDT(now);

            dRow["mod"] = "Y";
            dRow["discresionaryACH"] = "0";
            dRow["pmtInTransition"] = "0";
            dRow["names"] = workNames;
            dt.Rows.Add(dRow);
            dgv.DataSource = dt;
            dgv.Refresh();
            gridMain_CellValueChanged(null, null);
            if (workType.ToUpper() == "TRUST")
            {
                //if ( dt.Rows.Count == 1)
                //    gridMain.Columns["trustAmtFiled"].OptionsColumn.AllowEdit = false; // Fixed as per Cliff 05/31/2022
                //else
                //    gridMain.Columns["trustAmtFiled"].OptionsColumn.AllowEdit = true;
                dRow["type"] = "Trust";
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName != "paid")
                return;
            int dx = e.Bounds.Height;
            Brush brush = e.Cache.GetGradientBrush(e.Bounds, Color.Wheat, Color.FloralWhite, LinearGradientMode.Vertical);
            Rectangle r = e.Bounds;
            //Draw a 3D border 
            BorderPainter painter = BorderHelper.GetPainter(DevExpress.XtraEditors.Controls.BorderStyles.Style3D);
            AppearanceObject borderAppearance = new AppearanceObject(e.Appearance);
            borderAppearance.BorderColor = Color.DarkGray;
            painter.DrawObject(new BorderObjectInfoArgs(e.Cache, borderAppearance, r));
            //Fill the inner region of the cell 
            r.Inflate(-1, -1);
            e.Cache.FillRectangle(brush, r);
            //Draw a summary value 
            r.Inflate(-2, 0);
            double total = calculateTotalPayments();
            string text = G1.ReformatMoney(total);
            e.Appearance.DrawString(e.Cache, text, r);
            //Prevent default drawing of the cell 
            e.Handled = true;
        }
        /****************************************************************************************/
        private double calculateTotalPayments()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            double price = 0D;
            double total = 0D;
            string status = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString().Trim().ToUpper();
                if (status.ToUpper() == "DEPOSITED")
                {
                    price = dt.Rows[i]["paid"].ObjToDouble();
                    total += price;
                }
                else if ( status.ToUpper() == "DISCOUNT")
                {
                    price = dt.Rows[i]["paid"].ObjToDouble();
                    total -= price;
                }
            }
            status = G1.ReformatMoney(total);
            return total;
        }
        /****************************************************************************************/
        private void repositoryItemComboBox2_EditValueChanged(object sender, EventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                ComboBoxEdit combo = (ComboBoxEdit)sender;
                string what = combo.Text;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                string oldStatus = dr["status"].ObjToString();
                string depositNumber = dr["depositNumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( depositNumber))
                {
                    if ( what.ToUpper() != "DEPOSITED")
                    {
                        if (!G1.isAdminOrSuper() && !G1.isHomeOffice())
                        {
                            MessageBox.Show("Payment has Deposit Number!\nYou cannot change the Status once deposited!", "Status Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            dr["status"] = oldStatus;
                            dt.Rows[row]["status"] = oldStatus;
                            dgv.RefreshDataSource();
                            gridMain.RefreshEditor(true);
                            return;
                        }
                    }
                }
                dr["status"] = what;
                dr["mod"] = "Y";
                string set = dr["discresionaryACH"].ObjToString();

                string type = dr["type"].ObjToString();

                dt.Rows[row]["status"] = what;
                if (what.ToUpper() == "FILED" )
                {
                    dt.Rows[row]["dateFiled"] = G1.DTtoMySQLDT(DateTime.Now);
                    dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT("0001-01-01");
                    dr["bankAccount"] = "";
                    dr["localDescription"] = "";
                    dt.Rows[row]["bankAccount"] = "";
                    dt.Rows[row]["localDescription"] = "";
                    gridMain.RefreshEditor(true);
                    gridMain.Columns["localDescription"].ColumnEdit = null;
                }
                else if (what.ToUpper() == "DEPOSITED" || what.ToUpper() == "DEBIT" )
                {

                    if (set == "1")
                    {
                        string bankAccount = GetDepositBankAccount("discresionaryACH");
                        if (!String.IsNullOrWhiteSpace(bankAccount))
                        {
                            dr["bankAccount"] = bankAccount;
                            dt.Rows[row]["bankAccount"] = bankAccount;
                        }
                    }
                    else
                    {
                        //dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT(DateTime.Now); // ramma zamma changed 6/13/2023 b/c filed date was 0001/01/01 I think
                        if (type.ToUpper() != "INSURANCE DIRECT")
                        {
                            string bankAccount = GetDepositBankAccount("trustDeathClaims");
                            if ( !String.IsNullOrWhiteSpace ( type ))
                                bankAccount = GetDepositBankAccount(type);
                            if (!String.IsNullOrWhiteSpace(bankAccount))
                            {
                                dr["bankAccount"] = bankAccount;
                                dt.Rows[row]["bankAccount"] = bankAccount;
                            }
                        }
                    }
                    CheckForDiscountOrGrowth();
                    dr["mod"] = "Y";
                    gridMain.RefreshEditor(true);
                }
                else if (what.ToUpper() == "DEBIT")
                {
                    string bankAccount = GetDepositBankAccount(type);
                    if (!String.IsNullOrWhiteSpace(bankAccount))
                    {
                        dr["bankAccount"] = bankAccount;
                        dt.Rows[row]["bankAccount"] = bankAccount;
                    }

                    //if (set == "1")
                    //{
                    //    string bankAccount = GetDepositBankAccount("discresionaryACH");
                    //    if (!String.IsNullOrWhiteSpace(bankAccount))
                    //    {
                    //        dr["bankAccount"] = bankAccount;
                    //        dt.Rows[row]["bankAccount"] = bankAccount;
                    //    }
                    //}
                    //else
                    //{
                    //    dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT(DateTime.Now);
                    //    if (type.ToUpper() != "INSURANCE DIRECT")
                    //    {
                    //        string bankAccount = GetDepositBankAccount("trustDeathClaims");
                    //        if (!String.IsNullOrWhiteSpace(bankAccount))
                    //        {
                    //            dr["bankAccount"] = bankAccount;
                    //            dt.Rows[row]["bankAccount"] = bankAccount;
                    //        }
                    //    }
                    //}
                    //CheckForDiscountOrGrowth();
                    dr["mod"] = "Y";
                    gridMain.RefreshEditor(true);
                }
                else
                {
                    dt.Rows[row]["dateFiled"] = G1.DTtoMySQLDT("0001-01-01");
                    dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT("0001-01-01");
                    dr["bankAccount"] = "";
                    dr["localDescription"] = "";
                    dt.Rows[row]["bankAccount"] = "";
                    dt.Rows[row]["localDescription"] = "";
                    gridMain.Columns["localDescription"].ColumnEdit = null;
                    gridMain.RefreshEditor(true);
                    dr["mod"] = "Y";
                }
                dgv.RefreshDataSource();
            }
            catch (Exception ex)
            {

            }
            CalculateAmountDue();
            SetupSave();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox1_EditValueChanged(object sender, EventArgs e)
        {
            try
            {

                ComboBoxEdit combo = (ComboBoxEdit)sender;
                string what = combo.Text;
                DataRow dr = gridMain.GetFocusedDataRow();
                int row = gridMain.FocusedRowHandle;
                row = gridMain.GetDataSourceRowIndex(row);
                dr["type"] = what;
                DataTable dt = (DataTable)dgv.DataSource;
                dt.Rows[row]["type"] = what;
                dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
                dt.Rows[row]["lastUser"] = LoginForm.username;
                dt.Rows[row]["mod"] = "Y";
                dr["mod"] = "Y";

                string bankAccount = GetDepositBankAccount(what);
                if (!String.IsNullOrWhiteSpace(bankAccount))
                {
                    dr["bankAccount"] = bankAccount;
                    dt.Rows[row]["bankAccount"] = bankAccount;
                }
            }
            catch (Exception ex)
            {

            }
            CalculateAmountDue();
            SetupSave();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void GetProperBank ( string paymentType )
        {
            try
            {
                if (String.IsNullOrWhiteSpace(paymentType))
                    return;

                //ComboBoxEdit combo = (ComboBoxEdit)sender;
                string what = paymentType;
                DataRow dr = gridMain.GetFocusedDataRow();
                int row = gridMain.FocusedRowHandle;
                row = gridMain.GetDataSourceRowIndex(row);
                dr["type"] = what;
                DataTable dt = (DataTable)dgv.DataSource;
                dt.Rows[row]["type"] = what;
                dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
                dt.Rows[row]["lastUser"] = LoginForm.username;
                dt.Rows[row]["mod"] = "Y";
                dr["mod"] = "Y";

                string bankAccount = GetDepositBankAccount(what);
                if (!String.IsNullOrWhiteSpace(bankAccount))
                {
                    dr["bankAccount"] = bankAccount;
                    dt.Rows[row]["bankAccount"] = bankAccount;
                }
            }
            catch (Exception ex)
            {

            }
            CalculateAmountDue();
            SetupSave();
            gridMain.RefreshData();
        }
        /***********************************************************************************************/
        private void InitialBank ( DataTable dt, string type, int row )
        {
            if (dt == null)
                return;
            dt.Rows[row]["type"] = type;
            dt.Rows[row]["dateModified"] = G1.DTtoMySQLDT(DateTime.Now);
            dt.Rows[row]["lastUser"] = LoginForm.username;

            string bankAccount = GetDepositBankAccount(type);
            if (!String.IsNullOrWhiteSpace(bankAccount))
            {
                dt.Rows[row]["bankAccount"] = bankAccount;
                dt.AcceptChanges();
            }
        }
        /***********************************************************************************************/
        private void SetupTrustPaidCombo()
        {
            repositoryItemComboBox3.Items.Clear();
            string cmd = "Select * from `ref_trust_assignments`;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string trust = "";
                repositoryItemComboBox3.Items.Add("Clear");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    trust = dt.Rows[i]["trust_assignments"].ObjToString();
                    repositoryItemComboBox3.Items.Add(trust);
                }
            }
        }
        /****************************************************************************************/
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);

            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /****************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            AddMod(dt, gridMain);

            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string type = dr["type"].ObjToString().ToUpper();
            string column = gridMain.FocusedColumn.FieldName.ToUpper();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (column == "DATEDCFILED")
            {
                DateTime date = dr["dateDCFiled"].ObjToDateTime();
                using (GetDate dateForm = new GetDate(date, "Date DC Filed"))
                {
                    dateForm.TopMost = true;
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        date = dateForm.myDateAnswer;
                        dr["dateDCFiled"] = G1.DTtoMySQLDT(date);
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string type = dr["type"].ObjToString().ToUpper();
            string column = gridMain.FocusedColumn.FieldName.ToUpper();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if ( column == "DATEDCFILED" )
            {
                DateTime date = dr["dateDCFiled"].ObjToDateTime();
                dr["dateDCFiled"] = G1.DTtoMySQLDT(date);
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
        }
        /****************************************************************************************/
        private void GetInitialMoney ( DataTable dt )
        {
            string price = "";
            double paid = 0D;
            string status = "";
            amountFiled = 0D;
            amountReceived = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                price = dt.Rows[i]["paid"].ObjToString();
                status = dt.Rows[i]["status"].ObjToString().ToUpper();
                paid = price.ObjToDouble();
                if (status == "FILED" )
                    amountFiled += paid;
                else if (status == "DEPOSITED")
                    amountReceived += paid;
            }
        }
        /****************************************************************************************/
        private bool validateClosing()
        {
            bool okay = true;
            string type = "";
            string depositNumber = "";
            double amtActuallyReceived = 0D;
            string status = "";
            DateTime date = DateTime.Now;
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                amtActuallyReceived = dt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                date = dt.Rows[i]["dateReceived"].ObjToDateTime();
                type = dt.Rows[i]["type"].ObjToString().ToUpper();
                if ( status.ToUpper() == "DEPOSITED" && String.IsNullOrWhiteSpace ( depositNumber))
                {
                        MessageBox.Show("***ERROR*** You must provide a Deposit Number for all payments deposited!", "Deposit Verification Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        okay = false;
                        break;
                }
                else if (status.ToUpper() == "DEPOSITED" && date.Year < 100 )
                {
                    MessageBox.Show("***ERROR*** You must provide a Date Received for all payments deposited!", "Deposit Verification Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    okay = false;
                    break;
                }
                else if (status.ToUpper() == "DEPOSITED" && type.ToUpper() == "INSURANCE DIRECT")
                {
                    MessageBox.Show("***ERROR*** You must change type ( Insurance Direct ) to a different payment type if being deposited!", "Deposit Verification Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    okay = false;
                    break;
                }
                else if (status.ToUpper() == "DEPOSITED" && amtActuallyReceived == 0D )
                {
                    if (type != "CASH" && type != "CHECK-LOCAL" && type != "CHECK-REMOTE" && type != "CREDIT CARD" && type != "DISCOUNT")
                    {
                        MessageBox.Show("***ERROR*** You CANNOT leave a deposited payment of type (" + type + ") without providing an Amount Actually Received!", "Deposit Verification Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        okay = false;
                        break;
                    }
                }
            }
            return okay;
        }
        /****************************************************************************************/
        private void FunPaymentDetails_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.No;
            if (dataSaved)
                this.DialogResult = DialogResult.Yes;
            if (String.IsNullOrWhiteSpace(workContract))
                return;
            if (!funModified)
                return;
            this.TopMost = false;
            DialogResult result = MessageBox.Show("***Question***\nPayment Details have been modified!\nWould you like to SAVE these Details?", "Details Modified Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
                return;
            else if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (!validateClosing())
            {
                e.Cancel = true;
                return;
            }
            btnSavePayments_Click(null, null);
            this.DialogResult = DialogResult.Yes;
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            try
            {

                DataTable dt = (DataTable)dgv.DataSource;
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                DataRow dr = gridMain.GetFocusedDataRow();
                string record = dr["record"].ObjToString();

                string set = dr["discresionaryACH"].ObjToString();
                if (set != "1")
                    set = "1";
                else
                    set = "0";
                dr["mod"] = "Y";
                dt.Rows[row]["mod"] = "Y";

                dr["discresionaryACH"] = set;
                dt.Rows[row]["discresionaryACH"] = set;
                dr["bankAccount"] = "";
                dr["localDescription"] = "";
                dt.Rows[row]["bankAccount"] = "";
                dt.Rows[row]["localDescription"] = "";
                gridMain.Columns["localDescription"].ColumnEdit = null;
                gridMain_FocusedRowChanged(null, null);
                //if (set == "1")
                //{
                //    string bankAccount = GetDepositBankAccount("discresionaryACH");
                //    if (!String.IsNullOrWhiteSpace(bankAccount))
                //    {
                //        dr["bankAccount"] = bankAccount;
                //        dt.Rows[row]["bankAccount"] = bankAccount;
                //    }
                //}
                //else
                //{
                //    dr["bankAccount"] = "";
                //    dr["localDescription"] = "";
                //    dt.Rows[row]["bankAccount"] = "";
                //    dt.Rows[row]["localDescription"] = "";
                //    gridMain.Columns["localDescription"].ColumnEdit = null;
                //}
                dgv.RefreshDataSource();
            }
            catch (Exception ex)
            {

            }
            SetupSave();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void repositoryItemComboBox4_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);
            string initialized = dt.Rows[row]["initialized"].ObjToString();
            string type = dt.Rows[row]["type"].ObjToString();
            if (type.ToUpper() != "CREDIT CARD")
                return;
            if (initialized != "1")
            {
                try
                {
                    dt.Rows[row]["initialized"] = "1";
                    DataRow dr = gridMain.GetFocusedDataRow();
                    string what = dr["status"].ObjToString();
                    string set = dr["discresionaryACH"].ObjToString();
                    if (what.ToUpper() == "FILED")
                    {
                        dt.Rows[row]["dateFiled"] = G1.DTtoMySQLDT(DateTime.Now);
                        dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT("0001-01-01");
                        dr["bankAccount"] = "";
                        dr["localDescription"] = "";
                        dt.Rows[row]["bankAccount"] = "";
                        dt.Rows[row]["localDescription"] = "";
                        gridMain.Columns["localDescription"].ColumnEdit = null;
                        gridMain.RefreshEditor(true);
                    }
                    else if (what.ToUpper() == "RECEIVED" || what.ToUpper() == "DEPOSITED" )
                    {
                        if (set == "1")
                        {
                            string bankAccount = GetDepositBankAccount("Credit Card");
                            if (!String.IsNullOrWhiteSpace(bankAccount))
                            {
                                dr["bankAccount"] = bankAccount;
                                dt.Rows[row]["bankAccount"] = bankAccount;
                            }
                        }
                        else
                        {
                            //dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT(DateTime.Now); ramma zamma changed 6/13/2023 b/c date filed was 0001-01-01
                            string bankAccount = GetDepositBankAccount("Credit Card");
                            if (!String.IsNullOrWhiteSpace(bankAccount))
                            {
                                dr["bankAccount"] = bankAccount;
                                dt.Rows[row]["bankAccount"] = bankAccount;
                            }
                        }
                    }
                    //else
                    //{
                    //    dt.Rows[row]["dateFiled"] = G1.DTtoMySQLDT("0001-01-01");
                    //    dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT("0001-01-01");
                    //    dr["bankAccount"] = "";
                    //    dr["localDescription"] = "";
                    //    dt.Rows[row]["bankAccount"] = "";
                    //    dt.Rows[row]["localDescription"] = "";
                    //    gridMain.Columns["localDescription"].ColumnEdit = null;
                    //    gridMain.RefreshEditor(true);
                    //}
                    dgv.RefreshDataSource();
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
        private void repositoryItemComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataTable dx = (DataTable)dgv.DataSource;
            if (dx == null)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            dx.Rows[row]["mod"] = "Y";
            dr["mod"] = "Y";
            SetupSave();

            ComboBoxEdit edit = (ComboBoxEdit)sender;
            string str = edit.Text;
            string cmd = "Select * from `bank_accounts` where `localDescription` = '" + str + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
            {
                cmd = "Select * from `bank_accounts` where `account_title` = '" + str + "';";
                dt = G1.get_db_data(cmd);
            }
            if (dt.Rows.Count > 0)
            {
                string bankAccount = dt.Rows[0]["account_no"].ObjToString();
                dr["bankAccount"] = bankAccount;
                dx.Rows[row]["bankAccount"] = bankAccount;
                dr["localDescription"] = str;
                dx.Rows[row]["localDescription"] = str;

                gridMain.RefreshRowCell(rowHandle, gridMain.Columns["bankAccount"]);
                gridMain.RefreshEditor(true);
                gridMain.RefreshRow(rowHandle);
                dgv.RefreshDataSource();
                dgv.Refresh();
                gridMain.SelectRow(rowHandle);
                gridMain.SelectCell(rowHandle, gridMain.Columns["bankAccount"]);
                gridMain.RefreshEditor(true);

                gridMain.RefreshData();
            }
        }
        /****************************************************************************************/
        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int focusedRow = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(focusedRow);
            try
            {
                dt.Rows[row]["initialized"] = "1";
                DataRow dr = gridMain.GetFocusedDataRow();
                string what = dr["status"].ObjToString();
                string set = dr["discresionaryACH"].ObjToString();
                if (what.ToUpper() == "FILED" )
                {
                    dt.Rows[row]["dateFiled"] = G1.DTtoMySQLDT(DateTime.Now);
                    dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT("0001-01-01");
                    dr["bankAccount"] = "";
                    dr["localDescription"] = "";
                    dt.Rows[row]["bankAccount"] = "";
                    dt.Rows[row]["localDescription"] = "";
                    gridMain.Columns["localDescription"].ColumnEdit = null;
                    gridMain.RefreshEditor(true);
                }
                else if (what.ToUpper() == "DEPOSITED")
                {
                    string type = dr["type"].ObjToString();
                    if (type.ToUpper() == "TRUST")
                        type = "TrustDeathClaims";
                    string saveDescription = dr["localDescription"].ObjToString();
                    string saveBank = dr["bankAccount"].ObjToString();
                    if (set == "1")
                    {
                        string bankAccount = GetDepositBankAccount("discresionaryACH");
                        if (!String.IsNullOrWhiteSpace(bankAccount))
                        {
                            dr["bankAccount"] = bankAccount;
                            dt.Rows[row]["bankAccount"] = bankAccount;
                        }
                    }
                    else
                    {
                        //dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT(DateTime.Now); // ramma zamma changed 6/13/2023 b/c Filed was 0001-01-01
                        string bankAccount = GetDepositBankAccount(type);
                        if (!String.IsNullOrWhiteSpace(bankAccount))
                        {
                            dr["bankAccount"] = bankAccount;
                            dt.Rows[row]["bankAccount"] = bankAccount;
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(saveDescription))
                    {
                        dr["bankAccount"] = saveBank;
                        dr["localDescription"] = saveDescription;
                        dt.Rows[row]["bankAccount"] = saveBank;
                        dt.Rows[row]["localDescription"] = saveDescription;
                    }
                }
                else
                {
                    string type = dr["type"].ObjToString();
                    if (type.ToUpper() == "TRUST")
                        type = "TrustDeathClaims";
                    string saveDescription = dr["localDescription"].ObjToString();
                    string saveBank = dr["bankAccount"].ObjToString();
                    dt.Rows[row]["dateFiled"] = G1.DTtoMySQLDT("0001-01-01");
                    dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT("0001-01-01");
                    if (!String.IsNullOrWhiteSpace(saveDescription))
                    {
                        string bankAccount = GetDepositBankAccount(type);
                        if (!String.IsNullOrWhiteSpace(bankAccount))
                        {
                            dr["bankAccount"] = bankAccount;
                            dt.Rows[row]["bankAccount"] = bankAccount;
                        }
                    }
                    else
                    {
                        dr["bankAccount"] = "";
                        dr["localDescription"] = "";
                        dt.Rows[row]["bankAccount"] = "";
                        dt.Rows[row]["localDescription"] = "";
                        gridMain.Columns["localDescription"].ColumnEdit = null;
                    }
                    gridMain.RefreshEditor(true);
                }
                dgv.RefreshDataSource();
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string status = "";
            double totalPaid = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "DEPOSITED")
                    totalPaid += dt.Rows[i]["paid"].ObjToDouble();
            }
            e.TotalValueReady = true;
            if (workType.ToUpper() == "TRUST" )
                e.TotalValue = trustTotal;
            else
                e.TotalValue = totalPaid;
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
        }
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
//            Printer.DrawQuad(6, 8, 4, 4, "Funeral Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(3, 8, 8, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        public static void CalcTrust2013(string contractNumber, ref double endingBalance, ref double trust85Pending, ref double beginningBalance, ref string locind )
        {
            string cmd = "";
            DataTable dx = null;
            trust85Pending = 0D;
            endingBalance = 0D;
            beginningBalance = 0D;
            double payment = 0D;
            double removals = 0D;
            double value = 0D;
            string fill = "";
            DateTime charlotteDate = DateTime.Now;
            double originalDownPayment = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;

            cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' AND `endingBalance` > '0' ORDER BY `payDate8` DESC limit 1;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                removals = dx.Rows[0]["currentRemovals"].ObjToDouble();
                endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();
                beginningBalance = dx.Rows[0]["beginningBalance"].ObjToDouble();
                locind = dx.Rows[0]["locind"].ObjToString();

                charlotteDate = dx.Rows[0]["payDate8"].ObjToDateTime();

                dx = DailyHistory.GetPaymentData(contractNumber, charlotteDate, originalDownPayment, true);
                trust85Pending = 0D;
                if (dx != null)
                {
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        fill = dx.Rows[j]["fill"].ObjToString();
                        if (fill.ToUpper() != "D")
                        {
                            value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                            trust85Pending += value;
                        }
                    }
                }
            }
            else
            {
                charlotteDate = new DateTime(2000, 1, 1);
                dx = DailyHistory.GetPaymentData(contractNumber, charlotteDate, originalDownPayment, true);
                trust85Pending = 0D;
                if (dx == null)
                    return;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    fill = dx.Rows[j]["fill"].ObjToString();
                    if (fill.ToUpper() != "D")
                    {
                        value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                        trust85Pending += value;
                    }
                }
            }
        }
        /***********************************************************************************************/
        public static void CalcTrust2013Actual(string contractNumber, ref double endingBalance, ref double trust85Pending, ref double beginningBalance, ref string locind)
        {
            string cmd = "";
            DataTable dx = null;
            trust85Pending = 0D;
            endingBalance = 0D;
            beginningBalance = 0D;
            double payment = 0D;
            double removals = 0D;
            double value = 0D;
            string fill = "";
            DateTime charlotteDate = DateTime.Now;
            double originalDownPayment = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;

            cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' AND `endingBalance` > '0' ORDER BY `payDate8` DESC limit 1;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                removals = dx.Rows[0]["currentRemovals"].ObjToDouble();
                endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();
                beginningBalance = dx.Rows[0]["beginningBalance"].ObjToDouble();
                locind = dx.Rows[0]["locind"].ObjToString();

                charlotteDate = dx.Rows[0]["payDate8"].ObjToDateTime();
                cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' AND `endingBalance` = '0' AND `beginningBalance` > '0' AND `payDate8` > '" + charlotteDate.ToString("yyyy-MM-dd") + "' ORDER BY `payDate8` DESC limit 1;";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    beginningBalance = dx.Rows[0]["beginningBalance"].ObjToDouble();
                    removals = dx.Rows[0]["currentRemovals"].ObjToDouble();
                    payment = dx.Rows[0]["currentPayments"].ObjToDouble();
                    if ((beginningBalance + payment) == removals)
                        endingBalance = removals;
                }

                //dx = DailyHistory.GetPaymentData(contractNumber, charlotteDate, originalDownPayment, true);
                //trust85Pending = 0D;
                //if (dx != null)
                //{
                //    for (int j = 0; j < dx.Rows.Count; j++)
                //    {
                //        fill = dx.Rows[j]["fill"].ObjToString();
                //        if (fill.ToUpper() != "D")
                //        {
                //            value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                //            trust85Pending += value;
                //        }
                //    }
                //}
            }
            else
            {
                charlotteDate = new DateTime(2000, 1, 1);
                dx = DailyHistory.GetPaymentData(contractNumber, charlotteDate, originalDownPayment, true);
                trust85Pending = 0D;
                if (dx == null)
                    return;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    fill = dx.Rows[j]["fill"].ObjToString();
                    if (fill.ToUpper() != "D")
                    {
                        value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                        trust85Pending += value;
                    }
                }
            }
        }
        /***********************************************************************************************/
        public static void CalcTrust2013(DataTable trustDt, string contractNumber, ref double endingBalance, ref double trust85Pending, ref double beginningBalance, ref string locind)
        {
            string cmd = "";
            DataTable dx = null;
            trust85Pending = 0D;
            endingBalance = 0D;
            beginningBalance = 0D;
            double removals = 0D;
            double value = 0D;
            string fill = "";
            DateTime charlotteDate = DateTime.Now;
            double originalDownPayment = 0D;
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;

            if (contractNumber.ToUpper().IndexOf("SX") == 0)
                return;

            DataRow [] dRows = trustDt.Select("contractNumber='" + contractNumber + "'");
            if (dRows.Length > 0)
                dx = dRows.CopyToDataTable();
            else
            {
                cmd = "Select * from `trust2013r` where `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC limit 1;";
                dx = G1.get_db_data(cmd);
            }
            if (dx.Rows.Count > 0)
            {
                removals = dx.Rows[0]["currentRemovals"].ObjToDouble();
                endingBalance = dx.Rows[0]["endingBalance"].ObjToDouble();
                beginningBalance = dx.Rows[0]["beginningBalance"].ObjToDouble();
                locind = dx.Rows[0]["locind"].ObjToString();

                charlotteDate = dx.Rows[0]["payDate8"].ObjToDateTime();
                dx = DailyHistory.GetPaymentData(contractNumber, charlotteDate, originalDownPayment, true);
                trust85Pending = 0D;
                if (dx != null)
                {
                    for (int j = 0; j < dx.Rows.Count; j++)
                    {
                        fill = dx.Rows[j]["fill"].ObjToString();
                        if (fill.ToUpper() != "D")
                        {
                            value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                            trust85Pending += value;
                        }
                    }
                }
            }
            else
            {
                charlotteDate = new DateTime(2000, 1, 1);
                dx = DailyHistory.GetPaymentData(contractNumber, charlotteDate, originalDownPayment, true);
                trust85Pending = 0D;
                if (dx == null)
                    return;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    fill = dx.Rows[j]["fill"].ObjToString();
                    if (fill.ToUpper() != "D")
                    {
                        value = Math.Round(dx.Rows[j]["trust85P"].ObjToDouble(), 2);
                        trust85Pending += value;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            CreateReport();
        }
        /***********************************************************************************************/
        public XtraReport report = null;
        public XRSubreport subReport = null;
        public DevExpress.XtraReports.UI.XRPageBreak xrPageBreak2 = new XRPageBreak();
        public DevExpress.XtraReports.UI.GroupFooterBand xrGroupFooter = new GroupFooterBand();
        /***********************************************************************************************/
        public void CreateReport()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            try
            {
                xrPageBreak2 = new XRPageBreak();
                //xrPageBreak2.BeforePrint += XrPageBreak2_BeforePrint;

                //xrGroupFooter.AfterPrint += XrGroupFooter_AfterPrint;
                xrGroupFooter.PageBreak = PageBreak.AfterBand;


                XtraReport reportMerge = new XtraReport();
                reportMerge.Margins.Top = 50;
                reportMerge.Margins.Bottom = 50;
                reportMerge.Margins.Left = 30;
                reportMerge.Margins.Right = 75;

                DetailBand detailBand = new DetailBand();
                //detailBand.Height = labelDetail.Height;
                detailBand.Height = 0;
                detailBand.Name = "DetailBand";
                detailBand.KeepTogetherWithDetailReports = true;
                reportMerge.Bands.Add(detailBand);

                IList<XtraReport> reportList = new List<XtraReport>();

                //xrPageBreak1.Location = new System.Drawing.Point(0, 58);
                //xrPageBreak1.BeforePrint += XrPageBreak1_BeforePrint;

                Point point = new Point(0, 0);
                xrPageBreak2.Location = point;


                //DevExpress.XtraReports.UI.XRPageBreak xrPageBreak1 = new XRPageBreak();
                //xrPageBreak1.Name = "xrPageBreak1";
                //xrPageBreak1.BeforePrint += XrPageBreak1_BeforePrint;

                int pageCount = 0;

                int startRow = 0;
                int myLastRow = 0;
                bool includeBoth = false;
                string lastBreak = "{HEADER}";
                report = new XtraReport();
                report.Margins.Top = 50;
                report.Margins.Bottom = 40;
                report.Margins.Left = 30;
                report.Margins.Right = 75;

                float mainWidth = 500F;

                detailBand = new DetailBand();
                detailBand.Height = 0;
                detailBand.KeepTogetherWithDetailReports = true;
                report.Bands.Add(detailBand);

                XRTable tableDetail = new XRTable();
                tableDetail.BeginInit();
                tableDetail.WidthF = GetTotalPageWidth();
                tableDetail.KeepTogether = true;
                tableDetail.Rows.Clear();

                XRTableRow xrRow = new XRTableRow();
                XRTableCell cell = new XRTableCell();

                AddBlankReportRows(tableDetail, xrRow, cell, 20);

                xrRow = new XRTableRow();
                cell = new XRTableCell();
                cell.Text = "Bank Deposit Report";
                cell.WidthF = GetTotalPageWidth();
                cell.TextAlignment = TextAlignment.MiddleCenter;
                cell.Font = new Font("Times New Roman", 20F, System.Drawing.FontStyle.Bold | FontStyle.Underline);
                xrRow.Cells.Add(cell);
                tableDetail.Rows.Add(xrRow);

                AddBlankReportRows(tableDetail, xrRow, cell, 12);

                xrRow = new XRTableRow();
                cell = new XRTableCell();
                cell.Text = GetDetailFuneralHomeName(EditCust.activeFuneralHomeName);
                cell.WidthF = GetTotalPageWidth();
                cell.TextAlignment = TextAlignment.MiddleCenter;
                cell.Font = new Font("Times New Roman", 15F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);
                tableDetail.Rows.Add(xrRow);

                AddBlankReportRows(tableDetail, xrRow, cell, 12);

                string dateReceived = dr["dateReceived"].ObjToDateTime().ToString("MM/dd/yyyy");
                xrRow = new XRTableRow();
                cell = new XRTableCell();
                cell.Text = "Date : " + dateReceived;
                cell.WidthF = GetTotalPageWidth();
                cell.TextAlignment = TextAlignment.MiddleCenter;
                cell.Font = new Font("Times New Roman", 15F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);
                tableDetail.Rows.Add(xrRow);

                AddBlankReportRows(tableDetail, xrRow, cell, 20);

                xrRow = new XRTableRow();

                cell = new XRTableCell();
                cell.Text = "";
                cell.WidthF = 70;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 15F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                cell = new XRTableCell();
                cell.Text = "Funeral Number : " + serviceId;
                cell.WidthF = GetTotalPageWidth() / 2;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                cell = new XRTableCell();
                cell.Text = "Deceased Name : " + customerName;
                cell.WidthF = GetTotalPageWidth() / 2;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                tableDetail.Rows.Add(xrRow);

                AddBlankReportRows(tableDetail, xrRow, cell, 10);

                xrRow = new XRTableRow();

                cell = new XRTableCell();
                cell.Text = "";
                cell.WidthF = 70;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 15F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                cell = new XRTableCell();
                cell.Text = "Payer Name : " + dr["names"].ObjToString();
                cell.WidthF = GetTotalPageWidth();
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);
                tableDetail.Rows.Add(xrRow);

                AddBlankReportRows(tableDetail, xrRow, cell, 10);

                xrRow = new XRTableRow();

                cell = new XRTableCell();
                cell.Text = "";
                cell.WidthF = 70;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 15F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                cell = new XRTableCell();
                cell.Text = "Payment Type : " + dr["type"].ObjToString();
                cell.WidthF = GetTotalPageWidth() / 2;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                string paymentType = dr["type"].ObjToString().ToUpper();
                double payment  = 0D;
                double amtReceived = 0D;
                string paidFrom = dr["paidFrom"].ObjToString();
                amtReceived = dr["amtActuallyReceived"].ObjToDouble();

                if (paymentType == "CASH" || paymentType.IndexOf("CHECK") >= 0 || paymentType.IndexOf("CREDIT ") >= 0)
                {
                    payment = dr["paid"].ObjToDouble();
                    if ( !String.IsNullOrWhiteSpace ( paidFrom ))
                        payment = dr["amtActuallyReceived"].ObjToDouble();
                    if (amtReceived > 0D)
                        payment = amtReceived;
                }
                else
                    payment = dr["amtActuallyReceived"].ObjToDouble();
                //if (amtReceived > payment)
                //    payment = amtReceived;
                //payment = amtReceived;
                string str = G1.ReformatMoney(payment);

                cell = new XRTableCell();
                cell.Text = "Payment Amount : " + str;
                cell.WidthF = GetTotalPageWidth() / 2;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                tableDetail.Rows.Add(xrRow);

                AddBlankReportRows(tableDetail, xrRow, cell, 10);

                xrRow = new XRTableRow();

                cell = new XRTableCell();
                cell.Text = "";
                cell.WidthF = 70;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 15F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                cell = new XRTableCell();
                cell.Text = "Deposit Information : " + dr["localDescription"].ObjToString();
                cell.WidthF = GetTotalPageWidth() / 2;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                cell = new XRTableCell();
                cell.Text = "Deposit # : " + dr["depositNumber"].ObjToString();
                cell.WidthF = GetTotalPageWidth() / 2;
                cell.TextAlignment = TextAlignment.MiddleLeft;
                cell.Font = new Font("Times New Roman", 12F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);

                tableDetail.Rows.Add(xrRow);

                tableDetail.Borders = BorderSide.None;
                tableDetail.BorderColor = Color.DarkGray;
                tableDetail.Font = new Font("Tahoma", 10);
                tableDetail.Padding = 0;
                tableDetail.TextAlignment = DevExpress.XtraPrinting.TextAlignment.MiddleLeft;
                tableDetail.WidthF = GetTotalPageWidth();

                detailBand.Height = tableDetail.Height;
                detailBand.WidthF = 500F;
                tableDetail.WidthF = GetTotalPageWidth();
                tableDetail.LocationF = new PointF(0, 0);
                detailBand.Controls.Add(tableDetail);
                //detailBand.PageBreak = PageBreak.AfterBand;

                tableDetail.EndInit();

                //                point = new Point(0, point.Y + report.PageHeight);

                //              startRow = myLastRow + 1;

                bool topMost = this.TopMost;
                this.TopMost = false;
                ReportPrintTool printTool = new ReportPrintTool(report);
                printTool.ShowPreviewDialog();
                if (topMost)
                    this.TopMost = true;
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void AddBlankReportRows (XRTable tableDetail, XRTableRow xrRow, XRTableCell cell, int count )
        {
            for (int i = 0; i < count; i++)
            {
                xrRow = new XRTableRow();
                cell = new XRTableCell();
                cell.Text = "";
                cell.WidthF = GetTotalPageWidth();
                cell.TextAlignment = TextAlignment.MiddleCenter;
                cell.Font = new Font("Times New Roman", 15F, System.Drawing.FontStyle.Regular);
                xrRow.Cells.Add(cell);
                tableDetail.Rows.Add(xrRow);
            }
        }
        /***********************************************************************************************/
        private float GetTotalPageWidth()
        {
            float totalWidth = report.PageWidth - report.Margins.Left - report.Margins.Right;
            return totalWidth;
        }
        /****************************************************************************************/
        private string GetDetailFuneralHomeName ( string shortName)
        {
            string funeralHome = shortName;
            string cmd = "Select * from `funeralhomes` where `LocationCode` = '" + shortName + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                funeralHome = dt.Rows[0]["name"].ObjToString();
            return funeralHome;
        }
        /****************************************************************************************/
        private void gridMain_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;
                int newHeight = 0;
                string name = "";
                foreach (GridColumn column in gridMain.Columns)
                {
                    name = column.FieldName.ToUpper();
                    if ( name != "NOTES" && name != "LOCALDESCRIPTION" && name != "PAIDFROM" )
                        continue;
                    if (column.Visible)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                viewInfo.EditValue = gridMain.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                        maxHeight = newHeight;
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0)
                    e.RowHeight = maxHeight;
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        /****************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "DATEDCFILED")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["dateDCfILED"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
                dt.Rows[row]["mod"] = "Y";
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DATERECEIVED")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["dateReceived"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
                dt.Rows[row]["mod"] = "Y";
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DATEFILED")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["dateFiled"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
                dt.Rows[row]["mod"] = "Y";
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DCREQUIRED")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString().ToUpper();
                if ( !String.IsNullOrWhiteSpace ( oldWhat ))
                {
                    string str = oldWhat.Substring(0, 1);
                    if (str == "N")
                        oldWhat = "NO";
                    else if (str == "Y")
                        oldWhat = "YES";
                    dt.Rows[row]["DCRequired"] = oldWhat;
                    e.Value = oldWhat;
                    dt.Rows[row]["mod"] = "Y";
                }
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DATERECEIVEDFROMFH")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["dateReceivedFromFH"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
                dt.Rows[row]["mod"] = "Y";
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "DEPOSITNUMBER")
            {
                if (G1.isAdminOrSuper() || G1.isHomeOffice())
                    return;
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string depositNumber = dt.Rows[row]["depositNumber"].ObjToString();
                string status = dt.Rows[row]["status"].ObjToString();
                string record = dt.Rows[row]["record"].ObjToString();
                string type = dt.Rows[row]["type"].ObjToString();
                if( type.ToUpper() == "CREDIT CARD" && G1.isField() )
                {
                    MessageBox.Show("You cannot edit a Credit Card deposit number!", "Edit Deposit Number Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    e.Value = depositNumber;
                }
                else if ((status.ToUpper() == "DEPOSITED" || status.ToUpper() == "ACCEPT") && !String.IsNullOrWhiteSpace ( record ))
                {
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        MessageBox.Show("Payment has Deposit Number!\nYou cannot change this deposit number!", "Edit Deposit Number Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        e.Value = depositNumber;
                    }
                }
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "STATUS")
            {
                //if (G1.isAdminOrSuper() || G1.isHomeOffice())
                //    return;
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string depositNumber = dt.Rows[row]["depositNumber"].ObjToString();
                string status = dt.Rows[row]["status"].ObjToString();
                if (status.ToUpper() == "DEPOSITED" || status.ToUpper() == "ACCEPT")
                {
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        //MessageBox.Show("Payment has Deposit Number!\nYou cannot change this deposit number!", "Edit Deposit Number Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        e.Value = status;
                    }
                }
            }
            else if (view.FocusedColumn.FieldName.ToUpper() == "PAID")
            {
                if (G1.isAdminOrSuper() || G1.isHomeOffice())
                    return;
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string depositNumber = dt.Rows[row]["depositNumber"].ObjToString();
                string status = dt.Rows[row]["status"].ObjToString();
                string paid = dt.Rows[row]["paid"].ObjToString();
                if (status.ToUpper() == "DEPOSITED" || status.ToUpper() == "ACCEPT")
                {
                    if (!String.IsNullOrWhiteSpace(depositNumber))
                    {
                        MessageBox.Show("Payment has Deposit Number!\nYou cannot change this payment!", "Edit Payment Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        e.Value = paid;
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example
            AddMod(dt, gridMain);

            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string type = dr["type"].ObjToString().ToUpper();
            string column = gridMain.FocusedColumn.FieldName.ToUpper();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (column == "DATEDCFILED")
            {
                DateTime date = dr["dateDCFiled"].ObjToDateTime();
                using (GetDate dateForm = new GetDate(date, "Date DC Filed"))
                {
                    dateForm.TopMost = true;
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        date = dateForm.myDateAnswer;
                        dr["dateDCFiled"] = G1.DTtoMySQLDT(date);
                    }
                }
            }
        }
        /****************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource; // Leave as a GetDate example
            AddMod(dt, gridMain);

            var hitInfo = gridMain.CalcHitInfo(e.Location);
            if (hitInfo.InRowCell)
            {
                int rowHandle = hitInfo.RowHandle;
                GridColumn column = hitInfo.Column;
                string currentColumn = column.FieldName.Trim();
                if ( currentColumn.ToUpper() == "DATERECEIVED")
                {
                    DataRow dr = gridMain.GetFocusedDataRow();
                    DateTime date = dr["dateReceived"].ObjToDateTime();
                    using (GetDate dateForm = new GetDate(date, "Date Received"))
                    {
                        dateForm.TopMost = true;
                        dateForm.ShowDialog();
                        if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                        {
                            date = dateForm.myDateAnswer;
                            dr["dateReceived"] = G1.DTtoMySQLDT(date);
                            dr["mod"] = "Y";
                        }
                    }
                }
                else if ( currentColumn.ToUpper() == "LOCALDESCRIPTION")
                {
                    tryBankSetup();
                }
            }
        }
        /***********************************************************************************************/
        private void tryBankSetup()
        { // Location Changed
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);

            //string location = dr["location"].ObjToString();
            string paymentType = dr["type"].ObjToString();
            GetProperBank(paymentType); // This should cleanup the bank dropdown when bank selection is selected
            //if (!String.IsNullOrWhiteSpace(paymentType))
            //{
            //    //dr["paymentType"] = "Cash";
            //    //dr["bankAccount"] = "";
            //    //dr["localDescription"] = "";
            //    //paymentType = "";
            //    string bankAccount = GetDepositBankAccount(paymentType );

            //    //string bankAccount = GetDepositBankAccount(paymentType, location);
            //    //dr["bankAccount"] = bankAccount;
            //    dt.Rows[row]["bankAccount"] = bankAccount;
            //    dgv.RefreshDataSource();
            //    dgv.Refresh();
            //}
        }
        /****************************************************************************************/
        private void repositoryItemComboBox2_Validating(object sender, CancelEventArgs e)
        {
        }
        /****************************************************************************************/
        private void recalcAmountFiledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();

            string type = dr["type"].ObjToString();
            if (type.ToUpper() != "TRUST")
                return;
            DateTime depositDate = dr["dateReceived"].ObjToDateTime();
            DateTime dateEOM = G1.GetDateEOM(depositDate);
            string paymentRecord = dr["paymentRecord"].ObjToString();
            if (String.IsNullOrWhiteSpace(paymentRecord))
                return;

            this.Cursor = Cursors.WaitCursor;

            string contract = workDR["trust_policy"].ObjToString();
            double unityRefund = getPossibleRefund(contract);
            double endingBalance = 0D;
            double trust85Pending = 0D;
            double beginningBalance = 0D;
            string locind = "";

            CalcTrust2013Actual(contract, ref endingBalance, ref trust85Pending, ref beginningBalance, ref locind);

            if (endingBalance == 0D)
                endingBalance = beginningBalance;
            double totalTrust = endingBalance + trust85Pending;
            totalTrust = endingBalance;
            double dbr = PaymentsReport.isDBR(contract, dateEOM );
            if (dbr > 0D)
                totalTrust = totalTrust - dbr;
            totalTrust = G1.RoundValue(totalTrust);
            dr["trustAmtFiled"] = totalTrust; // Dont do this yet/ Need Confirmation from Cliff/Charlotte

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void goToTBBToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = workDR["trust_policy"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                //this.Hide();
                this.Cursor = Cursors.WaitCursor;
                PayOffDetail detail = new PayOffDetail(contract);
                detail.ShowDialog();
                this.Cursor = Cursors.Default;
                this.Show();
            }
        }
        /****************************************************************************************/
        private void goToDailyHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = workDR["trust_policy"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                //this.Hide();
                this.Cursor = Cursors.WaitCursor;
                //DataTable dt = (DataTable)dgv.DataSource;
                //CustomerDetails clientForm = new CustomerDetails(contract);
                //clientForm.ShowDialog();
                DailyHistory dailyForm = new DailyHistory(contract);
                dailyForm.ShowDialog();
                this.Cursor = Cursors.Default;
                this.Show();
            }
        }
        /****************************************************************************************/
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            try
            {

                DataTable dt = (DataTable)dgv.DataSource;
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                DataRow dr = gridMain.GetFocusedDataRow();
                string record = dr["record"].ObjToString();

                string set = dr["pmtInTransition"].ObjToString();
                if (set != "1")
                    set = "1";
                else
                    set = "0";
                dr["mod"] = "Y";
                dt.Rows[row]["mod"] = "Y";

                dr["pmtInTransition"] = set;
                dt.Rows[row]["pmtInTransition"] = set;
                //gridMain_FocusedRowChanged(null, null);
                dgv.RefreshDataSource();
            }
            catch (Exception ex)
            {
            }
            SetupSave();
            gridMain.RefreshData();
        }
        /****************************************************************************************/
        private void showTrustCompanyMoneyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = workDR["trust_policy"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                contract = workContract;
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                this.Hide();
                TrustDeceased deceased = new TrustDeceased(contract);
                deceased.acceptTrustMoney += Deceased_acceptTrustMoney;
                deceased.ShowDialog();
                this.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private DataTable addTrustRow ( DataTable dx, string trustCompany, double money )
        {
            DataRow dRow = null;
            string trust = "";
            string type = "";
            string paidFrom = "";
            bool found = false;
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                //found = false;
                trust = dx.Rows[i]["type"].ObjToString();
                paidFrom = dx.Rows[i]["paidFrom"].ObjToString();
                if ( trust.Trim().ToUpper() == "TRUST")
                {
                    if ( !String.IsNullOrWhiteSpace ( paidFrom ))
                    {
                        if (paidFrom.Trim().ToUpper() == trustCompany.Trim().ToUpper())
                            continue;
                    }
                    else
                    {
                        dx.Rows[i]["type"] = "Trust";
                        dx.Rows[i]["paidFrom"] = trustCompany;
                        dx.Rows[i]["trustAmtFiled"] = money;
                        dx.Rows[i]["pmtInTransition"] = "0";
                        dx.Rows[i]["discresionaryACH"] = "0";
                        found = true;
                    }
                }
            }
            if (!found)
            {
                dRow = dx.NewRow();
                dRow["type"] = "Trust";
                dRow["paidFrom"] = trustCompany;
                dRow["trustAmtFiled"] = money;
                dRow["pmtInTransition"] = "0";
                dRow["discresionaryACH"] = "0";
                dx.Rows.Add(dRow);
                found = true;
            }
            return dx;
        }
        /****************************************************************************************/
        private void Deceased_acceptTrustMoney(DataTable dt)
        {
            DataTable dx = (DataTable)dgv.DataSource;

            try
            {
                double securityNational = dt.Rows[0]["Security National"].ObjToDouble();
                double forethought = dt.Rows[0]["Forethought"].ObjToDouble();
                double unity = dt.Rows[0]["Unity"].ObjToDouble();
                double fdlic = dt.Rows[0]["FDLIC"].ObjToDouble();

                if (securityNational > 0D)
                    dx = addTrustRow(dx, "Security National", securityNational);
                if (forethought > 0D)
                    dx = addTrustRow(dx, "Forethought", forethought);
                if (unity > 0D)
                    dx = addTrustRow(dx, "Unity", unity);
                if (fdlic > 0D)
                    dx = addTrustRow(dx, "FDLIC", fdlic);
            }
            catch ( Exception ex)
            {
            }

            dgv.DataSource = dx;
            dgv.Refresh();
        }
    }
    /****************************************************************************************/
}