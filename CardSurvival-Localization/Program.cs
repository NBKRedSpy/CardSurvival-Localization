using System.Globalization;
using System.IO.Abstractions;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Moq;
using Newtonsoft.Json.Linq;


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
                sourceDirectory = args[0];

                sourceDirectory = ProcessMod(sourceDirectory, new FileSystem());

                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return -1;
            }
        }

        internal static string ProcessMod(string sourceDirectory, IFileSystem fileSystem)
        {
            if (!fileSystem.Directory.Exists(sourceDirectory))
            {
                throw new ArgumentException($"Mod Directory does not exist: {sourceDirectory}");
            }

            string modInfoFilePath = Path.Combine(sourceDirectory, "ModInfo.json");
            if (!fileSystem.File.Exists(modInfoFilePath))
            {
                throw new ArgumentException($"The ModInfo.json cannot be found in the mod directory: {modInfoFilePath}");
            }

            Console.WriteLine("Processing...");

            //---Extract info from .json files
            string[] files = fileSystem.Directory.GetFiles(sourceDirectory, "*.json", SearchOption.AllDirectories);

            LocalizationKeyExtrator localizationKeyExtrator = new();

            foreach (string file in files)
            {

                string jsonSource = fileSystem.File.ReadAllText(file);
                JObject jsonDoc = JObject.Parse(jsonSource);
                bool jsonModified = localizationKeyExtrator.Extract(jsonDoc, file);

                if (jsonModified)
                {
                    //A new localization key was set, write the changes.
                    fileSystem.File.WriteAllText(file, jsonDoc.ToString(Newtonsoft.Json.Formatting.Indented));
                }
            }

            string localizationFolder = Path.Combine(sourceDirectory, "Localization");

            if (!fileSystem.Directory.Exists(localizationFolder))
            {
                fileSystem.Directory.CreateDirectory(localizationFolder);
            }

            string errorFileName = Path.Combine(localizationFolder, "SimpEn_Errors.txt");

            string errorText = GetErrorsAndWarnings(localizationKeyExtrator);
            if (!string.IsNullOrEmpty(errorText))
            {

                fileSystem.File.WriteAllText(errorFileName, errorText);
                Console.WriteLine($"One or more translation warnings or errors occurred. See {errorFileName}");
            }

            string localizationFilePath = Path.Combine(localizationFolder, "SimpEn.psv");



            using (TextWriter outputWriter = new StreamWriter(fileSystem.FileStream.New(localizationFilePath, FileMode.Create)))
            {

                //Using Pipe format since spreadsheet programs like Google Sheets gets caught up on unicode comma like characters.
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "|"
                };

                //---- Write to English translation output
                using (CsvWriter csvWriter = new CsvWriter(outputWriter, csvConfig))
                {
                    foreach (List<LocalizationInfo> keyInfos in localizationKeyExtrator.LocalizationKeys.Values)
                    {

                        //For entries that had a new key created, there will always be one key.
                        //For duplicate existing keys, there may be one or move values
                        foreach (LocalizationInfo info in keyInfos)
                        {
                            //The game's example SimpCn.csv shows new lines to be escaped.
                            string encodedText = info.DefaultText.Replace("\n", "\\n");

                            csvWriter.WriteField(info.LocalizationKey);
                            csvWriter.WriteField("");  //Spot for English translation
                            csvWriter.WriteField(encodedText);

                            csvWriter.NextRecord();
                        }


                    }
                }
            }

            Console.WriteLine("Translation Completed.");
            return sourceDirectory;
        }

        private static string GetErrorsAndWarnings(LocalizationKeyExtrator localizationKeyExtractor)
        {

            //Duplicate text entries
            List<KeyValuePair<string, List<LocalizationInfo>>> multiDefinedInfo = localizationKeyExtractor.LocalizationKeys.Where(x => x.Value.Count > 1)
                .ToList();

            StringBuilder sb = new StringBuilder();

            if (localizationKeyExtractor.LocalizationKeys.ContainsKey("")){
                sb.AppendLine("---- Errors -----");
                sb.AppendLine("One or more keys are blank.  These entries can be created with the --create-keys option.");
            }

            if (multiDefinedInfo.Count() > 0)
            {
                sb.AppendLine("---- Errors -----");
                sb.AppendLine("Error: Multiple keys exist with different text");
                sb.AppendLine();

                foreach (KeyValuePair<string, List<LocalizationInfo>> dupeInfo in multiDefinedInfo)
                {
                    sb.AppendLine($"Key: \"{dupeInfo.Key}\"");

                    foreach (LocalizationInfo info in dupeInfo.Value)
                    {
                        sb.AppendLine($"\tText: {info.DefaultText}");
                        sb.AppendLine($"\tFile: {info.FileName}");
                        sb.AppendLine($"\tJSON Path: {info.JsonPath}");
                        sb.AppendLine();
                    }
                }

            }

            if(localizationKeyExtractor.GeneratedKeys.Count() > 0)
            {
                sb.AppendLine("---- Warnings -----");
                sb.AppendLine("New Keys Created.  JSON was updated.");
                sb.AppendLine();

                foreach (var newKeyEntry in localizationKeyExtractor.GeneratedKeys)
                {
                    var baseInfo = newKeyEntry.Value[0];

                    sb.AppendLine($"\tNew Key: \"{baseInfo.LocalizationKey}\"");
                    sb.AppendLine($"\tText: {baseInfo.DefaultText}");
                    sb.AppendLine();

                    foreach (var info in newKeyEntry.Value)
                    {
                        sb.AppendLine($"\tFile: {info.FileName}");
                        sb.AppendLine($"\tJSON Path: {info.JsonPath}");
                        sb.AppendLine();
                    }
                }
            }

            return sb.ToString();
        }

        private static bool ShowUsage(string[] args)
        {
            var firstArg = args.Length > 0 ? args[0] : "";
            firstArg = firstArg.Trim();

            if(args.Length != 1 || firstArg == "-h" || firstArg == "/?" || firstArg == "--help")
            {
                var text = @"
Usage: CardSurvival-Localization <path to mod directory>

Description:  

Creates a SimpEn.psv translation file which is used for a CSTI-ModLoader 
mod which is only in Chinese.  

Remember to translate the pipe delimited SimpEn.psv to a comma delimited SimpEn.csv or the mod will not use the file.

See https://github.com/NBKRedSpy/CardSurvival-Localization for documentation.


Parameters:
path to mod directory  The directory the mod is located
";

                Console.WriteLine(text);
                return true;
            }

            return false;
        }
    }


}