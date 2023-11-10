param (
    [Parameter(Mandatory=$true)][string]$target,
    [Parameter(Mandatory=$false)][bool]$clearTargetDirectory
)

$target = $target.ToUpper();

## Target array
$arr=@{}

$arr["HSA"] = @{}
$arr["HSA"]["customer.settings.json"] = "D:\localapps\LesesaalWeb\hsa\Frontend\client\config";
$arr["HSA"]["customer.translations.de.json"] = "D:\localapps\LesesaalWeb\hsa\Frontend\client\config";
$arr["HSA"]["customer.translations.en.json"] = "D:\localapps\LesesaalWeb\hsa\Frontend\client\config";
$arr["HSA"]["Facetten.json"] = "D:\localdata\HSA\CustomScripts";
$arr["HSA"]["HarvestCustomScript.cs"] = "D:\localdata\HSA\CustomScripts";
$arr["HSA"]["HarvestCustomScript.cs"] = "D:\localdata\HSA\CustomScripts";
$arr["HSA"]["loading-site.css"] = "D:\localdata\HSA\CustomScripts";
$arr["HSA"]["loading-texts.js"] = "D:\localapps\LesesaalWeb\hsa\Frontend\client\config";
$arr["HSA"]["logo.svg"] = "D:\localapps\LesesaalWeb\hsa\Frontend\client\assets\img";
$arr["HSA"]["logo.png"] = "D:\localapps\LesesaalWeb\hsa\Frontend\client\config";
$arr["HSA"]["templates.json"] = "D:\localdata\HSA\CustomScripts";
$arr["HSA"]["custom.css"] = "TODO";


$arr["DEV"] = @{}
$arr["DEV"]["customer.settings.json"] = "D:\localapps\LesesaalWeb\dev\Frontend\client\config";
$arr["DEV"]["customer.translations.de.json"] = "D:\localapps\LesesaalWeb\dev\Frontend\client\config";
$arr["DEV"]["customer.translations.en.json"] = "D:\localapps\LesesaalWeb\dev\Frontend\client\config";
$arr["DEV"]["Facetten.json"] = "D:\localdata\dev\CustomScripts";
$arr["DEV"]["HarvestCustomScript.cs"] = "D:\localdata\dev\CustomScripts";
$arr["DEV"]["HarvestCustomScript.cs"] = "D:\localdata\dev\CustomScripts";
$arr["DEV"]["loading-site.css"] = "D:\localdata\dev\CustomScripts";
$arr["DEV"]["loading-texts.js"] = "D:\localapps\LesesaalWeb\dev\Frontend\client\config";
$arr["DEV"]["logo.svg"] = "D:\localapps\LesesaalWeb\dev\Frontend\client\assets\img";
$arr["DEV"]["logo.png"] = "D:\localapps\LesesaalWeb\dev\Frontend\client\config";
$arr["DEV"]["templates.json"] = "D:\localdata\dev\CustomScripts";
$arr["DEV"]["custom.css"] = "TODO";

$arr["ETH"] = @{}
$arr["ETH"]["customer.settings.json"] = "D:\localapps\LesesaalWeb\Frontend\client\config";
$arr["ETH"]["customer.translations.de.json"] = "D:\localapps\LesesaalWeb\Frontend\client\config";
$arr["ETH"]["customer.translations.en.json"] = "D:\localapps\LesesaalWeb\Frontend\client\config";
$arr["ETH"]["Facetten.json"] = "D:\localdata\CustomScripts";
$arr["ETH"]["HarvestCustomScript.cs"] = "D:\localdata\CustomScripts";
$arr["ETH"]["HarvestCustomScript.cs"] = "D:\localdata\CustomScripts";
$arr["ETH"]["loading-site.css"] = "D:\localdata\CustomScripts";
$arr["ETH"]["loading-texts.js"] = "D:\localapps\LesesaalWeb\Frontend\client\config";
$arr["ETH"]["logo.svg"] = "D:\localapps\LesesaalWeb\Frontend\client\assets\img";
$arr["ETH"]["logo.png"] = "D:\localapps\LesesaalWeb\Frontend\client\assets\img";
$arr["ETH"]["templates.json"] = "D:\localdata\CustomScripts";
$arr["ETH"]["custom.css"] = "TODO";

