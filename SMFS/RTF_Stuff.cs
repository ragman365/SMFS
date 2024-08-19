using System;
using System.Windows.Forms;
using System.Data;
using System.IO;
//using RAGSpread;
using System.Text;
using System.Linq;
using DevExpress.XtraRichEdit;
using GeneralLib;
using EMRControlLib;

namespace SMFS
{
    /****************************************************************************/
    public class RTF_Stuff
    {
        public string cNum = "";
        public DataTable relativesDB = null;
        public static string activeFuneralHomeDirector = "";
        public static string activeFuneralHomeArranger = "";
        private bool removeEmptyDefault = true;
        /***********************************************************************************************/
        public static void ProcessRTB(string contractNumber, string formLocation, string formName, RichEditControl rtbx = null, bool removeEmpty = false)
        {
            DataTable dx = RTF_Stuff.ExtractFields(rtbx.Document.RtfText);
            RTF_Stuff.LoadFields(contractNumber, dx, rtbx, formLocation, formName, removeEmpty );
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

            string lines = RTF_Stuff.ParseFields(text, "[*");
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
                str = RTF_Stuff.cleanupField(str);
                if (!String.IsNullOrWhiteSpace(str))
                {
                    //if (str.ToUpper() != "MEMCONTENT")
                    //{
                        //dR = dt.Select("lookup='" + str + "'");
                        //if (dR.Length <= 0)
                        //{
                            DataRow dRow = dt.NewRow();
                            dRow["field"] = field;
                            dRow["lookup"] = str;
                            dt.Rows.Add(dRow);
                        //}
                    //}
                }
            }
            lines = RTF_Stuff.ParseFields(text, "[%");
            Lines = lines.Split('\n');
            str = "";
            bool holding = false;
            string hold = "";
            string searchHold = "";
            for (int i = 0; i < Lines.Length; i++)
            {
                field = Lines[i];
                str = field;
                str = str.Replace("[*", "");
                str = str.Replace("*]", "");
                str = str.Replace("[%", "");
                str = str.Replace("%]", "");
                str = str.Replace("\'94", "");
                str = str.Replace("\'96", "");
                str = str.Replace("\\u8221", "");
                str = str.Replace("\\", "");
                str = RTF_Stuff.cleanupField(str);
                if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                {
                    holding = true;
                    hold = str + "~";
                    searchHold = Lines[i];
                    continue;
                }
                if (str.ToUpper().IndexOf("END_TABLE") >= 0 && holding)
                {
                    holding = false;
                    hold += str;
                    searchHold += Lines[i];
                    str = hold;
                    field = searchHold;
                }
                if (holding)
                {
                    hold += str + "~";
                    searchHold += Lines[i];
                    continue;
                }
                if (!String.IsNullOrWhiteSpace(str))
                {
                    //if (str.ToUpper() != "MEMCONTENT")
                    //{
                        //dR = dt.Select("lookup='" + str + "'");
                        //if (dR.Length <= 0)
                        //{
                            DataRow dRow = dt.NewRow();
                            dRow["field"] = field;
                            dRow["lookup"] = str;
                            dRow["search"] = searchHold;
                            dt.Rows.Add(dRow);
                            searchHold = "";
                        //}
                    //}
                }
            }
            return dt;
        }
        /***********************************************************************************************/
        public static void LoadFields(string contractNumber, DataTable dx, RichEditControl rtbx = null, string formLocation = "", string formName = "", bool removeEmpty = false )
        {
            string cmd = "Select * from `structures` where `form` = '" + formName + "' order by `order`;";
            DataTable dt = G1.get_db_data(cmd);
            dt.Columns.Add("num");
            dt.Columns.Add("mod");
            dt.Columns.Add("F1");
            dt.Columns.Add("F2");
            dt.Columns.Add("search");
            DataRow[] dRows = null;
            string field = "";
            string str = "";
            bool gotTable = false;
            for (int i = 0; i < dx.Rows.Count; i++)
            {
                field = dx.Rows[i]["field"].ObjToString();
                str = field;
                str = str.Replace("[*", "");
                str = str.Replace("*]", "");
                str = str.Replace("[%", "");
                str = str.Replace("%]", "");
                str = RTF_Stuff.cleanupField(str);
                if (str.ToUpper() == "DECPIC")
                {

                }
                if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                    gotTable = true;
                else if (str.ToUpper().IndexOf("END_TABLE") >= 0)
                    gotTable = false;
                dRows = dt.Select("field='" + str + "'");
                if (dRows.Length <= 0 || gotTable)
                {
                    DataRow dR = dt.NewRow();
                    dR["F1"] = field;
                    dt.Rows.Add(dR);
                }
                else
                {
                    dRows[0]["F1"] = field;
                }
            }
            dt = LoadDbFields(contractNumber, formLocation, dt);
            PushFieldsToForm(contractNumber, dt, rtbx, removeEmpty);
        }
        /***********************************************************************************************/
        public static string cleanupField(string field)
        {
            string str = "";
            field = field.Replace("\'94", "");
            for (int i = field.Length - 1; i >= 0; i--)
            {
                str = field.Substring(i, 1);
                if (str == "%")
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
            bool found = false;
            for (;;)
            {
                found = false;
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
                        found = true;
                        if (saveField.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
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
                if (!found)
                    break;
            }
            return lines;
        }
        /***********************************************************************************************/
        public static DataTable LoadDbFields(string workContractNumber, string formLocation, DataTable dt)
        {
            if (String.IsNullOrWhiteSpace(workContractNumber))
                return dt;
            string table = "";
            string field = "";
            string qualify = "";
            string dbfield = "";
            string tableColumn = "";
            string qualifier = "";
            string moreOptions = "";
            string help = "";
            int idx = 0;
            string data = "";
            string str = "";
            string cmd = "";
            string[] lines = null;
            string file = "";
            bool gotDate = false;
            bool gotWithout = false;
            RichTextBoxEx rtb = null;
            FunServices serviceForm = null;
            DataTable funDt = null;
            DataTable payDt = null;
            bool gotMultiple = false;
            int multipleBy = 1;
            bool gotit = false;

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                try
                {
                    gotMultiple = false;
                    multipleBy = 1;
                    field = dt.Rows[i]["field"].ObjToString();
                    if (field.ToUpper() == "MEMTITLE" || field.ToUpper() == "MEMCONTENT")
                    {
                        continue;
                    }
                    if ( field.ToUpper() == "DECPIC")
                    {
                        dt.Rows[i]["help"] = dt.Rows[i]["more_options"].ObjToString();
                    }
                    if (field.ToUpper() == "PB1")
                    {
                    }
                    if (field.ToUpper() == "DOS")
                    {
                        cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContractNumber + "';";
                        DataTable ddx = G1.get_db_data(cmd);
                        if (ddx.Rows.Count > 0)
                        {
                            DateTime cremDate = ddx.Rows[0]["crematoriumDate"].ObjToDateTime();
                            if (cremDate.Year > 100)
                            {
                                dt.Rows[i]["F2"] = cremDate.ToString("MM/dd/yyyy");
                                continue;
                            }
                        }
                    }
                    field = dt.Rows[i]["F1"].ObjToString();
                    if (field.ToUpper().IndexOf("GPL ED") >= 0)
                    {
                        dt.Rows[i]["F2"] = GetGPLED(workContractNumber);
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("CPL ED") >= 0)
                    {
                        dt.Rows[i]["F2"] = GetCPLED(workContractNumber);
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("OBC ED") >= 0)
                    {
                        dt.Rows[i]["F2"] = GetOBCED(workContractNumber);
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("[%DATE(DOM)%]") >= 0)
                    {
                        dt.Rows[i]["F2"] = G1.DayOfMonthText(DateTime.Now);
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("[%DATE(MONTHTXT)%]") >= 0)
                    {
                        dt.Rows[i]["F2"] = DateTime.Now.ToString("MMMMMMMMMMMM");
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("[%YEAR%]") >= 0)
                    {
                        dt.Rows[i]["F2"] = DateTime.Now.Year.ToString();
                        continue;
                    }
                    if ( field.ToUpper().IndexOf( "CASKET MODEL AND DESCRIPTION") == 0 )
                    {
                        dt.Rows[i]["F2"] = field;
                        continue;
                    }
                }
                catch ( Exception ex)
                {
                }

                field = dt.Rows[i]["field"].ObjToString();
                if ( field.ToUpper().IndexOf ( "PB") == 0 )
                {
                }
                if (field.Trim().ToUpper().IndexOf("CASKET MODEL AND DESCRIPTION") == 0)
                {
                    if (funDt == null)
                    {
                        serviceForm = new FunServices(workContractNumber);
                        funDt = serviceForm.funServicesDT;
                    }
                    dt.Rows[i]["F2"] = GetCasket(funDt);
                    continue;
                }
                if (field.Trim().ToUpper().IndexOf("VAULT NAME AND DESCRIPTION") == 0)
                {
                    if (funDt == null)
                    {
                        serviceForm = new FunServices(workContractNumber);
                        funDt = serviceForm.funServicesDT;
                    }
                    dt.Rows[i]["F2"] = GetVault(funDt);
                    continue;
                }
                if (field.Trim().ToUpper().IndexOf("INS COMPANY NAME") == 0)
                {
                    if (payDt == null)
                    {
                        cmd = "Select * from `cust_payments` where `contractNumber` = '" + workContractNumber + "';";
                        payDt = G1.get_db_data(cmd);
                    }
                    dt.Rows[i]["F2"] = GetInsCompanyName(payDt, field );
                    continue;
                }
                if (field.Trim().ToUpper().IndexOf("UNITY OR DIRECT") == 0)
                {
                    if (payDt == null)
                    {
                        cmd = "Select * from `cust_payments` where `contractNumber` = '" + workContractNumber + "';";
                        payDt = G1.get_db_data(cmd);
                    }
                    dt.Rows[i]["F2"] = GetInsFiledThrough (payDt, field);
                    continue;
                }
                if (field.Trim().ToUpper().IndexOf("FILED AMT") == 0)
                {
                    if (payDt == null)
                    {
                        cmd = "Select * from `cust_payments` where `contractNumber` = '" + workContractNumber + "';";
                        payDt = G1.get_db_data(cmd);
                    }
                    dt.Rows[i]["F2"] = GetInsFiledAmount(payDt, field);
                    continue;
                }
                if ( field.ToUpper().IndexOf ( "BRANCH") >= 0 )
                {
                    //dt.Rows[i]["F2"] = "Hello There Funeral Home\nSomewhere in the US";
                }
                if (field.ToUpper().IndexOf ( "AGE") == 0 )
                {
                    dt.Rows[i]["F2"] = GetDecAge(workContractNumber);
                    gotit = true;
                }
                else if ( field.ToUpper() == "AGE/Y")
                {
                    dt.Rows[i]["F2"] = GetDecAge(workContractNumber);
                    gotit = true;
                }
                else if (field.ToUpper() == "AGE/M")
                {
                    dt.Rows[i]["F2"] = GetDecAge(workContractNumber, "M" );
                    gotit = true;
                }
                else if (field.ToUpper() == "AGE/D")
                {
                    dt.Rows[i]["F2"] = GetDecAge(workContractNumber, "D" );
                    gotit = true;
                }
                else if (field.ToUpper() == "DECCITY")
                    dt.Rows[i]["F2"] = GetDecCity(workContractNumber);
                else if (field.ToUpper() == "SSN")
                    dt.Rows[i]["F2"] = GetSSN(workContractNumber);
                else if (field.ToUpper() == "HESHECAP")
                {
                    str = GetDecGender(workContractNumber);
                    if (str.ToUpper() == "M")
                        dt.Rows[i]["F2"] = "He";
                    else
                        dt.Rows[i]["F2"] = "She";
                    dt.Rows[i]["dbfield"] = "";
                }
                else if (field.ToUpper() == "DOD")
                    dt.Rows[i]["F2"] = GetDOD(workContractNumber);
                else if (field.ToUpper() == "DOS")
                    dt.Rows[i]["F2"] = GetDOS(workContractNumber);
                else if (field.ToUpper() == "BRANCH")
                    dt.Rows[i]["F2"] = GetBranch();
                else if (field.ToUpper() == "WEBADDRESS")
                    dt.Rows[i]["F2"] = GetWebAddress();
                else if (field.ToUpper() == "FD")
                    dt.Rows[i]["F2"] = activeFuneralHomeDirector;
                else if (field.ToUpper() == "FA")
                    dt.Rows[i]["F2"] = activeFuneralHomeArranger;
                //else if (field.ToUpper() == "BRANCH")
                //    dt.Rows[i]["F2"] = GetBranch();
                //else if (field.ToUpper() == "BRANCHCTYST")
                //    dt.Rows[i]["F2"] = GetBranchCityState();
                //else if (field.ToUpper() == "FHPHONE")
                //    dt.Rows[i]["F2"] = GetFHPHONE();
                else if (field.ToUpper().IndexOf("PSALM 23") == 0)
                {
                    rtb = new RichTextBoxEx();
                    LoadGeneralForm("PSALMS 23", rtb);
                    dt.Rows[i]["F2"] = rtb.Text;
                    rtb.Clear();
                    rtb.Dispose();
                    rtb = null;
                    continue;
                }
                else if (field.ToUpper().IndexOf("OBIT-") >= 0)
                {
                    int idxx = field.ToUpper().IndexOf("OBIT-");
                    if (idxx >= 0)
                    {
                        cmd = field.Substring(idxx + 5);
                        string tempLocation = formLocation;
                        if (String.IsNullOrWhiteSpace(tempLocation))
                            tempLocation = EditCust.activeFuneralHomeName;
                        str = LoadAndInstallForm(workContractNumber, tempLocation, cmd);
                        dt.Rows[i]["F2"] = str;
                    }
                    continue;
                }
                else if (field.ToUpper().IndexOf("OBIT") == 0)
                {
                    string tempLocation = formLocation;
                    if (String.IsNullOrWhiteSpace(tempLocation))
                        tempLocation = EditCust.activeFuneralHomeName;
                    str = LoadAndInstallForm(workContractNumber, tempLocation, "SMFS Obit");
                    dt.Rows[i]["F2"] = str;
                    continue;
                }
                else if (field.ToUpper() == "PRECEDED")
                    dt.Rows[i]["F2"] = RTF_Stuff.Get_Relatives(workContractNumber, true);
                else if (field.ToUpper() == "PRECEDED1")
                    dt.Rows[i]["F2"] = RTF_Stuff.Get_Relatives(workContractNumber, true, true );
                else if (field.ToUpper() == "PRECEDED4")
                    dt.Rows[i]["F2"] = RTF_Stuff.Get_Relatives(workContractNumber, true, false, true, true );
                else if (field.ToUpper() == "PRECEDED2")
                    dt.Rows[i]["F2"] = RTF_Stuff.Get_RelativesList(field, workContractNumber, true );
                else if (field.ToUpper() == "PRECEDED3")
                    dt.Rows[i]["F2"] = RTF_Stuff.Get_RelativesList(field, workContractNumber, true, true, true );
                else if (field.ToUpper() == "PRECEDED5")
                    dt.Rows[i]["F2"] = RTF_Stuff.Get_RelativesList(field, workContractNumber, true, true, true);
                else if (field.ToUpper() == "SURVLIST")
                    dt.Rows[i]["F2"] = Get_Relatives(workContractNumber, false);
                else if (field.ToUpper() == "SURVLIST1")
                    dt.Rows[i]["F2"] = Get_Relatives(workContractNumber, false, true );
                else if (field.ToUpper() == "SURVLIST4")
                    dt.Rows[i]["F2"] = Get_Relatives(workContractNumber, false, false, true, true );
                else if (field.ToUpper() == "SURVLIST2")
                    dt.Rows[i]["F2"] = Get_RelativesList(field, workContractNumber, false );
                else if (field.ToUpper() == "SURVLIST3")
                    dt.Rows[i]["F2"] = Get_RelativesList(field, workContractNumber, false, true, true );
                else if (field.ToUpper() == "SURVLIST5")
                    dt.Rows[i]["F2"] = Get_RelativesList(field, workContractNumber, false, true, true);
                else if (field.ToUpper().IndexOf("MEMBERS,") >= 0)
                {
                    table = dt.Rows[i]["table"].ObjToString();
                    dt.Rows[i]["F2"] = "";
                    str = ParseOutMembers(table, field);
                    dt.Rows[i]["F2"] = str;
                }
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(table))
                    continue;
                if (String.IsNullOrWhiteSpace(dbfield))
                    continue;
                if ( dbfield.IndexOf ("|") > 0 )
                {
                }
                gotDate = false;
                gotWithout = false;
                qualify = dt.Rows[i]["qualify"].ObjToString();
                if (qualify.ToUpper().IndexOf("$DATE") >= 0)
                    gotDate = true;
                if (qualify.ToUpper().IndexOf("$W/O") >= 0)
                    gotWithout = true;

                data = dt.Rows[i]["F2"].ObjToString();
                if ( field.ToUpper().IndexOf("PRECED") >= 0 || field.ToUpper().IndexOf ("SURV") >= 0 )
                {
                    if (String.IsNullOrWhiteSpace(data))
                        continue;
                }
                if (!String.IsNullOrWhiteSpace(data) && String.IsNullOrWhiteSpace ( qualify ))
                    continue;
                if (!String.IsNullOrWhiteSpace(qualify) && !gotDate && !gotWithout )
                {
                    if (!String.IsNullOrWhiteSpace(field))
                    {
                        string[] Lines = qualify.Split('=');
                        if (Lines.Length == 2)
                        {
                            tableColumn = Lines[0];
                            qualifier = Lines[1];
                            idx = G1.StripNumeric(field);
                            moreOptions = dt.Rows[i]["more_options"].ObjToString();
                            gotMultiple = CheckForMultiple( moreOptions, ref multipleBy);
                            //if (field.ToUpper() == "PBX")
                            //    multipleBy = 2;
                            //if ( gotMultiple )
                            //{
                            //}
                            if (field.ToUpper() == "PBX" || field.ToUpper() == "HPBX" || field.ToUpper() == "CLERGYX" || field.ToUpper() == "MUSICIANX")
                            {
                                data = GetDbFieldAll(table, dbfield, tableColumn, qualifier, workContractNumber, multipleBy );
                            }
                            else
                            {
                                qualifier = qualifier.Replace(idx.ToString(), "");
                                if ( dbfield.IndexOf ( "|") > 0 )
                                {
                                }
                                data = GetDbField(table, dbfield, tableColumn, qualifier, workContractNumber, idx );
                            }

                        }
                    }
                    else
                        data = GetDbField(table, dbfield, qualify, workContractNumber);
                }
                else
                {
                    data = GetDbField(table, dbfield, qualify, workContractNumber);
                    if (dbfield.ToUpper() == "DECCITY")
                        data = GetDecCity(workContractNumber);
                    else
                    {
                        //if (field.ToUpper() == "MEMCONTENT")
                        //{
                        //    file = data.Trim();
                        //    if (!String.IsNullOrWhiteSpace(file))
                        //    {
                        //        rtb = new RichTextBoxEx();
                        //        LoadGeneralForm(file, rtb);
                        //        dt.Rows[i]["F2"] = rtb.Text;
                        //        if (String.IsNullOrWhiteSpace(rtb.Text))
                        //            dt.Rows[i]["F2"] = file;
                        //        rtb.Clear();
                        //        rtb.Dispose();
                        //        rtb = null;
                        //        continue;
                        //    }
                        //}
                    }
                }
                if (G1.validate_numeric(data) && data.IndexOf(".") >= 0)
                {
                    double money = data.ObjToDouble();
                    data = G1.ReformatMoney(money);
                }
                dt.Rows[i]["F2"] = data;
                if (field.ToUpper().IndexOf("PBX") >= 0)
                {
                    //dt.Rows[i]["F1"] = "[*PBX-2*]";
                }
                //if (gotit)
                //{
                //    for (int k = 0; k < dt.Rows.Count; k++)
                //    {
                //        str = dt.Rows[k]["field"].ObjToString().ToUpper();
                //        if (str == "AGE")
                //        {
                //            str = dt.Rows[k]["F2"].ObjToString();
                //            if (String.IsNullOrWhiteSpace(str))
                //            {
                //                break;
                //            }
                //        }
                //    }
                //}
            }
            string answer = "";
            string state = "";
            string abbreviation = "";
            DataTable stateDt = null;
            string form = dt.TableName.Trim().ToUpper();
            if (form == "REGISTER BOOK")
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    data = dt.Rows[i]["field"].ObjToString();
                    if (data.ToUpper().IndexOf("MEMBER") >= 0)
                    {
                        data = dt.Rows[i]["F2"].ObjToString();
                        string[] Lines = data.Split('\n');
                    }
                    else if (data.ToUpper().IndexOf("STATE") >= 0)
                    {
                        answer = dt.Rows[i]["F2"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(answer))
                        {
                            lines = answer.Split(',');
                            for (int j = 0; j < lines.Length; j++)
                            {
                                field = lines[j].Trim();
                                cmd = "Select * from `ref_states` WHERE `state` = '" + field + "';";
                                stateDt = G1.get_db_data(cmd);
                                if (stateDt.Rows.Count > 0)
                                {
                                    abbreviation = stateDt.Rows[0]["abbrev"].ObjToString();
                                    if (!String.IsNullOrWhiteSpace(abbreviation))
                                    {
                                        answer = answer.Replace(field, abbreviation);
                                        dt.Rows[i]["F2"] = answer;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            FinalizeData(dt, workContractNumber );

            return dt;
        }
        /***********************************************************************************************/
        public static string NumberToWords(int number)
        {
            if (number == 0)
                return "";

            if (number < 0)
                return "minus " + NumberToWords(Math.Abs(number));

            string words = "";

            if ((number / 1000000) > 0)
            {
                words += NumberToWords(number / 1000000) + " million ";
                number %= 1000000;
            }

            if ((number / 1000) > 0)
            {
                words += NumberToWords(number / 1000) + " thousand ";
                number %= 1000;
            }

            if ((number / 100) > 0)
            {
                words += NumberToWords(number / 100) + " hundred ";
                number %= 100;
            }

            if (number > 0)
            {
                if (words != "")
                    words += "and ";

                var unitsMap = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine", "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
                var tensMap = new[] { "zero", "ten", "twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety" };

                if (number < 20)
                    words += unitsMap[number];
                else
                {
                    words += tensMap[number / 10];
                    if ((number % 10) > 0)
                        words += "-" + unitsMap[number % 10];
                }
            }

            return words;
        }
        /***********************************************************************************************/
        public static void FinalizeData(DataTable dt, string workContract )
        {
            string table = "";
            string dbfield = "";
            string qualify = "";
            string data = "";
            string cmd = "";
            string field = "";
            string dispositionType = "";
            bool found = false;
            DataRow[] dRows = null;
            string location = "";
            string address = "";
            string deathPlace = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                table = dt.Rows[i]["table"].ObjToString();
                dbfield = dt.Rows[i]["dbfield"].ObjToString();
                if (String.IsNullOrWhiteSpace(table))
                    continue;
                if (String.IsNullOrWhiteSpace(dbfield))
                    continue;
                field = dt.Rows[i]["field"].ObjToString();
                if (field.ToUpper() == "MEMCONTENT" || field == "MEMTITLE")
                    continue;
                if (field == "Disposition Location Name")
                {
                    dRows = dt.Select("field='Disposition Type'");
                    if (dRows.Length > 0)
                    {
                        dispositionType = dRows[0]["F2"].ObjToString();
                        GetDispositionInfo(workContract, dispositionType, ref location, ref address);
                        dt.Rows[i]["F2"] = location;
                        dRows = dt.Select("field='Disposition City State'");
                        if (dRows.Length > 0)
                            dRows[0]["F2"] = address;
                    }
                }
                else if( field.IndexOf ("DeathPlace") >= 0 )
                {
                    deathPlace = dt.Rows[i]["F2"].ObjToString();
                    if ( String.IsNullOrWhiteSpace ( deathPlace))
                    {
                        deathPlace = GetDeathPlaceInfo(workContract);
                        dt.Rows[i]["F2"] = deathPlace;
                    }
                }
                else if (field.IndexOf("POD") >= 0)
                {
                    deathPlace = dt.Rows[i]["F2"].ObjToString();
                    if (String.IsNullOrWhiteSpace(deathPlace))
                    {
                        deathPlace = GetDeathPlaceInfo(workContract);
                        dt.Rows[i]["F2"] = deathPlace;
                    }
                }
            }
        }
        /***********************************************************************************************/
        public static void GetDispositionInfo(string workContract, string dispositionType, ref string location, ref string address)
        {
            location = "";
            address = "";
            if (String.IsNullOrWhiteSpace(dispositionType))
                return;

            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "' ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return;
            if (dispositionType.ToUpper() == "CREMATION")
            {
                location = dx.Rows[0]["crematorium"].ObjToString();
                //address = dx.Rows[0]["CremCityStateZip"].ObjToString();
                address = dx.Rows[0]["cremCity"].ObjToString() + ", " + dx.Rows[0]["cremState"].ObjToString();
            }
            else
            {
                location = dx.Rows[0]["cem"].ObjToString();
                //address = dx.Rows[0]["cemctyst"].ObjToString();
                address = dx.Rows[0]["cemCity"].ObjToString() + ", " + dx.Rows[0]["cemState"].ObjToString();
            }
        }
        /***********************************************************************************************/
        public static string GetDeathPlaceInfo(string workContract )
        {
            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContract + "' ";
            cmd += ";";
            DataTable dx = G1.get_db_data(cmd);
            if (dx.Rows.Count <= 0)
                return "";
            string deathPlace = dx.Rows[0]["Place of Death"].ObjToString();
            if ( String.IsNullOrWhiteSpace ( deathPlace))
                deathPlace = dx.Rows[0]["Place of Death Address"].ObjToString();
            return deathPlace;
        }
        /****************************************************************************************/
        public static bool CheckForMultiple ( string moreOptions, ref int multipleBy )
        {
            bool multiple = false;
            multipleBy = 0;
            if ( !String.IsNullOrWhiteSpace ( moreOptions ) )
            {
                if (moreOptions.ToUpper().IndexOf("BY") >= 0)
                {
                    multiple = true;
                    moreOptions = moreOptions.ToUpper().Replace("BY", "");
                    moreOptions = moreOptions.Trim();
                    multipleBy = moreOptions.ObjToInt32();
                }
            }
            return multiple;
        }
        /****************************************************************************************/
        public static string GetCasket ( DataTable dt )
        {
            string cmd = "Select * from `casket_master`;";
            DataTable dx = G1.get_db_data(cmd);
            string casket = "";
            string service = "";
            string type = "";
            string casketCode = "";
            string str = "";
            string[] Lines = null;
            DataRow[] dR = null;
            for ( int i=0; i<dt.Rows.Count; i++)
            {
                type = dt.Rows[i]["type"].ObjToString();
                if ( type.ToUpper() == "MERCHANDISE")
                {
                    service = dt.Rows[i]["service"].ObjToString();
                    if (String.IsNullOrWhiteSpace(service))
                        continue;
                    Lines = service.Split(' ');
                    if ( Lines.Length > 0 )
                    {
                        casketCode = Lines[0].Trim();
                        str = casketCode.Substring(0, 1).ToUpper();
                        if (str == "V" && casketCode.Length == 3)
                            continue;
                        dR = dx.Select("casketcode='" + casketCode + "'");
                        if ( dR.Length > 0 )
                        {
                            casket = dR[0]["casketdesc"].ObjToString();
                            if (casket.IndexOf(casketCode) < 0)
                                casket = casketCode + " " + casket;
                            break;
                        }
                    }
                }
            }
            return casket;
        }
        /****************************************************************************************/
        public static string GetVault(DataTable dt)
        {
            string cmd = "Select * from `casket_master`;";
            DataTable dx = G1.get_db_data(cmd);
            string casket = "";
            string service = "";
            string type = "";
            string casketCode = "";
            string str = "";
            string[] Lines = null;
            DataRow[] dR = null;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                type = dt.Rows[i]["type"].ObjToString();
                if (type.ToUpper() == "MERCHANDISE")
                {
                    service = dt.Rows[i]["service"].ObjToString();
                    if (String.IsNullOrWhiteSpace(service))
                        continue;
                    dR = dx.Select("casketdesc='" + service + "'");
                    if (dR.Length > 0)
                    {
                        casketCode = dR[0]["casketcode"].ObjToString().Trim();
                        if (!String.IsNullOrWhiteSpace(casketCode))
                        {
                            str = casketCode.Substring(0, 1);
                            if (str == "V" && casketCode.Length == 3)
                            {
                                casket = dR[0]["casketcode"].ObjToString() + " " + service;
                                break;
                            }
                        }
                    }
                }
            }
            return casket;
        }
        /****************************************************************************************/
        public static string GetInsCompanyName(DataTable dt, string field )
        {
            string casket = "";
            string service = "";
            string type = "";
            string companyName = "";
            string str = "";
            string[] Lines = null;
            DataRow[] dR = null;
            string cmd = "";
            string policy = "";
            string description = "";
            string record = "";
            DataTable dx = null;
            int row = 1;
            if (field.ToUpper().IndexOf("NAME2") > 0)
                row = 2;
            int count = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                policy = dt.Rows[i]["trust_policy"].ObjToString();
                if (String.IsNullOrWhiteSpace(policy))
                    continue;
                description = dt.Rows[i]["description"].ObjToString();
                if (description.IndexOf("~") < 0)
                    continue;
                Lines = description.Split('~');
                if (Lines.Length < 3)
                    continue;
                record = Lines[2].Trim();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    count++;
                    if (count < row)
                        continue;
                    cmd = "Select * from `policies` where `record` = '" + record + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0 )
                    {
                        companyName = dx.Rows[0]["companyCode"].ObjToString();
                        break;
                    }
                }
            }
            return companyName;
        }
        /****************************************************************************************/
        public static string GetInsFiledThrough (DataTable dt, string field)
        {
            string casket = "";
            string service = "";
            string type = "";
            string paidfrom = "";
            string str = "";
            string[] Lines = null;
            DataRow[] dR = null;
            string cmd = "";
            string policy = "";
            string description = "";
            string record = "";
            DataTable dx = null;
            int row = 1;
            if (field.ToUpper().IndexOf("DIRECT2") > 0)
                row = 2;
            int count = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                policy = dt.Rows[i]["trust_policy"].ObjToString();
                if (String.IsNullOrWhiteSpace(policy))
                    continue;
                description = dt.Rows[i]["description"].ObjToString();
                if (description.IndexOf("~") < 0)
                    continue;
                Lines = description.Split('~');
                if (Lines.Length < 3)
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    count++;
                    if (count < row)
                        continue;
                    cmd = "Select * from `cust_payment_details` where `paymentRecord` = '" + record + "';";
                    dx = G1.get_db_data(cmd);
                    if (dx.Rows.Count > 0)
                    {
                        paidfrom = dx.Rows[0]["paidFrom"].ObjToString();
                        if (paidfrom.ToUpper() != "UNITY")
                            paidfrom = "Direct";
                        break;
                    }
                }
            }
            return paidfrom;
        }
        /****************************************************************************************/
        public static string GetInsFiledAmount(DataTable dt, string field)
        {
            string casket = "";
            string service = "";
            string type = "";
            string payment = "";
            double pay = 0D;
            string str = "";
            string[] Lines = null;
            DataRow[] dR = null;
            string cmd = "";
            string policy = "";
            string description = "";
            string record = "";
            DataTable dx = null;
            int row = 1;
            if (field.ToUpper().IndexOf("AMT2") > 0)
                row = 2;
            int count = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                policy = dt.Rows[i]["trust_policy"].ObjToString();
                if (String.IsNullOrWhiteSpace(policy))
                    continue;
                description = dt.Rows[i]["description"].ObjToString();
                if (description.IndexOf("~") < 0)
                    continue;
                Lines = description.Split('~');
                if (Lines.Length < 3)
                    continue;
                record = dt.Rows[i]["record"].ObjToString();
                if (!String.IsNullOrWhiteSpace(record))
                {
                    count++;
                    if (count < row)
                        continue;
                    pay = dt.Rows[i]["payment"].ObjToDouble();
                    payment = " $" + G1.ReformatMoney(pay);
                    break;
                }
            }
            return payment;
        }
        /****************************************************************************************/
        public static void LoadGeneralForm(string form, EMRControlLib.RichTextBoxEx rtb)
        {
            string cmd = "Select * from `arrangementforms` where `location` = 'General' AND `formName` = '" + form + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return;
            string record = ddx.Rows[0]["record"].ObjToString();
            string str = G1.get_db_blob("arrangementforms", record, "image");
            if (str.IndexOf("rtf1") > 0)
            {
                rtb.Rtf = str;
            }
        }
        /****************************************************************************************/
        public static string LoadGeneralTitle(string title)
        {
            string cmd = "Select * from `titlecontent`;";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
                return "";
            DataRow[] dR = ddx.Select("title='" + title + "'");
            if (dR.Length <= 0)
                return "";
            string str = dR[0]["content"].ObjToString();
            if (str == null)
                str = "";
            return str;
        }
        /***********************************************************************************************/
        public static void PushFieldsToForm(string contractNumber, DataTable dt, RichEditControl rtbx = null, bool removeEmpty = false)
        {
            string text = rtbx.Document.RtfText;
            string field = "";
            string data = "";
            string qualify = "";
            bool pass = false;
            int i = 0;
            try
            {
                for (i = 0; i < dt.Rows.Count; i++)
                {
                    field = dt.Rows[i]["F1"].ObjToString();
                    if ( field.ToUpper() == "[%GPL ED%]")
                    {
                        dt.Rows[i]["field"] = "GPLED";
                        continue;
                    }
                    else if (field.ToUpper() == "[%CPL ED%]")
                    {
                        dt.Rows[i]["field"] = "CPLED";
                        continue;
                    }
                    else if (field.ToUpper() == "[%OBC ED%]")
                    {
                        dt.Rows[i]["field"] = "OBCED";
                        continue;
                    }
                    //                field = dt.Rows[i]["field"].ObjToString();
                    if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                    {
                        qualify = dt.Rows[i]["qualify"].ObjToString();
                        pass = true;
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("END_TABLE") >= 0)
                    {
                        pass = false;
                        continue;
                    }
                    if (pass)
                        continue;
                    data = dt.Rows[i]["F2"].ObjToString();
                    if (String.IsNullOrWhiteSpace(field))
                        continue;
                    if (String.IsNullOrWhiteSpace(data))
                    {
                        if (removeEmpty)
                            text = ReplaceField(text, field, data);
                        continue;
                    }
                    if (data.ToUpper().IndexOf("MULTI-RTF") >= 0)
                    {
                        continue;
                    }
                    else
                    {
                        if (data.IndexOf("rtf1") >= 0)
                        {
                            text = ReplaceField(text, field, data);
                            //                    rtbx.Document.RtfText = text;
                        }
                        else
                            text = ReplaceField(text, field, data);
                    }
                }
            }
            catch (Exception ex)
            {

            }
//            rtbx.Document.RtfText = text;
            rtbx.Document.RtfText = text;
            string result = RTF_Stuff.ProcessTables(contractNumber, dt, text);
            rtbx.Document.RtfText = result;

        }
        /***********************************************************************************************/
        public static string ReplaceField(string text, string field, string replace)
        {
            if (replace.ToUpper().IndexOf("MULTI-RTF") >= 0)
                return text;
            int idx = -1;
            string str = "";
            if (field.IndexOf("[*") >= 0)
            {
                idx = text.IndexOf(field);
                text = text.Replace(field, replace);
                return text;
            }
            str = field.Substring(0, 1);
            if (str == "$")
            {
                idx = text.IndexOf(field);
                text = text.Replace(field, replace);
                return text;
            }
            StringBuilder sb = new StringBuilder(text);
            StringBuilder xb = new StringBuilder(replace);
            int count = 0;
            for (;;)
            {
                idx = text.IndexOf(field);
                if (idx < 0)
                    break;
                for (int i = idx; i < text.Length; i++)
                {
                    str = text.Substring(i, 1);
                    if (str == "]")
                    {
                        sb[i] = (char)127; // Unprintable Char, Causes Underline to be visible up to this char
                        break;
                    }
                    else
                    {
                        if (count < xb.Length)
                            sb[i] = xb[count];
                        else
                        {
                            sb[i] = ' ';
                        }
                        count++;
                    }
                }
                text = sb.ToString();
                count = 0;
            }
            text = sb.ToString();
            return text;
        }
        /***********************************************************************************************/
        public static string GetDbField(string table, string field, string tableColumn, string qualifier, string contractNumber, int idx = 0, bool removeEmpty = false)
        {
            string data = "";
            string cmd = "";
            string str = "";
            bool gotOr = false;
            if ( field.IndexOf ( "|" ) > 0 )
            {
                gotOr = true;
            }
            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "' ";
                if (!String.IsNullOrWhiteSpace(tableColumn) && !String.IsNullOrWhiteSpace(qualifier))
                    cmd += " and `" + tableColumn + "` = '" + qualifier + "' AND `depRelationship` <> 'DISCLOSURES'";
                if (qualifier.ToUpper() == "PB")
                    cmd += " ORDER BY `PALORDER` ";
                else if (qualifier.ToUpper() == "CLERGY" || qualifier.ToUpper() == "HPB" || qualifier.ToUpper() == "MUSICIAN")
                    cmd += " ORDER BY `ORDER` ";
                cmd += ";";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    string strDelimitor = " +";
                    if (gotOr)
                        strDelimitor = " |";
                    string[] Lines = field.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        field = Lines[i].Trim();
                        try
                        {
                            if ( gotOr )
                            {
                                if ( String.IsNullOrWhiteSpace ( data ))
                                {
                                    if (idx > 0)
                                        str = dx.Rows[idx - 1][field].ObjToString().Trim();
                                    else
                                        str = dx.Rows[0][field].ObjToString().Trim();
                                    if (!String.IsNullOrWhiteSpace(str))
                                    {
                                        data = data.Trim();
                                        data += " " + str;
                                    }
                                }
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
                            if (removeEmpty)
                                data = "";
                        }
                    }
                }
                else
                {
                    if (removeEmpty)
                        data = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + field + " For ContractNumber " + contractNumber + "!!");
            }
            if (tableColumn == "authEmbalming")
            {
            }
            return data;
        }
        /***********************************************************************************************/
        public static string GetDbFieldAll(string table, string field, string tableColumn, string qualifier, string contractNumber, int multiBy, bool removeEmpty = false)
        {
            string data = "";
            string cmd = "";
            string str = "";
            string originalField = field;
            int multiCount = 0;
            RichTextBox rtb = new RichTextBox();
            rtb.AppendText("\n");
            string newline = rtb.Rtf;
            try
            {
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "' ";
                if (!String.IsNullOrWhiteSpace(tableColumn) && !String.IsNullOrWhiteSpace(qualifier))
                    cmd += " and `" + tableColumn + "` = '" + qualifier + "' ";

                if (qualifier.ToUpper() == "PB")
                    cmd += " ORDER BY `PALORDER` ";
                else if (qualifier.ToUpper() == "CLERGY" || qualifier.ToUpper() == "HPB" || qualifier.ToUpper() == "MUSICIAN")
                    cmd += " ORDER BY `ORDER` ";

                cmd += ";";
                DataTable dx = G1.get_db_data(cmd);
                if (dx.Rows.Count > 0)
                {
                    for (int k = 0; k < dx.Rows.Count; k++)
                    {
                        string strDelimitor = " +";
                        string[] Lines = originalField.Split(new[] { strDelimitor }, StringSplitOptions.None);
                        for (int i = 0; i < Lines.Length; i++)
                        {
                            field = Lines[i].Trim();
                            try
                            {
                                if (field.Trim() != "+")
                                {
                                    str = dx.Rows[k][field].ObjToString().Trim();
                                    data = data.Trim();
                                    data += " " + str;
                                }
                            }
                            catch
                            {
                                if (removeEmpty)
                                    data = "";
                            }
                        }
                        if (dx.Rows.Count > 1)
                        {
                            if (multiBy > 0)
                            {
                                multiCount++;
                                if ((multiCount % multiBy) == 0)
                                {
                                    rtb.AppendText(data + "\n");
                                    data = "";
                                }
                                else
                                {
                                    data += " - ";
                                    //data += "        ";
                                }
                            }
                            else
                            {
                                if (dx.Rows.Count > 2)
                                    data = data.Trim() + ", ";
                                if (k == (dx.Rows.Count - 2))
                                    data += "and ";
                            }
                        }
                    }
                }
                else
                {
                    if (removeEmpty)
                        data = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + field + " For ContractNumber " + contractNumber + "!!");
            }
            data = data.Trim();
            data = data.TrimEnd(',');
            if (multiBy > 0)
            {
                if (!String.IsNullOrWhiteSpace(data))
                    rtb.AppendText(data);
                rtb.SelectAll();
                rtb.SelectionAlignment = HorizontalAlignment.Center;
                data = "Multi-RTF" + rtb.Text;
            }
            return data;
        }
        /***********************************************************************************************/
        public static string GetDbField(string table, string dbfield, string qualify, string contractNumber)
        {
            string data = "";
            string cmd = "";
            string str = "";
            int idx = -1;
            bool gotOr = false;
            if (dbfield.IndexOf("|") > 0)
                gotOr = true;
            if ( qualify.ToUpper() == "$W/O LICENSE")
            {
            }
            if (table.ToUpper() == "RELATIVES")
            {

            }
            if (table.ToUpper() == "CUSTOMERS")
                table = "fcustomers";
            else if (table.ToUpper() == "CONTRACTS")
                table = "fcontracts";
            else if (table.ToUpper() == "CUST_EXTENDED")
                table = "fcust_extended";

            try
            {
                string myField = "";
                if (table.ToUpper() == "XXXX")
                    return "";
                cmd = "Select * from `" + table + "` where `contractNumber` = '" + contractNumber + "';";
                if ( table.ToUpper() == "FUNERALHOMES")
                    cmd = "Select * from `funeralhomes` where `atneedcode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
                DataTable dx = G1.get_db_data(cmd);
                if ( dx.Rows.Count <= 0 && table == "fcustomers")
                {
                    cmd = "Select * from `customers` where `contractNumber` = '" + contractNumber + "';";
                    dx = G1.get_db_data(cmd);
                }
                if (dx.Rows.Count > 0)
                {
                    string strDelimitor = "+";
                    if (gotOr)
                        strDelimitor = "|";
                    string[] Lines = dbfield.Split(new[] { strDelimitor }, StringSplitOptions.None);
                    for (int i = 0; i < Lines.Length; i++)
                    {
                        myField = Lines[i].Trim();
                        if (myField.ToUpper() == "MEMCONTENT" || myField.ToUpper() == "MEMTITLE")
                            continue;
                        try
                        {
                            if ( gotOr )
                            {
                                str = dx.Rows[0][myField].ObjToString().Trim();
                                if (!String.IsNullOrWhiteSpace(str))
                                {
                                    if (String.IsNullOrWhiteSpace(data))
                                        data += str;
                                }
                            }
                            else if (myField.Trim() != "+")
                            {
                                if (myField.IndexOf("\"") >= 0)
                                {
                                    string extraField = myField.Replace("\"", "");
                                    data += extraField;
                                }
                                else
                                {
                                    str = dx.Rows[0][myField].ObjToString().Trim();
                                    if (str.ToUpper() == "FEMALE")
                                        str = "Female";
                                    else if (str.ToUpper() == "MALE")
                                        str = "Male";
                                    data = data.Trim();
                                    if (qualify.ToUpper() == "$DATE=MONTH" && G1.validate_date(str))
                                        str = str.ObjToDateTime().ToString("MMMMMMMMMMMMM");
                                    else if (qualify.ToUpper() == "$DATE=DAY" && G1.validate_date(str))
                                        str = str.ObjToDateTime().Day.ToString();
                                    else if (qualify.ToUpper() == "$DATE=DOW" && G1.validate_date(str))
                                        str = G1.DayOfWeekText(str.ObjToDateTime());
                                    else if (qualify.ToUpper() == "$DATE=YEAR" && G1.validate_date(str))
                                        str = str.ObjToDateTime().Year.ToString();
                                    else if (qualify.ToUpper() == "$DATE=MM,DD,YYYY" && G1.validate_date(str))
                                        str = str.ObjToDateTime().ToString("MM,dd,yyyy");
                                    else if (qualify.ToUpper() == "$DATE=NOW")
                                        str = DateTime.Now.ToString("MM/dd/yyyy");
                                    else if (qualify.ToUpper() == "$DATE=FULL" && G1.validate_date(str))
                                        str = str.ObjToDateTime().ToString("MMMMMMMMMMMMM") + " " + str.ObjToDateTime().ToString("d, yyyy");
                                    else if (qualify.ToUpper() == "$W/O LICENSE")
                                    {
                                        idx = str.IndexOf("[");
                                        if (idx > 0)
                                        {
                                            str = str.Substring(0, idx);
                                            str = str.Trim();
                                        }
                                    }

                                    if ((myField.ToUpper() == "SUFFIX" || myField.ToUpper() == "DEPSUFFIX") && !String.IsNullOrWhiteSpace(str))
                                        data = data + ", " + str;
                                    else
                                        data += " " + str;
                                }
                            }
                        }
                        catch
                        {
                            data += myField;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("***ERROR*** Looking up Table " + table + " Field " + dbfield + " For ContractNumber " + contractNumber + "!!");
            }
            if ( data.IndexOf ( "his Residence") >= 0 )
                data = data.Replace("his Residence", "his residence");
            else if (data.IndexOf("her Residence") >= 0)
                data = data.Replace("her Residence", "her residence");
            if (dbfield.ToUpper() == "SSN")
                data = FunCustomer.FixSSN(data);
            return data;
        }
        /***********************************************************************************************/
        public static string GetDecAge(string workContractNumber, string option = "" )
        {
            string rv = "";
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime dob = dt.Rows[0]["birthDate"].ObjToDateTime();
                DateTime dod = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                if (dod.Year < 1875)
                    dod = DateTime.Now;
                int age = G1.GetAge(dob, dod);
                rv = age.ObjToString();
                if ( option.ToUpper() == "M" )
                {
                    int months = 0;
                    int days = 0;
                    G1.CalculateYourAge(dob, dod, ref age, ref months, ref days);
                    rv = months.ObjToString();
                }
                else if (option.ToUpper() == "D")
                {
                    int months = 0;
                    int days = 0;
                    G1.CalculateYourAge(dob, dod, ref age, ref months, ref days);
                    rv = days.ObjToString();
                }
            }
            return rv;
        }
        /***********************************************************************************************/
        public static string GetDecCity(string workContractNumber)
        {
            string rv = "";
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                rv = dt.Rows[0]["city"].ObjToString();
            return rv;
        }
        /***********************************************************************************************/
        public static string GetSSN(string workContractNumber)
        {
            string rv = "";
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                rv = dt.Rows[0]["ssn"].ObjToString();
            return rv;
        }
        /***********************************************************************************************/
        public static string GetGPLED(string workContractNumber)
        {
            string rv = "";
            string gplGroup = EditCust.activeFuneralHomeGroup;
            string cmd = "Select * from `effectivedates` where `category` = 'General Price List' AND `gpl_cpl` = '" + gplGroup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                rv = dt.Rows[0]["effectiveDate"].ObjToDateTime().ToString("MM/dd/yyyy");
            return rv;
        }
        /***********************************************************************************************/
        public static string GetCPLED(string workContractNumber)
        {
            string rv = "";
            string gplGroup = EditCust.activeFuneralHomeCasketGroup;
            string cmd = "Select * from `effectivedates` where `category` = 'Casket Price List' AND `gpl_cpl` = '" + gplGroup + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                rv = dt.Rows[0]["effectiveDate"].ObjToDateTime().ToString("MM/dd/yyyy");
            return rv;
        }
        /***********************************************************************************************/
        public static string GetOBCED(string workContractNumber)
        {
            string rv = "";
            string cmd = "Select * from `effectivedates` where `category` = 'Outer Burial Container Price List';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
                rv = dt.Rows[0]["effectiveDate"].ObjToDateTime().ToString("MM/dd/yyyy");
            return rv;
        }
        /***********************************************************************************************/
        public static string GetDOD(string workContractNumber)
        {
            string rv = "";
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime dod = dt.Rows[0]["deceasedDate"].ObjToDateTime();
                if (dod.Year > 1800)
                    rv = dod.ToString("MM/dd/yyyy");
            }
            return rv;
        }
        /***********************************************************************************************/
        public static string GetDOS(string workContractNumber)
        {
            string rv = "";
            string cmd = "Select * from `fcust_extended` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                DateTime dos = dt.Rows[0]["serviceDate"].ObjToDateTime();
                if (dos.Year > 1800)
                    rv = dos.ToString("MM/dd/yyyy");
            }
            return rv;
        }
        /***********************************************************************************************/
        public static string GetWebAddress()
        {
            string webaddress = "";
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";
            string activeFuneralHomeWeb = dt.Rows[0]["webaddress"].ObjToString();
            if (!String.IsNullOrWhiteSpace(activeFuneralHomeWeb))
                webaddress = activeFuneralHomeWeb;
            return webaddress;
        }
        /***********************************************************************************************/
        public static string GetBranch()
        {
            string branch = "";
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";
            string activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            if (!String.IsNullOrWhiteSpace(activeFuneralHomeName))
                branch = activeFuneralHomeName;
            return branch;
        }
        /***********************************************************************************************/
        public static string GetBranchCityState()
        {
            string branchcitystate = "";
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";
            string activeFuneralHomeName = dt.Rows[0]["name"].ObjToString();
            string city = dt.Rows[0]["city"].ObjToString();
            string state = dt.Rows[0]["state"].ObjToString();
            string zip = dt.Rows[0]["zip"].ObjToString();
            branchcitystate = city + ", " + state;
            if (String.IsNullOrWhiteSpace(city) && String.IsNullOrWhiteSpace(state))
                branchcitystate = "";
            return branchcitystate;
        }
        /***********************************************************************************************/
        public static string GetFHPHONE()
        {
            string cmd = "Select * from `funeralhomes` where `keycode` = '" + LoginForm.activeFuneralHomeKeyCode + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count <= 0)
                return "";
            string fhphone = dt.Rows[0]["phoneNumber"].ObjToString();
            return fhphone;
        }
        /***********************************************************************************************/
        public static string ParseOutMembers(string table, string field)
        {
            string data = "";
            string[] Lines = field.Split('~');
            string query = "";
            string[] LLines = Lines[1].Split(',');
            for (int kk = 0; kk < LLines.Length; kk++)
                query += "'" + LLines[kk].Trim() + "',";
            query = query.TrimEnd(',');
            string cmd = "Select * from `" + table + "` where `depRelationship` IN (" + query + ")";
            DataTable rDt = G1.get_db_data(cmd);
            for (int i = 0; i < rDt.Rows.Count; i++)
                data += "Member " + i.ToString() + "\n";
            data = data.TrimEnd('\n');
            return data;
        }
        /***********************************************************************************************/
        public static string LoadAndInstallForm(string workContractNumber, string formLocation, string formName, bool removeEmpty = false )
        {
            string rv = "";
            string record = "";
            string str = "";
            string cmd = "Select * from `agreements` where `contractNumber` = '" + workContractNumber + "' and `formName` = '" + formName + "';";
            DataTable ddx = G1.get_db_data(cmd);
            if (ddx.Rows.Count <= 0)
            {
                cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "' AND `location` = '" + formLocation + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                {
                    cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "' AND `location` = '';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count <= 0)
                        return rv;
                    record = ddx.Rows[0]["record"].ObjToString();
                    str = G1.get_db_blob("arrangementForms", record, "image");
                }
                else
                {
                    record = ddx.Rows[0]["record"].ObjToString();
                    str = G1.get_db_blob("arrangementForms", record, "image");
                }
            }
            else
            {
                record = ddx.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob("agreements", record, "image");
            }
            if (String.IsNullOrWhiteSpace(str))
            {
                cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "' AND `location` = '" + formLocation + "';";
                ddx = G1.get_db_data(cmd);
                if (ddx.Rows.Count <= 0)
                {
                    cmd = "Select * from `arrangementForms` where `formName` = '" + formName + "' AND `location` = '';";
                    ddx = G1.get_db_data(cmd);
                    if (ddx.Rows.Count <= 0)
                        return rv;
                }
                record = ddx.Rows[0]["record"].ObjToString();
                str = G1.get_db_blob("arrangementForms", record, "image");
                if (String.IsNullOrWhiteSpace(str))
                    return rv;
            }
            if (str.IndexOf("rtf1") > 0)
            {
                byte[] b = Encoding.UTF8.GetBytes(str);
                MemoryStream stream = new MemoryStream(b);
                DevExpress.XtraRichEdit.RichEditControl rtbx = new RichEditControl();
                rtbx.Document.Delete(rtbx.Document.Range);
                rtbx.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                string tt = rtbx.Document.Text;
                DataTable dx = RTF_Stuff.ExtractFields(rtbx.Document.RtfText);
                RTF_Stuff.LoadFields(workContractNumber, dx, rtbx, formLocation, formName, removeEmpty);
                LoadDbFields(workContractNumber, formLocation, dx);
                PushFieldsToForm(workContractNumber, dx, rtbx, removeEmpty);
                tt = rtbx.Document.Text;
                rv = rtbx.Document.RtfText;
            }
            return rv;
        }
        /***********************************************************************************************/
        public static string ProcessTables(string workContractNumber, DataTable ddx, string rtbText )
        {
            string str = "";
            string part3 = "";
            string whatRelationship = "";
            string match = "";
            //if (rtbx == null)
            //    rtbx = rtb;

            //if (1 == 1)
            //    return;

            extractDt = extractData( ref rtbText);

            string tt = rtbText;

            ddx.Columns.Add("detail");
            ddx.Columns.Add("match");
            ddx.Columns.Add("relationship");
            int i = 0;
            for (i = 0; i < extractDt.Rows.Count; i++)
            {
                str = extractDt.Rows[i]["field"].ObjToString();
                if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                {
                    str = str.Replace("[%", "");
                    str = str.Replace("%]", "");
                    //str = str.Replace("[*", "");
                    //str = str.Replace("*]", "");
                    str = str.Replace("\'94", "");
                    str = str.Replace("\'96", "");
                    str = str.Replace("\\u8221", "");
                    str = str.Replace("\\", "");
                    DataRow[] dRows = ddx.Select("field LIKE'%" + str + "%'");
                    if (dRows.Length > 0)
                    {
                        str = extractDt.Rows[i]["detail"].ObjToString();
                        dRows[0]["detail"] = extractDt.Rows[i]["detail"].ObjToString();
                        dRows[0]["match"] = extractDt.Rows[i]["match"].ObjToString();
                        dRows[0]["relationship"] = extractDt.Rows[i]["relationship"].ObjToString();
                    }
                }
            }
            string text = rtbText;
            tt = rtbText;
            part3 = "";
            string part4 = "";
            string allParts = "";
            string field = "";
            string dbField = "";
            string table = "";
            DataTable relationDt = null;
            string cmd = "";
            bool pass = false;
            string data = "";
            string answer = "";
            DateTime deceasedDate = DateTime.Now;
            bool workTable = false;

            G1.NumberDataTable(ddx);
            try
            {
                for (i = 0; i < ddx.Rows.Count; i++)
                {
                    if (i == 19)
                    {

                    }
                    workTable = false;
                    field = ddx.Rows[i]["F1"].ObjToString();
                    if (field == "[*BIOGRAPHY*]")
                    {
                        rtbText = ProcessBiography(workContractNumber, rtbText);
                        continue;
                    }
                    table = ddx.Rows[i]["table"].ObjToString();
                    if (table.ToUpper() == "XXXX")
                        continue;
                    if (String.IsNullOrWhiteSpace(table))
                        continue;
                    field = ddx.Rows[i]["field"].ObjToString();
                    if (pass)
                    {
                        if (field.ToUpper().IndexOf("END_TABLE") >= 0)
                            continue;
                        if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                            pass = false;
                        else
                            continue;
                    }
                    if (field.ToUpper().IndexOf("BEGIN_TABLE:~") >= 0)
                        workTable = true;
                    if (field.ToUpper().IndexOf("BEGIN_TABLE:DECEASED~") >= 0)
                        workTable = true;
                    if (workTable)
                    {
                        data = "";
                        bool deceased = false;
                        bool addParens = false;
                        if (field.ToUpper().IndexOf("DECEASED") >= 0)
                            deceased = true;
                        part4 = ddx.Rows[i]["detail"].ObjToString();
                        match = ddx.Rows[i]["match"].ObjToString();
                        relationDt = ParseOutTableMembers(workContractNumber, table, field, deceased );

                        relationDt = KillDuplicates(relationDt);

                        allParts = "";
                        if (table.ToUpper() == "CUST_EXTENDED")
                        {
                            string f = "";
                            part3 = part4;
                            field = ddx.Rows[i]["field"].ObjToString();
                            field = field.Replace("BEGIN_TABLE:~", "");
                            field = field.Replace("~END_TABLE", "");
                            string[] Lines = field.Split('~');
                            int start = 1;
                            int last = 2;
                            if (field.ToUpper().IndexOf("VISIT") >= 0)
                            {
                                if (field.ToUpper().IndexOf("VISIT1") >= 0)
                                    last = 1;
                                if (field.ToUpper().IndexOf("VISIT2") >= 0)
                                    start = 2;
                                for (int k = start; k <= last; k++)
                                {
                                    for (int kk = 0; kk < Lines.Length; kk++)
                                    {
                                        f = Lines[kk].Trim().ToUpper();
                                        try
                                        {
                                            if (f == "LOCATION")
                                            {
                                                answer = relationDt.Rows[0]["VIS" + k.ToString() + "LOC"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer += ", ";
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "ADDRESS")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["VST" + k.ToString() + "ADD"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["VIS" + k.ToString() + "ADD"].ObjToString();
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "DAY")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["VSTDAYDATE"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["VIS" + k.ToString() + "DAYDATE"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer = " on " + answer + ", ";
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "TYPE")
                                            {
                                                answer = relationDt.Rows[0]["VIS" + k.ToString() + "TYPE"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer = " (" + answer + ")";
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "DATE")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["VSTDATE"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["VIS" + k.ToString() + "DATE"].ObjToString();
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "START")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["VSTSTART"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["VIS" + k.ToString() + "TIMESTART"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer = "starting at " + answer;
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "STOP")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["VSTEND"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["VIS" + k.ToString() + "TIMESTOP"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer = "until " + answer;
                                                data = AddToData(data, answer);
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                            }
                            if (field.ToUpper().IndexOf("SERVICE") >= 0)
                            {
                                if (field.ToUpper().IndexOf("SERVICE1") >= 0)
                                    last = 1;
                                if (field.ToUpper().IndexOf("SERVICE2") >= 0)
                                    start = 2;
                                for (int k = start; k <= last; k++)
                                {
                                    for (int kk = 0; kk < Lines.Length; kk++)
                                    {
                                        f = Lines[kk].Trim().ToUpper();
                                        try
                                        {
                                            if (f == "LOCATION")
                                            {
                                                if ( k == 1)
                                                    answer = relationDt.Rows[0]["SRVLOC"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["SRV" + k.ToString() + "LOC"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer += ", ";
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "ADDRESS")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["SRVCITY"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["SRV" + k.ToString() + "CITY"].ObjToString();
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "DAY")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["SRVDAYDATE"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["SRV" + k.ToString() + "DAYDATE"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer = " on " + answer + ", ";
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "TYPE")
                                            {
                                                if ( k == 1)
                                                    answer = relationDt.Rows[0]["SRVTYPE"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["SRV" + k.ToString() + "TYPE"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer = " (" + answer + ")";
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "DATE")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["SRVDATE"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["SRV" + k.ToString() + "DATE"].ObjToString();
                                                data = AddToData(data, answer);
                                            }
                                            if (f == "START")
                                            {
                                                if (k == 1)
                                                    answer = relationDt.Rows[0]["SRVTIME"].ObjToString();
                                                else
                                                    answer = relationDt.Rows[0]["SRV" + k.ToString() + "TIME"].ObjToString();
                                                if (!String.IsNullOrWhiteSpace(answer))
                                                    answer = "starting at " + answer;
                                                data = AddToData(data, answer);
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                            }
                            part3 = part3.Replace(field, data);

                            part3 = part3.Replace("[%BEGIN_TABLE:~", "");
                            part3 = part3.Replace("[%", "");
                            part3 = part3.Replace("%]", "");
                            part3 = part3.Replace("[*", "");
                            part3 = part3.Replace("*]", "");
                            part3 = part3.Replace("END_TABLE", "");

                            allParts += part3;
                            part3 = part4;
                        }
                        else
                        {
                            for (int k = 0; k < relationDt.Rows.Count; k++)
                            {
                                addParens = false;
                                part3 = part4;
                                 field = ddx.Rows[i]["field"].ObjToString();
                                string[] Lines = field.Split('~');
                                //deceasedDate = relationDt.Rows[k]["depDOD"].ObjToDateTime();
                                //if (deceased)
                                //{
                                //    if (deceasedDate.Year < 1500)
                                //        continue;
                                //}
                                //else
                                //{
                                //    if (deceasedDate.Year > 1500)
                                //        continue;
                                //}
                                str = Lines[2].Trim();
                                Lines = str.Split(' ');
                                for (int j = 0; j < Lines.Length; j++)
                                {
                                    addParens = false;
                                    str = Lines[j].Trim();
                                    if (str.ToUpper() == "FIRSTNAME")
                                        str = "depFirstName";
                                    else if (str.ToUpper() == "LASTNAME")
                                        str = "depLastName";
                                    else if (str.ToUpper() == "MAIDEN")
                                    {
                                        str = "maidenName";
                                        addParens = true;
                                    }
                                    else if (str.ToUpper() == "MI")
                                        str = "depMI";
                                    else if (str.ToUpper() == "SUFFIX")
                                        str = "depSuffix";
                                    else if (str.ToUpper() == "PREFIX")
                                        str = "depPrefix";
                                    else if (str.ToUpper() == "DOB")
                                        str = "depDOB";
                                    else if (str.ToUpper() == "DOD")
                                        str = "depDOD";
                                    else if (str.ToUpper() == "RELATIONSHIP")
                                        str = "depRelationShip";
                                    else if (str.ToUpper() == "WIFE")
                                    {
                                        str = "spouseFirstName";
                                        addParens = true;
                                    }
                                    else if (str.ToUpper() == "SPOUSE")
                                    {
                                        str = "spouseFirstName";
                                        addParens = true;
                                    }
                                    dbField = str;
                                    if ( dbField.ToUpper() == "ZIP")
                                    {

                                    }
                                    data = "";
                                    try
                                    {
                                        if (G1.get_column_number(relationDt, dbField.Trim()) >= 0)
                                        {
                                            data = relationDt.Rows[k][dbField.Trim()].ObjToString();
                                            if (!String.IsNullOrWhiteSpace(data) && addParens)
                                                data = "(" + data + ")";
                                            Lines[j] = data;
                                            addParens = false;
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                data = "";
                                for (int j = 0; j < Lines.Length; j++)
                                {
                                    data += Lines[j].Trim() + " ";
                                }
                                data = data.TrimEnd(' ');
                                field = ddx.Rows[i]["field"].ObjToString();
                                field = field.Replace("~END_TABLE", "");
                                part3 = part3.Replace(field, data);

                                part3 = part3.Replace("[%", "");
                                part3 = part3.Replace("%]", "");
                                part3 = part3.Replace("[*", "");
                                part3 = part3.Replace("*]", "");
                                part3 = part3.Replace("END_TABLE", "");

                                allParts += part3;
                                part3 = part4;
                            }
                        }
//                        text = rtbx.Document.RtfText;
                        text = rtbText;
                        if (!String.IsNullOrWhiteSpace(match))
                        {
                            text = ReplaceField(text, match, allParts);
//                            rtbx.Document.RtfText = text;
                            rtbText = text;
                        }
                        allParts = "";
                        continue;
                    }
                    else if (field.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                    {
                        table = ddx.Rows[i]["table"].ObjToString();
                        if (table.ToUpper() == "XXXX")
                            continue;
                        part4 = ddx.Rows[i]["detail"].ObjToString();
                        match = ddx.Rows[i]["match"].ObjToString();
                        whatRelationship = ddx.Rows[i]["relationship"].ObjToString();
                        if (String.IsNullOrWhiteSpace(table) || String.IsNullOrWhiteSpace(whatRelationship))
                            continue;
                        //part4 = ddx.Rows[i]["search"].ObjToString();
                        allParts = "";
                        try
                        {
                            cmd = "Select * from `" + table + "` where `contractNumber` = '" + workContractNumber + "' ";
                            if (whatRelationship.ToUpper().IndexOf("ALL") < 0)
                                cmd += " and `depRelationship` IN " + whatRelationship + " ";
                            cmd += ";";
                            relationDt = G1.get_db_data(cmd);
                            for (int k = 0; k < relationDt.Rows.Count; k++)
                            {
                                part3 = part4;
                                field = ddx.Rows[i]["field"].ObjToString();
                                string[] Lines = field.Split('~');
                                bool deceased = false;
                                for (int j = 0; j < Lines.Length; j++)
                                {
                                    str = Lines[j].Trim();
                                    if (str.ToUpper().IndexOf("DECEASED=YES") >= 0)
                                        deceased = true;
                                }
                                deceasedDate = relationDt.Rows[k]["depDOD"].ObjToDateTime();
                                if (deceased)
                                {
                                    if (deceasedDate.Year < 1500)
                                        continue;
                                }
                                else
                                {
                                    if (deceasedDate.Year > 1500)
                                        continue;
                                }
                                for (int j = 0; j < Lines.Length; j++)
                                {
                                    str = Lines[j].Trim();
                                    if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                                        continue;
                                    str = str.Replace("R:", "");
                                    if (str.ToUpper() == "FIRST_NAME")
                                        str = "depFirstName";
                                    else if (str.ToUpper() == "LAST_NAME")
                                        str = "depLastName";
                                    else if (str.ToUpper() == "MI")
                                        str = "depMI";
                                    else if (str.ToUpper() == "PREFIX")
                                        str = "depPrefix";
                                    else if (str.ToUpper() == "SUFFIX")
                                        str = "depSuffix";
                                    else if (str.ToUpper() == "DOB")
                                        str = "depDOB";
                                    else if (str.ToUpper() == "DOD")
                                        str = "depDOD";
                                    else if (str.ToUpper() == "RELATIONSHIP")
                                        str = "depRelationShip";
                                    else if (str.ToUpper() == "SPOUSE_FIRST_NAME")
                                        str = "spouseFirstName";
                                    dbField = str;
                                    data = "";
                                    try
                                    {
                                        data = relationDt.Rows[k][dbField].ObjToString();
                                        part3 = ReplaceField(part3, Lines[j], data);
                                    }
                                    catch
                                    {
                                    }
                                    str = Lines[j].Trim();
                                    part3 = part3.Replace(str, "");
                                }
                                part3 = part3.Replace("[%", "");
                                part3 = part3.Replace("%]", "");
                                part3 = part3.Replace("[*", "");
                                part3 = part3.Replace("*]", "");

                                allParts += part3;
                                part3 = part4;
                            }
//                            text = rtbx.Document.RtfText;
                            text = rtbText;
                            text = ReplaceField(text, match, allParts);
//                            rtbx.Document.RtfText = text;
                            rtbText = text;
                            allParts = "";
                        }
                        catch (Exception ex)
                        {
                        }
                        //pass = true;
                        part3 = "";
                        continue;
                    }
                    if (field.ToUpper().IndexOf("END_TABLE") >= 0)
                    {
                        //text = rtb.Document.RtfText;
                        //text = ReplaceField(text, match, part3);
                        //rtb.Document.RtfText = text;
                        part3 = "";
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            allParts = "";
//            text = rtbx.Document.RtfText;
            text = rtbText;
            for (i = 1; i <= MatchCount; i++)
            {
                match = "$match" + i.ToString() + "$";
                text = ReplaceField(text, match, allParts);
            }
            //rtbx.Document.RtfText = text;
            //tt = rtbx.Document.Text;
            return text;
        }
        /***********************************************************************************************/
        public static DataTable KillDuplicates(DataTable dt)
        {
            if (dt.Rows.Count <= 0)
                return dt;
            //if (1 == 1)
            //    return dt;
            DataView tempview = dt.DefaultView;
            tempview.Sort = "depLastName asc, depFirstName asc, depMI asc, depSuffix asc";
            dt = tempview.ToTable();
            string lastName = "";
            string firstName = "";
            string mi = "";
            string suffix = "";
            string checkRecord = "";

            string lastLastName = "";
            string lastFirstName = "";
            string lastMi = "";
            string lastSuffix = "";

            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                lastName = dt.Rows[i]["depLastName"].ObjToString();
                firstName = dt.Rows[i]["depFirstName"].ObjToString();
                mi = dt.Rows[i]["depFirstName"].ObjToString();
                suffix = dt.Rows[i]["depFirstName"].ObjToString();

                if (String.IsNullOrWhiteSpace(lastName))
                    dt.Rows[i]["checkRecord"] = "D";
                else
                {
                    if (lastName == lastLastName && firstName == lastFirstName && mi == lastMi && suffix == lastSuffix )
                        dt.Rows[i]["checkRecord"] = "D";
                    lastLastName = lastName;
                    lastFirstName = firstName;
                    lastMi = mi;
                    lastSuffix = suffix;
                }
            }
            for ( int i=(dt.Rows.Count - 1); i>=0; i--)
            {
                checkRecord = dt.Rows[i]["checkRecord"].ObjToString();
                if (checkRecord == "D")
                    dt.Rows.RemoveAt(i);
            }
            return dt;
        }
        /***********************************************************************************************/
        public static string AddToData ( string data, string answer)
        {
            if (String.IsNullOrWhiteSpace(answer))
                return data;
            if (!String.IsNullOrWhiteSpace(data))
                data += " ";
            data += answer;
            return data;

        }
        /***********************************************************************************************/
        public static int MatchCount = 0;
        public static DataTable extractDt = null;
        /***********************************************************************************************/
        public static DataTable extractData(ref string text )
        {
            //if (rtbx == null)
            //    rtbx = rtb;
            int idx = 0;
            string str = "";
            string part1 = "";
            string part2 = "";
            string part3 = "";
            int startPosition = -1;
            int stopPosition = -1;
            string whatTable = "";
            string match = "";
            MatchCount = 0;
//            string text = rtbText;
            extractDt = ParseRTF(text, "[%");

            extractDt.Columns.Add("relationship");
            extractDt.Columns.Add("match");
            extractDt.Columns.Add("detail");
            int count = 1;
            bool workTable = false;
            bool workDeceased = false;
            for (int i = extractDt.Rows.Count - 1; i >= 0; i--)
            {
                workTable = false;
                str = extractDt.Rows[i]["field"].ObjToString();
                if (str.ToUpper().IndexOf("BEGIN_TABLE:~") >= 0)
                    workTable = true;
                if (str.ToUpper().IndexOf("BEGIN_TABLE:DECEASED~") >= 0)
                {
                    workTable = true;
                    workDeceased = true;
                }
                if (workTable)
                {
                    startPosition = extractDt.Rows[i]["position"].ObjToInt32();
                    match = "$match" + count.ToString() + "$";
                    extractDt.Rows[i]["match"] = match;
                    part1 = text.Substring(0, startPosition);
                    part1 += match;
                    part2 = text.Substring(stopPosition);
                    startPosition += str.Length;
                    //                        part2 = text.Substring(startPosition);

                    idx = text.IndexOf(str);
                    part3 = text.Substring(idx, stopPosition - idx);
                    //                    part3 = text.Substring(startPosition, stopPosition - startPosition);
                    extractDt.Rows[i]["detail"] = part3;
                    text = part1 + part2;
                    count++;
                }
                else if (str.ToUpper().IndexOf("BEGIN_TABLE") >= 0)
                {
                    whatTable = "";
                    startPosition = extractDt.Rows[i]["position"].ObjToInt32();
                    if (str.ToUpper().IndexOf("RELATIONSHIP") >= 0)
                    {
                        whatTable = parseRelationship(str);
                        extractDt.Rows[i]["relationship"] = whatTable;
                        match = "$match" + count.ToString() + "$";
                        extractDt.Rows[i]["match"] = match;
                        part1 = text.Substring(0, startPosition);
                        part1 += match;
                        part2 = text.Substring(stopPosition);
                        startPosition += str.Length;
                        //                        part2 = text.Substring(startPosition);
                        part3 = text.Substring(startPosition, stopPosition - startPosition);
                        extractDt.Rows[i]["detail"] = part3;
                        text = part1 + part2;
                        whatTable = "";
                        count++;
                    }
                }
                else if (str.ToUpper().IndexOf("END_TABLE") >= 0)
                {
                    stopPosition = extractDt.Rows[i]["position"].ObjToInt32();
                    stopPosition += str.Length;
                    startPosition = -1;
                }
            }
//            rtbx.Document.RtfText = text;
            MatchCount = count;
            return extractDt;
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
            bool found = false;
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
                found = false;
                for (int i = idx; i < text.Length; i++)
                {
                    str = text.Substring(i, 1);
                    if (str == "]")
                    {
                        found = true;
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
                if (!found)
                    break;
            }
            return dt;
        }
        /***********************************************************************************************/
        public static string ProcessBiography(string workContractNumber, string rtbText )
        {
            //if (rtbx == null)
            //    rtbx = rtb;
            string cmd = "Select * from `agreements` where `formName` = 'Biography' AND `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string record = dt.Rows[0]["record"].ObjToString();
                string str = G1.get_db_blob("agreements", record, "image");
                if (str.IndexOf("rtf1") > 0)
                {
                    byte[] bytes = Encoding.ASCII.GetBytes(str);

                    MemoryStream stream = new MemoryStream(bytes);

                    DevExpress.XtraRichEdit.RichEditControl bioRTB = new DevExpress.XtraRichEdit.RichEditControl();
                    bioRTB.Document.LoadDocument(stream, DevExpress.XtraRichEdit.DocumentFormat.Rtf);
                    str = bioRTB.Document.Text;

//                    string text = rtbx.Document.RtfText;
                    //int idx = text.IndexOf("[*BIOGRAPHY*]");
                    //if ( idx >= 0 )
                    //    rtb.Document.RtfText.Insert(idx, str);

                    rtbText = ReplaceField(rtbText, "[*BIOGRAPHY*]", str);
//                    rtbx.Document.RtfText = text;
                }
            }
            return rtbText;
        }
        /***********************************************************************************************/
        public static string parseRelationship(string text)
        {
            string originalText = text;
            string relationship = "";
            string[] relationships = LoadRelationships();
            int idx = text.ToUpper().IndexOf("RELATIONSHIPS=");
            if (idx < 0)
                return text;
            text = text.Substring(idx + 14);
            idx = text.IndexOf("%");
            if (idx < 0)
                return text;
            text = text.Substring(0, idx);

            string[] Lines = text.Split(',');
            text = "(";
            bool first = true;
            int count = 0;
            for (int i = 0; i < Lines.Length; i++)
            {
                if (!String.IsNullOrWhiteSpace(Lines[i]))
                {
                    relationship = Lines[i].Trim();
                    relationship = relationship.Replace("[%", "");
                    relationship = relationship.Replace("%]", "");
                    relationship = relationship.Replace("\'94", "");
                    relationship = relationship.Replace("\\u8221", "");
                    relationship = relationship.Replace("\\", "");
                    relationship = LocateRelationship(relationships, relationship);
                    if (!String.IsNullOrWhiteSpace(relationship))
                    {
                        if (!first)
                            text += ",";
                        text += "'";
                        text += relationship;
                        //                        text += Lines[i].Trim();
                        text += "'";
                        first = false;
                        count++;
                    }
                }
            }
            text += ")";
            if (count == 0)
                text = "";
            return text;
        }
        /***********************************************************************************************/
        public static string LocateRelationship(string[] relationships, string relationship)
        {
            string actualRelationship = "";
            for (int i = (relationships.Length - 1); i >= 0; i--)
            {
                if (relationship.ToUpper().IndexOf(relationships[i].ToUpper()) >= 0)
                {
                    actualRelationship = relationships[i].Trim();
                    break;
                }
            }
            return actualRelationship;
        }
        /***********************************************************************************************/
        public static string[] LoadRelationships()
        {
            string cmd = "Select * from `ref_relations`;";
            DataTable dt = G1.get_db_data(cmd);
            string relation = "";
            string relations = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                relation = dt.Rows[i]["relationship"].ObjToString();
                relations += relation + ",";
            }
            relations += "all,";
            relations = relations.TrimEnd(',');
            string[] relationships = relations.Split(',');
            return relationships;
        }
        /***********************************************************************************************/
        public static DataTable ParseOutTableMembers(string contractNumber, string table, string field, bool deceased )
        {
            bool all = false;
            bool deadOrAlive = false;
            bool nok = false;
            bool informant = false;
            bool purchaser = false;
            if ( deceased )
            {
            }
            if (field.ToUpper().IndexOf("NOK") >= 0)
                nok = true;
            if (field.ToUpper().IndexOf("INFORMANT") >= 0)
                informant = true;
            if (field.ToUpper().IndexOf("PURCHASER") >= 0)
                purchaser = true;
            string[] Lines = field.Split('~');
            string query = "";
            string[] LLines = Lines[1].Split(',');
            for (int kk = 0; kk < LLines.Length; kk++)
            {
                query += "'" + LLines[kk].Trim() + "',";
                if (LLines[kk].Trim().ToUpper().IndexOf("ALL") >= 0)
                    all = true;
                if (LLines[kk].Trim().ToUpper().IndexOf("DA") >= 0)
                    deadOrAlive = true;
            }
            query = query.TrimEnd(',');
            string cmd = "Select * from `" + table + "` WHERE `contractNumber` = '" + contractNumber + "' ";
            if (table.Trim().ToUpper() != "CUST_EXTENDED")
            {
                if (!all)
                {
                    if (!deadOrAlive)
                    {
                        if (deceased)
                            cmd += " AND `deceased` = '1' ";
                        else
                            cmd += " AND `deceased` <> '1' ";
                    }
                    if (nok)
                        cmd += " AND `nextOfKin` = '1' AND `depRelationship` <> 'DISCLOSURES' ";
                    else if (informant)
                        cmd += " AND `informant` = '1' ";
                    else if (purchaser)
                        cmd += " AND `purchaser` = '1' ";
                    else
                        cmd += " AND `depRelationship` IN (" + query + ") ";
                }
                else
                {
                    if (deceased)
                        cmd += " AND `deceased` = '1' ";
                    else
                        cmd += " AND `deceased` <> '1' ";
                }
            }
            cmd += ";";

            DataTable rDt = G1.get_db_data(cmd);
            return rDt;
        }
        /***********************************************************************************************/
        public static string GetDecGender(string workContractNumber)
        {
            string rv = "";
            string cmd = "Select * from `fcustomers` where `contractNumber` = '" + workContractNumber + "';";
            DataTable dt = G1.get_db_data(cmd);
            if (dt.Rows.Count > 0)
            {
                string sex = dt.Rows[0]["sex"].ObjToString().ToUpper();
                if (!String.IsNullOrWhiteSpace(sex))
                {
                    sex = sex.Trim().ToUpper();
                    sex = sex.Substring(0, 1);
                    rv = sex;
                }
            }
            return rv;
        }
        /***********************************************************************************************/
        public static string GetRelatives(string workContractNumber, bool deceased)
        {
            string relatives = "";
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContractNumber + "';";
            DataTable relativesDB = G1.get_db_data(cmd);
            string gender = GetDecGender(workContractNumber);
            string relationship = "";
            cmd = "Select * from `ref_relations`;";
            DataTable relations = G1.get_db_data(cmd);
            string relative = "";
            for (int i = 0; i < relations.Rows.Count; i++)
            {
                relative = "";
                relationship = relations.Rows[i]["relationship"].ObjToString();
                if (!String.IsNullOrWhiteSpace(relationship))
                    relative = GetRelative(relativesDB, gender, relationship, deceased);
                if (!String.IsNullOrWhiteSpace(relative))
                {
                    if (!String.IsNullOrWhiteSpace(relatives))
                        relatives += ", ";
                    relatives += relative;
                }
            }
            return relatives;
        }
        /***********************************************************************************************/
        public static string GetRelative(DataTable relativesDB, string decGender, string who, bool deceased)
        {
            string relative = "";
            if (relativesDB == null)
                return "";
            DateTime deceasedDate = DateTime.Now;
            DataRow[] dR = relativesDB.Select("depRelationship='" + who + "'");
            string relationShip = "";
            string name = "";
            string fname = "";
            string lname = "";
            string mname = "";
            bool dead = false;
            for (int i = 0; i < dR.Length; i++)
            {
                dead = false;
                if (dR[i]["deceased"].ObjToString() == "1")
                    dead = true;
                deceasedDate = dR[i]["depDOD"].ObjToDateTime();
                if (deceased)
                {
                    if (deceasedDate.Year < 100 && !dead )
                        continue;
                }
                else
                {
                    if (deceasedDate.Year > 100 || dead )
                        continue;
                }
                relationShip = dR[i]["depRelationship"].ObjToString();
                fname = dR[i]["depFirstName"].ObjToString();
                lname = dR[i]["depLastName"].ObjToString();
                mname = dR[i]["depMI"].ObjToString();
                name = fname;
                if (!String.IsNullOrWhiteSpace(mname))
                    name += " " + mname;
                if (!String.IsNullOrWhiteSpace(lname))
                    name += " " + lname;
                if (String.IsNullOrWhiteSpace(name))
                    continue;
                if (deceased)
                {
                    if ( deceasedDate.Year > 100 )
                        name += ", " + deceasedDate.Year.ObjToString();
                }
                if (!String.IsNullOrWhiteSpace(relationShip))
                    name += " (" + relationShip + "),";
                if (!String.IsNullOrWhiteSpace(relative))
                    relative += " ";
                relative += name;
            }
            relative = relative.TrimEnd(',');
            return relative;
        }
        /***********************************************************************************************/
        public static string Get_Relatives(string workContractNumber, bool deceased, bool individually = false, bool addCount = false, bool capRelation = false )
        {
            string relatives = "";
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContractNumber + "' ORDER BY `order`;";
            DataTable relativesDB = G1.get_db_data(cmd);
            string gender = GetDecGender(workContractNumber);
            string pronown = "his";
            if (gender.ToUpper() != "M")
                pronown = "her";
            if (individually)
            {
                pronown = "\\line\\line " + G1.force_lower_line(pronown);
                pronown = "\\line\\line ";
            }
            string relationship = "";
            cmd = "Select * from `ref_relations`;";
            DataTable relations = G1.get_db_data(cmd);
            string relative = "";
            int relativeCount = 0;
            bool gotWife = false;
            bool gotHusband = false;
            string numberText = "";
            DataRow[] wifes = relativesDB.Select("depRelationship='Wife'");
            DataRow[] husbands = relativesDB.Select("depRelationship='Husband'");
            DataRow[] grandchild = relativesDB.Select("depRelationship='Grandchild'");
            DataRow[] grandchildren = relativesDB.Select("depRelationship='Grandchildren'");
            if ( grandchild.Length > 0 && grandchildren.Length > 0 )
            {
                for ( int i=0; i<grandchildren.Length; i++)
                    grandchildren[i]["depRelationship"] = "Grandchild";
            }
            DataRow[] greatgrandchild = relativesDB.Select("depRelationship='Great Grandchild'");
            DataRow[] greatgrandchildren = relativesDB.Select("depRelationship='Great Grandchildren'");
            if (greatgrandchild.Length > 0 && greatgrandchildren.Length > 0)
            {
                for (int i = 0; i < greatgrandchildren.Length; i++)
                    greatgrandchildren[i]["depRelationship"] = "Great Grandchild";
            }

            for (int i = 0; i < relations.Rows.Count; i++)
            {
                relativeCount = 0;
                relative = "";
                relationship = relations.Rows[i]["relationship"].ObjToString();
                if (relationship.ToUpper() == "SPOUSE")
                {
                    if ( wifes.Length > 0 )
                        continue;
                    if (husbands.Length > 0)
                        continue;
                }
                if ( relationship == "Grandchild")
                {
                }
                if (!String.IsNullOrWhiteSpace(relationship))
                    relative = Get_Relative(relativesDB, gender, relationship, deceased, ref relativeCount );
                if (!String.IsNullOrWhiteSpace(relative))
                {
                    if (!String.IsNullOrWhiteSpace(relatives))
                    {
                        if (individually)
                            relatives += "\\line\\line";
                        else
                            relatives += "; ";
                    }
                    else
                        relatives += pronown;
                    if ( capRelation )
                        relationship = G1.force_lower_line(relationship);
                    if (relativeCount > 1)
                    {
                        relationship += "s";
                        if (relationship.ToUpper() == "GRANDCHILDS")
                            relationship = "Grandchildren";
                        else if (relationship.ToUpper() == "GREAT GRANDCHILDS")
                            relationship = "Great Grandchildren";
                        if (addCount)
                        {
                            numberText = NumberToWords(relativeCount);
                            relationship =  numberText + " " + relationship;
                        }
                    }
                    if ( !capRelation )
                        relationship = relationship.ToLower();
                    if (individually)
                        relationship = G1.force_lower_line(relationship);
                    if ( individually )
                        relatives += " " + relationship + ": ";
                    else
                        relatives += " " + relationship + ", ";
                    relatives += relative;
                }
            }
            return relatives;
        }
        /***********************************************************************************************/
        public static string Get_RelativesList(string option, string workContractNumber, bool deceased, bool addCount = false, bool capRelation = false )
        {
            if ( option.ToUpper() == "PRECEDED3" || option.ToUpper() == "SURVLIST3")
            {
                string rv = Get_Relatives3(option, workContractNumber, deceased, addCount, capRelation);
                return rv;
            }
            else if (option.ToUpper() == "PRECEDED5" || option.ToUpper() == "SURVLIST5")
            {
                string rv = Get_Relatives5(option, workContractNumber, deceased, addCount, capRelation);
                return rv;
            }
            string relatives = "";
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContractNumber + "' ORDER BY `order`;";
            DataTable relativesDB = G1.get_db_data(cmd);
            string gender = GetDecGender(workContractNumber);
            string pronown = "his";
            if (gender.ToUpper() != "M")
                pronown = "her";
            //if (individually)
            //{
            //    pronown = "\\line\\line " + G1.force_lower_line(pronown);
            //    pronown = "\\line\\line ";
            //}
            string relationship = "";
            cmd = "Select * from `ref_relations`;";
            DataTable relations = G1.get_db_data(cmd);
            string relative = "";
            int relativeCount = 0;
            bool gotWife = false;
            bool gotHusband = false;
            DataRow[] wifes = relativesDB.Select("depRelationship='Wife'");
            DataRow[] husbands = relativesDB.Select("depRelationship='Husband'");
            DataRow[] mother = relativesDB.Select("depRelationship='Mother'");
            DataRow[] father = relativesDB.Select("depRelationship='Father'");

            for (int i = 0; i < relations.Rows.Count; i++)
            {
                relativeCount = 0;
                relative = "";
                relationship = relations.Rows[i]["relationship"].ObjToString();
                if (relationship.ToUpper() == "SPOUSE")
                {
                    if (wifes.Length > 0)
                        continue;
                    if (husbands.Length > 0)
                        continue;
                }
                if (!String.IsNullOrWhiteSpace(relationship))
                    relative = Get_Relative(relativesDB, gender, relationship, deceased, ref relativeCount, addCount, capRelation );
                if (!String.IsNullOrWhiteSpace(relative))
                {
                    if (!String.IsNullOrWhiteSpace(relatives))
                            relatives += "\\line\\line";
                    if (relativeCount > 1)
                        relationship += "s";
                    relationship = relationship.ToLower();
                    relationship = G1.force_lower_line(relationship);
                    relatives += " " + relationship + ": ";
                    relatives += relative;
                }
            }
            return relatives;
        }
        /***********************************************************************************************/
        public static string Get_Relatives3(string option, string workContractNumber, bool deceased, bool addCount = false, bool capRelation = false)
        {
            string relatives = "";
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContractNumber + "' ORDER BY `order`;";
            DataTable relativesDB = G1.get_db_data(cmd);
            string gender = GetDecGender(workContractNumber);
            string pronown = "his";
            if (gender.ToUpper() != "M")
                pronown = "her";
            //if (individually)
            //{
            //    pronown = "\\line\\line " + G1.force_lower_line(pronown);
            //    pronown = "\\line\\line ";
            //}
            string relationship = "";
            cmd = "Select * from `ref_relations`;";
            DataTable relations = G1.get_db_data(cmd);
            string relative = "";
            int relativeCount = 0;
            bool gotWife = false;
            bool gotHusband = false;
            bool gotSpouse = false;
            DataRow[] wifes = relativesDB.Select("depRelationship='Wife'");
            DataRow[] husbands = relativesDB.Select("depRelationship='Husband'");
            DataRow[] mother = relativesDB.Select("depRelationship='Mother'");
            DataRow[] father = relativesDB.Select("depRelationship='Father'");
            DataRow[] spouse = relativesDB.Select("depRelationship='Spouse'");

            string fName = "";
            string lName = "";
            string city = "";
            string state = "";

            DataTable fatherDB = relativesDB.Clone();
            if (father.Length > 0)
                fatherDB = father.CopyToDataTable();

            DataTable motherDB = relativesDB.Clone();
            if (mother.Length > 0)
                motherDB = mother.CopyToDataTable();

            DataTable spouseDB = relativesDB.Clone();
            if (spouse.Length > 0)
                spouseDB = spouse.CopyToDataTable();

            if ( spouseDB.Rows.Count <= 0 )
            {
                if (wifes.Length > 0)
                    spouseDB = wifes.CopyToDataTable();
                else if (husbands.Length > 0)
                    spouseDB = husbands.CopyToDataTable();
            }

            if (deceased)
            {
                for (int i = 0; i < relations.Rows.Count; i++)
                {
                    relativeCount = 0;
                    relative = "";
                    relationship = relations.Rows[i]["relationship"].ObjToString();
                    if (!String.IsNullOrWhiteSpace(relationship))
                        relative = Get_Relative(relativesDB, gender, relationship, deceased, ref relativeCount, false, capRelation);
                    if (!String.IsNullOrWhiteSpace(relative))
                    {
                        if (relationship.ToUpper() == "SPOUSE")
                            gotSpouse = true;
                        if (relationship.ToUpper() == "WIFE" && gotSpouse)
                            continue;
                        if (relationship.ToUpper() == "HUSBAND" && gotSpouse)
                            continue;
                        if (!String.IsNullOrWhiteSpace(relatives))
                            relatives += "\\line";
                        relationship = relationship.ToLower();
                        relationship = G1.force_lower_line(relationship);
                        relatives = relatives.TrimEnd(' ');
                        relatives += " " + relative + " - " + relationship;
                    }
                }
            }
            else
            {
                bool gotFather = false;
                bool gotMother = false;
                for ( int i=0; i<fatherDB.Rows.Count; i++)
                {
                    if (fatherDB.Rows[i]["deceased"].ObjToString() != "1")
                        gotFather = true;
                }
                for (int i = 0; i < motherDB.Rows.Count; i++)
                {
                    if (motherDB.Rows[i]["deceased"].ObjToString() != "1")
                        gotMother = true;
                }
                if (gotFather || gotMother)
                {
                    if (gotFather && gotMother)
                        relationship = "Parents:";
                    else if (gotFather || gotMother)
                        relationship = "Parent:";
                    relatives += "\\b " + relationship + " \\b0\\line";
                    if (fatherDB.Rows.Count > 0)
                    {
                        for (int i = 0; i < fatherDB.Rows.Count; i++)
                        {
                            if (fatherDB.Rows[i]["deceased"].ObjToString() == "1")
                                continue;
                            fName = fatherDB.Rows[i]["depFirstName"].ObjToString();
                            lName = fatherDB.Rows[i]["depLastName"].ObjToString();
                            city = fatherDB.Rows[i]["city"].ObjToString();
                            state = fatherDB.Rows[i]["state"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(lName))
                            {
                                relatives += "     " + fName + " " + lName;
                                if (!String.IsNullOrWhiteSpace(city) || !String.IsNullOrWhiteSpace(state))
                                {
                                    relatives += " - " + city;
                                    if (!String.IsNullOrWhiteSpace(city))
                                        relatives += ", " + state;
                                }
                                relatives += "\\line";
                            }
                        }
                    }
                    if (motherDB.Rows.Count > 0)
                    {
                        for (int i = 0; i < motherDB.Rows.Count; i++)
                        {
                            if (motherDB.Rows[i]["deceased"].ObjToString() == "1")
                                continue;
                            fName = motherDB.Rows[i]["depFirstName"].ObjToString();
                            lName = motherDB.Rows[i]["depLastName"].ObjToString();
                            city = motherDB.Rows[i]["city"].ObjToString();
                            state = motherDB.Rows[i]["state"].ObjToString();
                            if (!String.IsNullOrWhiteSpace(lName))
                            {
                                relatives += "     " + fName + " " + lName;
                                if (!String.IsNullOrWhiteSpace(city) || !String.IsNullOrWhiteSpace(state))
                                {
                                    relatives += " - " + city;
                                    if (!String.IsNullOrWhiteSpace(city))
                                        relatives += ", " + state;
                                }
                                relatives += "\\line";
                            }
                        }
                    }
                }

                if (spouseDB.Rows.Count > 0)
                {
                    relationship = "Spouse:";
                    relatives += "\\line\\b " + relationship + " \\b0\\line";
                    for (int i = 0; i < spouseDB.Rows.Count; i++)
                    {
                        fName = spouseDB.Rows[i]["depFirstName"].ObjToString();
                        lName = spouseDB.Rows[i]["depLastName"].ObjToString();
                        city = spouseDB.Rows[i]["city"].ObjToString();
                        state = spouseDB.Rows[i]["state"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(lName))
                        {
                            relatives += "     " + fName + " " + lName;
                            if (!String.IsNullOrWhiteSpace(city) || !String.IsNullOrWhiteSpace(state))
                            {
                                relatives += " - " + city;
                                if (!String.IsNullOrWhiteSpace(city))
                                    relatives += ", " + state;
                            }
                            relatives += "\\line";
                        }
                    }
                }

                relatives = findRelatives(relativesDB, relations, relatives, "Children", "Son,Daughter");
                relatives = findRelatives(relativesDB, relations, relatives, "Siblings", "Brother,Sister");
                relatives = findRelatives(relativesDB, relations, relatives, "Grandchildren", "Granddaughter,Grandson,Grandchild");
                relatives = findRelatives(relativesDB, relations, relatives, "Great Grandchildren", "Great Granddaughter,Great Grandson,Great Grandchild");
                relatives = findRelatives(relativesDB, relations, relatives, "Nephews or Nieces", "Niece,Nephew");

                //for (int i = 0; i < relations.Rows.Count; i++)
                //{
                //    relativeCount = 0;
                //    relative = "";
                //    relationship = relations.Rows[i]["relationship"].ObjToString().ToUpper();
                //    if (relationship == "FATHER" || relationship == "MOTHER" || relationship== "SPOUSE" || relationship == "WIFE" || relationship == "HUSBAND" )
                //        continue;
                //    if (!String.IsNullOrWhiteSpace(relationship))
                //        relative = Get_Relative(relativesDB, gender, relationship, deceased, ref relativeCount, false, capRelation);
                //    if (!String.IsNullOrWhiteSpace(relative))
                //    {
                //        if (!String.IsNullOrWhiteSpace(relatives))
                //            relatives += "\\line\\line";
                //        if (relativeCount > 1)
                //            relationship += "s";
                //        relationship = relationship.ToLower();
                //        relationship = G1.force_lower_line(relationship);
                //        relatives += "\\b " + relationship + " \\b0\\line";
                //        relatives += relative;
                //    }
                //}
            }
            return relatives;
        }
        /***********************************************************************************************/
        public static string Get_Relatives5(string option, string workContractNumber, bool deceased, bool addCount = false, bool capRelation = false)
        {
            string relatives = "";
            string cmd = "Select * from `relatives` where `contractNumber` = '" + workContractNumber + "' ORDER BY `order`;";
            DataTable relativesDB = G1.get_db_data(cmd);
            string gender = GetDecGender(workContractNumber);
            string pronown = "his";
            if (gender.ToUpper() != "M")
                pronown = "her";

            string fName = "";
            string lName = "";
            string city = "";
            string state = "";
            string relationship = "";
            string relative = "";

            for (int i = 0; i < relativesDB.Rows.Count; i++)
            {
                relative = Get_Relative ( relativesDB, i, deceased, ref relationship );
                if (relationship == "PB" || relationship == "HPB" || relationship == "CLERGY" || relationship == "MUSICIAN" || relationship == "Funeral Director" || relationship == "DISCLOSURES")
                    continue;
                if ( !String.IsNullOrWhiteSpace ( relative ))
                    relatives += "\\line        " + relative + " - " + relationship + "";
            }

            return relatives;
        }
        /***********************************************************************************************/
        public static string findRelatives(DataTable relativesDB, DataTable relations, string relatives, string title, string relationList )
        {
            int relativeCount = 0;
            string relationship = "";
            string relative = "";
            string gender = "";
            bool deceased = false;
            bool capRelation = false;
            bool addCount = false;

            string relation = "";
            DataRow[] people = null;
            DataTable peopleDB = null;

            string fName = "";
            string lName = "";
            string city = "";
            string state = "";

            string[] List = relationList.Split(',');

            bool first = true;
            for ( int i=0; i<List.Length; i++)
            {
                relation = List[i].Trim();
                if (String.IsNullOrWhiteSpace(relation))
                    continue;
                people = relativesDB.Select("depRelationship='" + relation + "' AND `deceased` <> '1'");
                if (people.Length <= 0)
                    continue;
                if (first)
                {
                    relationship = title + ":";
                    relatives += "\\line\\b " + relationship + " \\b0\\line";
                    first = false;
                }
                peopleDB = people.CopyToDataTable();

                if (peopleDB.Rows.Count > 0)
                {
                    for (int j = 0; j < peopleDB.Rows.Count; j++)
                    {
                        fName = peopleDB.Rows[j]["depFirstName"].ObjToString();
                        lName = peopleDB.Rows[j]["depLastName"].ObjToString();
                        city = peopleDB.Rows[j]["city"].ObjToString();
                        state = peopleDB.Rows[j]["state"].ObjToString();
                        if (!String.IsNullOrWhiteSpace(lName))
                        {
                            relatives += "     " + fName + " " + lName;
                            if (!String.IsNullOrWhiteSpace(city) || !String.IsNullOrWhiteSpace(state))
                            {
                                relatives += " - " + city;
                                if (!String.IsNullOrWhiteSpace(city))
                                    relatives += ", " + state;
                            }
                            relatives += "\\line";
                        }
                    }
                }

            }
            return relatives;
        }
        /***********************************************************************************************/
        public static string Get_Relative(DataTable relativesDB, string decGender, string who, bool deceased, ref int relativeCount, bool addCount = false, bool capRelation = false )
        {
            string relative = "";
            relativeCount = 0;
            if (relativesDB == null)
                return "";
            DateTime deceasedDate = DateTime.Now;
            string cmd = "depRelationship='" + who + "'";
            if ( who.ToUpper() == "SPOUSE")
            {
                cmd = "depRelationship='" + who + "' OR depRelationship='Wife' OR depRelationship='Husband'";
            }
            DataRow[] dR = relativesDB.Select(cmd);
            string relationShip = "";
            string prefix = "";
            string suffix = "";
            string name = "";
            string fname = "";
            string lname = "";
            string mname = "";
            string spouse = "";
            string maiden = "";
            string city = "";
            string state = "";
            bool dead = false;
            for (int i = 0; i < dR.Length; i++)
            {
                dead = false;
                if (dR[i]["deceased"].ObjToString() == "1")
                    dead = true;
                deceasedDate = dR[i]["depDOD"].ObjToDateTime();
                if (deceased)
                {
                    if (deceasedDate.Year < 100 && !dead)
                        continue;
                }
                else
                {
                    if (deceasedDate.Year > 100 || dead)
                        continue;
                }
                relationShip = dR[i]["depRelationship"].ObjToString();
                prefix = dR[i]["depPrefix"].ObjToString();
                fname = dR[i]["depFirstName"].ObjToString();
                lname = dR[i]["depLastName"].ObjToString();
                mname = dR[i]["depMI"].ObjToString();
                maiden = dR[i]["maidenName"].ObjToString();
                suffix = dR[i]["depSuffix"].ObjToString();
                if (!String.IsNullOrWhiteSpace(maiden))
                {
                    if (!String.IsNullOrWhiteSpace(mname))
                        mname += " " + maiden;
                    else
                        mname = maiden;
                }
                name = prefix;
                if (!String.IsNullOrWhiteSpace(name))
                    name += " ";
                name += fname;
                if (!String.IsNullOrWhiteSpace(mname))
                    name += " " + mname;
                if (!String.IsNullOrWhiteSpace(lname))
                    name += " " + lname;
                if (!String.IsNullOrWhiteSpace(suffix))
                    name += ", " + suffix;
                if (String.IsNullOrWhiteSpace(name))
                    continue;
                spouse = dR[i]["spouseFirstName"].ObjToString();
                maiden = dR[i]["maidenName"].ObjToString();
                city = dR[i]["city"].ObjToString();
                state = dR[i]["state"].ObjToString();
                //if (deceased)
                //{
                //    if (deceasedDate.Year > 100)
                //        name += ", " + deceasedDate.Year.ObjToString();
                //}
                //if (!String.IsNullOrWhiteSpace(relationShip))
                //    name += " (" + relationShip + "),";
                //if (String.IsNullOrWhiteSpace(spouse) && !String.IsNullOrWhiteSpace(maiden))
                //    spouse = maiden;
                if (!String.IsNullOrWhiteSpace(spouse))
                    name += " (" + spouse + ")";
                else
                    relative += " ";

                if (relativeCount >= 1)
                {
                    //relative += "and" + relativeCount.ToString() + " ";
                    relative = relative.TrimEnd(' ');
                    relative += ",and" + i.ToString();
                }
                relative += name;

                if (!String.IsNullOrWhiteSpace(city) && !String.IsNullOrWhiteSpace(state))
                    relative += " of " + city + ", " + state + " ";
                else if (!String.IsNullOrWhiteSpace(city))
                    relative += " of " + city + " ";
                else if (!String.IsNullOrWhiteSpace(state))
                    relative += " of " + state + " ";

                relativeCount++;
                if (who.ToUpper() == "FATHER" || who.ToUpper() == "MOTHER")
                    break;
            }
            if ( !String.IsNullOrWhiteSpace ( relative))
            {
            }
            relative = relative.TrimEnd(',');
            relative = relative.Trim();
            string pronoun = "";
            //relativeCount = dR.Length;
            int count = dR.Length;
            for ( int i=(count-1); i>=0; i--)
            {
                pronoun = ",and" + i.ToString();
                if (i == (count - 1))
                    relative = relative.Replace(pronoun, " and ");
                else
                    relative = relative.Replace(pronoun, ", ");
            }
            //for ( int i=1; i<=(count-1); i++)
            //{
            //    pronoun = ",and" + i.ToString();
            //    if (i == (count-1))
            //        relative = relative.Replace(pronoun, " and ");
            //    else
            //        relative = relative.Replace(pronoun, ", ");
            //}
            return relative;
        }
        /***********************************************************************************************/
        public static string Get_Relative(DataTable relativesDB, int i, bool deceased, ref string relationship )
        {
            string relative = "";
            if (relativesDB == null)
                return "";
            DateTime deceasedDate = DateTime.Now;
            string prefix = "";
            string suffix = "";
            string name = "";
            string fname = "";
            string lname = "";
            string mname = "";
            string spouse = "";
            string maiden = "";
            string city = "";
            string state = "";
            bool dead = false;
            dead = false;
            if (relativesDB.Rows[i]["deceased"].ObjToString() == "1")
                dead = true;
            deceasedDate = relativesDB.Rows[i]["depDOD"].ObjToDateTime();
            if (deceased)
            {
                if (deceasedDate.Year < 100 && !dead)
                    return "";
            }
            else
            {
                if (deceasedDate.Year > 100 || dead)
                    return "";
            }
            relationship = relativesDB.Rows[i]["depRelationship"].ObjToString();
            prefix = relativesDB.Rows[i]["depPrefix"].ObjToString();
            fname = relativesDB.Rows[i]["depFirstName"].ObjToString();
            lname = relativesDB.Rows[i]["depLastName"].ObjToString();
            mname = relativesDB.Rows[i]["depMI"].ObjToString();
            maiden = relativesDB.Rows[i]["maidenName"].ObjToString();
            suffix = relativesDB.Rows[i]["depSuffix"].ObjToString();
            if (!String.IsNullOrWhiteSpace(maiden))
            {
                if (!String.IsNullOrWhiteSpace(mname))
                    mname += " " + maiden;
                else
                    mname = maiden;
            }
            name = prefix;
            if (!String.IsNullOrWhiteSpace(name))
                name += " ";
            name += fname;
            if (!String.IsNullOrWhiteSpace(mname))
                name += " " + mname;
            if (!String.IsNullOrWhiteSpace(lname))
                name += " " + lname;
            if (!String.IsNullOrWhiteSpace(suffix))
                name += ", " + suffix;
            if (String.IsNullOrWhiteSpace(name))
                return "";
            spouse = relativesDB.Rows[i]["spouseFirstName"].ObjToString();
            maiden = relativesDB.Rows[i]["maidenName"].ObjToString();
            city = relativesDB.Rows[i]["city"].ObjToString();
            state = relativesDB.Rows[i]["state"].ObjToString();
            if (!String.IsNullOrWhiteSpace(spouse))
                name += " (" + spouse + ")";

            relative = name;

            if (!String.IsNullOrWhiteSpace(city) && !String.IsNullOrWhiteSpace(state))
                relative += " of " + city + ", " + state + " ";
            else if (!String.IsNullOrWhiteSpace(city))
                relative += " of " + city + " ";
            else if (!String.IsNullOrWhiteSpace(state))
                relative += " of " + state + " ";

            return relative;
        }
        /***********************************************************************************************/
    }
}