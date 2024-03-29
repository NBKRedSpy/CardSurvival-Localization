﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.RegularExpressions;
using Cocona;
using Cocona.Help;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;


namespace CardSurvival_Localization
{
    internal class Program
    {
        [DescriptionTransformHelp]
        public void Run([Argument(Description = "The directory of the mod to translate.  Must contain the ModInfo.json file in the folder")]
                        string sourceDirectory,

                        [Option('e', Description = "How to escape unicode characters.  Defaults to retaining the file's format.  Ex:  For the letter A, escaped is \u0041, unescaped is A")]
                        [EnumDataType(typeof(UnicodeEscapeMode))]
                        UnicodeEscapeMode? unicodeEscapeMode)
        {
            try
            {
                unicodeEscapeMode ??= UnicodeEscapeMode.AutoDetect;
                ProcessMod(sourceDirectory, new FileSystem(), unicodeEscapeMode.Value);

                ReturnCode = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ReturnCode = 1;
            }

            //Console.WriteLine("To show help message, use '--help' option.");
        }

        //Hack until I can determine how to return result codes with Cocona.
        static int ReturnCode = 0;

        static int Main(string[] args)
        {
            //Not sure how to return an error code in the Cocona context.
            CoconaApp.Run<Program>(args);

            return ReturnCode;

        }


        public static string ProcessMod(string sourceDirectory, IFileSystem fileSystem, UnicodeEscapeMode escapeMode)
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

            Console.CursorVisible = false;
            Console.WriteLine("Processing...");

            //---Extract info from .json files
            string[] files = fileSystem.Directory.GetFiles(sourceDirectory, "*.json", SearchOption.AllDirectories)
                .Where(x => String.Equals(Path.GetFileName(x),"ModInfo.json",StringComparison.OrdinalIgnoreCase) == false)
                .ToArray();

            LocalizationKeyExtrator localizationKeyExtrator = new();

            foreach (string file in files)
            {
                Console.Write($"\r{Path.GetFileName(file)}                                  \r");

                string jsonSource = fileSystem.File.ReadAllText(file);

                JObject jsonDoc = JObject.Parse(jsonSource);

                bool jsonModified = localizationKeyExtrator.Extract(jsonDoc, file);


                //For AlwaysEscapeNonAscii and NoEncode, have to always write since there could
                //  be mixed escape/not escape.  Newtonsoft doesn't have a way to get a property's original value.
                if (
                    jsonModified && escapeMode == UnicodeEscapeMode.AutoDetect
                    || escapeMode == UnicodeEscapeMode.AlwaysEscapeNonAscii
                    || escapeMode == UnicodeEscapeMode.NoEncode)
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
            }

            Console.CursorVisible = true;

            string localizationFolder = Path.Combine(sourceDirectory, "Localization");

            if (!fileSystem.Directory.Exists(localizationFolder))
            {
                fileSystem.Directory.CreateDirectory(localizationFolder);
            }

            string errorFileName = Path.Combine(localizationFolder, "SimpEn_Errors.txt");

            string errorText = GetErrorsAndWarnings(localizationKeyExtrator, out int keysWithDifferentTextCount);

            Console.WriteLine();

            if (!string.IsNullOrEmpty(errorText))
            {
                fileSystem.File.WriteAllText(errorFileName, errorText);

                if (keysWithDifferentTextCount > 0)
                {
                    Console.WriteLine($"Important: There are {keysWithDifferentTextCount} keys that have more than text mapping.");
                    Console.WriteLine($"See {errorFileName}");
                }
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

            Console.Write("                                        \r");
            Console.WriteLine("Translation Completed.");
            return sourceDirectory;
        }

        /// <summary>
        /// Returns if there is escaped unicode in the text file.
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private static bool IsUnicodeEscaped(string json, out bool isLowerCased)
        {

            MatchCollection matches =  Regex.Matches(json, @"(\\u)([a-f0-9]{4})", RegexOptions.IgnoreCase);

            bool isEscaped = matches.Count > 0;

            isLowerCased = true;

            if(isEscaped)
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

        private static string GetErrorsAndWarnings(LocalizationKeyExtrator localizationKeyExtractor, out int keysWithDifferentTextCount)
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

