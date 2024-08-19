using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using GeneralLib;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using System.Net.Mail;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class AddHomeNew : Form
    {
        public static AddHomeNew addHomeFormNew = null;
        /***********************************************************************************************/
        private string workFunRec = "";
        private bool workNeeded = false;
        private bool modified = false;
        private bool loading = true;
        private DataTable originalDt = null;
        private bool pendingModified = false;
        private Color SaveBackColor = SystemColors.Control;
        private Color NewBackColor = Color.LightGreen;
        /***********************************************************************************************/
        public AddHomeNew(string record = "", bool needed = false)
        {
            int w = this.Width;
            InitializeComponent();
            workFunRec = record;
            workNeeded = needed;
            int width = this.Width;
            width = this.Width + 100;
            int height = this.Height;
            this.SetBounds(this.Left, this.Top, width, height);
            SetupTotalsSummary();
        }
        /****************************************************************************************/
        private void SetupTotalsSummary()
        {
            AddSummaryColumn("minimumOnHand", null, "{0}");
            AddSummaryColumn("Needed", null, "{0}");
            AddSummaryColumn("actualOnHand", null, "{0}");
            AddSummaryColumn("qty", null, "{0}");
            AddSummaryColumn("qtyOrdered", null, "{0}");
            AddSummaryColumn("qtyPending", null, "{0}");
            AddSummaryColumn("actualOnHand", gridMain2, "{0}");
            AddSummaryColumn("qty", gridMain2, "{0}");
            AddSummaryColumn("qtyPending", gridMain2, "{0}");

            AddSummaryColumn("minimumOnHand", gridMain3, "{0}");
            AddSummaryColumn("Needed", gridMain3, "{0}");
            AddSummaryColumn("actualOnHand", gridMain3, "{0}");
            AddSummaryColumn("qty", gridMain3, "{0}");
            AddSummaryColumn("qtyOrdered", gridMain3, "{0}");
            AddSummaryColumn("qtyPending", gridMain3, "{0}");
        }
        /****************************************************************************************/
        private void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                gMain = gridMain;
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /***********************************************************************************************/
        private void AddHomeNew_Load(object sender, EventArgs e)
        {
            //SaveBackColor = btnSave.BackColor;
            dgv3.Visible = false;
            pendingModified = false;
            this.panelOrdersTop.Hide();
            this.gridMain2.ExpandAllGroups();
            btnSaveOrders.Visible = false;
            gridBand3.Visible = false;
            picLoader.Hide();
            getLocations();
            G1.SetupVisibleColumns(gridMain, this.columnsToolStripMenuItem, nmenu_Click);
            if (!String.IsNullOrWhiteSpace(workFunRec))
            {
                loading = true;
                //this.txtKeyCode.Enabled = false;
                string cmd = "Select * from `funeralhomes` where `record` = '" + workFunRec + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    string keyCode = "";
                    string name = "";
                    string state = "";
                    string phone = "";
                    string excludeOnReport = "";
                    try
                    {
                        keyCode = dt.Rows[0]["keycode"].ObjToString();
                        this.txtKeyCode.Text = keyCode;
                        name = dt.Rows[0]["name"].ObjToString();
                        this.txtName.Text = name;
                        txtNeedCode.Text = dt.Rows[0]["atneedcode"].ObjToString();
                        txtAccountCode.Text = dt.Rows[0]["accountcode"].ObjToString();
                        txtLocationCode.Text = dt.Rows[0]["LocationCode"].ObjToString();
                        state = dt.Rows[0]["state"].ObjToString();
                        SetupComboTable(this.comboStates, "ref_states", "abbrev", state);
                        txtAddress.Text = dt.Rows[0]["address"].ObjToString();
                        txtCity.Text = dt.Rows[0]["city"].ObjToString();
                        txtZip.Text = dt.Rows[0]["zip"].ObjToString();
                        txtCashRemitHeading.Text = dt.Rows[0]["cashRemitHeading"].ObjToString();
                        txtRemitCombineKeyCodes.Text = dt.Rows[0]["remitCombineKeyCodes"].ObjToString();
                        txtLocind.Text = dt.Rows[0]["locind"].ObjToString();
                        txtReportHeading.Text = dt.Rows[0]["locindHeading"].ObjToString();
                        txtLicenseNo.Text = dt.Rows[0]["licenseNo"].ObjToString();
                        txtSlogan.Text = dt.Rows[0]["slogan"].ObjToString();
                        txtWeb.Text = dt.Rows[0]["webaddress"].ObjToString();
                        txtMerchandiseCode.Text = dt.Rows[0]["merchandiseCode"].ObjToString();

                        state = dt.Rows[0]["POState"].ObjToString();
                        SetupComboTable(this.cmbPOState, "ref_states", "abbrev", state);
                        txtPOBox.Text = dt.Rows[0]["POBox"].ObjToString();
                        txtPOCity.Text = dt.Rows[0]["POCity"].ObjToString();
                        txtPOZip.Text = dt.Rows[0]["POZip"].ObjToString();
                        txtPhone.Text = dt.Rows[0]["phoneNumber"].ObjToString();
                        txtManager.Text = dt.Rows[0]["manager"].ObjToString();
                        txtSigner.Text = dt.Rows[0]["signer"].ObjToString();
                        txtEmail.Text = dt.Rows[0]["email"].ObjToString();
                        txtOwnerSSN.Text = dt.Rows[0]["ownerSSN"].ObjToString();
                        txtSDICode.Text = dt.Rows[0]["SDICode"].ObjToString();
                        excludeOnReport = dt.Rows[0]["excludeOnReport"].ObjToString();
                        if (excludeOnReport.ToUpper() == "Y")
                            chkExcludeOnReport.Checked = true;
                    }
                    catch ( Exception ex )
                    {
                        MessageBox.Show("***ERROR*** Problem looking up funeral Home Record=" + workFunRec + " KeyCode=" + keyCode + " Name=" + name + " State=" + state);
                    }
                }
                txtAccountCode.Focus();
                this.ActiveControl = txtAccountCode;
                loading = false;
                this.Refresh();
            }
            if (workNeeded)
            {
                this.Text = "Minimum On-Hand Report";
                panelTop.Hide();
                btnAdd.Hide();
                btnDelete.Hide();
            }
            else 
            {
                lblLocation.Hide();
                chkComboLocation.Hide();
            }
//            btnSendEmailReport.Hide();
            LoadOnHand();
            modified = false;
            addHomeFormNew = this;
            dgv.Dock = DockStyle.Fill;
            dgv.Visible = true;

            if ( !workNeeded )
                panelBottom.Hide();
            else
            {
                this.WindowState = FormWindowState.Maximized;
                if (SMFS.SMFS_MainForm != null)
                    SMFS.SMFS_MainForm.WindowState = FormWindowState.Minimized;
            }
            //btnSave.BackColor = SaveBackColor;
        }
        /***********************************************************************************************/
        private void SetupComboTable(System.Windows.Forms.ComboBox box, string db, string field, string answer)
        {
            string cmd = "Select * from `" + db + "`;";
            DataTable dt = G1.get_db_data(cmd);

            DataSet myDataSet = new DataSet();
            myDataSet.Tables.Add(dt);

            box.DataSource = myDataSet.Tables[0].DefaultView;
            box.DisplayMember = field;
            box.Text = answer;
        }
        /***********************************************************************************************/
        private DataTable _LocationList;
        private void getLocations()
        {
            string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            _LocationList = G1.get_db_data(cmd);

            string str = "";

            for ( int i=_LocationList.Rows.Count-1; i>=0; i--)
            {
                str = _LocationList.Rows[i]["LocationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    _LocationList.Rows.RemoveAt(i);
            }

            chkComboLocation.Properties.DataSource = _LocationList;
        }
        /*******************************************************************************************/
        private string getLocationQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `locationCode` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void LoadOnHand()
        {
            this.Cursor = Cursors.WaitCursor;
            string cmd = "Select * from `inventory_on_hand` i JOIN `inventorylist` l on i.`!casketRecord` = l.`record` ";
            if (!workNeeded)
                cmd += " WHERE `!homeRecord` = '" + workFunRec + "' ";

            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("actualOnHand", Type.GetType("System.Int32"));
            dt.Columns.Add("Needed", Type.GetType("System.Int32"));
            dt.Columns.Add("locationCode");
            dt.Columns.Add("loc");
            dt.Columns.Add("accountcode");
            dt.Columns.Add("SerialNumber");
            dt.Columns.Add("ServiceID");
            dt.Columns.Add("DateUsed");
            dt.Columns.Add("Order", Type.GetType("System.Int32"));
            dt.Columns.Add("orderdate");
            dt.Columns.Add("replacement");
            dt.Columns.Add("qty", Type.GetType("System.Int32"));
            dt.Columns.Add("qtyPending", Type.GetType("System.Int32"));
            gridMain.Columns["locationCode"].Visible = true;
//            gridMain.Columns["loc"].Visible = true;
            gridMain.Columns["accountcode"].Visible = true;

            dt = CheckNeedOrders( dt );

            G1.NumberDataTable(dt);
            CheckPendingOrders( dt );
            originalDt = dt;
            dgv.DataSource = dt;
            if (chkSort.Checked)
                this.gridMain.ExpandAllGroups();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private DataTable CheckNeedOrders ( DataTable dt )
        {
            string record = "";
            string workRecord = "";
            string cmd = "";
            string str = "";
            string casketRecord = "";
            string locationCode = "";
            string accountcode = "";
            string casketdescription = "";
            string casketcode = "";
            string itemnumber = "";
            bool added = false;
            DataTable dx = null;
            DataTable dd = null;
            int minimum = 0;
            int onhand = 0;
            int need = 0;
            dt.Rows.Clear();
            cmd = "Select * from `funeralhomes` where `record` = '" + workFunRec + "' order by `keycode`;";
            if (workNeeded)
            {
                string where = " WHERE ";
                cmd = "Select * from `funeralhomes`  ";
                string locations = getLocationQuery();
                if (!String.IsNullOrWhiteSpace(locations))
                {
                    printLocation = chkComboLocation.Text;
                    cmd += " " + where + " " + locations;
                    where = "AND";
                }
                cmd += "order by `keycode`;";
            }
            dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return dt;
            DataTable dp = null;
            int order = 0;
            for (int k = 0; k < dx.Rows.Count; k++)
            {
                locationCode = dx.Rows[k]["LocationCode"].ObjToString();
                if (!workNeeded)
                    printLocation = locationCode;
                accountcode = dx.Rows[k]["accountcode"].ObjToString();
                workRecord = dx.Rows[k]["record"].ObjToString();
                cmd = "Select * from `inventory_on_hand` i JOIN `inventorylist` l on i.`!casketRecord` = l.`record` ";
                cmd += " WHERE `!homeRecord` = '" + workRecord + "' ";
                cmd += ";";
                dp = G1.get_db_data(cmd);
                dp.Columns.Add("num");
                dp.Columns.Add("actualOnHand", Type.GetType("System.Int32"));
                dp.Columns.Add("Needed", Type.GetType("System.Int32"));
                dp.Columns.Add("locationCode");
                dp.Columns.Add("loc");
                dp.Columns.Add("accountcode");
                dp.Columns.Add("Order", Type.GetType("System.Int32"));

                for (int i = 0; i < dp.Rows.Count; i++)
                {
                    added = false;
                    str = dp.Rows[i]["minimumOnHand"].ObjToString();
                    if (G1.validate_numeric(str))
                    {
                        minimum = str.ObjToInt32();
                        if (minimum > 0)
                        {
                            casketRecord = dp.Rows[i]["!casketRecord"].ObjToString();
                            cmd = "Select * from `inventorylist` where `record` = '" + casketRecord + "';";
                            dd = G1.get_db_data(cmd);
                            if (dd.Rows.Count <= 0)
                                continue;
                            casketdescription = dd.Rows[0]["casketdesc"].ObjToString();
                            casketcode = dd.Rows[0]["casketcode"].ObjToString();
                            itemnumber = dd.Rows[0]["itemnumber"].ObjToString();
                            onhand = ImportInventoryList.GetActualOnHand(locationCode, casketdescription);
                            if (onhand < minimum || !workNeeded || 1 == 1)
                            {
                                need = minimum - onhand;
                                dp.Rows[i]["locationCode"] = locationCode;
                                dp.Rows[i]["loc"] = locationCode;
                                dp.Rows[i]["casketcode"] = casketcode;
                                dp.Rows[i]["itemnumber"] = itemnumber;
                                dp.Rows[i]["accountcode"] = accountcode;
                                dp.Rows[i]["actualOnHand"] = onhand;
                                dp.Rows[i]["Needed"] = need;
                                dp.Rows[i]["Order"] = order++;
                                added = true;
                                dt.ImportRow(dp.Rows[i]);
                            }
                            if ( chkShowUsed.Checked )
                            {
                                DateTime date = dateTimePicker1.Value;
                                string date1 = G1.DateTimeToSQLDateTime(date);
                                DateTime ddate = dateTimePicker2.Value;
                                string date2 = G1.DateTimeToSQLDateTime(ddate);

                                cmd = "Select * from `inventory` where `LocationCode` = '" + locationCode + "' and `casketdescription` = '" + casketdescription + "' and `ServiceID` <> '' ";
                                cmd += " and `DateUsed` >= '" + date1 + "' ";
                                cmd += " and `DateUsed` <= '" + date2 + "' ";
                                cmd += " and `del` <> '1' ";
                                cmd += ";";
                                dd = G1.get_db_data(cmd);
                                for ( int j=0; j<dd.Rows.Count; j++)
                                {
                                    if (added)
                                    {
                                        int row = dt.Rows.Count - 1;
                                        dt.Rows[row]["SerialNumber"] = dd.Rows[j]["SerialNumber"].ObjToString();
                                        dt.Rows[row]["ServiceID"] = dd.Rows[j]["ServiceID"].ObjToString();
                                        dt.Rows[row]["DateUsed"] = dd.Rows[j]["DateUsed"].ObjToString();
                                        dt.Rows[row]["casketdesc"] = dd.Rows[j]["CasketDescription"].ObjToString();
                                        added = false;
                                    }
                                    else
                                    {
                                        DataRow dRow = dt.NewRow();
                                        dRow["SerialNumber"] = dd.Rows[j]["SerialNumber"].ObjToString();
                                        dRow["ServiceID"] = dd.Rows[j]["ServiceID"].ObjToString();
                                        dRow["DateUsed"] = dd.Rows[j]["DateUsed"].ObjToString();
                                        dRow["casketdesc"] = dd.Rows[j]["CasketDescription"].ObjToString();
                                        dRow["locationCode"] = locationCode;
                                        dRow["loc"] = locationCode;
                                        dRow["accountcode"] = accountcode;
                                        dRow["Order"] = order++;
                                        dt.Rows.Add(dRow);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if ( chkShowUsed.Checked )
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "Order DESC";
                dt = tempview.ToTable();

                if (chkAllOther.Checked)
                    ShowAllOther(dt, dx, order);
            }
            if (chkUnused.Checked)
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "Order DESC";
                dt = tempview.ToTable();
                ShowAllUnused(dt, dx, order);
                tempview = dt.DefaultView;
                tempview.Sort = "loc, Order";
                dt = tempview.ToTable();
            }
            return dt;
        }
        /***********************************************************************************************/
        private void ShowAllUnused (DataTable dt, DataTable fx, int order)
        {
            string serialNumber = "";
            string locationCode = "";
            string casketDesc = "";
            string serviceId = "";
            string ownership = "";
            string query = "";
            string str = "";
            string minimum = "";
            bool found = false;

            string cmd = "Select * from `inventory` where `ServiceID` = '' and `del` <> '1' order by `LocationCode`,`CasketDescription`";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                locationCode = dx.Rows[i]["locationCode"].ObjToString();
                casketDesc = dx.Rows[i]["CasketDescription"].ObjToString();
                if (casketDesc == "OS9 882 D Sand-28")
                {
                }
                serialNumber = dx.Rows[i]["SerialNumber"].ObjToString();
                serviceId = dx.Rows[i]["ServiceID"].ObjToString();
                ownership = dx.Rows[i]["Ownership"].ObjToString();
                //                DataRow[] dRows = dt.Select("SerialNumber='" + serialNumber + "'");
                query = "LocationCode='" + locationCode + "' AND casketdesc ='" + casketDesc + "'";
                DataRow[] dRows = dt.Select( query );
                if (dRows.Length <= 0)
                {
                    LoadInventoryItem(fx, ref order, dt, locationCode, serviceId, casketDesc, ownership);
//                    locationCode = dx.Rows[i]["locationCode"].ObjToString();
//                    DataRow dR = dt.NewRow();
////                    dR["SerialNumber"] = dx.Rows[i]["SerialNumber"].ObjToString();
//                    dR["ServiceID"] = dx.Rows[i]["ServiceID"].ObjToString();
////                    dR["DateUsed"] = dx.Rows[i]["DateUsed"].ObjToString();
//                    dR["casketdesc"] = dx.Rows[i]["CasketDescription"].ObjToString();
//                    dR["locationCode"] = locationCode;
//                    dR["loc"] = locationCode;
//                    dR["accountcode"] = "XXX";
//                    dR["actualOnHand"] = 1;
//                    dR["Order"] = order++;
//                    DataRow[] dRR = fx.Select("locationCode='" + locationCode + "'");
//                    if (dRR.Length > 0)
//                        dR["accountcode"] = dRR[0]["accountcode"].ObjToString();

//                    casketDesc = dx.Rows[i]["CasketDescription"].ObjToString();
//                    cmd = "Select * from `inventorylist` where `casketdesc` = '" + casketDesc + "';";
//                    DataTable dd = G1.get_db_data(cmd);
//                    if (dd.Rows.Count > 0)
//                    {
//                        dR["casketcode"] = dd.Rows[0]["casketcode"].ObjToString();
//                        dR["itemnumber"] = dd.Rows[0]["itemnumber"].ObjToString();
//                    }
//                    dR["num"] = dt.Rows.Count;
//                    dt.Rows.Add(dR);
                }
                else
                {
                    if ( dRows.Length > 1 )
                    {
                        if (casketDesc == "OS9 882 D Sand-28")
                        {
                        }
                    }
                    found = false;
                    for (int j = 0; j < dRows.Length; j++)
                    {
                        minimum = dRows[j]["minimumOnHand"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(minimum))
                        {
                            found = true;
                            break;
                        }

                        serialNumber = dRows[j]["SerialNumber"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(serialNumber))
                            continue;
                        int num = dRows[j]["num"].ObjToInt32();
                        int actualOnHand = dRows[j]["actualOnHand"].ObjToInt32();
                        actualOnHand++;
                        if (num > 0 && num < dt.Rows.Count)
                        {
                            str = dt.Rows[num]["casketdesc"].ObjToString();
                            dt.Rows[num]["actualOnHand"] = actualOnHand;
                        }
                        found = true;
                        break;
                    }
                    if ( !found )
                        LoadInventoryItem(fx, ref order, dt, locationCode, serviceId, casketDesc, ownership);
                }
            }
            dt.AcceptChanges();
        }
        /***********************************************************************************************/
        private void LoadInventoryItem( DataTable fx, ref int order, DataTable dt, string locationCode, string serviceId, string casketDescription, string ownership )
        {
            DataRow dR = dt.NewRow();
            dR["ServiceID"] = serviceId;
            dR["casketdesc"] = casketDescription;
            dR["locationCode"] = locationCode;
            dR["loc"] = locationCode;
            dR["Ownership"] = ownership;
            dR["accountcode"] = "XXX";
            dR["actualOnHand"] = 1;
            dR["Order"] = order++;
            DataRow[] dRR = fx.Select("locationCode='" + locationCode + "'");
            if (dRR.Length > 0)
                dR["accountcode"] = dRR[0]["accountcode"].ObjToString();

            string cmd = "Select * from `inventorylist` where `casketdesc` = '" + casketDescription + "';";
            DataTable dd = G1.get_db_data(cmd);
            if (dd.Rows.Count > 0)
            {
                dR["casketcode"] = dd.Rows[0]["casketcode"].ObjToString();
                dR["itemnumber"] = dd.Rows[0]["itemnumber"].ObjToString();
            }
            dR["num"] = dt.Rows.Count;
            dt.Rows.Add(dR);
        }
        /***********************************************************************************************/
        private DataTable ShowAllOther( DataTable dt, DataTable fx, int order )
        {
            string serialNumber = "";
            string locationCode = "";
            string casketCode = "";
            string casketDesc = "";
            DateTime date = dateTimePicker1.Value;
            string date1 = G1.DateTimeToSQLDateTime(date);
            DateTime ddate = dateTimePicker2.Value;
            string date2 = G1.DateTimeToSQLDateTime(ddate);

            string cmd = "Select * from `inventory` where `ServiceID` <> '' ";
            cmd += " and `DateUsed` >= '" + date1 + "' ";
            cmd += " and `DateUsed` <= '" + date2 + "' ";
            cmd += " and `del` <> '1' ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                serialNumber = dx.Rows[i]["SerialNumber"].ObjToString();
                DataRow[] dRows = dt.Select("SerialNumber='" + serialNumber + "'");
                if (dRows.Length <= 0)
                {
                    locationCode = dx.Rows[i]["locationCode"].ObjToString();
                    DataRow dR = dt.NewRow();
                    dR["SerialNumber"] = dx.Rows[i]["SerialNumber"].ObjToString();
                    dR["ServiceID"] = dx.Rows[i]["ServiceID"].ObjToString();
                    dR["DateUsed"] = dx.Rows[i]["DateUsed"].ObjToString();
                    dR["casketdesc"] = dx.Rows[i]["CasketDescription"].ObjToString();
                    dR["locationCode"] = locationCode;
                    dR["loc"] = "Z_" + locationCode;
                    dR["accountcode"] = "XXX";
                    dR["Order"] = order++;
                    DataRow[] dRR = fx.Select("locationCode='" + locationCode + "'");
                    if (dRR.Length > 0)
                        dR["accountcode"] = dRR[0]["accountcode"].ObjToString();

                    casketDesc = dx.Rows[i]["CasketDescription"].ObjToString();
                    cmd = "Select * from `inventorylist` where `casketdesc` = '" + casketDesc + "';";
                    DataTable dd = G1.get_db_data(cmd);
                    if (dd.Rows.Count > 0)
                    {
                        dR["casketcode"] = dd.Rows[0]["casketcode"].ObjToString();
                        dR["itemnumber"] = dd.Rows[0]["itemnumber"].ObjToString();
                    }
                    dt.Rows.Add(dR);
                }
            }
            dt.AcceptChanges();
            return dt;
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
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (modified)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to exit without saving your changes?", "Add/Edit Funeral Home Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                    return;
                if (result == DialogResult.Yes)
                {
                    modified = false;
                    this.Close();
                }
            }
            this.Close();
        }
        /***********************************************************************************************/
        private void txtName_KeyDown(object sender, KeyEventArgs e)
        {
            if (!loading)
                modified = true;
        }
        /***********************************************************************************************/
        private void txtKeyCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (!loading)
                modified = true;
        }
        /***********************************************************************************************/
        private bool checkDuplicateKeyCode()
        {
            if (loading)
                return false;
            string keycode = this.txtKeyCode.Text;
            if (String.IsNullOrWhiteSpace(keycode))
            {
                MessageBox.Show("***ERROR*** Invalid Key!\nKey must be unique and not blank!", "Add/Edit Funeral Home Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + keycode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                MessageBox.Show("***ERROR*** Duplicate Key!\nYou must enter a unique key!", "Add/Edit Funeral Home Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return true;
            }
            return false;
        }
        /***********************************************************************************************/
        private void txtKeyCode_TextChanged(object sender, EventArgs e)
        {
            if (!loading)
            {
                string keycode = this.txtKeyCode.Text;
                if (checkDuplicateKeyCode())
                    return;
                modified = true;
                btnSave.BackColor = NewBackColor;
            }
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            string keycode = this.txtKeyCode.Text;
            if (String.IsNullOrWhiteSpace(keycode))
                return;
            string name = this.txtName.Text;
            if (String.IsNullOrWhiteSpace(name))
                return;
            string record = workFunRec;
            if (String.IsNullOrWhiteSpace(record))
            {
                if (checkDuplicateKeyCode())
                    return;
                record = G1.create_record("funeralhomes", "keycode", "-1");
                if (string.IsNullOrWhiteSpace(record))
                    MessageBox.Show("***ERROR*** Creating DB Record!");
                else if (record == "-1")
                    MessageBox.Show("***ERROR*** Creating DB Record!");
            }
            G1.update_db_table("funeralhomes", "record", record, new string[] { "keycode", keycode, "name", name });
            string atneedcode = txtNeedCode.Text;
            string accountcode = txtAccountCode.Text;
            string locationcode = txtLocationCode.Text;
            string address = txtAddress.Text;
            string city = txtCity.Text;
            string state = comboStates.Text;
            string zip = txtZip.Text;
            string phone = txtPhone.Text;
            string manager = txtManager.Text;
            string signer = txtSigner.Text;
            string cashRemitHeading = txtCashRemitHeading.Text;
            string remitCombineKeyCodes = txtRemitCombineKeyCodes.Text;
            string email = txtEmail.Text;
            string ownerSSN = txtOwnerSSN.Text;
            string SDICode = txtSDICode.Text;
            G1.update_db_table("funeralhomes", "record", record, new string[] { "atneedcode", atneedcode, "accountcode", accountcode, "LocationCode", locationcode, "address", address, "city", city, "state", state, "zip", zip, "cashRemitHeading", cashRemitHeading, "remitCombineKeyCodes", remitCombineKeyCodes, "phoneNumber", phone, "manager", manager, "email", email, "ownerSSN", ownerSSN, "SDICode", SDICode, "signer", signer });

            address = txtPOBox.Text;
            city    = txtPOCity.Text;
            state   = cmbPOState.Text;
            zip     = txtPOZip.Text;
            string locind = txtLocind.Text;
            string reportHeading = txtReportHeading.Text;
            string licenseNo = txtLicenseNo.Text;
            string merchandiseCode = txtMerchandiseCode.Text;
            string slogan = txtSlogan.Text;
            slogan = G1.protect_data(slogan);
            string webaddress = txtWeb.Text;
            webaddress = G1.protect_data(webaddress);
            string excludeOnReport = "";
            if (chkExcludeOnReport.Checked)
                excludeOnReport = "Y";
            G1.update_db_table("funeralhomes", "record", record, new string[] { "POBox", address, "POCity", city, "POState", state, "POZip", zip, "locind", locind, "locindHeading", reportHeading, "licenseNo", licenseNo, "slogan", slogan, "merchandiseCode", merchandiseCode, "webaddress", webaddress, "excludeOnReport", excludeOnReport });

            modified = false;
            btnSave.BackColor = SaveBackColor;
            OnListDone();
//            this.Close();
        }
        /***********************************************************************************************/
        private void AddHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (modified || pendingModified )
            {
                DialogResult result = MessageBox.Show("Are you sure you want to exit without saving your changes?", "Add/Edit Funeral Home Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }
            addHomeFormNew = null;
            if (workNeeded)
            {
                if (SMFS.SMFS_MainForm != null)
                    SMFS.SMFS_MainForm.WindowState = FormWindowState.Normal;
            }
        }
        /***************************************************************************************/
        public delegate void d_void_eventdone_string(string s);
        public event d_void_eventdone_string ListDone;
        protected void OnListDone()
        {
            if (ListDone != null)
            {
                string data = workFunRec;
                if (!string.IsNullOrWhiteSpace(data))
                {
                    ListDone.Invoke(data);
                    this.Close();
                }
            }
        }
        /***********************************************************************************************/
        private void something_Changed(object sender, EventArgs e)
        {
            if (!loading)
            {
                modified = true;
                btnSave.BackColor = NewBackColor;
            }
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            InventoryList inventForm = new InventoryList(true, true);
            inventForm.ModuleDone += InventForm_ModuleDone;
            inventForm.Show();
        }
        /***********************************************************************************************/
        private void InventForm_ModuleDone(string s)
        {
            string merchandiseRecord = s;
            if (String.IsNullOrWhiteSpace(merchandiseRecord))
                return;
            string cmd = "Select * from `inventorylist` where `record` = '" + merchandiseRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string record = dt.Rows[0]["record"].ObjToString();
            DataTable dx = (DataTable)dgv.DataSource;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                string locRecord = dx.Rows[i]["!casketRecord"].ObjToString();
                if (locRecord == record)
                {
                    MessageBox.Show("***ERROR*** Location already exists in On-Hand Database!");
                    return;
                }
            }

            string newrecord = G1.create_record("inventory_on_hand", "minimumOnHand", "-1");
            if (String.IsNullOrWhiteSpace(newrecord) || newrecord == "-1")
            {
                MessageBox.Show("***ERROR*** Creating New Record in On-Hand Database!");
                return;
            }

            G1.update_db_table("inventory_on_hand", "record", newrecord, new string[] { "!casketRecord", merchandiseRecord, "!homeRecord", workFunRec, "minimumOnHand", "1" });
            LoadOnHand();
        }
        /***********************************************************************************************/
        private void CheckOrdersNeeded()
        {
            DataTable dt = (DataTable)dgv.DataSource;
            bool found = false;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                if ( dt.Rows[i]["qty"].ObjToDouble() > 0D)
                {
                    btnSaveOrders.Visible = true;
                    found = true;
                    break;
                }
            }
            if (!found)
                btnSaveOrders.Visible = false;
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            CheckOrdersNeeded();
            DataRow dr = gridMain.GetFocusedDataRow();
            int rowhandle = gridMain.FocusedRowHandle;
            int row = gridMain.GetDataSourceRowIndex(rowhandle);
            string record = dr["record"].ObjToString();
//            modified = true;
            if (e.Column.FieldName.Trim().ToUpper() == "MINIMUMONHAND")
            {
                string value = dr["minimumOnHand"].ObjToString();
                if ( G1.validate_numeric ( value ))
                {
                    G1.update_db_table("inventory_on_hand", "record", record, new string[] { "minimumOnHand", value });
                }
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "OWNERSHIP")
            {
                string value = dr["Ownership"].ObjToString();
                G1.update_db_table("inventory_on_hand", "record", record, new string[] { "Ownership", value });
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "QTY")
            {
                string value = dr["qty"].ObjToString();
                if ( G1.validate_numeric ( value))
                {
                    int ivalue = value.ObjToInt32();
                    if( ivalue > 0 )
                    {
                        string date = DateTime.Now.ToString("MM/dd/yyyy");
                        dr["orderdate"] = G1.DTtoMySQLDT(date);
                    }
                    else
                    {
                        dr["orderdate"] = null;
                    }
                }
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "ORDERDATE")
            {
                string value = dr["orderdate"].ObjToString();
                if (G1.validate_date(value))
                {
                    dr["orderdate"] = G1.DTtoMySQLDT(value);
                }
            }
        }
        /***********************************************************************************************/
        private int pageMarginLeft = 0;
        private int pageMarginRight = 0;
        private int pageMarginTop = 0;
        private int pageMarginBottom = 0;
        private int minimumOnHandWidth = -1;
        private int actualOnHandWidth = -1;
        private int NeededWidth = -1;
        private int locationWidth = -1;
        private int casketCodeWidth = -1;
        private int casketTypeWidth = -1;
        private int numWidth = -1;
        private string printLocation = "";
        /***********************************************************************************************/
        private void printableComponentLink1_BeforeCreateAreas(object sender, EventArgs e)
        {
            minimumOnHandWidth = gridMain.Columns["minimumOnHand"].Width;
            gridMain.Columns["minimumOnHand"].Caption = "Minimum";
            actualOnHandWidth = gridMain.Columns["actualOnHand"].Width;
            gridMain.Columns["actualOnHand"].Caption = "Actual";
            NeededWidth = gridMain.Columns["Needed"].Width;
            locationWidth = gridMain.Columns["locationCode"].Width;
            casketCodeWidth = gridMain.Columns["casketcode"].Width;
            casketTypeWidth = gridMain.Columns["caskettype"].Width;
            numWidth = gridMain.Columns["num"].Width;
            gridMain.Columns["minimumOnHand"].Width = 100;
            gridMain.Columns["actualOnHand"].Width = 100;
            gridMain.Columns["Needed"].Width = 100;
            gridMain.Columns["locationCode"].Width = 100;
            gridMain.Columns["casketcode"].Width = 125;
            gridMain.Columns["caskettype"].Width = 90;
            gridMain.Columns["num"].Width = 75;
        }
        /***********************************************************************************************/
        private void printableComponentLink1_AfterCreateAreas(object sender, EventArgs e)
        {
            gridMain.Columns["minimumOnHand"].Width = minimumOnHandWidth;
            gridMain.Columns["actualOnHand"].Width = actualOnHandWidth;
            gridMain.Columns["Needed"].Width = NeededWidth;
            gridMain.Columns["locationCode"].Width = locationWidth;
            gridMain.Columns["casketcode"].Width = casketCodeWidth;
            gridMain.Columns["caskettype"].Width = casketTypeWidth;
            gridMain.Columns["num"].Width = numWidth;
            gridMain.Columns["minimumOnHand"].Caption = "Minimum On-Hand";
            gridMain.Columns["actualOnHand"].Caption = "Actual On-Hand";
        }
        /***********************************************************************************************/
        private void printPreviewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreviewOrExport();
        }
        /***********************************************************************************************/
        private void printPreviewOrExport ( bool exportToPdf = false )
        {
            footerCount = 0;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            if ( dgv2.Visible )
                printableComponentLink1.Component = dgv2;
            if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            //printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 100, 50);
            gridMain.AppearancePrint.HeaderPanel.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;


            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            string filename = @"C:\rag\emailtest.pdf";
            if (File.Exists(filename))
                File.Delete(filename);
            if (exportToPdf)
                printableComponentLink1.ExportToPdf(filename);
            else
                printableComponentLink1.ShowPreview();
        }
        /***********************************************************************************************/
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            footerCount = 0;
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            printableComponentLink1.Component = dgv;
            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;

            printableComponentLink1.PrintingSystemBase = printingSystem1;
            //printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 50, 100, 50);

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
        private void printableComponentLink1_CreateMarginalHeaderArea(object sender, CreateAreaEventArgs e)
        {
            Printer.setupPrinterQuads(e, 2, 3);
            System.Drawing.Font font = new System.Drawing.Font("Ariel", 16);
            Printer.DrawQuad(1, 1, Printer.xQuads, 1, "South Mississippi Funeral Services, LLC", Color.Black, BorderSide.Bottom, font, HorizontalAlignment.Center);

            Printer.SetQuadSize(12, 12);

            font = new Font("Ariel", 8);
            Printer.DrawGridDate(2, 1, 2, 2, Color.Black, BorderSide.None, font);
            Printer.DrawGridPage(11, 2, 2, 2, Color.Black, BorderSide.None, font);
            Printer.DrawQuad(2, 6, 2, 2, "User : " + LoginForm.username, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //string search = "Search Date : All";
            //if (chkDate.Checked)
            //    search = printDate;
            //Printer.DrawQuad(1, 6, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            string search = "";
            font = new Font("Ariel", 8);
            search = "Location : All";
            if (!String.IsNullOrWhiteSpace(printLocation))
                search = "Location : " + printLocation;
            Printer.DrawQuad(2, 8, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            if ( chkShowUsed.Checked )
            {
                search = "Date : " + this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " - " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
                Printer.DrawQuad(2, 10, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }

            //search = "Type : All";
            //if (!String.IsNullOrWhiteSpace(printType))
            //    search = "Type : " + printType;
            //Printer.DrawQuad(1, 10, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //search = "Guage : All";
            //if (!String.IsNullOrWhiteSpace(printGuage))
            //    search = "Guage : " + printGuage;
            //Printer.DrawQuad(7, 6, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //search = "Ownership : All";
            //if (!String.IsNullOrWhiteSpace(printOwner))
            //    search = "Ownership : " + printOwner;
            //Printer.DrawQuad(7, 8, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //search = "Used/Unused : All";
            //if (!String.IsNullOrWhiteSpace(printUsed))
            //    search = "Used/Unused : " + printUsed;
            //Printer.DrawQuad(7, 10, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);

            //if (labValue.Visible)
            //{
            //    string str = labelValue.Text + " " + labValue.Text;
            //    Printer.DrawQuad(10, 9, 3, 3, str, Color.Black, BorderSide.Right, labValue.Font, HorizontalAlignment.Left, VertAlignment.Top);
            //}

            font = new Font("Ariel", 10, FontStyle.Bold | FontStyle.Italic);
            Printer.DrawQuad(6, 9, 6, 3, "On-Hand Inventory List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
//            Printer.DrawQuadBorderRight(6, 5, 1, 12, BorderSide.Right, 1, Color.Black);
            Printer.DrawQuadBorderRight(12, 6, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (dgv3.Visible)
            {
                if (gridMain3.OptionsFind.AlwaysVisible == true)
                    gridMain3.OptionsFind.AlwaysVisible = false;
                else
                    gridMain3.OptionsFind.AlwaysVisible = true;
            }
            else
            {
                if (gridMain.OptionsFind.AlwaysVisible == true)
                    gridMain.OptionsFind.AlwaysVisible = false;
                else
                    gridMain.OptionsFind.AlwaysVisible = true;
            }
        }
        /*******************************************************************************************/
        private string getLocationNameQuery()
        {
            string procLoc = "";
            string[] locIDs = this.chkComboLocation.EditValue.ToString().Split('|');
            for (int i = 0; i < locIDs.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(locIDs[i]))
                {
                    if (procLoc.Trim().Length > 0)
                        procLoc += ",";
                    procLoc += "'" + locIDs[i].Trim() + "'";
                }
            }
            return procLoc.Length > 0 ? " `locationCode` IN (" + procLoc + ") " : "";
        }
        /***********************************************************************************************/
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            if ( chkSort.Checked )
                this.gridMain.ExpandAllGroups();
//            LoadOnHand();
        }
        /***********************************************************************************************/
        private void xbtnSendEmailReport_Click(object sender, EventArgs e)
        {
            string to = "robbyxyzzy@gmail.com";
            string from = "robbyxyzzy@gmail.com";
            string subject = "Merchandise Orders Needed";
            string body = @"On-Hand Orders are needed.";
            MailMessage mail = new MailMessage(from, to, subject, body);

            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;

            smtp.EnableSsl = true;
            smtp.UseDefaultCredentials = false;
            smtp.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
            smtp.TargetName = "STARTTLS/smtp.gmail.com";
//            smtp.Credentials = new NetworkCredential("robbyxyzzy@gmail.com", "Xyzzy@0483");
            smtp.Send(mail);
        }
        /***********************************************************************************************/
        private DataTable getEmailUsers ()
        {
            DataTable dd = new DataTable();
            dd.Columns.Add("user");
            dd.Columns.Add("email");
            string cmd = "Select * from `preferenceUsers` where `module` = 'On-Hand Inventory' and `preferenceAnswer` = 'YES';";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                string user = dt.Rows[i]["userName"].ObjToString();
                cmd = "Select * from `users` where `userName` = '" + user + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count > 0)
                {
                    string email = dx.Rows[0]["email"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(email))
                    {
                        DataRow dRow = dd.NewRow();
                        dRow["user"] = user;
                        dRow["email"] = email;
                        dd.Rows.Add(dRow);
                    }
                }
            }
            return dd;
        }
        /***********************************************************************************************/
        private void btnSendEmailReport_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {

                if (!Directory.Exists(@"C:\RAG"))
                    Directory.CreateDirectory(@"C:\RAG");

                printPreviewOrExport(true);
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating PDF Email File!");
                this.Cursor = Cursors.Default;
                return;
            }

            DataTable dd = getEmailUsers();
            if ( dd.Rows.Count <= 0 )
            {
                MessageBox.Show("***ERROR*** No Email addresses are setup!");
                this.Cursor = Cursors.Default;
                return;
            }

            string from = "robbyxyzzy@gmail.com";
            string pw = "Xyzzy@0483";
            pw = "xkiypozlptspspwr";
            string option = "";
            string answer = "";

            DataTable ddd = G1.get_db_data("Select * from `options`;");
            for (int i = 0; i < ddd.Rows.Count; i++)
            {
                option = ddd.Rows[i]["option"].ObjToString();
                answer = ddd.Rows[i]["answer"].ObjToString();
                if (String.IsNullOrWhiteSpace(answer))
                    continue;
                if (option.Trim().ToUpper() == "ON-HAND EMAIL")
                    from = answer;
                else if (option.Trim().ToUpper() == "ON-HAND PW")
                    pw = answer;
            }

            string to = "robbyxyzzy@gmail.com";
            string subject = "Merchandise Orders Needed";
            string body = "On-Hand Orders are needed.";
            //to = "leland@colonialtel.com";
            //subject = "Test";
            //body = "Test";


            string senderID = from;
            string senderPassword = pw;
            if ( String.IsNullOrWhiteSpace ( from))
            {
                MessageBox.Show("***ERROR*** Email From Address is empty!");
                return;
            }
            if (String.IsNullOrWhiteSpace(pw))
            {
                MessageBox.Show("***ERROR*** Email PW is empty!");
                return;
            }
            RemoteCertificateValidationCallback orgCallback = ServicePointManager.ServerCertificateValidationCallback;
//            string body = "Test";
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnValidateCertificate);
                ServicePointManager.Expect100Continue = false;
                MailMessage mail = new MailMessage();

                for ( int i=0; i<dd.Rows.Count; i++)
                {
                    string email = dd.Rows[i]["email"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(email))
                        mail.To.Add(email);
                }
//                mail.To.Add(to);
                mail.From = new MailAddress(senderID);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                mail.Attachments.Add(new Attachment(@"C:\RAG\EMAILTEST.PDF"));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new System.Net.NetworkCredential(senderID, senderPassword);
                smtp.Send(mail);
                MessageBox.Show("Email Sent Successfully");
//                Console.WriteLine("Email Sent Successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Email Unsuccessful\n\n" + ex.Message.ToString());
                //                Console.WriteLine(ex.Message);
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = orgCallback;
                this.Cursor = Cursors.Default;
            }
        }
        private static bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        /***********************************************************************************************/
        private void btnDelete_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string cmd = "Select * from `inventory_on_hand` where `record` = '" + record + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string casketRecord = dt.Rows[0]["!casketRecord"].ObjToString();
            string homeRecord = dt.Rows[0]["!homeRecord"].ObjToString();

            cmd = "Select * from `inventorylist` where `record` = '" + casketRecord + "';";
            dt = G1.get_db_data(cmd);
            string casketDescription = "";
            if ( dt.Rows.Count > 0 )
                casketDescription = dt.Rows[0]["casketdesc"].ObjToString();

            cmd = "Select * from `funeralhomes` where `record` = '" + homeRecord + "';";
            dt = G1.get_db_data(cmd);
            string homeDescription = "";
            if (dt.Rows.Count > 0)
                homeDescription = dt.Rows[0]["name"].ObjToString();
            string str = "Are you sure you want to this On-Hand Minimum ";
            if (!String.IsNullOrWhiteSpace(casketDescription))
                str += "for\n" + casketDescription;
            if (!String.IsNullOrWhiteSpace(homeDescription))
                str += " from " + homeDescription;
            str += "?";

            DialogResult result = MessageBox.Show(str, "Remove Minimum On-Hand Inventory Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;
            G1.delete_db_table("inventory_on_hand", "record", record);
            LoadOnHand();
        }
        /***********************************************************************************************/
        private void chkSort_CheckedChanged(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            if (chkSort.Checked)
            {
                DataView tempview = dt.DefaultView;
//                tempview.Sort = "locationCode";
                tempview.Sort = "loc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

//                gridMain.Columns["locationCode"].GroupIndex = 0;
                gridMain.Columns["loc"].GroupIndex = 0;
                gridMain.OptionsView.ShowFooter = true;
                this.gridMain.ExpandAllGroups();
            }
            else
            {
                DataView tempview = dt.DefaultView;
//                tempview.Sort = "locationCode";
                tempview.Sort = "loc";
                dt = tempview.ToTable();
                G1.NumberDataTable(dt);
                dgv.DataSource = dt;

//                gridMain.Columns["locationCode"].GroupIndex = -1;
                gridMain.Columns["loc"].GroupIndex = -1;
                gridMain.OptionsView.ShowFooter = true;
                gridMain.CollapseAllGroups();
            }
            gridMain.RefreshData();
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private DataTable saveDt = null;
        private void ExpandScreen ()
        {
            int extra = 30;
            if (chkShowUsed.Checked || chkUnused.Checked )
            {
                saveDt = (DataTable)dgv.DataSource;
                gridBand3.Visible = true;
            }
            else
            {
                if ( saveDt != null)
                {
                    dgv.DataSource = saveDt;
                    saveDt = null;
                }
                gridBand3.Visible = false;
                extra = -30;
            }
            //            LoadOnHand();
            int top = this.panelBottomTop.Top;
            int left = this.panelBottomTop.Left;
            int width = this.panelBottomTop.Width;
            int height = this.panelBottomTop.Height + extra;

            this.panelBottomTop.SetBounds(left, top, width, height);
            this.panelBottomTop.Refresh();
            this.panelBottomBottom.Refresh();
            this.panelAll.Refresh();
            this.Refresh();
        }
        /***********************************************************************************************/
        private void chkUnused_CheckedChanged(object sender, EventArgs e)
        {
            if (chkShowUsed.Checked)
                return;
            ExpandScreen();
        }
        /***********************************************************************************************/
        private void chkShowUsed_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUnused.Checked)
                return;
            ExpandScreen();
        }
        /***********************************************************************************************/
        private void gridMain_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName.ToUpper() == "NUM")
            {
                if (e.RowHandle >= 0)
                {
                    string num = (e.RowHandle + 1).ToString();
                    e.DisplayText = num;
                }
            }
            else if (e.Column.FieldName.ToUpper() == "ORDERDATE")
            {
                if (e.RowHandle >= 0)
                {
                    string date = e.DisplayText;
                    DateTime d = date.ObjToDateTime();
                    if ( d.Year > 1950)
                        e.DisplayText = d.ToString("MM/dd/yyyy");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "NEEDED")
            {
                int count = e.DisplayText.ObjToInt32();
                if (count < 0)
                    e.Appearance.ForeColor = Color.Green;
                else if ( count == 0)
                    e.Appearance.ForeColor = Color.Black;
            }
        }
        /***********************************************************************************************/
        private void btnLeft_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker1.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(-1);
            this.dateTimePicker1.Value = date;
            //int days = DateTime.DaysInMonth(date.Year, date.Month);
            //date = new DateTime(date.Year, date.Month, days);
            //this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DateTime now = this.dateTimePicker2.Value;
            DateTime date = new DateTime(now.Year, now.Month, 1);
            date = date.AddMonths(1);
            this.dateTimePicker2.Value = date;
            //int days = DateTime.DaysInMonth(date.Year, date.Month);
            //date = new DateTime(date.Year, date.Month, days);
            //this.dateTimePicker2.Value = date;
        }
        /***********************************************************************************************/
        private void btnRun_Click(object sender, EventArgs e)
        {
            LoadOnHand();
        }
        /***********************************************************************************************/
        private void btnSaveOrders_Click(object sender, EventArgs e)
        {
            btnSaveOrders.Hide();
            ordersLoaded = false;
            string qty = "";
            string casketdesc = "";
            string casketcode = "";
            string location = "";
            string replacement = "";
            string orderdate = "";
            DateTime d = DateTime.Now;

            string cmd = "Delete from `inventory_orders` where `LocationCode` = '-1';";
            G1.get_db_data(cmd);

            string record = "";
            string user = LoginForm.username.Trim();
            DataTable dt = (DataTable)dgv.DataSource;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                if (dt.Rows[i]["qty"].ObjToDouble() > 0D)
                {
                    record = G1.create_record("inventory_orders", "LocationCode", "-1");
                    if (G1.BadRecord("inventory_orders", record))
                        continue;
                    qty = dt.Rows[i]["qty"].ObjToString();
                    casketdesc = dt.Rows[i]["casketdesc"].ObjToString();
                    casketcode = dt.Rows[i]["casketcode"].ObjToString();
                    location = dt.Rows[i]["locationCode"].ObjToString();
                    orderdate = dt.Rows[i]["orderdate"].ObjToString();
                    d = orderdate.ObjToDateTime();
                    orderdate = d.ToString("yyyy-MM-dd");
                    replacement = dt.Rows[i]["replacement"].ObjToString();
                    G1.update_db_table("inventory_orders", "record", record, new string[] { "qty", qty, "qtyPending", qty, "LocationCode", location, "CasketDescription", casketdesc, "CasketCode", casketcode});
                    G1.update_db_table("inventory_orders", "record", record, new string[] { "orderedby", user, "DateOrdered", orderdate, "replacement", replacement });
                }
            }
        }
        /***********************************************************************************************/
        private void LoadOrders ()
        {
            this.gridMain2.ExpandAllGroups();
            if (ordersLoaded)
                return;
            string locationCode = "";
            string casketDescription = "";
            string ownerShip = "";
            int actual = 0;
            string cmd = "Select * from `inventory_orders` where `del` <> '1' ORDER by `LocationCode`, `DateOrdered` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            if (G1.get_column_number(dt, "actualOnHand") <= 0)
                dt.Columns.Add("actualOnHand", Type.GetType("System.Int32"));
            dt.Columns.Add("mod");
            int qtyPending = 0;
            int qtyOrdered = 0;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                locationCode = dt.Rows[i]["LocationCode"].ObjToString();
                casketDescription = dt.Rows[i]["CasketDescription"].ObjToString();
                if (locationCode == "-1")
                    continue;
                if (String.IsNullOrWhiteSpace(locationCode))
                    continue;
                if (String.IsNullOrWhiteSpace(casketDescription))
                    continue;
                ownerShip = dt.Rows[i]["ownerShip"].ObjToString();
                actual = ImportInventoryList.GetActualOnHand(locationCode, casketDescription, ownerShip);
                dt.Rows[i]["actualOnHand"] = actual;
                qtyOrdered = dt.Rows[i]["qty"].ObjToInt32();
//                qtyPending = qtyOrdered - actual;
                //dt.Rows[i]["qtyPending"] = qtyPending;
            }
            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
            ordersLoaded = true;
        }
        /***********************************************************************************************/
        private void CheckPendingOrders(  DataTable dt )
        {
            string cmd = "Select * from `inventory_orders` where `del` <> '1' ORDER by `LocationCode`, `DateOrdered` DESC;";
            DataTable dx = G1.get_db_data(cmd);
            string casketdesc = "";
            string loc = "";
            int qtyPending = 0;
            int qtyOrdered = 0;
            int qty = 0;
            string dateOrdered = "";
            string replacement = "";
            DateTime date = DateTime.Now;
            if (G1.get_column_number(dt, "qtyOrdered") < 0)
                dt.Columns.Add("qtyOrdered", Type.GetType("System.Int32"));

            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                loc = dt.Rows[i]["loc"].ObjToString();
                casketdesc = dt.Rows[i]["casketdesc"].ObjToString();
                DataRow[] dRows = dx.Select("LocationCode='" + loc + "' AND CasketDescription='" + casketdesc + "'");
                if (dRows.Length > 0)
                {
                    qtyPending = 0;
                    qtyOrdered = 0;
                    dateOrdered = dt.Rows[i]["orderdate"].ObjToString();
                    replacement = dt.Rows[i]["replacement"].ObjToString();
                    for (int j = 0; j < dRows.Length; j++)
                    {
                        qty = dRows[j]["qtyPending"].ObjToInt32();
                        qtyPending += qty;
                        qty = dRows[j]["qty"].ObjToInt32();
                        qtyOrdered += qty;
                        date = dRows[j]["DateOrdered"].ObjToDateTime();
                        if (date.Year > 1900)
                        {
                            if (!dateOrdered.Contains(date.ToString("MM/dd/yyyy")))
                                dateOrdered += date.ToString("MM/dd/yyyy") + ",";
                        }
                        str = dRows[j]["replacement"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(str))
                        {
                            if (!replacement.Contains(str))
                                replacement += str + ",";
                        }
                    }
                    dateOrdered = dateOrdered.TrimEnd(',');
                    replacement = replacement.TrimEnd(',');
                    if (qtyPending <= 0)
                        qtyOrdered = 0;
                    dt.Rows[i]["qtyPending"] = qtyPending;
                    dt.Rows[i]["qtyOrdered"] = qtyOrdered;
                    dt.Rows[i]["orderdate"] = dateOrdered;
                    dt.Rows[i]["replacement"] = replacement;
                }
            }
        }
        /***********************************************************************************************/
        private bool ordersLoaded = false;
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABORDERS")
                LoadOrders();
        }
        /***********************************************************************************************/
        private void gridMain2_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);
            DataTable dt = (DataTable)dgv2.DataSource;
            dt.Rows[row]["mod"] = "Y";
            pendingModified = true;
            this.panelOrdersTop.Visible = true;
            if (e.Column.FieldName.Trim().ToUpper() == "QTY")
            {
                double OnHand = dr["actualOnHand"].ObjToDouble();
                double qty = dr["qty"].ObjToDouble();
                double pending = dr["qtyPending"].ObjToDouble();
                //                double value = qty - OnHand;
                //double value = value + qty;
                //dr["qtyPending"] = value.ObjToString();
            }
        }
        /***********************************************************************************************/
        private void deleteOrderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);
            string record = dr["record"].ObjToString();
            DataTable dt = (DataTable)dgv2.DataSource;

            DialogResult result = MessageBox.Show("Are you sure you want to DELETE this order?", "Delete Order Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;

            G1.delete_db_table("inventory_orders", "record", record);
            ordersLoaded = false;
            LoadOrders();
        }
        /***********************************************************************************************/
        private void btnSummarize_Click(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void doSummary()
        {
            dgv3.Visible = false;
            DataTable dt = (DataTable) dgv.DataSource;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "casketDesc ASC";
            dt = tempview.ToTable();

            gridMain2.Columns["loc"].GroupIndex = -1;
//            gridMain.OptionsView.ShowFooter = true;
            gridMain2.CollapseAllGroups();

            DataTable dx = dt.Clone();
            string saveDesc = "";
            string desc = "";
            int totalMinOnHand = 0;
            int totalActualOnHand = 0;
            int totalNeeded = 0;
            int totalQty = 0;
            int totalQtyPending = 0;
            int totalConsigned = 0;
            int minimumOnHand = 0;
            int actualOnHand = 0;
            int consigned = 0;
            int needed = 0;
            int qty = 0;
            int qtyPending = 0;
            int row = -1;
            bool gotSome = false;
            string ownership = "";
            string cmd = "";
            DataTable dd = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                consigned = 0;
                desc = dt.Rows[i]["casketDesc"].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(saveDesc))
                    saveDesc = desc;
                ownership = dt.Rows[i]["ownership"].ObjToString();
                //if (ownership.Trim().ToUpper() == "CONSIGNED")
                //    consigned++;
                minimumOnHand = dt.Rows[i]["minimumOnHand"].ObjToInt32();
                consigned = minimumOnHand;
                actualOnHand = dt.Rows[i]["actualOnHand"].ObjToInt32();
                needed = dt.Rows[i]["Needed"].ObjToInt32();
                qty = dt.Rows[i]["qty"].ObjToInt32();
                qtyPending = dt.Rows[i]["qtyPending"].ObjToInt32();
                if ( desc != saveDesc)
                {
                    cmd = "SELECT * FROM `inventory` WHERE casketDescription = '" + saveDesc + "' AND ServiceID = '';";
                    dd = G1.get_db_data(cmd);
                    totalActualOnHand = dd.Rows.Count;
                    if (saveDesc.Trim().ToUpper() == "N01 8K3 CDH SUTTER")
                    {

                    }
                    row = i;
                    dt.Rows[i-1]["minimumOnHand"] = totalConsigned;
                    dt.Rows[i-1]["actualOnHand"] = totalActualOnHand;
                    dt.Rows[i-1]["Needed"] = totalNeeded;
                    dt.Rows[i - 1]["qty"] = totalQty;
                    dt.Rows[i - 1]["qtyPending"] = totalQtyPending;
                    dx.ImportRow(dt.Rows[i-1]);
                    totalMinOnHand = 0;
                    totalActualOnHand = 0;
                    totalNeeded = 0;
                    totalConsigned = 0;
                    totalQty = 0;
                    totalQtyPending = 0;
                    saveDesc = desc;
                }
                gotSome = true;
                totalMinOnHand += minimumOnHand;
                totalActualOnHand += actualOnHand;
                totalConsigned += consigned;
                totalNeeded += needed;
                totalQty += qty;
                totalQtyPending = qtyPending;
            }
            if ( gotSome && row >= 0 )
            {
                dt.Rows[row]["minimumOnHand"] = totalConsigned;
                dt.Rows[row]["actualOnHand"] = totalActualOnHand;
                dt.Rows[row]["Needed"] = totalNeeded;
                dt.Rows[row]["qty"] = totalQty;
                dt.Rows[row]["qtyPending"] = totalQtyPending;
                dx.ImportRow(dt.Rows[row]);
            }
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                totalConsigned = dx.Rows[i]["minimumOnHand"].ObjToInt32();
                totalActualOnHand = dx.Rows[i]["actualOnHand"].ObjToInt32();
                //                needed = totalActualOnHand - totalConsigned;
                needed = totalConsigned - totalActualOnHand;
                dx.Rows[i]["Needed"] = needed;
            }
            gridMain3.Columns["locationCode"].Visible = false;
            gridMain3.Columns["accountcode"].Visible = false;
            gridMain3.Columns["itemnumber"].Visible = false;
            gridMain3.Columns["replacement"].Visible = false;
            gridMain3.Columns["ServiceID"].Visible = false;
            gridMain3.Columns["SerialNumber"].Visible = false;
            gridMain3.Columns["DateUsed"].Visible = false;
            gridMain3.Columns["orderdate"].Visible = false;
            gridMain3.Columns["casketcode"].Visible = false;
            gridMain3.Columns["qty"].Visible = false;
            gridMain3.Columns["qtyPending"].Visible = false;
            gridBand10.Visible = false;
            gridBand11.Visible = false;
            //gridBand4.Visible = false;
            G1.NumberDataTable(dx);
            dgv3.DataSource = dx;
            dgv3.Dock = DockStyle.Fill;
            dgv3.Visible = true;
            G1.SetColumnPosition(gridMain3, "num", 0);
            G1.SetColumnPosition(gridMain3, "casketdesc", 1);
            G1.SetColumnPosition(gridMain3, "minimumOnHand", 2);
            G1.SetColumnPosition(gridMain3, "actualOnHand", 3);
            G1.SetColumnPosition(gridMain3, "Needed", 4);
