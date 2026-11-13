using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

internal static class Program
{
    private const string PhaseId = "ROW14.3";
    private const string PassVerdict = "ROW14_3_STAGED_CONTINUE_TRAINING_EXPORT_PASS";
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private static readonly JsonSerializerOptions CompactJsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static async Task<int> Main(string[] args)
    {
        try
        {
            var repoRoot = FindRepoRoot();
            var evidenceDir = RequireEvidenceDir(args, repoRoot);
            Directory.CreateDirectory(evidenceDir);

            var root = Path.Combine(
                repoRoot,
                "docs",
                "plans",
                "evidence",
                "extension-full-readiness-dual-path",
                "scenario-execution",
                "frdp-p01-cursor-learnability-training-continue");
            var backupDir = Path.Combine(
                root,
                "row14-1-backup-export-preflight-no-training-20260626T102311Z",
                "backup-export");
            var row14_2Path = Path.Combine(
                root,
                "row14-2-dpo-rows-20260626T102909Z",
                "row14-2-skillselection-dpo-rows.jsonl");
            var baseManifestPath = Path.Combine(backupDir, "manifest.json");
            var baseCheckpointPath = Path.Combine(backupDir, "skill-selection-v1-checkpoint.json");
            var stagedExportDir = Path.Combine(evidenceDir, "staged-export");
            Directory.CreateDirectory(stagedExportDir);

            var sourceHashesBefore = HashSources(baseManifestPath, baseCheckpointPath, row14_2Path);
            var rows = ParseRows(row14_2Path, repoRoot);
            var baseManifest = ReadJson<SkillSelectionCheckpointManifest>(baseManifestPath)
                ?? throw new InvalidOperationException("Row14.1 manifest deserialized null.");
            if (baseManifest.SemanticQualityClaimed)
                throw new InvalidOperationException("Row14.1 backup manifest claims semantic quality.");

            var stagedMachine = new SkillSelectionMachine(stagedExportDir);
            var baseExport = new LearningMachineExport(
                baseManifest.MachineId,
                baseManifest.DomainTag,
                baseManifest.Version,
                baseCheckpointPath,
                baseManifestPath,
                baseManifest.AccuracyAtExport,
                baseManifest.ExportedAtUtc);
            var baseImport = await stagedMachine.ImportAsync(baseExport);
            RequireSuccess(baseImport, "Base Row14.1 import from checkpoint/manifest failed.");

            var baseCheckpoint = stagedMachine.ExportCheckpoint();
            var trainResult = await stagedMachine.TrainMachineBatchAsync(rows.Pairs);
            RequireSuccess(trainResult, "Continue-training from Row14.1 backup base failed.");

            var stagedInMemoryCheckpoint = stagedMachine.ExportCheckpoint();
            foreach (var pair in rows.Pairs)
            {
                if (!stagedInMemoryCheckpoint.TrainedPairIds.Contains(pair.PairId, StringComparer.Ordinal))
                    throw new InvalidOperationException($"Staged checkpoint missing trained pair id {pair.PairId}.");
            }

            var exportResult = await stagedMachine.ExportAsync();
            RequireSuccess(exportResult, "Staged export failed.");
            var export = exportResult.Value!;

            if (!File.Exists(export.CheckpointRef))
                throw new FileNotFoundException("Staged checkpoint was not created.", export.CheckpointRef);
            if (!File.Exists(export.ManifestRef))
                throw new FileNotFoundException("Staged manifest was not created.", export.ManifestRef);

            var stagedManifest = ReadJson<SkillSelectionCheckpointManifest>(export.ManifestRef)
                ?? throw new InvalidOperationException("Staged manifest deserialized null.");
            if (stagedManifest.SemanticQualityClaimed)
                throw new InvalidOperationException("Staged manifest claims semantic quality.");

            var stagedCheckpointSha256 = Sha256File(export.CheckpointRef);
            var stagedManifestSha256 = Sha256File(export.ManifestRef);

            var freshImportMachine = new SkillSelectionMachine();
            var freshImport = await freshImportMachine.ImportAsync(export);
            RequireSuccess(freshImport, "Fresh import of staged export failed.");

            var freshLoad = await SkillSelectionMachine.LoadCheckpointAsync(export.CheckpointRef);
            RequireSuccess(freshLoad, "Fresh LoadCheckpointAsync of staged checkpoint failed.");
            var freshCheckpoint = freshLoad.Value!.Checkpoint;
            var freshLoadCheck = new
            {
                loadMethod = "SkillSelectionMachine.LoadCheckpointAsync",
                importMethod = "SkillSelectionMachine.ImportAsync",
                loaded = true,
                nonMutatingCheck = "checkpoint metadata and trained pair ids inspected only; held-out evaluation not run",
                trainedPairCount = freshCheckpoint.TrainedPairIds.Count,
                baseTrainedPairCount = baseCheckpoint.TrainedPairIds.Count,
                addedRow14_2PairIds = rows.Pairs.Select(pair => pair.PairId).ToArray(),
                allRow14_2PairIdsPresent = rows.Pairs.All(pair => freshCheckpoint.TrainedPairIds.Contains(pair.PairId, StringComparer.Ordinal))
            };
            if (!freshLoadCheck.allRow14_2PairIdsPresent)
                throw new InvalidOperationException("Fresh-loaded staged checkpoint is missing one or more Row14.2 pair ids.");

            var sourceHashesAfter = HashSources(baseManifestPath, baseCheckpointPath, row14_2Path);
            var inputsUnchanged = SameHashes(sourceHashesBefore, sourceHashesAfter);
            if (!inputsUnchanged)
                throw new InvalidOperationException("Row14.1/Row14.2 source input hashes changed after run.");

            var flags = new SafetyFlags(
                UiRun: false,
                LocatorUpdate: false,
                Promotion: false,
                ProductionMutation: false,
                ReadinessClaimed: false,
                TokenSavingsClaimed: false,
                FullSemanticQualityClaimed: false,
                FullExtensionReadinessClaimed: false,
                TrainingReadinessClaimedBeyondScopedEvidence: false);

            var readBackPhrases = new[]
            {
                "continue-training from existing checkpoint",
                "fresh-load proof",
                "staged export",
                "locator update not performed",
                "promotion not performed",
                "raw JSONL as prompt=false"
            };
            var report = new
            {
                phase = PhaseId,
                verdict = PassVerdict,
                generatedAtUtc = DateTimeOffset.UtcNow,
                scope = "staged continue-training/export from Row14.1 backup base using reviewed Row14.2 rows",
                sourceInputs = new
                {
                    row14_1BackupManifest = Relative(repoRoot, baseManifestPath),
                    row14_1BackupCheckpoint = Relative(repoRoot, baseCheckpointPath),
                    row14_2Rows = Relative(repoRoot, row14_2Path)
                },
                sourceInputHashesBefore = sourceHashesBefore,
                sourceInputHashesAfter = sourceHashesAfter,
                sourceInputsUnchanged = inputsUnchanged,
                row14_2 = new
                {
                    parsedRowCount = rows.RowCount,
                    expectedRowCount = 2,
                    quarantineTrueFound = false,
                    convertedToSkillSelectionMachinePairDirectly = true,
                    rawJsonlAsPrompt = false,
                    pairIds = rows.Pairs.Select(pair => pair.PairId).ToArray(),
                    targetTrainingAdapters = rows.Pairs.Select(pair => new
                    {
                        pairId = pair.PairId,
                        targetTrainingAdapter = pair.TargetTrainingAdapter,
                        preservedConstant = "SkillSelectionMachine.PromptContextTrainingAdapter",
                        matchesPromptContextTrainingAdapter = pair.TargetTrainingAdapter == SkillSelectionMachine.PromptContextTrainingAdapter
                    }).ToArray()
                },
                continueTraining = new
                {
                    phrase = "continue-training from existing checkpoint",
                    startingState = "Row14.1 backup checkpoint/manifest imported before training",
                    baseImportSucceeded = baseImport.IsSuccess,
                    trainMethod = "SkillSelectionMachine.TrainMachineBatchAsync(IReadOnlyList<SkillSelectionMachinePair>)",
                    rowsTrained = trainResult.Value!.PairCount,
                    contractOnly = trainResult.Value.ContractOnly,
                    baseTrainedPairCount = baseCheckpoint.TrainedPairIds.Count,
                    stagedTrainedPairCount = stagedInMemoryCheckpoint.TrainedPairIds.Count
                },
                stagedExport = new
                {
                    phrase = "staged export",
                    exportMethod = "SkillSelectionMachine.ExportAsync",
                    directory = Relative(repoRoot, stagedExportDir),
                    checkpointRef = Relative(repoRoot, export.CheckpointRef),
                    manifestRef = Relative(repoRoot, export.ManifestRef),
                    checkpointExists = File.Exists(export.CheckpointRef),
                    manifestExists = File.Exists(export.ManifestRef),
                    checkpointSha256 = stagedCheckpointSha256,
                    manifestSha256 = stagedManifestSha256,
                    semanticQualityClaimed = stagedManifest.SemanticQualityClaimed
                },
                freshLoadProof = new
                {
                    phrase = "fresh-load proof",
                    freshLoadCheck.loadMethod,
                    freshLoadCheck.importMethod,
                    freshLoadCheck.loaded,
                    freshLoadCheck.nonMutatingCheck,
                    freshLoadCheck.trainedPairCount,
                    freshLoadCheck.baseTrainedPairCount,
                    freshLoadCheck.addedRow14_2PairIds,
                    freshLoadCheck.allRow14_2PairIdsPresent
                },
                notPerformed = new
                {
                    locatorUpdate = "locator update not performed",
                    promotion = "promotion not performed",
                    ui = "UI not run",
                    productionMutation = "production/common/user locator not mutated"
                },
                heldOutProbe = new
                {
                    status = "planned held-out probe eval-only and separate",
                    row14_4Next = true,
                    uiRun = false
                },
                flags,
                readBackPhrases,
                readBackLine = "continue-training from existing checkpoint; fresh-load proof; staged export; locator update not performed; promotion not performed; raw JSONL as prompt=false",
                limitations = new[]
                {
                    "This is scoped staged continue-training evidence only.",
                    "No UI was run.",
                    "No production/common/user locator was updated.",
                    "No promotion was performed.",
                    "No product readiness, token savings, full semantic quality, full extension readiness, or broad training readiness is claimed."
                }
            };

            var reportPath = Path.Combine(evidenceDir, "row14-3-staged-continue-training-export-report.json");
            var validationPath = Path.Combine(evidenceDir, "row14-3-staged-continue-training-export-validation.json");
            var markdownPath = Path.Combine(evidenceDir, "row14-3-staged-continue-training-export-report.md");
            var convertedPairsPath = Path.Combine(evidenceDir, "row14-3-converted-skillselection-machine-pairs.json");

            WriteJson(reportPath, report);
            WriteJson(validationPath, new
            {
                phase = PhaseId,
                verdict = PassVerdict,
                reportParsedByRunner = true,
                checks = new
                {
                    exactlyTwoRows = rows.RowCount == 2,
                    quarantineFalseForAllRows = true,
                    convertedRowsToSkillSelectionMachinePairDirectly = true,
                    rawJsonlAsPrompt = false,
                    baseCheckpointManifestImported = baseImport.IsSuccess,
                    continueTrainingSucceeded = trainResult.IsSuccess,
                    stagedCheckpointExists = File.Exists(export.CheckpointRef),
                    stagedManifestExists = File.Exists(export.ManifestRef),
                    stagedCheckpointSha256,
                    stagedManifestSha256,
                    freshLoadSucceeded = freshLoad.IsSuccess,
                    freshImportSucceeded = freshImport.IsSuccess,
                    sourceInputsUnchanged = inputsUnchanged,
                    uiRun = flags.UiRun,
                    locatorUpdate = flags.LocatorUpdate,
                    promotion = flags.Promotion,
                    productionMutation = flags.ProductionMutation,
                    readinessClaimed = flags.ReadinessClaimed
                },
                readBackPhrases
            });
            WriteJson(convertedPairsPath, rows.PairAudit);
            WriteText(markdownPath, BuildMarkdown(report));

            Console.WriteLine(JsonSerializer.Serialize(new
            {
                verdict = PassVerdict,
                reportPath,
                validationPath,
                stagedCheckpoint = export.CheckpointRef,
                stagedManifest = export.ManifestRef,
                stagedCheckpointSha256,
                stagedManifestSha256,
                freshLoadSucceeded = freshLoad.IsSuccess,
                sourceInputsUnchanged = inputsUnchanged
            }, CompactJsonOptions));
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static ParsedRows ParseRows(string row14_2Path, string repoRoot)
    {
        var lines = File.ReadAllLines(row14_2Path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToArray();
        if (lines.Length != 2)
            throw new InvalidOperationException($"Expected exactly 2 Row14.2 rows, found {lines.Length}.");

        var pairs = new List<SkillSelectionMachinePair>();
        var pairAudit = new List<object>();
        for (var index = 0; index < lines.Length; index++)
        {
            using var document = JsonDocument.Parse(lines[index]);
            var row = document.RootElement;
            if (HasQuarantineTrue(row))
                throw new InvalidOperationException($"Row {index + 1} contains quarantine=true.");

            var rowId = RequiredString(row, "rowId");
            var pairId = RequiredString(row, "pairId");
            var targetAdapterAlias = RequiredString(row, "targetTrainingAdapter");
            if (!string.Equals(targetAdapterAlias, "SkillSelectionMachine.PromptContextTrainingAdapter", StringComparison.Ordinal))
                throw new InvalidOperationException($"Row {rowId} targetTrainingAdapter was {targetAdapterAlias}.");

            var prompt = RequiredString(row, "prompt");
            var taskText = RequiredString(row, "taskText");
            var teachingPoint = RequiredString(row, "teachingPoint");
            var availableSkillIds = RequiredStringArray(row.GetProperty("availableSkillIds"));
            var requestPrompt = string.Join("\n", new[]
            {
                prompt,
                "Task: " + taskText,
                "Teaching point: " + teachingPoint
            });
            var request = new SkillSelectionRequest(
                Prompt: requestPrompt,
                ProjectRoot: repoRoot,
                AvailableSkillIds: availableSkillIds,
                HostFacts: new Dictionary<string, string>
                {
                    ["phase"] = PhaseId,
                    ["rowId"] = rowId,
                    ["row"] = RequiredString(row, "row"),
                    ["machineTarget"] = RequiredString(row, "machineTarget"),
                    ["targetSkillId"] = RequiredString(row, "targetSkillId"),
                    ["targetTrainingAdapterAlias"] = targetAdapterAlias,
                    ["rawJsonlAsPrompt"] = "false"
                },
                UserPromptSourceEntryId: RequiredString(row, "sourcePromptEntryId"));

            var chosen = BuildDecision(row.GetProperty("chosen"));
            var rejected = BuildDecision(row.GetProperty("rejected"));
            var pair = new SkillSelectionMachinePair(
                PairId: pairId,
                Request: request,
                Chosen: chosen,
                Rejected: rejected,
                RejectedType: "row14_2_near_miss",
                SourcePromptEntryId: RequiredString(row, "sourcePromptEntryId"),
                TargetTrainingAdapter: SkillSelectionMachine.PromptContextTrainingAdapter);
            pairs.Add(pair);
            pairAudit.Add(new
            {
                rowId,
                pairId,
                requestPromptSource = "Row14.2 prompt/taskText/teachingPoint fields; raw JSONL as prompt=false",
                availableSkillIds,
                chosenSelectedSkillIds = chosen.SelectedSkillIds,
                rejectedSelectedSkillIds = rejected.SelectedSkillIds,
                targetTrainingAdapterAlias = targetAdapterAlias,
                targetTrainingAdapterValue = pair.TargetTrainingAdapter,
                convertedToType = nameof(SkillSelectionMachinePair)
            });
        }

        return new ParsedRows(lines.Length, pairs, pairAudit);
    }

    private static SkillSelectionDecision BuildDecision(JsonElement element)
        => new(
            SelectedSkillIds: RequiredStringArray(element.GetProperty("selectedSkillIds")),
            RejectedSkillIds: RequiredStringArray(element.GetProperty("rejectedSkillIds")),
            EvidenceFeatures: RequiredStringArray(element.GetProperty("evidenceFeatures")),
            Confidence: element.GetProperty("confidence").GetDouble(),
            RequiresSecondPass: element.GetProperty("requiresSecondPass").GetBoolean(),
            DecisionText: RequiredString(element, "decisionText"));

    private static bool HasQuarantineTrue(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                if (string.Equals(property.Name, "quarantine", StringComparison.OrdinalIgnoreCase)
                    && property.Value.ValueKind == JsonValueKind.True)
                    return true;
                if (HasQuarantineTrue(property.Value))
                    return true;
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in element.EnumerateArray())
            {
                if (HasQuarantineTrue(item))
                    return true;
            }
        }

        return false;
    }

    private static IReadOnlyDictionary<string, string> HashSources(
        string baseManifestPath,
        string baseCheckpointPath,
        string row14_2Path)
        => new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["row14_1_manifest_sha256"] = Sha256File(baseManifestPath),
            ["row14_1_checkpoint_sha256"] = Sha256File(baseCheckpointPath),
            ["row14_2_rows_sha256"] = Sha256File(row14_2Path)
        };

