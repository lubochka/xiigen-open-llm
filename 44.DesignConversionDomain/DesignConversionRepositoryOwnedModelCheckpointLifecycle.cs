// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DesignConversionDomain;

using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

public sealed record DesignConversionRepositoryOwnedModelCheckpointRequest(
    string RepositoryRoot,
    string DomainSlug,
    string DesignPatternSlug,
    string ModelIdSlug,
    string ModelVersion,
    RepositoryOwnedModelRuntimeCompatibility RuntimeCompatibility,
    bool AllowColdStart = false,
    string? ColdStartReason = null,
    IReadOnlyList<string>? ProvenanceRefs = null);

/// <summary>
/// Optional live-run policy for DesignConversion model checkpoints. Disabled
/// keeps legacy tests and local experiments unchanged; enabled means the run
/// must resume from the repository model tree or use an explicit cold-start
/// reason before it can publish another checkpoint.
/// </summary>
public sealed record DesignConversionRepositoryOwnedModelCheckpointContext(
    bool Enabled,
    DesignConversionRepositoryOwnedModelCheckpointRequest? Request)
{
    public static DesignConversionRepositoryOwnedModelCheckpointContext
        Disabled { get; } = new(Enabled: false, Request: null);

    public const string InvalidErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_CONTEXT_INVALID";

    public const string InvalidSafeMessage =
        "DesignConversion repository-owned model checkpoint context is required and must name the canonical repository model identity, compatibility fingerprints, provenance refs, and explicit cold-start policy.";

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

        return DesignConversionRepositoryOwnedModelCheckpointLifecycle
            .ValidateContextRequest(Request);
    }
}

public sealed record DesignConversionRepositoryOwnedModelCheckpointStart
{
    internal DesignConversionRepositoryOwnedModelCheckpointStart(
        DesignConversionRepositoryOwnedModelCheckpointRequest Target,
        DesignConversionMachine Machine,
        RepositoryOwnedModelCheckpointManifest? ParentManifest,
        string? ColdStartReason,
        IReadOnlyList<string> EvidenceRefs,
        IReadOnlyList<string> Diagnostics)
    {
        ArgumentNullException.ThrowIfNull(Target);
        ArgumentNullException.ThrowIfNull(Machine);

        this.Target = SnapshotTarget(Target);
        this.Machine = Machine;
        this.ParentManifest = ParentManifest;
        this.ColdStartReason = ColdStartReason;
        this.EvidenceRefs = EvidenceRefs?.ToArray() ?? [];
        this.Diagnostics = Diagnostics?.ToArray() ?? [];
    }

    // WHY: the start object is the proof that publication followed the
    // disk-first repository check. External callers may receive it from
    // PrepareStartAsync, but they cannot manufacture a fake cold-start proof.
    public DesignConversionRepositoryOwnedModelCheckpointRequest Target { get; }
    public DesignConversionMachine Machine { get; }
    public RepositoryOwnedModelCheckpointManifest? ParentManifest { get; }
    public string? ColdStartReason { get; }
    public IReadOnlyList<string> EvidenceRefs { get; }
    public IReadOnlyList<string> Diagnostics { get; }

    private static DesignConversionRepositoryOwnedModelCheckpointRequest
        SnapshotTarget(DesignConversionRepositoryOwnedModelCheckpointRequest target)
        => target with
        {
            RuntimeCompatibility = new RepositoryOwnedModelRuntimeCompatibility(
                target.RuntimeCompatibility.ConfigSha256,
                target.RuntimeCompatibility.TopologySha256,
                target.RuntimeCompatibility.TokenizerSha256,
                target.RuntimeCompatibility.TrainerSha256,
                target.RuntimeCompatibility.SchemaSha256,
                target.RuntimeCompatibility.CompatibilityKey),
            ProvenanceRefs = target.ProvenanceRefs?.ToArray()
        };
}

public sealed record DesignConversionRepositoryOwnedModelCheckpointPipelineEvidence(
    string StartStatus,
    string? ParentManifestRelativePath,
    string PublishedManifestRelativePath,
    string PublishedWeightsRelativePath,
    int PublishedStep,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Diagnostics);

public sealed record DesignConversionRepositoryOwnedModelCheckpointLoadReport(
    DesignConversionMachine Machine,
    RepositoryOwnedModelCheckpointManifest Manifest,
    string WeightsSha256,
    long WeightsByteLength,
    IReadOnlyList<string> EvidenceRefs,
    IReadOnlyList<string> Diagnostics);

