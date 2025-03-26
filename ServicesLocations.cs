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

/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class ServicesLocations : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private bool alreadyLoaded = false;
        private string savePackage = "";
        private bool modified = false;
        private bool Selecting = false;
        private DataTable workDt = null;
        private string workContract = "";
        private bool loading = false;
        private string loadedPackage = "";
        private string loadededLocation = "";
        private string workFrom = "";

        private string workGroup = "";
        private string workPackage = "";
        private string workLocation = "";
        /***********************************************************************************************/
        public ServicesLocations( bool selecting, DataTable dt = null, string contract = "" )
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            workContract = contract;
            if (!selecting && dt == null && String.IsNullOrWhiteSpace(contract))
            {
            }
            else
                contextMenuStrip1.Dispose();
        }
        /***********************************************************************************************/
        public ServicesLocations(string from, bool selecting, DataTable dt = null, string contract = "")
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            workContract = contract;
            workFrom = from;
        }
        /***********************************************************************************************/
        public ServicesLocations(string fromGroup, string fromLocation, bool selecting, DataTable dt = null )
        {
            InitializeComponent();
            Selecting = selecting;
            workDt = dt;
            workContract = "";
            workGroup = fromGroup;
            workLocation = fromLocation;
            this.Text = "Custom Location Prices for GPL " + fromGroup + " and Location " + fromLocation;
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
        /***********************************************************************************************/
        private void ServicesLocations_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            loading = true;
            if ( !Selecting )
            {
                LoadGroupCombo();
                LoadPackagesCombo();
                gridMain.OptionsBehavior.ReadOnly = false;
                gridMain.Columns["total"].Visible = false;
                gridMain.Columns["data"].Visible = false;
            }
            else
            {
                gridMain.OptionsView.ShowFooter = true;
                gridMain.Columns["total"].Visible = false;
                gridMain.Columns["data"].Visible = false;
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
            this.BringToFront();
        }
        /***********************************************************************************************/
        private void ReSelectServices ()
        {
            if (workDt == null)
                return;
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");

            gridMain.OptionsBehavior.ReadOnly = false;

            DataTable ddx = null;

            if (workGroup.ToUpper() != "MASTER")
            {
                string cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + workGroup + "';";
                ddx = G1.get_db_data(cmd);
            }
            string availableService = "";
            string service = "";
            string select = "";
            double price = 0D;
            double price1 = 0D;
            double samePrice = 0D;
            double cost = 0D;
            string data = "";
            string type = "";
            string same = "";
            bool found = false;
            bool added = false;
            bool serviceRecord = false;
            string record = "";
            DataRow[] dR = null;
            if (G1.get_column_number(workDt, "!serviceRecord") < 0)
            {
                serviceRecord = true;
            }
            if ( G1.get_column_number ( workDt, "cost") < 0 )
                workDt.Columns.Add("cost", Type.GetType("System.Double"));
            if (G1.get_column_number(workDt, "SameAsMaster") < 0)
                workDt.Columns.Add("SameAsMaster");
            if (G1.get_column_number(workDt, "data") < 0)
                workDt.Columns.Add("data", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "data") < 0)
                dt.Columns.Add("data", Type.GetType("System.Double"));
            if (G1.get_column_number(dt, "type") < 0)
                dt.Columns.Add("type");
            if (G1.get_column_number(workDt, "select") < 0)
            {
                workDt.Columns.Add("select");
                added = true;
            }

            for ( int i=0; i<workDt.Rows.Count; i++)
            {
                service = workDt.Rows[i]["service"].ObjToString();
                if ( service == "TRANSFER OF REMAINS TO THE FUNERAL HOME")
                {

                }
                price = workDt.Rows[i]["price"].ObjToDouble();
                data = workDt.Rows[i]["data"].ObjToString();
                price1 = workDt.Rows[i]["price1"].ObjToDouble();
                cost = workDt.Rows[i]["cost"].ObjToDouble();
                type = workDt.Rows[i]["type"].ObjToString();
                same = workDt.Rows[i]["SameAsMaster"].ObjToString();
                select = workDt.Rows[i]["select"].ObjToString();
                if (added)
                    select = "1";
                if ( same == "1" && ddx != null )
                {
                    dR = ddx.Select("service='" + service + "'");
                    if (dR.Length > 0)
                    {
                        samePrice = dR[0]["price"].ObjToDouble();
                        if (samePrice != 0D)
                        {
                            if (price == 0D)
                                price = samePrice;
                            if (price1 == 0D)
                                price1 = samePrice;
                        }
                    }
                }
                found = false;
                for ( int j=0; j<dt.Rows.Count; j++)
                {
                    availableService = dt.Rows[j]["service"].ObjToString();
                    if (availableService == service)
                    {
                        dt.Rows[j]["select"] = select;
                        dt.Rows[j]["price"] = price;
                        dt.Rows[j]["data"] = cost;
                        if ( same == "1")
                        {
                            dt.Rows[j]["price"] = price1;
                            if ( !String.IsNullOrWhiteSpace ( data ))
                            {
                                if (G1.validate_numeric(data))
                                    dt.Rows[j]["data"] = data;
                            }
//                            dt.Rows[j]["data"] = data;
                            dt.Rows[j]["SameAsMaster"] = "1";
                        }
                        if (!String.IsNullOrWhiteSpace(type))
                            dt.Rows[j]["type"] = type;
                        found = true;
                        break;
                    }
                }
                if ( !found)
                {
                    DataRow dRow = dt.NewRow();
                    dRow["service"] = service;
                    if ( !String.IsNullOrWhiteSpace ( data))
                    {
                        if (G1.validate_numeric(data))
                            dRow["data"] = data;
                    }
//                    dRow["data"] = data;
                    dRow["type"] = type;
                    dRow["price"] = price;
                    dRow["select"] = select;
                    dt.Rows.Add(dRow);
                }
            }
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            ReCalcTotal();
        }
        /***********************************************************************************************/
        private void LoadPackage()
        {
            string serviceRecord = "";
            string group = GetGroup();
            string package = "";
            if (String.IsNullOrWhiteSpace(package))
                package = "Master";
            if ( String.IsNullOrWhiteSpace ( group) || String.IsNullOrWhiteSpace ( package ))
            {
                MessageBox.Show("***ERROR*** Empty group or package!");
                return;
            }
            gridMain.OptionsView.ShowFooter = true;
            if ( group.Trim().ToUpper() == "MASTER")
                gridMain.OptionsView.ShowFooter = false;
            if ( package.Trim().ToUpper() == "MASTER")
                gridMain.OptionsView.ShowFooter = false;

            if ( group != "Master" && package == "Master")
            {
                cmbLocation_SelectedIndexChanged(null, null);
                this.Cursor = Cursors.Default;
                return;
            }

            loadededLocation = group;
            loadedPackage = package;
            string list = "";
            string cmd = "Select * from `packages` where `groupname` = '" + group + "' and `PackageName` = '" + package + "';";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                serviceRecord = dx.Rows[i]["!serviceRecord"].ObjToString();
                if (String.IsNullOrWhiteSpace(serviceRecord))
                    continue;
                list += "'" + serviceRecord + "',";
            }
            list = list.TrimEnd(',');
            if (!String.IsNullOrWhiteSpace(list))
            {
                cmd = "Select * from `packages` p LEFT JOIN `funeral_master` s ON p.`!serviceRecord` = s.`record` where s.`record` IN (" + list + ") ";
                //                cmd = "Select * from `packages` p RIGHT JOIN `services` s ON p.`!serviceRecord` = s.`record` where s.`record` IN (" + list + ") ";
                cmd += " and `groupname` = '" + group + "' and `PackageName` = '" + package + "' ";
                cmd += ";";
                //                cmd = "Select * from `services` where `record` IN (" + list + ");";
                //if (!String.IsNullOrWhiteSpace(group) && !String.IsNullOrWhiteSpace(package))
                //{
                //    cmd = "Select * from `packages` p LEFT JOIN `funeral_gplgroups` s ON p.`!serviceRecord` = s.`record` where s.`record` IN (" + list + ") ";
                //    cmd += " and s.`groupname` = '" + group + "'  and `PackageName` = '" + package + "' ";
                //    cmd += ";";
                //}
            }
            else
            {
                cmd = "Select * from `packages` p JOIN `funeral_master` s ON p.`!serviceRecord` = s.`record` where `service` = 'xyzzyxxxx';";
//                cmd = "Select * from `packages` p JOIN `services` s ON p.`!serviceRecord` = s.`record` where `service` = 'xyzzyxxxx';";
            }
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("select");
//            dt.Columns.Add("SameAsMaster");
            dt.Columns.Add("total", Type.GetType("System.Double"));
            SetupSelection(dt);
            if (!String.IsNullOrWhiteSpace(group) && !String.IsNullOrWhiteSpace(package))
            {
                cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + group + "';";
                DataTable ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count > 0)
                    SetupSameAsMaster(dt, ddx);
                else
                    SetupSameAsMaster(dt);
            }
            else
                SetupSameAsMaster(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void LoadData()
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `gpl_locations` where `gpl` = '" + workGroup + "' AND `location` = '" + workLocation + "';";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("mod");
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void SetupSelection(DataTable dt )
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit3;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (G1.get_column_number(dt, "select") < 0)
                dt.Columns.Add("select");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["select"] = "0";
        }
        /***********************************************************************************************/
        private void SetupSameAsMaster(DataTable dt, DataTable ddx = null)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";
            if (dt != null )
            {
                if (ddx == null)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["SameAsMaster"] = "0";
                }
                else
                {
                    DataRow[] dR = null;
                    string select = "";
                    string service = "";
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        select = dt.Rows[i]["SameAsMaster"].ObjToString();
                        if (select == "1")
                        {
                            dt.Rows[i]["SameAsMaster"] = "1";
                            service = dt.Rows[i]["service"].ObjToString();
                            dR = ddx.Select("service='" + service + "'");
                            if ( dR.Length >0)
                            {
                                dt.Rows[i]["price"] = dR[0]["price"];
                            }
                        }
                        else
                            dt.Rows[i]["SameAsMaster"] = "0";
                    }
                }
            }
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
            if (loading)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain.FocusedRowHandle;
            string select = dr["select"].ObjToString();
            //string doit = "1";
            //if (select == "1")
            //    doit = "0";
            string doit = "0";
            if (select == "1")
                doit = "1";
            loading = true;
            dr["select"] = doit;
            loading = false;
            modified = true;
            ReCalcTotal();
            gridMain.RefreshData();
            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string location);
        public event d_void_eventdone_string SelectDone;
        protected void OnSelectDone()
        {
            SelectDone?.Invoke(workLocation);
        }
        /***********************************************************************************************/
        private void ReCalcTotal()
        {
            string select = "";
            double price = 0D;
            double total = 0D;
            string data = "";
            DataTable dt = (DataTable)dgv.DataSource;

            if (G1.get_column_number(dt, "total") < 0)
                dt.Columns.Add("total", Type.GetType("System.Double"));


            for ( int i=0; i<dt.Rows.Count; i++)
            {
                try
                {
                    if (G1.get_column_number(dt, "select") >= 0)
                        select = dt.Rows[i]["select"].ObjToString();
                    else if (G1.get_column_number(dt, "SameAsMaster") >= 0)
                        select = dt.Rows[i]["SameAsMaster"].ObjToString();
                    else
                        continue;
                    if (select == "1")
                    {
                        price = dt.Rows[i]["price"].ObjToDouble();
                        data = "";
                        if (G1.get_column_number(dt, "data") >= 0)
                            data = dt.Rows[i]["data"].ObjToString();
                        if (G1.validate_numeric(data))
                        {
                            if (!String.IsNullOrWhiteSpace(data))
                            {
                                if (data.ObjToDouble() > 0)
                                    price = data.ObjToDouble();
                            }
                            total = total + price;
                            dt.Rows[i]["total"] = total;
                        }
                        else
                            total += price;
                    }
                    else
                        dt.Rows[i]["total"] = 0D;
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        private bool CheckForSaving()
        {
            if (!modified)
                return true;
            DialogResult result = MessageBox.Show("***Question***\nMerchandise has been modified!\nWould you like to save your changes?", "Select Merchandise Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                savePackage = "";
                return false;
            }
            modified = false;
            if (result == DialogResult.No)
                return true;
            DataTable dt = (DataTable)dgv.DataSource;
            if (String.IsNullOrWhiteSpace(loadededLocation) || String.IsNullOrWhiteSpace(loadedPackage))
                return true;
            //if (loadededLocation == "MASTER" && loadedPackage == "MASTER")
            //    SaveServices();
            //else
            //    SaveServices(dt);
            return true;
        }
        /***********************************************************************************************/
        private void Services_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nServices have been modified!\nWould you like to save your changes?", "Select Services Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                G1.copy_dt_row(dt, i, dx, dx.Rows.Count);
            }
//            OnSelectDone(dx);
            OnSelectDone();
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
            if ( Selecting )
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
            DataTable dt = (DataTable)dgv.DataSource;
            Services serviceForm = new Services ( true, true, workGroup );
            serviceForm.SelectDone += ServiceForm_SelectDone1;
            serviceForm.Show();
        }
        /***********************************************************************************************/
        private void ServiceForm_SelectDone1(DataTable dt, string what )
        {
            DataTable dx = (DataTable)dgv.DataSource;
            string service = "";
            string price = "";
            string futurePrice = "";
            string pastPrice = "";
            string type = "";
            DataRow[] dRows = null;
            DataRow dR = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                service = dt.Rows[i]["service"].ObjToString();
                if (String.IsNullOrWhiteSpace(service))
                    continue;
                dRows = dx.Select("service='" + service + "'");
                if (dRows.Length > 0)
                    continue;
                dR = dx.NewRow();
                dR["service"] = service;
                price = dt.Rows[i]["price"].ObjToString();
                dR["price"] = price;
                futurePrice = dt.Rows[i]["futurePrice"].ObjToString();
                dR["futurePrice"] = futurePrice;
                pastPrice = dt.Rows[i]["pastPrice"].ObjToString();
                dR["pastPrice"] = pastPrice;
                type = dt.Rows[i]["type"].ObjToString();
                dR["type"] = type;
                dx.Rows.Add(dR);
            }
            dgv.DataSource = dx;
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void pictureDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string service = dr["service"].ObjToString();
            DialogResult result = MessageBox.Show("***Question*** Are you sure you want to delete this service (" + service + ") ?", "Delete Service Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;

            dr["mod"] = "D";
            modified = true;
            btnSave.Show();
            if (1 == 1)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
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
                    if ( dRow != null)
                        dRow["mod"] = "D";
                    dt.Rows[dtRow]["mod"] = "D";
                    modified = true;
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
            EditHelp helpForm = new EditHelp( "customers" );
            helpForm.Show();
        }
        /***********************************************************************************************/
        private void SaveServicesLocations( DataTable dt )
        {
            string record = "";
            string service = "";
            string type = "";
            string price = "";
            string futurePrice = "";
            string pastPrice = "";
            string mod = "";

            this.Cursor = Cursors.WaitCursor;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                mod = dt.Rows[i]["mod"].ObjToString();
                if (mod.ToUpper() == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("gpl_locations", "record", record);
                    continue;
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = G1.create_record("gpl_locations", "service", "-1");
                if (G1.BadRecord("gpl_locations", record))
                    continue;
                service = dt.Rows[i]["service"].ObjToString();
                price = dt.Rows[i]["price"].ObjToString();
                futurePrice = dt.Rows[i]["futurePrice"].ObjToString();
                pastPrice = dt.Rows[i]["pastPrice"].ObjToString();

                type = dt.Rows[i]["type"].ObjToString();
                G1.update_db_table("gpl_locations", "record", record, new string[] { "gpl", workGroup, "location", workLocation, "service", service, "price", price, "type", type, "futurePrice", futurePrice, "pastPrice", pastPrice, "order", i.ToString() });
            }
            btnSave.Hide();
            //modified = false;
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            modified = true;
            if ( !Selecting)
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
            if ( Selecting )
            {
                double price = dt.Rows[row]["price"].ObjToDouble();
                if ( price <= 0 )
                {
                    //e.Visible = false;
                    //e.Handled = true;
                }
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
                    if ( G1.validate_numeric ( data ))
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
        private void LoadGroupCombo()
        {
            string cmd = "Select * from `funeral_groups` ORDER BY `order`,`record`;";
            DataTable dt = G1.get_db_data(cmd);
            string groupname = "";
            string name = "";
            string locationCode = "";
            string str = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                groupname = dt.Rows[i]["shortname"].ObjToString();
            }
        }
        /***********************************************************************************************/
        private void LoadPackagesCombo()
        {
            string group = GetGroup();
            if (String.IsNullOrWhiteSpace(group))
                return;
            string cmd = "Select * from `packages` where `groupname` = '" + group + "' GROUP BY `PackageName`;";
            DataTable dt = G1.get_db_data(cmd);
            string firstPackage = "";
            string package = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                package = dt.Rows[i]["PackageName"].ObjToString();
                if (String.IsNullOrWhiteSpace(package))
                    continue;
                if (String.IsNullOrWhiteSpace(firstPackage))
                    firstPackage = package;
            }
        }
        /***********************************************************************************************/
        private string GetGroup ()
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
            btnSave.Hide();
            string group = GetGroup();
            string cmd = "Select * from `packages` where `groupname` = '" + group + "' GROUP BY `PackageName`;";
            DataTable dt = G1.get_db_data(cmd);
            string packageName = "";
            string firstPackage = "";
            if (group.ToUpper() == "MASTER")
            {
                gridMain.OptionsView.ShowFooter = false;
                firstPackage = "Master";
                
            }
            firstPackage = "Master";
            loading = true;
            loading = false;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                packageName = dt.Rows[i]["PackageName"].ObjToString();
                if (String.IsNullOrWhiteSpace(firstPackage))
                    firstPackage = packageName;
            }
            if (!String.IsNullOrWhiteSpace(firstPackage) && firstPackage != "Master")
            {
                LoadData();
//                LoadPackage(cmbPackage.Text);
                //cmd = "Select * from `packages` where `PackageName` = '" + firstPackage + "';";
                //dt = G1.get_db_data(cmd);
                //dgv.DataSource = dt;
            }
            else
            {
                cmd = "Select * from `packages` where `PackageName` = 'xyzzyxxx';";
                dt = G1.get_db_data(cmd);
                if (G1.get_column_number(dt, "futurePrice2") >= 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["futurePrice"] = dt.Rows[i]["futurePrice2"].ObjToDouble();
                }
                if (G1.get_column_number(dt, "pastPrice2") >= 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                        dt.Rows[i]["pastPrice"] = dt.Rows[i]["pastPrice2"].ObjToDouble();
                }
                dt.Columns.Add("num");
                //dt.Columns.Add("mod");
                ////            dt.Columns.Add("agreement");
                //dt.Columns.Add("select");
                if ( G1.get_column_number ( dt, "SameAsMaster") < 0 )
                    dt.Columns.Add("SameAsMaster");
                if (G1.get_column_number(dt, "mod") < 0)
                    dt.Columns.Add("mod");
                if (G1.get_column_number(dt, "data") < 0)
                    dt.Columns.Add("data");
                string type = "";
                string type1 = "";
                if (G1.get_column_number(dt, "type1") >= 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        type = dt.Rows[i]["type"].ObjToString();
                        type1 = dt.Rows[i]["type1"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(type1))
                            dt.Rows[i]["type"] = type1;
                    }
                }


                //dt.Columns.Add("total", Type.GetType("System.Double"));
                if ( Selecting )
                {
                    SetupSelection(dt);
                }
                SetupSameAsMaster(null);
                if ( group.Trim().ToUpper() == "MASTER" )
                {
                    gridMain.Columns["SameAsMaster"].Visible = false;
                    gridMain.Columns["data"].Visible = false;
                }
                else
                {
                    gridMain.Columns["SameAsMaster"].Visible = true;
                }

                G1.NumberDataTable(dt);

                dgv.DataSource = dt;
            }
            LoadGPLLocations();
        }
        /***********************************************************************************************/
        private void LoadGPLLocations ()
        {
            string gplGroup = "";
            if (gplGroup.ToUpper() == "MASTER")
                return;

            string cmd = "Select * from `gpl_locations` where `gpl` = '" + gplGroup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            bool saveLoading = loading;
            loading = true;
            string location = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                location = dt.Rows[i]["location"].ObjToString();
            }
            loading = saveLoading;
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
        private void LoadPackage ( string packName )
        {
            loadedPackage = packName;
            string group = GetGroup();
            loadededLocation = group;
            string cmd = "Select * from `packages` p RIGHT JOIN `services` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
            if ( packName.Trim().ToUpper() == "MASTER")
                cmd = "Select * from `packages` p JOIN `funeral_gplgroups` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
            else
                cmd = "Select * from `packages` p JOIN `funeral_master` s ON p.`!serviceRecord` = s.`record` where p.`groupname` = '" + group + "' and p.`PackageName` = '" + packName + "';";
            //cmd = "Select * from `funeral_gplgroups` p LEFT JOIN `services` s ON p.`!masterRecord` = s.`record` where p.`groupname` = '" + group + "';";
            //cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + group + "'";
            DataTable dt = G1.get_db_data(cmd);
            string record = "";
            if ( dt.Rows.Count > 0 )
                record = dt.Rows[0]["record"].ObjToString();
            //            Services serviceForm = new Services("Packages", true, dt, "");
            Services serviceForm = new Services(group, "", true, dt, "");
            serviceForm.SelectDone += ServiceForm_SelectDone;
            serviceForm.Show();
        }
        /***********************************************************************************************/
        private void ServiceForm_SelectDone(DataTable dt, string what )
        {
            //SaveServices(dt);
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            if ( !alreadyLoaded )
                dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void SaveServices(DataTable dt)
        {
            if (1 == 1)
                return;
            string service = "";
            string serviceRecord = "";
            string SameAsMaster = "";
            string data = "";
            double price = 0D;
            double futurePrice = 0D;
            double pastPrice = 0D;
            double cost = 0D;
            string type = "";
            string record = "";
            int recordCol = G1.get_column_number(dt, "record");
            if (G1.get_column_number(dt, "!serviceRecord") >= 0)
                recordCol = G1.get_column_number(dt, "!serviceRecord");
            if (String.IsNullOrWhiteSpace(loadededLocation))
            {
                MessageBox.Show("***ERROR*** Empty Location");
                return;
            }
            if (String.IsNullOrWhiteSpace(loadedPackage))
            {
                MessageBox.Show("***ERROR*** Empty Package");
                return;
            }
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Delete from `packages` where `groupname` = '" + loadededLocation + "' and `PackageName` = '" + loadedPackage + "';";
            G1.get_db_data(cmd);

            DataTable gDt = null;
            DataTable dx = null;
            string masterRecord = "";
            string mod = "";
            int modColumn = G1.get_column_number(dt, "mod");
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    serviceRecord = dt.Rows[i][recordCol].ObjToString();
                    if (modColumn >= 0)
                    {
                        mod = dt.Rows[i]["mod"].ObjToString();
                        if (mod == "D")
                            continue;
                    }
                    service = dt.Rows[i]["service"].ObjToString();
                    //data = dt.Rows[i]["data"].ObjToString();
                    type = dt.Rows[i]["type"].ObjToString();
                    if (type.Trim().ToUpper() == "MERCHANDISE")
                    {

                    }
                    SameAsMaster = dt.Rows[i]["SameAsMaster"].ObjToString();
                    price = dt.Rows[i]["price"].ObjToDouble();
                    futurePrice = dt.Rows[i]["futurePrice"].ObjToDouble();
                    pastPrice = dt.Rows[i]["pastPrice"].ObjToDouble();
                    cost = 0D;
                    if ( G1.get_column_number ( dt, "data") >= 0)
                        cost = dt.Rows[i]["data"].ObjToDouble();
                    record = G1.create_record("packages", "groupname", "-1");
                    if (G1.BadRecord("packages", record))
                        continue;
                    G1.update_db_table("packages", "record", record, new string[] { "groupname", loadededLocation, "PackageName", loadedPackage, "!serviceRecord", serviceRecord, "SameAsMaster", SameAsMaster, "price", price.ToString(), "cost", cost.ToString(), "futurePrice", futurePrice.ToString(), "pastPrice", pastPrice.ToString() });

                    if (loadedPackage.Trim().ToUpper() == "MASTER")
                    {

                        cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + loadededLocation + "' and `service` = '" + service + "';";
                        gDt = G1.get_db_data(cmd);
                        if (gDt.Rows.Count > 0)
                            record = gDt.Rows[0]["record"].ObjToString();
                        else
                            record = G1.create_record("funeral_gplgroups", "type", "-1");
                        if (G1.BadRecord("funeral_gplgroups", record))
                            continue;

                        masterRecord = "";
                        cmd = "Select * from `funeral_master` where `service` = '" + service + "';";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                            masterRecord = dx.Rows[0]["record"].ObjToString();

                        G1.update_db_table("funeral_gplgroups", "record", record, new string[] { "service", service, "price", price.ToString(), "groupname", loadededLocation, "!masterRecord", masterRecord, "SameAsMaster", SameAsMaster, "type", "service", "futurePrice", futurePrice.ToString(), "pastPrice", pastPrice.ToString() });
                    }
                }
                catch ( Exception ex)
                {
                }

                //G1.update_db_table("funeral_gplgroups", "record", serviceRecord, new string[] {"futurePrice", futurePrice.ToString() });
            }
            if (G1.get_column_number(dt, "num") < 0)
                dt.Columns.Add("num");
            G1.NumberDataTable(dt);
            modified = false;
            if (loadedPackage.Trim().ToUpper() == "MASTER")
            {
                cmbLocation_SelectedIndexChanged(null, null);
                alreadyLoaded = true;
            }
            else
            {
            }
            savePackage = "";
            loading = true;
            loading = false;
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
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
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dr = gridMain.GetFocusedDataRow();
            string service = dr["service"].ObjToString();
            string record = dr["record"].ObjToString();
            int rowHandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowHandle);
            string select = dr["SameAsMaster"].ObjToString();
            select = dt.Rows[row]["SameAsMaster"].ObjToString();
            
            string doit = "0";
            if (select == "0")
                doit = "1";
            dr["SameAsMaster"] = doit;
            dt.Rows[row]["SameAsMaster"] = doit;
            dgv.DataSource = dt;
            dgv.RefreshDataSource();

            if ( doit == "1" && !String.IsNullOrWhiteSpace ( workGroup))
            {
                try
                {
                    string cmd = "Select * from `services` where `record` = '" + record + "';";
                    if (!String.IsNullOrWhiteSpace(workGroup))
                        cmd = "Select * from `funeral_gplgroups` where `groupname` = '" + workGroup + "' and `service` = '" + service + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        dr["price"] = dx.Rows[0]["price"].ObjToDouble();
                        if (G1.get_column_number(dt, "data") >= 0)
                        {
                            if (G1.get_column_number(dt, "data") >= 0)
                            {
                                if (G1.get_column_number(dx, "data") >= 0)
                                    dr["data"] = dx.Rows[0]["data"].ObjToDouble();
                                else
                                    dr["data"] = dx.Rows[0]["price"].ObjToDouble();
                            }
                        }
                    }
                }
                catch ( Exception ex)
                {
                }
            }
            else
            {
                try
                {
                    if (workDt != null)
                    {
                        DataRow[] dRow = workDt.Select("service='" + service + "'");
                        if (dRow.Length > 0)
                        {
                            dr["price"] = dRow[0]["price"].ObjToDouble();
                            if (G1.get_column_number(dt, "data") >= 0)
                                dr["data"] = dRow[0]["cost"].ObjToDouble();
                        }
                        else
                        {
                            dr["price"] = 0D;
                            if (G1.get_column_number(dt, "data") >= 0)
                                dr["data"] = 0D;
                        }
                    }
                    else
                    {
                        dr["price"] = 0D;
                        if (G1.get_column_number(dt, "data") >= 0)
                            dr["data"] = 0D;
                    }
                }
                catch ( Exception )
                {
                }
            }
            modified = true;
            ReCalcTotal();
            gridMain.RefreshData();
