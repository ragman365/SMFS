using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;

using GeneralLib;
using DevExpress.XtraReports.UI;
using DevExpress.XtraReports.ReportGeneration;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Goals : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool modified = false;
        private bool loading = true;
        public Goals()
        {
            InitializeComponent();
//            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void Goals_Load(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            int days = DateTime.DaysInMonth(now.Year, now.Month);
            DateTime start = new DateTime(now.Year, now.Month, days);
            this.dateTimePicker2.Value = start;
            int year = start.AddYears(-1).Year;
            DateTime end = new DateTime(year, start.Month, 1);
            this.dateTimePicker1.Value = end;
            
            loading = false;
            modified = false;
            btnSave.Hide();

            string cmd = "Select * from `goals` GROUP by `agentCode` ORDER by `effectiveDate`;";
            DataTable dt = G1.get_db_data(cmd);

            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("edate");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            date = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(date);

            string effectiveDate = " where `effectiveDate` >= '" + date1 + "' and `effectiveDate` <= '" + date2 + "' ";

            string cmd = "Select * from `goals` g LEFT JOIN `agents` a ON g.`agentCode` = a.`agentCode` ";
            if ( chkEffDate.Checked )
                cmd += effectiveDate;
            cmd += " ORDER BY g.`agentCode`, `effectiveDate`;";

            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("edate");
            string str = "";
            long days = 0;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["effectiveDate"].ObjToString();
                days = G1.date_to_days(str);
                str = G1.days_to_date(days);
                dt.Rows[i]["edate"] = str;
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("recapAmount");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "${0:0,0.00}";
        }
        /****************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["agentCode"] = "ZZZZ";
            dRow["mod"] = "Y";
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            modified = true;
            btnSave.Show();
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string status = "";
            string type = "";
            string agentCode = "";
            string customGoals = "";
            string mod = "";
            string str = "";
            string edate = "";
            double percent;
            double goal;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                edate = G1.GetSQLDate(dt, i, "effectiveDate");
//                edate = dt.Rows[i]["edate"].ObjToString();
//                dt.Rows[i]["effectiveDate"] = edate; // Just for Testing. Needs Work
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod.ToUpper() != "Y")
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("goals", "type", "-1");
                if (G1.BadRecord("goals", record))
                    break;
                str = dt.Rows[i]["effectiveDate"].ObjToString();
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                status = dt.Rows[i]["status"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                customGoals = dt.Rows[i]["formula"].ObjToString();
                percent = dt.Rows[i]["percent"].ObjToDouble();
                percent = G1.RoundValue(percent);
                goal = dt.Rows[i]["goal"].ObjToDouble();
                goal = G1.RoundValue(goal);

                G1.update_db_table("goals", "record", record, new string[] { "agentCode", agentCode, "status", status, "formula", customGoals, "type", type, "effectiveDate", edate, "percent", percent.ToString(), "goal", goal.ToString() });
                dt.Rows[i]["mod"] = "";
            }
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string status = dr["status"].ObjToString();
            if (String.IsNullOrWhiteSpace(status))
                dr["status"] = "Inactive";
            else if (status.ToUpper() == "INACTIVE")
                dr["status"] = "";
            modified = true;
            btnSave.Show();
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            dr["mod"] = "Y";
            modified = true;
            btnSave.Show();
            if ( e.Column.FieldName.ToUpper() == "AGENTCODE")
            {
                string code = dr["agentCode"].ObjToString();
                DataTable dt = G1.get_db_data("Select * from `agents` where `agentCode` = '" + code + "';");
                if ( dt.Rows.Count > 0 )
                {
                    string fname = dt.Rows[0]["firstName"].ObjToString();
                    string lname = dt.Rows[0]["lastName"].ObjToString();
                    dr["firstName"] = fname;
                    dr["lastName"] = lname;
                }
            }
            else if ( e.Column.FieldName.ToUpper() == "EDATE")
            {
                string str = dr["edate"].ObjToString();
                dr["effectiveDate"] = G1.DTtoMySQLDT(str);
                long days = G1.date_to_days(str);
                str = G1.days_to_date(days);
                dr["edate"] = str;

            }
        }
        /****************************************************************************************/
        private void Agents_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nInformation has been modified!\nWould you like to save your changes?", "Add/Edit Agents Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            btnSave_Click(null, null);
        }
        /****************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            loading = true;
            modified = false;
            LoadData();
            loading = false;
            modified = false;
            btnSave.Hide();
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
        /****************************************************************************************/
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
        /****************************************************************************************/
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
        //private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        //{
        //    Printer.setupPrinterQuads(e, 2, 3);
        //    Font font = new Font("Ariel", 16);
        //    Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

        //    Printer.SetQuadSize(12, 12);
        //    //Printer.DrawQuadBorder(1, 1, 12, 6, BorderSide.All, 1, Color.Black);
        //    //Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

        //    font = new Font("Ariel", 8);
        //    Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
        //    Printer.DrawGridPage(11, 1, 2, 3, Color.Black, BorderSide.None, font);

        //    Printer.DrawQuad(1, 3, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

        //    font = new Font("Ariel", 10, FontStyle.Bold);
        //    Printer.DrawQuad(6, 3, 2, 3, "Agents Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

        //    font = new Font("Ariel", 7, FontStyle.Regular);
        //    //Printer.DrawQuad(1, 5, 4, 1, "Contract :" + workContract, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(3, 5, 3, 1, "Name :" + workName, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    Printer.DrawQuadBorder(1, 1, 12, 5, BorderSide.All, 1, Color.Black);
        //    Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

        //    //Printer.DrawQuad(1, 6, 3, 1, labelSerTot.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(1, 7, 3, 1, labelMerTot.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(1, 8, 3, 1, labDownPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(1, 9, 3, 1, labRemainingBalance.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(1, 10, 3, 1, lblDueDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(1, 11, 3, 1, lblAPR.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

        //    //Printer.DrawQuad(3, 6, 3, 1, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(3, 7, 3, 1, lblIssueDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(3, 8, 3, 1, lblNumPayments.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
        //    //Printer.DrawQuad(3, 9, 3, 1, lblTotalPaid.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


        //    Printer.SetQuadSize(12, 12);
        //    Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
        //    Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);



        //    //Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
        //    ////            Printer.DrawQuadTicks();
        //}
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

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 8, 2, 4, "Agent Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //string workDate = cmbYear.Text;
            //Printer.SetQuadSize(24, 12);
            //font = new Font("Ariel", 9, FontStyle.Bold);
            //Printer.DrawQuad(20, 8, 5, 4, "Report Year:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);



            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void btnTest_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `goals` WHERE `status` = 'active' ORDER by `effectiveDate`;";
            DataTable dt = G1.get_db_data(cmd);
        }
        /****************************************************************************************/
    }
}