public sealed record DesignConversionRepositoryOwnedModelCheckpointPublicationRequest(
    DesignConversionRepositoryOwnedModelCheckpointRequest Target,
    DesignConversionRepositoryOwnedModelCheckpointStart PreparedStart,
    DesignConversionMachine Machine,
    RepositoryOwnedDpoDatasetManifestIdentity DpoDatasetManifest,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<string> ProvenanceRefs,
    RepositoryOwnedModelCheckpointManifest? ParentCheckpoint = null,
    string? ColdStartReason = null);

public sealed record DesignConversionRepositoryOwnedModelCheckpointPublicationReport(
    RepositoryOwnedModelCheckpointManifest Manifest,
    string WeightsRelativePath,
    string ManifestRelativePath,
    IReadOnlyList<string> EvidenceRefs);

/// <summary>
/// Connects DesignConversionMachine to the repository-owned model tree. The
/// class is deliberately small: it does not train by itself, it only loads the
/// latest compatible saved weights and publishes the next checkpoint after a
/// separate admitted training step has already changed the machine.
/// </summary>
public static class DesignConversionRepositoryOwnedModelCheckpointLifecycle
{
    public const string LoadedFromRepositoryEvidence =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_LOADED";
    public const string ImportedIntoMachineEvidence =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_IMPORTED_INTO_MACHINE";
    public const string PublishedToRepositoryEvidence =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PUBLISHED";
    public const string ContinuedFromParentEvidence =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_CONTINUED_FROM_PARENT";
    public const string ColdStartPublishedEvidence =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_COLD_START_PUBLISHED";
    public const string InvalidErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_INVALID";
    public const string CheckpointPayloadInvalidErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_PAYLOAD_INVALID";
    public const string ColdStartNotAuthorizedErrorCode =
        "DESIGN_CONVERSION_REPOSITORY_OWNED_MODEL_CHECKPOINT_COLD_START_NOT_AUTHORIZED";

    public static async Task<OperationResult<
        DesignConversionRepositoryOwnedModelCheckpointStart?>>
        PrepareStartAsync(
            DesignConversionRepositoryOwnedModelCheckpointContext context,
            CancellationToken ct = default)
    {
        var validation = context.Validate();
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                DesignConversionRepositoryOwnedModelCheckpointStart?>();
        }

        if (!context.Enabled)
        {
            return OperationResult<
                DesignConversionRepositoryOwnedModelCheckpointStart?>.Ok(
                null);
        }

        var request = context.Request!;
        var loaded = await LoadLatestCompatibleAsync(
            request,
            ct).ConfigureAwait(false);
        if (loaded.IsSuccess && loaded.Value is not null)
        {
            return OperationResult<
                DesignConversionRepositoryOwnedModelCheckpointStart?>.Ok(
                new DesignConversionRepositoryOwnedModelCheckpointStart(
                    request,
                    loaded.Value.Machine,
                    loaded.Value.Manifest,
                    ColdStartReason: null,
                    [
                        LoadedFromRepositoryEvidence,
                        ImportedIntoMachineEvidence
                    ],
                    loaded.Value.Diagnostics));
        }

        if (IsCleanMissingRepositoryOwnedModelCheckpoint(loaded) &&
            request.AllowColdStart)
        {
            // Cold start is still repository-owned: the next successful
            // training mutation must publish step 000001 under
            // training-artifacts/models, not under a caller temp directory.
            return OperationResult<
                DesignConversionRepositoryOwnedModelCheckpointStart?>.Ok(
                new DesignConversionRepositoryOwnedModelCheckpointStart(
                    request,
                    new DesignConversionMachine(),
                    ParentManifest: null,
                    request.ColdStartReason,
                    [ColdStartPublishedEvidence],
                    [
                        string.IsNullOrWhiteSpace(loaded.ErrorMessage)
                            ? "No sealed DesignConversion checkpoint exists for this exact repository-owned model identity."
                            : loaded.ErrorMessage
                    ]));
        }

        if (IsCleanMissingRepositoryOwnedModelCheckpoint(loaded))
        {
            return OperationResult<
                DesignConversionRepositoryOwnedModelCheckpointStart?>.Fail(
                "DesignConversion repository-owned model checkpoint cold start is denied: no sealed parent exists for this exact model identity and no explicit human cold-start reason was supplied.",
                ColdStartNotAuthorizedErrorCode);
        }

