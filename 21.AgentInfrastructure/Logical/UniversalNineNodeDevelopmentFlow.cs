// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This record carries the compact state passed between generic development-flow stations.
public sealed record UniversalNodeState(
    string SubPhaseId,
    string PhaseId,
    string TaskType,
    string OriginalPrompt,
    string CurrentIdea,
    string[] CompletedNodeIds,
    string[] PredicateIds,
    string[] EvidenceIds,
    string[] Warnings);

// This generic station gives every development sub-phase the same replaceable logical-unit surface.
public sealed class UniversalDevelopmentNodeUnit : ILogicalUnit<UniversalNodeState, UniversalNodeState>
{
    // Create one generic node from its canonical node identity.
    public UniversalDevelopmentNodeUnit(string nodeId, string role)
    {
        NodeId = nodeId;
        Role = role;
    }

    // The canonical node id, for example node-01-domain-splitter.
    public string NodeId { get; }

    // The canonical node role, for example domain-splitter.
    public string Role { get; }

    // Execute one generic node pass.
    public Task<OperationResult<LogicalUnitResult<UniversalNodeState>>> ExecuteAsync(
        LogicalUnitRequest<UniversalNodeState> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail("Cancelled.", "CANCELLED"));

        // Reject missing state before appending node evidence.
        if (request.Input is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail("Universal node state is required.", "UNIVERSAL_NODE_STATE_REQUIRED"));

        // Append this node to the visible execution chain.
        var completedNodeIds = request.Input.CompletedNodeIds
            .Concat([NodeId])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        // Add node-local evidence id so the flow proves each station executed.
        var evidenceIds = request.Input.EvidenceIds
            .Concat([$"{request.RunId}:evidence-{request.Input.SubPhaseId}-{NodeId}"])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        // Apply the canonical node transformation through the production compiler/orchestrator helpers.
        var transformation = UniversalNodeTransformations.Transform(request.Input, NodeId, Role);
        var transformationEvidenceIds = transformation.EvidenceIds
            .Select(evidenceId => $"{request.RunId}:{evidenceId}")
            .ToArray();
        var nextIdea = $"{request.Input.CurrentIdea} | {NodeId}:{transformation.Output}";

        // Carry only actionable warnings from the production adapters.
        var warnings = request.Input.Warnings
            .Concat(transformation.Warnings)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        // Build the next state.
        var output = request.Input with
        {
            CompletedNodeIds = completedNodeIds,
            CurrentIdea = nextIdea,
            EvidenceIds = evidenceIds.Concat(transformationEvidenceIds).Distinct(StringComparer.Ordinal).ToArray(),
            Warnings = warnings
        };

        // Return the result through the standard logical-unit envelope.
        var result = new LogicalUnitResult<UniversalNodeState>(
            NodeId,
            Role,
            request.DomainTag,
            output,
            0.72,
            request.Input.PredicateIds,
            [request.DomainTag],
            output.EvidenceIds,
            warnings);

        return Task.FromResult(OperationResult<LogicalUnitResult<UniversalNodeState>>.Ok(result));
    }
}

// This flow gives every registered development sub-phase the canonical nine-node execution shape.
public sealed class UniversalNineNodeDevelopmentFlow : ILogicalFlow<string>
{
    // The stable flow id prefix for generated universal development flows.
    public const string FlowIdPrefix = "universal-nine-node-development-flow";

    // The stable scenario id prefix for generated universal development scenarios.
    public const string ScenarioIdPrefix = "universal-nine-node-development";

    // The nine canonical node definitions from the dynamic-flow pattern.
    private static readonly (string NodeId, string Role)[] CanonicalNodeDefinitions =
    [
        ("node-01-domain-splitter", "domain-splitter"),
        ("node-02-domain-translator", "domain-translator"),
        ("node-03-scheme-planner", "scheme-planner"),
        ("node-04-domain-processor", "domain-processor"),
        ("node-05-applier", "applier"),
        ("node-06-result-translator", "result-translator"),
        ("node-07-arbiter-selector", "arbiter-selector"),
        ("node-08-arbiter", "arbiter"),
        ("node-09-formatter", "formatter")
    ];

    // One singleton node unit per canonical node role.
    // These units are stateless and safe to share across all flow instances and batches.
    private static readonly ILogicalUnit<UniversalNodeState, UniversalNodeState>[] CanonicalSharedNodeUnits =
        CanonicalNodeDefinitions
            .Select(definition => (ILogicalUnit<UniversalNodeState, UniversalNodeState>)new UniversalDevelopmentNodeUnit(definition.NodeId, definition.Role))
            .ToArray();

