using System;
using System.Collections.Generic;
using System.Text;

namespace event_monitor
{
 class Win_event : IComparable<Win_event>
    {
        public long Id { get; set; }
        public DateTime Time_stamp { get; set; }
        public string Message { get; set; }
        public bool Alerted { get; set; }

        public Win_event(long _id, DateTime _time_stamp,string _message,bool _alerted = false)
        {
            Id = _id;
            Time_stamp = _time_stamp;
            Message = _message;
            Alerted = _alerted;
        }

        public Win_event(Win_event evt)
        {
            Id = evt.Id;
            Time_stamp = evt.Time_stamp;
            Message = evt.Message;
            Alerted = evt.Alerted;
        }

        private string rem_spec_chars(string str)
        {
            char[] exclude = {'\n','\r','\t',','};
            string mod_str = "";
            bool special = false;

            foreach(char ch in str)
            {
                foreach(char ex in exclude)
                {
                    if(ch == ex)
                    { 
                        special = true;
                    }
                }

                if(!special)
                {
                    mod_str += ch;
                }

                special = false;
            }

            return mod_str;
        }

        public int CompareTo(Win_event compare_event)
        {
            if(compare_event == null)
            {
                return 1;
            }
            else
            {
                return this.Id.CompareTo(compare_event.Id);
            }
        }

        public override string ToString()
        {
            return Id + "," + Time_stamp.ToString("yyyyMMddHHmmss") + "," + rem_spec_chars(Message) + "," + Alerted + ",";
        }
    }
}
