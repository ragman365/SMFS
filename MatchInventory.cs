using System;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.Base;
using System.Globalization;
using System.IO;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
using DevExpress.XtraVerticalGrid.ViewInfo;
using DevExpress.DataProcessing;
using DevExpress.XtraRichEdit.Mouse;
/***********************************************************************************************/
namespace SMFS
{
    public partial class MatchInventory : DevExpress.XtraEditors.XtraForm
    {
        public MatchInventory()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void MatchInventory_Load(object sender, EventArgs e)
        {
            btnMatch.Hide();
        }
        /***********************************************************************************************/
        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            string filename = "";
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    return;
                filename = ofd.FileName;
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("found");
            dt.Columns.Add("SerialNumber");
            dt.Columns.Add("location");
            dt.Columns.Add("caseid");
            dt.Columns.Add("DecedentFullName");
            dt.Columns.Add("date");
            dt.Columns.Add("ItemName");
            dt.Columns.Add("SecondaryArranger");
            dt.Columns.Add("DecedentDateofDeath");

            string location = "";
            string serialNumber = "";
            string caseId = "";
            string DecedentFullName = "";
            string ServiceDate = "";
            string ItemName = "";
            string SecondaryArranger = "";
            string DecedentDateofDeath = "";

            DateTime date = DateTime.Now;

            string[] Lines = null;
            string line = "";

            DataRow[] dRows = null;

            try
            {
                bool first = true;
                int row = 0;
                string str = "";

                FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (StreamReader sr = new StreamReader(fs))

                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        Application.DoEvents();
                        if (String.IsNullOrWhiteSpace(line))
                            continue;

                        if (line.IndexOf("Smfs Corporate") >= 0)
                            continue;

                        if (line.IndexOf("Report Ran On:") >= 0)
                            continue;

                        if (line.IndexOf("Case Identifier") >= 0)
                            continue;

                        if (line.IndexOf("Basic Filter Criteria") >= 0)
                            break;

                        if ( line.IndexOf ("Case Branch") >= 0 )
                        {
                            Lines = line.Split(':');
                            if (Lines.Length <= 1)
                                continue;
                            location = Lines[1].Trim();
                            location = location.Replace("\"", "");
                            location = location.Replace("\\", "");
                            location = location.Replace(",", "");
                            continue;
                        }
                        G1.parse_answer_data(line, ",");
                        if (G1.of_ans_count < 4)
                            continue;

                        serialNumber = G1.of_answer[4].Trim();
                        caseId = G1.of_answer[0].Trim();
                        DecedentFullName = G1.of_answer[1].Trim();
                        ServiceDate = G1.of_answer[2].Trim();

                        date = ServiceDate.ObjToDateTime();
                        if (date.Year > 100)
                            ServiceDate = date.ToString("MM/dd/yyyy");

                        ItemName = G1.of_answer[3].Trim();
                        SecondaryArranger = G1.of_answer[5].Trim();
                        DecedentDateofDeath = G1.of_answer[6].Trim();

                        DataRow dRow = dt.NewRow();

                        dRow["location"] = location;
                        dRow["serialNumber"] = serialNumber;
                        dRow["caseid"] = caseId;
                        dRow["DecedentFullName"] = DecedentFullName;
                        dRow["date"] = ServiceDate;
                        dRow["ItemName"] = ItemName;
                        dRow["SecondaryArranger"] = SecondaryArranger;
                        dRow["DecedentDateofDeath"] = DecedentDateofDeath;


                        dt.Rows.Add(dRow);
                        row++;
                    }
                    sr.Close();
                    G1.NumberDataTable(dt);
                    dgv.DataSource = dt;
                    btnMatch.Show();
                }
            }
            catch (Exception ex)
            {
                DevExpress.XtraEditors.XtraMessageBox.Show(ex.Message, "Error Occurred");
            }
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

            Printer.setupPrinterMargins(50, 100, 110, 50);

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

            Printer.setupPrinterMargins(50, 100, 110, 50);

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

            font = new Font("Ariel", 10, FontStyle.Regular);
            string title = this.Text;

            Printer.DrawQuad(5, 8, 9, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


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
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void btnMatch_Click(object sender, EventArgs e)
        {
            string found = "";
            string serialNumber = "";

            string itemName = "";
            string SMFSItemName = "";
            string date = "";
            string SMFSdate = "";
            string dod = "";
            string SMFSdod = "";
            string cmd = "";

            string caseId = "";
            string serviceId = "";

            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "found") < 0)
                dt.Columns.Add("found");
            if (G1.get_column_number(dt, "SMFSdate") < 0)
                dt.Columns.Add("SMFSdate");
            if (G1.get_column_number(dt, "SMFSItemName") < 0)
                dt.Columns.Add("SMFSItemName");
            if (G1.get_column_number(dt, "SMFSdod") < 0)
                dt.Columns.Add("SMFSdod");
            if (G1.get_column_number(dt, "SMFSServiceId") < 0)
                dt.Columns.Add("SMFSServiceId");

            DataTable dx = null;

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                found = "";
                serialNumber = dt.Rows[i]["SerialNumber"].ObjToString();
                if (String.IsNullOrWhiteSpace(serialNumber))
                    continue;

                cmd = "Select * from `inventory` where `SerialNumber` = '" + serialNumber + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                    found = "YES";
                else
                {
                    cmd = "Select * from `inventory` where `SerialNumber` LIKE '%" + serialNumber + "%';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                        found = "YES";
                }
                if ( found == "YES")
                {
                    dt.Rows[i]["found"] = "YES";

                    try
                    {
                        SMFSdod = dx.Rows[0]["deceasedDate"].ObjToDateTime().ToString("MM/dd/yyyy");
                        dod = dt.Rows[i]["DecedentDateofDeath"].ObjToString();

                        SMFSItemName = dx.Rows[0]["CasketDescription"].ObjToString();
                        itemName = dt.Rows[i]["ItemName"].ObjToString();

                        date = dt.Rows[i]["date"].ObjToString();
                        SMFSdate = dx.Rows[0]["DateUsed"].ObjToDateTime().ToString("MM/dd/yyyy");

                        caseId = dt.Rows[i]["caseid"].ObjToString();
                        serviceId = dx.Rows[0]["ServiceId"].ObjToString();

                        if (SMFSdod != dod)
                            dt.Rows[i]["SMFSdod"] = SMFSdod;
                        if (SMFSdate != date)
                            dt.Rows[i]["SMFSdate"] = SMFSdod;
                        if (SMFSItemName != itemName)
                            dt.Rows[i]["SMFSItemName"] = SMFSItemName;
                        if (serviceId != caseId)
                            dt.Rows[i]["SMFSServiceId"] = serviceId;
                    }
                    catch ( Exception ex)
                    {
                    }

                }
            }

            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
    }
}