$ErrorActionPreference = 'Continue'

$Root = 'C:\Users\lubao\Documents\xiigen'
$Run = 'C:\Users\lubao\Documents\xiigen\llm mvp\llm dynamic flow\all-documents-method-run'
$OutDir = Join-Path $Run 'all-files-including-zips-compact'
$BaseManifest = Join-Path $OutDir 'all-files-and-zip-entries-manifest-sanitized.csv'
$CombinedManifest = Join-Path $OutDir 'all-files-and-zip-entries-manifest-with-nested.csv'

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

function KindFor([string]$ext) {
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

function SampleEntry($entry, [string]$ext) {
  if (-not ($textExtensions -contains $ext.ToLowerInvariant())) { return '' }
  if ($entry.Length -gt 2097152) { return '' }
  try {
    $stream = $entry.Open()
    try {
      $buffer = New-Object byte[] ([Math]::Min(32768, [int]$entry.Length))
      $read = $stream.Read($buffer, 0, $buffer.Length)
      $text = [System.Text.Encoding]::UTF8.GetString($buffer, 0, $read)
      $line = (($text -split "\r?\n") | ForEach-Object { $_.Trim() } | Where-Object { $_.Length -gt 0 } | Select-Object -First 1)
      return (($line -replace "`r",' ') -replace "`n",' ' -replace '\s+',' ').Trim()
    } finally {
      $stream.Dispose()
    }
  } catch {
    return ''
  }
}

function ReadEntryBytes($entry) {
  $stream = $entry.Open()
  try {
    $ms = New-Object System.IO.MemoryStream
    try {
      $stream.CopyTo($ms)
      return $ms.ToArray()
    } finally {
      $ms.Dispose()
    }
  } finally {
    $stream.Dispose()
  }
}

$rows = Import-Csv -LiteralPath $BaseManifest
Copy-Item -LiteralPath $BaseManifest -Destination $CombinedManifest -Force
$sourceId = ($rows | ForEach-Object { [int]$_.SourceId } | Measure-Object -Maximum).Maximum
$nestedCandidates = $rows | Where-Object { $_.SourceKind -eq 'zip-entry' -and $_.Extension -ieq '.zip' }

$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
$writer = New-Object System.IO.StreamWriter($CombinedManifest, $true, $utf8NoBom)
$nestedEntries = 0
$nestedZipContainers = 0
$nestedOpenErrors = 0
$maxDepthSeen = 0

function ProcessZipBytes([byte[]]$bytes, [string]$chainFull, [string]$chainRel, [int]$depth) {
  if ($depth -gt $script:maxDepthSeen) { $script:maxDepthSeen = $depth }
  try {
    $ms = New-Object System.IO.MemoryStream(,$bytes)
    try {
      $archive = New-Object System.IO.Compression.ZipArchive($ms, [System.IO.Compression.ZipArchiveMode]::Read, $false)
      try {
        foreach ($entry in $archive.Entries) {
          if ([string]::IsNullOrWhiteSpace($entry.Name)) { continue }
          $script:sourceId++
          $script:nestedEntries++
          $entryExt = [System.IO.Path]::GetExtension($entry.FullName)
          $kind = KindFor $entryExt
          $cat = CategoryFor $kind ($chainFull + '!' + $entry.FullName)
          $sample = SampleEntry $entry $entryExt
          $writer.WriteLine((@(
            $script:sourceId,
            'nested-zip-entry',
            (CsvEscape ($chainFull + '!' + $entry.FullName)),
            (CsvEscape ($chainRel + '!' + $entry.FullName)),
            (CsvEscape $chainFull),
            (CsvEscape $chainRel),
            (CsvEscape $entry.FullName),
            (CsvEscape $entryExt),
            $entry.Length,
            (CsvEscape $entry.LastWriteTime.ToString('o')),
            (CsvEscape $cat),
            (CsvEscape $kind),
            'ok',
            (CsvEscape $sample)
          ) -join ','))

          if ($entryExt -ieq '.zip' -and $entry.Length -gt 0 -and $depth -lt 8) {
            $script:nestedZipContainers++
            try {
              $childBytes = ReadEntryBytes $entry
              ProcessZipBytes $childBytes ($chainFull + '!' + $entry.FullName) ($chainRel + '!' + $entry.FullName) ($depth + 1)
            } catch {
              $script:nestedOpenErrors++
            }
          }
        }
      } finally {
        $archive.Dispose()
      }
    } finally {
      $ms.Dispose()
    }
  } catch {
    $script:nestedOpenErrors++
  }
}

foreach ($candidate in $nestedCandidates) {
  try {
    $outer = [System.IO.Compression.ZipFile]::OpenRead($candidate.ArchiveFullName)
    try {
      $entry = $outer.GetEntry($candidate.ZipEntryPath)
      if ($null -eq $entry) {
        $nestedOpenErrors++
        continue
      }
      $bytes = ReadEntryBytes $entry
      ProcessZipBytes $bytes $candidate.PhysicalFullName $candidate.RelativePath 1
    } finally {
      $outer.Dispose()
    }
  } catch {
    $nestedOpenErrors++
  }
}

$writer.Dispose()

$summary = [ordered]@{
  baseSources = $rows.Count
  nestedZipCandidates = $nestedCandidates.Count
  nestedEntriesAdded = $nestedEntries
  nestedZipContainersFoundInsideNested = $nestedZipContainers
  nestedZipOpenErrors = $nestedOpenErrors
  maxNestedDepthSeen = $maxDepthSeen
  combinedManifest = $CombinedManifest
  combinedSources = $rows.Count + $nestedEntries
}

$summary | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $OutDir 'nested-zip-summary.json') -Encoding UTF8
$summary | ConvertTo-Json -Depth 4
