# CardSurvival-Localization

A utility to create English translations for mods that depend on the CSTI-ModLoader (AKA ModLoader).  Example mods are "Tea Lover" and "Greenhouse".

# Update Note
For users that have used a version of the app prior to 2.0.0, the arguments have changed.  The application now only requires the path to the target mod and will automatically create a SimpEn.csv and SimpEn_Errors.txt in the current working directory.  

Errors are now in a separate file called SimpEn_Errors.txt.

# Overview
This tool was made to help create localization files for Mod Loader mods that currently do not have an English translation.  This is not a replacement for a real translation or an official translation from the mod's author.  However, it is a useable workaround and can also be used to help mod authors start a translation file.

Users can either manually translate the text, or use a translation website.

This document is focused on Chinese to English translation, but can also be used for English to Chinese.

### Existing Translation File Note
If the mod is in Chinese and already has a ./Localization/SimpCn* file (For example, SimpCn.csv) and the file is not empty, then this mod is not needed. 
Simply copy the file to SimpEn.csv and put the translation in the second column. 

## Purpose
The challenge for creating English translations for Chinese only card packs is that often the mod does not have a SimpCn* translation file.  Without a translation file, the actual text source will be spread across all of the .json files, which in some cases is over 500 files.

This mod parses all the .json files and extracts the card translation key and related text.  The result is outputted to a single CSV file in the ModLoader's translation file format.

# Usage Summary
To translate a mod based on CSTI-ModLoader to English, the process is as follows:

* Run the tool, pointing to the mod's folder.
	* Example:  `CardSurvival-Localization.exe "E:\Mods\Apocalypse-43-1-39-1679945367\`
	* The app will create a SimpEn.csv file in the current working directory.  If there are errors, there will be a file named SimpEn_Errors.txt.
* Translate the SimpEnv.csv output file manually or with a translator such as translate.google.com or DeepL.com.
	* Put the text translated from the third column into the second column.
* Fix any errors listed in the SimpEn_Errors.txt file.
* Copy the SimpEn.csv into the Mod's Localization folder.

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
* Run the tool to create the SimpEn.csv output.
	* The app will create a SimpEn.csv file in the current working directory.  If there are errors, there will be a file named SimpEn_Errors.txt.
* Import the  SimpEn.csv document into Excel or Google Sheets.
* Copy the entirety of the third column (which will be in Chinese) and run it through a translator such as translate.google.com or deepl.com.
	* Alternatively, Google Sheets has a function called GoogleTranslate that can translate text in the spreadsheet.  For example Chinese to English is `=GOOGLETRANSLATE(C1,"zh","en")`.
* Paste those results into the spreadsheet's second column.
* Save the spreadsheet as SimpEn.csv.
* If a SimpEn_Error.txt file was created, fix any errors indicated in that file.
* Copy the SimpEn.csv file to the Mod's Localization directory.

When starting the game, the Mod's text should now reflect the translated text.


## Observations From Current Mods
A couple of things I've noticed in the current mods.  

* There might be duplicate translations for the same key.  This is most likely an oversight.  Translate the text and pick the best one, removing the others.  They can be left in the file, but the Mod Loader will pick one and ignore the rest.  This issue will show up in the errors section of the output.
* There might be entries that are from the base game, which can be removed.  These will often show up as having an English translation already.  They can be left in as the loader will ignore them, but it makes the translation file cleaner if they are removed.


# Command Line Parameters
|Arguments|Description|
|--|--|
|File Pattern|The full path to the mod that will be translated.|

## Source and Releases for this Utility
https://github.com/NBKRedSpy/CardSurvival-Localization

## CSTI-ModLoader is at

https://www.nexusmods.com/cardsurvivaltropicalisland/mods/23

https://github.com/dop-lm/CSTI-ModLoader  (Currently the NoReflection branch has the 2.0.1c code.  The master branch is out of date.  This doc will be updated when the repo is back in sync.)


# Version

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
