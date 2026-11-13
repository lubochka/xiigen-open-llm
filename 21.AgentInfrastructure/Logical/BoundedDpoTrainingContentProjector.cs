// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ContinuousLearning")]

namespace AgentInfrastructure.Logical;

using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

/// <summary>
/// One exact DPO row before it crosses a mutation-admission boundary. Context
/// identifies the canonical consumer projection; the vector is the vector
/// actually supplied to that projection.
/// </summary>
public sealed record DpoTrainingContentRowInput(
    string Prompt,
    string Chosen,
    string Rejected,
    IReadOnlyList<double>? TeachingVector,
    string Context);

/// <summary>
/// Immutable snapshot of one bounded DPO row.
/// </summary>
public sealed class DpoTrainingContentRow
{
    private readonly ReadOnlyCollection<double> _teachingVector;

    internal DpoTrainingContentRow(
        string prompt,
        string chosen,
        string rejected,
        double[] teachingVector,
        string context,
        string sha256)
    {
        Prompt = prompt;
        Chosen = chosen;
        Rejected = rejected;
        _teachingVector = Array.AsReadOnly(teachingVector);
        Context = context;
        Sha256 = sha256;
    }

    public string Prompt { get; }
    public string Chosen { get; }
    public string Rejected { get; }
    public IReadOnlyList<double> TeachingVector => _teachingVector;
    public string Context { get; }
    public string Sha256 { get; }
}

/// <summary>
/// Immutable, ordered content presented to legal teacher admission.
/// </summary>
public sealed class DpoTrainingContentBatch
{
    private readonly ReadOnlyCollection<DpoTrainingContentRow> _rows;

    internal DpoTrainingContentBatch(
        DpoTrainingContentRow[] rows,
        string sha256)
    {
        _rows = Array.AsReadOnly(rows);
        Sha256 = sha256;
    }

    public IReadOnlyList<DpoTrainingContentRow> Rows => _rows;
    public string Sha256 { get; }
}

/// <summary>
/// The single bounded deterministic digest contract used by generic DPO
/// consumers. Bounds are checked before vector copies or UTF-8 materialization.
/// Row order, prompt, chosen, rejected, vector bits, and consumer context all
/// participate in the batch identity. A row digest is deliberately independent
/// of its eventual ordinal so a trusted producer can bind durable provenance
/// before batching; the batch digest commits each row digest at its exact
/// ordinal. Identical rows therefore share a content digest, while their batch
/// positions remain part of the ordered batch commitment.
/// </summary>
public static class BoundedDpoTrainingContentProjector
{
    public const int MaximumRowCount = 4_096;
    public const int MaximumTextUtf8Bytes = 4 * 1_048_576;
    public const int MaximumContextUtf8Bytes = 16_384;
    public const int MaximumVectorLength = 65_536;
    public const long MaximumBatchUtf8Bytes = 64L * 1_048_576;
    public const long MaximumBatchVectorValues = 1_048_576;

    public const string InvalidErrorCode =
        "DPO_TRAINING_CONTENT_INVALID";
    public const string LimitExceededErrorCode =
        "DPO_TRAINING_CONTENT_LIMIT_EXCEEDED";

