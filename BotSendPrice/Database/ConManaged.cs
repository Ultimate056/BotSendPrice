using AutoSavePrices;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseArkona
{
    public class ConManaged
    {
        private SqlConnection _sqlConnection;

        public SqlConnection SqlConnection { get { return _sqlConnection; } }
        public ConManaged()
        {
            ConnectToDataBase();
        }

        public string ConnectionString
        {
            get
            {
                //return Program.mainConnection;
                return "Server=" + CommonProperty.DBServer + ";Database=" + CommonProperty.DBBase + ";Integrated Security=SSPI;Connect Timeout=1200";
            }
        }

        public bool ConnectToDataBase()
        {
            _sqlConnection = new SqlConnection(ConnectionString);
            try
            {
                _sqlConnection.Open();
                return true;
            }
            catch(Exception ex)
            {
                UniLogger.WriteLog($"Connect к БД", 1, $"Поток {Task.CurrentId} {ex.Message}");
                return false;
            }
        }

        public void CloseConnection()
        {
            if (_sqlConnection != null && _sqlConnection.State == System.Data.ConnectionState.Open)
            {
                _sqlConnection.Close();
            }
        }
    }
}
