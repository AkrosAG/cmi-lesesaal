param (
    [Parameter(Mandatory=$true)][string]$target,
    [Parameter(Mandatory=$false)][bool]$clearTargetDirectory
)

$target = $target.ToUpper();

## Target array
$arr=@{}

$arr["MFA"] = @{}
$arr["MFA"]["loading-site.css"] = "D:\localapps\MFA\LesesaalWeb\Management\client\assets\css";
$arr["MFA"]["loading-texts.js"] = "D:\localapps\MFA\LesesaalWeb\Management\client\config";
$arr["MFA"]["logo.svg"] = "D:\localapps\MFA\LesesaalWeb\Management\client\assets\img";
$arr["MFA"]["logo.png"] = "D:\localapps\MFA\LesesaalWeb\Management\client\assets\img";

$arr["HSA"] = @{}
$arr["HSA"]["loading-site.css"] = "D:\localapps\HSA\LesesaalWeb\Management\client\assets\css";
$arr["HSA"]["loading-texts.js"] = "D:\localapps\HSA\LesesaalWeb\Management\client\config";
$arr["HSA"]["logo.svg"] = "D:\localapps\HSA\LesesaalWeb\Management\client\assets\img";
$arr["HSA"]["logo.png"] = "D:\localapps\HSA\LesesaalWeb\Management\client\assets\img";

$arr["TMA"] = @{}
$arr["TMA"]["loading-site.css"] = "D:\localapps\TMA\LesesaalWeb\Management\client\assets\css";
$arr["TMA"]["loading-texts.js"] = "D:\localapps\TMA\LesesaalWeb\Management\client\config";
$arr["TMA"]["logo.svg"] = "D:\localapps\TMA\LesesaalWeb\Management\client\assets\img";
$arr["TMA"]["logo.png"] = "D:\localapps\TMA\LesesaalWeb\Management\client\assets\img";




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