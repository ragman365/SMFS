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
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Columns;

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class CasketsLocations : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool alreadyLoaded = false;
        private bool modified = false;
        private bool Selecting = false;
        private DataTable workDt = null;
        private DataTable originalDt = null;
        private string workContract = "";
        private bool loading = false;
        private string loadedPackage = "";
        private string loadededLocation = "";
        private string workLocation = "";
        private string workCasketGroup = "";
        private string workFrom = "";
        private bool workingLocation = false;
        /***********************************************************************************************/
        private bool casketsModified = false;
        private bool vaultsModified = false;
        private bool urnsModified = false;
        private bool miscModified = false;
        /***********************************************************************************************/
        public CasketsLocations(bool selecting, DataTable dt = null, string contract = "")
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            originalDt = dt.Copy();
            workContract = contract;
        }
        /***********************************************************************************************/
        public CasketsLocations(string fromCasketGroup, string fromLocation, bool selecting, DataTable dt = null )
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            originalDt = dt.Copy();
            workContract = "";
            workFrom = "";
            workCasketGroup = fromCasketGroup;
            workLocation = fromLocation;
            this.Text = "Custom Location Prices for " + workCasketGroup + " and Location " + workLocation;
        }
        /***********************************************************************************************/
        private void CasketsLocations_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            loading = true;
            if (!Selecting)
            {
                LoadGroupCombo();
                LoadGPLGroupCombo();
                gridMain.OptionsBehavior.ReadOnly = false;
                gridMain.Columns["total"].Visible = false;
            }
            else
            {
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["total"].Visible = false;
            }
            LoadData();
            if (Selecting)
            {
                pictureAdd.Hide();
                pictureDelete.Hide();
                btnInsert.Hide();
                picRowDown.Hide();
                picRowUp.Hide();
                gridMain.Columns["select"].Visible = true;
                gridMain.Columns["SameAsMaster"].Visible = true;
                ReSelectServices();
                this.Text = "Services for Contract (" + workContract + ")";
            }
            loading = false;
        }
        /***********************************************************************************************/
        private void ReSelectServices()
        {
            if (workDt == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            string availableService = "";
            string service = "";
            double price = 0D;
            double price1 = 0D;
            double cost = 0D;
            string data = "";
            string type = "";
            string same = "";
            bool found = false;
            bool serviceRecord = false;
            string record = "";
            if (G1.get_column_number(workDt, "!serviceRecord") < 0)
            {
                serviceRecord = true;
            }
            for (int i = 0; i < workDt.Rows.Count; i++)
            {
                service = workDt.Rows[i]["casketdesc"].ObjToString();
                price = workDt.Rows[i]["price"].ObjToDouble();
                //data = workDt.Rows[i]["data"].ObjToString();
                //price1 = workDt.Rows[i]["price1"].ObjToDouble();
                //cost = workDt.Rows[i]["cost"].ObjToDouble();
                //type = workDt.Rows[i]["type"].ObjToString();
                //same = workDt.Rows[i]["SameAsMaster"].ObjToString();
                found = false;
                for (int j = 0; j < dt.Rows.Count; j++)
                {
                    availableService = dt.Rows[j]["casketdesc"].ObjToString();
                    if (availableService == service)
                    {
                        dt.Rows[j]["select"] = "1";
                        dt.Rows[j]["casketcost"] = price;
//                        dt.Rows[j]["data"] = cost;
                        //if (same == "1")
                        //{
                        //    dt.Rows[j]["price"] = price1;
                        //    dt.Rows[j]["data"] = data;
                        //    dt.Rows[j]["SameAsMaster"] = "1";
                        //}
                        //if (!String.IsNullOrWhiteSpace(type))
                        //    dt.Rows[j]["type"] = type;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    DataRow dRow = dt.NewRow();
                    dRow["service"] = service;
                    dRow["data"] = data;
                    dRow["type"] = type;
                    dRow["select"] = "1";
                    dt.Rows.Add(dRow);
                }
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            ReCalcTotal();
        }
        /***********************************************************************************************/
        private void LoadMasterVaults()
        {
            string cmd = "Select * from `casket_master` where `casketcode` like 'V%';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("casket", Type.GetType("System.Double"));
            dt.Columns.Add("package", Type.GetType("System.Double"));

            //            dt.Columns.Add("SameAsMaster");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            SetupSelection(dt);
            //            SetupSameAsMaster(dt);
//            CalculateCosts(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadPackage()
        {
            string serviceRecord = "";
            string group = GetGroup();
            if (String.IsNullOrWhiteSpace(group))
            {
                MessageBox.Show("***ERROR*** Empty group or package!");
                return;
            }

            LoadPackagesCombo();

            loadededLocation = group;
            string list = "";
            string cmd = "Select * from `casket_packages` where `groupname` = '" + group + "';";
            DataTable dx = G1.get_db_data(cmd);
            if ( dx.Rows.Count <= 0 && group.ToUpper() == "MASTER VAULT")
            {
                LoadMasterVaults();
                return;
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                serviceRecord = dx.Rows[i]["!masterRecord"].ObjToString();
                if (String.IsNullOrWhiteSpace(serviceRecord))
                    continue;
                list += "'" + serviceRecord + "',";
            }
            list = list.TrimEnd(',');
            if (!String.IsNullOrWhiteSpace(list))
            {
                cmd = "Select * from `casket_packages` p RIGHT JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where s.`record` IN (" + list + ") ";
                cmd += " and `groupname` = '" + group + "' ";
                cmd += ";";
            }
            else
                cmd = "Select * from `casket_packages` p JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where `groupname` = 'xyzzyxxxx';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
            dt.Columns.Add("casket", Type.GetType("System.Double"));
            dt.Columns.Add("package", Type.GetType("System.Double"));

            //            dt.Columns.Add("SameAsMaster");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            SetupSelection(dt);
            //            SetupSameAsMaster(dt);
            CalculateCosts(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void CalculateCosts ( DataTable dt)
        {
            double cost = 0D;
            double markup = 0D;
            double casketCost = 0D;
            double packageCost = 0D;
            string cmd = "";
            string gplGroup = "";
            string masterRecord = "";
            DataTable dx = null;
            string basicRecord = "";
            double basicPackage = GetGPGBasicPackage();
            bool gotMarkup = false;
            if (G1.get_column_number(dt, "markup") >= 0)
                gotMarkup = true;
            if (G1.get_column_number(dt, "package") < 0 )
                dt.Columns.Add("package", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "futurecasket") < 0)
                dt.Columns.Add("futurecasket", Type.GetType("System.Double"));
            string casketcode = "";
            string casketdesc = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    markup = 1D;
                    if ( gotMarkup )
                        markup = dt.Rows[i]["markup"].ObjToDouble();
                    casketcode = dt.Rows[i]["casketcode"].ObjToString();
                    casketdesc = dt.Rows[i]["casketdesc"].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( casketcode))
                        cmd = "Select * from `casket_master` where `casketcode` = '" + casketcode + "';";
                    else
                        cmd = "Select * from `casket_master` where `casketdesc` = '" + casketdesc + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        casketCost = dx.Rows[0]["casketcost"].ObjToDouble();
                        casketCost = casketCost * markup;
                        dt.Rows[i]["casket"] = casketCost;
                        dt.Rows[i]["package"] = casketCost + basicPackage;
                    }
                    markup = dt.Rows[i]["futureMarkup"].ObjToDouble();
                    casketCost = dt.Rows[i]["futureCasketcost"].ObjToDouble();
                    if ( markup > 0D)
                    {
                        casketCost = casketCost * markup;
                        dt.Rows[i]["futurecasket"] = casketCost;
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private void LoadDataxxx()
        {
            btnSave.Hide();
            modified = false;
            string location = GetGroup();
            if (location.ToUpper() == "ALL BATESVILLE CASKETS")
                return;
            if (location.ToUpper() == "CRECENT URNS")
                return;
            if (location.ToUpper() == "MASTER VAULT")
                return;
            this.Cursor = Cursors.WaitCursor;
            gridMain.Columns["round"].Visible = true;
            if (location.ToUpper() != "MASTER")
            {
                //picMoveFuture.Hide();
                //picMovePast.Hide();
                gridMain.Columns["markup"].Visible = true;
                gridMain.Columns["futuremarkup"].Visible = true;
                gridMain.Columns["round"].Visible = false;
            }
            if (!Selecting)
            {
                if (location != "Master")
                {
                    LoadPackage();
                    /////////////LoadCasketLocations();
                    //tabControl1.TabPages.Remove(tabVaults);
                    //tabControl1.TabPages.Remove(tabUrns);
                    //tabControl1.TabPages.Remove(tabMisc);
                    pictureAdd.Hide();
                    pictureDelete.Hide();
                    btnInsert.Hide();
                    picRowDown.Hide();
                    picRowUp.Hide();
                    gridMain.Columns["markup"].Visible = true;
                    gridMain.Columns["futuremarkup"].Visible = true;
                    gridMain.Columns["futurecasket"].Visible = true;
                    gridMain.Columns["round"].Visible = false;
                    gridMain2.Columns["markup"].Visible = true;
                    gridMain2.Columns["futuremarkup"].Visible = true;
                    gridMain2.Columns["futurecasket"].Visible = true;
                    gridMain2.Columns["round"].Visible = false;
                    gridMain3.Columns["markup"].Visible = true;
                    gridMain3.Columns["futuremarkup"].Visible = true;
                    gridMain3.Columns["futurecasket"].Visible = true;
                    gridMain3.Columns["round"].Visible = false;
                    gridMain4.Columns["markup"].Visible = true;
                    gridMain4.Columns["futuremarkup"].Visible = true;
                    gridMain4.Columns["futurecasket"].Visible = true;
                    gridMain4.Columns["round"].Visible = false;

                    //FixArrowPosition(gridMain);
                    //FixArrowPosition(gridMain2);
                    //FixArrowPosition(gridMain3);
                    //FixArrowPosition(gridMain4);
                    return;
                }
                else
                {
                    pictureAdd.Show();
                    pictureDelete.Show();
                    btnInsert.Show();
                    picRowDown.Show();
                    picRowUp.Show();
                }
            }
            picMoveFuture.Show();
            picMovePast.Show();
            gridMain.Columns["markup"].Visible = false;
            gridMain.Columns["futuremarkup"].Visible = false;
            gridMain.Columns["futurecasket"].Visible = false;
            //string cmd = "Select * from `casket_master` order by `record`;";
            string cmd = "Select * from `casket_master` order by `order`;";
            DataTable dt = null;
            if (Selecting && workingLocation && workDt != null)
                dt = workDt;
            else
            {
                dt = G1.get_db_data(cmd);
                dt.Columns.Add("num");
                dt.Columns.Add("mod");
                dt.Columns.Add("select");
                dt.Columns.Add("casket", Type.GetType("System.Double"));
                dt.Columns.Add("package", Type.GetType("System.Double"));
                dt.Columns.Add("total", Type.GetType("System.Double"));
                dt.Columns.Add("futurecasket", Type.GetType("System.Double"));
            }
            if (G1.get_column_number(dt, "SameAsMaster") < 0)
                dt.Columns.Add("SameAsMaster");
            SetupSelection(dt);
            SetupTax(dt);
            SetupSameAsMaster(dt);

            double basicPackage = GetGPGBasicPackage();

            double pastPrice = 0D;
            double rounding = 0D;
            double packagePrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                pastPrice = dt.Rows[i]["casketCost"].ObjToDouble();
                rounding = dt.Rows[i]["round"].ObjToDouble();
                if (rounding > 0D)
                    pastPrice = Caskets.RoundTo(pastPrice, rounding);
                dt.Rows[i]["casket"] = pastPrice;
                packagePrice = pastPrice + basicPackage;
                dt.Rows[i]["package"] = packagePrice;
            }

            //LoadCasketLocations();

            if (gridMain.Columns["round"].Visible)
            {
                //SetupMaster(dt);
                if (tabControl1.TabPages.Count <= 1)
                {
                    tabControl1.TabPages.Add(tabVaults);
                    tabControl1.TabPages.Add(tabUrns);
                    tabControl1.TabPages.Add(tabMisc);
                }
            }
            else
            {
                //SetupMaster(dt);
                if (tabControl1.TabPages.Count <= 1)
                {
                    tabControl1.TabPages.Add(tabVaults);
                    tabControl1.TabPages.Add(tabUrns);
                    tabControl1.TabPages.Add(tabMisc);
                }
                gridMain2.Columns["select"].Visible = true;
                gridMain3.Columns["select"].Visible = true;
                gridMain4.Columns["select"].Visible = true;
            }

            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataTable dt3 = (DataTable)dgv3.DataSource;
            DataTable dt4 = (DataTable)dgv4.DataSource;

            //LoadLocations(dt, repositoryItemCheckedComboBoxEdit1);
            //LoadLocations(dt2, repositoryItemCheckedComboBoxEdit2);
            //LoadLocations(dt3, repositoryItemCheckedComboBoxEdit3);
            //LoadLocations(dt4, repositoryItemCheckedComboBoxEdit4);

            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void SetupTax(DataTable dt, DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = null)
        {
            if (selectnew == null)
                selectnew = this.repositoryItemCheckEdit9;
            selectnew.NullText = "";
            selectnew.ValueChecked = "Y";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "tax") < 0)
                dt.Columns.Add("tax");
            string tax = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                tax = dt.Rows[i]["tax"].ObjToString().ToUpper();
                if (tax == "Y")
                    dt.Rows[i]["tax"] = "Y";
                else
                    dt.Rows[i]["tax"] = "";
            }
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            gridMain.Columns["locations"].Visible = false;
            gridMain2.Columns["locations"].Visible = false;
            gridMain3.Columns["locations"].Visible = false;
            gridMain4.Columns["locations"].Visible = false;
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `casket_locations` where `casketGroup` = '" + workCasketGroup + "' AND `location` = '" + workLocation + "';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");
            dt.Columns.Add("package", Type.GetType("System.Double"));
            CalculateCosts(dt);
            SetupTax(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void SetupSameAsMaster(DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["SameAsMaster"] = "0";
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
        private void repositoryItemCheckEdit3_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain.FocusedRowHandle;
            try
            {
                string select = dr["select"].ObjToString();
                string doit = "1";
                if (select == "1")
                    doit = "0";
                //string doit = "0";
                //if (select == "1")
                //    doit = "1";
                dr["select"] = doit;
            }
            catch ( Exception ex)
            {

            }
            modified = true;
            if ( !Selecting || workingLocation )
                btnSave.Show();
            ReCalcTotal();
            gridMain.RefreshData();
            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_dt(DataTable dt);
        public event d_void_eventdone_dt SelectDone;
        protected void OnSelectDone(DataTable dt)
        {
            SelectDone?.Invoke(dt);
        }
        /***********************************************************************************************/
        private void ReCalcTotal()
        {
            string select = "";
            double price = 0D;
            double total = 0D;
            string data = "";
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    select = dt.Rows[i]["select"].ObjToString();
                    if (select == "1")
                    {
                        price = dt.Rows[i]["casketcost"].ObjToDouble();
                        total += price;
                        //data = dt.Rows[i]["data"].ObjToString();
                        //if (G1.validate_numeric(data))
                        //{
                        //    price = data.ObjToDouble();
                        //    total = total + price;
                        //    dt.Rows[i]["total"] = total;
                        //}
                    }
                    else
                        dt.Rows[i]["total"] = 0D;
                }
            }
            catch ( Exception ex)
            {

            }
        }
        /***********************************************************************************************/
        private bool CheckForSaving()
        {
            if (!modified)
                return true;
            DialogResult result = MessageBox.Show("***Question***\nMerchandise has been modified!\nWould you like to save your changes?", "Select Merchandise Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                return false;
            modified = false;
            if (result == DialogResult.No)
                return true;
            DataTable dt = (DataTable)dgv.DataSource;
            if (String.IsNullOrWhiteSpace(loadededLocation) || String.IsNullOrWhiteSpace(loadedPackage))
                return true;
            if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
                SaveServices();
            else
                SaveServices(dt);
            return true;
        }
        /***********************************************************************************************/
        private void Services_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nMerchandise has been modified!\nWould you like to save your changes?", "Select Merchandise Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (!Selecting)
            {
                //loadededLocation = cmbGroups.Text.ToUpper();
                //loadedPackage = cmbPackage.Text.ToUpper();
                //if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
                //    SaveServices();
                //else
                //    SaveServices(dt);
                return;
            }
            DataTable dx = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["select"].ObjToString() == "1")
                    G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
            }
            OnSelectDone(dx);
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

            font = new Font("Ariel", 10, FontStyle.Bold);
            if (Selecting)
                Printer.DrawQuad(6, 8, 4, 4, this.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(6, 8, 4, 4, "Services Report", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            DateTime date = DateTime.Now;
            string workDate = date.Month.ToString("D2") + "/" + date.Year.ToString("D4");
            Printer.SetQuadSize(24, 12);
            font = new Font("Ariel", 9, FontStyle.Bold);
            //            Printer.DrawQuad(20, 8, 5, 4, "Report Month:" + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(16, 8, 3, 4, lblPayment.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(19, 8, 3, 4, lblTrust85.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            //Printer.DrawQuad(22, 8, 3, 4, lblTrust100.Text, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureAdd_Click(object sender, EventArgs e)
        {
            DataTable tempDt = originalDt.Copy();
            Caskets serviceForm = new Caskets(true, true, workCasketGroup, tempDt);
            serviceForm.SelectDone += ServiceForm_SelectDone1;
            serviceForm.Show();
        }
        /***********************************************************************************************/
        private void ServiceForm_SelectDone1(DataTable dt )
        {
            DataTable dx = (DataTable)dgv.DataSource;
            try
            {
                string service = "";
                string price = "";
                string futurePrice = "";
                string pastPrice = "";
                string type = "";
                DataRow[] dRows = null;
                DataRow dR = null;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    service = dt.Rows[i]["casketdesc"].ObjToString();
                    if (String.IsNullOrWhiteSpace(service))
                        continue;
                    dRows = dx.Select("casketdesc='" + service + "'");
                    if (dRows.Length > 0)
                        continue;
                    dR = dx.NewRow();
                    dR["casketdesc"] = service;
                    dR["casketcode"] = dt.Rows[i]["casketcode"].ObjToString();
                    price = dt.Rows[i]["casketprice"].ObjToString();
                    dR["price"] = price;
                    price = dt.Rows[i]["casket"].ObjToString();
                    dR["casket"] = price;
                    price = dt.Rows[i]["markup"].ObjToString();
                    dR["markup"] = price;
                    price = dt.Rows[i]["casketcost"].ObjToString();
                    dR["casketcost"] = price;
                    futurePrice = dt.Rows[i]["futureCasketCost"].ObjToString();
                    dR["futureCasketCost"] = futurePrice;
                    pastPrice = dt.Rows[i]["pastCasketCost"].ObjToString();
                    dR["pastCasketCost"] = pastPrice;
                    pastPrice = dt.Rows[i]["pastmarkup"].ObjToString();
                    dR["pastmarkup"] = pastPrice;
                    pastPrice = dt.Rows[i]["futuremarkup"].ObjToString();
                    dR["futuremarkup"] = pastPrice;
                    pastPrice = dt.Rows[i]["round"].ObjToString();
                    dR["round"] = pastPrice;
                    dR["tax"] = "";
                    dR["taxAmount"] = 0D;
                    type = "Merchandise";
                    dR["type"] = type;
                    dx.Rows.Add(dR);
                }
                CalculateCosts(dx);
            }
            catch ( Exception ex)
            {
            }
            G1.NumberDataTable(dx);
            dgv.DataSource = dx;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string service = dr["casketdesc"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete (" + service + ") ?", "Delete Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            int[] rows = gridMain.GetSelectedRows();
            int dtRow = 0;
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            int row = 0;
            try
            {
                loading = true;
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dtRow = gridMain.GetDataSourceRowIndex(row);
                    if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                    {
                        continue;
                    }
                    var dRow = gridMain.GetDataRow(row);
                    if (dRow != null)
                        dRow["mod"] = "D";
                    dt.Rows[dtRow]["mod"] = "D";
                    modified = true;
                    if ( !Selecting )
                        btnSave.Show();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            loading = false;
            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
        }
        /***********************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            //MoveRowUp(dt, rowHandle);
            massRowsUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void massRowsUp(DataTable dt, int row)
        {
            int[] rows = gridMain.GetSelectedRows();
            int firstRow = 0;
            if (rows.Length > 0)
                firstRow = rows[0];
            try
            {
                G1.NumberDataTable(dt);
                dt.Columns.Add("Count", Type.GetType("System.Int32"));
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["Count"] = i.ToString();
                int moverow = rows[0];
                for (int i = 0; i < rows.Length; i++)
                {
                    row = rows[i];
                    dt.Rows[row]["Count"] = (row - 1).ToString();
                    //dt.Rows[row - 1]["Count"] = row.ToString();
                    //var dRow = gridMain.GetDataRow(row);
                    dt.Rows[row]["mod"] = "M";
                    modified = true;
                    if ( !Selecting )
                        btnSave.Show();
                }
                dt.Rows[moverow - 1]["Count"] = (moverow + (rows.Length - 1)).ToString();
                G1.sortTable(dt, "Count", "asc");
                dt.Columns.Remove("Count");
                G1.NumberDataTable(dt);
            }
            catch (Exception ex)
            {
                MessageBox.Show("*ERROR*** " + ex.Message.ToString());
            }

            //            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void picRowDown_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            MoveRowDown(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle + 1);
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***************************************************************************************/
        private void MoveRowDown(DataTable dt, int row)
        {
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row + 1).ToString();
            dt.Rows[row + 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Num"] = (i + 1).ToString();
        }
        /***********************************************************************************************/
        private void btnInsert_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
            if (dtRow < 0 || dtRow > (dt.Rows.Count - 1))
                return;
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /***********************************************************************************************/
        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditHelp helpForm = new EditHelp("customers");
            helpForm.Show();
        }
        /***********************************************************************************************/
        private void SaveServices()
        {
            if (1 == 1)
                return; // Don't want to mess up services here
            string record = "";
            string service = "";
            string data = "";
            string type = "";
            string price = "";
            string mod = "";

            DataTable dt = (DataTable)dgv.DataSource;
            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod.ToUpper() == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("services", "record", record);
                    continue;
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("services", "service", "-1");
                if (G1.BadRecord("services", record))
                    continue;
                service = dt.Rows[i]["service"].ObjToString();
                price = dt.Rows[i]["price"].ObjToString();
                data = dt.Rows[i]["data"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                G1.update_db_table("services", "record", record, new string[] { "service", service, "price", price, "data", data, "type", type, "order", i.ToString() });
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChangedxx(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
            if (!Selecting)
            {
                btnSave.Show();
                DataTable dt = (DataTable)dgv.DataSource;
                double casketCost = 0D;
                double markup = 0D;
                double basicPackage = GetGPGBasicPackage();               
                DataRow dr = gridMain.GetFocusedDataRow();
                if (e.Column.FieldName.Trim().ToUpper() == "MARKUP")
                {
                    markup = dr["markup"].ObjToDouble();
                    casketCost = dr["casketcost"].ObjToDouble();
                    casketCost = casketCost * markup;
                    dr["casket"] = casketCost;
                    //dr["package"] = casketCost + basicPackage;
                }
            }
        }
        /***********************************************************************************************/
        public static double RoundTo(double price, double rounding)
        {
            double newPrice = price;
            try
            {
                if (rounding <= 0D)
                    return newPrice;
                if (newPrice == 0D)
                    return newPrice;
                if (rounding == 1000D)
                    newPrice = G1.Round(price, -3);
                else if (rounding == 500D)
                {
                    double count = price / 500D;
                    count = G1.RoundValue(count);
                    int icount = Convert.ToInt32(count);
                    count = (double)icount;
                    newPrice = count * 500D;
                }
                else if (rounding == 100D)
                {
                    newPrice = G1.Round(price, -2);
                }
                else if (rounding == 95D)
                {
                    newPrice = G1.Round(price, -2);
                    newPrice = newPrice - 5D;
                }
                else if (rounding == 5D)
                {
                    double count = price / 5D;
                    count = G1.RoundValue(count);
                    int icount = Convert.ToInt32(count);
                    count = (double)icount;
                    newPrice = count * 5D;
                }
                else if (rounding == 9D)
                {
                    int rndVal = (int)(Math.Round(price / 10, MidpointRounding.AwayFromZero) * 10) - 1;
                    newPrice = Convert.ToDouble(rndVal);
                    if ((price % 10D) != 0D)
                    {
                        if ((price % 10D) != 9D)
                        {
                            if ((price % 5D) == 4D)
                                newPrice += 10D;
                        }
                    }
                }
                else
                {
                    double count = price / rounding;
                    count = G1.RoundValue(count);
                    int icount = Convert.ToInt32(count);
                    count = (double)icount;
                    newPrice = count * rounding;
                }
            }
            catch (Exception ex)
            {
            }
            return newPrice;
        }
        /***********************************************************************************************/
        private void CellModified(AdvBandedGridView gridMain, GridControl dgv, ref bool modified)
        {
            modified = true;
            if (!Selecting)
            {
                btnSave.Show();
                DataTable dt = (DataTable)dgv.DataSource;
                bool gotMarkup = false;
                if (G1.get_column_number(dt, "markup") >= 0)
                    gotMarkup = true;
                double casketCost = 0D;
                double price = 0D;
                double markup = 0D;
                double rounding = 0D;
                double basicPackage = GetGPGBasicPackage();
                DataRow dr = gridMain.GetFocusedDataRow();
                GridColumn currCol = gridMain.FocusedColumn;
                string currentColumn = currCol.FieldName;
                if (currentColumn.ToUpper() == "ROUND")
                {
                    if (gotMarkup)
                        markup = dr["markup"].ObjToDouble();
                    casketCost = dr["casketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    if (gotMarkup)
                        casketCost = casketCost * markup;
                    dr["casket"] = casketCost;
                    dr["package"] = casketCost + basicPackage;

                    if (gotMarkup)
                        markup = dr["futuremarkup"].ObjToDouble();
                    casketCost = dr["futurecasketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    if (gotMarkup)
                        casketCost = casketCost * markup;
                    casketCost = RoundTo(casketCost, rounding);
                    dr["futurecasket"] = casketCost;
                }
                else if (currentColumn.ToUpper() == "FUTUREMARKUP")
                {
                    if (gotMarkup)
                        markup = dr["futuremarkup"].ObjToDouble();
                    casketCost = dr["futurecasketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    if (gotMarkup)
                        casketCost = casketCost * markup;
                    casketCost = RoundTo(casketCost, rounding);
                    dr["futurecasket"] = casketCost;
                }
                if (currentColumn.ToUpper() == "MARKUP")
                {
                    if (gotMarkup)
                        markup = dr["markup"].ObjToDouble();
                    casketCost = dr["casketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    if (gotMarkup)
                        casketCost = casketCost * markup;
                    dr["casket"] = casketCost;
                    dr["package"] = casketCost + basicPackage;
                }
                else if (currentColumn.ToUpper() == "CASKET" && gotMarkup)
                {
                    casketCost = dr["casketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    price = dr["casket"].ObjToDouble();
                    if (casketCost > 0D)
                    {
                        markup = price / casketCost;
                        markup = G1.RoundValue(markup);
                        dr["markup"] = markup;
                    }
                }
                else if (currentColumn.ToUpper() == "FUTURECASKETCOST" && gotMarkup)
                {
                    if (gotMarkup)
                        markup = dr["futuremarkup"].ObjToDouble();
                    casketCost = dr["futurecasketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    if (gotMarkup)
                        casketCost = casketCost * markup;
                    casketCost = RoundTo(casketCost, rounding);
                    dr["futurecasket"] = casketCost;
                }
                else if (currentColumn.ToUpper() == "FUTURECASKET" && gotMarkup)
                {
                    casketCost = dr["futurecasketcost"].ObjToDouble();
                    rounding = dr["round"].ObjToDouble();
                    price = dr["futurecasket"].ObjToDouble();
                    if (casketCost > 0D)
                    {
                        markup = price / casketCost;
                        markup = G1.RoundValue(markup);
                        dr["futuremarkup"] = markup;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            if (dgv.Visible)
                CellModified(gridMain, dgv, ref casketsModified);
            else if (dgv2.Visible)
                CellModified(gridMain2, dgv2, ref vaultsModified);
            else if (dgv3.Visible)
                CellModified(gridMain3, dgv3, ref urnsModified);
            else if (dgv4.Visible)
                CellModified(gridMain4, dgv4, ref miscModified);

            if (casketsModified || vaultsModified || urnsModified || miscModified)
                btnSave.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            if (G1.get_column_number(dt, "mod") < 0)
                return;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                if (e.RowHandle >= 0)
                {
                    string data = e.DisplayText.ObjToString();
                    if (G1.validate_numeric(data))
                    {
                        double dvalue = data.ObjToDouble();
                        e.DisplayText = G1.ReformatMoney(dvalue);
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_RowCellStyle(object sender, DevExpress.XtraGrid.Views.Grid.RowCellStyleEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.Column.FieldName.ToUpper() == "DATA")
            {
                string str = View.GetRowCellValue(e.RowHandle, "data").ObjToString();
                if (str != null)
                {
                    if (G1.validate_numeric(str))
                        e.Appearance.TextOptions.HAlignment = HorzAlignment.Far;
                }
            }
        }
        /***********************************************************************************************/
        private void LoadGPLGroupCombo()
        {
            //string cmd = "Select * from `funeral_groups` ORDER BY `order`,`record`;";
            //DataTable dt = G1.get_db_data(cmd);
            //string groupname = "";
            //cmbGPLGroup.Items.Clear();
            //string firstGroup = "";
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    groupname = dt.Rows[i]["shortname"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(firstGroup))
            //        firstGroup = groupname;
            //    cmbGPLGroup.Items.Add(groupname);
            //}
            //cmbGPLGroup.Text = firstGroup;
        }
        /***********************************************************************************************/
        private void LoadGroupCombo()
        {
            //string cmd = "Select * from `casket_groups` ORDER BY `order`,`record`;";
            //DataTable dt = G1.get_db_data(cmd);
            //string groupname = "";
            //string name = "";
            //string locationCode = "";
            //string str = "";
            //cmbGroups.Items.Clear();
            //cmbGroups.Items.Add("Master");
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    groupname = dt.Rows[i]["shortname"].ObjToString();
            //    cmbGroups.Items.Add(groupname);
            //}
            //cmbGroups.Text = "Master";
        }
        /***********************************************************************************************/
        private void LoadPackagesCombo()
        {
            //string group = GetGroup();
            //if (String.IsNullOrWhiteSpace(group))
            //    return;
            //string cmd = "Select * from `packages` where `groupname` = '" + group + "' GROUP BY `PackageName`;";
            //DataTable dt = G1.get_db_data(cmd);
            //string firstPackage = "";
            //string package = "";
            //loading = true;
            //cmbPackage.Items.Clear();
            //cmbPackage.Items.Add("Master");
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    package = dt.Rows[i]["PackageName"].ObjToString();
            //    if (String.IsNullOrWhiteSpace(package))
            //        continue;
            //    if (String.IsNullOrWhiteSpace(firstPackage))
            //        firstPackage = package;
            //    cmbPackage.Items.Add(package);
            //}
            //cmbPackage.Text = "Master";
            //loading = false;
        }
        /***********************************************************************************************/
        private string GetGroup()
        {
            string location = "";
            if (location.ToUpper() == "MASTER")
                return location;
            return location;
        }
        /***********************************************************************************************/
        private void cmbLocation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!CheckForSaving())
                return;
            LoadData();
        }
        /***********************************************************************************************/
        private void btnAddPackage_Click(object sender, EventArgs e)
        {
            string packName = "";
            using (Ask askForm = new Ask("Enter New Package Name?"))
            {
                askForm.Text = "";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                packName = askForm.Answer;
                if (String.IsNullOrWhiteSpace(packName))
                    return;
            }
            LoadPackage(packName);
        }
        /***********************************************************************************************/
        private void LoadPackage(string packName)
        {
            loadedPackage = packName;
            string group = GetGroup();
            loadededLocation = group;
            string cmd = "Select * from `casket_packages` p JOIN `casket_master` s ON p.`!masterRecord` = s.`record` where p.`groupname` = '" + group + "';";
            DataTable dt = G1.get_db_data(cmd);
            Caskets serviceForm = new Caskets("CasketPackages", true, dt, "");
            serviceForm.SelectDone += ServiceForm_SelectDone;
            serviceForm.Show();
        }
        ///***********************************************************************************************/
        //private void LoadPackage(string packName)
        //{
        //    loadedPackage = packName;
        //    string group = GetGroup();
        //    loadededLocation = group;
        //    string cmd = "Select * from `packages` p JOIN `services` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
        //    DataTable dt = G1.get_db_data(cmd);
        //    Services serviceForm = new Services("Packages", true, dt, "");
        //    serviceForm.SelectDone += ServiceForm_SelectDone;
        //    serviceForm.Show();
        //}
        /***********************************************************************************************/
        private void ServiceForm_SelectDone(DataTable dt)
        {
            SaveServices(dt);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            if ( !alreadyLoaded )
                dgv.DataSource = dt;
            alreadyLoaded = false;
        }
        /***********************************************************************************************/
        private void SaveServices(DataTable dt)
        {
            string casketdesc = "";
            string casketcode = "";
            double price = 0D;
            string type = "";
            string record = "";
            double markup = 0D;
            double casket = 0.00;
            double casketcost = 0.00;
            double futureCasketCost = 0.00;
            double pastCasketCost = 0.00;
            double futureMarkup = 0D;
            double pastMarkup = 0D;
            string mod = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if ( mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("casket_locations", "record", record);
                    continue;
                }
                if ( String.IsNullOrWhiteSpace ( record))
                {
                    record = G1.create_record("casket_locations", "casketdesc", "-1");
                    if (G1.BadRecord("casket_locations", record))
                        continue;
                }
                casketdesc = dt.Rows[i]["casketdesc"].ObjToString();
                casketcode = dt.Rows[i]["casketcode"].ObjToString();
                price = dt.Rows[i]["casketcost"].ObjToDouble();
                markup = dt.Rows[i]["markup"].ObjToDouble();
                casket = dt.Rows[i]["casket"].ObjToDouble();
                casketcost = dt.Rows[i]["casketcost"].ObjToDouble();
                futureCasketCost = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                pastCasketCost = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                futureMarkup = dt.Rows[i]["futureMarkup"].ObjToDouble();
                pastMarkup = dt.Rows[i]["pastMarkup"].ObjToDouble();
                type = dt.Rows[i]["type"].ObjToString();

                G1.update_db_table("casket_locations", "record", record, new string[] { "casketcode", casketcode, "casketdesc", casketdesc, "price", price.ToString(), "casket", casket.ToString(), "casketcost", casketcost.ToString(), "futureCasketCost", futureCasketCost.ToString(), "pastCasketCost", pastCasketCost.ToString(), "casketGroup", workCasketGroup, "location", workLocation, "markup", markup.ToString(), "futuremarkup", futureMarkup.ToString(), "pastmarkup", pastMarkup.ToString(), "type", type, "order", i.ToString() });
            }

            G1.NumberDataTable(dt);
            CalculateCosts(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void cmbPackage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            if (!CheckForSaving())
                return;
            LoadData();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit1_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string service = dr["service"].ObjToString();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain.FocusedRowHandle;
            string select = dr["SameAsMaster"].ObjToString();
            string doit = "1";
            if (select == "1")
                doit = "0";
            dr["SameAsMaster"] = doit;
            DataTable dt = (DataTable)dgv.DataSource;
            if (doit == "1")
            {
                string cmd = "Select * from `services` where `record` = '" + record + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    dr["price"] = dx.Rows[0]["price"].ObjToDouble();
                    dr["data"] = dx.Rows[0]["data"].ObjToDouble();
                }
            }
            else
            {
                DataRow[] dRow = workDt.Select("service='" + service + "'");
                if (dRow.Length > 0)
                {
                    dr["price"] = dRow[0]["price"].ObjToDouble();
                    dr["data"] = dRow[0]["cost"].ObjToDouble();
                }
            }
            modified = true;
            if ( !Selecting || workingLocation )
                btnSave.Show();
            ReCalcTotal();
            gridMain.RefreshData();
            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void importCasketMasterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
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
            DialogResult result = MessageBox.Show("***Question***\nWould you like to first CLEAR the Casket Database before Import?", "Clear Casket Database Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if ( result == DialogResult.Yes )
            {
                string cmd = "DELETE FROM `casket_master` where `record` > '0';";
                G1.get_db_data(cmd);
            }

            string casketCode = "";
            string casketDesc = "";
            string price = "";
            string casketPrice = "";
            string record = "";
            string chr = "";
            bool vault = false;
            bool urn = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                casketCode = dt.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(casketCode))
                    continue;

                vault = false;
                chr = casketCode.Substring(0, 1).ToUpper();
                if (chr == "V" )
                    vault = true;
                if (casketCode.IndexOf("URN") >= 0)
                {
                    urn = true;
                    vault = true;
                }
                if (!vault)
                    casketDesc = dt.Rows[i][3].ObjToString();
                else
                    casketDesc = dt.Rows[i][2].ObjToString();
                if (String.IsNullOrWhiteSpace(casketDesc))
                    continue;

                if (String.IsNullOrWhiteSpace(casketDesc))
                    continue;
                price = dt.Rows[i][4].ObjToString();
                if (String.IsNullOrWhiteSpace(price))
                    continue;
                price = price.Replace("$", "");
                price = price.Replace(",", "");
                if (!G1.validate_numeric(price))
                    continue;
                casketDesc = G1.protect_data(casketDesc);
                casketPrice = "";
                if ( vault )
                {
                    casketPrice = dt.Rows[i][6].ObjToString();
                    if ( !String.IsNullOrWhiteSpace ( casketPrice ))
                    {
                        if ( urn)
                        {
                            string[] Lines = casketPrice.Split(',');
                            if (Lines.Length > 1)
                                casketPrice = Lines[1];
                        }
                        casketPrice = casketPrice.Replace("$", "");
                        casketPrice = casketPrice.Replace(",", "");
                        if (!G1.validate_numeric(casketPrice))
                            casketPrice = "";
                    }
                }
                record = G1.create_record("casket_master", "casketdesc", "-1");
                if (G1.BadRecord("casket_master", record))
                    continue;
                G1.update_db_table("casket_master", "record", record, new string[] { "casketdesc", casketDesc, "casketcode", casketCode, "casketcost", price, "casketprice", casketPrice });
            }
        }
        /***********************************************************************************************/
        private void SaveCasketMaster( DataTable dt )
        {
            string casketCode = "";
            string casketDesc = "";
            string price = "";
            string futurePrice = "";
            string pastPrice = "";
            string casketPrice = "";
            string markup = "";
            string record = "";
            string mod = "";
            string chr = "";
            bool gotMarkup = false;
            if (G1.get_column_number(dt, "markup") >= 0)
                gotMarkup = true;
            bool gotMaster = false;
            if (G1.get_column_number(dt, "") >= 0)
                gotMaster = true;
            bool vault = false;
            bool urn = false;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod == "D")
                    {
                        record = dt.Rows[i]["record"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(record))
                            G1.delete_db_table("casket_master", "record", record);
                        continue;
                    }
                    casketCode = dt.Rows[i]["casketcode"].ObjToString();
                    if (String.IsNullOrWhiteSpace(casketCode))
                        continue;

                    vault = false;
                    chr = casketCode.Substring(0, 1).ToUpper();
                    if (chr == "V")
                        vault = true;
                    if (casketCode.IndexOf("URN") >= 0)
                    {
                        urn = true;
                        vault = true;
                    }
                    casketDesc = dt.Rows[i]["casketdesc"].ObjToString();
                    if (String.IsNullOrWhiteSpace(casketDesc))
                        continue;
                    if (casketDesc.ToUpper() == "OLYMPIAN")
                    {

                    }

                    futurePrice = dt.Rows[i]["futureCasketCost"].ObjToString();
                    futurePrice = futurePrice.Replace("$", "");
                    futurePrice = futurePrice.Replace(",", "");

                    pastPrice = dt.Rows[i]["pastCasketCost"].ObjToString();
                    pastPrice = pastPrice.Replace("$", "");
                    pastPrice = pastPrice.Replace(",", "");

                    price = dt.Rows[i]["casketcost"].ObjToString();
                    if (String.IsNullOrWhiteSpace(price))
                        continue;
                    price = price.Replace("$", "");
                    price = price.Replace(",", "");
                    if (!G1.validate_numeric(price))
                        continue;
                    casketDesc = G1.protect_data(casketDesc);
                    casketPrice = "";
                    if (vault)
                    {
                        casketPrice = dt.Rows[i]["casketprice"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(casketPrice))
                        {
                            if (urn)
                            {
                                string[] Lines = casketPrice.Split(',');
                                if (Lines.Length > 1)
                                    casketPrice = Lines[1];
                            }
                            casketPrice = casketPrice.Replace("$", "");
                            casketPrice = casketPrice.Replace(",", "");
                            if (!G1.validate_numeric(casketPrice))
                                casketPrice = "";
                        }
                    }
                    //                record = dt.Rows[i]["!masterRecord"].ObjToString();
                    record = dt.Rows[i]["record"].ObjToString();
                    if (String.IsNullOrWhiteSpace(record))
                        record = G1.create_record("casket_master", "casketdesc", "-1");
                    if (G1.BadRecord("casket_master", record))
                        continue;
                    G1.update_db_table("casket_master", "record", record, new string[] { "casketdesc", casketDesc, "casketcode", casketCode, "casketcost", price, "casketprice", casketPrice, "futureCasketCost", futurePrice, "pastCasketCost", pastPrice });
                    if (gotMarkup)
                    {
                        record = dt.Rows[i]["record"].ObjToString();
                        markup = dt.Rows[i]["markup"].ObjToString();
                        markup = markup.Replace("$", "");
                        markup = markup.Replace(",", "");
                        G1.update_db_table("casket_packages", "record", record, new string[] { "markup", markup });
                    }
                }
                catch ( Exception ex)
                {
                }
            }

        }
        /***********************************************************************************************/
        private void importCasketGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import();
            importForm.SelectDone += ImportForm_SelectDone1;
            importForm.Show();
        }
        /***********************************************************************************************/
        private void ImportForm_SelectDone1(DataTable dt)
        {
            if (dt == null)
                return;
            if (dt.Rows.Count <= 0)
                return;
            string casketCode = "";
            string casketDesc = "";
            string price = "";
            string record = "";
            string masterRecord = "";
            string cmd = "";
            int goodCount = 0;
            int badCount = 0;
            DataTable dx = null;
            double cost = 0D;
            double markup = 0D;
            string str = "";
            //string groupName = cmbGroups.Text.Trim();
            //if (String.IsNullOrWhiteSpace(groupName))
            //    return;
            //if ( groupName.Trim().ToUpper() == "MASTER")
            //{
            //    MessageBox.Show("***ERROR*** Cannot import Groups as Master");
            //    return;
            //}
            //string gplGroup = cmbGPLGroup.Text.Trim();
            //if (String.IsNullOrWhiteSpace(gplGroup))
            //{
            //    MessageBox.Show("***ERROR*** You MUST assign a GPL Group to Assign to Casket Group!");
            //    return;
            //}
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    casketCode = dt.Rows[i][1].ObjToString();
            //    if (String.IsNullOrWhiteSpace(casketCode))
            //        continue;
            //    casketDesc = dt.Rows[i][3].ObjToString();
            //    if (String.IsNullOrWhiteSpace(casketDesc))
            //        continue;
            //    price = dt.Rows[i][4].ObjToString();
            //    if (String.IsNullOrWhiteSpace(price))
            //        continue;
            //    price = price.Replace("$", "");
            //    price = price.Replace(",", "");
            //    if (!G1.validate_numeric(price))
            //        continue;
            //    casketDesc = G1.protect_data(casketDesc);
            //    cmd = "Select * from `casket_master` where `casketdesc` = '" + casketDesc + "';";
            //    dx = G1.get_db_data(cmd);
            //    if (dx.Rows.Count > 0)
            //    {
            //        goodCount++;
            //        cost = dx.Rows[0]["casketcost"].ObjToDouble();
            //        masterRecord = dx.Rows[0]["record"].ObjToString();
            //        str = dt.Rows[i][7].ObjToString();
            //        str = str.Replace("%", "");
            //        markup = str.ObjToDouble() / 100D;
            //        record = G1.create_record("casket_packages", "groupname", "-1");
            //        if (G1.BadRecord("casket_packages", record))
            //            continue;
            //        G1.update_db_table("casket_packages", "record", record, new string[] { "!masterRecord", masterRecord, "markup", markup.ToString(), "GPL_Group", gplGroup, "groupname", groupName });
            //    }
            //    else
            //        badCount++;
            //}
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            SaveServices(dt);
            btnSave.Hide();
            modified = false;
        }
        /***********************************************************************************************/
        private double GetGPGBasicPackage ()
        {
            double basicPackage = 0D;
            //string gplGroup = cmbGPLGroup.Text.Trim();
            //if (String.IsNullOrWhiteSpace(gplGroup))
            //    return basicPackage;
            //string basics = "BASIC SERVICES OF FUNERAL DIRECTOR AND STAFF";
            //string cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + gplGroup + "' and `service` = '" + basics + "';";
            //DataTable dx = G1.get_db_data(cmd);
            //if (dx.Rows.Count > 0)
            //    basicPackage = dx.Rows[0]["price"].ObjToDouble();
            return basicPackage;
        }
        /***********************************************************************************************/
        private void cmbGPLGroup_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
            double basicPackage = GetGPGBasicPackage();

            LoadGPLLocations();

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            double casketPrice = 0D;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                casketPrice = dt.Rows[i]["casket"].ObjToDouble();
                dt.Rows[i]["package"] = casketPrice + basicPackage;
            }
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void picMoveFuture_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            double basicPackage = GetGPGBasicPackage();

            if ( G1.get_column_number ( dt, "package") < 0 )
                dt.Columns.Add("package", Type.GetType("System.Double"));


            double futurePrice = 0D;
            double currentPrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                currentPrice = dt.Rows[i]["casketCost"].ObjToDouble();
                dt.Rows[i]["pastCasketCost"] = currentPrice;
                futurePrice = dt.Rows[i]["futureCasketCost"].ObjToDouble();
                if (futurePrice <= 0D)
                    futurePrice = currentPrice;
                dt.Rows[i]["casketCost"] = futurePrice;
                dt.Rows[i]["package"] = futurePrice + basicPackage;
                dt.Rows[i]["casket"] = futurePrice;
            }

            dgv.DataSource = dt;
            dgv.Refresh();
            modified = true;
            btnSave.Show();
            picMoveFuture.Hide();
            picMovePast.Show();
        }
        /***********************************************************************************************/
        private void picMovePast_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            double basicPackage = GetGPGBasicPackage();

            double pastPrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                pastPrice = dt.Rows[i]["pastCasketCost"].ObjToDouble();
                dt.Rows[i]["casketCost"] = pastPrice;
                dt.Rows[i]["casket"] = pastPrice;
                dt.Rows[i]["package"] = pastPrice + basicPackage;
            }

            dgv.DataSource = dt;
            dgv.Refresh();

            modified = true;
            btnSave.Show();
            picMoveFuture.Show();
            picMovePast.Hide();
        }
        /***********************************************************************************************/
        private void LoadGPLLocations()
        {
            //string casketGroup = cmbGroups.Text.Trim();
            //cmbLocations.Items.Clear();
            //cmbLocations.Text = "";
            //if (casketGroup.ToUpper() == "MASTER")
            //    return;

            //lblLocations.Text = "Locations (0)";

            //string cmd = "Select * from `casket_locations` where `casketGroup` = '" + casketGroup + "' GROUP BY `location`;";
            //DataTable dt = G1.get_db_data(cmd);
            //if (dt.Rows.Count <= 0)
            //    return;
            //bool saveLoading = loading;
            //loading = true;
            //string location = "";
            //for (int i = 0; i < dt.Rows.Count; i++)
            //{
            //    location = dt.Rows[i]["location"].ObjToString();
            //    cmbLocations.Items.Add(location);
            //}
            //lblLocations.Text = "Locations (" + dt.Rows.Count.ToString() + ")";

            //loading = saveLoading;
        }
        /***********************************************************************************************/
        private void PullLocationGPL(DataTable dx)
        {
            //string casketGroup = cmbGroups.Text.Trim();
            //if (String.IsNullOrWhiteSpace(casketGroup))
            //    return;
            //if (casketGroup.Trim().ToUpper() == "MASTER")
            //    return;

            //string location = cmbLocations.Text.Trim();
            //if (String.IsNullOrWhiteSpace(location))
            //    return;

            //string cmd = "Select * from `casket_locations` where `casketGroup` = '" + casketGroup + "' and `location` = '" + location + "';";
            //DataTable dt = G1.get_db_data(cmd);
            //if (dt.Rows.Count <= 0)
            //    return;
            //if (dx != null)
            //{
            //    DataRow[] dRows = null;
            //    string service = "";
            //    bool gotData = false;
            //    if (G1.get_column_number(dx, "data") >= 0 )
            //        gotData = true;
            //    for (int i = 0; i < dt.Rows.Count; i++)
            //    {
            //        service = dt.Rows[i]["service"].ObjToString();
            //        dRows = dx.Select("casketDesc='" + service + "'");
            //        if (dRows.Length > 0)
            //        {
            //            dRows[0]["price"] = dt.Rows[i]["price"].ObjToString();
            //            dRows[0]["futurePrice"] = dt.Rows[i]["futurePrice"].ObjToString();
            //            dRows[0]["pastPrice"] = dt.Rows[i]["pastPrice"].ObjToString();
            //            if ( gotData )
            //                dRows[0]["data"] = "CUSTOM";
            //        }
            //    }
            //}
        }
        /***********************************************************************************************/
        private void btnAddLocation_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
    }
}