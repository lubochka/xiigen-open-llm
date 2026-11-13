// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This factory constructs the security verification flow with learned units when trusted replacements exist.
public sealed class SecurityVerification34FlowFactory : ILogicalFlowFactory<string>
{
    // Create a security verification flow for one execution.
    public AgentInfrastructure.OperationResult<ILogicalFlow<string>> Create(LogicalFlowFactoryRequest request)
    {
        // Reject missing factory requests because unit selection depends on flags and registries.
        if (request is null)
            return AgentInfrastructure.OperationResult<ILogicalFlow<string>>.Fail("Flow factory request is required.", "FLOW_FACTORY_REQUEST_REQUIRED");

        // Deterministic fallback remains the product-safe default when learned replacement is disabled.
        if (!request.Flags.UseLearnedUnitWhenAvailable)
            return AgentInfrastructure.OperationResult<ILogicalFlow<string>>.Ok(new SecurityVerification34FlowAdapter());

        // Select the newest trusted-enough learned arbiter as one atomic descriptor/executable tuple.
        var learnedEntry = request.Units.ResolveLatestLearnedEntry<SecurityArbiterInput, SecurityArbiterVerdict>(
            SecurityArbiterUnit.Role,
            SecurityArbiterUnit.DomainTag,
            request.MinimumTrustState);

        // Any missing, invalid, or ambiguous learned selection keeps the deterministic product-safe fallback.
        if (!learnedEntry.IsSuccess)
            return AgentInfrastructure.OperationResult<ILogicalFlow<string>>.Ok(new SecurityVerification34FlowAdapter());

        // Preserve the learned semantic verdict while rebinding only runtime-owned correlation fields.
        var correlatedEntry = new ResolvedLogicalUnitRegistryEntry<SecurityArbiterInput, SecurityArbiterVerdict>(
            learnedEntry.Value!.Descriptor,
            new SecurityArbiterRuntimeCorrelationUnit(learnedEntry.Value.Unit));

        // Preserve the selected descriptor and executable together through node-08 execution.
        return AgentInfrastructure.OperationResult<ILogicalFlow<string>>.Ok(
            SecurityVerification34FlowAdapter.CreateWithArbiterEntry(correlatedEntry));
    }
}

// Learned arbiters decide semantic verdict fields; this adapter owns only request-local correlation identity.
internal sealed class SecurityArbiterRuntimeCorrelationUnit(
    ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict> inner)
    : ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict>
{
    public async Task<AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>> ExecuteAsync(
        LogicalUnitRequest<SecurityArbiterInput> request,
        CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Fail(
                "Cancelled.",
                "CANCELLED");
        }

        var result = await inner.ExecuteAsync(request, cancellationToken);
        if (cancellationToken.IsCancellationRequested)
        {
            return AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Fail(
                "Cancelled.",
                "CANCELLED");
        }

        if (!result.IsSuccess
            || result.Value is null
            || result.Value.Output is null
            || request.Input is null
            || request.Input.SourcePairIds is not { Length: > 0 }
            || request.Input.SourcePairIds.Any(string.IsNullOrWhiteSpace)
            || result.Value.Output.RequiredResultSignals is null
            || result.Value.Output.ForbiddenResultSignals is null
            || result.Value.PredicateIds is null
            || result.Value.DomainTags is null
            || result.Value.EvidenceIds is null
            || result.Value.Warnings is null)
        {
            return result;
        }

        var source = result.Value;
        var learnedOutput = source.Output;
        var runtimeSourcePairId = request.Input.SourcePairIds.Contains(
            learnedOutput.SourcePairId,
            StringComparer.Ordinal)
                ? learnedOutput.SourcePairId
                : request.Input.SourcePairIds[0];
        var output = new SecurityArbiterVerdict(
            request.Input.ScenarioId,
            learnedOutput.Verdict,
            learnedOutput.ArbiterName,
            learnedOutput.RuleId,
            learnedOutput.Finding,
            runtimeSourcePairId,
            learnedOutput.RequiredResultSignals.ToArray(),
            learnedOutput.ForbiddenResultSignals.ToArray());
        var evidenceIds = source.EvidenceIds
            .Concat([runtimeSourcePairId])
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return AgentInfrastructure.OperationResult<LogicalUnitResult<SecurityArbiterVerdict>>.Ok(
            new LogicalUnitResult<SecurityArbiterVerdict>(
                source.UnitId,
                source.UnitRole,
                source.DomainTag,
                output,
                source.Confidence,
                source.PredicateIds.ToArray(),
                source.DomainTags.ToArray(),
                evidenceIds,
                source.Warnings.ToArray())
            {
                TransitionSignal = source.TransitionSignal
            });
    }
}
