using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{
    /// <summary>
    /// An entry from a card survival localization file.  For example, a line in the SimpEn.csv file.
    /// </summary>
    internal class CsLocalizationEntry
    {
        public string Key{ get; set; }
        public string English { get; set; }
        public string Chinese { get; set; }

    }
}
