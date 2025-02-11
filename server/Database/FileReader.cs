using System;
using System.IO;
using Microsoft.VisualBasic;

class FileReader
{
  public static Dictionary<string, string> Load(string filePath)
  {
    var list = new Dictionary<string, string>();


    if (!File.Exists(filePath)) throw new FileNotFoundException($"{filePath} not found!");

    foreach (var line in File.ReadAllLines(filePath))
    {
      if (string.IsNullOrWhiteSpace(line)) continue;

      var parts = line.Split('=', 2);
      if (parts.Length != 2) continue;

      var key = parts[0].Trim();
      var value = parts[1].Trim();
      list.Add(key, value);
    }

    return list;
  }
}