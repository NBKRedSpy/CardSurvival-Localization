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
    }

    public class LocalizationGroupCompare : IEqualityComparer<LocalizationInfo>
    {
        public bool Equals(LocalizationInfo? x, LocalizationInfo? y)
        {
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return String.Equals(x?.LocalizationKey, y?.LocalizationKey)
                && String.Equals(x?.DefaultText, y?.DefaultText, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] LocalizationInfo obj)
        {

            return (
                obj.LocalizationKey.GetHashCode(),
                obj.DefaultText.GetHashCode(StringComparison.OrdinalIgnoreCase))
                .GetHashCode();
        }
    }
}
