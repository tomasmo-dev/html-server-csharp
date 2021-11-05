using System;
using System.IO;

namespace HtmlSocketServer
{
    /*
     * 1 line in cfg is for www folder path
     * 2 line in cfg is for port
     * 3 line in cfg is for server log txt file
     * 4 line in cfg is for ip from index
     * 5 line in cfg is for login js
     * 6 line in cfg for user credentials
     * 7 line in cfg for fileloader path
     * 8 line in cfg for sql connect string
     * 9 line in cfg for cookies
     */


    class Program
    {
        private static void Set_default_values()
        {

            string path;
            string port;
            string lpath;
            string ip_choose;
            string userPath;
            string loadPath;
            string sqlCS;
            string loginCookies;

            string exec_path = Directory.GetCurrentDirectory();

            string config_path = exec_path + @"/" + "Server_config.txt";
            ServerConfig.cfgPath = config_path;

            //StreamReader reader = new StreamReader(config_path);
            string[] reader = File.ReadAllLines(config_path);

            path = reader[0];
            port = reader[1];
            lpath = reader[2];
            ip_choose = reader[3];
            userPath = reader[5];
            loadPath = reader[6];
            sqlCS = reader[7];
            loginCookies = reader[8];


            ServerConfig.port = int.Parse(port);
            ServerConfig.path = path;
            ServerConfig.Log_path = lpath;
            ServerConfig.ip_index = int.Parse(ip_choose);
            ServerConfig.userFile = userPath;
            FileLoaderConfig.path = loadPath;
            TimeTableImporter.ttJSONLoader.CHeader = loginCookies;
            Constants.SQLconString = sqlCS;

        }

        static void debuggSetup() // not required for normal use
        {
            // Set variables for debugging here
            Constants.debuggerRes();
        }

        static void startup()
        {
            debuggSetup();
            //Set_default_values();

            sqlResources.ConnectToDB();

            Server.Start_listening();
        }
        static void Main(string[] args)
        {
            startup();
        }

    }
}