    private static bool SameHashes(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after)
        => before.Count == after.Count
            && before.All(item =>
                after.TryGetValue(item.Key, out var value)
                && string.Equals(value, item.Value, StringComparison.Ordinal));

    private static void RequireSuccess<T>(OperationResult<T> result, string message)
    {
        if (!result.IsSuccess)
            throw new InvalidOperationException($"{message} {result.ErrorCode}: {result.ErrorMessage}");
    }

    private static T? ReadJson<T>(string path)
        => JsonSerializer.Deserialize<T>(File.ReadAllText(path, Encoding.UTF8), JsonOptions);

    private static void WriteJson(string path, object value)
        => File.WriteAllText(path, NormalizeText(JsonSerializer.Serialize(value, JsonOptions)), Utf8NoBom);

    private static void WriteText(string path, string value)
        => File.WriteAllText(path, NormalizeText(value), Utf8NoBom);

    private static string BuildMarkdown(object report)
    {
        var json = JsonSerializer.Serialize(report, JsonOptions);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var staged = root.GetProperty("stagedExport");
        var fresh = root.GetProperty("freshLoadProof");
        var flags = root.GetProperty("flags");
        return $"""
            # ROW14.3 staged continue-training export

            Verdict: `{root.GetProperty("verdict").GetString()}`

            This report proves scoped `continue-training from existing checkpoint`, `staged export`, and `fresh-load proof` only.

            - Rows parsed: `{root.GetProperty("row14_2").GetProperty("parsedRowCount").GetInt32()}`
            - Raw JSONL as prompt: `false`
            - Staged checkpoint: `{staged.GetProperty("checkpointRef").GetString()}`
            - Staged checkpoint sha256: `{staged.GetProperty("checkpointSha256").GetString()}`
            - Staged manifest: `{staged.GetProperty("manifestRef").GetString()}`
            - Staged manifest sha256: `{staged.GetProperty("manifestSha256").GetString()}`
            - Fresh-load trained pair count: `{fresh.GetProperty("trainedPairCount").GetInt32()}`
            - locator update not performed
            - promotion not performed
            - UI run: `{flags.GetProperty("uiRun").GetBoolean()}`
            - Production mutation: `{flags.GetProperty("productionMutation").GetBoolean()}`
            - Readiness claimed: `{flags.GetProperty("readinessClaimed").GetBoolean()}`

            Row14.4 remains the next separate held-out/fresh-load evaluation work order.
            """;
    }

