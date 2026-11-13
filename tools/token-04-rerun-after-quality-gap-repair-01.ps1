param(
    [string]$ScenarioPackPath = "artifacts/extension-runtime-token-quality-learnability/scenario-pack-token-quality-v1.jsonl",
    [string]$OutputDir = "artifacts/extension-runtime-token-quality-learnability",
    [string]$RunId = "TOKEN-04-rerun-after-quality-gap-repair-01",
    [string]$CoreRoot = ".",
    [string]$UserAdaptiveAssetRoot = "",
    [string]$PhaseId = "TOKEN-04-RERUN-AFTER-QUALITY-GAP-REPAIR-01",
    [string]$OutputPrefix = "token-04-rerun-after-quality-gap-repair-01",
    [switch]$WriteState,
    [string]$StatePath = "docs/plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-EXECUTION-STATE.json"
)

$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([Parameter(Mandatory = $true)][string]$Path)
    if ([System.IO.Path]::IsPathRooted($Path)) {
        return [System.IO.Path]::GetFullPath($Path)
    }
    return [System.IO.Path]::GetFullPath((Join-Path (Get-Location).Path $Path))
}

function Write-Utf8NoBom {
    param(
        [Parameter(Mandatory = $true)][string]$Path,
        [Parameter(Mandatory = $true)][string]$Text
    )
    $parent = Split-Path -Parent $Path
    if ($parent -and -not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Force -Path $parent | Out-Null
    }
    $normalized = $Text.Replace("`r`n", "`n").Replace("`r", "`n")
    if (-not $normalized.EndsWith("`n")) {
        $normalized += "`n"
    }
    [System.IO.File]::WriteAllText($Path, $normalized, [System.Text.UTF8Encoding]::new($false))
}

function ConvertTo-PrettyJson {
    param([Parameter(Mandatory = $true)]$Value)
    return $Value | ConvertTo-Json -Depth 100
}

function Read-Jsonl {
    param([Parameter(Mandatory = $true)][string]$Path)
    $rows = @()
    foreach ($line in [System.IO.File]::ReadLines($Path, [System.Text.Encoding]::UTF8)) {
        if ([string]::IsNullOrWhiteSpace($line)) { continue }
        $rows += ($line | ConvertFrom-Json)
    }
    return @($rows)
}

function Get-Sha256 {
    param([string]$Text)
    $bytes = [System.Text.Encoding]::UTF8.GetBytes($Text)
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        return "sha256:" + ([BitConverter]::ToString($sha.ComputeHash($bytes)) -replace "-", "").ToLowerInvariant()
    }
    finally {
        $sha.Dispose()
    }
}

function ConvertTo-CanonicalScoredJsonProjection {
    param([AllowNull()][string]$Text)

    # The scored projection hash is an evidence identifier. Indented JSON
    # newlines must not depend on the OS that generated or re-scored the row;
    # canonical CRLF preserves the existing TOKEN-04 hashes and makes Linux/WSL
    # and Windows generation agree.
    $normalized = if ($null -eq $Text) { "" } else { [string]$Text }
    $normalized = $normalized.Replace("`r`n", "`n").Replace("`r", "`n")
    return $normalized.Replace("`n", "`r`n")
}

function Get-TokenCount {
    param([string]$Text)
    $count = 0
    $inToken = $false
    foreach ($character in $Text.ToCharArray()) {
        if ([char]::IsLetterOrDigit($character) -or $character -eq "_" -or $character -eq "-") {
            if (-not $inToken) {
                $count++
                $inToken = $true
            }
            continue
        }
        $inToken = $false
        if (-not [char]::IsWhiteSpace($character)) {
            $count++
        }
    }
    return $count
}

