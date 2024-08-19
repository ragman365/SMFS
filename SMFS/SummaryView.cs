using DevExpress.XtraEditors;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
/****************************************************************************************/
namespace SMFS
{
    /****************************************************************************************/
    public partial class SummaryView : DevExpress.XtraEditors.XtraForm
    {
        private DataTable workDt = null;
        private DateTime reportDate = DateTime.Now;
        /****************************************************************************************/
        public SummaryView( DataTable dt, DateTime date )
        {
            InitializeComponent();
            workDt = dt;
            reportDate = date;
            dgv.Dock = DockStyle.Fill;
            dgv2.Visible = false;
            dgv2.Dock = DockStyle.Fill;
        }
        /****************************************************************************************/
        private void SummaryView_Load(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Loc");
            dt.Columns.Add("agentName");
            dt.Columns.Add("total", Type.GetType("System.Double"));

            DataRow dRow = null;
            string oldLocation = "";
            string location = "";
            string agent = "";
            double contractValue = 0D;

            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                location = workDt.Rows[i]["loc"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    continue;
                if (String.IsNullOrWhiteSpace(oldLocation))
                    oldLocation = location;
                if ( oldLocation != location)
                {
                }
                agent = workDt.Rows[i]["agentName"].ObjToString();
                contractValue = workDt.Rows[i]["total"].ObjToDouble();
                if (contractValue <= 0D)
                    continue;
                dRow = dt.NewRow();
                dRow["Loc"] = location;
                dRow["agentName"] = agent;
                dRow["total"] = contractValue;
                dt.Rows.Add(dRow);
            }
            if (dt.Rows.Count <= 0)
                return;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "Loc asc, agentName asc";
            dt = tempview.ToTable();

            oldLocation = "";
            string oldAgent = "";
            string str = "";
            int row = 0;

            DataTable newDt = new DataTable();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["loc"].ObjToString();
                agent = dt.Rows[i]["agentName"].ObjToString();
                contractValue = dt.Rows[i]["total"].ObjToDouble();
                if (oldLocation != location)
                {
                    newDt.Columns.Add(location);
                    if (row >= newDt.Rows.Count)
                    {
                        dRow = newDt.NewRow();
                        newDt.Rows.Add(dRow);
                    }
                    oldLocation = location;
                    oldAgent = "";
                    row = 0;
                    newDt.Rows[row][location] = oldAgent;
                }
                if (agent != oldAgent)
                {
                    if (row > 0)
                    {
                        row++;
                        if (row >= newDt.Rows.Count)
                        {
                            dRow = newDt.NewRow();
                            newDt.Rows.Add(dRow);
                        }
                        row++;
                    }
                    if (row >= newDt.Rows.Count)
                    {
                        dRow = newDt.NewRow();
                        newDt.Rows.Add(dRow);
                    }
                    oldAgent = agent;
                    newDt.Rows[row][oldLocation] = agent;
                }
                row++;
                if (row >= newDt.Rows.Count)
                {
                    dRow = newDt.NewRow();
                    newDt.Rows.Add(dRow);
                }
                dRow = newDt.NewRow();

                str = G1.ReformatMoney(contractValue);
                newDt.Rows[row][oldLocation] = str;
            }
            dRow = newDt.NewRow();
            newDt.Rows.Add(dRow);

            dRow = newDt.NewRow();
            newDt.Rows.Add(dRow);

            row = newDt.Rows.Count - 1;
            double totalContracts = 0D;
            double totalAll = 0D;
            string cmd = "";
            DataTable funDt = null;
            for ( int i=0; i<newDt.Columns.Count; i++)
            {
                totalContracts = 0D;
                for ( int j=0; j<newDt.Rows.Count; j++)
                {
                    contractValue = newDt.Rows[j][i].ObjToDouble();
                    totalContracts += contractValue;
                    if ( contractValue > 0D)
                    {
                        str = String.Format("{0,14:C2}", contractValue);
                        newDt.Rows[j][i] = str;
                    }
                }
                totalAll += totalContracts;
                str = String.Format("{0,14:C2}", totalContracts);
                newDt.Rows[row][i] = str;
                location = newDt.Columns[i].ColumnName.Trim();
                cmd = "Select * from `funeralHomes` where `keycode` = '" + location + "';";
                funDt = G1.get_db_data(cmd);
                if ( funDt.Rows.Count > 0 )
                {
                    str = funDt.Rows[0]["LocationCode"].ObjToString();
                    newDt.Columns[i].ColumnName = str;
                }
            }

            dRow = newDt.NewRow();
            newDt.Rows.Add(dRow);
            row = newDt.Rows.Count - 1;
            str = String.Format("{0,13:C2}", totalAll);

