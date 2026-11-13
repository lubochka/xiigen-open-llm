// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ContinuousLearning;

/// <summary>
/// One typed decomposition step. Description is retained only for auditability:
/// matching authority comes from step id, order, phase tag, and dependencies.
/// </summary>
public sealed record OrchestrationReferenceStep(
    string StepId,
    int Order,
    string PhaseTag,
    string Description);

/// <summary>
/// Exact dependency edge between typed step ids. Edges are a set: canonical
/// serialization sorts them, while validation rejects duplicates, bad
/// endpoints, cycles, and edges that do not point topologically forward by
/// step order.
/// </summary>
public sealed record OrchestrationReferenceDependencyEdge(
    string FromStepId,
    string ToStepId);

/// <summary>
/// Canonical frozen-reference payload for programming-task orchestration
/// decomposition. It is a measurement payload, not a teacher/model/training
/// output and not a score supplied by the caller.
/// </summary>
public sealed record OrchestrationReferenceDecompositionPayload
{
    public const string Schema =
        "xiigen.orchestration-reference-decomposition.v1";
    public const string DependencyOrderInvalidErrorCode =
        "ORCHESTRATION_REFERENCE_DEPENDENCY_ORDER_INVALID";
    public const string DependencyCycleErrorCode =
        "ORCHESTRATION_REFERENCE_DEPENDENCY_CYCLE";

    private const int MaximumPayloadChars = 131_072;
    private const int MaximumIdentityChars = 256;
    private const int MaximumDescriptionChars = 4_096;
    private const int MaximumStepCount = 256;
    private const int MaximumDependencyCount = 2_048;

    private static readonly JsonDocumentOptions StrictJsonDocumentOptions = new()
    {
        AllowTrailingCommas = false,
        CommentHandling = JsonCommentHandling.Disallow,
        MaxDepth = 8
    };

