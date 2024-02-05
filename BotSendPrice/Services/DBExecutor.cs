using AutoSavePrices.Models;
using BotSendPrice.Models;
using Dapper;
using DataBaseArkona;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AutoSavePrices
{
    public static class DBExecutor
    {
        public static DataTable GetPricesTable(Simbiot client)
        {
            try
            {
                decimal idKontr = client.isNeedKontr ? client.HandleClient.IdKontr : 0;
                string sql = $@"exec sp_getPriceImag 
                                {idKontr}, 
                                {client.HandleClient.Category},
                                60,
                                0, --idCur
                                2, -- inPrice
                                {client.HandleClient.IdTerritory},
                                0, --gnotwithougbd
                                '', -- листы всякие
                                '',
                                '',
                                '',
                                '',
                                ''";

                return DBExecuteMan.SelectTable(sql);
            }
            catch (Exception ex)
            {
                UniLogger.WriteLog("Ошибка при генерация прайсов SQL. ", 1, ex.Message);
                return null;
            }
        }
        public static DataRow GetAppParam(int idApp, int idParam)
        {
            string sql = $@"SELECT application_param.id_application,   
                             application_param.id_param,   
                             application_param.naim_param,   
                             application_param.value_param,   
                             application_param.yes_no
                            FROM application_param (nolock)
                            WHERE
                            application_param.id_application = {idApp}
                            and application_param.id_param = {idParam}";
            return DBExecute.SelectRow(sql);
        }

        public static int FlagSendMSK => Convert.ToInt32(GetAppParam(50, 464)["value_param"]);

        public static int GetIdKontrByEmail(string email)
        {
            string sql = $"select id_kontr from spr_kontr (nolock) where el_mail = {email.Trim()}";
            return 0;
        }

        public static void DeleteHistoryPrices(DateTime date)
        {
            string sql = $@"delete from spriceHistory where datePrice < @date";
            SqlParameter par_date = new SqlParameter("date", date);
            DBExecute.ExecuteQuery(sql, par_date);
        }

        public static int GetCategory(int idKontr)
        {
            string sql = $"select top 1 category from spr_agent_kontr (nolock) where id_kontr = {idKontr}";
            int? category = DBExecute.SelectScalar(sql) as int?;
            return category.HasValue ? category.Value : 0;
        }

        public static IList<Client> GetClientsTable(int idSchedule)
        {
            try
            {
                using (IDbConnection db = Connection.SqlConnection)
                {
                    return db.Query<Client>($@"select 
                    id_kontr as IdKontr,
                    n_kontr as NameKontr,
                    category,
                    havediscount as Discount,
                    email,
                    fpraisimag as F_PriceImag,
                    fsendok as F_Sendok,
                    fkontrimag as F_KontrImag,
                    iddirect,
                    idterritory,
                    nprice as NamePrice,
                    daynum,
                    nsubj as NameSubject,
                    fhavenotrecprice as F_NotNeedRRC,
                    fhavenotrecpricebr as F_NotNeedRRCByBrand,
                    farkonabonus as F_ArkonaBonus,
                    priceRegionCastrole as PriceRegionCastrol,
                    fnotseebp as F_NotSeeBP,
                    fSpecialPrice as F_SpecialPrice,
                    ftwosklad as F_TwoSklad
                    from f_getClientsBotPrices({idSchedule})").ToList();
                }
            }
            catch (Exception ex)
            {
                UniLogger.WriteLog("Ошибка при генерации клиентов SQL.", 1, ex.Message);
                return null;
            }
        }

        public static void InsertLog(int idTypeAction, int idSchedule, RoutePrice route, bool isByCategory = false, string errorMes = "")
        {
            if (idTypeAction < 29 && idTypeAction > 30)
                return;
            string Content = "";
            string sql = "";
            DateTime dt = DateTime.Now;
            SqlParameter p_date = new SqlParameter("@date", dt);
            string nameUser = "ARKONA\\veles";
            if (idTypeAction == 29)
            {
                if(isByCategory)
                {
                    Content = $"[{idSchedule}] Не отправлены прайс-листы покупателям категории ({route.Category}) {route.TypeSend.ToString()} - " + errorMes;
                    sql = $"insert LogeOrder select {route.IdClient}, @date, {idTypeAction}, '{Content}', 1, null, null, '{nameUser}'";
                }
                else
                {
                    Content = $"[{idSchedule}] Не отправлен прайс-листы покупателю ({route.IdClient}) {route.NameCompany} - " + errorMes;
                    sql = $"insert LogeOrder select {route.IdClient}, @date, {idTypeAction}, '{Content}', 1, null, null, '{nameUser}'";
                }
            }
            if (idTypeAction == 30)
            {
                Content = $"[{idSchedule}] Прайс-лист отправлен по адресу {route.Email}, код покупателя {route.IdClient}, категория {route.Category}.";
                sql = $"insert LogeOrder select {route.IdClient}, @date, {idTypeAction}, '{Content}', 0, null, null, '{nameUser}'";
            }

            DBExecuteMan.ExecuteQuery(sql, p_date);
        }


        public static void InsertLog(int idTypeAction, int idSchedule, string message = "")
        {
            if (idTypeAction < 29 && idTypeAction > 30)
                return;
            string Content = "";
            string sql = "";
            DateTime dt = DateTime.Now;
            SqlParameter p_date = new SqlParameter("@date", dt);
            string nameUser = "ARKONA\\veles";
            if (idTypeAction == 29)
            {
                Content = $"[{idSchedule}] {message}";
                sql = $"insert LogeOrder select -1, @date, {idTypeAction}, '{Content}', 1, null, null, '{nameUser}'";
            }
            if (idTypeAction == 30)
            {
                Content = $"[{idSchedule}] {message}";
                sql = $"insert LogeOrder select -1, @date, {idTypeAction}, '{Content}', 0, null, null, '{nameUser}'";
            }

            DBExecuteMan.ExecuteQuery(sql, p_date);
        }
    }
}
