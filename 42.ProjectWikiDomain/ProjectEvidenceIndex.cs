// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Globalization;
using System.Text.Json;
using SearchEngine;

public sealed class ProjectEvidenceIndex
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly SearchEngineFacade _search;
    private readonly Dictionary<string, ProjectWikiEvidenceRecord> _recordsById;

    private ProjectEvidenceIndex(
        SearchEngineFacade search,
        Dictionary<string, ProjectWikiEvidenceRecord> recordsById)
    {
        _search = search;
        _recordsById = recordsById;
    }

    public IReadOnlyCollection<ProjectWikiEvidenceRecord> Records => _recordsById.Values;

    public static AgentInfrastructure.OperationResult<ProjectEvidenceIndex> LoadFromWikiDomain(string wikiDomainRoot)
    {
        if (string.IsNullOrWhiteSpace(wikiDomainRoot))
            return AgentInfrastructure.OperationResult<ProjectEvidenceIndex>.Fail("Wiki domain root is required.", "WIKI_ROOT_REQUIRED");

        var evidencePath = Path.Combine(wikiDomainRoot, "evidence", "evidence-index.jsonl");
        return LoadFromEvidenceFileCore(evidencePath, wikiDomainRoot);
    }

    public static AgentInfrastructure.OperationResult<ProjectEvidenceIndex> LoadFromEvidenceFile(string evidencePath)
        => LoadFromEvidenceFileCore(evidencePath, null);

    public static AgentInfrastructure.OperationResult<ProjectEvidenceIndex> CreateFromRecords(
        IEnumerable<ProjectWikiEvidenceRecord> evidenceRecords)
    {
        if (evidenceRecords is null)
            return AgentInfrastructure.OperationResult<ProjectEvidenceIndex>.Fail("Evidence records are required.", "EVIDENCE_RECORDS_REQUIRED");

        var search = new SearchEngineFacade();
        var records = new Dictionary<string, ProjectWikiEvidenceRecord>(StringComparer.Ordinal);
        foreach (var record in evidenceRecords)
        {
            if (record is null || string.IsNullOrWhiteSpace(record.EvidenceId))
                continue;

            records[record.EvidenceId] = record;
            search.Add(record.EvidenceId, BuildSearchText(record));
        }

        if (records.Count == 0)
            return AgentInfrastructure.OperationResult<ProjectEvidenceIndex>.Fail("Evidence records are empty.", "EVIDENCE_RECORDS_EMPTY");

        return AgentInfrastructure.OperationResult<ProjectEvidenceIndex>.Ok(new ProjectEvidenceIndex(search, records));
    }

    private static AgentInfrastructure.OperationResult<ProjectEvidenceIndex> LoadFromEvidenceFileCore(
        string evidencePath,
        string? wikiDomainRoot)
    {
        if (string.IsNullOrWhiteSpace(evidencePath))
            return AgentInfrastructure.OperationResult<ProjectEvidenceIndex>.Fail("Evidence index path is required.", "EVIDENCE_PATH_REQUIRED");

        if (!File.Exists(evidencePath))
            return AgentInfrastructure.OperationResult<ProjectEvidenceIndex>.Fail($"Evidence index not found: {evidencePath}", "EVIDENCE_INDEX_NOT_FOUND");

        var search = new SearchEngineFacade();
        var records = new Dictionary<string, ProjectWikiEvidenceRecord>(StringComparer.Ordinal);
        var relationUnits = string.IsNullOrWhiteSpace(wikiDomainRoot)
            ? new Dictionary<string, RelationUnitSnapshot>(StringComparer.Ordinal)
            : LoadRelationUnits(wikiDomainRoot);

        foreach (var line in File.ReadLines(evidencePath))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var record = JsonSerializer.Deserialize<ProjectWikiEvidenceRecord>(line, JsonOptions);
            if (record is null || string.IsNullOrWhiteSpace(record.EvidenceId))
                continue;

            record = EnrichRecord(record, relationUnits, wikiDomainRoot);
            records[record.EvidenceId] = record;
            search.Add(record.EvidenceId, BuildSearchText(record));
        }

        if (records.Count == 0)
            return AgentInfrastructure.OperationResult<ProjectEvidenceIndex>.Fail($"Evidence index has no usable records: {evidencePath}", "EVIDENCE_INDEX_EMPTY");

        return AgentInfrastructure.OperationResult<ProjectEvidenceIndex>.Ok(new ProjectEvidenceIndex(search, records));
    }

    public bool ContainsEvidenceId(string evidenceId)
        => !string.IsNullOrWhiteSpace(evidenceId) && _recordsById.ContainsKey(evidenceId);

    public ProjectWikiEvidenceRecord? GetById(string evidenceId)
        => _recordsById.TryGetValue(evidenceId, out var record) ? record : null;

    public AgentInfrastructure.OperationResult<IReadOnlyList<ProjectWikiEvidenceHit>> Search(string query, int topK = 5)
    {
        if (string.IsNullOrWhiteSpace(query))
            return AgentInfrastructure.OperationResult<IReadOnlyList<ProjectWikiEvidenceHit>>.Ok([]);

        var searched = _search.Search(query, Math.Max(1, topK));
        if (!searched.IsSuccess)
            return AgentInfrastructure.OperationResult<IReadOnlyList<ProjectWikiEvidenceHit>>.Fail(searched.Error, "BM25_SEARCH_FAILED");

        var directHits = FindDirectEvidenceHits(query);
        var bm25Hits = searched.Value!
            .Where(result => _recordsById.ContainsKey(result.ExternalId))
            .Select(result => new ProjectWikiEvidenceHit(
                _recordsById[result.ExternalId],
                result.Score,
                "bm25"))
            .ToArray();
        var hits = directHits
            .Concat(bm25Hits)
            .GroupBy(hit => hit.Evidence.EvidenceId, StringComparer.Ordinal)
            .Select(group => group.OrderByDescending(hit => hit.Score).First())
            .Take(Math.Max(1, topK))
            .ToArray();

        return AgentInfrastructure.OperationResult<IReadOnlyList<ProjectWikiEvidenceHit>>.Ok(hits);
    }

    private static string BuildSearchText(ProjectWikiEvidenceRecord record)
    {
        var pieces = new[]
        {
            record.EvidenceId,
            record.ProjectId,
            record.DocumentationVersion,
            record.SymbolId,
            record.RelationSymbolId,
            record.DocPath,
            record.SourcePath,
            record.DocumentationLevel,
            record.TargetFramework,
            record.CSharpLanguageVersion,
            record.Meaning,
            record.Returns,
            string.Join(' ', record.Parameters),
            string.Join(' ', record.CalledFunctions),
            string.Join(' ', record.CalledBy),
            string.Join(' ', record.KnownCallees),
            record.StoredProcedureName,
            string.Join(' ', record.SqlParameters),
            string.Join(' ', record.SqlTablesRead),
            string.Join(' ', record.SqlTablesWritten),
            string.Join(' ', record.SqlResultColumns),
            record.SqlTransactionBehavior,
            string.Join(' ', record.SqlSideEffects),
            record.SourceLine.ToString(CultureInfo.InvariantCulture)
        };

        return string.Join(' ', pieces.Where(piece => !string.IsNullOrWhiteSpace(piece)));
    }

    private ProjectWikiEvidenceHit[] FindDirectEvidenceHits(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return [];

        return _recordsById.Values
            .Select(record => new ProjectWikiEvidenceHit(record, DirectMatchScore(query, record), "direct-evidence"))
            .Where(hit => hit.Score > 0)
            .OrderByDescending(hit => hit.Score)
            .ToArray();
    }

    private static double DirectMatchScore(string query, ProjectWikiEvidenceRecord record)
    {
        var score = 0d;
        if (query.Contains(record.EvidenceId, StringComparison.OrdinalIgnoreCase))
            score = Math.Max(score, 1_000_000_000d);

        var sourceLine = record.SourceLine.ToString(CultureInfo.InvariantCulture);
        var sourcePathLine = string.IsNullOrWhiteSpace(record.SourcePath)
            ? ""
            : $"{record.SourcePath}:{sourceLine}";
        if (!string.IsNullOrWhiteSpace(sourcePathLine) &&
            query.Contains(sourcePathLine, StringComparison.OrdinalIgnoreCase))
            score = Math.Max(score, 100_000_000d);

        if (!string.IsNullOrWhiteSpace(record.SourcePath) &&
            query.Contains(record.SourcePath, StringComparison.OrdinalIgnoreCase) &&
            query.Contains(sourceLine, StringComparison.OrdinalIgnoreCase))
            score = Math.Max(score, 50_000_000d);

        var hasSymbol = SymbolAliases(record)
            .Any(alias => ContainsAliasBoundary(query, alias));
        if (hasSymbol && query.Contains(sourceLine, StringComparison.OrdinalIgnoreCase))
            score = Math.Max(score, 10_000_000d);
        if (hasSymbol)
            score = Math.Max(score, 1_000_000d);

        if (!string.IsNullOrWhiteSpace(record.SourcePath) &&
            query.Contains(record.SourcePath, StringComparison.OrdinalIgnoreCase))
            score = Math.Max(score, 10_000d);

        return score;
    }

    private static string[] SymbolAliases(ProjectWikiEvidenceRecord record)
    {
        var aliases = new List<string>();
        AddAlias(aliases, record.SymbolId);
        AddAlias(aliases, record.RelationSymbolId);
        AddAlias(aliases, record.EvidenceId);
        AddAlias(aliases, record.SourcePath);
        if (!string.IsNullOrWhiteSpace(record.SourcePath) && record.SourceLine > 0)
            AddAlias(aliases, $"{record.SourcePath}:{record.SourceLine.ToString(CultureInfo.InvariantCulture)}");
        var parts = record.EvidenceId.Split("::", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
            AddAlias(aliases, parts[^1]);

        return aliases
            .Where(alias => alias.Length >= 4)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool ContainsAliasBoundary(string query, string alias)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(alias))
            return false;

        var start = 0;
        while (start < query.Length)
        {
            var index = query.IndexOf(alias, start, StringComparison.OrdinalIgnoreCase);
            if (index < 0)
                return false;

            var before = index == 0 ? '\0' : query[index - 1];
            var afterIndex = index + alias.Length;
            var after = afterIndex >= query.Length ? '\0' : query[afterIndex];
            if (!IsIdentifierCharacter(before) && !IsIdentifierCharacter(after))
                return true;

            start = index + 1;
        }

        return false;
    }

    private static bool IsIdentifierCharacter(char value)
        => char.IsLetterOrDigit(value) || value == '_';

    private static void AddAlias(List<string> aliases, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        aliases.Add(value);
        var parts = value.Split("::", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
            aliases.Add(parts[^1]);
    }

    private static Dictionary<string, RelationUnitSnapshot> LoadRelationUnits(string wikiDomainRoot)
    {
        var path = Path.Combine(wikiDomainRoot, "relations", "documentation-units.jsonl");
        var units = new Dictionary<string, RelationUnitSnapshot>(StringComparer.Ordinal);
        if (!File.Exists(path))
            return units;

        foreach (var line in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            using var doc = JsonDocument.Parse(line);
            var root = doc.RootElement;
            var id = ReadString(root, "documentation_unit_id");
            if (string.IsNullOrWhiteSpace(id))
                continue;

            units[id] = new RelationUnitSnapshot(
                ReadString(root, "project_id"),
                ReadString(root, "documentation_version"),
                ReadString(root, "symbol_name"),
                ReadString(root, "relation_symbol_id"),
                ReadString(root, "meaning"),
                ReadParameters(root),
                ReadReturn(root),
                ReadNestedSymbols(root, "callees", "callee_id"),
                ReadNestedSymbols(root, "callers", "caller_id"));
        }

        return units;
    }

    private static ProjectWikiEvidenceRecord EnrichRecord(
        ProjectWikiEvidenceRecord record,
        IReadOnlyDictionary<string, RelationUnitSnapshot> relationUnits,
        string? wikiDomainRoot)
    {
        relationUnits.TryGetValue(record.EvidenceId, out var relation);
        var markdown = ReadDocumentationMarkdown(record, wikiDomainRoot);

        return record with
        {
            ProjectId = FirstNonEmpty(record.ProjectId, relation?.ProjectId),
            DocumentationVersion = FirstNonEmpty(record.DocumentationVersion, relation?.DocumentationVersion),
            SymbolId = FirstNonEmpty(record.SymbolId, relation?.SymbolName),
            RelationSymbolId = FirstNonEmpty(record.RelationSymbolId, relation?.RelationSymbolId),
            Meaning = FirstNonEmpty(record.Meaning, relation?.Meaning, markdown),
            Parameters = record.Parameters.Length > 0 ? record.Parameters : relation?.Parameters ?? [],
            Returns = FirstNonEmpty(record.Returns, relation?.ReturnValue),
            CalledFunctions = record.CalledFunctions.Length > 0 ? record.CalledFunctions : relation?.Callees ?? [],
            CalledBy = record.CalledBy.Length > 0 ? record.CalledBy : relation?.Callers ?? [],
            KnownCallees = record.KnownCallees.Length > 0 ? record.KnownCallees : relation?.Callees ?? []
        };
    }

    private static string ReadDocumentationMarkdown(ProjectWikiEvidenceRecord record, string? wikiDomainRoot)
    {
        if (string.IsNullOrWhiteSpace(wikiDomainRoot) || string.IsNullOrWhiteSpace(record.DocPath))
            return "";

        var relative = record.DocPath
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        if (relative.StartsWith($".xiigen{Path.DirectorySeparatorChar}wiki-domain{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            relative = relative[$".xiigen{Path.DirectorySeparatorChar}wiki-domain{Path.DirectorySeparatorChar}".Length..];
        var path = Path.Combine(wikiDomainRoot, relative);
        return File.Exists(path) ? File.ReadAllText(path) : "";
    }

    private static string ReadString(JsonElement element, string property)
        => element.TryGetProperty(property, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? ""
            : "";

    private static string[] ReadParameters(JsonElement root)
    {
        if (!root.TryGetProperty("parameters", out var parameters) || parameters.ValueKind != JsonValueKind.Array)
            return [];

        return parameters.EnumerateArray()
            .Select(parameter =>
            {
                if (parameter.ValueKind == JsonValueKind.String)
                    return parameter.GetString() ?? "";

                var name = ReadString(parameter, "name");
                var type = ReadString(parameter, "type");
                var meaning = ReadString(parameter, "meaning");
                return string.Join(" ", new[] { name, type, meaning }.Where(value => !string.IsNullOrWhiteSpace(value)));
            })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    private static string ReadReturn(JsonElement root)
    {
        if (!root.TryGetProperty("return_value", out var value))
            return "";

        if (value.ValueKind == JsonValueKind.String)
            return value.GetString() ?? "";

        var type = ReadString(value, "type");
        var meaning = ReadString(value, "meaning");
        return string.Join(" ", new[] { type, meaning }.Where(item => !string.IsNullOrWhiteSpace(item)));
    }

    private static string[] ReadNestedSymbols(JsonElement root, string arrayName, string idName)
    {
        if (!root.TryGetProperty(arrayName, out var array) || array.ValueKind != JsonValueKind.Array)
            return [];

        return array.EnumerateArray()
            .Select(item =>
            {
                if (item.ValueKind == JsonValueKind.String)
                    return item.GetString() ?? "";

                var id = ReadString(item, idName);
                var symbol = ReadString(item, arrayName == "callees" ? "callee_symbol" : "caller_symbol");
                var meaning = ReadString(item, "meaning");
                return string.Join(" ", new[] { id, symbol, meaning }.Where(value => !string.IsNullOrWhiteSpace(value)));
            })
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .ToArray();
    }

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "";

    private sealed record RelationUnitSnapshot(
        string ProjectId,
        string DocumentationVersion,
        string SymbolName,
        string RelationSymbolId,
        string Meaning,
        string[] Parameters,
        string ReturnValue,
        string[] Callees,
        string[] Callers);
}
