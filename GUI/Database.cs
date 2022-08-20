using System;
using System.IO;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;

namespace GUI
{
    internal class Database
    {
        private static List<List<List<double>>> data = null;
        public static List<List<List<double>>> Data => data;

        public static bool IsLoad()
        {
            return data != null;
        }

        public static void LoadDatabase(String pathToColorDatabase)
        {
            if (!File.Exists(pathToColorDatabase))
            {
                data = null;
                return;
            }

            IWorkbook workbook;
            using (FileStream stream = File.OpenRead(pathToColorDatabase))
            {
                if (pathToColorDatabase.EndsWith(".xls"))
                    workbook = new HSSFWorkbook(stream);
                else
                    workbook = new XSSFWorkbook(stream);
            }
            
            data = new(workbook.NumberOfSheets);
            for (int i = 0; i < workbook.NumberOfSheets; ++i)
                GetSheetIntoDataBase(workbook.GetSheetAt(i));

            workbook.Close();
        }

        private static void GetSheetIntoDataBase(ISheet sheet)
        {
            List<List<double>> sheetInList = new(sheet.LastRowNum);
            foreach (IRow row in sheet)
            {
                List<double> rowInList = new(row.LastCellNum);
                foreach (ICell cell in row)
                    rowInList.Add(cell.NumericCellValue);

                sheetInList.Add(rowInList);
            }

            data.Add(sheetInList);
        }
    }
}
