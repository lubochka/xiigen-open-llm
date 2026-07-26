// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.RegularExpressions;
using AgentInfrastructure;

public sealed class StackAndVersionClassifierUnit
{
    public const string UnitId = "stack-and-version-classifier-unit";
    public const string ScenarioId = "project-wiki-stack-version-classification";

    public OperationResult<StackAndVersionClassification> Classify(string csprojText)
    {
        if (string.IsNullOrWhiteSpace(csprojText))
            return OperationResult<StackAndVersionClassification>.Fail("Project file text is required.", "CSPROJ_TEXT_REQUIRED");

        var target = MatchValue(csprojText, @"<TargetFramework>(?<v>[^<]+)</TargetFramework>");
        if (string.IsNullOrWhiteSpace(target))
            target = MatchValue(csprojText, @"<TargetFrameworkVersion>(?<v>[^<]+)</TargetFrameworkVersion>");
        if (string.IsNullOrWhiteSpace(target))
            target = "unknown";

        var runtime = target.StartsWith("netcoreapp", StringComparison.OrdinalIgnoreCase) ||
                      Regex.IsMatch(target, @"^net[5-9]", RegexOptions.IgnoreCase)
            ? "NET_CORE"
            : target.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase)
                ? "NET_STANDARD"
                : target == "unknown" ? "unknown" : "NET_FRAMEWORK";
        var language = MatchValue(csprojText, @"<LangVersion>(?<v>[^<]+)</LangVersion>");
        if (string.IsNullOrWhiteSpace(language))
            language = runtime == "NET_CORE" ? "CSharp8_or_newer_inferred" : InferLegacyLanguage(target);
        else
            language = $"explicit:{language}";
        var style = csprojText.Contains("<Project Sdk=", StringComparison.OrdinalIgnoreCase)
            ? "sdk_style"
            : "legacy_non_sdk";
        var warnings = target == "unknown" ? ["target_framework_unknown"] : Array.Empty<string>();
        return OperationResult<StackAndVersionClassification>.Ok(new StackAndVersionClassification(
            runtime,
            target,
            language,
            style,
            warnings.Length == 0 ? 0.94 : 0.62,
            BuildMarkers(target, language, style),
            warnings));
    }

    public OperationResult<StackAndVersionClassification> ClassifyCrossStack(
        string clientPackageJsonText,
        string serverProjectText,
        string databaseEvidenceText)
    {
        var server = Classify(serverProjectText);
        if (!server.IsSuccess)
            return server;

        var serverClassification = server.Value!;
        var clientFramework = DetectClientFramework(clientPackageJsonText);
        var clientVersion = DetectClientFrameworkVersion(clientPackageJsonText, clientFramework);
        var clientLanguage = DetectClientLanguage(clientPackageJsonText, clientFramework);
        var databaseLanguage = string.IsNullOrWhiteSpace(databaseEvidenceText) ? "" : "SQL";
        var databaseDialect = DetectDatabaseDialect(databaseEvidenceText);
        var warnings = serverClassification.Warnings
            .Concat(BuildCrossStackWarnings(clientPackageJsonText, serverProjectText, databaseEvidenceText))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var markers = serverClassification.EvidenceMarkers
            .Concat([clientFramework, clientVersion, clientLanguage, databaseLanguage, databaseDialect])
            .Where(marker => !string.IsNullOrWhiteSpace(marker))
            .ToArray();
        var confidence = warnings.Length == 0 ? Math.Min(0.96, serverClassification.Confidence + 0.02) : Math.Min(serverClassification.Confidence, 0.74);
        return OperationResult<StackAndVersionClassification>.Ok(serverClassification with
        {
            Confidence = confidence,
            EvidenceMarkers = markers,
            Warnings = warnings,
            ClientLanguage = clientLanguage,
            ClientFramework = clientFramework,
            ClientFrameworkVersion = clientVersion,
            ServerLanguage = "C#",
            ServerFramework = serverClassification.RuntimeFamily,
            ServerFrameworkVersion = serverClassification.TargetFramework,
            DatabaseLanguage = databaseLanguage,
            DatabaseDialect = databaseDialect
        });
    }

    private static string InferLegacyLanguage(string target)
        => target.Contains("3.5", StringComparison.OrdinalIgnoreCase) || target.Equals("net35", StringComparison.OrdinalIgnoreCase)
            ? "CSharp3"
            : "CSharp5_or_CSharp6_review_required";

    private static string MatchValue(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
        return match.Success ? match.Groups["v"].Value.Trim() : "";
    }

    private static string[] BuildMarkers(string target, string language, string style)
        => [target, language, style];

    private static string DetectClientFramework(string packageJsonText)
    {
        if (string.IsNullOrWhiteSpace(packageJsonText))
            return "";
        if (packageJsonText.Contains("\"@angular/core\"", StringComparison.OrdinalIgnoreCase))
            return "Angular";
        if (packageJsonText.Contains("\"react\"", StringComparison.OrdinalIgnoreCase))
            return "React";
        return "unknown_client_framework";
    }

    private static string DetectClientFrameworkVersion(string packageJsonText, string framework)
    {
        if (string.IsNullOrWhiteSpace(packageJsonText) || string.IsNullOrWhiteSpace(framework))
            return "";
        var key = framework.Equals("Angular", StringComparison.OrdinalIgnoreCase) ? "@angular/core" : framework.ToLowerInvariant();
        return MatchJsonStringProperty(packageJsonText, key);
    }

    private static string DetectClientLanguage(string packageJsonText, string framework)
    {
        if (string.IsNullOrWhiteSpace(packageJsonText))
            return "";
        var typeScript = MatchJsonStringProperty(packageJsonText, "typescript");
        if (!string.IsNullOrWhiteSpace(typeScript))
            return $"TypeScript {typeScript}";
        return framework.Equals("React", StringComparison.OrdinalIgnoreCase) ? "JavaScript_or_TypeScript_review_required" : "unknown_client_language";
    }

    private static string DetectDatabaseDialect(string databaseEvidenceText)
    {
        if (string.IsNullOrWhiteSpace(databaseEvidenceText))
            return "";
        if (Regex.IsMatch(databaseEvidenceText, @"CREATE\s+PROCEDURE|ALTER\s+PROCEDURE|\[[^\]]+\]\.\[[^\]]+\]", RegexOptions.IgnoreCase))
            return "T-SQL";
        return "SQL_dialect_review_required";
    }

    private static string MatchJsonStringProperty(string text, string propertyName)
    {
        var escaped = Regex.Escape(propertyName);
        return MatchValue(text, $"\"{escaped}\"\\s*:\\s*\"(?<v>[^\"]+)\"");
    }

    private static string[] BuildCrossStackWarnings(string clientPackageJsonText, string serverProjectText, string databaseEvidenceText)
    {
        var warnings = new List<string>();
        if (string.IsNullOrWhiteSpace(clientPackageJsonText))
            warnings.Add("client_package_json_missing");
        if (string.IsNullOrWhiteSpace(serverProjectText))
            warnings.Add("server_project_text_missing");
        if (string.IsNullOrWhiteSpace(databaseEvidenceText))
            warnings.Add("database_evidence_missing");
        return warnings.ToArray();
    }
}
