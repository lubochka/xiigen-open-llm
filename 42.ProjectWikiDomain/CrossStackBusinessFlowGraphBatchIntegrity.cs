// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Globalization;
using System.Security.Cryptography;
using System.Text;

internal static class CrossStackBusinessFlowGraphBatchIntegrity
{
    internal const string RowFingerprintVersion = "cross-stack-business-flow-dpo-rows/v1";
    internal const string MatrixCellsFingerprintVersion = "cross-stack-business-flow-matrix-cells/v1";
    internal const string SourceProvenanceVersion = "cross-stack-business-flow-source-provenance/v1";
    internal const string ValidationCodeVersion = "cross-stack-flow-pair-batch-validator/v2";
    internal const string PublicationAuthorityVersion = "cross-stack-business-flow-publication-authority/v2";
    internal const string BaselineAllowlistVersion = "cross-stack-business-flow-baseline-allowlist/v1";

    internal static string ComputeRowsSha256(CrossStackBusinessFlowGraphDpoBatch batch)
        => ComputeRowsSha256(batch.Pairs ?? []);

    internal static string ComputeRowsSha256(IEnumerable<CrossStackBusinessFlowGraphDpoPair> rows)
    {
        var builder = new StringBuilder();
        AppendToken(builder, RowFingerprintVersion);
        var materialized = rows.ToArray();
        AppendInt(builder, materialized.Length);
        foreach (var row in materialized)
            AppendPair(builder, row);

        return Sha256(builder.ToString());
    }

    internal static string ComputeSourceProvenanceSha256(
        DpoBatchSourceKind sourceKind,
        DpoBatchSplitRole splitRole,
        string batchId,
        string rowFingerprintSha256,
        string issuer)
    {
        var builder = new StringBuilder();
        AppendToken(builder, SourceProvenanceVersion);
        AppendToken(builder, sourceKind.ToString());
        AppendToken(builder, splitRole.ToString());
        AppendToken(builder, batchId);
        AppendToken(builder, rowFingerprintSha256);
        AppendToken(builder, issuer);
        return Sha256(builder.ToString());
    }

    internal static string ComputeMatrixCellsSha256(IEnumerable<string> cells)
    {
        var builder = new StringBuilder();
        AppendToken(builder, MatrixCellsFingerprintVersion);
        AppendArray(builder, cells.Order(StringComparer.Ordinal));
        return Sha256(builder.ToString());
    }

    internal static string ComputeValidationSha256(CrossStackFlowPairBatchValidationResult result)
    {
        var builder = new StringBuilder();
        AppendToken(builder, ValidationCodeVersion);
        AppendToken(builder, result.SourceKind.ToString());
        AppendToken(builder, result.SplitRole.ToString());
        AppendToken(builder, result.BatchId);
        AppendToken(builder, result.RowFingerprintSha256);
        AppendToken(builder, result.SourceProvenanceFingerprintSha256);
        AppendArray(builder, result.ExpectedMatrixCells);
        AppendArray(builder, result.ActualMatrixCells);
        AppendToken(builder, result.ExpectedMatrixCellsSha256);
        AppendToken(builder, result.ActualMatrixCellsSha256);
        AppendBool(builder, result.MatrixComplete);
        AppendArray(builder, result.MissingMatrixCells);
        AppendArray(builder, result.DuplicateMatrixCells);
        AppendArray(builder, result.UnknownRejectedTypes);
        AppendDouble(builder, result.RecomputedWrongAnnotationRate);
        AppendBool(builder, result.Accepted);
        AppendToken(builder, result.ErrorCode);
        return Sha256(builder.ToString());
    }

