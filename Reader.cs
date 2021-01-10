using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace event_monitor
{
    class Reader //Reads Win_events from a text file and places them into a List.
    {
        public string Ifile { get; set; }
        public List<Win_event> Data { get; set; }

        public Reader(string _ifile,List<Win_event> _data)
        {
            Ifile = _ifile;
            Data = _data;
        }

        public void Read()
        {
            if(!File.Exists(Ifile))
            {
                return;
            }

            using(StreamReader sr = new StreamReader(Ifile))
            {
                long id = 0;
                DateTime time_stamp = new DateTime();
                string msg = "";
                string line = "";
                bool alerted = false;

                while((line = sr.ReadLine()) != null)
                {
                    string[] vals = line.Split(',');

                    if(!String.IsNullOrEmpty(vals[0]))
                    {
                        id = long.Parse(vals[0]); 
                    }

                    if(!String.IsNullOrEmpty(vals[1]))
                    {
                        time_stamp = DateTime.ParseExact(vals[1],"yyyyMMddHHmmss",null); 
                    }

                    if(!String.IsNullOrEmpty(vals[2]))
                    {
                        msg = vals[2];
                    }

                    if(!String.IsNullOrEmpty(vals[3]))
                    {
                        alerted = Convert.ToBoolean(vals[3]);
                    }

                    //if(!alerted) Data.Add(new Win_event(id,time_stamp,msg,alerted));
                    Data.Add(new Win_event(id,time_stamp,msg,alerted));
                }
            }
        }
    }
}
