using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace event_monitor
{
    class Writer //Writes a list of Win_events to a text file
    {
        public string Ofile { get; set; }
        public List<Win_event> Data { get; set; }
        
        public Writer(string _ofile,List<Win_event> _data)
        {
            Ofile = _ofile;
            Data = _data;
        }

        public void Write()
        {
            using StreamWriter sw = File.AppendText(Ofile);
        
            for(int i = 0;i < Data.Count;i++)
            {
                sw.WriteLine(Data[i]);
            }
            sw.Close();
        }

        public void overwrite(List<Win_event> _data)
        {
            Data = _data;
            using StreamWriter sw = new StreamWriter(Ofile);

            for(int i = 0;i < Data.Count;i++)
            {
                sw.WriteLine(Data[i]);
            }
            sw.Close();
        }
    }
}
