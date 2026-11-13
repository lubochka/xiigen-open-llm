// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// This session wires the first inspectable topology mutation path for the desktop inspector.
public sealed class InspectorSession
{
    // The traced topology mutation flow is the first runnable path shown by the inspector.
    private readonly ILogicalFlow<LogicalTopologyCommand> _topologyMutationFlow;

    // The traced add-domain unit is reused by the parallel-wave proof path.
    private readonly ILogicalUnit<AddDomainCommand, LogicalDomainEntry> _addDomainUnit;

    // The traced permission gate exposes project 21 approval decisions in the inspector.
    private readonly TracingPermissionGate _permissionGate;

    // The traced learning scheduler exposes project 21 idle-learning queue behavior in the inspector.
    private readonly TracingLearningEventScheduler _learningEventScheduler;

    // The traced internal LLM gateway exposes internal-first adapter routing in the inspector.
    private readonly TracingInternalLlmGateway _internalLlmGateway;

    // The traced route classifier exposes project 22 routing behavior in the inspector.
    private readonly TracingRouteClassifier _project22RouteClassifier;

    // The traced skill filter exposes project 22 KNN skill selection in the inspector.
    private readonly TracingKnnSkillFilter _project22SkillFilter;

    // The traced result examiner exposes project 22 internal-first quality checks in the inspector.
    private readonly TracingInternalResultExaminer _project22ResultExaminer;

    // The traced approval package factory exposes project 22 external-AI approval prompts.
    private readonly TracingExternalAiRequestPackageFactory _project22PackageFactory;

    // The traced result adapter exposes project 22 arbiter-aware external answer adaptation.
    private readonly TracingExternalAiResultAdapter _project22ResultAdapter;

    // Consumers subscribe to this event to receive trace start and completion events.
    public event EventHandler<StationTrace>? StationTraced;

    // Create a session using the current in-memory topology registry and traced units.
    public InspectorSession()
    {
        // Wrap today's concrete registry so storage calls are visible in the trace tree.
        var registry = new TracingStorageDecorator(new InMemoryLogicalTopologyRegistry(), EmitTrace);

        // Wrap the project 21 permission gate so external-AI approval decisions are visible.
        _permissionGate = new TracingPermissionGate(new PermissionGate(Environment.CurrentDirectory), EmitTrace);

        // Wrap the project 21 learning scheduler so idle-learning release decisions are visible.
        _learningEventScheduler = new TracingLearningEventScheduler(new LearningEventScheduler(() => true, 5), EmitTrace);

        // Wrap the project 21 internal gateway so internal-first routing is visible without external calls.
        _internalLlmGateway = new TracingInternalLlmGateway(
            new InternalLlmGateway(new DeterministicInspectorLlmClient(), new DeterministicInspectorDomainModelRegistry()),
            EmitTrace);

        // Wrap the project 22 route classifier so routing decisions are visible.
        _project22RouteClassifier = new TracingRouteClassifier(new AgentAlgorithms.RouteClassifier(), EmitTrace);

        // Wrap the project 22 KNN skill filter with deterministic local search for the sample path.
        _project22SkillFilter = new TracingKnnSkillFilter(new AgentAlgorithms.KnnSkillFilter(new DeterministicProject22SearchProvider()), EmitTrace);

        // Wrap the project 22 result examiner so confidence and missing competence are visible.
        _project22ResultExaminer = new TracingInternalResultExaminer(new AgentAlgorithms.InternalResultExaminer(), EmitTrace);

        // Wrap the project 22 approval package construction so prompt criteria are visible.
        _project22PackageFactory = new TracingExternalAiRequestPackageFactory(EmitTrace);

        // Wrap the project 22 external result adapter so arbiter enforcement is visible.
        _project22ResultAdapter = new TracingExternalAiResultAdapter(new AgentAlgorithms.ExternalAiResultAdapter(), EmitTrace);

        // Wrap the add-domain unit so unit boundaries are visible before registry storage happens.
        var addDomainUnit = new TracingLogicalUnit<AddDomainCommand, LogicalDomainEntry>(
            new AddDomainTopologyMutationUnit(registry),
            nameof(AddDomainTopologyMutationUnit),
            EmitTrace);

        // Store the traced add-domain unit so Phase 67 can emit parallel siblings under one flow.
        _addDomainUnit = addDomainUnit;

        // Wrap the add-phase unit so phase mutation will be inspectable when the command bar reaches it.
        var addPhaseUnit = new TracingLogicalUnit<AddPhaseCommand, LogicalPhaseEntry>(
            new AddPhaseTopologyMutationUnit(registry),
            nameof(AddPhaseTopologyMutationUnit),
            EmitTrace);

        // Wrap the add-sub-phase unit so sub-phase mutation shares the same trace shape.
        var addSubPhaseUnit = new TracingLogicalUnit<AddSubPhaseCommand, LogicalSubPhaseEntry>(
            new AddSubPhaseTopologyMutationUnit(registry),
            nameof(AddSubPhaseTopologyMutationUnit),
            EmitTrace);

        // Wrap the add-transition unit so transition storage can be inspected later without rewiring.
        var addTransitionUnit = new TracingLogicalUnit<AddTransitionCommand, LogicalTransitionEntry>(
            new AddTransitionTopologyMutationUnit(registry),
            nameof(AddTransitionTopologyMutationUnit),
            EmitTrace);

        // Create the real topology mutation flow from traced logical units.
        var flow = new TopologyMutationFlow(addDomainUnit, addPhaseUnit, addSubPhaseUnit, addTransitionUnit);

        // Wrap the flow so the trace tree shows the flow boundary above units.
        _topologyMutationFlow = new TracingFlowDecorator<LogicalTopologyCommand>(
            flow,
            nameof(TopologyMutationFlow),
            EmitTrace);
    }

