using BotSendPrice.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoSavePrices
{
    public class QAllClients
    {
        private List<Simbiot> _allClients;

        private readonly int maxClientsOneThread;
       

        public QAllClients(int maxClientsOneThread, List<Simbiot> handledClients)
        {
            this.maxClientsOneThread = maxClientsOneThread;
            _allClients = handledClients;
        }

        public int countClients => _allClients.Count;

        /// <summary>
        /// Взять след.клиента
        /// </summary>
        /// <returns></returns>
        public Simbiot Next()
        {
            try
            {
                if (_allClients.Count == 0)
                    throw new NullReferenceException("Пустой список клиентов");
                Simbiot NextClient = _allClients.First();
                _allClients.RemoveAt(0);

                return NextClient;
            }
            catch (Exception ex)
            {
                UniLogger.WriteLog("Ошибка при взятии клиента", 1, ex.Message);
                return null;
            }

        }

        /// <summary>
        /// Взять первых клиентов
        /// </summary>
        /// <returns></returns>
        public Simbiot[] GetTopClients()
        {
            if (_allClients.Count == 0)
                throw new NullReferenceException("Пустой список клиентов");
            Simbiot[] resultClients = _allClients.Take(maxClientsOneThread).ToArray();
            _allClients.RemoveRange(0, resultClients.Count());

            return resultClients;
        }

    }
}
