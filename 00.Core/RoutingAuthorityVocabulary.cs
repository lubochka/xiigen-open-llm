// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

// This is the single production owner of routing-authority role and publication-key literals.
public static class RoutingAuthorityVocabulary
{
    // Deterministic candidate generators may propose routes but never claim learned authority.
    public const string BaselineCandidateGeneratorRole = "baseline_candidate_generator";

    // Admitted learned edge units own trained edge-routing decisions.
    public const string TrainedEdgeUnitRole = "trained_edge_unit";

    // Public metadata and result metrics publish the authority role under this exact key.
    public const string MetadataKey = "routing_authority_role";

    // External boundaries accept only the closed routing-authority role vocabulary.
    public static bool IsCanonicalRole(string? authorityRole)
        => StringComparer.Ordinal.Equals(authorityRole, BaselineCandidateGeneratorRole)
           || StringComparer.Ordinal.Equals(authorityRole, TrainedEdgeUnitRole);
}
