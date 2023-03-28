using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using System.Text.Json;
//using System.Text.Json.Nodes;
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

        //public List<LocalizationInfo> Extract(string json)
        //{

        //    JsonDocument doc = JsonDocument.Parse(json);


        //    //Search for DefaultText != ""
        //    //Error if "LocalizationKey" == ""

        //    //If object or array, search those.

        //    foreach (JsonProperty item in doc.RootElement.EnumerateObject())
        //    {
        //        if(item.Value.ValueKind == JsonValueKind.Object)
        //        {
        //            //sub iterator.
        //        }
        //        else if(item.Value.ValueKind == JsonValueKind.String)
        //        {

        //        }
        //        else if (item.Value.ValueKind == JsonValueKind.Array)
        //        {

        //        }

        //    }

        //    throw new NotImplementedException();
        //}


        public List<LocalizationInfo> Extract(string json)
        {
            //JsonDocument doc = JsonDocument.Parse(json);


            List<LocalizationInfo> localizationEntries = new();


            JObject doc = JObject.Parse(json);

            //IEnumerable<JToken> match = doc.SelectTokens("$..[?(@.DefaultText.length>0)]");
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
