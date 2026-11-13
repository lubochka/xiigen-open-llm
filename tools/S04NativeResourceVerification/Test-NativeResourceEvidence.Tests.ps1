# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD; AGPL-3.0-only.
# Evidence regression plus actual runner Git/stream/Windows owned-process controls; no model, native resource, Pester or network.
[CmdletBinding()]
param([Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference='Stop'
. (Join-Path $PSScriptRoot 'Test-NativeResourceEvidence.ps1')
$OutputDirectory=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $OutputDirectory){throw 'Create-new regression directory required'}
[void][IO.Directory]::CreateDirectory($OutputDirectory)
$utf8=[Text.UTF8Encoding]::new($false)
$script:nativeHelperRoot=$PSScriptRoot;$script:InsideEvidenceOperation=$false;$script:inTimedPhase=$false;$script:writingPhaseCheckpoint=$false
$fixture=Join-Path $OutputDirectory 'synthetic-control';[void][IO.Directory]::CreateDirectory($fixture)
$identities=@(Get-NativeResourceIdentities)
$sha='a'*64;$candidate='b'*40
function Write-ControlFile([string]$relative,[string]$text){$path=Join-Path $fixture $relative;[void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($path));[IO.File]::WriteAllText($path,$text,$utf8);[ordered]@{path=$relative;length=(Get-Item -LiteralPath $path).Length;sha256=(Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant()}}
function Get-ControlledBuildAdmissionTrace([string]$BuildPhase,[int]$PhaseSeconds,[int]$AfterBuildDelaySeconds=0) {
    # Real Build-And-List orchestration/admission. Only subprocess execution and
    # compiler capture I/O are replaced; the explicit clock consumes their maxima.
    $path=Join-Path $script:nativeHelperRoot 'Invoke-NativeResourceVerification.ps1';$tokens=$null;$errors=$null
    $ast=[Management.Automation.Language.Parser]::ParseFile($path,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Production admission AST invalid'}
    $definitions=@{}
    foreach($name in @('Build-And-List','Get-OwnedCommandAdmission')){
        $rows=@($ast.FindAll({param($node)$node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$true))
        if($rows.Count -ne 1){throw "Expected one actual admission function $name"};$definitions[$name]=$rows[0].Extent.Text
    }
    $traceRoot=Join-Path $OutputDirectory ('admission-'+$BuildPhase+'-'+$PhaseSeconds+'-'+$AfterBuildDelaySeconds)
    [void][IO.Directory]::CreateDirectory($traceRoot)
    $module=New-Module -ArgumentList @($definitions,$BuildPhase,$PhaseSeconds,$AfterBuildDelaySeconds,$traceRoot,@(Get-NativeResourceIdentities)) -ScriptBlock {
        param($definitions,$buildPhase,$phaseSeconds,$afterBuildDelay,$traceRoot,$identities)
        foreach($definition in $definitions.Values){. ([scriptblock]::Create($definition))}
        $script:start=[DateTimeOffset]::Parse('2026-10-10T00:00:00Z');$script:clock=$script:start
        $script:phaseDeadline=$script:start.AddSeconds($phaseSeconds);$script:overallDeadline=$script:start.AddSeconds(10340);$script:cleanupReserveSeconds=2940
        $script:Workspace=$traceRoot;$script:EvidenceDirectory=$traceRoot;$script:dotnet='CONTROLLED subprocess seam'
        $script:buildPhase=$buildPhase;$script:afterBuildDelay=$afterBuildDelay;$script:identities=$identities
        $script:trace=[Collections.Generic.List[object]]::new();$script:captured=$false;$script:e=@{discovery=@()}
        if($buildPhase -cne 'default-before'){$script:e.discovery+=,@{phase='default-before';identities=@('LlmFromScratch.Tests.Portable.Control')}}
        function Get-NativeResourceIdentities {$script:identities}
        function Write-Json([string]$path,$value){} # Only evidence I/O seam; real ordinal discovery code runs.
        function Invoke-OwnedCommand([string]$name,[string]$executable,[string[]]$arguments,[switch]$Restoration,[int]$Seconds=600){
            $admission=Get-OwnedCommandAdmission $name -Seconds $Seconds -Restoration:$Restoration -Now $script:clock
            $script:trace.Add(@{name=$name;clockUtc=$script:clock.ToString('o');deadlineUtc=$admission.deadline.ToString('o');availableSeconds=$admission.availableSeconds;
                allowedSeconds=($admission.deadline-$script:clock).TotalSeconds;reason=$admission.reason;effectsStarted=-not [bool]$admission.reason})
            if($admission.reason){throw $admission.reason}
            if($name.EndsWith('-list')){
                $names=@('LlmFromScratch.Tests.Portable.Control');if($script:buildPhase -ceq 'opt-in'){$names+=$script:identities}
                [IO.File]::WriteAllText((Join-Path $script:EvidenceDirectory ($name+'.stdout.log')),($names -join "`n"))
            }
            $script:clock=$admission.deadline.AddSeconds(110+5) # Actual command closure110; finite controller slack5.
            if($name.EndsWith('-build')){$script:clock=$script:clock.AddSeconds($script:afterBuildDelay)}
        }
        function Capture-Build([string]$phase){
            $admission=Get-OwnedCommandAdmission 'controlled-Capture-Build' -Seconds 300 -Now $script:clock
            $script:trace.Add(@{name='Capture-Build';clockUtc=$script:clock.ToString('o');deadlineUtc=$admission.deadline.ToString('o');availableSeconds=$admission.availableSeconds;
                allowedSeconds=($admission.deadline-$script:clock).TotalSeconds;reason=$admission.reason;effectsStarted=-not [bool]$admission.reason})
            if($admission.reason){throw $admission.reason};$script:clock=$admission.deadline.AddSeconds(110+5);$script:captured=$true
        }
        function Invoke-ControlledBuild {
            $failure=$null;try{Build-And-List $script:buildPhase -Restoration:($script:buildPhase -ceq 'default-restoration')}catch{$failure=$_.Exception.Message}
            @{trace=@($script:trace);failure=$failure;captured=$script:captured;baselineCount=@($script:e.discovery|Where-Object phase -CEQ $script:buildPhase).Count;
                discovery=@($script:e.discovery);phaseSeconds=$phaseSeconds;checkpointEndUtc=$script:clock.AddSeconds(70+5).ToString('o');phaseDeadlineUtc=$script:phaseDeadline.ToString('o');resourceEffects=0;scope='Actual production admission/orchestration with controlled clock and lowest-I/O seams; NOT elapsed build/native'}
        }
        Export-ModuleMember -Function @() -Variable @()
    }
    try{$result=& $module {Invoke-ControlledBuild}}finally{Remove-Module -ModuleInfo $module -Force};$result
}
function Get-ControlledProvisionGraph {
    # Execute the production orchestration and its download/seal/promotion wrappers.
    # Only the lowest resource/evidence I/O seam is stubbed; no host roots are probed or created.
    $path=Join-Path $script:nativeHelperRoot 'Invoke-NativeResourceVerification.ps1';$tokens=$null;$errors=$null
    $ast=[Management.Automation.Language.Parser]::ParseFile($path,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Production provisioning AST invalid'}
    $definitions=@{};$hashes=@{}
    foreach($name in @('Provision-PublicResources','Invoke-Download','Seal-OwnedLeaf','Get-OwnedSealAdmission','Promote-OwnedLeaf','Resource-Hashes','Hash-Bytes','Get-NativeUtc')){
        if($name -ceq 'Get-NativeUtc'){$definitions[$name]='function Get-NativeUtc {'+(Get-Command Get-NativeUtc).ScriptBlock.ToString()+'}';continue}
        $rows=@($ast.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$true))
        if($rows.Count -ne 1){throw "Expected one actual provisioning function $name"}
        $definitions[$name]=$rows[0].Extent.Text
        $hashes[$name]=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($utf8.GetBytes($rows[0].Extent.Text))).ToLowerInvariant()
    }
    $module=New-Module -ArgumentList @($definitions,$script:nativeHelperRoot,$OutputDirectory) -ScriptBlock {
        param($definitions,$helperRoot,$tempRoot)
        $script:nativeHelperRoot=$helperRoot;$script:RunnerTemp=$tempRoot;$script:EvidenceDirectory=$tempRoot;$script:InsideEvidenceOperation=$false;$script:operationSequence=0
        $script:utf8=[Text.UTF8Encoding]::new($false);$script:ownedLeaves=[Collections.Generic.List[object]]::new()
        $script:e=@{disk=@{availableBytes=0;requiredBytes=0};downloads=@()};$script:trace=[Collections.Generic.List[object]]::new()
        $script:phaseDeadline=[DateTimeOffset]::UtcNow.AddSeconds(1500)
        $script:virtualRoots=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
        foreach($root in @((Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)) 'Programs/Ollama'),
            (Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)) '.ollama/models'),
            (Join-Path $tempRoot ('s04-native-download-'+$env:GITHUB_RUN_ID+'-'+$env:GITHUB_RUN_ATTEMPT)))){[void]$script:virtualRoots.Add($root)}
        foreach($definition in $definitions.Values){. ([scriptblock]::Create($definition))}
        function Test-Path([string]$LiteralPath){if(-not $script:virtualRoots.Contains($LiteralPath)){throw 'Unexpected physical path query in controlled graph'};$false}
        function Assert-SafeAncestors([string]$path){if(-not $script:virtualRoots.Contains($path)){throw 'Unexpected ancestor probe in controlled graph'}}
        function New-OwnedDirectory([string]$path){if(-not $script:virtualRoots.Contains($path)){throw 'Unexpected directory creation in controlled graph'}}
        function Write-Json([string]$path,$value){} # Metadata is represented by the returned trace, not physical provisioning output.
        function Get-OwnedSealLength([string]$path,[Nullable[long]]$DeclaredLength=$null){$leaf=@($script:ownedLeaves|Where-Object path -CEQ $path);if($leaf.Count -ne 1 -or ($null -ne $DeclaredLength -and $leaf[0].length -ne [long]$DeclaredLength)){throw 'Virtual seal length differs'};[long]$leaf[0].length}
        function Invoke-BoundedEvidenceOperation([string]$operation,[object[]]$arguments,[int]$seconds=600,$SealAdmission=$null){
            $script:operationSequence++;$id=$null;$target=$null
            switch -Exact ($operation){
                'Invoke-DownloadStream' {$pin=$arguments[0];$target=$arguments[1]+'.partial';$id=$pin.id
                    $script:ownedLeaves.Add(@{path=$target;length=[long]$pin.length;sha256=$pin.sha256})
                    $script:e.downloads+=,@{id=$pin.id;startUtc=[DateTimeOffset]::UtcNow.ToString('o')}}
                'Expand-OwnedMember' {$pin=$arguments[1];$target=$arguments[2];$id=$pin.id
                    $script:ownedLeaves.Add(@{path=$target;length=[long]$pin.length;sha256=$pin.sha256})}
                'Write-OwnedManifest' {$target=$arguments[1];$bytes=$script:utf8.GetBytes($arguments[0]);$id='manifest'
                    $script:ownedLeaves.Add(@{path=$target;length=$bytes.Length;sha256=(Hash-Bytes $bytes)})}
                'Seal-OwnedLeaf' {$target=$arguments[0];if(@($script:ownedLeaves|Where-Object path -CEQ $target).Count -ne 1){throw 'Controlled seal lacks unique virtual leaf'}}
                'Promote-OwnedLeaf' {$target=$arguments[1];$leaves=@($script:ownedLeaves|Where-Object path -CEQ $arguments[0])
                    if($leaves.Count -ne 1 -or @($script:ownedLeaves|Where-Object path -CEQ $target).Count){throw 'Controlled promotion custody mismatch'};$leaves[0].path=$target}
                'Resource-Hashes' {if($arguments.Count -ne 1 -or $arguments[0] -cne 'pre'){throw 'Unexpected provisioning hash stage'}}
                default {throw "Unexpected resource operation $operation"}
            }
            $script:trace.Add(@{operation=$operation;resourceId=$id;target=$target;seconds=$seconds})
        }
        function Invoke-ControlledGraph {
            Provision-PublicResources
            $counts=@{};foreach($row in $script:trace){if(-not $counts.ContainsKey($row.operation)){$counts[$row.operation]=0};$counts[$row.operation]++}
            @{completed=$true;counts=$counts;trace=@($script:trace);virtualPromotedLeaves=@($script:ownedLeaves);resourceEffects=0;scope='Actual production control flow with lowest-I/O stubs; NOT real provisioning/native'}
        }
        Export-ModuleMember -Function @() -Variable @() # Never export I/O stubs into the validator/test scope.
    }
    try{$graph=& $module {Invoke-ControlledGraph}}finally{Remove-Module -ModuleInfo $module -Force}
    $graph.runnerSha256=(Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant();$graph.actualFunctionSha256=$hashes
    $graph
}
# Emit a tiny pure managed PE/portable-PDB fixture using PowerShell's bundled Roslyn.
# This never loads the emitted assembly and does not build product/native/model code.
[void](Write-ControlFile 'Tests/obj/Release/net9.0/S04PureEvidenceControl.g.cs' 'public sealed class S04PureEvidenceControl { public int Value(int value) { return value + 1; } }')
$controlSourcePath=Join-Path $fixture 'Tests/obj/Release/net9.0/S04PureEvidenceControl.g.cs'
# The positive record is returned by the actual Windows producer, not a pre-normalized fixture.
$producerTokens=$null;$producerErrors=$null
$producerAst=[Management.Automation.Language.Parser]::ParseFile((Join-Path $PSScriptRoot 'Invoke-NativeResourceVerification.ps1'),[ref]$producerTokens,[ref]$producerErrors)
if($producerErrors.Count){throw 'Production producer AST invalid'}
foreach($name in @('Assert-SafeAncestors','Input-Record')){
    $definition=@($producerAst.FindAll({param($node)$node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$true))
    if($definition.Count -ne 1){throw 'No unique actual producer definition'};. ([scriptblock]::Create($definition[0].Extent.Text))
}
$Workspace=$fixture;$sourceMap=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
$generatedProducerRecord=Input-Record $controlSourcePath
if($generatedProducerRecord.path -cne 'Tests/obj/Release/net9.0/S04PureEvidenceControl.g.cs' -or $generatedProducerRecord.kind -cne 'generated' -or
    $generatedProducerRecord.length -ne (Get-Item -LiteralPath $controlSourcePath).Length -or $generatedProducerRecord.sha256 -cne (Get-FileHash -LiteralPath $controlSourcePath).Hash.ToLowerInvariant()){throw 'Actual generated producer is not canonical or byte bound'}
Add-Type -TypeDefinition 'internal sealed class S04EvidenceRegressionCompilerAvailability {}'
$tree=[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText([IO.File]::ReadAllText($controlSourcePath),$null,$controlSourcePath,$utf8,[Threading.CancellationToken]::None)
$reference=[Microsoft.CodeAnalysis.MetadataReference]::CreateFromFile([object].Assembly.Location)
$options=[Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions]::new([Microsoft.CodeAnalysis.OutputKind]::DynamicallyLinkedLibrary).WithDeterministic($true)
$compilation=[Microsoft.CodeAnalysis.CSharp.CSharpCompilation]::Create('S04PureEvidenceControl',[Microsoft.CodeAnalysis.SyntaxTree[]]@($tree),[Microsoft.CodeAnalysis.MetadataReference[]]@($reference),$options)
$emitOptions=[Microsoft.CodeAnalysis.Emit.EmitOptions]::new().WithDebugInformationFormat([Microsoft.CodeAnalysis.Emit.DebugInformationFormat]::PortablePdb)
$pePath=Join-Path $fixture 'control.dll';$pdbPath=Join-Path $fixture 'control.pdb'
$peStream=[IO.File]::Open($pePath,[IO.FileMode]::CreateNew);$pdbStream=[IO.File]::Open($pdbPath,[IO.FileMode]::CreateNew)
try{$emitted=$compilation.Emit($peStream,$pdbStream,$null,$null,$null,$emitOptions,[Threading.CancellationToken]::None)
    if(-not $emitted.Success){throw ($emitted.Diagnostics -join "`n")}
}finally{$peStream.Dispose();$pdbStream.Dispose()}
$sourcePaths=@('.github/workflows/s03-windows-verification.yml','tools/S04NativeResourceVerification/Invoke-NativeResourceVerification.ps1','tools/S04NativeResourceVerification/Invoke-NativeEvidenceOperation.ps1','tools/S04NativeResourceVerification/Test-NativeResourceEvidence.ps1','tools/S04NativeResourceVerification/Test-NativeResourceEvidence.Tests.ps1','tools/S04NativeResourceVerification/public-resource-pins.json','Tests/CodeDocumentationConfiguredQwenNativeResourceTests.cs','Tests/LlmFromScratch.Tests.csproj','14.ContinuousLearning/Training/ConfiguredQwenVocabularyLease.cs')
$sources=@(foreach($path in $sourcePaths){[ordered]@{path=$path;length=10;candidateLength=10;afterLength=10;sha256=$sha;candidateSha256=$sha;afterSha256=$sha;workflowSha256=$sha;workflowLength=10}})
$filter=($identities | ForEach-Object {'FullyQualifiedName='+$_}) -join '|'
$commands=@(foreach($name in @('restore','default-before-build','default-before-list','opt-in-build','opt-in-list','native3','default-restoration-build','default-restoration-list')){
    [void](Write-ControlFile "$name.stdout.log" 'synthetic command output');[void](Write-ControlFile "$name.stderr.log" '')
    [ordered]@{name=$name;executable='C:\synthetic\dotnet.exe';argv=@('test','Tests/LlmFromScratch.Tests.csproj','--configuration','Release','--no-build','--no-restore','-p:XiigenEnableConfiguredQwenNativeResourceTests=true','--filter',$filter);pid=100;birthUtc='2026-10-10T00:00:00Z';startUtc='2026-10-10T00:00:00Z';endUtc='2026-10-10T00:00:10Z';deadlineUtc='2026-10-10T00:10:00Z';exitCode=0;timedOut=$false;assignedBeforeResume=$true;activeAfterCleanup=0;totalProcesses=1;stdout="$name.stdout.log";stderr="$name.stderr.log"}
})
$discovery=@();$builds=@();$portable=@('LlmFromScratch.Tests.Portable.Control(a: "A")','LlmFromScratch.Tests.Portable.Control(a: "a")')
foreach($phase in @('default-before','opt-in','default-restoration')){
    $names=@($portable);if($phase -ceq 'opt-in'){$names+=,$identities[0];$names+=,$identities[1];$names+=,$identities[2]}
    # Genuine producer can repeat display names. Only portable ordinal set is normalized.
    $raw=@($names)+@($portable[0]);[void](Write-ControlFile "$phase-list.stdout.log" ($raw -join "`n"))
    $discovery+=,[ordered]@{phase=$phase;identities=$names;rawCount=$raw.Count}
    $artifacts=@(foreach($name in @('LlmFromScratch.Tests','ModelServer','ContinuousLearning','TrainableRouter')){foreach($suffix in @('dll','pdb')){
        $relative="managed/$phase/$name.$suffix";$target=Join-Path $fixture $relative;[void][IO.Directory]::CreateDirectory([IO.Path]::GetDirectoryName($target))
        [IO.File]::Copy($(if($suffix -ceq 'dll'){$pePath}else{$pdbPath}),$target,$false)
        [ordered]@{path=$relative;length=(Get-Item -LiteralPath $target).Length;sha256=(Get-FileHash -LiteralPath $target).Hash.ToLowerInvariant()}
    }})
    $pdbBindings=@(foreach($name in @('LlmFromScratch.Tests','ModelServer','ContinuousLearning','TrainableRouter')){
        $binding=Read-NativeManagedPdbBinding (Join-Path $fixture "managed/$phase/$name.dll") (Join-Path $fixture "managed/$phase/$name.pdb") {
            param($path) Input-Record $path
        };$binding.name=$name;$binding
    })
    $cscCommand='csc.exe Tests/Control.cs';if($phase -ceq 'opt-in'){$cscCommand+=' CodeDocumentationConfiguredQwenNativeResourceTests.cs'}
    $cscRows=@(@{type='TaskCommandLineEventArgs';task='Csc';projectFile=(Join-Path $fixture 'Tests/LlmFromScratch.Tests.csproj');commandLine=$cscCommand},@{type='BuildFinishedEventArgs';succeeded=$true}) | ConvertTo-Json
    $builds+=,[ordered]@{phase=$phase;binlog=(Write-ControlFile "$phase.binlog" 'SYNTHETIC binlog artifact');cscEvidence=(Write-ControlFile "$phase-csc.json" $cscRows);testsCscCount=1;nativeIncluded=[int]($phase -ceq 'opt-in');artifacts=$artifacts;
        compileInputs=@(@{path='Tests/Control.cs';kind='tracked';sha256=$sha;candidateSha256=$sha;length=10;candidateLength=10},$generatedProducerRecord);
        pdbBindings=$pdbBindings;
        testsDllSha256=$artifacts[0].sha256;testsPdbSha256=$artifacts[1].sha256}
}
$pins=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'public-resource-pins.json') -Raw | ConvertFrom-Json -Depth 30
$provisionGraph=Get-ControlledProvisionGraph
[IO.File]::WriteAllText((Join-Path $OutputDirectory 'provision-orchestration.json'),(($provisionGraph|ConvertTo-Json -Depth 30)+"`n"),$utf8)
$resources=@(foreach($stage in @('pre','post')){foreach($pin in @($pins.members)+@($pins.blobs)+@($pins.manifest)){[ordered]@{stage=$stage;id=$pin.id;sha256=$pin.sha256;length=$pin.length;fullHash=$true}}})
$downloads=@(foreach($pin in @($pins.archive)+@($pins.blobs)){[ordered]@{id=$pin.id;url=$pin.url;sha256=$pin.sha256;length=$pin.length;diskSha256=$pin.sha256;diskLength=$pin.length;fullHash=$true;completed=$true}})
$control=[ordered]@{schema=1;gate='S04.04A-NATIVE-RESOURCE';candidate=$candidate;workflow=$candidate;head=$candidate;runId='synthetic';runAttempt='1';workflowRef='synthetic@candidate';runner='synthetic';sdk='9.0.synthetic';workspace=$fixture;sources=$sources;
    regression=@{exitCode=0;passed=30};commands=$commands;discovery=$discovery;builds=$builds;commandBinarySha256=$builds[1].testsDllSha256;commandPdbSha256=$builds[1].testsPdbSha256;resources=$resources;downloads=$downloads;disk=@{availableBytes=40GB;requiredBytes=8097805929L+6GB};
    restoration=@{completed=$true;nativeCount=0};cleanup=@{completed=$true;failures=@();activeProcesses=0;leaves=@(1..13 | ForEach-Object {@{owned=$true;absentBefore=$true;absentAfter=$true;reparse=$false;identity='00000001:0000000000000001';creationTicks=1;sha256=$sha;length=1}})};fixture=@{beforeDigest=$sha;afterDigest=$sha};failures=@()}
$definition=@();$entry=@();$result=@()
# Complete synthetic timing control. These rows test validation only, not real native execution.
$policy=Get-NativeTimingPolicy;$epoch=[DateTimeOffset]::Parse('2026-10-10T00:00:00Z')
$phaseStarts=@{prepare=-360;default=-300;provision=-240;optin=-180;preflight=-120;native=0;restoration=11;closure=22}
$phaseRows=@(foreach($name in @('prepare','default','provision','optin','preflight','native','restoration','closure')){
    $begin=$epoch.AddSeconds($phaseStarts[$name]);@{name=$name;startUtc=$begin.ToString('o');endUtc=$begin.AddSeconds(10).ToString('o');deadlineUtc=$begin.AddSeconds($policy.phases[$name]).ToString('o');completed=$true;error=$null}
})
$control.timing=@{policy=$policy;startUtc=$epoch.AddSeconds(-360).ToString('o');regularDeadlineUtc=$epoch.AddSeconds(7040).ToString('o');
    restorationDeadlineUtc=$epoch.AddSeconds(8960).ToString('o');closureDeadlineUtc=$epoch.AddSeconds(9560).ToString('o');overallDeadlineUtc=$epoch.AddSeconds(9980).ToString('o');endUtc=$epoch.AddSeconds(32).ToString('o');phases=$phaseRows;operations=@();admissions=@();metadataWrites=@();unknownEffects=$false}
foreach($phase in $phaseRows){$begin=$epoch.AddSeconds($phaseStarts[$phase.name]);$control.timing.metadataWrites+=,@{path='timing.json';phase=$phase.name;startUtc=$begin.ToString('o');deadlineUtc=$begin.AddSeconds(30).ToString('o');closureDeadlineUtc=$begin.AddSeconds(70).ToString('o');endUtc=$begin.AddSeconds(1).ToString('o');pid=100;birthUtc=$begin.ToString('o');processEndUtc=$begin.AddSeconds(1).ToString('o');exitCode=0;timedOut=$false;assignedBeforeResume=$true;activeAfterCleanup=0;totalProcesses=1;error=$null}}
foreach($command in $commands){
    $phase=switch($command.name){'restore'{'prepare'}'default-before-build'{'default'}'default-before-list'{'default'}'opt-in-build'{'optin'}'opt-in-list'{'optin'}'native3'{'native'}default{'restoration'}}
    $begin=$epoch.AddSeconds($phaseStarts[$phase]);$allowance=if($phase -ceq 'native'){$policy.nativeGroup.commandSeconds}else{[Math]::Min(600,$policy.phases[$phase]-180)}
    $command.phase=$phase;$command.startUtc=$begin.ToString('o');$command.birthUtc=$begin.ToString('o');$command.endUtc=$begin.AddSeconds(10).ToString('o');$command.processEndUtc=$command.endUtc
    $command.deadlineUtc=$begin.AddSeconds($allowance).ToString('o');$command.closureDeadlineUtc=$begin.AddSeconds($allowance+110).ToString('o')
}
$opCounts=[ordered]@{'Capture-Sources'=1;'Capture-Build'=3;'Fixture-Inventory'=2;'Capture-SourceAfter'=1;'Cleanup-Owned'=1}
foreach($name in $provisionGraph.counts.Keys){$opCounts[$name]=$provisionGraph.counts[$name]}
$opCounts['Resource-Hashes']++ # Provision's actual pre-hash plus the unchanged closure post-hash.
$opIndex=0
foreach($op in $opCounts.Keys){for($j=0;$j -lt $opCounts[$op];$j++){
    $opIndex++;$name="synthetic-operation-$opIndex";$phase=switch($op){'Capture-Sources'{'prepare'}'Capture-Build'{@('default','optin','restoration')[$j]}'Fixture-Inventory'{@('preflight','closure')[$j]}'Resource-Hashes'{@('provision','closure')[$j]}'Capture-SourceAfter'{'closure'}'Cleanup-Owned'{'closure'}default{'provision'}}
    $offset=switch($op){'Invoke-DownloadStream'{0}'Seal-OwnedLeaf'{3}'Promote-OwnedLeaf'{5}default{0}}
    $begin=$epoch.AddSeconds($phaseStarts[$phase]+$offset);$finish=$begin.AddSeconds(1);$deadline=$begin.AddSeconds(30)
    [void](Write-ControlFile "$name.stdout.log" 'synthetic operation');[void](Write-ControlFile "$name.stderr.log" '')
    $receipt=Write-ControlFile "$name.receipt.json" (@{operation=$op;candidate=$candidate;deadlineUtc=$deadline.ToString('o');endUtc=$finish.ToString('o');requestSha256=$sha;requestIdentity='01234567:0123456789abcdef';helperSha256=$sha;error=$null}|ConvertTo-Json -Compress)
    $commands+=,@{name=$name;phase=$phase;executable='C:\synthetic\pwsh.exe';argv=@('-File','tools/S04NativeResourceVerification/Invoke-NativeEvidenceOperation.ps1');pid=100;birthUtc=$begin.ToString('o');startUtc=$begin.ToString('o');endUtc=$finish.ToString('o');processEndUtc=$finish.ToString('o');deadlineUtc=$deadline.ToString('o');closureDeadlineUtc=$deadline.AddSeconds(110).ToString('o');exitCode=0;timedOut=$false;assignedBeforeResume=$true;activeAfterCleanup=0;totalProcesses=1;stdout="$name.stdout.log";stderr="$name.stderr.log"}
    $control.timing.operations+=,@{name=$name;operation=$op;phase=$phase;startUtc=$begin.ToString('o');endUtc=$finish.ToString('o');deadlineUtc=$deadline.ToString('o');helperSha256=$sha;requestSha256=$sha;requestIdentity='01234567:0123456789abcdef';receipt=$receipt;completed=$true;error=$null}
}}
$control.commands=$commands
for($i=0;$i -lt $downloads.Count;$i++){
    $download=$downloads[$i];$begin=$epoch.AddSeconds(-240)
    $download.startUtc=$begin.ToString('o');$download.streamEndUtc=$begin.AddMilliseconds(500).ToString('o');$download.transferEndUtc=$begin.AddMilliseconds(800).ToString('o');$download.endUtc=$begin.AddSeconds(6).ToString('o')
    foreach($pair in @(@('streamCommand','Invoke-DownloadStream'),@('sealCommand','Seal-OwnedLeaf'),@('promotionCommand','Promote-OwnedLeaf'))){$download[$pair[0]]=@($control.timing.operations|Where-Object operation -CEQ $pair[1])[$i].name}
}
for($i=0;$i -lt 3;$i++){
    $id=('00000000-0000-0000-0000-{0:D12}' -f ($i+1));$executionId=('10000000-0000-0000-0000-{0:D12}' -f ($i+1));$method=$identities[$i].Substring($identities[$i].LastIndexOf('.')+1)
    # isValid intentionally absent: genuine xUnit/VSTest TestMethod does not emit it.
    $codeBase=[Security.SecurityElement]::Escape((Join-Path $fixture 'Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll'))
    $definition+="<UnitTest id='$id' name='$($identities[$i])' storage='$codeBase'><Execution id='$executionId'/><TestMethod codeBase='$codeBase' adapterTypeName='executor://xunit/VsTestRunner2/netcoreapp' className='LlmFromScratch.Tests.CodeDocumentationConfiguredQwenNativeResourceTests' name='$method'/></UnitTest>"
    $entry+="<TestEntry testId='$id' executionId='$executionId'/>"
    $result+="<UnitTestResult testId='$id' executionId='$executionId' testName='$($identities[$i])' outcome='Passed' startTime='2026-10-10T00:00:02Z' endTime='2026-10-10T00:00:03Z' duration='00:00:01'/>"
}
$trx="<TestRun xmlns='http://microsoft.com/schemas/VisualStudio/TeamTest/2010'><Times start='2026-10-10T00:00:01Z' finish='2026-10-10T00:00:04Z'/><TestDefinitions>$($definition -join '')</TestDefinitions><TestEntries>$($entry -join '')</TestEntries><Results>$($result -join '')</Results><ResultSummary outcome='Completed'><Counters total='3' executed='3' passed='3' failed='0' notExecuted='0'/></ResultSummary></TestRun>"
$controlJson=$control | ConvertTo-Json -Depth 100
$cases=[Collections.Generic.List[object]]::new()
function Case([string]$name,[string]$code,[scriptblock]$mutate,[string]$exactDetail=''){$cases.Add(@{name=$name;code=$code;mutate=$mutate;exactDetail=$exactDetail})}
Case 'download stale full-seal interval refused' 'TIMING' {param($e,$x) $e.downloads[0] | Add-Member -NotePropertyName startUtc -NotePropertyValue '2026-10-10T00:00:00Z' -Force; $e.downloads[0] | Add-Member -NotePropertyName endUtc -NotePropertyValue '2026-10-10T03:00:00Z' -Force}
Case 'missing finite timing policy refused' 'TIMING' {param($e,$x) $e.timing.policy=$null}
Case 'overdue synchronous phase refused' 'TIMING' {param($e,$x) $e.timing.phases[2].endUtc='2026-10-10T03:00:00Z'}
Case 'unknown operation effects refuse closure' 'TIMING' {param($e,$x) $e.timing.unknownEffects=$true}
Case 'absent owned sealing receipt refused' 'TIMING' {param($e,$x) $e.timing.operations=@($e.timing.operations|Where-Object operation -CNE 'Seal-OwnedLeaf')}
Case 'stream-only completion refused' 'TIMING' {param($e,$x) $e.downloads[0].endUtc=$e.downloads[0].streamEndUtc}
Case 'short native allowance refused' 'TIMING' {param($e,$x) $e.commands[5].deadlineUtc='2026-10-10T00:09:00Z'}
Case 'short build allowance refused' 'TIMING' {param($e,$x) $e.commands[1].deadlineUtc=(Get-NativeUtc $e.commands[1].startUtc).AddSeconds(420).ToString('o');$e.commands[1].closureDeadlineUtc=(Get-NativeUtc $e.commands[1].deadlineUtc).AddSeconds(110).ToString('o')} 'Build/discovery requires full600 admission'
Case 'short discovery allowance refused' 'TIMING' {param($e,$x) $e.commands[2].deadlineUtc=(Get-NativeUtc $e.commands[2].startUtc).AddSeconds(420).ToString('o');$e.commands[2].closureDeadlineUtc=(Get-NativeUtc $e.commands[2].deadlineUtc).AddSeconds(110).ToString('o')} 'Build/discovery requires full600 admission'
Case 'foreign helper binding refused' 'TIMING' {param($e,$x) $e.timing.operations[0].helperSha256='0'*64}
Case 'compact receipt absence refused' 'TIMING' {param($e,$x) $e.timing.operations[0].receipt=$null}
Case 'missing phase evidence writer refused' 'TIMING' {param($e,$x) $e.timing.metadataWrites=@($e.timing.metadataWrites|Where-Object phase -CNE 'native')}
Case 'overdue evidence writer refused' 'TIMING' {param($e,$x) $e.timing.metadataWrites[0].endUtc='2026-10-10T03:00:00Z'}
Case 'missing promotion from actual provisioning graph refused' 'TIMING' {param($e,$x)
    $promotion=@($e.timing.operations|Where-Object operation -CEQ 'Promote-OwnedLeaf')[-1]
    $e.timing.operations=@($e.timing.operations|Where-Object name -CNE $promotion.name);$e.commands=@($e.commands|Where-Object name -CNE $promotion.name)} 'Missing/duplicate owned operation Promote-OwnedLeaf'
Case 'extra bound promotion beyond actual provisioning graph refused' 'TIMING' {param($e,$x)
    $promotion=@($e.timing.operations|Where-Object operation -CEQ 'Promote-OwnedLeaf')[-1];$command=@($e.commands|Where-Object name -CEQ $promotion.name)[0]
    $extra=$promotion|ConvertTo-Json -Depth 10|ConvertFrom-Json -Depth 10;$extraCommand=$command|ConvertTo-Json -Depth 10|ConvertFrom-Json -Depth 10
    $extra.name+='-extra';$extraCommand.name=$extra.name;$extraCommand.stdout=$extra.name+'.stdout.log';$extraCommand.stderr=$extra.name+'.stderr.log'
    [void](Write-ControlFile $extraCommand.stdout 'synthetic extra promotion');[void](Write-ControlFile $extraCommand.stderr '')
    $receipt=Get-Content -LiteralPath (Join-Path $fixture $promotion.receipt.path) -Raw
    $extra.receipt=Write-ControlFile ($extra.name+'.receipt.json') $receipt
    $e.timing.operations+=,$extra;$e.commands+=,$extraCommand} 'Missing/duplicate owned operation Promote-OwnedLeaf'
Case 'zero counters' 'TRX' {param($e,$x) $x.TestRun.ResultSummary.Counters.SetAttribute('total','0')}
Case 'partial result' 'TRX' {param($e,$x) [void]$x.TestRun.Results.RemoveChild($x.TestRun.Results.FirstChild)}
Case 'duplicate definition ID' 'TRX' {param($e,$x) $x.TestRun.TestDefinitions.UnitTest[1].SetAttribute('id',$x.TestRun.TestDefinitions.UnitTest[0].id)}
Case 'duplicate execution ID' 'TRX' {param($e,$x) $x.TestRun.TestDefinitions.UnitTest[1].Execution.SetAttribute('id',$x.TestRun.TestDefinitions.UnitTest[0].Execution.id)}
Case 'orphan entry' 'TRX' {param($e,$x) $x.TestRun.TestEntries.TestEntry[0].SetAttribute('testId','20000000-0000-0000-0000-000000000001')}
Case 'foreign method' 'TRX' {param($e,$x) $x.TestRun.TestDefinitions.UnitTest[0].TestMethod.SetAttribute('name','ForeignFact')}
Case 'foreign codebase' 'TRX' {param($e,$x) $x.TestRun.TestDefinitions.UnitTest[0].TestMethod.SetAttribute('codeBase','C:\foreign\LlmFromScratch.Tests.dll')}
Case 'skipped result' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('outcome','Skipped')}
Case 'NotExecuted result' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('outcome','NotExecuted')}
Case 'foreign XML element' 'TRX' {param($e,$x) [void]$x.TestRun.AppendChild($x.CreateElement('Foreign','urn:foreign'))}
Case 'foreign same-namespace element' 'TRX' {param($e,$x) [void]$x.TestRun.AppendChild($x.CreateElement('Foreign',$x.DocumentElement.NamespaceURI))}
Case 'nested result' 'TRX' {param($e,$x) [void]$x.TestRun.AppendChild($x.TestRun.Results.FirstChild.CloneNode($true))}
Case 'nested definition' 'TRX' {param($e,$x) [void]$x.TestRun.AppendChild($x.TestRun.TestDefinitions.FirstChild.CloneNode($true))}
Case 'nested entry' 'TRX' {param($e,$x) [void]$x.TestRun.AppendChild($x.TestRun.TestEntries.FirstChild.CloneNode($true))}
Case 'foreign namespace' 'TRX' {param($e,$x) $script:mutatedTrx=$x.OuterXml.Replace('http://microsoft.com/schemas/VisualStudio/TeamTest/2010','urn:wrong')}
Case 'result timestamp outside command' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('startTime','2026-10-09T00:00:02Z')}
Case 'duration exceeds elapsed run at completed result' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('duration','00:00:02.0000001')} 'Result duration/command/TRX run interval mismatch.'
Case 'negative result duration' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('duration','-00:00:01')} 'Result duration/command/TRX run interval mismatch.'
Case 'malformed result duration' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('duration','not-a-duration')}
Case 'duration exceeds whole command' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('duration','00:00:11')} 'Result duration/command/TRX run interval mismatch.'
Case 'reversed result receipt timestamps' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('endTime','2026-10-10T00:00:01.9Z')} 'Result duration/command/TRX run interval mismatch.'
Case 'malformed result receipt timestamp' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('endTime','not-a-timestamp')}
Case 'completed receipt after TRX run inside command' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('endTime','2026-10-10T00:00:05Z')} 'Result duration/command/TRX run interval mismatch.'
Case 'completed receipt after native command' 'TRX' {param($e,$x) $x.TestRun.Results.UnitTestResult[0].SetAttribute('endTime','2026-10-10T00:00:10.0000001Z')} 'Result duration/command/TRX run interval mismatch.'
Case 'nonzero optional skipped' 'TRX' {param($e,$x) $x.TestRun.ResultSummary.Counters.SetAttribute('skipped','1')}
Case 'failed RunInfo' 'TRX' {param($e,$x) $info=$x.CreateElement('RunInfo',$x.DocumentElement.NamespaceURI);$info.SetAttribute('outcome','Error');[void]$x.TestRun.ResultSummary.AppendChild($info)}
Case 'source SHA drift' 'SOURCE_IDENTITY' {param($e,$x) $e.sources[0].afterSha256='d'*64}
Case 'workflow SHA differs' 'SOURCE_IDENTITY' {param($e,$x) $e.workflow='d'*40}
Case 'stale binary' 'STALE_BINARY' {param($e,$x) $e.commandBinarySha256='d'*64}
Case 'wrong PDB binding' 'BINARY_BINDING' {param($e,$x) $e.builds[1].pdbBindings[0].dllCodeViewId='d'*32}
Case 'generated recorded backslash refused' 'BINARY_BINDING' {param($e,$x) $e.builds[1].pdbBindings[0].documents[0].path=$e.builds[1].pdbBindings[0].documents[0].path.Replace('/','\')}
Case 'generated foreign path refused' 'BINARY_BINDING' {param($e,$x) $e.builds[1].pdbBindings[0].documents[0].path='Tests/obj/foreign.g.cs'}
Case 'generated path case refused' 'BINARY_BINDING' {param($e,$x) $e.builds[1].pdbBindings[0].documents[0].path=$e.builds[1].pdbBindings[0].documents[0].path.ToUpperInvariant()}
Case 'generated duplicate path refused' 'BINARY_BINDING' {param($e,$x) $e.builds[1].pdbBindings[0].documents+=,$e.builds[1].pdbBindings[0].documents[0]}
Case 'generated kind refused' 'BINARY_BINDING' {param($e,$x) $e.builds[1].pdbBindings[0].documents[0].kind='tracked'}
Case 'generated source hash refused' 'BINARY_BINDING' {param($e,$x) $e.builds[1].pdbBindings[0].documents[0].sha256='d'*64}
Case 'generated checksum refused' 'BINARY_BINDING' {param($e,$x) $e.builds[1].pdbBindings[0].documents[0].checksum='d'*64}
Case 'ordinary601 refused by both validators' 'COMMAND' {param($e,$x) $c=$e.commands[1];$c.deadlineUtc=(Get-NativeUtc $c.startUtc).AddSeconds(601).ToString('o');$c.closureDeadlineUtc=(Get-NativeUtc $c.deadlineUtc).AddSeconds(110).ToString('o')}
Case 'native wrong phase refused' 'TIMING' {param($e,$x) $e.commands[5].phase='default'}
Case 'native duplicate command refused' 'COMMAND' {param($e,$x) $e.commands+=,$e.commands[5]}
Case 'alternate native casing600 refused' 'COMMAND' {param($e,$x) $c=$e.commands[5];$c.name='Native3';$c.deadlineUtc=(Get-NativeUtc $c.startUtc).AddSeconds(600).ToString('o');$c.closureDeadlineUtc=(Get-NativeUtc $c.deadlineUtc).AddSeconds(110).ToString('o')}
Case 'alternate native name600 refused' 'COMMAND' {param($e,$x) $c=$e.commands[5];$c.name='native-3';$c.deadlineUtc=(Get-NativeUtc $c.startUtc).AddSeconds(600).ToString('o');$c.closureDeadlineUtc=(Get-NativeUtc $c.deadlineUtc).AddSeconds(110).ToString('o')}
Case 'alternate native casing1800 refused' 'COMMAND' {param($e,$x) $e.commands[5].name='Native3'}
Case 'alternate native name1800 refused' 'COMMAND' {param($e,$x) $e.commands[5].name='native-3'}
Case 'generated physical source length and checksum drift refused' 'BINARY_BINDING' {param($e,$x) [IO.File]::AppendAllText($controlSourcePath,'//physical length drift',$utf8)}
Case 'raw Csc contradicted by summary' 'BINARY_BINDING' {param($e,$x)
    $rows=@(@{type='TaskCommandLineEventArgs';task='Csc';projectFile=(Join-Path $fixture 'Tests/LlmFromScratch.Tests.csproj');commandLine='csc.exe Tests/Control.cs'},@{type='BuildFinishedEventArgs';succeeded=$true}) | ConvertTo-Json
    $e.builds[1].cscEvidence=Write-ControlFile 'opt-in-csc.json' $rows
}
Case 'PDB content drift with refreshed custody hash' 'BINARY_BINDING' {param($e,$x)
    $path=Join-Path $fixture 'managed/opt-in/LlmFromScratch.Tests.pdb';$bytes=[IO.File]::ReadAllBytes($path);$bytes[$bytes.Length-1]=$bytes[$bytes.Length-1] -bxor 1
    [IO.File]::WriteAllBytes($path,$bytes);$e.builds[1].artifacts[1].sha256=(Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant()
    $e.builds[1].testsPdbSha256=$e.builds[1].artifacts[1].sha256;$e.commandPdbSha256=$e.builds[1].artifacts[1].sha256
}
Case 'resource drift' 'RESOURCE_HASH' {param($e,$x) $e.resources[12].sha256='d'*64}
Case 'partial download masquerades as full' 'DOWNLOAD' {param($e,$x) $e.downloads[0].fullHash=$false}
Case 'stream hash differs from disk hash' 'DOWNLOAD' {param($e,$x) $e.downloads[0].diskSha256='d'*64}
Case 'restoration incomplete' 'RESTORATION' {param($e,$x) $e.restoration.completed=$false}
Case 'foreign cleanup leaf' 'CLEANUP' {param($e,$x) $e.cleanup.leaves[0].owned=$false}
Case 'missing physical cleanup identity' 'CLEANUP' {param($e,$x) $e.cleanup.leaves[0].identity=$null}
Case 'process still running' 'COMMAND' {param($e,$x) $e.commands[5].activeAfterCleanup=1}
Case 'no exit' 'COMMAND' {param($e,$x) $e.commands[5].exitCode=$null}
Case 'timeout' 'COMMAND' {param($e,$x) $e.commands[5].timedOut=$true}
Case 'wrong command project' 'COMMAND_FILTER' {param($e,$x) $e.commands[5].argv[1]='Other.csproj'}
Case 'missing no-build' 'COMMAND_FILTER' {param($e,$x) $e.commands[5].argv=@($e.commands[5].argv | Where-Object {$_ -cne '--no-build'})}
Case 'fixture history changed' 'FIXTURE_INVENTORY' {param($e,$x) $e.fixture.afterDigest='d'*64}
Case 'changed casing identity' 'DISCOVERY' {param($e,$x) $e.discovery[2].identities[1]='LlmFromScratch.Tests.Portable.Control(a: "B")'}
Case 'loader even exit zero' 'LOADER' {param($e,$x) [void](Write-ControlFile 'native3.stderr.log' 'An Application Control policy has blocked this file. (0x800711C7)')}
# Sanitized R4 producer shape: no Results/Definitions/Entries, zero counters, Warning RunInfo, outer exit0.
Case 'actual R4 zero loader shape' 'TRX' {param($e,$x) $script:mutatedTrx="<TestRun xmlns='http://microsoft.com/schemas/VisualStudio/TeamTest/2010'><Times creation='2026-10-10T00:00:04Z' queuing='2026-10-10T00:00:04Z' start='2026-10-10T00:00:01Z' finish='2026-10-10T00:00:04Z'/><TestSettings name='default'/><TestLists/><ResultSummary outcome='Completed'><Counters total='0' executed='0' passed='0' failed='0' error='0' timeout='0' aborted='0' inconclusive='0' passedButRunAborted='0' notRunnable='0' notExecuted='0' disconnected='0' warning='0' completed='0' inProgress='0' pending='0'/><Output><StdOut>xUnit.net VSTest Adapter v2.8.2</StdOut></Output><RunInfos><RunInfo computerName='sanitized' outcome='Warning' timestamp='2026-10-10T00:00:04Z'><Text>Skipping: LlmFromScratch.Tests (could not load dependent assembly): An Application Control policy has blocked this file. (0x800711C7)</Text></RunInfo></RunInfos></ResultSummary></TestRun>"}
# F01: keep valid direct rows while injecting known elements under invalid parents.
Case 'orphan root Execution' 'TRX' {param($e,$x) [void]$x.TestRun.AppendChild($x.TestRun.TestDefinitions.UnitTest[0].Execution.CloneNode($true))}
Case 'root Output nested contradictory summary' 'TRX' {param($e,$x)
    $output=$x.CreateElement('Output',$x.DocumentElement.NamespaceURI);$summary=$x.TestRun.ResultSummary.CloneNode($true)
    $summary.SetAttribute('outcome','Failed');$summary.Counters.SetAttribute('total','0');[void]$output.AppendChild($summary);[void]$x.TestRun.AppendChild($output)
}
Case 'valid Output parent nested contradictory summary' 'TRX' {param($e,$x)
    $output=$x.CreateElement('Output',$x.DocumentElement.NamespaceURI);$summary=$x.TestRun.ResultSummary.CloneNode($true)
    $summary.SetAttribute('outcome','Failed');$summary.Counters.SetAttribute('total','0');[void]$output.AppendChild($summary);[void]$x.TestRun.ResultSummary.AppendChild($output)
}
Case 'orphan root Counters' 'TRX' {param($e,$x) [void]$x.TestRun.AppendChild($x.TestRun.ResultSummary.Counters.CloneNode($true))}
Case 'duplicate optional TestSettings' 'TRX' {param($e,$x) foreach($i in 1..2){[void]$x.TestRun.AppendChild($x.CreateElement('TestSettings',$x.DocumentElement.NamespaceURI))}}
Case 'duplicate result Output' 'TRX' {param($e,$x) foreach($i in 1..2){[void]$x.TestRun.Results.UnitTestResult[0].AppendChild($x.CreateElement('Output',$x.DocumentElement.NamespaceURI))}}
Case 'duplicate summary RunInfos' 'TRX' {param($e,$x) foreach($i in 1..2){[void]$x.TestRun.ResultSummary.AppendChild($x.CreateElement('RunInfos',$x.DocumentElement.NamespaceURI))}}
# F02: command interval stays valid while the exact direct TRX interval does not contain results.
Case 'Times finish before results' 'TRX' {param($e,$x) $x.TestRun.Times.SetAttribute('finish','2026-10-10T00:00:01Z')}
Case 'Times start after results start' 'TRX' {param($e,$x) $x.TestRun.Times.SetAttribute('start','2026-10-10T00:00:02.5Z')}
Case 'one result before Times start' 'TRX' {param($e,$x)
    $x.TestRun.Results.UnitTestResult[0].SetAttribute('startTime','2026-10-10T00:00:00Z')
    $x.TestRun.Results.UnitTestResult[0].SetAttribute('endTime','2026-10-10T00:00:01Z')
}
# F03: custody bytes remain genuine while scalar summaries or exact inventories are corrupted.
Case 'build DLL hash differs from saved bytes' 'BINARY_BINDING' {param($e,$x) $e.builds[1].testsDllSha256='d'*64}
Case 'build PDB hash differs from saved bytes' 'BINARY_BINDING' {param($e,$x) $e.builds[1].testsPdbSha256='d'*64}
Case 'correlated build and command DLL hash corruption' 'BINARY_BINDING' {param($e,$x) $e.builds[1].testsDllSha256='d'*64;$e.commandBinarySha256='d'*64}
Case 'correlated build and command PDB hash corruption' 'BINARY_BINDING' {param($e,$x) $e.builds[1].testsPdbSha256='d'*64;$e.commandPdbSha256='d'*64}
Case 'command PDB hash differs from saved bytes' 'STALE_BINARY' {param($e,$x) $e.commandPdbSha256='d'*64}
Case 'missing managed DLL inventory row' 'BINARY_BINDING' {param($e,$x) $e.builds[1].artifacts=@($e.builds[1].artifacts | Where-Object path -CNE 'managed/opt-in/LlmFromScratch.Tests.dll')}
Case 'duplicate DLL replaces PDB inventory row' 'BINARY_BINDING' {param($e,$x) $e.builds[1].artifacts[1]=$e.builds[1].artifacts[0]}
Case 'foreign phase DLL inventory row' 'BINARY_BINDING' {param($e,$x) $e.builds[1].artifacts[0]=$e.builds[0].artifacts[0]}
Case 'foreign owner PDB inventory row' 'BINARY_BINDING' {param($e,$x) $e.builds[1].artifacts[1]=$e.builds[1].artifacts[3]}
Case 'default build DLL hash differs from saved bytes' 'BINARY_BINDING' {param($e,$x) $e.builds[0].testsDllSha256='d'*64}
Case 'restoration build PDB hash differs from saved bytes' 'BINARY_BINDING' {param($e,$x) $e.builds[2].testsPdbSha256='d'*64}
Case 'duplicate build phase' 'BINARY_BINDING' {param($e,$x) $e.builds+=,$e.builds[1]}
$rows=@();$failed=0
$positiveRows=@()
$script:graphBaselineResult=$null
function Check-Control {
    [IO.File]::WriteAllText($controlSourcePath,'public sealed class S04PureEvidenceControl { public int Value(int value) { return value + 1; } }',$utf8)
    [IO.File]::Copy($pdbPath,(Join-Path $fixture 'managed/opt-in/LlmFromScratch.Tests.pdb'),$true)
    $rows=@(@{type='TaskCommandLineEventArgs';task='Csc';projectFile=(Join-Path $fixture 'Tests/LlmFromScratch.Tests.csproj');commandLine='csc.exe Tests/Control.cs CodeDocumentationConfiguredQwenNativeResourceTests.cs'},@{type='BuildFinishedEventArgs';succeeded=$true}) | ConvertTo-Json
    [void](Write-ControlFile 'opt-in-csc.json' $rows)
    [void](Write-ControlFile 'native3.trx' $trx);[void](Write-ControlFile 'native3.stderr.log' '')
    $e=$controlJson | ConvertFrom-Json -Depth 100
    $result=Test-NativeResourceEvidence -Directory $fixture -Evidence $e
    if($null -eq $script:graphBaselineResult){$script:graphBaselineResult=$result
        [IO.File]::WriteAllText((Join-Path $OutputDirectory 'provision-graph-validator-positive.json'),((@{graph=$provisionGraph.counts;fixture=$opCounts;result=$result}|ConvertTo-Json -Depth 20)+"`n"),$utf8)}
    if(-not $result.valid){throw "Valid structural synthetic control rejected: $($result | ConvertTo-Json -Depth 20 -Compress)"}
}
function Test-Full600GuardCounterfactual {
    # Keep the complete valid fixture. Remove only the full600 timing guard in
    # an isolated module, never in the live validator or parent function scope.
    $validatorPath=Join-Path $PSScriptRoot 'Test-NativeResourceEvidence.ps1'
    $beforeHash=(Get-FileHash -LiteralPath $validatorPath).Hash.ToLowerInvariant();$tokens=$null;$errors=$null
    $ast=[Management.Automation.Language.Parser]::ParseFile($validatorPath,[ref]$tokens,[ref]$errors)
    if($errors.Count){throw 'Counterfactual validator AST invalid'}
    $definitions=@($ast.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq 'Test-NativeTimingEvidence'},$true))
    if($definitions.Count -ne 1){throw 'No unique actual timing validator'}
    $guards=@($definitions[0].FindAll({param($n)$n -is [Management.Automation.Language.IfStatementAst] -and $n.Extent.Text.EndsWith("{throw 'Build/discovery requires full600 admission'}")},$true))
    if($guards.Count -ne 1){throw 'No unique full600 guard for counterfactual'}
    $replacement=$definitions[0].Extent.Text.Replace($guards[0].Extent.Text,'# Isolated counterfactual: only full600 build/discovery guard removed.')
    $module=New-Module -ArgumentList @($validatorPath,$replacement) -ScriptBlock {
        param($validatorPath,$replacement)
        . $validatorPath
        . ([scriptblock]::Create($replacement))
        Export-ModuleMember -Function @() -Variable @()
    }
    $proof=[ordered]@{scope='Complete synthetic positive fixture through actual validator; only one timing guard removed in isolated module; NOT native runtime';
        validatorSha256=$beforeHash;removedGuard=$guards[0].Extent.Text;positiveCurrent=$null;positiveRemoved=$null;mutations=@();liveValidatorUnchanged=$false;passed=$false}
    try {
        $proof.positiveCurrent=Test-NativeResourceEvidence -Directory $fixture -Evidence ($controlJson|ConvertFrom-Json -Depth 100)
        $proof.positiveRemoved=& $module {param($directory,$json) Test-NativeResourceEvidence -Directory $directory -Evidence ($json|ConvertFrom-Json -Depth 100)} $fixture $controlJson
        if(-not $proof.positiveCurrent.valid -or -not $proof.positiveRemoved.valid){throw 'Counterfactual requires complete valid positive controls'}
        foreach($case in @($cases|Where-Object {$_.name -cin @('short build allowance refused','short discovery allowance refused')})){
            $e=$controlJson|ConvertFrom-Json -Depth 100;[xml]$x=$trx;& $case.mutate $e $x
            $current=Test-NativeResourceEvidence -Directory $fixture -Evidence $e
            $removed=& $module {param($directory,$json) Test-NativeResourceEvidence -Directory $directory -Evidence ($json|ConvertFrom-Json -Depth 100)} $fixture ($e|ConvertTo-Json -Depth 100)
            $exactCurrent=-not $current.valid -and $current.failures.Count -eq 1 -and $current.failures[0].code -ceq 'TIMING' -and $current.failures[0].detail -ceq $case.exactDetail
            $proof.mutations+=,@{name=$case.name;expectedExactDetail=$case.exactDetail;current=$current;guardRemoved=$removed;
                currentNegativePassed=$exactCurrent;guardRemovedNegativePassed=(-not $removed.valid -and @($removed.failures|Where-Object code -CEQ 'TIMING').Count -gt 0)}
        }
        if($proof.mutations.Count -ne 2 -or @($proof.mutations|Where-Object {$_.expectedExactDetail -cne 'Build/discovery requires full600 admission' -or -not $_.currentNegativePassed -or -not $_.guardRemoved.valid -or $_.guardRemoved.failures.Count -ne 0 -or $_.guardRemovedNegativePassed}).Count){throw 'Full600 negatives are not causally sensitive to the actual guard'}
        $proof.passed=$true
    }finally {
        Remove-Module -ModuleInfo $module -Force
        $proof.liveValidatorUnchanged=(Get-FileHash -LiteralPath $validatorPath).Hash.ToLowerInvariant() -ceq $beforeHash
        [IO.File]::WriteAllText((Join-Path $OutputDirectory 'full600-guard-counterfactual.json'),(($proof|ConvertTo-Json -Depth 30)+"`n"),$utf8)
        if(-not $proof.liveValidatorUnchanged){throw 'Live validator changed during isolated counterfactual'}
    }
    $proof
}
try {
    Check-Control
    $full600GuardCounterfactual=Test-Full600GuardCounterfactual
    # Genuine VSTest optional containers, repeated lists/run infos and exact interval boundaries.
    [xml]$genuine=$trx;$namespace=$genuine.DocumentElement.NamespaceURI
    $settings=$genuine.CreateElement('TestSettings',$namespace);$settings.InnerXml="<Deployment xmlns='$namespace' runDeploymentRoot='sanitized'/><Execution xmlns='$namespace'/>";[void]$genuine.TestRun.AppendChild($settings)
    $lists=$genuine.CreateElement('TestLists',$namespace);$lists.InnerXml="<TestList xmlns='$namespace' name='Results Not in a List' id='00000000-0000-0000-0000-000000000010'/><TestList xmlns='$namespace' name='All Loaded Results' id='00000000-0000-0000-0000-000000000011'/>";[void]$genuine.TestRun.AppendChild($lists)
    $genuine.TestRun.Times.SetAttribute('start','2026-10-10T00:00:02Z');$genuine.TestRun.Times.SetAttribute('finish','2026-10-10T00:00:03Z')
    $output=$genuine.CreateElement('Output',$namespace);$output.InnerXml="<StdOut xmlns='$namespace'>xUnit.net VSTest Adapter</StdOut>";[void]$genuine.TestRun.ResultSummary.AppendChild($output)
    $infos=$genuine.CreateElement('RunInfos',$namespace);$infos.InnerXml="<RunInfo xmlns='$namespace' outcome='Information'><Text>synthetic diagnostic</Text></RunInfo><RunInfo xmlns='$namespace' outcome='Information'><Text>second diagnostic</Text></RunInfo>";[void]$genuine.TestRun.ResultSummary.AppendChild($infos)
    $resultOutput=$genuine.CreateElement('Output',$namespace);$resultOutput.InnerXml="<StdOut xmlns='$namespace'>passed diagnostic</StdOut><StdErr xmlns='$namespace'>benign label</StdErr><DebugTrace xmlns='$namespace'>trace</DebugTrace>";[void]$genuine.TestRun.Results.UnitTestResult[0].AppendChild($resultOutput)
    [void](Write-ControlFile 'native3.trx' $genuine.OuterXml)
    $genuineResult=Test-NativeResourceEvidence -Directory $fixture -Evidence ($controlJson | ConvertFrom-Json -Depth 100)
    $positiveRows+=,[ordered]@{name='genuine optional VSTest containers and exact interval boundaries';passed=$genuineResult.valid;actualFailures=@($genuineResult.failures)}
    if(-not $genuineResult.valid){throw 'Genuine optional VSTest shape rejected'}
    Check-Control
    # Same complete genuine-shaped fixture, with independently measured durations
    # and near-identical receipt timestamps as observed in native run38077832828.
    # This is synthetic validation; it does not execute the native Facts.
    [xml]$receipt=$genuine.OuterXml
    $receipt.TestRun.Times.SetAttribute('start','2026-10-10T00:00:01Z');$receipt.TestRun.Times.SetAttribute('finish','2026-10-10T00:00:04Z')
    $receiptStarts=@('2026-10-10T00:00:02.7258235Z','2026-10-10T00:00:03.5062122Z','2026-10-10T00:00:03.8742871Z')
    $receiptEnds=@('2026-10-10T00:00:02.7258634Z','2026-10-10T00:00:03.5062134Z','2026-10-10T00:00:03.8742875Z')
    $receiptDurations=@('00:00:01.5794636','00:00:01.7769498','00:00:01.8468846')
    for($i=0;$i -lt 3;$i++){
        $receipt.TestRun.Results.UnitTestResult[$i].SetAttribute('startTime',$receiptStarts[$i]);$receipt.TestRun.Results.UnitTestResult[$i].SetAttribute('endTime',$receiptEnds[$i]);$receipt.TestRun.Results.UnitTestResult[$i].SetAttribute('duration',$receiptDurations[$i])
    }
    [void](Write-ControlFile 'native3.trx' $receipt.OuterXml)
    [IO.File]::WriteAllText((Join-Path $OutputDirectory 'receipt-style-control.trx'),$receipt.OuterXml,$utf8)
    [IO.File]::WriteAllText((Join-Path $OutputDirectory 'receipt-style-control.evidence.json'),$controlJson,$utf8)
    $receiptResult=Test-NativeResourceEvidence -Directory $fixture -Evidence ($controlJson|ConvertFrom-Json -Depth 100)
    $positiveRows+=,[ordered]@{name='genuine-shaped independent durations and receipt timestamps';passed=$receiptResult.valid;actualFailures=@($receiptResult.failures);startTimes=$receiptStarts;endTimes=$receiptEnds;durations=$receiptDurations;scope='Complete synthetic fixture; no native Facts'}
    if(-not $receiptResult.valid){throw 'Legitimate receipt-style duration rejected'}
    # Exact boundary and zero remain allowed; completion bounds refuse even one tick over.
    $receipt.TestRun.Results.UnitTestResult[0].SetAttribute('duration','00:00:01.7258634')
    $receipt.TestRun.Results.UnitTestResult[1].SetAttribute('duration','00:00:00')
    [void](Write-ControlFile 'native3.trx' $receipt.OuterXml)
    $boundaryResult=Test-NativeResourceEvidence -Directory $fixture -Evidence ($controlJson|ConvertFrom-Json -Depth 100)
    $positiveRows+=,[ordered]@{name='duration at completed-receipt run boundary and nonnegative zero';passed=$boundaryResult.valid;actualFailures=@($boundaryResult.failures)}
    if(-not $boundaryResult.valid){throw 'Completed receipt duration boundary rejected'}
    Check-Control
    foreach($case in $cases){
        $e=$controlJson | ConvertFrom-Json -Depth 100;[xml]$x=$trx;$script:mutatedTrx=$null
        & $case.mutate $e $x
        [void](Write-ControlFile 'native3.trx' $(if($script:mutatedTrx){$script:mutatedTrx}else{$x.OuterXml}))
        $result=Test-NativeResourceEvidence -Directory $fixture -Evidence $e
        $passed=(-not $result.valid) -and @($result.failures | Where-Object code -CEQ $case.code).Count -gt 0
        if($case.exactDetail){$passed=$passed -and $result.failures.Count -eq 1 -and $result.failures[0].detail -ceq $case.exactDetail}
        $rows+=,[ordered]@{name=$case.name;expected=$case.code;expectedExactDetail=$case.exactDetail;observedPromotionCount=@($e.timing.operations|Where-Object operation -CEQ 'Promote-OwnedLeaf').Count;passed=$passed;actualFailures=@($result.failures)}
        Write-Output "$($case.name): expected=$($case.code) passed=$passed"
        if(-not $passed){$failed++}
        Check-Control
    }
    # Benign ErrorCode label is not an explicit loader error.
    [void](Write-ControlFile 'native3.stderr.log' 'ErrorCode label: no error occurred')
    $benign=Test-NativeResourceEvidence -Directory $fixture -Evidence ($controlJson | ConvertFrom-Json -Depth 100)
    if(-not $benign.valid){throw 'Benign ErrorCode label rejected'}
    Check-Control
} catch {$failed++;$rows+=,[ordered]@{name='control/setup';passed=$false;error=$_.Exception.ToString()};Write-Output $_.Exception.ToString()}
$runnerRows=[Collections.Generic.List[object]]::new()
function Runner-Case([string]$name,[scriptblock]$check) {
    try { & $check; $runnerRows.Add([ordered]@{name=$name;passed=$true}) }
    catch { $runnerRows.Add([ordered]@{name=$name;passed=$false;error=$_.Exception.ToString()}) }
}
try {
    # Load only the actual Git function and selected await statements from the runner AST.
    # Never dot-source its top-level provisioning/build/native lifecycle.
    $runnerPath=Join-Path $PSScriptRoot 'Invoke-NativeResourceVerification.ps1'
    $tokens=$null;$parseErrors=$null
    $runnerAst=[Management.Automation.Language.Parser]::ParseFile($runnerPath,[ref]$tokens,[ref]$parseErrors)
    if($parseErrors.Count){throw 'Runner AST parse failed'}
    $gitFunction=@($runnerAst.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq 'Git-Bytes'},$true))
    if($gitFunction.Count -ne 1){throw 'Expected one actual Git-Bytes function'}
    . ([scriptblock]::Create($gitFunction[0].Extent.Text))
    $Workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
    $git=(Get-Command git -CommandType Application | Select-Object -First 1).Source
    $head=(& $git -C $Workspace rev-parse HEAD).Trim()
    if($LASTEXITCODE -ne 0 -or $head -cnotmatch '^[0-9a-f]{40}$'){throw 'Actual Git HEAD unavailable'}
    Runner-Case 'actual Git HEAD returns one byte array and decodes' {
        $values=@(Git-Bytes @('rev-parse','HEAD'))
        if($values.Count -ne 1 -or $values[0] -isnot [byte[]]){throw 'Git-Bytes leaked non-byte success output'}
        if($utf8.GetString($values[0]).Trim() -cne $head){throw 'Actual HEAD decoding differs'}
    }
    Runner-Case 'actual Git full source blob preserves bytes' {
        # Use an unchanged tracked source owner: the opt-in test source is an intentional uncommitted slice delta.
        $relative='tools/S04NativeResourceVerification/Invoke-NativeEvidenceOperation.ps1'
        $bytes=Git-Bytes @('cat-file','blob',"${head}:$relative")
        $expected=[IO.File]::ReadAllBytes((Join-Path $Workspace $relative))
        if($bytes -isnot [byte[]] -or $bytes.Length -ne $expected.Length -or
            [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -cne
            [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($expected))){throw 'Full source blob bytes differ'}
    }
    Runner-Case 'actual Git NUL-delimited inventory preserves separators' {
        $bytes=Git-Bytes @('ls-files','-z','--','tools/S04NativeResourceVerification')
        $lines=@(& git -C $Workspace ls-files -- tools/S04NativeResourceVerification)
        if($LASTEXITCODE -ne 0 -or $lines.Count -lt 4 -or @($lines|Where-Object {$_ -notmatch '^tools/S04NativeResourceVerification/[^"\r\n]+$'}).Count){throw 'Independent line inventory unavailable'}
        $expected=$utf8.GetBytes(($lines|ForEach-Object {$_+[char]0}) -join '')
        if($bytes -isnot [byte[]] -or [Convert]::ToHexString($bytes) -cne [Convert]::ToHexString($expected)){throw 'NUL-delimited inventory bytes differ'}
    }
    Runner-Case 'actual Git full binary blob preserves embedded NUL bytes' {
        $relative='.kiro/xiigen-slices/luna/work/results/20260915T081605-34946022520/s03-windows-verification-evidence.zip'
        $bytes=Git-Bytes @('cat-file','blob',"${head}:$relative")
        $expected=[IO.File]::ReadAllBytes((Join-Path $Workspace $relative))
        if($bytes -isnot [byte[]] -or $expected.Length -eq 0 -or [Array]::IndexOf($expected,[byte]0) -lt 0 -or
            $bytes.Length -ne $expected.Length -or [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($bytes)) -cne
            [Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($expected))){throw 'Full binary blob bytes differ'}
    }
    Runner-Case 'actual invalid Git operation still fails' {
        $failure=$null
        try { [void](Git-Bytes @('cat-file','blob','S04-async-regression-invalid-object')) } catch { $failure=$_.Exception.Message }
        if(-not $failure -or $failure -cnotmatch '^git read failed:'){throw 'Git error contract was lost'}
    }
    $commandDefinition=@($runnerAst.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq 'Invoke-OwnedCommand'},$true))[0]
    $drain=@($commandDefinition.FindAll({param($node) $node -is [Management.Automation.Language.PipelineAst] -and
        $node.Extent.Text -cmatch '^(?:\[void\])?\$task\.GetAwaiter\(\)\.GetResult\(\)$'},$true))
    $write=@($runnerAst.FindAll({param($node) $node -is [Management.Automation.Language.PipelineAst] -and
        $node.Extent.Text -cmatch '^(?:\[void\])?\$output\.WriteAsync\(\$buffer,0,\$count,\$cancel.Token\)\.GetAwaiter\(\)\.GetResult\(\)$'},$true))
    if($drain.Count -ne 1 -or $write.Count -ne 1){throw 'Expected exact actual runner drain/write await statements'}
    $drainStatement=[scriptblock]::Create($drain[0].Extent.Text)
    $writeStatement=[scriptblock]::Create($write[0].Extent.Text)
    Runner-Case 'actual stream-drain await emits no completion value and preserves bytes' {
        $buffer=[byte[]]@(0,1,2,0,255,13,10,128);$input=[IO.MemoryStream]::new($buffer);$output=[IO.MemoryStream]::new()
        try {$task=$input.CopyToAsync($output);$values=@(& $drainStatement)
            if($values.Count -ne 0 -or [Convert]::ToHexString($output.ToArray()) -cne [Convert]::ToHexString($buffer)){throw 'Drain completion leaked or bytes changed'}
        }finally{$input.Dispose();$output.Dispose()}
    }
    Runner-Case 'actual stream-drain await propagates task failure' {
        $task=[Threading.Tasks.Task]::FromException([IO.IOException]::new('S04 drain fault control'));$failure=$null
        try { & $drainStatement } catch { $failure=$_.Exception.ToString() }
        if(-not $failure -or $failure -cnotmatch 'S04 drain fault control'){throw 'Drain task failure was swallowed'}
    }
    Runner-Case 'actual stream-write await emits no completion value and preserves bytes' {
        $buffer=[byte[]]@(0,1,2,0,255,13,10,128);$count=$buffer.Length;$cancel=[Threading.CancellationTokenSource]::new();$output=[IO.MemoryStream]::new()
        try {$values=@(& $writeStatement)
            if($values.Count -ne 0 -or [Convert]::ToHexString($output.ToArray()) -cne [Convert]::ToHexString($buffer)){throw 'Write completion leaked or bytes changed'}
        }finally{$output.Dispose();$cancel.Dispose()}
    }
    Runner-Case 'actual stream-write await propagates cancellation' {
        $buffer=[byte[]]@(0,255);$count=$buffer.Length;$cancel=[Threading.CancellationTokenSource]::new();$output=[IO.MemoryStream]::new();$cancel.Cancel();$failure=$null
        try {try { & $writeStatement } catch { $failure=$_.Exception.ToString() }
            if(-not $failure -or $failure -cnotmatch 'OperationCanceledException|TaskCanceledException' -or $output.Length -ne 0){throw 'Write cancellation was swallowed or wrote bytes'}
        }finally{$output.Dispose();$cancel.Dispose()}
    }
} catch { $runnerRows.Add([ordered]@{name='runner control/setup';passed=$false;error=$_.Exception.ToString()}) }
$ownedRows=[Collections.Generic.List[object]]::new()
function Owned-Case([string]$name,[scriptblock]$check) {
    $detail=[ordered]@{name=$name;passed=$false;error=$null;commands=@();valuesCount=$null;expectedFailure=$null}
    try { & $check $detail; $detail.passed=$true }
    catch { $detail.error=$_.Exception.ToString() }
    $ownedRows.Add($detail)
}
function Assert-OwnedRecord($record,[int]$expectedExit,[bool]$expectedTimeout,[int]$maximumSeconds=600) {
    if($record.exitCode -isnot [int] -or $record.exitCode -ne $expectedExit -or $record.timedOut -ne $expectedTimeout){throw 'Exact numeric exit/timeout differs'}
    if(-not $record.assignedBeforeResume -or $record.pid -le 0 -or -not $record.birthUtc -or
        $record.totalProcesses -lt 1 -or $record.activeAfterCleanup -ne 0){throw 'Owned process identity/cessation missing'}
    if($record.closureError){throw 'Owned process closure failed'}
    $start=[DateTimeOffset]::Parse($record.startUtc);$end=[DateTimeOffset]::Parse($record.endUtc);$deadline=[DateTimeOffset]::Parse($record.deadlineUtc)
    if($end -lt $start -or $deadline -le $start -or ($deadline-$start).TotalSeconds -gt $maximumSeconds){throw 'Owned command interval/deadline invalid'}
}
function Assert-OwnedStreams($record,[string]$expectedOut,[string]$expectedErr) {
    foreach($pair in @(@($record.stdout,$expectedOut),@($record.stderr,$expectedErr))){
        $bytes=[IO.File]::ReadAllBytes((Join-Path $EvidenceDirectory $pair[0]))
        if([Convert]::ToHexString($bytes) -cne [Convert]::ToHexString($utf8.GetBytes($pair[1]))){throw 'Owned raw stream bytes differ'}
    }
}
try {
    if(-not $IsWindows){throw 'Actual owned-process controls require Windows'}
    # These are the production definitions, with no top-level runner execution or provisioning.
    foreach($name in @('Write-BoundedMetadata','Write-Json','Initialize-NativeHelpers','Get-OwnedCommandAdmission','Invoke-OwnedCommand')){
        $definition=@($runnerAst.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$true))
        if($definition.Count -ne 1){throw "Expected one actual $name function"}
        . ([scriptblock]::Create($definition[0].Extent.Text))
    }
    $terminalGuard=@($runnerAst.FindAll({param($node) $node -is [Management.Automation.Language.IfStatementAst] -and
        $node.Extent.Text -cmatch '^if\(\$record\.timedOut -or \$record\.exitCode'},$true))
    if($terminalGuard.Count -ne 1){throw 'Expected one actual owned-command terminal guard'}
    $ownedTerminalGuard=[scriptblock]::Create($terminalGuard[0].Extent.Text)
    Initialize-NativeHelpers
    $EvidenceDirectory=Join-Path $OutputDirectory 'owned-process-controls';[void][IO.Directory]::CreateDirectory($EvidenceDirectory)
    $shell=Join-Path $env:WINDIR 'System32/WindowsPowerShell/v1.0/powershell.exe'
    $e=[ordered]@{commands=@()};$cleanupReserveSeconds=0
    Owned-Case 'actual owned success records exit zero and exact raw streams without void leakage' {
        param($detail)
        $overallDeadline=[DateTimeOffset]::UtcNow.AddSeconds(600)
        try {
            $values=@(Invoke-OwnedCommand 'owned-success' $shell @('-NoProfile','-NonInteractive','-Command',"[Console]::Out.Write('S04 stdout`r`n'); [Console]::Error.Write('S04 stderr`r`n'); exit 0"))
            $detail.valuesCount=$values.Count
            if($values.Count -ne 1 -or $values[0] -isnot [Collections.IDictionary]){throw 'Owned command leaked non-record success output'}
            Assert-OwnedRecord $values[0] 0 $false
            Assert-OwnedStreams $values[0] "S04 stdout`r`n" "S04 stderr`r`n"
            # Root-exit accounting is diagnostic; success is decided after owned cleanup.
        } finally { $detail.commands=@($e.commands | Where-Object name -CEQ 'owned-success') }
    }
    $script:ownedDescendantRecord=$null
    Owned-Case 'actual owned success closes a guaranteed lingering descendant before return' {
        param($detail)
        $overallDeadline=[DateTimeOffset]::UtcNow.AddSeconds(600)
        $birthPath=Join-Path $EvidenceDirectory 'owned-descendant.birth.json'
        $childCode=@'
$ProgressPreference='SilentlyContinue'
$process=[Diagnostics.Process]::GetCurrentProcess()
$birth=@{pid=$process.Id;birthUtc=$process.StartTime.ToUniversalTime().ToString('o');rootPid=__ROOT_PID__}
[IO.File]::WriteAllText('__BIRTH_PATH__.partial',($birth|ConvertTo-Json -Compress),[Text.UTF8Encoding]::new($false))
[IO.File]::Move('__BIRTH_PATH__.partial','__BIRTH_PATH__')
Start-Sleep -Seconds 60
'@.Replace('__BIRTH_PATH__',$birthPath.Replace("'","''"))
        $rootCode=@'
$ProgressPreference='SilentlyContinue'
$childCode='__CHILD_CODE__'.Replace('__ROOT_PID__',[string]$PID)
$start=[Diagnostics.ProcessStartInfo]::new()
$start.FileName='__SHELL__';$start.UseShellExecute=$false;$start.CreateNoWindow=$true
$start.Arguments='-NoProfile -NonInteractive -EncodedCommand '+[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($childCode))
$child=[Diagnostics.Process]::Start($start)
$readyDeadline=[DateTimeOffset]::UtcNow.AddSeconds(15)
while(-not [IO.File]::Exists('__BIRTH_PATH__')){
    if($child.HasExited -or [DateTimeOffset]::UtcNow -ge $readyDeadline){throw 'Owned descendant birth handshake failed'}
    Start-Sleep -Milliseconds 20
}
$birth=[IO.File]::ReadAllText('__BIRTH_PATH__')|ConvertFrom-Json
if($child.HasExited -or $birth.pid -ne $child.Id -or $birth.rootPid -ne $PID -or
    [DateTimeOffset]::Parse($birth.birthUtc).UtcDateTime.Ticks -ne $child.StartTime.ToUniversalTime().Ticks){throw 'Owned descendant birth identity differs'}
$child.Dispose()
[Console]::Out.Write("S04 descendant stdout`r`n");[Console]::Error.Write("S04 descendant stderr`r`n")
exit 0
'@.Replace('__CHILD_CODE__',$childCode.Replace("'","''")).Replace('__SHELL__',$shell.Replace("'","''")).Replace('__BIRTH_PATH__',$birthPath.Replace("'","''"))
        try {
            $values=@(Invoke-OwnedCommand 'owned-descendant-success' $shell @('-NoProfile','-NonInteractive','-EncodedCommand',[Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($rootCode))) -Seconds 30)
            $detail.valuesCount=$values.Count
            if($values.Count -ne 1 -or $values[0] -isnot [Collections.IDictionary]){throw 'Owned descendant command leaked non-record output'}
            $record=$values[0];Assert-OwnedRecord $record 0 $false
            Assert-OwnedStreams $record "S04 descendant stdout`r`n" "S04 descendant stderr`r`n"
            if($record.totalProcesses -lt 2 -or $record.activeAfterRootExit -lt 1){throw 'Lingering owned descendant was not observed at root exit'}
            $birth=Get-Content -LiteralPath $birthPath -Raw|ConvertFrom-Json
            $childBirth=[DateTimeOffset]$birth.birthUtc;$rootBirth=[DateTimeOffset]::Parse($record.birthUtc)
            $rootExit=[DateTimeOffset]::Parse($record.processEndUtc);$closed=[DateTimeOffset]::Parse($record.endUtc)
            if($birth.pid -le 0 -or $birth.rootPid -ne $record.pid -or $childBirth -lt $rootBirth -or $childBirth -gt $rootExit -or
                $closed -lt $rootExit -or $closed -gt [DateTimeOffset]::Parse($record.closureDeadlineUtc)){throw 'Exact root/descendant birth and closure interval differs'}
            $stillAlive=$false;$child=$null
            try{$child=[Diagnostics.Process]::GetProcessById([int]$birth.pid)
                $stillAlive=-not $child.HasExited -and $child.StartTime.ToUniversalTime().Ticks -eq $childBirth.UtcDateTime.Ticks
            }catch [ArgumentException]{}finally{if($child){$child.Dispose()}}
            if($stillAlive){throw 'Exact descendant birth is still alive after owned command return'}
            $oldFailure=$null
            try{if($record.activeAfterRootExit -ne 0){throw 'Success left an active owned descendant'}}catch{$oldFailure=$_.Exception.Message}
            if($oldFailure -cne 'Success left an active owned descendant'){throw 'Old root-exit criterion did not falsify the terminal success control'}
            $detail.descendant=@{pid=$birth.pid;birthUtc=$birth.birthUtc;rootPid=$birth.rootPid;aliveAfterReturn=$stillAlive}
            $detail.counterfactual=@{scope='Old assertion replayed on this actual command receipt';rejected=$true;error=$oldFailure}
            $script:ownedDescendantRecord=$record
        } finally { $detail.commands=@($e.commands | Where-Object name -CEQ 'owned-descendant-success') }
    }
    foreach($mutation in @('activeAfterCleanup','closureError')){
        Owned-Case "actual terminal guards reject controlled $mutation on closed descendant receipt" {
            param($detail)
            if(-not $script:ownedDescendantRecord){throw 'No actual closed descendant receipt for terminal negative control'}
            $record=[ordered]@{};foreach($key in $script:ownedDescendantRecord.Keys){$record[$key]=$script:ownedDescendantRecord[$key]}
            $record[$mutation]=if($mutation -ceq 'activeAfterCleanup'){1}else{'S04 controlled closure error'}
            $helperFailure=$null;try{Assert-OwnedRecord $record 0 $false}catch{$helperFailure=$_.Exception.Message}
            $expectedHelper=if($mutation -ceq 'activeAfterCleanup'){'Owned process identity/cessation missing'}else{'Owned process closure failed'}
            if($helperFailure -cne $expectedHelper){throw 'Regression terminal guard accepted mutation or failed for another reason'}
            $name=$record.name;$ownerFailure=$null;try{& $ownedTerminalGuard}catch{$ownerFailure=$_.Exception.Message}
            if($ownerFailure -cne "$name failed/nonterminal; raw streams retained"){throw 'Production terminal guard accepted mutation or failed for another reason'}
            $detail.mutation=@{scope='Controlled copy of actual terminal receipt; no command executed';field=$mutation;value=$record[$mutation];helperFailure=$helperFailure;ownerFailure=$ownerFailure}
        }
    }
    Owned-Case 'actual owned nonzero retains exit 37 and refuses after exact stream drain' {
        param($detail)
        $overallDeadline=[DateTimeOffset]::UtcNow.AddSeconds(600);$failure=$null;$values=@()
        try {
            try {$values=@(Invoke-OwnedCommand 'owned-nonzero' $shell @('-NoProfile','-NonInteractive','-Command',"[Console]::Out.Write('S04 stdout`r`n'); [Console]::Error.Write('S04 stderr`r`n'); exit 37"))}catch{$failure=$_.Exception.ToString()}
            $detail.valuesCount=$values.Count;$detail.expectedFailure=$failure
            $record=@($e.commands | Where-Object name -CEQ 'owned-nonzero')
            if($record.Count -ne 1 -or -not $failure -or $failure -cnotmatch 'owned-nonzero failed/nonterminal; raw streams retained' -or $values.Count){throw 'Nonzero command refusal/output contract changed'}
            Assert-OwnedRecord $record[0] 37 $false
            Assert-OwnedStreams $record[0] "S04 stdout`r`n" "S04 stderr`r`n"
        } finally { $detail.commands=@($e.commands | Where-Object name -CEQ 'owned-nonzero') }
    }
    Owned-Case 'actual suspended helper retains reflected exit 37 after root ceases' {
        param($detail)
        $job=$null;$started=$null;$out=[IO.MemoryStream]::new();$err=[IO.MemoryStream]::new();$outTask=$null;$errTask=$null
        $directStart=[DateTimeOffset]::UtcNow
        $record=[ordered]@{name='owned-direct';executable=$shell;argv=@('-NoProfile','-NonInteractive','-Command','exit 37');cwd=$Workspace;startUtc=$directStart.ToString('o');endUtc=$null;deadlineUtc=$directStart.AddSeconds(600).ToString('o');pid=$null;birthUtc=$null;exitCode=$null;timedOut=$false;assignedBeforeResume=$false;totalProcesses=0;activeAfterRootExit=$null;activeAfterCleanup=$null}
        try {
            $job=[S04NativeOwnedJob]::Create();$started=$job.StartSuspendedAndAssign($shell,$record.argv,$Workspace)
            $record.pid=$started.Process.Id;$record.birthUtc=$started.Process.StartTime.ToUniversalTime().ToString('o');$record.assignedBeforeResume=$true
            $outTask=$started.StandardOutput.CopyToAsync($out);$errTask=$started.StandardError.CopyToAsync($err)
            if(-not $started.Process.WaitForExit(30000)){throw 'Small direct helper did not cease'}
            # A method invocation surfaces a hidden PowerShell property-getter failure as a real exception.
            $record.exitCode=[Diagnostics.Process].GetProperty('ExitCode').GetValue($started.Process)
            $record.activeAfterRootExit=[int]$job.GetActiveProcessCount();$record.totalProcesses=[int]$job.GetTotalProcessCount()
        } finally {
            if($job){$job.Terminate();$stop=[DateTimeOffset]::UtcNow.AddSeconds(10)
                do{$record.activeAfterCleanup=[int]$job.GetActiveProcessCount();if($record.activeAfterCleanup -eq 0){break};Start-Sleep -Milliseconds 50}while([DateTimeOffset]::UtcNow -lt $stop)
                if($record.activeAfterCleanup -eq 0){foreach($task in @($outTask,$errTask)){if($task){if(-not $task.Wait(10000)){throw 'Direct helper stream drain deadline'};[void]$task.GetAwaiter().GetResult()}}}
                if($started){$started.StandardOutput.Dispose();$started.StandardError.Dispose();$started.Process.Dispose()};$job.Dispose()
            }
            [IO.File]::WriteAllBytes((Join-Path $EvidenceDirectory 'owned-direct.stdout.log'),$out.ToArray());[IO.File]::WriteAllBytes((Join-Path $EvidenceDirectory 'owned-direct.stderr.log'),$err.ToArray());$out.Dispose();$err.Dispose();$record.endUtc=[DateTimeOffset]::UtcNow.ToString('o');$detail.commands=@($record)
        }
        Assert-OwnedRecord $record 37 $false
        if((Get-Item -LiteralPath (Join-Path $EvidenceDirectory 'owned-direct.stdout.log')).Length -ne 0 -or
            (Get-Item -LiteralPath (Join-Path $EvidenceDirectory 'owned-direct.stderr.log')).Length -ne 0){throw 'Direct helper emitted unexpected bytes'}
    }
    Owned-Case 'actual owned timeout still terminates and refuses with numeric exit' {
        param($detail)
        $overallDeadline=[DateTimeOffset]::UtcNow.AddSeconds(60);$failure=$null
        try {
            try {[void](Invoke-OwnedCommand 'owned-timeout' $shell @('-NoProfile','-NonInteractive','-Command',"[Console]::Out.Write('S04 timeout`r`n'); Start-Sleep -Seconds 30; exit 0") -Seconds 10)}catch{$failure=$_.Exception.ToString()}
            $detail.expectedFailure=$failure;$record=@($e.commands | Where-Object name -CEQ 'owned-timeout')
            if($record.Count -ne 1 -or -not $failure -or $failure -cnotmatch 'owned-timeout failed/nonterminal; raw streams retained'){throw 'Timeout refusal contract changed'}
            Assert-OwnedRecord $record[0] 1 $true
            Assert-OwnedStreams $record[0] "S04 timeout`r`n" ''
        } finally { $detail.commands=@($e.commands | Where-Object name -CEQ 'owned-timeout') }
    }
} catch { $ownedRows.Add([ordered]@{name='owned process control/setup';passed=$false;error=$_.Exception.ToString()}) }
$inputOwnerRows=[Collections.Generic.List[object]]::new()
function Input-OwnerCase([string]$name,[scriptblock]$check) {
    try{& $check;$inputOwnerRows.Add([ordered]@{name=$name;passed=$true})}
    catch{$inputOwnerRows.Add([ordered]@{name=$name;passed=$false;error=$_.Exception.ToString()})}
}
try {
    foreach($name in @('Assert-SafeAncestors','Input-Record')) {
        $definition=@($runnerAst.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$true))
        if($definition.Count -ne 1){throw "No unique actual $name"};. ([scriptblock]::Create($definition[0].Extent.Text))
    }
    $ownerRoot=Join-Path $OutputDirectory 'compile-owner-controls';$ownerProject=Join-Path $ownerRoot 'Tests/LlmFromScratch.Tests.csproj'
    $ownerPackage=Join-Path $ownerRoot 'packages/microsoft.net.test.sdk/17.12.0'
    $ownerPath=Join-Path $ownerPackage 'build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs'
    $ownerTarget=Join-Path $ownerPackage 'build/netcoreapp3.1/Microsoft.NET.Test.Sdk.targets'
    $ownerArchive=Join-Path $ownerPackage 'microsoft.net.test.sdk.17.12.0.nupkg'
    $ownerAssets=Join-Path $ownerRoot 'Tests/obj/project.assets.json'
    $ownerImports=Join-Path $ownerRoot 'Tests/obj/LlmFromScratch.Tests.csproj.nuget.g.targets'
    foreach($dir in @([IO.Path]::GetDirectoryName($ownerPath),[IO.Path]::GetDirectoryName($ownerAssets))){[void][IO.Directory]::CreateDirectory($dir)}
    $ownerProgramBytes=[Convert]::FromBase64String('77u/Ly8gPGF1dG8tZ2VuZXJhdGVkPiBUaGlzIGZpbGUgaGFzIGJlZW4gYXV0byBnZW5lcmF0ZWQuIDwvYXV0by1nZW5lcmF0ZWQ+DQp1c2luZyBTeXN0ZW07DQpbTWljcm9zb2Z0LlZpc3VhbFN0dWRpby5UZXN0UGxhdGZvcm0uVGVzdFNES0F1dG9HZW5lcmF0ZWRDb2RlXQ0KY2xhc3MgQXV0b0dlbmVyYXRlZFByb2dyYW0ge3N0YXRpYyB2b2lkIE1haW4oc3RyaW5nW10gYXJncyl7fX0=');$ownerTargetBytes=[Convert]::FromBase64String('PCEtLQ0KICoqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqDQogTWljcm9zb2Z0Lk5FVC5UZXN0LlNkay50YXJnZXRzDQoNCiBXQVJOSU5HOiAgRE8gTk9UIE1PRElGWSB0aGlzIGZpbGUgdW5sZXNzIHlvdSBhcmUga25vd2xlZGdlYWJsZSBhYm91dCBNU0J1aWxkIGFuZCBoYXZlDQogICAgICAgICAgIGNyZWF0ZWQgYSBiYWNrdXAgY29weS4gIEluY29ycmVjdCBjaGFuZ2VzIHRvIHRoaXMgZmlsZSB3aWxsIG1ha2UgaXQNCiAgICAgICAgICAgaW1wb3NzaWJsZSB0byBsb2FkIG9yIGJ1aWxkIHlvdXIgcHJvamVjdHMgZnJvbSB0aGUgY29tbWFuZC1saW5lIG9yIHRoZSBJREUuDQoNCiBDb3B5cmlnaHQgKGMpIC5ORVQgRm91bmRhdGlvbi4gQWxsIHJpZ2h0cyByZXNlcnZlZC4gDQogKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioNCi0tPg0KDQo8UHJvamVjdCBJbml0aWFsVGFyZ2V0cz0iR2VuZXJhdGVQcm9ncmFtRmlsZSIgeG1sbnM9Imh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vZGV2ZWxvcGVyL21zYnVpbGQvMjAwMyI+DQoNCiAgPCEtLQ0KICAgIE91dHB1dCB0eXBlIGZvciAuTkVUIENvcmUgdGVzdCBwcm9qZWN0cyBzaG91bGQgYmUgZXhlLg0KICAgIGh0dHBzOi8vZGV2ZGl2LnZpc3VhbHN0dWRpby5jb20vRGV2RGl2L193b3JraXRlbXM/aWQ9Mzc1Njg4Jl9hPWVkaXQNCiAgLS0+DQogIDxQcm9wZXJ0eUdyb3VwPg0KICAgIDxPdXRwdXRUeXBlPkV4ZTwvT3V0cHV0VHlwZT4NCiAgPC9Qcm9wZXJ0eUdyb3VwPg0KDQogIDxQcm9wZXJ0eUdyb3VwPg0KICAgIDxHZW5lcmF0ZWRQcm9ncmFtRmlsZSBDb25kaXRpb249IickKEdlbmVyYXRlZFByb2dyYW1GaWxlKScgPT0gJyciPiQoTVNCdWlsZFRoaXNGaWxlRGlyZWN0b3J5KU1pY3Jvc29mdC5ORVQuVGVzdC5TZGsuUHJvZ3JhbSQoRGVmYXVsdExhbmd1YWdlU291cmNlRXh0ZW5zaW9uKTwvR2VuZXJhdGVkUHJvZ3JhbUZpbGU+DQogICAgPEdlbmVyYXRlUHJvZ3JhbUZpbGUgQ29uZGl0aW9uPSInJChHZW5lcmF0ZVByb2dyYW1GaWxlKScgPT0gJyciPnRydWU8L0dlbmVyYXRlUHJvZ3JhbUZpbGU+DQogIDwvUHJvcGVydHlHcm91cD4NCg0KICA8IS0tDQogICAgPT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09DQogICAgR2VuZXJhdGVQcm9ncmFtRmlsZQ0KICAgIEdlbmVyYXRlcyBQcm9ncmFtIGZpbGUgd2hpY2ggY29udGFpbnMgdGhlIE1haW4gZW50cnkgcG9pbnQNCiAgICA9PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT09PT0NCiAgLS0+DQogIDxUYXJnZXQgTmFtZT0iR2VuZXJhdGVQcm9ncmFtRmlsZSINCiAgICAgICAgICBDb25kaXRpb249IickKEdlbmVyYXRlUHJvZ3JhbUZpbGUpJyA9PSAndHJ1ZSciPg0KDQogICAgPEl0ZW1Hcm91cCBDb25kaXRpb249IickKExhbmd1YWdlKScgPT0gJ1ZCJyBvciAnJChMYW5ndWFnZSknID09ICdDIyciPg0KICAgICAgPFJlbW92ZUV4aXN0aW5nTWljcm9zb2Z0TkVUVGVzdFNka1Byb2dyYW0gSW5jbHVkZT0iQChDb21waWxlKSIgQ29uZGl0aW9uPSInJShGaWxlTmFtZSknID09ICdNaWNyb3NvZnQuTkVULlRlc3QuU2RrLlByb2dyYW0nIiAvPg0KICAgICAgPENvbXBpbGUgUmVtb3ZlPSJAKFJlbW92ZUV4aXN0aW5nTWljcm9zb2Z0TkVUVGVzdFNka1Byb2dyYW0pIiAvPg0KICAgICAgPENvbXBpbGUgSW5jbHVkZT0iJChHZW5lcmF0ZWRQcm9ncmFtRmlsZSkiLz4NCiAgICA8L0l0ZW1Hcm91cD4NCg0KICAgIDxJdGVtR3JvdXAgQ29uZGl0aW9uPSInJChMYW5ndWFnZSknID09ICdGIyciPg0KICAgICAgPFByb2dyYW1Db21waWxlcyBJbmNsdWRlPSJAKENvbXBpbGUpIiBDb25kaXRpb249IiclKElkZW50aXR5KScgPT0gJ1Byb2dyYW0uZnMnIiAvPg0KICAgICAgPENvbXBpbGVBZnRlciBJbmNsdWRlPSIkKEdlbmVyYXRlZFByb2dyYW1GaWxlKSIgQ29uZGl0aW9uPSJAKFByb2dyYW1Db21waWxlcy0mZ3Q7Q291bnQoKSkgPT0gMCIgLz4NCiAgICA8L0l0ZW1Hcm91cD4NCg0KICAgIDxXYXJuaW5nIENvbmRpdGlvbj0iQChQcm9ncmFtQ29tcGlsZXMtJmd0O0NvdW50KCkpICE9IDAiIFRleHQ9IkEgJ1Byb2dyYW0uZnMnIGZpbGUgY2FuIGJlIGF1dG9tYXRpY2FsbHkgZ2VuZXJhdGVkIGZvciBGIyAuTkVUIENvcmUgdGVzdCBwcm9qZWN0cy4gVG8gZml4IHRoaXMgd2FybmluZywgZWl0aGVyIGRlbGV0ZSB0aGUgZmlsZSBmcm9tIHRoZSBwcm9qZWN0LCBvciBzZXQgdGhlICZsdDtHZW5lcmF0ZVByb2dyYW1GaWxlJmd0OyBwcm9wZXJ0eSB0byAnZmFsc2UnLiIgLz4NCg0KICA8L1RhcmdldD4NCg0KPC9Qcm9qZWN0Pg0K')
    $ownerProjectText='<Project><ItemGroup><PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.12.0" /></ItemGroup></Project>'
    $ownerImportsText='<Project><Import Project="$(NuGetPackageRoot)microsoft.net.test.sdk\17.12.0\build\netcoreapp3.1\Microsoft.NET.Test.Sdk.targets" /></Project>'
    function Reset-OwnerFixture {
        [IO.File]::WriteAllText($ownerProject,$ownerProjectText,$utf8);[IO.File]::WriteAllText($ownerImports,$ownerImportsText,$utf8)
        [IO.File]::WriteAllBytes($ownerPath,$ownerProgramBytes);[IO.File]::WriteAllBytes($ownerTarget,$ownerTargetBytes)
        if(Test-Path -LiteralPath $ownerArchive){[IO.File]::Delete($ownerArchive)}
        $zip=[IO.Compression.ZipFile]::Open($ownerArchive,[IO.Compression.ZipArchiveMode]::Create)
        try{foreach($pair in @(@('build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs',$ownerProgramBytes),@('build/netcoreapp3.1/Microsoft.NET.Test.Sdk.targets',$ownerTargetBytes),@('Microsoft.NET.Test.Sdk.nuspec',$utf8.GetBytes('<package><metadata><id>Microsoft.NET.Test.Sdk</id><version>17.12.0</version></metadata></package>')))){
            $s=$zip.CreateEntry($pair[0]).Open();try{$s.Write($pair[1],0,$pair[1].Length)}finally{$s.Dispose()}
        }}finally{$zip.Dispose()}
        $archiveHash=[Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($ownerArchive)))
        [IO.File]::WriteAllText($ownerArchive+'.sha512',$archiveHash,$utf8)
        $ownerAssetsObject=@{project=@{restore=@{projectPath=$ownerProject}};packageFolders=@{(Join-Path $ownerRoot 'packages')=@{}};libraries=@{'Microsoft.NET.Test.Sdk/17.12.0'=@{type='package';path='microsoft.net.test.sdk/17.12.0';sha512='kt/PKBZ91rFCWxVIJZSgVLk+YR+4KxTuHf799ho8WNiK5ZQpJNAEZCAWX86vcKrs+DiYjiibpYKdGZP6+/N17w==';files=@('build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs','build/netcoreapp3.1/Microsoft.NET.Test.Sdk.targets')}};targets=@{net9=@{'Microsoft.NET.Test.Sdk/17.12.0'=@{build=@{'build/netcoreapp3.1/Microsoft.NET.Test.Sdk.targets'=@{}}}}}}
        [IO.File]::WriteAllText($ownerAssets,($ownerAssetsObject|ConvertTo-Json -Depth 20),$utf8)
        $sourceMap[$ownerProject]=@{candidateSha256=(Get-FileHash -LiteralPath $ownerProject).Hash.ToLowerInvariant();candidateLength=(Get-Item -LiteralPath $ownerProject).Length}
    }
    function Assert-OwnerRejected([scriptblock]$mutate,[string]$path=$ownerPath) {
        Reset-OwnerFixture;& $mutate;$failure=$null
        try{[void](Input-Record $path $ownerProject)}catch{$failure=$_.Exception.Message}
        if(-not $failure){throw 'Mutated external owner accepted'}
    }
    $oldWorkspace=$Workspace;$Workspace=$ownerRoot;$sourceMap=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    Input-OwnerCase 'actual runner admits exact restored owner and official member bytes' {
        Reset-OwnerFixture;$record=Input-Record $ownerPath $ownerProject
        if($record.kind -cne 'nuget-package' -or $record.sha256 -cne 'd66f12f9433816a8c7dc1b2acb9e1010f5418f5eeb51e80f1df57aa1dcab8d8a' -or -not $record.owner.assetsSha256){throw 'Owner positive differs'}
        $again=Input-Record $ownerPath $ownerProject
        if(($again|ConvertTo-Json -Depth 20 -Compress) -cne ($record|ConvertTo-Json -Depth 20 -Compress)){throw 'Unchanged byte fingerprint unstable'}
    }
    Input-OwnerCase 'foreign same filename is refused' {Assert-OwnerRejected { $foreign=Join-Path $ownerRoot 'Microsoft.NET.Test.Sdk.Program.cs';[IO.File]::WriteAllBytes($foreign,$ownerProgramBytes) } (Join-Path $ownerRoot 'Microsoft.NET.Test.Sdk.Program.cs')}
    Input-OwnerCase 'changed extracted source bytes are refused' {Assert-OwnerRejected {[IO.File]::AppendAllText($ownerPath,'//mutated',$utf8)}}
    Input-OwnerCase 'foreign project restore owner is refused' {Assert-OwnerRejected {$a=Get-Content -LiteralPath $ownerAssets -Raw|ConvertFrom-Json -AsHashtable;$a.project.restore.projectPath=Join-Path $ownerRoot 'Foreign.csproj';[IO.File]::WriteAllText($ownerAssets,($a|ConvertTo-Json -Depth 20),$utf8)}}
    Input-OwnerCase 'different direct package version is refused' {Assert-OwnerRejected {[IO.File]::WriteAllText($ownerProject,$ownerProjectText.Replace('17.12.0','17.13.0'),$utf8)}}
    Input-OwnerCase 'different restore library hash is refused' {Assert-OwnerRejected {$a=Get-Content -LiteralPath $ownerAssets -Raw|ConvertFrom-Json -AsHashtable;$a.libraries['Microsoft.NET.Test.Sdk/17.12.0'].sha512='foreign';[IO.File]::WriteAllText($ownerAssets,($a|ConvertTo-Json -Depth 20),$utf8)}}
    Input-OwnerCase 'missing actual NuGet target import is refused' {Assert-OwnerRejected {[IO.File]::WriteAllText($ownerImports,'<Project />',$utf8)}}
    Input-OwnerCase 'coherent archive sidecar and assets substitution cannot replace pinned source' {
        Assert-OwnerRejected {
            $z=[IO.Compression.ZipFile]::Open($ownerArchive,[IO.Compression.ZipArchiveMode]::Update)
            try{$entry=$z.GetEntry('build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs');$entry.Delete();$s=$z.CreateEntry('build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs').Open();$bytes=$utf8.GetBytes('public class Foreign {}');try{$s.Write($bytes,0,$bytes.Length)}finally{$s.Dispose()};[IO.File]::WriteAllBytes($ownerPath,$bytes)}finally{$z.Dispose()}
            $h=[Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($ownerArchive)));[IO.File]::WriteAllText($ownerArchive+'.sha512',$h,$utf8)
            $a=Get-Content -LiteralPath $ownerAssets -Raw|ConvertFrom-Json -AsHashtable;$a.libraries['Microsoft.NET.Test.Sdk/17.12.0'].sha512=$h;[IO.File]::WriteAllText($ownerAssets,($a|ConvertTo-Json -Depth 20),$utf8)
        }
    }
    Input-OwnerCase 'coherent archive and sidecar with unchanged library cannot replace pinned source' {
        Assert-OwnerRejected {
            $z=[IO.Compression.ZipFile]::Open($ownerArchive,[IO.Compression.ZipArchiveMode]::Update)
            try{$z.GetEntry('build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs').Delete();$s=$z.CreateEntry('build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs').Open();$bytes=$utf8.GetBytes('public class Foreign {}');try{$s.Write($bytes,0,$bytes.Length)}finally{$s.Dispose()};[IO.File]::WriteAllBytes($ownerPath,$bytes)}finally{$z.Dispose()}
            $h=[Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData([IO.File]::ReadAllBytes($ownerArchive)));[IO.File]::WriteAllText($ownerArchive+'.sha512',$h,$utf8)
        }
    }
    $Workspace=$oldWorkspace
}catch{$inputOwnerRows.Add([ordered]@{name='compile owner control/setup';passed=$false;error=$_.Exception.ToString()})}
$inputOwnerFailed=@($inputOwnerRows|Where-Object passed -EQ $false).Count
$timingRows=[Collections.Generic.List[object]]::new()
function Timing-Case([string]$name,[scriptblock]$check) {
    $detail=[ordered]@{name=$name;passed=$false;error=$null;commands=@();metadataWrites=@();facts=@{}}
    try{& $check $detail;$detail.passed=$true}catch{$detail.error=$_.Exception.ToString()}
    $timingRows.Add($detail)
}
try {
    foreach($name in @('Write-NewJson','Read-ControlBytes','File-Record','Hash-Bytes','Invoke-TimedPhase','Save-OwnershipJournal','Import-OperationOwnership','Assert-OperationReceipt',
        'Invoke-BoundedEvidenceOperation','Assert-SafeAncestors','New-OwnedDirectory','New-OwnedLeaf','Seal-OwnedLeaf','Get-OwnedSealLength','Get-OwnedSealAdmission','Write-OperationProgress','Read-FullDiskHash','Resource-Hashes','Promote-OwnedLeaf','Fixture-Inventory','Cleanup-Owned')){
        $definition=@($runnerAst.FindAll({param($node) $node -is [Management.Automation.Language.FunctionDefinitionAst] -and $node.Name -ceq $name},$true))
        if($definition.Count -ne 1){throw "Expected one actual timing $name"};. ([scriptblock]::Create($definition[0].Extent.Text))
    }
    $Workspace=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'));$RunnerTemp=$OutputDirectory
    $EvidenceDirectory=Join-Path $OutputDirectory 'timing-controls';[void][IO.Directory]::CreateDirectory($EvidenceDirectory)
    $CandidateSha=$head;$WorkflowSha=$head;$script:sdkRoot=$null;$script:operationSequence=0;$script:InsideEvidenceOperation=$false
    $runStart=[DateTimeOffset]::UtcNow;$regularSeconds=7400;$cleanupReserveSeconds=2940;$overallDeadline=$runStart.AddSeconds(10340)
    $script:phaseDeadline=$runStart.AddSeconds(600);$script:phaseName='default';$timingPolicy=Get-NativeTimingPolicy
    $sourceMap=[Collections.Generic.Dictionary[string,object]]::new([StringComparer]::OrdinalIgnoreCase)
    $ownedLeaves=[Collections.Generic.List[object]]::new();$ownedDirectories=[Collections.Generic.List[object]]::new()
    $e=[ordered]@{commands=@();sources=@();failures=@();downloads=@();resources=@();cleanup=@{completed=$false;failures=@();leaves=@();directories=@();activeProcesses=$null};
        timing=@{phases=@();operations=@();admissions=@();metadataWrites=@();unknownEffects=$false}}
    $smallRoot=Join-Path $OutputDirectory 'physical-owned-small';$smallPath=Join-Path $smallRoot 'resource';New-OwnedDirectory $smallRoot
    $smallBytes=$utf8.GetBytes('tiny');$smallHash=Hash-Bytes $smallBytes
    $resources=@(@{pin=@{id='small-owned-control';length=4;sha256=$smallHash};path=$smallPath})
    foreach($pair in @(@('default-before','default'),@('opt-in','optin'),@('default-restoration','restoration'))){
        $buildPhase=$pair[0];$policyPhase=$pair[1]
        Timing-Case "production $buildPhase sequential maxima retain full build discovery and capture" {
            param($detail)
            $trace=Get-ControlledBuildAdmissionTrace $buildPhase $timingPolicy.phases[$policyPhase]
            $detail.facts=$trace
            if($trace.failure -or -not $trace.captured -or $trace.baselineCount -ne 1 -or $trace.trace.Count -ne 3 -or
                $trace.trace[0].allowedSeconds -ne 600 -or $trace.trace[1].allowedSeconds -ne 600 -or $trace.trace[2].allowedSeconds -ne 300 -or
                (Get-NativeUtc $trace.checkpointEndUtc) -gt (Get-NativeUtc $trace.phaseDeadlineUtc)){throw 'Production build/list/capture full sequence does not fit'}
        }
    }
    Timing-Case 'production old600 phase exposes420 then refuses before default baseline' {
        param($detail)
        $trace=Get-ControlledBuildAdmissionTrace 'default-before' 600;$detail.facts=$trace
        if(-not $trace.failure -or $trace.trace.Count -ne 1 -or $trace.trace[0].availableSeconds -ne 420 -or $trace.trace[0].effectsStarted -or $trace.baselineCount -ne 0 -or $trace.captured){throw 'Old phase defect no longer inspectable'}
    }
    Timing-Case 'production genuine intermediate overrun refuses discovery before effects and baseline' {
        param($detail)
        $trace=Get-ControlledBuildAdmissionTrace 'default-before' $timingPolicy.phases.default 461;$detail.facts=$trace
        if(-not $trace.failure -or $trace.trace.Count -ne 2 -or $trace.trace[0].allowedSeconds -ne 600 -or $trace.trace[1].effectsStarted -or $trace.trace[1].availableSeconds -ge 600 -or $trace.baselineCount -ne 0 -or $trace.captured){throw 'Genuine overrun was admitted or baseline fabricated'}
    }
    Timing-Case 'actual default phase build has600 window and terminal owned process closure' {
        param($detail)
        Invoke-TimedPhase 'default' {[void](Invoke-OwnedCommand 'default-before-build' $shell @('-NoProfile','-NonInteractive','-Command','exit 0'))}
        $row=$e.commands[-1];Assert-OwnedRecord $row 0 $false
        if([Math]::Abs(((Get-NativeUtc $row.deadlineUtc)-(Get-NativeUtc $row.startUtc)).TotalSeconds-600) -gt 0.01){throw 'Actual phase still shrinks build'}
        $detail.commands=@($row);$detail.metadataWrites=@($e.timing.metadataWrites);$detail.facts=@{phase=$e.timing.phases[-1];nativeFacts=0}
    }
    Timing-Case 'profile sequence envelope and workflow preserve maxima and startup upload margin' {
        param($detail)
        $sequence=$timingPolicy.buildSequence
        $sequenceSeconds=$sequence.buildSeconds+$sequence.discoverySeconds+$sequence.captureSeconds+$sequence.commandClosures*$timingPolicy.closureSeconds+$timingPolicy.phaseCheckpointSeconds+$sequence.controllerSlackSeconds
        $workflow=[IO.File]::ReadAllText((Join-Path $Workspace '.github/workflows/s03-windows-verification.yml'))
        $match=[regex]::Match($workflow,'(?ms)^  native-resource-verification:.*?^    timeout-minutes: (\d+)')
        if($sequenceSeconds -ne 1920 -or $sequence.buildSeconds -ne 600 -or $sequence.discoverySeconds -ne 600 -or $sequence.captureSeconds -ne 300 -or
            @(@('default','optin','restoration')|Where-Object {$timingPolicy.phases[$_] -ne $sequenceSeconds}).Count -or -not $match.Success -or
            [int]$match.Groups[1].Value*60 -ne $timingPolicy.jobSeconds -or $timingPolicy.jobSeconds-($regularSeconds+$cleanupReserveSeconds) -ne 640){throw 'Profile/workflow envelope or retained maxima drift'}
        $detail.facts=@{sequence=$sequence;sequenceSeconds=$sequenceSeconds;regularSeconds=$regularSeconds;reserveSeconds=$cleanupReserveSeconds;overallSeconds=10340;jobSeconds=$timingPolicy.jobSeconds;startupUploadMarginSeconds=640}
    }
    Timing-Case 'T01 actual initializer keeps global7400 despite local caps8600' {
        param($detail)
        $names=@('timingPolicy','runStart','regularSeconds','cleanupReserveSeconds','overallDeadline')
        $init=@($runnerAst.EndBlock.Statements|Where-Object {$_ -is [Management.Automation.Language.AssignmentStatementAst] -and $_.Left -is [Management.Automation.Language.VariableExpressionAst] -and $_.Left.VariablePath.UserPath -cin $names})
        if($init.Count -ne 5){throw 'Actual initializer extraction drift'}
        $values=& {foreach($row in $init){. ([scriptblock]::Create($row.Extent.Text))};@{regular=$regularSeconds;reserve=$cleanupReserveSeconds;overall=($overallDeadline-$runStart).TotalSeconds;policy=$timingPolicy}}
        $caps=$timingPolicy.phases.prepare+$timingPolicy.phases.default+$timingPolicy.phases.provision+$timingPolicy.phases.optin+$timingPolicy.phases.preflight+$timingPolicy.phases.native
        if($caps -ne 8600 -or $values.regular -ne 7400 -or $values.reserve -ne 2940 -or $values.overall -ne 10340 -or $timingPolicy.phases.native -ne 2000 -or $timingPolicy.nativeGroup.factCount*$timingPolicy.nativeGroup.allocationBaseSeconds -ne 1800){throw 'Actual startup silently grows absolute budget'}
        $detail.facts=@{initializer=$init.Extent.Text;actual=$values;localCapSum=$caps;jobSeconds=$timingPolicy.jobSeconds;margin=$timingPolicy.jobSeconds-$values.overall}
    }
    Timing-Case 'T02 T03 exact native admission clips global absolute and phase edges' {
        param($detail)
        $now=[DateTimeOffset]::Parse('2026-10-10T00:00:00Z');$priorOverall=$overallDeadline;$priorPhase=$script:phaseDeadline;$priorName=$script:phaseName
        try{
            $script:phaseName='native';$script:phaseDeadline=$now.AddSeconds(1980);$overallDeadline=$now.AddSeconds(10340)
            $positive=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now
            $script:phaseDeadline=$now.AddSeconds(1979.999);$short=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now
            $script:phaseDeadline=$now.AddSeconds(2000);$overallDeadline=$now.AddSeconds(2940+1979.999);$globalShort=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now
            $overallDeadline=$now.AddSeconds(2940+1980);$globalFit=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now
            $earlier=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now -AbsoluteDeadline $now.AddSeconds(1799.999)
            if($positive.reason -or $globalFit.reason -or ($positive.deadline-$now).TotalSeconds -ne 1800 -or -not $short.reason -or -not $globalShort.reason -or -not $earlier.reason){throw 'Native full group/global/absolute admission violated'}
            $detail.facts=@{exactFit=$positive;phaseShort=$short;globalShort=$globalShort;globalFit=$globalFit;earlierAbsolute=$earlier;effectsStarted=$false}
        }finally{$overallDeadline=$priorOverall;$script:phaseDeadline=$priorPhase;$script:phaseName=$priorName}
    }
    Timing-Case 'T04 narrow native exception retains ordinary1..600 and build discovery600' {
        param($detail)
        $now=[DateTimeOffset]::UtcNow;$priorOverall=$overallDeadline;$priorPhase=$script:phaseDeadline;$priorName=$script:phaseName
        try{
            $overallDeadline=$now.AddSeconds(10340);$script:phaseDeadline=$now.AddSeconds(2000);$script:phaseName='native';$negative=@()
            foreach($seconds in @(600,1799,1801)){$negative+=Get-OwnedCommandAdmission 'native3' -Seconds $seconds -Now $now}
            $negative+=Get-OwnedCommandAdmission 'Native3' -Seconds 1800 -Now $now
            $negative+=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now -Restoration
            $script:phaseName='default';$negative+=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now
            foreach($seconds in @(601,1200,1800)){$negative+=Get-OwnedCommandAdmission 'ordinary' -Seconds $seconds -Now $now}
            if(@($negative|Where-Object {-not $_.reason}).Count){throw 'Exception became general or borrowed restoration'}
            foreach($name in @('ordinary','default-before-build','default-before-list')){if((Get-OwnedCommandAdmission $name -Seconds 600 -Now $now).reason){throw 'Ordinary full600 lost'}}
            $detail.facts=@{negative=$negative;ordinaryFull600=$true}
        }finally{$overallDeadline=$priorOverall;$script:phaseDeadline=$priorPhase;$script:phaseName=$priorName}
    }
    Timing-Case 'T02 actual owned native-shaped managed command refuses1799.999 before process then1800 closes' {
        param($detail)
        $definition=@($runnerAst.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq 'Get-OwnedCommandAdmission'},$true))[0].Extent.Text
        $priorOverall=$overallDeadline;$priorPhase=$script:phaseDeadline;$priorName=$script:phaseName;$priorAdmissions=$e.timing.admissions;$now=[DateTimeOffset]::UtcNow
        try{
            . ([scriptblock]::Create($definition.Replace('function Get-OwnedCommandAdmission','function Get-ControlActualAdmission')))
            function Get-OwnedCommandAdmission([string]$name,[int]$Seconds=600,[switch]$Restoration,[Nullable[DateTimeOffset]]$AbsoluteDeadline=$null){Get-ControlActualAdmission $name -Seconds $Seconds -Restoration:$Restoration -AbsoluteDeadline $AbsoluteDeadline -Now $now}
            $overallDeadline=$now.AddSeconds(10340);$script:phaseName='native';$script:phaseDeadline=$now.AddSeconds(1979.999)
            $before=$e.commands.Count;$beforeAdmissions=$e.timing.admissions.Count;$failure=$null
            try{[void](Invoke-OwnedCommand 'native3' $shell @('-NoProfile','-NonInteractive','-Command','exit 0') -Seconds 1800)}catch{$failure=$_.Exception.Message}
            if(-not $failure -or $e.commands.Count -ne $before -or $e.timing.admissions.Count -ne $beforeAdmissions+1 -or $e.timing.admissions[-1].effectsStarted -ne $false){throw 'Exact shortage created a process or omitted refusal'}
            $refusal=$e.timing.admissions[-1];$script:phaseDeadline=$now.AddSeconds(1980)
            $row=Invoke-OwnedCommand 'native3' $shell @('-NoProfile','-NonInteractive','-Command','exit 0') -Seconds 1800
            Assert-OwnedRecord $row 0 $false 1800
            if(((Get-NativeUtc $row.deadlineUtc)-(Get-NativeUtc $row.startUtc)).TotalSeconds -ne 1800){throw 'Managed positive shortened native group'}
            $detail.commands=@($row);$detail.facts=@{exactShortage=$refusal;processesBeforeRefusal=$before;nativeResourceFacts=0;restoredExactFit=$true}
        }finally{. ([scriptblock]::Create($definition));$overallDeadline=$priorOverall;$script:phaseDeadline=$priorPhase;$script:phaseName=$priorName;$e.timing.admissions=$priorAdmissions}
    }
    Timing-Case 'actual finite helper writes seals and promotes exact small physical owner' {
        param($detail)
        [void](Invoke-BoundedEvidenceOperation 'Write-OwnedManifest' @('tiny',($smallPath+'.partial')) 30)
        Seal-OwnedLeaf ($smallPath+'.partial');Promote-OwnedLeaf ($smallPath+'.partial') $smallPath
        if((Get-FileHash -LiteralPath $smallPath).Hash.ToLowerInvariant() -cne $smallHash -or $ownedLeaves.Count -ne 1 -or
            $ownedLeaves[0].path -cne $smallPath -or $ownedLeaves[0].sha256 -cne $smallHash){throw 'Actual seal/promotion custody mismatch'}
        $detail.commands=@($e.commands);$detail.metadataWrites=@($e.timing.metadataWrites);$detail.facts=@{identity=$ownedLeaves[0].identity;length=$ownedLeaves[0].length;sha256=$smallHash}
    }
    # The original receipt is a live deadline-bound positive. Exercise it immediately
    # after promotion, before the longer independent operation controls can age it out.
    Timing-Case 'foreign new leaf receipt refuses deletion authority and original receipt restores' {
        param($detail)
        $requestPath=Join-Path $EvidenceDirectory 'operation-003.request.json'
        $request=Get-Content -LiteralPath $requestPath -Raw|ConvertFrom-Json -AsHashtable -Depth 100
        $state=Get-Content -LiteralPath ($requestPath+'.result.json') -Raw|ConvertFrom-Json -AsHashtable -Depth 100
        $oldLeaves=$ownedLeaves;$oldDirectories=$ownedDirectories
        $script:ownedLeaves=[Collections.Generic.List[object]]::new();foreach($row in $request.ownedLeaves){$script:ownedLeaves.Add($row)}
        $script:ownedDirectories=[Collections.Generic.List[object]]::new();foreach($row in $request.ownedDirectories){$script:ownedDirectories.Add($row)}
        $original=$state.ownedLeaves;$foreignPath=Join-Path $EvidenceDirectory 'foreign-preserved-control.txt';[IO.File]::WriteAllText($foreignPath,'foreign control',$utf8)
        try{$state.ownedLeaves+=,@{path=$foreignPath;owned=$true;absentBefore=$true;absentAfter=$false;reparse=$false;identity=[S04NativeCustody]::Identity($foreignPath);creationTicks=(Get-Item -LiteralPath $foreignPath).CreationTimeUtc.Ticks;sha256=$smallHash;length=4}
            $failure=$null;try{Assert-OperationReceipt $state $request (Hash-Bytes ([IO.File]::ReadAllBytes($requestPath))) ([S04NativeCustody]::Identity($requestPath))}catch{$failure=$_.Exception.Message}
            if(-not $failure -or -not (Test-Path -LiteralPath $foreignPath) -or $ownedLeaves.Count -ne 1){throw 'Foreign receipt acquired authority'}
            $state.ownedLeaves=$original;Assert-OperationReceipt $state $request (Hash-Bytes ([IO.File]::ReadAllBytes($requestPath))) ([S04NativeCustody]::Identity($requestPath))
            $detail.facts=@{failure=$failure;foreignPreserved=$true;originalReceiptRestored=$true}
        }finally{$script:ownedLeaves=$oldLeaves;$script:ownedDirectories=$oldDirectories}
    }
    Timing-Case 'bulk seal admission uses exact pinned size and original clock within unchanged envelopes' {
        param($detail)
        $pins=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'public-resource-pins.json') -Raw|ConvertFrom-Json
        $blob=@($pins.blobs|Where-Object length -EQ 6594462816L)
        if($blob.Count -ne 1){throw 'Exact declared bulk pin absent'}
        $prior=$script:phaseDeadline
        try{$script:phaseDeadline=[DateTimeOffset]::Parse('2026-10-10T13:15:04.6511641Z')
            $now=[DateTimeOffset]::Parse('2026-10-10T12:53:06.7755393Z');$start=[DateTimeOffset]::Parse('2026-10-10T12:51:55Z')
            $admission=Get-OwnedSealAdmission ([long]$blob[0].length) -Now $now -DownloadStart $start
            if($admission.reason -or $admission.seconds -ne 414 -or $admission.phaseReserveSeconds -ne 320 -or $admission.downloadTailReserveSeconds -ne 250 -or
                $admission.downloadEnvelopeSeconds -ne 1140 -or $admission.availableSeconds -lt 414){throw 'Size-bound bulk interval does not fit original finite graph'}
            $phaseRefusal=Get-OwnedSealAdmission ([long]$blob[0].length) -Now $script:phaseDeadline.AddSeconds(-593)
            $downloadRefusal=Get-OwnedSealAdmission ([long]$blob[0].length) -Now $start.AddSeconds(477) -DownloadStart $start
            $oversize=Get-OwnedSealAdmission (10GB) -Now $now
            if(-not $phaseRefusal.reason -or -not $downloadRefusal.reason -or -not $oversize.reason){throw 'Bulk admission ignored phase/download/600 limit'}
            $detail.facts=@{pinId=$blob[0].id;pinSha256=$blob[0].sha256;admitted=$admission;phaseRefusal=$phaseRefusal;downloadRefusal=$downloadRefusal;oversizeRefusal=$oversize;throughput='allocation heuristic, not measured throughput'}
        }finally{$script:phaseDeadline=$prior}
    }
    Timing-Case 'actual bulk seal near phase limit reserves both closures promotion and checkpoint' {
        param($detail)
        $prior=$script:phaseDeadline;$priorAdmissions=$e.timing.admissions;$before=$e.commands.Count
        try{
            $script:phaseDeadline=[DateTimeOffset]::Parse('2026-10-10T13:15:04.6511641Z')
            $oldBoundary=Get-OwnedSealAdmission 6594462816L -Now $script:phaseDeadline.AddSeconds(-594)
            $oneSecondShort=Get-OwnedSealAdmission 6594462816L -Now $script:phaseDeadline.AddSeconds(-733)
            $exact=Get-OwnedSealAdmission 6594462816L -Now $script:phaseDeadline.AddSeconds(-734)
            $afterPromotion=(Get-NativeUtc $exact.nowUtc).AddSeconds($exact.seconds+110+30+110)
            $checkpointRemaining=($script:phaseDeadline-$afterPromotion).TotalSeconds
            if(-not $oldBoundary.reason -or -not $oneSecondShort.reason -or $exact.reason -or $exact.phaseReserveSeconds -ne 320 -or
                $checkpointRemaining -ne 70 -or $e.commands.Count -ne $before){throw 'Seal chain can exhaust promotion closure or checkpoint reserve'}
            $lateFailure=$null
            try{[void](Invoke-BoundedEvidenceOperation 'Seal-OwnedLeaf' @($smallPath,4) 414 $exact)}catch{$lateFailure=$_.Exception.Message}
            if($lateFailure -cne 'Full seal tail reserve no longer fits before operation effects' -or $e.commands.Count -ne $before){throw 'Expired seal admission started command effects'}
            $detail.facts=@{old594=$oldBoundary;oneSecondShort733=$oneSecondShort;exact734=$exact;sealClosureSeconds=110;promotionSeconds=30;promotionClosureSeconds=110;checkpointRemainingSeconds=$checkpointRemaining;lateFailure=$lateFailure;commandsStarted=0}
        }finally{$script:phaseDeadline=$prior;$e.timing.admissions=$priorAdmissions}
    }
    Timing-Case 'actual seal admission rejects changed declared length and physical identity before command effects' {
        param($detail)
        $before=$e.commands.Count;$prior=$ownedLeaves[0].identity;$failures=@()
        try{try{Seal-OwnedLeaf $smallPath 5}catch{$failures+=,$_.Exception.Message}
            $ownedLeaves[0].identity='00000000:0000000000000000'
            try{Seal-OwnedLeaf $smallPath 4}catch{$failures+=,$_.Exception.Message}
            if($failures.Count -ne 2 -or $e.commands.Count -ne $before -or -not (Test-Path -LiteralPath $smallPath)){throw 'Foreign/changed seal admission started command effects'}
            $detail.facts=@{failures=$failures;commandsStarted=0;present=$true}
        }finally{$ownedLeaves[0].identity=$prior}
    }
    Timing-Case 'actual full disk reader seals tail bytes and fresh pre post drift controls under owned Job' {
        param($detail)
        $root=Join-Path $OutputDirectory 'physical-owned-tail';$path=Join-Path $root 'resource.partial';$length=64MB+1
        $priorResources=$resources;$priorLeaves=$ownedLeaves;$priorDirectories=$ownedDirectories;$priorCleanup=$e.cleanup;$before=$e.commands.Count
        $script:ownedLeaves=[Collections.Generic.List[object]]::new();$script:ownedDirectories=[Collections.Generic.List[object]]::new();New-OwnedDirectory $root
        $stream=New-OwnedLeaf $path;try{$stream.SetLength($length);$stream.Position=$length-1;$stream.WriteByte(123);$stream.Flush($true)}finally{$stream.Dispose()}
        $expected=(Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant();$resources=@(@{pin=@{id='tail-control';length=$length;sha256=$expected};path=$path})
        try{
            $values=@(Seal-OwnedLeaf $path $length);Resource-Hashes 'pre';Resource-Hashes 'post'
            $owner=@($ownedLeaves|Where-Object path -CEQ $path)[0];$first=@($e.resources|Where-Object id -CEQ 'tail-control')
            if($values.Count -ne 0 -or $owner.length -ne $length -or $owner.sha256 -cne $expected -or $first.Count -ne 2 -or @($first|Where-Object {$_.sha256 -cne $expected -or $_.fullHash -ne $true}).Count){throw 'Full tail hash or independent fresh pre/post scan missing'}
            $stream=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Write,[IO.FileShare]::None)
            try{$stream.Position=$length-1;$stream.WriteByte(124);$stream.Flush($true)}finally{$stream.Dispose()}
            $failure=$null;try{Resource-Hashes 'post'}catch{$failure=$_.Exception.Message}
            if(-not $failure -or $e.resources[-1].sha256 -ceq $expected -or $e.resources[-1].fullHash -ne $true -or -not (Test-Path -LiteralPath $path)){throw 'Late tail drift accepted or cached seal reused'}
            $stream=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Write,[IO.FileShare]::None)
            try{$stream.Position=$length-1;$stream.WriteByte(123);$stream.Flush($true)}finally{$stream.Dispose()}
            Resource-Hashes 'post'
            $records=@($e.commands|Select-Object -Skip $before);foreach($r in $records){Assert-OwnedRecord $r $(if($r.exitCode -eq 0){0}else{1}) $false}
            $raw=[IO.File]::ReadAllText((Join-Path $EvidenceDirectory $records[0].stderr))
            if($raw -cnotmatch 'seal-progress' -or $raw -cnotmatch 'seal-complete' -or $raw -cnotmatch 'terminal-complete'){throw 'Raw progress did not survive terminal seal'}
            $detail.commands=$records;$detail.facts=@{length=$length;fullSha256=$expected;diskSeal=$owner;prePost=$first;lateDriftFailure=$failure;restoredFreshPost=$e.resources[-1];resultValues=$values.Count;tracePresent=$true}
        }finally{try{Cleanup-Owned}finally{$script:resources=$priorResources;$script:ownedLeaves=$priorLeaves;$script:ownedDirectories=$priorDirectories;$e.cleanup=$priorCleanup}}
    }
    Timing-Case 'actual slow ordinary phase times out with numeric closure and restoration still executes' {
        param($detail)
        $failure=$null
        try{Invoke-TimedPhase 'default' {[void](Invoke-OwnedCommand 'timing-slow' $shell @('-NoProfile','-NonInteractive','-Command','Start-Sleep -Seconds 30; exit 0') -Seconds 1)}}catch{$failure=$_.Exception.ToString()}
        $row=@($e.commands|Where-Object name -CEQ 'timing-slow')[0];Assert-OwnedRecord $row 1 $true
        if(-not $failure -or -not (Test-Path -LiteralPath $smallPath)){throw 'Slow timeout/refusal or owned fixture lost'}
        Invoke-TimedPhase 'restoration' {[void](Invoke-OwnedCommand 'timing-restoration' $shell @('-NoProfile','-NonInteractive','-Command','exit 0') -Restoration -Seconds 30)}
        $positive=@($e.commands|Where-Object name -CEQ 'timing-restoration')[0];Assert-OwnedRecord $positive 0 $false
        $detail.commands=@($row,$positive);$detail.facts=@{failure=$failure;restorationExecuted=$true;ownedFixtureStillPresent=$true}
    }
    Timing-Case 'production preflight phase admits actual fixture inventory with closure reserve' {
        param($detail)
        $priorWorkspace=$Workspace;$probeRoot=Join-Path $OutputDirectory 'preflight-fixture'
        $probeDirectory=Join-Path $probeRoot 'training-data/reference-corpora';[void][IO.Directory]::CreateDirectory($probeDirectory)
        $probeFile=Join-Path $probeDirectory 'tiny.txt';$stream=[IO.File]::Open($probeFile,[IO.FileMode]::CreateNew)
        try{$stream.Write($smallBytes,0,$smallBytes.Length)}finally{$stream.Dispose()}
        $before=$e.commands.Count
        try{$script:Workspace=$probeRoot;$script:preflightInventory=$null
            Invoke-TimedPhase 'preflight' {$script:preflightInventory=Fixture-Inventory}
            $row=$e.commands[-1];Assert-OwnedRecord $row 0 $false
            if($preflightInventory.files.Count -ne 1 -or $preflightInventory.files[0].sha256 -cne $smallHash -or
                $e.timing.phases[-1].completed -ne $true -or $e.commands.Count -ne $before+1 -or $e.timing.operations[-1].completed -ne $true){throw 'Production preflight did not complete exact small inventory'}
            $detail.commands=@($row);$detail.metadataWrites=@($e.timing.metadataWrites|Where-Object {$_.startUtc -ge $row.startUtc})
            $detail.facts=@{phase=$e.timing.phases[-1];inventoryDigest=$preflightInventory.digest;receipt=$e.timing.operations[-1].receipt;fullFixtureHash=$smallHash}
        }finally{$script:Workspace=$priorWorkspace}
    }
    Timing-Case 'late native admission records refusal before process effects' {
        param($detail)
        $before=$e.commands.Count;$script:phaseDeadline=[DateTimeOffset]::UtcNow.AddSeconds(60);$script:phaseName='native';$failure=$null
        try{[void](Invoke-OwnedCommand 'native3' $shell @('-NoProfile','-NonInteractive','-Command','exit 0') -Seconds $timingPolicy.nativeGroup.commandSeconds)}catch{$failure=$_.Exception.ToString()}
        if(-not $failure -or $e.commands.Count -ne $before -or $e.timing.admissions.Count -ne 1 -or $e.timing.admissions[0].effectsStarted -ne $false){throw 'Late admission started effects or omitted reason'}
        $detail.facts=@{refusal=$e.timing.admissions[0];commandsStarted=0}
        $script:phaseDeadline=[DateTimeOffset]::UtcNow.AddSeconds(600);$script:phaseName='closure'
    }
    Timing-Case 'stale candidate operation receipt cannot alter prior custody' {
        param($detail)
        $requestPath=Join-Path $EvidenceDirectory 'operation-003.request.json'
        $request=Get-Content -LiteralPath $requestPath -Raw|ConvertFrom-Json -AsHashtable -Depth 100
        $state=Get-Content -LiteralPath ($requestPath+'.result.json') -Raw|ConvertFrom-Json -AsHashtable -Depth 100
        # Promotion already completed. Restore request's prior state for this import-only probe.
        $oldLeaves=$ownedLeaves;$oldDirectories=$ownedDirectories
        $script:ownedLeaves=[Collections.Generic.List[object]]::new();foreach($row in $request.ownedLeaves){$script:ownedLeaves.Add($row)}
        $script:ownedDirectories=[Collections.Generic.List[object]]::new();foreach($row in $request.ownedDirectories){$script:ownedDirectories.Add($row)}
        try{$state.candidate='0'*40;$failure=$null
            try{Assert-OperationReceipt $state $request (Hash-Bytes ([IO.File]::ReadAllBytes($requestPath))) ([S04NativeCustody]::Identity($requestPath))}catch{$failure=$_.Exception.Message}
            if(-not $failure -or $ownedLeaves[0].path -cne $request.ownedLeaves[0].path){throw 'Stale receipt adopted custody'};$detail.facts=@{failure=$failure;custodyUnchanged=$true}
        }finally{$script:ownedLeaves=$oldLeaves;$script:ownedDirectories=$oldDirectories}
    }
    Timing-Case 'changed owned content is preserved then restored bytes clean successfully' {
        param($detail)
        $before=$e.commands.Count
        [IO.File]::WriteAllText($smallPath,'evil',$utf8);Cleanup-Owned
        if($e.cleanup.completed -or -not (Test-Path -LiteralPath $smallPath)){throw 'Changed resource was deleted/accepted'}
        $failures=@($e.cleanup.failures);[IO.File]::WriteAllBytes($smallPath,$smallBytes)
        $e.cleanup=@{completed=$false;failures=@();leaves=@();directories=@();activeProcesses=$null};Cleanup-Owned
        if(-not $e.cleanup.completed -or (Test-Path -LiteralPath $smallRoot)){throw 'Restored positive did not clean owned fixture'}
        $detail.commands=@($e.commands|Select-Object -Skip $before);$detail.facts=@{negativeFailures=$failures;restoredCleanup=$true;rootAbsent=$true}
    }
    Timing-Case 'actual later noncleanup terminal import retains accepted leaf and directory absence' {
        param($detail)
        $before=$e.commands.Count;$snapshot=(@{leaves=@($ownedLeaves);directories=@($ownedDirectories)}|ConvertTo-Json -Depth 20 -Compress)
        [void](Invoke-BoundedEvidenceOperation 'Capture-SourceAfter' @() 30)
        $row=$e.commands[-1];Assert-OwnedRecord $row 0 $false
        if($e.commands.Count -ne $before+1 -or (@{leaves=@($ownedLeaves);directories=@($ownedDirectories)}|ConvertTo-Json -Depth 20 -Compress) -cne $snapshot -or
            $ownedLeaves[0].absentAfter -ne $true -or $ownedLeaves[0].deletedByCleanup -ne $true -or
            @($ownedDirectories|Where-Object {$_.absentAfter -ne $true -or $_.deletedByCleanup -ne $true}).Count){throw 'Accepted cleanup absence was lost on exact readback'}
        $detail.commands=@($row);$detail.facts=@{operation=$e.timing.operations[-1];leafAbsent=$true;directoriesAbsent=$true;custodyUnchanged=$true}
    }
    # Each mutation invokes the actual production import, starting from accepted physical cleanup.
    $absenceMutations=@(
        @{name='leaf path';edit={param($s) $s.ownedLeaves[0].path+='-foreign'}},
        @{name='leaf identity';edit={param($s) $s.ownedLeaves[0].identity='00000000:0000000000000000'}},
        @{name='leaf birth';edit={param($s) $s.ownedLeaves[0].creationTicks++}},
        @{name='leaf hash';edit={param($s) $s.ownedLeaves[0].sha256='0'*64}},
        @{name='leaf length';edit={param($s) $s.ownedLeaves[0].length++}},
        @{name='leaf false absence';edit={param($s) $s.ownedLeaves[0].absentAfter=$false}},
        @{name='leaf missing deletion witness';edit={param($s) $s.ownedLeaves[0].deletedByCleanup=$false}},
        @{name='leaf reparse';edit={param($s) $s.ownedLeaves[0].reparse=$true}},
        @{name='leaf lost owner';edit={param($s) $s.ownedLeaves=@()}},
        @{name='leaf duplicate owner';edit={param($s) $s.ownedLeaves+=,$s.ownedLeaves[0]}},
        @{name='directory path';edit={param($s) $s.ownedDirectories[0].path+='-foreign'}},
        @{name='directory identity';edit={param($s) $s.ownedDirectories[0].identity='00000000:0000000000000000'}},
        @{name='directory birth';edit={param($s) $s.ownedDirectories[0].creationTicks++}},
        @{name='directory false absence';edit={param($s) $s.ownedDirectories[0].absentAfter=$false}},
        @{name='directory missing deletion witness';edit={param($s) $s.ownedDirectories[0].deletedByCleanup=$false}},
        @{name='directory reparse';edit={param($s) $s.ownedDirectories[0].reparse=$true}},
        @{name='directory lost owner';edit={param($s) $s.ownedDirectories=@()}},
        @{name='directory duplicate owner';edit={param($s) $s.ownedDirectories+=,$s.ownedDirectories[0]}}
    )
    foreach($mutation in $absenceMutations){
        Timing-Case ("accepted absence rejects changed "+$mutation.name) {
            param($detail)
            $snapshot=(@{ownedLeaves=@($ownedLeaves);ownedDirectories=@($ownedDirectories)}|ConvertTo-Json -Depth 20 -Compress)
            $state=$snapshot|ConvertFrom-Json -AsHashtable -Depth 20;& $mutation.edit $state
            $request=@{arguments=@();allowedLeafPaths=@();allowedDirectoryPaths=@();deadlineUtc=[DateTimeOffset]::UtcNow.AddSeconds(30).ToString('o')}
            $failure=$null;try{Import-OperationOwnership $state $request 'Capture-SourceAfter'}catch{$failure=$_.Exception.Message}
            if(-not $failure -or (@{ownedLeaves=@($ownedLeaves);ownedDirectories=@($ownedDirectories)}|ConvertTo-Json -Depth 20 -Compress) -cne $snapshot){throw 'Mutated accepted absence imported or changed prior custody'}
            $detail.facts=@{failure=$failure;custodyUnchanged=$true;mutation=$mutation.name}
        }
    }
    foreach($kind in @('leaf','directory')){
        Timing-Case ("accepted absence rejects resurrected physical "+$kind) {
            param($detail)
            $root=$smallRoot;$leaf=$smallPath
            [void][IO.Directory]::CreateDirectory($root)
            if($kind -ceq 'leaf'){[IO.File]::WriteAllBytes($leaf,$smallBytes)}
            try{$state=@{ownedLeaves=@($ownedLeaves);ownedDirectories=@($ownedDirectories)}
                $request=@{arguments=@();allowedLeafPaths=@();allowedDirectoryPaths=@();deadlineUtc=[DateTimeOffset]::UtcNow.AddSeconds(30).ToString('o')}
                $failure=$null;try{Import-OperationOwnership $state $request 'Capture-SourceAfter'}catch{$failure=$_.Exception.Message}
                if(-not $failure -or -not (Test-Path -LiteralPath $root) -or ($kind -ceq 'leaf' -and -not (Test-Path -LiteralPath $leaf))){throw 'Resurrection imported/deleted'}
                $detail.facts=@{failure=$failure;foreignResurrectionPreservedByProduction=$true}
            }finally{
                # Only these exact test-created resurrection paths are removed by the fixture owner.
                if($kind -ceq 'leaf'){Remove-Item -LiteralPath $leaf};Remove-Item -LiteralPath $root
            }
        }
    }
    foreach($kind in @('leaf','directory')){
        Timing-Case ("noncleanup rejects newly missing unaccepted "+$kind+" owner") {
            param($detail)
            $priorLeaves=$ownedLeaves;$priorDirectories=$ownedDirectories
            $state=(@{ownedLeaves=@($ownedLeaves);ownedDirectories=@($ownedDirectories)}|ConvertTo-Json -Depth 20 -Compress)|ConvertFrom-Json -AsHashtable -Depth 20
            try{
                $script:ownedLeaves=[Collections.Generic.List[object]]::new();foreach($r in $state.ownedLeaves){$script:ownedLeaves.Add(($r|ConvertTo-Json -Depth 10|ConvertFrom-Json -AsHashtable))}
                $script:ownedDirectories=[Collections.Generic.List[object]]::new();foreach($r in $state.ownedDirectories){$script:ownedDirectories.Add(($r|ConvertTo-Json -Depth 10|ConvertFrom-Json -AsHashtable))}
                $owner=if($kind -ceq 'leaf'){$ownedLeaves[0]}else{$ownedDirectories[0]};$owner.absentAfter=$false;$owner.deletedByCleanup=$false
                $request=@{arguments=@();allowedLeafPaths=@();allowedDirectoryPaths=@();deadlineUtc=[DateTimeOffset]::UtcNow.AddSeconds(30).ToString('o')}
                $failure=$null;try{Import-OperationOwnership $state $request 'Capture-SourceAfter'}catch{$failure=$_.Exception.Message}
                if(-not $failure -or $owner.absentAfter -ne $false -or $owner.deletedByCleanup -ne $false){throw 'New disappearance was accepted as prior cleanup'}
                $detail.facts=@{failure=$failure;newDeletionAuthority=$false}
            }finally{$script:ownedLeaves=$priorLeaves;$script:ownedDirectories=$priorDirectories}
        }
    }
    Timing-Case 'actual unsealed partial remains present unsealed and undeletable after cleanup and readback' {
        param($detail)
        $priorCleanup=$e.cleanup;$path=Join-Path $EvidenceDirectory 'unsealed-preserved-control.partial'
        $priorResources=$resources;$script:resources+=,@{pin=@{id='unsealed-control';length=11;sha256='0'*64};path=$path.Substring(0,$path.Length-8)}
        [void](Invoke-BoundedEvidenceOperation 'Write-OwnedManifest' @('interrupted',$path) 30)
        $owner=@($ownedLeaves|Where-Object path -CEQ $path)[0];$identity=$owner.identity;$ticks=$owner.creationTicks
        Cleanup-Owned;[void](Invoke-BoundedEvidenceOperation 'Capture-SourceAfter' @() 30)
        $owner=@($ownedLeaves|Where-Object path -CEQ $path)[0]
        if($e.cleanup.completed -or -not (Test-Path -LiteralPath $path) -or $owner.sha256 -or $null -ne $owner.length -or $owner.absentAfter -ne $false -or $owner.deletedByCleanup -ne $false -or
            $owner.identity -cne $identity -or $owner.creationTicks -ne $ticks){throw 'Unsealed leaf was sealed/deleted/adopted during closure'}
        $detail.facts=@{identity=$identity;creationTicks=$ticks;present=$true;unsealed=$true;cleanupCompleted=$false;failures=@($e.cleanup.failures)}
        # Preserve this exact fixture in the raw evidence tree; no cleanup authority is manufactured.
        $e.cleanup=$priorCleanup
        $script:resources=$priorResources
    }
    Timing-Case 'missing terminal receipt leaves unknown effects and prevents completed cleanup' {
        param($detail)
        $failure=$null;try{[void](Invoke-BoundedEvidenceOperation 'S04-unrecognized-operation-control' @() 30)}catch{$failure=$_.Exception.ToString()}
        if(-not $failure -or $e.timing.unknownEffects -ne $true){throw 'Missing terminal receipt silently accepted'}
        $row=$e.commands[-1];Assert-OwnedRecord $row 1 $false
        $e.cleanup=@{completed=$false;failures=@();leaves=@();directories=@();activeProcesses=$null};Cleanup-Owned
        if($e.cleanup.completed -or $e.timing.unknownEffects -ne $true){throw 'Unknown effects released completion'}
        $detail.commands=@($row);$detail.facts=@{failure=$failure;unknownEffects=$true;cleanupCompleted=$false}
    }
    Timing-Case 'oversized control rejected before read and original terminal control remains valid' {
        param($detail)
        $path=Join-Path $EvidenceDirectory 'oversized-control.bin';$stream=[IO.File]::Open($path,[IO.FileMode]::CreateNew)
        try{$stream.SetLength(16MB+1)}finally{$stream.Dispose()}
        $failure=$null;try{[void](Read-ControlBytes $path)}catch{$failure=$_.Exception.Message}
        if(-not $failure -or $failure -cnotmatch 'exceeds16MiB'){throw 'Oversized controller input read/accepted'}
        $bytes=Read-ControlBytes (Join-Path $EvidenceDirectory 'operation-003.request.json.result.json')
        if($bytes.Length -le 0 -or $bytes.Length -gt 16MB){throw 'Original control not restored/size-bound'}
        $detail.facts=@{failure=$failure;refusedBytes=16MB+1;originalControlBytes=$bytes.Length;restoredOriginalRead=$true}
    }
    Timing-Case 'measured pre-native timeline fits full1800 group with restoration and evidence reserves' {
        param($detail)
        $elapsed=27*60;$available=$regularSeconds-$elapsed
        if($available -lt 2000 -or $cleanupReserveSeconds -ne 2940 -or ($overallDeadline-$runStart).TotalSeconds -ne 10340){throw 'Measured timeline still cannot fit full native allowance'}
        $detail.facts=@{measuredPreparationSeconds=$elapsed;regularRemainingSeconds=$available;nativeProcessSeconds=1800;restorationAndClosureSeconds=2940;jobSeconds=$timingPolicy.jobSeconds}
    }
    Timing-Case 'T06 actual native caller requests group1800 and scopes env only after admission' {
        param($detail)
        $calls=@($runnerAst.FindAll({param($n)$n -is [Management.Automation.Language.CommandAst] -and $n.GetCommandName() -ceq 'Invoke-TimedPhase' -and $n.CommandElements[1].Extent.Text -ceq "'native'"},$true))
        if($calls.Count -ne 1){throw 'Actual native caller drift'}
        $action=$calls[0].CommandElements[2].ScriptBlock.Extent.Text
        $module=New-Module -ArgumentList @($action) -ScriptBlock {
            param($action)
            $script:action=[scriptblock]::Create($action.Substring(1,$action.Length-2));$script:allowed=$true;$script:calls=@();$script:e=@{timing=@{admissions=@()}}
            $script:timingPolicy=Get-NativeTimingPolicy;$script:EvidenceDirectory='C:\control\s04-native-resource-evidence';$script:CandidateSha='a'*40;$script:dotnet='dotnet';$script:filter='exact existing identity filter'
            function Get-OwnedCommandAdmission([string]$name,[int]$Seconds){$script:calls+=,@{kind='admission';name=$name;seconds=$Seconds;path=[Environment]::GetEnvironmentVariable('S04_NATIVE_STAGE_PATH','Process')};@{now=[DateTimeOffset]::UtcNow;availableSeconds=1799.999;reason=$(if($script:allowed){$null}else{'controlled full-group refusal'})}}
            function Write-Json([string]$path,$value) {} # Preserve refusal metadata shape without starting a writer in this caller seam.
            function Invoke-OwnedCommand([string]$name,[string]$executable,[string[]]$arguments,[int]$Seconds){$script:calls+=,@{kind='invoke';name=$name;seconds=$Seconds;argv=$arguments;path=[Environment]::GetEnvironmentVariable('S04_NATIVE_STAGE_PATH','Process')};if(-not $script:allowed){throw 'controlled full-group refusal'}}
            function Probe([bool]$allowed){$script:allowed=$allowed;$script:calls=@();$failure=$null;try{& $script:action}catch{$failure=$_.Exception.Message};@{calls=$script:calls;failure=$failure;after=[Environment]::GetEnvironmentVariable('S04_NATIVE_STAGE_PATH','Process')}}
            Export-ModuleMember -Function @() -Variable @()
        }
        $callerPrior=@{};foreach($key in @('S04_NATIVE_STAGE_PATH','S04_NATIVE_STAGE_CANDIDATE','S04_NATIVE_STAGE_RUN')){$callerPrior[$key]=[Environment]::GetEnvironmentVariable($key,'Process')}
        try{
            [Environment]::SetEnvironmentVariable('S04_NATIVE_STAGE_PATH','prior-control','Process')
            $positive=& $module {Probe $true};$negative=& $module {Probe $false};$restored=& $module {Probe $true}
            if($positive.failure -or $restored.failure -or $positive.calls.Count -ne 2 -or $positive.calls[0].path -cne 'prior-control' -or $positive.calls[1].seconds -ne 1800 -or
                $positive.calls[1].path -cne 'C:\control\s04-native-resource-evidence\native3-progress.ndjson' -or $positive.after -cne 'prior-control' -or
                $negative.failure -cne 'controlled full-group refusal' -or @($negative.calls|Where-Object path -CNE 'prior-control').Count -or $negative.after -cne 'prior-control'){throw 'Actual caller started trace effects before admission or failed env restore'}
            $restoreProof=@()
            foreach($key in @('S04_NATIVE_STAGE_PATH','S04_NATIVE_STAGE_CANDIDATE','S04_NATIVE_STAGE_RUN')){
            foreach($case in @(@{name='null';value=[NullString]::Value},@{name='empty';value=''})){
                [Environment]::SetEnvironmentVariable($key,$case.value,'Process')
                $beforeValue=[Environment]::GetEnvironmentVariable($key,'Process')
                if(($case.name -ceq 'null') -ne ($null -eq $beforeValue)){throw 'Requested null/empty fixture was not actually distinct'}
                $caseResult=& $module {Probe $true}
                $afterValue=[Environment]::GetEnvironmentVariable($key,'Process')
                if($caseResult.failure -or (($null -eq $beforeValue) -ne ($null -eq $afterValue)) -or ($null -ne $beforeValue -and $beforeValue -cne $afterValue)){throw 'Actual CLR null/empty environment restore changed prior value'}
                $restoreProof+=@{key=$key;requested=$case.name;beforeNull=$null -eq $beforeValue;afterNull=$null -eq $afterValue;beforeLength=$(if($null -eq $beforeValue){$null}else{$beforeValue.Length});afterLength=$(if($null -eq $afterValue){$null}else{$afterValue.Length});result=$caseResult}
            }
            }
            $detail.facts=@{actualCaller=$action;positive=$positive;negative=$negative;restored=$restored;nullEmptyRestoration=$restoreProof}
        }finally{foreach($key in $callerPrior.Keys){if($null -eq $callerPrior[$key]){[Environment]::SetEnvironmentVariable($key,[NullString]::Value,'Process')}else{[Environment]::SetEnvironmentVariable($key,$callerPrior[$key],'Process')}};Remove-Module -ModuleInfo $module -Force}
    }
    Timing-Case 'T06 removing actual native full allowance guard makes shortage negative fail' {
        param($detail)
        $definition=@($runnerAst.FindAll({param($n)$n -is [Management.Automation.Language.FunctionDefinitionAst] -and $n.Name -ceq 'Get-OwnedCommandAdmission'},$true))[0]
        $guard=@($definition.FindAll({param($n)$n -is [Management.Automation.Language.IfStatementAst] -and $n.Extent.Text -cmatch '^if\(\$name -ieq ''native3''\)'},$true))
        if($guard.Count -ne 1){throw 'Actual full-group guard not unique'}
        $source=$definition.Extent.Text;$offset=$guard[0].Extent.StartOffset-$definition.Extent.StartOffset
        $mutated=$source.Substring(0,$offset)+"if(`$Seconds -lt 1 -or `$Seconds -gt 1800){`$reason='controlled bounds'}"+$source.Substring($offset+$guard[0].Extent.Text.Length)
        # The if AST includes elseif ordinary/build branches: replacing it isolates the native enforcement counterfactual.
        $module=New-Module -ArgumentList @($source,$mutated) -ScriptBlock {
            param($source,$mutated)
            $script:overallDeadline=[DateTimeOffset]::Parse('2026-10-10T02:52:20Z');$script:cleanupReserveSeconds=2940;$script:phaseName='native'
            $now=[DateTimeOffset]::Parse('2026-10-10T00:00:00Z');$script:phaseDeadline=$now.AddSeconds(1979.999)
            . ([scriptblock]::Create($source));$negative=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now
            . ([scriptblock]::Create($mutated));$counterfactual=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now
            . ([scriptblock]::Create($source));$script:phaseDeadline=$now.AddSeconds(1980);$restored=Get-OwnedCommandAdmission 'native3' -Seconds 1800 -Now $now
            function Result {@{negative=$negative;guardRemoved=$counterfactual;restored=$restored}};Export-ModuleMember -Function Result
        }
        try{$proof=& $module {Result};if(-not $proof.negative.reason -or $proof.guardRemoved.reason -or $proof.restored.reason){throw 'Full-group negative is not sensitive to its actual guard'};$detail.facts=$proof}finally{Remove-Module -ModuleInfo $module -Force}
    }
    # Compile only the actual diagnostic class extracted from the opt-in source. No native/resource recipient is loaded.
    $nativeSourcePath=Join-Path $Workspace 'Tests/CodeDocumentationConfiguredQwenNativeResourceTests.cs'
    $nativeSource=[IO.File]::ReadAllText($nativeSourcePath)
    $nativeTree=[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($nativeSource)
    $helperNodes=@($nativeTree.GetRoot().DescendantNodes()|Where-Object {$_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax] -and $_.Identifier.ValueText -ceq 'NativeResourceTestStageTrace'})
    if($helperNodes.Count -ne 1){throw 'Actual source trace class extraction drift'}
    $helperText=$helperNodes[0].ToFullString()
    Timing-Case 'T07 D03 exact three Fact identities and every original third invocation retained' {
        param($detail)
        # Immutable pre-diagnostic baseline, independently approved by CODE10 on 380582dd80e0b13774627e99f13b50f840d17420.
        # Original native source: 6953 bytes, SHA256 56ae4ec1231712c15a91a2642af8ab68d78d59ca70f6b738cd145ccc1046112f.
        # CODE10 regression01/result.json: SHA256 e294c774792107042d05593344a8f1014b90966e659ca89271169b350823317d.
        # Roslyn preorder InvocationExpressionSyntax.ToString(), whitespace runs replaced by one space;
        # ordered UTF8 join('|') commitment. No HEAD/history/fetch dependency or candidate-as-baseline comparison.
        $originalIdentities=@('NativeInstalledVocabulary_ExactVectorsAndAbi','HeldCompleteContext_NativePrepareAndOverflow','NativeResourceDriftAndCancellation')
        $originalInvocationSha256='942a5657ce4d175f02b4fcebfb17ed164d4a6f431d3cfa3c604a47ee56a68126'
        $before=@'
[
  "Open()",
  "QwenAdmissionTestSupport.Configuration(installed: true)",
  "H.TeacherContextHarness()",
  "H.BindTeacherCompleted(h)",
  "h.Files()",
  "h.HistoryBytes()",
  "h.Open.Session.PrepareOrdinarySelectedTeacherRequestNativeResourcesSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, null, H.AssessTeacherHistory, configuration, native)",
  "Assert.True(prepared.IsSuccess, prepared.ErrorCode + \":\" + prepared.ErrorMessage)",
  "Assert.Equal(\"NativeConfigured\", prepared.Value!.Request.TokenizerOrigin)",
  "Assert.Equal(\"SyntheticTest\", prepared.Value.Request.SourceOrigin)",
  "Assert.InRange(prepared.Value.Request.InputTokens, 1, 6144)",
  "Assert.Equal(2048, prepared.Value.Request.OutputReserve)",
  "QwenAdmissionTestSupport.AssertCompleteRendered(prepared.Value.Request.Rendered, h, b)",
  "QwenAdmissionTestSupport.AssertBody(prepared.Value.Request)",
  "Assert.False(prepared.Value.FullTeacherContextReady)",
  "Assert.Equal(10, h.Client.Calls)",
  "Assert.Equal(history, h.HistoryBytes())",
  "h.HistoryBytes()",
  "h.AssertFiles(files)",
  "H.TeacherContextHarness(chosenHistorySuffix: string.Concat(Enumerable.Repeat(\"a \", 6400)))",
  "string.Concat(Enumerable.Repeat(\"a \", 6400))",
  "Enumerable.Repeat(\"a \", 6400)",
  "H.BindTeacherCompleted(h)",
  "h.Files()",
  "h.HistoryBytes()",
  "h.Open.Session.PrepareOrdinarySelectedTeacherRequestNativeResourcesSyntheticForTestingAsync(b, h.Catalog, h.Fixture.OrdinarySelector, a, null, H.AssessTeacherHistory, configuration, native)",
  "Assert.False(result.IsSuccess)",
  "Assert.Null(result.Value)",
  "Assert.Equal(ConfiguredQwenVocabularyLease.BudgetExceeded, result.ErrorCode)",
  "Assert.Equal(history, h.HistoryBytes())",
  "h.HistoryBytes()",
  "Assert.Equal(10, h.Client.Calls)",
  "Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting)",
  "h.AssertFiles(files)"
]
'@ | ConvertFrom-Json
        $baselineHash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($utf8.GetBytes(($before -join '|')))).ToLowerInvariant()
        if($before.Count -ne 34 -or $baselineHash -cne $originalInvocationSha256){throw 'Independent original invocation baseline commitment changed'}
        function Fact-Methods($tree){@($tree.GetRoot().DescendantNodes()|Where-Object {$_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] -and @($_.AttributeLists|ForEach-Object {$_.Attributes}|Where-Object {$_ -and $_.Name.ToString() -ceq 'Fact'}).Count})}
        function Assert-OriginalNativeSemantics([string]$source){
            $tree=[Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
            if(@($tree.GetDiagnostics()|Where-Object Severity -EQ 'Error').Count){throw 'Native source syntax invalid'}
            $facts=Fact-Methods $tree
            if($facts.Count -ne 3 -or ($facts.Identifier.ValueText -join '|') -cne ($originalIdentities -join '|')){throw 'Original three Facts changed'}
            $method=@($facts|Where-Object {$_.Identifier.ValueText -ceq 'HeldCompleteContext_NativePrepareAndOverflow'})[0]
            # Only the already admitted SOURCE10 diagnostics/lifetime expressions are outside this semantic inventory.
            $calls=@($method.DescendantNodes()|Where-Object {$_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and $_.Expression.ToString() -cnotin @('NativeResourceTestStageTrace.FromEnvironment','trace.Stage','native.Dispose')}|ForEach-Object {$_.ToString() -replace '\s+',' '})
            $hash=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($utf8.GetBytes(($calls -join '|')))).ToLowerInvariant()
            if($calls.Count -ne 34 -or $hash -cne $originalInvocationSha256 -or ($calls -join '|') -cne ($before -join '|')){throw 'Original third calls/assertions/context or accounting changed'}
            @{identities=$facts.Identifier.ValueText;invocations=$calls;invocationsSha256=$hash}
        }
        $positive=Assert-OriginalNativeSemantics $nativeSource
        $mutationProof=@()
        foreach($mutation in @(
            @{name='semantic invocation deleted';old='QwenAdmissionTestSupport.AssertBody(prepared.Value.Request);';new='';expected='Original third calls/assertions/context or accounting changed'},
            @{name='semantic assertion changed';old='Assert.Null(result.Value);';new='Assert.NotNull(result.Value);';expected='Original third calls/assertions/context or accounting changed'},
            @{name='history repeats changed';old='Enumerable.Repeat("a ", 6400)';new='Enumerable.Repeat("a ", 6401)';expected='Original third calls/assertions/context or accounting changed'},
            @{name='context assessor changed';old='a, null, H.AssessTeacherHistory, configuration, native';new='a, null, null, configuration, native';expected='Original third calls/assertions/context or accounting changed'},
            @{name='student accounting changed';old='Assert.Equal((1, 1, false), h.Open.Session.SelectedStudentAccounting)';new='Assert.Equal((1, 2, false), h.Open.Session.SelectedStudentAccounting)';expected='Original third calls/assertions/context or accounting changed'},
            @{name='original Fact identity changed';old='public async Task HeldCompleteContext_NativePrepareAndOverflow()';new='public async Task HeldCompleteContext_NativePrepareAndOverflowChanged()';expected='Original three Facts changed'})){
            $occurrences=([regex]::Matches($nativeSource,[regex]::Escape($mutation.old))).Count
            if($occurrences -lt 1){throw ('Actual source mutation target missing: '+$mutation.name)}
            $mutated=$nativeSource.Replace($mutation.old,$mutation.new)
            $failure=$null;try{[void](Assert-OriginalNativeSemantics $mutated)}catch{$failure=$_.Exception.Message}
            if($failure -cne $mutation.expected){throw ('Original preservation negative failed: '+$mutation.name+'; observed='+$failure)}
            $restored=Assert-OriginalNativeSemantics $nativeSource
            $mutationProof+=@{name=$mutation.name;occurrences=$occurrences;old=$mutation.old;replacement=$mutation.new;expected=$mutation.expected;observed=$failure;refused=$true;restoredInvocationsSha256=$restored.invocationsSha256;nativeExecution=$false}
        }
        $detail.facts=@{identities=$positive.identities;originalInvocations=$before;currentInvocations=$positive.invocations;originalInvocationsSha256=$baselineHash;currentInvocationsSha256=$positive.invocationsSha256;
            baselineCommit='380582dd80e0b13774627e99f13b50f840d17420';baselineSourceSha256='56ae4ec1231712c15a91a2642af8ab68d78d59ca70f6b738cd145ccc1046112f';baselineIndependentResultSha256='e294c774792107042d05593344a8f1014b90966e659ca89271169b350823317d';mutations=$mutationProof;
            historyRepeats=6400;studentCalls=10;outputReserve=2048;fullTeacherContextReady=$false;nativeExecution=$false}
    }
    $probe=@'
