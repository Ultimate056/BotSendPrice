using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseArkona
{
    public static class DBExecuteMan
    {
        public static object SelectScalar(string Query, params SqlParameter[] Parameters)
        {
            ConManaged con = new ConManaged();
            SqlCommand cmd = new SqlCommand(Query, con.SqlConnection);
            AddParameters(cmd, Parameters);
            object result = cmd.ExecuteScalar();
            con.CloseConnection();
            return result;
        }

        public static DataSet SelectDataSet(string Query, params SqlParameter[] Parameters)
        {
            ConManaged con = new ConManaged();
            DataSet ds = new DataSet();
            SqlDataAdapter da = new SqlDataAdapter(Query, con.SqlConnection);
            AddParameters(da.SelectCommand, Parameters);
            da.SelectCommand.CommandTimeout = 0;
            da.Fill(ds);
            con.CloseConnection();
            return ds;
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
            ConManaged con = new ConManaged();
            SqlCommand cmd = new SqlCommand(Query, con.SqlConnection);
            AddParameters(cmd, Parameters);
            cmd.CommandTimeout = 0;
            cmd.ExecuteNonQuery();
            con.CloseConnection();
            return true;
        }

        public static int ExecuteQueryReturn(string Query, params SqlParameter[] Parameters)
        {
            ConManaged con = new ConManaged();
            SqlCommand cmd = new SqlCommand(Query, con.SqlConnection);
            AddParameters(cmd, Parameters);
            cmd.CommandTimeout = 0;
            int res = cmd.ExecuteNonQuery();
            con.CloseConnection();
            return res;
        }


    }
}
