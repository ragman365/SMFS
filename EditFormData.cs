using System;
using System.Data;
using System.Text;
using System.Windows.Forms;
using GeneralLib;
using System.IO;
using DevExpress.XtraRichEdit;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Columns;
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;

using System.Drawing;
using DevExpress.XtraPrinting;
using DevExpress.Utils;
using DocumentFormat.OpenXml.Office2013.PowerPoint.Roaming;
/***********************************************************************************************/
namespace SMFS
{
    /***********************************************************************************************/
    public partial class EditFormData : DevExpress.XtraEditors.XtraForm
    {
        private string workContract = "";
        private string workForm = "";
        private string workLocation = "";
        private string workRecord = "";
        private bool modified = false;
        private int DBTableCount = 0;
        private string[] DBTable = new string[10];
        private DataTable[] DBTables = new DataTable[10];
        private bool doAll = false;
        /***********************************************************************************************/
        public EditFormData( string contractNumber, string form, string location, string record )
        {
            InitializeComponent();
            workContract = contractNumber;
            workForm = form;
            workLocation = location;
            workRecord = record;
            if (form.Trim().ToUpper() == "ALL")
                doAll = true;
            if (workLocation.Trim().ToUpper() == "ALL")
                workLocation = "";
        }
        /***********************************************************************************************/
        private void EditFormData_Load(object sender, EventArgs e)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(this.btnRight, "Push Field to DB Field");

            gridMain4.ClipboardRowPasting += GridMain4_ClipboardRowPasting;

            this.Text = "Fields and Data for Form (" + workForm + ")";
            btnSave.Hide();
            btnSaveData.Hide();
            btnSaveDBFields.Hide();
            if ( doAll )
            {
                LoadAllForms();
                return;
            }
            DataTable dx = null;
            string form = workForm;
            string record1 = "";
            string str = "";
            if ( !String.IsNullOrWhiteSpace ( workContract ))
                str = G1.get_db_blob("agreements", workRecord, "image");
            if (String.IsNullOrWhiteSpace(str))
            {
                string cmd = "Select * from `arrangementforms` where `formName` = '" + workForm + "';";
                DataTable dt = G1.get_db_data(cmd);
                if (dt.Rows.Count > 0)
                {
                    record1 = dt.Rows[0]["record"].ObjToString();
                    str = G1.get_db_blob("arrangementforms", record1, "image");
                }
            }
            if (str.IndexOf("rtf1") > 0)
            {
                byte[] bytes = Encoding.ASCII.GetBytes(str);
                MemoryStream stream = new MemoryStream(bytes);
                DevExpress.XtraRichEdit.RichEditControl rtb = new RichEditControl();
                rtb.Document.Delete(rtb.Document.Range);
                rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                //                dx = ExtractFields(rtb.Document.RtfText);
                string text1 = rtb.Document.Text;
                dx = Structures.ExtractFields(rtb.Document.RtfText);
                CleanupTable(dx);
            }
            else if (str.IndexOf("PDF") > 0)
            {
                string command = "Select `image` from `agreements` where `record` = '" + workRecord + "';";
                if (!String.IsNullOrWhiteSpace(record1))
                    command = "Select `image` from `arrangementforms` where `record` = '" + record1 + "';";
                MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
                cmd1.Connection.Open();
                try
                {
                    using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
                    {
                        if (dR.Read())
                        {
                            byte[] fileData = (byte[])dR.GetValue(0);
                            dx = LoadUpPDF(fileData);
                            //                                dx = ExtractPdfFields(fileData);
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }
            G1.NumberDataTable(dx);
            dgv4.DataSource = dx;
        }
        /***********************************************************************************************/
        private void LoadAllForms ()
        {
            string cmd = "Select * from `arrangementforms`;";
            DataTable dt = G1.get_db_data(cmd);
            string record = "";
            string formName = "";
            string type = "";
            string str = "";
            DataTable dx = null;
            DataTable mainDt = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                record = dt.Rows[i]["record"].ObjToString();
                formName = dt.Rows[i]["formName"].ObjToString();
                type = dt.Rows[i]["type"].ObjToString();
                str = G1.get_db_blob("arrangementforms", record, "image");
                if (String.IsNullOrWhiteSpace(str))
                    continue;
                if (str.IndexOf("rtf1") < 0)
                    continue;
                workForm = formName;
                workRecord = record;
                byte[] bytes = Encoding.ASCII.GetBytes(str);
                MemoryStream stream = new MemoryStream(bytes);
                DevExpress.XtraRichEdit.RichEditControl rtb = new RichEditControl();
                rtb.Document.Delete(rtb.Document.Range);
                rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf); 
                rtb.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                dx = Structures.ExtractFields(rtb.Document.RtfText);
                dx.Columns.Add("where");
                CleanupTable(dx);
                for (int j = 0; j < dx.Rows.Count; j++)
                    dx.Rows[j]["where"] = formName;
                if (mainDt == null)
                    mainDt = dx.Copy();
                else
                {
                    for (int j = 0; j < dx.Rows.Count; j++)
                        mainDt.ImportRow(dx.Rows[j]);
                }
                dx.Rows.Clear();
            }
            G1.NumberDataTable(mainDt);
            dgv4.DataSource = mainDt;
            gridMain4.Columns["where"].Visible = true;
        }
        /***********************************************************************************************/
        //        private void EditFormData_Load(object sender, EventArgs e)
        //        {
        //            gridMain4.ClipboardRowPasting += GridMain4_ClipboardRowPasting;

