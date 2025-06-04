// Assets/Scripts/Infrastructure/ExcelConverter.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using ClosedXML.Excel;

public static class ExcelConverter
{
    // Converts an Excel worksheet to a list of dictionaries using ClosedXML.
    // Each dictionary represents a row with keys from the header row.
    public static List<Dictionary<string, string>> ConvertExcel(string excelPath, string sheetName = null)
    {
        var result = new List<Dictionary<string, string>>();

        if (!File.Exists(excelPath))
        {
            Debug.LogError($"Excel file not found at {excelPath}");
            return result;
        }

        try
        {
            using (var workbook = new XLWorkbook(excelPath))
            {
                var worksheet = string.IsNullOrEmpty(sheetName)
                    ? workbook.Worksheet(1)
                    : workbook.Worksheet(sheetName);

                var firstRowUsed = worksheet.FirstRowUsed();
                if (firstRowUsed == null) return result;

                var headers = new List<string>();
                foreach (var cell in firstRowUsed.Cells())
                {
                    headers.Add(cell.GetString());
                }

                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var entry = new Dictionary<string, string>();
                    int colIndex = 0;
                    foreach (var cell in row.Cells(1, headers.Count))
                    {
                        if (colIndex < headers.Count)
                            entry[headers[colIndex]] = cell.GetString();
                        colIndex++;
                    }
                    result.Add(entry);
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to convert Excel: {ex.Message}");
        }

        return result;
    }
}
