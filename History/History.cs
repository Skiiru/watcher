using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using UrlHistoryLibrary;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Finisar.SQLite;


namespace History
{
    public class InternetExplorer
    {
        public InternetExplorer()
        {
            URLs = new List<URL>();
        }
        // List of URL objects
        public List<URL> URLs { get; set; }
        public IEnumerable<URL> GetHistory(DateTime filter)
        {
            // Initiate main object
            UrlHistoryWrapperClass urlhistory = new UrlHistoryWrapperClass();

            // Enumerate URLs in History
            UrlHistoryWrapperClass.STATURLEnumerator enumerator =
                                               urlhistory.GetEnumerator();

            // Iterate through the enumeration
            while (enumerator.MoveNext())
            {
                if(enumerator.Current.LastVisited>filter)
                {
                    // Obtain URL and Title
                    string url = enumerator.Current.URL.Replace('\'', ' ');
                    // In the title, eliminate single quotes to avoid confusion
                    string title = string.IsNullOrEmpty(enumerator.Current.Title)
                              ? enumerator.Current.Title.Replace('\'', ' ') : "";
                    DateTime time = enumerator.Current.LastVisited;

                    // Create new entry
                    URL U = new URL(url, title, time, "Internet Explorer");

                    // Add entry to list
                    URLs.Add(U);
                }
            }

            // Optional
            enumerator.Reset();

            // Clear URL History
            urlhistory.ClearHistory();

            return URLs;
        }
    }

    public class Firefox
    {
        public List<URL> URLs { get; set; }
        public IEnumerable<URL> GetHistory(DateTime filter)
        {
            // Get Current Users App Data
            string documentsFolder = Environment.GetFolderPath
                              (Environment.SpecialFolder.ApplicationData);

            // Move to Firefox Data
            documentsFolder += "\\Mozilla\\Firefox\\Profiles\\";

            // Check if directory exists
            if (Directory.Exists(documentsFolder))
            {
                // Loop each Firefox Profile
                foreach (string folder in Directory.GetDirectories
                                                    (documentsFolder))
                {
                    // Fetch Profile History
                    ExtractUserHistory(folder,filter);
                }
            }
            return URLs;
          
        }

        IEnumerable<URL> ExtractUserHistory(string folder,DateTime filter)
        {
            try
            {
                // Get User history info
                DataTable historyDT = ExtractFromTable("moz_places", folder);

                // Get visit Time/Data info
                DataTable visitsDT = ExtractFromTable("moz_historyvisits",
                                                       folder);

                // Loop each history entry
                foreach (DataRow row in historyDT.Rows)
                {
                    // Select entry Date from visits
                    var entryDate = (from dates in visitsDT.AsEnumerable()
                                     where dates["place_id"].ToString() == row["id"].ToString()
                                     select dates["visit_date"]).LastOrDefault();
                    // If history entry has date
                    if ((entryDate != null) && (DateTime.FromFileTime(Convert.ToInt64(entryDate)) > filter))
                    {
                        // Obtain URL and Title strings
                        string url = row["Url"].ToString();
                        string title = row["title"].ToString();
                        DateTime time = DateTime.FromFileTime(Convert.ToInt64(Convert.ToString(entryDate)));
                        // Create new Entry
                        URL u = new URL(url.Replace('\'', ' '), title.Replace('\'', ' '), time, "Mozilla Firefox");

                        // Add entry to list
                        URLs.Add(u);
                    }
                }
            }
            catch { }
            // Clear URL History
            DeleteFromTable("moz_places", folder);
            DeleteFromTable("moz_historyvisits", folder);

            return URLs;
        }

        void DeleteFromTable(string table, string folder)
        {
            Finisar.SQLite.SQLiteConnection sql_con;
            Finisar.SQLite.SQLiteCommand sql_cmd;

            // FireFox database file
            string dbPath = folder + "\\places.sqlite";

            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new Finisar.SQLite.SQLiteConnection("Data Source=" + dbPath +
                                    ";Version=3;New=False;Compress=True;");

                // Open the Conn
                sql_con.Open();

                // Delete Query
                string CommandText = "delete from " + table;

                // Create command
                sql_cmd = new Finisar.SQLite.SQLiteCommand(CommandText, sql_con);

                sql_cmd.ExecuteNonQuery();

                // Clean up
                sql_con.Close();
            }
        }

        DataTable ExtractFromTable(string table, string folder)
        {
            Finisar.SQLite.SQLiteConnection sql_con;
            Finisar.SQLite.SQLiteCommand sql_cmd;
            Finisar.SQLite.SQLiteDataAdapter DB;
            DataTable DT = new DataTable();

            // FireFox database file
            string dbPath = folder + "\\places.sqlite";

            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new Finisar.SQLite.SQLiteConnection("Data Source=" + dbPath +
                                    ";Version=3;New=False;Compress=True;");

                // Open the Connection
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Select Query
                string CommandText = "select * from " + table;

                // Populate Data Table
                DB = new Finisar.SQLite.SQLiteDataAdapter(CommandText, sql_con);
                DB.Fill(DT);

                // Clean up
                sql_con.Close();
            }
            return DT;
        }
    }
    public class GoogleChrome
    {
        public List<URL> URLs = new List<URL>();
        public void GetHistory(DateTime filter)
	{
		string chromeHistoryFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+ @"\Google\Chrome\User Data\Default\History";
		if (File.Exists(chromeHistoryFile))
		{
            System.Data.SQLite.SQLiteConnection connection = new System.Data.SQLite.SQLiteConnection("Data Source=" + chromeHistoryFile + ";Version=3;New=False;Compress=True;");
			connection.Open();
			DataSet dataset = new DataSet();
            System.Data.SQLite.SQLiteDataAdapter adapter = new System.Data.SQLite.SQLiteDataAdapter("select * from urls order by last_visit_time desc", connection);
			adapter.Fill(dataset);
			if (dataset != null && dataset.Tables.Count > 0 & dataset.Tables[0] != null)
			{
				DataTable dt = dataset.Tables[0];
				foreach (DataRow historyRow in dt.Rows)
				{
                     string url = Convert.ToString(historyRow["url"]);
                     string Title = Convert.ToString(historyRow["title"]);
					// Chrome stores time elapsed since Jan 1, 1601 (UTC format) in microseconds
					long utcMicroSeconds = Convert.ToInt64(historyRow["last_visit_time"]);
					// Windows file time UTC is in nanoseconds, so multiplying by 10
					DateTime gmtTime = DateTime.FromFileTimeUtc(10 * utcMicroSeconds);
					// Converting to local time
					DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(gmtTime, TimeZoneInfo.Local);
                    if (localTime > filter)
                    {
                        URL historyItem = new URL(url, Title, localTime, "Google Chrome");
                        URLs.Add(historyItem);
                    }
				}
			}
		}
	}
    }

}
