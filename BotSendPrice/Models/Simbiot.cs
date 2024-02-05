using AutoSavePrices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSendPrice.Models
{
    /// <summary>
    /// Клиент, по которому формируется прайс с коллекцией клиентов 1 типа
    /// </summary>
    public class Simbiot
    {
        /// <summary>
        /// Хранит ссылку на клиента, по которому формируется прайс
        /// </summary>
        public Client HandleClient { get; set; }

        public TypeSendPrice TypeSend { get; set; }

        /// <summary>
        /// Хранит коллекцию на всех клиентов одного типа, что и клиент needClient + самого себя
        /// </summary>
        public List<Client> FilterClients { get; set; }

        public int Category { get; set; }

        public bool isNeedKontr { get; set; } = true;
    }

    public enum TypeSendPrice
    {
        Exclusive = 8, // 8 категория
        Default = 228, // 2,3,9,20,21,22,23,31,32 категория
        DefaultNoRRC = 229, // 2,3,9,20,21,22,23,31,32 категория
        APlus = 230, // 2,3,9,20,21,22,23,31,32 категория
        FiveSix = 56, // 5,6 категория
        IPVrnMoscow = 7, // 7 категория
        IPOnlyMoscow = 9, // 7 категория
        Individual = 88 // Разные категории
    };
}
