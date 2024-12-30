using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.IO.Abstractions;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
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

                ModProcessor processor = new();

                processor.ProcessMod(sourceDirectory, new FileSystem(), unicodeEscapeMode.Value);

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


      
    }
}

