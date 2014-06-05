using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Gma.UserActivityMonitor;
using FS;
using History;
using System.ServiceModel;
using Data;
using System.IO;

namespace SystemWatcher
{
    public enum Browser
    {
        chrome,ie,firefox
    }

    [ServiceContract]
    public interface IMyObject
    {
        [OperationContract] // Делегируемый метод.
        string LoadData(string data, DateTime date, string type, int UserID);
        [OperationContract]
        int GetUserID(string UserName, string ComputerName);
        [OperationContract]
        int GetComputerID(string ComputerName);
        [OperationContract]
        bool check();
        [OperationContract]
        string LoadComputer(string name);
        [OperationContract]
        string LoadUser(string name);
    }

    public partial class SystemWatcher : ServiceBase
    {
        public long timer;
        public long timer_stop;
        bool connection;
        List<Data.Data> DataForLoad;
        DateTime dt;
        string uri;

        public SystemWatcher()
        {
            InitializeComponent();
            dt = DateTime.Now;
            timer = 0;
            DataForLoad = new List<Data.Data>();
            FileStream fs = new FileStream("C://options.ini", FileMode.OpenOrCreate);
            using (StreamReader sw = new StreamReader(fs))
            {
                string s = sw.ReadLine();
                if (s == null)
                {
                    fs.Dispose();
                    using (FileStream fs1 = new FileStream("C://options.ini", FileMode.Create))
                    using (StreamWriter swr = new StreamWriter(fs1))
                    {
                        swr.WriteLine("http://localhost");
                        fs1.Dispose();
                        fs = new FileStream("C://options.ini", FileMode.Create);
                        s = sw.ReadLine();
                    }
                }
                uri = s;
                fs.Dispose();
            }

        }

        protected override void OnStart(string[] args)
        {
            AddLog("Started " + DateTime.Now.ToString());


            HookManager.KeyDown += HookManager_KeyDown_Up;
            HookManager.KeyPress += HookManager_KeyPress;
            HookManager.KeyUp += HookManager_KeyDown_Up;
            HookManager.MouseClick += HookManager_MouseClick;
            HookManager.MouseClickExt += HookManager_MouseClickExt;
            HookManager.MouseDoubleClick += HookManager_MouseClick;
            HookManager.MouseDown += HookManager_MouseClick;
            HookManager.MouseUp += HookManager_MouseClick;
            HookManager.MouseMove += HookManager_MouseClick;
            HookManager.MouseMoveExt += HookManager_MouseClickExt;
            HookManager.MouseWheel += HookManager_MouseClick;

            // проблема с потоками
            FS.FS f = new FS.FS();
            foreach (var fs in f.fs_lst)
            {
                fs.EnableRaisingEvents = true;
                fs.IncludeSubdirectories = true;
                fs.Changed += fs_Changed;
                fs.Created += fs_Changed;
                fs.Deleted += fs_Changed;
                fs.Renamed += fs_Renamed;
            }


        }

        protected override void OnStop()
        {
            AddLog("Stoped " + DateTime.Now.ToString());
        }


        #region События
        void exit_proc_Exited(object sender, EventArgs e)
        {
            try
            {
                if ((sender as Process).ProcessName == "chrome.exe")
                {
                    Browsers(Browser.chrome);
                }
                if ((sender as Process).ProcessName == "iexplore.exe")
                {
                    Browsers(Browser.ie);
                }
                if ((sender as Process).ProcessName == "firefox.exe")
                {
                    Browsers(Browser.firefox);
                }
            }
            catch{}
            Data(uri, (sender as Process).ProcessName + ' ' + (sender as Process).StartTime.ToString() + ' ' + (sender as Process).ExitTime.ToString(), DateTime.Now, global::Data.Type.process);
        }

        void fs_Renamed(object sender, System.IO.RenamedEventArgs e)
        {
            Data(uri, "Файл " + e.OldFullPath + " быд перемеинован в " + e.FullPath, DateTime.Now, global::Data.Type.files);
        }

