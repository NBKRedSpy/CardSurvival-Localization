# CardSurvival-Localization

A quick and dirty utility to extract localization data from ModLoader based mods.  

Useful for creating an English translation for Chinese ModLoader mods that do not have a localization file (SimpCn.csv).

## CSTI-ModLoader is at:

https://www.nexusmods.com/cardsurvivaltropicalisland/mods/23

https://github.com/dop-lm/CSTI-ModLoader  (Out of date.  2.0.1b was released on NexusMods on 3/28/2023, but the repository doesn't currently reflect the change)


# Operation
The utility goes through every .json file in a ModLoader based mod's folder and extracts every DefaultText and related LocalizationKey.
The output is a CSV in the same format as SimpCn.csv
The result can be exported to the console or a file.

If there are any errors or warnings, they would be outputted to stderr.  For example, multiple entries of the same key with different text.

# Usage:
Run the exe, passing in the folder with *.json at the end.

For example:
Assuming the target mod is located at `E:\Mods\Apocalypse-43-1-39-1679945367` and is not in zip format.

`CardSurvival-Localization.exe "E:\Mods\Apocalypse-43-1-39-1679945367\*.json" "c:\work\SimpCn.csv"`

|Arguments|Description|
|--|--|
|File Pattern|Use the full path with *.json at the end.  For example:  "SomeModLoaderMod\\\*.json"|
|Output File|If not provided, will output to the console.  Otherwise will write to the path specified|

# Output:
Creates a CSV output text file with the following columns:

Localization Key  
Empty  
The DefaultText for that key.  


## Example:
Bp_Hail_IceCoolBall.LogText,,冰晶球完成了

## Translation
An easy way to translate is using Google Sheets with the translate function.
For example, translating Chinese to English: `=GOOGLETRANSLATE(C1,"zh","en")`

# Version

## 1.1.0
* Added CSV encoding
* Remove duplicates based on key/text
* Warns if a key has multiple entries with different versions.

## 1.0.0
* Release