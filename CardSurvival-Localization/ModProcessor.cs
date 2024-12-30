using CsvHelper.Configuration;
using CsvHelper;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Reflection;
using Microsoft.Extensions.FileProviders;

namespace CardSurvival_Localization
{
    public class ModProcessor
    {
        private HashSet<string> GamesLocalizationKeys { get; set; }

        public ModProcessor()
        {

            using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CardSurvival_Localization.GameDefaultKeys.txt"))
            using (StreamReader reader = new StreamReader(resourceStream))
            {
                string data = reader.ReadToEnd();
                GamesLocalizationKeys = new HashSet<string>(data.Split("\r\n"));
            }

        }
        public string ProcessMod(string sourceDirectory, IFileSystem fileSystem, UnicodeEscapeMode escapeMode)
        {
            Regex unicodeReplaceRegEx = new Regex(@"(\\u)([a-f0-9]{4})", RegexOptions.Compiled);

            if (!fileSystem.Directory.Exists(sourceDirectory))
            {
                throw new ArgumentException($"Mod Directory does not exist: {sourceDirectory}");
            }

            string modInfoFilePath = Path.Combine(sourceDirectory, "ModInfo.json");
            if (!fileSystem.File.Exists(modInfoFilePath))
            {
                throw new ArgumentException($"The ModInfo.json cannot be found in the mod directory: {modInfoFilePath}");
            }

            if (!Console.IsOutputRedirected) Console.CursorVisible = false;

            Console.WriteLine("Processing...");

            //---Extract info from .json files
            string[] files = fileSystem.Directory.GetFiles(sourceDirectory, "*.json", SearchOption.AllDirectories)
                .Where(x => String.Equals(Path.GetFileName(x), "ModInfo.json", StringComparison.OrdinalIgnoreCase) == false)
                .ToArray();

            LocalizationKeyExtrator localizationKeyExtrator = new();

            foreach (string file in files)
            {
                //debug
                //if (i++ >= 100) break;

                Console.Write($"\r{Path.GetFileName(file)}                                  \r");

                string jsonSource = fileSystem.File.ReadAllText(file);

                JObject jsonDoc = JObject.Parse(jsonSource);

                bool jsonModified = localizationKeyExtrator.Extract(jsonDoc, file);


                //Write out the json file.  Handles adding the newly created keys to the json objects
                //  as well as handling any Unicode escaping consistencies.
                //For AlwaysEscapeNonAscii and NoEncode, have to always write since there could
                //  be mixed escape/not escape.  Newtonsoft doesn't have a way to get a property's original value.
                if (
                    jsonModified && escapeMode == UnicodeEscapeMode.AutoDetect
                    || escapeMode == UnicodeEscapeMode.AlwaysEscapeNonAscii
                    || escapeMode == UnicodeEscapeMode.NoEncode)
                {
                    WriteGameFileChange(fileSystem, escapeMode, unicodeReplaceRegEx, file, jsonSource, jsonDoc);
                }
            }

            if (!Console.IsOutputRedirected) Console.CursorVisible = true;      //Required since unit tests don't have a console, but do have a stream.

            string localizationFolder = Path.Combine(sourceDirectory, "Localization");

            if (!fileSystem.Directory.Exists(localizationFolder))
            {
                fileSystem.Directory.CreateDirectory(localizationFolder);
            }

            localizationFolder = Path.Combine(sourceDirectory, "localization");

            List<CsLocalizationEntry> englishLocalization;
            List<CsLocalizationEntry> chineseLocalization;

            try
            {
                englishLocalization = ParseSimpFile(fileSystem, Path.Combine(localizationFolder, "SimpEn.csv"));
            }
            catch (Exception ex)
            {
                englishLocalization = new();
                Console.WriteLine($"Error loading SimpEn.csv: {ex}");
            }

            try
            {
                chineseLocalization = ParseSimpFile(fileSystem, Path.Combine(localizationFolder, "SimpCn.csv"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading SimpCn.csv: {ex}");
                chineseLocalization = new();
            }

            string errorFileName = Path.Combine(localizationFolder, "SimpEn_Errors.txt");
            string errorText = GetErrorsAndWarnings(localizationKeyExtrator, out int keysWithDifferentTextCount);

            Console.WriteLine();

            if (!string.IsNullOrEmpty(errorText))
            {
                fileSystem.File.WriteAllText(errorFileName, errorText);

                if (keysWithDifferentTextCount > 0)
                {
                    Console.WriteLine($"Important: There are {keysWithDifferentTextCount} keys that have more than one text mapping.");
                    Console.WriteLine($"See {errorFileName}");
                }
            }

            WriteCombinedPsv(fileSystem, localizationKeyExtrator, localizationFolder, englishLocalization, chineseLocalization);

            Console.Write("                                        \r");
            Console.WriteLine("Translation Completed.");
            return sourceDirectory;
        }

        /// <summary>
        /// Writes out the TranslationData.psv, combining the localization sources.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="localizationKeyExtrator"></param>
        /// <param name="localizationFolder"></param>
        /// <param name="englishLocalization"></param>
        /// <param name="chineseLocalization"></param>
        private void WriteCombinedPsv(IFileSystem fileSystem, LocalizationKeyExtrator localizationKeyExtrator, string localizationFolder, List<CsLocalizationEntry> englishLocalization, List<CsLocalizationEntry> chineseLocalization)
        {
            List<CombinedLocalizationInfo> combinedLocalization = GetCombinedLocalization(localizationKeyExtrator,
                            englishLocalization, chineseLocalization);

            string localizationFilePath = Path.Combine(localizationFolder, "TranslationData.psv");

            //Get the full join data for each key.

            //TODO:  Simplify this.  Try changing to an object with multiple SelectMany with a downstream projection.
            var flattenedInfo = combinedLocalization
                .SelectMany(x => x.Json.DefaultIfEmpty(), (item, json) => new
                {
                    item,
                    item.Key,
                    json = json?.DefaultText ?? ""
                })
                .SelectMany(x => x.item.EnglishData.DefaultIfEmpty(new CsLocalizationEntry()), (item, english) => new
                {
                    item.item,
                    item.Key,
                    item.json,
                    en_english = english.English,
                    en_chinese = english.Chinese
                })
                .SelectMany(x => x.item.ChineseData.DefaultIfEmpty(new CsLocalizationEntry()), (item, chinese) => new
                {
                    item.item,
                    isDuplicate = item.item.EnglishData.Count > 1 || item.item.ChineseData.Count > 1 || item.item.Json.Count > 1,
                    item.Key,
                    item.json,
                    item.en_english,
                    item.en_chinese,
                    IsCardKey = localizationKeyExtrator.LocalizationKeys.ContainsKey(item.Key),    //True if the key is from a card (newly generated or not)
                    IsGameKey = GamesLocalizationKeys.Contains(item.Key),   //True if it is a default game key.
                    cn_english = chinese.English,
                    cn_chinese = chinese.Chinese
                })
                .OrderBy(x => x.Key)
                .ToList();

            using (TextWriter outputWriter = new StreamWriter(fileSystem.FileStream.New(localizationFilePath, FileMode.Create)))
            {
                //Using Pipe format since spreadsheet programs like Google Sheets gets caught up on unicode comma like characters.
                var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    Delimiter = "|",
                };

                //---- Write to English translation output
                using (CsvWriter csvWriter = new CsvWriter(outputWriter, csvConfig))
                {
                    //----Header
                    csvWriter.WriteFields(
                        "Key",
                        "English",
                        "Chinese",
                        "IsDuplicate",
                        "IsNotCardKey",
                        "IsGameKey",
                        "CardDefault",
                        "SimpEn-English",
                        "SimpEn-Chinese",
                        "SimpCn-English",
                        "SimpCn-Chinese");

                    csvWriter.NextRecord();

                    foreach (var flattened in flattenedInfo)
                    {
                        csvWriter.WriteFields(
                            flattened.Key,
                            "",
                            "",
                            flattened.isDuplicate ? "x" : "",
                            flattened.IsCardKey ? "" : "x",
                            flattened.IsGameKey ? "x" : "",
                            flattened.json.Replace("\n", "\\n"), //Escape the new lines.
                            flattened.en_english,
                            flattened.en_chinese,
                            flattened.cn_english,
                            flattened.cn_chinese
                        );

                        csvWriter.NextRecord();
                    }
                }
            }
        }


        /// <summary>
        /// Combines the Json data, existing SimpEn.csv, and SimpCn.csv data,
        /// mapping all to the translation key.  
        /// Duplicates will have a cartesian join.
        /// </summary>
        /// <param name="localizationKeyExtrator"></param>
        /// <param name="englishLocalization"></param>
        /// <param name="chineseLocalization"></param>
        /// <returns></returns>The combined data.
        private List<CombinedLocalizationInfo> GetCombinedLocalization(LocalizationKeyExtrator localizationKeyExtractor,
            List<CsLocalizationEntry> englishLocalization, List<CsLocalizationEntry> chineseLocalization)
        {
            Dictionary<string, CombinedLocalizationInfo> dataLookup = new Dictionary<string, CombinedLocalizationInfo>();


            //Json data
            dataLookup = localizationKeyExtractor.LocalizationKeys.Select(x => new CombinedLocalizationInfo()
            {
                Key = x.Key,
                Json = new List<LocalizationInfo>(x.Value),
            })
            .ToDictionary(x => x.Key);


            //SimpEn.csv (English data)


            AddSimpData(true, englishLocalization, dataLookup);
            AddSimpData(false, chineseLocalization, dataLookup);

            return dataLookup.Values.ToList();
        }

        /// <summary>
        /// Adds the Simp* data to an existing CombinedLocalizationInfo dictionary.
        /// Based on the Simp* entry's key, will create a new row or add to a row with the same key.
        /// </summary>
        /// <param name="simpData">All of the lines in a Simp*.csv file.  Keys can be duplicated.</param>
        /// <param name="dataLookup">The dictionary to add the data to.</param>
        private void AddSimpData(bool isEnglish, List<CsLocalizationEntry> simpRecords, Dictionary<string, CombinedLocalizationInfo> dataLookup)
        {
            var keyGrouping = simpRecords
                .GroupBy(x => x.Key);

            foreach (var group in keyGrouping)
            {
                CombinedLocalizationInfo info;

                if (!dataLookup.TryGetValue(group.Key, out info))
                {
                    info = new CombinedLocalizationInfo() { Key = group.Key };
                    dataLookup[group.Key] = info;
                }

                if (isEnglish)
                {
                    info.EnglishData = group.ToList();
                }
                else
                {
                    info.ChineseData = group.ToList();
                }

            }
        }

        private List<CsLocalizationEntry> ParseSimpFile(IFileSystem fileSystem, string simpSourceFile)
        {
            if (!fileSystem.File.Exists(simpSourceFile)) return new List<CsLocalizationEntry>();

            using StreamReader reader = fileSystem.File.OpenText(simpSourceFile);
            using CsvReader csvReader = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,

            });


