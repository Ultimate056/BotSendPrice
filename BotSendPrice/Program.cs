using AutoSavePrices;
using BotSendPrice.Configurations;
using BotSendPrice.Models;
using BotSendPrice.Services;
using CreateCommerchialRequest.Helpers;
using DataBaseArkona;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BotSendPrice
{
    class Program 
    {
        private static int idSchedule = 1;

        public static int GetSchedule() => idSchedule;

        public static void WriteLine(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(text);
        }

        public static void SendBotLastAction(int idRobot)
        {
            SqlParameter idRobotPar = new SqlParameter("idRobot", idRobot);
            DBExecute.ExecuteQuery("update logRobot set dtLastAction = GetDate(), fSendedMessage = 1 where idRobot = @idRobot", idRobotPar);
        }

        //кодировка строки из UTF в WIN1251
        public static string UTF8ToWin1251(string sourceStr)
        {
            Encoding utf8 = Encoding.UTF8;
            Encoding win1251 = Encoding.GetEncoding(1251);
            byte[] utf8Bytes = utf8.GetBytes(sourceStr);
            byte[] win1251Bytes = Encoding.Convert(utf8, win1251, utf8Bytes);
            return win1251.GetString(win1251Bytes);
        }

        static void Main(string[] args)
        {
            DBExecute.ExecuteQuery("dbcc freeproccache");

            SendBotLastAction(5);

            Console.Title = "Kraken Bot 2023";
            Console.WriteLine("Бот Кракен запущен...");
            Console.WriteLine("_____________________________________");


            CultureInfoDefault.CheckAndSetCultureInfo();

            try
            {
                if(!Directory.Exists(Props.FolderPrices))
                {
                    Directory.CreateDirectory(Props.FolderPrices);
                }

                if (DeleteFoldersPrices() == 1)
                    throw new IOException("Удаление папок прайсов. ");


                Stopwatch startTime = Stopwatch.StartNew();
                // =============================================
                // 1 этап: Определение расписания (режим работы)
                // =============================================
                //startTime.Start();
                DateTime dtNow = DateTime.Now;
                int curHour = dtNow.Hour;
                int curMinutes = dtNow.Minute;
                if ((curHour == 3 && curMinutes > 30) || (curHour >= 4 && curHour < 7))
                {
                    try
                    {
                        DataRow row_params = DBExecutor.GetAppParam(50, 482);
                        if (row_params == null)
                            throw new NullReferenceException("Пустой список параметров 50,482");
                        object value_param = row_params["value_param"];
                        if (DBNull.Value.Equals(value_param) || value_param == null)
                            throw new NullReferenceException("Пустое значение параметра 50,482");

                        int day_diff = (int)value_param;
                        DateTime dtDiff = dtNow.AddDays(-day_diff);
                        DBExecutor.DeleteHistoryPrices(dtDiff);
                    }
                    catch (Exception ex)
                    {
                        WriteLogConsole(ConsoleColor.Red, "История прайсов не удалена.", "Причина : " + ex.Message, 3);
                    }
                    idSchedule = 1;
                }
                if (curHour >= 10 && curHour < 12) idSchedule = 2;
                if (curHour >= 13 && curHour < 15) idSchedule = 3;
                if (curHour >= 16 && curHour < 18) idSchedule = 4;
                if (curHour >= 22 && curHour < 23) idSchedule = 5;

                if (idSchedule == 0)
                    throw new InvalidOperationException("Неверное определение режима.");

                startTime.Start();

                // =============================================
                // 2 этап: Определение всех клиентов для формирования прайсов
                // =============================================
                WriteLogConsole(ConsoleColor.Gray, "Режим бота: ", idSchedule, 0);

                List<Client> allClients = DBExecutor.GetClientsTable(idSchedule) as List<Client>;

                WriteLine(ConsoleColor.Gray, "Загружено клиентов: " + allClients.Count);
                if (allClients == null || allClients.Count == 0)
                {
                    throw new NullReferenceException("Пустой список входных клиентов");
                }
                int countInClients = allClients.Count;

                //для отправки отдельному клиенту
                //allClients = allClients.Where(x => x.IdKontr == 556107).ToList();

                // Определение клиентов - по которым будут формировываться прайсы исходя из общих категорий
                List<Simbiot> HandleClients = GeneratorHandledClients.GetHandledSimbiots(ref allClients);

                if (HandleClients == null || HandleClients.Count == 0)
                {
                    throw new NullReferenceException("Пустой список клиентов, по кот. создаются прайсы");
                }

                WriteLine(ConsoleColor.Gray, "Будет создано: " + HandleClients.Count + " прайсов");

                int sumCountClients = 0;
                foreach (var item in HandleClients) sumCountClients += item.FilterClients.Count;

                bool isEqualCounts = sumCountClients >= countInClients;
                if (isEqualCounts)
                    WriteLogConsole(ConsoleColor.Yellow, "Info: кол-во клиентов (всего) ", $"Успех. Все {sumCountClients} клиентов разложены по полкам", 0);
                else
                    WriteLogConsole(ConsoleColor.Red, "Err: кол-во клиентов (всего) ", $"Нашлось {countInClients - sumCountClients} неразложенных клиентов", 3);

                // =============================================
                // 3 этап: Формирование очереди
                // =============================================

                int maxCountThreads = Props.MaxCountThreads;
                WriteLine(ConsoleColor.Gray, "Открыто потоков: " + Props.MaxCountThreads);

                int AverateExecuteThreadTime = 90; // Среднее время выполнения 1 задачи, с
                double orientirTime = (HandleClients.Count / maxCountThreads * AverateExecuteThreadTime) / 60;
                WriteLine(ConsoleColor.Gray, "Ориентировочное время выполнения задач: " + orientirTime + " мин");

                QAllClients que = new QAllClients(maxCountThreads, HandleClients);

                // =============================================
                // 4 этап: Запуск бота
                // =============================================

                WriteLine(ConsoleColor.Green, "Запуск бота...");
                WriteLine(ConsoleColor.Gray, "_____________________________________");
                MainExecutor main = new MainExecutor();
                main.StartBot(que);

                startTime.Stop();

                WriteLogConsole(ConsoleColor.Cyan, "", $"Info: Бот завершил свою работу за {MainExecutor.elapsedTime(startTime)}...", 0);

                //Console.ReadKey();

            }
            catch (Exception ex)
            {
                WriteLogConsole(ConsoleColor.Red, "Ошибка при выполнении Program.cs", ex.Message, 1);
                DBExecutor.InsertLog(29, idSchedule, ex.Message);
                Environment.Exit(0);
            }
        }



        /// <summary>
        /// Написать лог в консоль и в файл. Параметр mode - для UniLogger (0 - все норм, 1 - критичная ошибка, 3 - некритичная ошибка)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="color"></param>
        /// <param name="action"></param>
        /// <param name="text"></param>
        /// <param name="mode"></param>
        public static void WriteLogConsole<T>(ConsoleColor color, string action, T text, int mode)
        {
            WriteLine(ConsoleColor.Gray, $"{action} {text.ToString()}");
            UniLogger.WriteLog(action, mode, text.ToString());
        }


        private static int countTry = 1;
        /// <summary>
        /// Вычищение папок прайсов  на всякий случай
        /// </summary>
        /// <returns></returns>
        private static int DeleteFoldersPrices()
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(Props.FolderPrices);
                foreach (var dir in di.GetDirectories())
                {
                    dir.Delete(true);
                }
                return 0;
            }
            catch (IOException ioex) {
                countTry++;
                UniLogger.WriteLog($"Ошибка при удалении папок прайсов. Попытка {countTry} ", 1, ioex.Message);
                Thread.Sleep(5000);
                if (countTry <= 3)
                {
                    return DeleteFoldersPrices();
                }
                else
                    throw new Exception($"Не получилось удалить папки с прайсами. {ioex.Message}");
            }
            catch(Exception ex)
            {
                WriteLogConsole(ConsoleColor.Red, "Удаление папок прайсов", ex.Message, 1);
                return 1;
            }

        }

    }
}
