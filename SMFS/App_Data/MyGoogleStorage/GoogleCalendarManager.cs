// Google Calendar Integration in ASP.NET: Create / Edit / Delete events by m.waqasiqbal
// http://www.codeproject.com/Articles/565032/Google-Calendar-Integration-in-ASP-NET-Create-ed
// Rewritten for Google Calendar API v3 by Hewbert Gabon

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

using System.IO;
using System.Threading;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Services;
using Google.Apis.Util.Store;


public class GoogleCalendarManager
{
    private static string calID = "m.waqasiqbal@gmail.com"; //System.Configuration.ConfigurationManager.AppSettings["GoogleCalendarID"].ToString()
    private static string UserId = "m.waqasiqbal"; //System.Web.HttpContext.Current.User.Identity.Name
    private static string gFolder = System.Web.HttpContext.Current.Server.MapPath("/App_Data/MyGoogleStorage");

    public static CalendarService GetCalendarService(GoogleTokenModel GoogleTokenModelObj)
    {
        CalendarService service = null;

        IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = GetClientConfiguration().Secrets,
                DataStore = new FileDataStore(gFolder),
                Scopes = new[] { CalendarService.Scope.Calendar }
            });

        var uri = /*"http://localhost:19594/GoogleCalendarRegistration.aspx";*/System.Web.HttpContext.Current.Request.Url.ToString();
        var code = System.Web.HttpContext.Current.Request["code"];
        if (code != null)
        {
            var token = flow.ExchangeCodeForTokenAsync(UserId, code,
                uri.Substring(0, uri.IndexOf("?")), CancellationToken.None).Result;

            // Extract the right state.
            var oauthState = AuthWebUtility.ExtracRedirectFromState(
                flow.DataStore, UserId, System.Web.HttpContext.Current.Request["state"]).Result;
            System.Web.HttpContext.Current.Response.Redirect(oauthState);
        }
        else
        {
            var result = new AuthorizationCodeWebApp(flow, uri, uri).AuthorizeAsync(UserId, CancellationToken.None).Result;
            if (result.RedirectUri != null)
            {
                // Redirect the user to the authorization server.
                System.Web.HttpContext.Current.Response.Redirect(result.RedirectUri);
                //var page = System.Web.HttpContext.Current.CurrentHandler as Page;
                //page.ClientScript.RegisterClientScriptBlock(page.GetType(),
                //    "RedirectToGoogleScript", "window.top.location = '" + result.RedirectUri + "'", true);
            }
            else
            {
                // The data store contains the user credential, so the user has been already authenticated.
                service = new CalendarService(new BaseClientService.Initializer
                {
                    ApplicationName = "My ASP.NET Google Calendar App",
                    HttpClientInitializer = result.Credential
                });
            }
        }

        return service;
    }

    public static GoogleClientSecrets GetClientConfiguration()
    {
        using (var stream = new FileStream(gFolder + @"\client_secret.json", FileMode.Open, FileAccess.Read))
        {
            return GoogleClientSecrets.Load(stream);
        }
    }

    public static bool AddUpdateDeleteEvent(List<GoogleCalendarAppointmentModel> GoogleCalendarAppointmentModelList, List<GoogleTokenModel> GoogleTokenModelList, double TimeOffset)
    {
        //Get the calendar service for a user to add/update/delete events
        CalendarService calService = GetCalendarService(GoogleTokenModelList[0]);

        if (GoogleCalendarAppointmentModelList != null && GoogleCalendarAppointmentModelList.Count > 0)
        {
            foreach (GoogleCalendarAppointmentModel GoogleCalendarAppointmentModelObj in GoogleCalendarAppointmentModelList)
            {
                EventsResource er = new EventsResource(calService);
                string ExpKey = "EventID";
                string ExpVal = GoogleCalendarAppointmentModelObj.EventID;

                var queryEvent = er.List(calID);
                queryEvent.SharedExtendedProperty = ExpKey + "=" + ExpVal; //"EventID=9999"
                var EventsList = queryEvent.Execute();

                //to restrict the appointment for specific staff only
                //Delete this appointment from google calendar
                if (GoogleCalendarAppointmentModelObj.DeleteAppointment == true)
                {
                    string FoundEventID = String.Empty;
                    foreach (Event evItem in EventsList.Items)
                    {
                        FoundEventID = evItem.Id;
                        if (!String.IsNullOrEmpty(FoundEventID))
                        {
                            er.Delete(calID, FoundEventID).Execute();
                        }
                    }
                    return true;
                }
                //Add if not found OR update if appointment already present on google calendar
                else
                {
                    Event eventEntry = new Event();

                    EventDateTime StartDate = new EventDateTime();
                    EventDateTime EndDate = new EventDateTime();
                    StartDate.Date = GoogleCalendarAppointmentModelObj.EventStartTime.ToString("yyyy-MM-dd"); //"2014-11-17";
                    EndDate.Date = StartDate.Date; //GoogleCalendarAppointmentModelObj.EventEndTime

                    //Always append Extended Property whether creating or updating event
                    Event.ExtendedPropertiesData exp = new Event.ExtendedPropertiesData();
                    exp.Shared = new Dictionary<string, string>();
                    exp.Shared.Add(ExpKey, ExpVal);

                    eventEntry.Summary = GoogleCalendarAppointmentModelObj.EventTitle;
                    eventEntry.Start = StartDate;
                    eventEntry.End = EndDate;
                    eventEntry.Location = GoogleCalendarAppointmentModelObj.EventLocation;
                    eventEntry.Description = GoogleCalendarAppointmentModelObj.EventDetails;
                    eventEntry.ExtendedProperties = exp;

                    string FoundEventID = String.Empty;
                    foreach (var evItem in EventsList.Items)
                    {
                        FoundEventID = evItem.Id;
                        if (!String.IsNullOrEmpty(FoundEventID))
                        {
                            //Update the event
                            er.Update(eventEntry, calID, FoundEventID).Execute();
                        }
                    }

                    if (String.IsNullOrEmpty(FoundEventID))
                    {
                        //create the event
                        er.Insert(eventEntry, calID).Execute();
                    }

                    return true;
                }
            }
        }

        return false;
    }
}
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//using Google.GData.Client;
//using Google.GData.Calendar;
//using Google.GData.AccessControl;
//using Google.GData.Extensions;
//using System.Net;
//using System.IO;



