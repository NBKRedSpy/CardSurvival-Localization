# CardSurvival-Localization

A command line utility to create English translations for mods that depend on the CSTI-ModLoader (AKA ModLoader).  Example mods are "Tea Lover" and "Greenhouse".

# Change Notice
See the [Change Log](#changes) below as versions 2.0.0+ and 3.0.0+ have significant changes.

# Summary - What Does This Do?

This is a command line utility for CSTI-ModLoader type mods to create a translation file.  
Important:  This utility does not handle cards that are created or modified in a .DLL. See the [Observations From Current Mods](#observations-from-current-mods) section.

The utility searches all of the mod's .json files and finds any DefaultText that is set.  If the entry has a LocalizationKey, the key and text will be written to SimpEn.psv.  
If it does not contain a LocalizationKey, a new key will be created and written to the .json file.  The new key and the DefaultText will be added to the SimpEn.psv.  

The resulting SimpEn.psv can be translated by putting the English text in the second column, and then saved as a CSV formatted file named SimpEn.csv.  If there is a SimpEn_Errors.txt, correct any issues listed in that file.   Then copy the SimpEn.csv to the ./Localization folder.  The game will now show the English text.

This utility does not handle cards that are created or modified in a .dll.  Those types of mods require a code change.  A helper class for .DLL localization can be found here at https://github.com/NBKRedSpy/CardSurvival-LocalizationStringUtility .

This document is focused on Chinese to English translation, but can also be used for English to Chinese.

### Existing Translation File Note
If the mod is in Chinese and already has a ./Localization/SimpCn* file (For example, SimpCn.csv) and the file is not empty, then this mod is most likely not needed. 
Simply copy the SimpCn* file to SimpEn* and put the translation in the second column. 

# Usage Summary
To translate a mod based on CSTI-ModLoader to English, the process is as follows:

* In a command prompt, run the tool, passing the target mod's folder as the first parameter.
	* Example:  ```CardSurvival-Localization.exe "E:\Mods\Apocalypse-43-1-392-1680010396\DisasterBeacons"```
	* The app will create a SimpEn.psv file in mod's Localization folder.  If there are errors, there will also be a file named SimpEn_Errors.txt.
* Translate the text in the third column of the SimpEnv.psv and place the result in the second column.
	* translate.google.com and DeepL.com are great sites to handle text translation.
* Fix any errors listed in the SimpEn_Errors.txt file.
* Convert the pipe delimited text to a comma delimited file ./Localization/SimpEn.csv.
	* Google Sheets: Download as .csv as SimpEn.csv
	* Manually:  Any lines that have a double quote or a comma, wrap the text in double quotes and change the double quotes to two sets of double quotes.  Then change all '|' characters to ',' characters.  
		* Example:  ```SomeKey|Foo "bars", fizz|Chinese Translation``` should be changed to ```SomeKey,"Foo ""bars"", fizz",Chinese Translation```
* Delete the SimpEn.psv and SimpEn_Errors.txt files.

Note that this program will often need to change the mod's json files, so the entire mod folder will need to be distributed.

## Example Output

### Output From Tool
SimpEn_Errors.txt
```
Error: Multiple keys exist with different text
Key: "Bp_ConservatoriesNc_CardDescription"
	Text: 为植物生长提供一个温馨的家园。
	File: .\example.json
	JSON Path: CardDescription

	Text: 保护植物并加速生长
	File: .\example.json
	JSON Path: CardDescription2
```

SimpEn.psv
```
Bp_ConservatoriesNc_CardDescription||为植物生长提供一个温馨的家园。
Bp_ConservatoriesNc_CardDescription||保护植物并加速生长
Bp_ConservatoriesNc_Two_CardName||二号温室
```

## Finished SimpEn.csv:
```
Bp_ConservatoriesNc_CardDescription,Provides a warm home for plant growth.,为植物生长提供一个温馨的家园。
Bp_ConservatoriesNc_Two_CardName,Greenhouse No. 2,二号温室
```


# Spreadsheet Recommended Workflow
It is recommend to use a spreadsheet application.  
Note that since the initial output is pipe delimited, the user will need to select a "custom" delimiter and use | as the character.
When done, save the sheet as a CSV format with the name of SimpEn.csv.

## Benefits
* Translation tools often have a character limit, not word limit.  With a spreadsheet, it is easy to copy only the Chinese text column to the translator.  It is also easier to translate a single item in case of an issue.
* Spreadsheets often have a function to translate a cell directly in the spreadsheet. It also makes it easier to see accidental English to English translations.  Often translation tools will retranslate English text differently if included in the translation submission.  Sometimes translators will combine multiple rows into a single row.
* Some spreadsheets already have a translation function built in.  
	* Google Sheets has a function called GoogleTranslate() can be used to translate text.  Example:  For example, Chinese to English is `=GOOGLETRANSLATE(C1,"zh","en")`.
	* A DeepL translation function for Google Sheets can be found at https://github.com/DeepLcom/google-sheets-example/
* Text that have quotes or commas will be automatically escaped
	* Example that has both a comma and a quote:  After 15 minutes, will create a "Foo".  Will be encoded as: "After 15 minutes, will create a ""Foo"""

## Observations From Current Mods

* There might be duplicate translations for the same key.  This is most likely an oversight.  Translate the text and pick the best one, removing the others.  They can be left in the file, but the Mod Loader will pick one and ignore the rest.  This issue will show up in the errors section of the output.  

* There might be entries that are from the base game, which can be removed.  These will often show up as having an English translation already.  They can be left in as the loader will ignore them, but it makes the translation file cleaner if they are removed.

* Some mods will have no LocalizationKey at all and only Default Text.  This tool will generate a localization key and change the mod's .json files to use that new key.

* Some mods include DLL's which may create or modify cards.  In this case, the code has to be changed to handle the translation.  If the mod has a DLL and uses ModLoader, the LocalizationKey can be set in the Code and then the entry added to the mod's ./Localization/SimpEn.csv.  Additionally, the C# class at https://github.com/NBKRedSpy/CardSurvival-LocalizationStringUtility can be used to create keys programmatically with a minimum amount of code changes.


# Command Line Parameters
|Arguments|Description|
|--|--|
|Path to Mod Directory|The full path to the mod that will be translated.  This must be the root of the mod and will contain a ModInfo.json |

## Mod's Root Folder
Note that the root of the mod may be several directories deep.  Look for the ModInfo.json file

# Source and Releases for this Utility
https://github.com/NBKRedSpy/CardSurvival-Localization

## CSTI-ModLoader is at

https://www.nexusmods.com/cardsurvivaltropicalisland/mods/23

https://github.com/dop-lm/CSTI-ModLoader  (Currently the NoReflection branch has the latest code.  The master branch is out of date.  This doc will be updated when the repo is back in sync.)



# Version
## 3.1.0
* Changed key generation to use T-(text's SHA1 Hash).
	* This is the same as the https://github.com/NBKRedSpy/CardSurvival-LocalizationStringUtility for cross mod key compatibility.
	* Strings are trimmed before creating the hash.
* Changed output to use pipes (|) instead of commas to avoid needing to find all Unicode characters that get interpreted as commas in Google Sheets.
	* The user should now convert the pipes to commas before inserting into SimpEn.csv


## 3.0.1
Added code to replace unicode characters that Google Sheets is interpreting as commas to commas.
## 3.0.0
* Can generate LocalizationKey's for objects which have DefaultText, but no key.
* SimpEn.csv and SimpEn_Errors.txt files are now placed in the Mod's Localization folder.
* The target mod's path must point to the root of the mod, which contains a ModInfo.json.
* Due to possible key generation, this tool may now modify .json files and therefore must be deployed as a whole.
	All files which had a key generated will have an entry in the SimpEn_Errors.txt file.
* Internal - Significant code changes to support generating keys.
* Internal - Added unit tests.

## 2.0.0

### Upgrade Notes
For users that have used a previous version of the app, the arguments have changed.  The application now only requires the path to the target mod and will automatically create a SimpEn.csv and SimpEn_Errors.txt in the current working directory.  
### Changes
* Changed arguments to only require path.
* Changed to output errors to SimpEn_Errors.txt instead of in the translated file.
* Removed second parameter.  Now always exports to SimpEn.csv in the current working folder.

## 1.2.0
* Moved errors to output instead of stderr.
* Added File Name and JSON path to errors.
* Moved some code to a function.
* Escapes new lines as \n in the text.
* Fixed packaging not using Release build.

## 1.1.0
* Added CSV encoding.
* Remove duplicates based on key/text.
* Warns if a key has multiple entries with different versions.

## 1.0.0
* Release
