// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// Resolves a learned, trusted-enough L2 edge authority by exact registry identity.
public sealed class RegistryDynamicGraphEdgeAuthorityResolver : IDynamicGraphEdgeAuthorityResolver
{
    private const string ExpectedRole = "edge-authority";
    private const string ExpectedLevel = "L2";
    private const LogicalUnitTrustState MinimumTrustState = LogicalUnitTrustState.Probationary;

    private readonly InMemoryLogicalUnitRegistry _registry;

    public RegistryDynamicGraphEdgeAuthorityResolver(InMemoryLogicalUnitRegistry registry)
    {
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
    }

    // Resolve validates edge and registry metadata but never executes the selected unit.
    public OperationResult<ResolvedDynamicGraphEdgeAuthority> Resolve(
        IDynamicGraphEdge edge,
        string domainTag)
    {
        if (edge is null)
            return Failure("Dynamic graph edge is required.", "EDGE_AUTHORITY_EDGE_REQUIRED");

        if (string.IsNullOrWhiteSpace(edge.AuthorityUnitId))
            return Failure("Edge authority unit id is required.", "EDGE_AUTHORITY_UNIT_ID_REQUIRED");

        if (!StringComparer.Ordinal.Equals(edge.AuthorityUnitId, edge.AuthorityUnitId.Trim()))
            return Failure("Edge authority unit id must be canonical.", "EDGE_AUTHORITY_UNIT_ID_NOT_CANONICAL");

        if (string.IsNullOrWhiteSpace(edge.AuthorityUnitLevel))
            return Failure("Edge authority unit level is required.", "EDGE_AUTHORITY_UNIT_LEVEL_REQUIRED");

        if (!StringComparer.Ordinal.Equals(edge.AuthorityUnitLevel, ExpectedLevel))
            return Failure("Edge authority unit level must be L2.", "EDGE_AUTHORITY_UNIT_LEVEL_MISMATCH");

        if (edge.DeterministicFinalAuthority)
            return Failure("Learned edge authority cannot claim deterministic final authority.", "EDGE_AUTHORITY_FINAL_AUTHORITY_FORBIDDEN");

        if (string.IsNullOrWhiteSpace(domainTag))
            return Failure("Edge authority domain tag is required.", "EDGE_AUTHORITY_DOMAIN_REQUIRED");

        if (!StringComparer.Ordinal.Equals(domainTag, domainTag.Trim()))
            return Failure("Edge authority domain tag must be canonical.", "EDGE_AUTHORITY_DOMAIN_NOT_CANONICAL");

        // Exact-id resolution selects the newest tuple before any role/domain/type/trust/learned validation.
        var resolved = _registry.ResolveLatestById<EdgeAuthorityInput, EdgeAuthorityDecision>(edge.AuthorityUnitId);
        if (!resolved.IsSuccess)
            return OperationResult<ResolvedDynamicGraphEdgeAuthority>.Fail(
                string.IsNullOrWhiteSpace(resolved.ErrorMessage)
                    ? "Edge authority registry resolution failed."
                    : resolved.ErrorMessage,
                string.IsNullOrWhiteSpace(resolved.ErrorCode) ? "EDGE_AUTHORITY_REGISTRY_RESOLUTION_FAILED" : resolved.ErrorCode);

        if (resolved.Value is null)
            return Failure("Edge authority registry returned no resolved entry.", "EDGE_AUTHORITY_REGISTRY_RESULT_REQUIRED");

        var descriptor = resolved.Value.Descriptor;
        if (!StringComparer.Ordinal.Equals(descriptor.UnitId, edge.AuthorityUnitId))
            return Failure("Resolved edge authority unit id does not match the edge.", "EDGE_AUTHORITY_DESCRIPTOR_UNIT_ID_MISMATCH");

        if (!StringComparer.Ordinal.Equals(descriptor.UnitRole, ExpectedRole))
            return Failure("Resolved edge authority role is invalid.", "EDGE_AUTHORITY_ROLE_MISMATCH");

        if (!StringComparer.Ordinal.Equals(descriptor.DomainTag, domainTag))
            return Failure("Resolved edge authority domain does not match the requested domain.", "EDGE_AUTHORITY_DOMAIN_MISMATCH");

        if (descriptor.InputType != typeof(EdgeAuthorityInput))
            return Failure("Resolved edge authority input type is invalid.", "EDGE_AUTHORITY_INPUT_TYPE_MISMATCH");

        if (descriptor.OutputType != typeof(EdgeAuthorityDecision))
            return Failure("Resolved edge authority output type is invalid.", "EDGE_AUTHORITY_OUTPUT_TYPE_MISMATCH");

        if (!descriptor.IsLearned)
            return Failure("Resolved edge authority unit is not learned.", "EDGE_AUTHORITY_UNIT_NOT_LEARNED");

        if (!Enum.IsDefined(descriptor.TrustState))
            return Failure("Resolved edge authority trust state is invalid.", "EDGE_AUTHORITY_TRUST_STATE_INVALID");

        if (descriptor.TrustState == LogicalUnitTrustState.Rejected)
            return Failure("Resolved edge authority unit is rejected.", "EDGE_AUTHORITY_UNIT_REJECTED");

        if (descriptor.TrustState < MinimumTrustState)
            return Failure("Resolved edge authority unit is below the minimum trust state.", "EDGE_AUTHORITY_TRUST_TOO_LOW");

        return OperationResult<ResolvedDynamicGraphEdgeAuthority>.Ok(
            new ResolvedDynamicGraphEdgeAuthority(descriptor, resolved.Value.Unit));
    }

    private static OperationResult<ResolvedDynamicGraphEdgeAuthority> Failure(string message, string code)
        => OperationResult<ResolvedDynamicGraphEdgeAuthority>.Fail(message, code);
}
