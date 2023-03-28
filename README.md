# CardSurvival-Localization

A quick and dirty utility to extract localization data from ModLoader based mods.

# Usage:
Run the exe, passing in the folder, using wild cards.  

|Arguments|Description|
|--|--|
|File Pattern|Use the full with *.json.  For example:  "SomeModLoaderMod\*.json"|
|Output File|If not provided, will output to the console.  Otherwise will write to the path specified|

# Output:
Creates a CSV output text file with the following columns:
Localization Key
Empty
The DefaultText for that key.

Example:
Bp_Hail_IceCoolBall.LogText,,冰晶球完成了

An easy way to translate is using Google Sheets with the translate function.
For example, translating Chinese to English: ```=GOOGLETRANSLATE(C1,"zh","en")```

# Todo:
* It should be changed to use tabs instead of commas since the text can have commas.
* It needs to duplicates, or at least warn about them.  There are mods which have the same key multiple times.  Maybe just by need of the ModLoader format?