    internal static string ComputeTrainingResultIdentitySha256(CrossStackBusinessFlowGraphTrainingRunResult result)
    {
        var builder = new StringBuilder();
        AppendToken(builder, "cross-stack-business-flow-training-result/v1");
        AppendInt(builder, result.TrainingRowCount);
        AppendInt(builder, result.HeldOutRowCount);
        AppendDouble(builder, result.ClientToStoredProcedurePathRecall);
        AppendDouble(builder, result.ClientServerEdgeRecall);
        AppendDouble(builder, result.StoredProcedureEdgeRecall);
        AppendDouble(builder, result.EvidenceGroundingRate);
        AppendDouble(builder, result.WrongAnnotationRate);
        AppendToken(builder, result.TrainingBatchId);
        AppendToken(builder, result.HeldOutBatchId);
        AppendToken(builder, result.TrainingRowsSha256);
        AppendToken(builder, result.HeldOutRowsSha256);
        AppendToken(builder, result.TrainingValidationFingerprintSha256);
        AppendToken(builder, result.HeldOutValidationFingerprintSha256);
        AppendToken(builder, result.TrainingExpectedMatrixCellsSha256);
        AppendToken(builder, result.TrainingActualMatrixCellsSha256);
        AppendToken(builder, result.HeldOutExpectedMatrixCellsSha256);
        AppendToken(builder, result.HeldOutActualMatrixCellsSha256);
        AppendToken(builder, result.Verdict);
        AppendInt(builder, result.Cases?.Length ?? 0);
        foreach (var item in result.Cases ?? [])
        {
            AppendToken(builder, item.PairId);
            AppendDouble(builder, item.ClientToStoredProcedurePathRecall);
            AppendDouble(builder, item.ClientServerEdgeRecall);
            AppendDouble(builder, item.StoredProcedureEdgeRecall);
            AppendDouble(builder, item.EvidenceGroundingRate);
            AppendDouble(builder, item.Confidence);
        }

        AppendToken(builder, result.UnitTrace.BatchId);
        AppendInt(builder, result.UnitTrace.RowsTrained);
        AppendInt(builder, result.UnitTrace.HeldOutRows);
        return Sha256(builder.ToString());
    }

    internal static string ComputeDpoMetricIdentitySha256(ProjectWikiDpoMetricRunResult result)
    {
        var builder = new StringBuilder();
        AppendToken(builder, "project-wiki-dpo-metric-result/v1");
        AppendToken(builder, result.RunId);
        AppendToken(builder, result.UnitId);
        AppendToken(builder, result.BatchId);
        AppendToken(builder, result.PolicyCheckpointPath);
        AppendToken(builder, result.ReferenceCheckpointPath);
        AppendInt(builder, result.TrainingRowCount);
        AppendInt(builder, result.HeldOutRowCount);
        AppendDouble(builder, result.FinalLoss);
        AppendDouble(builder, result.FinalAccuracy);
        AppendDouble(builder, result.FinalRewardMargin);
        AppendDouble(builder, result.HeldOutLoss);
        AppendDouble(builder, result.HeldOutAccuracy);
        AppendDouble(builder, result.HeldOutRewardMargin);
        AppendToken(builder, result.CrossStackTrainingBatchId);
        AppendToken(builder, result.CrossStackHeldOutBatchId);
        AppendToken(builder, result.CrossStackTrainingRowsSha256);
        AppendToken(builder, result.CrossStackHeldOutRowsSha256);
        AppendToken(builder, result.CrossStackTrainingValidationFingerprintSha256);
        AppendToken(builder, result.CrossStackHeldOutValidationFingerprintSha256);
        AppendInt(builder, result.BatchSummaries?.Length ?? 0);
        foreach (var item in result.BatchSummaries ?? [])
        {
            AppendInt(builder, item.BatchIndex);
            AppendToken(builder, item.FirstPairId);
            AppendToken(builder, item.LastPairId);
            AppendInt(builder, item.PairCount);
            AppendDouble(builder, item.Loss);
            AppendDouble(builder, item.Accuracy);
            AppendDouble(builder, item.RewardMargin);
        }

        return Sha256(builder.ToString());
    }

