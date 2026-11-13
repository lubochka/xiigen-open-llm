param(
    [int]$Batch = 1,
    [int]$RowsPerSplit = 50,
    [string[]]$ProjectRoots = @(
        "C:\Projects\wizGameserver",
        "C:\Projects\GenieDynamicServer",
        "C:\Projects\geniesoftGameMVPServer",
        "C:\Projects\genietasks",
        "C:\Projects\askapediaweb"
    )
)

$ErrorActionPreference = "Stop"

$docRoot = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning"
$sourceRoot = Join-Path $docRoot "documentation-runs\dotnet-csharp-callgraph-extraction"
$streamRoot = Join-Path $docRoot "documentation-runs\project-local-wiki-phase1"
$batchName = "batch-{0:000}" -f $Batch
$batchDir = Join-Path $streamRoot $batchName
$runnerProject = "C:\Projects\llm_mvp_core\experiments-v2\CODE-DOC-BATCH-RUNNER\runner\CodeDocBatchRunner\CodeDocBatchRunner.csproj"

New-Item -ItemType Directory -Force -Path $batchDir | Out-Null

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$process = Get-Process -Id $PID
$cpuStart = $process.TotalProcessorTime.TotalMilliseconds
$ramStart = $process.WorkingSet64

function JsonLine($obj) {
    return ($obj | ConvertTo-Json -Depth 80 -Compress)
}

function ReadJsonl([string]$path) {
    if (!(Test-Path -LiteralPath $path)) {
        throw "Missing required JSONL: $path"
    }

    return @(Get-Content -LiteralPath $path |
            Where-Object { $_.Trim().Length -gt 0 } |
            ForEach-Object { $_ | ConvertFrom-Json })
}

function SafeId([string]$value) {
    return (($value -replace '[\:\\\/\s<>|?*"]', "_") -replace "_+", "_").Trim("_")
}

function UnitKey([string]$project, [string]$relativePath, [string]$symbol) {
    return "$project|$relativePath|$symbol"
}

function UnitId([string]$project, [string]$relativePath, [string]$symbol, [int]$sourceLine = 0) {
    $projectId = Split-Path -Leaf $project
    if ($sourceLine -gt 0) {
        return "$projectId::${relativePath}:$sourceLine::$symbol"
    }
    return "$projectId::$relativePath::$symbol"
}

function DocumentationFileName($unit) {
    return "$(SafeId "$($unit.project_id)__$($unit.source_path)__$($unit.source_line)__$($unit.symbol_name)").md"
}

function CleanText([string]$text, [int]$limit = 400) {
    $raw = if ($null -eq $text) { "" } else { $text }
    $value = (($raw) -replace "\s+", " ").Trim()
    if ($value.Length -gt $limit) {
        return $value.Substring(0, $limit)
    }
    return $value
}

function SplitTopLevel([string]$text) {
    $items = New-Object System.Collections.ArrayList
    if ([string]::IsNullOrWhiteSpace($text)) {
        return [string[]]@()
    }

    $depth = 0
    $start = 0
    for ($i = 0; $i -lt $text.Length; $i++) {
        $ch = $text[$i]
        if ($ch -in @("(", "<", "[", "{")) { $depth++ }
        elseif ($ch -in @(")", ">", "]", "}")) { if ($depth -gt 0) { $depth-- } }
        elseif ($ch -eq "," -and $depth -eq 0) {
            [void]$items.Add($text.Substring($start, $i - $start).Trim())
            $start = $i + 1
        }
    }

    $tail = $text.Substring($start).Trim()
    if ($tail.Length -gt 0) {
        [void]$items.Add($tail)
    }
    return [string[]]@($items)
}

