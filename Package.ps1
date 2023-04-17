$ProjectName = "CardSurvival-Localization"
$PackageFolder = "./Package/"
$ArchiveName = "./$ProjectName.zip"

mkdir -ErrorAction SilentlyContinue $PackageFolder
Remove-Item -ErrorAction SilentlyContinue -Recurse ./Package/*
Remove-Item -ErrorAction SilentlyContinue $ArchiveName

dotnet publish ".\CardSurvival-Localization\CardSurvival-Localization.csproj" -c Release -o $PackageFolder


Compress-Archive -Path $PackageFolder/* -DestinationPath $ArchiveName


