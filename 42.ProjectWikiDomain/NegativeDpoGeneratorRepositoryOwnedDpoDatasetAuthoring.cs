// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;
using DpoPairSideAuthorKind = DPO.Models.DpoPairSideAuthorKind;

public sealed record NegativeDpoGeneratorRepositoryOwnedDpoDatasetCreationRequest(
    string BatchId,
    string RepositoryRoot,
    string DatasetVersion,
    RepositoryOwnedDpoDatasetAddress Address,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
    int ExpectedPairCount);

public sealed record NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext(
    string RepositoryRoot,
    string DatasetVersion,
    RepositoryOwnedDpoDatasetAddress Address,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
    Func<NegativeDpoGeneratorRepositoryOwnedDpoDatasetCreationRequest,
        CancellationToken,
        Task<OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>>>?
        CreatePairsAsync = null)
{
    public const string InvalidErrorCode =
        "NEGATIVE_DPO_REPOSITORY_OWNED_DPO_CONTEXT_INVALID";

    public const string InvalidSafeMessage =
        "NegativeDpoGenerator repository-owned DPO context must carry the canonical repository root, dataset version, address, and exact request identity.";

    public OperationResult<bool> Validate()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(RepositoryRoot) ||
                string.IsNullOrWhiteSpace(DatasetVersion) ||
                Address is null ||
                RequestIdentity is null ||
                !RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                    Address) ||
                !RepositoryOwnedDpoDatasetManifestSealer
                    .IsValidRequestIdentity(RequestIdentity) ||
                !SamePath(RepositoryRoot, Address.RepositoryRoot) ||
                !StringComparer.Ordinal.Equals(DatasetVersion, Address.DatasetVersion) ||
                !StringComparer.Ordinal.Equals(
                    NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                        .DomainSlug,
                    Address.DomainSlug) ||
                !StringComparer.Ordinal.Equals(
                    NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                        .DesignPatternSlug,
                    Address.DesignPatternSlug))
            {
                return OperationResult<bool>.Fail(
                    InvalidSafeMessage,
                    InvalidErrorCode);
            }

            return OperationResult<bool>.Ok(true);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<bool>.Fail(
                InvalidSafeMessage,
                InvalidErrorCode);
        }
    }

    private static bool SamePath(string first, string second)
        => string.Equals(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(first)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(second)),
            OperatingSystem.IsWindows()
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal);
}

public sealed record NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch(
    NegativeDpoGeneratorBatch Batch,
    RepositoryOwnedDpoDatasetAddress Address,
    IReadOnlyList<ContinuousLearning.DpoPair> ReopenedRepositoryPairs,
    RepositoryOwnedDpoDatasetResolutionSource Source,
    string ManifestRelativePath,
    string ManifestSha256,
    string ReopenedProjectPairsSha256);

/// <summary>
/// Names the exact NegativeDpoGenerator model identity under the shared
/// repository-owned checkpoint tree. A concrete step is still created by the
/// common layout helper so this domain never treats a caller checkpoint folder
/// as model authority.
/// </summary>
public sealed record NegativeDpoGeneratorRepositoryOwnedModelCheckpointAddress(
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
        if (!StringComparer.Ordinal.Equals(
                DomainSlug,
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                    .DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                DesignPatternSlug,
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
                    .DesignPatternSlug))
        {
            return OperationResult<bool>.Fail(
                NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                    .InvalidSafeMessage,
                NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                    .InvalidErrorCode);
        }

        var probe = CreateStepAddress(step: 1);
        if (!probe.IsSuccess || probe.Value is null)
        {
            return OperationResult<bool>.Fail(
                NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                    .InvalidSafeMessage,
                NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                    .InvalidErrorCode);
        }

        return OperationResult<bool>.Ok(true);
    }
}

