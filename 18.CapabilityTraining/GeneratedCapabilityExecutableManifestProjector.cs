// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;

/// <summary>
/// Canonical, self-excluding executable identity for a generated capability
/// manifest. The persisted manifest-file hash and executable attestation are
/// deliberately excluded to avoid circular identity.
/// </summary>
internal static class GeneratedCapabilityExecutableManifestProjector
{
    private const int MaximumArrayCount = 256;
    internal const int MaximumCanonicalPayloadBytes =
        (int)TeachingRegistryPersistence.MaximumManifestBytes;

    internal const string InvalidErrorCode =
        "GENERATED_CAPABILITY_EXECUTABLE_IDENTITY_INVALID";

    internal static OperationResult<string> ProjectSha256(
        GeneratedCapabilityCheckpointManifest? manifest,
        string? exactTokenizerArtifactSha256,
        string? exactPolicyArtifactSha256)
    {
        if (manifest is null ||
            manifest.SchemaVersion !=
                GeneratedCapabilityCheckpointManifest.CurrentSchemaVersion ||
            !TeachingRegistryPersistence.HasBoundedExecutablePredicateIds(
                manifest.PredicateIds) ||
            manifest.UnitRegistrationOrder is null or <= 0 ||
            manifest.FlowRegistrationOrder is null or <= 0 ||
            !RuntimeProvenance.IsSha256Hex(exactTokenizerArtifactSha256) ||
            !RuntimeProvenance.IsSha256Hex(exactPolicyArtifactSha256) ||
            !RuntimeProvenance.IsSha256Hex(manifest.TokenizerSha256) ||
            !RuntimeProvenance.IsSha256Hex(
                manifest.PolicyCheckpointSha256) ||
            !RuntimeProvenance.IsSha256Hex(
                manifest.RuntimeProvenanceSha256) ||
            !RuntimeProvenance.FixedTimeSha256Equals(
                manifest.TokenizerSha256!,
                exactTokenizerArtifactSha256!) ||
            !RuntimeProvenance.FixedTimeSha256Equals(
                manifest.PolicyCheckpointSha256!,
                exactPolicyArtifactSha256!) ||
            manifest.ReplayAttestation is not { IsCanonical: true })
        {
            return Invalid();
        }

        var knnCorpus = GeneratedCapabilityKnnCorpusProjector.Project(
            manifest.KnnExamples);
        if (!knnCorpus.IsSuccess || knnCorpus.Value is null ||
            !RuntimeProvenance.IsSha256Hex(manifest.KnnCorpusSha256) ||
            !RuntimeProvenance.FixedTimeSha256Equals(
                manifest.KnnCorpusSha256!,
                knnCorpus.Value.Sha256))
        {
            return Invalid();
        }

        try
        {
            using var payload = new MemoryStream();
            WriteString(payload,
                "generated-capability-executable-manifest-v2");
            WriteInt32(payload, manifest.SchemaVersion.Value);

            WriteString(payload, manifest.UnitId);
            WriteString(payload, manifest.UnitRole);
            WriteString(payload, manifest.DomainTag);
            WriteString(payload, manifest.Version);
            WriteString(payload, manifest.TrustState);
            WriteString(payload, manifest.FlowId);
            WriteString(payload, manifest.ScenarioId);
            WriteString(payload, manifest.PhaseId);
            WriteString(payload, manifest.SubPhaseId);
            WriteString(payload, manifest.TaskType);

            WriteString(payload,
                TeachingRegistrySnapshotKinds.GeneratedCapabilityUnit);
            WriteString(payload,
                TeachingRegistrySnapshotKinds.GeneratedCapabilityFlow);
            WriteString(payload, "System.String");
            WriteBoolean(payload, value: false);
            WriteInt64(payload, manifest.UnitRegistrationOrder.Value);
            WriteInt64(payload, manifest.FlowRegistrationOrder.Value);

            WriteString(payload, manifest.TrainingText);
            WriteString(payload, manifest.OutputText);
            WriteString(payload, manifest.ExampleId);
            WriteStrings(payload, manifest.PredicateIds);
            WriteStrings(payload, manifest.DomainTags);
            WriteString(payload, manifest.OriginalPrompt);

            WriteInt32(payload, knnCorpus.Value.Examples.Count);
            foreach (var example in knnCorpus.Value.Examples)
            {
                WriteString(payload, example.ExampleId);
                WriteString(payload, example.TrainingText);
                WriteString(payload, example.OutputText);
                WriteStrings(payload, example.PredicateIds);
                WriteStrings(payload, example.DomainTags);
            }
            WriteString(payload, knnCorpus.Value.Sha256);

            WriteString(payload, manifest.PolicyCheckpointPath);
            WriteString(payload, manifest.TokenizerPath);
            WriteString(payload, exactTokenizerArtifactSha256!);
            WriteString(payload, exactPolicyArtifactSha256!);
            WriteString(payload, manifest.RuntimeProvenanceSha256!);

            WriteInt32(payload, manifest.VocabSize);
            WriteInt32(payload, manifest.ContextLen);
            WriteInt32(payload, manifest.HiddenDim);
            WriteInt32(payload, manifest.NumLayers);
            WriteInt32(payload, manifest.NumHeads);
            WriteInt32(payload, manifest.NumKvHeads);
            WriteInt32(payload, manifest.FfnHiddenDim);
            WriteNullableSingle(payload, manifest.RopeTheta);
            WriteNullableSingle(payload, manifest.RmsNormEps);
            WriteInt32(payload, manifest.LoraRank);
            WriteSingle(payload, manifest.LoraAlpha);
            WriteSingle(payload, manifest.LoraDropout);
            WriteStrings(payload, manifest.LoraTargetModules);

            WriteUtcTimestamp(payload, manifest.CreatedAtUtc);
            WriteString(payload, manifest.UnitFamilyId);
            WriteNullableString(payload, manifest.ParentUnitId);
            WriteNullableString(
                payload,
                manifest.ParentPolicyCheckpointPath);
            WriteNullableString(payload, manifest.ParentTokenizerPath);
            WriteInt32(payload, manifest.TrainingStepCount);

            WriteNullableString(payload, manifest.ServerBaseUrl);
            WriteNullableString(payload, manifest.ServerUnitEndpoint);
            WriteNullableString(payload, manifest.TrainingBranch);
            WriteNullableString(payload, manifest.TrainingRunId);
            WriteNullableString(payload, manifest.SourceDatasetSha256);
            WriteNullableUtcTimestamp(payload, manifest.UploadedAtUtc);

            var replay = manifest.ReplayAttestation;
            WriteInt32(payload, replay.SchemaVersion);
            WriteInt32(payload, (int)replay.OriginalSourceKind);
            WriteString(payload, replay.ReplaySealGroupSha256);
            WriteStrings(payload, replay.SourceBindings.Values);
            WriteString(payload, replay.ReplayPayloadSha256);
            WriteString(payload, replay.DpoPreferenceSha256);
            WriteString(payload, replay.AuthorityKeyRef);
            WriteString(payload, replay.MacSha256);

            var digest = SHA256.HashData(payload.GetBuffer().AsSpan(
                0,
                checked((int)payload.Length)));
            return OperationResult<string>.Ok(
                Convert.ToHexString(digest).ToLowerInvariant());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }
    }