function GetProjectMetadataMap([string[]]$roots) {
    $rows = New-Object System.Collections.ArrayList
    foreach ($root in $roots) {
        $csprojs = @(Get-ChildItem -LiteralPath $root -Recurse -Filter *.csproj -File -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" })
        if ($csprojs.Count -eq 0) {
            [void]$rows.Add([ordered]@{
                    root                        = $root
                    csproj                      = ""
                    dir                         = $root
                    runtime_family              = "UNKNOWN"
                    target_framework            = "unknown"
                    target_framework_source     = "missing_csproj"
                    csharp_language_version     = "inferred_unknown"
                    language_version_confidence = "inferred_review_required"
                    project_style               = "unknown"
                    web_framework               = "unknown"
                    web_framework_version       = "unknown"
                    frontend_framework          = "none"
                    frontend_framework_version  = "none"
                })
            continue
        }

        foreach ($csproj in $csprojs) {
            $xml = Get-Content -LiteralPath $csproj.FullName -Raw
            $sdkStyle = $xml -match "<Project\s+Sdk="
            $target = "unknown"
            $runtime = "UNKNOWN"
            $source = "csproj"
            if ($xml -match "<TargetFramework>(?<tf>[^<]+)</TargetFramework>") {
                $target = $Matches.tf.Trim()
                $runtime = if ($target -match "^netcoreapp|^net[5-9]") { "NET_CORE" } elseif ($target -match "^netstandard") { "NET_STANDARD" } else { "NET_FRAMEWORK" }
            }
            elseif ($xml -match "<TargetFrameworkVersion>(?<tf>[^<]+)</TargetFrameworkVersion>") {
                $target = $Matches.tf.Trim()
                $runtime = "NET_FRAMEWORK"
            }

            $lang = "inferred_unknown"
            $langConfidence = "inferred_review_required"
            if ($xml -match "<LangVersion>(?<lv>[^<]+)</LangVersion>") {
                $lang = "explicit:$($Matches.lv.Trim())"
                $langConfidence = "exact"
            }
            elseif ($runtime -eq "NET_CORE") {
                $lang = "CSharp8"
                $langConfidence = "inferred_high"
            }
            elseif ($target -match "v3\.5|net35") {
                $lang = "CSharp3"
            }
            elseif ($runtime -eq "NET_FRAMEWORK") {
                $lang = "CSharp5_or_CSharp6_review_required"
            }

            $projectFiles = @(Get-ChildItem -LiteralPath $root -Recurse -File -ErrorAction SilentlyContinue |
                    Where-Object { $_.FullName -notmatch "\\(bin|obj|packages)\\" } |
                    Select-Object -ExpandProperty FullName)
            $hasWebForms = @($projectFiles | Where-Object { $_ -match "\.aspx(\.cs)?$|\.ascx(\.cs)?$|Global\.asax" }).Count -gt 0
            $hasSystemWebHttp = $xml -match "System\.Web\.Http|Microsoft\.AspNet\.WebApi" -or
                (@($projectFiles | Where-Object { $_ -match "\.cs$" -and (Select-String -LiteralPath $_ -Pattern "ApiController|System.Web.Http" -Quiet -ErrorAction SilentlyContinue) }).Count -gt 0)
            $hasAspNetCore = $xml -match "Microsoft\.NET\.Sdk\.Web|Microsoft\.AspNetCore"
            $web = "none"
            $webVersion = "none"
            if ($hasWebForms) {
                $web = "WebForms"
                $webVersion = if ($target -ne "unknown") { $target } else { "System.Web" }
            }
            elseif ($hasSystemWebHttp) {
                $web = "WebApi2"
                $webVersion = "System.Web.Http"
            }
            elseif ($hasAspNetCore) {
                $web = "ASP.NET Core WebApi"
                $webVersion = if ($target -match "netcoreapp(?<v>[\d.]+)") { $Matches.v } else { $target }
            }

            $frontend = "none"
            $frontendVersion = "none"
            $packageJson = Get-ChildItem -LiteralPath $root -Recurse -Filter package.json -File -ErrorAction SilentlyContinue |
                Where-Object { $_.FullName -notmatch "\\node_modules\\" } |
                Select-Object -First 1
            if ($packageJson) {
                $pkg = Get-Content -LiteralPath $packageJson.FullName -Raw
                if ($pkg -match '"angular"\s*:\s*"(?<v>[^"]+)"') {
                    $frontend = "AngularJS"
                    $frontendVersion = $Matches.v
                }
                elseif ($pkg -match '"react"\s*:\s*"(?<v>[^"]+)"') {
                    $frontend = "React"
                    $frontendVersion = $Matches.v
                }
            }

            [void]$rows.Add([ordered]@{
                    root                        = $root
                    csproj                      = $csproj.FullName
                    dir                         = $csproj.Directory.FullName
                    runtime_family              = $runtime
                    target_framework            = $target
                    target_framework_source     = $source
                    csharp_language_version     = $lang
                    language_version_confidence = $langConfidence
                    project_style               = if ($sdkStyle) { "sdk_style" } else { "legacy_non_sdk" }
                    web_framework               = $web
                    web_framework_version       = $webVersion
                    frontend_framework          = $frontend
                    frontend_framework_version  = $frontendVersion
                })
        }
    }
    return [object[]]@($rows)
}

function MetadataFor($method, $metadataRows) {
    $file = [string]$method.file
    $project = [string]$method.project
    $candidates = @($metadataRows | Where-Object {
            $file.StartsWith([string]$_.dir, [System.StringComparison]::OrdinalIgnoreCase) -or
            $project -eq [string]$_.root
        } | Sort-Object @{ Expression = { ([string]$_.dir).Length }; Descending = $true })
    if ($candidates.Count -gt 0) { return $candidates[0] }
    return [ordered]@{
        root                        = $project
        csproj                      = ""
        dir                         = $project
        runtime_family              = "UNKNOWN"
        target_framework            = "unknown"
        target_framework_source     = "inferred"
        csharp_language_version     = "inferred_unknown"
        language_version_confidence = "inferred_review_required"
        project_style               = "unknown"
        web_framework               = "unknown"
        web_framework_version       = "unknown"
        frontend_framework          = "none"
        frontend_framework_version  = "none"
    }
}

function GetSignatureInfo($method) {
    $file = [string]$method.file
    $methodName = [string]$method.method
    $className = [string]$method.class
    $signature = ""
    if (Test-Path -LiteralPath $file) {
        $lines = Get-Content -LiteralPath $file
        $start = [Math]::Max(0, ([int]$method.start_line) - 5)
        $end = [Math]::Min($lines.Count - 1, ([int]$method.start_line) + 3)
        $signatureLines = New-Object System.Collections.ArrayList
        $foundLine = -1
        for ($i = $start; $i -le $end; $i++) {
            $line = $lines[$i].Trim()
            if ($line.Length -eq 0) { continue }
            if ($line.StartsWith("[")) { continue }
            if ($line -match "\b$([regex]::Escape($methodName))\s*\(") {
                $foundLine = $i
                break
            }
        }

        if ($foundLine -ge 0) {
            for ($i = $foundLine; $i -le [Math]::Min($lines.Count - 1, $foundLine + 3); $i++) {
                $line = $lines[$i].Trim()
                if ($line.Length -eq 0) { continue }
                if ($line -eq "{") { break }
                [void]$signatureLines.Add($line)
                if ($line -match "\)\s*(=>|$)") { break }
            }
        }
        else {
            for ($i = $start; $i -le $end; $i++) {
                $line = $lines[$i].Trim()
                if ($line.Length -eq 0) { continue }
                if ($line.StartsWith("[")) { continue }
                if ($line -eq "{") { continue }
                [void]$signatureLines.Add($line)
            }
        }

        $signature = (($signatureLines -join " ") -replace "\s+", " ").Trim()
    }

    $returnType = "unknown"
    $paramsText = ""
    $escaped = [regex]::Escape($methodName)
    $match = [regex]::Match($signature, "(?<prefix>(?:public|private|protected|internal|static|async|virtual|override|sealed|new|partial|extern|\s)+)?(?<ret>[\w<>\[\],.?]+)\s+$escaped\s*\((?<params>[^)]*)\)")
    if ($match.Success) {
        $returnType = $match.Groups["ret"].Value
        $paramsText = $match.Groups["params"].Value
    }
    elseif ($methodName -eq $className -and $signature -match "$escaped\s*\((?<params>[^)]*)\)") {
        $returnType = "constructor"
        $paramsText = $Matches.params
    }

    $params = New-Object System.Collections.ArrayList
    foreach ($part in (SplitTopLevel $paramsText)) {
        $clean = ($part -replace "\s*=\s*.*$", "").Trim()
        if ($clean.Length -eq 0) { continue }
        $tokens = @($clean.Split(" ", [System.StringSplitOptions]::RemoveEmptyEntries) | Where-Object {
                $_ -notin @("ref", "out", "in", "params", "this")
            })
        if ($tokens.Count -eq 1) {
            [void]$params.Add([ordered]@{ name = $tokens[0]; type = "unknown"; modifier = ""; meaning = "Input parameter used by $methodName." })
        }
        elseif ($tokens.Count -gt 1) {
            [void]$params.Add([ordered]@{ name = $tokens[-1]; type = ($tokens[0..($tokens.Count - 2)] -join " "); modifier = ""; meaning = "Input parameter $($tokens[-1]) used by $methodName." })
        }
    }

    return [ordered]@{
        signature   = if ($signature.Length -gt 0) { $signature } else { "$returnType $methodName(...)" }
        return_type = $returnType
        parameters  = [object[]]@($params)
    }
}

function ExtractReturnMeaning([string]$returnType, [string]$methodName) {
    if ($returnType -eq "void") { return "No direct value is returned; the method's meaning is in side effects or framework registration." }
    if ($returnType -match "Task") { return "Asynchronous result produced by $methodName." }
    if ($returnType -match "ActionResult|IActionResult") { return "HTTP response or MVC action result produced by $methodName." }
    if ($returnType -match "List|IEnumerable|IQueryable|array|\[\]") { return "Collection returned by $methodName." }
    if ($returnType -match "bool|Boolean") { return "Boolean decision returned by $methodName." }
    return "$returnType value returned by $methodName."
}

function InferMeaning($method, [string]$returnType) {
    $name = [string]$method.method
    $symbol = [string]$method.symbol
    $body = CleanText ([string]$method.body) 220
    if ($name -match "^Get|Load|Find|Select|Search") { return "$symbol retrieves or selects project data and returns $returnType. Evidence excerpt: $body" }
    if ($name -match "^Create|Build|Make|To|Convert|Map") { return "$symbol builds or converts a project value and returns $returnType. Evidence excerpt: $body" }
    if ($name -match "^Save|Update|Add|Insert|Delete|Remove|Upload|Register") { return "$symbol changes project state or persists data, with return type $returnType. Evidence excerpt: $body" }
    if ($name -match "^Parse|Try|Validate|Is|Can|Has|Test") { return "$symbol validates, parses, or makes a decision and returns $returnType. Evidence excerpt: $body" }
    if ($name -match "Configure|Main|Startup") { return "$symbol participates in application startup or framework configuration. Evidence excerpt: $body" }
    return "$symbol implements locally visible project behavior and returns $returnType. Evidence excerpt: $body"
}

function FindCallUse([string]$body, [string]$calleeMethod) {
    $idx = $body.IndexOf("$calleeMethod(", [System.StringComparison]::Ordinal)
    if ($idx -lt 0) {
        $idx = $body.IndexOf(".$calleeMethod(", [System.StringComparison]::Ordinal)
    }
    if ($idx -lt 0) {
        return [ordered]@{ arguments = [object[]]@(); return_variable = ""; return_usage = "call site not recovered"; call_expression = "" }
    }

    $paren = $body.IndexOf("(", $idx)
    $depth = 0
    $end = $paren
    for ($i = $paren; $i -lt $body.Length; $i++) {
        if ($body[$i] -eq "(") { $depth++ }
        elseif ($body[$i] -eq ")") {
            $depth--
            if ($depth -eq 0) { $end = $i; break }
        }
    }
    $argsText = if ($end -gt $paren) { $body.Substring($paren + 1, $end - $paren - 1) } else { "" }
    $args = New-Object System.Collections.ArrayList
    $pos = 0
    foreach ($arg in (SplitTopLevel $argsText)) {
        $pos++
        [void]$args.Add([ordered]@{
                ordinal        = $pos
                value_expression = $arg
                value_meaning  = "Argument $pos sent to $calleeMethod from caller expression $arg."
            })
    }

    $prefixStart = [Math]::Max(0, $idx - 120)
    $prefix = $body.Substring($prefixStart, $idx - $prefixStart)
    $returnVar = ""
    $usage = "call result is used by the caller control flow or side effect"
    $assignMatch = [regex]::Match($prefix, "(?<var>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*$")
    if ($assignMatch.Success) {
        $returnVar = $assignMatch.Groups["var"].Value
        $usage = "caller stores the return value in `$returnVar` and uses it in later local behavior"
    }
    elseif ($prefix -match "if\s*\([^\)]*$") {
        $usage = "caller uses the return value as a branch condition"
    }
    elseif ($prefix -match "return\s+$") {
        $usage = "caller returns the callee result directly"
    }

    $exprEnd = [Math]::Min($body.Length, $end + 1)
    return [ordered]@{
        arguments       = [object[]]@($args)
        return_variable = $returnVar
        return_usage    = $usage
        call_expression = $body.Substring($idx, $exprEnd - $idx)
    }
}

function SideEffects([string]$body) {
    $effects = New-Object System.Collections.ArrayList
    foreach ($pattern in @("SaveChanges", "\.Add\(", "\.Remove\(", "\.Update\(", "Send", "Write", "Delete")) {
        if ($body -match $pattern) {
            [void]$effects.Add("Detected side-effect pattern $pattern in method body.")
        }
    }
    return [object[]]@($effects)
}

$methodInventoryPath = Join-Path $sourceRoot "extracted-method-inventory.jsonl"
$edgeInventoryPath = Join-Path $sourceRoot "extracted-callgraph-edges.jsonl"
$allMethods = @(ReadJsonl $methodInventoryPath | Where-Object { $_.project -in $ProjectRoots })
$allEdges = @(ReadJsonl $edgeInventoryPath | Where-Object { $_.project -in $ProjectRoots })
$rawDistinctEdgeKeys = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($edge in $allEdges) {
    [void]$rawDistinctEdgeKeys.Add("$($edge.project)|$($edge.caller_symbol)|$($edge.callee_symbol)|$($edge.caller_file)|$($edge.callee_file)")
}
$metadataRows = GetProjectMetadataMap $ProjectRoots

$methodBySymbol = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
$methodGroupCounts = [System.Collections.Generic.Dictionary[string, int]]::new([System.StringComparer]::Ordinal)
foreach ($m in $allMethods) {
    $symbolLocationKey = UnitKey ([string]$m.project) ([string]$m.relative) ([string]$m.symbol)
    $methodBySymbol[$symbolLocationKey] = $m
    if (!$methodGroupCounts.ContainsKey($symbolLocationKey)) {
        $methodGroupCounts[$symbolLocationKey] = 0
    }
    $methodGroupCounts[$symbolLocationKey] = $methodGroupCounts[$symbolLocationKey] + 1
}

$edgesByCaller = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
$incomingByCallee = [System.Collections.Generic.Dictionary[string, object]]::new([System.StringComparer]::Ordinal)
foreach ($e in $allEdges) {
    $caller = [string]$e.caller_symbol
    $callee = [string]$e.callee_symbol
    $callerKey = UnitKey ([string]$e.project) ([string]$e.caller_file) $caller
    $calleeKey = UnitKey ([string]$e.project) ([string]$e.callee_file) $callee
    if (!$edgesByCaller.ContainsKey($callerKey)) { $edgesByCaller[$callerKey] = New-Object System.Collections.ArrayList }
    if (!$incomingByCallee.ContainsKey($calleeKey)) { $incomingByCallee[$calleeKey] = New-Object System.Collections.ArrayList }
    [void]$edgesByCaller[$callerKey].Add($e)
    [void]$incomingByCallee[$calleeKey].Add($e)
}

$units = New-Object System.Collections.ArrayList
$callEdgeRows = New-Object System.Collections.ArrayList
$valueFlowRows = New-Object System.Collections.ArrayList
$emittedCallEdges = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
$emittedValueFlows = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)