    private static readonly JsonWriterOptions CanonicalJsonWriterOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Indented = false
    };

    private readonly string _canonicalString;

    private OrchestrationReferenceDecompositionPayload(
        string domainTag,
        string subjectId,
        string patternId,
        string role,
        string referenceCaseId,
        string scenarioId,
        string sourceEntityFingerprint,
        IReadOnlyList<OrchestrationReferenceStep> steps,
        IReadOnlyList<OrchestrationReferenceDependencyEdge> dependencyEdges,
        string canonicalString)
    {
        DomainTag = domainTag;
        SubjectId = subjectId;
        PatternId = patternId;
        Role = role;
        ReferenceCaseId = referenceCaseId;
        ScenarioId = scenarioId;
        SourceEntityFingerprint = sourceEntityFingerprint;
        Steps = steps;
        DependencyEdges = dependencyEdges;
        _canonicalString = canonicalString;
    }

    public string DomainTag { get; }

    public string SubjectId { get; }

    public string PatternId { get; }

    public string Role { get; }

    public string ReferenceCaseId { get; }

    public string ScenarioId { get; }

    public string SourceEntityFingerprint { get; }

    public IReadOnlyList<OrchestrationReferenceStep> Steps { get; }

    public IReadOnlyList<OrchestrationReferenceDependencyEdge> DependencyEdges { get; }

    public IReadOnlyList<string> StepIdsInOrder
        => Steps.Select(static step => step.StepId).ToArray();

    public IReadOnlyList<string> StepIdSet
        => Steps
            .Select(static step => step.StepId)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

    public IReadOnlyList<string> PhaseTagSet
        => Steps
            .Select(static step => step.PhaseTag)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

    public IReadOnlyDictionary<string, string> StepPhaseAssignments
        => Steps.ToDictionary(
            static step => step.StepId,
            static step => step.PhaseTag,
            StringComparer.Ordinal);

    public IReadOnlyList<string> DependencyKeys
        => DependencyEdges
            .Select(static edge => EdgeKey(edge.FromStepId, edge.ToStepId))
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

    public static OperationResult<OrchestrationReferenceDecompositionPayload> Create(
        string domainTag,
        string subjectId,
        string patternId,
        string role,
        string referenceCaseId,
        string scenarioId,
        string sourceEntityFingerprint,
        IEnumerable<OrchestrationReferenceStep?> steps,
        IEnumerable<OrchestrationReferenceDependencyEdge?> dependencyEdges)
    {
        var normalizedDomain = NormalizeIdentity(domainTag, "ORCHESTRATION_REFERENCE_DOMAIN_INVALID");
        var normalizedSubject = NormalizeIdentity(subjectId, "ORCHESTRATION_REFERENCE_SUBJECT_INVALID");
        var normalizedPattern = NormalizeIdentity(patternId, "ORCHESTRATION_REFERENCE_PATTERN_INVALID");
        var normalizedRole = NormalizeIdentity(role, "ORCHESTRATION_REFERENCE_ROLE_INVALID");
        var normalizedReferenceCaseId = NormalizeIdentity(
            referenceCaseId,
            "ORCHESTRATION_REFERENCE_CASE_ID_INVALID");
        var normalizedScenarioId = NormalizeIdentity(
            scenarioId,
            "ORCHESTRATION_REFERENCE_SCENARIO_ID_INVALID");
        var normalizedSourceFingerprint = NormalizeSha256(
            sourceEntityFingerprint,
            "ORCHESTRATION_REFERENCE_SOURCE_FINGERPRINT_INVALID");
        if (!normalizedDomain.IsSuccess || normalizedDomain.Value is null)
            return normalizedDomain.MapError<OrchestrationReferenceDecompositionPayload>();
        if (!normalizedSubject.IsSuccess || normalizedSubject.Value is null)
            return normalizedSubject.MapError<OrchestrationReferenceDecompositionPayload>();
        if (!normalizedPattern.IsSuccess || normalizedPattern.Value is null)
            return normalizedPattern.MapError<OrchestrationReferenceDecompositionPayload>();
        if (!normalizedRole.IsSuccess || normalizedRole.Value is null)
            return normalizedRole.MapError<OrchestrationReferenceDecompositionPayload>();
        if (!normalizedReferenceCaseId.IsSuccess || normalizedReferenceCaseId.Value is null)
            return normalizedReferenceCaseId.MapError<OrchestrationReferenceDecompositionPayload>();
        if (!normalizedScenarioId.IsSuccess || normalizedScenarioId.Value is null)
            return normalizedScenarioId.MapError<OrchestrationReferenceDecompositionPayload>();
        if (!normalizedSourceFingerprint.IsSuccess || normalizedSourceFingerprint.Value is null)
            return normalizedSourceFingerprint.MapError<OrchestrationReferenceDecompositionPayload>();

        var normalizedSteps = NormalizeSteps(steps);
        if (!normalizedSteps.IsSuccess || normalizedSteps.Value is null)
            return normalizedSteps.MapError<OrchestrationReferenceDecompositionPayload>();
        var normalizedEdges = NormalizeDependencyEdges(
            dependencyEdges,
            normalizedSteps.Value);
        if (!normalizedEdges.IsSuccess || normalizedEdges.Value is null)
            return normalizedEdges.MapError<OrchestrationReferenceDecompositionPayload>();

        var canonical = WriteCanonicalString(
            normalizedDomain.Value,
            normalizedSubject.Value,
            normalizedPattern.Value,
            normalizedRole.Value,
            normalizedReferenceCaseId.Value,
            normalizedScenarioId.Value,
            normalizedSourceFingerprint.Value,
            normalizedSteps.Value,
            normalizedEdges.Value);
        if (canonical.Length > MaximumPayloadChars)
        {
            return Failure(
                "Orchestration reference decomposition payload exceeds the frozen-reference content limit.",
                "ORCHESTRATION_REFERENCE_PAYLOAD_TOO_LARGE");
        }

        return OperationResult<OrchestrationReferenceDecompositionPayload>.Ok(
            new OrchestrationReferenceDecompositionPayload(
                normalizedDomain.Value,
                normalizedSubject.Value,
                normalizedPattern.Value,
                normalizedRole.Value,
                normalizedReferenceCaseId.Value,
                normalizedScenarioId.Value,
                normalizedSourceFingerprint.Value,
                normalizedSteps.Value,
                normalizedEdges.Value,
                canonical));
    }

    public static OperationResult<OrchestrationReferenceDecompositionPayload> Parse(
        string payload)
    {
        var normalizedPayload = NormalizePayloadContent(payload);
        if (!normalizedPayload.IsSuccess || normalizedPayload.Value is null)
            return normalizedPayload.MapError<OrchestrationReferenceDecompositionPayload>();
        if (normalizedPayload.Value.Length > MaximumPayloadChars)
        {
            return Failure(
                "Orchestration reference decomposition payload exceeds the frozen-reference content limit.",
                "ORCHESTRATION_REFERENCE_PAYLOAD_TOO_LARGE");
        }

        try
        {
            using var document = JsonDocument.Parse(
                normalizedPayload.Value,
                StrictJsonDocumentOptions);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return Failure(
                    "Orchestration reference decomposition payload must be a JSON object.",
                    "ORCHESTRATION_REFERENCE_PAYLOAD_SHAPE_INVALID");
            }

            var properties = root.EnumerateObject().ToArray();
            if (properties.Length != 10 ||
                !StringComparer.Ordinal.Equals(properties[0].Name, "schema") ||
                !StringComparer.Ordinal.Equals(properties[1].Name, "domain_tag") ||
                !StringComparer.Ordinal.Equals(properties[2].Name, "subject_id") ||
                !StringComparer.Ordinal.Equals(properties[3].Name, "pattern_id") ||
                !StringComparer.Ordinal.Equals(properties[4].Name, "role") ||
                !StringComparer.Ordinal.Equals(properties[5].Name, "reference_case_id") ||
                !StringComparer.Ordinal.Equals(properties[6].Name, "scenario_id") ||
                !StringComparer.Ordinal.Equals(properties[7].Name, "source_entity_fingerprint") ||
                !StringComparer.Ordinal.Equals(properties[8].Name, "steps") ||
                !StringComparer.Ordinal.Equals(properties[9].Name, "dependency_edges"))
            {
                return Failure(
                    "Orchestration reference decomposition payload must use the canonical schema, domain_tag, subject_id, pattern_id, role, reference_case_id, scenario_id, source_entity_fingerprint, steps, dependency_edges property order with no extras.",
                    "ORCHESTRATION_REFERENCE_PAYLOAD_SHAPE_INVALID");
            }

            if (properties[0].Value.ValueKind != JsonValueKind.String ||
                !StringComparer.Ordinal.Equals(properties[0].Value.GetString(), Schema))
            {
                return Failure(
                    "Orchestration reference decomposition payload schema is not supported.",
                    "ORCHESTRATION_REFERENCE_PAYLOAD_SCHEMA_INVALID");
            }

            for (var index = 1; index <= 7; index++)
            {
                if (properties[index].Value.ValueKind != JsonValueKind.String)
                {
                    return Failure(
                        "Orchestration reference decomposition payload scalar fields must be strings.",
                        "ORCHESTRATION_REFERENCE_PAYLOAD_SHAPE_INVALID");
                }
            }

            var steps = ParseSteps(properties[8].Value);
            if (!steps.IsSuccess)
                return steps.MapError<OrchestrationReferenceDecompositionPayload>();
            var edges = ParseDependencyEdges(properties[9].Value);
            if (!edges.IsSuccess)
                return edges.MapError<OrchestrationReferenceDecompositionPayload>();

            return Create(
                properties[1].Value.GetString()!,
                properties[2].Value.GetString()!,
                properties[3].Value.GetString()!,
                properties[4].Value.GetString()!,
                properties[5].Value.GetString()!,
                properties[6].Value.GetString()!,
                properties[7].Value.GetString()!,
                steps.Value!,
                edges.Value!);
        }
        catch (JsonException)
        {
            return Failure(
                "Orchestration reference decomposition payload is malformed JSON.",
                "ORCHESTRATION_REFERENCE_PAYLOAD_MALFORMED");
        }
    }

    public string ToCanonicalString() => _canonicalString;

    public static string EdgeKey(string fromStepId, string toStepId)
        => fromStepId + "->" + toStepId;

    private static OperationResult<IReadOnlyList<OrchestrationReferenceStep?>> ParseSteps(
        JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceStep?>>.Fail(
                "Orchestration reference steps must be an array.",
                "ORCHESTRATION_REFERENCE_STEPS_SHAPE_INVALID");
        }

        var steps = new List<OrchestrationReferenceStep?>();
        foreach (var stepElement in element.EnumerateArray())
        {
            if (stepElement.ValueKind != JsonValueKind.Object)
            {
                return OperationResult<IReadOnlyList<OrchestrationReferenceStep?>>.Fail(
                    "Orchestration reference step must be an object.",
                    "ORCHESTRATION_REFERENCE_STEP_SHAPE_INVALID");
            }

            var properties = stepElement.EnumerateObject().ToArray();
            if (properties.Length != 4 ||
                !StringComparer.Ordinal.Equals(properties[0].Name, "step_id") ||
                !StringComparer.Ordinal.Equals(properties[1].Name, "order") ||
                !StringComparer.Ordinal.Equals(properties[2].Name, "phase_tag") ||
                !StringComparer.Ordinal.Equals(properties[3].Name, "description") ||
                properties[0].Value.ValueKind != JsonValueKind.String ||
                !properties[1].Value.TryGetInt32(out var order) ||
                properties[2].Value.ValueKind != JsonValueKind.String ||
                properties[3].Value.ValueKind != JsonValueKind.String)
            {
                return OperationResult<IReadOnlyList<OrchestrationReferenceStep?>>.Fail(
                    "Orchestration reference step must use the canonical step_id, order, phase_tag, description shape with no extras.",
                    "ORCHESTRATION_REFERENCE_STEP_SHAPE_INVALID");
            }

            steps.Add(new OrchestrationReferenceStep(
                properties[0].Value.GetString()!,
                order,
                properties[2].Value.GetString()!,
                properties[3].Value.GetString()!));
        }

        return OperationResult<IReadOnlyList<OrchestrationReferenceStep?>>.Ok(steps);
    }

    private static OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge?>>
        ParseDependencyEdges(JsonElement element)
    {
        if (element.ValueKind != JsonValueKind.Array)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge?>>.Fail(
                "Orchestration reference dependency edges must be an array.",
                "ORCHESTRATION_REFERENCE_DEPENDENCIES_SHAPE_INVALID");
        }

        var edges = new List<OrchestrationReferenceDependencyEdge?>();
        foreach (var edgeElement in element.EnumerateArray())
        {
            if (edgeElement.ValueKind != JsonValueKind.Object)
            {
                return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge?>>.Fail(
                    "Orchestration reference dependency edge must be an object.",
                    "ORCHESTRATION_REFERENCE_DEPENDENCY_SHAPE_INVALID");
            }

            var properties = edgeElement.EnumerateObject().ToArray();
            if (properties.Length != 2 ||
                !StringComparer.Ordinal.Equals(properties[0].Name, "from_step_id") ||
                !StringComparer.Ordinal.Equals(properties[1].Name, "to_step_id") ||
                properties[0].Value.ValueKind != JsonValueKind.String ||
                properties[1].Value.ValueKind != JsonValueKind.String)
            {
                return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge?>>.Fail(
                    "Orchestration reference dependency edge must use the canonical from_step_id, to_step_id shape with no extras.",
                    "ORCHESTRATION_REFERENCE_DEPENDENCY_SHAPE_INVALID");
            }

            edges.Add(new OrchestrationReferenceDependencyEdge(
                properties[0].Value.GetString()!,
                properties[1].Value.GetString()!));
        }

        return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge?>>.Ok(edges);
    }

    private static OperationResult<IReadOnlyList<OrchestrationReferenceStep>> NormalizeSteps(
        IEnumerable<OrchestrationReferenceStep?> steps)
    {
        if (steps is null)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Fail(
                "Orchestration reference steps are required.",
                "ORCHESTRATION_REFERENCE_STEPS_REQUIRED");
        }

        var snapshot = new List<OrchestrationReferenceStep>();
        try
        {
            foreach (var step in steps)
            {
                if (step is null)
                {
                    return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Fail(
                        "Orchestration reference step is required.",
                        "ORCHESTRATION_REFERENCE_STEP_REQUIRED");
                }

                var stepId = NormalizeIdentity(step.StepId, "ORCHESTRATION_REFERENCE_STEP_ID_INVALID");
                var phaseTag = NormalizeIdentity(step.PhaseTag, "ORCHESTRATION_REFERENCE_PHASE_TAG_INVALID");
                var description = NormalizeDescription(step.Description);
                if (!stepId.IsSuccess || stepId.Value is null)
                    return stepId.MapError<IReadOnlyList<OrchestrationReferenceStep>>();
                if (!phaseTag.IsSuccess || phaseTag.Value is null)
                    return phaseTag.MapError<IReadOnlyList<OrchestrationReferenceStep>>();
                if (!description.IsSuccess || description.Value is null)
                    return description.MapError<IReadOnlyList<OrchestrationReferenceStep>>();

                snapshot.Add(new OrchestrationReferenceStep(
                    stepId.Value,
                    step.Order,
                    phaseTag.Value,
                    description.Value));
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Fail(
                "Orchestration reference steps could not be snapshotted.",
                "ORCHESTRATION_REFERENCE_STEPS_SNAPSHOT_FAILED");
        }

        if (snapshot.Count == 0)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Fail(
                "Orchestration reference must contain at least one step.",
                "ORCHESTRATION_REFERENCE_STEPS_EMPTY");
        }

        if (snapshot.Count > MaximumStepCount)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Fail(
                "Orchestration reference contains too many steps.",
                "ORCHESTRATION_REFERENCE_STEPS_TOO_MANY");
        }

        var duplicateStepId = snapshot
            .GroupBy(static step => step.StepId, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1)
            ?.Key;
        if (duplicateStepId is not null)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Fail(
                "Orchestration reference step ids must be unique.",
                "ORCHESTRATION_REFERENCE_STEP_ID_DUPLICATE");
        }

        var duplicateOrder = snapshot
            .GroupBy(static step => step.Order)
            .FirstOrDefault(static group => group.Count() > 1)
            ?.Key;
        if (duplicateOrder is not null)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Fail(
                "Orchestration reference step orders must be unique.",
                "ORCHESTRATION_REFERENCE_STEP_ORDER_DUPLICATE");
        }

        var ordered = snapshot
            .OrderBy(static step => step.Order)
            .ThenBy(static step => step.StepId, StringComparer.Ordinal)
            .ToArray();
        for (var index = 0; index < ordered.Length; index++)
        {
            if (ordered[index].Order != index + 1)
            {
                return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Fail(
                    "Orchestration reference step orders must be contiguous from one.",
                    "ORCHESTRATION_REFERENCE_STEP_ORDER_NONCONTIGUOUS");
            }
        }

        return OperationResult<IReadOnlyList<OrchestrationReferenceStep>>.Ok(
            Array.AsReadOnly(ordered));
    }

    private static OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>
        NormalizeDependencyEdges(
            IEnumerable<OrchestrationReferenceDependencyEdge?> dependencyEdges,
            IReadOnlyList<OrchestrationReferenceStep> steps)
    {
        if (dependencyEdges is null)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                "Orchestration reference dependency edges are required.",
                "ORCHESTRATION_REFERENCE_DEPENDENCIES_REQUIRED");
        }

        var snapshot = new List<OrchestrationReferenceDependencyEdge>();
        try
        {
            foreach (var edge in dependencyEdges)
            {
                if (edge is null)
                {
                    return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                        "Orchestration reference dependency edge is required.",
                        "ORCHESTRATION_REFERENCE_DEPENDENCY_REQUIRED");
                }

                var fromStepId = NormalizeIdentity(
                    edge.FromStepId,
                    "ORCHESTRATION_REFERENCE_DEPENDENCY_ENDPOINT_INVALID");
                var toStepId = NormalizeIdentity(
                    edge.ToStepId,
                    "ORCHESTRATION_REFERENCE_DEPENDENCY_ENDPOINT_INVALID");
                if (!fromStepId.IsSuccess || fromStepId.Value is null)
                    return fromStepId.MapError<IReadOnlyList<OrchestrationReferenceDependencyEdge>>();
                if (!toStepId.IsSuccess || toStepId.Value is null)
                    return toStepId.MapError<IReadOnlyList<OrchestrationReferenceDependencyEdge>>();

                snapshot.Add(new OrchestrationReferenceDependencyEdge(
                    fromStepId.Value,
                    toStepId.Value));
            }
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                "Orchestration reference dependency edges could not be snapshotted.",
                "ORCHESTRATION_REFERENCE_DEPENDENCIES_SNAPSHOT_FAILED");
        }

        if (snapshot.Count > MaximumDependencyCount)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                "Orchestration reference contains too many dependency edges.",
                "ORCHESTRATION_REFERENCE_DEPENDENCIES_TOO_MANY");
        }

        var stepOrderById = steps.ToDictionary(
            static step => step.StepId,
            static step => step.Order,
            StringComparer.Ordinal);

        foreach (var edge in snapshot)
        {
            if (!stepOrderById.ContainsKey(edge.FromStepId) ||
                !stepOrderById.ContainsKey(edge.ToStepId))
            {
                return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                    "Orchestration reference dependency endpoints must exist in steps.",
                    "ORCHESTRATION_REFERENCE_DEPENDENCY_ENDPOINT_UNKNOWN");
            }

            if (StringComparer.Ordinal.Equals(edge.FromStepId, edge.ToStepId))
            {
                return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                    "Orchestration reference dependency edge cannot point to itself.",
                    "ORCHESTRATION_REFERENCE_DEPENDENCY_SELF_EDGE");
            }
        }

        var duplicateEdge = snapshot
            .GroupBy(static edge => EdgeKey(edge.FromStepId, edge.ToStepId), StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1)
            ?.Key;
        if (duplicateEdge is not null)
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                "Orchestration reference dependency edges must be unique.",
                "ORCHESTRATION_REFERENCE_DEPENDENCY_DUPLICATE");
        }

        if (ContainsDependencyCycle(snapshot, stepOrderById))
        {
            return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                "Orchestration reference dependency graph must be acyclic.",
                DependencyCycleErrorCode);
        }

        foreach (var edge in snapshot)
        {
            if (stepOrderById[edge.FromStepId] >= stepOrderById[edge.ToStepId])
            {
                return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Fail(
                    "Orchestration reference dependency edges must point from an earlier step to a later step.",
                    DependencyOrderInvalidErrorCode);
            }
        }

        return OperationResult<IReadOnlyList<OrchestrationReferenceDependencyEdge>>.Ok(
            Array.AsReadOnly(
                snapshot
                    .OrderBy(static edge => edge.FromStepId, StringComparer.Ordinal)
                    .ThenBy(static edge => edge.ToStepId, StringComparer.Ordinal)
                    .ToArray()));
    }

    private static bool ContainsDependencyCycle(
        IReadOnlyList<OrchestrationReferenceDependencyEdge> edges,
        IReadOnlyDictionary<string, int> stepOrderById)
    {
        var outgoing = stepOrderById.Keys.ToDictionary(
            static stepId => stepId,
            static _ => new List<string>(),
            StringComparer.Ordinal);
        var inboundCount = stepOrderById.Keys.ToDictionary(
            static stepId => stepId,
            static _ => 0,
            StringComparer.Ordinal);

        foreach (var edge in edges)
        {
            outgoing[edge.FromStepId].Add(edge.ToStepId);
            inboundCount[edge.ToStepId]++;
        }

        foreach (var targets in outgoing.Values)
            targets.Sort(StringComparer.Ordinal);

        var ready = new SortedSet<string>(
            inboundCount
                .Where(static item => item.Value == 0)
                .Select(static item => item.Key),
            StringComparer.Ordinal);
        var visitedCount = 0;
        while (ready.Count > 0)
        {
            var stepId = ready.Min!;
            ready.Remove(stepId);
            visitedCount++;

            foreach (var targetStepId in outgoing[stepId])
            {
                inboundCount[targetStepId]--;
                if (inboundCount[targetStepId] == 0)
                    ready.Add(targetStepId);
            }
        }

        return visitedCount != inboundCount.Count;
    }

    private static OperationResult<string> NormalizeIdentity(
        string? value,
        string errorCode)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return OperationResult<string>.Fail(
                "Orchestration reference identity is required.",
                errorCode);
        }

        string normalized;
        try
        {
            normalized = value.Normalize(NormalizationForm.FormC);
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "Orchestration reference identity must be valid Unicode.",
                errorCode);
        }

        if (!StringComparer.Ordinal.Equals(normalized, normalized.Trim()) ||
            normalized.Length > MaximumIdentityChars ||
            !char.IsAsciiLetterOrDigit(normalized[0]) ||
            normalized.Any(static character =>
                !char.IsAsciiLetterOrDigit(character) &&
                character is not ('-' or '_' or '.' or ':')))
        {
            return OperationResult<string>.Fail(
                "Orchestration reference identity is blank or unsafe.",
                errorCode);
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static OperationResult<string> NormalizeSha256(
        string? value,
        string errorCode)
    {
        if (value is not { Length: 71 } ||
            !value.StartsWith("sha256:", StringComparison.Ordinal) ||
            value.AsSpan("sha256:".Length).IndexOfAnyExcept("0123456789abcdef") >= 0)
        {
            return OperationResult<string>.Fail(
                "Orchestration reference fingerprint must be a canonical sha256 ref.",
                errorCode);
        }

        return OperationResult<string>.Ok(value);
    }

    private static OperationResult<string> NormalizePayloadContent(string? value)
    {
        if (value is null)
        {
            return OperationResult<string>.Fail(
                "Orchestration reference decomposition payload is required.",
                "ORCHESTRATION_REFERENCE_PAYLOAD_INVALID");
        }

        string normalized;
        try
        {
            normalized = value
                .Normalize(NormalizationForm.FormC)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "Orchestration reference decomposition payload must be valid Unicode.",
                "ORCHESTRATION_REFERENCE_PAYLOAD_INVALID");
        }

        if (normalized.Any(static character =>
                char.IsControl(character) && character is not ('\n' or '\t')))
        {
            return OperationResult<string>.Fail(
                "Orchestration reference decomposition payload content is unsafe.",
                "ORCHESTRATION_REFERENCE_PAYLOAD_INVALID");
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static OperationResult<string> NormalizeDescription(string? value)
    {
        if (value is null)
        {
            return OperationResult<string>.Fail(
                "Orchestration reference step description is required.",
                "ORCHESTRATION_REFERENCE_STEP_DESCRIPTION_INVALID");
        }

        string normalized;
        try
        {
            normalized = value
                .Normalize(NormalizationForm.FormC)
                .Replace("\r\n", "\n", StringComparison.Ordinal)
                .Replace('\r', '\n');
        }
        catch (ArgumentException)
        {
            return OperationResult<string>.Fail(
                "Orchestration reference step description must be valid Unicode.",
                "ORCHESTRATION_REFERENCE_STEP_DESCRIPTION_INVALID");
        }

        if (normalized.Length > MaximumDescriptionChars ||
            normalized.Any(static character =>
                char.IsControl(character) && character is not ('\n' or '\t')))
        {
            return OperationResult<string>.Fail(
                "Orchestration reference step description is unsafe or too large.",
                "ORCHESTRATION_REFERENCE_STEP_DESCRIPTION_INVALID");
        }

        return OperationResult<string>.Ok(normalized);
    }

    private static string WriteCanonicalString(
        string domainTag,
        string subjectId,
        string patternId,
        string role,
        string referenceCaseId,
        string scenarioId,
        string sourceEntityFingerprint,
        IReadOnlyList<OrchestrationReferenceStep> steps,
        IReadOnlyList<OrchestrationReferenceDependencyEdge> dependencyEdges)
    {
        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer, CanonicalJsonWriterOptions))
        {
            writer.WriteStartObject();
            writer.WriteString("schema", Schema);
            writer.WriteString("domain_tag", domainTag);
            writer.WriteString("subject_id", subjectId);
            writer.WriteString("pattern_id", patternId);
            writer.WriteString("role", role);
            writer.WriteString("reference_case_id", referenceCaseId);
            writer.WriteString("scenario_id", scenarioId);
            writer.WriteString("source_entity_fingerprint", sourceEntityFingerprint);
            writer.WritePropertyName("steps");
            writer.WriteStartArray();
            foreach (var step in steps)
            {
                writer.WriteStartObject();
                writer.WriteString("step_id", step.StepId);
                writer.WriteNumber("order", step.Order);
                writer.WriteString("phase_tag", step.PhaseTag);
                writer.WriteString("description", step.Description);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WritePropertyName("dependency_edges");
            writer.WriteStartArray();
            foreach (var edge in dependencyEdges)
            {
                writer.WriteStartObject();
                writer.WriteString("from_step_id", edge.FromStepId);
                writer.WriteString("to_step_id", edge.ToStepId);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(buffer.ToArray());
    }

    private static OperationResult<OrchestrationReferenceDecompositionPayload> Failure(
        string message,
        string code)
        => OperationResult<OrchestrationReferenceDecompositionPayload>.Fail(message, code);
}

public sealed record OrchestrationReferenceMatchResult(
    bool IsMatch,
    string Status,
    int CandidateStepCount,
    int ReferenceStepCount,
    int CandidateDependencyCount,
    int ReferenceDependencyCount);

/// <summary>
/// Frozen-reference matcher for actual programming-task decomposition. It does
/// not reuse <see cref="OrchestrationAcceptanceGateEvaluator"/> because that
/// gate feeds expected steps into a keyword planner; this rule compares the
/// candidate decomposition payload against frozen paid-reference bytes.
/// </summary>
public static class OrchestrationReferenceMatchingRule
{
    public const string DomainTag = "programming-task-orchestration";
    public const string SubjectId = "orchestration-task-decomposition";
    public const string PatternId = "ilearning-orchestrator";
    public const string StudentCandidateRole = "student_candidate";
    public const string PaidReferenceRole = "paid_reference";
    public const string RuleId = "orchestration-task-decomposition-reference-match";
    public const string RuleVersion =
        "schema-xiigen.orchestration-reference-decomposition.v1.domain-programming-task-orchestration.subject-orchestration-task-decomposition.pattern-ilearning-orchestrator.roles-student_candidate-paid_reference.policy-exact-bindings-order-dag-forward-deps-phases";
    public const string RuleIdentityMismatchErrorCode =
        "ORCHESTRATION_REFERENCE_MATCHING_RULE_IDENTITY_MISMATCH";
    public const string CorpusSubjectMismatchErrorCode =
        "ORCHESTRATION_REFERENCE_MATCHING_CORPUS_SUBJECT_MISMATCH";
    public const string MatchStatus = "match";
    public const string EnvelopeMismatchStatus = "schema_domain_subject_pattern_mismatch";
    public const string RoleMismatchStatus = "role_mismatch_or_swapped";
    public const string ReferenceBindingMismatchStatus = "reference_binding_mismatch";
    public const string StepCoverageMismatchStatus = "step_id_coverage_mismatch";
    public const string StepOrderMismatchStatus = "ordered_step_sequence_mismatch";
    public const string DependencyMismatchStatus = "dependency_edge_set_mismatch";
    public const string PhaseTagSetMismatchStatus = "phase_tag_set_mismatch";
    public const string StepPhaseAssignmentMismatchStatus = "step_to_phase_assignment_mismatch";

    public static OperationResult<ReferenceMatchingRuleIdentity> CreateIdentity()
        => ReferenceMatchingRuleIdentity.Create(
            DomainTag,
            RuleId,
            RuleVersion,
            "Programming-task orchestration frozen-reference decomposition matcher for schema xiigen.orchestration-reference-decomposition.v1. It compares actual candidate decomposition JSON against a paid_reference JSON payload using exact non-circular reference_case_id, scenario_id, source_entity_fingerprint, step id coverage, ordered step sequence, DAG/topologically-forward dependency edge set, phase-tag set, and step-to-phase assignment. Step descriptions are audit prose, not authority. It does not use OrchestrationAcceptanceGateEvaluator as authority, does not accept caller self-score fields, and is not a paid corpus, DPO, model, training, checkpoint, or quality claim.");

    public static ReferenceMatchingRulePredicate CreatePredicate()
        => IsMatch;

    public static bool IsMatch(
        string candidateAnswer,
        string referenceAnswer)
    {
        var result = EvaluateMatch(candidateAnswer, referenceAnswer);
        return result.IsSuccess && result.Value is { IsMatch: true };
    }

    public static OperationResult<OrchestrationReferenceMatchResult> EvaluateMatch(
        string candidateAnswer,
        string referenceAnswer)
    {
        var candidate = OrchestrationReferenceDecompositionPayload.Parse(candidateAnswer);
        if (!candidate.IsSuccess || candidate.Value is null)
            return candidate.MapError<OrchestrationReferenceMatchResult>();
        var reference = OrchestrationReferenceDecompositionPayload.Parse(referenceAnswer);
        if (!reference.IsSuccess || reference.Value is null)
            return reference.MapError<OrchestrationReferenceMatchResult>();

        var status = ResolveStatus(candidate.Value, reference.Value);
        return OperationResult<OrchestrationReferenceMatchResult>.Ok(
            new OrchestrationReferenceMatchResult(
                StringComparer.Ordinal.Equals(status, MatchStatus),
                status,
                candidate.Value.Steps.Count,
                reference.Value.Steps.Count,
                candidate.Value.DependencyEdges.Count,
                reference.Value.DependencyEdges.Count));
    }

    public static OperationResult<ReferenceMatchingRuleCalibrationResult>
        EvaluateCalibration(ReferenceMatchingCalibrationInput input)
    {
        if (input is null)
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Orchestration reference matching calibration input is required.",
                "ORCHESTRATION_REFERENCE_MATCHING_CALIBRATION_INPUT_REQUIRED");
        }

        var expectedIdentity = CreateIdentity();
        if (!expectedIdentity.IsSuccess || expectedIdentity.Value is null)
            return expectedIdentity.MapError<ReferenceMatchingRuleCalibrationResult>();

        if (!StringComparer.Ordinal.Equals(
                input.RuleIdentity.IdentitySha256,
                expectedIdentity.Value.IdentitySha256))
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input rule identity does not match the orchestration reference matching policy.",
                RuleIdentityMismatchErrorCode);
        }

        if (!StringComparer.Ordinal.Equals(input.Corpus.SubjectId, SubjectId))
        {
            return OperationResult<ReferenceMatchingRuleCalibrationResult>.Fail(
                "Calibration input frozen corpus subject does not match the orchestration reference matching policy.",
                CorpusSubjectMismatchErrorCode);
        }

        return ReferenceMatchingRuleCalibrationEvaluator.Evaluate(
            input,
            CreatePredicate());
    }

    private static string ResolveStatus(
        OrchestrationReferenceDecompositionPayload candidate,
        OrchestrationReferenceDecompositionPayload reference)
    {
        if (!HasExpectedEnvelope(candidate) || !HasExpectedEnvelope(reference))
            return EnvelopeMismatchStatus;

        if (!StringComparer.Ordinal.Equals(candidate.Role, StudentCandidateRole) ||
            !StringComparer.Ordinal.Equals(reference.Role, PaidReferenceRole))
        {
            return RoleMismatchStatus;
        }

        if (!StringComparer.Ordinal.Equals(candidate.ReferenceCaseId, reference.ReferenceCaseId) ||
            !StringComparer.Ordinal.Equals(candidate.ScenarioId, reference.ScenarioId) ||
            !StringComparer.Ordinal.Equals(candidate.SourceEntityFingerprint, reference.SourceEntityFingerprint))
        {
            return ReferenceBindingMismatchStatus;
        }

        if (!EqualSequence(candidate.StepIdSet, reference.StepIdSet))
            return StepCoverageMismatchStatus;

        if (!EqualSequence(candidate.StepIdsInOrder, reference.StepIdsInOrder))
            return StepOrderMismatchStatus;

        if (!EqualSequence(candidate.DependencyKeys, reference.DependencyKeys))
            return DependencyMismatchStatus;

        if (!EqualSequence(candidate.PhaseTagSet, reference.PhaseTagSet))
            return PhaseTagSetMismatchStatus;

        if (!EqualAssignments(candidate.StepPhaseAssignments, reference.StepPhaseAssignments))
            return StepPhaseAssignmentMismatchStatus;

        return MatchStatus;
    }

    private static bool HasExpectedEnvelope(OrchestrationReferenceDecompositionPayload payload)
        => StringComparer.Ordinal.Equals(payload.DomainTag, DomainTag) &&
           StringComparer.Ordinal.Equals(payload.SubjectId, SubjectId) &&
           StringComparer.Ordinal.Equals(payload.PatternId, PatternId);

    private static bool EqualSequence(
        IReadOnlyList<string> left,
        IReadOnlyList<string> right)
        => left.Count == right.Count &&
           left.Zip(right).All(static pair =>
               StringComparer.Ordinal.Equals(pair.First, pair.Second));

    private static bool EqualAssignments(
        IReadOnlyDictionary<string, string> left,
        IReadOnlyDictionary<string, string> right)
    {
        if (left.Count != right.Count)
            return false;

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var rightPhase) ||
                !StringComparer.Ordinal.Equals(pair.Value, rightPhase))
            {
                return false;
            }
        }

        return true;
    }
}