//public class GoogleCalendarManager
//{

//    private static string ReturnUrl = @System.Configuration.ConfigurationSettings.AppSettings["GoogleReturnPageAddress"].ToString();

//    private static string ClientID = System.Configuration.ConfigurationSettings.AppSettings["GoogleCalendarApplicationClientID"].ToString();

//    private static string ClientSecret = System.Configuration.ConfigurationSettings.AppSettings["GoogleCalendarApplicationClientSecret"].ToString();


//    private static string PrivateFeed = @"http://www.google.com/calendar/feeds/default/private/full";
//    private static string OwnCalendarFeed = @"http://www.google.com/calendar/feeds/default/owncalendars/full";
//    private static string AllCalendarFeed = @"https://www.googleapis.com/calendar/v3/users/me/calendarList";//@"http://www.google.com/calendar/feeds/default/allcalendars/full";

//    private static String userName, userPassword, feedUri;

//    /// <summary>
//    /// Prints a list of the user's calendars.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>

//    #region Currently Required Functions

//    public static string GenerateGoogleOAuthURL()
//    {
//        string Url = "https://accounts.google.com/o/oauth2/auth?scope={0}&redirect_uri={1}&response_type={2}&client_id={3}&state={4}&access_type={5}&approval_prompt={6}";
//        //string scope = UrlEncodeForGoogle("http://www.google.com/calendar/feeds/default/private/full").Replace("%20", "+");
//        string scope = UrlEncodeForGoogle(PrivateFeed).Replace("%20", "+");
//        string redirect_uri_encode = UrlEncodeForGoogle(ReturnUrl);
//        string response_type = "code";
//        string state = "";
//        string access_type = "offline";
//        string approval_prompt = "auto";
//        return string.Format(Url, scope, redirect_uri_encode, response_type, ClientID, state, access_type, approval_prompt);
//    }

