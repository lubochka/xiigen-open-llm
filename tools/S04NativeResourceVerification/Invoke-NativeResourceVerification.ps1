# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD; AGPL-3.0-only.
# Explicit public-resource CI lane. Deterministic helper, no semantic authority.
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$CandidateSha,
    [Parameter(Mandatory)][string]$WorkflowSha,
    [Parameter(Mandatory)][string]$Workspace,
    [Parameter(Mandatory)][string]$RunnerTemp,
    [Parameter(Mandatory)][string]$EvidenceDirectory
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version 3
$utf8 = [Text.UTF8Encoding]::new($false, $true)
$script:nativeHelperRoot=$PSScriptRoot
$Workspace = [IO.Path]::GetFullPath($Workspace)
$RunnerTemp = [IO.Path]::GetFullPath($RunnerTemp)
$EvidenceDirectory = [IO.Path]::GetFullPath($EvidenceDirectory)
. (Join-Path $PSScriptRoot 'Test-NativeResourceEvidence.ps1')
$timingPolicy=Get-NativeTimingPolicy
$runStart=[DateTimeOffset]::UtcNow
$regularSeconds=$timingPolicy.regularSeconds
$cleanupReserveSeconds=$timingPolicy.phases.restoration+$timingPolicy.phases.closure+$timingPolicy.phases.validation
$overallDeadline=$runStart.AddSeconds($regularSeconds+$cleanupReserveSeconds)
$script:phaseDeadline=$null;$script:phaseName=$null;$script:InsideEvidenceOperation=$false;$script:operationSequence=0;$script:sdkRoot=$null
$script:inTimedPhase=$false;$script:writingPhaseCheckpoint=$false
$e = [ordered]@{ schema=1; gate='S04.04A-NATIVE-RESOURCE'; candidate=$CandidateSha; workflow=$WorkflowSha; head=$null;
    runId=$env:GITHUB_RUN_ID; runAttempt=$env:GITHUB_RUN_ATTEMPT; workflowRef=$env:GITHUB_WORKFLOW_REF; runner=$env:RUNNER_NAME;
    workspace=$Workspace; sdk=$null; sources=@(); commands=@(); builds=@(); discovery=@(); resources=@(); downloads=@(); failures=@();
    regression=@{exitCode=$null;passed=0}; disk=@{availableBytes=0;requiredBytes=0}; restoration=@{completed=$false;nativeCount=$null};
    cleanup=@{completed=$false;failures=@();leaves=@();directories=@();activeProcesses=$null}; fixture=@{beforeDigest=$null;afterDigest=$null};
    commandBinarySha256=$null; commandPdbSha256=$null;
    timing=@{policy=$timingPolicy;startUtc=$runStart.ToString('o');regularDeadlineUtc=$runStart.AddSeconds($regularSeconds).ToString('o');
        restorationDeadlineUtc=$runStart.AddSeconds($regularSeconds+$timingPolicy.phases.restoration).ToString('o');closureDeadlineUtc=$runStart.AddSeconds($regularSeconds+$timingPolicy.phases.restoration+$timingPolicy.phases.closure).ToString('o');
        overallDeadlineUtc=$overallDeadline.ToString('o');endUtc=$null;phases=@();operations=@();admissions=@();metadataWrites=@();unknownEffects=$false} }