public static class S04StageTraceProbe {
    public static object Open() => NativeResourceTestStageTrace.FromEnvironment();
    public static void Stage(object value,string stage,string edge) => ((NativeResourceTestStageTrace)value).Stage(stage,edge);
    public static void Dispose(object value) => ((NativeResourceTestStageTrace)value).Dispose();
    public static string Status(object value) => ((NativeResourceTestStageTrace)value).FailureStatus;
    public static string Type(object value) => ((NativeResourceTestStageTrace)value).FailureType;
    public static void BreakStream(object value) { var f=typeof(NativeResourceTestStageTrace).GetField("stream",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance); ((FileStream)f.GetValue(value)).Dispose(); }
    public static void Alive() { using(var trace=NativeResourceTestStageTrace.FromEnvironment()) { trace.Stage("fact","start"); trace.Stage("native-open","start"); System.Threading.Thread.Sleep(60000); } }
    public static bool PreservePrimary() {
        var trace=NativeResourceTestStageTrace.FromEnvironment(); BreakStream(trace);
        using(var cancel=new System.Threading.CancellationTokenSource()) {
            cancel.Cancel(); var primary=new OperationCanceledException(cancel.Token); int cleanup=0;
            try { try { throw primary; } finally { trace.Stage("fact","start"); cleanup++; trace.Dispose(); } }
            catch(OperationCanceledException observed) { return object.ReferenceEquals(primary,observed) && observed.CancellationToken==cancel.Token && cleanup==1 && trace.FailureStatus=="WRITE_FAILED" && trace.FailureType=="DISPOSED"; }
        }
    }
    private sealed class DisposeFaultStream : FileStream {
        public DisposeFaultStream(string path):base(path,FileMode.Open,FileAccess.Write,FileShare.Read){}
        protected override void Dispose(bool disposing) { base.Dispose(disposing); if(disposing) throw new IOException("probe-only disposal fault"); }
    }
    public static bool PreserveDisposePrimary(string path) {
        var trace=NativeResourceTestStageTrace.FromEnvironment(); BreakStream(trace);
        var field=typeof(NativeResourceTestStageTrace).GetField("stream",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance);
        field.SetValue(trace,new DisposeFaultStream(path));
        var primary=new InvalidOperationException("probe-only primary"); int cleanup=0;
        try { try { throw primary; } finally { cleanup++; trace.Dispose(); } }
        catch(InvalidOperationException observed) { return object.ReferenceEquals(primary,observed) && cleanup==1 && trace.FailureStatus=="DISPOSE_FAILED" && trace.FailureType=="IO"; }
    }
}
'@
    $exactHelperCode="#nullable enable`nusing System;using System.IO;using System.Text;using System.Diagnostics;using System.Globalization;using System.Text.RegularExpressions;namespace LlmFromScratch.Tests {`n"+$helperText+"`n#nullable disable`n"+$probe+"`n}"
    Add-Type -TypeDefinition $exactHelperCode
    $traceRoot=Join-Path $OutputDirectory 'trace-controls';[void][IO.Directory]::CreateDirectory($traceRoot)
    $traceSource=Join-Path $traceRoot 'actual-helper-child.cs';[IO.File]::WriteAllText($traceSource,$exactHelperCode,$utf8)
    $traceDriver=Join-Path $traceRoot 'actual-helper-child.ps1'
    [IO.File]::WriteAllText($traceDriver,"Add-Type -Path '"+$traceSource.Replace("'","''")+"'; [LlmFromScratch.Tests.S04StageTraceProbe]::Alive()",$utf8)
    $traceKeys=@('S04_NATIVE_STAGE_PATH','S04_NATIVE_STAGE_CANDIDATE','S04_NATIVE_STAGE_RUN','RUNNER_TEMP','GITHUB_ACTIONS','GITHUB_RUN_ID','GATE_NAME','RUNNER_ENVIRONMENT')
    $tracePrior=@{};foreach($key in $traceKeys){$tracePrior[$key]=[Environment]::GetEnvironmentVariable($key,'Process')}
    function Set-TraceControlEnvironment([string]$name){
        $root=Join-Path $traceRoot $name;$parent=Join-Path $root 's04-native-resource-evidence';[void][IO.Directory]::CreateDirectory($parent)
        $path=Join-Path $parent 'native3-progress.ndjson'
        foreach($pair in @(@('RUNNER_TEMP',$root),@('GITHUB_ACTIONS','true'),@('GITHUB_RUN_ID','12345'),@('GATE_NAME','S04.04A-NATIVE-RESOURCE'),@('RUNNER_ENVIRONMENT','github-hosted'),@('S04_NATIVE_STAGE_PATH',$path),@('S04_NATIVE_STAGE_CANDIDATE',('a'*40)),@('S04_NATIVE_STAGE_RUN','12345'))){[Environment]::SetEnvironmentVariable($pair[0],$pair[1],'Process')}
        $path
    }
    function Read-LiveTrace([string]$path){
        $stream=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Read,[IO.FileShare]::ReadWrite)
        try{$reader=[IO.StreamReader]::new($stream,$utf8);$reader.ReadToEnd()}finally{if($reader){$reader.Dispose()}else{$stream.Dispose()}}
    }
    try{
        Timing-Case 'D01 actual extracted writer prefix is readable alive and survives exact owned Job kill' {
            param($detail)
            $path=Set-TraceControlEnvironment 'alive';$job=$null;$started=$null;$out=[IO.MemoryStream]::new();$err=[IO.MemoryStream]::new();$outTask=$null;$errTask=$null
            $record=@{pid=$null;birthUtc=$null;exitCode=$null;activeAfterCleanup=$null;assignedBeforeResume=$false;readWhileAlive=$false;prefix=$null}
            try{
                $job=[S04NativeOwnedJob]::Create();$started=$job.StartSuspendedAndAssign((Get-Command pwsh -CommandType Application).Source,@('-NoProfile','-NonInteractive','-File',$traceDriver),$Workspace)
                $record.pid=$started.Process.Id;$record.birthUtc=$started.Process.StartTime.ToUniversalTime().ToString('o');$record.assignedBeforeResume=$true
                $outTask=$started.StandardOutput.CopyToAsync($out);$errTask=$started.StandardError.CopyToAsync($err);$readDeadline=[DateTimeOffset]::UtcNow.AddSeconds(30)
                do{
                    if(Test-Path -LiteralPath $path){$prefix=Read-LiveTrace $path;if(($prefix -split "`n"|Where-Object {$_}).Count -eq 3){break}}
                    if($started.Process.HasExited){throw 'Managed writer exited before observable prefix'};Start-Sleep -Milliseconds 50
                }while([DateTimeOffset]::UtcNow -lt $readDeadline)
                if($started.Process.HasExited -or -not $prefix -or $prefix -notmatch '"stage":"native-open","edge":"start"'){throw 'No actual flushed prefix while child alive'}
                $record.readWhileAlive=$true;$record.prefix=$prefix
            }finally{
                if($job){$job.Terminate();$stop=[DateTimeOffset]::UtcNow.AddSeconds(10);do{$record.activeAfterCleanup=[int]$job.GetActiveProcessCount();if($record.activeAfterCleanup -eq 0){break};Start-Sleep -Milliseconds 50}while([DateTimeOffset]::UtcNow -lt $stop)
                    if($started -and $started.Process.WaitForExit(10000)){$record.exitCode=$started.Process.ExitCode}
                    foreach($task in @($outTask,$errTask)){if($task -and -not $task.Wait(10000)){throw 'Managed trace stream closure timeout'}}
                    if($started){$started.StandardOutput.Dispose();$started.StandardError.Dispose();$started.Process.Dispose()};$job.Dispose()}
                [IO.File]::WriteAllBytes((Join-Path $traceRoot 'alive.stdout.log'),$out.ToArray());[IO.File]::WriteAllBytes((Join-Path $traceRoot 'alive.stderr.log'),$err.ToArray());$out.Dispose();$err.Dispose();$detail.commands=@($record)
            }
            $retained=Read-LiveTrace $path
            if($record.activeAfterCleanup -ne 0 -or $record.exitCode -ne 1 -or $retained -cne $record.prefix -or $retained -match '"edge":"end"'){throw 'Owned trace prefix lost, fabricated end or nonterminal closure'}
            $detail.facts=@{helperSha256=(Hash-Bytes $utf8.GetBytes($helperText));retained=$retained;nativeFacts=0}
        }
        Timing-Case 'D02 actual trace complete27 bounded frames and retained handle denies replacement' {
            param($detail)
            $path=Set-TraceControlEnvironment 'complete';$trace=[LlmFromScratch.Tests.S04StageTraceProbe]::Open()
            try{
                $denied=$false;try{$attempt=[IO.File]::Open($path,[IO.FileMode]::Open,[IO.FileAccess]::Write,[IO.FileShare]::ReadWrite);$attempt.Dispose()}catch{$denied=$true}
                if(-not $denied){throw 'Trace allows another writer'}
                $deleteDenied=$false;try{[IO.File]::Delete($path)}catch{$deleteDenied=$true};if(-not $deleteDenied){throw 'Trace handle allows deletion'}
                $stages=@('fact.start','native-open.start','native-open.end','fit-harness-create.start','fit-harness-create.end','fit-bind.start','fit-bind.end','fit-prepare.start','fit-prepare.end','fit-assertions.start','fit-assertions.end','fit-harness-dispose.start','fit-harness-dispose.end','overflow-harness-create.start','overflow-harness-create.end','overflow-bind.start','overflow-bind.end','overflow-prepare.start','overflow-prepare.end','overflow-assertions.start','overflow-assertions.end','overflow-harness-dispose.start','overflow-harness-dispose.end','native-dispose.start','native-dispose.end','fact.end')
                foreach($stage in $stages){$pair=$stage.Split('.');[LlmFromScratch.Tests.S04StageTraceProbe]::Stage($trace,$pair[0],$pair[1])}
                if([LlmFromScratch.Tests.S04StageTraceProbe]::Status($trace)){throw 'Valid actual stage sequence failed'}
                [LlmFromScratch.Tests.S04StageTraceProbe]::Stage($trace,'fact','end')
                if([LlmFromScratch.Tests.S04StageTraceProbe]::Status($trace) -cne 'WRITE_FAILED' -or [LlmFromScratch.Tests.S04StageTraceProbe]::Type($trace) -cne 'INVALID_DATA'){throw 'Duplicate stage extended bounded trace'}
            }finally{[LlmFromScratch.Tests.S04StageTraceProbe]::Dispose($trace)}
            $frames=@((Read-LiveTrace $path).Split("`n")|Where-Object {$_})
            if($frames.Count -ne 27 -or (Get-Item -LiteralPath $path).Length -gt 16384 -or @($frames|Where-Object {$utf8.GetByteCount($_+"`n") -gt 512}).Count){throw 'Trace bounds violated'}
            $objects=@($frames|ForEach-Object {$_|ConvertFrom-Json -AsHashtable});if($objects[0].candidate -cne ('a'*40) -or $objects[0].runId -cne '12345' -or $objects[-1].sequence -ne 26 -or $objects[-1].stage -cne 'fact' -or $objects[-1].edge -cne 'end'){throw 'Trace binding/terminal stage drift'}
            $detail.facts=@{frames=$frames.Count;bytes=(Get-Item -LiteralPath $path).Length;frameMax=($frames|ForEach-Object {$utf8.GetByteCount($_+"`n")}|Measure-Object -Maximum).Maximum;writeDenied=$denied;deleteDenied=$deleteDenied}
        }
        Timing-Case 'D02 actual trace refuses partial malformed outside preexisting hosted bindings before native' {
            param($detail)
            $refusals=@()
            foreach($kind in @('partial','candidate','oversized','run','outside','preexisting','host','gate','runner')){
                $path=Set-TraceControlEnvironment ('refuse-'+$kind)
                switch($kind){'partial'{[Environment]::SetEnvironmentVariable('S04_NATIVE_STAGE_RUN',[NullString]::Value,'Process')}'candidate'{[Environment]::SetEnvironmentVariable('S04_NATIVE_STAGE_CANDIDATE',('A'*40),'Process')}'oversized'{[Environment]::SetEnvironmentVariable('S04_NATIVE_STAGE_CANDIDATE',('a'*513),'Process')}'run'{[Environment]::SetEnvironmentVariable('S04_NATIVE_STAGE_RUN','54321','Process')}'outside'{[Environment]::SetEnvironmentVariable('S04_NATIVE_STAGE_PATH',(Join-Path $traceRoot 'outside.ndjson'),'Process')}'preexisting'{[IO.File]::WriteAllText($path,'retained',$utf8)}'host'{[Environment]::SetEnvironmentVariable('GITHUB_ACTIONS','false','Process')}'gate'{[Environment]::SetEnvironmentVariable('GATE_NAME','S04.04A','Process')}'runner'{[Environment]::SetEnvironmentVariable('RUNNER_ENVIRONMENT','self-hosted','Process')}}
                $refused=$false;try{$trace=[LlmFromScratch.Tests.S04StageTraceProbe]::Open();[LlmFromScratch.Tests.S04StageTraceProbe]::Dispose($trace)}catch{$refused=$true}
                if(-not $refused -or ($kind -cne 'preexisting' -and (Test-Path -LiteralPath $path)) -or ($kind -ceq 'preexisting' -and [IO.File]::ReadAllText($path) -cne 'retained')){throw ('Trace setup effect/refusal violated: '+$kind)}
                $refusals+=@{kind=$kind;refused=$true;nativeOpenCalls=0}
            }
            foreach($key in @('S04_NATIVE_STAGE_PATH','S04_NATIVE_STAGE_CANDIDATE','S04_NATIVE_STAGE_RUN')){[Environment]::SetEnvironmentVariable($key,[NullString]::Value,'Process');if($null -ne [Environment]::GetEnvironmentVariable($key,'Process')){throw 'Missing diagnostic fixture is not absent'}}
            $disabled=[LlmFromScratch.Tests.S04StageTraceProbe]::Open();[LlmFromScratch.Tests.S04StageTraceProbe]::Stage($disabled,'unknown','unknown');[LlmFromScratch.Tests.S04StageTraceProbe]::Dispose($disabled)
            $detail.facts=@{refusals=$refusals;allMissingPreservesDirectSemantics=$true}
        }
        Timing-Case 'D03 actual trace write failure preserves original OCE token object outcome and cleanup' {
            param($detail)
            $path=Set-TraceControlEnvironment 'primary'
            if(-not [LlmFromScratch.Tests.S04StageTraceProbe]::PreservePrimary()){throw 'Diagnostic masked original OCE or cleanup'}
            $disposePath=Set-TraceControlEnvironment 'dispose-primary'
            if(-not [LlmFromScratch.Tests.S04StageTraceProbe]::PreserveDisposePrimary($disposePath)){throw 'Diagnostic disposal masked primary or cleanup'}
            $unknownPath=Set-TraceControlEnvironment 'unknown-stage';$trace=[LlmFromScratch.Tests.S04StageTraceProbe]::Open()
            try{[LlmFromScratch.Tests.S04StageTraceProbe]::Stage($trace,'unknown','start');if([LlmFromScratch.Tests.S04StageTraceProbe]::Status($trace) -cne 'WRITE_FAILED' -or [LlmFromScratch.Tests.S04StageTraceProbe]::Type($trace) -cne 'INVALID_DATA'){throw 'Unknown stage not attributable'}}finally{[LlmFromScratch.Tests.S04StageTraceProbe]::Dispose($trace)}
            if((Read-LiveTrace $path) -match '"edge":"end"' -or (Read-LiveTrace $unknownPath) -match 'unknown'){throw 'Diagnostic fabricated payload/success'}
            $detail.facts=@{primarySameObject=$true;tokenPreserved=$true;cleanupCount=1;outcome='original OCE and original assertion-like exception';writeFailureType='DISPOSED';disposeFailureType='IO';unknownType='INVALID_DATA';nativeFacts=0}
        }
        Timing-Case 'D02 actual trace refuses a test-owned junction ancestor without creating a file' {
            param($detail)
            $target=Join-Path $traceRoot 'reparse-target';$junction=Join-Path $traceRoot 'reparse-junction'
            [void][IO.Directory]::CreateDirectory((Join-Path $target 's04-native-resource-evidence'))
            [void](New-Item -ItemType Junction -Path $junction -Target $target)
            [Environment]::SetEnvironmentVariable('RUNNER_TEMP',$junction,'Process');$path=Join-Path $junction 's04-native-resource-evidence/native3-progress.ndjson';[Environment]::SetEnvironmentVariable('S04_NATIVE_STAGE_PATH',$path,'Process')
            $refused=$false;try{$trace=[LlmFromScratch.Tests.S04StageTraceProbe]::Open();[LlmFromScratch.Tests.S04StageTraceProbe]::Dispose($trace)}catch{$refused=$true}
            if(-not $refused -or (Test-Path -LiteralPath $path)){throw 'Actual reparse ancestor admitted trace effects'}
            $detail.facts=@{refused=$refused;ancestorReparse=$true;nativeOpenCalls=0;retainedTestOwnedJunction=$junction}
        }
    }finally{foreach($key in $traceKeys){if($null -eq $tracePrior[$key]){[Environment]::SetEnvironmentVariable($key,[NullString]::Value,'Process')}else{[Environment]::SetEnvironmentVariable($key,$tracePrior[$key],'Process')}}}
}catch{$timingRows.Add(@{name='timing control/setup';passed=$false;error=$_.Exception.ToString()})}
$timingFailed=@($timingRows|Where-Object passed -EQ $false).Count;$failed+=$timingFailed
$timingReport=@{kind='Actual production owned operation and custody controls plus explicit technical timeline; NOT native3';cases=$timingRows.Count;passed=@($timingRows|Where-Object passed -EQ $true).Count;failed=$timingFailed;rows=@($timingRows)}
$failed+=$inputOwnerFailed
$inputOwnerReport=[ordered]@{kind='Actual runner Input-Record AST with self-contained restore-owner fixtures and pinned official member bytes; NOT native3';cases=$inputOwnerRows.Count;passed=@($inputOwnerRows|Where-Object passed -EQ $true).Count;failed=$inputOwnerFailed;rows=@($inputOwnerRows)}
foreach($row in $inputOwnerRows){Write-Output "$($row.name): passed=$($row.passed)"}
$runnerFailed=@($runnerRows | Where-Object passed -EQ $false).Count
$ownedFailed=@($ownedRows | Where-Object passed -EQ $false).Count
$failed+=$runnerFailed+$ownedFailed
$runnerReport=[ordered]@{kind='Actual runner AST Git/stream controls; NOT full Windows Job/download/native execution';runnerSha256=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot 'Invoke-NativeResourceVerification.ps1')).Hash.ToLowerInvariant();cases=$runnerRows.Count;passed=@($runnerRows | Where-Object passed -EQ $true).Count;failed=$runnerFailed;rows=@($runnerRows)}
$ownedReport=[ordered]@{kind='Actual production AST Windows Job/process controls; NOT native3/resource/inference execution';cases=$ownedRows.Count;passed=@($ownedRows | Where-Object passed -EQ $true).Count;failed=$ownedFailed;rows=@($ownedRows)}
foreach($row in $runnerRows){Write-Output "$($row.name): passed=$($row.passed)"}
foreach($row in $ownedRows){Write-Output "$($row.name): passed=$($row.passed)"}
$report=[ordered]@{schema=1;kind='Synthetic same-validator evidence regression plus actual runner AST and Windows owned-process controls; NOT native3 execution';cases=$cases.Count;passed=@($rows | Where-Object passed -EQ $true).Count;failed=$failed;restoredControl=$failed -eq 0;rows=$rows;positiveRows=$positiveRows;runnerRegression=$runnerReport;ownedProcessRegression=$ownedReport;inputOwnerRegression=$inputOwnerReport;timingRegression=$timingReport;full600GuardCounterfactual=$full600GuardCounterfactual;
    provisionGraphRegression=@{completed=$provisionGraph.completed;actualCounts=$provisionGraph.counts;fixtureCounts=$opCounts;validatorPositive=$script:graphBaselineResult;runnerSha256=$provisionGraph.runnerSha256;resourceEffects=0;scope=$provisionGraph.scope}}
[IO.File]::WriteAllText((Join-Path $OutputDirectory 'result.json'),(($report | ConvertTo-Json -Depth 30)+"`n"),$utf8)
Write-Output "cases=$($report.cases) passed=$($report.passed) failed=$failed; native resource Facts=0"
if($failed){exit 1}
