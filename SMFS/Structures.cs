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
using org.apache.pdfbox.util;
using iTextSharp.text.pdf;

/***********************************************************************************************/
namespace SMFS

{
    /***********************************************************************************************/
    public partial class Structures : DevExpress.XtraEditors.XtraForm
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
        public Structures(string formName, string type, string record )
        {
            InitializeComponent();
            workFormName = formName;
            workType = type;
            workRecord = record;
        }
        /***********************************************************************************************/
        private void Structures_Load(object sender, EventArgs e)
        {
            this.Text = "Edit Structure for form " + workFormName;
//            string cmd = "Select * from `arrangementforms` where `formName` = '" + workFormName + "';";
            string cmd = "Select * from `arrangementforms` where `record` = '" + workRecord + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return;
            string record = dt.Rows[0]["record"].ObjToString();
            workLocation = dt.Rows[0]["location"].ObjToString();
            string str = G1.get_db_blob("arrangementforms", record, "image");
            if (str.IndexOf("rtf1") > 0)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(str);

                MemoryStream stream = new MemoryStream(bytes);

                DevExpress.XtraRichEdit.RichEditControl rtb = new DevExpress.XtraRichEdit.RichEditControl();

                rtb.Document.Delete(rtb.Document.Range);

                rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);

                DataTable dx = ExtractFields(rtb.Document.RtfText);
                dt = LoadFields(dx, workLocation, workFormName );
                dgv.DataSource = dt;
            }
            else if (str.IndexOf("PDF") > 0)
            {
                string command = "Select `image` from `arrangementforms` where `record` = '" + record + "';";
                MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                cmd1.Connection.Open();
                try
                {
                    using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                    {
                        if (dR.Read())
                        {
                            byte[] fileData = (byte[])dR.GetValue(0);
                            dt = LoadUpPDF(fileData, workLocation, workFormName);
                            dgv.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            btnLoad.Hide();
        }
        /***********************************************************************************************/
        public static DataTable LoadUpPDF ( byte [] b, string workLocation, string workFormName )
        {
            DataTable dx = new DataTable();
            dx.Columns.Add("field");
            dx.Columns.Add("mod");
            dx.Columns.Add("status");

            //string pdfRecord = dt.Rows[0]["record"].ObjToString();
            //string str = G1.get_db_blob("pdfimages", pdfRecord, "image");
            //byte[] b = Encoding.ASCII.GetBytes(str);

            string str = "";
            iTextSharp.text.pdf.AcroFields fields = null;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                //creating a sample Document
                iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30f, 30f, 30f, 30f);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();

                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(b);


                StringBuilder sb = new StringBuilder();
                var formFields = reader.AcroFields;
                AcroFields.Item item = formFields.GetFieldItem("Dec Race Checkbox");
                if (item != null)
                {
                    PdfDictionary mergedDict = item.GetMerged(0); // which isn't documented in the online docs at the above URLs.  Bruno?
                    PdfDictionary valueDict = item.GetValue(0);
                }

                foreach (var de in reader.AcroFields.Fields)
                {
                    sb.Append(de.Key.ToString() + Environment.NewLine);
                }
                str = sb.ToString();
                str = str.Replace("\r", "");
                string[] Lines = str.Split('\n');

                iTextSharp.text.pdf.PdfStamper pdfStamper = new iTextSharp.text.pdf.PdfStamper(reader, mo);
                fields = pdfStamper.AcroFields;

                for (int i = 0; i < Lines.Length; i++)
                {
                    string name = Lines[i].Trim();
                    DataRow dRow = dx.NewRow();
                    dRow["field"] = name;
                    dx.Rows.Add(dRow);
                }
                pdfStamper.Close();
                reader.Close();
            }
            DataTable dt = LoadFields(dx, workLocation, workFormName);

            if (G1.get_column_number(dt, "options") < 0)
                dt.Columns.Add("options");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["field"].ObjToString();
                String[] values = fields.GetAppearanceStates(name);
                if (values != null)
                {
                    str = "";
                    for (int j = 0; j < values.Length; j++)
                        str += values[j].Trim() + "~";
                    dt.Rows[i]["options"] = str;
                }
            }

            //            dgv.DataSource = ddt;
            return dt;
        }
       /***********************************************************************************************/
        public static DataTable LoadFields ( DataTable dx, string workLocation, string workFormName )
        {
            if (workLocation.Trim().ToUpper() == "GENERAL")
                workLocation = "";
            string cmd = "Select * from `structures` where `form` = '" + workFormName + "' ";
            cmd += " AND `location` = '" + workLocation + "' ";
            //if ( !String.IsNullOrWhiteSpace ( workLocation ))
            //    cmd += " AND `location` = '" + workLocation + "' ";
            cmd += " order by `order`;";
            DataTable dt = null;
            bool gotSearch = false;
            if (G1.get_column_number(dx, "search") >= 0)
                gotSearch = true;
            try
            {
                dt = G1.get_db_data(cmd);
                dt.Columns.Add("num");
                dt.Columns.Add("mod");
                dt.Columns.Add("F1");
                dt.Columns.Add("F2");
                dt.Columns.Add("search");
                DataRow[] dRows = null;
                string field = "";
                string location = "";
                string str = "";
                int locCol = G1.get_column_number(dx, "location");
                for (int i = 0; i < dx.Rows.Count; i++)
                {
                    field = dx.Rows[i]["field"].ObjToString();
                    str = field;
                    str = str.Replace("[*", "");
                    str = str.Replace("*]", "");
                    str = str.Replace("[%", "");
                    str = str.Replace("%]", "");
                    str = str.Replace("\'94", "");
                    str = str.Replace("\\u8221", "");
                    str = str.Replace("\\", "");
                    str = Structures.cleanupField(str);

                    if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                    {
                        field = field.Replace("[%", "");
                        field = field.Replace("%]", "");
                        field = field.Replace("\'94", "");
                        field = field.Replace("\\u8221", "");
                        field = field.Replace("\\", "");
                    }
                    if (str.ToUpper().IndexOf("END_TABLE") >= 0)
                    {
                        str = str.Replace("END_TABLE", "~END_TABLE");
                    }
                    location = "";
                    if (locCol >= 0)
                        location = dx.Rows[i]["location"].ObjToString();
                    dRows = dt.Select("field='" + str + "'");
                    if (dRows.Length <= 0)
                    {
                        DataRow dR = dt.NewRow();
                        dR["form"] = workFormName;
                        dR["field"] = field;
                        dR["field"] = str;
                        dR["location"] = location;
                        dR["F1"] = field;
                        if (gotSearch)
                            dR["search"] = dx.Rows[i]["search"].ObjToString();
                        dt.Rows.Add(dR);
                    }
                    else
                    {
                        dRows[0]["F1"] = field;
                        dRows[0]["F1"] = str;
                        if ( gotSearch )
                            dRows[0]["search"] = dx.Rows[i]["search"].ObjToString();
                    }
                }
            }
            catch ( Exception ex)
            {
            }
            G1.NumberDataTable(dt);
            //            dgv.DataSource = dt;
            return dt;
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
                if (data != "STRING" && data != "NUMERIC" && data != "DATE" && data != "INT" && data != "DOUBLE" && data != "TEXT" )
                {
                    MessageBox.Show("***ERROR*** Data Type must be either 'string,numeric,date,int, or double'.");
                    dr["type"] = "";
                    return;
                }
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "TABLE")
            {
                string table = dr["Table"].ObjToString();
                LoadDBTableFields(table, repositoryItemComboBox4);
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

            //if (table.ToUpper() == "CUSTOMERS")
            //    table = "fcustomers";
            //else if (table.ToUpper() == "CONTRACTS")
            //    table = "fcontracts";
            //else if (table.ToUpper() == "CUST_EXTENDED")
            //    table = "fcust_extended";

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
            DataTable tempDb = new DataTable();
            tempDb.Columns.Add("field");
            DataRow dR = null;
            string name = "";
            for (int i = 0; i < rx.Rows.Count; i++)
            {
                name = rx.Rows[i]["field"].ToString().Trim();
                if (name.Trim().ToUpper() == "TMSTAMP")
                    continue;
                else if (name.Trim().ToUpper() == "RECORD")
                    continue;
                combo.Items.Add(name);
                dR = tempDb.NewRow();
                dR["field"] = name;
                tempDb.Rows.Add(dR);

            }
            DataView tempview = tempDb.DefaultView;
            tempview.Sort = "field asc";
            tempDb = tempview.ToTable();

            combo.Items.Clear();
            for (int i = 0; i < tempDb.Rows.Count; i++)
                combo.Items.Add(tempDb.Rows[i]["field"].ObjToString());
            if (!found)
            {
                DBTable[DBTableCount] = table;
                DBTables[DBTableCount] = rx;
                DBTableCount++;
            }
        }
        /***********************************************************************************************/
        private void Structures_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!modified)
                return;
            DialogResult result = MessageBox.Show("***Question***\nOptions have been modified!\nWould you like to save your changes?", "Add/Edit Options Dialog", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
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
            SaveStructure(dt, workFormName, workLocation );
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        public static void SaveStructure ( DataTable dt, string workFormName, string location )
        {
            string record = "";
            string field = "";
            string type = "";
            string moreOptions = "";
            string length = "";
            string user = LoginForm.username;
            string olduser = "";
            DateTime now = DateTime.Now;
            string added = G1.DateTimeToSQLDateTime(now);
            string qualify = "";
            string help = "";
            string status = "";
            string table = "";
            string dbfield = "";

            string cmd = "Select * from `structures` where `form` = '" + workFormName + "' ";
            cmd += " AND `location` = '" + location + "' ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            dx.Columns.Add("DoesExist");

            string mod = "";
            DataRow[] dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["mod"].ObjToString();
                //if (mod != "Y")
                //    continue;
                record = dt.Rows[i]["record"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( record ))
                {
                    dR = dx.Select("record='" + record + "'");
                    if (dR.Length > 0)
                        dR[0]["DoesExist"] = "Y";
                }
                if (mod == "D")
                {
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("structures", "record", record);
                    continue;
                }
                field = dt.Rows[i]["field"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                moreOptions = dt.Rows[i]["more_options"].ObjToString();
                length = dt.Rows[i]["length"].ObjToString();
                qualify = dt.Rows[i]["qualify"].ObjToString();
                olduser = dt.Rows[i]["user"].ObjToString();
                help = dt.Rows[i]["help"].ObjToString();
                status = dt.Rows[i]["status"].ObjToString();
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(table))
                    table = "XXXX";
                if ( table.ToUpper() == "RELATIVES")
                {

                }
                if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                {
                    field = field.Replace("[%", "");
                    field = field.Replace("%]", "");
                    field = field.Replace("\'94", "");
                    field = field.Replace("\\u8221", "");
                    field = field.Replace("\\", "");
                }
                if (String.IsNullOrWhiteSpace(record))
                    record = "-1";
                if (record == "-1")
                    record = G1.create_record("structures", "field", "-1");
                if (G1.BadRecord("structures", record))
                    continue;
                G1.update_db_table("structures", "record", record, new string[] { "table", table, "field", field, "dbfield", dbfield, "type", type, "length", length, "qualify", qualify, "help", help, "status", status, "form", workFormName, "location", location, "more_options", moreOptions, "order", i.ToString() });
                if (String.IsNullOrWhiteSpace(olduser))
                    G1.update_db_table("structures", "record", record, new string[] { "dateAdded", added, "user", user });
                dt.Rows[i]["mod"] = "";
                dt.Rows[i]["record"] = record;
            }

            string str = "";
            for ( int i=0; i<dx.Rows.Count; i++)
            {
                str = dx.Rows[i]["DoesExist"].ObjToString();
                if ( str.ToUpper() != "Y")
                {
                    record = dx.Rows[i]["record"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(record))
                        G1.delete_db_table("structures", "record", record);
                }
            }
        }
        /***********************************************************************************************/
        private void picAdd_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            DataRow dRow = dt.NewRow();
            dt.Rows.Add(dRow);
            G1.NumberDataTable(dt);
            dgv.DataSource = dt;
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
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR 1*** Adding New Field " + dbfield + " for Table " + table + " " + ex.Message.ToString());
                    }
                }
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR 2*** Adding New Field " + dbfield + " for Table " + table + " " + ex.Message.ToString());
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
        public static bool TieDbTable(string tablename, DataTable dt)
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
                int limit = dt.Columns.Count;
                for (int i = 0; i < limit; i++)
                {
                    row = i;
                    bool found = false;
                    original = dt.Columns[i].ColumnName.Trim();
                    name = original.ToUpper();
                    type = dt.Columns[i].DataType.ObjToString().ToUpper();
                    length = "100";
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
                        else if (type == "SYSTEM.DECIMAL")
                            newstr = "alter table `" + tablename + "` add `" + original + "` DECIMAL (10,2) NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.INT32")
                            newstr = "alter table `" + tablename + "` add `" + original + "` INT NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.INT64")
                            newstr = "alter table `" + tablename + "` add `" + original + "` BIGINT NOT NULL DEFAULT '0' ;";
                        else if (type == "SYSTEM.DATE")
                            newstr = "alter table `" + tablename + "` add `" + original + "` DATE NOT NULL DEFAULT '0000-00-00' ;";
                        else if (type == "SYSTEM.DATETIME")
                            newstr = "alter table `" + tablename + "` add `" + original + "` DATETIME NOT NULL DEFAULT '0000-00-00' ;";
                        else if (type.ToUpper().IndexOf("SYSTEM.BYTE") >= 0)
                            newstr = "alter table `" + tablename + "` add `" + original + "` BLOB ;";
                        else if (type == "MYSQL.DATA.TYPES.MYSQLDATETIME")
                        {
//                            newstr = "alter table `" + tablename + "` add `" + original + "` VARCHAR (" + length + ") NOT NULL DEFAULT '' ;";
                            newstr = "alter table `" + tablename + "` add `" + original + "` DATE NOT NULL DEFAULT '0000-00-00' ;";
                        }
                        try
                        {
                            if (String.IsNullOrWhiteSpace(newstr))
                            {
                                MessageBox.Show("***ERROR*** Invalid DataType for field " + original + "!");
                                break;
                            }
                            DataTable ddx = G1.get_db_data(newstr);
                        }
                        catch (Exception ex)
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
            DataTable dt = (DataTable)dgv.DataSource;
            string record = "";
            int row = 0;
            int[] Rows = gridMain.GetSelectedRows();

            int firstRow = 0;
            if (Rows.Length > 0)
                firstRow = Rows[0];

            for ( int i=0; i<Rows.Length; i++)
            {
                row = Rows[i];
                DataRow dr = gridMain.GetDataRow(row);
                record = dr["record"].ToString();
                G1.delete_db_table("structures", "record", record);
                dt.Rows.Remove(dr);
            }

            //Structures_Load(null, null);

            if (firstRow > (dt.Rows.Count - 1))
                firstRow = (dt.Rows.Count - 1);
            dgv.DataSource = dt;
            gridMain.RefreshData();
            dgv.Refresh();

            gridMain.FocusedRowHandle = firstRow;
            gridMain.SelectRow(firstRow);
        }
        /***********************************************************************************************/
        private void gridMain_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv.DataSource;
            string delete = dt.Rows[row]["mod"].ObjToString();
            if (delete.ToUpper() == "D")
            {
                e.Visible = false;
                e.Handled = true;
            }
            if ( chkActiveOnly.Checked )
            {
                string active = dt.Rows[row]["status"].ObjToString();
                if ( active.ToUpper() == "INACTIVE")
                {
                    e.Visible = false;
                    e.Handled = true;
                }
            }
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
            modified = false;
            btnSave.Hide();
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
            modified = false;
            btnSave.Hide();
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
            if (rowHandle == (dt.Rows.Count - 1))
                return; // Already at the last row
            DataRow dRow = dt.NewRow();
            dRow["mod"] = "Y";
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
            btnSave.Show();
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
        private void chkActiveOnly_CheckedChanged(object sender, EventArgs e)
        {
            gridMain.RefreshData();
            dgv.Refresh();
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
        public static DataTable ExtractFields(string text)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("num");
            dt.Columns.Add("record");
            dt.Columns.Add("table");
            dt.Columns.Add("field");
            dt.Columns.Add("mod");
            dt.Columns.Add("modData");
            dt.Columns.Add("modDBField");
            dt.Columns.Add("status");
            dt.Columns.Add("data");
            dt.Columns.Add("type");
            dt.Columns.Add("help");
            dt.Columns.Add("user");
            dt.Columns.Add("dbfield");
            dt.Columns.Add("length", Type.GetType("System.Int32"));
            dt.Columns.Add("lookup");
            dt.Columns.Add("search");

            DataRow[] dR = null;

            string lines = Structures.ParseFields(text, "[*");
            string[] Lines = lines.Split('\n');
            string str = "";
            string field = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                field = Lines[i];
                str = field;
                str = str.Replace("[*", "");
                str = str.Replace("*]", "");
                str = str.Replace("[%", "");
                str = str.Replace("%]", "");
                str = str.Replace("\'94", "");
                str = str.Replace("\\u8221", "");
                str = str.Replace("\\", "");
                str = Structures.cleanupField(str);
                if (!String.IsNullOrWhiteSpace(str))
                {
                    dR = dt.Select("lookup='" + str + "'");
                    if (dR.Length <= 0)
                    {
                        DataRow dRow = dt.NewRow();
                        dRow["field"] = field;
                        dRow["lookup"] = str;
                        dt.Rows.Add(dRow);
                    }
                }
            }
            lines = Structures.ParseFields(text, "[%");
            Lines = lines.Split('\n');
            str = "";
            bool holding = false;
            string hold = "";
            string searchHold = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                field = Lines[i];
                if (String.IsNullOrWhiteSpace(field))
                    continue;
                str = field;
                str = str.Replace("[*", "");
                str = str.Replace("*]", "");
                str = str.Replace("[%", "");
                str = str.Replace("%]", "");
                str = str.Replace("\'94", "");
                str = str.Replace("\'96", "");
                str = str.Replace("\\u8221", "");
                str = str.Replace("\\", "");
                str = Structures.cleanupField(str);
                if ( str.ToUpper().IndexOf ( "BEGIN_TABLE") >= 0 )
                {
                    holding = true;
                    hold = str + "~";
                    searchHold = Lines[i];
                    continue;
                }
                if (str.ToUpper().IndexOf("END_TABLE") >= 0 && holding )
                {
                    holding = false;
                    hold += str;
                    searchHold += Lines[i];
                    str = hold;
                    field = searchHold;
                }
                if ( holding )
                {
                    hold += str + "~";
                    searchHold += Lines[i];
                    continue;
                }
                if (!String.IsNullOrWhiteSpace(str))
                {
                    dR = dt.Select("lookup='" + str + "'");
                    if (dR.Length <= 0)
                    {
                        DataRow dRow = dt.NewRow();
                        dRow["field"] = field;
                        dRow["lookup"] = str;
                        dRow["search"] = searchHold;
                        dt.Rows.Add(dRow);
                        searchHold = "";
                    }
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        //private DataTable ExtractFields( string text )
        //{
        //    DataTable dt = new DataTable();
        //    dt.Columns.Add("fields");
        //    dt.Columns.Add("mod");
        //    dt.Columns.Add("status");

        //    string lines = ParseFields(text, "[*");
        //    string[] Lines = lines.Split('\n');
        //    string str = "";
        //    for (int i = 0; i < Lines.Length; i++)
        //    {
        //        str = Lines[i];
        //        if (!String.IsNullOrWhiteSpace(str))
        //        {
        //            str = str.Replace("[*", "");
        //            str = str.Replace("*]", "");
        //            DataRow dRow = dt.NewRow();
        //            dRow["fields"] = str;
        //            dt.Rows.Add(dRow);
        //        }
        //    }
        //    lines = ParseFields(text, "[%");
        //    Lines = lines.Split('\n');
        //    str = "";
        //    for (int i = 0; i < Lines.Length; i++)
        //    {
        //        str = Lines[i];
        //        if (!String.IsNullOrWhiteSpace(str))
        //        {
        //            str = str.Replace("[%", "");
        //            str = str.Replace("%]", "");
        //            str = cleanupField(str);
        //            DataRow dRow = dt.NewRow();
        //            dRow["fields"] = str;
        //            dt.Rows.Add(dRow);
        //        }
        //    }
        //    return dt;
        //}
        /***********************************************************************************************/
        public static string cleanupField ( string field )
        {
            string str = "";
            field = field.Replace("\'94", "");
            field = field.Replace("\'96", "");
            for ( int i=field.Length-1; i>=0; i--)
            {
                str = field.Substring(i, 1);
                if ( str == "%")
                {
                    field = field.Substring(0, i);
                    break;
                }
            }
            return field;
        }
        /***********************************************************************************************/
        public static string ParseFields(string text, string field)
        {
            int idx = -1;
            string str = "";
            string lines = "";
            string saveField = "";
            int position = 0;
            for (;;)
            {
                idx = text.IndexOf(field);
                if (idx < 0)
                {
                    if (!String.IsNullOrWhiteSpace(saveField))
                        lines += saveField + "\n";
                    break;
                }
                saveField = "";
                for (int i = idx; i < text.Length; i++)
                {
                    str = text.Substring(i, 1);
                    if (str == "]")
                    {
                        if ( saveField.ToUpper().IndexOf ( "BEGIN_TABLE") >= 0 )
                        {

                        }
                        saveField += "]";
                        lines += saveField + "\n";
                        saveField = "";
                        position = idx;
                        if (idx + field.Length >= text.Length)
                        {
                            return lines;
                        }
                        text = text.Substring(idx + field.Length);
                        break;
                    }
                    else
                        saveField += str;
                }
            }
            return lines;
        }
        /***********************************************************************************************/
        public static DataTable ParseRTF(string text, string field)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("field");
            dt.Columns.Add("position", Type.GetType("System.Int32"));
            int idx = -1;
            string str = "";
            string lines = "";
            string saveField = "";
            int position = 0;
            for (;;)
            {
                idx = text.IndexOf(field);
                if (idx < 0)
                {
                    if (!String.IsNullOrWhiteSpace(saveField))
                    {
                        lines += saveField + "\n";
                        DataRow dRow = dt.NewRow();
                        dRow["field"] = saveField;
                        dRow["position"] = position;
                        dt.Rows.Add(dRow);
                    }
                    break;
                }
                saveField = "";
                for (int i = idx; i < text.Length; i++)
                {
                    str = text.Substring(i, 1);
                    if (str == "]")
                    {
                        position += idx;
                        saveField += "]";
                        lines += saveField + "\n";
                        DataRow dRow = dt.NewRow();
                        dRow["field"] = saveField;
                        dRow["position"] = position;
                        dt.Rows.Add(dRow);
                        saveField = "";
                        if (idx + field.Length >= text.Length)
                        {
                            return dt;
                        }
                        text = text.Substring(idx + field.Length);
                        position += field.Length;
                        break;
                    }
                    else
                        saveField += str;
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void btnPull_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv.DataSource;
            string field = "";
            string dbfield = "";
            string table = "";
            string cmd = "Select * from `structures` where `form` = '" + workFormName + "';";
            DataTable dx = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                table = dt.Rows[i]["table"].ObjToString();
                if (!String.IsNullOrWhiteSpace(dbfield) && !String.IsNullOrWhiteSpace(table))
                    continue;
                DataRow [] dR = dx.Select("field='" + field + "'");
                if ( dR.Length > 0 )
                {
                    dt.Rows[i]["dbfield"] = dR[0]["dbfield"].ObjToString();
                    dt.Rows[i]["table"] = dR[0]["table"].ObjToString();
                    dt.Rows[i]["qualify"] = dR[0]["qualify"].ObjToString();
                    dt.Rows[i]["form"] = workFormName;
                    dt.Rows[i]["location"] = workLocation;
                }
            }
            cmd = "Select * from `structures`;"; // Find all other fields
            dx = G1.get_db_data(cmd);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                table = dt.Rows[i]["table"].ObjToString();
                if (!String.IsNullOrWhiteSpace(dbfield) && !String.IsNullOrWhiteSpace(table))
                    continue;
                DataRow[] dR = dx.Select("field='" + field + "'");
                if (dR.Length > 0)
                {
                    dt.Rows[i]["dbfield"] = dR[0]["dbfield"].ObjToString();
                    dt.Rows[i]["table"] = dR[0]["table"].ObjToString();
                    dt.Rows[i]["qualify"] = dR[0]["qualify"].ObjToString();
                    dt.Rows[i]["form"] = workFormName;
                    dt.Rows[i]["location"] = workLocation;
                }
            }
            dgv.DataSource = dt;
            dgv.Refresh();
        }
        /***********************************************************************************************/
        private void repositoryItemComboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
        }
        /***********************************************************************************************/
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage current = (sender as TabControl).SelectedTab;
            if (current.Name.Trim().ToUpper() == "TABFIELDS")
            {
                LoadAllTables();
            }
        }
        /***********************************************************************************************/
        private void LoadAllTables ()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("table");
            dt.Columns.Add("dbfield");
            DataRow dRow = null;

            cmbTables.Items.Clear();
            cmbTables.Items.Add("All");

            string table = "";
            for ( int i=0; i<repositoryItemComboBox3.Items.Count; i++)
            {
                table = repositoryItemComboBox3.Items[i].ObjToString();
                bool found = false;
                DataTable rx = null;
                for (int k = 0; k < DBTableCount; k++)
                {
                    if (DBTable[k].ToUpper() == table.ToUpper())
                    {
                        found = true;
                        table = DBTable[k];
                        rx = (DataTable)DBTables[k];
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

                cmbTables.Items.Add(table);
                string name = "";
                for (int k = 0; k < rx.Rows.Count; k++)
                {
                    name = rx.Rows[k]["field"].ToString().Trim();
                    if (name.Trim().ToUpper() == "TMSTAMP")
                        continue;
                    dRow = dt.NewRow();
                    dRow["table"] = table;
                    dRow["dbfield"] = name;
                    dt.Rows.Add(dRow);
                }
                if (!found)
                {
                    DBTable[DBTableCount] = table;
                    DBTables[DBTableCount] = rx;
                    DBTableCount++;
                }
            }
            G1.NumberDataTable(dt);
            dgv2.DataSource = dt;
            cmbTables.Text = "All";
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain2.OptionsFind.AlwaysVisible == true)
                gridMain2.OptionsFind.AlwaysVisible = false;
            else
                gridMain2.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
        private void cmbTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            gridMain2.RefreshData();
            gridMain2.RefreshEditor(true);
            dgv2.Refresh();
        }
        /***********************************************************************************************/
        private void gridMain2_CustomRowFilter(object sender, DevExpress.XtraGrid.Views.Base.RowFilterEventArgs e)
        {
            string FilterTable = cmbTables.Text.Trim();
            if (FilterTable.ToUpper() == "ALL" || String.IsNullOrWhiteSpace ( FilterTable))
                return;

            int row = e.ListSourceRow;
            DataTable dt = (DataTable)dgv2.DataSource;

            string table = dt.Rows[row]["table"].ObjToString();
            if ( table != FilterTable)
            {
                e.Visible = false;
                e.Handled = true;
                return;
            }
        }
        /***********************************************************************************************/
    }
}