// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class NegativeDpoGeneratorBatchBuilder
{
    private static readonly string[] RequiredNegativeTypes =
    [
        "missing_location",
        "missing_parameters",
        "missing_return_contract",
        "missing_callee_transit",
        "missing_caller_context"
    ];

    private readonly NegativeDtoDocumentationOrchestrator _orchestrator;

    public NegativeDpoGeneratorBatchBuilder(NegativeDtoDocumentationOrchestrator? orchestrator = null)
    {
        _orchestrator = orchestrator ?? new NegativeDtoDocumentationOrchestrator();
    }

    public Task<OperationResult<NegativeDpoGeneratorBatch>> BuildBatchAsync(
        string batchId = "negative-dpo-generator-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<NegativeDpoGeneratorBatch>.Fail("Cancelled.", "CANCELLED"));

        var focusedBatch = IsFocusedBatch(batchId);
        var units = BuildUnits(batchId, heldOut);
        var bundle = _orchestrator.BuildBundle(units);
        var candidates = bundle.Candidates.ToArray();
        var validations = bundle.Validations.ToArray();
        var pairs = bundle.DpoPairs.ToArray();
        var wrongAnnotationRate = candidates.Length == 0
            ? 0
            : candidates.Count(candidate => candidate.RejectedDocumentation.Contains("[WRONG:", StringComparison.Ordinal)) / (double)candidates.Length;
        var acceptedRate = validations.Length == 0
            ? 0
            : validations.Count(validation => validation.IsAccepted) / (double)validations.Length;
        var coverage = focusedBatch
            ? 1.0
            : RequiredNegativeTypes.Count(type => candidates.Any(candidate => candidate.NegativeType == type)) / (double)RequiredNegativeTypes.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");
        if (acceptedRate < 1.0)
            warnings.Add("accepted_candidate_rate_below_gate");
        if (!focusedBatch && coverage < 1.0)
            warnings.Add("negative_type_coverage_below_gate");

        return Task.FromResult(OperationResult<NegativeDpoGeneratorBatch>.Ok(new NegativeDpoGeneratorBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            acceptedRate,
            coverage,
            units,
            candidates,
            validations,
            pairs,
            warnings.ToArray())));
    }

    private static ProjectDocumentationUnit[] BuildUnits(string batchId, bool heldOut)
    {
        if (IsWeakReplayBatch(batchId))
            return BuildWeakReplayUnits(batchId, heldOut);

        if (IsCallerReplayBatch(batchId))
            return BuildCallerReplayUnits(batchId, heldOut);

        if (IsPathReplayBatch(batchId))
            return BuildPathReplayUnits(batchId, heldOut);

        foreach (var negativeType in RequiredNegativeTypes)
        {
            if (batchId.Contains(negativeType, StringComparison.OrdinalIgnoreCase))
                return BuildFocusedUnits(negativeType, batchId, heldOut);
        }

        return BuildMixedUnits(batchId, heldOut);
    }

    private static bool IsFocusedBatch(string batchId)
        => RequiredNegativeTypes.Any(type => batchId.Contains(type, StringComparison.OrdinalIgnoreCase));

    private static bool IsWeakReplayBatch(string batchId)
        => batchId.Contains("weak-replay", StringComparison.OrdinalIgnoreCase) ||
           batchId.Contains("replay-weak", StringComparison.OrdinalIgnoreCase);

    private static bool IsCallerReplayBatch(string batchId)
        => batchId.Contains("caller-replay", StringComparison.OrdinalIgnoreCase) ||
           batchId.Contains("replay-caller", StringComparison.OrdinalIgnoreCase);

    private static bool IsPathReplayBatch(string batchId)
        => batchId.Contains("path-replay", StringComparison.OrdinalIgnoreCase) ||
           batchId.Contains("replay-path", StringComparison.OrdinalIgnoreCase);

    private static ProjectDocumentationUnit[] BuildMixedUnits(string batchId, bool heldOut)
    {
        var prefix = heldOut ? "heldout" : "train";
        var projects = new[] { "askapediaweb", "genietasks", "GenieDynamicServer", "wizGameserver", "geniesoftGameMVPServer" };
        var units = new List<ProjectDocumentationUnit>(50);
        var offset = BatchOffset(batchId);

        for (var index = 0; index < 10; index++)
        {
            var actualIndex = offset + index;
            var projectId = projects[index % projects.Length];
            units.Add(Unit(
                prefix,
                projectId,
                actualIndex,
                "order-workflow",
                $"OrderWorkflow.ResolveOrder{actualIndex}",
                $"Loads the order aggregate and returns the resolved order summary for the request workflow.",
                ["orderId:int"],
                "OrderSummary resolvedOrder",
                ["OrderRepository.LoadOrder", "OrderPolicy.ApplyVisibility"],
                []));
            units.Add(Unit(
                prefix,
                projectId,
                actualIndex,
                "access-policy",
                $"AccessPolicy.CanEditEntity{actualIndex}",
                $"Checks whether a user can edit an entity before the controller continues the save workflow.",
                ["userId:int", "entityId:int"],
                "bool allowed",
                [],
                ["EntityController.Post", "EntityController.Put"]));
            units.Add(Unit(
                prefix,
                projectId,
                actualIndex,
                "request-mapping",
                $"RequestMapper.MapRequest{actualIndex}",
                $"Converts an incoming request DTO into the domain request used by downstream services.",
                ["dto:RequestDto", "tenantId:string"],
                "DomainRequest mappedRequest",
                ["RequestValidator.ValidateDto"],
                ["RequestController.Post"]));
            units.Add(Unit(
                prefix,
                projectId,
                actualIndex,
                "pricing",
                $"PricingCalculator.CalculateTotal{actualIndex}",
                $"Calculates the persisted total that caller workflows pass to invoice and audit steps.",
                ["items:OrderLine[]", "discountCode:string"],
                "decimal total",
                ["TaxService.Apply", "DiscountPolicy.Apply"],
                ["OrderController.BuildOrder", "InvoiceService.CreateDraft"]));
            units.Add(Unit(
                prefix,
                projectId,
                actualIndex,
                "normalization",
                $"SlugNormalizer.Normalize{actualIndex}",
                $"Normalizes a display value before it is stored or compared by the project wiki.",
                [],
                "",
                [],
                []));
        }

        return units.ToArray();
    }

    private static ProjectDocumentationUnit[] BuildFocusedUnits(string negativeType, string batchId, bool heldOut)
    {
        var prefix = heldOut ? "heldout" : "train";
        var projects = new[] { "askapediaweb", "genietasks", "GenieDynamicServer", "wizGameserver", "geniesoftGameMVPServer" };
        var units = new List<ProjectDocumentationUnit>(50);
        var offset = BatchOffset(batchId);
        for (var index = 0; index < 50; index++)
        {
            var actualIndex = offset + index;
            var projectId = projects[index % projects.Length];
            units.Add(BuildUnitForNegativeType(prefix, projectId, actualIndex, negativeType));
        }

        return units.ToArray();
    }

    private static ProjectDocumentationUnit[] BuildWeakReplayUnits(string batchId, bool heldOut)
    {
        var prefix = heldOut ? "heldout" : "train";
        var projects = new[] { "askapediaweb", "genietasks", "GenieDynamicServer", "wizGameserver", "geniesoftGameMVPServer" };
        var countsByType = new (string Type, int Count)[]
        {
            ("missing_return_contract", 15),
            ("missing_callee_transit", 15),
            ("missing_location", 10),
            ("missing_parameters", 5),
            ("missing_caller_context", 5)
        };
        var units = new List<ProjectDocumentationUnit>(50);
        var offset = BatchOffset(batchId);
        var index = 0;
        foreach (var (negativeType, count) in countsByType)
        {
            for (var item = 0; item < count; item++)
            {
                var actualIndex = offset + index;
                var projectId = projects[index % projects.Length];
                units.Add(BuildUnitForNegativeType(prefix, projectId, actualIndex, negativeType));
                index++;
            }
        }

        return units.ToArray();
    }

    private static ProjectDocumentationUnit[] BuildCallerReplayUnits(string batchId, bool heldOut)
    {
        var prefix = heldOut ? "heldout" : "train";
        var projects = new[] { "askapediaweb", "genietasks", "GenieDynamicServer", "wizGameserver", "geniesoftGameMVPServer" };
        var countsByType = new (string Type, int Count)[]
        {
            ("missing_caller_context", 15),
            ("missing_return_contract", 15),
            ("missing_callee_transit", 10),
            ("missing_location", 5),
            ("missing_parameters", 5)
        };
        var units = new List<ProjectDocumentationUnit>(50);
        var offset = BatchOffset(batchId);
        var index = 0;
        foreach (var (negativeType, count) in countsByType)
        {
            for (var item = 0; item < count; item++)
            {
                var actualIndex = offset + index;
                var projectId = projects[index % projects.Length];
                units.Add(BuildUnitForNegativeType(prefix, projectId, actualIndex, negativeType));
                index++;
            }
        }

        return units.ToArray();
    }

    private static ProjectDocumentationUnit[] BuildPathReplayUnits(string batchId, bool heldOut)
    {
        var prefix = heldOut ? "heldout" : "train";
        var projects = new[] { "askapediaweb", "genietasks", "GenieDynamicServer", "wizGameserver", "geniesoftGameMVPServer" };
        var countsByType = new (string Type, int Count)[]
        {
            ("missing_callee_transit", 20),
            ("missing_caller_context", 20),
            ("missing_return_contract", 5),
            ("missing_location", 3),
            ("missing_parameters", 2)
        };
        var units = new List<ProjectDocumentationUnit>(50);
        var offset = BatchOffset(batchId);
        var index = 0;
        foreach (var (negativeType, count) in countsByType)
        {
            for (var item = 0; item < count; item++)
            {
                var actualIndex = offset + index;
                var projectId = projects[index % projects.Length];
                units.Add(BuildUnitForNegativeType(prefix, projectId, actualIndex, negativeType));
                index++;
            }
        }

        return units.ToArray();
    }

    private static ProjectDocumentationUnit BuildUnitForNegativeType(
        string prefix,
        string projectId,
        int actualIndex,
        string negativeType)
        => negativeType switch
        {
            "missing_location" => Unit(
                prefix,
                projectId,
                actualIndex,
                "normalization",
                $"SlugNormalizer.NormalizeValue{actualIndex}",
                "Normalizes a display value before it is stored or compared by the project wiki.",
                [],
                "",
                [],
                []),
            "missing_parameters" => BuildParameterContractUnit(prefix, projectId, actualIndex),
            "missing_return_contract" => Unit(
                prefix,
                projectId,
                actualIndex,
                "pricing",
                $"PricingCalculator.CalculateTotal{actualIndex}",
                "Calculates the persisted total that caller workflows pass to invoice and audit steps.",
                [$"items{actualIndex}:OrderLine[]", $"discountCode{actualIndex}:string"],
                $"decimal total{actualIndex}",
                [$"TaxService.Apply{actualIndex}", $"DiscountPolicy.Apply{actualIndex}"],
                [$"OrderController.BuildOrder{actualIndex}", $"InvoiceService.CreateDraft{actualIndex}"]),
            "missing_callee_transit" => Unit(
                prefix,
                projectId,
                actualIndex,
                "order-workflow",
                $"OrderWorkflow.ResolveOrder{actualIndex}",
                "Loads the order aggregate and returns the resolved order summary for the request workflow.",
                [$"orderId{actualIndex}:int"],
                $"OrderSummary resolvedOrder{actualIndex}",
                [$"OrderRepository.LoadOrder{actualIndex}", $"OrderPolicy.ApplyVisibility{actualIndex}"],
                []),
            "missing_caller_context" => Unit(
                prefix,
                projectId,
                actualIndex,
                "access-policy",
                $"AccessPolicy.CanEditEntity{actualIndex}",
                "Checks whether a user can edit an entity before the controller continues the save workflow.",
                [$"userId{actualIndex}:int", $"entityId{actualIndex}:int"],
                "bool allowed",
                [],
                [$"EntityController.Post{actualIndex}", $"EntityController.Put{actualIndex}"]),
            _ => throw new InvalidOperationException($"Unsupported negative type {negativeType}.")
        };

    private static ProjectDocumentationUnit BuildParameterContractUnit(
        string prefix,
        string projectId,
        int actualIndex)
        => Math.Abs(actualIndex % 5) switch
        {
            0 => Unit(
                prefix,
                projectId,
                actualIndex,
                "request-mapping",
                $"RequestMapper.MapRequest{actualIndex}",
                "Converts an incoming request DTO into the domain request used by downstream services.",
                [$"dto{actualIndex}:RequestDto", $"tenantId{actualIndex}:string"],
                $"DomainRequest mappedRequest{actualIndex}",
                [$"RequestValidator.ValidateDto{actualIndex}"],
                [$"RequestController.Post{actualIndex}"]),
            1 => Unit(
                prefix,
                projectId,
                actualIndex,
                "entity-update",
                $"EntityUpdateMapper.MapEntityUpdate{actualIndex}",
                "Maps an entity update command into the domain entity mutation used by persistence services.",
                [$"entityId{actualIndex}:int", $"patch{actualIndex}:EntityPatchDto", $"actorId{actualIndex}:int"],
                $"EntityMutation mutation{actualIndex}",
                [$"EntityPatchValidator.Validate{actualIndex}"],
                [$"EntityController.Put{actualIndex}"]),
            2 => Unit(
                prefix,
                projectId,
                actualIndex,
                "search-query",
                $"SearchQueryBuilder.BuildSearchQuery{actualIndex}",
                "Builds a repository query from developer-facing filters before a workflow reads matching entities.",
                [$"filters{actualIndex}:SearchFilters", $"page{actualIndex}:int", $"pageSize{actualIndex}:int"],
                $"SearchQuery query{actualIndex}",
                [$"SearchFilterNormalizer.Normalize{actualIndex}"],
                [$"SearchController.Get{actualIndex}"]),
            3 => Unit(
                prefix,
                projectId,
                actualIndex,
                "report-filter",
                $"ReportFilterFactory.CreateReportFilter{actualIndex}",
                "Creates a report filter object that controls which records enter the exported report workflow.",
                [$"dateFrom{actualIndex}:DateTime", $"dateTo{actualIndex}:DateTime", $"status{actualIndex}:string"],
                $"ReportFilter filter{actualIndex}",
                [$"ReportDateRangeGuard.EnsureValid{actualIndex}"],
                [$"ReportController.Export{actualIndex}"]),
            _ => Unit(
                prefix,
                projectId,
                actualIndex,
                "workflow-command",
                $"WorkflowCommandFactory.CreateCommand{actualIndex}",
                "Creates the command object that a workflow dispatcher uses to start a project operation.",
                [$"workflowId{actualIndex}:Guid", $"payload{actualIndex}:WorkflowPayload", $"requestedBy{actualIndex}:string"],
                $"WorkflowCommand command{actualIndex}",
                [$"WorkflowPayloadValidator.Validate{actualIndex}"],
                [$"WorkflowController.Start{actualIndex}"])
        };

    private static int BatchOffset(string batchId)
    {
        var value = 0;
        foreach (var character in batchId)
            value = unchecked((value * 31) + character);
        return Math.Abs(value % 10000) * 100;
    }

    private static ProjectDocumentationUnit Unit(
        string prefix,
        string projectId,
        int index,
        string area,
        string symbolName,
        string meaning,
        string[] parameters,
        string returnValue,
        string[] callees,
        string[] callers)
        => new(
            $"{prefix}-{projectId}-{area}-{index:000}",
            projectId,
            $@"C:\Projects\{projectId}",
            $"{projectId}/Services/{area}/Unit{index}.cs",
            10 + index,
            symbolName,
            "D1",
            index % 2 == 0 ? "v4.6.1" : "net8.0",
            index % 2 == 0 ? "CSharp7" : "CSharp12",
            meaning,
            parameters,
            returnValue,
            callees,
            callers,
            [$"{projectId}:{area}:Unit{index}"]);
}
