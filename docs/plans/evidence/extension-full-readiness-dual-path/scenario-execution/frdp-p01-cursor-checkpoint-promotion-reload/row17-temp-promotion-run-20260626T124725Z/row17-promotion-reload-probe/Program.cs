using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

var parsedForCatch = ParseArgs(args);
var runFolderForCatch = parsedForCatch.TryGetValue("--run-folder", out var catchRunFolder)
    ? Path.GetFullPath(catchRunFolder)
    : Directory.GetCurrentDirectory();

try
{
    var runFolder = Path.GetFullPath(Required(parsedForCatch, "--run-folder"));
    var sourceRoot = Path.GetFullPath(Required(parsedForCatch, "--source-root"));
    var harnessDir = Path.GetFullPath(Required(parsedForCatch, "--harness-dir"));
    var commandRun = Required(parsedForCatch, "--command-run");
    var logPath = parsedForCatch.TryGetValue("--log-path", out var parsedLogPath)
        ? Path.GetFullPath(parsedLogPath)
        : "";
    var expectedCheckpointSha256 = Required(parsedForCatch, "--expected-checkpoint-sha256").ToLowerInvariant();
    var expectedManifestSha256 = Required(parsedForCatch, "--expected-manifest-sha256").ToLowerInvariant();

    EnsureUnderRoot(harnessDir, runFolder, "harness-dir");
    if (!string.IsNullOrWhiteSpace(logPath))
        EnsureUnderRoot(logPath, runFolder, "log-path");
    Directory.CreateDirectory(runFolder);

    var tempPromotionRoot = Path.Combine(runFolder, "temp-promotion-area");
    var candidateRoot = Path.Combine(tempPromotionRoot, "candidate");
    var activeDir = Path.Combine(tempPromotionRoot, "active");
    var logsDir = Path.Combine(runFolder, "logs");
    Directory.CreateDirectory(activeDir);
    Directory.CreateDirectory(logsDir);

    var sourceCheckpointPath = Path.Combine(sourceRoot, "skill-selection-checkpoint.json");
    var sourceManifestPath = Path.Combine(sourceRoot, "skill-selection-manifest.json");
    var candidateCheckpointPath = Path.Combine(candidateRoot, "skill-selection-checkpoint.json");
    var candidateManifestPath = Path.Combine(candidateRoot, "skill-selection-manifest.json");
    var activeManifestPath = Path.Combine(activeDir, "temporary-active-manifest.json");
    var baselineManifestPath = Path.Combine(activeDir, "temporary-active-manifest.baseline.json");
    var rollbackEvidencePath = Path.Combine(runFolder, "row17-temp-rollback-evidence.json");
    var resultJsonPath = Path.Combine(runFolder, "row17-promotion-reload-execution-packet.json");
    var resultMdPath = Path.Combine(runFolder, "row17-promotion-reload-execution-packet-ru.md");

    EnsureUnderRoot(candidateRoot, runFolder, "candidate-root");
    EnsureUnderRoot(activeDir, runFolder, "active-dir");
    EnsureUnderRoot(candidateCheckpointPath, runFolder, "candidate-checkpoint");
    EnsureUnderRoot(candidateManifestPath, runFolder, "candidate-manifest");
    EnsureUnderRoot(activeManifestPath, runFolder, "active-manifest");
    EnsureUnderRoot(baselineManifestPath, runFolder, "baseline-manifest");
    EnsureUnderRoot(rollbackEvidencePath, runFolder, "rollback-evidence");
    EnsureUnderRoot(resultJsonPath, runFolder, "result-json");
    EnsureUnderRoot(resultMdPath, runFolder, "result-md");

    RequireFile(sourceCheckpointPath, "source checkpoint");
    RequireFile(sourceManifestPath, "source manifest");
    RequireFile(candidateCheckpointPath, "temp candidate checkpoint");
    RequireFile(candidateManifestPath, "temp candidate manifest");

    var sourceCheckpointBefore = Sha256(sourceCheckpointPath);
    var sourceManifestBefore = Sha256(sourceManifestPath);
    var candidateCheckpointBefore = Sha256(candidateCheckpointPath);
    var candidateManifestBefore = Sha256(candidateManifestPath);

    var baselineLocator = new
    {
        schema = "xiigen.row17.temp_active_locator.v1",
        status = "baseline_no_active_candidate",
        tempOnly = true,
        createdAtUtc = DateTimeOffset.UtcNow,
        runFolder,
        activeCandidate = (object?)null,
        nonClaims = NonClaims()
    };
    await WriteJsonAsync(activeManifestPath, baselineLocator);
    File.Copy(activeManifestPath, baselineManifestPath, overwrite: true);
    var activeLocatorBeforeSwitchSha256 = Sha256(activeManifestPath);
    var baselineLocatorSha256 = Sha256(baselineManifestPath);

    var activeLocator = new
    {
        schema = "xiigen.row17.temp_active_locator.v1",
        status = "temp_active_candidate",
        tempOnly = true,
        switchedAtUtc = DateTimeOffset.UtcNow,
        runFolder,
        candidate = new
        {
            manifestRelativePath = ToPortableRelativePath(activeDir, candidateManifestPath),
            checkpointRelativePath = ToPortableRelativePath(activeDir, candidateCheckpointPath),
            expectedCheckpointSha256,
            expectedManifestSha256,
            source = "Row14 staged-export copied into Row17 temp promotion area"
        },
        nonClaims = NonClaims()
    };
    await WriteJsonAsync(activeManifestPath, activeLocator);
    var activeLocatorAfterSwitchSha256 = Sha256(activeManifestPath);

    using var activeDoc = JsonDocument.Parse(await File.ReadAllTextAsync(activeManifestPath, Encoding.UTF8));
    var candidateElement = activeDoc.RootElement.GetProperty("candidate");
    var manifestRelativePath = RequiredProperty(candidateElement, "manifestRelativePath");
    var checkpointRelativePath = RequiredProperty(candidateElement, "checkpointRelativePath");
    var resolvedManifestPath = Path.GetFullPath(Path.Combine(activeDir, manifestRelativePath));
    var resolvedCheckpointPath = Path.GetFullPath(Path.Combine(activeDir, checkpointRelativePath));
    EnsureUnderRoot(resolvedManifestPath, runFolder, "resolved-manifest");
    EnsureUnderRoot(resolvedCheckpointPath, runFolder, "resolved-checkpoint");
    if (!string.Equals(resolvedManifestPath, Path.GetFullPath(candidateManifestPath), StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Temp active locator did not resolve to copied candidate manifest.");
    if (!string.Equals(resolvedCheckpointPath, Path.GetFullPath(candidateCheckpointPath), StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Temp active locator did not resolve to copied candidate checkpoint.");

    var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    var manifest = JsonSerializer.Deserialize<SkillSelectionCheckpointManifest>(
        await File.ReadAllTextAsync(resolvedManifestPath, Encoding.UTF8),
        jsonOptions) ?? throw new InvalidOperationException("Manifest deserialized null.");

    var export = new LearningMachineExport(
        manifest.MachineId,
        manifest.DomainTag,
        manifest.Version,
        resolvedCheckpointPath,
        resolvedManifestPath,
        manifest.AccuracyAtExport,
        manifest.ExportedAtUtc);

    var importedMachine = new SkillSelectionMachine();
    var importResult = await importedMachine.ImportAsync(export);
    if (!importResult.IsSuccess)
        throw new InvalidOperationException($"ImportAsync failed: {importResult.ErrorCode} {importResult.ErrorMessage}");

    var loadResult = await SkillSelectionMachine.LoadCheckpointAsync(resolvedCheckpointPath);
    if (!loadResult.IsSuccess || loadResult.Value is null)
        throw new InvalidOperationException($"LoadCheckpointAsync failed: {loadResult.ErrorCode} {loadResult.ErrorMessage}");

    var request = new SkillSelectionRequest(
        Prompt: string.Join(' ', new[]
        {
            "clientadaptivelearningloop",
            "adaptive learning loop",
            "target skill client adaptive learning loop",
        }),
        ProjectRoot: @"C:\Projects\llm_mvp_core",
        AvailableSkillIds:
        [
            "claim-verification",
            "client-adaptive-learning-loop",
            "e2e-test-matrix",
            "self-verification"
        ],
        HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["machineTarget"] = "SkillSelectionMachine",
            ["phase"] = "Row17 temp-only checkpoint promotion reload",
            ["rawJsonlAsPrompt"] = "false",
            ["rowId"] = "Row17",
            ["targetSkillId"] = "client-adaptive-learning-loop",
            ["targetTrainingAdapterAlias"] = "client-adaptive-learning-loop"
        },
        UserPromptSourceEntryId: "row14-2-client-adaptive-learning-loop-pair-001");

    var importedDecision = importedMachine.DecideFromTrainedState(request);
    if (!importedDecision.IsSuccess || importedDecision.Value is null)
        throw new InvalidOperationException($"Imported machine probe failed: {importedDecision.ErrorCode} {importedDecision.ErrorMessage}");

    var loadedDecision = loadResult.Value.Machine.DecideFromTrainedState(request);
    if (!loadedDecision.IsSuccess || loadedDecision.Value is null)
        throw new InvalidOperationException($"Loaded machine probe failed: {loadedDecision.ErrorCode} {loadedDecision.ErrorMessage}");

    var candidateCheckpointAfterReload = Sha256(candidateCheckpointPath);
    var candidateManifestAfterReload = Sha256(candidateManifestPath);
    var sourceCheckpointAfterReload = Sha256(sourceCheckpointPath);
    var sourceManifestAfterReload = Sha256(sourceManifestPath);

    File.Copy(baselineManifestPath, activeManifestPath, overwrite: true);
    var activeLocatorAfterRollbackSha256 = Sha256(activeManifestPath);
    var candidateCheckpointAfterRollback = Sha256(candidateCheckpointPath);
    var candidateManifestAfterRollback = Sha256(candidateManifestPath);
    var sourceCheckpointAfterRollback = Sha256(sourceCheckpointPath);
    var sourceManifestAfterRollback = Sha256(sourceManifestPath);

    using var rollbackDoc = JsonDocument.Parse(await File.ReadAllTextAsync(activeManifestPath, Encoding.UTF8));
    var rollbackStatus = rollbackDoc.RootElement.GetProperty("status").GetString() ?? "";
    var rollbackRestoredBaseline = string.Equals(rollbackStatus, "baseline_no_active_candidate", StringComparison.Ordinal)
        && activeLocatorAfterRollbackSha256 == baselineLocatorSha256;

    var expectedSkill = "client-adaptive-learning-loop";
    var selectedSkill = loadedDecision.Value.SelectedSkillIds.FirstOrDefault() ?? "";
    var importedSelectedSkill = importedDecision.Value.SelectedSkillIds.FirstOrDefault() ?? "";
    var hashChecksPass =
        sourceCheckpointBefore == expectedCheckpointSha256
        && sourceManifestBefore == expectedManifestSha256
        && candidateCheckpointBefore == expectedCheckpointSha256
        && candidateManifestBefore == expectedManifestSha256
        && candidateCheckpointAfterReload == expectedCheckpointSha256
        && candidateManifestAfterReload == expectedManifestSha256
        && candidateCheckpointAfterRollback == expectedCheckpointSha256
        && candidateManifestAfterRollback == expectedManifestSha256
        && sourceCheckpointAfterReload == sourceCheckpointBefore
        && sourceManifestAfterReload == sourceManifestBefore
        && sourceCheckpointAfterRollback == sourceCheckpointBefore
        && sourceManifestAfterRollback == sourceManifestBefore;
    var reloadProbePass =
        importResult.Value
        && string.Equals(manifest.MachineId, SkillSelectionMachine.MachineId, StringComparison.Ordinal)
        && string.Equals(manifest.DomainTag, SkillSelectionMachine.DomainTag, StringComparison.Ordinal)
        && string.Equals(manifest.Version, SkillSelectionMachine.Version, StringComparison.Ordinal)
        && loadResult.Value.Checkpoint.TrainedPairIds.Count == 10
        && loadedDecision.Value.SelectedSkillIds.Contains(expectedSkill, StringComparer.Ordinal)
        && importedDecision.Value.SelectedSkillIds.Contains(expectedSkill, StringComparer.Ordinal);
    var pass = hashChecksPass && reloadProbePass && rollbackRestoredBaseline;
    var status = pass
        ? "ROW17_PROMOTION_RELOAD_EXECUTION_PASS_REVIEW_REQUIRED"
        : "ROW17_PROMOTION_RELOAD_EXECUTION_NEEDS_FIX";

    var rollbackEvidence = new
    {
        schema = "xiigen.row17.temp_rollback_evidence.v1",
        status = rollbackRestoredBaseline ? "ROLLBACK_PASS" : "ROLLBACK_FAIL",
        tempOnly = true,
        activeManifestPath,
        baselineManifestPath,
        activeLocatorBeforeSwitchSha256,
        activeLocatorAfterSwitchSha256,
        activeLocatorAfterRollbackSha256,
        baselineLocatorSha256,
        rollbackRestoredBaseline,
        productionLocatorMutation = false,
        realActiveLocatorMutation = false
    };
    await WriteJsonAsync(rollbackEvidencePath, rollbackEvidence);

    var result = new
    {
        schema = "xiigen.row17.promotion_reload_execution_packet.v1",
        status,
        pass,
        executedAtUtc = DateTimeOffset.UtcNow,
        subagentId = "bounded-sub-agent-current-session",
        activeLubaInstruction = "continue via sub-agents; no parent execution; no stop gates; human-readable status required",
        scenario = new
        {
            row = 17,
            scenarioId = "frdp-p01-cursor-checkpoint-promotion-reload",
            row17Accepted = false,
            productReadinessFullPass = false,
            fullJoinedPass = false
        },
        commandRun,
        commandLineObservedByHarness = Environment.CommandLine,
        runFolder,
        filesCreated = new[]
        {
            candidateCheckpointPath,
            candidateManifestPath,
            activeManifestPath,
            baselineManifestPath,
            rollbackEvidencePath,
            resultJsonPath,
            resultMdPath,
            Path.Combine(harnessDir, "Row17PromotionReloadProbe.csproj"),
            Path.Combine(harnessDir, "Program.cs"),
            Path.Combine(harnessDir, "Directory.Build.props"),
            logPath
        },
        source = new
        {
            sourceRoot,
            sourceCheckpointPath,
            sourceManifestPath,
            sourceCheckpointBefore,
            sourceManifestBefore,
            sourceCheckpointAfterReload,
            sourceManifestAfterReload,
            sourceCheckpointAfterRollback,
            sourceManifestAfterRollback,
            sourceUntouchedByHash = sourceCheckpointBefore == sourceCheckpointAfterRollback
                && sourceManifestBefore == sourceManifestAfterRollback
        },
        tempPromotion = new
        {
            tempPromotionRoot,
            candidateRoot,
            activeDir,
            activeManifestPath,
            candidateCheckpointPath,
            candidateManifestPath,
            resolvedCheckpointPath,
            resolvedManifestPath,
            resolvedFromTempActiveLocator = true,
            allResolvedCandidatePathsUnderRunFolder = true
        },
        hashes = new
        {
            expectedCheckpointSha256,
            expectedManifestSha256,
            candidateCheckpointBefore,
            candidateManifestBefore,
            candidateCheckpointAfterReload,
            candidateManifestAfterReload,
            candidateCheckpointAfterRollback,
            candidateManifestAfterRollback,
            hashChecksPass
        },
        tempLocator = new
        {
            activeLocatorBeforeSwitchSha256,
            activeLocatorAfterSwitchSha256,
            activeLocatorAfterRollbackSha256,
            baselineLocatorSha256,
            rollbackRestoredBaseline,
            tempOnly = true
        },
        reloadProbe = new
        {
            importAsyncSucceeded = importResult.Value,
            loadCheckpointAsyncSucceeded = true,
            importPath = "SkillSelectionMachine.ImportAsync(temp LearningMachineExport from temporary active locator candidate)",
            loadPath = "SkillSelectionMachine.LoadCheckpointAsync(temp candidate checkpoint resolved from temporary active locator)",
            machineId = manifest.MachineId,
            domainTag = manifest.DomainTag,
            version = manifest.Version,
            trainedPairCount = loadResult.Value.Checkpoint.TrainedPairIds.Count,
            expectedSkill,
            selectedSkill,
            importedSelectedSkill,
            selectedSkillIds = loadedDecision.Value.SelectedSkillIds,
            importedSelectedSkillIds = importedDecision.Value.SelectedSkillIds,
            loadedProbeEvidenceFeatures = loadedDecision.Value.EvidenceFeatures,
            loadedProbeConfidence = loadedDecision.Value.Confidence,
            loadedProbeRequiresSecondPass = loadedDecision.Value.RequiresSecondPass,
            reloadProbePass
        },
        rollback = rollbackEvidence,
        nonClaims = NonClaims(),
        narrowHumanClaim = pass
            ? "Row17 temp-only core checkpoint promotion/reload proof passed: the copied candidate was selected through temporary active locator, loaded through core SkillSelection APIs, selected client-adaptive-learning-loop, and the temp locator was rolled back to baseline."
            : "Row17 temp-only core checkpoint promotion/reload proof did not pass; see packet fields for the failing gate.",
        needsFixNextTinyWorkOrder = pass
            ? ""
            : "Inspect row17-promotion-reload-execution-packet.json fields hashChecksPass, reloadProbePass, and rollbackRestoredBaseline; fix only the failing temp-run-folder harness or temp locator artifact, then rerun inside the same Row17 scope."
    };

    await WriteJsonAsync(resultJsonPath, result);
    await File.WriteAllTextAsync(resultMdPath, BuildMarkdown(
        status,
        pass,
        runFolder,
        commandRun,
        sourceCheckpointBefore,
        sourceManifestBefore,
        candidateCheckpointBefore,
        candidateManifestBefore,
        candidateCheckpointAfterReload,
        candidateManifestAfterReload,
        candidateCheckpointAfterRollback,
        candidateManifestAfterRollback,
        selectedSkill,
        importedSelectedSkill,
        hashChecksPass,
        reloadProbePass,
        rollbackRestoredBaseline,
        activeLocatorBeforeSwitchSha256,
        activeLocatorAfterSwitchSha256,
        activeLocatorAfterRollbackSha256,
        baselineLocatorSha256,
        resultJsonPath,
        rollbackEvidencePath), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

    Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
    return pass ? 0 : 2;
}
catch (Exception ex)
{
    Directory.CreateDirectory(runFolderForCatch);
    var needsFixJsonPath = Path.Combine(runFolderForCatch, "row17-promotion-reload-execution-needs-fix.json");
    var needsFixMdPath = Path.Combine(runFolderForCatch, "row17-promotion-reload-execution-needs-fix-ru.md");
    var packet = new
    {
        schema = "xiigen.row17.promotion_reload_execution_packet.v1",
        status = "ROW17_PROMOTION_RELOAD_EXECUTION_NEEDS_FIX",
        pass = false,
        executedAtUtc = DateTimeOffset.UtcNow,
        subagentId = "bounded-sub-agent-current-session",
        runFolder = runFolderForCatch,
        errorType = ex.GetType().FullName,
        errorMessage = ex.Message,
        stackTrace = ex.ToString(),
        productReadinessFullPass = false,
        fullJoinedPass = false,
        row17Accepted = false,
        nonClaims = NonClaims(),
        nextTinyWorkOrder = "Fix the failing Row17 temp-run-folder-only harness or temp locator artifact named in errorMessage, then rerun the same Row17 command. Do not edit production code or any file outside the Row17 run folder."
    };
    await WriteJsonAsync(needsFixJsonPath, packet);
    await File.WriteAllTextAsync(needsFixMdPath, $"""
    # Row17 checkpoint promotion/reload execution

    Статус: `ROW17_PROMOTION_RELOAD_EXECUTION_NEEDS_FIX`

    Ошибка: `{ex.Message}`

    Узкое следующее задание: исправить только failing temp-run-folder harness или temp locator artifact, затем повторить Row17 command. Не редактировать production code и не писать вне Row17 run folder.

    Non-claims: product readiness `false`, full joined pass `false`, host UI `false`, real active locator mutation `false`.
    """, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
    Console.Error.WriteLine(ex);
    return 2;
}

static Dictionary<string, string> ParseArgs(string[] args)
{
    var parsed = new Dictionary<string, string>(StringComparer.Ordinal);
    for (var i = 0; i < args.Length; i += 2)
    {
        if (i + 1 >= args.Length)
            throw new ArgumentException($"Missing value for argument {args[i]}.");
        parsed[args[i]] = args[i + 1];
    }

    return parsed;
}

static string Required(IReadOnlyDictionary<string, string> args, string key)
    => args.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value)
        ? value
        : throw new ArgumentException($"Required argument missing: {key}");

static string RequiredProperty(JsonElement element, string name)
    => element.TryGetProperty(name, out var property) && property.GetString() is { Length: > 0 } value
        ? value
        : throw new InvalidOperationException($"Required JSON property missing: {name}");

static void RequireFile(string path, string label)
{
    if (!File.Exists(path))
        throw new FileNotFoundException($"{label} not found.", path);
}

static string Sha256(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

static async Task WriteJsonAsync<T>(string path, T value)
{
    var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    var json = JsonSerializer.Serialize(value, options)
        .Replace("\r\n", "\n", StringComparison.Ordinal)
        .Replace("\r", "\n", StringComparison.Ordinal);
    await File.WriteAllTextAsync(path, json, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
}

static string ToPortableRelativePath(string fromDirectory, string path)
    => Path.GetRelativePath(fromDirectory, path).Replace(Path.DirectorySeparatorChar, '/');

static void EnsureUnderRoot(string path, string root, string label)
{
    var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    if (string.Equals(fullPath, fullRoot, StringComparison.OrdinalIgnoreCase))
        return;

    var fullRootWithSeparator = fullRoot + Path.DirectorySeparatorChar;
    if (!fullPath.StartsWith(fullRootWithSeparator, StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException($"{label} is outside run folder: {fullPath}");
}

static object NonClaims()
    => new
    {
        productReadiness = false,
        tokenSavings = false,
        fullSemanticQuality = false,
        trainingReadiness = false,
        commonPromotion = false,
        realProductionLocatorUpdate = false,
        realActiveLocatorMutation = false,
        hostUiReadiness = false,
        fullJoinedPass = false,
        fullExtensionReadiness = false,
        row17Accepted = false,
        trainingPerformed = false,
        evaluationPerformed = false,
        checkpointMutationOutsideTempRunFolder = false,
        sourceAssetRootMutation = false,
        gitActionPerformed = false
    };

static string BuildMarkdown(
    string status,
    bool pass,
    string runFolder,
    string commandRun,
    string sourceCheckpointBefore,
    string sourceManifestBefore,
    string candidateCheckpointBefore,
    string candidateManifestBefore,
    string candidateCheckpointAfterReload,
    string candidateManifestAfterReload,
    string candidateCheckpointAfterRollback,
    string candidateManifestAfterRollback,
    string selectedSkill,
    string importedSelectedSkill,
    bool hashChecksPass,
    bool reloadProbePass,
    bool rollbackRestoredBaseline,
    string activeLocatorBeforeSwitchSha256,
    string activeLocatorAfterSwitchSha256,
    string activeLocatorAfterRollbackSha256,
    string baselineLocatorSha256,
    string resultJsonPath,
    string rollbackEvidencePath)
    => $"""
    # Row17 checkpoint promotion/reload execution

    Статус: `{status}`

    PASS: `{pass}`

    Run folder: `{runFolder}`

    Команда: `{commandRun}`

    ## Hash evidence

    - source checkpoint before: `{sourceCheckpointBefore}`
    - source manifest before: `{sourceManifestBefore}`
    - temp checkpoint before reload: `{candidateCheckpointBefore}`
    - temp manifest before reload: `{candidateManifestBefore}`
    - temp checkpoint after reload: `{candidateCheckpointAfterReload}`
    - temp manifest after reload: `{candidateManifestAfterReload}`
    - temp checkpoint after rollback: `{candidateCheckpointAfterRollback}`
    - temp manifest after rollback: `{candidateManifestAfterRollback}`
    - hashChecksPass: `{hashChecksPass}`

    ## Reload/probe evidence

    - core import path: `SkillSelectionMachine.ImportAsync`
    - core load path: `SkillSelectionMachine.LoadCheckpointAsync`
    - expected selected skill: `client-adaptive-learning-loop`
    - loaded selected skill: `{selectedSkill}`
    - imported selected skill: `{importedSelectedSkill}`
    - reloadProbePass: `{reloadProbePass}`

    ## Rollback evidence

    - active locator before switch SHA-256: `{activeLocatorBeforeSwitchSha256}`
    - active locator after switch SHA-256: `{activeLocatorAfterSwitchSha256}`
    - active locator after rollback SHA-256: `{activeLocatorAfterRollbackSha256}`
    - baseline locator SHA-256: `{baselineLocatorSha256}`
    - rollbackRestoredBaseline: `{rollbackRestoredBaseline}`
    - rollback evidence JSON: `{rollbackEvidencePath}`

    ## Non-claims

    - product readiness: `false`
    - token savings: `false`
    - full semantic quality: `false`
    - training readiness: `false`
    - common promotion: `false`
    - real production locator update: `false`
    - real active locator mutation: `false`
    - host UI readiness: `false`
    - full joined pass: `false`
    - full extension readiness: `false`
    - Row17 accepted: `false`

    ## Narrow human claim

    {(pass
        ? "Row17 temp-only core checkpoint promotion/reload proof passed: the copied candidate was selected through a temporary active locator, loaded through core SkillSelection APIs, selected `client-adaptive-learning-loop`, and the temp locator was rolled back to baseline."
        : "Row17 temp-only core checkpoint promotion/reload proof did not pass. See the JSON packet for the failing gate.")}

    JSON packet: `{resultJsonPath}`
    """;
