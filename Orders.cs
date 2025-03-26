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
    public partial class Orders : Form
    {
        private string workFunRec = "";
        private string workName = "";
        private bool workNeeded = false;
        private bool modified = false;
        private bool loading = true;
        private DataTable originalDt = null;
        private bool pendingModified = false;
        private Color SaveBackColor = SystemColors.Control;
        private Color NewBackColor = Color.LightGreen;
        /***********************************************************************************************/
        public Orders(string record = "", bool needed = false)
        {
            int w = this.Width;

            InitializeComponent();

            if (!String.IsNullOrWhiteSpace(record))
            {
                if (!G1.validate_numeric(record))
                {
                    chkComboLocation.EditValue = record;
                    chkComboLocation.Text = record;
                }
                else
                    workFunRec = record;
            }

            workNeeded = needed;
            int width = this.Width;
            width = this.Width + 100;
            int height = this.Height;
            this.SetBounds(this.Left, this.Top, width, height);

            SetupTotalsSummary();

            SetupRemoveOrders();
            SetupCheckOrders();

            chkShowUsed.Hide();
            chkUnused.Hide();
            chkSummarize.Hide();
            btnSendEmailReport.Hide();
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
        private void SetupCheckOrders()
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit2;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "";
            selectnew.ValueGrayed = "x";
        }
        /***********************************************************************************************/
        private void SetupRemoveOrders()
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew1 = this.repositoryItemCheckEdit4;
            selectnew1.NullText = "";
            selectnew1.ValueChecked = "1";
            selectnew1.ValueUnchecked = "";
            selectnew1.ValueGrayed = "x";
        }
        /***********************************************************************************************/
        private void Orders_Load(object sender, EventArgs e)
        {
            //SaveBackColor = btnSave.BackColor;
            dgv3.Visible = false;
            pendingModified = false;
            this.gridMain2.ExpandAllGroups();
            btnSaveOrders.Visible = false;
            gridBand3.Visible = false;
            picLoader.Hide();
            //getLocations();
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
                    try
                    {
                        keyCode = dt.Rows[0]["keycode"].ObjToString();
                        name = dt.Rows[0]["name"].ObjToString();
                        this.Text = this.Text + " for " + name;
                        workName = dt.Rows[0]["LocationCode"].ObjToString();
                    }
                    catch ( Exception ex )
                    {
                        MessageBox.Show("***ERROR*** Problem looking up funeral Home Record=" + workFunRec + " KeyCode=" + keyCode + " Name=" + name + " State=" + state);
                    }
                }
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
            //else 
            //{
            //    lblLocation.Hide();
            //    chkComboLocation.Hide();
            //}
            //            btnSendEmailReport.Hide();

            getLocations();

            LoadOnHand();

            modified = false;
            dgv.Dock = DockStyle.Fill;
            dgv.Visible = true;
            this.WindowState = FormWindowState.Maximized;
            if (SMFS.SMFS_MainForm != null)
                SMFS.SMFS_MainForm.WindowState = FormWindowState.Minimized;
            btnShowBottom.Hide();
            btnShowMiddle.Hide();
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
            //if (!workNeeded)
            //    cmd += " WHERE `!homeRecord` = '" + workFunRec + "' ";

            cmd += ";";
            DataTable dt = G1.get_db_data(cmd);

            DataRow [] dRows = dt.Select ("casketdesc='6LN 840 HD COLONIAL PINE'");
            if ( dRows.Length > 0 )
            {
                DataTable ddx = dRows.CopyToDataTable();
            }
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
            dRows = dt.Select("casketdesc='6LN 840 H COLONIAL PINE'");
            if (dRows.Length > 0)
            {
                DataTable ddx = dRows.CopyToDataTable();
            }

            G1.NumberDataTable(dt);
            CheckPendingOrders( dt );

            dRows = dt.Select("casketdesc='6LN 840 H COLONIAL PINE'");
            if (dRows.Length > 0)
            {
                DataTable ddx = dRows.CopyToDataTable();
            }

            for (int i = 0; i < dt.Rows.Count; i++)
                dt.Rows[i]["Ownership"] = "Consigned";

            originalDt = dt;
            dgv.DataSource = dt;
            if (chkSort.Checked)
                this.gridMain.ExpandAllGroups();

            LoadOrders();

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

            DataRow[] dRows = null;

            cmd = "Select * from `inventorylist`;";
            DataTable invDt = G1.get_db_data(cmd);

            cmd = "Select * from `funeralhomes` where `record` = '" + workFunRec + "' order by `keycode`;";
            //if (workNeeded)
            //{
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
            //}
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

                            dRows = invDt.Select("record='" + casketRecord + "'");
                            if (dRows.Length <= 0)
                                continue;

                            //cmd = "Select * from `inventorylist` where `record` = '" + casketRecord + "';";
                            //dd = G1.get_db_data(cmd);
                            //if (dd.Rows.Count <= 0)
                            //    continue;

                            casketdescription = dRows[0]["casketdesc"].ObjToString();
                            casketcode = dRows[0]["casketcode"].ObjToString();
                            itemnumber = dRows[0]["itemnumber"].ObjToString();

                            //onhand = ImportInventoryList.GetActualOnHand(locationCode, casketdescription, "Consigned");
                            onhand = ImportInventoryList.GetActualOnHand(locationCode, casketdescription, "!Rental");
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
                            //if ( chkShowUsed.Checked )
                            //{
                            //    DateTime date = dateTimePicker1.Value;
                            //    string date1 = G1.DateTimeToSQLDateTime(date);
                            //    DateTime ddate = dateTimePicker2.Value;
                            //    string date2 = G1.DateTimeToSQLDateTime(ddate);

                            //    cmd = "Select * from `inventory` where `LocationCode` = '" + locationCode + "' and `casketdescription` = '" + casketdescription + "' and `ServiceID` <> '' ";
                            //    cmd += " and `DateUsed` >= '" + date1 + "' ";
                            //    cmd += " and `DateUsed` <= '" + date2 + "' ";
                            //    cmd += " and `del` <> '1' ";
                            //    cmd += ";";
                            //    dd = G1.get_db_data(cmd);
                            //    for ( int j=0; j<dd.Rows.Count; j++)
                            //    {
                            //        if (added)
                            //        {
                            //            int row = dt.Rows.Count - 1;
                            //            dt.Rows[row]["SerialNumber"] = dd.Rows[j]["SerialNumber"].ObjToString();
                            //            dt.Rows[row]["ServiceID"] = dd.Rows[j]["ServiceID"].ObjToString();
                            //            dt.Rows[row]["DateUsed"] = dd.Rows[j]["DateUsed"].ObjToString();
                            //            dt.Rows[row]["casketdesc"] = dd.Rows[j]["CasketDescription"].ObjToString();
                            //            added = false;
                            //        }
                            //        else
                            //        {
                            //            DataRow dRow = dt.NewRow();
                            //            dRow["SerialNumber"] = dd.Rows[j]["SerialNumber"].ObjToString();
                            //            dRow["ServiceID"] = dd.Rows[j]["ServiceID"].ObjToString();
                            //            dRow["DateUsed"] = dd.Rows[j]["DateUsed"].ObjToString();
                            //            dRow["casketdesc"] = dd.Rows[j]["CasketDescription"].ObjToString();
                            //            dRow["locationCode"] = locationCode;
                            //            dRow["loc"] = locationCode;
                            //            dRow["accountcode"] = accountcode;
                            //            dRow["Order"] = order++;
                            //            dt.Rows.Add(dRow);
                            //        }
                            //    }
                            //}
                        }
                    }
                }
            }
            if ( chkShowUsed.Checked )
            {
                DataView tempview = dt.DefaultView;
                tempview.Sort = "Order DESC";
                dt = tempview.ToTable();

                //if (chkAllOther.Checked)
                //    ShowAllOther(dt, dx, order);
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
        private void AddHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            //if (modified || pendingModified )
            //{
            //    DialogResult result = MessageBox.Show("Are you sure you want to exit without saving your changes?", "Funeral Home Order Entry Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            //    if (result == DialogResult.No)
            //    {
            //        e.Cancel = true;
            //        return;
            //    }
            //}
            if (SMFS.SMFS_MainForm != null)
                SMFS.SMFS_MainForm.WindowState = FormWindowState.Normal;
        }
        /***********************************************************************************************/
        private void btnAdd_Click(object sender, EventArgs e)
        {
            string str= chkComboLocation.Text.Trim();

            if ( String.IsNullOrWhiteSpace ( str ))
            {
                MessageBox.Show("***ERROR*** A location must be selected to add new on-hand setup", "On-Hand Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string [] Lines = str.Split('|');
            if ( Lines.Length <= 0 )
            {
                MessageBox.Show("***ERROR*** A location must be selected to add new on-hand setup", "On-Hand Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            string comboLocation = Lines[0].Trim();

            string question = "Are you ADDING a new On-Hand Item for Location (" + comboLocation + ") ?";
            DialogResult result = MessageBox.Show(question, "Add On-Hand Item Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            if (result == DialogResult.No)
            {
                MessageBox.Show("***INFO*** Okay, Nothing Added!", "Add On-Hand Item Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                this.Cursor = Cursors.Default;
                return;
            }


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

            string str = chkComboLocation.Text.Trim();

            if (String.IsNullOrWhiteSpace(str))
                return;

            string[] Lines = str.Split('|');
            if (Lines.Length <= 0)
                return;

            string comboLocation = Lines[0].Trim();

            string cmd = "Select * from `funeralhomes` where `LocationCode` = '" + comboLocation + "';";
            DataTable funDt = G1.get_db_data(cmd);
            if (funDt.Rows.Count <= 0)
                return;

            string funRec = funDt.Rows[0]["record"].ObjToString();

            cmd = "Select * from `inventorylist` where `record` = '" + merchandiseRecord + "';";
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
                    MessageBox.Show("***ERROR*** Location/Casket already exists in On-Hand Database!", "Add On - Hand Item Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }
            }

            string newrecord = G1.create_record("inventory_on_hand", "minimumOnHand", "-1");
            if (String.IsNullOrWhiteSpace(newrecord) || newrecord == "-1")
            {
                MessageBox.Show("***ERROR*** Creating New Record in On-Hand Database!", "Add On-Hand Item Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }

            G1.update_db_table("inventory_on_hand", "record", newrecord, new string[] { "!casketRecord", merchandiseRecord, "!homeRecord", funRec, "minimumOnHand", "1" });
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
            {
                btnSaveOrders.Visible = false;
            }
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
                    G1.update_db_table("inventory_on_hand", "record", record, new string[] { "minimumOnHand", value }); // SAVE MINIMUM ON HAND
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
            if ( printOrders )
                printableComponentLink1.Component = dgv2;
            if ( printOnOrders )
                printableComponentLink1.Component = dgv4;

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
            //if (dgv2.Visible)
            //    printableComponentLink1.Component = dgv2;
            //if (dgv3.Visible)
            //    printableComponentLink1.Component = dgv3;

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
                search = "Date : ";// + this.dateTimePicker1.Value.ToString("MM/dd/yyyy") + " - " + this.dateTimePicker2.Value.ToString("MM/dd/yyyy");
                Printer.DrawQuad(2, 10, 6, 2, search, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Center);
            }

            font = new Font("Ariel", 10, FontStyle.Italic);
            if ( printOnOrders )
                Printer.DrawQuad(6, 9, 6, 3, "Inventory On-Order List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else if ( printOrders )
                Printer.DrawQuad(6, 9, 6, 3, "Inventory Needs Ordering List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);
            else
                Printer.DrawQuad(6, 9, 6, 3, "On-Hand Inventory List", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
//            Printer.DrawQuadBorderRight(6, 5, 1, 12, BorderSide.Right, 1, Color.Black);
            Printer.DrawQuadBorderRight(12, 6, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain);
        }
        /***********************************************************************************************/
        private DataTable _LocationList;
        private void getLocations()
        {
            string cmd = "SELECT `LocationCode` FROM `inventory` GROUP BY `LocationCode` ASC;";
            _LocationList = G1.get_db_data(cmd);

            string str = "";

            for (int i = _LocationList.Rows.Count - 1; i >= 0; i--)
            {
                str = _LocationList.Rows[i]["LocationCode"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    _LocationList.Rows.RemoveAt(i);
            }

            chkComboLocation.Properties.DataSource = _LocationList;
            if (!String.IsNullOrWhiteSpace(workName))
            {
                chkComboLocation.EditValue = workName;
                chkComboLocation.Text = workName;
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
        private void chkComboLocation_EditValueChangedxxx(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;
            string names = getLocationNameQuery();
            DataRow[] dRows = originalDt.Select(names);
            DataTable dt = originalDt.Clone();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            if ( chkSort.Checked )
                this.gridMain.ExpandAllGroups();
            LoadOrders();
//            LoadOnHand();
        }
        /***********************************************************************************************/
        private int mainCount = 0;
        private void chkComboLocation_EditValueChanged(object sender, EventArgs e)
        {
            if (originalDt == null)
                return;

            mainCount++;
            //if (mainCount < 3)
            //    return;

            DataTable dt = (DataTable)dgv.DataSource;

            dt = CheckNeedOrders(dt);

            G1.NumberDataTable(dt);
            CheckPendingOrders(dt);
            originalDt = dt;
            dgv.DataSource = dt;
            if (chkSort.Checked)
                this.gridMain.ExpandAllGroups();

            LoadOrders();

            if (chkSort.Checked)
                this.gridMain.ExpandAllGroups();

            this.Cursor = Cursors.Default;





            //string names = getLocationNameQuery();
            //DataRow[] dRows = originalDt.Select(names);
            //DataTable dt = originalDt.Clone();
            //for (int i = 0; i < dRows.Length; i++)
            //    dt.ImportRow(dRows[i]);
            //G1.NumberDataTable(dt);
            //dgv.DataSource = dt;
            //if (chkSort.Checked)
            //    this.gridMain.ExpandAllGroups();
            //LoadOrders();
            ////            LoadOnHand();
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
            //int top = this.panelBottomTop.Top;
            //int left = this.panelBottomTop.Left;
            //int width = this.panelBottomTop.Width;
            //int height = this.panelBottomTop.Height + extra;

            //this.panelBottomTop.SetBounds(left, top, width, height);
            //this.panelBottomTop.Refresh();
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
            else if (e.Column.FieldName.ToUpper() == "QTYORDERED")
            {
                int count = e.DisplayText.ObjToInt32();
                if (count <= 0)
                    e.DisplayText = "";
            }
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
        private void SaveOrder(DataTable dt, int i, string record)
        {
            string qty = "";
            string casketdesc = "";
            string casketcode = "";
            string location = "";
            string replacement = "";
            string orderdate = "";
            DateTime d = DateTime.Now;

            string cmd = "Delete from `inventory_orders` where `LocationCode` = '-1';";
            G1.get_db_data(cmd);

            try
            {
                string user = LoginForm.username.Trim();
                if (dt.Rows[i]["qty"].ObjToDouble() > 0D)
                {
                    if (record == "-1")
                        record = G1.create_record("inventory_orders", "LocationCode", "-1");
                    if (G1.BadRecord("inventory_orders", record))
                        return;
                    qty = dt.Rows[i]["qty"].ObjToString();
                    casketdesc = dt.Rows[i]["CasketDescription"].ObjToString();
                    casketcode = dt.Rows[i]["CasketCode"].ObjToString();
                    location = dt.Rows[i]["LocationCode"].ObjToString();
                    orderdate = dt.Rows[i]["DateOrdered"].ObjToString();
                    d = orderdate.ObjToDateTime();
                    if (d.Year <= 100)
                        d = DateTime.Now;
                    orderdate = d.ToString("yyyy-MM-dd");
                    dt.Rows[i]["DateOrdered"] = G1.DTtoMySQLDT(d);
                    dt.Rows[i]["orderedby"] = user;
                    dt.Rows[i]["record"] = record;
                    replacement = dt.Rows[i]["replacement"].ObjToString();
                    G1.update_db_table("inventory_orders", "record", record, new string[] { "qty", qty, "qtyPending", qty, "LocationCode", location, "CasketDescription", casketdesc, "CasketCode", casketcode });
                    G1.update_db_table("inventory_orders", "record", record, new string[] { "orderedby", user, "DateOrdered", orderdate, "replacement", replacement });
                }
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void LoadOrders ()
        {
            this.gridMain2.ExpandAllGroups(); // Ramma Zamma
            this.gridMain4.ExpandAllGroups();
            //if (ordersLoaded)
            //    return;
            DataTable dddx = null;
            DataTable dddy = null;

            string locationCode = "";
            string casketDescription = "";
            string casketCode = "";
            string ownerShip = "";
            int actual = 0;
            string cmd = "Select * from `inventory_orders` where `del` <> '1' ";
            string locations = getLocationQuery();
            if (!String.IsNullOrWhiteSpace(locations))
            {
                printLocation = chkComboLocation.Text;
                cmd += " AND " + locations;
            }
            cmd += " ORDER by `LocationCode`, `DateOrdered` DESC; ";
            DataTable dt2 = G1.get_db_data(cmd);

            if (G1.get_column_number(dt2, "actualOnHand") <= 0)
                dt2.Columns.Add("actualOnHand", Type.GetType("System.Int32"));

            if (G1.get_column_number(dt2, "needed") <= 0)
                dt2.Columns.Add("needed", Type.GetType("System.Int32"));

            if (G1.get_column_number(dt2, "orderThis") <= 0)
                dt2.Columns.Add("orderThis" );

            if (G1.get_column_number(dt2, "removeThis") <= 0)
                dt2.Columns.Add("removeThis");

            if (G1.get_column_number(dt2, "accountcode") <= 0)
                dt2.Columns.Add("accountcode");

            if (G1.get_column_number(dt2, "itemnumber") <= 0)
                dt2.Columns.Add("itemnumber");

            if (G1.get_column_number(dt2, "removeThis") <= 0)
                dt2.Columns.Add("removeThis");
            
            dt2.Columns.Add("mod");

            int qtyPending = 0;
            int qtyOrdered = 0;
            int needed = 0;
            int minimum = 0;

            DataRow dR = null;
            DataRow[] dRows = null;
            DataRow[] xRows = null;

            string serviceId = "";

            DataTable dt = (DataTable)dgv.DataSource;

            for ( int i=0; i<dt2.Rows.Count; i++)
            {
                dt2.Rows[i]["removeThis"] = "";
                dt2.Rows[i]["orderThis"] = "";
                locationCode = dt2.Rows[i]["LocationCode"].ObjToString();
                casketDescription = dt2.Rows[i]["CasketDescription"].ObjToString();
                actual = ImportInventoryList.GetActualOnHand(locationCode, casketDescription, "");
                dt2.Rows[i]["actualOnHand"] = actual;

                dRows = dt.Select("LocationCode='" + locationCode + "' AND casketdesc='" + casketDescription + "'");
                if ( dRows.Length > 0 )
                {
                    minimum = dRows[0]["minimumOnHand"].ObjToInt32();
                    needed = minimum - actual;
                    dt2.Rows[i]["needed"] = needed;
                }
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                locationCode = dt.Rows[i]["LocationCode"].ObjToString();
                casketDescription = dt.Rows[i]["casketdesc"].ObjToString();
                if (locationCode == "-1")
                    continue;
                if (String.IsNullOrWhiteSpace(locationCode))
                    continue;
                if (String.IsNullOrWhiteSpace(casketDescription))
                    continue;
                if ( casketDescription == "A40 818 ND Primrose")
                {
                }
                ownerShip = dt.Rows[i]["ownerShip"].ObjToString();
                casketCode = dt.Rows[i]["casketCode"].ObjToString();
                needed = dt.Rows[i]["Needed"].ObjToInt32();
                if ( needed > 0 )
                {
                    dRows = dt2.Select("CasketDescription='" + casketDescription + "' AND LocationCode='" + locationCode + "' AND matched <> 'MATCHED'");
                    if ( dRows.Length < needed )
                    {
                        for (int j = 0; j < (needed - dRows.Length); j++)
                        {
                            dR = dt2.NewRow();
                            actual = ImportInventoryList.GetActualOnHand(locationCode, casketDescription, ownerShip);
                            dR["actualOnHand"] = actual;
                            dR["qty"] = 0;
                            dR["CasketDescription"] = casketDescription;
                            dR["LocationCode"] = locationCode;
                            dR["casketCode"] = casketCode;
                            dR["ownerShip"] = ownerShip;
                            dR["orderThis"] = "";
                            dR["removeThis"] = "";
                            dR["needed"] = 1;
                            dR["record"] = -1;

                            if ( casketDescription == "Triton Grey")
                            {
                            }

                            cmd = "Select * from `fcust_services` f JOIN `fcontracts` c ON f.`contractNumber` = c.`contractNumber` WHERE `service` = '" + casketDescription + "' AND `serialNumber` <> '' ORDER BY f.`tmstamp` DESC LIMIT 100;";
                            dddx = G1.get_db_data(cmd);

                            if (dddx.Rows.Count <= 0)
                            {
                                cmd = "Select * from `fcust_services` f JOIN `fcontracts` c ON f.`contractNumber` = c.`contractNumber` WHERE `service` LIKE '%" + casketDescription + "%' AND `serialNumber` <> '' ORDER BY f.`tmstamp` DESC LIMIT 100;";
                                dddx = G1.get_db_data(cmd);
                            }

                            for ( int k=0; k<dddx.Rows.Count; k++)
                            {
                                serviceId = dddx.Rows[k]["serviceId"].ObjToString();
                                if (!String.IsNullOrWhiteSpace(serviceId))
                                {
                                    cmd = "Select * from `inventory_orders` WHERE `CasketDescription` = '" + casketDescription + "' AND `replacement` = '" + serviceId + "';"; // If Service Id has ever been used as the PO of an order don't use it again.
                                    dddy = G1.get_db_data(cmd);
                                    if (dddy.Rows.Count <= 0)
                                    {
                                        xRows = dt2.Select("replacement='" + serviceId + "'");
                                        if (xRows.Length <= 0)
                                        {
                                            dR["replacement"] = serviceId;
                                            break;
                                        }
                                    }
                                }
                            }

                            dt2.Rows.Add(dR);
                        }
                    }
                }
            }

            for (int i = 0; i < dt2.Rows.Count; i++)
                dt2.Rows[i]["removeThis"] = "";

            LoadupOtherInfo(dt2);

            G1.NumberDataTable(dt2);
            dgv2.DataSource = dt2;
            dgv4.DataSource = dt2;
            ordersLoaded = true;
        }
        /***********************************************************************************************/
        private void LoadupOtherInfo ( DataTable dt2 )
        {
            DataTable dt = (DataTable)dgv.DataSource;

            DataTable funDt = G1.get_db_data("Select * from `funeralhomes`;");

            string casketDesc = "";
            DataRow[] dRows = null;

            string accountCode = "";
            string itemNumber = "";
            string ownership = "";
            string LocationCode = "";
            string homeRecord = "";

            for ( int i = 0; i < dt2.Rows.Count; i++)
            {
                casketDesc = dt2.Rows[i]["CasketDescription"].ObjToString();
                LocationCode = dt2.Rows[i]["LocationCode"].ObjToString();
                dRows = funDt.Select("LocationCode='" + LocationCode + "'");
                if (dRows.Length > 0)
                {
                    homeRecord = dRows[0]["record"].ObjToString();
                    dRows = dt.Select("casketdesc='" + casketDesc + "' AND !homeRecord='" + homeRecord + "'");
                    if (dRows.Length > 0)
                    {
                        accountCode = dRows[0]["accountcode"].ObjToString();
                        itemNumber = dRows[0]["itemnumber"].ObjToString();
                        dt2.Rows[i]["accountcode"] = accountCode;
                        dt2.Rows[i]["itemnumber"] = itemNumber;
                        ownership = dt2.Rows[i]["Ownership"].ObjToString();
                        if (String.IsNullOrWhiteSpace(ownership))
                            dt2.Rows[i]["Ownership"] = "Consigned";
                    }
                }
            }
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
            string matched = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                loc = dt.Rows[i]["loc"].ObjToString();
                casketdesc = dt.Rows[i]["casketdesc"].ObjToString();
                DataRow[] dRows = dx.Select("LocationCode='" + loc + "' AND CasketDescription='" + casketdesc + "'");
                if (dRows.Length > 0)
                {
                    DataTable ddx = dRows.CopyToDataTable();
                    qtyPending = 0;
                    qtyOrdered = 0;
                    dateOrdered = dt.Rows[i]["orderdate"].ObjToString();
                    replacement = dt.Rows[i]["replacement"].ObjToString();
                    for (int j = 0; j < dRows.Length; j++)
                    {
                        matched = dRows[j]["matched"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(matched))
                            continue;
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
            if (dr == null)
                return;
            int rowhandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowhandle);
            DataTable dt = (DataTable)dgv2.DataSource;
            dt.Rows[row]["mod"] = "Y";
            pendingModified = true;

            string user = LoginForm.username.Trim();
            string qty = "0";
            DateTime d = DateTime.Now;
            string orderdate = d.ToString("yyyy-MM-dd");
            string replacement = dt.Rows[row]["replacement"].ObjToString();
            string location = dt.Rows[row]["LocationCode"].ObjToString();
            string casketDesc = dt.Rows[row]["CasketDescription"].ObjToString();
            string casketcode = dt.Rows[row]["CasketCode"].ObjToString();
            int pending = dt.Rows[row]["qtyPending"].ObjToInt32();

            string record = dt.Rows[row]["record"].ObjToString();
            if (String.IsNullOrWhiteSpace(record))
                record = "-1";
            if (record == "0" || record == "-1")
                record = G1.create_record("inventory_orders", "LocationCode", "-1");
            if (G1.BadRecord("inventory_orders", record))
                return;

            G1.update_db_table("inventory_orders", "record", record, new string[] { "qty", qty, "qtyPending", pending.ToString(), "LocationCode", location, "CasketDescription", casketDesc, "CasketCode", casketcode });
            G1.update_db_table("inventory_orders", "record", record, new string[] { "orderedby", user, "DateOrdered", orderdate, "replacement", replacement });
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
        private void btnHidePanelBottom_Click(object sender, EventArgs e)
        {
            this.panelBottom.Hide();
            this.panelMiddle.Dock = DockStyle.Fill;
            btnShowBottom.Show();
            btnShowBottom.Refresh();

            btnHidePanelMiddle.Hide();
            btnHidePanelMiddle.Refresh();
        }
        /***********************************************************************************************/
        private void btnShowBottom_Click(object sender, EventArgs e)
        {
            this.panelMiddle.Dock = DockStyle.Top;
            this.panelBottom.Show();
            btnShowBottom.Hide();
            btnShowBottom.Refresh();

            btnHidePanelMiddle.Show();
            btnHidePanelMiddle.Refresh();
        }
        /***********************************************************************************************/
        private void btnHidePanelMiddle_Click(object sender, EventArgs e)
        {
            this.panelMiddle.Hide();
            this.panelBottom.Dock = DockStyle.Fill;
            btnShowMiddle.Show();
            btnShowMiddle.Refresh();

            btnHidePanelBottom.Hide();
            btnHidePanelBottom.Refresh();
        }
        /***********************************************************************************************/
        private void btnShowMiddle_Click(object sender, EventArgs e)
        {
            this.panelBottom.Dock = DockStyle.Fill;
            this.panelMiddle.Show();
            btnShowMiddle.Hide();
            btnShowMiddle.Refresh();

            btnHidePanelBottom.Show();
            btnHidePanelBottom.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;
            if (dt == null)
                return;

            string type = "";
            try
            {
                int qtyOrdered = dt.Rows[row]["qty"].ObjToInt32();
                if (qtyOrdered >= 1)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /***********************************************************************************************/
        private void gridMain4_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv4.DataSource;
            string type = "";
            try
            {
                int qtyOrdered = dt.Rows[row]["qty"].ObjToInt32();
                if (qtyOrdered <= 0)
                {
                    e.Visible = false;
                    e.Handled = true;
                    return;
                }
                string match = dt.Rows[row]["matched"].ObjToString();
                string what = cmbShowWhat.Text;
                string showOrdered = cmbShowOrdered.Text;
                if (what == "Show Open Orders")
                {
                    if (showOrdered == "All")
                    {
                        if (match.ToUpper() == "MISMATCHED")
                        {
                            e.Visible = true;
                            e.Handled = true;
                            return;
                        }
                        if ( !String.IsNullOrWhiteSpace ( match ))
                        {
                            e.Visible = false;
                            e.Handled = true;
                        }
                        return;
                    }
                    ShowWhenOrdered( dt, row, showOrdered, e);
                    return;
                }
                if ( what == "Show Matches" )
                {
                    if ( match.ToUpper() != "MATCHED")
                    {
                        e.Visible = false;
                        e.Handled = true;
                        return;
                    }
                    ShowWhenOrdered( dt, row, showOrdered, e);
                }
                else if (what == "Show MisMatches")
                {
                    if (match.ToUpper() != "MISMATCHED")
                    {
                        e.Visible = false;
                        e.Handled = true;
                        return;
                    }
                    ShowWhenOrdered( dt, row, showOrdered, e);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
        /***********************************************************************************************/
        private void ShowWhenOrdered ( DataTable dt, int row, string showOrdered, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            DateTime dateOrdered = dt.Rows[row]["DateOrdered"].ObjToDateTime();
            DateTime dateImported = dt.Rows[row]["DateImported"].ObjToDateTime();
            DateTime dateDelivered = dt.Rows[row]["DateDelivered"].ObjToDateTime();

            DateTime date = dateTimePicker1.Value;

            DateTime date1 = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            
            date = dateTimePicker2.Value;
            DateTime date2 = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);

            if ( showOrdered == "Ordered")
            {
                if ( dateOrdered < date1 || dateOrdered > date2 )
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            else if ( showOrdered == "Imported")
            {
                if (dateImported < date1 || dateImported > date2)
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
            else if ( showOrdered == "Delivered")
            {
                if (dateDelivered < date1 || dateDelivered > date2)
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        {
            int rowHandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowHandle);

            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataTable dt4 = (DataTable)dgv4.DataSource;

            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;
            DataRow dr = gridMain4.GetFocusedDataRow();

            RemoveOrder(isChecked);
            //if (isChecked)
            //{
            //    //dr["orderThis"] = "";
            //    //dr["qty"] = 0;
            //    dt2.Rows[row]["orderThis"] = "";
            //    dt2.Rows[row]["qty"] = 0;

            //    dt4.Rows[row]["orderThis"] = "";
            //    dt4.Rows[row]["qty"] = 0;

            //    dt2.Rows[row]["removeThis"] = "";
            //    dt4.Rows[row]["removeThis"] = "";

            //    string location = dr["LocationCode"].ObjToString();
            //    string casket = dr["CasketDescription"].ObjToString();

            //    DataTable dx = (DataTable)dgv.DataSource;
            //    DataRow[] dRows = dx.Select("locationCode='" + location + "' AND casketDesc='" + casket + "'");
            //    if (dRows.Length > 0)
            //    {
            //        int ordered = dRows[0]["qtyOrdered"].ObjToInt32();
            //        ordered = ordered - 1;
            //        if (ordered < 0)
            //            ordered = 0;
            //        dRows[0]["qtyOrdered"] = ordered;

            //        gridMain.RefreshEditor(true);
            //    }
            //    string record = dt4.Rows[row]["record"].ObjToString();
            //    if (record != "-1")
            //    {
            //        if (!String.IsNullOrWhiteSpace(record))
            //        {
            //            G1.update_db_table("inventory_orders", "record", record, new string[] { "qty", "0" });
            //           // G1.delete_db_table("inventory_orders", "record", record); //Don't delete
            //        }
            //        //dt4.Rows[row]["record"] = -1;
            //        //dt2.Rows[row]["record"] = -1;
            //    }
            //}
            ////dr["removeThis"] = "";

            //for (int i = 0; i < dt2.Rows.Count; i++)
            //    dt2.Rows[i]["removeThis"] = "";
            //for (int i = 0; i < dt2.Rows.Count; i++)
            //    dt2.Rows[i]["orderThis"] = "";

            //dgv2.DataSource = dt2;
            //dgv4.DataSource = dt2;

            //dgv4.RefreshDataSource();
            //gridMain4.RefreshData();
            //gridMain4.RefreshEditor(true);

            //dgv2.RefreshDataSource();
            //gridMain2.RefreshData();
            //gridMain2.RefreshEditor(true);

            //dgv2.Refresh();
            //dgv4.Refresh();
        }
        /***********************************************************************************************/
        private void RemoveOrder ( bool isChecked )
        {
            int rowHandle = gridMain4.FocusedRowHandle;
            int row = gridMain4.GetDataSourceRowIndex(rowHandle);

            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataTable dt4 = (DataTable)dgv4.DataSource;

            DataRow dr = gridMain4.GetFocusedDataRow();
            if (isChecked)
            {
                //dr["orderThis"] = "";
                //dr["qty"] = 0;
                dt2.Rows[row]["orderThis"] = "";
                dt2.Rows[row]["qty"] = 0;

                dt4.Rows[row]["orderThis"] = "";
                dt4.Rows[row]["qty"] = 0;

                dt2.Rows[row]["removeThis"] = "";
                dt4.Rows[row]["removeThis"] = "";

                string location = dr["LocationCode"].ObjToString();
                string casket = dr["CasketDescription"].ObjToString();

                DataTable dx = (DataTable)dgv.DataSource;
                DataRow[] dRows = dx.Select("locationCode='" + location + "' AND casketDesc='" + casket + "'");
                if (dRows.Length > 0)
                {
                    int ordered = dRows[0]["qtyOrdered"].ObjToInt32();
                    ordered = ordered - 1;
                    if (ordered < 0)
                        ordered = 0;
                    dRows[0]["qtyOrdered"] = ordered;

                    gridMain.RefreshEditor(true);
                }
                string record = dt4.Rows[row]["record"].ObjToString();
                if (record != "-1")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                    {
                        G1.update_db_table("inventory_orders", "record", record, new string[] { "qty", "0" });
                        // G1.delete_db_table("inventory_orders", "record", record); //Don't delete
                    }
                    //dt4.Rows[row]["record"] = -1;
                    //dt2.Rows[row]["record"] = -1;
                }
            }
            //dr["removeThis"] = "";

            for (int i = 0; i < dt2.Rows.Count; i++)
                dt2.Rows[i]["removeThis"] = "";
            for (int i = 0; i < dt2.Rows.Count; i++)
                dt2.Rows[i]["orderThis"] = "";

            dgv2.DataSource = dt2;
            dgv4.DataSource = dt2;

            dgv4.RefreshDataSource();
            gridMain4.RefreshData();
            gridMain4.RefreshEditor(true);

            dgv2.RefreshDataSource();
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);

            dgv2.Refresh();
            dgv4.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit2_CheckedChanged(object sender, EventArgs e)
        {
            int rowHandle = gridMain2.FocusedRowHandle;
            int row = gridMain2.GetDataSourceRowIndex(rowHandle);

            DataTable dt2 = (DataTable)dgv2.DataSource;
            DataTable dt4 = (DataTable)dgv4.DataSource;

            DevExpress.XtraEditors.CheckEdit check = (DevExpress.XtraEditors.CheckEdit)sender;
            bool isChecked = true;
            if (!check.Checked)
                isChecked = false;
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (isChecked)
            {
                //dr["orderThis"] = "1";
                //dr["qty"] = 1;
                //                dt2.Rows[row]["orderThis"] = "1";
                dt2.Rows[row]["orderThis"] = "";
                dt2.Rows[row]["qty"] = 1;

                dt2.Rows[row]["orderedBy"] = LoginForm.username;

                //                dt4.Rows[row]["orderThis"] = "1";
                dt4.Rows[row]["orderThis"] = "";
                dt4.Rows[row]["qty"] = 1;

                dt2.Rows[row]["removeThis"] = "";
                dt4.Rows[row]["removeThis"] = "";
                string record = dt2.Rows[row]["record"].ObjToString();

                SaveOrder(dt2, row, record);

                string location = dr["LocationCode"].ObjToString();
                string casket = dr["CasketDescription"].ObjToString();

                DataTable dx = (DataTable)dgv.DataSource;
                DataRow[] dRows = dx.Select("locationCode='" + location + "' AND casketDesc='" + casket + "'");
                if ( dRows.Length > 0 )
                {
                    int ordered = dRows[0]["qtyOrdered"].ObjToInt32();
                    ordered = ordered + 1;
                    dRows[0]["qtyOrdered"] = ordered;

                    gridMain.RefreshEditor(true);
                }
            }
            //dr["removeThis"] = "";

            for (int i = 0; i < dt2.Rows.Count; i++)
                dt2.Rows[i]["orderThis"] = "";
            for (int i = 0; i < dt4.Rows.Count; i++)
                dt4.Rows[i]["orderThis"] = "";

            dgv2.DataSource = dt2;
            dgv4.DataSource = dt4;

            dgv4.RefreshDataSource();
            gridMain4.RefreshData();
            gridMain4.RefreshEditor(true);

            //dgv2.RefreshDataSource();
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);

            dgv2.Refresh();
            dgv4.Refresh();
        }
        /***********************************************************************************************/
        private void btnShowOnHand_Click(object sender, EventArgs e)
        {
            string str = btnShowOnHand.Text;
            if ( str.ToUpper() == "SHOW ON HAND")
            {
                panelBottom.Hide();
                panelMiddle.Hide();
                panelTop.Dock = DockStyle.Fill;
                btnShowOnHand.Text = "Show All";
                btnShowOnHand.Refresh();
            }
            else
            {
                panelTop.Dock = DockStyle.Top;
                btnShowOnHand.Text = "Show On Hand";
                btnShowOnHand.Refresh();


                panelBottom.Show();
                panelMiddle.Show();
                btnHidePanelMiddle.Show();
                btnHidePanelBottom.Show();

                btnShowMiddle.Hide();
                btnShowBottom.Hide();
            }
        }
        /***********************************************************************************************/
        private void gridMain2_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        private void gridMain4_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
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
        private void enterReceivedMerchandiseToolStripMenuItem_Click(object sender, EventArgs e)
        { // Generate Report Data Entry for Delivered Merchandise
            OrdersMatch matchForm = new OrdersMatch();
            matchForm.Show();
        }
        /***********************************************************************************************/
        private void gridMain4_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    if (date.Year > 100)
                        e.DisplayText = date.ToString("MM/dd/yyyy");
                    else
                        e.DisplayText = "";
                }
            }
        }
        /***********************************************************************************************/
        private bool printOrders = false;
        private void btnPrintOrders_Click(object sender, EventArgs e)
        {
            printOrders = true;
            printPreviewToolStripMenuItem_Click(null, null);
            printOrders = false;
        }
        /***********************************************************************************************/
        private bool printOnOrders = false;
        private void btnPrintOnOrder_Click(object sender, EventArgs e)
        {
            printOnOrders = true;
            printPreviewToolStripMenuItem_Click(null, null);
            printOnOrders = false;
        }
        /***********************************************************************************************/
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain2);
        }
        /***********************************************************************************************/
        private void pictureBox3_Click(object sender, EventArgs e)
        {
            G1.ShowHideFindPanel(gridMain4);
        }
        /***********************************************************************************************/
        private void btnAddOrder_Click(object sender, EventArgs e)
        {
            InventoryAdd orderForm = new InventoryAdd();
            orderForm.SelectDone += OrderForm_SelectDone;
            orderForm.ShowDialog();
        }
        /***********************************************************************************************/
        private void OrderForm_SelectDone(string status, string poNumber, string description, string location)
        {
            string record = G1.create_record("inventory_orders", "LocationCode", "-1");
            if (G1.BadRecord("inventory_orders", record))
                return;
            string qty = "1";
            string casketdesc = description;
            string casketcode = "";
            string[] Lines = description.Split(' ');
            if ( Lines.Length > 0 )
                casketcode = Lines[0].Trim();
            string orderdate = DateTime.Now.ToString("yyyy-MM-dd");
            string replacement = poNumber;
            string user = LoginForm.username;

            G1.update_db_table("inventory_orders", "record", record, new string[] { "qty", qty, "qtyPending", qty, "LocationCode", location, "CasketDescription", casketdesc, "CasketCode", casketcode });
            G1.update_db_table("inventory_orders", "record", record, new string[] { "orderedby", user, "DateOrdered", orderdate, "replacement", replacement });

            this.Cursor = Cursors.WaitCursor;
            LoadOrders();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnDeleteOrder_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;

            string casket = dr["CasketDescription"].ObjToString();
            string poNumber = dr["replacement"].ObjToString();

            DialogResult result = MessageBox.Show("Are you sure you want to DELETE this order (" + casket + ") ?", "Delete Order Dialog", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (result == DialogResult.No)
                return;

            if (!String.IsNullOrWhiteSpace(poNumber))
            {
                RemoveOrder(true);
                return;
            }

            string record = dr["record"].ObjToString();
            if ( !String.IsNullOrWhiteSpace ( record))
            {
                if ( record != "0" && record != "-1")
                {
                    G1.delete_db_table("inventory_orders", "record", record);

                    this.Cursor = Cursors.WaitCursor;
                    LoadOrders();
                    this.Cursor = Cursors.Default;
                }
            }
        }
        /***********************************************************************************************/
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain4.RefreshEditor( true );
            gridMain4.RefreshData();
            dgv4.Refresh();
        }
        /***********************************************************************************************/
        private void cmbShowOrdered_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain4.RefreshEditor(true);
            gridMain4.RefreshData();
            dgv4.Refresh();
        }
        /***********************************************************************************************/
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            string showOrdered = cmbShowOrdered.Text;
            if (showOrdered == "All")
                return;
            gridMain4.RefreshEditor(true);
            gridMain4.RefreshData();
            dgv4.Refresh();
        }
        /***********************************************************************************************/
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            string showOrdered = cmbShowOrdered.Text;
            if (showOrdered == "All")
                return;
            gridMain4.RefreshEditor(true);
            gridMain4.RefreshData();
            dgv4.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain_CustomColumnDisplayText(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
            if (e.Column.FieldName.ToUpper().IndexOf("DATE") >= 0 && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText.IndexOf("0000") >= 0 || e.DisplayText.IndexOf("0001") >= 0)
                    e.DisplayText = "";
                else
                {
                    DateTime date = e.DisplayText.ObjToString().ObjToDateTime();
                    e.DisplayText = date.ToString("MM/dd/yyyy");
                    if (date.Year < 30)
                        e.DisplayText = "";
                }
            }
            else if (e.Column.FieldName.ToUpper() == "QTYORDERED" && e.ListSourceRowIndex != DevExpress.XtraGrid.GridControl.InvalidRowHandle)
            {
                if (e.DisplayText == "0")
                    e.DisplayText = "";
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain2.GetFocusedDataRow();
            if (dr == null)
                return;
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            string ownership = combo.Text.Trim();
            string record = dr["record"].ObjToString();
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("inventory_orders", "record", record, new string[] { "Ownership", ownership});
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;
            string record = dr["record"].ObjToString();
            DevExpress.XtraEditors.ComboBoxEdit combo = (DevExpress.XtraEditors.ComboBoxEdit)sender;
            string ownership = combo.Text.Trim();
            if (!String.IsNullOrWhiteSpace(record))
                G1.update_db_table("inventory_orders", "record", record, new string[] { "Ownership", ownership });
        }
        /***********************************************************************************************/
        private void changeSerialNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            if (dr == null)
                return;

            DataTable dt = (DataTable)dgv4.DataSource;

            string serialNumber = dr["serialNumber"].ObjToString();

            using (Ask askForm = new Ask("Enter Correct Serial Number!"))
            {
                askForm.Text = "Change Serial Number (" + serialNumber + ")";
                askForm.ShowDialog();
                if (askForm.DialogResult != System.Windows.Forms.DialogResult.OK)
                    return;
                string newSerialNumber = askForm.Answer;
                if (String.IsNullOrWhiteSpace(newSerialNumber))
                    return;
                string record = dr["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    G1.update_db_table("inventory_orders", "record", record, new string[] { "serialNumber", newSerialNumber });
                    dr["serialNumber"] = newSerialNumber;
                }
            }
        }
        /***********************************************************************************************/
    }
}
