using AutoSavePrices.Configurations;
using AutoSavePrices.Models;
using AutoSavePrices.Services;
using BotSendPrice;
using BotSendPrice.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using BotSendPrice.Configurations;
using System.Text;

namespace AutoSavePrices
{
    public class MainExecutor
    {
        // Пул отслеживаемых потоков
        public List<Task> PoolTasks = new List<Task>();


        /// <summary>
        /// Запуск бота, первых потоков и отслеживание в цикле потоков
        /// Если поток из очереди завершен, то запускаем новый поток
        /// </summary>
        /// <param name="needClients"></param>
        public void StartBot(QAllClients needClients)
        {
            // Первые клиенты
            Simbiot[] topClients = needClients.GetTopClients();

            // Запуск первых потоков
            Start(topClients);

            // В главном потоке отслеживаем состояния потоков
            while (true)
            {
                if (PoolTasks.Count > 0)
                {
                    for (int i = 0; i < PoolTasks.Count; i++)
                    {
                        if (PoolTasks[i].IsFaulted ||
                            PoolTasks[i].IsCompleted ||
                            PoolTasks[i].IsCanceled)
                        {
                            PoolTasks.RemoveAt(i);

                            if (needClients.countClients > 0)
                            {
                                var nextClient = needClients.Next();
                                GenerateTask(nextClient);
                                i = 0; i--;
                            }
                        }
                    }
                }
                else
                {
                    if (needClients.countClients > 0)
                    {
                        // Берем еще сколько необходимо
                        for (int i = 0; i < needClients.countClients; i++)
                        {
                            GenerateTask(needClients.Next());
                        }
                    }
                    else
                    {
                        // Ждем когда довыполняются задачи
                        Task.WaitAll(PoolTasks.ToArray());
                        break;
                    }
                }
            }

            GC.Collect();


        }

        public static string StringToUTF8(string text)
        {
            byte[] uft8Data = Encoding.UTF8.GetBytes(text);
            return Encoding.UTF8.GetString(uft8Data);
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        // Формирует и запускает задачу по клиенту
        public void GenerateTask(Simbiot client)
        {
            bool isErrorTask = false;

            // Настройка путей для файла
            RoutePrice route = new RoutePrice()
            {
                IdClient = client.HandleClient.IdKontr.ToString(),
                Category = client.HandleClient.Category.ToString(),
                NameFile = ConfExp.GetNamePrice(client.HandleClient),
                //Subject = ConfExp.GetSubject(client.HandleClient),
                Subject = "=?UTF-8?B?" + Base64Encode(ConfExp.GetSubject(client.HandleClient)) + "?=",
                //раскомментить для запуска
                Email = client.HandleClient.Email,
                //Email = Props.DebugEmail,
                TypeSend = client.TypeSend,
                NameCompany = client.HandleClient.NameKontr,
                NameCategory = client.TypeSend != TypeSendPrice.Individual ?
                                client.Category + " " + client.TypeSend.ToString() :
                                client.TypeSend.ToString() + " " + client.HandleClient.IdKontr,
                Client = client.HandleClient,
                MainClient = client.HandleClient
            };
            route.FullPath = ConfExp.GeneratePath(ref route, client.Category, client.TypeSend);
            string nameCategory = route.NameCategory;



            Task newTask = new Task(() =>
            {
                try
                {
                    if (route.FullPath == null)
                        throw new NullReferenceException("Ошибка при формировании пути до главного прайса");
  
                    Program.WriteLine(ConsoleColor.Yellow, $"Info: Началось формирование прайса и отправка клиентам категории {nameCategory}");
                    Stopwatch startTime = Stopwatch.StartNew();
                    // Получаем данные
                    DataTable dt = DBExecutor.GetPricesTable(client);

                    if (dt == null || dt.Rows.Count == 0)
                        throw new NullReferenceException($"Err: Пустые прайсы. Клиент от которого формировались прайсы: {client.HandleClient.IdKontr}. Терр: {client.HandleClient.IdTerritory}, Кат: {client.HandleClient.Category}");

                    // Экспортируем в Excel
                    var exp = new ExportToExcel(dt);
                    bool isExported = exp.StartExport(route);
                    startTime.Stop();
                    dt.Dispose();
                    if (isExported)
                        UniLogger.WriteLog($"Suc: Экспорт прайсов категории {nameCategory} успешно завершен за ", 0, elapsedTime(startTime));
                    else
                        throw new Exception($"Err: Ошибка при переносе в xls. Категория {nameCategory}");

                    // Отправка email всем кто находится в категории
                    startTime.Start();
                    var sme = new SendMailExecutor();
                    int countSended = sme.SendEmailAsync(client, route).Result;
                    
                    startTime.Stop();

                    if (countSended.Equals(client.FilterClients.Count))
                    {
                        UniLogger.WriteLog($"Suc: Всем ({countSended} кл.) отправились прайсы категории  {nameCategory} за ", 0, elapsedTime(startTime));
                        Program.WriteLine(ConsoleColor.Green, $"Suc: Всем ({countSended} кл.) отправились прайсы категории  {nameCategory} за {elapsedTime(startTime)}");
                    }
                    else
                        throw new Exception($"Err: Не Отправилось {client.FilterClients.Count - countSended}  прайсов  категории {nameCategory}");
                }
                catch (Exception ex)
                {
                    UniLogger.WriteLog($"Err: Не отработал таск по категории {nameCategory} ", 1, ex.Message);
                    Program.WriteLine(ConsoleColor.Red, $"Err: Не отработал таск по категории {nameCategory}. Описание ошибки: {ex.Message}");
                    DBExecutor.InsertLog(29, Program.GetSchedule(), route, isByCategory: true, errorMes: ex.Message);
                    isErrorTask = true;
                }
                finally
                {
                    if (Directory.Exists(route.PathDirectory))
                    {
                        Directory.Delete(route.PathDirectory, true);
                    }
                    string tempErr = isErrorTask ? "с ошибкой" : "";
                    Program.WriteLine(isErrorTask ? ConsoleColor.Red : ConsoleColor.DarkYellow, $"Info: Задача по категории {nameCategory} завершена {tempErr}");
                }
            });
            
            PoolTasks.Add(newTask);
            newTask.Start();
        }


        // Формируем потоки первых клиентов
        public void Start(Simbiot[] firstClients)
        {
            foreach(var client in firstClients)
            {
                GenerateTask(client);
                Thread.Sleep(300);
            }
        }

        

        public static  string elapsedTime(Stopwatch time)
        {
            return String.Format("{0} мин | {1} сек",
                            time.Elapsed.Minutes,
                            time.Elapsed.Seconds);
        }

        

    }
}