        //            this.Text = "Fields and Data for Form (" + workForm + ")";
        //            btnSave.Hide();
        //            btnSaveData.Hide();
        //            btnSaveDBFields.Hide();
        //            DataTable dx = null;
        //            string form = workForm;
        //            //if (!String.IsNullOrWhiteSpace(workContract))
        //            //    form = workContract + " " + workForm;

        //            //string cmd = "Select * from `pdfimages` where `filename` = '" + form + "';";
        //            //DataTable dt = G1.get_db_data(cmd);
        //            //if (dt.Rows.Count <= 0 )
        //            //{
        //            //    cmd = "Select * from `pdfimages` where `filename` = '" + workForm + "';";
        //            //    dt = G1.get_db_data(cmd);
        //            //    if (dt.Rows.Count > 0)
        //            //        form = workForm;
        //            //}

        //            string str = G1.get_db_blob("arrangements", workRecord, "image");
        //            if (String.IsNullOrWhiteSpace(str))
        //            {
        //                string cmd = "Select * from `arrangementforms` where `formName` = '" + workForm + "';";
        //                DataTable dt = G1.get_db_data(cmd);
        //                if (dt.Rows.Count > 0)
        //                {
        //                    string record1 = dt.Rows[0]["record"].ObjToString();
        //                    str = G1.get_db_blob("arrangementforms", record1, "image");
        //                }
        //            }


