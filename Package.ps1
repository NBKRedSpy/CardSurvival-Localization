dotnet publish
# Get the file name from the C# project name
$ProjectName = "CardSurvival-Localization"

Compress-Archive -Path "./CardSurvival-Localization/bin/Debug/net6.0/publish/*" -Force -DestinationPath ./$ProjectName.zip
