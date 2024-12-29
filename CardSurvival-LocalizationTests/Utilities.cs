using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CardSurvival_LocalizationTests
{
    internal static class Utilities
    {
        public static void SetClipboard(string text)
        {
            TextCopy.ClipboardService.SetText(text);
        }


        public static void LogActualString(ITestOutputHelper output, string currentMethod, string text)
        {

            //Utilities.LogActualString(Output, System.Reflection.MethodBase.GetCurrentMethod().Name, actual);

            output.WriteLine(currentMethod);
            output.WriteLine("\"\"\"\n" + text + "\n\"\"\";\n");

        }
    }

}
