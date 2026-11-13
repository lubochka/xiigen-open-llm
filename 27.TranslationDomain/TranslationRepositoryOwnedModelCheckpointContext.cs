// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TranslationDomain;

using AgentInfrastructure;
using ContinuousLearning;

/// <summary>
/// Names the exact Translation-owned model identity under the common
/// repository checkpoint tree. The runtime still uses the common address type
/// for a concrete step; this wrapper keeps Translation from accepting a caller
/// supplied directory as authority.
/// </summary>
public sealed record TranslationRepositoryOwnedModelCheckpointAddress(
    string RepositoryRoot,
    string DomainSlug,
    string DesignPatternSlug,
    string ModelIdSlug,
    string ModelVersion)
{
    public OperationResult<RepositoryOwnedModelCheckpointAddress>
        CreateStepAddress(int step)
        => RepositoryOwnedTrainingArtifactLayout.CreateModelCheckpointAddress(
            RepositoryRoot,
            DomainSlug,
            DesignPatternSlug,
            ModelIdSlug,
            ModelVersion,
            step);

    public OperationResult<bool> Validate()
    {
        var probe = CreateStepAddress(step: 1);
        if (!probe.IsSuccess || probe.Value is null)
        {
            return OperationResult<bool>.Fail(
                TranslationRepositoryOwnedModelCheckpointContext
                    .InvalidSafeMessage,
                TranslationRepositoryOwnedModelCheckpointContext
                    .InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }
}

/// <summary>
/// Carries the model checkpoint request policy for one Translation DPO run:
/// compatibility must match before loading, and cold start must be explicit.
/// </summary>
public sealed record TranslationRepositoryOwnedModelCheckpointRequest(
    TranslationRepositoryOwnedModelCheckpointAddress Address,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    bool AllowColdStart,
    string? ColdStartReason,
    IReadOnlyList<string> ProvenanceRefs)
{
    public OperationResult<bool> Validate()
    {
        if (Address is null ||
            RuntimeCompatibility is null ||
            ProvenanceRefs is null ||
            !RuntimeCompatibilityIsWellFormed(RuntimeCompatibility) ||
            !ColdStartPolicyIsWellFormed(AllowColdStart, ColdStartReason) ||
            !ProvenanceRefsAreWellFormed(ProvenanceRefs))
        {
            return OperationResult<bool>.Fail(
                TranslationRepositoryOwnedModelCheckpointContext
                    .InvalidSafeMessage,
                TranslationRepositoryOwnedModelCheckpointContext
                    .InvalidErrorCode);
        }

        return Address.Validate();
    }

    private static bool RuntimeCompatibilityIsWellFormed(
        RepositoryOwnedModelRuntimeCompatibility compatibility)
        => IsCanonicalSha256Hex(compatibility.ConfigSha256) &&
           IsCanonicalSha256Hex(compatibility.TopologySha256) &&
           IsCanonicalSha256Hex(compatibility.TokenizerSha256) &&
           IsCanonicalSha256Hex(compatibility.TrainerSha256) &&
           IsCanonicalSha256Hex(compatibility.SchemaSha256) &&
           IsSafeCompatibilityKey(compatibility.CompatibilityKey);

    private static bool ColdStartPolicyIsWellFormed(
        bool allowColdStart,
        string? coldStartReason)
        => allowColdStart
            ? IsBoundedHumanText(coldStartReason)
            : string.IsNullOrEmpty(coldStartReason);

    private static bool ProvenanceRefsAreWellFormed(
        IReadOnlyList<string> provenanceRefs)
        => provenanceRefs.Count is > 0 and <= 24 &&
           provenanceRefs.Distinct(StringComparer.Ordinal).Count() ==
           provenanceRefs.Count &&
           provenanceRefs.All(IsBoundedHumanText);

    private static bool IsCanonicalSha256Hex(string? value)
        => value is { Length: 64 } &&
           value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static bool IsSafeCompatibilityKey(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 256 &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           value.All(static character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character is '-' or '_' or '.');

    private static bool IsBoundedHumanText(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= 256 &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           !Path.IsPathRooted(value) &&
           !value.Contains('\\') &&
           !value.Contains("..", StringComparison.Ordinal) &&
           value.All(static character => !char.IsControl(character));
}

/// <summary>
/// Optional Translation DPO model checkpoint authority. Disabled preserves the
/// legacy runner path; enabled means the repository tree is the only model
/// continuation/publish authority for the run.
/// </summary>
public sealed record TranslationRepositoryOwnedModelCheckpointContext(
    bool Enabled,
    TranslationRepositoryOwnedModelCheckpointRequest? Request)
{
    public static TranslationRepositoryOwnedModelCheckpointContext Disabled
    { get; } = new(Enabled: false, Request: null);

    public const string InvalidErrorCode =
        "TRANSLATION_REPOSITORY_OWNED_MODEL_CHECKPOINT_CONTEXT_INVALID";

    public const string InvalidSafeMessage =
        "Translation repository-owned model checkpoint context is required and must point to a canonical model identity, runtime compatibility, provenance refs, and explicit cold-start policy.";

    public OperationResult<bool> Validate()
    {
        if (!Enabled)
        {
            return Request is null
                ? OperationResult<bool>.Ok(true)
                : OperationResult<bool>.Fail(
                    InvalidSafeMessage,
                    InvalidErrorCode);
        }

        return Request is null
            ? OperationResult<bool>.Fail(InvalidSafeMessage, InvalidErrorCode)
            : Request.Validate();
    }
}
