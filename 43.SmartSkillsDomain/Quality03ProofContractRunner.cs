// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure;

public sealed class Quality03ProofContractRunner
{
    public const string ReportSchema = "xiigen.quality_03.proof_contract_runner.report.v1";
    public const string RowDecisionSchema = "xiigen.quality_03.proof_contract_runner.row_decision.v1";
    public const string RunnerReadyInputSchema = "xiigen.quality_03.proof_contract_runner.runner_ready_eval_input.v1";
    public const string DeterministicRunnerLabel = "local deterministic QA/proof-contract artifact checker; not semantic model, trainable unit, product readiness, or quality claim authority";
    public const int RequiredQualityRowCount = 9;
    public const int RequiredProofCategoryCount = 7;
    public const int RequiredBlockingProofCount = 63;

    private const string Pass = "PASS";
    private const string Risk = "RISK";
    private const string Defer = "DEFER";
    private const string ProofPass = "PASS";
    private const string ProofRiskDeferred = "RISK_DEFERRED_NOT_PASS";
    private const string ProofDeferred = "DEFERRED_NOT_PASS";
    private const string Lf = "\n";
    private const string AuthorizedSecondRaterPacketFileName = "per-row-authorized-second-rater-adjudication.jsonl";
    private const string InterRaterProofOnlyPassScope = "INTER_RATER_ADJUDICATION_PROOF_REF_ONLY_NOT_QUALITY_PASS";

    private static readonly CategorySpec[] CategorySpecs =
    [
        new("held_out_split", "approved_held_out_source_corpus_manifest_ref", "approved-held-out-source-corpus-manifest.json"),
        new("source_disjoint", "row_specific_source_disjoint_pass_ref", "row-specific-source-disjoint-pass-or-risk.jsonl"),
        new("nearest_neighbour_similarity_risk", "row_specific_nearest_neighbour_pass_ref", "row-specific-nearest-neighbour-pass-or-risk.jsonl"),
        new("ablation", "row_specific_ablation_runner_pass_ref", "row-specific-ablation-runner-report.jsonl"),
        new("no_leakage", "row_specific_no_leakage_pass_ref", "row-specific-no-leakage-pass-or-risk.jsonl"),
        new("no_memorization", "row_specific_no_memorization_pass_ref", "row-specific-no-memorization-pass-or-risk.jsonl"),
        new("inter_rater_adjudication", "independent_second_rater_adjudication_pass_ref", "row-specific-independent-second-rater-adjudication.jsonl")
    ];

    private static readonly string[] RequiredEvidenceJsonlFiles =
    [
        "row-specific-held-out-split.jsonl",
        "row-specific-source-disjoint-check.jsonl",
        "row-specific-nearest-neighbour-report.jsonl",
        "row-specific-ablation-report.jsonl",
        "row-specific-no-leakage-report.jsonl",
        "row-specific-no-memorization-report.jsonl",
        "row-specific-inter-rater-adjudication.jsonl",
        "per-row-blocking-proof-refs.jsonl"
    ];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions JsonLineOptions = new(JsonOptions)
    {
        WriteIndented = false
    };

    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    public OperationResult<Quality03ProofContractRunnerReport> Run(Quality03ProofContractRunnerOptions? options)
    {
        var optionsValidation = ValidateOptions(options);
        if (!optionsValidation.IsSuccess || options is null)
            return OperationResult<Quality03ProofContractRunnerReport>.Fail(
                optionsValidation.ErrorMessage,
                optionsValidation.ErrorCode);

        var loadedResult = LoadPacks(options);
        if (!loadedResult.IsSuccess || loadedResult.Value is null)
            return OperationResult<Quality03ProofContractRunnerReport>.Fail(
                loadedResult.ErrorMessage,
                loadedResult.ErrorCode);

        var report = BuildReport(options, loadedResult.Value);
        if (options.WriteReports)
        {
            var writeResult = WriteReportFiles(options, report);
            if (!writeResult.IsSuccess || writeResult.Value is null)
                return OperationResult<Quality03ProofContractRunnerReport>.Fail(
                    writeResult.ErrorMessage,
                    writeResult.ErrorCode);

            report = report with { OutputRefs = writeResult.Value };
        }

        return OperationResult<Quality03ProofContractRunnerReport>.Ok(report);
    }

    public OperationResult<string> ResolveOutputPath(string outputRoot, string relativePath)
    {
        if (string.IsNullOrWhiteSpace(outputRoot))
            return OperationResult<string>.Fail("Output root is required.", "OUTPUT_ROOT_REQUIRED");
        if (string.IsNullOrWhiteSpace(relativePath))
            return OperationResult<string>.Fail("Output relative path is required.", "OUTPUT_RELATIVE_PATH_REQUIRED");
        if (Path.IsPathRooted(relativePath))
            return OperationResult<string>.Fail("Output path must be relative to the caller-provided output root.", "OUTPUT_PATH_OUTSIDE_ROOT");

        var root = Path.GetFullPath(outputRoot);
        var candidate = Path.GetFullPath(Path.Combine(root, relativePath));
        var rootWithSeparator = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
        if (!string.Equals(candidate, root, StringComparison.OrdinalIgnoreCase) &&
            !candidate.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase))
        {
            return OperationResult<string>.Fail(
                "Output path resolved outside the caller-provided output root.",
                "OUTPUT_PATH_OUTSIDE_ROOT");
        }

