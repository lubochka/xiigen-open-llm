$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Reflection.Metadata
$e = 'C:\Projects\llm_mvp_core\artifacts\fbe\CODEX-NOV12-RECHECK\attempt-0001'
$repoRoot = 'C:\Projects\llm_mvp_core'
$dll = Join-Path $e 'native-build\bin\Tests\release\LlmFromScratch.Tests.dll'
$pdb = [System.IO.Path]::ChangeExtension($dll, '.pdb')
$pre = Get-Content -LiteralPath (Join-Path $e 'codex-source-pre-build.json') -Raw | ConvertFrom-Json
$post = Get-Content -LiteralPath (Join-Path $e 'codex-source-post-test.json') -Raw | ConvertFrom-Json
$peStream = [System.IO.File]::OpenRead($dll)
$pdbStream = [System.IO.File]::OpenRead($pdb)
try {
 $pe = [System.Reflection.PortableExecutable.PEReader]::new($peStream, [System.Reflection.PortableExecutable.PEStreamOptions]::LeaveOpen, 0)
 $provider = [System.Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream($pdbStream, [System.Reflection.Metadata.MetadataStreamOptions]::LeaveOpen, 0)
 $reader = $provider.GetMetadataReader([System.Reflection.Metadata.MetadataReaderOptions]::Default, $null)
 [byte[]]$pdbId = $reader.DebugMetadataHeader.Id
 $pdbGuid = [Guid]::new([byte[]]$pdbId[0..15])
 $pdbStamp = [BitConverter]::ToUInt32($pdbId, 16)
 $codeView = @()
 foreach ($entry in $pe.ReadDebugDirectory()) {
  if ($entry.Type -eq [System.Reflection.PortableExecutable.DebugDirectoryEntryType]::CodeView) {
   $cv = $pe.ReadCodeViewDebugDirectoryData($entry)
   $codeView += [pscustomobject]@{Guid=$cv.Guid.ToString();Age=$cv.Age;Path=$cv.Path;Stamp=$entry.Stamp;PortablePdbMatches=($cv.Guid -eq $pdbGuid -and $entry.Stamp -eq $pdbStamp -and $cv.Age -eq 1)}
  }
 }
 $targets = @('Tests/ExperimentFixes/ExperimentAttemptOwnershipTests.cs','Tests/ExperimentFixes/ExperimentAttemptScope.cs')
 $documents = @()
 foreach ($handle in $reader.Documents) {
  $doc = $reader.GetDocument($handle)
  $name = $reader.GetString($doc.Name).Replace('\','/')
  foreach ($target in $targets) {
   if ($name.EndsWith('/'+$target, [StringComparison]::OrdinalIgnoreCase)) {
    $algorithm = $reader.GetGuid($doc.HashAlgorithm)
    $compilerSha = [Convert]::ToHexString($reader.GetBlobBytes($doc.Hash)).ToLowerInvariant()
    $currentFile = Join-Path $repoRoot $target
    $currentSha = (Get-FileHash -LiteralPath $currentFile -Algorithm SHA256).Hash.ToLowerInvariant()
    $before = @($pre.inputs | Where-Object file -eq $target)[0].sha256
    $after = @($post.inputs | Where-Object file -eq $target)[0].sha256
    $documents += [pscustomobject]@{File=$target;DocumentPath=$name;AlgorithmGuid=$algorithm.ToString();AlgorithmIsSHA256=($algorithm.ToString() -eq '8829d00f-11b8-4213-878b-770e8597ac16');CompilerSha256=$compilerSha;PreBuildSha256=$before;PostTestSha256=$after;CurrentSha256=$currentSha;MatchesPreBuild=($compilerSha -eq $before);MatchesPostTest=($compilerSha -eq $after);MatchesCurrent=($compilerSha -eq $currentSha)}
   }
  }
 }
 $result = [pscustomobject]@{Dll=$dll;DllSha256=(Get-FileHash -LiteralPath $dll -Algorithm SHA256).Hash.ToLowerInvariant();Pdb=$pdb;PdbSha256=(Get-FileHash -LiteralPath $pdb -Algorithm SHA256).Hash.ToLowerInvariant();PortablePdbId=[Convert]::ToHexString($pdbId).ToLowerInvariant();PortablePdbGuid=$pdbGuid.ToString();PortablePdbStamp=$pdbStamp;CodeView=$codeView;Documents=$documents}
 $json = ($result | ConvertTo-Json -Depth 8).Replace("`r`n","`n")
 [System.IO.File]::WriteAllText((Join-Path $e 'codex-pdb-source-proof.json'), $json+"`n", [System.Text.UTF8Encoding]::new($false))
 Write-Output $json
 if ($documents.Count -ne 2 -or @($documents | Where-Object { -not $_.AlgorithmIsSHA256 -or -not $_.MatchesCurrent }).Count -gt 0 -or @($codeView | Where-Object PortablePdbMatches).Count -ne 1) { exit 1 }
} finally {
 if ($null -ne $provider) { $provider.Dispose() }
 if ($null -ne $pe) { $pe.Dispose() }
 $peStream.Dispose()
 $pdbStream.Dispose()
}
