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
    public partial class AgentsX : DevExpress.XtraEditors.XtraForm
    {
        /****************************************************************************************/
        private bool modified = false;
        private bool loading = true;
        public AgentsX()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void agents_Load(object sender, EventArgs e)
        {
            SetPanelsBottom(true);
            if ( !LoginForm.administrator )
            {
                lblCopyTo.Hide();
                txtCopyYear.Hide();
                btnCopyYear.Hide();
                btnRemoveYear.Hide();
            }
            LoadYearCombo();
            LoadData();
            loading = false;
            modified = false;
            gridMain.FocusedRowHandle = 0;
            gridMain.SelectRow(0);
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void SetPanelsBottom ( bool active )
        {
            if ( !active )
            {
                panelBottomBottom.Hide();
                panelBottom.Dock = DockStyle.Fill;
            }
            else
            {
                panelBottom.Dock = DockStyle.Top;
                panelBottomBottom.Show();
            }
        }
        /****************************************************************************************/
        private void LoadYearCombo ()
        {
            DateTime now = DateTime.Now;
            for ( int i=2012; i<now.Year+1; i++)
                cmbYear.Items.Add(i.ToString());
            cmbYear.Text = now.Year.ToString();
        }
        /****************************************************************************************/
        private void LoadData()
        {
            string year = GetValidYear();
            if (String.IsNullOrWhiteSpace(year))
                return;
            string cmd = "Select * from `agents` where `year` = '" + year + "' ";
            cmd += " ORDER BY `agentCode`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
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
            dRow["agentCode"] = "ZZ";
            dRow["mod"] = "Y";
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            gridMain.FocusedRowHandle = dt.Rows.Count - 1;
            gridMain.SelectRow(dt.Rows.Count-1);
            modified = true;
            btnSave.Show();
        }
        /****************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string year = GetValidYear();
            if (String.IsNullOrWhiteSpace(year))
                return;
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string status = "";
            string super = "";
            string fname = "";
            string lname = "";
            string agentCode = "";
            double commission = 0D;
            double goal = 0D;
            double goalpercent = 0D;
            string customGoals = "";
            string mod = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod != "Y")
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("agents", "lastName", "-1");
                if (G1.BadRecord("agents", record))
                    break;
                super = dt.Rows[i]["super"].ObjToString();
                agentCode = dt.Rows[i]["agentCode"].ObjToString();
                status = dt.Rows[i]["status"].ObjToString();
                fname = dt.Rows[i]["firstName"].ObjToString();
                lname = dt.Rows[i]["lastName"].ObjToString();
                commission = dt.Rows[i]["commission"].ObjToDouble();
                goal = dt.Rows[i]["goal"].ObjToDouble();
                goalpercent = dt.Rows[i]["goalpercent"].ObjToDouble();
                customGoals = dt.Rows[i]["customGoals"].ObjToString();
                G1.update_db_table("agents", "record", record, new string[] { "super", super, "agentCode", agentCode, "status", status, "firstName", fname, "lastName", lname, "commission", commission.ToString(), "goal", goal.ToString(), "goalpercent", goalpercent.ToString(), "year", year, "customGoals", customGoals});
                dt.Rows[i]["mod"] = "";
            }
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private string GetValidYear ()
        {
            DateTime now = DateTime.Now;
            string year = cmbYear.Text.Trim();
            if (!G1.validate_numeric(year))
            {
                MessageBox.Show("***ERROR*** Year (" + year + ") is invalid!");
                return "";
            }
            int iyear = year.ObjToInt32();
            if (iyear < 1980 || iyear > (now.Year + 1))
            {
                MessageBox.Show("***ERROR*** Year " + year + " is Out or Range (1980-" + (now.Year+1).ToString() + "!");
                return "";
            }
            return year;
        }
        /****************************************************************************************/
        private void editSplitsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void editAdditionalGoalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
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
            DataTable dt = (DataTable)dgv.DataSource;
            bool found = false;
            if ( e.Column.FieldName.ToUpper() == "AGENTCODE")
            {
                int row = gridMain.FocusedRowHandle;
                string agent = "";
                string str = e.Value.ObjToString();
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    agent = dt.Rows[i]["agentCode"].ObjToString();
                    if ( agent == str )
                    {
                        if ( i != row )
                        {
                            dr["agentCode"] = oldValue;
                            oldValue = "";
                            gridMain.FocusedRowHandle = i;
                            gridMain.SelectRow(i);
                            found = true;
                            break;
                        }
                    }
                }
            }
            if (!found)
            {
                dr["mod"] = "Y";
                modified = true;
                btnSave.Show();
                oldValue = "";
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
            if (modified)
            {
                DialogResult result = MessageBox.Show("***Question***\nInformation has been modified!\nWould you like to save your changes?", "Add/Edit Agents Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                    return;
                if (result == DialogResult.Yes)
                    btnSave_Click(null, null);
                    return;
            }
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


            //            Printer.DrawQuadTicks();
            //            DateTime date = this.dateTimePicker1.Value;
            string workDate = cmbYear.Text;
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            Printer.DrawQuad(20, 8, 5, 4, "Report Year:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void cmbYear_SelectedIndexChanged(object sender, EventArgs e)
        {
            loading = true;
            modified = false;
            LoadData();
            loading = false;
            modified = false;
            btnSave.Hide();
        }
        /****************************************************************************************/
        private void editCustomCommissionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }
        /****************************************************************************************/
        private void editGoalsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Goals goalForm = new Goals();
            goalForm.Show();
        }
        /****************************************************************************************/
        private string oldValue = "";
        /****************************************************************************************/
        private void gridMain_CellValueChanging(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            string columnName = e.Column.FieldName.ToUpper();
            if (columnName != "AGENTCODE")
            {
                oldValue = "";
                return;
            }
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int row = e.RowHandle;
            if (String.IsNullOrWhiteSpace(oldValue))
                oldValue = dt.Rows[row]["agentCode"].ObjToString();
        }
        /****************************************************************************************/
        private void SetActiveAgent ()
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string agent = dr["agentCode"].ObjToString();
            string cmd = "Select * from `goals` WHERE `agentCode` = '" + agent + "' ORDER by `effectiveDate`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("edate");
            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            SetPanelsBottom(true);
        }

        private void gridMain_Click(object sender, EventArgs e)
        {
            SetActiveAgent();

        }

        private void gridMain_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            SetActiveAgent();
        }
        /****************************************************************************************/
    }
}