        return OperationResult<string>.Ok(candidate);
    }

    public OperationResult<IReadOnlyList<string>> WriteReportFiles(
        Quality03ProofContractRunnerOptions? options,
        Quality03ProofContractRunnerReport? report)
    {
        if (options is null)
            return OperationResult<IReadOnlyList<string>>.Fail("Runner options are required.", "OPTIONS_REQUIRED");
        if (report is null)
            return OperationResult<IReadOnlyList<string>>.Fail("Runner report is required.", "REPORT_REQUIRED");

        try
        {
            Directory.CreateDirectory(options.OutputDirectory);
            var reportJsonPath = ResolveRequiredOutputPath(options.OutputDirectory, "quality-03-proof-contract-runner-report.json");
            var rowsJsonlPath = ResolveRequiredOutputPath(options.OutputDirectory, "quality-03-proof-contract-row-results.jsonl");
            var markdownPath = ResolveRequiredOutputPath(options.OutputDirectory, "quality-03-proof-contract-runner-report.md");
            var markdownRuPath = ResolveRequiredOutputPath(options.OutputDirectory, "quality-03-proof-contract-runner-report-ru.md");
            var outputRefs = new List<string>
            {
                NormalizePath(reportJsonPath),
                NormalizePath(rowsJsonlPath),
                NormalizePath(markdownPath),
                NormalizePath(markdownRuPath)
            };

            string? refusalPath = null;
            if (!report.FullProofPass)
            {
                refusalPath = ResolveRequiredOutputPath(options.OutputDirectory, "quality-03-proof-contract-refusal-report.json");
                outputRefs.Add(NormalizePath(refusalPath));
            }

            string? runnerReadyPath = null;
            var runnerReadyRows = Array.Empty<Quality03ProofContractRunnerReadyInputRow>();
            if (report.FullProofPass)
            {
                runnerReadyPath = ResolveRequiredOutputPath(options.OutputDirectory, "runner-ready-eval-inputs.jsonl");
                runnerReadyRows = BuildRunnerReadyRows(report);
                outputRefs.Add(NormalizePath(runnerReadyPath));
            }

            var reportWithRefs = report with
            {
                OutputRefs = outputRefs,
                RunnerReadyEvalInputsCreated = report.FullProofPass
            };
            WriteUtf8LfText(reportJsonPath, JsonSerializer.Serialize(reportWithRefs, JsonOptions));
            WriteUtf8LfText(rowsJsonlPath, BuildJsonLines(report.RowDecisions.Select(row => JsonSerializer.Serialize(row, JsonLineOptions))));
            WriteUtf8LfText(markdownPath, BuildMarkdown(reportWithRefs));
            WriteUtf8LfText(markdownRuPath, BuildMarkdownRu(reportWithRefs));

            if (refusalPath is not null)
                WriteUtf8LfText(refusalPath, BuildRefusalJson(reportWithRefs));
            if (runnerReadyPath is not null)
                WriteUtf8LfText(runnerReadyPath, BuildJsonLines(runnerReadyRows.Select(row => JsonSerializer.Serialize(row, JsonLineOptions))));

            return OperationResult<IReadOnlyList<string>>.Ok(outputRefs);
        }
        catch (IOException ex)
        {
            return OperationResult<IReadOnlyList<string>>.Fail($"Could not write QUALITY-03 proof runner reports: {ex.Message}", "REPORT_WRITE_FAILED");
        }
        catch (UnauthorizedAccessException ex)
        {
            return OperationResult<IReadOnlyList<string>>.Fail($"Could not write QUALITY-03 proof runner reports: {ex.Message}", "REPORT_WRITE_FORBIDDEN");
        }
    }

    private static OperationResult<bool> ValidateOptions(Quality03ProofContractRunnerOptions? options)
    {
        if (options is null)
            return OperationResult<bool>.Fail("Runner options are required.", "OPTIONS_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.EvidencePackDirectory))
            return OperationResult<bool>.Fail("Evidence pack directory is required.", "EVIDENCE_PACK_DIRECTORY_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.ProofContractPackDirectory))
            return OperationResult<bool>.Fail("Proof contract pack directory is required.", "PROOF_CONTRACT_PACK_DIRECTORY_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
            return OperationResult<bool>.Fail("Output directory is required.", "OUTPUT_DIRECTORY_REQUIRED");
        if (!Directory.Exists(options.EvidencePackDirectory))
            return OperationResult<bool>.Fail($"Evidence pack directory not found: {options.EvidencePackDirectory}", "EVIDENCE_PACK_DIRECTORY_NOT_FOUND");
        if (!Directory.Exists(options.ProofContractPackDirectory))
            return OperationResult<bool>.Fail($"Proof contract pack directory not found: {options.ProofContractPackDirectory}", "PROOF_CONTRACT_PACK_DIRECTORY_NOT_FOUND");
        return OperationResult<bool>.Ok(true);
    }

    private OperationResult<LoadedPacks> LoadPacks(Quality03ProofContractRunnerOptions options)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        foreach (var evidenceFileName in RequiredEvidenceJsonlFiles)
            ValidateJsonlFileCount(options.EvidencePackDirectory, evidenceFileName, RequiredQualityRowCount, errors, warnings);

        var blockingRows = ReadBlockingRows(Path.Combine(options.ProofContractPackDirectory, "per-row-blocking-proof-refs.jsonl"), errors);
        if (blockingRows.Count != RequiredQualityRowCount)
            errors.Add($"proof_contract_pack/per-row-blocking-proof-refs.jsonl must contain exactly {RequiredQualityRowCount} rows; actual={blockingRows.Count}.");
        foreach (var blockingRow in blockingRows.Where(row => row.CategoryResultsCount != RequiredProofCategoryCount))
            errors.Add($"{blockingRow.Row.RowId}: per-row-blocking-proof-refs category_results must contain exactly {RequiredProofCategoryCount} categories; actual={blockingRow.CategoryResultsCount}.");

        var inputRows = blockingRows.Select(row => row.Row).ToArray();
        ValidateUniqueRows(inputRows, errors);

        var rowsByCategory = new Dictionary<string, IReadOnlyDictionary<string, Quality03ProofContractArtifactRow>>(StringComparer.Ordinal);
        foreach (var spec in CategorySpecs.Where(spec => !string.Equals(spec.CategoryId, "held_out_split", StringComparison.Ordinal)))
        {
            var rows = ReadArtifactRows(Path.Combine(options.ProofContractPackDirectory, spec.FileName), spec.CategoryId, errors);
            if (rows.Count != RequiredQualityRowCount)
                errors.Add($"proof_contract_pack/{spec.FileName} must contain exactly {RequiredQualityRowCount} rows; actual={rows.Count}.");
            rowsByCategory[spec.CategoryId] = RowsById(rows, spec.FileName, errors);
        }

        var approvedCorpus = ReadApprovedCorpusAuthority(
            Path.Combine(options.ProofContractPackDirectory, "approved-held-out-source-corpus-manifest.json"),
            errors,
            warnings);
        var nearestNeighbourIndex = ReadNearestNeighbourIndexAuthority(
            Path.Combine(options.ProofContractPackDirectory, "row-specific-nearest-neighbour-index-manifest.json"),
            errors,
            warnings);
        var authorizedSecondRaterPacket = ReadAuthorizedSecondRaterPacket(options, inputRows, warnings);

        ValidateCategoryRowsMatchInputRows(inputRows, rowsByCategory, errors);
        ValidateCorpusRowsMatchInputRows(inputRows, approvedCorpus, errors);

        var validation = new Quality03ProofContractRunnerValidation(errors.Count == 0, errors, warnings);
        return OperationResult<LoadedPacks>.Ok(new LoadedPacks(
            Validation: validation,
            InputRows: inputRows,
            RowsByCategory: rowsByCategory,
            ApprovedCorpus: approvedCorpus,
            NearestNeighbourIndex: nearestNeighbourIndex,
            AuthorizedSecondRaterPacket: authorizedSecondRaterPacket));
    }

    private static Quality03ProofContractRunnerReport BuildReport(
        Quality03ProofContractRunnerOptions options,
        LoadedPacks loaded)
    {
        var generatedAt = options.GeneratedAtUtc.ToString("O");
        var runId = string.IsNullOrWhiteSpace(options.RunId)
            ? $"quality-03-proof-contract-runner-{options.GeneratedAtUtc:yyyyMMdd-HHmmss}"
            : options.RunId;
        var rowDecisions = loaded.InputRows
            .Select(row => BuildRowDecision(options, loaded, runId, generatedAt, row))
            .ToArray();
        var blockingPass = rowDecisions.Sum(row => row.ResolvedBlockingRefCount);
        var blockingMissing = RequiredBlockingProofCount - blockingPass;
        var fullProofPass = loaded.Validation.Passed && blockingPass == RequiredBlockingProofCount && rowDecisions.Length == RequiredQualityRowCount;
        rowDecisions = rowDecisions
            .Select(row => ApplyGlobalRunnerGate(row, fullProofPass))
            .ToArray();
        var runnerReadyRows = fullProofPass ? rowDecisions.Count(row => row.RunnerReadyCandidate) : 0;
        var missingRefs = rowDecisions
            .SelectMany(row => row.CategoryResults
                .Where(category => !category.BlockingRefResolved)
                .Select(category => $"{row.RowId}:{category.CategoryId}:{category.ProofStatus}"))
            .ToArray();
        var interRaterPassCount = rowDecisions
            .SelectMany(row => row.CategoryResults)
            .Count(category => string.Equals(category.CategoryId, "inter_rater_adjudication", StringComparison.Ordinal) &&
                               category.BlockingRefResolved);
        var nextBoundedWorkOrder = fullProofPass
            ? "Run the separately approved eval-only quality scorer on runner-ready inputs."
            : interRaterPassCount == RequiredQualityRowCount
                ? "QUALITY-03-MISSING-NON-INTER-RATER-PROOF-PRODUCERS-09L: produce held-out/source-disjoint/nearest-neighbour/ablation/no-leakage/no-memorization proof refs until 63/63 PASS."
                : "QUALITY-03-INDEPENDENT-SECOND-RATER-ADJUDICATION-PRODUCE-09H plus missing proof producers until 63/63 blocking refs PASS.";

        return new Quality03ProofContractRunnerReport(
            Schema: ReportSchema,
            WorkOrderId: options.WorkOrderId,
            RunId: runId,
            GeneratedAtUtc: generatedAt,
            EvidencePackDirectory: NormalizePath(options.EvidencePackDirectory),
            ProofContractPackDirectory: NormalizePath(options.ProofContractPackDirectory),
            SecondRaterPacketPath: loaded.AuthorizedSecondRaterPacket.Provided
                ? loaded.AuthorizedSecondRaterPacket.NormalizedPacketPath
                : string.Empty,
            OutputDirectory: NormalizePath(options.OutputDirectory),
            Validation: loaded.Validation,
            QualityRowsSeen: rowDecisions.Length,
            RequiredQualityRows: RequiredQualityRowCount,
            ProofCategoriesRequired: RequiredProofCategoryCount,
            BlockingProofRefsTotal: RequiredBlockingProofCount,
            BlockingProofRefsPass: blockingPass,
            BlockingProofRefsRiskOrDeferred: Math.Max(0, blockingMissing),
            RunnerReadyRows: runnerReadyRows,
            QualityPassRows: 0,
            FullProofPass: fullProofPass,
            RunnerReadyEvalInputsCreated: false,
            RunnerScorerRun: false,
            ExternalAiModelCalled: rowDecisions.SelectMany(row => row.CategoryResults).Any(category => category.RiskCodes.Contains("EXTERNAL_AI_MODEL_CALLED", StringComparer.Ordinal)),
            TrainingDpoCheckpointModelLocatorMutation: rowDecisions.SelectMany(row => row.CategoryResults).Any(category => category.RiskCodes.Contains("TRAINING_DPO_CHECKPOINT_MODEL_LOCATOR_MUTATION", StringComparer.Ordinal)),
            ClaimLocks: new Quality03ProofContractClaimLocks(),
            RowDecisions: rowDecisions,
            MissingOrDeferredRefs: missingRefs,
            OutputRefs: [],
            DeterministicRunnerLabel: DeterministicRunnerLabel,
            WhatThisProves: "This proves only deterministic local parsing and QA/proof-contract aggregation over supplied artifacts.",
            WhatThisDoesNotProve: "This does not prove semantic quality, product readiness, learnability, token savings, model quality, runtime readiness, training readiness, target promotion, locator updates, or final readiness.",
            NextBoundedWorkOrder: nextBoundedWorkOrder);
    }

    private static Quality03ProofContractRowDecision BuildRowDecision(
        Quality03ProofContractRunnerOptions options,
        LoadedPacks loaded,
        string runId,
        string generatedAt,
        Quality03ProofContractInputRow row)
    {
        var categoryResults = CategorySpecs
            .Select(spec => DecideCategory(options, loaded, row, spec))
            .ToArray();
        var resolved = categoryResults.Count(category => category.BlockingRefResolved);
        var missing = RequiredProofCategoryCount - resolved;
        var ready = resolved == RequiredProofCategoryCount;

        return new Quality03ProofContractRowDecision(
            Schema: RowDecisionSchema,
            WorkOrderId: options.WorkOrderId,
            RunId: runId,
            GeneratedAtUtc: generatedAt,
            RowId: row.RowId,
            PairSequenceId: row.PairSequenceId,
            PairId: row.PairId,
            ScenarioId: row.ScenarioId,
            SourceScenarioId: row.SourceScenarioId,
            Host: row.Host,
            Method: row.Method,
            EvidenceClass: row.EvidenceClass,
            PromptSha256: row.PromptSha256,
            CategoryResults: categoryResults,
            ResolvedBlockingRefCount: resolved,
            MissingOrDeferredBlockingRefCount: missing,
            RunnerReadyCandidate: ready,
            QualityPassRow: false,
            RunnerScorerRunAllowed: false,
            Decision: ready
                ? "All seven proof-contract categories PASS for this row as a candidate only; scorer remains locked until 63/63 PASS."
                : $"Row remains blocked: {resolved}/{RequiredProofCategoryCount} proof refs PASS. runner-ready input is not emitted for this row.");
    }

    private static Quality03ProofContractRowDecision ApplyGlobalRunnerGate(
        Quality03ProofContractRowDecision row,
        bool fullProofPass)
    {
        var runnerScorerRunAllowed = fullProofPass && row.RunnerReadyCandidate;
        var decision = row.RunnerReadyCandidate
            ? fullProofPass
                ? "All seven proof-contract categories PASS for this row and global 63/63 proof refs PASS; scorer/runner row authorization is unlocked."
                : "All seven proof-contract categories PASS for this row as a candidate only; scorer remains locked until 63/63 PASS."
            : row.Decision;

        return row with
        {
            RunnerScorerRunAllowed = runnerScorerRunAllowed,
            Decision = decision
        };
    }

    private static Quality03ProofContractCategoryDecision DecideCategory(
        Quality03ProofContractRunnerOptions options,
        LoadedPacks loaded,
        Quality03ProofContractInputRow row,
        CategorySpec spec)
    {
        var categoryRow = GetCategoryRow(loaded, spec.CategoryId, row.RowId);
        var secondRaterPacketOverridesProofPack =
            string.Equals(spec.CategoryId, "inter_rater_adjudication", StringComparison.Ordinal) &&
            loaded.AuthorizedSecondRaterPacket.Provided;
        if (!secondRaterPacketOverridesProofPack && categoryRow is { ExternalAiModelCalled: true })
            return RiskDecision(options, row, spec, categoryRow, "Artifact declares external AI/model use.", ["EXTERNAL_AI_MODEL_CALLED"]);
        if (!secondRaterPacketOverridesProofPack && categoryRow is { TrainingDpoCheckpointModelLocatorMutation: true })
            return RiskDecision(options, row, spec, categoryRow, "Artifact declares training/DPO/checkpoint/model/locator mutation.", ["TRAINING_DPO_CHECKPOINT_MODEL_LOCATOR_MUTATION"]);

        return spec.CategoryId switch
        {
            "held_out_split" => DecideHeldOutSplit(options, loaded, row, spec),
            "source_disjoint" => DecideSourceDisjoint(options, loaded, row, spec, categoryRow),
            "nearest_neighbour_similarity_risk" => DecideNearestNeighbour(options, loaded, row, spec, categoryRow),
            "ablation" => DecideAblation(options, row, spec, categoryRow),
            "no_leakage" => DecideReplayProbe(options, row, spec, categoryRow, "no-leakage"),
            "no_memorization" => DecideReplayProbe(options, row, spec, categoryRow, "no-memorization"),
            "inter_rater_adjudication" => DecideSecondRater(options, loaded, row, spec, categoryRow),
            _ => RiskDecision(options, row, spec, categoryRow, "Unknown category.", ["UNKNOWN_CATEGORY"])
        };
    }

    private static Quality03ProofContractCategoryDecision DecideHeldOutSplit(
        Quality03ProofContractRunnerOptions options,
        LoadedPacks loaded,
        Quality03ProofContractInputRow row,
        CategorySpec spec)
    {
        if (HasApprovedCorpusAuthorityForRow(loaded.ApprovedCorpus, row.RowId))
            return PassDecision(options, row, spec, null, "Approved corpus and held-out split authority is present for this row.");

        return DeferDecision(
            options,
            row,
            spec,
            null,
            "Approved corpus/split authority is missing or row assignment is not approved.",
            ["MISSING_APPROVED_CORPUS_SPLIT_AUTHORITY"]);
    }

    private static Quality03ProofContractCategoryDecision DecideSourceDisjoint(
        Quality03ProofContractRunnerOptions options,
        LoadedPacks loaded,
        Quality03ProofContractInputRow row,
        CategorySpec spec,
        Quality03ProofContractArtifactRow? categoryRow)
    {
        if (!HasApprovedCorpusAuthorityForRow(loaded.ApprovedCorpus, row.RowId) ||
            !HasSourceDisjointAuthorityForRow(loaded.ApprovedCorpus, row.RowId))
        {
            return DeferDecision(
                options,
                row,
                spec,
                categoryRow,
                "Source-disjoint PASS is blocked until approved corpus/split authority and row-level source-disjoint authority are present.",
                ["MISSING_APPROVED_CORPUS_SPLIT_AUTHORITY"]);
        }

        if (CategoryArtifactPasses(categoryRow))
            return PassDecision(options, row, spec, categoryRow, "Source-disjoint row PASS is backed by approved corpus authority.");

        return RiskDecision(options, row, spec, categoryRow, "Source-disjoint artifact row is not PASS.", ["SOURCE_DISJOINT_ROW_NOT_PASS"]);
    }

    private static Quality03ProofContractCategoryDecision DecideNearestNeighbour(
        Quality03ProofContractRunnerOptions options,
        LoadedPacks loaded,
        Quality03ProofContractInputRow row,
        CategorySpec spec,
        Quality03ProofContractArtifactRow? categoryRow)
    {
        if (!NearestNeighbourIndexPasses(loaded.NearestNeighbourIndex))
        {
            return DeferDecision(
                options,
                row,
                spec,
                categoryRow,
                "Approved nearest-neighbour index manifest is missing or not PASS.",
                ["MISSING_APPROVED_NEAREST_NEIGHBOUR_INDEX_MANIFEST"]);
        }

        if (categoryRow is null)
            return DeferDecision(options, row, spec, null, "Nearest-neighbour row result is missing.", ["MISSING_NEAREST_NEIGHBOUR_ROW"]);
        if (!categoryRow.IndexContractPresent)
            return DeferDecision(options, row, spec, categoryRow, "Nearest-neighbour row does not link an index contract.", ["MISSING_ROW_INDEX_CONTRACT"]);
        if (categoryRow.TopKResultsCount <= 0)
            return RiskDecision(options, row, spec, categoryRow, "Nearest-neighbour row has no top-k result evidence.", ["MISSING_TOP_K_ROW_RESULTS"]);
        if (CategoryArtifactPasses(categoryRow))
            return PassDecision(options, row, spec, categoryRow, "Nearest-neighbour index manifest and row top-k PASS are present.");

        return RiskDecision(options, row, spec, categoryRow, "Nearest-neighbour row is not PASS.", ["NEAREST_NEIGHBOUR_ROW_NOT_PASS"]);
    }

    private static Quality03ProofContractCategoryDecision DecideAblation(
        Quality03ProofContractRunnerOptions options,
        Quality03ProofContractInputRow row,
        CategorySpec spec,
        Quality03ProofContractArtifactRow? categoryRow)
    {
        if (categoryRow is null)
            return DeferDecision(options, row, spec, null, "Ablation runner row is missing.", ["MISSING_ABLATION_ROW"]);
        if (!categoryRow.RunnerExecuted || IsNotRunStatus(categoryRow.RunnerStatus))
            return DeferDecision(options, row, spec, categoryRow, "Ablation runner did not execute for this row.", ["ABLATION_RUNNER_NOT_RUN"]);
        if (!categoryRow.RunnerContractAvailableForCurrentRows)
            return DeferDecision(options, row, spec, categoryRow, "Ablation runner contract is not available for current rows.", ["ABLATION_RUNNER_CONTRACT_MISSING"]);
        if (categoryRow.ShortcutOnly)
            return RiskDecision(options, row, spec, categoryRow, "Shortcut-only ablation evidence cannot PASS.", ["ABLATION_SHORTCUT_ONLY"]);
        if (!AllControlsRan(categoryRow))
            return RiskDecision(options, row, spec, categoryRow, "Ablation controls are incomplete.", ["ABLATION_CONTROLS_INCOMPLETE"]);
        if (CategoryArtifactPasses(categoryRow))
            return PassDecision(options, row, spec, categoryRow, "Deterministic ablation runner evidence PASS is present.");

        return RiskDecision(options, row, spec, categoryRow, "Ablation artifact row is not PASS.", ["ABLATION_ROW_NOT_PASS"]);
    }

    private static Quality03ProofContractCategoryDecision DecideReplayProbe(
        Quality03ProofContractRunnerOptions options,
        Quality03ProofContractInputRow row,
        CategorySpec spec,
        Quality03ProofContractArtifactRow? categoryRow,
        string label)
    {
        if (categoryRow is null)
            return DeferDecision(options, row, spec, null, $"{label} replay row is missing.", [$"MISSING_{NormalizeCode(label)}_ROW"]);
        if (!categoryRow.ReplayProbeExecuted)
            return DeferDecision(options, row, spec, categoryRow, $"{label} replay probe did not execute.", [$"{NormalizeCode(label)}_REPLAY_PROBE_NOT_RUN"]);
        if (categoryRow.PassBoundaryFlagsOnly)
            return RiskDecision(options, row, spec, categoryRow, $"{label} boundary flags alone cannot PASS.", [$"{NormalizeCode(label)}_BOUNDARY_FLAGS_ONLY"]);
        if (categoryRow.ApprovedRawPrivateRefs.Count == 0)
            return RiskDecision(options, row, spec, categoryRow, $"{label} approved raw/private refs are missing.", [$"{NormalizeCode(label)}_APPROVED_RAW_PRIVATE_REFS_MISSING"]);
        if (CategoryArtifactPasses(categoryRow))
            return PassDecision(options, row, spec, categoryRow, $"{label} replay probe PASS has approved raw/private refs.");

        return RiskDecision(options, row, spec, categoryRow, $"{label} artifact row is not PASS.", [$"{NormalizeCode(label)}_ROW_NOT_PASS"]);
    }

    private static Quality03ProofContractCategoryDecision DecideSecondRater(
        Quality03ProofContractRunnerOptions options,
        LoadedPacks loaded,
        Quality03ProofContractInputRow row,
        CategorySpec spec,
        Quality03ProofContractArtifactRow? categoryRow)
    {
        var packet = loaded.AuthorizedSecondRaterPacket;
        var evidenceRefOverride = packet.Provided
            ? $"{packet.NormalizedPacketPath}#{row.RowId}"
            : null;
        if (packet.Provided)
        {
            categoryRow = packet.RowsById.TryGetValue(row.RowId, out var authorizedRow)
                ? authorizedRow
                : null;
            if (!packet.RowSetMatchesExpectedRows)
            {
                return DeferDecision(
                    options,
                    row,
                    spec,
                    categoryRow,
                    "Authorized second-rater packet row ids do not exactly match the current proof-contract rows.",
                    ["AUTHORIZED_SECOND_RATER_PACKET_ROW_SET_MISMATCH"],
                    evidenceRefOverride);
            }
        }

        if (categoryRow is null)
            return DeferDecision(options, row, spec, null, "Independent second-rater row is missing.", ["MISSING_SECOND_RATER_ROW"], evidenceRefOverride);
        if (packet.Provided && !SecondRaterPacketRowMatchesInputRow(categoryRow, row))
        {
            return RiskDecision(
                options,
                row,
                spec,
                categoryRow,
                "Authorized second-rater packet row does not match row_id, pair_id, and scenario_id for this proof-contract row.",
                ["AUTHORIZED_SECOND_RATER_PACKET_ROW_MISMATCH"],
                evidenceRefOverride);
        }
        if (categoryRow.ExternalAiModelCalled)
            return RiskDecision(options, row, spec, categoryRow, "Second-rater artifact declares external AI/model use.", ["EXTERNAL_AI_MODEL_CALLED"], evidenceRefOverride);
        if (categoryRow.TrainingDpoCheckpointModelLocatorMutation)
            return RiskDecision(options, row, spec, categoryRow, "Second-rater artifact declares training/DPO/checkpoint/model/locator mutation.", ["TRAINING_DPO_CHECKPOINT_MODEL_LOCATOR_MUTATION"], evidenceRefOverride);
        if (!categoryRow.IndependentSecondRaterAvailable)
            return DeferDecision(options, row, spec, categoryRow, "Independent second rater is absent.", ["INDEPENDENT_SECOND_RATER_ABSENT"], evidenceRefOverride);
        if (string.IsNullOrWhiteSpace(categoryRow.ReviewerId2))
            return DeferDecision(options, row, spec, categoryRow, "reviewer_id_2 is missing.", ["REVIEWER_ID_2_MISSING"], evidenceRefOverride);
        if (string.Equals(categoryRow.ReviewerId1, categoryRow.ReviewerId2, StringComparison.OrdinalIgnoreCase))
            return RiskDecision(options, row, spec, categoryRow, "reviewer_id_2 matches reviewer_id_1.", ["SECOND_RATER_NOT_INDEPENDENT"], evidenceRefOverride);
        if (categoryRow.FakedIndependence || LooksLikeSelfChecker(categoryRow.ReviewerId2))
            return DeferDecision(options, row, spec, categoryRow, "Deterministic self-checker is not an independent reviewer.", ["DETERMINISTIC_SELF_CHECKER_NOT_INDEPENDENT"], evidenceRefOverride);
        if (packet.Provided && !categoryRow.CanSatisfyInterRaterAdjudicationProof)
            return DeferDecision(options, row, spec, categoryRow, "Authorized packet cannot satisfy inter-rater adjudication proof.", ["AUTHORIZED_PACKET_CANNOT_SATISFY_INTER_RATER_PROOF"], evidenceRefOverride);
        if (packet.Provided && !IsInterRaterProofOnlyPassScope(categoryRow.PassScope))
            return RiskDecision(options, row, spec, categoryRow, "Authorized packet pass_scope is not limited to inter-rater proof.", ["AUTHORIZED_PACKET_PASS_SCOPE_NOT_INTER_RATER_ONLY"], evidenceRefOverride);
        if (packet.Provided && (categoryRow.RunnerReadyAfterThisItem || categoryRow.QualityPassAfterThisItem))
            return RiskDecision(options, row, spec, categoryRow, "Authorized packet attempts to unlock runner-ready or quality-pass outside inter-rater proof.", ["AUTHORIZED_PACKET_OVERCLAIMS_RUNNER_OR_QUALITY_PASS"], evidenceRefOverride);
        if (!AdjudicationPasses(categoryRow.AdjudicationStatus))
            return DeferDecision(options, row, spec, categoryRow, "Adjudication packet is absent or deferred.", ["ADJUDICATION_PACKET_NOT_PASS"], evidenceRefOverride);
        if (CategoryArtifactPasses(categoryRow))
            return PassDecision(options, row, spec, categoryRow, "Independent second-rater/adjudication PASS is present.");

        return RiskDecision(options, row, spec, categoryRow, "Second-rater artifact row is not PASS.", ["SECOND_RATER_ROW_NOT_PASS"], evidenceRefOverride);
    }

    private static Quality03ProofContractCategoryDecision PassDecision(
        Quality03ProofContractRunnerOptions options,
        Quality03ProofContractInputRow row,
        CategorySpec spec,
        Quality03ProofContractArtifactRow? categoryRow,
        string reason)
        => new(
            CategoryId: spec.CategoryId,
            RequiredProofRef: spec.RequiredProofRef,
            PassOrRisk: Pass,
            ProofStatus: ProofPass,
            BlockingRefResolved: true,
            Deferred: false,
            ConcreteProofRef: categoryRow?.ConcreteProofRef ?? BuildArtifactRef(options, row, spec),
            EvidenceRef: categoryRow?.ArtifactRef ?? BuildArtifactRef(options, row, spec),
            RiskCodes: [],
            DecisionReason: reason);

    private static Quality03ProofContractCategoryDecision RiskDecision(
        Quality03ProofContractRunnerOptions options,
        Quality03ProofContractInputRow row,
        CategorySpec spec,
        Quality03ProofContractArtifactRow? categoryRow,
        string reason,
        IReadOnlyList<string> riskCodes,
        string? evidenceRefOverride = null)
        => new(
            CategoryId: spec.CategoryId,
            RequiredProofRef: spec.RequiredProofRef,
            PassOrRisk: Risk,
            ProofStatus: ProofRiskDeferred,
            BlockingRefResolved: false,
            Deferred: true,
            ConcreteProofRef: null,
            EvidenceRef: categoryRow?.ArtifactRef ?? evidenceRefOverride ?? BuildArtifactRef(options, row, spec),
            RiskCodes: categoryRow?.RiskCodes.Concat(riskCodes).Distinct(StringComparer.Ordinal).ToArray() ?? riskCodes,
            DecisionReason: reason);

    private static Quality03ProofContractCategoryDecision DeferDecision(
        Quality03ProofContractRunnerOptions options,
        Quality03ProofContractInputRow row,
        CategorySpec spec,
        Quality03ProofContractArtifactRow? categoryRow,
        string reason,
        IReadOnlyList<string> riskCodes,
        string? evidenceRefOverride = null)
        => new(
            CategoryId: spec.CategoryId,
            RequiredProofRef: spec.RequiredProofRef,
            PassOrRisk: Defer,
            ProofStatus: ProofDeferred,
            BlockingRefResolved: false,
            Deferred: true,
            ConcreteProofRef: null,
            EvidenceRef: categoryRow?.ArtifactRef ?? evidenceRefOverride ?? BuildArtifactRef(options, row, spec),
            RiskCodes: categoryRow?.RiskCodes.Concat(riskCodes).Distinct(StringComparer.Ordinal).ToArray() ?? riskCodes,
            DecisionReason: reason);

    private static IReadOnlyDictionary<string, Quality03ProofContractArtifactRow> RowsById(
        IReadOnlyList<Quality03ProofContractArtifactRow> rows,
        string fileName,
        List<string> errors)
    {
        var result = new Dictionary<string, Quality03ProofContractArtifactRow>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.RowId))
            {
                errors.Add($"{fileName}: row_id is required.");
                continue;
            }

            if (!result.TryAdd(row.RowId, row))
                errors.Add($"{fileName}: duplicate row_id {row.RowId}.");
        }

        return result;
    }

    private static void ValidateUniqueRows(IReadOnlyList<Quality03ProofContractInputRow> rows, List<string> errors)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.RowId))
                errors.Add("per-row-blocking-proof-refs row_id is required.");
            else if (!seen.Add(row.RowId))
                errors.Add($"per-row-blocking-proof-refs duplicate row_id {row.RowId}.");
        }
    }

    private static void ValidateCategoryRowsMatchInputRows(
        IReadOnlyList<Quality03ProofContractInputRow> inputRows,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, Quality03ProofContractArtifactRow>> rowsByCategory,
        List<string> errors)
    {
        var expected = inputRows.Select(row => row.RowId).Where(rowId => !string.IsNullOrWhiteSpace(rowId)).ToHashSet(StringComparer.Ordinal);
        foreach (var spec in CategorySpecs.Where(spec => !string.Equals(spec.CategoryId, "held_out_split", StringComparison.Ordinal)))
        {
            if (!rowsByCategory.TryGetValue(spec.CategoryId, out var rows))
                continue;
            foreach (var rowId in expected.Where(rowId => !rows.ContainsKey(rowId)))
                errors.Add($"{spec.FileName}: missing row_id {rowId}.");
            foreach (var rowId in rows.Keys.Where(rowId => !expected.Contains(rowId)))
                errors.Add($"{spec.FileName}: unexpected row_id {rowId}.");
        }
    }

    private static void ValidateCorpusRowsMatchInputRows(
        IReadOnlyList<Quality03ProofContractInputRow> inputRows,
        Quality03ProofContractApprovedCorpusAuthority authority,
        List<string> errors)
    {
        if (!authority.FilePresent)
            return;

        var expected = inputRows.Select(row => row.RowId).Where(rowId => !string.IsNullOrWhiteSpace(rowId)).ToHashSet(StringComparer.Ordinal);
        foreach (var rowId in expected.Where(rowId => !authority.RowsById.ContainsKey(rowId)))
            errors.Add($"approved-held-out-source-corpus-manifest.json: missing row_id {rowId}.");
        foreach (var rowId in authority.RowsById.Keys.Where(rowId => !expected.Contains(rowId)))
            errors.Add($"approved-held-out-source-corpus-manifest.json: unexpected row_id {rowId}.");
        if (authority.RowCount != RequiredQualityRowCount)
            errors.Add($"approved-held-out-source-corpus-manifest.json rows must contain exactly {RequiredQualityRowCount} rows; actual={authority.RowCount}.");
    }

    private static void ValidateJsonlFileCount(
        string directory,
        string fileName,
        int requiredCount,
        List<string> errors,
        List<string> warnings)
    {
        var path = Path.Combine(directory, fileName);
        if (!File.Exists(path))
        {
            errors.Add($"{fileName} is required.");
            return;
        }

        var count = CountJsonlRows(path, errors, warnings);
        if (count != requiredCount)
            errors.Add($"{fileName} must contain exactly {requiredCount} rows; actual={count}.");
    }

    private static int CountJsonlRows(string path, List<string> errors, List<string> warnings)
    {
        var count = 0;
        var lineNumber = 0;
        try
        {
            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                using var _ = JsonDocument.Parse(line);
                count++;
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"{Path.GetFileName(path)} malformed JSONL at line {lineNumber}: {ex.Message}");
        }
        catch (IOException ex)
        {
            errors.Add($"{Path.GetFileName(path)} could not be read: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            warnings.Add($"{Path.GetFileName(path)} read access failed: {ex.Message}");
        }

        return count;
    }

    private static IReadOnlyList<BlockingRow> ReadBlockingRows(string path, List<string> errors)
    {
        if (!File.Exists(path))
        {
            errors.Add("proof_contract_pack/per-row-blocking-proof-refs.jsonl is required.");
            return [];
        }

        var rows = new List<BlockingRow>();
        var lineNumber = 0;
        try
        {
            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                using var doc = JsonDocument.Parse(line);
                var root = doc.RootElement;
                rows.Add(new BlockingRow(
                    Row: ParseInputRow(root),
                    CategoryResultsCount: GetArrayCount(root, "category_results")));
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"per-row-blocking-proof-refs.jsonl malformed JSONL at line {lineNumber}: {ex.Message}");
        }
        catch (IOException ex)
        {
            errors.Add($"per-row-blocking-proof-refs.jsonl could not be read: {ex.Message}");
        }

        return rows;
    }

    private static IReadOnlyList<Quality03ProofContractArtifactRow> ReadArtifactRows(
        string path,
        string expectedCategoryId,
        List<string> errors)
    {
        if (!File.Exists(path))
        {
            errors.Add($"{Path.GetFileName(path)} is required.");
            return [];
        }

        var rows = new List<Quality03ProofContractArtifactRow>();
        var lineNumber = 0;
        try
        {
            foreach (var line in File.ReadLines(path, Encoding.UTF8))
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                using var doc = JsonDocument.Parse(line);
                rows.Add(ParseArtifactRow(doc.RootElement, expectedCategoryId));
            }
        }
        catch (JsonException ex)
        {
            errors.Add($"{Path.GetFileName(path)} malformed JSONL at line {lineNumber}: {ex.Message}");
        }
        catch (IOException ex)
        {
            errors.Add($"{Path.GetFileName(path)} could not be read: {ex.Message}");
        }

        return rows;
    }

    private static AuthorizedSecondRaterPacket ReadAuthorizedSecondRaterPacket(
        Quality03ProofContractRunnerOptions options,
        IReadOnlyList<Quality03ProofContractInputRow> inputRows,
        List<string> warnings)
    {
        if (string.IsNullOrWhiteSpace(options.SecondRaterPacketPath))
            return AuthorizedSecondRaterPacket.NotProvided;

        var resolved = ResolveAuthorizedSecondRaterPacketPath(options.SecondRaterPacketPath, warnings);
        if (string.IsNullOrWhiteSpace(resolved))
            return new AuthorizedSecondRaterPacket(
                Provided: true,
                NormalizedPacketPath: SafeNormalizePath(options.SecondRaterPacketPath),
                RowsById: new Dictionary<string, Quality03ProofContractArtifactRow>(StringComparer.Ordinal),
                RowSetMatchesExpectedRows: false);

        var rows = ReadArtifactRows(resolved, "inter_rater_adjudication", warnings);
        var rowsById = RowsById(rows, AuthorizedSecondRaterPacketFileName, warnings);
        var expected = inputRows
            .Select(row => row.RowId)
            .Where(rowId => !string.IsNullOrWhiteSpace(rowId))
            .ToHashSet(StringComparer.Ordinal);
        var rowSetMatches = rows.Count == RequiredQualityRowCount &&
                            rowsById.Count == RequiredQualityRowCount &&
                            expected.SetEquals(rowsById.Keys);
        if (!rowSetMatches)
            warnings.Add($"{AuthorizedSecondRaterPacketFileName}: authorized packet row_id set does not exactly match current proof-contract input rows.");

        return new AuthorizedSecondRaterPacket(
            Provided: true,
            NormalizedPacketPath: NormalizePath(resolved),
            RowsById: rowsById,
            RowSetMatchesExpectedRows: rowSetMatches);
    }

    private static string? ResolveAuthorizedSecondRaterPacketPath(string packetPath, List<string> warnings)
    {
        try
        {
            var fullPath = Path.GetFullPath(packetPath);
            if (Directory.Exists(fullPath))
            {
                var candidate = Path.Combine(fullPath, AuthorizedSecondRaterPacketFileName);
                if (File.Exists(candidate))
                    return candidate;
                warnings.Add($"{AuthorizedSecondRaterPacketFileName} not found under authorized second-rater packet directory: {fullPath}");
                return null;
            }

            if (File.Exists(fullPath))
                return fullPath;

            warnings.Add($"Authorized second-rater packet path not found: {fullPath}");
            return null;
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            warnings.Add($"Authorized second-rater packet path is invalid: {ex.Message}");
            return null;
        }
    }

    private static Quality03ProofContractApprovedCorpusAuthority ReadApprovedCorpusAuthority(
        string path,
        List<string> errors,
        List<string> warnings)
    {
        if (!File.Exists(path))
        {
            errors.Add("approved-held-out-source-corpus-manifest.json is required.");
            return new Quality03ProofContractApprovedCorpusAuthority(false, false, string.Empty, 0, false, false, false, new Dictionary<string, Quality03ProofContractApprovedCorpusRow>());
        }

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            var root = doc.RootElement;
            var rows = new Dictionary<string, Quality03ProofContractApprovedCorpusRow>(StringComparer.Ordinal);
            if (root.TryGetProperty("rows", out var rowsElement) && rowsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var rowElement in rowsElement.EnumerateArray())
                {
                    var row = new Quality03ProofContractApprovedCorpusRow(
                        RowId: GetString(rowElement, "row_id"),
                        PairId: GetString(rowElement, "pair_id"),
                        Host: GetString(rowElement, "host"),
                        Method: GetString(rowElement, "method"),
                        PromptSha256: GetString(rowElement, "prompt_sha256"),
                        ApprovedAssignment: GetString(rowElement, "approved_assignment"),
                        SourceDisjointPass: GetBool(rowElement, "source_disjoint_pass"));
                    if (!string.IsNullOrWhiteSpace(row.RowId))
                        rows[row.RowId] = row;
                }
            }
            else
            {
                errors.Add("approved-held-out-source-corpus-manifest.json rows array is required.");
            }

            return new Quality03ProofContractApprovedCorpusAuthority(
                FilePresent: true,
                Approved: GetBool(root, "approved"),
                ManifestStatus: GetString(root, "manifest_status"),
                RowCount: rows.Count,
                HasApprovedTrainSourceCorpusManifestRef: HasAnyString(root, "approved_train_source_corpus_manifest_ref", "train_source_corpus_manifest_ref", "corpus_manifest_ref"),
                HasApprovedHeldOutSplitManifestRef: HasAnyString(root, "approved_held_out_split_manifest_ref", "held_out_split_manifest_ref", "split_manifest_ref"),
                HasApprovedRawPrivateCorpusRef: HasAnyString(root, "approved_raw_private_corpus_ref", "raw_private_corpus_ref", "private_corpus_pointer"),
                RowsById: rows);
        }
        catch (JsonException ex)
        {
            errors.Add($"approved-held-out-source-corpus-manifest.json malformed JSON: {ex.Message}");
        }
        catch (IOException ex)
        {
            errors.Add($"approved-held-out-source-corpus-manifest.json could not be read: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            warnings.Add($"approved-held-out-source-corpus-manifest.json read access failed: {ex.Message}");
        }

        return new Quality03ProofContractApprovedCorpusAuthority(false, false, string.Empty, 0, false, false, false, new Dictionary<string, Quality03ProofContractApprovedCorpusRow>());
    }

    private static Quality03ProofContractNearestNeighbourIndexAuthority ReadNearestNeighbourIndexAuthority(
        string path,
        List<string> errors,
        List<string> warnings)
    {
        if (!File.Exists(path))
        {
            errors.Add("row-specific-nearest-neighbour-index-manifest.json is required.");
            return new Quality03ProofContractNearestNeighbourIndexAuthority(false, false, false, string.Empty, string.Empty, string.Empty, null, null);
        }

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(path, Encoding.UTF8));
            var root = doc.RootElement;
            return new Quality03ProofContractNearestNeighbourIndexAuthority(
                FilePresent: true,
                IndexBuilt: GetBool(root, "index_built"),
                Pass: GetBool(root, "pass"),
                ManifestStatus: GetString(root, "manifest_status"),
                CorpusManifestRef: GetString(root, "corpus_manifest_ref"),
                IndexAlgorithm: GetString(root, "index_algorithm"),
                SimilarityThreshold: GetNullableDouble(root, "similarity_threshold"),
                TopK: GetNullableInt(root, "top_k"));
        }
        catch (JsonException ex)
        {
            errors.Add($"row-specific-nearest-neighbour-index-manifest.json malformed JSON: {ex.Message}");
        }
        catch (IOException ex)
        {
            errors.Add($"row-specific-nearest-neighbour-index-manifest.json could not be read: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            warnings.Add($"row-specific-nearest-neighbour-index-manifest.json read access failed: {ex.Message}");
        }

        return new Quality03ProofContractNearestNeighbourIndexAuthority(false, false, false, string.Empty, string.Empty, string.Empty, null, null);
    }

    private static Quality03ProofContractInputRow ParseInputRow(JsonElement root)
        => new(
            RowId: GetString(root, "row_id"),
            PairSequenceId: GetString(root, "pair_sequence_id"),
            PairId: GetString(root, "pair_id"),
            ScenarioId: GetString(root, "scenario_id"),
            SourceScenarioId: GetString(root, "source_scenario_id"),
            Host: GetString(root, "host"),
            Method: GetString(root, "method"),
            EvidenceClass: GetString(root, "evidence_class"),
            PromptSha256: GetString(root, "prompt_sha256"));

    private static Quality03ProofContractArtifactRow ParseArtifactRow(JsonElement root, string expectedCategoryId)
    {
        var categoryId = GetString(root, "category_id");
        return new Quality03ProofContractArtifactRow
        {
            RowId = GetString(root, "row_id"),
            PairSequenceId = GetString(root, "pair_sequence_id"),
            PairId = GetString(root, "pair_id"),
            ScenarioId = GetString(root, "scenario_id"),
            SourceScenarioId = GetString(root, "source_scenario_id"),
            Host = GetString(root, "host"),
            Method = GetString(root, "method"),
            EvidenceClass = GetString(root, "evidence_class"),
            PromptSha256 = GetString(root, "prompt_sha256"),
            CategoryId = string.IsNullOrWhiteSpace(categoryId) ? expectedCategoryId : categoryId,
            ArtifactRef = GetString(root, "artifact_ref"),
            Pass = GetBool(root, "pass"),
            PassOrRisk = GetString(root, "pass_or_risk"),
            ProofStatus = GetString(root, "proof_status"),
            RiskStatus = GetString(root, "risk_status"),
            BlockingRefResolved = GetBool(root, "blocking_ref_resolved"),
            Deferred = GetBool(root, "deferred"),
            ConcreteProofRef = GetNullableString(root, "concrete_proof_ref"),
            ExternalAiModelCalled = GetBool(root, "external_ai_model_called"),
            TrainingDpoCheckpointModelLocatorMutation = GetBool(root, "training_dpo_checkpoint_model_locator_mutation"),
            IndexContractPresent = GetBool(root, "index_contract_present"),
            TopKResultsCount = GetArrayCount(root, "top_k_results"),
            RunnerExecuted = GetBool(root, "runner_executed"),
            RunnerContractAvailableForCurrentRows = GetBool(root, "runner_contract_available_for_current_rows"),
            ShortcutOnly = GetBool(root, "shortcut_only") || GetBool(root, "shortcut_only_result") || ContainsShortcutOnly(root),
            RunnerStatus = FirstString(root, "runner_status", "result_status", "execution_status"),
            ControlsRequired = GetStringArray(root, "controls_required"),
            ControlsRun = GetStringArray(root, "controls_run"),
            ReplayProbeExecuted = GetBool(root, "replay_probe_executed"),
            PassBoundaryFlagsOnly = GetBool(root, "pass_boundary_flags_only"),
            ExactMatchScanScope = GetString(root, "exact_match_scan_scope"),
            LongestCommonFragmentTokenCount = GetNullableInt(root, "longest_common_fragment_token_count"),
            ApprovedRawPrivateRefs = GetApprovedRawPrivateRefs(root),
            IndependentSecondRaterAvailable = GetBool(root, "independent_second_rater_available"),
            ReviewerId1 = GetString(root, "reviewer_id_1"),
            ReviewerId2 = GetString(root, "reviewer_id_2"),
            AdjudicationStatus = GetString(root, "adjudication_status"),
            FakedIndependence = GetBool(root, "faked_independence"),
            PassScope = GetString(root, "pass_scope"),
            CanSatisfyInterRaterAdjudicationProof = GetBool(root, "can_satisfy_inter_rater_adjudication_proof"),
            RunnerReadyAfterThisItem = GetBool(root, "runner_ready_after_this_item"),
            QualityPassAfterThisItem = GetBool(root, "quality_pass_after_this_item"),
            RiskCodes = GetStringArray(root, "risk_codes")
        };
    }

    private static bool HasApprovedCorpusAuthorityForRow(Quality03ProofContractApprovedCorpusAuthority authority, string rowId)
        => authority.FilePresent &&
           authority.Approved &&
           !LooksDeferredOrRisk(authority.ManifestStatus) &&
           authority.HasApprovedTrainSourceCorpusManifestRef &&
           authority.HasApprovedHeldOutSplitManifestRef &&
           authority.HasApprovedRawPrivateCorpusRef &&
           authority.RowsById.TryGetValue(rowId, out var row) &&
           !string.IsNullOrWhiteSpace(row.ApprovedAssignment) &&
           !row.ApprovedAssignment.Contains("NONE", StringComparison.OrdinalIgnoreCase) &&
           !row.ApprovedAssignment.Contains("DEFER", StringComparison.OrdinalIgnoreCase);

    private static bool HasSourceDisjointAuthorityForRow(Quality03ProofContractApprovedCorpusAuthority authority, string rowId)
        => authority.RowsById.TryGetValue(rowId, out var row) && row.SourceDisjointPass;

    private static bool NearestNeighbourIndexPasses(Quality03ProofContractNearestNeighbourIndexAuthority authority)
        => authority.FilePresent &&
           authority.IndexBuilt &&
           authority.Pass &&
           !LooksDeferredOrRisk(authority.ManifestStatus) &&
           !string.IsNullOrWhiteSpace(authority.CorpusManifestRef) &&
           !string.Equals(authority.IndexAlgorithm, "NOT_RUN", StringComparison.OrdinalIgnoreCase) &&
           authority.SimilarityThreshold.HasValue &&
           authority.TopK is > 0;

    private static bool CategoryArtifactPasses(Quality03ProofContractArtifactRow? row)
        => row is not null &&
           row.Pass &&
           row.BlockingRefResolved &&
           (string.Equals(row.PassOrRisk, Pass, StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(row.PassOrRisk)) &&
           (string.Equals(row.ProofStatus, ProofPass, StringComparison.OrdinalIgnoreCase) || IsPassLike(row.ProofStatus));

    private static bool AllControlsRan(Quality03ProofContractArtifactRow row)
    {
        if (row.ControlsRequired.Count == 0)
            return false;
        var run = row.ControlsRun.ToHashSet(StringComparer.Ordinal);
        return row.ControlsRequired.All(run.Contains);
    }

    private static bool AdjudicationPasses(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           !LooksDeferredOrRisk(value) &&
           (value.Contains("PASS", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("ADJUDICATED", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("COMPLETE", StringComparison.OrdinalIgnoreCase));

    private static bool SecondRaterPacketRowMatchesInputRow(
        Quality03ProofContractArtifactRow packetRow,
        Quality03ProofContractInputRow inputRow)
        => string.Equals(packetRow.RowId, inputRow.RowId, StringComparison.Ordinal) &&
           string.Equals(packetRow.PairId, inputRow.PairId, StringComparison.Ordinal) &&
           string.Equals(packetRow.ScenarioId, inputRow.ScenarioId, StringComparison.Ordinal);

    private static bool IsInterRaterProofOnlyPassScope(string value)
    {
        var normalized = value.Replace('-', '_').ToUpperInvariant();
        return string.Equals(normalized, InterRaterProofOnlyPassScope, StringComparison.Ordinal) ||
               string.Equals(normalized, "INTER_RATER_ADJUDICATION_PROOF_ONLY", StringComparison.Ordinal) ||
               string.Equals(normalized, "INTER_RATER_PROOF_ONLY", StringComparison.Ordinal);
    }

    private static bool LooksLikeSelfChecker(string reviewerId)
        => reviewerId.Contains("self", StringComparison.OrdinalIgnoreCase) ||
           reviewerId.Contains("deterministic", StringComparison.OrdinalIgnoreCase) ||
           reviewerId.Contains("checker", StringComparison.OrdinalIgnoreCase);

    private static bool LooksDeferredOrRisk(string value)
        => string.IsNullOrWhiteSpace(value) ||
           value.Contains("DEFER", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("RISK", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("NOT_PASS", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("NOT_APPROVED", StringComparison.OrdinalIgnoreCase);

    private static bool IsPassLike(string value)
        => !LooksDeferredOrRisk(value) &&
           value.Contains("PASS", StringComparison.OrdinalIgnoreCase);

    private static bool IsNotRunStatus(string value)
        => value.Contains("NOT_RUN", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("NOT_EXECUTED", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeCode(string label)
        => label.Replace('-', '_').ToUpperInvariant();

    private static Quality03ProofContractArtifactRow? GetCategoryRow(
        LoadedPacks loaded,
        string categoryId,
        string rowId)
        => loaded.RowsByCategory.TryGetValue(categoryId, out var rows) && rows.TryGetValue(rowId, out var row)
            ? row
            : null;

    private static string BuildArtifactRef(
        Quality03ProofContractRunnerOptions options,
        Quality03ProofContractInputRow row,
        CategorySpec spec)
        => NormalizePath(Path.Combine(options.ProofContractPackDirectory, spec.FileName)) + "#" + row.RowId;

    private static string ResolveRequiredOutputPath(string outputRoot, string relativePath)
    {
        var result = new Quality03ProofContractRunner().ResolveOutputPath(outputRoot, relativePath);
        if (!result.IsSuccess || string.IsNullOrWhiteSpace(result.Value))
            throw new IOException(result.ErrorMessage);
        return result.Value;
    }

    private static Quality03ProofContractRunnerReadyInputRow[] BuildRunnerReadyRows(Quality03ProofContractRunnerReport report)
        => report.RowDecisions
            .Where(row => row.RunnerReadyCandidate)
            .Select(row => new Quality03ProofContractRunnerReadyInputRow(
                Schema: RunnerReadyInputSchema,
                WorkOrderId: report.WorkOrderId,
                RunId: report.RunId,
                GeneratedAtUtc: report.GeneratedAtUtc,
                RowId: row.RowId,
                PairSequenceId: row.PairSequenceId,
                PairId: row.PairId,
                ScenarioId: row.ScenarioId,
                Host: row.Host,
                Method: row.Method,
                PromptSha256: row.PromptSha256,
                BlockingProofRefs: row.CategoryResults.Select(category => category.ConcreteProofRef ?? category.EvidenceRef).ToArray(),
                QualityImprovementClaimed: false,
                SemanticQualityClaimed: false,
                LearnabilityClaimed: false,
                ProductReadinessClaimed: false,
                ExternalAiModelCalled: false,
                TrainingDpoCheckpointModelLocatorMutation: false))
            .ToArray();

    private static string BuildRefusalJson(Quality03ProofContractRunnerReport report)
        => JsonSerializer.Serialize(
               new
               {
                   schema = "xiigen.quality_03.proof_contract_runner.refusal_report.v1",
                   report.WorkOrderId,
                   report.RunId,
                   report.GeneratedAtUtc,
                   reason = "runner-ready-eval-inputs.jsonl is refused until all 63 blocking proof refs PASS.",
                   report.BlockingProofRefsTotal,
                   report.BlockingProofRefsPass,
                   report.BlockingProofRefsRiskOrDeferred,
                   report.RunnerReadyRows,
                   report.QualityPassRows,
                   report.MissingOrDeferredRefs,
                   report.ClaimLocks,
                   report.ExternalAiModelCalled,
                   report.TrainingDpoCheckpointModelLocatorMutation
                },
               JsonOptions);

    private static void WriteUtf8LfText(string path, string content)
        => File.WriteAllText(path, NormalizeTextArtifact(content), Utf8NoBom);

    private static string BuildJsonLines(IEnumerable<string> rows)
    {
        var materializedRows = rows.ToArray();
        return materializedRows.Length == 0
            ? string.Empty
            : string.Join(Lf, materializedRows) + Lf;
    }

    private static string NormalizeTextArtifact(string content)
    {
        var lfText = content.Replace("\r\n", Lf).Replace("\r", Lf);
        var lines = lfText.Split('\n');
        for (var index = 0; index < lines.Length; index++)
            lines[index] = lines[index].TrimEnd(' ', '\t');

        var normalized = string.Join(Lf, lines);
        return normalized.Length == 0 || normalized.EndsWith(Lf, StringComparison.Ordinal)
            ? normalized
            : normalized + Lf;
    }

    private static string BuildMarkdown(Quality03ProofContractRunnerReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-03 Proof Contract Runner Report");
        builder.AppendLine();
        builder.AppendLine(DeterministicRunnerLabel);
        builder.AppendLine();
        builder.AppendLine($"- rows_seen: {report.QualityRowsSeen}");
        builder.AppendLine($"- second_rater_packet_path: {report.SecondRaterPacketPath}");
        builder.AppendLine($"- blocking_proof_refs_pass: {report.BlockingProofRefsPass}/{report.BlockingProofRefsTotal}");
        builder.AppendLine($"- runner_ready_rows: {report.RunnerReadyRows}");
        builder.AppendLine($"- quality_pass_rows: {report.QualityPassRows}");
        builder.AppendLine($"- runner_ready_eval_inputs_created: {report.RunnerReadyEvalInputsCreated.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- runner_scorer_run: {report.RunnerScorerRun.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- external_ai_model_called: {report.ExternalAiModelCalled.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- training_dpo_checkpoint_model_locator_mutation: {report.TrainingDpoCheckpointModelLocatorMutation.ToString().ToLowerInvariant()}");
        builder.AppendLine();
        builder.AppendLine("## Claim Locks");
        builder.AppendLine();
        builder.AppendLine($"- quality_improvement_claimed: {report.ClaimLocks.QualityImprovementClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- semantic_quality_claimed: {report.ClaimLocks.SemanticQualityClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- semantic_readiness_claimed: {report.ClaimLocks.SemanticReadinessClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- learnability_claimed: {report.ClaimLocks.LearnabilityClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- product_readiness_claimed: {report.ClaimLocks.ProductReadinessClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- final_readiness_claimed: {report.ClaimLocks.FinalReadinessClaimed.ToString().ToLowerInvariant()}");
        builder.AppendLine();
        builder.AppendLine("## Scope");
        builder.AppendLine();
        builder.AppendLine(report.WhatThisProves);
        builder.AppendLine();
        builder.AppendLine(report.WhatThisDoesNotProve);
        builder.AppendLine();
        builder.AppendLine($"Next bounded work order: {report.NextBoundedWorkOrder}");
        return builder.ToString();
    }

    private static string BuildMarkdownRu(Quality03ProofContractRunnerReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-03: отчет proof contract runner");
        builder.AppendLine();
        builder.AppendLine("Это локальная детерминированная проверка артефактов proof-contract. Она не является оценкой качества, готовности продукта, обучением или модельным выводом.");
        builder.AppendLine();
        builder.AppendLine($"- строк проверено: {report.QualityRowsSeen}");
        builder.AppendLine($"- authorized second-rater packet: {EmptyAsNone(report.SecondRaterPacketPath)}");
        builder.AppendLine($"- proof PASS: {report.BlockingProofRefsPass}/{report.BlockingProofRefsTotal}");
        builder.AppendLine($"- runner-ready rows: {report.RunnerReadyRows}");
        builder.AppendLine($"- quality-pass rows: {report.QualityPassRows}");
        builder.AppendLine($"- runner-ready eval inputs created: {report.RunnerReadyEvalInputsCreated.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- scorer/runner executed: {report.RunnerScorerRun.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- external_ai_model_called: {report.ExternalAiModelCalled.ToString().ToLowerInvariant()}");
        builder.AppendLine($"- training_dpo_checkpoint_model_locator_mutation: {report.TrainingDpoCheckpointModelLocatorMutation.ToString().ToLowerInvariant()}");
        builder.AppendLine();
        builder.AppendLine("## Что это доказывает");
        builder.AppendLine();
        builder.AppendLine("Только то, что локальный runner распарсил supplied evidence/proof artifacts и применил gates к 63 blocking proof refs.");
        builder.AppendLine();
        builder.AppendLine("## Чего это не доказывает");
        builder.AppendLine();
        builder.AppendLine("Не доказывает semantic quality, learnability, product readiness, token savings, training readiness, locator updates или final readiness.");
        builder.AppendLine();
        builder.AppendLine($"Следующий bounded work order: {report.NextBoundedWorkOrder}");
        return builder.ToString();
    }

    private static string GetString(JsonElement root, string propertyName)
        => GetNullableString(root, propertyName) ?? string.Empty;

    private static string? GetNullableString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return null;
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => null
        };
    }

    private static string FirstString(JsonElement root, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var value = GetNullableString(root, propertyName);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return string.Empty;
    }

    private static bool GetBool(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return false;
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) ? parsed : string.Equals(value.GetString(), "PASS", StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }

    private static int? GetNullableInt(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
            return number;
        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed))
            return parsed;
        return null;
    }

    private static double? GetNullableDouble(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
            return number;
        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), out var parsed))
            return parsed;
        return null;
    }

    private static int GetArrayCount(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.GetArrayLength()
            : 0;

    private static IReadOnlyList<string> GetStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return [];
        if (value.ValueKind == JsonValueKind.String)
        {
            var text = value.GetString();
            return string.IsNullOrWhiteSpace(text) ? [] : [text];
        }
        if (value.ValueKind != JsonValueKind.Array)
            return [];

        return value.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : item.GetRawText())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .Select(item => item!)
            .ToArray();
    }

    private static IReadOnlyList<string> GetApprovedRawPrivateRefs(JsonElement root)
    {
        var refs = new List<string>();
        foreach (var propertyName in new[]
        {
            "approved_raw_private_ref",
            "approved_raw_private_refs",
            "approved_private_corpus_ref",
            "approved_private_corpus_refs",
            "approved_raw_private_corpus_ref",
            "approved_raw_private_corpus_refs",
            "approved_output_corpus_ref",
            "approved_output_corpus_refs",
            "raw_private_corpus_ref"
        })
        {
            refs.AddRange(GetStringArray(root, propertyName));
        }

        return refs.Where(value => !string.IsNullOrWhiteSpace(value)).Distinct(StringComparer.Ordinal).ToArray();
    }

    private static bool HasAnyString(JsonElement root, params string[] propertyNames)
        => propertyNames.Any(propertyName => !string.IsNullOrWhiteSpace(GetString(root, propertyName)));

    private static bool ContainsShortcutOnly(JsonElement root)
        => FirstString(root, "ablation_mode", "evidence_mode", "defer_reason", "defer_reason_ru", "decision", "decision_ru")
            .Contains("shortcut", StringComparison.OrdinalIgnoreCase);

    private static string EmptyAsNone(string value)
        => string.IsNullOrWhiteSpace(value) ? "none" : value;

    private static string NormalizePath(string path)
        => Path.GetFullPath(path).Replace('\\', '/');

    private static string SafeNormalizePath(string path)
    {
        try
        {
            return NormalizePath(path);
        }
        catch (Exception ex) when (ex is ArgumentException or NotSupportedException or PathTooLongException)
        {
            return path.Replace('\\', '/');
        }
    }

    private sealed record CategorySpec(string CategoryId, string RequiredProofRef, string FileName);

    private sealed record BlockingRow(Quality03ProofContractInputRow Row, int CategoryResultsCount);

    private sealed record AuthorizedSecondRaterPacket(
        bool Provided,
        string NormalizedPacketPath,
        IReadOnlyDictionary<string, Quality03ProofContractArtifactRow> RowsById,
        bool RowSetMatchesExpectedRows)
    {
        public static AuthorizedSecondRaterPacket NotProvided { get; } = new(
            Provided: false,
            NormalizedPacketPath: string.Empty,
            RowsById: new Dictionary<string, Quality03ProofContractArtifactRow>(StringComparer.Ordinal),
            RowSetMatchesExpectedRows: false);
    }

    private sealed record LoadedPacks(
        Quality03ProofContractRunnerValidation Validation,
        IReadOnlyList<Quality03ProofContractInputRow> InputRows,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, Quality03ProofContractArtifactRow>> RowsByCategory,
        Quality03ProofContractApprovedCorpusAuthority ApprovedCorpus,
        Quality03ProofContractNearestNeighbourIndexAuthority NearestNeighbourIndex,
        AuthorizedSecondRaterPacket AuthorizedSecondRaterPacket);
}
