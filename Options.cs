using System;
using System.Collections.Generic;
using System.Text;

using Mono.Options;

namespace event_monitor
{
    class Options
    {
        public Config Conf {get; set; }
        public Monitored_events Mon_events { get; set; }
        public Whitelist_events Whitelisted_events { get; set; }

        public Options(string[] args)
        {
            Conf = new Config();
            Mon_events = new Monitored_events();
            Whitelisted_events = new Whitelist_events();

            string monitored_event_file = "monitored_events";
            string whitelist_file = "whitelist";

            Conf.read_config();
            Mon_events.read_events(monitored_event_file);
            Whitelisted_events.read_events(whitelist_file);

            bool show_help = false;
            bool show_config = false;
            bool show_events = false;
            bool add_event = false;
            bool remove_event = false;
            bool whitelist = false;
            bool rem_whitelist = false;
            bool show_whitelist = false;

            //Parse options
            var options = new OptionSet()
                {
                    "Usage: log_monitor [OPTIONS]+",
                    "Notifies administrators of designated events logged in Windows logs." +
                    "The monitored events can also be modified with a CSV file editor.",
                    "",
                    "Options:",
                    {
                        "server=","FQDN or IP address of the email server",
                        v => Conf.Email_server = v
                    },
                    {
                        "sender=","the {EMAIL} of sender",
                        v => Conf.Sender = v
                    },
                    {
                        "recipient=","the {EMAIL} of recipient",
                        v => Conf.Recipient = v
                    },
                    {
                        "config", "displays log_monitor.exe current configuration",
                        v => show_config = v != null
                    },
                    {
                        "s|show", "show events being monitored",
                        v => show_events = v != null
                    },
                    {
                        "a|add", "add event to monitore list",
                        v => add_event = v != null
                    },
                    {
                        "r|remove", "add event to monitore list",
                        v => remove_event = v != null
                    },
                    {
                        "w|white", "add event or executable to the whitelist",
                        v => whitelist = v != null
                    },
                    {
                        "remove_white", "Remove event or executable to the whitelist",
                        v => rem_whitelist = v != null
                    },
                    {
                        "show_white", "Show whitelisted events and executables",
                        v => show_whitelist = v != null
                    },
                    {
                        "h|help", "show this message and exit",
                        v => show_help = v != null
                    },
            };

            List<string> extra;
            try
            {
                extra = options.Parse(args);

                if(!show_help)
                {
                    Conf.write_config();
                }
            }
            catch(OptionException e)
            {
                Console.WriteLine("Try `log_monitor --help' for more information.");
            }

            if(show_help)
            {
                options.WriteOptionDescriptions(Console.Out);
                return;
            }

            if(show_config)
            {
                Console.WriteLine(Conf.ToString());
                return;
            }

            if(show_events)
            {
                foreach(Monitored_event e in Mon_events.Events)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("-----------------------------");
                }

                return;
            }

            if(add_event)
            {
                Program.append_event(Mon_events,monitored_event_file);
                return;
            }

            if(remove_event)
            {
                Program.pop_event(Mon_events,monitored_event_file);
                return;
            }

            if(whitelist)
            {
                Program.whitelist_event(Whitelisted_events,whitelist_file);
                return;
            }

            if(rem_whitelist)
            {
                Program.pop_whitelist_event(Whitelisted_events,whitelist_file);
                return;
            }

            if(show_whitelist)
            {
                foreach(Whitelist_event e in Whitelisted_events.Events)
                {
                    Console.WriteLine(e.ToString());
                    Console.WriteLine("-----------------------------");
                }

                return;
            }

            if(args.Length > 0) Console.WriteLine("Invalid options: Try `log_monitor --help' for more information.");
        }
    }
}
