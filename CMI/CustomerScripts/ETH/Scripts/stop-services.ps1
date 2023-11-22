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

    Write-Host "Stoppe alle Servces für Instanz $Instance."
    StartServices
}

Function StartServices {
    
    foreach($service in  Get-Service -ErrorAction SilentlyContinue | Where-Object {$_.Status -eq 'Running'  -and $_.Name -like "*$Instance" })
    {
        Write-Host Stoppe $service.Name ...
        Stop-Service $service
    }
}

Main