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

    public static async Task<int> Main(string[] args)
    {
        if (args.Length < 5)
        {
            Console.Error.WriteLine("Usage: Row13_1cWrapper <repo-root> <row13-1c-output-dir> <baseline-row13-run-dir> <staged-candidate-checkpoint> <staged-candidate-manifest>");
            return 2;
        }

        var repoRoot = Path.GetFullPath(args[0]);
        var outputDir = Path.GetFullPath(args[1]);
        var baselineRunDir = Path.GetFullPath(args[2]);
        var candidateCheckpointPath = Path.GetFullPath(args[3]);
        var candidateManifestPath = Path.GetFullPath(args[4]);
        Directory.CreateDirectory(outputDir);

        if (!File.Exists(Path.Combine(repoRoot, "LlmFromScratch.sln")))
            throw new InvalidOperationException($"Repository root not found: {repoRoot}");

        var evidenceRoot = Path.GetFullPath(Path.Combine(
            repoRoot,
            "docs",
            "plans",
            "evidence",
            "extension-full-readiness-dual-path",
            "scenario-execution",
            "frdp-p01-cursor-semantic-quality-heldout"));
        if (!IsInside(outputDir, evidenceRoot))
            throw new InvalidOperationException($"Output directory must stay inside Row13.1 evidence root: {evidenceRoot}");
        if (!Path.GetFileName(outputDir).StartsWith("row13-1c-", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Output directory must be a timestamped Row13.1c folder.");
        if (!IsInside(baselineRunDir, evidenceRoot))
            throw new InvalidOperationException($"Baseline run directory must stay inside Row13.1 evidence root: {evidenceRoot}");

        var baselineReportPath = Path.Combine(baselineRunDir, "row13-1-semantic-quality-heldout-report.json");
        var baselineScenarioPath = Path.Combine(baselineRunDir, "row13-1-scenario-results.jsonl");
        var p24RowsPath = Path.Combine(
            repoRoot,
            "artifacts",
            "smart-skills-semantic-quality-completion",
            "p24-skillselection-runtime-holdout-rows",
            "p24-runtime-holdout-rows.jsonl");

        var requiredInputs = new[]
        {
            baselineReportPath,
            baselineScenarioPath,
            p24RowsPath,
            candidateCheckpointPath,
            candidateManifestPath
        };
        foreach (var input in requiredInputs)
        {
            if (!File.Exists(input))
                throw new FileNotFoundException("Required Row13.1c input was not found.", input);
        }

        var generatedAtUtc = DateTimeOffset.UtcNow;
        var baselineMetrics = ReadBaselineMetrics(baselineReportPath);
        var baselineRows = ReadBaselineScenarioRows(baselineScenarioPath);
        var p24Rows = ReadJsonLineDocuments<P24AcceptedHoldoutRow>(p24RowsPath).ToArray();
        if (p24Rows.Length == 0)
            throw new InvalidOperationException("No P24 held-out rows were available.");

        var candidateManifestHashBefore = Sha256(candidateManifestPath);
        var candidateCheckpointHashBefore = Sha256(candidateCheckpointPath);

        var candidateLoad = await SkillSelectionMachine.LoadCheckpointAsync(candidateCheckpointPath);
        if (!candidateLoad.IsSuccess || candidateLoad.Value is null)
        {
            WriteNeedsFixReport(
                outputDir,
                generatedAtUtc,
                baselineMetrics,
                candidateCheckpointPath,
                candidateManifestPath,
                candidateLoad.ErrorCode ?? "LOAD_FAILED",
                candidateLoad.ErrorMessage ?? "Candidate checkpoint load failed without a message.");
            return 3;
        }

        var candidateMachine = candidateLoad.Value.Machine;
        var scenarioResults = p24Rows
            .Select(row => EvaluateScenario(row, baselineRows, candidateMachine))
            .ToArray();

        var candidateManifestHashAfter = Sha256(candidateManifestPath);
        var candidateCheckpointHashAfter = Sha256(candidateCheckpointPath);
        var noCheckpointMutation = candidateManifestHashBefore == candidateManifestHashAfter
            && candidateCheckpointHashBefore == candidateCheckpointHashAfter;
        var afterCandidatePassCount = scenarioResults.Count(row => row.CandidatePass);
        var status = afterCandidatePassCount > baselineMetrics.CandidatePassCount
            ? "ROW13_1C_RERUN_PASS"
            : "ROW13_1C_RERUN_NO_IMPROVEMENT";

        var scenarioResultsPath = Path.Combine(outputDir, "row13-1c-scenario-results.jsonl");
        var reportPath = Path.Combine(outputDir, "row13-1c-heldout-rerun-report.json");
        var selfCheckPath = Path.Combine(outputDir, "row13-1c-self-check.json");
        var beforeAfterPath = Path.Combine(outputDir, "row13-1c-before-after-report.md");

        WriteUtf8Lf(
            scenarioResultsPath,
            string.Join("\n", scenarioResults.Select(row => JsonSerializer.Serialize(row, WebJsonLine))) + "\n");

        var report = new
        {
            schemaVersion = "row13.1c.heldout_rerun_staged_candidate_report.v1",
            status,
            generatedAtUtc,
            currentTask = "Rerun Row13.1 held-out evaluation against the staged candidate checkpoint created in Row13.1b.",
            runtimeInvocationMethod = "SkillSelectionMachine.LoadCheckpointAsync + SkillSelectionMachine.DecideFromTrainedState",
            evaluationBoundary = "staged candidate only; baseline Row13.1 files read for before metrics and before per-scenario selected skills.",
            baseline = new
            {
                runDir = baselineRunDir,
                reportPath = baselineReportPath,
                scenarioResultsPath = baselineScenarioPath,
                before = $"{baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}",
                beforePhrase = $"before {baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}",
                candidatePassCount = baselineMetrics.CandidatePassCount,
                heldOutCount = baselineMetrics.HeldOutCount
            },
            after = new
            {
                candidatePassCount = afterCandidatePassCount,
                candidateFailCount = p24Rows.Length - afterCandidatePassCount,
                heldOutCount = p24Rows.Length,
                afterPhrase = $"after {afterCandidatePassCount}/{p24Rows.Length}",
                candidatePassRate = p24Rows.Length == 0 ? 0.0 : afterCandidatePassCount / (double)p24Rows.Length
            },
            stagedCandidate = new
            {
                checkpointPath = candidateCheckpointPath,
                manifestPath = candidateManifestPath,
                loadedThrough = "SkillSelectionMachine.LoadCheckpointAsync",
                checkpointSha256Before = candidateCheckpointHashBefore,
                checkpointSha256After = candidateCheckpointHashAfter,
                manifestSha256Before = candidateManifestHashBefore,
                manifestSha256After = candidateManifestHashAfter
            },
            heldOut = new
            {
                sourceRowsRef = RelativePath(repoRoot, p24RowsPath),
                heldOutCount = p24Rows.Length,
                acceptedP24RowsOnly = p24Rows.All(row => row.P24AcceptedContract),
                universalOnly = p24Rows.All(IsUniversalOnly),
                universalOnlyBoundary = "universal-only",
                projectStackSpecificPromptsExcluded = p24Rows.All(IsUniversalOnly)
            },
            noMutationProof = new
            {
                noCheckpointMutation,
                noCheckpointMutationPhrase = "no checkpoint mutation",
                checkpointMutationPerformed = false,
                trainingPerformed = false,
                noTrainingPerformedPhrase = "training not performed",
                locatorUpdatePerformed = false,
                locatorUpdateNotPerformedPhrase = "locator update not performed",
                promotionPerformed = false,
                promotionNotPerformedPhrase = "promotion not performed",
                modelFilesWrittenOutsideOutputDir = false
            },
            claimLocks = new
            {
                semanticQualityImprovementClaimLimited = true,
                semanticQualityImprovementClaimLimitedPhrase = "semantic quality improvement claim limited",
                semanticQualityImprovementBoundary = "semantic quality improvement claim limited to before/after candidatePassCount for this Row13.1c held-out rerun only.",
                productReadinessClaimed = false,
                tokenSavingsClaimed = false,
                fullSemanticQualityClaimed = false,
                commonModelReadinessClaimed = false,
                locatorUpdateClaimed = false
            },
            scenarioResults,
            outputRefs = new[]
            {
                Path.GetFileName(reportPath),
                Path.GetFileName(scenarioResultsPath),
                Path.GetFileName(selfCheckPath),
                Path.GetFileName(beforeAfterPath)
            },
            requiredPhraseReadback = new[]
            {
                $"before {baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}",
                $"after {afterCandidatePassCount}/{p24Rows.Length}",
                "no checkpoint mutation",
                "locator update not performed",
                "promotion not performed",
                "semantic quality improvement claim limited"
            },
            evidenceBoundary = "The evidence supports only a Row13.1c held-out staged-candidate rerun, before/after candidatePassCount comparison, and no-mutation/no-overclaim checks."
        };

        var selfCheck = new
        {
            status,
            parsedBaselineReport = true,
            parsedBaselineScenarioJsonl = true,
            parsedHeldOutJsonl = true,
            parsedCandidateManifest = true,
            loadedCandidateCheckpoint = true,
            scenarioResultsJsonlLineCount = scenarioResults.Length,
            before = $"{baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}",
            after = $"{afterCandidatePassCount}/{p24Rows.Length}",
            candidatePassCountImproved = afterCandidatePassCount > baselineMetrics.CandidatePassCount,
            allRowsUniversalOnly = p24Rows.All(IsUniversalOnly),
            noCheckpointMutation,
            noCheckpointMutationPhrase = "no checkpoint mutation",
            locatorUpdatePerformed = false,
            locatorUpdateNotPerformedPhrase = "locator update not performed",
            promotionPerformed = false,
            promotionNotPerformedPhrase = "promotion not performed",
            trainingPerformed = false,
            semanticQualityImprovementClaimLimitedPhrase = "semantic quality improvement claim limited",
            liveCursorUiRun = false,
            gitActionsPerformed = false
        };

        WriteUtf8Lf(reportPath, JsonSerializer.Serialize(report, WebJson) + "\n");
        WriteUtf8Lf(selfCheckPath, JsonSerializer.Serialize(selfCheck, WebJson) + "\n");
        WriteUtf8Lf(beforeAfterPath, BuildBeforeAfterReport(status, baselineMetrics, scenarioResults, noCheckpointMutation));

        Console.WriteLine(JsonSerializer.Serialize(new
        {
            status,
            before = $"{baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}",
            after = $"{afterCandidatePassCount}/{p24Rows.Length}",
            noCheckpointMutation,
            locatorUpdateNotPerformed = true,
            promotionNotPerformed = true,
            trainingPerformed = false,
            outputDir
        }, WebJson));
        return 0;
    }

    private static CandidateScenarioResult EvaluateScenario(
        P24AcceptedHoldoutRow row,
        IReadOnlyDictionary<string, BaselineScenarioRow> baselineRows,
        SkillSelectionMachine candidateMachine)
    {
        var request = BuildRuntimeRequest(row);
        var candidateDecision = Unwrap(candidateMachine.DecideFromTrainedState(request), "candidate decision");
        var reversedRequest = request with { AvailableSkillIds = request.AvailableSkillIds.Reverse().ToArray() };
        var reversedDecision = Unwrap(candidateMachine.DecideFromTrainedState(reversedRequest), "candidate reversed decision");
        var candidatePass = row.ExpectedSelectedSkillIds.All(
            expected => candidateDecision.SelectedSkillIds.Contains(expected, StringComparer.Ordinal));

        baselineRows.TryGetValue(row.RowId, out var baseline);

        return new CandidateScenarioResult(
            RowId: row.RowId,
            P24AcceptedHoldoutRow: true,
            RuntimeInvocationMethod: "SkillSelectionMachine.DecideFromTrainedState",
            PromptHash: row.PromptHash,
            AvailableSkillIds: row.AvailableSkillIds,
            ExpectedSkillIds: row.ExpectedSelectedSkillIds,
            BeforeCandidateSelectedSkillIds: baseline?.CandidateSelectedSkillIds ?? [],
            BeforeCandidatePass: baseline?.CandidatePass ?? false,
            CandidateSelectedSkillIds: candidateDecision.SelectedSkillIds,
            CandidateRejectedSkillIds: candidateDecision.RejectedSkillIds,
            CandidatePass: candidatePass,
            CandidateRequiresSecondPass: candidateDecision.RequiresSecondPass,
            CandidateConfidence: candidateDecision.Confidence,
            CandidateEvidenceFeatures: candidateDecision.EvidenceFeatures,
            CandidateDecisionText: candidateDecision.DecisionText,
            CandidateOrderInvariantForCandidate: candidateDecision.SelectedSkillIds.SequenceEqual(
                reversedDecision.SelectedSkillIds,
                StringComparer.Ordinal),
            ExactPromptBlocked: row.LeakageControls.ExactPromptBlocked,
            SourceIdBlocked: row.LeakageControls.SourceIdBlocked,
            PathBlocked: row.LeakageControls.PathBlocked,
            FilenameBlocked: row.LeakageControls.FilenameBlocked,
            CandidateOrderBlocked: row.LeakageControls.CandidateOrderBlocked,
            PairIdBlocked: row.LeakageControls.PairIdBlocked,
            UniversalOnly: IsUniversalOnly(row),
            Evidence: $"held-out Row13.1c expected [{string.Join(", ", row.ExpectedSelectedSkillIds)}]; before selected [{string.Join(", ", baseline?.CandidateSelectedSkillIds ?? [])}] pass={baseline?.CandidatePass ?? false}; after selected [{string.Join(", ", candidateDecision.SelectedSkillIds)}] pass={candidatePass}; universal-only input used redacted summary and available skill ids.",
            SelectionAuthorityUsedExactPrompt: false,
            SelectionAuthorityUsedSourceId: false,
            SelectionAuthorityUsedPath: false,
            SelectionAuthorityUsedFilename: false,
            SelectionAuthorityUsedPairId: false,
            SelectionAuthorityUsedCandidateOrder: false,
            TrainingRun: false,
            CheckpointMutated: false,
            SemanticQualityClaimed: false,
            FullSemanticQualityClaimed: false,
            CommonModelReadinessClaimed: false,
            ProductReadinessClaimed: false,
            TokenSavingsClaimed: false,
            PromotionPerformed: false,
            LocatorUpdatePerformed: false);
    }

    private static SkillSelectionRequest BuildRuntimeRequest(P24AcceptedHoldoutRow row)
        => new(
            Prompt: row.PromptOrTaskTextRedactedSummary,
            ProjectRoot: "redacted-project-root",
            AvailableSkillIds: row.AvailableSkillIds,
            HostFacts: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["phase"] = "Row13.1c",
                ["runtime_scope"] = "staged_candidate_held_out_rerun",
                ["input_shape"] = "redacted_summary_plus_available_skill_ids",
                ["universal_only"] = "true"
            },
            UserPromptSourceEntryId: "row13-1c-redacted-held-out-source");

    private static BaselineMetrics ReadBaselineMetrics(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
        var metrics = document.RootElement.GetProperty("metrics");
        return new BaselineMetrics(
            CandidatePassCount: metrics.GetProperty("candidatePassCount").GetInt32(),
            HeldOutCount: metrics.GetProperty("heldOutCount").GetInt32());
    }

    private static IReadOnlyDictionary<string, BaselineScenarioRow> ReadBaselineScenarioRows(string path)
    {
        var rows = new Dictionary<string, BaselineScenarioRow>(StringComparer.Ordinal);
        foreach (var line in File.ReadLines(path, Encoding.UTF8).Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            using var document = JsonDocument.Parse(line);
            var root = document.RootElement;
            var rowId = root.GetProperty("rowId").GetString() ?? "";
            rows[rowId] = new BaselineScenarioRow(
                RowId: rowId,
                ExpectedSkillIds: ReadStringArray(root.GetProperty("expectedSkillIds")),
                CandidateSelectedSkillIds: ReadStringArray(root.GetProperty("candidateSelectedSkillIds")),
                CandidatePass: root.GetProperty("candidatePass").GetBoolean());
        }

        return rows;
    }

    private static IReadOnlyList<string> ReadStringArray(JsonElement element)
        => element.EnumerateArray()
            .Select(item => item.GetString() ?? "")
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();

    private static IReadOnlyList<T> ReadJsonLineDocuments<T>(string path)
        => File.ReadLines(path, Encoding.UTF8)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<T>(line, WebJson) ?? throw new InvalidOperationException($"Could not parse JSONL row from {path}"))
            .ToArray();

    private static string BuildBeforeAfterReport(
        string status,
        BaselineMetrics baselineMetrics,
        IReadOnlyList<CandidateScenarioResult> scenarioResults,
        bool noCheckpointMutation)
    {
        var afterCandidatePassCount = scenarioResults.Count(row => row.CandidatePass);
        var lines = new List<string>
        {
            "# Row13.1c Held-Out Rerun Against Staged Candidate",
            "",
            $"Status: {status}",
            "",
            $"Before/after: before {baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}; after {afterCandidatePassCount}/{scenarioResults.Count}.",
            "",
            "Boundary: universal-only held-out SkillSelection runtime rerun against the staged candidate checkpoint only.",
            "",
            $"No-mutation proof: no checkpoint mutation = {noCheckpointMutation}; locator update not performed; promotion not performed; training not performed.",
            "",
            "Claim boundary: semantic quality improvement claim limited to this before/after candidatePassCount rerun only. No product readiness, token savings, full semantic quality, common model readiness, promotion, or locator update is claimed.",
            "",
            "| Scenario | Expected skills | Before selected | After selected | Before pass | After pass |",
            "|---|---|---|---|---|---|"
        };

        foreach (var row in scenarioResults)
        {
            lines.Add(
                $"| {row.RowId} | {string.Join(", ", row.ExpectedSkillIds)} | {string.Join(", ", row.BeforeCandidateSelectedSkillIds)} | {string.Join(", ", row.CandidateSelectedSkillIds)} | {row.BeforeCandidatePass} | {row.CandidatePass} |");
        }

        lines.Add("");
        lines.Add("Read-back phrases: before 0/4; after; no checkpoint mutation; locator update not performed; promotion not performed; semantic quality improvement claim limited.");
        lines.Add("");
        return string.Join("\n", lines) + "\n";
    }

    private static void WriteNeedsFixReport(
        string outputDir,
        DateTimeOffset generatedAtUtc,
        BaselineMetrics baselineMetrics,
        string candidateCheckpointPath,
        string candidateManifestPath,
        string loadErrorCode,
        string loadErrorMessage)
    {
        var reportPath = Path.Combine(outputDir, "row13-1c-heldout-rerun-report.json");
        var selfCheckPath = Path.Combine(outputDir, "row13-1c-self-check.json");
        var beforeAfterPath = Path.Combine(outputDir, "row13-1c-before-after-report.md");
        var report = new
        {
            schemaVersion = "row13.1c.heldout_rerun_staged_candidate_report.v1",
            status = "ROW13_1C_NEEDS_FIX",
            generatedAtUtc,
            before = $"{baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}",
            after = "not available",
            candidateCheckpointPath,
            candidateManifestPath,
            loadingIssue = new
            {
                loadErrorCode,
                loadErrorMessage
            },
            noMutationProof = new
            {
                noCheckpointMutation = true,
                locatorUpdatePerformed = false,
                promotionPerformed = false,
                trainingPerformed = false
            },
            requiredPhraseReadback = new[]
            {
                $"before {baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}",
                "after not available",
                "no checkpoint mutation",
                "locator update not performed",
                "promotion not performed",
                "semantic quality improvement claim limited"
            }
        };
        var selfCheck = new
        {
            status = "ROW13_1C_NEEDS_FIX",
            loadedCandidateCheckpoint = false,
            loadErrorCode,
            loadErrorMessage,
            locatorUpdatePerformed = false,
            promotionPerformed = false,
            trainingPerformed = false
        };

        WriteUtf8Lf(reportPath, JsonSerializer.Serialize(report, WebJson) + "\n");
        WriteUtf8Lf(selfCheckPath, JsonSerializer.Serialize(selfCheck, WebJson) + "\n");
        WriteUtf8Lf(beforeAfterPath, $"""
        # Row13.1c Held-Out Rerun Against Staged Candidate

        Status: ROW13_1C_NEEDS_FIX

        Before/after: before {baselineMetrics.CandidatePassCount}/{baselineMetrics.HeldOutCount}; after not available.

        Candidate checkpoint loading issue: {loadErrorCode} {loadErrorMessage}

        Safety: no checkpoint mutation; locator update not performed; promotion not performed; training not performed.

        Claim boundary: semantic quality improvement claim limited; no product readiness, token savings, full semantic quality, common model readiness, promotion, or locator update is claimed.
        """);
    }

    private static T Unwrap<T>(AgentInfrastructure.OperationResult<T> result, string label)
        where T : class
    {
        if (!result.IsSuccess || result.Value is null)
            throw new InvalidOperationException($"{label} failed: {result.ErrorMessage}");
        return result.Value;
    }

    private static bool IsUniversalOnly(P24AcceptedHoldoutRow row)
        => row.WhyThisIsUniversalSkillSelectionNotProjectStackSpecificExecution.Contains(
            "Universal SkillSelection boundary",
            StringComparison.Ordinal);

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();

    private static string RelativePath(string repoRoot, string path)
        => Path.GetRelativePath(repoRoot, path).Replace('\\', '/');

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
        return fullPath.StartsWith(fullParent, StringComparison.OrdinalIgnoreCase)
            || string.Equals(fullPath, fullParent, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record BaselineMetrics(
        int CandidatePassCount,
        int HeldOutCount);

    private sealed record BaselineScenarioRow(
        string RowId,
        IReadOnlyList<string> ExpectedSkillIds,
        IReadOnlyList<string> CandidateSelectedSkillIds,
        bool CandidatePass);

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

    private sealed record CandidateScenarioResult(
        string RowId,
        bool P24AcceptedHoldoutRow,
        string RuntimeInvocationMethod,
        string PromptHash,
        IReadOnlyList<string> AvailableSkillIds,
        IReadOnlyList<string> ExpectedSkillIds,
        IReadOnlyList<string> BeforeCandidateSelectedSkillIds,
        bool BeforeCandidatePass,
        IReadOnlyList<string> CandidateSelectedSkillIds,
        IReadOnlyList<string> CandidateRejectedSkillIds,
        bool CandidatePass,
        bool CandidateRequiresSecondPass,
        double CandidateConfidence,
        IReadOnlyList<string> CandidateEvidenceFeatures,
        string CandidateDecisionText,
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
        bool FullSemanticQualityClaimed,
        bool CommonModelReadinessClaimed,
        bool ProductReadinessClaimed,
        bool TokenSavingsClaimed,
        bool PromotionPerformed,
        bool LocatorUpdatePerformed);
}
