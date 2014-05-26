using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace FS
{
    /// <summary>
    /// Создает FileSystemWatcher для каждого логического диска и помещает в список
    /// </summary>
    public class FS
    {
        public List<FileSystemWatcher> fs_lst;
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        /// <summary>
        /// Cоздает FileSystemWatcher для каждого логического диска и помещает в список
        /// </summary>
        public FS()
        {
            fs_lst = new List<FileSystemWatcher>();
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
        }
    }
}
