using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{
    public static class Utilities
    {
        [return: NotNull]
        public static T ThrowIfNull<T>(T value, string name = "")
        {
            if (value == null) throw new ArgumentNullException(name);
            return value!;
        }
    }
}

