using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{

    /// <summary>
    /// Determines if non ASCII characters will be written as Unicode or escaped Unicode.
    /// For example:   π = \u03c0
    /// </summary>
    internal enum UnicodeEscapeMode
    {
        /// <summary>
        /// If the file contains escaped Unicode, the modified file will encode non ASCII characters.
        /// </summary>
        AutoDetect = 1,

        /// <summary>
        /// Always encode non ASCII characters.
        /// </summary>
        AlwaysEscapeNonAscii,

        /// <summary>
        /// No characters will be encoded
        /// </summary>
        NoEncode
    }
}
