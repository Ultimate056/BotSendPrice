using AutoSavePrices.Configurations;
using AutoSavePrices.Models;
using DevExpress.Export.Xl;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace AutoSavePrices
{
    public class ExportToExcel
    {
        private readonly DataTable dataTable;

        public ExportToExcel(DataTable dt)
        {
            dataTable = dt;
        }

        public bool StartExport(RoutePrice path_price)
        {
            try
            {
                // Create an exporter instance.
                //IXlExporter exporter = XlExport.CreateExporter(XlDocumentFormat.Xlsx);
                IXlExporter exporter = XlExport.CreateExporter(XlDocumentFormat.Xls);

                var path = path_price.FullPath;

                // Проверка есть ли файл с таким названием

                using (FileStream stream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
                {

                    // Create a new document and begin to write it to the specified stream.
                    using (IXlDocument document = exporter.CreateDocument(stream))
                    {
                        // Add a new worksheet to the document.
                        using (IXlSheet sheet = document.CreateSheet())
                        {
                            // Specify the worksheet name.
                            sheet.Name = "Прайс-лист Аркона";

                            // Create the first column and set its width.
                            using (IXlColumn column = sheet.CreateColumn())
                            {
                                column.WidthInPixels = 100;
                            }

                            // Create the second column and set its width.
                            using (IXlColumn column = sheet.CreateColumn())
                            {
                                column.WidthInPixels = 150;
                            }

                            // Create the third column and set the specific number format for its cells.
                            using (IXlColumn column = sheet.CreateColumn())
                            {
                                column.WidthInPixels = 600;
                            }

                            bool isAsia = path_price.Client.Category == 5 || path_price.Client.Category == 6;

                            // Если по категории 5 или 6 , то добавляем доп. столбец (Шт в коробке)
                            if(isAsia)
                            {
                                using (IXlColumn column = sheet.CreateColumn())
                                {
                                    column.WidthInPixels = 65;
                                }
                            }

                            using (IXlColumn column = sheet.CreateColumn())
                            {
                                column.WidthInPixels = 65;
                            }
                            using (IXlColumn column = sheet.CreateColumn())
                            {
                                column.WidthInPixels = 65;
                            }
                            using (IXlColumn column = sheet.CreateColumn())
                            {
                                column.WidthInPixels = 100;
                            }

                            // Specify cell font attributes.
                            XlCellFormatting cellFormatting = new XlCellFormatting();
                            cellFormatting.Font = new XlFont();
                            cellFormatting.Font.Name = "Arial";
                            cellFormatting.Font.Size = 10;
                            cellFormatting.Font.SchemeStyle = XlFontSchemeStyles.None;

                            // Specify formatting settings for the header row.
                            XlCellFormatting headerRowFormatting = new XlCellFormatting();
                            headerRowFormatting.CopyFrom(cellFormatting);
                            headerRowFormatting.Font.Bold = true;
                            headerRowFormatting.Font.Color = XlColor.DefaultForeground; //XlColor.FromTheme(XlThemeColor.Light1, 0.0);
                            headerRowFormatting.Fill = XlFill.SolidFill(XlColor.FromTheme(XlThemeColor.Accent4, 0.0));

                            // Create the header row.
                            using (IXlRow row = sheet.CreateRow())
                            {
                                XlCellFormatting cellHeaderFormatting = new XlCellFormatting();
                                cellFormatting.Font = new XlFont();
                                cellFormatting.Font.Name = "Arial";
                                cellFormatting.Font.Size = 10;
                                cellFormatting.Font.SchemeStyle = XlFontSchemeStyles.None;

                                row.HeightInPixels = 40;
                                row.ApplyFormatting(cellHeaderFormatting);
                                row.Formatting.Alignment = new XlCellAlignment();
                                row.Formatting.Alignment.VerticalAlignment = XlVerticalAlignment.Center;
                                row.Formatting.Alignment.HorizontalAlignment = XlHorizontalAlignment.Center;

                                var ListCols = isAsia ?
                                    ConfExp.NameColsAzia : ConfExp.NameCols;
                                foreach(var nameColumn in ListCols)
                                {
                                    using (IXlCell cell = row.CreateCell())
                                    {
                                        cell.Value = nameColumn;
                                        cell.ApplyFormatting(headerRowFormatting);
                                    }
                                }
                            }

                            var ListColsValue = isAsia ?
                                    ConfExp.NameColsDtAzia : ConfExp.NameColsDataTable;
                            foreach (DataRow dtrow in dataTable.Rows)
                            {
                                using (IXlRow row = sheet.CreateRow())
                                {
                                    foreach(var valueColumn in ListColsValue)
                                    {
                                        using (IXlCell cell = row.CreateCell())
                                        {
                                            cell.Value = dtrow[valueColumn].ToString();
                                            cell.ApplyFormatting(cellFormatting);
                                        }
                                    }
                                }
                            }

                            // Enable AutoFilter for the created cell range.
                            sheet.AutoFilterRange = sheet.DataRange;
                        }
                    }
                }
                GC.Collect();
                return true;
            }
            catch (Exception ex)
            {
                UniLogger.WriteLog($"Экспорт DataTable в xls. Категория {path_price.NameCategory}", 1, ex.Message);
                return false;
            }



        }
    } 
}


