using System;
using System.Collections.Generic;
using System.Text;

namespace event_monitor
{
    class Report
    {
        public string Body { get;set; }
        
        public Report(string _body)
        {
            Body = _body;
        }
                       
        public bool email_alert(string _email_server,string _sender,string _recipient)
        {
            string computer = Environment.MachineName;
            string email_server = _email_server;
            string sender = _sender;
            string recipient = _recipient;
            string body = Body;
            
            string subject = "WARNING: UNUSUAL LOG ACTIVITY FOUND ON: " + computer;

            if(Email.send_email(email_server,sender,recipient,body,subject)) return true;
               
            return false;
        }
    }
}