        return loaded.MapError<
            DesignConversionRepositoryOwnedModelCheckpointStart?>();
    }

    public static async Task<OperationResult<
        DesignConversionRepositoryOwnedModelCheckpointLoadReport>>
        LoadLatestCompatibleAsync(
            DesignConversionRepositoryOwnedModelCheckpointRequest request,
            CancellationToken ct = default)
    {
        var validation = ValidateTarget(request);
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                DesignConversionRepositoryOwnedModelCheckpointLoadReport>();
        }

        var loaded = new RepositoryOwnedModelCheckpointLoader()
            .LoadLatestCompatible(
                request.RepositoryRoot,
                request.DomainSlug,
                request.DesignPatternSlug,
                request.ModelIdSlug,
                request.ModelVersion,
                request.RuntimeCompatibility);
        if (!loaded.IsSuccess || loaded.Value is null)
        {
            return loaded.MapError<
                DesignConversionRepositoryOwnedModelCheckpointLoadReport>();
        }

        using var loadedCheckpoint = loaded.Value;
        var checkpoint = await ReadCheckpointAsync(
            loadedCheckpoint.Weights,
            ct).ConfigureAwait(false);
        if (!checkpoint.IsSuccess || checkpoint.Value is null)
        {
            return checkpoint.MapError<
                DesignConversionRepositoryOwnedModelCheckpointLoadReport>();
        }

        var machine = new DesignConversionMachine();
        var import = machine.ImportCheckpoint(checkpoint.Value);
        if (!import.IsSuccess)
        {
            return import.MapError<
                DesignConversionRepositoryOwnedModelCheckpointLoadReport>();
        }

        return OperationResult<
            DesignConversionRepositoryOwnedModelCheckpointLoadReport>.Ok(
            new DesignConversionRepositoryOwnedModelCheckpointLoadReport(
                machine,
                loadedCheckpoint.Manifest,
                loadedCheckpoint.WeightsSha256,
                loadedCheckpoint.WeightsByteLength,
                [
                    LoadedFromRepositoryEvidence,
                    ImportedIntoMachineEvidence,
                    "repository_manifest:" +
                    loadedCheckpoint.Manifest.RepositoryRelativeManifestPath,
                    "repository_weights:" +
                    loadedCheckpoint.Manifest.RepositoryRelativeWeightsPath
                ],
                loadedCheckpoint.Diagnostics));
    }

    public static OperationResult<
        DesignConversionRepositoryOwnedModelCheckpointPublicationReport>
        PublishNext(
            DesignConversionRepositoryOwnedModelCheckpointPublicationRequest
                request)
    {
        var validation = ValidatePublication(request);
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                DesignConversionRepositoryOwnedModelCheckpointPublicationReport>();
        }

        var preparedStart = request.PreparedStart;
        var parent = preparedStart.ParentManifest;
        var nextStep = parent is null ? 1 : checked(parent.Step + 1);
        var address = RepositoryOwnedTrainingArtifactLayout
            .CreateModelCheckpointAddress(
                request.Target.RepositoryRoot,
                request.Target.DomainSlug,
                request.Target.DesignPatternSlug,
                request.Target.ModelIdSlug,
                request.Target.ModelVersion,
                nextStep);
        if (!address.IsSuccess || address.Value is null)
        {
            return address.MapError<
                DesignConversionRepositoryOwnedModelCheckpointPublicationReport>();
        }

        var checkpoint = preparedStart.Machine.ExportCheckpoint();
        if (!checkpoint.IsSuccess || checkpoint.Value is null)
        {
            return checkpoint.MapError<
                DesignConversionRepositoryOwnedModelCheckpointPublicationReport>();
        }

        var checkpointBytes = JsonSerializer.SerializeToUtf8Bytes(
            checkpoint.Value,
            DesignConversionCheckpointJsonOptions.Standard);
        using var checkpointStream = new MemoryStream(
            checkpointBytes,
            writable: false);
        var manifestRequest = new RepositoryOwnedModelCheckpointManifestRequest(
            ParentCheckpoint: parent is null
                ? null
                : new RepositoryOwnedModelCheckpointParentIdentity(
                    parent.RepositoryRelativeManifestPath,
                    parent.Step,
                    parent.WeightsSha256,
                    parent.WeightsByteLength),
            ColdStartReason: parent is null
                ? request.ColdStartReason!
                : string.Empty,
            DpoDatasetManifest: request.DpoDatasetManifest,
            RuntimeCompatibility: request.Target.RuntimeCompatibility,
            CreatedAtUtc: request.CreatedAtUtc,
            ProvenanceRefs: request.ProvenanceRefs);
        var sealedManifest =
            RepositoryOwnedModelCheckpointManifestSealer.PersistAndSeal(
                address.Value,
                checkpointStream,
                manifestRequest);
        if (!sealedManifest.IsSuccess || sealedManifest.Value is null)
        {
            return sealedManifest.MapError<
                DesignConversionRepositoryOwnedModelCheckpointPublicationReport>();
        }

        return OperationResult<
            DesignConversionRepositoryOwnedModelCheckpointPublicationReport>.Ok(
            new DesignConversionRepositoryOwnedModelCheckpointPublicationReport(
                sealedManifest.Value,
                sealedManifest.Value.RepositoryRelativeWeightsPath,
                sealedManifest.Value.RepositoryRelativeManifestPath,
                parent is null
                    ? [PublishedToRepositoryEvidence, ColdStartPublishedEvidence]
                    : [
                        PublishedToRepositoryEvidence,
                        ContinuedFromParentEvidence,
                        "parent_manifest:" +
                        parent.RepositoryRelativeManifestPath
                    ]));
    }

    private static async Task<OperationResult<
        DesignConversionMachineCheckpoint>> ReadCheckpointAsync(
        Stream weights,
        CancellationToken ct)
    {
        try
        {
            if (weights.CanSeek)
                weights.Position = 0;

            var checkpoint =
                await JsonSerializer.DeserializeAsync<
                    DesignConversionMachineCheckpoint>(
                    weights,
                    DesignConversionCheckpointJsonOptions.Standard,
                    ct).ConfigureAwait(false);
            return checkpoint is null
                ? OperationResult<DesignConversionMachineCheckpoint>.Fail(
                    "Repository-owned DesignConversion checkpoint payload is empty.",
                    CheckpointPayloadInvalidErrorCode)
                : OperationResult<DesignConversionMachineCheckpoint>.Ok(
                    checkpoint);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            return OperationResult<DesignConversionMachineCheckpoint>.Fail(
                "Repository-owned DesignConversion checkpoint load cancelled.",
                "CANCELLED");
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<DesignConversionMachineCheckpoint>.Fail(
                "Repository-owned DesignConversion checkpoint payload is not a valid DesignConversionMachine checkpoint.",
                CheckpointPayloadInvalidErrorCode);
        }
    }

    private static OperationResult<bool> ValidatePublication(
        DesignConversionRepositoryOwnedModelCheckpointPublicationRequest?
            request)
    {
        if (request is null ||
            request.PreparedStart is null ||
            request.Machine is null ||
            request.DpoDatasetManifest is null ||
            request.ProvenanceRefs is null)
        {
            return Invalid();
        }

        var target = ValidateTarget(request.Target);
        if (!target.IsSuccess)
            return target;

        // WHY: the prepared start owns the exact machine that was loaded from
        // disk or created as the allowed cold start. Publishing a different
        // object would let a caller save unrelated weights under that proof.
        if (!ReferenceEquals(request.PreparedStart.Machine, request.Machine))
            return Invalid();

        // WHY: a caller must not reuse a valid prepared start from one
        // repository/model/runtime identity to publish files into another tree.
        if (!TargetsAreSame(request.PreparedStart.Target, request.Target))
            return Invalid();

        if (request.ParentCheckpoint is null)
        {
            if (!PreparedColdStartMatchesPublication(request))
                return Invalid();
        }
        else if (!PreparedParentMatchesPublication(request))
        {
            return Invalid();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<bool> ValidateTarget(
        DesignConversionRepositoryOwnedModelCheckpointRequest? request)
        => request is not null &&
           IsBoundedText(request.RepositoryRoot) &&
           IsBoundedText(request.DomainSlug) &&
           IsBoundedText(request.DesignPatternSlug) &&
           IsBoundedText(request.ModelIdSlug) &&
           IsBoundedText(request.ModelVersion) &&
           request.RuntimeCompatibility is not null
            ? OperationResult<bool>.Ok(true)
            : Invalid();

    internal static OperationResult<bool> ValidateContextRequest(
        DesignConversionRepositoryOwnedModelCheckpointRequest? request)
    {
        var target = ValidateTarget(request);
        if (!target.IsSuccess)
            return OperationResult<bool>.Fail(
                DesignConversionRepositoryOwnedModelCheckpointContext
                    .InvalidSafeMessage,
                DesignConversionRepositoryOwnedModelCheckpointContext
                    .InvalidErrorCode);

        if (!RuntimeCompatibilityIsWellFormed(
                request!.RuntimeCompatibility) ||
            !ColdStartPolicyIsWellFormed(
                request.AllowColdStart,
                request.ColdStartReason) ||
            !ProvenanceRefsAreWellFormed(request.ProvenanceRefs))
        {
            return OperationResult<bool>.Fail(
                DesignConversionRepositoryOwnedModelCheckpointContext
                    .InvalidSafeMessage,
                DesignConversionRepositoryOwnedModelCheckpointContext
                    .InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }

    private static bool ParentMatchesTarget(
        RepositoryOwnedModelCheckpointManifest parent,
        DesignConversionRepositoryOwnedModelCheckpointRequest target)
        => StringComparer.Ordinal.Equals(parent.Domain, target.DomainSlug) &&
           StringComparer.Ordinal.Equals(
               parent.DesignPattern,
               target.DesignPatternSlug) &&
           StringComparer.Ordinal.Equals(parent.ModelId, target.ModelIdSlug) &&
           StringComparer.Ordinal.Equals(parent.Version, target.ModelVersion) &&
           RuntimeCompatibilityMatches(
               parent.RuntimeCompatibility,
               target.RuntimeCompatibility);

    private static bool PreparedColdStartMatchesPublication(
        DesignConversionRepositoryOwnedModelCheckpointPublicationRequest request)
        => request.PreparedStart.ParentManifest is null &&
           IsBoundedText(request.PreparedStart.ColdStartReason) &&
           string.Equals(
               request.PreparedStart.ColdStartReason,
               request.ColdStartReason,
               StringComparison.Ordinal) &&
           request.PreparedStart.EvidenceRefs.Contains(
               ColdStartPublishedEvidence,
               StringComparer.Ordinal);

    private static bool PreparedParentMatchesPublication(
        DesignConversionRepositoryOwnedModelCheckpointPublicationRequest request)
    {
        var parent = request.ParentCheckpoint;
        var preparedParent = request.PreparedStart.ParentManifest;
        if (parent is null ||
            preparedParent is null ||
            !ParentMatchesTarget(parent, request.Target) ||
            !ParentManifestsAreSame(parent, preparedParent))
        {
            return false;
        }

        // WHY: a resumed publication should not carry a cold-start reason at
        // all. Otherwise a caller could make a child checkpoint look like a
        // fresh model and hide which saved parent was actually loaded.
        return string.IsNullOrEmpty(request.ColdStartReason) &&
               string.IsNullOrEmpty(request.PreparedStart.ColdStartReason) &&
               request.PreparedStart.EvidenceRefs.Contains(
                   LoadedFromRepositoryEvidence,
                   StringComparer.Ordinal);
    }

    private static bool TargetsAreSame(
        DesignConversionRepositoryOwnedModelCheckpointRequest left,
        DesignConversionRepositoryOwnedModelCheckpointRequest right)
        => string.Equals(left.RepositoryRoot, right.RepositoryRoot, StringComparison.Ordinal) &&
           string.Equals(left.DomainSlug, right.DomainSlug, StringComparison.Ordinal) &&
           string.Equals(
               left.DesignPatternSlug,
               right.DesignPatternSlug,
               StringComparison.Ordinal) &&
           string.Equals(left.ModelIdSlug, right.ModelIdSlug, StringComparison.Ordinal) &&
           string.Equals(left.ModelVersion, right.ModelVersion, StringComparison.Ordinal) &&
           left.AllowColdStart == right.AllowColdStart &&
           string.Equals(
               left.ColdStartReason,
               right.ColdStartReason,
               StringComparison.Ordinal) &&
           RuntimeCompatibilityMatches(
               left.RuntimeCompatibility,
               right.RuntimeCompatibility) &&
           TextListsAreSame(left.ProvenanceRefs, right.ProvenanceRefs);

    private static bool TextListsAreSame(
        IReadOnlyList<string>? left,
        IReadOnlyList<string>? right)
    {
        if (left is null || right is null)
            return left is null && right is null;
        if (left.Count != right.Count)
            return false;
        for (var index = 0; index < left.Count; index++)
        {
            if (!string.Equals(left[index], right[index], StringComparison.Ordinal))
                return false;
        }

        return true;
    }

    private static bool ParentManifestsAreSame(
        RepositoryOwnedModelCheckpointManifest left,
        RepositoryOwnedModelCheckpointManifest right)
        => left.Step == right.Step &&
           left.WeightsByteLength == right.WeightsByteLength &&
           string.Equals(
               left.RepositoryRelativeManifestPath,
               right.RepositoryRelativeManifestPath,
               StringComparison.Ordinal) &&
           string.Equals(
               left.WeightsSha256,
               right.WeightsSha256,
               StringComparison.Ordinal) &&
           string.Equals(
               left.ManifestSha256,
               right.ManifestSha256,
               StringComparison.Ordinal);

    private static bool RuntimeCompatibilityMatches(
        RepositoryOwnedModelRuntimeCompatibility left,
        RepositoryOwnedModelRuntimeCompatibility right)
        => StringComparer.Ordinal.Equals(left.ConfigSha256, right.ConfigSha256) &&
           StringComparer.Ordinal.Equals(
               left.TopologySha256,
               right.TopologySha256) &&
           StringComparer.Ordinal.Equals(
               left.TokenizerSha256,
               right.TokenizerSha256) &&
           StringComparer.Ordinal.Equals(left.TrainerSha256, right.TrainerSha256) &&
           StringComparer.Ordinal.Equals(left.SchemaSha256, right.SchemaSha256) &&
           StringComparer.Ordinal.Equals(
               left.CompatibilityKey,
               right.CompatibilityKey);

    private static bool IsBoundedText(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           value.Length <= 512 &&
           !value.Any(char.IsControl);

    private static bool RuntimeCompatibilityIsWellFormed(
        RepositoryOwnedModelRuntimeCompatibility compatibility)
        => IsCanonicalSha256(compatibility.ConfigSha256) &&
           IsCanonicalSha256(compatibility.TopologySha256) &&
           IsCanonicalSha256(compatibility.TokenizerSha256) &&
           IsCanonicalSha256(compatibility.TrainerSha256) &&
           IsCanonicalSha256(compatibility.SchemaSha256) &&
           IsSafeSlug(compatibility.CompatibilityKey, maximumLength: 256);

    private static bool ColdStartPolicyIsWellFormed(
        bool allowColdStart,
        string? coldStartReason)
        => allowColdStart
            ? IsBoundedText(coldStartReason)
            : string.IsNullOrEmpty(coldStartReason);

    private static bool ProvenanceRefsAreWellFormed(
        IReadOnlyList<string>? provenanceRefs)
        => provenanceRefs is { Count: > 0 and <= 32 } &&
           provenanceRefs.Distinct(StringComparer.Ordinal).Count() ==
           provenanceRefs.Count &&
           provenanceRefs.All(IsBoundedText);

    private static bool IsCanonicalSha256(string? value)
        => value is { Length: 64 } &&
           value.All(static character => character is >= '0' and <= '9'
               or >= 'a' and <= 'f');

    private static bool IsSafeSlug(string? value, int maximumLength)
        => !string.IsNullOrWhiteSpace(value) &&
           value.Length <= maximumLength &&
           StringComparer.Ordinal.Equals(value, value.Trim()) &&
           value.All(static character =>
               character is >= 'a' and <= 'z'
               || character is >= '0' and <= '9'
               || character is '-' or '_' or '.');

    private static bool IsCleanMissingRepositoryOwnedModelCheckpoint<T>(
        OperationResult<T> result)
        => string.Equals(
               result.ErrorCode,
               RepositoryOwnedModelCheckpointLoader.NoCheckpointTreeErrorCode,
               StringComparison.Ordinal) ||
           string.Equals(
               result.ErrorCode,
               RepositoryOwnedModelCheckpointLoader.NoSealedCandidateErrorCode,
               StringComparison.Ordinal);

    private static OperationResult<bool> Invalid()
        => OperationResult<bool>.Fail(
            "DesignConversion repository-owned model checkpoint request is invalid.",
            InvalidErrorCode);
}