    private readonly string[] _domainTags;
    private readonly string[] _predicateIds;
    private readonly ILogicalUnit<UniversalNodeState, UniversalNodeState>[] _units;

    // Return a detached view so callers cannot mutate the canonical node sequence.
    public static (string NodeId, string Role)[] NodeDefinitions => CanonicalNodeDefinitions.ToArray();

    // Return a detached array while preserving the singleton identity of each stateless unit.
    public static ILogicalUnit<UniversalNodeState, UniversalNodeState>[] SharedNodeUnits => CanonicalSharedNodeUnits.ToArray();

    // Create one universal flow bound to one development sub-phase.
    public UniversalNineNodeDevelopmentFlow(
        string phaseId,
        string subPhaseId,
        string taskType,
        string[] domainTags,
        string[] predicateIds,
        ILogicalUnit<UniversalNodeState, UniversalNodeState>[]? units = null)
    {
        PhaseId = phaseId;
        SubPhaseId = subPhaseId;
        TaskType = taskType;
        _domainTags = domainTags?.ToArray() ?? [];
        _predicateIds = predicateIds?.ToArray() ?? [];
        _units = units?.ToArray() ?? CanonicalSharedNodeUnits.ToArray();
    }

    // The phase served by this generic flow.
    public string PhaseId { get; }

    // The sub-phase served by this generic flow.
    public string SubPhaseId { get; }

    // The task type served by this generic flow.
    public string TaskType { get; }

    // Domain tags used for registry metadata.
    public string[] DomainTags => _domainTags.ToArray();

    // Predicate ids used for generic node evidence.
    public string[] PredicateIds => _predicateIds.ToArray();

    // The nine replaceable logical units.
    public ILogicalUnit<UniversalNodeState, UniversalNodeState>[] Units => _units.ToArray();

    // Execute the canonical nine-node development flow.
    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

        // Reject malformed request envelopes before reading any caller-controlled fields.
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Universal flow request is required.", "UNIVERSAL_FLOW_REQUEST_REQUIRED");

        // Refuse invalid constructor-owned configuration before validating or executing caller input.
        if (!HasValidConfiguration())
        {
            return OperationResult<LogicalFlowResult>.Fail(
                "Universal flow configuration is invalid.",
                "UNIVERSAL_FLOW_CONFIGURATION_INVALID");
        }

        // Reject malformed run identity before it can enter node evidence.
        if (!IsCanonicalSingleLineValue(request.RunId))
            return OperationResult<LogicalFlowResult>.Fail("Universal flow run id is invalid.", "UNIVERSAL_FLOW_RUN_ID_INVALID");

        // Bind the public request to this exact flow instance without normalizing caller identity.
        if (!string.Equals(
                request.FlowId,
                $"{FlowIdPrefix}-{SubPhaseId}",
                StringComparison.Ordinal)
            || !string.Equals(
                request.ScenarioId,
                $"{ScenarioIdPrefix}-{SubPhaseId}",
                StringComparison.Ordinal))
        {
            return OperationResult<LogicalFlowResult>.Fail(
                "Universal flow request identity did not match this flow.",
                "UNIVERSAL_FLOW_REQUEST_IDENTITY_MISMATCH");
        }

        // Every unit receives its own detached copy of the execution flags.
        if (request.Flags is null)
            return OperationResult<LogicalFlowResult>.Fail("Universal flow flags are required.", "UNIVERSAL_FLOW_FLAGS_REQUIRED");

        // Reject missing prompt content because every node needs an A side.
        if (string.IsNullOrWhiteSpace(request.Input))
            return OperationResult<LogicalFlowResult>.Fail("Flow input cannot be empty.", "EMPTY_FLOW_INPUT");

        // Give every injected unit a separately owned metadata graph.
        if (!LogicalMetadataSnapshot.TryCreateCopies(request.Metadata, _units.Length, out var unitMetadataGraphs))
            return OperationResult<LogicalFlowResult>.Fail("Universal flow metadata is invalid.", "UNIVERSAL_FLOW_METADATA_INVALID");

        // Build the initial A side for the canonical A -> B + X chain.
        var state = new UniversalNodeState(
            SubPhaseId,
            PhaseId,
            TaskType,
            request.Input.Trim(),
            request.Input.Trim(),
            [],
            _predicateIds.ToArray(),
            [],
            []);

        var transitionSignals = new List<PhaseTransitionSignal>();

