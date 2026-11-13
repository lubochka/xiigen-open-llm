# Copyright (C) 2026 Luba Karpenko, Genie Soft LTD; AGPL-3.0-only.
# Deterministic CI evidence helper. This is not a semantic/model authority.
[CmdletBinding()]
param([Alias('EvidenceDirectory')][string]$ValidationDirectory)

function Get-NativeTimingPolicy {
    # A build/list/capture sequence needs600+600+300, three110s closures,
    # checkpoint70 and finite controller handoff slack20. Maxima are not a forecast.
    # These are finite technical envelopes, not financial authorization or retry grants.
    [ordered]@{id='native-phases-v4-group';regularSeconds=7400;commandSeconds=600;closureSeconds=110;phaseCheckpointSeconds=70;controlBytes=16777216;jobSeconds=10980;
        nativeGroup=[ordered]@{factCount=3;allocationBaseSeconds=600;commandSeconds=1800;controllerSlackSeconds=20};
        buildSequence=[ordered]@{buildSeconds=600;discoverySeconds=600;captureSeconds=300;commandClosures=3;controllerSlackSeconds=20};
        phases=[ordered]@{prepare=900;default=1920;provision=1500;optin=1920;preflight=360;native=2000;restoration=1920;closure=600;validation=420}}
}
function Get-NativeUtc($value) {
    if($value -is [DateTimeOffset]){return $value.ToUniversalTime()}
    if($value -is [DateTime]){return [DateTimeOffset]::new($value.ToUniversalTime())}
    if([string]::IsNullOrWhiteSpace([string]$value)){throw 'Missing UTC timestamp'}
    [DateTimeOffset]::Parse([string]$value,[Globalization.CultureInfo]::InvariantCulture,[Globalization.DateTimeStyles]::RoundtripKind).ToUniversalTime()
}

function Get-NativeResourceIdentities {
    $prefix = 'LlmFromScratch.Tests.CodeDocumentationConfiguredQwenNativeResourceTests.'
    @('NativeInstalledVocabulary_ExactVectorsAndAbi', 'HeldCompleteContext_NativePrepareAndOverflow', 'NativeResourceDriftAndCancellation') | ForEach-Object { $prefix + $_ }
}

