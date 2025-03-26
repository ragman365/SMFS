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
using DevExpress.XtraGrid.Views.Grid;
using System.IO;
using DevExpress.XtraRichEdit;
using DevExpress.XtraEditors.Repository;
using MySql.Data.MySqlClient;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DevExpress.XtraGrid.Columns;
using System.Text.RegularExpressions;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;

/***********************************************************************************************/
namespace SMFS

{
    /***********************************************************************************************/
    public partial class CustomerExtra : DevExpress.XtraEditors.XtraForm
    {
        private bool modified = false;
        private bool loading = true;
        private string workFormName = "";
        private string workType = "";
        private string workRecord = "";
        private string workLocation = "";
        private int DBTableCount = 0;
        private string[] DBTable = new string[10];
        private DataTable[] DBTables = new DataTable[10];
        /***********************************************************************************************/
        public CustomerExtra()
        {
            InitializeComponent();
        }
        /***********************************************************************************************/
        private void CustomerExtra_Load(object sender, EventArgs e)
        {
            btnSave.Hide();
            btnLoad.Hide();

            LoadData();
        }
        /***********************************************************************************************/
        private void LoadData ()
        {
            string cmd = "Select * from `cust_extended_layout` ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("ODB");
            dt.Columns.Add("track");
            dt.Columns.Add("dropOnly");
            dt.Columns.Add("addContact");
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["dbfield"].ObjToString();
                dt.Rows[i]["ODB"] = str; // Original Database Field
            }
            LoadTracking(dt);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
        }
        /***********************************************************************************************/
        private void LoadTracking ( DataTable dt)
        {
            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew = this.repositoryItemCheckEdit1;
            selectnew.NullText = "";
            selectnew.ValueChecked = "1";
            selectnew.ValueUnchecked = "0";
            selectnew.ValueGrayed = "";

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew4 = this.repositoryItemCheckEdit4;
            selectnew4.NullText = "";
            selectnew4.ValueChecked = "1";
            selectnew4.ValueUnchecked = "0";
            selectnew4.ValueGrayed = "";

            DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit selectnew5 = this.repositoryItemCheckEdit5;
            selectnew5.NullText = "";
            selectnew5.ValueChecked = "1";
            selectnew5.ValueUnchecked = "0";
            selectnew5.ValueGrayed = "";

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                dt.Rows[i]["track"] = "0";
                dt.Rows[i]["dropOnly"] = "0";
                dt.Rows[i]["addContact"] = "0";
            }

            string field = "";
            string set = "";
            DataRow[] dR = null;
            string cmd = "Select * from `tracking`;";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                field = dx.Rows[i]["tracking"].ObjToString();
                dR = dt.Select("dbfield='" + field + "'");
                if (dR.Length > 0)
                    dR[0]["track"] = "1";

                set = dx.Rows[i]["dropOnly"].ObjToString();
                if (set == "1")
                    dR[0]["dropOnly"] = "1";

