using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace event_monitor
{
    class Monitored_event
    {
        public long Id { get; set; }
        public char Kind { get; set; }
        private int alert_tshd_time = 0; 
        public int Alert_tshd_time 
        { 
            get 
            {
                return alert_tshd_time;
            }

            set
            {
                if (value > 0) alert_tshd_time = value;
            }
        }

        private int alert_tshd_num = 0; 
        public int Alert_tshd_num 
        { 
            get 
            {
                return alert_tshd_num;
            }

            set
            {
                if (value > 0) alert_tshd_num = value;
            }
        }

        public string Description { get; set; }

        public Monitored_event(long _id,char _kind,int _alert_tshd_time,int _alert_tshd_num,string _description)
        {
            Id = _id;
            Kind = _kind;
            Alert_tshd_time = _alert_tshd_time;
            Alert_tshd_num = _alert_tshd_num;
            Description = _description;
        }

        public void write_event(string _ofile)
        {
            using StreamWriter sw = File.AppendText(_ofile);

            sw.WriteLine(
                Id + ","
                + Kind + ","
                + Alert_tshd_time + ","
                + Alert_tshd_num + ","
                + Description + ",");
        }

        public override string ToString()
        {
            return "Event ID: " + Id + "\n"
                + "Event Type: " + ((Kind == 'o') ? "One Off" : "Cluster")+ "\n"
                + "Alert_threshold_time (sec): " +  Alert_tshd_time + "\n"
                + "Alert_threshold_num: " + Alert_tshd_num + "\n"
                + "Event Description: " + Description;
        }
    }

    class Monitored_events
    {
        public List<Monitored_event> Events { get; set; }

        private Monitored_event parse_event(string evt)
        {
            long id = 0;
            char kind = 'o';
            int alert_tshd_time = 0;
            int alert_tshd_num = 0;
            string description = "";
            string[] val = evt.Split(',');

            for(int i = 0;i < val.Length;i++)
            {
                if(i == 0 && !String.IsNullOrEmpty(val[0]))
                {
                    id = long.Parse(val[0]);
                }

                if(i == 1 && !String.IsNullOrEmpty(val[1]))
                {
                    if(val[1].Length < 2)
                    {
                        kind = Convert.ToChar(val[1]);
                    }
                }

                if(i == 2 && !String.IsNullOrEmpty(val[2]))
                {
                    alert_tshd_time = Convert.ToInt32(val[2]);
                }

                if(i == 3 && !String.IsNullOrEmpty(val[3]))
                {
                    alert_tshd_num = Convert.ToInt32(val[3]);
                }

                if(i == 4 && !String.IsNullOrEmpty(val[4]))
                {
                    description = val[4];
                }
            }

            return new Monitored_event(id,kind,alert_tshd_time,alert_tshd_num,description);
        }

        public void read_events(string _ifile)
        {
            if(File.Exists(_ifile))
            {
                using(StreamReader sr = new StreamReader(_ifile))
                {
                    string line = "";
                    while((line = sr.ReadLine()) != null)
                    {
                        Events.Add(parse_event(line));
                    }

                    sr.Close();
                }
            }
        }

        public void write_events(string _ofile)
        {
            using StreamWriter sw = new StreamWriter(_ofile);

            foreach(Monitored_event evt in Events)
            {
                sw.WriteLine(
                    evt.Id + ","
                    + evt.Kind + ","
                    + evt.Alert_tshd_time + ","
                    + evt.Alert_tshd_num + ","
                    + evt.Description + ",");
            }

            sw.Close();
        }

        public Monitored_events()
        {
            Events = new List<Monitored_event>();
        }
    }
}