            List<CsLocalizationEntry> records = csvReader.GetRecords<CsLocalizationEntry>()
                .ToList();

            //Trim the text and remove any entries that only have a key.
            records = records
                .Select(x => new CsLocalizationEntry(x.Key, x.English.Trim(), x.Chinese.Trim()))
                .Where(x => x.English != "" && x.Chinese != "")
                .ToList();

            return records;
        }

        /// <summary>
        /// Writes the json file.  Called to update data such as key info or to make the Unicode encoding method consistent.
        /// </summary>
        /// <param name="fileSystem"></param>
        /// <param name="escapeMode"></param>
        /// <param name="unicodeReplaceRegEx"></param>
        /// <param name="file"></param>
        /// <param name="jsonSource"></param>
        /// <param name="jsonDoc"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void WriteGameFileChange(IFileSystem fileSystem, UnicodeEscapeMode escapeMode, Regex unicodeReplaceRegEx, string file, string jsonSource, JObject jsonDoc)
        {
            using (MemoryStream resultWriterStream = new())
            using (StreamWriter streamWriter = new StreamWriter(resultWriterStream))
            using (JsonWriter writer = new JsonTextWriter(streamWriter))
            {
                writer.Formatting = Formatting.Indented;
                writer.AutoCompleteOnClose = true;

                bool isUnicodeLowerCased = true; //Default to lower case to match the ModEditor.

                switch (escapeMode)
                {
                    case UnicodeEscapeMode.AutoDetect:
                        writer.StringEscapeHandling = IsUnicodeEscaped(jsonSource, out isUnicodeLowerCased) ?
                            StringEscapeHandling.EscapeNonAscii : StringEscapeHandling.Default;
                        break;
                    case UnicodeEscapeMode.AlwaysEscapeNonAscii:
                        writer.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
                        isUnicodeLowerCased = true; //ModEditor uses lower.
                        break;
                    case UnicodeEscapeMode.NoEncode:
                        writer.StringEscapeHandling = StringEscapeHandling.Default;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(escapeMode), $"Unexpected value: {escapeMode}");
                }

                jsonDoc.WriteTo(writer);
                writer.Flush();

                byte[] jsonResultArray = resultWriterStream.ToArray();

                if (isUnicodeLowerCased)
                {
                    fileSystem.File.WriteAllBytes(file, jsonResultArray);
                }
                else
                {
                    //Dev Note:  This is a bit inefficient, but is fine for the performance target of this utility.
                    //  Unfortunately was not able to intercept JsonWriter since it encodes Unicode after the
                    //  converters are executed.  The encoding methods are also private.
                    //
                    //  A custom stream would require intercepting the bytes as they stream in, so not worth the 
                    //  work since this accomplishes the need.

                    string result = Encoding.UTF8.GetString(jsonResultArray);

                    result = unicodeReplaceRegEx.Replace(result, (Match match) =>
                        match.Groups[1].Value + match.Groups[2].Value.ToUpper());

                    fileSystem.File.WriteAllText(file, result);

                }
            }
        }

        /// <summary>
        /// Returns if there is escaped unicode in the text file.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private bool IsUnicodeEscaped(string json, out bool isLowerCased)
        {

            MatchCollection matches = Regex.Matches(json, @"(\\u)([a-f0-9]{4})", RegexOptions.IgnoreCase);

            bool isEscaped = matches.Count > 0;

            isLowerCased = true;

            if (isEscaped)
            {
                //If all numbers, assume lower.  If one item is lower, assume lower.
                isLowerCased = matches.Any(x =>
                {
                    string hexCode = x.Groups[2].Value;
                    //Does not count.
                    if (int.TryParse(hexCode, out _)) return false;

                    return hexCode.ToLower() == hexCode;
                });
            }

            return isEscaped;
        }

        private string GetErrorsAndWarnings(LocalizationKeyExtrator localizationKeyExtractor, out int keysWithDifferentTextCount)
        {

            //Duplicate text entries
            List<KeyValuePair<string, List<LocalizationInfo>>> multiDefinedInfo = localizationKeyExtractor.LocalizationKeys.Where(x => x.Value.Count > 1)
                .ToList();

            keysWithDifferentTextCount = multiDefinedInfo.Count;

            StringBuilder sb = new StringBuilder();

            if (multiDefinedInfo.Count() > 0)
            {
                sb.AppendLine("---- Errors -----");
                sb.AppendLine($"Error: Multiple keys exist with different text.");
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

            if (localizationKeyExtractor.GeneratedKeys.Count() > 0)
            {
                sb.AppendLine("---- Informational ----");
                sb.AppendLine($"New Keys Created.  JSON was updated.");
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
    }
}
