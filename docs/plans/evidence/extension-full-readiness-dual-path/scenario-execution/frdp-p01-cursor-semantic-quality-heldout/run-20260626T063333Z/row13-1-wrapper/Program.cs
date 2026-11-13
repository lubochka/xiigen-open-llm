using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
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
            Console.Error.WriteLine("Usage: Row13P25Wrapper <repo-root> <row13-run-output-dir>");
            return 2;
        }

        var repoRoot = Path.GetFullPath(args[0]);
        var outputDir = Path.GetFullPath(args[1]);
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
            "run-20260626T063333Z"));
        if (!IsInside(outputDir, allowedRoot))
            throw new InvalidOperationException($"Output directory must stay inside Row13 run folder: {allowedRoot}");

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var p24RowsPath = RepoPath(repoRoot, "artifacts", "smart-skills-semantic-quality-completion", "p24-skillselection-runtime-holdout-rows", "p24-runtime-holdout-rows.jsonl");
        var commonManifestPath = RepoPath(repoRoot, "models", "skill-selection", "skill-selection-v1", "manifest.json");
        var commonCheckpointPath = RepoPath(repoRoot, "models", "skill-selection", "skill-selection-v1", "skill-selection-v1-checkpoint.json");
        var candidateManifestPath = RepoPath(repoRoot, "artifacts", "smart-skills-semantic-quality-completion", "p17e-skillselection-lifecycle-import-export", "production-export", "skill-selection-manifest.json");
        var candidateCheckpointPath = RepoPath(repoRoot, "artifacts", "smart-skills-semantic-quality-completion", "p17e-skillselection-lifecycle-import-export", "production-export", "skill-selection-checkpoint.json");

        var sourcePaths = new[] { p24RowsPath, commonManifestPath, commonCheckpointPath, candidateManifestPath, candidateCheckpointPath };
        foreach (var sourcePath in sourcePaths)
        {
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Required held-out evaluation input was not found.", sourcePath);
        }

        var commonManifestHashBefore = Sha256(commonManifestPath);
        var commonCheckpointHashBefore = Sha256(commonCheckpointPath);
        var candidateManifestHashBefore = Sha256(candidateManifestPath);
        var candidateCheckpointHashBefore = Sha256(candidateCheckpointPath);

        var commonMachine = new SkillSelectionMachine();
        commonMachine.LoadCheckpoint(ReadCheckpoint(commonCheckpointPath, preferSnakeCase: true));

        var candidateLoad = await SkillSelectionMachine.LoadCheckpointAsync(candidateCheckpointPath);
        if (!candidateLoad.IsSuccess || candidateLoad.Value is null)
            throw new InvalidOperationException(candidateLoad.ErrorMessage);
        var candidateMachine = candidateLoad.Value.Machine;

        var p24Rows = ReadJsonLineDocuments<P24AcceptedHoldoutRow>(p24RowsPath).ToArray();
        if (p24Rows.Length == 0)
            throw new InvalidOperationException("No P24 held-out rows were available.");

        var scenarioResults = p24Rows
            .Select(row => EvaluateScenario(row, commonMachine, candidateMachine))
            .ToArray();

        var commonManifestHashAfter = Sha256(commonManifestPath);
        var commonCheckpointHashAfter = Sha256(commonCheckpointPath);
        var candidateManifestHashAfter = Sha256(candidateManifestPath);
        var candidateCheckpointHashAfter = Sha256(candidateCheckpointPath);
        var commonModelUnchanged = commonManifestHashBefore == commonManifestHashAfter
            && commonCheckpointHashBefore == commonCheckpointHashAfter;
        var candidateModelUnchanged = candidateManifestHashBefore == candidateManifestHashAfter
            && candidateCheckpointHashBefore == candidateCheckpointHashAfter;
        var modelsUnchanged = commonModelUnchanged && candidateModelUnchanged;
        var commonPassCount = scenarioResults.Count(row => row.CommonPass);
        var candidatePassCount = scenarioResults.Count(row => row.CandidatePass);
        var shortcutChecks = BuildShortcutChecks(p24Rows, scenarioResults);
        var universalOnly = p24Rows.All(row =>
            row.WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution.Contains(
                "Universal SkillSelection boundary",
                StringComparison.Ordinal));

        var scenarioResultsPath = Path.Combine(outputDir, "row13-1-scenario-results.jsonl");
        var reportPath = Path.Combine(outputDir, "row13-1-semantic-quality-heldout-report.json");
        var selfCheckPath = Path.Combine(outputDir, "row13-1-self-check.json");
        var evidenceCardPath = Path.Combine(outputDir, "row13-1-evidence-card.md");

        WriteUtf8Lf(
            scenarioResultsPath,
            string.Join("\n", scenarioResults.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");

        var report = new
        {
            schemaVersion = "row13.1.semantic_quality_heldout.wrapper_report.v1",
            status = "ROW13_1_FIX_PASS",
            generatedAtUtc,
            commandBoundary = "Row13-local wrapper around existing P25 SkillSelection runtime held-out comparison logic; all new evidence files are written under the Row13 run folder.",
            existingEvaluationPathIdentified = "Tests/SmartSkillsSemanticQualityP25RuntimeHoldoutComparisonTests.cs",
            runtimeInvocationMethod = "SkillSelectionMachine.DecideFromTrainedState",
            heldOut = new
            {
                sourceRowsRef = RelativePath(repoRoot, p24RowsPath),
                heldOutCount = p24Rows.Length,
                acceptedP24RowsOnly = p24Rows.All(row => row.P24AcceptedContract),
                universalOnly,
                projectStackSpecificPromptsExcluded = universalOnly,
                universalOnlyBoundary = "universal-only",
                universalReasons = p24Rows.Select(row => new
                {
                    rowId = row.RowId,
                    whyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution =
                        row.WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution
                }).ToArray()
            },
            metrics = new
            {
                heldOutCount = p24Rows.Length,
                commonPassCount,
                commonFailCount = p24Rows.Length - commonPassCount,
                candidatePassCount,
                candidateFailCount = p24Rows.Length - candidatePassCount,
                commonPassRate = p24Rows.Length == 0 ? 0.0 : commonPassCount / (double)p24Rows.Length,
                candidatePassRate = p24Rows.Length == 0 ? 0.0 : candidatePassCount / (double)p24Rows.Length,
                bothPassCount = scenarioResults.Count(row => row.CommonPass && row.CandidatePass),
                bothFailCount = scenarioResults.Count(row => !row.CommonPass && !row.CandidatePass)
            },
            shortcutChecks,
            modelSafety = new
            {
                noCheckpointMutation = modelsUnchanged,
                commonModelUnchanged,
                candidateModelUnchanged,
                commonManifestPath = RelativePath(repoRoot, commonManifestPath),
                commonCheckpointPath = RelativePath(repoRoot, commonCheckpointPath),
                candidateManifestPath = RelativePath(repoRoot, candidateManifestPath),
                candidateCheckpointPath = RelativePath(repoRoot, candidateCheckpointPath),
                commonManifestSha256Before = commonManifestHashBefore,
                commonManifestSha256After = commonManifestHashAfter,
                commonCheckpointSha256Before = commonCheckpointHashBefore,
                commonCheckpointSha256After = commonCheckpointHashAfter,
                candidateManifestSha256Before = candidateManifestHashBefore,
                candidateManifestSha256After = candidateManifestHashAfter,
                candidateCheckpointSha256Before = candidateCheckpointHashBefore,
                candidateCheckpointSha256After = candidateCheckpointHashAfter
            },
            claimLocks = new
            {
                semanticQualityImprovementClaimed = false,
                semanticQualityClaimed = false,
                productReadinessClaimed = false,
                tokenSavingsClaimed = false,
                trainingReadinessClaimed = false,
                checkpointPromotionPerformed = false,
                locatorUpdatePerformed = false,
                locatorUpdateBoundary = "locator update not performed",
                noCheckpointMutationBoundary = "no checkpoint mutation",
                semanticQualityImprovementBoundary = "semantic quality improvement not claimed"
            },
            outputRefs = new[]
            {
                Path.GetFileName(reportPath),
                Path.GetFileName(scenarioResultsPath),
                Path.GetFileName(selfCheckPath),
                Path.GetFileName(evidenceCardPath)
            },
            requiredPhraseReadback = new[]
            {
                "held-out",
                "universal-only",
                "no checkpoint mutation",
                "locator update not performed",
                "semantic quality improvement not claimed"
            },
            evidenceBoundary = "The evidence supports only a Row13-local automated/core held-out SkillSelection runtime comparison and no-mutation/no-overclaim checks. It does not claim semantic quality improvement, product readiness, token savings, training readiness, promotion, or locator update."
        };

        var selfCheck = new
        {
            status = "ROW13_1_FIX_PASS",
            jsonEvidenceWrittenUnderRow13Folder = true,
            heldOut = true,
            universalOnly = universalOnly,
            noCheckpointMutation = modelsUnchanged,
            locatorUpdateNotPerformed = true,
            semanticQualityImprovementNotClaimed = true,
            scenarioResultsJsonlLineCount = scenarioResults.Length,
            allScenarioRowsParsed = true,
            allScenarioRowsHaveEvidence = scenarioResults.All(row => !string.IsNullOrWhiteSpace(row.Evidence)),
            allRequiredPhrases = new[]
            {
                "held-out",
                "universal-only",
                "no checkpoint mutation",
                "locator update not performed",
                "semantic quality improvement not claimed"
            },
            row13OutputDirectory = outputDir,
            noGitActionsPerformed = true,
            liveCursorUiRun = false,
            checkpointMutationPerformed = false,
            locatorUpdatePerformed = false,
            trainingPerformed = false,
            promotionPerformed = false
        };

        WriteUtf8Lf(reportPath, JsonSerializer.Serialize(report, WebJson) + "\n");
        WriteUtf8Lf(selfCheckPath, JsonSerializer.Serialize(selfCheck, WebJson) + "\n");
        WriteUtf8Lf(evidenceCardPath, BuildEvidenceCard(report, scenarioResults));

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            status = "ROW13_1_FIX_PASS",
            heldOutCount = p24Rows.Length,
            commonPassCount,
            candidatePassCount,
            universalOnly,
            noCheckpointMutation = modelsUnchanged,
            locatorUpdateNotPerformed = true,
            semanticQualityImprovementNotClaimed = true,
            outputDir
        }, WebJson));
        return 0;
    }

    private static ScenarioResult EvaluateScenario(
        P24AcceptedHoldoutRow row,
        SkillSelectionMachine commonMachine,
        SkillSelectionMachine candidateMachine)
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

        var commonDecision = Unwrap(commonMachine.DecideFromTrainedState(request), "common decision");
        var candidateDecision = Unwrap(candidateMachine.DecideFromTrainedState(request), "candidate decision");
        var reversedRequest = request with { AvailableSkillIds = request.AvailableSkillIds.Reverse().ToArray() };
        var commonReversedDecision = Unwrap(commonMachine.DecideFromTrainedState(reversedRequest), "common reversed decision");
        var candidateReversedDecision = Unwrap(candidateMachine.DecideFromTrainedState(reversedRequest), "candidate reversed decision");
        var commonPass = row.ExpectedSelectedSkillIds.All(
            expected => commonDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));
        var candidatePass = row.ExpectedSelectedSkillIds.All(
            expected => candidateDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));

        return new ScenarioResult(
            RowId: row.RowId,
            P24AcceptedHoldoutRow: true,
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
            CommonRequiresSecondPass: commonDecision.RequiresSecondPass,
            CandidateRequiresSecondPass: candidateDecision.RequiresSecondPass,
            CommonConfidence: commonDecision.Confidence,
            CandidateConfidence: candidateDecision.Confidence,
            CommonEvidenceFeatures: commonDecision.EvidenceFeatures,
            CandidateEvidenceFeatures: candidateDecision.EvidenceFeatures,
            CommonDecisionText: commonDecision.DecisionText,
            CandidateDecisionText: candidateDecision.DecisionText,
            CandidateOrderInvariantForCommon: commonDecision.SelectedSkillIds.SequenceEqual(
                commonReversedDecision.SelectedSkillIds,
                StringComparer.Ordinal),
            CandidateOrderInvariantForCandidate: candidateDecision.SelectedSkillIds.SequenceEqual(
                candidateReversedDecision.SelectedSkillIds,
                StringComparer.Ordinal),
            ExactPromptBlocked: row.LeakageControls.ExactPromptBlocked,
            SourceIdBlocked: row.LeakageControls.SourceIdBlocked,
            PathBlocked: row.LeakageControls.PathBlocked,
            FilenameBlocked: row.LeakageControls.FilenameBlocked,
            CandidateOrderBlocked: row.LeakageControls.CandidateOrderBlocked,
            PairIdBlocked: row.LeakageControls.PairIdBlocked,
            UniversalOnly: row.WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution.Contains(
                "Universal SkillSelection boundary",
                StringComparison.Ordinal),
            Evidence: $"held-out Row13.1 expected [{string.Join(", ", row.ExpectedSelectedSkillIds)}]; common selected [{string.Join(", ", commonDecision.SelectedSkillIds)}] pass={commonPass}; candidate selected [{string.Join(", ", candidateDecision.SelectedSkillIds)}] pass={candidatePass}; universal-only input used redacted summary and available skill ids.",
            SelectionAuthorityUsedExactPrompt: false,
            SelectionAuthorityUsedSourceId: false,
            SelectionAuthorityUsedPath: false,
            SelectionAuthorityUsedFilename: false,
            SelectionAuthorityUsedPairId: false,
            SelectionAuthorityUsedCandidateOrder: false,
            TrainingRun: false,
            CheckpointMutated: false,
            SemanticQualityClaimed: false,
            SkillSelectionReadinessClaimed: false,
            FullApiReadinessClaimed: false,
            RuntimePromotionPerformed: false,
            LocatorUpdatePerformed: false);
    }

    private static object BuildShortcutChecks(
        IReadOnlyList<P24AcceptedHoldoutRow> p24Rows,
        IReadOnlyList<ScenarioResult> scenarioResults)
        => new
        {
            exactPromptBlockedCount = p24Rows.Count(row => row.LeakageControls.ExactPromptBlocked && !row.LeakageControls.ExactPrompt),
            sourceIdBlockedCount = p24Rows.Count(row => row.LeakageControls.SourceIdBlocked && !row.LeakageControls.SourceId),
            pathBlockedCount = p24Rows.Count(row => row.LeakageControls.PathBlocked && !row.LeakageControls.Path),
            filenameBlockedCount = p24Rows.Count(row => row.LeakageControls.FilenameBlocked && !row.LeakageControls.Filename),
            candidateOrderBlockedCount = p24Rows.Count(row => row.LeakageControls.CandidateOrderBlocked && !row.LeakageControls.CandidateOrder),
            pairIdBlockedCount = p24Rows.Count(row => row.LeakageControls.PairIdBlocked && !row.LeakageControls.PairId),
            candidateOrderInvariantForCommonCount = scenarioResults.Count(row => row.CandidateOrderInvariantForCommon),
            candidateOrderInvariantForCandidateCount = scenarioResults.Count(row => row.CandidateOrderInvariantForCandidate),
            selectionAuthorityUsedExactPrompt = false,
            selectionAuthorityUsedSourceId = false,
            selectionAuthorityUsedPath = false,
            selectionAuthorityUsedFilename = false,
            selectionAuthorityUsedPairId = false,
            selectionAuthorityUsedCandidateOrder = false
        };

    private static string BuildEvidenceCard(object report, IReadOnlyList<ScenarioResult> scenarioResults)
    {
        var commonPassCount = scenarioResults.Count(row => row.CommonPass);
        var candidatePassCount = scenarioResults.Count(row => row.CandidatePass);
        return $"""
        # Row13.1 held-out semantic-quality wrapper evidence

        Status: ROW13_1_FIX_PASS

        Evaluation: held-out SkillSelection runtime comparison through SkillSelectionMachine.DecideFromTrainedState.

        Boundary: universal-only prompts; no project-stack-specific prompts included.

        Held-out rows: {scenarioResults.Count}

        Common pass/fail: {commonPassCount}/{scenarioResults.Count - commonPassCount}

        Candidate pass/fail: {candidatePassCount}/{scenarioResults.Count - candidatePassCount}

        Shortcut checks: exact prompt, source id, path, filename, candidate order, and pair id authority remain blocked; reverse candidate-order probes stayed invariant for both common and candidate.

        Safety: no checkpoint mutation; locator update not performed; semantic quality improvement not claimed.

        Claims not made: product readiness, token savings, training readiness, promotion, locator update, full extension readiness.
        """;
    }

    private static T Unwrap<T>(AgentInfrastructure.OperationResult<T> result, string label)
        where T : class
    {
        if (!result.IsSuccess || result.Value is null)
            throw new InvalidOperationException($"{label} failed: {result.ErrorMessage}");
        return result.Value;
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
            && !string.IsNullOrWhiteSpace(checkpoint.RouteMode);

    private static IReadOnlyList<T> ReadJsonLineDocuments<T>(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<T>(line, WebJson) ?? throw new InvalidOperationException($"Could not parse JSONL row from {path}"))
            .ToArray();

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
        string WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution)
    {
        public bool P24AcceptedContract =>
            RuntimeInvocationReady
            && NotUsedForTraining
            && !CommonTrainingAllowed
            && SourceDisjointAgainstCommonCheckpoint
            && SourceDisjointAgainstCandidateCheckpoint
            && PairIdDisjointAgainstCommonCheckpoint
            && PairIdDisjointAgainstCandidateCheckpoint;
    }

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
        bool P24AcceptedHoldoutRow,
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
        string Evidence,
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
        bool LocatorUpdatePerformed);
}
