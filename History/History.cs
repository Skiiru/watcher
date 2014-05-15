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


namespace History
{
    public class History
    {
        
    }
    public class InternetExplorer
    {
        // List of URL objects
        public List<URL> URLs { get; set; }
        public IEnumerable<URL> GetHistory()
        {
            // Initiate main object
            UrlHistoryWrapperClass urlhistory = new UrlHistoryWrapperClass();

            // Enumerate URLs in History
            UrlHistoryWrapperClass.STATURLEnumerator enumerator =
                                               urlhistory.GetEnumerator();

            // Iterate through the enumeration
            while (enumerator.MoveNext())
            {
                // Obtain URL and Title
                string url = enumerator.Current.URL.Replace('\'', ' ');
                // In the title, eliminate single quotes to avoid confusion
                string title = string.IsNullOrEmpty(enumerator.Current.Title)
                          ? enumerator.Current.Title.Replace('\'', ' ') : "";
                DateTime time = enumerator.Current.LastVisited;

                // Create new entry
                URL U = new URL(url, title,time, "Internet Explorer");

                // Add entry to list
                URLs.Add(U);
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
        public IEnumerable<URL> GetHistory()
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
                    return ExtractUserHistory(folder);
                }
            }
            return null;
        }

        IEnumerable<URL> ExtractUserHistory(string folder)
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
                                 select dates).LastOrDefault();
                // If history entry has date
                if (entryDate != null)
                {
                    // Obtain URL and Title strings
                    string url = row["Url"].ToString();
                    string title = row["title"].ToString();
                    DateTime time = DateTime.Now;
                    // Create new Entry
                    URL u = new URL(url.Replace('\'', ' '),title.Replace('\'', ' '),time,"Mozilla Firefox");

                    // Add entry to list
                    URLs.Add(u);
                }
            }
            // Clear URL History
            DeleteFromTable("moz_places", folder);
            DeleteFromTable("moz_historyvisits", folder);

            return URLs;
        }

        void DeleteFromTable(string table, string folder)
        {
            SQLiteConnection sql_con;
            SQLiteCommand sql_cmd;

            // FireFox database file
            string dbPath = folder + "\\places.sqlite";

            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new SQLiteConnection("Data Source=" + dbPath +
                                    ";Version=3;New=False;Compress=True;");

                // Open the Conn
                sql_con.Open();

                // Delete Query
                string CommandText = "delete from " + table;

                // Create command
                sql_cmd = new SQLiteCommand(CommandText, sql_con);

                sql_cmd.ExecuteNonQuery();

                // Clean up
                sql_con.Close();
            }
        }

        DataTable ExtractFromTable(string table, string folder)
        {
            SQLiteConnection sql_con;
            SQLiteCommand sql_cmd;
            SQLiteDataAdapter DB;
            DataTable DT = new DataTable();

            // FireFox database file
            string dbPath = folder + "\\places.sqlite";

            // If file exists
            if (File.Exists(dbPath))
            {
                // Data connection
                sql_con = new SQLiteConnection("Data Source=" + dbPath +
                                    ";Version=3;New=False;Compress=True;");

                // Open the Connection
                sql_con.Open();
                sql_cmd = sql_con.CreateCommand();

                // Select Query
                string CommandText = "select * from " + table;

                // Populate Data Table
                DB = new SQLiteDataAdapter(CommandText, sql_con);
                DB.Fill(DT);

                // Clean up
                sql_con.Close();
            }
            return DT;
        }
    }

    public class HistoryItem
    {
        public string URL { get; set; }
        public string Title { get; set; }
        public DateTime VisitedTime { get; set; }
    }
    public class GoogleChrome
    {
        static void Main()
	{
		string chromeHistoryFile = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)+ @"\Google\Chrome\User Data\Default\History";
		if (File.Exists(chromeHistoryFile))
		{
			SQLiteConnection connection = new SQLiteConnection("Data Source=" + chromeHistoryFile + ";Version=3;New=False;Compress=True;");
			connection.Open();
			DataSet dataset = new DataSet();
			SQLiteDataAdapter adapter = new SQLiteDataAdapter("select * from urls order by last_visit_time desc", connection);
			adapter.Fill(dataset);
			if (dataset != null && dataset.Tables.Count > 0 & dataset.Tables[0] != null)
			{
				DataTable dt = dataset.Tables[0];
                List<HistoryItem> allHistoryItems = new List<HistoryItem>();
				foreach (DataRow historyRow in dt.Rows)
				{
					HistoryItem historyItem = new HistoryItem();
					{
						string url = Convert.ToString(historyRow["url"]);
                        string Title = Convert.ToString(historyRow["title"]);
					};
					// Chrome stores time elapsed since Jan 1, 1601 (UTC format) in microseconds
					long utcMicroSeconds = Convert.ToInt64(historyRow["last_visit_time"]);
					// Windows file time UTC is in nanoseconds, so multiplying by 10
					DateTime gmtTime = DateTime.FromFileTimeUtc(10 * utcMicroSeconds);
					// Converting to local time
					DateTime localTime = TimeZoneInfo.ConvertTimeFromUtc(gmtTime, TimeZoneInfo.Local);
					historyItem.VisitedTime = localTime;
					allHistoryItems.Add(historyItem);
				}
			}
		}
	}
    }

}
