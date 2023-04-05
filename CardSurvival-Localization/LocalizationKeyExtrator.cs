using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel.DataAnnotations;
using System.Data;
using static CardSurvival_Localization.Utilities;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace CardSurvival_Localization
{
    public class LocalizationKeyExtrator
    {
        /// <summary>
        /// The localization keys, used to keep track of existing strings of text as well as
        /// newly generated keys.
        /// </summary>
        public Dictionary<string, List<LocalizationInfo>> LocalizationKeys { get; private set; } = new();

        /// <summary>
        /// The lookup for generated keys.  
        ///     Key: The DefaultText that generated the key,
        ///     Value: The created entry.
        /// </summary>
        public Dictionary<string, List<LocalizationInfo>> GeneratedKeys { get; private set; } = new();

        private IGuidFactory GuidFactory { get; }

        public LocalizationKeyExtrator(IGuidFactory guidFactory) 
        {
            GuidFactory = guidFactory;
        }

        /// <summary>
        /// Adds the localization info into the dictionary.
        /// If a key has more than one DefaultText, the info will be added to the Key's info list.
        /// </summary>
        /// <param name="info"></param>
        private void AddLocalizationInfo(LocalizationInfo info)
        {

            List<LocalizationInfo> list;

            if (LocalizationKeys.TryGetValue(info.LocalizationKey, out list! ))
            { 
                if(!list.Any(x=> x.DefaultText == info.DefaultText))
                {
                    //Add only if the text does not already exist for this key.
                    list.Add(info);
                }
            }
            else
            {
                LocalizationKeys.Add(info.LocalizationKey, new List<LocalizationInfo>() { info});
            }
        }

        /// <summary>
        /// Extracts the localization key and text into the LocalizationKeys dictionary.
        /// If the key and exact text already exist, a new LocalizationInfo will not be created.
        /// </summary>
        /// <param name="doc">The json document from the root of the file.</param>
        /// <param name="fileName">The name of the file that the json was loaded from.
        /// </param>
        /// <returns>True if the json was modified to add a previously missing key.</returns>
        public bool Extract(JObject doc, string fileName)
        {
            IEnumerable<JToken> match = doc.SelectTokens(@"$..[?(@.DefaultText!='')]");
            bool jsonUpdated = false;

            foreach (JToken token in match)
            {
                LocalizationInfo info = new();

                info.LocalizationKey = token["LocalizationKey"]?.Value<string>() ?? string.Empty;
                info.DefaultText = ThrowIfNull(token["DefaultText"]?.Value<string>()).Trim();
                info.FileName = fileName;
                info.JsonPath = token.Path;

                if(String.IsNullOrEmpty(info.LocalizationKey))
                {
                    CreateNewKey(token, info);
                }
                else
                {
                    info.KeyWasCreated = false;
                }
                
                AddLocalizationInfo(info);

                jsonUpdated |= info.KeyWasCreated;
            };

            return jsonUpdated;
        }

        /// <summary>
        /// Used if CreateMissingLocalizationKeys is true and the key is empty.
        /// Will try to re-use an existing created key, otherwise will create a new one.
        /// All keys will be added to the GeneratedKeys dictionary.
        /// Updates the token to contain the new key.
        /// </summary>
        /// <param name="token">The token to create a new key for</param>
        /// <param name="info">The LocalizationInfo to update with the new key.</param>
        private void CreateNewKey(JToken token, LocalizationInfo info)
        {
            info.KeyWasCreated = true;

            List<LocalizationInfo> generatedInfos;

            //Consolidate any keys
            if (GeneratedKeys.TryGetValue(info.DefaultText, out generatedInfos!))
            {
                info.LocalizationKey = generatedInfos[0].LocalizationKey;
                generatedInfos.Add(info);
            }
            else
            {
                info.LocalizationKey = CreateKeyString();
                GeneratedKeys.Add(info.DefaultText, new() { info });
            }

            token["LocalizationKey"] = info.LocalizationKey;
        }

        /// <summary>
        /// Generates a new key.  Used for localization info that does not have a key.
        /// </summary>
        /// <returns></returns>
        private string CreateKeyString()
        {
            return $"translate-{GuidFactory.Create().ToString()}";
        }
    }
}
