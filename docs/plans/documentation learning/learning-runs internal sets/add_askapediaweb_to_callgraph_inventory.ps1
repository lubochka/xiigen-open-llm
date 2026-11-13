$ErrorActionPreference = "Stop"

$projectRoot = "C:\Projects\askapediaweb"
$runRoot = "C:\Users\lubao\Documents\xiigen\llm mvp\documentation learning\documentation-runs\dotnet-csharp-callgraph-extraction"
$methodPath = Join-Path $runRoot "extracted-method-inventory.jsonl"
$edgePath = Join-Path $runRoot "extracted-callgraph-edges.jsonl"
$summaryPath = Join-Path $runRoot "CALLGRAPH-EXTRACTION-SUMMARY.json"

if (!(Test-Path $projectRoot)) {
    throw "Missing project root: $projectRoot"
}
if (!(Test-Path $methodPath)) {
    throw "Missing method inventory: $methodPath"
}
if (!(Test-Path $edgePath)) {
    throw "Missing edge inventory: $edgePath"
}

function JsonLine($obj) {
    return ($obj | ConvertTo-Json -Depth 40 -Compress)
}

function Get-ClassNameBefore([string]$text, [int]$index) {
    $before = $text.Substring(0, [Math]::Min($index, $text.Length))
    $matches = [regex]::Matches($before, "\b(class|struct)\s+([A-Za-z_][A-Za-z0-9_]*)")
    if ($matches.Count -eq 0) {
        return "Global"
    }
    return $matches[$matches.Count - 1].Groups[2].Value
}

function Find-MatchingBrace([string]$text, [int]$openIndex) {
    $depth = 0
    $inString = $false
    $inChar = $false
    $escape = $false
    for ($i = $openIndex; $i -lt $text.Length; $i++) {
        $ch = $text[$i]
        if ($escape) {
            $escape = $false
            continue
        }
        if ($ch -eq "\") {
            $escape = $true
            continue
        }
        if (!$inChar -and $ch -eq '"') {
            $inString = !$inString
            continue
        }
        if (!$inString -and $ch -eq "'") {
            $inChar = !$inChar
            continue
        }
        if ($inString -or $inChar) {
            continue
        }
        if ($ch -eq "{") {
            $depth++
        }
        elseif ($ch -eq "}") {
            $depth--
            if ($depth -eq 0) {
                return $i
            }
        }
    }
    return -1
}

