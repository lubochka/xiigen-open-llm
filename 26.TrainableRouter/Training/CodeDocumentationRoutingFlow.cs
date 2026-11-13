// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace TrainableRouter.Training;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed class CodeDocumentationRoutingFlow : ILogicalFlow<CodeDocumentationRequest>
{
    public const string FlowId = "code-documentation-routing-flow";
    public const string ScenarioId = "legacy_dotnet_bottom_up_documentation";

    private static readonly string[] CandidateUnits =
    [
        OrchestrationRoutingCurriculum.OrchestrationV309UnitId,
        OrchestrationRoutingCurriculum.TranslationRuEnUnitId,
        OrchestrationRoutingCurriculum.DocumentationDotNetUnitId
    ];

    private readonly DistributedRoutingRuntime _runtime;

    public CodeDocumentationRoutingFlow(DistributedRoutingRuntime runtime)
    {
        _runtime = runtime;
    }

    public async Task<OperationResult<CodeDocumentationRoutingResult>> RouteDocumentationAsync(
        CodeDocumentationRequest request,
        CancellationToken ct = default)
    {
        if (request is null)
            return OperationResult<CodeDocumentationRoutingResult>.Fail("Code documentation request is required.", "CODE_DOC_REQUEST_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.CodeText))
            return OperationResult<CodeDocumentationRoutingResult>.Fail("Code text is required for documentation routing.", "CODE_TEXT_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.DocumentationLevel))
            return OperationResult<CodeDocumentationRoutingResult>.Fail("Documentation level is required.", "DOCUMENTATION_LEVEL_REQUIRED");

        var routingRequest = DescribeRoutingRequest(request); // The admission projection and normal routing consume the same complete request inputs.

        var routed = await DistributedRoutingBootstrap.RouteAndLocateAsync(_runtime, routingRequest, ct);
        if (!routed.IsSuccess)
            return routed.MapError<CodeDocumentationRoutingResult>();

        var sourceGate = EvaluateSourceGate(request);
        var versionGate = EvaluateVersionGate(request);
        var transitGate = EvaluateTransitGate(request);
        var stages = BuildStages(request, transitGate);
        var warnings = BuildWarnings(sourceGate, versionGate, transitGate);
        var verdict = routed.Value!.Decision.ChosenUnitId == OrchestrationRoutingCurriculum.DocumentationDotNetUnitId &&
                      sourceGate.IsPassed &&
                      versionGate.IsPassed &&
                      transitGate.IsPassed
            ? "DOCUMENTATION_ROUTE_READY"
            : "DOCUMENTATION_ROUTE_REVIEW_REQUIRED";

        var result = new CodeDocumentationRoutingResult(
            request.RequestId,
            routed.Value.Decision.ChosenUnitId,
            routed.Value.Manifest.TrainingRunId,
            routed.Value.Manifest.SourceDatasetSha256,
            request.AppSide,
            request.RuntimeFamily,
            request.TargetFramework,
            request.CSharpLanguageVersion,
            request.LanguageVersionConfidence,
            request.ProjectStyle,
            request.WebFramework,
            request.WebFrameworkVersion,
            request.FrontendFramework,
            request.FrontendFrameworkVersion,
            request.StackLabel,
            request.ArtifactKind,
            request.DocumentationLevel,
            stages,
            sourceGate,
            versionGate,
            transitGate,
            routed.Value.Decision.Confidence,
            verdict,
            warnings)
        {
            AuthorityRole = routed.Value.Decision.AuthorityRole
        };

        return OperationResult<CodeDocumentationRoutingResult>.Ok(result);
    }

    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<CodeDocumentationRequest> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Logical flow request is required.", "FLOW_REQUEST_REQUIRED");

        var routed = await RouteDocumentationAsync(request.Input, cancellationToken);
        if (!routed.IsSuccess)
            return routed.MapError<LogicalFlowResult>();

        var value = routed.Value!;
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["chosen_unit_id"] = value.ChosenUnitId,
            ["training_run_id"] = value.TrainingRunId,
            ["source_dataset_sha256"] = value.SourceDatasetSha256,
            ["app_side"] = value.AppSide,
            ["runtime_family"] = value.RuntimeFamily,
            ["target_framework"] = value.TargetFramework,
            ["csharp_language_version"] = value.CSharpLanguageVersion,
            ["language_version_confidence"] = value.LanguageVersionConfidence,
            ["project_style"] = value.ProjectStyle,
            ["web_framework"] = value.WebFramework,
            ["web_framework_version"] = value.WebFrameworkVersion,
            ["frontend_framework"] = value.FrontendFramework,
            ["frontend_framework_version"] = value.FrontendFrameworkVersion,
            ["stack_label"] = value.StackLabel,
            ["artifact_kind"] = value.ArtifactKind,
            ["documentation_level"] = value.DocumentationLevel,
            ["source_selection_accuracy"] = value.SourceGate.Score.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
            ["source_root_role"] = value.SourceGate.SourceRootRole,
            ["version_routing_accuracy"] = value.VersionGate.Score.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
            ["framework_lifecycle_accuracy"] = FrameworkLifecycleAccuracy(value).ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
            ["bottom_up_order_accuracy"] = value.TransitGate.BottomUpOrderScore.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
            ["callee_summary_transit_accuracy"] = value.TransitGate.CalleeTransitScore.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
            ["local_vs_delegated_accuracy"] = value.TransitGate.LocalVsDelegatedScore.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
            ["missing_callee_summary_count"] = value.TransitGate.MissingCalleeSummaryCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["wrong_version_count"] = value.VersionGate.WrongVersionCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["stage_tags"] = string.Join(";", value.Stages.Select(stage => stage.StageTag)),
            ["routing_confidence"] = value.RoutingConfidence.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture),
            [RoutingAuthorityVocabulary.MetadataKey] = value.AuthorityRole
        };

        var primaryScore = Math.Min(value.SourceGate.Score, Math.Min(value.VersionGate.Score, value.TransitGate.CalleeTransitScore));
        var summary = new LogicalFlowSummary(
            request.RunId,
            value.Stages.Length,
            0,
            primaryScore,
            "documentation_readiness",
            false,
            string.Equals(value.Verdict, "DOCUMENTATION_ROUTE_READY", StringComparison.Ordinal),
            false,
            value.Verdict,
            primaryScore,
            value.ChosenUnitId,
            $"Routed {value.StackLabel} {value.DocumentationLevel} documentation with {value.Stages.Length} bottom-up stages.",
            0,
            0);

        var finalHumanResult = string.Join(
            " | ",
            value.Stages.Select(stage => $"{stage.Order:00}:{stage.StageTag}:{stage.SymbolId}"));

        var flowResult = new LogicalFlowResult(
            request.FlowId,
            request.ScenarioId,
            value.Verdict,
            finalHumanResult,
            summary,
            metrics,
            [$"{value.RequestId}:code-doc-route:{value.ChosenUnitId}"],
            value.Warnings);

        return OperationResult<LogicalFlowResult>.Ok(flowResult);
    }

    internal static string[] DescribeCandidateUnits() => CandidateUnits.ToArray(); // Never expose the actual private static candidate vector.

    internal static RoutingRequest DescribeRoutingRequest(CodeDocumentationRequest request) // Project complete normal routing inputs without executing routing or gates.
        => new(request.RequestId, BuildPrompt(request), BuildTaskSlice(request), null, "documentation", // Project complete normal routing inputs without executing routing or gates.
            DescribeCandidateUnits(), BuildRoutingContext(request)); // Pure input projection contains no routing, scorer, locator or gate execution.

    private static string BuildPrompt(CodeDocumentationRequest request)
        => $"Document legacy C# code bottom-up for {request.StackLabel} {request.TargetFramework} {request.CSharpLanguageVersion}. Symbol: {request.SymbolId}. Level: {request.DocumentationLevel}.";

    private static string BuildTaskSlice(CodeDocumentationRequest request)
        => string.Join(
            " ",
            [
                "documentation",
                "legacy dotnet code documentation",
                "bottom-up callee caller propagation",
                request.AppSide,
                request.RuntimeFamily,
                request.TargetFramework,
                request.CSharpLanguageVersion,
                request.WebFramework,
                request.FrontendFramework,
                request.StackLabel,
                request.ArtifactKind,
                request.DocumentationLevel
            ]);

    private static IReadOnlyDictionary<string, string> BuildRoutingContext(CodeDocumentationRequest request)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["app_side"] = request.AppSide,
            ["runtime_family"] = request.RuntimeFamily,
            ["target_framework"] = request.TargetFramework,
            ["csharp_language_version"] = request.CSharpLanguageVersion,
            ["project_style"] = request.ProjectStyle,
            ["web_framework"] = request.WebFramework,
            ["frontend_framework"] = request.FrontendFramework,
            ["documentation_level"] = request.DocumentationLevel,
            ["stack_label"] = request.StackLabel
        };

    private static CodeDocumentationSourceGate EvaluateSourceGate(CodeDocumentationRequest request)
    {
        var excluded = DetectExcludedSourceReason(request);
        if (!string.IsNullOrWhiteSpace(excluded))
        {
            return new CodeDocumentationSourceGate(
                false,
                0.0,
                "excluded_generated_or_vendor",
                excluded,
                [$"source_root_excluded:{excluded}"]);
        }

        var role = ClassifySourceRootRole(request);
        return new CodeDocumentationSourceGate(
            true,
            1.0,
            role,
            "",
            [$"source_root_role:{role}"]);
    }

    private static string DetectExcludedSourceReason(CodeDocumentationRequest request)
    {
        var path = request.FilePath.Replace('/', '\\');
        if (path.Contains("\\node_modules\\", StringComparison.OrdinalIgnoreCase))
            return "node_modules_vendor_file";
        if (path.Contains("\\packages\\", StringComparison.OrdinalIgnoreCase))
            return "nuget_package_vendor_file";
        if (path.Contains("\\bin\\", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("\\obj\\", StringComparison.OrdinalIgnoreCase))
            return "build_output_file";
        if (path.EndsWith(".Designer.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".tt", StringComparison.OrdinalIgnoreCase))
            return "generated_code_file";
        if (path.Contains("\\Scripts\\", StringComparison.OrdinalIgnoreCase) &&
            (path.Contains(".min.", StringComparison.OrdinalIgnoreCase) ||
             path.Contains("breeze.debug", StringComparison.OrdinalIgnoreCase) ||
             path.Contains("jquery", StringComparison.OrdinalIgnoreCase) ||
             path.Contains("angular", StringComparison.OrdinalIgnoreCase)))
            return "third_party_script_file";
        if (request.ArtifactKind.Contains("vendor", StringComparison.OrdinalIgnoreCase) ||
            request.ArtifactKind.Contains("generated", StringComparison.OrdinalIgnoreCase))
            return "artifact_kind_excluded";

        return "";
    }

    private static string ClassifySourceRootRole(CodeDocumentationRequest request)
    {
        if (IsFrameworkLifecycleSurface(request))
            return "framework_lifecycle_root";
        if (request.ArtifactKind.Contains("config", StringComparison.OrdinalIgnoreCase) ||
            request.FilePath.EndsWith("Web.config", StringComparison.OrdinalIgnoreCase))
            return "configuration_surface";
        if (request.ArtifactKind.Contains("startup", StringComparison.OrdinalIgnoreCase) ||
            request.FilePath.EndsWith("Global.asax.cs", StringComparison.OrdinalIgnoreCase))
            return "startup_root";
        if (string.Equals(request.DocumentationLevel, "D1", StringComparison.OrdinalIgnoreCase))
            return "leaf_source";
        if (NeedsCalleeSummaries(request))
            return "caller_or_workflow_root";

        return "source_file";
    }

    private static CodeDocumentationVersionGate EvaluateVersionGate(CodeDocumentationRequest request)
    {
        var wrong = 0;
        var notes = new List<string>();
        if (string.IsNullOrWhiteSpace(request.TargetFramework))
        {
            wrong++;
            notes.Add("target_framework_missing");
        }

        if (string.IsNullOrWhiteSpace(request.CSharpLanguageVersion))
        {
            wrong++;
            notes.Add("csharp_language_version_missing");
        }

        if (request.TargetFramework.Contains("netcoreapp", StringComparison.OrdinalIgnoreCase) &&
            !request.RuntimeFamily.Contains("NET_CORE", StringComparison.OrdinalIgnoreCase))
        {
            wrong++;
            notes.Add("netcore_target_requires_NET_CORE_runtime_family");
        }

        if (IsNetFrameworkTarget(request.TargetFramework) &&
            !request.RuntimeFamily.Contains("NET_FRAMEWORK", StringComparison.OrdinalIgnoreCase))
        {
            wrong++;
            notes.Add("net_framework_target_requires_NET_FRAMEWORK_runtime_family");
        }

        if (request.TargetFramework.Contains("netcoreapp3.1", StringComparison.OrdinalIgnoreCase) &&
            !request.CSharpLanguageVersion.Contains("CSharp8", StringComparison.OrdinalIgnoreCase) &&
            !request.CSharpLanguageVersion.StartsWith("explicit:", StringComparison.OrdinalIgnoreCase))
        {
            wrong++;
            notes.Add("netcoreapp3_1_should_use_CSharp8_unless_explicit");
        }

        if (request.ProjectStyle.Contains("legacy", StringComparison.OrdinalIgnoreCase) &&
            request.CSharpLanguageVersion.Contains("CSharp8", StringComparison.OrdinalIgnoreCase))
        {
            wrong++;
            notes.Add("legacy_non_sdk_should_not_default_to_CSharp8");
        }

        var score = wrong == 0 ? 1.0 : 0.0;
        return new CodeDocumentationVersionGate(wrong == 0, score, wrong, notes.ToArray());
    }

    private static CodeDocumentationTransitGate EvaluateTransitGate(CodeDocumentationRequest request)
    {
        var needsCalleeSummaries = NeedsCalleeSummaries(request);
        var knownIds = request.KnownCallees
            .Select(item => item.SymbolId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.Ordinal);
        var missing = needsCalleeSummaries
            ? request.Calls.Count(call => !knownIds.Contains(call.CalleeSymbolId))
            : 0;
        var transitScore = missing == 0 ? 1.0 : 0.0;
        var hasRequiredCallees = !needsCalleeSummaries || missing == 0;
        var bottomUpScore = string.Equals(request.DocumentationLevel, "D1", StringComparison.OrdinalIgnoreCase) || hasRequiredCallees || request.Calls.Count == 0
            ? 1.0
            : 0.0;
        var localVsDelegated = hasRequiredCallees || request.Calls.Count == 0
            ? 1.0
            : 0.0;
        var required = BuildRequiredEvidence(request, needsCalleeSummaries);

        return new CodeDocumentationTransitGate(
            missing == 0,
            needsCalleeSummaries,
            missing,
            bottomUpScore,
            transitScore,
            localVsDelegated,
            required);
    }

    private static bool NeedsCalleeSummaries(CodeDocumentationRequest request)
        => (string.Equals(request.DocumentationLevel, "D3", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(request.DocumentationLevel, "D4", StringComparison.OrdinalIgnoreCase)) &&
           request.Calls.Count > 0;

    private static string[] BuildRequiredEvidence(
        CodeDocumentationRequest request,
        bool needsCalleeSummaries)
    {
        var evidence = new List<string>
        {
            "source_code",
            "version_fields",
            "symbol_id"
        };

        if (needsCalleeSummaries)
            evidence.Add("accepted_callee_summaries");

        if (string.Equals(request.DocumentationLevel, "D4", StringComparison.OrdinalIgnoreCase))
            evidence.Add("caller_chain_summary");

        return evidence.ToArray();
    }

    private static CodeDocumentationStage[] BuildStages(
        CodeDocumentationRequest request,
        CodeDocumentationTransitGate transitGate)
    {
        var stages = new List<CodeDocumentationStage>
        {
            new("leaf-method-summary", 1, request.SymbolId, "Document locally visible behavior without caller invention.")
        };

        if (IsFrameworkLifecycleSurface(request))
            stages.Add(new("framework-lifecycle-summary", stages.Count + 1, request.SymbolId, "Use framework lifecycle contracts to explain host, startup, middleware, controller activation, and endpoint dispatch without inventing domain workflow."));

        if (!string.Equals(request.DocumentationLevel, "D1", StringComparison.OrdinalIgnoreCase))
            stages.Add(new("component-context-summary", stages.Count + 1, request.SymbolId, "Use method summaries to describe the class or component boundary."));

        if (string.Equals(request.DocumentationLevel, "D3", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(request.DocumentationLevel, "D4", StringComparison.OrdinalIgnoreCase))
        {
            stages.Add(new("callee-summary-propagation", stages.Count + 1, request.SymbolId, transitGate.IsPassed
                ? "Propagate accepted callee summaries to the caller."
                : "Blocked until accepted callee summaries are supplied."));
            stages.Add(new("caller-chain-summary", stages.Count + 1, request.SymbolId, "Separate local caller behavior from delegated callee behavior."));
        }

        if (string.Equals(request.DocumentationLevel, "D4", StringComparison.OrdinalIgnoreCase))
            stages.Add(new("workflow-module-summary", stages.Count + 1, request.SymbolId, "Build workflow documentation from accepted D1-D3 evidence."));

        return stages.ToArray();
    }

    private static string[] BuildWarnings(
        CodeDocumentationSourceGate sourceGate,
        CodeDocumentationVersionGate versionGate,
        CodeDocumentationTransitGate transitGate)
    {
        var warnings = new List<string>();
        warnings.AddRange(sourceGate.Notes);
        warnings.AddRange(versionGate.Notes);
        if (transitGate.MissingCalleeSummaryCount > 0)
            warnings.Add("missing_accepted_callee_summaries_for_caller_documentation");
        return warnings.ToArray();
    }

    private static bool IsNetFrameworkTarget(string targetFramework)
        => targetFramework.Equals("net46", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("net461", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("net472", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("net45", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("net452", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("net35", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("v4.5", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("v4.6", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("v4.6.1", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("v4.7.2", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("v4.5.2", StringComparison.OrdinalIgnoreCase) ||
           targetFramework.Equals("v3.5", StringComparison.OrdinalIgnoreCase);

    private static bool IsFrameworkLifecycleSurface(CodeDocumentationRequest request)
        => request.ArtifactKind.Contains("framework_lifecycle", StringComparison.OrdinalIgnoreCase) ||
           request.ArtifactKind.Contains("startup", StringComparison.OrdinalIgnoreCase) ||
           request.WebFramework.Contains("ASP.NET Core", StringComparison.OrdinalIgnoreCase) &&
           (request.FilePath.EndsWith("Program.cs", StringComparison.OrdinalIgnoreCase) ||
            request.FilePath.EndsWith("Startup.cs", StringComparison.OrdinalIgnoreCase) ||
            request.SymbolId.Contains("CreateHostBuilder", StringComparison.OrdinalIgnoreCase) ||
            request.SymbolId.Contains("Startup.Configure", StringComparison.OrdinalIgnoreCase));

    private static double FrameworkLifecycleAccuracy(CodeDocumentationRoutingResult value)
        => value.SourceGate.SourceRootRole == "framework_lifecycle_root"
            ? Math.Min(value.SourceGate.Score, Math.Min(value.VersionGate.Score, value.TransitGate.CalleeTransitScore))
            : value.VersionGate.Score;
}

public sealed record CodeDocumentationRequest(
    string RequestId,
    string SourceId,
    string Repository,
    string CommitSha,
    string FilePath,
    string SymbolId,
    string CodeText,
    IReadOnlyList<CodeDocumentationCallEdge> Calls,
    IReadOnlyList<CodeDocumentationCalleeSummary> KnownCallees,
    string AppSide,
    string RuntimeFamily,
    string TargetFramework,
    string CSharpLanguageVersion,
    string LanguageVersionConfidence,
    string ProjectStyle,
    string WebFramework,
    string WebFrameworkVersion,
    string FrontendFramework,
    string FrontendFrameworkVersion,
    string StackLabel,
    string ArtifactKind,
    string DocumentationLevel);

public sealed record CodeDocumentationCallEdge(
    string CallerSymbolId,
    string CalleeSymbolId,
    string CallKind);

public sealed record CodeDocumentationCalleeSummary(
    string SymbolId,
    string Summary,
    string[] EvidenceIds);

public sealed record CodeDocumentationRoutingResult(
    string RequestId,
    string ChosenUnitId,
    string TrainingRunId,
    string SourceDatasetSha256,
    string AppSide,
    string RuntimeFamily,
    string TargetFramework,
    string CSharpLanguageVersion,
    string LanguageVersionConfidence,
    string ProjectStyle,
    string WebFramework,
    string WebFrameworkVersion,
    string FrontendFramework,
    string FrontendFrameworkVersion,
    string StackLabel,
    string ArtifactKind,
    string DocumentationLevel,
    CodeDocumentationStage[] Stages,
    CodeDocumentationSourceGate SourceGate,
    CodeDocumentationVersionGate VersionGate,
    CodeDocumentationTransitGate TransitGate,
    double RoutingConfidence,
    string Verdict,
    string[] Warnings)
{
    public string AuthorityRole { get; init; } = string.Empty;
}

public sealed record CodeDocumentationStage(
    string StageTag,
    int Order,
    string SymbolId,
    string Purpose);

public sealed record CodeDocumentationSourceGate(
    bool IsPassed,
    double Score,
    string SourceRootRole,
    string ExcludedReason,
    string[] Notes);

public sealed record CodeDocumentationVersionGate(
    bool IsPassed,
    double Score,
    int WrongVersionCount,
    string[] Notes);

public sealed record CodeDocumentationTransitGate(
    bool IsPassed,
    bool RequiresCalleeSummaries,
    int MissingCalleeSummaryCount,
    double BottomUpOrderScore,
    double CalleeTransitScore,
    double LocalVsDelegatedScore,
    string[] RequiredEvidence);
