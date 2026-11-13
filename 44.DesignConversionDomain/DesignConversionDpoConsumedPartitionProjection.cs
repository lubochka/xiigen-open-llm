// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure.Logical;

/// <summary>
/// One owned snapshot of the exact train and held-out rows consumed by the
/// evaluated bridge. All admission, packing, evaluation, and published
/// identities must use this projection.
/// </summary>
internal sealed record DesignConversionDpoConsumedPartitionProjection(
    string DatasetId,
    IReadOnlyList<DesignConversionDpoTrainingRecord> TrainRecords,
    IReadOnlyList<DesignConversionDpoTrainingRecord> HeldOutRecords,
    string TrainRecordSetSha256,
    string HeldOutRecordSetSha256,
    string TrainSourceSetSha256,
    string HeldOutSourceSetSha256,
    string PartitionSnapshotSha256)
{
    internal int SourceRecordCount => TrainRecords.Count + HeldOutRecords.Count;
}

internal static class DesignConversionDpoConsumedPartitionProjectionContract
{
    internal static bool TryCreate(
        DesignConversionDpoResolvedPartition? partition,
        int maximumTrainRecords,
        out DesignConversionDpoConsumedPartitionProjection? projection)
    {
        projection = null;
        if (partition is null ||
            string.IsNullOrWhiteSpace(partition.DatasetId) ||
            partition.TrainRecords is null ||
            partition.HeldOutRecords is null ||
            maximumTrainRecords <= 0)
        {
            return false;
        }

        try
        {
            if (!DesignConversionDpoTrainingContentAuthority
                    .TryCapturePartition(
                        partition.TrainRecords,
                        partition.HeldOutRecords,
                        maximumTrainRecords,
                        out var train,
                        out var heldOut) ||
                train.Length == 0 || heldOut.Length == 0 ||
                train.Any(static row => row is null || row.Provenance is null) ||
                heldOut.Any(static row => row is null || row.Provenance is null) ||
                heldOut.Length > int.MaxValue - train.Length)
            {
                return false;
            }

            var trainRecordIdentities = train
                .Select(DesignConversionDpoDatasetBuilder
                    .ComputeRecordIdentityHash)
                .ToArray();
            var heldOutRecordIdentities = heldOut
                .Select(DesignConversionDpoDatasetBuilder
                    .ComputeRecordIdentityHash)
                .ToArray();
            if (trainRecordIdentities
                .Intersect(heldOutRecordIdentities, StringComparer.Ordinal)
                .Any())
            {
                return false;
            }

            var trainSourceIdentities = train
                .Select(DesignConversionDpoDatasetBuilder
                    .ComputeSourceIdentityHash)
                .ToArray();
            var heldOutSourceIdentities = heldOut
                .Select(DesignConversionDpoDatasetBuilder
                    .ComputeSourceIdentityHash)
                .ToArray();
            if (trainSourceIdentities
                .Intersect(heldOutSourceIdentities, StringComparer.Ordinal)
                .Any())
            {
                return false;
            }

            var trainRecordSetSha256 = ComputeSetHash(
                "design-conversion-dpo-train-record-set-v1",
                trainRecordIdentities);
            var heldOutRecordSetSha256 = ComputeSetHash(
                "design-conversion-dpo-heldout-record-set-v1",
                heldOutRecordIdentities);
            var trainSourceSetSha256 = ComputeSetHash(
                "design-conversion-dpo-train-source-set-v1",
                trainSourceIdentities);
            var heldOutSourceSetSha256 = ComputeSetHash(
                "design-conversion-dpo-heldout-source-set-v1",
                heldOutSourceIdentities);
            var partitionSnapshotSha256 = HashSnapshot(
                partition.DatasetId,
                train,
                heldOut,
                trainRecordSetSha256,
                heldOutRecordSetSha256,
                trainSourceSetSha256,
                heldOutSourceSetSha256);

            projection = new DesignConversionDpoConsumedPartitionProjection(
                partition.DatasetId,
                train,
                heldOut,
                trainRecordSetSha256,
                heldOutRecordSetSha256,
                trainSourceSetSha256,
                heldOutSourceSetSha256,
                partitionSnapshotSha256);
            return true;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return false;
        }
    }

    internal static bool MatchesDeclaredFullPartition(
        DesignConversionDpoResolvedPartition partition)
    {
        if (!TryCreate(
                partition,
                partition.TrainRecords?.Count ?? 0,
                out var full) ||
            full is null)
        {
            return false;
        }

        return string.Equals(
                   partition.DatasetId,
                   full.DatasetId,
                   StringComparison.Ordinal) &&
               string.Equals(
                   partition.TrainRecordSetSha256,
                   full.TrainRecordSetSha256,
                   StringComparison.Ordinal) &&
               string.Equals(
                   partition.HeldOutRecordSetSha256,
                   full.HeldOutRecordSetSha256,
                   StringComparison.Ordinal) &&
               string.Equals(
                   partition.TrainSourceSetSha256,
                   full.TrainSourceSetSha256,
                   StringComparison.Ordinal) &&
               string.Equals(
                   partition.HeldOutSourceSetSha256,
                   full.HeldOutSourceSetSha256,
                   StringComparison.Ordinal) &&
               string.Equals(
                   partition.PartitionSnapshotSha256,
                   full.PartitionSnapshotSha256,
                   StringComparison.Ordinal);
    }

    internal static string ComputeSetHash(
        string domain,
        IEnumerable<string> values)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendString(hash, domain);
        var ordered = values
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();
        AppendInt32(hash, ordered.Length);
        foreach (var value in ordered)
            AppendString(hash, value);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static string HashSnapshot(
        string datasetId,
        IReadOnlyList<DesignConversionDpoTrainingRecord> train,
        IReadOnlyList<DesignConversionDpoTrainingRecord> heldOut,
        params string[] bindings)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendString(hash, "design-conversion-dpo-safe-partition-snapshot-v1");
        AppendString(hash, datasetId);
        foreach (var binding in bindings)
            AppendString(hash, binding);
        AppendRecords(hash, "train", train);
        AppendRecords(hash, "heldout", heldOut);
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static void AppendRecords(
        IncrementalHash hash,
        string partition,
        IReadOnlyList<DesignConversionDpoTrainingRecord> records)
    {
        AppendString(hash, partition);
        var contentHashes = records
            .Select(record => Convert.ToHexString(SHA256.HashData(
                JsonSerializer.SerializeToUtf8Bytes(
                    record,
                    DesignConversionCheckpointJsonOptions.Standard)))
                .ToLowerInvariant())
            .Order(StringComparer.Ordinal)
            .ToArray();
        AppendInt32(hash, contentHashes.Length);
        foreach (var contentHash in contentHashes)
            AppendString(hash, contentHash);
    }

    private static void AppendString(IncrementalHash hash, string value)
    {
        var bytes = Encoding.UTF8.GetBytes(value);
        AppendInt32(hash, bytes.Length);
        hash.AppendData(bytes);
    }

    private static void AppendInt32(IncrementalHash hash, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        hash.AppendData(bytes);
    }
}
