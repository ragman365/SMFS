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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.Grid;
using iTextSharp.text.pdf;
using System.IO;
//using iTextSharp.text;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class EditBankDeposits : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool modified = false;
        private bool Selecting = false;
        private bool loading = true;
        private bool foundLocalPreference = false;
        private string workAccountNo = "";
        private string workAccountTitle = "";
        private string workBankAccount = "";
        private DateTime workDate1 = DateTime.Now;
        private DateTime workDate2 = DateTime.Now;
        /***********************************************************************************************/
        public EditBankDeposits( string accountTitle, string account_no, DateTime date1, DateTime date2, string bankAccount )
        {
            workAccountTitle = accountTitle;
            workAccountNo = account_no;
            workBankAccount = bankAccount;
            workDate1 = date1;
            workDate2 = date2;
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void EditBankDeposits_Load(object sender, EventArgs e)
        {
            loading = true;

            cmbSelectColumns.Hide();
            btnSelectColumns.Hide();

            this.dateTimePicker1.Value = workDate1;
            this.dateTimePicker2.Value = workDate2;

            //loadGroupCombo(cmbSelectColumns, "EditBankAccounts", "Primary");

            this.Text = "Bank Account Deposits for " + workAccountTitle + "-" + workAccountNo;

            LoadData();

            loading = false;

            btnRun_Click(null, null);
        }
        /***********************************************************************************************/
        private void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module)
        {
            string primaryName = "";
            cmb.Items.Clear();
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' group by name;";
            DataTable dt = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Name"].ToString();
                if (name.Trim().ToUpper() == "PRIMARY")
                    primaryName = name;
                cmb.Items.Add(name);
            }
            if (!String.IsNullOrWhiteSpace(primaryName))
                cmb.Text = primaryName;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;

            string cmd = "Select * from `bank_accounts` WHERE `location` = 'XyZZy54321' ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("deposits", Type.GetType("System.Double"));
            dt.Columns.Add("insuranceDeposits", Type.GetType("System.Double"));
            dt.Columns.Add("funeralDeposits", Type.GetType("System.Double"));
            dt.Columns.Add("totalDeposits", Type.GetType("System.Double"));

            dt.Columns.Add("type");
            dt.Columns.Add("account");
            dt.Columns.Add("lastName");
            dt.Columns.Add("firstName");
            dt.Columns.Add("depositDate");

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
            modified = false;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("deposits", null);
            AddSummaryColumn("insuranceDeposits", null);
            AddSummaryColumn("funeralDeposits", null);
            AddSummaryColumn("totalDeposits", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void EditBankAccounts_FormClosing(object sender, FormClosingEventArgs e)
        {
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
            Printer.DrawQuad(5, 8, 6, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
//            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void massRowsUp(DataTable dt, int row)
        {
            int[] rows = gridMain.GetSelectedRows();
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            try
            {
                G1.NumberDataTable(dt);
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["Count"] = i.ToString();
                int moverow = rows[0];
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dt.Rows[row]["Count"] = (row - 1).ToString();
                    //dt.Rows[row - 1]["Count"] = row.ToString();
                    //var dRow = gridMain.GetDataRow(row);
                    dt.Rows[row]["mod"] = "M";
                    modified = true;
                }
                dt.Rows[moverow - 1]["Count"] = (moverow + (rows.Length - 1)).ToString();
                G1.sortTable(dt, "Count", "asc");
                dt.Columns.Remove("Count");
                G1.NumberDataTable(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
            dgv.DataSource = dt;
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditHelp helpForm = new EditHelp( "customers" );
            helpForm.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "mod") < 0)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                if (e.RowHandle >= 0)
                {
                    string data = e.DisplayText.ObjToString();
                    if ( G1.validate_numeric ( data ))
                    {
                        double dvalue = data.ObjToDouble();
                        e.DisplayText = G1.ReformatMoney(dvalue);
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    e.DisplayText = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + date.Year.ToString("D4");
                    if (date.Year == 1)
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                string str = View.GetRowCellValue(e.RowHandle, "data").ObjToString();
                if (str != null)
                {
                    if (G1.validate_numeric(str))
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                }
            }
        }
        /****************************************************************************************/
        private string oldWhat = "";
        /***********************************************************************************************/
        private void gridMain_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            GridView view = sender as GridView;
            if (view.FocusedColumn.FieldName.ToUpper() == "ASOFDATE")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataRow dr = gridMain.GetFocusedDataRow();
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                oldWhat = e.Value.ObjToString();
                DateTime date = oldWhat.ObjToDateTime();
                dt.Rows[row]["ASOFDATE"] = G1.DTtoMySQLDT(date);
                e.Value = G1.DTtoMySQLDT(date);
            }
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            now = now.AddMonths(1);
            now = new DateTime(now.Year, now.Month, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            this.dateTimePicker2.Value = new DateTime(now.Year, now.Month, days);
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {

            DataTable dt = (DataTable)dgv.DataSource;

            try
            {
                SetupTotalsSummary();

                DateTime date1 = this.dateTimePicker1.Value;
                DateTime date2 = this.dateTimePicker2.Value;

                string sDate = date1.ToString("yyyy-MM-dd");
                string eDate = date2.ToString("yyyy-MM-dd");

                string cmd = "";
                DataTable dx = null;
                DataTable dxx = null;

                int length = 0;
                int start = 0;
                string search = workBankAccount;

                string bankAccount = "";

                double totalDeposits = 0D;
                double payment = 0D;
                double downPayment = 0D;

                double totalInsurance = 0D;
                double totalFuneral = 0D;
                double totalTotal = 0D;
                string depositNumber = "";

                string account = "";
                string firstName = "";
                string lastName = "";
                string depositDate = "";

                DataRow dRow = null;

                cmd = "Select * from `payments` where `payDate8` >= '" + sDate + "' AND `payDate8` <= '" + eDate + "' AND `bank_account` LIKE '%" + search + "';";
                dx = G1.get_db_data(cmd);
                totalDeposits = 0D;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    account = dx.Rows[j]["contractNumber"].ObjToString();
                    lastName = dx.Rows[j]["lastName"].ObjToString();
                    firstName = dx.Rows[j]["firstName"].ObjToString();
                    depositDate = dx.Rows[j]["payDate8"].ObjToDateTime().ToString("yyyy-MM-dd");

                    payment = DailyHistory.getPayment(dx, j);
                    downPayment = DailyHistory.getDownPayment(dx, j);

                    if (downPayment > 0D && payment == 0D)
                        continue;

                    dRow = dt.NewRow();
                    dRow["type"] = "Trust";
                    dRow["account"] = account;
                    dRow["lastName"] = lastName;
                    dRow["firstName"] = firstName;
                    dRow["deposits"] = payment;
                    dRow["depositDate"] = depositDate;
                    dt.Rows.Add(dRow);

                    totalDeposits += payment;
                }

                cmd = "Select * from `downpayments` where `date` >= '" + sDate + "' AND `date` <= '" + eDate + "' AND `bankAccount` LIKE '%" + search + "';";
                dx = G1.get_db_data(cmd);
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    account = "Down Payment";
                    lastName = dx.Rows[j]["lastName"].ObjToString();
                    firstName = dx.Rows[j]["firstName"].ObjToString();
                    depositDate = dx.Rows[j]["date"].ObjToDateTime().ToString("yyyy-MM-dd");

                    payment = dx.Rows[j]["totalDeposit"].ObjToDouble();

                    dRow = dt.NewRow();
                    dRow["type"] = "Trust";
                    dRow["account"] = account;
                    dRow["lastName"] = lastName;
                    dRow["firstName"] = firstName;
                    dRow["deposits"] = payment;
                    dRow["depositDate"] = depositDate;
                    dt.Rows.Add(dRow);

                    totalDeposits += payment;
                }
                totalDeposits = G1.RoundValue(totalDeposits);
//                dt.Rows[i]["deposits"] = totalDeposits;

                totalDeposits = G1.RoundValue(totalDeposits);
                //dt.Rows[i]["deposits"] = totalDeposits;

                cmd = "Select * from `ipayments` where `payDate8` >= '" + sDate + "' AND `payDate8` <= '" + eDate + "' AND `bank_account` LIKE '%" + search + "';";
                dx = G1.get_db_data(cmd);
                totalInsurance = 0D;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    account = dx.Rows[j]["payer"].ObjToString();
                    lastName = dx.Rows[j]["lastName"].ObjToString();
                    firstName = dx.Rows[j]["firstName"].ObjToString();
                    depositDate = dx.Rows[j]["payDate8"].ObjToDateTime().ToString("yyyy-MM-dd");

                    payment = DailyHistory.getPayment(dx, j);

                    dRow = dt.NewRow();
                    dRow["type"] = "Insurance";
                    dRow["account"] = account;
                    dRow["lastName"] = lastName;
                    dRow["firstName"] = firstName;
                    dRow["insuranceDeposits"] = payment;
                    dRow["depositDate"] = depositDate;
                    dt.Rows.Add(dRow);

                    totalInsurance += payment;
                }
                totalInsurance = G1.RoundValue(totalInsurance);
                //dt.Rows[i]["insuranceDeposits"] = totalInsurance;

                cmd = "Select * from `cust_payment_details` where `dateReceived` >= '" + sDate + "' AND `dateReceived` <= '" + eDate + "' AND `bankAccount` LIKE '%" + search + "';";
                dx = G1.get_db_data(cmd);
                totalFuneral = 0D;
                for (int j = 0; j < dx.Rows.Count; j++)
                {
                    depositNumber = dx.Rows[j]["depositNumber"].ObjToString().ToUpper();
                    if (depositNumber.IndexOf("TD") < 0 && depositNumber.IndexOf("CCT") < 0)
                    {
                        firstName = "";
                        lastName = "";

                        account = dx.Rows[j]["contractNumber"].ObjToString();
                        cmd = "Select * from `fcustomers` WHERE `contractNumber` = '" + account + "';";
                        dxx = G1.get_db_data(cmd);
                        if ( dxx.Rows.Count > 0 )
                        {
                            lastName = dxx.Rows[0]["lastName"].ObjToString();
                            firstName = dxx.Rows[0]["firstName"].ObjToString();
                            account = dxx.Rows[0]["serviceId"].ObjToString();
                        }

                        payment = dx.Rows[j]["paid"].ObjToDouble();
                        depositDate = dx.Rows[j]["dateReceived"].ObjToDateTime().ToString("yyyy-MM-dd");

                        dRow = dt.NewRow();
                        dRow["type"] = "Funeral";
                        dRow["account"] = account;
                        dRow["lastName"] = lastName;
                        dRow["firstName"] = firstName;
                        dRow["funeralDeposits"] = payment;
                        dRow["depositDate"] = depositDate;
                        dt.Rows.Add(dRow);

                        totalFuneral += payment;
                    }
                }
                totalFuneral = G1.RoundValue(totalFuneral);
                //dt.Rows[i]["funeralDeposits"] = totalFuneral;

                totalTotal = totalDeposits + totalInsurance + totalFuneral;
                //dt.Rows[i]["totalDeposits"] = totalTotal;

                totalDeposits = 0D;
                double trustDeposits = 0D;
                double insuranceDeposits = 0D;
                double funeralDeposits = 0D;


                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    trustDeposits = dt.Rows[i]["deposits"].ObjToDouble();
                    insuranceDeposits = dt.Rows[i]["insuranceDeposits"].ObjToDouble();
                    funeralDeposits = dt.Rows[i]["funeralDeposits"].ObjToDouble();

                    totalDeposits = trustDeposits + insuranceDeposits + funeralDeposits;
                    dt.Rows[i]["totalDeposits"] = totalDeposits;
                }
            }
            catch (Exception ex)
            {
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "depositDate";
            dt = tempview.ToTable();

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void btnSelectColumns_Click(object sender, EventArgs e)
        {
            string actualName = cmbSelectColumns.Text;
            SelectColumns sform = new SelectColumns(dgv, "EditBankAccounts", "Primary", actualName);
            sform.Done += new SelectColumns.d_void_eventdone(sform_Done);
            sform.Show();
        }
        /***********************************************************************************************/
        void sform_Done(DataTable dt)
        {
            string name = cmbSelectColumns.Text.Trim();
            if (String.IsNullOrWhiteSpace(name))
                name = "Primary";
            string saveName = "EditBankAccounts " + name;
            string skinName = "";
            SetupSelectedColumns("EditBankAccounts", name, dgv);
            foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, saveName, ref skinName);
            gridMain.OptionsView.ShowFooter = true;
            SetupTotalsSummary();
            string field = "";
            string select = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                select = dt.Rows[i]["select"].ObjToString();
                if (G1.get_column_number(gridMain, field) >= 0)
                {
                    if (select == "0")
                        gridMain.Columns[field].Visible = false;
                    else
                        gridMain.Columns[field].Visible = true;
                }
            }
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        void sform_Done()
        {
            dgv.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void SetupSelectedColumns(string procType, string group, DevExpress.XtraGrid.GridControl dgv)
        {
            if (String.IsNullOrWhiteSpace(group))
                return;
            if (String.IsNullOrWhiteSpace(procType))
                procType = "";
            string cmd = "Select * from procfiles where name = '" + group + "' and ProcType = '" + procType + "' order by seq";
            DataTable dt = G1.get_db_data(cmd);
            DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain = (DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView)dgv.MainView;
            for (int i = 0; i < gridMain.Columns.Count; i++)
                gridMain.Columns[i].Visible = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["Description"].ToString();
                int index = dt.Rows[i]["seq"].ObjToInt32();
                try
                {
                    ((GridView)dgv.MainView).Columns[name].Visible = true;
                }
                catch
                {
                }
            }
        }
        /***********************************************************************************************/
        private void cmbSelectColumns_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            //ComboBox combo = (ComboBox)sender;
            //string comboName = combo.Text;
            string comboName = cmbSelectColumns.Text;
            string skinName = "";
            if (!String.IsNullOrWhiteSpace(comboName))
            {
                SetupSelectedColumns("EditBankAccounts", comboName, dgv);
                string name = "EditBankAccounts " + comboName;
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                SetupTotalsSummary();
                gridMain.OptionsView.ShowFooter = true;
            }
            else
            {
                SetupSelectedColumns("EditBankAccounts", "Primary", dgv);
                string name = "EditBankAccounts Primary";
                foundLocalPreference = G1.RestoreGridLayout(this, this.dgv, gridMain, LoginForm.username, name, ref skinName);
                gridMain.OptionsView.ShowFooter = true;
                SetupTotalsSummary();
            }
        }
        /***********************************************************************************************/
        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroup.Checked)
            {
                gridMain.Columns["type"].GroupIndex = 0;
                gridMain.RefreshEditor(true);
                gridMain.ExpandAllGroups();
            }
            else
            {
                gridMain.Columns["type"].GroupIndex = -1;
                gridMain.RefreshEditor(true);
            }

            dgv.Refresh();
        }
        /***********************************************************************************************/
    }
}