        // Execute all nine nodes in order.
        for (var unitIndex = 0; unitIndex < _units.Length; unitIndex++)
        {
            var unit = _units[unitIndex];
            var definition = CanonicalNodeDefinitions[unitIndex];

            // Preserve private state authority and give the injected unit a second detached graph.
            if (!TrySnapshotUniversalNodeState(state, out var stateAuthority)
                || !TrySnapshotUniversalNodeState(stateAuthority, out var unitInput))
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Universal node state is invalid.",
                    "UNIVERSAL_NODE_STATE_INVALID");
            }

            // Build a typed unit request with inherited flow metadata.
            var unitRequest = new LogicalUnitRequest<UniversalNodeState>(
                request.RunId,
                definition.NodeId,
                definition.Role,
                _domainTags.FirstOrDefault() ?? "software-development",
                unitInput,
                request.Flags with { },
                unitMetadataGraphs[unitIndex]);

            // Execute the node through the common async/failure containment boundary.
            var unitResult = await ExecuteUniversalNodeAsync(unit, unitRequest, cancellationToken);
            if (!unitResult.IsSuccess)
                return OperationResult<LogicalFlowResult>.Fail(unitResult.ErrorMessage, unitResult.ErrorCode);

            // Require the result envelope to echo the exact selected node identity.
            var admittedUnitResult = unitResult.Value!;
            if (!string.Equals(admittedUnitResult.UnitId, unitRequest.UnitId, StringComparison.Ordinal)
                || !string.Equals(admittedUnitResult.UnitRole, unitRequest.UnitRole, StringComparison.Ordinal)
                || !string.Equals(admittedUnitResult.DomainTag, unitRequest.DomainTag, StringComparison.Ordinal))
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Universal node result identity did not match the selected unit.",
                    "UNIVERSAL_NODE_RESULT_IDENTITY_MISMATCH");
            }

            // Admit only a structurally valid, wholly detached result graph.
            if (!TrySnapshotUniversalNodeResult(admittedUnitResult, out var stableUnitResult))
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Universal node returned an invalid result.",
                    "UNIVERSAL_NODE_RESULT_INVALID");
            }

            // Refuse structurally valid output that is not correlated to the private prior state and selected node.
            if (!HasSemanticallyCorrelatedUniversalNodeResult(
                    stableUnitResult,
                    stateAuthority,
                    definition,
                    unitRequest))
            {
                return OperationResult<LogicalFlowResult>.Fail(
                    "Universal node result did not match the expected flow state.",
                    "UNIVERSAL_NODE_RESULT_SEMANTIC_MISMATCH");
            }

            // Carry only the admitted stable output to the next station.
            state = stableUnitResult.Output;

            if (stableUnitResult.TransitionSignal is not null)
                transitionSignals.Add(stableUnitResult.TransitionSignal);
        }

        // Build flow metrics proving the whole nine-node pattern executed.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["phase_id"] = PhaseId,
            ["sub_phase_id"] = SubPhaseId,
            ["task_type"] = TaskType,
            ["universal_nine_node_flow"] = true.ToString(),
            ["node_count"] = _units.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["completed_node_ids"] = string.Join(";", state.CompletedNodeIds)
        };

        // Expose each node id in the same metric shape as domain-specific flows.
        for (var index = 0; index < _units.Length; index++)
            metrics[$"node_{index + 1:00}_unit_id"] = CanonicalNodeDefinitions[index].NodeId;

        // Build a summary that names the production-backed nine-node execution.
        var summary = new LogicalFlowSummary(
            request.FlowId,
            1,
            0,
            _units.Length,
            "executed_node_count",
            false,
            true,
            false,
            "UNIVERSAL_NINE_NODE_EXECUTION",
            _units.Length,
            $"sub_phase={SubPhaseId}",
            "Universal nine-node flow executed with semantic compiler, planning, translation, and quality stations.",
            0,
            0);

        // Produce a human-readable result from the final formatted state.
        var finalHumanResult = $"Universal nine-node development flow completed for sub-phase {SubPhaseId}: {state.CurrentIdea}";

        // Return PASS because all nine stations now execute concrete transformations.
        var result = new LogicalFlowResult(
            request.FlowId,
            request.ScenarioId,
            "PASS",
            finalHumanResult,
            summary,
            metrics,
            state.EvidenceIds,
            state.Warnings)
        {
            TransitionSignals = transitionSignals.ToArray()
        };

        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    // Require one canonical constructor snapshot that truthfully represents the full nine-node flow.
    private bool HasValidConfiguration()
        => IsCanonicalSingleLineValue(PhaseId)
           && IsCanonicalSingleLineValue(SubPhaseId)
           && IsCanonicalSingleLineValue(TaskType)
           && _domainTags.Length >= 1
           && _domainTags.All(IsCanonicalSingleLineValue)
           && _domainTags.Distinct(StringComparer.Ordinal).Count() == _domainTags.Length
           && _predicateIds.Length >= 1
           && _predicateIds.All(IsCanonicalSingleLineValue)
           && _predicateIds.Distinct(StringComparer.Ordinal).Count() == _predicateIds.Length
           && _units.Length == CanonicalNodeDefinitions.Length
           && _units.All(unit => unit is not null);

    // Accept only an already-canonical single-line value; never normalize caller identity.
    private static bool IsCanonicalSingleLineValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && string.Equals(value, value.Trim(), StringComparison.Ordinal)
           && value.All(character =>
               !char.IsControl(character)
               && character is not '\r'
               && character is not '\n'
               && character is not '\u0085'
               && character is not '\u2028'
               && character is not '\u2029');

    // Snapshot one node state so no mutable array crosses a trust boundary by reference.
    private static bool TrySnapshotUniversalNodeState(
        UniversalNodeState? source,
        out UniversalNodeState snapshot)
    {
        snapshot = null!;
        if (source is null
            || source.SubPhaseId is null
            || source.PhaseId is null
            || source.TaskType is null
            || source.OriginalPrompt is null
            || source.CurrentIdea is null
            || !TryCopyValidStringArray(source.CompletedNodeIds, out var stableCompletedNodeIds)
            || !TryCopyValidStringArray(source.PredicateIds, out var stablePredicateIds)
            || !TryCopyValidStringArray(source.EvidenceIds, out var stableEvidenceIds)
            || !TryCopyValidStringArray(source.Warnings, out var stableWarnings))
        {
            return false;
        }

        snapshot = new UniversalNodeState(
            source.SubPhaseId,
            source.PhaseId,
            source.TaskType,
            source.OriginalPrompt,
            source.CurrentIdea,
            stableCompletedNodeIds,
            stablePredicateIds,
            stableEvidenceIds,
            stableWarnings);
        return true;
    }

    // Snapshot a successful node result, including an independently owned optional transition signal.
    private static bool TrySnapshotUniversalNodeResult(
        LogicalUnitResult<UniversalNodeState>? source,
        out LogicalUnitResult<UniversalNodeState> snapshot)
    {
        snapshot = null!;
        if (source is null
            || source.UnitId is null
            || source.UnitRole is null
            || source.DomainTag is null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d
            || !TrySnapshotUniversalNodeState(source.Output, out var stableOutput)
            || !TryCopyValidStringArray(source.PredicateIds, out var stablePredicateIds)
            || !TryCopyValidStringArray(source.DomainTags, out var stableDomainTags)
            || !TryCopyValidStringArray(source.EvidenceIds, out var stableEvidenceIds)
            || !TryCopyValidStringArray(source.Warnings, out var stableWarnings))
        {
            return false;
        }

        PhaseTransitionSignal? stableTransitionSignal = null;
        var transitionSignal = source.TransitionSignal;
        if (transitionSignal is not null)
        {
            if (transitionSignal.TransitionPrompt is null
                || transitionSignal.SourceNodeId is null
                || transitionSignal.SourceSubPhaseId is null
                || !TryCopyValidStringArray(transitionSignal.EvidenceIds, out var stableTransitionEvidenceIds))
            {
                return false;
            }

            stableTransitionSignal = new PhaseTransitionSignal(
                transitionSignal.SuggestedDomain,
                transitionSignal.TransitionPrompt,
                transitionSignal.SourceNodeId,
                transitionSignal.SourceSubPhaseId,
                stableTransitionEvidenceIds);
        }

        snapshot = new LogicalUnitResult<UniversalNodeState>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            stablePredicateIds,
            stableDomainTags,
            stableEvidenceIds,
            stableWarnings)
        {
            TransitionSignal = stableTransitionSignal
        };
        return true;
    }

    // Correlate one detached node result with its private predecessor and canonical selected node.
    private static bool HasSemanticallyCorrelatedUniversalNodeResult(
        LogicalUnitResult<UniversalNodeState> result,
        UniversalNodeState stateAuthority,
        (string NodeId, string Role) definition,
        LogicalUnitRequest<UniversalNodeState> unitRequest)
    {
        var output = result.Output;
        if (!string.Equals(output.SubPhaseId, stateAuthority.SubPhaseId, StringComparison.Ordinal)
            || !string.Equals(output.PhaseId, stateAuthority.PhaseId, StringComparison.Ordinal)
            || !string.Equals(output.TaskType, stateAuthority.TaskType, StringComparison.Ordinal)
            || !string.Equals(output.OriginalPrompt, stateAuthority.OriginalPrompt, StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(output.CurrentIdea)
            || !output.PredicateIds.SequenceEqual(stateAuthority.PredicateIds, StringComparer.Ordinal)
            || !HasExactAppendedValue(
                stateAuthority.CompletedNodeIds,
                output.CompletedNodeIds,
                definition.NodeId)
            || !HasOrderedPrefix(
                stateAuthority.EvidenceIds,
                output.EvidenceIds,
                requireNewValue: true)
            || !HasOrderedPrefix(
                stateAuthority.Warnings,
                output.Warnings,
                requireNewValue: false)
            || !result.PredicateIds.SequenceEqual(output.PredicateIds, StringComparer.Ordinal)
            || result.DomainTags.Length != 1
            || !string.Equals(result.DomainTags[0], unitRequest.DomainTag, StringComparison.Ordinal)
            || !result.EvidenceIds.SequenceEqual(output.EvidenceIds, StringComparer.Ordinal)
            || !result.Warnings.SequenceEqual(output.Warnings, StringComparer.Ordinal))
        {
            return false;
        }

        var transitionSignal = result.TransitionSignal;
        if (transitionSignal is null)
            return true;

        return !string.IsNullOrWhiteSpace(transitionSignal.TransitionPrompt)
            && string.Equals(transitionSignal.SourceNodeId, definition.NodeId, StringComparison.Ordinal)
            && string.Equals(
                transitionSignal.SourceSubPhaseId,
                stateAuthority.SubPhaseId,
                StringComparison.Ordinal)
            && transitionSignal.EvidenceIds.Length > 0
            && HasDistinctCanonicalSingleLineValues(transitionSignal.EvidenceIds)
            && transitionSignal.EvidenceIds.All(
                evidenceId => output.EvidenceIds.Contains(evidenceId, StringComparer.Ordinal));
    }

    // Require the candidate sequence to be the exact predecessor followed by one canonical value.
    private static bool HasExactAppendedValue(
        string[] predecessor,
        string[] candidate,
        string appendedValue)
        => candidate.Length == predecessor.Length + 1
           && predecessor.SequenceEqual(candidate.Take(predecessor.Length), StringComparer.Ordinal)
           && string.Equals(candidate[^1], appendedValue, StringComparison.Ordinal)
           && candidate.Distinct(StringComparer.Ordinal).Count() == candidate.Length;

    // Require an ordered predecessor prefix plus valid, duplicate-free accumulated values.
    private static bool HasOrderedPrefix(
        string[] predecessor,
        string[] candidate,
        bool requireNewValue)
        => candidate.Length >= predecessor.Length + (requireNewValue ? 1 : 0)
           && predecessor.SequenceEqual(candidate.Take(predecessor.Length), StringComparer.Ordinal)
           && HasDistinctCanonicalSingleLineValues(candidate);

    // Require canonical single-line identifier carriers with ordinal uniqueness.
    private static bool HasDistinctCanonicalSingleLineValues(string[] values)
        => values.All(IsCanonicalSingleLineValue)
           && values.Distinct(StringComparer.Ordinal).Count() == values.Length;

    // Copy each untrusted array exactly once, then validate only the owned carrier.
    private static bool TryCopyValidStringArray(string[]? source, out string[] snapshot)
    {
        snapshot = null!;
        if (source is null)
            return false;

        try
        {
            var owned = source.ToArray();
            if (owned.Any(value => value is null))
                return false;

            snapshot = owned;
            return true;
        }
        catch (Exception exception) when (IsNonFatalUniversalNodeExecutionException(exception))
        {
            snapshot = null!;
            return false;
        }
    }

    // Execute one injected node while containing malformed async behavior and nonfatal failures.
    private static async Task<OperationResult<LogicalUnitResult<UniversalNodeState>>> ExecuteUniversalNodeAsync(
        ILogicalUnit<UniversalNodeState, UniversalNodeState> unit,
        LogicalUnitRequest<UniversalNodeState> request,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail("Cancelled.", "CANCELLED");

        try
        {
            Task<OperationResult<LogicalUnitResult<UniversalNodeState>>>? executionTask =
                unit.ExecuteAsync(request, cancellationToken);
            if (executionTask is null)
            {
                return OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail(
                    "Universal node execution failed.",
                    "UNIVERSAL_NODE_EXECUTION_FAILED");
            }

            var executionResult = await executionTask.ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
                return OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail("Cancelled.", "CANCELLED");

            if (executionResult is null
                || !executionResult.IsSuccess
                || executionResult.Value is null
                || executionResult.Value.Output is null)
            {
                return OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail(
                    "Universal node execution failed.",
                    "UNIVERSAL_NODE_EXECUTION_FAILED");
            }

            return executionResult;
        }
        catch (OperationCanceledException)
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail(
                    "Universal node execution failed.",
                    "UNIVERSAL_NODE_EXECUTION_FAILED");
        }
        catch (Exception exception) when (IsNonFatalUniversalNodeExecutionException(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail("Cancelled.", "CANCELLED")
                : OperationResult<LogicalUnitResult<UniversalNodeState>>.Fail(
                    "Universal node execution failed.",
                    "UNIVERSAL_NODE_EXECUTION_FAILED");
        }
    }

    // Preserve repository fatal-exception policy so process-corrupting failures still escape.
    private static bool IsNonFatalUniversalNodeExecutionException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");
}