                set = dx.Rows[i]["addContact"].ObjToString();
                if (set == "1")
                    dR[0]["addContact"] = "1";
            }
        }
        /***********************************************************************************************/
        private void CopyOriginalNames ( DataTable dt )
        {
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dt.Rows[i]["originalfield"] = dt.Rows[i]["field"].ObjToString();
                dt.Rows[i]["originaltype"] = dt.Rows[i]["type"].ObjToString();
                dt.Rows[i]["originallength"] = dt.Rows[i]["length"].ObjToString();
                dt.Rows[i]["originalqualify"] = dt.Rows[i]["length"].ObjToString();
            }
        }
        /***********************************************************************************************/
        private void gridMain_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (e.Column.FieldName.ToUpper() == "TYPE")
            {
                string data = dr["type"].ObjToString().ToUpper();
                if (data != "STRING" && data != "NUMERIC" && data != "DATE" && data != "INT" && data != "DOUBLE" && data != "TEXT" && data != "DAY" && data != "FULLDATE" )
                {
                    MessageBox.Show("***ERROR*** Data Type must be either 'string,numeric,date,day,int, or double'.");
                    dr["type"] = "";
                    return;
                }
            }
            dr["mod"] = "Y";
            modified = true;
            btnSave.Show();
        }
        /***********************************************************************************************/
        private void LoadDBTableFields(string table, RepositoryItemComboBox combo)
        {
            if (String.IsNullOrWhiteSpace(table))
                return;
            if (table.ToUpper() == "NONE")
            {
                combo.Items.Clear();
                return;
            }
            bool found = false;
            DataTable rx = null;
            for (int i = 0; i < DBTableCount; i++)
            {
                if (DBTable[i].ToUpper() == table.ToUpper())
                {
                    found = true;
                    table = DBTable[i];
                    rx = (DataTable)DBTables[i];
                    break;
                }
            }
            if (!found)
            {
                string command = "SHOW COLUMNS FROM `" + table + "`;";
                rx = G1.get_db_data(command);
                if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                    return; // Somehow the table does not exist
            }
            combo.Items.Clear();
            string name = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                name = rx.Rows[i]["field"].ToString().Trim();
                if (name.Trim().ToUpper() == "TMSTAMP")
                    continue;
                combo.Items.Add(name);
            }
            if (!found)
            {
                DBTable[DBTableCount] = table;
                DBTables[DBTableCount] = rx;
                DBTableCount++;
            }
        }
        /***********************************************************************************************/
        private void CustomerExtra_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nExtended Fields have been modified!\nWould you like to save your changes?", "Add/Edit Fields Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            modified = false;
            if (result == DialogResult.No)
                return;
            btnSave_Click(null, null);
        }
        /***********************************************************************************************/
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv.DataSource;
            SaveLayout(dt, workFormName);
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void SaveLayout ( DataTable dt, string workFormName )
        {
            string record = "";
            string oldRecord = "";
            string field = "";
            string type = "";
            string user = LoginForm.username;
            string olduser = "";
            DateTime now = DateTime.Now;
            string added = G1.DateTimeToSQLDateTime(now);
            string help = "";
            string reference = "";
            string status = "";
            string group = "";
            string dbfield = "";
            string odb = "";
            string saveGroup = "";

            string cmd = "Select * from `cust_extended_layout`;";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("DoesExist");

            string mod = "";
            DataRow[] dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                status = dt.Rows[i]["status"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                odb = dt.Rows[i]["ODB"].ObjToString();
                record = dt.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    dR = dx.Select("record='" + record + "'");
                    if (dR.Length > 0)
                        dR[0]["DoesExist"] = "Y";
                }
                group = dt.Rows[i]["group"].ObjToString();
                if (!String.IsNullOrWhiteSpace(group))
                    saveGroup = group;
                else
                    group = saveGroup;

                if (!String.IsNullOrWhiteSpace(record))
                {
                    if (odb.Trim().ToUpper() != dbfield.Trim().ToUpper())
                    {
                        if (!String.IsNullOrWhiteSpace(odb) && !String.IsNullOrWhiteSpace(dbfield))
                        {
                            cmd = "ALTER TABLE `cust_extended` CHANGE `" + odb + "` `" + dbfield + "` varchar(100);";
                            try
                            {
                                G1.get_db_data(cmd);
                                odb = dbfield;
                            }
                            catch (Exception ex)
                            {
                            }
                        }
                    }
                }

                if (mod == "D" || status.ToUpper() == "DELETE")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("cust_extended_layout", "record", record);
                    RemoveField("cust_extended", dbfield);
                    continue;
                }

                field = dt.Rows[i]["field"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                if (String.IsNullOrWhiteSpace(type))
                    type = "TEXT";
                olduser = dt.Rows[i]["user"].ObjToString();
                help = dt.Rows[i]["help"].ObjToString();
                reference = dt.Rows[i]["reference"].ObjToString();
                if (String.IsNullOrWhiteSpace(record))
                    record = "-1";
                if (record == "-1")
                    record = G1.create_record("cust_extended_layout", "field", "-1");
                if (G1.BadRecord("cust_extended_layout", record))
                    continue;
                G1.update_db_table("cust_extended_layout", "record", record, new string[] { "group", group, "field", field, "dbfield", dbfield, "type", type, "help", help, "status", status, "reference", reference, "order", i.ToString() });
                if (String.IsNullOrWhiteSpace(olduser))
                    G1.update_db_table("cust_extended_layout", "record", record, new string[] { "dateAdded", added, "user", user });
                dt.Rows[i]["record"] = record;
                dt.Rows[i]["ODB"] = dbfield;
                verifyFieldExists("cust_extended", dbfield);
                verifyFieldExists("fcust_extended", dbfield);
            }

            LoadData();
        }
        /***********************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dRow["track"] = "0";
            dRow["dropOnly"] = "0";
            dRow["addContact"] = "0";
            // dRow["mod"] = "Y"; // Update if Modified
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
            // modified = true;
        }
        /***********************************************************************************************/
        private void btnLoad_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            //string tablename = "options_test";
            //RemoveAllColumns(tablename);
            string table = "";
            string dbfield = "";
            string str = "";
            string strDelimitor = " +";
            DataTable dt = (DataTable)dgv.DataSource;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    table = dt.Rows[i]["table"].ObjToString();
                    dbfield = dt.Rows[i]["dbfield"].ObjToString();
                    if (String.IsNullOrWhiteSpace(table))
                        continue;
                    if (String.IsNullOrWhiteSpace(dbfield))
                        continue;
                    string[] Lines = dbfield.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int j = 0; j < Lines.Length; j++)
                    {
                        dbfield = Lines[j].Trim();
                        if (String.IsNullOrWhiteSpace(dbfield))
                            continue;
                        verifyFieldExists(table, dbfield);
                    }

                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Strolling through Table " + table + " Field " + dbfield + " " + ex.Message.ToString());
            }