    private static void WriteStrings(
        Stream destination,
        IReadOnlyList<string>? values)
    {
        if (values is null ||
            values.Count > MaximumArrayCount)
        {
            throw new InvalidDataException();
        }

        WriteInt32(destination, values.Count);
        for (var index = 0; index < values.Count; index++)
            WriteString(destination, values[index]);
    }

    private static void WriteNullableString(
        Stream destination,
        string? value)
    {
        WriteBoolean(destination, value is not null);
        if (value is not null)
            WriteString(destination, value);
    }

    private static void WriteString(Stream destination, string? value)
    {
        if (value is null ||
            value.Length > TeachingRegistryPersistence.MaximumManifestBytes)
        {
            throw new InvalidDataException();
        }

        var byteCount = Encoding.UTF8.GetByteCount(value);
        if (byteCount >
            TeachingRegistryPersistence.MaximumManifestBytes)
        {
            throw new InvalidDataException();
        }

        EnsureWritable(
            destination,
            checked(sizeof(int) + byteCount));
        WriteInt32Unchecked(destination, byteCount);
        var bytes = Encoding.UTF8.GetBytes(value);
        destination.Write(bytes, 0, bytes.Length);
    }

    private static void WriteNullableSingle(
        Stream destination,
        float? value)
    {
        WriteBoolean(destination, value is not null);
        if (value is not null)
            WriteSingle(destination, value.Value);
    }