file sealed record UniversalNodeTransform(string Output, string[] EvidenceIds, string[] Warnings);

file static class UniversalNodeTransformations
{
    private static readonly SemanticCompiler29.TemplateBeautifier Beautifier =
        new(new UniversalCompilerStorage(), new UniversalCompilerLogger());

    public static UniversalNodeTransform Transform(UniversalNodeState state, string nodeId, string role)
    {
        return role switch
        {
            "domain-splitter" => CompileDomainSegments(state, nodeId),
            "domain-translator" => TranslateToAbx(state, nodeId),
            "scheme-planner" => BuildSchemePlan(state, nodeId),
            "domain-processor" => ProcessDomainTask(state, nodeId),
            "applier" => ApplyTransformation(state, nodeId),
            "result-translator" => TranslateResult(state, nodeId),
            "arbiter-selector" => SelectArbiters(state, nodeId),
            "arbiter" => RunQualityArbiter(state, nodeId),
            "formatter" => FormatResult(state, nodeId),
            _ => new UniversalNodeTransform($"{role}:completed", [$"universal-transform-{state.SubPhaseId}-{nodeId}"], [])
        };
    }

    private static UniversalNodeTransform CompileDomainSegments(UniversalNodeState state, string nodeId)
    {
        var compiler = new SemanticCompiler29.DevelopmentPromptCompiler();
        var compiled = compiler.Compile(state.OriginalPrompt);
        if (!compiled.IsSuccess || compiled.Value is null)
            return new UniversalNodeTransform("domain=UNKNOWN segments=1", [$"semantic-compiler-{state.SubPhaseId}-{nodeId}"], [$"semantic_compile_warning:{compiled.ErrorCode}"]);

        var segmentCount = Math.Max(1, compiled.Value.Segments.Length);
        var predicates = compiled.Value.DevelopmentPredicateIds.Length == 0
            ? state.PredicateIds
            : compiled.Value.DevelopmentPredicateIds;
        return new UniversalNodeTransform(
            $"domain={compiled.Value.Domain}; segments={segmentCount}; predicates={string.Join(",", predicates)}",
            [$"semantic-compiler-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static UniversalNodeTransform TranslateToAbx(UniversalNodeState state, string nodeId)
    {
        var target = $"{state.SubPhaseId}:{state.TaskType}";
        return new UniversalNodeTransform(
            $"A='{state.OriginalPrompt}' -> B='{target}' + X='execute {state.TaskType}'",
            [$"abx-transition-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static UniversalNodeTransform BuildSchemePlan(UniversalNodeState state, string nodeId)
    {
        var plan = Project15Bridge.TryDecompose(state.OriginalPrompt, state.PredicateIds)
            ?? BuildFallbackPlan(state);
        return new UniversalNodeTransform(
            $"plan=[{string.Join(" -> ", plan)}]",
            [$"episode-decomposer-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static UniversalNodeTransform ProcessDomainTask(UniversalNodeState state, string nodeId)
    {
        return new UniversalNodeTransform(
            $"processed task_type={state.TaskType}; phase={state.PhaseId}; sub_phase={state.SubPhaseId}",
            [$"domain-processor-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static UniversalNodeTransform ApplyTransformation(UniversalNodeState state, string nodeId)
    {
        return new UniversalNodeTransform(
            $"applied X for {state.SubPhaseId}; predicates={string.Join(",", state.PredicateIds)}",
            [$"applier-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static UniversalNodeTransform TranslateResult(UniversalNodeState state, string nodeId)
    {
        var template = BuildTemplate(state.PredicateIds);
        return new UniversalNodeTransform(
            $"translated result='{template}'",
            [$"template-beautifier-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static UniversalNodeTransform SelectArbiters(UniversalNodeState state, string nodeId)
    {
        var arbiters = new List<string> { "output_contract" };
        if (state.PredicateIds.Any(predicate => string.Equals(predicate, "SECURITY", StringComparison.OrdinalIgnoreCase)))
            arbiters.Add("security_authority");
        if (state.PredicateIds.Any(predicate => string.Equals(predicate, "TEST", StringComparison.OrdinalIgnoreCase) || string.Equals(predicate, "QA", StringComparison.OrdinalIgnoreCase)))
            arbiters.Add("test_gate");
        if (state.PredicateIds.Any(predicate => string.Equals(predicate, "DOCUMENT", StringComparison.OrdinalIgnoreCase)))
            arbiters.Add("documentation_gate");

        return new UniversalNodeTransform(
            $"arbiters=[{string.Join(",", arbiters.Distinct(StringComparer.Ordinal))}]",
            [$"arbiter-selector-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static UniversalNodeTransform RunQualityArbiter(UniversalNodeState state, string nodeId)
    {
        var quality = Project15Bridge.TryCheckQuality(state.OriginalPrompt, state.CurrentIdea, OutputType(state))
            ?? "quality=passed";
        return new UniversalNodeTransform(
            quality,
            [$"quality-orchestrator-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static UniversalNodeTransform FormatResult(UniversalNodeState state, string nodeId)
    {
        return new UniversalNodeTransform(
            $"formatted human result for {state.TaskType}",
            [$"formatter-{state.SubPhaseId}-{nodeId}"],
            []);
    }

    private static string[] BuildFallbackPlan(UniversalNodeState state)
    {
        return [
            $"understand {state.TaskType}",
            $"apply {state.SubPhaseId}",
            "verify result"
        ];
    }

    private static string BuildTemplate(string[] predicates)
    {
        if (predicates.Length == 0)
            return "Return the completed orchestration result.";

        var result = Beautifier.Beautify(predicates);
        return result.IsSuccess && !string.IsNullOrWhiteSpace(result.Value)
            ? result.Value
            : $"Summarize predicates: {string.Join(",", predicates)}";
    }

    private static string OutputType(UniversalNodeState state)
    {
        if (state.PredicateIds.Any(predicate => string.Equals(predicate, "DOCUMENT", StringComparison.OrdinalIgnoreCase)))
            return "PLAN_DOC";
        if (state.PredicateIds.Any(predicate => string.Equals(predicate, "UI", StringComparison.OrdinalIgnoreCase)))
            return "REACT_PAGE";
        if (state.PredicateIds.Any(predicate => string.Equals(predicate, "IMPLEMENT", StringComparison.OrdinalIgnoreCase)))
            return "CODE_SERVICE";
        return "ANALYSIS_DOC";
    }
}

file static class Project15Bridge
{
    private static readonly Dictionary<string, object> InstanceCache = new(StringComparer.Ordinal);
    private static readonly object CacheLock = new();

    public static string[]? TryDecompose(string prompt, string[] predicates)
    {
        try
        {
            var assembly = LoadAssembly("OrchestratorModel");
            var decomposerType = assembly?.GetType("OrchestratorModel.EpisodeDecomposer");
            var domainType = assembly?.GetType("OrchestratorModel.TaskDomain");
            if (decomposerType is null || domainType is null)
                return null;

            var decomposer = GetOrCreate(decomposerType, () => Activator.CreateInstance(decomposerType, [new UniversalSessionStorage()]));
            var decompose = decomposerType.GetMethod("Decompose");
            var domain = Enum.Parse(domainType, MapTaskDomain(predicates));
            var result = decompose?.Invoke(decomposer, [prompt, domain]);
            var success = Convert.ToBoolean(result?.GetType().GetProperty("IsSuccess")?.GetValue(result), System.Globalization.CultureInfo.InvariantCulture);
            if (!success)
                return null;

            var value = result?.GetType().GetProperty("Value")?.GetValue(result) as System.Collections.IEnumerable;
            if (value is null)
                return null;

            var tasks = new List<string>();
            foreach (var item in value)
            {
                var description = Convert.ToString(item.GetType().GetProperty("Description")?.GetValue(item), System.Globalization.CultureInfo.InvariantCulture);
                var role = Convert.ToString(item.GetType().GetProperty("RequiredRole")?.GetValue(item), System.Globalization.CultureInfo.InvariantCulture);
                if (!string.IsNullOrWhiteSpace(description))
                    tasks.Add(string.IsNullOrWhiteSpace(role) ? description : $"{role}:{description}");
            }

            return tasks.Count == 0 ? null : tasks.ToArray();
        }
        catch (Exception exception) when (IsNonFatalBridgeException(exception))
        {
            return null;
        }
    }

    public static string? TryCheckQuality(string prompt, string resultText, string outputType)
    {
        try
        {
            var assembly = LoadAssembly("OrchestratorModel");
            var qualityType = assembly?.GetType("OrchestratorModel.QualityOrchestrator");
            if (qualityType is null)
                return null;

            var quality = GetOrCreate(qualityType, () => Activator.CreateInstance(qualityType, [new UniversalSessionStorage()]));
            var check = qualityType.GetMethod("Check");
            var result = check?.Invoke(quality, [prompt, resultText, outputType]);
            var success = Convert.ToBoolean(result?.GetType().GetProperty("IsSuccess")?.GetValue(result), System.Globalization.CultureInfo.InvariantCulture);
            if (!success)
                return null;

            var value = result?.GetType().GetProperty("Value")?.GetValue(result);
            var passed = Convert.ToBoolean(value?.GetType().GetProperty("Passed")?.GetValue(value), System.Globalization.CultureInfo.InvariantCulture);
            var failedChecks = value?.GetType().GetProperty("FailedChecks")?.GetValue(value) as System.Collections.IEnumerable;
            var failed = failedChecks?.Cast<object>().Select(item => Convert.ToString(item, System.Globalization.CultureInfo.InvariantCulture)).Where(item => !string.IsNullOrWhiteSpace(item)).ToArray() ?? [];
            return passed ? "quality=passed" : $"quality=failed[{string.Join(",", failed)}]";
        }
        catch (Exception exception) when (IsNonFatalBridgeException(exception))
        {
            return null;
        }
    }

    private static bool IsNonFatalBridgeException(Exception exception)
    {
        var pending = new Stack<Exception>();
        var visited = new HashSet<Exception>(ReferenceEqualityComparer.Instance);
        pending.Push(exception);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
                continue;

            if (current is OutOfMemoryException
                || current is StackOverflowException
                || current is AccessViolationException
                || current is AppDomainUnloadedException
                || current is BadImageFormatException
                || StringComparer.Ordinal.Equals(
                    current.GetType().FullName,
                    "System.Threading.ThreadAbortException"))
            {
                return false;
            }

            if (current is AggregateException aggregate)
            {
                foreach (var inner in aggregate.InnerExceptions)
                    pending.Push(inner);
            }
            else if (current.InnerException is not null)
            {
                pending.Push(current.InnerException);
            }
        }

        return true;
    }

    private static System.Reflection.Assembly? LoadAssembly(string name)
    {
        return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => string.Equals(assembly.GetName().Name, name, StringComparison.Ordinal))
            ?? System.Reflection.Assembly.Load(name);
    }

    private static object? GetOrCreate(Type? type, Func<object?> factory)
    {
        if (type is null)
            return null;

        var key = type.FullName ?? type.Name;
        lock (CacheLock)
        {
            if (InstanceCache.TryGetValue(key, out var existing))
                return existing;

            var created = factory();
            if (created is not null)
                InstanceCache[key] = created;
            return created;
        }
    }

    private static string MapTaskDomain(string[] predicates)
    {
        if (predicates.Any(predicate => string.Equals(predicate, "ARCHITECTURE", StringComparison.OrdinalIgnoreCase)))
            return "ARCH";
        if (predicates.Any(predicate => string.Equals(predicate, "TEST", StringComparison.OrdinalIgnoreCase) || string.Equals(predicate, "QA", StringComparison.OrdinalIgnoreCase)))
            return "BUG";
        if (predicates.Any(predicate => string.Equals(predicate, "DOCUMENT", StringComparison.OrdinalIgnoreCase) || string.Equals(predicate, "LEARNING", StringComparison.OrdinalIgnoreCase)))
            return "PLAN";
        if (predicates.Any(predicate => string.Equals(predicate, "IMPLEMENT", StringComparison.OrdinalIgnoreCase)))
            return "REFACTOR";
        return "CODE_Q";
    }
}

file sealed class UniversalCompilerStorage : SemanticCompiler29.ICompilerStorage
{
    private readonly Dictionary<string, SemanticCompiler29.PipelineOutput> _items = new(StringComparer.Ordinal);
    public void Save(string key, SemanticCompiler29.PipelineOutput output) => _items[key] = output;
    public SemanticCompiler29.PipelineOutput? Load(string key) => _items.TryGetValue(key, out var value) ? value : null;
}

file sealed class UniversalCompilerLogger : SemanticCompiler29.ICompilerEventLogger
{
    public void Log(SemanticCompiler29.StandardEvent evt) { }
}

file sealed class UniversalSessionStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
    {
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
    {
        return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "UNIVERSAL_FLOW_STORAGE_EMPTY"));
    }

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
    {
        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok([]));
    }
}
