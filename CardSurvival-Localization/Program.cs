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

                result.ForEach(x => x.FileName = file);
                localizationInfos.AddRange(result);
            }


            //---- Remove exact duplicates

            //Remove exact key/text (case insensitive) duplicates.
            //Group by items that have more than one text for the same key.
            //  Note - Keys are case insensitive.  Currently CSTI-ModLoader is case sensitive.
            List<IGrouping<string, LocalizationInfo>> groupedInfo = localizationInfos
                .Distinct(new LocalizationGroupCompare())
                .OrderBy(x => x.LocalizationKey)
                    .ThenBy(x => x.DefaultText)
                .GroupBy(x => x.LocalizationKey)
                .OrderBy(x => x.Key)
                .ToList();

            bool isConsole = args.Length < 2;

            TextWriter outputWriter = isConsole ? Console.Out : new StreamWriter(args[1]);
            using (outputWriter)
            {

                //---- Output warnings to stderr
                WriteErrors(outputWriter, groupedInfo);

                //---- Write to output
                using (CsvWriter csvWriter = new CsvWriter(outputWriter, CultureInfo.InvariantCulture))
                {
                    foreach (var item in groupedInfo.SelectMany(x => x.ToList()))
                    {
                        //The game's example SimpCn.csv shows new lines to be escaped.
                        string encodedText = item.DefaultText.Replace("\n", "\\n");

                        csvWriter.WriteField(item.LocalizationKey);
                        csvWriter.WriteField("");
                        csvWriter.WriteField(encodedText);

                        csvWriter.NextRecord();
                    }
                }
            }
        }

        private static void WriteErrors(TextWriter output,  List<IGrouping<string, LocalizationInfo>> groupedInfo)
        {
            var multiDefinedInfo = groupedInfo.Where(x => x.Count() > 1);

            if (multiDefinedInfo.Count() > 0)
            {

                output.WriteLine("Error: Multiple keys exist with different text");

                foreach (var multiGroup in multiDefinedInfo)
                {
                    output.WriteLine($"Key: \"{multiGroup.Key}\"");

                    foreach (var info in multiGroup)
                    {
                        output.WriteLine($"\tText: {info.DefaultText}");
                        output.WriteLine($"\tFile: {info.FileName}");
                        output.WriteLine($"\tJSON Path: {info.JsonPath }");
                        output.WriteLine();

                    }
                }

                output.WriteLine("-----");
            }
        }
    }
}