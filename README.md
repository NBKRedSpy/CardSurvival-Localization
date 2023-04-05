# CardSurvival-Localization

A utility to create English translations for mods that depend on the CSTI-ModLoader (AKA ModLoader).  Example mods are "Tea Lover" and "Greenhouse".

# 3.0.0 Notes for Existing Users
Version 3.0.0 now supports translating data which has DefaultText but no LocalizationKey.
3.0.0 will generate a new LocalizationKey to allow translation.  However, this key must be written to the affected .json file, which means all of the mod's .json files must be distributed.

The tool now requires pointing to the root of the mod's directory, which will contain a ModInfo.json file.
The SimpEn.csv file will automatically be put in the mod's ./Localization folder instead of the current working directory.

# Overview
This tool was made to help create localization files for Mod Loader mods that currently do not have an English translation.  This is not a replacement for a real translation or an official translation from the mod's author.  However, it is a useable workaround and can also be used to help mod authors start a translation file.

Users can either manually translate the text, or use a translation website.

This document is focused on Chinese to English translation, but can also be used for English to Chinese.

### Existing Translation File Note
If the mod is in Chinese and already has a ./Localization/SimpCn* file (For example, SimpCn.csv) and the file is not empty, then this mod is not needed. 
Simply copy the file to SimpEn.csv and put the translation in the second column. 

# Usage Summary
To translate a mod based on CSTI-ModLoader to English, the process is as follows:

* Run the tool, pointing to the mod's folder.
	* Example:  `CardSurvival-Localization.exe "E:\Mods\Apocalypse-43-1-392-1680010396\DisasterBeacons"`
	* The app will create a SimpEn.csv file in mod's Localization folder.  If there are errors, there will also be a file named SimpEn_Errors.txt.
* Translate the SimpEnv.csv output file manually or with a translator such as translate.google.com or DeepL.com.
	* Put the text translated from the third column into the second column.
* Fix any errors listed in the SimpEn_Errors.txt file.

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

SimpEn.csv
```
Bp_ConservatoriesNc_CardDescription,,为植物生长提供一个温馨的家园。
Bp_ConservatoriesNc_CardDescription,,保护植物并加速生长
Bp_ConservatoriesNc_Two_CardName,,二号温室
```

## Finished SimpEn.csv:
```
Bp_ConservatoriesNc_CardDescription,Provides a warm home for plant growth.,为植物生长提供一个温馨的家园。
Bp_ConservatoriesNc_Two_CardName,Greenhouse No. 2,二号温室
```


# Spreadsheet Recommended Workflow
My recommendation would be to do as follows:
* Run the tool to create the Localization/SimpEn.csv output.
	* If there are errors, there will also be a file named SimpEn_Errors.txt as well.
* Import the  SimpEn.csv document into Excel or Google Sheets.
* Copy the entirety of the third column (which will be in Chinese) and run it through a translator such as translate.google.com or deepl.com.
	* Alternatively, Google Sheets has a function called GoogleTranslate that can translate text in the spreadsheet.  For example, Chinese to English is `=GOOGLETRANSLATE(C1,"zh","en")`.
	* ```=DETECTLANGUAGE(C1)``` can be used to find text that is already in English.  This can be used to sort the sheet so all of the English items are together.  This makes it easier to remove any translations that are duplicates of the game's text.
* Paste those results into the spreadsheet's second column.
* Save the spreadsheet as SimpEn.csv.
* If a SimpEn_Error.txt file was created, fix any errors indicated in that file.

When starting the game, the Mod's text should now reflect the translated text.


## Observations From Current Mods

* There might be duplicate translations for the same key.  This is most likely an oversight.  Translate the text and pick the best one, removing the others.  They can be left in the file, but the Mod Loader will pick one and ignore the rest.  This issue will show up in the errors section of the output.

* There might be entries that are from the base game, which can be removed.  These will often show up as having an English translation already.  They can be left in as the loader will ignore them, but it makes the translation file cleaner if they are removed.

* Some mods will have no LocalizationKey at all and only Default Text.  This tool will generate a localization key and change the mod's .json files to use that new key.


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