//    private static string GetCalendarID(CalendarService CalService)
//    {
//        Uri postUri = new Uri(AllCalendarFeed);
//        CalendarQuery CalendarQuery = new CalendarQuery();

//        CalendarQuery.Uri = postUri;

//        CalendarFeed calFeed = CalService.Query(CalendarQuery);

//        string CalendarID = "";

//        if (calFeed != null && calFeed.Entries.Count > 0)
//        {
//            foreach (CalendarEntry CalEntry in calFeed.Entries)
//            {
//                //Commented to post the new appointments on the main calendar instead of cleverfox calendar
//                //if (CalEntry.Title.Text.Contains("Cleverfox") == true)
//                //{
//                //CalendarID = CalEntry.Title.Text;
//                CalendarID = CalEntry.EditUri.ToString().Substring(CalEntry.EditUri.ToString().LastIndexOf("/") + 1);
//                break;
//                //}
//            }
//        }

//        #region Commented to post the new appointments on the main calendar instead of cleverfox calendar
//        /*if (string.IsNullOrEmpty(CalendarID) == false)
//            {

//            }
//            else
//            {
//                Google.GData.Client.AtomEntry cal = new AtomEntry();
//                cal.Title.Text = "Cleverfox";

//                CalService.Insert(new Uri(OwnCalendarFeed), cal);
//            }
//            calFeed = CalService.Query(CalendarQuery);

//            //if search contains result then update
//            if (calFeed != null && calFeed.Entries.Count > 0)
//            {
//                foreach (CalendarEntry CalEntry in calFeed.Entries)
//                {
//                    if (CalEntry.Title.Text.Contains("Cleverfox") == true)
//                    {
//                        //CalendarName = CalEntry.Title.Text;
//                        CalendarID = CalEntry.EditUri.ToString().Substring(CalEntry.EditUri.ToString().LastIndexOf("/") + 1);
//                        //if (CalEntry.TimeZone != "Canada/Vancouver")
//                        //{
//                        //    CalEntry.TimeZone = "(GMT-07:00) Arizona";
//                        //    CalEntry.Update();
//                        //}
//                        if (CalEntry.TimeZone != MerchantTimeZone)
//                        {
//                            CalEntry.TimeZone = MerchantTimeZone;
//                            CalEntry.Update();
//                        }
//                        break;
//                    }
//                }
//            }*/
//        #endregion
//        return CalendarID;
//    }
//    public static CalendarService GetCalendarService(GoogleTokenModel GoogleTokenModelObj)
//    {
//        GAuthSubRequestFactory authFactory = new GAuthSubRequestFactory("cl", "API Project");
//        authFactory.Token = GoogleTokenModelObj.Access_Token;
//        authFactory.KeepAlive = true;

//        CalendarService service = new CalendarService("cl");
//        service.RequestFactory = authFactory;

//        return service;
//    }
//    public static bool AddUpdateDeleteEvent(List<GoogleTokenModel> GoogleTokenModelList, List<GoogleCalendarAppointmentModel> GoogleCalendarAppointmentModelList, double TimeOffset)
//    {
//        bool result = false;
//        foreach (GoogleTokenModel GoogleAppointmentOAuthDetailsObj in GoogleTokenModelList)
//        {
//            //Get the calendar service for a user to add/update/delete events
//            CalendarService CalService = GetCalendarService(GoogleAppointmentOAuthDetailsObj);
//            //get the calendar id for this user to add/update/delete events
//            string CalendarID = GetCalendarID(CalService);

//            EventEntry InsertedEntry = new EventEntry();

//            if (GoogleCalendarAppointmentModelList != null && GoogleCalendarAppointmentModelList.Count > 0)
//            {
//                foreach (GoogleCalendarAppointmentModel GoogleCalendarAppointmentModelObj in GoogleCalendarAppointmentModelList)
//                {
//                    //to restrict the appointment for specific staff only
//                    //Delete this appointment from google calendar
//                    if (GoogleCalendarAppointmentModelObj.DeleteAppointment == true)
//                    {
//                        Google.GData.Calendar.EventEntry Entry = new Google.GData.Calendar.EventEntry();