$ownedLeaves = [Collections.Generic.List[object]]::new()
$ownedDirectories = [Collections.Generic.List[object]]::new()
$sourceMap = [Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
$resources = @()
$defaultStarted = $false
$fixtureBefore = $null
$evidenceCreated=$false

function Write-BoundedMetadata([string]$path,[string]$text,[switch]$CreateNew) {
    $bytes=$utf8.GetBytes($text);if($bytes.Length -gt 16MB){throw 'Control metadata exceeds16MiB bound'}
    if($script:InsideEvidenceOperation){
        $stream=[IO.File]::Open($path,$(if($CreateNew){[IO.FileMode]::CreateNew}else{[IO.FileMode]::Create}),[IO.FileAccess]::Write,[IO.FileShare]::Read)
        try{$stream.Write($bytes,0,$bytes.Length)}finally{$stream.Dispose()};return
    }
    $now=[DateTimeOffset]::UtcNow;$deadline=$now.AddSeconds(30);$limit=$overallDeadline.AddSeconds(-40)
    if($script:inTimedPhase){$limit=$script:phaseDeadline.AddSeconds(-40)
        if(-not $script:writingPhaseCheckpoint){$limit=$limit.AddSeconds(-70)}}
    if($deadline -gt $limit){$deadline=$limit}
    if($deadline -le $now){throw 'No metadata closure budget remains'}
    $pipeName='s04-evidence-'+[Guid]::NewGuid().ToString('N')
    $pipe=[IO.Pipes.NamedPipeServerStream]::new($pipeName,[IO.Pipes.PipeDirection]::Out,1,[IO.Pipes.PipeTransmissionMode]::Byte,[IO.Pipes.PipeOptions]::Asynchronous -bor [IO.Pipes.PipeOptions]::CurrentUserOnly)
    $cancel=[Threading.CancellationTokenSource]::new($deadline-$now);$job=$null;$started=$null
    $out=[IO.MemoryStream]::new();$err=[IO.MemoryStream]::new();$outTask=$null;$errTask=$null;$failure=$null
    $row=[ordered]@{path=[IO.Path]::GetRelativePath($EvidenceDirectory,$path);phase=$script:phaseName;startUtc=$now.ToString('o');deadlineUtc=$deadline.ToString('o');closureDeadlineUtc=$deadline.AddSeconds(40).ToString('o');endUtc=$null;pid=$null;birthUtc=$null;processEndUtc=$null;exitCode=$null;timedOut=$false;assignedBeforeResume=$false;activeAfterCleanup=$null;totalProcesses=0}
    try {
        $args=@('-NoProfile','-NonInteractive','-File',(Join-Path $script:nativeHelperRoot 'Invoke-NativeEvidenceOperation.ps1'),'-MetadataPipe',$pipeName,'-OutputPath',$path,'-EvidenceRoot',$EvidenceDirectory)
        if($CreateNew){$args+=,'-CreateNew'}
        $job=[S04NativeOwnedJob]::Create();$started=$job.StartSuspendedAndAssign((Get-Command pwsh -CommandType Application).Source,$args,$Workspace)
        $row.assignedBeforeResume=$true;$row.pid=$started.Process.Id;$row.birthUtc=$started.Process.StartTime.ToUniversalTime().ToString('o')
        $outTask=$started.StandardOutput.CopyToAsync($out);$errTask=$started.StandardError.CopyToAsync($err)
        [void]$pipe.WaitForConnectionAsync($cancel.Token).GetAwaiter().GetResult()
        [void]$pipe.WriteAsync($bytes,0,$bytes.Length,$cancel.Token).GetAwaiter().GetResult();$pipe.Dispose()
        if(-not $started.Process.WaitForExit([Math]::Max(1,[int]($deadline-[DateTimeOffset]::UtcNow).TotalMilliseconds))){$row.timedOut=$true;throw 'Metadata writer deadline'}
        $row.exitCode=$started.Process.ExitCode;$row.processEndUtc=$started.Process.ExitTime.ToUniversalTime().ToString('o')
    }catch{$failure=$_.Exception.ToString();if([DateTimeOffset]::UtcNow -ge $deadline){$row.timedOut=$true}}
    finally {
        if($job){try{$job.Terminate();$stop=[DateTimeOffset]::UtcNow.AddSeconds(10)
            do{$row.activeAfterCleanup=[int]$job.GetActiveProcessCount();if($row.activeAfterCleanup -eq 0){break};Start-Sleep -Milliseconds 50}while([DateTimeOffset]::UtcNow -lt $stop)
            $row.totalProcesses=[int]$job.GetTotalProcessCount()
            if($started -and $started.Process.WaitForExit(10000)){$row.exitCode=$started.Process.ExitCode;$row.processEndUtc=$started.Process.ExitTime.ToUniversalTime().ToString('o')}
            if($row.activeAfterCleanup -eq 0){foreach($task in @($outTask,$errTask)){if($task){if(-not $task.Wait(10000)){throw 'Metadata stream closure deadline'};[void]$task.GetAwaiter().GetResult()}}}
        }catch{$failure=$_.Exception.ToString()}finally{if($started){$started.StandardOutput.Dispose();$started.StandardError.Dispose();$started.Process.Dispose()};$job.Dispose()}}
        $row.endUtc=[DateTimeOffset]::UtcNow.ToString('o');$row.error=$failure
        if($e.Contains('timing')){$e.timing.metadataWrites+=,$row;if($row.activeAfterCleanup -ne 0){$e.timing.unknownEffects=$true}}
        $pipe.Dispose();$cancel.Dispose();$out.Dispose();$err.Dispose()
    }
    if($failure -or $row.exitCode -ne 0 -or $row.activeAfterCleanup -ne 0){throw "Metadata evidence write failed/nonterminal: $failure"}
}
function Write-Json([string]$path, $value) { Write-BoundedMetadata $path (($value|ConvertTo-Json -Depth 100)+"`n") }
function Write-NewJson([string]$path,$value) {
    Write-BoundedMetadata $path (($value|ConvertTo-Json -Depth 100)+"`n") -CreateNew
}
function Invoke-TimedPhase([string]$name,[scriptblock]$action) {
    $begin=[DateTimeOffset]::UtcNow;$limit=$overallDeadline.AddSeconds(-$cleanupReserveSeconds)
    if($name -ceq 'restoration'){$limit=$runStart.AddSeconds($regularSeconds+$timingPolicy.phases.restoration)}
    elseif($name -ceq 'closure'){$limit=$runStart.AddSeconds($regularSeconds+$timingPolicy.phases.restoration+$timingPolicy.phases.closure)}
    elseif($name -ceq 'validation'){$limit=$overallDeadline}
    $script:phaseName=$name;$script:phaseDeadline=$begin.AddSeconds($timingPolicy.phases[$name])
    if($script:phaseDeadline -gt $limit){$script:phaseDeadline=$limit}
    $row=[ordered]@{name=$name;startUtc=$begin.ToString('o');deadlineUtc=$script:phaseDeadline.ToString('o');endUtc=$null;completed=$false;error=$null}
    $script:inTimedPhase=$true
    try {if($script:phaseDeadline.AddSeconds(-70) -le $begin){throw "Phase admission expired: $name"}; & $action
        if([DateTimeOffset]::UtcNow -gt $script:phaseDeadline.AddSeconds(-70)){throw "Phase action exceeded checkpoint deadline: $name"};$row.completed=$true
    }catch{$row.error=$_.Exception.ToString();throw}
    finally{
        $row.endUtc=[DateTimeOffset]::UtcNow.ToString('o');$e.timing.phases+=,$row;$script:writingPhaseCheckpoint=$true
        try{Write-Json (Join-Path $EvidenceDirectory 'timing.json') $e.timing
            if([DateTimeOffset]::UtcNow -gt $script:phaseDeadline){throw "Phase checkpoint exceeded deadline: $name"}
        }catch{$row.completed=$false;$row.error=$_.Exception.ToString();throw}
        finally{$row.endUtc=[DateTimeOffset]::UtcNow.ToString('o');$script:writingPhaseCheckpoint=$false;$script:inTimedPhase=$false}
    }
}
function Read-ControlBytes([string]$path) {
    # Bounded-size controller I/O only; bulk files are never read through this path.
    Assert-SafeAncestors $path
    $item=Get-Item -LiteralPath $path -Force
    if($item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or $item.Length -gt 16MB){throw 'Control file exceeds16MiB or is not a regular leaf'}
    $stream=[IO.File]::OpenRead($path)
    try{
        if($stream.Length -gt 16MB){throw 'Control file grew beyond16MiB'}
        $bytes=[byte[]]::new([int]$stream.Length);$stream.ReadExactly($bytes,0,$bytes.Length)
        if($stream.ReadByte() -ne -1){throw 'Control file grew while reading'};return ,$bytes
    }finally{$stream.Dispose()}
}
function Save-OwnershipJournal {
    if($script:InsideEvidenceOperation){Write-Json $script:OwnershipJournal @{ownedLeaves=@($ownedLeaves);ownedDirectories=@($ownedDirectories)}}
}
function Import-OperationOwnership($state,$request,[string]$operation) {
    # Validate the full transition before adopting any deletion authority.
    $paths=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase);$ids=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    foreach($row in @($state.ownedLeaves)+@($state.ownedDirectories)){
        if(-not $paths.Add($row.path) -or -not $ids.Add($row.identity) -or $row.identity -cnotmatch '^[0-9a-f]{8}:[0-9a-f]{16}$' -or $row.creationTicks -le 0){throw 'Duplicate/invalid receipt physical custody'}
    }
    foreach($old in @($ownedLeaves)){
        $rows=@($state.ownedLeaves|Where-Object identity -CEQ $old.identity)
        if($rows.Count -ne 1){throw 'Prior physical leaf owner lost/duplicated'};$row=$rows[0]
        $expectedPath=$old.path
        if($operation -ceq 'Promote-OwnedLeaf' -and $old.path -ceq $request.arguments[0]){$expectedPath=$request.arguments[1]}
        if($row.path -cne $expectedPath -or $row.creationTicks -ne $old.creationTicks -or $row.owned -ne $old.owned -or $row.absentBefore -ne $old.absentBefore -or $row.reparse -ne $old.reparse){throw 'Prior custody changed'}
        $seal=$operation -ceq 'Seal-OwnedLeaf' -and $old.path -ceq $request.arguments[0]
        if(-not $seal -and ($row.sha256 -cne $old.sha256 -or $row.length -ne $old.length)){throw 'Prior seal changed without seal operation'}
        if($old.absentAfter -eq $true -and ($row.absentAfter -ne $true -or $row.path -cne $old.path -or $row.sha256 -cne $old.sha256 -or $row.length -ne $old.length)){throw "Accepted leaf absence changed: $($old.path) [$($old.identity)]"}
        if($row.absentAfter -ne $old.absentAfter -and $operation -cne 'Cleanup-Owned'){throw 'Custody absence changed without cleanup'}
        if($row.deletedByCleanup -ne $old.deletedByCleanup -and $operation -cne 'Cleanup-Owned'){throw 'Leaf deletion witness changed without cleanup'}
    }
    foreach($row in @($state.ownedLeaves)){
        $old=@($ownedLeaves|Where-Object identity -CEQ $row.identity)
        if($old.Count -gt 1){throw 'Duplicate previous physical owner'}
        if($old.Count -eq 0 -and ($row.path -cnotin $request.allowedLeafPaths -or $row.owned -ne $true -or $row.absentBefore -ne $true -or $row.absentAfter -ne $false -or $row.deletedByCleanup -ne $false -or $row.reparse -ne $false)){throw 'Foreign new leaf authority'}
        Assert-SafeAncestors $row.path
        if(Test-Path -LiteralPath $row.path){if($row.absentAfter -eq $true){throw "Accepted leaf resurrected: $($row.path) [$($row.identity)]"};if([S04NativeCustody]::Identity($row.path) -cne $row.identity -or (Get-Item -LiteralPath $row.path).CreationTimeUtc.Ticks -ne $row.creationTicks){throw 'Receipt physical leaf differs'}}
        elseif($old.Count -ne 1 -or $row.absentAfter -ne $true -or $row.owned -ne $true -or $row.absentBefore -ne $true -or $row.reparse -ne $false -or
            $row.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $null -eq $row.length -or $row.length -lt 0 -or
            $row.deletedByCleanup -ne $true -or ($operation -cne 'Cleanup-Owned' -and ($old[0].absentAfter -ne $true -or $old[0].deletedByCleanup -ne $true))){throw "Unproven leaf absence: $($row.path) [$($row.identity)]"}
    }
    foreach($old in @($ownedDirectories)){
        $rows=@($state.ownedDirectories|Where-Object path -CEQ $old.path)
        if($rows.Count -ne 1 -or $rows[0].identity -cne $old.identity -or $rows[0].creationTicks -ne $old.creationTicks -or
            $rows[0].owned -ne $old.owned -or $rows[0].absentBefore -ne $old.absentBefore -or $rows[0].reparse -ne $old.reparse){throw 'Prior directory custody changed'}
        if($old.absentAfter -eq $true -and $rows[0].absentAfter -ne $true){throw "Accepted directory absence changed: $($old.path) [$($old.identity)]"}
        if($rows[0].absentAfter -ne $old.absentAfter -and $operation -cne 'Cleanup-Owned'){throw 'Directory absence changed without cleanup'}
        if($rows[0].deletedByCleanup -ne $old.deletedByCleanup -and $operation -cne 'Cleanup-Owned'){throw 'Directory deletion witness changed without cleanup'}
    }
    foreach($row in @($state.ownedDirectories)){
        $old=@($ownedDirectories|Where-Object path -CEQ $row.path)
        if($old.Count -gt 1){throw 'Duplicate previous directory owner'}
        if($old.Count -eq 0 -and ($row.path -cnotin $request.allowedDirectoryPaths -or $row.owned -ne $true -or $row.absentBefore -ne $true -or $row.absentAfter -ne $false -or $row.deletedByCleanup -ne $false -or $row.reparse -ne $false)){throw 'Foreign new directory authority'}
        Assert-SafeAncestors $row.path
        if(Test-Path -LiteralPath $row.path){if($row.absentAfter -eq $true){throw "Accepted directory resurrected: $($row.path) [$($row.identity)]"};if([S04NativeCustody]::Identity($row.path) -cne $row.identity -or (Get-Item -LiteralPath $row.path).CreationTimeUtc.Ticks -ne $row.creationTicks){throw 'Receipt physical directory differs'}}
        elseif($old.Count -ne 1 -or $row.absentAfter -ne $true -or $row.owned -ne $true -or $row.absentBefore -ne $true -or $row.reparse -ne $false -or
            $row.deletedByCleanup -ne $true -or ($operation -cne 'Cleanup-Owned' -and ($old[0].absentAfter -ne $true -or $old[0].deletedByCleanup -ne $true))){throw "Unproven directory absence: $($row.path) [$($row.identity)]"}
    }
    if([DateTimeOffset]::UtcNow -gt (Get-NativeUtc $request.deadlineUtc).AddSeconds(110)){throw 'Custody transition exceeded controller closure deadline'}
    $script:ownedLeaves=[Collections.Generic.List[object]]::new();foreach($row in $state.ownedLeaves){$script:ownedLeaves.Add($row)}
    $script:ownedDirectories=[Collections.Generic.List[object]]::new();foreach($row in $state.ownedDirectories){$script:ownedDirectories.Add($row)}
}
function Assert-OperationReceipt($state,$request,[string]$requestHash,[string]$requestIdentity) {
    if($state.operation -cne $request.operation -or $state.candidate -cne $request.candidate -or (Get-NativeUtc $state.deadlineUtc) -ne (Get-NativeUtc $request.deadlineUtc) -or
        $state.requestSha256 -cne $requestHash -or $state.requestIdentity -cne $requestIdentity -or $state.helperSha256 -cne $request.helperSha256 -or
        (Get-NativeUtc $state.endUtc) -gt (Get-NativeUtc $request.deadlineUtc)) {throw 'Stale/foreign/overdue operation receipt'}
    Import-OperationOwnership $state $request $request.operation
}
function Invoke-BoundedEvidenceOperation([string]$operation,[object[]]$arguments,[int]$seconds=600,$SealAdmission=$null) {
    if($seconds -lt 1 -or $seconds -gt 600){throw 'Evidence operation bound must be1..600'}
    $script:operationSequence++;$name=('operation-{0:D3}' -f $script:operationSequence)
    $requestPath=Join-Path $EvidenceDirectory "$name.request.json"
    foreach($path in @($requestPath,($requestPath+'.result.json'),($requestPath+'.ownership.json'),(Join-Path $EvidenceDirectory "$name.receipt.json"))){if(Test-Path -LiteralPath $path){throw 'Existing operation control file refused'}}
    $begin=[DateTimeOffset]::UtcNow;$deadline=$begin.AddSeconds($seconds)
    if($operation -ceq 'Seal-OwnedLeaf'){
        $sealLimit=$script:phaseDeadline.AddSeconds(-320)
        if($null -eq $SealAdmission -or $SealAdmission.reason -or $SealAdmission.seconds -ne $seconds){throw 'Missing exact finite seal admission'}
        $admittedLimit=Get-NativeUtc $SealAdmission.limitUtc;if($admittedLimit -lt $sealLimit){$sealLimit=$admittedLimit}
        if($deadline -gt $sealLimit){$e.timing.admissions+=,@{name='Seal-OwnedLeaf';utc=$begin.ToString('o');reason='Full seal tail reserve no longer fits before operation effects';effectsStarted=$false;availableSeconds=($sealLimit-$begin).TotalSeconds;seal=$SealAdmission};throw 'Full seal tail reserve no longer fits before operation effects'}
    }
    if($deadline -gt $script:phaseDeadline.AddSeconds(-180)){$deadline=$script:phaseDeadline.AddSeconds(-180)}
    $allowedLeafPaths=@($ownedLeaves|ForEach-Object path)+@($resources|ForEach-Object {$_.path+'.partial'})
    # Archive staging is the only extra resource destination, explicitly selected by this caller.
    if($operation -ceq 'Invoke-DownloadStream'){$allowedLeafPaths+=,($arguments[1]+'.partial')}
    $allowedDirectoryPaths=@(foreach($path in $allowedLeafPaths){for($cursor=[IO.Path]::GetDirectoryName($path);$cursor -and -not (Test-Path -LiteralPath $cursor);$cursor=[IO.Path]::GetDirectoryName($cursor)){$cursor}})
    $helper=Join-Path $script:nativeHelperRoot 'Invoke-NativeEvidenceOperation.ps1';$helperHash=(Get-FileHash -LiteralPath $helper).Hash.ToLowerInvariant()
    if($sourceMap.ContainsKey($helper) -and $sourceMap[$helper].sha256 -cne $helperHash){throw 'Operation helper differs from captured candidate'}
    $request=[ordered]@{operation=$operation;arguments=$arguments;workspace=$Workspace;evidenceDirectory=$EvidenceDirectory;runnerTemp=$RunnerTemp;helperSha256=$helperHash;
        allowedLeafPaths=$allowedLeafPaths;allowedDirectoryPaths=$allowedDirectoryPaths;
        candidate=$CandidateSha;workflow=$WorkflowSha;deadlineUtc=$deadline.ToString('o');sdkRoot=$script:sdkRoot;e=$e;
        ownedLeaves=@($ownedLeaves);ownedDirectories=@($ownedDirectories);sourceRows=@($sourceMap.Values);resources=$resources}
    # Small control metadata only. All bulk I/O, flush/seal and result writes run in the Job.
    Write-NewJson $requestPath $request
    $requestHash=Hash-Bytes (Read-ControlBytes $requestPath);$requestIdentity=[S04NativeCustody]::Identity($requestPath)
    $row=[ordered]@{name=$name;operation=$operation;phase=$script:phaseName;startUtc=$begin.ToString('o');endUtc=$null;deadlineUtc=$deadline.ToString('o');
        helperSha256=$helperHash;requestSha256=$requestHash;requestIdentity=$requestIdentity;receipt=$null;completed=$false;error=$null}
    if($operation -ceq 'Seal-OwnedLeaf'){$row.sealAdmission=$SealAdmission}
    $failure=$null;$result=$null
    try {[void](Invoke-OwnedCommand $name (Get-Command pwsh -CommandType Application).Source @('-NoProfile','-NonInteractive','-File',(Join-Path $script:nativeHelperRoot 'Invoke-NativeEvidenceOperation.ps1'),'-RequestPath',$requestPath) -Seconds $seconds -AbsoluteDeadline $deadline)
    }catch{$failure=$_.Exception.ToString()}
    finally {
        $commands=$e.commands;$timing=$e.timing
        $receipt=$requestPath+'.result.json'
        if(Test-Path -LiteralPath $receipt){
            try {
                $result=$utf8.GetString((Read-ControlBytes $receipt))|ConvertFrom-Json -AsHashtable -Depth 100
                if([DateTimeOffset]::UtcNow -gt $deadline.AddSeconds(110)){throw 'Operation control import exceeded closure deadline'}
                if([S04NativeCustody]::Identity($requestPath) -cne $requestIdentity -or (Hash-Bytes (Read-ControlBytes $requestPath)) -cne $requestHash){throw 'Operation request custody/content changed'}
                $compactPath=Join-Path $EvidenceDirectory "$name.receipt.json"
                $compact=$utf8.GetString((Read-ControlBytes $compactPath))|ConvertFrom-Json -AsHashtable -Depth 10
                foreach($key in @('operation','candidate','requestSha256','requestIdentity','helperSha256','error')){if($compact[$key] -cne $result[$key]){throw 'Compact/terminal receipt binding differs'}}
                if((Get-NativeUtc $compact.deadlineUtc) -ne (Get-NativeUtc $result.deadlineUtc) -or (Get-NativeUtc $compact.endUtc) -ne (Get-NativeUtc $result.endUtc)){throw 'Compact receipt interval differs'}
                $row.receipt=File-Record $compactPath
                if($row.receipt.length -gt 65536){throw 'Compact receipt exceeds64KiB'}
                Assert-OperationReceipt $result $request $requestHash $requestIdentity
            }catch{$e.timing.unknownEffects=$true;$failure=$_.Exception.ToString();$result=$null}
            if($result){$script:e=$result.e;$e.commands=$commands;$e.timing=$timing
            $sourceMap.Clear();foreach($source in $result.sourceRows){$sourceMap.Add((Join-Path $Workspace $source.path),$source)}
            if($result.error -and -not $failure){$failure=$result.error}
            }
        }else{
            $e.timing.unknownEffects=$true
            # A journal without an authenticated terminal receipt is diagnosis only.
            if(-not $failure){$failure='Operation terminal receipt absent'}
        }
        $row.endUtc=[DateTimeOffset]::UtcNow.ToString('o');$row.error=$failure;$row.completed=-not $failure;$e.timing.operations+=,$row
    }
    if($failure){throw $failure}
    if($result.value.Count -eq 1){return $result.value[0]};if($result.value.Count){return ,$result.value}
}
function File-Record([string]$path) {
    $item = Get-Item -LiteralPath $path -Force -ErrorAction Stop
    if ($item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw "Not a regular file: $path" }
    [ordered]@{path=[IO.Path]::GetRelativePath($EvidenceDirectory,$path).Replace('\','/');length=$item.Length;sha256=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()}
}
function Assert-SafeAncestors([string]$path) {
    for ($cursor=[IO.Path]::GetFullPath($path); $cursor; $cursor=[IO.Path]::GetDirectoryName($cursor)) {
        if ((Test-Path -LiteralPath $cursor) -and ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw "Reparse path: $cursor" }
    }
}
function New-OwnedDirectory([string]$path) {
    Assert-SafeAncestors $path
    if (Test-Path -LiteralPath $path) { if (-not (Test-Path -LiteralPath $path -PathType Container)) { throw "Foreign leaf: $path" }; return }
    $parent=[IO.Path]::GetDirectoryName($path)
    if (-not (Test-Path -LiteralPath $parent)) { New-OwnedDirectory $parent }
    [S04NativeCustody]::CreateDirectoryNew($path)
    $ownedDirectories.Add([ordered]@{path=$path;identity=[S04NativeCustody]::Identity($path);creationTicks=(Get-Item -LiteralPath $path).CreationTimeUtc.Ticks;owned=$true;absentBefore=$true;absentAfter=$false;deletedByCleanup=$false;reparse=$false})
    Save-OwnershipJournal
}
function New-OwnedLeaf([string]$path) {
    Assert-SafeAncestors $path
    if (Test-Path -LiteralPath $path) { throw "Pre-existing destination refused: $path" }
    New-OwnedDirectory ([IO.Path]::GetDirectoryName($path))
    $stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::None)
    $ownedLeaves.Add([ordered]@{path=$path;owned=$true;absentBefore=$true;absentAfter=$false;deletedByCleanup=$false;reparse=$false;identity=[S04NativeCustody]::Identity($path);creationTicks=(Get-Item -LiteralPath $path).CreationTimeUtc.Ticks;sha256=$null;length=$null})
    Save-OwnershipJournal
    $stream
}
function Get-OwnedSealLength([string]$path,[Nullable[long]]$DeclaredLength=$null) {
    $rows=@($ownedLeaves|Where-Object path -CEQ $path)
    if($rows.Count -ne 1 -or $rows[0].owned -ne $true -or $rows[0].absentBefore -ne $true -or $rows[0].absentAfter -ne $false -or $rows[0].reparse -ne $false){throw 'No present exact seal owner'}
    Assert-SafeAncestors $path;$item=Get-Item -LiteralPath $path -Force
    if($item.PSIsContainer -or [S04NativeCustody]::Identity($path) -cne $rows[0].identity -or $item.CreationTimeUtc.Ticks -ne $rows[0].creationTicks){throw 'Seal admission physical custody changed'}
    if($null -ne $DeclaredLength -and $item.Length -ne [long]$DeclaredLength){throw 'Declared disk seal length differs'}
    [long]$item.Length
}
function Get-OwnedSealAdmission([long]$length,[DateTimeOffset]$Now=[DateTimeOffset]::UtcNow,[Nullable[DateTimeOffset]]$DownloadStart=$null) {
    # A declared size selects a finite allowance, not a measured throughput promise.
    $seconds=[int][Math]::Max(180,[Math]::Ceiling($length/16MB)+20)
    # Seal closure110 + promotion30 + promotion closure110 + phase checkpoint70.
    $limit=$script:phaseDeadline.AddSeconds(-320)
    if($DownloadStart){$downloadLimit=(Get-NativeUtc $DownloadStart).AddSeconds(1140-110-30-110);if($downloadLimit -lt $limit){$limit=$downloadLimit}}
    $reason=$null
    if($length -lt 0 -or $seconds -gt 600){$reason='Declared full seal exceeds finite600 envelope'}
    elseif($Now.AddSeconds($seconds) -gt $limit){$reason='Full size-dependent seal plus closure/checkpoint/promotion reserve does not fit'}
    [ordered]@{length=$length;bytesPerSecond=16MB;startupSeconds=20;seconds=$seconds;nowUtc=$Now.ToString('o');phaseDeadlineUtc=$script:phaseDeadline.ToString('o');
        limitUtc=$limit.ToString('o');phaseReserveSeconds=320;downloadEnvelopeSeconds=1140;downloadTailReserveSeconds=250;availableSeconds=($limit-$Now).TotalSeconds;reason=$reason}
}
function Write-OperationProgress([string]$boundary,[long]$bytes=0,[long]$length=0) {
    # Raw stream diagnosis only; progress never authorizes custody adoption.
    [Console]::Error.WriteLine('S04_OPERATION '+(@{boundary=$boundary;utc=[DateTimeOffset]::UtcNow.ToString('o');pid=$PID;bytes=$bytes;length=$length}|ConvertTo-Json -Compress))
}
function Read-FullDiskHash([string]$path,[Nullable[long]]$DeclaredLength=$null,[string]$Boundary='disk') {
    Assert-SafeAncestors $path
    $item=Get-Item -LiteralPath $path -Force
    if($item.PSIsContainer){throw 'Full disk hash requires regular leaf'}
    $identity=[S04NativeCustody]::Identity($path);$ticks=$item.CreationTimeUtc.Ticks
    $length=$item.Length;if($null -ne $DeclaredLength -and $length -ne [long]$DeclaredLength){throw 'Declared disk seal length differs'}
    Write-OperationProgress ($Boundary+'-open') 0 $length
    $input=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::Read)
    $hash=[Security.Cryptography.IncrementalHash]::CreateHash([Security.Cryptography.HashAlgorithmName]::SHA256)
    $read=0L;$nextProgress=64MB;$buffer=[byte[]]::new(4MB)
    try{
        if($input.Length -ne $length){throw 'Disk seal length changed before read'}
        Write-OperationProgress ($Boundary+'-read') 0 $length
        while(($count=$input.Read($buffer,0,$buffer.Length)) -gt 0){
            if([DateTimeOffset]::UtcNow -ge $script:OperationDeadline){throw 'Full disk seal deadline'}
            $hash.AppendData($buffer,0,$count);$read+=$count
            if($read -ge $nextProgress){Write-OperationProgress ($Boundary+'-progress') $read $length;$nextProgress=$read+64MB}
        }
        if($read -ne $length -or $input.Length -ne $length -or [S04NativeCustody]::Identity($path) -cne $identity -or (Get-Item -LiteralPath $path).CreationTimeUtc.Ticks -ne $ticks){throw 'Full disk seal custody/length changed'}
        $digest=[Convert]::ToHexString($hash.GetHashAndReset()).ToLowerInvariant()
    }finally{$hash.Dispose();$input.Dispose()}
    Write-OperationProgress ($Boundary+'-complete') $read $length
    [ordered]@{sha256=$digest;length=$read;identity=$identity;creationTicks=$ticks;fullHash=$true}
}
function Seal-OwnedLeaf([string]$path,[Nullable[long]]$DeclaredLength=$null,[Nullable[DateTimeOffset]]$DownloadStart=$null) {
    if(-not $script:InsideEvidenceOperation){
        $length=Get-OwnedSealLength $path $DeclaredLength
        $admission=Get-OwnedSealAdmission $length -DownloadStart $DownloadStart
        if($admission.reason){$e.timing.admissions+=,@{name='Seal-OwnedLeaf';utc=$admission.nowUtc;reason=$admission.reason;effectsStarted=$false;availableSeconds=$admission.availableSeconds;seal=$admission};throw $admission.reason}
        return Invoke-BoundedEvidenceOperation 'Seal-OwnedLeaf' @($path,$length) $admission.seconds $admission
    }
    Write-OperationProgress 'seal-enter'
    $length=Get-OwnedSealLength $path $DeclaredLength
    $row=@($ownedLeaves|Where-Object path -CEQ $path)[0]
    $disk=Read-FullDiskHash $path $length 'seal'
    if($disk.identity -cne $row.identity -or $disk.creationTicks -ne $row.creationTicks){throw 'Physical owner changed during seal'}
    $row.sha256=$disk.sha256;$row.length=$disk.length
}
function Promote-OwnedLeaf([string]$staged,[string]$destination) {
    if(-not $script:InsideEvidenceOperation){return Invoke-BoundedEvidenceOperation 'Promote-OwnedLeaf' @($staged,$destination) 30}
    Assert-SafeAncestors $destination
    if(Test-Path -LiteralPath $destination){throw 'Foreign promotion destination'}
    $row=@($ownedLeaves | Where-Object path -CEQ $staged)
    if($row.Count -ne 1 -or -not $row[0].sha256 -or [S04NativeCustody]::Identity($staged) -cne $row[0].identity){throw 'Unsealed/foreign staged leaf'}
    [IO.File]::Move($staged,$destination,$false)
    $row[0].path=$destination
    if([S04NativeCustody]::Identity($destination) -cne $row[0].identity){throw 'Promotion changed physical identity'}
}
function Git-Bytes([string[]]$arguments) {
    $info=[Diagnostics.ProcessStartInfo]::new('git'); $info.WorkingDirectory=$Workspace; $info.UseShellExecute=$false; $info.CreateNoWindow=$true
    $info.RedirectStandardOutput=$true; $info.RedirectStandardError=$true
    foreach($arg in $arguments){[void]$info.ArgumentList.Add($arg)}
    $process=[Diagnostics.Process]::new(); $process.StartInfo=$info
    $memory=[IO.MemoryStream]::new()
    try { if(-not $process.Start()){throw 'git did not start'}; $copy=$process.StandardOutput.BaseStream.CopyToAsync($memory); $errorTask=$process.StandardError.ReadToEndAsync()
        if(-not $process.WaitForExit(60000)){ $process.Kill($true); [void]$process.WaitForExit(5000); throw 'git read deadline' }
        [void]$copy.GetAwaiter().GetResult(); $errorText=$errorTask.GetAwaiter().GetResult()
        if($process.ExitCode -ne 0 -or $errorText){throw "git read failed: $errorText"}; return ,$memory.ToArray()
    } finally { $memory.Dispose(); $process.Dispose() }
}
function Hash-Bytes([byte[]]$bytes) { [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)).ToLowerInvariant() }
function Capture-Sources {
    if(-not $script:InsideEvidenceOperation){return Invoke-BoundedEvidenceOperation 'Capture-Sources' @()}
    $e.head=$utf8.GetString((Git-Bytes @('rev-parse','HEAD'))).Trim()
    if($CandidateSha -cnotmatch '^[0-9a-f]{40}$' -or $WorkflowSha -cne $CandidateSha -or $e.head -cne $CandidateSha){throw 'C=W=checkout HEAD required before effects'}
    $paths=$utf8.GetString((Git-Bytes @('ls-files','-z'))).Split([char]0,[StringSplitOptions]::RemoveEmptyEntries)
    $paths=@($paths | Where-Object { $_ -cmatch '\.(cs|csproj|props|targets)$|^global\.json$|^tools/S04NativeResourceVerification/|^\.github/workflows/s03-windows-verification\.yml$' })
    foreach($relative in $paths){
        $path=Join-Path $Workspace $relative; Assert-SafeAncestors $path
        $disk=[IO.File]::ReadAllBytes($path); $candidate=Git-Bytes @('cat-file','blob',"${CandidateSha}:$relative")
        $hash=Hash-Bytes $disk; $candidateHash=Hash-Bytes $candidate
        if($disk.Length -ne $candidate.Length -or $hash -cne $candidateHash){throw "Candidate blob mismatch: $relative"}
        $row=[ordered]@{path=$relative;sha256=$hash;length=$disk.Length;candidateSha256=$candidateHash;candidateLength=$candidate.Length;afterSha256=$null;afterLength=$null}
        if($relative -ceq '.github/workflows/s03-windows-verification.yml'){$workflow=Git-Bytes @('cat-file','blob',"${WorkflowSha}:$relative"); $row.workflowSha256=Hash-Bytes $workflow; $row.workflowLength=$workflow.Length
            if($row.workflowSha256 -cne $hash -or $workflow.Length -ne $disk.Length){throw 'Executing workflow blob differs'}}
        $e.sources+=,$row; $sourceMap.Add([IO.Path]::GetFullPath($path),$row)
    }
    foreach($required in @('Invoke-NativeResourceVerification.ps1','Invoke-NativeEvidenceOperation.ps1','Test-NativeResourceEvidence.ps1','Test-NativeResourceEvidence.Tests.ps1','public-resource-pins.json')){
        if(-not $sourceMap.ContainsKey((Join-Path $script:nativeHelperRoot $required))){throw "Untracked helper $required"}
    }
    Write-Json (Join-Path $EvidenceDirectory 'source-before.json') $e.sources
}

