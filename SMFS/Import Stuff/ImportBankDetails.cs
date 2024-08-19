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
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;


//using java.awt;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportBankDetails : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool saveChangedData = false;
        private DataTable workDt = null;
        private DataTable workSaveDt = null;
        private bool testingAll = false;
        private string saveTitle = "";
        private DateTime workDate = DateTime.Now;
        private bool loaded = false;
        /***********************************************************************************************/
        public ImportBankDetails()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        public ImportBankDetails( DataTable sDt, DataTable dt, DateTime date, string Title = "" )
        {
            InitializeComponent();
            workSaveDt = sDt;
            workDt = dt;
            saveTitle = Title;
            workDate = date;
            this.dateTimePicker2.Value = date;
            this.dateTimePicker3.Value = date;
        }
        /***********************************************************************************************/
        private void ImportBankDetails_Load(object sender, EventArgs e)
        {
            picAdd.Hide();
            pictureDelete.Hide();
            barImport.Hide();

            btnSaveNotPosted.Hide();

            btnShowDateRange.Hide();
            btnShowDaySpan.Hide();
            btnCurrentDay.Hide();
            btnFindDifference.Hide();

            SetupTotalsSummary();

            GetBankAccounts();

            if (!G1.RobbyServer)
                tabControl1.TabPages.Remove(tabPage2);

            tabControl1.SelectedTab = tabPage3;
            panelTab3Middle.Hide();

            chkGroup.Hide();
            chkGroupDays.Hide();
            dgv3.ContextMenuStrip = this.contextMenuStrip2;

            if ( workDt != null )
            {
                if (loaded)
                    this.Cursor = Cursors.WaitCursor;
                picAdd.Show();
                picAdd.Refresh();
                pictureDelete.Show();
                pictureDelete.Refresh();
                panelTab3Middle.Show();
                chkAll.Hide();

                if ( workDt.Rows.Count > 0 )
                {
                    DateTime date = workDt.Rows[0]["date"].ObjToDateTime();
                    string bankAccount = workDt.Rows[0]["bankAccount"].ObjToString();
                    DataTable bankDt = G1.get_db_data("Select * from `bank_accounts` WHERE `account_no` = '" + bankAccount + "';");
                    if (bankDt.Rows.Count > 0)
                        bankAccount = bankDt.Rows[0]["account_title"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
                    if (String.IsNullOrWhiteSpace(saveTitle))
                        this.Text = "In-System Deposits on " + date.ToString("MM/dd/yyyy") + " for " + bankAccount;
                    else
                        this.Text = saveTitle;
                }

                if (!loaded)
                {
                    int top = this.Top - 40;
                    int left = this.Left + 400;
                    int width = this.Width - 400;
                    int height = this.Height + 200;
                    this.SetBounds(left, top, width, height);
                }

                double diff = 0D;
                double amount = 0D;
                double systemAmount = 0D;

                for (int i = 0; i < workDt.Rows.Count; i++)
                {
                    amount = workDt.Rows[i]["amount"].ObjToDouble();
                    systemAmount = workDt.Rows[i]["inSystem"].ObjToDouble();
                    diff = amount - systemAmount;
                    workDt.Rows[i]["diff"] = diff;
                }


                gridMain3.PaintStyleName = "Office2003";

                Matchup(workDt);

                tabControl1.TabPages.Remove(tabPage1);
                panelTab3Top.Hide();
                dgv3.DataSource = workDt;
                dgv3.Refresh();
                gridMain3.Columns["depositNumber"].Visible = true;
                gridMain3.Columns["bankDetails"].Visible = false;
                gridMain3.Columns["inSystem"].Visible = true;
                gridMain3.Columns["inSystem"].Caption = "Amount in Bank";
                gridMain3.Columns["amount"].Caption = "Amount in System";
                gridMain3.CollapseAllGroups();
                gridMain3.RefreshEditor(true);
                gridMain3.Columns["found"].GroupIndex = 0;
                gridMain3.ExpandAllGroups();
                gridMain3.RefreshEditor( true );
                dgv3.Refresh();
                dgv3.ContextMenuStrip = this.contextMenuStrip1;

                btnFindDifference.Show();
                loaded = true;
                this.Cursor = Cursors.Default;
            }
            else
            {
                gridMain3.Columns["depositNumber"].Visible = false;
                gridMain3.Columns["tmstamp"].Visible = false;
            }
        }
        /****************************************************************************************/
        private void Matchup(DataTable dt)
        {
            //DataTable dt = (DataTable)dgv3.DataSource;
            if (G1.get_column_number(dt, "Green") < 0)
                dt.Columns.Add("Green");
            string found = "";
            string green = "";
            double inSystem = 0D;
            double inBank = 0D;

            DataRow[] systemDt = dt.Select("found<>'Z InBank'");

            DataRow[] bnkDt = dt.Select("found='Z InBank'");

            for (int i = 0; i < systemDt.Length; i++)
            {
                inSystem = systemDt[i]["amount"].ObjToDouble();
                if (inSystem == 7005.00D)
                {
                }

                for (int j = 0; j < bnkDt.Length; j++)
                {
                    green = bnkDt[j]["Green"].ObjToString();
                    if (green.ToUpper() == "Y")
                        continue;
                    inBank = bnkDt[j]["inSystem"].ObjToDouble();
                    if (inBank == inSystem)
                    {
                        bnkDt[j]["Green"] = "Y";
                        systemDt[i]["Green"] = "Y";
                        break;
                    }
                }
            }

            bnkDt = dt.Select("found='Z InBank'");
            systemDt = dt.Select("found<>'Z InBank'");
            if (systemDt.Length <= 0)
                return;
            DataTable tempDt = systemDt.CopyToDataTable();

            List<double> amounts = new List<double>();
            bool gotit = false;

            for (int i = 0; i < systemDt.Length; i++)
            {
                found = systemDt[i]["Green"].ObjToString().ToUpper();
                if (found != "Y")
                {
                    inSystem = systemDt[i]["amount"].ObjToDouble();
                    amounts.Add(inSystem);
                    gotit = true;
                }
            }

            if ( gotit && amounts.Count > 100)
            {
                MessageBox.Show("***INFO*** Green Combinations may not be accurate\nbecause there are more than " + amounts.Count.ToString() + "\nFactorial Combinations!", "Green Combinations Warning Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            if (gotit && amounts.Count <= 100 )
            {
                List<List<double>> result = new List<List<double>>();

                //GetCombinationMain<long>(amounts, result );
                result = GetAllCombos(amounts);
                if (result == null)
                    return;

                // IOrderedEnumerable<List<double>> sorted = result.OrderByDescending(s => s.Count);

                double total = 0D;
                DataRow[] myMatches = null;
                for (int i = 0; i < bnkDt.Length; i++)
                {
                    inBank = bnkDt[i]["inSystem"].ObjToDouble();
                    found = bnkDt[i]["inSystem"].ObjToString();
                    if (found == "Y")
                        continue;

                    for ( int j=0; j<result.Count; j++)
                    {
                        total = 0D;
                        for (int k = 0; k < result[j].Count; k++)
                            total += result[j][k];
                        if ( total == inBank )
                        {
                            if (bnkDt[i]["Green"].ObjToString() == "Y")
                                continue;
                            bnkDt[i]["Green"] = "Y";
                            for (int k = 0; k < result[j].Count; k++)
                            {
                                total = result[j][k];
                                myMatches = dt.Select("amount='" + total.ToString() + "'");
                                if ( myMatches.Length > 0 )
                                {
                                    for (int l = 0; l < myMatches.Length; l++)
                                    {
                                        found = myMatches[l]["Green"].ObjToString();
                                        if (found != "Y")
                                        {
                                            myMatches[l]["Green"] = "Y";
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        /****************************************************************************************/
        private List<List<double>> GetAllCombos(List<double> list)
        {
            int comboCount = (int)Math.Pow(2, list.Count) - 1;
            List<List<double>> result = new List<List<double>>();
            try
            {
                barImport.Show();
                barImport.Refresh();
                barImport.Minimum = 0;
                barImport.Maximum = comboCount;
                barImport.Value = 0;
                barImport.Refresh();

                if ( comboCount >= 100217727 )
                {
                    DialogResult Dresult = MessageBox.Show("***Warning*** Building Combinations are very large!\nYou may run out of Memory too!\nDo you want to continue anyway?", "Green Combinations Warning Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    if ( Dresult == DialogResult.No )
                    {
                        barImport.Hide();
                        return null;
                    }
                }
                int i = 0;
                //for (int i = 1; i < comboCount + 1; i++)
                for ( ; ; )
                {
                    i++;
                    if (i >= comboCount + 1)
                        break;
                    barImport.Value = i-1;
                    barImport.Refresh();
                    // make each combo here
                    result.Add(new List<double>());
                    if (result.Count > 3000000)
                    {
                        MessageBox.Show("***ERROR*** Building Combinations (" + result.Count.ToString() + ") are more than allowed!\nGreen results may not be accurate!", "Green Combinations Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        break;
                    }
                    for (int j = 0; j < list.Count; j++)
                    {
                        if ((i >> j) % 2 != 0)
                            result.Last().Add(list[j]);
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Building Combinations " + ex.Message.ToString(), "Green Combinations Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            barImport.Value = comboCount;
            barImport.Refresh();
            //barImport.Hide();
            return result;
        }
        /****************************************************************************************/
        private static void GetCombinationMain<T>(List<double> set, List<List<double>> result)
        {
            for (int i = 0; i < set.Count; i++)
            {
                if (i > 20)
                    break;
                List<double> temp = new List<double>(set.Where((s, index) => index != i));

                if (temp.Count > 0 && !result.Where(l => l.Count == temp.Count).Any(l => l.SequenceEqual(temp)))
                {
                    result.Add(temp);

                    GetCombination<T>(temp, result);
                }
            }
        }
        /****************************************************************************************/
        private static void GetCombination<T>(List<double> set, List<List<double>> result )
        {
            for (int i = 0; i < set.Count; i++)
            {
                List<double> temp = new List<double>(set.Where((s, index) => index != i));

                if (temp.Count > 0 && !result.Where(l => l.Count == temp.Count).Any(l => l.SequenceEqual(temp)))
                {
                    result.Add(temp);

                    GetCombination<T>(temp, result );
                }
            }
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("amount", null);
            AddSummaryColumn("debit", null);

            AddSummaryColumn("amount", gridMain3);
            AddSummaryColumn("debit", gridMain3);
            AddSummaryColumn("inSystem", gridMain3);
            AddSummaryColumn("diff", gridMain3);
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
                G1.ShowHideFindPanel(gridMain);
            else if (dgv3.Visible)
                G1.ShowHideFindPanel(gridMain3);
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
            if (workDt != null )
            {
                string column = e.Column.FieldName.ToUpper();
                if (column == "INSYSTEM")
                {
                    DataTable dt = (DataTable)dgv3.DataSource;
                    double debit = dt.Rows[e.ListSourceRowIndex]["debit"].ObjToDouble();
                    if (debit != 0D)
                    {
                        string assignTo = dt.Rows[e.ListSourceRowIndex]["assignTo"].ObjToString();
                        string debitDepNum = dt.Rows[e.ListSourceRowIndex]["debitDepNum"].ObjToString();
                        assignTo += "~" + debitDepNum;
                        if ( assignTo.Trim() != "~" )
                            e.DisplayText = assignTo + "~" + debitDepNum;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                return;

            if ( dgv3.Visible )
            {
                DataRow dr = gridMain3.GetFocusedDataRow();
                string record = dr["record"].ObjToString();
                string amount = dr["amount"].ObjToString();
                string found = dr["found"].ObjToString().ToUpper();
                if ( found != "ADJUSTMENT")
                {
                    MessageBox.Show("***ERROR*** You can only DELETE lines that are previous Adjustments!", "Delete Detail Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
                DialogResult result = MessageBox.Show("***Question*** Are you sure you want to DELETE this Bank Detail for (" + amount + ") ?", "Delete Bank Detail Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                if (result == DialogResult.No)
                    return;

                DataTable dt = (DataTable)dgv3.DataSource;
                int rowHandle = gridMain3.FocusedRowHandle;
                int row = gridMain3.GetDataSourceRowIndex(rowHandle);
                dt.Rows.RemoveAt(row);
                gridMain3.ClearSelection();
                G1.delete_db_table("bank_details", "record", record);

                double diff = 0D;
                double Amount = 0D;
                double systemAmount = 0D;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    Amount = dt.Rows[i]["amount"].ObjToDouble();
                    systemAmount = dt.Rows[i]["inSystem"].ObjToDouble();
                    diff = Amount - systemAmount;
                    dt.Rows[i]["diff"] = diff;
                }
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
            if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;

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

            if ( workDt == null )
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
            string text = "Bank Details Imported Data";
            if ( dgv.Visible )
            {
                Printer.DrawQuad(6, 7, 4, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }
            else if ( dgv3.Visible )
            {
                if ( workDt != null )
                    Printer.DrawQuad(3, 7, 9, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
                else
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
            if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;

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

            if (workDt == null)
                printingSystem1.Document.AutoFitToPagesWidth = 1;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            if ( dgv3.Visible )
            {
                DataRow dr = gridMain3.GetFocusedDataRow();
                DataTable dt = (DataTable)dgv3.DataSource;
                if (dt == null)
                    return;
                if (dt.Rows.Count <= 0 )
                    return;

                string editRecord = "";
                string found = dr["found"].ObjToString();
                string adjustment = "";
                if (found.ToUpper() == "ADJUSTMENT")
                {
                    adjustment = "Y";
                    editRecord = dr["record"].ObjToString();
                }

                DateTime depositDate = dr["date"].ObjToDateTime();
                double credit = dr["amount"].ObjToDouble();
                double debit = dr["debit"].ObjToDouble();
                string bankAccount = dr["bankAccount"].ObjToString();
                string description = dr["depositNumber"].ObjToString();
                if (found.ToUpper() == "Z INBANK")
                {
                    credit = dr["inSystem"].ObjToDouble();
                    description = dr["depositNumber"].ObjToString();
                }
                else
                {
                    if ( adjustment != "Y" )
                    {
                        MessageBox.Show("***ERROR*** You can only edit lines that are previous Adjustments!", "Edit Detail Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        return;
                    }
                }


                using (ManuallyAddBankDeposit askForm = new ManuallyAddBankDeposit( depositDate, credit, debit, bankAccount, description ))
                {
                    string title = "Manually Edit Bank Adjustment";
                    if (found.ToUpper() == "Z INBANK")
                        title = "Manually Add Bank Adjustment";
                    askForm.Text = title;

                    askForm.ShowDialog();
                    if (askForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                    {
                        PleaseWait waitForm = G1.StartWait("Please Wait While Reprocessing!");

                        depositDate = askForm.wDate.ObjToDateTime();
                        credit = askForm.wCredit.ObjToDouble();
                        debit = askForm.wDebit.ObjToDouble();
                        bankAccount = askForm.wBankAccount.ObjToString();
                        description = askForm.wDescription.ObjToString();

                        string cmd = "Delete from `bank_details` WHERE `bankAccount` = '-1';";
                        G1.get_db_data(cmd);

                        string record = "";

                        try
                        {
                            record = editRecord;
                            if ( String.IsNullOrWhiteSpace ( record ))
                                record = G1.create_record("bank_details", "bankAccount", "-1");
                            if (G1.BadRecord("bank_details", record))
                                return;
                            G1.update_db_table("bank_details", "record", record, new string[] { "date", depositDate.ToString("MM/dd/yyyy"), "amount", credit.ToString(), "bankAccount", bankAccount, "debit", debit.ToString(), "description", description, "filename", "Manually Added", "adjustment", "Y" });
                        }
                        catch ( Exception ex)
                        {
                            MessageBox.Show("***ERROR*** Occurred during Add Record " + ex.Message.ToString() + "!!!", "Add Bank Detail Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            return;
                        }

                        DataTable ddt = pullData(workSaveDt, workDate, bankAccount);
                        workDt = ddt;

                        G1.NumberDataTable(workDt);


                        //btnPullPostedData_Click(null, null);
                        this.Cursor = Cursors.WaitCursor;

                        ImportBankDetails_Load(null, null);

                        G1.StopWait(ref waitForm);

                        this.Cursor = Cursors.Default;
                    }
                }
            }
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
        public static DataTable getCSVFile ( string filename )
        {
            DataTable dt = Import.ImportCSVfile(filename, 13 );
            if ( dt != null )
            {
                if ( dt.Rows.Count > 0 )
                {
                    if (G1.get_column_number(dt, "num") >= 0)
                        dt.Columns.Remove("num");
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        public static DataTable getExcelFile( string filename )
        {
            DataTable dt = new DataTable();

            try
            {
                //Create COM Objects. Create a COM object for everything that is referenced
                Excel.Application xlApp = new Excel.Application();
                Excel.Workbook xlWorkbook = xlApp.Workbooks.Open(filename);
                Excel._Worksheet xlWorksheet = xlWorkbook.Sheets[1];
                Excel.Range xlRange = xlWorksheet.UsedRange;

                int rowCount = xlRange.Rows.Count;
                int colCount = xlRange.Columns.Count;

                //iterate over the rows and columns and print to the console as it appears in the file
                //excel is not zero based!!

                for (int i = 1; i <= colCount; i++)
                {
                    dt.Columns.Add("COL " + i.ToString());
                }
                for (int i = 1; i <= rowCount; i++)
                {
                    for (int j = 1; j <= colCount; j++)
                    {
                        //new line
                        if (j == 1)
                        {
                            DataRow dr = dt.NewRow();
                            dt.Rows.Add(dr);
                            //Console.Write("\r\n");
                        }

                        //write the value to the console
                        if (xlRange.Cells[i, j] != null && xlRange.Cells[i, j].Value2 != null)
                        {
                            dt.Rows[i - 1][j - 1] = xlRange.Cells[i, j].Value2.ToString();
                            //Console.Write(xlRange.Cells[i, j].Value2.ToString() + "\t");
                        }
                    }
                }

                //cleanup
                GC.Collect();
                GC.WaitForPendingFinalizers();

                //rule of thumb for releasing com objects:
                //  never use two dots, all COM objects must be referenced and released individually
                //  ex: [somthing].[something].[something] is bad

                //release com objects to fully kill excel process from running in the background
                Marshal.ReleaseComObject(xlRange);
                Marshal.ReleaseComObject(xlWorksheet);

                //close and release
                xlWorkbook.Close();
                Marshal.ReleaseComObject(xlWorkbook);

                //quit and release
                xlApp.Quit();
                Marshal.ReleaseComObject(xlApp);
            }
            finally
            {
            }

            return dt;
        }

        /***********************************************************************************************/
        private string importFolder = "";
        private DataTable SelectFiles ()
        {
            DataTable dx = null;
            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            // Always default to Folder Selection.
            folderBrowser.FileName = "Folder Selection.";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                string folderPath = Path.GetDirectoryName(folderBrowser.FileName);
                string lines = "Entire Directory\nOnly the File";
                using (SelectFromList listForm = new SelectFromList(lines, false))
                {
                    listForm.Text = "Select Entire Directory to choose files from or only the file";
                    //listForm.ListDone += ListForm_PictureDone;
                    listForm.ShowDialog();
                    string what = SelectFromList.theseSelections;
                    if (String.IsNullOrWhiteSpace(what))
                        return dx;
                    if ( what == "Only the File")
                    {
                        importFolder = folderPath;
                        SelectForm_ListDone(folderBrowser.FileName);
                        return dx;
                    }
                }
                string[] files = Directory.GetFiles(folderPath, "*.csv");
                if (files.Length <= 0)
                    MessageBox.Show("No Files found!!!!", "Files Found Dialog");
                else
                {
                    importFolder = folderPath;
                    dx = LoadFileList(folderPath, files);
                    string selections = "";
                    for ( int i=0; i<dx.Rows.Count; i++)
                    {
                        selections += dx.Rows[i]["filename"].ObjToString() + "\n";
                    }
                    SelectFromList selectForm = new SelectFromList(selections, true);
                    selectForm.ListDone += SelectForm_ListDone;
                    selectForm.ShowDialog();
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        private void SelectForm_ListDone(string s)
        {
            string[] Lines = s.Split('\n');
            int Count = Lines.Length;
            if (Count == 0)
                return;

            string filename = "";
            FileInfo file = null;
            string extension = "";
            string firstWord = "";
            DataTable dt = null;
            DataTable mainDt = null;
            string[] moreLines = null;
            for ( int i=0; i<Lines.Length; i++)
            {
                filename = Lines[i].Trim();

                file = new FileInfo(filename);
                extension = file.Extension.Trim().ToUpper();


                moreLines = file.Name.Split(' ');
                if (moreLines.Length > 0)
                    firstWord = moreLines[0].Trim();

                if (extension == ".CSV")
                {
                    try
                    {
                        string accountNumber = "";
                        dt = getCSVFile(filename);

                        dt = CleanupDataTable(dt);

                        dt = ProcessTable(file.Name, dt, firstWord, ref accountNumber);
                        if (dt != null)
                        {
                            if (mainDt == null)
                                mainDt = dt.Copy();
                            else
                            {
                                for (int j = 0; j < dt.Rows.Count; j++)
                                    mainDt.ImportRow(dt.Rows[j]);
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        MessageBox.Show("***ERROR*** Fatal Error " + ex.Message.ToUpper() + "!!!", "Import Bank Detail Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    }
                }
                else
                {
                }
            }
            dgv.DataSource = mainDt;
            btnSaveNotPosted.Visible = true;
            btnSaveNotPosted.Refresh();
        }
        /***********************************************************************************************/
        private DataTable LoadFileList(string filePath, string[] files)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("filename");
            DataRow dRow = null;
            string filename = "";
            int lastrow = files.Length;

            bool rv = false;

            for (int i = 0; i < files.Length; i++)
            {
                filename = files[i].Trim();
                dRow = dt.NewRow();
                dRow["filename"] = filename;
                dt.Rows.Add(dRow);
            }
            G1.NumberDataTable(dt);
            return dt;
        }
        /***********************************************************************************************/
        private void btnPullFile_Click(object sender, EventArgs e)
        {
            pictureDelete.Show();
            pictureDelete.Refresh();

            DataTable filesDt = SelectFiles();
            if (1 == 1)
                return;

            DataTable dt = null;
            string str = "";
            this.Cursor = Cursors.WaitCursor;
            string firstWord = "";
            string[] Lines = null;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = ofd.FileName;

                    this.Cursor = Cursors.WaitCursor;

                    int idx = filename.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = filename.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }

                    FileInfo file = new FileInfo(filename);
                    string extension = file.Extension.Trim().ToUpper();

                    Lines = file.Name.Split(' ');
                    if (Lines.Length > 0)
                        firstWord = Lines[0].Trim();

                    if (extension == ".CSV")
                    {
                        string accountNumber = "";
                        dt = getCSVFile(filename);

                        dt = CleanupDataTable(dt);

                        dt = ProcessTable( file.Name, dt, firstWord, ref accountNumber );
                        if ( dt != null )
                            dgv.DataSource = dt;
                    }
                    else
                    {
                    }


                    //DataTable newDt = buildActualImportTable(dt);
                    //if (newDt != null)
                    //    dgv.DataSource = newDt;

                    this.Cursor = Cursors.Default;
                }
            }
            this.Cursor = Cursors.Default;
            btnSaveNotPosted.Show();
        }
        /***********************************************************************************************/
        private DataTable CleanupDataTable ( DataTable dt)
        {
            if (G1.get_column_number(dt, "COL 3") < 0)
                return dt;

            string data = "";
            int i = 0;
            int j = 0;
            string name = "";
            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    data = dt.Rows[i]["COL 3"].ObjToString();
                    if (String.IsNullOrWhiteSpace(data))
                        dt.Rows[i][0] = "-1";
                    else
                    {
                        for (j = 0; j < dt.Columns.Count; j++)
                        {
                            name = dt.Rows[i][j].ObjToString();
                            if ( !String.IsNullOrWhiteSpace ( name ))
                                dt.Columns[j].ColumnName = name;
                        }
                        dt.Rows[i][0] = "-1";
                        break;
                    }
                }
            }
            catch ( Exception ex)
            {
            }

            try
            {
                for (i = dt.Rows.Count - 1; i >= 0; i--)
                {
                    data = dt.Rows[i][0].ObjToString();
                    if (data == "-1")
                    {
                        dt.Rows.RemoveAt(i);
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable ProcessTable ( string filename, DataTable dt, string firstWord, ref string accountNumber )
        {
            DataTable dx = new DataTable();
            dx.Columns.Add("date");
            dx.Columns.Add("amount", Type.GetType("System.Double"));
            dx.Columns.Add("debit", Type.GetType("System.Double"));
            dx.Columns.Add("description" );
            dx.Columns.Add("bankAccount");
            dx.Columns.Add("status");
            dx.Columns.Add("filename");

            DataRow dRow = null;
            string data = "";
            string str = "";
            double credit = 0D;
            double debit = 0D;
            string debitCredit = "";
            string description = "";
            string checkNumber = "";
            string status = "";
            accountNumber = "";
            int idx = 0;
            DateTime date = DateTime.Now;
            string cmd = "";
            DataTable bankDt = null;
            string[] Lines = null;
            int length = 0;
            int start = 0;
            string search = "";

            if (dt.Rows.Count <= 0)
                return dt;

            if (String.IsNullOrWhiteSpace(firstWord))
                return dt;

            cmd = "Select * from `bank_accounts` WHERE `account_no` LIKE '%" + firstWord + "';";
            bankDt = G1.get_db_data(cmd);
            if (bankDt.Rows.Count <= 0)
                return dt;

            accountNumber = bankDt.Rows[0]["account_no"].ObjToString();

            int amountCol = -1;
            int debitCol = -1;
            int dateCol = -1;
            int debitOrCredit = -1;
            int descCol = -1;
            int statusCol = -1;
            int checkNumberCol = -1;
            string DC = "";
            //dt.Columns.Add("DebitOrCredit");

            for ( int i=0; i<dt.Columns.Count; i++)
            {
                data = dt.Columns[i].ColumnName.Trim().ToUpper().Trim();
                if (data.IndexOf("DATE") >= 0)
                    dateCol = i;
                else
                {
                    if (data.IndexOf("AMOUNT") >= 0)
                    {
                        if (data.IndexOf("AMOUNT CREDIT") >= 0)
                            amountCol = i;
                        else if (data.IndexOf("CREDIT AMOUNT") >= 0)
                            amountCol = i;
                        else if (data.IndexOf("DEBIT AMOUNT") >= 0)
                            debitCol = i;
                        else if (data.IndexOf("AMOUNT DEBIT") >= 0)
                            debitCol = i;
                        else
                            amountCol = i;
                    }
                    else if (data.IndexOf("STATUS") >= 0)
                        statusCol = i;
                    else if (data.IndexOf("DEBITORCREDIT") >= 0)
                        debitOrCredit = i;
                    else if (data.IndexOf("CREDIT OR DEBIT") >= 0)
                        debitOrCredit = i;
                    else if (data.IndexOf("CR/DR") >= 0)
                        debitOrCredit = i;
                    else if (data.IndexOf("DEBIT") >= 0)
                        debitCol = i;
                    else if (data.IndexOf("CREDIT") >= 0)
                        amountCol = i;
                    else if (data == "DESCRIPTION")
                        descCol = i;
                    else if (data == "CHECK NUMBER")
                        checkNumberCol = i;
                }
            }

            if ( dateCol < 0 )
            {
                MessageBox.Show( "***ERROR*** Cannot locate Date Column in file " + "!!!", "Bank Detail Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return null;
            }
            else if ( debitOrCredit == -1 && amountCol == -1 )
            {
                MessageBox.Show("***ERROR*** Cannot locate Amount Column in file " + "!!!", "Bank Detail Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return null;
            }
            else if ( amountCol == -1 )
            {
                MessageBox.Show("***ERROR*** Cannot locate Amount Column in file " + "!!!", "Bank Detail Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return null;
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    debit = 0D;
                    credit = 0D;
                    debitCredit = "";
                    description = "";
                    checkNumber = "";
                    status = "";
                    data = dt.Rows[i][dateCol].ObjToString();
                    if (String.IsNullOrWhiteSpace(data))
                        continue;
                    date = data.ObjToDateTime();

                    if (descCol > 0)
                        description = dt.Rows[i][descCol].ObjToString();
                    if (debitOrCredit >= 0)
                        debitCredit = dt.Rows[i][debitOrCredit].ObjToString().Trim().ToUpper();
                    data = dt.Rows[i][amountCol].ObjToString();
                    data = data.Replace("$", "");
                    if ( data.IndexOf ( "(") >= 0 )
                    {
                        if ( data.IndexOf ( ")" ) >= 0 )
                        {
                            data = data.Replace("(", "");
                            data = data.Replace(")", "");
                            credit = data.ObjToDouble();
                            credit = credit * -1D;
                        }
                    }
                    else
                        credit = data.ObjToDouble();

                    if (statusCol >= 0)
                        status = dt.Rows[i][statusCol].ObjToString();
                    if (checkNumberCol >= 0)
                        checkNumber = dt.Rows[i][checkNumberCol].ObjToString();

                    if ( debitCol >= 0 )
                    {
                        data = dt.Rows[i][debitCol].ObjToString();
                        data = data.Replace("$", "");
                        debit = data.ObjToDouble();
                        debit = Math.Abs(debit);
                    }
                    if (credit != 0D || debit != 0D )
                    {
                        dRow = dx.NewRow();
                        dRow["date"] = date.ToString("MM/dd/yyyy");
                        if (credit < 0D)
                            dRow["debit"] = Math.Abs(credit);
                        else
                        {
                            if (!String.IsNullOrWhiteSpace(debitCredit))
                            {
                                if (debitCredit == "DEBIT" || debitCredit.ToUpper() == "DR" )
                                    dRow["debit"] = Math.Abs(credit);
                                else
                                    dRow["amount"] = Math.Abs(credit);
                            }
                            else
                            {
                                dRow["amount"] = credit;
                                dRow["debit"] = debit;
                            }
                        }
                        if (!String.IsNullOrWhiteSpace(checkNumber))
                            description += " " + checkNumber;
                        dRow["description"] = description;
                        dRow["bankAccount"] = accountNumber;
                        dRow["filename"] = filename;
                        dRow["status"] = status;
                        dx.Rows.Add(dRow);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return dx;


            //data = dt.Columns[0].ColumnName.Trim();
            //if ( data.ToUpper() == "ACCOUNT NAME" )
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        data = dt.Rows[i]["Account Name"].ObjToString().ToUpper();
            //        if ( String.IsNullOrWhiteSpace ( accountNumber) )
            //        {
            //            if (data == "FUNERAL CREDIT CARD")
            //            {
            //                Lines = funeralBankCC.Split('~');
            //                if (Lines.Length >= 3)
            //                    accountNumber = Lines[2];
            //            }
            //            else if (data.IndexOf("LOCK BOX") >= 0)
            //            {
            //                data = data.Replace("LOCK BOX", "").Trim();
            //                cmd = "Select * from `bank_accounts` WHERE `account_no` LIKE '%" + data + "';";
            //                bankDt = G1.get_db_data(cmd);
            //                if (bankDt.Rows.Count > 0)
            //                    accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
            //            }
            //            else if (data.IndexOf("REMOTE ACCT") >= 0)
            //            {
            //                data = data.Replace("REMOTE ACCT", "").Trim();
            //                cmd = "Select * from `bank_accounts` WHERE `account_no` LIKE '%" + data + "';";
            //                bankDt = G1.get_db_data(cmd);
            //                if (bankDt.Rows.Count > 0)
            //                    accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
            //            }
            //        }
            //        date = dt.Rows[i]["Processed Date"].ObjToDateTime();
            //        debitCredit = dt.Rows[i]["Credit or Debit"].ObjToString();
            //        if (debitCredit.ToUpper() == "CREDIT")
            //        {
            //            amount = dt.Rows[i]["Amount"].ObjToDouble();
            //            dRow = dx.NewRow();
            //            dRow["date"] = date.ToString("MM/dd/yyyy");
            //            dRow["amount"] = amount;
            //            dRow["bankAccount"] = accountNumber;
            //            dx.Rows.Add(dRow);
            //        }
            //    }
            //}
            //else
            //{
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        str = dt.Rows[i][0].ObjToString();
            //        if ( str.ToUpper().IndexOf ( "ACCOUNT NUMBER") >= 0 )
            //        {
            //            idx = str.IndexOf(":");
            //            if (idx > 0)
            //            {
            //                data = str.Substring(idx + 1);
            //                length = data.Length;
            //                start = length - 4;
            //                if (start < 0)
            //                    continue;
            //                search = data.Substring(start, 4);
            //                cmd = "Select * from `bank_accounts` WHERE `account_no` LIKE '%" + search + "';";
            //                bankDt = G1.get_db_data(cmd);
            //                if (bankDt.Rows.Count > 0)
            //                    accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
            //            }
            //        }
            //        else if ( str.ToUpper().IndexOf ( "DATE RANGE") >= 0 )
            //        {

            //        }
            //        if (str.ToUpper() == "TRANSACTION NUMBER")
            //        {
            //            for (int j = 0; j < dt.Columns.Count; j++)
            //            {
            //                str = dt.Rows[i][j].ObjToString();
            //                str = str.Replace(" ", "");
            //                dt.Rows[i][j] = str;
            //                dt.Columns[j].ColumnName = str;
            //            }
            //        }
            //    }
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        try
            //        {
            //            data = dt.Rows[i]["Date"].ObjToString();
            //            if (String.IsNullOrWhiteSpace(data))
            //                continue;
            //            date = dt.Rows[i]["Date"].ObjToDateTime();
            //            //debitCredit = dt.Rows[i]["Credit or Debit"].ObjToString();
            //            data = dt.Rows[i]["AmountCredit"].ObjToString();
            //            if (String.IsNullOrWhiteSpace(data))
            //                continue;
            //            amount = dt.Rows[i]["AmountCredit"].ObjToDouble();
            //            if (amount > 0D)
            //            {
            //                dRow = dx.NewRow();
            //                dRow["date"] = date.ToString("MM/dd/yyyy");
            //                dRow["amount"] = amount;
            //                dRow["bankAccount"] = accountNumber;
            //                dx.Rows.Add(dRow);
            //            }
            //        }
            //        catch ( Exception ex)
            //        {
            //        }
            //    }
            //}
            //return dx;
        }
        /***********************************************************************************************/
        private DataTable ProcessTablex(DataTable dt, string firstWord, ref string accountNumber)
        {
            DataTable dx = new DataTable();
            dx.Columns.Add("date");
            dx.Columns.Add("amount", Type.GetType("System.Double"));
            dx.Columns.Add("bankAccount");

            DataRow dRow = null;
            string data = "";
            string str = "";
            double amount = 0D;
            string debitCredit = "";
            accountNumber = "";
            int idx = 0;
            DateTime date = DateTime.Now;
            string cmd = "";
            DataTable bankDt = null;
            string[] Lines = null;
            int length = 0;
            int start = 0;
            string search = "";

            if (dt.Rows.Count <= 0)
                return dt;

            if (String.IsNullOrWhiteSpace(firstWord))
                return dt;

            cmd = "Select * from `bank_accounts` WHERE `account_no` LIKE '%" + firstWord + "';";
            bankDt = G1.get_db_data(cmd);
            if (bankDt.Rows.Count <= 0)
                return dt;

            accountNumber = bankDt.Rows[0]["account_no"].ObjToString();

            data = dt.Columns[0].ColumnName.Trim();
            if (data.ToUpper() == "ACCOUNT NAME")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = dt.Rows[i]["Account Name"].ObjToString().ToUpper();
                    if (String.IsNullOrWhiteSpace(accountNumber))
                    {
                        if (data == "FUNERAL CREDIT CARD")
                        {
                            Lines = funeralBankCC.Split('~');
                            if (Lines.Length >= 3)
                                accountNumber = Lines[2];
                        }
                        else if (data.IndexOf("LOCK BOX") >= 0)
                        {
                            data = data.Replace("LOCK BOX", "").Trim();
                            cmd = "Select * from `bank_accounts` WHERE `account_no` LIKE '%" + data + "';";
                            bankDt = G1.get_db_data(cmd);
                            if (bankDt.Rows.Count > 0)
                                accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
                        }
                        else if (data.IndexOf("REMOTE ACCT") >= 0)
                        {
                            data = data.Replace("REMOTE ACCT", "").Trim();
                            cmd = "Select * from `bank_accounts` WHERE `account_no` LIKE '%" + data + "';";
                            bankDt = G1.get_db_data(cmd);
                            if (bankDt.Rows.Count > 0)
                                accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
                        }
                    }
                    date = dt.Rows[i]["Processed Date"].ObjToDateTime();
                    debitCredit = dt.Rows[i]["Credit or Debit"].ObjToString();
                    if (debitCredit.ToUpper() == "CREDIT")
                    {
                        amount = dt.Rows[i]["Amount"].ObjToDouble();
                        dRow = dx.NewRow();
                        dRow["date"] = date.ToString("MM/dd/yyyy");
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = accountNumber;
                        dx.Rows.Add(dRow);
                    }
                }
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    str = dt.Rows[i][0].ObjToString();
                    if (str.ToUpper().IndexOf("ACCOUNT NUMBER") >= 0)
                    {
                        idx = str.IndexOf(":");
                        if (idx > 0)
                        {
                            data = str.Substring(idx + 1);
                            length = data.Length;
                            start = length - 4;
                            if (start < 0)
                                continue;
                            search = data.Substring(start, 4);
                            cmd = "Select * from `bank_accounts` WHERE `account_no` LIKE '%" + search + "';";
                            bankDt = G1.get_db_data(cmd);
                            if (bankDt.Rows.Count > 0)
                                accountNumber = bankDt.Rows[0]["account_no"].ObjToString();
                        }
                    }
                    else if (str.ToUpper().IndexOf("DATE RANGE") >= 0)
                    {

                    }
                    if (str.ToUpper() == "TRANSACTION NUMBER")
                    {
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            str = dt.Rows[i][j].ObjToString();
                            str = str.Replace(" ", "");
                            dt.Rows[i][j] = str;
                            dt.Columns[j].ColumnName = str;
                        }
                    }
                }
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    try
                    {
                        data = dt.Rows[i]["Date"].ObjToString();
                        if (String.IsNullOrWhiteSpace(data))
                            continue;
                        date = dt.Rows[i]["Date"].ObjToDateTime();
                        //debitCredit = dt.Rows[i]["Credit or Debit"].ObjToString();
                        data = dt.Rows[i]["AmountCredit"].ObjToString();
                        if (String.IsNullOrWhiteSpace(data))
                            continue;
                        amount = dt.Rows[i]["AmountCredit"].ObjToDouble();
                        if (amount > 0D)
                        {
                            dRow = dx.NewRow();
                            dRow["date"] = date.ToString("MM/dd/yyyy");
                            dRow["amount"] = amount;
                            dRow["bankAccount"] = accountNumber;
                            dx.Rows.Add(dRow);
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        private string actualFile = "";
        /***********************************************************************************************/
        private void btnPullFile_ClickX(object sender, EventArgs e)
        {
            DataTable ddt = null;
            this.Cursor = Cursors.WaitCursor;
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = ofd.FileName;

                    ddt = getExcelFile(filename);
                    int idx = filename.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = filename.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }
                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    DataTable dt = new DataTable();
                    dt.Columns.Add("referenceNumber");
                    dt.Columns.Add("transactionDate");
                    dt.Columns.Add("transactionType");
                    dt.Columns.Add("cardNumber");
                    dt.Columns.Add("paymentType");
                    dt.Columns.Add("authorizedAmount", Type.GetType("System.Double"));
                    dt.Columns.Add("transactionAmount", Type.GetType("System.Double"));
                    dt.Columns.Add("returnVoid");
                    dt.Columns.Add("amount", Type.GetType("System.Double"));
                    dt.Columns.Add("salesTax", Type.GetType("System.Double"));
                    dt.Columns.Add("fee", Type.GetType("System.Double"));
                    dt.Columns.Add("surCharge", Type.GetType("System.Double"));
                    dt.Columns.Add("trustFuneral");
                    dt.Columns.Add("invoiceNumber");
                    try
                    {
                        if (!File.Exists(filename))
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("***ERROR*** File does not exist!");
                            return;
                        }
                        try
                        {
                            bool first = true;
                            string line = "";
                            int row = 0;
                            string delimiter = ",";
                            char cDelimiter = (char)delimiter[0];
                            string transactionType = "";
                            string cardNumber = "";
                            string invoiceNumber = "";

                            double dValue = 0D;
                            string referenceNumber = "";


                            FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            using (StreamReader sr = new StreamReader(fs))
                            {
                                while ((line = sr.ReadLine()) != null)
                                {
                                    Application.DoEvents();
                                    if (line.ToUpper().IndexOf("REPORT NAME") == 0)
                                        continue;
                                    if (line.IndexOf("$") > 0)
                                        line = preprocessLine(line);
                                    string[] Lines = line.Split(cDelimiter);
                                    G1.parse_answer_data(line, delimiter);
                                    int count = G1.of_ans_count;
                                    cardNumber = Lines[4].ObjToString();
                                    cardNumber = cardNumber.Replace("*", "");
                                    if (cardNumber == "Card Number")
                                        continue;
                                    if (string.IsNullOrWhiteSpace(cardNumber))
                                        continue;

                                    transactionType = Lines[3].ObjToString().Trim();
                                    transactionType = transactionType.Replace("Credit Card Sale", "");
                                    transactionType = transactionType.Trim();

                                    if (transactionType.ToUpper().IndexOf("VOID") >= 0)
                                        transactionType = "Void";

                                    DataRow dRow = dt.NewRow();
                                    dRow["cardNumber"] = cardNumber;
                                    dRow["transactionType"] = transactionType;
                                    dRow["transactionDate"] = Lines[0];
                                    dRow["paymentType"] = Lines[5].ObjToString().Trim();
                                    dRow["authorizedAmount"] = cleanupMoney(Lines[6].ObjToString());
                                    dRow["transactionAmount"] = cleanupMoney(Lines[7].ObjToString());
                                    dRow["returnVoid"] = Lines[8].ObjToString().Trim();
                                    dRow["amount"] = cleanupMoney(Lines[9].ObjToString());
                                    dRow["salesTax"] = cleanupMoney(Lines[10].ObjToString());
                                    dRow["fee"] = cleanupMoney(Lines[11].ObjToString());
                                    dRow["surCharge"] = cleanupMoney(Lines[12].ObjToString());
                                    invoiceNumber = Lines[14].ObjToString().Trim();
                                    dRow["invoiceNumber"] = invoiceNumber;
                                    dRow["trustFuneral"] = categorizeInvoiceNumber( ref invoiceNumber);
                                    dRow["invoiceNumber"] = invoiceNumber;

                                    referenceNumber = Lines[15].ObjToString().Trim();
                                    dValue = Double.Parse(referenceNumber, System.Globalization.NumberStyles.Float);
                                    dRow["referenceNumber"] = dValue.ToString();
                                    dt.Rows.Add(dRow);
                                }
                                row++;
                                sr.Close();
                                dgv.DataSource = dt;
                            }
                        }
                        catch (Exception ex)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
                        }
                        G1.NumberDataTable(dt);
                    }
                    catch (Exception ex)
                    {
                    }
                    this.Cursor = Cursors.Default;
                    if (dt != null && dt.Rows.Count > 0)
                    {
                    }
                }
            }
            this.Cursor = Cursors.Default;
            btnSaveNotPosted.Show();
        }
        /***********************************************************************************************/
        private string categorizeInvoiceNumber ( ref string invoiceNumber )
        {
            string rtn = "Trust";
            string[] Lines = invoiceNumber.Split(' ');
            if (Lines.Length <= 0)
                return rtn;
            string what = Lines[0].Trim();
            what = what.Replace("-", "");
            string cmd = "Select * from `contracts` where `contractNumber` = '" + what + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 )
            {
                cmd = "Select * from `fcust_extended` where `serviceId` = '" + what + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    rtn = "Funeral";
                else
                {
                    rtn = "N/A";
                    Lines = invoiceNumber.Split(' ');
                    if (Lines.Length <= 0)
                        return rtn;
                    what = Lines[0].Trim();
                    cmd = "Select * from `icustomers` where `payer` = '" + what + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        rtn = "Insurance";
                    else
                    {
                        rtn = "N/A";
                        if (invoiceNumber.IndexOf("Down Pmt") >= 0)
                            rtn = "DownPmt";

                        invoiceNumber = invoiceNumber.Replace("Trst Down Pmt", "").Trim();
                        invoiceNumber = invoiceNumber.Replace("Trust Down Pmt", "").Trim();
                        invoiceNumber = invoiceNumber.Replace("/", " ");

                        Lines = invoiceNumber.Split(' ');
                        if ( Lines.Length >= 2 )
                        {
                            string fname = Lines[0];
                            string lname = Lines[1];
                            if (!String.IsNullOrWhiteSpace(fname) && !String.IsNullOrWhiteSpace(lname))
                            {
                                cmd = "Select * from `downpayments` where `lastName` = '" + lname + "' AND `firstName` = '" + fname + "' LIMIT 10;";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count > 0)
                                    rtn = "DownPmt";
                            }
                        }
                    }
                }
            }
            return rtn;
        }
        /***********************************************************************************************/
        private double cleanupMoney ( string str)
        {
            str = str.Trim();
            str = str.Replace("\"", "");
            str = str.Replace("$", "");
            double dValue = str.ObjToDouble();
            return dValue;
        }
        /***********************************************************************************************/
        private string preprocessLine ( string line )
        {
            string newStr = "";
            string str = "";
            bool started = false;
            for ( int i=0; i<line.Length; i++)
            {
                str = line.Substring(i, 1);
                if (started)
                {
                    if (str == ",")
                        continue;
                    if (str == ".")
                        started = false;
                    newStr += str;
                    continue;
                }
                if (str == "$")
                {
                    started = true;
                    continue;
                }
                else
                    newStr += str;
            }
            return newStr;
        }
        /***********************************************************************************************/
        private void btnSaveNotPosted_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            DateTime date = DateTime.Now;
            double amount = 0D;
            double debit = 0D;
            string bankAccount = "";
            string filename = "";
            string description = "";

            string cmd = "Delete from `bank_details` WHERE `bankAccount` = '-1';";
            G1.get_db_data(cmd);

            DataTable dx = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    amount = dt.Rows[i]["amount"].ObjToDouble();
                    debit = dt.Rows[i]["debit"].ObjToDouble();
                    bankAccount = dt.Rows[i]["bankAccount"].ObjToString();
                    filename = dt.Rows[i]["filename"].ObjToString();
                    description = dt.Rows[i]["description"].ObjToString();

                    record = G1.create_record("bank_details", "bankAccount", "-1");
                    if (G1.BadRecord("bank_details", record))
                        continue;
                    G1.update_db_table("bank_details", "record", record, new string[] { "date", date.ToString("MM/dd/yyyy"), "amount", amount.ToString(), "bankAccount", bankAccount, "debit", debit.ToString(), "description", description, "filename", filename });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("*** ERROR *** Importing Bank Detail : " + ex.Message.ToString() + "!!!", "Bank Detail Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            btnSaveNotPosted.Hide();
            btnSaveNotPosted.Refresh();

            string oldFilename = "";

            if (!String.IsNullOrWhiteSpace(importFolder))
            {
                string newDirectory = importFolder + "/backups";
                if (!Directory.Exists(newDirectory))
                    Directory.CreateDirectory(newDirectory);
                string fullPath = "";
                string newPath = "";
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        filename = dt.Rows[i]["filename"].ObjToString();
                        if (filename == oldFilename)
                            continue;
                        fullPath = importFolder + "/" + filename;

                        newPath = newDirectory + "/" + filename;
                        File.Copy(fullPath, newPath, true);
                        oldFilename = filename;
                        File.Delete(fullPath);
                    }
                }
                catch ( Exception ex)
                {
                }
                dt.Rows.Clear();
                dgv.DataSource = dt;
                dgv.RefreshDataSource();
            }
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
        /***********************************************************************************************/
        private string decodeBankAccont ( string bankAccount)
        {
            string account = "";
            string [] Lines = bankAccount.Split('~');
            if (Lines.Length > 2)
            {
                string localDescription = Lines[0];
                account = Lines[2];
            }
            return account;
        }
        /***********************************************************************************************/
        DataTable bankDt = null;
        private void LoadCashBankAccounts ()
        {
            if (bankDt != null)
                return;
            string cmd = "Select * from `bank_accounts` WHERE `account_title` LIKE 'CASH%'";
            bankDt = G1.get_db_data(cmd);
        }
        /***********************************************************************************************/
        private DataTable LoadMainData ( ref DataTable adjustDt )
        {
            LoadCashBankAccounts();

            DataTable dt = null;

            try
            {
                string cmd = "Select * from `bank_details` WHERE `bankAccount` = 'XYZZYX';";
                DataTable maindt = G1.get_db_data(cmd);
                maindt.Columns.Add("bankDetails");
                maindt.Columns.Add("found");
                maindt.Columns.Add("ID");
                maindt.Columns.Add("sDate");
                maindt.Columns.Add("inSystem", Type.GetType("System.Double"));
                maindt.Columns.Add("diff", Type.GetType("System.Double"));
                maindt.Columns.Add("depositNumber");

                DataRow[] dRows = null;
                DataRow dRow = null;

                DateTime date = DateTime.Now;
                string sdate1 = date.ToString("yyyy-MM-dd 00:00:00");
                string sdate2 = sdate1;
                if (chkHonorDates.Checked)
                {
                    date = this.dateTimePicker2.Value;
                    sdate1 = date.ToString("yyyy-MM-dd 00:00:00");

                    date = this.dateTimePicker3.Value;
                    sdate2 = date.ToString("yyyy-MM-dd 23:59:59");
                    cmd = "Select * from `bank_details` WHERE `date` >= '" + sdate1 + "' AND `date` <= '" + sdate2 + "';";
                }

                dt = G1.get_db_data(cmd);

                dt.Columns.Add("bankDetails");
                dt.Columns.Add("found");
                dt.Columns.Add("ID");
                dt.Columns.Add("sDate");
                dt.Columns.Add("inSystem", Type.GetType("System.Double"));
                dt.Columns.Add("diff", Type.GetType("System.Double"));
                dt.Columns.Add("depositNumber");

                DateTime date1 = sdate1.ObjToDateTime();
                DateTime date2 = sdate2.ObjToDateTime();
                DateTime testDate = date1;

                bool exclude = this.chkExclude.Checked;

                for (; ; )
                {
                    if (testDate >= date2)
                        break;
                    if (exclude)
                    {
                        if (testDate.DayOfWeek == DayOfWeek.Sunday || testDate.DayOfWeek == DayOfWeek.Saturday)
                        {
                            testDate = testDate.AddDays(1);
                            continue;
                        }
                    }

                    for (int i = 0; i < bankDt.Rows.Count; i++)
                    {
                        dRow = maindt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(testDate);
                        dRow["bankAccount"] = bankDt.Rows[i]["account_no"].ObjToString();
                        dRow["sDate"] = testDate.ToString("yyyyMMdd");
                        maindt.Rows.Add(dRow);
                    }
                    testDate = testDate.AddDays(1);
                }

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    dt.Rows[i]["sDate"] = date.ToString("yyyyMMdd");
                }

                string account = "";
                string bankDetails = "";
                string sDate = "";
                double amount = 0D;
                double debit = 0D;

                adjustDt = maindt.Clone();

                for (int i = 0; i < maindt.Rows.Count; i++)
                {
                    date = maindt.Rows[i]["date"].ObjToDateTime();
                    sDate = date.ToString("yyyyMMdd");
                    account = maindt.Rows[i]["bankAccount"].ObjToString();
                    if (String.IsNullOrWhiteSpace(account))
                        continue;
                    //if (String.IsNullOrWhiteSpace(account))
                    //{
                    //    dt.Rows[i]["bankAccount"] = "No Bank Account Listed";
                    //    continue;
                    //}
                    dRows = bankDt.Select("account_no='" + account + "'");
                    if (dRows.Length > 0)
                        maindt.Rows[i]["bankDetails"] = dRows[0]["account_title"].ObjToString() + " " + account;


                    dRows = dt.Select("sDate='" + sDate + "' AND bankAccount='" + account + "' AND adjustment<>'Y'");
                    amount = 0D;
                    debit = 0D;
                    for (int j = 0; j < dRows.Length; j++)
                    {
                        amount += dRows[j]["amount"].ObjToDouble();
                        debit += dRows[j]["debit"].ObjToDouble();
                    }

                    maindt.Rows[i]["amount"] = amount;
                    maindt.Rows[i]["debit"] = debit;
                    maindt.Rows[i]["inSystem"] = 0D;
                    maindt.Rows[i]["sDate"] = sDate;
                }

                //dRows = dx.Select("sDate='" + sDate + "' AND adjustment = 'Y'");
                dRows = dt.Select("adjustment = 'Y'");
                if (dRows.Length > 0)
                    adjustDt = dRows.CopyToDataTable();

                dt = maindt.Copy();
            }
            catch ( Exception ex)
            {
            }

            return dt;
        }
        /***********************************************************************************************/
        private void btnPullPostedData_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            btnShowDateRange.Show();
            btnShowDaySpan.Show();
            btnCurrentDay.Show();
            //btnFindDifference.Show();

            DataTable adjustDt = null;

            DataTable dt = LoadMainData( ref adjustDt );

            DataRow[] dRows = null;

            DateTime date = DateTime.Now;
            string sdate1 = date.ToString("yyyy-MM-dd 00:00:00");
            string sdate2 = "";
            string cmd = "Select * from `bank_details`;";
            if (chkHonorDates.Checked)
            {
                date = this.dateTimePicker2.Value;
                sdate1 = date.ToString("yyyy-MM-dd 00:00:00");

                date = this.dateTimePicker3.Value;
                sdate2 = date.ToString("yyyy-MM-dd 23:59:59");
            }

            DateTime date1 = DateTime.MaxValue;
            DateTime date2 = DateTime.MinValue;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["date"].ObjToDateTime();
                if (date < date1)
                    date1 = date;
                if (date > date2)
                    date2 = date;
            }



            //            DataTable dt = G1.get_db_data(cmd);
            //            dt.Columns.Add("bankDetails");
            //            dt.Columns.Add("found");
            //            dt.Columns.Add("ID");
            //            dt.Columns.Add("sDate");
            //            dt.Columns.Add("inSystem", Type.GetType("System.Double"));
            //            dt.Columns.Add("diff", Type.GetType("System.Double"));
            //            dt.Columns.Add("depositNumber");

            //            dRows = dt.Select("bankAccount=''");
            //            if ( dRows.Length > 0 )
            //            {
            //                for (int i = 0; i < dRows.Length; i++)
            //                    dRows[i]["bankAccount"] = "No Bank Account Listed";
            //            }

            //            DataTable dx = dt.Copy();

            //            dt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["date"], Col2 = r["bankAccount"] }).Select(g => g.OrderBy(r => r["date"]).First()).CopyToDataTable();


            //            DateTime date1 = DateTime.MaxValue;
            //            DateTime date2 = DateTime.MinValue;

            //            for (int i = 0; i < dx.Rows.Count; i++)
            //            {
            //                date = dx.Rows[i]["date"].ObjToDateTime();
            //                if (date < date1)
            //                    date1 = date;
            //                if (date > date2)
            //                    date2 = date;
            //                dx.Rows[i]["sDate"] = date.ToString("yyyyMMdd");
            //            }


            //            string account = "";
            //            string bankDetails = "";
            //            string sDate = "";
            //            double amount = 0D;
            //            double debit = 0D;
            //            adjustDt = dx.Clone();

            //            for (int i = 0; i < dt.Rows.Count; i++)
            //            {
            //                date = dt.Rows[i]["date"].ObjToDateTime();
            //                sDate = date.ToString("yyyyMMdd");
            //                account = dt.Rows[i]["bankAccount"].ObjToString();
            //                if (String.IsNullOrWhiteSpace(account))
            //                {
            //                    dt.Rows[i]["bankAccount"] = "No Bank Account Listed";
            //                    continue;
            //                }
            //                dRows = bankDt.Select("account_no='" + account + "'");
            //                if (dRows.Length > 0)
            //                    dt.Rows[i]["bankDetails"] = dRows[0]["account_title"].ObjToString() + " " + account;
            //                dRows = dx.Select("sDate='" + sDate + "' AND bankAccount='" + account + "' AND adjustment<>'Y'");
            //                amount = 0D;
            //                debit = 0D;
            //                for (int j = 0; j < dRows.Length; j++)
            //                {
            //                    amount += dRows[j]["amount"].ObjToDouble();
            //                    debit += dRows[j]["debit"].ObjToDouble();
            //                }

            ////                dRows = dx.Select("sDate='" + sDate + "' AND bankAccount='" + account + "' AND adjustment = 'Y'");

            //                dt.Rows[i]["amount"] = amount;
            //                dt.Rows[i]["debit"] = debit;
            //                dt.Rows[i]["inSystem"] = 0D;
            //                dt.Rows[i]["sDate"] = sDate;
            //            }

            //            //dRows = dx.Select("sDate='" + sDate + "' AND adjustment = 'Y'");
            //            dRows = dx.Select("adjustment = 'Y'");
            //            if (dRows.Length > 0)
            //                adjustDt = dRows.CopyToDataTable();

            double systemAmount = 0D;

            dt = pullSystemData(dt, adjustDt, date1, date2);

            double diff = 0D;
            double amount = 0D;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                amount = dt.Rows[i]["amount"].ObjToDouble();
                systemAmount = dt.Rows[i]["inSystem"].ObjToDouble();
                if (amount != systemAmount)
                    dt.Rows[i]["found"] = "MISMATCH";
                diff = amount - systemAmount;
                dt.Rows[i]["diff"] = diff;
            }

            gridMain3.Columns["ID"].Visible = false;
            gridMain3.Columns["tmstamp"].Visible = false;

            G1.NumberDataTable(dt);
            dgv3.DataSource = dt;

            chkGroup.Visible = true;
            chkGroupDays.Visible = true;

            picAdd.Hide();
            picAdd.Refresh();

            pictureDelete.Hide();
            pictureDelete.Refresh();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void chkGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroup.Checked)
            {
                //gridMain3.Columns["bankAccount"].GroupIndex = 0;
                gridMain3.Columns["sDate"].GroupIndex = 0;
                gridMain3.Columns["bankDetails"].GroupIndex = 1;
                //gridMain3.Columns["bankDetails"].Visible = false;
                gridMain3.Columns["bankAccount"].Visible = false;
                gridMain3.RefreshEditor(true);
                gridMain3.ExpandAllGroups();
            }
            else
            {
                //gridMain3.Columns["bankAccount"].GroupIndex = -1;
                gridMain3.Columns["bankDetails"].GroupIndex = -1;
                gridMain3.Columns["sDate"].GroupIndex = -1;
                //gridMain3.Columns["bankDetails"].Visible = true;
                gridMain3.Columns["bankAccount"].Visible = true;
                gridMain3.RefreshEditor(true);
            }

            dgv3.Refresh();
        }
        /***********************************************************************************************/
        private void btnHitBank_Click(object sender, EventArgs e)
        {
            //System.Diagnostics.Process.Start("https://selections.batesville.com/#/burial-solutions/caskets?selectionRoomId=32611"); // Batesville Caskets
            //System.Diagnostics.Process.Start("https://www.wellsfargoadvisors.com/online-access/signon.htm");

            try
            {
                webBrowser1.Navigate("www.wellsfargoadvisors.com/online-access/signon.htm");
            }
            catch ( Exception ex )
            {
            }


            //var elems = webBrowser1.Document.GetElementsByTagName("INPUT");

            //foreach (HtmlElement elem in elems)
            //{
            //    if (elem.GetAttribute("value") == "Save changes")
            //    {
            //        elem.InvokeMember("click");
            //    }
            //}
        }
        /***********************************************************************************************/
        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {

            HtmlElementCollection theElementCollection;
                theElementCollection = webBrowser1.Document.GetElementsByTagName("input");
                foreach (HtmlElement curElement in theElementCollection)
                {
                    if ((curElement.GetAttribute("id").ToString() == "user_name"))
                    {
                        curElement.SetAttribute("Value", "robby0483");
                    }
                    else if ((curElement.GetAttribute("id").ToString() == "user_password"))
                    {
                        curElement.SetAttribute("Value", "Xyzzy.0483##");
                        // In addition,you can get element value like this:
                        // MessageBox.Show(curElement.GetAttribute("Value"))
                    }
                }
                theElementCollection = webBrowser1.Document.GetElementsByTagName("input");
                foreach (HtmlElement curElement in theElementCollection)
                {
                    if (curElement.GetAttribute("id").Equals("login_button"))
                    {
                        curElement.InvokeMember("click");
                        //  javascript has a click method for we need to invoke on button and hyperlink elements.
                    }
                }
            //var elems = webBrowser1.Document.GetElementsByTagName("INPUT");

            //var junk = webBrowser1.Document.GetElementsByTagName( "UserName" );

            //var activeE = webBrowser1.Document.ActiveElement;

            //foreach (HtmlElement elem in elems)
            //{
            //    if (elem.GetAttribute("value") == "Save changes")
            //    {
            //        elem.InvokeMember("click");
            //    }
            //}
        }
        /***********************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv3.DataSource;
            int rowHandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetFocusedDataSourceRowIndex();
            if (workDt != null)
            {
                string found = dr["found"].ObjToString().ToUpper();
                string what = dr["ID"].ObjToString();
                string depositNumber = dr["depositNumber"].ObjToString();
                //string lastName = dr["lastName"].ObjToString();
                //string firstName = dr["firstName"].ObjToString();
                DateTime localDate = dr["date"].ObjToDateTime();
                if (found == "INSURANCE" )
                {
                    string cmd = "Select * from `payers` where `payer` = '" + what + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        what = dx.Rows[0]["contractNumber"].ObjToString();
                        this.Cursor = Cursors.WaitCursor;
                        if (!String.IsNullOrWhiteSpace(what))
                        {
                            CustomerDetails detailsForm = new CustomerDetails(what);
                            detailsForm.TopMost = true;
                            detailsForm.Show();
                        }
                        this.Cursor = Cursors.Default;
                    }
                }
                if ( found == "TRUST" )
                {
                    this.Cursor = Cursors.WaitCursor;
                    if (!String.IsNullOrWhiteSpace(what))
                    {
                        CustomerDetails detailsForm = new CustomerDetails(what);
                        detailsForm.TopMost = true;
                        detailsForm.Show();
                    }
                    this.Cursor = Cursors.Default;
                }
                if (found == "DOWNPMT")
                {
                    string list = "Daily History\nDown Payments";
                    string selection = "";
                    using (ListSelect listForm = new ListSelect(list, false))
                    {
                        listForm.Text = "Select What?";
                        listForm.ShowDialog();
                        selection = ListSelect.list_detail;
                    }
                    if (selection == "Daily History")
                    {
                        this.Cursor = Cursors.WaitCursor;
                        if (!String.IsNullOrWhiteSpace(what))
                        {
                            CustomerDetails detailsForm = new CustomerDetails(what);
                            detailsForm.TopMost = true;
                            detailsForm.Show();
                        }
                        this.Cursor = Cursors.Default;
                    }
                    else
                    {
                        string title = "Down Payments for " + depositNumber + " for " + localDate.ToString("yyyy-MM-dd");
                        string cmd = "Select * from `downpayments` where `date` = '" + localDate.ToString("yyyy-MM-dd") + "' AND `depositNumber` = '" + depositNumber + "' LIMIT 10;";
                        DataTable dx = G1.get_db_data(cmd);
                        if ( dx.Rows.Count <= 0 )
                        {
                            title = "Down Payments for " + depositNumber + " for " + localDate.AddDays(-5).ToString("yyyy-MM-dd") + " to " + localDate.AddDays(5).ToString("yyyy-MM-dd");
                            cmd = "Select * from `downpayments` where `date` >= '" + (localDate.AddDays(-5).ToString("yyyy-MM-dd")) + "' AND `date` <= '" + (localDate.AddDays(5).ToString("yyyy-MM-dd")) + "' AND `depositNumber` = '" + depositNumber + "' LIMIT 10;";
                            dx = G1.get_db_data(cmd);
                            if ( dx.Rows.Count <= 0 )
                            {
                                title = "Down Payments for " + depositNumber + " for " + localDate.AddDays(-10).ToString("yyyy-MM-dd") + " to " + localDate.AddDays(10).ToString("yyyy-MM-dd");
                                cmd = "Select * from `downpayments` where `date` >= '" + (localDate.AddDays(-10).ToString("yyyy-MM-dd")) + "' AND `date` <= '" + (localDate.AddDays(10).ToString("yyyy-MM-dd")) + "' AND `depositNumber` = '" + depositNumber + "' LIMIT 10;";
                                dx = G1.get_db_data(cmd);
                                if (dx.Rows.Count <= 0 )
                                {
                                    MessageBox.Show("*** ERROR *** Cannot find any Down Payments for :\n" + depositNumber + " for " + localDate.AddDays(-10).ToString("yyyy-MM-dd") + " to " + localDate.AddDays(10).ToString("yyyy-MM-dd"), "Down Payment Lookup Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                }
                            }
                        }
                        if (dx.Rows.Count > 0)
                        {
                            this.Cursor = Cursors.WaitCursor;
                            DownPayments dpForm = new DownPayments(dx, title );
                            dpForm.Text = title;
                            dpForm.TopMost = true;
                            dpForm.Show();
                            this.Cursor = Cursors.Default;
                        }

                    }
                }
                if (found == "FUNERAL")
                {
                    if (!String.IsNullOrWhiteSpace(what))
                    {
                        string cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + what + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            this.Cursor = Cursors.WaitCursor;
                            string contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                            FunPayments editFunPayments = new FunPayments(this, contractNumber, "", false, false);
                            editFunPayments.TopMost = true;
                            editFunPayments.Show();
                            this.Cursor = Cursors.Default;
                        }
                    }
                }
                if (found == "DOWN PAYMENT")
                {
                    if (!String.IsNullOrWhiteSpace(what))
                    {
                        string cmd = "Select * from `downpayments` where `date` = '" + localDate.ToString("yyyy-MM-dd") + "' AND `depositNumber` = '" + what + "' LIMIT 10;";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            this.Cursor = Cursors.WaitCursor;
                            DownPayments dpForm = new DownPayments(dx);
                            dpForm.TopMost = true;
                            dpForm.Show();
                            this.Cursor = Cursors.Default;
                        }
                        else
                        {
                            MessageBox.Show("*** ERROR *** Cannot find any Down Payments for :\n" + what + "!!!", "Down Payment Lookup Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        }
                    }
                }
                if (found == "Z INBANK")
                {
                    double debit = dr["debit"].ObjToDouble();
                    if ( debit != 0D )
                    {
                        DataTable ddz = dt.Clone();
                        ddz.ImportRow(dr);
                        BankEditDebit debitForm = new BankEditDebit(ddz);
                        debitForm.Text = "Document Debit for " + workDate.ToString("MM/dd/yyyy");
                        debitForm.TopMost = true;
                        debitForm.ManualDone += DebitForm_ManualDone;
                        debitForm.ShowDialog();
                    }
                }
                if ( found.ToUpper() == "ADJUSTMENT")
                {
                    picAdd_Click(null, null);
                }
                return;
            }

            this.Cursor = Cursors.WaitCursor;

            DateTime date = dr["date"].ObjToDateTime();
            string bankAccount = dr["bankAccount"].ObjToString();

            DataTable ddx = dt.Clone();
            ddx.ImportRow(dt.Rows[row]);

            //DataTable ddt = pullSystemData(ddx, date, date);



            DataTable ddt = pullData(dt, date, bankAccount);

            G1.NumberDataTable(ddt);

            ImportBankDetails bankForm = new ImportBankDetails(dt, ddt, this.dateTimePicker2.Value );
            bankForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void DebitForm_ManualDone(DataTable dd)
        {
            if (dd == null)
                return;
            if (dd.Rows.Count <= 0)
                return;
            string record = dd.Rows[0]["record"].ObjToString();
            double debit = dd.Rows[0]["debit"].ObjToDouble();
            string bank = dd.Rows[0]["bankAccount"].ObjToString();
            string debitDepNum = dd.Rows[0]["debitDepNum"].ObjToString();
            string depositNumber = dd.Rows[0]["depositNumber"].ObjToString();
            string assignTo = dd.Rows[0]["assignTo"].ObjToString();

            string found = "";
            string bankRecord = "";

            DataTable dt = (DataTable)dgv3.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                found = dt.Rows[i]["found"].ObjToString().ToUpper();
                if (found == "Z INBANK")
                {
                    bankRecord = dt.Rows[i]["record"].ObjToString();
                    if ( bankRecord == record )
                    {
                        G1.update_db_table("bank_details", "record", record, new string[] {"assignTo", assignTo, "description", depositNumber, "debitDepNum", debitDepNum });
                        dt.Rows[i]["depositNumber"] = depositNumber;
                        dt.Rows[i]["assignTo"] = assignTo;
                        dt.Rows[i]["debitDepNum"] = debitDepNum;
                        gridMain3.RefreshEditor(true);
                        break;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private DataTable pullData ( DataTable ddt, DateTime searchDate, string account )
        {
            DataTable dx = null;
            DataTable ddx = null;
            DateTime date = DateTime.Now;
            DateTime sDate = DateTime.MaxValue;
            DateTime eDate = DateTime.MinValue;

            string date1 = "";
            string date2 = "";
            string dateStr = "";
            string firstName = "";
            string lastName = "";

            string bankAccount = "";
            string saveAccount = account;
            string bankDetails = "";
            double amount = 0D;
            bool found = false;

            DataRow[] dRows = null;
            DataRow dRow = null;
            DataTable dt = ddt.Clone();

            string depositNumber = "";
            string record = "";

            string account2 = "";
            string account3 = "";

            string cmd = "Select * from `bank_accounts` WHERE `account_no` = '" + account + "';";
            DataTable bankDt = G1.get_db_data(cmd);
            if (bankDt.Rows.Count <= 0)
            {
                account2 = account;
                account3 = account;
            }
            else
            {
                account = bankDt.Rows[0]["location"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();

                account2 = bankDt.Rows[0]["account_title"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
                account3 = bankDt.Rows[0]["localDescription"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
            }

            date1 = searchDate.ToString("yyyy-MM-dd");
            date2 = searchDate.ToString("yyyy-MM-dd");

            cmd = "Select * from `bank_details` WHERE `date` = '" + searchDate.ToString("yyyy-MM-dd") + "' and `bankAccount` = '" + saveAccount + "' ORDER BY `date` asc;";
            DataTable detailDt = G1.get_db_data(cmd);

            dRows = detailDt.Select("adjustment <> 'Y'");
            DataTable bankDetailDt = detailDt.Clone();
            if (dRows.Length > 0)
                bankDetailDt = dRows.CopyToDataTable();

            DataTable adjustDt = detailDt.Clone();
            dRows = detailDt.Select("adjustment = 'Y'");
            if (dRows.Length > 0)
                adjustDt = dRows.CopyToDataTable();


            try
            {
                //cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + account + "' ORDER BY `payDate8` asc;";
                cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ORDER BY `payDate8` asc;";
                dx = G1.get_db_data(cmd);

                string[] Lines = null;

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    bankAccount = bankDetails;
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    dRows = ddt.Select("bankAccount='" + bankDetails + "'");

                    if (bankAccount == account )
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["payer"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Insurance";
                        dRow["sDate"] = searchDate.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }

                //cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + account + "' ORDER BY `payDate8` asc;";
                cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ORDER BY `payDate8` asc;";
                dx = G1.get_db_data(cmd);

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                    if (amount <= 0D)
                        continue;
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    bankAccount = bankDetails;
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    dRows = ddt.Select("bankAccount='" + bankDetails + "'");

                    if (bankAccount == account )
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["contractNumber"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Trust";
                        dRow["sDate"] = searchDate.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }

                double lossRecovery = 0D;
                double ccFee = 0D;
                bool gotit = false;

                //cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND ( `paymentType` = 'Cash' OR `paymentType` = 'Check-Local' ) AND `bankAccount` = '" + account3 + "';";
                cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND ( `paymentType` = 'Cash' OR `paymentType` = 'Check-Local' ) ;";
                cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' ;";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["date"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");
                    amount = dx.Rows[i]["downPayment"].ObjToDouble();
                    lossRecovery = dx.Rows[i]["lossRecoveryFee"].ObjToDouble();
                    ccFee = dx.Rows[i]["ccFee"].ObjToDouble();
                    amount += lossRecovery + ccFee;

                    bankDetails = dx.Rows[i]["bankAccount"].ObjToString();
                    bankAccount = bankDetails;
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    gotit = false;

                    dRows = ddt.Select("bankAccount='" + bankDetails + "'");
                    if (bankAccount == account3)
                        gotit = true;
                    else if (account3.Contains(bankAccount))
                        gotit = true;

                    //gotit = true;

                    if ( gotit )
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Down Payment";
                        dRow["sDate"] = searchDate.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }

                //cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` WHERE c.`dateReceived` >= '" + date1 + "' and c.`dateReceived` <= '" + date2 + "' AND `bankAccount` = '" + saveAccount + "';";
                cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` JOIN `fcustomers` g ON c.`contractNumber` = g.`contractNumber` WHERE c.`dateReceived` >= '" + date1 + "' and c.`dateReceived` <= '" + date2 + "' ;";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["dateReceived"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");
                    amount = dx.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (amount <= 0D)
                        amount = dx.Rows[i]["paid"].ObjToDouble();
                    bankDetails = dx.Rows[i]["bankAccount"].ObjToString();
                    bankAccount = bankDetails;
                    depositNumber = dx.Rows[i]["depositNumber"].ObjToString();
                    if ( depositNumber.ToUpper().IndexOf( "TD") == 0 || depositNumber.ToUpper().IndexOf ( "CCTD") == 0 )
                    {
                        dRows = dt.Select("depositNumber='" + depositNumber + "' AND found = 'Down Payment' AND sDate = '" + dateStr + "'");
                        if (dRows.Length > 0)
                            continue;
                    }

                    dRows = ddt.Select("bankAccount='" + saveAccount + "'");

                    if (bankAccount == saveAccount )
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["serviceId"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Funeral";
                        dRow["sDate"] = searchDate.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }

                //cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `downPayment` > '0.00'  AND `bank_account` = '" + account + "'ORDER BY `payDate8` asc;";
                cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `downPayment` > '0.00' ORDER BY `payDate8` asc;";
                dx = G1.get_db_data(cmd);

                lossRecovery = (double) DownPayments.GetLossRecoveryFee();

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["downPayment"].ObjToDouble();
                    ccFee = dx.Rows[i]["ccFee"].ObjToDouble();
                    if ( ccFee > 0D )
                        amount += lossRecovery + ccFee;
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    bankAccount = bankDetails;
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    depositNumber = dx.Rows[i]["depositNumber"].ObjToString();
                    if ( depositNumber.ToUpper().IndexOf ( "TD") == 0 || depositNumber.ToUpper().IndexOf ( "CCTD") == 0 )
                    {
                        dRows = dt.Select("depositNumber='" + depositNumber + "' AND found = 'Down Payment' AND sDate = '" + dateStr + "'");
                        if (dRows.Length > 0)
                            continue;
                    }

                    dRows = ddt.Select("bankAccount='" + bankDetails + "'");

                    if (bankAccount == account3 )
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["contractNumber"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "DownPmt";
                        dRow["sDate"] = searchDate.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }

                if ( adjustDt.Rows.Count > 0)
                {
                    for ( int k=0; k<adjustDt.Rows.Count; k++)
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["amount"] = adjustDt.Rows[k]["amount"].ObjToDouble();
                        dRow["debit"] = adjustDt.Rows[k]["debit"].ObjToDouble();
                        dRow["bankAccount"] = saveAccount;
                        dRow["ID"] = "Adjustment";
                        dRow["found"] = "Adjustment";
                        dRow["depositNumber"] = adjustDt.Rows[k]["description"].ObjToString();
                        dRow["sDate"] = searchDate.ToString("yyyyMMdd");
                        dRow["record"] = adjustDt.Rows[k]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }
                //cmd = "Select * from `bank_details` WHERE `date` = '" + searchDate.ToString("yyyy-MM-dd") + "' and `bankAccount` = '" + saveAccount + "' ORDER BY `date` asc;";
                //dx = G1.get_db_data(cmd);

                DateTime timeDate = DateTime.Now;

                dx = bankDetailDt.Copy();
                if ( dx.Rows.Count > 0 )
                {
                    if (G1.get_column_number(dt, "debitDepNum") < 0)
                        dt.Columns.Add("debitDepNum");
                    for ( int k=0; k<dx.Rows.Count; k++)
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["inSystem"] = dx.Rows[k]["amount"].ObjToDouble();
                        dRow["debit"] = dx.Rows[k]["debit"].ObjToDouble();
                        dRow["bankAccount"] = saveAccount;
                        dRow["ID"] = "In Bank";
                        dRow["found"] = "Z InBank";
                        dRow["depositNumber"] = dx.Rows[k]["description"].ObjToString();
                        dRow["sDate"] = searchDate.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[k]["record"].ObjToString();
                        timeDate = dx.Rows[k]["tmStamp"].ObjToDateTime();
                        dRow["tmStamp"] = G1.DTtoMySQLDT(timeDate);
                        dRow["assignTo"] = dx.Rows[k]["assignTo"].ObjToString();
                        dRow["debitDepNum"] = dx.Rows[k]["debitDepNum"].ObjToString();
                        dt.Rows.Add(dRow);

                    }
                }
            }
            catch ( Exception ex )
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private DataTable pullSystemData(DataTable dt, DataTable adjustDt, DateTime minDate, DateTime maxDate )
        {
            double systemAmount = 0D;

            DataTable dx = null;
            DateTime date = DateTime.Now;
            DateTime sDate = DateTime.MaxValue;
            DateTime eDate = DateTime.MinValue;

            string date1 = "";
            string date2 = "";
            string dateStr = "";
            string cmd = "";

            string bankAccount = "";
            string saveAccount = "";
            string bankDetails = "";
            string depositNumber = "";
            double amount = 0D;
            double debit = 0D;
            bool found = false;

            bool addExtra = false;
            if (chkAll.Checked)
                addExtra = true;

            DataRow[] dRows = null;
            DataRow[] ddRows = null;
            string[] Lines = null;
            DataRow dRow = null;

            date1 = minDate.ToString("yyyy-MM-dd");
            date2 = maxDate.ToString("yyyy-MM-dd");

            DataTable extraDt = dt.Clone();
            DataTable dpDt = dt.Clone();

            double paid = 0D;
            double amtReceived = 0D;

            try
            {
                //cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` = '" + account + "' ORDER BY `payDate8` asc;";
                cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ORDER BY `payDate8` asc;";
                dx = G1.get_db_data(cmd);

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    dRows = dt.Select("sDate='" + dateStr + "' AND bankAccount='" + bankDetails + "'");
                    if (dRows.Length > 0)
                    {
                        if (bankDetails == "45007424")
                        {
                        }
                        systemAmount = dRows[0]["inSystem"].ObjToDouble() + amount;
                        dRows[0]["inSystem"] = systemAmount;
                        dRows[0]["record"] = dx.Rows[i]["record"].ObjToString();
                    }
                    else if ( addExtra )
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["sDate"] = dateStr;
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        systemAmount = dRow["inSystem"].ObjToDouble() + amount;
                        dRow["inSystem"] = systemAmount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["bankDetails"] = dx.Rows[i]["bank_account"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }

                cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' ORDER BY `payDate8` asc;";
                dx = G1.get_db_data(cmd);

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    dRows = dt.Select("sDate='" + dateStr + "' AND bankAccount='" + bankDetails + "'");
                    if (dRows.Length > 0)
                    {
                        if (bankDetails == "45007424")
                        {
                        }
                        systemAmount = dRows[0]["inSystem"].ObjToDouble() + amount;
                        dRows[0]["inSystem"] = systemAmount;
                        dRows[0]["record"] = dx.Rows[i]["record"].ObjToString();
                    }
                    else if(addExtra)
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["sDate"] = dateStr;
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        systemAmount = dRow["inSystem"].ObjToDouble() + amount;
                        dRow["inSystem"] = systemAmount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["bankDetails"] = dx.Rows[i]["bank_account"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }

                double lossRecovery = 0D;
                double ccFee = 0D;

                cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND ( `paymentType` = 'Cash' OR `paymentType` = 'Check-Local' ) ;";
                cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "';";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["date"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");
                    amount = dx.Rows[i]["downPayment"].ObjToDouble();
                    lossRecovery = dx.Rows[i]["lossRecoveryFee"].ObjToDouble();
                    ccFee = dx.Rows[i]["ccFee"].ObjToDouble();
                    amount += lossRecovery + ccFee;

                    bankDetails = dx.Rows[i]["bankAccount"].ObjToString();
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    dRows = dt.Select("sDate='" + dateStr + "' AND bankAccount='" + bankDetails + "'");
                    if (dRows.Length > 0)
                    {
                        if (bankDetails == "45007424")
                        {
                        }
                        systemAmount = dRows[0]["inSystem"].ObjToDouble() + amount;
                        dRows[0]["inSystem"] = systemAmount;

                        dRow = dpDt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["sDate"] = dateStr;
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dRow["inSystem"] = amount;
                        dRow["found"] = "Down Payment";
                        dRow["bankAccount"] = bankDetails;
                        dRow["bankDetails"] = dx.Rows[i]["bankAccount"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dpDt.Rows.Add(dRow);
                    }
                    else if (addExtra)
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["sDate"] = dateStr;
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        systemAmount = dRow["inSystem"].ObjToDouble() + amount;
                        dRow["inSystem"] = systemAmount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["bankDetails"] = dx.Rows[i]["bankAccount"].ObjToString();
                        dt.Rows.Add( dRow );
                    }
                }

                //cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` WHERE c.`dateReceived` >= '" + date1 + "' and c.`dateReceived` <= '" + date2 + "' AND `bankAccount` = '" + saveAccount + "';";
                cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` WHERE c.`dateReceived` >= '" + date1 + "' and c.`dateReceived` <= '" + date2 + "';";
                dx = G1.get_db_data(cmd);
                string lastPaymentRecord = "";
                string paymentRecord = "";
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["dateReceived"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");
                    amount = dx.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    paymentRecord = dx.Rows[i]["paymentRecord"].ObjToString();
                    if (paymentRecord == lastPaymentRecord)
                        continue;
                    lastPaymentRecord = paymentRecord;
                    //if (amount == 0D)
                    //    continue;
                    if (amount == 0D)
                        amount = dx.Rows[i]["paid"].ObjToDouble();
                    bankDetails = dx.Rows[i]["bankAccount"].ObjToString();

                    dRows = dt.Select("sDate='" + dateStr + "' AND bankAccount='" + bankDetails + "'");
                    if (dRows.Length > 0)
                    {
                        if (bankDetails == "45007424")
                        {
                        }
                        depositNumber = dx.Rows[i]["depositNumber"].ObjToString();
                        if (depositNumber.ToUpper().IndexOf("TD") == 0 || depositNumber.ToUpper().IndexOf ( "CCTD") == 0 )
                        {
                            ddRows = dpDt.Select("depositNumber='" + depositNumber + "' AND found = 'Down Payment' AND sDate = '" + dateStr + "'");
                            if (ddRows.Length > 0)
                                continue;
                        }

                        systemAmount = dRows[0]["inSystem"].ObjToDouble() + amount;
                        dRows[0]["inSystem"] = systemAmount;
                        dRows[0]["record"] = dx.Rows[i]["record"].ObjToString();
                    }
                    else if (addExtra)
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["sDate"] = dateStr;
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        systemAmount = dRow["inSystem"].ObjToDouble() + amount;
                        dRow["inSystem"] = systemAmount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["bankDetails"] = dx.Rows[i]["bankAccount"].ObjToString();
                        dt.Rows.Add( dRow );
                    }
                }

                cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `downPayment` > '0.00' ORDER BY `payDate8` asc;";
                dx = G1.get_db_data(cmd);

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["downPayment"].ObjToDouble();
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    dRows = dt.Select("sDate='" + dateStr + "' AND bankAccount='" + bankDetails + "'");
                    if (dRows.Length > 0)
                    {
                        if (bankDetails == "45007424")
                        {
                        }
                        depositNumber = dx.Rows[i]["depositNumber"].ObjToString();
                        if (depositNumber.ToUpper().IndexOf("TD") == 0 || depositNumber.ToUpper().IndexOf("CCTD") == 0)
                        {
                            ddRows = dpDt.Select("depositNumber='" + depositNumber + "' AND found = 'Down Payment' AND sDate = '" + dateStr + "'");
                            if (ddRows.Length > 0)
                                continue;
                        }
                        systemAmount = dRows[0]["inSystem"].ObjToDouble() + amount;
                        dRows[0]["inSystem"] = systemAmount;
                        dRows[0]["record"] = dx.Rows[i]["record"].ObjToString();
                    }
                    else if (addExtra)
                    {
                        dRow = dt.NewRow();
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["sDate"] = dateStr;
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        systemAmount = dRow["inSystem"].ObjToDouble() + amount;
                        dRow["inSystem"] = systemAmount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["bankDetails"] = dx.Rows[i]["bank_account"].ObjToString();
                        dt.Rows.Add( dRow);
                    }
                }

                if ( adjustDt.Rows.Count > 0 )
                {
                    for (int i = 0; i < adjustDt.Rows.Count; i++)
                    {
                        date = adjustDt.Rows[i]["date"].ObjToDateTime();
                        dateStr = date.ToString("yyyyMMdd");

                        amount = adjustDt.Rows[i]["amount"].ObjToDouble();
                        debit = adjustDt.Rows[i]["debit"].ObjToDouble();
                        bankDetails = adjustDt.Rows[i]["bankAccount"].ObjToString();
                        Lines = bankDetails.Split('~');
                        if (Lines.Length >= 3)
                            bankDetails = Lines[2].Trim();

                        dRows = dt.Select("sDate='" + dateStr + "' AND bankAccount='" + bankDetails + "'");
                        if (dRows.Length > 0)
                        {
                            systemAmount = dRows[0]["inSystem"].ObjToDouble() + amount - Math.Abs(debit);
                            dRows[0]["inSystem"] = systemAmount;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int rowHandle = 0; // Ramma Zamma
            TabPage current = (sender as TabControl).SelectedTab;
            if (current == null)
                return;
            string name = current.Text.Trim().ToUpper();
            if (name == "POSTED")
                this.Text = "Imported Data List";
            else
                this.Text = "Import Bank Credit Card File";
        }
        /***********************************************************************************************/
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            this.dateTimePicker3.Value = this.dateTimePicker2.Value;
            chkHonorDates.Checked = true;
        }
        /***********************************************************************************************/
        private void chkGroupDays_CheckedChanged(object sender, EventArgs e)
        {
            if (chkGroupDays.Checked)
            {
                //gridMain3.Columns["bankAccount"].GroupIndex = 0;
                gridMain3.Columns["sDate"].GroupIndex = 0;
                //gridMain3.Columns["bankDetails"].GroupIndex = 1;
                //gridMain3.Columns["bankDetails"].Visible = false;
                gridMain3.Columns["bankAccount"].Visible = false;
                gridMain3.RefreshEditor(true);
                gridMain3.ExpandAllGroups();
            }
            else
            {
                //gridMain3.Columns["bankAccount"].GroupIndex = -1;
                //gridMain3.Columns["bankDetails"].GroupIndex = -1;
                gridMain3.Columns["sDate"].GroupIndex = -1;
                //gridMain3.Columns["bankDetails"].Visible = true;
                gridMain3.Columns["bankAccount"].Visible = true;
                gridMain3.RefreshEditor(true);
            }
            dgv3.Refresh();
        }
        /***********************************************************************************************/
        private void searchFuneralsForAmountToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv3.DataSource;
            int rowHandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetFocusedDataSourceRowIndex();

            string what = this.cmbSearch.Text;

            string level = this.cmbLevel.Text;

            DateTime date = dr["date"].ObjToDateTime();
            string ID = dr["ID"].ObjToString();
            if (ID.ToUpper() != "IN BANK")
            {
                MessageBox.Show("*** INFO *** Can Only Search for Bank Deposits!", "Search Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            DataTable ddt = new DataTable();
            ddt.Columns.Add("what");
            ddt.Columns.Add("date");
            ddt.Columns.Add("amount", Type.GetType("System.Double"));
            ddt.Columns.Add("diff", Type.GetType("System.Double"));
            ddt.Columns.Add("who");
            ddt.Columns.Add("level");
            ddt.Columns.Add("depositNumber");

            double amount = dr["inSystem"].ObjToDouble();
            string sAmount = amount.ToString();
            string sDate = date.ToString("yyyy-MM-dd");
            string sDate1 = date.AddDays(-10).ToString("yyyy-MM-dd");
            string sDate2 = date.AddDays(10).ToString("yyyy-MM-dd");

            string cmd = "";
            DataTable testDt = null;

            this.Cursor = Cursors.WaitCursor;

            PleaseWait waitForm = G1.StartWait( "Please Wait!\nSearching Trust Payments!");

            //cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '2022-07-01' AND `payDate8` <= '2022-07-31' AND `paymentAmount` = '485.00' ORDER BY `payDate8` ASC;";
            if (what.ToUpper().IndexOf("TRUSTS") >= 0 || String.IsNullOrWhiteSpace ( what ) )
            {
                waitForm.FireEvent2("Please Wait!\nSearching Trust Payments!");
                cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '" + sDate1 + "' AND `payDate8` <= '" + sDate2 + "' AND `paymentAmount` <= '" + sAmount + "' ORDER BY `payDate8` ASC;";
                testDt = G1.get_db_data(cmd);
                ddt = ProcessSearchData(ddt, testDt, amount, "payDate8", "paymentAmount", "contractNumber", "Trust");
            }

            if (what.ToUpper().IndexOf("INSURANCED") >= 0 || String.IsNullOrWhiteSpace(what))
            {
                waitForm.FireEvent2("Please Wait!\nSearching Insurance!");
                //cmd = "SELECT* FROM `ipayments` WHERE `payDate8` >= '2022-07-01' AND `payDate8` <= '2022-07-31' AND `paymentAmount` = '485.00' ORDER BY `payDate8` ASC;";
                cmd = "SELECT* FROM `ipayments` WHERE `payDate8` >= '" + sDate1 + "' AND `payDate8` <= '" + sDate2 + "' AND `paymentAmount` <= '" + sAmount + "' ORDER BY `payDate8` ASC;";
                testDt = G1.get_db_data(cmd);
                //testDt = CondenseByDepositNumber(testDt);
                ddt = ProcessSearchData(ddt, testDt, amount, "payDate8", "paymentAmount", "contractNumber", "Insurance");
            }

            if (what.ToUpper().IndexOf("FUNERALS") >= 0 || String.IsNullOrWhiteSpace(what))
            {
                waitForm.FireEvent2("Please Wait!\nSearching Funerals!");
                //cmd = "SELECT* FROM `cust_payment_details` WHERE `dateReceived` >= '2022-07-01' AND `dateReceived` <= '2022-07-31' AND`amtActuallyReceived` = '485.00';";
                cmd = "SELECT* FROM `cust_payment_details` WHERE `dateReceived` >= '" + sDate1 + "' AND `dateReceived` <= '" + sDate2 + "' ORDER BY `dateReceived` ASC;";
                testDt = G1.get_db_data(cmd);
                double paid = 0D;
                double amtReceived = 0D;
                for ( int i=0; i<testDt.Rows.Count; i++)
                {
                    paid = testDt.Rows[i]["paid"].ObjToDouble();
                    amtReceived = testDt.Rows[i]["amtActuallyReceived"].ObjToDouble();
                    if (amtReceived == 0D && paid != 0D)
                        testDt.Rows[i]["amtActuallyReceived"] = paid;
                }
                //ddt = ProcessSearchData(ddt, testDt, amount, "dateReceived", "paid", "contractNumber", "Funeral");
                ddt = ProcessSearchData(ddt, testDt, amount, "dateReceived", "amtActuallyReceived", "contractNumber", "Funeral");
            }

            if (what.ToUpper().IndexOf("DOWN PAYMENTS") >= 0 || String.IsNullOrWhiteSpace(what))
            {
                waitForm.FireEvent2("Please Wait!\nSearching Down Payments!");
                //cmd = "SELECT* FROM `downpayments` WHERE `date` >= '2022-07-01' AND `date` <= '2022-07-31' AND `totalDeposit` = '485.00' ORDER BY `date` ASC;";
                cmd = "SELECT* FROM `downpayments` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + "' ORDER BY `date` ASC;";
                testDt = G1.get_db_data(cmd);
                ddt = ProcessSearchData(ddt, testDt, amount, "date", "downPayment", "depositNumber", "Down Payments");
                ddt = ProcessSearchData(ddt, testDt, amount, "date", "totalDeposit", "depositNumber", "Down Payments");
            }


            cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '2022-07-01' AND `payDate8` <= '2022-07-31' AND `paymentAmount` <= '485.00' ORDER BY `payDate8` ASC;";
            cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '2022-07-01' AND `payDate8` <= '2022-07-31' AND `downPayment` > '0' AND `downPayment` <= '485.00' ORDER BY `payDate8` ASC;";

            cmd = "SELECT* FROM `ipayments` WHERE `payDate8` = '2022-07-27' ORDER BY `payDate8` ASC;";

            cmd = "SELECT* FROM `cust_payment_details` WHERE `dateReceived` = '2022-07-27' AND `paid` <= '485.00';";

            if (what.ToUpper().IndexOf("DOWN PAYMENTS") >= 0 || String.IsNullOrWhiteSpace(what))
            {
                waitForm.FireEvent2("Please Wait!\nSearching Payment Down Payments!");
                cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '" + sDate1 + "' AND `payDate8` <= '" + sDate2 + "' AND `downPayment` > '0.00' AND `downPayment` <= '" + sAmount + "' ORDER BY `payDate8` ASC;";
                testDt = G1.get_db_data(cmd);
                ddt = ProcessSearchData(ddt, testDt, amount, "payDate8", "downPayment", "contractNumber", "DownPmt");
            }

            G1.StopWait(ref waitForm);

            if (ddt.Rows.Count > 0)
            {
                //DataView tempview = ddt.DefaultView;
                //tempview.Sort = "date asc";
                //ddt = tempview.ToTable();

                ViewDataTable viewForm = new ViewDataTable(ddt, "what,date,depositNumber,amount,who,level");
                viewForm.Text = this.Text + " for " + date.ToString("MM/dd/yyyy");
                viewForm.TopMost = true;
                viewForm.ManualDone += ViewForm_ManualDone;
                viewForm.Show();
            }
            else
            {
                sAmount = G1.ReformatMoney(amount);
                MessageBox.Show("*** INFO *** Cannot locate any matching data for $" + sAmount + "!", "Locate Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable CondenseByDepositNumber ( DataTable dt )
        {
            DataView tempview = dt.DefaultView;
            tempview.Sort = "payDate8 ASC, depositNumber ASC";
            dt = tempview.ToTable();
            DataTable dx = dt.Clone();

            DateTime oldDate = DateTime.MinValue;
            DateTime date = DateTime.Now;
            string oldDepositNumber = "";
            string depositNumber = "";
            double paymentAmount = 0D;
            double totalPayment = 0D;
            int lastRow = -1;
            bool first = true;
            try
            {
                for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                {
                    if (first)
                    {
                        dx.ImportRow(dt.Rows[i]);
                        first = false;
                    }
                    date = dt.Rows[i]["payDate8"].ObjToDateTime();
                    if (oldDate == DateTime.MinValue)
                        oldDate = date;
                    depositNumber = dt.Rows[i]["depositNumber"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldDepositNumber))
                        oldDepositNumber = depositNumber;
                    if (oldDate != date || oldDepositNumber != depositNumber)
                    {
                        dx.ImportRow(dt.Rows[i]);
                        totalPayment = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        oldDate = date;
                        oldDepositNumber = depositNumber;

                    }
                    else
                    {
                        paymentAmount = dt.Rows[i]["paymentAmount"].ObjToDouble();
                        totalPayment += paymentAmount;
                        lastRow = dx.Rows.Count - 1;
                        dx.Rows[lastRow]["paymentAmount"] = totalPayment;
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            return dx;
        }
        /***********************************************************************************************/
        private void ViewForm_ManualDone( DataTable dd, DataRow dr)
        {
            DateTime date = dr["date"].ObjToDateTime();
            string what = dr["what"].ObjToString();
            string who = dr["who"].ObjToString();
            double amount = dr["amount"].ObjToDouble();
            string level = dr["level"].ObjToString();

            string cmd = "";
            DataTable dx = null;
            //string depositNumber dr["depositNumber"].ObjToString();

            if ( what.Trim().ToUpper() == "TRUST" && !String.IsNullOrWhiteSpace ( who ))
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(who);
                clientForm.TopMost = true;
                clientForm.Show();
            }
            else if ( what.Trim().ToUpper() == "FUNERAL" && !String.IsNullOrWhiteSpace ( who ))
            {
                cmd = "Select * from `fcust_extended` WHERE `serviceId` = '" + who + "';";
                dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 )
                {
                    cmd = "Select * from `fcust_extended` WHERE `contractNumber` = '" + who + "';";
                    dx = G1.get_db_data(cmd);
                }
                if ( dx.Rows.Count > 0 )
                {
                    string contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                    this.Cursor = Cursors.WaitCursor;
                    EditCust clientForm = new EditCust (contractNumber );
                    clientForm.TopMost = true;
                    clientForm.Show();
                }
            }
        }
        /***********************************************************************************************/
        private void TestAllBankDeposits ()
        {
            DataRow dr = gridMain3.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv3.DataSource;

            DataRow [] dRows = dt.Select ( "found='Z INBANK'");
            if ( dRows.Length <= 0 )
            {
                MessageBox.Show("*** INFO *** There are no Bank Deposits in which to search!", "Search Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            int rowHandle = gridMain3.FocusedRowHandle;
            int row = gridMain3.GetFocusedDataSourceRowIndex();

            string what = this.cmbSearch.Text;

            string level = this.cmbLevel.Text;

            DataTable ddt = new DataTable();
            ddt.Columns.Add("what");
            ddt.Columns.Add("date");
            ddt.Columns.Add("amount", Type.GetType("System.Double"));
            ddt.Columns.Add("diff", Type.GetType("System.Double"));
            ddt.Columns.Add("who");
            ddt.Columns.Add("level");
            ddt.Columns.Add("depositNumber");
            ddt.Columns.Add("record");

            double amount = 0D;
            DateTime date = DateTime.Now;

            string where = "";
            string str = "";
            string found = "";

            testingAll = true;

            double paid = 0D;
            double amtReceived = 0D;

            PleaseWait waitForm = null;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    found = dt.Rows[i]["found"].ObjToString().ToUpper();
                    if (found != "Z INBANK")
                        continue;

                    amount = dt.Rows[i]["inSystem"].ObjToDouble();
                    date = dt.Rows[i]["date"].ObjToDateTime();
                    string sAmount = amount.ToString();
                    string sDate = date.ToString("yyyy-MM-dd");
                    string sDate1 = date.AddDays(-10).ToString("yyyy-MM-dd");
                    string sDate2 = date.AddDays(10).ToString("yyyy-MM-dd");

                    string cmd = "";
                    DataTable testDt = null;

                    ddt.Rows.Clear();

                    this.Cursor = Cursors.WaitCursor;

                    waitForm = G1.StartWait("Please Wait!\nSearching Trust Payments!");

                    //cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '2022-07-01' AND `payDate8` <= '2022-07-31' AND `paymentAmount` = '485.00' ORDER BY `payDate8` ASC;";
                    if (what.ToUpper().IndexOf("TRUSTS") >= 0 || String.IsNullOrWhiteSpace(what))
                    {
                        waitForm.FireEvent2("Please Wait!\nSearching Trust Payments!");
                        cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '" + sDate1 + "' AND `payDate8` <= '" + sDate2 + "' AND `paymentAmount` <= '" + sAmount + "' ORDER BY `payDate8` ASC;";
                        testDt = G1.get_db_data(cmd);
                        ddt = ProcessSearchData(ddt, testDt, amount, "payDate8", "paymentAmount", "contractNumber", "Trust");
                    }

                    if (what.ToUpper().IndexOf("INSURANCED") >= 0 || String.IsNullOrWhiteSpace(what))
                    {
                        waitForm.FireEvent2("Please Wait!\nSearching Insurance!");
                        //cmd = "SELECT* FROM `ipayments` WHERE `payDate8` >= '2022-07-01' AND `payDate8` <= '2022-07-31' AND `paymentAmount` = '485.00' ORDER BY `payDate8` ASC;";
                        cmd = "SELECT* FROM `ipayments` WHERE `payDate8` >= '" + sDate1 + "' AND `payDate8` <= '" + sDate2 + "' AND `paymentAmount` <= '" + sAmount + "' ORDER BY `payDate8` ASC;";
                        testDt = G1.get_db_data(cmd);
                        ddt = ProcessSearchData(ddt, testDt, amount, "payDate8", "paymentAmount", "contractNumber", "Insurance");
                    }

                    if (what.ToUpper().IndexOf("FUNERALS") >= 0 || String.IsNullOrWhiteSpace(what))
                    {
                        waitForm.FireEvent2("Please Wait!\nSearching Funerals!");
                        //cmd = "SELECT* FROM `cust_payment_details` WHERE `dateReceived` >= '2022-07-01' AND `dateReceived` <= '2022-07-31' AND`amtActuallyReceived` = '485.00';";
                        cmd = "SELECT* FROM `cust_payment_details` WHERE `dateReceived` >= '" + sDate1 + "' AND `dateReceived` <= '" + sDate2 + "' ORDER BY `dateReceived` ASC;";
                        testDt = G1.get_db_data(cmd);
                        for (int j = 0; j < testDt.Rows.Count; j++)
                        {
                            paid = testDt.Rows[j]["paid"].ObjToDouble();
                            amtReceived = testDt.Rows[j]["amtActuallyReceived"].ObjToDouble();
                            if (amtReceived == 0D && paid != 0D)
                                testDt.Rows[j]["amtActuallyReceived"] = paid;
                        }
                        //ddt = ProcessSearchData(ddt, testDt, amount, "dateReceived", "paid", "contractNumber", "Funeral");
                        ddt = ProcessSearchData(ddt, testDt, amount, "dateReceived", "amtActuallyReceived", "contractNumber", "Funeral");
                    }

                    if (what.ToUpper().IndexOf("DOWN PAYMENTS") >= 0 || String.IsNullOrWhiteSpace(what))
                    {
                        waitForm.FireEvent2("Please Wait!\nSearching Down Payments!");
                        //cmd = "SELECT* FROM `downpayments` WHERE `date` >= '2022-07-01' AND `date` <= '2022-07-31' AND `totalDeposit` = '485.00' ORDER BY `date` ASC;";
                        cmd = "SELECT* FROM `downpayments` WHERE `date` >= '" + sDate1 + "' AND `date` <= '" + sDate2 + "' ORDER BY `date` ASC;";
                        testDt = G1.get_db_data(cmd);
                        ddt = ProcessSearchData(ddt, testDt, amount, "date", "downPayment", "depositNumber", "Down Payments");
                        ddt = ProcessSearchData(ddt, testDt, amount, "date", "totalDeposit", "depositNumber", "Down Payments");
                    }


                    cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '2022-07-01' AND `payDate8` <= '2022-07-31' AND `paymentAmount` <= '485.00' ORDER BY `payDate8` ASC;";
                    cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '2022-07-01' AND `payDate8` <= '2022-07-31' AND `downPayment` > '0' AND `downPayment` <= '485.00' ORDER BY `payDate8` ASC;";

                    cmd = "SELECT* FROM `ipayments` WHERE `payDate8` = '2022-07-27' ORDER BY `payDate8` ASC;";

                    cmd = "SELECT* FROM `cust_payment_details` WHERE `dateReceived` = '2022-07-27' AND `paid` <= '485.00';";

                    if (what.ToUpper().IndexOf("DOWN PAYMENTS") >= 0 || String.IsNullOrWhiteSpace(what))
                    {
                        waitForm.FireEvent2("Please Wait!\nSearching Payment Down Payments!");
                        cmd = "SELECT* FROM `payments` WHERE `payDate8` >= '" + sDate1 + "' AND `payDate8` <= '" + sDate2 + "' AND `downPayment` > '0.00' AND `downPayment` <= '" + sAmount + "' ORDER BY `payDate8` ASC;";
                        testDt = G1.get_db_data(cmd);
                        ddt = ProcessSearchData(ddt, testDt, amount, "payDate8", "downPayment", "contractNumber", "DownPmt");
                    }

                    if (ddt.Rows.Count > 0)
                    {
                        str = "";
                        for (int j = 0; j < ddt.Rows.Count; j++)
                        {
                            where = ddt.Rows[j]["what"].ObjToString();
                            if (!str.Contains(where))
                                str += where + ",";
                        }
                        str = str.Trim();
                        str = str.TrimEnd(',');
                        dt.Rows[i]["depositNumber"] = str;
                    }
                    else
                        dt.Rows[i]["depositNumber"] = "";

                    G1.StopWait(ref waitForm);
                }
            }
            catch ( Exception ex )
            {
                if ( waitForm != null )
                    G1.StopWait(ref waitForm);
            }

            dgv3.DataSource = dt;
            dgv3.Refresh();

            //if (ddt.Rows.Count > 0)
            //{
            //    //DataView tempview = ddt.DefaultView;
            //    //tempview.Sort = "date asc";
            //    //ddt = tempview.ToTable();

            //    ViewDataTable viewForm = new ViewDataTable(ddt, "what,date,amount,who,level");
            //    viewForm.Text = this.Text + " for " + date.ToString("MM/dd/yyyy");
            //    viewForm.TopMost = true;
            //    viewForm.ShowDialog();
            //}
            //else
            //{
            //    sAmount = G1.ReformatMoney(amount);
            //    MessageBox.Show("*** INFO *** Cannot locate any matching data for $" + sAmount + "!", "Locate Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            //}

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable ProcessSearchData ( DataTable ddt, DataTable testDt, double searchAmount, string dateCol, string amountCol, string whoCol, string what )
        {
            if (testDt.Rows.Count <= 0)
                return ddt;

            DataRow dRow = null;
            double newAmount = 0D;
            bool found = false;

            string level = this.cmbLevel.Text.Trim();
            if (String.IsNullOrWhiteSpace(level))
                level = "2";

            for ( int i=0; i<testDt.Rows.Count; i++)
            {
                newAmount = testDt.Rows[i][amountCol].ObjToDouble();
                if (newAmount == searchAmount)
                {
                    dRow = ddt.NewRow();
                    dRow["what"] = what;
                    dRow["who"] = testDt.Rows[i][whoCol].ObjToString();
                    dRow["date"] = testDt.Rows[i][dateCol].ObjToDateTime().ToString("yyyy-MM-dd");
                    dRow["depositNumber"] = testDt.Rows[i]["depositNumber"].ObjToString();
                    dRow["amount"] = newAmount;
                    dRow["level"] = "1";
                    ddt.Rows.Add(dRow);
                    found = true;
                    break;
                }
            }
            if (found)
                return ddt;

            if (level == "1")
                return ddt;

            double newAmount2 = 0D;
            for (int i = 0; i < testDt.Rows.Count; i++)
            {
                newAmount = testDt.Rows[i][amountCol].ObjToDouble();
                for (int j = 0; j < testDt.Rows.Count; j++)
                {
                    if (j == i)
                        continue;
                    newAmount2 = testDt.Rows[j][amountCol].ObjToDouble();
                    if ( ( newAmount2 + newAmount) == searchAmount)
                    {
                        dRow = ddt.NewRow();
                        dRow["what"] = what;
                        dRow["who"] = testDt.Rows[i][whoCol].ObjToString();
                        dRow["date"] = testDt.Rows[i][dateCol].ObjToDateTime().ToString("MM/dd/yyyy");
                        dRow["depositNumber"] = testDt.Rows[i]["depositNumber"].ObjToString();
                        dRow["amount"] = newAmount;
                        dRow["level"] = "1";
                        ddt.Rows.Add(dRow);

                        dRow = ddt.NewRow();
                        dRow["what"] = what;
                        dRow["who"] = testDt.Rows[j][whoCol].ObjToString();
                        dRow["date"] = testDt.Rows[j][dateCol].ObjToDateTime().ToString("MM/dd/yyyy");
                        dRow["depositNumber"] = testDt.Rows[j]["depositNumber"].ObjToString();
                        dRow["amount"] = newAmount2;
                        dRow["level"] = "2";
                        ddt.Rows.Add(dRow);

                        found = true;
                        break;
                    }
                }
            }

            if (ddt.Rows.Count > 0)
                return ddt;
            if (level == "2")
                return ddt;

            double newAmount3 = 0D;
            for (int i = 0; i < testDt.Rows.Count; i++)
            {
                newAmount = testDt.Rows[i][amountCol].ObjToDouble();
                for (int j = 0; j < testDt.Rows.Count; j++)
                {
                    newAmount2 = testDt.Rows[j][amountCol].ObjToDouble();
                    for (int k = 0; k < testDt.Rows.Count; k++)
                    {
                        if (k == j)
                            continue;
                        newAmount3 = testDt.Rows[j][amountCol].ObjToDouble();
                        if ((newAmount2 + newAmount3 + newAmount) == searchAmount)
                        {
                            dRow = ddt.NewRow();
                            dRow["what"] = what;
                            dRow["who"] = testDt.Rows[i][whoCol].ObjToString();
                            dRow["date"] = testDt.Rows[i][dateCol].ObjToDateTime().ToString("MM/dd/yyyy");
                            dRow["depositNumber"] = testDt.Rows[i]["depositNumber"].ObjToString();
                            dRow["amount"] = newAmount;
                            dRow["level"] = "1";
                            ddt.Rows.Add(dRow);

                            dRow = ddt.NewRow();
                            dRow["what"] = what;
                            dRow["who"] = testDt.Rows[j][whoCol].ObjToString();
                            dRow["date"] = testDt.Rows[j][dateCol].ObjToDateTime().ToString("MM/dd/yyyy");
                            dRow["depositNumber"] = testDt.Rows[j]["depositNumber"].ObjToString();
                            dRow["amount"] = newAmount2;
                            dRow["level"] = "2";
                            ddt.Rows.Add(dRow);

                            dRow = ddt.NewRow();
                            dRow["what"] = what;
                            dRow["who"] = testDt.Rows[k][whoCol].ObjToString();
                            dRow["date"] = testDt.Rows[k][dateCol].ObjToDateTime().ToString("MM/dd/yyyy");
                            dRow["depositNumber"] = testDt.Rows[k]["depositNumber"].ObjToString();
                            dRow["amount"] = newAmount3;
                            dRow["level"] = "3";
                            ddt.Rows.Add(dRow);

                            found = true;
                            break;
                        }
                    }
                }
            }

            return ddt;
        }
        /***********************************************************************************************/
        private void cmbSearch_EditValueChanged(object sender, EventArgs e)
        {
            string procLoc = "";
            string[] locIDs = this.cmbSearch.EditValue.ToString().Split('|');
            string what = this.cmbSearch.Text;
        }
        /***********************************************************************************************/
        private void btnTestAll_Click(object sender, EventArgs e)
        {
            TestAllBankDeposits();
            testingAll = false;
        }
        /***********************************************************************************************/
        private void showBankDayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();
            string date1 = date.ToString("yyyy-MM-dd");
            string date2 = date1;
            double amount = dr["amount"].ObjToDouble();
            string bankAccount = dr["bankAccount"].ObjToString();
            string bankDetails = dr["bankDetails"].ObjToString();

            ShowDateData(date, date1, date2, amount, bankAccount, bankDetails);
        }
        /***********************************************************************************************/
        private void showBankDateRangeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            string date1 = date.ToString("yyyy-MM-dd");
            date = this.dateTimePicker3.Value;
            string date2 = date.ToString("yyyy-MM-dd");

            DataTable dt = (DataTable) dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            date = dr["date"].ObjToDateTime();
            double amount = dr["amount"].ObjToDouble();
            string bankAccount = dr["bankAccount"].ObjToString();
            string bankDetails = dr["bankDetails"].ObjToString();

            ShowDateData(date, date1, date2, amount, bankAccount, bankDetails );
        }
        /***********************************************************************************************/
        private void ShowDateData ( DateTime bankDate, string date1, string date2, double searchAmount, string bankAccount, string searchBankDetails )
        {
            DataTable dtt = (DataTable)dgv3.DataSource;
            DataTable dt = dtt.Clone();

            DateTime date = DateTime.Now;
            string dateStr = "";
            string bankDetails = "";
            double amount = 0D;
            string saveAccount = bankAccount;

            string[] Lines = null;
            DataRow[] dRows = null;
            DataRow dRow = null;

            this.Cursor = Cursors.WaitCursor;

            string cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` LIKE '%" + bankAccount + "' ORDER BY `payDate8` asc;";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                Lines = null;

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                    if (amount <= 0D)
                        continue;
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    bankAccount = bankDetails;
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    if (bankAccount == bankAccount)
                    {
                        dRow = dt.NewRow();
                        //dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["payer1"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Insurance";
                        dRow["sDate"] = date.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }
            }

            cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' and `payDate8` <= '" + date2 + "' AND `bank_account` LIKE '%" + bankAccount + "' ORDER BY `payDate8` asc;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                Lines = null;

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                    if (amount <= 0D)
                        continue;
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    bankAccount = bankDetails;
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    if (bankAccount == bankAccount)
                    {
                        dRow = dt.NewRow();
                        //dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["contractNumber"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Trust";
                        dRow["sDate"] = date.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }
            }

            double lossRecovery = 0D;
            double ccFee = 0D;

            string account = bankAccount;
            string account2 = bankAccount;
            string account3 = bankAccount;
            string account4 = bankDetails;

            cmd = "Select * from `bank_accounts` WHERE `account_no` = '" + bankAccount + "';";
            DataTable bankDt = G1.get_db_data(cmd);
            if (bankDt.Rows.Count > 0 )
            {
                account = bankDt.Rows[0]["location"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();

                account2 = bankDt.Rows[0]["account_title"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
                account3 = bankDt.Rows[0]["localDescription"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
            }


            cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND ( `paymentType` = 'Cash' OR `paymentType` = 'Check-Local' ) AND `bankAccount` = '" + account3 + "';";
            //cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND ( `paymentType` = 'Cash' OR `paymentType` = 'Check-Local' ) ;";
            cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' and `date` <= '" + date2 + "' AND `bankAccount` LIKE '%" + account4 + "';";
            dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                date = dx.Rows[i]["date"].ObjToDateTime();
                dateStr = date.ToString("yyyyMMdd");
                amount = dx.Rows[i]["downPayment"].ObjToDouble();
                lossRecovery = dx.Rows[i]["lossRecoveryFee"].ObjToDouble();
                ccFee = dx.Rows[i]["ccFee"].ObjToDouble();
                amount += lossRecovery + ccFee;

                bankDetails = dx.Rows[i]["bankAccount"].ObjToString();
                bankAccount = bankDetails;
                Lines = bankDetails.Split('~');
                if (Lines.Length >= 3)
                    bankDetails = Lines[2].Trim();

                if (bankAccount == account3)
                {
                    dRow = dt.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["amount"] = amount;
                    dRow["bankAccount"] = bankDetails;
                    dRow["ID"] = dx.Rows[i]["depositNumber"].ObjToString();
                    dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                    dRow["found"] = "Down Payment";
                    dRow["sDate"] = date.ToString("yyyyMMdd");
                    dRow["record"] = dx.Rows[i]["record"].ObjToString();
                    dt.Rows.Add(dRow);
                }
            }


            string depositNumber = "";
            cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` WHERE c.`dateReceived` >= '" + date1 + "' and c.`dateReceived` <= '" + date2 + "' AND `bankAccount` = '" + saveAccount + "';";
            //cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` JOIN `fcustomers` g ON c.`contractNumber` = g.`contractNumber` WHERE c.`dateReceived` >= '" + date1 + "' and c.`dateReceived` <= '" + date2 + "' ;";
            dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                date = dx.Rows[i]["dateReceived"].ObjToDateTime();
                dateStr = date.ToString("yyyyMMdd");
                amount = dx.Rows[i]["amtActuallyReceived"].ObjToDouble();
                if (amount <= 0D)
                    amount = dx.Rows[i]["paid"].ObjToDouble();
                bankDetails = dx.Rows[i]["bankAccount"].ObjToString();
                bankAccount = bankDetails;
                depositNumber = dx.Rows[i]["depositNumber"].ObjToString();
                if (depositNumber.ToUpper().IndexOf("TD") == 0 || depositNumber.ToUpper().IndexOf("CCTD") == 0)
                {
                    dRows = dt.Select("depositNumber='" + depositNumber + "' AND found = 'Down Payment' AND sDate = '" + dateStr + "'");
                    if (dRows.Length > 0)
                        continue;
                }

                if (bankAccount == saveAccount)
                {
                    dRow = dt.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["amount"] = amount;
                    dRow["bankAccount"] = bankDetails;
                    dRow["ID"] = dx.Rows[i]["serviceId"].ObjToString();
                    dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                    dRow["found"] = "Funeral";
                    dRow["sDate"] = date.ToString("yyyyMMdd");
                    dRow["record"] = dx.Rows[i]["record"].ObjToString();
                    dt.Rows.Add(dRow);
                }
            }

            cmd = "Select * from `bank_details` WHERE `date` = '" + bankDate.ToString("yyyy-MM-dd") + "' and `bankAccount` = '" + saveAccount + "' ORDER BY `date` asc;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int k = 0; k < dx.Rows.Count; k++)
                {
                    dRow = dt.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(bankDate);
                    dRow["inSystem"] = dx.Rows[k]["amount"].ObjToDouble();
                    dRow["debit"] = dx.Rows[k]["debit"].ObjToDouble();
                    dRow["bankAccount"] = saveAccount;
                    dRow["ID"] = "In Bank";
                    dRow["found"] = "Z InBank";
                    dRow["depositNumber"] = dx.Rows[k]["description"].ObjToString();
                    dRow["sDate"] = bankDate.ToString("yyyyMMdd");
                    dt.Rows.Add(dRow);

                }
            }

            double diff = 0D;
            double systemAmount = 0D;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                amount = dt.Rows[i]["amount"].ObjToDouble();
                systemAmount = dt.Rows[i]["inSystem"].ObjToDouble();
                diff = amount - systemAmount;
                dt.Rows[i]["diff"] = diff;
            }


            G1.NumberDataTable(dt);

            string sAmount = G1.ReformatMoney(searchAmount);
            string title = "Searching for $" + sAmount + " in Bank Account " + searchBankDetails + " from " + date1 + " to " + date2;

            DataTable saveDt = (DataTable) dgv3.DataSource;

            ImportBankDetails bankForm = new ImportBankDetails(saveDt, dt, this.dateTimePicker2.Value, title);

            bankForm.Show();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSubGroupDays.Checked)
            {
                //gridMain3.Columns["bankAccount"].GroupIndex = 0;
                gridMain3.Columns["sDate"].GroupIndex = 0;
                //gridMain3.Columns["bankDetails"].GroupIndex = 1;
                //gridMain3.Columns["bankDetails"].Visible = false;
                gridMain3.Columns["bankAccount"].Visible = false;
                gridMain3.RefreshEditor(true);
                gridMain3.ExpandAllGroups();
            }
            else
            {
                //gridMain3.Columns["bankAccount"].GroupIndex = -1;
                //gridMain3.Columns["bankDetails"].GroupIndex = -1;
                gridMain3.Columns["sDate"].GroupIndex = -1;
                //gridMain3.Columns["bankDetails"].Visible = true;
                gridMain3.Columns["bankAccount"].Visible = true;
                gridMain3.RefreshEditor(true);
            }
            dgv3.Refresh();
        }
        /***********************************************************************************************/
        private void btnCurrentDay_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();
            string date1 = date.ToString("yyyy-MM-dd");
            string date2 = date1;
            double amount = dr["amount"].ObjToDouble();
            string bankAccount = dr["bankAccount"].ObjToString();
            string bankDetails = dr["bankDetails"].ObjToString();

            ShowDateData(date, date1, date2, amount, bankAccount, bankDetails);
        }
        /***********************************************************************************************/
        private void btnShowDaySpan_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            DateTime date = dr["date"].ObjToDateTime();
            DateTime tDate = date.AddDays(-1);
            string date1 = tDate.ToString("yyyy-MM-dd");
            tDate = date.AddDays(1);
            string date2 = tDate.ToString("yyyy-MM-dd");
            double amount = dr["amount"].ObjToDouble();
            string bankAccount = dr["bankAccount"].ObjToString();
            string bankDetails = dr["bankDetails"].ObjToString();

            ShowDateData(date, date1, date2, amount, bankAccount, bankDetails);
        }
        /***********************************************************************************************/
        private void btnShowDateRange_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            string date1 = date.ToString("yyyy-MM-dd");
            date = this.dateTimePicker3.Value;
            string date2 = date.ToString("yyyy-MM-dd");

            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            date = dr["date"].ObjToDateTime();
            double amount = dr["amount"].ObjToDouble();
            string bankAccount = dr["bankAccount"].ObjToString();
            string bankDetails = dr["bankDetails"].ObjToString();

            ShowDateData(date, date1, date2, amount, bankAccount, bankDetails);
        }
        /***********************************************************************************************/
        private void btnFindDifference_Click(object sender, EventArgs e)
        {
            DataTable ddt = (DataTable)dgv3.DataSource;

            if (ddt.Rows.Count <= 0)
                return;

            string account = ddt.Rows[0]["bankAccount"].ObjToString();
            string saveAccount = account;
            string bankAccount = account;

            string account2 = bankAccount;
            string account3 = bankAccount;
            string account4 = bankAccount;

            string cmd = "Select * from `bank_accounts` WHERE `account_no` = '" + account + "';";
            DataTable bankDt = G1.get_db_data(cmd);
            if (bankDt.Rows.Count > 0)
            {
                account = bankDt.Rows[0]["location"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();

                account2 = bankDt.Rows[0]["account_title"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
                account3 = bankDt.Rows[0]["localDescription"].ObjToString() + "~" + bankDt.Rows[0]["general_ledger_no"].ObjToString() + "~" + bankDt.Rows[0]["account_no"].ObjToString();
            }


            double systemAmount = 0D;
            double amount = 0D;
            double diff = 0D;

            for (int i = 0; i < ddt.Rows.Count; i++)
            {
                amount += ddt.Rows[i]["amount"].ObjToDouble();
                systemAmount += ddt.Rows[i]["inSystem"].ObjToDouble();
            }
            diff = amount - systemAmount;
            diff = Math.Abs(diff);
            diff = G1.RoundValue(diff);

            DataTable dt = ddt.Clone();

            this.Cursor = Cursors.WaitCursor;

            DateTime date = this.dateTimePicker2.Value;
            string date1 = date.ToString("yyyy-MM-dd");

            string[] Lines = null;
            DataRow[] dRows = null;
            DataRow dRow = null;
            string bankDetails = "";
            string dateStr = "";

            cmd = "Select * from `ipayments` p LEFT JOIN `icustomers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 +"' AND `bank_account` = '" + account + "' AND `paymentAmount` = '" + diff.ToString() + "' ORDER BY `payDate8` asc;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                Lines = null;

                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                    if (amount <= 0D)
                        continue;
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    bankAccount = bankDetails;
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    if (bankAccount == bankAccount)
                    {
                        dRow = dt.NewRow();
                        //dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["payer1"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Insurance";
                        dRow["sDate"] = date.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }
            }

            cmd = "Select * from `payments` p LEFT JOIN `customers` f ON p.`contractNumber` = f.`contractNumber` WHERE `payDate8` >= '" + date1 + "' AND `bank_account` = '" + account + "' AND `paymentAmount` = '" + diff.ToString() + "' ORDER BY `payDate8` asc;";
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count > 0)
            {
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    date = dx.Rows[i]["payDate8"].ObjToDateTime();
                    dateStr = date.ToString("yyyyMMdd");

                    amount = dx.Rows[i]["paymentAmount"].ObjToDouble();
                    if (amount <= 0D)
                        continue;
                    bankDetails = dx.Rows[i]["bank_account"].ObjToString();
                    bankAccount = bankDetails;
                    Lines = bankDetails.Split('~');
                    if (Lines.Length >= 3)
                        bankDetails = Lines[2].Trim();

                    if (bankAccount == bankAccount)
                    {
                        dRow = dt.NewRow();
                        //dRow["date"] = G1.DTtoMySQLDT(searchDate);
                        dRow["date"] = G1.DTtoMySQLDT(date);
                        dRow["amount"] = amount;
                        dRow["bankAccount"] = bankDetails;
                        dRow["ID"] = dx.Rows[i]["contractNumber"].ObjToString();
                        dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                        dRow["found"] = "Insurance";
                        dRow["sDate"] = date.ToString("yyyyMMdd");
                        dRow["record"] = dx.Rows[i]["record"].ObjToString();
                        dt.Rows.Add(dRow);
                    }
                }
            }

            double lossRecovery = 0D;

            cmd = "Select * from `downpayments`  WHERE `date` >= '" + date1 + "' AND `bankAccount` = '" + account4 + "' AND `downPayment` = '" + diff.ToString() + "' ;";
            dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                date = dx.Rows[i]["date"].ObjToDateTime();
                dateStr = date.ToString("yyyyMMdd");
                amount = dx.Rows[i]["downPayment"].ObjToDouble();
                lossRecovery = dx.Rows[i]["lossRecoveryFee"].ObjToDouble();
                amount += lossRecovery;

                bankDetails = dx.Rows[i]["bankAccount"].ObjToString();
                bankAccount = bankDetails;
                Lines = bankDetails.Split('~');
                if (Lines.Length >= 3)
                    bankDetails = Lines[2].Trim();

                if (bankAccount == account3)
                {
                    dRow = dt.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["amount"] = amount;
                    dRow["bankAccount"] = bankDetails;
                    dRow["ID"] = dx.Rows[i]["depositNumber"].ObjToString();
                    dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                    dRow["found"] = "Down Payment";
                    dRow["sDate"] = date.ToString("yyyyMMdd");
                    dRow["record"] = dx.Rows[i]["record"].ObjToString();
                    dt.Rows.Add(dRow);
                }
            }

            string depositNumber = "";
            cmd = "Select * from `cust_payment_details` c JOIN `fcust_extended` f ON c.`contractNumber` = f.`contractNumber` WHERE `dateReceived` >= '" + date1 + "' AND `bankAccount` = '" + saveAccount + "' AND `amtActuallyReceived` = '" + diff.ToString() + "' ;";
            dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                date = dx.Rows[i]["dateReceived"].ObjToDateTime();
                dateStr = date.ToString("yyyyMMdd");
                amount = dx.Rows[i]["amtActuallyReceived"].ObjToDouble();
                if (amount <= 0D)
                    amount = dx.Rows[i]["paid"].ObjToDouble();
                bankDetails = dx.Rows[i]["bankAccount"].ObjToString();
                bankAccount = bankDetails;
                depositNumber = dx.Rows[i]["depositNumber"].ObjToString();
                if (depositNumber.ToUpper().IndexOf("TD") == 0 || depositNumber.ToUpper().IndexOf("CCTD") == 0)
                {
                    dRows = dt.Select("depositNumber='" + depositNumber + "' AND found = 'Down Payment' AND sDate = '" + dateStr + "'");
                    if (dRows.Length > 0)
                        continue;
                }

                if (bankAccount == saveAccount)
                {
                    dRow = dt.NewRow();
                    dRow["date"] = G1.DTtoMySQLDT(date);
                    dRow["amount"] = amount;
                    dRow["bankAccount"] = bankDetails;
                    dRow["ID"] = dx.Rows[i]["serviceId"].ObjToString();
                    dRow["depositNumber"] = dx.Rows[i]["depositNumber"].ObjToString();
                    dRow["found"] = "Funeral";
                    dRow["sDate"] = date.ToString("yyyyMMdd");
                    dRow["record"] = dx.Rows[i]["record"].ObjToString();
                    dt.Rows.Add(dRow);
                }
            }

            string title = "Searching for $" + diff.ToString() + " in Bank Account " + account4 + " from " + date1;

            DataTable saveDt = (DataTable) dgv3.DataSource;

            ImportBankDetails bankForm = new ImportBankDetails(saveDt, dt, this.dateTimePicker2.Value, title);

            bankForm.Show();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void fixDateForSelectedRowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            string ID = dr["ID"].ObjToString();
            if (ID.ToUpper() == "IN BANK")
            {
                MessageBox.Show("*** INFO *** Cannot fix Bank Deposit Dates!\nOnly Deposit Dates in the System!", "Fix Data Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            bool gotDate = false;
            DateTime date = dr["date"].ObjToDateTime();
            DateTime newDate = date;
            using (GetDate dateForm = new GetDate(date, "Select Date Desired"))
            {
                dateForm.TopMost = true;
                dateForm.ShowDialog();
                if (dateForm.DialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    gotDate = true;
                    newDate = dateForm.myDateAnswer;
                }
            }
            if (!gotDate)
                return;

            DataRow drr = null;
            int row = -1;
            int rowIndex = -1;
            string found = "";
            string record = "";
            int[] rows = gridMain3.GetSelectedRows();
            try
            {
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    rowIndex = gridMain3.GetDataSourceRowIndex(row);
                    drr = dt.Rows[rowIndex];
                    ID = drr["ID"].ObjToString();
                    date = drr["date"].ObjToDateTime();
                    found = drr["found"].ObjToString().Trim().ToUpper();
                    dt.Rows[rowIndex]["date"] = G1.DTtoMySQLDT(newDate);
                    dt.Rows[rowIndex]["sDate"] = newDate.ToString("yyyyMMdd");
                    record = drr["record"].ObjToString();
                    if ( found == "INSURANCE" && !String.IsNullOrWhiteSpace ( record ))
                        G1.update_db_table("ipayments", "record", record, new string[] { "payDate8", newDate.ToString("yyyy-MM-dd") });
                    else if (found == "TRUST" && !String.IsNullOrWhiteSpace(record))
                        G1.update_db_table("payments", "record", record, new string[] { "payDate8", newDate.ToString("yyyy-MM-dd") });
                    else if (found == "FUNERAL" && !String.IsNullOrWhiteSpace(record))
                        G1.update_db_table("cust_payment_details", "record", record, new string[] { "dateReceived", newDate.ToString("yyyy-MM-dd") });
                }
                gridMain3.RefreshEditor(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            date = date.AddDays(-1);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker2.Value;
            date = date.AddDays(1);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void gridMain3_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            if (G1.get_column_number(dt, "Green") < 0)
                return;
            if (e.RowHandle < 0)
                return;

            int row = gridMain3.GetDataSourceRowIndex(e.RowHandle);

            string found = dt.Rows[row]["found"].ObjToString().ToUpper();

            if (e.Column.FieldName.ToUpper() == "AMOUNT")
            {
                if (found != "Z INBANK")
                {
                    string green = dt.Rows[row]["Green"].ObjToString();
                    if (green.ToUpper() == "Y")
                        e.Appearance.BackColor = Color.LightGreen;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "INSYSTEM")
            {
                if (found == "Z INBANK")
                {
                    string green = dt.Rows[row]["Green"].ObjToString();
                    if (green.ToUpper() == "Y")
                        e.Appearance.BackColor = Color.LightGreen;
                }
            }
        }
        /***********************************************************************************************/
        private void dateTimePicker3_ValueChanged(object sender, EventArgs e)
        {
            DateTime date1 = this.dateTimePicker2.Value;
            DateTime date2 = this.dateTimePicker3.Value;
            if (date2 < date1)
                this.dateTimePicker3.Value = date1;
        }
        /***********************************************************************************************/
    }
}