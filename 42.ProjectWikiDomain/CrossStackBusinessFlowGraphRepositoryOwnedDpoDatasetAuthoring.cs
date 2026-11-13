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

/// <summary>
/// The exact clean-miss request handed to the future admitted local-teacher
/// authoring seam for one CrossStack split.
/// </summary>
public sealed record CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetCreationRequest(
    string BatchId,
    string RepositoryRoot,
    string DatasetVersion,
    DpoBatchSplitRole SplitRole,
    RepositoryOwnedDpoDatasetAddress Address,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
    int ExpectedPairCount);

/// <summary>
/// Caller-owned repository address and request identity for one CrossStack
/// repository-owned DPO split.
/// </summary>
public sealed record CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext(
    string RepositoryRoot,
    string DatasetVersion,
    DpoBatchSplitRole SplitRole,
    RepositoryOwnedDpoDatasetAddress Address,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
    Func<CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetCreationRequest,
        CancellationToken,
        Task<OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>>>?
        CreatePairsAsync = null)
{
    public const string InvalidErrorCode =
        "CROSS_STACK_REPOSITORY_OWNED_DPO_CONTEXT_INVALID";

    public const string InvalidSafeMessage =
        "CrossStack repository-owned DPO context must carry the canonical repository root, dataset version, split role, address, and exact request identity.";

    public OperationResult<bool> Validate()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(RepositoryRoot) ||
                string.IsNullOrWhiteSpace(DatasetVersion) ||
                SplitRole is not DpoBatchSplitRole.Training and
                    not DpoBatchSplitRole.HeldOut ||
                Address is null ||
                RequestIdentity is null ||
                !RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                    Address) ||
                !RepositoryOwnedDpoDatasetManifestSealer
                    .IsValidRequestIdentity(RequestIdentity) ||
                !SamePath(RepositoryRoot, Address.RepositoryRoot) ||
                !StringComparer.Ordinal.Equals(
                    DatasetVersion,
                    Address.DatasetVersion) ||
                !StringComparer.Ordinal.Equals(
                    CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
                        .DomainSlug,
                    Address.DomainSlug) ||
                !StringComparer.Ordinal.Equals(
                    CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
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

/// <summary>
/// Reopened repository-owned CrossStack split plus the domain batch reconstructed
/// from sealed DPO pair files.
/// </summary>
public sealed record CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch(
    CrossStackBusinessFlowGraphDpoBatch Batch,
    RepositoryOwnedDpoDatasetAddress Address,
    IReadOnlyList<ContinuousLearning.DpoPair> ReopenedRepositoryPairs,
    RepositoryOwnedDpoDatasetResolutionSource Source,
    RepositoryOwnedDpoDatasetRequestIdentity RequestIdentity,
    string ManifestRelativePath,
    string ManifestSha256,
    string ReopenedProjectPairsSha256);

/// <summary>
/// ProjectWiki-owned bridge between CrossStack rows and the shared
/// repository-owned DPO sealer/resolver.
/// </summary>
public static class CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetAuthoring
{
    public const string DomainSlug = "cross-stack-business-flow-graph";
    public const string DesignPatternSlug = "ilearning-unit";
    public const string PairCarrierSchema =
        "cross-stack-business-flow-graph-repository-owned-pair-carrier-v1";
    public const string InvalidErrorCode =
        "CROSS_STACK_REPOSITORY_OWNED_DPO_AUTHORING_INVALID";
    public const string BatchSizeErrorCode =
        "CROSS_STACK_REPOSITORY_OWNED_DPO_BATCH_SIZE_NOT_50";

    private const int ExpectedPairCount = 50;
    private const int MaximumTextUtf8Bytes = 128 * 1024;

    private static readonly JsonDocumentOptions StrictJson = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 12
    };

    public static OperationResult<RepositoryOwnedDpoDatasetAddress>
        CreateAddress(
            string repositoryRoot,
            string datasetVersion)
        => RepositoryOwnedTrainingArtifactLayout.CreateDpoDatasetAddress(
            repositoryRoot,
            DomainSlug,
            DesignPatternSlug,
            datasetVersion);

    public static OperationResult<
        CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext>
        CreateContext(
            string repositoryRoot,
            string datasetVersion,
            DpoBatchSplitRole splitRole,
            RepositoryOwnedDpoDatasetRequestIdentity requestIdentity,
            Func<CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetCreationRequest,
                CancellationToken,
                Task<OperationResult<IReadOnlyList<ContinuousLearning.DpoPair>>>>?
                createPairsAsync = null)
    {
        var address = CreateAddress(repositoryRoot, datasetVersion);
        if (!address.IsSuccess || address.Value is null)
            return address.MapError<
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext>();

        try
        {
            var context =
                new CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext(
                    Path.GetFullPath(repositoryRoot),
                    datasetVersion,
                    splitRole,
                    address.Value,
                    requestIdentity,
                    createPairsAsync);
            var validation = context.Validate();
            return validation.IsSuccess
                ? OperationResult<
                    CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext>
                    .Ok(context)
                : validation.MapError<
                    CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext>();
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext>.Fail(
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext
                    .InvalidSafeMessage,
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext
                    .InvalidErrorCode);
        }
    }

    public static async Task<OperationResult<
        CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>>
        ResolveBatchAsync(
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetContext context,
            string batchId,
            CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return OperationResult<
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>
                .Fail(
                    "CrossStack repository-owned DPO resolution was cancelled.",
                    RepositoryOwnedDpoDatasetResolver.CancelledErrorCode);
        }

        if (string.IsNullOrWhiteSpace(batchId))
        {
            return OperationResult<
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>
                .Fail(
                    "Batch id is required for CrossStack repository-owned DPO resolution.",
                    InvalidErrorCode);
        }

        var validation = context.Validate();
        if (!validation.IsSuccess)
        {
            return validation.MapError<
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>();
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
                        "CrossStack repository-owned DPO creator received a different dataset address.",
                        InvalidErrorCode);
                }

                var request =
                    new CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetCreationRequest(
                        batchId,
                        context.RepositoryRoot,
                        context.DatasetVersion,
                        context.SplitRole,
                        context.Address,
                        context.RequestIdentity,
                        ExpectedPairCount);
                var created = await context.CreatePairsAsync(request, ct)
                    .ConfigureAwait(false);
                if (!created.IsSuccess)
                    return created;

                var createdValidation = ValidateRepositoryRowsForSplit(
                    context.Address,
                    context.SplitRole,
                    created.Value);
                return createdValidation.IsSuccess
                    ? created
                    : createdValidation.MapError<
                        IReadOnlyList<ContinuousLearning.DpoPair>>();
            };
        }

        // WHY: this method deliberately delegates every hit/miss decision to
        // the common resolver. Exact sealed hits reopen from disk before the
        // injected local-teacher creator exists in the call chain; dirty,
        // mismatched, or partial datasets return an error and are not
        // regenerated.
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
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>();
        }

        if (!SameRequestIdentity(
                context.RequestIdentity,
                resolved.Value.Dataset.Manifest.RequestIdentity))
        {
            return OperationResult<
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>
                .Fail(
                    "CrossStack repository-owned DPO manifest identity no longer matches the requesting context.",
                    RepositoryOwnedDpoDatasetManifestSealer
                        .RequestIdentityMismatchErrorCode);
        }

        return BuildBatchFromReopenedDataset(
            context.Address,
            context.SplitRole,
            batchId,
            resolved.Value);
    }

    public static OperationResult<ContinuousLearning.DpoPair>
        MapProjectPairToRepositoryPair(
            RepositoryOwnedDpoDatasetAddress address,
            DpoBatchSplitRole splitRole,
            CrossStackBusinessFlowGraphDpoPair projectPair,
            TrainingTeacherProvenance teacherProvenance,
            DpoPairSideAuthorKind chosenAuthor,
            DpoPairSideAuthorKind rejectedAuthor)
    {
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address) ||
            splitRole is not DpoBatchSplitRole.Training and
                not DpoBatchSplitRole.HeldOut ||
            !StringComparer.Ordinal.Equals(address.DomainSlug, DomainSlug) ||
            !StringComparer.Ordinal.Equals(
                address.DesignPatternSlug,
                DesignPatternSlug) ||
            !IsValidProjectPair(projectPair) ||
            !HasAdmittedRepositoryOwnedAuthorship(
                teacherProvenance,
                chosenAuthor,
                rejectedAuthor))
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                "CrossStack project pair cannot be mapped to a repository-owned DPO row.",
                InvalidErrorCode);
        }

        ContinuousLearning.DpoPair pair;
        try
        {
            pair = new ContinuousLearning.DpoPair(
                Prompt: BuildCarrier(projectPair, splitRole),
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
                        splitRole,
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
                "CrossStack repository-owned DPO authorship is invalid.",
                InvalidErrorCode);
        }
        catch (Exception exception)
            when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<ContinuousLearning.DpoPair>.Fail(
                "CrossStack project pair could not be framed for repository-owned DPO storage.",
                InvalidErrorCode);
        }

        var bound = DpoPairTrainingContentAuthority.Bind(pair);
        return bound.IsSuccess && bound.Value is not null
            ? bound
            : bound.MapError<ContinuousLearning.DpoPair>();
    }

    public static OperationResult<CrossStackBusinessFlowGraphDpoPair>
        MapRepositoryPairToProjectPair(
            RepositoryOwnedDpoDatasetAddress address,
            DpoBatchSplitRole splitRole,
            ContinuousLearning.DpoPair pair)
    {
        if (!RepositoryOwnedTrainingArtifactLayout.IsValidDpoDatasetAddress(
                address) ||
            splitRole is not DpoBatchSplitRole.Training and
                not DpoBatchSplitRole.HeldOut ||
            pair is null ||
            !StringComparer.Ordinal.Equals(pair.Domain, address.DomainSlug) ||
            !IsBoundedText(pair.Chosen) ||
            !IsBoundedText(pair.Rejected))
        {
            return OperationResult<CrossStackBusinessFlowGraphDpoPair>.Fail(
                "Repository-owned CrossStack row does not match the dataset address.",
                InvalidErrorCode);
        }

        if (!HasAdmittedRepositoryPairAuthorship(pair))
        {
            return OperationResult<CrossStackBusinessFlowGraphDpoPair>.Fail(
                "Repository-owned CrossStack rows require OpenTeacher authorship on both trainable sides.",
                InvalidErrorCode);
        }

        var carrier = ParseCarrier(pair.Prompt);
        if (!carrier.IsSuccess || carrier.Value is null)
            return carrier.MapError<CrossStackBusinessFlowGraphDpoPair>();

        if (carrier.Value.SplitRole != splitRole)
        {
            return OperationResult<CrossStackBusinessFlowGraphDpoPair>.Fail(
                "Repository-owned CrossStack row belongs to a different split role.",
                InvalidErrorCode);
        }

        var projectPair = new CrossStackBusinessFlowGraphDpoPair(
            carrier.Value.PairId,
            carrier.Value.BoundaryMap,
            carrier.Value.A,
            pair.Chosen,
            pair.Rejected,
            carrier.Value.RejectedType,
            carrier.Value.RequiredPathIds,
            carrier.Value.EvidenceIds);
        return IsValidProjectPair(projectPair)
            ? OperationResult<CrossStackBusinessFlowGraphDpoPair>.Ok(
                projectPair)
            : OperationResult<CrossStackBusinessFlowGraphDpoPair>.Fail(
                "Repository-owned CrossStack row carrier fields are invalid.",
                InvalidErrorCode);
    }

    public static string ComputeProjectPairsSha256(
        IReadOnlyList<CrossStackBusinessFlowGraphDpoPair> pairs)
        => CrossStackBusinessFlowGraphBatchIntegrity.ComputeRowsSha256(pairs);

    private static OperationResult<
        CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>
        BuildBatchFromReopenedDataset(
            RepositoryOwnedDpoDatasetAddress address,
            DpoBatchSplitRole splitRole,
            string batchId,
            RepositoryOwnedDpoDatasetResolution resolution)
    {
        var mapped = new List<CrossStackBusinessFlowGraphDpoPair>(
            resolution.Dataset.Pairs.Count);
        foreach (var pair in resolution.Dataset.Pairs)
        {
            var projectPair = MapRepositoryPairToProjectPair(
                address,
                splitRole,
                pair);
            if (!projectPair.IsSuccess || projectPair.Value is null)
            {
                return projectPair.MapError<
                    CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>();
            }

            mapped.Add(projectPair.Value);
        }

        if (mapped.Count != ExpectedPairCount)
        {
            return OperationResult<
                CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>
                .Fail(
                    "Repository-owned CrossStack dataset must reopen exactly 50 rows.",
                    BatchSizeErrorCode);
        }

        var pairs = mapped.ToArray();
        var repositoryPairs = resolution.Dataset.Pairs
            .Select(static pair => pair with { })
            .ToArray();
        var wrongAnnotationRate = pairs.Count(pair =>
            pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) /
            (double)pairs.Length;
        var batch = new CrossStackBusinessFlowGraphDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            Warnings: []);

        // WHY: repository-owned rows are accepted only after strict disk
        // reopen. The public SourceKind therefore describes sealed local
        // corpus bytes, not the legacy deterministic baseline generator.
        var sourced =
            CrossStackBusinessFlowGraphBatchProvenanceProof
                .AttachRepositoryOwnedCorpus(batch, splitRole);

        return OperationResult<
            CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch>
            .Ok(new CrossStackBusinessFlowGraphRepositoryOwnedDpoDatasetTrainingBatch(
                sourced,
                address,
                repositoryPairs,
                resolution.Source,
                resolution.Dataset.Manifest.RequestIdentity,
                address.ManifestRelativePath,
                resolution.Dataset.Manifest.ManifestSha256,
                ComputeProjectPairsSha256(pairs)));
    }

    private static OperationResult<bool> ValidateRepositoryRowsForSplit(
        RepositoryOwnedDpoDatasetAddress address,
        DpoBatchSplitRole splitRole,
        IReadOnlyList<ContinuousLearning.DpoPair>? pairs)
    {
        if (pairs is null || pairs.Count != ExpectedPairCount)
        {
            return OperationResult<bool>.Fail(
                "CrossStack repository-owned DPO creator must return exactly 50 reconstructable rows before sealing.",
                BatchSizeErrorCode);
        }

        foreach (var pair in pairs)
        {
            var mapped = MapRepositoryPairToProjectPair(
                address,
                splitRole,
                pair);
            if (!mapped.IsSuccess)
                return mapped.MapError<bool>();
        }

        return OperationResult<bool>.Ok(true);
    }

    private static OperationResult<ProjectPairCarrier> ParseCarrier(
        string frame)
    {
        if (!IsBoundedText(frame))
        {
            return OperationResult<ProjectPairCarrier>.Fail(
                "Repository-owned CrossStack row is missing its framed prompt carrier.",
                InvalidErrorCode);
        }

        try
        {
            using var document = JsonDocument.Parse(frame, StrictJson);
            var root = document.RootElement;
            if (root.ValueKind is not JsonValueKind.Object ||
                root.EnumerateObject().Count() != 8)
            {
                return OperationResult<ProjectPairCarrier>.Fail(
                    "Repository-owned CrossStack carrier has an invalid shape.",
                    InvalidErrorCode);
            }

            var schema = ReadString(root, "schema");
            var splitRoleText = ReadString(root, "split_role");
            var pairId = ReadString(root, "pair_id");
            var a = ReadString(root, "a");
            var rejectedType = ReadString(root, "rejected_type");
            var requiredPathIds = ReadStringArray(root, "required_path_ids");
            var evidenceIds = ReadStringArray(root, "evidence_ids");
            if (!root.TryGetProperty(
                    "boundary_map",
                    out var boundaryElement))
            {
                return OperationResult<ProjectPairCarrier>.Fail(
                    "Repository-owned CrossStack carrier is missing its boundary map.",
                    InvalidErrorCode);
            }

            var boundaryMap = ReadBoundaryMap(boundaryElement);
            if (!StringComparer.Ordinal.Equals(schema, PairCarrierSchema) ||
                !Enum.TryParse<DpoBatchSplitRole>(
                    splitRoleText,
                    ignoreCase: false,
                    out var splitRole) ||
                splitRole is not DpoBatchSplitRole.Training and
                    not DpoBatchSplitRole.HeldOut ||
                !IsBoundedText(pairId) ||
                !IsBoundedText(a) ||
                !RejectedTypes.Contains(rejectedType, StringComparer.Ordinal) ||
                boundaryMap is null ||
                requiredPathIds is null ||
                requiredPathIds.Length == 0 ||
                requiredPathIds.Any(static item => !IsBoundedText(item)) ||
                evidenceIds is null ||
                evidenceIds.Length == 0 ||
                evidenceIds.Any(static item => !IsBoundedText(item)))
            {
                return OperationResult<ProjectPairCarrier>.Fail(
                    "Repository-owned CrossStack carrier fields are invalid.",
                    InvalidErrorCode);
            }

            return OperationResult<ProjectPairCarrier>.Ok(
                new ProjectPairCarrier(
                    splitRole,
                    pairId!,
                    boundaryMap,
                    a!,
                    rejectedType!,
                    requiredPathIds,
                    evidenceIds));
        }
        catch (JsonException)
        {
            return OperationResult<ProjectPairCarrier>.Fail(
                "Repository-owned CrossStack carrier is not strict JSON.",
                InvalidErrorCode);
        }
    }

    private static string BuildCarrier(
        CrossStackBusinessFlowGraphDpoPair pair,
        DpoBatchSplitRole splitRole)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            // WHY: the shared rich DpoPair stores prompt/chosen/rejected but
            // has no CrossStack-specific fields. This strict JSON carrier keeps
            // pair id, split, boundary map, required paths, and evidence ids
            // typed so restart reads never scrape prose to rebuild the batch.
            writer.WriteStartObject();
            writer.WriteString("schema", PairCarrierSchema);
            writer.WriteString("split_role", splitRole.ToString());
            writer.WriteString("pair_id", pair.PairId);
            writer.WritePropertyName("boundary_map");
            WriteBoundaryMap(writer, pair.BoundaryMap);
            writer.WriteString("a", pair.A);
            writer.WriteString("rejected_type", pair.RejectedType);
            WriteStringArray(writer, "required_path_ids", pair.RequiredPathIds);
            WriteStringArray(writer, "evidence_ids", pair.EvidenceIds);
            writer.WriteEndObject();
            writer.Flush();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static void WriteBoundaryMap(
        Utf8JsonWriter writer,
        SystemBoundaryMapResult boundary)
    {
        writer.WriteStartObject();
        writer.WriteString("system_id", boundary.SystemId);
        WriteStringArray(writer, "project_ids", boundary.ProjectIds);
        writer.WriteStartArray("edges");
        foreach (var edge in boundary.Edges)
        {
            writer.WriteStartObject();
            writer.WriteString("edge_id", edge.EdgeId);
            writer.WriteString("source_project_id", edge.SourceProjectId);
            writer.WriteString("target_project_id", edge.TargetProjectId);
            writer.WriteString("boundary_kind", edge.BoundaryKind);
            WriteStringArray(writer, "shared_markers", edge.SharedMarkers);
            WriteStringArray(writer, "evidence_ids", edge.EvidenceIds);
            writer.WriteNumber("confidence", edge.Confidence);
            writer.WriteString("status", edge.Status);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
        WriteStringArray(
            writer,
            "unknown_boundaries",
            boundary.UnknownBoundaries);
        WriteStringArray(
            writer,
            "required_follow_up_evidence",
            boundary.RequiredFollowUpEvidence);
        writer.WriteNumber("confidence", boundary.Confidence);
        WriteStringArray(writer, "warnings", boundary.Warnings);
        writer.WriteEndObject();
    }

    private static SystemBoundaryMapResult? ReadBoundaryMap(JsonElement value)
    {
        if (value.ValueKind is not JsonValueKind.Object ||
            value.EnumerateObject().Count() != 7)
        {
            return null;
        }

        var systemId = ReadString(value, "system_id");
        var projectIds = ReadStringArray(value, "project_ids");
        var edges = ReadBoundaryEdges(value);
        var unknownBoundaries = ReadStringArray(value, "unknown_boundaries");
        var requiredFollowUpEvidence =
            ReadStringArray(value, "required_follow_up_evidence");
        var confidence = ReadDouble(value, "confidence");
        var warnings = ReadStringArray(value, "warnings");
        var boundary = new SystemBoundaryMapResult(
            systemId ?? "",
            projectIds ?? [],
            edges ?? [],
            unknownBoundaries ?? [],
            requiredFollowUpEvidence ?? [],
            confidence ?? double.NaN,
            warnings ?? []);
        return IsValidBoundaryMap(boundary) ? boundary : null;
    }

    private static SystemBoundaryEdge[]? ReadBoundaryEdges(JsonElement root)
    {
        if (!root.TryGetProperty("edges", out var value) ||
            value.ValueKind is not JsonValueKind.Array)
        {
            return null;
        }

        var edges = new List<SystemBoundaryEdge>();
        foreach (var item in value.EnumerateArray())
        {
            if (item.ValueKind is not JsonValueKind.Object ||
                item.EnumerateObject().Count() != 8)
            {
                return null;
            }

            var confidence = ReadDouble(item, "confidence");
            if (confidence is null)
                return null;

            edges.Add(new SystemBoundaryEdge(
                ReadString(item, "edge_id") ?? "",
                ReadString(item, "source_project_id") ?? "",
                ReadString(item, "target_project_id") ?? "",
                ReadString(item, "boundary_kind") ?? "",
                ReadStringArray(item, "shared_markers") ?? [],
                ReadStringArray(item, "evidence_ids") ?? [],
                confidence.Value,
                ReadString(item, "status") ?? ""));
        }

        return edges.ToArray();
    }

    private static void WriteStringArray(
        Utf8JsonWriter writer,
        string propertyName,
        IEnumerable<string> values)
    {
        writer.WriteStartArray(propertyName);
        foreach (var value in values)
            writer.WriteStringValue(value);
        writer.WriteEndArray();
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
        // WHY: repository-owned CrossStack rows must come from an admitted
        // local open-teacher seam on both trainable sides. The deterministic
        // baseline generator can still build eval fixtures, but a mixed
        // DeterministicGenerator/OpenTeacher row is not a real corpus row for
        // this local-teacher route.
        teacherProvenance is
        {
            SourceKind: TrainingTeacherSourceKind.ExternalTeacherOutput,
            ClosedProprietaryModel: false,
            TeacherOutputUsedForTraining: true,
            Family: not null
        } &&
        chosenAuthor is DpoPairSideAuthorKind.OpenTeacher &&
        rejectedAuthor is DpoPairSideAuthorKind.OpenTeacher;

    private static bool HasAdmittedRepositoryPairAuthorship(
        ContinuousLearning.DpoPair pair)
    {
        var provenance = pair.TeacherProvenance;
        return IsRepositoryOwnedOpenTeacherProvenance(provenance) &&
               IsSameOpenTeacherSide(pair.ChosenAuthorship, provenance!) &&
               IsSameOpenTeacherSide(pair.RejectedAuthorship, provenance!);
    }

    private static bool IsRepositoryOwnedOpenTeacherProvenance(
        TrainingTeacherProvenance? provenance)
        => provenance is
           {
               SourceKind: TrainingTeacherSourceKind.ExternalTeacherOutput,
               ClosedProprietaryModel: false,
               TeacherOutputUsedForTraining: true,
               Family: not null
           } &&
           IsCanonicalSha256Hex(provenance.TrainingContentSha256);

    private static bool IsSameOpenTeacherSide(
        DpoPairSideAuthorship? authorship,
        TrainingTeacherProvenance pairPassport)
        =>
        // WHY: reopened repository rows are already content-bound by the
        // common sealer. CrossStack adds the stricter route rule: each side
        // must carry that same bound OpenTeacher passport, not a deterministic
        // or user/local side beside a teacher-authored side.
        authorship is
        {
            AuthorKind: DpoPairSideAuthorKind.OpenTeacher,
            OpenTeacherPassport: not null
        } &&
        EqualityComparer<TrainingTeacherProvenance?>.Default.Equals(
            authorship.OpenTeacherPassport,
            pairPassport);

    private static IReadOnlyList<string> BuildGovernanceEvidenceRefs(
        RepositoryOwnedDpoDatasetAddress address,
        DpoBatchSplitRole splitRole,
        CrossStackBusinessFlowGraphDpoPair pair)
        =>
        [
            "sha256:" + ComputeTextSha256(
                "cross-stack-business-flow-graph-repository-dataset\n" +
                address.ManifestRelativePath),
            "sha256:" + ComputeTextSha256("split-role\n" + splitRole),
            "sha256:" + ComputeTextSha256("pair-id\n" + pair.PairId),
            "sha256:" + ComputeTextSha256(
                "system-id\n" + pair.BoundaryMap.SystemId),
            "sha256:" + ComputeTextSha256(
                "rejected-type\n" + pair.RejectedType),
            "sha256:" + ComputeTextSha256(
                "required-paths\n" +
                string.Join("\n", pair.RequiredPathIds)),
            "sha256:" + ComputeTextSha256(
                "evidence\n" + string.Join("\n", pair.EvidenceIds))
        ];

    private static bool IsValidProjectPair(
        CrossStackBusinessFlowGraphDpoPair? pair)
        => pair is not null &&
           IsBoundedText(pair.PairId) &&
           IsValidBoundaryMap(pair.BoundaryMap) &&
           IsBoundedText(pair.A) &&
           IsBoundedText(pair.Chosen) &&
           IsBoundedText(pair.Rejected) &&
           RejectedTypes.Contains(pair.RejectedType, StringComparer.Ordinal) &&
           pair.RequiredPathIds is { Length: > 0 } &&
           pair.RequiredPathIds.All(static item => IsBoundedText(item)) &&
           pair.EvidenceIds is { Length: > 0 } &&
           pair.EvidenceIds.All(static item => IsBoundedText(item)) &&
           !string.Equals(
               pair.Chosen.Trim(),
               pair.Rejected.Trim(),
               StringComparison.Ordinal);

    private static bool IsValidBoundaryMap(SystemBoundaryMapResult? boundary)
        => boundary is not null &&
           IsBoundedText(boundary.SystemId) &&
           boundary.ProjectIds is { Length: > 0 } &&
           boundary.ProjectIds.All(static item => IsBoundedText(item)) &&
           boundary.Edges is { Length: > 0 } &&
           boundary.Edges.All(IsValidBoundaryEdge) &&
           boundary.UnknownBoundaries is not null &&
           boundary.UnknownBoundaries.All(static item => IsBoundedText(item)) &&
           boundary.RequiredFollowUpEvidence is not null &&
           boundary.RequiredFollowUpEvidence.All(
               static item => IsBoundedText(item)) &&
           IsFiniteUnitInterval(boundary.Confidence) &&
           boundary.Warnings is not null &&
           boundary.Warnings.All(static item => IsBoundedText(item));

    private static bool IsValidBoundaryEdge(SystemBoundaryEdge? edge)
        => edge is not null &&
           IsBoundedText(edge.EdgeId) &&
           IsBoundedText(edge.SourceProjectId) &&
           IsBoundedText(edge.TargetProjectId) &&
           IsBoundedText(edge.BoundaryKind) &&
           edge.SharedMarkers is { Length: > 0 } &&
           edge.SharedMarkers.All(static item => IsBoundedText(item)) &&
           edge.EvidenceIds is { Length: > 0 } &&
           edge.EvidenceIds.All(static item => IsBoundedText(item)) &&
           IsFiniteUnitInterval(edge.Confidence) &&
           IsBoundedText(edge.Status);

    private static bool IsFiniteUnitInterval(double value)
        => !double.IsNaN(value) &&
           !double.IsInfinity(value) &&
           value is >= 0 and <= 1;

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

    private static double? ReadDouble(
        JsonElement root,
        string propertyName)
        => root.TryGetProperty(propertyName, out var value) &&
           value.ValueKind is JsonValueKind.Number &&
           value.TryGetDouble(out var number)
            ? number
            : null;

    private static bool SameAddress(
        RepositoryOwnedDpoDatasetAddress first,
        RepositoryOwnedDpoDatasetAddress second)
        => string.Equals(
               first.ManifestPath,
               second.ManifestPath,
               OperatingSystem.IsWindows()
                   ? StringComparison.OrdinalIgnoreCase
                   : StringComparison.Ordinal) &&
           string.Equals(
               first.DomainSlug,
               second.DomainSlug,
               StringComparison.Ordinal) &&
           string.Equals(
               first.DesignPatternSlug,
               second.DesignPatternSlug,
               StringComparison.Ordinal) &&
           string.Equals(
               first.DatasetVersion,
               second.DatasetVersion,
               StringComparison.Ordinal);

    private static bool SameRequestIdentity(
        RepositoryOwnedDpoDatasetRequestIdentity first,
        RepositoryOwnedDpoDatasetRequestIdentity second)
        => RepositoryOwnedDpoDatasetManifestSealer
               .IsValidRequestIdentity(first) &&
           RepositoryOwnedDpoDatasetManifestSealer
               .IsValidRequestIdentity(second) &&
           string.Equals(
               first.RequestIdentitySha256,
               second.RequestIdentitySha256,
               StringComparison.Ordinal);

    private static bool IsCanonicalSha256Hex(string? value)
        => value is not null &&
           value.Length == 64 &&
           value.All(static character =>
               character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static string ComputeTextSha256(string value)
        => ComputeSha256(Encoding.UTF8.GetBytes(value));

    private static string ComputeSha256(byte[] bytes)
        => Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    private static readonly string[] RejectedTypes =
    [
        "ui_api_only",
        "server_sql_only",
        "procedure_without_result_shape",
        "separate_repositories",
        "incomplete_cross_stack_business_flow"
    ];

    private sealed record ProjectPairCarrier(
        DpoBatchSplitRole SplitRole,
        string PairId,
        SystemBoundaryMapResult BoundaryMap,
        string A,
        string RejectedType,
        string[] RequiredPathIds,
        string[] EvidenceIds);
}