    private static string NormalizeText(string value)
    {
        var normalized = value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal);
        return string.Join('\n', normalized.Split('\n').Select(line => line.TrimEnd())).TrimEnd() + "\n";
    }

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        return Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    private static string RequiredString(JsonElement element, string propertyName)
    {
        var value = element.GetProperty(propertyName).GetString();
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Required string property {propertyName} is missing.");
        return value;
    }

    private static string[] RequiredStringArray(JsonElement element)
        => element.EnumerateArray()
            .Select(item => item.GetString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray();

    private static string RequireEvidenceDir(string[] args, string repoRoot)
    {
        var index = Array.IndexOf(args, "--evidence-dir");
        if (index < 0 || index + 1 >= args.Length)
            throw new InvalidOperationException("Usage: --evidence-dir <ROW14.3 evidence directory>");

        var evidenceDir = Path.GetFullPath(args[index + 1]);
        var allowedRoot = Path.GetFullPath(Path.Combine(
            repoRoot,
            "docs",
            "plans",
            "evidence",
            "extension-full-readiness-dual-path",
            "scenario-execution",
            "frdp-p01-cursor-learnability-training-continue"));
        if (!evidenceDir.StartsWith(allowedRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Evidence dir must be under {allowedRoot}.");
        if (!Path.GetFileName(evidenceDir).StartsWith("row14-3-staged-continue-training-export-", StringComparison.Ordinal))
            throw new InvalidOperationException("Evidence dir must be a row14-3-staged-continue-training-export timestamped folder.");
        return evidenceDir;
    }

    private static string FindRepoRoot()
    {
        var directory = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "LlmFromScratch.sln")))
                return directory.FullName;
            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root with LlmFromScratch.sln was not found.");
    }

    private static string Relative(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path).Replace('\\', '/');

    private sealed record ParsedRows(
        int RowCount,
        IReadOnlyList<SkillSelectionMachinePair> Pairs,
        IReadOnlyList<object> PairAudit);

    private sealed record SafetyFlags(
        [property: JsonPropertyName("uiRun")] bool UiRun,
        [property: JsonPropertyName("locatorUpdate")] bool LocatorUpdate,
        [property: JsonPropertyName("promotion")] bool Promotion,
        [property: JsonPropertyName("productionMutation")] bool ProductionMutation,
        [property: JsonPropertyName("readinessClaimed")] bool ReadinessClaimed,
        [property: JsonPropertyName("tokenSavingsClaimed")] bool TokenSavingsClaimed,
        [property: JsonPropertyName("fullSemanticQualityClaimed")] bool FullSemanticQualityClaimed,
        [property: JsonPropertyName("fullExtensionReadinessClaimed")] bool FullExtensionReadinessClaimed,
        [property: JsonPropertyName("trainingReadinessClaimedBeyondScopedEvidence")] bool TrainingReadinessClaimedBeyondScopedEvidence);
}