function Test-NativeTimingEvidence($e,[string]$directory) {
    $errors=[Collections.Generic.List[string]]::new()
    function Stamp($value){if($null -eq $value -or [string]::IsNullOrWhiteSpace([string]$value)){throw 'Missing timestamp'};if($value -is [DateTimeOffset]){return $value.ToUniversalTime()};if($value -is [DateTime]){return [DateTimeOffset]::new($value.ToUniversalTime())};[DateTimeOffset]::Parse([string]$value,[Globalization.CultureInfo]::InvariantCulture)}
    try {
        $policy=Get-NativeTimingPolicy;$t=$e.timing
        if(($t.policy|ConvertTo-Json -Depth 10 -Compress) -cne ($policy|ConvertTo-Json -Depth 10 -Compress)){throw 'Missing/stale timing policy'}
        $start=Stamp $t.startUtc;$regular=Stamp $t.regularDeadlineUtc;$restore=Stamp $t.restorationDeadlineUtc
        $close=Stamp $t.closureDeadlineUtc;$overall=Stamp $t.overallDeadlineUtc;$end=Stamp $t.endUtc
        if(($regular-$start).TotalSeconds -ne $policy.regularSeconds -or ($restore-$regular).TotalSeconds -ne 1920 -or
            ($close-$restore).TotalSeconds -ne 600 -or ($overall-$close).TotalSeconds -ne 420 -or
            $end -lt $start -or $end -gt $overall -or $t.unknownEffects -ne $false -or @($t.admissions).Count){throw 'Invalid overall/reserve/admission/unknown closure'}
        $previous=$start;$phaseMap=@{}
        foreach($name in @('prepare','default','provision','optin','preflight','native','restoration','closure')) {
            $rows=@($t.phases|Where-Object name -CEQ $name)
            if($rows.Count -ne 1){throw "Missing/duplicate timing phase $name"};$row=$rows[0]
            $begin=Stamp $row.startUtc;$finish=Stamp $row.endUtc;$deadline=Stamp $row.deadlineUtc
            $limit=if($name -ceq 'restoration'){$restore}elseif($name -ceq 'closure'){$close}else{$regular}
            if($row.completed -ne $true -or $row.error -or $begin -lt $previous -or $finish -lt $begin -or $finish -gt $deadline -or
                $deadline -le $begin -or ($deadline-$begin).TotalSeconds -gt $policy.phases[$name] -or $deadline -gt $limit){throw "Overdue/incomplete phase $name"}
            $phaseMap[$name]=$row;$previous=$finish
        }
        if($end -lt $previous){throw 'Closure end predates last phase'}
        foreach($name in $phaseMap.Keys){if(@($t.metadataWrites|Where-Object {$_.phase -ceq $name -and $_.path -ceq 'timing.json'}).Count -lt 1){throw "Missing phase evidence writer $name"}}
        foreach($writer in $t.metadataWrites){
            $phase=$phaseMap[$writer.phase];$begin=Stamp $writer.startUtc;$finish=Stamp $writer.endUtc;$deadline=Stamp $writer.deadlineUtc;$stop=Stamp $writer.closureDeadlineUtc
            if(-not $phase -or $begin -lt (Stamp $phase.startUtc) -or $finish -gt (Stamp $phase.deadlineUtc) -or $finish -gt $stop -or
                $deadline -le $begin -or ($deadline-$begin).TotalSeconds -gt 30.1 -or ($stop-$deadline).TotalSeconds -ne 40 -or
                $writer.pid -le 0 -or $writer.exitCode -ne 0 -or $writer.timedOut -ne $false -or $writer.assignedBeforeResume -ne $true -or
                $writer.activeAfterCleanup -ne 0 -or $writer.totalProcesses -lt 1 -or $writer.error -or
                (Stamp $writer.birthUtc) -lt $begin.AddSeconds(-2) -or (Stamp $writer.processEndUtc) -gt $deadline.AddMilliseconds(250)){throw 'Missing/overdue/nonterminal evidence writer'}
        }
        foreach($command in $e.commands){
            $phase=$phaseMap[$command.phase];if(-not $phase){throw 'Command has no admitted phase'}
            $begin=Stamp $command.startUtc;$finish=Stamp $command.endUtc;$deadline=Stamp $command.deadlineUtc;$closureDeadline=Stamp $command.closureDeadlineUtc
            if($begin -lt (Stamp $phase.startUtc) -or $finish -gt (Stamp $phase.deadlineUtc) -or $deadline -le $begin -or
                ($deadline-$begin).TotalSeconds -gt $(if($command.name -ceq 'native3'){$policy.nativeGroup.commandSeconds}else{$policy.commandSeconds}) -or ($closureDeadline-$deadline).TotalSeconds -ne $policy.closureSeconds -or
                $closureDeadline -gt (Stamp $phase.deadlineUtc).AddSeconds(-$policy.phaseCheckpointSeconds)) {throw 'Command spills phase/closure reserve'}
            if($command.name -ceq 'native3' -and ($command.phase -cne 'native' -or ($deadline-$begin).TotalSeconds -ne $policy.nativeGroup.commandSeconds)){throw 'Native requires exact1800 admission in native phase'}
            if($command.name -cmatch '^(default-before|opt-in|default-restoration)-(build|list)$' -and [Math]::Abs(($deadline-$begin).TotalSeconds-600) -gt 0.01){throw 'Build/discovery requires full600 admission'}
        }
        $required=@{'Capture-Sources'=1;'Capture-Build'=3;'Fixture-Inventory'=2;'Resource-Hashes'=2;'Capture-SourceAfter'=1;'Cleanup-Owned'=1;
            'Invoke-DownloadStream'=5;'Expand-OwnedMember'=7;'Write-OwnedManifest'=1;'Seal-OwnedLeaf'=13;'Promote-OwnedLeaf'=13}
        foreach($key in $required.Keys){if(@($t.operations|Where-Object operation -CEQ $key).Count -ne $required[$key]){throw "Missing/duplicate owned operation $key"}}
        foreach($operation in $t.operations){
            $commands=@($e.commands|Where-Object name -CEQ $operation.name)
            if($commands.Count -ne 1 -or $operation.completed -ne $true -or $operation.error){throw 'Operation lacks terminal command'}
            $command=$commands[0];$begin=Stamp $operation.startUtc;$finish=Stamp $operation.endUtc;$deadline=Stamp $operation.deadlineUtc
            if($begin -gt (Stamp $command.startUtc) -or $finish -lt (Stamp $command.endUtc) -or $finish -gt (Stamp $command.closureDeadlineUtc) -or
                $deadline -ne (Stamp $command.deadlineUtc) -or $command.phase -cne $operation.phase){throw 'Operation duration/command mismatch'}
            $argv=@($command.argv)
            if([IO.Path]::GetFileNameWithoutExtension([string]$command.executable) -cne 'pwsh' -or
                @($argv|Where-Object {[IO.Path]::GetFileName($_) -ceq 'Invoke-NativeEvidenceOperation.ps1'}).Count -ne 1){throw 'Operation not bound to executed helper'}
            $helper=@($e.sources|Where-Object path -CEQ 'tools/S04NativeResourceVerification/Invoke-NativeEvidenceOperation.ps1')
            if($helper.Count -ne 1 -or $operation.helperSha256 -cne $helper[0].candidateSha256 -or
                $operation.requestSha256 -cnotmatch '^[0-9a-f]{64}$' -or $operation.requestIdentity -cnotmatch '^[0-9a-f]{8}:[0-9a-f]{16}$'){throw 'Operation lacks candidate/helper/request binding'}
            $record=$operation.receipt;$expected=$operation.name+'.receipt.json'
            if($record.path -cne $expected -or $record.length -le 0 -or $record.length -gt 65536 -or $record.sha256 -cnotmatch '^[0-9a-f]{64}$'){throw 'Operation compact receipt missing/oversized'}
            $path=Join-Path $directory $expected;$item=Get-Item -LiteralPath $path -Force
            if($item.PSIsContainer -or ($item.Attributes -band [IO.FileAttributes]::ReparsePoint) -or $item.Length -ne $record.length -or
                (Get-FileHash -LiteralPath $path).Hash.ToLowerInvariant() -cne $record.sha256){throw 'Operation compact receipt changed'}
            $receipt=Get-Content -LiteralPath $path -Raw|ConvertFrom-Json -AsHashtable -Depth 10
            if($receipt.operation -cne $operation.operation -or $receipt.candidate -cne $e.candidate -or $receipt.helperSha256 -cne $operation.helperSha256 -or
                $receipt.requestSha256 -cne $operation.requestSha256 -or $receipt.requestIdentity -cne $operation.requestIdentity -or $receipt.error -or
                (Stamp $receipt.deadlineUtc) -ne $deadline -or (Stamp $receipt.endUtc) -gt $deadline -or (Stamp $receipt.endUtc) -lt $begin){throw 'Stale/foreign/overdue compact receipt'}
        }
        foreach($download in $e.downloads){
            $begin=Stamp $download.startUtc;$streamEnd=Stamp $download.streamEndUtc;$transferEnd=Stamp $download.transferEndUtc;$finish=Stamp $download.endUtc
            $stream=@($e.commands|Where-Object name -CEQ $download.streamCommand);$seal=@($e.commands|Where-Object name -CEQ $download.sealCommand);$promotion=@($e.commands|Where-Object name -CEQ $download.promotionCommand)
            if($stream.Count -ne 1 -or $seal.Count -ne 1 -or $promotion.Count -ne 1){throw 'Download lacks transfer/seal/promotion commands'}
            if($begin -lt (Stamp $stream[0].startUtc) -or $streamEnd -lt $begin -or $transferEnd -lt $streamEnd -or $transferEnd -gt (Stamp $stream[0].processEndUtc) -or
                (Stamp $seal[0].startUtc) -lt (Stamp $stream[0].endUtc) -or (Stamp $promotion[0].startUtc) -lt (Stamp $seal[0].endUtc) -or
                $finish -lt (Stamp $promotion[0].endUtc) -or $finish -gt (Stamp $promotion[0].closureDeadlineUtc) -or
                ($streamEnd-$begin).TotalSeconds -gt 600 -or ($finish-$begin).TotalSeconds -gt 1140){throw 'Stale/fabricated/full-seal download interval'}
        }
    }catch{$errors.Add($_.Exception.Message)}
    @($errors)
}