    // Run one topology command through the inspectable orchestrator-to-storage chain.
    public async Task<OperationResult<LogicalFlowResult>> RunTopologyCommandAsync(
        LogicalTopologyCommand command,
        CancellationToken cancellationToken = default)
    {
        // Create a new run id because each command execution is one inspector run.
        var runId = Guid.NewGuid().ToString("N");

        // Create the root orchestrator start trace.
        var start = CreateOrchestratorStartTrace(runId, command);

        // Emit the root start event before any flow is selected.
        EmitTrace(start);

        // Push the orchestrator as the parent for the selected flow.
        using (TraceContextScope.Push(new TraceContext(runId, start.TraceId, 1, 0)))
        {
            // Build the flow request from the user topology command.
            var request = BuildTopologyFlowRequest(runId, command);

            // Execute the traced flow exactly once.
            var result = await _topologyMutationFlow.ExecuteAsync(request, cancellationToken);

            // Emit the root completion event with the flow result envelope.
            EmitTrace(CompleteOrchestratorTrace(start, result));

            // Return the flow result unchanged to the caller.
            return result;
        }
    }

    // Run multiple add-domain commands under one flow with the same wave index.
    public async Task<OperationResult<LogicalFlowResult>> RunParallelDomainCommandsAsync(
        IReadOnlyList<AddDomainCommand> commands,
        CancellationToken cancellationToken = default)
    {
        // Create a new run id because each inspector command execution is one run.
        var runId = Guid.NewGuid().ToString("N");

        // Create the root orchestrator start trace for the parallel command batch.
        var rootStart = CreateParallelOrchestratorStartTrace(runId, commands);

        // Emit the root start before selecting the parallel flow.
        EmitTrace(rootStart);

        // Create the parallel flow start trace as a child of the root.
        var flowStart = CreateParallelFlowStartTrace(runId, rootStart.TraceId, commands);

        // Emit the flow start before dispatching unit siblings.
        EmitTrace(flowStart);

        // Build unit requests for every command in the parallel wave.
        var requests = commands.Select(command => BuildAddDomainUnitRequest(runId, command)).ToArray();

        // Execute every unit sibling with the same parent flow and same wave index.
        var unitResults = new List<OperationResult<LogicalUnitResult<LogicalDomainEntry>>>();

        // Dispatch each unit through the same wave context so the UI renders them side by side.
        foreach (var request in requests)
            unitResults.Add(await ExecuteParallelAddDomainUnitAsync(flowStart, request, cancellationToken));

        // Build the normalized flow result from all unit results.
        var flowResult = BuildParallelFlowResult(unitResults);

        // Emit the parallel flow completion trace.
        EmitTrace(CompleteParallelFlowTrace(flowStart, flowResult));

        // Emit the root completion trace.
        EmitTrace(CompleteOrchestratorTrace(rootStart, flowResult));

        // Return the normalized result to the caller.
        return flowResult;
    }