    internal static string ComputeAuthoritySha256(
        DpoBatchSourceKind sourceKind,
        DpoBatchSplitRole trainingSplitRole,
        DpoBatchSplitRole heldOutSplitRole,
        string trainingBatchId,
        string heldOutBatchId,
        string trainingRowsSha256,
        string heldOutRowsSha256,
        string trainingSourceProvenanceSha256,
        string heldOutSourceProvenanceSha256,
        string trainingValidationSha256,
        string heldOutValidationSha256,
        string trainingExpectedMatrixCellsSha256,
        string trainingActualMatrixCellsSha256,
        string heldOutExpectedMatrixCellsSha256,
        string heldOutActualMatrixCellsSha256,
        string trainingResultIdentitySha256,
        string dpoMetricIdentitySha256)
    {
        var builder = new StringBuilder();
        AppendToken(builder, PublicationAuthorityVersion);
        AppendToken(builder, sourceKind.ToString());
        AppendToken(builder, trainingSplitRole.ToString());
        AppendToken(builder, heldOutSplitRole.ToString());
        AppendToken(builder, trainingBatchId);
        AppendToken(builder, heldOutBatchId);
        AppendToken(builder, trainingRowsSha256);
        AppendToken(builder, heldOutRowsSha256);
        AppendToken(builder, trainingSourceProvenanceSha256);
        AppendToken(builder, heldOutSourceProvenanceSha256);
        AppendToken(builder, trainingValidationSha256);
        AppendToken(builder, heldOutValidationSha256);
        AppendToken(builder, trainingExpectedMatrixCellsSha256);
        AppendToken(builder, trainingActualMatrixCellsSha256);
        AppendToken(builder, heldOutExpectedMatrixCellsSha256);
        AppendToken(builder, heldOutActualMatrixCellsSha256);
        AppendToken(builder, ValidationCodeVersion);
        AppendToken(builder, trainingResultIdentitySha256);
        AppendToken(builder, dpoMetricIdentitySha256);
        return Sha256(builder.ToString());
    }

    internal static CrossStackBusinessFlowGraphBaselineDisposition CreateBatchBaselineDisposition(
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainingValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation)
        => CreateBaselineDisposition(
            "cross_stack_batch_root",
            trainingBatch,
            heldOutBatch,
            trainingValidation,
            heldOutValidation,
            [
                "baseline-disposition.json",
                "dpo-pairs.jsonl",
                "heldout-dpo-pairs.jsonl",
                "independent-validation-report.json",
                "batch-summary.json",
                "dpo-runs/**"
            ]);

