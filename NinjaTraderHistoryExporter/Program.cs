using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NinjaTraderHistoryExporter
{
    class Program
    {
        private static void Main(string[] args)
        {
            var program = new Program();
            program.Run(args);
        }

        private void Run(IEnumerable<string> args)
        {
            var keyValuePairs = args.Select(a => a.Split('=')).ToDictionary(pair => pair[0].Trim().TrimStart('/', '-').ToLower(), pair => pair.Length > 1 ? pair[1].Trim() : null);

            var inDirectory = GetValueByKey(keyValuePairs, "in") ??
                              Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) +
                              @"\NinjaTrader 7\db\minute\";

            var outDirectory = GetValueByKey(keyValuePairs, "out") ?? @"C:\temp\NTHistoryExport\";
            var separator = GetValueByKey(keyValuePairs, "sep") ?? ";";

            var reader = new NtMinuteFileReader();
            var writeHeader = !OptionSpecified(keyValuePairs, "noheader");

            Directory.CreateDirectory(outDirectory);

            foreach (var symbolDir in Directory.EnumerateDirectories(inDirectory))
            {
                var barParts = Directory.EnumerateFiles(symbolDir, "*.ntd").SelectMany(f => reader.Read(Path.GetFileName(f), new FileStream(f,
                                            FileMode.Open, FileAccess.Read, FileShare.ReadWrite))).OrderBy(r => r.DateTime).Select(record => new[]
                                                                        {
                                                                            record.DateTime.ToString("yyyyMMdd HHmmss"),
                                                                            GetNumberString(record.Open),
                                                                            GetNumberString(record.High),
                                                                            GetNumberString(record.Low),
                                                                            GetNumberString(record.Close),
                                                                            record.Volume.ToString()
                                                                        }).ToArray();

                if (barParts.Any())
                {
                    var writer = File.CreateText(Path.Combine(outDirectory, symbolDir.Substring(symbolDir.LastIndexOf("\\") + 1) + ".txt"));

                    if (writeHeader)
                    {
                        writer.WriteLine(string.Join(separator, new[] { "DateTime", "Open", "High", "Low", "Close", "Volume" }));
                    }

                    foreach (var bar in barParts)
                    {
                        writer.WriteLine(string.Join(separator, bar));
                    }

                    writer.Flush();
                    writer.Close();
                }
            }
        }

        private string GetNumberString(decimal num)
        {
            return num.ToString("0.######");
        }

        private string GetValueByKey(IDictionary<string, string> keyValuePairs, string option)
        {
            string val;
            return keyValuePairs.TryGetValue(option.ToLower(), out val) ? val : null;
        }

        private bool OptionSpecified(IDictionary<string, string> keyValuePairs, string option)
        {
            return keyValuePairs.Keys.Contains(option.ToLower());
        }
    }
}
