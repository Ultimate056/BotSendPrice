using AutoSavePrices.Models;
using BotSendPrice.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoSavePrices.Configurations
{
    public static class ConfExp
    {


        private static readonly string main_path = @"C:\Falcon";

        private static readonly string extFormat = ".xls";

        public static string GeneratePath(ref RoutePrice p, int Category, TypeSendPrice typeSend)
        {
            try
            {
                if (typeSend == TypeSendPrice.Individual)
                {
                    string DateNow = DateTime.Now.ToString("dd-MM-yyyy HH_mm_ss_ffff");
                    string nameInd = $"Индивидуал " + p.IdClient;
                    p.PathDirectory = Path.Combine(main_path, nameInd, DateNow);
                }
                else
                {
                    string nameCategory = $"Категория {Category}" + " - " + typeSend.ToString();
                    p.PathDirectory = Path.Combine(main_path, nameCategory);
                }
                Directory.CreateDirectory(p.PathDirectory);
                return p.PathDirectory + @"\" + p.NameFile + extFormat;
            }
            catch(Exception ex)
            {
                UniLogger.WriteLog($"Err. Формирование пути главного прайса по {Category} {typeSend.ToString()}",
                    3, ex.Message);
                return null;
            }
        }

        public static string RenamePath(ref RoutePrice p, string oldPath)
        {
            string newPath = p.PathDirectory + @"\" + p.NameFile + extFormat;
            File.Copy(oldPath, newPath);
            return newPath;
        }
        public static string GetFullPath(string path_category, string nameFile)
        {
            return path_category + @"\" + nameFile + extFormat;
        }

        public static readonly List<string> NameCols = new List<string>()
        {
            "Бренд",
            "Артикул",
            "Наименование товара",
            "Кол-во",
            "Мин.парт",
            "Цена"
        };

        public static readonly List<string> NameColsDataTable = new List<string>()
        {
            "tm_name",
            "id_tov_oem_short",
            "n_tov",
            "resttov",
            "min_part",
            "price"
        };

        public static readonly List<string> NameColsAzia = new List<string>()
        {
            "Бренд",
            "Артикул",
            "Наименование товара",
            "Шт. в кор.",
            "Кол-во",
            "Мин.парт",
            "Цена"
        };

        public static readonly List<string> NameColsDtAzia = new List<string>()
        {
            "tm_name",
            "id_tov_oem_short",
            "n_tov",
            "in_box",
            "resttov",
            "min_part",
            "price"
        };


        public static string GetNamePrice(Client client)
        {
            if (client.NamePrice == null) client.NamePrice = "";
            string res = client.NamePrice;
            int len = res.Length;
            int cat = client.Category;

            if (cat == 5 && len == 0)
                res = "Azia_sklad2_day0";
            else if (cat == 6 && len == 0)
                res = "Azia_sklad1_day1";
            else if (cat == 7 && len == 0)
                res = "PriceList_0days";
            else if (cat == 4 && len == 0)
                res = client.F_TwoSklad > 0 ? "PriceList_1days" : "PriceList";
            else if ((cat == 5 || cat == 6) &&
                      len > 0)
                res = client.NamePrice;
            else
                res = client.NamePrice.Trim() == "" ? "PriceList" : client.NamePrice;

            return res;
        }

        public static string GetSubject(Client client)
        {
            if (client.NameSubject == null) client.NameSubject = "";
            string res = client.NameSubject;
            int len = res.Length;
            int cat = client.Category;

            if (cat == 5 && len == 0)
                res = "Azia_sklad2_day0";
            else if (cat == 6 && len == 0)
                res = "Azia_sklad1_day1";
            else if (cat == 7 && len == 0)
                res = "Прайс-лист Аркона 0 день";
            else if (cat == 4 && len == 0)
                res = client.F_TwoSklad > 0 ? "Прайс-лист Аркона 1 день" : "Прайс-лист";
            else if ((cat == 5 || cat == 6) && len > 0)
                res = "ПрайсЛист";
            else
                res = client.NameSubject.Trim() == "" ? "Прайс-лист" : client.NameSubject;

            return res;
        }

    }
}
