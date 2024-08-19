using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using GeneralLib;
using MySql.Data.MySqlClient;
using MySql.Data.Types;

using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Views.BandedGrid;
using System.Drawing.Drawing2D;
using DevExpress.Utils.Drawing;
using DevExpress.XtraGrid.Views.Base;
using Excel = Microsoft.Office.Interop.Excel;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class SplitPolicies : Form
    {
        /***********************************************************************************************/
        private string workContractNumber = "";
        private bool selecting = false;
        private bool loading = true;
        private int saveRow = -1;
        private bool loadAll = false;
        private string workPayer = "";
        private string workContract = "";
        private DataTable workDt = null;

        private string workPayerRecord1 = "";
        private string workPayerRecord2 = "";
        /***********************************************************************************************/
        public SplitPolicies( string contract, string payer, DataTable dt )
        {
            loadAll = true;
            InitializeComponent();
            workContract = contract;
            workPayer = payer;
            workDt = dt;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            if ( G1.get_column_number ( workDt, "annual")< 0 )
                workDt.Columns.Add("annual", Type.GetType("System.Double"));

            LoadAnnual(workContract, workDt);
            workPayerRecord1 = GetPayerRecord(workContract);

            dgv.DataSource = workDt;
            DataTable dx = workDt.Clone();
            dgv2.DataSource = dx;

            txtFromPayer.Text = workPayer;
            txtFromPayer.Enabled = false;

            loadAll = false;
            loading = false;
        }
        /***********************************************************************************************/
        private string GetPayerRecord ( string contractNumber)
        {
            string record = "";
            string cmd = "Select * from `payers` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                record = dt.Rows[0]["record"].ObjToString();
            return record;
        }
        /***********************************************************************************************/
        private void FixAnnualPremium ( DataTable dt, double annual )
        {
            string cnum = "";
            string oldCnum = "";
            string cmd = "";
            DataTable dx = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                cnum = dt.Rows[i]["contractNumber"].ObjToString();
                if (oldCnum == cnum)
                    continue;
                cmd = "Select * from `icontracts` where `contractNumber` = '" + cnum + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    oldCnum = cnum;
                    string record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table("icontracts", "record", record, new string[] { "annualPremium", annual.ToString() });
                }
            }
        }
        /***********************************************************************************************/
        private void LoadAnnual ( string contract, DataTable dx )
        {
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            double annual = dt.Rows[0]["annualPremium"].ObjToDouble();
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                dx.Rows[i]["annual"] = annual;
            }
        }
        /***********************************************************************************************/
        private void LoadDatax()
        {
            loading = true;
            this.Cursor = Cursors.WaitCursor;

            string payerFname = "";
            string payerLname = "";


            string cmd = "Select * from `icustomers` p JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            if (!String.IsNullOrWhiteSpace(workContractNumber))
                cmd += " WHERE p.`contractNumber` = '" + workContractNumber + "' ORDER BY p.`contractNumber` DESC ";
            else
                cmd += " JOIN `policies` q ON p.`contractNumber` = q.`contractNumber` ORDER by q.`payer`";
            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                CustomerDetails clientForm = new CustomerDetails(workContractNumber);
                clientForm.Show();
                return;
            }
            if ( dt.Rows.Count > 0 && !loadAll )
            {
                payerFname = dt.Rows[0]["firstName"].ObjToString();
                payerLname = dt.Rows[0]["lastName"].ObjToString();
                string payer = dt.Rows[0]["payer"].ObjToString();
                if (!String.IsNullOrWhiteSpace(payer))
                {
                    if (!String.IsNullOrWhiteSpace(workContractNumber))
                    {
                        cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
                        if (!String.IsNullOrWhiteSpace(workContractNumber))
                            cmd += " WHERE p.`payer` = '" + payer + "' ORDER BY p.`contractNumber` DESC ";
                        dt = G1.get_db_data(cmd);
                        dt.Columns.Add("ddate");
                        dt.Columns.Add("duedate");
                        DateTime date = DateTime.Now;
                        for ( int i=0; i<dt.Rows.Count; i++)
                        {
                            dt.Rows[i]["firstName"] = payerFname;
                            dt.Rows[i]["lastName"] = payerLname;
                            date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                            dt.Rows[i]["ddate"] = date.ToString("MM/dd/yyyy");
                            date = dt.Rows[i]["dueDate8"].ObjToDateTime();
                            dt.Rows[i]["dueDate"] = date.ToString("MM/dd/yyyy");
                        }
                    }
                }
                else
                {
                    CustomerDetails clientForm = new CustomerDetails(workContractNumber);
                    clientForm.Show();
                    return;
                }
            }
            if (dt.Rows.Count > 0 && !loadAll)
            {
                string name = dt.Rows[0]["firstName"].ObjToString() + " " + dt.Rows[0]["lastName"].ObjToString() + " (" + dt.Rows[0]["payer"].ObjToString() + ")";
                this.Text = name;
            }
            else if ( loadAll)
            {
                this.Text = "List of All Policies";
            }

