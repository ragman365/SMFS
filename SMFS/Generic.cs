using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Data;
using System.Configuration;
using MySql.Data.MySqlClient;
using MySql.Data.Types;
using System.Globalization;
using System.IO;
//using RAGSpread;
using System.Text;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Collections.Specialized;
using System.Security.Cryptography;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.Utils;
using System.Linq;
using System.Xml;
using Ionic.Zlib;
using DevExpress.XtraRichEdit;
using DevExpress.XtraGrid.Views.Grid;
using System.Windows.Forms.DataVisualization.Charting;
using DevExpress.Pdf;
using DevExpress.Pdf.Interop;
using DevExpress.XtraPrinting.Export.Pdf;
using DevExpress.XtraGrid.Controls;
using SMFS;
using DevExpress.XtraGrid.Columns;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Collections.Generic;

namespace GeneralLib
{
    /****************************************************************************/
    public class G1
    {
        /****************************************************************************************/
        public static Image IMAGE_NewMail { get { return ImageFromResource("Resources.mailnew32.ico"); } }
        public static Image IMAGE_Mail { get { return ImageFromResource("Resources.mail32.ico"); } }
        private static PerformanceCounter ramCounter;

        public static DateTime staticStart1900Date = new DateTime(1900, 1, 1);
        public static DateTime staticStart1970Date = new DateTime(1970, 1, 1);
        public static bool lost_network = false;
        public static bool RobbyServer = false;
        public static bool oldCopy = false;
        public static int of_ans_count = 0;
        public static string[] of_answer = new string[500];
        /***********************************************************************************************/
        public static bool validate_numeric(string str)
        {
            int i;
            string data = str.Trim();
            int len = data.Length;
            if (len == 0)
                return false;
            for (i = 0; i < len; i++)
            {
                char c = data[i];
                if (c == 0)
                    break;
                if (c < '0' || c > '9')
                {
                    if (c == '.' || c == ',' || c == '-')
                        continue;
                    return false;
                }
            }
            return true;
        }
        /***********************************************************************************************/
        public static int StripNumeric(string str)
        {
            int i;
            int number = -1;
            string data = str.Trim();
            int len = data.Length;
            if (len == 0)
                return 0;
            string text = "";
            for (i = 0; i < len; i++)
            {
                char c = data[i];
                if (c == 0)
                    break;
                if (c < '0' || c > '9')
                    continue;
                else
                    text += data.Substring(i, 1);
            }
            if (!String.IsNullOrWhiteSpace(text))
            {
                if (G1.validate_numeric(text))
                    number = text.ObjToInt32();
            }
            return number;
        }
        /***********************************************************************************************/
        public static void NumberDataTable(DataTable dt)
        {
            if (dt == null)
                return;
            try
            {
                if (G1.get_column_number(dt, "num") < 0)
                    dt.Columns.Add("num", typeof(string)).SetOrdinal(0);
                for (int i = 0; i < dt.Rows.Count; i++)
                    dt.Rows[i]["num"] = (i + 1).ToString();
            }
            catch
            {
            }
        }
        /***********************************************************************************************/
        public static int get_column_number(DevExpress.XtraGrid.Views.Grid.GridView dt, string name)
        {
            if (dt == null)
                return -1;
            name = name.ToUpper();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string str = dt.Columns[i].FieldName.Trim().ToUpper();
                if (str == name)
                    return i;
            }
            return -1;
        }
        /***********************************************************************************************/
        public static int get_column_number(DataTable dt, string name)
        {
            if (dt == null)
                return -1;
            name = name.ToUpper();
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string str = dt.Columns[i].ColumnName.Trim().ToUpper();
                if (str == name)
                    return i;
            }
            return -1;
        }
        /***********************************************************************************************/
        public static bool is_valid_column(DataRow dRow, string name)
        {
            if (dRow == null)
                return false;
            name = name.ToUpper();
            if (dRow.Table.Columns.Contains(name))
                return true;
            return false;
        }
        /****************************************************************************/
        public static Font Toggle_Font(DevExpress.XtraRichEdit.RichEditControl rtb, string name, float mysize)
        {
            Font myFont = new Font(name, (int)mysize);
            rtb.Font = myFont;
            rtb.Document.DefaultCharacterProperties.FontName = name;
            rtb.Document.DefaultCharacterProperties.FontSize = mysize;
            return myFont;
        }
        /****************************************************************************/
        /// <param name="bPersist"></param>
        public static Font Toggle_Bold(DevExpress.XtraRichEdit.RichEditControl rtb, bool bold, bool italics)
        {
            return Toggle_Bold(rtb, bold, italics, false);
        }
        /****************************************************************************/
        /// <param name="bPersist"></param>
        public static Font Toggle_Bold(DevExpress.XtraRichEdit.RichEditControl my_rtb, bool bold, bool italics, bool underline)
        {
            return Toggle_Bold(my_rtb, bold, italics, underline, 1F);
        }
        /****************************************************************************/
        /// <param name="bPersist"></param>
        public static Font Toggle_Bold(DevExpress.XtraRichEdit.RichEditControl my_rtb, bool bold, bool italics, float size)
        {
            return Toggle_Bold(my_rtb, bold, italics, false, size);
        }
        /***************************************************************************************/
        public static Font Toggle_Bold(DevExpress.XtraRichEdit.RichEditControl my_rtb, bool bold, bool italics, bool underline, float size)
        {
            System.Drawing.Font currentFont;
            System.Drawing.FontStyle newFontStyle;
            if (my_rtb.Font == null)
                currentFont = my_rtb.Font;
            else
                currentFont = my_rtb.Font;

            newFontStyle = FontStyle.Regular;

            if (bold == true)
                newFontStyle |= FontStyle.Bold;
            if (italics == true)
                newFontStyle |= FontStyle.Italic;
            if (underline == true)
                newFontStyle |= FontStyle.Underline;

            float newsize = size * currentFont.Size;

            Font newFont = new Font(currentFont.FontFamily, newsize, newFontStyle);
            my_rtb.Font = newFont;
            my_rtb.Document.DefaultCharacterProperties.FontName = currentFont.FontFamily.Name;
            my_rtb.Document.DefaultCharacterProperties.FontSize = newsize;
            return newFont;
        }
        /****************************************************************************/
        public static Font Toggle_Font(RichTextBox rtb, string name, float mysize)
        {
            Font myFont = new Font(name, (int)mysize);
            rtb.SelectionFont = myFont;
            return myFont;
        }
        /****************************************************************************/
        /// <param name="bPersist"></param>
        public static Font Toggle_Bold(RichTextBox rtb, bool bold, bool italics)
        {
            return Toggle_Bold(rtb, bold, italics, false);
        }
        /****************************************************************************/
        /// <param name="bPersist"></param>
        public static Font Toggle_Bold(RichTextBox my_rtb, bool bold, bool italics, bool underline)
        {
            return Toggle_Bold(my_rtb, bold, italics, underline, 1F);
        }
        /****************************************************************************/
        /// <param name="bPersist"></param>
        public static Font Toggle_Bold(RichTextBox my_rtb, bool bold, bool italics, float size)
        {
            return Toggle_Bold(my_rtb, bold, italics, false, size);
        }
        /***************************************************************************************/
        public static Font Toggle_Bold(RichTextBox my_rtb, bool bold, bool italics, bool underline, float size)
        {
            System.Drawing.Font currentFont;
            System.Drawing.FontStyle newFontStyle;

            if (my_rtb.SelectionFont == null)
                currentFont = my_rtb.Font;
            else
                currentFont = my_rtb.SelectionFont;

            newFontStyle = FontStyle.Regular;

            if (bold == true)
                newFontStyle |= FontStyle.Bold;
            if (italics == true)
                newFontStyle |= FontStyle.Italic;
            if (underline == true)
                newFontStyle |= FontStyle.Underline;

            float newsize = size * currentFont.Size;

            Font newFont = new Font(currentFont.FontFamily, newsize, newFontStyle);
            my_rtb.SelectionFont = newFont;
            return newFont;
        }
        /****************************************************************************************/
        public static string lay_in_string(string pline, string str, int position, int length)
        {
            string line = pline;
            if (str.Length > length)
                str = str.Substring(0, length);
            var b = line.ToCharArray();
            var c = str.ToCharArray();
            try
            {
                for (int i = 0; i < str.Length; i++)
                {
                    if ((position + i) < pline.Length)
                        b[position + i] = c[i];
                }
            }
            catch (Exception ex)
            {

            }
            line = new string(b);
            //line = line.Insert(position, str);
            return line;
        }
        /********************************************************************************************/
        public static string spreadpath
        {
            get { return ConfigurationValue("spreadpath").ToString(); }
        }
        /********************************************************************************************/
        public static object ConfigurationValue(string item)
        {
            AppConfig appcfg = new AppConfig();
            appcfg.ConfigType = (int)ConfigFileType.AppConfig;
            appcfg.LoadDoc();
            object retval = appcfg.GetValue(item, typeof(string));
            return retval;
        }
        /********************************************************************************************/
        public static MySqlConnection conn10
        {
            get
            {
                MySqlConnection mySQL = new MySqlConnection("host=localhost;database=smfs;uid=root;password=P@ssword.1090;allow zero datetime = yes");
                return mySQL;
            }
        } 
        /********************************************************************************************/
        public static bool firstSQL = true;
        public static MySqlConnection conn1
        {
            get
            {
                //if ( firstSQL)
                //    MessageBox.Show("HERE BEFORE SERVER!");
                string server = ConfigurationValue("server").ToString();
                //                if (G1.RobbyServer)
                server = ConfigurationValue("RobbyServer").ToString();
                string database = ConfigurationValue("database").ToString();
                if ( oldCopy )
                    database = ConfigurationValue("database2").ToString();

                //if (firstSQL)
                //    MessageBox.Show("HERE Server = " + server + "!");

                //MySqlConnection mySQL = new MySqlConnection("host=" + server +
                //    ";database=" + ConfigurationValue("database").ToString() +
                ////                    ";uid=smfsus;password=P@ssword.1090;allow zero datetime = yes");
                ////                return new MySqlConnection("host=" + ConfigurationValue("server").ToString() +
                //";uid=root;password=P@ssword.1090;allow zero datetime = yes");


                MySqlConnection mySQL = null;

                if (!oldCopy)
                {
                    mySQL = new MySqlConnection("host=" + server +
                        ";database=" + ConfigurationValue("database").ToString() +
                    //                    ";uid=smfsus;password=P@ssword.1090;allow zero datetime = yes");
                    //                return new MySqlConnection("host=" + ConfigurationValue("server").ToString() +
                    ";uid=root;password=P@ssword.1090;allow zero datetime = yes");
                }
                else
                {
                    mySQL = new MySqlConnection("host=" + server +
                        ";database=" + ConfigurationValue("database2").ToString() +
                    //                    ";uid=smfsus;password=P@ssword.1090;allow zero datetime = yes");
                    //                return new MySqlConnection("host=" + ConfigurationValue("server").ToString() +
                    ";uid=root;password=P@ssword.1090;allow zero datetime = yes");
                }
                //if (firstSQL)
                //    MessageBox.Show("HERE AFTER OPEN!");
                firstSQL = false;
                return mySQL;
            }
        }
        /***********************************************************************************************/
        public static string GetWhatDatabase()
        {
            string database = "SMFS";
            try
            {
                database = G1.conn1.Database.ObjToString();
            }
            catch (Exception ex)
            {
            }
            return database;
        }
        /***********************************************************************************************/
        public static void OpenConnection10()
        {
            if (conn10.State != ConnectionState.Open)
                conn10.Open();
        }
        /***********************************************************************************************/
        public static void OpenConnection()
        {
            if (conn1.State != ConnectionState.Open)
                conn1.Open();
        }
        /***********************************************************************************************/
        public static void OpenConnection(MySqlConnection conn)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
        }
        /***********************************************************************************************/
        public static void CloseConnection()
        {
            if (conn1.State != ConnectionState.Closed)
                conn1.Close();
        }
        /***********************************************************************************************/
        public static void CloseConnection(MySqlConnection conn)
        {
            if (conn.State != ConnectionState.Closed)
                conn.Close();
        }
        /***********************************************************************************************/
        public static void CreateTable(string table, string dataColumn)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand("CREATE TABLE `" + table + "` (record BIGINT NOT NULL AUTO_INCREMENT,`" + dataColumn + "` VARCHAR(100) NOT NULL, PRIMARY KEY (record)) COLLATE='utf8_general_ci' ENGINE=InnoDB;", G1.conn1);
                OpenConnection(cmd.Connection);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Creating Table " + table + " " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        public static void LockTable(string table, MySqlConnection connin)
        {
            MySqlCommand cmd = new MySqlCommand("LOCK TABLE `" + table + "` WRITE ", connin);
            OpenConnection(cmd.Connection);
            cmd.ExecuteNonQuery();
        }
        /***********************************************************************************************/
        public static void UnLockTable(MySqlConnection connin)
        {
            MySqlCommand cmd = new MySqlCommand("UNLOCK TABLES", connin);
            OpenConnection(cmd.Connection);
            cmd.ExecuteNonQuery();
        }
        /****************************************************************************************/
        public static void verify_path(string path)
        {
            string mainpath = "";
            path = path.Replace("\\", "/");
            //            path = G1.replace_text(path, "\\", "/");
            path += "/";
            path = path.Replace("/", "\\");
            //            path = G1.replace_text(path, "/", "\\");
            int cptr = path.IndexOf("\\");
            bool server = false;
            bool first = true;
            int count = 0;
            for (; ; )
            {
                if (cptr < 0)
                    break;
                if (cptr == 0)
                {
                    mainpath += "\\";
                    if ((cptr + 1) > path.Length)
                        break;
                    path = path.Substring((cptr + 1));
                    cptr = path.IndexOf("\\");
                    continue;
                }
                mainpath += path.Substring(0, cptr);
                if ((cptr + 1) <= path.Length)
                    path = path.Substring((cptr + 1));
                if (server == false && first)
                {
                    if (mainpath.IndexOf("\\\\") >= 0)
                    {
                        server = true;
                        cptr = path.IndexOf("\\");
                        count = 1;
                        mainpath += "\\";
                        continue;
                    }
                }
                else if (server == true)
                {
                    if (count == 1)
                    {
                        cptr = path.IndexOf("\\");
                        mainpath += "\\";
                        count = count + 1;
                        continue;
                    }
                }
                if (!Directory.Exists(mainpath))
                    Directory.CreateDirectory(mainpath);
                first = false;
                mainpath += "\\";
                count = count + 1;
                cptr = path.IndexOf("\\");
            }
        }
        /***********************************************************************************************/
        public static string DecodePath(string path )
        {
            string newPath = "";
            string mainpath = "";
            string lastpath = "";
            path = path.Replace("\\", "/");
            string str = "";
            for ( int i=path.Length-1; i>=0; i-- )
            {
                str = path.Substring(i, 1);
                if ( str == "/")
                {
                    newPath = path.Substring(0, i);
                    lastpath = path.Substring(i);
                    break;
                }    
            }
            return newPath;
        }
        /***********************************************************************************************/
        public static string LastPath(string path)
        {
            string newPath = "";
            string mainpath = DecodePath(path);
            string lastpath = "";
            path = path.Replace("\\", "/");
            string str = "";
            for (int i = mainpath.Length - 1; i >= 0; i--)
            {
                str = mainpath.Substring(i, 1);
                if (str == "/")
                {
                    newPath = mainpath.Substring(0, i);
                    lastpath = mainpath.Substring(i);
                    lastpath = lastpath.Replace("/", "");
                    break;
                }
            }
            return lastpath;
        }
        /***********************************************************************************************/
        public static string GetReportPath()
        {
            string path = @"C:\SMFS_Reports\";
            G1.verify_path(path);
            return path;
        }
        /***********************************************************************************************/
        public static string ToMonthName(DateTime dateTime)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(dateTime.Month);
        }
        /***********************************************************************************************/
        public static string GetNextRecord(string db, string key)
        {
            MySqlDataAdapter da = new MySqlDataAdapter();
            DataTable dx = new DataTable();
            string command = "select MAX(`" + key + "`) from " + db + ";";
            MySqlCommand mycmd = new MySqlCommand(command, conn1);
            da.SelectCommand = mycmd;
            string odata;
            try
            {
                da.Fill(dx);
                odata = dx.Rows[0][0].ToString();
            }
            catch
            {
                odata = "0";
            }
            int max_record = 0;
            if (G1.validate_numeric(odata))
                max_record = G1.myint(odata);
            max_record = max_record + 1;
            return max_record.ToString();
        }
        /***********************************************************************************************/
        public static string create_record1(string tablename, string fieldname, string initvalue)
        {
            string rec = "";
            MySqlConnection mconn = null;

            try
            {
                mconn = G1.conn1;

                string str = "INSERT INTO `" + tablename + "` (record); SELECT `record`, now() FROM `" + tablename + "` WHERE `record` NOT IN ( SELECT `record` FROM `" + tablename + "` AS rec LIMIT 1);";

                //MySqlCommand cmd = new MySqlCommand("INSERT INTO `" + tablename + "` (`" + fieldname + "`) VALUES('" + initvalue + "');SELECT @@IDENTITY from `" + tablename + "` limit 1", mconn);



                //                LockTable(tablename, mconn);
                //                rec = G1.GetNextRecord(tablename, "Record");
                MySqlCommand cmd = new MySqlCommand(str, mconn);
                OpenConnection(mconn);
                cmd.ExecuteScalar();
                rec = cmd.ExecuteScalar().ToString();
                UnLockTable(mconn);
            }
            catch (Exception ex)
            {
                if (G1.conn1 != null && G1.conn1.State != ConnectionState.Closed)
                    UnLockTable(G1.conn1);

                //				G1.LogError("A critical exception has occurred while attempting to aquire a record for table " + tablename + ":\n" + ex.Message, ex, true);

            }
            finally
            {
                if (G1.conn1 != null && G1.conn1.State != ConnectionState.Closed)
                    G1.conn1.Close();
            }

            return rec;

        }
        /***********************************************************************************************/
        public static string create_record(string tablename, string fieldname, string initvalue)
        {
            string rec = "";
            MySqlConnection mconn = null;

            try
            { // Hopefully this can happen without a lock
                mconn = G1.conn1;
                LockTable(tablename, mconn);
                //                rec = G1.GetNextRecord(tablename, "Record");
                string str = "INSERT INTO `" + tablename + "` (`" + fieldname + "`) VALUES(" + initvalue + ");SELECT @@IDENTITY from `" + tablename + "` limit 1";
                MySqlCommand cmd = new MySqlCommand("INSERT INTO `" + tablename + "` (`" + fieldname + "`) VALUES('" + initvalue + "');SELECT @@IDENTITY from `" + tablename + "` limit 1", mconn);
                OpenConnection(mconn);
                rec = cmd.ExecuteScalar().ObjToString();
                //				rec = cmd.ExecuteScalar().ToString();				
                UnLockTable(mconn);
            }
            catch (Exception ex)
            {
                if (mconn != null && mconn.State != ConnectionState.Closed)
                    UnLockTable(mconn);

                MessageBox.Show("A critical exception has occurred while attempting to aquire a record for table " + tablename + ":\n" + ex.Message, "Create Record Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            finally
            {
                if (mconn != null && mconn.State != ConnectionState.Closed)
                    mconn.Close();
            }

            return rec;
        }
        /***********************************************************************************************/
        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        /***********************************************************************************************/
        public static bool checkUserPreference(string user, string module, string preference)
        {
            string answer = G1.getPreference(user, module, preference);
            if (answer.Trim().ToUpper() == "YES")
                return true;
            MessageBox.Show("***Warning***\nYou do not have permission to perform this operation!\nSorry.", "Check Preferences Dialog", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }
        /***********************************************************************************************/
        public static DataTable getPreferenceUsers (string module, string preference )
        {
            string cmd = "Select * from `preferenceusers` where `module` = '" + module + "' and `preference` = '" + preference + "' AND `preferenceAnswer` = 'YES';";
            DataTable dt = G1.get_db_data(cmd);
            return dt;
        }
        /***********************************************************************************************/
        public static string getPreference(string user, string module, string preference, bool showError = false)
        {
            //if (1 == 1)
            //    return "YES";
            //if (LoginForm.isRobby)
            //    return "YES";
            string cmd = "Select * from `preferenceList` where `module` = '" + module + "' and `preference` = '" + preference + "'; ";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "YES";

            cmd = "Select * from `preferenceusers` where `userName` = '" + user + "' and `module` = '" + module + "' and `preference` = '" + preference + "';";
            dt = G1.get_db_data(cmd);
            string answer = "";
            if (dt.Rows.Count > 0)
                answer = dt.Rows[0]["preferenceAnswer"].ObjToString();
            if (answer.ToUpper() == "Y")
                answer = "YES";
            if (answer.ToUpper() != "YES")
                answer = CheckUserSpecifics(user, module, preference);
            if (showError)
            {
                if (answer.ToUpper() != "YES")
                {
                    MessageBox.Show("You do not have permission to perform this function!", preference + " Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
            return answer.ToUpper();
        }
        /***********************************************************************************************/
        public static string CheckUserSpecifics(string user, string module, string preference)
        {
            string classification = "";
            string Admin = "";
            string status = "";
            string answer = "";

            string cmd = "Select * from `users` where `username` = '" + user + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return answer;
            classification = dt.Rows[0]["classification"].ObjToString().ToUpper();
            if (String.IsNullOrWhiteSpace(classification))
                classification = "ADMIN";
            Admin = dt.Rows[0]["admin"].ObjToString();
            status = dt.Rows[0]["status"].ObjToString();
            if (status.ToUpper() != "ACTIVE")
                return answer;
            //if (status.ToUpper() == "ACTIVE")
            //{
            //    if (Admin == "1")
            //        return "YES";
            //}

            cmd = "Select * from `preferenceList` where `module` = '" + module + "' and `preference` = '" + preference + "'; ";
            dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return answer;
            string HR = dt.Rows[0]["HR"].ObjToString().ToUpper();
            if (HR.ToUpper() == "Y" || HR.ToUpper() == "YES")
                HR = "YES";
            string admin = dt.Rows[0]["Admin"].ObjToString().ToUpper();
            if (admin.ToUpper() == "Y" || admin.ToUpper() == "YES")
                admin = "YES";

            if (HR == "YES")
                admin = "YES";

            string superuser = dt.Rows[0]["SuperUser"].ObjToString().ToUpper();
            if (superuser.ToUpper() == "Y" || superuser.ToUpper() == "YES")
                superuser = "YES";
            string homeoffice = dt.Rows[0]["HomeOffice"].ObjToString().ToUpper();
            if (homeoffice.ToUpper() == "Y" || homeoffice.ToUpper() == "YES")
                homeoffice = "YES";
            string field = dt.Rows[0]["field"].ObjToString().ToUpper();
            if (field.ToUpper() == "Y" || field.ToUpper() == "YES")
                field = "YES";
            string defaultAnswer = dt.Rows[0]["default"].ObjToString().ToUpper();
            if (defaultAnswer.ToUpper() == "Y" || defaultAnswer.ToUpper() == "YES")
                defaultAnswer = "YES";

            //if (!String.IsNullOrWhiteSpace(admin))
            //    admin = admin.Substring(0, 1).ToUpper();
            //if (!String.IsNullOrWhiteSpace(superuser))
            //    superuser = superuser.Substring(0, 1).ToUpper();
            //if (!String.IsNullOrWhiteSpace(homeoffice))
            //    homeoffice = homeoffice.Substring(0, 1).ToUpper();
            //if (!String.IsNullOrWhiteSpace(field))
            //    field = field.Substring(0, 1).ToUpper();

            if (classification == "HR")
            {
                if (HR == "YES" || admin == "YES" || superuser == "YES" || homeoffice == "YES" || field == "YES")
                    answer = "YES";
            }
            else if (classification == "ADMIN")
            {
                if (admin == "YES" || superuser == "YES" || homeoffice == "YES" || field == "YES")
                    answer = "YES";
            }
            else if (classification == "SUPERUSER")
            {
                if (superuser == "YES" || homeoffice == "YES" || field == "YES")
                    answer = "YES";
            }
            else if (classification == "HOMEOFFICE")
            {
                if (homeoffice == "YES" || field == "YES")
                    answer = "YES";
            }
            else if (classification == "FIELD")
            {
                if (field == "YES")
                    answer = "YES";
            }
            if (answer != "YES")
            {
                //    if (Admin == "1")
                //        return "YES";
            }
            return answer;
        }
        /****************************************************************************************/
        public static string SaveToLapseDatabase(string rtfText, string type)
        {
            byte[] b = Encoding.UTF8.GetBytes(rtfText);
            string record = G1.create_record("lapse_notices", "type", "-1");
            if (G1.BadRecord("lapse_notices", record))
                return "";

            G1.update_db_table("lapse_notices", "record", record, new string[] { "type", type });

            G1.update_blob("lapse_notices", "record", record, "image", b);
            return record;
        }
        /***********************************************************************************************/
        public static void delete_db_table(string db, string field, string record)
        {
            if (record == "")
                return;
            if (record == "0")
                return;
            string command = "Delete from `" + db + "` ";
            command += " where `" + field + "` = '" + record + "' ";
            update_db_data(command);
        }
        /***************************************************************************************/
        public static DataTable get_db_data(string command)
        {
            int counter = 0;
            bool trying = true;
            while (trying)
            {
                try
                {
                    DataTable dx = new DataTable();
                    MySqlDataAdapter da = new MySqlDataAdapter();
                    da.SelectCommand = new MySqlCommand(command, G1.conn1);
                    da.Fill(dx);
                    lost_network = false;
                    if (counter > 0) // Lost Network, Gotit back, Wait just 2 seconds
                        G1.sleep(2000);
                    return dx;
                }
                catch (MySqlException ex)
                {
                    if (command.ToUpper().IndexOf(" ASC") > 0)
                    {
                        command = command.Replace(" ASC", " ");
                        continue;
                    }
                    lost_network = true;
                    counter = counter + 1;
                    if (counter > 5)
                    {
                        string str = "***ERROR*** In SQL Database Connection!\n";
                        str += ex.Message + "\n";
                        str += "Keep Trying?";
                        DialogResult result = MessageBox.Show(str, "MySQL Connection Error Dialog",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == DialogResult.No)
                        {
                            throw;
                            return null;
                        }
                        counter = 0;
                    }
                    G1.sleep(1000);
                }
            }
            lost_network = false;
            return null;
        }
        /****************************************************************************************/
        public static byte[] get_pdf_blob(string record, string field)
        {
            string command = "select `" + field + "` from `pdfimages` where `Record` = '" + record + "';";

            DataTable dt = G1.get_db_data(command);
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];
                if (dr[field] == null)
                    return null;
                string str = dr[field].ToString();
                int len = str.Length;
                try
                {
                    var bytes = Convert.FromBase64String(str.Replace("-", "+").Replace("_", "/"));
                    return bytes;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                return null;
            }
            return null;
        }
        /***********************************************************************************************/
        public static string DecodeFilename(string fullname, bool trim = false)
        {
            if (String.IsNullOrWhiteSpace(fullname))
                return "";
            string filename = new FileInfo(fullname).Name;
            if (trim)
            {
                int idx = filename.IndexOf(".");
                if (idx > 0)
                    filename = filename.Substring(0, idx);
            }
            return filename;
        }
        /***********************************************************************************************/
        public static void ReadAndStorePDF(string table, string record, string filename)
        {
            if (filename.Trim().ToUpper().IndexOf(".PDF") > 0)
            {
                FileStream fStream = File.OpenRead(filename);
                byte[] contents = new byte[fStream.Length];
                fStream.Read(contents, 0, (int)fStream.Length);
                fStream.Close();
                G1.update_blob("pdfimages", "record", record, "image", contents);
            }
            else
            {
                DevExpress.XtraRichEdit.RichEditControl rtb = new RichEditControl();
                rtb.Document.LoadDocument(filename, DevExpress.XtraRichEdit.DocumentFormat.Undefined);
                string rtfText = rtb.Document.RtfText;
                byte[] b = Encoding.UTF8.GetBytes(rtfText);
                G1.update_blob("pdfImages", "record", record, "image", b);
            }
        }
        /****************************************************************************************/
        public static int update_db_blob(string table, string record, string field, string data)
        {
            MySqlConnection mconn = null;
            int rv = -1;

            string str = G1.CompressString(data);

            mconn = G1.conn1;
            MySqlCommand sqlCommand = new MySqlCommand("Update `" + table + "`  Set `" + field + "` = '" + str + "' where `record`= '" + record + "';", mconn);
            try
            {
                OpenConnection(mconn);
                rv = sqlCommand.ExecuteNonQuery().ObjToInt32();
            }
            catch (Exception ex)
            {
                if (mconn != null && mconn.State != ConnectionState.Closed)
                    UnLockTable(mconn);

                //				G1.LogError("A critical exception has occurred while attempting to aquire a record for table " + tablename + ":\n" + ex.Message, ex, true);

            }
            finally
            {
                if (mconn != null && mconn.State != ConnectionState.Closed)
                    mconn.Close();
            }

            return rv;
        }
        /***********************************************************************************************/
        public static void GrantFileAccess(string fullPath)
        {
            try
            {
                if (!File.Exists(fullPath))
                    return;
                DirectoryInfo dInfo = new DirectoryInfo(fullPath);
                DirectorySecurity dSecurity = null;
                try
                {
                    dSecurity = dInfo.GetAccessControl();
                }
                catch (Exception ex)
                {
                }
                dSecurity.AddAccessRule(new FileSystemAccessRule(
                    new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                    FileSystemRights.FullControl,
                    InheritanceFlags.ObjectInherit |
                       InheritanceFlags.ContainerInherit,
                    PropagationFlags.NoPropagateInherit,
                    AccessControlType.Allow));

                dInfo.SetAccessControl(dSecurity);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Message.ToString());
            }
        }
        /***********************************************************************************************/
        public static void GrantDirectoryAccess(string directory)
        {
            bool exists = System.IO.Directory.Exists(directory);
            if (!exists)
            {
                try
                {
                    DirectoryInfo di = System.IO.Directory.CreateDirectory(directory);
                }
                catch ( Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updaing Creating Directory For " + directory + " " + ex.Message.ToString(), "Create Directory Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return;
                }

                //Console.WriteLine("The Folder is created Sucessfully");
            }
            else
            {
                //Console.WriteLine("The Folder already exists");
            }
            try
            {
                DirectoryInfo dInfo = new DirectoryInfo(directory);
                DirectorySecurity dSecurity = dInfo.GetAccessControl();
                dSecurity.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), FileSystemRights.FullControl, InheritanceFlags.ObjectInherit | InheritanceFlags.ContainerInherit, PropagationFlags.NoPropagateInherit, AccessControlType.Allow));
                dInfo.SetAccessControl(dSecurity);
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Updaing Security Permissions For " + directory + " " + ex.Message.ToString(), "Update Security Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
        }
        /***********************************************************************************************/
        public static bool isAdmin()
        {
            if (LoginForm.classification.ToUpper() == "ADMIN")
                return true;
            if (LoginForm.classification.ToUpper() == "HR")
                return true;
            return false;
        }
        /***********************************************************************************************/
        public static bool isAdminOrSuper()
        {
            if (LoginForm.classification.ToUpper() == "HR")
                return true;
            if (LoginForm.classification.ToUpper() == "ADMIN")
                return true;
            if (LoginForm.classification.ToUpper() == "SUPERUSER")
                return true;
            return false;
        }
        /***********************************************************************************************/
        public static bool isSuperUser()
        {
            if (LoginForm.classification.ToUpper() == "SUPERUSER")
                return true;
            return false;
        }
        /***********************************************************************************************/
        public static bool isHomeOffice()
        {
            if (LoginForm.classification.ToUpper() == "HOMEOFFICE")
                return true;
            return false;
        }
        /***********************************************************************************************/
        public static bool isField ()
        {
            if (LoginForm.classification.ToUpper() == "FIELD")
                return true;
            return false;
        }
        /***********************************************************************************************/
        public static bool isHR()
        {
            if (LoginForm.classification.ToUpper() == "HR")
                return true;
            if (G1.RobbyServer && LoginForm.username.ToUpper() == "ROBBY" )
                return true;
            return false;
        }
        /***********************************************************************************************/
        public static bool isSpecial()
        {
            if (LoginForm.classification.ToUpper() == "HR")
                return true;
            if (G1.RobbyServer && LoginForm.username.ToUpper() == "ROBBY")
                return true;
            if (LoginForm.username.Trim().ToUpper() == "CJENKINS")
                return true;
            if (LoginForm.username.Trim().ToUpper() == "TIM")
                return true;
            return false;
        }
        /***********************************************************************************************/
        public static string CompressString(string text)
        {
            if (String.IsNullOrEmpty(text))
                text = "";

            var buffer = Encoding.UTF8.GetBytes(text);
            string output;

            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
                    zip.Write(buffer, 0, buffer.Length);
                output = Convert.ToBase64String(ms.ToArray());
            }

            return output;
        }
        /***********************************************************************************************/
        public static string ReformatMoney(double money)
        {
            if (money == 0D)
                return "0.00";
            long lvalue = (long)((money + .005D) * 100D);
            if (money < 0D)
                lvalue = (long)((money - .005D) * 100D);

            string strMoney = G1.commatize(lvalue);
            return strMoney;
        }
        /***********************************************************************************************/
        public static int myint(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                int retval;
                try
                {
                    if (validate_numeric(str))
                    {
                        if (Int32.TryParse(str.TrimStart(new char[] { '.' }), out retval))
                            return retval;
                        else
                            return 0;
                    }
                    else
                        return 0;
                }
                catch (Exception ex)
                {
                    //                    LogError(ex.Message, ex, false);
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        /****************************************************************************************/
        public static string TrimDecimals(string str)
        {
            str = str.Replace(",", "");
            int idx = str.IndexOf('.');
            if (idx >= 0)
                str = str.Substring(0, idx);
            return str;
        }
        /***************************************************************************************/
        public static void update_db_data(string command)
        {
            int counter = 0;
            bool trying = true;
            while (trying)
            {
                MySqlCommand cmd = new MySqlCommand(command, G1.conn1);
                int rv = G1.TryNonQueryStatic(cmd);
                if (rv == -1)
                {
                    counter = counter + 1;
                    if (counter > 5)
                    {
                        string str = "***ERROR*** In SQL Database Connection!\n";
                        str += G1.lastMySqlErrorMessage + "\n";
                        str += "Keep Trying for awhile?";
                        DialogResult result = MessageBox.Show(str, "MySQL Connection Error Dialog",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation);
                        if (result == DialogResult.No)
                            return;
                        counter = 0;
                    }
                    G1.sleep(1000);
                }
                else
                    trying = false;
            }
        }
        /***********************************************************************************************/
        public static int TryNonQueryStatic(MySqlCommand cmd)
        {
            int rowsaffected = -1;

            try
            {
                cmd.Connection.Open();
                rowsaffected = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                //G1.LogError(ex.Message, ex, false);
                lastMySqlErrorMessage = ex.Message;
                rowsaffected = -1;
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
            }

            return rowsaffected;
        }
        /***************************************************************************************/
        public static int count_words(string str)
        {
            int count = 0;
            int idx = str.IndexOf(",");
            if (idx >= 0)
                str = str.Substring(0, idx);
            idx = str.IndexOf(" ");
            if (idx < 0)
                return 1;
            str = str.Trim() + " ";
            for (; ; )
            {
                if (idx <= 0)
                    break;
                string word = str.Substring(0, idx);
                count++;
                if ((idx + 1) >= str.Length)
                    break;
                str = str.Substring((idx + 1));
                idx = str.IndexOf(" ");
            }
            return count;
        }
        /***************************************************************************************/
        public static string fix_fname(string str)
        {
            bool md, dr, sr, jr, iii, ii, dds, bro, rev;
            bool comma = false;
            int i, j, k, nealint;
            string[] words = new string[100];
            string oldstr = str;
            string newstr = "";
            str = str.Replace("(", "");
            str = str.Replace(")", "");
            str += " ~";
            j = str.IndexOf(",");
            int last_count = 0;

            if (j >= 0)
            {
                comma = true;
                last_count = count_words(str);
                str = str.Replace(",", "");
            }
            md = false;
            dr = false;
            sr = false;
            jr = false;
            ii = false;
            iii = false;
            dds = false;
            bro = false;
            rev = false;
            i = 0;
            nealint = j;
            for (; ; )
            {
                j = str.IndexOf(" ");
                if ((str.IndexOf(" ", 0, str.Length) < nealint) && (i == 0))
                    j = nealint;
                if (j < 0)
                    break;
                string text = str.Substring(0, j);
                if (text == "~")
                    break;
                if (text.ToUpper() == "MD")
                    md = true;
                else if (text.ToUpper() == "DR")
                    dr = true;
                else if (text.ToUpper() == "SR")
                    sr = true;
                else if (text.ToUpper() == "JR")
                    jr = true;
                else if (text.ToUpper() == "II")
                    ii = true;
                else if (text.ToUpper() == "III")
                    iii = true;
                else if (text.ToUpper() == "DDS")
                    dds = true;
                else if (text.ToUpper() == "BRO")
                    bro = true;
                else if (text.ToUpper() == "REV")
                    rev = true;
                else
                {
                    words[i] = text;
                    i++;
                }
                str = str.Substring(j + 1, str.Length - j - 1);
            }
            int start = 0;
            if (comma == true)
                start = last_count;
            newstr = "";
            for (k = start; k < i; k++)
            {
                if (newstr.Length > 0)
                    newstr += " ";
                newstr += words[k];
            }
            if (comma == true)
            {
                for (k = 0; k < last_count; k++)
                    newstr += " " + words[k];
            }

            if (md == true)
                newstr += ", MD";
            else if (dr == true)
                newstr = "Dr. " + newstr;
            else if (sr == true)
                newstr += ", Sr.";
            else if (jr == true)
                newstr += ", Jr.";
            else if (ii == true)
                newstr += ", II";
            else if (iii == true)
                newstr += ", III";
            else if (dds == true)
                newstr += ", DDS";
            return newstr.Trim();
        }
        /***********************************************************************************************/
        public static void ParseOutName(string name, ref string prefix, ref string firstName, ref string lastName, ref string miName, ref string suffix)
        {
            string[] title = new string[] { "DR", "BRO", "REV", "MR", "MRS", "MS", "BISHOP, PASTOR" };
            string[] endings = new string[] { "MD", "M.D.", "DO", "D.O.", "DDS", "SR", "JR", "II", "III" };
            string[] Lines = name.Split(' ');

            string str = "";
            string firstWord = "";
            string lastWord = "";

            prefix = "";
            firstName = "";
            lastName = "";
            miName = "";
            suffix = "";

            bool hasTitle = false;
            bool hasEnding = false;

            try
            {
                string[] words = name.Split(' ');
                if (words.Length <= 0)
                    return;

                firstWord = words[0].Trim();
                if (words.Length > 1)
                    lastWord = words[words.Length - 1].Trim();

                for (int i = 0; i < title.Length; i++)
                {
                    str = title[i];
                    if (firstWord == str )
                        hasTitle = true;
                    str = G1.force_lower_line(str);
                    if (firstWord == str )
                        hasTitle = true;
                    str = title[i] + ".";
                    if (firstWord == str )
                        hasTitle = true;
                    str = G1.force_lower_line(str);
                    if (firstWord == str )
                        hasTitle = true;
                    if (hasTitle)
                        break;
                }
                for (int i = 0; i < endings.Length; i++)
                {
                    str = endings[i];
                    if (lastWord == str )
                        hasEnding = true;
                    str = G1.force_lower_line(str);
                    if (lastWord == str )
                        hasEnding = true;
                    str = endings[i] + ".";
                    if (lastWord == str )
                        hasEnding = true;
                    str = G1.force_lower_line(str);
                    if (lastWord == str )
                        hasEnding = true;
                    if (hasEnding)
                        break;
                }
            }
            catch ( Exception ex )
            {
            }
            //bool hasEnding = endings.Any(name.Contains);
            //bool hasTitle = title.Any(name.Contains);
            if (hasTitle)
            {
                prefix = Lines[0].Trim();
                name = name.Replace(prefix, "").Trim();
            }
            if (hasEnding)
            {
                suffix = Lines[Lines.Length - 1].Trim();
                name = name.Replace(suffix, "").Trim();
            }
            Lines = name.Split(' ');
            firstName = "";
            miName = "";
            lastName = "";
            if (Lines.Length <= 0)
                return;
            if (Lines.Length == 1)
                lastName = Lines[0].Trim();
            else if (Lines.Length == 2)
            {
                firstName = Lines[0].Trim();
                lastName = Lines[1].Trim();
            }
            else if (Lines.Length == 3)
            {
                firstName = Lines[0].Trim();
                miName = Lines[1].Trim();
                if ( !String.IsNullOrWhiteSpace ( miName ))
                    miName = miName.Substring(0, 1) + ".";
                lastName = Lines[2].Trim();
            }
            else if (Lines.Length > 3)
            {
                firstName = Lines[0].Trim();
                miName = Lines[1].Trim();
                if (!String.IsNullOrWhiteSpace(miName))
                    miName = miName.Substring(0, 1) + ".";
                lastName = Lines[Lines.Length - 1].Trim();
            }
        }
        /****************************************************************************************/
        public static string BuildFullName ( DataRow dRow )
        {
            string fullName = "";

            try
            {
                string prefix = dRow["depPrefix"].ObjToString().Trim();
                string firstName = dRow["depFirstName"].ObjToString().Trim();
                string middleName = dRow["depMI"].ObjToString().Trim();
                string lastName = dRow["depLastName"].ObjToString().Trim();
                string suffix = dRow["depSuffix"].ObjToString().Trim();

                fullName = BuildFullName(prefix, firstName, middleName, lastName, suffix);
            }
            catch ( Exception ex )
            {
            }
            return fullName;
        }
        /****************************************************************************************/
        public static string BuildFullName(string prefix, string firstName, string mi, string lastName, string suffix)
        {
            string fullName = prefix;
            if (!String.IsNullOrWhiteSpace(fullName))
                fullName += " ";
            fullName += firstName;
            if (!String.IsNullOrWhiteSpace(firstName))
                fullName += " ";
            fullName += mi;
            if (!String.IsNullOrWhiteSpace(mi))
                fullName += " ";
            fullName += lastName;
            if (!String.IsNullOrWhiteSpace(lastName) && !String.IsNullOrWhiteSpace(suffix))
                fullName += ", ";
            fullName += suffix;
            return fullName;
        }
        /***********************************************************************************************/
        public static void ParseName(string name, ref string firstname, ref string mi, ref string lastname, bool fullNames = false )
        {
            firstname = "";
            mi = "";
            lastname = "";
            string[] Lines = name.Split(' ');
            for (int i = 0; i < Lines.Length; i++)
            {
                if (Lines[i].Trim().ToUpper() == "MD")
                    Lines[i] = "";
                else if (Lines[i].ToUpper() == "DO")
                    Lines[i] = "";
            }
            if (Lines.Length >= 1)
            {
                firstname = Lines[0].ToString().Trim();
                if (firstname.IndexOf(",") > 0)
                {
                    lastname = Lines[0].ObjToString().Replace(",", "").Trim();
                    if (Lines.Length >= 2)
                    {
                        firstname = Lines[1].ObjToString().Trim();
                        if (Lines.Length >= 3)
                        {
                            if (Lines[2].ToString().Trim().Length > 0)
                                mi = Lines[2].ObjToString().Trim();
                        }
                    }
                }
                else
                {
                    if (Lines.Length >= 2)
                    {
                        lastname = Lines[1].ToString().Trim();
                        if (Lines.Length >= 3)
                        {
                            if (Lines[2].ToString().Trim().Length > 0)
                            {
                                mi = lastname;
                                lastname = Lines[2].ToString().Trim();
                            }
                            else
                            {
                                mi = lastname;
                                lastname = Lines[Lines.Length - 1].ObjToString().Trim();
                            }
                        }
                    }
                }
            }
            if (mi.Trim().Length > 0)
                mi = mi.Replace(".", "");
            if (firstname.Length > 25)
                firstname = firstname.Substring(0, 24);
            if (lastname.Length > 25)
                lastname = lastname.Substring(0, 24);
            if (!fullNames)
            {
                if (mi.Length > 1)
                    mi = mi.Substring(0, 1);
            }
        }
        /***********************************************************************************************/
        public static int parse_date(int what, string date)
        {
            var dttmp = date.ObjToDateTime();
            if (dttmp == DateTime.MinValue)
                return -1;

            switch (what)
            {
                case 1:
                    return dttmp.Day;
                case 2:
                    return dttmp.Month;
                case 3:
                    return dttmp.Year;
                default:
                    return -1;
            }
        }
        /****************************************************************************************/
        public static DateTime GetDateBOM ( DateTime date  )
        {
            DateTime dateBOM = DateTime.MinValue;
            if (date.Year < 1000)
                return dateBOM;
            dateBOM = new DateTime(date.Year, date.Month, 1);
            return dateBOM;
        }
        /****************************************************************************************/
        public static DateTime GetDateEOM(DateTime date)
        {
            DateTime dateEOM = DateTime.MinValue;
            if (date.Year < 1000)
                return dateEOM;
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            dateEOM = new DateTime(date.Year, date.Month, days );
            return dateEOM;
        }
        /****************************************************************************************/
        public static DateTime DateToEOM ( DateTime date )
        {
            int days = DateTime.DaysInMonth(date.Year, date.Month);
            DateTime newDate = new DateTime(date.Year, date.Month, days);
            return newDate;
        }
        /****************************************************************************************/
        public static int parse_time(int what, string time)
        {
            var dttmp = time.ObjToDateTime();
            if (dttmp == DateTime.MinValue)
                return -1;

            switch (what)
            {
                case 1:
                    return dttmp.Hour;
                case 2:
                    return dttmp.Minute;
                case 3:
                    return dttmp.Second;
                default:
                    return -1;
            }
        }
        /***********************************************************************************************/
        public static string DateTimeToSQLDateTime(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd");
        }
        /***********************************************************************************************/
        public static object DTtoMySQLDT(object val)
        {
            DateTime dt;

            try
            {
                dt = ParseDateTime(val);
                if (dt >= DateTime.MinValue)
                    return new MySqlDateTime(dt);
                else
                    return DBNull.Value;

            }
            catch (Exception ex)
            {
                G1.LogError(ex.Message, ex, false);
                return DBNull.Value;
            }
        }
        /***********************************************************************************************/
        public static string DayOfWeekText(DateTime date)
        {
            string dow = "";
            if (date.DayOfWeek == DayOfWeek.Sunday)
                dow = "Sunday";
            else if (date.DayOfWeek == DayOfWeek.Monday)
                dow = "Monday";
            else if (date.DayOfWeek == DayOfWeek.Tuesday)
                dow = "Tuesday";
            else if (date.DayOfWeek == DayOfWeek.Wednesday)
                dow = "Wednesday";
            else if (date.DayOfWeek == DayOfWeek.Thursday)
                dow = "Thursday";
            else if (date.DayOfWeek == DayOfWeek.Friday)
                dow = "Friday";
            else if (date.DayOfWeek == DayOfWeek.Saturday)
                dow = "Saturday";
            return dow;
        }
        /***********************************************************************************************/
        public static string DayOfMonthText(DateTime date)
        {
            string dom = "";
            int day = date.Day;
            if (day == 1 || day == 21 || day == 31)
                dom = day.ToString() + "st";
            else if (day == 2 || day == 22)
                dom = day.ToString() + "nd";
            else if (day == 3 || day == 23)
                dom = day.ToString() + "rd";
            else
                dom = day.ToString() + "th";
            return dom;
        }
        /***********************************************************************************************/
        public static string GetSQLDate(string date)
        {
            //            string date = dt.Rows[row][columnName].ObjToString();
            string sql_date = G1.date_to_sql(date).Trim();
            if (sql_date == "0001-01-01")
                sql_date = "0000-00-00";
            return sql_date;
        }
        /***********************************************************************************************/
        public static string GetSQLDate(DataTable dt, int row, string columnName)
        {
            string date = dt.Rows[row][columnName].ObjToString();
            string sql_date = G1.date_to_sql(date).Trim();
            if (sql_date == "0001-01-01")
                sql_date = "0000-00-00";
            return sql_date;
        }
        /***********************************************************************************************/
        public static DateTime GetDateTimeNow ()
        {
            DateTime date = DateTime.Now;
            date = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            return date;
        }
        /***********************************************************************************************/
        public static string DTto24HrString(DateTime dt)
        {
            return dt.ToString("HH:mm:ss");
        }
        /***********************************************************************************************/
        public static string date_to_sql(string cdate)
        {
            DateTime dttmp = cdate.ObjToDateTime();
            return dttmp.ToString("yyyy-MM-dd");
        }
        /***********************************************************************************************/
        public static string days_to_date(long days)
        {
            DateTime dttmp = staticStart1900Date.AddDays(days);
            return dttmp.ToString("MM/dd/yyyy");
        }
        /***********************************************************************************************/
        public static long date_to_days(string date2)
        {
            var dttmp = date2.ObjToDateTime();
            return dttmp == DateTime.MinValue
                       ? 0
                       : (dttmp > staticStart1900Date
                              ? dttmp.Subtract(staticStart1900Date).Days
                              : staticStart1900Date.Subtract(dttmp).Days);
        }
        ///***********************************************************************************************/
        //public static string DTtoYMDString(DateTime dt)
        //{
        //    return dt.ToString("yyyy/MM/dd");
        //}
        /***********************************************************************************************/
        public static string DTtoYMDString(string date, string decodestring = "")
        {
            DateTime dt = date.ObjToDateTime();
            string rtnDate = dt.ToString("yyyy/MM/dd");
            try
            {
                if (!String.IsNullOrWhiteSpace(decodestring))
                    rtnDate = dt.ToString(decodestring);
            }
            catch
            {
            }
            return rtnDate;
        }
        /***********************************************************************************************/
        public static string DTtoYMDString(DateTime dt, string decodestring = "")
        {
            string rtnDate = dt.ToString("yyyy/MM/dd");
            try
            {
                if (!String.IsNullOrWhiteSpace(decodestring))
                    rtnDate = dt.ToString(decodestring);
            }
            catch
            {
            }
            return rtnDate;
        }
        /***********************************************************************************************/
        public static string DTtoSQLString(DateTime dt)
        {
            return dt.ToString("yyyy-MM-dd HH:mm:ss");
        }
        /***********************************************************************************************/
        public static DateTime ParseDateTime(object val)
        {
            DateTime dt;

            try
            {
                if (val == null || val == DBNull.Value)
                    return DateTime.MinValue;

                if (!DateTime.TryParse(val.ToString(), out dt))
                {
                    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo("en-US");
                    string[] expFormats = { "d", "g", "G", "t", "T", "yyyyMMdd", "MMddyyyy", "yyyyMMddHHmm", "yyyyMMddHHmmss", "MMddyyyyHHmmss", "yyyy-M-d", "yyyy/M/d", "yyyy/M/d HH:mm:ss", "yyyy-M-d HH:mm:ss", "yyyy-M-d HH:mm", "HH:mm", "HH:mm:ss", "MMddyy" };

                    if (!DateTime.TryParseExact(val.ToString(), expFormats, ci, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out dt))
                        return DateTime.MinValue;
                }

                return dt;

            }
            catch (Exception ex)
            {
                //                G1.LogError(ex.Message, ex, false);
                return DateTime.MinValue;
            }
        }
        /***********************************************************************************************/
        public static int GetMonthsBetween(DateTime from, DateTime to, DateTime dolp)
        {
            int months = 0;
            DateTime originalDOLP = dolp;
            DateTime date1 = new DateTime(from.Year, from.Month, from.Day);
            DateTime date2 = new DateTime(to.Year, to.Month, to.Day);
            for (; ; )
            {
                dolp = dolp.AddMonths(1);
//                if (dolp >= from && dolp <= to)
                if (dolp >= from )
                {
                    if (dolp >= date2)
                    {
                        if ( originalDOLP <= date2 && months == 0 )
                            months++;
                        break;
                    }
                    months++;
                    continue;
                }
                else if (date1 == date2)
                {
                    months = GetMonthsBetween(to, originalDOLP);
                }
                else
                {
                    months = GetMonthsBetween(to, originalDOLP);

                }
                break;
            }
            return months;
        }
        /***********************************************************************************************/
        public static int GetMonthsBetween(DateTime from, DateTime to)
        {
            int months = ((from.Year - to.Year) * 12) + from.Month - to.Month;
            if (1 == 1)
                return months;
            if (from > to) return GetMonthsBetween(to, from);

            var monthDiff = Math.Abs((to.Year * 12 + (to.Month - 1)) - (from.Year * 12 + (from.Month - 1)));

            if (from.AddMonths(monthDiff) > to || to.Day < from.Day)
            {
                return monthDiff - 1;
            }
            else
            {
                return monthDiff;
            }
        }
        /***********************************************************************************************/
        public static void CalculateYourAge(DateTime Bday, DateTime Cday, ref int years, ref int months, ref int days )
        {
            if ((Cday.Year - Bday.Year) > 0 ||
            (((Cday.Year - Bday.Year) == 0) && ((Bday.Month < Cday.Month) ||
            ((Bday.Month == Cday.Month) && (Bday.Day <= Cday.Day)))))
            {
                int DaysInBdayMonth = DateTime.DaysInMonth(Bday.Year, Bday.Month);
                int DaysRemain = Cday.Day + (DaysInBdayMonth - Bday.Day);
                if (Cday.Month > Bday.Month)
                {
                    years = Cday.Year - Bday.Year;
                    months = Cday.Month - (Bday.Month + 1) + Math.Abs(DaysRemain / DaysInBdayMonth);
                    days = (DaysRemain % DaysInBdayMonth + DaysInBdayMonth) % DaysInBdayMonth;
                }
                else if (Cday.Month == Bday.Month)
                {
                    if (Cday.Day >= Bday.Day)
                    {
                        years = Cday.Year - Bday.Year;
                        months = 0;
                        days = Cday.Day - Bday.Day;
                    }
                    else
                    {
                        years = (Cday.Year - 1) - Bday.Year;
                        months = 11;
                        days = DateTime.DaysInMonth(Bday.Year, Bday.Month) - (Bday.Day - Cday.Day);
                    }
                }
                else
                {
                    years = (Cday.Year - 1) - Bday.Year;
                    months = Cday.Month + (11 - Bday.Month) + Math.Abs(DaysRemain / DaysInBdayMonth);
                    days = (DaysRemain % DaysInBdayMonth + DaysInBdayMonth) % DaysInBdayMonth;
                }
            }
            else
            {
                throw new ArgumentException("Birthday date must be earlier than current date");
            }
        }
        /***********************************************************************************************/
        public static void AgeCount(DateTime Bday, DateTime Cday, ref int years, ref int months, ref int days )
        {
            if ((Cday.Year - Bday.Year) > 0 ||
            (((Cday.Year - Bday.Year) == 0) && ((Bday.Month < Cday.Month) ||
            ((Bday.Month == Cday.Month) && (Bday.Day <= Cday.Day)))))
            {
                int DaysInBdayMonth = DateTime.DaysInMonth(Bday.Year, Bday.Month);
                int DaysRemain = Cday.Day + (DaysInBdayMonth - Bday.Day);
                if (Cday.Month > Bday.Month)
                {
                    years = Cday.Year - Bday.Year;
                    months = Cday.Month - (Bday.Month + 1) + Math.Abs(DaysRemain / DaysInBdayMonth);
                    days = (DaysRemain % DaysInBdayMonth + DaysInBdayMonth) % DaysInBdayMonth;
                }
                else if (Cday.Month == Bday.Month)
                {
                    if (Cday.Day >= Bday.Day)
                    {
                        years = Cday.Year - Bday.Year;
                        months = 0;
                        days = Cday.Day - Bday.Day;
                    }
                    else
                    {
                        years = (Cday.Year - 1) - Bday.Year;
                        months = 11;
                        days = DateTime.DaysInMonth(Bday.Year, Bday.Month) - (Bday.Day - Cday.Day);
                    }
                }
                else
                {
                    years = (Cday.Year - 1) - Bday.Year;
                    months = Cday.Month + (11 - Bday.Month) + Math.Abs(DaysRemain / DaysInBdayMonth);
                    days = (DaysRemain % DaysInBdayMonth + DaysInBdayMonth) % DaysInBdayMonth;
                }
            }
            else
            {
                throw new ArgumentException("Birthday date must be earlier than current date");
            }
        }
        /***********************************************************************************************/
        public static string GetEncriptedWord(string word = "")
        {
            if (String.IsNullOrWhiteSpace(word))
                word = "xyzzy";
            string encryptedString = EncryptStringSample.StringCipher.Encrypt(word, "GSYAHAGCBDUUADIADKOPAAAW");
            return encryptedString;
        }
        /***********************************************************************************************/
        public static string GetDecriptedWord(string word)
        {
            if (String.IsNullOrWhiteSpace(word))
                return "";
            string decryptedString = EncryptStringSample.StringCipher.Decrypt(word, "GSYAHAGCBDUUADIADKOPAAAW");
            return decryptedString;
        }
        /***********************************************************************************************/
        public static string try_protect_data(string str)
        {
            int j = 8217; // This is some sort of apostrophe
            string test = "";
            test += (char)j;
            int idx = str.IndexOf("'");
            if (idx >= 0)
                str = str.Replace("'", test);
            //idx = str.IndexOf("&");
            //if (idx >= 0)
            //    str = str.Replace("&", " and ");
            //str = str.Replace("\'", "\\'");
            //str = str.Replace("\\", "");
            //str = str.Replace("\"", "\\\"");

            return str;
        }
        /***********************************************************************************************/
        public static string protect_data(string str)
        {
            int j = 8217; // This is some sort of apostrophe
            string test = "";
            test += (char)j;
            str = str.Replace(test, "'");
            int idx = str.IndexOf("'");
            if (idx >= 0)
                str = str.Replace("'", "`");
            idx = str.IndexOf("'");
            if (idx >= 0)
                str = str.Replace("'", "`");
            idx = str.IndexOf("\"");
            if (idx >= 0)
                str = str.Replace("\"", "`");
            idx = str.IndexOf("\\");
            if (idx >= 0)
                str = str.Replace("\\", "/");
            return str;
        }
        /***********************************************************************************************/
        public static bool update_db_table(string db, string keyfield, string record, string Myfields)
        {
            string[] Lines = Myfields.Split(',');
            if (Lines.Length % 2 != 0)
                return false;
            int count = Lines.Length;
            string[] fields = new string[count];
            for (int i = 0; i < count; i = i + 2)
            {
                fields[i] = Lines[i];
                fields[i + 1] = Lines[i + 1];
            }
            bool found = update_db_table(db, keyfield, record, fields);
            return found;
        }
        /***********************************************************************************************/
        public static bool update_db_table(string db, string keyfield, string record, string[] fields)
        {
            if ((fields.Length % 2) != 0)
                return false;

            if (String.IsNullOrWhiteSpace(record))
                return false;

            bool found = false;
            string field = "";
            string data = "";
            string cmd = "Update `" + db + "` SET ";
            for (int i = 0; i < fields.Length; i = i + 2)
            {
                field = fields[i].ToString();
                data = fields[i + 1].ToString();
                if (data == "NODATA")
                    data = "";
                string command = "select column_name,data_type,column_key,character_maximum_length,column_default from information_schema.`COLUMNS` where table_schema = 'smfs'";
                command += " and table_name = '" + db + "' and column_name = '" + field + "' ;";
                DataTable dx = G1.get_db_data(command);
                if (dx.Rows.Count <= 0)
                    continue;
                string type = "";
                string def = "";
                int len = 0;
                try
                {
                    type = dx.Rows[0]["data_type"].ToString().ToUpper();
                    def = dx.Rows[0]["column_default"].ToString();
                    len = dx.Rows[0]["character_maximum_length"].ObjToInt32();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updaing Field " + field + " Error " + ex.Message.ToString(), "Update Data Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    continue;
                }
                //				MySQL.ColumnStructure cs = MySQL.getColumnStructure(db, field);
                //string type = cs.Type;
                //string def = cs.Default;

                if (type == "INT" || type == "BIGINT")
                {
                    if (data == null)
                        data = def;
                    if (data == "")
                        data = def;
                    data = data.Replace(",", "");
                    if (found)
                        cmd += ", ";
                    cmd += " `" + field + "` = '" + data + "' ";
                    found = true;
                }
                else if (type == "CHAR" || type == "VARCHAR" || type == "TEXT")
                {
                    if (data == null)
                        data = " ";
                    else if (data == "")
                        data = "";
                    if (found)
                        cmd += ", ";
                    data = G1.try_protect_data(data);
                    if (data.Length > len)
                        data = data.Substring(0, len - 1);
                    cmd += " `" + field + "` = '" + data + "' ";
                    found = true;
                }
                else if (type == "TINYINT")
                {
                    if (data == "")
                        data = "0";
                    if (data.ToUpper() == "FALSE")
                        data = "0";
                    else if (data.ToUpper() == "0")
                        data = "0";
                    else
                        data = "1";
                    if (found)
                        cmd += ", ";
                    cmd += " `" + field + "` = '" + data + "' ";
                    found = true;
                }
                else if (type == "FLOAT" || type == "DOUBLE" || type == "DECIMAL")
                {
                    if (data == null)
                        data = "0.0";
                    if (data == "")
                        data = "0.0";
                    if (data == "0")
                        data = "0.0";
                    if (data.IndexOf(".") < 0)
                        data += ".0";
                    data = data.Replace(",", "");
                    if (found)
                        cmd += ", ";
                    cmd += " `" + field + "` = '" + data + "' ";
                    found = true;
                }
                else if (type == "DATETIME")
                {
                    if (data.Trim().Length == 0)
                        continue;
                    DateTime dt = G1.ParseDateTime(data);
                    if (found)
                        cmd += ", ";
                    cmd += " `" + field + "` = '" + G1.DTtoSQLString(dt) + "' ";
                    found = true;
                }
                else if (type == "DATE")
                {
                    if (data.Trim().Length == 0)
                        data = "0000-00-00";
                    DateTime dt = G1.ParseDateTime(data);
                    if (found)
                        cmd += ", ";
                    if (data == "0000-00-00")
                    {
                        cmd += " `" + field + "` = '0000-00-00' ";
                    }
                    else
                        cmd += " `" + field + "` = '" + G1.DTtoYMDString(dt) + "' ";
                    found = true;
                    #region Old
                    //if (data.Trim().Length == 0)
                    //    continue;
                    //long ldate = G1.date_to_days(data);
                    //data = G1.days_to_date(ldate);
                    //data = G1.date_to_sql(data);
                    //if (found)
                    //    cmd += ", ";
                    //cmd += " `" + field + "` = '" + data + "' ";
                    //found = true;
                    #endregion
                }
                else if (type == "TIME")
                {
                    if (data.Trim().Length == 0)
                        continue;
                    DateTime dt = G1.ParseDateTime(data);
                    if (found)
                        cmd += ", ";
                    cmd += " `" + field + "` = '" + G1.DTto24HrString(dt) + "' ";
                    found = true;
                    #region Old
                    if (data.Trim().Length == 0)
                        continue;
                    int year = G1.parse_date(3, data);
                    int hour = G1.parse_time(1, data);
                    if (hour < 0)
                        hour = 0;
                    int minute = G1.parse_time(2, data);
                    if (minute < 0)
                        minute = 0;
                    int seconds = G1.parse_time(3, data);
                    if (seconds < 0)
                        seconds = 0;
                    string stamp = hour.ToString("D2") + minute.ToString("D2") + seconds.ToString("D2");
                    if (found)
                        cmd += ", ";
                    cmd += " `" + field + "` = '" + stamp + "' ";
                    found = true;
                    #endregion
                }
            }
            if (found)
            {
                cmd += " where `" + keyfield + "` = '" + record + "';";
                try
                {
                    G1.get_db_data(cmd);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updating Field '" + field + "/" + data + "' " + ex.Message.ToString());
                }
            }
            return found;
        }

        /***********************************************************************************************/
        public static void update_record(string table, string record, string field, string data)
        {
            if (record == "")
                return;
            if (record == "0")
                return;
            data = protect_data(data);
            string command = "Update " + table + " Set " + field + " = '" + data + "' ";
            command += " where record = '" + record + "' ";
            MySqlCommand cmd = new MySqlCommand(command, G1.conn1);
            G1.OpenConnection(G1.conn1);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                G1.LogError("A critical exception has occurred while attempting to aquire a record for field " + field + ":\n" + ex.Message, ex, true);
            }
            finally
            {
                G1.CloseConnection(G1.conn1);
            }
        }
        /********************************************************************************************/
        public static string reformat_datetime(string cdate)
        {
            if (cdate == String.Empty)
                cdate = DateTime.MinValue.ToString();

            string sql_date = cdate;
            string sql_time = "";
            sql_date.Trim();
            int xlen = sql_date.IndexOf(" ", 0, sql_date.Length);
            if (xlen > 0)
            {
                sql_date = sql_date.Substring(0, xlen);
                sql_time = cdate.Substring(xlen, (cdate.Length - xlen));
            }
            sql_date.Trim();
            xlen = sql_date.IndexOf("/", 0, sql_date.Length);
            string mm = sql_date.Substring(0, xlen);
            sql_date = sql_date.Substring((xlen + 1), (sql_date.Length - (xlen + 1)));
            xlen = sql_date.IndexOf("/", 0, sql_date.Length);
            string dd = sql_date.Substring(0, xlen);
            sql_date = sql_date.Substring((xlen + 1), (sql_date.Length - (xlen + 1)));
            int i = myint(mm);
            mm = i.ToString("d2");
            i = myint(dd);
            dd = i.ToString("d2");
            string newdate = mm + "/" + dd + "/" + sql_date;
            sql_time = sql_time.Trim();
            xlen = sql_time.IndexOf(":", 0, sql_time.Length);
            string hh = sql_time.Substring(0, xlen);
            i = myint(hh);
            if (sql_time.IndexOf("PM", 0, sql_time.Length) >= 0 && i < 12)
                i = i + 12;
            hh = i.ToString("d2");
            sql_time = sql_time.Substring((xlen + 1), (sql_time.Length - (xlen + 1)));
            xlen = sql_time.IndexOf(":");
            string mn = sql_time.Substring(0, xlen);
            i = myint(mn);
            mn = i.ToString("d2");

            newdate += " " + hh + ":" + mn;
            if ((xlen + 1) < sql_time.Length)
            {
                sql_time = sql_time.Substring((xlen + 1)).Trim();
                sql_time = sql_time.Replace("AM", "");
                sql_time = sql_time.Replace("PM", "");
                sql_time = sql_time.Trim();
                if (validate_numeric(sql_time))
                {
                    int seconds = myint(sql_time);
                    string ss = seconds.ToString("d2");
                    newdate += ":" + ss;
                }
            }

            return newdate;
        }
        /***********************************************************************************************/
        public static void ConvertToTable(DataRow[] dRows, DataTable dt)
        {
            dt.Rows.Clear();
            for (int i = 0; i < dRows.Length; i++)
                dt.ImportRow(dRows[i]);
        }
        /***************************************************************************************/
        public static void duplicate_dt_column(DataTable fromdt, string from, DataTable todt )
        {
            if (G1.get_column_number(fromdt, from) < 0)
                return;
            if (G1.get_column_number ( todt, from ) < 0)
            {
                try
                {
                    string type = fromdt.Columns[from].DataType.ToString();
                    if (type.ToUpper().IndexOf("MYSQLDATETIME") >= 0)
                        type = "System.DateTime";
                    todt.Columns.Add(from, Type.GetType(type));
                }
                catch ( Exception ex)
                {
                }
            }

            if ( fromdt.Rows.Count > todt.Rows.Count )
            {
                int rows = fromdt.Rows.Count - todt.Rows.Count;
                for ( int i=0; i<rows; i++)
                {
                    try
                    {
                        DataRow dRow = todt.NewRow();
                        todt.Rows.Add(dRow);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }

            try
            {
                copy_dt_column(fromdt, from, todt, from);
            }
            catch ( Exception ex)
            {
            }
        }
        /***************************************************************************************/
        public static void copy_dt_column(DataTable fromdt, string from, DataTable todt, string to)
        {
            if (G1.get_column_number(fromdt, from) < 0)
                return;
            if (G1.get_column_number(todt, to) < 0)
                return;

            string type = fromdt.Columns[from].DataType.ToString().ToUpper();


            for (int i = 0; i < fromdt.Rows.Count; i++)
            {
                if (type.IndexOf("MYSQLDATETIME") >= 0)
                    todt.Rows[i][to] = G1.DTtoMySQLDT(fromdt.Rows[i][from]);
                else if (type.IndexOf("DOUBLE") >= 0)
                    todt.Rows[i][to] = fromdt.Rows[i][from].ObjToDouble();
                else if (type.IndexOf("DECIMAL") >= 0)
                    todt.Rows[i][to] = fromdt.Rows[i][from].ObjToDecimal();
                else if (type.IndexOf("INT32") >= 0)
                    todt.Rows[i][to] = fromdt.Rows[i][from].ObjToInt32();
                else if (type.IndexOf("INT64") >= 0)
                    todt.Rows[i][to] = fromdt.Rows[i][from].ObjToInt64();
                else
                    todt.Rows[i][to] = fromdt.Rows[i][from].ObjToString();
            }
        }
        /***************************************************************************************/
        public static bool HardCopyDataTable(DataTable fromdt, DataTable todt, bool showError = false)
        {
            bool success = true;
            try
            {
                bool good = false;
                for (int i = 0; i < fromdt.Rows.Count; i++)
                    good = G1.HardCopyDtRow(fromdt, i, todt, todt.Rows.Count);
            }
            catch
            {
                success = false;
                if (showError)
                    MessageBox.Show("***ERROR*** Problem Copying DataTable!");
            }
            return success;
        }
        /***************************************************************************************/
        public static bool HardCopyDtRow(DataTable fromdt, int from, DataTable todt, int to, bool showError = false)
        {
            string type = "";
            string toType = "";
            string name = "";
            int to_i = 0;
            bool success = true;
            try
            {
                if (todt.Columns.Count < fromdt.Columns.Count)
                {
                    for (int i = 0; i < fromdt.Columns.Count; i++)
                    {
                        try
                        {
                            name = fromdt.Columns[i].ColumnName.ObjToString();
                            if (G1.get_column_number(todt, name) < 0)
                            {
                                type = fromdt.Columns[i].DataType.ToString();
                                todt.Columns.Add(name, Type.GetType(type));
                            }
                        }
                        catch ( Exception ex)
                        {
                        }
                    }
                }
                if (to > (todt.Rows.Count - 1))
                {
                    DataRow dRow = todt.NewRow();
                    todt.Rows.Add(dRow);
                }
                for (int i = 0; i < fromdt.Columns.Count; i++)
                {
                    type = fromdt.Columns[i].DataType.ToString().ToUpper();
                    if (type == null)
                        continue;
                    name = fromdt.Columns[i].ColumnName.ObjToString();
                    if (String.IsNullOrWhiteSpace(name))
                        continue;
                    try
                    {
                        to_i = G1.get_column_number(todt, name);
                        if (to_i < 0)
                            continue;
                        if ( name.ToUpper() == "PRICE1")
                        {
                        }
                        toType = todt.Columns[to_i].DataType.ObjToString().ToUpper();
                        if (toType != type)
                        {
                            if (toType.IndexOf("MYSQLDATETIME") >= 0)
                                todt.Rows[to][to_i] = G1.DTtoMySQLDT(fromdt.Rows[from][i].ObjToString());
                            else if (toType.IndexOf("DOUBLE") >= 0)
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToString().ObjToDouble();
                            else if (toType.IndexOf("DECIMAL") >= 0)
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToString().ObjToDecimal();
                            else if (toType.IndexOf("INT32") >= 0)
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToString().ObjToInt32();
                            else if (toType.IndexOf("INT64") >= 0)
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToString().ObjToInt64();
                            else if (toType.ToUpper() == "SYSTEM.BYTE[]")
                                continue;
                            else
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToString();
                        }
                        else
                        {
                            if (type.IndexOf("MYSQLDATETIME") >= 0)
                                todt.Rows[to][to_i] = G1.DTtoMySQLDT(fromdt.Rows[from][i]);
                            else if (type.IndexOf("DOUBLE") >= 0)
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToDouble();
                            else if (type.IndexOf("DECIMAL") >= 0)
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToDecimal();
                            else if (type.IndexOf("INT32") >= 0)
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToInt32();
                            else if (type.IndexOf("INT64") >= 0)
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToInt64();
                            else if (type.ToUpper() == "SYSTEM.BYTE[]")
                                continue;
                            else
                                todt.Rows[to][to_i] = fromdt.Rows[from][i].ObjToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("***ERROR*** " + ex.Message.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                if (showError)
                    MessageBox.Show("***ERROR*** Name=" + name + " " + ex.ToString());
                success = false;
            }
            todt.AcceptChanges();
            return success;
        }
        /***************************************************************************************/
        public static bool copyDataTable(DataTable fromdt, DataTable todt, bool showError = false)
        {
            bool success = true;
            try
            {
                bool good = false;
                for (int i = 0; i < fromdt.Rows.Count; i++)
                    good = G1.copy_dt_row(fromdt, i, todt, todt.Rows.Count);
            }
            catch
            {
                success = false;
                if (showError)
                    MessageBox.Show("***ERROR*** Problem Copying DataTable!");
            }
            return success;
        }
        /***************************************************************************************/
        public static bool copy_dt_row(DataTable fromdt, int from, DataTable todt, int to, bool showError = false)
        {
            string type = "";
            string name = "";
            bool success = true;
            try
            {
                if (todt.Columns.Count < fromdt.Columns.Count)
                {
                    for (int i = 0; i < fromdt.Columns.Count; i++)
                    {
                        name = fromdt.Columns[i].ColumnName.ObjToString();
                        if (G1.get_column_number(todt, name) < 0)
                        {
                            type = fromdt.Columns[i].DataType.ToString();
                            todt.Columns.Add(name, Type.GetType(type));
                        }
                    }
                }
                if (to > (todt.Rows.Count - 1))
                {
                    DataRow dRow = todt.NewRow();
                    todt.Rows.Add(dRow);
                }
                for (int i = 0; i < fromdt.Columns.Count; i++)
                {
                    type = fromdt.Columns[i].DataType.ToString().ToUpper();
                    try
                    {
                        if (type.IndexOf("MYSQLDATETIME") >= 0)
                            todt.Rows[to][i] = G1.DTtoMySQLDT(fromdt.Rows[from][i]);
                        else if (type.IndexOf("DOUBLE") >= 0)
                            todt.Rows[to][i] = fromdt.Rows[from][i].ObjToDouble();
                        else if (type.IndexOf("DECIMAL") >= 0)
                            todt.Rows[to][i] = fromdt.Rows[from][i].ObjToDecimal();
                        else if (type.IndexOf("INT32") >= 0)
                            todt.Rows[to][i] = fromdt.Rows[from][i].ObjToInt32();
                        else if (type.IndexOf("INT64") >= 0)
                            todt.Rows[to][i] = fromdt.Rows[from][i].ObjToInt64();
                        else
                            todt.Rows[to][i] = fromdt.Rows[from][i].ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                if (showError)
                    MessageBox.Show("***ERROR*** " + ex.ToString());
                success = false;
            }
            todt.AcceptChanges();
            return success;
        }
        /***************************************************************************************/
        public static bool copy_dr_row(DataRow fromdt, DataRow todt, bool showError = false)
        {
            string type = "";
            string name = "";
            bool success = true;

            DataTable fromTable = fromdt.Table;
            DataTable toTable = todt.Table;
            try
            {
                if (todt.Table.Columns.Count < fromdt.Table.Columns.Count)
                {
                    for (int i = 0; i < fromdt.Table.Columns.Count; i++)
                    {
                        name = fromdt.Table.Columns[i].ColumnName.ObjToString();
                        if (G1.get_column_number(todt.Table, name) < 0)
                        {
                            type = fromdt.Table.Columns[i].DataType.ToString();
                            todt.Table.Columns.Add(name, Type.GetType(type));
                        }
                    }
                }
                //if (to > (todt.Table.Rows.Count - 1))
                //{
                //    DataRow dRow = todt.Table.NewRow();
                //    todt.Table.Rows.Add(dRow);
                //}
                for (int i = 0; i < fromdt.Table.Columns.Count; i++)
                {
                    type = fromdt.Table.Columns[i].DataType.ToString().ToUpper();
                    try
                    {
                        if (type.IndexOf("MYSQLDATETIME") >= 0)
                            todt[i] = G1.DTtoMySQLDT(fromdt[i]);
                        else if (type.IndexOf("DOUBLE") >= 0)
                            todt[i] = fromdt[i].ObjToDouble();
                        else if (type.IndexOf("INT32") >= 0)
                            todt[i] = fromdt[i].ObjToInt32();
                        else if (type.IndexOf("INT64") >= 0)
                            todt[i] = fromdt[i].ObjToInt64();
                        else if (type.IndexOf("BYTE") >= 0)
                            todt[i] = fromdt[i].ObjToBytes();
                        else if (type.IndexOf("BITMAP") >= 0)
                            continue;
                        else
                            todt[i] = fromdt[i].ToString();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                if (showError)
                    MessageBox.Show("***ERROR*** " + ex.ToString());
                success = false;
            }
            todt.AcceptChanges();
            return success;
        }
        /***********************************************************************************************/
        public static string lastMySqlErrorMessage;
        /****************************************************************************************/
        public static int update_blob(string db, string key, string record, string field, Byte[] obj)
        {
            MySqlCommand cmd = new MySqlCommand("Update `" + db + "` Set `" + field + "` = (@val) where `" + key + "` = '" + record + "' ", G1.conn1);
            cmd.Parameters.AddWithValue("@val", obj);
            int affected = MySQL.TryNonQuery(cmd, true, false);
            return affected;
        }
        /***********************************************************************************************/
        public static byte[] GetBytesFromStream(MemoryStream fs)
        {
            fs.Flush();

            fs.Position = 0;

            byte[] bytes = new byte[fs.Length];
            fs.Read(bytes, 0, bytes.Length);

            return bytes;
        }
        /***********************************************************************************************/
        public static Image ImageFromBytes(byte[] bytes)
        {
            try
            {
                if (bytes == null || bytes.Length == 0)
                    return null;

                var st = new MemoryStream();
                st.Write(bytes, 0, bytes.Length);
                return Image.FromStream(st);
            }
            catch
            {
                return null;
            }
        }
        /***********************************************************************************************/
        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            Image returnImage = new Bitmap(1, 1);
            try
            {
                MemoryStream ms = new MemoryStream(byteArrayIn);
                returnImage = Image.FromStream(ms);
            }
            catch (Exception)
            {

            }
            return returnImage;
        }
        /***********************************************************************************************/
        public static byte[] imageToByteArray(System.Drawing.Image imageIn)
        {
            MemoryStream ms = new MemoryStream();
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            return ms.ToArray();
        }
        /***********************************************************************************************/
        public static void sortTable(DataTable dt, string name, string direction)
        {
            dt.AcceptChanges();
            if (dt.Rows.Count <= 0)
                return;
            try
            {
                DataTable t = dt.Copy();
                t.AcceptChanges();
                DataRow[] r = t.Select("", name + " " + direction);
                if (r.Length <= 0)
                    return;

                DataTable dx = r.CopyToDataTable();
                dt.Rows.Clear();
                for (int i = 0; i < r.Length; i++)
                    dt.ImportRow(r[i]);
                try
                { // Just in case there is no column named Num
                    for (int i = 1; i <= dt.Rows.Count; i++)
                        dt.Rows[i - 1]["Num"] = i.ToString();
                }
                catch
                {
                    return;
                }
            }
            catch ( Exception ex)
            {
            }
            return;
        }
        /***********************************************************************************************/
        public static DataTable GetGroupBy (DataTable dt, string byColumn)
        {
            if (dt.Rows.Count <= 0)
                return dt;

            DataTable groupDt = dt.Clone();
            //DataTable newDt = dt.Clone();

            try
            {
                //if (G1.get_column_number(dt, "Int32_id") < 0)
                //    dt.Columns.Add("Int32_id", typeof(int), "num");

                //groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r[byColumn] }).Select(g => g.OrderBy(r => r["Int32_id"]).First()).CopyToDataTable();
                //groupDt.Columns.Remove("Int32_id");


                groupDt = dt.AsEnumerable().GroupBy(r => new { Col1 = r[byColumn] }).Select(g => g.OrderBy(r => r[byColumn]).First()).CopyToDataTable();
            }
            catch (Exception ex)
            {
            }
            return groupDt;
        }
        /****************************************************************************************/
        public static void GoToLastRow(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            if (gridMain == null)
                return;
            if (gridMain.GridControl == null)
                return;
            DevExpress.XtraGrid.GridControl dgv = gridMain.GridControl;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;

            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = dt.Rows.Count - 1;

                gridMain.TopRowIndex = gridMain.RowCount - 1;
                gridMain.SelectRow(gridMain.RowCount - 1);
                gridMain.FocusedRowHandle = gridMain.RowCount - 1;
                gridMain.RefreshEditor(true);
                gridMain.RefreshData();
                //dgv.RefreshDataSource();
                //gridMain.FocusedRowHandle = row;
                dgv.Refresh();

                //gridMain.SelectRow(row);
                //gridMain.FocusedRowHandle = row;
                //gridMain.RefreshData();
                //gridMain.RefreshEditor(true);
                //dgv.RefreshDataSource();
                //dgv.Refresh();
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        public static void GoToFirstRow(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            if (gridMain == null)
                return;
            if (gridMain.GridControl == null)
                return;
            DevExpress.XtraGrid.GridControl dgv = gridMain.GridControl;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;

            try
            {
                DataTable dt = (DataTable)dgv.DataSource;
                int row = 0;

                gridMain.TopRowIndex = 0;
                gridMain.SelectRow(0);
                gridMain.FocusedRowHandle = 0;
                gridMain.RefreshEditor(true);
                gridMain.RefreshData();
                //dgv.RefreshDataSource();
                //gridMain.FocusedRowHandle = row;
                dgv.Refresh();

                //gridMain.SelectRow(row);
                //gridMain.FocusedRowHandle = row;
                //gridMain.RefreshData();
                //gridMain.RefreshEditor(true);
                //dgv.RefreshDataSource();
                //dgv.Refresh();
            }
            catch (Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        public static double Round(double value, int digits)
        {
            if ((digits < -15) || (digits > 15))
                throw new ArgumentOutOfRangeException("digits", "Rounding digits must be between -15 and 15, inclusive.");

            if (digits >= 0)
                return Math.Round(value, digits);

            double n = Math.Pow(10, -digits);
            return Math.Round(value / n, 0) * n;
        }
        /***********************************************************************************************/
        public static bool WithInPenny(double value1, double value2)
        {
            bool yes = false;
            double value = value1 - value2;
            value = G1.RoundValue(value);
            if (value >= -0.01 && value <= 0.01)
                yes = true;
            return yes;
        }
        /***********************************************************************************************/
        public static double RoundValue(double value)
        {
            if (value < 0D)
            {
                long lvalue = (long)((value - .005D) * 100.0D);
                value = (double)(lvalue) / 100.0D;
            }
            else
            {
                long lvalue = (long)((value + .005D) * 100.0D);
                value = (double)(lvalue) / 100.0D;
            }
            //            Math.Floor(41.75 * 0.1);
            return value;
        }
        /***********************************************************************************************/
        public static double RoundUp(double value)
        {
            return Math.Ceiling(value * Math.Pow(10, 2)) / Math.Pow(10, 2);
        }
        /***********************************************************************************************/
        public static double RoundDown(double value)
        {
            return Math.Floor(value * Math.Pow(10, 2)) / Math.Pow(10, 2);
        }
        /***********************************************************************************************/
        public static void sleep(int milliseconds)
        {
            System.Threading.Thread.Sleep(milliseconds); // Sleep in Milliseconds
        }
        /****************************************************************************************/
        public static DataTable RemoveDuplicates(DataTable dt, string columnName)
        {
            DataTable newDt = dt.Copy();
            try
            {
                newDt = dt.AsEnumerable()
                                 .GroupBy(x => x.Field<string>(columnName))
                                 .Select(y => y.First())
                                 .CopyToDataTable();
            }
            catch (Exception ex)
            {
            }
            return newDt;
        }
        /****************************************************************************************/
        public static DataTable RemoveDuplicates(DataTable dt, string columnName, string columnName2, bool first = true )
        {
            DataTable newDt = dt.Copy();
            try
            {
                if (first)
                {
                    newDt = dt.AsEnumerable()
                    .OrderBy(x => x.Field<string>(columnName))
                    .GroupBy(x => new { name = x.Field<string>(columnName), dept = x.Field<string>(columnName2) })
                    .Select(x => x.First())
                    .CopyToDataTable();
                }
                else
                {
                    newDt = dt.AsEnumerable()
                    .OrderBy(x => x.Field<string>(columnName))
                    .GroupBy(x => new { name = x.Field<string>(columnName), dept = x.Field<string>(columnName2) })
                    .Select(x => x.Last())
                    .CopyToDataTable();
                }

            }
            catch (Exception ex)
            {
            }
            return newDt;
        }
        /****************************************************************************************/
        public static string CheckForDuplicates(DataTable dt, string columnName)
        {
            string found = "";
            string str1 = "";
            string str2 = "";
            double price = 0D;
            try
            {
                for ( int i=0; i<dt.Rows.Count; i++)
                {
                    str1 = dt.Rows[i][columnName].ObjToString();
                    price = dt.Rows[i]["price"].ObjToDouble();
                    if (price == 0D)
                        continue;
                    for ( int j=0; j<dt.Rows.Count; j++)
                    {
                        if (j == i)
                            continue;
                        str2 = dt.Rows[j][columnName].ObjToString();
                        price = dt.Rows[j]["price"].ObjToDouble();
                        if (price == 0D)
                            continue;
                        if ( str2 == str1 )
                        {
                            found = str1;
                            break;
                        }
                    }
                    if (!String.IsNullOrWhiteSpace(found))
                        break;
                }
            }
            catch ( Exception ex )
            {
            }
            return found;
        }
        /********************************************************************************************/
        public static void parse_answer_data(string str, string delimiter, bool removeEmpty = false )
        {
            for (int i = 0; i < 499; i++)
                of_answer[i] = "";
            of_ans_count = 0;
            str += delimiter;
            int length = str.Length;
            bool quote = false;
            int delim = (int)delimiter[0];
            string data = "";
            byte[] pstr = new byte[500];
            int count = 0;
            for (int i = 0; i < length; i++)
            {
                int ch = (int)(str[i]);
                if (ch == '"')
                {
                    if (!quote)
                    {
                        quote = true;
                        continue;
                    }
                    quote = false;
                    continue;
                }
                if (ch == delim)
                {
                    if (quote)
                    {
                        pstr[count] = (byte)ch;
                        count++;
                        continue;
                    }
                    else
                    {
                        pstr[count] = (byte)'\0';
                        count = 0;
                        data = G1.ConvertToString(pstr);
                        data = G1.trim(data);
                        if ( removeEmpty )
                        {
                            if (String.IsNullOrWhiteSpace(data))
                                continue;
                        }
                        of_answer[of_ans_count] = data;
                        of_ans_count++;
                        if (of_ans_count >= 499)
                            break;
                        continue;
                    }
                }
                if (count < 497)
                {
                    pstr[count] = (byte)ch;
                    count++;
                }
                else
                {
                    data = G1.ConvertToString(pstr);
                    //                    sw.WriteLine("Data IDX Out! " + of_count.ToString() + " = " + data);
                }
            }
        }
        /***********************************************************************************************/
        public static string trim(string str)
        {
            string text = "";
            int j = 0;
            for (int i = 0; i < str.Length; i++)
            {
                j = (int)(str[i]);
                if (j <= 0)
                    break;
                text += str.Substring(i, 1);
            }
            return text;
        }
        /****************************************************************************************/
        public static string get_db_blob(string table, string record, string field)
        {
            string str = "";
            string command = "select `" + field + "` from `" + table + "` where `record` = '" + record + "';";
            MySqlConnection mconn = G1.conn1;

            MySqlCommand cmd = new MySqlCommand(command, mconn);
            try
            {
                OpenConnection(mconn);
                Byte[] obj = cmd.ExecuteScalar().ObjToBytes();
                if ((obj != null))
                {
                    str = G1.ConvertToString(obj);
                    str = G1.DecompressString(str);
                }
            }
            catch (Exception ex)
            {
                if (mconn != null && mconn.State != ConnectionState.Closed)
                    UnLockTable(mconn);

                //				G1.LogError("A critical exception has occurred while attempting to aquire a record for table " + tablename + ":\n" + ex.Message, ex, true);

            }
            finally
            {
                if (mconn != null && mconn.State != ConnectionState.Closed)
                    mconn.Close();
            }

            return str;
        }
        /****************************************************************************************/
        public static string ConvertToString(byte[] stuff)
        {
            if (1 == 1)
            {
                string converted = Encoding.UTF8.GetString(stuff, 0, stuff.Length);
                return converted;
            }
            string str = "";
            for (int i = 0; i < stuff.Length; i++)
            {
                if (stuff[i] == 0)
                    break;
                str += (char)stuff[i];
            }
            return str;
        }
        /****************************************************************************************/
        public static string ConvertToStringx(byte[] stuff)
        {
            string str = "";
            int len, i;
            i = 0;
            len = 0;
            for (; ; )
            {
                if (stuff[i] == 0)
                    break;
                len = len + 1;
                i = i + 1;
            }
            if (len > 0)
            {
                i = 0;
                for (; ; )
                {
                    if (stuff[i] == 0)
                        break;
                    str += (char)stuff[i];
                    i++;
                }
            }
            return str;
        }
        /***********************************************************************************************/
        public static string DecompressString(string compressedText)
        {
            try
            {
                if (String.IsNullOrEmpty(compressedText))
                    return "";

                var gzBuffer = Convert.FromBase64String(compressedText);
                using (var ms = new MemoryStream())
                {
                    var msgLength = BitConverter.ToInt32(gzBuffer, 0);
                    ms.Write(gzBuffer, 4, gzBuffer.Length - 4);

                    var buffer = new byte[msgLength];

                    ms.Position = 0;
                    using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                        zip.Read(buffer, 0, buffer.Length);

                    return Encoding.UTF8.GetString(buffer).Trim('\0');
                }
            }
            catch
            {
                try
                {
                    if (compressedText == null)
                        return "";
                    var gzBuffer = Convert.FromBase64String(compressedText);
                    using (var ms = new MemoryStream())
                    {
                        var msgLength = BitConverter.ToInt32(gzBuffer, 0);
                        ms.Write(gzBuffer, 0, gzBuffer.Length);

                        var buffer = new byte[msgLength];

                        ms.Position = 0;
                        using (var zip = new GZipStream(ms, CompressionMode.Decompress))
                            zip.Read(buffer, 0, buffer.Length);

                        return Encoding.UTF8.GetString(buffer).Trim('\0');
                    }
                }
                catch
                {
                    return compressedText;
                }
            }
        }
        /*******************************************************************************************/
        public static string commatize(long money)
        {
            int len = money.ToString().Length;
            double d = (double)(money) / 100D;
            string data = "";
            if (len >= 11)
                data = d.ToString("###,###,###,###.00");
            else if (len >= 8)
                data = d.ToString("###,###,###.00");
            else if (len >= 5)
                data = d.ToString("###,###.00");
            else
                data = d.ToString("###.00");
            return data;
        }
        /***********************************************************************************************/
        public static DataTable LoadCCFeeTable()
        {
            DateTime date = DateTime.Now;
            DataTable dt = G1.get_db_data("Select * from `creditcard_fees` ORDER BY `beginDate` DESC;");
            dt.Columns.Add("bDate");
            dt.Columns.Add("eDate");

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["endDate"].ObjToDateTime();
                if (date.Year <= 1000)
                    dt.Rows[i]["endDate"] = G1.DTtoMySQLDT(DateTime.Now);
            }

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                date = dt.Rows[i]["beginDate"].ObjToDateTime();
                dt.Rows[i]["bDate"] = date.ToString("yyyyMMdd");

                date = dt.Rows[i]["endDate"].ObjToDateTime();
                dt.Rows[i]["eDate"] = date.ToString("yyyyMMdd");
            }

            DataView tempview = dt.DefaultView;
            tempview.Sort = "endDate DESC";
            dt = tempview.ToTable();
            return dt;
        }
        /***********************************************************************************************/
        public static double GetCCFee(DataTable feeDt, string workContract, string payer)
        {
            if (feeDt == null)
                return 0D;
            if (feeDt.Rows.Count <= 0)
                return 0D;

            string allowFee = "";

            string lookup = workContract;
            if (!String.IsNullOrWhiteSpace(payer))
                lookup = payer;
            string cmd = "Select * from `creditcards` WHERE `contractNumber` = '" + lookup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                allowFee = dt.Rows[0]["allowFee"].ObjToString().ToUpper();
                if (String.IsNullOrWhiteSpace(allowFee))
                    return 0D;
                if (allowFee == "N")
                    return 0D;
            }

            double ccFee = 0D;
            try
            {
                DataRow[] dRows = feeDt.Select("eDate>='" + DateTime.Now.ToString("yyyyMMdd") + "'");
                if (dRows.Length > 0)
                {
                    //DataTable dd = dRows.CopyToDataTable();
                    ccFee = dRows[0]["fee"].ObjToDouble();
                    ccFee = ccFee / 100D;
                    ccFee = G1.RoundValue(ccFee);
                }
            }
            catch (Exception ex)
            {
            }
            return ccFee;
        }
        /***********************************************************************************************/
        public static double LookupCCFee ( DateTime date )
        {
            double ccFee = 0D;
            try
            {
                DataTable feeDt = G1.LoadCCFeeTable();
                if (feeDt == null)
                    return ccFee;
                if (feeDt.Rows.Count <= 0)
                    return ccFee;

                DataRow[] dRows = feeDt.Select("eDate>='" + DateTime.Now.ToString("yyyyMMdd") + "'");
                if (dRows.Length > 0)
                {
                    //DataTable dd = dRows.CopyToDataTable();
                    ccFee = dRows[0]["fee"].ObjToDouble();
                    ccFee = ccFee / 100D;
                    ccFee = G1.RoundValue(ccFee);
                }
            }
            catch (Exception ex)
            {
            }
            return ccFee;
        }
        /***********************************************************************************************/
        public static void PrintPreview(DevExpress.XtraPrinting.PrintableComponentLink printableComponentLink1, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            G1.AdjustColumnWidths(gridMain, 0.65D, true);

            printableComponentLink1.CreateDocument();
            printableComponentLink1.ShowPreview();
            //printableComponentLink1.ShowPreviewDialog();

            G1.AdjustColumnWidths(gridMain, 0.65D, false);
        }
        /***********************************************************************************************/
        public static void AdjustColumnWidths(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, double factor, bool reduce )
        {
            //int width = 0;
            //int newWidth = 0;
            //for (int i = 0; i < gridMain.Columns.Count; i++)
            //{
            //    width = gridMain.Columns[i].Width;
            //    if ( reduce )
            //        newWidth = (int)((double)width * factor);
            //    else
            //        newWidth = (int)((double)width / factor);
            //    if (newWidth > 0)
            //        gridMain.Columns[i].Width = newWidth;
            //}
        }
        /***********************************************************************************************/
        public static void SetColumnWidth(DevExpress.XtraGrid.Views.BandedGrid.BandedGridView grid, string name, int width)
        {
            if (!CheckGridColumnExists(grid, name))
                return;
            try
            {
                grid.Columns[name].MinWidth = 5;
                grid.Columns[name].MaxWidth = width * 2;
                grid.Columns[name].Width = width;
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        public static void AddNewColumn(DevExpress.XtraGrid.Views.BandedGrid.BandedGridView grid, string name, string caption, string format, FormatType type, int width, bool visible = true)
        {
            if (CheckGridColumnExists(grid, name))
                return;
            BandedGridColumn column = new BandedGridColumn();
            column.Caption = caption;
            //            column.DisplayFormat.FormatType = FormatType.Numeric;
            column.DisplayFormat.FormatType = type;
            column.FieldName = name;
            column.Name = name;
            column.Visible = visible; // true;
            column.MinWidth = width;
            column.Width = width; // 125;
            column.OptionsColumn.FixedWidth = true;
            int index = grid.Bands.Count - 1;
            if (index < 0)
                index = 0;
            grid.Bands[index].Columns.Add(column);
            //            TaxBand.Columns.Add(column);
            grid.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] { column });
            grid.Columns[name].DisplayFormat.FormatString = format;
            grid.Columns[name].DisplayFormat.FormatType = type;
            grid.Columns[name].OptionsColumn.FixedWidth = false;
            grid.Columns[name].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Right;
        }
        /***********************************************************************************************/
        public static void InsertNewColumn(DevExpress.XtraGrid.Views.BandedGrid.BandedGridView grid, string beforeName, string name, string caption, string format, FormatType type, int width, bool visible = true)
        {
            if (CheckGridColumnExists(grid, name))
                return;
            BandedGridColumn column = new BandedGridColumn();
            column.Caption = caption;
            column.DisplayFormat.FormatType = type;
            column.FieldName = name;
            column.Name = name;
            column.Visible = visible; // true;
            column.MinWidth = width;
            column.Width = width; // 125;
            column.OptionsColumn.FixedWidth = true;
            try
            {
                int index = G1.get_column_number(grid, beforeName);
                if (index < 0)
                    return;
                //int index = grid.Bands.Count - 1;
                //if (index < 0)
                //    index = 0;
                //grid.Bands[index].Columns.Add(column);
                //grid.Columns.AddRange(new DevExpress.XtraGrid.Views.BandedGrid.BandedGridColumn[] { column });

                grid.Columns.Insert(index, column);
                grid.Columns[name].DisplayFormat.FormatString = format;
                grid.Columns[name].DisplayFormat.FormatType = type;
                grid.Columns[name].OptionsColumn.FixedWidth = false;
                grid.Columns[name].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Right;

                index = grid.Columns[beforeName].ColIndex;
                //grid.Columns[name].ColIndex = index;
                grid.Columns[name].ColVIndex = index;
                grid.Columns[name].AbsoluteIndex = index;
            }
            catch ( Exception ex )
            {
            }
        }
        /***********************************************************************************************/
        public static bool CheckGridColumnExists(string name, DevExpress.XtraGrid.Views.BandedGrid.BandedGridView gridView)
        {
            for (int i = 0; i < gridView.Columns.Count; i++)
            {
                BandedGridColumn column = gridView.Columns[i];
                if (column.FieldName.ToUpper() == name.ToUpper())
                    return true;
            }
            return false;
        }
        /***********************************************************************************************/
        public static bool CheckGridColumnExists(DevExpress.XtraGrid.Views.BandedGrid.BandedGridView grid, string name)
        {
            for (int i = 0; i < grid.Columns.Count; i++)
            {
                BandedGridColumn column = grid.Columns[i];
                if (column.FieldName.ToUpper() == name.ToUpper())
                    return true;
            }
            return false;
        }
        /***********************************************************************************************/
        public static void SetColumnPosition(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridView, string name, int position, int width = 0 )
        {
            try
            {
                if (String.IsNullOrWhiteSpace(name))
                    return;
                BandedGridColumn column = gridView.Columns[name];
                if (column == null)
                {
                    if (name == "Num")
                    {
                        column = gridView.Columns["num"];
                        name = "num";
                    }
                    if ( column == null )
                        return;
                }
                column.Visible = true;
                //column.AbsoluteIndex = position;
                gridView.SetColumnPosition(column, 0, position);
                gridView.Columns[name].AbsoluteIndex = position;
                gridView.Columns[name].VisibleIndex = position;
                if (width > 0)
                {
                    gridView.SetColumnWidth(column, width);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Setting Column Position for Column " + name + " Position " + position.ToString() + "!");
            }
        }
        /***********************************************************************************************/
        public static void SetColumnPosition(DevExpress.XtraGrid.Views.BandedGrid.BandedGridView gridView, string name, int position, int width = 0)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(name))
                    return;
                BandedGridColumn column = gridView.Columns[name];
                if (column == null)
                {
                    if (name == "Num")
                    {
                        column = gridView.Columns["num"];
                        name = "num";
                    }
                    if (column == null)
                        return;
                }
                column.Visible = true;
                //column.AbsoluteIndex = position;

                //gridView.SetColumnPosition(column, 0, position);
                gridView.Columns[name].AbsoluteIndex = position;
                gridView.Columns[name].VisibleIndex = position;
                if (width > 0)
                {
                    gridView.Columns[name].Width = width;
                    //gridView.SetColumnWidth(column, width);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Setting Column Position for Column " + name + " Position " + position.ToString() + "!");
            }
        }
        /***********************************************************************************************/
        public static void SetColumnPosition(DevExpress.XtraGrid.Views.Grid.GridView gridView, string name, int position)
        {
            try
            {
                gridView.Columns[name].AbsoluteIndex = position;
                gridView.Columns[name].Visible = true;
                gridView.Columns[name].Fixed = DevExpress.XtraGrid.Columns.FixedStyle.Left;
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Setting Column Position for Column " + name + " Position " + position.ToString() + "!");
            }
        }
        /***********************************************************************************************/
        public static void SetColumnPosition(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.BandedGridView gridView)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string name = dt.Columns[i].Caption.Trim();
                try
                {
                    BandedGridColumn column = gridView.Columns[name];
                    // gridView.SetColumnPosition(column, 0, i);
                }
                catch
                {
                    var str = dt.Rows[0][i];
                    Type type = str.GetType();
                    if (str.GetType() == Type.GetType("System.String"))
                    {
                        AddNewColumn(gridView, name, name, "", FormatType.None, 75, true);
                    }
                    else if (str.GetType() == Type.GetType("System.Double"))
                    {
                        AddNewColumn(gridView, name, name, "N2", FormatType.Numeric, 75, true);
                    }
                    else
                    {
                        AddNewColumn(gridView, name, name, "N2", FormatType.Numeric, 75, true);
                    }
                    BandedGridColumn column = gridView.Columns[name];
                    //                    gridView.SetColumnPosition(column, 0, i);
                }
            }
        }
        /***********************************************************************************************/
        public static void SetColumnPositions(DataTable dt, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridView)
        {
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                string name = dt.Columns[i].Caption.Trim();
                try
                {
                    G1.SetColumnPosition(gridView, name, i+1);
                }
                catch
                {
                    var str = dt.Rows[0][i];
                    Type type = str.GetType();
                    if (str.GetType() == Type.GetType("System.String"))
                    {
                        AddNewColumn(gridView, name, name, "", FormatType.None, 75, true);
                    }
                    else if (str.GetType() == Type.GetType("System.Double"))
                    {
                        AddNewColumn(gridView, name, name, "N2", FormatType.Numeric, 75, true);
                    }
                    else
                    {
                        AddNewColumn(gridView, name, name, "N2", FormatType.Numeric, 75, true);
                    }
                    BandedGridColumn column = gridView.Columns[name];
                    gridView.Columns[name].AbsoluteIndex = i;
                    //                    gridView.SetColumnPosition(column, 0, i);
                }
            }
        }
        /***********************************************************************************************/
        public static void HideGridChooser(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain)
        {
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                if (!gridMain.Columns[i].Visible)
                    gridMain.Columns[i].OptionsColumn.ShowInCustomizationForm = false;
            }
        }
        /***********************************************************************************************/
        public static void FindGridViewRow ( DevExpress.XtraGrid.GridControl dgv, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gridMain, string columnName, string findName )
        {
            try
            {
                string cnum = "";
                DataTable dt = (DataTable)dgv.DataSource;
                if (dt == null)
                    return;
                if (G1.get_column_number(dt, columnName) < 0)
                    return;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    cnum = dt.Rows[i][columnName].ObjToString();
                    if (cnum == findName )
                    {
                        gridMain.FocusedRowHandle = i;
                        gridMain.SelectRow(i);
                        gridMain.RefreshEditor(true);
                        dgv.RefreshDataSource();
                        gridMain.RefreshData();
                        dgv.Refresh();
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Finding Row for Column " + findName + "!");
            }
        }
        /***************************************************************************************/
        public static string force_lower_line(string str)
        {
            string newstr = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
            return newstr;
        }
        /***************************************************************************************/
        public static string force_lower_line_special(string str)
        {
            string newstr = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
            string[] Lines = newstr.Split(' ');
            bool gotThe = false;
            if ( Lines.Length > 0 )
            {
                for (int i = 0; i < Lines.Length; i++)
                {
                    if (!String.IsNullOrWhiteSpace(Lines[i]))
                    {
                        if (Lines[i].ToUpper().Trim() == "THE")
                            gotThe = true;
                        break;
                    }
                }
            }
            newstr = newstr.Replace(" Of ", " of ");
            newstr = newstr.Replace(" And ", " and ");
            if ( !gotThe )
                newstr = newstr.Replace(" The ", " the ");
            newstr = newstr.Replace(" Dvd ", " DVD ");
            return newstr;
        }
        /***********************************************************************************************/
        public static string GetNextLine(StreamReader sr)
        {
            string line = null;
            for (; ; )
            {
                line = sr.ReadLine();
                if (line == null)
                    break;
                if (line.Trim().Length == 0)
                    continue;
                break; // Must have something
            }
            return line;
        }
        /***********************************************************************************************/
        public static void LogError(string msg, Exception ex, bool Display)
        {
            if (Display)
                MessageBox.Show(msg, "Error Occurred");
            return;
        }
        /***********************************************************************************************/
        public static string DBtoString(object val)
        {
            return (val != null && val != DBNull.Value) ? val.ToString() : String.Empty;
        }
        /***********************************************************************************************/
        public static string CalcAge(string hiredate)
        {
            DateTime date = G1.ParseDateTime(hiredate);
            if ( date.Year > 1900 )
                return G1.GetAge(date, DateTime.Today).ToString();
            return "";
        }
        /***********************************************************************************************/
        public static int GetAge(DateTime bDate, DateTime now)
        {
            //            return DateTime.Now.CompareTo(bDate.AddYears(now.Year - bDate.Year)) < 0 ? (now.Year - bDate.Year) - 1 : (now.Year - bDate.Year);
            return CalculateAgeCorrect(bDate, now);
        }
        /***********************************************************************************************/
        public static int CalculateAgeCorrect(DateTime birthDate, DateTime now)
        {
            int age = now.Year - birthDate.Year;

            if (now.Month < birthDate.Month || (now.Month == birthDate.Month && now.Day < birthDate.Day))
                age--;

            return age;
        }
        /****************************************************************************/
        public static string AddToAudit(string user, string module, string field, string what, string contract = "")
        {
            string record = G1.create_record("audit", "module", "-1");
            bool error = false;
            if (String.IsNullOrWhiteSpace(record))
                error = true;
            if (record == "-1")
                error = true;
            if (error)
            {
                MessageBox.Show("***ERROR*** Creating Audit Record for Module " + module + " Field " + field + " Message " + what + " Record " + record);
                return record;
            }
            string local_user = "";
            string machine = "";
            try
            {
                local_user = Environment.GetEnvironmentVariable("USERNAME").ToUpper();
                machine = System.Environment.MachineName.ObjToString().Trim();
            }
            catch (Exception ex)
            {
            }
            try
            {
                G1.update_db_table("audit", "record", record, new string[] { "module", module, "field", field, "what", what, "user", user, "computerUserName", local_user, "machineName", machine, "contract", contract });
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Audit File! " + ex.Message.ToString());
            }
            return record;
        }
        /***********************************************************************************************/
        public static bool ValidateOverridePassword( string question = "" )
        {
            if (String.IsNullOrWhiteSpace(question))
                question = "Enter Override Password > ";
            using (Ask fmrmyform = new Ask( question ))
            {
                fmrmyform.Text = "";
                fmrmyform.ShowDialog();
                string answer = fmrmyform.Answer.Trim().ToUpper();
                if (String.IsNullOrWhiteSpace(answer))
                    return false; // Loser!
                if (!CheckPassword ( answer) )
                {
                    MessageBox.Show("***ERROR*** Invalid Password!!!", "Override Problem Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                    return false;
                }
            }
            return true;
        }
        /**************************************************************************************/
        public static bool CheckPassword(string password)
        {
            DateTime now = DateTime.Now;
            string day = now.Day.ToString("D2").Reverse();
            string month = now.Month.ToString("D2").Reverse();
            string year = now.Year.ToString("D4").Reverse();
            string str = day + month + year;
            if (str == password)
                return true;
            return false;
        }
        /***********************************************************************************************/
        public static string cUser;
        public static void SaveLocalPreferences(DevExpress.XtraEditors.XtraForm me, GridView GridView, string user, string LayoutName)
        {
            try
            {
                //if (GridView.Bands.VisibleBandCount == 0)//Armor to protect against writing out a corrupt grid layout
                //    return;
                cUser = user;
                if (LayoutName.Trim().Length == 0)
                    LayoutName = "Default";
                string completeLayout = "LAYOUT" + "_" + LayoutName.Trim();
                string completeSize = "SIZE" + "_" + LayoutName.Trim();
                string completeSkin = "SKIN" + "_" + LayoutName.Trim();

                string directory = "C:/TEMP";
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string completePath = directory + "\\" + user + "_" + completeLayout.Trim() + ".xml";
                if (File.Exists(completePath))
                    File.Delete(completePath);
                GridView.SaveLayoutToXml(completePath);
                string xmlAsString = "";
                using (StreamReader streamReader = new StreamReader(completePath))
                {
                    xmlAsString = streamReader.ReadToEnd();
                    streamReader.Close();
                }

                G1.RemovePreference(user, completeLayout);
                G1.RemovePreference(user, completeSize);
                G1.RemovePreference(user, completeSkin);
                //RemovePreference(completeFilter);
                //RemovePreference(completeDocs);

                SavePreference(completeLayout, xmlAsString);
                //User.getInstance.UserPreferences.RemovePreference(completeLayout.Trim());

                string detail = G1.GetSizeAndPosition(me);
                SavePreference(completeSize, detail);
            }
            catch (Exception ex)
            {
                G1.LogError("Error occurred while attempting to save local preferences.", ex, false);
            }
        }
        public static void SaveLocalPreferences(DevExpress.XtraEditors.XtraForm me, BandedGridView GridView, string user, string LayoutName)
        {
            try
            {
                if (GridView.Bands.VisibleBandCount == 0)//Armor to protect against writing out a corrupt grid layout
                    return;
                cUser = user;
                if (LayoutName.Trim().Length == 0)
                    LayoutName = "Default";
                string completeLayout = "LAYOUT" + "_" + LayoutName.Trim();
                string completeSize = "SIZE" + "_" + LayoutName.Trim();
                string completeSkin = "SKIN" + "_" + LayoutName.Trim();

                string directory = "C:/TEMP";
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string completePath = directory + "\\" + user + "_" + completeLayout.Trim() + ".xml";
                if (File.Exists(completePath))
                    File.Delete(completePath);
                GridView.SaveLayoutToXml(completePath);
                string xmlAsString = "";
                using (StreamReader streamReader = new StreamReader(completePath))
                {
                    xmlAsString = streamReader.ReadToEnd();
                    streamReader.Close();
                }

                G1.RemovePreference(user, completeLayout);
                G1.RemovePreference(user, completeSize);
                G1.RemovePreference(user, completeSkin);
                //RemovePreference(completeFilter);
                //RemovePreference(completeDocs);

                SavePreference(completeLayout, xmlAsString);
                //User.getInstance.UserPreferences.RemovePreference(completeLayout.Trim());

                string detail = G1.GetSizeAndPosition(me);
                SavePreference(completeSize, detail);
                //User.getInstance.UserPreferences.RemovePreference(completeSize.Trim());

                //if (!String.IsNullOrEmpty(statusFilter.EditValue.ToString()))
                //{
                //    string filter = statusFilter.EditValue.ToString();
                //    SavePreference(completeFilter, filter);
                //    User.getInstance.UserPreferences.RemovePreference(completeFilter.Trim());
                //}

                //if (!String.IsNullOrEmpty(doctorSelection.EditValue.ToString()))
                //{
                //    if (CurrentLayoutName.Trim().ToUpper() != "DEFAULT" && CurrentLayoutName.Trim().ToUpper() != "MASTER")
                //    { // Don't save doctors for default or master layouts
                //        string docs = doctorSelection.EditValue.ToString();
                //        SavePreference(completeDocs, docs);
                //        User.getInstance.UserPreferences.RemovePreference(completeDocs.Trim());
                //    }
                //}

                //string skinName = this.LookAndFeel.SkinName.Trim();
                //skinName += "~";
                //string color = this.twoLineGridView.Appearance.EvenRow.BackColor.Name.ToString();
                //if (this.twoLineGridView.Appearance.EvenRow.Options.UseBackColor == false)
                //    color = "";
                //if (color.Trim().Length > 0)
                //    skinName += color;
                //if (twoLineGridView.OptionsView.ColumnAutoWidth)
                //    skinName += "~AUTOSIZE";

                //string skinName = me.LookAndFeel.SkinName.Trim();
                //if ( skinName != "DevExpress Style")
                //    SavePreference(completeSkin, skinName);

                //User.getInstance.UserPreferences.RemovePreference(completeSkin.Trim());
            }
            catch (Exception ex)
            {
                G1.LogError("Error occurred while attempting to save local preferences.", ex, false);
            }
        }
        /***********************************************************************************************/
        public static void SavePreference(string preferenceName, string data)
        { // Leave this code here. Why did it have to change?
            bool doBlob = false;
            if (data.IndexOf("MRUFilt") > 0)
            {
                string record = G1.create_record("screen_preferences", "user", LoginForm.username);
                if (G1.BadRecord("screen_preferences", record))
                    return;

                G1.update_db_table("screen_preferences", "record", record, new string[] { "computer_resolution", ResolutionID.ToString(), "preference_name", preferenceName, "preference_value", "USEBLOB", });

                byte[] b = Encoding.UTF8.GetBytes(data);
                G1.update_blob("screen_preferences", "record", record, "preference_blob", b);
            }
            else
            {
                try
                {
                    string cmd = "insert into `screen_preferences`  (`user`, `computer_resolution`, `preference_name`, `preference_value`) ";
                    cmd += "VALUES ('" + cUser + "','" + ResolutionID + "','" + preferenceName + "','" + data + "');";
                    G1.update_db_data(cmd);
                }
                catch (Exception ex)
                {
                    G1.LogError("***ERROR*** On saving Layout Preference. Call I/T!", ex, true);
                }
            }
        }
        private static long _resolutionID;
        public static long ResolutionID
        {
            get
            {
                //if (_resolutionID == 0)
                //    _resolutionID = G1.GetResolutionID(VirtualResolutionHash);
                if (_resolutionID == 0)
                    _resolutionID = VirtualResolutionHash;
                return _resolutionID;
            }
        }
        public static Int64 VirtualResolutionHash
        {
            get
            {
                var vrHash = SystemInformation.VirtualScreen.Width.ObjToString();
                vrHash += SystemInformation.VirtualScreen.Height.ObjToString();
                return vrHash.ObjToInt64();
            }
        }
        /***************************************************************************************/
        public static bool RestoreGridLayout(DevExpress.XtraEditors.XtraForm me, DevExpress.XtraGrid.GridControl dgv, GridView GridView, string user, string LayoutName, ref string skinName)
        {
            bool foundLayout = false;
            string holding = "";
            skinName = "";
            GridView.GridControl.ForceInitialize();
            try
            {
                string searchName = "";
                if (LayoutName.Trim().Length > 0)
                {
                    string completeLayout = "LAYOUT" + "_" + LayoutName.Trim();
                    string completeSize = "SIZE" + "_" + LayoutName.Trim();
                    string completeSkin = "SKIN" + "_" + LayoutName.Trim();

                    string directory = "C:/TEMP";
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    string completePath = directory + "\\" + user + "_" + completeLayout.Trim() + ".xml";
                    searchName = completeLayout;
                    string cmd = "select * from screen_preferences where `user` = '" + user + "' and `computer_resolution` = '" + ResolutionID + "' and `preference_name` like '%" + searchName.Trim() + "%';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        string value = dt.Rows[0]["preference_value"].ToString();
                        G1.WriteGridLayoutToFile(completePath, value);
                        GridView.RestoreLayoutFromXml(completePath.Trim());
                        // Don't change this. I don't want it saved this way
                    }
                    string detail = G1.ReadPreference(user, completeSize.Trim());
                    if (detail.Trim().Length == 0 && holding.Trim().Length > 0)
                        detail = holding;
                    if (!String.IsNullOrWhiteSpace(detail))
                    {
                        G1.SetSizeAndPosition(me, detail);
                        foundLayout = true;
                    }
                    skinName = G1.ReadPreference(user, completeSkin.Trim());
                }
            }
            catch (Exception ex) // Do 1 Line View even if does not exist for user
            {
                foundLayout = false;
            }
            dgv.MainView = GridView; // This is now the layout
            return foundLayout;
        }
        /***************************************************************************************/
        public static bool RestoreGridLayoutExact(DevExpress.XtraEditors.XtraForm me, DevExpress.XtraGrid.GridControl dgv, BandedGridView GridView, string user, string LayoutName, ref string skinName)
        {
            bool foundLayout = false;
            string holding = "";
            skinName = "";
            GridView.GridControl.ForceInitialize();
            try
            {
                string searchName = "";
                if (LayoutName.Trim().Length > 0)
                {
                    string completeLayout = "LAYOUT" + "_" + LayoutName.Trim();
                    string completeSize = "SIZE" + "_" + LayoutName.Trim();
                    string completeSkin = "SKIN" + "_" + LayoutName.Trim();

                    string directory = "C:/TEMP";
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    string completePath = directory + "\\" + user + "_" + completeLayout.Trim() + ".xml";
                    searchName = completeLayout;
                    string cmd = "select * from screen_preferences where `user` = '" + user + "' and `computer_resolution` = '" + ResolutionID + "' and `preference_name` = '" + searchName.Trim() + "';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                    {
                        cmd = "select * from screen_preferences where `user` = 'cjenkins' and `preference_name` like '%" + searchName.Trim() + "%';";
                        dt = G1.get_db_data(cmd);
                    }
                    if (dt.Rows.Count > 0)
                    {
                        string value = dt.Rows[0]["preference_value"].ToString();
                        if (String.IsNullOrWhiteSpace(value))
                        {
                            string record = dt.Rows[0]["record"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(record))
                            {
                                value = G1.get_db_blob("screen_preferences", record, "preference_blob");
                                if (!String.IsNullOrWhiteSpace(value))
                                    G1.WriteGridLayoutToFile(completePath, value);
                            }
                        }
                        else
                            G1.WriteGridLayoutToFile(completePath, value);
                        GridView.RestoreLayoutFromXml(completePath.Trim());
                        // Don't change this. I don't want it saved this way
                    }
                    string detail = G1.ReadPreference(user, completeSize.Trim());
                    if (detail.Trim().Length == 0 && holding.Trim().Length > 0)
                        detail = holding;
                    if (!String.IsNullOrWhiteSpace(detail))
                    {
                        G1.SetSizeAndPosition(me, detail);
                        foundLayout = true;
                    }
                    skinName = G1.ReadPreference(user, completeSkin.Trim());
                }
            }
            catch (Exception ex) // Do 1 Line View even if does not exist for user
            {
                foundLayout = false;
                MessageBox.Show("***ERROR*** Restoring Layout! " + ex.Message.ToString(), "Screen layout Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            if (dgv != null)
                dgv.MainView = GridView; // This is now the layout
            return foundLayout;
        }
        /***************************************************************************************/
        public static bool RestoreGridLayout(DevExpress.XtraEditors.XtraForm me, DevExpress.XtraGrid.GridControl dgv, BandedGridView GridView, string user, string LayoutName, ref string skinName )
        {
            bool foundLayout = false;
            string holding = "";
            skinName = "";
            GridView.GridControl.ForceInitialize();
            try
            {
                string searchName = "";
                if (LayoutName.Trim().Length > 0)
                {
                    string completeLayout = "LAYOUT" + "_" + LayoutName.Trim();
                    string completeSize = "SIZE" + "_" + LayoutName.Trim();
                    string completeSkin = "SKIN" + "_" + LayoutName.Trim();

                    string directory = "C:/TEMP";
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);
                    string completePath = directory + "\\" + user + "_" + completeLayout.Trim() + ".xml";
                    searchName = completeLayout;
                    string cmd = "select * from screen_preferences where `user` = '" + user + "' and `computer_resolution` = '" + ResolutionID + "' and `preference_name` like '%" + searchName.Trim() + "%';";
                    DataTable dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count <= 0)
                    {
                        cmd = "select * from screen_preferences where `user` = '" + user + "' and `preference_name` like '%" + searchName.Trim() + "%';";
                        dt = G1.get_db_data(cmd);
                        if (dt.Rows.Count <= 0)
                        {
                            cmd = "select * from screen_preferences where `user` = 'cjenkins' and `preference_name` like '%" + searchName.Trim() + "%';";
                            dt = G1.get_db_data(cmd);
                        }
                    }
                    if (dt.Rows.Count > 0)
                    {
                        string value = dt.Rows[0]["preference_value"].ToString();
                        if ( String.IsNullOrWhiteSpace ( value ))
                        {
                            string record = dt.Rows[0]["record"].ObjToString();
                            if ( !String.IsNullOrWhiteSpace ( record ))
                            {
                                value = G1.get_db_blob("screen_preferences", record, "preference_blob");
                                if ( !String.IsNullOrWhiteSpace ( value ))
                                    G1.WriteGridLayoutToFile(completePath, value);
                            }
                        }
                        else
                            G1.WriteGridLayoutToFile(completePath, value);
                        GridView.RestoreLayoutFromXml(completePath.Trim());
                        // Don't change this. I don't want it saved this way
                    }
                    string detail = G1.ReadPreference(user, completeSize.Trim());
                    if (detail.Trim().Length == 0 && holding.Trim().Length > 0)
                        detail = holding;
                    if (!String.IsNullOrWhiteSpace(detail))
                    {
                        G1.SetSizeAndPosition(me, detail);
                        foundLayout = true;
                    }
                    skinName = G1.ReadPreference(user, completeSkin.Trim());
                }
            }
            catch (Exception ex) // Do 1 Line View even if does not exist for user
            {
                foundLayout = false;
                MessageBox.Show("***ERROR*** Restoring Layout! " + ex.Message.ToString(), "Screen layout Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }
            if ( dgv != null )
                dgv.MainView = GridView; // This is now the layout
            return foundLayout;
        }
        //public static bool isValidLayout(string content)
        //{
        //    var doc = XDocument.Parse(content);
        //    return
        //        doc.Descendants("property")
        //            .Where(element => element.Attribute("name").Value.Equals("Bands", StringComparison.OrdinalIgnoreCase))
        //            .All(element => !element.Attribute("value").Value.Equals("0", StringComparison.OrdinalIgnoreCase));
        //}
        /***************************************************************************************/
        public static void WriteGridLayoutToFile(string filename, string layout)
        {
            if (File.Exists(filename))
                File.Delete(filename);
            StreamWriter sw = new StreamWriter(filename, false);
            sw.WriteLine(layout);
            sw.Close();
        }
        /***********************************************************************************************/
        public static string GetSizeAndPosition(DevExpress.XtraEditors.XtraForm me)
        {
            string data = "LPX:" + me.Left.ToString() + ",LPY:" + me.Top.ToString() + ",W:" + me.Width.ToString() + ",H:" + me.Height.ToString() + ",";
            return data;
        }
        /***********************************************************************************************/
        public static void SetSizeAndPosition(DevExpress.XtraEditors.XtraForm me, string detail)
        {
            int left = -1;
            int top = -1;
            int width = 0;
            int height = 0;
            string Skin = "";
            if (detail.Trim().Length > 0)
            {
                string[] Lines = detail.Split(',');
                if (Lines.Length < 4)
                    return;
                string str = Lines[0].Trim().Replace("LPX:", "");
                if (str.ObjIsInt())
                    left = str.ObjToInt32();
                str = Lines[1].Trim().Replace("LPY:", "");
                if (str.ObjIsInt())
                    top = str.ObjToInt32();
                str = Lines[2].Trim().Replace("W:", "");
                if (str.ObjIsInt())
                    width = str.ObjToInt32();
                str = Lines[3].Trim().Replace("H:", "");
                if (str.ObjIsInt())
                    height = str.ObjToInt32();
                if (Lines.Length >= 5)
                {
                    str = Lines[4].Trim().Replace("S:", "");
                    if (str.Trim().Length > 0)
                        Skin = str;
                }
            }
            //if (work_position)
            //{
            //    //if (height == 0)
            //    //    height = this.Height;
            //    //if (width == 0)
            //    //    width = this.Width;
            //    left = work_left;
            //    top = work_top;
            //    height = work_bottom - work_top;
            //    width = work_right - work_left;
            //}

            if (G1.IsOnScreen(new Rectangle(left, top, width, height)))
                me.SetBounds(left, top, width, height);
            //if ( left >= 0 && top >= 0 )
            //	this.SetBounds(left, top, width, height);
            if (Skin.Trim().Length > 0)
                me.LookAndFeel.SetSkinStyle(Skin);
            //            string data = "LPX:" + this.Left.ToString() + ",LPY:" + this.Top.ToString() + ",W:" + this.Width.ToString() + ",H:" + this.Height.ToString() + ",";
        }
        public static bool IsOnScreen(Rectangle formRectangle)
        {
            var screens = Screen.AllScreens;
            return (from screen in screens let tl = new Point(formRectangle.X, formRectangle.Y) let tr = new Point(formRectangle.X + formRectangle.Width, formRectangle.Y) let bl = new Point(formRectangle.X, formRectangle.Y + formRectangle.Height) let br = new Point(formRectangle.X + formRectangle.Width, formRectangle.Y + formRectangle.Height) where screen.WorkingArea.Contains(tl) || screen.WorkingArea.Contains(tr) || screen.WorkingArea.Contains(bl) || screen.WorkingArea.Contains(br) select screen).Any();
        }
        /***********************************************************************************************/
        public static string ReadPreference(string user, string preferenceName)
        {
            string rtn = "";
            string cmd = "Select * from `screen_preferences` where `user` = '" + user + "' and `computer_resolution` = '" + ResolutionID + "' and `preference_name` = '" + preferenceName + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                rtn = dt.Rows[0]["preference_value"].ToString();
            return rtn;
        }
        /***********************************************************************************************/
        public static void RemovePreference(string user, string preferenceName)
        {
            //string cmd = "delete from `screen_preferences` where `user` = '" + user + "' and `computer_resolution` = '" + ResolutionID + "' and `preference_name` = '" + preferenceName + "';";
            string cmd = "delete from `screen_preferences` where `user` = '" + user + "' and `preference_name` = '" + preferenceName + "';";
            G1.get_db_data(cmd);
        }
        /***********************************************************************************************/
        public static void RemoveLocalPreferences(string user, string LayoutName)
        {
            string completeLayout = "LAYOUT" + "_" + LayoutName.Trim();
            string completeSize = "SIZE" + "_" + LayoutName.Trim();
            string completeSkin = "SKIN" + "_" + LayoutName.Trim();
            G1.RemovePreference(user, completeLayout);
            G1.RemovePreference(user, completeSize);
            G1.RemovePreference(user, completeSkin);

            string directory = "C:/TEMP";
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            string completePath = directory + "\\" + user + "_" + completeLayout.Trim() + ".xml";
            try
            {
                File.Delete(completePath);
            }
            catch ( Exception ex)
            {
            }
        }
        /***********************************************************************************************/
        public static ToolStripMenuItem PopulateSkinsMenu(ToolStripMenuItem skinsMenu, EventHandler onChangingSkin)
        {
            DevExpress.Skins.SkinContainerCollection skins = DevExpress.Skins.SkinManager.Default.Skins;

            ToolStripMenuItem activeSkin = null;
            var chitem = new ToolStripMenuItem { Text = "Windows Default", CheckOnClick = true };
            chitem.Click += onChangingSkin;
            if (DevExpress.LookAndFeel.UserLookAndFeel.Default.UseWindowsXPTheme)
            {
                chitem.Checked = true;
                activeSkin = chitem;
            }
            skinsMenu.DropDownItems.Add(chitem);

            foreach (DevExpress.Skins.SkinContainer s in skins)
            {
                var pi = s.GetType().GetProperty("Creator", BindingFlags.NonPublic | BindingFlags.Instance);
                var creator = pi.GetValue(s, null) as DevExpress.Skins.Info.SkinXmlCreator;
                pi = creator.GetType().GetProperty("SkinAssembly", BindingFlags.NonPublic | BindingFlags.Instance);
                var assembly = pi.GetValue(creator, null) as Assembly;
                var atr = AssemblyDescriptionAttribute.GetCustomAttribute(assembly, typeof(AssemblyDescriptionAttribute)) as AssemblyDescriptionAttribute;
                var category = (atr == null) ? "" : atr.Description;

                chitem = new ToolStripMenuItem { Text = s.SkinName, CheckOnClick = true };
                chitem.Click += onChangingSkin;
                if (s.SkinName == DevExpress.LookAndFeel.UserLookAndFeel.Default.SkinName & !DevExpress.LookAndFeel.UserLookAndFeel.Default.UseWindowsXPTheme)
                {
                    chitem.Checked = true;
                    activeSkin = chitem;
                }
                if (category == "Utils Library")
                {
                    skinsMenu.DropDownItems.Add(chitem);
                }
                else
                {
                    var subitem = skinsMenu.DropDownItems["skins:" + category] as ToolStripMenuItem;
                    if (subitem == null)
                    {
                        subitem = new ToolStripMenuItem(category) { Name = "skins:" + category };
                        skinsMenu.DropDownItems.Add(subitem);
                    }
                    subitem.DropDownItems.Add(chitem);
                }
            }
            return activeSkin;
        }
        /***********************************************************************************************/
        public static void SetupVisibleColumns(BandedGridView gridMain, ToolStripMenuItem columnsToolStripMenuItem, EventHandler nmenu_Click)
        {
            ToolStripMenuItem menu = columnsToolStripMenuItem;
            for (int i = 0; i < gridMain.Columns.Count; i++)
            {
                string name = gridMain.Columns[i].Name;
                string caption = gridMain.Columns[i].Caption;
                ToolStripMenuItem nmenu = new ToolStripMenuItem();
                nmenu.Name = name;
                nmenu.Text = caption;
                if (gridMain.Columns[i].VisibleIndex >= 0)
                    nmenu.Checked = true;
                nmenu.Click += new EventHandler(nmenu_Click);
                menu.DropDownItems.Add(nmenu);
            }
        }
        /***********************************************************************************************/
        public static int getGridColumnIndex(BandedGridView gridMain, string columnName)
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
                else
                {
                    name = gridMain.Columns[i].FieldName.Trim();
                    if ( name == columnName )
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }
        /// <summary>
        /// Generic method to load a Form into a XtraTabPage
        /// </summary>
        /// <param name="form">Form to load</param>
        /// <param name="tabPage">Tab Page to load form into.</param>
        /****************************************************************************/
        public static void LoadFormInTab(Form form, DevExpress.XtraTab.XtraTabPage tabPage)
        {
            if (form == null || tabPage == null)
                return;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;
            tabPage.Controls.Add(form);
        }
        /****************************************************************************/
        public static void LoadFormInTab(Form form, System.Windows.Forms.TabPage tabPage)
        {
            if (form == null || tabPage == null)
                return;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;
            tabPage.Controls.Add(form);
        }

        /// <summary>
        /// Generic method to clear all controls from a XtraTabPage
        ///     It also will close any attached forms.
        /// </summary>
        /// <param name="tabPage">Tab Page to have cleared</param>
        /****************************************************************************/
        public static void ClearTabPageControls(DevExpress.XtraTab.XtraTabPage tabPage)
        {
            while (tabPage.Controls.Count > 0)
            {
                var c = tabPage.Controls[0];
                var t = c.GetType();
                if (t.BaseType == typeof(Form) || t.BaseType == typeof(DevExpress.XtraEditors.XtraForm))
                    ((Form)tabPage.Controls[0]).Close();
                else
                    tabPage.Controls.RemoveAt(0);
            }
        }

        /// <summary>
        /// Generic method to load a Form into a Dock Panel
        /// </summary>
        /// <param name="form">Form to load</param>
        /// <param name="dockPanel"> </param>
        /****************************************************************************/
        public static void LoadFormInDockPanel(Form form, DevExpress.XtraBars.Docking.DockPanel dockPanel)
        {
            if (form == null || dockPanel == null)
                return;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;
            dockPanel.Controls.Add(form);
        }

        /// <summary>
        /// Generic method to load a Form into a Control
        /// </summary>
        /// <param name="form">Form to load</param>
        /// <param name="control">Control to load form into.</param>
        /****************************************************************************/
        public static void LoadFormInControl(Form form, Control control)
        {
            if (form == null || control == null)
                return;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;
            control.Controls.Add(form);
        }

        /// <summary>
        /// Generic method to unload a Form and Controls from a Control
        /// </summary>
        /// <param name="control">Control to clear all forms and controls.</param>
        /****************************************************************************/
        public static void ClearControlControls(Control control)
        {
            while (control.Controls.Count > 0)
            {
                var c = control.Controls[0];
                var t = c.GetType();
                if (t.BaseType == typeof(Form) || t.BaseType == typeof(DevExpress.XtraEditors.XtraForm))
                    ((Form)control.Controls[0]).Close();
                else
                    control.Controls.RemoveAt(0);
            }
        }
        /****************************************************************************/
        public static void ClearControl(Control control, string name)
        {
            for (int i = 0; i < control.Controls.Count; i++)
            {
                var c = control.Controls[i];
                var t = c.GetType();
                if (!String.IsNullOrWhiteSpace(name))
                {
                    var fname = c.Name;
                    if (fname == name)
                    {
                        //if (t.BaseType == typeof(Form) || t.BaseType == typeof(DevExpress.XtraEditors.XtraForm))
                        //    ((Form)control.Controls[i]).Close();
                        //                        else
                        control.Controls.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        /***********************************************************************************************/
        public static void UpdatePreviousCustomer(string contractNumber, string user)
        {
            string record = "";
            string cmd = "Select * from `previouscustomers` where `user` = '" + user + "' order by `tmstamp` DESC;";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DataRow[] dRow = dt.Select("contractNumber='" + contractNumber + "'");
                if (dRow.Length > 0)
                {
                    record = dRow[0]["record"].ObjToString();
                    G1.delete_db_table("previouscustomers", "record", record);
                    //                    G1.update_db_table("previouscustomers", "record", record, new string[] { "contractNumber", contractNumber, "user", user });
                    //                    return;
                }
            }
            if (dt.Rows.Count > 10)
            {
                for (int i = 9; i < dt.Rows.Count; i++)
                {
                    record = dt.Rows[i]["record"].ObjToString();
                    G1.delete_db_table("previouscustomers", "record", record);
                }
            }
            record = G1.create_record("previouscustomers", "user", "-1");
            if (G1.BadRecord("previouscustomers", record))
                return;
            G1.update_db_table("previouscustomers", "record", record, new string[] { "contractNumber", contractNumber, "user", user });
        }
        /***********************************************************************************************/
        public static bool BadRecord(string table, string record)
        {
            if (String.IsNullOrWhiteSpace(record))
            {
                MessageBox.Show("***ERROR*** Record is null or blank adding to table " + table + "!");
                return true;
            }
            if (record == "-1")
            {
                MessageBox.Show("***ERROR*** Bad Create Record for table " + table + "!");
                return true;
            }
            return false;
        }
        /***********************************************************************************************/
        public static long TimeToUnix(DateTime date)
        {
            long ldate = DateTimeToUnixTimestamp(date);
            return ldate;
        }
        /***********************************************************************************************/
        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            double dValue = (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
            return (long)dValue;
        }
        /***********************************************************************************************/
        public static bool validate_date(object str)
        {
            try
            {
                DateTime dd = str.ObjToDateTime();
                if (dd > DateTime.MinValue)
                    return true;
            }
            catch { }

            //status = classify_date ( str );
            //if ( status == 1 || status == 3 )
            //    return true;
            return false;
        }
        /***********************************************************************************************/
        public static int ConvertMonthToIndex ( string monthIn )
        {
            Dictionary<string, string> months = new Dictionary<string, string>()
            {
                { "january", "01"},
                { "february", "02"},
                { "march", "03"},
                { "april", "04"},
                { "may", "05"},
                { "june", "06"},
                { "july", "07"},
                { "august", "08"},
                { "september", "09"},
                { "october", "10"},
                { "november", "11"},
                { "december", "12"},
            };
            int monthIndex = -1;
            if (String.IsNullOrWhiteSpace(monthIn))
                return monthIndex;
            foreach (var month in months)
            {
                if ( monthIn.ToLower().Contains(month.Key))
                {
                    monthIndex = month.Value.ObjToInt32();
                }
            }
            return monthIndex;
        }
        /***********************************************************************************************/
        public static DataTable GridtoTable(DataGrid dg)
        {
            DataTable retTable;

            if (dg == null)
                return new DataTable();

            if (dg.DataSource == null)
                return new DataTable();
            else
            {
                string name = dg.DataSource.GetType().Name.ToUpper();
                object obj = dg.DataSource;

                switch (name)
                {
                    case "DATAVIEW":
                        DataView dv = (DataView)obj;
                        retTable = dv.Table;
                        break;
                    case "DATATABLE":
                        retTable = (DataTable)obj;
                        break;
                    case "DATASET":
                        DataSet ds = (DataSet)obj;
                        if (ds.Tables.Count < 1)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("An error has occurred.  There appears to be no Tables in the " +
                                "dataset that is bound to the datagrid");
                            retTable = null;
                        }
                        else
                            retTable = ds.Tables[0];
                        break;
                    default:
                        DevExpress.XtraEditors.XtraMessageBox.Show("The following type of object was found as the datasource of the " +
                            "datagrid and was not handled: " + obj.ToString());
                        retTable = null;
                        break;
                }

                return retTable;
            }
        }
        /***********************************************************************************************/
        public static DataTable GridtoTable(DataGridView dg)
        {
            DataTable retTable;

            if (dg == null)
                return new DataTable();

            if (dg.DataSource == null)
                return new DataTable();
            else
            {
                string name = dg.DataSource.GetType().Name.ToUpper();
                object obj = dg.DataSource;

                switch (name)
                {
                    case "DATAVIEW":
                        DataView dv = (DataView)obj;
                        retTable = dv.Table;
                        break;
                    case "DATATABLE":
                        retTable = (DataTable)obj;
                        break;
                    case "DATASET":
                        DataSet ds = (DataSet)obj;
                        if (ds.Tables.Count < 1)
                        {
                            DevExpress.XtraEditors.XtraMessageBox.Show("An error has occurred.  There appears to be no Tables in the " +
                                "dataset that is bound to the datagrid");
                            retTable = null;
                        }
                        else
                            retTable = ds.Tables[0];
                        break;
                    default:
                        DevExpress.XtraEditors.XtraMessageBox.Show("The following type of object was found as the datasource of the " +
                            "datagrid and was not handled: " + obj.ToString());
                        retTable = null;
                        break;
                }

                return retTable;
            }
        }
        /***********************************************************************************************/
        public static void remove_endrow(object mo, DataGridView dg1)
        {
            int rows = dg1.Rows.Count;
            if (rows <= 0)
                return;
            CurrencyManager cm = (CurrencyManager)((Control)mo).BindingContext[dg1.DataSource, dg1.DataMember];
            ((DataView)cm.List).AllowNew = false;
        }
        /***********************************************************************************************/
        public static bool DoesGridViewColumnExist ( AdvBandedGridView gridMain, string name )
        {
            bool found = true;
            GridColumn column = gridMain.Columns[name];
            if (column == null)
                found = false;
            return found;
        }
        /****************************************************************************************/
        public static DataTable GetGridViewTable(AdvBandedGridView gridMain, DataTable dt)
        {
            GridView dv = (GridView)gridMain;
            DataRow ddr = null;
            DataTable dx = dt.Clone();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                ddr = dv.GetDataRow(i);
                dx.ImportRow(ddr);
            }
            return dx;
        }
        /***********************************************************************************************/
        public static int get_column_number(DataGrid dg, string name)
        {
            DataTable dt = GridtoTable(dg);
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                if (dt.Columns[i].ColumnName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }
        /***********************************************************************************************/
        public static string decode_month(int month)
        {
            try
            {
                return new DateTime(2000, month, 1).ToString("MMMM");
            }
            catch (Exception ex)
            {
                LogError(ex.Message, ex, false);
                return "";
            }
        }
        /********************************************************************************************/
        public static Image ImageFromResource(string imgname)
        {
            Assembly _assembly;
            _assembly = Assembly.GetExecutingAssembly();
            Stream xslStream = _assembly.GetManifestResourceStream(imgname);
            XmlReader _xmlReader = XmlReader.Create(xslStream);
            Bitmap bm = new Bitmap(xslStream);

            //Stream manStream =  Assembly.GetExecutingAssembly().GetManifestResourceStream(typeof(Icon), imgname);
            //Bitmap bm = new Bitmap(manStream);
            return (Image)bm;
        }
        /***********************************************************************************************/
        public static void loadGroupCombo(System.Windows.Forms.ComboBox cmb, string key, string module, bool addOriginal = false, string user = "" )
        {
            cmb.Items.Clear();
            if (addOriginal)
                cmb.Items.Add("Original");
            string cmd = "Select * from procfiles where ProcType = '" + key + "' AND `module` = '" + module + "' ";
            if (!String.IsNullOrWhiteSpace(user))
                cmd += " AND (`user` = '" + user + "' OR `user` = '' OR `user` = 'Common' ) ";
            else
                cmd += " AND (`user` = '' OR `user` = 'Common' ) ";
            cmd += " group by user,name;";
            DataTable dt = G1.get_db_data(cmd);
            bool removeOriginal = false;
            string activeUsers = "";
            string name = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                user = dt.Rows[i]["user"].ObjToString();
                activeUsers = dt.Rows[i]["activeUsers"].ObjToString();
                if ( !String.IsNullOrWhiteSpace ( activeUsers))
                {
                    if ( !LoginForm.administrator )
                    {
                        if (!activeUsers.ToUpper().Contains(LoginForm.username.ToUpper()))
                            continue;
                    }
                }
                name = dt.Rows[i]["Name"].ToString();
                if (name.Trim().ToUpper() == "ORIGINAL")
                    removeOriginal = true;
                if (user.ToUpper() == "COMMON")
                    name = "(C) " + name;
                cmb.Items.Add(name);
            }
            if (removeOriginal)
                cmb.Items.RemoveAt(0);
        }
        /****************************************************************************************/
        public static void AddSummaryColumn(string columnName, DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain = null, string format = "")
        {
            if (gMain == null)
                return;
            if (String.IsNullOrWhiteSpace(format))
                format = "${0:0,0.00}";
            gMain.Columns[columnName].SummaryItem.SummaryType = DevExpress.Data.SummaryItemType.Sum;
            gMain.Columns[columnName].SummaryItem.DisplayFormat = format;
        }
        /****************************************************************************************/
        public static void ClarifyDownPayments(DataTable dt, DateTime date1, DateTime date2)
        {
            DateTime issueDate = DateTime.Now;
            string contractNumber = "";
            double downPayment = 0D;
            double ccFee = 0D;
            string cmd = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                issueDate = dt.Rows[i]["issueDate8"].ObjToDateTime();
                if (issueDate.Year > 1850)
                {
                    if (issueDate >= date1 && issueDate <= date2)
                    {
                        contractNumber = dt.Rows[i]["contractNumber"].ObjToString();
                        cmd = "Select * from `payments` where `contractNumber` = '" + contractNumber + "' AND `downPayment` > '0';";
                        DataTable dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count > 0)
                        {
                            downPayment = dx.Rows[0]["downPayment"].ObjToDouble();
                            ccFee = dx.Rows[0]["ccFee"].ObjToDouble();

                            downPayment += ccFee;
                            //dt.Rows[i]["dpp"] = downPayment;
                            //dt.Rows[i]["downPayment"] = downPayment;
                        }
                    }
                }
            }
        }
        /****************************************************************************/
        public static string GetUserFullName ()
        {
            string name = LoginForm.username;
            string cmd = "Select * from `users` where `username` = '" + name + "';";
            DataTable dt = G1.get_db_data(cmd);
            if ( dt.Rows.Count > 0 )
            {
                name = dt.Rows[0]["firstName"].ObjToString() + " ";
                name += dt.Rows[0]["lastName"].ObjToString();
            }
            else
            {
                if ( LoginForm.isRobby )
                {
                    name = "Robby Graham";
                }
            }
            return name;
        }
        /****************************************************************************/
        public static void LoadFormInPanel(Form form, System.Windows.Forms.Panel panel)
        {
            if (form == null || panel == null)
                return;
            try
            {
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                form.Visible = true;
                panel.Controls.Add(form);
                //form.Visible = true;
            }
            catch ( Exception ex)
            {
            }
        }
        /****************************************************************************/
        public static void LoadFormInPanel(Form form, DevExpress.XtraEditors.PanelControl panel)
        {
            if (form == null || panel == null)
                return;

            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            form.Visible = true;
            panel.Controls.Add(form);
        }
        /****************************************************************************/
        public static void ClearPanelControls(System.Windows.Forms.Panel panel, bool onlyForms = false)
        {
            for (int i = 0; i < panel.Controls.Count; i++)
            {
                var c = panel.Controls[i];
                var t = c.GetType();
                if (t.BaseType == typeof(Form) || t.BaseType == typeof(DevExpress.XtraEditors.XtraForm))
                    ((Form)panel.Controls[0]).Close();
                else
                {
                    if (!onlyForms)
                        panel.Controls.RemoveAt(0);
                }
            }
        }
        /****************************************************************************/
        public static void ClearPanelControls(DevExpress.XtraEditors.PanelControl panel)
        {
            while (panel.Controls.Count > 0)
            {
                var c = panel.Controls[0];
                var t = c.GetType();
                if (t.BaseType == typeof(Form) || t.BaseType == typeof(DevExpress.XtraEditors.XtraForm))
                    ((Form)panel.Controls[0]).Close();
                else
                    panel.Controls.RemoveAt(0);
            }
        }
        /****************************************************************************************/
        public static string auditFile = "fix.txt";
        public static void CreateAudit(string file = "")
        {
            DateTime date = DateTime.Now;
            //auditFile = "c:/rag/fix_" + date.ToString("yyyyMMdd_hhmmss") + ".txt";

            string path = "C:/rag/audit files/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            //if (!String.IsNullOrWhiteSpace(auditFile))
                auditFile = path + file + "_" + date.ToString("yyyyMMdd_hhmmss") + ".txt";

            string filename = auditFile;
            if (File.Exists(filename))
                File.Delete(filename);
            StreamWriter sw = File.CreateText(filename);
            sw.WriteLine(date.ToString("MM/dd/yyyy hh:mm:ss") + " Start Audit Trail . . .");
            ((IDisposable)sw).Dispose();
            sw.Close();
        }
        /****************************************************************************************/
        public static void WriteAudit(string str)
        {
            string filename = auditFile;
            if (auditFile.IndexOf ( "fix.txt") >= 0 )
            { 
            }

            if (!File.Exists(filename))
                CreateAudit();
            using (StreamWriter sw = File.AppendText(filename))
            {
                DateTime date = DateTime.Now;
                sw.WriteLine(date.ToString("MM/dd/yyyy hh:mm:ss") + " " + str);
                sw.Flush();
                sw.Close();
            }
        }
        /***********************************************************************************************/
        public static string RandomizeFilename ( string fileName )
        {
            if (fileName.ToUpper().IndexOf(".PDF") < 0)
                return fileName;
            string newFilename = "";
            int idx = fileName.IndexOf(".PDF");
            if (idx < 0)
                idx = fileName.IndexOf(".pdf");
            newFilename = fileName.Substring(0, idx);
            DateTime now = DateTime.Now;
            string subString = now.ToString("yyyyMMddHHmmss");
            newFilename += subString;
            newFilename += ".pdf";
            return newFilename;
        }
        /***********************************************************************************************/
        public static string getAvailableRAM()
        {
            if (ramCounter != null)
            {
                try
                {
                    return ramCounter.NextValue() + "Mb";
                }
                catch (Exception ex)
                {
                    LogError(ex.Message, ex, false);
                }
            }

            return string.Empty;
        }
        /****************************************************************************************/
        public static double get_used_memory()
        {
            //return 0.0;
            double memory = 0.0;
            try
            {
                PerformanceCounter objMemperf = new PerformanceCounter("Memory", "Available Bytes");
                memory = objMemperf.NextValue();
            }
            catch (Exception ex)
            {
                G1.LogError("Error in G1.get_used_memory", ex, false);
                memory = 0.0;
            }
            return memory;
        }
        /****************************************************************************************/
        public static void CleanupDataGrid(ref DevExpress.XtraGrid.GridControl dgv)
        {
            if (1 == 1)
                return;
            if (dgv == null)
                return;
            if (dgv.DataSource == null)
                return;
            //double myMen = G1.get_used_memory();
            //string str = G1.ReformatMoney(myMen);
            //str = str.Replace(".00", "");
            //G1.WriteAudit("Before Used=" + str);
            DataTable dt = (DataTable)dgv.DataSource;
            if (dt != null)
            {
                dt.Dispose();
                dt = null;
            }
            dgv.DataSource = null;
            GC.Collect();
            //myMen = G1.get_used_memory();
            //str = G1.ReformatMoney(myMen);
            //str = str.Replace(".00", "");
            //G1.WriteAudit("After Used=" + str);
        }
        /****************************************************************************************/
        public static string[] WordWrap(string text, int maxLength)
        {
            text = text.Replace("\n", " ");
            text = text.Replace("\r", " ");
            text = text.Replace(".", ". ");
            text = text.Replace(">", "> ");
            text = text.Replace("\t", " ");
            text = text.Replace(",", ", ");
            text = text.Replace(";", "; ");
            text = text.Replace("<br>", " ");
            text = text.Replace(" ", " ");

            var words = text.Split(' ');
            var currentLineLength = 0;
            var lines = new ArrayList(text.Length / maxLength);
            var currentLine = "";
            var inTag = false;

            foreach (var currentWord in words.Where(currentWord => currentWord.Length > 0))
            {
                if (currentWord.Substring(0, 1) == "<")
                    inTag = true;

                if (inTag)
                {
                    //handle filenames inside html tags
                    if (currentLine.EndsWith("."))
                    {
                        currentLine += currentWord;
                    }
                    else
                        currentLine += " " + currentWord;

                    if (currentWord.IndexOf(">", StringComparison.Ordinal) > -1)
                        inTag = false;
                }
                else
                {
                    if (currentLineLength + currentWord.Length + 1 < maxLength)
                    {
                        currentLine += " " + currentWord;
                        currentLineLength += (currentWord.Length + 1);
                    }
                    else
                    {
                        lines.Add(currentLine);
                        currentLine = currentWord;
                        currentLineLength = currentWord.Length;
                    }
                }
            }
            if (currentLine != "")
                lines.Add(currentLine);

            var textLinesStr = new string[lines.Count];
            lines.CopyTo(textLinesStr, 0);
            return textLinesStr;
        }
        /***********************************************************************************************/
        public static void CreateCSVfile(DataTable dtable, string strFilePath, bool includeHeader = false, string delimiter = ",")
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            int icolcount = dtable.Columns.Count;
            string str = "";
            string colName = "";
            if (includeHeader)
            {
                for (int i = 0; i < icolcount; i++)
                {
                    colName = dtable.Columns[i].ColumnName;
                    sw.Write(colName);
                    if (i < icolcount - 1)
                        sw.Write(delimiter);
                }
                sw.Write(sw.NewLine);
            }
            DateTime date = DateTime.Now;

            string data = "";
            foreach (DataRow drow in dtable.Rows)
            {
                for (int i = 0; i < icolcount; i++)
                {
                    if (!Convert.IsDBNull(drow[i]))
                    {
                        colName = dtable.Columns[i].ColumnName.ToUpper();
                        if (drow[i].ObjToString().IndexOf(",") >= 0)
                        {
                            str = drow[i].ObjToString();
                            str = str.Replace("\\,", ",");
                            str = str.Replace(",", "\\,");
                            //                            str = "\"" + str + "\"";
                            sw.Write(str);
                        }
                        else
                        {
                            if (colName.IndexOf("DATE") >= 0)
                            {
                                date = drow[i].ObjToDateTime();
                                data = "";
                                if (date.Year > 1)
                                    data = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2") + " 00:00:00";

                            }
                            else
                                data = drow[i].ToString();
                            sw.Write(data);
                        }
                    }
                    if (i < icolcount - 1)
                    {
                        sw.Write(delimiter);
                    }
                }
                sw.Write(sw.NewLine);
            }
            sw.Close();
            sw.Dispose();
        }
        /***********************************************************************************************/
        public static void SetupToolTip(PictureBox picture, string tip)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(picture, tip);
        }
        /***********************************************************************************************/
        public static void SetupToolTip(Button picture, string tip)
        {
            ToolTip tt = new ToolTip();
            tt.SetToolTip(picture, tip);
        }
        /***********************************************************************************************/
        public static Form IsFormOpen(string name, string contract = "" )
        {
            Form myForm = null;
            bool retval = false;
            string myContract = "";
            FormCollection fc = Application.OpenForms;
            foreach (Form Appforms in fc)
            {
                if (!String.IsNullOrWhiteSpace(contract))
                {
                    myContract = Appforms.Tag.ObjToString();
                    if (myContract == contract)
                    {
                        if (Appforms.Name.ToUpper() == name.ToUpper())
                        {
                            myForm = Appforms;
                            break;
                        }
                    }
                }
                else
                {
                    if (Appforms.Name.ToUpper() == name.ToUpper())
                    {
                        myForm = Appforms;
                        break;
                    }
                }
            }
            return (myForm);
        }
        /***********************************************************************************************/
        public static void SpyGlass (GridView gridMain)
        {
            ShowHideFindPanel(gridMain);
        }
        public static void SpyGlass(BandedGridView gridMain)
        {
            ShowHideFindPanel(gridMain);
        }
        /****************************************************************************************/
        public static void ClearAllPositions(DevExpress.XtraGrid.Views.BandedGrid.AdvBandedGridView gMain )
        {
            if (gMain == null)
                return;

            for (int i = 0; i < gMain.Columns.Count; i++)
            {
                gMain.Columns[i].Visible = false;
            }
        }
        /***********************************************************************************************/
        public static void ShowHideFindPanel(GridView gridMain)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
            {
                gridMain.FindFilterText = "";
                gridMain.OptionsFind.AlwaysVisible = false;
                gridMain.FindFilterText = "";
                gridMain.HideFindPanel();
            }
            else
            {
                try
                {
                    gridMain.OptionsFind.AlwaysVisible = true;
                    gridMain.ShowFindPanel();
                    FindControl find = gridMain.GridControl.Controls.Find("FindControlCore", true)[0] as FindControl;
                    find.FindEdit.Focus();
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        public static void ShowHideFindPanel(BandedGridView gridMain)
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
            {
                gridMain.OptionsFind.AlwaysVisible = false;
                gridMain.HideFindPanel();
            }
            else
            {
                try
                {
                    gridMain.OptionsFind.AlwaysVisible = true;
                    gridMain.ShowFindPanel();
                    FindControl find = gridMain.GridControl.Controls.Find("FindControlCore", true)[0] as FindControl;
                    find.FindEdit.Focus();
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        public static string GetFuneralFileDirectory ( string workContract )
        {
            string cmd = "Select * from `fcustomers` WHERE `contractNumber`= '" + workContract + "';";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return "";
            string serviceId = dx.Rows[0]["serviceId"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( serviceId ))
                return "";
            string firstName = dx.Rows[0]["firstName"].ObjToString();
            string lastName = dx.Rows[0]["lastName"].ObjToString();
            DateTime now = DateTime.Now;
            string mainDirectory = "C:/Users/Public/Desktop/SMFS Funerals";
            GrantDirectoryAccess ( mainDirectory );
            if (!Directory.Exists(mainDirectory))
                Directory.CreateDirectory(mainDirectory);
            if (!Directory.Exists(mainDirectory))
            {
                MessageBox.Show("***ERROR*** Problen Creating Directory\n" + mainDirectory + "!!!", "Bad Main Directory Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return "";
            }
            string fullDirectory = mainDirectory + "/" + serviceId + "_" + lastName + "_" + firstName;
            GrantDirectoryAccess(fullDirectory);
            if (!Directory.Exists(fullDirectory))
                Directory.CreateDirectory(fullDirectory);
            if ( !Directory.Exists(fullDirectory))
            {
                MessageBox.Show("***ERROR*** Problen Creating Service ID Directory\n" + fullDirectory + "!!!", "Bad Service ID Directory Dialog", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                return "";
            }
            return fullDirectory;
        }
        /***********************************************************************************************/
        public static void ShowHideFindPanel ( AdvBandedGridView gridMain )
        {
            if (gridMain.OptionsFind.AlwaysVisible == true)
            {
                gridMain.OptionsFind.AlwaysVisible = false;
                gridMain.HideFindPanel();
            }
            else
            {
                try
                {
                    gridMain.OptionsFind.AlwaysVisible = true;
                    gridMain.ShowFindPanel();
                    FindControl find = gridMain.GridControl.Controls.Find("FindControlCore", true)[0] as FindControl;
                    find.FindEdit.Focus();
                }
                catch (Exception ex)
                {
                }
            }
        }
        /****************************************************************************/
        public static PleaseWait StartWait( string waitMessage = "" )
        {
            PleaseWait pleaseForm = null;
            if (String.IsNullOrWhiteSpace(waitMessage))
                waitMessage = "Please Wait . . .";

            pleaseForm = new PleaseWait ( waitMessage );
            pleaseForm.Show();
            pleaseForm.Refresh();
            return pleaseForm;
        }
        /****************************************************************************/
        public static void StopWait ( ref PleaseWait pleaseForm )
        {
            if ( pleaseForm != null )
            {
                pleaseForm.FireEvent1();
                //pleaseForm.Dispose();
                pleaseForm = null;
            }
        }
        /****************************************************************************/
        public static void ReadMyPDF()
        {
            string filename = "C:/rag/MS_Death_Certificate.PDF";
            DevExpress.XtraPdfViewer.PdfViewer pdfViewer1 = new DevExpress.XtraPdfViewer.PdfViewer();
            pdfViewer1.LoadDocument(filename);
            pdfViewer1.DetachStreamAfterLoadComplete = true;

            //PdfDocumentProcessor documentProcessor = new PdfDocumentProcessor();
            //documentProcessor.LoadDocument(filename);

            //using (PdfDocumentProcessor documentProcessor = new PdfDocumentProcessor())
            //{
            //    documentProcessor.LoadDocument(filename);

            //    // Obtain interactive form data from a document.
            //    PdfFormData formData = documentProcessor.GetFormData();

            //    // Specify the value for FirstName and LastName text boxes.
            //    formData["FirstName"].Value = "Janet";
            //    formData["LastName"].Value = "Leverling";

            //    // Specify the value for the Gender radio group.
            //    formData["Gender"].Value = "Female";

            //    // Specify the check box checked appearance name.
            //    formData["Check"].Value = "Yes";

            //    // Specify values for the Category list box.
            //    formData["Category"].Value = new string[] { "Entertainment", "Meals", "Morale" };

            //    // Obtain data from the Address form field and specify values for Address child form fields.
            //    PdfFormData address = formData["Address"];

            //    // Specify the value for the Country combo box. 
            //    address["Country"].Value = "United States";

            //    // Specify the value for City and Address text boxes. 
            //    address["City"].Value = "California";
            //    address["Address"].Value = "20 Maple Avenue";

            //    // Apply data to the interactive form. 
            //    documentProcessor.ApplyFormData(formData);

            //    // Save the modified document.
            //    btnFillFormData.Enabled = false;
            //    btnLoadFilledPDF.Enabled = true;
            //}
        }
        /****************************************************************************/
    }// End of Class
} // End of Namespace Generic
public struct DateTimeSpan
{
    public int Years { get; }
    public int Months { get; }
    public int Days { get; }
    public int Hours { get; }
    public int Minutes { get; }
    public int Seconds { get; }
    public int Milliseconds { get; }

    public DateTimeSpan(int years, int months, int days, int hours, int minutes, int seconds, int milliseconds)
    {
        Years = years;
        Months = months;
        Days = days;
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
        Milliseconds = milliseconds;
    }

    enum Phase { Years, Months, Days, Done }

    public static DateTimeSpan CompareDates(DateTime date1, DateTime date2)
    {
        if (date2 < date1)
        {
            var sub = date1;
            date1 = date2;
            date2 = sub;
        }

        DateTime current = date1;
        int years = 0;
        int months = 0;
        int days = 0;

        Phase phase = Phase.Years;
        DateTimeSpan span = new DateTimeSpan();
        int officialDay = current.Day;

        while (phase != Phase.Done)
        {
            switch (phase)
            {
                case Phase.Years:
                    if (current.AddYears(years + 1) > date2)
                    {
                        phase = Phase.Months;
                        current = current.AddYears(years);
                    }
                    else
                    {
                        years++;
                    }
                    break;
                case Phase.Months:
                    if (current.AddMonths(months + 1) > date2)
                    {
                        phase = Phase.Days;
                        current = current.AddMonths(months);
                        if (current.Day < officialDay && officialDay <= DateTime.DaysInMonth(current.Year, current.Month))
                            current = current.AddDays(officialDay - current.Day);
                    }
                    else
                    {
                        months++;
                    }
                    break;
                case Phase.Days:
                    if (current.AddDays(days + 1) > date2)
                    {
                        current = current.AddDays(days);
                        var timespan = date2 - current;
                        span = new DateTimeSpan(years, months, days, timespan.Hours, timespan.Minutes, timespan.Seconds, timespan.Milliseconds);
                        phase = Phase.Done;
                    }
                    else
                    {
                        days++;
                    }
                    break;
            }
        }

        return span;
    }
}
/********************************************************************************************/
public class TransparentRichTextBox : RichTextBox
{
    public TransparentRichTextBox()
    {
        base.ScrollBars = RichTextBoxScrollBars.None;
    }
    /********************************************************************************************/
    override protected CreateParams CreateParams
    {
        get
        {
            CreateParams cp = base.CreateParams;
            cp.ExStyle |= 0x20;
            return cp;
        }
    }
    /********************************************************************************************/
    override protected void OnPaintBackground(PaintEventArgs e)
    {
        this.Refresh();
    }
    /********************************************************************************************/
}
public class PanelNoScroll : System.Windows.Forms.Panel
{
    protected override System.Drawing.Point ScrollToControl(Control activeControl)
    {
        return DisplayRectangle.Location;
    }
}
public class PanelScroll : System.Windows.Forms.Panel
{
    /***********************************************************************************************/
    protected override System.Drawing.Point ScrollToControl(Control activeControl)
    {
        return DisplayRectangle.Location;
    }
    /***********************************************************************************************/
    public class PanelNoScroll : System.Windows.Forms.Panel
    {
        protected override System.Drawing.Point ScrollToControl(Control activeControl)
        {
            return DisplayRectangle.Location;
        }
    }
    /***********************************************************************************************/
}


