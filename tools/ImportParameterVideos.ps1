<#
.SYNOPSIS
    Copies the parameter videos of the Honey Story (D-108) into the folder the application serves them from.

.DESCRIPTION
    The application plays one short video per laboratory parameter and language in the value sheet of the Honey Story. The videos
    are not in git; they live in a folder on the server (setting Videos:Root, default <application folder>\App_Data\parameter-videos)
    as <lang>\<key>.mp4. This script copies them there and prints, per language, which parameters have no video (the page then falls
    back to English, then Turkish, and without any file shows the text only).

    The source may be
      - a folder already in the final layout (<lang>\<key>.mp4), such as the Videos folder of the installation package, or
      - the consumer mini app's video folder (TOP4Honey-Consumer-MiniApp\assets\videos: English files at the top, Turkish in tr\,
        with the mini app's own file names).

    Keep the key list in step with Services/ParameterVideos.cs.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File ImportParameterVideos.ps1 -Source D:\Paket\Videos -Target C:\inetpub\TOP4HoneyChains\App_Data\parameter-videos

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tools\ImportParameterVideos.ps1 -Source "C:\...\TOP4Honey-Consumer-MiniApp\assets\videos" -Target .\Videos
#>
param(
    [Parameter(Mandatory = $true)][string]$Source,
    [Parameter(Mandatory = $true)][string]$Target,
    # write the missing list to this file as well
    [string]$MissingReport
)

$ErrorActionPreference = 'Stop'

$languages = @('tr', 'en', 'es', 'pl', 'lv')

# key -> the mini app's file names (English at the top of its video folder, Turkish in tr\)
$keys = [ordered]@{
    'moisture'                = @{ en = 'moisture.mp4';                tr = 'nem.mp4' }
    'hmf'                     = @{ en = 'hmf.mp4';                     tr = 'hmf.mp4' }
    'diastase'                = @{ en = 'diastaz.mp4';                 tr = 'diastaz.mp4' }
    'c4-sugar'                = @{ en = 'sugar-balance.mp4';           tr = 'balda-seker-dengesi.mp4' }
    'sucrose'                 = @{ en = 'sucrose.mp4';                 tr = 'sakkaroz.mp4' }
    'fructose-glucose'        = @{ en = 'fructose-glucose.mp4';        tr = 'fruktoz-glikoz.mp4' }
    'maltose'                 = @{ en = 'maltose.mp4';                 tr = 'maltose.mp4' }
    'pesticides'              = @{ en = 'pesticides.mp4';              tr = 'pesticides.mp4' }
    'antibiotics'             = @{ en = 'antibiotics.mp4';             tr = 'antibiotics.mp4' }
    'electrical-conductivity' = @{ en = 'electrical-conductivity.mp4'; tr = 'elektriksel-iletkenlik.mp4' }
    'proline'                 = @{ en = 'proline.mp4';                 tr = 'prolin.mp4' }
    'ph'                      = @{ en = 'ph.mp4';                      tr = 'ph.mp4' }
    'free-acidity'            = @{ en = 'free-acidity.mp4';            tr = 'serbest-asitlik.mp4' }
    'insoluble-matter'        = @{ en = 'insoluble-matter.mp4';        tr = 'balda-bulunan-cozunmeyen-maddeler.mp4' }
    'naphthalene'             = @{ en = 'naphthalene.mp4';             tr = 'naftalin.mp4' }
}

if (-not (Test-Path -LiteralPath $Source -PathType Container)) {
    throw "Source folder not found: $Source"
}
$Source = (Resolve-Path -LiteralPath $Source).Path
New-Item -ItemType Directory -Force -Path $Target | Out-Null
$Target = (Resolve-Path -LiteralPath $Target).Path

# the file of one key and language in the source: the final layout first, then the mini app's names
function Find-Video([string]$lang, [string]$key) {
    $final = Join-Path (Join-Path $Source $lang) "$key.mp4"
    if (Test-Path -LiteralPath $final -PathType Leaf) { return $final }
    $names = $keys[$key]
    if ($lang -eq 'en' -and $names.en) {
        $p = Join-Path $Source $names.en
        if (Test-Path -LiteralPath $p -PathType Leaf) { return $p }
    }
    if ($lang -eq 'tr' -and $names.tr) {
        $p = Join-Path (Join-Path $Source 'tr') $names.tr
        if (Test-Path -LiteralPath $p -PathType Leaf) { return $p }
    }
    return $null
}

$copied = 0
$missing = New-Object System.Collections.Generic.List[string]
foreach ($lang in $languages) {
    $absent = @()
    foreach ($key in $keys.Keys) {
        $from = Find-Video $lang $key
        if ($null -eq $from) { $absent += $key; continue }
        $dir = Join-Path $Target $lang
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
        $to = Join-Path $dir "$key.mp4"
        if ((Resolve-Path -LiteralPath $from).Path -ne $to) {
            Copy-Item -LiteralPath $from -Destination $to -Force
        }
        $copied++
    }
    if ($absent.Count -eq 0) {
        $line = "${lang}: all $($keys.Count) videos"
    } elseif ($absent.Count -eq $keys.Count) {
        $line = "${lang}: no videos (the page shows the English video, else the Turkish one)"
    } else {
        $line = "${lang}: missing $($absent.Count) of $($keys.Count): $($absent -join ', ')"
    }
    $missing.Add($line)
}

Write-Output "Copied $copied video(s) to $Target"
$missing | ForEach-Object { Write-Output $_ }
Write-Output "No video for these parameters in any language (the sheet shows the text only): Sensory Analysis"
if ($MissingReport) {
    ($missing + "No video in any language (text only): Sensory Analysis") | Set-Content -LiteralPath $MissingReport -Encoding UTF8
}
