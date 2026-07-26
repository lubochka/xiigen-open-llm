// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class CrossStackBusinessFlowGraphUnit
{
    public const string UnitId = "cross-stack-business-flow-graph-unit";
    public const string ScenarioId = "project-wiki-cross-stack-business-flow-graph";

    public OperationResult<CrossStackBusinessFlowGraph> Build(string graphId, SystemBoundaryMapResult boundaryMap)
    {
        if (string.IsNullOrWhiteSpace(graphId))
            return OperationResult<CrossStackBusinessFlowGraph>.Fail("Graph id is required.", "GRAPH_ID_REQUIRED");
        if (boundaryMap.ProjectIds.Length == 0)
            return OperationResult<CrossStackBusinessFlowGraph>.Fail("Boundary map must contain projects.", "BOUNDARY_MAP_EMPTY");

        var nodes = boundaryMap.ProjectIds
            .Select(projectId => new CrossStackBusinessFlowNode(
                $"{graphId}:{projectId}",
                projectId,
                ResolveNodeKind(projectId),
                ResolveStackLabel(projectId),
                boundaryMap.Edges
                    .Where(edge => edge.SourceProjectId.Equals(projectId, StringComparison.OrdinalIgnoreCase) ||
                                   edge.TargetProjectId.Equals(projectId, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(edge => edge.EvidenceIds)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray()))
            .ToArray();

        var edges = boundaryMap.Edges
            .Select(edge => new CrossStackBusinessFlowEdge(
                edge.EdgeId,
                $"{graphId}:{edge.SourceProjectId}",
                $"{graphId}:{edge.TargetProjectId}",
                ResolveEdgeKind(edge.BoundaryKind),
                edge.EvidenceIds,
                edge.Confidence,
                edge.Status))
            .ToArray();

        var clientToStoredProcedurePaths = BuildClientToStoredProcedurePaths(nodes, edges);
        var unknownPaths = boundaryMap.UnknownBoundaries
            .Concat(edges.Where(edge => edge.EdgeKind == CrossStackBusinessFlowEdgeKind.UnknownBoundary).Select(edge => edge.EdgeId))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var warnings = unknownPaths.Length == 0 ? Array.Empty<string>() : ["cross_stack_paths_need_more_evidence"];
        return OperationResult<CrossStackBusinessFlowGraph>.Ok(new CrossStackBusinessFlowGraph(
            graphId,
            boundaryMap.SystemId,
            nodes,
            edges,
            clientToStoredProcedurePaths,
            unknownPaths,
            CalculateRecall(edges, CrossStackBusinessFlowEdgeKind.ClientToServerApi),
            CalculateRecall(edges, CrossStackBusinessFlowEdgeKind.ServerToStoredProcedure),
            clientToStoredProcedurePaths.Length > 0 ? 1.0 : 0.0,
            warnings));
    }

    private static CrossStackBusinessFlowEdgeKind ResolveEdgeKind(string boundaryKind)
        => boundaryKind switch
        {
            "client_to_server_api" => CrossStackBusinessFlowEdgeKind.ClientToServerApi,
            "server_to_database" => CrossStackBusinessFlowEdgeKind.ServerToDatabase,
            "database_boundary" => CrossStackBusinessFlowEdgeKind.ServerToStoredProcedure,
            _ => CrossStackBusinessFlowEdgeKind.UnknownBoundary
        };

    private static string ResolveNodeKind(string projectId)
    {
        if (projectId.StartsWith("angular-", StringComparison.OrdinalIgnoreCase) ||
            projectId.EndsWith("-client", StringComparison.OrdinalIgnoreCase) ||
            projectId.Contains("client-ui", StringComparison.OrdinalIgnoreCase))
            return "client";
        if (projectId.StartsWith("sql-", StringComparison.OrdinalIgnoreCase) ||
            projectId.EndsWith("-database", StringComparison.OrdinalIgnoreCase) ||
            projectId.Contains("stored-procedure", StringComparison.OrdinalIgnoreCase))
            return "database";
        return "server_or_mixed";
    }

    private static string ResolveStackLabel(string projectId)
    {
        if (projectId.Contains("bizguide", StringComparison.OrdinalIgnoreCase) ||
            projectId.Contains("game", StringComparison.OrdinalIgnoreCase))
            return "Angular";
        if (projectId.Contains("dynamicapp", StringComparison.OrdinalIgnoreCase))
            return "React";
        if (projectId.Contains("askapedia", StringComparison.OrdinalIgnoreCase))
            return "ASP.NET Framework + SQL Server";
        return ".NET";
    }

    private static string[] BuildClientToStoredProcedurePaths(CrossStackBusinessFlowNode[] nodes, CrossStackBusinessFlowEdge[] edges)
    {
        var clientNodes = nodes.Where(node => node.NodeKind == "client").ToArray();
        var sqlEdges = edges.Where(edge => edge.EdgeKind is CrossStackBusinessFlowEdgeKind.ServerToStoredProcedure or CrossStackBusinessFlowEdgeKind.ServerToDatabase).ToArray();
        return clientNodes
            .SelectMany(client => edges
                .Where(edge => edge.SourceNodeId == client.NodeId && edge.EdgeKind == CrossStackBusinessFlowEdgeKind.ClientToServerApi)
                .SelectMany(apiEdge => sqlEdges
                    .Where(sqlEdge => sqlEdge.SourceNodeId == apiEdge.TargetNodeId || sqlEdge.TargetNodeId == apiEdge.TargetNodeId)
                    .Select(sqlEdge => $"{client.NodeId}->{apiEdge.TargetNodeId}->{sqlEdge.TargetNodeId}")))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static double CalculateRecall(CrossStackBusinessFlowEdge[] edges, CrossStackBusinessFlowEdgeKind kind)
        => edges.Any(edge => edge.EdgeKind == kind && edge.Confidence >= 0.85) ? 1.0 : 0.0;
}
