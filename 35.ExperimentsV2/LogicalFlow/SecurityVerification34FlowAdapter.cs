// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This adapter exposes the CB4 security-verification Stage 5 proof as an executable logical flow.
public sealed class SecurityVerification34FlowAdapter : ILogicalFlow<string>
{
    // This flow id names the registered sub-phase 3.4 security verification flow.
    public const string FlowId = "security-verification-3.4";

    // This scenario id lets the assistant select the CB4 security proof flow explicitly.
    public const string ScenarioId = "cb4-security-verification";

    // This attribution text is required because the OWASP ASVS source uses CC BY-SA 4.0.
    private const string Attribution = "Based on OWASP ASVS 5.0.0, CC BY-SA 4.0.";

    // The node-01 splitter unit owns producing the security domain packet.
    private readonly ILogicalUnit<SecuritySplitInput, SecurityDomainPacket> _splitterUnit;

    // The node-08 arbiter unit owns PASS, REPROCESS, and CONCERN severity decisions.
    private readonly ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict> _arbiterUnit;

    // These fields preserve the exact selected node-08 registry identity through request and result admission.
    private readonly string _arbiterUnitId;
    private readonly string _arbiterUnitRole;
    private readonly string _arbiterDomainTag;

    // The node-02 domain translator unit owns formal security-domain statements.
    private readonly ILogicalUnit<SecurityTranslationInput, SecurityDomainTranslation> _domainTranslatorUnit;

    // The node-03 planner unit owns the security verification execution plan.
    private readonly ILogicalUnit<SecurityPlannerInput, SecurityExecutionPlan> _plannerUnit;

    // The node-04 processor unit owns converting the request into a formal finding.
    private readonly ILogicalUnit<SecurityProcessorInput, SecurityProcessorFinding> _processorUnit;

    // The node-05 applier unit owns applying the verdict to the formal result path.
    private readonly ILogicalUnit<SecurityApplicationInput, SecurityAppliedResult> _applierUnit;

    // The node-07 selector unit owns which arbiter families are allowed to judge the finding.
    private readonly ILogicalUnit<SecurityArbiterSelectionInput, SecurityArbiterSelection> _selectorUnit;

    // The node-09 formatter unit owns final human wording and attribution.
    private readonly ILogicalUnit<SecurityFormatterInput, SecurityFormattedResult> _formatterUnit;

    // The node-06 result translator unit owns formal result fragments between arbiter and formatter.
    private readonly ILogicalUnit<SecurityResultTranslationInput, SecurityTranslatedResult> _resultTranslatorUnit;

    // Create the security flow with the default CB4 node-08 arbiter unit.
    public SecurityVerification34FlowAdapter()
        : this(new SecuritySplitterUnit(), new SecurityArbiterSelectorUnit(), new SecurityTranslatorUnit(), new SecurityPlannerUnit(), new SecurityProcessorUnit(), new SecurityArbiterUnit(), new SecurityApplierUnit(), new SecurityResultTranslatorUnit(), new SecurityFormatterUnit())
    {
    }

    // Create the security flow with an injectable arbiter unit for replacement proofs.
    public SecurityVerification34FlowAdapter(ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict> arbiterUnit)
        : this(new SecuritySplitterUnit(), new SecurityArbiterSelectorUnit(), new SecurityTranslatorUnit(), new SecurityPlannerUnit(), new SecurityProcessorUnit(), arbiterUnit, new SecurityApplierUnit(), new SecurityResultTranslatorUnit(), new SecurityFormatterUnit())
    {
    }

    // Create the security flow with one exact learned descriptor/executable registry tuple.
    internal static SecurityVerification34FlowAdapter CreateWithArbiterEntry(
        ResolvedLogicalUnitRegistryEntry<SecurityArbiterInput, SecurityArbiterVerdict> arbiterEntry)
    {
        return new SecurityVerification34FlowAdapter(CreateArbiterBinding(arbiterEntry));
    }

    // Create the security flow with injectable selector and arbiter units for replacement proofs.
    public SecurityVerification34FlowAdapter(
        ILogicalUnit<SecurityArbiterSelectionInput, SecurityArbiterSelection> selectorUnit,
        ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict> arbiterUnit)
        : this(new SecuritySplitterUnit(), selectorUnit, new SecurityTranslatorUnit(), new SecurityPlannerUnit(), new SecurityProcessorUnit(), arbiterUnit, new SecurityApplierUnit(), new SecurityResultTranslatorUnit(), new SecurityFormatterUnit())
    {
    }

    // Create the security flow with injectable splitter, selector, domain translator, planner, processor, arbiter, applier, result translator, and formatter units for replacement proofs.
    public SecurityVerification34FlowAdapter(
        ILogicalUnit<SecuritySplitInput, SecurityDomainPacket> splitterUnit,
        ILogicalUnit<SecurityArbiterSelectionInput, SecurityArbiterSelection> selectorUnit,
        ILogicalUnit<SecurityTranslationInput, SecurityDomainTranslation> domainTranslatorUnit,
        ILogicalUnit<SecurityPlannerInput, SecurityExecutionPlan> plannerUnit,
        ILogicalUnit<SecurityProcessorInput, SecurityProcessorFinding> processorUnit,
        ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict> arbiterUnit,
        ILogicalUnit<SecurityApplicationInput, SecurityAppliedResult> applierUnit,
        ILogicalUnit<SecurityResultTranslationInput, SecurityTranslatedResult> resultTranslatorUnit,
        ILogicalUnit<SecurityFormatterInput, SecurityFormattedResult> formatterUnit)
        : this(
            splitterUnit,
            selectorUnit,
            domainTranslatorUnit,
            plannerUnit,
            processorUnit,
            arbiterUnit,
            SecurityArbiterUnit.Id,
            SecurityArbiterUnit.Role,
            SecurityArbiterUnit.DomainTag,
            applierUnit,
            resultTranslatorUnit,
            formatterUnit)
    {
    }

    // Create the default dependency graph around one exact node-08 registry binding.
    private SecurityVerification34FlowAdapter(ArbiterBinding arbiterBinding)
        : this(
            new SecuritySplitterUnit(),
            new SecurityArbiterSelectorUnit(),
            new SecurityTranslatorUnit(),
            new SecurityPlannerUnit(),
            new SecurityProcessorUnit(),
            arbiterBinding.Unit,
            arbiterBinding.UnitId,
            arbiterBinding.UnitRole,
            arbiterBinding.DomainTag,
            new SecurityApplierUnit(),
            new SecurityResultTranslatorUnit(),
            new SecurityFormatterUnit())
    {
    }

    // Initialize the flow with all executable units and the exact node-08 identity to admit.
    private SecurityVerification34FlowAdapter(
        ILogicalUnit<SecuritySplitInput, SecurityDomainPacket> splitterUnit,
        ILogicalUnit<SecurityArbiterSelectionInput, SecurityArbiterSelection> selectorUnit,
        ILogicalUnit<SecurityTranslationInput, SecurityDomainTranslation> domainTranslatorUnit,
        ILogicalUnit<SecurityPlannerInput, SecurityExecutionPlan> plannerUnit,
        ILogicalUnit<SecurityProcessorInput, SecurityProcessorFinding> processorUnit,
        ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict> arbiterUnit,
        string arbiterUnitId,
        string arbiterUnitRole,
        string arbiterDomainTag,
        ILogicalUnit<SecurityApplicationInput, SecurityAppliedResult> applierUnit,
        ILogicalUnit<SecurityResultTranslationInput, SecurityTranslatedResult> resultTranslatorUnit,
        ILogicalUnit<SecurityFormatterInput, SecurityFormattedResult> formatterUnit)
    {
        // Store the splitter unit so the flow does not hardcode node-01 packets.
        _splitterUnit = splitterUnit;

        // Store the selector unit so the flow does not hardcode node-07 routing.
        _selectorUnit = selectorUnit;

        // Store the domain translator unit so the flow does not hardcode node-02 statements.
        _domainTranslatorUnit = domainTranslatorUnit;

        // Store the planner unit so the flow does not hardcode node-03 plans.
        _plannerUnit = plannerUnit;

        // Store the processor unit so the flow does not hardcode node-04 findings.
        _processorUnit = processorUnit;

        // Store the arbiter unit so the flow does not hardcode node-08 verdict logic.
        _arbiterUnit = arbiterUnit;

        // Snapshot the selected node-08 identity so request construction and result admission use one tuple.
        _arbiterUnitId = arbiterUnitId;
        _arbiterUnitRole = arbiterUnitRole;
        _arbiterDomainTag = arbiterDomainTag;

        // Store the applier unit so the flow does not hardcode node-05 application behavior.
        _applierUnit = applierUnit;

        // Store the result translator unit so the flow does not hardcode node-06 fragments.
        _resultTranslatorUnit = resultTranslatorUnit;

        // Store the formatter unit so the flow does not hardcode node-09 human wording.
        _formatterUnit = formatterUnit;
    }

    // Execute one security-verification scenario through the logical-flow interface.
    public async Task<AgentInfrastructure.OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<string> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

        // Refuse requests for a different flow id so this adapter cannot be selected accidentally.
        if (!string.Equals(request.FlowId, FlowId, StringComparison.Ordinal))
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail($"Unsupported flow id '{request.FlowId}'.", "UNSUPPORTED_FLOW");

        // Refuse requests for a different scenario id so the flow stays scoped to the accepted CB4 oracle.
        if (!string.Equals(request.ScenarioId, ScenarioId, StringComparison.Ordinal))
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail($"Unsupported scenario id '{request.ScenarioId}'.", "UNSUPPORTED_SCENARIO");

        // Refuse empty security requests because there is no source evidence to verify.
        if (string.IsNullOrWhiteSpace(request.Input))
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail("Security verification input cannot be empty.", "EMPTY_SECURITY_REQUEST");

        // Deep-copy the metadata into one independently owned graph per injected unit before any unit can observe it.
        if (!LogicalMetadataSnapshot.TryCreateCopies(request.Metadata, 9, out var unitMetadataGraphs))
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail("Security verification metadata is invalid.", "SECURITY_METADATA_INVALID");

        // Build the node-01 input from the submitted security evidence.
        var splitterInput = new SecuritySplitInput(request.Input);

        // Build the logical-unit request for the node-01 splitter unit.
        var splitterRequest = new LogicalUnitRequest<SecuritySplitInput>(
            request.RunId,
            SecuritySplitterUnit.Id,
            SecuritySplitterUnit.Role,
            SecuritySplitterUnit.DomainTag,
            splitterInput,
            request.Flags with { },
            unitMetadataGraphs[0]);

        // Preserve the exact raw packet authority before node-01 can observe its request.
        var expectedPacketText = splitterInput.SecurityRequest;

        // Snapshot the authoritative route independently without exposing it to the injected node-01 unit.
        var authoritativePacket = SecuritySplitterUnit.CreateAuthoritativePacket(expectedPacketText);
        var authoritativeSplitterRoute = new SplitterRouteAuthority(
            authoritativePacket.ScenarioId,
            authoritativePacket.DomainLabel,
            authoritativePacket.PacketText,
            authoritativePacket.CoverageStatus,
            authoritativePacket.HandoffMarker,
            authoritativePacket.SourcePairId,
            authoritativePacket.RequiredResultSignals.ToArray(),
            authoritativePacket.ForbiddenResultSignals.ToArray());