$arr["TMA"] = @{}
$arr["TMA"]["customer.settings.json"] = "D:\localapps\LesesaalWeb\TMA\Frontend\client\config";
$arr["TMA"]["customer.translations.de.json"] = "D:\localapps\LesesaalWeb\TMA\Frontend\client\config";
$arr["TMA"]["customer.translations.en.json"] = "D:\localapps\LesesaalWeb\TMA\Frontend\client\config";
$arr["TMA"]["Facetten.json"] = "D:\localdata\TMA\CustomScripts";
$arr["TMA"]["HarvestCustomScript.cs"] = "D:\localdata\TMA\CustomScripts";
$arr["TMA"]["HarvestCustomScript.cs"] = "D:\localdata\TMA\CustomScripts";
$arr["TMA"]["loading-site.css"] = "D:\localdata\TMA\CustomScripts";
$arr["TMA"]["loading-texts.js"] = "D:\localapps\LesesaalWeb\TMA\Frontend\client\config";
$arr["TMA"]["logo.svg"] = "D:\localapps\LesesaalWeb\TMA\Frontend\client\assets\img";
$arr["TMA"]["logo.png"] = "D:\localapps\LesesaalWeb\TMA\Frontend\client\config";
$arr["TMA"]["templates.json"] = "D:\localdata\TMA\CustomScripts";
$arr["TMA"]["custom.css"] = "TODO";


$arr["MFA"] = @{}
$arr["MFA"]["customer.settings.json"] = "D:\localapps\LesesaalWeb\MFA\Frontend\client\config";
$arr["MFA"]["customer.translations.de.json"] = "D:\localapps\LesesaalWeb\MFA\Frontend\client\config";
$arr["MFA"]["customer.translations.en.json"] = "D:\localapps\LesesaalWeb\MFA\Frontend\client\config";
$arr["MFA"]["Facetten.json"] = "D:\localdata\MFA\CustomScripts";
$arr["MFA"]["HarvestCustomScript.cs"] = "D:\localdata\MFA\CustomScripts";
$arr["MFA"]["HarvestCustomScript.cs"] = "D:\localdata\MFA\CustomScripts";
$arr["MFA"]["loading-site.css"] = "D:\localdata\MFA\CustomScripts";
$arr["MFA"]["loading-texts.js"] = "D:\localapps\LesesaalWeb\MFA\Frontend\client\config";
$arr["MFA"]["logo.svg"] = "D:\localapps\LesesaalWeb\MFA\Frontend\client\assets\img";
$arr["MFA"]["logo.png"] = "D:\localapps\LesesaalWeb\MFA\Frontend\client\config";
$arr["MFA"]["templates.json"] = "D:\localdata\MFA\CustomScripts";
$arr["MFA"]["custom.css"] = "TODO";

## Source
switch($target) {
    "DEV" { $source = ".\HSA\" }
    "ETH" { $source = ".\HSA\" }
    "HSA" { $source = ".\HSA\" }

    "MFA" { $source = ".\MFA\" }

    "TMA" { $source = ".\TMA\" }

    Default { 
        Write-Host "Target with name $target not found" -ForegroundColor Red
        exit -1
    }
}

if($arr[$target] -eq $null) {
    Write-Host "Target $target does not exists" -ForegroundColor Red
    exit 1
}

## Clear target directory if needed
if($clearTargetDirectory -eq $true) {
    foreach($item in $arr[$target].Keys) {
        $targetDir = $arr[$target][$item]
        if((Test-Path $targetDir) -eq $true) {
            Write-Host "Deleting directory $targetDir" -ForegroundColor Yellow
            Remove-Item $targetDir -Recurse
        }
    }
}

## Move
foreach($item in $arr[$target].Keys) {
    $targetDir = $arr[$target][$item]

    if((Test-Path -Path $targetDir) -eq $false) {
        New-Item -ItemType Directory -Path $targetDir
    }

    $sourceFile = [System.IO.Path]::Combine($PSScriptRoot, $source, $item)
    $destinationFile = [System.IO.Path]::Combine($targetDir, $item)

    if((Test-Path $sourceFile) -eq $true) {
        Copy-Item $sourceFile -Destination $destinationFile -Force
        Write-Host "$sourceFile File copied to $destinationFile"
    }
}