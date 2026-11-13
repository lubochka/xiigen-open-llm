// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.RegularExpressions;
using AgentInfrastructure;

public sealed class DocumentationGeneratorUnit
{
    public const string UnitId = "documentation-generator-unit";
    public const string ScenarioId = "project-wiki-documentation-generation";

    public OperationResult<DocumentationGeneratorOutput> Generate(ProjectDocumentationUnit unit)
    {
        if (unit is null)
            return OperationResult<DocumentationGeneratorOutput>.Fail("Documentation unit is required.", "DOCUMENTATION_UNIT_REQUIRED");

        var text = IsStoredProcedureUnit(unit)
            ? GenerateStoredProcedureDocumentation(unit)
            : GenerateGenericDocumentation(unit);
        var flags = new[]
        {
            !string.IsNullOrWhiteSpace(unit.Meaning),
            !string.IsNullOrWhiteSpace(unit.SourcePath) && unit.SourceLine > 0,
            unit.Parameters.Length >= 0,
            !string.IsNullOrWhiteSpace(unit.ReturnValue),
            unit.Callers.Length >= 0,
            unit.Callees.Length >= 0,
            !string.IsNullOrWhiteSpace(unit.TargetFramework) && !string.IsNullOrWhiteSpace(unit.CSharpLanguageVersion),
            unit.EvidenceIds.Length > 0
        };
        var warnings = flags.All(flag => flag) ? Array.Empty<string>() : ["documentation_required_field_missing"];

        return OperationResult<DocumentationGeneratorOutput>.Ok(new DocumentationGeneratorOutput(
            unit.DocumentationUnitId,
            text,
            flags[0],
            flags[1],
            flags[2],
            flags[3],
            flags[4],
            flags[5],
            flags[6],
            flags[7],
            flags.Count(flag => flag) / (double)flags.Length,
            warnings));
    }

    private static string FormatList(string[] items)
        => items.Length == 0 ? "- none recorded" : string.Join(Environment.NewLine, items.Select(item => $"- {item}"));

    private static bool IsStoredProcedureUnit(ProjectDocumentationUnit unit)
        => unit.SourcePath.EndsWith(".sql", StringComparison.OrdinalIgnoreCase) ||
           unit.Parameters.Any(parameter => parameter.StartsWith("@", StringComparison.Ordinal));

    private static string GenerateGenericDocumentation(ProjectDocumentationUnit unit)
        => $"""
           # {unit.SymbolName}

           ## Meaning
           {unit.Meaning}

           ## Location
           {unit.SourcePath}:{unit.SourceLine}

           ## Parameters
           {FormatList(unit.Parameters)}

           ## Return
           {unit.ReturnValue}

           ## Called By
           {FormatList(unit.Callers)}

           ## Calls
           {FormatList(unit.Callees)}

           ## Stack And Version
           Target framework: {unit.TargetFramework}
           C# language version: {unit.CSharpLanguageVersion}

           ## Evidence
           {FormatList(unit.EvidenceIds)}
           """;

    private static string GenerateStoredProcedureDocumentation(ProjectDocumentationUnit unit)
        => $"""
           # {unit.SymbolName}

           ## Stored Procedure Identity
           {unit.SymbolName}

           ## SQL Parameters
           {FormatList(unit.Parameters)}

           ## SQL Result Shape
           {unit.ReturnValue}

           ## SQL Side Effects
           {BuildSqlSideEffectSummary(unit)}

           ## .NET Callers
           {FormatList(unit.Callers)}

           ## Meaning
           {unit.Meaning}

           ## Location
           {unit.SourcePath}:{unit.SourceLine}

           ## Calls
           {FormatList(unit.Callees)}

           ## Stack And Version
           Target framework: {unit.TargetFramework}
           C# language version: {unit.CSharpLanguageVersion}

           ## Evidence
           {FormatList(unit.EvidenceIds)}
           """;

    private static string BuildSqlSideEffectSummary(ProjectDocumentationUnit unit)
    {
        var meaning = unit.Meaning;
        var effects = new List<string>();
        var readMatch = Regex.Match(meaning, @"reads (?<tables>[^,]+(?: and [^,]+)?)", RegexOptions.IgnoreCase);
        var writeMatch = Regex.Match(meaning, @"writes (?<tables>[^,]+)(?<tail> when [^,]+)?", RegexOptions.IgnoreCase);
        var transactionMatch = Regex.Match(meaning, @"uses (?<behavior>[^.]+? transaction behavior[^.]*)", RegexOptions.IgnoreCase);
        if (readMatch.Success)
            effects.Add($"Reads: {readMatch.Groups["tables"].Value.Trim()}");
        if (writeMatch.Success)
            effects.Add($"Writes: {writeMatch.Groups["tables"].Value.Trim()}{writeMatch.Groups["tail"].Value.Trim()}");
        if (transactionMatch.Success)
            effects.Add($"Transaction: {transactionMatch.Groups["behavior"].Value.Trim()}");

        return effects.Count == 0
            ? "- no side effects recorded"
            : string.Join(Environment.NewLine, effects.Select(effect => $"- {effect}"));
    }
}
