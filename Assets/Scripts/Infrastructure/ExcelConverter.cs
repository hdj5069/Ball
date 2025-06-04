// Assets/Scripts/Infrastructure/ExcelConverter.cs
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ExcelConverter
{
    // Very simple CSV to list converter. In a real project you would use a
    // package like ExcelDataReader but that requires external dependencies.
    public static List<Dictionary<string, string>> ConvertCsv(string csvPath)
    {
        var result = new List<Dictionary<string, string>>();
        if (!File.Exists(csvPath))
        {
            Debug.LogError($"CSV file not found at {csvPath}");
            return result;
        }

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length == 0) return result;

        var headers = lines[0].Split(',');
        for (int i = 1; i < lines.Length; i++)
        {
            var row = lines[i].Split(',');
            var entry = new Dictionary<string, string>();
            for (int j = 0; j < headers.Length && j < row.Length; j++)
            {
                entry[headers[j]] = row[j];
            }
            result.Add(entry);
        }
        return result;
    }
}
