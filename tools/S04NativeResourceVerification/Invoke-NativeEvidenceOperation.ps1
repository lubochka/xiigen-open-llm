# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD; AGPL-3.0-only.
# Finite resource/evidence operation executor, always launched in the existing owned Job.
[CmdletBinding()]
param([string]$RequestPath,[string]$MetadataPipe,[string]$OutputPath,[string]$EvidenceRoot,[switch]$CreateNew)
$ErrorActionPreference='Stop'
Set-StrictMode -Version 3
$utf8=[Text.UTF8Encoding]::new($false,$true)
$script:nativeHelperRoot=$PSScriptRoot
if($RequestPath){[Console]::Error.WriteLine('S04_OPERATION '+(@{boundary='helper-enter';utc=[DateTimeOffset]::UtcNow.ToString('o');pid=$PID}|ConvertTo-Json -Compress))}
if($MetadataPipe){
    # Controller bounds this child in a Job. Synchronous write/flush cannot strand its host.
    $root=[IO.Path]::GetFullPath($EvidenceRoot).TrimEnd('\')+'\';$path=[IO.Path]::GetFullPath($OutputPath)
    if(-not $path.StartsWith($root,[StringComparison]::OrdinalIgnoreCase)){throw 'Metadata path escapes owned evidence root'}
    for($cursor=$path;$cursor;$cursor=[IO.Path]::GetDirectoryName($cursor)){if((Test-Path -LiteralPath $cursor) -and ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Metadata reparse path'}}
    $pipe=[IO.Pipes.NamedPipeClientStream]::new('.',$MetadataPipe,[IO.Pipes.PipeDirection]::In)
    $output=$null
    try{$pipe.Connect(30000);$output=[IO.File]::Open($path,$(if($CreateNew){[IO.FileMode]::CreateNew}else{[IO.FileMode]::Create}),[IO.FileAccess]::Write,[IO.FileShare]::Read)
        $buffer=[byte[]]::new(65536);$length=0
        while(($count=$pipe.Read($buffer,0,$buffer.Length)) -gt 0){$length+=$count;if($length -gt 16MB){throw 'Metadata size bound'};$output.Write($buffer,0,$count)}
        $output.Flush($true)
    }finally{if($output){$output.Dispose()};$pipe.Dispose()};exit 0
}
if(-not $RequestPath){throw 'Operation request path required'}
if((Get-Item -LiteralPath $RequestPath).Length -gt 16MB){throw 'Operation request exceeds16MiB'}
$request=Get-Content -LiteralPath $RequestPath -Raw | ConvertFrom-Json -AsHashtable -Depth 100
[Console]::Error.WriteLine('S04_OPERATION '+(@{boundary='request-read';utc=[DateTimeOffset]::UtcNow.ToString('o');pid=$PID;operation=$request.operation}|ConvertTo-Json -Compress))
$requestHash=(Get-FileHash -LiteralPath $RequestPath).Hash.ToLowerInvariant()
$helperHash=(Get-FileHash -LiteralPath $PSCommandPath).Hash.ToLowerInvariant()
if($helperHash -cne $request.helperSha256){throw 'Operation helper hash differs from request'}
$allowed=@('Capture-Sources','Capture-Build','Fixture-Inventory','Resource-Hashes','Cleanup-Owned','Seal-OwnedLeaf',
    'Promote-OwnedLeaf','Invoke-DownloadStream','Expand-OwnedMember','Write-OwnedManifest','Capture-SourceAfter','Complete-EvidenceValidation')
if($request.operation -cnotin $allowed){throw 'Unknown evidence operation'}
$Workspace=$request.workspace;$EvidenceDirectory=$request.evidenceDirectory;$RunnerTemp=$request.runnerTemp
$CandidateSha=$request.candidate;$WorkflowSha=$request.workflow;$e=$request.e
$ownedLeaves=[Collections.Generic.List[object]]::new();foreach($row in $request.ownedLeaves){$ownedLeaves.Add($row)}
$ownedDirectories=[Collections.Generic.List[object]]::new();foreach($row in $request.ownedDirectories){$ownedDirectories.Add($row)}
$resources=@($request.resources)
$sourceMap=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
foreach($row in $request.sourceRows){$sourceMap.Add((Join-Path $Workspace $row.path),$row)}
$script:InsideEvidenceOperation=$true
$script:OwnershipJournal=$RequestPath+'.ownership.json'
$script:OperationDeadline=if($request.deadlineUtc -is [DateTime]){[DateTimeOffset]::new($request.deadlineUtc.ToUniversalTime())}else{[DateTimeOffset]::Parse($request.deadlineUtc)}
$tokens=$null;$errors=$null
$ast=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Invoke-NativeResourceVerification.ps1'),[ref]$tokens,[ref]$errors)
if($errors.Count){throw 'Operation runner AST invalid'}
# Use the actual production functions; never execute its top-level/native lifecycle.
foreach($definition in $ast.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst]},$true)){
    . ([scriptblock]::Create($definition.Extent.Text))
}
. (Join-Path $PSScriptRoot 'Test-NativeResourceEvidence.ps1')
Write-OperationProgress 'helpers-initialize'
Initialize-NativeHelpers
Write-OperationProgress 'helpers-ready'
$requestIdentity=[S04NativeCustody]::Identity($RequestPath)
if($request.sdkRoot){
    [void][Reflection.Assembly]::LoadFrom((Join-Path $request.sdkRoot 'Microsoft.Build.Framework.dll'))
    $buildAssembly=[Reflection.Assembly]::LoadFrom((Join-Path $request.sdkRoot 'Microsoft.Build.dll'))
    $script:replayType=$buildAssembly.GetType('Microsoft.Build.Logging.BinaryLogReplayEventSource')
}
$failure=$null;$value=@()
try {
    if([DateTimeOffset]::UtcNow -ge $script:OperationDeadline){throw 'Operation deadline expired before effects'}
    $operationArguments=@($request.arguments)
    $value=@(& $request.operation @operationArguments)
}catch{$failure=$_.Exception.ToString()}
finally {
    Write-OperationProgress 'terminal-write'
    # Parent imports only this terminal receipt. Missing receipt stays unknown/fail-closed.
    $state=[ordered]@{operation=$request.operation;candidate=$request.candidate;deadlineUtc=$request.deadlineUtc;requestSha256=$requestHash;requestIdentity=$requestIdentity;helperSha256=$helperHash;error=$failure;value=$value;e=$e;ownedLeaves=@($ownedLeaves);
        ownedDirectories=@($ownedDirectories);sourceRows=@($sourceMap.Values);endUtc=[DateTimeOffset]::UtcNow.ToString('o')}
    Write-NewJson ($RequestPath+'.result.json') $state
    $compact=[ordered]@{operation=$state.operation;candidate=$state.candidate;deadlineUtc=$state.deadlineUtc;endUtc=$state.endUtc;requestSha256=$requestHash;requestIdentity=$requestIdentity;helperSha256=$helperHash;error=$failure}
    Write-NewJson ($RequestPath.Replace('.request.json','.receipt.json')) $compact
    Write-OperationProgress 'terminal-complete'
}
if($failure){[Console]::Error.WriteLine($failure);exit 1}
