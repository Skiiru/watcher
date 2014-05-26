using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;


namespace Proc
{
    public class Proc
    {
        /// <summary>
        /// Функция на тик таймера. Добавляет нужную функцию в событие Exited
        /// </summary>
        public void timer_tik()
        {
            foreach (Process x in System.Diagnostics.Process.GetProcesses())
            {                   
                try
                    {
                        x.EnableRaisingEvents = true;
                        x.Exited += new EventHandler(x_Exited);
                    }
                catch { };
            }
        }

        private void x_Exited(object sender, EventArgs e)
        {
            
        }
    }
}
