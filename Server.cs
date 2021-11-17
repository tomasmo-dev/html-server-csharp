using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HtmlSocketServer
{
    public static class ServerConfig
    {
        public static int buffer_Size = 1024;

        public static int port;
        public static int ip_index;
        public static string IpAdd;

        public static string path;

        public static string Log_path;
        public static string cfgPath;
        public static string userFile;
    }
    public class FileTypes
    {
        public static string html_response = "HTTP/1.1 200 OK\r\n";

        public static string jpg_type = "Content-Type: image/jpeg\r\n";
        public static string html_type = "Content-Type: text/html\r\n";
        public static string css_type = "Content-Type: text/css\r\n";
        public static string js_type = "Content-Type: text/javascript\r\n";
        public static string json_type = "Content-Type: application/json\r\n";
    }
    public class Server
    {
        public static void Log_text(string s)
        {
            //if (ServerConfig.Log_path == null)
            //{
            //    return;
            //}

            //using (StreamWriter w = File.AppendText(ServerConfig.Log_path))
            //{
            //    w.WriteLine(s);
            //}
        }
        public static string[] getCredentials(string postBody)
        {
            string[] response = new string[2] { "false", "false" };

            if (postBody == "!?false?!") return response;

            var credentials = JObject.Parse(postBody);

            response[0] = credentials["user"].ToString();
            response[1] = credentials["pwd"].ToString();

            return response;
        }
        public static bool checkLoginValidity(string[] headers)
        {

            string jsonValues = ServerFunctions.getPostBody(headers);

            if (jsonValues == "!?false?!") return false;

            var response = JObject.Parse(jsonValues);

            try
            {
                if (FLoader.checkLogin(response["user"].ToString(), response["pwd"].ToString()))
                {
                    return true;

                }
                else
                {
                    return false;
                }

            }
            catch (Exception)
            {

                return false;
            }
        }

        public static string GetCookies(string[] headers)
        {
            foreach (string item in headers)
            {
                if (Regex.IsMatch(item, @"Cookie[:]"))
                {
                    return item;
                }
            }

            return "false";
        }

        private static void HandleClient(object arg)
        {
            bool sysCall = false;
            bool getuserQ = false;

            Socket handler = (Socket)arg;
            string clientIp = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();

            string data;
            byte[] bytes = new Byte[ServerConfig.buffer_Size];

            string[] headers = null;
            string filename = null;

            try
            {

                int recvBytes = handler.Receive(bytes);
                data = Encoding.ASCII.GetString(bytes, 0, recvBytes);


                headers = data.Split('\n');
                filename = headers[0].Split(' ')[1];

            }
            catch (Exception)
            {
                getuserQ = true;
            }


            switch (filename)
            {
                case "/":
                    filename = "/index.html";
                    break;

                case "/Login":
                    filename = "/adLogin/login.html";
                    break;

                #region session_id

                case "/$getId":
                    sysCall = true;

                    string sesId = "";

                    try
                    {
                        sesId = ServerFunctions.checkGenerateSID(headers, clientIp, false);

                    }
                    catch (Exception)
                    {

                        getuserQ = true;
                    }

                    byte[] response = Encoding.ASCII.GetBytes(sesId);

                    byte[] r = Encoding.ASCII.GetBytes(String.Format(SessionIdentifier.postResponse, 200, "OK") + "\r\n")
                                                       .Concat(Encoding.ASCII.GetBytes(FileTypes.json_type + "\r\n"))
                                                       .Concat(response).ToArray();

                    ProtectedSocketSend(r, handler);

                    break;
                #endregion

                #region ttReply
                case "/$getTimeTable":
                    sysCall = true;
                    ProtectedSocketSend(ServerFunctions.getTTJSONresp(), handler);
                    break;

                case "/TimeTable":
                    filename = "/timetable/betterTable.html";
                    break;
                #endregion

                #region load-securePG
                case "/login-pages":
                    sysCall = true;
                    string sId = "";
                    try
                    {
                        sId = ServerFunctions.checkGenerateSID(headers, clientIp, true);

                    }
                    catch (Exception)
                    {
                        getuserQ = true;
                    }

                    byte[] pageResponse;

                    int rCode;
                    string rM;

                    if (sId != "false")
                    {
                        pageResponse = FLoader.GetBytesFromFile("/index.html");
                        rCode = 200;
                        rM = "OK";
                    }
                    else
                    {
                        pageResponse = Encoding.ASCII.GetBytes("Kinda nice try <br > <a href=\"../Login\">Go back</a>"); //dont return page only bad code
                        rCode = 403;
                        rM = "FORBIDDEN";
                    }

                    byte[] CResponse = Encoding.ASCII.GetBytes(($"HTTP/1.1 {rCode} {rM}\r\n"))
                                                     .Concat(Encoding.ASCII.GetBytes(FileTypes.html_type + "\r\n"))
                                                     .Concat(pageResponse)
                                                     .ToArray();

                    ProtectedSocketSend(CResponse, handler);

                    break;

                case "/login-pages/$getCurrentUser":
                    sysCall = true;

                    string cookieIdent = GetCookies(headers);

                    string username = ServerFunctions.getUsername(cookieIdent);

                    byte[] u_content;

                    if (username == "false")
                    {
                        u_content = Encoding.ASCII.GetBytes("Server-Side Error");
                    }
                    else
                    {
                        u_content = Encoding.ASCII.GetBytes(username);
                    }

                    byte[] u_response = Encoding.ASCII.GetBytes($"HTTP/1.1 200 OK\r\n").Concat(Encoding.ASCII.GetBytes(FileTypes.html_type + "\r\n")).Concat(u_content).ToArray();

                    ProtectedSocketSend(u_response, handler);

                    break;


                #endregion

                #region bot-redirects
                case "/config/getuser?index=0":
                    filename = "/error_pages/exception_page.html";
                    break;

                case "/.env":
                    filename = "/error_pages/exception_page.html";
                    break;

                case "/phpmyadmin/":
                    filename = "/error_pages/exception_page.html";
                    break;

                #endregion

                #region W_API

                case "/apis/Weather/":
                    filename = "/apis/Weather/wapi.html";
                    break;

                case "/apis/Weather/$getWeather":
                    sysCall = true;
                    string wJSON = WeatherApi.GetWeatherJson();

                    byte[] byte_wJSON = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK").Concat(Encoding.ASCII.GetBytes(FileTypes.json_type + "\r\n")).Concat(Encoding.ASCII.GetBytes(wJSON)).ToArray();

                    ProtectedSocketSend(byte_wJSON, handler);

                    break;

                #endregion


                #region Server_Secret
                case "/$GetSSecret":

                    try
                    {
                        string secretSID = ServerFunctions.checkGenerateSID(headers, clientIp);
                    }
                    catch (Exception)
                    {

                        filename = "/error_pages/404.html";
                    }

                    break;
                #endregion

            }

            if (!sysCall)
            {
                string fileType = null;
                try
                {
                    string[] _fileType = filename.Split('.');
                    fileType = _fileType[_fileType.Length - 1];

                }
                catch (Exception)
                {

                    getuserQ = true;
                }


                if (!getuserQ)
                {
                    // Finds file type to send to browser
                    switch (fileType)
                    {
                        case "jpg":
                            fileType = FileTypes.jpg_type;
                            break;

                        case "ico":
                            fileType = FileTypes.jpg_type;
                            break;

                        case "css":
                            fileType = FileTypes.css_type;
                            break;

                        case "html":
                            fileType = FileTypes.html_type;
                            break;

                        case "js":
                            fileType = FileTypes.js_type;
                            break;

                        default:
                            fileType = FileTypes.html_type;
                            break;

                    }

                    //filename = filename.Replace("/", @"\");
                    //Path + file
                    filename = ServerConfig.path + filename;

                }
                else
                {
                    fileType = FileTypes.html_type;
                    filename = ServerConfig.path + @"/error_pages/exception_page.html";
                }

                Console.WriteLine("Request from : " + clientIp + " Request : " + filename);
                Log_text("Request from : " + clientIp + " Request : " + filename);

                try
                {
                    byte[] content = File.ReadAllBytes(filename);

                    byte[] response = Encoding.ASCII.GetBytes(FileTypes.html_response)
                                                            .Concat(Encoding.ASCII.GetBytes(fileType))
                                                            .Concat(Encoding.ASCII.GetBytes("\r\n"))
                                                            .Concat(content)
                                                            .ToArray();

                    ProtectedSocketSend(response, handler);
                }
                catch (Exception)
                {
                    Console.WriteLine("Page not found by : " + clientIp);

                    Log_text("404 : " + clientIp);

                    string[] response_404 = { "HTTP/1.1", "404 NOT FOUND\r\n\r\n" };
                    string error_path = ServerConfig.path + @"/" + "error_pages" + @"/" + "404.html";   //CombinePaths(ServerConfig.path, "error_pages");
                                                                                                        //error_path = CombinePaths(error_path, "404.html");
                    byte[] content = File.ReadAllBytes(error_path);

                    byte[] response = Encoding.ASCII.GetBytes(response_404[0])
                                                    .Concat(Encoding.ASCII.GetBytes(response_404[1]))
                                                    .Concat(content)
                                                    .ToArray();

                    ProtectedSocketSend(response, handler);

                }

            }
            try
            {

                handler.Dispose();
                handler.Close();
            }

            catch (Exception)
            {

            }
        }
        public static void Start_listening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ip = ipHostInfo.AddressList[ServerConfig.ip_index];
            IPEndPoint localEndPoint = new IPEndPoint(ip, ServerConfig.port);

            ServerConfig.IpAdd = ipHostInfo.AddressList[ServerConfig.ip_index].ToString();
            //Program.Edit_ip_in_js(ServerConfig.IpAdd);

            Thread idWatch = new Thread(SessionIdentifier.DeleteOldRecords);
            idWatch.IsBackground = true;
            idWatch.Start();

            Thread timetable = new Thread(timeTableUpdater.updater);
            timetable.IsBackground = true;
            timetable.Start();

            Console.WriteLine("-----------------------------------------");
            Console.WriteLine("Server Running on : ");
            Console.WriteLine(ipHostInfo.AddressList[ServerConfig.ip_index] + " Address");
            Console.WriteLine(localEndPoint.Port + " Port");
            Console.WriteLine(ipHostInfo.AddressList.Length + " : available ips");
            Console.WriteLine("Starting sessionId watcher...");
            Console.WriteLine("-----------------------------------------");

            Socket listener = new Socket(ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(50);

                while (true)
                {
                    Socket handler;

                    try
                    {
                        handler = listener.Accept();

                    }
                    catch (Exception)
                    {
                        handler = null;
                        continue;
                    }

                    string connection_address = ((IPEndPoint)handler.RemoteEndPoint).Address.ToString();
                    Console.WriteLine("Connection from : " + connection_address);
                    Log_text("client connected : " + connection_address);

                    try
                    {
                        object[] thread_pass = { handler };

                        Thread handle_client = new Thread(HandleClient);
                        handle_client.IsBackground = true;

                        handle_client.Start(handler);

                    }
                    catch (Exception)
                    {

                        continue;
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
                Environment.Exit(77);
            }

        }

        static bool ProtectedSocketSend(byte[] msg, Socket handle)
        {
            try
            {
                handle.Send(msg);
                return true;
            }
            catch (Exception)
            {

                return false;
            }
        }

    }

}

//string[] files = Directory.GetFiles(ServerConfig.path);
//string[] folders = Directory.GetDirectories(ServerConfig.path);

//files = files.Concat(folders).ToArray();

//foreach (string f in files)
//{
//    Console.WriteLine(f);
//}