//                        ExtendedProperty oExtendedProperty = new ExtendedProperty();
//                        oExtendedProperty.Name = "EventID";
//                        oExtendedProperty.Value = GoogleCalendarAppointmentModelObj.EventID;

//                        //search the calendar so to update or add appointment in it
//                        string ThisFeedUri = "http://www.google.com/calendar/feeds/" + CalendarID + "/private/full";
//                        Uri postUri = new Uri(ThisFeedUri);
//                        EventQuery Query = new EventQuery(ThisFeedUri);
//                        Query.ExtraParameters = "extq=[EventID:" + GoogleCalendarAppointmentModelObj.EventID + "]";
//                        Query.Uri = postUri;
//                        Entry.ExtensionElements.Add(oExtendedProperty);
//                        EventFeed calFeed = CalService.Query(Query);

//                        //if search contains result then update
//                        if (calFeed != null && calFeed.Entries.Count > 0)
//                        {
//                            foreach (EventEntry SearchedEntry in calFeed.Entries)
//                            {
//                                SearchedEntry.Delete();
//                                result = true;
//                                break;
//                            }
//                            //return null;
//                        }
//                    }
//                    //Add if not found OR update if appointment already present on google calendar
//                    else
//                    {
//                        Google.GData.Calendar.EventEntry Entry = new Google.GData.Calendar.EventEntry();
//                        // Set the title and content of the entry.
//                        Entry.Title.Text = "Event generated through google api";

//                        System.Text.StringBuilder EventDetails = new System.Text.StringBuilder();
//                        EventDetails.Append(GoogleCalendarAppointmentModelObj.EventDetails);                        
//                        Entry.Content.Content = EventDetails.ToString();

//                        When EventTime = new When();
//                        EventTime.StartTime = GoogleCalendarAppointmentModelObj.EventStartTime.AddMinutes(-TimeOffset);
//                        EventTime.EndTime = GoogleCalendarAppointmentModelObj.EventEndTime.AddMinutes(-TimeOffset);

//                        Entry.Times.Add(EventTime);

//                        // Set a location for the event.
//                        Where eventLocation = new Where();
//                        if (string.IsNullOrEmpty(GoogleCalendarAppointmentModelObj.EventLocation) == false)
//                        {
//                            eventLocation.ValueString = GoogleCalendarAppointmentModelObj.EventLocation;
//                            Entry.Locations.Add(eventLocation);
//                        }
//                        //Add appointment ID to update/ delete the appointment afterwards
//                        ExtendedProperty oExtendedProperty = new ExtendedProperty();
//                        oExtendedProperty.Name = "EventID";
//                        oExtendedProperty.Value = GoogleCalendarAppointmentModelObj.EventID;
//                        Entry.ExtensionElements.Add(oExtendedProperty);

//                        //search the calendar so to update or add appointment in it
//                        string ThisFeedUri = "http://www.google.com/calendar/feeds/" + CalendarID + "/private/full";
//                        Uri postUri = new Uri(ThisFeedUri);
//                        EventQuery Query = new EventQuery(ThisFeedUri);
//                        Query.ExtraParameters = "extq=[EventID:" + GoogleCalendarAppointmentModelObj.EventID + "]";
//                        Query.Uri = postUri;
//                        Entry.ExtensionElements.Add(oExtendedProperty);
//                        EventFeed calFeed = CalService.Query(Query);

//                        //if search contains result then update
//                        if (calFeed != null && calFeed.Entries.Count > 0)
//                        {
//                            foreach (EventEntry SearchedEntry in calFeed.Entries)
//                            {
//                                SearchedEntry.Content = Entry.Content;
//                                SearchedEntry.Title = Entry.Title;
//                                SearchedEntry.Times.RemoveAt(0);
//                                SearchedEntry.Times.Add(EventTime);
//                                SearchedEntry.Locations.RemoveAt(0);
//                                SearchedEntry.Locations.Add(eventLocation);

