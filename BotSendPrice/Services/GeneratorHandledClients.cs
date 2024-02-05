using AutoSavePrices;
using BotSendPrice.Extensions;
using BotSendPrice.Helpers;
using BotSendPrice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotSendPrice.Services
{
    public static class GeneratorHandledClients
    {
        private readonly static int[] PacketCategories = new int[] { 2, 3, 4, 9, 20, 21, 22, 23, 31, 32 };

        private readonly static Dictionary<string, int> DicTerritory = new Dictionary<string, int>
        {
            { "VRN", 1},
            { "MSK", 4 }
        };

        public static List<Simbiot> GetHandledSimbiots(ref List<Client> allClients)
        {
            List<Simbiot> HandleClients = new List<Simbiot>();


            // 0. Убираем из всего списка московских клиентов, если рассылка выключена
            int fSendMsk = 1; // По умолчанию включена отправка московских прайсов
            try
            {
                fSendMsk = DBExecutor.FlagSendMSK;
            }
            catch (Exception ex)
            {
                Program.WriteLogConsole(ConsoleColor.Red, "Флаг мск прайсов. Параметр 50 464", ex.Message, 3);
            }

            if(fSendMsk == 0)
            {
                List<Client> temp = allClients.Where(x => x.IdTerritory == DicTerritory["MSK"]).ToList();
                allClients.Except(temp);
            }


            // =============================================
            // 1. Экслюзивный тип клиентов
            // =============================================

            // Устанавливаем текущий тип прайса
            TypeSendPrice curType = TypeSendPrice.Exclusive;

            // Выбираем клиентов, удовлетворяющих условию
            List<Client> ObservableClients = allClients.Where(x => x.Category == 8).ToList();

            // Добавляем 1 клиента из отфильтрованнй коллекции и  охраняем эту коллекцию
            HandleClients.AddRange(GetOneSimbiot(ObservableClients, curType));

            // Исключаем из коллекции всех клиентов полученную отфильтрованную коллекцию
            allClients =  allClients.Except(ObservableClients).ToList();

            // =============================================
            // 2. обычный прайс лист без скидок,  не с двумя складами по списку категорий
            // =============================================
            curType = TypeSendPrice.Default;
            ObservableClients = allClients.Where(x =>  x.F_NotNeedRRC == 0 &&
                                                        x.F_NotNeedRRCByBrand == 0 &&
                                                        x.F_SpecialPrice == 0 &&
                                                        x.F_ArkonaBonus == 0 &&
                                                        x.F_NotSeeBP == 0 &&
                                                        x.Discount == 0 &&
                                                        x.F_TwoSklad == 0 
                                                        //TODO: Castrol ? 
                                                        ).ToList();
            HandleClients.AddRange(GetSimbiotsByCategory(ref allClients, ObservableClients, curType));

            // =============================================
            // 3. Не имаги, без скидок + не нужно РРЦ
            // =============================================
            curType = TypeSendPrice.DefaultNoRRC;
            ObservableClients = allClients.Where(x => 
                                                          x.F_NotNeedRRC > 0 &&
                                                          x.F_NotNeedRRCByBrand == 0 &&
                                                          x.F_SpecialPrice == 0 &&
                                                          x.F_ArkonaBonus == 0 &&
                                                          x.F_NotSeeBP == 0 &&
                                                          x.Discount == 0 &&
                                                          x.F_TwoSklad == 0                                                    
                                                          //TODO: Castrol ? 
                                                          ).ToList();

            HandleClients.AddRange(GetSimbiotsByCategory(ref allClients, ObservableClients, curType));


            // =============================================
            // 4. Участники программы Аркона Бонус +
            // =============================================
            curType = TypeSendPrice.APlus;
            ObservableClients = allClients.Where(x => x.F_NotNeedRRC == 0 &&
                                                          x.F_NotNeedRRCByBrand == 0 &&
                                                          x.F_SpecialPrice == 0 &&
                                                          x.F_ArkonaBonus > 0 &&
                                                          x.F_NotSeeBP == 0 &&
                                                          x.Discount == 0 &&
                                                          x.F_TwoSklad == 0
                                                          //TODO: Castrol ? 
                                                          ).ToList();

            HandleClients.AddRange(GetSimbiotsByCategory(ref allClients, ObservableClients, curType));


            // =============================================
            // 5. Клиенты с категорией  6 (рассылка от Азия оил  врн склад)
            // =============================================
            curType = TypeSendPrice.FiveSix;
            ObservableClients = allClients.Where(x => x.Category == 6 &&
                                                        x.F_NotSeeBP == 0 &&
                                                        x.F_SpecialPrice == 0 &&
                                                        x.F_ArkonaBonus == 0 &&
                                                        x.F_NotNeedRRC == 0 &&
                                                        x.F_NotNeedRRCByBrand == 0
                                                        && x.F_TwoSklad == 0 &&
                                                        x.Discount == 0)
                                                        .ToList();
            ObservableClients.UpdateTerritory(DicTerritory["VRN"]); // Принудительно всем ставится idTerritory (ВРН)
            HandleClients.AddRange(GetOneSimbiot(ObservableClients, curType));
            allClients = allClients.Except(ObservableClients).ToList();



            // =============================================
            // 5. Клиенты с категорией 5 (рассылка от Азия оил мск склад)
            // =============================================
            ObservableClients = allClients.Where(x => x.Category == 5 &&
                                            x.F_NotSeeBP == 0 &&
                                            x.F_SpecialPrice == 0 &&
                                            x.F_ArkonaBonus == 0 &&
                                            x.F_NotNeedRRC == 0 &&
                                            x.F_NotNeedRRCByBrand == 0
                                            && x.F_TwoSklad == 0 &&
                                            x.Discount == 0)
                                            .ToList();
            // Если рассылка московских прайс-листов разрешена
            if (fSendMsk == 1)
            {
                curType = TypeSendPrice.FiveSix;
                ObservableClients.UpdateTerritory(DicTerritory["MSK"]);
                HandleClients.AddRange(GetOneSimbiot(ObservableClients, curType, true));
            }
            allClients = allClients.Except(ObservableClients).ToList();

            // =============================================
            // 6. Клиенты работают с Московским складом и воронежским, без бонусов
            // =============================================
            curType = TypeSendPrice.IPVrnMoscow;
            ObservableClients = allClients.Where(x => (x.Category == 4 || x.Category == 7) &&
                                        x.F_TwoSklad > 0 && x.F_SpecialPrice == 0 
                                        && x.Discount == 0 && x.F_NotSeeBP == 0
                                        && x.F_NotNeedRRC == 0 && x.F_NotNeedRRCByBrand == 0
                                        && x.F_ArkonaBonus == 0).ToList();
            ObservableClients.Where(x => x.Category == 7).UpdateTerritory(DicTerritory["MSK"]);
            ObservableClients.Where(x => x.Category == 4).UpdateTerritory(DicTerritory["VRN"]);
            if (fSendMsk == 1)
            {
                HandleClients.AddRange(GetSimbiotsTwoSklad(ObservableClients, curType)); // 2 клиента: 1 мск 1 врн
            }
            else
            {
                HandleClients.AddRange(GetOneSimbiot(ObservableClients, curType, DicTerritory["VRN"])); // 1 клиент, категория 4
            }
            allClients = allClients.Except(ObservableClients).ToList();

            // =============================================
            // 7. Клиенты работают только с Московским складом, без бонусов
            // =============================================
            curType = TypeSendPrice.IPOnlyMoscow;
            ObservableClients = allClients.Where(x => x.Category == 7 &&
                            x.F_TwoSklad == 0 &&
                            x.F_NotSeeBP == 0 &&
                            x.F_SpecialPrice == 0 &&
                            x.F_ArkonaBonus == 0 &&
                            x.F_NotNeedRRC == 0 &&
                            x.F_NotNeedRRCByBrand == 0 &&
                            x.Discount == 0).ToList();
            if (fSendMsk == 1)
            {
                ObservableClients.UpdateTerritory(DicTerritory["MSK"]);
                HandleClients.AddRange(GetOneSimbiot(ObservableClients, curType, true));
            }
            allClients = allClients.Except(ObservableClients).ToList();



            // =============================================
            // 8. Индивидуальные , со скидками и т.д.
            // Здесь каждому по 1 прайсу должно генерится
            // =============================================

            allClients.Where(x => x.IdTerritory == 0).UpdateTerritory(DicTerritory["VRN"]);
            ObservableClients = allClients;
            curType = TypeSendPrice.Individual;

            // Группируем по уникальным столбцам
            HandleClients.AddRange(ObservableClients.Distinct(new ClientComparer()).Select(x => new Simbiot
            {
                HandleClient = x,
                FilterClients = ObservableClients.FindAll(y => y.IdKontr == x.IdKontr 
                                                            && y.IdTerritory == x.IdTerritory 
                                                            && y.Category == x.Category),
                TypeSend = curType
            }));
            allClients = allClients.Except(ObservableClients).ToList();

            return HandleClients;
        }


        /// <summary>
        /// Взять пачку клиентов по кот.формируется прайс по списку категорий
        /// </summary>
        /// <param name="allClients"></param>
        /// <param name="parentClients"></param>
        /// <param name="tSendPrice"></param>
        /// <returns></returns>
        static List<Simbiot> GetSimbiotsByCategory(ref List<Client> allClients, List<Client> parentClients, TypeSendPrice tSendPrice)
        {
            List<Simbiot> res = new List<Simbiot>();
            for (int i = 0; i < PacketCategories.Length; i++)
            {
                // Суб коллекции по разным категориям 2, 3, 9, 20, 21, 22, 23, 31, 32
                List<Client> clientsByCategory = parentClients.Where(x => x.Category == PacketCategories[i]).ToList();
                res.AddRange(GetOneSimbiot(clientsByCategory, tSendPrice));
                allClients = allClients.Except(clientsByCategory).ToList();
            }
            return res;
        }


        /// <summary>
        /// Взять одного клиента, по кот.формируется прайс
        /// </summary>
        /// <param name="parentClients"></param>
        /// <param name="tSendPrice"></param>
        /// <param name="isMsk">Работает с московской территорией</param>
        /// <returns></returns>
        static IEnumerable<Simbiot> GetOneSimbiot(List<Client> parentClients, TypeSendPrice tSendPrice, bool isMsk = false)
        {
            List<Simbiot> res = new List<Simbiot>();

            // Берем первого попавшеся с территорией не 0
            res.AddRange(parentClients
                .Where(x=> x.IdTerritory != 0).Take(1)
                .Select(x => new Simbiot
                {
                    HandleClient = x,
                    FilterClients = new List<Client>(parentClients),
                    TypeSend = tSendPrice,
                    Category = x.Category
                }));

            // Если не нашел, то берем хоть кого-то)
            if(parentClients.Count > 0 && res.Count == 0)
            {
                // Но сначала принудительно меняем территорию, т.к. территория мб 0
                if (isMsk)
                    parentClients.UpdateTerritory(DicTerritory["MSK"]);
                else
                    parentClients.UpdateTerritory(DicTerritory["VRN"]);

                return GetOneSimbiot(parentClients, tSendPrice, isMsk);
            }


            foreach(Simbiot sim in res)
            {
                if(sim.TypeSend == TypeSendPrice.Default)
                {
                    sim.isNeedKontr = false;
                }
            }

            return res;
        }


        /// <summary>
        /// Взять клиента по кот. формируется прайс по категории
        /// </summary>
        /// <param name="parentClients"></param>
        /// <param name="tSendPrice"></param>
        /// <param name="Category"></param>
        /// <returns></returns>
        static IEnumerable<Simbiot> GetOneSimbiot(List<Client> parentClients, TypeSendPrice tSendPrice, int Category)
        {
            List<Simbiot> res = new List<Simbiot>();
            res.AddRange(parentClients
                .Where(x=> x.Category == Category)
                .Take(1)
                .Select(x => new Simbiot
                {
                    HandleClient = x,
                    FilterClients = new List<Client>(parentClients),
                    TypeSend = tSendPrice,
                    Category = x.Category
                }));
            return res;
        }


        /// <summary>
        /// Взять двух клиентов по кот. будет формировываться прайс. 1 с врн другой с мск
        /// </summary>
        /// <param name="parentClients"></param>
        /// <param name="tSendPrice"></param>
        /// <returns></returns>
        static IEnumerable<Simbiot> GetSimbiotsTwoSklad(List<Client> parentClients, TypeSendPrice tSendPrice)
        {
            List<Simbiot> res = new List<Simbiot>();

            res.AddRange(parentClients
                .Where(x=> x.IdTerritory == DicTerritory["VRN"])
                .Take(1)
                .Select(x => new Simbiot
                {
                    HandleClient = x,
                    FilterClients = new List<Client>(parentClients.Where(z=> z.IdTerritory == DicTerritory["VRN"]).ToList()),
                    TypeSend = tSendPrice,
                    Category = x.Category
                }));
            res.AddRange(parentClients
                .Where(x => x.IdTerritory == DicTerritory["MSK"])
                .Take(1)
                .Select(x => new Simbiot
                {
                    HandleClient = x,
                    FilterClients = new List<Client>(parentClients.Where(z => z.IdTerritory == DicTerritory["MSK"]).ToList()),
                    TypeSend = tSendPrice,
                    Category = x.Category
                }));
            return res;
        }

    }
}
