[size=4][b]CardSurvival-Localization[/b][/size]

A command line utility which helps create an English translation file for CSTI-ModLoader mods that are in Chinese.  This file is used by the game to show the text as English.

[size=4][b]Summary[/b][/size]

[size=3][b]Preface[/b][/size]

This is not the CSTI-ModLoader.  This utility creates an English translation file (SimpEn.csv) for ModLoader mods that are currently in Chinese.

*Important*:  This only handles ModLoader cards (.json files).  Some ModLoader mods will also include a .dll which may also create or modify cards.  Those cards will need to be translated in the dll's source code.

See the [Observations From Current Mods](#observations-from-current-mods) section.


[size=3][b]Video[/b][/size]
A simple tutorial video can be found here:  https://youtu.be/svdUEJXaGnY

[size=3][b]What Does This Utility Do?[/b][/size]

Helps create a SimpEn.csv file which the game can use to display English text.

Functionality:
* Extracts all DefaultText and LocalizationKeys in all of the .json files (the translation key/value pairs).
* Generates unique localization keys if text is set, but no key was defined.
	* If any keys were generated, the .json files will be updated with those keys.
* Warns if an existing localization key is used multiple times, but has different text.

This document is focused on Chinese to English translation, but can also be used for English to Chinese.

[size=4][b]Logical Flow of ModLoader's Translation Load[/b][/size]

The graphic below depicts the logical flow of how ModLoader decides which text to show in the game.

![Alt text](media/Card%20Survival%20Translation%20Flow.png)


[size=2][b]Existing Translation File Note[/b][/size]

If the mod is in Chinese and already has a ./Localization/SimpCn* file (For example, SimpCn.csv) and the file is not empty, then this utility may not be needed. 

In this case:
* Copy the SimpCn* file (usually SimpCn.csv) to SimpEn.csv
* Translate the Chinese text in the third column and put the result into the second column.
* Run the game and check if any cards are missing translations.
	* The "Debug Mode" mod can spawn cards to help testing.

[size=4][b]Usage Summary[/b][/size]

To translate a mod based on CSTI-ModLoader to English, the process is as follows:

* In a command prompt, run this tool using the target mod's folder as the first parameter.
	* Example:  [code]CardSurvival-Localization.exe "E:\Mods\Apocalypse-43-1-392-1680010396\DisasterBeacons"[/code]
	* The app will create a SimpEn.psv file in mod's Localization folder.  If there are errors, there will also be a file named SimpEn_Errors.txt.
* Translate the text in the third column of the SimpEnv.psv and place the result in the second column.
	* translate.google.com and DeepL.com are great sites to handle text translation.
* Fix any errors listed in the SimpEn_Errors.txt file.
* Convert the pipe delimited text to a comma delimited file ./Localization/SimpEn.csv.
	* Google Sheets version: Download as .csv with the file name SimpEn.csv
[font=Courier New] 
| * Manual version:  Any lines that have a   | ' characters to ',' characters. |
| double quote or a comma, wrap the text in  |                                 |
| double quotes and change the double quotes |                                 |
| to two sets of double quotes.  Then change |                                 |
| all '                                      |                                 |
[/font]
[font=Courier New] 
| * Example:  [code]SomeKey | Foo "bars", fizz | Chinese Translation[/code]    |
|                           |                  | should be changed to [        |
|                           |                  | code]SomeKey,"Foo ""bars"",   |
|                           |                  | fizz",Chinese Translation[    |
|                           |                  | /code]                        |
[/font]
* Delete the SimpEn.psv and SimpEn_Errors.txt files.

Note that this program will often need to change the mod's json files, so the entire mod folder will need to be distributed.

[size=3][b]Example Output[/b][/size]

[size=2][b]Output From Tool[/b][/size]

SimpEn_Errors.txt
[code]
Error: Multiple keys exist with different text
Key: "Bp_ConservatoriesNc_CardDescription"
	Text: ???????????????
	File: .\example.json
	JSON Path: CardDescription

	Text: ?????????
	File: .\example.json
	JSON Path: CardDescription2
[/code]

SimpEn.psv
[code]
[font=Courier New] 
.
[/font]
[/code]

[size=3][b]Finished SimpEn.csv:[/b][/size]

[code]
Bp_ConservatoriesNc_CardDescription,Provides a warm home for plant growth.,???????????????
Bp_ConservatoriesNc_Two_CardName,Greenhouse No. 2,????
[/code]

[size=4][b]Spreadsheet Recommended Workflow[/b][/size]

It is recommend to use a spreadsheet application.  
[font=Courier New] 
| Note that since the initial output is pipe delimited,    | as the character. |
| the user will need to select a "custom" delimiter and    |                   |
| use                                                      |                   |
[/font]
When done, save the sheet as a CSV format with the name of SimpEn.csv.

[size=3][b]Benefits[/b][/size]

* Translation tools often have a character limit, not word limit.  With a spreadsheet, it is easy to copy only the Chinese text column to the translator.  It is also easier to translate a single item in case of an issue.
* Spreadsheets often have a function to translate a cell directly in the spreadsheet. It also makes it easier to see accidental English to English translations.  Often translation tools will retranslate English text differently if included in the translation submission.  Sometimes translators will combine multiple rows into a single row.
* Some spreadsheets already have a translation function built in.  
	* Google Sheets has a function called GoogleTranslate() can be used to translate text.  Example:  For example, Chinese to English is `=GOOGLETRANSLATE(C1,"zh","en")`.
	* A DeepL translation function for Google Sheets can be found at https://github.com/DeepLcom/google-sheets-example/
