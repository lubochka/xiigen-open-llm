// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearning;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;

/// <summary>Descriptive startup pins; no path, checkpoint ID or effect capability.</summary>
internal sealed record CodeDocumentationOneLessonParentCheckpointV2(
    int Step, string DomainSlug, string DesignPatternSlug, string ModelIdSlug,
    string ModelVersion, string ManifestSha256, string WeightsSha256, long WeightsByteLength);

internal sealed record CodeDocumentationOneLessonCandidateSelectionV2(
    string CandidateId, string CandidateSha256);

/// <summary>
/// Pure immutable full parent/candidate binding. Fresh readback uses the existing
/// latest-compatible loader and typed sealer; snapshots retain no weight stream.
/// </summary>
internal sealed class CodeDocumentationOneLessonCheckpointSelection
{
    internal const string InvalidErrorCode = "ONE_LESSON_CHECKPOINT_SELECTION_INVALID";
    internal CodeDocumentationOneLessonParentCheckpointV2 Parent { get; }
    internal CodeDocumentationOneLessonCandidateSelectionV2 Candidate { get; }
    internal string ParentCheckpointSha256 { get; }
    internal string CandidateSelectionSha256 { get; }

    private CodeDocumentationOneLessonCheckpointSelection(
        CodeDocumentationOneLessonParentCheckpointV2 parent,
        CodeDocumentationOneLessonCandidateSelectionV2 candidate)
    {
        Parent = parent;
        Candidate = candidate;
        ParentCheckpointSha256 = Commitment("xiigen-s04-parentcheckpoint-v2\n", parent);
        CandidateSelectionSha256 = Commitment("xiigen-s04-candidateselection-v2\n", candidate);
    }

    internal static OperationResult<CodeDocumentationOneLessonCheckpointSelection> Create(
        CodeDocumentationOneLessonParentCheckpointV2? parent,
        CodeDocumentationOneLessonCandidateSelectionV2? candidate)
    {
        if (parent is null || candidate is null || parent.Step <= 0 ||
            !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(parent.DomainSlug) ||
            !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(parent.DesignPatternSlug) ||
            !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(parent.ModelIdSlug) ||
            !RepositoryOwnedTrainingArtifactLayout.ValidateSlug(parent.ModelVersion) ||
            !CodeDocumentationOneLessonV2Wire.Hash(parent.ManifestSha256) ||
            !CodeDocumentationOneLessonV2Wire.Hash(parent.WeightsSha256) ||
            parent.WeightsByteLength < 1 ||
            parent.WeightsByteLength > RepositoryOwnedModelCheckpointManifestSealer.MaximumWeightBytes ||
            !Token(candidate.CandidateId) ||
            !CodeDocumentationOneLessonV2Wire.Hash(candidate.CandidateSha256))
            return OperationResult<CodeDocumentationOneLessonCheckpointSelection>.Fail(
                "The complete startup checkpoint selection is invalid.", InvalidErrorCode);

        // WHY: every member is an immutable primitive, and admission owns new records.
        return OperationResult<CodeDocumentationOneLessonCheckpointSelection>.Ok(
            new(parent with { }, candidate with { }));
    }

    internal OperationResult<CodeDocumentationOneLessonCheckpointSelectionOpenResult> OpenFresh(
        string repositoryRoot, RepositoryOwnedModelRuntimeCompatibility serverCompatibility)
    {
        var opened = new RepositoryOwnedModelCheckpointLoader().LoadLatestCompatible(
            repositoryRoot, Parent.DomainSlug, Parent.DesignPatternSlug,
            Parent.ModelIdSlug, Parent.ModelVersion, serverCompatibility);
        if (!opened.IsSuccess || opened.Value is null)
            return opened.MapError<CodeDocumentationOneLessonCheckpointSelectionOpenResult>();

        // The existing result owns its weight stream. Every later return or
        // exception leaves this scope through Dispose, including pin mismatch.
        using var loaded = opened.Value;
        var loadedParent = Readback(loaded.Manifest);
        if (loadedParent != Parent || loaded.WeightsSha256 != Parent.WeightsSha256 ||
            loaded.WeightsByteLength != Parent.WeightsByteLength)
            return InvalidOpen();

        var address = RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
            repositoryRoot, loadedParent.DomainSlug, loadedParent.DesignPatternSlug,
            loadedParent.ModelIdSlug, loadedParent.ModelVersion, loadedParent.Step);
        if (!address.IsSuccess || address.Value is null)
            return address.MapError<CodeDocumentationOneLessonCheckpointSelectionOpenResult>();
        var sealedReadback = RepositoryOwnedModelCheckpointManifestSealer.OpenSealed(address.Value);
        if (!sealedReadback.IsSuccess || sealedReadback.Value is null)
            return sealedReadback.MapError<CodeDocumentationOneLessonCheckpointSelectionOpenResult>();
        var observedParent = Readback(sealedReadback.Value.Manifest);
        if (observedParent != loadedParent || observedParent != Parent ||
            sealedReadback.Value.WeightsSha256 != loaded.WeightsSha256 ||
            sealedReadback.Value.WeightsByteLength != loaded.WeightsByteLength)
            return InvalidOpen();

        var frozen = Create(observedParent, Candidate);
        if (!frozen.IsSuccess || frozen.Value is null ||
            frozen.Value.ParentCheckpointSha256 != ParentCheckpointSha256 ||
            frozen.Value.CandidateSelectionSha256 != CandidateSelectionSha256)
            return InvalidOpen();

        // ID is derived only here, from successful owner readback. Neither pure
        // startup tuples nor catalog commitments claim a checkpoint capability.
        var checkpointId = Commitment("xiigen-s04-checkpoint-id-v2\n", frozen.Value.Parent);
        return OperationResult<CodeDocumentationOneLessonCheckpointSelectionOpenResult>.Ok(
            new(frozen.Value, checkpointId));
    }

    private static CodeDocumentationOneLessonParentCheckpointV2 Readback(
        RepositoryOwnedModelCheckpointManifest manifest)
        => new(manifest.Step, manifest.Domain, manifest.DesignPattern, manifest.ModelId,
            manifest.Version, manifest.ManifestSha256, manifest.WeightsSha256, manifest.WeightsByteLength);

    private static string Commitment<T>(string domain, T value)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes(domain));
        hash.AppendData(CodeDocumentationOneLessonV2Wire.CanonicalBytes(JsonSerializer.SerializeToElement(value)));
        return Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant();
    }

    private static bool Token(string? value)
        => value is { Length: >= 1 and <= 128 } &&
           (value[0] is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9') &&
           value.All(c => c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9'
               or '.' or '_' or ':' or '-');

    private static OperationResult<CodeDocumentationOneLessonCheckpointSelectionOpenResult> InvalidOpen()
        => OperationResult<CodeDocumentationOneLessonCheckpointSelectionOpenResult>.Fail(
            "Fresh checkpoint owner readback does not match the complete startup selection.", InvalidErrorCode);
}

/// <summary>Descriptive owner-readback snapshot, not a lease or publication receipt.</summary>
internal sealed record CodeDocumentationOneLessonCheckpointSelectionOpenResult(
    CodeDocumentationOneLessonCheckpointSelection Selection, string CheckpointId);