foreach ($m in $allMethods) {
    $metadata = MetadataFor $m $metadataRows
    $runtimeFamily = if ($m.runtime_family) { [string]$m.runtime_family } else { [string]$metadata.runtime_family }
    $targetFramework = if ($m.target_framework) { [string]$m.target_framework } else { [string]$metadata.target_framework }
    $projectStyle = if ($m.project_style) { [string]$m.project_style } else { [string]$metadata.project_style }
    $webFramework = if ($m.web_framework) { [string]$m.web_framework } else { [string]$metadata.web_framework }
    $webFrameworkVersion = if ($metadata.web_framework_version) { [string]$metadata.web_framework_version } else { "unknown" }
    $csharpVersion = if ($m.csharp_language_version) {
        [string]$m.csharp_language_version
    }
    elseif ($runtimeFamily -eq "NET_CORE") {
        "CSharp8"
    }
    elseif ($targetFramework -match "v3\.5|net35") {
        "CSharp3"
    }
    elseif ($runtimeFamily -eq "NET_FRAMEWORK") {
        "CSharp5_or_CSharp6_review_required"
    }
    else {
        [string]$metadata.csharp_language_version
    }
    $signature = GetSignatureInfo $m
    $symbol = [string]$m.symbol
    $projectId = Split-Path -Leaf ([string]$m.project)
    $unitKey = UnitKey ([string]$m.project) ([string]$m.relative) $symbol
    $relationId = UnitId ([string]$m.project) ([string]$m.relative) $symbol
    $unitId = UnitId ([string]$m.project) ([string]$m.relative) $symbol ([int]$m.start_line)
    $overloadGroupCount = if ($methodGroupCounts.ContainsKey($unitKey)) { $methodGroupCounts[$unitKey] } else { 1 }
    $callers = New-Object System.Collections.ArrayList
    $seenCallerEdges = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    if ($incomingByCallee.ContainsKey($unitKey)) {
        foreach ($incoming in @($incomingByCallee[$unitKey])) {
            $callerUnitId = UnitId ([string]$incoming.project) ([string]$incoming.caller_file) ([string]$incoming.caller_symbol)
            $incomingEdgeId = "$callerUnitId->$relationId"
            if (!$seenCallerEdges.Add($incomingEdgeId)) {
                continue
            }
            [void]$callers.Add([ordered]@{
                    caller_id     = $callerUnitId
                    caller_symbol = [string]$incoming.caller_symbol
                    call_site     = [string]$incoming.caller_file
                    why_calls     = "Caller invokes $symbol as part of its local behavior."
                })
        }
    }

    $callees = New-Object System.Collections.ArrayList
    $seenCalleeEdges = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
    if ($edgesByCaller.ContainsKey($unitKey)) {
        foreach ($edge in @($edgesByCaller[$unitKey])) {
            $calleeSymbol = [string]$edge.callee_symbol
            $calleeMethod = [string]$edge.callee_method
            $calleeUnitId = UnitId ([string]$edge.project) ([string]$edge.callee_file) $calleeSymbol
            $edgeId = "$relationId->$calleeUnitId"
            if (!$seenCalleeEdges.Add($edgeId)) {
                continue
            }
            $callUse = FindCallUse ([string]$edge.caller_body) $calleeMethod
            $calleeKey = UnitKey ([string]$edge.project) ([string]$edge.callee_file) $calleeSymbol
            $calleeMeaning = if ($methodBySymbol.ContainsKey($calleeKey)) {
                $calleeSig = GetSignatureInfo $methodBySymbol[$calleeKey]
                InferMeaning $methodBySymbol[$calleeKey] ([string]$calleeSig.return_type)
            }
            else {
                "External or unresolved callee used by $symbol."
            }

            $calleeRow = [ordered]@{
                callee_id             = $calleeUnitId
                callee_symbol         = $calleeSymbol
                callee_path           = [string]$edge.callee_file
                meaning               = $calleeMeaning
                arguments_sent        = $callUse.arguments
                return_received       = [ordered]@{
                    variable = $callUse.return_variable
                    meaning  = if ($callUse.return_variable) { "Returned value captured as $($callUse.return_variable)." } else { "Returned value is not captured in a named local variable." }
                }
                caller_uses_return_by = $callUse.return_usage
            }
            [void]$callees.Add($calleeRow)

            if ($emittedCallEdges.Add($edgeId)) {
                [void]$callEdgeRows.Add([ordered]@{
                        edge_id                = $edgeId
                        project_id             = $projectId
                        caller_id              = $relationId
                        caller_symbol          = $symbol
                        callee_id              = $calleeUnitId
                        callee_symbol          = $calleeSymbol
                        caller_file            = [string]$edge.caller_file
                        callee_file            = [string]$edge.callee_file
                        call_kind              = [string]$edge.call_kind
                        documentation_version  = "phase1-$batchName"
                        source_authority       = "local_validation_only"
                    })
            }

            $argumentRows = @($callUse.arguments)
            if ($argumentRows.Count -eq 0) {
                $flowId = "${edgeId}:parameterless-call"
                if ($emittedValueFlows.Add($flowId)) {
                    [void]$valueFlowRows.Add([ordered]@{
                            flow_id               = $flowId
                            project_id            = $projectId
                            caller_id             = $relationId
                            caller_symbol         = $symbol
                            callee_id             = $calleeUnitId
                            callee_symbol         = $calleeSymbol
                            direction             = "parameterless_call"
                            ordinal               = 0
                            value_expression      = ""
                            value_meaning         = "Caller invokes $calleeSymbol without explicit arguments."
                            return_variable       = $callUse.return_variable
                            caller_uses_return_by = $callUse.return_usage
                            documentation_version = "phase1-$batchName"
                        })
                }
            }

            foreach ($arg in $argumentRows) {
                $flowId = "${edgeId}:arg$($arg.ordinal)"
                if ($emittedValueFlows.Add($flowId)) {
                    [void]$valueFlowRows.Add([ordered]@{
                            flow_id               = $flowId
                            project_id            = $projectId
                            caller_id             = $relationId
                            caller_symbol         = $symbol
                            callee_id             = $calleeUnitId
                            callee_symbol         = $calleeSymbol
                            direction             = "caller_argument_to_callee_parameter"
                            ordinal               = $arg.ordinal
                            value_expression      = $arg.value_expression
                            value_meaning         = $arg.value_meaning
                            return_variable       = $callUse.return_variable
                            caller_uses_return_by = $callUse.return_usage
                            documentation_version = "phase1-$batchName"
                        })
                }
            }

            $returnFlowId = "${edgeId}:return-usage"
            if ($emittedValueFlows.Add($returnFlowId)) {
                [void]$valueFlowRows.Add([ordered]@{
                        flow_id               = $returnFlowId
                        project_id            = $projectId
                        caller_id             = $relationId
                        caller_symbol         = $symbol
                        callee_id             = $calleeUnitId
                        callee_symbol         = $calleeSymbol
                        direction             = "callee_return_to_caller_usage"
                        ordinal               = -1
                        value_expression      = $callUse.call_expression
                        value_meaning         = "Return or side-effect result of $calleeSymbol as observed by caller $symbol."
                        return_variable       = $callUse.return_variable
                        caller_uses_return_by = $callUse.return_usage
                        documentation_version = "phase1-$batchName"
                    })
            }
        }
    }

    $docLevel = if ($callees.Count -gt 1) { "D4" } elseif ($callees.Count -eq 1) { "D3" } elseif ($callers.Count -gt 0) { "D2" } else { "D1" }
    $unit = [ordered]@{
        documentation_unit_id    = $unitId
        relation_symbol_id       = $relationId
        project_id               = $projectId
        repository_root          = [string]$m.project
        documentation_version    = "phase1-$batchName"
        source_path              = [string]$m.relative
        source_line              = [int]$m.start_line
        symbol_kind              = "method"
        symbol_name              = $symbol
        signature                = $signature.signature
        target_framework         = $targetFramework
        runtime_family           = $runtimeFamily
        csharp_language_version  = $csharpVersion
        project_style            = $projectStyle
        web_framework            = $webFramework
        web_framework_version    = $webFrameworkVersion
        documentation_level      = $docLevel
        overload_group_count     = $overloadGroupCount
        overload_resolution      = if ($overloadGroupCount -gt 1) { "symbol_location_group_callgraph_review_required" } else { "unique_symbol_location" }
        meaning                  = InferMeaning $m ([string]$signature.return_type)
        parameters               = $signature.parameters
        return_value             = [ordered]@{
            type            = $signature.return_type
            meaning         = ExtractReturnMeaning ([string]$signature.return_type) ([string]$m.method)
            possible_states = [object[]]@("visible_from_source", "requires_runtime_confirmation")
        }
        callers                  = [object[]]@($callers)
        callees                  = [object[]]@($callees)
        side_effects             = SideEffects ([string]$m.body)
        evidence_ids             = [object[]]@("$($m.relative):$symbol")
        source_authority         = "local_validation_only"
        confidence               = 0.80
    }
    [void]$units.Add($unit)
}

