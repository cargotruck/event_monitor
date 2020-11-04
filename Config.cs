using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace event_monitor
{
    class Config
    {
        private string Config_path = "options.config";
        public string Email_server { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public DateTime Last_run { get; set; }

        public Config()
        {
            Email_server = "mail.yourdomain.com";
            Sender = "alerts@yourdomain.com";
            Recipient = "itadmin@yourdomain.com";

            if(File.Exists(Config_path)) read_config();
        }

        public void read_config()
        {
            if(File.Exists(Config_path))
            {
                using(StreamReader sr = new StreamReader(Config_path))
                {
                    string line = "";
                    while((line = sr.ReadLine()) != null)
                    {
                        string[] val = line.Split();
                        if(val[0] == "email_server:")
                        {
                            Email_server = val[1];
                        }

                        if(val[0] == "sender:")
                        {
                            Sender = val[1];
                        }

                        if(val[0] == "recipient:")
                        {
                            Recipient = val[1];
                        }
                        
                        if(val[0] == "last_run:")
                        {
                            Last_run = DateTime.ParseExact(val[1],"yyyyMMddHHmmss",null);
                        }
                    }
                    sr.Close();
                }
            }
        }
        
        public void write_config()
        {
            StreamWriter sw = new StreamWriter(Config_path);
            sw.WriteLine(ToString());
            sw.Close();
        }

        public void update_last_run()
        {
            write_config();
        }

        public override string ToString()
        {
            return "email_server: " + Email_server + "\n" 
                    + "sender: " + Sender + "\n"
                    + "recipient: " + Recipient + "\n"
                    + "last_run: " + DateTime.Now.ToString("yyyyMMddHHmmss");
        }
    }
}
