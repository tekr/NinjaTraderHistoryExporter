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

            if (OptionSpecified(keyValuePairs, "help"))
            {
                Console.WriteLine(
@"Usage:
NinjaTraderDataExporter [-in=<dir>] [-out=<dir>] [-sep=separator] [-noheader]

 in:<input dir>     - Directory to read files from. If not specified, will
                      default to '<My Documents>\NinjaTrader 7\db\minute'
 out:<output dir>   - Directory to write output to. Defaults to
                      'C:\Temp\NTHistoryExport' if not specified
 sep:<separator>    - Separator for fields. Defaults to ';' (NinjaTrader
                      export format) if not specified
 noheader           - Exclude header row from output files
 ?/help             - Print usage instructions
");
                return;
            }

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
                                                                            record.DateTime.ToUniversalTime().ToString("yyyyMMdd HHmmss"),
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
