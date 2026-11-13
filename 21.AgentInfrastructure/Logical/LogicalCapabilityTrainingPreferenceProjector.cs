// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using System.Buffers.Binary;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using AgentInfrastructure;
using SemanticCompilerAdapter;

public sealed class LogicalCapabilityTrainingCurrentProjection
{
    private readonly float[] _teachingVector;

    internal LogicalCapabilityTrainingCurrentProjection(
        string prompt,
        string chosen,
        string rejected,
        float[] teachingVector,
        string dpoPreferenceSha256)
    {
        Prompt = prompt;
        Chosen = chosen;
        Rejected = rejected;
        _teachingVector = teachingVector.ToArray();
        DpoPreferenceSha256 = dpoPreferenceSha256;
    }

    public string Prompt { get; }
    public string Chosen { get; }
    public string Rejected { get; }
    public float[] TeachingVector => _teachingVector.ToArray();
    public string DpoPreferenceSha256 { get; }
}

public sealed class LogicalCapabilityTrainingReplayProjection
{
    private readonly float[] _teachingVector;

    internal LogicalCapabilityTrainingReplayProjection(
        LogicalCapabilityTrainingReplayDescriptor descriptor,
        string trainingText,
        string outputText,
        string rejectedText,
        string exampleId,
        float[] teachingVector)
    {
        Descriptor = descriptor with { };
        TrainingText = trainingText;
        OutputText = outputText;
        RejectedText = rejectedText;
        ExampleId = exampleId;
        _teachingVector = teachingVector.ToArray();
    }

    public LogicalCapabilityTrainingReplayDescriptor Descriptor { get; }
    public string TrainingText { get; }
    public string OutputText { get; }
    public string RejectedText { get; }
    public string ExampleId { get; }
    public float[] TeachingVector => _teachingVector.ToArray();
}

/// <summary>
/// The single deterministic projection for current and durable replay DPO
/// content. Projection is public and reproducible; only the trusted Core issuer
/// can turn its digests into authority.
/// </summary>
public static class LogicalCapabilityTrainingPreferenceProjector
{
    private const int MaximumTextUtf8Bytes = 1_048_576;
    private const int MaximumIdentifierUtf8Bytes = 4_096;
    private const int MaximumOrderedValueCount = 4_096;
    private const int MaximumVectorLength = 65_536;
    private const int MaximumCurrentProjectionCount = 256;

    public static OperationResult<LogicalCapabilityTrainingCurrentProjection>
        ProjectCurrent(
            string? originalUserRequest,
            LogicalFlowResult? branch,
            LogicalSubPhase? subPhase)
    {
        try
        {
            if (!IsBounded(originalUserRequest, MaximumTextUtf8Bytes) ||
                branch is null || subPhase is null ||
                !IsBounded(branch.FinalHumanResult, MaximumTextUtf8Bytes) ||
                !IsBounded(subPhase.SubPhaseId, MaximumIdentifierUtf8Bytes) ||
                !IsBounded(subPhase.Name, MaximumIdentifierUtf8Bytes) ||
                !IsBounded(subPhase.Purpose, MaximumIdentifierUtf8Bytes) ||
                !TrySnapshotOrdered(
                    subPhase.PredicateIds,
                    out var predicateIds))
            {
                return InvalidCurrent();
            }

            var compiled = new SemanticTeachingPromptCompiler().Compile(
                originalUserRequest!);
            var promptSide = compiled.IsSuccess && compiled.Value is not null
                ? compiled.Value.CanonicalText.Trim()
                : originalUserRequest!.Trim();
            var sourceVector = compiled.IsSuccess && compiled.Value is not null
                ? compiled.Value.TeachingVector
                : TrainingVectorBuilder.Build(promptSide);
            var vector = sourceVector?.Take(MaximumVectorLength + 1).ToArray();
            if (!IsBounded(promptSide, MaximumTextUtf8Bytes) ||
                vector is null || vector.Length is 0 or > MaximumVectorLength ||
                vector.Any(static value => !float.IsFinite(value)))
            {
                return InvalidCurrent();
            }

            var raw = string.Join(" ",
                "semantic_prompt:",
                promptSide,
                "branch_result:",
                branch.FinalHumanResult,
                "sub_phase:",
                subPhase.SubPhaseId,
                subPhase.Name,
                subPhase.Purpose,
                string.Join(" ", predicateIds));
            var vectorText = string.Join(",", vector.Select(value =>
                value.ToString("0.###", CultureInfo.InvariantCulture)));
            var prompt =
                $"{raw} semantic_compressed: {promptSide} teaching_vector_60: {vectorText}"
                    .Trim();
            var chosen = BuildChosen(subPhase);
            var rejected = BuildRejected(subPhase.SubPhaseId);
            if (!IsBounded(prompt, MaximumTextUtf8Bytes) ||
                !IsBounded(chosen, MaximumTextUtf8Bytes) ||
                !IsBounded(rejected, MaximumTextUtf8Bytes))
            {
                return InvalidCurrent();
            }

            var digest = ComputeDpoPreferenceSha256(
                prompt,
                chosen,
                rejected,
                vector);
            return digest.IsSuccess && digest.Value is not null
                ? OperationResult<LogicalCapabilityTrainingCurrentProjection>
                    .Ok(new(
                        prompt,
                        chosen,
                        rejected,
                        vector,
                        digest.Value))
                : digest.MapError<
                    LogicalCapabilityTrainingCurrentProjection>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidCurrent();
        }
    }