    internal static CrossStackBusinessFlowGraphBaselineDisposition CreateMetricRunBaselineDisposition(
        CrossStackBusinessFlowGraphBaselineDisposition sourceDisposition)
        => sourceDisposition with
        {
            ScopeName = "project_wiki_dpo_metric_run_root",
            AllowedArtifactPatterns =
            [
                "baseline-disposition.json",
                "dpo-pairs.jsonl",
                "dpo-pair-diagnostics.jsonl",
                "project-wiki-dpo-metric-summary.json",
                "checkpoints/policy_final.bin",
                "checkpoints/reference_base.bin",
                "checkpoints/dpo_policy_step_*.bin",
                "checkpoints/dpo_step_*.json",
                "checkpoints/.dpo_step_*.lock"
            ],
            CreatedAtUtc = DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture)
        };

    private static CrossStackBusinessFlowGraphBaselineDisposition CreateBaselineDisposition(
        string scopeName,
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainingValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation,
        string[] allowedArtifactPatterns)
    {
        var disposition = new CrossStackBusinessFlowGraphBaselineDisposition(
            CrossStackBusinessFlowGraphUnit.UnitId,
            scopeName,
            CrossStackBusinessFlowGraphArtifactDisposition.BaselineOnlyNotPromotable,
            trainingBatch.SourceKind,
            trainingBatch.BatchId,
            heldOutBatch.BatchId,
            trainingValidation.RowFingerprintSha256,
            heldOutValidation.RowFingerprintSha256,
            trainingValidation.ValidationFingerprintSha256,
            heldOutValidation.ValidationFingerprintSha256,
            BaselineAllowlistVersion,
            allowedArtifactPatterns,
            [
                "generated_capability_manifest.json",
                "Trusted",
                "remote_unit_register",
                "model_repository_upload"
            ],
            DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture));

        return disposition with
        {
            TrainingExpectedMatrixCellsSha256 = trainingValidation.ExpectedMatrixCellsSha256,
            TrainingActualMatrixCellsSha256 = trainingValidation.ActualMatrixCellsSha256,
            HeldOutExpectedMatrixCellsSha256 = heldOutValidation.ExpectedMatrixCellsSha256,
            HeldOutActualMatrixCellsSha256 = heldOutValidation.ActualMatrixCellsSha256
        };
    }

    private static void AppendPair(StringBuilder builder, CrossStackBusinessFlowGraphDpoPair row)
    {
        AppendToken(builder, row.PairId);
        AppendBoundary(builder, row.BoundaryMap);
        AppendToken(builder, row.A);
        AppendToken(builder, row.Chosen);
        AppendToken(builder, row.Rejected);
        AppendToken(builder, row.RejectedType);
        AppendArray(builder, row.RequiredPathIds);
        AppendArray(builder, row.EvidenceIds);
    }

    private static void AppendBoundary(StringBuilder builder, SystemBoundaryMapResult boundary)
    {
        AppendToken(builder, boundary.SystemId);
        AppendArray(builder, boundary.ProjectIds);
        AppendInt(builder, boundary.Edges?.Length ?? 0);
        foreach (var edge in boundary.Edges ?? [])
        {
            AppendToken(builder, edge.EdgeId);
            AppendToken(builder, edge.SourceProjectId);
            AppendToken(builder, edge.TargetProjectId);
            AppendToken(builder, edge.BoundaryKind);
            AppendArray(builder, edge.SharedMarkers);
            AppendArray(builder, edge.EvidenceIds);
            AppendDouble(builder, edge.Confidence);
            AppendToken(builder, edge.Status);
        }

        AppendArray(builder, boundary.UnknownBoundaries);
        AppendArray(builder, boundary.RequiredFollowUpEvidence);
        AppendDouble(builder, boundary.Confidence);
        AppendArray(builder, boundary.Warnings);
    }

    private static void AppendArray(StringBuilder builder, IEnumerable<string>? values)
    {
        var materialized = values?.ToArray() ?? [];
        AppendInt(builder, materialized.Length);
        foreach (var value in materialized)
            AppendToken(builder, value);
    }

    private static void AppendBool(StringBuilder builder, bool value)
        => AppendToken(builder, value ? "true" : "false");

    private static void AppendDouble(StringBuilder builder, double value)
        => AppendToken(builder, value.ToString("R", CultureInfo.InvariantCulture));

    private static void AppendInt(StringBuilder builder, int value)
        => AppendToken(builder, value.ToString(CultureInfo.InvariantCulture));

    private static void AppendToken(StringBuilder builder, string? value)
    {
        var text = value ?? "";
        builder.Append(text.Length.ToString(CultureInfo.InvariantCulture));
        builder.Append(':');
        builder.Append(text);
        builder.Append(';');
    }

    private static string Sha256(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
}

internal static class CrossStackBusinessFlowGraphAtomicArtifactWriter
{
    internal static Task WriteJsonAsync<T>(
        string path,
        T value,
        System.Text.Json.JsonSerializerOptions options,
        CancellationToken ct)
        => WriteTextAsync(
            path,
            System.Text.Json.JsonSerializer.Serialize(value, options),
            ct);

    internal static Task WriteJsonLinesAsync<T>(
        string path,
        IEnumerable<T> values,
        System.Text.Json.JsonSerializerOptions options,
        CancellationToken ct)
    {
        // JSONL — это один полный JSON-объект на одну физическую строку. Общие
        // настройки сериализации сохраняем через копию, но отступы выключаем,
        // чтобы отступы обычных JSON-файлов не ломали построчное чтение.
        var jsonLineOptions =
            CreateJsonLinesSerializerOptions(options);
        return WriteTextAsync(
            path,
            string.Concat(values.Select(value =>
                System.Text.Json.JsonSerializer.Serialize(value, jsonLineOptions) +
                Environment.NewLine)),
            ct);
    }