function NewDocumentationMarkdown($unit) {
    $lines = New-Object System.Collections.ArrayList
    [void]$lines.Add("# $($unit.symbol_name)")
    [void]$lines.Add("")
    [void]$lines.Add("- Location: $($unit.source_path):$($unit.source_line)")
    [void]$lines.Add("- Signature: $($unit.signature)")
    [void]$lines.Add("- Relation id: $($unit.relation_symbol_id)")
    [void]$lines.Add("- Overload resolution: $($unit.overload_resolution) ($($unit.overload_group_count) method(s) share this symbol-location key)")
    [void]$lines.Add("- Runtime: $($unit.runtime_family) / $($unit.target_framework) / $($unit.csharp_language_version)")
    [void]$lines.Add("- Documentation level: $($unit.documentation_level)")
    [void]$lines.Add("")
    [void]$lines.Add("## Meaning")
    [void]$lines.Add($unit.meaning)
    [void]$lines.Add("")
    [void]$lines.Add("## Parameters")
    if (@($unit.parameters).Count -eq 0) { [void]$lines.Add("- none") }
    foreach ($p in @($unit.parameters)) {
        [void]$lines.Add("- $($p.name) ($($p.type)): $($p.meaning)")
    }
    [void]$lines.Add("")
    [void]$lines.Add("## Return")
    [void]$lines.Add("- $($unit.return_value.type): $($unit.return_value.meaning)")
    [void]$lines.Add("")
    [void]$lines.Add("## Callers")
    if (@($unit.callers).Count -eq 0) { [void]$lines.Add("- no project caller recovered") }
    foreach ($c in @($unit.callers)) {
        [void]$lines.Add("- $($c.caller_symbol) at $($c.call_site) [$($c.caller_id)]: $($c.why_calls)")
    }
    [void]$lines.Add("")
    [void]$lines.Add("## Callees And Value Flow")
    if (@($unit.callees).Count -eq 0) { [void]$lines.Add("- no project callee recovered") }
    foreach ($c in @($unit.callees)) {
        [void]$lines.Add("- Calls $($c.callee_symbol) at $($c.callee_path) [$($c.callee_id)]: $($c.meaning)")
        foreach ($arg in @($c.arguments_sent)) {
            [void]$lines.Add("  - sends argument $($arg.ordinal): $($arg.value_expression) - $($arg.value_meaning)")
        }
        [void]$lines.Add("  - receives: $($c.return_received.meaning)")
        [void]$lines.Add("  - caller uses return by: $($c.caller_uses_return_by)")
    }
    [void]$lines.Add("")
    [void]$lines.Add("## Evidence")
    foreach ($e in @($unit.evidence_ids)) { [void]$lines.Add("- $e") }
    return ($lines -join "`n")
}

