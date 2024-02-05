using AutoSavePrices;
using BotSendPrice.Models;
using DataBaseArkona;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSendPrice.Services
{
    public class DBExUnitTest
    {
        private string connectionString = "Server=" + CommonProperty.DBServer + ";Database=" + CommonProperty.DBBase + ";Integrated Security=SSPI;Connect Timeout=12000";
        public DBExUnitTest()
        {

        }

        public DataTable GetPricesTable(Simbiot client)
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

                UniLogger.WriteLog("Старт П.прайсов", 4, $"поток: {Task.CurrentId} {client.HandleClient.IdKontr} {client.HandleClient.Category} {DateTime.Now.ToLongTimeString()}");

                DataTable dtRes = new DataTable();
                using (var connection = new SqlConnection(connectionString))
                {
                    using (var command = connection.CreateCommand())
                    {
                        try
                        {
                            if (connection.State != ConnectionState.Open)
                                connection.Open();
                            command.CommandText = sql;
                            command.CommandTimeout = 1500;
                            var dr = command.ExecuteReader();

                            dtRes.Load(dr);
                            dr.Close();

                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                int countRowsDebug = dtRes.Rows.Count;
                UniLogger.WriteLog("Вернул прайс", 4, $"поток: {Task.CurrentId} {client.HandleClient.IdKontr} {client.HandleClient.Category} {DateTime.Now.ToLongTimeString()}");
                UniLogger.WriteLog("Кол-во прайсов", 4, $"поток: {Task.CurrentId} {client.HandleClient.IdKontr} {client.HandleClient.Category} {DateTime.Now.ToLongTimeString()} Получено строк {countRowsDebug}");
                return dtRes;
            }
            catch (Exception ex)
            {
                UniLogger.WriteLog("Ошибка при генерация прайсов SQL. ", 1, ex.Message);
                return null;
            }
        }
    }
}
