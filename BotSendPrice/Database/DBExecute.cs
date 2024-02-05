using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseArkona
{
    public static class DBExecute
    {
        public static object SelectScalar(string Query, params SqlParameter[] Parameters)
        {
            if (Connection.ConnectToDataBase())
            {
                SqlCommand cmd = new SqlCommand(Query, Connection.SqlConnection);
                AddParameters(cmd, Parameters);
                object result = cmd.ExecuteScalar();
                Connection.CloseConnection();
                return result;
            }
            return null;
        }

        public static DataSet SelectDataSet(string Query, params SqlParameter[] Parameters)
        {
            if (Connection.ConnectToDataBase())
            {
                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(Query, Connection.SqlConnection);
                AddParameters(da.SelectCommand, Parameters);
                da.SelectCommand.CommandTimeout = 0;
                da.Fill(ds);
                Connection.CloseConnection();
                return ds;
            }
            return null;
        }

        private static void AddParameters(SqlCommand sqlCommand, SqlParameter[] Parameters)
        {
            if (Parameters != null && Parameters.Count() > 0)
            {
                foreach (var par in Parameters)
                    sqlCommand.Parameters.Add(par);
            }
        }

        public static DataTable SelectTable(string Query, params SqlParameter[] Parameters)
        {
            DataSet result = SelectDataSet(Query, Parameters);
            if (result != null && result.Tables.Count > 0)
                return result.Tables[0];
            return null;
        }

        public static DataRow SelectRow(string Query, params SqlParameter[] Parameters)
        {
            DataTable result = SelectTable(Query, Parameters);
            if (result != null && result.Rows.Count > 0)
                return result.Rows[0];
            return null;
        }

        public static bool ExecuteTranzactionQuery(string Query)
        {
            return ExecuteQuery("begin tran " + Query + " commit tran");
        }

        public static bool ExecuteQuery(string Query, params SqlParameter[] Parameters)
        {
            if (Connection.ConnectToDataBase())
            {
                SqlCommand cmd = new SqlCommand(Query, Connection.SqlConnection);
                AddParameters(cmd, Parameters);
                cmd.CommandTimeout = 0;
                cmd.ExecuteNonQuery();
                Connection.CloseConnection();
                return true;
            }
            return false;
        }

        public static int ExecuteQueryReturn(string Query, params SqlParameter[] Parameters)
        {
            if (Connection.ConnectToDataBase())
            {
                SqlCommand cmd = new SqlCommand(Query, Connection.SqlConnection);
                AddParameters(cmd, Parameters);
                cmd.CommandTimeout = 0;
                int res = cmd.ExecuteNonQuery();
                Connection.CloseConnection();
                return res;
            }
            return -1;
        }

        public static bool ExecuteProcedure(string NameProcedure, params SqlParameter[] Parameters)
        {
            if (Connection.ConnectToDataBase())
            {
                SqlCommand cmd = new SqlCommand(NameProcedure, Connection.SqlConnection);
                AddParameters(cmd, Parameters);
                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                Connection.CloseConnection();
                return true;
            }
            return false;
        }

        public static DataTable ExecuteProcedureTable(string NameProcedure, params SqlParameter[] Parameters)
        {
            if (Connection.ConnectToDataBase())
            {
                SqlCommand cmd = new SqlCommand(NameProcedure, Connection.SqlConnection);
                AddParameters(cmd, Parameters);
                cmd.CommandTimeout = 0;
                cmd.CommandType = CommandType.StoredProcedure;
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();

                da.Fill(ds);
                Connection.CloseConnection();

                if (ds != null && ds.Tables.Count > 0)
                    return ds.Tables[0];

            }
            return null;
        }
    }
}
