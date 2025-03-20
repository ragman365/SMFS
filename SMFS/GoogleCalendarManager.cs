// Google Calendar Integration in ASP.NET: Create / Edit / Delete events by m.waqasiqbal
// http://www.codeproject.com/Articles/565032/Google-Calendar-Integration-in-ASP-NET-Create-ed
// Rewritten for Google Calendar API v3 by Hewbert Gabon
  
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

using System.IO;
using System.Threading;
using Google.Apis.Calendar.v3.Data;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using System.Threading.Tasks;

using Google.Apis.Calendar;

//using Google.Apis.Calendar.v3;
//using Google.Apis.Calendar.v3.Data;
//using Google.Apis.Auth.OAuth2;
//using Google.Apis.Auth.OAuth2.Flows;
//using Google.Apis.Auth.OAuth2.Web;
//using Google.Apis.Services;
//using Google.Apis.Util.Store;


public class GoogleCalendarManager
{
    //public struct GoogleCalendarAppointmentModel
    //{
    //    public string EventID;
    //    public bool DeleteAppointment;
    //    public DateTime EventStartTime;
    //    public string EventTitle;
    //    public string EventLocation;
    //    public string EventDetails;
    //}

    public static DataTable CalendarDt = null;
    /***************************************************************************************/
    public static void InitCalander(string title)
    {
        if (CalendarDt == null)
        {
            CalendarDt = new DataTable();
            CalendarDt.Columns.Add("Title");
            CalendarDt.Columns.Add("Who");
            CalendarDt.Columns.Add("Start");
            CalendarDt.Columns.Add("Stop");
            CalendarDt.Columns.Add("Location");
            CalendarDt.Columns.Add("Details");
            CalendarDt.Columns.Add("Result");
        }
        if ( CalendarDt != null )
        {
            CalendarDt.Rows.Clear();
        }        
    }
    /****************************************************************************************/
    public static void AddCalanderEvent(string title, string who, string details, string location, string result, DateTime start, DateTime stop)
    {
        if (CalendarDt == null)
            InitCalander(title);

        DataRow dRow = CalendarDt.NewRow();
        dRow["Title"] = title;
        dRow["Who"] = who;
        dRow["Details"] = details;
        dRow["Location"] = location;
        dRow["Start"] = start;
        dRow["Stop"] = stop;
        dRow["Result"] = result;

        CalendarDt.Rows.Add(dRow);
    }
    /****************************************************************************************/
    public static DataTable GetCalendarEvents ()
    {
        return CalendarDt;
    }
}