    // RunProject21InfrastructureSampleAsync emits project 21 permission and learning scheduler stations.
    public Task<OperationResult<LogicalFlowResult>> RunProject21InfrastructureSampleAsync(
        CancellationToken cancellationToken = default)
    {
        // Create a new run id because this sample is one inspection run.
        var runId = Guid.NewGuid().ToString("N");

        // Create the root orchestrator start trace for the project 21 sample.
        var rootStart = CreateProject21OrchestratorStartTrace(runId);

        // Emit the root start event before the project 21 flow begins.
        EmitTrace(rootStart);

        // Create the project 21 flow start trace as a child of the root.
        var flowStart = CreateProject21FlowStartTrace(runId, rootStart.TraceId);

        // Emit the flow start before calling project 21 stations.
        EmitTrace(flowStart);

        // Push the project 21 flow as the parent for the inspected stations.
        using (TraceContextScope.Push(new TraceContext(runId, flowStart.TraceId, flowStart.Depth + 1, 0)))
        {
            // Build the external-AI permission request used by the internal-first approval path.
            var permissionArgs = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                ["reason"] = "inspect external AI approval gate",
                ["expected_result"] = "requires one request-scoped approval"
            };

            // Run the real permission gate through its tracing adapter.
            var permission = _permissionGate.Check("call_external_ai", permissionArgs);

            // Queue enough examples to cross the scheduler threshold.
            var queued = _learningEventScheduler.QueueIfReady("logical_topology", 5, "Phase 73 inspector learning scheduler sample.");

            // Drain immediately because this sample scheduler is configured as idle.
            var drained = _learningEventScheduler.DrainIfIdle();

            // Build the normalized project 21 flow result from the inspected outputs.
            var flowResult = BuildProject21FlowResult(permission, queued, drained);

            // Emit the project 21 flow completion trace.
            EmitTrace(CompleteProject21FlowTrace(flowStart, flowResult));

            // Emit the root orchestrator completion trace.
            EmitTrace(CompleteOrchestratorTrace(rootStart, flowResult));

            // Return the normalized result.
            return Task.FromResult(flowResult);
        }
    }

    // RunProject22AlgorithmsSampleAsync emits project 22 routing, KNN, examiner, approval, and adapter stations.
    public async Task<OperationResult<LogicalFlowResult>> RunProject22AlgorithmsSampleAsync(
        CancellationToken cancellationToken = default)
    {
        // Create a new run id because this sample is one inspection run.
        var runId = Guid.NewGuid().ToString("N");

        // Create the root orchestrator start trace for the project 22 sample.
        var rootStart = CreateProject22OrchestratorStartTrace(runId);

        // Emit the root start event before the project 22 flow begins.
        EmitTrace(rootStart);

        // Create the project 22 flow start trace as a child of the root.
        var flowStart = CreateProject22FlowStartTrace(runId, rootStart.TraceId);

        // Emit the flow start before calling project 22 stations.
        EmitTrace(flowStart);

        // Push the project 22 flow as the parent for the inspected stations.
        using (TraceContextScope.Push(new TraceContext(runId, flowStart.TraceId, flowStart.Depth + 1, 0)))
        {
            // Run the real internal gateway routing path without sending to any external model.
            var gatewayRouting = _internalLlmGateway.InspectRouting("explain how to fix null crash during repeated provider callback");

            // Run the real route classifier on a V06-style compound bug prompt.
            var route = _project22RouteClassifier.Classify("explain how to fix null crash during repeated provider callback");

            // Build a small deterministic skill catalogue for the KNN filter.
            var skills = new[]
            {
                // This skill should win because it matches the sample bug-fix task.
                new AgentAlgorithms.TextChunk("bug-fix-skill", "Fix null crashes and failed provider callbacks.", 0, AgentAlgorithms.ContentType.SkillDocument),

                // This skill should remain relevant as diagnostics support.
                new AgentAlgorithms.TextChunk("crash-diagnostics-skill", "Inspect stack traces and provider callback failures.", 0, AgentAlgorithms.ContentType.SkillDocument),

                // This skill should not dominate the bug-fix task.
                new AgentAlgorithms.TextChunk("code-question-skill", "Explain general code patterns and examples.", 0, AgentAlgorithms.ContentType.SkillDocument)
            };

            // Run the real KNN skill filter through deterministic local scores.
            var filtered = await _project22SkillFilter.FilterAsync(
                skills,
                "fix null crash during repeated provider callback",
                2,
                0.40);

            // Run the real internal result examiner with intentionally incomplete local coverage.
            var examination = _project22ResultExaminer.Examine(
                "Fix the repeated provider callback null crash while keeping one account identity.",
                "The internal answer can validate the callback but lacks account creation and auth-session coverage.",
                ["VALIDATE_OP", "CREATE_OP", "AUTH_OP"],
                ["identity_consistency", "token_safety"],
                0.75);

            // Stop early if a required project 22 station failed.
            if (!gatewayRouting.IsSuccess || !route.IsSuccess || !filtered.IsSuccess || !examination.IsSuccess)
            {
                // Build the first failure as a project 21 operation envelope because this public method returns runtime flow results.
                var failure = BuildProject22EarlyFailure(gatewayRouting, route, filtered, examination);

                // Emit the project 22 flow completion trace for the failure.
                EmitTrace(CompleteProject22FlowTrace(flowStart, failure));

                // Emit the root orchestrator completion trace for the failure.
                EmitTrace(CompleteOrchestratorTrace(rootStart, failure));

                // Return the failure envelope to the caller.
                return failure;
            }

            // Build the external-AI request package from the examiner's named gap.
            var package = new AgentAlgorithms.ExternalAiRequestPackage(
                "Fix the repeated provider callback null crash while keeping one account identity.",
                "VALIDATE_OP CREATE_OP AUTH_OP",
                examination.Value!.CorrectedResponse,
                examination.Value!.Confidence,
                examination.Value!.MissingCompetence,
                "Return local-form remediation criteria and do not override arbiters.",
                ["identity_consistency", "token_safety"]);

            // Trace the exact approval package and prompt text.
            var inspectedPackage = _project22PackageFactory.Create(package);

            // Adapt a safe external answer through the real arbiter-aware adapter.
            var adapted = _project22ResultAdapter.Adapt(
                package,
                "Validate provider identity, reuse the existing account for a repeated callback, and keep session tokens in secure httpOnly cookies.",
                "registration");

            // Build the normalized project 22 flow result from the inspected outputs.
            var flowResult = BuildProject22FlowResult(gatewayRouting, route, filtered, examination, inspectedPackage, adapted);

            // Emit the project 22 flow completion trace.
            EmitTrace(CompleteProject22FlowTrace(flowStart, flowResult));

            // Emit the root orchestrator completion trace.
            EmitTrace(CompleteOrchestratorTrace(rootStart, flowResult));

            // Return the normalized result.
            return flowResult;
        }
    }

    // RunProject23StateMachineSampleAsync emits project 23 state-machine, node, and guard stations.
    public async Task<OperationResult<LogicalFlowResult>> RunProject23StateMachineSampleAsync(
        CancellationToken cancellationToken = default)
    {
        // Create a new run id because this sample is one inspection run.
        var runId = Guid.NewGuid().ToString("N");

        // Create a temporary checkpoint database for this deterministic inspection run.
        var dbPath = Path.Combine(Path.GetTempPath(), $"llm-inspector-project23-{runId}.db");

        // Create the root orchestrator start trace for the project 23 sample.
        var rootStart = CreateProject23OrchestratorStartTrace(runId);

        // Emit the root start event before the project 23 state machine begins.
        EmitTrace(rootStart);

        try
        {
            // Create the concrete project 23 checkpointer used by the real state machine.
            await using var checkpointer = new OssEquivalents.StateCheckpointer(dbPath);

            // Create the real project 23 state machine.
            var machine = new OssEquivalents.AgentStateMachine(checkpointer);

            // Create the traced external-AI guard used by the conditional edge.
            var guard = new TracingExternalAiStateGuard(EmitTrace);

            // Add the examine node that simulates a low-confidence named knowledge edge.
            machine.AddNode(TracingAgentStateNode.Create(
                "examine",
                "project23_state_node_examine",
                state =>
                {
                    // Copy the state so node output is a new inspectable dictionary.
                    var next = new Dictionary<string, object>(state, StringComparer.Ordinal);

                    // Store the confidence value that should trigger approval routing.
                    next[OssEquivalents.ExternalAiStateGuards.ExaminerConfidenceKey] = 0.31;

                    // Store the named missing competence that makes low confidence actionable.
                    next[OssEquivalents.ExternalAiStateGuards.MissingCompetenceKey] = "biometric passport verification";

                    // Return the updated state to the machine.
                    return Task.FromResult(next);
                },
                EmitTrace));

            // Add the approval node that records the one-request external approval marker.
            machine.AddNode(TracingAgentStateNode.Create(
                "approval",
                "project23_state_node_approval_request",
                state =>
                {
                    // Copy the state so node output is a new inspectable dictionary.
                    var next = new Dictionary<string, object>(state, StringComparer.Ordinal);

                    // Mark that this run has requested external-AI approval once.
                    next[OssEquivalents.ExternalAiStateGuards.ApprovalRequestedKey] = true;

                    // Store a readable approval status for the station output.
                    next["approval_status"] = "requires_user_approval";

                    // Return the updated state to the machine.
                    return Task.FromResult(next);
                },
                EmitTrace));

            // Route from examine to approval only when the traced guard says approval is required.
            machine.AddConditionalEdge("examine", state => guard.ShouldRequestExternalAiApproval(state), "approval");

            // Route from approval to the terminal state after the approval marker is written.
            machine.AddConditionalEdge("approval", _ => true, "done");

            // Wrap the real state machine so the state-machine boundary is inspectable.
            var tracedMachine = new TracingAgentStateMachine(machine, EmitTrace);

            // Create the initial state for this inspection run.
            var initialState = new Dictionary<string, object>(StringComparer.Ordinal)
            {
                // Store the source marker so the input panel shows this was an inspector sample.
                ["source"] = "project23-state-machine-sample"
            };

            // Push the root orchestrator as the parent for the state-machine flow.
            using (TraceContextScope.Push(new TraceContext(runId, rootStart.TraceId, rootStart.Depth + 1, 0)))
            {
                // Run the real project 23 state machine through the tracing adapter.
                var machineResult = await tracedMachine.RunAsync(initialState, "examine", "done", cancellationToken);

                // Build the normalized project 23 flow result from the state-machine output.
                var flowResult = BuildProject23FlowResult(machineResult, machine.CheckpointsSaved);

                // Emit the root orchestrator completion trace.
                EmitTrace(CompleteOrchestratorTrace(rootStart, flowResult));

                // Return the normalized result.
                return flowResult;
            }
        }
        finally
        {
            // Attempt to delete the temporary checkpoint database after the sample run.
            try
            {
                // Delete the file when SQLite has already released its handle.
                if (File.Exists(dbPath))
                    File.Delete(dbPath);
            }
            catch (IOException)
            {
                // Leave the file to OS temp cleanup when SQLite still holds a short-lived handle.
            }
        }
    }

    // Create the root station trace for one inspector command run.
    private static StationTrace CreateOrchestratorStartTrace(string runId, LogicalTopologyCommand command)
    {
        // Create a fresh trace id for the root orchestrator station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the command so the inspector can show what entered the orchestration path.
        var input = PropertyNodeSerialiser.Serialise(command);

        // Return the root start trace with no parent.
        return new StationTrace(
            traceId,
            null,
            runId,
            0,
            0,
            StationKind.Orchestrator,
            nameof(InspectorSession),
            "topology_command_orchestrator",
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Create the root station trace for a parallel add-domain batch.
    private static StationTrace CreateParallelOrchestratorStartTrace(
        string runId,
        IReadOnlyList<AddDomainCommand> commands)
    {
        // Create a fresh trace id for the root orchestrator station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the command batch so the inspector can show the batch input.
        var input = PropertyNodeSerialiser.Serialise(commands);

        // Return the root start trace with no parent.
        return new StationTrace(
            traceId,
            null,
            runId,
            0,
            0,
            StationKind.Orchestrator,
            nameof(InspectorSession),
            "topology_parallel_command_orchestrator",
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Create the flow station trace for the parallel add-domain wave.
    private static StationTrace CreateParallelFlowStartTrace(
        string runId,
        string parentTraceId,
        IReadOnlyList<AddDomainCommand> commands)
    {
        // Create a fresh trace id for the flow station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the command batch as the flow input.
        var input = PropertyNodeSerialiser.Serialise(commands);

        // Return the flow start trace under the root orchestrator.
        return new StationTrace(
            traceId,
            parentTraceId,
            runId,
            1,
            0,
            StationKind.Flow,
            "ParallelDomainMutationFlow",
            "flow-topology-parallel-domain-v1",
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Create the root station trace for the project 21 inspection sample.
    private static StationTrace CreateProject21OrchestratorStartTrace(string runId)
    {
        // Create a fresh trace id for the root orchestrator station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the sample name so the inspector can show what entered the path.
        var input = PropertyNodeSerialiser.Serialise("project21-infrastructure-sample");

        // Return the root start trace with no parent.
        return new StationTrace(
            traceId,
            null,
            runId,
            0,
            0,
            StationKind.Orchestrator,
            nameof(InspectorSession),
            "project21_inspection_orchestrator",
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Create the flow station trace for the project 21 inspection sample.
    private static StationTrace CreateProject21FlowStartTrace(string runId, string parentTraceId)
    {
        // Create a fresh trace id for the flow station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the project 21 station list as the flow input.
        var input = PropertyNodeSerialiser.Serialise(new[] { "PermissionGate.Check", "LearningEventScheduler.QueueIfReady", "LearningEventScheduler.DrainIfIdle" });

        // Return the flow start trace under the root orchestrator.
        return new StationTrace(
            traceId,
            parentTraceId,
            runId,
            1,
            0,
            StationKind.Flow,
            "Project21InfrastructureInspectionFlow",
            "flow-project21-infrastructure-inspection-v1",
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Create the root station trace for the project 22 inspection sample.
    private static StationTrace CreateProject22OrchestratorStartTrace(string runId)
    {
        // Create a fresh trace id for the root orchestrator station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the sample name so the inspector can show what entered the path.
        var input = PropertyNodeSerialiser.Serialise("project22-algorithms-sample");

        // Return the root start trace with no parent.
        return new StationTrace(
            traceId,
            null,
            runId,
            0,
            0,
            StationKind.Orchestrator,
            nameof(InspectorSession),
            "project22_inspection_orchestrator",
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Create the flow station trace for the project 22 inspection sample.
    private static StationTrace CreateProject22FlowStartTrace(string runId, string parentTraceId)
    {
        // Create a fresh trace id for the flow station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the project 22 station list as the flow input.
        var input = PropertyNodeSerialiser.Serialise(new[]
        {
            "InternalLlmGateway.InspectRouting",
            "RouteClassifier.Classify",
            "KnnSkillFilter.FilterAsync",
            "InternalResultExaminer.Examine",
            "ExternalAiRequestPackage.Create",
            "ExternalAiResultAdapter.Adapt"
        });

        // Return the flow start trace under the root orchestrator.
        return new StationTrace(
            traceId,
            parentTraceId,
            runId,
            1,
            0,
            StationKind.Flow,
            "Project22AlgorithmsInspectionFlow",
            "flow-project22-algorithms-inspection-v1",
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Create the root station trace for the project 23 inspection sample.
    private static StationTrace CreateProject23OrchestratorStartTrace(string runId)
    {
        // Create a fresh trace id for the root orchestrator station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the sample name so the inspector can show what entered the path.
        var input = PropertyNodeSerialiser.Serialise("project23-state-machine-sample");

        // Return the root start trace with no parent.
        return new StationTrace(
            traceId,
            null,
            runId,
            0,
            0,
            StationKind.Orchestrator,
            nameof(InspectorSession),
            "project23_inspection_orchestrator",
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Build the logical-flow request used by the topology mutation flow.
    private static LogicalFlowRequest<LogicalTopologyCommand> BuildTopologyFlowRequest(
        string runId,
        LogicalTopologyCommand command)
    {
        // Provide minimal metadata that explains this run came from the inspector session.
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["source"] = "orchestrator-inspector"
        };

        // Return the runtime request with default flags enabled.
        return new LogicalFlowRequest<LogicalTopologyCommand>(
            runId,
            TopologyMutationFlow.FlowId,
            TopologyMutationFlow.ScenarioId,
            command,
            new LogicalExecutionFlags(),
            metadata);
    }

    // Build a typed add-domain unit request for one command in the parallel wave.
    private static LogicalUnitRequest<AddDomainCommand> BuildAddDomainUnitRequest(
        string runId,
        AddDomainCommand command)
    {
        // Provide minimal metadata that identifies the parallel-wave inspector proof path.
        var metadata = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["source"] = "orchestrator-inspector-parallel-wave"
        };

        // Return the typed request consumed by the add-domain mutation unit.
        return new LogicalUnitRequest<AddDomainCommand>(
            runId,
            AddDomainTopologyMutationUnit.Id,
            AddDomainTopologyMutationUnit.Role,
            AddDomainTopologyMutationUnit.DomainTag,
            command,
            new LogicalExecutionFlags(),
            metadata);
    }

    // Execute one parallel unit sibling with the flow as its parent context.
    private async Task<OperationResult<LogicalUnitResult<LogicalDomainEntry>>> ExecuteParallelAddDomainUnitAsync(
        StationTrace flowStart,
        LogicalUnitRequest<AddDomainCommand> request,
        CancellationToken cancellationToken)
    {
        // Push the same flow parent and wave zero for every sibling unit.
        using (TraceContextScope.Push(new TraceContext(flowStart.RunId, flowStart.TraceId, flowStart.Depth + 1, 0)))
        {
            // Execute the traced add-domain unit.
            return await _addDomainUnit.ExecuteAsync(request, cancellationToken);
        }
    }

    // Build the flow result for a parallel add-domain batch.
    private static OperationResult<LogicalFlowResult> BuildParallelFlowResult(
        IReadOnlyList<OperationResult<LogicalUnitResult<LogicalDomainEntry>>> unitResults)
    {
        // Find the first failed unit result, if any.
        var failure = unitResults.FirstOrDefault(result => !result.IsSuccess);

        // Return the first failure through the flow envelope.
        if (failure is not null)
            return OperationResult<LogicalFlowResult>.Fail(failure.ErrorMessage, failure.ErrorCode);

        // Read the successful unit payloads.
        var values = unitResults.Select(result => result.Value!).ToArray();

        // Read stored domain ids for metrics and human output.
        var domainIds = values.Select(value => value.Output.Domain.DomainId).ToArray();

        // Build the summary for this visual parallel-wave proof.
        var summary = new LogicalFlowSummary(
            "flow-topology-parallel-domain-v1",
            1,
            0,
            domainIds.Length,
            "parallel_domain_mutation_success",
            false,
            true,
            false,
            "READY_FOR_TRACE_TREE_PANEL",
            1.0,
            "all_parallel_add_domain_units_completed",
            $"domains={string.Join(",", domainIds)}",
            0,
            0);

        // Build machine-readable metrics for the inspector proof.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["parallel_unit_count"] = domainIds.Length.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["parallel_wave_index"] = "0",
            ["stored_domain_ids"] = string.Join(",", domainIds)
        };

        // Collect evidence ids from all unit results.
        var evidenceIds = values.SelectMany(value => value.EvidenceIds).Distinct(StringComparer.Ordinal).ToArray();

        // Collect warnings from all unit results.
        var warnings = values.SelectMany(value => value.Warnings).ToArray();

        // Build the human-readable parallel flow result.
        var result = new LogicalFlowResult(
            "flow-topology-parallel-domain-v1",
            "logical_topology_parallel_domain_mutation",
            "PARALLEL_DOMAIN_MUTATIONS_APPLIED",
            $"Added or updated logical domains '{string.Join("', '", domainIds)}' in one wave.",
            summary,
            metrics,
            evidenceIds,
            warnings);

        // Return the successful parallel flow result.
        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    // BuildProject21FlowResult summarizes the inspected project 21 station outputs.
    private static OperationResult<LogicalFlowResult> BuildProject21FlowResult(
        OperationResult<PermissionDecision> permission,
        OperationResult<bool> queued,
        OperationResult<LearningDrainResult> drained)
    {
        // Return permission failure through the flow envelope.
        if (!permission.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(permission.ErrorMessage, permission.ErrorCode);

        // Return scheduler queue failure through the flow envelope.
        if (!queued.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(queued.ErrorMessage, queued.ErrorCode);

        // Return scheduler drain failure through the flow envelope.
        if (!drained.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(drained.ErrorMessage, drained.ErrorCode);

        // Build the summary for this project 21 inspection path.
        var summary = new LogicalFlowSummary(
            "flow-project21-infrastructure-inspection-v1",
            1,
            0,
            3,
            "project21_station_trace_count",
            false,
            true,
            false,
            "READY_FOR_PROJECT22_TRACES",
            1.0,
            "permission_gate_and_learning_scheduler_traced",
            "project21 infrastructure trace complete",
            0,
            0);

        // Build machine-readable metrics for the project 21 inspection path.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["permission_decision"] = permission.Value!.ToString(),
            ["learning_event_queued"] = queued.Value!.ToString(),
            ["learning_drain_did_run"] = drained.Value!.DidRun.ToString()
        };

        // Build the human-readable result for the trace card output.
        var result = new LogicalFlowResult(
            "flow-project21-infrastructure-inspection-v1",
            "project21_permission_and_learning_scheduler",
            "PROJECT21_STATIONS_TRACED",
            "PermissionGate and LearningEventScheduler emitted inspectable station traces.",
            summary,
            metrics,
            ["PHASE-73"],
            []);

        // Return the successful project 21 inspection result.
        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    // BuildProject22EarlyFailure converts the first project 22 failure into the inspector flow envelope.
    private static OperationResult<LogicalFlowResult> BuildProject22EarlyFailure(
        OperationResult<InternalLlmRoutingInspection> gatewayRouting,
        AgentAlgorithms.OperationResult<AgentAlgorithms.DomainClassification> route,
        AgentAlgorithms.OperationResult<IReadOnlyList<AgentAlgorithms.TextChunk>> filtered,
        AgentAlgorithms.OperationResult<AgentAlgorithms.ResultExamination> examination)
    {
        // Return gateway failure when internal routing did not produce an adapter decision.
        if (!gatewayRouting.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(gatewayRouting.ErrorMessage, gatewayRouting.ErrorCode);

        // Return route failure when classification did not produce a domain.
        if (!route.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(route.ErrorMessage, route.ErrorCode);

        // Return filter failure when skill selection did not produce candidates.
        if (!filtered.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(filtered.ErrorMessage, filtered.ErrorCode);

        // Return examiner failure when result examination did not produce a confidence report.
        return OperationResult<LogicalFlowResult>.Fail(examination.ErrorMessage, examination.ErrorCode);
    }

    // BuildProject22FlowResult summarizes the inspected project 22 station outputs.
    private static OperationResult<LogicalFlowResult> BuildProject22FlowResult(
        OperationResult<InternalLlmRoutingInspection> gatewayRouting,
        AgentAlgorithms.OperationResult<AgentAlgorithms.DomainClassification> route,
        AgentAlgorithms.OperationResult<IReadOnlyList<AgentAlgorithms.TextChunk>> filtered,
        AgentAlgorithms.OperationResult<AgentAlgorithms.ResultExamination> examination,
        AgentAlgorithms.OperationResult<ExternalAiRequestPackageInspection> inspectedPackage,
        AgentAlgorithms.OperationResult<AgentAlgorithms.PreprocessedLearningData> adapted)
    {
        // Return package failure through the flow envelope.
        if (!inspectedPackage.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(inspectedPackage.ErrorMessage, inspectedPackage.ErrorCode);

        // Return adapter failure through the flow envelope.
        if (!adapted.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(adapted.ErrorMessage, adapted.ErrorCode);

        // Build the summary for this project 22 inspection path.
        var summary = new LogicalFlowSummary(
            "flow-project22-algorithms-inspection-v1",
            1,
            0,
            6,
            "project22_station_trace_count",
            false,
            true,
            false,
            "READY_FOR_PROJECT23_TRACES",
            1.0,
            "route_knn_examiner_package_and_adapter_traced",
            "project22 algorithms trace complete",
            0,
            0);

        // Build machine-readable metrics for the project 22 inspection path.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["internal_gateway_domain"] = gatewayRouting.Value!.Domain.ToString(),
            ["internal_gateway_adapter"] = gatewayRouting.Value!.AdapterPath,
            ["route_label"] = route.Value!.Label.ToString(),
            ["route_confidence"] = route.Value!.Confidence.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
            ["filtered_skill_count"] = filtered.Value!.Count.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["examiner_needs_external_ai"] = examination.Value!.NeedsExternalAi.ToString(),
            ["approval_scope"] = inspectedPackage.Value!.Package.ApprovalScope,
            ["adapter_signal_type"] = adapted.Value!.SignalType,
            ["adapter_predicate_count"] = adapted.Value!.Predicates.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        // Build the human-readable result for the trace card output.
        var result = new LogicalFlowResult(
            "flow-project22-algorithms-inspection-v1",
            "project22_internal_first_algorithm_chain",
            "PROJECT22_STATIONS_TRACED",
            "InternalLlmGateway routing, RouteClassifier, KnnSkillFilter, InternalResultExaminer, ExternalAiRequestPackage, and ExternalAiResultAdapter emitted inspectable station traces.",
            summary,
            metrics,
            ["PHASE-78"],
            []);

        // Return the successful project 22 inspection result.
        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    // BuildProject23FlowResult summarizes the inspected project 23 state-machine outputs.
    private static OperationResult<LogicalFlowResult> BuildProject23FlowResult(
        OssEquivalents.OperationResult<Dictionary<string, object>> machineResult,
        int checkpointsSaved)
    {
        // Return state-machine failure through the flow envelope.
        if (!machineResult.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(machineResult.ErrorMessage, machineResult.ErrorCode);

        // Read the final state from the project 23 result.
        var state = machineResult.Value!;

        // Read whether approval was requested in the final state.
        var approvalRequested = state.TryGetValue(OssEquivalents.ExternalAiStateGuards.ApprovalRequestedKey, out var approvalValue) &&
                                approvalValue is bool approvalBool &&
                                approvalBool;

        // Read the final missing competence value for metrics.
        var missingCompetence = state.TryGetValue(OssEquivalents.ExternalAiStateGuards.MissingCompetenceKey, out var missingValue) &&
                                missingValue is string missingText
            ? missingText
            : "";

        // Build the summary for this project 23 inspection path.
        var summary = new LogicalFlowSummary(
            "flow-project23-state-machine-inspection-v1",
            1,
            0,
            4,
            "project23_station_trace_count",
            false,
            true,
            false,
            "READY_FOR_NESTED_REGISTRY_ORCHESTRATOR",
            1.0,
            "state_machine_nodes_and_guard_traced",
            "project23 state machine trace complete",
            0,
            0);

        // Build machine-readable metrics for the project 23 inspection path.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["approval_requested"] = approvalRequested.ToString(),
            ["checkpoints_saved"] = checkpointsSaved.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["missing_competence"] = missingCompetence,
            ["final_state_key_count"] = state.Count.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };

        // Build the human-readable result for the trace card output.
        var result = new LogicalFlowResult(
            "flow-project23-state-machine-inspection-v1",
            "project23_state_machine_approval_routing",
            "PROJECT23_STATIONS_TRACED",
            "AgentStateMachine, examine node, external-AI state guard, and approval-request node emitted inspectable station traces.",
            summary,
            metrics,
            ["PHASE-79"],
            []);

        // Return the successful project 23 inspection result.
        return OperationResult<LogicalFlowResult>.Ok(result);
    }

    // Convert the parallel flow result into the matching flow completion trace.
    private static StationTrace CompleteParallelFlowTrace(
        StationTrace start,
        OperationResult<LogicalFlowResult> result)
    {
        // Serialise the full flow result envelope for the inspector output panel.
        var output = PropertyNodeSerialiser.Serialise(result);

        // Copy the flow trace identity and attach completion information.
        return start with
        {
            Output = output,
            ErrorCode = result.IsSuccess ? null : result.ErrorCode,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
            CompletedAt = DateTimeOffset.UtcNow,
            Resources = StationResourceSampler.Capture()
        };
    }

    // Convert the project 21 flow result into the matching flow completion trace.
    private static StationTrace CompleteProject21FlowTrace(
        StationTrace start,
        OperationResult<LogicalFlowResult> result)
    {
        // Serialise the full flow result envelope for the inspector output panel.
        var output = PropertyNodeSerialiser.Serialise(result);

        // Copy the flow trace identity and attach completion information.
        return start with
        {
            Output = output,
            ErrorCode = result.IsSuccess ? null : result.ErrorCode,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
            CompletedAt = DateTimeOffset.UtcNow,
            Resources = StationResourceSampler.Capture()
        };
    }

    // Convert the project 22 flow result into the matching flow completion trace.
    private static StationTrace CompleteProject22FlowTrace(
        StationTrace start,
        OperationResult<LogicalFlowResult> result)
    {
        // Serialise the full flow result envelope for the inspector output panel.
        var output = PropertyNodeSerialiser.Serialise(result);

        // Copy the flow trace identity and attach completion information.
        return start with
        {
            Output = output,
            ErrorCode = result.IsSuccess ? null : result.ErrorCode,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
            CompletedAt = DateTimeOffset.UtcNow,
            Resources = StationResourceSampler.Capture()
        };
    }

    // Convert the root flow result into the root completion trace.
    private static StationTrace CompleteOrchestratorTrace(
        StationTrace start,
        OperationResult<LogicalFlowResult> result)
    {
        // Serialise the full flow result envelope for the inspector output panel.
        var output = PropertyNodeSerialiser.Serialise(result);

        // Copy the root trace identity and attach completion information.
        return start with
        {
            Output = output,
            ErrorCode = result.IsSuccess ? null : result.ErrorCode,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
            CompletedAt = DateTimeOffset.UtcNow,
            Resources = StationResourceSampler.Capture()
        };
    }

    // Emit one trace event to all subscribers.
    private void EmitTrace(StationTrace trace)
    {
        // Raise the event with this session as the source so UI consumers can subscribe once.
        StationTraced?.Invoke(this, trace);
    }
}