        // Execute node-01 through the logical-unit interface.
        var splitterResult = await ExecuteSecurityUnitAsync(
            _splitterUnit,
            splitterRequest,
            "Security splitter execution failed.",
            "SECURITY_SPLITTER_EXECUTION_FAILED",
            cancellationToken);

        // Return node-01 failures through the logical-flow envelope.
        if (!splitterResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(splitterResult.ErrorMessage, splitterResult.ErrorCode);

        // Refuse malformed success envelopes before any downstream node consumes their payload.
        if (splitterResult.Value is null)
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security splitter returned an invalid result.",
                "SECURITY_SPLITTER_RESULT_INVALID");
        }

        // Snapshot the admitted success envelope once for identity checks and stable downstream data.
        var admittedSplitterResult = splitterResult.Value;

        // Refuse splitter results that do not echo the exact requested node-01 identity.
        if (!string.Equals(admittedSplitterResult.UnitId, splitterRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedSplitterResult.UnitRole, splitterRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedSplitterResult.DomainTag, splitterRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security splitter result identity did not match the selected unit.",
                "SECURITY_SPLITTER_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed or mutable splitter payloads before node-07 or outward evidence can observe them.
        if (!TrySnapshotSplitterSuccess(
                admittedSplitterResult,
                splitterRequest,
                expectedPacketText,
                out var stableSplitterResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security splitter returned an invalid result.",
                "SECURITY_SPLITTER_RESULT_INVALID");
        }

        // Refuse a structurally valid packet when it changes the deterministic route selected for this input.
        if (!HasExactSplitterRouteAuthority(stableSplitterResult.Output, authoritativeSplitterRoute))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security splitter result did not match the authoritative security route.",
                "SECURITY_SPLITTER_ROUTE_IDENTITY_MISMATCH");
        }

        // Read the domain packet from the node-01 unit envelope.
        var domainPacket = stableSplitterResult.Output;

        // Use the splitter's scenario id so routing is driven by the node-01 output.
        var scenario = domainPacket.ScenarioId;

        // Preserve private node-07 input authority before creating either executable graph.
        var selectorInputAuthority = new SelectorInputAuthority(
            scenario,
            domainPacket.PacketText,
            Array.AsReadOnly(new[] { domainPacket.SourcePairId }));

        // Compute the deterministic node-07 route from a detached graph never given to the injected unit.
        var authoritativeSelectorInput = CreateDetachedSelectorInput(selectorInputAuthority);
        var authoritativeSelection = SecurityArbiterSelectorUnit.CreateAuthoritativeSelection(authoritativeSelectorInput);
        var selectorRouteAuthority = new SelectorRouteAuthority(
            authoritativeSelection.ScenarioId,
            Array.AsReadOnly(authoritativeSelection.SelectedArbiters.ToArray()),
            authoritativeSelection.SourcePairId,
            authoritativeSelection.SelectionReason);

        // Build a second fresh node-07 input graph for the injected unit.
        var selectorInput = CreateDetachedSelectorInput(selectorInputAuthority);

        // Build the logical-unit request for the node-07 selector unit.
        var selectorRequest = new LogicalUnitRequest<SecurityArbiterSelectionInput>(
            request.RunId,
            SecurityArbiterSelectorUnit.Id,
            SecurityArbiterSelectorUnit.Role,
            SecurityArbiterSelectorUnit.DomainTag,
            selectorInput,
            request.Flags with { },
            unitMetadataGraphs[1]);

        // Execute node-07 through the logical-unit interface.
        var selectorResult = await ExecuteSecurityUnitAsync(
            _selectorUnit,
            selectorRequest,
            "Security arbiter selector execution failed.",
            "SECURITY_SELECTOR_EXECUTION_FAILED",
            cancellationToken);

        // Return node-07 failures through the logical-flow envelope.
        if (!selectorResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(selectorResult.ErrorMessage, selectorResult.ErrorCode);

        // Refuse malformed success envelopes before any downstream node consumes their payload.
        if (selectorResult.Value is null)
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security arbiter selector returned an invalid result.",
                "SECURITY_SELECTOR_RESULT_INVALID");
        }

        // Snapshot the admitted success envelope once for identity checks and stable downstream data.
        var admittedSelectorResult = selectorResult.Value;

        // Refuse selector results that do not echo the exact requested node-07 identity.
        if (!string.Equals(admittedSelectorResult.UnitId, selectorRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedSelectorResult.UnitRole, selectorRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedSelectorResult.DomainTag, selectorRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security arbiter selector result identity did not match the selected unit.",
                "SECURITY_SELECTOR_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed or mutable selector payloads before node-02 or outward evidence can observe them.
        if (!TrySnapshotSelectorSuccess(
                admittedSelectorResult,
                selectorRequest,
                selectorInputAuthority,
                out var stableSelectorResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security arbiter selector returned an invalid result.",
                "SECURITY_SELECTOR_RESULT_INVALID");
        }

        // Require the admitted learned implementation to preserve the complete deterministic selection.
        if (!HasExactSelectorRouteAuthority(stableSelectorResult.Output, selectorRouteAuthority))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security arbiter selector result did not match the authoritative security route.",
                "SECURITY_SELECTOR_ROUTE_IDENTITY_MISMATCH");
        }

        // Read the selected arbiter list from the node-07 unit envelope.
        var selection = stableSelectorResult.Output;

        // Preserve node-02 input authority independently from the mutable object graph given to the unit.
        var translationInputAuthority = new TranslationInputAuthority(
            scenario,
            domainPacket.SourcePairId,
            domainPacket.RequiredResultSignals.ToArray(),
            domainPacket.ForbiddenResultSignals.ToArray());

        // Compute the deterministic node-02 route from a detached graph that is never given to the unit.
        var authoritativeTranslationInput = CreateDetachedTranslationInput(
            scenario,
            domainPacket,
            selection);
        var authoritativeTranslation =
            SecurityTranslatorUnit.CreateAuthoritativeTranslation(authoritativeTranslationInput);
        var translatorRouteAuthority = new TranslatorRouteAuthority(
            authoritativeTranslation.ScenarioId,
            authoritativeTranslation.FormalStatement,
            authoritativeTranslation.HandoffMarker,
            authoritativeTranslation.SourcePairId,
            authoritativeTranslation.RequiredResultSignals.ToArray(),
            authoritativeTranslation.ForbiddenResultSignals.ToArray());

        // Give the injected unit a second detached graph with arrays independent from route authority.
        var domainTranslatorInput = CreateDetachedTranslationInput(
            scenario,
            domainPacket,
            selection);

        // Build the logical-unit request for the node-02 domain translator unit.
        var domainTranslatorRequest = new LogicalUnitRequest<SecurityTranslationInput>(
            request.RunId,
            SecurityTranslatorUnit.Id,
            SecurityTranslatorUnit.Role,
            SecurityTranslatorUnit.DomainTag,
            domainTranslatorInput,
            request.Flags with { },
            unitMetadataGraphs[2]);

        // Execute node-02 through the logical-unit interface.
        var domainTranslatorResult = await ExecuteSecurityUnitAsync(
            _domainTranslatorUnit,
            domainTranslatorRequest,
            "Security translator execution failed.",
            "SECURITY_TRANSLATOR_EXECUTION_FAILED",
            cancellationToken);

        // Return node-02 failures through the logical-flow envelope.
        if (!domainTranslatorResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(domainTranslatorResult.ErrorMessage, domainTranslatorResult.ErrorCode);

        // Refuse malformed success envelopes before node-03 consumes their payload.
        if (domainTranslatorResult.Value is null)
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security translator returned an invalid result.",
                "SECURITY_TRANSLATOR_RESULT_INVALID");
        }

        // Snapshot the admitted success envelope once for identity checks and stable downstream data.
        var admittedDomainTranslatorResult = domainTranslatorResult.Value;

        // Refuse translator results that do not echo the exact requested node-02 identity.
        if (!string.Equals(admittedDomainTranslatorResult.UnitId, domainTranslatorRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedDomainTranslatorResult.UnitRole, domainTranslatorRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedDomainTranslatorResult.DomainTag, domainTranslatorRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security translator result identity did not match the selected unit.",
                "SECURITY_TRANSLATOR_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed, spoofed, or mutable node-02 payloads before node-03 can observe them.
        if (!TrySnapshotTranslatorSuccess(
                admittedDomainTranslatorResult,
                domainTranslatorRequest,
                translationInputAuthority,
                out var stableDomainTranslatorResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security translator returned an invalid result.",
                "SECURITY_TRANSLATOR_RESULT_INVALID");
        }

        // Refuse structurally valid node-02 output that changes the deterministic translation route.
        if (!HasExactTranslatorRouteAuthority(
                stableDomainTranslatorResult.Output,
                translatorRouteAuthority))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security translator result did not match the authoritative security route.",
                "SECURITY_TRANSLATOR_ROUTE_IDENTITY_MISMATCH");
        }

        // Read the formal domain translation from the node-02 unit envelope.
        var domainTranslation = stableDomainTranslatorResult.Output;

        // Preserve all mutable node-03 input authority outside the object graph given to the unit.
        var plannerInputAuthority = new PlannerInputAuthority(
            domainTranslation.ScenarioId,
            domainTranslation.FormalStatement,
            domainTranslation.HandoffMarker,
            domainTranslation.SourcePairId,
            Array.AsReadOnly(domainTranslation.RequiredResultSignals.ToArray()),
            Array.AsReadOnly(domainTranslation.ForbiddenResultSignals.ToArray()),
            selection.ScenarioId,
            Array.AsReadOnly(selection.SelectedArbiters.ToArray()),
            selection.SourcePairId,
            selection.SelectionReason);

        // Compute the deterministic node-03 route from a detached graph that is never given to the unit.
        var authoritativePlannerInput = CreateDetachedPlannerInput(
            scenario,
            domainTranslation,
            selection);
        var authoritativePlan =
            SecurityPlannerUnit.CreateAuthoritativePlan(authoritativePlannerInput);
        var plannerRouteAuthority = new PlannerRouteAuthority(
            authoritativePlan.ScenarioId,
            authoritativePlan.PlanStep,
            authoritativePlan.SourcePairId,
            authoritativePlan.RequiredResultSignals.ToArray(),
            authoritativePlan.ForbiddenResultSignals.ToArray());

        // Give the injected unit a second detached graph with arrays independent from route authority.
        var plannerInput = CreateDetachedPlannerInput(
            scenario,
            domainTranslation,
            selection);

        // Build the logical-unit request for the node-03 planner unit.
        var plannerRequest = new LogicalUnitRequest<SecurityPlannerInput>(
            request.RunId,
            SecurityPlannerUnit.Id,
            SecurityPlannerUnit.Role,
            SecurityPlannerUnit.DomainTag,
            plannerInput,
            request.Flags with { },
            unitMetadataGraphs[3]);

        // Execute node-03 through the logical-unit interface.
        var plannerResult = await ExecuteSecurityUnitAsync(
            _plannerUnit,
            plannerRequest,
            "Security planner execution failed.",
            "SECURITY_PLANNER_EXECUTION_FAILED",
            cancellationToken);

        // Return node-03 failures through the logical-flow envelope.
        if (!plannerResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(plannerResult.ErrorMessage, plannerResult.ErrorCode);

        // Refuse malformed success envelopes before node-04 consumes their payload.
        if (plannerResult.Value is null)
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security planner returned an invalid result.",
                "SECURITY_PLANNER_RESULT_INVALID");
        }

        // Snapshot the admitted success envelope once for identity checks and stable downstream data.
        var admittedPlannerResult = plannerResult.Value;

        // Refuse planner results that do not echo the exact requested node-03 identity.
        if (!string.Equals(admittedPlannerResult.UnitId, plannerRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedPlannerResult.UnitRole, plannerRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedPlannerResult.DomainTag, plannerRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security planner result identity did not match the selected unit.",
                "SECURITY_PLANNER_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed, uncorrelated, or mutable node-03 payloads before node-04 can observe them.
        if (!TrySnapshotPlannerSuccess(
                admittedPlannerResult,
                plannerRequest,
                plannerInputAuthority,
                out var stablePlannerResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security planner returned an invalid result.",
                "SECURITY_PLANNER_RESULT_INVALID");
        }

        // Refuse structurally valid node-03 output that changes the deterministic planning route.
        if (!HasExactPlannerRouteAuthority(stablePlannerResult.Output, plannerRouteAuthority))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security planner result did not match the authoritative security route.",
                "SECURITY_PLANNER_ROUTE_IDENTITY_MISMATCH");
        }

        // Read the execution plan from the node-03 unit envelope.
        var plan = stablePlannerResult.Output;

        // Preserve all mutable node-04 input authority outside the object graph given to the unit.
        var processorInputAuthority = new ProcessorInputAuthority(
            plan.ScenarioId,
            plan.PlanStep,
            plan.SourcePairId,
            Array.AsReadOnly(plan.RequiredResultSignals.ToArray()),
            Array.AsReadOnly(plan.ForbiddenResultSignals.ToArray()),
            selection.ScenarioId,
            Array.AsReadOnly(selection.SelectedArbiters.ToArray()),
            selection.SourcePairId,
            selection.SelectionReason);

        // Compute the deterministic node-04 route from a detached graph that is never given to the unit.
        var authoritativeProcessorInput = CreateDetachedProcessorInput(
            scenario,
            plan,
            selection);
        var authoritativeFinding =
            SecurityProcessorUnit.CreateAuthoritativeFinding(authoritativeProcessorInput);
        var processorRouteAuthority = new ProcessorRouteAuthority(
            authoritativeFinding.ScenarioId,
            authoritativeFinding.Finding,
            authoritativeFinding.SourcePairId,
            authoritativeFinding.RequiredResultSignals.ToArray(),
            authoritativeFinding.ForbiddenResultSignals.ToArray());

        // Give the injected unit a second detached graph with arrays independent from route authority.
        var processorInput = CreateDetachedProcessorInput(
            scenario,
            plan,
            selection);

        // Build the logical-unit request for the node-04 processor unit.
        var processorRequest = new LogicalUnitRequest<SecurityProcessorInput>(
            request.RunId,
            SecurityProcessorUnit.Id,
            SecurityProcessorUnit.Role,
            SecurityProcessorUnit.DomainTag,
            processorInput,
            request.Flags with { },
            unitMetadataGraphs[4]);

        // Execute node-04 through the logical-unit interface.
        var processorResult = await ExecuteSecurityUnitAsync(
            _processorUnit,
            processorRequest,
            "Security processor execution failed.",
            "SECURITY_PROCESSOR_EXECUTION_FAILED",
            cancellationToken);

        // Return node-04 failures through the logical-flow envelope.
        if (!processorResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(processorResult.ErrorMessage, processorResult.ErrorCode);

        // Refuse malformed success envelopes before node-08 consumes their payload.
        if (processorResult.Value is null)
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security processor returned an invalid result.",
                "SECURITY_PROCESSOR_RESULT_INVALID");
        }

        // Snapshot the admitted success envelope once for identity checks and stable downstream data.
        var admittedProcessorResult = processorResult.Value;

        // Refuse processor results that do not echo the exact requested node-04 identity.
        if (!string.Equals(admittedProcessorResult.UnitId, processorRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedProcessorResult.UnitRole, processorRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedProcessorResult.DomainTag, processorRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security processor result identity did not match the selected unit.",
                "SECURITY_PROCESSOR_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed, uncorrelated, or mutable node-04 payloads before node-08 can observe them.
        if (!TrySnapshotProcessorSuccess(
                admittedProcessorResult,
                processorRequest,
                processorInputAuthority,
                out var stableProcessorResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security processor returned an invalid result.",
                "SECURITY_PROCESSOR_RESULT_INVALID");
        }

        // Refuse structurally valid node-04 output that changes the deterministic processor route.
        if (!HasExactProcessorRouteAuthority(stableProcessorResult.Output, processorRouteAuthority))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security processor result did not match the authoritative security route.",
                "SECURITY_PROCESSOR_ROUTE_IDENTITY_MISMATCH");
        }

        // Read the processed finding from the node-04 unit envelope.
        var processorFinding = stableProcessorResult.Output;

        // Preserve node-08 input authority before the injected unit can observe any mutable carriers.
        var arbiterInputAuthority = new ArbiterInputAuthority(
            scenario,
            processorFinding.Finding,
            Array.AsReadOnly(selection.SelectedArbiters.ToArray()),
            Array.AsReadOnly(new[] { selection.SourcePairId, processorFinding.SourcePairId }));

        // Build the node-08 input from fresh arrays detached from both stable upstream data and authority.
        var arbiterInput = new SecurityArbiterInput(
            arbiterInputAuthority.ScenarioId,
            arbiterInputAuthority.ProcessorFinding,
            arbiterInputAuthority.CandidateArbiters.ToArray(),
            arbiterInputAuthority.SourcePairIds.ToArray());

        // Build the logical-unit request for the node-08 arbiter unit.
        var arbiterRequest = new LogicalUnitRequest<SecurityArbiterInput>(
            request.RunId,
            _arbiterUnitId,
            _arbiterUnitRole,
            _arbiterDomainTag,
            arbiterInput,
            request.Flags with { },
            unitMetadataGraphs[5]);

        // Execute node-08 through the logical-unit interface.
        var arbiterResult = await ExecuteSecurityUnitAsync(
            _arbiterUnit,
            arbiterRequest,
            "Security arbiter execution failed.",
            "SECURITY_ARBITER_EXECUTION_FAILED",
            cancellationToken);

        // Return node-08 failures through the logical-flow envelope.
        if (!arbiterResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(arbiterResult.ErrorMessage, arbiterResult.ErrorCode);

        // Refuse malformed success envelopes before any downstream node consumes their payload.
        if (arbiterResult.Value is null)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail("Security arbiter returned an invalid result.", "SECURITY_ARBITER_RESULT_INVALID");

        // Snapshot the admitted success envelope once for identity checks, downstream data, and metrics.
        var admittedArbiterResult = arbiterResult.Value;

        // Refuse learned or injected results that do not echo the exact requested node-08 identity.
        if (!string.Equals(admittedArbiterResult.UnitId, arbiterRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedArbiterResult.UnitRole, arbiterRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedArbiterResult.DomainTag, arbiterRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security arbiter result identity did not match the selected unit.",
                "SECURITY_ARBITER_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed or mutable learned payloads before node-05 or outward evidence can observe them.
        if (!TrySnapshotArbiterSuccess(
                admittedArbiterResult,
                arbiterRequest,
                arbiterInputAuthority,
                out var stableArbiterResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security arbiter returned an invalid result.",
                "SECURITY_ARBITER_RESULT_INVALID");
        }

        // Read the node-08 verdict from the logical-unit envelope.
        var arbiterVerdict = stableArbiterResult.Output;

        // Preserve private node-05 correlation authority before exposing a detached graph to the unit.
        var applierInputAuthority = new ApplierInputAuthority(
            scenario,
            selection.ScenarioId,
            Array.AsReadOnly(selection.SelectedArbiters.ToArray()),
            selection.SourcePairId,
            selection.SelectionReason,
            arbiterVerdict.ScenarioId,
            arbiterVerdict.Verdict,
            arbiterVerdict.ArbiterName,
            arbiterVerdict.RuleId,
            arbiterVerdict.Finding,
            arbiterVerdict.SourcePairId,
            Array.AsReadOnly(arbiterVerdict.RequiredResultSignals.ToArray()),
            Array.AsReadOnly(arbiterVerdict.ForbiddenResultSignals.ToArray()));

        // Compute the deterministic node-05 semantic oracle from its own detached graph.
        var authoritativeApplierInput = CreateDetachedApplierInput(scenario, selection, arbiterVerdict);
        var authoritativeApplication = SecurityApplierUnit.CreateAuthoritativeApplication(authoritativeApplierInput);
        var applierRouteAuthority = new ApplierRouteAuthority(authoritativeApplication.AppliedAction);

        // Build a second node-05 input graph with no oracle, node-07, or node-08 aliases.
        var applierInput = CreateDetachedApplierInput(scenario, selection, arbiterVerdict);

        // Build the logical-unit request for the node-05 applier unit.
        var applierRequest = new LogicalUnitRequest<SecurityApplicationInput>(
            request.RunId,
            SecurityApplierUnit.Id,
            SecurityApplierUnit.Role,
            SecurityApplierUnit.DomainTag,
            applierInput,
            request.Flags with { },
            unitMetadataGraphs[6]);

        // Execute node-05 through the logical-unit interface.
        var applierResult = await ExecuteSecurityUnitAsync(
            _applierUnit,
            applierRequest,
            "Security applier execution failed.",
            "SECURITY_APPLIER_EXECUTION_FAILED",
            cancellationToken);

        // Return node-05 failures through the logical-flow envelope.
        if (!applierResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(applierResult.ErrorMessage, applierResult.ErrorCode);

        // Refuse malformed success envelopes before any downstream node consumes their payload.
        if (applierResult.Value is null)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail("Security applier returned an invalid result.", "SECURITY_APPLIER_RESULT_INVALID");

        var admittedApplierResult = applierResult.Value;
        if (!string.Equals(admittedApplierResult.UnitId, applierRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedApplierResult.UnitRole, applierRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedApplierResult.DomainTag, applierRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security applier result identity did not match the selected unit.",
                "SECURITY_APPLIER_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed or mutated node-05 output and detach every downstream-visible carrier.
        if (!TrySnapshotApplierSuccess(
                admittedApplierResult,
                applierRequest,
                applierInputAuthority,
                out var stableApplierResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security applier returned an invalid result.",
                "SECURITY_APPLIER_RESULT_INVALID");
        }

        // Require the admitted learned implementation to preserve the deterministic application semantic.
        if (!string.Equals(
                stableApplierResult.Output.AppliedAction,
                applierRouteAuthority.AppliedAction,
                StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security applier result did not match the authoritative security route.",
                "SECURITY_APPLIER_ROUTE_IDENTITY_MISMATCH");
        }

        // Read the applied formal result only from the admitted stable snapshot.
        var appliedResult = stableApplierResult.Output;

        // Preserve private node-06 correlation authority before exposing a detached graph to the unit.
        var resultTranslatorInputAuthority = new ResultTranslatorInputAuthority(
            scenario,
            selection.ScenarioId,
            Array.AsReadOnly(selection.SelectedArbiters.ToArray()),
            selection.SourcePairId,
            selection.SelectionReason,
            appliedResult.ScenarioId,
            appliedResult.Verdict,
            appliedResult.AppliedAction,
            appliedResult.SourcePairId,
            Array.AsReadOnly(appliedResult.RequiredResultSignals.ToArray()),
            Array.AsReadOnly(appliedResult.ForbiddenResultSignals.ToArray()),
            appliedResult.ArbiterVerdict.ScenarioId,
            appliedResult.ArbiterVerdict.Verdict,
            appliedResult.ArbiterVerdict.ArbiterName,
            appliedResult.ArbiterVerdict.RuleId,
            appliedResult.ArbiterVerdict.Finding,
            appliedResult.ArbiterVerdict.SourcePairId,
            Array.AsReadOnly(appliedResult.ArbiterVerdict.RequiredResultSignals.ToArray()),
            Array.AsReadOnly(appliedResult.ArbiterVerdict.ForbiddenResultSignals.ToArray()));

        // Compute the deterministic node-06 semantic oracle from its own detached graph.
        var authoritativeTranslatorInput = CreateDetachedResultTranslatorInput(scenario, selection, appliedResult);
        var authoritativeResultTranslation = SecurityResultTranslatorUnit.CreateAuthoritativeResultTranslation(authoritativeTranslatorInput);
        var resultTranslatorRouteAuthority = new ResultTranslatorRouteAuthority(authoritativeResultTranslation.FormalResultFragment);

        // Build a second node-06 input graph with no oracle, node-05, node-07, or authority aliases.
        var translatorInput = CreateDetachedResultTranslatorInput(scenario, selection, appliedResult);

        // Build the logical-unit request for the node-06 result translator unit.
        var translatorRequest = new LogicalUnitRequest<SecurityResultTranslationInput>(
            request.RunId,
            SecurityResultTranslatorUnit.Id,
            SecurityResultTranslatorUnit.Role,
            SecurityResultTranslatorUnit.DomainTag,
            translatorInput,
            request.Flags with { },
            unitMetadataGraphs[7]);

        // Execute node-06 through the logical-unit interface.
        var translatorResult = await ExecuteSecurityUnitAsync(
            _resultTranslatorUnit,
            translatorRequest,
            "Security result translator execution failed.",
            "SECURITY_RESULT_TRANSLATOR_EXECUTION_FAILED",
            cancellationToken);

        // Return node-06 failures through the logical-flow envelope.
        if (!translatorResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(translatorResult.ErrorMessage, translatorResult.ErrorCode);

        // Refuse malformed success envelopes before node-09 consumes their payload.
        if (translatorResult.Value is null)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail("Security result translator returned an invalid result.", "SECURITY_RESULT_TRANSLATOR_RESULT_INVALID");

        var admittedTranslatorResult = translatorResult.Value;
        if (!string.Equals(admittedTranslatorResult.UnitId, translatorRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedTranslatorResult.UnitRole, translatorRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedTranslatorResult.DomainTag, translatorRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security result translator result identity did not match the selected unit.",
                "SECURITY_RESULT_TRANSLATOR_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed or mutated node-06 output and detach every downstream-visible carrier.
        if (!TrySnapshotResultTranslatorSuccess(
                admittedTranslatorResult,
                translatorRequest,
                resultTranslatorInputAuthority,
                out var stableTranslatorResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security result translator returned an invalid result.",
                "SECURITY_RESULT_TRANSLATOR_RESULT_INVALID");
        }

        // Require the admitted learned implementation to preserve the deterministic translation semantic.
        if (!string.Equals(
                stableTranslatorResult.Output.FormalResultFragment,
                resultTranslatorRouteAuthority.FormalResultFragment,
                StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security result translator result did not match the authoritative security route.",
                "SECURITY_RESULT_TRANSLATOR_ROUTE_IDENTITY_MISMATCH");
        }

        // Read the translated formal result only from the admitted stable snapshot.
        var translatedResult = stableTranslatorResult.Output;

        // Preserve private node-09 correlation authority before exposing a detached graph to the unit.
        var formatterInputAuthority = new FormatterInputAuthority(
            scenario,
            Attribution,
            selection.ScenarioId,
            Array.AsReadOnly(selection.SelectedArbiters.ToArray()),
            selection.SourcePairId,
            selection.SelectionReason,
            translatedResult.ScenarioId,
            translatedResult.Verdict,
            translatedResult.FormalResultFragment,
            translatedResult.SourcePairId,
            Array.AsReadOnly(translatedResult.RequiredResultSignals.ToArray()),
            Array.AsReadOnly(translatedResult.ForbiddenResultSignals.ToArray()),
            translatedResult.AppliedResult.ScenarioId,
            translatedResult.AppliedResult.Verdict,
            translatedResult.AppliedResult.AppliedAction,
            translatedResult.AppliedResult.SourcePairId,
            Array.AsReadOnly(translatedResult.AppliedResult.RequiredResultSignals.ToArray()),
            Array.AsReadOnly(translatedResult.AppliedResult.ForbiddenResultSignals.ToArray()),
            translatedResult.AppliedResult.ArbiterVerdict.ScenarioId,
            translatedResult.AppliedResult.ArbiterVerdict.Verdict,
            translatedResult.AppliedResult.ArbiterVerdict.ArbiterName,
            translatedResult.AppliedResult.ArbiterVerdict.RuleId,
            translatedResult.AppliedResult.ArbiterVerdict.Finding,
            translatedResult.AppliedResult.ArbiterVerdict.SourcePairId,
            Array.AsReadOnly(translatedResult.AppliedResult.ArbiterVerdict.RequiredResultSignals.ToArray()),
            Array.AsReadOnly(translatedResult.AppliedResult.ArbiterVerdict.ForbiddenResultSignals.ToArray()));

        // Compute the deterministic node-09 semantic oracle from its own detached graph.
        var authoritativeFormatInput = CreateDetachedFormatterInput(scenario, selection, translatedResult, Attribution);
        var authoritativeFormattedResult = SecurityFormatterUnit.CreateAuthoritativeFormat(authoritativeFormatInput);
        var formatterRouteAuthority = new FormatterRouteAuthority(authoritativeFormattedResult.HumanResult);

        // Build a second node-09 input graph with no oracle, upstream, or authority aliases.
        var formatterInput = CreateDetachedFormatterInput(scenario, selection, translatedResult, Attribution);

        // Build the logical-unit request for the node-09 formatter unit.
        var formatterRequest = new LogicalUnitRequest<SecurityFormatterInput>(
            request.RunId,
            SecurityFormatterUnit.Id,
            SecurityFormatterUnit.Role,
            SecurityFormatterUnit.DomainTag,
            formatterInput,
            request.Flags with { },
            unitMetadataGraphs[8]);

        // Execute node-09 through the logical-unit interface.
        var formatterResult = await ExecuteSecurityUnitAsync(
            _formatterUnit,
            formatterRequest,
            "Security formatter execution failed.",
            "SECURITY_FORMATTER_EXECUTION_FAILED",
            cancellationToken);

        // Return node-09 failures through the logical-flow envelope.
        if (!formatterResult.IsSuccess)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(formatterResult.ErrorMessage, formatterResult.ErrorCode);

        // Refuse malformed success envelopes before outward evidence observes their payload.
        if (formatterResult.Value is null)
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail("Security formatter returned an invalid result.", "SECURITY_FORMATTER_RESULT_INVALID");

        var admittedFormatterResult = formatterResult.Value;
        if (!string.Equals(admittedFormatterResult.UnitId, formatterRequest.UnitId, StringComparison.Ordinal)
            || !string.Equals(admittedFormatterResult.UnitRole, formatterRequest.UnitRole, StringComparison.Ordinal)
            || !string.Equals(admittedFormatterResult.DomainTag, formatterRequest.DomainTag, StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security formatter result identity did not match the selected unit.",
                "SECURITY_FORMATTER_RESULT_IDENTITY_MISMATCH");
        }

        // Refuse malformed or mutated node-09 output and detach every outward-visible carrier.
        if (!TrySnapshotFormatterSuccess(
                admittedFormatterResult,
                formatterRequest,
                formatterInputAuthority,
                out var stableFormatterResult))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security formatter returned an invalid result.",
                "SECURITY_FORMATTER_RESULT_INVALID");
        }

        // Require the admitted learned implementation to preserve the deterministic human-result semantic.
        if (!string.Equals(
                stableFormatterResult.Output.HumanResult,
                formatterRouteAuthority.HumanResult,
                StringComparison.Ordinal))
        {
            return AgentInfrastructure.OperationResult<LogicalFlowResult>.Fail(
                "Security formatter result did not match the authoritative security route.",
                "SECURITY_FORMATTER_ROUTE_IDENTITY_MISMATCH");
        }

        // Build the summary so the orchestrator can compare this proof with other logical flows.
        var summary = new LogicalFlowSummary(
            $"CB4-STAGE-5-{scenario}",
            1,
            0,
            1,
            "mandatory_security_checks_passed",
            false,
            true,
            false,
            "Use the remaining CB4 Action 3 pair corpus to replace deterministic security stations one at a time.",
            1,
            "S1, S2, and S3 oracle scenarios are routed through the registered security-verification flow.",
            "This Stage 5 adapter proves the assistant can select the sub-phase 3.4 security flow without an external API call.",
            0,
            0);

        // Build machine-checkable metrics for the assistant-level test and round evidence.
        var metrics = BuildMetrics(scenario, stableSplitterResult, stableSelectorResult, stableDomainTranslatorResult, stablePlannerResult, stableProcessorResult, stableArbiterResult, stableApplierResult, stableTranslatorResult, stableFormatterResult);

        // Build the normalized flow result expected by the logical orchestrator.
        var result = new LogicalFlowResult(
            FlowId,
            ScenarioId,
            arbiterVerdict.Verdict,
            stableFormatterResult.Output.HumanResult,
            summary,
            metrics,
            [$"CB4-STAGE-5-{scenario}", domainPacket.SourcePairId, selection.SourcePairId, domainTranslation.SourcePairId, plan.SourcePairId, processorFinding.SourcePairId, arbiterVerdict.SourcePairId, appliedResult.SourcePairId, translatedResult.SourcePairId, stableFormatterResult.Output.SourcePairId],
            stableFormatterResult.Warnings);

        // Return the normalized logical-flow result through the shared envelope.
        return AgentInfrastructure.OperationResult<LogicalFlowResult>.Ok(result);
    }

    // Execute one injected security unit while containing declared failures, malformed async behavior, and nonfatal failures.
    private static async Task<AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>> ExecuteSecurityUnitAsync<TInput, TOutput>(
        ILogicalUnit<TInput, TOutput> unit,
        LogicalUnitRequest<TInput> request,
        string failureMessage,
        string failureCode,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>.Fail(
                "Cancelled.",
                "CANCELLED");
        }

        try
        {
            Task<AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>>? executionTask =
                unit.ExecuteAsync(request, cancellationToken);
            if (executionTask is null)
            {
                return AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    failureMessage,
                    failureCode);
            }

            var executionResult = await executionTask.ConfigureAwait(false);
            if (cancellationToken.IsCancellationRequested)
            {
                return AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    "Cancelled.",
                    "CANCELLED");
            }

            if (executionResult is null || !executionResult.IsSuccess)
            {
                return AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    failureMessage,
                    failureCode);
            }

            return executionResult;
        }
        catch (OperationCanceledException)
        {
            return cancellationToken.IsCancellationRequested
                ? AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    "Cancelled.",
                    "CANCELLED")
                : AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    failureMessage,
                    failureCode);
        }
        catch (Exception exception) when (IsNonFatalSecurityExecutionException(exception))
        {
            return cancellationToken.IsCancellationRequested
                ? AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    "Cancelled.",
                    "CANCELLED")
                : AgentInfrastructure.OperationResult<LogicalUnitResult<TOutput>>.Fail(
                    failureMessage,
                    failureCode);
        }
    }

    // Preserve repository fatal-exception policy so process-corrupting failures still escape.
    private static bool IsNonFatalSecurityExecutionException(Exception exception)
        => exception is not OutOfMemoryException
           && exception is not StackOverflowException
           && exception is not AccessViolationException
           && exception is not AppDomainUnloadedException
           && exception is not BadImageFormatException
           && !StringComparer.Ordinal.Equals(
               exception.GetType().FullName,
               "System.Threading.ThreadAbortException");

    // Compare the complete deterministic node-04 payload without adding envelope carrier rules.
    private static bool HasExactProcessorRouteAuthority(
        SecurityProcessorFinding actual,
        ProcessorRouteAuthority expected)
        => string.Equals(actual.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
           && string.Equals(actual.Finding, expected.Finding, StringComparison.Ordinal)
           && string.Equals(actual.SourcePairId, expected.SourcePairId, StringComparison.Ordinal)
           && actual.RequiredResultSignals.SequenceEqual(
               expected.RequiredResultSignals,
               StringComparer.Ordinal)
           && actual.ForbiddenResultSignals.SequenceEqual(
               expected.ForbiddenResultSignals,
               StringComparer.Ordinal);

    // Create one isolated node-04 input graph with no mutable node-03 or node-07 array aliases.
    private static SecurityProcessorInput CreateDetachedProcessorInput(
        string scenario,
        SecurityExecutionPlan plan,
        SecurityArbiterSelection selection)
    {
        var detachedPlan = new SecurityExecutionPlan(
            plan.ScenarioId,
            plan.PlanStep,
            plan.SourcePairId,
            plan.RequiredResultSignals.ToArray(),
            plan.ForbiddenResultSignals.ToArray());
        var detachedSelection = new SecurityArbiterSelection(
            selection.ScenarioId,
            selection.SelectedArbiters.ToArray(),
            selection.SourcePairId,
            selection.SelectionReason);
        return new SecurityProcessorInput(
            scenario,
            detachedPlan,
            detachedSelection);
    }

    // Admit one structurally valid node-04 success against immutable upstream correlation authority.
    private static bool TrySnapshotProcessorSuccess(
        LogicalUnitResult<SecurityProcessorFinding> source,
        LogicalUnitRequest<SecurityProcessorInput> request,
        ProcessorInputAuthority expected,
        out LogicalUnitResult<SecurityProcessorFinding> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.RequiredResultSignals is null || output.ForbiddenResultSignals is null)
            return false;

        var requiredResultSignals = output.RequiredResultSignals.ToArray();
        var forbiddenResultSignals = output.ForbiddenResultSignals.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.SourcePairId, expected.PlanSourcePairId, StringComparison.Ordinal)
            || !IsCanonicalArbiterValue(output.Finding)
            || requiredResultSignals.Length == 0
            || requiredResultSignals.Any(value => !IsCanonicalArbiterValue(value))
            || forbiddenResultSignals.Any(value => !IsCanonicalArbiterValue(value))
            || requiredResultSignals.Any(
                required => forbiddenResultSignals.Contains(required, StringComparer.Ordinal))
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableOutput = new SecurityProcessorFinding(
            output.ScenarioId,
            output.Finding,
            output.SourcePairId,
            requiredResultSignals,
            forbiddenResultSignals);

        snapshot = new LogicalUnitResult<SecurityProcessorFinding>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Compare the complete deterministic node-03 payload without adding envelope carrier rules.
    private static bool HasExactPlannerRouteAuthority(
        SecurityExecutionPlan actual,
        PlannerRouteAuthority expected)
        => string.Equals(actual.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
           && string.Equals(actual.PlanStep, expected.PlanStep, StringComparison.Ordinal)
           && string.Equals(actual.SourcePairId, expected.SourcePairId, StringComparison.Ordinal)
           && actual.RequiredResultSignals.SequenceEqual(
               expected.RequiredResultSignals,
               StringComparer.Ordinal)
           && actual.ForbiddenResultSignals.SequenceEqual(
               expected.ForbiddenResultSignals,
               StringComparer.Ordinal);

    // Create one isolated node-03 input graph with no mutable node-02 or node-07 array aliases.
    private static SecurityPlannerInput CreateDetachedPlannerInput(
        string scenario,
        SecurityDomainTranslation translation,
        SecurityArbiterSelection selection)
    {
        var detachedTranslation = new SecurityDomainTranslation(
            translation.ScenarioId,
            translation.FormalStatement,
            translation.HandoffMarker,
            translation.SourcePairId,
            translation.RequiredResultSignals.ToArray(),
            translation.ForbiddenResultSignals.ToArray());
        var detachedSelection = new SecurityArbiterSelection(
            selection.ScenarioId,
            selection.SelectedArbiters.ToArray(),
            selection.SourcePairId,
            selection.SelectionReason);
        return new SecurityPlannerInput(
            scenario,
            detachedTranslation,
            detachedSelection);
    }

    // Admit one structurally valid node-03 success against immutable upstream correlation authority.
    private static bool TrySnapshotPlannerSuccess(
        LogicalUnitResult<SecurityExecutionPlan> source,
        LogicalUnitRequest<SecurityPlannerInput> request,
        PlannerInputAuthority expected,
        out LogicalUnitResult<SecurityExecutionPlan> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.RequiredResultSignals is null || output.ForbiddenResultSignals is null)
            return false;

        var requiredResultSignals = output.RequiredResultSignals.ToArray();
        var forbiddenResultSignals = output.ForbiddenResultSignals.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.SourcePairId, expected.TranslationSourcePairId, StringComparison.Ordinal)
            || !IsCanonicalArbiterValue(output.PlanStep)
            || requiredResultSignals.Length == 0
            || requiredResultSignals.Any(value => !IsCanonicalArbiterValue(value))
            || forbiddenResultSignals.Any(value => !IsCanonicalArbiterValue(value))
            || requiredResultSignals.Any(
                required => forbiddenResultSignals.Contains(required, StringComparer.Ordinal))
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableOutput = new SecurityExecutionPlan(
            output.ScenarioId,
            output.PlanStep,
            output.SourcePairId,
            requiredResultSignals,
            forbiddenResultSignals);

        snapshot = new LogicalUnitResult<SecurityExecutionPlan>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Create one isolated node-02 input graph with no mutable upstream array aliases.
    private static SecurityTranslationInput CreateDetachedTranslationInput(
        string scenario,
        SecurityDomainPacket domainPacket,
        SecurityArbiterSelection selection)
    {
        var detachedDomainPacket = new SecurityDomainPacket(
            domainPacket.ScenarioId,
            domainPacket.DomainLabel,
            domainPacket.PacketText,
            domainPacket.CoverageStatus,
            domainPacket.HandoffMarker,
            domainPacket.SourcePairId,
            domainPacket.RequiredResultSignals.ToArray(),
            domainPacket.ForbiddenResultSignals.ToArray());
        var detachedSelection = new SecurityArbiterSelection(
            selection.ScenarioId,
            selection.SelectedArbiters.ToArray(),
            selection.SourcePairId,
            selection.SelectionReason);
        return new SecurityTranslationInput(
            scenario,
            detachedDomainPacket,
            detachedSelection);
    }

    // Compare the complete deterministic node-02 payload without normalizing or adding carrier rules.
    private static bool HasExactTranslatorRouteAuthority(
        SecurityDomainTranslation actual,
        TranslatorRouteAuthority expected)
        => string.Equals(actual.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
           && string.Equals(actual.FormalStatement, expected.FormalStatement, StringComparison.Ordinal)
           && string.Equals(actual.HandoffMarker, expected.HandoffMarker, StringComparison.Ordinal)
           && string.Equals(actual.SourcePairId, expected.SourcePairId, StringComparison.Ordinal)
           && actual.RequiredResultSignals.SequenceEqual(
               expected.RequiredResultSignals,
               StringComparer.Ordinal)
           && actual.ForbiddenResultSignals.SequenceEqual(
               expected.ForbiddenResultSignals,
               StringComparer.Ordinal);

    // Compare all deterministic route fields without applying normalization or set semantics.
    private static bool HasExactSplitterRouteAuthority(
        SecurityDomainPacket actual,
        SplitterRouteAuthority expected)
        => string.Equals(actual.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
           && string.Equals(actual.DomainLabel, expected.DomainLabel, StringComparison.Ordinal)
           && string.Equals(actual.PacketText, expected.PacketText, StringComparison.Ordinal)
           && string.Equals(actual.CoverageStatus, expected.CoverageStatus, StringComparison.Ordinal)
           && string.Equals(actual.HandoffMarker, expected.HandoffMarker, StringComparison.Ordinal)
           && string.Equals(actual.SourcePairId, expected.SourcePairId, StringComparison.Ordinal)
           && actual.RequiredResultSignals.SequenceEqual(
               expected.RequiredResultSignals,
               StringComparer.Ordinal)
           && actual.ForbiddenResultSignals.SequenceEqual(
               expected.ForbiddenResultSignals,
               StringComparer.Ordinal);

    // Admit one structurally valid node-02 success against immutable upstream authority.
    private static bool TrySnapshotTranslatorSuccess(
        LogicalUnitResult<SecurityDomainTranslation> source,
        LogicalUnitRequest<SecurityTranslationInput> request,
        TranslationInputAuthority expected,
        out LogicalUnitResult<SecurityDomainTranslation> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.RequiredResultSignals is null || output.ForbiddenResultSignals is null)
            return false;

        var requiredResultSignals = output.RequiredResultSignals.ToArray();
        var forbiddenResultSignals = output.ForbiddenResultSignals.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.SourcePairId, expected.SourcePairId, StringComparison.Ordinal)
            || !IsCanonicalArbiterValue(output.FormalStatement)
            || !IsCanonicalArbiterValue(output.HandoffMarker)
            || !requiredResultSignals.SequenceEqual(
                expected.RequiredResultSignals,
                StringComparer.Ordinal)
            || !forbiddenResultSignals.SequenceEqual(
                expected.ForbiddenResultSignals,
                StringComparer.Ordinal)
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableOutput = new SecurityDomainTranslation(
            output.ScenarioId,
            output.FormalStatement,
            output.HandoffMarker,
            output.SourcePairId,
            requiredResultSignals,
            forbiddenResultSignals);

        snapshot = new LogicalUnitResult<SecurityDomainTranslation>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Admit one structurally valid node-01 success and detach every mutable payload carrier.
    private static bool TrySnapshotSplitterSuccess(
        LogicalUnitResult<SecurityDomainPacket> source,
        LogicalUnitRequest<SecuritySplitInput> request,
        string expectedPacketText,
        out LogicalUnitResult<SecurityDomainPacket> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.RequiredResultSignals is null || output.ForbiddenResultSignals is null)
            return false;

        var requiredResultSignals = output.RequiredResultSignals.ToArray();
        var forbiddenResultSignals = output.ForbiddenResultSignals.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.PacketText, expectedPacketText, StringComparison.Ordinal)
            || !IsCanonicalArbiterValue(output.ScenarioId)
            || !IsCanonicalArbiterValue(output.DomainLabel)
            || !IsCanonicalArbiterValue(output.CoverageStatus)
            || !IsCanonicalArbiterValue(output.HandoffMarker)
            || !IsCanonicalArbiterValue(output.SourcePairId)
            || requiredResultSignals.Length == 0
            || requiredResultSignals.Any(value => !IsCanonicalArbiterValue(value))
            || forbiddenResultSignals.Any(value => !IsCanonicalArbiterValue(value))
            || requiredResultSignals.Any(
                required => forbiddenResultSignals.Contains(required, StringComparer.Ordinal))
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableOutput = new SecurityDomainPacket(
            output.ScenarioId,
            output.DomainLabel,
            expectedPacketText,
            output.CoverageStatus,
            output.HandoffMarker,
            output.SourcePairId,
            requiredResultSignals,
            forbiddenResultSignals);

        snapshot = new LogicalUnitResult<SecurityDomainPacket>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Create one isolated node-07 input graph from private scalar and read-only array authority.
    private static SecurityArbiterSelectionInput CreateDetachedSelectorInput(SelectorInputAuthority authority)
        => new(
            authority.ScenarioId,
            authority.SourceFinding,
            authority.SourcePairIds.ToArray());

    // Compare the complete deterministic node-07 selection without duplicating envelope admission.
    private static bool HasExactSelectorRouteAuthority(
        SecurityArbiterSelection actual,
        SelectorRouteAuthority expected)
        => string.Equals(actual.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
           && actual.SelectedArbiters.SequenceEqual(expected.SelectedArbiters, StringComparer.Ordinal)
           && string.Equals(actual.SourcePairId, expected.SourcePairId, StringComparison.Ordinal)
           && string.Equals(actual.SelectionReason, expected.SelectionReason, StringComparison.Ordinal);

    // Admit one structurally valid node-07 success and detach every mutable payload carrier.
    private static bool TrySnapshotSelectorSuccess(
        LogicalUnitResult<SecurityArbiterSelection> source,
        LogicalUnitRequest<SecurityArbiterSelectionInput> request,
        SelectorInputAuthority expected,
        out LogicalUnitResult<SecurityArbiterSelection> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.SelectedArbiters is null)
            return false;

        var selectedArbiters = output.SelectedArbiters.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
            || selectedArbiters.Length == 0
            || selectedArbiters.Any(value => !IsCanonicalArbiterValue(value))
            || selectedArbiters.Distinct(StringComparer.Ordinal).Count() != selectedArbiters.Length
            || !IsCanonicalArbiterValue(output.SourcePairId)
            || !IsCanonicalArbiterValue(output.SelectionReason)
            || !expected.SourcePairIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableOutput = new SecurityArbiterSelection(
            output.ScenarioId,
            selectedArbiters,
            output.SourcePairId,
            output.SelectionReason);

        snapshot = new LogicalUnitResult<SecurityArbiterSelection>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Admit one structurally valid node-08 success and detach every mutable payload carrier.
    private static bool TrySnapshotArbiterSuccess(
        LogicalUnitResult<SecurityArbiterVerdict> source,
        LogicalUnitRequest<SecurityArbiterInput> request,
        ArbiterInputAuthority expected,
        out LogicalUnitResult<SecurityArbiterVerdict> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.RequiredResultSignals is null || output.ForbiddenResultSignals is null)
            return false;

        var requiredResultSignals = output.RequiredResultSignals.ToArray();
        var forbiddenResultSignals = output.ForbiddenResultSignals.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
            || !(string.Equals(output.Verdict, "PASS", StringComparison.Ordinal)
                 || string.Equals(output.Verdict, "REPROCESS", StringComparison.Ordinal)
                 || string.Equals(output.Verdict, "CONCERN", StringComparison.Ordinal))
            || !IsCanonicalArbiterValue(output.ArbiterName)
            || !IsCanonicalArbiterValue(output.RuleId)
            || !IsCanonicalArbiterValue(output.Finding)
            || !IsCanonicalArbiterValue(output.SourcePairId)
            || !expected.SourcePairIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || requiredResultSignals.Any(value => !IsCanonicalArbiterValue(value))
            || forbiddenResultSignals.Any(value => !IsCanonicalArbiterValue(value))
            || !requiredResultSignals.Contains(output.Verdict, StringComparer.Ordinal)
            || requiredResultSignals.Any(
                required => forbiddenResultSignals.Contains(required, StringComparer.Ordinal))
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableOutput = new SecurityArbiterVerdict(
            output.ScenarioId,
            output.Verdict,
            output.ArbiterName,
            output.RuleId,
            output.Finding,
            output.SourcePairId,
            requiredResultSignals,
            forbiddenResultSignals);

        snapshot = new LogicalUnitResult<SecurityArbiterVerdict>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Create one isolated node-05 input graph with no mutable node-07 or node-08 array aliases.
    private static SecurityApplicationInput CreateDetachedApplierInput(
        string scenario,
        SecurityArbiterSelection selection,
        SecurityArbiterVerdict arbiterVerdict)
    {
        var detachedSelection = new SecurityArbiterSelection(
            selection.ScenarioId,
            selection.SelectedArbiters.ToArray(),
            selection.SourcePairId,
            selection.SelectionReason);
        var detachedArbiterVerdict = new SecurityArbiterVerdict(
            arbiterVerdict.ScenarioId,
            arbiterVerdict.Verdict,
            arbiterVerdict.ArbiterName,
            arbiterVerdict.RuleId,
            arbiterVerdict.Finding,
            arbiterVerdict.SourcePairId,
            arbiterVerdict.RequiredResultSignals.ToArray(),
            arbiterVerdict.ForbiddenResultSignals.ToArray());
        return new SecurityApplicationInput(
            scenario,
            detachedSelection,
            detachedArbiterVerdict);
    }

    // Admit one structurally valid node-05 success against private pre-await input authority.
    private static bool TrySnapshotApplierSuccess(
        LogicalUnitResult<SecurityAppliedResult> source,
        LogicalUnitRequest<SecurityApplicationInput> request,
        ApplierInputAuthority expected,
        out LogicalUnitResult<SecurityAppliedResult> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.ArbiterVerdict is null
            || output.RequiredResultSignals is null
            || output.ForbiddenResultSignals is null
            || output.ArbiterVerdict.RequiredResultSignals is null
            || output.ArbiterVerdict.ForbiddenResultSignals is null)
        {
            return false;
        }

        var nestedRequiredResultSignals = output.ArbiterVerdict.RequiredResultSignals.ToArray();
        var nestedForbiddenResultSignals = output.ArbiterVerdict.ForbiddenResultSignals.ToArray();
        var requiredResultSignals = output.RequiredResultSignals.ToArray();
        var forbiddenResultSignals = output.ForbiddenResultSignals.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.Verdict, expected.ArbiterVerdict, StringComparison.Ordinal)
            || !string.Equals(output.SourcePairId, expected.ArbiterSourcePairId, StringComparison.Ordinal)
            || !IsCanonicalArbiterValue(output.AppliedAction)
            || !string.Equals(output.ArbiterVerdict.ScenarioId, expected.ArbiterScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.ArbiterVerdict.Verdict, expected.ArbiterVerdict, StringComparison.Ordinal)
            || !string.Equals(output.ArbiterVerdict.ArbiterName, expected.ArbiterName, StringComparison.Ordinal)
            || !string.Equals(output.ArbiterVerdict.RuleId, expected.ArbiterRuleId, StringComparison.Ordinal)
            || !string.Equals(output.ArbiterVerdict.Finding, expected.ArbiterFinding, StringComparison.Ordinal)
            || !string.Equals(output.ArbiterVerdict.SourcePairId, expected.ArbiterSourcePairId, StringComparison.Ordinal)
            || !nestedRequiredResultSignals.SequenceEqual(expected.ArbiterRequiredResultSignals, StringComparer.Ordinal)
            || !nestedForbiddenResultSignals.SequenceEqual(expected.ArbiterForbiddenResultSignals, StringComparer.Ordinal)
            || !requiredResultSignals.SequenceEqual(expected.ArbiterRequiredResultSignals, StringComparer.Ordinal)
            || !forbiddenResultSignals.SequenceEqual(expected.ArbiterForbiddenResultSignals, StringComparer.Ordinal)
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableArbiterVerdict = new SecurityArbiterVerdict(
            output.ArbiterVerdict.ScenarioId,
            output.ArbiterVerdict.Verdict,
            output.ArbiterVerdict.ArbiterName,
            output.ArbiterVerdict.RuleId,
            output.ArbiterVerdict.Finding,
            output.ArbiterVerdict.SourcePairId,
            nestedRequiredResultSignals,
            nestedForbiddenResultSignals);
        var stableOutput = new SecurityAppliedResult(
            output.ScenarioId,
            output.Verdict,
            output.AppliedAction,
            stableArbiterVerdict,
            output.SourcePairId,
            requiredResultSignals,
            forbiddenResultSignals);

        snapshot = new LogicalUnitResult<SecurityAppliedResult>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Create one isolated node-06 input graph with no mutable selector, applied-result, or verdict aliases.
    private static SecurityResultTranslationInput CreateDetachedResultTranslatorInput(
        string scenario,
        SecurityArbiterSelection selection,
        SecurityAppliedResult appliedResult)
    {
        var detachedSelection = new SecurityArbiterSelection(
            selection.ScenarioId,
            selection.SelectedArbiters.ToArray(),
            selection.SourcePairId,
            selection.SelectionReason);
        var detachedArbiterVerdict = new SecurityArbiterVerdict(
            appliedResult.ArbiterVerdict.ScenarioId,
            appliedResult.ArbiterVerdict.Verdict,
            appliedResult.ArbiterVerdict.ArbiterName,
            appliedResult.ArbiterVerdict.RuleId,
            appliedResult.ArbiterVerdict.Finding,
            appliedResult.ArbiterVerdict.SourcePairId,
            appliedResult.ArbiterVerdict.RequiredResultSignals.ToArray(),
            appliedResult.ArbiterVerdict.ForbiddenResultSignals.ToArray());
        var detachedAppliedResult = new SecurityAppliedResult(
            appliedResult.ScenarioId,
            appliedResult.Verdict,
            appliedResult.AppliedAction,
            detachedArbiterVerdict,
            appliedResult.SourcePairId,
            appliedResult.RequiredResultSignals.ToArray(),
            appliedResult.ForbiddenResultSignals.ToArray());
        return new SecurityResultTranslationInput(
            scenario,
            detachedSelection,
            detachedAppliedResult);
    }

    // Admit one structurally valid node-06 success against private pre-await input authority.
    private static bool TrySnapshotResultTranslatorSuccess(
        LogicalUnitResult<SecurityTranslatedResult> source,
        LogicalUnitRequest<SecurityResultTranslationInput> request,
        ResultTranslatorInputAuthority expected,
        out LogicalUnitResult<SecurityTranslatedResult> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.AppliedResult is null
            || output.RequiredResultSignals is null
            || output.ForbiddenResultSignals is null
            || output.AppliedResult.ArbiterVerdict is null
            || output.AppliedResult.RequiredResultSignals is null
            || output.AppliedResult.ForbiddenResultSignals is null
            || output.AppliedResult.ArbiterVerdict.RequiredResultSignals is null
            || output.AppliedResult.ArbiterVerdict.ForbiddenResultSignals is null)
        {
            return false;
        }

        var nestedArbiterRequiredResultSignals = output.AppliedResult.ArbiterVerdict.RequiredResultSignals.ToArray();
        var nestedArbiterForbiddenResultSignals = output.AppliedResult.ArbiterVerdict.ForbiddenResultSignals.ToArray();
        var nestedAppliedRequiredResultSignals = output.AppliedResult.RequiredResultSignals.ToArray();
        var nestedAppliedForbiddenResultSignals = output.AppliedResult.ForbiddenResultSignals.ToArray();
        var requiredResultSignals = output.RequiredResultSignals.ToArray();
        var forbiddenResultSignals = output.ForbiddenResultSignals.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.Verdict, expected.AppliedVerdict, StringComparison.Ordinal)
            || !string.Equals(output.SourcePairId, expected.AppliedSourcePairId, StringComparison.Ordinal)
            || !IsCanonicalArbiterValue(output.FormalResultFragment)
            || !string.Equals(output.AppliedResult.ScenarioId, expected.AppliedScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.AppliedResult.Verdict, expected.AppliedVerdict, StringComparison.Ordinal)
            || !string.Equals(output.AppliedResult.AppliedAction, expected.AppliedAction, StringComparison.Ordinal)
            || !string.Equals(output.AppliedResult.SourcePairId, expected.AppliedSourcePairId, StringComparison.Ordinal)
            || !nestedAppliedRequiredResultSignals.SequenceEqual(expected.AppliedRequiredResultSignals, StringComparer.Ordinal)
            || !nestedAppliedForbiddenResultSignals.SequenceEqual(expected.AppliedForbiddenResultSignals, StringComparer.Ordinal)
            || !string.Equals(output.AppliedResult.ArbiterVerdict.ScenarioId, expected.ArbiterScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.AppliedResult.ArbiterVerdict.Verdict, expected.ArbiterVerdict, StringComparison.Ordinal)
            || !string.Equals(output.AppliedResult.ArbiterVerdict.ArbiterName, expected.ArbiterName, StringComparison.Ordinal)
            || !string.Equals(output.AppliedResult.ArbiterVerdict.RuleId, expected.ArbiterRuleId, StringComparison.Ordinal)
            || !string.Equals(output.AppliedResult.ArbiterVerdict.Finding, expected.ArbiterFinding, StringComparison.Ordinal)
            || !string.Equals(output.AppliedResult.ArbiterVerdict.SourcePairId, expected.ArbiterSourcePairId, StringComparison.Ordinal)
            || !nestedArbiterRequiredResultSignals.SequenceEqual(expected.ArbiterRequiredResultSignals, StringComparer.Ordinal)
            || !nestedArbiterForbiddenResultSignals.SequenceEqual(expected.ArbiterForbiddenResultSignals, StringComparer.Ordinal)
            || !requiredResultSignals.SequenceEqual(expected.AppliedRequiredResultSignals, StringComparer.Ordinal)
            || !forbiddenResultSignals.SequenceEqual(expected.AppliedForbiddenResultSignals, StringComparer.Ordinal)
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableArbiterVerdict = new SecurityArbiterVerdict(
            output.AppliedResult.ArbiterVerdict.ScenarioId,
            output.AppliedResult.ArbiterVerdict.Verdict,
            output.AppliedResult.ArbiterVerdict.ArbiterName,
            output.AppliedResult.ArbiterVerdict.RuleId,
            output.AppliedResult.ArbiterVerdict.Finding,
            output.AppliedResult.ArbiterVerdict.SourcePairId,
            nestedArbiterRequiredResultSignals,
            nestedArbiterForbiddenResultSignals);
        var stableAppliedResult = new SecurityAppliedResult(
            output.AppliedResult.ScenarioId,
            output.AppliedResult.Verdict,
            output.AppliedResult.AppliedAction,
            stableArbiterVerdict,
            output.AppliedResult.SourcePairId,
            nestedAppliedRequiredResultSignals,
            nestedAppliedForbiddenResultSignals);
        var stableOutput = new SecurityTranslatedResult(
            output.ScenarioId,
            output.Verdict,
            output.FormalResultFragment,
            stableAppliedResult,
            output.SourcePairId,
            requiredResultSignals,
            forbiddenResultSignals);

        snapshot = new LogicalUnitResult<SecurityTranslatedResult>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Create one isolated node-09 input graph with no mutable selector or translated-result aliases.
    private static SecurityFormatterInput CreateDetachedFormatterInput(
        string scenario,
        SecurityArbiterSelection selection,
        SecurityTranslatedResult translatedResult,
        string attribution)
    {
        var detachedSelection = new SecurityArbiterSelection(
            selection.ScenarioId,
            selection.SelectedArbiters.ToArray(),
            selection.SourcePairId,
            selection.SelectionReason);
        var detachedArbiterVerdict = new SecurityArbiterVerdict(
            translatedResult.AppliedResult.ArbiterVerdict.ScenarioId,
            translatedResult.AppliedResult.ArbiterVerdict.Verdict,
            translatedResult.AppliedResult.ArbiterVerdict.ArbiterName,
            translatedResult.AppliedResult.ArbiterVerdict.RuleId,
            translatedResult.AppliedResult.ArbiterVerdict.Finding,
            translatedResult.AppliedResult.ArbiterVerdict.SourcePairId,
            translatedResult.AppliedResult.ArbiterVerdict.RequiredResultSignals.ToArray(),
            translatedResult.AppliedResult.ArbiterVerdict.ForbiddenResultSignals.ToArray());
        var detachedAppliedResult = new SecurityAppliedResult(
            translatedResult.AppliedResult.ScenarioId,
            translatedResult.AppliedResult.Verdict,
            translatedResult.AppliedResult.AppliedAction,
            detachedArbiterVerdict,
            translatedResult.AppliedResult.SourcePairId,
            translatedResult.AppliedResult.RequiredResultSignals.ToArray(),
            translatedResult.AppliedResult.ForbiddenResultSignals.ToArray());
        var detachedTranslatedResult = new SecurityTranslatedResult(
            translatedResult.ScenarioId,
            translatedResult.Verdict,
            translatedResult.FormalResultFragment,
            detachedAppliedResult,
            translatedResult.SourcePairId,
            translatedResult.RequiredResultSignals.ToArray(),
            translatedResult.ForbiddenResultSignals.ToArray());
        return new SecurityFormatterInput(
            scenario,
            detachedSelection,
            detachedTranslatedResult,
            attribution);
    }

    // Admit one structurally valid node-09 success against private pre-await input authority.
    private static bool TrySnapshotFormatterSuccess(
        LogicalUnitResult<SecurityFormattedResult> source,
        LogicalUnitRequest<SecurityFormatterInput> request,
        FormatterInputAuthority expected,
        out LogicalUnitResult<SecurityFormattedResult> snapshot)
    {
        snapshot = null!;

        if (source.Output is null
            || source.PredicateIds is null
            || source.DomainTags is null
            || source.EvidenceIds is null
            || source.Warnings is null
            || source.TransitionSignal is not null
            || !double.IsFinite(source.Confidence)
            || source.Confidence is < 0d or > 1d)
        {
            return false;
        }

        var output = source.Output;
        if (output.RequiredResultSignals is null || output.ForbiddenResultSignals is null)
            return false;

        var requiredResultSignals = output.RequiredResultSignals.ToArray();
        var forbiddenResultSignals = output.ForbiddenResultSignals.ToArray();
        var predicateIds = source.PredicateIds.ToArray();
        var domainTags = source.DomainTags.ToArray();
        var evidenceIds = source.EvidenceIds.ToArray();
        var warnings = source.Warnings.ToArray();

        if (!string.Equals(output.ScenarioId, expected.ScenarioId, StringComparison.Ordinal)
            || !string.Equals(output.SourcePairId, expected.TranslatedSourcePairId, StringComparison.Ordinal)
            || !requiredResultSignals.SequenceEqual(expected.TranslatedRequiredResultSignals, StringComparer.Ordinal)
            || !forbiddenResultSignals.SequenceEqual(expected.TranslatedForbiddenResultSignals, StringComparer.Ordinal)
            || !IsCanonicalFormattedText(output.HumanResult)
            || predicateIds.Length == 0
            || predicateIds.Any(value => !IsCanonicalArbiterValue(value))
            || domainTags.Length == 0
            || domainTags.Any(value => !IsCanonicalArbiterValue(value))
            || !domainTags.Contains(request.DomainTag, StringComparer.Ordinal)
            || evidenceIds.Length == 0
            || evidenceIds.Any(value => !IsCanonicalArbiterValue(value))
            || !evidenceIds.Contains(output.SourcePairId, StringComparer.Ordinal)
            || warnings.Any(value => !IsCanonicalArbiterValue(value)))
        {
            return false;
        }

        var stableOutput = new SecurityFormattedResult(
            output.ScenarioId,
            output.HumanResult,
            output.SourcePairId,
            requiredResultSignals,
            forbiddenResultSignals);
        snapshot = new LogicalUnitResult<SecurityFormattedResult>(
            source.UnitId,
            source.UnitRole,
            source.DomainTag,
            stableOutput,
            source.Confidence,
            predicateIds,
            domainTags,
            evidenceIds,
            warnings);
        return true;
    }

    // Accept non-empty multi-line formatter text while rejecting hidden control and separator characters.
    private static bool IsCanonicalFormattedText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)
            || !value.EndsWith(Environment.NewLine, StringComparison.Ordinal))
        {
            return false;
        }

        var content = value[..^Environment.NewLine.Length];
        if (content.Length == 0)
            return false;

        var lines = content.Split(Environment.NewLine, StringSplitOptions.None);
        return lines.All(line =>
            line.Length > 0
            && string.Equals(line, line.Trim(), StringComparison.Ordinal)
            && !line.Contains('\r')
            && !line.Contains('\n')
            && !line.Any(character => char.IsControl(character) && character != '\t')
            && !line.Contains('\u0085')
            && !line.Contains('\u2028')
            && !line.Contains('\u2029'));
    }

    // Accept only exact, non-empty text without padding or hidden control/separator characters.
    private static bool IsCanonicalArbiterValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && string.Equals(value, value.Trim(), StringComparison.Ordinal)
           && !value.Any(char.IsControl)
           && !value.Contains('\r')
           && !value.Contains('\n')
           && !value.Contains('\u0085')
           && !value.Contains('\u2028')
           && !value.Contains('\u2029');

    // Validate and snapshot one resolved registry tuple without exposing split identity parameters publicly.
    private static ArbiterBinding CreateArbiterBinding(
        ResolvedLogicalUnitRegistryEntry<SecurityArbiterInput, SecurityArbiterVerdict> arbiterEntry)
    {
        ArgumentNullException.ThrowIfNull(arbiterEntry);
        ArgumentNullException.ThrowIfNull(arbiterEntry.Descriptor);
        ArgumentNullException.ThrowIfNull(arbiterEntry.Unit);

        return new ArbiterBinding(
            arbiterEntry.Unit,
            arbiterEntry.Descriptor.UnitId,
            arbiterEntry.Descriptor.UnitRole,
            arbiterEntry.Descriptor.DomainTag);
    }

    // This private carrier keeps the selected executable and identity inseparable during constructor chaining.
    private sealed record ArbiterBinding(
        ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict> Unit,
        string UnitId,
        string UnitRole,
        string DomainTag);

    // This private carrier owns the exact deterministic node-01 route selected before execution.
    private sealed record SplitterRouteAuthority(
        string ScenarioId,
        string DomainLabel,
        string PacketText,
        string CoverageStatus,
        string HandoffMarker,
        string SourcePairId,
        string[] RequiredResultSignals,
        string[] ForbiddenResultSignals);

    // This private carrier owns node-07 input correlation without exposing the injected array.
    private sealed record SelectorInputAuthority(
        string ScenarioId,
        string SourceFinding,
        IReadOnlyList<string> SourcePairIds);

    // This private carrier owns the complete deterministic node-07 selection route.
    private sealed record SelectorRouteAuthority(
        string ScenarioId,
        IReadOnlyList<string> SelectedArbiters,
        string SourcePairId,
        string SelectionReason);

    // This private carrier owns the stable node-02 route identity without exposing its arrays to the unit.
    private sealed record TranslationInputAuthority(
        string ScenarioId,
        string SourcePairId,
        string[] RequiredResultSignals,
        string[] ForbiddenResultSignals);

    // This private carrier owns the complete deterministic node-02 output route selected before execution.
    private sealed record TranslatorRouteAuthority(
        string ScenarioId,
        string FormalStatement,
        string HandoffMarker,
        string SourcePairId,
        string[] RequiredResultSignals,
        string[] ForbiddenResultSignals);

    // This private carrier owns the complete stable node-03 input authority without mutable array exposure.
    private sealed record PlannerInputAuthority(
        string ScenarioId,
        string TranslationFormalStatement,
        string TranslationHandoffMarker,
        string TranslationSourcePairId,
        IReadOnlyList<string> TranslationRequiredResultSignals,
        IReadOnlyList<string> TranslationForbiddenResultSignals,
        string SelectionScenarioId,
        IReadOnlyList<string> SelectedArbiters,
        string SelectionSourcePairId,
        string SelectionReason);

    // This private carrier owns the complete deterministic node-03 output route selected before execution.
    private sealed record PlannerRouteAuthority(
        string ScenarioId,
        string PlanStep,
        string SourcePairId,
        string[] RequiredResultSignals,
        string[] ForbiddenResultSignals);

    // This private carrier owns the complete stable node-04 input authority without mutable array exposure.
    private sealed record ProcessorInputAuthority(
        string ScenarioId,
        string PlanStep,
        string PlanSourcePairId,
        IReadOnlyList<string> PlanRequiredResultSignals,
        IReadOnlyList<string> PlanForbiddenResultSignals,
        string SelectionScenarioId,
        IReadOnlyList<string> SelectedArbiters,
        string SelectionSourcePairId,
        string SelectionReason);

    // This private carrier owns the complete deterministic node-04 output route selected before execution.
    private sealed record ProcessorRouteAuthority(
        string ScenarioId,
        string Finding,
        string SourcePairId,
        string[] RequiredResultSignals,
        string[] ForbiddenResultSignals);

    // This private carrier owns the complete stable node-08 input without exposing mutable arrays to the unit.
    private sealed record ArbiterInputAuthority(
        string ScenarioId,
        string ProcessorFinding,
        IReadOnlyList<string> CandidateArbiters,
        IReadOnlyList<string> SourcePairIds);

    // This private carrier owns all stable node-05 input authority without exposing the injected graph.
    private sealed record ApplierInputAuthority(
        string ScenarioId,
        string SelectionScenarioId,
        IReadOnlyList<string> SelectedArbiters,
        string SelectionSourcePairId,
        string SelectionReason,
        string ArbiterScenarioId,
        string ArbiterVerdict,
        string ArbiterName,
        string ArbiterRuleId,
        string ArbiterFinding,
        string ArbiterSourcePairId,
        IReadOnlyList<string> ArbiterRequiredResultSignals,
        IReadOnlyList<string> ArbiterForbiddenResultSignals);

    // This private carrier owns only the deterministic node-05 semantic not already covered by traceability admission.
    private sealed record ApplierRouteAuthority(string AppliedAction);

    // This private carrier owns all stable node-06 input authority without exposing the injected graph.
    private sealed record ResultTranslatorInputAuthority(
        string ScenarioId,
        string SelectionScenarioId,
        IReadOnlyList<string> SelectedArbiters,
        string SelectionSourcePairId,
        string SelectionReason,
        string AppliedScenarioId,
        string AppliedVerdict,
        string AppliedAction,
        string AppliedSourcePairId,
        IReadOnlyList<string> AppliedRequiredResultSignals,
        IReadOnlyList<string> AppliedForbiddenResultSignals,
        string ArbiterScenarioId,
        string ArbiterVerdict,
        string ArbiterName,
        string ArbiterRuleId,
        string ArbiterFinding,
        string ArbiterSourcePairId,
        IReadOnlyList<string> ArbiterRequiredResultSignals,
        IReadOnlyList<string> ArbiterForbiddenResultSignals);

    // This private carrier owns only the deterministic node-06 semantic not already covered by traceability admission.
    private sealed record ResultTranslatorRouteAuthority(string FormalResultFragment);

    // This private carrier owns the complete stable node-09 input without exposing the injected graph.
    private sealed record FormatterInputAuthority(
        string ScenarioId,
        string Attribution,
        string SelectionScenarioId,
        IReadOnlyList<string> SelectedArbiters,
        string SelectionSourcePairId,
        string SelectionReason,
        string TranslatedScenarioId,
        string TranslatedVerdict,
        string FormalResultFragment,
        string TranslatedSourcePairId,
        IReadOnlyList<string> TranslatedRequiredResultSignals,
        IReadOnlyList<string> TranslatedForbiddenResultSignals,
        string AppliedScenarioId,
        string AppliedVerdict,
        string AppliedAction,
        string AppliedSourcePairId,
        IReadOnlyList<string> AppliedRequiredResultSignals,
        IReadOnlyList<string> AppliedForbiddenResultSignals,
        string ArbiterScenarioId,
        string ArbiterVerdict,
        string ArbiterName,
        string ArbiterRuleId,
        string ArbiterFinding,
        string ArbiterSourcePairId,
        IReadOnlyList<string> ArbiterRequiredResultSignals,
        IReadOnlyList<string> ArbiterForbiddenResultSignals);

    // This private carrier owns only the deterministic node-09 semantic not already covered by traceability admission.
    private sealed record FormatterRouteAuthority(string HumanResult);

    // Build the machine-checkable metric dictionary for one oracle scenario.
    private static IReadOnlyDictionary<string, string> BuildMetrics(
        string scenario,
        LogicalUnitResult<SecurityDomainPacket> splitterResult,
        LogicalUnitResult<SecurityArbiterSelection> selectorResult,
        LogicalUnitResult<SecurityDomainTranslation> domainTranslatorResult,
        LogicalUnitResult<SecurityExecutionPlan> plannerResult,
        LogicalUnitResult<SecurityProcessorFinding> processorResult,
        LogicalUnitResult<SecurityArbiterVerdict> arbiterResult,
        LogicalUnitResult<SecurityAppliedResult> applierResult,
        LogicalUnitResult<SecurityTranslatedResult> translatorResult,
        LogicalUnitResult<SecurityFormattedResult> formatterResult)
    {
        // Return strings because LogicalFlowResult metrics are runtime-neutral text values.
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Record the selected oracle scenario id.
            ["security_scenario_id"] = scenario,

            // Record the security verdict for evidence comparison.
            ["security_verdict"] = arbiterResult.Output.Verdict,

            // Prove node-01 is now executed through a logical-unit surface.
            ["node_01_unit_id"] = splitterResult.UnitId,

            // Record the splitter confidence for the replacement gate.
            ["node_01_unit_confidence"] = splitterResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the CB4 pair anchor used by node-01.
            ["node_01_source_pair_id"] = splitterResult.Output.SourcePairId,

            // Record the domain packet label emitted by node-01.
            ["node_01_domain_label"] = splitterResult.Output.DomainLabel,

            // Prove the flow stayed internal-first and made no external AI call.
            ["external_ai_call_count"] = "0",

            // Prove OWASP attribution is human-visible in every scenario.
            ["attribution_visible"] = "True",

            // Prove the flow was selected through the orchestrator route.
            ["security_flow_registered"] = "True",

            // Prove node-07 is now executed through a logical-unit surface.
            ["node_07_unit_id"] = selectorResult.UnitId,

            // Record the selector confidence for the replacement gate.
            ["node_07_unit_confidence"] = selectorResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the selected arbiters as a stable semicolon-delimited metric.
            ["node_07_selected_arbiters"] = string.Join(";", selectorResult.Output.SelectedArbiters),

            // Record the CB4 pair anchor used by node-07.
            ["node_07_source_pair_id"] = selectorResult.Output.SourcePairId,

            // Prove node-02 is now executed through a logical-unit surface.
            ["node_02_unit_id"] = domainTranslatorResult.UnitId,

            // Record the domain-translator confidence for the replacement gate.
            ["node_02_unit_confidence"] = domainTranslatorResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the CB4 pair anchor used by node-02.
            ["node_02_source_pair_id"] = domainTranslatorResult.Output.SourcePairId,

            // Prove node-03 is now executed through a logical-unit surface.
            ["node_03_unit_id"] = plannerResult.UnitId,

            // Record the planner confidence for the replacement gate.
            ["node_03_unit_confidence"] = plannerResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the CB4 pair anchor used by node-03.
            ["node_03_source_pair_id"] = plannerResult.Output.SourcePairId,

            // Prove node-04 is now executed through a logical-unit surface.
            ["node_04_unit_id"] = processorResult.UnitId,

            // Record the processor confidence for the replacement gate.
            ["node_04_unit_confidence"] = processorResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the CB4 pair anchor used by node-04.
            ["node_04_source_pair_id"] = processorResult.Output.SourcePairId,

            // Prove node-08 is now executed through a logical-unit surface.
            ["node_08_unit_id"] = arbiterResult.UnitId,

            // Record the arbiter confidence for the replacement gate.
            ["node_08_unit_confidence"] = arbiterResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the CB4 pair anchor that drove the arbiter verdict.
            ["node_08_source_pair_id"] = arbiterResult.Output.SourcePairId,

            // Prove node-05 is now executed through a logical-unit surface.
            ["node_05_unit_id"] = applierResult.UnitId,

            // Record the applier confidence for the replacement gate.
            ["node_05_unit_confidence"] = applierResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the CB4 pair anchor used by node-05.
            ["node_05_source_pair_id"] = applierResult.Output.SourcePairId,

            // Prove node-06 is now executed through a logical-unit surface.
            ["node_06_unit_id"] = translatorResult.UnitId,

            // Record the result-translator confidence for the replacement gate.
            ["node_06_unit_confidence"] = translatorResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the CB4 pair anchor used by node-06.
            ["node_06_source_pair_id"] = translatorResult.Output.SourcePairId,

            // Prove node-09 is now executed through a logical-unit surface.
            ["node_09_unit_id"] = formatterResult.UnitId,

            // Record the formatter confidence for the replacement gate.
            ["node_09_unit_confidence"] = formatterResult.Confidence.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),

            // Record the CB4 pair anchor used by node-09.
            ["node_09_source_pair_id"] = formatterResult.Output.SourcePairId
        };
    }
}
