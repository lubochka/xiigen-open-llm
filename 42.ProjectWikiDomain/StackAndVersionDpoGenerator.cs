// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using AgentInfrastructure;

public sealed class StackAndVersionDpoGenerator
{
    private readonly StackAndVersionClassifierUnit _classifier;

    public StackAndVersionDpoGenerator(StackAndVersionClassifierUnit? classifier = null)
    {
        _classifier = classifier ?? new StackAndVersionClassifierUnit();
    }

    public Task<OperationResult<StackAndVersionDpoBatch>> BuildBatchAsync(
        string batchId = "stack-version-classifier-batch-001",
        bool heldOut = false,
        CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
            return Task.FromResult(OperationResult<StackAndVersionDpoBatch>.Fail("Cancelled.", "CANCELLED"));

        var seeds = BuildSeeds(heldOut);
        var pairs = seeds.Select((seed, index) =>
        {
            var classified = _classifier.Classify(seed.CsprojText).Value!;
            return new StackAndVersionDpoPair(
                $"{batchId}-pair-{index + 1:000}",
                seed.ProjectType,
                seed.CsprojText,
                classified.RuntimeFamily,
                classified.TargetFramework,
                classified.CSharpLanguageVersion,
                classified.ProjectStyle,
                seed.CsprojText,
                BuildChosen(classified),
                BuildRejected(seed.RejectedType, classified),
                seed.RejectedType);
        }).ToArray();
        var wrongAnnotationRate = pairs.Count(pair => pair.Rejected.Contains("[WRONG:", StringComparison.Ordinal)) / (double)pairs.Length;
        var warnings = new List<string>();
        if (pairs.Length != 50)
            warnings.Add("batch_row_count_not_50");
        if (wrongAnnotationRate < 1.0)
            warnings.Add("wrong_annotation_rate_below_gate");

        return Task.FromResult(OperationResult<StackAndVersionDpoBatch>.Ok(new StackAndVersionDpoBatch(
            batchId,
            pairs.Length,
            wrongAnnotationRate,
            pairs,
            warnings.ToArray())));
    }

    private static string BuildChosen(StackAndVersionClassification classification)
        => $"runtime_family: {classification.RuntimeFamily}\ntarget_framework: {classification.TargetFramework}\ncsharp_language_version: {classification.CSharpLanguageVersion}\nproject_style: {classification.ProjectStyle}\nevidence_markers: {string.Join(", ", classification.EvidenceMarkers)}";

    private static string BuildRejected(string rejectedType, StackAndVersionClassification classification)
        => rejectedType switch
        {
            "core_as_framework" =>
                $"runtime_family: NET_FRAMEWORK\ntarget_framework: {classification.TargetFramework}\n[WRONG: core_as_framework maps SDK-style or net5+ projects to legacy .NET Framework, causing the documentation pipeline to infer wrong APIs and language features.]",
            "framework_as_core" =>
                $"runtime_family: NET_CORE\ntarget_framework: {classification.TargetFramework}\n[WRONG: framework_as_core treats legacy .NET Framework projects as modern .NET, hiding WebForms/MVC-era constraints and wrong C# defaults.]",
            "missing_language_version" =>
                $"runtime_family: {classification.RuntimeFamily}\ntarget_framework: {classification.TargetFramework}\ncsharp_language_version: unknown\n[WRONG: missing_language_version prevents documentation from explaining which syntax and framework-era behavior is valid.]",
            "wrong_project_style" =>
                $"runtime_family: {classification.RuntimeFamily}\nproject_style: {(classification.ProjectStyle == "sdk_style" ? "legacy_non_sdk" : "sdk_style")}\n[WRONG: wrong_project_style confuses SDK-style .NET projects with legacy non-SDK project files and breaks stack-specific documentation routing.]",
            _ =>
                $"target_framework: unknown\n[WRONG: underspecified_stack_version omits runtime, language version, and project style needed for safe documentation.]"
        };

    private static StackSeed[] BuildSeeds(bool heldOut)
    {
        string[] targetFrameworks = heldOut
            ? new[] { "v3.5", "v4.0", "v4.5.1", "v4.7.2", "net6.0", "net7.0", "net8.0", "netstandard2.0", "netcoreapp3.1", "net9.0" }
            : new[] { "v3.5", "v4.5", "v4.5.2", "v4.6.1", "v4.8", "net5.0", "net6.0", "net8.0", "netstandard2.1", "netcoreapp2.1" };
        var rows = new List<StackSeed>(50);
        for (var index = 0; index < 5; index++)
        {
            foreach (var target in targetFrameworks)
            {
                var sdk = target.StartsWith("net", StringComparison.OrdinalIgnoreCase) && !target.StartsWith("netstandard", StringComparison.OrdinalIgnoreCase);
                var explicitLang = index % 3 == 0 ? "<LangVersion>latest</LangVersion>" : "";
                var csproj = sdk
                    ? $"<Project Sdk=\"Microsoft.NET.Sdk.Web\"><PropertyGroup><TargetFramework>{target}</TargetFramework>{explicitLang}</PropertyGroup></Project>"
                    : $"<Project ToolsVersion=\"15.0\" xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup><TargetFrameworkVersion>{target}</TargetFrameworkVersion>{explicitLang}</PropertyGroup></Project>";
                var rejected = sdk ? "core_as_framework" : "framework_as_core";
                if (index % 3 == 0)
                    rejected = "missing_language_version";
                if (index % 5 == 0)
                    rejected = "wrong_project_style";
                rows.Add(new StackSeed($"stack_{target}_{index}", csproj, rejected));
            }
        }

        return rows.Take(50).ToArray();
    }

    private sealed record StackSeed(
        string ProjectType,
        string CsprojText,
        string RejectedType);
}
