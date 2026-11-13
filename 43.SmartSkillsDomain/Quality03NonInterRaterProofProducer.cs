// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using AgentInfrastructure;

public sealed class Quality03NonInterRaterProofProducer
{
    public const string ReportSchema = "xiigen.quality_03.non_inter_rater_proof_producer.report.v1";
    public const string PerRowMapSchema = "xiigen.quality_03.non_inter_rater_proof_producer.per_row_map.v1";
    public const string ProofBundleSchema = "xiigen.quality_03.non_inter_rater_proof_bundle.v1";
    public const string ProofArtifactSchema = "xiigen.quality_03.non_inter_rater_proof_artifact.v1";
    public const string DeterministicRunnerLabel = "local deterministic proof-data producer; not semantic model, trainable unit, quality scorer, product readiness, or learnability authority";
    public const int RequiredQualityRowCount = 9;
    public const int RequiredTotalProofRefs = 63;
    public const int InterRaterProofRefsCarriedForward = 9;

    private const string Pass = "PASS";
    private const string Risk = "RISK";
    private const string ProofPass = "PASS";
    private const string ProofDeferred = "DEFERRED_NOT_PASS";
    private const string MissingProofArtifact = "MISSING_PROOF_ARTIFACT";
    private const string InvalidRef = "INVALID_REF";
    private const string NotRunMissingInputs = "NOT_RUN_MISSING_INPUTS";
    private const string ProofBundleFileName = "quality-03-non-inter-rater-proof-bundle.json";
    private const string Lf = "\n";

