# IIS Setup Script for eService
# Run this as Administrator

$appcmd = "C:\Windows\System32\inetsrv\appcmd.exe"
$sitePath = "F:\AITest\MOH\CodeBaseNew\e-service\ES"
$siteName = "eService"
$poolName = "eServicePool"
$port = 9000

Write-Host "Setting up IIS for eService..." -ForegroundColor Cyan

# Delete existing site if exists
Write-Host "Removing existing sites..."
$sites = & $appcmd list site
foreach ($site in $sites) {
    if ($site -match 'SITE "(.+?)"') {
        $name = $matches[1]
        Write-Host "  Deleting site: $name"
        & $appcmd delete site $name
    }
}

# Delete existing app pool if exists  
Write-Host "Removing existing app pool..."
& $appcmd delete apppool $poolName 2>$null

# Create new app pool with .NET 4.0
Write-Host "Creating app pool: $poolName"
& $appcmd add apppool /name:$poolName /managedRuntimeVersion:v4.0 /managedPipelineMode:Integrated

# Create new site
Write-Host "Creating site: $siteName on port $port"
& $appcmd add site /name:$siteName /physicalPath:$sitePath /bindings:"http/*:${port}:"

# Set site app pool
Write-Host "Setting app pool..."
& $appcmd set app "$siteName/" /applicationPool:$poolName

# Start site
Write-Host "Starting site..."
& $appcmd start site $siteName

Write-Host ""
Write-Host "IIS site configured successfully!" -ForegroundColor Green
Write-Host "Access the site at: http://localhost:$port" -ForegroundColor Yellow
Write-Host "Admin login: http://localhost:$port/BACKMIN/Login" -ForegroundColor Yellow

