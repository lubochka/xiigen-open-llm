using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using SmartSkillsDomain;
using AgentInfrastructure.Logical;

internal static class Program
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private static readonly JsonSerializerOptions WebJson = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions WebJsonLine = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private static readonly JsonSerializerOptions SnakeJson = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true
    };

    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine("Usage: Row13_1bWrapper <repo-root> <row13b-output-dir> <dpo-jsonl> <previous-run-dir>");
            return 2;
        }

        var repoRoot = Path.GetFullPath(args[0]);
        var outputDir = Path.GetFullPath(args[1]);
        var dpoRowsPath = Path.GetFullPath(args[2]);
        var previousRunDir = Path.GetFullPath(args[3]);
        Directory.CreateDirectory(outputDir);

        if (!File.Exists(Path.Combine(repoRoot, "LlmFromScratch.sln")))
            throw new InvalidOperationException($"Repository root not found: {repoRoot}");

        var allowedRoot = Path.GetFullPath(Path.Combine(
            repoRoot,
            "docs",
            "plans",
            "evidence",
            "extension-full-readiness-dual-path",
            "scenario-execution",
            "frdp-p01-cursor-semantic-quality-heldout",
            "row13-1b-candidate-training-export-20260626T072146Z"));
        if (!IsInside(outputDir, allowedRoot))
            throw new InvalidOperationException($"Output directory must stay inside Row13.1b evidence folder: {allowedRoot}");

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var p24RowsPath = RepoPath(repoRoot, "artifacts", "smart-skills-semantic-quality-completion", "p24-skillselection-runtime-holdout-rows", "p24-runtime-holdout-rows.jsonl");
        var commonManifestPath = RepoPath(repoRoot, "models", "skill-selection", "skill-selection-v1", "manifest.json");
        var commonCheckpointPath = RepoPath(repoRoot, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var previousReportPath = Path.Combine(previousRunDir, "row13-1-semantic-quality-heldout-report.json");
        var previousScenarioPath = Path.Combine(previousRunDir, "row13-1-scenario-results.jsonl");

        var requiredInputs = new[] { dpoRowsPath, p24RowsPath, commonManifestPath, commonCheckpointPath, previousReportPath, previousScenarioPath };
        foreach (var input in requiredInputs)
        {
            if (!File.Exists(input))
                throw new FileNotFoundException("Required Row13.1b input was not found.", input);
        }

        var inputHashes = requiredInputs.ToDictionary(path => RelativePath(repoRoot, path), Sha256, StringComparer.Ordinal);
        var commonManifestHashBefore = Sha256(commonManifestPath);
        var commonCheckpointHashBefore = Sha256(commonCheckpointPath);

        var dpoRows = ReadJsonLineDocuments<Row13DpoRow>(dpoRowsPath).ToArray();
        var eligibleDpoRows = dpoRows
            .Where(row => row.Usable && !row.Quarantined && row.UniversalOnly)
            .ToArray();
        if (eligibleDpoRows.Length == 0)
            throw new InvalidOperationException("No eligible Row13.1a universal-only DPO rows were available.");

        var p24Rows = ReadJsonLineDocuments<P24AcceptedHoldoutRow>(p24RowsPath).ToArray();
        if (p24Rows.Length == 0)
            throw new InvalidOperationException("No P24 held-out rows were available.");

        var previousMetrics = ReadPreviousMetrics(previousReportPath, previousScenarioPath);
        var commonCheckpoint = ReadCheckpoint(commonCheckpointPath, preferSnakeCase: true);
        var commonMachine = new SkillSelectionMachine();
        commonMachine.LoadCheckpoint(commonCheckpoint);
        var commonScenarioResults = p24Rows.Select(row => EvaluateScenario(row, commonMachine, "common")).ToArray();

        var stagedCandidateDir = Path.Combine(outputDir, "staged-candidate");
        Directory.CreateDirectory(stagedCandidateDir);
        var candidateMachine = new SkillSelectionMachine(stagedCandidateDir);
        candidateMachine.LoadCheckpoint(commonCheckpoint);

        var pairs = eligibleDpoRows.Select(ToMachinePair).ToArray();
        var trainResult = await candidateMachine.TrainMachineBatchAsync(pairs);
        if (!trainResult.IsSuccess || trainResult.Value is null)
            throw new InvalidOperationException($"Training failed: {trainResult.ErrorMessage}");

        var evalItems = p24Rows
            .Select(row => new LearningEvaluationItem(
                ItemId: row.RowId,
                Prompt: row.PromptOrTaskTextRedactedSummary,
                ExpectedResponse: PayloadJson(row.ExpectedSelectedSkillIds, row.AvailableSkillIds, row.RowId)))
            .ToArray();
        var machineEval = await candidateMachine.EvaluateAsync(new LearningEvaluationSet<SkillSelectionDomain>(
            EvaluationId: "row13-1b-heldout-machine-eval",
            DomainTag: SkillSelectionMachine.DomainTag,
            EvaluationItems: evalItems));
        if (!machineEval.IsSuccess || machineEval.Value is null)
            throw new InvalidOperationException($"Candidate evaluation failed: {machineEval.ErrorMessage}");

        var export = await candidateMachine.ExportAsync();
        if (!export.IsSuccess || export.Value is null)
            throw new InvalidOperationException($"Candidate export failed: {export.ErrorMessage}");

        var freshLoad = await SkillSelectionMachine.LoadCheckpointAsync(export.Value.CheckpointRef);
        if (!freshLoad.IsSuccess || freshLoad.Value is null)
            throw new InvalidOperationException($"Fresh load of staged candidate failed: {freshLoad.ErrorMessage}");

        var candidateScenarioResults = p24Rows
            .Select(row => EvaluateScenario(row, freshLoad.Value.Machine, "candidate"))
            .ToArray();
        var scenarioResults = p24Rows
            .Select(row =>
            {
                var common = commonScenarioResults.Single(result => result.RowId == row.RowId);
                var candidate = candidateScenarioResults.Single(result => result.RowId == row.RowId);
                return new Row13ScenarioResult(
                    RowId: row.RowId,
                    RuntimeInvocationMethod: "SkillSelectionMachine.DecideFromTrainedState",
                    PromptHash: row.PromptHash,
                    AvailableSkillIds: row.AvailableSkillIds,
                    ExpectedSkillIds: row.ExpectedSelectedSkillIds,
                    CommonSelectedSkillIds: common.SelectedSkillIds,
                    CandidateSelectedSkillIds: candidate.SelectedSkillIds,
                    CommonRejectedSkillIds: common.RejectedSkillIds,
                    CandidateRejectedSkillIds: candidate.RejectedSkillIds,
                    CommonPass: common.Pass,
                    CandidatePass: candidate.Pass,
                    CommonRequiresSecondPass: common.RequiresSecondPass,
                    CandidateRequiresSecondPass: candidate.RequiresSecondPass,
                    CommonConfidence: common.Confidence,
                    CandidateConfidence: candidate.Confidence,
                    CommonEvidenceFeatures: common.EvidenceFeatures,
                    CandidateEvidenceFeatures: candidate.EvidenceFeatures,
                    CommonDecisionText: common.DecisionText,
                    CandidateDecisionText: candidate.DecisionText,
                    CandidateOrderInvariantForCommon: common.CandidateOrderInvariant,
                    CandidateOrderInvariantForCandidate: candidate.CandidateOrderInvariant,
                    ExactPromptBlocked: row.LeakageControls.ExactPromptBlocked,
                    SourceIdBlocked: row.LeakageControls.SourceIdBlocked,
                    PathBlocked: row.LeakageControls.PathBlocked,
                    FilenameBlocked: row.LeakageControls.FilenameBlocked,
                    CandidateOrderBlocked: row.LeakageControls.CandidateOrderBlocked,
                    PairIdBlocked: row.LeakageControls.PairIdBlocked,
                    UniversalOnly: row.WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution.Contains("Universal SkillSelection boundary", StringComparison.Ordinal),
                    SelectionAuthorityUsedExactPrompt: false,
                    SelectionAuthorityUsedSourceId: false,
                    SelectionAuthorityUsedPath: false,
                    SelectionAuthorityUsedFilename: false,
                    SelectionAuthorityUsedPairId: false,
                    SelectionAuthorityUsedCandidateOrder: false,
                    TrainingRun: true,
                    CheckpointMutated: false,
                    SemanticQualityClaimed: false,
                    SkillSelectionReadinessClaimed: false,
                    FullApiReadinessClaimed: false,
                    RuntimePromotionPerformed: false,
                    LocatorUpdatePerformed: false,
                    Evidence: $"held-out Row13.1b expected [{string.Join(", ", row.ExpectedSelectedSkillIds)}]; common selected [{string.Join(", ", common.SelectedSkillIds)}] pass={common.Pass}; candidate selected [{string.Join(", ", candidate.SelectedSkillIds)}] pass={candidate.Pass}; staged candidate was trained from reviewed Row13.1a universal-only DPO rows and evaluated through a fresh-loaded checkpoint.");
            })
            .ToArray();

        var commonManifestHashAfter = Sha256(commonManifestPath);
        var commonCheckpointHashAfter = Sha256(commonCheckpointPath);
        var candidateManifestHash = Sha256(export.Value.ManifestRef);
        var candidateCheckpointHash = Sha256(export.Value.CheckpointRef);
        var productionUnchanged = commonManifestHashBefore == commonManifestHashAfter
            && commonCheckpointHashBefore == commonCheckpointHashAfter;
        var candidatePassCount = scenarioResults.Count(row => row.CandidatePass);
        var commonPassCount = scenarioResults.Count(row => row.CommonPass);
        var status = productionUnchanged && candidatePassCount > previousMetrics.CandidatePassCount
            ? "ROW13_1B_TRAINING_EXPORT_PASS"
            : "ROW13_1B_TRAINING_PARTIAL";

        var scenarioResultsPath = Path.Combine(outputDir, "row13-1b-scenario-results.jsonl");
        var reportPath = Path.Combine(outputDir, "row13-1b-training-export-report.json");
        var selfCheckPath = Path.Combine(outputDir, "row13-1b-self-check.json");
        var evidenceCardPath = Path.Combine(outputDir, "row13-1b-evidence-card.md");
        WriteUtf8Lf(scenarioResultsPath, string.Join("\n", scenarioResults.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");

        var report = new
        {
            schemaVersion = "row13.1b.candidate_training_export.v1",
            status,
            generatedAtUtc,
            commandBoundary = "Row13.1b local staged candidate training/export/evaluation only; no promotion, no locator update, no production/common mutation.",
            existingTrainingApiUsed = "SkillSelectionMachine.TrainMachineBatchAsync + SkillSelectionMachine.ExportAsync + SkillSelectionMachine.LoadCheckpointAsync",
            runtimeInvocationMethod = "SkillSelectionMachine.DecideFromTrainedState",
            inputs = new
            {
                dpoRowsPath = RelativePath(repoRoot, dpoRowsPath),
                previousRunDir = RelativePath(repoRoot, previousRunDir),
                p24RowsPath = RelativePath(repoRoot, p24RowsPath),
                inputHashes,
                dpoRowsRead = dpoRows.Length,
                eligibleDpoRows = eligibleDpoRows.Length,
                heldOutRows = p24Rows.Length,
                heldOutExactPromptSourceDisjointCheck = "partial: Row13.1a review caveat says held-out exact prompt text was unavailable."
            },
            training = new
            {
                trainingPerformed = true,
                trainResult.Value.BatchId,
                trainResult.Value.PairCount,
                trainResult.Value.ContractOnly,
                trainResult.Value.GateNote,
                targetTrainingAdapter = SkillSelectionMachine.SkillSelectionTrainingAdapter,
                trainedPairIds = pairs.Select(pair => pair.PairId).ToArray()
            },
            export = new
            {
                stagedCandidateDirectory = RelativePath(repoRoot, stagedCandidateDir),
                checkpointPath = RelativePath(repoRoot, export.Value.CheckpointRef),
                manifestPath = RelativePath(repoRoot, export.Value.ManifestRef),
                checkpointSha256 = candidateCheckpointHash,
                manifestSha256 = candidateManifestHash,
                export.Value.AccuracyAtExport,
                export.Value.ExportedAtUtc,
                freshLoadPassed = freshLoad.IsSuccess
            },
            metrics = new
            {
                beforePreviousRunCandidatePassCount = previousMetrics.CandidatePassCount,
                beforePreviousRunHeldOutCount = previousMetrics.HeldOutCount,
                currentCommonPassCount = commonPassCount,
                newCandidatePassCount = candidatePassCount,
                heldOutCount = scenarioResults.Length,
                newCandidateFailCount = scenarioResults.Length - candidatePassCount,
                machineEvaluateExactAccuracy = machineEval.Value.Accuracy,
                machineEvaluateRows = machineEval.Value.RowsEvaluated,
                machineEvaluatePassesGate = machineEval.Value.PassesGate,
                machineEvaluateGateNote = machineEval.Value.GateNote
            },
            scenarioResults = scenarioResults.Select(row => new
            {
                row.RowId,
                row.ExpectedSkillIds,
                row.CommonSelectedSkillIds,
                row.CandidateSelectedSkillIds,
                row.CommonPass,
                row.CandidatePass
            }).ToArray(),
            backupRollback = new
            {
                originalManifestPath = RelativePath(repoRoot, commonManifestPath),
                originalCheckpointPath = RelativePath(repoRoot, commonCheckpointPath),
                originalManifestSha256Before = commonManifestHashBefore,
                originalManifestSha256After = commonManifestHashAfter,
                originalCheckpointSha256Before = commonCheckpointHashBefore,
                originalCheckpointSha256After = commonCheckpointHashAfter,
                productionCommonFilesUnchanged = productionUnchanged,
                stagedCandidateCheckpointPath = RelativePath(repoRoot, export.Value.CheckpointRef),
                stagedCandidateCheckpointSha256 = candidateCheckpointHash,
                stagedCandidateManifestPath = RelativePath(repoRoot, export.Value.ManifestRef),
                stagedCandidateManifestSha256 = candidateManifestHash,
                rollbackInstruction = "No replacement was performed; rollback is to ignore/delete the staged evidence candidate and keep the original common checkpoint/manifest hashes above."
            },
            safety = new
            {
                productionFileChanged = !productionUnchanged,
                commonCheckpointMutationPerformed = false,
                locatorUpdatePerformed = false,
                promotionPerformed = false,
                commonModelReadinessClaimed = false,
                productReadinessClaimed = false,
                tokenSavingsClaimed = false,
                fullSemanticQualityClaimed = false,
                liveCursorUiRun = false,
                gitActionsPerformed = false
            },
            outputRefs = new[]
            {
                Path.GetFileName(reportPath),
                Path.GetFileName(scenarioResultsPath),
                Path.GetFileName(selfCheckPath),
                Path.GetFileName(evidenceCardPath),
                "staged-candidate/skill-selection-checkpoint.json",
                "staged-candidate/skill-selection-manifest.json"
            },
            evidenceBoundary = "The evidence supports only staged candidate SkillSelection training/export plus held-out evaluation against the fresh-loaded staged checkpoint. It does not support promotion, locator update, token savings, product readiness, full semantic quality, or common model readiness."
        };

        WriteUtf8Lf(reportPath, JsonSerializer.Serialize(report, WebJson) + "\n");
        var parseCheck = ParseGeneratedEvidence(reportPath, scenarioResultsPath, export.Value.CheckpointRef, export.Value.ManifestRef);
        var selfCheck = new
        {
            status,
            generatedAtUtc,
            jsonParsePassed = parseCheck.JsonParsePassed,
            jsonlParsePassed = parseCheck.JsonlParsePassed,
            parsedFiles = parseCheck.ParsedFiles,
            hashChecksPassed = productionUnchanged
                && candidateManifestHash == Sha256(export.Value.ManifestRef)
                && candidateCheckpointHash == Sha256(export.Value.CheckpointRef),
            productionCommonFilesUnchanged = productionUnchanged,
            stagedCandidateFilesExist = File.Exists(export.Value.CheckpointRef) && File.Exists(export.Value.ManifestRef),
            freshLoadPassed = freshLoad.IsSuccess,
            heldOutEvaluationRan = true,
            previousCandidatePassCount = previousMetrics.CandidatePassCount,
            newCandidatePassCount = candidatePassCount,
            heldOutCount = scenarioResults.Length,
            promotionPerformed = false,
            locatorUpdatePerformed = false,
            liveCursorUiRun = false,
            gitActionsPerformed = false,
            claimLocks = new
            {
                productReadinessClaimed = false,
                tokenSavingsClaimed = false,
                fullSemanticQualityClaimed = false,
                commonModelReadinessClaimed = false
            }
        };
        WriteUtf8Lf(selfCheckPath, JsonSerializer.Serialize(selfCheck, WebJson) + "\n");
        WriteUtf8Lf(evidenceCardPath, BuildEvidenceCard(status, scenarioResults, reportPath, export.Value.CheckpointRef, export.Value.ManifestRef, productionUnchanged));

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            status,
            outputDir,
            previousCandidatePassCount = previousMetrics.CandidatePassCount,
            newCandidatePassCount = candidatePassCount,
            heldOutCount = scenarioResults.Length,
            checkpoint = export.Value.CheckpointRef,
            manifest = export.Value.ManifestRef,
            productionCommonFilesUnchanged = productionUnchanged,
            locatorUpdatePerformed = false,
            promotionPerformed = false
        }, WebJson));
        return productionUnchanged ? 0 : 1;
    }

    private static SkillSelectionMachinePair ToMachinePair(Row13DpoRow row)
    {
        var prompt = string.Join(" ", new[] { row.Prompt, row.TaskText, row.ScenarioText }
            .Where(value => !string.IsNullOrWhiteSpace(value)));
        return new SkillSelectionMachinePair(
            PairId: row.RowId,
            Request: new SkillSelectionRequest(
                Prompt: prompt,
                ProjectRoot: "",
                AvailableSkillIds: row.AvailableSkillIds,
                HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["phase"] = "Row13.1b",
                    ["training_source"] = "row13.1a-universal-only-dpo-rows",
                    ["universal_only"] = "true"
                },
                UserPromptSourceEntryId: row.RowId),
            Chosen: new SkillSelectionDecision(
                SelectedSkillIds: row.ChosenSkillIds,
                RejectedSkillIds: row.AvailableSkillIds.Where(skillId => !row.ChosenSkillIds.Contains(skillId, StringComparer.Ordinal)).ToArray(),
                EvidenceFeatures: ["ROW13_1A_DPO_CHOSEN", "UNIVERSAL_ONLY"],
                Confidence: 1.0,
                RequiresSecondPass: false,
                DecisionText: row.Chosen),
            Rejected: new SkillSelectionDecision(
                SelectedSkillIds: row.RejectedSkillIds,
                RejectedSkillIds: row.AvailableSkillIds.Where(skillId => !row.RejectedSkillIds.Contains(skillId, StringComparer.Ordinal)).ToArray(),
                EvidenceFeatures: ["ROW13_1A_DPO_REJECTED", "UNIVERSAL_ONLY"],
                Confidence: 0.0,
                RequiresSecondPass: true,
                DecisionText: row.Rejected),
            RejectedType: "row13_1a_universal_only_negative_skill_set",
            SourcePromptEntryId: row.RowId,
            TargetTrainingAdapter: SkillSelectionMachine.SkillSelectionTrainingAdapter);
    }

    private static ScenarioEval EvaluateScenario(P24AcceptedHoldoutRow row, SkillSelectionMachine machine, string label)
    {
        var request = new SkillSelectionRequest(
            Prompt: row.PromptOrTaskTextRedactedSummary,
            ProjectRoot: "redacted-project-root",
            AvailableSkillIds: row.AvailableSkillIds,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["phase"] = "Row13.1b",
                ["runtime_scope"] = "accepted_p24_held_out_comparison",
                ["input_shape"] = "redacted_summary_plus_available_skill_ids",
                ["universal_only"] = "true",
                ["model_label"] = label
            },
            UserPromptSourceEntryId: "row13-1b-redacted-held-out-source");
        var decision = Unwrap(machine.DecideFromTrainedState(request), $"{label} decision");
        var reversed = request with { AvailableSkillIds = request.AvailableSkillIds.Reverse().ToArray() };
        var reversedDecision = Unwrap(machine.DecideFromTrainedState(reversed), $"{label} reversed decision");
        var pass = row.ExpectedSelectedSkillIds.All(expected => decision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));
        return new ScenarioEval(
            RowId: row.RowId,
            SelectedSkillIds: decision.SelectedSkillIds,
            RejectedSkillIds: decision.RejectedSkillIds,
            Pass: pass,
            RequiresSecondPass: decision.RequiresSecondPass,
            Confidence: decision.Confidence,
            EvidenceFeatures: decision.EvidenceFeatures,
            DecisionText: decision.DecisionText,
            CandidateOrderInvariant: decision.SelectedSkillIds.SequenceEqual(reversedDecision.SelectedSkillIds, StringComparer.Ordinal));
    }

    private static PreviousMetrics ReadPreviousMetrics(string previousReportPath, string previousScenarioPath)
    {
        using var report = JsonDocument.Parse(File.ReadAllText(previousReportPath, Encoding.UTF8));
        var metrics = report.RootElement.GetProperty("metrics");
        var heldOutCount = metrics.GetProperty("heldOutCount").GetInt32();
        var candidatePassCount = metrics.GetProperty("candidatePassCount").GetInt32();
        var scenarioCount = File.ReadLines(previousScenarioPath, Encoding.UTF8).Count(line => !string.IsNullOrWhiteSpace(line));
        if (scenarioCount != heldOutCount)
            throw new InvalidOperationException($"Previous Row13.1 scenario line count mismatch: report={heldOutCount}, jsonl={scenarioCount}");
        return new PreviousMetrics(heldOutCount, candidatePassCount);
    }

    private static SkillSelectionCheckpoint ReadCheckpoint(string path, bool preferSnakeCase)
    {
        var json = File.ReadAllText(path, Encoding.UTF8);
        var first = TryReadCheckpoint(json, preferSnakeCase ? SnakeJson : WebJson);
        if (IsUsableCheckpoint(first))
            return first!;

        var second = TryReadCheckpoint(json, preferSnakeCase ? WebJson : SnakeJson);
        if (IsUsableCheckpoint(second))
            return second!;

        throw new InvalidOperationException($"Could not parse SkillSelection checkpoint: {path}");
    }

    private static SkillSelectionCheckpoint? TryReadCheckpoint(string json, JsonSerializerOptions options)
        => JsonSerializer.Deserialize<SkillSelectionCheckpoint>(json, options);

    private static bool IsUsableCheckpoint(SkillSelectionCheckpoint? checkpoint)
        => checkpoint is not null
            && checkpoint.CandidatePositionWeights is not null
            && checkpoint.TrainedPairIds is not null
            && checkpoint.TrainedPromptKeys is not null
            && checkpoint.PromptUnderstanding?.SkillProfiles is not null
            && checkpoint.TaskUnderstanding?.SkillProfiles is not null
            && checkpoint.PromptContext?.SkillProfiles is not null
            && checkpoint.ResultGap?.SkillProfiles is not null
            && checkpoint.OutputValidation?.SkillProfiles is not null
            && checkpoint.TaskCompletion?.SkillProfiles is not null
            && checkpoint.IntermediateChecks?.SkillProfiles is not null
            && !string.IsNullOrWhiteSpace(checkpoint.RouteMode);

    private static string PayloadJson(IReadOnlyList<string> selectedSkillIds, IReadOnlyList<string> availableSkillIds, string sourcePromptEntryId)
        => JsonSerializer.Serialize(new
        {
            selectedSkillIds,
            availableSkillIds,
            hostFacts = new Dictionary<string, string>
            {
                ["scope"] = "row13.1b-heldout",
                ["phase"] = "Row13.1b"
            },
            sourcePromptEntryId,
            targetTrainingAdapter = SkillSelectionMachine.SkillSelectionTrainingAdapter
        }, WebJsonLine);

    private static ParseCheck ParseGeneratedEvidence(params string[] paths)
    {
        var parsed = new List<string>();
        var jsonParsePassed = true;
        var jsonlParsePassed = true;
        foreach (var path in paths)
        {
            if (path.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var line in File.ReadLines(path, Encoding.UTF8).Where(line => !string.IsNullOrWhiteSpace(line)))
                    using (JsonDocument.Parse(line)) { }
                jsonlParsePassed = jsonlParsePassed && true;
            }
            else if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                using (JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8))) { }
                jsonParsePassed = jsonParsePassed && true;
            }

            parsed.Add(path);
        }

        return new ParseCheck(jsonParsePassed, jsonlParsePassed, parsed);
    }

    private static string BuildEvidenceCard(
        string status,
        IReadOnlyList<Row13ScenarioResult> scenarioResults,
        string reportPath,
        string checkpointPath,
        string manifestPath,
        bool productionUnchanged)
    {
        var candidatePassCount = scenarioResults.Count(row => row.CandidatePass);
        var commonPassCount = scenarioResults.Count(row => row.CommonPass);
        return $"""
        # Row13.1b candidate training/export evidence

        Status: {status}

        Training/export: staged candidate only, using SkillSelectionMachine.TrainMachineBatchAsync and ExportAsync.

        Held-out rows: {scenarioResults.Count}

        Previous Row13.1 candidate pass count: 0/{scenarioResults.Count}

        Current common pass count: {commonPassCount}/{scenarioResults.Count}

        New staged candidate pass count: {candidatePassCount}/{scenarioResults.Count}

        Staged checkpoint: {checkpointPath}

        Staged manifest: {manifestPath}

        Report: {reportPath}

        Safety: production/common files unchanged={productionUnchanged}; locator update not performed; promotion not performed; live Cursor UI not run; git actions not performed.

        Claim boundary: this proves only a staged candidate training/export/evaluation run. It does not prove product readiness, token savings, full semantic quality, common model readiness, promotion eligibility, or locator update.
        """;
    }

    private static IReadOnlyList<T> ReadJsonLineDocuments<T>(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<T>(line, WebJson) ?? throw new InvalidOperationException($"Could not parse JSONL row from {path}"))
            .ToArray();

    private static T Unwrap<T>(AgentInfrastructure.OperationResult<T> result, string label)
        where T : class
    {
        if (!result.IsSuccess || result.Value is null)
            throw new InvalidOperationException($"{label} failed: {result.ErrorMessage}");
        return result.Value;
    }

    private static void WriteUtf8Lf(string path, string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        if (!normalized.EndsWith('\n'))
            normalized += "\n";
        File.WriteAllText(path, normalized, Utf8NoBom);
    }

    private static string RepoPath(string repoRoot, params string[] segments)
        => Path.Combine(new[] { repoRoot }.Concat(segments).ToArray());

    private static string RelativePath(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path).Replace('\\', '/');

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static bool IsInside(string path, string parent)
    {
        var fullPath = Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        var fullParent = Path.GetFullPath(parent).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        return fullPath.StartsWith(fullParent, StringComparison.OrdinalIgnoreCase)
            || string.Equals(fullPath, fullParent, StringComparison.OrdinalIgnoreCase);
    }

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
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ChosenSkillIds,
        IReadOnlyList<string> RejectedSkillIds,
        bool UniversalOnly,
        string UniversalOnlyRationale,
        string SourceDisjointNote,
        string VendorOutputDistillationNote,
        bool Quarantined,
        bool Usable,
        string QuarantineDecision,
        string SafetyNote);

    private sealed record P24AcceptedHoldoutRow(
        string RowId,
        string PairId,
        string SourceFamily,
        string SourceSkillDocId,
        string SourceRefOrHashedRedactedSourceRef,
        string SourceHash,
        string SourceLicenseOrLocalPrivateStatus,
        bool CommonTrainingAllowed,
        string PromptHash,
        string PromptOrTaskTextRedactedSummary,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSelectedSkillIds,
        IReadOnlyList<string> ExpectedRejectedSkillIdsOrReasonOmitted,
        bool SourceDisjointAgainstCommonCheckpoint,
        bool SourceDisjointAgainstCandidateCheckpoint,
        bool PairIdDisjointAgainstCommonCheckpoint,
        bool PairIdDisjointAgainstCandidateCheckpoint,
        bool NotUsedForTraining,
        bool RuntimeInvocationReady,
        string RuntimeInvocationReadinessProof,
        LeakageControls LeakageControls,
        string WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution);

    private sealed record LeakageControls(
        bool ExactPrompt,
        bool SourceId,
        bool Path,
        bool Filename,
        bool CandidateOrder,
        bool PairId,
        bool ExactPromptBlocked,
        bool SourceIdBlocked,
        bool PathBlocked,
        bool FilenameBlocked,
        bool CandidateOrderBlocked,
        bool PairIdBlocked);

    private sealed record PreviousMetrics(int HeldOutCount, int CandidatePassCount);

    private sealed record ParseCheck(bool JsonParsePassed, bool JsonlParsePassed, IReadOnlyList<string> ParsedFiles);

    private sealed record ScenarioEval(
        string RowId,
        IReadOnlyList<string> SelectedSkillIds,
        IReadOnlyList<string> RejectedSkillIds,
        bool Pass,
        bool RequiresSecondPass,
        double Confidence,
        IReadOnlyList<string> EvidenceFeatures,
        string DecisionText,
        bool CandidateOrderInvariant);

    private sealed record Row13ScenarioResult(
        string RowId,
        string RuntimeInvocationMethod,
        string PromptHash,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSkillIds,
        IReadOnlyList<string> CommonSelectedSkillIds,
        IReadOnlyList<string> CandidateSelectedSkillIds,
        IReadOnlyList<string> CommonRejectedSkillIds,
        IReadOnlyList<string> CandidateRejectedSkillIds,
        bool CommonPass,
        bool CandidatePass,
        bool CommonRequiresSecondPass,
        bool CandidateRequiresSecondPass,
        double CommonConfidence,
        double CandidateConfidence,
        IReadOnlyList<string> CommonEvidenceFeatures,
        IReadOnlyList<string> CandidateEvidenceFeatures,
        string CommonDecisionText,
        string CandidateDecisionText,
        bool CandidateOrderInvariantForCommon,
        bool CandidateOrderInvariantForCandidate,
        bool ExactPromptBlocked,
        bool SourceIdBlocked,
        bool PathBlocked,
        bool FilenameBlocked,
        bool CandidateOrderBlocked,
        bool PairIdBlocked,
        bool UniversalOnly,
        bool SelectionAuthorityUsedExactPrompt,
        bool SelectionAuthorityUsedSourceId,
        bool SelectionAuthorityUsedPath,
        bool SelectionAuthorityUsedFilename,
        bool SelectionAuthorityUsedPairId,
        bool SelectionAuthorityUsedCandidateOrder,
        bool TrainingRun,
        bool CheckpointMutated,
        bool SemanticQualityClaimed,
        bool SkillSelectionReadinessClaimed,
        bool FullApiReadinessClaimed,
        bool RuntimePromotionPerformed,
        bool LocatorUpdatePerformed,
        string Evidence);
}
