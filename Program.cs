/* event_monitor.exe
 * Created by nicholas.flesch@outlook.com
 * Last Modified: 2020.10.13
 * Description:
 *      The program reads a list of user defined Windows events,
 *      compares them with events logged in Windows Event Logs,
 *      and send an email alert to a specified email address through a 
 *      specificed email server when occurances of the user defined
 *      events are found in the Windows Event Logs.
 *      
 *      User defined events are categorized as 'o', one-off, or 
 *      'c', cluster. One-off events trigger the email alert upon
 *      their first occurance. Cluster events only trigger the 
 *      email alert if the event occurs a user defined number of times
 *      within a user defined time frame.
 *      
 *      Users can also whitelist event IDs and executables. When the
 *      executable is whitelisted the program will search the Win_event
 *      Message look for a string that matches the user defined executable.
 * 
 * Resources used:
 *  https://orangematter.solarwinds.com/2018/01/25/microsoft-workstation-logs-focus-on-whats-important/
 *  https://statemigration.com/top-3-workstation-logs-to-monitor/
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;

using Mono.Options;

namespace event_monitor
{
    class Program
    {
        public static void clear_screen()
        {
            for(int i = 0; i < 100;i++) Console.WriteLine();
        }

        public static void append_event(Monitored_events events,string events_path)
        {
            bool exit = false;
            
            while(!exit)
            {
                long id;
                char kind;
                int time = 0;
                int num = 0;
                string description = "n/a";
                
                Console.WriteLine("Event ID:");
                id = long.Parse(Console.ReadLine());
                if(id < 1 || id > 100000)
                {
                    clear_screen();
                    Console.WriteLine("Error: ID must be between 1 and 100000");
                    continue;
                }

                Console.WriteLine("Description:");
                description = Console.ReadLine();

                Console.WriteLine("Event type ('o' == one-off, 'c' == cluster):");
                string temp_kind = Console.ReadLine();
                temp_kind = temp_kind.ToLower();
                kind = temp_kind[0];
                if(kind != 'o' && kind != 'c')
                {
                    clear_screen();
                    Console.WriteLine("Error: Type must be 'o' or 'c'. Try again.");
                    continue;
                }
                
                if(kind == 'c')
                {
                    Console.WriteLine("Time threshhold (seconds):");
                    try
                    {
                        time = int.Parse(Console.ReadLine());
                        if(time < 0 || time > 10000) throw new Exception("Time out of range");
                    }
                    catch(Exception e)
                    {
                        clear_screen();
                        Console.WriteLine("Error: Time must be between 1 and 10000.");
                        continue;
                    }

                    Console.WriteLine("Count threshold");
                    try
                    {
                        num = int.Parse(Console.ReadLine());
                        if(num < 0) throw new Exception("Number out of range.");
                    }
                    catch(Exception e)
                    {
                        clear_screen();
                        Console.WriteLine("Error: Threshold number must be greater than 0.");
                        continue;
                    }
                }

                Monitored_event new_event = new Monitored_event(id,kind,time,num,description);

                events.Events.Add(new_event);

                exit = want_to_continue("Add another event? ([Y]es/[N]o)");
            }

            events.write_events(events_path);
        }

        public static void pop_event(Monitored_events events,string events_path)
        {
            bool exit = false;

            while(!exit)
            {
                Console.WriteLine("Monitored Events:\n");
                for(int i = 0;i < events.Events.Count;i++)
                {
                    Console.Write('\t');
                    Console.WriteLine(i + 1 + ":\t" + events.Events[i].Id + ": " + events.Events[i].Description);
                }

                Console.WriteLine();
                Console.Write("Enter an event to remove: ");

                try
                {
                    int answer = int.Parse(Console.ReadLine());
                    if(answer < 1 || answer > events.Events.Count)
                    {
                        clear_screen();
                        Console.WriteLine("Error: Selection must be between 1 and " + events.Events.Count);
                        continue;
                    }

                    Console.WriteLine("Removing:\t" + events.Events[answer - 1].Id + ": " + events.Events[answer - 1].Description);
                    events.Events.RemoveAt(answer - 1);
                    events.write_events(events_path);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }

                exit = want_to_continue("Remove another event? ([Y]es/[N]o)");
            }

        }

        public static void whitelist_event(Whitelist_events whitelisted_events,string whitelist_path)
        {
            bool exit = false;
            string answer = "";
            long id = -1;
            string exe = "";

            while(!exit)
            {
                Console.WriteLine();
                Console.Write("Event ID ([Enter] for default):\n");
                answer = Console.ReadLine();
                if(!String.IsNullOrEmpty(answer))
                {
                    try
                    {
                        id = long.Parse(answer);
                        if(id < 1 || id > 100000)
                        {
                            clear_screen();
                            Console.WriteLine("Error: ID must be between 1 and 100000");
                            continue;
                        }
                    }
                    catch(Exception e)
                    {
                        clear_screen();
                        Console.WriteLine(e);
                        continue;
                    }
                }
                else
                {
                    id = -1;
                }

                Console.WriteLine();
                Console.WriteLine("Exe Path: ([Enter] for default):\n");
                answer = Console.ReadLine();
                if(!String.IsNullOrEmpty(answer))
                {
                    exe = answer;
                }
                else
                {
                    exe = "";
                }

                Whitelist_event evt = new Whitelist_event(id,exe);

                if(!already_whitelisted(evt,whitelisted_events))
                { 
                    whitelisted_events.Events.Add(evt);
                    whitelisted_events.write_events(whitelist_path);
                }
                else
                {
                    Console.WriteLine();
                    Console.WriteLine("=========================");
                    Console.Write("Error:\n\n" + evt.ToString() + "\n\nIs already whitelisted.\n");
                    Console.Write("=========================");
                    Console.WriteLine();
                    Console.WriteLine();
                }

                exit = want_to_continue("Add another item to whitelist? ([Y]es/[N]o)");
            }
        }

        private static bool already_whitelisted(Whitelist_event evt,Whitelist_events whitelisted_events)
        {
            foreach(Whitelist_event e in whitelisted_events.Events)
            {
                if(evt == e) return true;
            }

            return false;
        }

        public static void pop_whitelist_event(Whitelist_events whitelisted_events,string whitelist_path)
        {
            bool exit = false;

            while(!exit)
            {
                Console.WriteLine("Whitelisted Events:\n");
                for(int i = 0;i < whitelisted_events.Events.Count;i++)
                {
                    Console.Write('\t');
                    Console.WriteLine(i + 1 + ":\tID: " + whitelisted_events.Events[i].Id + "\n\t\tExe path: " + whitelisted_events.Events[i].Exe);
                    Console.WriteLine();
                }

                Console.WriteLine();
                Console.Write("Enter an event to remove: ");

                try
                {
                    int answer = int.Parse(Console.ReadLine());
                    if(answer < 1 || answer > whitelisted_events.Events.Count)
                    {
                        clear_screen();
                        Console.WriteLine("Error: Selection must be between 1 and " + whitelisted_events.Events.Count);
                        continue;
                    }

                    Console.WriteLine("Removing:\n\tID: " + whitelisted_events.Events[answer - 1].Id + "\n\tExe path: " + whitelisted_events.Events[answer - 1].Exe);
                    whitelisted_events.Events.RemoveAt(answer - 1);
                    whitelisted_events.write_events(whitelist_path);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                    break;
                }

                exit = want_to_continue("Remove another event? ([Y]es/[N]o)");
            }
        }

        private static bool want_to_continue(string msg)
        {
            while(true)
            {
                Console.WriteLine(msg);
                string temp_answer = Console.ReadLine();
                temp_answer = temp_answer.ToLower();
                char yesno = temp_answer[0];

                if(yesno != 'n' && yesno != 'y')
                {
                    Console.WriteLine("Error: Invalid answer.");
                    continue;
                }

                if(yesno == 'n') return true;
                
                if(yesno == 'y') return false;
            }
        }

        static void Main(string[] args)
        {
            Options options = new Options(args);
            if(args.Length > 0) return;

            List<Win_event> new_entries = new List<Win_event>();
            EventLog[] logs = EventLog.GetEventLogs(".");
            string log_dir = "logs";
            string captured_events_log = DateTime.Today.ToString("yyyyMMdd") + "captured_events.log";
            captured_events_log = System.IO.Path.Combine(log_dir,captured_events_log);
            
            if(!File.Exists(log_dir)) System.IO.Directory.CreateDirectory(log_dir);

            if(!File.Exists(captured_events_log))
            {
                using (StreamWriter sw = File.CreateText(captured_events_log))
                {
                    sw.Close();
                }
            }

            Log_utils.remove_old_logs(log_dir);
            if(Log_utils.exceeds_max_size(captured_events_log))
            {
                Log_utils.rename_file(captured_events_log);
                using (StreamWriter sw = File.CreateText(captured_events_log))
                {
                    sw.Close();
                }
            }

            foreach(EventLog log in logs)
            {
                foreach(EventLogEntry entry in log.Entries)
                {
                    if(entry.TimeGenerated >= options.Conf.Last_run)
                    {
                        foreach(Monitored_event evt in options.Mon_events.Events)
                        {
                            if(entry.InstanceId == evt.Id)
                            {
                                new_entries.Add(new Win_event(entry.InstanceId,entry.TimeGenerated,entry.Message));
                            }
                        }
                    }
                }
            }
        
            Writer writer = new Writer(captured_events_log,new_entries);
            writer.Write();

            List<Win_event> all_entries = new List<Win_event>();
            Reader reader = new Reader(captured_events_log,all_entries);
            reader.Read();

            Log_analysis log_analysis = new Log_analysis(all_entries,options.Mon_events,options.Whitelisted_events,options.Conf);
            log_analysis.analyze();

            writer.overwrite(log_analysis.Processed);

            options.Conf.update_last_run();
        }
    }
}
