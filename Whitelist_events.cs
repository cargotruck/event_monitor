using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace event_monitor
{
    class Whitelist_event
    {
        public long Id { get; set; }
        public string Exe { get; set; }

        public Whitelist_event()
        {
            Id = -1;
            Exe = "";
        }

        public Whitelist_event(long _id)
        {
            Id = _id;
            Exe = "";
        }

        public Whitelist_event(string _exe)
        {
            Id = -1;
            Exe = _exe;
        }

        public Whitelist_event(long _id,string _exe)
        {
            Id = _id;
            Exe = _exe;
        }

        public override bool Equals(object o)
        {
            if((o == null) || !this.GetType().Equals(o.GetType())) return false;

            Whitelist_event a = (Whitelist_event)o;

            return this.Id == a.Id && this.Exe == a.Exe;
        }

        public static bool operator==(Whitelist_event a,Whitelist_event b)
        {
            return a.Equals(b);
        }

        public static bool operator!=(Whitelist_event a,Whitelist_event b)
        {
            return !a.Equals(b);
        }

        public override string ToString()
        {
            if(Id > -1 && String.IsNullOrEmpty(Exe))
            {
                return "ID: " + Id + "\n"
                    + "Exe: n/a";
            }

            if(Id < 0 && !String.IsNullOrEmpty(Exe))
            {
                return "ID: n/a" + "\n"
                    + "Exe: " + Exe;
            }

            return "ID: " + Id + "\n"
                    + "Exe: " + Exe;
        }
    }

    class Whitelist_events
    {
        public List<Whitelist_event> Events { get; set;}
        
        public Whitelist_events()
        {
            Events = new List<Whitelist_event>();
        }

        private Whitelist_event parse_event(string evt)
        {
            long id = -1;
            string exe = "";
            string[] val = evt.Split(',');

            for(int i = 0;i < val.Length;i++)
            {
                if(i == 0 && !String.IsNullOrEmpty(val[0]))
                {
                    id = long.Parse(val[0]);
                }

                if(i == 1 && !String.IsNullOrEmpty(val[1]))
                {
                    exe = val[1];
                }
            }

            return new Whitelist_event(id,exe);
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

            foreach(Whitelist_event evt in Events)
            {
                sw.WriteLine(
                    evt.Id + ","
                    + evt.Exe);
            }

            sw.Close();
        }
    }
}
