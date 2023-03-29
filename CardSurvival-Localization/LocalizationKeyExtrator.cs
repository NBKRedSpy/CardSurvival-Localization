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

        public List<LocalizationInfo> Extract(string json)
        {
            List<LocalizationInfo> localizationEntries = new();


            JObject doc = JObject.Parse(json);

            IEnumerable<JToken> match = doc.SelectTokens(@"$..[?(@.DefaultText!='')]");

            foreach (JToken token in match)
            {
                LocalizationInfo info = new()
                {
                    DefaultText = ThrowIfNull(token["DefaultText"]?.Value<string>()),
                    LocalizationKey = ThrowIfNull(token["LocalizationKey"]?.Value<string>()),
                    JsonPath = token.Path
                };

                localizationEntries.Add(info);
            };

            return localizationEntries;
        }
    }
}
