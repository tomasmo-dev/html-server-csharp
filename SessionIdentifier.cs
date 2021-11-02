using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace HtmlSocketServer
{

    public struct sesionId
    {
        public string id;
        public DateTime timeCreated;

        public sesionId(string i, DateTime created, bool logIn)
        {
            id = i;
            timeCreated = created;
        }

    }
    public struct sIdResponse
    {
        public string id;

        public sIdResponse(string i)
        {
            id = i;
        }
    }

    class SessionIdentifier
    {
        public static string postResponse = "HTTP/1.1 {0} {1}";

        private static string usableChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public static Dictionary<string, sesionId> ActiveSessions = new Dictionary<string, sesionId>();

        public static string GenerateSessionId(string cIp)
        {
            DateTime date = DateTime.Now;
            SHA512 hash = new SHA512Managed();
            Random r = new Random();

            string sesId = date.ToString();
            sesId += cIp;

            for (int x = 0; x < 12; x++)
            {
                sesId += usableChars[r.Next(usableChars.Length)].ToString();
            }
            char[] sesIdArr = sesId.ToCharArray();

            for (int index = 0; index < sesIdArr.Length; index++)
            {
                if (sesIdArr[index] == ' ')
                {
                    sesIdArr[index] = '_';
                }
            }

            sesId = new string(sesIdArr);

            sesId = Convert.ToBase64String(Encoding.ASCII.GetBytes(sesId));

            byte[] hashedId = hash.ComputeHash(Encoding.UTF8.GetBytes(sesId));
            sesId = "";

            foreach (byte b in hashedId)
            {
                sesId += String.Format("{0:x2}", b);
            }

            string formattedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string sqlString = "USE SiteResources";
            string insertString = $"INSERT INTO sessionIds VALUES ('{sesId}', '{formattedTime}')";

            SqlCommand command = new SqlCommand(sqlString, SQL_REFERENCES.siteDB_Reference);
            SqlCommand command1 = new SqlCommand(insertString, SQL_REFERENCES.siteDB_Reference);

            command.ExecuteNonQuery();
            command1.ExecuteNonQuery();



            return (sesId);

        }

        public static void DeleteOldRecords()
        {
            while (true)
            {
                SqlCommand command = new SqlCommand("DELETE FROM sessionIds WHERE dtCreated < DATEADD(HOUR, -2, GETDATE())", SQL_REFERENCES.siteDB_Reference);
                command.ExecuteNonQuery();
                Thread.Sleep(TimeSpan.FromMinutes(2));
            }
        }

    }
}