$scenarioFullPath = Resolve-FullPath $ScenarioPackPath
$outputFullDir = Resolve-FullPath $OutputDir
$coreFullRoot = Resolve-FullPath $CoreRoot
$userAdaptiveFullRoot = if ([string]::IsNullOrWhiteSpace($UserAdaptiveAssetRoot)) { "" } else { Resolve-FullPath $UserAdaptiveAssetRoot }
$stateFullPath = Resolve-FullPath $StatePath
$captureRoot = Join-Path $outputFullDir "$OutputPrefix-fresh-api-capture"
$tracePath = Join-Path $outputFullDir "$OutputPrefix-fresh-traces.jsonl"
$pairPath = Join-Path $outputFullDir "$OutputPrefix-fresh-pair-results.jsonl"
$qualityInputPath = Join-Path $outputFullDir "$OutputPrefix-quality-scoring-inputs.jsonl"
$reportPath = Join-Path $outputFullDir "$OutputPrefix-capture-report.json"
$reportMdPath = Join-Path $outputFullDir "$OutputPrefix-capture-report.md"
$validationPath = Join-Path $outputFullDir "$OutputPrefix-capture.validation.json"

$scenarios = Read-Jsonl $scenarioFullPath
if ($scenarios.Count -ne 12) {
    throw "Expected 12 TOKEN-01 scenarios, got $($scenarios.Count)."
}

$tmpRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("xiigen-token04-rerun-" + [Guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Force -Path $tmpRoot | Out-Null
$projectPath = Join-Path $tmpRoot "Token04RerunInvoker.csproj"
$programPath = Join-Path $tmpRoot "Program.cs"
$resultPath = Join-Path $tmpRoot "runner-output.json"
$smartSkillsProject = Join-Path $coreFullRoot "43.SmartSkillsDomain\43.SmartSkillsDomain.csproj"

try {
    @"
<Project Sdk="Microsoft.NET.Sdk">
  <ItemGroup>
    <ProjectReference Include="$smartSkillsProject" />
  </ItemGroup>
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
"@ | Set-Content -LiteralPath $projectPath -Encoding UTF8

    @'
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using SmartSkillsDomain;

if (args.Length < 7)
{
    Console.Error.WriteLine("Usage: <scenario-pack-jsonl> <capture-root> <output-json> <core-root> <user-adaptive-root> <phase-id> <run-id>");
    return 2;
}

var scenarioPath = Path.GetFullPath(args[0]);
var captureRoot = Path.GetFullPath(args[1]);
var outputPath = Path.GetFullPath(args[2]);
var coreRoot = Path.GetFullPath(args[3]);
var userAdaptiveRoot = string.IsNullOrWhiteSpace(args[4]) ? "" : Path.GetFullPath(args[4]);
var phaseId = args[5];
var runId = args[6];
Directory.CreateDirectory(captureRoot);
Directory.SetCurrentDirectory(coreRoot);

var jsonOptions = new JsonSerializerOptions
{
    WriteIndented = true,
    PropertyNameCaseInsensitive = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

var methods = new[] { "prompt-to-task", "orchestration-plan", "skill-selection" };
var availableSkills = new[]
{
    "architecture-audit",
    "authority-governance",
    "backup-export-import-share",
    "checkpoint-safety",
    "code-reference-grounding",
    "debug-session",
    "documentation-sync",
    "dpo-triple-format",
    "extension-runtime",
    "final-readiness-review",
    "held-out-ablation",
    "model-lifecycle",
    "plan-review",
    "planning-dpo-authoring",
    "privacy-redaction",
    "quality-review",
    "runtime-QA",
    "shortcut-audit",
    "skill-selection-invariance",
    "source-license-review",
    "test-integrity",
    "token-measurement",
    "trainable-unit-design"
};

var scenarios = File.ReadLines(scenarioPath)
    .Where(line => !string.IsNullOrWhiteSpace(line))
    .Select(line => JsonNode.Parse(line)!.AsObject())
    .ToArray();
var rows = new List<JsonObject>();

foreach (var scenario in scenarios)
{
    var scenarioId = scenario["scenario_id"]!.GetValue<string>();
    var prompt = scenario["prompt_text"]!.GetValue<string>();
    var scenarioMethods = scenario["methods_to_run"]!.AsArray().Select(item => item!.GetValue<string>()).ToArray();
    var hosts = scenario["hosts"]!.AsArray().Select(item => item!.GetValue<string>()).ToArray();
    var modes = scenario["modes"]!.AsArray().Select(item => item!.GetValue<string>()).ToArray();
    var expectedOutputs = scenario["expected_outputs"]?.DeepClone();
    var scoringRubric = scenario["scoring_rubric"]?.DeepClone();

    foreach (var host in hosts)
    foreach (var method in scenarioMethods)
    foreach (var mode in modes)
    {
        var pairId = $"{scenarioId}|{host}|{method}|debug-false";
        var correlationId = $"token04-rerun-{scenarioId}-{host}-{method}-{mode}";
        var requestedSkills = BuildRequestedSkillsFromTaskSemantics(method, prompt, availableSkills);
        var request = new XiigenExtensionRequest(
            HostName: host,
            HostType: host.ToLowerInvariant(),
            Method: method,
            UserPromptOrTaskText: prompt,
            HostFacts: new Dictionary<string, string>
            {
                ["workspace"] = coreRoot,
                ["user_language"] = "en",
                ["host"] = host.ToLowerInvariant(),
                ["source"] = "token-04-rerun-after-quality-gap-repair-01"
            },
            DebugEnabled: false,
            RequestedSkills: requestedSkills,
            AvailableSkills: availableSkills,
            CheckSetInputs: new Dictionary<string, string>
            {
                ["semantic_quality_claimed"] = "false",
                ["deterministic_shortcut"] = "forbidden"
            },
            Provenance: new Dictionary<string, string>
            {
                ["scenario_id"] = scenarioId,
                ["source_prompt_entry_id"] = $"token04-rerun:{scenarioId}",
                ["mode"] = mode
            },
            CorrelationId: correlationId);

        var service = mode == "adaptive_common_plus_user_model" && !string.IsNullOrWhiteSpace(userAdaptiveRoot)
            ? new XiigenExtensionApiService(coreRoot, userAdaptiveRoot)
            : new XiigenExtensionApiService(coreRoot);
        var result = service.ProcessHostExtensionRequest(request);
        var response = result.IsSuccess && result.Value is not null
            ? result.Value
            : new XiigenExtensionResponse(
                Success: false,
                Status: "core_boundary_error",
                ChatDisplayText: "Xiigen core boundary failed.",
                SelectedSkills: Array.Empty<string>(),
                CheckSetResults: new Dictionary<string, string>
                {
                    ["core_boundary_executed"] = "false",
                    ["semantic_quality_claimed"] = "false",
                    ["runtime_extension_claimed"] = "false"
                },
                StructuredTaskRefs: Array.Empty<string>(),
                ScenarioRefs: Array.Empty<string>(),
                Warnings: new[] { "semantic_quality_claimed=false" },
                Errors: new[] { result.ErrorCode, result.ErrorMessage },
                DebugTrace: null);

        var rowDir = Path.Combine(captureRoot, scenarioId, host, method, mode);
        Directory.CreateDirectory(rowDir);
        var requestPath = Path.Combine(rowDir, "request.audit.json");
        var responsePath = Path.Combine(rowDir, "response.json");
        var tracePath = Path.Combine(rowDir, "trace.json");
        var promptHash = GetSha256(prompt);
        var promptTokenEstimate = GetTokenCount(prompt);
        var outputProjection = CanonicalizeScoredJsonProjectionNewlines(JsonSerializer.Serialize(new
        {
            response.ChatDisplayText,
            response.SelectedSkills,
            response.StructuredTaskRefs,
            response.ScenarioRefs,
            response.Warnings,
            PromptToTaskStatus = response.CheckSetResults.TryGetValue("prompt_to_task_status", out var pttStatus) ? pttStatus : "",
            OrchestrationReasoningStatus = response.CheckSetResults.TryGetValue("orchestration_reasoning_status", out var orStatus) ? orStatus : "",
            SkillSelectionRequiresSecondPass = response.CheckSetResults.TryGetValue("skill_selection_requires_second_pass", out var secondPass) ? secondPass : "",
            SkillSelectionEvidence = response.CheckSetResults.TryGetValue("skill_selection_evidence", out var skillEvidence) ? skillEvidence : "",
            RequestedSkillsCount = response.CheckSetResults.TryGetValue("requested_skills_count", out var requestedSkillsCount) ? requestedSkillsCount : requestedSkills.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
            SelectedSkillsCount = response.CheckSetResults.TryGetValue("selected_skills_count", out var selectedSkillsCount) ? selectedSkillsCount : response.SelectedSkills.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            RequiredSkillSelectionAbsent = response.CheckSetResults.TryGetValue("required_skill_selection_absent", out var requiredSkillSelectionAbsent) ? requiredSkillSelectionAbsent : "false",
            SkillSelectionMethodRationale = response.CheckSetResults.TryGetValue("skill_selection_method_rationale", out var skillSelectionMethodRationale) ? skillSelectionMethodRationale : "",
            SemanticQualityClaimed = response.CheckSetResults.TryGetValue("semantic_quality_claimed", out var projectionSemantic) ? projectionSemantic : "false",
            RuntimeExtensionClaimed = response.CheckSetResults.TryGetValue("runtime_extension_claimed", out var projectionRuntime) ? projectionRuntime : "false"
        }, jsonOptions));
        File.WriteAllText(requestPath, JsonSerializer.Serialize(new
        {
            schema_version = "xiigen.extension_redacted_request_envelope.v1",
            privacy_gate = "raw_private_prompt_not_persisted",
            request.HostName,
            request.HostType,
            request.Method,
            UserPromptOrTaskText = "<redacted>",
            UserPromptHash = promptHash,
            UserPromptTokenEstimate = promptTokenEstimate,
            request.HostFacts,
            request.DebugEnabled,
            request.RequestedSkills,
            request.AvailableSkills,
            request.CheckSetInputs,
            request.Provenance,
            request.CorrelationId,
            request.TargetLocator
        }, jsonOptions) + Environment.NewLine);
        File.WriteAllText(responsePath, JsonSerializer.Serialize(response, jsonOptions) + Environment.NewLine);
        File.WriteAllText(tracePath, JsonSerializer.Serialize(new
        {
            schema_version = 1,
            phase_id = phaseId,
            run_id = runId,
            call_target = "SmartSkillsDomain.XiigenExtensionApiService.ProcessHostExtensionRequest",
            host,
            method,
            mode,
            scenario_id = scenarioId,
            correlation_id = correlationId,
            request_path = requestPath,
            response_path = responsePath,
            user_adaptive_asset_root_used = mode == "adaptive_common_plus_user_model" && !string.IsNullOrWhiteSpace(userAdaptiveRoot),
            response_success = response.Success,
            response_status = response.Status,
            semantic_quality_claimed = response.CheckSetResults.TryGetValue("semantic_quality_claimed", out var semantic) ? semantic : "false",
            runtime_extension_claimed = response.CheckSetResults.TryGetValue("runtime_extension_claimed", out var runtime) ? runtime : "false",
            user_adaptive_any_asset_loaded = response.CheckSetResults.TryGetValue("user_adaptive_any_asset_loaded", out var adaptiveLoaded) ? adaptiveLoaded : "false",
            external_ai_used = false,
            training_started = false,
            checkpoint_mutation_started = false,
            capture_scope = "fresh direct core API capture for TOKEN-04 rerun; not live host chat runtime"
        }, jsonOptions) + Environment.NewLine);

        rows.Add(new JsonObject
        {
            ["scenario_id"] = scenarioId,
            ["pair_id"] = pairId,
            ["host"] = host,
            ["mode"] = mode,
            ["method"] = method,
            ["prompt_hash"] = promptHash,
            ["prompt_tokens"] = promptTokenEstimate,
            ["redacted_prompt"] = "<redacted>",
            ["output"] = outputProjection,
            ["output_projection_scope"] = "safe_extension_api_output_projection_no_raw_prompt",
            ["request_path"] = requestPath,
            ["response_path"] = responsePath,
            ["trace_path"] = tracePath,
            ["requested_skills_count"] = requestedSkills.Length,
            ["selected_skills_count"] = response.SelectedSkills.Count,
            ["required_skill_selection_absent"] = response.CheckSetResults.TryGetValue("required_skill_selection_absent", out var rowRequiredSkillAbsent) ? rowRequiredSkillAbsent : "false",
            ["expected_outputs"] = expectedOutputs?.DeepClone(),
            ["scoring_rubric"] = scoringRubric?.DeepClone(),
            ["user_adaptive_asset_root_used"] = mode == "adaptive_common_plus_user_model" && !string.IsNullOrWhiteSpace(userAdaptiveRoot),
            ["user_adaptive_any_asset_loaded"] = response.CheckSetResults.TryGetValue("user_adaptive_any_asset_loaded", out var rowAdaptiveLoaded) ? rowAdaptiveLoaded : "false",
            ["adaptive_effect_classification"] = mode == "baseline_common_model_only"
                ? "baseline_common_model_only_no_adaptive_effect"
                : response.CheckSetResults.TryGetValue("user_adaptive_any_asset_loaded", out var loaded) && loaded == "true"
                    ? "adaptive_user_local_asset_path_loaded_effect_requires_pair_hash_diff"
                    : "adaptive_requested_but_user_local_model_unavailable_no_real_adaptive_effect"
        });
    }
}

static string[] BuildRequestedSkillsFromTaskSemantics(string method, string prompt, IReadOnlyList<string> availableSkills)
{
    var available = new HashSet<string>(availableSkills, StringComparer.Ordinal);
    var desired = new List<string>();
    var normalizedMethod = string.IsNullOrWhiteSpace(method) ? string.Empty : method.Trim().ToLowerInvariant();
    var normalizedPrompt = string.IsNullOrWhiteSpace(prompt) ? string.Empty : prompt.ToLowerInvariant();

    void Add(string skillId)
    {
        if (available.Contains(skillId) && !desired.Contains(skillId, StringComparer.Ordinal))
            desired.Add(skillId);
    }

    if (normalizedMethod == "prompt-to-task")
    {
        Add("plan-review");
        Add("privacy-redaction");
        Add("code-reference-grounding");
    }
    else if (normalizedMethod == "orchestration-plan")
    {
        Add("implementation-integrity");
        Add("test-integrity");
        Add("quality-review");
    }
    else if (normalizedMethod == "skill-selection")
    {
        Add("skill-selection-invariance");
        Add("shortcut-audit");
        Add("test-integrity");
    }

    if (normalizedPrompt.Contains("document") || normalizedPrompt.Contains("diagram") || normalizedPrompt.Contains("quote"))
    {
        Add("documentation-sync");
        Add("code-reference-grounding");
    }

    if (normalizedPrompt.Contains("pause") || normalizedPrompt.Contains("why") || normalizedPrompt.Contains("permission") || normalizedPrompt.Contains("approval"))
    {
        Add("authority-governance");
        Add("plan-review");
    }

    if (normalizedPrompt.Contains("runtime") || normalizedPrompt.Contains("debug") || normalizedPrompt.Contains("host") || normalizedPrompt.Contains("extension"))
    {
        Add("extension-runtime");
        Add("runtime-QA");
        Add("debug-session");
    }

    if (normalizedPrompt.Contains("skill") || normalizedPrompt.Contains("candidate") || normalizedPrompt.Contains("select"))
    {
        Add("skill-selection-invariance");
        Add("shortcut-audit");
    }

    if (normalizedPrompt.Contains("token"))
        Add("token-measurement");

    if (normalizedPrompt.Contains("checkpoint") || normalizedPrompt.Contains("model") || normalizedPrompt.Contains("backup") || normalizedPrompt.Contains("import"))
    {
        Add("model-lifecycle");
        Add("checkpoint-safety");
        Add("backup-export-import-share");
    }

    if (desired.Count == 0)
    {
        Add("plan-review");
        Add("quality-review");
    }

    return desired.Take(6).ToArray();
}

static string GetSha256(string value)
{
    var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
    using var sha = SHA256.Create();
    return "sha256:" + Convert.ToHexString(sha.ComputeHash(bytes)).ToLowerInvariant();
}

static string CanonicalizeScoredJsonProjectionNewlines(string value)
{
    // System.Text.Json's indented writer follows the current platform newline.
    // TOKEN-04 hashes the projection itself, so the generator stores a
    // canonical CRLF projection before any later PowerShell scoring step.
    var normalized = (value ?? string.Empty)
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace("\r", "\n", StringComparison.Ordinal);
    return normalized.Replace("\n", "\r\n", StringComparison.Ordinal);
}

static int GetTokenCount(string value)
{
    var count = 0;
    var inToken = false;
    foreach (var character in (value ?? string.Empty))
    {
        if (char.IsLetterOrDigit(character) || character == '_' || character == '-')
        {
            if (!inToken)
            {
                count++;
                inToken = true;
            }
            continue;
        }

        inToken = false;
        if (!char.IsWhiteSpace(character))
            count++;
    }

    return count;
}

File.WriteAllText(outputPath, JsonSerializer.Serialize(rows, jsonOptions) + Environment.NewLine);
return 0;
'@ | Set-Content -LiteralPath $programPath -Encoding UTF8

    & dotnet run --project $projectPath -- $scenarioFullPath $captureRoot $resultPath $coreFullRoot $userAdaptiveFullRoot $PhaseId $RunId
    if ($LASTEXITCODE -ne 0) {
        throw "Fresh TOKEN-04 rerun invoker failed with exit code $LASTEXITCODE."
    }

    $capturedRows = Get-Content -Raw -LiteralPath $resultPath -Encoding UTF8 | ConvertFrom-Json
}
finally {
    if (Test-Path -LiteralPath $tmpRoot) {
        Remove-Item -LiteralPath $tmpRoot -Recurse -Force
    }
}

if ($capturedRows.Count -ne 108) {
    throw "Expected 108 fresh captured rows, got $($capturedRows.Count)."
}

$traceRows = New-Object System.Collections.Generic.List[object]
$qualityRows = New-Object System.Collections.Generic.List[object]
$pairs = New-Object System.Collections.Generic.List[object]
$pairGroups = @{}

foreach ($row in $capturedRows) {
    $output = ConvertTo-CanonicalScoredJsonProjection ([string]$row.output)
    $promptTokens = [int]$row.prompt_tokens
    $outputTokens = Get-TokenCount $output
    $promptHash = [string]$row.prompt_hash
    $outputHash = Get-Sha256 $output
    $qualityRow = [ordered]@{
        schema_version = "xiigen.extension_token_04_rerun_quality_scoring_input.v1"
        run_id = $RunId
        scenario_id = [string]$row.scenario_id
        pair_id = [string]$row.pair_id
        host = [string]$row.host
        mode = [string]$row.mode
        method = [string]$row.method
        debug_enabled = $false
        prompt_hash = $promptHash
        output_hash = $outputHash
        tokenizer_name = "xiigen-heuristic-word-punctuation"
        tokenizer_version = "v1"
        tokenizer_accounting_type = "heuristic"
        prompt_tokens = $promptTokens
        output_tokens = $outputTokens
        total_tokens = $promptTokens + $outputTokens
        requested_skills_count = [int]$row.requested_skills_count
        selected_skills_count = [int]$row.selected_skills_count
        required_skill_selection_absent = [string]$row.required_skill_selection_absent
        redacted_prompt = "<redacted>"
        redacted_output = "<redacted>"
        raw_prompt_ref_private = ""
        host_runtime_request_ref = [string]$row.request_path
        raw_output_ref_private = [string]$row.response_path
        scoring_rubric = $row.scoring_rubric
        expected_outputs = $row.expected_outputs
        scoring_status = "PENDING_NOT_SCORED"
        quality_non_regression_passed = $false
        token_savings_claim_allowed = $false
        adaptive_effect_classification = [string]$row.adaptive_effect_classification
        user_adaptive_asset_root_used = [bool]$row.user_adaptive_asset_root_used
        user_adaptive_any_asset_loaded = [string]$row.user_adaptive_any_asset_loaded
        trace_ref = [string]$row.trace_path
        capture_scope = "fresh_direct_core_api_token_04_rerun_not_live_host_chat_runtime"
        stale_trace_rerun_invalid = $false
    }
    $qualityRows.Add($qualityRow)
    $traceRows.Add($qualityRow)

    if (-not $pairGroups.ContainsKey($qualityRow.pair_id)) {
        $pairGroups[$qualityRow.pair_id] = @{}
    }
    $pairGroups[$qualityRow.pair_id][$qualityRow.mode] = $qualityRow
}

foreach ($pairId in ($pairGroups.Keys | Sort-Object)) {
    $baseline = $pairGroups[$pairId]["baseline_common_model_only"]
    $adaptive = $pairGroups[$pairId]["adaptive_common_plus_user_model"]
    $hasBoth = $null -ne $baseline -and $null -ne $adaptive
    $tokenDelta = if ($hasBoth) { [int]$baseline.total_tokens - [int]$adaptive.total_tokens } else { 0 }
    $adaptiveEffectProven = $hasBoth -and
        [string]$adaptive.user_adaptive_any_asset_loaded -eq "true" -and
        [string]$adaptive.output_hash -ne [string]$baseline.output_hash
    $failureReasons = New-Object System.Collections.Generic.List[string]
    $failureReasons.Add("quality_not_scored_yet")
    if (-not $adaptiveEffectProven) {
        $failureReasons.Add("adaptive_effect_not_proven")
    }
    $failureReasons.Add("token_savings_claim_forbidden")
    $pairs.Add([ordered]@{
        schema_version = "xiigen.extension_token_04_rerun_pair_result.v1"
        run_id = $RunId
        scenario_id = if ($hasBoth) { $baseline.scenario_id } else { "" }
        pair_id = $pairId
        host = if ($hasBoth) { $baseline.host } else { "" }
        method = if ($hasBoth) { $baseline.method } else { "" }
        baseline_mode = "baseline_common_model_only"
        adaptive_mode = "adaptive_common_plus_user_model"
        baseline_total_tokens = if ($hasBoth) { $baseline.total_tokens } else { $null }
        adaptive_total_tokens = if ($hasBoth) { $adaptive.total_tokens } else { $null }
        token_delta = $tokenDelta
        token_reduction_observed = $tokenDelta -gt 0
        quality_non_regression_passed = $false
        no_deterministic_shortcut_gate = [ordered]@{ required = $true; passed = $false; evidence = "pending TOKEN-04 rerun scoring" }
        token_savings_claim_allowed = $false
        pairing_fields_preserved = $hasBoth
        adaptive_effect_proven = $adaptiveEffectProven
        failure_reasons = @($failureReasons)
        claim_scope = "fresh direct core API capture after QUALITY-GAP-REPAIR-01; no host runtime or semantic claim"
    })
}

Write-Utf8NoBom $tracePath (($traceRows | ForEach-Object { $_ | ConvertTo-Json -Depth 100 -Compress }) -join "`n")
Write-Utf8NoBom $qualityInputPath (($qualityRows | ForEach-Object { $_ | ConvertTo-Json -Depth 100 -Compress }) -join "`n")
Write-Utf8NoBom $pairPath (($pairs | ForEach-Object { $_ | ConvertTo-Json -Depth 100 -Compress }) -join "`n")

$previousReportPath = Join-Path $outputFullDir "token-04-quality-scoring-full-hosts-report.json"
$previousReport = if (Test-Path -LiteralPath $previousReportPath) {
    Get-Content -Raw -LiteralPath $previousReportPath -Encoding UTF8 | ConvertFrom-Json
} else {
    $null
}
$captureReport = [ordered]@{
    schema = "xiigen.extension_token_04_rerun_after_quality_gap_repair_01_capture_report.v1"
    phase_id = $phaseId
    run_id = $RunId
    base_commit = "not_queried_git_forbidden"
    stale_token_03_traces_reused = $false
    stale_trace_rerun_invalid = $false
    fresh_capture_scope = "direct XiigenExtensionApiService core API capture with staged user adaptive asset path where applicable; not full live host chat runtime"
    scenario_count = $scenarios.Count
    captured_rows = $qualityRows.Count
    pair_count = $pairs.Count
    adaptive_rows_with_user_asset_loaded = @($qualityRows | Where-Object { $_.mode -eq "adaptive_common_plus_user_model" -and $_.user_adaptive_any_asset_loaded -eq "true" }).Count
    adaptive_effect_pairs_in_fresh_capture = @($pairs | Where-Object { $_.adaptive_effect_proven }).Count
    hosts = @($qualityRows | ForEach-Object { $_.host } | Sort-Object -Unique)
    methods = @($qualityRows | ForEach-Object { $_.method } | Sort-Object -Unique)
    modes = @($qualityRows | ForEach-Object { $_.mode } | Sort-Object -Unique)
    previous_token_04_counts = if ($previousReport) {
        [ordered]@{
            row_count = $previousReport.row_count
            pair_count = $previousReport.pair_count
            quality_passed_rows = $previousReport.quality_passed_rows
            quality_gate_pairs = $previousReport.quality_gate_pairs
            adaptive_effect_proven_pairs = $previousReport.adaptive_effect_proven_pairs
            token_savings_claim_allowed_pairs = $previousReport.token_savings_claim_allowed_pairs
        }
    } else { $null }
    artifacts = [ordered]@{
        fresh_traces = $tracePath
        quality_scoring_inputs = $qualityInputPath
        pair_results = $pairPath
        capture_root = $captureRoot
    }
    claims = [ordered]@{
        token_savings_claim_allowed = $false
        quality_improvement_claim_allowed = $false
        semantic_quality_claimed = $false
        learnability_claimed = $false
        training_started = $false
        checkpoint_mutation_started = $false
        host_runtime_claimed = $false
    }
    next_action = "TOKEN-04-RERUN-SCORE-AFTER-FRESH-CAPTURE: score $OutputPrefix-quality-scoring-inputs.jsonl and compare against previous TOKEN-04 counts."
}
Write-Utf8NoBom $reportPath (ConvertTo-PrettyJson $captureReport)

$captureMd = @"
# TOKEN-04 Rerun After QUALITY-GAP-REPAIR-01 Capture

This capture did **not** reuse stale TOKEN-03 traces. It generated fresh direct `XiigenExtensionApiService` API outputs without querying git because this bounded work order forbids git commands.

Scope: direct core API behavior after the repair. This is not a full Claude/Cursor/Codex live chat runtime proof and does not mutate checkpoints or train models.

| Metric | Value |
|---|---:|
| Scenarios | $($scenarios.Count) |
| Fresh captured rows | $($qualityRows.Count) |
| Baseline/adaptive pairs | $($pairs.Count) |
| Adaptive rows with user asset loaded | $($captureReport.adaptive_rows_with_user_asset_loaded) |
| Adaptive-effect pairs in fresh capture | $($captureReport.adaptive_effect_pairs_in_fresh_capture) |

Claims remain closed: token savings, quality improvement, semantic quality, learnability, training, checkpoint mutation, and host runtime readiness are all `false`.
"@
Write-Utf8NoBom $reportMdPath $captureMd

$validation = [ordered]@{
    schema = "xiigen.extension_token_04_rerun_capture_validation.v1"
    phase_id = $phaseId
    expected_rows = 108
    actual_rows = $qualityRows.Count
    expected_pairs = 54
    actual_pairs = $pairs.Count
    row_count_pass = $qualityRows.Count -eq 108
    pair_count_pass = $pairs.Count -eq 54
    stale_token_03_traces_reused = $false
    adaptive_rows_with_user_asset_loaded = $captureReport.adaptive_rows_with_user_asset_loaded
    adaptive_effect_pairs_in_fresh_capture = $captureReport.adaptive_effect_pairs_in_fresh_capture
    token_savings_claim_allowed = $false
    semantic_quality_claimed = $false
    learnability_claimed = $false
}
Write-Utf8NoBom $validationPath (ConvertTo-PrettyJson $validation)

if ($WriteState) {
    $state = Get-Content -Raw -LiteralPath $stateFullPath -Encoding UTF8 | ConvertFrom-Json
    $state.updated_at_local = (Get-Date).ToString("o")
    $state.current_phase = $phaseId
    $state.phase_status = "FRESH_CORE_API_CAPTURE_WITH_ADAPTIVE_PATH_COMPLETE_SCORING_REQUIRED"
    $state.phase_scope = "Fresh direct core API capture with staged user adaptive asset path where applicable; not stale TOKEN-03 traces and not full live host chat runtime."
    $state.goal_reached = $false
    $state.stop_allowed = $false
    $state.continuation_required = $true
    $state.training_started = $false
    $state.checkpoint_mutation_started = $false
    $state | Add-Member -NotePropertyName "token_savings_claim_allowed" -NotePropertyValue $false -Force
    $state | Add-Member -NotePropertyName "quality_improvement_claim_allowed" -NotePropertyValue $false -Force
    $state | Add-Member -NotePropertyName "semantic_quality_claimed" -NotePropertyValue $false -Force
    $state | Add-Member -NotePropertyName "learnability_claimed" -NotePropertyValue $false -Force
    $state.next_action = $captureReport.next_action
    $state | Add-Member -NotePropertyName "token_04_rerun_after_quality_gap_repair_01_capture" -NotePropertyValue $captureReport -Force
    Write-Utf8NoBom $stateFullPath (ConvertTo-PrettyJson $state)
}

Write-Output (ConvertTo-PrettyJson $validation)
