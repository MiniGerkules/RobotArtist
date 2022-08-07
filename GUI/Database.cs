using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Office.Interop.Excel;

using Range = Microsoft.Office.Interop.Excel.Range;
using Application = Microsoft.Office.Interop.Excel.Application;

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

            Application excel = new();
            Workbook excelBook = excel.Workbooks.Open(pathToColorDatabase, UpdateLinks: 0,
                ReadOnly: true, Origin: XlPlatform.xlWindows);

            data = new(excelBook.Sheets.Count);
            for (int i = 0; i < excelBook.Sheets.Count; ++i)
                GetSheetIntoDataBase(excelBook, i);

            excel.Quit();
        }

        private static void GetSheetIntoDataBase(Workbook excelBook, int curIndex)
        {
            Worksheet current = (Worksheet)excelBook.Sheets[curIndex + 1];
            Range rows = current.Rows;

            int numOfRows = GetNumberOfRows(current);
            data.Add(new(numOfRows));
            for (int j = 0; j < numOfRows; ++j)
            {
                object[,] values = ((object[,])rows[j + 1].Value2);

                data[curIndex].Add(new(6));
                for (int k = 0; k < 6; ++k)
                    data[curIndex][j].Add(Convert.ToDouble(values[1, k + 1]));
            }
        }

        private static int GetNumberOfRows(Worksheet sheet)
        {
            Range first_cell = sheet.Cells[1, 1];
            Range lastRow = sheet.Cells.get_End(XlDirection.xlDown);

            return sheet.get_Range(first_cell, lastRow).Count;
        }
    }
}
