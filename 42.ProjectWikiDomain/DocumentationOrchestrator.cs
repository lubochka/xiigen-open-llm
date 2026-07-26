// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using AgentInfrastructure;

public sealed partial class DocumentationOrchestrator
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public async Task<OperationResult<ProjectDocumentationResult>> GenerateAsync(
        ProjectDocumentationRequest request,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return OperationResult<ProjectDocumentationResult>.Fail("Cancelled.", "CANCELLED");

        if (request is null || string.IsNullOrWhiteSpace(request.ProjectRoot))
            return OperationResult<ProjectDocumentationResult>.Fail("Project root is required.", "PROJECT_ROOT_REQUIRED");

        if (!Directory.Exists(request.ProjectRoot))
            return OperationResult<ProjectDocumentationResult>.Fail($"Project root not found: {request.ProjectRoot}", "PROJECT_ROOT_NOT_FOUND");

        var projectId = Path.GetFileName(Path.GetFullPath(request.ProjectRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
        var wikiRoot = string.IsNullOrWhiteSpace(request.WikiDomainRoot)
            ? Path.Combine(request.ProjectRoot, ".xiigen", "wiki-domain")
            : request.WikiDomainRoot!;
        var version = string.IsNullOrWhiteSpace(request.DocumentationVersion)
            ? $"generated-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}"
            : request.DocumentationVersion;
        var metadata = ReadProjectMetadata(request.ProjectRoot);
        var sourceFileCount = CountIncludedSourceFiles(request.ProjectRoot);
        var methods = ExtractMethods(request.ProjectRoot, projectId, metadata).ToArray();
        var clientArtifacts = ExtractClientArtifacts(request.ProjectRoot, projectId, metadata).ToArray();
        var sqlProcedures = ExtractSqlProcedures(request.ProjectRoot).ToArray();
        var callEdges = BuildCallEdges(projectId, methods).ToArray();
        var valueFlows = BuildValueFlows(projectId, callEdges).ToArray();
        var units = BuildUnits(request.ProjectRoot, projectId, version, metadata, methods, clientArtifacts, callEdges).ToArray();
        var evidenceRecords = BuildEvidenceRecords(version, units, methods, sqlProcedures).ToArray();
        var negativeBundle = new NegativeDtoDocumentationOrchestrator().BuildBundle(units);
        var index = ProjectEvidenceIndex.CreateFromRecords(evidenceRecords);
        if (!index.IsSuccess)
            return index.MapError<ProjectDocumentationResult>();

        var questionSeeds = new ProjectWikiQuestionSeedGenerator().Generate(index.Value!, maxRecords: Math.Max(1, units.Length));
        var trace = new ProjectCodeTrace(
            projectId,
            request.ProjectRoot,
            version,
            "INTAKE_ACCEPTED",
            [metadata.TargetFramework],
            [metadata.CSharpLanguageVersion],
            sourceFileCount,
            methods.Length + clientArtifacts.Length,
            callEdges.Length,
            valueFlows.Length,
            0,
            0,
            methods.Length == 0 && clientArtifacts.Length == 0 ? ["no_code_artifacts_detected"] : []);
        var graph = new ProjectDocumentationKnowledgeGraph(
            projectId,
            request.ProjectRoot,
            version,
            trace,
            units,
            callEdges,
            valueFlows,
            evidenceRecords,
            questionSeeds,
            negativeBundle);

        if (request.PersistArtifacts)
        {
            WriteWikiDomain(request.ProjectRoot, wikiRoot, projectId, version, metadata, graph);
            WriteQuestionSeeds(wikiRoot, questionSeeds);
        }

        var acceptedNegativeCount = negativeBundle.Validations.Count(validation => validation.IsAccepted);
        var negativeAcceptanceRate = negativeBundle.Candidates.Count == 0
            ? 0
            : acceptedNegativeCount / (double)negativeBundle.Candidates.Count;

        await Task.CompletedTask;
        return OperationResult<ProjectDocumentationResult>.Ok(new ProjectDocumentationResult(
            projectId,
            wikiRoot,
            version,
            units.Length,
            callEdges.Length,
            valueFlows.Length,
            questionSeeds.Count,
            negativeBundle.DpoPairs.Count,
            negativeAcceptanceRate,
            methods.Length == 0 && clientArtifacts.Length == 0 ? ["no_code_artifacts_detected"] : [],
            graph));
    }

    private static ProjectMetadata ReadProjectMetadata(string projectRoot)
    {
        var csproj = Directory.EnumerateFiles(projectRoot, "*.csproj", SearchOption.AllDirectories)
            .FirstOrDefault(path => !IsExcludedPath(path));
        if (csproj is null)
            return ReadClientProjectMetadata(projectRoot);

        var xml = File.ReadAllText(csproj);
        var target = MatchValue(xml, @"<TargetFramework>(?<v>[^<]+)</TargetFramework>");
        if (string.IsNullOrWhiteSpace(target))
            target = MatchValue(xml, @"<TargetFrameworkVersion>(?<v>[^<]+)</TargetFrameworkVersion>");
        if (string.IsNullOrWhiteSpace(target))
            target = "unknown";

        var runtime = target.StartsWith("netcoreapp", StringComparison.OrdinalIgnoreCase) ||
                      Regex.IsMatch(target, @"^net[5-9]", RegexOptions.IgnoreCase)
            ? "NET_CORE"
            : target.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase)
                ? "NET_STANDARD"
                : "NET_FRAMEWORK";
        var language = MatchValue(xml, @"<LangVersion>(?<v>[^<]+)</LangVersion>");
        if (string.IsNullOrWhiteSpace(language))
            language = runtime == "NET_CORE" ? "CSharp8" : InferLegacyLanguage(target);
        else
            language = $"explicit:{language}";
        var style = xml.Contains("<Project Sdk=", StringComparison.OrdinalIgnoreCase)
            ? "sdk_style"
            : "legacy_non_sdk";

        return new ProjectMetadata(runtime, language, target, style);
    }

    private static ProjectMetadata ReadClientProjectMetadata(string projectRoot)
    {
        var packageJson = Directory.EnumerateFiles(projectRoot, "package.json", SearchOption.AllDirectories)
            .FirstOrDefault(path => !IsExcludedPath(path));
        if (packageJson is null)
            return new ProjectMetadata("unknown", "inferred_unknown", "unknown", "unknown");

        var packageText = File.ReadAllText(packageJson);
        var angularVersion = MatchValue(packageText, @"""@angular/core""\s*:\s*""(?<v>[^""]+)""");
        var typescriptVersion = MatchValue(packageText, @"""typescript""\s*:\s*""(?<v>[^""]+)""");
        if (!string.IsNullOrWhiteSpace(angularVersion))
            return new ProjectMetadata("ANGULAR_CLIENT", $"TypeScript {FirstNonEmpty(typescriptVersion, "unknown")}", $"Angular {angularVersion}", "package_json");

        var reactVersion = MatchValue(packageText, @"""react""\s*:\s*""(?<v>[^""]+)""");
        if (!string.IsNullOrWhiteSpace(reactVersion))
            return new ProjectMetadata("REACT_CLIENT", $"TypeScript {FirstNonEmpty(typescriptVersion, "unknown")}", $"React {reactVersion}", "package_json");

        return new ProjectMetadata("CLIENT_UNKNOWN", $"TypeScript {FirstNonEmpty(typescriptVersion, "unknown")}", "unknown_client_framework", "package_json");
    }

    private static IEnumerable<ExtractedMethod> ExtractMethods(
        string projectRoot,
        string projectId,
        ProjectMetadata metadata)
    {
        foreach (var file in Directory.EnumerateFiles(projectRoot, "*.cs", SearchOption.AllDirectories).Where(path => !IsExcludedPath(path)))
        {
            var text = File.ReadAllText(file);
            foreach (Match match in MethodPattern().Matches(text))
            {
                var name = match.Groups["name"].Value;
                if (IsControlKeyword(name))
                    continue;

                var openBrace = text.IndexOf('{', match.Index);
                if (openBrace < 0)
                    continue;

                var closeBrace = FindMatchingBrace(text, openBrace);
                if (closeBrace <= openBrace)
                    continue;

                var className = FindNearestClassName(text, match.Index);
                var relative = Path.GetRelativePath(projectRoot, file);
                var line = 1 + text[..match.Index].Count(ch => ch == '\n');
                var returnType = match.Groups["return"].Value;
                var parameters = SplitParameters(match.Groups["parameters"].Value);
                var body = text.Substring(openBrace + 1, closeBrace - openBrace - 1);
                var symbol = string.IsNullOrWhiteSpace(className)
                    ? $"{projectId}::{name}"
                    : $"{projectId}::{className}.{name}";

                yield return new ExtractedMethod(
                    projectId,
                    file,
                    relative,
                    line,
                    symbol,
                    name,
                    className,
                    returnType,
                    parameters,
                    body,
                    metadata);
            }
        }
    }

    private static IEnumerable<ProjectDocumentationCallEdge> BuildCallEdges(
        string projectId,
        IReadOnlyList<ExtractedMethod> methods)
    {
        var byName = methods
            .GroupBy(method => method.Name, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.ToArray(), StringComparer.Ordinal);

        foreach (var caller in methods)
        {
            foreach (var pair in byName)
            {
                if (string.Equals(pair.Key, caller.Name, StringComparison.Ordinal))
                    continue;

                var expression = FindCallExpression(caller.Body, pair.Key);
                if (string.IsNullOrWhiteSpace(expression))
                    continue;

                var callee = pair.Value.FirstOrDefault(method => string.Equals(method.ProjectId, caller.ProjectId, StringComparison.Ordinal));
                if (callee is null)
                    continue;

                yield return new ProjectDocumentationCallEdge(
                    projectId,
                    UnitId(caller),
                    UnitId(callee),
                    caller.Symbol,
                    callee.Symbol,
                    expression);
            }
        }
    }

    private static IEnumerable<ProjectDocumentationValueFlow> BuildValueFlows(
        string projectId,
        IReadOnlyList<ProjectDocumentationCallEdge> edges)
    {
        foreach (var edge in edges)
        {
            var args = ExtractArguments(edge.CallExpression);
            var usage = edge.CallExpression.Contains("return ", StringComparison.Ordinal)
                ? "caller returns the callee result directly"
                : "caller uses the callee result or side effect in local behavior";

            yield return new ProjectDocumentationValueFlow(
                projectId,
                edge.CallerId,
                edge.CalleeId,
                args,
                usage);
        }
    }

    private static IEnumerable<ProjectDocumentationUnit> BuildUnits(
        string projectRoot,
        string projectId,
        string version,
        ProjectMetadata metadata,
        IReadOnlyList<ExtractedMethod> methods,
        IReadOnlyList<ExtractedClientArtifact> clientArtifacts,
        IReadOnlyList<ProjectDocumentationCallEdge> callEdges)
    {
        foreach (var method in methods)
        {
            var id = UnitId(method);
            var callees = callEdges.Where(edge => edge.CallerId == id).Select(edge => edge.CalleeId).Distinct(StringComparer.Ordinal).ToArray();
            var callers = callEdges.Where(edge => edge.CalleeId == id).Select(edge => edge.CallerId).Distinct(StringComparer.Ordinal).ToArray();
            var level = callees.Length == 0 && callers.Length == 0
                ? "D1"
                : callees.Length > 0 && callers.Length > 0 ? "D3" : "D2";
            var evidenceId = id;
            var meaning = InferMeaning(method, callees, callers);

            yield return new ProjectDocumentationUnit(
                id,
                projectId,
                projectRoot,
                method.RelativePath,
                method.Line,
                method.Symbol,
                level,
                metadata.TargetFramework,
                metadata.CSharpLanguageVersion,
                meaning,
                method.Parameters,
                $"{method.ReturnType} returned by {method.Symbol}",
                callees,
                callers,
                [evidenceId]);
        }

        foreach (var artifact in clientArtifacts)
        {
            var id = UnitId(artifact);
            yield return new ProjectDocumentationUnit(
                id,
                projectId,
                projectRoot,
                artifact.RelativePath,
                artifact.Line,
                artifact.Symbol,
                artifact.ArtifactKind,
                metadata.TargetFramework,
                metadata.CSharpLanguageVersion,
                artifact.Meaning,
                artifact.Parameters,
                artifact.ReturnValue,
                [],
                [],
                [id]);
        }
    }

    private static IEnumerable<ExtractedClientArtifact> ExtractClientArtifacts(
        string projectRoot,
        string projectId,
        ProjectMetadata metadata)
    {
        foreach (var file in Directory.EnumerateFiles(projectRoot, "*.ts", SearchOption.AllDirectories).Where(path => !IsExcludedPath(path)))
        {
            var text = File.ReadAllText(file);
            var relative = Path.GetRelativePath(projectRoot, file);
            foreach (Match match in ClientClassPattern().Matches(text))
            {
                var name = match.Groups["name"].Value;
                var line = 1 + text[..match.Index].Count(ch => ch == '\n');
                var decorators = ExtractAngularDecorators(text, match.Index);
                var httpCalls = ExtractHttpCalls(text);
                var routes = ExtractRouteStrings(text);
                var meaning = $"{projectId}::{name} is an Angular/TypeScript artifact in {relative}. Decorators: {FormatInline(decorators)}. HTTP/API calls: {FormatInline(httpCalls)}. Routes or path literals: {FormatInline(routes)}.";
                yield return new ExtractedClientArtifact(
                    projectId,
                    file,
                    relative,
                    line,
                    $"{projectId}::{name}",
                    name,
                    "client_component_or_service",
                    [.. decorators, .. httpCalls, .. routes],
                    metadata,
                    meaning);
            }
        }

        foreach (var file in Directory.EnumerateFiles(projectRoot, "*.html", SearchOption.AllDirectories).Where(path => !IsExcludedPath(path)))
        {
            var text = File.ReadAllText(file);
            if (string.IsNullOrWhiteSpace(text))
                continue;

            var relative = Path.GetRelativePath(projectRoot, file);
            var bindings = ExtractTemplateBindings(text);
            var symbol = $"{projectId}::{Path.GetFileNameWithoutExtension(file)}Template";
            yield return new ExtractedClientArtifact(
                projectId,
                file,
                relative,
                1,
                symbol,
                Path.GetFileName(file),
                "client_template",
                bindings,
                metadata,
                $"{symbol} defines Angular template bindings and user-visible UI state. Bindings: {FormatInline(bindings)}.");
        }
    }

    private static IEnumerable<ProjectWikiEvidenceRecord> BuildEvidenceRecords(
        string version,
        IReadOnlyList<ProjectDocumentationUnit> units,
        IReadOnlyList<ExtractedMethod> methods,
        IReadOnlyList<SqlProcedureSnapshot> sqlProcedures)
    {
        var methodsByUnitId = methods.ToDictionary(UnitId, StringComparer.Ordinal);
        foreach (var unit in units)
        {
            methodsByUnitId.TryGetValue(unit.DocumentationUnitId, out var method);
            var procedureCall = method is null ? null : ExtractStoredProcedureCall(method);
            var procedure = FindSqlProcedure(sqlProcedures, procedureCall?.ProcedureName ?? "");
            var sqlParameters = MergeSqlParameters(procedureCall, procedure);
            var sqlResultColumns = procedure?.ResultColumns ?? [];
            var sqlTablesRead = procedure?.TablesRead ?? [];
            var sqlTablesWritten = procedure?.TablesWritten ?? [];
            var sqlSideEffects = procedure?.SideEffects ?? [];
            var storedProcedureName = FirstNonEmpty(procedureCall?.ProcedureName, procedure?.Name);
            var returns = sqlResultColumns.Length == 0
                ? unit.ReturnValue
                : $"{unit.ReturnValue}; SQL result columns: {string.Join(", ", sqlResultColumns)}";
            var meaning = string.IsNullOrWhiteSpace(storedProcedureName)
                ? unit.Meaning
                : $"{unit.Meaning} Stored procedure evidence: {storedProcedureName}; parameters {FormatInline(sqlParameters)}; reads {FormatInline(sqlTablesRead)}; writes {FormatInline(sqlTablesWritten)}; result columns {FormatInline(sqlResultColumns)}; side effects {FormatInline(sqlSideEffects)}.";

            yield return new ProjectWikiEvidenceRecord
            {
                EvidenceId = unit.DocumentationUnitId,
                ProjectId = unit.ProjectId,
                DocumentationVersion = version,
                SymbolId = unit.SymbolName,
                RelationSymbolId = unit.DocumentationUnitId,
                DocPath = $".xiigen/wiki-domain/documentation/{DocumentationFileName(unit)}",
                SourcePath = unit.SourcePath,
                SourceLine = unit.SourceLine,
                DocumentationLevel = unit.DocumentationLevel,
                TargetFramework = unit.TargetFramework,
                CSharpLanguageVersion = unit.CSharpLanguageVersion,
                Meaning = meaning,
                Parameters = unit.Parameters,
                Returns = returns,
                CalledFunctions = unit.Callees,
                CalledBy = unit.Callers,
                KnownCallees = unit.Callees,
                StoredProcedureName = storedProcedureName,
                SqlParameters = sqlParameters,
                SqlTablesRead = sqlTablesRead,
                SqlTablesWritten = sqlTablesWritten,
                SqlResultColumns = sqlResultColumns,
                SqlTransactionBehavior = procedure?.TransactionBehavior ?? "",
                SqlSideEffects = sqlSideEffects
            };
        }
    }

    private static IEnumerable<SqlProcedureSnapshot> ExtractSqlProcedures(string projectRoot)
    {
        foreach (var file in Directory.EnumerateFiles(projectRoot, "*.sql", SearchOption.AllDirectories).Where(path => !IsExcludedPath(path)))
        {
            var text = File.ReadAllText(file);
            foreach (Match match in SqlProcedurePattern().Matches(text))
            {
                var name = NormalizeSqlIdentifier(match.Groups["name"].Value);
                var definition = match.Groups["definition"].Value;
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(definition))
                    continue;

                var tablesWritten = ExtractSqlTables(definition, @"\b(?:insert\s+into|update|delete\s+from)\s+(?<table>(?:\[[^\]]+\]|[A-Za-z_#][A-Za-z0-9_#]*)(?:\s*\.\s*(?:\[[^\]]+\]|[A-Za-z_#][A-Za-z0-9_#]*))?)")
                    .Where(table => !table.StartsWith("#", StringComparison.Ordinal))
                    .ToArray();
                var sideEffects = ExtractSqlSideEffects(definition, tablesWritten);

                yield return new SqlProcedureSnapshot(
                    name,
                    file,
                    ExtractSqlParameters(definition),
                    ExtractSqlTables(definition, @"\b(?:from|join)\s+(?<table>(?:\[[^\]]+\]|[A-Za-z_#][A-Za-z0-9_#]*)(?:\s*\.\s*(?:\[[^\]]+\]|[A-Za-z_#][A-Za-z0-9_#]*))?)")
                        .Where(table => !table.StartsWith("#", StringComparison.Ordinal))
                        .ToArray(),
                    tablesWritten,
                    ExtractSqlResultColumns(definition),
                    InferSqlTransactionBehavior(definition, tablesWritten, sideEffects),
                    sideEffects);
            }
        }
    }

    private static StoredProcedureCall? ExtractStoredProcedureCall(ExtractedMethod method)
    {
        var commandMatch = SqlQueryCommandPattern().Match(method.Body);
        if (!commandMatch.Success)
            return null;

        var command = commandMatch.Groups["command"].Value;
        var procedureMatch = SqlProcedureNameInCommandPattern().Match(command);
        if (!procedureMatch.Success)
            return null;

        var procedureName = NormalizeSqlIdentifier(procedureMatch.Groups["name"].Value);
        var bindings = SqlParameterConstructionPattern()
            .Matches(method.Body)
            .Select(match => new SqlParameterBinding(
                match.Groups["param"].Value,
                match.Groups["source"].Value.Trim()))
            .ToArray();

        return new StoredProcedureCall(procedureName, bindings);
    }

    private static SqlProcedureSnapshot? FindSqlProcedure(
        IReadOnlyList<SqlProcedureSnapshot> procedures,
        string procedureName)
    {
        if (string.IsNullOrWhiteSpace(procedureName))
            return null;

        var normalized = NormalizeSqlIdentifier(procedureName);
        return procedures.FirstOrDefault(procedure => string.Equals(procedure.Name, normalized, StringComparison.OrdinalIgnoreCase)) ??
               procedures.FirstOrDefault(procedure => procedure.Name.EndsWith($".{normalized.Split('.').Last()}", StringComparison.OrdinalIgnoreCase));
    }

    private static string[] MergeSqlParameters(StoredProcedureCall? call, SqlProcedureSnapshot? procedure)
    {
        if (call is null && procedure is null)
            return [];

        var bindings = call?.ParameterBindings.ToDictionary(binding => binding.ParameterName, binding => binding.SourceExpression, StringComparer.OrdinalIgnoreCase)
            ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (procedure is not null && procedure.Parameters.Length > 0)
        {
            return procedure.Parameters
                .Select(parameter =>
                {
                    var name = parameter.Split(':')[0];
                    return bindings.TryGetValue(name, out var source)
                        ? $"{parameter} <- {source}"
                        : parameter;
                })
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        return call!.ParameterBindings
            .Select(binding => $"{binding.ParameterName} <- {binding.SourceExpression}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] ExtractSqlParameters(string definition)
    {
        var asMatch = Regex.Match(definition, @"\bAS\b", RegexOptions.IgnoreCase);
        var header = asMatch.Success ? definition[..asMatch.Index] : definition;
        return SqlParameterDefinitionPattern()
            .Matches(header)
            .Select(match => $"{match.Groups["param"].Value}:{NormalizeSqlType(match.Groups["type"].Value)}")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] ExtractSqlTables(string definition, string pattern)
        => Regex.Matches(definition, pattern, RegexOptions.IgnoreCase)
            .Select(match => NormalizeSqlIdentifier(match.Groups["table"].Value))
            .Where(table => !string.IsNullOrWhiteSpace(table) &&
                            !table.Equals("select", StringComparison.OrdinalIgnoreCase) &&
                            !table.Equals("where", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string[] ExtractSqlResultColumns(string definition)
    {
        var matches = Regex.Matches(definition, @"\bselect\s+(?<columns>.*?)\s+\bfrom\b", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (matches.Count == 0)
            return [];

        return matches
            .SelectMany(match => SplitSqlColumns(match.Groups["columns"].Value))
            .Select(ExtractSqlColumnName)
            .Where(column => !string.IsNullOrWhiteSpace(column) && column != "*")
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] SplitSqlColumns(string columns)
    {
        var parts = new List<string>();
        var start = 0;
        var depth = 0;
        var inString = false;
        for (var i = 0; i < columns.Length; i++)
        {
            var ch = columns[i];
            if (ch == '\'')
                inString = !inString;
            else if (!inString && ch == '(')
                depth++;
            else if (!inString && ch == ')' && depth > 0)
                depth--;
            else if (!inString && depth == 0 && ch == ',')
            {
                parts.Add(columns[start..i].Trim());
                start = i + 1;
            }
        }

        parts.Add(columns[start..].Trim());
        return parts.Where(part => !string.IsNullOrWhiteSpace(part)).ToArray();
    }

    private static string ExtractSqlColumnName(string expression)
    {
        var alias = Regex.Match(expression, @"\bas\s+(?<alias>\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*)\s*$", RegexOptions.IgnoreCase);
        if (alias.Success)
            return NormalizeSqlIdentifier(alias.Groups["alias"].Value);

        var trimmed = expression.Trim();
        if (trimmed.Contains('(') || trimmed.Contains('\''))
            return "";

        var last = trimmed.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).LastOrDefault() ?? "";
        return NormalizeSqlIdentifier(last);
    }

    private static string[] ExtractSqlSideEffects(string definition, string[] tablesWritten)
    {
        var sideEffects = new List<string>();
        foreach (var table in tablesWritten)
            sideEffects.Add($"writes {table}");

        foreach (Match match in Regex.Matches(definition, @"(?:into|table)\s+(?<temp>#[A-Za-z_][A-Za-z0-9_]*)", RegexOptions.IgnoreCase))
            sideEffects.Add($"uses temp table {match.Groups["temp"].Value}");

        if (Regex.IsMatch(definition, @"\bexec\s*\(", RegexOptions.IgnoreCase))
            sideEffects.Add("executes dynamic SQL");
        if (Regex.IsMatch(definition, @"\bcursor\b", RegexOptions.IgnoreCase))
            sideEffects.Add("uses cursor");

        return sideEffects.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static string InferSqlTransactionBehavior(string definition, string[] tablesWritten, string[] sideEffects)
    {
        if (Regex.IsMatch(definition, @"\bbegin\s+transaction\b", RegexOptions.IgnoreCase))
            return "explicit transaction in procedure body";

        if (tablesWritten.Length == 0)
            return sideEffects.Length == 0
                ? "no explicit transaction; read-only result procedure"
                : "no explicit transaction; read/query procedure with local temp or dynamic processing";

        return $"no explicit transaction detected; writes {string.Join(", ", tablesWritten)}";
    }

    private static string NormalizeSqlIdentifier(string value)
        => Regex.Replace(value, @"[\[\]\s]+", "").Trim();

    private static string NormalizeSqlType(string value)
        => Regex.Replace(value, @"\s+", "").Trim();

    private static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? "";

    private static string FormatInline(string[] values)
        => values.Length == 0 ? "none recorded" : string.Join(", ", values);

    private static void WriteWikiDomain(
        string projectRoot,
        string wikiRoot,
        string projectId,
        string version,
        ProjectMetadata metadata,
        ProjectDocumentationKnowledgeGraph graph)
    {
        var docDir = Path.Combine(wikiRoot, "documentation");
        var evidenceDir = Path.Combine(wikiRoot, "evidence");
        var relationsDir = Path.Combine(wikiRoot, "relations");
        var evalDir = Path.Combine(wikiRoot, "eval");
        var trainingDir = Path.Combine(wikiRoot, "training", "batch-001");
        Directory.CreateDirectory(docDir);
        Directory.CreateDirectory(evidenceDir);
        Directory.CreateDirectory(relationsDir);
        Directory.CreateDirectory(evalDir);
        Directory.CreateDirectory(trainingDir);

        foreach (var file in Directory.EnumerateFiles(docDir, "*.md"))
            File.Delete(file);

        foreach (var unit in graph.DocumentationUnits)
            File.WriteAllText(Path.Combine(docDir, DocumentationFileName(unit)), BuildMarkdown(unit), Encoding.UTF8);

        WriteJson(Path.Combine(wikiRoot, "domain-manifest.json"), new
        {
            project_id = projectId,
            repository_root = projectRoot,
            documentation_version = version,
            method_count = graph.DocumentationUnits.Count,
            call_edge_count = graph.CallEdges.Count,
            value_flow_edge_count = graph.ValueFlows.Count,
            negative_dpo_pair_count = graph.NegativeDpo.DpoPairs.Count,
            negative_dto_acceptance_rate = graph.NegativeDpo.Candidates.Count == 0 ? 0 : graph.NegativeDpo.Validations.Count(validation => validation.IsAccepted) / (double)graph.NegativeDpo.Candidates.Count,
            runtime_family = metadata.RuntimeFamily,
            target_framework = metadata.TargetFramework,
            csharp_language_version = metadata.CSharpLanguageVersion,
            project_style = metadata.ProjectStyle,
            question_database = "question_seeds_generated"
        });
        WriteJson(Path.Combine(wikiRoot, "model-manifest.json"), new
        {
            unit_id = $"wiki-{projectId}-phase1",
            model_status = "documentation_ready_training_pending",
            base_model_id = "xiigen-small-doc-wiki-base",
            weight_artifact_ref = "not_trained_yet",
            local_cache_path = ".xiigen/wiki-domain/weights/",
            documentation_version = version
        });
        WriteJsonl(Path.Combine(evidenceDir, "evidence-index.jsonl"), graph.EvidenceRecords);
        WriteJson(Path.Combine(relationsDir, "code-trace.json"), graph.CodeTrace);
        WriteJsonl(Path.Combine(relationsDir, "documentation-units.jsonl"), graph.DocumentationUnits);
        WriteJsonl(Path.Combine(relationsDir, "call-edges.jsonl"), graph.CallEdges);
        WriteJsonl(Path.Combine(relationsDir, "value-flow-edges.jsonl"), graph.ValueFlows);
        WriteJsonl(Path.Combine(trainingDir, "negative-dto-candidates.jsonl"), graph.NegativeDpo.Candidates);
        WriteJsonl(Path.Combine(trainingDir, "documentation-dpo-pairs.jsonl"), graph.NegativeDpo.DpoPairs);
        WriteJsonl(Path.Combine(evalDir, "negative-dto-validation-ledger.jsonl"), graph.NegativeDpo.Validations);
    }

    private static void WriteQuestionSeeds(string wikiRoot, IReadOnlyList<ProjectWikiQuestionSeed> questionSeeds)
    {
        var questionDir = Path.Combine(wikiRoot, "questions");
        Directory.CreateDirectory(questionDir);
        WriteJsonl(Path.Combine(questionDir, "question-seeds.jsonl"), questionSeeds);
    }

    private static string BuildMarkdown(ProjectDocumentationUnit unit)
        => new DocumentationGeneratorUnit().Generate(unit).Value?.Text ?? $"# {unit.SymbolName}";

    private static void WriteJson(string path, object value)
        => File.WriteAllText(path, JsonSerializer.Serialize(value, JsonOptions), Encoding.UTF8);

    private static void WriteJsonl(string path, IEnumerable<object> values)
        => File.WriteAllLines(path, values.Select(value => JsonSerializer.Serialize(value, JsonOptions)), Encoding.UTF8);

    private static bool IsExcludedPath(string path)
        => path.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
           path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
           path.Contains($"{Path.DirectorySeparatorChar}packages{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
           path.Contains($"{Path.DirectorySeparatorChar}node_modules{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
           path.Contains($"{Path.DirectorySeparatorChar}dist{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) ||
           path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) ||
           path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase);

    private static int CountIncludedSourceFiles(string projectRoot)
        => Directory.EnumerateFiles(projectRoot, "*.*", SearchOption.AllDirectories)
            .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".sql", StringComparison.OrdinalIgnoreCase))
            .Count(path => !IsExcludedPath(path));

    private static string UnitId(ExtractedMethod method)
        => $"{method.ProjectId}::{method.RelativePath}:{method.Line}::{method.Symbol}";

    private static string UnitId(ExtractedClientArtifact artifact)
        => $"{artifact.ProjectId}::{artifact.RelativePath}:{artifact.Line}::{artifact.Symbol}";

    private static string DocumentationFileName(ProjectDocumentationUnit unit)
    {
        var readable = Regex.Replace($"{unit.ProjectId}_{Path.GetFileNameWithoutExtension(unit.SourcePath)}_{unit.SourceLine}", @"[\\/:*?""<>|\s]+", "_");
        var source = $"{unit.ProjectId}|{unit.SourcePath}|{unit.SourceLine}|{unit.SymbolName}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source)))[..16].ToLowerInvariant();
        return $"{readable}_{hash}.md";
    }

    private static int FindMatchingBrace(string text, int openBrace)
    {
        var depth = 0;
        for (var i = openBrace; i < text.Length; i++)
        {
            if (text[i] == '{')
                depth++;
            else if (text[i] == '}')
            {
                depth--;
                if (depth == 0)
                    return i;
            }
        }

        return -1;
    }

    private static string FindNearestClassName(string text, int methodIndex)
    {
        var prefix = text[..methodIndex];
        var matches = ClassPattern().Matches(prefix);
        return matches.Count == 0 ? "" : matches[^1].Groups["name"].Value;
    }

    private static string[] SplitParameters(string parameters)
        => parameters.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(parameter => Regex.Replace(parameter, @"\s*=\s*.*$", ""))
            .Where(parameter => !string.IsNullOrWhiteSpace(parameter))
            .ToArray();

    private static string InferLegacyLanguage(string target)
        => target.Contains("3.5", StringComparison.OrdinalIgnoreCase) || target.Equals("net35", StringComparison.OrdinalIgnoreCase)
            ? "CSharp3"
            : "CSharp5_or_CSharp6_review_required";

    private static string InferMeaning(ExtractedMethod method, string[] callees, string[] callers)
    {
        var callText = callees.Length == 0
            ? "It has no detected internal callee."
            : $"It delegates to {string.Join(", ", callees)}.";
        var callerText = callers.Length == 0
            ? "No internal caller was detected."
            : $"It is called by {string.Join(", ", callers)}.";

        return $"{method.Symbol} implements local project behavior with return type {method.ReturnType}. {callText} {callerText}";
    }

    private static string FindCallExpression(string body, string methodName)
    {
        var match = Regex.Match(body, $@"(?<expr>(?:return\s+)?(?:[A-Za-z_][A-Za-z0-9_]*\s*=\s*)?(?:[A-Za-z_][A-Za-z0-9_]*\.)?{Regex.Escape(methodName)}\s*\((?<args>[^;]*)\))");
        return match.Success ? match.Groups["expr"].Value.Trim() : "";
    }

    private static string[] ExtractArguments(string callExpression)
    {
        var match = Regex.Match(callExpression, @"\((?<args>.*)\)");
        return match.Success
            ? match.Groups["args"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];
    }

    private static string[] ExtractAngularDecorators(string text, int classIndex)
    {
        var prefixStart = Math.Max(0, classIndex - 500);
        var prefix = text[prefixStart..classIndex];
        return Regex.Matches(prefix, @"@(?<name>Component|Injectable|Directive|Pipe|NgModule)\b", RegexOptions.IgnoreCase)
            .Select(match => match.Groups["name"].Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string[] ExtractHttpCalls(string text)
        => Regex.Matches(text, @"\b(?:this\.)?http\.(?<verb>get|post|put|patch|delete)\s*\(\s*(?<route>[^,\)]+)", RegexOptions.IgnoreCase)
            .Select(match => $"{match.Groups["verb"].Value}:{CleanClientLiteral(match.Groups["route"].Value)}")
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string[] ExtractRouteStrings(string text)
        => Regex.Matches(text, @"['""](?<route>\/?(?:api|admin|client|game|task|user|auth|workflow)[^'""]*)['""]", RegexOptions.IgnoreCase)
            .Select(match => CleanClientLiteral(match.Groups["route"].Value))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToArray();

    private static string[] ExtractTemplateBindings(string text)
        => Regex.Matches(text, @"(?:\[(?<property>[A-Za-z0-9_-]+)\]|\((?<event>[A-Za-z0-9_-]+)\)|\{\{\s*(?<interpolation>[^}]+)\s*\}\})", RegexOptions.IgnoreCase)
            .Select(match => FirstNonEmpty(match.Groups["property"].Value, match.Groups["event"].Value, match.Groups["interpolation"].Value).Trim())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(30)
            .ToArray();

    private static string CleanClientLiteral(string value)
        => value.Trim().Trim('\'', '"', '`', ' ', '\t', '\r', '\n');

    private static string MatchValue(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["v"].Value.Trim() : "";
    }

    private static bool IsControlKeyword(string name)
        => name is "if" or "for" or "foreach" or "while" or "switch" or "catch" or "using" or "lock";

    [GeneratedRegex(@"(?<return>[A-Za-z_][A-Za-z0-9_<>\[\],.?]*)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\((?<parameters>[^)]*)\)\s*(?:where\s+[^{]+)?\{", RegexOptions.Multiline)]
    private static partial Regex MethodPattern();

    [GeneratedRegex(@"\b(class|struct|record)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)")]
    private static partial Regex ClassPattern();

    [GeneratedRegex(@"\b(?:export\s+)?class\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)")]
    private static partial Regex ClientClassPattern();

    [GeneratedRegex(@"^\s*CREATE\s+PROCEDURE\s+(?<name>(?:\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*)(?:\s*\.\s*(?:\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*))?)\s*(?<definition>.*?)(?=^\s*GO\s*$|\z)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline)]
    private static partial Regex SqlProcedurePattern();

    [GeneratedRegex(@"\.SqlQuery(?:<[^>]+>)?\s*\(\s*""(?<command>[^""]+)""", RegexOptions.IgnoreCase)]
    private static partial Regex SqlQueryCommandPattern();

    [GeneratedRegex(@"(?<name>(?:\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*)(?:\s*\.\s*(?:\[[^\]]+\]|[A-Za-z_][A-Za-z0-9_]*))?)", RegexOptions.IgnoreCase)]
    private static partial Regex SqlProcedureNameInCommandPattern();

    [GeneratedRegex(@"(?<param>@[A-Za-z_][A-Za-z0-9_]*)\s+(?<type>[A-Za-z_][A-Za-z0-9_]*(?:\s*\(\s*(?:max|\d+)(?:\s*,\s*\d+)?\s*\))?)", RegexOptions.IgnoreCase)]
    private static partial Regex SqlParameterDefinitionPattern();

    [GeneratedRegex(@"(?<var>[A-Za-z_][A-Za-z0-9_]*)\s*=\s*new\s+SqlParameter\s*\(\s*""(?<param>@[^""]+)""\s*,\s*(?<source>[^);]+)\)", RegexOptions.IgnoreCase)]
    private static partial Regex SqlParameterConstructionPattern();

    private sealed record ProjectMetadata(
        string RuntimeFamily,
        string CSharpLanguageVersion,
        string TargetFramework,
        string ProjectStyle);

    private sealed record ExtractedMethod(
        string ProjectId,
        string FilePath,
        string RelativePath,
        int Line,
        string Symbol,
        string Name,
        string ClassName,
        string ReturnType,
        string[] Parameters,
        string Body,
        ProjectMetadata Metadata);

    private sealed record ExtractedClientArtifact(
        string ProjectId,
        string FilePath,
        string RelativePath,
        int Line,
        string Symbol,
        string Name,
        string ArtifactKind,
        string[] Parameters,
        ProjectMetadata Metadata,
        string Meaning)
    {
        public string ReturnValue { get; } = $"{ArtifactKind} evidence for {Symbol}";
    }

    private sealed record SqlProcedureSnapshot(
        string Name,
        string FilePath,
        string[] Parameters,
        string[] TablesRead,
        string[] TablesWritten,
        string[] ResultColumns,
        string TransactionBehavior,
        string[] SideEffects);

    private sealed record StoredProcedureCall(
        string ProcedureName,
        SqlParameterBinding[] ParameterBindings);

    private sealed record SqlParameterBinding(
        string ParameterName,
        string SourceExpression);
}