    public static OperationResult<DpoTrainingContentBatch> Project(
        IReadOnlyList<DpoTrainingContentRowInput>? source)
    {
        if (source is null)
            return Invalid();

        int count;
        try
        {
            count = source.Count;
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }
        if (count is <= 0 or > MaximumRowCount)
            return LimitExceeded();

        var rows = new DpoTrainingContentRow[count];
        long totalUtf8Bytes = 0;
        long totalVectorValues = 0;
        try
        {
            for (var ordinal = 0; ordinal < count; ordinal++)
            {
                var input = source[ordinal];
                if (input is null ||
                    !TryGetBoundedUtf8Count(
                        input.Prompt,
                        MaximumTextUtf8Bytes,
                        out var promptBytes) ||
                    !TryGetBoundedUtf8Count(
                        input.Chosen,
                        MaximumTextUtf8Bytes,
                        out var chosenBytes) ||
                    !TryGetBoundedUtf8Count(
                        input.Rejected,
                        MaximumTextUtf8Bytes,
                        out var rejectedBytes) ||
                    !TryGetBoundedUtf8Count(
                        input.Context,
                        MaximumContextUtf8Bytes,
                        out var contextBytes) ||
                    string.IsNullOrWhiteSpace(input.Prompt) ||
                    string.IsNullOrWhiteSpace(input.Chosen) ||
                    string.IsNullOrWhiteSpace(input.Rejected) ||
                    string.IsNullOrWhiteSpace(input.Context))
                {
                    return Invalid();
                }

                totalUtf8Bytes = checked(
                    totalUtf8Bytes + promptBytes + chosenBytes +
                    rejectedBytes + contextBytes);
                if (totalUtf8Bytes > MaximumBatchUtf8Bytes)
                    return LimitExceeded();

                var vector = SnapshotVector(
                    input.TeachingVector,
                    ref totalVectorValues);
                if (vector is null)
                    return LimitExceeded();

                var rowSha256 = ComputeRowSha256(
                    input.Prompt,
                    input.Chosen,
                    input.Rejected,
                    vector,
                    input.Context);
                rows[ordinal] = new DpoTrainingContentRow(
                    input.Prompt,
                    input.Chosen,
                    input.Rejected,
                    vector,
                    input.Context,
                    rowSha256);
            }

            using var batchHash =
                IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            AppendString(batchHash, "bounded-dpo-training-batch-v1");
            AppendInt32(batchHash, rows.Length);
            for (var ordinal = 0; ordinal < rows.Length; ordinal++)
            {
                AppendInt32(batchHash, ordinal);
                AppendString(batchHash, rows[ordinal].Sha256);
            }
            return OperationResult<DpoTrainingContentBatch>.Ok(
                new DpoTrainingContentBatch(
                    rows,
                    Convert.ToHexString(batchHash.GetHashAndReset())
                        .ToLowerInvariant()));
        }
        catch (OverflowException)
        {
            return LimitExceeded();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }
    }

    private static double[]? SnapshotVector(
        IReadOnlyList<double>? source,
        ref long totalVectorValues)
    {
        if (source is null)
            return [];

        var count = source.Count;
        if (count > MaximumVectorLength)
            return null;
        totalVectorValues = checked(totalVectorValues + count);
        if (totalVectorValues > MaximumBatchVectorValues)
            return null;

        var snapshot = new double[count];
        for (var index = 0; index < count; index++)
        {
            var value = source[index];
            if (!double.IsFinite(value))
                return null;
            snapshot[index] = value;
        }
        return snapshot;
    }

    private static string ComputeRowSha256(
        string prompt,
        string chosen,
        string rejected,
        double[] vector,
        string context)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        AppendString(hash, "bounded-dpo-training-row-v1");
        AppendString(hash, context);
        AppendString(hash, prompt);
        AppendString(hash, chosen);
        AppendString(hash, rejected);
        AppendInt32(hash, vector.Length);
        foreach (var value in vector)
            AppendInt64(hash, BitConverter.DoubleToInt64Bits(value));
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static bool TryGetBoundedUtf8Count(
        string? value,
        int maximum,
        out int bytes)
    {
        bytes = 0;
        if (value is null)
            return false;
        bytes = Encoding.UTF8.GetByteCount(value);
        return bytes <= maximum;
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

    private static void AppendInt64(IncrementalHash hash, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, value);
        hash.AppendData(bytes);
    }

    private static OperationResult<DpoTrainingContentBatch> Invalid()
        => OperationResult<DpoTrainingContentBatch>.Fail(
            "DPO training content is invalid.",
            InvalidErrorCode);

    private static OperationResult<DpoTrainingContentBatch> LimitExceeded()
        => OperationResult<DpoTrainingContentBatch>.Fail(
            "DPO training content exceeds a bounded admission limit.",
            LimitExceededErrorCode);
}
