using System;
using System.Data;
//using RAGSpread;
using GeneralLib;
using System.Net.Mail;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
/****************************************************************************/
namespace SMFS
{
    public partial class RemoteProcessing
    {
        public RemoteProcessing()
        {
            //            MessageBox.Show("***REMOTE***");
            G1.AddToAudit("System", "AutoRun", "remote_processing", "Starting Remote . . . . . . . ", "");
            string cmd = "Select * from `remote_processing`;";
            DataTable dt = G1.get_db_data(cmd);
            string report = "";
            DateTime date = DateTime.Now;
            int dayToRun = 0;
            int presentDay = date.Day;
            string status = "";
            bool foundReport = false;
            string frequency = "";
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                status = dt.Rows[i]["status"].ObjToString();
                if (status.ToUpper() == "INACTIVE")
                    continue;
                dayToRun = dt.Rows[i]["day_to_run"].ObjToInt32();
                frequency = dt.Rows[i]["dateIncrement"].ObjToString();
                if (frequency.ToUpper() == "MONTHLY")
                {
                    if (presentDay != dayToRun)
                        continue;
                }
                else if (frequency.ToUpper() == "WEEKLY")
                {
                }
                if (!foundReport)
                {
                    G1.AddToAudit("System", "AutoRun", "AutoRun", "Started Reports", "");
                    foundReport = true;
                }
                report = dt.Rows[i]["report"].ObjToString();
                if (report.ToUpper() == "POTENTIAL LAPSE")
                {
                    PastDue pastForm = new PastDue(true, false, "Potential Lapse Report (3.0)");
                    continue;
                }
                if (report.ToUpper() == "REINSTATEMENT REPORT")
                {
                    PaymentsReport paymentForm = new PaymentsReport(true, false, "Reinstatement Report", "Reinstatement Report (5.0)");
                    continue;
                }
                if (report.ToUpper() == "PAID-OUT REPORT")
                {
                    PaymentsReport paymentForm = new PaymentsReport(true, false, "Paid Up Contracts Report", "Trust Paid Off Contracts (2.0)");
                    continue;
                }
                if (report.ToUpper() == "LAPSE REPORT")
                {
                    Trust85 trustForm = new Trust85(true, false );
                    continue;
                }
                else if (report.ToUpper() == "DEBIT AND CREDIT REPORT")
                {
                    DebitsAndCredits debitForm = new DebitsAndCredits(true, false, "Debits and Credits Report");
                    continue;
                }
                else if (report.ToUpper() == "TRUST SUMMARY")
                {
                    TrustSummary trustForm = new TrustSummary( true, false );
                    continue;
                }
                else if (report.ToUpper() == "INSURANCE SUMMARY")
                {
                    InsuranceSummary trustForm = new InsuranceSummary(true, false);
                    continue;
                }
                else if (report.ToUpper() == "FUNERAL ACTIVITY REPORT")
                {
                    //G1.AddToAudit("System", "AutoRun", "Funeral Activity Report", "Starting Report . . . . . . . ", "");
                    FuneralActivityReport funeralForm = new FuneralActivityReport(true, true);
                    continue;
                }
            }
        }
        /***********************************************************************************************/
        public static void AutoRunSend(string title, string filename, string sendTo, string sendWhere, string da = "", string emailLocations = "")
        {
            if (String.IsNullOrWhiteSpace(sendTo))
                return;
            if (String.IsNullOrWhiteSpace(sendWhere))
                return;
            string[] Lines = sendTo.Split('~');
            string userName = "";
            string name = "";
            int idx = 0;
            DataTable dt = null;
            string cmd = "";
            string email = "";
            bool doLocation = false;
            string[] xLines = null;
            //MessageBox.Show("Auto Sent To");
            for (int i = 0; i < Lines.Length; i++)
            {
                doLocation = false;
                name = Lines[i].Trim();
                if (name.ToUpper() == "LOCATION")
                {
                    name = "(" + name + ")";
                    doLocation = true;
                }
                idx = name.IndexOf(")");
                if (idx > 0)
                {
                    userName = name.Substring(0, idx);
                    userName = userName.Replace("(", "");
                    userName = userName.Replace(")", "");
                    cmd = "Select * from `users` where `userName` = 'SMFS';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count < 0)
                        break;
                    string fromRecord = dt.Rows[0]["record"].ObjToString();
                    da = "cvtncquxnwjllljk";
                    da = "hranncwgetlvkxoi";
                    if (doLocation)
                    {
                        if (!String.IsNullOrWhiteSpace(emailLocations))
                        {
                            xLines = emailLocations.Split(';');
                            for (int k = 0; k < xLines.Length; k++)
                            {
                                email = xLines[k].Trim();
                                if (!String.IsNullOrWhiteSpace(email))
                                    SendEmailToSomewhere(title, filename, email, da);
                            }
                        }
                        continue;
                    }
                    cmd = "Select * from `users` where `userName` = '" + userName + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        email = dt.Rows[0]["email"].ObjToString();
                        //MessageBox.Show("AutoRun Send to Email " + email + "Send Where = " + sendWhere);
                        if ((sendWhere.ToUpper() == "BOTH" || sendWhere.ToUpper() == "LOCAL") && !doLocation)
                        {
                            string record = G1.create_record("messages", "fromUser", "-1");
                            if (G1.BadRecord("messages", record))
                                continue;
                            DateTime sendDate = DateTime.Now;
                            string toRecord = dt.Rows[0]["record"].ObjToString();
                            string localFile = filename;
                            localFile = localFile.Replace("\\", "/");
                            G1.update_db_table("messages", "record", record, new string[] { "fromUser", "SMFS", "toUser", sendTo, "subject", title, "message", "AutoRun Report", "senddate", sendDate.ToString("MM/dd/yyyy"), "fromRecord", fromRecord, "toRecord", toRecord, "attachment", "Y", "filename", localFile });
                        }

                        if (sendWhere.ToUpper() == "BOTH" || sendWhere.ToUpper() == "EMAIL")
                        {
                            SendEmailToSomewhere(title, filename, email, da);
                            ////MessageBox.Show("AutoRun Starting Email Send to");
                            //string from = "robbyxyzzy@gmail.com";
                            //string pw = "Xyzzy@0483";
                            //pw = "xkiypozlptspspwr";
                            //string to = email;
                            //string subject = title;
                            //string body = title + " Generated";

                            //string senderID = from;
                            //string senderPassword = pw;
                            //if (String.IsNullOrWhiteSpace(from))
                            //{
                            //    //MessageBox.Show("***ERROR*** Email From Address is empty!");
                            //    return;
                            //}
                            //if (String.IsNullOrWhiteSpace(pw))
                            //{
                            //    //MessageBox.Show("***ERROR*** Email PW is empty!");
                            //    return;
                            //}
                            //RemoteCertificateValidationCallback orgCallback = ServicePointManager.ServerCertificateValidationCallback;
                            ////            string body = "Test";
                            //try
                            //{
                            //    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnValidateCertificate);
                            //    ServicePointManager.Expect100Continue = false;
                            //    MailMessage mail = new MailMessage();

                            //    mail.To.Add(email);

                            //    mail.From = new MailAddress(senderID);
                            //    mail.Subject = subject;
                            //    mail.Body = body;
                            //    mail.IsBodyHtml = true;
                            //    mail.Attachments.Add(new Attachment(filename));
                            //    SmtpClient smtp = new SmtpClient();
                            //    smtp.Host = "smtp.gmail.com";
                            //    smtp.Port = 587;
                            //    smtp.EnableSsl = true;
                            //    smtp.Credentials = new System.Net.NetworkCredential(senderID, senderPassword);
                            //    smtp.Send(mail);
                            //    string audit = "Sent to " + email + " Successful";
                            //    G1.AddToAudit("System", title, "AutoRun", audit, "");

                            //    //MessageBox.Show("Email Sent Successfully");
                            //    //                Console.WriteLine("Email Sent Successfully");
                            //}
                            //catch (Exception ex)
                            //{
                            //    string audit = "Sent to " + email + " " + ex.Message.ToString();
                            //    G1.AddToAudit("System", title, "AutoRun", audit, "");
                            //    //MessageBox.Show("***ERROR*** Email Unsuccessful\n\n" + ex.Message.ToString());
                            //    //                Console.WriteLine(ex.Message);
                            //}
                            //finally
                            //{
                            //    ServicePointManager.ServerCertificateValidationCallback = orgCallback;
                            //}
                        }
                    }
                }
            }
        }
        /***********************************************************************************************/
        public static void AutoRunSendTo(string title, string filename, string sendTo, string sendWhere, string da = "", string emailLocations = "")
        {
            if (String.IsNullOrWhiteSpace(sendTo))
                return;
            if (String.IsNullOrWhiteSpace(sendWhere))
                return;
            string[] Lines = sendTo.Split('~');
            string userName = "";
            string name = "";
            int idx = 0;
            DataTable dt = null;
            string cmd = "";
            string email = "";
            bool doLocation = false;
            string[] xLines = null;
            //MessageBox.Show("Auto Sent To");
            for (int i = 0; i < Lines.Length; i++)
            {
                doLocation = false;
                name = Lines[i].Trim();
                if (name.ToUpper() == "LOCATION")
                {
                    name = "(" + name + ")";
                    doLocation = true;
                }
                idx = name.IndexOf(")");
                if (idx > 0)
                {
                    userName = name.Substring(0, idx);
                    userName = userName.Replace("(", "");
                    userName = userName.Replace(")", "");
                    cmd = "Select * from `users` where `userName` = 'robby';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count < 0)
                        break;
                    string fromRecord = dt.Rows[0]["record"].ObjToString();
                    if ( doLocation )
                    {
                        if (!String.IsNullOrWhiteSpace(emailLocations))
                        {
                            xLines = emailLocations.Split(';');
                            for ( int k=0; k<xLines.Length; k++)
                            {
                                email = xLines[k].Trim();
                                if ( !String.IsNullOrWhiteSpace ( email))
                                    SendEmailToSomewhere(title, filename, email, da );
                            }
                        }
                        continue;
                    }
                    cmd = "Select * from `users` where `userName` = '" + userName + "';";
                    dt = G1.get_db_data(cmd);
                    if (dt.Rows.Count > 0)
                    {
                        email = dt.Rows[0]["email"].ObjToString();
                        //MessageBox.Show("AutoRun Send to Email " + email + "Send Where = " + sendWhere);
                        if ((sendWhere.ToUpper() == "BOTH" || sendWhere.ToUpper() == "LOCAL") && !doLocation)
                        {
                            string record = G1.create_record("messages", "fromUser", "-1");
                            if (G1.BadRecord("messages", record))
                                continue;
                            DateTime sendDate = DateTime.Now;
                            string toRecord = dt.Rows[0]["record"].ObjToString();
                            string localFile = filename;
                            localFile = localFile.Replace("\\", "/");
                            G1.update_db_table("messages", "record", record, new string[] { "fromUser", "Robby", "toUser", sendTo, "subject", title, "message", "AutoRun Report", "senddate", sendDate.ToString("MM/dd/yyyy"), "fromRecord", fromRecord, "toRecord", toRecord, "attachment", "Y", "filename", localFile });
                        }

                        if (sendWhere.ToUpper() == "BOTH" || sendWhere.ToUpper() == "EMAIL")
                        {
                            SendEmailToSomewhere(title, filename, email, da );
                            ////MessageBox.Show("AutoRun Starting Email Send to");
                            //string from = "robbyxyzzy@gmail.com";
                            //string pw = "Xyzzy@0483";
                            //pw = "xkiypozlptspspwr";
                            //string to = email;
                            //string subject = title;
                            //string body = title + " Generated";

                            //string senderID = from;
                            //string senderPassword = pw;
                            //if (String.IsNullOrWhiteSpace(from))
                            //{
                            //    //MessageBox.Show("***ERROR*** Email From Address is empty!");
                            //    return;
                            //}
                            //if (String.IsNullOrWhiteSpace(pw))
                            //{
                            //    //MessageBox.Show("***ERROR*** Email PW is empty!");
                            //    return;
                            //}
                            //RemoteCertificateValidationCallback orgCallback = ServicePointManager.ServerCertificateValidationCallback;
                            ////            string body = "Test";
                            //try
                            //{
                            //    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnValidateCertificate);
                            //    ServicePointManager.Expect100Continue = false;
                            //    MailMessage mail = new MailMessage();

                            //    mail.To.Add(email);

                            //    mail.From = new MailAddress(senderID);
                            //    mail.Subject = subject;
                            //    mail.Body = body;
                            //    mail.IsBodyHtml = true;
                            //    mail.Attachments.Add(new Attachment(filename));
                            //    SmtpClient smtp = new SmtpClient();
                            //    smtp.Host = "smtp.gmail.com";
                            //    smtp.Port = 587;
                            //    smtp.EnableSsl = true;
                            //    smtp.Credentials = new System.Net.NetworkCredential(senderID, senderPassword);
                            //    smtp.Send(mail);
                            //    string audit = "Sent to " + email + " Successful";
                            //    G1.AddToAudit("System", title, "AutoRun", audit, "");

                            //    //MessageBox.Show("Email Sent Successfully");
                            //    //                Console.WriteLine("Email Sent Successfully");
                            //}
                            //catch (Exception ex)
                            //{
                            //    string audit = "Sent to " + email + " " + ex.Message.ToString();
                            //    G1.AddToAudit("System", title, "AutoRun", audit, "");
                            //    //MessageBox.Show("***ERROR*** Email Unsuccessful\n\n" + ex.Message.ToString());
                            //    //                Console.WriteLine(ex.Message);
                            //}
                            //finally
                            //{
                            //    ServicePointManager.ServerCertificateValidationCallback = orgCallback;
                            //}
                        }
                    }
                }
            }
        }
        /****************************************************************************/
        public static bool OnValidateCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
        /****************************************************************************/
        public static void SendEmailToSomewhere( string title, string filename, string email, string da, string extraBody = "" )
        {
            //MessageBox.Show("AutoRun Starting Email Send to");
            string from = "robbyxyzzy@gmail.com";
            string pw = "Xyzzy@0483";
            pw = "xkiypozlptspspwr";
            pw = da;
            string to = email;
            string subject = title;
            string body = title + " Generated";
            if (!String.IsNullOrWhiteSpace(extraBody))
                body = extraBody;

            string senderID = from;
            string senderPassword = pw;
            if (String.IsNullOrWhiteSpace(from))
            {
                //MessageBox.Show("***ERROR*** Email From Address is empty!");
                return;
            }
            if (String.IsNullOrWhiteSpace(pw))
            {
                //MessageBox.Show("***ERROR*** Email PW is empty!");
                return;
            }
            RemoteCertificateValidationCallback orgCallback = ServicePointManager.ServerCertificateValidationCallback;
            //            string body = "Test";
            try
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(OnValidateCertificate);
                ServicePointManager.Expect100Continue = false;
                MailMessage mail = new MailMessage();

                mail.To.Add(email);

                mail.From = new MailAddress(senderID);
                mail.Subject = subject;
                mail.Body = body;
                mail.IsBodyHtml = true;
                if ( !String.IsNullOrWhiteSpace ( filename))
                    mail.Attachments.Add(new Attachment(filename));
                SmtpClient smtp = new SmtpClient();
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.Credentials = new System.Net.NetworkCredential(senderID, senderPassword);
                smtp.Send(mail);
                string audit = "Sent to " + email + " Successful";
                G1.AddToAudit("System", title, "AutoRun", audit, "");

                //MessageBox.Show("Email Sent Successfully");
                //                Console.WriteLine("Email Sent Successfully");
            }
            catch (Exception ex)
            {
                string audit = "Sent to " + email + " " + ex.Message.ToString();
                G1.AddToAudit("System", title, "AutoRun", audit, "");
                //MessageBox.Show("***ERROR*** Email Unsuccessful\n\n" + ex.Message.ToString());
                //                Console.WriteLine(ex.Message);
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback = orgCallback;
            }
        }
        /****************************************************************************/
    }
}
