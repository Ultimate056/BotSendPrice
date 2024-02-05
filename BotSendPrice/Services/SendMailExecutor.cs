using System;
using System.Text;
using System.Threading.Tasks;
using AutoSavePrices.Models;
using System.IO;
using BotSendPrice.Models;
using AutoSavePrices.Configurations;
using System.Net;
using System.Net.Mail;
using System.Collections.Generic;
using BotSendPrice;
using BotSendPrice.Configurations;
using System.Net.Http.Headers;

namespace AutoSavePrices.Services
{
    public class SendMailExecutor
    {
        private readonly string from = Props.BoxArkonaLogin;
        private readonly string password = Props.BoxArkonaPassword;
        private readonly string mailToMy = Props.DebugEmail;


        private const string spareMail = "muhinan@arkona36.ru";
        private const string sparePassword = "jhjfjc15";

        public SendMailExecutor()
        {
            
        }

        public static string StringToUTF8(string text)
        {
            byte[] uft8Data = Encoding.UTF8.GetBytes(text);
            return Encoding.UTF8.GetString(uft8Data);
        }

        private string GetHtmlBodyDebug(RoutePrice route) =>
         $"<b>Id клиента:</b> {route.IdClient} </br>" +
            $"<b>Id клиента ПФП:</b> {route.MainClient.IdKontr} </br>" +
            $"<b>Тип выборки:</b> {route.TypeSend.ToString()} </br>" +
            $"<b>Категория клиента:</b> {route.Category} </br>" +
            $"<b>Категория клиента ПФП:</b> {route.MainClient.Category} </br>" +
            $"<b>Имя файла:</b> {route.Client.NamePrice.Trim()} </br>" +
            $"<b>Имя темы сообщения:</b> {route.Client.NameSubject.Trim()} </br>" +
            $"<b>Работает по двум складам ПФП:</b> {route.MainClient.F_TwoSklad} </br>" +
            $"<b>Территория:</b> {route.Client.IdTerritory} </br>" +
            $"<b>Территория ПФП:</b> {route.MainClient.IdTerritory} </br>" +
            $"<b>Почта: </b> {route.Client.Email} </br>";


