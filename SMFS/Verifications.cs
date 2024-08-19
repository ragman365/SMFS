using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraPrinting;
using GeneralLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class Verifications : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private DataTable originalDt = null;
        private string _answer = "";
        private bool loading = true;
        private int pickupRow = -1;
        public string A_Answer { get { return _answer; } }
        /****************************************************************************************/
        public Verifications()
        {
            InitializeComponent();
        }
        /****************************************************************************************/
        private void Verifications_Load(object sender, EventArgs e)
        {
            _answer = "";
            barImport.Hide();

            btnStop.Hide();
            btnStop.Refresh();
            btnPickup.Hide();
            btnPickup.Refresh();
            stopRunning = false;

            lblCount.Hide();
            lblCount.Refresh();

            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            //AddSummaryColumn("endingBalance", null);
            //AddSummaryColumn("endingPaymentBalance", null);

            gridMain.Columns["endingPaymentBalance"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["endingPaymentBalance"].SummaryItem.DisplayFormat = "{0:C2}";

            gridMain.Columns["endingBalance"].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Custom;
            gridMain.Columns["endingBalance"].SummaryItem.DisplayFormat = "{0:C2}";

        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            dr["mod"] = "Y";
            modified = true;
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
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            //string delete = dt.Rows[row]["mod"].ObjToString();
            //if (delete.ToUpper() == "D")
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
        }
        /****************************************************************************************/
        private void EditTracking_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nData has been modified!\nWould you like to save your changes?", "Add/Edit Policy/Trusts Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            string record = "";
            string cmd = "";
            DataTable dx = null;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string from = cmbFrom.Text.Trim().ToUpper();
            if (from == "TRUSTEE DATA")
            {
                TrustData trustForm = new TrustData(contract);
                trustForm.Show();
                return;
            }
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                cmd = "";
                cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contract + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Customer Contract Does Not Exist!\nWould you like to create it anyway?", "Contract Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    record = G1.create_record("contracts", "contractNumber", "-1");
                    if (G1.BadRecord("contracts", record))
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    G1.update_db_table("contracts", "record", record, new string[] { "contractNumber", contract });
                }

                cmd = "Select * from `customers` WHERE `contractNumber` = '" + contract + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Customer Does Not Exist!\nDo you want to create it\nand then edit ?", "Customer Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    record = G1.create_record("customers", "contractNumber", "-1");
                    if (G1.BadRecord("customers", record))
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    G1.update_db_table("customers", "record", record, new string[] { "contractNumber", contract });
                }

                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.ListSourceRowIndex == DevExpress.XtraGrid.GridControl.InvalidRowHandle)
                return;
            double dValue = 0D;
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

            if (e.DisplayText.Trim() == "0.00")
                e.DisplayText = "";
        }
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
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
            gridMain.OptionsPrint.ExpandAllGroups = false;

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

            Printer.setupPrinterMargins(10, 5, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printingSystem1.Document.AutoFitToPagesWidth = 1;

            Font saveFont = gridMain.AppearancePrint.Row.Font;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            gridMain.Appearance.Row.Font = saveFont;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            gridMain.OptionsPrint.ExpandAllGroups = false;

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

            Printer.setupPrinterMargins(10, 5, 80, 50);

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
            //string title = "Contract Activity Report";
            //Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            string reportName = this.Text;
            string report = reportName;
            Printer.DrawQuad(5, 8, 8, 4, report, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);



            //DateTime date = this.dateTimePicker1.Value;
            //string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            //Printer.DrawQuad(20, 8, 5, 4, "Month Closing - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            //Trustee Data
            //Charlotte Trust Data
            //Contracts
            //Customers

            btnStop.Hide();
            btnStop.Refresh();
            btnPickup.Hide();
            btnPickup.Refresh();
            pickupRow = -1;

            stopRunning = false;

            lblCount.Hide();
            lblCount.Refresh();

            barImport.Hide();
            barImport.Refresh();

            if (chkExists.Checked)
                lblNotIn.Text = "-  Is In -";
            else
                lblNotIn.Text = "- Not In -";
            lblNotIn.Refresh();

            DataRow[] dRows = null;
            string title = "Valid Alive Contracts in " + cmbFrom.Text.Trim() + " -Not In- " + cmbTo.Text.Trim();
            if (chkExists.Checked)
                title = "Valid Alive Contracts in " + cmbFrom.Text.Trim() + " -Also In- " + cmbTo.Text.Trim();
            this.Text = title;
            string from = cmbFrom.Text.Trim();
            string to = cmbTo.Text.Trim();
            if (String.IsNullOrWhiteSpace(from) || String.IsNullOrWhiteSpace(to))
            {
                MessageBox.Show("***ERROR*** From or To is Blank!", "Run Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            if (from == to)
            {
                MessageBox.Show("***ERROR*** From and To Cannot be the same!", "Run Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            string cmd = "";
            DataTable dx = null;
            this.Cursor = Cursors.WaitCursor;
            if (from == "Trustee Data")
            {
                if (to == "Contracts")
                    cmd = "SELECT * FROM `trust_data` WHERE NOT EXISTS(SELECT * FROM `contracts` b WHERE b.`contractNumber` = trust_data.`contractNumber`) AND `billingReason` <> 'DC' AND `deathClaimAmount` = '0' GROUP BY `contractNumber`";
                else if (to == "Customers")
                    cmd = "SELECT * FROM `trust_data` WHERE NOT EXISTS(SELECT * FROM `customers` b WHERE b.`contractNumber` = trust_data.`contractNumber`) AND `billingReason` <> 'DC' AND `deathClaimAmount` = '0' GROUP BY `contractNumber`;";
                else if (to == "Charlotte Trust Data")
                {
                    cmd = "SELECT * FROM `trust_data` WHERE NOT EXISTS(SELECT * FROM `trust2013r` b WHERE b.`contractNumber` = trust_data.`contractNumber` AND currentRemovals = '0' ) AND `billingReason` <> 'DC' AND `deathClaimAmount` = '0' GROUP BY `contractNumber`;";
                    if (chkExists.Checked)
                    {
                        cmd = "SELECT * FROM `trust_data` WHERE EXISTS(SELECT * FROM `trust2013r` b WHERE b.`contractNumber` = trust_data.`contractNumber` AND currentRemovals = '0' ) AND `billingReason` <> 'DC' AND `deathClaimAmount` = '0' GROUP BY `contractNumber`;";
                        //cmd = "SELECT * FROM `trust_data` r JOIN `trust2013r` b WHERE b.`contractNumber` = r.`contractNumber` AND currentRemovals = '0' ) AND `billingReason` <> 'DC' AND `deathClaimAmount` = '0' GROUP BY r.`contractNumber`;";
                    }
                }
                dx = G1.get_db_data(cmd);
                dx = filter300Series(dx);
            }
            else if (from == "Charlotte Trust Data")
            {
                if (to == "Trustee Data")
                {
                    cmd = "SELECT *, c.`lastName` FROM `trust2013r` r JOIN `customers` c ON r.`contractNumber` = c.`contractNumber` WHERE NOT EXISTS(SELECT * FROM `trust_data` b WHERE b.`contractNumber` = r.`contractNumber` AND b.`billingReason` <> 'DC') AND currentRemovals = '0' AND c.`deceasedDate` < '1000-01-01' GROUP BY r.`contractNumber`;";
                    cmd = "SELECT r.contractNumber, MAX(payDate8),MAX(currentRemovals) AS MaxDate,r.lastName,r.firstName,r.endingBalance FROM trust2013r r WHERE NOT EXISTS(SELECT* FROM `trust_data` b WHERE b.`contractNumber` = r.`contractNumber` AND ( b.`billingReason` <> 'DC' OR b.`deathClaimAmount` = '0') ) AND r.`contractNumber` NOT LIKE 'RF%' AND r.`contractNumber` NOT LIKE 'NM%' GROUP BY r.contractNumber; ";
                    if (!chkExists.Checked)
                        dx = GetTrust2013ToTrustData(true);
                    if (chkExists.Checked)
                    {
                        cmd = "SELECT *, c.`lastName` FROM `trust2013r` r JOIN `customers` c ON r.`contractNumber` = c.`contractNumber` WHERE EXISTS(SELECT * FROM `trust_data` b WHERE b.`contractNumber` = r.`contractNumber` AND b.`billingReason` <> 'DC') AND currentRemovals = '0' AND c.`deceasedDate` < '1000-01-01' GROUP BY r.`contractNumber`;";
                        cmd = "SELECT r.contractNumber, MAX(payDate8),MAX(endingBalance) AS MaxDate,r.lastName,r.firstName,r.endingBalance FROM trust2013r r WHERE EXISTS(SELECT* FROM `trust_data` b WHERE b.`contractNumber` = r.`contractNumber` AND ( b.`billingReason` <> 'DC' OR b.`deathClaimAmount` = '0') ) GROUP BY r.contractNumber; ";
                        cmd = "SELECT r.contractNumber, MAX(payDate8),MAX(endingBalance) AS MaxDate,MAX(currentRemovals) as currentRemovals, r.lastName,r.firstName,r.endingBalance,b.policyNumber,MAX(b.endingPaymentBalance) AS endingPaymentBalance FROM trust2013r r LEFT JOIN `trust_data` b ON b.`contractNumber` = r.`contractNumber` WHERE ( b.`billingReason` <> 'DC' OR b.`deathClaimAmount` = '0') AND r.`contractNumber` NOT LIKE 'RF%' AND r.`contractNumber` NOT LIKE 'NM%' GROUP BY r.contractNumber;";
                        dx = GetTrust2013ToTrustData(false);
                    }
                }
                else if (to == "Customers")
                {
                    cmd = "SELECT * FROM `trust2013r` r WHERE NOT EXISTS(SELECT* FROM `customers` b WHERE b.`contractNumber` = r.`contractNumber` ) AND currentRemovals = '0' GROUP BY r.`contractNumber`;";
                    dx = G1.get_db_data(cmd);
                }
                else if (to == "Contracts")
                {
                    cmd = "SELECT * FROM `trust2013r` r WHERE NOT EXISTS(SELECT* FROM `contracts` b WHERE b.`contractNumber` = r.`contractNumber` ) AND currentRemovals = '0' GROUP BY r.`contractNumber`;";
                    if (chkExists.Checked)
                        cmd = "SELECT r.contractNumber, MAX(payDate8),MAX(endingBalance) AS MaxDate,MAX(currentRemovals) as currentRemovals, r.lastName,r.firstName,r.endingBalance FROM trust2013r r LEFT JOIN `contracts` b ON b.`contractNumber` = r.`contractNumber` GROUP BY r.contractNumber;";
                    dx = G1.get_db_data(cmd);
                }
            }
            else if (from == "Contracts")
            {
                if (to == "Charlotte Trust Data")
                    cmd = "SELECT * FROM `contracts` r JOIN `customers` c ON r.`contractNumber` = c.`contractNumber` WHERE NOT EXISTS(SELECT * FROM `trust2013r` b WHERE b.`contractNumber` = r.`contractNumber` AND currentRemovals = '0' ) AND c.`deceasedDate` < '1000-01-01' GROUP BY r.`contractNumber`;";
                else if (to == "Trustee Data")
                    cmd = "SELECT *, c.`lastName` FROM `contracts` r JOIN `customers` c ON r.`contractNumber` = c.`contractNumber` WHERE NOT EXISTS(SELECT * FROM `trust2013r` b WHERE b.`contractNumber` = r.`contractNumber` AND currentRemovals = '0' ) AND c.`deceasedDate` < '1000-01-01' GROUP BY r.`contractNumber`;";
                else if (to == "Customers")
                    cmd = "SELECT * FROM `contracts` r WHERE NOT EXISTS(SELECT* FROM `customers` b WHERE b.`contractNumber` = r.`contractNumber` AND b.`deceasedDate` < '1000-01-01') AND r.`deceasedDate` < '1000-01-01' GROUP BY r.`contractNumber`;";
                dx = G1.get_db_data(cmd);
            }
            else if (from == "Customers")
            {
                if (to == "Charlotte Trust Data")
                    cmd = "SELECT * FROM `customers` r WHERE NOT EXISTS(SELECT * FROM `trust2013r` b WHERE b.`contractNumber` = r.`contractNumber` AND currentRemovals = '0' ) AND c.`deceasedDate` < '1000-01-01' GROUP BY r.`contractNumber`;";
                else if (to == "Contracts")
                    cmd = "SELECT * FROM `customers` r WHERE NOT EXISTS(SELECT* FROM `contracts` b WHERE b.`contractNumber` = r.`contractNumber` ) AND r.`deceasedDate` < '1000-01-0' GROUP BY r.`contractNumber`;";
                else if (to == "Trustee Data")
                    cmd = "SELECT * FROM `customers` r WHERE NOT EXISTS(SELECT * FROM `trust_data` b WHERE b.`contractNumber` = r.`contractNumber`  AND b.`billingReason` <> 'DC' AND `deathClaimAmount` = '0' ) AND r.`deceasedDate` < '1000-01-01' GROUP BY r.`contractNumber`;";
                dx = G1.get_db_data(cmd);
            }
            /*
            SELECT* FROM `trust_data` WHERE NOT EXISTS(SELECT * FROM `contracts` b WHERE b.`contractNumber` = trust_data.`contractNumber`) AND `billingReason` <> 'DC' AND `deathClaimAmount` = '0' GROUP BY `contractNumber`;
            SELECT* FROM `trust_data` WHERE NOT EXISTS(SELECT * FROM `customers` b WHERE b.`contractNumber` = trust_data.`contractNumber`) AND `billingReason` <> 'DC' AND `deathClaimAmount` = '0' GROUP BY `contractNumber`;

            SELECT *, c.`lastName` FROM `trust2013r` r JOIN `customers` c ON r.`contractNumber` = c.`contractNumber` WHERE NOT EXISTS(SELECT * FROM `trust_data` b WHERE b.`contractNumber` = r.`contractNumber` AND b.`billingReason` <> 'DC') AND currentRemovals = '0' AND c.`deceasedDate` < '1000-01-01' GROUP BY r.`contractNumber`;
            SELECT* FROM `trust_data` r WHERE NOT EXISTS(SELECT* FROM `customers` b WHERE b.`contractNumber` = r.`contractNumber` AND b.`deceasedDate` < '1000-01-01' ) AND r.`billingReason` <> 'DC' GROUP BY r.`contractNumber`;

            SELECT* FROM `trust2013r` r WHERE NOT EXISTS(SELECT* FROM `customers` b WHERE b.`contractNumber` = r.`contractNumber` ) AND currentRemovals = '0' GROUP BY r.`contractNumber`;
            SELECT* FROM `trust2013r` r WHERE NOT EXISTS(SELECT* FROM `contracts` b WHERE b.`contractNumber` = r.`contractNumber` ) AND currentRemovals = '0' GROUP BY r.`contractNumber`;
            */

            if (String.IsNullOrWhiteSpace(cmd))
            {
                MessageBox.Show("***ERROR*** Invalid Combination", "Run Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }


            //DataTable dx = G1.get_db_data(cmd);
            dRows = dx.Select("contractNumber = 'B17064LI'");
            if ( dRows.Length > 0 )
            {
                DataTable ddd = dRows.CopyToDataTable();
            }
            if (from == "Charlotte Trust Data" && to == "Trustee Data")
            {
                if (chkExists.Checked)
                {
                    //FixData(dx);
                    dRows = dx.Select("currentRemovals <= '0'"); // should be all live contracts
                    if (dRows.Length > 0)
                        dx = dRows.CopyToDataTable();
                }
            }
            else if (from == "Charlotte Trust Data" && to == "Contracts")
            {
                if (chkExists.Checked)
                {
                    //dRows = dx.Select("MaxDate > '0'");
                    dRows = dx.Select("currentRemovals <= '0'");
                }
                else
                    dRows = dx.Select("MaxDate <= '0'");
                if (dRows.Length > 0)
                    dx = dRows.CopyToDataTable();
            }
            dRows = dx.Select("contractNumber<>''");
            if (dRows.Length > 0)
                dx = dRows.CopyToDataTable();

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;

            dRows = dx.Select("contractNumber = 'B0638'");
            if ( dRows.Length > 0 )
            {
                DataTable ddd = dRows.CopyToDataTable();
            }


            gridMain.RefreshData();
            gridMain.RefreshEditor(true);
            dgv.Refresh();

            this.Cursor = Cursors.Default;
        }
        /****************************************************************************************/
        private DataTable GetTrust2013ToTrustData ( bool notIn )
        {
            string cmd = "SELECT * FROM ( SELECT MAX(r.record) AS record FROM trust2013r r GROUP BY r.`contractNumber`) AS sw INNER JOIN trust2013r ON trust2013r.record = sw.record WHERE `endingBalance` > '1.00' AND `contractNumber` NOT LIKE 'RF%' AND `contractNumber` NOT LIKE 'NM%' && `payDate8` > '2020-01-01';";
            DataTable dt1 = G1.get_db_data(cmd);

            cmd = "SELECT * FROM ( SELECT MAX(r.record) AS record FROM trust_data r GROUP BY r.`contractNumber`) AS sw INNER JOIN trust_data ON trust_data.record = sw.record WHERE ( `billingReason` <> 'DC' OR `deathClaimAmount` = '0' );";
            DataTable dt2 = G1.get_db_data(cmd);
            dt2 = filter300Series(dt2);

            var prod = dt2.AsEnumerable().ToDictionary(p => p["contractNumber"]);

            var query = from imp in dt1.AsEnumerable()  where !prod.ContainsKey(imp["contractNumber"]) select imp;
            if (notIn == false )
                query = from imp in dt1.AsEnumerable() where prod.ContainsKey(imp["contractNumber"]) select imp;

            DataTable newDt = dt1.Clone();
            try
            {
                newDt = query.CopyToDataTable();
            }
            catch (Exception ex)
            {
            }

            newDt.Columns.Add("endingPaymentBalance", Type.GetType("System.Double"));

            if ( notIn == false )
            {

                DataRow[] dRows = null;
                string contractNumber = "";
                double endingPaymentBalance = 0D;
                for ( int i=0; i<newDt.Rows.Count; i++)
                {
                    contractNumber = newDt.Rows[i]["contractNumber"].ObjToString();
                    dRows = dt2.Select("contractNumber='" + contractNumber + "'");
                    if (dRows.Length > 0)
                        newDt.Rows[i]["endingPaymentBalance"] = dRows[0]["endingPaymentBalance"].ObjToDouble();
                }
            }


            return newDt;
        }
        /****************************************************************************************/
        private DataTable filter300Series ( DataTable dt )
        {
            string contractNumber = "";
            string contract = "";
            string trust = "";
            string loc = "";
            DataTable dx = dt.Clone();
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                contractNumber = dt.Rows[i]["contractNumber"].ObjToString();

                contract = Trust85.decodeContractNumber(contractNumber, ref trust, ref loc);
                if (contract.Length == 5)
                {
                    contract = contract.Substring(2, 1);
                    if (contract == "3")
                    {
                        dx.ImportRow(dt.Rows[i]);
                        if (chkFilter300.Checked)
                            dt.Rows.RemoveAt(i);
                    }
                }
            }
            if (chkOnly300.Checked)
                dt = dx.Copy();
            return dt;
        }
        /****************************************************************************************/
        private void FixData(DataTable dt)
        {
            string chart = "";
            double dValue1 = 0D;
            double dValue2 = 0D;
            double removals = 0D;
            double finale = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dValue1 = dt.Rows[i]["endingBalance"].ObjToDouble();
                dValue2 = dt.Rows[i]["MaxDate"].ObjToDouble();
                removals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                finale = dValue2 - removals;
                //dt.Rows[i]["endingBalance"] = dt.Rows[i]["MaxDate"].ObjToDouble();
                dt.Rows[i]["endingBalance"] = finale;
                if (finale > 0D && removals > 0D)
                    dt.Rows[i]["currentRemovals"] = 0D;
            }
        }
    /****************************************************************************************/
    private void checkForCharlotteTrustDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string to = cmbTo.Text.Trim();
                string cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    MessageBox.Show("***Confirmed*** Contract Does Not Exist in the Charlotte Trust Data!", "Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                else
                    MessageBox.Show("*** Okay *** Contract DOES Exist in the Charlotte Trust Data!", "Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void checkForTrusteeDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string to = cmbTo.Text.Trim();
                string cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    MessageBox.Show("***Confirmed*** Contract Does Not Exist in the Trustee Data!", "Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                else
                    MessageBox.Show("*** Okay *** Contract DOES Exist in the Trustee Data!", "Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void checkForContractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string to = cmbTo.Text.Trim();
                string cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    MessageBox.Show("***Confirmed*** Contract Does Not Exist in the Contract Data!", "Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                else
                    MessageBox.Show("*** Okay *** Contract DOES Exist in the Contract Data!", "Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void checkForCustomerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                string to = cmbTo.Text.Trim();
                string cmd = "Select * from `customers` WHERE `contractNumber` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    MessageBox.Show("***Confirmed*** Contract Does Not Exist in the Customer Data!", "Contract Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                else
                    MessageBox.Show("*** Okay *** Contract DOES Exist in the Customer Data!", "Contract Confirmed Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private void btnValidate_Click(object sender, EventArgs e)
        {
            pickupRow = -1;
            ValidateAll();
        }
        /****************************************************************************************/
        private void ValidateAll ()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string chart = txtContract.Text.Trim();
            if ( !String.IsNullOrWhiteSpace ( chart ))
            {
                DataRow[] xxRows = dt.Select("contractNumber='" + chart + "'");
                if (xxRows.Length > 0)
                    dt = xxRows.CopyToDataTable();
            }
            int lastRow = dt.Rows.Count;
            //lastRow = 10;

            barImport.Show();
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            barImport.Value = 0;
            barImport.Refresh();

            lblCount.Text = lastRow.ToString();
            lblCount.Show();
            lblCount.Refresh();

            string contractNumber = "";
            string newContract = "";
            string cmd = "";
            string found = "";
            DataTable dx = null;
            DateTime deceasedDate = DateTime.Now;
            double dValue = 0D;
            double ai = 0D;
            double am = 0D;
            string str = "";
            string lapsed = "";
            string policyNumber = "";

            cmd = "Select * from `policyTrusts` p JOIN `contracts` c ON p.`contractNumber` = c.`contractNumber`;";
            DataTable ptDt = G1.get_db_data(cmd);

            if (G1.get_column_number(dt, "trusteeData") < 0)
                dt.Columns.Add("trusteeData");
            if (G1.get_column_number(dt, "contract") < 0)
                dt.Columns.Add("contract");
            if (G1.get_column_number(dt, "customer") < 0)
                dt.Columns.Add("customer");
            if (G1.get_column_number(dt, "dDate") < 0)
                dt.Columns.Add("dDate");
            if (G1.get_column_number(dt, "services") < 0)
                dt.Columns.Add("services");
            if (G1.get_column_number(dt, "endingBalance") < 0)
                dt.Columns.Add("endingBalance", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "endingPaymentBalance") < 0)
                dt.Columns.Add("endingPaymentBalance", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "tbb") < 0)
                dt.Columns.Add("tbb");

            string from = cmbFrom.Text.Trim().ToUpper();
            string to = cmbTo.Text.Trim().ToUpper();

            string service = "";
            string data = "";
            bool noService = false;

            btnStop.Show();
            btnStop.Refresh();
            btnPickup.Hide();
            btnPickup.Refresh();
            int startRow = 0;
            if (pickupRow > 0)
                startRow = pickupRow;
            else
            {
                double dValue1 = 0D;
                double dValue2 = 0D;
                double removals = 0D;


                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    dt.Rows[i]["services"] = "";
                    dt.Rows[i]["contract"] = "";
                    dt.Rows[i]["customer"] = "";
                    dt.Rows[i]["trusteeData"] = "";
                    dt.Rows[i]["tbb"] = "";
                    if (chkExists.Checked  )
                    {
                        //if (from == "CHARLOTTE TRUST DATA" && to == "TRUSTEE DATA")
                        //{
                        //    dValue1 = dt.Rows[i]["endingBalance"].ObjToDouble();
                        //    dValue2 = dt.Rows[i]["MaxDate"].ObjToDouble();
                        //    removals = dt.Rows[i]["currentRemovals"].ObjToDouble();
                        //    if ( dValue2 > 0D && removals > 0D )
                        //    {
                        //        chart = dt.Rows[i]["contractNumber"].ObjToString();
                        //    }
                        //    if (dValue1 != dValue2 )
                        //    {
                        //        chart = dt.Rows[i]["contractNumber"].ObjToString();
                        //    }
                        //    dt.Rows[i]["endingBalance"] = dt.Rows[i]["MaxDate"].ObjToDouble();
                        //}
                    }
                    else
                    {
                        if (from != "CHARLOTTE TRUST DATA" && !chkExists.Checked)
                            dt.Rows[i]["endingBalance"] = 0D;
                        else if (from != "TRUSTEE DATA" && !chkExists.Checked)
                            dt.Rows[i]["endingPaymentBalance"] = 0D;
                    }
                }
            }
            pickupRow = -1;
            bool first = true;
            DateTime trustDate = DateTime.Now;
            DateTime date = DateTime.Now;
            double balance = 0D;
            double oldBalance = 0D;
            DataRow[] dRows = null;

            //Trustee Data
            //Charlotte Trust Data
            //Contracts
            //Customers

            DataTable tempDt = null;


            for (int i = startRow; i < lastRow; i++)
            {
                if (stopRunning)
                {
                    stopRunning = false;
                    btnStop.Hide();
                    btnStop.Refresh();

                    btnPickup.Show();
                    btnPickup.Refresh();
                    pickupRow = i;
                    break;
                }
                Application.DoEvents();

                try
                {
                    barImport.Value = i;
                    barImport.Refresh();

                    contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                    if ( contractNumber == "HT21017LI")
                    {
                    }

                    if (from != "TRUSTEE DATA" )
                    {
                        //cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contractNumber + "' AND `billingReason` <> 'DC' AND `deathClaimAmount` = '0' LIMIT 1;";
                        cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contractNumber + "' LIMIT 1;";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                            dt.Rows[i]["trusteeData"] = "X";
                        else
                        {
                            if (to != "TRUSTEE DATA")
                            {
                                dValue = dx.Rows[0]["endingPaymentBalance"].ObjToDouble();
                                dt.Rows[i]["endingPaymentBalance"] = dValue;
                                gridMain.RefreshData();
                                gridMain.RefreshEditor(true);
                            }
                            else
                            {
                                cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contractNumber + "' ORDER BY `date` DESC;";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                {
                                    balance = 0D;
                                    trustDate = dx.Rows[0]["date"].ObjToDateTime();
                                    for (int j = 0; j < dx.Rows.Count; j++)
                                    {
                                        date = dx.Rows[j]["date"].ObjToDateTime();
                                        if (date != trustDate)
                                        {
                                            balance = G1.RoundValue(balance);
                                            oldBalance = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                                            if (oldBalance != balance)
                                                dt.Rows[i]["endingPaymentBalance"] = balance;
                                            break;
                                        }
                                        balance += dx.Rows[j]["endingPaymentBalance"].ObjToDouble();
                                    }
                                    balance = G1.RoundValue(balance);
                                    oldBalance = dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                                    if ( oldBalance != balance )
                                        dt.Rows[i]["endingPaymentBalance"] = balance;
                                }
                            }
                            if (dx.Rows[0]["billingReason"].ObjToString().Trim().ToUpper() == "DC")
                                dt.Rows[i]["trusteeData"] = "DC";
                            else
                            {
                                dValue = dx.Rows[0]["deathClaimAmount"].ObjToDouble();
                                if (dValue > 0D)
                                    dt.Rows[i]["trusteeData"] = "DC";
                            }
                        }
                    }

                    if (from != "CHARLOTTE TRUST DATA")
                    {
                        //cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + contractNumber + "' AND `currentRemovals` = '0' ORDER BY `payDate8` DESC LIMIT 1;";
                        cmd = "Select * from `trust2013r` WHERE `contractNumber` = '" + contractNumber + "' ORDER BY `payDate8` DESC LIMIT 1;";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                            dt.Rows[i]["tbb"] = "X";
                        else
                        {
                            dValue = dx.Rows[0]["currentRemovals"].ObjToDouble();
                            if ( dValue > 0D )
                                dt.Rows[i]["tbb"] = "R";
                            dt.Rows[i]["endingBalance"] = dx.Rows[0]["endingBalance"].ObjToDouble();
                            if (chkExists.Checked)
                            {
                                //dt.Rows[i]["endingBalance"] = dx.Rows[0]["MaxDate"].ObjToDouble();
                                dt.Rows[i]["endingBalance"] = dx.Rows[0]["endingbalance"].ObjToDouble();
                            }
                            gridMain.RefreshData();
                            gridMain.RefreshEditor(true);
                        }
                    }

                    if (from != "CONTRACTS")
                    {
                        cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contractNumber + "' LIMIT 1;";
                        dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count <= 0 && from == "TRUSTEE DATA")
                        {
                            policyNumber = dt.Rows[i]["policyNumber"].ObjToString();
                            if ( !String.IsNullOrWhiteSpace ( policyNumber))
                            {
                                dRows = ptDt.Select("policyNumber='" + policyNumber + "'");
                                if ( dRows.Length > 0 )
                                {
                                    tempDt = dRows.CopyToDataTable();
                                    newContract = dRows[0]["contractNumber"].ObjToString();
                                    if (newContract != contractNumber)
                                    {
                                        cmd = "Select * from `contracts` WHERE `contractNumber` = '" + newContract + "' LIMIT 1;";
                                        dx = G1.get_db_data(cmd);
                                        if (dx.Rows.Count > 0)
                                        {
                                            contractNumber = newContract;
                                            dt.Rows[i]["contractNumber"] = contractNumber;
                                        }
                                    }
                                }
                            }
                        }
                        if (dx.Rows.Count > 0)
                        {
                            deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                            if (deceasedDate.Year > 1000)
                                dt.Rows[i]["dDate"] = deceasedDate.ToString("MM/dd/yyyy");
                            lapsed = dx.Rows[0]["lapsed"].ObjToString().ToUpper();
                            ai = dx.Rows[0]["allowInsurance"].ObjToDouble();
                            am = dx.Rows[0]["allowMerchandise"].ObjToDouble();
                            dValue = ai + am;
                            if (dValue > 0D)
                            {
                                str = "";
                                if (ai > 0D)
                                    str = "AI";
                                if (am > 0D)
                                {
                                    if (!String.IsNullOrWhiteSpace(str))
                                        str += "/AM";
                                    else
                                        str = "AM";
                                }
                                else if ( am > 0D )
                                    str = "AM";
                                if ( !String.IsNullOrWhiteSpace ( lapsed ))
                                {
                                    if (!String.IsNullOrWhiteSpace(str))
                                        str += "-L";
                                    else
                                        str = "L";
                                }
                                dt.Rows[i]["contract"] = str;
                                balance = dt.Rows[i]["endingBalance"].ObjToDouble();
                                if ( chkExists.Checked && dt.Rows[i]["trusteeData"].ObjToString() != "DC")
                                    dValue += balance;
                                //dt.Rows[i]["endingBalance"] = dValue;
                                if (from == "TRUSTEE DATA")
                                {
                                    cmd = "Select * from `trust_data` WHERE `contractNumber` = '" + contractNumber + "' ORDER BY `date` DESC;";
                                    dx = G1.get_db_data(cmd);
                                    if ( dx.Rows.Count > 0 )
                                    {
                                        balance = 0D;
                                        trustDate = dx.Rows[0]["date"].ObjToDateTime();
                                        for ( int j=0; j<dx.Rows.Count; j++)
                                        {
                                            date = dx.Rows[j]["date"].ObjToDateTime();
                                            if ( date != trustDate )
                                            {
                                                dt.Rows[i]["endingPaymentBalance"] = balance;
                                                break;
                                            }
                                            balance += dx.Rows[j]["endingPaymentBalance"].ObjToDouble();
                                        }
                                    }
                                }
                            }
                        }
                        else
                            dt.Rows[i]["contract"] = "X";
                    }

                    if (from != "CUSTOMERS")
                    {
                        cmd = "Select * from `customers` WHERE `contractNumber` = '" + contractNumber + "' LIMIT 1;";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            deceasedDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                            if (deceasedDate.Year > 1000)
                                dt.Rows[i]["dDate"] = deceasedDate.ToString("MM/dd/yyyy");
                        }
                        else
                            dt.Rows[i]["customer"] = "X";
                    }
                    cmd = "Select * from `cust_services` WHERE `contractNumber` = '" + contractNumber + "' LIMIT 1;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        dt.Rows[i]["services"] = "X";
                    else
                    {
                        noService = false;
                        for ( int j=0; j<dx.Rows.Count; j++)
                        {
                            service = dx.Rows[j]["service"].ObjToString().ToUpper();
                            data = dx.Rows[j]["data"].ObjToString();
                            if (service == "CASKET NAME" && string.IsNullOrWhiteSpace(data))
                            {
                                noService = true;
                                continue;
                            }
                            else if (service == "OUTER CONTAINER NAME" && string.IsNullOrWhiteSpace(data))
                            {
                                noService = true;
                                continue;
                            }
                            if (service == "CASKET PRICE" || service == "OUTER CONTAINER PRICE")
                                continue;
                            noService = false;
                            break;
                        }
                        if (noService)
                            dt.Rows[i]["services"] = "X";
                    }
                    if (!this.Visible)
                        break;
                }
                catch ( Exception ex)
                {
                }
            }

            dgv.DataSource = dt;
            dgv.Refresh();

            gridMain.PostEditor();
            gridMain.RefreshData();
            gridMain.RefreshEditor(true);

            barImport.Value = lastRow;
            barImport.Refresh();

            btnStop.Hide();
            btnStop.Refresh();
        }
        /****************************************************************************************/
        private void lookupDailyHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string record = "";
            string cmd = "";
            DataTable dx = null;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string from = cmbFrom.Text.Trim().ToUpper();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                cmd = "";
                cmd = "Select * from `contracts` WHERE `contractNumber` = '" + contract + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Customer Contract Does Not Exist!\nWould you like to create it anyway?", "Contract Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    record = G1.create_record("contracts", "contractNumber", "-1");
                    if (G1.BadRecord("contracts", record))
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    G1.update_db_table("contracts", "record", record, new string[] { "contractNumber", contract });
                }

                cmd = "Select * from `customers` WHERE `contractNumber` = '" + contract + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                {
                    DialogResult result = MessageBox.Show("***ERROR*** Customer Does Not Exist!\nDo you want to create it\nand then edit ?", "Customer Error Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if (result == DialogResult.No)
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    record = G1.create_record("customers", "contractNumber", "-1");
                    if (G1.BadRecord("customers", record))
                    {
                        this.Cursor = Cursors.Default;
                        return;
                    }
                    G1.update_db_table("customers", "record", record, new string[] { "contractNumber", contract });
                }

                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private bool stopRunning = false;
        private void btnStop_Click(object sender, EventArgs e)
        {
            stopRunning = true;
        }
        /****************************************************************************************/
        private void gridMain_CustomSummaryCalculate(object sender, DevExpress.Data.CustomSummaryEventArgs e)
        {
            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                if (dt.Rows.Count <= 0)
                    return;
                double value = e.TotalValue.ObjToDouble();
                double totalValue = 0D;
                string field = (e.Item as GridSummaryItem).FieldName.ObjToString();


                if (field.ToUpper() == "ENDINGBALANCE")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        totalValue += dt.Rows[i]["endingBalance"].ObjToDouble();
                    e.TotalValueReady = true;
                    e.TotalValue = totalValue;
                    gridMain.PostEditor();
                    gridMain.RefreshEditor(true);
                }
                if (field.ToUpper() == "ENDINGPAYMENTBALANCE")
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        totalValue += dt.Rows[i]["endingPaymentBalance"].ObjToDouble();
                    e.TotalValueReady = true;
                    e.TotalValue = totalValue;
                    gridMain.PostEditor();
                    gridMain.RefreshEditor(true);
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************************/
        private void btnPickup_Click(object sender, EventArgs e)
        {
            ValidateAll();
        }
        /****************************************************************************************/
        private void lookupTtrusteeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contract))
                return;
            string policyNumber = dr["policyNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(policyNumber))
                return;
            contract = "";

            this.Cursor = Cursors.WaitCursor;
            TrustData trustForm = new TrustData(contract, policyNumber );
            trustForm.Show();
            this.Cursor = Cursors.Default;
            return;
        }
        /****************************************************************************************/
    }
}