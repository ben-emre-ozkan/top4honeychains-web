# Builds the fictional-only database of the user manuals, TOP4HoneyChainsDb_Manual, on LocalDB:
#   1. the schema of db/demo/schema.sql (the live schema), no rows;
#   2. the reference tables of the local database, without any link to a person (the same list as the demo database, D-86);
#   3. the platform organization;
#   4. the Şile cast and its history (tools/live-like/cast.json), recorded through the application by tools/live-like, with fictional
#      names for the two real organisations of the cast (the manual shows no real organisation; places may stay real);
#   5. a backup and a copy of the app's files, so that every screenshot run starts from the same state (reset.ps1).
# Nothing else is read from the local database. The app's files (lab reports, documents) go to the manual's own folder
# (%LOCALAPPDATA%\TOP4-manual\app), never to the web project's App_Data.
#
#   powershell -ExecutionPolicy Bypass -File docs\manual\capture\setup-db.ps1
#
# The practice password of the manual's accounts is made here and kept in capture\.env.local (untracked). Never printed.

$ErrorActionPreference = 'Stop'
$here = $PSScriptRoot
$repo = (Resolve-Path (Join-Path $here '..\..\..')).Path
$web = Join-Path $repo 'TOP4HONEYCHAINS\TOP4HoneyChains.Web'
$server = '(localdb)\MSSQLLocalDB'
$db = 'TOP4HoneyChainsDb_Manual'
$source = 'TOP4HoneyChainsDb'
$work = Join-Path $env:LOCALAPPDATA 'TOP4-manual'
$appRoot = Join-Path $work 'app'
$envFile = Join-Path $here '.env.local'

