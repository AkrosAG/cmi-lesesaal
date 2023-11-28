#Requires -RunAsAdministrator

param(
    [String]$Instance                  <# Name der Instanz #>
)

Function Main
{
    if ([System.String]::IsNullOrWhitespace($Instance))
    {
        Write-Host "Service Instanze wurde nicht angegeben." -ForegroundColor red
        exit
    }

    Write-Host "Starte alle Servces für Instanz $Instance."
    StartServices
}

Function StartServices {
    
    foreach($service in  Get-Service -ErrorAction SilentlyContinue | Where-Object {$_.Status -eq 'Stopped'  -and $_.Name -like "*$Instance" })
    {
        Write-Host Starte $service.Name ...
        Start-Service $service
    }
}

Main