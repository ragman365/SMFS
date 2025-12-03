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
using DevExpress.CodeParser;
using sun.nio.cs;
using DevExpress.Utils.Filtering;
using DevExpress.XtraVerticalGrid.ViewInfo;
using DevExpress.XtraRichEdit.Commands.Internal;
using DevExpress.Charts.Native;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class InsuranceCoupons : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private DataTable workCopy = null;
        private DataTable NatSecDt = null;
        private string SecurityNationalFile = "";
        private DataTable originalDt = null;
        /***********************************************************************************************/
        public InsuranceCoupons()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void InsuranceCoupons_Load(object sender, EventArgs e)
        {
            btnProcess.Show();
            btnSave.Hide();

            DateTime now = DateTime.Now;
            DateTime startDate = new DateTime(now.Year+1, 1, 1);
            DateTime stopDate = startDate.AddMonths(12);
            stopDate = stopDate.AddDays(-1);
            //while (now >= startDate && now < stopDate)
            //{
            //    startDate = startDate.AddMonths(6);
            //    stopDate = startDate.AddMonths(6);
            //}
            //startDate = startDate.AddMonths(-6);
            //stopDate = startDate.AddMonths(6);
            this.dateTimePicker1.Value = startDate;
            this.dateTimePicker2.Value = stopDate;
            labelMaximum.Text = "";
            LoadInitialPayerData();

            tabControl1.TabPages.Remove(tabPage2);
            tabControl1.TabPages.Remove(tabPage3);
        }
        /***********************************************************************************************/
        private void LoadInitialPayerData()
        {
            string cmd = "Select * from `payers` p JOIN `icontracts` c ON p.`contractNumber` = c.`contractNumber` ";
            cmd += ";";

            workDt = G1.get_db_data(cmd);
            G1.NumberDataTable(workDt);
            workCopy = workDt;
        }
        /***********************************************************************************************/
        private string BuildName(string name, string text)
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                if (!String.IsNullOrWhiteSpace(name))
                    name += " ";
                name += text;
            }
            return name;
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
            if (!chkIncludeHeader.Checked)
                gridMain.OptionsPrint.PrintHeader = false;
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});


            printableComponentLink1.Component = dgv;
            if ( dgv2.Visible )
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
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

            if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, true);
            else if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain2, 0.65D, true);
            else if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain3, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();

            if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain, 0.65D, false );
            else if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain2, 0.65D, false );
            else if (dgv.Visible)
                G1.AdjustColumnWidths(gridMain3, 0.65D, false );

            isPrinting = false;
            gridMain.OptionsPrint.PrintHeader = true;
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!chkIncludeHeader.Checked)
                gridMain.OptionsPrint.PrintHeader = false;
            isPrinting = true;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();
            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;

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
            isPrinting = false;
            gridMain.OptionsPrint.PrintHeader = true;
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
            if (!chkIncludeHeader.Checked)
                return;
            if (1 == 1)
                return;
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);


            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(5, 8, 4, 4, "Insurance Coupon Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Day.ToString("D2") + "/" + (date.Year % 100).ToString("D2");

            string str = "Report : " + workDate;

            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(19, 8, 5, 4, str, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void btnGetFile_Click(object sender, EventArgs e)
        {
            SecurityNationalFile = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "All files (*.csv)|*.csv";
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string filename = ofd.FileName;
                    filename = filename.Replace('\\', '/');
                    SecurityNationalFile = filename;
                    btnProcess.Show();
                    string file = ofd.FileName;
                    this.Cursor = Cursors.WaitCursor;
                    NatSecDt = Import.ImportCSVfile(file, null, false, "~");
                    NatSecDt.Columns["Num"].ColumnName = "num";
                    G1.NumberDataTable(NatSecDt);
                    this.Cursor = Cursors.Default;
                    //dgv3.DataSource = dt;
                    //ScaleCells();
                    //G1.SetupVisibleColumns(gridMain3, this.columnsToolStripMenuItem, nmenu_Click);
                    //tabControl1.SelectTab("tabDetail");
                }
            }
        }
        /***********************************************************************************************/
        private void LoadCompanyCodes ( DataTable dx = null )
        {
            if (secNatDt == null)
                secNatDt = G1.get_db_data("Select * from `secnat`;");

            DataTable dt = null;

            if (dx == null)
            {
                string cmd = "Select * from `policies` GROUP BY `companyCode`;";
                dt = G1.get_db_data(cmd);
            }
            else
            {
            }
            if (dt.Rows.Count <= 0)
                return;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "companyCode asc";
            dt = tempview.ToTable();

            chkComboLocation.Properties.DataSource = dt;

        }
        /***********************************************************************************************/
        private DataTable GetGroupData(DataTable dt, string group)
        {
            if (dt.Rows.Count <= 0)
                return dt;
            if (G1.get_column_number(dt, "Int32_id") < 0)
                dt.Columns.Add("Int32_id", typeof(int), "id");

            DataTable groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["book"] }).Select(g => g.OrderBy(r => r["Int32_id"]).First()).CopyToDataTable();
            groupDt.Columns.Remove("Int32_id");
            return groupDt;
        }
        /***********************************************************************************************/
        private DataTable GetGroupData(DataTable dt )
        {
            if (dt.Rows.Count <= 0)
                return dt;

            DataTable groupDt = dt.Clone();

            try
            {
                if (G1.get_column_number(dt, "Int32_id") < 0)
                    dt.Columns.Add("Int32_id", typeof(int), "num");

                groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["companyCodes"] }).Select(g => g.OrderBy(r => r["Int32_id"]).First()).CopyToDataTable();
                groupDt.Columns.Remove("Int32_id");
                string companyCode = "";
                string[] Lines = null;
                DataRow dR = null;
                DataTable dd = new DataTable();
                dd.Columns.Add("companyCode");
                for ( int i=0; i<groupDt.Rows.Count; i++)
                {
                    companyCode = groupDt.Rows[i]["companyCodes"].ObjToString();
                    Lines = companyCode.Split(',');
                    for ( int j=0; j<Lines.Length; j++)
                    {
                        companyCode = Lines[j].Trim();
                        if ( !String.IsNullOrWhiteSpace ( companyCode))
                        {
                            if ( dd.Select ( "companyCode='" + companyCode + "'").Length <= 0 )
                            {
                                dR = dd.NewRow();
                                dR["companyCode"] = companyCode;
                                dd.Rows.Add(dR);
                            }
                        }
                    }
                }
                DataView tempview = dd.DefaultView;
                tempview.Sort = "companyCode asc";
                dd = tempview.ToTable();

                chkComboLocation.Properties.DataSource = dd;
            }
            catch ( Exception ex)
            {
            }
            return groupDt;
        }
        /***********************************************************************************************/
        private DataTable GetGroupSDI(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return dt;

            DataRow[] dRows = null;
            string sdiCode = "";
            string companyCode = "";
            string[] Lines = null;
            string funeralHome = "";
            int length = 0;

            //DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");

            DataTable groupDt = dt.Clone();

            //if (G1.get_column_number(dt, "funeralhome") < 0)
            //    dt.Columns.Add("funeralhome");

            try
            {
                if (G1.get_column_number(dt, "Int32_id") < 0)
                    dt.Columns.Add("Int32_id", typeof(int), "num");

                groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["number"] }).Select(g => g.OrderBy(r => r["Int32_id"]).First()).CopyToDataTable();
                groupDt.Columns.Remove("Int32_id");
                DataRow dR = null;
                DataTable dd = new DataTable();
                dd.Columns.Add("number");
                for (int i = 0; i < groupDt.Rows.Count; i++)
                {
                    companyCode = groupDt.Rows[i]["number"].ObjToString();
                    //length = companyCode.Length;
                    //if ( length > 2 )
                    //{
                    //    sdiCode = companyCode.Substring(length - 2);
                    //    dRows = funDt.Select("SDICode='" + sdiCode + "'" );
                    //    if (dRows.Length > 0)
                    //        funeralHome = dRows[0]["locationCode"].ObjToString();
                    //}
                    Lines = companyCode.Split(',');
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        companyCode = Lines[j].Trim();
                        if (!String.IsNullOrWhiteSpace(companyCode))
                        {
                            if (dd.Select("number='" + companyCode + "'").Length <= 0)
                            {
                                dR = dd.NewRow();
                                dR["number"] = companyCode;
                                dd.Rows.Add(dR);
                            }
                        }
                    }
                }
                DataView tempview = dd.DefaultView;
                tempview.Sort = "number asc";
                dd = tempview.ToTable();

                chkComboSDI.Properties.DataSource = dd;
            }
            catch (Exception ex)
            {
            }

            //for ( int i=0; i<dt.Rows.Count; i++)
            //{
            //    companyCode = dt.Rows[i]["number"].ObjToString();
            //    length = companyCode.Length;
            //    if (length > 2)
            //    {
            //        sdiCode = companyCode.Substring(length - 2);
            //        dRows = funDt.Select("SDICode='" + sdiCode + "'");
            //        if (dRows.Length > 0)
            //        {
            //            funeralHome = dRows[0]["locationCode"].ObjToString();
            //            dt.Rows[i]["funeralhome"] = funeralHome;
            //        }
            //    }
            //}
            return groupDt;
        }
        /***********************************************************************************************/
        private bool GenerateCoupons = false;
        private void btnProcess_Click(object sender, EventArgs e)
        {
            G1.CreateAudit("Insurance Coupon Generation");

            this.Cursor = Cursors.WaitCursor;

            LoadCompanyCodes();

            string[] Lines = null;
            string paymentType = "";
            string payer = "";
            string contract = "";
            string trust = "";
            string loc = "";
            bool copy = false;
            DataTable policyDt = null;
            string policy = "";
            string cmd = "";
            double totalPremium = 0D;
            double premium = 0D;
            string companyCode = "";
            bool found = false;
            DataRow[] dRows = null;
            int good = 0;
            int bad = 0;
            int count = 0;
            DateTime date = DateTime.Now;
            DateTime deceasedDate = DateTime.Now;
            DateTime lapseDate = DateTime.Now;
            DateTime reinstateDate = DateTime.Now;
            DateTime dueDate = DateTime.Now;
            workDt = workCopy.Copy();
            workDt.Columns.Add("companyCodes");
            workDt.Columns.Add("reports");
            workDt.Columns.Add("ach");
            DataTable localDt = workDt.Clone();
            DateTime startDate = this.dateTimePicker1.Value;
            DateTime stopDate = this.dateTimePicker2.Value;
            string lapsed = "";
            string expression = string.Format("dueDate8 >= '{0}'", startDate.ToString("yyyy-MM-dd hh:mm:ss tt"));
            string start = startDate.ToString("yyyyMMdd");
            workDt.Columns.Add("sDate");
            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                date = workDt.Rows[i]["dueDate8"].ObjToDateTime();
                workDt.Rows[i]["sDate"] = date.ToString("yyyyMMdd");
            }
            int lastRow = workDt.Rows.Count;
            //lastRow = 50;

            string testPayer = txtPayer.Text;
            if (!String.IsNullOrWhiteSpace(testPayer))
            {
                //0000100002
                dRows = workDt.Select("payer='" + testPayer + "'");
                //DataTable ddx = workDt.Select("payer='" + testPayer + "'").CopyToDataTable();
                if (dRows.Length <= 0)
                {
                    MessageBox.Show("***ERROR*** Payer not available!");
                    this.Cursor = Cursors.Default;
                    return;
                }
                DataTable ddx = dRows.CopyToDataTable();
                workDt = ddx.Copy();
                lastRow = workDt.Rows.Count;
            }
            bool thirdPartyOnly = false;
            bool hasThirdParty = false;
            bool hasSecNatOnly = false;
            bool hasSecNat = false;
            bool notThirdParty = false;
            bool isMixed = false;
            //lastRow = 10;
            int lRow = 0;
            int mainCount = 0;
            int remainderCount = 0;
            barImport.Minimum = 0;
            barImport.Maximum = lastRow;
            this.labelMaximum.Text = "0 of " + lastRow.ToString();
            this.labelMaximum.Show();
            this.labelMaximum.Refresh();

            double monthlyPremium = 0D;
            double historicPremium = 0D;
            double monthlySecNat = 0D;
            string companyCodes = "";
            string reports = "";
            string report = "";
            string ach = "";
            bool addCode = false;
            DataTable achDt = null;

            bool doit = false;
            GenerateCoupons = true;

            for (int i = 0; i < lastRow; i++)
            {
                Application.DoEvents();

                this.labelMaximum.Text = (i + 1).ToString() + " of " + lastRow.ToString();
                this.labelMaximum.Refresh();
                barImport.Value = i + 1;

                doit = false;

                payer = workDt.Rows[i]["payer"].ObjToString();

                deceasedDate = workDt.Rows[i]["deceasedDate"].ObjToDateTime();
                if (deceasedDate.Year > 100)
                    continue;

                lapsed = workDt.Rows[i]["lapsed"].ObjToString();
                lapseDate = workDt.Rows[i]["lapseDate8"].ObjToDateTime();
                reinstateDate = workDt.Rows[i]["reinstateDate8"].ObjToDateTime();
                dueDate = workDt.Rows[i]["dueDate8"].ObjToDateTime();
                date = workDt.Rows[i]["dueDate81"].ObjToDateTime();
                if (date > dueDate)
                {
                    dueDate = date;
                    workDt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(dueDate);
                }

                if (dueDate >= this.dateTimePicker1.Value && dueDate <= this.dateTimePicker2.Value)
                    doit = true;
                else if (lapsed.ToUpper() == "Y")
                {
                    if (dueDate >= this.dateTimePicker1.Value.AddMonths(-6))
                        doit = true;
                }
                else if (lapseDate.Year > 100)
                {
                    if (reinstateDate < lapseDate)
                    {
                        if (dueDate >= this.dateTimePicker1.Value.AddMonths(-6))
                            doit = true;
                    }
                }
                else
                {
                    if (dueDate >= this.dateTimePicker1.Value.AddMonths(-6) && dueDate <= this.dateTimePicker2.Value)
                        doit = true;
                }
                if (dueDate > this.dateTimePicker2.Value)
                    doit = false;
                if (!String.IsNullOrWhiteSpace(testPayer))
                    doit = true;
                if (!doit)
                    continue;

                cmd = "Select * from `policies` where `payer` = '" + payer + "';";
                policyDt = G1.get_db_data(cmd);

                companyCodes = "";
                reports = "";
                ach = "";
                found = false;
                thirdPartyOnly = true;
                hasThirdParty = false;
                hasSecNatOnly = true;
                hasSecNat = false;
                notThirdParty = false;
                isMixed = false;
                if ( payer == "JNA174-693")
                {

                }
                //150509A

                for (int j = 0; j < policyDt.Rows.Count; j++)
                {
                    try
                    {
                        companyCode = policyDt.Rows[j]["companyCode"].ObjToString();
                        addCode = true;
                        Lines = companyCodes.Split(',');
                        for ( int k=0; k<Lines.Length; k++)
                        {
                            if ( Lines[k].Trim() == companyCode )
                            {
                                addCode = false;
                                break;
                            }
                        }
                        if (addCode)
                            companyCodes += companyCode + ",";
                        report = policyDt.Rows[j]["report"].ObjToString();
                        dRows = secNatDt.Select("cc='" + companyCode + "'");
                        if (dRows.Length <= 0)
                        {
                            if (report.ToUpper() != "NOT THIRD PARTY" && !String.IsNullOrWhiteSpace(report))
                                hasThirdParty = true;
                            hasSecNatOnly = false;
                            found = true;
                            //break;
                        }
                        else
                        {
                            hasSecNat = true;
                            if ( !hasSecNatOnly )
                                isMixed = true;
                        }
                        report = policyDt.Rows[j]["report"].ObjToString();
                        if (report.ToUpper() == "NOT THIRD PARTY" || String.IsNullOrWhiteSpace(report))
                            notThirdParty = true;
                        else
                            thirdPartyOnly = false;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                if ( cmbReport.Text.Trim() == "Report Payers with Security National Policies Only")
                {
                    if (!hasSecNatOnly)
                        continue;
                }
                else if ( cmbReport.Text.Trim() == "Report All Not 3rd Party Payers")
                {
                    if (!notThirdParty)
                        continue;
                }
                else if ( cmbReport.Text.Trim() == "Report All Security National Policies even if Mixed")
                {
                    if (thirdPartyOnly)
                        continue;
                    if (!hasSecNatOnly && !hasSecNat )
                        continue;
                }
                else if (cmbReport.Text.Trim() == "Report Payers with 3rd Party other than Security National")
                {
                    if (thirdPartyOnly)
                        continue;
                    if (hasSecNat)
                        continue;
                }
                else if (cmbReport.Text.Trim() == "Report All")
                {
                    reports = "";
                    ach = "";
                    if (hasSecNat)
                        reports = "HSN,";
                    if (notThirdParty)
                        reports += "HNTP,";
                    if (!thirdPartyOnly)
                    {
                        //if ( !hasSecNatOnly )
                        //    reports += "HTP,";
                        if (hasThirdParty)
                            reports += "HTP,";
                    }
                    reports = reports.TrimEnd(',');
                    cmd = "Select * from `ach` where `payer` = '" + payer + "';";
                    achDt = G1.get_db_data(cmd);
                    if (achDt.Rows.Count > 0)
                        ach= "ACH";
                }

                mainCount = policyDt.Rows.Count;
                if (cmbReport.Text.Trim() == "Report Payers with Security National Policies Only")
                    policyDt = filterSecNat(true, policyDt);


                remainderCount = policyDt.Rows.Count;
                if (remainderCount <= 0)
                    continue;

                totalPremium = 0D;
                copy = false;
                reinstateDate = workDt.Rows[i]["reinstateDate8"].ObjToDateTime();
                count = policyDt.Rows.Count;
                good = 0;
                bad = 0;
                for (int j = 0; j < policyDt.Rows.Count; j++)
                {
                    try
                    {
                        payer = policyDt.Rows[j]["payer"].ObjToString();
                        report = policyDt.Rows[j]["report"].ObjToString();
                        policy = policyDt.Rows[j]["policyNumber"].ObjToString();
                        if (String.IsNullOrWhiteSpace(policy))
                            continue;

                        deceasedDate = policyDt.Rows[j]["deceasedDate"].ObjToDateTime();
                        if (deceasedDate.Year > 100)
                            continue; // Avoid this one because of deceased
                        lapseDate = policyDt.Rows[j]["lapsedDate8"].ObjToDateTime();
                        if (lapseDate.Year > 100)
                            continue;
                        lapsed = policyDt.Rows[j]["lapsed"].ObjToString();
                        if (lapsed.ToUpper() == "Y")
                            continue;
                        if (cmbReport.Text.Trim() == "Report All Not 3rd Party Payers")
                        {
                            if (report.ToUpper() != "NOT THIRD PARTY")
                                continue;
                        }
                        premium = policyDt.Rows[j]["premium"].ObjToDouble();
                        if (premium < 0D)
                            premium = 0D;
                        totalPremium += premium;
                        good++;
                        copy = true;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                if (copy)
                {
                    try
                    {
                        bad = count - good;
                        workDt.Rows[i]["amtOfMonthlyPayt"] = totalPremium;
                        G1.copy_dt_row(workDt, i, localDt, localDt.Rows.Count);
                        lRow = localDt.Rows.Count - 1;
                        localDt.Rows[lRow]["companyCodes"] = companyCodes.TrimEnd ( ',');
                        localDt.Rows[lRow]["reports"] = reports;
                        localDt.Rows[lRow]["ach"] = ach;
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                {
                }
            }
            DataTable dx = generateCouponList(localDt);

            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
            originalDt = dx;

            GetGroupData(dx);
            GetGroupSDI(dx);

            try
            {
                gridMain.RefreshData();
                gridMain.Columns["contractNumber"].Visible = false;
                gridMain.Columns["num"].Visible = false;
                gridMain.Columns["record"].Visible = false;
                if (cmbReport.Text.Trim() != "Report All")
                {
                    gridMain.Columns["companyCodes"].Visible = false;
                    gridMain.Columns["reports"].Visible = false;
                    gridMain.Columns["ach"].Visible = false;
                }
            }
            catch ( Exception ex)
            {
            }
            gridMain.RefreshData();
            this.Cursor = Cursors.Default;

            GenerateCoupons = false;
        }
        /***********************************************************************************************/
        private DataTable GetUniquePolicies(DataTable policyDt)
        {
            DataTable groupDt = policyDt.Clone();
            try
            {
                if (policyDt.Rows.Count <= 0)
                    return policyDt;
                DataTable dt = policyDt.Copy();
                groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r["policyNumber"] }).Select(g => g.OrderBy(r => r["policyNumber"]).First()).CopyToDataTable();
            }
            catch (Exception ex)
            {
            }
            return groupDt;
        }
        /***********************************************************************************************/
        public static DataTable Locate_SDI_Key_Code ( DataTable payerDt, string payer )
        {
            string prefix = "";
            string keyCode = "";
            string checkPayer = "";
            string burial_association = "";
            int len = 0;
            int length = 0;
            string str = "";
            bool found = false;

            DataRow dRow = null;
            DataTable prefixDt = new DataTable();
            prefixDt.Columns.Add("prefix");
            prefixDt.Columns.Add("burial_association");
            prefixDt.Columns.Add("SDI_Key_Code");

            for ( int i=0; i<payerDt.Rows.Count; i++)
            {
                try
                {
                    keyCode = payerDt.Rows[i]["SDI_Key_Code"].ObjToString();
                    burial_association = payerDt.Rows[i]["burial_association"].ObjToString();
                    prefix = payerDt.Rows[i]["prefix"].ObjToString();
                    if (String.IsNullOrWhiteSpace(prefix))
                        continue;
                    len = prefix.Length;
                    checkPayer = payer.Substring(0, len);
                    if (prefix == checkPayer)
                    {
                        found = false;
                        length = len;
                        for ( int j=0; j<prefixDt.Rows.Count; j++)
                        {
                            str = prefixDt.Rows[j]["prefix"].ObjToString();
                            if ( str.Length < length )
                            {
                                prefixDt.Rows[j]["prefix"] = prefix;
                                prefixDt.Rows[j]["burial_association"] = burial_association;
                                prefixDt.Rows[j]["SDI_Key_Code"] = keyCode;
                                found = true;
                            }
                        }
                        if (!found)
                        {
                            dRow = prefixDt.NewRow();
                            dRow["prefix"] = prefix;
                            dRow["burial_association"] = burial_association;
                            dRow["SDI_Key_Code"] = keyCode;
                            prefixDt.Rows.Add(dRow);
                        }
                    }
                }
                catch ( Exception ex )
                {
                }
            }
            return prefixDt;
        }
        /***********************************************************************************************/
        private DataTable generateCouponList(DataTable dt)
        {
            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");
            DataTable burialDt = G1.get_db_data("Select * from `burial_association`");

            DataTable payerDt = BuildPayerTable(burialDt);

            DataTable dx = new DataTable();
            dx.Columns.Add("Number");
            dx.Columns.Add("Payer");
            dx.Columns.Add("Name");
            dx.Columns.Add("Empty");
            dx.Columns.Add("Re");
            dx.Columns.Add("Address");
            dx.Columns.Add("CityStateZip");
            dx.Columns.Add("ALWAYSM");
            dx.Columns.Add("Zero");
            dx.Columns.Add("NumPayments");
            dx.Columns.Add("Payment");
            dx.Columns.Add("IssueDate");
            dx.Columns.Add("ALWAYSM2");
            dx.Columns.Add("contractNumber");
            dx.Columns.Add("companyCodes");
            dx.Columns.Add("reports");
            dx.Columns.Add("ach");
            dx.Columns.Add("funeralHome");
            dx.Columns.Add("record");

            string contract = "";
            string payer = "";
            string firstName = "";
            string middleName = "";
            string lastName = "";
            string suffix = "";
            string name = "";
            string re = "";
            string address = "";
            string address1 = "";
            string address2 = "";
            string city = "";
            string state = "";
            string zip = "";
            string zip2 = "";
            string gender = "";
            string payment = "";
            string issueDate = "";
            string gender2 = "";
            string str = "";
            string agent = "";
            string oldloc = "";
            DateTime date = DateTime.Now;

            DataTable pDt = null;

            DataRow dRow = null;

            string SDICode = "";
            string companyCode = "";
            DataTable sdiDt = null;
            string cnum = "";
            string trust = "";
            string loc = "";
            string cmd = "";
            double numberPayments = 0D;
            string SDI_Key_Code = "";
            string TOTAL_TO_PAY = "";
            DataTable ddt = null;
            DateTime dueDate = DateTime.Now;
            DateTime stopDate = this.dateTimePicker2.Value;

            string depositNumber = "";
            string reports = "";
            string ach = "";
            string funeralHome = "";
            string record = "";
            DataRow[] dRows = null;
            DataTable prefixDt = null;

            DateTime firstDate = new DateTime(stopDate.Year, 1, 1);

            string testPayer = txtPayer.Text.Trim();

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    SDI_Key_Code = dt.Rows[i]["SDICode"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( SDICode ))
                        SDI_Key_Code = "YY";
                    payer = dt.Rows[i]["payer"].ObjToString();
                    prefixDt = Locate_SDI_Key_Code(payerDt, payer);
                    if ( prefixDt.Rows.Count == 1 )
                        SDI_Key_Code = prefixDt.Rows[0]["SDI_Key_Code"].ObjToString();
                    else if ( prefixDt.Rows.Count > 1 )
                    {
                    }

                    companyCode = dt.Rows[i]["companyCodes"].ObjToString();
                    reports = dt.Rows[i]["reports"].ObjToString();
                    ach = dt.Rows[i]["ach"].ObjToString();
                    contract = dt.Rows[i]["contractNumber"].ObjToString();
                    cmd = "Select * from `icustomers` where `contractNumber` = '" + contract + "';";
                    ddt = G1.get_db_data(cmd);
                    if (ddt.Rows.Count <= 0)
                        continue;

                    cmd = "Select * from `ipayments` where `contractNumber` = '" + contract + "' ORDER by `payDate8` DESC LIMIT 1;";
                    pDt = G1.get_db_data(cmd);
                    if ( pDt.Rows.Count > 0 )
                    {
                        depositNumber = pDt.Rows[0]["depositNumber"].ObjToString();
                        if ( !String.IsNullOrWhiteSpace ( depositNumber))
                        {
                            str = depositNumber.Substring(0, 1);
                            if (str.ToUpper() == "A")
                            {
                                if ( String.IsNullOrWhiteSpace ( testPayer ))
                                    continue;
                            }
                        }
                    }

                    dueDate = dt.Rows[i]["dueDate8"].ObjToDateTime();
                    payer = ddt.Rows[0]["payer"].ObjToString();
                    agent = ddt.Rows[0]["agentCode"].ObjToString();
                    oldloc = ddt.Rows[0]["oldloc"].ObjToString();

                    dRow = dx.NewRow();

                    str = payer.PadLeft(10, '0');
                    dRow["payer"] = str;

                    dRow["re"] = "RE: Payer " + payer;

                    funeralHome = "";

                    SDICode = getSDICode(agent, oldloc);
                    SDICode = SDI_Key_Code;
                    if (String.IsNullOrWhiteSpace(SDICode))
                        SDICode = "XX";
                    dRow["Number"] = "947402" + SDICode;

                        dRows = funDt.Select("SDICode='" + SDICode + "'");
                        if (dRows.Length > 0)
                            funeralHome = dRows[0]["locationCode"].ObjToString();


                    address1 = ddt.Rows[0]["ADDRESS1"].ObjToString();
                    address2 = ddt.Rows[0]["ADDRESS2"].ObjToString();
                    address = address1;
                    if (!String.IsNullOrWhiteSpace(address2))
                    {
                        if (!String.IsNullOrWhiteSpace(address))
                            address += " ";
                        address += address2;
                    }
                    dRow["address"] = address;

                    city = ddt.Rows[0]["CITY"].ObjToString();
                    state = ddt.Rows[0]["STATE"].ObjToString();
                    zip = ddt.Rows[0]["ZIP1"].ObjToString();
                    zip2 = ddt.Rows[0]["ZIP2"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(zip2))
                    {
                        if (zip2 != "0")
                            zip += "-" + zip2;
                    }

                    name = "";
                    name = BuildName(name, city);
                    if (!String.IsNullOrWhiteSpace(name))
                        name += ", ";
                    name = BuildName(name, state);
                    name = BuildName(name, zip);
                    dRow["CityStateZip"] = name;

                    firstName = ddt.Rows[0]["FIRSTNAME"].ObjToString();
                    lastName = ddt.Rows[0]["LASTNAME"].ObjToString();
                    name = "";
                    name = BuildName(name, firstName);
                    name = BuildName(name, lastName);
                    dRow["name"] = name;

                    gender = ddt.Rows[0]["SEX"].ObjToString();
                    //dRow["gender"] = gender;
                    //dRow["gender2"] = gender;
                    dRow["ALWAYSM"] = "M";
                    dRow["ALWAYSM2"] = "M";
                    dRow["contractNumber"] = contract;

                    payment = dt.Rows[i]["amtOfMonthlyPayt"].ObjToString();
                    TOTAL_TO_PAY = payment;
                    dRow["payment"] = payment;

                    if (dueDate < this.dateTimePicker1.Value)
                        dueDate = this.dateTimePicker1.Value;

                    numberPayments = (double)G1.GetMonthsBetween(stopDate, dueDate);
                    numberPayments++;
                    if (cmbReport.Text.Trim() == "Report Payers with Security National Policies Only")
                        numberPayments = 0;
                    if ( dueDate < firstDate)
                    {
                        dt.Rows[i]["dueDate8"] = G1.DTtoMySQLDT(firstDate);
                        dueDate = firstDate;
                        numberPayments = 12;
                    }
                    dRow["NumPayments"] = numberPayments.ObjToString();

                    dRow["zero"] = "0";

                    dRow["IssueDate"] = dueDate.ToString("MM/dd/yyyy");
                    dRow["companyCodes"] = companyCode;
                    dRow["reports"] = reports;
                    dRow["ach"] = ach;
                    dRow["funeralHome"] = funeralHome;
                    dRow["record"] = record;
                    dx.Rows.Add(dRow);
                }
                catch (Exception ex)
                {
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        public static DataTable BuildPayerTable ( DataTable burialDt )
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("prefix");
            dt.Columns.Add("SDI_Key_Code");
            dt.Columns.Add("burial_association");

            string burial_association = "";
            string payer_prefixes = "";
            string prefix = "";
            string SDI_Key_Code = "";

            string[] Lines = null;

            DataRow dRow = null;

            for ( int i=0; i<burialDt.Rows.Count; i++)
            {
                burial_association = burialDt.Rows[i]["burial_association"].ObjToString();
                SDI_Key_Code = burialDt.Rows[i]["SDI_Key_Code"].ObjToString();
                payer_prefixes = burialDt.Rows[i]["payer_prefixes"].ObjToString();

                if (String.IsNullOrWhiteSpace(payer_prefixes))
                    continue;
                Lines = payer_prefixes.Split(',');

                for ( int j=0; j<Lines.Length; j++)
                {
                    prefix = Lines[j].ObjToString().Trim();
                    dRow = dt.NewRow();
                    dRow["burial_association"] = burial_association;
                    dRow["SDI_Key_Code"] = SDI_Key_Code;
                    dRow["prefix"] = prefix;
                    dt.Rows.Add(dRow);
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        public static string getSDICode(string agent, string oldloc)
        {
            string sdiCode = "05";
            if (!String.IsNullOrWhiteSpace(agent))
            {
                string agent1 = agent.Substring(0, 1);

                if (agent == "100" || agent == "186" || agent == "200" || agent == "221" || agent1 == "P")
                    oldloc = "P";
                else if (agent == "790" || agent == "T02" || agent == "T01" || agent == "W02")
                    oldloc = "T";
            }

            if (oldloc == "J" || oldloc == "C")
                sdiCode = "01";
            else if (oldloc == "P")
                sdiCode = "02";
            else if (oldloc == "T")
                sdiCode = "03";
            else if (oldloc == "B" || oldloc == "S" || oldloc == "R" || oldloc == "F")
                sdiCode = "06";
            else if (oldloc == "L")
                sdiCode = "07";
            else if (oldloc == "M")
                sdiCode = "08";
            else if (oldloc == "E")
                sdiCode = "09";
            else if (oldloc == "V")
                sdiCode = "10";
            else if (oldloc == "U")
                sdiCode = "14";
            else
                sdiCode = "05";
            return sdiCode;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string contract = dr["contractNumber"].ObjToString();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                this.Cursor = Cursors.WaitCursor;
                //                DataTable dt = (DataTable)dgv.DataSource;
                G1.UpdatePreviousCustomer(contract, LoginForm.username);
                CustomerDetails clientForm = new CustomerDetails(contract);
                clientForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /****************************************************************************************/
        private DataTable secNatDt = null;
        private DataTable filterSecNat(bool include, DataTable dt)
        {
            if (secNatDt == null)
                secNatDt = G1.get_db_data("Select * from `secnat`;");

            DataTable newDt = dt.Clone();
            try
            {
                if (!include)
                {
                    var result = dt.AsEnumerable()
                           .Where(row => !secNatDt.AsEnumerable()
                                                 .Select(r => r.Field<string>("cc"))
                                                 .Any(x => x == row.Field<string>("companyCode"))
                          ).CopyToDataTable();
                    newDt = result.Copy();
                }
                else
                {
                    var result = dt.AsEnumerable()
                           .Where(row => secNatDt.AsEnumerable()
                                                 .Select(r => r.Field<string>("cc"))
                                                 .Any(x => x == row.Field<string>("companyCode"))
                          ).CopyToDataTable();
                    newDt = result.Copy();
                }
            }
            catch (Exception ex)
            {
            }
            return newDt;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv.Visible)
                SetSpyGlass(gridMain);
            else if (dgv2.Visible)
                SetSpyGlass(gridMain2);
            else if (dgv3.Visible)
                SetSpyGlass(gridMain3);
        }
        /***********************************************************************************************/
        private void SetSpyGlass(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView grid)
        {
            G1.ShowHideFindPanel(grid);
            //if (grid.OptionsFind.AlwaysVisible == true)
            //    grid.OptionsFind.AlwaysVisible = false;
            //else
            //    grid.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void readOldDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    DataTable dt = Import.ImportCSVfile(file, null, false, ",");
                    dgv.DataSource = dt;
                }
            }
        }
        /***********************************************************************************************/
        private void compare2ndFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string file = ofd.FileName;
                    DataTable dt = Import.ImportCSVfile(file, null, false, ",");
                    dgv2.DataSource = dt;
                    CompareFiles();
                }
            }
        }
        /***********************************************************************************************/
        private void CompareFiles()
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataTable dt2 = (DataTable)dgv2.DataSource;
            dt2.Columns.Add("found");


            DataTable dt3 = new DataTable();
            dt3.Columns.Add("payer");
            dt3.Columns.Add("status");
            dt3.Columns.Add("smfsPremium", Type.GetType("System.Double"));
            dt3.Columns.Add("as400Premium", Type.GetType("System.Double"));
            dt3.Columns.Add("smfsOnly");
            dt3.Columns.Add("as400Only");

            string payer = "";
            double smfsPremium = 0D;
            double as400Premium = 0D;
            string str = "";
            bool found = false;
            DataRow[] dRows = null;
            DataRow dR = null;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    payer = dt.Rows[i]["payer"].ObjToString();
                    if ( payer == "00UC-M6912")
                    {
                    }
                    dRows = dt2.Select("payer='" + payer + "'");
                    if (dRows.Length > 0)
                    {
                        if (dRows.Length > 1)
                        {
                        }
                        smfsPremium = dt.Rows[i]["num payments"].ObjToDouble();
                        as400Premium = dRows[0]["num payments"].ObjToDouble();
                        if (as400Premium != smfsPremium)
                        {
                            dR = dt3.NewRow();
                            dR["payer"] = payer;
                            dR["smfsPremium"] = smfsPremium;
                            dR["as400Premium"] = as400Premium;
                            dR["status"] = "Mismatch Prem";
                            dt3.Rows.Add(dR);
                            dRows[0]["found"] = "Y";
                        }
                        else
                            dRows[0]["found"] = "Y";
                    }
                    else
                    {
                        smfsPremium = dt.Rows[i]["num payments"].ObjToDouble();
                        dR = dt3.NewRow();
                        dR["payer"] = payer;
                        dR["smfsOnly"] = "Y";
                        dR["smfsPremium"] = smfsPremium;
                        dt3.Rows.Add(dR);
                    }
                }
                catch (Exception ex)
                {
                }
            }

            int all = dt2.Rows.Count;
            int icount = 0;

            for ( int i=0; i<dt2.Rows.Count; i++)
            {
                str = dt2.Rows[i]["found"].ObjToString();
                if (str != "Y")
                    icount++;
            }


            for (int i = 0; i < dt2.Rows.Count; i++)
            {
                try
                {
                    str = dt2.Rows[i]["found"].ObjToString();
                    if (str != "Y")
                    {
                        payer = dt2.Rows[i]["payer"].ObjToString();
                        as400Premium = dt2.Rows[i]["num payments"].ObjToDouble();
                        dR = dt3.NewRow();
                        dR["payer"] = payer;
                        dR["as400Only"] = "Y";
                        dR["as400Premium"] = as400Premium;
                        dt3.Rows.Add(dR);
                    }
                }
                catch (Exception ex)
                {
                }
            }
            int smfsOnly = 0;
            int as400Only = 0;

            for ( int i=0; i<dt3.Rows.Count; i++)
            {
                str = dt3.Rows[i]["smfsOnly"].ObjToString();
                if (str == "Y")
                    smfsOnly++;
                str = dt3.Rows[i]["as400Only"].ObjToString();
                if (str == "Y")
                    as400Only++;
            }

            G1.NumberDataTable(dt3);
            dgv3.DataSource = dt3;
        }
        /***********************************************************************************************/
        private void gridMain3_DoubleClick(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv3.DataSource;
            DataRow dr = gridMain3.GetFocusedDataRow();
            string payer = dr["payer"].ObjToString();
            if (!String.IsNullOrWhiteSpace(payer))
            {
                payer = payer.TrimStart('0');
                string cmd = "Select * from `icustomers` where `payer` = '" + payer + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string contract = dx.Rows[0]["contractNumber"].ObjToString();
                    this.Cursor = Cursors.WaitCursor;
                    CustomerDetails clientForm = new CustomerDetails(contract);
                    clientForm.Show();
                    this.Cursor = Cursors.Default;
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain3_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            DataTable dt = originalDt.Clone();

            DataRow[] dR = null;
            DataRow [] dRow = null;
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            string policy = "";
            string companyCode = "";
            string[] Lines = null;
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    dR = originalDt.Select("companyCodes LIKE '%" + locIDs[i].Trim() + "%'");
                    if ( dR.Length > 0 )
                    {
                        for (int j = 0; j < dR.Length; j++)
                        {
                            companyCode = dR[j]["companyCodes"].ObjToString();
                            Lines = companyCode.Split(',');
                            for (int k = 0; k < Lines.Length; k++)
                            {
                                companyCode = Lines[k].ObjToString().Trim();
                                if (companyCode == locIDs[i].Trim())
                                {
                                    policy = dR[j]["payer"].ObjToString();
                                    dRow = dt.Select("payer='" + policy + "'");
                                    if (dRow.Length <= 0)
                                        dt.ImportRow(dR[j]);
                                }
                            }
                        }
                    }
                }
            }

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void chkComboSDI_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;

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
            string[] locIDs = this.chkComboSDI.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `number` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");

            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            DataRow dr = gridMain.GetFocusedDataRow();
            if (dr == null)
                return;

            dr["mod"] = "Y";

            btnSave.Show();
            btnSave.Refresh();
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "mod") < 0)
                dt.Columns.Add("mod");

            string mod = "";
            string payer = "";
            string record = "";
            string sdiCode = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString().ToUpper();
                if (mod != "Y")
                    continue;

                record = dt.Rows[i]["record"].ObjToString();
                payer = dt.Rows[i]["payer"].ObjToString();
                sdiCode = dt.Rows[i]["Number"].ObjToString();
                sdiCode = sdiCode.Replace("947402", "");
                if (String.IsNullOrWhiteSpace(sdiCode))
                    continue;

                if (String.IsNullOrWhiteSpace(record))
                    continue;
                G1.update_db_table("payers", "record", record, new string[] { "SDICode", sdiCode });
            }
        }
        /***********************************************************************************************/
    }
}