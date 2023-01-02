using System;
using System.IO;
using System.Collections.Generic;

using NPOI.SS.UserModel;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

using GeneralComponents;

namespace GUI {
    internal class DatabaseLoader {
        private static Database? database = null;
        public static Database Database => database ??
                        throw new NullReferenceException("Database wasn't loaded!");

        public static bool IsLoaded() => database != null;

        public static void LoadDatabase(string pathToColorDatabase) {
            if (!File.Exists(pathToColorDatabase)) return;

            IWorkbook workbook;
            using (FileStream stream = File.OpenRead(pathToColorDatabase)) {
                if (pathToColorDatabase.EndsWith(".xls"))
                    workbook = new HSSFWorkbook(stream);
                else
                    workbook = new XSSFWorkbook(stream);
            }

            List<List<List<double>>> data = new(workbook.NumberOfSheets);
            for (int i = 0; i < workbook.NumberOfSheets; ++i)
                GetSheetIntoDataBase(data, workbook.GetSheetAt(i));

            workbook.Close();
            database = new(data);
        }

        private static void GetSheetIntoDataBase(List<List<List<double>>> data, ISheet sheet) {
            List<List<double>> sheetInList = new(sheet.LastRowNum);
            foreach (IRow row in sheet) {
                List<double> rowInList = new(row.LastCellNum);
                foreach (ICell cell in row)
                    rowInList.Add(cell.NumericCellValue);

                sheetInList.Add(rowInList);
            }

            data.Add(sheetInList);
        }
    }
}