    private static readonly string[] NonInterRaterCategories =
    [
        "held_out_split",
        "source_disjoint",
        "nearest_neighbour_similarity_risk",
        "ablation",
        "no_leakage",
        "no_memorization"
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

    private static readonly string[] RequiredAblationControls =
    [
        "exact_prompt_lookup_ablation",
        "filename_or_bm25_route_ablation",
        "candidate_position_order_ablation",
        "nearest_only_retrieval_only_ablation"
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

    public OperationResult<Quality03NonInterRaterProofProducerReport> Run(Quality03NonInterRaterProofProducerOptions? options)
    {
        var validation = ValidateOptions(options);
        if (!validation.IsSuccess || options is null)
            return OperationResult<Quality03NonInterRaterProofProducerReport>.Fail(validation.ErrorMessage, validation.ErrorCode);

        var rowLoad = LoadRows(Path.Combine(options.SourceProofContractPackDirectory, "per-row-blocking-proof-refs.jsonl"));
        if (!rowLoad.IsSuccess || rowLoad.Value is null)
            return OperationResult<Quality03NonInterRaterProofProducerReport>.Fail(rowLoad.ErrorMessage, rowLoad.ErrorCode);

        if (rowLoad.Value.Count != RequiredQualityRowCount)
            return OperationResult<Quality03NonInterRaterProofProducerReport>.Fail($"Expected {RequiredQualityRowCount} QUALITY-03 rows; actual={rowLoad.Value.Count}.", "QUALITY_ROW_COUNT_MISMATCH");

        var generatedAt = options.GeneratedAtUtc.ToString("O");
        var runId = string.IsNullOrWhiteSpace(options.RunId)
            ? $"quality-scoring-{options.GeneratedAtUtc:yyyyMMdd-HHmmss}"
            : options.RunId;
        var rows = rowLoad.Value;
        var proofBundle = LoadProofBundle(options, rows);
        var rowAnalyses = BuildRowAnalyses(proofBundle, rows);
        var outputRefs = new List<string>();

        if (options.WriteReports)
        {
            var writeResult = WriteOutputFiles(options, runId, generatedAt, rows, rowAnalyses, outputRefs);
            if (!writeResult.IsSuccess)
                return OperationResult<Quality03NonInterRaterProofProducerReport>.Fail(writeResult.ErrorMessage, writeResult.ErrorCode);
        }

        var counts = BuildCategoryCounts(rowAnalyses);
        var producedPass = counts.Values.Sum(category => category.PassProduced);
        var report = new Quality03NonInterRaterProofProducerReport(
            Schema: ReportSchema,
            WorkOrderId: options.WorkOrderId,
            RunId: runId,
            GeneratedAtUtc: generatedAt,
            EvidencePackDirectory: NormalizePath(options.EvidencePackDirectory),
            SourceProofContractPackDirectory: NormalizePath(options.SourceProofContractPackDirectory),
            OutputDirectory: NormalizePath(options.OutputDirectory),
            ApprovedProofBundleRoot: NormalizeOptionalPath(options.ApprovedProofBundleRoot),
            ValidProofBundleLoaded: proofBundle.Loaded,
            ProofBundleValidationMessages: proofBundle.Messages,
            TotalQualityRows: rows.Count,
            TotalProofRefs: RequiredTotalProofRefs,
            PassBeforeWork: InterRaterProofRefsCarriedForward,
            NonInterRaterProofRefsTotal: RequiredQualityRowCount * NonInterRaterCategories.Length,
            NonInterRaterProofRefsPassProduced: producedPass,
            NonInterRaterProofRefsRiskOrDeferred: RequiredQualityRowCount * NonInterRaterCategories.Length - producedPass,
            InterRaterProofRefsCarriedForward: InterRaterProofRefsCarriedForward,
            BlockingProofRefsPassAfterProducer: InterRaterProofRefsCarriedForward + producedPass,
            RunnerReadyRows: 0,
            QualityPassRows: 0,
            RunnerReadyEvalInputsCreated: false,
            ExternalAiModelCalled: false,
            TrainingDpoCheckpointModelLocatorMutation: false,
            CountsByCategory: counts,
            OutputRefs: outputRefs,
            DeterministicRunnerLabel: DeterministicRunnerLabel,
            WhatThisProves: "This proves only that local deterministic proof producers created consumable QUALITY-03 proof-contract inputs from local files.",
            WhatThisDoesNotProve: "This does not prove semantic quality, token savings, learnability, product readiness, training readiness, target promotion, locator updates, or final readiness.",
            NextBoundedWorkOrder: producedPass == RequiredQualityRowCount * NonInterRaterCategories.Length
                ? "Run Quality03ProofContractRunner with this produced proof-contract pack and the authorized second-rater packet; then run the separately approved quality scorer only if 63/63 proof refs PASS."
                : "QUALITY-03-NON-INTER-RATER-PROOF-DATA-AND-RUNNERS-10B: provide approved corpus/source split refs, prompt/output replay text, and private/raw corpus refs for the rows still marked RISK/DEFER by the producer.");

        if (options.WriteReports)
        {
            var writeReportResult = WriteReportFiles(options, report, rowAnalyses);
            if (!writeReportResult.IsSuccess)
                return OperationResult<Quality03NonInterRaterProofProducerReport>.Fail(writeReportResult.ErrorMessage, writeReportResult.ErrorCode);
        }

        return OperationResult<Quality03NonInterRaterProofProducerReport>.Ok(report);
    }

    private static OperationResult<bool> ValidateOptions(Quality03NonInterRaterProofProducerOptions? options)
    {
        if (options is null)
            return OperationResult<bool>.Fail("Producer options are required.", "OPTIONS_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.EvidencePackDirectory))
            return OperationResult<bool>.Fail("Evidence pack directory is required.", "EVIDENCE_PACK_DIRECTORY_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.SourceProofContractPackDirectory))
            return OperationResult<bool>.Fail("Source proof-contract pack directory is required.", "SOURCE_PROOF_CONTRACT_PACK_DIRECTORY_REQUIRED");
        if (string.IsNullOrWhiteSpace(options.OutputDirectory))
            return OperationResult<bool>.Fail("Output directory is required.", "OUTPUT_DIRECTORY_REQUIRED");
        if (!Directory.Exists(options.EvidencePackDirectory))
            return OperationResult<bool>.Fail($"Evidence pack directory not found: {options.EvidencePackDirectory}", "EVIDENCE_PACK_DIRECTORY_NOT_FOUND");
        if (!Directory.Exists(options.SourceProofContractPackDirectory))
            return OperationResult<bool>.Fail($"Source proof-contract pack directory not found: {options.SourceProofContractPackDirectory}", "SOURCE_PROOF_CONTRACT_PACK_DIRECTORY_NOT_FOUND");
        foreach (var fileName in RequiredEvidenceJsonlFiles)
        {
            var path = Path.Combine(options.EvidencePackDirectory, fileName);
            if (!File.Exists(path))
                return OperationResult<bool>.Fail($"Evidence file not found: {path}", "EVIDENCE_FILE_NOT_FOUND");
        }

        var blockingPath = Path.Combine(options.SourceProofContractPackDirectory, "per-row-blocking-proof-refs.jsonl");
        if (!File.Exists(blockingPath))
            return OperationResult<bool>.Fail($"Blocking proof refs file not found: {blockingPath}", "BLOCKING_PROOF_REFS_NOT_FOUND");
        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<IReadOnlyList<ProducerInputRow>> LoadRows(string path)
    {
        try
        {
            var rows = File.ReadLines(path, Encoding.UTF8)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => JsonNode.Parse(line)!.AsObject())
                .Select(ReadInputRow)
                .ToArray();
            return OperationResult<IReadOnlyList<ProducerInputRow>>.Ok(rows);
        }
        catch (JsonException ex)
        {
            return OperationResult<IReadOnlyList<ProducerInputRow>>.Fail($"Could not parse blocking proof refs JSONL: {ex.Message}", "BLOCKING_PROOF_REFS_PARSE_FAILED");
        }
        catch (IOException ex)
        {
            return OperationResult<IReadOnlyList<ProducerInputRow>>.Fail($"Could not read blocking proof refs JSONL: {ex.Message}", "BLOCKING_PROOF_REFS_READ_FAILED");
        }
    }

    private static ProducerInputRow ReadInputRow(JsonObject root)
        => new(
            RowId: GetString(root, "row_id"),
            PairSequenceId: GetString(root, "pair_sequence_id"),
            PairId: GetString(root, "pair_id"),
            ScenarioId: GetString(root, "scenario_id"),
            SourceScenarioId: GetString(root, "source_scenario_id"),
            Host: GetString(root, "host"),
            Method: GetString(root, "method"),
            EvidenceClass: GetString(root, "evidence_class"),
            PromptSha256: GetString(root, "prompt_sha256"),
            PromptText: FirstString(root, "prompt_text", "scenario_prompt", "raw_prompt_text", "input_text"),
            BaselineOutputText: FirstString(root, "baseline_output_text", "baseline_response_text", "baseline_output"),
            AdaptiveOutputText: FirstString(root, "adaptive_output_text", "adaptive_response_text", "adaptive_output"));

    private static ValidatedProofBundle LoadProofBundle(
        Quality03NonInterRaterProofProducerOptions options,
        IReadOnlyList<ProducerInputRow> rows)
    {
        var messages = new List<string>();
        var missingReason = HasLegacyApprovedRefs(options) ? InvalidRef : MissingProofArtifact;
        if (string.IsNullOrWhiteSpace(options.ApprovedProofBundleRoot))
        {
            messages.Add($"{missingReason}: proof bundle root is missing.");
            if (HasLegacyApprovedRefs(options))
                messages.Add($"{InvalidRef}: approved ref strings are evidence labels only and cannot authorize PASS without a local proof bundle.");
            return MissingProofBundle(rows, missingReason, messages);
        }

        var rootPathResult = ResolveProofBundlePath(options.ApprovedProofBundleRoot);
        if (!rootPathResult.IsSuccess || string.IsNullOrWhiteSpace(rootPathResult.Value))
        {
            messages.Add($"{rootPathResult.ErrorCode}: {rootPathResult.ErrorMessage}");
            return MissingProofBundle(rows, rootPathResult.ErrorCode == InvalidRef ? InvalidRef : MissingProofArtifact, messages);
        }

        try
        {
            var bundlePath = rootPathResult.Value;
            using var doc = JsonDocument.Parse(File.ReadAllText(bundlePath, Encoding.UTF8));
            var root = doc.RootElement;
            if (!string.Equals(GetString(root, "schema"), ProofBundleSchema, StringComparison.Ordinal))
            {
                messages.Add($"{InvalidRef}: proof bundle schema must be {ProofBundleSchema}.");
                return MissingProofBundle(rows, InvalidRef, messages);
            }

            if (!IsPassOrApproved(FirstString(root, "status", "approval_status", "proof_status")))
            {
                messages.Add($"{InvalidRef}: proof bundle status must be PASS or APPROVED.");
                return MissingProofBundle(rows, InvalidRef, messages);
            }

            var resultMap = NewMissingResultMap(rows, MissingProofArtifact);
            var rowsById = rows.ToDictionary(row => row.RowId, StringComparer.Ordinal);
            if (!root.TryGetProperty("rows", out var bundleRows) || bundleRows.ValueKind != JsonValueKind.Array)
            {
                messages.Add($"{InvalidRef}: proof bundle rows array is required.");
                return MissingProofBundle(rows, InvalidRef, messages);
            }

            foreach (var bundleRow in bundleRows.EnumerateArray())
            {
                var rowId = GetString(bundleRow, "row_id");
                if (string.IsNullOrWhiteSpace(rowId) || !rowsById.TryGetValue(rowId, out var expectedRow))
                {
                    messages.Add($"{InvalidRef}: proof bundle contains unknown or empty row_id '{rowId}'.");
                    continue;
                }

                if (!BundleRowMatchesInput(bundleRow, expectedRow, messages))
                    continue;

                if (!bundleRow.TryGetProperty("proofs", out var proofs) || proofs.ValueKind != JsonValueKind.Object)
                {
                    messages.Add($"{MissingProofArtifact}: proof bundle row {rowId} has no proofs object.");
                    continue;
                }

                foreach (var category in NonInterRaterCategories)
                {
                    if (!proofs.TryGetProperty(category, out var proofRef))
                    {
                        resultMap[rowId][category] = MissingProofResult(category, MissingProofArtifact);
                        continue;
                    }

                    resultMap[rowId][category] = ValidateProofArtifact(bundlePath, proofRef, expectedRow, category, messages);
                }
            }

            return new ValidatedProofBundle(
                Loaded: true,
                RootPath: NormalizePath(bundlePath),
                Messages: messages,
                ResultsByRow: resultMap.ToDictionary(
                    row => row.Key,
                    row => (IReadOnlyDictionary<string, ValidatedProofResult>)row.Value,
                    StringComparer.Ordinal));
        }
        catch (JsonException ex)
        {
            messages.Add($"{InvalidRef}: proof bundle JSON is malformed: {ex.Message}");
            return MissingProofBundle(rows, InvalidRef, messages);
        }
        catch (IOException ex)
        {
            messages.Add($"{MissingProofArtifact}: proof bundle could not be read: {ex.Message}");
            return MissingProofBundle(rows, MissingProofArtifact, messages);
        }
        catch (UnauthorizedAccessException ex)
        {
            messages.Add($"{MissingProofArtifact}: proof bundle could not be accessed: {ex.Message}");
            return MissingProofBundle(rows, MissingProofArtifact, messages);
        }
    }

    private static OperationResult<string> ResolveProofBundlePath(string root)
    {
        if (Uri.TryCreate(root, UriKind.Absolute, out var uri) && !uri.IsFile)
            return OperationResult<string>.Fail($"Proof bundle root must be a local file or directory path: {root}", InvalidRef);

        var localPath = Uri.TryCreate(root, UriKind.Absolute, out var fileUri) && fileUri.IsFile
            ? fileUri.LocalPath
            : root;
        var fullPath = Path.GetFullPath(localPath);
        var bundlePath = Directory.Exists(fullPath)
            ? Path.Combine(fullPath, ProofBundleFileName)
            : fullPath;
        if (!File.Exists(bundlePath))
            return OperationResult<string>.Fail($"Proof bundle file not found: {bundlePath}", MissingProofArtifact);
        return OperationResult<string>.Ok(bundlePath);
    }

    private static bool BundleRowMatchesInput(JsonElement bundleRow, ProducerInputRow expectedRow, List<string> messages)
    {
        var rowMatches =
            RequiredStringMatches(bundleRow, "row_id", expectedRow.RowId) &&
            RequiredStringMatches(bundleRow, "scenario_id", expectedRow.ScenarioId) &&
            RequiredStringMatches(bundleRow, "prompt_sha256", expectedRow.PromptSha256);
        if (!rowMatches)
            messages.Add($"{InvalidRef}: proof bundle row {GetString(bundleRow, "row_id")} does not match row_id, scenario_id, and prompt_sha256.");
        if (!OptionalStringMatches(bundleRow, "pair_id", expectedRow.PairId))
        {
            messages.Add($"{InvalidRef}: proof bundle row {expectedRow.RowId} pair_id does not match.");
            rowMatches = false;
        }

        return rowMatches;
    }

    private static ValidatedProofResult ValidateProofArtifact(
        string bundlePath,
        JsonElement proofRef,
        ProducerInputRow expectedRow,
        string categoryId,
        List<string> messages)
    {
        var artifactRef = ProofArtifactRef(proofRef);
        if (string.IsNullOrWhiteSpace(artifactRef))
            return MissingProofResult(categoryId, MissingProofArtifact);

        var artifactPathResult = ResolveProofArtifactPath(bundlePath, artifactRef);
        if (!artifactPathResult.IsSuccess || string.IsNullOrWhiteSpace(artifactPathResult.Value))
        {
            messages.Add($"{artifactPathResult.ErrorCode}: {expectedRow.RowId}/{categoryId} proof artifact ref did not validate: {artifactPathResult.ErrorMessage}");
            return MissingProofResult(categoryId, artifactPathResult.ErrorCode == InvalidRef ? InvalidRef : MissingProofArtifact);
        }

        var artifactPath = artifactPathResult.Value;
        var expectedSha = ProofArtifactSha(proofRef);
        if (!string.IsNullOrWhiteSpace(expectedSha) && !string.Equals(expectedSha, Sha256File(artifactPath), StringComparison.OrdinalIgnoreCase))
            return MissingProofResult(categoryId, InvalidRef);

        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(artifactPath, Encoding.UTF8));
            var root = doc.RootElement;
            var risks = new List<string>();
            if (!string.Equals(GetString(root, "schema"), ProofArtifactSchema, StringComparison.Ordinal))
                risks.Add("INVALID_PROOF_ARTIFACT_SCHEMA");
            if (!string.Equals(GetString(root, "category_id"), categoryId, StringComparison.Ordinal))
                risks.Add("PROOF_ARTIFACT_CATEGORY_MISMATCH");
            if (!ArtifactRowMatchesInput(root, expectedRow))
                risks.Add("PROOF_ARTIFACT_ROW_SCENARIO_HASH_MISMATCH");
            if (!IsPassOrApproved(FirstString(root, "status", "approval_status", "proof_status")) && !GetBool(root, "pass"))
                risks.Add("PROOF_ARTIFACT_STATUS_NOT_PASS_OR_APPROVED");
            if (GetBool(root, "external_ai_model_called"))
                risks.Add("EXTERNAL_AI_MODEL_CALLED");
            if (GetBool(root, "training_dpo_checkpoint_model_locator_mutation"))
                risks.Add("TRAINING_DPO_CHECKPOINT_MODEL_LOCATOR_MUTATION");

            var details = ValidateCategorySpecificArtifact(root, categoryId);
            risks.AddRange(details.RiskCodes);
            var pass = risks.Count == 0;
            if (!pass)
                messages.Add($"{InvalidRef}: {expectedRow.RowId}/{categoryId} proof artifact did not validate: {string.Join(",", risks)}.");

            return new ValidatedProofResult(
                CategoryId: categoryId,
                Pass: pass,
                ProofStatus: pass ? ProofPass : FirstRiskStatus(risks),
                ArtifactRef: NormalizePath(artifactPath),
                ControlActuallyRan: details.ControlActuallyRan,
                RunnerContractAvailable: details.RunnerContractAvailable,
                ShortcutOnly: details.ShortcutOnly,
                RunnerStatus: details.RunnerStatus,
                ControlsRun: details.ControlsRun,
                TopKResults: details.TopKResults,
                ApprovedRawPrivateRefs: details.ApprovedRawPrivateRefs,
                LongestCommonFragmentTokenCount: details.LongestCommonFragmentTokenCount,
                TrainSourceCorpusManifestRef: details.TrainSourceCorpusManifestRef,
                HeldOutSplitManifestRef: details.HeldOutSplitManifestRef,
                RawPrivateCorpusRef: details.RawPrivateCorpusRef,
                ApprovedAssignment: details.ApprovedAssignment,
                RiskCodes: pass ? [] : risks.Distinct(StringComparer.Ordinal).ToArray());
        }
        catch (JsonException ex)
        {
            messages.Add($"{InvalidRef}: proof artifact JSON is malformed for {expectedRow.RowId}/{categoryId}: {ex.Message}");
            return MissingProofResult(categoryId, InvalidRef);
        }
        catch (IOException ex)
        {
            messages.Add($"{MissingProofArtifact}: proof artifact could not be read for {expectedRow.RowId}/{categoryId}: {ex.Message}");
            return MissingProofResult(categoryId, MissingProofArtifact);
        }
        catch (UnauthorizedAccessException ex)
        {
            messages.Add($"{MissingProofArtifact}: proof artifact could not be accessed for {expectedRow.RowId}/{categoryId}: {ex.Message}");
            return MissingProofResult(categoryId, MissingProofArtifact);
        }
    }

    private static OperationResult<string> ResolveProofArtifactPath(string bundlePath, string artifactRef)
    {
        var refWithoutFragment = artifactRef.Split('#')[0];
        if (Uri.TryCreate(refWithoutFragment, UriKind.Absolute, out var uri) && !uri.IsFile)
            return OperationResult<string>.Fail($"Proof artifact ref must be a local file path: {artifactRef}", InvalidRef);

        var localPath = Uri.TryCreate(refWithoutFragment, UriKind.Absolute, out var fileUri) && fileUri.IsFile
            ? fileUri.LocalPath
            : refWithoutFragment;
        var path = Path.IsPathRooted(localPath)
            ? localPath
            : Path.Combine(Path.GetDirectoryName(bundlePath)!, localPath);
        var fullPath = Path.GetFullPath(path);
        var bundleRoot = Path.GetFullPath(Path.GetDirectoryName(bundlePath)!);
        if (!IsPathUnderRoot(bundleRoot, fullPath))
            return OperationResult<string>.Fail($"Proof artifact ref escapes proof bundle root: {artifactRef}", InvalidRef);

        if (!File.Exists(fullPath))
            return OperationResult<string>.Fail($"Proof artifact file not found: {fullPath}", MissingProofArtifact);
        return OperationResult<string>.Ok(fullPath);
    }

    private static bool IsPathUnderRoot(string rootPath, string candidatePath)
    {
        var relativePath = Path.GetRelativePath(rootPath, candidatePath);
        return !string.Equals(relativePath, ".", StringComparison.Ordinal) &&
            !string.Equals(relativePath, "..", StringComparison.Ordinal) &&
            !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
            !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal) &&
            !Path.IsPathRooted(relativePath);
    }

    private static CategorySpecificValidation ValidateCategorySpecificArtifact(JsonElement root, string categoryId)
    {
        var controlsRun = GetStringArray(root, "controls_run");
        var topKResults = GetStringArray(root, "top_k_results");
        var approvedRawRefs = GetStringArray(root, "approved_raw_private_refs");
        var risks = new List<string>();
        var heldOutSplitExecuted = GetBool(root, "held_out_split_executed");
        var sourceDisjointExecuted = GetBool(root, "source_disjoint_check_executed");
        var nearestNeighbourExecuted = GetBool(root, "nearest_neighbour_check_executed");
        var ablationRunnerExecuted = GetBool(root, "runner_executed");
        var replayProbeExecuted = GetBool(root, "replay_probe_executed");
        var controlActuallyRan = categoryId switch
        {
            "held_out_split" => heldOutSplitExecuted,
            "source_disjoint" => sourceDisjointExecuted,
            "nearest_neighbour_similarity_risk" => nearestNeighbourExecuted,
            "ablation" => ablationRunnerExecuted,
            "no_leakage" => replayProbeExecuted,
            "no_memorization" => replayProbeExecuted,
            _ => false
        };
        var runnerStatus = FirstString(root, "runner_status", "result_status", "execution_status");
        switch (categoryId)
        {
            case "held_out_split":
                RequireTrue(heldOutSplitExecuted, risks, "HELD_OUT_SPLIT_CONTROL_NOT_RUN");
                RequireNonDeferred(FirstString(root, "approved_assignment", "held_out_assignment"), risks, "HELD_OUT_ASSIGNMENT_NOT_APPROVED");
                RequireString(root, risks, "APPROVED_TRAIN_SOURCE_CORPUS_REF_MISSING", "train_source_corpus_manifest_ref", "approved_train_source_corpus_manifest_ref", "corpus_manifest_ref");
                RequireString(root, risks, "APPROVED_HELD_OUT_SPLIT_REF_MISSING", "held_out_split_manifest_ref", "approved_held_out_split_manifest_ref", "split_manifest_ref");
                break;
            case "source_disjoint":
                RequireTrue(sourceDisjointExecuted, risks, "SOURCE_DISJOINT_CONTROL_NOT_RUN");
                RequireTrue(GetBool(root, "source_disjoint_pass"), risks, "SOURCE_DISJOINT_NOT_PASS");
                RequireZero(root, risks, "SOURCE_OVERLAP_COUNT_NON_ZERO", "overlap_count", "source_overlap_count");
                break;
            case "nearest_neighbour_similarity_risk":
                RequireTrue(nearestNeighbourExecuted, risks, "NEAREST_NEIGHBOUR_CONTROL_NOT_RUN");
                RequireTrue(GetBool(root, "index_built") || GetBool(root, "index_contract_present"), risks, "NEAREST_NEIGHBOUR_INDEX_NOT_BUILT");
                RequireTrue(topKResults.Count > 0, risks, "NEAREST_NEIGHBOUR_TOP_K_MISSING");
                RequireMaxDouble(root, risks, "NEAREST_NEIGHBOUR_MAX_SIMILARITY_TOO_HIGH", 0.82d, "max_similarity", "nearest_neighbour_max_similarity");
                break;
            case "ablation":
                RequireTrue(ablationRunnerExecuted, risks, "ABLATION_RUNNER_NOT_RUN");
                RequireTrue(!IsNotRunStatus(runnerStatus), risks, "ABLATION_RUNNER_NOT_RUN");
                RequireTrue(!GetBool(root, "shortcut_only") && !GetBool(root, "shortcut_only_result"), risks, "ABLATION_SHORTCUT_ONLY");
                foreach (var requiredControl in RequiredAblationControls)
                    RequireTrue(controlsRun.Contains(requiredControl, StringComparer.Ordinal), risks, $"ABLATION_CONTROL_MISSING_{requiredControl.ToUpperInvariant()}");
                break;
            case "no_leakage":
                RequireTrue(replayProbeExecuted, risks, "NO_LEAKAGE_REPLAY_PROBE_NOT_RUN");
                RequireTrue(approvedRawRefs.Count > 0, risks, "NO_LEAKAGE_APPROVED_RAW_PRIVATE_REFS_MISSING");
                RequireTrue(!GetBool(root, "leakage_found"), risks, "NO_LEAKAGE_LEAKAGE_FOUND");
                RequireZero(root, risks, "NO_LEAKAGE_EXACT_MATCH_COUNT_NON_ZERO", "exact_match_count", "leakage_match_count");
                break;
            case "no_memorization":
                RequireTrue(replayProbeExecuted, risks, "NO_MEMORIZATION_REPLAY_PROBE_NOT_RUN");
                RequireTrue(approvedRawRefs.Count > 0, risks, "NO_MEMORIZATION_APPROVED_RAW_PRIVATE_REFS_MISSING");
                RequireTrue(!GetBool(root, "memorization_found"), risks, "NO_MEMORIZATION_FOUND");
                RequireMaxInt(root, risks, "NO_MEMORIZATION_LONG_COMMON_FRAGMENT_AT_OR_ABOVE_THRESHOLD", 23, "longest_common_fragment_token_count");
                break;
        }

        return new CategorySpecificValidation(
            ControlActuallyRan: controlActuallyRan,
            RunnerContractAvailable: GetBool(root, "runner_contract_available_for_current_rows") || categoryId == "ablation" && ablationRunnerExecuted,
            ShortcutOnly: GetBool(root, "shortcut_only") || GetBool(root, "shortcut_only_result"),
            RunnerStatus: string.IsNullOrWhiteSpace(runnerStatus) && categoryId == "ablation"
                ? ablationRunnerExecuted ? "EXECUTED_PASS" : NotRunMissingInputs
                : runnerStatus,
            ControlsRun: controlsRun,
            TopKResults: topKResults,
            ApprovedRawPrivateRefs: approvedRawRefs,
            LongestCommonFragmentTokenCount: GetNullableInt(root, "longest_common_fragment_token_count"),
            TrainSourceCorpusManifestRef: FirstString(root, "train_source_corpus_manifest_ref", "approved_train_source_corpus_manifest_ref", "corpus_manifest_ref"),
            HeldOutSplitManifestRef: FirstString(root, "held_out_split_manifest_ref", "approved_held_out_split_manifest_ref", "split_manifest_ref"),
            RawPrivateCorpusRef: FirstNonEmpty(approvedRawRefs.Concat(new[] { FirstString(root, "raw_private_corpus_ref", "approved_raw_private_corpus_ref", "private_corpus_pointer") })),
            ApprovedAssignment: FirstString(root, "approved_assignment", "held_out_assignment"),
            RiskCodes: risks.Distinct(StringComparer.Ordinal).ToArray());
    }

    private static ValidatedProofBundle MissingProofBundle(
        IReadOnlyList<ProducerInputRow> rows,
        string reason,
        IReadOnlyList<string> messages)
        => new(
            Loaded: false,
            RootPath: string.Empty,
            Messages: messages,
            ResultsByRow: NewMissingResultMap(rows, reason).ToDictionary(
                row => row.Key,
                row => (IReadOnlyDictionary<string, ValidatedProofResult>)row.Value,
                StringComparer.Ordinal));

    private static Dictionary<string, Dictionary<string, ValidatedProofResult>> NewMissingResultMap(
        IReadOnlyList<ProducerInputRow> rows,
        string reason)
        => rows.ToDictionary(
            row => row.RowId,
            _ => NonInterRaterCategories.ToDictionary(category => category, category => MissingProofResult(category, reason), StringComparer.Ordinal),
            StringComparer.Ordinal);

    private static ValidatedProofResult GetProofResult(
        ValidatedProofBundle proofBundle,
        string rowId,
        string categoryId)
        => proofBundle.ResultsByRow.TryGetValue(rowId, out var rowResults) && rowResults.TryGetValue(categoryId, out var result)
            ? result
            : MissingProofResult(categoryId, MissingProofArtifact);

    private static ValidatedProofResult MissingProofResult(string categoryId, string reason)
        => new(
            CategoryId: categoryId,
            Pass: false,
            ProofStatus: string.IsNullOrWhiteSpace(reason) ? MissingProofArtifact : reason,
            ArtifactRef: string.Empty,
            ControlActuallyRan: false,
            RunnerContractAvailable: false,
            ShortcutOnly: false,
            RunnerStatus: NotRunMissingInputs,
            ControlsRun: [],
            TopKResults: [],
            ApprovedRawPrivateRefs: [],
            LongestCommonFragmentTokenCount: null,
            TrainSourceCorpusManifestRef: string.Empty,
            HeldOutSplitManifestRef: string.Empty,
            RawPrivateCorpusRef: string.Empty,
            ApprovedAssignment: string.Empty,
            RiskCodes: [string.IsNullOrWhiteSpace(reason) ? MissingProofArtifact : reason]);

    private static string ProofArtifactRef(JsonElement proofRef)
        => proofRef.ValueKind == JsonValueKind.String
            ? proofRef.GetString() ?? string.Empty
            : FirstString(proofRef, "artifact_ref", "proof_artifact_ref", "path");

    private static string ProofArtifactSha(JsonElement proofRef)
        => proofRef.ValueKind == JsonValueKind.Object
            ? FirstString(proofRef, "artifact_sha256", "sha256")
            : string.Empty;

    private static bool ArtifactRowMatchesInput(JsonElement root, ProducerInputRow expectedRow)
        => RequiredStringMatches(root, "row_id", expectedRow.RowId) &&
           RequiredStringMatches(root, "scenario_id", expectedRow.ScenarioId) &&
           RequiredStringMatches(root, "prompt_sha256", expectedRow.PromptSha256) &&
           OptionalStringMatches(root, "pair_id", expectedRow.PairId);

    private static bool RequiredStringMatches(JsonElement root, string propertyName, string expected)
    {
        var actual = GetString(root, propertyName);
        return !string.IsNullOrWhiteSpace(actual) && string.Equals(actual, expected, StringComparison.Ordinal);
    }

    private static bool OptionalStringMatches(JsonElement root, string propertyName, string expected)
    {
        var actual = GetString(root, propertyName);
        return string.IsNullOrWhiteSpace(actual) || string.Equals(actual, expected, StringComparison.Ordinal);
    }

    private static bool IsPassOrApproved(string value)
        => !string.IsNullOrWhiteSpace(value) &&
           !value.Contains("RISK", StringComparison.OrdinalIgnoreCase) &&
           !value.Contains("DEFER", StringComparison.OrdinalIgnoreCase) &&
           !value.Contains("MISSING", StringComparison.OrdinalIgnoreCase) &&
           !value.Contains("INVALID", StringComparison.OrdinalIgnoreCase) &&
           (value.Contains("PASS", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("APPROVED", StringComparison.OrdinalIgnoreCase));

    private static string FirstRiskStatus(IReadOnlyList<string> risks)
        => risks.Count == 0 ? ProofDeferred : risks[0];

    private static void RequireTrue(bool condition, List<string> risks, string riskCode)
    {
        if (!condition)
            risks.Add(riskCode);
    }

    private static void RequireString(JsonElement root, List<string> risks, string riskCode, params string[] propertyNames)
    {
        if (string.IsNullOrWhiteSpace(FirstString(root, propertyNames)))
            risks.Add(riskCode);
    }

    private static void RequireNonDeferred(string value, List<string> risks, string riskCode)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Contains("NONE", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("DEFER", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("RISK", StringComparison.OrdinalIgnoreCase))
        {
            risks.Add(riskCode);
        }
    }

    private static void RequireZero(JsonElement root, List<string> risks, string riskCode, params string[] propertyNames)
    {
        var value = FirstNullableInt(root, propertyNames);
        if (!value.HasValue || value.Value != 0)
            risks.Add(riskCode);
    }

    private static void RequireMaxDouble(JsonElement root, List<string> risks, string riskCode, double maxInclusive, params string[] propertyNames)
    {
        var value = FirstNullableDouble(root, propertyNames);
        if (!value.HasValue || value.Value > maxInclusive)
            risks.Add(riskCode);
    }

    private static void RequireMaxInt(JsonElement root, List<string> risks, string riskCode, int maxInclusive, params string[] propertyNames)
    {
        var value = FirstNullableInt(root, propertyNames);
        if (!value.HasValue || value.Value > maxInclusive)
            risks.Add(riskCode);
    }

    private static bool HasLegacyApprovedRefs(Quality03NonInterRaterProofProducerOptions options)
        => !string.IsNullOrWhiteSpace(options.ApprovedTrainSourceCorpusManifestRef) ||
           !string.IsNullOrWhiteSpace(options.ApprovedHeldOutSplitManifestRef) ||
           !string.IsNullOrWhiteSpace(options.ApprovedRawPrivateCorpusRef);

    private static string Sha256File(string path)
    {
        using var stream = File.OpenRead(path);
        var bytes = SHA256.HashData(stream);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static IReadOnlyList<RowAnalysis> BuildRowAnalyses(
        ValidatedProofBundle proofBundle,
        IReadOnlyList<ProducerInputRow> rows)
        => rows
            .Select(row =>
            {
                var proofResults = NonInterRaterCategories
                    .ToDictionary(category => category, category => GetProofResult(proofBundle, row.RowId, category), StringComparer.Ordinal);
                var nearestProof = proofResults["nearest_neighbour_similarity_risk"];
                var noMemorizationProof = proofResults["no_memorization"];
                return new RowAnalysis(
                    Row: row,
                    HeldOutPass: proofResults["held_out_split"].Pass,
                    SourceDisjointPass: proofResults["source_disjoint"].Pass,
                    NearestPass: nearestProof.Pass,
                    AblationPass: proofResults["ablation"].Pass,
                    NoLeakagePass: proofResults["no_leakage"].Pass,
                    NoMemorizationPass: noMemorizationProof.Pass,
                    ProofResults: proofResults,
                    NearestTopKResults: nearestProof.TopKResults,
                    LongestCommonFragmentTokenCount: noMemorizationProof.LongestCommonFragmentTokenCount ?? 0,
                    RiskReasons: BuildRiskReasons(proofResults));
            })
            .ToArray();

    private static IReadOnlyDictionary<string, Quality03NonInterRaterProofProducerCategoryCounts> BuildCategoryCounts(
        IReadOnlyList<RowAnalysis> rows)
        => new Dictionary<string, Quality03NonInterRaterProofProducerCategoryCounts>(StringComparer.Ordinal)
        {
            ["held_out_split"] = Count(rows, row => row.HeldOutPass),
            ["source_disjoint"] = Count(rows, row => row.SourceDisjointPass),
            ["nearest_neighbour_similarity_risk"] = Count(rows, row => row.NearestPass),
            ["ablation"] = Count(rows, row => row.AblationPass),
            ["no_leakage"] = Count(rows, row => row.NoLeakagePass),
            ["no_memorization"] = Count(rows, row => row.NoMemorizationPass)
        };

    private static Quality03NonInterRaterProofProducerCategoryCounts Count(
        IReadOnlyList<RowAnalysis> rows,
        Func<RowAnalysis, bool> predicate)
    {
        var passed = rows.Count(predicate);
        return new Quality03NonInterRaterProofProducerCategoryCounts(rows.Count, passed, rows.Count - passed);
    }

    private static OperationResult<bool> WriteOutputFiles(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        IReadOnlyList<ProducerInputRow> rows,
        IReadOnlyList<RowAnalysis> rowAnalyses,
        List<string> outputRefs)
    {
        try
        {
            Directory.CreateDirectory(options.OutputDirectory);
            CopySourceFile(options, "per-row-blocking-proof-refs.jsonl", outputRefs);
            CopySourceFile(options, "row-specific-independent-second-rater-adjudication.jsonl", outputRefs);
            WriteJson(Path.Combine(options.OutputDirectory, "approved-held-out-source-corpus-manifest.json"), BuildApprovedCorpusManifest(options, runId, generatedAt, rowAnalyses), outputRefs);
            WriteJson(Path.Combine(options.OutputDirectory, "row-specific-nearest-neighbour-index-manifest.json"), BuildNearestNeighbourIndexManifest(options, runId, generatedAt, rowAnalyses), outputRefs);
            WriteJsonl(Path.Combine(options.OutputDirectory, "row-specific-source-disjoint-pass-or-risk.jsonl"), rowAnalyses.Select(row => BuildCategoryRow(options, runId, generatedAt, row, "source_disjoint", row.SourceDisjointPass)), outputRefs);
            WriteJsonl(Path.Combine(options.OutputDirectory, "row-specific-nearest-neighbour-pass-or-risk.jsonl"), rowAnalyses.Select(row => BuildNearestNeighbourRow(options, runId, generatedAt, row)), outputRefs);
            WriteJsonl(Path.Combine(options.OutputDirectory, "row-specific-ablation-runner-report.jsonl"), rowAnalyses.Select(row => BuildAblationRow(options, runId, generatedAt, row)), outputRefs);
            WriteJsonl(Path.Combine(options.OutputDirectory, "row-specific-no-leakage-pass-or-risk.jsonl"), rowAnalyses.Select(row => BuildReplayRow(options, runId, generatedAt, row, "no_leakage", row.NoLeakagePass)), outputRefs);
            WriteJsonl(Path.Combine(options.OutputDirectory, "row-specific-no-memorization-pass-or-risk.jsonl"), rowAnalyses.Select(row => BuildReplayRow(options, runId, generatedAt, row, "no_memorization", row.NoMemorizationPass)), outputRefs);
            WriteJsonl(Path.Combine(options.OutputDirectory, "per-row-non-inter-rater-proof-map.jsonl"), rowAnalyses.Select(row => BuildPerRowMap(options, runId, generatedAt, row)), outputRefs);
            return OperationResult<bool>.Ok(true);
        }
        catch (IOException ex)
        {
            return OperationResult<bool>.Fail($"Could not write non-inter-rater proof producer files: {ex.Message}", "PRODUCER_WRITE_FAILED");
        }
        catch (UnauthorizedAccessException ex)
        {
            return OperationResult<bool>.Fail($"Could not write non-inter-rater proof producer files: {ex.Message}", "PRODUCER_WRITE_FORBIDDEN");
        }
    }

    private static OperationResult<bool> WriteReportFiles(
        Quality03NonInterRaterProofProducerOptions options,
        Quality03NonInterRaterProofProducerReport report,
        IReadOnlyList<RowAnalysis> rows)
    {
        try
        {
            var manifestPath = Path.Combine(options.OutputDirectory, "non-inter-rater-proof-producer-manifest.json");
            var reportPath = Path.Combine(options.OutputDirectory, "non-inter-rater-proof-producer-report-ru.md");
            var gapPath = Path.Combine(options.OutputDirectory, "non-inter-rater-proof-producer-gap-note-ru.md");
            WriteUtf8LfText(manifestPath, JsonSerializer.Serialize(report, JsonOptions));
            WriteUtf8LfText(reportPath, BuildMarkdownRu(report));
            WriteUtf8LfText(gapPath, BuildGapNoteRu(report, rows));
            return OperationResult<bool>.Ok(true);
        }
        catch (IOException ex)
        {
            return OperationResult<bool>.Fail($"Could not write non-inter-rater proof producer reports: {ex.Message}", "PRODUCER_REPORT_WRITE_FAILED");
        }
        catch (UnauthorizedAccessException ex)
        {
            return OperationResult<bool>.Fail($"Could not write non-inter-rater proof producer reports: {ex.Message}", "PRODUCER_REPORT_WRITE_FORBIDDEN");
        }
    }

    private static JsonObject BuildApprovedCorpusManifest(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        IReadOnlyList<RowAnalysis> rows)
    {
        var approved = rows.Any(row => row.HeldOutPass);
        var trainSourceRef = FirstNonEmpty(rows.Select(row => row.ProofResults["held_out_split"].TrainSourceCorpusManifestRef));
        var heldOutSplitRef = FirstNonEmpty(rows.Select(row => row.ProofResults["held_out_split"].HeldOutSplitManifestRef));
        var rawPrivateRef = FirstNonEmpty(rows.SelectMany(row => new[]
        {
            row.ProofResults["no_leakage"].RawPrivateCorpusRef,
            row.ProofResults["no_memorization"].RawPrivateCorpusRef
        }));
        var rowArray = new JsonArray();
        foreach (var row in rows)
        {
            var heldOutProof = row.ProofResults["held_out_split"];
            rowArray.Add(new JsonObject
            {
                ["row_id"] = row.Row.RowId,
                ["pair_id"] = row.Row.PairId,
                ["host"] = row.Row.Host,
                ["method"] = row.Row.Method,
                ["prompt_sha256"] = row.Row.PromptSha256,
                ["approved_assignment"] = row.HeldOutPass ? FirstNonEmpty(new[] { heldOutProof.ApprovedAssignment, "held_out" }) : "NONE_DEFERRED",
                ["source_disjoint_pass"] = row.SourceDisjointPass
            });
        }

        return new JsonObject
        {
            ["schema"] = "xiigen.quality_03.approved_held_out_source_corpus_manifest.v2",
            ["work_order_id"] = options.WorkOrderId,
            ["run_id"] = runId,
            ["generated_at_utc"] = generatedAt,
            ["manifest_status"] = approved ? Pass : "DEFERRED_NOT_APPROVED_NOT_PASS",
            ["approved"] = approved,
            ["approved_train_source_corpus_manifest_ref"] = EmptyToNull(trainSourceRef),
            ["approved_held_out_split_manifest_ref"] = EmptyToNull(heldOutSplitRef),
            ["approved_raw_private_corpus_ref"] = EmptyToNull(rawPrivateRef),
            ["rows"] = rowArray,
            ["exact_defer_reason"] = approved ? null : "A valid row-matched held-out proof artifact is missing for every QUALITY-03 row."
        };
    }

    private static JsonObject BuildNearestNeighbourIndexManifest(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        IReadOnlyList<RowAnalysis> rows)
    {
        var pass = rows.Any(row => row.NearestPass);
        var corpusRef = FirstNonEmpty(rows.Select(row => row.ProofResults["held_out_split"].TrainSourceCorpusManifestRef));
        return new JsonObject
        {
            ["schema"] = "xiigen.quality_03.nearest_neighbour_index_manifest.v2",
            ["work_order_id"] = options.WorkOrderId,
            ["run_id"] = runId,
            ["generated_at_utc"] = generatedAt,
            ["manifest_status"] = pass ? Pass : "RISK_OR_DEFERRED_NOT_PASS",
            ["index_built"] = pass,
            ["pass"] = pass,
            ["corpus_manifest_ref"] = pass ? corpusRef : null,
            ["index_algorithm"] = "deterministic-local-token-jaccard-top-k",
            ["similarity_threshold"] = 0.82d,
            ["top_k"] = 5,
            ["deterministic_runner_label"] = DeterministicRunnerLabel
        };
    }

    private static JsonObject BuildCategoryRow(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        RowAnalysis row,
        string categoryId,
        bool pass)
    {
        var json = CommonCategoryRow(options, runId, generatedAt, row, categoryId, pass);
        json["risk_codes"] = JsonArrayFrom(pass ? [] : row.RiskReasons[categoryId]);
        return json;
    }

    private static JsonObject BuildNearestNeighbourRow(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        RowAnalysis row)
    {
        var json = BuildCategoryRow(options, runId, generatedAt, row, "nearest_neighbour_similarity_risk", row.NearestPass);
        var proof = row.ProofResults["nearest_neighbour_similarity_risk"];
        json["index_contract_present"] = proof.ControlActuallyRan && row.NearestPass;
        json["top_k_results"] = JsonArrayFrom(proof.ControlActuallyRan ? row.NearestTopKResults : Array.Empty<string>());
        json["similarity_threshold"] = 0.82d;
        return json;
    }

    private static JsonObject BuildAblationRow(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        RowAnalysis row)
    {
        var json = BuildCategoryRow(options, runId, generatedAt, row, "ablation", row.AblationPass);
        var proof = row.ProofResults["ablation"];
        json["runner_executed"] = proof.ControlActuallyRan;
        json["runner_contract_available_for_current_rows"] = proof.ControlActuallyRan && proof.RunnerContractAvailable;
        json["shortcut_only"] = proof.ShortcutOnly;
        json["runner_status"] = proof.ControlActuallyRan
            ? row.AblationPass ? "EXECUTED_PASS" : FirstNonEmpty(new[] { proof.RunnerStatus, "EXECUTED_RISK_OR_DEFERRED_NOT_PASS" })
            : NotRunMissingInputs;
        json["controls_required"] = JsonArrayFrom(RequiredAblationControls);
        json["controls_run"] = JsonArrayFrom(proof.ControlActuallyRan ? proof.ControlsRun : Array.Empty<string>());
        return json;
    }

    private static JsonObject BuildReplayRow(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        RowAnalysis row,
        string categoryId,
        bool pass)
    {
        var json = BuildCategoryRow(options, runId, generatedAt, row, categoryId, pass);
        var proof = row.ProofResults[categoryId];
        json["replay_probe_executed"] = proof.ControlActuallyRan;
        json["pass_boundary_flags_only"] = false;
        json["exact_match_scan_scope"] = "local prompt/output text fields from proof input rows";
        json["longest_common_fragment_token_count"] = row.LongestCommonFragmentTokenCount;
        json["approved_raw_private_refs"] = JsonArrayFrom(proof.ControlActuallyRan ? proof.ApprovedRawPrivateRefs : Array.Empty<string>());
        return json;
    }

    private static JsonObject CommonCategoryRow(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        RowAnalysis analysis,
        string categoryId,
        bool pass)
    {
        var row = analysis.Row;
        var proof = analysis.ProofResults[categoryId];
        var relativeFile = CategoryFileName(categoryId);
        var artifactRef = NormalizePath(Path.Combine(options.OutputDirectory, relativeFile)) + "#" + row.RowId;
        return new JsonObject
        {
            ["schema"] = "xiigen.quality_03.non_inter_rater_proof_row.v1",
            ["work_order_id"] = options.WorkOrderId,
            ["run_id"] = runId,
            ["generated_at_utc"] = generatedAt,
            ["row_id"] = row.RowId,
            ["pair_sequence_id"] = row.PairSequenceId,
            ["pair_id"] = row.PairId,
            ["scenario_id"] = row.ScenarioId,
            ["source_scenario_id"] = row.SourceScenarioId,
            ["host"] = row.Host,
            ["method"] = row.Method,
            ["evidence_class"] = row.EvidenceClass,
            ["prompt_sha256"] = row.PromptSha256,
            ["category_id"] = categoryId,
            ["artifact_ref"] = artifactRef,
            ["source_proof_artifact_ref"] = EmptyToNull(proof.ArtifactRef),
            ["proof_validation_status"] = pass ? "VALIDATED_PASS" : proof.ProofStatus,
            ["control_actually_ran"] = proof.ControlActuallyRan,
            ["pass"] = pass,
            ["pass_or_risk"] = pass ? Pass : Risk,
            ["proof_status"] = pass ? ProofPass : proof.ProofStatus,
            ["risk_status"] = pass ? Pass : proof.ProofStatus,
            ["blocking_ref_resolved"] = pass,
            ["deferred"] = !pass,
            ["concrete_proof_ref"] = pass ? proof.ArtifactRef : null,
            ["external_ai_model_called"] = false,
            ["training_dpo_checkpoint_model_locator_mutation"] = false
        };
    }

    private static JsonObject BuildPerRowMap(
        Quality03NonInterRaterProofProducerOptions options,
        string runId,
        string generatedAt,
        RowAnalysis row)
        => new()
        {
            ["schema"] = PerRowMapSchema,
            ["work_order_id"] = options.WorkOrderId,
            ["run_id"] = runId,
            ["generated_at_utc"] = generatedAt,
            ["row_id"] = row.Row.RowId,
            ["pair_id"] = row.Row.PairId,
            ["scenario_id"] = row.Row.ScenarioId,
            ["host"] = row.Row.Host,
            ["method"] = row.Row.Method,
            ["category_results"] = new JsonObject
            {
                ["held_out_split"] = CategoryMapEntry(options, row, "held_out_split", row.HeldOutPass),
                ["source_disjoint"] = CategoryMapEntry(options, row, "source_disjoint", row.SourceDisjointPass),
                ["nearest_neighbour_similarity_risk"] = CategoryMapEntry(options, row, "nearest_neighbour_similarity_risk", row.NearestPass),
                ["ablation"] = CategoryMapEntry(options, row, "ablation", row.AblationPass),
                ["no_leakage"] = CategoryMapEntry(options, row, "no_leakage", row.NoLeakagePass),
                ["no_memorization"] = CategoryMapEntry(options, row, "no_memorization", row.NoMemorizationPass)
            },
            ["runner_ready_candidate"] = false,
            ["quality_pass_row"] = false
        };

    private static JsonObject CategoryMapEntry(
        Quality03NonInterRaterProofProducerOptions options,
        RowAnalysis row,
        string categoryId,
        bool pass)
    {
        var proof = row.ProofResults[categoryId];
        return new JsonObject
        {
            ["proof_status"] = pass ? ProofPass : proof.ProofStatus,
            ["proof_validation_status"] = pass ? "VALIDATED_PASS" : proof.ProofStatus,
            ["blocking_ref_resolved"] = pass,
            ["evidence_ref"] = NormalizePath(Path.Combine(options.OutputDirectory, CategoryFileName(categoryId))) + "#" + row.Row.RowId,
            ["source_proof_artifact_ref"] = EmptyToNull(proof.ArtifactRef),
            ["control_actually_ran"] = proof.ControlActuallyRan,
            ["risk_codes"] = JsonArrayFrom(pass ? [] : row.RiskReasons[categoryId])
        };
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildRiskReasons(
        IReadOnlyDictionary<string, ValidatedProofResult> proofResults)
        => NonInterRaterCategories.ToDictionary(
            category => category,
            category => (IReadOnlyList<string>)(proofResults[category].Pass ? Array.Empty<string>() : proofResults[category].RiskCodes),
            StringComparer.Ordinal);

    private static string CategoryFileName(string categoryId)
        => categoryId switch
        {
            "held_out_split" => "approved-held-out-source-corpus-manifest.json",
            "source_disjoint" => "row-specific-source-disjoint-pass-or-risk.jsonl",
            "nearest_neighbour_similarity_risk" => "row-specific-nearest-neighbour-pass-or-risk.jsonl",
            "ablation" => "row-specific-ablation-runner-report.jsonl",
            "no_leakage" => "row-specific-no-leakage-pass-or-risk.jsonl",
            "no_memorization" => "row-specific-no-memorization-pass-or-risk.jsonl",
            _ => $"{categoryId}.jsonl"
        };

    private static void CopySourceFile(Quality03NonInterRaterProofProducerOptions options, string fileName, List<string> outputRefs)
    {
        var sourcePath = Path.Combine(options.SourceProofContractPackDirectory, fileName);
        var outputPath = Path.Combine(options.OutputDirectory, fileName);
        if (File.Exists(sourcePath))
        {
            File.Copy(sourcePath, outputPath, overwrite: true);
            outputRefs.Add(NormalizePath(outputPath));
        }
    }

    private static void WriteJson(string path, JsonObject root, List<string> outputRefs)
    {
        WriteUtf8LfText(path, root.ToJsonString(JsonOptions));
        outputRefs.Add(NormalizePath(path));
    }

    private static void WriteJsonl(string path, IEnumerable<JsonObject> rows, List<string> outputRefs)
    {
        WriteUtf8LfText(path, BuildJsonLines(rows.Select(row => row.ToJsonString(JsonLineOptions))));
        outputRefs.Add(NormalizePath(path));
    }

    private static string BuildJsonLines(IEnumerable<string> rows)
    {
        var materializedRows = rows.ToArray();
        return materializedRows.Length == 0 ? string.Empty : string.Join(Lf, materializedRows) + Lf;
    }

    private static string BuildMarkdownRu(Quality03NonInterRaterProofProducerReport report)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-03 non-inter-rater proof producer");
        builder.AppendLine();
        builder.AppendLine("Это локальный deterministic producer proof-data. Он не является semantic scorer, learning proof или product readiness gate.");
        builder.AppendLine();
        builder.AppendLine($"- total proof refs: {report.TotalProofRefs}");
        builder.AppendLine($"- PASS до работы: {report.PassBeforeWork}");
        builder.AppendLine($"- non-inter-rater PASS после producer: {report.NonInterRaterProofRefsPassProduced}/{report.NonInterRaterProofRefsTotal}");
        builder.AppendLine($"- ожидаемый proof PASS после producer плюс carried inter-rater: {report.BlockingProofRefsPassAfterProducer}/{report.TotalProofRefs}");
        builder.AppendLine($"- runner-ready rows: {report.RunnerReadyRows}");
        builder.AppendLine($"- quality-pass rows: {report.QualityPassRows}");
        builder.AppendLine();
        builder.AppendLine("## По категориям");
        foreach (var category in report.CountsByCategory)
            builder.AppendLine($"- {category.Key}: PASS {category.Value.PassProduced}/{category.Value.TotalRows}; RISK/DEFER {category.Value.RiskOrDeferred}/{category.Value.TotalRows}");
        builder.AppendLine();
        builder.AppendLine("## Что это доказывает");
        builder.AppendLine(report.WhatThisProves);
        builder.AppendLine();
        builder.AppendLine("## Чего это не доказывает");
        builder.AppendLine(report.WhatThisDoesNotProve);
        builder.AppendLine();
        builder.AppendLine("## CLI examples");
        builder.AppendLine("```powershell");
        builder.AppendLine("dotnet run --project 43.SmartSkillsDomain.Tools/Quality03NonInterRaterProofProducers/Quality03NonInterRaterProofProducers.csproj -- --evidence-pack <evidence-pack> --source-proof-contract-pack <proof-pack> --output-dir <new-quality-scoring-dir>");
        builder.AppendLine("dotnet run --project 43.SmartSkillsDomain.Tools/Quality03ProofContractRunner/Quality03ProofContractRunner.csproj -- --evidence-pack <evidence-pack> --proof-contract-pack <new-quality-scoring-dir> --second-rater-packet <authorized-second-rater-packet-dir-or-file> --output-dir <runner-output-dir>");
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine($"Следующий bounded work order: {report.NextBoundedWorkOrder}");
        return builder.ToString();
    }

    private static string BuildGapNoteRu(
        Quality03NonInterRaterProofProducerReport report,
        IReadOnlyList<RowAnalysis> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# QUALITY-03 gap note");
        builder.AppendLine();
        builder.AppendLine("PASS не был сфабрикован. Каждая строка с недостаточными corpus/prompt/output/private refs оставлена RISK/DEFER.");
        builder.AppendLine();
        foreach (var row in rows)
        {
            builder.AppendLine($"## {row.Row.RowId}");
            foreach (var category in NonInterRaterCategories)
            {
                var pass = category switch
                {
                    "held_out_split" => row.HeldOutPass,
                    "source_disjoint" => row.SourceDisjointPass,
                    "nearest_neighbour_similarity_risk" => row.NearestPass,
                    "ablation" => row.AblationPass,
                    "no_leakage" => row.NoLeakagePass,
                    "no_memorization" => row.NoMemorizationPass,
                    _ => false
                };
                if (!pass)
                    builder.AppendLine($"- {category}: {string.Join(", ", row.RiskReasons[category])}");
            }
        }

        builder.AppendLine();
        builder.AppendLine($"Следующий bounded work order: {report.NextBoundedWorkOrder}");
        return builder.ToString();
    }

    private static void WriteUtf8LfText(string path, string content)
        => File.WriteAllText(path, NormalizeTextArtifact(content), Utf8NoBom);

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

    private static JsonArray JsonArrayFrom(IEnumerable<string> values)
    {
        var array = new JsonArray();
        foreach (var value in values)
            array.Add(value);
        return array;
    }

    private static string GetString(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return string.Empty;
        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString() ?? string.Empty,
            JsonValueKind.Number => value.ToString(),
            JsonValueKind.True => "true",
            JsonValueKind.False => "false",
            _ => string.Empty
        };
    }

    private static bool GetBool(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return false;
        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => bool.TryParse(value.GetString(), out var parsed) && parsed,
            _ => false
        };
    }

    private static IReadOnlyList<string> GetStringArray(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Array)
            return [];
        return value.EnumerateArray()
            .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : item.ToString())
            .Where(item => !string.IsNullOrWhiteSpace(item))
            .ToArray();
    }

    private static int? GetNullableInt(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var number))
            return number;
        if (value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return null;
    }

    private static double? GetNullableDouble(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value))
            return null;
        if (value.ValueKind == JsonValueKind.Number && value.TryGetDouble(out var number))
            return number;
        if (value.ValueKind == JsonValueKind.String && double.TryParse(value.GetString(), NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
            return parsed;
        return null;
    }

    private static int? FirstNullableInt(JsonElement root, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var value = GetNullableInt(root, propertyName);
            if (value.HasValue)
                return value;
        }

        return null;
    }

    private static double? FirstNullableDouble(JsonElement root, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var value = GetNullableDouble(root, propertyName);
            if (value.HasValue)
                return value;
        }

        return null;
    }

    private static string FirstString(JsonElement root, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var value = GetString(root, propertyName);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return string.Empty;
    }

    private static string GetString(JsonObject root, string propertyName)
        => root.TryGetPropertyValue(propertyName, out var value) && value is not null
            ? value.GetValueKind() switch
            {
                JsonValueKind.String => value.GetValue<string>(),
                JsonValueKind.Number => value.ToJsonString(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                _ => string.Empty
            }
            : string.Empty;

    private static string FirstString(JsonObject root, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            var value = GetString(root, propertyName);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return string.Empty;
    }

    private static JsonNode? EmptyToNull(string value)
        => string.IsNullOrWhiteSpace(value) ? null : JsonValue.Create(value);

    private static string FirstNonEmpty(IEnumerable<string> values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    private static string NormalizePath(string path)
        => Path.GetFullPath(path).Replace('\\', '/');

    private static string NormalizeOptionalPath(string path)
        => string.IsNullOrWhiteSpace(path) || (Uri.TryCreate(path, UriKind.Absolute, out var uri) && !uri.IsFile)
            ? path
            : NormalizePath(path);

    private static bool IsNotRunStatus(string value)
        => value.Contains("NOT_RUN", StringComparison.OrdinalIgnoreCase) ||
           value.Contains("NOT_EXECUTED", StringComparison.OrdinalIgnoreCase);

    public static string Sha256(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private sealed record ProducerInputRow(
        string RowId,
        string PairSequenceId,
        string PairId,
        string ScenarioId,
        string SourceScenarioId,
        string Host,
        string Method,
        string EvidenceClass,
        string PromptSha256,
        string PromptText,
        string BaselineOutputText,
        string AdaptiveOutputText);

    private sealed record RowAnalysis(
        ProducerInputRow Row,
        bool HeldOutPass,
        bool SourceDisjointPass,
        bool NearestPass,
        bool AblationPass,
        bool NoLeakagePass,
        bool NoMemorizationPass,
        IReadOnlyDictionary<string, ValidatedProofResult> ProofResults,
        IReadOnlyList<string> NearestTopKResults,
        int LongestCommonFragmentTokenCount,
        IReadOnlyDictionary<string, IReadOnlyList<string>> RiskReasons);

    private sealed record ValidatedProofBundle(
        bool Loaded,
        string RootPath,
        IReadOnlyList<string> Messages,
        IReadOnlyDictionary<string, IReadOnlyDictionary<string, ValidatedProofResult>> ResultsByRow);

    private sealed record ValidatedProofResult(
        string CategoryId,
        bool Pass,
        string ProofStatus,
        string ArtifactRef,
        bool ControlActuallyRan,
        bool RunnerContractAvailable,
        bool ShortcutOnly,
        string RunnerStatus,
        IReadOnlyList<string> ControlsRun,
        IReadOnlyList<string> TopKResults,
        IReadOnlyList<string> ApprovedRawPrivateRefs,
        int? LongestCommonFragmentTokenCount,
        string TrainSourceCorpusManifestRef,
        string HeldOutSplitManifestRef,
        string RawPrivateCorpusRef,
        string ApprovedAssignment,
        IReadOnlyList<string> RiskCodes);

    private sealed record CategorySpecificValidation(
        bool ControlActuallyRan,
        bool RunnerContractAvailable,
        bool ShortcutOnly,
        string RunnerStatus,
        IReadOnlyList<string> ControlsRun,
        IReadOnlyList<string> TopKResults,
        IReadOnlyList<string> ApprovedRawPrivateRefs,
        int? LongestCommonFragmentTokenCount,
        string TrainSourceCorpusManifestRef,
        string HeldOutSplitManifestRef,
        string RawPrivateCorpusRef,
        string ApprovedAssignment,
        IReadOnlyList<string> RiskCodes);
}
