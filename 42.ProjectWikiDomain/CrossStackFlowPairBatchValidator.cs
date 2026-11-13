// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class CrossStackFlowPairBatchValidator
{
    private static readonly string[] TrainingDomains =
    [
        "campaign",
        "client",
        "diet",
        "media",
        "task",
        "game",
        "profile",
        "report",
        "notification",
        "booking"
    ];

    private static readonly string[] HeldOutDomains =
    [
        "invoice",
        "refund",
        "shipment",
        "audit",
        "tenant",
        "license",
        "subscription",
        "cart",
        "checkout",
        "message"
    ];

    private static readonly string[] RejectedTypes =
    [
        "ui_api_only",
        "server_sql_only",
        "procedure_without_result_shape",
        "separate_repositories",
        "incomplete_cross_stack_business_flow"
    ];

    public OperationResult<CrossStackFlowPairBatchValidationResult> Validate(
        CrossStackBusinessFlowGraphDpoBatch? batch,
        bool heldOut)
    {
        var result = Evaluate(batch, heldOut);
        if (result.Accepted)
            return OperationResult<CrossStackFlowPairBatchValidationResult>.Ok(result);

        return OperationResult<CrossStackFlowPairBatchValidationResult>.Fail(
            $"Cross-stack graph DPO batch validation failed: {result.ErrorCode}.",
            result.ErrorCode);
    }

    public CrossStackFlowPairBatchValidationResult Evaluate(
        CrossStackBusinessFlowGraphDpoBatch? batch,
        bool heldOut)
    {
        var expectedRole = heldOut ? DpoBatchSplitRole.HeldOut : DpoBatchSplitRole.Training;
        var expectedCells = BuildExpectedCells(heldOut);
        if (batch is null)
        {
            return BuildResult(
                DpoBatchSourceKind.Unknown,
                DpoBatchSplitRole.Unknown,
                "",
                expectedCells,
                [],
                false,
                expectedCells,
                [],
                [],
                0,
                "CROSS_STACK_BATCH_REQUIRED",
                "",
                "");
        }

        var expectedSet = expectedCells.ToHashSet(StringComparer.Ordinal);
        var validTypes = RejectedTypes.ToHashSet(StringComparer.Ordinal);
        var actualRows = batch.Pairs ?? [];
        var actualCells = actualRows
            .Select(ToMatrixCell)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var knownActualCells = actualRows
            .Where(row => validTypes.Contains(row.RejectedType))
            .Select(ToMatrixCell)
            .ToArray();
        var missingCells = expectedCells
            .Where(cell => !knownActualCells.Contains(cell, StringComparer.Ordinal))
            .ToArray();
        var duplicateCells = knownActualCells
            .GroupBy(cell => cell, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var unknownRejectedTypes = actualRows
            .Select(row => row.RejectedType)
            .Where(type => !validTypes.Contains(type))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var wrongAnnotationRate = actualRows.Length == 0
            ? 0
            : actualRows.Count(row => row.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) /
              (double)actualRows.Length;
        var matrixComplete = actualRows.Length == expectedCells.Length &&
                             missingCells.Length == 0 &&
                             duplicateCells.Length == 0 &&
                             unknownRejectedTypes.Length == 0 &&
                             knownActualCells.All(expectedSet.Contains);
        var rowFingerprint = CrossStackBusinessFlowGraphBatchIntegrity.ComputeRowsSha256(batch);
        var provenance = batch.SourceProvenanceProof;
        var sourceProvenanceFingerprint = provenance?.ProvenanceFingerprintSha256 ?? "";
        var provenanceMatches = provenance?.Matches(batch, expectedRole) == true;
        var errorCode = ResolveErrorCode(
            batch.SourceKind,
            batch.SplitRole,
            expectedRole,
            provenance,
            provenanceMatches,
            matrixComplete,
            duplicateCells,
            unknownRejectedTypes,
            wrongAnnotationRate);

        // Acceptance появляется только здесь: validator заново читает raw rows,
        // provenance и split-role. Вложенный batch.IndependentValidation —
        // telemetry прошлого запуска, а не доверенная подпись.
        return BuildResult(
            batch.SourceKind,
            batch.SplitRole,
            batch.BatchId,
            expectedCells,
            actualCells,
            matrixComplete,
            missingCells,
            duplicateCells,
            unknownRejectedTypes,
            wrongAnnotationRate,
            errorCode,
            rowFingerprint,
            sourceProvenanceFingerprint);
    }

    internal OperationResult<CrossStackBusinessFlowGraphPublicationAuthority> CreatePublicationAuthority(
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics)
    {
        var validation = ValidatePublicationInputsBeforeAuthority(
            trainingBatch,
            heldOutBatch,
            trainingResult,
            dpoMetrics,
            requireSemanticSource: true);
        if (!validation.IsSuccess)
            return validation.MapError<CrossStackBusinessFlowGraphPublicationAuthority>();

        var candidate = validation.Value!;
        return OperationResult<CrossStackBusinessFlowGraphPublicationAuthority>.Ok(
            CrossStackBusinessFlowGraphPublicationAuthority.Create(
                candidate.Training.SourceKind,
                trainingBatch,
                heldOutBatch,
                candidate.Training,
                candidate.HeldOut,
                trainingResult,
                dpoMetrics));
    }

    private OperationResult<CrossStackBusinessFlowGraphPublicationValidation>
        ValidatePublicationInputsBeforeAuthority(
            CrossStackBusinessFlowGraphDpoBatch? trainingBatch,
            CrossStackBusinessFlowGraphDpoBatch? heldOutBatch,
            CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
            ProjectWikiDpoMetricRunResult dpoMetrics,
            bool requireSemanticSource)
        => ValidatePublicationCandidateCore(
            trainingBatch,
            heldOutBatch,
            trainingResult,
            dpoMetrics,
            authority: null,
            requireSemanticSource);

    internal OperationResult<CrossStackBusinessFlowGraphPublicationValidation> ValidatePublicationCandidate(
        CrossStackBusinessFlowGraphDpoBatch? trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch? heldOutBatch,
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics,
        CrossStackBusinessFlowGraphPublicationAuthority? authority,
        bool requireSemanticSource)
    {
        if (authority is null)
        {
            // Publication path всегда fail-closed: только validator-owned
            // authority может открыть trusted side effects.
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "Opaque CrossStack publication authority is required.",
                "CROSS_STACK_GRAPH_PUBLICATION_AUTHORITY_REQUIRED");
        }

        return ValidatePublicationCandidateCore(
            trainingBatch,
            heldOutBatch,
            trainingResult,
            dpoMetrics,
            authority,
            requireSemanticSource);
    }

    private OperationResult<CrossStackBusinessFlowGraphPublicationValidation> ValidatePublicationCandidateCore(
        CrossStackBusinessFlowGraphDpoBatch? trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch? heldOutBatch,
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics,
        CrossStackBusinessFlowGraphPublicationAuthority? authority,
        bool requireSemanticSource)
    {
        if (trainingBatch is null || heldOutBatch is null)
        {
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "Exact train and held-out batches are required before CrossStack publication.",
                "CROSS_STACK_GRAPH_PUBLICATION_BATCHES_REQUIRED");
        }

        var trainingValidation = Validate(trainingBatch, heldOut: false);
        if (!trainingValidation.IsSuccess)
            return trainingValidation.MapError<CrossStackBusinessFlowGraphPublicationValidation>();

        var heldOutValidation = Validate(heldOutBatch, heldOut: true);
        if (!heldOutValidation.IsSuccess)
            return heldOutValidation.MapError<CrossStackBusinessFlowGraphPublicationValidation>();

        if (trainingBatch.SplitRole != DpoBatchSplitRole.Training ||
            heldOutBatch.SplitRole != DpoBatchSplitRole.HeldOut)
        {
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "CrossStack publication requires distinct train and held-out split roles.",
                "CROSS_STACK_GRAPH_PUBLICATION_SPLIT_ROLE_MISMATCH");
        }

        if (string.Equals(trainingBatch.BatchId, heldOutBatch.BatchId, StringComparison.Ordinal) ||
            string.Equals(trainingValidation.Value!.RowFingerprintSha256, heldOutValidation.Value!.RowFingerprintSha256, StringComparison.Ordinal))
        {
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "CrossStack publication requires distinct train and held-out batch identities.",
                "CROSS_STACK_GRAPH_PUBLICATION_SPLITS_NOT_DISTINCT");
        }

        if (trainingValidation.Value!.SourceKind != heldOutValidation.Value!.SourceKind)
        {
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "CrossStack publication source provenance differs between train and held-out.",
                "CROSS_STACK_GRAPH_PUBLICATION_SOURCE_MISMATCH");
        }

        if (!TrainingAndMetricIdentityMatches(
                trainingBatch,
                heldOutBatch,
                trainingValidation.Value!,
                heldOutValidation.Value!,
                trainingResult,
                dpoMetrics))
        {
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "CrossStack publication training result or DPO metric identity does not match the exact train/held-out batches.",
                "CROSS_STACK_GRAPH_PUBLICATION_TRAINING_IDENTITY_MISMATCH");
        }

        if (authority is not null &&
            !authority.Matches(
                trainingBatch,
                heldOutBatch,
                trainingValidation.Value!,
                heldOutValidation.Value!,
                trainingResult,
                dpoMetrics))
        {
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "CrossStack publication authority does not match the exact batches, validations, training result, and DPO metric result.",
                "CROSS_STACK_GRAPH_PUBLICATION_AUTHORITY_REPLAY_OR_MISMATCH");
        }

        if (requireSemanticSource &&
            trainingValidation.Value!.SourceKind is DpoBatchSourceKind.Unknown or DpoBatchSourceKind.DeterministicSeed)
        {
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "Deterministic seed or unknown batches are baseline-only and cannot publish Trusted semantic capability manifests.",
                "CROSS_STACK_GRAPH_PUBLICATION_SOURCE_NOT_AUTHORIZED");
        }

        if (requireSemanticSource &&
            trainingValidation.Value!.SourceKind is not DpoBatchSourceKind.RealRepositoryCorpus and not DpoBatchSourceKind.UserFeedback)
        {
            return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Fail(
                "CrossStack publication source kind is not authorized.",
                "CROSS_STACK_GRAPH_PUBLICATION_SOURCE_NOT_AUTHORIZED");
        }

        return OperationResult<CrossStackBusinessFlowGraphPublicationValidation>.Ok(
            new CrossStackBusinessFlowGraphPublicationValidation(
                trainingValidation.Value!,
                heldOutValidation.Value!));
    }

    private static CrossStackFlowPairBatchValidationResult BuildResult(
        DpoBatchSourceKind sourceKind,
        DpoBatchSplitRole splitRole,
        string batchId,
        string[] expectedCells,
        string[] actualCells,
        bool matrixComplete,
        string[] missingCells,
        string[] duplicateCells,
        string[] unknownRejectedTypes,
        double wrongAnnotationRate,
        string errorCode,
        string rowFingerprint,
        string sourceProvenanceFingerprint)
    {
        var accepted = string.IsNullOrEmpty(errorCode);
        var result = new CrossStackFlowPairBatchValidationResult(
            sourceKind,
            expectedCells.Length,
            actualCells.Length,
            expectedCells,
            actualCells,
            matrixComplete,
            missingCells,
            duplicateCells,
            unknownRejectedTypes,
            wrongAnnotationRate,
            accepted,
            errorCode)
        {
            BatchId = batchId,
            SplitRole = splitRole,
            RowFingerprintSha256 = rowFingerprint,
            SourceProvenanceFingerprintSha256 = sourceProvenanceFingerprint,
            ValidationCodeVersion = CrossStackBusinessFlowGraphBatchIntegrity.ValidationCodeVersion,
            ExpectedMatrixCellsSha256 = CrossStackBusinessFlowGraphBatchIntegrity.ComputeMatrixCellsSha256(expectedCells),
            ActualMatrixCellsSha256 = CrossStackBusinessFlowGraphBatchIntegrity.ComputeMatrixCellsSha256(actualCells)
        };

        return result with
        {
            ValidationFingerprintSha256 = CrossStackBusinessFlowGraphBatchIntegrity.ComputeValidationSha256(result)
        };
    }

    private static bool TrainingAndMetricIdentityMatches(
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainingValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation,
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics)
    {
        var heldOutCaseIds = trainingResult.Cases
            .Select(item => item.PairId)
            .ToArray();
        var heldOutPairIds = heldOutBatch.Pairs
            .Select(item => item.PairId)
            .ToArray();

        // Publication не принимает "похожий" результат по counts: обе ветки
        // обучения должны нести exact fingerprints тех же rows и validator-ов.
        return trainingResult.TrainingRowCount == trainingBatch.RowCount &&
               trainingResult.HeldOutRowCount == heldOutBatch.RowCount &&
               trainingResult.UnitTrace.RowsTrained == trainingBatch.RowCount &&
               trainingResult.UnitTrace.HeldOutRows == heldOutBatch.RowCount &&
               heldOutCaseIds.SequenceEqual(heldOutPairIds, StringComparer.Ordinal) &&
               string.Equals(trainingResult.TrainingBatchId, trainingBatch.BatchId, StringComparison.Ordinal) &&
               string.Equals(trainingResult.HeldOutBatchId, heldOutBatch.BatchId, StringComparison.Ordinal) &&
               string.Equals(trainingResult.TrainingRowsSha256, trainingValidation.RowFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(trainingResult.HeldOutRowsSha256, heldOutValidation.RowFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(trainingResult.TrainingValidationFingerprintSha256, trainingValidation.ValidationFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(trainingResult.HeldOutValidationFingerprintSha256, heldOutValidation.ValidationFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(trainingResult.TrainingExpectedMatrixCellsSha256, trainingValidation.ExpectedMatrixCellsSha256, StringComparison.Ordinal) &&
               string.Equals(trainingResult.TrainingActualMatrixCellsSha256, trainingValidation.ActualMatrixCellsSha256, StringComparison.Ordinal) &&
               string.Equals(trainingResult.HeldOutExpectedMatrixCellsSha256, heldOutValidation.ExpectedMatrixCellsSha256, StringComparison.Ordinal) &&
               string.Equals(trainingResult.HeldOutActualMatrixCellsSha256, heldOutValidation.ActualMatrixCellsSha256, StringComparison.Ordinal) &&
               dpoMetrics.TrainingRowCount == trainingBatch.RowCount &&
               dpoMetrics.HeldOutRowCount == heldOutBatch.RowCount &&
               string.Equals(dpoMetrics.BatchId, trainingBatch.BatchId, StringComparison.Ordinal) &&
               string.Equals(dpoMetrics.UnitId, CrossStackBusinessFlowGraphUnit.UnitId, StringComparison.Ordinal) &&
               string.Equals(dpoMetrics.CrossStackTrainingBatchId, trainingBatch.BatchId, StringComparison.Ordinal) &&
               string.Equals(dpoMetrics.CrossStackHeldOutBatchId, heldOutBatch.BatchId, StringComparison.Ordinal) &&
               string.Equals(dpoMetrics.CrossStackTrainingRowsSha256, trainingValidation.RowFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(dpoMetrics.CrossStackHeldOutRowsSha256, heldOutValidation.RowFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(dpoMetrics.CrossStackTrainingValidationFingerprintSha256, trainingValidation.ValidationFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(dpoMetrics.CrossStackHeldOutValidationFingerprintSha256, heldOutValidation.ValidationFingerprintSha256, StringComparison.Ordinal);
    }

    private static string[] BuildExpectedCells(bool heldOut)
    {
        var domains = heldOut ? HeldOutDomains : TrainingDomains;
        return domains
            .SelectMany(domain => RejectedTypes.Select(type => MatrixCell(domain, type)))
            .Order(StringComparer.Ordinal)
            .ToArray();
    }

    private static string ToMatrixCell(CrossStackBusinessFlowGraphDpoPair row)
        => MatrixCell(ResolveDomain(row), row.RejectedType);

    private static string ResolveDomain(CrossStackBusinessFlowGraphDpoPair row)
    {
        foreach (var rejectedType in RejectedTypes)
        {
            var typeSlug = Slug(rejectedType);
            const string prefix = "system-";
            var systemId = row.BoundaryMap.SystemId;
            if (systemId.StartsWith(prefix, StringComparison.Ordinal) &&
                systemId.EndsWith($"-{typeSlug}", StringComparison.Ordinal))
                return systemId[prefix.Length..^($"-{typeSlug}".Length)];
        }

        var firstProject = row.BoundaryMap.ProjectIds.FirstOrDefault() ?? "";
        if (!firstProject.StartsWith("angular-", StringComparison.Ordinal))
            return "";

        foreach (var rejectedType in RejectedTypes)
        {
            var suffix = $"-{Slug(rejectedType)}";
            if (firstProject.EndsWith(suffix, StringComparison.Ordinal))
                return firstProject["angular-".Length..^suffix.Length];
        }

        return "";
    }

    private static string ResolveErrorCode(
        DpoBatchSourceKind sourceKind,
        DpoBatchSplitRole splitRole,
        DpoBatchSplitRole expectedRole,
        CrossStackBusinessFlowGraphBatchProvenanceProof? provenance,
        bool provenanceMatches,
        bool matrixComplete,
        string[] duplicateCells,
        string[] unknownRejectedTypes,
        double wrongAnnotationRate)
    {
        if (sourceKind == DpoBatchSourceKind.Unknown)
            return "CROSS_STACK_BATCH_SOURCE_UNKNOWN";
        if (splitRole == DpoBatchSplitRole.Unknown || splitRole != expectedRole)
            return "CROSS_STACK_BATCH_SPLIT_ROLE_MISMATCH";
        if (provenance is null)
            return "CROSS_STACK_BATCH_PROVENANCE_MISSING";
        if (!provenanceMatches)
            return "CROSS_STACK_BATCH_PROVENANCE_MISMATCH";
        if (unknownRejectedTypes.Length > 0)
            return "CROSS_STACK_SEED_REJECTED_TYPE_UNKNOWN";
        if (duplicateCells.Length > 0)
            return "CROSS_STACK_SEED_MATRIX_DUPLICATE";
        if (!matrixComplete)
            return "CROSS_STACK_SEED_MATRIX_INCOMPLETE";
        return wrongAnnotationRate >= 1.0
            ? ""
            : "CROSS_STACK_SEED_WRONG_ANNOTATION_RATE_INCOMPLETE";
    }

    private static string MatrixCell(string domain, string rejectedType)
        => $"{domain}|{rejectedType}";

    private static string Slug(string value)
        => value.Replace('_', '-');
}

internal sealed record CrossStackBusinessFlowGraphPublicationValidation(
    CrossStackFlowPairBatchValidationResult Training,
    CrossStackFlowPairBatchValidationResult HeldOut);
