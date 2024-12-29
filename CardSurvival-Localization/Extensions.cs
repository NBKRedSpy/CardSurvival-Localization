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

    }
}
