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
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using System.IO;
using GeneralLib;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class CompareTrustResults : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        /****************************************************************************************/
        public CompareTrustResults( DataTable dt )
        {
            InitializeComponent();
            workDt = dt.Copy();
        }
        /****************************************************************************************/
        private void CompareTrustResults_Load(object sender, EventArgs e)
        {
            chkFilter.Hide();
            if ( G1.get_column_number ( workDt,"found") < 0 )
                workDt.Columns.Add("found");
            if (G1.get_column_number(workDt, "foundDP") < 0)
                workDt.Columns.Add("foundDP");
            if (G1.get_column_number(workDt, "foundPayment") < 0)
                workDt.Columns.Add("foundPayment");
            if (G1.get_column_number(workDt, "foundDebit") < 0)
                workDt.Columns.Add("foundDebit");
            if (G1.get_column_number(workDt, "foundCredit") < 0)
                workDt.Columns.Add("foundCredit");
            if (G1.get_column_number(workDt, "foundTrust85") < 0)
                workDt.Columns.Add("foundTrust85");
            if (G1.get_column_number(workDt, "foundTrust100") < 0)
                workDt.Columns.Add("foundTrust100");

            int count = workDt.Rows.Count;
            G1.NumberDataTable(workDt);
            dgv.DataSource = workDt;
        }
        /****************************************************************************************/
        private void btnCompare_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Import AS400 Monthly Trust Results");
            importForm.SelectDone += ImportForm_SelectDone;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            dt.Columns.Add("BAD");
            string contractNumber = "";
            string cmd = "";
            DataTable dx = null;
            string record = "";
            double dpay = 0.0D;
            double payment = 0.0D;
            double debit = 0.0D;
            double credit = 0.0D;
            double trust100 = 0D;
            double trust85 = 0D;
            bool found = false;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    contractNumber = dt.Rows[i]["CNUM"].ObjToString();
                    dpay = dt.Rows[i]["DPAY"].ObjToDouble();
                    payment = dt.Rows[i]["PAYAMT"].ObjToDouble();
                    debit = dt.Rows[i]["DEBIT"].ObjToDouble();
                    credit = dt.Rows[i]["CREDIT"].ObjToDouble();
                    trust100 = dt.Rows[i]["TRUST100"].ObjToDouble();
                    trust85 = dt.Rows[i]["TRUST85"].ObjToDouble();
                    found = FindData(contractNumber, dpay, payment, debit, credit, trust85, trust100);
                    if (!found)
                    {
                        if (dpay != 0D || payment != 0D || debit != 0D || credit != 0D)
                            dt.Rows[i]["BAD"] = "BAD";
                    }
                    else
                        dt.Rows[i]["BAD"] = "FOUND";
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR 1 *** " + ex.Message.ToString());
            }

            try
            {
                DataRow[] dRows = dt.Select("BAD='BAD'");
                for (int i = 0; i < dRows.Length; i++)
                {
                    contractNumber = dRows[i]["CNUM"].ObjToString();
                    dpay = dRows[i]["DPAY"].ObjToDouble();
                    payment = dRows[i]["PAYAMT"].ObjToDouble();
                    debit = dRows[i]["DEBIT"].ObjToDouble();
                    credit = dRows[i]["CREDIT"].ObjToDouble();
                    trust85 = dRows[i]["TRUST85"].ObjToDouble();
                    trust100 = dRows[i]["TRUST100"].ObjToDouble();

                    DataRow nRow = workDt.NewRow();
                    nRow["contractNumber"] = contractNumber;
                    nRow["newBusiness"] = dpay;
                    nRow["paymentAmount"] = payment;
                    nRow["debitAdjustment"] = debit;
                    nRow["creditAdjustment"] = credit;
                    nRow["trust85P"] = trust85;
                    nRow["trust100P"] = trust100;
                    nRow["FOUND"] = "BAD";
                    workDt.Rows.Add(nRow);
                }
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR 2*** " + ex.Message.ToString());
            }
            this.Cursor = Cursors.Default;
            chkFilter.Show();
        }
        /****************************************************************************************/
        private bool FindData(string contract, double foundDpay, double foundPayment, double foundDebit, double foundCredit, double foundTrust85, double foundTrust100 )
        {
            bool found = false;
            DataTable dt = workDt;

            double downPayment = 0D;
            double downPayment1 = 0D;
            double paymentAmount = 0D;
            double debit = 0D;
            double credit = 0D;
            string str = "";

            DataRow[] dRows = null;

            try
            {
                dRows = dt.Select("contractNumber='" + contract + "'");
                for (int j = 0; j < dRows.Length; j++)
                {
                    str = dRows[j]["FOUND"].ObjToString();
                    if (str.ToUpper() == "FOUND")
                        continue;
                    downPayment = dRows[j]["downPayment"].ObjToDouble();
                    downPayment1 = dRows[j]["downPayment1"].ObjToDouble();
                    downPayment1 = dRows[j]["newBusiness"].ObjToDouble();
                    paymentAmount = dRows[j]["paymentAmount"].ObjToDouble();
                    debit = dRows[j]["debitAdjustment"].ObjToDouble();
                    credit = dRows[j]["creditAdjustment"].ObjToDouble();
                    if (foundDpay > 0D)
                    {
                        if (downPayment1 == foundDpay)
                        {
                            dRows[j]["FOUND"] = "FOUND";
                            dRows[j]["FOUNDDP"] = "FOUND";
                            dRows[j]["FOUNDTRUST85"] = foundTrust85;
                            dRows[j]["FOUNDTRUST100"] = foundTrust100;
                            found = true;
                            break;
                        }
                    }
                    else if (foundPayment > 0D)
                    {
                        if (paymentAmount == foundPayment)
                        {
                            dRows[j]["FOUND"] = "FOUND";
                            dRows[j]["FOUNDPAYMENT"] = "FOUND";
                            dRows[j]["FOUNDTRUST85"] = foundTrust85;
                            dRows[j]["FOUNDTRUST100"] = foundTrust100;
                            found = true;
                            break;
                        }
                    }
                    else if (foundDebit > 0D)
                    {
                        if (debit == foundDebit)
                        {
                            dRows[j]["FOUND"] = "FOUND";
                            dRows[j]["FOUNDDEBIT"] = "FOUND";
                            dRows[j]["FOUNDTRUST85"] = foundTrust85;
                            dRows[j]["FOUNDTRUST100"] = foundTrust100;
                            found = true;
                            break;
                        }
                    }
                    else if (foundCredit > 0D)
                    {
                        if (credit == foundCredit)
                        {
                            dRows[j]["FOUND"] = "FOUND";
                            dRows[j]["FOUNDCREDIT"] = "FOUND";
                            dRows[j]["FOUNDTRUST85"] = foundTrust85;
                            dRows[j]["FOUNDTRUST100"] = foundTrust100;
                            found = true;
                            break;
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR 0*** " + ex.Message.ToString());
            }
            return found;
        }
        /****************************************************************************************/
        private int compressData ( string [] Compressed )
        {
            int compressedAnswers = 0;
            int count = G1.of_ans_count;

            for ( int i=0; i<count; i++)
            {
                if ( !String.IsNullOrWhiteSpace ( G1.of_answer[i]))
                {
                    Compressed[compressedAnswers] = G1.of_answer[i];
                    compressedAnswers++;
                    if (compressedAnswers >= 10)
                        break;
                }
            }
            return compressedAnswers;
        }
        /****************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string contractNumber = dr["contractNumber"].ObjToString();
            if (String.IsNullOrWhiteSpace(contractNumber))
                return;
            string cmd = "Select * from `contracts` where `contractNumber` = '" + contractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0)
            {
                MessageBox.Show("***ERROR*** Bad Contract Number " + contractNumber + "!");
                return;
            }
            CustomerDetails clientForm = new CustomerDetails(contractNumber);
            clientForm.Show();
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

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 3, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 3, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 9, 2, 3, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            string title = "Compare Trust Data to AS400";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


//            Printer.DrawQuad(20, 8, 5, 4, title + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
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
        /****************************************************************************************/
        private void chkFilter_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /****************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            if (!chkFilter.Checked)
                return;
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string found = dt.Rows[row]["FOUND"].ObjToString();
            if ( found.ToUpper() == "FOUND")
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /****************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0)
                return;
            if (e.Column.FieldName.ToUpper().IndexOf("TRUST85") >= 0)
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = gridMain.GetDataSourceRowIndex(e.RowHandle);

                double trust85 = dt.Rows[row]["trust85P"].ObjToDouble();
                double foundTrust85 = dt.Rows[row]["foundTrust85"].ObjToDouble();
                double diff = foundTrust85 - trust85;
                if ( foundTrust85 != trust85 )
                {
                    e.Appearance.BackColor = Color.Red;
                }
            }
        }
        /****************************************************************************************/
    }
}