//            gridMain.Columns["actualOnHand"].Caption = "Consigned";
            gridMain3.Columns["minimumOnHand"].Caption = "Consigned";
        }
        /***********************************************************************************************/
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadOnHand();
        }
        /***************************************************************************************/
        public bool FireEventInventoryImported()
        {
            if ( addHomeFormNew == null)
                return false;
            LoadOnHand();
            return true;
        }
        /***********************************************************************************************/
        private void btnSavePending_Click(object sender, EventArgs e)
        { // Save Pending Information
            DataTable dt = (DataTable)dgv2.DataSource;
            string mod = "";
            string record = "";
            string qty_pending = "";
            string qty = "";
            string replacement = "";
            string update = "";
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                if (String.IsNullOrWhiteSpace(mod))
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( record))
                {
                    qty_pending = dt.Rows[i]["qtyPending"].ObjToString();
                    qty = dt.Rows[i]["qty"].ObjToString();
                    replacement = dt.Rows[i]["replacement"].ObjToString();
                    update = "qtyPending," + qty_pending;
                    update += ",qty," + qty;
                    update += ",replacement," + replacement;
                    G1.update_db_table("inventory_orders", "record", record, update);
                }
            }
            pendingModified = false;
            this.panelOrdersTop.Visible = false;
        }
        /***********************************************************************************************/
        private void chkSummarize_CheckedChanged(object sender, EventArgs e)
        {
            if ( chkSummarize.Checked )
                doSummary();
            else
            {
                dgv3.Visible = false;
                dgv.Visible = true;
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
            else if (e.Column.FieldName.ToUpper() == "ORDERDATE")
            {
                if (e.RowHandle >= 0)
                {
                    string date = e.DisplayText;
                    DateTime d = date.ObjToDateTime();
                    if (d.Year > 1950)
                        e.DisplayText = d.ToString("MM/dd/yyyy");
                }
            }
            else if (e.Column.FieldName.ToUpper() == "NEEDED")
            {
                int count = e.DisplayText.ObjToInt32();
                if (count < 0)
                    e.Appearance.ForeColor = Color.Green;
                else if (count == 0)
                    e.Appearance.ForeColor = Color.Black;
            }
        }
        /***********************************************************************************************/
        private int footerCount = 0;
        private void gridMain3_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /***********************************************************************************************/
        private void gridMain3_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    //if (chkSort.Checked)
                    //    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /***********************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 1)
                {
                    footerCount = 0;
                    if (chkSort.Checked)
                        e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /***********************************************************************************************/
        private void btnShowPrices_Click(object sender, EventArgs e)
        {
            PriceLists priceForm = new PriceLists("Current", workFunRec);
            priceForm.Show();
        }
        /***********************************************************************************************/
        private void selectBanksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string cmd = "Select * from `funeralhomes` where `record` = '" + workFunRec + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string workHome = dt.Rows[0]["LocationCode"].ObjToString();
            SelectFuneralHomeBanks bankForm = new SelectFuneralHomeBanks(workHome, workFunRec);
            bankForm.Show();
        }
        /***********************************************************************************************/
        private void btnShowOrders_Click(object sender, EventArgs e)
        {
            Orders ordersForm = new Orders(workFunRec, workNeeded);
            ordersForm.Show();
        }
        /***********************************************************************************************/
    }
}