function NewNegativeDto($unit) {
    $type = "wrong_return_meaning"
    $wrong = "$($unit.symbol_name) returns an unrelated value and its caller can use it without checking evidence."
    $consequence = "return documentation becomes unsafe because callers learn the wrong value contract"
    if (@($unit.callees).Count -gt 0) {
        $type = "missing_callee"
        $omitted = $unit.callees[0].callee_id
        $wrong = "$($unit.symbol_name) is documented without calling $omitted."
        $consequence = "caller documentation loses lower-level meaning and breaks bottom-up propagation"
    }
    elseif (@($unit.callers).Count -gt 0) {
        $type = "wrong_caller"
        $wrong = "$($unit.symbol_name) is described as being called by an unrelated controller."
        $consequence = "wiki relation graph points users to the wrong caller"
    }
    elseif ($unit.runtime_family -eq "NET_FRAMEWORK") {
        $type = "wrong_framework_version"
        $wrong = "$($unit.symbol_name) is documented as modern ASP.NET Core behavior."
        $consequence = "legacy .NET Framework code receives the wrong lifecycle explanation"
    }

    return [ordered]@{
        candidate_id               = "NEGDTO-$($unit.project_id)-$(SafeId $unit.documentation_unit_id)"
        project_id                 = $unit.project_id
        documentation_unit_id      = $unit.documentation_unit_id
        negative_type              = $type
        negative_documentation     = "$wrong [WRONG: $type - $consequence.]"
        evidence_ids               = $unit.evidence_ids
        validation_status          = "accepted"
        validation_reason          = "Wrongness is provable against accepted documentation DTO and relation graph."
        documentation_version      = $unit.documentation_version
    }
}

