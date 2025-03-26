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
using DevExpress.XtraGrid.Views.Base;
using DevExpress.Xpo.Helpers;
using System.IO;
using ExcelLibrary.BinaryFileFormat;
using System.Security.Cryptography;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.Office.Utils;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.Utils.Drawing;
using System.Drawing.Drawing2D;
using DevExpress.Data;
//using java.awt;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CCBankReport : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool saveChangedData = false;
        /***********************************************************************************************/
        public CCBankReport()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void CCBankReport_Load(object sender, EventArgs e)
        {
            picAdd.Hide();

            btnSaveNotPosted.Hide();

            SetupTotalsSummary();

            GetBankAccounts();

            GridGroupSummaryItem item = new GridGroupSummaryItem()
            {
                FieldName = "name",
                SummaryType = DevExpress.Data.SummaryItemType.Custom,
                ShowInGroupColumnFooter = gridMain.Columns["name"]
            };
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("authorizedAmount", null);
            AddSummaryColumn("transactionAmount", null);
            AddSummaryColumn("amount", null);
            AddSummaryColumn("salesTax", null);
            AddSummaryColumn("fee", null);
            AddSummaryColumn("ccFee", null);
            AddSummaryColumn("surCharge", null);
            AddSummaryColumn("charge", null);
            AddSummaryColumn("lossRecovery", null);
            AddSummaryColumn("principal", null);
            AddSummaryColumn("interest", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            //if (String.IsNullOrWhiteSpace(format))
            //    format = "${0:0,0.00}";
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            if (grid.OptionsFind.AlwaysVisible == true)
                grid.OptionsFind.AlwaysVisible = false;
            else
                grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
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
            //else if (e.Column.FieldName.ToUpper().IndexOf("CODE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            //{
            //    int row = e.ListSourceRowIndex;
            //    DataTable dt = (DataTable)dgv.DataSource;
            //    string payer = dt.Rows[row]["payer"].ObjToString();
            //    string contractNumber = dt.Rows[row]["contractNumber"].ObjToString();
            //    if (!String.IsNullOrWhiteSpace(payer))
            //        e.DisplayText = "02";
            //    else if (contractNumber.ToUpper().Contains("ZZ"))
            //        e.DisplayText = "02";
            //}
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["invoiceNumber"].ObjToString();
            string[] Lines = contract.Split(' ');
            if (Lines.Length <= 0)
                return;
            contract = Lines[0].Trim();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string what = dr["trustFuneral"].ObjToString();
                if ( what.ToUpper() == "TRUST")
                {
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                }
                else if (what.ToUpper() == "INSURANCE")
                {
                    string cmd = "Select * from `payers` where `payer` = '" + contract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count == 0 )
                    {
                        cmd = "Select * from `icustomers` where `payer` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                    }
                    if (dx.Rows.Count > 0)
                    {
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                        CustomerDetails clientForm = new CustomerDetails(contract);
                        clientForm.Show();
                    }
                }
                else if (what.ToUpper() == "DOWNPMT")
                {
                    string invoiceNumber = dr["invoiceNumber"].ObjToString();
                    Lines = invoiceNumber.Split(' ');
                    if (Lines.Length >= 2)
                    {
                        int count = Lines.Length;
                        string fname = Lines[count - 2];
                        string lname = Lines[count - 1];
                        if (!String.IsNullOrWhiteSpace(fname) && !String.IsNullOrWhiteSpace(lname))
                        {
                            string cmd = "Select * from `downpayments` where `lastName` = '" + lname + "' AND `firstName` = '" + fname + "' LIMIT 10;";
                            DataTable dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                DownPayments dpForm = new DownPayments(dx);
                                dpForm.Show();
                            }
                            else
                            {
                                MessageBox.Show("*** ERROR *** Cannot find any Down Payments for :\n" + fname + " " + lname + "!!!", "Down Payment Lookup Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            }
                        }
                    }
                }
                else
                {
                    string serviceId = contract;
                    string cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + contract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                        EditCust custForm = new EditCust(contract);
                        custForm.Show();
                    }
                    else
                        MessageBox.Show("*** ERROR *** Funeral Service ID : " + serviceId + " does not exist!!!", "Funeral Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
            {
                DataRow dr = gridMain.GetFocusedDataRow();
                string record = dr["record"].ObjToString();
                string contractNumber = dr["invoiceNumber"].ObjToString();
                DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Bank CC (" + contractNumber + ") ?", "Delete Bank CC Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                    return;

                DataTable dt = (DataTable)dgv.DataSource;
                int rowHandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowHandle);
                dt.Rows.RemoveAt(row);
                gridMain.ClearSelection();
                G1.delete_db_table("bank_cc", "record", record);
                string who = contractNumber;
                G1.AddToAudit(LoginForm.username, "Bank CC", "CC", "CC Payment Removed for " + who, contractNumber);
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private bool isPrinting = false;
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
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

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
            isPrinting = false;
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

            font = new Font("Ariel", 12);
            string text = "Credit Card Cover Sheet";
            if ( dgv.Visible )
            {
                Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }

            font = new Font("Ariel", 10, FontStyle.Bold);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            isPrinting = true;
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

            Printer.setupPrinterMargins(50, 100, 80, 50);

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
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            //DataRow dr = gridMain.GetFocusedDataRow();
            //DataTable dt = (DataTable)dgv.DataSource;
            //if ( dt == null)
            //{
            //    string cmd = "Select * from `ach` where `contractNumber` = 'ABCDXXX';";
            //    dt = G1.get_db_data(cmd);
            //    dt.Columns.Add("effectiveDate");
            //    dt.Columns.Add("name");
            //    dt.Columns.Add("ID");
            //    dt.Columns.Add("DebitCredit");
            //    dt.Columns.Add("status");
            //    dt.Columns.Add("backupName");
            //}

            //DateTime effectiveDate = this.dateTimePicker1.Value;
            //using (ACHExtraPayment extraForm = new ACHExtraPayment(dt, effectiveDate))
            //{
            //    DialogResult result = extraForm.ShowDialog();
            //    if (result != DialogResult.OK)
            //        return;
            //    DataTable dx = (DataTable)extraForm.ACH_Answer;
            //    if (dx != null)
            //    {
            //        int row = 0;
            //        for (int i = 0; i < dx.Rows.Count; i++)
            //        {
            //            dt.ImportRow(dx.Rows[i]);
            //            row = dt.Rows.Count - 1;
            //            dt.Rows[row]["backupName"] = dt.Rows[row]["name"].ObjToString();
            //        }
            //        G1.NumberDataTable(dt);
            //        dgv.DataSource = dt;
            //        dgv.Refresh();
            //    }
            //}
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            string field = (e.Item as GridSummaryItem).FieldName.ObjToString();
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
        }
        /***********************************************************************************************/
        private string actualFile = "";
        private void btnPullFile_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `bank_cc` WHERE `posted` = 'Y' AND `transactionType` <> 'Decline' AND `transactionType` <> 'Void';";
            DateTime date = this.dateTimePicker1.Value;
            string date1 = date.ToString("yyyy-MM-dd 00:00:00");

            date = this.dateTimePicker2.Value;
            string date2 = date.ToString("yyyy-MM-dd 23:59:59");
            cmd = "Select * from `bank_cc` WHERE `posted` = 'Y' AND `postedDate` >= '" + date1 + "' AND `postedDate` <= '" + date2 + "' AND `transactionType` <> 'Decline' AND `transactionType` <> 'Void';";

            if ( chkTransactionDate.Checked )
                cmd = "Select * from `bank_cc` WHERE `transactionDate` >= '" + date1 + "' AND `transactionDate` <= '" + date2 + "' AND `transactionType` <> 'Decline' AND `transactionType` <> 'Void';";

            //cmd = "Select * from `bank_cc` WHERE `transactionType` <> 'Decline' AND `transactionType` <> 'Void';";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("mod");
            dt.Columns.Add("name");
            dt.Columns.Add("contractNumber");
            dt.Columns.Add("tf");
            dt.Columns.Add("charge", Type.GetType("System.Double"));
            dt.Columns.Add("lossRecovery", Type.GetType("System.Double"));
            dt.Columns.Add("principal", Type.GetType("System.Double"));
            dt.Columns.Add("interest", Type.GetType("System.Double"));

            this.Cursor = Cursors.WaitCursor;

            ProcessData(dt);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;

            gridMain.Columns["name"].SummaryItem.DisplayFormat = "T85";

            string str = "";

            for (int i = 0; i < gridMain.GroupSummary.Count; i++)
            {
                str = gridMain.GroupSummary[i].FieldName.Trim().ToUpper();
                if (str == "NAME")
                {
                    gridMain.GroupSummary[i].DisplayFormat = "T86";
                }
            }

            gridMain.ExpandAllGroups();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void ProcessData(DataTable dt)
        {
            string invoiceNumber = "";
            string depositNumber = "";
            double amount = 0D;
            double lossRecovery = 0D;
            double charge = 0D;
            double principal = 0D;
            double interest = 0D;
            double ccFee = 0D;
            string[] Lines = null;
            DataTable dx = null;
            DataTable ddx = null;
            string cmd = "";
            string what = "";
            string contract = "";
            string postedDate = "";
            string findRecord = "";
            double newTrust85 = 0D;
            double newTrust100 = 0D;
            string tf = "";
            string fname = "";
            string lname = "";
            string payer = "";
            string serviceId = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                amount = dt.Rows[i]["amount"].ObjToDouble();
                postedDate = dt.Rows[i]["postedDate"].ObjToDateTime().ToString("yyyy-MM-dd");
                contract = dt.Rows[i]["invoiceNumber"].ObjToString();
                Lines = contract.Split(' ');
                if (Lines.Length <= 0)
                    return;
                what = dt.Rows[i]["trustFuneral"].ObjToString();
                contract = Lines[0].Trim();

                contract = Lines[0].Trim();
                if (!String.IsNullOrWhiteSpace(contract))
                {
                    if (what.ToUpper() == "TRUST")
                    {
                        tf = "Trust Payments";
                        cmd = "Select * from `creditcards` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count > 0 )
                            tf = "Trust Drafts";
                        dt.Rows[i]["tf"] = tf;

                        cmd = "Select * from `customers` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                            continue;
                        fname = dx.Rows[0]["firstName"].ObjToString();
                        lname = dx.Rows[0]["lastName"].ObjToString();
                        dt.Rows[i]["name"] = fname + " " + lname;
                        dt.Rows[i]["contractNumber"] = contract;
                        dt.Rows[i]["lossRecovery"] = 0D;
                        cmd = "Select * from `payments` where `contractNumber` = '" + contract + "' AND `depositNumber` = '" + depositNumber + "' AND `payDate8` = '" + postedDate + "';";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count > 0 )
                        {
                            findRecord = dx.Rows[0]["record"].ObjToString();
                            ccFee = dx.Rows[0]["ccFee"].ObjToDouble();
                            if (!String.IsNullOrWhiteSpace(findRecord))
                            {
                                ddx = DailyHistory.CalcPaymentData(contract, findRecord, ref interest, ref newTrust85, ref newTrust100);
                                dt.Rows[i]["interest"] = interest;
                                principal = amount - interest - ccFee;
                                principal = G1.RoundValue(principal);
                                if (ddx != null)
                                    principal = ddx.Rows[0]["prince"].ObjToDouble();
                                dt.Rows[i]["principal"] = principal;
                                dt.Rows[i]["ccFee"] = ccFee;
                                dt.Rows[i]["charge"] = amount;
                            }

                        }
                        //CustomerDetails clientForm = new CustomerDetails(contract);
                        //clientForm.Show();
                    }
                    else if (what.ToUpper() == "INSURANCE")
                    {
                        payer = contract;
                        tf = "Insurance Payments";
                        cmd = "Select * from `creditcards` where `contractNumber` = '" + payer + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            tf = "Insurance Drafts";
                        dt.Rows[i]["tf"] = tf;

                        cmd = "Select * from `payers` where `payer` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count == 0)
                        {
                            cmd = "Select * from `icustomers` where `payer` = '" + contract + "';";
                            dx = G1.get_db_data(cmd);
                        }
                        if (dx.Rows.Count > 0)
                        {
                            contract = dx.Rows[0]["contractNumber"].ObjToString();
                            fname = dx.Rows[0]["firstName"].ObjToString();
                            lname = dx.Rows[0]["lastName"].ObjToString();
                            dt.Rows[i]["name"] = fname + " " + lname;
                            dt.Rows[i]["contractNumber"] = payer;
                            dt.Rows[i]["lossRecovery"] = 0D;
                            dt.Rows[i]["interest"] = 0D;
                            cmd = "Select * from `ipayments` where `contractNumber` = '" + contract + "' AND `depositNumber` = '" + depositNumber + "' AND `payDate8` = '" + postedDate + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                            {
                                ccFee = dx.Rows[0]["ccFee"].ObjToDouble();
                                principal = amount - - ccFee;
                                principal = G1.RoundValue(principal);
                                dt.Rows[i]["principal"] = principal;
                                dt.Rows[i]["ccFee"] = ccFee;
                                dt.Rows[i]["charge"] = amount;
                            }
                            //CustomerDetails clientForm = new CustomerDetails(contract);
                            //clientForm.Show();
                            //CustomerDetails clientForm = new CustomerDetails(contract);
                            //clientForm.Show();
                        }
                    }
                    else if (what.ToUpper() == "DOWNPMT")
                    {
                        dt.Rows[i]["tf"] = "Trust Down Payments";
                        invoiceNumber = dt.Rows[i]["invoiceNumber"].ObjToString();
                        Lines = invoiceNumber.Split(' ');
                        if (Lines.Length >= 2)
                        {
                            int count = Lines.Length;
                            fname = Lines[count - 2];
                            lname = Lines[count - 1];

                            if (!String.IsNullOrWhiteSpace(fname) && !String.IsNullOrWhiteSpace(lname))
                            {
                                cmd = "Select * from `downpayments` where `lastName` = '" + lname + "' AND `firstName` = '" + fname + "' AND `depositNumber` = '" + depositNumber + "' AND `downPayment` = '" + amount.ToString() + "';";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                {
                                    lossRecovery = dx.Rows[0]["lossRecoveryFee"].ObjToDouble();
                                    dt.Rows[i]["lossRecovery"] = lossRecovery;
                                    ccFee = dx.Rows[0]["ccFee"].ObjToDouble();
                                    charge = dx.Rows[0]["downPayment"].ObjToDouble() + ccFee + lossRecovery;
                                    dt.Rows[i]["charge"] = charge;
                                    dt.Rows[i]["name"] = fname + " " + lname;
                                    dt.Rows[i]["principal"] = charge - ccFee;
                                    dt.Rows[i]["interest"] = 0D;
                                    dt.Rows[i]["contractNumber"] = "N/A";
                                }
                                else
                                {
                                    //MessageBox.Show("*** ERROR *** Cannot find any Down Payments for :\n" + fname + " " + lname + "!!!", "Down Payment Lookup Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                }
                            }
                        }
                    }
                    else if ( what.ToUpper() == "FUNERAL")
                    {
                        serviceId = contract;
                        cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            contract = dx.Rows[0]["contractNumber"].ObjToString();

                            tf = "Funeral Payments";
                            cmd = "Select * from `creditcards` where `contractNumber` = '" + contract + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count > 0)
                                tf = "Funeral Drafts";
                            dt.Rows[i]["tf"] = tf;

                            cmd = "Select * from `fcustomers` where `contractNumber` = '" + contract + "';";
                            dx = G1.get_db_data(cmd);
                            if (dx.Rows.Count <= 0)
                                continue;
                            fname = dx.Rows[0]["firstName"].ObjToString();
                            lname = dx.Rows[0]["lastName"].ObjToString();
                            dt.Rows[i]["name"] = fname + " " + lname;
                            dt.Rows[i]["contractNumber"] = serviceId;
                            dt.Rows[i]["lossRecovery"] = 0D;
                            dt.Rows[i]["interest"] = 0D;
                            dt.Rows[i]["principal"] = amount - ccFee;
                            dt.Rows[i]["ccFee"] = ccFee;
                            dt.Rows[i]["charge"] = amount;
                            cmd = "Select * from `cust_payment_details` where `contractNumber` = '" + contract + "' AND `depositNumber` = '" + depositNumber + "' AND `dateReceived` = '" + postedDate + "';";
                            //dx = G1.get_db_data(cmd);
                            //if ( dx.Rows.Count > 0 )
                            //{

                            //}
                        }
                        //else
                        //    MessageBox.Show("*** ERROR *** Funeral Service ID : " + serviceId + " does not exist!!!", "Funeral Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void btnSaveNotPosted_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string referenceNumber = "";
            string transactionType = "";
            string cardNumber = "";
            string paymentType = "";
            double authorizedAmount = 0D;
            double transactionAmount = 0D;
            string returnVoid = "";
            double amount = 0D;
            double salesTax = 0D;
            double fee = 0D;
            double surCharge = 0D;
            string invoiceNumber = "";
            string trustFuneral = "";
            string depositNumber = "";
            string mod = "";
            string transactionDate = "";
            string postedDate = "";

            string cmd = "Delete from `bank_cc` WHERE `paymentType` = '-1';";
            G1.get_db_data(cmd);
            DataTable dx = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod != "Y")
                        continue;

                    referenceNumber = dt.Rows[i]["referenceNumber"].ObjToString().Trim();
                    transactionType = dt.Rows[i]["transactionType"].ObjToString();
                    cardNumber = dt.Rows[i]["cardNumber"].ObjToString();
                    paymentType = dt.Rows[i]["paymentType"].ObjToString();
                    authorizedAmount = dt.Rows[i]["authorizedAmount"].ObjToDouble();
                    transactionAmount = dt.Rows[i]["transactionAmount"].ObjToDouble();
                    returnVoid = dt.Rows[i]["returnVoid"].ObjToString();
                    amount = dt.Rows[i]["amount"].ObjToDouble();
                    salesTax = dt.Rows[i]["salesTax"].ObjToDouble();
                    fee = dt.Rows[i]["ccFee"].ObjToDouble();
                    surCharge = dt.Rows[i]["surCharge"].ObjToDouble();
                    trustFuneral = dt.Rows[i]["trustFuneral"].ObjToString();
                    invoiceNumber = dt.Rows[i]["invoiceNumber"].ObjToString();
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    transactionDate = dt.Rows[i]["transactionDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                    postedDate = dt.Rows[i]["postedDate"].ObjToDateTime().ToString("MM/dd/yyyy");

                    record = dt.Rows[i]["record"].ObjToString();
                    if (record == "-1")
                        continue;

                    if (G1.BadRecord("bank_cc", record))
                        continue;
                    G1.update_db_table("bank_cc", "record", record, new string[] {"referenceNumber", referenceNumber, "cardNumber", cardNumber, "transactionType", transactionType, "depositNumber", depositNumber, "transactionDate", transactionDate, "postedDate", postedDate,
                        "paymentType", paymentType, "authorizedAmount", authorizedAmount.ToString(), "transactionAmount", transactionAmount.ToString(), "returnVoid", returnVoid, "amount", amount.ToString(), "salesTax", salesTax.ToString(), "ccFee", fee.ToString(), "surCharge", surCharge.ToString(), "trustFuneral", trustFuneral, "invoiceNumber", invoiceNumber });

                    dt.Rows[i]["mod"] = "";
                    dt.Rows[i]["record"] = record.ObjToInt32();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("*** ERROR *** Importing Reference Number : " + referenceNumber + "!!!", "CC Import Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            btnSaveNotPosted.Hide();
            btnSaveNotPosted.Refresh();
        }
        /***********************************************************************************************/
        private string trustBankCC = "";
        private string funeralBankCC = "";
        private void GetBankAccounts ()
        {
            string description = "";
            string location = "";
            string bank_gl = "";
            string bankAccount = "";
            string cc_account = "";

            trustBankCC = "";
            funeralBankCC = "";
            string cmd = "Select * from `bank_accounts` WHERE `localDescription` LIKE 'Credit Card -%';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    description = dx.Rows[i]["localDescription"].ObjToString().ToUpper();
                    location = dx.Rows[i]["location"].ObjToString();
                    bank_gl = dx.Rows[i]["general_ledger_no"].ObjToString();
                    bankAccount = dx.Rows[i]["account_no"].ObjToString();
                    cc_account = location + "~" + bank_gl + "~" + bankAccount;
                    if (description.IndexOf("TRUST") > 0)
                        trustBankCC = cc_account;
                    else if (description.IndexOf("FUNERAL") > 0)
                        funeralBankCC = cc_account;
                }
            }
        }
        /***************************************************************************************/
        private bool chargeCCFee (string workContract, string payer)
        {
            string lookup = workContract;
            if (!String.IsNullOrWhiteSpace(payer))
                lookup = payer;
            string cmd = "Select * from `creditcards` WHERE `contractNumber` = '" + lookup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return true;
            return false;
        }
        /***********************************************************************************************/
        private void duplicatePaymentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["invoiceNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            try
            {
                dr["duplicate"] = "Y";
                dt.Rows[row]["duplicate"] = "Y";
                DataTable tempDt = dt.Clone();
                G1.copy_dt_row(dt, row, tempDt, 0);

                DataRow dR = dt.NewRow();
                dt.Rows.InsertAt(dR, row);
                G1.copy_dt_row(tempDt, 0, dt, row);
            }
            catch (Exception ex)
            {

            }
            dt.AcceptChanges();
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;
            DateTime stop = DateTime.Now;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddDays(-1);
            start = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = start;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime start = DateTime.Now;
            DateTime stop = DateTime.Now;
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddDays(1);
            start = new DateTime(now.Year, now.Month, 1);
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            stop = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker1.Value = start;
            this.dateTimePicker2.Value = start;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();

            dr["mod"] = "Y";

            saveChangedData = true;

            btnSaveNotPosted.Show();
            btnSaveNotPosted.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_ShowingEditor(object sender, CancelEventArgs e)
        {
            //GridView view = sender as GridView;
            //DataRow dr = gridMain.GetFocusedDataRow();
            //string column = gridMain.FocusedColumn.FieldName.ToUpper();

            //int rowHandle = gridMain.FocusedRowHandle;
            //int row = gridMain.GetDataSourceRowIndex(rowHandle);
            //if (column == "POSTEDDATE")
            //{
            //    DateTime date = dr["postedDate"].ObjToDateTime();
            //    dr["postedDate"] = G1.DTtoMySQLDT(date);
            //}
        }
        /***********************************************************************************************/
        private void gridMain_KeyDown(object sender, KeyEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            GridView view = sender as GridView;
            DataRow dr = gridMain.GetFocusedDataRow();
            string column = gridMain.FocusedColumn.FieldName.ToUpper();

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            if (column == "POSTEDDATE")
            {
                DateTime date = dr["postedDate"].ObjToDateTime();
                using (GetDate dateForm = new GetDate(date, "Posted Date"))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        date = dateForm.myDateAnswer;
                        dr["postedDate"] = G1.DTtoMySQLDT(date);
                        dr["mod"] = "Y";
                        btnSaveNotPosted.Show();
                        btnSaveNotPosted.Refresh();
                        saveChangedData = true;
                    }
                }
            }
            else if (column == "TRANSACTIONDATE")
            {
                DateTime date = dr["transactionDate"].ObjToDateTime();
                using (GetDate dateForm = new GetDate(date, "Transaction Date"))
                {
                    dateForm.ShowDialog();
                    if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        date = dateForm.myDateAnswer;
                        dr["transactionDate"] = G1.DTtoMySQLDT(date);
                        dr["mod"] = "Y";
                        btnSaveNotPosted.Show();
                        btnSaveNotPosted.Refresh();
                        saveChangedData = true;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private string groupTotal = "";
        private void gridMain_CustomDrawGroupRow(object sender, RowObjectCustomDrawEventArgs e)
        {
            var view = (GridView)sender;
            var info = (GridGroupRowInfo)e.Info;
            var caption = info.Column.Caption;
            if (info.Column.Caption == string.Empty)
            {
                caption = info.Column.ToString();
            }
            info.GroupText = $"{caption} : {info.GroupValueText} ({view.GetChildRowCount(e.RowHandle)})";
            groupTotal = info.GroupText;
        }
        /***********************************************************************************************/
        private void gridMain_CustomSummaryCalculate_1(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            int summaryID = 0;
            if (e.SummaryProcess == CustomSummaryProcess.Start)
            {
                //totalPrice_8 = 0D;
                //totalTrust85_8 = 0D;
                //GridView view = sender as GridView;
                //summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                //if (summaryID == 3)
                //    localTrust85_8 = 0D;
            }
            double dbr = 0D;
            double trust85 = 0D;
            if (e.SummaryProcess == CustomSummaryProcess.Calculate)
            {
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                //switch (summaryID)
                //{
                //    case 1: // The total summary calculated against the 'UnitPrice' column.  
                //        dbr = gridMain8.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                //        trust85 = gridMain8.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                //        totalPrice_8 += trust85 - dbr;
                //        break;
                //    case 2: // The total summary calculated against the 'UnitPrice' column.  
                //        dbr = gridMain8.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                //        trust85 = gridMain8.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                //        totalTrust85_8 += trust85 - dbr;
                //        break;
                //    case 3: // The total summary calculated against the 'UnitPrice' column.  
                //        dbr = gridMain8.GetRowCellValue(e.RowHandle, "dbr").ObjToDouble();
                //        trust85 = gridMain8.GetRowCellValue(e.RowHandle, "trust85P").ObjToDouble();
                //        localTrust85_8 += trust85 - dbr;
                //        break;
                //}
            }
            if (e.SummaryProcess == CustomSummaryProcess.Finalize)
            {
                GridView view = sender as GridView;
                summaryID = Convert.ToInt32((e.Item as GridSummaryItem).Tag);
                switch (summaryID)
                {
                    case 1:
                        e.TotalValue = "Total 1";
                        break;
                    case 2:
                        e.TotalValue = "Total 2";
                        break;
                    case 3:
                        e.TotalValue = "Total 3";
                        break;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, FooterCellCustomDrawEventArgs e)
        {
            if (1 == 1)
                return;
            string columnName = e.Column.FieldName.ObjToString().ToUpper();
            if (columnName != "NAME")
                return;            
            int rowHandle = e.RowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;
            string location = groupTotal;
            //bandLocation = location;

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
            e.Appearance.DrawString(e.Cache, location, r);
            //            e.Appearance.DrawString(e.Cache, e.Info.DisplayText, r);
            //Prevent default drawing of the cell 
            e.Handled = true;

        }

        private void gridMain_CustomDrawFooter(object sender, RowObjectCustomDrawEventArgs e)
        {

        }

        private void gridMain_CustomDrawGroupRowCell(object sender, RowGroupRowCellEventArgs e)
        {

        }

        private void gridMain_CustomDrawRowFooter(object sender, RowObjectCustomDrawEventArgs e)
        {
            //e.Bounds.Inflate(-5, -5);
            //e.Appearance.ForeColor = Color.Teal;
            //e.Appearance.TextOptions.HAlignment = HorzAlignment.Center;
            //e.Appearance.FontSizeDelta = 3;
            //e.DefaultDraw();
            //e.Cache.DrawRectangle(e.Cache.GetPen(Color.DarkOliveGreen, 5), e.Bounds);
            //e.Handled = true;
        }
        /***********************************************************************************************/
    }
}