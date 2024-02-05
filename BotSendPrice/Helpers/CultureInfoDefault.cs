using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace CreateCommerchialRequest.Helpers
{
    public static class CultureInfoDefault
    {
        public static void CheckAndSetCultureInfo()
        {
            var UserCulture = Thread.CurrentThread.CurrentCulture;
            var UserCultureUI = Thread.CurrentThread.CurrentUICulture;

            CultureInfo newCulture = CultureInfo.GetCultureInfo("ru-RU").Clone() as CultureInfo;
            CultureInfo newCultureUI = CultureInfo.GetCultureInfo("ru-RU").Clone() as CultureInfo;

            newCulture.NumberFormat.CurrencyDecimalSeparator = ".";
            newCulture.NumberFormat.NumberGroupSeparator = " ";
            newCulture.NumberFormat.NumberDecimalSeparator = ".";
            newCulture.NumberFormat.NegativeSign = "-";
            newCulture.NumberFormat.PercentDecimalSeparator = ".";

            newCultureUI.NumberFormat.CurrencyDecimalSeparator = ".";
            newCultureUI.NumberFormat.NumberDecimalSeparator = ".";
            newCultureUI.NumberFormat.NumberGroupSeparator = " ";
            newCultureUI.NumberFormat.NegativeSign = "-";
            newCultureUI.NumberFormat.PercentDecimalSeparator = ".";
            

            CultureInfo.DefaultThreadCurrentCulture = newCulture;
            CultureInfo.DefaultThreadCurrentUICulture = newCultureUI;
        }
    }
}