function Get-TargetFrameworkForFile([string]$file) {
    $dir = Split-Path -Parent $file
    while ($dir -and $dir.StartsWith($projectRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
        $csproj = Get-ChildItem -LiteralPath $dir -Filter "*.csproj" -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($csproj) {
            $raw = Get-Content -LiteralPath $csproj.FullName -Raw
            $match = [regex]::Match($raw, "<TargetFrameworkVersion>\s*([^<]+)\s*</TargetFrameworkVersion>|<TargetFramework>\s*([^<]+)\s*</TargetFramework>")
            if ($match.Success) {
                $value = if ($match.Groups[1].Success) { $match.Groups[1].Value.Trim() } else { $match.Groups[2].Value.Trim() }
                return $value
            }
        }
        $parent = Split-Path -Parent $dir
        if ($parent -eq $dir) { break }
        $dir = $parent
    }
    return "v4.6.1"
}

function Get-RelativePath([string]$file) {
    return $file.Substring($projectRoot.Length).TrimStart("\", "/")
}

$methodRegex = [regex]'(?ms)^\s*(?:\[.*?\]\s*)*(?:public|private|protected|internal)\s+(?:static\s+|virtual\s+|override\s+|async\s+|sealed\s+|partial\s+)*[A-Za-z_][A-Za-z0-9_<>,\[\]\.\?\s]*\s+([A-Za-z_][A-Za-z0-9_]*)\s*\([^;{}]*\)\s*(?:where\s+[^{]+)?\{'
$files = @(Get-ChildItem -LiteralPath $projectRoot -Recurse -Filter "*.cs" -File |
    Where-Object {
        $_.FullName -notmatch "\\bin\\|\\obj\\|\\packages\\" -and
        $_.Name -notmatch "\.Designer\.cs$|\.g\.cs$|AssemblyInfo\.cs$"
    })

$newMethods = New-Object System.Collections.ArrayList
foreach ($file in $files) {
    $text = Get-Content -LiteralPath $file.FullName -Raw
    foreach ($match in $methodRegex.Matches($text)) {
        $openIndex = $match.Index + $match.Value.LastIndexOf("{")
        $closeIndex = Find-MatchingBrace $text $openIndex
        if ($closeIndex -lt 0) {
            continue
        }
        $body = $text.Substring($openIndex, $closeIndex - $openIndex + 1)
        if ([string]::IsNullOrWhiteSpace($body) -or $body.Length -lt 20) {
            continue
        }
        $className = Get-ClassNameBefore $text $match.Index
        $methodName = $match.Groups[1].Value
        $relative = Get-RelativePath $file.FullName
        $framework = Get-TargetFrameworkForFile $file.FullName
        $symbol = "askapediaweb::$className.$methodName"
        $startLine = 1 + ($text.Substring(0, $match.Index).Split("`n").Count - 1)
        [void]$newMethods.Add([ordered]@{
                project          = $projectRoot
                file             = $file.FullName
                relative         = $relative
                class            = $className
                method           = $methodName
                symbol           = $symbol
                body             = $body
                start_line       = $startLine
                target_framework = $framework
                runtime_family   = "NET_FRAMEWORK"
                project_style    = "legacy_non_sdk"
                web_framework    = "ASP.NET MVC/WebApi/WebForms"
            })
    }
}

$methodNames = @($newMethods | ForEach-Object { [string]$_["method"] } | Sort-Object -Unique)
$excludedNames = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
foreach ($name in @("ToString", "Equals", "GetHashCode", "Where", "Select", "FirstOrDefault", "ToList", "Add", "Remove", "SaveChanges", "Count", "Any", "OrderBy", "ThenBy", "Contains", "StartsWith", "EndsWith")) {
    [void]$excludedNames.Add($name)
}

$methodsByName = @{}
foreach ($m in $newMethods) {
    $name = [string]$m["method"]
    if (!$methodsByName.ContainsKey($name)) {
        $methodsByName[$name] = New-Object System.Collections.ArrayList
    }
    [void]$methodsByName[$name].Add($m)
}

$newEdges = New-Object System.Collections.ArrayList
foreach ($caller in $newMethods) {
    $body = [string]$caller["body"]
    foreach ($name in $methodNames) {
        if ($excludedNames.Contains($name)) {
            continue
        }
        if ($name -eq [string]$caller["method"]) {
            continue
        }
        if ($body -match ("\b" + [regex]::Escape($name) + "\s*\(")) {
            foreach ($callee in @($methodsByName[$name])) {
                [void]$newEdges.Add([ordered]@{
                        project       = $projectRoot
                        caller_file   = [string]$caller["relative"]
                        caller_symbol = [string]$caller["symbol"]
                        caller_method = [string]$caller["method"]
                        caller_body   = [string]$caller["body"]
                        callee_file   = [string]$callee["relative"]
                        callee_symbol = [string]$callee["symbol"]
                        callee_method = [string]$callee["method"]
                        callee_body   = [string]$callee["body"]
                        call_kind     = "askapediaweb_extracted_direct_method_name_match"
                    })
            }
        }
    }
}

$existingMethods = @(Get-Content -LiteralPath $methodPath | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json } |
    Where-Object { [string]$_.project -ne $projectRoot })
$existingEdges = @(Get-Content -LiteralPath $edgePath | Where-Object { $_.Trim().Length -gt 0 } | ForEach-Object { $_ | ConvertFrom-Json } |
    Where-Object { [string]$_.project -ne $projectRoot })

Set-Content -LiteralPath $methodPath -Value (@($existingMethods) + @($newMethods) | ForEach-Object { JsonLine $_ }) -Encoding UTF8
Set-Content -LiteralPath $edgePath -Value (@($existingEdges) + @($newEdges) | ForEach-Object { JsonLine $_ }) -Encoding UTF8

$summary = [ordered]@{
    method_count    = $existingMethods.Count + $newMethods.Count
    edge_count      = $existingEdges.Count + $newEdges.Count
    projects        = [object[]]@(
        "C:\Projects\wizGameserver",
        "C:\Projects\GenieDynamicServer",
        "C:\Projects\geniesoftGameMVPServer",
        "C:\Projects\genietasks",
        "C:\Projects\askapediaweb"
    )
    generated_at    = (Get-Date).ToString("o")
    extraction_note = "Regex/direct-name callgraph with common .NET/Object/LINQ/API names excluded before held-out generation. askapediaweb added as fifth local validation-only legacy .NET Framework surface."
}
Set-Content -LiteralPath $summaryPath -Value ($summary | ConvertTo-Json -Depth 20) -Encoding UTF8

Write-Host ("askapediaweb added methods={0} edges={1} total_methods={2} total_edges={3}" -f $newMethods.Count, $newEdges.Count, $summary.method_count, $summary.edge_count)
