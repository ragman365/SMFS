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
using GeneralLib;
namespace Tracking
{
    /****************************************************************************/
    public class T1
    {
        /****************************************************************************************/
        public static void SaveOtherData(string workContract, DataTable dt, bool funeral)
        {
            try
            {
                string custExtendedFile = "cust_extended";
                if (funeral)
                    custExtendedFile = "fcust_extended";
                string cmd = "Select * from `" + custExtendedFile + "` where `contractNumber` = '" + workContract + "';";
                DataTable dx = G1.get_db_data(cmd);

                string record = "";
                if (dx.Rows.Count > 0)
                    record = dx.Rows[0]["record"].ObjToString();
                if (String.IsNullOrWhiteSpace(record) || record == "-1")
                    record = G1.create_record(custExtendedFile, "field", "-1");
                if (G1.BadRecord(custExtendedFile, record))
                    return;
                G1.update_db_table(custExtendedFile, "record", record, new string[] { "contractNumber", workContract });

                string dbfield = "";
                string data = "";
                string mod = "";
                string myList = "";
                DateTime serviceDate = DateTime.Now;
                string mysqlDate = "";
                bool gotTracking = false;
                string tracking = "";
                string dropOnly = "";
                string addContact = "";
                try
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        mod = dt.Rows[i]["mod"].ObjToString();
                        if (mod == "Y")
                        {
                            tracking = dt.Rows[i]["tracking"].ObjToString();
                            if (tracking.ToUpper() == "T")
                                gotTracking = true;
                            dropOnly = dt.Rows[i]["dropOnly"].ObjToString();
                            addContact = dt.Rows[i]["addContact"].ObjToString();
                            dbfield = dt.Rows[i]["dbfield"].ObjToString();
                            if (String.IsNullOrWhiteSpace(dbfield))
                                continue;
                            data = dt.Rows[i]["data"].ObjToString();
                            data = G1.protect_data(data);
                            if (G1.get_column_number(dx, dbfield) >= 0)
                            {
                                if (data.IndexOf(",") >= 0)
                                {
                                    G1.update_db_table(custExtendedFile, "record", record, new string[] { dbfield, data });
                                }
                                else
                                    myList += dbfield + "," + data + ",";
                                if (dbfield.ToUpper() == "SRVDATE")
                                {
                                    serviceDate = data.ObjToDateTime();
                                    mysqlDate = serviceDate.ToString("MM/dd/yyyy");
                                    G1.update_db_table(custExtendedFile, "record", record, new string[] { "serviceDate", mysqlDate });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                }

                //if (funeral)
                //{
                //    DataRow[] dRows = dt.Select("dbfield='Funeral Arranger'");
                //    if (dRows.Length > 0)
                //    {
                //        string arranger = dRows[0]["data"].ObjToString();
                //        if (String.IsNullOrWhiteSpace(arranger))
                //        {
                //            string serviceId = dx.Rows[0]["serviceId"].ObjToString();
                //            Messages.SendTheMessage(LoginForm.username, "cjenkins", "Blank Arranger for Service Id " + serviceId, "Funeral Service Id " + serviceId );
                //        }
                //    } 
                //}
                //            myList = myList.TrimEnd(',');
                if (String.IsNullOrWhiteSpace(myList))
                    return;
                try
                {
                    myList = myList.Remove(myList.LastIndexOf(","), 1);
                    if (!String.IsNullOrWhiteSpace(myList))
                    {
                        G1.update_db_table(custExtendedFile, "record", record, myList);
                        if (gotTracking && funeral)
                        {
                            cmd = "Select * from `" + custExtendedFile + "` where `contractNumber` = '" + workContract + "';";
                            dx = G1.get_db_data(cmd);
                            {
                                if (dx.Rows.Count > 0)
                                    serviceDate = dx.Rows[0]["serviceDate"].ObjToDateTime();
                                ProcessTracking(dt, serviceDate);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("***ERROR*** Updating Extended Data for Contract " + workContract + " Error " + ex.Message.ToString());
                }
            }
            catch (Exception ex)
            {
            }
        }
        /****************************************************************************************/
        public static string DecodeDirector(string director)
        {
            if (String.IsNullOrWhiteSpace(director))
                return director;
            try
            {
                int idx = director.IndexOf("[");
                if (idx > 0)
                {
                    string license = director.Substring((idx + 1));
                    license = license.Replace("]", "").Trim();
                    string cmd = "Select * from `directors` where `license` = '" + license + "';";
                    DataTable dx = G1.get_db_data(cmd);
                    if ( dx.Rows.Count > 0 )
                    {
                        director = dx.Rows[0]["lastName"].ObjToString();
                        string firstName = dx.Rows[0]["firstName"].ObjToString();
                        string middleName = dx.Rows[0]["middleName"].ObjToString();
                        director += ", " + firstName;
                        if (!String.IsNullOrWhiteSpace(middleName))
                            director += " " + middleName;
                    }

                }
            }
            catch ( Exception ex )
            {
            }
            return director;
        }
        /****************************************************************************************/
        public static void ProcessTracking(DataTable dt, DateTime serviceDate )
        {
            string dbfield = "";
            string data = "";
            string mod = "";
            string myList = "";
            string tracking = "";
            string dropOnly = "";
            string addContact = "";
            string contacts = "";
            string cmd = "";
            string record = "";
            string[] Lines = null;
            string[] nLines = null;
            string[] mLines = null;
            string field = "";
            string contactName = "";
            string str1 = "";
            string str2 = "";
            DataTable dx = null;
            string location = EditCust.activeFuneralHomeName;

            try
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    mod = dt.Rows[i]["mod"].ObjToString();
                    if (mod == "Y")
                    {
                        tracking = dt.Rows[i]["tracking"].ObjToString();
                        dropOnly = dt.Rows[i]["dropOnly"].ObjToString();
                        if (dropOnly == "1")
                            continue;
                        addContact = dt.Rows[i]["addContact"].ObjToString();
                        dbfield = dt.Rows[i]["dbfield"].ObjToString();
                        if (String.IsNullOrWhiteSpace(dbfield))
                            continue;
                        if (tracking.ToUpper() != "T")
                            continue;
                        data = dt.Rows[i]["data"].ObjToString();
                        data = G1.protect_data(data);
                        //location = dt.Rows[i]["location"].ObjToString();
                        //if (String.IsNullOrWhiteSpace(location))
                        //    location = EditCust.activeFuneralHomeName;

                        contactName = data;

                        cmd = "Select * from `track` where `tracking` = '" + dbfield + "' AND `answer` = '" + data + "' ";
                        //if (!String.IsNullOrWhiteSpace(location))
                        //    cmd += " AND `location` = '" + location + "'";
                        cmd += ";";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                        {
                            record = G1.create_record("track", "answer", "-1");
                            if (G1.BadRecord("track", record))
                                return;
                            G1.update_db_table("track", "record", record, new string[] { "tracking", dbfield, "answer", data, "location", EditCust.activeFuneralHomeName });
                            dx = G1.get_db_data(cmd);
                        }
                        if (dx.Rows.Count > 0)
                        {
                            myList = "";
                            record = dx.Rows[0]["record"].ObjToString();
                            DataRow[] dRows = dt.Select("reference LIKE '" + dbfield + "~%'");
                            if (dRows.Length > 0)
                            {
                                string reference = "";
                                for (int j = 0; j < dRows.Length; j++)
                                {
                                    data = dRows[j]["data"].ObjToString();
                                    reference = dRows[j]["reference"].ObjToString();
                                    Lines = reference.Split('~');
                                    if (Lines.Length > 1)
                                    {
                                        field = Lines[1].Trim();
                                        if (!String.IsNullOrWhiteSpace(data))
                                        {
                                            if ( field.IndexOf ( "," ) > 0 )
                                            {
                                                nLines = field.Split(',');
                                                mLines = data.Split(',');
                                                for ( int kk=0; kk<nLines.Length; kk++)
                                                {
                                                    str1 = nLines[kk].Trim();
                                                    str1 = str1.Replace("+", "" );
                                                    str1 = str1.Trim();
                                                    str2 = mLines[kk].Trim();
                                                    myList += str1 + "," + str2 + ",";
                                                }
                                            }
                                            else
                                                myList += field + "," + data + ",";
                                        }
                                    }
                                }
                            }
                            if (!String.IsNullOrWhiteSpace(record) && !String.IsNullOrWhiteSpace(myList))
                            {
                                myList = myList.Remove(myList.LastIndexOf(","), 1);
                                G1.update_db_table("track", "record", record, myList);
                            }
                            if ( addContact == "1" )
                            {
                                if (!contacts.Contains(contactName))
                                    contacts += dbfield + "~" + contactName + "~";
                            }
                        }
                    }
                }
                if (!String.IsNullOrWhiteSpace(contacts))
                    AddContacts(contacts, serviceDate);
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Updating Trackingm Data Error " + ex.Message.ToString());
            }
        }
        /****************************************************************************************/
        public static void AddContacts ( string contacts, DateTime serviceDate )
        {
            string contact = "";
            string dbfield = "";
            string contactType = "";
            string agent = "";
            string str = "";
            int frequency = 4;
            DateTime lastContactDate = DateTime.Now;
            DateTime newContactDate = DateTime.Now;
            bool foundLastContactDate = false;
            string record = "";
            bool addContact = false;
            int months = 0;

            string cmd = "DELETE from `contacts` WHERE `agent` = '-1'";
            G1.get_db_data(cmd);

            DataTable dx = null;
            string[] Lines = contacts.Split('~');
            for ( int i=0; i<Lines.Length; i=i+2)
            {
                addContact = false;
                try
                {
                    dbfield = Lines[i].Trim();
                    if (String.IsNullOrWhiteSpace(dbfield))
                        continue;
                    contact = Lines[i + 1].Trim();
                    if (String.IsNullOrWhiteSpace(contact))
                        continue;

                    contactType = "";
                    foundLastContactDate = false;
                    agent = "";
                    cmd = "Select * from `contacts` where `contactName` = '" + contact + "' ORDER by `apptDate` DESC;";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        contactType = dx.Rows[0]["contactType"].ObjToString().Trim();
                        lastContactDate = dx.Rows[0]["apptDate"].ObjToDateTime();
                        foundLastContactDate = true;
                        frequency = dx.Rows[0]["frequency"].ObjToInt32();
                        if (frequency <= 0)
                            frequency = 4;
                        str = dx.Rows[0]["agent"].ObjToString().Trim();
                        if (!String.IsNullOrWhiteSpace(str))
                            agent = str;
                    }

                    if (String.IsNullOrWhiteSpace(contactType))
                    {
                        cmd = "Select * from `track` where `tracking` = '" + dbfield + "' AND `answer` = '" + contact + "' ";
                        cmd += ";";
                        dx = G1.get_db_data(cmd);
                        if (dx.Rows.Count <= 0)
                            continue;
                        for (int j = 0; j < dx.Rows.Count; j++)
                        {
                            str = dx.Rows[j]["contactType"].ObjToString().Trim();
                            if (!String.IsNullOrWhiteSpace(str))
                            {
                                contactType = str;
                                break;
                            }
                        }
                    }
                    if (String.IsNullOrWhiteSpace(agent))
                    {
                        agent = RTF_Stuff.activeFuneralHomeDirector;
                        agent = DecodeDirector(agent);
                    }

                    if (!foundLastContactDate)
                    {
                        newContactDate = serviceDate.AddMonths(frequency);
                        addContact = true;
                    }
                    else
                    {
                        if (lastContactDate > serviceDate)
                        {
                            months = G1.GetMonthsBetween(lastContactDate, serviceDate);
                            if (months > 1 && months < frequency)
                            {
                                addContact = true;
                                newContactDate = serviceDate.AddMonths(frequency);
                            }
                        }
                        else
                        {
                            months = G1.GetMonthsBetween(serviceDate, lastContactDate);
                            if (months > 1 && months < frequency)
                            {
                                addContact = true;
                                newContactDate = serviceDate.AddMonths(frequency);
                            }
                        }
                    }
                    if ( addContact )
                    {
                        record = G1.create_record("contacts", "agent", "-1");
                        if (G1.BadRecord("contacts", record))
                            return;
                        G1.update_db_table("contacts", "record", record, new string[] { "contactName", contact, "agent", agent, "apptDate", newContactDate.ToString("yyyy-MM-dd"), "contactType", contactType });
                    }
                }
                catch ( Exception ex)
                {
                }
            }
        }
        /****************************************************************************************/
    }
}
