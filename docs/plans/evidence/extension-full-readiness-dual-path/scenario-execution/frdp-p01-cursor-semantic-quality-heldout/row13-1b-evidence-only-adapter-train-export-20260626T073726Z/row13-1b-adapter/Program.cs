using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using SmartSkillsDomain;

internal static class Program
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: Row13_1bEvidenceOnlyAdapter <repo-root> <dpo-jsonl> <output-dir> <command-output-path>");
            return 2;
        }

        var repoRoot = Path.GetFullPath(args[0]);
        var dpoRowsPath = Path.GetFullPath(args[1]);
        var outputDir = Path.GetFullPath(args[2]);
        var commandOutputPath = Path.GetFullPath(args[3]);
        var generatedAtUtc = DateTimeOffset.UtcNow;

        if (!File.Exists(Path.Combine(repoRoot, "LlmFromScratch.sln")))
            throw new InvalidOperationException($"Repository root not found: {repoRoot}");
        if (!File.Exists(dpoRowsPath))
            throw new FileNotFoundException("Row13.1a DPO rows file not found.", dpoRowsPath);

        var allowedRoot = Path.GetFullPath(Path.Combine(
            repoRoot,
            "docs",
            "plans",
            "evidence",
            "extension-full-readiness-dual-path",
            "scenario-execution",
            "frdp-p01-cursor-semantic-quality-heldout"));
        if (!IsInside(outputDir, allowedRoot))
            throw new InvalidOperationException($"Output directory must stay inside Row13.1 evidence root: {allowedRoot}");
        if (!Path.GetFileName(outputDir).StartsWith("row13-1b-evidence-only-adapter-train-export-", StringComparison.Ordinal))
            throw new InvalidOperationException("Output directory must be a timestamped Row13.1b evidence-only adapter folder.");

        Directory.CreateDirectory(outputDir);
        var stagedCandidateDir = Path.Combine(outputDir, "staged-candidate");
        Directory.CreateDirectory(stagedCandidateDir);

        var rowReadResults = ReadRows(dpoRowsPath);
        var rowsRead = rowReadResults.Count;
        var rowsUsed = rowReadResults
            .Where(row => row.Row is not null && IsEligible(row.Row))
            .Select(row => row.Row!)
            .ToArray();
        var quarantinedRows = rowReadResults
            .Where(row => row.Row is null || !IsEligible(row.Row))
            .Select(row => new QuarantinedRow(
                RowId: row.Row?.RowId ?? $"line-{row.LineNumber}",
                Reason: row.Error ?? QuarantineReason(row.Row!)))
            .ToArray();

        if (rowsUsed.Length == 0)
            throw new InvalidOperationException("No eligible Row13.1a universal-only DPO rows were available for Row13.1b evidence-only training.");

        var pairs = rowsUsed.Select(ToMachinePair).ToArray();
        var machine = new SkillSelectionMachine(stagedCandidateDir);
        var trainResult = await machine.TrainMachineBatchAsync(pairs);
        if (!trainResult.IsSuccess || trainResult.Value is null)
            throw new InvalidOperationException($"TrainMachineBatchAsync failed: {trainResult.ErrorMessage}");

        var checkpointPath = Path.Combine(stagedCandidateDir, "skill-selection-checkpoint.json");
        var manifestPath = Path.Combine(stagedCandidateDir, "skill-selection-manifest.json");
        var saveResult = await machine.SaveCheckpointAsync(checkpointPath, manifestPath);
        if (!saveResult.IsSuccess || saveResult.Value is null)
            throw new InvalidOperationException($"SaveCheckpointAsync failed: {saveResult.ErrorMessage}");

        var checkpointExists = File.Exists(checkpointPath);
        var manifestExists = File.Exists(manifestPath);
        if (!checkpointExists || !manifestExists)
            throw new InvalidOperationException("SaveCheckpointAsync returned success but checkpoint or manifest is missing.");

        var checkpointHash = Sha256(checkpointPath);
        var manifestHash = Sha256(manifestPath);
        var skillsTrained = rowsUsed
            .SelectMany(row => row.ChosenSkillIds ?? [])
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var reportPath = Path.Combine(outputDir, "row13-1b-evidence-only-adapter-train-export-report.json");
        var reportMdPath = Path.Combine(outputDir, "row13-1b-evidence-only-adapter-train-export-report.md");

        var report = new
        {
            schemaVersion = "row13.1b.evidence_only_adapter_train_export.v1",
            status = "ROW13_1B_ADAPTER_TRAIN_EXPORT_PASS",
            generatedAtUtc,
            input = new
            {
                dpoRowsPath,
                dpoRowsSha256 = Sha256(dpoRowsPath)
            },
            adapter = new
            {
                apiUsed = "SkillSelectionMachine.TrainMachineBatchAsync(IReadOnlyList<SkillSelectionMachinePair>) + SaveCheckpointAsync(checkpointPath, manifestPath)",
                targetTrainingAdapter = SkillSelectionMachine.SkillSelectionTrainingAdapter,
                startedFromFreshMachine = true
            },
            counts = new
            {
                rowsRead,
                rowsUsed = rowsUsed.Length,
                rowsQuarantined = quarantinedRows.Length
            },
            rowsRead,
            rowsUsed = rowsUsed.Length,
            rowsQuarantined = quarantinedRows.Length,
            quarantinedRows,
            skillsTrained,
            training = new
            {
                trainResult.Value.BatchId,
                trainResult.Value.PairCount,
                trainResult.Value.ContractOnly,
                trainResult.Value.GateNote,
                pairIds = pairs.Select(pair => pair.PairId).ToArray()
            },
            stagedCandidate = new
            {
                directory = stagedCandidateDir,
                checkpointPath,
                checkpointSha256 = checkpointHash,
                checkpointExists,
                manifestPath,
                manifestSha256 = manifestHash,
                manifestExists,
                saveResult.Value.MachineId,
                saveResult.Value.DomainTag,
                saveResult.Value.Version
            },
            commandOutputPath,
            verification = new
            {
                reportJsonWritten = true,
                checkpointFileExists = checkpointExists,
                manifestFileExists = manifestExists,
                checkpointHashVerifiedAtWrite = checkpointHash == Sha256(checkpointPath),
                manifestHashVerifiedAtWrite = manifestHash == Sha256(manifestPath),
                evaluationOutputFilesGenerated = Array.Empty<string>()
            },
            noProductionCheckpointMutation = true,
            locatorUpdateNotPerformed = true,
            promotionNotPerformed = true,
            evaluationRun = false,
            semanticQualityImprovementNotClaimed = true,
            noEvalInThisStep = true,
            gitActionsPerformed = false,
            evidenceBoundary = "The evidence supports only Row13.1b staged candidate adapter train/export from reviewed Row13.1a JSONL rows. It does not support held-out quality, readiness, promotion, locator update, or production/common checkpoint mutation."
        };

        WriteUtf8Lf(reportPath, JsonSerializer.Serialize(report, JsonOptions) + "\n");
        WriteUtf8Lf(reportMdPath, BuildMarkdownReport(
            rowsRead,
            rowsUsed.Length,
            quarantinedRows.Length,
            skillsTrained,
            checkpointPath,
            checkpointHash,
            manifestPath,
            manifestHash,
            commandOutputPath));

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            status = "ROW13_1B_ADAPTER_TRAIN_EXPORT_PASS",
            rowsRead,
            rowsUsed = rowsUsed.Length,
            rowsQuarantined = quarantinedRows.Length,
            skillsTrained,
            checkpointPath,
            checkpointSha256 = checkpointHash,
            manifestPath,
            manifestSha256 = manifestHash,
            reportPath,
            reportMdPath,
            noProductionCheckpointMutation = true,
            locatorUpdateNotPerformed = true,
            promotionNotPerformed = true,
            evaluationRun = false,
            semanticQualityImprovementNotClaimed = true
        }, JsonOptions));
        return 0;
    }

    private static SkillSelectionMachinePair ToMachinePair(Row13DpoRow row)
    {
        var prompt = string.Join(" ", new[] { row.Prompt, row.TaskText, row.ScenarioText }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        var availableSkillIds = row.AvailableSkillIds ?? [];
        var chosenSkillIds = row.ChosenSkillIds ?? [];
        var rejectedSkillIds = row.RejectedSkillIds ?? [];

        return new SkillSelectionMachinePair(
            PairId: row.RowId,
            Request: new SkillSelectionRequest(
                Prompt: prompt,
                ProjectRoot: "",
                AvailableSkillIds: availableSkillIds,
                HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["phase"] = "Row13.1b",
                    ["training_source"] = "row13.1a-universal-only-dpo-rows",
                    ["evidence_only_adapter_train_export"] = "true",
                    ["universal_only"] = "true"
                },
                UserPromptSourceEntryId: row.RowId),
            Chosen: new SkillSelectionDecision(
                SelectedSkillIds: chosenSkillIds,
                RejectedSkillIds: availableSkillIds.Where(skillId => !chosenSkillIds.Contains(skillId, StringComparer.Ordinal)).ToArray(),
                EvidenceFeatures: ["ROW13_1A_DPO_CHOSEN", "UNIVERSAL_ONLY", "EVIDENCE_ONLY_TRAIN_EXPORT"],
                Confidence: 1.0,
                RequiresSecondPass: false,
                DecisionText: row.Chosen),
            Rejected: new SkillSelectionDecision(
                SelectedSkillIds: rejectedSkillIds,
                RejectedSkillIds: availableSkillIds.Where(skillId => !rejectedSkillIds.Contains(skillId, StringComparer.Ordinal)).ToArray(),
                EvidenceFeatures: ["ROW13_1A_DPO_REJECTED", "UNIVERSAL_ONLY", "EVIDENCE_ONLY_TRAIN_EXPORT"],
                Confidence: 0.0,
                RequiresSecondPass: true,
                DecisionText: row.Rejected),
            RejectedType: "row13_1a_universal_only_negative_skill_set",
            SourcePromptEntryId: row.RowId,
            TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter);
    }

    private static IReadOnlyList<RowReadResult> ReadRows(string path)
    {
        var results = new List<RowReadResult>();
        var lineNumber = 0;
        foreach (var line in File.ReadLines(path, Encoding.UTF8))
        {
            lineNumber++;
            if (string.IsNullOrWhiteSpace(line))
                continue;

            try
            {
                var row = JsonSerializer.Deserialize<Row13DpoRow>(line, JsonOptions);
                results.Add(new RowReadResult(lineNumber, row, row is null ? "json_deserialized_null" : null));
            }
            catch (JsonException ex)
            {
                results.Add(new RowReadResult(lineNumber, null, $"json_parse_failed:{ex.Message}"));
            }
        }

        return results;
    }

    private static bool IsEligible(Row13DpoRow row)
        => row.Usable
            && !row.Quarantined
            && row.UniversalOnly
            && !string.IsNullOrWhiteSpace(row.RowId)
            && !string.IsNullOrWhiteSpace(row.Prompt)
            && !string.IsNullOrWhiteSpace(row.Chosen)
            && !string.IsNullOrWhiteSpace(row.Rejected)
            && (row.AvailableSkillIds?.Count ?? 0) > 0
            && (row.ChosenSkillIds?.Count ?? 0) > 0
            && (row.RejectedSkillIds?.Count ?? 0) > 0;

    private static string QuarantineReason(Row13DpoRow row)
    {
        var reasons = new List<string>();
        if (!row.Usable) reasons.Add("usable=false");
        if (row.Quarantined) reasons.Add("quarantined=true");
        if (!row.UniversalOnly) reasons.Add("universalOnly=false");
        if (string.IsNullOrWhiteSpace(row.RowId)) reasons.Add("rowId_missing");
        if (string.IsNullOrWhiteSpace(row.Prompt)) reasons.Add("prompt_missing");
        if (string.IsNullOrWhiteSpace(row.Chosen)) reasons.Add("chosen_missing");
        if (string.IsNullOrWhiteSpace(row.Rejected)) reasons.Add("rejected_missing");
        if ((row.AvailableSkillIds?.Count ?? 0) == 0) reasons.Add("availableSkillIds_empty");
        if ((row.ChosenSkillIds?.Count ?? 0) == 0) reasons.Add("chosenSkillIds_empty");
        if ((row.RejectedSkillIds?.Count ?? 0) == 0) reasons.Add("rejectedSkillIds_empty");
        return string.Join(";", reasons);
    }

    private static string BuildMarkdownReport(
        int rowsRead,
        int rowsUsed,
        int rowsQuarantined,
        IReadOnlyList<string> skillsTrained,
        string checkpointPath,
        string checkpointHash,
        string manifestPath,
        string manifestHash,
        string commandOutputPath)
        => $"""
        # Row13.1b evidence-only adapter train/export

        Status: ROW13_1B_ADAPTER_TRAIN_EXPORT_PASS

        Rows read: {rowsRead}

        Rows used: {rowsUsed}

        Rows quarantined: {rowsQuarantined}

        Skills trained: {string.Join(", ", skillsTrained)}

        Staged checkpoint: {checkpointPath}

        Staged checkpoint sha256: {checkpointHash}

        Staged manifest: {manifestPath}

        Staged manifest sha256: {manifestHash}

        Command output: {commandOutputPath}

        Safety flags: noProductionCheckpointMutation=true; locatorUpdateNotPerformed=true; promotionNotPerformed=true; evaluationRun=false; semanticQualityImprovementNotClaimed=true.

        Evidence boundary: staged candidate train/export only. No held-out eval, no production/common checkpoint mutation, no locator update, no promotion, and no quality/readiness improvement claim.
        """;

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static void WriteUtf8Lf(string path, string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        if (!normalized.EndsWith('\n'))
            normalized += "\n";
        File.WriteAllText(path, normalized, Utf8NoBom);
    }

    private static bool IsInside(string path, string parent)
    {
        var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullParent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(fullParent, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record RowReadResult(int LineNumber, Row13DpoRow? Row, string? Error);

    private sealed record QuarantinedRow(string RowId, string Reason);

    private sealed record Row13DpoRow(
        string SchemaVersion,
        string RowId,
        string TargetSkill,
        string SkillId,
        string Prompt,
        string Chosen,
        string Rejected,
        string TaskText,
        string ScenarioText,
        IReadOnlyList<string>? AvailableSkillIds,
        IReadOnlyList<string>? ChosenSkillIds,
        IReadOnlyList<string>? RejectedSkillIds,
        bool UniversalOnly,
        string UniversalOnlyRationale,
        string SourceDisjointNote,
        string VendorOutputDistillationNote,
        bool Quarantined,
        bool Usable,
        string QuarantineDecision,
        string SafetyNote);
}
