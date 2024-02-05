using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DataBaseArkona
{
    public static class DBUsers
    {
        public static object GetCurrentWindowUserId()
        {
            string query = @"select id_kontr from spr_kontr where sql_id = dbo.f_user_id('Arkona')";
            return DBExecute.SelectScalar(query);
        }

        public static object GetCurrentWindowUserName()
        {
            string query = @"select n_kontr from spr_kontr where sql_id = dbo.f_user_id('Arkona')";
            return DBExecute.SelectScalar(query);
        }

        public static DataRow checkUser(string userlogin, string userpassword)
        {
            return DBExecute.SelectRow(@"select u.idUser, k.n_kontr_full  from sLoginUser u join spr_kontr k on u.idUser = k.id_kontr where lower(u.Login) = @login and lower(u.Pass) = @pass", new System.Data.SqlClient.SqlParameter("@login", userlogin), new System.Data.SqlClient.SqlParameter("@pass", userpassword));
        }
    }
}
