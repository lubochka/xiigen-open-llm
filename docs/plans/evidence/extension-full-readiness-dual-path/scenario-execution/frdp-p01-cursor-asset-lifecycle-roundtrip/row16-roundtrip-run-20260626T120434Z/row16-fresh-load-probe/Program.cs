using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

var parsed = ParseArgs(args);
var assetRoot = Required(parsed, "--asset-root");
var evidenceDir = Required(parsed, "--evidence-dir");
var expectedCheckpointSha256 = Required(parsed, "--expected-checkpoint-sha256").ToLowerInvariant();
var expectedManifestSha256 = Required(parsed, "--expected-manifest-sha256").ToLowerInvariant();

Directory.CreateDirectory(evidenceDir);

var checkpointPath = Path.Combine(assetRoot, "skill-selection-checkpoint.json");
var manifestPath = Path.Combine(assetRoot, "skill-selection-manifest.json");
var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true
};
var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var checkpointShaBefore = Sha256(checkpointPath);
var manifestShaBefore = Sha256(manifestPath);

var manifest = JsonSerializer.Deserialize<SkillSelectionCheckpointManifest>(
    await File.ReadAllTextAsync(manifestPath, Encoding.UTF8),
    jsonOptions) ?? throw new InvalidOperationException("Manifest deserialized null.");

var export = new LearningMachineExport(
    manifest.MachineId,
    manifest.DomainTag,
    manifest.Version,
    checkpointPath,
    manifestPath,
    manifest.AccuracyAtExport,
    manifest.ExportedAtUtc);

var importedMachine = new SkillSelectionMachine();
var importResult = await importedMachine.ImportAsync(export);
if (!importResult.IsSuccess)
    throw new InvalidOperationException($"ImportAsync failed: {importResult.ErrorCode} {importResult.ErrorMessage}");

var loadResult = await SkillSelectionMachine.LoadCheckpointAsync(checkpointPath);
if (!loadResult.IsSuccess || loadResult.Value is null)
    throw new InvalidOperationException($"LoadCheckpointAsync failed: {loadResult.ErrorCode} {loadResult.ErrorMessage}");

var request = new SkillSelectionRequest(
    Prompt: string.Join(' ', new[]
    {
        "clientadaptivelearningloop",
        "adaptive learning loop",
        "target skill client adaptive learning loop"
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
        ["phase"] = "Row16 asset lifecycle roundtrip",
        ["rawJsonlAsPrompt"] = "false",
        ["rowId"] = "Row16",
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

var checkpointShaAfter = Sha256(checkpointPath);
var manifestShaAfter = Sha256(manifestPath);
var selectedSkill = loadedDecision.Value.SelectedSkillIds.FirstOrDefault() ?? "";
var expectedSkill = "client-adaptive-learning-loop";
var pass =
    importResult.Value
    && checkpointShaBefore == expectedCheckpointSha256
    && manifestShaBefore == expectedManifestSha256
    && checkpointShaAfter == expectedCheckpointSha256
    && manifestShaAfter == expectedManifestSha256
    && string.Equals(manifest.MachineId, SkillSelectionMachine.MachineId, StringComparison.Ordinal)
    && string.Equals(manifest.DomainTag, SkillSelectionMachine.DomainTag, StringComparison.Ordinal)
    && string.Equals(manifest.Version, SkillSelectionMachine.Version, StringComparison.Ordinal)
    && loadResult.Value.Checkpoint.TrainedPairIds.Count == 10
    && loadedDecision.Value.SelectedSkillIds.Contains(expectedSkill, StringComparer.Ordinal);

var result = new
{
    schema = "xiigen.row16.fresh_load_probe.v1",
    assetRoot,
    evidenceDir,
    checkpointPath,
    manifestPath,
    manifestLoaded = true,
    checkpointLoaded = true,
    importAsyncSucceeded = importResult.Value,
    loadCheckpointAsyncSucceeded = true,
    machineId = manifest.MachineId,
    domainTag = manifest.DomainTag,
    version = manifest.Version,
    trainedPairCount = loadResult.Value.Checkpoint.TrainedPairIds.Count,
    expectedSkill,
    selectedSkill,
    selectedSkillIds = loadedDecision.Value.SelectedSkillIds,
    importedSelectedSkillIds = importedDecision.Value.SelectedSkillIds,
    loadedProbeEvidenceFeatures = loadedDecision.Value.EvidenceFeatures,
    loadedProbeConfidence = loadedDecision.Value.Confidence,
    loadedProbeRequiresSecondPass = loadedDecision.Value.RequiresSecondPass,
    checkpointSha256BeforeLoad = checkpointShaBefore,
    manifestSha256BeforeLoad = manifestShaBefore,
    checkpointSha256AfterLoad = checkpointShaAfter,
    manifestSha256AfterLoad = manifestShaAfter,
    expectedCheckpointSha256,
    expectedManifestSha256,
    hashStableAcrossLoad = checkpointShaBefore == checkpointShaAfter && manifestShaBefore == manifestShaAfter,
    trainingPerformed = false,
    evaluationPerformed = false,
    promotionPerformed = false,
    locatorUpdatePerformed = false,
    hostUiRun = false,
    pass
};

var jsonPath = Path.Combine(evidenceDir, "fresh-load-probe-result.json");
var mdPath = Path.Combine(evidenceDir, "fresh-load-probe-report-ru.md");
await File.WriteAllTextAsync(jsonPath, JsonSerializer.Serialize(result, jsonOptions), utf8NoBom);
await File.WriteAllTextAsync(mdPath, BuildMarkdown(result), utf8NoBom);

Console.WriteLine(JsonSerializer.Serialize(result, jsonOptions));
return pass ? 0 : 2;

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

static string Sha256(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

static string BuildMarkdown(dynamic result)
{
    var selected = string.Join(", ", result.selectedSkillIds);
    return $"""
    # Row16 fresh-load probe

    Статус: {(result.pass ? "PASS" : "FAIL")}

    Что проверено:
    - fresh asset root: `{result.assetRoot}`
    - `SkillSelectionMachine.ImportAsync`: `{result.importAsyncSucceeded}`
    - `SkillSelectionMachine.LoadCheckpointAsync`: `{result.loadCheckpointAsyncSucceeded}`
    - manifestLoaded: `{result.manifestLoaded}`
    - checkpointLoaded: `{result.checkpointLoaded}`
    - machineId: `{result.machineId}`
    - domainTag: `{result.domainTag}`
    - version: `{result.version}`
    - trainedPairCount: `{result.trainedPairCount}`
    - expected skill: `{result.expectedSkill}`
    - selected skill: `{result.selectedSkill}`
    - selected skill ids: `{selected}`

    Хэши:
    - checkpoint before load: `{result.checkpointSha256BeforeLoad}`
    - checkpoint after load: `{result.checkpointSha256AfterLoad}`
    - manifest before load: `{result.manifestSha256BeforeLoad}`
    - manifest after load: `{result.manifestSha256AfterLoad}`
    - hashStableAcrossLoad: `{result.hashStableAcrossLoad}`

    Не выполнялось:
    - training: `{result.trainingPerformed}`
    - evaluation: `{result.evaluationPerformed}`
    - promotion: `{result.promotionPerformed}`
    - locator update: `{result.locatorUpdatePerformed}`
    - host UI: `{result.hostUiRun}`

    Узкое утверждение: импортированный во fresh root asset загружается через core API и дает ожидаемый skill-selection результат для `client-adaptive-learning-loop`; это не product readiness и не acceptance Row16.
    """;
}