    private static System.Text.Json.JsonSerializerOptions
        CreateJsonLinesSerializerOptions(
            System.Text.Json.JsonSerializerOptions options)
        => new(options)
        {
            WriteIndented = false
        };

    internal static Task WriteJsonLinesAsync(
        string path,
        IEnumerable<string> serializedRows,
        CancellationToken ct)
        => WriteTextAsync(
            path,
            string.Concat(serializedRows.Select(row => row + Environment.NewLine)),
            ct);

    internal static async Task WriteTextAsync(
        string path,
        string text,
        CancellationToken ct)
        => await ReplaceWithTempFileAsync(
            path,
            async tempPath =>
            {
                await File.WriteAllTextAsync(
                    tempPath,
                    text,
                    Encoding.UTF8,
                    ct);
            },
            ct);

    internal static async Task ReplaceWithTempFileAsync(
        string path,
        Func<string, Task> writeTempFile,
        CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);

        // Baseline artifact появляется через временный файл в той же папке:
        // читатель видит либо старую версию, либо полностью записанную новую.
        var tempPath = Path.Combine(
            directory ?? ".",
            $".{Path.GetFileName(path)}.{Guid.NewGuid():N}.tmp");
        try
        {
            await writeTempFile(tempPath);
            ct.ThrowIfCancellationRequested();
            File.Move(tempPath, path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }
}

internal sealed class CrossStackBusinessFlowGraphBatchProvenanceProof
{
    private const string DeterministicSeedIssuer = "CrossStackBusinessFlowGraphDpoGenerator";
    private const string RepositoryOwnedCorpusIssuer =
        "CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring";

    private CrossStackBusinessFlowGraphBatchProvenanceProof(
        DpoBatchSourceKind sourceKind,
        DpoBatchSplitRole splitRole,
        string batchId,
        string rowFingerprintSha256,
        string issuer,
        string provenanceFingerprintSha256)
    {
        SourceKind = sourceKind;
        SplitRole = splitRole;
        BatchId = batchId;
        RowFingerprintSha256 = rowFingerprintSha256;
        Issuer = issuer;
        ProvenanceFingerprintSha256 = provenanceFingerprintSha256;
    }

    internal DpoBatchSourceKind SourceKind { get; }

    internal DpoBatchSplitRole SplitRole { get; }

    internal string BatchId { get; }

    internal string RowFingerprintSha256 { get; }

    internal string Issuer { get; }

    internal string ProvenanceFingerprintSha256 { get; }

    internal static CrossStackBusinessFlowGraphDpoBatch AttachDeterministicSeed(
        CrossStackBusinessFlowGraphDpoBatch batch,
        DpoBatchSplitRole splitRole)
    {
        var labeled = batch with
        {
            SourceKind = DpoBatchSourceKind.DeterministicSeed,
            SplitRole = splitRole
        };
        return labeled with { SourceProvenanceProof = Mint(labeled, DeterministicSeedIssuer) };
    }

    internal static CrossStackBusinessFlowGraphDpoBatch
        AttachRepositoryOwnedCorpus(
            CrossStackBusinessFlowGraphDpoBatch batch,
            DpoBatchSplitRole splitRole)
    {
        var labeled = batch with
        {
            SourceKind = DpoBatchSourceKind.RealRepositoryCorpus,
            SplitRole = splitRole
        };
        // WHY: repository-owned CrossStack rows get a separate issuer from the
        // deterministic seed baseline, so a reopened corpus cannot inherit the
        // baseline proof by accident or by replaying the old generator path.
        return labeled with
        {
            SourceProvenanceProof = Mint(
                labeled,
                RepositoryOwnedCorpusIssuer)
        };
    }