    public static OperationResult<LogicalCapabilityTrainingReplayProjection>
        ProjectAuthorizedReplay(
            IReadOnlyList<LogicalCapabilityTrainingCurrentProjection>?
                currentProjections,
            IReadOnlyList<string>? sourceBindingSha256s,
            string? replaySealGroupSha256,
            string? domainTag,
            LogicalSubPhase? subPhase)
    {
        try
        {
            var currents = currentProjections?
                .Take(MaximumCurrentProjectionCount + 1)
                .ToArray() ?? [];
            if (currents.Length is 0 or > MaximumCurrentProjectionCount ||
                currents.Any(static value => value is null) ||
                subPhase is null)
            {
                return InvalidReplay();
            }

            var texts = currents
                .Select(static value => value.Prompt)
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            long characters = Math.Max(0, texts.Length - 1);
            long bytes = Math.Max(0, texts.Length - 1);
            foreach (var text in texts)
            {
                characters += text.Length;
                bytes += Encoding.UTF8.GetByteCount(text);
                if (characters > MaximumTextUtf8Bytes ||
                    bytes > MaximumTextUtf8Bytes)
                {
                    return InvalidReplay();
                }
            }
            var trainingText = string.Join(" ", texts);
            return ProjectReplayPayload(
                sourceBindingSha256s,
                replaySealGroupSha256,
                domainTag,
                subPhase.SubPhaseId,
                $"capability-row-{Sanitize(subPhase.SubPhaseId)}-{replaySealGroupSha256}",
                trainingText,
                BuildChosen(subPhase),
                subPhase.PredicateIds,
                subPhase.DomainIds);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidReplay();
        }
    }

    public static OperationResult<LogicalCapabilityTrainingReplayProjection>
        ProjectReplayPayload(
            IReadOnlyList<string>? sourceBindingSha256s,
            string? replaySealGroupSha256,
            string? domainTag,
            string? subPhaseId,
            string? exampleId,
            string? trainingText,
            string? outputText,
            IReadOnlyList<string>? predicateIds,
            IReadOnlyList<string>? domainTags)
    {
        try
        {
            var sourceBindings =
                new LogicalCapabilityTrainingSourceBindingComposite(
                    sourceBindingSha256s);
            if (!sourceBindings.IsCanonical ||
                !LogicalCapabilityTrainingReplayDescriptor.IsCanonicalSha256(
                    replaySealGroupSha256) ||
                !IsBounded(domainTag, MaximumIdentifierUtf8Bytes) ||
                !IsBounded(subPhaseId, MaximumIdentifierUtf8Bytes) ||
                !IsBounded(exampleId, MaximumIdentifierUtf8Bytes) ||
                !IsBounded(trainingText, MaximumTextUtf8Bytes) ||
                !IsBounded(outputText, MaximumTextUtf8Bytes) ||
                !TrySnapshotOrdered(predicateIds, out var predicates) ||
                !TrySnapshotOrdered(domainTags, out var domains))
            {
                return InvalidReplay();
            }

            var rejectedText = BuildRejected(subPhaseId!);
            var vector = TrainingVectorBuilder.Build(trainingText!);
            if (!IsBounded(rejectedText, MaximumTextUtf8Bytes) ||
                vector is null || vector.Length is 0 or > MaximumVectorLength ||
                vector.Any(static value => !float.IsFinite(value)))
            {
                return InvalidReplay();
            }

            using var payload = new MemoryStream();
            WriteString(payload, "logical-capability-replay-payload-v2");
            WriteString(payload, replaySealGroupSha256!);
            WriteInt32(payload, sourceBindings.Values.Count);
            foreach (var binding in sourceBindings.Values)
                WriteString(payload, binding);
            WriteString(payload, domainTag!);
            WriteString(payload, subPhaseId!);
            WriteString(payload, exampleId!);
            WriteString(payload, trainingText!);
            WriteString(payload, outputText!);
            WriteString(payload, rejectedText);
            WriteInt32(payload, vector.Length);
            foreach (var value in vector)
                WriteInt32(payload, BitConverter.SingleToInt32Bits(value));
            WriteStrings(payload, predicates);
            WriteStrings(payload, domains);

            var replayPayloadSha256 = Convert.ToHexString(
                SHA256.HashData(payload.ToArray())).ToLowerInvariant();
            var dpoPreferenceSha256 = ComputeDpoPreferenceSha256(
                trainingText,
                outputText,
                rejectedText,
                vector);
            if (!dpoPreferenceSha256.IsSuccess ||
                dpoPreferenceSha256.Value is null)
            {
                return dpoPreferenceSha256
                    .MapError<LogicalCapabilityTrainingReplayProjection>();
            }
            var descriptor = new LogicalCapabilityTrainingReplayDescriptor(
                LogicalCapabilityTrainingReplayDescriptor.CurrentSchemaVersion,
                LogicalTrainingContentSourceKind
                    .DeterministicSyntheticNoExternalTeacher,
                replaySealGroupSha256!,
                sourceBindings,
                replayPayloadSha256,
                dpoPreferenceSha256.Value);
            return descriptor.IsCanonical
                ? OperationResult<LogicalCapabilityTrainingReplayProjection>
                    .Ok(new(
                        descriptor,
                        trainingText!,
                        outputText!,
                        rejectedText,
                        exampleId!,
                        vector))
                : InvalidReplay();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidReplay();
        }
    }

