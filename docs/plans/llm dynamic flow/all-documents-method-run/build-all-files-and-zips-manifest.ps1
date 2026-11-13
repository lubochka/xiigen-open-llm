$ErrorActionPreference = 'Continue'

$Root = 'C:\Users\lubao\Documents\xiigen'
$Run = 'C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\all-documents-method-run'
$OutDir = Join-Path $Run 'all-files-including-zips-compact'
$Manifest = Join-Path $OutDir 'all-files-and-zip-entries-manifest.csv'
$ExcludePrefix = $Run

New-Item -ItemType Directory -Force -Path $OutDir | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem

$textExtensions = @(
  '.md','.txt','.json','.yaml','.yml','.xml','.svg','.csv','.tsv','.html','.htm','.css',
  '.scss','.js','.mjs','.cjs','.ts','.tsx','.cs','.py','.php','.rs','.java','.go','.rb',
  '.sh','.ps1','.sql','.csproj','.sln','.gitignore','.env','.config','.toml','.ini','.lock'
)

function CsvEscape([string]$s) {
  if ($null -eq $s) { return '' }
  $v = $s -replace '"','""'
  if ($v -match '[,"\r\n]') { return '"' + $v + '"' }
  return $v
}

function RelPath([string]$full) {
  if ($full.StartsWith($Root, [System.StringComparison]::OrdinalIgnoreCase)) {
    return $full.Substring($Root.Length).TrimStart('\')
  }
  return $full
}

function KindFor([string]$ext, [string]$path) {
  $e = $ext.ToLowerInvariant()
  if ($e -eq '.zip') { return 'zip archive' }
  if ($textExtensions -contains $e) { return 'readable text' }
  if (@('.docx','.pdf') -contains $e) { return 'document binary' }
  if (@('.png','.jpg','.jpeg','.gif','.webp','.ico','.bmp') -contains $e) { return 'image binary' }
  if (@('.dll','.exe','.pdb','.ttf','.woff','.woff2') -contains $e) { return 'runtime binary' }
  if ([string]::IsNullOrWhiteSpace($e)) { return 'extensionless file' }
  return 'other file'
}

function CategoryFor([string]$kind, [string]$path) {
  $p = $path.ToLowerInvariant()
  if ($kind -eq 'zip archive') { return 'archive container' }
  if ($p -match 'session|history|old|older|worktree|\.claude') { return 'session-history-or-version' }
  if ($p -match 'node_modules|vendor|dist|build|bin|obj') { return 'tooling-or-build-copy' }
  if ($kind -eq 'readable text') { return 'readable source file' }
  if ($kind -match 'binary') { return 'binary asset' }
  return 'general file'
}

function SamplePhysical([string]$full, [string]$ext) {
  if (-not ($textExtensions -contains $ext.ToLowerInvariant())) { return '' }
  try {
    $fs = [System.IO.File]::Open($full, [System.IO.FileMode]::Open, [System.IO.FileAccess]::Read, [System.IO.FileShare]::ReadWrite)
    try {
      $buffer = New-Object byte[] ([Math]::Min(32768, [int]$fs.Length))
      $read = $fs.Read($buffer, 0, $buffer.Length)
      $text = [System.Text.Encoding]::UTF8.GetString($buffer, 0, $read)
      return (($text -split "\r?\n") | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 } | Select-Object -First 1)
    } finally {
      $fs.Dispose()
    }
  } catch {
    return ''
  }
}

function SampleZipEntry($archive, $entry, [string]$ext) {
  if (-not ($textExtensions -contains $ext.ToLowerInvariant())) { return '' }
  if ($entry.Length -gt 2097152) { return '' }
  try {
    $stream = $entry.Open()
    try {
      $buffer = New-Object byte[] ([Math]::Min(32768, [int]$entry.Length))
      $read = $stream.Read($buffer, 0, $buffer.Length)
      $text = [System.Text.Encoding]::UTF8.GetString($buffer, 0, $read)
      return (($text -split "\r?\n") | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 } | Select-Object -First 1)
    } finally {
      $stream.Dispose()
    }
  } catch {
    return ''
  }
}

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$writer = New-Object System.IO.StreamWriter($Manifest, $false, $utf8NoBom)
$writer.WriteLine('SourceId,SourceKind,PhysicalFullName,RelativePath,ArchiveFullName,ArchiveRelativePath,ZipEntryPath,Extension,Length,LastWriteTime,Category,ReadableKind,ReadStatus,TextSample')

$sourceId = 0
$physicalCount = 0
$zipEntryCount = 0
$zipErrorCount = 0

$files = Get-ChildItem -LiteralPath $Root -Recurse -File -Force -ErrorAction SilentlyContinue |
  Where-Object { -not ($_.FullName.StartsWith($ExcludePrefix, [System.StringComparison]::OrdinalIgnoreCase)) }

foreach ($file in $files) {
  $sourceId++
  $physicalCount++
  $rel = RelPath $file.FullName
  $ext = $file.Extension
  $kind = KindFor $ext $file.FullName
  $cat = CategoryFor $kind $file.FullName
  $sample = SamplePhysical $file.FullName $ext
  $writer.WriteLine((@(
    $sourceId,
    'physical-file',
    (CsvEscape $file.FullName),
    (CsvEscape $rel),
    '',
    '',
    '',
    (CsvEscape $ext),
    $file.Length,
    (CsvEscape $file.LastWriteTime.ToString('o')),
    (CsvEscape $cat),
    (CsvEscape $kind),
    'ok',
    (CsvEscape $sample)
  ) -join ','))

  if ($ext -ieq '.zip') {
    try {
      $archive = [System.IO.Compression.ZipFile]::OpenRead($file.FullName)
      try {
        foreach ($entry in $archive.Entries) {
          if ([string]::IsNullOrWhiteSpace($entry.Name)) { continue }
          $sourceId++
          $zipEntryCount++
          $entryExt = [System.IO.Path]::GetExtension($entry.FullName)
          $entryKind = KindFor $entryExt $entry.FullName
          $entryCat = CategoryFor $entryKind ($file.FullName + '!' + $entry.FullName)
          $sample = SampleZipEntry $archive $entry $entryExt
          $writer.WriteLine((@(
            $sourceId,
            'zip-entry',
            (CsvEscape ($file.FullName + '!' + $entry.FullName)),
            (CsvEscape ($rel + '!' + $entry.FullName)),
            (CsvEscape $file.FullName),
            (CsvEscape $rel),
            (CsvEscape $entry.FullName),
            (CsvEscape $entryExt),
            $entry.Length,
            (CsvEscape $entry.LastWriteTime.ToString('o')),
            (CsvEscape $entryCat),
            (CsvEscape $entryKind),
            'ok',
            (CsvEscape $sample)
          ) -join ','))
        }
      } finally {
        $archive.Dispose()
      }
    } catch {
      $zipErrorCount++
    }
  }
}

$writer.Dispose()

$summary = [ordered]@{
  physicalFiles = $physicalCount
  zipArchives = ($files | Where-Object Extension -ieq '.zip').Count
  zipEntries = $zipEntryCount
  zipOpenErrors = $zipErrorCount
  totalSources = $sourceId
  manifest = $Manifest
}

$summary | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutDir 'manifest-summary.json') -Encoding UTF8
$summary | ConvertTo-Json -Depth 4
