using BotSendPrice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSavePrices.Models
{
    public class RoutePrice : ICloneable
    {
        public string Category { get; set; }

        public string IdClient { get; set; }

        /// <summary>
        /// Полный путь до файла прайсов
        /// </summary>
        public string FullPath { get; set; }

        /// <summary>
        /// Путь до папки, в которой лежат прайсы
        /// </summary>
        public string PathDirectory { get; set; }

        /// <summary>
        /// Имя файла для отправки на email
        /// </summary>
        public string NameFile { get; set; }

        /// <summary>
        /// Заголовок сообщения email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Email кому отправить
        /// </summary>
        public string Email { get; set; }

        public TypeSendPrice TypeSend {get;set;}

        public string NameCompany { get; set; }

        public string NameCategory { get; set; }

        public Client Client { get; set; } 

        public Client MainClient { get; set; }

        public object Clone()
        {
            return new RoutePrice()
            {
                Category = Category.Clone().ToString(),
                IdClient = IdClient.Clone().ToString(),
                FullPath = FullPath.Clone().ToString(),
                PathDirectory = PathDirectory.Clone().ToString(),
                NameFile = NameFile.Clone().ToString(),
                Subject = Subject.Clone().ToString(),
                Email = Email.Clone().ToString(),
                TypeSend = TypeSend,
                NameCompany = NameCompany.Clone().ToString(),
                NameCategory = NameCategory.Clone().ToString(),
                Client = Client
            };
        }
    }
}