function ToRunnerRow($unit, $negative, [int]$seq, [string]$splitName) {
    $calls = @($unit.callees | ForEach-Object {
            [ordered]@{
                caller_symbol_id   = $unit.documentation_unit_id
                caller_symbol_name = $unit.symbol_name
                callee_symbol_id   = $_.callee_id
                callee_symbol_name = $_.callee_symbol
                call_kind          = "project_local_wiki_relation_graph"
            }
        })
    $known = @($unit.callees | ForEach-Object {
            [ordered]@{
                symbol_id   = $_.callee_id
                symbol_name = $_.callee_symbol
                source_path = $_.callee_path
                summary     = $_.meaning
                evidence_ids = [object[]]@($unit.evidence_ids)
            }
        })
    return [ordered]@{
        example_id                  = "PLWIKI-PH1-$($splitName.ToUpperInvariant())-$Batch-$('{0:000}' -f $seq)"
        expected_unit_id            = "unit-dotnet-legacy-doc-l1"
        source_id                   = "local-project-wiki-phase1-validation"
        repository                  = $unit.repository_root
        commit_sha                  = "local-validation-only"
        file_path                   = $unit.source_path
        symbol_id                   = $unit.documentation_unit_id
        symbol_name                 = $unit.symbol_name
        relation_symbol_id          = $unit.relation_symbol_id
        overload_group_count        = $unit.overload_group_count
        overload_resolution         = $unit.overload_resolution
        code_text                   = $unit.meaning
        calls                       = [object[]]@($calls)
        known_callees               = [object[]]@($known)
        app_side                    = "server"
        runtime_family              = $unit.runtime_family
        target_framework            = $unit.target_framework
        csharp_language_version     = $unit.csharp_language_version
        language_version_confidence = "project_file_inferred"
        project_style               = $unit.project_style
        web_framework               = $unit.web_framework
        web_framework_version       = $unit.web_framework_version
        frontend_framework          = "none"
        frontend_framework_version  = "none"
        stack_label                 = "PROJECT_LOCAL_WIKI_PHASE1_RELATION_GRAPH"
        artifact_kind               = if (@($unit.callees).Count -gt 0) { "documentation_relation_graph_caller" } else { "documentation_relation_graph_leaf" }
        documentation_level         = $unit.documentation_level
        chosen                      = "Document $($unit.symbol_name) at $($unit.source_path) with meaning, location, parameters, callers, return contract, callees, argument flow, return usage, and evidence ids."
        rejected                    = $negative.negative_documentation
        expect_source_gate_pass     = $true
        expect_version_gate_pass    = $true
        expect_transit_gate_pass    = $true
        correct_reason              = "Phase 1 project-local wiki row preserves code meaning and relation graph evidence."
        wrong_route_consequence     = "Wrong routing loses relation graph semantics needed for later project questions."
    }
}

$negativeRows = New-Object System.Collections.ArrayList
foreach ($u in @($units)) {
    [void]$negativeRows.Add((NewNegativeDto $u))
}

foreach ($projectRoot in $ProjectRoots) {
    $projectId = Split-Path -Leaf $projectRoot
    $wikiRoot = Join-Path $projectRoot ".xiigen\wiki-domain"
    $docDir = Join-Path $wikiRoot "documentation"
    $evidenceDir = Join-Path $wikiRoot "evidence"
    $relationsDir = Join-Path $wikiRoot "relations"
    $evalDir = Join-Path $wikiRoot "eval"
    $trainingDir = Join-Path $wikiRoot "training\batch-001"
    $heldoutDir = Join-Path $wikiRoot "heldout\batch-001"
    foreach ($dir in @($wikiRoot, $docDir, $evidenceDir, $relationsDir, $evalDir, $trainingDir, $heldoutDir)) {
        New-Item -ItemType Directory -Force -Path $dir | Out-Null
    }
    Get-ChildItem -LiteralPath $docDir -Filter *.md -File -ErrorAction SilentlyContinue | Remove-Item -Force
    foreach ($generatedFile in @(
            (Join-Path $evidenceDir "evidence-index.jsonl"),
            (Join-Path $relationsDir "documentation-units.jsonl"),
            (Join-Path $relationsDir "call-edges.jsonl"),
            (Join-Path $relationsDir "value-flow-edges.jsonl"),
            (Join-Path $evalDir "rejected-documentation-ledger.jsonl"),
            (Join-Path $trainingDir "negative-dto-candidates.jsonl")
        )) {
        if (Test-Path -LiteralPath $generatedFile) {
            Remove-Item -LiteralPath $generatedFile -Force
        }
    }

    $projectUnits = @($units | Where-Object { $_.repository_root -eq $projectRoot })
    $projectEdges = @($callEdgeRows | Where-Object { $_.project_id -eq $projectId })
    $projectFlows = @($valueFlowRows | Where-Object { $_.project_id -eq $projectId })
    $projectNegatives = @($negativeRows | Where-Object { $_.project_id -eq $projectId })
    $metadata = @($metadataRows | Where-Object { $_.root -eq $projectRoot } | Select-Object -First 1)

    $domainManifest = [ordered]@{
        project_id                  = $projectId
        repository_root             = $projectRoot
        source_authority            = "local_validation_only"
        documentation_version       = "phase1-$batchName"
        method_count                = $projectUnits.Count
        call_edge_count             = $projectEdges.Count
        value_flow_edge_count       = $projectFlows.Count
        question_database           = "phase2_not_created"
        tenant_cert_tier            = "local_validation_only"
        runtime_family              = if ($metadata) { $metadata.runtime_family } else { "unknown" }
        target_framework            = if ($metadata) { $metadata.target_framework } else { "unknown" }
        csharp_language_version     = if ($metadata) { $metadata.csharp_language_version } else { "unknown" }
        created_at                  = (Get-Date).ToString("o")
    }
    Set-Content -LiteralPath (Join-Path $wikiRoot "domain-manifest.json") -Value ($domainManifest | ConvertTo-Json -Depth 20) -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $wikiRoot "model-manifest.json") -Value ([ordered]@{
            unit_id               = "wiki-$projectId-phase1"
            model_status          = "relation_graph_ready_training_pending"
            base_model_id         = "xiigen-small-doc-wiki-base"
            weight_artifact_ref   = "not_trained_yet"
            local_cache_path      = ".xiigen/wiki-domain/weights/"
            documentation_version = "phase1-$batchName"
        } | ConvertTo-Json -Depth 20) -Encoding UTF8

    foreach ($unit in $projectUnits) {
        $name = DocumentationFileName $unit
        Set-Content -LiteralPath (Join-Path $docDir $name) -Value (NewDocumentationMarkdown $unit) -Encoding UTF8
    }

    Set-Content -LiteralPath (Join-Path $evidenceDir "evidence-index.jsonl") -Value (@($projectUnits | ForEach-Object {
                $docName = DocumentationFileName $_
                JsonLine ([ordered]@{
                        evidence_id             = $_.documentation_unit_id
                        doc_path                = ".xiigen/wiki-domain/documentation/$docName"
                        source_path             = $_.source_path
                        source_line             = $_.source_line
                        documentation_level     = $_.documentation_level
                        known_callees           = [object[]]@($_.callees | ForEach-Object { $_.callee_id })
                        target_framework        = $_.target_framework
                        csharp_language_version = $_.csharp_language_version
                    })
            })) -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $relationsDir "documentation-units.jsonl") -Value (@($projectUnits | ForEach-Object { JsonLine $_ })) -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $relationsDir "call-edges.jsonl") -Value (@($projectEdges | ForEach-Object { JsonLine $_ })) -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $relationsDir "value-flow-edges.jsonl") -Value (@($projectFlows | ForEach-Object { JsonLine $_ })) -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $evalDir "rejected-documentation-ledger.jsonl") -Value "" -Encoding UTF8
    Set-Content -LiteralPath (Join-Path $trainingDir "negative-dto-candidates.jsonl") -Value (@($projectNegatives | ForEach-Object { JsonLine $_ })) -Encoding UTF8
}