//            bool rtn = TieTable(tablename, dt);
//            if (rtn)
//            {
////                string cmd = "Select * from `options_test`;";
////                DataTable dx = G1.get_db_data(cmd);
//                TieDataTable("clients", dt);
//                this.Cursor = Cursors.Default;
//                MessageBox.Show("Adding Complete . . .");
//            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private bool verifyFieldExists ( string table, string dbfield )
        {
            bool rv = false;
            if (String.IsNullOrWhiteSpace(table))
                return rv;
            if (String.IsNullOrWhiteSpace(dbfield))
                return rv;
            try
            {
                string command = "SHOW COLUMNS FROM `" + table + "`;";
                DataTable rx = G1.get_db_data(command);
                if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                    return false; // Somehow the table does not exist
                DataRow[] dRows = rx.Select("Field='" + dbfield + "'");
                if (dRows.Length <= 0)
                {
                    string length = "100";
                    string newstr = "alter table `" + table + "` add `" + dbfield + "` VARCHAR (" + length + ") NOT NULL DEFAULT '';";
                    try
                    {
                        DataTable ddx = G1.get_db_data(newstr);
                        rv = true;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR 1*** Adding New Field " + dbfield + " for Table " + table + " " + ex.Message.ToString());
                    }
                }
                else
                    rv = true;
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR 2*** Adding New Field " + dbfield + " for Table " + table + " " + ex.Message.ToString());
            }

            return rv;
        }
        /***********************************************************************************************/
        private bool RemoveField(string table, string dbfield)
        {
            bool rv = false;
            if (String.IsNullOrWhiteSpace(table))
                return rv;
            if (String.IsNullOrWhiteSpace(dbfield))
                return rv;
            try
            {
                string cmd = "alter table `" + table + "` DROP `" + dbfield + "`;";
                DataTable ddx = G1.get_db_data(cmd);
                rv = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Removing from Table " + table + " Field " + dbfield + " Error " + ex.Data.ToString());
            }
            return rv;
        }
        /***********************************************************************************************/
        private void RemoveAllColumns(string tablename)
        {
            string command = "SHOW COLUMNS FROM `" + tablename + "`;";
            command = "Select * from `" + tablename + "`;";
            DataTable dt = G1.get_db_data(command);
            //            DataTable rx = G1.get_db2_data(cmd);
            if (dt == null )
                return; // Somehow the table does not exist
            string original = "";
            string newstr = "";
            int limit = dt.Columns.Count;
            int row = 0;
            try
            {
                for (int i = 0; i < limit; i++)
                {
                    row = i;
                    original = dt.Columns[i].ColumnName.ToString().Trim();
                    if (original.ToUpper() == "TMSTAMP")
                        continue;
                    else if (original.ToUpper() == "RECORD")
                        continue;
                    newstr = "alter table `" + tablename + "` DROP `" + original + "`;";
                    DataTable ddx = G1.get_db_data(newstr);
                }
            }
            catch ( Exception ex)
            {
                MessageBox.Show("***ERROR*** Removing from table " + tablename + " " + original + " " + row.ToString() + " Error " + ex.Data.ToString());
            }
        }
        /***********************************************************************************************/
        public static bool TieTable(string tablename, DataTable dt)
        {
            //string cmd = "select column_name,data_type,column_key,character_maximum_length,column_default,extra from information_schema.`COLUMNS` where `table_schema` = 'JMAPATIENTS'";
            //cmd += " and `table_name` = '" + tablename.ToUpper() + "';";
            tablename = "options_test";
            string command = "SHOW COLUMNS FROM `" + tablename + "`;";
            DataTable rx = G1.get_db_data(command);
            //            DataTable rx = G1.get_db2_data(cmd);
            if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                return false; // Somehow the table does not exist
            string original = "";
            string name = "";
            string type = "";
            string length = "";
            string mod = "";
            string columnname = "";
            string newstr = "";
            int row = 0;
            try
            {
                int limit = dt.Rows.Count;
                for (int i = 0; i < limit; i++)
                {
                    row = i;
                    bool found = false;
                    mod = dt.Rows[i]["mod"].ToString().Trim();
                    original = dt.Rows[i]["field"].ToString().Trim();
                    name = original.ToUpper();
                    type = dt.Rows[i]["type"].ToString().Trim().ToUpper();
                    if (type == "STRING")
                        type = "SYSTEM.STRING";
                    else if (type == "TEXT")
                        type = "SYSTEM.STRING";
                    else if (type == "NUMERIC")
                        type = "SYSTEM.DOUBLE";
                    else if (type == "DOUBLE")
                        type = "SYSTEM.DOUBLE";
                    else if (type == "INT")
                        type = "SYSTEM.INT32";
                    else if (type == "DATE")
                        type = "SYSTEM.DATE";
                    length = dt.Rows[i]["length"].ToString().Trim().ToUpper();
                    if (G1.validate_numeric(length))
                    {
                        int len = length.ObjToInt32();
                        if (len <= 0)
                            length = "10";
                        else if (len > 200)
                            length = "200";
                    }
                    else
                        length = "10";
                    if (name.Trim().Length == 0)
                        continue;
                    for (int j = 0; j < rx.Rows.Count; j++)
                    {
                        columnname = rx.Rows[j]["field"].ToString().Trim().ToUpper();
                        if (columnname.Trim().Length == 0)
                            continue;
                        if (columnname == name)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    { // Didn't find the column, so create it.
                        newstr = "";
                        if (type == "SYSTEM.STRING")
                            newstr = "alter table `" + tablename + "` add `" + original + "` VARCHAR (" + length + ") NOT NULL DEFAULT '';";
                        else if (type == "SYSTEM.DOUBLE")
                            newstr = "alter table `" + tablename + "` add `" + original + "` DOUBLE NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.INT32")
                            newstr = "alter table `" + tablename + "` add `" + original + "` INT NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.INT64")
                            newstr = "alter table `" + tablename + "` add `" + original + "` BIGINT NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.DATE")
                            newstr = "alter table `" + tablename + "` add `" + original + "` DATE NOT NULL DEFAULT '0000-00-00' ;";
                        try
                        {
                            if ( String.IsNullOrWhiteSpace ( newstr))
                            {
                                MessageBox.Show("***ERROR*** Invalid DataType for field " + original + "!");
                                break;
                            }
                            DataTable ddx = G1.get_db_data(newstr);
                        }
                        catch ( Exception ex )
                        {
                            MessageBox.Show("***ERROR*** Adding New Field " + original + " " + ex.Message.ToString());
                        }
                    }
                    else
                    {
                        if ( mod == "D")
                        {
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Adding to table " + tablename + " " + original + " " + row.ToString() + " Error " + ex.Data.ToString());
                return false;
            }
            return true;
        }
        /***********************************************************************************************/
        public static bool TieDataTable(string tablename, DataTable dt)
        {
            //string cmd = "select column_name,data_type,column_key,character_maximum_length,column_default,extra from information_schema.`COLUMNS` where `table_schema` = 'JMAPATIENTS'";
            //cmd += " and `table_name` = '" + tablename.ToUpper() + "';";
//            tablename = "options_test";
            string command = "SHOW COLUMNS FROM `" + tablename + "`;";
            DataTable rx = G1.get_db_data(command);
            //            DataTable rx = G1.get_db2_data(cmd);
            if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                return false; // Somehow the table does not exist
            string original = "";
            string name = "";
            string type = "";
            string length = "";
            string mod = "";
            string columnname = "";
            string newstr = "";
            int row = 0;
            try
            {
                int limit = dt.Rows.Count;
                for (int i = 0; i < limit; i++)
                {
                    row = i;
                    bool found = false;
                    mod = dt.Rows[i]["mod"].ToString().Trim();
                    original = dt.Rows[i]["field"].ToString().Trim();
                    name = original.ToUpper();
                    type = dt.Rows[i]["type"].ToString().Trim().ToUpper();
                    if (type == "STRING")
                        type = "SYSTEM.STRING";
                    else if (type == "TEXT")
                        type = "SYSTEM.STRING";
                    else if (type == "NUMERIC")
                        type = "SYSTEM.DOUBLE";
                    else if (type == "DOUBLE")
                        type = "SYSTEM.DOUBLE";
                    else if (type == "INT")
                        type = "SYSTEM.INT32";
                    else if (type == "DATE")
                        type = "SYSTEM.DATE";
                    length = dt.Rows[i]["length"].ToString().Trim().ToUpper();
                    if (G1.validate_numeric(length))
                    {
                        int len = length.ObjToInt32();
                        if (len <= 0)
                            length = "10";
                        else if (len > 200)
                            length = "200";
                    }
                    else
                        length = "10";
                    if (name.Trim().Length == 0)
                        continue;
                    for (int j = 0; j < rx.Rows.Count; j++)
                    {
                        columnname = rx.Rows[j]["field"].ToString().Trim().ToUpper();
                        if (columnname.Trim().Length == 0)
                            continue;
                        if (columnname == name)
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    { // Didn't find the column, so create it.
                        newstr = "";
                        if (type == "SYSTEM.STRING")
                            newstr = "alter table `" + tablename + "` add `" + original + "` VARCHAR (" + length + ") NOT NULL DEFAULT '';";
                        else if (type == "SYSTEM.DOUBLE")
                            newstr = "alter table `" + tablename + "` add `" + original + "` DOUBLE NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.INT32")
                            newstr = "alter table `" + tablename + "` add `" + original + "` INT NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.INT64")
                            newstr = "alter table `" + tablename + "` add `" + original + "` BIGINT NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.DATE")
                            newstr = "alter table `" + tablename + "` add `" + original + "` DATE NOT NULL DEFAULT '0000-00-00' ;";
                        try
                        {
                            if (String.IsNullOrWhiteSpace(newstr))
                            {
                                MessageBox.Show("***ERROR*** Invalid DataType for field " + original + "!");
                                break;
                            }
                            DataTable ddx = G1.get_db_data(newstr);
                        }
                        catch ( Exception ex )
                        {
                            MessageBox.Show("***ERROR*** Adding New Field to Table" + original + " " + ex.Message.ToString());
                        }
                    }
                    else
                    {
                        if (mod == "D")
                        {
                        }
                        else
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Adding to table " + tablename + " " + original + " " + row.ToString() + " Error " + ex.Data.ToString());
                return false;
            }
            return true;
        }
        /***********************************************************************************************/
        private void picDelete_Click(object sender, EventArgs e)
        {
            int row = 0;
            string mod = "";
            string delete = "";
            int[] Rows = gridMain.GetSelectedRows();
            for ( int i=0; i<Rows.Length; i++)
            {
                row = Rows[i];
                DataRow dr = gridMain.GetDataRow(row);
                mod = dr["mod"].ObjToString();
                delete = dr["status"].ObjToString();
                if (mod == "D" || delete == "DELETE")
                {
                    dr["mod"] = "Y";
                    dr["status"] = "";
                }
                else
                {
                    dr["mod"] = "D";
                    dr["status"] = "DELETE";
                }
                modified = true;
            }
            if (modified)
                btnSave.Show();
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string delete = dt.Rows[row]["mod"].ObjToString();
            //if (delete.ToUpper() == "D")
            //{
            //    e.Visible = false;
            //    e.Handled = true;
            //}
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
            MoveRowUp(dt, rowHandle);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.SelectRow(rowHandle - 1);
            gridMain.FocusedRowHandle = rowHandle - 1;
            gridMain.RefreshData();
            dgv.Refresh();
            modified = true;
            btnSave.Show();
        }
        /***************************************************************************************/
        private void MoveRowUp(DataTable dt, int row)
        {
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
            modified = true;
            btnSave.Show();
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
        private string GetClipBoard()
        {
            string text = "";
            if (Clipboard.ContainsText())
            {
                text = Clipboard.GetText(TextDataFormat.Text);
            }
            return text;
        }
        /***********************************************************************************************/
        private void gridMain_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)22)
            { // Ctrl+V
                string text = GetClipBoard();
                string strDelimitor = "\r\n";
                string[] Lines = text.Split(new[] { strDelimitor }, StringSplitOptions.None);
                if (Lines.Length > 0)
                {
                    DataTable dt = (DataTable)dgv.DataSource;
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        if (!String.IsNullOrWhiteSpace(Lines[i]))
                        {
                            DataRow dRow = dt.NewRow();
                            dRow["field"] = Lines[i].Trim();
                            dt.Rows.Add(dRow);
                            modified = true;
                        }
                    }
                }
            }
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
            //if (rowHandle == (dt.Rows.Count - 1))
            //    return; // Already at the last row
            DataRow dRow = dt.NewRow();
            //dRow["mod"] = "Y"; //Don't set modified unless something is typed in
            dRow["track"] = "0";
            dRow["dropOnly"] = "0";
            dRow["addContact"] = "0";
            modified = true;
            dt.Rows.InsertAt(dRow, dtRow);
            G1.NumberDataTable(dt);
            dt.AcceptChanges();
            dgv.DataSource = dt;
            gridMain.ClearSelection();
            gridMain.RefreshData();
            gridMain.FocusedRowHandle = rowHandle + 1;
            gridMain.SelectRow(rowHandle + 1);
            dgv.Refresh();
            //btnSave.Show();
        }
        /***********************************************************************************************/
        private bool DetermineCutAndPasteType(string line)
        {
            string[] fields = line.Split('\t');
            if (fields.Length == 0)
                return false;
            if (fields[0].Trim().ToUpper().IndexOf("NUM") >= 0)
                return true;
            return false;
        }
        /***********************************************************************************************/
        private string ConvertToFieldNames(string Line)
        {
            var gv = (GridView)gridMain;
            string fieldNames = "";
            string caption = "";
            string[] lines = Line.Split('\t');
            for (int i = 0; i < lines.Length; i++)
            {
                caption = lines[i].ToUpper();
                for (int j = 0; j < gv.Columns.Count; j++)
                {
                    if (gv.Columns[j].Caption.ToUpper() == caption)
                    {
                        fieldNames += gv.Columns[j].FieldName + "\t";
                    }
                }
            }
            return fieldNames;
        }
        /***********************************************************************************************/
        private void PasteFromSelf(string[] Lines, int position)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            var gv = (GridView)gridMain;

            string str = "";

            string field = "";
            string data = "";
            try
            {
                str = ConvertToFieldNames(Lines[0]);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** with Import Heading!");
                return;
            }
            string[] lines = str.Split('\t');
            int i = 0;
            int j = 0;

            for (i = 1; i < Lines.Length; i++)
            {
                try
                {
                    str = Lines[i].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        continue;
                    string[] fields = str.Split('\t');

                    DataRow dRow = dt.NewRow();
                    dt.Rows.InsertAt(dRow, position + i - 1);

                    //                    dt.Rows.Add(dRow);
                    int rowIndex = dt.Rows.Count - 1;
                    rowIndex = position + i - 1;

                    for (j = 0; j < fields.Length; j++)
                    {
                        data = fields[j].Trim();
                        field = lines[j];
                        dt.Rows[rowIndex][field] = data;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** with import fields! Field=" + field + "! " + ex.Message.ToString());
                }
            }
            dgv.DataSource = dt;
            dgv.RefreshDataSource();
            modified = true;
        }
        /***********************************************************************************************/
        private bool modifyBox = false;
        private void repositoryItemComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (modifyBox)
                return;
            ComboBoxEdit box = (ComboBoxEdit)sender;
            string item = box.EditValue.ObjToString();
            if ( item.ToUpper() == "ACTIVE")
            {
                modifyBox = true;
                box.EditValue = "";
                modifyBox = false;
                DataRow dr = gridMain.GetFocusedDataRow();
                dr["status"] = "";
                int rowHandle = gridMain.FocusedRowHandle;
                int dtRow = gridMain.GetDataSourceRowIndex(rowHandle);
                DataTable dt = (DataTable)dgv.DataSource;
                dt.Rows[dtRow]["status"] = "";
                dgv.DataSource = dt;
                dgv.RefreshDataSource();
                dgv.Refresh();
            }
        }
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABPAGELAYOUT")
            {
                DataTable dt = (DataTable)dgv.DataSource;
                DataTable dx = dt.Copy();
                dx.Columns.Add("data");
                string field = "";
                int groupNumber = 1;
                string oldGroup = "";
                string group = "";
                string help = "";

                for ( int i=0; i<dx.Rows.Count; i++)
                {
                    help = dx.Rows[i]["help"].ObjToString();
                    group = dx.Rows[i]["group"].ObjToString();
                    if (String.IsNullOrWhiteSpace(oldGroup))
                        oldGroup = group;
                    if (String.IsNullOrWhiteSpace(group))
                        group = oldGroup;
                    if (group != oldGroup)
                        groupNumber++;
                    oldGroup = group;
                    group = groupNumber.ToString() + ". " + group;
                    dx.Rows[i]["group"] = group;
                    field = dx.Rows[i]["field"].ObjToString().Trim() + ":";
                    dx.Rows[i]["field"] = field;
                }
                dgv2.DataSource = dx;
                gridMain2.ExpandAllGroups();
            }
            else if ( current.Name.Trim().ToUpper() == "TABPAGEDECODE")
            {
                bool doBuild = false;
                if (dgv3 == null)
                    doBuild = true;
                else if (dgv3.DataSource == null)
                    doBuild = true;
                if (!doBuild)
                    return;
                BuildDecode();
            }
        }
        /***********************************************************************************************/
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
            printPreview();
        }
        /***********************************************************************************************/
        private void printPreview()
        {
            if (this.components == null)
                this.components = new System.ComponentModel.Container();

            DevExpress.XtraPrinting.PrintingSystem printingSystem1 = new DevExpress.XtraPrinting.PrintingSystem(this.components);
            DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1 = new DevExpress.XtraPrinting.PrintableComponentLink(this.components);

            printingSystem1.PageSettingsChanged += PrintingSystem1_PageSettingsChanged;

            printingSystem1.Links.AddRange(new object[] {
            printableComponentLink1});

            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else
                printableComponentLink1.Component = dgv;

            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 10, 80, 50);

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
        private void PrintingSystem1_PageSettingsChanged(object sender, EventArgs e)
        {
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

            if (dgv2.Visible)
                printableComponentLink1.Component = dgv2;
            else if (dgv3.Visible)
                printableComponentLink1.Component = dgv3;
            else
                printableComponentLink1.Component = dgv;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(10, 10, 80, 50);

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
            Printer.DrawQuad(6, 8, 7, 4, "Extended Data Layout", Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

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
        private void repositoryItemCheckEdit1_CheckedChanged(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string dbfield = "";
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["track"].ObjToString();
                if (set == "0")
                {
                    dbfield = dr["dbfield"].ObjToString();
                    if (String.IsNullOrWhiteSpace(dbfield))
                        return; // Nothing there yet
                    string cmd = "Select * from `tracking` where `tracking` = '" + dbfield + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        dr["track"] = "1";
                        return; // Already in Database
                    }
                    string selection = SelectUsing(dbfield);
                    if (String.IsNullOrWhiteSpace(selection))
                        return;
                    dr["track"] = "1";
                    dgv.RefreshDataSource();
                    record = G1.create_record("tracking", "tracking", "-1");
                    if (G1.BadRecord("tracking", record))
                        return;
                    G1.update_db_table("tracking", "record", record, new string[] { "tracking", dbfield, "using", selection });
                }
                else
                {
                    dr["track"] = "0";
                    dbfield = dr["dbfield"].ObjToString();
                    if (String.IsNullOrWhiteSpace(dbfield))
                        return; // Nothing there yet
                    string cmd = "Select * from `tracking` where `tracking` = '" + dbfield + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count <= 0)
                        return; // Didn't find in Database
                    record = dx.Rows[0]["record"].ObjToString();
                    G1.delete_db_table("tracking", "record", record);
//                    G1.update_db_table("contracts", "record", record, new string[] { "SetAsDBR", "N" });
                }
            }
        }
        /***********************************************************************************************/
        private string SelectUsing ( string dbfield)
        {
            string cmd = "Select * from `tracking` GROUP by `using` ORDER BY `using`;";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return dbfield;
            string list = "";
            string field = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                field = dx.Rows[i]["using"].ObjToString();
                if (String.IsNullOrWhiteSpace(field))
                    field = dx.Rows[i]["tracking"].ObjToString();
                if (list.Contains(field))
                    continue;
                list += field + "\n";
            }
            if (String.IsNullOrWhiteSpace(list))
                return dbfield;
            list = list.TrimEnd('\n');
            string selection = "";
            using (ListSelect listForm = new ListSelect(list, false))
            {
                listForm.Text = "Track What?";
                listForm.ShowDialog();
                selection = ListSelect.list_detail;
            }
            if (String.IsNullOrWhiteSpace(selection))
                return dbfield;
            return selection;
        }
        /***********************************************************************************************/
        RepositoryItemComboBox ciLookup = null;
        /***********************************************************************************************/
        private void gridMain2_ShownEditor(object sender, EventArgs e)
        {
            if (ciLookup == null)
            {
                ciLookup = new RepositoryItemComboBox();
                ciLookup.SelectedIndexChanged += repositoryItemComboBox1_SelectedIndexChanged;
            }
            GridColumn currCol = gridMain2.FocusedColumn;
            string currentColumn = currCol.FieldName;
            if (currentColumn.ToUpper() != "DATA")
            {
                gridMain2.Columns["data"].ColumnEdit = null;
                return;
            }
            try
            {
                DataRow dr = gridMain2.GetFocusedDataRow();
                string dbField = dr["dbfield"].ObjToString();
                string help = dr["reference"].ObjToString();
                if (String.IsNullOrWhiteSpace(help))
                {
                    gridMain2.Columns["data"].ColumnEdit = null;
                    return;
                }
                string cmd = help.Substring(0, 1);
                if (cmd != "$")
                {
                    gridMain2.Columns["data"].ColumnEdit = null;
                    return;
                }
                DataTable dt = (DataTable)dgv2.DataSource;
                int focusedRow = gridMain2.FocusedRowHandle;
                int row = gridMain2.GetDataSourceRowIndex(focusedRow);
                gridMain2.ClearSelection();
                gridMain2.SelectRow(focusedRow);
                gridMain2.FocusedRowHandle = row;
                string[] Lines = help.Split('=');
                if (Lines.Length < 2)
                    return;
                string db = Lines[0];
                db = db.Replace("$", "");
                cmd = "Select * from `" + db + "`;";
                string field = Lines[1];
                DataTable dd = G1.get_db_data(cmd);

                ciLookup.Items.Clear();
                for (int i = 0; i < dd.Rows.Count; i++)
                    ciLookup.Items.Add(dd.Rows[i][field].ObjToString());
                gridMain2.Columns["data"].ColumnEdit = ciLookup;
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        { // Selection Changed
            try
            {
                DataTable dt = (DataTable)dgv2.DataSource;
                DataRow dr = gridMain2.GetFocusedDataRow();
                int rowhandle = gridMain2.FocusedRowHandle;
                int row = gridMain2.GetDataSourceRowIndex(rowhandle);

                ComboBoxEdit combo = (ComboBoxEdit)sender;
                string answer = combo.Text.Trim();
                dr["data"] = answer;
            }
            catch ( Exception ex)
            {
            }
            //string location = dr["location"].ObjToString();
            //if (!String.IsNullOrWhiteSpace(location))
            //{
            //    try
            //    {
            //        string bankAccount = GetDepositBankAccount(paymentType, location);
            //        if (!String.IsNullOrWhiteSpace(bankAccount))
            //        {
            //            dr["bankAccount"] = bankAccount;
            //            dt.Rows[row]["bankAccount"] = bankAccount;
            //            dgv.RefreshDataSource();
            //            dgv.Refresh();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}
        }
        /***********************************************************************************************/
        private void addEditReferenceTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string reference = dr["reference"].ObjToString();
            bool protect = false;
            if (!String.IsNullOrWhiteSpace(reference))
            {
                if (reference.IndexOf("/") > 0)
                    protect = true;
                string[] Lines = reference.Split('=');
                reference = Lines[0];
                reference = reference.Replace("$", "");
                reference = Regex.Replace(reference, "ref_", "", RegexOptions.IgnoreCase);
            }

            using (AddEditReferenceTable refForm = new AddEditReferenceTable(record, reference, protect))
            {
                DialogResult result = refForm.ShowDialog();
                if ( result == DialogResult.Yes )
                {
                    string cmd = "Select * from `cust_extended_layout` where `record` = '" + record + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        reference = dx.Rows[0]["reference"].ObjToString();
                        dr["mod"] = "Y";
                        dr["reference"] = reference;
                        btnSave.Show();
                        modified = true;
                    }
                }
            }
        }
        /***********************************************************************************************/
        private void gridMain_DoubleClick(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string tracking = dr["track"].ObjToString();
            if (tracking != "1")
                return;
            string dbField = dr["dbfield"].ObjToString();
            if (String.IsNullOrWhiteSpace(dbField))
                return;
            EditTracking trackForm = new EditTracking(dbField, EditCust.activeFuneralHomeName);
            trackForm.Show();
        }
        /***********************************************************************************************/
        private void BuildDecode()
        {
            string cmd = "Select * from `cust_extended_layout` ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("ODB");
            dt.Columns.Add("track");
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                str = dt.Rows[i]["dbfield"].ObjToString();
                dt.Rows[i]["ODB"] = str; // Original Database Field
            }

            cmd = "Select * from `structures`;";
            DataTable dx = G1.get_db_data(cmd);

            string dbField = "";
            DataRow[] dRows = null;
            DataTable tempDt = null;

            string forms = "";
            string form = "";

            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dbField = dt.Rows[i]["dbField"].ObjToString();
                dRows = dx.Select("dbfield LIKE '%" + dbField + "%'");
                if ( dRows.Length > 0 )
                {
                    forms = "";
                    tempDt = dRows.CopyToDataTable();
                    for ( int j=0; j< tempDt.Rows.Count; j++)
                    {
                        form = tempDt.Rows[j]["form"].ObjToString();
                        if (String.IsNullOrWhiteSpace(forms))
                            forms = form + "~";
                        else
                        {
                            if (!forms.Contains(form))
                                forms += form + "~";
                        }
                    }
                    forms.TrimEnd('~');
                    dt.Rows[i]["help"] = forms;
                }
            }
            G1.NumberDataTable(dt);
            dgv3.DataSource = dt;
        }
        /***********************************************************************************************/
        private void gridMain3_CalcRowHeight(object sender, RowHeightEventArgs e)
        {
            GridView View = sender as GridView;
            if (e.RowHandle >= 0)
            {
                int maxHeight = 0;

                int newHeight = 0;
                bool doit = false;
                string name = "";
                foreach (GridColumn column in gridMain3.Columns)
                {
                    name = column.FieldName.ToUpper();
                    if (name == "HELP" )
                        doit = true;
                    if (doit)
                    {
                        using (RepositoryItemMemoEdit edit = new RepositoryItemMemoEdit())
                        {
                            using (MemoEditViewInfo viewInfo = edit.CreateViewInfo() as MemoEditViewInfo)
                            {
                                viewInfo.EditValue = gridMain3.GetRowCellValue(e.RowHandle, column.FieldName);
                                viewInfo.Bounds = new Rectangle(0, 0, column.VisibleWidth, dgv3.Height);
                                using (Graphics graphics = dgv.CreateGraphics())
                                using (GraphicsCache cache = new GraphicsCache(graphics))
                                {
                                    viewInfo.CalcViewInfo(graphics);
                                    var height = ((IHeightAdaptable)viewInfo).CalcHeight(cache, column.VisibleWidth);
                                    newHeight = Math.Max(height, maxHeight);
                                    if (newHeight > maxHeight)
                                        maxHeight = newHeight;
                                }
                            }
                        }
                    }
                }

                if (maxHeight > 0)
                    e.RowHeight = maxHeight;
            }
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            G1.SpyGlass(gridMain);
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit4_CheckedChanged(object sender, EventArgs e)
        { // Dropdown Only
            DevExpress.XtraEditors.CheckEdit menu = (DevExpress.XtraEditors.CheckEdit)sender;

            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string dbfield = "";
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["track"].ObjToString();
                if (set == "0")
                    return;// Tracking must be set to "1" first

                dbfield = dr["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(dbfield))
                    return; // Nothing there yet

                set = "0";
                if (menu.Checked)
                    set = "1";

                string cmd = "Select * from `tracking` where `tracking` = '" + dbfield + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    dr["dropOnly"] = set;
                    G1.update_db_table("tracking", "record", record, new string[] { "dropOnly", set });
                    return; // Already in Database
                }
            }
        }
        /***********************************************************************************************/
        private void repositoryItemCheckEdit5_CheckedChanged(object sender, EventArgs e)
        { // Add to Contacts
            DevExpress.XtraEditors.CheckEdit menu = (DevExpress.XtraEditors.CheckEdit)sender;

            DataRow dr = gridMain.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string dbfield = "";
            if (!String.IsNullOrWhiteSpace(record))
            {
                string set = dr["track"].ObjToString();
                if (set == "0")
                    return; // Tracking must be set to "1" first

                dbfield = dr["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(dbfield))
                    return; // Nothing there yet

                set = "0";
                if (menu.Checked)
                    set = "1";

                string cmd = "Select * from `tracking` where `tracking` = '" + dbfield + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    record = dx.Rows[0]["record"].ObjToString();
                    dr["addContact"] = set;
                    G1.update_db_table("tracking", "record", record, new string[] { "addContact", set });
                    return; // Already in Database
                }
            }
        }
        /***********************************************************************************************/
        private void showUsageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain.GetFocusedDataRow();
            string dbField = dr["dbField"].ObjToString();

            //string cmd = "SELECT * FROM cust_extended_layout WHERE dbfield LIKE '%" + dbField + "%'";
            //DataTable dt = G1.get_db_data(cmd);

            string cmd = "SELECT * FROM `structures` WHERE `dbfield` LIKE '%" + dbField + "%'";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count <= 0 )
            {
                MessageBox.Show("This field is not assigned to any forms!", "Forms Search Dialog", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return;
            }
            using (ViewDataTable viewForm = new ViewDataTable(dt, "field, location, form"))
            {
                viewForm.Text = "Forms Containing " + dbField;
                //viewForm.TopMost = true;
                viewForm.ManualDone += ViewForm_ManualDone;
                viewForm.ShowDialog();
            }
        }
        /***********************************************************************************************/
        private void ViewForm_ManualDone(DataTable dd, DataRow dx)
        {
            //DataTable dt = (DataTable)dgv.DataSource;

            string location = dx["location"].ObjToString();
            string formName = dx["form"].ObjToString();

            string cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "' and `location` = '" + location + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;

            //string formName = dr["formName"].ObjToString();
            //string location = dr["location"].ObjToString();
            string record = dt.Rows[0]["record"].ObjToString();
            string type = dt.Rows[0]["type"].ObjToString();
            if (!String.IsNullOrWhiteSpace(formName) && String.IsNullOrWhiteSpace(type))
            {
                string str = G1.get_db_blob("arrangementforms", record, "image");
                byte[] b = Encoding.ASCII.GetBytes(str);

                this.Cursor = Cursors.WaitCursor;
                ArrangementForms aForm = new ArrangementForms(formName, location, record, "", b);
                aForm.Show();
                this.Cursor = Cursors.Default;
            }
        }
        /***********************************************************************************************/
    }
}