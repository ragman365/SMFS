using System;
using System.Data;
using System.Text;
using System.IO;
using System.Windows.Forms;
using System.Globalization;
using MySql.Data.MySqlClient;
using System.Security.Cryptography;

namespace GeneralLib
{
	/// <summary>
	/// Summary description for MySQL.
	/// </summary>
	public class MySQL
	{
        private static bool IsAuthenticated = true;

		//Database connection variables
		private static MySQL instance = null;
		static readonly object padlock = new object();
		private MySqlConnection Conn = new MySqlConnection();
		private DataTable dt;
		private static string mUserId, mPassword, mServer, mDatabase;

        public static MySqlConnection Connection
        {
            get
            {
                NumberOfConnections++;

                //if (G1.local_copy)
                //    return LocalConnection;
                //else if (G1.cchit_copy)
                //    return CCHITConnection;
                //else if (G1.test_copy)
                //    return DevConnection;
                if (IsAuthenticated)
                    return LiveConnection;
                else
                    return LiveConnection_LOGIN;
            }
        }

        private MySQL()
		{
		}

		#region Members
		public string UserId{get{return mUserId;}}
		public string Password{get{return mPassword;}}
		public string Server{get{return mServer;}}
		public string Database{get{return mDatabase;}}
		#endregion
		
		#region Static Methods
		public static MySQL setInstance(string user, string password, string database, string server)
		{
			mUserId = user;
			mPassword = password;
			mServer = server;
			mDatabase = database;

			instance = null;
			lock (padlock)
			{
				if (instance==null)
				{
					instance = new MySQL();
					instance.Conn.ConnectionString = GetConnectString();
				}
			}
		
			return instance;
		}

		public static MySQL getInstance()
		{
			if (instance==null)
			{
				lock (padlock)
				{
					if (instance==null)
					{
						instance = new MySQL();
						//						MessageBox.Show("An error has occurred!\n" +
						//							"The application has lost reference to the database!");
					}
				}
			}
			return instance;   
		}
		
		private static string GetConnectString()
		{
			return String.Format("server = {0};user id = {1};password = {2};database = {3};allow zero datetime = yes",
				mServer,mUserId,mPassword,mDatabase);
		}

        //public static MySqlConnection LocalConnection
        //{
        //    get
        //    {
        //        return new MySqlConnection("host=" + G1.ConfigurationValue("localserver") +
        //                ";database=" + G1.ConfigurationValue("database") +
        //                ";uid=root;password=nbtis01;allow zero datetime = yes");
        //    }
        //}

