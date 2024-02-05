using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DataBaseArkona
{
    class CommonProperty
    {
        public static string DBServer { get; set; }
        public static string DBBase { get; set; }

        static CommonProperty()
        {
            LoadDataAppConfig();
        }

        public static void LoadDataAppConfig()
        {
            DBServer = ConfigurationManager.AppSettings.Get("DBServer");
            DBBase = ConfigurationManager.AppSettings.Get("DBBase");
        }

    }
}
