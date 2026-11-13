using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AgentInfrastructure.Logical;
using SmartSkillsDomain;

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
        if (args.Length < 2)
        {
            Console.Error.WriteLine("Usage: Row13TrainingDpoSlice <repo-root> <evidence-output-dir>");
            return 2;
        }

        var repoRoot = Path.GetFullPath(args[0]);
        var outputDir = Path.GetFullPath(args[1]);
        Directory.CreateDirectory(outputDir);

        if (!File.Exists(Path.Combine(repoRoot, "LlmFromScratch.sln")))
            throw new InvalidOperationException($"Repository root not found: {repoRoot}");

        var allowedRoot = RepoPath(repoRoot, "docs", "plans", "evidence", "extension-full-readiness-dual-path", "scenario-execution", "frdp-p01-cursor-semantic-quality-heldout");
        if (!IsInside(outputDir, allowedRoot))
            throw new InvalidOperationException($"Output directory must stay inside Row13.1 evidence root: {allowedRoot}");

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var p24RowsPath = RepoPath(repoRoot, "artifacts", "smart-skills-semantic-quality-completion", "p24-skillselection-runtime-holdout-rows", "p24-runtime-holdout-rows.jsonl");
        var commonManifestPath = RepoPath(repoRoot, "models", "skill-selection", "skill-selection-v1", "manifest.json");
        var commonCheckpointPath = RepoPath(repoRoot, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var originalCandidateManifestPath = RepoPath(repoRoot, "artifacts", "smart-skills-semantic-quality-completion", "p17e-skillselection-lifecycle-import-export", "production-export", "skill-selection-manifest.json");
        var originalCandidateCheckpointPath = RepoPath(repoRoot, "artifacts", "smart-skills-semantic-quality-completion", "p17e-skillselection-lifecycle-import-export", "production-export", "skill-selection-checkpoint.json");
        var locatorPaths = DiscoverLocatorFiles(repoRoot).ToArray();

        foreach (var requiredPath in new[] { p24RowsPath, commonManifestPath, commonCheckpointPath, originalCandidateManifestPath, originalCandidateCheckpointPath })
        {
            if (!File.Exists(requiredPath))
                throw new FileNotFoundException("Required input was not found.", requiredPath);
        }

        var safetyBefore = SnapshotHashes(commonManifestPath, commonCheckpointPath, originalCandidateManifestPath, originalCandidateCheckpointPath, locatorPaths);
        var p24Rows = ReadJsonLineDocuments<P24AcceptedHoldoutRow>(p24RowsPath).ToArray();
        if (p24Rows.Length != 4)
            throw new InvalidOperationException($"Expected exactly 4 P24 held-out rows, found {p24Rows.Length}.");

        var dpoRows = BuildDpoRows(p24Rows).ToArray();
        var dpoPath = Path.Combine(outputDir, "row13-1-candidate-only-dpo-rows.jsonl");
        WriteJsonl(dpoPath, dpoRows);

        var commonMachine = new SkillSelectionMachine();
        commonMachine.LoadCheckpoint(ReadCheckpoint(commonCheckpointPath, preferSnakeCase: true));

        var originalCandidateLoad = await SkillSelectionMachine.LoadCheckpointAsync(originalCandidateCheckpointPath);
        if (!originalCandidateLoad.IsSuccess || originalCandidateLoad.Value is null)
            throw new InvalidOperationException(originalCandidateLoad.ErrorMessage);

        var candidateMachine = originalCandidateLoad.Value.Machine;
        var beforeResults = p24Rows.Select(row => EvaluateScenario(row, commonMachine, candidateMachine, "original-candidate-before-training")).ToArray();
        var trainItems = dpoRows.Select(ToLearningTrainingItem).ToArray();
        var batch = new LearningBatch<SkillSelectionDomain>(
            BatchId: "row13-1-candidate-only-skill-selection-dpo-slice",
            DomainTag: SkillSelectionMachine.DomainTag,
            TrainingItems: trainItems,
            SourceIds: dpoRows.Select(row => row.SourcePromptEntryId).ToArray());

        var trainResult = await candidateMachine.TrainBatchAsync(batch);
        if (!trainResult.IsSuccess || trainResult.Value is null)
            throw new InvalidOperationException(trainResult.ErrorMessage);

        var evalSet = new LearningEvaluationSet<SkillSelectionDomain>(
            EvaluationId: "row13-1-candidate-only-p24-heldout-eval",
            DomainTag: SkillSelectionMachine.DomainTag,
            EvaluationItems: p24Rows.Select(ToLearningEvaluationItem).ToArray());
        var evalResult = await candidateMachine.EvaluateAsync(evalSet);
        if (!evalResult.IsSuccess || evalResult.Value is null)
            throw new InvalidOperationException(evalResult.ErrorMessage);

        var stagedDir = Path.Combine(outputDir, "staged-candidate-checkpoint");
        Directory.CreateDirectory(stagedDir);
        var stagedCheckpointPath = Path.Combine(stagedDir, "skill-selection-checkpoint.json");
        var stagedManifestPath = Path.Combine(stagedDir, "skill-selection-manifest.json");
        var exportResult = await candidateMachine.SaveCheckpointAsync(stagedCheckpointPath, stagedManifestPath);
        if (!exportResult.IsSuccess || exportResult.Value is null)
            throw new InvalidOperationException(exportResult.ErrorMessage);

        var stagedLoad = await SkillSelectionMachine.LoadCheckpointAsync(stagedCheckpointPath);
        if (!stagedLoad.IsSuccess || stagedLoad.Value is null)
            throw new InvalidOperationException(stagedLoad.ErrorMessage);

        var afterResults = p24Rows.Select(row => EvaluateScenario(row, commonMachine, stagedLoad.Value.Machine, "staged-candidate-after-training")).ToArray();
        var freshEval = await stagedLoad.Value.Machine.EvaluateAsync(evalSet);
        if (!freshEval.IsSuccess || freshEval.Value is null)
            throw new InvalidOperationException(freshEval.ErrorMessage);

        var commonPassCount = afterResults.Count(row => row.CommonPass);
        var beforeCandidatePassCount = beforeResults.Count(row => row.CandidatePass);
        var afterCandidatePassCount = afterResults.Count(row => row.CandidatePass);
        var status = afterCandidatePassCount == p24Rows.Length
            ? "ROW13_1_TRAINING_PASS"
            : afterCandidatePassCount > 0
                ? "ROW13_1_TRAINING_PARTIAL"
                : "ROW13_1_TRAINING_NEEDS_IMPLEMENTATION";

        var safetyAfter = SnapshotHashes(commonManifestPath, commonCheckpointPath, originalCandidateManifestPath, originalCandidateCheckpointPath, locatorPaths);
        var unchanged = CompareSnapshots(safetyBefore, safetyAfter);
        var stagedCheckpointHash = Sha256(stagedCheckpointPath);
        var stagedManifestHash = Sha256(stagedManifestPath);

        var scenarioResultsPath = Path.Combine(outputDir, "row13-1-candidate-scenario-results.jsonl");
        var trainingReportPath = Path.Combine(outputDir, "row13-1-training-report.json");
        var trainingReportMdPath = Path.Combine(outputDir, "row13-1-training-report.md");
        var backupRollbackPath = Path.Combine(outputDir, "backup-rollback-evidence.json");
        var selfCheckPath = Path.Combine(outputDir, "row13-1-training-self-check.json");

        WriteJsonl(scenarioResultsPath, afterResults);
        WriteJson(backupRollbackPath, new
        {
            schemaVersion = "row13.1.backup_rollback_evidence.v1",
            originalCandidateCheckpointPath = RelativePath(repoRoot, originalCandidateCheckpointPath),
            originalCandidateCheckpointSha256Before = safetyBefore.OriginalCandidateCheckpointSha256,
            originalCandidateCheckpointSha256After = safetyAfter.OriginalCandidateCheckpointSha256,
            originalCandidateManifestPath = RelativePath(repoRoot, originalCandidateManifestPath),
            originalCandidateManifestSha256Before = safetyBefore.OriginalCandidateManifestSha256,
            originalCandidateManifestSha256After = safetyAfter.OriginalCandidateManifestSha256,
            stagedCandidateCheckpointPath = RelativePath(repoRoot, stagedCheckpointPath),
            stagedCandidateCheckpointSha256 = stagedCheckpointHash,
            stagedCandidateManifestPath = RelativePath(repoRoot, stagedManifestPath),
            stagedCandidateManifestSha256 = stagedManifestHash,
            rollbackInstruction = "Discard the evidence leaf staged-candidate-checkpoint folder; the original candidate checkpoint and production/common files were not changed by this run.",
            promotionPerformed = false,
            locatorUpdatePerformed = false,
            productionLocatorOrCheckpointChanged = !unchanged.AllTrackedInputsUnchanged
        });

        var report = new
        {
            schemaVersion = "row13.1.candidate_only_training_report.v1",
            status,
            generatedAtUtc,
            boundary = "candidate-only SkillSelection DPO slice; no production/common checkpoint mutation, no promotion, no locator update, no live Cursor UI",
            subagentId = "Row13.1-training-bounded-subagent",
            evidenceRoot = RelativePath(repoRoot, outputDir),
            dpoRows = new
            {
                rowCount = dpoRows.Length,
                targetSkills = dpoRows.Select(row => row.SkillId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
                rowsPath = RelativePath(repoRoot, dpoPath),
                examplesSummary = dpoRows
                    .GroupBy(row => row.SkillId)
                    .OrderBy(group => group.Key, StringComparer.Ordinal)
                    .Select(group => new
                    {
                        skillId = group.Key,
                        count = group.Count(),
                        chosen = group.First().ChosenSkillIds,
                        rejectedExamples = group.First().RejectedSkillIds,
                        universalOnly = group.All(row => row.UniversalOnly),
                        quarantineDecision = group.First().QuarantineDecision,
                        vendorOutputDistillationIssue = group.Any(row => row.VendorOutputDistillationIssue)
                    })
                    .ToArray()
            },
            metrics = new
            {
                heldOutCount = p24Rows.Length,
                commonPassCount,
                commonFailCount = p24Rows.Length - commonPassCount,
                originalCandidatePassCountBeforeTraining = beforeCandidatePassCount,
                originalCandidateFailCountBeforeTraining = p24Rows.Length - beforeCandidatePassCount,
                stagedCandidatePassCountAfterTraining = afterCandidatePassCount,
                stagedCandidateFailCountAfterTraining = p24Rows.Length - afterCandidatePassCount,
                trainRowsTrained = trainResult.Value.RowsTrained,
                trainAccuracyBefore = trainResult.Value.AccuracyBefore,
                trainAccuracyAfter = trainResult.Value.AccuracyAfter,
                trainRewardMargin = trainResult.Value.RewardMargin,
                heldoutAccuracyAfterTraining = evalResult.Value.Accuracy,
                freshLoadHeldoutAccuracy = freshEval.Value.Accuracy,
                candidatePassCountTargetMet = afterCandidatePassCount == p24Rows.Length
            },
            modelSafety = new
            {
                commonManifestPath = RelativePath(repoRoot, commonManifestPath),
                commonCheckpointPath = RelativePath(repoRoot, commonCheckpointPath),
                originalCandidateManifestPath = RelativePath(repoRoot, originalCandidateManifestPath),
                originalCandidateCheckpointPath = RelativePath(repoRoot, originalCandidateCheckpointPath),
                stagedCandidateManifestPath = RelativePath(repoRoot, stagedManifestPath),
                stagedCandidateCheckpointPath = RelativePath(repoRoot, stagedCheckpointPath),
                stagedCandidateManifestSha256 = stagedManifestHash,
                stagedCandidateCheckpointSha256 = stagedCheckpointHash,
                hashesBefore = safetyBefore,
                hashesAfter = safetyAfter,
                unchanged,
                noProductionCommonCheckpointMutation = unchanged.CommonManifestUnchanged && unchanged.CommonCheckpointUnchanged,
                originalCandidateUnchanged = unchanged.OriginalCandidateManifestUnchanged && unchanged.OriginalCandidateCheckpointUnchanged,
                locatorUpdatePerformed = false,
                promotionPerformed = false,
                locatorFilesTracked = locatorPaths.Select(path => RelativePath(repoRoot, path)).ToArray()
            },
            evidenceRefs = new[]
            {
                Path.GetFileName(dpoPath),
                Path.GetFileName(scenarioResultsPath),
                Path.GetFileName(backupRollbackPath),
                Path.GetFileName(selfCheckPath),
                "staged-candidate-checkpoint/skill-selection-checkpoint.json",
                "staged-candidate-checkpoint/skill-selection-manifest.json"
            },
            claimLocks = new
            {
                productReadinessClaimed = false,
                tokenSavingsClaimed = false,
                fullSemanticQualityClaimed = false,
                commonModelReadinessClaimed = false,
                trainingCompleteBeyondCandidateSliceClaimed = false,
                promotionPerformed = false,
                locatorUpdatePerformed = false,
                liveCursorUiRun = false,
                gitActionsPerformed = false
            },
            failureDiagnosis = afterCandidatePassCount == 0
                ? "The staged candidate still selected zero expected Row13.1 held-out skills after candidate-only DPO training; inspect final-selection feature overlap or implement a safer training adapter."
                : null
        };

        WriteJson(trainingReportPath, report);
        WriteText(trainingReportMdPath, BuildMarkdownReport(status, p24Rows.Length, commonPassCount, beforeCandidatePassCount, afterCandidatePassCount, dpoRows, repoRoot, outputDir, stagedCheckpointPath, stagedCheckpointHash, unchanged));

        var parseCheck = ParseWrittenJsonAndJsonl(outputDir);
        var selfCheck = new
        {
            schemaVersion = "row13.1.training_self_check.v1",
            status,
            jsonAndJsonlParseCheck = parseCheck,
            allJsonAndJsonlParsed = parseCheck.AllParsed,
            noPromotion = true,
            noLocatorUpdate = true,
            noGitActionsPerformed = true,
            liveCursorUiRun = false,
            productionCommonAndOriginalCandidateUnchanged = unchanged.AllTrackedInputsUnchanged,
            stagedCandidateCheckpointExists = File.Exists(stagedCheckpointPath),
            stagedCandidateManifestExists = File.Exists(stagedManifestPath),
            reportPath = RelativePath(repoRoot, trainingReportPath)
        };
        WriteJson(selfCheckPath, selfCheck);

        // Reparse after self-check is written.
        parseCheck = ParseWrittenJsonAndJsonl(outputDir);
        WriteJson(selfCheckPath, new
        {
            schemaVersion = "row13.1.training_self_check.v1",
            status,
            jsonAndJsonlParseCheck = parseCheck,
            allJsonAndJsonlParsed = parseCheck.AllParsed,
            noPromotion = true,
            noLocatorUpdate = true,
            noGitActionsPerformed = true,
            liveCursorUiRun = false,
            productionCommonAndOriginalCandidateUnchanged = unchanged.AllTrackedInputsUnchanged,
            stagedCandidateCheckpointExists = File.Exists(stagedCheckpointPath),
            stagedCandidateManifestExists = File.Exists(stagedManifestPath),
            reportPath = RelativePath(repoRoot, trainingReportPath)
        });

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            status,
            dpoRowCount = dpoRows.Length,
            heldOutCount = p24Rows.Length,
            commonPassCount,
            originalCandidatePassCountBeforeTraining = beforeCandidatePassCount,
            stagedCandidatePassCountAfterTraining = afterCandidatePassCount,
            stagedCandidateCheckpointPath = stagedCheckpointPath,
            stagedCandidateCheckpointSha256 = stagedCheckpointHash,
            allTrackedInputsUnchanged = unchanged.AllTrackedInputsUnchanged,
            locatorUpdatePerformed = false,
            promotionPerformed = false,
            outputDir
        }, WebJson));

        return afterCandidatePassCount > 0 ? 0 : 3;
    }

    private static IReadOnlyList<DpoTrainingRow> BuildDpoRows(IReadOnlyList<P24AcceptedHoldoutRow> p24Rows)
    {
        var rows = new List<DpoTrainingRow>();
        AddRows(rows, "e2e-test-matrix", ["test-integrity", "implementation-integrity", "plan-review"],
            [
                "Design an end to end scenario matrix for success paths, idempotent retry behavior, delayed processing, recovery boundaries, and security checks before implementation starts.",
                "Plan a coverage matrix that spans happy path, idempotency, delayed callbacks, recovery behavior, boundary inputs, and security outcomes before any code is written.",
                "When a request asks for scenario coverage shape across success, retry, delay, recovery, boundary, and security cases, select the skill for an e2e test matrix."
            ]);
        AddRows(rows, "no-product-decisions", ["implementation-integrity", "planning-session-startup", "claim-verification"],
            [
                "Before choosing between alternatives that change user behavior, public interface shape, or architecture direction, require a product decision instead of implementing.",
                "A task that asks the agent to decide product behavior or public API direction needs the no product decisions boundary, not an implementation integrity check.",
                "If competing options alter user workflow, visible interface, or architecture direction, select the skill that blocks product decisions by the agent."
            ]);
        AddRows(rows, "output-skepticism", ["phase-completion-packager", "verification-before-completion", "plan-review"],
            [
                "Check a proposed result claim against contradicting evidence, scope validity, and proxy metric risk before reporting success.",
                "When the output asks whether evidence really supports a claim, select the skepticism skill that tests refuting evidence and proxy measure risk.",
                "A completion report with possible scope mismatch, weak proxy metrics, or refuting facts needs output skepticism before packaging the phase."
            ]);
        AddRows(rows, "self-verification", ["test-integrity", "output-skepticism", "implementation-integrity"],
            [
                "After changing an artifact, the implementer must verify its own change with fresh fixtures, blast radius classification, cascade tracing, and prediction matching.",
                "A request for the executor to self check changed work using new fixtures, blast radius, cascade paths, and prediction comparison selects self verification.",
                "When post change proof must be generated by the implementer against fresh cases and expected outcomes, choose the self verification skill."
            ]);

        foreach (var row in rows)
        {
            var matchingP24 = p24Rows.Single(p24 => p24.ExpectedSelectedSkillIds.Contains(row.SkillId, StringComparer.Ordinal));
            row.SourceDisjointNotes = new SourceDisjointNotes(
                UsesP24ExactPrompt: string.Equals(row.Prompt, matchingP24.PromptOrTaskTextRedactedSummary, StringComparison.Ordinal),
                UsesP24PairId: string.Equals(row.RowId, matchingP24.PairId, StringComparison.Ordinal),
                UsesP24SourceRef: false,
                UsesP24PathOrFilename: false,
                Note: "Candidate-only synthetic universal prompt; P24 row remains held-out eval only; no exact P24 prompt, pair id, source ref, path, or filename used.");
        }

        return rows;
    }

    private static void AddRows(List<DpoTrainingRow> rows, string skillId, IReadOnlyList<string> rejectedSkillIds, IReadOnlyList<string> prompts)
    {
        var available = new[] { skillId }.Concat(rejectedSkillIds).Distinct(StringComparer.Ordinal).ToArray();
        for (var i = 0; i < prompts.Count; i++)
        {
            var rowId = $"ROW13_1_TRAIN_{skillId.ToUpperInvariant().Replace("-", "_")}_{i + 1:D3}";
            rows.Add(new DpoTrainingRow
            {
                RowId = rowId,
                SourcePromptEntryId = $"row13-1-candidate-only-src-{skillId}-{i + 1:D3}",
                SkillId = skillId,
                Prompt = prompts[i],
                AvailableSkillIds = available,
                ChosenSkillIds = [skillId],
                RejectedSkillIds = rejectedSkillIds,
                QuarantineDecision = "accepted_for_candidate_only_training; not eligible for common/prod promotion; P24 held-out rows remain quarantined for evaluation only",
                VendorOutputDistillationIssue = false,
                UniversalOnly = true,
                TargetTrainingAdapter = SkillSelectionMachine.SkillSelectionTrainingAdapter
            });
        }
    }

    private static LearningTrainingItem ToLearningTrainingItem(DpoTrainingRow row)
        => new(
            ItemId: row.RowId,
            Prompt: row.Prompt,
            ChosenResponse: PayloadJson(row, row.ChosenSkillIds),
            RejectedResponse: PayloadJson(row, row.RejectedSkillIds));

    private static LearningEvaluationItem ToLearningEvaluationItem(P24AcceptedHoldoutRow row)
        => new(
            ItemId: row.RowId,
            Prompt: row.PromptOrTaskTextRedactedSummary,
            ExpectedResponse: JsonSerializer.Serialize(new
            {
                selectedSkillIds = row.ExpectedSelectedSkillIds,
                availableSkillIds = row.AvailableSkillIds,
                hostFacts = new Dictionary<string, string> { ["runtime_scope"] = "row13-1-heldout-eval" },
                sourcePromptEntryId = "row13-1-heldout-eval-redacted-source",
                targetTrainingAdapter = SkillSelectionMachine.SkillSelectionTrainingAdapter
            }, WebJsonLine));

    private static string PayloadJson(DpoTrainingRow row, IReadOnlyList<string> selectedSkillIds)
        => JsonSerializer.Serialize(new
        {
            selectedSkillIds,
            availableSkillIds = row.AvailableSkillIds,
            hostFacts = new Dictionary<string, string>
            {
                ["scope"] = "candidate-only-row13-1-universal-skill-selection",
                ["universal_only"] = "true",
                ["no_vendor_output_distillation_issue"] = (!row.VendorOutputDistillationIssue).ToString().ToLowerInvariant()
            },
            sourcePromptEntryId = row.SourcePromptEntryId,
            targetTrainingAdapter = row.TargetTrainingAdapter
        }, WebJsonLine);

    private static ScenarioResult EvaluateScenario(P24AcceptedHoldoutRow row, SkillSelectionMachine commonMachine, SkillSelectionMachine candidateMachine, string candidateLabel)
    {
        var request = new SkillSelectionRequest(
            Prompt: row.PromptOrTaskTextRedactedSummary,
            ProjectRoot: "redacted-project-root",
            AvailableSkillIds: row.AvailableSkillIds,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["phase"] = "Row13.1",
                ["runtime_scope"] = "accepted_p24_held_out_comparison",
                ["input_shape"] = "redacted_summary_plus_available_skill_ids",
                ["universal_only"] = "true"
            },
            UserPromptSourceEntryId: "row13-1-redacted-held-out-source");

        var commonDecision = commonMachine.DecideFromTrainedState(request).Value!;
        var candidateDecision = candidateMachine.DecideFromTrainedState(request).Value!;
        var reversedRequest = request with { AvailableSkillIds = request.AvailableSkillIds.Reverse().ToArray() };
        var candidateReversedDecision = candidateMachine.DecideFromTrainedState(reversedRequest).Value!;
        var commonPass = row.ExpectedSelectedSkillIds.All(expected => commonDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));
        var candidatePass = row.ExpectedSelectedSkillIds.All(expected => candidateDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));

        return new ScenarioResult(
            RowId: row.RowId,
            CandidateLabel: candidateLabel,
            P24AcceptedHoldoutRow: true,
            UniversalOnly: row.WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution.Contains("Universal SkillSelection boundary", StringComparison.Ordinal),
            RuntimeInvocationMethod: "SkillSelectionMachine.DecideFromTrainedState",
            PromptHash: row.PromptHash,
            AvailableSkillIds: row.AvailableSkillIds,
            ExpectedSkillIds: row.ExpectedSelectedSkillIds,
            CommonSelectedSkillIds: commonDecision.SelectedSkillIds,
            CandidateSelectedSkillIds: candidateDecision.SelectedSkillIds,
            CommonRejectedSkillIds: commonDecision.RejectedSkillIds,
            CandidateRejectedSkillIds: candidateDecision.RejectedSkillIds,
            CommonPass: commonPass,
            CandidatePass: candidatePass,
            CommonConfidence: commonDecision.Confidence,
            CandidateConfidence: candidateDecision.Confidence,
            CandidateOrderInvariantForCandidate: candidateDecision.SelectedSkillIds.SequenceEqual(candidateReversedDecision.SelectedSkillIds, StringComparer.Ordinal),
            CommonEvidenceFeatures: commonDecision.EvidenceFeatures,
            CandidateEvidenceFeatures: candidateDecision.EvidenceFeatures,
            Evidence: $"held-out Row13.1 expected [{string.Join(", ", row.ExpectedSelectedSkillIds)}]; common selected [{string.Join(", ", commonDecision.SelectedSkillIds)}] pass={commonPass}; {candidateLabel} selected [{string.Join(", ", candidateDecision.SelectedSkillIds)}] pass={candidatePass}; redacted summary and available skill ids only.",
            TrainingRun: false,
            CheckpointMutated: false,
            SemanticQualityClaimed: false,
            ProductReadinessClaimed: false,
            TokenSavingsClaimed: false,
            PromotionPerformed: false,
            LocatorUpdatePerformed: false);
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

    private static IReadOnlyList<T> ReadJsonLineDocuments<T>(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<T>(line, WebJson) ?? throw new InvalidOperationException($"Could not parse JSONL row from {path}"))
            .ToArray();

    private static IReadOnlyList<string> DiscoverLocatorFiles(string repoRoot)
    {
        var root = RepoPath(repoRoot, "artifacts", "extension-runtime-token-quality-learnability");
        if (!Directory.Exists(root))
            return [];

        return Directory.EnumerateFiles(root, "*locator*.json", SearchOption.AllDirectories)
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static HashSnapshot SnapshotHashes(string commonManifestPath, string commonCheckpointPath, string candidateManifestPath, string candidateCheckpointPath, IReadOnlyList<string> locatorPaths)
        => new(
            CommonManifestSha256: Sha256(commonManifestPath),
            CommonCheckpointSha256: Sha256(commonCheckpointPath),
            OriginalCandidateManifestSha256: Sha256(candidateManifestPath),
            OriginalCandidateCheckpointSha256: Sha256(candidateCheckpointPath),
            LocatorSha256ByPath: locatorPaths.ToDictionary(path => path, Sha256, StringComparer.OrdinalIgnoreCase));

    private static HashComparison CompareSnapshots(HashSnapshot before, HashSnapshot after)
    {
        var locatorsUnchanged = before.LocatorSha256ByPath.Count == after.LocatorSha256ByPath.Count
            && before.LocatorSha256ByPath.All(item =>
                after.LocatorSha256ByPath.TryGetValue(item.Key, out var hash)
                && string.Equals(hash, item.Value, StringComparison.Ordinal));

        var commonManifestUnchanged = before.CommonManifestSha256 == after.CommonManifestSha256;
        var commonCheckpointUnchanged = before.CommonCheckpointSha256 == after.CommonCheckpointSha256;
        var originalCandidateManifestUnchanged = before.OriginalCandidateManifestSha256 == after.OriginalCandidateManifestSha256;
        var originalCandidateCheckpointUnchanged = before.OriginalCandidateCheckpointSha256 == after.OriginalCandidateCheckpointSha256;
        return new HashComparison(
            CommonManifestUnchanged: commonManifestUnchanged,
            CommonCheckpointUnchanged: commonCheckpointUnchanged,
            OriginalCandidateManifestUnchanged: originalCandidateManifestUnchanged,
            OriginalCandidateCheckpointUnchanged: originalCandidateCheckpointUnchanged,
            LocatorFilesUnchanged: locatorsUnchanged,
            AllTrackedInputsUnchanged: commonManifestUnchanged
                && commonCheckpointUnchanged
                && originalCandidateManifestUnchanged
                && originalCandidateCheckpointUnchanged
                && locatorsUnchanged);
    }

    private static ParseCheck ParseWrittenJsonAndJsonl(string outputDir)
    {
        var parsed = new List<string>();
        var failures = new List<string>();
        foreach (var path in Directory.EnumerateFiles(outputDir, "*.json", SearchOption.AllDirectories)
                     .Concat(Directory.EnumerateFiles(outputDir, "*.jsonl", SearchOption.AllDirectories))
                     .Order(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                if (path.EndsWith(".jsonl", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (var line in File.ReadLines(path, Encoding.UTF8).Where(line => !string.IsNullOrWhiteSpace(line)))
                        using (JsonDocument.Parse(line))
                        {
                        }
                }
                else
                {
                    using var _ = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
                }

                parsed.Add(path);
            }
            catch (Exception ex)
            {
                failures.Add($"{path}: {ex.Message}");
            }
        }

        return new ParseCheck(parsed.Count, failures.Count, failures.Count == 0, parsed, failures);
    }

    private static string BuildMarkdownReport(
        string status,
        int heldOutCount,
        int commonPassCount,
        int beforeCandidatePassCount,
        int afterCandidatePassCount,
        IReadOnlyList<DpoTrainingRow> dpoRows,
        string repoRoot,
        string outputDir,
        string stagedCheckpointPath,
        string stagedCheckpointHash,
        HashComparison unchanged)
        => $"""
        # Row13.1 candidate-only SkillSelection training/DPO slice

        Status: {status}

        Boundary: candidate-only staged checkpoint under this evidence folder. No production/common checkpoint mutation, no promotion, no locator update, no live Cursor UI, no git action.

        DPO rows: {dpoRows.Count}

        Target skills: {string.Join(", ", dpoRows.Select(row => row.SkillId).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))}

        Before/after metrics:
        - heldOutCount: {heldOutCount}
        - commonPassCount: {commonPassCount}
        - originalCandidatePassCountBeforeTraining: {beforeCandidatePassCount}
        - stagedCandidatePassCountAfterTraining: {afterCandidatePassCount}

        Staged candidate:
        - checkpoint: {RelativePath(repoRoot, stagedCheckpointPath)}
        - checkpointSha256: {stagedCheckpointHash}

        Backup/rollback:
        - original candidate checkpoint hash stayed unchanged: {unchanged.OriginalCandidateCheckpointUnchanged.ToString().ToLowerInvariant()}
        - common checkpoint hash stayed unchanged: {unchanged.CommonCheckpointUnchanged.ToString().ToLowerInvariant()}
        - tracked locator files stayed unchanged: {unchanged.LocatorFilesUnchanged.ToString().ToLowerInvariant()}
        - rollback is discard-only: remove this evidence leaf staged-candidate-checkpoint folder; no production locator/promotion state was changed.

        Claim locks:
        - product readiness: not claimed
        - token savings: not claimed
        - full semantic quality: not claimed
        - common model readiness: not claimed
        - training complete beyond this candidate slice: not claimed

        Evidence folder: {RelativePath(repoRoot, outputDir)}
        """;

    private static void WriteJson<T>(string path, T value)
        => WriteText(path, JsonSerializer.Serialize(value, WebJson) + "\n");

    private static void WriteJsonl<T>(string path, IEnumerable<T> rows)
        => WriteText(path, string.Join("\n", rows.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");

    private static void WriteText(string path, string text)
    {
        var normalized = text.Replace("\r\n", "\n").Replace("\r", "\n");
        if (!normalized.EndsWith('\n'))
            normalized += "\n";

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);
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

    private sealed record HashSnapshot(
        string CommonManifestSha256,
        string CommonCheckpointSha256,
        string OriginalCandidateManifestSha256,
        string OriginalCandidateCheckpointSha256,
        IReadOnlyDictionary<string, string> LocatorSha256ByPath);

    private sealed record HashComparison(
        bool CommonManifestUnchanged,
        bool CommonCheckpointUnchanged,
        bool OriginalCandidateManifestUnchanged,
        bool OriginalCandidateCheckpointUnchanged,
        bool LocatorFilesUnchanged,
        bool AllTrackedInputsUnchanged);

    private sealed record ParseCheck(
        int ParsedFileCount,
        int FailureCount,
        bool AllParsed,
        IReadOnlyList<string> ParsedFiles,
        IReadOnlyList<string> Failures);

    private sealed class DpoTrainingRow
    {
        public required string RowId { get; init; }
        public required string SourcePromptEntryId { get; init; }
        public required string SkillId { get; init; }
        public required string Prompt { get; init; }
        public required IReadOnlyList<string> AvailableSkillIds { get; init; }
        public required IReadOnlyList<string> ChosenSkillIds { get; init; }
        public required IReadOnlyList<string> RejectedSkillIds { get; init; }
        public SourceDisjointNotes? SourceDisjointNotes { get; set; }
        public required string QuarantineDecision { get; init; }
        public required bool VendorOutputDistillationIssue { get; init; }
        public required bool UniversalOnly { get; init; }
        public required string TargetTrainingAdapter { get; init; }
    }

    private sealed record SourceDisjointNotes(
        bool UsesP24ExactPrompt,
        bool UsesP24PairId,
        bool UsesP24SourceRef,
        bool UsesP24PathOrFilename,
        string Note);

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

    private sealed record ScenarioResult(
        string RowId,
        string CandidateLabel,
        bool P24AcceptedHoldoutRow,
        bool UniversalOnly,
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
        double CommonConfidence,
        double CandidateConfidence,
        bool CandidateOrderInvariantForCandidate,
        IReadOnlyList<string> CommonEvidenceFeatures,
        IReadOnlyList<string> CandidateEvidenceFeatures,
        string Evidence,
        bool TrainingRun,
        bool CheckpointMutated,
        bool SemanticQualityClaimed,
        bool ProductReadinessClaimed,
        bool TokenSavingsClaimed,
        bool PromotionPerformed,
        bool LocatorUpdatePerformed);
}
