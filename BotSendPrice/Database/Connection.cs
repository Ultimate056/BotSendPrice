using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseArkona
{
    public static class Connection
    {
        private static SqlConnection _sqlConnection;

        public static SqlConnection SqlConnection { get { return _sqlConnection; } }
        static Connection()
        {
            ConnectToDataBase();
        }

        public static string ConnectionString
        {
            get
            {
                //return Program.mainConnection;
                return "Server=" + CommonProperty.DBServer + ";Database=" + CommonProperty.DBBase + ";Integrated Security=SSPI;Connect Timeout=1200";
            }
        }

        public static bool ConnectToDataBase()
        {
            _sqlConnection = new SqlConnection(ConnectionString);

            try
            {
                _sqlConnection.Open();
                return true;
            }
            catch
            {

                return false;
            }
        }

        public static void CloseConnection()
        {
            if (_sqlConnection != null && _sqlConnection.State == System.Data.ConnectionState.Open)
            {
                _sqlConnection.Close();
            }
        }
    }
}
