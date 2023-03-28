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

# Todo:
* Maybe use tabs instead of commas since the text can have commas.  Maybe have option between CSV and TSV since tabs are easier import and manipulate.
* It needs to remove duplicates, or at least warn about them.  There are mods which have the same key multiple times.  Usually it has the same text.  Maybe just by need of the ModLoader format?
* Should remove the *.json glob.  They will always be .json and just needs a root directly specified.
* CSV output should support wrapping the columns in quotes and quotation escaping.
