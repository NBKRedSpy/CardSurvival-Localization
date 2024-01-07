using Cocona.Command;
using Cocona.Help;
using Cocona.Help.DocumentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardSurvival_Localization
{
    internal class DescriptionTransformHelpAttribute : TransformHelpAttribute
    {
        public override void TransformHelp(HelpMessage helpMessage, CommandDescriptor command)
        {
            var descSection = (HelpSection)helpMessage.Children.First(x => x is HelpSection section && section.Id == HelpSectionId.Description);
            descSection.Children.Add(new HelpPreformattedText(@"
Description:  
    Creates a SimpEn.psv translation file which is used for a CSTI-ModLoader 
    mod which is only in Chinese.  

    Remember to translate the pipe delimited SimpEn.psv to a comma delimited SimpEn.csv or the mod will not use the file.

    See https://github.com/NBKRedSpy/CardSurvival-Localization for documentation.
"));

        //    helpMessage.Children.Add(new HelpSection(
        //        new HelpHeading("Example:"),
        //        new HelpSection(
        //            new HelpParagraph("MyApp --foo --bar")
        //        )
        //    ));
        }
    }
}
