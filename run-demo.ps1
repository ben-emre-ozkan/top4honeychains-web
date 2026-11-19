# Starts the local demo at http://localhost:5255 on the local database TOP4HoneyChainsDb.
# Fake SMS: no message leaves this machine. Each phone number has a phone page with its messages (D-88); the platform
# administrator also sees them in the SMS outbox.
# The settings exist only as environment variables of the started process; no appsettings file changes.
# "Show animations" is a per-user setting, on by default.
#
#   -Lan     also listens on this PC's network address, so a phone on the same Wi-Fi can open the phone pages; prints the links.
#            Without it nothing changes: only http://localhost:5255.
#   -NewKey  makes a new phone page key; earlier phone links stop working. The key is kept in the user secrets of the web
#            project (Sms:PhonePageKey), so a bookmarked link keeps working after a restart.
#
# Stop with Ctrl+C.

param(
    [switch]$Lan,
    [switch]$NewKey
)

$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'TOP4HONEYCHAINS\TOP4HoneyChains.Web'
if (-not (Test-Path $project)) { throw "Project not found: $project" }

$listening = Get-NetTCPConnection -LocalPort 5255 -State Listen -ErrorAction SilentlyContinue
if ($listening) { throw 'Port 5255 is already in use. Stop the running app first.' }

# the phone page key (D-88), kept in dotnet user-secrets
$key = $null
if (-not $NewKey) {
    foreach ($line in (& dotnet user-secrets list --project $project 2>$null)) {
        if ($line -match '^Sms:PhonePageKey = (.+)$') { $key = $Matches[1].Trim() }
    }
}
if (-not $key -or $key -notmatch '^[A-Za-z0-9_-]{16,64}$') {
    $bytes = New-Object byte[] 18
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    $key = [Convert]::ToBase64String($bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=')
    & dotnet user-secrets set 'Sms:PhonePageKey' $key --project $project | Out-Null
    Write-Host 'A new phone page key was made; earlier phone links no longer work.'
}

$urls = @('http://localhost:5255')
if ($Lan) {
    $address = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        Where-Object { $_.IPAddress -notlike '127.*' -and $_.IPAddress -notlike '169.254.*' -and $_.PrefixOrigin -in @('Dhcp', 'Manual') } |
        Sort-Object InterfaceMetric | Select-Object -First 1 -ExpandProperty IPAddress
    if (-not $address) { throw 'No network address found for -Lan. Connect this PC to the network first.' }
    $urls += "http://${address}:5255"
}

$settings = [ordered]@{
    'ASPNETCORE_ENVIRONMENT'               = 'Development'
    'ASPNETCORE_URLS'                      = ($urls -join ';')
    'ConnectionStrings__DefaultConnection' = 'Server=(localdb)\MSSQLLocalDB;Database=TOP4HoneyChainsDb;Trusted_Connection=True;TrustServerCertificate=True'
    'Sms__Provider'                        = 'Fake'
    'Sms__AllowFake'                       = 'true'
    'Sms__PhonePageKey'                    = $key
    'Features__OrgRoleModel'               = 'true'
    'Features__CustodyIntake'              = 'true'
    'Features__BeekeeperAccounts'          = 'true'
    'Features__ConsumerAccounts'           = 'true'
}

$startInfo = New-Object System.Diagnostics.ProcessStartInfo
$startInfo.FileName = 'dotnet'
$startInfo.Arguments = "run --no-launch-profile --project `"$project`""
$startInfo.WorkingDirectory = $PSScriptRoot
$startInfo.UseShellExecute = $false
foreach ($name in $settings.Keys) { $startInfo.EnvironmentVariables[$name] = $settings[$name] }

Write-Host 'TOP4HoneyChains demo: http://localhost:5255  (Ctrl+C to stop)'

# the phone links: one per person of the local cast (tools/live-like/cast.json), otherwise the pattern
$castFile = Join-Path $PSScriptRoot 'tools\live-like\cast.json'
$people = @()
if (Test-Path $castFile) {
    $cast = Get-Content $castFile -Raw -Encoding UTF8 | ConvertFrom-Json
    $people = @($cast.people) + @($cast.beekeepers) | Where-Object { $_.phone }
}
foreach ($url in $urls) {
    Write-Host ''
    Write-Host "Phone pages on ${url}:"
    if ($people.Count -gt 0) {
        foreach ($p in $people) {
            $digits = ($p.phone -replace '[^0-9]', '')
            Write-Host ('  {0,-22} {1}/phone/{2}/{3}' -f $p.name, $url, $key, $digits)
        }
    }
    else {
        Write-Host "  $url/phone/$key/<number, digits only, e.g. 905550000101>"
    }
}
if ($Lan) {
    Write-Host ''
    Write-Host 'Open a link on a phone in the same network. If Windows asks, allow dotnet on private networks.'
}
Write-Host ''

$process = [System.Diagnostics.Process]::Start($startInfo)
# D-91: wait in short slices so Ctrl+C reaches this script, and stop the whole tree on the way out: `dotnet run` starts the app
# as a child process, which otherwise kept running (and kept port 5255) after Ctrl+C
try {
    while (-not $process.WaitForExit(500)) { }
}
finally {
    if (-not $process.HasExited) { & taskkill /PID $process.Id /T /F 2>$null | Out-Null }
    Get-NetTCPConnection -LocalPort 5255 -State Listen -ErrorAction SilentlyContinue |
        ForEach-Object { Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue } |
        Where-Object { $_.ProcessName -eq 'TOP4HoneyChains.Web' } | Stop-Process -Force -ErrorAction SilentlyContinue
}
exit $process.ExitCode
