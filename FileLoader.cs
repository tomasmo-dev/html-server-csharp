using System;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace HtmlSocketServer
{
    class tTableAddr
    {
        public static string mon = "https://sis.ssakhk.cz/TimeTable/PO1.jpg";
        public static string tue = "https://sis.ssakhk.cz/TimeTable/UT1.jpg";
        public static string wen = "https://sis.ssakhk.cz/TimeTable/ST1.jpg";
        public static string thu = "https://sis.ssakhk.cz/TimeTable/CT1.jpg";
        public static string fri = "https://sis.ssakhk.cz/TimeTable/PA1.jpg";
    }

    class FileLoaderConfig
    {
        public static string path;
    }

    class FLoader
    {
        public static byte[] GetBytesFromFile(string fname)
        {
            try
            {
                return Encoding.ASCII.GetBytes(File.ReadAllText(FileLoaderConfig.path + @"/" + fname));

            }
            catch (Exception)
            {
                return new byte[] { 1, 0 };
            }
        }
        public static int countLines(SqlCommand statement)// retest problems here
        {

            SqlDataReader dR = statement.ExecuteReader();

            int c = 0;

            while (dR.Read())
            {
                c++;
            }

            dR.Close();

            return c;
        }

        public static bool checkLogin(string usr, string pwd)
        {
            SqlCommand tryGetTheUser = new SqlCommand($"SELECT * FROM userCredentials WHERE username LIKE '{usr}'", SQL_REFERENCES.siteDB_Reference);

            if (countLines(tryGetTheUser) > 0)
            {
                SqlDataReader reader = tryGetTheUser.ExecuteReader();

                reader.Read();
                if (reader.GetString(0) == usr && reader.GetString(1) == pwd)
                {
                    reader.Close();
                    return true;
                }
                else
                {
                    reader.Close();
                    return false;
                }

            }
            else
            {
                return false;
            }

        }
    }

    class timeTableUpdater
    {
        static string path = ServerConfig.path + @"/bTt.json";
        public static void updater()
        {
            while (true)
            { // update func
                ttUpdates();
                WeatherApi.LoadSaveWeatherData();

                Thread.Sleep(TimeSpan.FromHours(2.6));
            }

        }

        private static void ttUpdates()
        {
            WebClient client = new WebClient();

            DateTime now = DateTime.Now;
            int day;
            string selLink;

            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    day = 1;
                    selLink = tTableAddr.mon;
                    break;

                case DayOfWeek.Tuesday:
                    day = 2;
                    selLink = tTableAddr.tue;
                    break;

                case DayOfWeek.Wednesday:
                    day = 3;
                    selLink = tTableAddr.wen;
                    break;

                case DayOfWeek.Thursday:
                    day = 4;
                    selLink = tTableAddr.thu;
                    break;

                case DayOfWeek.Friday:
                    day = 5;
                    selLink = tTableAddr.fri;
                    break;

                default:
                    day = 1;
                    selLink = tTableAddr.mon;
                    break;
            }


            UpdatedUpdater(day);

            byte[] ttBytes = client.DownloadData(selLink);
            File.WriteAllBytes(ServerConfig.path + @"/images/tt.jpg", ttBytes);

        }

        static void UpdatedUpdater(int day)
        {
            string JSONtt = TimeTableImporter.ttJSONLoader.htmlTimeTDayToJSON(TimeTableImporter.ttJSONLoader.getTimeTableForDay(day));

            File.WriteAllText(path, JSONtt);
        }
    }
}