* Text that have quotes or commas will be automatically escaped
	* Example that has both a comma and a quote:  After 15 minutes, will create a "Foo".  Will be encoded as: "After 15 minutes, will create a ""Foo"""

[size=3][b]Observations From Current Mods[/b][/size]

* There might be duplicate translations for the same key.  This is most likely an oversight.  Translate the text and pick the best one, removing the others.  They can be left in the file, but the ModLoader will pick one and ignore the rest.  This issue will show up in the errors section of the output.  

* There might be entries that are from the base game, which can be removed.  These will often show up as having an English translation already.  They can be left in as the loader will ignore them, but it makes the translation file cleaner if they are removed.

* Some mods will have no LocalizationKey at all and only Default Text.  This tool will generate a localization key and change the mod's .json files to use that new key.

* Some mods include DLL's which may create or modify cards.  In this case, the code has to be changed to handle the translation.  If the mod has a DLL and uses ModLoader, the LocalizationKey can be set in the Code and then the entry added to the mod's ./Localization/SimpEn.csv.  Additionally, the C# class at https://github.com/NBKRedSpy/CardSurvival-LocalizationStringUtility can be used to create keys programmatically with a minimum amount of code changes.


[size=4][b]Command Line Parameters[/b][/size]

[font=Courier New] 
| Arguments             | Description                                          |
| --------------------- | ---------------------------------------------------- |
| Path to Mod Directory | The full path to the mod that will be translated.    |
|                       | This must be the root of the mod and will contain a  |
|                       | ModInfo.json                                         |
[/font]

See the --help command for all parameters and options.


[size=3][b]Unicode Escape Mode Option[/b][/size]

The -e option determines how the program will translate Unicode characters.

[code]
Unicode Examples:

Encoded:  \u4e00\u9635\u98d3\u98ce
Not Encoded: ????
[/code]

By default the program will use AutoDetect and retain the same Unicode encoding as the file.  However, if the text encoding is inconsistent, the AutoDetect will encode all non ASCII characters.

This option makes it easier to compare the original .json files to the modified fileas as the ModEditor encodes Unicode by default.

The SimpEn.psv file's text will always be unencoded.

[size=3][b]Mod's Root Folder[/b][/size]

Note that the root of the mod may be several directories deep.  Look for the ModInfo.json file

[size=4][b]Source and Releases for this Utility[/b][/size]

https://github.com/NBKRedSpy/CardSurvival-Localization

[size=3][b]CSTI-ModLoader is at[/b][/size]

https://www.nexusmods.com/cardsurvivaltropicalisland/mods/23

https://github.com/dop-lm/CSTI-ModLoader  (Currently the NoReflection branch has the latest code.  The master branch is out of date.  This doc will be updated when the repo is back in sync.)



[size=4][b]Change Log[/b][/size]

[size=3][b]3.3.0[/b][/size]

* Added uppercase text for escaped Unicode.  By default the ModEditor will uppercase the values.  This is optional via the 'u' parameter.
	* This simplifies comparing the original files to the modified files.
* Changed to .NET 8.

[size=3][b]3.2.0[/b][/size]

* Added option to set how Unicode text is escaped. Default attempts to keep the same encoding.
* Changed command line processing to use the Cocona library.

[size=3][b]3.1.0[/b][/size]

* Changed key generation to use T-(text's SHA1 Hash).
	* This is the same as the https://github.com/NBKRedSpy/CardSurvival-LocalizationStringUtility for cross mod key compatibility.
	* Strings are trimmed before creating the hash.
[font=Courier New] 
| * Changed output to use pipes ( | ) instead of commas to avoid needing to    |
|                                 | find all Unicode characters that get       |
|                                 | interpreted as commas in Google Sheets.    |
[/font]
	* The user should now convert the pipes to commas before inserting into SimpEn.csv


[size=3][b]3.0.1[/b][/size]

Added code to replace unicode characters that Google Sheets is interpreting as commas to commas

[size=3][b]3.0.0[/b][/size]

* Can generate LocalizationKey's for objects which have DefaultText, but no key.
* SimpEn.csv and SimpEn_Errors.txt files are now placed in the Mod's Localization folder.
* The target mod's path must point to the root of the mod, which contains a ModInfo.json.
* Due to possible key generation, this tool may now modify .json files and therefore must be deployed as a whole.
	All files which had a key generated will have an entry in the SimpEn_Errors.txt file.
* Internal - Significant code changes to support generating keys.
* Internal - Added unit tests.

[size=3][b]2.0.0[/b][/size]

[size=2][b]Upgrade Notes[/b][/size]

For users that have used a previous version of the app, the arguments have changed.  The application now only requires the path to the target mod and will automatically create a SimpEn.csv and SimpEn_Errors.txt in the current working directory. 

[size=2][b]Changes[/b][/size]

* Changed arguments to only require path.
* Changed to output errors to SimpEn_Errors.txt instead of in the translated file.
* Removed second parameter.  Now always exports to SimpEn.csv in the current working folder.

[size=3][b]1.2.0[/b][/size]

* Moved errors to output instead of stderr.
* Added File Name and JSON path to errors.
* Moved some code to a function.
* Escapes new lines as \n in the text.
* Fixed packaging not using Release build.

[size=3][b]1.1.0[/b][/size]

* Added CSV encoding.
* Remove duplicates based on key/text.
* Warns if a key has multiple entries with different versions.

[size=3][b]1.0.0[/b][/size]

* Release


