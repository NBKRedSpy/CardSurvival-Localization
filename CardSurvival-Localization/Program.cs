using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using CsvHelper;
using CsvHelper.Configuration;


namespace CardSurvival_Localization
{
    internal class Program
    {
        static int Main(string[] args)
        {

            try
            {
                if (ShowUsage(args))
                {
                    return -2;
                }

                string sourceDirectory;

                if (!Directory.Exists(args[0]))
                {
                    throw new ArgumentException($"Mod Directory does not exist: {args[0]}");
                }


                Console.WriteLine("Processing...");

                sourceDirectory = args[0];

                //---Extract info from .json files
                string[] files = Directory.GetFiles(sourceDirectory, "*.json", SearchOption.AllDirectories);

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

                //---Write Output Errors to file.

                StringWriter sw = new StringWriter();
                GetErrorText(sw, groupedInfo);

                string errors = sw.ToString();

                const string errorFileName = "SimpEn_Errors.txt";

                if (errors.Length >0)
                {
                    File.WriteAllText(errorFileName ,errors);
                    Console.WriteLine($"One or more translation errors occurred. See {errorFileName}");
                }

                const string localizationFilePath = "SimpEn.csv";

                using (var outputWriter = new StreamWriter(localizationFilePath))
                {
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

                Console.WriteLine("Translation Completed.");

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1;
            }
        }

        private static bool ShowUsage(string[] args)
        {
            if(args.Length != 1 || args[0].Trim() == "-h" || args[0].Trim() == "/?" || args[0].Trim() == "--help")
            {
                Console.WriteLine("Usage: CardSurvival-Localization <path to mod directory>");
                Console.WriteLine();
                return true;
            }

            return false;
        }
        private static void GetErrorText(TextWriter output,  List<IGrouping<string, LocalizationInfo>> groupedInfo)
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