# Shared by the manual's scripts: capture\.env.local (untracked) holds the practice password and the run settings, KEY=value per line.

function Read-ManualEnv([string]$file) {
    $cfg = [ordered]@{}
    if (Test-Path $file) {
        foreach ($line in Get-Content $file -Encoding UTF8) {
            if ($line -match '^\s*([A-Z_]+)=(.*)$') { $cfg[$Matches[1]] = $Matches[2].Trim() }
        }
    }
    return $cfg
}

function Write-ManualEnv([string]$file, $cfg) {
    $lines = @('# TOP4HoneyChains user manual: local settings of the screenshot runs. Untracked; never commit.')
    foreach ($k in $cfg.Keys) { $lines += "$k=$($cfg[$k])" }
    [System.IO.File]::WriteAllLines($file, $lines, (New-Object System.Text.UTF8Encoding($false)))
}

$ManualDb = 'TOP4HoneyChainsDb_Manual'
$ManualWork = Join-Path $env:LOCALAPPDATA 'TOP4-manual'
