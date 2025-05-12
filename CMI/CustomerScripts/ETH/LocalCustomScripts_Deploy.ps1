param (
    [Parameter(Mandatory=$true)][string]$target
)

$target = $target.ToUpper();

## Target array
$arr=@{}

$arr["HSA"] = @{}
$arr["HSA"]["customer.settings.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["HSA"]["customer.translations.de.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["HSA"]["customer.translations.en.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["HSA"]["loading-site.css"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\css"
$arr["HSA"]["loading-texts.js"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\js";
$arr["HSA"]["logo.svg"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\assets\img";
$arr["HSA"]["logo.png"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\assets\img";
$arr["HSA"]["custom.css"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\css";


$arr["TMA"] = @{}
$arr["TMA"]["customer.settings.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["TMA"]["customer.translations.de.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["TMA"]["customer.translations.en.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["TMA"]["loading-site.css"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\css";
$arr["TMA"]["loading-texts.js"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\js";
$arr["TMA"]["logo.svg"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\assets\img";
$arr["TMA"]["logo.png"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\assets\img";
$arr["TMA"]["custom.css"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\css";


$arr["MFA"] = @{}
$arr["MFA"]["customer.settings.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["MFA"]["customer.translations.de.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["MFA"]["customer.translations.en.json"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config";
$arr["MFA"]["loading-site.css"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\css";
$arr["MFA"]["loading-texts.js"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\js";
$arr["MFA"]["logo.svg"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\assets\img";
$arr["MFA"]["logo.png"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\assets\img";
$arr["MFA"]["custom.css"] = "C:\Dev\CMInformatik\cmi-lesesaal\CMI\Web\CMI.Web.Frontend\client\config\css";

## Source
switch($target) {
    "DEV" { $source = ".\HSA\" }
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