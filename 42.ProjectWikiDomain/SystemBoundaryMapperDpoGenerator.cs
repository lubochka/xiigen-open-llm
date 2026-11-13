// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class SystemBoundaryMapperDpoGenerator
{
    private readonly SystemBoundaryMapperUnit _mapper;

    public SystemBoundaryMapperDpoGenerator(SystemBoundaryMapperUnit? mapper = null)
    {
        _mapper = mapper ?? new SystemBoundaryMapperUnit();
    }

    public Task<OperationResult<SystemBoundaryMapperDpoBatch>> BuildBatchAsync(
        string batchId = "system-boundary-mapper-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<SystemBoundaryMapperDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var seeds = BuildSeeds(heldOut);
        var pairs = seeds.Select((seed, index) =>
        {
            var mapped = _mapper.Map(seed.SystemId, seed.ProjectEvidence).Value!;
            return new SystemBoundaryMapperDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.ProjectEvidence,
                BuildPrompt(seed),
                BuildChosen(mapped),
                BuildRejected(seed.RejectedType, mapped),
                seed.RejectedType,
                mapped.Edges.Select(edge => edge.EdgeId).ToArray(),
                mapped.Edges.SelectMany(edge => edge.EvidenceIds).Distinct(StringComparer.OrdinalIgnoreCase).ToArray());
        }).ToArray();

        var wrongAnnotationRate = pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return Task.FromResult(OperationResult<SystemBoundaryMapperDpoBatch>.Ok(new SystemBoundaryMapperDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildPrompt(SystemBoundarySeed seed)
        => $"Map one business system across Angular client, .NET server, and SQL stored procedures.\nsystem_id: {seed.SystemId}\nprojects: {string.Join(", ", seed.ProjectEvidence.Select(project => $"{project.ProjectId}:{project.ProjectRole}:{project.Stack}:{project.Version}"))}";

    private static string BuildChosen(SystemBoundaryMapResult mapped)
    {
        var edges = mapped.Edges.Select(edge => $"{edge.EdgeId}:{edge.BoundaryKind}:{string.Join("|", edge.SharedMarkers)}");
        var unknown = mapped.UnknownBoundaries.Length == 0 ? "none" : string.Join(", ", mapped.UnknownBoundaries);
        return $"system_id: {mapped.SystemId}\nproject_ids: {string.Join(", ", mapped.ProjectIds)}\nboundary_edges: {string.Join("; ", edges)}\nunknown_boundaries: {unknown}\nrequired_follow_up_evidence: {string.Join("; ", mapped.RequiredFollowUpEvidence)}\nconfidence: {mapped.Confidence:0.######}";
    }

    private static string BuildRejected(string rejectedType, SystemBoundaryMapResult mapped)
        => rejectedType switch
        {
            "single_repo_only" =>
                $"system_id: {mapped.SystemId}\nboundary_edges: none\nunknown_boundaries: none\n[WRONG: single_repo_only - It treats one repository as the whole system and misses Angular to .NET and .NET to SQL boundaries.]",
            "client_server_only" =>
                $"system_id: {mapped.SystemId}\nboundary_edges: {string.Join("; ", mapped.Edges.Where(edge => edge.BoundaryKind.Contains("client", StringComparison.OrdinalIgnoreCase)).Select(edge => edge.EdgeId))}\nunknown_boundaries: none\n[WRONG: client_server_only - It maps UI to API but drops database and stored procedure evidence required for business logic.]",
            "database_as_server" =>
                $"system_id: {mapped.SystemId}\nboundary_edges: {string.Join("; ", mapped.Edges.Select(edge => edge.EdgeId.Replace("database", "server", StringComparison.OrdinalIgnoreCase)))}\n[WRONG: database_as_server - It collapses SQL stored procedure ownership into the .NET server and loses persistence-side behavior.]",
            "missing_unknown_boundary" =>
                $"system_id: {mapped.SystemId}\nboundary_edges: {string.Join("; ", mapped.Edges.Select(edge => edge.EdgeId))}\nunknown_boundaries: none\n[WRONG: missing_unknown_boundary - It claims full system coverage even when a project has no connecting route, call, deployment, or SQL evidence.]",
            _ =>
                $"system_id: {mapped.SystemId}\nboundary_edges: incomplete\n[WRONG: incomplete_boundary_map - It omits required cross-repository evidence and cannot support advanced wiki questions.]"
        };

    private static SystemBoundarySeed[] BuildSeeds(bool heldOut)
    {
        var suffixes = heldOut
            ? new[] { "order", "invoice", "payment", "shipment", "refund", "inventory", "tenant", "message", "ticket", "audit" }
            : new[] { "campaign", "client", "diet", "media", "task", "game", "profile", "report", "notification", "booking" };
        var rows = new List<SystemBoundarySeed>(50);
        for (var round = 0; round < 5; round++)
        {
            foreach (var suffix in suffixes)
            {
                var marker = $"flow:{suffix}:{round}";
                var api = $"api:/{suffix}/{round}";
                var procedure = $"sp:{suffix}_save_{round}";
                var projects = new List<SystemBoundaryProjectEvidence>
                {
                    new($"angular-{suffix}-{round}", $"C:/Projects/{suffix}-client", "angular_client", "Angular", round % 2 == 0 ? "15" : "16", [marker, api], [$"{suffix}Dto"], [$"ev-{suffix}-{round}-client"]),
                    new($"dotnet-{suffix}-{round}", $"C:/Projects/{suffix}-server", ".net_server", ".NET", round % 2 == 0 ? "Framework 4.8" : "net8.0", [marker, api, procedure], [$"{suffix}Entity"], [$"ev-{suffix}-{round}-server"]),
                    new($"sql-{suffix}-{round}", $"C:/Projects/{suffix}-database", "sql_database", "SQL Server", "2019", [marker, procedure], [$"{suffix}Table"], [$"ev-{suffix}-{round}-sql"])
                };
                if (round == 4)
                    projects.Add(new($"unknown-{suffix}-{round}", $"C:/Projects/{suffix}-worker", "background_worker", ".NET", "unknown", [], [], [$"ev-{suffix}-{round}-unknown"]));
                var rejected = round switch
                {
                    0 => "single_repo_only",
                    1 => "client_server_only",
                    2 => "database_as_server",
                    4 => "missing_unknown_boundary",
                    _ => "incomplete_boundary_map"
                };
                rows.Add(new SystemBoundarySeed($"system-{suffix}-{round}", projects.ToArray(), rejected));
            }
        }

        return rows.Take(50).ToArray();
    }

    private sealed record SystemBoundarySeed(
        string SystemId,
        SystemBoundaryProjectEvidence[] ProjectEvidence,
        string RejectedType);
}