function Initialize-NativeHelpers {
    Add-Type -TypeDefinition @'
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
public static class S04NativeCustody {
 [StructLayout(LayoutKind.Sequential)] struct Info {
  public uint Attributes, CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh;
  public uint Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow;
 }
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern SafeFileHandle CreateFileW(string name,uint access,uint share,IntPtr security,uint mode,uint flags,IntPtr template);
 [DllImport("kernel32.dll",SetLastError=true)] static extern bool GetFileInformationByHandle(SafeFileHandle handle,out Info info);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern bool CreateDirectoryW(string name,IntPtr security);
 public static void CreateDirectoryNew(string path) {
  if(!CreateDirectoryW(path,IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error(),"Create-new owned directory failed: "+path);
 }
 public static string Identity(string path) {
  using(var handle=CreateFileW(path,0,7,IntPtr.Zero,3,0x02200000,IntPtr.Zero)) {
   Info info;
   if(handle.IsInvalid || !GetFileInformationByHandle(handle,out info)) throw new Win32Exception(Marshal.GetLastWin32Error());
   if((info.Attributes & 0x400)!=0) throw new InvalidOperationException("Reparse physical owner refused");
   return info.Volume.ToString("x8")+":"+info.IndexHigh.ToString("x8")+info.IndexLow.ToString("x8");
  }
 }
}
'@
    Add-Type -TypeDefinition @'
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;

public sealed class S04NativeOwnedJob : IDisposable
{
    private const int JobObjectBasicAccountingInformation = 1;
    private const uint CreateSuspended = 0x00000004;
    private const uint CreateNoWindow = 0x08000000;
    private const int StartfUseStdHandles = 0x00000100;
    private const uint HandleFlagInherit = 0x00000001;
    private IntPtr handle;

    [StructLayout(LayoutKind.Sequential)]
    private struct BasicAccountingInformation
    {
        public long TotalUserTime;
        public long TotalKernelTime;
        public long ThisPeriodTotalUserTime;
        public long ThisPeriodTotalKernelTime;
        public uint TotalPageFaultCount;
        public uint TotalProcesses;
        public uint ActiveProcesses;
        public uint TotalTerminatedProcesses;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SecurityAttributes
    {
        public int Length;
        public IntPtr SecurityDescriptor;
        public int InheritHandle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct StartupInfo
    {
        public int Size;
        public string Reserved;
        public string Desktop;
        public string Title;
        public int X;
        public int Y;
        public int XSize;
        public int YSize;
        public int XCountChars;
        public int YCountChars;
        public int FillAttribute;
        public int Flags;
        public short ShowWindow;
        public short Reserved2;
        public IntPtr Reserved2Pointer;
        public IntPtr StandardInput;
        public IntPtr StandardOutput;
        public IntPtr StandardError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct ProcessInformation
    {
        public IntPtr Process;
        public IntPtr Thread;
        public uint ProcessId;
        public uint ThreadId;
    }

    public sealed class SuspendedProcess
    {
        public SuspendedProcess(Process process, Stream standardOutput, Stream standardError)
        {
            Process = process;
            StandardOutput = standardOutput;
            StandardError = standardError;
        }

        public Process Process { get; }
        public Stream StandardOutput { get; }
        public Stream StandardError { get; }
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CreateJobObject(IntPtr attributes, string name);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CreatePipe(
        out IntPtr readPipe, out IntPtr writePipe,
        ref SecurityAttributes attributes, uint size);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetHandleInformation(
        IntPtr handle, uint mask, uint flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern bool CreateProcess(
        string applicationName, StringBuilder commandLine,
        IntPtr processAttributes, IntPtr threadAttributes,
        bool inheritHandles, uint creationFlags, IntPtr environment,
        string currentDirectory, ref StartupInfo startupInfo,
        out ProcessInformation processInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint ResumeThread(IntPtr thread);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool TerminateProcess(IntPtr process, uint exitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool QueryInformationJobObject(
        IntPtr job, int informationClass, IntPtr information,
        uint informationLength, IntPtr returnLength);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool TerminateJobObject(IntPtr job, uint exitCode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool CloseHandle(IntPtr handle);

    private S04NativeOwnedJob(IntPtr value) => handle = value;

    public static void AssertBasicAccountingAbi()
    {
        var size = Marshal.SizeOf<BasicAccountingInformation>();
        var activeOffset = Marshal.OffsetOf<BasicAccountingInformation>(
            nameof(BasicAccountingInformation.ActiveProcesses)).ToInt32();
        if (size != 48 || activeOffset != 40)
            throw new InvalidOperationException(
                $"JOBOBJECT_BASIC_ACCOUNTING_INFORMATION ABI mismatch: size={size}, ActiveProcessesOffset={activeOffset}.");
    }

    public static S04NativeOwnedJob Create()
    {
        var value = CreateJobObject(IntPtr.Zero, null);
        if (value == IntPtr.Zero)
            throw new Win32Exception(Marshal.GetLastWin32Error(), "CreateJobObject failed.");
        try { AssertBasicAccountingAbi(); return new S04NativeOwnedJob(value); }
        catch
        {
            CloseHandle(value);
            throw;
        }
    }

    private static string QuoteArgument(string value)
    {
        if (value == null) throw new ArgumentNullException(nameof(value));
        if (value.Length == 0) return "\"\"";
        var builder = new StringBuilder();
        builder.Append('"');
        var backslashes = 0;
        foreach (var character in value)
        {
            if (character == '\\') { backslashes++; continue; }
            if (character == '"')
            {
                builder.Append('\\', backslashes * 2 + 1);
                builder.Append('"');
                backslashes = 0;
                continue;
            }
            builder.Append('\\', backslashes);
            builder.Append(character);
            backslashes = 0;
        }
        builder.Append('\\', backslashes * 2);
        builder.Append('"');
        return builder.ToString();
    }

    public SuspendedProcess StartSuspendedAndAssign(
        string fileName, string[] arguments, string workingDirectory)
    {
        if (handle == IntPtr.Zero) throw new ObjectDisposedException(nameof(S04NativeOwnedJob));
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Executable path is required.", nameof(fileName));
        var commandLine = new StringBuilder(QuoteArgument(fileName));
        foreach (var argument in arguments ?? Array.Empty<string>())
        {
            commandLine.Append(' ');
            commandLine.Append(QuoteArgument(argument));
        }

        var attributes = new SecurityAttributes { Length = Marshal.SizeOf<SecurityAttributes>(), InheritHandle = 1 };
        IntPtr stdoutRead = IntPtr.Zero, stdoutWrite = IntPtr.Zero;
        IntPtr stderrRead = IntPtr.Zero, stderrWrite = IntPtr.Zero;
        ProcessInformation processInformation = default;
        Process process = null;
        FileStream stdout = null, stderr = null;
        try
        {
            if (!CreatePipe(out stdoutRead, out stdoutWrite, ref attributes, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "CreatePipe(stdout) failed.");
            if (!SetHandleInformation(stdoutRead, HandleFlagInherit, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "SetHandleInformation(stdout) failed.");
            if (!CreatePipe(out stderrRead, out stderrWrite, ref attributes, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "CreatePipe(stderr) failed.");
            if (!SetHandleInformation(stderrRead, HandleFlagInherit, 0))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "SetHandleInformation(stderr) failed.");

            var startupInfo = new StartupInfo
            {
                Size = Marshal.SizeOf<StartupInfo>(),
                Flags = StartfUseStdHandles,
                StandardOutput = stdoutWrite,
                StandardError = stderrWrite
            };
            if (!CreateProcess(
                    fileName, commandLine, IntPtr.Zero, IntPtr.Zero, true,
                    CreateSuspended | CreateNoWindow, IntPtr.Zero,
                    string.IsNullOrWhiteSpace(workingDirectory) ? null : workingDirectory,
                    ref startupInfo, out processInformation))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "CreateProcess(CREATE_SUSPENDED) failed.");

            CloseHandle(stdoutWrite); stdoutWrite = IntPtr.Zero;
            CloseHandle(stderrWrite); stderrWrite = IntPtr.Zero;
            process = Process.GetProcessById(checked((int)processInformation.ProcessId));
            // GetProcessById alone does not retain a handle for ExitCode after exit.
            // Cache it while the creation handle keeps this exact root alive and suspended.
            _ = process.SafeHandle;
            if (!AssignProcessToJobObject(handle, processInformation.Process))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "AssignProcessToJobObject(before resume) failed.");
            if (ResumeThread(processInformation.Thread) == uint.MaxValue)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "ResumeThread failed.");

            CloseHandle(processInformation.Thread); processInformation.Thread = IntPtr.Zero;
            CloseHandle(processInformation.Process); processInformation.Process = IntPtr.Zero;
            stdout = new FileStream(new SafeFileHandle(stdoutRead, true), FileAccess.Read, 65536, false);
            stdoutRead = IntPtr.Zero;
            stderr = new FileStream(new SafeFileHandle(stderrRead, true), FileAccess.Read, 65536, false);
            stderrRead = IntPtr.Zero;
            return new SuspendedProcess(process, stdout, stderr);
        }
        catch
        {
            if (processInformation.Process != IntPtr.Zero)
            {
                try { TerminateProcess(processInformation.Process, 1); } catch { }
            }
            if (stdout != null) stdout.Dispose();
            if (stderr != null) stderr.Dispose();
            if (process != null) process.Dispose();
            if (processInformation.Thread != IntPtr.Zero) CloseHandle(processInformation.Thread);
            if (processInformation.Process != IntPtr.Zero) CloseHandle(processInformation.Process);
            if (stdoutRead != IntPtr.Zero) CloseHandle(stdoutRead);
            if (stdoutWrite != IntPtr.Zero) CloseHandle(stdoutWrite);
            if (stderrRead != IntPtr.Zero) CloseHandle(stderrRead);
            if (stderrWrite != IntPtr.Zero) CloseHandle(stderrWrite);
            throw;
        }
    }

    public uint GetActiveProcessCount()
    {
        if (handle == IntPtr.Zero) throw new ObjectDisposedException(nameof(S04NativeOwnedJob));
        var size = Marshal.SizeOf<BasicAccountingInformation>();
        var buffer = Marshal.AllocHGlobal(size);
        try
        {
            if (!QueryInformationJobObject(
                    handle, JobObjectBasicAccountingInformation, buffer,
                    (uint)size, IntPtr.Zero))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "QueryInformationJobObject failed.");
            return Marshal.PtrToStructure<BasicAccountingInformation>(buffer).ActiveProcesses;
        }
        finally
        {
            Marshal.FreeHGlobal(buffer);
        }
    }

    public uint GetTotalProcessCount() { if (handle == IntPtr.Zero) throw new ObjectDisposedException(nameof(S04NativeOwnedJob)); var size=Marshal.SizeOf<BasicAccountingInformation>(); var buffer=Marshal.AllocHGlobal(size); try { if(!QueryInformationJobObject(handle,JobObjectBasicAccountingInformation,buffer,(uint)size,IntPtr.Zero)) throw new Win32Exception(Marshal.GetLastWin32Error()); return Marshal.PtrToStructure<BasicAccountingInformation>(buffer).TotalProcesses; } finally { Marshal.FreeHGlobal(buffer); } }

    public void Terminate()
    {
        if (handle == IntPtr.Zero) return;
        if (!TerminateJobObject(handle, 1))
            throw new Win32Exception(Marshal.GetLastWin32Error(), "TerminateJobObject failed.");
    }

    public void Dispose()
    {
        var value = handle;
        handle = IntPtr.Zero;
        if (value != IntPtr.Zero) CloseHandle(value);
    }
}
'@
    Add-Type -TypeDefinition @'
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
public static class S04NativeBinlogReader {
 public static List<Dictionary<string,object>> Records = new List<Dictionary<string,object>>();
 static Dictionary<string,string> ProjectFiles = new Dictionary<string,string>();
 static object Get(object item,string name) { return item.GetType().GetProperty(name)?.GetValue(item); }
 static string ProjectKey(object item) { object context=Get(item,"BuildEventContext"); return context==null ? "" : Convert.ToString(Get(context,"NodeId"))+":"+Convert.ToString(Get(context,"ProjectContextId")); }
 [DllImport("shell32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern IntPtr CommandLineToArgvW(string commandLine,out int count);
 [DllImport("kernel32.dll")] static extern IntPtr LocalFree(IntPtr value);
 public static string[] SplitCommand(string command) {
  int count; IntPtr buffer=CommandLineToArgvW(command,out count);
  if(buffer==IntPtr.Zero) throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
  try { var values=new string[count]; for(int i=0;i<count;i++) values[i]=Marshal.PtrToStringUni(Marshal.ReadIntPtr(buffer,i*IntPtr.Size)); return values; }
  finally { LocalFree(buffer); }
 }
 public static void OnEvent(object sender,object item) {
  string type=item.GetType().Name;
  string task=Get(item,"TaskName") as string;
  if(type=="ProjectStartedEventArgs") ProjectFiles[ProjectKey(item)]=Get(item,"ProjectFile") as string;
  if(type=="TaskCommandLineEventArgs" && task=="Csc") {
   string projectFile=null; ProjectFiles.TryGetValue(ProjectKey(item),out projectFile);
   Records.Add(new Dictionary<string,object>{{"type",type},{"task",task},{"commandLine",Get(item,"CommandLine")},{"projectFile",projectFile},{"contextKey",ProjectKey(item)}});
  } else if(type=="BuildFinishedEventArgs") {
   Records.Add(new Dictionary<string,object>{{"type",type},{"succeeded",Get(item,"Succeeded")},{"message",Get(item,"Message")}});
  }
 }
 public static List<Dictionary<string,object>> Read(Type sourceType,string path) {
  Records.Clear();
  ProjectFiles.Clear();
  object source=Activator.CreateInstance(sourceType);
  EventInfo ev=sourceType.GetEvent("AnyEventRaised");
  MethodInfo invoke=ev.EventHandlerType.GetMethod("Invoke");
  var p=invoke.GetParameters();
  var a=Expression.Parameter(p[0].ParameterType,"sender");
  var b=Expression.Parameter(p[1].ParameterType,"item");
  var body=Expression.Call(typeof(S04NativeBinlogReader).GetMethod("OnEvent"),Expression.Convert(a,typeof(object)),Expression.Convert(b,typeof(object)));
  Delegate handler=Expression.Lambda(ev.EventHandlerType,body,a,b).Compile();
  ev.AddEventHandler(source,handler);
  sourceType.GetMethod("Replay",new[]{typeof(string)}).Invoke(source,new object[]{path});
  ev.RemoveEventHandler(source,handler);
  (source as IDisposable)?.Dispose();
  return new List<Dictionary<string,object>>(Records);
 }
}
'@
}

function Get-OwnedCommandAdmission([string]$name,[int]$Seconds=600,[switch]$Restoration,[Nullable[DateTimeOffset]]$AbsoluteDeadline=$null,[DateTimeOffset]$Now=[DateTimeOffset]::UtcNow) {
    # The same admission calculation is exercised with an explicit phase clock in regression.
    $policy=Get-NativeTimingPolicy;$groupSeconds=$policy.nativeGroup.commandSeconds
    $now=$Now; $limit=$overallDeadline
    if(-not $Restoration){$limit=$limit.AddSeconds(-$cleanupReserveSeconds)}
    $phase=Get-Variable -Name phaseDeadline -ValueOnly -ErrorAction SilentlyContinue
    if($phase -and $phase -lt $limit){$limit=$phase}
    if($phase){$limit=$limit.AddSeconds(-($policy.closureSeconds+$policy.phaseCheckpointSeconds))}
    if($AbsoluteDeadline -and $AbsoluteDeadline -lt $limit){$limit=$AbsoluteDeadline}
    $reason=$null
    if($name -ieq 'native3'){
        $currentPhase=Get-Variable -Name phaseName -ValueOnly -ErrorAction SilentlyContinue
        if($name -cne 'native3' -or $Seconds -ne $groupSeconds -or $currentPhase -cne 'native' -or $Restoration -or -not $phase){$reason='Native requires exact1800 admission in native phase without restoration'}
        elseif(($limit-$now).TotalSeconds -lt $groupSeconds){$reason='Native1800-second group plus closure and checkpoint does not fit'}
    }
    elseif($Seconds -lt 1 -or $Seconds -gt 600){$reason='Owned command bound must be1..600'}
    elseif($name -cmatch '^(default-before|opt-in|default-restoration)-(build|list)$' -and ($Seconds -ne 600 -or ($limit-$now).TotalSeconds -lt 600)){$reason='Build/discovery requires full600-second admission'}
    $deadline=$now.AddSeconds($Seconds);if($deadline -gt $limit){$deadline=$limit}
    if($deadline -le $now){$reason="No bounded command time remains: $name"}
    [ordered]@{now=$now;limit=$limit;deadline=$deadline;reason=$reason;availableSeconds=($limit-$now).TotalSeconds}
}
function Invoke-OwnedCommand([string]$name,[string]$executable,[string[]]$arguments,[switch]$Restoration,[int]$Seconds=600,[Nullable[DateTimeOffset]]$AbsoluteDeadline=$null) {
    $admission=Get-OwnedCommandAdmission $name -Seconds $Seconds -Restoration:$Restoration -AbsoluteDeadline $AbsoluteDeadline
    $now=$admission.now;$limit=$admission.limit;$deadline=$admission.deadline;$reason=$admission.reason
    if($reason){
        if($e.Contains('timing')){$e.timing.admissions+=,@{name=$name;utc=$now.ToString('o');reason=$reason;effectsStarted=$false;availableSeconds=($limit-$now).TotalSeconds};Write-Json (Join-Path $EvidenceDirectory 'timing.json') $e.timing}
        throw $reason
    }
    $record=[ordered]@{name=$name;executable=$executable;argv=$arguments;cwd=$Workspace;startUtc=$now.ToString('o');endUtc=$null;deadlineUtc=$deadline.ToString('o');pid=$null;birthUtc=$null;
        phase=(Get-Variable -Name phaseName -ValueOnly -ErrorAction SilentlyContinue);processEndUtc=$null;closureDeadlineUtc=$deadline.AddSeconds(110).ToString('o');
        exitCode=$null;timedOut=$false;assignedBeforeResume=$false;totalProcesses=0;activeAfterRootExit=$null;activeAfterCleanup=$null;closureError=$null;stdout="$name.stdout.log";stderr="$name.stderr.log"}
    $job=$null;$started=$null;$out=$null;$err=$null;$outTask=$null;$errTask=$null
    try {
        $job=[S04NativeOwnedJob]::Create(); $started=$job.StartSuspendedAndAssign($executable,$arguments,$Workspace)
        $record.assignedBeforeResume=$true; $record.pid=$started.Process.Id; $record.birthUtc=$started.Process.StartTime.ToUniversalTime().ToString('o')
        $out=[IO.File]::Open((Join-Path $EvidenceDirectory $record.stdout),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
        $err=[IO.File]::Open((Join-Path $EvidenceDirectory $record.stderr),[IO.FileMode]::CreateNew,[IO.FileAccess]::Write,[IO.FileShare]::Read)
        $outTask=$started.StandardOutput.CopyToAsync($out); $errTask=$started.StandardError.CopyToAsync($err)
        $milliseconds=[Math]::Max(1,[int]($deadline-[DateTimeOffset]::UtcNow).TotalMilliseconds)
        if(-not $started.Process.WaitForExit($milliseconds)){$record.timedOut=$true; $job.Terminate()}
        if(-not $started.Process.WaitForExit(10000)){throw "Root did not cease: $name"}
        $record.exitCode=$started.Process.ExitCode; $record.activeAfterRootExit=[int]$job.GetActiveProcessCount(); $record.totalProcesses=[int]$job.GetTotalProcessCount()
        $record.processEndUtc=$started.Process.ExitTime.ToUniversalTime().ToString('o')
    } finally {
        try {
        if($job){
            $job.Terminate(); $stopDeadline=[DateTimeOffset]::UtcNow.AddSeconds(10)
            do {$record.activeAfterCleanup=[int]$job.GetActiveProcessCount(); if($record.activeAfterCleanup -eq 0){break}; Start-Sleep -Milliseconds 100} while([DateTimeOffset]::UtcNow -lt $stopDeadline)
            if($record.activeAfterCleanup -eq 0){foreach($task in @($outTask,$errTask)){if($task){if(-not $task.Wait(10000)){throw 'Stream drain deadline'};[void]$task.GetAwaiter().GetResult()}}}
        }
        }catch{$record.closureError=$_.Exception.ToString();if($e.Contains('timing')){$e.timing.unknownEffects=$true}}
        finally {
        if($started){$started.StandardOutput.Dispose();$started.StandardError.Dispose();$started.Process.Dispose()};if($job){$job.Dispose()}
        if($out){$out.Dispose()};if($err){$err.Dispose()};$record.endUtc=[DateTimeOffset]::UtcNow.ToString('o');$e.commands+=,$record
        Write-Json (Join-Path $EvidenceDirectory 'commands.json') $e.commands
        }
    }
    if($record.timedOut -or $record.exitCode -ne 0 -or $record.activeAfterCleanup -ne 0 -or $record.closureError){throw "$name failed/nonterminal; raw streams retained"}
    $record
}
function Invoke-DownloadStream($pin,[string]$path) {
    $begin=[DateTimeOffset]::UtcNow; $seconds=[Math]::Min(600,($script:OperationDeadline-$begin).TotalSeconds)
    if($seconds -le 0){throw 'No download time remains'}
    $cancel=[Threading.CancellationTokenSource]::new([TimeSpan]::FromSeconds($seconds));$client=[Net.Http.HttpClient]::new();$response=$null;$input=$null;$output=$null
    $row=[ordered]@{id=$pin.id;url=$pin.url;length=0;sha256=$null;fullHash=$false;completed=$false;startUtc=$begin.ToString('o');streamEndUtc=$null;transferEndUtc=$null;endUtc=$null;streamCommand=$null;sealCommand=$null;promotionCommand=$null}
    $staged=$path+'.partial'
    try {
        $client.Timeout=[Threading.Timeout]::InfiniteTimeSpan
        $response=$client.GetAsync($pin.url,[Net.Http.HttpCompletionOption]::ResponseHeadersRead,$cancel.Token).GetAwaiter().GetResult()
        if([int]$response.StatusCode -ne 200 -or $response.Content.Headers.ContentRange){throw 'Only a complete HTTP200 download admitted'}
        if($response.Content.Headers.ContentLength -and $response.Content.Headers.ContentLength -ne [long]$pin.length){throw 'HTTP length differs from pin'}
        $row.finalUrl=$response.RequestMessage.RequestUri.AbsoluteUri
        $input=$response.Content.ReadAsStreamAsync($cancel.Token).GetAwaiter().GetResult();$output=New-OwnedLeaf $staged
        $buffer=[byte[]]::new(1048576);$hash=[Security.Cryptography.IncrementalHash]::CreateHash([Security.Cryptography.HashAlgorithmName]::SHA256)
        try {while(($count=$input.ReadAsync($buffer,0,$buffer.Length,$cancel.Token).GetAwaiter().GetResult()) -gt 0){
            $row.length+=$count;if($row.length -gt [long]$pin.length){throw 'Download exceeds pin'}
            [void]$output.WriteAsync($buffer,0,$count,$cancel.Token).GetAwaiter().GetResult();$hash.AppendData($buffer,0,$count)
        };$row.streamEndUtc=[DateTimeOffset]::UtcNow.ToString('o');$output.Flush($true);$row.sha256=[Convert]::ToHexString($hash.GetHashAndReset()).ToLowerInvariant();$row.fullHash=$true}
        finally{$hash.Dispose()}
        if($row.length -ne [long]$pin.length -or $row.sha256 -cne $pin.sha256){throw 'Full download hash/length mismatch'}
    } finally {if($output){$output.Dispose()};if($input){$input.Dispose()};if($response){$response.Dispose()};$client.Dispose();$cancel.Dispose();$row.transferEndUtc=[DateTimeOffset]::UtcNow.ToString('o');$e.downloads+=,$row;Write-Json (Join-Path $EvidenceDirectory 'downloads.json') $e.downloads}
}
function Invoke-Download($pin,[string]$path) {
    $streamName=('operation-{0:D3}' -f ($script:operationSequence+1))
    [void](Invoke-BoundedEvidenceOperation 'Invoke-DownloadStream' @($pin,$path) 600)
    $sealName=('operation-{0:D3}' -f ($script:operationSequence+1));$transfer=@($e.downloads|Where-Object id -CEQ $pin.id)[0];Seal-OwnedLeaf ($path+'.partial') ([long]$pin.length) (Get-NativeUtc $transfer.startUtc)
    $disk=@($ownedLeaves|Where-Object path -CEQ ($path+'.partial'))[0]
    if($disk.length -ne [long]$pin.length -or $disk.sha256 -cne $pin.sha256){throw 'Full on-disk download hash/length mismatch'}
    $promotionName=('operation-{0:D3}' -f ($script:operationSequence+1));Promote-OwnedLeaf ($path+'.partial') $path
    $row=@($e.downloads|Where-Object id -CEQ $pin.id)[0]
    $row.diskSha256=$disk.sha256;$row.diskLength=$disk.length;$row.streamCommand=$streamName;$row.sealCommand=$sealName;$row.promotionCommand=$promotionName
    $row.completed=$true;$row.endUtc=[DateTimeOffset]::UtcNow.ToString('o');Write-Json (Join-Path $EvidenceDirectory 'downloads.json') $e.downloads
}
function Resource-Hashes([string]$stage) {
    if(-not $script:InsideEvidenceOperation){return Invoke-BoundedEvidenceOperation 'Resource-Hashes' @($stage) 300}
    $errors=[Collections.Generic.List[string]]::new()
    foreach($resource in $resources){
        $row=[ordered]@{stage=$stage;id=$resource.pin.id;path=$resource.path;length=$null;sha256=$null;fullHash=$false;error=$null}
        try{Assert-SafeAncestors $resource.path;$item=Get-Item -LiteralPath $resource.path -Force
            if($item.PSIsContainer){throw 'Resource is a directory'}
            $disk=Read-FullDiskHash $resource.path ([long]$resource.pin.length) ('resource-'+$stage+'-'+$resource.pin.id)
            $row.length=$disk.length;$row.sha256=$disk.sha256;$row.fullHash=$disk.fullHash
            if($row.length -ne [long]$resource.pin.length -or $row.sha256 -cne $resource.pin.sha256){throw "Resource drift $stage/$($resource.pin.id)"}
        }catch{$row.error=$_.Exception.Message;$errors.Add($row.error)}
        $e.resources+=,$row
    }
    Write-Json (Join-Path $EvidenceDirectory 'resource-hashes.json') $e.resources
    if($errors.Count){throw ($errors -join '; ')}
}
function Provision-PublicResources {
    $pins=Get-Content -LiteralPath (Join-Path $script:nativeHelperRoot 'public-resource-pins.json') -Raw | ConvertFrom-Json -Depth 30
    $ollama=Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)) 'Programs/Ollama'
    $models=Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)) '.ollama/models'
    # Never adopt an existing runtime/model root, even if matching pinned leaves exist.
    foreach($root in @($ollama,$models)){Assert-SafeAncestors $root;if(Test-Path -LiteralPath $root){throw "Existing root refused: $root"}}
    $script:resources=@(foreach($pin in @($pins.members)+@($pins.blobs)+@($pins.manifest)){
        $root=if($pin.root -ceq 'ollama'){$ollama}elseif($pin.root -ceq 'models'){$models}else{throw 'Unknown pin root'}
        $path=[IO.Path]::GetFullPath((Join-Path $root $pin.relativePath));if(-not $path.StartsWith($root+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Pin path escape'}
        [ordered]@{pin=$pin;path=$path}
    })
    $drive=[IO.DriveInfo]::new([IO.Path]::GetPathRoot($RunnerTemp));$e.disk.availableBytes=$drive.AvailableFreeSpace
    # Full ZIP+12 resources, 4GiB build/binlog/binary reserve, 2GiB restoration/cleanup reserve.
    $e.disk.requiredBytes=8097805929L+6GB
    if($e.disk.availableBytes -lt $e.disk.requiredBytes){throw 'Measured disk space insufficient'}
    $staging=Join-Path $RunnerTemp ('s04-native-download-'+$env:GITHUB_RUN_ID+'-'+$env:GITHUB_RUN_ATTEMPT)
    if(Test-Path -LiteralPath $staging){throw 'Foreign staging path'};New-OwnedDirectory $staging
    $zipPath=Join-Path $staging 'ollama.zip';Invoke-Download $pins.archive $zipPath
    foreach($member in $pins.members){
        $destination=@($resources | Where-Object {$_.pin.id -ceq $member.id})[0].path
        $staged=$destination+'.partial';[void](Invoke-BoundedEvidenceOperation 'Expand-OwnedMember' @($zipPath,$member,$staged) 180)
        Seal-OwnedLeaf $staged ([long]$member.length)
        $disk=@($ownedLeaves | Where-Object path -CEQ $staged)[0]
        if($disk.length -ne [long]$member.length -or $disk.sha256 -cne $member.sha256){throw 'ZIP member full hash/length mismatch'}
        Promote-OwnedLeaf $staged $destination
    }
    foreach($blob in $pins.blobs){Invoke-Download $blob (@($resources | Where-Object {$_.pin.id -ceq $blob.id})[0].path)}
    $manifest=[Text.Encoding]::UTF8.GetBytes($pins.manifest.utf8NoBomNoNewline)
    if($manifest.Length -ne 709 -or (Hash-Bytes $manifest) -cne $pins.manifest.sha256){throw 'Pinned compact manifest mismatch'}
    $destination=@($resources | Where-Object {$_.pin.id -ceq $pins.manifest.id})[0].path;$staged=$destination+'.partial'
    [void](Invoke-BoundedEvidenceOperation 'Write-OwnedManifest' @($pins.manifest.utf8NoBomNoNewline,$staged) 30);Seal-OwnedLeaf $staged
    Promote-OwnedLeaf $staged $destination
    Resource-Hashes 'pre'
}

# Csc replay resolves the active SDK below, rather than a developer-machine SDK path.
function Expand-OwnedMember([string]$zipPath,$member,[string]$staged) {
    $zip=[IO.Compression.ZipFile]::OpenRead($zipPath);$input=$null;$output=$null
    try {
        $entry=@($zip.Entries|Where-Object FullName -CEQ $member.member)
        if($entry.Count -ne 1 -or $entry[0].Length -ne [long]$member.length){throw 'Missing/duplicate/wrong ZIP member'}
        $output=New-OwnedLeaf $staged;$input=$entry[0].Open();$input.CopyTo($output,1048576);$output.Flush($true)
    }finally{if($input){$input.Dispose()};if($output){$output.Dispose()};$zip.Dispose()}
}
function Write-OwnedManifest([string]$content,[string]$staged) {
    $bytes=$utf8.GetBytes($content);$output=New-OwnedLeaf $staged
    try{$output.Write($bytes,0,$bytes.Length);$output.Flush($true)}finally{$output.Dispose()}
}

function Input-Record([string]$path,[string]$projectFile=(Join-Path $Workspace 'Tests/LlmFromScratch.Tests.csproj')) {
    $path=[IO.Path]::GetFullPath($path);Assert-SafeAncestors $path
    $item=Get-Item -LiteralPath $path; $hash=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    if($sourceMap.ContainsKey($path)){$source=$sourceMap[$path];if($hash -cne $source.sha256 -or $item.Length -ne $source.length){throw "Changed compile input $path"}
        return [ordered]@{path=$source.path;kind='tracked';length=$item.Length;sha256=$hash;candidateSha256=$source.candidateSha256;candidateLength=$source.candidateLength}}
    if(-not $path.StartsWith($Workspace+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or $path -notmatch '[/\\]obj[/\\]') {
        $projectFile=[IO.Path]::GetFullPath($projectFile)
        if($projectFile -cne (Join-Path $Workspace 'Tests/LlmFromScratch.Tests.csproj') -or -not $sourceMap.ContainsKey($projectFile)){throw 'No candidate-pinned Test.Sdk project owner'}
        $projectSource=$sourceMap[$projectFile]
        if((Get-FileHash -LiteralPath $projectFile).Hash.ToLowerInvariant() -cne $projectSource.candidateSha256 -or (Get-Item -LiteralPath $projectFile).Length -ne $projectSource.candidateLength){throw 'Changed Test.Sdk candidate project owner'}
        return Get-NativeTestSdkInput $path $projectFile
    }
    [ordered]@{path=[IO.Path]::GetRelativePath($Workspace,$path).Replace('\','/');kind='generated';length=$item.Length;sha256=$hash}
}
function Read-PdbBinding([string]$dll,[string]$pdb) { Read-NativeManagedPdbBinding $dll $pdb { param($path) Input-Record $path } }
function Capture-Build([string]$phase) {
    if(-not $script:InsideEvidenceOperation){return Invoke-BoundedEvidenceOperation 'Capture-Build' @($phase) 300}
    $binlog=Join-Path $EvidenceDirectory "$phase-build.binlog"; $rows=[S04NativeBinlogReader]::Read($script:replayType,$binlog)
    $events=Join-Path $EvidenceDirectory "$phase-csc.json";Write-Json $events $rows
    $commands=@($rows | Where-Object {$_.type -ceq 'TaskCommandLineEventArgs'})
    $tests=@($commands | Where-Object {$_.projectFile -and [IO.Path]::GetFullPath($_.projectFile) -ceq (Join-Path $Workspace 'Tests/LlmFromScratch.Tests.csproj')})
    if($tests.Count -ne 1 -or @($rows | Where-Object {$_.type -ceq 'BuildFinishedEventArgs' -and $_.succeeded -eq $true}).Count -ne 1){throw 'Incomplete rebuild Csc evidence'}
    $inputs=@(foreach($command in $commands){if(-not $command.projectFile){throw 'Missing Csc project context'};$projectDir=Split-Path -LiteralPath $command.projectFile
        foreach($argument in [S04NativeBinlogReader]::SplitCommand($command.commandLine)){if($argument.EndsWith('.cs',[StringComparison]::OrdinalIgnoreCase) -and -not $argument.StartsWith('/')){
            $path=if([IO.Path]::IsPathRooted($argument)){$argument}else{Join-Path $projectDir $argument};Input-Record $path $command.projectFile}}
    })
    $testArguments=[S04NativeBinlogReader]::SplitCommand($tests[0].commandLine)
    $nativeCount=@($testArguments | Where-Object { [IO.Path]::GetFileName($_) -ceq 'CodeDocumentationConfiguredQwenNativeResourceTests.cs' }).Count
    if($nativeCount -ne [int]($phase -ceq 'opt-in')){throw 'Wrong native compile include'}
    $copyRoot=Join-Path $EvidenceDirectory "managed/$phase";[void][IO.Directory]::CreateDirectory($copyRoot)
    $artifacts=@();$bindings=@()
    foreach($name in @('LlmFromScratch.Tests','ModelServer','ContinuousLearning','TrainableRouter')){
        $dll=Join-Path $Workspace "Tests/bin/Release/net9.0/$name.dll";$pdb=Join-Path $Workspace "Tests/bin/Release/net9.0/$name.pdb"
        $binding=Read-PdbBinding $dll $pdb;$binding.name=$name;$bindings+=,$binding
        foreach($file in @($dll,$pdb)){$copy=Join-Path $copyRoot ([IO.Path]::GetFileName($file));[IO.File]::Copy($file,$copy,$false);$artifacts+=,(File-Record $copy)}
    }
    $record=[ordered]@{phase=$phase;binlog=(File-Record $binlog);cscEvidence=(File-Record $events);testsCscCount=$tests.Count;nativeIncluded=$nativeCount;compileInputs=$inputs;pdbBindings=$bindings;artifacts=$artifacts;
        testsDllSha256=(@($artifacts | Where-Object {$_.path.EndsWith('/LlmFromScratch.Tests.dll')})[0].sha256);testsPdbSha256=(@($artifacts | Where-Object {$_.path.EndsWith('/LlmFromScratch.Tests.pdb')})[0].sha256)}
    $e.builds+=,$record;Write-Json (Join-Path $EvidenceDirectory 'build-binding.json') $e.builds
}
function Build-And-List([string]$phase,[switch]$Restoration) {
    $value=if($phase -ceq 'opt-in'){'true'}else{'false'};$property="-p:XiigenEnableConfiguredQwenNativeResourceTests=$value"
    [void](Invoke-OwnedCommand "$phase-build" $script:dotnet @('build','Tests/LlmFromScratch.Tests.csproj','--configuration','Release','--no-restore','-t:Rebuild',$property,'-nodeReuse:false','-p:UseSharedCompilation=false',"-bl:$(Join-Path $EvidenceDirectory "$phase-build.binlog")") -Restoration:$Restoration)
    [void](Invoke-OwnedCommand "$phase-list" $script:dotnet @('test','Tests/LlmFromScratch.Tests.csproj','--configuration','Release','--no-restore','--no-build','--list-tests',$property) -Restoration:$Restoration)
    $text=[IO.File]::ReadAllText((Join-Path $EvidenceDirectory "$phase-list.stdout.log"))
    [string[]]$rawNames=@($text -split '\r?\n' | ForEach-Object {$_.Trim()} | Where-Object {$_ -cmatch '^LlmFromScratch\.Tests\.'})
    $ordinal=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal);$ordinal.UnionWith($rawNames)
    [string[]]$names=@($ordinal)
    [Array]::Sort($names,[StringComparer]::Ordinal)
    if($names.Count -eq 0){throw 'Empty discovery'}
    $nativeIdentities=@(Get-NativeResourceIdentities)
    $nativeRows=@($rawNames | Where-Object {$_ -clike 'LlmFromScratch.Tests.CodeDocumentationConfiguredQwenNativeResourceTests.*'})
    if($phase -ceq 'opt-in'){
        $nativeSet=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal);$nativeSet.UnionWith([string[]]$nativeRows)
        if($nativeRows.Count -ne 3 -or -not $nativeSet.SetEquals([string[]]$nativeIdentities)){throw 'Opt-in discovery must contain exact native3 once each'}
    }elseif($nativeRows.Count){throw 'Default discovery exposed native tests'}
    if($phase -cne 'default-before'){
        $baseline=@($e.discovery | Where-Object phase -CEQ 'default-before')
        if($baseline.Count -ne 1){throw 'No unique default discovery baseline'}
        $portable=[Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $portable.UnionWith([string[]]@($names | Where-Object {$_ -cnotin $nativeIdentities}))
        if(-not $portable.SetEquals([string[]]$baseline[0].identities)){throw 'Portable ordinal discovery differs from default baseline'}
    }
    $e.discovery+=,[ordered]@{phase=$phase;identities=$names;rawCount=$rawNames.Count};Write-Json (Join-Path $EvidenceDirectory 'discovery.json') $e.discovery
    Capture-Build $phase
}
function Fixture-Inventory {
    if(-not $script:InsideEvidenceOperation){return Invoke-BoundedEvidenceOperation 'Fixture-Inventory' @() 180}
    $rows=[Collections.Generic.List[object]]::new()
    foreach($relative in @('artifacts/test-results/code-documentation-repository-owned-dpo-training-fixtures','training-artifacts','training-data/dpo','training-data/reference-corpora','models')){
        $root=Join-Path $Workspace $relative
        if(-not (Test-Path -LiteralPath $root)){continue}
        $queue=[Collections.Generic.Queue[string]]::new();$queue.Enqueue($root)
        while($queue.Count){$directory=$queue.Dequeue();Assert-SafeAncestors $directory
            foreach($item in Get-ChildItem -LiteralPath $directory -Force){if($item.Attributes -band [IO.FileAttributes]::ReparsePoint){throw 'Fixture reparse path'}
                if($item.PSIsContainer){$queue.Enqueue($item.FullName)}else{$rows.Add([ordered]@{path=[IO.Path]::GetRelativePath($Workspace,$item.FullName);length=$item.Length;ticks=$item.LastWriteTimeUtc.Ticks;sha256=(Get-FileHash -LiteralPath $item.FullName).Hash.ToLowerInvariant()})}}
        }
    }
    $ordered=@($rows | Sort-Object -Property path -CaseSensitive);$json=ConvertTo-Json -InputObject $ordered -Depth 10 -Compress
    [ordered]@{digest=(Hash-Bytes ($utf8.GetBytes($json)));files=$ordered}
}
function Cleanup-Owned {
    if(-not $script:InsideEvidenceOperation){return Invoke-BoundedEvidenceOperation 'Cleanup-Owned' @() 300}
    if(@($e.commands | Where-Object {$_.activeAfterCleanup -ne 0}).Count){throw 'Unknown/live owned processes; deletion refused'}
    $e.cleanup.activeProcesses=0
    foreach($leaf in $ownedLeaves){try{
        Assert-SafeAncestors $leaf.path
        if($leaf.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $null -eq $leaf.length -or $leaf.length -lt 0){throw 'Unsealed owner; preserve leaf'}
        if($leaf.absentAfter -eq $true -and (Test-Path -LiteralPath $leaf.path)){throw 'Accepted absent leaf resurrected; preserve leaf'}
        if($leaf.absentAfter -ne $true -and -not (Test-Path -LiteralPath $leaf.path)){throw 'Unproved missing leaf; cleanup cannot adopt disappearance'}
        if(Test-Path -LiteralPath $leaf.path){$item=Get-Item -LiteralPath $leaf.path -Force
            if($item.PSIsContainer -or [S04NativeCustody]::Identity($leaf.path) -cne $leaf.identity -or $item.CreationTimeUtc.Ticks -ne $leaf.creationTicks -or -not $leaf.sha256 -or $item.Length -ne $leaf.length){throw 'Ownership/content changed; preserve leaf'}
            $disk=Read-FullDiskHash $leaf.path ([long]$leaf.length) 'cleanup'
            if($disk.identity -cne $leaf.identity -or $disk.creationTicks -ne $leaf.creationTicks -or $disk.sha256 -cne $leaf.sha256){throw 'Ownership/content changed; preserve leaf'}
            Remove-Item -LiteralPath $leaf.path -ErrorAction Stop
            $leaf.deletedByCleanup=$true
        };$leaf.absentAfter=-not (Test-Path -LiteralPath $leaf.path)
    }catch{$e.cleanup.failures+=,"$($leaf.path):$($_.Exception.Message)"}}
    for($i=$ownedDirectories.Count-1;$i -ge 0;$i--){$owner=$ownedDirectories[$i];$directory=$owner.path;try{Assert-SafeAncestors $directory
        if($owner.absentAfter -ne $true -and -not (Test-Path -LiteralPath $directory)){throw 'Unproved missing directory; cleanup cannot adopt disappearance'}
        if(Test-Path -LiteralPath $directory){
            if($owner.absentAfter -eq $true){throw 'Accepted absent directory resurrected; preserve directory'}
            if([S04NativeCustody]::Identity($directory) -cne $owner.identity -or (Get-Item -LiteralPath $directory).CreationTimeUtc.Ticks -ne $owner.creationTicks){throw 'Physical directory owner changed; preserve directory'}
            if(@(Get-ChildItem -LiteralPath $directory -Force).Count){throw 'Foreign/unknown remaining content; preserve directory'};Remove-Item -LiteralPath $directory -ErrorAction Stop;$owner.deletedByCleanup=$true}
        $owner.absentAfter=-not (Test-Path -LiteralPath $directory)
        $e.cleanup.directories+=,[ordered]@{path=$directory;owned=$true;absentAfter=$owner.absentAfter}
    }catch{$e.cleanup.failures+=,"${directory}:$($_.Exception.Message)"}}
    $e.cleanup.leaves=@($ownedLeaves);$e.cleanup.completed=$e.cleanup.failures.Count -eq 0 -and -not $e.timing.unknownEffects
    Write-Json (Join-Path $EvidenceDirectory 'cleanup.json') $e.cleanup
}

function Capture-SourceAfter {
    foreach($source in $e.sources){$path=Join-Path $Workspace $source.path;$source.afterSha256=(Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant();$source.afterLength=(Get-Item -LiteralPath $path).Length}
}
function Complete-EvidenceValidation {
    Write-Json (Join-Path $EvidenceDirectory 'restoration.json') $e.restoration
    Write-Json (Join-Path $EvidenceDirectory 'evidence.json') $e
    $begin=[DateTimeOffset]::UtcNow
    $validation=Test-NativeResourceEvidence -Directory $EvidenceDirectory
    $validation|Add-Member -NotePropertyName startUtc -NotePropertyValue $begin.ToString('o')
    $validation|Add-Member -NotePropertyName endUtc -NotePropertyValue ([DateTimeOffset]::UtcNow.ToString('o'))
    Write-Json (Join-Path $EvidenceDirectory 'validation-report.json') $validation
}

try {
    if(-not $IsWindows -or $env:GITHUB_ACTIONS -cne 'true' -or $env:GATE_NAME -cne $e.gate -or $env:RUNNER_ENVIRONMENT -cne 'github-hosted'){throw 'This lane is explicit hosted Windows CI only'}
    if(-not $EvidenceDirectory.StartsWith($RunnerTemp+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){throw 'Evidence must be under RunnerTemp'}
    Assert-SafeAncestors $EvidenceDirectory
    if(Test-Path -LiteralPath $EvidenceDirectory){throw 'Evidence destination already exists'}
    [void][IO.Directory]::CreateDirectory($EvidenceDirectory)
    $evidenceCreated=$true
    Initialize-NativeHelpers
    Invoke-TimedPhase 'prepare' {
    Capture-Sources
    $script:dotnet=(Get-Command dotnet -CommandType Application).Source
    # SDK query/validator regression also use the same owned process containment.
    [void](Invoke-OwnedCommand 'sdk-version' $script:dotnet @('--version'))
    $e.sdk=[IO.File]::ReadAllText((Join-Path $EvidenceDirectory 'sdk-version.stdout.log')).Trim()
    [void](Invoke-OwnedCommand 'sdk-list' $script:dotnet @('--list-sdks'))
    $sdkRows=[IO.File]::ReadAllLines((Join-Path $EvidenceDirectory 'sdk-list.stdout.log'))
    $sdkRow=@($sdkRows | Where-Object {$_ -cmatch ('^'+[regex]::Escape($e.sdk)+' \[')})
    if($sdkRow.Count -ne 1 -or $sdkRow[0] -cnotmatch '^\S+ \[(.+)\]$'){throw 'Active SDK resolution failed'}
    $script:sdkRoot=Join-Path $Matches[1] $e.sdk
    [void][Reflection.Assembly]::LoadFrom((Join-Path $sdkRoot 'Microsoft.Build.Framework.dll'))
    $buildAssembly=[Reflection.Assembly]::LoadFrom((Join-Path $sdkRoot 'Microsoft.Build.dll'));$script:replayType=$buildAssembly.GetType('Microsoft.Build.Logging.BinaryLogReplayEventSource')
    if(-not $script:replayType){throw 'SDK binlog reader unavailable'}
    $pwsh=(Get-Command pwsh -CommandType Application).Source
    $regression=Invoke-OwnedCommand 'validator-regression' $pwsh @('-NoProfile','-NonInteractive','-File',(Join-Path $PSScriptRoot 'Test-NativeResourceEvidence.Tests.ps1'),'-OutputDirectory',(Join-Path $EvidenceDirectory 'validator-regression'))
    $regressionReport=Get-Content -LiteralPath (Join-Path $EvidenceDirectory 'validator-regression/result.json') -Raw | ConvertFrom-Json
    $e.regression=@{exitCode=$regression.exitCode;passed=$regressionReport.passed}
    # Disable reuse for only this CI command host and descendants; never stop global build servers.
    $env:DOTNET_CLI_DO_NOT_USE_MSBUILD_SERVER='1';$env:MSBUILDDISABLENODEREUSE='1'
    [void](Invoke-OwnedCommand 'restore' $script:dotnet @('restore','Tests/LlmFromScratch.Tests.csproj','-nodeReuse:false','-p:UseSharedCompilation=false'))
    }
    Invoke-TimedPhase 'default' {$script:defaultStarted=$true;Build-And-List 'default-before'}
    Invoke-TimedPhase 'provision' {Provision-PublicResources}
    Invoke-TimedPhase 'optin' {Build-And-List 'opt-in'}
    Invoke-TimedPhase 'preflight' {
    $script:fixtureBefore=Fixture-Inventory;$e.fixture.beforeDigest=$fixtureBefore.digest;Write-Json (Join-Path $EvidenceDirectory 'fixture-before.json') $fixtureBefore
    $build=@($e.builds|Where-Object phase -CEQ 'opt-in')[0]
    $e.commandBinarySha256=$build.testsDllSha256;$e.commandPdbSha256=$build.testsPdbSha256
    }
    $filter=((Get-NativeResourceIdentities) | ForEach-Object {'FullyQualifiedName='+$_}) -join '|'
    Invoke-TimedPhase 'native' {
    # Admission precedes scoped diagnostic environment effects; Invoke-OwnedCommand rechecks before creating the Job.
    $admission=Get-OwnedCommandAdmission 'native3' -Seconds $timingPolicy.nativeGroup.commandSeconds
    if($admission.reason){
        $e.timing.admissions+=,@{name='native3';utc=$admission.now.ToString('o');reason=$admission.reason;effectsStarted=$false;availableSeconds=$admission.availableSeconds}
        Write-Json (Join-Path $EvidenceDirectory 'timing.json') $e.timing
        throw $admission.reason
    }
    $traceEnvironment=[ordered]@{S04_NATIVE_STAGE_PATH=(Join-Path $EvidenceDirectory 'native3-progress.ndjson');S04_NATIVE_STAGE_CANDIDATE=$CandidateSha;S04_NATIVE_STAGE_RUN=$env:GITHUB_RUN_ID}
    $priorTraceEnvironment=@{}
    foreach($key in $traceEnvironment.Keys){$priorTraceEnvironment[$key]=[Environment]::GetEnvironmentVariable($key,'Process')}
    try{
        foreach($key in $traceEnvironment.Keys){[Environment]::SetEnvironmentVariable($key,$traceEnvironment[$key],'Process')}
        [void](Invoke-OwnedCommand 'native3' $script:dotnet @('test','Tests/LlmFromScratch.Tests.csproj','--configuration','Release','--no-restore','--no-build','-p:XiigenEnableConfiguredQwenNativeResourceTests=true','--filter',$filter,'--logger','trx;LogFileName=native3.trx','--results-directory',$EvidenceDirectory,'--diag',(Join-Path $EvidenceDirectory 'vstest-diag.log')) -Seconds $timingPolicy.nativeGroup.commandSeconds)
    }finally{foreach($key in $traceEnvironment.Keys){
        # PowerShell converts an ordinary $null string argument to empty. Preserve absence explicitly.
        if($null -eq $priorTraceEnvironment[$key]){[Environment]::SetEnvironmentVariable($key,[NullString]::Value,'Process')}
        else{[Environment]::SetEnvironmentVariable($key,$priorTraceEnvironment[$key],'Process')}
    }}
    }
} catch {
    $e.failures+=,$_.Exception.ToString()
} finally {
    if($evidenceCreated){
        # Restore first. Post-hashes/inventory/cleanup cannot spend its separate reserve.
        if($defaultStarted){try{Invoke-TimedPhase 'restoration' {Build-And-List 'default-restoration' -Restoration;$d=@($e.discovery | Where-Object phase -CEQ 'default-restoration')[0]
            $e.restoration.nativeCount=@($d.identities | Where-Object {$_ -clike 'LlmFromScratch.Tests.CodeDocumentationConfiguredQwenNativeResourceTests.*'}).Count;$e.restoration.completed=$e.restoration.nativeCount -eq 0
        }}catch{$e.failures+=,$_.Exception.ToString()}}
        try{Invoke-TimedPhase 'closure' {
        if($e.failures.Count){Write-BoundedMetadata (Join-Path $EvidenceDirectory 'first-failure.txt') $e.failures[0]}
        if($resources.Count){try{Resource-Hashes 'post'}catch{$e.failures+=,$_.Exception.ToString()}}
        if($fixtureBefore){try{$after=Fixture-Inventory;$e.fixture.afterDigest=$after.digest;Write-Json (Join-Path $EvidenceDirectory 'fixture-after.json') $after}catch{$e.failures+=,$_.Exception.ToString()}}
        try{[void](Invoke-BoundedEvidenceOperation 'Capture-SourceAfter' @() 180)}catch{$e.failures+=,$_.Exception.ToString()}
        # An interrupted/unsealed leaf is preserved. Closure does not manufacture a seal.
        try{Cleanup-Owned}catch{$e.cleanup.failures+=,$_.Exception.ToString();$e.failures+=,$_.Exception.ToString()}
        }}catch{$e.failures+=,$_.Exception.ToString()}
        $e.timing.endUtc=[DateTimeOffset]::UtcNow.ToString('o')
        try{
            Invoke-TimedPhase 'validation' {[void](Invoke-BoundedEvidenceOperation 'Complete-EvidenceValidation' @() 300)}
        }catch{Write-Json (Join-Path $EvidenceDirectory 'validation-report.json') @{valid=$false;failures=@($_.Exception.ToString())}}
    }
}
if(-not $evidenceCreated -or -not (Test-Path -LiteralPath (Join-Path $EvidenceDirectory 'validation-report.json'))){throw 'Owned validation evidence absent'}
$final=Get-Content -LiteralPath (Join-Path $EvidenceDirectory 'validation-report.json') -Raw | ConvertFrom-Json
if(-not $final.valid){Write-Output ($final | ConvertTo-Json -Depth 20);exit 1}
Write-Output 'Exact native3 evidence validated; full S04 remains open.'