//                                CalService.Update(SearchedEntry);
//                                result = true;
//                                break;
//                            }
//                        }
//                        //otherwise add the entry
//                        else
//                        {
//                            InsertedEntry = CalService.Insert(postUri, Entry);
//                            result = true;
//                        }
//                    }
//                }
//            }
//        }
//        return result ;
//    }
//    public static bool RevokeAccessToken(string Access_Token, string Refresh_Token)
//    {
//        try
//        {
//            //check and revoke right from google if staff is registered i.e. he has proper acccess token
//            if (string.IsNullOrEmpty(Access_Token) == false)
//            {
//                string Url = "https://accounts.google.com/o/oauth2/revoke?token=" + Refresh_Token;

//                string redirect_uri_encode = UrlEncodeForGoogle(ReturnUrl);
//                string data = "refresh_token={0}";

//                HttpWebRequest request = HttpWebRequest.Create(Url) as HttpWebRequest;
//                string result = null;
//                request.Method = "POST";
//                request.KeepAlive = true;
//                request.ContentType = "application/x-www-form-urlencoded";
//                string param = string.Format(data, Refresh_Token);
//                var bs = Encoding.UTF8.GetBytes(param);
//                using (Stream reqStream = request.GetRequestStream())
//                {
//                    //reqStream.Write(bs, 0, bs.Length);
//                }

//                using (WebResponse response = request.GetResponse())
//                {
//                    var sr = new StreamReader(response.GetResponseStream());
//                    result = sr.ReadToEnd();
//                    sr.Close();
//                }
//            }

//            return true;
//        }
//        catch
//        {
//            //GoogleCalendarManager.DeleteGoogleAppointmentOAuthDetailsRecord(GoogleAppointmentOAuthDetailsObj.ID);
//            return false;
//        }

//    }

//    #region Utility functions
//    public static string UrlEncodeForGoogle(string url)
//    {

//        string UnReservedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-_.~";
//        var result = new StringBuilder();

//        foreach (char symbol in url)
//        {
//            if (UnReservedChars.IndexOf(symbol) != -1)
//            {
//                result.Append(symbol);
//            }
//            else
//            {
//                result.Append('%' + String.Format("{0:X2}", (int)symbol));
//            }
//        }

//        return result.ToString();
//    }
//    #endregion


//    #endregion

//    #region Other functions which are not used now
//    static void PrintUserCalendars(CalendarService service)
//    {
//        FeedQuery query = new FeedQuery();
//        query.Uri = new Uri("http://www.google.com/calendar/feeds/default");

//        // Tell the service to query:
//        AtomFeed calFeed = service.Query(query);

//        Console.WriteLine("Your calendars:");
//        Console.WriteLine();
//        for (int i = 0; i < calFeed.Entries.Count; i++)
//        {
//            Console.WriteLine(calFeed.Entries[i].Title.Text);
//        }
//        Console.WriteLine();
//    }

//    /// <summary>
//    /// Prints the titles of all events on the specified calendar.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>
//    static void PrintAllEvents(CalendarService service)
//    {
//        EventQuery myQuery = new EventQuery(feedUri);
//        EventFeed myResultsFeed = service.Query(myQuery) as EventFeed;

//        Console.WriteLine("All events on your calendar:");
//        Console.WriteLine();
//        for (int i = 0; i < myResultsFeed.Entries.Count; i++)
//        {
//            Console.WriteLine(myResultsFeed.Entries[i].Title.Text);
//        }
//        Console.WriteLine();
//    }

//    /// <summary>
//    /// Prints the titles of all events matching a full-text query.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>
//    /// <param name="queryString">The text for which to query.</param>
//    static void FullTextQuery(CalendarService service, String queryString)
//    {
//        EventQuery myQuery = new EventQuery(feedUri);
//        myQuery.Query = queryString;

//        EventFeed myResultsFeed = service.Query(myQuery) as EventFeed;

//        Console.WriteLine("Events matching \"{0}\":", queryString);
//        Console.WriteLine();
//        for (int i = 0; i < myResultsFeed.Entries.Count; i++)
//        {
//            Console.WriteLine(myResultsFeed.Entries[i].Title.Text);
//        }
//        Console.WriteLine();
//    }

