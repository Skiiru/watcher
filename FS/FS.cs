using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FS
{
    public class FS
    {
        List<FileSystemWatcher> fs_lst;
        public FS()
        {
            try
            {
                foreach (string x in Environment.GetLogicalDrives())
                {
                    if (x == "C:\\")
                    {
                        FileSystemWatcher fs = new FileSystemWatcher(x);
                        fs.Filter = "Documents and settings";
                        fs_lst.Add(fs);
                    }
                    else
                        fs_lst.Add(new FileSystemWatcher(x));
                }
            }
            catch { };
            foreach (FileSystemWatcher x in fs_lst)
            {
                x.IncludeSubdirectories = true;
                x.EnableRaisingEvents = true;
                x.Changed += new FileSystemEventHandler(x_Changed);
                x.Created += new FileSystemEventHandler(x_Changed);
                x.Deleted += new FileSystemEventHandler(x_Changed);
                x.Renamed += new RenamedEventHandler(x_Renamed);

            }
        }
        
        void x_Renamed(object sender, RenamedEventArgs e)
        {
            
        }
        void x_Changed(object sender, FileSystemEventArgs e)
        {

        }
    }
}
