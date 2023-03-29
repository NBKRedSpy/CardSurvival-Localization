# CardSurvival-Localization

A utility to extract localization data from ModLoader based mods (AKA CSTI-ModLoader).  

Useful for creating an English translation for Chinese ModLoader mods that do not have a localization file (For example, SimpEn.csv).

# Imporant
This is a tool for modders and is not useful otherwise.

# Overview
This tool was made to help create localization files for Mod Loader mods that currently do not have an English translation.  This is not a replacement for a real translation or an offical translation from the mod's author.  However, it is a useable workaround and can also be used to help mod authors start a translation file.

## Mod's Text Source
In the Mod Loader mods, the source for the text can come from one of two places:  The card data itself (spread across many .json files), or localization files in the Mod's Localization folder.

This is for mods that do not have a SimpEn* file in the Localization directory. For example SimpEn.csv or SimpEn2.csv.

## Recommendations
As described later in the document, use this tool to create a new .csv.  

My recommendation would be to do as follows:
* Remove any errors at the top of the file and import the CSV part of the document into Excel or Google Sheets.  Then take the 3rd column (which will be in Chinse) and run it through a translater like translate.google.com or www.deepl.com.
	* Alternatively Google Sheets has a function called GoogleTranslate that can translate text in the spreadsheet.  For example Chinese to English is `=GOOGLETRANSLATE(C1,"zh","en")`.
* Copy those results to the spreadsheet's second column.
* Save the spreadsheet as SimpEn.csv.
* With the newly created SimpEn.csv file, fix the errors that were listed at the top of the original output.  
* Copy the result to the Mod's Localization directory.

When starting the game, the Mod's text should now reflect the translated text.


## Observations From Current Mods
A couple of things I've noticed in the current mods.  

* There might be duplicate translations for the same key.  This is most likely an oversight.  Translate the text and pick the best one, removing the others.  They can be left in the file, but the Mod Loader will pick one and ignore the rest.  This issue will show up in the errors section of the output.
* There might be entries that are from the base game, which can be removed.  These will often show up as having an English translation already.  They can be left in as the loader will ignore them, but it just makes the translation file cleaner to remove them.

## CSTI-ModLoader is at:

https://www.nexusmods.com/cardsurvivaltropicalisland/mods/23

https://github.com/dop-lm/CSTI-ModLoader  (Out of date.  The repository doesn't refelect the latest changes (2.0.1c) from 3/28/2023.  This doc will be updated when the repo is back in sync.)

## Source and Releases for this Mod
https://github.com/NBKRedSpy/CardSurvival-Localization

# Operation
The utility goes through every .json file in a ModLoader based mod's folder and extracts every LocalizationKey and the DefaultText for that key.  
The output will be a SimpEn.csv compatible file.  See The [Output](#output) section below.

The result can be exported to the console or a file; however, it is best to supply a file name for the output file or the console might corrupt Unicode characters.

If there are any errors or warnings, they will be added to the start of the output.  For example, multiple entries of the same key with different text.

# Usage
Run the exe, passing in the folder with *.json at the end.

For example:

Assuming the target mod is located at `E:\Mods\Apocalypse-43-1-39-1679945367` and is not in zip format.

`CardSurvival-Localization.exe "E:\Mods\Apocalypse-43-1-39-1679945367\*.json" "c:\work\SimpEn.csv"`

## Example Output

```
Error: Multiple keys exist with different text
Key: "test_key"
	Text: some "text"
	File: <..>\TestData\Test.json
	JSON Path: DefaultStatusName.Description.subTest

	Text: test
	File: <..>\TestData\Test.json
	JSON Path: DefaultStatusName.Description

-----
exact_dupe_test,,exact duplicate test
Gs_Hail_IceCool_Descriptions,,冰爽的感觉
test_key,,"some ""text"""
test_key,,test

```


|Arguments|Description|
|--|--|
|File Pattern|Use the full path with a search pattern at the end *.json at the end.  For example:  "SomeModLoaderMod\\\*.json"|
|Output File|If not provided, will output to the console.  Otherwise will write to the path specified.  It is recommended to use this argument.|

# Output
Creates a CSV output text file with the following:

* Any errors
* The data in CSV format:
	* Localization Key
	* Empty (The space for the English translation)
	* The DefaultText for that key.  

If the output is showing unicode characters as ?'s, supply a file name as the second parameter.


## Translation
An easy way to translate entries is using Google Sheets with the translate function.
For example, translating Chinese to English: `=GOOGLETRANSLATE(C1,"zh","en")`

# Version

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
