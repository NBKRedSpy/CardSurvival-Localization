using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{
    public class LocalizationInfo
    {
        public string FileName { get; set; } = "";
        public string JsonPath { get; set; } = "";
        public string DefaultText { get; set; } = "";
        public string LocalizationKey { get; set; } = "";

        /// <summary>
        /// True if true, then a new key was generated for this entry.
        /// </summary>
        public bool KeyWasCreated { get; set; }
    }
}
