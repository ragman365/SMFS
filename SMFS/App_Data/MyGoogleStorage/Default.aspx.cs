using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Web;
using Google.Apis.Calendar.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

namespace GoogleCalendarTest
{
    public partial class Default : System.Web.UI.Page
    {
        XmlDocument XmlDoc = new XmlDocument();

        static string UserID = "UserID";
        static string UserName = "UserName";
        static string Password = "Password";
        static string AccessToken = "AccessToken";
        static string RefreshToken = "RefreshToken";

        private static string calID = "m.waqasiqbal@gmail.com"; //System.Configuration.ConfigurationManager.AppSettings["GoogleCalendarID"].ToString()
        private static string UserId = "m.waqasiqbal"; //System.Web.HttpContext.Current.User.Identity.Name
        private static string gFolder = System.Web.HttpContext.Current.Server.MapPath("/App_Data/MyGoogleStorage");

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Request.QueryString["state"] != null)
                {
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
                }

                try
                {
                    if (XmlDoc.HasChildNodes == false)
                    {
                        XmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");
                    }
                    //check if the user has logged in
                    if (Session[UserID] != null && string.IsNullOrEmpty(Session[UserID].ToString()) == false)
                    {
                        //if the user is registered
                        if (string.IsNullOrEmpty(XmlDoc.DocumentElement.ChildNodes[0].Attributes[AccessToken].Value.ToString()) == false)
                        {
                            PnlLogin.Visible = false;
                            PnlRegister.Visible = false;
                            PnlEvents.Visible = true;

                            TxtStartTime.Text = DateTime.Now.ToString();
                            TxtEndTime.Text = DateTime.Now.AddHours(2).ToString();

                            //check if event is created or not
                            if (XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventTitle"].Value == "")
                            {
                                BtnDeleteEvent.Enabled = false;
                            }
                            else
                            {
                                TxtTitle.Text = XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventTitle"].Value;
                                TxtStartTime.Text = XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventStartTime"].Value;
                                TxtEndTime.Text = XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventEndTime"].Value;
                                TxtEventDetails.Text = XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventDetails"].Value;

                                BtnDeleteEvent.Enabled = true;
                            }
                        }
                        else
                        {
                            PnlLogin.Visible = false;
                            PnlRegister.Visible = true;
                            PnlEvents.Visible = false;
                        }
                    }

                    else
                    {
                        PnlLogin.Visible = true;
                        PnlRegister.Visible = false;
                        PnlEvents.Visible = false;

                        LblMessage.Text = "Please login";
                    }
                }
                catch { }
            }
            //else
            //{
            //    BtnCreateUpdateEvent_Click(null, null);
            //}
        }
        protected void BtnLogin_Click(object sender, EventArgs e)
        {
            if (XmlDoc.HasChildNodes == false)
            {
                XmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");
            }

            //check the user credentials
            if (string.IsNullOrEmpty(XmlDoc.DocumentElement.ChildNodes[0].Attributes[UserName].Value.ToString()) == false
             && XmlDoc.DocumentElement.ChildNodes[0].Attributes[UserName].Value.ToLower() == TxtBoxUserName.Text.ToLower()
             && string.IsNullOrEmpty(XmlDoc.DocumentElement.ChildNodes[0].Attributes[Password].Value.ToString()) == false
             && XmlDoc.DocumentElement.ChildNodes[0].Attributes[Password].Value.ToLower() == TxtBoxPwd.Text.ToLower())
            {
                Session[UserID] = XmlDoc.DocumentElement.ChildNodes[0].Attributes[UserID].Value.ToLower();

                //check if the user is registered or not
                //if not then ask the user to register

                if (string.IsNullOrEmpty(XmlDoc.DocumentElement.ChildNodes[0].Attributes[AccessToken].Value.ToString()) == false ||
                string.IsNullOrEmpty(XmlDoc.DocumentElement.ChildNodes[0].Attributes[RefreshToken].Value.ToString()) == false)
                {
                    PnlLogin.Visible = false;
                    PnlRegister.Visible = true;
                    PnlEvents.Visible = false;
                }

                else if (XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventTitle"].Value == "")
                {
                    PnlLogin.Visible = false;
                    PnlRegister.Visible = false;
                    PnlEvents.Visible = true;

                    TxtStartTime.Text = DateTime.Now.ToString();
                    TxtEndTime.Text = DateTime.Now.AddHours(2).ToString();

                    LblMessage.Text = "Login Successful. Please click the register button to register with google calendar ";
                }

                else
                {
                    PnlLogin.Visible = false;
                    PnlRegister.Visible = false;
                    PnlEvents.Visible = true;

                    //check if event is created or not
                    if (XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventTitle"].Value == "")
                    {
                        BtnDeleteEvent.Enabled = false;
                        LblMessage.Text = "You can create new event now";
                    }
                    else
                    {
                        TxtTitle.Text = XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventTitle"].Value;
                        TxtStartTime.Text = XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventStartTime"].Value;
                        TxtEndTime.Text = XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventEndTime"].Value;
                        TxtEventDetails.Text = XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventDetails"].Value;

                        BtnDeleteEvent.Enabled = true;

                        LblMessage.Text = "You can edit / delete this event now";
                    }
                    //PopulateDDLEvent();
                }
            }
            else
            {
                LblMessage.Text = "Login UnSuccessful. Please provide the correct credentials";
            }
        }
        protected void BtnRegisterWithGoogleCalendar_Click(object sender, EventArgs e)
        {
            CalendarService service = null;
            string GoogleReturnPageAddress = System.Configuration.ConfigurationManager.AppSettings["GoogleReturnPageAddress"];

            IAuthorizationCodeFlow flow = new GoogleAuthorizationCodeFlow(
            new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = GetClientConfiguration().Secrets,
                DataStore = new FileDataStore(gFolder),
                Scopes = new[] { CalendarService.Scope.Calendar }
            });

            var uri = System.Web.HttpContext.Current.Request.Url.ToString();
            var result = new AuthorizationCodeWebApp(flow, uri, uri).AuthorizeAsync(UserId, CancellationToken.None).Result;
            if (result.RedirectUri != null)
            {
                // Redirect the user to the authorization server.
                System.Web.HttpContext.Current.Response.Redirect(result.RedirectUri);
                //var page = System.Web.HttpContext.Current.CurrentHandler as Page;
                //page.ClientScript.RegisterClientScriptBlock(page.GetType(),
                //    "RedirectToGoogleScript", "window.top.location = '" + result.RedirectUri + "'", true);
            }
            else if (result.Credential != null)
            {
                // The data store contains the user credential, so the user has been already authenticated.
                //service = new CalendarService(new BaseClientService.Initializer
                //{
                //    ApplicationName = "My ASP.NET Google Calendar App",
                //    HttpClientInitializer = result.Credential
                //});

                PnlLogin.Visible = false;
                PnlRegister.Visible = false;
                PnlEvents.Visible = true;

                TxtStartTime.Text = DateTime.Now.ToString();
                TxtEndTime.Text = DateTime.Now.AddHours(2).ToString();

            }
            //Response.Redirect(GoogleCalendarManager.GenerateGoogleOAuthURL());
        }

        public static GoogleClientSecrets GetClientConfiguration()
        {
            using (var stream = new FileStream(gFolder + @"\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                return GoogleClientSecrets.Load(stream);
            }
        }

        protected void BtnRevoke_Click(object sender, EventArgs e)
        {
            if (XmlDoc.HasChildNodes == false)
            {
                XmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");
            }

            string Access_Token = XmlDoc.DocumentElement.SelectSingleNode("//User[@UserID='1']").Attributes[AccessToken].Value;
            string Refresh_Token = XmlDoc.DocumentElement.SelectSingleNode("//User[@UserID='1']").Attributes[RefreshToken].Value;

            //Attempt the revoke from google
            //if successful then do a db / xml delete as well
            //if (GoogleCalendarManager.RevokeAccessToken(Access_Token, Refresh_Token) == true)
            //{
            //    XmlDoc.DocumentElement.SelectSingleNode("//User[@UserID='1']").Attributes[AccessToken].Value = "";
            //    XmlDoc.DocumentElement.SelectSingleNode("//User[@UserID='1']").Attributes[RefreshToken].Value = "";

            //    XmlDoc.Save(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");

            //    LblMessage.Text = "Rights revoked successfully.You can <a href='https://accounts.google.com/b/0/IssuedAuthSubTokens?hl=en' target='blank'>view</a> that you gmail account is not linked with your calendar application anymore";

            //    PnlLogin.Visible = false;
            //    PnlRegister.Visible = true;
            //    PnlEvents.Visible = false;

            //}
        }
        protected void BtnCreateUpdateEvent_Click(object sender, EventArgs e)
        {
            List<GoogleCalendarAppointmentModel> GoogleCalendarAppointmentModelList = new List<GoogleCalendarAppointmentModel>();
            List<GoogleTokenModel> GoogleTokenModelList = new List<GoogleTokenModel>();

            GoogleCalendarAppointmentModel GoogleCalendarAppointmentModelObj = new GoogleCalendarAppointmentModel();
            GoogleTokenModel GoogleTokenModelObj = new GoogleTokenModel();

            #region populate GoogleAppointment values

            GoogleCalendarAppointmentModelObj.EventID = "1";
            GoogleCalendarAppointmentModelObj.EventTitle = string.IsNullOrEmpty(TxtTitle.Text) == false ? TxtTitle.Text : "New Event from google api";
            GoogleCalendarAppointmentModelObj.EventStartTime = DateTime.Now;
            GoogleCalendarAppointmentModelObj.EventEndTime.AddHours(2);
            //Giving the proper location so you can view on the map in google calendar
            GoogleCalendarAppointmentModelObj.EventLocation = "Johar Town, Lahore, Pakistan";
            GoogleCalendarAppointmentModelObj.EventDetails = string.IsNullOrEmpty(TxtEventDetails.Text) == false ? TxtEventDetails.Text : "New Details";
            GoogleCalendarAppointmentModelList.Add(GoogleCalendarAppointmentModelObj);
            #endregion

            #region populate GoogleToken values

            if (XmlDoc.HasChildNodes == false)
            {
                XmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");
            }

            GoogleTokenModelObj.Access_Token = XmlDoc.DocumentElement.SelectSingleNode("//User[@UserID='1']").Attributes[AccessToken].Value;
            GoogleTokenModelObj.Refresh_Token = XmlDoc.DocumentElement.SelectSingleNode("//User[@UserID='1']").Attributes[RefreshToken].Value;
            GoogleTokenModelList.Add(GoogleTokenModelObj);

            #endregion
            #region Add event to google calendar

            if (GoogleCalendarManager.AddUpdateDeleteEvent(GoogleCalendarAppointmentModelList, GoogleTokenModelList, 0) == true)
            {
                if (XmlDoc.HasChildNodes == false)
                {
                    XmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");
                }

                //save data in DB / xml
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventTitle"].Value = GoogleCalendarAppointmentModelObj.EventTitle;
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventStartTime"].Value = GoogleCalendarAppointmentModelObj.EventStartTime.ToString();
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventEndTime"].Value = GoogleCalendarAppointmentModelObj.EventEndTime.ToString();
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventLocation"].Value = GoogleCalendarAppointmentModelObj.EventLocation;
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventDetails"].Value = GoogleCalendarAppointmentModelObj.EventDetails;

                XmlDoc.Save(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");

                LblMessage.Text = "Event Created / updated successfully. Go to <a href='https://www.google.com/calendar/' target='blank'>Google Calendar</a> to view your event ";
                BtnDeleteEvent.Enabled = true;
            }
            #endregion

        }
        //private void PopulateDDLEvent()
        //{
        //    if (XmlDoc.HasChildNodes == false)
        //    {
        //        XmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");
        //    }
        //    XmlNodeList AllEventNodes = XmlDoc.DocumentElement.SelectNodes("//Events/Event");
        //    if (AllEventNodes != null && AllEventNodes.Count > 0)
        //    {
        //        foreach (XmlNode XmlNodeObj in AllEventNodes)
        //        {
        //            DDLEvent.Items.Add(new ListItem(XmlNodeObj.Attributes["EventTitle"].Value, XmlNodeObj.Attributes["EventID"].Value));
        //        }
        //    }
        //}
        //protected void DDLEvent_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    if (DDLEvent.SelectedIndex != 0)
        //    {
        //        HdnEventID.Value = DDLEvent.SelectedValue;
        //    }
        //}
        protected void BtnDeleteEvent_Click(object sender, EventArgs e)
        {
            List<GoogleCalendarAppointmentModel> GoogleCalendarAppointmentModelList = new List<GoogleCalendarAppointmentModel>();
            List<GoogleTokenModel> GoogleTokenModelList = new List<GoogleTokenModel>();

            GoogleCalendarAppointmentModel GoogleCalendarAppointmentModelObj = new GoogleCalendarAppointmentModel();
            GoogleTokenModel GoogleTokenModelObj = new GoogleTokenModel();

            #region populate GoogleAppointment values
            GoogleCalendarAppointmentModelObj.EventID = "1";
            GoogleCalendarAppointmentModelObj.DeleteAppointment = true;
            GoogleCalendarAppointmentModelList.Add(GoogleCalendarAppointmentModelObj);
            #endregion
            #region populate GoogleToken values

            if (XmlDoc.HasChildNodes == false)
            {
                XmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");
            }

            GoogleTokenModelObj.Access_Token = XmlDoc.DocumentElement.SelectSingleNode("//User[@UserID='1']").Attributes[AccessToken].Value;
            GoogleTokenModelObj.Refresh_Token = XmlDoc.DocumentElement.SelectSingleNode("//User[@UserID='1']").Attributes[RefreshToken].Value;
            GoogleTokenModelList.Add(GoogleTokenModelObj);

            #endregion

            if (GoogleCalendarManager.AddUpdateDeleteEvent(GoogleCalendarAppointmentModelList, GoogleTokenModelList, 0) == true)
            {
                if (XmlDoc.HasChildNodes == false)
                {
                    XmlDoc.Load(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");
                }

                //save data in DB / xml
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventTitle"].Value = "";
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventStartTime"].Value = "";
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventEndTime"].Value = "";
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventLocation"].Value = "";
                XmlDoc.DocumentElement.SelectSingleNode("//Event").Attributes["EventDetails"].Value = "";

                XmlDoc.Save(AppDomain.CurrentDomain.BaseDirectory + "App_Data\\XMLfile.xml");

                LblMessage.Text = "Event deleted successfully. Go to <a href='https://www.google.com/calendar/' target='blank'>Google Calendar</a> to view your event ";

                BtnDeleteEvent.Enabled = false;
            }

        }
    }
}