        //            string pdfRecord = "";
        //            if (dt.Rows.Count > 0)
        //            {
        //                pdfRecord = dt.Rows[0]["record"].ObjToString();
        //                string str = G1.get_db_blob("pdfimages", pdfRecord, "image");
        //                if (str.IndexOf("rtf1") > 0)
        //                {
        //                    byte[] bytes = Encoding.ASCII.GetBytes(str);
        //                    MemoryStream stream = new MemoryStream(bytes);
        //                    DevExpress.XtraRichEdit.RichEditControl rtb = new RichEditControl();
        //                    rtb.Document.Delete(rtb.Document.Range);
        //                    rtb.Document.LoadDocument(stream, DocumentFormat.Rtf);
        //                    dx = ExtractFields(rtb.Document.RtfText);
        //                    CleanupTable(dx);
        //                }
        //                else if (str.IndexOf("PDF") > 0)
        //                {
        //                    string command = "Select `image` from `pdfimages` where `filename` = '" + form + "';";
        //                    MySqlCommand cmd1 = new MySqlCommand(command, G1.conn1);
        //                    cmd1.Connection.Open();
        //                    try
        //                    {
        //                        using (MySqlDataReader dR = cmd1.ExecuteReader(System.Data.CommandBehavior.Default))
        //                        {
        //                            if (dR.Read())
        //                            {
        //                                byte[] fileData = (byte[])dR.GetValue(0);
        //                                dx = LoadUpPDF(fileData);
        ////                                dx = ExtractPdfFields(fileData);
        //                            }
        //                        }
        //                    }
        //                    catch ( Exception ex )
        //                    {
        //                    }
        //                }
        //                G1.NumberDataTable(dx);
        //                dgv4.DataSource = dx;
        //            }
        //        }
        /***********************************************************************************************/
        private void CleanupTable ( DataTable dt )
        {
            if ( G1.get_column_number ( dt, "help") <= 0)
                dt.Columns.Add("help");
            if (G1.get_column_number(dt, "dbfield") <= 0)
                dt.Columns.Add("dbfield");
            if (G1.get_column_number(dt, "qualify") < 0)
                dt.Columns.Add("qualify");
            if (G1.get_column_number(dt, "location") < 0)
                dt.Columns.Add("location");
            string field = "";
            string location = "";
            string dbField = "";
            string table = "";
            string qualify = "";
            string help = "";
            string record = "";
            string cmd = "";
            bool gotTable = false;
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                field = dt.Rows[i]["lookup"].ObjToString();
                if (String.IsNullOrWhiteSpace(field))
                    dt.Rows.RemoveAt(i);
                else
                {
                    field = field.Replace("[*", "");
                    field = field.Replace("*]", "");
                    field = field.Replace("[%", "");
                    field = field.Replace("%]", "");
                    DataRow[] dRows = dt.Select("lookup='" + field + "'");
                    if (dRows.Length <= 0 )
                    {
                        dt.Rows[i]["field"] = "DELETE";
                        continue;
                    }
                    dt.Rows[i]["field"] = field;
                    if (!String.IsNullOrWhiteSpace(field))
                    {
                        cmd = "Select * from `structures` where `form` = '" + workForm + "' and `field` = '" + field + "';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            if (field.ToUpper() == "PB1")
                            {

                            }
                            location = dx.Rows[0]["location"].ObjToString();
                            table = dx.Rows[0]["table"].ObjToString();
                            qualify = dx.Rows[0]["qualify"].ObjToString();
                            dbField = dx.Rows[0]["dbfield"].ObjToString();
                            help = dx.Rows[0]["help"].ObjToString();
                            record = dx.Rows[0]["record"].ObjToString();
                            dt.Rows[i]["dbfield"] = dbField;
                            dt.Rows[i]["help"] = help;
                            dt.Rows[i]["location"] = location;
                            dt.Rows[i]["table"] = table;
                            dt.Rows[i]["qualify"] = qualify;
                            dt.Rows[i]["record"] = record;
                            dt.Rows[i]["length"] = dx.Rows[0]["length"].ObjToInt32();
                            if (!String.IsNullOrWhiteSpace(table) && !String.IsNullOrWhiteSpace(dbField))
                                dt.Rows[i]["data"] = GetDataField(table, dbField, field, qualify);
                        }
                    }
                }
            }
            for ( int i=dt.Rows.Count-1; i>=0; i--)
            {
                field = dt.Rows[i]["field"].ObjToString();
                if (field.ToUpper() == "DELETE")
                    dt.Rows.RemoveAt(i);
            }
        }
        /***********************************************************************************************/
        private string GetDataField(string table, string dbfield, string field = "", string qualify = "")
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return "";
            string data = "";
            string cmd = "";
            if (String.IsNullOrWhiteSpace(table))
                return data;
            if (String.IsNullOrWhiteSpace(dbfield))
                return data;
            string str = "";
            string strDelimitor = " +";
            string additional = "";
            int idx = 0;
            if ( !String.IsNullOrWhiteSpace ( qualify))
            {
                if ( !String.IsNullOrWhiteSpace ( field))
                {
                    string[] Lines = qualify.Split('=');
                    if ( Lines.Length == 2 )
                    {
                        additional = "`" + Lines[0] + "` = '" + Lines[1] + "'";
                        idx = G1.StripNumeric(field);
                    }
                }
            }
            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + workContract + "' ";
                if (table.ToUpper() == "FUNERALHOMES")
                    cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "' ";
                if ( !String.IsNullOrWhiteSpace ( additional ))
                    cmd += " AND " + additional;
                cmd += ";";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