            int col = newDt.Columns.Count - 1;
            newDt.Rows[row][col] = str;
            newDt.Rows[row][col-1] = "TOTAL ALL";

            dgv.DataSource = newDt;
            ScaleCells();
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
            if ( dgv2.Visible )
                printableComponentLink1.Component = dgv2;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

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
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;

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
            string title = "New Trust Written - By Location";
            if ( dgv2.Visible )
                title = "New Trust Written -By Agent";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            DateTime date = DateTime.Now;
            string workDate = reportDate.ToString("MMMMMMMMMMMMM") + "  " + reportDate.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Regular);
            Printer.DrawQuad(20, 8, 5, 4, " " + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            int rowHandle = e.RowHandle;
            if (!gridMain.IsDataRow(rowHandle))
            {
                DevExpress.XtraPrinting.BrickGraphics brick = (DevExpress.XtraPrinting.BrickGraphics)e.BrickGraphics;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
        }
        /****************************************************************************************/
        private void chkReverse_CheckedChanged(object sender, EventArgs e)
        {
            if (!chkReverse.Checked)
            {
                dgv2.Visible = false;
                dgv.Visible = true;
                SummaryView_Load(null, null);
                return;
            }
            dgv.Visible = false;
            dgv2.Visible = true;
            DataTable dt = new DataTable();
            dt.Columns.Add("Loc");
            dt.Columns.Add("agentName");
            dt.Columns.Add("total", Type.GetType("System.Double"));

            DataRow dRow = null;
            string oldLocation = "";
            string location = "";
            string agent = "";
            double contractValue = 0D;

            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                location = workDt.Rows[i]["loc"].ObjToString();
                if (String.IsNullOrWhiteSpace(location))
                    continue;
                if (String.IsNullOrWhiteSpace(oldLocation))
                    oldLocation = location;
                if (oldLocation != location)
                {
                }
                agent = workDt.Rows[i]["agentName"].ObjToString();
                contractValue = workDt.Rows[i]["total"].ObjToDouble();
                if (contractValue <= 0D)
                    continue;
                dRow = dt.NewRow();
                dRow["Loc"] = location;
                dRow["agentName"] = agent;
                dRow["total"] = contractValue;
                dt.Rows.Add(dRow);
            }
            if (dt.Rows.Count <= 0)
                return;

            DataView tempview = dt.DefaultView;
            tempview.Sort = "agentName asc, Loc asc";
            dt = tempview.ToTable();

            oldLocation = "";
            string oldAgent = "";
            string str = "";
            int row = 0;

            DataTable newDt = new DataTable();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["loc"].ObjToString();
                agent = dt.Rows[i]["agentName"].ObjToString();
                contractValue = dt.Rows[i]["total"].ObjToDouble();
                if (oldAgent != agent)
                {
                    newDt.Columns.Add(agent);
                    if (row >= newDt.Rows.Count)
                    {
                        dRow = newDt.NewRow();
                        newDt.Rows.Add(dRow);
                    }
                    oldAgent = agent;
                    oldLocation = "";
                    row = 0;
                    //newDt.Rows[row][agent] = location;
                }
                if (location != oldLocation)
                {
                    if (row > 0)
                    {
                        row++;
                        if (row >= newDt.Rows.Count)
                        {
                            dRow = newDt.NewRow();
                            newDt.Rows.Add(dRow);
                        }
                        row++;
                    }
                    if (row >= newDt.Rows.Count)
                    {
                        dRow = newDt.NewRow();
                        newDt.Rows.Add(dRow);
                    }
                    oldLocation = location;
                    newDt.Rows[row][oldAgent] = location;
                }
                row++;
                if (row >= newDt.Rows.Count)
                {
                    dRow = newDt.NewRow();
                    newDt.Rows.Add(dRow);
                }
                dRow = newDt.NewRow();

                str = G1.ReformatMoney(contractValue);
                newDt.Rows[row][oldAgent] = str;
            }
            dRow = newDt.NewRow();
            newDt.Rows.Add(dRow);

            dRow = newDt.NewRow();
            newDt.Rows.Add(dRow);

