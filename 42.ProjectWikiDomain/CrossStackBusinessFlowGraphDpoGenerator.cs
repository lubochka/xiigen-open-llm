// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class CrossStackBusinessFlowGraphDpoGenerator
{
    private readonly CrossStackBusinessFlowGraphUnit _unit;

    public CrossStackBusinessFlowGraphDpoGenerator(CrossStackBusinessFlowGraphUnit? unit = null)
    {
        _unit = unit ?? new CrossStackBusinessFlowGraphUnit();
    }

    public Task<OperationResult<CrossStackBusinessFlowGraphDpoBatch>> BuildBatchAsync(
        string batchId = "cross-stack-business-flow-graph-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var seeds = BuildSeeds(heldOut);
        var pairs = seeds.Select((seed, index) =>
        {
            var graph = _unit.Build(seed.GraphId, seed.BoundaryMap).Value!;
            return new CrossStackBusinessFlowGraphDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.BoundaryMap,
                BuildPrompt(seed),
                BuildChosen(graph),
                BuildRejected(seed.RejectedType, graph),
                seed.RejectedType,
                graph.ClientToStoredProcedurePaths,
                graph.Nodes.SelectMany(node => node.EvidenceIds).Concat(graph.Edges.SelectMany(edge => edge.EvidenceIds)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
        }).ToArray();

        var wrongAnnotationRate = pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphDpoBatch>.Ok(new CrossStackBusinessFlowGraphDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildPrompt(CrossStackBusinessFlowSeed seed)
        => $"Build the business flow graph for one system across Angular client, .NET server, and SQL stored procedures.\ngraph_id: {seed.GraphId}\nsystem_id: {seed.BoundaryMap.SystemId}\nprojects: {string.Join(", ", seed.BoundaryMap.ProjectIds)}";

    private static string BuildChosen(CrossStackBusinessFlowGraph graph)
        => $"PASS_TRUE CROSS_STACK_BUSINESS_FLOW_GRAPH_COMPLETE ANGULAR_DOTNET_SQL_ONE_SYSTEM CLIENT_TO_API_TO_STORED_PROCEDURE_PATHS_PRESENT STORED_PROCEDURE_RESULT_TO_UI_CONSUMER_COVERED\nsystem_id: {graph.SystemId}\ngraph_id: {graph.GraphId}\nclient_to_stored_procedure_paths: {string.Join("; ", graph.ClientToStoredProcedurePaths)}\nedges: {string.Join("; ", graph.Edges.Select(edge => $"{edge.EdgeId}:{edge.EdgeKind}:{string.Join("|", edge.EvidenceIds)}"))}\nunknown_paths: {(graph.UnknownPaths.Length == 0 ? "none" : string.Join(", ", graph.UnknownPaths))}\nclient_server_edge_recall: {graph.ClientServerEdgeRecall:0.######}\nstored_procedure_edge_recall: {graph.RepositoryToStoredProcedureEdgeRecall:0.######}\npath_recall: {graph.ClientToStoredProcedurePathRecall:0.######}\nFINAL_DECISION ACCEPTED_COMPLETE_GRAPH ACCEPTED_COMPLETE_GRAPH ACCEPTED_COMPLETE_GRAPH PASS_TRUE_COMPLETE";

    private static string BuildRejected(string rejectedType, CrossStackBusinessFlowGraph graph)
        => rejectedType switch
        {
            "ui_api_only" =>
                $"FAIL_FALSE CROSS_STACK_GRAPH_INCOMPLETE CLIENT_TO_STORED_PROCEDURE_PATHS_MISSING\nsystem_id: {graph.SystemId}\nclient_to_stored_procedure_paths: none\nedges: {string.Join("; ", graph.Edges.Where(edge => edge.EdgeKind == CrossStackBusinessFlowEdgeKind.ClientToServerApi).Select(edge => edge.EdgeId))}\n[WRONG: ui_api_only - It stops at Angular service to API and misses repository, stored procedure, SQL tables, result shape, and UI state consumers.]\nFINAL_DECISION REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH FAIL_FALSE_INCOMPLETE",
            "server_sql_only" =>
                $"FAIL_FALSE CROSS_STACK_GRAPH_INCOMPLETE ANGULAR_ENTRYPOINT_MISSING\nsystem_id: {graph.SystemId}\nclient_to_stored_procedure_paths: none\nedges: {string.Join("; ", graph.Edges.Where(edge => edge.EdgeKind != CrossStackBusinessFlowEdgeKind.ClientToServerApi).Select(edge => edge.EdgeId))}\n[WRONG: server_sql_only - It maps .NET to SQL but drops the Angular route, component, service call, and client state part of the same business flow.]\nFINAL_DECISION REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH FAIL_FALSE_INCOMPLETE",
            "procedure_without_result_shape" =>
                $"FAIL_FALSE CROSS_STACK_GRAPH_INCOMPLETE SQL_RESULT_SHAPE_MISSING\nsystem_id: {graph.SystemId}\nclient_to_stored_procedure_paths: {string.Join("; ", graph.ClientToStoredProcedurePaths)}\n[WRONG: procedure_without_result_shape - It names the stored procedure but omits table/result/DTO/UI consumer evidence needed to answer advanced wiki questions.]\nFINAL_DECISION REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH FAIL_FALSE_INCOMPLETE",
            "separate_repositories" =>
                $"FAIL_FALSE CROSS_STACK_GRAPH_INCOMPLETE ONE_SYSTEM_BOUNDARY_DENIED\nsystem_id: {graph.SystemId}\nclient_to_stored_procedure_paths: none\n[WRONG: separate_repositories - It treats Angular, .NET, and SQL as separate projects instead of one business system.]\nFINAL_DECISION REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH FAIL_FALSE_INCOMPLETE",
            _ =>
                $"FAIL_FALSE CROSS_STACK_GRAPH_INCOMPLETE\nsystem_id: {graph.SystemId}\nclient_to_stored_procedure_paths: incomplete\n[WRONG: incomplete_cross_stack_business_flow - It cannot prove the full Angular to .NET to stored procedure business path.]\nFINAL_DECISION REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH FAIL_FALSE_INCOMPLETE"
        };

    private static CrossStackBusinessFlowSeed[] BuildSeeds(bool heldOut)
    {
        var domains = heldOut
            ? new[] { "invoice", "refund", "shipment", "audit", "tenant", "license", "subscription", "cart", "checkout", "message" }
            : new[] { "campaign", "client", "diet", "media", "task", "game", "profile", "report", "notification", "booking" };
        var rows = new List<CrossStackBusinessFlowSeed>(50);
        for (var round = 0; round < 5; round++)
        {
            foreach (var domain in domains)
            {
                var marker = $"flow:{domain}:{round}";
                var api = $"api:/{domain}/{round}";
                var procedure = $"sp:{domain}_save_{round}";
                var result = $"result:{domain}_summary_{round}";
                var uiState = $"ui:{domain}_state_{round}";
                var projects = new[] { $"angular-{domain}-{round}", $"dotnet-{domain}-{round}", $"sql-{domain}-{round}" };
                var edges = new[]
                {
                    new SystemBoundaryEdge($"edge-client-api-{domain}-{round}", projects[0], projects[1], "client_to_server_api", [marker, api], [$"ev-{domain}-{round}-client", $"ev-{domain}-{round}-api"], 0.97, "known"),
                    new SystemBoundaryEdge($"edge-server-sql-{domain}-{round}", projects[1], projects[2], "database_boundary", [marker, procedure, result, uiState], [$"ev-{domain}-{round}-server", $"ev-{domain}-{round}-sql", $"ev-{domain}-{round}-result"], 0.96, "known")
                };
                var boundary = new SystemBoundaryMapResult(
                    $"system-{domain}-{round}",
                    projects,
                    edges,
                    [],
                    [$"confirm DTO mapping for {domain}", $"confirm UI state consumer {uiState}"],
                    0.94,
                    []);
                var rejected = round switch
                {
                    0 => "ui_api_only",
                    1 => "server_sql_only",
                    2 => "procedure_without_result_shape",
                    3 => "separate_repositories",
                    _ => "incomplete_cross_stack_business_flow"
                };
                rows.Add(new CrossStackBusinessFlowSeed($"graph-{domain}-{round}", boundary, rejected));
            }
        }

        return rows.Take(50).ToArray();
    }

    private sealed record CrossStackBusinessFlowSeed(
        string GraphId,
        SystemBoundaryMapResult BoundaryMap,
        string RejectedType);
}
