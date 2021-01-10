using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace event_monitor
{
    class Log_analysis
    // work flow: analyze() > catagorize() > evaluate()
    {
        public List<Win_event> Detected { get; set; }
        public List<Win_event> Processed { get; private set; }
        public Monitored_events Monitored { get; set; }
        public Whitelist_events Whitelisted { get; set;}
        private Config config { get; set; }

        private bool is_single_event(Win_event evnt)
        {
            foreach(Monitored_event me in Monitored.Events)
            {
                if(evnt.Id == me.Id)
                {
                    if(me.Kind == 'o') return true;
                }
            }

            return false;
        }

         private bool is_cluster_event(Win_event evnt)
        {
            foreach(Monitored_event me in Monitored.Events)
            {
                if(evnt.Id == me.Id)
                {
                    if(me.Kind == 'c') return true;
                }
            }

            return false;
        }

        private string insert_escapes(string s)
        {
            string mod_s = "";
            foreach(char c in s)
            {
                if(c == '\\') mod_s += c;
                if(c == '(') mod_s += '\\';
                if(c == ')') mod_s += '\\';

                mod_s += c;
            }

            return mod_s;
        }

        private bool is_whitelisted(Win_event evnt)
        {
            foreach(Whitelist_event we in Whitelisted.Events)
            {
                if(we.Id > -1)
                {
                    if(!String.IsNullOrEmpty(we.Exe)) //if ID and EXE have values, test against both
                    {
                        Console.WriteLine("Whitelist ID and EXE match");
                        Console.WriteLine(insert_escapes(we.Exe));
                        if(we.Id == evnt.Id && Regex.IsMatch(evnt.format_message(),insert_escapes(we.Exe),RegexOptions.IgnoreCase)) return true;
                    }
                    else //if no value in EXE, test only against ID
                    {
                        Console.WriteLine("Whitelist ID match");
                        if(we.Id == evnt.Id) return true;
                    }
                    
                }
                else //test only against EXE
                {
                    Console.WriteLine("Whitelist EXE match");
                    if(Regex.IsMatch(evnt.format_message(),insert_escapes(we.Exe),RegexOptions.IgnoreCase)) return true;
                }
                

            }

            Console.WriteLine("Whitelist didn't match a thang!");
            return false;
        }

        private Monitored_event find_monitored_event(List<Win_event> events)
        {
            foreach(Win_event evnt in events)
            {
                foreach(Monitored_event me in Monitored.Events)
                {
                    if(me.Id == evnt.Id)
                    {
                        return me;
                    }
                }
            }

            return new Monitored_event(0,'o',0,0,"");
        }

        private bool check_cluster(List<Win_event> events)
        {
            Monitored_event mon_evnt = find_monitored_event(events);
        
            int evnt_a = 0;
            int evnt_b = 1;
            int alert_count = 0;

            while(evnt_b < events.Count)
            {
                if(Convert.ToInt32(events[evnt_b].Time_stamp.Subtract(events[evnt_a].Time_stamp).TotalSeconds) < mon_evnt.Alert_tshd_time)
                {
                    ++alert_count;
                    ++evnt_b;
                }
                else
                {
                    evnt_a = evnt_b;
                    ++evnt_b;
                    alert_count = 0;
                }

                if(alert_count >= mon_evnt.Alert_tshd_num - 1) return true;
            }

            return false;
        }

        private void mark_all_alerted(List<Win_event> events)
        {
            foreach(Win_event evnt in events)
            {
                evnt.Alerted = true;
            }
        }
        private void evaluate(List<Win_event> events)
        {
            /*
             * If one off event, send alert for that event, mark all as processed, return
             * If cluster event, look for cluster, send alert if found, 
             *      mark all as (including outside cluster) processed, return
             * In both cases find the first occurance and quits
            */
        
            events.Sort(
                delegate(Win_event ea,Win_event eb)
                {
                    if(ea.Time_stamp == null && eb.Time_stamp == null) return 0;
                    else if(ea.Time_stamp == null) return -1;
                    else if(eb.Time_stamp == null) return 1;
                    else return ea.Time_stamp.CompareTo(eb.Time_stamp);
                }
            );

            foreach(Win_event evnt in events)
            {
                if(!evnt.Alerted)
                {
                    if(is_single_event(evnt))
                    {
                        Console.WriteLine(evnt);
                        Console.WriteLine("!!!SEND ALERT FOR SINGLES!!!");
                        string message = "The below event was detected:\n\n"
                            + "Windows Event ID: " + evnt.Id + "\n"
                            + "Time Stamp: " + evnt.Time_stamp + "\n"
                            + "Event Information: " + evnt.format_message() + "\n\n"
                            + "------------------------------------" + "\n\n"
                            + "This alert was triggered because the above event was "
                            + "recorded AT LEAST once in the computer's event logs";
                        Report report = new Report(message);
                        
                        if(report.email_alert(config.Email_server,config.Sender,config.Recipient))
                        {
                            mark_all_alerted(events);
                        }
                        
                        break;
                    }

                    if(is_cluster_event(evnt))
                    {
                        if(check_cluster(events))
                        {
                            Console.WriteLine(evnt);
                            Console.WriteLine("!!!SEND ALERT FOR CLUSTERS!!!");
                            string message = "The below event was detected:\n\n"
                            + "Windows Event ID: " + evnt.Id + "\n"
                            + "Time Stamp: " + evnt.Time_stamp +"\n"
                            + "Event Information: " + evnt.format_message() + "\n\n"
                            + "------------------------------------" + "\n\n"
                            + "This alert was triggered because the above event was "
                            + "recorded MULTIPLE times in a limited timeframe "
                            + "in the computer's event logs.";
                            Report report = new Report(message);
                            
                            if(report.email_alert(config.Email_server,config.Sender,config.Recipient))
                            {
                                mark_all_alerted(events);
                            }
                            
                        }

                        break;
                    }
                }
            }
        }

        private void catagorize()
        {
            List<Win_event> detected_copy = Detected;
            List<int> indexes = new List<int>();

            // Remove whitelisted events from detected events.
            for(int i = 0;i < detected_copy.Count;i++)
            {
                if(is_whitelisted(detected_copy[i])) indexes.Add(i);
            }

            for(int i = indexes.Count - 1;i > -1;i--)
            {
                detected_copy.RemoveAt(indexes[i]);
            }

            // Evaluate events detected events
            while(detected_copy.Count > 0)
            {
                Win_event evnt = detected_copy[0];
                List<Win_event> events = detected_copy.FindAll(i => i.Id == evnt.Id);
                evaluate(events);
                foreach(Win_event ev in events) Processed.Add(ev);
                detected_copy.RemoveAll(i => i.Id == evnt.Id);
            }
        }

        public void analyze()
        {
            catagorize();
        }

        public Log_analysis(List<Win_event> _detected,Monitored_events _monitored,Whitelist_events _whitelisted,Config _config)
        {
            Detected = _detected;
            Processed = new List<Win_event>();
            Monitored = _monitored;
            Whitelisted = _whitelisted;
            config = _config;
        }
    }

}
