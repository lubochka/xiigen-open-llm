// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class CrossStackBusinessFlowGraphDpoGenerator
{
    internal static readonly string[] RejectedTypeCoverageProgram =
    [
        "ui_api_only",
        "server_sql_only",
        "procedure_without_result_shape",
        "separate_repositories",
        "incomplete_cross_stack_business_flow"
    ];

    private static readonly string[] TrainingDomains =
    [
        "campaign",
        "client",
        "diet",
        "media",
        "task",
        "game",
        "profile",
        "report",
        "notification",
        "booking"
    ];

    private static readonly string[] HeldOutDomains =
    [
        "invoice",
        "refund",
        "shipment",
        "audit",
        "tenant",
        "license",
        "subscription",
        "cart",
        "checkout",
        "message"
    ];

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
        var domains = heldOut ? HeldOutDomains : TrainingDomains;
        var splitRole = heldOut ? DpoBatchSplitRole.HeldOut : DpoBatchSplitRole.Training;
        return BuildBatchForDomainsAsync(batchId, domains, splitRole, ct);
    }

    private Task<OperationResult<CrossStackBusinessFlowGraphDpoBatch>> BuildBatchForDomainsAsync(
        string batchId,
        string[]? domains,
        DpoBatchSplitRole splitRole,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        // Public production calls still choose only the canonical arrays above.
        // This private seam only lets mutation tests cross the old fifty-row
        // boundary while reusing the production seed-to-pair builder.
        var domainProgram = ValidateDomainProgram(domains);
        if (!domainProgram.IsSuccess)
            return Task.FromResult(domainProgram.MapError<CrossStackBusinessFlowGraphDpoBatch>());

        var seeds = BuildSeeds(domainProgram.Value!);
        if (!seeds.IsSuccess)
            return Task.FromResult(seeds.MapError<CrossStackBusinessFlowGraphDpoBatch>());

        var pairs = seeds.Value!.Select((seed, index) =>
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

        var batch = new CrossStackBusinessFlowGraphDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())
        {
            IndependentlyEvaluated = false
        };

        // Только реальный генератор посева в этой сборке может поставить
        // internal provenance-proof; публичный SourceKind сам по себе ничего
        // не разрешает для публикации.
        return Task.FromResult(OperationResult<CrossStackBusinessFlowGraphDpoBatch>.Ok(
            CrossStackBusinessFlowGraphBatchProvenanceProof.AttachDeterministicSeed(batch, splitRole)));
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
            "incomplete_cross_stack_business_flow" =>
                $"FAIL_FALSE CROSS_STACK_GRAPH_INCOMPLETE\nsystem_id: {graph.SystemId}\nclient_to_stored_procedure_paths: incomplete\n[WRONG: incomplete_cross_stack_business_flow - It cannot prove the full Angular to .NET to stored procedure business path.]\nFINAL_DECISION REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH REJECTED_INCOMPLETE_GRAPH FAIL_FALSE_INCOMPLETE"
            ,
            _ => throw new InvalidOperationException($"Unknown cross-stack rejected type: {rejectedType}.")
        };

    private static OperationResult<string[]> ValidateDomainProgram(string[]? domains)
    {
        if (domains is null)
        {
            return OperationResult<string[]>.Fail(
                "Cross-stack seed domain program is required.",
                "CROSS_STACK_SEED_DOMAIN_PROGRAM_REQUIRED");
        }

        if (domains.Length == 0)
        {
            return OperationResult<string[]>.Fail(
                "Cross-stack seed domain program must contain at least one domain.",
                "CROSS_STACK_SEED_DOMAIN_PROGRAM_EMPTY");
        }

        if (domains.Any(domain => string.IsNullOrWhiteSpace(domain) || domain.Trim() != domain))
        {
            return OperationResult<string[]>.Fail(
                "Cross-stack seed domain program must not contain blank or whitespace-padded domains.",
                "CROSS_STACK_SEED_DOMAIN_PROGRAM_BLANK");
        }

        if (domains.Distinct(StringComparer.OrdinalIgnoreCase).Count() != domains.Length)
        {
            return OperationResult<string[]>.Fail(
                "Cross-stack seed domain program must not contain duplicate domains.",
                "CROSS_STACK_SEED_DOMAIN_PROGRAM_DUPLICATE");
        }

        return OperationResult<string[]>.Ok(domains.ToArray());
    }

    private static OperationResult<CrossStackBusinessFlowSeed[]> BuildSeeds(string[] domains)
    {
        var rows = new List<CrossStackBusinessFlowSeed>(
            domains.Length * RejectedTypeCoverageProgram.Length);
        foreach (var domain in domains)
        {
            foreach (var rejected in RejectedTypeCoverageProgram)
            {
                var cellSlug = Slug(rejected);
                var marker = $"flow:{domain}:{cellSlug}";
                var api = $"api:/{domain}/{cellSlug}";
                var procedure = $"sp:{domain}_save_{cellSlug}";
                var result = $"result:{domain}_summary_{cellSlug}";
                var uiState = $"ui:{domain}_state_{cellSlug}";
                var projects = new[] { $"angular-{domain}-{cellSlug}", $"dotnet-{domain}-{cellSlug}", $"sql-{domain}-{cellSlug}" };
                var edges = new[]
                {
                    new SystemBoundaryEdge($"edge-client-api-{domain}-{cellSlug}", projects[0], projects[1], "client_to_server_api", [marker, api], [$"ev-{domain}-{cellSlug}-client", $"ev-{domain}-{cellSlug}-api"], 0.97, "known"),
                    new SystemBoundaryEdge($"edge-server-sql-{domain}-{cellSlug}", projects[1], projects[2], "database_boundary", [marker, procedure, result, uiState], [$"ev-{domain}-{cellSlug}-server", $"ev-{domain}-{cellSlug}-sql", $"ev-{domain}-{cellSlug}-result"], 0.96, "known")
                };
                var boundary = new SystemBoundaryMapResult(
                    $"system-{domain}-{cellSlug}",
                    projects,
                    edges,
                    [],
                    [$"confirm DTO mapping for {domain}", $"confirm UI state consumer {uiState}"],
                    0.94,
                    []);
                rows.Add(new CrossStackBusinessFlowSeed($"graph-{domain}-{cellSlug}", boundary, rejected));
            }
        }

        return ValidateSeedMatrix(domains, rows);
    }

    private static OperationResult<CrossStackBusinessFlowSeed[]> ValidateSeedMatrix(
        string[] domains,
        List<CrossStackBusinessFlowSeed> rows)
    {
        var expected = domains
            .SelectMany(domain => RejectedTypeCoverageProgram.Select(type => MatrixCell(domain, type)))
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actual = rows
            .Select(seed => MatrixCell(ExtractDomain(seed), seed.RejectedType))
            .Order(StringComparer.Ordinal)
            .ToArray();

        // Генератор проверяет только собственную программу посева; независимый
        // acceptance выполняется отдельным validator-ом в pipeline.
        if (!expected.SequenceEqual(actual, StringComparer.Ordinal))
        {
            return OperationResult<CrossStackBusinessFlowSeed[]>.Fail(
                "Cross-stack deterministic seed matrix must cover every domain and rejected type exactly once.",
                "CROSS_STACK_SEED_MATRIX_INCOMPLETE");
        }

        return OperationResult<CrossStackBusinessFlowSeed[]>.Ok(rows.ToArray());
    }

    private static string ExtractDomain(CrossStackBusinessFlowSeed seed)
    {
        var typeSlug = Slug(seed.RejectedType);
        const string prefix = "system-";
        var value = seed.BoundaryMap.SystemId;
        if (!value.StartsWith(prefix, StringComparison.Ordinal) ||
            !value.EndsWith($"-{typeSlug}", StringComparison.Ordinal))
            return "";

        return value[prefix.Length..^($"-{typeSlug}".Length)];
    }

    internal static string MatrixCell(string domain, string rejectedType)
        => $"{domain}|{rejectedType}";

    internal static string Slug(string value)
        => value.Replace('_', '-');

    private sealed record CrossStackBusinessFlowSeed(
        string GraphId,
        SystemBoundaryMapResult BoundaryMap,
        string RejectedType);
}