function Get-NativeTestSdkInput {
    # One restore-owned compiler input, not permission for arbitrary NuGet source.
    param([string]$Path,[string]$ProjectFile)
    $path=[IO.Path]::GetFullPath($Path);$project=[IO.Path]::GetFullPath($ProjectFile)
    $id='Microsoft.NET.Test.Sdk';$version='17.12.0';$key="$id/$version"
    $member='build/netcoreapp3.1/Microsoft.NET.Test.Sdk.Program.cs'
    $target='build/netcoreapp3.1/Microsoft.NET.Test.Sdk.targets'
    # Exact bytes read from the official NuGet v3 flat-container 17.12.0 archive.
    $memberPins=@{$member='d66f12f9433816a8c7dc1b2acb9e1010f5418f5eeb51e80f1df57aa1dcab8d8a';$target='2bc2e0e7d2d0b7e0223abd25ddfe38ad85866b34b03a99679dfcdd77bbbc4ccb'}
    $assetsPath=Join-Path ([IO.Path]::GetDirectoryName($project)) 'obj/project.assets.json'
    $importsPath=Join-Path ([IO.Path]::GetDirectoryName($assetsPath)) ([IO.Path]::GetFileName($project)+'.nuget.g.targets')
    foreach($leaf in @($path,$project,$assetsPath,$importsPath)) {
        for($cursor=$leaf;$cursor;$cursor=[IO.Path]::GetDirectoryName($cursor)) {
            if((Test-Path -LiteralPath $cursor) -and ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Reparse NuGet owner path'}
        }
        if(-not (Test-Path -LiteralPath $leaf -PathType Leaf)){throw 'Missing NuGet owner file'}
    }
    [xml]$projectXml=[IO.File]::ReadAllText($project)
    $refs=@($projectXml.SelectNodes("//*[local-name()='PackageReference' and @Include='$id' and @Version='$version']"))
    if($refs.Count -ne 1){throw 'No exact direct Test.Sdk project owner'}
    $assets=Get-Content -LiteralPath $assetsPath -Raw | ConvertFrom-Json -AsHashtable -Depth 100
    if([IO.Path]::GetFullPath($assets.project.restore.projectPath) -cne $project){throw 'Foreign restore project owner'}
    $library=$assets.libraries[$key]
    if(-not $library -or $library.type -cne 'package' -or $library.path -cne 'microsoft.net.test.sdk/17.12.0' -or
        $member -cnotin $library.files -or $target -cnotin $library.files){throw 'Wrong Test.Sdk restore library owner'}
    $frameworks=@($assets.targets.Keys | Where-Object {$assets.targets[$_].Contains($key) -and $assets.targets[$_][$key].build.Contains($target)})
    if($frameworks.Count -eq 0){throw 'No Test.Sdk restore build target owner'}
    $roots=@($assets.packageFolders.Keys | Where-Object {[IO.Path]::GetFullPath((Join-Path $_ ($library.path+'/'+$member))) -ceq $path})
    if($roots.Count -ne 1){throw 'Foreign NuGet compiler input path'}
    $packageRoot=Join-Path $roots[0] $library.path
    $archive=Join-Path $packageRoot 'microsoft.net.test.sdk.17.12.0.nupkg'
    for($cursor=$archive;$cursor;$cursor=[IO.Path]::GetDirectoryName($cursor)) {
        if((Test-Path -LiteralPath $cursor) -and ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)){throw 'Reparse NuGet archive path'}
    }
    $archiveBytes=[IO.File]::ReadAllBytes($archive)
    $sha512=[Convert]::ToBase64String([Security.Cryptography.SHA512]::HashData($archiveBytes))
    # NuGet assets use the package content hash (before repository signing), while
    # the sidecar hashes the signed ZIP. Authenticate consumed bytes with the
    # immutable official-member pins below; mutable metadata is not an origin pin.
    if($library.sha512 -cne 'kt/PKBZ91rFCWxVIJZSgVLk+YR+4KxTuHf799ho8WNiK5ZQpJNAEZCAWX86vcKrs+DiYjiibpYKdGZP6+/N17w==' -or
        [IO.File]::ReadAllText($archive+'.sha512').Trim() -cne $sha512){throw 'NuGet archive restore hash mismatch'}
    [xml]$imports=[IO.File]::ReadAllText($importsPath)
    $expectedImport='$(NuGetPackageRoot)microsoft.net.test.sdk\17.12.0\build\netcoreapp3.1\Microsoft.NET.Test.Sdk.targets'
    if(@($imports.SelectNodes("//*[local-name()='Import']") | Where-Object {$_.Project -ceq $expectedImport -or $_.Project -ceq (Join-Path $packageRoot $target)}).Count -ne 1){throw 'No exact Test.Sdk generated import'}
    $zip=[IO.Compression.ZipFile]::OpenRead($archive)
    try {
        $content=@{}
        foreach($name in @($member,$target,'Microsoft.NET.Test.Sdk.nuspec')) {
            $entries=@($zip.Entries | Where-Object FullName -CEQ $name)
            if($entries.Count -ne 1){throw 'Missing/duplicate NuGet owner archive member'}
            $stream=$entries[0].Open();$memory=[IO.MemoryStream]::new()
            try{$stream.CopyTo($memory);$content[$name]=$memory.ToArray()}finally{$stream.Dispose();$memory.Dispose()}
        }
        [xml]$nuspec=([Text.Encoding]::UTF8.GetString($content['Microsoft.NET.Test.Sdk.nuspec'])).TrimStart([char]0xfeff)
        if($nuspec.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='id']").InnerText -cne $id -or
            $nuspec.SelectSingleNode("//*[local-name()='metadata']/*[local-name()='version']").InnerText -cne $version){throw 'Foreign NuGet archive identity'}
        foreach($name in @($member,$target)) {
            $disk=Join-Path $packageRoot $name
            $expected=[Convert]::ToHexString([Security.Cryptography.SHA256]::HashData($content[$name])).ToLowerInvariant()
            if($expected -cne $memberPins[$name]){throw 'NuGet compiler/target immutable content pin mismatch'}
            if((Get-FileHash -LiteralPath $disk -Algorithm SHA256).Hash.ToLowerInvariant() -cne $expected -or (Get-Item -LiteralPath $disk).Length -ne $content[$name].Length){throw 'NuGet compiler/target archive bytes differ'}
        }
        [xml]$targetXml=[Text.Encoding]::UTF8.GetString($content[$target])
        $generated=$targetXml.SelectSingleNode("//*[local-name()='GeneratedProgramFile']")
        if(-not $generated -or $generated.InnerText -cne '$(MSBuildThisFileDirectory)Microsoft.NET.Test.Sdk.Program$(DefaultLanguageSourceExtension)' -or
            @($targetXml.SelectNodes('//*[local-name()="Target" and @Name="GenerateProgramFile"]//*[local-name()="Compile" and @Include="$(GeneratedProgramFile)"]')).Count -ne 1){throw 'Test.Sdk source inclusion contract differs'}
    }finally{$zip.Dispose()}
    $hash=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
    [ordered]@{path=$path;kind='nuget-package';length=(Get-Item -LiteralPath $path).Length;sha256=$hash;
        owner=[ordered]@{projectFile=$project;assetsPath=$assetsPath;assetsSha256=(Get-FileHash -LiteralPath $assetsPath).Hash.ToLowerInvariant();importsSha256=(Get-FileHash -LiteralPath $importsPath).Hash.ToLowerInvariant();packageId=$id;packageVersion=$version;member=$member;archivePath=$archive;archiveSha512=$sha512;memberSha256=$hash}}
}