    internal bool Matches(CrossStackBusinessFlowGraphDpoBatch batch, DpoBatchSplitRole expectedRole)
    {
        var rowFingerprint = CrossStackBusinessFlowGraphBatchIntegrity.ComputeRowsSha256(batch);
        var expectedFingerprint = CrossStackBusinessFlowGraphBatchIntegrity.ComputeSourceProvenanceSha256(
            SourceKind,
            SplitRole,
            BatchId,
            RowFingerprintSha256,
            Issuer);
        return batch.SourceKind == SourceKind &&
               batch.SplitRole == SplitRole &&
               expectedRole == SplitRole &&
               string.Equals(batch.BatchId, BatchId, StringComparison.Ordinal) &&
               string.Equals(rowFingerprint, RowFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(expectedFingerprint, ProvenanceFingerprintSha256, StringComparison.Ordinal);
    }

    private static CrossStackBusinessFlowGraphBatchProvenanceProof Mint(
        CrossStackBusinessFlowGraphDpoBatch batch,
        string issuer)
    {
        var rowsSha256 = CrossStackBusinessFlowGraphBatchIntegrity.ComputeRowsSha256(batch);
        var provenanceSha256 = CrossStackBusinessFlowGraphBatchIntegrity.ComputeSourceProvenanceSha256(
            batch.SourceKind,
            batch.SplitRole,
            batch.BatchId,
            rowsSha256,
            issuer);
        return new CrossStackBusinessFlowGraphBatchProvenanceProof(
            batch.SourceKind,
            batch.SplitRole,
            batch.BatchId,
            rowsSha256,
            issuer,
            provenanceSha256);
    }
}

internal sealed class CrossStackBusinessFlowGraphPublicationAuthority
{
    private CrossStackBusinessFlowGraphPublicationAuthority(
        DpoBatchSourceKind sourceKind,
        DpoBatchSplitRole trainingSplitRole,
        DpoBatchSplitRole heldOutSplitRole,
        string trainingBatchId,
        string heldOutBatchId,
        string trainingRowsSha256,
        string heldOutRowsSha256,
        string trainingSourceProvenanceSha256,
        string heldOutSourceProvenanceSha256,
        string trainingValidationFingerprintSha256,
        string heldOutValidationFingerprintSha256,
        string trainingExpectedMatrixCellsSha256,
        string trainingActualMatrixCellsSha256,
        string heldOutExpectedMatrixCellsSha256,
        string heldOutActualMatrixCellsSha256,
        string trainingResultIdentitySha256,
        string dpoMetricIdentitySha256,
        string authorityFingerprintSha256)
    {
        SourceKind = sourceKind;
        ProofVersion = CrossStackBusinessFlowGraphBatchIntegrity.PublicationAuthorityVersion;
        ValidationCodeVersion = CrossStackBusinessFlowGraphBatchIntegrity.ValidationCodeVersion;
        TrainingSplitRole = trainingSplitRole;
        HeldOutSplitRole = heldOutSplitRole;
        TrainingBatchId = trainingBatchId;
        HeldOutBatchId = heldOutBatchId;
        TrainingRowsSha256 = trainingRowsSha256;
        HeldOutRowsSha256 = heldOutRowsSha256;
        TrainingSourceProvenanceSha256 = trainingSourceProvenanceSha256;
        HeldOutSourceProvenanceSha256 = heldOutSourceProvenanceSha256;
        TrainingValidationFingerprintSha256 = trainingValidationFingerprintSha256;
        HeldOutValidationFingerprintSha256 = heldOutValidationFingerprintSha256;
        TrainingExpectedMatrixCellsSha256 = trainingExpectedMatrixCellsSha256;
        TrainingActualMatrixCellsSha256 = trainingActualMatrixCellsSha256;
        HeldOutExpectedMatrixCellsSha256 = heldOutExpectedMatrixCellsSha256;
        HeldOutActualMatrixCellsSha256 = heldOutActualMatrixCellsSha256;
        TrainingResultIdentitySha256 = trainingResultIdentitySha256;
        DpoMetricIdentitySha256 = dpoMetricIdentitySha256;
        AuthorityFingerprintSha256 = authorityFingerprintSha256;
    }