//                    data = dx.Rows[0][dbfield].ObjToString();
                    string[] Lines = dbfield.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int j = 0; j < Lines.Length; j++)
                    {

                        dbfield = Lines[j].Trim();
                        if (String.IsNullOrWhiteSpace(dbfield))
                            continue;
                        try
                        {
                            if (dbfield.Trim() != "+")
                            {
                                if (idx > 0)
                                    str = dx.Rows[idx - 1][dbfield].ObjToString().Trim();
                                else
                                    str = dx.Rows[0][dbfield].ObjToString().Trim();
                                data = data.Trim();
                                data += " " + str;
                            }
                        }
                        catch
                        {
                            data += dbfield;
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return data;
        }
        /***********************************************************************************************/
        private void PutDataField(string table, string dbfield, string data )
        {
            string cmd = "";
            string oldData = "";
            string record = "";
            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    oldData = dx.Rows[0][dbfield].ObjToString();
                    record = dx.Rows[0]["record"].ObjToString();
                    G1.update_db_table(table, "record", record, new string[] { dbfield, data });
                }
            }
            catch (Exception ex)
            {

            }
            return;
        }
        /***********************************************************************************************/
        private DataTable LoadUpPDF(byte[] b)
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
            dt.Columns.Add("length", Type.GetType("System.Int32"));
            dt.Columns.Add("help");
            dt.Columns.Add("dbfield");

            string str = "";
            iTextSharp.text.pdf.AcroFields fields = null;

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                //creating a sample Document
                iTextSharp.text.Document doc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 30f, 30f, 30f, 30f);

                System.IO.MemoryStream mo = new System.IO.MemoryStream();

                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(b);

                StringBuilder sb = new StringBuilder();
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
                    DataRow dRow = dt.NewRow();
                    dRow["field"] = name;
                    dt.Rows.Add(dRow);
                }
                pdfStamper.Close();
                reader.Close();
            }
            DataTable dx = LoadFields(dt);

            if (G1.get_column_number(dx, "options") < 0)
                dx.Columns.Add("options");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                string name = dt.Rows[i]["field"].ObjToString();
                String[] values = fields.GetAppearanceStates(name);
                if (values != null)
                {
                    str = "";
                    for (int j = 0; j < values.Length; j++)
                        str += values[j].Trim() + "~";
                    dx.Rows[i]["options"] = str;
                }
            }
            return dx;
        }
        /***********************************************************************************************/
        private DataTable LoadFields(DataTable dx)
        {
            string cmd = "Select * from `structures` where `form` = '" + workForm + "' order by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("modData");
            dt.Columns.Add("modDBField");
            dt.Columns.Add("data");
            DataRow[] dRows = null;
            string field = "";
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                field = dx.Rows[i]["field"].ObjToString();
                dRows = dt.Select("field='" + field + "'");
                if (dRows.Length <= 0)
                {
                    DataRow dR = dt.NewRow();
                    dR["field"] = field;
                    dt.Rows.Add(dR);
                }
            }
            dt = LoadDbFields(dt);
            G1.NumberDataTable(dt);
            return dt;
        }
        /***********************************************************************************************/
        private DataTable LoadDbFields(DataTable dt)
        {
            if (String.IsNullOrWhiteSpace(workContract))
                return dt;
            string table = "";
            string dbfield = "";
            string qualify = "";
            string data = "";
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(table))
                    continue;
                if (String.IsNullOrWhiteSpace(dbfield))
                    continue;
                qualify = dt.Rows[i]["qualify"].ObjToString();
                data = GetDbField(table, dbfield, qualify, workContract);
                dt.Rows[i]["data"] = data;
                str = dt.Rows[i]["length"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    dt.Rows[i]["length"] = "100";
                else if (str == "0")
                    dt.Rows[i]["length"] = "100";
            }
            return dt;
        }
        /***********************************************************************************************/
        private string GetDbField(string table, string field, string qualify, string contractNumber)
        {
            string data = "";
            string cmd = "";
            string str = "";
            string additional = "";
            if ( !String.IsNullOrWhiteSpace ( qualify))
            {
            }
            if ( field.ToUpper() == "DECAGE")
            {
            }
            //if (table.ToUpper() == "RELATIVES")
            //    return "";
            int idx = 0;
            if (!String.IsNullOrWhiteSpace(qualify))
            {
                if (!String.IsNullOrWhiteSpace(field))
                {
                    string[] Lines = qualify.Split('=');
                    if (Lines.Length == 2)
                    {
                        additional = "`" + Lines[0] + "` = '" + Lines[1] + "'";
                        idx = G1.StripNumeric(field);
                    }
                }
            }

            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "' ";
                if (table.ToUpper() == "FUNERALHOMES")
                    cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "' ";
                if (!String.IsNullOrWhiteSpace(additional))
                    cmd += " AND " + additional;
                cmd += ";";

                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string strDelimitor = " +";
                    string[] Lines = field.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        field = Lines[i].Trim();
                        try
                        {
                            if (field.ToUpper() == "DECAGE")
                            {
                                DateTime birthDate = dx.Rows[0]["birthDate"].ObjToDateTime();
                                DateTime deathDate = dx.Rows[0]["deceasedDate"].ObjToDateTime();
                                data = G1.GetAge(birthDate, deathDate).ToString();
                            }
                            else if (field.Trim() != "+")
                            {
                                if (idx > 0)
                                    str = dx.Rows[idx - 1][field].ObjToString().Trim();
                                else
                                    str = dx.Rows[0][field].ObjToString().Trim();
                                data = data.Trim();
                                data += " " + str;

                            }
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + field + " For ContractNumber " + contractNumber + "!!");
            }
            return data;
        }
        /***********************************************************************************************/
        public static DataTable ExtractFields( string text )
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
            dt.Columns.Add("length", Type.GetType("System.Int32"));

            string lines = Structures.ParseFields(text, "[*");
            string[] Lines = lines.Split('\n');
            string str = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                str = Lines[i];
                if (!String.IsNullOrWhiteSpace(str))
                {
                    DataRow dRow = dt.NewRow();
                    dRow["field"] = str;
                    dt.Rows.Add(dRow);
                }
            }
            lines = Structures.ParseFields(text, "[%");
            Lines = lines.Split('\n');
            str = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                str = Lines[i];
                if (!String.IsNullOrWhiteSpace(str))
                {
                    DataRow dRow = dt.NewRow();
                    dRow["field"] = str;
                    dt.Rows.Add(dRow);
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        private void gridMain4_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            if (e.Column.FieldName.Trim().ToUpper() == "TABLE")
            {
                string table = dr["Table"].ObjToString();
                string list = "";
                if (table.Trim().ToUpper() == "CUST_EXTENDED")
                    list = BuildCustExtendedList();
                LoadDBTableFields(table, repositoryItemComboBox2, list );
            }
            modified = true;
            if (e.Column.FieldName.Trim().ToUpper() == "DATA")
            {
                btnSaveData.Show();
                dr["modData"] = "Y";
            }
            else if (e.Column.FieldName.Trim().ToUpper() == "DBFIELD")
            {
                if (!LoadDBField(dr, record))
                {
                    string table = dr["table"].ObjToString();
                    string dbfield = dr["dbfield"].ObjToString();
                    //if ( !CheckFieldExists ( table, dbfield ))
                    //    btnSaveDBFields.Show();
//                    dr["modDBField"] = "Y";
//                    dr["modData"] = "Y";
                    dr["mod"] = "Y";
                    btnSave.Show();
                }
            }
            else
            {
                dr["mod"] = "Y";
                btnSave.Show();
            }
        }
        /***********************************************************************************************/
        private bool LoadDBField( DataRow dr, string record )
        {
            bool rv = false;
            string table = dr["table"].ObjToString();
            if (String.IsNullOrWhiteSpace(table) || table.Trim().ToUpper() == "XXXX")
                return false;
            string dbfield = dr["dbField"].ObjToString();
            string data = "";
            string cmd = "";
            if (String.IsNullOrWhiteSpace(table) || String.IsNullOrWhiteSpace(dbfield))
                return false;
            try
            {
                if (table.ToUpper() != "FUNERALHOMES")
                {
                    cmd = "Select * from `" + table + "` where `contractNumber` = '" + workContract + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        data = dx.Rows[0][dbfield].ObjToString();
                        dr[dbfield] = data;
                        rv = true;
                    }
                }
                else
                    rv = true;
            }
            catch (Exception ex)
            {
            }
            return rv;
        }
        /***********************************************************************************************/
        private void LoadDBTableFields(string table, RepositoryItemComboBox combo, string list )
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
            if (!String.IsNullOrWhiteSpace(list))
            {
                string[] Lines = list.Split('\n');
                for ( int j=0; j<Lines.Length; j++)
                {
                    name = Lines[j].Trim();
                    combo.Items.Add(name);
                }
            }
            else
            {
                for (int i = 0; i < rx.Rows.Count; i++)
                {
                    name = rx.Rows[i]["field"].ToString().Trim();
                    if (name.Trim().ToUpper() == "TMSTAMP")
                        continue;
                    combo.Items.Add(name);
                }
            }
            if (!found)
            {
                DBTable[DBTableCount] = table;
                DBTables[DBTableCount] = rx;
                DBTableCount++;
            }
        }
        /***********************************************************************************************/
        private void LoadDBTableFieldsx(string table, RepositoryItemComboBox combo)
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
        private void btnSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            DataTable dt = (DataTable)dgv4.DataSource;
            Structures.SaveStructure(dt, workForm, workLocation );
            modified = false;
            btnSave.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void btnSaveDBFields_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            string table = "";
            string dbfield = "";
            string mod = "";
            string strDelimitor = " +";
            DataTable dt = (DataTable)dgv4.DataSource;
            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    mod = dt.Rows[i]["modDBField"].ObjToString();
                    if (mod != "Y")
                        continue;
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
                    dt.Rows[i]["modDBField"] = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Strolling through Table " + table + " Field " + dbfield + " " + ex.Message.ToString());
            }
            modified = false;
            btnSaveDBFields.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private bool CheckFieldExists ( string table, string dbfield)
        {
            if (String.IsNullOrWhiteSpace(table))
                return true;
            if (String.IsNullOrWhiteSpace(dbfield))
                return true;
            string command = "SHOW COLUMNS FROM `" + table + "`;";
            DataTable rx = G1.get_db_data(command);
            if (rx == null || rx.Rows == null || rx.Rows.Count == 0)
                return false; // Somehow the table does not exist
            DataRow[] dRows = rx.Select("Field='" + dbfield + "'");
            if (dRows.Length <= 0)
                return false;
            return true;
        }
        /***********************************************************************************************/
        private bool verifyFieldExists(string table, string dbfield)
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
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR 2*** Adding New Field " + dbfield + " for Table " + table + " " + ex.Message.ToString());
            }

            return rv;
        }
        /***********************************************************************************************/
        private void btnSaveData_Click(object sender, EventArgs e)
        {
            if (btnSave.Visible)
                btnSave_Click(null, null);
            //if (btnSaveDBFields.Visible)
            //    btnSaveDBFields_Click(null, null);
            this.Cursor = Cursors.WaitCursor;
            string record = "";
            string field = "";
            string type = "";
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
            string data = "";

            string mod = "";
            DataTable dt = (DataTable)dgv4.DataSource;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                mod = dt.Rows[i]["modData"].ObjToString();
                if (mod != "Y")
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                field = dt.Rows[i]["field"].ObjToString();
                help = dt.Rows[i]["help"].ObjToString();
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();

                if (String.IsNullOrWhiteSpace(record))
                    continue;

                data = dt.Rows[i]["data"].ObjToString();

                PutDataField(table, dbfield, data);

                dt.Rows[i]["modData"] = "";
            }
            modified = false;
            btnSaveData.Hide();
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private void longDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string data = dr["data"].ObjToString();
            if ( G1.validate_date ( data ))
            {
                try
                {
                    DateTime date = data.ObjToDateTime();
                    data = date.ToLongDateString();
                    dr["data"] = data;
                    dr["modData"] = "Y";
                    btnSaveData.Show();
                }
                catch ( Exception ex )
                {
                }
            }
        }
        /***********************************************************************************************/
        private void shortDateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            string record = dr["record"].ObjToString();
            string data = dr["data"].ObjToString();
            try
            {
                DateTime date = data.ObjToDateTime();
                if (date.Year > 1800)
                {
                    data = date.ToString("MM/dd/yyyy");
                    dr["data"] = data;
                    dr["modData"] = "Y";
                    btnSaveData.Show();
                }
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        private void btnRight_Click(object sender, EventArgs e)
        {
            DataRow dr = gridMain4.GetFocusedDataRow();
            string field = dr["field"].ObjToString();
            if (field.Trim().ToUpper().IndexOf("BEGIN_TABLE") >= 0)
            {
                dr["qualify"] = field;
                dr["mod"] = "Y";
                btnSave.Show();
            }
            else
            {
                dr["dbfield"] = field;
                dr["modDBField"] = "Y";
//                btnSaveDBFields.Show();
            }
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
        private void GridMain4_ClipboardRowPasting(object sender, DevExpress.XtraGrid.Views.Grid.ClipboardRowPastingEventArgs e)
        {
            GridView view = sender as GridView;
            GridColumn column = view.Columns["data"];

            string pastedString = e.Values[column].ToString();
            string newString = pastedString.Replace('_', ' ');
            string newCapitalizedString = Regex.Replace(newString, @"(^\w)|(\s\w)", m => m.Value.ToUpper());
            e.Values[column] = newCapitalizedString;
        }
        /***********************************************************************************************/
        private void dgv4_ProcessGridKey(object sender, KeyEventArgs e)
        {
            //ColumnView view = (sender as GridControl).FocusedView as ColumnView;
            //if (view == null) return;
            //if (e.Control && e.KeyCode == Keys.V)
            //{
            //    DataRow dr = gridMain4.GetFocusedDataRow();
            //    string text = GetClipBoard();
            //    string str = text.ObjToString();
            //    text = str;
            //    text = text.Replace("\n", "");
            //    text = text.Replace("\r", "");
            //    dr["data"] = CleanupString(text);
            //    btnSaveData.Show();
            //    dr["modData"] = "Y";
            //    str = dr["data"].ObjToString();
            //}
        }
        /***********************************************************************************************/
        private string CleanupString ( string str )
        {
            string final = "";
            int c = 0;
            for ( int i=0; i<str.Length; i++)
            {
                c = (int)str[i];
                if (c >= 65 && c <= 122)
                    final += str.Substring(i, 1);
            }
            return final;
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


            printableComponentLink1.Component = dgv4;
            printableComponentLink1.PrintingSystemBase = printingSystem1;

            printableComponentLink1.EnablePageDialog = true;

            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

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

            printableComponentLink1.Component = dgv4;
            printableComponentLink1.PrintingSystemBase = printingSystem1;
            printableComponentLink1.CreateDetailHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateDetailHeaderArea);
            printableComponentLink1.CreateMarginalHeaderArea += new DevExpress.XtraPrinting.CreateAreaEventHandler(this.printableComponentLink1_CreateMarginalHeaderArea);
            printableComponentLink1.BeforeCreateAreas += new System.EventHandler(this.printableComponentLink1_BeforeCreateAreas);
            printableComponentLink1.AfterCreateAreas += new System.EventHandler(this.printableComponentLink1_AfterCreateAreas);
            printableComponentLink1.Landscape = true;

            Printer.setupPrinterMargins(50, 100, 80, 50);

            pageMarginLeft = Printer.pageMarginLeft;
            pageMarginRight = Printer.pageMarginRight;
            pageMarginTop = Printer.pageMarginTop;
            pageMarginBottom = Printer.pageMarginBottom;

            printableComponentLink1.Margins.Left = pageMarginLeft;
            printableComponentLink1.Margins.Right = pageMarginRight;
            printableComponentLink1.Margins.Top = pageMarginTop;
            printableComponentLink1.Margins.Bottom = pageMarginBottom;

            printableComponentLink1.CreateDocument();
            if (LoginForm.doLapseReport)
                printableComponentLink1.Print();
            else
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
            string title = "Arrangement Form Fields Report";
            Printer.DrawQuad(6, 8, 4, 4, title, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);


            //            Printer.DrawQuadTicks();
            string workDate = "";
            Printer.SetQuadSize(24, 12);
//            Printer.DrawQuad(20, 8, 5, 4, title + workDate, Color.Black, BorderSide.None, font, HorizontalAlignment.Left, VertAlignment.Bottom);

            Printer.SetQuadSize(12, 12);
            Printer.DrawQuadBorder(1, 1, 12, 12, BorderSide.All, 1, Color.Black);
            Printer.DrawQuadBorder(12, 1, 1, 12, BorderSide.Right, 1, Color.Black);
        }
        /****************************************************************************************/
        private int footerCount = 0;
        private void gridMain_BeforePrintRow(object sender, DevExpress.XtraGrid.Views.Printing.CancelPrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
            }
        }
        /****************************************************************************************/
        private void gridMain_AfterPrintRow(object sender, DevExpress.XtraGrid.Views.Printing.PrintRowEventArgs e)
        {
            if (e.HasFooter)
            {
                footerCount++;
                if (footerCount >= 2)
                {
                    footerCount = 0;
                    //                    e.PS.InsertPageBreak(e.Y);
                }
            }
        }
        /***********************************************************************************************/
        private void btnFindCommon_Click(object sender, EventArgs e)
        {
            DataTable dt = (DataTable)dgv4.DataSource;
            string field = "";
            string table = "";
            string dbField = "";
            string qualify = "";
            string help = "";
            DataTable dx = null;
            string cmd = "";
            bool found = false;
            this.Cursor = Cursors.WaitCursor;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                field = dt.Rows[i]["field"].ObjToString();
                if ( field.Trim().ToUpper() == "DEC")
                {

                }
                table = dt.Rows[i]["table"].ObjToString();
                dbField = dt.Rows[i]["dbfield"].ObjToString();
                //if (!String.IsNullOrWhiteSpace(table) || !String.IsNullOrWhiteSpace(dbField))
                //    continue;
                cmd = "Select * from `structures` where `field` = '" + field + "';";
                dx = G1.get_db_data(cmd);
                if (dx.Rows.Count <= 0)
                    continue;
                table = dx.Rows[0]["table"].ObjToString();
                dbField = dx.Rows[0]["dbfield"].ObjToString();
                qualify = dx.Rows[0]["qualify"].ObjToString();
                help = dx.Rows[0]["help"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( table))
                {
                    dt.Rows[i]["table"] = table;
                    dt.Rows[i]["dbfield"] = dbField;
                    dt.Rows[i]["qualify"] = qualify;
                    dt.Rows[i]["help"] = help;
                    if (!String.IsNullOrWhiteSpace(workContract))
                    {
                        if ( !String.IsNullOrWhiteSpace ( dbField ))
                            dt.Rows[i]["data"] = GetDataField(table, dbField, field, qualify);
                    }
                    found = true;
                }
            }
            if (found)
            {
                btnSave.Show();
//                btnSaveDBFields.Show();
            }
            this.Cursor = Cursors.Default;
        }
        /***********************************************************************************************/
        private string BuildCustExtendedList()
        {
            string list = "";
            string dbField = "";
            string cmd = "Select * from `cust_extended_layout` ORDER by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                dbField = dt.Rows[i]["dbField"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( dbField))
                {
                    if (!String.IsNullOrWhiteSpace(list))
                        list += "\n";
                    list += dbField;
                }
            }
            return list;
        }
        /***********************************************************************************************/
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (gridMain4.OptionsFind.AlwaysVisible == true)
                gridMain4.OptionsFind.AlwaysVisible = false;
            else
                gridMain4.OptionsFind.AlwaysVisible = true;
        }
        /***********************************************************************************************/
    }
}