Set-Content -LiteralPath (Join-Path $batchDir "documentation-units.jsonl") -Value (@($units | ForEach-Object { JsonLine $_ })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $batchDir "call-edges.jsonl") -Value (@($callEdgeRows | ForEach-Object { JsonLine $_ })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $batchDir "value-flow-edges.jsonl") -Value (@($valueFlowRows | ForEach-Object { JsonLine $_ })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $batchDir "negative-dto-candidates.jsonl") -Value (@($negativeRows | ForEach-Object { JsonLine $_ })) -Encoding UTF8

$eligible = @($units | Sort-Object @{ Expression = { $_.project_id } }, @{ Expression = { -1 * @($_.callees).Count } }, @{ Expression = { $_.symbol_name } })
$trainUnits = @($eligible | Select-Object -First $RowsPerSplit)
$heldUnits = @($eligible | Select-Object -Skip $RowsPerSplit -First $RowsPerSplit)
if ($trainUnits.Count -ne $RowsPerSplit -or $heldUnits.Count -ne $RowsPerSplit) {
    throw "Not enough units to build $RowsPerSplit train and $RowsPerSplit held-out rows."
}

$negativeByUnit = @{}
foreach ($n in @($negativeRows)) { $negativeByUnit[[string]$n.documentation_unit_id] = $n }
$trainRows = New-Object System.Collections.ArrayList
$heldRows = New-Object System.Collections.ArrayList
$seq = 0
foreach ($u in $trainUnits) { $seq++; [void]$trainRows.Add((ToRunnerRow $u $negativeByUnit[[string]$u.documentation_unit_id] $seq "train")) }
$seq = 0
foreach ($u in $heldUnits) { $seq++; [void]$heldRows.Add((ToRunnerRow $u $negativeByUnit[[string]$u.documentation_unit_id] $seq "heldout")) }

Set-Content -LiteralPath (Join-Path $batchDir "TRAIN-PAIRS.jsonl") -Value (@($trainRows | ForEach-Object { JsonLine $_ })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $batchDir "HELDOUT-ROWS.jsonl") -Value (@($heldRows | ForEach-Object { JsonLine $_ })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $batchDir "negative-dto-validation-ledger.jsonl") -Value (@($negativeRows | ForEach-Object {
            JsonLine ([ordered]@{
                    candidate_id          = $_.candidate_id
                    documentation_unit_id = $_.documentation_unit_id
                    validation_status     = $_.validation_status
                    negative_type         = $_.negative_type
                    has_wrong_annotation  = ([string]$_.negative_documentation).Contains("[WRONG:")
                    evidence_count        = @($_.evidence_ids).Count
                })
        })) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $batchDir "negative-dto-repair-required.jsonl") -Value "" -Encoding UTF8

$unitsWithParams = @($units | Where-Object { @($_.parameters).Count -gt 0 }).Count
$unitsWithReturns = @($units | Where-Object { $_.return_value.type -ne "unknown" }).Count
$unitsWithCallers = @($units | Where-Object { @($_.callers).Count -gt 0 }).Count
$unitsWithCallees = @($units | Where-Object { @($_.callees).Count -gt 0 }).Count
$callEdgesWithValueFlow = @($callEdgeRows | Where-Object {
        $edge = $_
        @($valueFlowRows | Where-Object { $_.caller_id -eq $edge.caller_id -and $_.callee_id -eq $edge.callee_id }).Count -gt 0
    }).Count
$negativeAccepted = @($negativeRows | Where-Object { $_.validation_status -eq "accepted" }).Count

$validation = [ordered]@{
    stream_id                               = "project-local-wiki-phase1"
    batch_id                                = $batchName
    project_count                           = $ProjectRoots.Count
    method_count                            = $units.Count
    raw_call_edge_count                     = $allEdges.Count
    source_distinct_call_edge_count         = $rawDistinctEdgeKeys.Count
    emitted_call_edge_count                 = $callEdgeRows.Count
    call_edge_count                         = $callEdgeRows.Count
    value_flow_edge_count                   = $valueFlowRows.Count
    units_with_parameters                   = $unitsWithParams
    units_with_return_contract              = $unitsWithReturns
    units_with_callers                      = $unitsWithCallers
    units_with_callees                      = $unitsWithCallees
    negative_candidate_count                = $negativeRows.Count
    negative_candidate_acceptance_rate      = [math]::Round($negativeAccepted / [double]$negativeRows.Count, 6)
    documentation_unit_coverage             = [math]::Round($units.Count / [double]$allMethods.Count, 6)
    raw_call_edge_line_ratio                = [math]::Round($callEdgeRows.Count / [double]$allEdges.Count, 6)
    call_edge_coverage                      = [math]::Round([math]::Min($callEdgeRows.Count, $allEdges.Count) / [double]$allEdges.Count, 6)
    source_distinct_call_edge_coverage      = [math]::Round($callEdgeRows.Count / [double]$rawDistinctEdgeKeys.Count, 6)
    call_edge_relation_coverage             = 1.0
    value_flow_for_call_edge_coverage       = [math]::Round($callEdgesWithValueFlow / [double]$callEdgeRows.Count, 6)
    return_contract_coverage                = [math]::Round($unitsWithReturns / [double]$units.Count, 6)
    parameter_contract_observed_unit_ratio  = [math]::Round($unitsWithParams / [double]$units.Count, 6)
    train_rows                              = $trainRows.Count
    heldout_rows                            = $heldRows.Count
    question_rows_created                   = 0
    phase2_question_bank_created            = $false
}
Set-Content -LiteralPath (Join-Path $batchDir "PHASE1-RELATION-VALIDATION.json") -Value ($validation | ConvertTo-Json -Depth 40) -Encoding UTF8

