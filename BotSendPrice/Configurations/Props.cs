using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSendPrice.Configurations
{
    public static class Props
    {
        public static readonly int MaxCountThreads;

        public static readonly string BoxArkonaLogin;

        public static readonly string BoxArkonaPassword;

        public static readonly string DebugEmail;

        public static readonly string FolderPrices;

        static Props()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings.Get("maxCountThreads"), out MaxCountThreads))
            {
                MaxCountThreads = 4; // По умолчанию 4 потока
            }
            BoxArkonaLogin = ConfigurationManager.AppSettings.Get("BoxArkonaLogin").ToString();
            BoxArkonaPassword = ConfigurationManager.AppSettings.Get("BoxArkonaPassword").ToString();

            DebugEmail = ConfigurationManager.AppSettings.Get("DebugEmail").ToString();

            FolderPrices = ConfigurationManager.AppSettings.Get("FolderPrices").ToString();
        }
    }
}