/// <summary>
/// Caller-owned policy for one repository-owned NegativeDpoGenerator model
/// continuation. Runtime fingerprints choose a compatible parent; first-step
/// cold start is allowed only when the caller records a bounded human reason.
/// </summary>
public sealed record NegativeDpoGeneratorRepositoryOwnedModelCheckpointRequest(
    NegativeDpoGeneratorRepositoryOwnedModelCheckpointAddress Address,
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
            !ColdStartPolicyIsWellFormed(
                AllowColdStart,
                ColdStartReason) ||
            !ProvenanceRefsAreWellFormed(ProvenanceRefs))
        {
            return OperationResult<bool>.Fail(
                NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                    .InvalidSafeMessage,
                NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
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
/// Repository-owned model checkpoint authority for the live NegativeDpo route.
/// Disabled keeps legacy callers source-compatible; the live repository-owned
/// route rejects disabled/missing context before trainer mutation.
/// </summary>
public sealed record NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext(
    bool Enabled,
    NegativeDpoGeneratorRepositoryOwnedModelCheckpointRequest? Request)
{
    public static NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
        Disabled { get; } = new(Enabled: false, Request: null);

    public const string InvalidErrorCode =
        "NEGATIVE_DPO_REPOSITORY_OWNED_MODEL_CHECKPOINT_CONTEXT_INVALID";

    public const string InvalidSafeMessage =
        "NegativeDpoGenerator repository-owned model checkpoint context must point to the canonical model identity, runtime compatibility, provenance refs, and explicit cold-start policy.";

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

/// <summary>
/// Caller-side bridge between the ProjectWiki NegativeDpo training batch and
/// the shared repository-owned DPO sealer/resolver. It intentionally owns no
/// lock or manifest format; those remain in ContinuousLearning.
/// </summary>
public static class NegativeDpoGeneratorRepositoryOwnedDpoDatasetAuthoring
{
    public const string DomainSlug = "dpo-dataset-creation";
    public const string DesignPatternSlug = "negative-dpo-generator";
    public const string DefaultDatasetVersion = "v1";
    public const string PairCarrierSchema =
        "negative-dpo-generator-repository-owned-pair-carrier-v1";
    public const string InvalidErrorCode =
        "NEGATIVE_DPO_REPOSITORY_OWNED_DPO_AUTHORING_INVALID";
    public const string BatchSizeErrorCode =
        "NEGATIVE_DPO_REPOSITORY_OWNED_DPO_BATCH_SIZE_NOT_50";

    private const int ExpectedPairCount = 50;
    private const int MaximumTextUtf8Bytes = 128 * 1024;

    private static readonly JsonDocumentOptions StrictJson = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 8
    };

    public static OperationResult<RepositoryOwnedDpoDatasetAddress>
        CreateAddress(
            string repositoryRoot,
            string datasetVersion = DefaultDatasetVersion)
        => RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            DomainSlug,
            DesignPatternSlug,
            datasetVersion);

    public static OperationResult<
        NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext>
        CreateModelCheckpointContext(
            string repositoryRoot,
            string modelIdSlug,
            string modelVersion,
            RepositoryOwnedModelRuntimeCompatibility runtimeCompatibility,
            bool allowColdStart,
            string? coldStartReason,
            IReadOnlyList<string> provenanceRefs)
    {
        try
        {
            var context =
                new NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext(
                    Enabled: true,
                    Request: new NegativeDpoGeneratorRepositoryOwnedModelCheckpointRequest(
                        new NegativeDpoGeneratorRepositoryOwnedModelCheckpointAddress(
                            Path.GetFullPath(repositoryRoot),
                            DomainSlug,
                            DesignPatternSlug,
                            modelIdSlug,
                            modelVersion),
                        runtimeCompatibility,
                        allowColdStart,
                        coldStartReason,
                        provenanceRefs));
            var validation = context.Validate();
            return validation.IsSuccess
                ? OperationResult<
                    NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext>
                    .Ok(context)
                : validation.MapError<
                    NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext>
                .Fail(
                    NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                        .InvalidSafeMessage,
                    NegativeDpoGeneratorRepositoryOwnedModelCheckpointContext
                        .InvalidErrorCode);
        }
    }

    public static OperationResult<
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext> CreateContext(
            string repositoryRoot,
            string datasetVersion,
            RepositoryOwnedDpoDatasetRequestIdentity requestIdentity,
            Func<NegativeDpoGeneratorRepositoryOwnedDpoDatasetCreationRequest,
                CancellationToken,
                Task<OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>>>?
                createPairsAsync = null)
    {
        var address = CreateAddress(repositoryRoot, datasetVersion);
        if (!address.IsSuccess || address.Value is null)
            return address.MapError<
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext>();

        try
        {
            var context =
                new NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext(
                    Path.GetFullPath(repositoryRoot),
                    datasetVersion,
                    address.Value,
                    requestIdentity,
                    createPairsAsync);
            var validation = context.Validate();
            return validation.IsSuccess
                ? OperationResult<
                    NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext>.Ok(
                        context)
                : validation.MapError<
                    NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext>.Fail(
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext
                    .InvalidSafeMessage,
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext
                    .InvalidErrorCode);
        }
    }

    public static async Task<OperationResult<
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>>
        ResolveTrainingBatchAsync(
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetContext context,
            string batchId,
            CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>.Fail(
                "NegativeDpoGenerator repository-owned DPO resolution was cancelled.",
                RepositoryOwnedDpoDatasetResolver.CancelledErrorCode);
        }

        if (string.IsNullOrWhiteSpace(batchId))
        {
            return OperationResult<
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>.Fail(
                "Batch id is required for NegativeDpoGenerator repository-owned DPO resolution.",
                InvalidErrorCode);
        }

        var validation = context.Validate();
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>();
        }

        Func<RepositoryOwnedDpoDatasetAddress, CancellationToken,
            Task<OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>>>?
            creator = null;
        if (context.CreatePairsAsync is not null)
        {
            creator = async (address, ct) =>
            {
                if (!SameAddress(address, context.Address))
                {
                    return OperationResult<
                        IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                        "NegativeDpoGenerator repository-owned DPO creator received a different dataset address.",
                        InvalidErrorCode);
                }

                var request =
                    new NegativeDpoGeneratorRepositoryOwnedDpoDatasetCreationRequest(
                        batchId,
                        context.RepositoryRoot,
                        context.DatasetVersion,
                        context.Address,
                        context.RequestIdentity,
                        ExpectedPairCount);
                var created = await context.CreatePairsAsync(request, ct)
                    .ConfigureAwait(false);
                if (!created.IsSuccess)
                    return created;

                if (created.Value is null ||
                    created.Value.Count != ExpectedPairCount)
                {
                    return OperationResult<
                        IReadOnlyList<ContinuousLearning.DpoPair>>.Fail(
                        "NegativeDpoGenerator repository-owned DPO creator must return exactly 50 rows before sealing.",
                        BatchSizeErrorCode);
                }

                return created;
            };
        }

        // WHY: this is the hard disk-first boundary. The injected creator is
        // passed as a delegate, but the resolver calls it only after an exact
        // clean miss under the repository-owned creation reservation.
        var resolved = await RepositoryOwnedDpoDatasetResolver
            .OpenExistingOrCreateAndSealAsync(
                context.Address,
                context.RequestIdentity,
                creator,
                cancellationToken)
            .ConfigureAwait(false);
        if (!resolved.IsSuccess || resolved.Value is null)
        {
            return resolved.MapError<
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>();
        }

        return BuildTrainingBatchFromReopenedDataset(
            context.Address,
            batchId,
            resolved.Value);
    }

    public static OperationResult<ContinuousLearning.DpoPair>
        MapProjectPairToRepositoryPair(
            RepositoryOwnedDpoDatasetAddress address,
            ProjectDocumentationDpoPair projectPair,
            TrainingTeacherProvenance teacherProvenance,
            DpoPairSideAuthorKind chosenAuthor,
            DpoPairSideAuthorKind rejectedAuthor)
    {
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address) ||
            !IsValidProjectPair(projectPair) ||
            !HasAdmittedRepositoryOwnedAuthorship(
                teacherProvenance,
                chosenAuthor,
                rejectedAuthor))
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                "NegativeDpoGenerator project pair cannot be mapped to a repository-owned DPO row.",
                InvalidErrorCode);
        }

        ContinuousLearning.DpoPair pair;
        try
        {
            pair = new ContinuousLearning.DpoPair(
                Prompt: BuildCarrier(projectPair),
                Chosen: projectPair.Chosen,
                Rejected: projectPair.Rejected,
                Domain: address.DomainSlug,
                CapturedAt: DateTimeOffset.UnixEpoch)
            {
                TeacherProvenance = teacherProvenance,
                GovernanceDecision = new TrainingDataGovernanceDecision(
                    licenseAccepted: true,
                    isQuarantined: false,
                    evidenceRefs: BuildGovernanceEvidenceRefs(
                        address,
                        projectPair)),
                ChosenAuthorship = CreateAuthorship(
                    chosenAuthor,
                    teacherProvenance),
                RejectedAuthorship = CreateAuthorship(
                    rejectedAuthor,
                    teacherProvenance)
            };
        }
        catch (ArgumentException)
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                "NegativeDpoGenerator repository-owned DPO authorship is invalid.",
                InvalidErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                "NegativeDpoGenerator project pair could not be framed for repository-owned DPO storage.",
                InvalidErrorCode);
        }

        var bound = DpoPairTrainingContentAuthority.Bind(pair);
        return bound.IsSuccess && bound.Value is not null
            ? bound
            : bound.MapError<ContinuousLearning.DpoPair>();
    }

    public static OperationResult<ProjectDocumentationDpoPair>
        MapRepositoryPairToProjectPair(
            RepositoryOwnedDpoDatasetAddress address,
            ContinuousLearning.DpoPair pair)
    {
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address) ||
            pair is null ||
            !StringComparer.Ordinal.Equals(pair.Domain, address.DomainSlug) ||
            !IsBoundedText(pair.Chosen) ||
            !IsBoundedText(pair.Rejected))
        {
            return OperationResult<ProjectDocumentationDpoPair>.Fail(
                "Repository-owned NegativeDpoGenerator row does not match the dataset address.",
                InvalidErrorCode);
        }

        var carrier = ParseCarrier(pair.Prompt);
        if (!carrier.IsSuccess || carrier.Value is null)
            return carrier.MapError<ProjectDocumentationDpoPair>();

        return OperationResult<ProjectDocumentationDpoPair>.Ok(
            new ProjectDocumentationDpoPair(
                carrier.Value.PairId,
                carrier.Value.DocumentationUnitId,
                carrier.Value.A,
                pair.Chosen,
                pair.Rejected,
                carrier.Value.NegativeType,
                carrier.Value.EvidenceIds));
    }

    public static string ComputeProjectPairsSha256(
        IReadOnlyList<ProjectDocumentationDpoPair> pairs)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            writer.WriteStartArray();
            foreach (var pair in pairs)
            {
                writer.WriteStartObject();
                writer.WriteString("pair_id", pair.PairId);
                writer.WriteString(
                    "documentation_unit_id",
                    pair.DocumentationUnitId);
                writer.WriteString("a", pair.A);
                writer.WriteString("chosen", pair.Chosen);
                writer.WriteString("rejected", pair.Rejected);
                writer.WriteString("negative_type", pair.NegativeType);
                writer.WriteStartArray("evidence_ids");
                foreach (var evidenceId in pair.EvidenceIds)
                    writer.WriteStringValue(evidenceId);
                writer.WriteEndArray();
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.Flush();
        }

        return ComputeSha256(buffer.ToArray());
    }

    private static OperationResult<
        NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>
        BuildTrainingBatchFromReopenedDataset(
            RepositoryOwnedDpoDatasetAddress address,
            string batchId,
            RepositoryOwnedDpoDatasetResolution resolution)
    {
        var mapped = new List<ProjectDocumentationDpoPair>(
            resolution.Dataset.Pairs.Count);
        foreach (var pair in resolution.Dataset.Pairs)
        {
            var projectPair = MapRepositoryPairToProjectPair(
                address,
                pair);
            if (!projectPair.IsSuccess || projectPair.Value is null)
            {
                return projectPair.MapError<
                    NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>();
            }

            mapped.Add(projectPair.Value);
        }

        if (mapped.Count != ExpectedPairCount)
        {
            return OperationResult<
                NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>.Fail(
                "Repository-owned NegativeDpoGenerator training dataset must reopen exactly 50 rows.",
                BatchSizeErrorCode);
        }

        var pairs = mapped.ToArray();
        var repositoryPairs = resolution.Dataset.Pairs
            .Select(static pair => pair with { })
            .ToArray();
        var coverage = RequiredNegativeTypes.Count(type => pairs.Any(pair =>
            string.Equals(
                pair.NegativeType,
                type,
                StringComparison.Ordinal))) /
            (double)RequiredNegativeTypes.Length;
        var wrongAnnotationRate = pairs.Count(pair =>
            pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) /
            (double)pairs.Length;
        var batch = new NegativeDpoGeneratorBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            AcceptedCandidateRate: 1.0,
            coverage,
            DocumentationUnits: [],
            Candidates: [],
            Validations: [],
            pairs,
            Warnings: []);

        return OperationResult<
            NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch>.Ok(
            new NegativeDpoGeneratorRepositoryOwnedDpoDatasetTrainingBatch(
                batch,
                address,
                repositoryPairs,
                resolution.Source,
                address.ManifestRelativePath,
                resolution.Dataset.Manifest.ManifestSha256,
                ComputeProjectPairsSha256(pairs)));
    }

    private static OperationResult<ProjectPairCarrier> ParseCarrier(
        string frame)
    {
        if (!IsBoundedText(frame))
        {
            return OperationResult<ProjectPairCarrier>.Fail(
                "Repository-owned NegativeDpoGenerator row is missing its framed prompt carrier.",
                InvalidErrorCode);
        }

        try
        {
            using var document = JsonDocument.Parse(frame, StrictJson);
            var root = document.RootElement;
            if (root.ValueKind is not JsonValueKind.Object ||
                root.EnumerateObject().Count() != 6)
            {
                return OperationResult<ProjectPairCarrier>.Fail(
                    "Repository-owned NegativeDpoGenerator carrier has an invalid shape.",
                    InvalidErrorCode);
            }

            var schema = ReadString(root, "schema");
            var pairId = ReadString(root, "pair_id");
            var documentationUnitId =
                ReadString(root, "documentation_unit_id");
            var a = ReadString(root, "a");
            var negativeType = ReadString(root, "negative_type");
            var evidenceIds = ReadStringArray(root, "evidence_ids");
            if (!StringComparer.Ordinal.Equals(schema, PairCarrierSchema) ||
                !IsBoundedText(pairId) ||
                !IsBoundedText(documentationUnitId) ||
                !IsBoundedText(a) ||
                !RequiredNegativeTypes.Contains(
                    negativeType,
                    StringComparer.Ordinal) ||
                evidenceIds is null ||
                evidenceIds.Length == 0 ||
                evidenceIds.Any(static item => !IsBoundedText(item)))
            {
                return OperationResult<ProjectPairCarrier>.Fail(
                    "Repository-owned NegativeDpoGenerator carrier fields are invalid.",
                    InvalidErrorCode);
            }

            return OperationResult<ProjectPairCarrier>.Ok(
                new ProjectPairCarrier(
                    pairId!,
                    documentationUnitId!,
                    a!,
                    negativeType!,
                    evidenceIds));
        }
        catch (JsonException)
        {
            return OperationResult<ProjectPairCarrier>.Fail(
                "Repository-owned NegativeDpoGenerator carrier is not strict JSON.",
                InvalidErrorCode);
        }
    }

    private static string BuildCarrier(ProjectDocumentationDpoPair pair)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            // WHY: the shared rich DpoPair has no PairId or negative-type
            // fields. A strict JSON prompt carrier lets us recover those
            // semantics without scraping arbitrary answer text later.
            writer.WriteStartObject();
            writer.WriteString("schema", PairCarrierSchema);
            writer.WriteString("pair_id", pair.PairId);
            writer.WriteString(
                "documentation_unit_id",
                pair.DocumentationUnitId);
            writer.WriteString("a", pair.A);
            writer.WriteString("negative_type", pair.NegativeType);
            writer.WriteStartArray("evidence_ids");
            foreach (var evidenceId in pair.EvidenceIds)
                writer.WriteStringValue(evidenceId);
            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static DpoPairSideAuthorship CreateAuthorship(
        DpoPairSideAuthorKind author,
        TrainingTeacherProvenance teacherProvenance)
        => author is DpoPairSideAuthorKind.OpenTeacher
            ? new DpoPairSideAuthorship(author, teacherProvenance)
            : new DpoPairSideAuthorship(author);

    private static bool HasAdmittedRepositoryOwnedAuthorship(
        TrainingTeacherProvenance? teacherProvenance,
        DpoPairSideAuthorKind chosenAuthor,
        DpoPairSideAuthorKind rejectedAuthor)
        =>
        // WHY: the repository-owned sealer admits an ExternalTeacherOutput
        // passport matching the request identity. This bridge therefore refuses
        // paid sides and refuses deterministic/deterministic rows paired with a
        // fake open-teacher provenance before the shared sealer ever sees them.
        teacherProvenance is
        {
            SourceKind: TrainingTeacherSourceKind.ExternalTeacherOutput,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            Family: not null
        } &&
        chosenAuthor is not DpoPairSideAuthorKind.PaidExternalModel &&
        rejectedAuthor is not DpoPairSideAuthorKind.PaidExternalModel &&
        (chosenAuthor is DpoPairSideAuthorKind.OpenTeacher ||
         rejectedAuthor is DpoPairSideAuthorKind.OpenTeacher);

    private static IReadOnlyList<string> BuildGovernanceEvidenceRefs(
        RepositoryOwnedDpoDatasetAddress address,
        ProjectDocumentationDpoPair pair)
        =>
        [
            "sha256:" + ComputeTextSha256(
                "negative-dpo-generator-repository-dataset\n" +
                address.ManifestRelativePath),
            "sha256:" + ComputeTextSha256("pair-id\n" + pair.PairId),
            "sha256:" + ComputeTextSha256(
                "documentation-unit\n" + pair.DocumentationUnitId),
            "sha256:" + ComputeTextSha256(
                "negative-type\n" + pair.NegativeType),
            "sha256:" + ComputeTextSha256(
                "evidence\n" + string.Join("\n", pair.EvidenceIds))
        ];

    private static bool IsValidProjectPair(ProjectDocumentationDpoPair? pair)
        => pair is not null &&
           IsBoundedText(pair.PairId) &&
           IsBoundedText(pair.DocumentationUnitId) &&
           IsBoundedText(pair.A) &&
           IsBoundedText(pair.Chosen) &&
           IsBoundedText(pair.Rejected) &&
           RequiredNegativeTypes.Contains(
               pair.NegativeType,
               StringComparer.Ordinal) &&
           pair.EvidenceIds is { Length: > 0 } &&
           pair.EvidenceIds.All(static item => IsBoundedText(item)) &&
           !string.Equals(
               pair.Chosen.Trim(),
               pair.Rejected.Trim(),
               StringComparison.Ordinal);

    private static bool IsBoundedText(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           Encoding.UTF8.GetByteCount(value) <= MaximumTextUtf8Bytes &&
           !value.Any(static character =>
               char.IsControl(character) &&
               character is not '\n' and not '\r' and not '\t');

    private static string? ReadString(JsonElement root, string propertyName)
        => root.TryGetProperty(propertyName, out var value) &&
           value.ValueKind is JsonValueKind.String
            ? value.GetString()
            : null;

    private static string[]? ReadStringArray(
        JsonElement root,
        string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var value) ||
            value.ValueKind is not JsonValueKind.Array)
        {
            return null;
        }

        var values = new List<string>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind is not JsonValueKind.String)
                return null;
            values.Add(item.GetString() ?? string.Empty);
        }

        return values.ToArray();
    }

    private static bool SameAddress(
        RepositoryOwnedDpoDatasetAddress first,
        RepositoryOwnedDpoDatasetAddress second)
        => string.Equals(
               first.ManifestPath,
               second.ManifestPath,
               OperatingSystem.IsWindows()
                   ? StringComparison.OrdinalIgnoreCase
                   : StringComparison.Ordinal) &&
           string.Equals(first.DomainSlug, second.DomainSlug, StringComparison.Ordinal) &&
           string.Equals(first.DesignPatternSlug, second.DesignPatternSlug, StringComparison.Ordinal) &&
           string.Equals(first.DatasetVersion, second.DatasetVersion, StringComparison.Ordinal);

    private static string ComputeTextSha256(string value)
        => ComputeSha256(Encoding.UTF8.GetBytes(value));

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static readonly string[] RequiredNegativeTypes =
    [
        "missing_location",
        "missing_parameters",
        "missing_return_contract",
        "missing_callee_transit",
        "missing_caller_context"
    ];

    private sealed record ProjectPairCarrier(
        string PairId,
        string DocumentationUnitId,
        string A,
        string NegativeType,
        string[] EvidenceIds);
}