    public DpoBatchSourceKind SourceKind { get; }

    public string ProofVersion { get; }

    public string ValidationCodeVersion { get; }

    internal DpoBatchSplitRole TrainingSplitRole { get; }

    internal DpoBatchSplitRole HeldOutSplitRole { get; }

    internal string TrainingBatchId { get; }

    internal string HeldOutBatchId { get; }

    internal string TrainingRowsSha256 { get; }

    internal string HeldOutRowsSha256 { get; }

    public string AuthorityFingerprintSha256 { get; }

    internal string TrainingSourceProvenanceSha256 { get; }

    internal string HeldOutSourceProvenanceSha256 { get; }

    internal string TrainingValidationFingerprintSha256 { get; }

    internal string HeldOutValidationFingerprintSha256 { get; }

    internal string TrainingExpectedMatrixCellsSha256 { get; }

    internal string TrainingActualMatrixCellsSha256 { get; }

    internal string HeldOutExpectedMatrixCellsSha256 { get; }

    internal string HeldOutActualMatrixCellsSha256 { get; }

    internal string TrainingResultIdentitySha256 { get; }

    internal string DpoMetricIdentitySha256 { get; }

    internal static CrossStackBusinessFlowGraphPublicationAuthority Create(
        DpoBatchSourceKind sourceKind,
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainingValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation,
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics)
    {
        var trainingResultIdentity = CrossStackBusinessFlowGraphBatchIntegrity
            .ComputeTrainingResultIdentitySha256(trainingResult);
        var dpoMetricIdentity = CrossStackBusinessFlowGraphBatchIntegrity
            .ComputeDpoMetricIdentitySha256(dpoMetrics);
        var authorityFingerprint = CrossStackBusinessFlowGraphBatchIntegrity.ComputeAuthoritySha256(
            sourceKind,
            trainingValidation.SplitRole,
            heldOutValidation.SplitRole,
            trainingBatch.BatchId,
            heldOutBatch.BatchId,
            trainingValidation.RowFingerprintSha256,
            heldOutValidation.RowFingerprintSha256,
            trainingValidation.SourceProvenanceFingerprintSha256,
            heldOutValidation.SourceProvenanceFingerprintSha256,
            trainingValidation.ValidationFingerprintSha256,
            heldOutValidation.ValidationFingerprintSha256,
            trainingValidation.ExpectedMatrixCellsSha256,
            trainingValidation.ActualMatrixCellsSha256,
            heldOutValidation.ExpectedMatrixCellsSha256,
            heldOutValidation.ActualMatrixCellsSha256,
            trainingResultIdentity,
            dpoMetricIdentity);
        return new CrossStackBusinessFlowGraphPublicationAuthority(
            sourceKind,
            trainingValidation.SplitRole,
            heldOutValidation.SplitRole,
            trainingBatch.BatchId,
            heldOutBatch.BatchId,
            trainingValidation.RowFingerprintSha256,
            heldOutValidation.RowFingerprintSha256,
            trainingValidation.SourceProvenanceFingerprintSha256,
            heldOutValidation.SourceProvenanceFingerprintSha256,
            trainingValidation.ValidationFingerprintSha256,
            heldOutValidation.ValidationFingerprintSha256,
            trainingValidation.ExpectedMatrixCellsSha256,
            trainingValidation.ActualMatrixCellsSha256,
            heldOutValidation.ExpectedMatrixCellsSha256,
            heldOutValidation.ActualMatrixCellsSha256,
            trainingResultIdentity,
            dpoMetricIdentity,
            authorityFingerprint);
    }

