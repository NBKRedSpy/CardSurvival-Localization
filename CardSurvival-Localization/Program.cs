using System.Runtime.CompilerServices;

namespace CardSurvival_Localization
{
    internal class Program
    {
        static void Main(string[] args)
        {

            if(args.Length < 1)
            {

                Console.Error.WriteLine("Must be at least 1 argument:  SearchPath [OutputFile]");
                return;
            }
            Utilities.ThrowIfNull(args[0]);

            string[] files = Directory.GetFiles(Path.GetDirectoryName(args[0])!, Path.GetFileName(args[0])!, SearchOption.AllDirectories);

            //const string FilePath = @"C:\src\CardSurvival\CardSurvival-Localization\CardSurvival-Localization\TestData\Test.json";

            LocalizationKeyExtrator localizationKeyExtrator = new();

            List<LocalizationInfo> localizationInfos = new();

            foreach (string file in files)
            {
                List<LocalizationInfo> result = localizationKeyExtrator.Extract(File.ReadAllText(file));

                localizationInfos.ForEach(x=> x.FileName = file);   
                localizationInfos.AddRange(result);
            }

            bool isConsole = args.Length < 2;

            StreamWriter outputStream = null;
            
            if (isConsole == false)
            {
                outputStream = File.CreateText(args[1]);
            }

            using (outputStream)
            {
                foreach (var item in localizationInfos)
                {
                    string result = string.Join(',', item.LocalizationKey, "", item.DefaultText);

                    if (isConsole)
                    {
                        Console.WriteLine(result);
                    }
                    else
                    {
                        outputStream!.WriteLine(result);
                    }
                }
            }
        }
    }
}