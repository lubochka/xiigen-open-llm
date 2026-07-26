// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class SystemBoundaryMapperUnit
{
    public const string UnitId = "system-boundary-mapper-unit";
    public const string ScenarioId = "project-wiki-system-boundary-mapping";

    public OperationResult<SystemBoundaryMapResult> Map(string systemId, IReadOnlyList<SystemBoundaryProjectEvidence> projects)
    {
        if (string.IsNullOrWhiteSpace(systemId))
            return OperationResult<SystemBoundaryMapResult>.Fail("System id is required.", "SYSTEM_ID_REQUIRED");
        if (projects.Count == 0)
            return OperationResult<SystemBoundaryMapResult>.Fail("At least one project evidence item is required.", "PROJECT_EVIDENCE_REQUIRED");

        var edges = new List<SystemBoundaryEdge>();
        var unknowns = new List<string>();
        foreach (var source in projects)
        {
            foreach (var target in projects)
            {
                if (source.ProjectId.Equals(target.ProjectId, StringComparison.OrdinalIgnoreCase))
                    continue;
                var sharedMarkers = source.RouteOrCallMarkers
                    .Intersect(target.RouteOrCallMarkers, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
                if (sharedMarkers.Length == 0)
                    continue;
                edges.Add(new SystemBoundaryEdge(
                    $"{source.ProjectId}-to-{target.ProjectId}",
                    source.ProjectId,
                    target.ProjectId,
                    ResolveBoundaryKind(source.ProjectRole, target.ProjectRole),
                    sharedMarkers,
                    source.EvidenceIds.Concat(target.EvidenceIds).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                    sharedMarkers.Length >= 2 ? 0.92 : 0.76,
                    sharedMarkers.Length >= 2 ? "validated_by_shared_markers" : "weak_single_marker"));
            }
        }

        foreach (var project in projects)
        {
            if (!edges.Any(edge => edge.SourceProjectId.Equals(project.ProjectId, StringComparison.OrdinalIgnoreCase) ||
                                   edge.TargetProjectId.Equals(project.ProjectId, StringComparison.OrdinalIgnoreCase)))
                unknowns.Add($"{project.ProjectId}: unknown_not_enough_evidence");
        }

        var warnings = unknowns.Count == 0 ? Array.Empty<string>() : ["some_project_boundaries_unknown"];
        var confidence = edges.Count > 0 && unknowns.Count == 0 ? 0.93 : edges.Count > 0 ? 0.78 : 0.42;
        return OperationResult<SystemBoundaryMapResult>.Ok(new SystemBoundaryMapResult(
            systemId,
            projects.Select(project => project.ProjectId).ToArray(),
            edges.ToArray(),
            unknowns.ToArray(),
            unknowns.Select(unknown => $"Add route, caller, deployment, or database evidence for {unknown}.").ToArray(),
            confidence,
            warnings));
    }

    private static string ResolveBoundaryKind(string sourceRole, string targetRole)
    {
        if (sourceRole.Contains("client", StringComparison.OrdinalIgnoreCase) &&
            targetRole.Contains("server", StringComparison.OrdinalIgnoreCase))
            return "client_to_server_api";
        if (sourceRole.Contains("server", StringComparison.OrdinalIgnoreCase) &&
            targetRole.Contains("sql", StringComparison.OrdinalIgnoreCase))
            return "server_to_database";
        if (sourceRole.Contains("database", StringComparison.OrdinalIgnoreCase) ||
            targetRole.Contains("database", StringComparison.OrdinalIgnoreCase))
            return "database_boundary";
        return "cross_repository_dependency";
    }
}
