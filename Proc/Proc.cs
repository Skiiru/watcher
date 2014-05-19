using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Security.Permissions;

namespace Proc
{
    public class Proc
    {
        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        public void timer_tik()
        {
            foreach (Process x in System.Diagnostics.Process.GetProcesses())
            {
                if (x.ProcessName != "SearchFilterHost")
                {
                    try
                    {
                        x.EnableRaisingEvents = true;
                        x.Exited += new EventHandler(x_Exited);
                    }
                    catch { };
                }
            }
        }

        void x_Exited(object sender, EventArgs e)
        {

        }
    }
}
