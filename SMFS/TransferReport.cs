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
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class TransferReport : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private DataTable workDt = null;
        private DateTime workDate = DateTime.Now;
        private DataTable workLocationDt = null;
        /****************************************************************************************/
        public TransferReport( DataTable dt, DateTime date, DataTable locationDt )
        {
            InitializeComponent();
            workDt = dt;
            workDate = date;
            workLocationDt = locationDt;

            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void TransferReport_Load(object sender, EventArgs e)
        {
            loading = false;
            this.dateTimePicker1.Value = workDate;
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("beginningBalance", null);
            AddSummaryColumn("transfer", null);
            AddSummaryColumn("endingBalance", null);
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName)
        {
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            //gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = "{0:N2}";
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
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (loading)
                return;
            modified = true;
            DataRow dr = gridMain.GetFocusedDataRow();
//            dr["mod"] = "Y";
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;
            //string delete = dt.Rows[row]["mod"].ObjToString();
            //if (delete.ToUpper() == "D")
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
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
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "YEAR" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                string year = e.DisplayText;
                year = year.Replace(",", "");
                e.DisplayText = year;
            }
        }
        /****************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            DataTable dt = workDt.Copy();

            DataView tempview = dt.DefaultView;
            tempview.Sort = "accountTitle asc, date asc";
            dt = tempview.ToTable();

            DataTable dx = new DataTable();
            dx.Columns.Add("bank");
            dx.Columns.Add("location");
            dx.Columns.Add("beginningBalance", Type.GetType("System.Double"));
            dx.Columns.Add("transfer", Type.GetType("System.Double"));
            dx.Columns.Add("endingBalance", Type.GetType("System.Double"));
            dx.Columns.Add("checkNum");
            dx.Columns.Add("balance", Type.GetType("System.Double"));


            DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["accountTitle"] }).Select(g => g.OrderBy(r => r["accountTitle"]).First()).CopyToDataTable();

            string location = "";
            string oldLocation = "";
            double oldBalance = 0D;
            double lastBalance = 0D;
            double beginningBalance = 0D;
            double endingBalance = 0D;
            double actualBalance = 0D;
            double transfer = 0D;

            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker1.Value;
            DateTime date = DateTime.Now;

            string cmd = "Select * from `funeralReports`;";
            DataTable funReportDt = G1.get_db_data(cmd);


            DataRow[] dRows = null;
            DataRow dRow = null;
            DataTable newDt = null;
            string comment = "";
            string str = "";
            string loc = "";
            string bank = "";
            string temp = "";
            string accountNumber = "";
            string glNumber = "";
            string bankName = "";
            string fullBankDetails = "";
            string cashLocal = "";
            bool stop = false;
            int idx = 0;
            DataTable funDt = null;
            string[] Lines = null;

            for ( int i=0; i<groupDt.Rows.Count; i++)
            {
                try
                {
                    location = groupDt.Rows[i]["accountTitle"].ObjToString();

                    loc = location;

                    loc = loc.Replace("Cash", "");
                    loc = loc.Trim();
                    dRows = workLocationDt.Select("localDescription='" + loc + "'");
                    if (dRows.Length <= 0)
                        continue;

                    accountNumber = dRows[0]["account_no"].ObjToString();
                    glNumber = dRows[0]["general_ledger_no"].ObjToString();
                    bankName = dRows[0]["location"].ObjToString();

                    fullBankDetails = bankName + "~" + glNumber + "~" + accountNumber;

                    bankName = "";
                    cashLocal = "";

                    cmd = "Select * from `funeralHomes` where `cashLocal` LIKE '" + glNumber + "/" + accountNumber + "%';";
                    funDt = G1.get_db_data(cmd);
                    if (funDt.Rows.Count > 0)
                    {
                        for (int j = 0; j < funDt.Rows.Count; j++)
                        {
                            str = funDt.Rows[j]["locationCode"].ObjToString();
                            if (location.IndexOf(str) >= 0)
                            {
                                bankName = str;
                                cashLocal = funDt.Rows[j]["cashLocal"].ObjToString();
                                break;
                            }
                            else if ( str == "Brookhaven" && location.IndexOf ("B’ Haven") > 0 )
                            {
                                bankName = str;
                                cashLocal = funDt.Rows[j]["cashLocal"].ObjToString();
                                break;
                            }
                            else
                            {
                                idx = location.IndexOf("-");
                                if ( idx > 0 )
                                {
                                    temp = location.Substring(idx + 1).Trim();
                                    if (location.IndexOf(temp) >= 0)
                                    {
                                        bankName = temp;
                                        cashLocal = funDt.Rows[j]["cashLocal"].ObjToString();
                                        break;
                                    }
                                    else if ( temp.IndexOf ( "Fisher") >= 0 )
                                    {
                                        bankName = "Fisher";
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        idx = location.IndexOf("-");
                        if (idx > 0)
                        {
                            temp = location.Substring(idx + 1).Trim();
                            dRows = funReportDt.Select("reportName='" + temp + "'");
                            if (dRows.Length > 0)
                                bankName = temp;
                            else if (temp.IndexOf("Fisher") >= 0)
                                bankName = "Fisher";
                        }
                        else
                        {
                            Lines = location.Split(' ');
                            if ( Lines.Length > 0 )
                            {
                                bankName = Lines[Lines.Length - 1].Trim();
                            }
                        }
                    }

                    dRows = dt.Select("accountTitle='" + location + "'");
                    if (dRows.Length <= 0)
                        continue;
                    newDt = dRows.CopyToDataTable();
                    lastBalance = -999.99D;
                    beginningBalance = -999.99D;
                    endingBalance = -999.99D;
                    actualBalance = -999.99D;
                    stop = false;
                    transfer = 0D;
                    comment = "";
                    for ( int j=0; j<newDt.Rows.Count; j++)
                    {
                        date = newDt.Rows[j]["date"].ObjToDateTime();
                        if ( date == startDate )
                            actualBalance = newDt.Rows[j]["balance"].ObjToDouble();
                        //if ( date == startDate && beginningBalance == -999.99D)
                        //    beginningBalance = newDt.Rows[j]["balance"].ObjToDouble();
                        if ( date >= startDate && date <= stopDate )
                        {
                            stop = true;
                            if (beginningBalance == -999.99D)
                                beginningBalance = newDt.Rows[j]["balance"].ObjToDouble();
                            transfer += newDt.Rows[j]["transfers"].ObjToDouble();
                            if ( transfer > 0D && endingBalance == -999.99D )
                                endingBalance = newDt.Rows[j]["balance"].ObjToDouble();
                            str = newDt.Rows[j]["comment"].ObjToString();
                            comment = str;
                        }
                        if ( !stop )
                            lastBalance = newDt.Rows[j]["balance"].ObjToDouble();
                        if (date > stopDate)
                            break;
                    }
                    if (beginningBalance == -999.99D)
                        beginningBalance = lastBalance;
                    if (actualBalance == -999.99D)
                        actualBalance = lastBalance;
                    if (endingBalance == -999.99D)
                        endingBalance = beginningBalance;

                    dRow = dx.NewRow();
                    dRow["bank"] = location;
                    dRow["location"] = bankName;
                    dRow["beginningBalance"] = beginningBalance;
                    if (transfer > 0D)
                        dRow["beginningBalance"] = lastBalance;
                    dRow["endingBalance"] = endingBalance;
                    dRow["transfer"] = transfer;
                    dRow["balance"] = actualBalance;
                    if ( !String.IsNullOrWhiteSpace ( comment ))
                    {
                        Lines = comment.Split('~');
                        if ( Lines.Length > 0 )
                        {
                            comment = Lines[0].Trim();
                            comment = locateString(comment);
                        }
                    }
                    dRow["checkNum"] = comment;
                    dx.Rows.Add(dRow);
                }
                catch ( Exception ex)
                {
                }
            }

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
        }
        /****************************************************************************************/
        private string locateString ( string str )
        {
            if (String.IsNullOrWhiteSpace(str))
                return "";
            string newStr = "";
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (c == 0)
                    break;
                if ( c == ' ')
                {
                    if (!String.IsNullOrWhiteSpace(newStr))
                        newStr += " ";
                    continue;
                }
                if (c < '0' || c > '9')
                    continue;
                newStr += c.ToString();
            }
            return newStr;
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
            printableComponentLink1.Landscape = false;

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

            DataTable ddd = (DataTable)dgv.DataSource;

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
            printableComponentLink1.Landscape = false;

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

            //            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;
            Printer.DrawQuad(6, 7, 4, 3, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            DateTime date = this.dateTimePicker1.Value;
            string workDate = date.ToString("MM/dd/yyyy");
            date = this.dateTimePicker1.Value;
            workDate += " - " + date.ToString("MM/dd/yyyy");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(17, 8, 10, 4, "Run Dates - " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddDays(1);
            this.dateTimePicker1.Value = date;
        }
        /****************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime date = this.dateTimePicker1.Value;
            date = date.AddDays(-1);
            this.dateTimePicker1.Value = date;
        }
        /****************************************************************************************/
    }
}