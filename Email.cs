using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text;

namespace event_monitor
{
    static class Email
    {
        public static bool send_email(string _email_server,string sender,string recipient,string message,string subject = "")
        {
            string email_server = _email_server;
            MailMessage email = new MailMessage(sender,recipient);
            email.Subject = subject;
            email.Body = message;
            if(!String.IsNullOrEmpty(subject)) email.Subject = subject;
            SmtpClient client = new SmtpClient(email_server);
            client.UseDefaultCredentials = true;

            try
            {
                client.Send(email);
                return true;
            }catch(Exception e){
                Console.WriteLine("Error sending email: " + e);
                return false;
            }
        }
    }
}