//    /// <summary>
//    /// Prints the titles of all events in a specified date/time range.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>
//    /// <param name="startTime">Start time (inclusive) of events to print.</param>
//    /// <param name="endTime">End time (exclusive) of events to print.</param>
//    static void DateRangeQuery(CalendarService service, DateTime startTime, DateTime endTime)
//    {
//        EventQuery myQuery = new EventQuery(feedUri);
//        myQuery.StartTime = startTime;
//        myQuery.EndTime = endTime;

//        EventFeed myResultsFeed = service.Query(myQuery) as EventFeed;

//        Console.WriteLine("Matching events from {0} to {1}:",
//                          startTime.ToShortDateString(),
//                          endTime.ToShortDateString());
//        Console.WriteLine();
//        for (int i = 0; i < myResultsFeed.Entries.Count; i++)
//        {
//            Console.WriteLine(myResultsFeed.Entries[i].Title.Text);
//        }
//        Console.WriteLine();
//    }

//    /// <summary>
//    /// Helper method to create either single-instance or recurring events.
//    /// For simplicity, some values that might normally be passed as parameters
//    /// (such as author name, email, etc.) are hard-coded.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>
//    /// <param name="entryTitle">Title of the event to create.</param>
//    /// <param name="recurData">Recurrence value for the event, or null for
//    ///                         single-instance events.</param>
//    /// <returns>The newly-created EventEntry on the calendar.</returns>
//    public static Service GetService(string SessionToken)
//    {
//        GAuthSubRequestFactory authFactory = new GAuthSubRequestFactory("cl", "CalendarSampleApp");
//        authFactory.Token = SessionToken;
//        Service service = new Service("cl", authFactory.ApplicationName);
//        service.RequestFactory = authFactory;

//        return service;
//    }

//    /// <summary>
//    /// Creates a single-instance event on a calendar.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>
//    /// <param name="entryTitle">Title of the event to create.</param>
//    /// <returns>The newly-created EventEntry on the calendar.</returns>
//    public static EventEntry CreateSingleEvent(CalendarService service, String entryTitle)
//    {
//        //return AddUpdateDeleteEvent(service, entryTitle, null, null, null);
//        return new EventEntry();
//    }

//    /// <summary>
//    /// Creates a recurring event on a calendar. In this example, the event
//    /// occurs every Tuesday from May 1, 2007 through September 4, 2007. Note
//    /// that we are using iCal (RFC 2445) syntax; see http://www.ietf.org/rfc/rfc2445.txt
//    /// for more information.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>
//    /// <param name="entryTitle">Title of the event to create.</param>
//    /// <returns>The newly-created EventEntry on the calendar.</returns>
//    static EventEntry CreateRecurringEvent(CalendarService service, String entryTitle)
//    {
//        String recurData =
//          "DTSTART;VALUE=DATE:20070501\r\n" +
//          "DTEND;VALUE=DATE:20070502\r\n" +
//          "RRULE:FREQ=WEEKLY;BYDAY=Tu;UNTIL=20070904\r\n";

//        //return AddUpdateDeleteEvent(service, entryTitle, recurData, null, null);
//        return new EventEntry();
//    }

//    /// <summary>
//    /// Updates the title of an existing calendar event.
//    /// </summary>
//    /// <param name="entry">The event to update.</param>
//    /// <param name="newTitle">The new title for this event.</param>
//    /// <returns>The updated EventEntry object.</returns>
//    static EventEntry UpdateTitle(EventEntry entry, String newTitle)
//    {
//        entry.Title.Text = newTitle;
//        return (EventEntry)entry.Update();
//    }

//    /// <summary>
//    /// Adds a reminder to a calendar event.
//    /// </summary>
//    /// <param name="entry">The event to update.</param>
//    /// <param name="numMinutes">Reminder time, in minutes.</param>
//    /// <returns>The updated EventEntry object.</returns>
//    static EventEntry AddReminder(EventEntry entry, int numMinutes)
//    {
//        Reminder reminder = new Reminder();
//        reminder.Minutes = numMinutes;
//        entry.Reminder = reminder;

//        return (EventEntry)entry.Update();
//    }

