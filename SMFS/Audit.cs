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
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class Audit : DevExpress.XtraEditors.XtraForm
    {
        string printDate = "";
        DataTable _ModuleList;
        DataTable _FieldList;
        DataTable _UserList;
        DataTable _ComputerUserList;
        DataTable _MachineList;
        DataTable _ContractList;
        /***********************************************************************************************/
        public Audit()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void Audit_Load(object sender, EventArgs e)
        {
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            LoadModuleCombo();
            LoadFieldCombo();
            LoadUserCombo();
            LoadComputerUserCombo();
            LoadMachineCombo();
            LoadContractCombo();
        }
        /***********************************************************************************************/
        void nmenu_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string name = menu.Name;
            int index = getGridColumnIndex(name);
            if (index < 0)
                return;
            if (menu.Checked)
            {
                menu.Checked = false;
                gridMain.Columns[index].Visible = false;
            }
            else
            {
                menu.Checked = true;
                gridMain.Columns[index].Visible = true;
            }
            gridMain.RefreshData();
            dgv.Refresh();
            ToolStripMenuItem xmenu = this.columnsToolStripMenuItem;
            xmenu.ShowDropDown();
        }
        /***********************************************************************************************/
        private int getGridColumnIndex(string columnName)
        {
            int index = -1;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                if (name == columnName)
                {
                    index = i;
                    break;
                }
            }
            return index;
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            string cmd = "Select * from `audit` ";
            string where = " where ";

            string contract = txtContract.Text.Trim();
            if (!String.IsNullOrWhiteSpace(contract))
            {
                cmd += " WHERE `contract` = '" + contract + "' ORDER BY `tmstamp` DESC;";
            }
            else
            {
                string dates = getDateQuery();
                if (!String.IsNullOrWhiteSpace(dates))
                {
                    cmd += " " + where + " " + dates;
                    where = "AND";
                }

                string locations = getModuleQuery();
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    cmd += " " + where + " " + locations;
                    where = "AND";
                }

                string fields = getFieldQuery();
                if (!String.IsNullOrWhiteSpace(fields))
                {
                    cmd += " " + where + " " + fields;
                    where = "AND";
                }

                string users = getUserQuery();
                if (!String.IsNullOrWhiteSpace(users))
                {
                    cmd += " " + where + " " + users;
                    where = "AND";
                }

                string computerusers = getComputerUserQuery();
                if (!String.IsNullOrWhiteSpace(computerusers))
                {
                    cmd += " " + where + " " + computerusers;
                    where = "AND";
                }

                string machines = getMachineQuery();
                if (!String.IsNullOrWhiteSpace(machines))
                {
                    cmd += " " + where + " " + machines;
                    where = "AND";
                }

                string contracts = getContractQuery();
                if (!String.IsNullOrWhiteSpace(contracts))
                {
                    cmd += " " + where + " " + contracts;
                    where = "AND";
                }
                cmd += " ORDER BY `tmstamp` DESC;";
            }

            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            G1.NumberDataTable(dt);

            dgv.DataSource = dt;
            gridMain.BestFitColumns();
        }
        /*******************************************************************************************/
        private string getDateQuery()
        {
            string dates = "";
            printDate = "";
            if (chkDate.Checked)
            {
                DateTime date = this.dateTimePicker1.Value;
                string date1 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2") + " 00:00:00";

                date = this.dateTimePicker2.Value;
                string date2 = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2") + " 23:59:59";

                dates = " `tmstamp` BETWEEN '" + date1 + "' AND '" + date2 + "' ";
                printDate = "Search Date : " + date1;
                printDate += " -to- " + date2;
            }
            return dates;
        }
        /*******************************************************************************************/
        private string getModuleQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboModule.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `module` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getFieldQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboField.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `field` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getUserQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboUsers.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `user` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getComputerUserQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboComputerUser.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `computerUserName` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getMachineQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboMachine.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `machineName` IN (" + procLoc + ") " : "";
        }
        /*******************************************************************************************/
        private string getContractQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboContract.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `contract` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void LoadModuleCombo()
        {
            string cmd = "SELECT `module` FROM `audit` GROUP BY `module` ASC;";
            _ModuleList = G1.get_db_data(cmd);

            chkComboModule.Properties.DataSource = _ModuleList;
        }
        /***********************************************************************************************/
        private void LoadFieldCombo()
        {
            string cmd = "SELECT `field` FROM `audit` GROUP BY `field` ASC;";
            _FieldList = G1.get_db_data(cmd);

            chkComboField.Properties.DataSource = _FieldList;
        }
        /***********************************************************************************************/
        private void LoadUserCombo()
        {
            string cmd = "SELECT `user` FROM `audit` GROUP BY `user` ASC;";
            _UserList = G1.get_db_data(cmd);

            chkComboUsers.Properties.DataSource = _UserList;
        }
        /***********************************************************************************************/
        private void LoadMachineCombo()
        {
            string cmd = "SELECT `machineName` FROM `audit` GROUP BY `machineName` ASC;";
            _MachineList = G1.get_db_data(cmd);

            chkComboMachine.Properties.DataSource = _MachineList;
        }
        /***********************************************************************************************/
        private void LoadContractCombo()
        {
            string cmd = "SELECT `contract` FROM `audit` GROUP BY `contract` ASC;";
            _ContractList = G1.get_db_data(cmd);

            chkComboContract.Properties.DataSource = _ContractList;
        }
        /***********************************************************************************************/
        private void LoadComputerUserCombo()
        {
            string cmd = "SELECT `computerUserName` FROM `audit` GROUP BY `computerUserName` ASC;";
            _ComputerUserList = G1.get_db_data(cmd);

            chkComboComputerUser.Properties.DataSource = _ComputerUserList;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadData();
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
                gridMain.OptionsFind.AlwaysVisible = false;
            else
                gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        public static void CheckAuditField ( string module, string field, DataRow oldDt, DataRow newDt)
        {
            if (oldDt == null)
                return;
            if (newDt == null)
                return;
            string what = "";
            string record = "";
            string oldData = "";
            string newData = "";
            try
            {
                oldData = oldDt[field].ObjToString();
                newData = newDt[field].ObjToString();
                if (oldData == newData)
                    return;
                what = oldData + " to " + newData;
                G1.AddToAudit(LoginForm.username, module, field, what);
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Audit Record for Module " + module + " Field " + field + " Message " + what + " Record " + record);
            }
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            this.dateTimePicker1.Value = date;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            this.dateTimePicker1.Value = date;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            date = new DateTime(date.Year, date.Month, days);
            this.dateTimePicker2.Value = date;
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
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

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
            //Printer.DrawQuadBorder(1, 1, 12, 6, BorderSide.All, 1, Color.Black);
            //Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 1, 2, 3, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 1, 2, 3, Color.Black, BorderSide.None, font);

            Printer.DrawQuad(1, 3, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 3, 2, 3, "Audit Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            font = new Font("Ariel", 7, FontStyle.Regular);
            //Printer.DrawQuad(1, 5, 4, 1, "Contract :" + workContract, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 5, 3, 1, "Name :" + workName, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuadBorder(1, 1, 12, 5, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 6, BorderSide.Right, 1, Color.Black);

            string contracts = chkComboContract.Text.Trim();
            string contract = txtContract.Text.Trim();
            if (!String.IsNullOrWhiteSpace(contract))
                contracts = contract;

            Printer.DrawQuad(1, 6, 3, 1, "Users: " + chkComboUsers.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 7, 3, 1, "Module: " + chkComboModule.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 8, 3, 1, "Field: " + chkComboField.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 9, 3, 1, "C-User: " + chkComboComputerUser.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 10, 3, 1, "Machine: " + chkComboMachine.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            Printer.DrawQuad(1, 11, 3, 1, "Contract: " + contracts, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            //Printer.DrawQuad(3, 6, 3, 1, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 7, 3, 1, lblIssueDate.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 8, 3, 1, lblNumPayments.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(3, 9, 3, 1, lblTotalPaid.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);



            //Printer.DrawQuadBorder(1, 1, 12, 11, BorderSide.All, 1, Color.Black);
            ////            Printer.DrawQuadTicks();
        }
        /***********************************************************************************************/
        private void btnSystem_Click(object sender, EventArgs e)
        {
            chkComboUsers.Text = "System";
            LoadData();
        }
        /***********************************************************************************************/
    }
}