        /// <summary>
        /// формирование subject с префиксом и постфиксом в кодировке base64
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        private MailMessage GetMailMessage(RoutePrice route)
        {
            try
            {
                //MailMessage mailMes = new MailMessage(from, mailToMy);
                //раскомментить для запуска

                MailMessage mailMes = new MailMessage(from, route.Email);
                Attachment price = new Attachment(route.FullPath);
                mailMes.Attachments.Add(price);
                mailMes.Subject = "=?UTF-8?B?" + Base64Encode(route.Subject) + "?=";
                mailMes.Body = "";// GetHtmlBodyDebug(route);
                mailMes.IsBodyHtml = true;
                
                if (mailMes.Attachments.Count == 0) throw new NullReferenceException("Нет вложения прайса");

                return mailMes;
            }
            catch(Exception ex)
            {
                UniLogger.WriteLog($"Ошибка при формировании сообщения клиенту {route.IdClient} категории {route.NameCategory}. ", 1, ex.Message);
                return null;
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Отправка письма, формирование subject с префиксом и постфиксом в кодировке base64
        /// </summary>
        /// <param name="MainSimbiot"></param>
        /// <param name="mainRoute"></param>
        /// <returns></returns>
        public async Task<int> SendEmailAsync(Simbiot MainSimbiot, RoutePrice mainRoute)
        {
            int countSended = 0; // Сумма отправленных сообщений по категории
            try
            {
                using (var client = new SmtpClient("mail.arkona36.ru"))
                {
                    var mainCredentials = new NetworkCredential(from, password);
                    
                    List <string> NameFiles = new List<string>(); // Определяем названия файлов-копии таблицы
                    NameFiles.Add(mainRoute.NameFile);

                    foreach (Client cley in MainSimbiot.FilterClients)
                    {
                        RoutePrice route = new RoutePrice
                        {
                            Category = cley.Category.ToString(), // Категория клиента
                            IdClient = cley.IdKontr.ToString(), // ID клиента
                            PathDirectory = mainRoute.PathDirectory, // Папка где лежит прайс для него
                            Email = cley.Email, // email клиента
                            NameFile = ConfExp.GetNamePrice(cley), // Желаемое имя файла xls
                            
                            Subject = "=?UTF-8?B?" + Base64Encode(ConfExp.GetSubject(cley)) + "?=", // Желаемая тема сообщения
                            TypeSend = MainSimbiot.TypeSend, // Тип выборки клиентов
                            NameCompany = cley.NameKontr, // Имя компании
                            NameCategory = mainRoute.TypeSend != TypeSendPrice.Individual ? // Имя выборки клиентов
                                cley.Category + " " + mainRoute.TypeSend.ToString() :
                                mainRoute.TypeSend.ToString() + " " + cley.IdKontr, 
                            Client = cley, // Доп данные по клиенту
                            MainClient = MainSimbiot.HandleClient // Клиент по которому формировался прайс
                        };
                        route.FullPath = ConfExp.GetFullPath(route.PathDirectory, route.NameFile); // Формируется полный путь до файла
                        try
                        {
                            if (route.FullPath == null)
                                throw new NullReferenceException("Ошибка при формировании пути до прайса");
                            if (cley.Email == null || cley.Email.Trim().Length == 0)
                                throw new NullReferenceException("Отсутствует email");

                            // Если название файла требуется другое, то копируем файл с другим названием и будем отправлять его
                            if (!NameFiles.Contains(route.NameFile))
                            {
                                NameFiles.Add(route.NameFile);
                                route.FullPath = ConfExp.RenamePath(ref route, mainRoute.FullPath);
                            }

                            var message = GetMailMessage(route);

                            var AltBody = AlternateView.CreateAlternateViewFromString(message.Body, new System.Net.Mime.ContentType("text/html"));
                            message.AlternateViews.Add(AltBody);

                            client.Credentials = mainCredentials;

                            //исключительный клиент, который не может видеть вложения без заполненного body ))
                            if (route.Client.IdKontr == 556107)
                            {
                                client.Credentials = new NetworkCredential(spareMail, sparePassword);
                                message.From = new MailAddress(spareMail, "ООО Аркона");
                            //    message.Body = "PriceList"; 
                                var contentDisposition = new ContentDispositionHeaderValue("attachment");
                                contentDisposition.FileName = $"{Path.GetFileName(route.FullPath)}";
                                message.Headers.Add("Content-Disposition", contentDisposition.ToString());
                                
                            }

                            //message.Headers["Content-Disposition", "attachment;filename="];
                            message.Headers["Content-type"] = "text/html; charset=UTF-8";
                            message.Headers["Content-Transfer-Encoding"] = "base64";

                            if (message == null)
                                throw new NullReferenceException("Сообщение не инициализировано (null)");
                            
                            await client.SendMailAsync(message);
                            message.Dispose();
                            countSended++;
                            DBExecutor.InsertLog(30, Program.GetSchedule(), route);
                        }
                        catch (Exception ex)
                        {
                            UniLogger.WriteLog($"Ошибка при отправке cообщения клиенту {cley.IdKontr} {route.NameCompany} {cley.Email} категории {route.NameCategory}", 1, ex.Message);
                            DBExecutor.InsertLog(29, Program.GetSchedule(), route, isByCategory: false, errorMes: ex.Message);
                            continue;
                        }
                    }
                }



                return countSended;
            }
            catch(Exception ex)
            {
                UniLogger.WriteLog($"Ошибка при отправке на почты категории {MainSimbiot.Category} {MainSimbiot.TypeSend.ToString()}", 1, ex.Message);
                return countSended;
            }

        }
    }
}
