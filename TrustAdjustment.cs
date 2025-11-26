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
using MySql.Data.MySqlClient;
using GeneralLib;
using DevExpress.XtraGrid.Views.Grid;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class TrustAdjustment : DevExpress.XtraEditors.XtraForm
    {
        private DateTime workDate = DateTime.Now;
        private double workTrust100 = 0D;
        private double workTrust85 = 0D;
        private double workRetained = 0D;
        private double workInterest = 0D;
        private string workReason = "";
        private string workDepositNumber = "";
        private string workContract = "";
        private string workName = "";
        private bool workEdit = false;
        private DateTime _TrustDate = DateTime.Now;
        private double _Trust100Amount = 0D;
        private double _Trust85Amount = 0D;
        private double _TrustRetained = 0D;
        private double _TrustInterest = 0D;
        private string _TrustDepositNumber = "";
        private string _TrustReason = "";
        public DateTime TrustDate { get { return _TrustDate; } }
        public double Trust100Amount { get { return _Trust100Amount; } }
        public double Trust85Amount { get { return _Trust85Amount; } }
        public double TrustRetained { get { return _TrustRetained; } }
        public double TrustInterest { get { return _TrustInterest; } }
        public string TrustReason { get { return _TrustReason; } }
        public string TrustDepositNumber { get { return _TrustDepositNumber; } }
        /****************************************************************************************/
        public TrustAdjustment(string contract, string name, DateTime date, double trust100, double trust85, double retained, double interest, string depositNumber, string reason, bool editing)
        {
            InitializeComponent();
            workEdit = editing;
            workDate = date;
            workTrust100 = trust100;
            workTrust85 = trust85;
            workRetained = retained;
            workInterest = interest;
            workDepositNumber = depositNumber;
            workReason = reason;
            workContract = contract;
            workName = name;
        }
        /****************************************************************************************/
        private void TrustAdjustment_Load(object sender, EventArgs e)
        {
            chkNoSolve.Hide(); // Hide this because not solving for the unknown caused problems
            if (!workEdit)
            {
                workTrust100 = 0D;
                workTrust85 = 0D;
                workRetained = 0D;
            }
            DataTable dt = new DataTable();
            dt.Columns.Add("field");
            dt.Columns.Add("data");
            DataRow dR = dt.NewRow();
            dR["field"] = "Trust Adjustment Date";
            dR["data"] = workDate.ToString("MM/dd/yyyy");
            dt.Rows.Add(dR);

            string str = G1.ReformatMoney(workTrust100);
            dR = dt.NewRow();
            dR["field"] = "Trust100 Adjustment Amount";
            dR["data"] = str;
            dt.Rows.Add(dR);

            str = G1.ReformatMoney(workTrust85);
            dR = dt.NewRow();
            dR["field"] = "Trust85 Adjustment Amount";
            dR["data"] = str;
            dt.Rows.Add(dR);

            str = G1.ReformatMoney(workRetained);
            dR = dt.NewRow();
            dR["field"] = "Retained Interest";
            dR["data"] = str;
            dt.Rows.Add(dR);

            str = G1.ReformatMoney(workInterest);
            dR = dt.NewRow();
            dR["field"] = "Interest";
            dR["data"] = str;
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["field"] = "Trust Deposit Number";
            dR["data"] = workDepositNumber;
            dt.Rows.Add(dR);

            dR = dt.NewRow();
            dR["field"] = "Trust Adjustment Reason";
            dR["data"] = workReason;
            dt.Rows.Add(dR);

            dgv.DataSource = dt;
        }
        /****************************************************************************************/
        private void btnPost_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            _TrustDate = dt.Rows[0]["data"].ObjToDateTime();
            _Trust100Amount = dt.Rows[1]["data"].ObjToString().ObjToDouble();
            _Trust85Amount = dt.Rows[2]["data"].ObjToString().ObjToDouble();
            _TrustRetained = dt.Rows[3]["data"].ObjToString().ObjToDouble();
            _TrustInterest = dt.Rows[4]["data"].ObjToString().ObjToDouble();
            _TrustDepositNumber = dt.Rows[5]["data"].ObjToString();
            _TrustReason = dt.Rows[6]["data"].ObjToString();
            printPreviewToolStripMenuItem_Click(null, null);
            this.DialogResult = DialogResult.OK;
            this.Cursor = Cursors.Default;
            this.Close();
        }
        /****************************************************************************************/
        private void btnAbort_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            DataTable dt = (DataTable)dgv.DataSource;
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string field = dr["field"].ObjToString();
            if (gridMain.FocusedColumn == gridMain.Columns["data"])
            {
                if (field == "Trust Adjustment Date")
                {
                    string str = dr["data"].ObjToString();
                    if (!G1.validate_date(str))
                    {
                        MessageBox.Show("***ERROR*** Invalid Date Entered!");
                        dr["data"] = workDate.ToString("MM/dd/yyyy");
                        return;
                    }
                    DateTime date = str.ObjToDateTime();
                    str = date.ToString("MM/dd/yyyy");
                    dr["data"] = str;
                    gridMain.RefreshData();
                    workDate = date;
                }
                else if (field == "Trust100 Adjustment Amount")
                {
                    double money = 0D;
                    string str = dr["data"].ObjToString();
                    if (!G1.validate_numeric(str))
                    {
                        MessageBox.Show("***ERROR*** Invalid Amount Entered!");
                        str = G1.ReformatMoney(workTrust100);
                        money = str.ObjToDouble();
                        str = G1.ReformatMoney(money);
                        dr["data"] = str;
                        return;
                    }
                    money = str.ObjToDouble();
                    str = G1.ReformatMoney(money);
                    dr["data"] = str;
                    if (!chkNoSolve.Checked)
                    {
                        workTrust100 = money;
                        workTrust85 = money * 0.85D;
                        workRetained = workTrust100 * (-1D);
                        PutMoney("Trust85 Adjustment Amount", workTrust85);
                        PutMoney("Retained Interest", workRetained);
                    }
                }
                else if (field == "Trust85 Adjustment Amount")
                {
                    double money = 0D;
                    string str = dr["data"].ObjToString();
                    if (!G1.validate_numeric(str))
                    {
                        MessageBox.Show("***ERROR*** Invalid Amount Entered!");
                        str = G1.ReformatMoney(workTrust85);
                        money = str.ObjToDouble();
                        str = G1.ReformatMoney(money);
                        dr["data"] = str;
                        return;
                    }
                    money = str.ObjToDouble();
                    str = G1.ReformatMoney(money);
                    dr["data"] = str;
                    //if (!chkNoSolve.Checked)
                    //{
                    //    workTrust85 = money;
                    //    workTrust100 = money / 0.85D;
                    //    workRetained = workTrust100 * (-1D);
                    //    PutMoney("Trust100 Adjustment Amount", workTrust100);
                    //    PutMoney("Retained Interest", workRetained);
                    //}
                }
                else if (field == "Retained Interest")
                {
                    double money = 0D;
                    string str = dr["data"].ObjToString();
                    if (!G1.validate_numeric(str))
                    {
                        MessageBox.Show("***ERROR*** Invalid Amount Entered!");
                        str = G1.ReformatMoney(workRetained);
                        money = str.ObjToDouble();
                        str = G1.ReformatMoney(money);
                        dr["data"] = str;
                        return;
                    }
                    money = str.ObjToDouble();
                    str = G1.ReformatMoney(money);
                    dr["data"] = str;
                    //if (!chkNoSolve.Checked)
                    //{
                    //    workTrust100 = money * (-1D);
                    //    workTrust85 = workTrust100 * 0.85D;
                    //    PutMoney("Trust100 Adjustment Amount", workTrust100);
                    //    PutMoney("Trust85 Adjustment Amount", workTrust85);
                    //}
                }
                else if (field == "Interest")
                {
                    double money = 0D;
                    string str = dr["data"].ObjToString();
                    if (!G1.validate_numeric(str))
                    {
                        MessageBox.Show("***ERROR*** Invalid Amount Entered!");
                        str = G1.ReformatMoney(workRetained);
                        money = str.ObjToDouble();
                        str = G1.ReformatMoney(money);
                        dr["data"] = str;
                        return;
                    }
                    money = str.ObjToDouble();
                    str = G1.ReformatMoney(money);
                    dr["data"] = str;
                }
            }
        }
        /***************************************************************************************/
        private double PutMoney(string name, double money )
        {
            double value = 0D;
            string str = "";
            DataTable dt = (DataTable)dgv.DataSource;
            int row = FindRow(name);
            if (row >= 0)
            {
                value = G1.RoundValue(money);
                str = G1.ReformatMoney(value);
                dt.Rows[row]["data"] = str;
            }
            return value;
        }
        /***************************************************************************************/
        private int FindRow(string desc)
        {
            int row = -1;
            string description = "";
            DataTable dt = (DataTable)dgv.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                description = dt.Rows[i]["field"].ObjToString().ToUpper();
                if (description == desc.ToUpper())
                {
                    row = i;
                    break;
                }
            }
            return row;
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        /***********************************************************************************************/
        private void printableComponentLink1_CreateDetailHeaderArea(object sender, CreateAreaEventArgs e)
        {
        }
        /***********************************************************************************************/
        private void printableComponentLink1_CreateMarginalHeader(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            Font font = new Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 2, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Top, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 1, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 3, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 10, 4, 2, "Trust Adjustment", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 7, FontStyle.Regular);
            Printer.DrawQuad(1, 11, 4, 1, "Contract :" + workContract, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(3, 11, 3, 1, "Name :" + workName, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 5, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
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
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeader);
            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 50, 150, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreviewDialog();
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
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeader);

            printableComponentLink1.Landscape = false;

            Printer.setupPrinterMargins(50, 100, 150, 50);

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
        /****************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /****************************************************************************************/
    }
}