            row = newDt.Rows.Count - 1;
            double totalContracts = 0D;
            double totalAll = 0D;
            string cmd = "";
            DataTable funDt = null;
            for (int i = 0; i < newDt.Columns.Count; i++)
            {
                totalContracts = 0D;
                for (int j = 0; j < newDt.Rows.Count; j++)
                {
                    contractValue = newDt.Rows[j][i].ObjToDouble();
                    totalContracts += contractValue;
                    if (contractValue > 0D)
                    {
                        str = String.Format("{0,14:C2}", contractValue);
                        newDt.Rows[j][i] = str;
                    }
                    else
                    {
                        str = newDt.Rows[j][i].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            cmd = "Select * from `funeralHomes` where `keycode` = '" + str + "';";
                            funDt = G1.get_db_data(cmd);
                            if (funDt.Rows.Count > 0)
                            {
                                str = funDt.Rows[0]["LocationCode"].ObjToString();
                                newDt.Rows[j][i] = str;
                            }
                        }
                    }
                }
                totalAll += totalContracts;
                str = String.Format("{0,13:C2}", totalContracts);
                newDt.Rows[row][i] = str;
                location = newDt.Columns[i].ColumnName.Trim();
                //cmd = "Select * from `funeralHomes` where `keycode` = '" + location + "';";
                //funDt = G1.get_db_data(cmd);
                //if (funDt.Rows.Count > 0)
                //{
                //    str = funDt.Rows[0]["LocationCode"].ObjToString();
                //    newDt.Columns[i].ColumnName = str;
                //}
            }

            dRow = newDt.NewRow();
            newDt.Rows.Add(dRow);
            row = newDt.Rows.Count - 1;
            str = String.Format("{0,13:C2}", totalAll);

            int col = newDt.Columns.Count - 1;
            newDt.Rows[row][col] = str;
            newDt.Rows[row][col - 1] = "TOTAL ALL";

            dgv2.DataSource = newDt;
            dgv2.RefreshDataSource();
            dgv2.Refresh();
            gridMain2.RefreshData();
            ScaleCells();
        }
        /****************************************************************************************/
        private double originalSize = 0D;
        private Font mainFont = null;
        private double originalSize2 = 0D;
        private Font mainFont2 = null;
        private void ScaleCells()
        {
            if (dgv.Visible)
            {
                if (originalSize == 0D)
                {
                    originalSize = gridMain.Columns[0].AppearanceCell.Font.Size;
                    mainFont = gridMain.Columns[0].AppearanceCell.Font;
                }
                double scale = txtScale.Text.ObjToDouble();
                double size = scale / 100D * originalSize;
                Font font = new Font(mainFont.Name, (float)size);
                for (int i = 0; i < gridMain.Columns.Count; i++)
                {
                    gridMain.Columns[i].AppearanceCell.Font = font;
                }
                gridMain.RefreshData();
                dgv.Refresh();
            }
            else
            {
                if (originalSize2 == 0D)
                {
                    originalSize2 = gridMain2.Columns[0].AppearanceCell.Font.Size;
                    mainFont2 = gridMain2.Columns[0].AppearanceCell.Font;
                }
                double scale = txtScale.Text.ObjToDouble();
                double size = scale / 100D * originalSize2;
                Font font = new Font(mainFont2.Name, (float)size);
                for (int i = 0; i < gridMain2.Columns.Count; i++)
                {
                    gridMain2.Columns[i].AppearanceCell.Font = font;
                }
                gridMain2.RefreshData();
                dgv2.Refresh();
            }
            this.Refresh();
        }
        /****************************************************************************************/
        private void txtScale_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string balance = txtScale.Text.Trim();
                if (!G1.validate_numeric(balance))
                {
                    MessageBox.Show("***ERROR*** Scale must be numeric!");
                    return;
                }
                double money = balance.ObjToDouble();
                balance = G1.ReformatMoney(money);
                txtScale.Text = balance;
                ScaleCells();
                return;
            }
            // Initialize the flag to false.
            bool nonNumberEntered = false;

            // Determine whether the keystroke is a number from the top of the keyboard.
            if (e.KeyCode < Keys.D0 || e.KeyCode > Keys.D9)
            {
                // Determine whether the keystroke is a number from the keypad.
                if (e.KeyCode < Keys.NumPad0 || e.KeyCode > Keys.NumPad9)
                {
                    // Determine whether the keystroke is a backspace.
                    if (e.KeyCode != Keys.Back)
                    {
                        // A non-numerical keystroke was pressed.
                        // Set the flag to true and evaluate in KeyPress event.
                        if (e.KeyCode != Keys.OemPeriod)
                            nonNumberEntered = true;
                    }
                }
            }
            //If shift key was pressed, it's not a number.
            if (Control.ModifierKeys == Keys.Shift)
            {
                nonNumberEntered = true;
            }
            if (nonNumberEntered)
            {
                MessageBox.Show("***ERROR*** Key entered must be a number!");
                e.Handled = true;
            }
        }
        /****************************************************************************************/
    }
}