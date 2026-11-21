# The user manual's own run of the platform, on the fictional-only database TOP4HoneyChainsDb_Manual (setup-db.ps1).
#   app.ps1 reset    restores the database and the app's files to the state after setup-db.ps1 (the app must be stopped)
#   app.ps1 start    starts the app on http://localhost:<MANUAL_PORT> (default 5256) in the background; waits until it answers
#   app.ps1 stop     stops it
# Settings exist only as environment variables of the started process. Fake SMS: no message leaves this machine.

param([Parameter(Mandatory)][ValidateSet('reset', 'start', 'stop')][string]$Action)

$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
. (Join-Path $here 'manual-env.ps1')
$repo = (Resolve-Path (Join-Path $here '..\..\..')).Path
$web = Join-Path $repo 'TOP4HONEYCHAINS\TOP4HoneyChains.Web'
$cfg = Read-ManualEnv (Join-Path $here '.env.local')
if (-not $cfg['MANUAL_PASSWORD']) { throw 'Run setup-db.ps1 first.' }
$port = [int]$cfg['MANUAL_PORT']
$appRoot = Join-Path $ManualWork 'app'
$pidFile = Join-Path $ManualWork 'app.pid'

function Stop-ManualApp {
    if (Test-Path $pidFile) {
        $id = [int](Get-Content $pidFile)
        if (Get-Process -Id $id -ErrorAction SilentlyContinue) { & taskkill /PID $id /T /F | Out-Null }
        Remove-Item $pidFile
    }
    $listen = Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue
    foreach ($l in $listen) {
        $p = Get-Process -Id $l.OwningProcess -ErrorAction SilentlyContinue
        if ($p -and $p.ProcessName -in @('dotnet', 'TOP4HoneyChains.Web')) { Stop-Process -Id $p.Id -Force }
    }
}

switch ($Action) {
    'stop' { Stop-ManualApp; Write-Host 'stopped' }
    'reset' {
        Stop-ManualApp
        $bak = Join-Path $ManualWork "$ManualDb.bak"
        & sqlcmd -S '(localdb)\MSSQLLocalDB' -d master -E -I -b -Q "ALTER DATABASE [$ManualDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [$ManualDb] FROM DISK = N'$bak' WITH REPLACE; ALTER DATABASE [$ManualDb] SET MULTI_USER;" | Out-Null
        if ($LASTEXITCODE -ne 0) { throw 'restore failed' }
        $data = Join-Path $appRoot 'App_Data'
        if (Test-Path $data) { Remove-Item -Recurse -Force $data }
        Copy-Item -Recurse (Join-Path $ManualWork 'App_Data.snapshot') $data
        Write-Host 'reset to the state after setup'
    }
    'start' {
        if (Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue) { throw "Port $port is in use." }
        $dll = Join-Path $web 'bin\Debug\net8.0\TOP4HoneyChains.Web.dll'
        if (-not (Test-Path $dll)) { throw "Build the web project first: dotnet build $web" }
        $key = $null
        foreach ($line in (& dotnet user-secrets list --project $web 2>$null)) { if ($line -match '^Sms:PhonePageKey = (.+)$') { $key = $Matches[1].Trim() } }
        $settings = [ordered]@{
            'ASPNETCORE_ENVIRONMENT'               = 'Development'
            'ASPNETCORE_URLS'                      = "http://localhost:$port"
            'ASPNETCORE_CONTENTROOT'               = $appRoot
            'ASPNETCORE_WEBROOT'                   = (Join-Path $web 'wwwroot')
            'ConnectionStrings__DefaultConnection' = "Server=(localdb)\MSSQLLocalDB;Database=$ManualDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
            'Sms__Provider'                        = 'Fake'
            'Sms__AllowFake'                       = 'true'
            'Sms__DemoPanel'                       = 'false'
            'Sms__PhonePageKey'                    = $key
            'Features__OrgRoleModel'               = 'true'
            'Features__CustodyIntake'              = 'true'
            'Features__BeekeeperAccounts'          = 'true'
            'Features__ConsumerAccounts'           = 'true'
            'App__PublicBaseUrl'                   = $cfg['MANUAL_PUBLIC_URL']
            'Landing__DemoHoneyStoryQr'            = $cfg['MANUAL_STORY_QR']
            'Logging__LogLevel__Default'           = 'Warning'
            'Logging__LogLevel__Microsoft.EntityFrameworkCore' = 'Warning'
        }
        # A launcher file in the manual's work folder (outside the repository) sets the variables and starts the app. It is started
        # through Win32_Process, so the app inherits no handle of the caller: a script that waits for this one's output never hangs.
        $launcher = Join-Path $ManualWork 'start-app.cmd'
        $lines = @('@echo off', 'chcp 65001 >nul')
        foreach ($k in $settings.Keys) { $lines += "set `"$k=$($settings[$k])`"" }
        $lines += "cd /d `"$appRoot`""
        $lines += "dotnet `"$dll`" > `"$(Join-Path $ManualWork 'app.log')`" 2> `"$(Join-Path $ManualWork 'app.err.log')`""
        [System.IO.File]::WriteAllLines($launcher, $lines, (New-Object System.Text.UTF8Encoding($false)))
        $r = Invoke-CimMethod -ClassName Win32_Process -MethodName Create -Arguments @{ CommandLine = "cmd.exe /c `"$launcher`""; CurrentDirectory = $appRoot }
        if ($r.ReturnValue -ne 0) { throw "could not start the app ($($r.ReturnValue))" }
        Set-Content $pidFile $r.ProcessId
        for ($i = 0; $i -lt 90; $i++) {
            Start-Sleep -Milliseconds 500
            if (-not (Get-Process -Id $r.ProcessId -ErrorAction SilentlyContinue)) { throw "the app stopped; see $(Join-Path $ManualWork 'app.err.log')" }
            try { if ((Invoke-WebRequest "http://localhost:$port/" -UseBasicParsing -TimeoutSec 5).StatusCode -eq 200) { Write-Host "running on http://localhost:$port"; return } } catch {}
        }
        throw 'the app did not answer within 45 s'
    }
}
