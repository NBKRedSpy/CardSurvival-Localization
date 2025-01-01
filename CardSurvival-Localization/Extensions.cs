using CsvHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{
    internal static class Extensions
    {
        /// <summary>
        /// Writes the list of values as fields.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="values"></param>
        public static void WriteFields(this CsvWriter writer, params string[] values)
        {
            foreach (var value in values)
            {
                writer.WriteField(value);
            }
        }

        public static bool TryAddNew<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key,
                   Func<TKey, TValue> createNew, out TValue value) where TKey : notnull
        {
            if (dictionary.TryGetValue(key, out TValue? lookupValue))
            {
                value = lookupValue;
                return false;
            }
            else
            {
                value = createNew(key);
                dictionary.Add(key, value);
                return true;
            }
        }

    }
}