    private static void WriteSingle(Stream destination, float value)
        => WriteInt32(destination, BitConverter.SingleToInt32Bits(value));

    private static void WriteNullableUtcTimestamp(
        Stream destination,
        DateTimeOffset? value)
    {
        WriteBoolean(destination, value is not null);
        if (value is not null)
            WriteUtcTimestamp(destination, value.Value);
    }

    private static void WriteUtcTimestamp(
        Stream destination,
        DateTimeOffset value)
        => WriteInt64(destination, value.UtcDateTime.Ticks);

    private static void WriteBoolean(Stream destination, bool value)
    {
        EnsureWritable(destination, sizeof(byte));
        destination.WriteByte(value ? (byte)1 : (byte)0);
    }

    private static void WriteInt32(Stream destination, int value)
    {
        EnsureWritable(destination, sizeof(int));
        WriteInt32Unchecked(destination, value);
    }

    private static void WriteInt32Unchecked(Stream destination, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        destination.Write(bytes);
    }

    private static void WriteInt64(Stream destination, long value)
    {
        EnsureWritable(destination, sizeof(long));
        WriteInt64Unchecked(destination, value);
    }

    private static void WriteInt64Unchecked(Stream destination, long value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(long)];
        BinaryPrimitives.WriteInt64BigEndian(bytes, value);
        destination.Write(bytes);
    }

    private static void EnsureWritable(Stream destination, int byteCount)
    {
        if (byteCount < 0)
            throw new InvalidDataException();

        long resultingLength;
        try
        {
            resultingLength = checked(destination.Length + byteCount);
        }
        catch (OverflowException)
        {
            throw new InvalidDataException();
        }

        if (resultingLength > MaximumCanonicalPayloadBytes)
            throw new InvalidDataException();
    }

    private static OperationResult<string> Invalid()
        => OperationResult<string>.Fail(
            "Generated capability executable manifest identity is invalid.",
            InvalidErrorCode);
}

internal sealed class GeneratedCapabilityKnnCorpusProjection
{
    internal GeneratedCapabilityKnnCorpusProjection(
        GeneratedCapabilityKnnExample[] examples,
        string sha256)
    {
        Examples = Array.AsReadOnly(examples);
        Sha256 = sha256;
    }

    internal IReadOnlyList<GeneratedCapabilityKnnExample> Examples { get; }
    internal string Sha256 { get; }
}

/// <summary>
/// Owns and hashes the exact ordered retrieval-memory corpus. This identity is
/// separate from the enclosing executable-manifest hash so restore can reject
/// a corpus mismatch before constructing a tokenizer, model, or unit.
/// </summary>
internal static class GeneratedCapabilityKnnCorpusProjector
{
    internal const int MaximumExampleCount = 259;
    internal const int MaximumTagCount = 256;
    internal const int MaximumAggregateTagCount = 26_000;
    internal const int MaximumIdentifierUtf8Bytes = 16_384;
    internal const int MaximumTextUtf8Bytes = 1_048_576;
    internal const int MaximumCanonicalPayloadBytes = 1_048_576;
    internal const string InvalidErrorCode =
        "GENERATED_CAPABILITY_KNN_CORPUS_INVALID";

