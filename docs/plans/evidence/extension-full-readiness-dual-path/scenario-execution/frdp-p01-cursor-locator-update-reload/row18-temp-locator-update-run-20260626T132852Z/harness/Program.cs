using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
{
    WriteIndented = true,
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
};

static string Sha256File(string path)
{
    using var stream = File.OpenRead(path);
    return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
}

static bool IsUnder(string path, string root)
{
    var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    var fullRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    return fullPath.Equals(fullRoot, StringComparison.OrdinalIgnoreCase)
        || fullPath.StartsWith(fullRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
        || fullPath.StartsWith(fullRoot + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
}

static void Require(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

var locatorPath = args.Length > 0 ? args[0] : throw new InvalidOperationException("locator path argument is required");
var resultPath = args.Length > 1 ? args[1] : throw new InvalidOperationException("result path argument is required");
var locatorJson = await File.ReadAllTextAsync(locatorPath, Encoding.UTF8);
var locator = JsonNode.Parse(locatorJson)?.AsObject() ?? throw new InvalidOperationException("locator JSON parse failed");
var runFolder = locator["runFolder"]!.GetValue<string>();
var candidateRoot = locator["candidateRoot"]!.GetValue<string>();
var checkpointPath = Path.GetFullPath(locator["checkpointRef"]!.GetValue<string>());
var manifestPath = Path.GetFullPath(locator["manifestRef"]!.GetValue<string>());
var expectedCheckpointSha = locator["expectedCheckpointSha256"]!.GetValue<string>();
var expectedManifestSha = locator["expectedManifestSha256"]!.GetValue<string>();
var expectedSkill = locator["expectedSkill"]!.GetValue<string>();

object report;
try
{
    Require(locator["status"]!.GetValue<string>() == "active_temp_candidate", "locator status is not active_temp_candidate");
    Require(locator["realUserGlobalCommonLocatorMutation"]!.GetValue<bool>() == false, "real/global locator mutation flag must be false");
    Require(IsUnder(locatorPath, runFolder), "locator path is outside run folder");
    Require(IsUnder(candidateRoot, runFolder), "candidate root is outside run folder");
    Require(IsUnder(checkpointPath, candidateRoot), "checkpoint path is outside temp candidate root");
    Require(IsUnder(manifestPath, candidateRoot), "manifest path is outside temp candidate root");

    var checkpointSha = Sha256File(checkpointPath);
    var manifestSha = Sha256File(manifestPath);
    Require(checkpointSha == expectedCheckpointSha, "checkpoint hash mismatch in harness");
    Require(manifestSha == expectedManifestSha, "manifest hash mismatch in harness");

    using var manifestDoc = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath, Encoding.UTF8));
    var manifest = manifestDoc.RootElement;
    var accuracyAtExport = manifest.TryGetProperty("accuracyAtExport", out var accuracyElement)
        ? accuracyElement.GetDouble()
        : 0.0;
    var exportedAtUtc = manifest.TryGetProperty("exportedAtUtc", out var exportedAtElement)
        && DateTimeOffset.TryParse(exportedAtElement.GetString(), out var parsedExportedAt)
            ? parsedExportedAt
            : DateTimeOffset.UtcNow;

    var load = await SkillSelectionMachine.LoadCheckpointAsync(checkpointPath);
    Require(load.IsSuccess && load.Value is not null, $"LoadCheckpointAsync failed: {load.ErrorCode} {load.ErrorMessage}");

    var export = new LearningMachineExport(
        SkillSelectionMachine.MachineId,
        SkillSelectionMachine.DomainTag,
        SkillSelectionMachine.Version,
        checkpointPath,
        manifestPath,
        accuracyAtExport,
        exportedAtUtc);

    var importedMachine = new SkillSelectionMachine();
    var import = await importedMachine.ImportAsync(export);
    Require(import.IsSuccess && import.Value, $"ImportAsync failed: {import.ErrorCode} {import.ErrorMessage}");

    var request = new SkillSelectionRequest(
        Prompt: "redacted Row18 core lifecycle proof for client adaptive learning loop locator reload; continue training, user local adaptive DPO authoring, checkpoint lifecycle, fresh load, promotion proof, verification, backup export, common model claims false, model selection near misses avoided",
        ProjectRoot: runFolder,
        AvailableSkillIds: new[]
        {
            expectedSkill,
            "planning-dpo-authoring",
            "dpo-triple-format",
            "training-data-gap-audit",
            "skill-graph-sync",
            "ai-model-selection",
            "phase-completion-packager",
            "claim-verification"
        },
        HostFacts: new Dictionary<string, string>
        {
            ["machineTarget"] = "SkillSelectionMachine",
            ["phase"] = "row18-locator-reload-proof",
            ["rawJsonlAsPrompt"] = "false",
            ["rowId"] = "row18",
            ["targetSkillId"] = "clientAdaptiveLearningLoop",
            ["targetTrainingAdapterAlias"] = "clientAdaptiveLearningLoop"
        },
        UserPromptSourceEntryId: "row18-locator-update-reload-proof");

    var loadedDecision = load.Value!.Machine.DecideFromTrainedState(request);
    Require(loadedDecision.IsSuccess && loadedDecision.Value is not null, $"loaded decision failed: {loadedDecision.ErrorCode} {loadedDecision.ErrorMessage}");
    var importedDecision = importedMachine.DecideFromTrainedState(request);
    Require(importedDecision.IsSuccess && importedDecision.Value is not null, $"imported decision failed: {importedDecision.ErrorCode} {importedDecision.ErrorMessage}");

    var loadedSelected = loadedDecision.Value!.SelectedSkillIds.ToArray();
    var importedSelected = importedDecision.Value!.SelectedSkillIds.ToArray();
    Require(loadedSelected.Contains(expectedSkill, StringComparer.Ordinal), "loaded machine did not select expected skill");
    Require(importedSelected.Contains(expectedSkill, StringComparer.Ordinal), "imported machine did not select expected skill");
    Require(!loadedDecision.Value.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal), "loaded decision used candidate-position scaffold evidence");
    Require(!importedDecision.Value.EvidenceFeatures.Contains(SkillSelectionMachine.CandidatePositionScaffoldEvidence, StringComparer.Ordinal), "imported decision used candidate-position scaffold evidence");

    report = new
    {
        status = "PASS",
        locatorPath,
        runFolder,
        candidateRoot,
        checkpointPath,
        manifestPath,
        checkpointSha256 = checkpointSha,
        manifestSha256 = manifestSha,
        methodsExercised = new[]
        {
            "SkillSelectionMachine.LoadCheckpointAsync",
            "SkillSelectionMachine.ImportAsync",
            "SkillSelectionMachine.DecideFromTrainedState"
        },
        expectedSkill,
        loadedSelectedSkillIds = loadedSelected,
        importedSelectedSkillIds = importedSelected,
        loadedDecisionEvidence = loadedDecision.Value.EvidenceFeatures,
        importedDecisionEvidence = importedDecision.Value.EvidenceFeatures,
        loadedRequiresSecondPass = loadedDecision.Value.RequiresSecondPass,
        importedRequiresSecondPass = importedDecision.Value.RequiresSecondPass,
        sourceFallbackUsed = false,
        hostUiActionPerformed = false,
        realUserGlobalCommonLocatorMutation = false,
        semanticQualityClaimed = false,
        productReadinessClaimed = false
    };
}
catch (Exception ex)
{
    report = new
    {
        status = "FAIL",
        locatorPath,
        error = ex.GetType().FullName,
        message = ex.Message,
        hostUiActionPerformed = false,
        realUserGlobalCommonLocatorMutation = false,
        semanticQualityClaimed = false,
        productReadinessClaimed = false
    };
    Environment.ExitCode = 2;
}

var text = JsonSerializer.Serialize(report, jsonOptions).Replace("\r\n", "\n").Replace("\r", "\n") + "\n";
await File.WriteAllTextAsync(resultPath, text, new UTF8Encoding(false));
Console.WriteLine(text);