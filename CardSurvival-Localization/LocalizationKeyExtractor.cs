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
using System.Diagnostics.Eventing.Reader;
using System.IO.Abstractions;
using System.Runtime.InteropServices;


namespace CardSurvival_Localization
{
    internal class LocalizationKeyExtractor
    {

        public IFileSystem FileSystem { get; set; }

        /// <summary>
        /// The list of the new key for any Localization Keys that were re-generated.
        /// This is used to make existing duplicate Localization Keys unique.
        /// Contains the new key and one or more entries that were updated based on that new key.
        /// The key is already based on the info's text.
        /// </summary>
        public Dictionary<string, List<LocalizationInfo>> RegeneratedKeys { get; private set; } = new();

        /// <summary>
        /// The localization keys that were created for cards that had text, but no key.
        /// Keyed by the card's DefaultText and contains the list of LocalizationInfo's all use that text.
        /// </summary>
        public Dictionary<string, List<LocalizationInfo>> LocalizationKeys { get; private set; } = new();

        private LocalizationKeyGenerator KeyGen { get;} = new LocalizationKeyGenerator();

        /// <summary>
        /// The lookup for generated keys.  
        ///     Key: The DefaultText that generated the key,
        ///     Value: The created entry.
        /// </summary>
        public Dictionary<string, List<LocalizationInfo>> GeneratedKeys { get; private set; } = new();

        public LocalizationKeyExtractor(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
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
                 
                if(String.IsNullOrWhiteSpace(info.LocalizationKey))
                {
                    CreateNewKeyByText(token, info);
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
        /// For any keys that are duplicate, create new keys. 
        /// Exclude any keys that are in the excludeKeys list.
        /// 
        /// </summary>
        /// <remarks>This is primarily used to deconflict any keys that are in the json data, but not in the SimpCn.csv file.
        /// Often Chinese mods will re-use the same key based on what the card was copied from.  Since there is no entry in the SimpCn.csv,
        /// The DefaultText member is used.  Therefore it works fine for the Chinese mode, but not the translated English mode.
        /// </remarks>
        /// <param name="excludeKeys">The keys to not de-dupe</param>
        public void FixDuplicateKeys(HashSet<string> excludeKeys, Action<string>? progress = null)
        {

            //TODO:  This is creating new keys even though the key is defined in the SimpCn and the Default Text should be ignored.
            //Cod_Exp_SawMill_ExplorationResults[0].Action.ActionName,,找到通往丛林外围的路,,,x,,,,,Exp_Sawmill_ExplorationResults_25,找到通往丛林外围的路
            //Cod_Exp_SawMill_ExplorationResults[0].Action.ActionName__T - 4 + rWTD0wOza4yI2Utp1En5Bzong =,,Exp_Sawmill_ExplorationResults_25,N,,,,Exp_Sawmill_ExplorationResults_25,,,,
            //Cod_Exp_SawMill_ExplorationResults[0].Action.ActionName__T - vK1ygvbYnScoRsORd + AJH5GQJLk =,,Exp_Sawmill_ExplorationResults_125,N,,,,Exp_Sawmill_ExplorationResults_125,,,,



            var duplicateKeys = LocalizationKeys
                .Where(x => x.Value.Count > 1 && !excludeKeys.Contains(x.Key))
                .ToList();

            bool useProgress = progress is not null;

            foreach (KeyValuePair<string, List<LocalizationInfo>> localizationInfos
                in duplicateKeys)
            {
                

                if (useProgress) progress!(localizationInfos.Key);


                //Remove from the localization lookup
                LocalizationKeys.Remove(localizationInfos.Key);

                foreach (LocalizationInfo info in localizationInfos.Value)
                {
                    CreateNewKeyByKey(info);
                    LocalizationKeys.Add(info.LocalizationKey, new List<LocalizationInfo> { info });
                }
            }
        }

        /// <summary>
        /// Generates a new unique key for a LocalizationInfo, updates the json file and adds it
        /// to the RegeneratedKeys list
        /// Will try to re-use previous keys with the same text and existing key.
        /// </summary>
        /// <param name="info"></param>
        private void CreateNewKeyByKey(LocalizationInfo info)
        {
            //Keep the base key to make it obvious that the key was duplicated.
            string newKey = KeyGen.Create(info.DefaultText, prefix: info.LocalizationKey + "__");

            List<LocalizationInfo> generatedInfos;
            info.OldLocalizationKey = info.LocalizationKey;
            info.LocalizationKey = newKey;

            if (RegeneratedKeys.TryGetValue(newKey, out generatedInfos!))
            {
                generatedInfos.Add(info);
                
            }
            else
            {
                RegeneratedKeys.Add(newKey, new List<LocalizationInfo>() { info });
            }

            info.ReplaceKeyInFile(FileSystem, newKey);


        }

        /// <summary>
        /// Used if CreateMissingLocalizationKeys is true and the key is empty.
        /// Will try to re-use an existing created key, otherwise will create a new one.
        /// All keys will be added to the GeneratedKeys dictionary.
        /// Updates the token to contain the new key.
        /// </summary>
        /// <param name="token">The token to create a new key for</param>
        /// <param name="info">The LocalizationInfo to update with the new key.</param>
        /// <param name="prefix">The prefix to add to the key.</param>
        private void CreateNewKeyByText(JToken token, LocalizationInfo info)
        {
            info.KeyWasCreated = true;

            List<LocalizationInfo> generatedInfos;

            string text = info.DefaultText.Trim();

            //Consolidate any keys
            if (GeneratedKeys.TryGetValue(text, out generatedInfos!))
            {
                info.LocalizationKey = generatedInfos[0].LocalizationKey;
                generatedInfos.Add(info);
            }
            else
            {
                info.LocalizationKey = KeyGen.Create(text);
                GeneratedKeys.Add(text, new() { info });
            }

            token["LocalizationKey"] = info.LocalizationKey;
        }

    }
}