. (Join-Path $here 'manual-env.ps1')
$cfg = Read-ManualEnv $envFile
if (-not $cfg['MANUAL_PASSWORD']) {
    # 16 characters from an alphabet without look-alikes, plus a digit and a sign (the password rules: 10+, not common)
    $chars = 'abcdefghijkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789'
    $bytes = New-Object byte[] 16
    [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
    $cfg['MANUAL_PASSWORD'] = (-join ($bytes | ForEach-Object { $chars[$_ % $chars.Length] })) + '7!'
}
if (-not $cfg['MANUAL_PORT']) { $cfg['MANUAL_PORT'] = '5256' }
if (-not $cfg['MANUAL_PUBLIC_URL']) { $cfg['MANUAL_PUBLIC_URL'] = 'https://top4honeychains.intalalab.net' }
Write-ManualEnv $envFile $cfg

if (Get-NetTCPConnection -LocalPort ([int]$cfg['MANUAL_PORT']) -State Listen -ErrorAction SilentlyContinue) { throw "Port $($cfg['MANUAL_PORT']) is in use: stop the manual app first (stop-app.ps1)." }
if (Get-NetTCPConnection -LocalPort 5255 -State Listen -ErrorAction SilentlyContinue) { throw 'Port 5255 is in use: stop run-demo.ps1 first (the cast tool builds the web project).' }

function Invoke-Sql([string]$database, [string]$query) {
    $out = & sqlcmd -S $server -d $database -E -I -b -f 65001 -h -1 -W -Q "SET NOCOUNT ON; $query"
    if ($LASTEXITCODE -ne 0) { throw "sqlcmd failed: $out" }
    return $out
}

# ---- 1. a new, empty database with the live schema
Write-Host "1. database $db"
Invoke-Sql 'master' "IF DB_ID(N'$db') IS NOT NULL BEGIN ALTER DATABASE [$db] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$db]; END; CREATE DATABASE [$db]; ALTER DATABASE [$db] SET COMPATIBILITY_LEVEL = 150;" | Out-Null
& sqlcmd -S $server -d $db -E -I -b -f 65001 -i (Join-Path $repo 'db\demo\schema.sql') | Out-Null
if ($LASTEXITCODE -ne 0) { throw 'schema.sql failed' }

# the cast's own season is made by the cast tool; the local database has it too, so it is not copied
$castPeriod = (Get-Content (Join-Path $repo 'tools\live-like\cast.json') -Raw -Encoding UTF8 | ConvertFrom-Json).history.period.title.Replace("'", "''")

# ---- 2. reference tables, in dependency order; a person's id in them is not copied (TOP4HoneyChains.Web.Tests/Demo/DemoApp.cs)
Write-Host '2. reference tables'
$tables = @(
    @('__EFMigrationsHistory', $null, @()), @('AspNetRoles', $null, @()), @('AssignmentStatuses', $null, @()), @('HoneyTypes', $null, @()),
    @('RawHoneyProductionPeriods', "RawHoneyProductionPeriodTitle <> N'$castPeriod'", @()), @('BeekeeperEducationLevels', $null, @()), @('BeekeepingPurposeTypes', $null, @()),
    @('BeekeepingTypes', $null, @()), @('Capabilities', $null, @()), @('OrgRoles', $null, @()), @('CriticalControlPoints', $null, @()),
    @('CriticalControlPointParameterCategories', $null, @()), @('CriticalControlPointParameters', $null, @()),
    @('CriticalControlPointParameterLimits', $null, @()), @('CriticalControlPointParameterContents', $null, @()),
    @('CcpResponsibilities', $null, @('UpdatedByUserId')), @('CcpResponsibilityCapabilities', $null, @()), @('CcpResponsibilityRoles', $null, @()),
    @('Provinces', $null, @()), @('Districts', $null, @()), @('RolePermissions', $null, @('UpdatedByUserId')),
    @('ReferenceTranslations', $null, @('UpdatedByUserId')), @('Notices', 'OrgId IS NULL', @('PublishedByUserId'))
)
foreach ($t in $tables) {
    $name, $where, $nulls = $t
    $cols = @(Invoke-Sql $db "SELECT name FROM sys.columns WHERE object_id = OBJECT_ID(N'dbo.[$name]') AND is_computed = 0 ORDER BY column_id")
    $identity = [int](Invoke-Sql $db "SELECT COUNT(*) FROM sys.identity_columns WHERE object_id = OBJECT_ID(N'dbo.[$name]')")
    $list = ($cols | ForEach-Object { "[$_]" }) -join ', '
    $select = ($cols | ForEach-Object { if ($nulls -contains $_) { 'NULL' } else { "[$_]" } }) -join ', '
    $sql = "INSERT INTO dbo.[$name] ($list) SELECT $select FROM [$source].dbo.[$name]" + $(if ($where) { " WHERE $where" } else { '' }) + ';'
    if ($identity) { $sql = "SET IDENTITY_INSERT dbo.[$name] ON; $sql SET IDENTITY_INSERT dbo.[$name] OFF;" }
    Invoke-Sql $db $sql | Out-Null
}

# ---- 3. the platform organization (migration 004 creates it on the live database), with the live name
Write-Host '3. platform organization'
Invoke-Sql $db "INSERT dbo.Organizations (DisplayName, Status, IsPlatform, VerifiedAt, CreatedAt) SELECT TOP 1 DisplayName, N'Verified', 1, SYSUTCDATETIME(), SYSUTCDATETIME() FROM [$source].dbo.Organizations WHERE IsPlatform = 1 ORDER BY OrgId; INSERT dbo.OrgCapabilities (OrgId, Capability, IsAccredited, AddedAt) SELECT OrgId, N'PlatformAdmin', 0, SYSUTCDATETIME() FROM dbo.Organizations WHERE IsPlatform = 1;" | Out-Null

# the schema must match the local database (tables and columns only; no rows are compared)
$diff = Invoke-Sql $db "SELECT t.name + '.' + c.name FROM [$source].sys.columns c JOIN [$source].sys.tables t ON t.object_id = c.object_id WHERE t.name <> 'sysdiagrams' EXCEPT SELECT t.name + '.' + c.name FROM sys.columns c JOIN sys.tables t ON t.object_id = c.object_id"
if ($diff) { throw "The manual database lacks columns of the local database: $($diff -join ', '). Regenerate db/demo/schema.sql or apply the newer migrations." }

# ---- 4. the app's own folder: appsettings files and an empty App_Data
Write-Host "4. app folder $appRoot"
if (Test-Path $appRoot) { Remove-Item -Recurse -Force $appRoot }
New-Item -ItemType Directory -Force (Join-Path $appRoot 'App_Data') | Out-Null
Copy-Item (Join-Path $web 'appsettings.json'), (Join-Path $web 'appsettings.Development.json') $appRoot

# ---- 5. the cast, through the application
Write-Host '5. cast (tools/live-like)'
# The manual's copy of the cast: the directorate and the association get invented names (checked on the web on 2026-11-21: no
# organisation or place is called "Akçakovan"). The demo cast in tools/live-like/cast.json stays as it is.
$castText = Get-Content (Join-Path $repo 'tools\live-like\cast.json') -Raw -Encoding UTF8
foreach ($pair in (Get-Content (Join-Path $here 'fictional-names.json') -Raw -Encoding UTF8 | ConvertFrom-Json).PSObject.Properties) { $castText = $castText.Replace($pair.Name, $pair.Value) }
$manualCast = Join-Path $work 'cast.json'
[System.IO.File]::WriteAllText($manualCast, $castText, (New-Object System.Text.UTF8Encoding($false)))
$env:LIVE_LIKE_CAST = $manualCast
$env:LIVE_LIKE_DATABASE = $db
$env:LIVE_LIKE_CONTENT_ROOT = $appRoot
$env:LIVE_LIKE_LOGINS = Join-Path $work 'logins.txt'
$env:LIVE_LIKE_PASSWORD = $cfg['MANUAL_PASSWORD']
$env:LIVE_LIKE_BASE_URL = $cfg['MANUAL_PUBLIC_URL']
try {
    & dotnet build (Join-Path $repo 'tools\live-like') -v q -clp:ErrorsOnly | Out-Null
    if ($LASTEXITCODE -ne 0) { throw 'the cast tool does not build' }
    & dotnet run --no-build --project (Join-Path $repo 'tools\live-like') | Tee-Object -Variable castLog | Write-Host
    if ($LASTEXITCODE -ne 0) { throw 'the cast tool failed' }
} finally {
    'LIVE_LIKE_DATABASE', 'LIVE_LIKE_CONTENT_ROOT', 'LIVE_LIKE_LOGINS', 'LIVE_LIKE_PASSWORD', 'LIVE_LIKE_BASE_URL', 'LIVE_LIKE_CAST' | ForEach-Object { Remove-Item "env:$_" -ErrorAction SilentlyContinue }
}
$qr = ([regex]::Match(($castLog -join "`n"), '/honeystory/([0-9A-F-]{36})')).Groups[1].Value
if (-not $qr) { throw 'no Honey Story address in the cast log' }
$cfg['MANUAL_STORY_QR'] = $qr
Write-ManualEnv $envFile $cfg

# no account or person of the local database came along (counts only; the cast's own names are allowed)
$castUsers = (Get-Content (Join-Path $repo 'tools\live-like\cast.json') -Raw -Encoding UTF8 | ConvertFrom-Json).people.user
$known = ($castUsers + 'seda.gunes' | ForEach-Object { "N'$_'" }) -join ','
$leak = [int](Invoke-Sql $db "SELECT COUNT(*) FROM dbo.AspNetUsers m WHERE m.UserName NOT IN ($known) AND m.UserName NOT LIKE 'TR-BEE-%' AND EXISTS (SELECT 1 FROM [$source].dbo.AspNetUsers s WHERE s.UserName = m.UserName)")
if ($leak) { throw "$leak account(s) of the local database found in the manual database" }

# ---- 5b. the sample production period in the other four languages (as an administrator would translate it)
& sqlcmd -S $server -d $db -E -I -b -f 65001 -i (Join-Path $here 'period-names.sql') | Write-Host
if ($LASTEXITCODE -ne 0) { throw 'period-names.sql failed' }

# ---- 6. the state every screenshot run starts from
Write-Host '6. backup'
$bak = Join-Path $work "$db.bak"
Invoke-Sql 'master' "BACKUP DATABASE [$db] TO DISK = N'$bak' WITH INIT, COPY_ONLY" | Out-Null
$snap = Join-Path $work 'App_Data.snapshot'
if (Test-Path $snap) { Remove-Item -Recurse -Force $snap }
Copy-Item -Recurse (Join-Path $appRoot 'App_Data') $snap
Write-Host "done: $db, backup $bak, logins $(Join-Path $work 'logins.txt')"