//            DataRow[] dRow = dt.Select("contractNumber='P16050UI'");
//            int len = dRow.Length;
            dt.Columns.Add("num");
            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("select");
            dt.Columns.Add("fullname");
            dt.Columns.Add("policyfullname");
            if (G1.get_column_number(dt, "ddate") < 0)
                dt.Columns.Add("ddate");
            if (G1.get_column_number(dt, "dueDate") < 0)
                dt.Columns.Add("dueDate");


            G1.NumberDataTable(dt);
            //FixDates(dt, "birthDate", "bDate");
            //FormatSSN(dt, "ssn", "ssno");
            //SetupFullNames(dt);
            //FixDeceasedDate(dt);
            if (selecting)
                gridMain.Columns["select"].Visible = true;

            //if (!String.IsNullOrWhiteSpace(workContractNumber))
            //    SetupPayerLine(dt);
            dgv.DataSource = dt;
            gridBand2.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridMain.Columns["num"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridMain.Columns["contractNumber"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            AddSummaryColumn("premium", gridMain);
            this.Cursor = Cursors.Default;
            loading = false;
            if (saveRow == -2)
            {
                gridMain.FocusedRowHandle = dt.Rows.Count - 1;
                gridMain.SelectRow(dt.Rows.Count-1);
                gridMain.RefreshData();
                dgv.Refresh();
            }
            else if (saveRow >= 0)
            {
                gridMain.FocusedRowHandle = saveRow;
                gridMain.SelectRow(saveRow);
                gridMain.RefreshData();
                dgv.Refresh();
            }
            saveRow = -1;
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
        private void SetupPayerLine ( DataTable dt )
        {
            DataRow dRow = dt.NewRow();
            dRow["contractNumber"] = workContractNumber;
            dt.Rows.InsertAt(dRow, 0);
        }
        /***********************************************************************************************/
        private void FixDeceasedDate(DataTable dt)
        {
            string date1 = "";
            string date2 = "";
            if (G1.get_column_number(dt, "deceasedDate") < 0)
                return;
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    date1 = dt.Rows[i]["deceasedDate"].ObjToString();
            //    if (date1.IndexOf("0000") >= 0)
            //    {
            //        date2 = dt.Rows[i]["deceasedDate1"].ObjToString();
            //        if (date2.IndexOf("0000") < 0)
            //            dt.Rows[i]["deceasedDate"] = dt.Rows[i]["deceasedDate1"];
            //    }
            //}
        }
        /***********************************************************************************************/
        private void SetupFullNames(DataTable dt)
        {
            string fullname = "";
            string fname = "";
            string lname = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                fname = dt.Rows[i]["firstName"].ObjToString();
                lname = dt.Rows[i]["lastName"].ObjToString();
                fullname = fname + " " + lname;
                dt.Rows[i]["fullname"] = fullname;
                fname = dt.Rows[i]["policyFirstName"].ObjToString();
                lname = dt.Rows[i]["policyLastName"].ObjToString();
                fullname = fname + " " + lname;
                dt.Rows[i]["policyfullname"] = fullname;
            }
        }
        /***********************************************************************************************/
        private void FormatSSN(DataTable dt, string columnName, string newColumn)
        {
            string ssn = "";
            string ssno = "";
            int row = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = i;
                    ssn = dt.Rows[i][columnName].ObjToString().Trim();
                    ssn = ssn.Replace("-", "");
                    ssno = ssn;
                    if (ssn.Trim().Length >= 8)
                        try { ssno = "XXX-XX-" + ssn.Substring(5, 4); }
                        catch { }
                    dt.Rows[i][newColumn] = ssno;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Fixing Date Field " + columnName + " Row= " + row + "SSN= " + ssn + " " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void FixDates(DataTable dt, string columnName, string newColumn)
        {
            string date = "";
            long ldate = 0L;
            int row = 0;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    row = i;
                    date = dt.Rows[i][columnName].ObjToString();
                    if (String.IsNullOrWhiteSpace(date))
                        continue;
                    if (date == "0000-00-00")
                    {
                        date = "";
                        dt.Rows[i][columnName] = date;
                    }
                    else
                    {
                        ldate = G1.date_to_days(date);
                        date = G1.days_to_date(ldate);
                        dt.Rows[i][newColumn] = date;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Fixing Date Field " + columnName + " Row= " + row + "Date= " + date + " " + ex.Message.ObjToString());
            }
        }
        /***********************************************************************************************/
        private void SplitPolicies_Load(object sender, EventArgs e)
        {
            if (!loadAll)
            {
                gridMain.Columns["firstName"].Visible = false;
                gridMain.Columns["lastName"].Visible = false;
                gridMain.Columns["policyFirstName"].Visible = false;
                gridMain.Columns["policyLastName"].Visible = false;
                gridMain.Columns["payer"].Visible = false;
                gridMain.Columns["fullname"].Visible = true;
                gridMain.Columns["policyfullname"].Visible = true;
                gridMain.Columns["dueDate8"].Visible = false;
                gridMain.Columns["duedate"].Visible = true;
                gridMain.Columns["beneficiary"].Visible = false;

                gridMain2.Columns["firstName"].Visible = false;
                gridMain2.Columns["lastName"].Visible = false;
                gridMain2.Columns["policyFirstName"].Visible = false;
                gridMain2.Columns["policyLastName"].Visible = false;
                gridMain2.Columns["payer"].Visible = false;
                gridMain2.Columns["fullname"].Visible = true;
                gridMain2.Columns["policyfullname"].Visible = true;
                gridMain2.Columns["dueDate8"].Visible = false;
                gridMain2.Columns["duedate"].Visible = true;
                gridMain2.Columns["beneficiary"].Visible = false;
            }
            else
            {
                gridMain.OptionsView.ShowFooter = false;
                gridMain.Columns["balanceDue"].Visible = false;
                gridMain.Columns["nowDue"].Visible = false;
                gridMain.Columns["firstName"].Visible = true;
                gridMain.Columns["lastName"].Visible = true;
                gridMain.Columns["policyFirstName"].Visible = true;
                gridMain.Columns["policyLastName"].Visible = true;
                gridMain.Columns["payer"].Visible = true;
                gridMain.Columns["fullname"].Visible = false;
                gridMain.Columns["policyfullname"].Visible = false;
                gridMain.Columns["dueDate8"].Visible = true;
                gridMain.Columns["duedate"].Visible = false;
                gridMain.Columns["ddate"].Visible = false;
                gridMain.Columns["deceasedDate2"].Visible = true;
                gridMain.Columns["beneficiary"].Visible = true;
                gridMain.Columns["contractValue"].Visible = false;
                gridMain.Columns["percentPaid"].Visible = false;
                gridMain.Columns["paid"].Visible = false;
                gridMain.Columns["purchase"].Visible = false;

                gridMain2.OptionsView.ShowFooter = false;
                gridMain2.Columns["balanceDue"].Visible = false;
                gridMain2.Columns["nowDue"].Visible = false;
                gridMain2.Columns["firstName"].Visible = true;
                gridMain2.Columns["lastName"].Visible = true;
                gridMain2.Columns["policyFirstName"].Visible = true;
                gridMain2.Columns["policyLastName"].Visible = true;
                gridMain2.Columns["payer"].Visible = true;
                gridMain2.Columns["fullname"].Visible = false;
                gridMain2.Columns["policyfullname"].Visible = false;
                gridMain2.Columns["dueDate8"].Visible = true;
                gridMain2.Columns["duedate"].Visible = false;
                gridMain2.Columns["ddate"].Visible = false;
                gridMain2.Columns["deceasedDate2"].Visible = true;
                gridMain2.Columns["beneficiary"].Visible = true;
                gridMain2.Columns["contractValue"].Visible = false;
                gridMain2.Columns["percentPaid"].Visible = false;
                gridMain2.Columns["paid"].Visible = false;
                gridMain2.Columns["purchase"].Visible = false;
            }

            gridMain.OptionsView.ShowFooter = true;
            gridMain2.OptionsView.ShowFooter = true;
            AddSummaryColumn("premium", gridMain);
            AddSummaryColumn("liability", gridMain);
            AddSummaryColumn("premium", gridMain2);
            AddSummaryColumn("liability", gridMain2);

            LoadData();
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
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** Locating Contract " + contract + "!");
                return;
            }
            string contractRecord = dx.Rows[0]["record"].ObjToString();
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            G1.UpdatePreviousCustomer(contract, LoginForm.username);
            string policyNumber = dr["policyNumber"].ObjToString();
            string policyFirstName = dr["policyFirstName"].ObjToString();
            string policyLastName = dr["policyLastName"].ObjToString();
            string policyRecord = dr["record"].ObjToString();
//            CustomerDetails clientForm = new CustomerDetails(contract, policyRecord);
            CustomerDetails clientForm = new CustomerDetails(contract);
            clientForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf ( "DATE") >= 0 )
            {
                if (e.RowHandle >= 0)
                {
                    if (!String.IsNullOrWhiteSpace(e.DisplayText))
                    {
                        DateTime date = e.DisplayText.ObjToDateTime();
                        if (date.Year < 1850)
                            e.DisplayText = "";
                        else
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            if (!chkFilterDeceased.Checked)
            {
                if (!chkFilterLapsed.Checked )
                    return;
            }
            ColumnView view = sender as ColumnView;
            if (chkFilterDeceased.Checked)
            {
                DateTime deceasedDate = dt.Rows[row]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 100)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if (chkFilterLapsed.Checked)
            {
                DateTime lapsedDate = dt.Rows[row]["lapsedDate8"].ObjToDateTime();
                if (lapsedDate.Year > 100)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
                string lapsed = dt.Rows[row]["lapsed"].ObjToString();
                if (lapsed == "Y")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
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

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
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

            font = new Font("Ariel", 6);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            string title = "Insurance Policies Report for " + this.Text;
            Printer.DrawQuad(4, 8, 9, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(16, 7, 5, 2, lblBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            //Printer.DrawQuad(16, 10, 5, 2, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 8);
//            Printer.DrawQuad(1, 6, 6, 3, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Top);
            Printer.DrawQuad(1, 9, 6, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void changeContractNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if ( !LoginForm.administrator )
            {
                MessageBox.Show("***ERROR*** You do not have permission to do this.");
                return;
            }
            string goodContractNumber = "";

            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataTable dt = (DataTable)dgv.DataSource;

            string badContractNumber = dr["contractNumber"].ObjToString();

            using (Ask askForm = new Ask("Enter Good Contract #?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                goodContractNumber = askForm.Answer;
                if (String.IsNullOrWhiteSpace(goodContractNumber))
                    return;
            }

            DialogResult result = MessageBox.Show("***Warning*** Are you SURE you want to change contract number " + badContractNumber + " to " + goodContractNumber + "?", "Change Contract # Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            this.Cursor = Cursors.WaitCursor;
            ChangeContractNumber("policies", badContractNumber, goodContractNumber);
            ChangeContractNumber("icontracts", badContractNumber, goodContractNumber);
            ChangeContractNumber("icustomers", badContractNumber, goodContractNumber);
            ChangeContractNumber("ipayments", badContractNumber, goodContractNumber);
//            ChangeContractNumber("cust_services", badContractNumber, goodContractNumber);
            dr["contractNumber"] = goodContractNumber;
            dt.Rows[row]["contractNumber"] = goodContractNumber;
            this.Cursor = Cursors.Default;
            MessageBox.Show("***Good News*** Contracts are changes!");
        }
        /***********************************************************************************************/
        private void ChangeContractNumber ( string table, string badContractNumber, string goodContractNumber )
        {
            string record = "";
            string cmd = "Select * from `" + table + "` where `contractNumber` = '" + goodContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***WARNING*** Contract " + goodContractNumber + " Already Exists in " + table + " Table!");
            }
            else
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + badContractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                {
                    MessageBox.Show("***WARNING*** Contract " + badContractNumber + " DOES NOT Exists in " + table + " Table!");
                }
                else
                {
                    for ( int i=0; i<dt.Rows.Count; i++)
                    {
                        record = dt.Rows[i]["record"].ObjToString();
                        G1.update_db_table(table, "record", record, new string[] { "contractNumber", goodContractNumber });
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            if (e.Column.FieldName.ToUpper() == "DUEDATE")
            {
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string cnum = dt.Rows[row]["contractNumber"].ObjToString();
                if ( !String.IsNullOrWhiteSpace (cnum))
                {
                    string date = e.Value.ObjToString();
                    date = dr["dueDate"].ObjToString();
                    if (G1.validate_date(date))
                    {
                        MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                        string dueDate = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        string record = dt.Rows[row]["record1"].ObjToString();
                        G1.update_db_table("icontracts", "record", record, new string[] {"dueDate8", dueDate });
                        G1.update_db_table("payers", "record", workPayerRecord1, new string[] { "dueDate8", dueDate });
                        BandedGridView view = sender as BandedGridView;
                        loading = true;
                        view.SetRowCellValue(e.RowHandle, view.Columns["dueDate"], dueDate);
                        dr["dueDate"] = myDate;
                        loading = false;
                        gridMain.RefreshData();
                        this.Refresh();
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DDATE")
            {
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string cnum = dt.Rows[row]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(cnum))
                {
                    string date = e.Value.ObjToString();
                    date = dr["ddate"].ObjToString();
                    if (G1.validate_date(date))
                    {
                        MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                        string deceasedDate = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        string record = dt.Rows[row]["record"].ObjToString();
                        G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", deceasedDate });
                        BandedGridView view = sender as BandedGridView;
                        loading = true;
//                        view.SetRowCellValue(e.RowHandle, view.Columns["deceasedDate"], deceasedDate);
                        loading = false;
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "ANNUAL")
            {
                int rowhandle = gridMain.FocusedRowHandle;
                int row = gridMain.GetDataSourceRowIndex(rowhandle);
                string cnum = dt.Rows[row]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(cnum))
                {
                    double annual = e.Value.ObjToDouble();
                    annual = G1.RoundValue(annual);
                    if (annual > 0D)
                    {
                        FixAnnualPremium(dt, annual);
                        G1.update_db_table("payers", "record", workPayerRecord1, new string[] {"annualPremium", annual.ToString() });
                    }
                }
            }
        }
        /***********************************************************************************************/
        //void sform_Done()
        //{
        //    dgv.Refresh();
        //    this.Refresh();
        //}
        /***********************************************************************************************/
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        { // Set as Lapsed // Policies have lapsedDate8 and icontracts have lapseDate8
        }
        /***********************************************************************************************/
        private void SetAllLapsedOrNot ( bool allLapsed, string contracts)
        {
            string cmd = "";
            string record = "";
            string contractNumber = "";
            DataTable dt = null;
            DateTime today = DateTime.Now;
            string lapseDate = today.ToString("yyyy-MM-dd");

            contracts = contracts.TrimEnd(',');
            string[] Lines = contracts.Split(',');
            for (int i = 0; i < Lines.Length; i++)
            {
                contractNumber = Lines[i].Trim();
                cmd = "Select * from `icontracts` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    if ( allLapsed )
                        G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "Y", "lapseDate8", lapseDate });
                    else
                        G1.update_db_table("icontracts", "record", record, new string[] { "lapsed", "", "lapseDate8", "" });
                }
                cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record = dt.Rows[0]["record"].ObjToString();
                    if ( allLapsed )
                        G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "Y" });
                    else
                        G1.update_db_table("icustomers", "record", record, new string[] { "lapsed", "" });
                }
            }
        }
        /***********************************************************************************************/
        private bool UpdatePolicyPremium(double premium)
        {
            string contract = workContractNumber;
            string cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return false;
            string record = dt.Rows[0]["record"].ObjToString();
            double balanceDue = premium;
            double nowDue = premium;
            G1.update_db_table("icontracts", "record", record, new string[] { "amtOfMonthlyPayt", premium.ToString(), "balanceDue", balanceDue.ToString(), "nowDue", nowDue.ToString() } );
            return true;
        }
        /***********************************************************************************************/
        public static double CalcMonthlyPremium ( string contractNumber, string payer, double amtOfMonthlyPayment )
        {
            if (!DailyHistory.isInsurance(contractNumber))
                return amtOfMonthlyPayment;
            if (amtOfMonthlyPayment < 500D)
                return amtOfMonthlyPayment;
            if (!String.IsNullOrWhiteSpace(payer))
                return CalcMonthlyPremium(payer);
            else if ( !String.IsNullOrWhiteSpace ( contractNumber))
            {
                string cmd = "Select * from `icustomers` where `contractNumber` = '" + contractNumber + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count <= 0)
                    return amtOfMonthlyPayment;
                payer = dt.Rows[0]["payer"].ObjToString();
                if (!String.IsNullOrWhiteSpace(payer))
                    amtOfMonthlyPayment = CalcMonthlyPremium(payer);
            }
            return amtOfMonthlyPayment;
        }
        /***********************************************************************************************/
        public static double CalcMonthlyPremium(string payer)
        {
            double monthlyPremium = 0D;
            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return monthlyPremium;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    continue;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                monthlyPremium += premium;
            }
            monthlyPremium = G1.RoundDown(monthlyPremium);
            return monthlyPremium;
        }
        /***********************************************************************************************/
        public static double CalcAnnualPremium(string payer)
        {
            double annualPremium = 0D;
            string cmd = "Select * from `policies` where `payer` = '" + payer + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return annualPremium;

            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate8 = DateTime.Now;
            double premium = 0D;
            double totalPremium = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                deceasedDate = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 1800)
                    continue;
                lapseDate8 = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (lapseDate8.Year > 1800)
                    continue;
                premium = dt.Rows[i]["premium"].ObjToDouble();
                premium = premium * 12D;
                premium = G1.RoundValue(premium);
                premium = premium * 0.95D;
                premium = G1.RoundDown(premium);
                totalPremium += premium;
            }
            annualPremium = totalPremium;
            annualPremium = G1.RoundDown(annualPremium);
            return annualPremium;
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawFooterCell(object sender, DevExpress.XtraGrid.Views.Grid.FooterCellCustomDrawEventArgs e)
        {
            if (loadAll)
                return;
            if (e.Column.FieldName != "premium")
                return;
            int dx = e.Bounds.Height;
            Brush brush = new System.Drawing.SolidBrush(this.gridMain.Appearance.BandPanelBackground.BackColor);
//            Brush brush = e.Cache.GetGradientBrush(e.Bounds, this.gridMain.Appearance.BandPanelBackground.BackColor, Color.FloralWhite, );
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
            double total = calculateTotalPremiums();
            string text = G1.ReformatMoney(total);
            e.Appearance.DrawString(e.Cache, text, r);
            //Prevent default drawing of the cell 
            e.Handled = true;
        }
        /****************************************************************************************/
        private double calculateTotalPremiums()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            double price = 0D;
            double total = 0D;
            string lapsed = "";
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (date.Year > 100)
                    continue;

                date = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (date.Year > 100)
                    continue;
                lapsed = dt.Rows[i]["lapsed"].ObjToString();
                if (lapsed.ToUpper() == "Y")
                    continue;

                price = dt.Rows[i]["premium"].ObjToDouble();
                total += price;
            }
            return total;
        }
        /***********************************************************************************************/
        private void chkFilterDeceased_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void chkFIlterLapsed_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if (date.Year < 100)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
            else if ( e.Column.FieldName.ToUpper() == "SSN" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string ssn = e.DisplayText.Trim();
                ssn = ssn.Replace("-", "");
                string ssno = ssn;
                if (ssn.Trim().Length >= 8)
                    try { ssno = "XXX-XX-" + ssn.Substring(5, 4); }
                    catch { }
                e.DisplayText = ssno;
            }
            else if (e.Column.FieldName.ToUpper() == "FULLNAME" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = e.ListSourceRowIndex;
                string fname = dt.Rows[row]["firstName"].ObjToString();
                string lname = dt.Rows[row]["lastName"].ObjToString();
                e.DisplayText = fname + " " + lname;
            }
            else if (e.Column.FieldName.ToUpper() == "POLICYFULLNAME" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = e.ListSourceRowIndex;
                string fname = dt.Rows[row]["policyFirstName"].ObjToString();
                string lname = dt.Rows[row]["policyLastName"].ObjToString();
                e.DisplayText = fname + " " + lname;
            }
        }
        /***********************************************************************************************/
        private object missing = Type.Missing;
        /***********************************************************************************************/
        private void ExportToExcel()
        {
            DialogResult result = MessageBox.Show("Do you REALLY want to SAVE this data to an Excel File?", "Save Excel Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            DateTime startTime = DateTime.Now;

            Excel.Application oXL = new Excel.Application();
            oXL.Visible = false;
            Excel.Workbook oWB = oXL.Workbooks.Add(missing);

            try
            {
                Excel.Worksheet oSheet = oWB.ActiveSheet as Excel.Worksheet;
                DataTable dt = (DataTable)dgv.DataSource;
                LoadUpExcelTab(dt, oSheet, "Policies", gridMain);

            }
            catch (Exception ex)
            {
            }

            try
            {
                using (SaveFileDialog ofdImage = new SaveFileDialog())
                {
                    ofdImage.Filter = "Excel files (*.xlsx)|*.xlsx";

                    if (ofdImage.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string fileName = ofdImage.FileName;

                        if (!String.IsNullOrWhiteSpace(fileName))
                        {
                            oWB.SaveAs(fileName, Excel.XlFileFormat.xlOpenXMLWorkbook,
                                missing, missing, missing, missing,
                                Excel.XlSaveAsAccessMode.xlNoChange,
                                missing, missing, missing, missing, missing);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            oWB.Close(missing, missing, missing);
            oXL.UserControl = true;
            oXL.Quit();

            DateTime stopTime = DateTime.Now;
            TimeSpan ts = stopTime - startTime;

            int hours = ts.Hours;
            int minutes = ts.Minutes;
            int seconds = ts.Seconds;

            MessageBox.Show("***INFO*** Total Processing Time = " + hours.ToString("D2") + ":" + minutes.ToString("D2") + ":" + seconds.ToString("D2") + "!!");
        }
        /***********************************************************************************************/
        private void LoadUpExcelTab(DataTable dt, Excel.Worksheet oSheet, string name, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            oSheet.Name = name;
            //txtSavingTab.Text = oSheet.Name;
            //txtSavingTab.Refresh();

            string caption = "";
            string data = "";
            int index = 0;

            DataTable sortDt = new DataTable();
            sortDt.Columns.Add("columns", Type.GetType("System.Int32"));
            sortDt.Columns.Add("col", Type.GetType("System.Int32"));
            for (int col = 0; col < gridMain.Columns.Count; col++)
            {
                if (!gridMain.Columns[col].Visible)
                    continue;
                index = gridMain.Columns[col].ColIndex.ObjToInt32();
                if (index < 0)
                    continue;
                DataRow dRow = sortDt.NewRow();
                dRow["columns"] = index;
                dRow["col"] = col;
                sortDt.Rows.Add(dRow);
            }
            DataView tempview = sortDt.DefaultView;
            tempview.Sort = "columns asc";
            sortDt = tempview.ToTable();

            int myCol = 0;

            for (int col = 0; col < sortDt.Rows.Count; col++)
            {
                try
                {
                    myCol = sortDt.Rows[col]["col"].ObjToInt32();
                    if (!gridMain.Columns[myCol].Visible)
                        continue;
                    caption = gridMain.Columns[myCol].Caption;
                    //txtSavingColumn.Text = caption;
                    //txtSavingColumn.Refresh();
                    name = gridMain.Columns[myCol].FieldName;
                    //                    oSheet.Cells[col + 1, 1] = caption;
                    oSheet.Cells[1, col + 1] = caption;
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        data = dt.Rows[j][name].ObjToString();
                        if (!String.IsNullOrWhiteSpace(data))
                            oSheet.Cells[col + 1][j + 2] = data;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void txtToPayer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                txtToPayer_Leave(sender, e);
        }
        /***********************************************************************************************/
        private void txtToPayer_Leave(object sender, EventArgs e)
        {
            string payer = txtToPayer.Text;
            if (String.IsNullOrWhiteSpace(payer))
                return;
            string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            if (String.IsNullOrWhiteSpace(payer))
                return;
            cmd += " WHERE p.`payer` = '" + payer + "' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if (G1.get_column_number(dt, "annual") < 0)
                dt.Columns.Add("annual", Type.GetType("System.Double"));
            dt.Columns.Add("num");
            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("select");
            dt.Columns.Add("fullname");
            dt.Columns.Add("policyfullname");
            dt.Columns.Add("myDeceasedDate");

            string contract = "";
            if (dt.Rows.Count > 0)
                contract = dt.Rows[0]["contractNumber"].ObjToString();
            loading = true;
            G1.NumberDataTable(dt);
            FixDates(dt, "birthDate", "bDate");
            FormatSSN(dt, "ssn", "ssno");
            SetupFullNames(dt);
            FixDeceasedDate(dt);
            LoadAnnual(contract, dt);
            workPayerRecord2 = GetPayerRecord(contract);
            dgv2.DataSource = dt;
            loading = false;
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawFooterCell(object sender, DevExpress.XtraGrid.Views.Grid.FooterCellCustomDrawEventArgs e)
        {
            if (loadAll)
                return;
            if (e.Column.FieldName != "premium")
                return;
            int dx = e.Bounds.Height;
            Brush brush = new System.Drawing.SolidBrush(this.gridMain2.Appearance.BandPanelBackground.BackColor);
            //            Brush brush = e.Cache.GetGradientBrush(e.Bounds, this.gridMain.Appearance.BandPanelBackground.BackColor, Color.FloralWhite, );
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
            double total = calculateTotalPremiums2();
            string text = G1.ReformatMoney(total);
            e.Appearance.DrawString(e.Cache, text, r);
            //Prevent default drawing of the cell 
            e.Handled = true;
        }
        /****************************************************************************************/
        private double calculateTotalPremiums2()
        {
            DataTable dt = (DataTable)dgv2.DataSource;
            double price = 0D;
            double total = 0D;
            string lapsed = "";
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (date.Year > 100)
                    continue;

                date = dt.Rows[i]["lapsedDate8"].ObjToDateTime();
                if (date.Year > 100)
                    continue;
                lapsed = dt.Rows[i]["lapsed"].ObjToString();
                if (lapsed.ToUpper() == "Y")
                    continue;

                price = dt.Rows[i]["premium"].ObjToDouble();
                total += price;
            }
            return total;
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
            else if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0)
            {
                if (e.RowHandle >= 0)
                {
                    if (!String.IsNullOrWhiteSpace(e.DisplayText))
                    {
                        DateTime date = e.DisplayText.ObjToDateTime();
                        if (date.Year < 1850)
                            e.DisplayText = "";
                        else
                            e.DisplayText = date.ToString("MM/dd/yyyy");
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;
            if (!chkFilterDeceased.Checked)
            {
                if (!chkFilterLapsed.Checked)
                    return;
            }
            ColumnView view = sender as ColumnView;
            if (chkFilterDeceased.Checked)
            {
                DateTime deceasedDate = dt.Rows[row]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 100)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            if (chkFilterLapsed.Checked)
            {
                DateTime lapsedDate = dt.Rows[row]["lapsedDate8"].ObjToDateTime();
                if (lapsedDate.Year > 100)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
                string lapsed = dt.Rows[row]["lapsed"].ObjToString();
                if (lapsed == "Y")
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomColumnDisplayText(object sender, CustomColumnDisplayTextEventArgs e)
        {
            ColumnView view = sender as ColumnView;
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToDateTime();
                    if (date.Year < 100)
                        e.DisplayText = "";
                    else
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "SSN" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string ssn = e.DisplayText.Trim();
                ssn = ssn.Replace("-", "");
                string ssno = ssn;
                if (ssn.Trim().Length >= 8)
                    try { ssno = "XXX-XX-" + ssn.Substring(5, 4); }
                    catch { }
                e.DisplayText = ssno;
            }
            else if (e.Column.FieldName.ToUpper() == "FULLNAME" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv2.DataSource;
                int row = e.ListSourceRowIndex;
                string fname = dt.Rows[row]["firstName"].ObjToString();
                string lname = dt.Rows[row]["lastName"].ObjToString();
                e.DisplayText = fname + " " + lname;
            }
            else if (e.Column.FieldName.ToUpper() == "POLICYFULLNAME" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                DataTable dt = (DataTable)dgv2.DataSource;
                int row = e.ListSourceRowIndex;
                string fname = dt.Rows[row]["policyFirstName"].ObjToString();
                string lname = dt.Rows[row]["policyLastName"].ObjToString();
                e.DisplayText = fname + " " + lname;
            }
        }
        /***********************************************************************************************/
        private void changePayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newPayer = txtToPayer.Text;
            if (String.IsNullOrWhiteSpace(newPayer))
            {
                DialogResult result = MessageBox.Show("New Payer Field is Empty!\nYou must enter a new payer!", "Empty New Payer Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string cmd = "Select * from `icustomers` where `payer` = '" + newPayer + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** New Payer Does Not Exist yet! You must first create a new Insurance Payer!");
                return;
            }
            string contractNumber = dx.Rows[0]["contractNumber"].ObjToString();

            int[] rows = gridMain.GetSelectedRows();

            DataTable dt = (DataTable)dgv.DataSource;
            string oldRule = DetermineRule(dt);

            DataTable dt2 = (DataTable)dgv2.DataSource;
            string newRule = DetermineRule(dt2);

            if (String.IsNullOrWhiteSpace(newRule))
                newRule = oldRule;
            else if (String.IsNullOrWhiteSpace(oldRule))
                oldRule = newRule;
            if ( newRule == "95%" || oldRule == "95%")
            {
                newRule = "95%";
                oldRule = "95%";
            }
            if ( String.IsNullOrWhiteSpace ( oldRule) && String.IsNullOrWhiteSpace ( newRule))
            {
                MessageBox.Show("Cannot Calculate Annual Premium because of not Rules!");
                return;
            }

            int row = 0;
            double premium = 0D;

            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                row = gridMain.GetDataSourceRowIndex(row);
                premium = dt.Rows[row]["premium"].ObjToDouble();
                string oldPolicyRecord = dt.Rows[row]["record"].ObjToString();
                G1.update_db_table("policies", "record", oldPolicyRecord, new string[] { "contractNumber", contractNumber, "payer", newPayer });
                dt.Rows[row]["payer"] = newPayer;


                G1.copy_dt_row(dt, row, dt2, dt2.Rows.Count);
            }
            string p = "";
            for ( int i=(dt.Rows.Count-1); i>=0; i--)
            {
                p = dt.Rows[i]["payer"].ObjToString();
                if (p == newPayer)
                    dt.Rows.RemoveAt(i);
            }

            double annualPremium = 0D;
            DetermineAnnualPremium(dt, oldRule, ref annualPremium, ref premium );
            G1.update_db_table("payers", "record", workPayerRecord1, new string[] { "annualPremium", annualPremium.ToString(), "amtOfMonthlyPayt", premium.ToString() });
            DetermineAnnualPremium(dt2, newRule, ref annualPremium, ref premium );
            G1.update_db_table("payers", "record", workPayerRecord2, new string[] { "annualPremium", annualPremium.ToString(), "amtOfMonthlyPayt", premium.ToString() });

            dgv.DataSource = dt;
            dgv2.DataSource = dt2;
            //txtToPayer_Leave(null, null);
            //LoadOldData();
        }
        /***********************************************************************************************/
        private void DetermineAnnualPremium ( DataTable dt, string rule, ref double annual, ref double totalPremium )
        {
            totalPremium = 0D;
            annual = 0D;
            double premium = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                premium = dt.Rows[i]["premium"].ObjToDouble();
                totalPremium += premium;
            }
            if (rule == "12")
                annual = totalPremium * 12D;
            else if (rule == "11")
                annual = totalPremium * 11D;
            else if (rule == "95%")
                annual = totalPremium * 0.95D * 12D;
            string contractNumber = "";
            string oldContract = "";
            string record = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["annualPremium"] = annual;
                dt.Rows[i]["annual"] = annual;
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                if ( contractNumber != oldContract )
                {
                    oldContract = contractNumber;
                    record = dt.Rows[i]["record2"].ObjToString();
                    G1.update_db_table("icontracts", "record", record, new string[] { "annualPremium", annual.ToString(), "amtOfMonthlyPayt", totalPremium.ToString() });
                }
            }
        }
        /***********************************************************************************************/
        private string DetermineRule ( DataTable dt)
        {
            string rule = "";
            if (dt == null)
                return rule;
            if (dt.Rows.Count <= 0)
                return rule;
            double annual = 0D;
            double totalPremium = 0D;
            double premium = 0D;
            if (dt.Rows.Count > 0)
                annual = dt.Rows[0]["annual"].ObjToDouble();
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                premium = dt.Rows[i]["premium"].ObjToDouble();
                totalPremium += premium;
            }
            double percent = (totalPremium * 12D) * .95D;
            double eleven = totalPremium * 11D;
            double twelve = totalPremium * 12D;

            if (percent >= (annual - 0.02D) && percent <= (annual + 0.02D))
                rule = "95%";
            if (annual == eleven)
                rule = "11";
            if (String.IsNullOrWhiteSpace(rule))
                rule = "12";
            return rule;
        }
        /***********************************************************************************************/
        private void moveToOldPayerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string newPayer = txtFromPayer.Text;
            if (String.IsNullOrWhiteSpace(newPayer))
                return;

            string cmd = "Select * from `icustomers` where `payer` = '" + newPayer + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** New Payer Does Not Exist yet! You must first create a new Insurance Payer!");
                return;
            }
            string contractNumber = dx.Rows[0]["contractNumber"].ObjToString();

            int[] rows = gridMain2.GetSelectedRows();

            DataTable dt = (DataTable)dgv.DataSource;
            string oldRule = DetermineRule(dt);

            DataTable dt2 = (DataTable)dgv2.DataSource;
            string newRule = DetermineRule(dt2);

            if (String.IsNullOrWhiteSpace(newRule))
                newRule = oldRule;
            else if (String.IsNullOrWhiteSpace(oldRule))
                oldRule = newRule;
            if (newRule == "95%" || oldRule == "95%")
            {
                newRule = "95%";
                oldRule = "95%";
            }
            if (String.IsNullOrWhiteSpace(oldRule) && String.IsNullOrWhiteSpace(newRule))
            {
                MessageBox.Show("Cannot Calculate Annual Premium because of not Rules!");
                return;
            }
            int row = 0;
            double premium = 0D;

            for (int i = 0; i < rows.Length; i++)
            {
                row = rows[i];
                row = gridMain2.GetDataSourceRowIndex(row);
                premium = dt2.Rows[row]["premium"].ObjToDouble();
                string oldPolicyRecord = dt2.Rows[row]["record"].ObjToString();
                G1.update_db_table("policies", "record", oldPolicyRecord, new string[] { "contractNumber", contractNumber, "payer", newPayer });
                dt2.Rows[row]["payer"] = newPayer;

                G1.copy_dt_row(dt2, row, dt, dt.Rows.Count);
            }

            string p = "";
            for (int i = (dt2.Rows.Count - 1); i >= 0; i--)
            {
                p = dt2.Rows[i]["payer"].ObjToString();
                if (p == newPayer)
                    dt2.Rows.RemoveAt(i);
            }

            premium = 0D;
            double annualPremium = 0D;

            DetermineAnnualPremium(dt, oldRule, ref annualPremium, ref premium );
            G1.update_db_table("payers", "record", workPayerRecord1, new string[] { "annualPremium", annualPremium.ToString(), "amtOfMonthlyPayt", premium.ToString() });
            DetermineAnnualPremium(dt2, newRule, ref annualPremium, ref premium );
            G1.update_db_table("payers", "record", workPayerRecord2, new string[] { "annualPremium", annualPremium.ToString(), "amtOfMonthlyPayt", premium.ToString() });

            dgv.DataSource = dt;
            dgv2.DataSource = dt2;

            //LoadOldData();
            //txtToPayer_Leave(null, null);
        }
        /***********************************************************************************************/
        private void LoadOldData()
        {
            string payer = txtFromPayer.Text;
            if (String.IsNullOrWhiteSpace(payer))
                return;
            string cmd = "Select * from `policies` p JOIN `icustomers` d ON p.`contractNumber` = d.`contractNumber` JOIN `icontracts` x ON p.`contractNumber` = x.`contractNumber` ";
            if (String.IsNullOrWhiteSpace(payer))
                return;
            cmd += " WHERE p.`payer` = '" + payer + "' ";
            cmd += ";";

            DataTable dt = G1.get_db_data(cmd);
            if (G1.get_column_number(dt, "annual") < 0)
                dt.Columns.Add("annual", Type.GetType("System.Double"));
            dt.Columns.Add("num");
            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("select");
            dt.Columns.Add("fullname");
            dt.Columns.Add("policyfullname");
            dt.Columns.Add("myDeceasedDate");

            G1.NumberDataTable(dt);
            FixDates(dt, "birthDate", "bDate");
            FormatSSN(dt, "ssn", "ssno");
            SetupFullNames(dt);
            FixDeceasedDate(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain2.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv2.DataSource;
            if (e.Column.FieldName.ToUpper() == "DUEDATE")
            {
                int rowhandle = gridMain2.FocusedRowHandle;
                int row = gridMain2.GetDataSourceRowIndex(rowhandle);
                string cnum = dt.Rows[row]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(cnum))
                {
                    string date = e.Value.ObjToString();
                    date = dr["dueDate"].ObjToString();
                    if (G1.validate_date(date))
                    {
                        MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                        string dueDate = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        string record = dt.Rows[row]["record1"].ObjToString();
                        G1.update_db_table("icontracts", "record", record, new string[] { "dueDate8", dueDate });
                        G1.update_db_table("payers", "record", workPayerRecord2, new string[] { "dueDate8", dueDate });
                        BandedGridView view = sender as BandedGridView;
                        loading = true;
                        view.SetRowCellValue(e.RowHandle, view.Columns["dueDate"], dueDate);
                        dr["dueDate"] = myDate;
                        loading = false;
                        gridMain2.RefreshData();
                        this.Refresh();
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "DDATE")
            {
                int rowhandle = gridMain2.FocusedRowHandle;
                int row = gridMain2.GetDataSourceRowIndex(rowhandle);
                string cnum = dt.Rows[row]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(cnum))
                {
                    string date = e.Value.ObjToString();
                    date = dr["ddate"].ObjToString();
                    if (G1.validate_date(date))
                    {
                        MySqlDateTime myDate = (MySqlDateTime)G1.DTtoMySQLDT(date);
                        string deceasedDate = myDate.Month.ToString("D2") + "/" + myDate.Day.ToString("D2") + "/" + myDate.Year.ToString("D4");
                        string record = dt.Rows[row]["record"].ObjToString();
                        G1.update_db_table("policies", "record", record, new string[] { "deceasedDate", deceasedDate });
                        BandedGridView view = sender as BandedGridView;
                        loading = true;
                        //                        view.SetRowCellValue(e.RowHandle, view.Columns["deceasedDate"], deceasedDate);
                        loading = false;
                    }
                }
            }
            else if (e.Column.FieldName.ToUpper() == "ANNUAL")
            {
                int rowhandle = gridMain2.FocusedRowHandle;
                int row = gridMain2.GetDataSourceRowIndex(rowhandle);
                string cnum = dt.Rows[row]["contractNumber"].ObjToString();
                if (!String.IsNullOrWhiteSpace(cnum))
                {
                    double annual = e.Value.ObjToDouble();
                    annual = G1.RoundValue(annual);
                    if (annual > 0D)
                    {
                        FixAnnualPremium(dt, annual);
                        G1.update_db_table("payers", "record", workPayerRecord2, new string[] { "annualPremium", annual.ToString() });
                    }
                }
            }
        }
        /***********************************************************************************************/
    }
}
