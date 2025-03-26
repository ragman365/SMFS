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
using DevExpress.XtraEditors.Repository;
//using DevExpress.XtraEditors.Controls;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class ActiveContracts : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = false;
        /****************************************************************************************/
        public ActiveContracts()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void ActiveContracts_Load(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            now = now.AddMonths(-1);
            now = new DateTime(now.Year, 1, 1);
            this.dateTimePicker1.Value = now;
            int days = DateTime.DaysInMonth(now.Year, 12 );
            DateTime stop = new DateTime(now.Year, 12, days);
            this.dateTimePicker2.Value = stop;

            SetupTotalsSummary();

            loadLocatons();
        }
        /***********************************************************************************************/
        private void loadLocatons()
        {
            string cmd = "Select * from `funeralhomes` order by `keycode`;";
            DataTable locDt = G1.get_db_data(cmd);
            //chkComboLocNames.Properties.DataSource = locDt;
            chkComboLocation.Properties.DataSource = locDt;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("contractValue", null);
            AddSummaryColumn("numContracts", null, "{0:0}");

        }
        ///****************************************************************************************/
        //private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.Grid.GridView gMain = null)
        //{
        //    if (gMain == null)
        //        gMain = gridMain;
        //    //            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
        //    gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
        //    //            gMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
        //    gMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
        //}
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
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        { // Spy Glass
            G1.SpyGlass(gridMain);
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
            }
        }
        /****************************************************************************************/
        private void EditTable_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
        /****************************************************************************************/
        private string customersFile = "customers";
        private string contractsFile = "contracts";
        private DataTable originalDt = null;
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            G1.CleanupDataGrid(ref dgv);
            GC.Collect();
            dgv.Show();
            loading = true;
            gridMain.Columns["cbal"].Visible = false;
            gridMain.Columns["newduedate"].Visible = false;
            gridMain.Columns["days"].Visible = false;
            gridMain.Columns["creditBalance"].Visible = false;
            gridMain.Columns["totalInterest"].Visible = false;
            gridMain.Columns["cint"].Visible = false;
            gridMain.Columns["lapseDate8"].Visible = false;
            gridMain.Columns["ServiceId1"].Visible = false;
            gridMain.Columns["deceasedDate"].Visible = false;
            gridMain.Columns["DDATE"].Visible = false;
            gridMain.Columns["payer"].Visible = false;
            this.Cursor = Cursors.WaitCursor;


            customersFile = "customers";
            contractsFile = "contracts";

            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            string cmd = "Select * from `" + customersFile + "` p JOIN `" + contractsFile + "` d ON p.`contractNumber` = d.`contractNumber` WHERE ";
            cmd += " `issueDate8` >= '" + date1 + "' AND `issueDate8` <= '" + date2 + "' ";

            cmd += " AND p.`coverageType` <> 'ZZ' ";
            cmd += " AND p.`lapsed` <> 'Y' AND d.`lapsed` <> 'Y' ";
            cmd += " AND d.`deceasedDate` < '19101231' ";
            cmd += " AND p.`contractNumber` NOT LIKE 'RF%' ";


            cmd += ";";
            cmd = cmd.Replace("WHERE ;", ";");

            DataTable dt = G1.get_db_data(cmd);

            //            DataRow[] dRow = dt.Select("contractNumber='P16050UI'");
            //            int len = dRow.Length;
            dt.Columns.Add("num");
            dt.Columns.Add("bDate");
            dt.Columns.Add("ssno");
            dt.Columns.Add("agreement");
            dt.Columns.Add("select");
            dt.Columns.Add("fullname");
            dt.Columns.Add("paid", Type.GetType("System.Double"));
            dt.Columns.Add("purchase", Type.GetType("System.Double"));
            dt.Columns.Add("dueDate");
            dt.Columns.Add("DOLP");
            dt.Columns.Add("cbal", Type.GetType("System.Double"));
            dt.Columns.Add("newduedate");
            dt.Columns.Add("days", Type.GetType("System.Int32"));
            dt.Columns.Add("cint", Type.GetType("System.Double"));
            dt.Columns.Add("financed", Type.GetType("System.Double"));
            dt.Columns.Add("trust85", Type.GetType("System.Double"));
            dt.Columns.Add("contractValue", Type.GetType("System.Double"));
            dt.Columns.Add("percentPaid", Type.GetType("System.Double"));
            dt.Columns.Add("idate");
            dt.Columns.Add("realDOLP");
            dt.Columns.Add("DDATE");
            dt.Columns.Add("loc");
            dt.Columns.Add("age", Type.GetType("System.Int32"));
            dt.Columns.Add("numContracts", Type.GetType("System.Int32"));

            DateTime ddate = DateTime.Now;
            gridMain.Columns["lapseDate8"].Visible = false;
            gridMain.Columns["DOLP"].Visible = false;

            //if (chkIncludePaid.Checked)
            //{
            //    gridMain.Columns["paid"].Visible = true;
            //    gridMain.Columns["purchase"].Visible = true;
            //}
            //else
            //{
            //    gridMain.Columns["paid"].Visible = false;
            //    gridMain.Columns["purchase"].Visible = false;
            //}

            bool showOrNot = true;

            gridMain.Columns["trust85"].Visible = showOrNot;
            gridMain.Columns["cbal"].Visible = showOrNot;
            gridMain.Columns["purchase"].Visible = showOrNot;
            gridMain.Columns["paid"].Visible = showOrNot;
            gridMain.Columns["financed"].Visible = showOrNot;
            gridMain.Columns["newduedate"].Visible = showOrNot;
            gridMain.Columns["days"].Visible = showOrNot;
            gridMain.Columns["apr"].Visible = showOrNot;
            gridMain.Columns["percentPaid"].Visible = showOrNot;
            gridMain.Columns["contractValue"].Visible = showOrNot;
            gridMain.Columns["creditBalance"].Visible = showOrNot;
            gridMain.Columns["extraItemAmtMI1"].Visible = showOrNot;
            gridMain.Columns["extraItemAmtMI2"].Visible = showOrNot;
            gridMain.Columns["trustRemoved"].Visible = showOrNot;

            gridMain.Columns["cbal"].Visible = false;
            gridMain.Columns["newduedate"].Visible = false;
            gridMain.Columns["days"].Visible = false;
            gridMain.Columns["creditBalance"].Visible = false;
            gridMain.Columns["totalInterest"].Visible = false;
            gridMain.Columns["cint"].Visible = false;

            string contractNumber = "";
            string loc = "";
            string trust = "";
            string contract = "";
            DateTime bDate = DateTime.Now;
            int age = 0;
            string city = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                dt.Rows[i]["loc"] = loc;

                bDate = dt.Rows[i]["birthDate"].ObjToDateTime();
                if ( bDate.Year > 100 )
                {
                    age = G1.GetAge(bDate, DateTime.Now);
                    dt.Rows[i]["age"] = age;
                }
                dt.Rows[i]["numContracts"] = 1;

                city = dt.Rows[i]["city"].ObjToString();
                city = G1.force_lower_line(city);
                dt.Rows[i]["city"] = city;
            }

            NewByDetail.RemoveCemeteries(dt);

            G1.NumberDataTable(dt);
            //FixDates(dt, "birthDate", "bDate");
            FormatSSN(dt, "ssn", "ssno");
            //SetupFullNames(dt);

            gridMain.Columns["trustRemoved"].Visible = false;

            GetFinancedAmount(dt);

            FixDeceasedDate(dt);
            dgv.DataSource = dt;
            gridBand2.Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridMain.Columns["num"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            gridMain.Columns["contractNumber"].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;

            SetFieldUserFormat();

            originalDt = dt;

            dgv.DataSource = dt;
            dgv.Show();

            this.Cursor = Cursors.Default;
            loading = false;

            string what = cmbGroupBy.Text.Trim().ToUpper();
            if (what != "NONE")
                cmbGroupBy_SelectedIndexChanged(null, null);
        }
        /****************************************************************************************/
        private void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null)
        {
            if (gMain == null)
                gMain = gridMain;
            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                gMain.Columns[i].Visible = false;
            }
        }
        /****************************************************************************************/
        private void SetFieldUserFormat()
        {
            ClearAllPositions(gridMain);

            G1.AddNewColumn(gridMain, "address1", "Address1", "", FormatType.None, 100, true);
            G1.SetColumnWidth(gridMain, "address1", 100);
            G1.AddNewColumn(gridMain, "address2", "Address2", "", FormatType.None, 50, true);
            G1.SetColumnWidth(gridMain, "address2", 50);
            G1.AddNewColumn(gridMain, "city", "City", "", FormatType.None, 100, true);
            G1.SetColumnWidth(gridMain, "city", 100);
            G1.AddNewColumn(gridMain, "state", "State", "", FormatType.None, 100, true);
            G1.SetColumnWidth(gridMain, "state", 100);
            G1.AddNewColumn(gridMain, "zip1", "Zip", "", FormatType.None, 50, true);
            G1.SetColumnWidth(gridMain, "zip1", 50);

            int i = 1;
            G1.SetColumnPosition(gridMain, "num", ++i);
            G1.SetColumnPosition(gridMain, "loc", ++i);
            G1.SetColumnPosition(gridMain, "contractNumber", ++i);
            //G1.SetColumnPosition(gridMain, "ServiceId1", ++i);
            //G1.SetColumnPosition(gridMain, "lapsed", ++i);
            //G1.SetColumnPosition(gridMain, "fullname", ++i);
            G1.SetColumnPosition(gridMain, "lastName", ++i);
            G1.SetColumnPosition(gridMain, "firstName", ++i);
            G1.SetColumnPosition(gridMain, "ssno", ++i);
            G1.SetColumnPosition(gridMain, "birthDate", ++i);
            G1.SetColumnPosition(gridMain, "age", ++i);
            G1.SetColumnPosition(gridMain, "issueDate8", ++i);
            G1.SetColumnPosition(gridMain, "dueDate8", ++i);
            G1.SetColumnPosition(gridMain, "numContracts", ++i);
            G1.SetColumnPosition(gridMain, "contractValue", ++i);
            //G1.SetColumnPosition(gridMain, "deceasedDate", ++i);
            G1.SetColumnPosition(gridMain, "address1", ++i);
            G1.SetColumnPosition(gridMain, "address2", ++i);
            G1.SetColumnPosition(gridMain, "city", ++i);
            G1.SetColumnPosition(gridMain, "state", ++i);
            G1.SetColumnPosition(gridMain, "zip1", ++i);

            if (G1.isField())
                gridMain.OptionsMenu.EnableColumnMenu = false;
            else
                gridMain.OptionsMenu.EnableColumnMenu = true;
            if (!LoginForm.administrator)
                dgv.ContextMenuStrip = null;
        }
        /***********************************************************************************************/
        private void GetFinancedAmount(DataTable dt)
        {
            double financed = 0D;
            double contractValue = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                financed = DailyHistory.GetFinanceValue(dt.Rows[i]);
                dt.Rows[i]["financed"] = financed;
                contractValue = DailyHistory.GetContractValue(dt.Rows[i]);
                dt.Rows[i]["contractValue"] = contractValue;
            }
        }
        /***********************************************************************************************/
        private void FixDeceasedDate(DataTable dt)
        {
            string date1 = "";
            string date2 = "";
            if (G1.get_column_number(dt, "DDATE") < 0)
                dt.Columns.Add("DDATE");
            if (G1.get_column_number(dt, "deceasedDate") < 0)
                return;
            if (G1.get_column_number(dt, "deceasedDate1") < 0)
                return;
            DateTime date = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date1 = dt.Rows[i]["deceasedDate"].ObjToString();
                date = date1.ObjToDateTime();
                if (date.Year > 100)
                    dt.Rows[i]["DDATE"] = date.ToString("yyyy-MM-dd");
                //if (date1.IndexOf("0000") >= 0)
                //{
                //    date2 = dt.Rows[i]["deceasedDate1"].ObjToString();
                //    if (date2.IndexOf("0000") < 0)
                //        dt.Rows[i]["deceasedDate"] = dt.Rows[i]["deceasedDate1"];
                //}
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
        /****************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `loc` IN (" + procLoc + ") " : "";
        }
        /****************************************************************************************/
        private void cmbGroupBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            gridMain.Columns["age"].GroupIndex = -1;
            gridMain.Columns["city"].GroupIndex = -1;
            gridMain.Columns["loc"].GroupIndex = -1;
            gridMain.Columns["zip1"].GroupIndex = -1;

            string what = cmbGroupBy.Text.Trim().ToUpper();
            if ( what == "LOCATION")
                gridMain.Columns["loc"].GroupIndex = 0;
            else if (what == "CITY")
                gridMain.Columns["city"].GroupIndex = 0;
            else if (what == "AGE")
                gridMain.Columns["age"].GroupIndex = 0;
            else if (what == "ZIP")
                gridMain.Columns["zip1"].GroupIndex = 0;

            if (what != "NONE")
                gridMain.ExpandAllGroups();

            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
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
        private int printCount = 0;
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

            //Printer.setupPrinterMargins(50, 100, 110, 50);

            Printer.setupPrinterMargins(30, 30, 90, 50);


            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
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

            //Printer.setupPrinterMargins(50, 100, 110, 50);
            Printer.setupPrinterMargins(30, 30, 90, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printCount = 0;

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

            font = new Font("Ariel", 12, FontStyle.Regular);
            string title = this.Text;
            int startX = 6;
            Printer.DrawQuad(startX, 8, 9, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


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
        /****************************************************************************************/
        private void editLettersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Arrangements lettersForm = new Arrangements(false, true);
            lettersForm.Show();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void selectLetterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            using (Arrangements LettersForm = new Arrangements(true, true ))
            {
                LettersForm.Text = "List of Letters";
                LettersForm.ListDone += LettersForm_ListDone;
                LettersForm.ShowDialog();
            }
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DevExpress.XtraRichEdit.RichEditControl rtbNew = null;
        private DevExpress.XtraRichEdit.RichEditControl rtb = null;
        private DevExpress.XtraRichEdit.RichEditControl rtbOriginal = null;
        private DevExpress.XtraRichEdit.RichEditControl rtbPage = null;
        /****************************************************************************************/
        private void LettersForm_ListDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            string record = s;
            string cmd = "Select * from `arrangementforms` where record = '" + record + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            Rectangle rect = this.panelTop.Bounds;
            if (rect.Height <= 60)
            {
                int height = rect.Height + 38;
                this.panelTop.SetBounds(rect.Left, rect.Top, rect.Width, height);
            }

            txtActiveLetter.Text = dt.Rows[0]["formName"].ObjToString();

            string str = G1.get_db_blob("arrangementforms", record, "image");
            byte[] b = Encoding.UTF8.GetBytes(str);
            byte[] bytes = Encoding.ASCII.GetBytes(str);

            MemoryStream stream = new MemoryStream(bytes);

            if (rtbOriginal != null)
            {
                rtbOriginal.Document.Delete(rtbOriginal.Document.Range);
                rtbOriginal.Dispose();
                rtbOriginal = null;
            }


            rtbOriginal = new DevExpress.XtraRichEdit.RichEditControl();

            rtbOriginal.Document.Delete(rtbOriginal.Document.Range);

            rtbOriginal.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);


            this.Refresh();
        }
        /****************************************************************************************/
        private void btnGenerateLetters_Click(object sender, EventArgs e)
        {
            if (rtbOriginal == null)
                return;
            if (rtbOriginal.Document == null)
                return;

            if (rtb != null)
                rtb.Document.Delete(rtb.Document.Range);

            DataTable dt = (DataTable)dgv.DataSource;

            int[] rows = gridMain.GetSelectedRows();
            int lastRow = dt.Rows.Count;
            if (rows.Length > 0)
                lastRow = rows.Length;

            string filename = "";
            string contractNumber = "";
            int row = 0;
            string str = "";
            string text = "";
            DataRow dr = null;
            int count = 0;

            rtbPage = new DevExpress.XtraRichEdit.RichEditControl();
            rtbPage.Document.AppendRtfText(@"{\rtf1 \par \page}");

            string pageText = @"{\rtf1\deff0{\fonttbl{\f0 Calibri;}{\f1 Courier New;}}{\colortbl ;\red0\green0\blue255 ;}{\*\defchp \fs22}{\stylesheet {\ql\fs22 Normal;}{\*\cs1\fs22 Default Paragraph Font;}{\*\cs2\sbasedon1\fs22 Line Number;}{\*\cs3\ul\fs22\cf1 Hyperlink;}{\*\ts4\tsrowd\fs22\ql\tscellpaddfl3\tscellpaddl108\tscellpaddfb3\tscellpaddfr3\tscellpaddr108\tscellpaddft3\tsvertalt\cltxlrtb Normal Table;}{\*\ts5\tsrowd\sbasedon4\fs22\ql\trbrdrt\brdrs\brdrw10\trbrdrl\brdrs\brdrw10\trbrdrb\brdrs\brdrw10\trbrdrr\brdrs\brdrw10\trbrdrh\brdrs\brdrw10\trbrdrv\brdrs\brdrw10\tscellpaddfl3\tscellpaddl108\tscellpaddfr3\tscellpaddr108\tsvertalt\cltxlrtb Table Simple 1;}}{\*\listoverridetable}{\info{\creatim\yr2016\mo8\dy25\hr8\min34}{\version1}}\nouicompat\splytwnine\htmautsp\sectd\pard\plain\ql{\langnp1033\langfenp1033\noproof\f1\fs20\cf1 \\page}\fs22\par}";


            string junk = rtbPage.Document.RtfText;

            rtbNew = new DevExpress.XtraRichEdit.RichEditControl();

            this.Cursor = Cursors.WaitCursor;

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();

                row = rows[i];
                row = gridMain.GetDataSourceRowIndex(row);

                dr = dt.Rows[row];

                contractNumber = dr["contractNumber"].ObjToString();

                if (rtb == null)
                    rtb = new DevExpress.XtraRichEdit.RichEditControl();

                text = this.rtbOriginal.RtfText;

                if (rtbNew != null)
                    rtbNew.Document.Delete(rtbNew.Document.Range);
                rtbNew.RtfText = text;

                string formName = txtActiveLetter.Text.Trim();

                //string tt = rtbx.Document.Text;
                DataTable dx = RTF_Stuff.ExtractFields ( text );

                RTF_Stuff.LoadFields ( contractNumber, dx, rtbNew, "Letter", formName, false );

                RTF_Stuff.LoadDbFields(contractNumber, "Letter", dx);

                ArrangementForms.PushFieldsToForm ( contractNumber, dx, rtbNew, false );


                //text = RTF_Stuff.ReplaceField(text, "[*Heading*]", "This is a Heading for Contract " + contractNumber + "\\line Address\\line City, State, zip\\line");

                if (count >= 1)
                    rtb.Document.AppendRtfText(rtbPage.Document.RtfText);

                text = rtbNew.RtfText;

                rtb.Document.AppendRtfText(text);
                count++;

            }

            string userprofile = Environment.GetEnvironmentVariable("USERPROFILE").ToUpper();
            //G1.GrantAccess(userprofile);
            G1.GrantFileAccess(userprofile);
            userprofile += @"\pdfFiles";
            if (!Directory.Exists(userprofile))
            {
                //G1.GrantAccess(userprofile);
                G1.GrantFileAccess(userprofile);
                Directory.CreateDirectory(userprofile);
            }
            userprofile += @"\lettersx.pdf";

            filename = userprofile;

            //filename = "c:/users/robby/downloads/lettersx.pdf";
            //G1.GrantAccess(filename);
            G1.GrantFileAccess(filename);

            if (File.Exists(filename))
            {
                File.SetAttributes(filename, FileAttributes.Normal);
                File.Delete(filename);
            }

            try
            {
                G1.GrantFileAccess(filename);
                rtb.ExportToPdf(filename);
            }
            catch (Exception ex)
            {
            }

            this.Cursor = Cursors.Default;

            ViewPDF pdfForm = new ViewPDF("Test", filename);
            pdfForm.WindowState = FormWindowState.Maximized;
            pdfForm.Show();
        }
        /****************************************************************************************/
    }
}