using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid.ViewInfo;
using DevExpress.XtraPrinting;
using DevExpress.Utils;

using GeneralLib;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class InventoryList : DevExpress.XtraEditors.XtraForm
    {
        /***********************************************************************************************/
        private Bitmap emptyImage;
        private bool workAdd = false;
        private bool workOnHand = false;
        private bool workOrders = false;
        private DataTable originalDt = null;
        private bool funModified = false;
        /***********************************************************************************************/
        public InventoryList( bool adding = false, bool onHand = false, bool orders = false )
        {
            InitializeComponent();
            workAdd = adding;
            workOnHand = onHand;
            workOrders = orders;
        }
        /***********************************************************************************************/
        private void InventoryList_Load(object sender, EventArgs e)
        {
            btnSaveAll.Hide();
            SetupVisibleColumns();
            LoadData();
            if ( workAdd )
            {
                btnAdd.Hide();
                btnDelete.Hide();
            }
            else
            {
                btnAddToInventory.Hide();
            }
            loadTypes();
            loadGuage();
        }
        /***********************************************************************************************/
        private void loadTypes()
        {
            string cmd = "Select `caskettype` from `inventorylist` group by `caskettype`;";
            DataTable dt = G1.get_db_data(cmd);
            chkComboType.Properties.DataSource = dt;
        }
        /***********************************************************************************************/
        private void loadGuage()
        {
            string cmd = "Select `casketguage` from `inventorylist` group by `casketguage`;";
            DataTable dt = G1.get_db_data(cmd);
            chkComboGuage.Properties.DataSource = dt;
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
        private void SetupVisibleColumns()
        {
            ToolStripMenuItem menu = this.columnsToolStripMenuItem;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                string caption = gridMain.Columns[i].Caption;
                ToolStripMenuItem nmenu = new ToolStripMenuItem();
                nmenu.Name = name;
                nmenu.Text = caption;
                nmenu.Checked = true;
                nmenu.Click += new EventHandler(nmenu_Click);
                menu.DropDownItems.Add(nmenu);
            }
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            emptyImage = new Bitmap(1, 1);
            string cmd = "Select * from `inventorylist` ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("merchandise", typeof(Bitmap));
            dt.Columns.Add("num");
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["casketguage"] = dt.Rows[i]["casketguage"].ObjToString().Trim();

            SetupPresentColumn(dt);

            G1.NumberDataTable(dt);
            LoadMerchandise(dt);
            checkPreferences();
            dgv.DataSource = dt;
            originalDt = dt;
        }
        /***********************************************************************************************/
        private void checkPreferences ()
        {
            //string preference = G1.getPreference(LoginForm.username, "Merchandise", "Allow Add");
            //if (preference != "YES")
            //    btnAdd.Hide();
            //preference = G1.getPreference(LoginForm.username, "Merchandise", "Allow Delete");
            //if (preference != "YES")
            //    btnDelete.Hide();
        }
        /***********************************************************************************************/
        private void LoadMerchandise ( DataTable dt )
        {
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    dt.Rows[i]["merchandise"] = (Bitmap)(emptyImage);
                    Byte[] bytes = dt.Rows[i]["picture"].ObjToBytes();
                    Image myImage = emptyImage;
                    if (bytes != null)
                    {
                        myImage = G1.byteArrayToImage(bytes);
                        dt.Rows[i]["merchandise"] = (Bitmap)myImage;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Reading Image " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        private void loadMerchandiseImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string desc = dr["casketdesc"].ObjToString();
            using (OpenFileDialog ofdImage = new OpenFileDialog())
            {
                ofdImage.Multiselect = false;

                if (ofdImage.ShowDialog() == DialogResult.OK)
                {
                    string filename = ofdImage.FileName;
                    filename = filename.Replace('\\', '/');
                    if (!String.IsNullOrWhiteSpace(filename))
                    {
                        try
                        {
                            //                        string filename = @"C:\Users\Robby\Documents\SMFS\Inventory\Caskets\Y33_833_TDH_Finch.jpg";
                            Bitmap myNewImage = new Bitmap(filename);
                            ImageConverter converter = new ImageConverter();
                            var bytes = (byte[])converter.ConvertTo(myNewImage, typeof(byte[]));
                            G1.update_blob("inventorylist", "record", record, "picture", bytes);
                            LoadData();
                        }
                        catch ( Exception ex )
                        {
                            MessageBox.Show("***ERROR*** Storing Image " + ex.ToString());
                        }
                    }
                }
                dgv.Refresh();
                this.Refresh();
            }
        }
        /***********************************************************************************************/
        private void clearImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            DialogResult result = MessageBox.Show("Are you sure you want to this Image from this Merchandise?", "Clear Image Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
                return;
            ImageConverter converter = new ImageConverter();
            var bytes = (byte[])converter.ConvertTo(emptyImage, typeof(byte[]));
            G1.update_blob("inventorylist", "record", record, "picture", bytes);
            LoadData();
        }
        /***********************************************************************************************/
        private void gridMain_MouseDown(object sender, MouseEventArgs e)
        {
            int rowHandle = gridMain.FocusedRowHandle;
            GridHitInfo hi = gridMain.CalcHitInfo(e.Location);
            if (hi.RowHandle < 0)
                return;
            if (hi.Column != null)
            {
                if (hi.Column.FieldName.ToUpper() == "MERCHANDISE")
                {
                    rowHandle = hi.RowHandle;
                    DataRow dr = gridMain.GetDataRow(rowHandle);
                    string record = dr["record"].ObjToString();
                    Merchandise mercForm = new Merchandise(record, "EDIT");
                    mercForm.Show();
                }
            }
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
            //if (gridMain.OptionsFind.AlwaysVisible == true)
            //    gridMain.OptionsFind.AlwaysVisible = false;
            //else
            //    gridMain.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void btnLocations_Click(object sender, EventArgs e)
        {
            InventoryLocations inventForm = new InventoryLocations();
            inventForm.Show();
        }
        /***********************************************************************************************/
        private void pictureBox12_Click(object sender, EventArgs e)
        {
            string preference = G1.getPreference(LoginForm.username, "Merchandise", "Allow Add", true);
            if (preference != "YES")
                return;
            Merchandise mercAddForm = new Merchandise("", true);
            mercAddForm.ModuleDone += MercAddForm_ModuleDone;
            mercAddForm.Show();
        }
        /***********************************************************************************************/
        private void MercAddForm_ModuleDone(string s)
        {
            if ( s.Trim().ToUpper().IndexOf ( "RELOAD") >= 0 )
            {
                LoadData();
                DataTable dt = (DataTable)dgv.DataSource;
                int lastrow = dt.Rows.Count;
                gridMain.SelectRow(lastrow);
                gridMain.RefreshData();
                dgv.Refresh();
                ReLoad(s);
            }
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

            font = new Font("Ariel", 10, FontStyle.Bold);
            Printer.DrawQuad(6, 8, 2, 4, "Merchandise List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
//            Printer.DrawQuadTicks();
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ModuleDone;
        /***********************************************************************************************/
        private void btnAddToInventory_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string description = dr["casketdesc"].ObjToString();
            string message = "Are you sure you want to ADD " + description + " to inventory?";
            if ( workOrders )
                message = "Are you sure you want to ADD " + description + " as a New Order?";
            string heading = "Select Merchandise Dialog";
            if (workOnHand)
            {
                message = "Are you sure you want to ADD " + description + " to On-Hand inventory?";
                heading = "Select On-Hand Merchandise Dialog";
            }
            DialogResult result = MessageBox.Show(message, heading, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
                return;
            if (result == DialogResult.No)
            {
                this.Close();
            }
            else
            {
                if (ModuleDone != null)
                    ModuleDone.Invoke(record);
                this.Close();
            }
        }
        /***********************************************************************************************/
        private DataTable CopyTheView(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, DataTable dt )
        {
            DataTable dx = dt.Clone();
            int row = 0;
            for (int i = 0; i < gridMain.RowCount; i++)
            {
                row = gridMain.GetDataSourceRowIndex(i);
                dx.ImportRow(dt.Rows[row]);
            }
            return dx;
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if ( workAdd )
            {
                btnAddToInventory_Click(null, null);
                return;
            }
            DataTable dt = (DataTable)dgv.DataSource;
            DataTable dx = CopyTheView(gridMain, dt);
            Merchandise mercForm = new Merchandise(record, "EDIT", dx);
            mercForm.ModuleDone += MercForm_ModuleDone;
            mercForm.Show();
        }
        /***********************************************************************************************/
        private void ReLoad ( string s )
        {
            try
            {
                string[] Lines = s.Split(' ');
                if (Lines.Length >= 2)
                {
                    if (G1.validate_numeric(Lines[1]))
                    {
                        DataTable dt = (DataTable)dgv.DataSource;
                        string record = Lines[1].ObjToString();
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            if (dt.Rows[i]["record"].ObjToString() == record)
                            {
                                gridMain.FocusedRowHandle = i;
                                gridMain.SelectRow(i);
                                break;
                            }
                        }
                    }
                }
            }
            catch { }
        }
        /***********************************************************************************************/
        private void MercForm_ModuleDone(string s)
        {
            if (String.IsNullOrWhiteSpace(s))
                return;
            if ( s.Trim().ToString().IndexOf ( "RELOAD") >= 0 )
            {
                LoadData();
                ReLoad(s);
                return;
            }
            string cmd = "Select * from `inventorylist` where `record` = '" + s + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            Byte[] bytes = dt.Rows[0]["picture"].ObjToBytes();
            Image myImage = emptyImage;
            if (bytes != null)
                myImage = G1.byteArrayToImage(bytes);

            double money = dt.Rows[0]["casketcost"].ObjToString().ObjToDouble();
            string rtbDesc = dt.Rows[0]["casketdesc"].ObjToString();
            string txtCode = dt.Rows[0]["casketcode"].ObjToString();
            string txtType = dt.Rows[0]["caskettype"].ObjToString();
            string txtGuage = dt.Rows[0]["casketguage"].ObjToString();

            string localRecord = "";
            DataTable dt2 = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt2.Rows.Count; i++)
            {
                localRecord = dt2.Rows[i]["record"].ObjToString();
                if ( s == localRecord)
                {
                    dt2.Rows[i]["casketcost"] = money.ToString();
                    dt2.Rows[i]["casketdesc"] = rtbDesc;
                    dt2.Rows[i]["casketcode"] = txtCode;
                    dt2.Rows[i]["caskettype"] = txtType;
                    dt2.Rows[i]["casketguage"] = txtGuage;
                    dt2.Rows[i]["merchandise"] = (Bitmap)myImage;
                    dgv.RefreshDataSource();
                    dgv.Refresh();
                    gridMain.SelectRow(i);
                    this.Refresh();
                    break;
                }
            }
        }
        /***********************************************************************************************/
        private void pictureBox11_Click(object sender, EventArgs e)
        {
            string preference = G1.getPreference(LoginForm.username, "Merchandise", "Allow Delete", true);
            if (preference != "YES")
                return;

            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string desc = dr["casketdesc"].ObjToString();
            DialogResult result = MessageBox.Show("***Warning*** Are you SURE you want to DELETE " + desc + "?", "Delete Merchandise Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, Merchandise not deleted!", "Delete Merchandise Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            try
            {
                G1.delete_db_table("inventorylist", "record", record);
                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Deleting Merchandise " + desc + "!");
            }
        }
        /***********************************************************************************************/
        private void importBatesvilleItemNumbersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Import importForm = new Import("Batesville");
            importForm.Show();
        }
        /***********************************************************************************************/
        private void chkComboType_EditValueChanged(object sender, EventArgs e)
        {
            string query = getTypeQuery();
            string guages = getGuageQuery();
            if (!String.IsNullOrWhiteSpace(guages))
            {
                if (!String.IsNullOrWhiteSpace(query))
                    query += " AND ";
                query += guages;
            }
            DataRow[] dRows = originalDt.Select(query);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getTypeQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboType.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `caskettype` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkComboGuage_EditValueChanged(object sender, EventArgs e)
        {
            string query = getGuageQuery();
            string types = getTypeQuery();
            if (!String.IsNullOrWhiteSpace(types))
            {
                if ( !String.IsNullOrWhiteSpace(query))
                    query += " AND ";
                query += types;
            }
            DataRow[] dRows = originalDt.Select(query);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /*******************************************************************************************/
        private string getGuageQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboGuage.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `casketguage` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void picRowUp_Click(object sender, EventArgs e)
        {
            if (dgv == null)
                return;
            if (gridMain == null)
                return;

            DataTable dt = (DataTable)dgv.DataSource;
            if (dt.Rows.Count <= 0)
                return;
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowHandle = gridMain.FocusedRowHandle;
            if (rowHandle == 0)
                return; // Already at the first row
            MoveRowUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            btnSaveAll.Show();
            funModified = true;
        }
        /***************************************************************************************/
        private void MoveRowUp(DataTable dt, int row)
        {
            string record = "";
            string record2 = "";
            dt.Columns.Add("Count", Type.GetType("System.Int32"));
            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Count"] = i.ToString();
            dt.Rows[row]["Count"] = (row - 1).ToString();
            dt.Rows[row - 1]["Count"] = row.ToString();
            G1.sortTable(dt, "Count", "asc");
            dt.Columns.Remove("Count");
            G1.NumberDataTable(dt);
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
            btnSaveAll.Show();
            funModified = true;
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
        private void btnSaveAll_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                if (G1.BadRecord("inventorylist", record))
                    continue;
                str = dt.Rows[i]["ps"].ObjToString();
                if (str == "1")
                    str = "Y";
                else
                    str = "";
                G1.update_db_table("inventorylist", "record", record, new string[] { "order", i.ToString(), "present", str });
            }
            funModified = false;
            btnSaveAll.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void SetupPresentColumn(DataTable dt)
        {
            if (G1.get_column_number(dt, "ps") < 0)
                dt.Columns.Add ( "ps" );
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            string text = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                text = dt.Rows[i]["present"].ObjToString();
                if (text == "Y")
                    dt.Rows[i]["ps"] = "1";
                else
                    dt.Rows[i]["ps"] = "0";
            }
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            funModified = true;
            btnSaveAll.Show();
        }
        /***********************************************************************************************/
        private void casketCostsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ImportCasketCosts importForm = new ImportCasketCosts();
            importForm.Show();
        }
        /***********************************************************************************************/
    }
}