using AutoSavePrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSendPrice.Extensions
{
    public static class CustomExtensions
    {
        /// <summary>
        /// Обновляет idTerritory в коллекции клиентов
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="newTerritory"></param>
        /// <returns></returns>
        public static void UpdateTerritory<T>(this IEnumerable<T> list, int newTerritory)
        {
            bool isClient = list is IEnumerable<Client>;
            if (isClient)
            {
                foreach(T item in list)
                {
                    var client = item as Client;
                    client.IdTerritory = newTerritory;
                }
            }
        }
    }
}
