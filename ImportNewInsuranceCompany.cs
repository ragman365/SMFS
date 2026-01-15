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
using MySql.Data.MySqlClient;
using System.Web.UI.WebControls;
using DevExpress.XtraGrid.Columns;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ImportNewInsuranceCompany : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        /* Forethough - Import All Active Data. This code determines Pre or Post */
        /* FDLIC - Import All - FDLIC PB is included all together
        /***********************************************************************************************/
        private string workTable = "contacts_preneed_mapping";
        private string workMap = "preneedMuse";
        private DataTable workTableDt = null;
        private DataTable existingDt = null;

        private DataTable workDt = null;
        private string workWhat = "";
        private bool workDC = false;
        private string title = "";

        private DataTable problemDt = null;
        /***********************************************************************************************/
        public ImportNewInsuranceCompany()
        {
            InitializeComponent();
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;

            if (String.IsNullOrWhiteSpace(format))
                format = "{0:N2}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, string format = "")
        {
            if (String.IsNullOrWhiteSpace(format))
                format = "{0:0,0.00}";
            gridMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gridMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void ImportNewInsuranceCompany_Load(object sender, EventArgs e)
        {
            barImport.Hide();

            btnCreateNew.Hide();
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
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataRow dr = gridMain.GetFocusedDataRow();
            string payer = dr["Payer"].ObjToString();
            string contract = dr["Exists"].ObjToString();

            if ( contract.IndexOf ( "P-" ) == 0 )
            {
                contract = contract.Replace("P-", "").Trim();
                string cmd = "Select * from `icustomers` where `payer` = '" + contract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    contract = dx.Rows[0]["contractNumber"].ObjToString();
            }

            if (!String.IsNullOrWhiteSpace(contract) && contract != "NO" )
            {
                this.Cursor = Cursors.WaitCursor;
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
            else
            {
                string cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    contract = dx.Rows[0]["contractNumber"].ObjToString();
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
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
            string text = this.Text;
            Printer.DrawQuad(5, 7, 5, 4, text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

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

            printableComponentLink1.CreateDocument();
            printableComponentLink1.PrintDlg();
            isPrinting = false;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
        }
        /***********************************************************************************************/
        private void CleanupCommas ( DataTable dt, string column )
        {
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i][column].ObjToString();
                if ( str.IndexOf ( "$") >= 0 )
                {
                    str = str.Replace("$", "");
                    dt.Rows[i][column] = str;
                }
                if (String.IsNullOrWhiteSpace(str))
                    dt.Rows[i][column] = "0";
                else if ( str.IndexOf ( ",") > 0 )
                {
                    str = str.Replace(",", "");
                    dt.Rows[i][column] = str;
                }
            }
        }
        /***********************************************************************************************/
        private string actualFile = "";
        private string importedFile = "";
        private void btnImport_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string sheetName = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    importedFile = file;
                    int idx = file.LastIndexOf("\\");
                    if (idx > 0)
                    {
                        actualFile = file.Substring(idx);
                        actualFile = actualFile.Replace("\\", "");
                    }

                    dgv.DataSource = null;
                    this.Cursor = Cursors.WaitCursor;
                    workDt = null;
                    string str = "";

                    try
                    {
                        string tab = txtWhat.Text.Trim();
                        if ( String.IsNullOrWhiteSpace ( tab))
                            tab = "Sheet2";
                        workDt = ExcelWriter.ReadFile2(file, 0, tab );
                        for (int i = 0; i < workDt.Columns.Count; i++)
                        {
                            str = workDt.Rows[0][i].ObjToString();
                            if (String.IsNullOrWhiteSpace(str))
                                continue;
                            if (G1.get_column_number(workDt, str) >= 0)
                                str = str + "2";
                            workDt.Columns[i].ColumnName = str;
                            workDt.Columns[i].Caption = str;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** Reading File!");
                        workDt = null;
                    }

                    workDt.TableName = actualFile;

                    if (G1.get_column_number(workDt, "num") < 0)
                        workDt.Columns.Add("num").SetOrdinal(0);

                    workDt.Rows.RemoveAt(0);

                    G1.NumberDataTable(workDt);

                    ProcessPulledData(workDt);

                    dgv.DataSource = workDt;

                    gridMain.Columns["num"].Width = 60;
                }
            }

            btnCreateNew.Show();
            btnCreateNew.Refresh();

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable ProcessPulledData ( DataTable dt )
        {
            gridMain.BeginUpdate();

            gridMain.Columns.Clear();
            gridMain.EndUpdate();

            dt.Columns.RemoveAt(1);

            int numCol = G1.get_column_number(dt, "num");
            dt.Columns.Add("Exists").SetOrdinal(1);

            string cmd = "";
            DataTable dx = null;

            string payer = "";
            string contract = "";
            string str = "";
            bool found = false;
            int firstRow = 0;
            string payerCol = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                payer = dt.Rows[i][2].ObjToString().Trim();
                if ( payer == "Payor #" || payer == "Payer #" )
                {
                    try
                    {
                        payerCol = "Payer";
                        dt.Columns[2].ColumnName = "Payer";
                        dt.Columns[2].Caption = "Payer";
                        found = true;
                        firstRow = i + 1;
                        for (int j = 3; j < dt.Columns.Count; j++)
                        {
                            try
                            {
                                str = dt.Rows[i][j].ObjToString().Trim();
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    if (G1.get_column_number(dt, str) >= 0)
                                        str += " 2";
                                    dt.Columns[j].ColumnName = str;
                                }
                            }
                            catch ( Exception ex )
                            {
                            }
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                    break;
                }
            }

            ConvertExcelDate(workDt, "Policy Issue Date");
            ConvertExcelDate(workDt, "Due Date");
            ConvertExcelDate(workDt, "Deceased Insured Date Of Death");

            dt.Columns.Add("Pay").SetOrdinal(2);
            dt.Columns.Add("Con").SetOrdinal(3);
            dt.Columns.Add("Cus").SetOrdinal(4);

            string tab = txtWhat.Text.Trim();

            string lookFor = "CA-";
            if (tab.ToUpper().IndexOf("EV") == 0)
                lookFor = "EV-";

            for ( int i=firstRow; i<dt.Rows.Count; i++)
            {
                try
                {
                    payer = dt.Rows[i]["Payer"].ObjToString().Trim();
                    if (payer.IndexOf( lookFor ) < 0)
                        continue;
                    cmd = "Select * from `payers` WHERE `payer` = '" + payer + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                    {
                        cmd = "Select * from `policies` where `payer` = '" + payer + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            contract = SearchPayer(payer);
                            dt.Rows[i]["Exists"] = contract;
                        }
                        else
                            dt.Rows[i]["Exists"] = "P-" + dx.Rows[0]["payer"].ObjToString();
                    }
                    else
                        dt.Rows[i]["Pay"] = dx.Rows[0]["contractNumber"].ObjToString();

                    cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                    dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        contract = dx.Rows[0]["contractNumber"].ObjToString();
                        dt.Rows[i]["Cus"] = contract;
                        cmd = "Select * from `icontracts` where `contractNumber` = '" + contract + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            dt.Rows[i]["Con"] = dx.Rows[0]["contractNumber"].ObjToString();
                    }
                }
                catch ( Exception ex)
                {
                }
            }

            for (int i = firstRow - 1; i >= 0; i--)
                dt.Rows.RemoveAt(i);

            string pay = "";
            string cus = "";
            string con = "";

            for (int i = dt.Rows.Count-1; i >= 0; i--)
            {
                try
                {
                    payer = dt.Rows[i]["Payer"].ObjToString();
                    if (String.IsNullOrWhiteSpace(payer))
                        dt.Rows.RemoveAt(i);
                    else
                    {
                        pay = dt.Rows[i]["Pay"].ObjToString();
                        cus = dt.Rows[i]["Cus"].ObjToString();
                        con = dt.Rows[i]["Con"].ObjToString();
                        if (String.IsNullOrWhiteSpace(pay) && String.IsNullOrWhiteSpace(cus) && String.IsNullOrWhiteSpace(con))
                            dt.Rows.RemoveAt(i);
                    }
                }
                catch ( Exception ex )
                {
                }
            }

            G1.NumberDataTable(dt);
            return dt;
        }
        /***********************************************************************************************/
        private string SearchPayer ( string payer )
        {
            string cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count > 0 )
            {
                return dx.Rows[0]["contractNumber"].ObjToString();
            }
            return "NO";
        }
        /***********************************************************************************************/
        private void ConvertExcelDate ( DataTable dt, string column )
        {
            if (G1.get_column_number(dt, column) < 0)
                return;
            DateTime date = DateTime.Now;
            double str = 0;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                str = dt.Rows[i][column].ObjToDouble();
                date = DateTime.FromOADate(str);

                if ( date.Year > 1900 )
                    dt.Rows[i][column] = date.ToString("MM/dd/yyyy");
            }
        }
        /***********************************************************************************************/
        private DataTable mapTheColumns ( DataTable dt )
        {
            dt = MapColumn(dt, "Funeral Home", "funeralHome");
            dt = MapColumn(dt, "SMFS Agent", "agent");
            dt = MapColumn(dt, "Title", "prefix");
            dt = MapColumn(dt, "First Name", "firstName");
            dt = MapColumn(dt, "Last Name", "lastName");
            dt = MapColumn(dt, "Middle Name / Initial", "middleName");
            dt = MapColumn(dt, "Mobile Phone Number", "mobilePhone");
            dt = MapColumn(dt, "Home Phone Number", "homePhone");
            dt = MapColumn(dt, "Work Phone Number", "workPhone");
            dt = MapColumn(dt, "Address 2", "address2");
            dt = MapColumn(dt, "Lead Source", "leadSource");
            dt = MapColumn(dt, "Prospect Creation Date", "prospectCreationDate");
            dt = MapColumn(dt, "Contact Status / Interest Level", "contactStatus");
            dt = MapColumn(dt, "Last Touch Date", "lastTouchDate");
            dt = MapColumn(dt, "Last Touch Time", "lastTouchTime");
            dt = MapColumn(dt, "Last Touch Activity", "lastTouchActivity");
            dt = MapColumn(dt, "Last Touch Result", "lastTouchResult");
            dt = MapColumn(dt, "Next Scheduled Touch Date", "nextScheduledTouchDate");
            dt = MapColumn(dt, "Next Scheduled Touch Time", "nextScheduledTouchTime");
            dt = MapColumn(dt, "Next Touch Result", "nextTouchResult");
            dt = MapColumn(dt, "Scheduled Activity", "scheduledActivity");
            dt = MapColumn(dt, "Total # Touches Made", "totalTouches");
            dt = MapColumn(dt, "Reference Funeral #", "referenceFuneral");
            dt = MapColumn(dt, "Reference Deceased Title", "refDeceasedPrefix");
            dt = MapColumn(dt, "Reference Deceased First Name", "refDeceasedFirstName");
            dt = MapColumn(dt, "Reference Deceased Middle Name", "refDeceasedMiddleName");
            dt = MapColumn(dt, "Reference Deceased Last Name", "refDeceasedLastName");
            dt = MapColumn(dt, "Reference Deceased Suffix", "refDeceasedSuffix");
            dt = MapColumn(dt, "Prospect Relationship to Reference Funeral", "funeralRelationship");
            dt = MapColumn(dt, "Reference Trust #", "referenceTrust");
            dt = MapColumn(dt, "Special Meeting", "specialMeeting");
            return dt;
        }
        /***********************************************************************************************/
        private DataTable MapColumn ( DataTable dt, string fromCol, string toCol )
        {
            try
            {
                if (G1.get_column_number(dt, fromCol) >= 0)
                {
                    if (G1.get_column_number(dt, toCol) < 0)
                    {
                        dt.Columns[fromCol].ColumnName = toCol;
                        dt.Columns[toCol].Caption = dt.Columns[toCol].ColumnName.ObjToString().Trim();
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            catch ( Exception ex )
            {
            }
            return dt;
        }
        /***********************************************************************************************/
        private void btnCreateNew_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataTable payDt = null;
            DataTable conDt = null;
            DataTable cusDt = null;
            DataTable policyDt = null;

            string payer = "";
            string oldPayer = "";

            string pay = "";
            string cus = "";
            string con = "";

            string contract = "";
            string contractNumber = "";
            string policyNumber = "";

            string record = "";
            string cmd = "";

            string payerFirstName = "";
            string payerLastName = "";
            string payerSuffix = "";
            string firstName = "";
            string lastName = "";
            string suffix = "";

            string payerPhone = "";
            string payerPhone2 = "";

            string insuredFirstName = "";
            string insuredLastName = "";
            string insuredSuffix = "";

            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip = "";

            double liability = 0D;
            double premium = 0D;
            double monthlyPremium = 0D;
            double yearlyPremium = 0D;
            double historicPremium = 0D;

            string issueDate = "";
            string dueDate = "";
            string deceasedDate = "";
            string payerDeceasedDate = "";
            string version = "";

            string companyCode = txtCC.Text;

            DateTime dDate = DateTime.Now;

            int lastRow = dt.Rows.Count - 1;
            //lastRow = 5;

            cmd = "Select * from `icustomers` ORDER by `contractNumber` DESC LIMIT 1;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;

            contract = dx.Rows[0]["contractNumber"].ObjToString();
            contract = contract.Replace("ZZ", "");

            bool justPolicies = chkJustPolicies.Checked;

            int totalCustomers = contract.ObjToInt32();
            //totalCustomers = totalCustomers + 1;

            //int totalCustomers = 34574;


            for ( int i=0; i<lastRow; i++)
            {

                try
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    policyNumber = dt.Rows[i]["Policy Number"].ObjToString();

                    pay = dt.Rows[i]["Pay"].ObjToString();
                    cus = dt.Rows[i]["Cus"].ObjToString();
                    con = dt.Rows[i]["Con"].ObjToString();

                    firstName = dt.Rows[i]["Payer First Name"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(firstName))
                        payerFirstName = firstName;
                    lastName = dt.Rows[i]["Payer Last Name"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(lastName))
                        payerLastName = lastName;
                    suffix = dt.Rows[i]["Payer Suffix"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(suffix))
                        payerSuffix = suffix;

                    payerPhone = dt.Rows[i]["Payor Phone"].ObjToString();
                    payerPhone2 = dt.Rows[i]["Payor Phone 2"].ObjToString();

                    insuredFirstName = dt.Rows[i]["Policy Insured First Name"].ObjToString();
                    insuredLastName = dt.Rows[i]["Policy Insured Last Name"].ObjToString();
                    insuredSuffix = dt.Rows[i]["Policy Insured Suffix"].ObjToString();

                    address1 = dt.Rows[i]["Payer Address 1"].ObjToString();
                    address2 = dt.Rows[i]["Payer Address 2"].ObjToString();
                    city = dt.Rows[i]["Payer City"].ObjToString();
                    state = dt.Rows[i]["Payer State"].ObjToString();
                    zip = dt.Rows[i]["Payer Zip"].ObjToString();

                    liability = dt.Rows[i]["Policy Liability"].ObjToDouble();
                    premium = dt.Rows[i]["Policy Premium"].ObjToDouble();
                    monthlyPremium = dt.Rows[i]["Payer Monthly Premium"].ObjToDouble();
                    yearlyPremium = dt.Rows[i]["Payer Annual Premium (5 % Disc)"].ObjToDouble();

                    historicPremium = dt.Rows[i]["Policy Historic Premium"].ObjToDouble();

                    issueDate = dt.Rows[i]["Policy Issue Date"].ObjToString().Trim();
                    dueDate = dt.Rows[i]["Due Date"].ObjToString();
                    deceasedDate = dt.Rows[i]["Deceased Insured Date Of Death"].ObjToString();

                    payerDeceasedDate = "";
                    if (monthlyPremium <= 0D)
                        payerDeceasedDate = deceasedDate;

                    if (payer != oldPayer)
                    {
                        cmd = "Select * from `payers` WHERE `payer` = '" + payer + "';";

                        //payDt = G1.get_db_data(cmd);

                        //if (payDt.Rows.Count > 0)
                        //{
                        //    contract = payDt.Rows[0]["contractNumber"].ObjToString();
                        //    if (contract.IndexOf("ZZ") != 0)
                        //    {

                        //        payDt.Rows.Clear();
                        //    }
                        //    else
                        //    {
                        //        version = payDt.Rows[0]["Version"].ObjToString();
                        //        if (version != "2.0")
                        //        {
                        //            MessageBox.Show("***ERROR*** Got Old Payer ZZ and not Version 2.0", "Possible Version Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        //        }
                        //    }
                        //}

                        //if (payDt.Rows.Count <= 0)
                        //    record = G1.create_record("payers", "version", "2.0");
                        //else
                        //    record = payDt.Rows[0]["record"].ObjToString();

                        totalCustomers++;
                        contractNumber = "ZZ" + totalCustomers.ToString("D7");

                        cmd = "Select * from `payers` WHERE `payer` = '" + payer + "';";
                        if ( justPolicies )
                        {
                            cmd = "Select * from `payers` WHERE `payer` = '" + payer + "' AND `version` = '2.0';";
                            dx = G1.get_db_data(cmd);
                            if ( dx.Rows.Count <= 0 )
                            {
                                MessageBox.Show("***ERROR*** Problem looking up Payer " + payer + " for Version 2.0", "Possible Version Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                                continue;
                            }
                            contractNumber = dx.Rows[0]["contractNumber"].ObjToString();
                        }
                        else
                        {
                            record = G1.create_record("payers", "version", "2.0");
                            G1.update_db_table("payers", "record", record, new string[] { "contractNumber", contractNumber, "payer", payer, "firstName", payerFirstName, "lastName", payerLastName, "amtOfMonthlyPayt", monthlyPremium.ToString(), "annualPremium", yearlyPremium.ToString(), "dueDate8", dueDate, "deceasedDate", payerDeceasedDate });
                        }

                        cmd = "Select * from `icustomers` WHERE `contractNumber` = '" + contractNumber + "';";
                        //cusDt = G1.get_db_data(cmd);
                        //if ( cusDt.Rows.Count > 0 )
                        //{
                        //    contract = cusDt.Rows[0]["contractNumber"].ObjToString();
                        //    if (contract.IndexOf("ZZ") != 0)
                        //        cusDt.Rows.Clear();
                        //    else
                        //    {
                        //        version = cusDt.Rows[0]["Version"].ObjToString();
                        //        if (version != "2.0")
                        //        {
                        //            MessageBox.Show("***ERROR*** Got Old Customer " + contract + " and not Version 2.0", "Possible Version Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        //        }
                        //    }
                        //}
                        //if (cusDt.Rows.Count <= 0)
                        //    record = G1.create_record("icustomers", "version", "2.0");
                        //else
                        //    record = cusDt.Rows[0]["record"].ObjToString();

                        if (!justPolicies)
                        {
                            record = G1.create_record("icustomers", "version", "2.0");
                            G1.update_db_table("icustomers", "record", record, new string[] { "contractNumber", contractNumber, "payer", payer, "firstName", payerFirstName, "lastName", payerLastName, "suffix", payerSuffix, "address1", address1, "address2", address2, "city", city, "state", state, "zip1", zip, "phoneNumber1", payerPhone, "phoneNumber2", payerPhone2, "deceasedDate", payerDeceasedDate });
                        }

                        cmd = "Select * from `icontracts` WHERE `contractNumber` = '" + contractNumber + "';";
                        //conDt = G1.get_db_data(cmd);
                        //if (cusDt.Rows.Count > 0)
                        //{
                        //    contract = conDt.Rows[0]["contractNumber"].ObjToString();
                        //    if (contract.IndexOf("ZZ") != 0)
                        //        conDt.Rows.Clear();
                        //    else
                        //    {
                        //        version = conDt.Rows[0]["Version"].ObjToString();
                        //        if (version != "2.0")
                        //        {
                        //            MessageBox.Show("***ERROR*** Got Old Contract " + contract + " and not Version 2.0", "Possible Version Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        //        }
                        //    }
                        //}
                        //if (conDt.Rows.Count <= 0)
                        //    record = G1.create_record("icontracts", "version", "2.0");
                        //else
                        //    record = conDt.Rows[0]["record"].ObjToString();

                        if (!justPolicies)
                        {
                            record = G1.create_record("icontracts", "version", "2.0");
                            G1.update_db_table("icontracts", "record", record, new string[] { "contractNumber", contractNumber, "amtOfMonthlyPayt", monthlyPremium.ToString(), "annualPremium", yearlyPremium.ToString(), "dueDate8", dueDate, "deceasedDate", payerDeceasedDate, "issueDate8", issueDate });
                        }
                        oldPayer = payer;
                    }

                    if (String.IsNullOrWhiteSpace(policyNumber))
                        continue;

                    cmd = "Select * from `policies` WHERE `contractNumber` = '" + contractNumber + "' AND `policyNumber` = '" + policyNumber + "';";
                    if ( justPolicies )
                        cmd = "Select * from `policies` WHERE `contractNumber` = '" + contractNumber + "' AND `policyNumber` = '" + policyNumber + "' AND `version` = '2.0';";
                    policyDt = G1.get_db_data(cmd);
                    if (policyDt.Rows.Count > 0)
                    {
                        contract = policyDt.Rows[0]["contractNumber"].ObjToString();
                        if (contract.IndexOf("ZZ") != 0)
                            policyDt.Rows.Clear();
                        else
                        {
                            version = policyDt.Rows[0]["Version"].ObjToString();
                            if (version != "2.0")
                            {
                                MessageBox.Show("***ERROR*** Got Old Policy " + contract + " and not Version 2.0", "Possible Version Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                            }
                        }
                    }
                    if (policyDt.Rows.Count <= 0)
                        record = G1.create_record("policies", "version", "2.0");
                    else
                        record = policyDt.Rows[0]["record"].ObjToString();

                    G1.update_db_table("policies", "record", record, new string[] { "contractNumber", contractNumber, "payer", payer, "policyNumber", policyNumber, "firstName", payerFirstName, "lastName", payerLastName, "policyFirstName", insuredFirstName, "policyLastName", insuredLastName, "premium", premium.ToString(), "historicPremium", historicPremium.ToString(), "dueDate8", dueDate, "deceasedDate", deceasedDate, "issueDate8", issueDate, "companyCode", companyCode, "liability", liability.ToString(), "report", "Not Third Party" });
                }
                catch ( Exception ex)
                {
                }
            }
            MessageBox.Show("***INFO*** Finished", "Finished Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
        }
        /***********************************************************************************************/
        private void deleteAllVersion20DataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                this.Cursor = Cursors.WaitCursor;
                string record = "";
                string cmd = "Select * from `payers` WHERE `version` = '2.0'";
                DataTable dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("payers", "record", record);
                }

                cmd = "Select * from `icustomers` WHERE `version` = '2.0'";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("icustomers", "record", record);
                }

                cmd = "Select * from `icontracts` WHERE `version` = '2.0'";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("icontracts", "record", record);
                }

                cmd = "Select * from `policies` WHERE `version` = '2.0'";
                dx = G1.get_db_data(cmd);
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("policies", "record", record);
                }
            }
            catch ( Exception ex)
            {
            }

            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
    }
}