    public static OperationResult<string> ComputeDpoPreferenceSha256(
        string? prompt,
        string? chosen,
        string? rejected,
        IReadOnlyList<float>? teachingVector)
    {
        try
        {
            var vector = teachingVector?.Take(MaximumVectorLength + 1)
                .ToArray();
            if (!IsBounded(prompt, MaximumTextUtf8Bytes) ||
                !IsBounded(chosen, MaximumTextUtf8Bytes) ||
                !IsBounded(rejected, MaximumTextUtf8Bytes) ||
                vector is null || vector.Length is 0 or > MaximumVectorLength ||
                vector.Any(static value => !float.IsFinite(value)))
            {
                return InvalidDigest();
            }

            using var payload = new MemoryStream();
            WriteString(payload, "logical-capability-dpo-preference-v1");
            WriteString(payload, prompt!);
            WriteString(payload, chosen!);
            WriteString(payload, rejected!);
            WriteInt32(payload, vector.Length);
            foreach (var value in vector)
                WriteInt32(payload, BitConverter.SingleToInt32Bits(value));
            return OperationResult<string>.Ok(Convert.ToHexString(
                SHA256.HashData(payload.ToArray())).ToLowerInvariant());
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidDigest();
        }
    }

    private static string BuildChosen(LogicalSubPhase subPhase)
        => $"Generated capability for sub-phase {subPhase.SubPhaseId}: {subPhase.Name}. Execute the requested work through task type {Sanitize(subPhase.Name)}.";

    private static string BuildRejected(string subPhaseId)
        => $"Do not generate a capability for sub-phase {subPhaseId}.";

    private static string Sanitize(string value)
    {
        var safeValue = string.IsNullOrWhiteSpace(value)
            ? "unknown"
            : value.Trim();
        var characters = safeValue.Select(character =>
                char.IsLetterOrDigit(character)
                    ? char.ToLowerInvariant(character)
                    : '-')
            .ToArray();
        return string.Join("-", new string(characters).Split(
            '-',
            StringSplitOptions.RemoveEmptyEntries));
    }

    private static bool TrySnapshotOrdered(
        IReadOnlyList<string>? values,
        out string[] snapshot)
    {
        snapshot = [];
        if (values is null)
            return false;
        snapshot = values.Take(MaximumOrderedValueCount + 1).ToArray();
        return snapshot.Length <= MaximumOrderedValueCount &&
               snapshot.All(value =>
                   IsBounded(value, MaximumIdentifierUtf8Bytes));
    }

    private static bool IsBounded(string? value, int maximumUtf8Bytes)
        => value is not null &&
           Encoding.UTF8.GetByteCount(value) <= maximumUtf8Bytes;

    private static void WriteStrings(Stream destination, string[] values)
    {
        WriteInt32(destination, values.Length);
        foreach (var value in values)
            WriteString(destination, value);
    }

    private static void WriteString(Stream destination, string value)
    {
        var encoded = Encoding.UTF8.GetBytes(value);
        WriteInt32(destination, encoded.Length);
        destination.Write(encoded, 0, encoded.Length);
    }

    private static void WriteInt32(Stream destination, int value)
    {
        Span<byte> encoded = stackalloc byte[sizeof(int)];
        BinaryPrimitives.WriteInt32BigEndian(encoded, value);
        destination.Write(encoded);
    }

    private static OperationResult<LogicalCapabilityTrainingCurrentProjection>
        InvalidCurrent()
        => OperationResult<LogicalCapabilityTrainingCurrentProjection>.Fail(
            "Logical capability current projection is invalid.",
            "LOGICAL_CAPABILITY_TRAINING_PROJECTION_INVALID");

    private static OperationResult<LogicalCapabilityTrainingReplayProjection>
        InvalidReplay()
        => OperationResult<LogicalCapabilityTrainingReplayProjection>.Fail(
            "Logical capability replay payload is invalid.",
            "LOGICAL_CAPABILITY_TRAINING_REPLAY_INVALID");

    private static OperationResult<string> InvalidDigest()
        => OperationResult<string>.Fail(
            "Logical capability DPO preference is invalid.",
            "LOGICAL_CAPABILITY_TRAINING_REPLAY_INVALID");
}