//    /// <summary>
//    /// Adds an extended property to a calendar event.
//    /// </summary>
//    /// <param name="entry">The event to update.</param>
//    /// <returns>The updated EventEntry object.</returns>
//    static EventEntry AddExtendedProperty(EventEntry entry)
//    {
//        ExtendedProperty property = new ExtendedProperty();
//        property.Name = "http://www.example.com/schemas/2005#mycal.id";
//        property.Value = "1234";

//        entry.ExtensionElements.Add(property);

//        return (EventEntry)entry.Update();
//    }

//    /// <summary>
//    /// Retrieves and prints the access control lists of all
//    /// of the authenticated user's calendars.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>
//    static void RetrieveAcls(CalendarService service)
//    {
//        FeedQuery query = new FeedQuery();
//        query.Uri = new Uri("http://www.google.com/calendar/feeds/default");
//        AtomFeed calFeed = service.Query(query);

//        Console.WriteLine();
//        Console.WriteLine("Sharing permissions for your calendars:");

//        // Retrieve the meta-feed of all calendars.
//        foreach (AtomEntry calendarEntry in calFeed.Entries)
//        {
//            Console.WriteLine("Calendar: {0}", calendarEntry.Title.Text);
//            AtomLink link = calendarEntry.Links.FindService(
//                AclNameTable.LINK_REL_ACCESS_CONTROL_LIST, null);

//            // For each calendar, retrieve its ACL feed.
//            if (link != null)
//            {
//                AclFeed feed = service.Query(new AclQuery(link.HRef.ToString()));
//                foreach (AclEntry aclEntry in feed.Entries)
//                {
//                    Console.WriteLine("\tScope: Type={0} ({1})", aclEntry.Scope.Type,
//                        aclEntry.Scope.Value);
//                    Console.WriteLine("\tRole: {0}", aclEntry.Role.Value);
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// Shares a calendar with the specified user.  Note that this method
//    /// will not run by default.
//    /// </summary>
//    /// <param name="service">The authenticated CalendarService object.</param>
//    /// <param name="aclFeedUri">the ACL feed URI of the calendar being shared.</param>
//    /// <param name="userEmail">The email address of the user with whom to share.</param>
//    /// <param name="role">The role of the user with whom to share.</param>
//    /// <returns>The AclEntry returned by the server.</returns>
//    static AclEntry AddAccessControl(CalendarService service, string aclFeedUri,
//        string userEmail, AclRole role)
//    {
//        AclEntry entry = new AclEntry();

//        entry.Scope = new AclScope();
//        entry.Scope.Type = AclScope.SCOPE_USER;
//        entry.Scope.Value = userEmail;

//        entry.Role = role;

//        Uri aclUri =
//            new Uri("http://www.google.com/calendar/feeds/gdata.ops.test@gmail.com/acl/full");

//        AclEntry insertedEntry = service.Insert(aclUri, entry);
//        Console.WriteLine("Added user {0}", insertedEntry.Scope.Value);

//        return insertedEntry;
//    }

//    /// <summary>
//    /// Updates a user to have new access permissions over a calendar.
//    /// Note that this method will not run by default.
//    /// </summary>
//    /// <param name="entry">An existing AclEntry representing sharing permissions.</param>
//    /// <param name="newRole">The new role (access permissions) for the user.</param>
//    /// <returns>The updated AclEntry.</returns>
//    static AclEntry UpdateEntry(AclEntry entry, AclRole newRole)
//    {
//        entry.Role = newRole;
//        AclEntry updatedEntry = entry.Update() as AclEntry;

//        Console.WriteLine("Updated {0} to have role {1}", updatedEntry.Scope.Value,
//            entry.Role.Value);
//        return updatedEntry;
//    }

//    /// <summary>
//    /// Deletes a user from a calendar's access control list, preventing
//    /// that user from accessing the calendar.  Note that this method will
//    /// not run by default.
//    /// </summary>
//    /// <param name="entry">An existing AclEntry representing sharing permissions.</param>
//    static void DeleteEntry(AclEntry entry)
//    {
//        entry.Delete();
//    }
//    #endregion
//}