//            gridMain.RefreshRow(rowHandle);
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void ImportFuneralMaster( DataTable dt)
        {
            string service = "";
            string price = "";
            string record = "";
            string num = "";

            string cmd = "DELETE FROM `funeral_master` where `record` > '0';";
            G1.get_db_data(cmd);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                num = dt.Rows[i][0].ObjToString();
                service = dt.Rows[i][1].ObjToString();
                if (String.IsNullOrWhiteSpace(service))
                    continue;
                price = dt.Rows[i][2].ObjToString();
                //if (String.IsNullOrWhiteSpace(price))
                //    continue;
                if (String.IsNullOrWhiteSpace(price))
                    price = "$0.00";
                price = price.Replace("$", "");
                price = price.Replace(",", "");
                if (!G1.validate_numeric(price))
                    continue;
                service = G1.protect_data(service);

                if (service.ToUpper().IndexOf("ITEMS THAT HAVE PRICES") >= 0)
                    continue;
                if (service.ToUpper().IndexOf("EFFECTIVE DATE") >= 0)
                    continue;
                if (service.ToUpper().IndexOf("THESE PRICES ARE") >= 0)
                    continue;

                record = G1.create_record("funeral_master", "service", "-1");
                if (G1.BadRecord("funeral_master", record))
                    continue;
                G1.update_db_table("funeral_master", "record", record, new string[] { "service", service, "price", price, "type", "service" });
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            SaveServicesLocations(dt);
        }
        /***********************************************************************************************/
        private void updateAllCustomersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string type = dr["type"].ObjToString();
            string service = dr["service"].ObjToString();
            string record = "";

            if ( String.IsNullOrWhiteSpace(type) || String.IsNullOrWhiteSpace ( service))
            {
                MessageBox.Show("***ERROR*** Type and Service must not be blank!");
                return;
            }
            string cmd = "Select * from `cust_services` where `service` = '" + service + "';";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {

            }
        }
        /***********************************************************************************************/
        private void btnRunMarkup_Click(object sender, EventArgs e)
        {
            string str = txtMarkup.Text;
            if (!G1.validate_numeric(str))
                return;
            double markup = str.ObjToDouble();
            double price = 0D;
            double futurePrice = 0D;
            double pastPrice = 0D; // Don't do anything with pastPrice
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                price = dt.Rows[i]["price"].ObjToDouble();
                futurePrice = price * markup;
                dt.Rows[i]["futurePrice"] = futurePrice;
            }
            dgv.DataSource = dt;
            dgv.Refresh();
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void txtMarkup_TextChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void txtMarkup_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter)
                return;
            string str = txtMarkup.Text.Trim();
            if (!G1.validate_numeric(str))
            {
                MessageBox.Show("***Warning*** Markup must me numeric!");
                txtMarkup.Text = "1.00";
                return;
            }
            double markup = txtMarkup.Text.ObjToDouble();
            str = G1.ReformatMoney(markup);
            txtMarkup.Text = str;
        }
        /***********************************************************************************************/
        private void txtMarkup_Leave(object sender, EventArgs e)
        {
            string str = txtMarkup.Text.Trim();
            if (!G1.validate_numeric(str))
            {
                MessageBox.Show("***Warning*** Markup must me numeric!");
                txtMarkup.Text = "1.00";
                return;
            }
            double markup = txtMarkup.Text.ObjToDouble();
            str = G1.ReformatMoney(markup);
            txtMarkup.Text = str;
        }
        /***********************************************************************************************/
        private void picMoveFuture_Click(object sender, EventArgs e)
        {
            if (loading)
                return;
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt == null)
                return;

            double futurePrice = 0D;
            double currentPrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                currentPrice = dt.Rows[i]["price"].ObjToDouble();
                dt.Rows[i]["pastPrice"] = currentPrice;
                futurePrice = dt.Rows[i]["futurePrice"].ObjToDouble();
                if (futurePrice <= 0D)
                    futurePrice = currentPrice;
                dt.Rows[i]["price"] = futurePrice;
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

            double pastPrice = 0D;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                pastPrice = dt.Rows[i]["pastPrice"].ObjToDouble();
                dt.Rows[i]["price"] = pastPrice;
            }

            dgv.DataSource = dt;
            dgv.Refresh();

            modified = true;
            btnSave.Show();
            picMoveFuture.Show();
            picMovePast.Hide();
        }
        /***********************************************************************************************/
        private void btnAddLocation_Click(object sender, EventArgs e)
        {
            string gplGroup = "";
            if ( gplGroup.ToUpper() == "MASTER")
            {
                MessageBox.Show("***ERROR*** You cannot customize the Master GPL Group!\nChoose another GPL Group!");
                return;
            }
            string cmd = "Select * from `funeralhomes` ORDER BY `atneedcode`;";
            DataTable dt = G1.get_db_data(cmd);
            string lines = "";
            string atNeedCode = "";
            string location = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                atNeedCode = dt.Rows[i]["atneedcode"].ObjToString();
                location = dt.Rows[i]["LocationCode"].ObjToString();
                lines += "(" + atNeedCode + ")" + " " + location + "\n";
            }
            using (ListSelect listForm = new ListSelect(lines, true))
            {
                listForm.ListDone += ListForm_LocationDone;
                listForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private void ListForm_LocationDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            string gplGroup = "";
            if (gplGroup.ToUpper() == "MASTER")
                return;
            string location = s;
            string[] Lines = s.Split('\n');
            if (Lines.Length <= 0)
                return;
            location = Lines[0];
            string record = G1.create_record("gpl_locations", "location", "-1");
            if (G1.BadRecord("gpl_location", record))
                return;
            G1.update_db_table ( "gpl_locations", "record", record, new string[]{ "gpl", gplGroup, "location", location });
        }
        /***********************************************************************************************/
        private void cmbLocations_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loading)
                return;
        }
        /***********************************************************************************************/
    }
}