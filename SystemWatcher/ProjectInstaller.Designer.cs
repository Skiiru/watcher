namespace SystemWatcher
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором компонентов

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.WatcherProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.SystemWatcherInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // WatcherProcessInstaller
            // 
            this.WatcherProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.WatcherProcessInstaller.Password = null;
            this.WatcherProcessInstaller.Username = null;
            // 
            // SystemWatcherInstaller
            // 
            this.SystemWatcherInstaller.Description = "Service for monitoring user activity";
            this.SystemWatcherInstaller.DisplayName = "SystemWatcher";
            this.SystemWatcherInstaller.ServiceName = "SystemWatcher";
            this.SystemWatcherInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.WatcherProcessInstaller,
            this.SystemWatcherInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller WatcherProcessInstaller;
        private System.ServiceProcess.ServiceInstaller SystemWatcherInstaller;
    }
}