    internal static OperationResult<GeneratedCapabilityKnnCorpusProjection>
        Project(IReadOnlyList<GeneratedCapabilityKnnExample>? examples)
    {
        if (examples is null ||
            examples.Count is <= 0 or > MaximumExampleCount)
        {
            return Invalid();
        }

        try
        {
            var owned = new GeneratedCapabilityKnnExample[examples.Count];
            var exampleIds = new HashSet<string>(StringComparer.Ordinal);
            var aggregateTagCount = 0;
            using var payload = new MemoryStream();
            WriteString(payload, "generated-capability-knn-corpus-v1");
            WriteInt32(payload, examples.Count);
            for (var index = 0; index < examples.Count; index++)
            {
                var source = examples[index];
                if (source is null ||
                    !IsBounded(
                        source.ExampleId,
                        MaximumIdentifierUtf8Bytes) ||
                    !IsBounded(
                        source.TrainingText,
                        MaximumTextUtf8Bytes) ||
                    !IsBounded(
                        source.OutputText,
                        MaximumTextUtf8Bytes) ||
                    !TrySnapshotTags(source.PredicateIds, out var predicates) ||
                    !TrySnapshotTags(source.DomainTags, out var domains) ||
                    !exampleIds.Add(source.ExampleId))
                {
                    return Invalid();
                }
                if (predicates.Length >
                        MaximumAggregateTagCount - aggregateTagCount)
                {
                    return Invalid();
                }
                aggregateTagCount += predicates.Length;
                if (domains.Length >
                        MaximumAggregateTagCount - aggregateTagCount)
                {
                    return Invalid();
                }
                aggregateTagCount += domains.Length;

                var captured = new GeneratedCapabilityKnnExample(
                    source.ExampleId,
                    source.TrainingText,
                    source.OutputText,
                    predicates,
                    domains);
                owned[index] = captured;
                WriteString(payload, captured.ExampleId);
                WriteString(payload, captured.TrainingText);
                WriteString(payload, captured.OutputText);
                WriteStrings(payload, captured.PredicateIds);
                WriteStrings(payload, captured.DomainTags);
            }

            var digest = SHA256.HashData(payload.GetBuffer().AsSpan(
                0,
                checked((int)payload.Length)));
            return OperationResult<GeneratedCapabilityKnnCorpusProjection>.Ok(
                new GeneratedCapabilityKnnCorpusProjection(
                    owned,
                    Convert.ToHexString(digest).ToLowerInvariant()));
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return Invalid();
        }
    }

    private static bool TrySnapshotTags(
        IReadOnlyList<string>? values,
        out string[] snapshot)
    {
        snapshot = null!;
        if (values is null ||
            values.Count is <= 0 or > MaximumTagCount)
        {
            return false;
        }

        snapshot = new string[values.Count];
        for (var index = 0; index < snapshot.Length; index++)
        {
            var value = values[index];
            if (!IsBounded(value, MaximumIdentifierUtf8Bytes))
                return false;
            snapshot[index] = value;
        }
        return true;
    }

    private static bool IsBounded(string? value, int maximumUtf8Bytes)
        => value is not null &&
           value.Length <= maximumUtf8Bytes &&
           Encoding.UTF8.GetByteCount(value) <= maximumUtf8Bytes;

    private static void WriteStrings(
        Stream destination,
        IReadOnlyList<string> values)
    {
        WriteInt32(destination, values.Count);
        for (var index = 0; index < values.Count; index++)
            WriteString(destination, values[index]);
    }

    private static void WriteString(Stream destination, string value)
    {
        var byteCount = Encoding.UTF8.GetByteCount(value);
        EnsureWritable(destination, checked(sizeof(int) + byteCount));
        WriteInt32Unchecked(destination, byteCount);
        var bytes = Encoding.UTF8.GetBytes(value);
        destination.Write(bytes, 0, bytes.Length);
    }

    private static void WriteInt32(Stream destination, int value)
    {
        EnsureWritable(destination, sizeof(int));
        WriteInt32Unchecked(destination, value);
    }

    private static void WriteInt32Unchecked(Stream destination, int value)
    {
        Span<byte> bytes = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(bytes, value);
        destination.Write(bytes);
    }

    private static void EnsureWritable(Stream destination, int byteCount)
    {
        if (byteCount < 0 ||
            destination.Length > MaximumCanonicalPayloadBytes - byteCount)
        {
            throw new InvalidDataException();
        }
    }

    private static OperationResult<GeneratedCapabilityKnnCorpusProjection>
        Invalid()
        => OperationResult<GeneratedCapabilityKnnCorpusProjection>.Fail(
            "Generated capability KNN corpus is invalid.",
            InvalidErrorCode);
}
