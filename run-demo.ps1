# Starts the local presenter demo at http://localhost:5255 on the local database TOP4HoneyChainsDb.
# Fake SMS: no message leaves this machine; codes appear in the SMS Outbox (Platform administrator).
# The settings exist only as environment variables of the started process; no appsettings file changes.
# "Show animations" is a per-user setting, on by default; the presenter accounts keep it on.
# Stop with Ctrl+C.

$ErrorActionPreference = 'Stop'

$project = Join-Path $PSScriptRoot 'TOP4HONEYCHAINS\TOP4HoneyChains.Web'
if (-not (Test-Path $project)) { throw "Project not found: $project" }

$listening = Get-NetTCPConnection -LocalPort 5255 -State Listen -ErrorAction SilentlyContinue
if ($listening) { throw 'Port 5255 is already in use. Stop the running app first.' }

$settings = [ordered]@{
    'ASPNETCORE_ENVIRONMENT'               = 'Development'
    'ASPNETCORE_URLS'                      = 'http://localhost:5255'
    'ConnectionStrings__DefaultConnection' = 'Server=(localdb)\MSSQLLocalDB;Database=TOP4HoneyChainsDb;Trusted_Connection=True;TrustServerCertificate=True'
    'Sms__Provider'                        = 'Fake'
    'Sms__AllowFake'                       = 'true'
    'Features__OrgRoleModel'               = 'true'
    'Features__CustodyIntake'              = 'true'
    'Features__BeekeeperAccounts'          = 'true'
}

$startInfo = New-Object System.Diagnostics.ProcessStartInfo
$startInfo.FileName = 'dotnet'
$startInfo.Arguments = "run --no-launch-profile --project `"$project`""
$startInfo.WorkingDirectory = $PSScriptRoot
$startInfo.UseShellExecute = $false
foreach ($name in $settings.Keys) { $startInfo.EnvironmentVariables[$name] = $settings[$name] }

Write-Host 'TOP4HoneyChains presenter demo: http://localhost:5255  (Ctrl+C to stop)'
$process = [System.Diagnostics.Process]::Start($startInfo)
$process.WaitForExit()
exit $process.ExitCode