        void fs_Changed(object sender, System.IO.FileSystemEventArgs e)
        {
            Data(uri, "Файл " + e.FullPath.ToString() + ' ' + e.ChangeType.ToString(), DateTime.Now, global::Data.Type.files);
        }

        void HookManager_MouseClickExt(object sender, MouseEventExtArgs e)
        {
            TimerActivity();
            timer = 0;
        }

        void HookManager_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            TimerActivity();
            timer = 0;
        }

        void HookManager_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            TimerActivity();
            timer = 0;
        }

        void HookManager_KeyDown_Up(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            TimerActivity();
            timer = 0;
        }
        #endregion

        public void AddLog(string log)
        {
            try
            {
                if (EventLog.SourceExists("SystemWatcher"))
                {
                    EventLog.CreateEventSource("SystemWatcher", "SystemWatcher");
                }
                else
                {
                    eventLog.Source = "SystemWatcher";
                    eventLog.WriteEntry(log);
                }
            }
            catch
            {

            }
        }

        public void ProcessTT()
        {
            foreach (Process exit_proc in Process.GetProcesses())
            {
                try
                {
                    exit_proc.EnableRaisingEvents = true;
                    if(exit_proc.ProcessName=="chrome")
                    {

                    }
                    exit_proc.Exited += exit_proc_Exited;
                }
                catch { };

            }
        }

        public void TimerActivity()
        {
            if (timer < 300)
            {
                if (timer_stop > 300)
                {
                    Data(uri, "Простой системы:" + ((double)(timer_stop / 60)).ToString() + " минут. Конец простоя:" + DateTime.Now.TimeOfDay.ToString(),DateTime.Now,global::Data.Type.activity);
                    timer = 0;
                }
                else
                {

                }
            }
            else
            {
                timer_stop = timer;
            }
        }

        public void Data(string uri, string data, DateTime date, Data.Type type)
        {
            Uri tcpUri = new Uri(uri);
            EndpointAddress address = new EndpointAddress(tcpUri);
            BasicHttpBinding binding = new BasicHttpBinding();
            ChannelFactory<IMyObject> factory = new ChannelFactory<IMyObject>(binding, address);
            IMyObject service = factory.CreateChannel();

            try
            {
                connection = service.check();
            }
            catch
            {
                connection = false;
            }
            if (connection)
            {
                if (service.GetUserID(Environment.UserName, Environment.MachineName) == -1)
                {
                    service.LoadComputer(Environment.MachineName);
                    service.LoadUser(Environment.UserName);
                }
                foreach (var d in DataForLoad)
                {
                    service.LoadData(d.data, d.time, d.type.ToString(), d.user_id);
                }
                DataForLoad.Clear();
                service.LoadData(data, date, type.ToString(), service.GetUserID(Environment.UserName, Environment.MachineName));
                AddLog("Данные были загружены");
            }
            else
            {
                AddLog("Проблема с соединением. Данные внесены в список.");
                DataForLoad.Add(new Data.Data(data, date, type, service.GetUserID(Environment.UserName, Environment.MachineName)));
            }
        }

        public void Browsers(Browser br)
        {
            switch (br)
            {
                case Browser.chrome:
                {
                    History.GoogleChrome chrome = new GoogleChrome();
                    chrome.GetHistory(dt);
                    foreach (var x in chrome.URLs)
                    {
                        Data(uri, x.url + ' ' + x.title + ' ', x.time, global::Data.Type.history);
                    }
                    dt = DateTime.Now;
                    break;
                }
                case Browser.firefox:
                {
                    History.Firefox fx = new Firefox();
                    foreach (var x in fx.GetHistory(dt))
                    {
                        Data(uri, x.url + ' ' + x.title + ' ', x.time, global::Data.Type.history);
                    }
                    dt = DateTime.Now;
                    break;
                }
                case Browser.ie:
                {
                    History.InternetExplorer ie = new InternetExplorer();
                    ie.GetHistory(dt);
                    foreach (var x in ie.URLs)
                    {
                        Data(uri, x.url + ' ' + x.title + ' ', x.time, global::Data.Type.history);
                    }
                    dt = DateTime.Now;
                    break;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer++;
            ProcessTT();
        }
    }
}
