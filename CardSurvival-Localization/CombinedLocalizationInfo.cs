using Cocona.ShellCompletion.Candidate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{
    /// <summary>
    /// The consolidated localization text for a key.
    /// Contains the localization key and all realted texts from the .json files, SimpEn.csv and SimpCn.csv.
    /// While there should only be one text entry per source, it is common for more than one to be present.
    /// </summary>
    internal class CombinedLocalizationInfo
    {
        /// <summary>
        /// The key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The text from the json file
        /// </summary>
        public List<LocalizationInfo> Json { get; set; } = new();

        /// <summary>
        /// Data from the SimpEn.csv file.
        /// </summary>
        public List<CsLocalizationEntry> English { get; set; } = new();

        /// <summary>
        /// Data from the SimpCn.csv file.
        /// </summary>
        public List<CsLocalizationEntry> Chinese { get; set; } = new();
    }
}