    internal bool Matches(
        CrossStackBusinessFlowGraphDpoBatch trainingBatch,
        CrossStackBusinessFlowGraphDpoBatch heldOutBatch,
        CrossStackFlowPairBatchValidationResult trainingValidation,
        CrossStackFlowPairBatchValidationResult heldOutValidation,
        CrossStackBusinessFlowGraphTrainingRunResult trainingResult,
        ProjectWikiDpoMetricRunResult dpoMetrics)
    {
        var trainingResultIdentity = CrossStackBusinessFlowGraphBatchIntegrity
            .ComputeTrainingResultIdentitySha256(trainingResult);
        var dpoMetricIdentity = CrossStackBusinessFlowGraphBatchIntegrity
            .ComputeDpoMetricIdentitySha256(dpoMetrics);
        var expectedAuthorityFingerprint = CrossStackBusinessFlowGraphBatchIntegrity.ComputeAuthoritySha256(
            SourceKind,
            trainingValidation.SplitRole,
            heldOutValidation.SplitRole,
            trainingBatch.BatchId,
            heldOutBatch.BatchId,
            trainingValidation.RowFingerprintSha256,
            heldOutValidation.RowFingerprintSha256,
            trainingValidation.SourceProvenanceFingerprintSha256,
            heldOutValidation.SourceProvenanceFingerprintSha256,
            trainingValidation.ValidationFingerprintSha256,
            heldOutValidation.ValidationFingerprintSha256,
            trainingValidation.ExpectedMatrixCellsSha256,
            trainingValidation.ActualMatrixCellsSha256,
            heldOutValidation.ExpectedMatrixCellsSha256,
            heldOutValidation.ActualMatrixCellsSha256,
            trainingResultIdentity,
            dpoMetricIdentity);

        return string.Equals(ProofVersion, CrossStackBusinessFlowGraphBatchIntegrity.PublicationAuthorityVersion, StringComparison.Ordinal) &&
               string.Equals(ValidationCodeVersion, CrossStackBusinessFlowGraphBatchIntegrity.ValidationCodeVersion, StringComparison.Ordinal) &&
               SourceKind == trainingBatch.SourceKind &&
               SourceKind == heldOutBatch.SourceKind &&
               TrainingSplitRole == DpoBatchSplitRole.Training &&
               HeldOutSplitRole == DpoBatchSplitRole.HeldOut &&
               TrainingSplitRole == trainingValidation.SplitRole &&
               HeldOutSplitRole == heldOutValidation.SplitRole &&
               string.Equals(TrainingBatchId, trainingBatch.BatchId, StringComparison.Ordinal) &&
               string.Equals(HeldOutBatchId, heldOutBatch.BatchId, StringComparison.Ordinal) &&
               string.Equals(TrainingRowsSha256, trainingValidation.RowFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(HeldOutRowsSha256, heldOutValidation.RowFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(TrainingSourceProvenanceSha256, trainingValidation.SourceProvenanceFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(HeldOutSourceProvenanceSha256, heldOutValidation.SourceProvenanceFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(TrainingValidationFingerprintSha256, trainingValidation.ValidationFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(HeldOutValidationFingerprintSha256, heldOutValidation.ValidationFingerprintSha256, StringComparison.Ordinal) &&
               string.Equals(TrainingExpectedMatrixCellsSha256, trainingValidation.ExpectedMatrixCellsSha256, StringComparison.Ordinal) &&
               string.Equals(TrainingActualMatrixCellsSha256, trainingValidation.ActualMatrixCellsSha256, StringComparison.Ordinal) &&
               string.Equals(HeldOutExpectedMatrixCellsSha256, heldOutValidation.ExpectedMatrixCellsSha256, StringComparison.Ordinal) &&
               string.Equals(HeldOutActualMatrixCellsSha256, heldOutValidation.ActualMatrixCellsSha256, StringComparison.Ordinal) &&
               string.Equals(TrainingResultIdentitySha256, trainingResultIdentity, StringComparison.Ordinal) &&
               string.Equals(DpoMetricIdentitySha256, dpoMetricIdentity, StringComparison.Ordinal) &&
               string.Equals(AuthorityFingerprintSha256, expectedAuthorityFingerprint, StringComparison.Ordinal);
    }
}