function Read-NativeManagedPdbBinding {
    # Read PE/PDB metadata without loading/executing the inspected assembly.
    param([string]$Dll, [string]$Pdb, [scriptblock]$InputResolver)
    $dllStream=[IO.File]::OpenRead($Dll);$pdbStream=[IO.File]::OpenRead($Pdb);$provider=$null;$pe=$null
    try {
        $pe=[Reflection.PortableExecutable.PEReader]::new($dllStream)
        $provider=[Reflection.Metadata.MetadataReaderProvider]::FromPortablePdbStream($pdbStream)
        $reader=$provider.GetMetadataReader();$header=[byte[]]@($reader.DebugMetadataHeader.Id)
        $pdbId=[Guid]::new([byte[]]$header[0..15]).ToString('N')
        $codeView=@($pe.ReadDebugDirectory() | Where-Object {$_.Type -eq [Reflection.PortableExecutable.DebugDirectoryEntryType]::CodeView})
        if($codeView.Count -ne 1){throw 'No unique PE CodeView'}
        $dllId=$pe.ReadCodeViewDebugDirectoryData($codeView[0]).Guid.ToString('N')
        if($dllId -cne $pdbId -or $codeView[0].Stamp -ne [BitConverter]::ToUInt32($header,16)){throw 'DLL/PDB ID/stamp mismatch'}
        $checks=@($pe.ReadDebugDirectory() | Where-Object {$_.Type -eq [Reflection.PortableExecutable.DebugDirectoryEntryType]::PdbChecksum})
        if($checks.Count -ne 1){throw 'Missing PE PDB checksum'}
        $checksum=$pe.ReadPdbChecksumDebugDirectoryData($checks[0]);$bytes=[IO.File]::ReadAllBytes($Pdb)
        # The deterministic content checksum predates the portable PDB's patched 20-byte ID.
        $idOffset=[int]$reader.DebugMetadataHeader.IdStartOffset
        if($header.Length -ne 20 -or $idOffset -lt 0 -or $idOffset -gt $bytes.Length-20){throw 'Portable PDB ID bounds invalid'}
        if($checksum.AlgorithmName -cnotin @('SHA256','SHA1')){throw 'Unsupported PE PDB checksum algorithm'}
        [Array]::Clear($bytes,$idOffset,20)
        $algorithm=[Security.Cryptography.HashAlgorithm]::Create($checksum.AlgorithmName)
        if(-not $algorithm){throw 'Unknown PE PDB checksum algorithm'}
        try{$actualPdb=[Convert]::ToHexString($algorithm.ComputeHash($bytes)).ToLowerInvariant()}finally{$algorithm.Dispose()}
        $expectedPdb=[Convert]::ToHexString([byte[]]@($checksum.Checksum)).ToLowerInvariant()
        if($actualPdb -cne $expectedPdb){throw 'PE PDB content checksum mismatch'}
        $documents=@(foreach($handle in $reader.Documents){
            $doc=$reader.GetDocument($handle);$name=$reader.GetString($doc.Name)
            $input=& $InputResolver $name;$hashAlgorithm=$reader.GetGuid($doc.HashAlgorithm).ToString()
            $algorithmName=if($hashAlgorithm -ceq '8829d00f-11b8-4213-878b-770e8597ac16'){'SHA256'}elseif($hashAlgorithm -ceq 'ff1816ec-aa5e-4d10-87f7-6f4963833460'){'SHA1'}else{throw 'Unknown PDB document hash algorithm'}
            $expected=[Convert]::ToHexString($reader.GetBlobBytes($doc.Hash)).ToLowerInvariant()
            $actual=(Get-FileHash -LiteralPath $name -Algorithm $algorithmName).Hash.ToLowerInvariant()
            if($expected -cne $actual){throw "PDB source checksum mismatch: $name"}
            $record=[ordered]@{path=$input.path;kind=$input.kind;algorithm=$algorithmName;checksum=$expected;actualChecksum=$actual;sha256=$input.sha256}
            if($input.kind -ceq 'nuget-package'){$record.owner=$input.owner}
            $record
        })
        [ordered]@{dllCodeViewId=$dllId;pdbId=$pdbId;pdbChecksum=$expectedPdb;actualPdbChecksum=$actualPdb;documents=$documents}
    } finally {if($provider){$provider.Dispose()};if($pe){$pe.Dispose()};$pdbStream.Dispose();$dllStream.Dispose()}
}