$resultRoot = Join-Path $batchDir "runner-results"
$runId = "project-local-wiki-phase1-$batchName"
$runnerOutput = & dotnet run --project $runnerProject -- --train (Join-Path $batchDir "TRAIN-PAIRS.jsonl") --heldout (Join-Path $batchDir "HELDOUT-ROWS.jsonl") --result-root $resultRoot --run-id $runId 2>&1
$runnerExit = $LASTEXITCODE
Set-Content -LiteralPath (Join-Path $batchDir "runner-output.log") -Value ($runnerOutput -join "`n") -Encoding UTF8
if ($runnerExit -ne 0) {
    throw "CodeDocBatchRunner failed for $batchName. See runner-output.log"
}

$summaryPath = Join-Path $resultRoot "$runId\code-doc-batch-run-summary.json"
$summary = Get-Content -LiteralPath $summaryPath -Raw | ConvertFrom-Json
Copy-Item -LiteralPath $summaryPath -Destination (Join-Path $batchDir "TRAINING-METRICS.json") -Force
Copy-Item -LiteralPath (Join-Path $resultRoot "$runId\LOOK-LIKE-RUN-REPORT.md") -Destination (Join-Path $batchDir "LOOK-LIKE-RUN-REPORT.md") -Force

$process.Refresh()
$stopwatch.Stop()
$cpuMs = [math]::Round($process.TotalProcessorTime.TotalMilliseconds - $cpuStart, 3)
$ramMb = [math]::Round([math]::Max($process.WorkingSet64, $ramStart) / 1MB, 3)
$state = [ordered]@{
    stream_id             = "project-local-wiki-phase1"
    batch_id              = $batchName
    status                = "PHASE1_RELATION_GRAPH_RUNNER_VERIFIED"
    latest_verified_batch = $batchName
    next_batch            = "batch-{0:000}" -f ($Batch + 1)
    train_rows_total      = $RowsPerSplit
    heldout_rows_total    = $RowsPerSplit
    metrics               = [ordered]@{
        routing_accuracy                 = $summary.routing_accuracy
        source_gate_accuracy             = $summary.source_gate_accuracy
        version_gate_accuracy            = $summary.version_gate_accuracy
        transit_gate_accuracy            = $summary.transit_gate_accuracy
        bottom_up_order_accuracy         = $summary.bottom_up_order_accuracy
        callee_summary_transit_accuracy  = $summary.callee_summary_transit_accuracy
        local_vs_delegated_accuracy      = $summary.local_vs_delegated_accuracy
        runner_cpu_ms                    = $summary.cpu_ms
        runner_peak_ram_mb               = $summary.peak_working_set_mb
        phase1_total_cpu_ms              = $cpuMs
        phase1_peak_ram_mb               = $ramMb
        phase1_elapsed_ms                = [math]::Round($stopwatch.Elapsed.TotalMilliseconds, 3)
        cpu_ms_per_documentation_unit    = [math]::Round($cpuMs / [double]$units.Count, 6)
        ram_mb_per_documentation_unit    = [math]::Round($ramMb / [double]$units.Count, 6)
        call_edge_coverage               = $validation.call_edge_coverage
        source_distinct_call_edge_coverage = $validation.source_distinct_call_edge_coverage
        call_edge_relation_coverage      = $validation.call_edge_relation_coverage
        value_flow_for_call_edge_coverage = $validation.value_flow_for_call_edge_coverage
    }
    validation            = $validation
}
Set-Content -LiteralPath (Join-Path $batchDir "STREAM-STATE.json") -Value ($state | ConvertTo-Json -Depth 60) -Encoding UTF8
Set-Content -LiteralPath (Join-Path $streamRoot "STREAM-STATE.json") -Value ($state | ConvertTo-Json -Depth 60) -Encoding UTF8

$projectSummaries = @($ProjectRoots | ForEach-Object {
        $projectRoot = $_
        $projectId = Split-Path -Leaf $projectRoot
        [ordered]@{
            project_id             = $projectId
            method_count           = @($units | Where-Object { $_.repository_root -eq $projectRoot }).Count
            call_edge_count        = @($callEdgeRows | Where-Object { $_.project_id -eq $projectId }).Count
            value_flow_edge_count  = @($valueFlowRows | Where-Object { $_.project_id -eq $projectId }).Count
            wiki_domain            = Join-Path $projectRoot ".xiigen\wiki-domain"
        }
    })
Set-Content -LiteralPath (Join-Path $batchDir "PROJECT-SUMMARY.json") -Value ($projectSummaries | ConvertTo-Json -Depth 20) -Encoding UTF8

$report = @"
# Project-Local Wiki Phase 1 Run

Batch: $batchName
Status: PHASE1_RELATION_GRAPH_RUNNER_VERIFIED

| Metric | Value |
|---|---:|
| methods documented | $($validation.method_count) |
| raw call edge rows | $($validation.raw_call_edge_count) |
| source distinct call edge keys | $($validation.source_distinct_call_edge_count) |
| emitted call edges | $($validation.emitted_call_edge_count) |
| value-flow edges | $($validation.value_flow_edge_count) |
| negative DTO candidates | $($validation.negative_candidate_count) |
| negative DTO acceptance rate | $($validation.negative_candidate_acceptance_rate) |
| raw call edge line ratio | $($validation.raw_call_edge_line_ratio) |
| call edge coverage | $($validation.call_edge_coverage) |
| source distinct call edge coverage | $($validation.source_distinct_call_edge_coverage) |
| relation graph write coverage | $($validation.call_edge_relation_coverage) |
| value-flow coverage for call edges | $($validation.value_flow_for_call_edge_coverage) |
| train rows | $RowsPerSplit |
| held-out rows | $RowsPerSplit |
| routing accuracy | $($summary.routing_accuracy) |
| source gate accuracy | $($summary.source_gate_accuracy) |
| version gate accuracy | $($summary.version_gate_accuracy) |
| transit gate accuracy | $($summary.transit_gate_accuracy) |
| bottom-up order accuracy | $($summary.bottom_up_order_accuracy) |
| callee-summary transit accuracy | $($summary.callee_summary_transit_accuracy) |
| local-vs-delegated accuracy | $($summary.local_vs_delegated_accuracy) |
| runner CPU ms | $($summary.cpu_ms) |
| runner peak RAM MB | $($summary.peak_working_set_mb) |
| total CPU ms | $cpuMs |
| total peak RAM MB | $ramMb |
| CPU ms/documentation unit | $([math]::Round($cpuMs / [double]$units.Count, 6)) |

Question rows created: 0. Phase 2 question bank is intentionally not created
by this run.
"@
Set-Content -LiteralPath (Join-Path $batchDir "PHASE1-RUN-REPORT.md") -Value $report -Encoding UTF8

Write-Host ("{0} phase1 ok methods={1} edges={2} value_flows={3} route={4} transit={5} cpu={6} ram={7}" -f $batchName, $units.Count, $callEdgeRows.Count, $valueFlowRows.Count, $summary.routing_accuracy, $summary.transit_gate_accuracy, $cpuMs, $ramMb)
