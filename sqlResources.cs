using System.Data.SqlClient;

namespace HtmlSocketServer
{
    class SQL_REFERENCES
    {
        public static SqlConnection siteDB_Reference;
    }

    class sqlResources
    {
        public static void ConnectToDB()
        {
            SqlConnection tempConVar = new SqlConnection();
            tempConVar.ConnectionString = Constants.SQLconString;
            SQL_REFERENCES.siteDB_Reference = tempConVar;
            SQL_REFERENCES.siteDB_Reference.Open();

        }


    }
}