        public static MySqlConnection LocalPatientsConnection
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("localserver") +
                        ";database=jmapatients" + 
                        ";uid=root;password=nbtis01;allow zero datetime = yes");
            }
        }
        public static MySqlConnection EmployeesTimeConnection
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("server") +
                        ";database=aion" + 
                        ";uid=root;password=nbtis01;allow zero datetime = yes");
            }
        }
		#endregion
        
		public void SetDataBase(string db)
		{
			mDatabase = db;
			instance.Conn.ChangeDatabase(db);
		}

		public static bool CheckConnect(bool throwexc)
		{
			try
			{
                G1.conn1.Open();
                G1.conn1.Close();
				return true;
			}
			catch(MySqlException ex)
			{
				if(throwexc == true)
					throw;
				else
					ShowMessage(ex);
				return false;
			}
		}

        public static MySqlConnection ConnectionZeroDT
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("server") +
                    ";port=" + G1.ConfigurationValue("serverport") + 
					";database=" + G1.ConfigurationValue("database") +
					";uid=emruser;password=admin;allow zero datetime = no");
            }
        }

        public static Int64 NumberOfConnections = 0;

        public static MySqlConnection LiveConnection
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("server") +
                    ";port=" + G1.ConfigurationValue("serverport") + 
                        ";database=" + G1.ConfigurationValue("database") +
                        ";uid=emruser;password=admin;allow zero datetime = yes");
            }
        }

        //ToDo: Set up login user for Database(used strictly for login validation)
        //Intended for testing and authorizing login credentials only
        public static MySqlConnection LiveConnection_LOGIN
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("server") +
                    ";port=" + G1.ConfigurationValue("serverport") + 
                        ";database=" + G1.ConfigurationValue("database") +
                        ";uid=emruser;password=admin;allow zero datetime = yes");
            }
        }

        public static MySqlConnection CCHITConnection
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("CCHITserver") +
                        ";database=" + G1.ConfigurationValue("database") + 
                        ";uid=root;password=nbtis01;allow zero datetime = yes");
            }
        }

        public static MySqlConnection LocalConnection
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("localserver") +
                        ";database=" + G1.ConfigurationValue("database") +
                        ";uid=root;password=nbtis01;allow zero datetime = yes");
            }
        }

        //public static MySqlConnection LocalPatientsConnection
        //{
        //    get
        //    {
        //        return new MySqlConnection("host=" + G1.ConfigurationValue("localserver") +
        //                ";database=jmapatients" + 
        //                ";uid=root;password=nbtis01;allow zero datetime = yes");
        //    }
        //}

        public static MySqlConnection DevConnection
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("testserver") +
                        ";database=" + G1.ConfigurationValue("database") +
                        ";uid=emruser;password=admin;allow zero datetime = yes");
            }
        }

        public static MySqlConnection StatementConnection
        {
            get
            {
                return new MySqlConnection("host=" + G1.ConfigurationValue("server") +
                    ";port=" + G1.ConfigurationValue("serverport") + 
                        ";database=" + G1.ConfigurationValue("StatementDataBase") +
                        ";uid=root;password=nbtis01;allow zero datetime = yes");
            }
        }

        //public static bool CheckConnect(bool throwexc)
        //{
        //    return TestConnection(Connection, throwexc);
        //}


         public static object TryScalar(MySqlCommand cmd, bool bmsg, bool bthrow)
        {
            object obj = null;

            try
            {
                cmd.Connection.Open();
                obj = cmd.ExecuteScalar();
            }

            catch (MySqlException ex)
            {
                G1.LogError(ex.Message, ex, false);
#if(!DEBUG)
                if (bmsg)
                {
                    ShowMessage(ex);
                }
#else
                ShowMessage(ex);
#endif
                if (bthrow)
                    throw;
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
            }

            return obj;
        }
       public static DataTable TryFill(MySqlDataAdapter da, bool bmsg, bool bthrow)
        {
            DataTable dt = new DataTable();
            try
            {
                da.Fill(dt);
                return dt.Copy();
            }
            catch(MySqlException ex)
            {
                G1.LogError(ex.Message, ex, false);
                if (bmsg)
                    ShowMessage(ex);
                if(bthrow)
                    throw;

                return new DataTable();
            }
        }
        public static int TryNonQuery(MySqlCommand cmd, bool bmsg, bool bthrow)
        {
            int rowsaffected = -1;

            try
            {
                cmd.Connection.Open();
                rowsaffected = cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                G1.LogError(ex.Message, ex, false);
                if (bmsg)
                    ShowMessage(ex);
                if (bthrow)
                    throw;
            }
            finally
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
            }

            return rowsaffected;
        }
		private static void ShowMessage(MySqlException ex)
		{
			switch(ex.Number)
			{
				case 1044:
					IncorrectLogin();
					break;
				case 1045:
					IncorrectPassword();
					break;
				case 1142: case 1143:
					InsufficientRights();
					break;
				case 2013:
					LostConnection();
					break;
				default:
					Unhandled(ex);
					break;
			}
		}

		private static void IncorrectLogin()
		{
			DevExpress.XtraEditors.XtraMessageBox.Show("You have entered an incorrect Login ID or attempted to connect to an invalid Database.\n" +
				"If you believe you have reached this message an error, contact Technical Support.",
				"Incorrect Login",MessageBoxButtons.OK,MessageBoxIcon.Error);
		}

		private static void IncorrectPassword()
		{
			DevExpress.XtraEditors.XtraMessageBox.Show("You have entered an incorrect password.\n" +
				"If you believe you have reached this message an error, contact Technical Support.",
				"Incorrect Login",MessageBoxButtons.OK,MessageBoxIcon.Error);
		}

		private static void InsufficientRights()
		{
			DevExpress.XtraEditors.XtraMessageBox.Show("You do not have sufficient rights to complete the attempted task.\n" +
				"If you believe you have reached this message an error, contact the IT staff",
				"Insufficient Rights",MessageBoxButtons.OK,MessageBoxIcon.Error);
		}

		private static void LostConnection()
		{
			DevExpress.XtraEditors.XtraMessageBox.Show("Connection to the Database was lost.\n" +
				"Try your task again in a few moments.\n" + 
                "If the problem persists, contact the IT staff",
				"Connection Lost",MessageBoxButtons.OK,MessageBoxIcon.Error);
		}

		private static void Unhandled(MySqlException ex)
		{
			DevExpress.XtraEditors.XtraMessageBox.Show("The Following Unhandled Error Occurred:\n" + 
				"Error Message = " + ex.Message + "\nError Source = " + ex.Source + 
				"\nError Code = " + ex.Number.ToString(),"Unhandled Error",MessageBoxButtons.OK,
				MessageBoxIcon.Error);
		}
        /***********************************************************************************************/
        public static void SetMaxAllowedPackets()
        {
            string cmd = "SET GLOBAL max_allowed_packet=16*1024*1024;";
            try
            {
                G1.get_db_data(cmd);
            }
            catch ( Exception ex )
            {
                MessageBox.Show("***ERROR*** Setting Max Allowed Packets! " + ex.Message.ToString());
            }
            //using (MySqlConnection conn = new MySqlConnection(connectionString))
            //{
            //    using (MySqlCommand cmd = new MySqlCommand())
            //    {
            //        cmd.Connection = conn;
            //        conn.Open();

            //        cmd.CommandText = "SET GLOBAL max_allowed_packet=32*1024*1024;";
            //        cmd.ExecuteNonQuery();

            //        // Close and Reopen the Connection
            //        conn.Close();
            //        conn.Open();

            //        // Start to take effect here...
            //        // Do something....

            //        conn.Close();
            //    }
            //}
        }
        /***********************************************************************************************/
        public static void CreateCSVfile(DataTable dtable, string strFilePath, bool includeHeader = false, string delimiter = "," )
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
        public static void CreateCSVfile(DataTable dtable, string strFilePath, DataTable visibleDt, bool includeHeader = false, string delimiter = ",")
        {
            StreamWriter sw = new StreamWriter(strFilePath, false);
            int icolcount = dtable.Columns.Count;
            int jcolcount = visibleDt.Columns.Count;
            string str = "";
            string colName = "";
            int idx = 0;
            if (includeHeader)
            {
                for (int i = 0; i < jcolcount; i++)
                {
                    try
                    {
                        colName = visibleDt.Columns[i].ColumnName;
                        idx = G1.get_column_number(dtable, colName);
                        if (idx < 0)
                            continue;
                        sw.Write(colName);
                        if (i < jcolcount - 1)
                            sw.Write(delimiter);
                    }
                    catch ( Exception ex)
                    {
                    }
                }
                sw.Write(sw.NewLine);
            }
            DateTime date = DateTime.Now;

            string data = "";
            foreach (DataRow drow in dtable.Rows)
            {
                for (int i = 0; i < jcolcount; i++)
                {
                    try
                    {
                        colName = visibleDt.Columns[i].ColumnName.ToUpper();
                        idx = G1.get_column_number(dtable, colName);
                        if (idx < 0)
                            continue;
                        if (!Convert.IsDBNull(drow[idx]))
                        {
                            //colName = dtable.Columns[i].ColumnName.ToUpper();
                            if (drow[idx].ObjToString().IndexOf(",") >= 0)
                            {
                                str = drow[idx].ObjToString();
                                str = str.Replace(",", "\\,");
                                //                            str = "\"" + str + "\"";
                                sw.Write(str);
                            }
                            else
                            {
                                if (colName.IndexOf("DATE") >= 0)
                                {
                                    date = drow[idx].ObjToDateTime();
                                    data = "";
                                    if (date.Year > 1)
                                        data = date.Year.ToString("D4") + "-" + date.Month.ToString("D2") + "-" + date.Day.ToString("D2");

                                }
                                else
                                    data = drow[idx].ToString();
                                sw.Write(data);
                            }
                        }
                    }
                    catch ( Exception ex)
                    {
                    }
                    if (i < jcolcount - 1)
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
        public static void ImportMySQL( DataTable dt, string tableName, string runNumber )
        {
            DataTable orderDetail = dt.Copy();
            if ( G1.get_column_number ( orderDetail, "tmstamp") >= 0 )
                orderDetail.Columns.Remove("tmstamp");
            if (G1.get_column_number(orderDetail, "record") >= 0)
                orderDetail.Columns.Remove("record");
            if (G1.get_column_number(orderDetail, "runNumber") >= 0)
                orderDetail.Columns.Remove("runNumber");
            if (G1.get_column_number(orderDetail, "tmstamp1") >= 0)
                orderDetail.Columns.Remove("tmstamp1");
            if (G1.get_column_number(orderDetail, "standardFormula") >= 0)
                orderDetail.Columns.Remove("standardFormula");
            if (G1.get_column_number(orderDetail, "activeStatus") >= 0)
                orderDetail.Columns.Remove("activeStatus");
            if (G1.get_column_number(orderDetail, "employeeStatus") >= 0)
                orderDetail.Columns.Remove("employeeStatus");

            DataColumn Col = orderDetail.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;

            DataColumn Col1 = orderDetail.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            DataColumn Col2 = orderDetail.Columns.Add("runNumber", Type.GetType("System.Int32"));
            Col2.SetOrdinal(0);// to put the column in position 0;

            int t1Col = G1.get_column_number(orderDetail, "tmstamp1");
            DateTime myDate = DateTime.Now;
            DateTime oldDate = DateTime.MinValue;
            string str = "";

            for ( int i=0; i<orderDetail.Rows.Count; i++)
            {
                //orderDetail.Rows[i]["tmstamp"] = "0000-00-00";
                orderDetail.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                orderDetail.Rows[i]["record"] = "0";
                orderDetail.Rows[i]["runNumber"] = runNumber.ObjToInt32();
                if (t1Col > 0)
                    orderDetail.Rows[i][t1Col] = myDate.ToString("yyyy-MM-dd HH:mm:ss");
                orderDetail.Rows[i]["resultCommission"] = "0.00";
                str = orderDetail.Rows[i]["pastFailures"].ObjToString();
                if ( String.IsNullOrWhiteSpace ( str ))
                    orderDetail.Rows[i]["pastFailures"] = "0.00";
                str = orderDetail.Rows[i]["pastDBR"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    orderDetail.Rows[i]["pastDBR"] = "0.00";
                str = orderDetail.Rows[i]["splitCommission"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    orderDetail.Rows[i]["splitCommission"] = "0.00";
                str = orderDetail.Rows[i]["splitBaseCommission"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    orderDetail.Rows[i]["splitBaseCommission"] = "0.00";
                str = orderDetail.Rows[i]["splitGoalCommission"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    orderDetail.Rows[i]["splitGoalCommission"] = "0.00";
                str = orderDetail.Rows[i]["fbi$"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    orderDetail.Rows[i]["fbi$"] = "0.00";
                str = orderDetail.Rows[i]["MR"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    orderDetail.Rows[i]["MR"] = "0.00";
                str = orderDetail.Rows[i]["MC"].ObjToString();
                if (String.IsNullOrWhiteSpace(str))
                    orderDetail.Rows[i]["MC"] = "0.00";

                str = orderDetail.Rows[i]["effectiveDate"].ObjToString();
                if ( str.IndexOf ( "0000") >= 0 )
                    orderDetail.Rows[i]["effectiveDate"] = G1.DTtoMySQLDT(myDate.AddYears (500));
            }

            SMFS.Structures.TieDbTable(tableName, orderDetail);

            string cmd = "DELETE FROM `" + tableName + "` where `runNumber` = '" + runNumber + "';";
            G1.get_db_data(cmd);

            //string connectMySQL = "Server=localhost;Database=test;Uid=username;Pwd=password;";
            string strFile = "/TempFolder/MySQL" + DateTime.Now.Ticks.ToString() + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/TempFolder/"))
                Directory.CreateDirectory(Server + "/TempFolder/");

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read
            CreateCSVfile(orderDetail, Server + strFile);
            //using (MySqlConnection cn1 = new MySqlConnection(connectMySQL))
            //{
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = tableName; //Create ProductOrder table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                bcp1.Load();
            }
            catch ( Exception ex)
            {
                MessageBox.Show("A critical exception has occurred while attempting to Save Commissions\n" + ex.Message + "\n", "Commission Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            //Once data write into db then delete file..
            try
                {
                    File.Delete(Server + strFile);
                }
                catch (Exception ex)
                {
                    str = ex.Message;
                }
            //}
        }
        /***********************************************************************************************/
        public static void LapsedMySQL(DataTable dt, string tableName, string runNumber)
        {
            DataTable orderDetail = dt.Copy();
            if (G1.get_column_number(orderDetail, "tmstamp") >= 0)
                orderDetail.Columns.Remove("tmstamp");
            if (G1.get_column_number(orderDetail, "record") >= 0)
                orderDetail.Columns.Remove("record");
            if (G1.get_column_number(orderDetail, "runNumber") >= 0)
                orderDetail.Columns.Remove("runNumber");
            if (G1.get_column_number(orderDetail, "tmstamp1") >= 0)
                orderDetail.Columns.Remove("tmstamp1");
            if (G1.get_column_number(orderDetail, "standardFormula") >= 0)
                orderDetail.Columns.Remove("standardFormula");
            if (G1.get_column_number(orderDetail, "activeStatus") >= 0)
                orderDetail.Columns.Remove("activeStatus");
            if (G1.get_column_number(orderDetail, "employeeStatus") >= 0)
                orderDetail.Columns.Remove("employeeStatus");

            DataColumn Col2 = orderDetail.Columns.Add("runNumber", Type.GetType("System.Int32"));
            Col2.SetOrdinal(0);// to put the column in position 0;

            DataColumn Col = orderDetail.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;

            DataColumn Col1 = orderDetail.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            int t1Col = G1.get_column_number(orderDetail, "tmstamp1");
            DateTime myDate = DateTime.Now;
            DateTime oldDate = DateTime.MinValue;
            string str = "";

            for (int i = 0; i < orderDetail.Rows.Count; i++)
            {
                try
                {
                    orderDetail.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    orderDetail.Rows[i]["record"] = "0";
                    orderDetail.Rows[i]["runNumber"] = runNumber.ObjToInt32();
                    if (t1Col > 0)
                        orderDetail.Rows[i][t1Col] = myDate.ToString("yyyy-MM-dd HH:mm:ss");
                    str = orderDetail.Rows[i]["formula"].ObjToString();
                    if (str.Length > 250)
                    {
                        str = str.Substring(0, 250);
                        orderDetail.Rows[i]["formula"] = str;
                    }
                }
                catch ( Exception ex)
                {
                }

                //str = orderDetail.Rows[i]["effectiveDate"].ObjToString();
                //if (str.IndexOf("0000") >= 0)
                //    orderDetail.Rows[i]["effectiveDate"] = G1.DTtoMySQLDT(myDate.AddYears(500));
            }

            FixDecimal(orderDetail, "apr");
            FixDecimal(orderDetail, "fbi");
            FixDecimal(orderDetail, "dbcMoney");
            FixDecimal(orderDetail, "downPayment");
            FixDecimal(orderDetail, "lapseContract$");
            FixDecimal(orderDetail, "reinstateContract$");
            FixDecimal(orderDetail, "dbc");
            FixDecimal(orderDetail, "paymentAmount");
            FixDecimal(orderDetail, "numMonthPaid");
            FixDecimal(orderDetail, "debitAdjustment");
            FixDecimal(orderDetail, "creditAdjustment");
            FixDecimal(orderDetail, "interestPaid");
            FixDecimal(orderDetail, "trust85P");
            FixDecimal(orderDetail, "trust100P");
            FixDecimal(orderDetail, "whatever");
            FixDecimal(orderDetail, "oldBalance");
            FixDate(orderDetail, "lastDatePaid");
            FixDate(orderDetail, "lastDatePaid8");

            CleanupTable(orderDetail);

            SMFS.Structures.TieDbTable(tableName, orderDetail);

            string cmd = "DELETE FROM `" + tableName + "` where `runNumber` = '" + runNumber + "';";
            G1.get_db_data(cmd);

            //string connectMySQL = "Server=localhost;Database=test;Uid=username;Pwd=password;";
            string strFile = "/TempFolder/MySQL" + DateTime.Now.Ticks.ToString() + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/TempFolder/"))
                Directory.CreateDirectory(Server + "/TempFolder/");

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read
            CreateCSVfile(orderDetail, Server + strFile);
            //using (MySqlConnection cn1 = new MySqlConnection(connectMySQL))
            //{
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = tableName; //Create ProductOrder table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("A critical exception has occurred while attempting to Save Commissions\n" + ex.Message + "\n", "Commission Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            //Once data write into db then delete file..
            try
            {
                File.Delete(Server + strFile);
            }
            catch (Exception ex)
            {
                str = ex.Message;
            }
            //}
        }
        /***********************************************************************************************/
        public static void FixDecimal ( DataTable dt, string columnName )
        {
            string str = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    str = dt.Rows[i][columnName].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        dt.Rows[i][columnName] = "0.00";
                    else if (str == "0")
                        dt.Rows[i][columnName] = "0.00";
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        public static void FixDate(DataTable dt, string columnName)
        {
            string str = "";
            DateTime date = new DateTime(1910, 1, 1, 1, 1, 1);
            DateTime newDate = DateTime.Now;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    str = dt.Rows[i][columnName].ObjToString();
                    if (String.IsNullOrWhiteSpace(str))
                        dt.Rows[i][columnName] = G1.DTtoMySQLDT(date);
                    else if (!G1.validate_date(str))
                        dt.Rows[i][columnName] = G1.DTtoMySQLDT(date);
                    else if (str.IndexOf("/") >= 0)
                    {
                        newDate = str.ObjToDateTime();
                        dt.Rows[i][columnName] = G1.DTtoMySQLDT(newDate);
                    }
                }
                catch (Exception ex)
                {
                }
            }
        }
        /***********************************************************************************************/
        public static void TrustMySQL(DataTable dt, string tableName, string runNumber)
        { // Force all date fields in mySQL to be varchar 100
            DataTable orderDetail = dt.Copy();
            if (G1.get_column_number(orderDetail, "tmstamp") >= 0)
                orderDetail.Columns.Remove("tmstamp");
            if (G1.get_column_number(orderDetail, "record") >= 0)
                orderDetail.Columns.Remove("record");
            if (G1.get_column_number(orderDetail, "runNumber") >= 0)
                orderDetail.Columns.Remove("runNumber");
            if (G1.get_column_number(orderDetail, "tmstamp1") >= 0)
                orderDetail.Columns.Remove("tmstamp1");
            if (G1.get_column_number(orderDetail, "standardFormula") >= 0)
                orderDetail.Columns.Remove("standardFormula");
            if (G1.get_column_number(orderDetail, "activeStatus") >= 0)
                orderDetail.Columns.Remove("activeStatus");
            if (G1.get_column_number(orderDetail, "employeeStatus") >= 0)
                orderDetail.Columns.Remove("employeeStatus");

            DataColumn Col2 = orderDetail.Columns.Add("runNumber", Type.GetType("System.Int32"));
            Col2.SetOrdinal(0);// to put the column in position 0;

            DataColumn Col = orderDetail.Columns.Add("record", System.Type.GetType("System.String"));
            Col.SetOrdinal(0);// to put the column in position 0;

            DataColumn Col1 = orderDetail.Columns.Add("tmstamp", System.Type.GetType("System.String"));
            Col1.SetOrdinal(0);// to put the column in position 0;

            int t1Col = G1.get_column_number(orderDetail, "tmstamp1");
            DateTime myDate = DateTime.Now;
            DateTime oldDate = DateTime.MinValue;
            string str = "";

            for (int i = 0; i < orderDetail.Rows.Count; i++)
            {
                orderDetail.Rows[i]["tmstamp"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                orderDetail.Rows[i]["record"] = "0";
                orderDetail.Rows[i]["runNumber"] = runNumber.ObjToInt32();
                if (t1Col > 0)
                    orderDetail.Rows[i][t1Col] = myDate.ToString("yyyy-MM-dd HH:mm:ss");
                //orderDetail.Rows[i]["resultCommission"] = "0.00";
                //str = orderDetail.Rows[i]["pastFailures"].ObjToString();
                //if (String.IsNullOrWhiteSpace(str))
                //    orderDetail.Rows[i]["pastFailures"] = "0.00";
                //str = orderDetail.Rows[i]["pastDBR"].ObjToString();
                //if (String.IsNullOrWhiteSpace(str))
                //    orderDetail.Rows[i]["pastDBR"] = "0.00";
                //str = orderDetail.Rows[i]["splitCommission"].ObjToString();
                //if (String.IsNullOrWhiteSpace(str))
                //    orderDetail.Rows[i]["splitCommission"] = "0.00";
                //str = orderDetail.Rows[i]["splitBaseCommission"].ObjToString();
                //if (String.IsNullOrWhiteSpace(str))
                //    orderDetail.Rows[i]["splitBaseCommission"] = "0.00";
                //str = orderDetail.Rows[i]["splitGoalCommission"].ObjToString();
                //if (String.IsNullOrWhiteSpace(str))
                //    orderDetail.Rows[i]["splitGoalCommission"] = "0.00";
                //str = orderDetail.Rows[i]["fbi$"].ObjToString();
                //if (String.IsNullOrWhiteSpace(str))
                //    orderDetail.Rows[i]["fbi$"] = "0.00";

                //str = orderDetail.Rows[i]["effectiveDate"].ObjToString();
                //if (str.IndexOf("0000") >= 0)
                //    orderDetail.Rows[i]["effectiveDate"] = G1.DTtoMySQLDT(myDate.AddYears(500));
            }

            CleanupTable(orderDetail);

            SMFS.Structures.TieDbTable(tableName, orderDetail);

            string cmd = "DELETE FROM `" + tableName + "` where `runNumber` = '" + runNumber + "';";
            G1.get_db_data(cmd);

            //string connectMySQL = "Server=localhost;Database=test;Uid=username;Pwd=password;";
            string strFile = "/TempFolder/MySQL" + DateTime.Now.Ticks.ToString() + ".csv";
            string Server = "C:/rag";
            //Create directory if not exist... Make sure directory has required rights..
            if (!Directory.Exists(Server + "/TempFolder/"))
                Directory.CreateDirectory(Server + "/TempFolder/");

            //If file does not exist then create it and right data into it..
            if (!File.Exists(Server + strFile))
            {
                FileStream fs = new FileStream(Server + strFile, FileMode.Create, FileAccess.Write);
                fs.Close();
                fs.Dispose();
            }

            //Generate csv file from where data read
            CreateCSVfile(orderDetail, Server + strFile);
            //using (MySqlConnection cn1 = new MySqlConnection(connectMySQL))
            //{
            try
            {
                G1.conn1.Open();
                MySqlBulkLoader bcp1 = new MySqlBulkLoader(G1.conn1);
                bcp1.TableName = tableName; //Create ProductOrder table into MYSQL database...
                bcp1.FieldTerminator = ",";

                bcp1.LineTerminator = "\r\n";
                bcp1.FileName = Server + strFile;
                bcp1.NumberOfLinesToSkip = 0;
                bcp1.Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("A critical exception has occurred while attempting to Save Commissions\n" + ex.Message + "\n", "Commission Error Dialog", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
            }

            //Once data write into db then delete file..
            try
            {
                File.Delete(Server + strFile);
            }
            catch (Exception ex)
            {
                str = ex.Message;
            }
            //}
        }
        /***********************************************************************************************/
        public static bool CleanupTable(DataTable dt)
        {
            string original = "";
            string name = "";
            string type = "";
            string length = "";
            string mod = "";
            string columnname = "";
            string newstr = "";
            DateTime date = DateTime.Now;
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
                    if ( name == "LASTDATEPAID")
                    {
                    }
                    type = dt.Columns[i].DataType.ObjToString().ToUpper();
                    length = "100";
                    if (name.Trim().Length == 0)
                        continue;
                        newstr = "";
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        newstr = dt.Rows[j][i].ObjToString();
                        if (String.IsNullOrWhiteSpace(newstr))
                        {
                            if (type == "SYSTEM.DOUBLE")
                                dt.Rows[j][i] = "0.00";
                            else if (type == "SYSTEM.DECIMAL")
                                dt.Rows[j][i] = "0.00";
                            else if (type == "SYSTEM.INT32")
                                dt.Rows[j][i] = "0";
                            else if (type == "SYSTEM.INT64")
                                dt.Rows[j][i] = "0";
                            else if (type == "MYSQL.DATA.TYPES.MYSQLDATETIME")
                            {
                            }
                        }
                        else
                        {
                            if ((type == "SYSTEM.DOUBLE" || type == "SYSTEM.DECIMAL") && newstr == "0")
                                dt.Rows[j][i] = "0.00";
                            else if ( type == "MYSQL.DATA.TYPES.MYSQLDATETIME")
                            {
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** " + ex.Data.ToString());
                return false;
            }
            return true;
        }
        /***********************************************************************************************/
    }
}