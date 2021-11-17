using Newtonsoft.Json;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HtmlSocketServer
{
    public class ServerFunctions
    {
        static bool SessionIdExists(string sId)
        {
            if (sId == "")
            {
                return false;
            }

            string sIdValue = Regex.Split(sId, ": ")[1];
            sIdValue = Regex.Replace(sIdValue, "\\r$", "");

            SqlCommand idCheck = new SqlCommand($"SELECT * FROM sessionIds WHERE id LIKE '{sIdValue}'", SQL_REFERENCES.siteDB_Reference);

            if (FLoader.countLines(idCheck) == 1)
            {
                return true;
            }
            else
            {
                System.Console.WriteLine($"{FLoader.countLines(idCheck)} | Multiple Session Ids found attention required");
                return false;   // true?
            }
        }
        static string generateSessionId_Object(string cIp, string username)
        {
            string sId;
            sId = SessionIdentifier.GenerateSessionId(cIp, username);
            sIdResponse response = new sIdResponse(sId);

            string serializedId = JsonConvert.SerializeObject(response);
            return serializedId;
        }

        public static string checkGenerateSID(string[] headers, string cip, bool OnlyCheck = true)
        {
            string sid;
            bool cookies_found;

            string postBody = getPostBody(headers);
            string[] credentials = Server.getCredentials(postBody);

            string response;

            sid = Server.GetCookies(headers);
            cookies_found = sid != "false";


            if (!cookies_found && !OnlyCheck)
            {
                string serializedId;
                if (Server.checkLoginValidity(headers))
                {
                    serializedId = generateSessionId_Object(cip, credentials[1]);

                }
                else
                {

                    string WId;
                    WId = "wrong_creds";
                    sIdResponse r = new sIdResponse(WId);

                    serializedId = JsonConvert.SerializeObject(r);
                }
                response = serializedId;
            }
            else if (cookies_found && OnlyCheck)
            {
                if (SessionIdExists(sid))
                {
                    response = sid;
                }
                else
                {
                    response = "false";
                }
            }
            else if (cookies_found && !OnlyCheck)
            {
                string serializedId;
                if (Server.checkLoginValidity(headers))
                {
                    serializedId = generateSessionId_Object(cip, credentials[1]);
                }
                else
                {
                    string WId;
                    WId = "wrong_creds";
                    sIdResponse r = new sIdResponse(WId);

                    serializedId = JsonConvert.SerializeObject(r);
                }
                response = serializedId;
            }
            else if (!cookies_found && OnlyCheck)
            {
                response = "false";
            }
            else
            {
                response = "false";
            }

            return response;

        }

        public static string getPostBody(string[] headers)
        {
            int index = 0;
            int bodyHeaderIndex = 0;
            int hits = 0;

            string response;

            foreach (string item in headers)
            {
                if (item == "\r")
                {
                    bodyHeaderIndex = index;
                    hits++;
                }
                index++;
            }
            if (hits == 0 || hits > 1)
            {
                response = "!?false?!";
            }


            if (bodyHeaderIndex >= headers.Length)
            {
                response = "!?false?!";
            }
            else
            {
                response = headers[bodyHeaderIndex + 1];
            }


            return response;
        }
        public static byte[] getTTJSONresp()
        {
            string TableResponse = FileTypes.html_response + FileTypes.json_type + "\r\n";
            string ttJson = File.ReadAllText(ServerConfig.path + @"/bTt.json");

            byte[] r = Encoding.UTF8.GetBytes(TableResponse).Concat(Encoding.ASCII.GetBytes(ttJson)).ToArray();

            return r;
        }

    }
}
