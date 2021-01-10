using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace event_monitor
{
    class Log_utils
    {
        static public void rename_file(string path)
        {
            if(!File.Exists(path)) return;

            int mod = 1;
            while(true)
            {
                string ext = Path.GetExtension(path);
                string old_name = Path.GetFileNameWithoutExtension(path);
                string new_name = old_name + "-" + mod + ext;
                string new_path = Path.Combine(Path.GetDirectoryName(path),new_name);
                try
                {
                    File.Move(path,new_path);
                    break;
                }
                catch
                {
                    ++mod;
                }
            }
        }

        static public void remove_old_logs(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] fi = di.GetFiles();

            foreach(FileInfo f in fi)
            {
                string file_path = System.IO.Path.Combine(path,f.Name);
                if((File.Exists(file_path)) && (File.GetCreationTime(file_path) < DateTime.Today))
                {
                    Console.WriteLine("Remove old log file: {0}",f.Name);
                    f.Delete();
                }
            }
        }

        static public bool exceeds_max_size(string path)
        {
            FileInfo fi = new FileInfo(path);
            if(fi.Length > 500000)
            {
                Console.WriteLine("The size of {0} is {1} bytes.",fi.Name,fi.Length);
                return true;
            }
            
            return false;
        }
    }
}
