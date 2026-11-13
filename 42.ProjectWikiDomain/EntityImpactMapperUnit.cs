// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class EntityImpactMapperUnit
{
    public const string UnitId = "entity-impact-mapper-unit";
    public const string ScenarioId = "project-wiki-entity-impact-mapping";

    public OperationResult<EntityImpactMapResult> Map(
        string projectId,
        string entityName,
        IReadOnlyList<ProjectWikiEvidenceRecord> evidenceRecords)
    {
        if (string.IsNullOrWhiteSpace(projectId))
            return OperationResult<EntityImpactMapResult>.Fail("Project id is required.", "PROJECT_ID_REQUIRED");

        if (string.IsNullOrWhiteSpace(entityName))
            return OperationResult<EntityImpactMapResult>.Fail("Entity name is required.", "ENTITY_NAME_REQUIRED");

        if (evidenceRecords is null)
            return OperationResult<EntityImpactMapResult>.Fail("Evidence records are required.", "EVIDENCE_REQUIRED");

        var relevant = evidenceRecords
            .Where(record => string.Equals(record.ProjectId, projectId, StringComparison.Ordinal) &&
                             Mentions(record, entityName))
            .ToArray();
        var input = relevant.Where(IsInput).Select(FormatTouchpoint).Distinct(StringComparer.Ordinal).ToArray();
        var validation = relevant.Where(IsValidation).Select(FormatTouchpoint).Distinct(StringComparer.Ordinal).ToArray();
        var mapping = relevant.Where(IsMapping).Select(FormatTouchpoint).Distinct(StringComparer.Ordinal).ToArray();
        var calculation = relevant.Where(IsCalculation).Select(FormatTouchpoint).Distinct(StringComparer.Ordinal).ToArray();
        var persistence = relevant.Where(IsPersistence).Select(FormatTouchpoint).Distinct(StringComparer.Ordinal).ToArray();
        var output = relevant.Where(IsOutput).Select(FormatTouchpoint).Distinct(StringComparer.Ordinal).ToArray();
        var tests = relevant.Where(IsTest).Select(FormatTouchpoint).Distinct(StringComparer.Ordinal).ToArray();
        var categorized = input
            .Concat(validation)
            .Concat(mapping)
            .Concat(calculation)
            .Concat(persistence)
            .Concat(output)
            .Concat(tests)
            .ToHashSet(StringComparer.Ordinal);
        var unknown = relevant
            .Select(FormatTouchpoint)
            .Where(item => !categorized.Contains(item))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var evidenceIds = relevant.Select(record => record.EvidenceId).Distinct(StringComparer.Ordinal).ToArray();
        var warnings = evidenceIds.Length == 0 ? ["entity_has_no_evidence"] : Array.Empty<string>();
        var confidence = evidenceIds.Length == 0 ? 0.35 : Math.Min(0.98, 0.70 + evidenceIds.Length * 0.03);

        return OperationResult<EntityImpactMapResult>.Ok(new EntityImpactMapResult(
            projectId,
            entityName,
            input,
            validation,
            mapping,
            calculation,
            persistence,
            output,
            tests,
            unknown,
            evidenceIds,
            confidence,
            warnings));
    }

    private static bool Mentions(ProjectWikiEvidenceRecord record, string entityName)
        => Contains(record.SymbolId, entityName) ||
           Contains(record.RelationSymbolId, entityName) ||
           Contains(record.Meaning, entityName) ||
           Contains(record.Returns, entityName) ||
           record.Parameters.Any(value => Contains(value, entityName)) ||
           record.CalledFunctions.Any(value => Contains(value, entityName)) ||
           record.CalledBy.Any(value => Contains(value, entityName)) ||
           record.KnownCallees.Any(value => Contains(value, entityName));

    private static bool IsInput(ProjectWikiEvidenceRecord record)
        => ContainsAny(record.SymbolId, "controller", "create", "update", "save", "post", "handle") ||
           record.Parameters.Length > 0;

    private static bool IsValidation(ProjectWikiEvidenceRecord record)
        => ContainsAny(record.SymbolId, "validate", "validator", "guard") ||
           ContainsAny(record.Meaning, "validate", "validation", "guard", "required");

    private bool IsMapping(ProjectWikiEvidenceRecord record)
        => ContainsAny(record.SymbolId, "map", "mapper", "dto", "convert", "bind") ||
           ContainsAny(record.Meaning, "map", "dto", "convert", "bind");

    private static bool IsCalculation(ProjectWikiEvidenceRecord record)
        => ContainsAny(record.SymbolId, "calculate", "compute", "total", "price", "score", "discount") ||
           ContainsAny(record.Meaning, "calculate", "compute", "total", "price", "score", "discount");

    private static bool IsPersistence(ProjectWikiEvidenceRecord record)
        => ContainsAny(record.SymbolId, "repository", "store", "save", "persist", "load", "query") ||
           record.CalledFunctions.Any(value => ContainsAny(value, "repository", "store", "save", "persist", "load", "query")) ||
           record.KnownCallees.Any(value => ContainsAny(value, "repository", "store", "save", "persist", "load", "query"));

    private static bool IsOutput(ProjectWikiEvidenceRecord record)
        => ContainsAny(record.SymbolId, "get", "load", "index", "view", "response", "serialize") ||
           ContainsAny(record.Returns, "dto", "json", "view", "response", "string");

    private static bool IsTest(ProjectWikiEvidenceRecord record)
        => ContainsAny(record.SourcePath, "test", "tests", "spec") ||
           ContainsAny(record.SymbolId, "test", "should");

    private static string FormatTouchpoint(ProjectWikiEvidenceRecord record)
        => $"{record.EvidenceId}:{record.SymbolId}";

    private static bool Contains(string value, string part)
        => value.Contains(part, StringComparison.OrdinalIgnoreCase);

    private static bool ContainsAny(string value, params string[] parts)
        => parts.Any(part => Contains(value, part));
}
