using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{
    internal class LocalizationInfo
    {
        public string FileName { get; set; } = "";
        public string JsonPath { get; set; } = "";
        public string DefaultText { get; set; } = "";
        public string LocalizationKey { get; set; } = "";

        /// <summary>
        /// True if true, then a new key was generated for this entry.
        /// </summary>
        public bool KeyWasCreated { get; set; }


        /// <summary>
        /// Set when the LocalizationKey was regenerated.
        /// LocalizationKey will contain the new key and this will contain 
        /// the old key.
        /// </summary>
        /// <remarks>
        /// This is used by the functionality that de-duplicates keys.
        /// </remarks>
        public string OldLocalizationKey { get; set; } = string.Empty;

        /// <summary>
        /// True if the key was recreated.  
        /// </summary>
        /// 
        public bool KeyWasRegenerated => !String.IsNullOrEmpty(OldLocalizationKey);


        /// <summary>
        /// Replaces the key for the json object, updating the json file.
        /// </summary>
        /// <param name="newKey"></param>
        /// <exception cref="ArgumentException"></exception>
        public void ReplaceKeyInFile(IFileSystem fileSystem, string newKey)
        {
            if (string.IsNullOrEmpty(JsonPath))
            {
                throw new ArgumentException("The json path is not set", nameof(JsonPath));  
            }

            JObject doc = JObject.Parse(fileSystem.File.ReadAllText(FileName));
            JToken? localizationKeyToken = doc.SelectToken(JsonPath + ".LocalizationKey", true);
            ArgumentNullException.ThrowIfNull(localizationKeyToken);

            ((JValue)localizationKeyToken).Value = newKey;
            string newJson = JsonConvert.SerializeObject(doc, Formatting.Indented);
            File.WriteAllText(FileName, newJson);
        }

        public override string ToString()
        {
            return $"'{LocalizationKey}' '{DefaultText}'";
        }
    }
}