function Test-NativeResourceEvidence {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string]$Directory, [object]$Evidence)
    $failures = [Collections.Generic.List[object]]::new()
    function Reject([string]$code, [string]$detail) { $failures.Add([ordered]@{ code = $code; detail = $detail }) }
    function ResolveTestSdkInput([string]$path) {
        $relative='Tests/LlmFromScratch.Tests.csproj';$project=Join-Path $e.workspace $relative
        $owner=@($source | Where-Object path -CEQ $relative)
        if($owner.Count -ne 1 -or (Get-FileHash -LiteralPath $project).Hash.ToLowerInvariant() -cne $owner[0].candidateSha256 -or
            (Get-Item -LiteralPath $project).Length -ne $owner[0].candidateLength){throw 'No unchanged candidate Test.Sdk project owner'}
        Get-NativeTestSdkInput $path $project
    }
    function SameSet($left, $right) {
        $set = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        $set.UnionWith([string[]]@($left))
        @($left).Count -eq @($right).Count -and $set.Count -eq @($left).Count -and $set.SetEquals([string[]]@($right))
    }
    function Utc($value) {
        # ConvertFrom-Json on newer PowerShell materializes ISO timestamps as DateTime.
        if ($value -is [DateTimeOffset]) { return $value.ToUniversalTime() }
        if ($value -is [DateTime]) { return [DateTimeOffset]::new($value.ToUniversalTime()) }
        $time = [DateTimeOffset]::MinValue
        if (-not [DateTimeOffset]::TryParse([string]$value, [Globalization.CultureInfo]::InvariantCulture, [Globalization.DateTimeStyles]::RoundtripKind, [ref]$time)) { throw "Invalid UTC: $value" }
        $time.ToUniversalTime()
    }
    $root = [IO.Path]::GetFullPath($Directory).TrimEnd([IO.Path]::DirectorySeparatorChar) + [IO.Path]::DirectorySeparatorChar
    function EvidencePath([string]$relative) {
        if ([string]::IsNullOrWhiteSpace($relative) -or [IO.Path]::IsPathRooted($relative)) { throw 'Evidence path must be relative.' }
        $path = [IO.Path]::GetFullPath((Join-Path $root $relative))
        if (-not $path.StartsWith($root, [StringComparison]::OrdinalIgnoreCase)) { throw 'Evidence path escapes root.' }
        $cursor = $path
        while ($cursor.Length -ge $root.TrimEnd([IO.Path]::DirectorySeparatorChar).Length) {
            if ((Test-Path -LiteralPath $cursor) -and ((Get-Item -LiteralPath $cursor -Force).Attributes -band [IO.FileAttributes]::ReparsePoint)) { throw 'Reparse evidence path.' }
            $cursor = [IO.Path]::GetDirectoryName($cursor)
            if (-not $cursor) { break }
        }
        $path
    }
    function CheckFile($record, [string]$code) {
        try {
            $path = EvidencePath $record.path
            $item = Get-Item -LiteralPath $path -Force -ErrorAction Stop
            if ($item.PSIsContainer -or [long]$record.length -le 0 -or $record.sha256 -cnotmatch '^[0-9a-f]{64}$' -or
                $item.Length -ne [long]$record.length -or (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -cne $record.sha256) { throw 'Content/hash/length mismatch.' }
        } catch { Reject $code $_.Exception.Message }
    }
    try {
        if ($null -eq $Evidence) { $Evidence = Get-Content -LiteralPath (EvidencePath 'evidence.json') -Raw -ErrorAction Stop | ConvertFrom-Json -Depth 100 -ErrorAction Stop }
        $e = $Evidence
        foreach($error in @(Test-NativeTimingEvidence $e $Directory)){Reject 'TIMING' $error}
        if ($e.schema -ne 1 -or $e.gate -cne 'S04.04A-NATIVE-RESOURCE') { Reject 'SCHEMA' 'Wrong evidence schema or gate.' }
        if ($e.candidate -cnotmatch '^[0-9a-f]{40}$' -or $e.head -cne $e.candidate -or $e.workflow -cne $e.candidate) { Reject 'SOURCE_IDENTITY' 'C, W and checkout HEAD must be the same exact SHA.' }
        foreach ($name in @('runId', 'runAttempt', 'workflowRef', 'runner', 'sdk', 'workspace')) {
            if ([string]::IsNullOrWhiteSpace([string]$e.$name)) { Reject 'SOURCE_IDENTITY' "Missing $name." }
        }
        $source = @($e.sources)
        $requiredSource = @('.github/workflows/s03-windows-verification.yml', 'tools/S04NativeResourceVerification/Invoke-NativeResourceVerification.ps1',
            'tools/S04NativeResourceVerification/Test-NativeResourceEvidence.ps1', 'tools/S04NativeResourceVerification/Test-NativeResourceEvidence.Tests.ps1',
            'tools/S04NativeResourceVerification/Invoke-NativeEvidenceOperation.ps1',
            'tools/S04NativeResourceVerification/public-resource-pins.json', 'Tests/CodeDocumentationConfiguredQwenNativeResourceTests.cs',
            'Tests/LlmFromScratch.Tests.csproj', '14.ContinuousLearning/Training/ConfiguredQwenVocabularyLease.cs')
        foreach ($p in $requiredSource) { if (@($source | Where-Object path -CEQ $p).Count -ne 1) { Reject 'SOURCE_IDENTITY' "Missing/duplicate source $p." } }
        foreach ($s in $source) {
            if ($s.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $s.sha256 -cne $s.candidateSha256 -or $s.sha256 -cne $s.afterSha256 -or
                [long]$s.length -le 0 -or [long]$s.length -ne [long]$s.candidateLength -or [long]$s.length -ne [long]$s.afterLength) { Reject 'SOURCE_IDENTITY' "Dirty/stale source: $($s.path)." }
            if ($s.path -ceq '.github/workflows/s03-windows-verification.yml' -and ($s.workflowSha256 -cne $s.sha256 -or [long]$s.workflowLength -ne [long]$s.length)) { Reject 'SOURCE_IDENTITY' 'Executing workflow blob differs.' }
        }
        if ($e.regression.exitCode -ne 0 -or $e.regression.passed -lt 20) { Reject 'VALIDATOR_REGRESSION' 'Same-validator regression absent/failed.' }
        $commands = @($e.commands)
        foreach ($name in @('restore', 'default-before-build', 'default-before-list', 'opt-in-build', 'opt-in-list', 'native3', 'default-restoration-build', 'default-restoration-list')) {
            if (@($commands | Where-Object name -CEQ $name).Count -ne 1) { Reject 'COMMAND' "Missing/duplicate command $name." }
        }
        foreach ($c in $commands) {
            try {
                $start = Utc $c.startUtc; $end = Utc $c.endUtc; $birth = Utc $c.birthUtc; $deadline = Utc $c.deadlineUtc;$processEnd=Utc $c.processEndUtc;$closureDeadline=Utc $c.closureDeadlineUtc
                if ($c.exitCode -isnot [int] -and $c.exitCode -isnot [long]) { throw 'No numeric exit code.' }
                if ($c.exitCode -ne 0 -or $c.timedOut -ne $false -or $c.pid -le 0 -or $birth -gt $end -or $birth -lt $start.AddSeconds(-2) -or
                    $end -lt $start -or $end -gt $closureDeadline -or $processEnd -lt $birth -or $processEnd -gt $deadline.AddMilliseconds(250) -or
                    ($deadline - $start).TotalSeconds -gt $(if($c.name -ceq 'native3'){(Get-NativeTimingPolicy).nativeGroup.commandSeconds}else{(Get-NativeTimingPolicy).commandSeconds}) -or
                    $c.assignedBeforeResume -ne $true -or $c.activeAfterCleanup -ne 0 -or $c.totalProcesses -lt 1) { throw 'Failed/nonterminal/uncontained command or time mismatch.' }
                foreach ($stream in @('stdout', 'stderr')) {
                    $path = EvidencePath $c.$stream
                    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) { throw "Missing $stream." }
                    $text = [IO.File]::ReadAllText($path)
                    if ($text -match '(?im)0x800711c7|testhost process exited with error|failed to load.*(?:dll|library)|no test is available|test run aborted|CONFIGURED_QWEN_(?:RESOURCE_UNAVAILABLE|NATIVE_ABI_UNSUPPORTED)') { Reject 'LOADER' "$($c.name): explicit loader/discovery failure." }
                }
            } catch { Reject 'COMMAND' "$($c.name): $($_.Exception.Message)" }
        }
        $native = @($commands | Where-Object name -CEQ 'native3')
        $identities = @(Get-NativeResourceIdentities)
        $filter = ($identities | ForEach-Object { 'FullyQualifiedName=' + $_ }) -join '|'
        if ($native.Count -eq 1) {
            $argv = @($native[0].argv)
            $index = [Array]::IndexOf([string[]]$argv, '--filter')
            if ($argv.Count -lt 2 -or $argv[0] -cne 'test' -or $argv[1] -cne 'Tests/LlmFromScratch.Tests.csproj' -or
                [IO.Path]::GetFileNameWithoutExtension([string]$native[0].executable) -cne 'dotnet' -or
                @($argv | Where-Object { $_ -ceq '--no-restore' }).Count -ne 1 -or
                [Array]::IndexOf([string[]]$argv,'--configuration') -lt 0 -or $argv[[Array]::IndexOf([string[]]$argv,'--configuration')+1] -cne 'Release' -or
                $index -lt 0 -or $index + 1 -ge $argv.Count -or $argv[$index + 1] -cne $filter -or
                @($argv | Where-Object { $_ -ceq '--no-build' }).Count -ne 1 -or
                @($argv | Where-Object { $_ -ceq '-p:XiigenEnableConfiguredQwenNativeResourceTests=true' }).Count -ne 1) { Reject 'COMMAND_FILTER' 'Native command must be the exact three identities with opt-in/no-build.' }
        }
        $phases = @('default-before', 'opt-in', 'default-restoration')
        $baseline = @()
        foreach ($phase in $phases) {
            $d = @($e.discovery | Where-Object phase -CEQ $phase)
            if ($d.Count -ne 1) { Reject 'DISCOVERY' "Missing/duplicate $phase."; continue }
            $names = @($d[0].identities); $actual = @($names | Where-Object { $_.StartsWith('LlmFromScratch.Tests.CodeDocumentationConfiguredQwenNativeResourceTests.', [StringComparison]::Ordinal) })
            try {
                $raw = [IO.File]::ReadAllText((EvidencePath "$phase-list.stdout.log"))
                $rawNames = @($raw -split '\r?\n' | ForEach-Object { $_.Trim() } | Where-Object { $_ -cmatch '^LlmFromScratch\.Tests\.' })
                $rawSet = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal); $rawSet.UnionWith([string[]]$rawNames)
                if (-not (SameSet $names @($rawSet)) -or $d[0].rawCount -ne $rawNames.Count -or
                    ($phase -ceq 'opt-in' -and @($rawNames | Where-Object { $_ -clike 'LlmFromScratch.Tests.CodeDocumentationConfiguredQwenNativeResourceTests.*' }).Count -ne 3)) { throw 'Recorded discovery differs from raw ordinal set or exact native rows.' }
            } catch { Reject 'DISCOVERY' $_.Exception.Message }
            if ($names.Count -eq 0 -or -not (SameSet $names $names)) { Reject 'DISCOVERY' 'Empty/duplicate discovery.' }
            if ($phase -ceq 'opt-in') { if (-not (SameSet $actual $identities)) { Reject 'DISCOVERY' 'Opt-in is not exact3.' } }
            elseif ($actual.Count -ne 0) { Reject 'RESTORATION' 'Default discovery includes native Facts.' }
            $portable = @($names | Where-Object { $_ -cnotlike 'LlmFromScratch.Tests.CodeDocumentationConfiguredQwenNativeResourceTests.*' })
            if ($phase -ceq 'default-before') { $baseline = $portable }
            elseif (-not (SameSet $baseline $portable)) { Reject 'DISCOVERY' 'Portable ordinal identity set changed.' }
        }
        if (-not (SameSet @($e.builds | ForEach-Object phase) $phases)) { Reject 'BINARY_BINDING' 'Wrong build phase inventory.' }
        $savedTestHashes = @{}
        foreach ($phase in $phases) {
            $b = @($e.builds | Where-Object phase -CEQ $phase)
            if ($b.Count -ne 1) { Reject 'BINARY_BINDING' "Missing/duplicate build $phase."; continue }
            $b = $b[0]
            CheckFile $b.binlog 'BINARY_BINDING'
            CheckFile $b.cscEvidence 'BINARY_BINDING'
            try {
                $events=@(Get-Content -LiteralPath (EvidencePath $b.cscEvidence.path) -Raw | ConvertFrom-Json -Depth 100)
                $testsCsc=@($events | Where-Object { $_.type -ceq 'TaskCommandLineEventArgs' -and
                    [string]::Equals([IO.Path]::GetFullPath([string]$_.projectFile),[IO.Path]::GetFullPath((Join-Path $e.workspace 'Tests/LlmFromScratch.Tests.csproj')),[StringComparison]::OrdinalIgnoreCase) })
                if($testsCsc.Count -ne 1 -or @($events | Where-Object {$_.type -ceq 'BuildFinishedEventArgs' -and $_.succeeded -eq $true}).Count -ne 1){throw 'Raw Csc replay lacks unique tests Csc/successful build event'}
                $nativeTokens=@([regex]::Matches([string]$testsCsc[0].commandLine,'"[^"]*"|\S+') | ForEach-Object {$_.Value.Trim('"')} | Where-Object {
                    [IO.Path]::GetFileName($_) -ceq 'CodeDocumentationConfiguredQwenNativeResourceTests.cs'})
                if($nativeTokens.Count -ne [int]($phase -ceq 'opt-in')){throw 'Raw Csc native include differs from phase'}
            }catch{Reject 'BINARY_BINDING' $_.Exception.Message}
            foreach ($a in @($b.artifacts)) { CheckFile $a 'BINARY_BINDING' }
            # Custody must cover each exact phase/owner DLL and PDB once, not just eight arbitrary files.
            $managedNames = @('LlmFromScratch.Tests','ModelServer','ContinuousLearning','TrainableRouter')
            $artifactPaths = @(foreach ($name in $managedNames) { foreach ($suffix in @('dll','pdb')) { "managed/$phase/$name.$suffix" } })
            if (-not (SameSet @($b.artifacts | ForEach-Object path) $artifactPaths)) {
                Reject 'BINARY_BINDING' "Missing/duplicate/foreign managed artifact inventory for $phase."
            } else {
                try {
                    $dllHash = (Get-FileHash -LiteralPath (EvidencePath "managed/$phase/LlmFromScratch.Tests.dll") -Algorithm SHA256 -ErrorAction Stop).Hash.ToLowerInvariant()
                    $pdbHash = (Get-FileHash -LiteralPath (EvidencePath "managed/$phase/LlmFromScratch.Tests.pdb") -Algorithm SHA256 -ErrorAction Stop).Hash.ToLowerInvariant()
                    $savedTestHashes[$phase] = @{ dll = $dllHash; pdb = $pdbHash }
                    if ($b.testsDllSha256 -cne $dllHash -or $b.testsPdbSha256 -cne $pdbHash) {
                        throw 'Build tests DLL/PDB hashes differ from saved managed bytes.'
                    }
                } catch { Reject 'BINARY_BINDING' $_.Exception.Message }
            }
            if (@($b.artifacts).Count -ne 8 -or $b.testsCscCount -ne 1 -or
                $b.nativeIncluded -ne [int]($phase -ceq 'opt-in') -or @($b.compileInputs).Count -eq 0 -or @($b.pdbBindings).Count -ne 4) { Reject 'BINARY_BINDING' 'Incomplete Csc/DLL/PDB source binding.' }
            foreach ($input in @($b.compileInputs)) {
                if($input.kind -cnotin @('tracked','generated','nuget-package')){Reject 'BINARY_BINDING' 'Unknown Csc source owner.'}
                if($input.kind -ceq 'nuget-package') {
                    try {
                        if([IO.Path]::GetFullPath($input.owner.projectFile) -cne (Join-Path $e.workspace 'Tests/LlmFromScratch.Tests.csproj')){throw 'Foreign compiler package project owner'}
                        $actual=ResolveTestSdkInput $input.path
                        if(($actual | ConvertTo-Json -Depth 20 -Compress) -cne ($input | ConvertTo-Json -Depth 20 -Compress)){throw 'Recorded compiler package owner differs from recomputation'}
                    }catch{Reject 'BINARY_BINDING' $_.Exception.Message}
                }
                if ($input.sha256 -cnotmatch '^[0-9a-f]{64}$' -or ($input.kind -ceq 'tracked' -and ($input.sha256 -cne $input.candidateSha256 -or $input.length -ne $input.candidateLength))) { Reject 'BINARY_BINDING' 'Csc input differs from candidate or has no hash.' }
            }
            $bindingNames = @($b.pdbBindings | ForEach-Object name)
            if (-not (SameSet $bindingNames @('LlmFromScratch.Tests','ModelServer','ContinuousLearning','TrainableRouter'))) { Reject 'BINARY_BINDING' 'Wrong managed PDB binding inventory.' }
            foreach ($p in @($b.pdbBindings)) {
                if ($p.dllCodeViewId -cne $p.pdbId -or $p.pdbId -notmatch '^[0-9a-f]{32}$' -or @($p.documents).Count -eq 0) { Reject 'BINARY_BINDING' 'Portable PDB does not bind to DLL.' }
                foreach ($doc in @($p.documents)) { if ($doc.checksum -cne $doc.actualChecksum -or $doc.kind -notin @('tracked','generated','nuget-package')) { Reject 'BINARY_BINDING' 'PDB document checksum/origin mismatch.' } }
                try {
                    $dll = EvidencePath "managed/$phase/$($p.name).dll"; $pdb = EvidencePath "managed/$phase/$($p.name).pdb"
                    $workspacePrefix = [IO.Path]::GetFullPath([string]$e.workspace).TrimEnd('\','/') + [IO.Path]::DirectorySeparatorChar
                    $actualBinding = Read-NativeManagedPdbBinding $dll $pdb {
                        param($name)
                        $path = [IO.Path]::GetFullPath($name)
                        if (-not $path.StartsWith($workspacePrefix,[StringComparison]::OrdinalIgnoreCase)) { return ResolveTestSdkInput $path }
                        $relative = [IO.Path]::GetRelativePath($e.workspace,$path).Replace('\','/')
                        $hash = (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant()
                        $sourceRow = @($source | Where-Object path -CEQ $relative)
                        if ($sourceRow.Count -eq 1) { if ($hash -cne $sourceRow[0].candidateSha256) { throw 'PDB tracked source differs from candidate.' }; $kind='tracked' }
                        elseif ($relative -match '(^|/)obj/') { $kind='generated' }
                        else { return ResolveTestSdkInput $path }
                        @{path=$relative;kind=$kind;sha256=$hash}
                    }
                    foreach ($field in @('dllCodeViewId','pdbId','pdbChecksum','actualPdbChecksum')) { if ($p.$field -cne $actualBinding.$field) { throw "Recorded PDB $field differs from saved binary metadata." } }
                    if (@($p.documents).Count -ne @($actualBinding.documents).Count) { throw 'Recorded PDB document count differs.' }
                    foreach ($doc in @($actualBinding.documents)) {
                        $recorded=@($p.documents | Where-Object path -CEQ $doc.path)
                        if ($recorded.Count -ne 1 -or $recorded[0].checksum -cne $doc.checksum -or $recorded[0].sha256 -cne $doc.sha256 -or $recorded[0].kind -cne $doc.kind) { throw 'Recorded PDB source binding differs from recomputation.' }
                        if($doc.kind -ceq 'nuget-package' -and ($recorded[0].owner | ConvertTo-Json -Depth 20 -Compress) -cne ($doc.owner | ConvertTo-Json -Depth 20 -Compress)){throw 'Recorded PDB package owner differs from recomputation'}
                    }
                } catch { Reject 'BINARY_BINDING' $_.Exception.Message }
            }
        }
        $opt = @($e.builds | Where-Object phase -CEQ 'opt-in')
        if ($opt.Count -eq 1 -and (-not $savedTestHashes.ContainsKey('opt-in') -or
            $e.commandBinarySha256 -cne $savedTestHashes['opt-in'].dll -or $e.commandPdbSha256 -cne $savedTestHashes['opt-in'].pdb -or
            $e.commandBinarySha256 -cne $opt[0].testsDllSha256 -or $e.commandPdbSha256 -cne $opt[0].testsPdbSha256)) { Reject 'STALE_BINARY' 'Command binary differs from saved opt-in DLL/PDB.' }
        $pins = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'public-resource-pins.json') -Raw | ConvertFrom-Json -Depth 30
        $expected = @($pins.members) + @($pins.blobs) + @($pins.manifest)
        foreach ($stage in @('pre','post')) {
            $rows = @($e.resources | Where-Object stage -CEQ $stage)
            if ($rows.Count -ne 12) { Reject 'RESOURCE_HASH' "$stage must contain twelve full hashes." }
            foreach ($pin in $expected) {
                $row = @($rows | Where-Object id -CEQ $pin.id)
                if ($row.Count -ne 1 -or $row[0].sha256 -cne $pin.sha256 -or [long]$row[0].length -ne [long]$pin.length -or $row[0].fullHash -ne $true) { Reject 'RESOURCE_HASH' "$stage/$($pin.id): wrong full hash/length." }
            }
        }
        if(@($e.downloads).Count -ne 5){Reject 'DOWNLOAD' 'Expected exact archive and four blob downloads.'}
        foreach ($pin in @($pins.archive) + @($pins.blobs)) {
            $download = @($e.downloads | Where-Object id -CEQ $pin.id)
            if ($download.Count -ne 1 -or $download[0].url -cne $pin.url -or $download[0].sha256 -cne $pin.sha256 -or
                [long]$download[0].length -ne [long]$pin.length -or $download[0].diskSha256 -cne $pin.sha256 -or
                [long]$download[0].diskLength -ne [long]$pin.length -or $download[0].fullHash -ne $true -or $download[0].completed -ne $true) { Reject 'DOWNLOAD' "Missing/full-download mismatch $($pin.id)." }
        }
        if ($e.disk.availableBytes -lt $e.disk.requiredBytes -or $e.disk.requiredBytes -lt 8097805929L + 2GB) { Reject 'DISK' 'Missing measured free-disk/reserve admission.' }
        if ($e.restoration.completed -ne $true -or $e.restoration.nativeCount -ne 0) { Reject 'RESTORATION' 'Default restoration did not complete.' }
        if ($e.cleanup.completed -ne $true -or @($e.cleanup.failures).Count -ne 0 -or $e.cleanup.activeProcesses -ne 0) { Reject 'CLEANUP' 'Cleanup/command cessation incomplete.' }
        foreach ($row in @($e.cleanup.leaves)) { if ($row.owned -ne $true -or $row.absentBefore -ne $true -or $row.absentAfter -ne $true -or $row.reparse -ne $false -or
            $row.identity -cnotmatch '^[0-9a-f]{8}:[0-9a-f]{16}$' -or $row.creationTicks -le 0 -or $row.sha256 -cnotmatch '^[0-9a-f]{64}$' -or $row.length -le 0) { Reject 'CLEANUP' 'Invalid owned leaf cleanup.' } }
        if (@($e.cleanup.leaves).Count -lt 13 -or $e.fixture.beforeDigest -cne $e.fixture.afterDigest -or $e.fixture.beforeDigest -cnotmatch '^[0-9a-f]{64}$') { Reject 'FIXTURE_INVENTORY' 'Fixture inventory or owned-leaf ledger missing/changed.' }
        if (@($e.failures).Count -gt 0) { Reject 'LIFECYCLE_FAILURE' (@($e.failures) -join '; ') }
        # Direct namespace-aware TRX structure, never descendant-only or summary-only proof.
        try {
            $settings = [Xml.XmlReaderSettings]::new(); $settings.DtdProcessing = [Xml.DtdProcessing]::Prohibit; $settings.XmlResolver = $null
            $reader = [Xml.XmlReader]::Create((EvidencePath 'native3.trx'), $settings)
            try { $xml = [Xml.XmlDocument]::new(); $xml.XmlResolver = $null; $xml.Load($reader) } finally { $reader.Dispose() }
            $ns = [Xml.XmlNamespaceManager]::new($xml.NameTable); $ns.AddNamespace('t', 'http://microsoft.com/schemas/VisualStudio/TeamTest/2010')
            if ($xml.DocumentElement.LocalName -cne 'TestRun' -or $xml.DocumentElement.NamespaceURI -cne $ns.LookupNamespace('t')) { throw 'Foreign root/namespace.' }
            foreach ($container in @('TestDefinitions','TestEntries','Results','ResultSummary','Times')) { if ($xml.SelectNodes("/t:TestRun/t:$container", $ns).Count -ne 1) { throw "Missing/duplicate direct $container." } }
            $defs = @($xml.SelectNodes('/t:TestRun/t:TestDefinitions/t:UnitTest', $ns)); $entries = @($xml.SelectNodes('/t:TestRun/t:TestEntries/t:TestEntry', $ns)); $results = @($xml.SelectNodes('/t:TestRun/t:Results/t:UnitTestResult', $ns))
            if ($defs.Count -ne 3 -or $entries.Count -ne 3 -or $results.Count -ne 3 -or
                $xml.SelectNodes('//t:UnitTestResult', $ns).Count -ne 3 -or $xml.SelectNodes('//t:UnitTest', $ns).Count -ne 3 -or
                $xml.SelectNodes('//t:TestEntry', $ns).Count -ne 3 -or $xml.SelectNodes('//*[namespace-uri() != "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"]').Count -gt 0 -or
                $xml.SelectNodes('/t:TestRun/t:TestDefinitions/*', $ns).Count -ne 3 -or $xml.SelectNodes('/t:TestRun/t:TestEntries/*', $ns).Count -ne 3 -or
                $xml.SelectNodes('/t:TestRun/t:Results/*', $ns).Count -ne 3) { throw 'Expected exactly three direct definitions/entries/results; nested/foreign elements forbidden.' }
            if($xml.SelectNodes('//t:TestMethod',$ns).Count -ne 3){throw 'Orphan/nested TestMethod outside direct definition'}
            # Finite VSTest parent/child grammar; known names are not valid in arbitrary locations.
            $children = @{
                TestRun=@('Times','TestSettings','Results','TestDefinitions','TestEntries','TestLists','ResultSummary')
                TestSettings=@('Deployment','Execution'); Results=@('UnitTestResult'); UnitTestResult=@('Output')
                Output=@('StdOut','StdErr','DebugTrace','ErrorInfo'); ErrorInfo=@('Message','StackTrace')
                TestDefinitions=@('UnitTest'); UnitTest=@('Execution','TestMethod'); TestEntries=@('TestEntry')
                TestLists=@('TestList'); ResultSummary=@('Counters','Output','RunInfos'); RunInfos=@('RunInfo'); RunInfo=@('Text')
                Times=@(); Deployment=@(); Execution=@(); StdOut=@(); StdErr=@(); DebugTrace=@(); Message=@(); StackTrace=@()
                TestMethod=@(); TestEntry=@(); TestList=@(); Counters=@(); Text=@()
            }
            $repeatable = @('TestDefinitions/UnitTest','TestEntries/TestEntry','Results/UnitTestResult','TestLists/TestList','RunInfos/RunInfo')
            foreach ($element in $xml.SelectNodes('//*')) {
                if (-not $children.ContainsKey($element.LocalName)) { throw 'Foreign XML element.' }
                if ($element -ne $xml.DocumentElement -and $element.LocalName -cnotin $children[$element.ParentNode.LocalName]) { throw 'Orphan/nested XML element in wrong parent.' }
                foreach ($childName in $children[$element.LocalName]) {
                    if ("$($element.LocalName)/$childName" -cnotin $repeatable -and $element.SelectNodes("t:$childName",$ns).Count -gt 1) { throw 'Duplicate singleton XML element.' }
                }
            }
            $times = $xml.SelectSingleNode('/t:TestRun/t:Times', $ns); $runStart = Utc $times.GetAttribute('start'); $runFinish = Utc $times.GetAttribute('finish')
            if ($runFinish -lt $runStart -or $native.Count -ne 1 -or $runStart -lt (Utc $native[0].startUtc) -or $runFinish -gt (Utc $native[0].endUtc)) { throw 'TRX run interval mismatch.' }
            $ids = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal); $executionIds = [Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal); $fqns = @()
            foreach ($def in $defs) {
                $methods = @($def.SelectNodes('t:TestMethod', $ns)); $exec = @($def.SelectNodes('t:Execution', $ns))
                $id = $def.GetAttribute('id')
                if ($methods.Count -ne 1 -or $exec.Count -ne 1 -or $id -notmatch '^[0-9a-fA-F-]{36}$' -or -not $ids.Add($id)) { throw 'Invalid/duplicate definition ID/TestMethod/Execution.' }
                $fqn = $methods[0].GetAttribute('className') + '.' + $methods[0].GetAttribute('name'); $fqns += $fqn
                $executionId = $exec[0].GetAttribute('id')
                if ($executionId -notmatch '^[0-9a-fA-F-]{36}$' -or -not $executionIds.Add($executionId)) { throw 'Invalid/duplicate execution ID.' }
                $r = @($results | Where-Object { $_.GetAttribute('testId') -ceq $id }); $entry = @($entries | Where-Object { $_.GetAttribute('testId') -ceq $id })
                if ($r.Count -ne 1 -or $entry.Count -ne 1 -or $r[0].GetAttribute('executionId') -cne $executionId -or $entry[0].GetAttribute('executionId') -cne $executionId -or
                    $r[0].GetAttribute('testName') -cne $fqn -or $r[0].GetAttribute('outcome') -cne 'Passed' -or
                    ($methods[0].HasAttribute('isValid') -and $methods[0].GetAttribute('isValid') -cne 'true') -or
                    $methods[0].GetAttribute('adapterTypeName') -cne 'executor://xunit/VsTestRunner2/netcoreapp' -or
                    -not [string]::Equals([IO.Path]::GetFullPath($methods[0].GetAttribute('codeBase')),[IO.Path]::GetFullPath((Join-Path $e.workspace 'Tests/bin/Release/net9.0/LlmFromScratch.Tests.dll')),[StringComparison]::OrdinalIgnoreCase) -or
                    ($def.HasAttribute('storage') -and -not [string]::Equals([IO.Path]::GetFullPath($def.GetAttribute('storage')),[IO.Path]::GetFullPath($methods[0].GetAttribute('codeBase')),[StringComparison]::OrdinalIgnoreCase))) { throw 'Orphan/foreign/duplicate/result binding or non-Passed.' }
                $start = Utc $r[0].GetAttribute('startTime'); $end = Utc $r[0].GetAttribute('endTime'); $duration = [TimeSpan]::Parse($r[0].GetAttribute('duration'), [Globalization.CultureInfo]::InvariantCulture)
                # xUnit/VSTest may stamp start/end at result receipt, independently
                # of measured execution Duration. Bound duration by elapsed time
                # available before the completed receipt, not by its timestamp delta.
                if ($end -lt $start -or $duration.Ticks -lt 0 -or $native.Count -ne 1 -or
                    $start -lt (Utc $native[0].startUtc) -or $end -gt (Utc $native[0].endUtc) -or
                    $start -lt $runStart -or $end -gt $runFinish -or
                    $duration -gt ($end-$runStart) -or $duration -gt ($end-(Utc $native[0].startUtc))) { throw 'Result duration/command/TRX run interval mismatch.' }
            }
            if (-not (SameSet $fqns $identities)) { throw 'Foreign/missing exact identity.' }
            $summary = $xml.SelectSingleNode('/t:TestRun/t:ResultSummary', $ns); $counters = @($summary.SelectNodes('t:Counters', $ns))
            if ($counters.Count -ne 1 -or $summary.GetAttribute('outcome') -cne 'Completed') { throw 'Missing counter/invalid summary.' }
            foreach ($attr in @('total','executed','passed')) { if ($counters[0].GetAttribute($attr) -cne '3') { throw "Wrong $attr counter." } }
            foreach ($attr in @('failed','error','timeout','aborted','inconclusive','passedButRunAborted','notRunnable','notExecuted','disconnected','warning','completed','inProgress','pending','skipped')) { if ($counters[0].HasAttribute($attr) -and $counters[0].GetAttribute($attr) -cne '0') { throw "Nonzero $attr counter." } }
            if ($xml.SelectNodes('//t:RunInfo[@outcome="Error" or @outcome="Failed" or @outcome="Aborted"]|//t:ErrorInfo', $ns).Count -gt 0) { throw 'Loader/platform/RunInfo error.' }
        } catch { Reject 'TRX' $_.Exception.Message }
    } catch { Reject 'INVALID_EVIDENCE' $_.Exception.Message }
    [pscustomobject]@{ schema = 1; valid = $failures.Count -eq 0; failures = @($failures); nativeExecuted = if ($failures.Count -eq 0) { 3 } else { $null } }
}

if ($MyInvocation.InvocationName -ne '.' -and $ValidationDirectory) {
    $result = Test-NativeResourceEvidence -Directory $ValidationDirectory
    $result | ConvertTo-Json -Depth 20
    if (-not $result.valid) { exit 1 }
}
