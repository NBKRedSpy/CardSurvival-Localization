using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;

namespace CardSurvival_Localization
{
    internal class Program
    {
        static void Main(string[] args)
        {

            if (args.Length < 1)
            {

                Console.Error.WriteLine("Must be at least 1 argument:  SearchPath [OutputFile]");
                return;
            }
            Utilities.ThrowIfNull(args[0]);

            //---Extract info from .json files
            string[] files = Directory.GetFiles(Path.GetDirectoryName(args[0])!, Path.GetFileName(args[0])!, SearchOption.AllDirectories);

            LocalizationKeyExtrator localizationKeyExtrator = new();

            List<LocalizationInfo> localizationInfos = new();

            foreach (string file in files)
            {
                List<LocalizationInfo> result = localizationKeyExtrator.Extract(File.ReadAllText(file));

                localizationInfos.ForEach(x => x.FileName = file);
                localizationInfos.AddRange(result);
            }


            //---- Remove exact duplicates

            //Remove exact key/text (case insensitive) duplicates.
            //Group by items that have more than one text for the same key.
            //  Note - Keys are case insensitive.  Currently CSTI-ModLoader is case sensitive.
            List<IGrouping<string, LocalizationInfo>> groupedInfo = localizationInfos
                .Distinct(new LocalizationGroupCompare())
                .OrderBy(x=> x.LocalizationKey)
                    .ThenBy(x=> x.DefaultText)
                .GroupBy(x => x.LocalizationKey)
                .OrderBy(x=> x.Key)
                .ToList();

            //---- Output warnings to stderr
            var multiDefinedInfo = groupedInfo.Where(x => x.Count() > 1);

            if(multiDefinedInfo.Count() > 0)
            {
                Console.Error.WriteLine("Error: Multiple keys exist with different text");

                foreach (var multiGroup in multiDefinedInfo)
                {
                    Console.Error.WriteLine($"Key: \"{multiGroup.Key}\"");

                    foreach (var info in multiGroup)
                    {
                        Console.Error.WriteLine($"\t{info.DefaultText}");
                    }
                }

                Console.Error.WriteLine("-----");
                Console.Error.WriteLine();
            }

            //---- Write to output
            bool isConsole = args.Length < 2;

            TextWriter outputWriter = isConsole ? Console.Out : new StreamWriter(args[1]);

            using (outputWriter)
            using (CsvWriter csvWriter = new CsvWriter(outputWriter, CultureInfo.InvariantCulture))
            {
                foreach (var item in groupedInfo.SelectMany(x=> x.ToList()))
                {
                    //CsvHelper.CsvWriter csvWriter = new CsvWriter()
                    string result = string.Join(',', item.LocalizationKey, "", item.DefaultText);

                    csvWriter.WriteField(item.LocalizationKey);
                    csvWriter.WriteField("");
                    csvWriter.WriteField(item.DefaultText);

                    csvWriter.NextRecord();
                }
            }
        }

    }
}