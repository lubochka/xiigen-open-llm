// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure.Logical;
using TrainableRouter.Training;

public sealed class CodeDocumentationRoutingFlowTests
{
    [Fact]
    public async Task CodeDocumentationRoutingFlow_RoutesNetCore31CallerWithAcceptedCalleeSummaries()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);

        var request = BuildNetCore31DynamicControllerRequest("D3", withCallees: true);

        var routed = await flow.RouteDocumentationAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(OrchestrationRoutingCurriculum.DocumentationDotNetUnitId, routed.Value!.ChosenUnitId);
        Assert.Equal(
            RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
            routed.Value.AuthorityRole);
        Assert.Equal("netcoreapp3.1", routed.Value.TargetFramework);
        Assert.Equal("CSharp8", routed.Value.CSharpLanguageVersion);
        Assert.Equal("NETCORE_WEBAPI", routed.Value.StackLabel);
        Assert.Equal("DOCUMENTATION_ROUTE_READY", routed.Value.Verdict);
        Assert.True(routed.Value.SourceGate.IsPassed);
        Assert.True(routed.Value.VersionGate.IsPassed);
        Assert.True(routed.Value.TransitGate.IsPassed);
        Assert.Contains(routed.Value.Stages, stage => stage.StageTag == "leaf-method-summary");
        Assert.Contains(routed.Value.Stages, stage => stage.StageTag == "callee-summary-propagation");
        Assert.Contains(routed.Value.Stages, stage => stage.StageTag == "caller-chain-summary");
    }

    [Fact]
    public async Task CodeDocumentationRoutingFlow_PreservesNetFramework46AngularJsLegacyContext()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);

        var request = BuildNetFrameworkAuthRequest();

        var executed = await flow.ExecuteAsync(new LogicalFlowRequest<CodeDocumentationRequest>(
            "run-genietasks-auth",
            CodeDocumentationRoutingFlow.FlowId,
            CodeDocumentationRoutingFlow.ScenarioId,
            request,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(executed.IsSuccess, executed.ErrorMessage);
        Assert.Equal("DOCUMENTATION_ROUTE_READY", executed.Value!.OverallVerdict);
        Assert.Equal("mixed", executed.Value.Metrics["app_side"]);
        Assert.Equal("NET_FRAMEWORK", executed.Value.Metrics["runtime_family"]);
        Assert.Equal("net46", executed.Value.Metrics["target_framework"]);
        Assert.Equal("CSharp5_or_CSharp6_review_required", executed.Value.Metrics["csharp_language_version"]);
        Assert.Equal("AngularJS", executed.Value.Metrics["frontend_framework"]);
        Assert.Equal("1.4.8", executed.Value.Metrics["frontend_framework_version"]);
        Assert.Equal("1", executed.Value.Metrics["version_routing_accuracy"]);
        Assert.Equal("1", executed.Value.Metrics["source_selection_accuracy"]);
        Assert.Equal("1", executed.Value.Metrics["callee_summary_transit_accuracy"]);
        Assert.Equal(
            RoutingAuthorityVocabulary.BaselineCandidateGeneratorRole,
            executed.Value.Metrics[RoutingAuthorityVocabulary.MetadataKey]);
    }

    [Fact]
    public async Task CodeDocumentationRoutingFlow_BlocksCallerDocumentationWithoutCalleeSummaries()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);

        var request = BuildNetCore31DynamicControllerRequest("D3", withCallees: false);

        var routed = await flow.RouteDocumentationAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal(OrchestrationRoutingCurriculum.DocumentationDotNetUnitId, routed.Value!.ChosenUnitId);
        Assert.Equal("DOCUMENTATION_ROUTE_REVIEW_REQUIRED", routed.Value.Verdict);
        Assert.False(routed.Value.TransitGate.IsPassed);
        Assert.True(routed.Value.TransitGate.MissingCalleeSummaryCount > 0);
        Assert.Contains("missing_accepted_callee_summaries_for_caller_documentation", routed.Value.Warnings);
    }

    [Fact]
    public async Task CodeDocumentationRoutingFlow_BlocksCallerDocumentationWhenAnyCalledSymbolIsMissing()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);

        var request = BuildNetFrameworkAuthRequest() with
        {
            KnownCallees =
            [
                new CodeDocumentationCalleeSummary(
                    "HelpAuthClass.ConvertToMD5",
                    "Hashes the supplied password string with MD5 and returns a lowercase hexadecimal digest.",
                    ["AuthController.cs:114"])
            ]
        };

        var routed = await flow.RouteDocumentationAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal("DOCUMENTATION_ROUTE_REVIEW_REQUIRED", routed.Value!.Verdict);
        Assert.False(routed.Value.TransitGate.IsPassed);
        Assert.Equal(2, routed.Value.TransitGate.MissingCalleeSummaryCount);
        Assert.Equal(0.0, routed.Value.TransitGate.CalleeTransitScore);
    }

    [Fact]
    public async Task CodeDocumentationRoutingFlow_FlagsWrongLegacyVersionDefaults()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);

        var request = BuildNetFrameworkAuthRequest() with
        {
            CSharpLanguageVersion = "CSharp8"
        };

        var routed = await flow.RouteDocumentationAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal("DOCUMENTATION_ROUTE_REVIEW_REQUIRED", routed.Value!.Verdict);
        Assert.False(routed.Value.VersionGate.IsPassed);
        Assert.Equal(1, routed.Value.VersionGate.WrongVersionCount);
        Assert.Contains("legacy_non_sdk_should_not_default_to_CSharp8", routed.Value.VersionGate.Notes);
    }

    [Fact]
    public async Task CodeDocumentationRoutingFlow_FlagsGeneratedOrVendorSourceFiles()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);

        var request = BuildNetFrameworkAuthRequest() with
        {
            FilePath = @"A1ProjectManagement\Scripts\breeze.debug.js",
            SymbolId = "Scripts.breeze.debug.vendor",
            CodeText = "function EntityQuery(resourceName) { this.resourceName = resourceName; }",
            Calls = [],
            KnownCallees = [],
            ArtifactKind = "generated_vendor_file",
            DocumentationLevel = "D1"
        };

        var routed = await flow.RouteDocumentationAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal("DOCUMENTATION_ROUTE_REVIEW_REQUIRED", routed.Value!.Verdict);
        Assert.False(routed.Value.SourceGate.IsPassed);
        Assert.Equal("excluded_generated_or_vendor", routed.Value.SourceGate.SourceRootRole);
        Assert.Equal("third_party_script_file", routed.Value.SourceGate.ExcludedReason);
    }

    [Fact]
    public async Task CodeDocumentationRoutingFlow_RoutesThinAspNetCoreLifecycleWithFrameworkTransit()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);

        var request = BuildGeniesoftLifecycleRequest();

        var executed = await flow.ExecuteAsync(new LogicalFlowRequest<CodeDocumentationRequest>(
            "run-geniesoft-lifecycle",
            CodeDocumentationRoutingFlow.FlowId,
            CodeDocumentationRoutingFlow.ScenarioId,
            request,
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        Assert.True(executed.IsSuccess, executed.ErrorMessage);
        Assert.Equal("DOCUMENTATION_ROUTE_READY", executed.Value!.OverallVerdict);
        Assert.Equal("framework_lifecycle_root", executed.Value.Metrics["source_root_role"]);
        Assert.Equal("1", executed.Value.Metrics["framework_lifecycle_accuracy"]);
        Assert.Contains("framework-lifecycle-summary", executed.Value.Metrics["stage_tags"]);
        Assert.Equal("1", executed.Value.Metrics["callee_summary_transit_accuracy"]);
    }

    [Fact]
    public async Task CodeDocumentationRoutingFlow_RecognizesLegacyFrameworkVersionsBeyondNet46()
    {
        var runtime = await DistributedRoutingBootstrap.BuildDefaultAsync();
        Assert.True(runtime.IsSuccess, runtime.ErrorMessage);
        var flow = new CodeDocumentationRoutingFlow(runtime.Value!);

        var request = BuildNetFrameworkAuthRequest() with
        {
            TargetFramework = "v3.5",
            CSharpLanguageVersion = "CSharp3",
            StackLabel = "NETFX35_WEBFORMS",
            WebFramework = "ASP.NET WebForms",
            WebFrameworkVersion = "System.Web v3.5"
        };

        var routed = await flow.RouteDocumentationAsync(request);

        Assert.True(routed.IsSuccess, routed.ErrorMessage);
        Assert.Equal("DOCUMENTATION_ROUTE_READY", routed.Value!.Verdict);
        Assert.True(routed.Value.VersionGate.IsPassed);
        Assert.Equal("v3.5", routed.Value.TargetFramework);
        Assert.Equal("CSharp3", routed.Value.CSharpLanguageVersion);
    }

    private static CodeDocumentationRequest BuildNetCore31DynamicControllerRequest(
        string documentationLevel,
        bool withCallees)
        => new(
            "code-doc-wiz-dynamic-store",
            "local-wizGameserver-static-eval",
            @"C:\Projects\wizGameserver",
            "local",
            @"WebApplication2\Controllers\DynamicController.cs",
            "WebApplication2.Controllers.DynamicController.StoreDocument(string,string,object)",
            "public ActionResult StoreDocument(string index_name, string id, object document) { var myDocument = ParseObjectAlternative(document); return Ok(myDocument); }",
            [
                new CodeDocumentationCallEdge(
                    "DynamicController.StoreDocument",
                    "DynamicController.ParseObjectAlternative",
                    "direct_method_call")
            ],
            withCallees
                ?
                [
                    new CodeDocumentationCalleeSummary(
                        "DynamicController.ParseObjectAlternative",
                        "Recursively normalizes JSON object and array values and delegates primitive conversion to leaf parsers.",
                        ["DynamicController.cs:204"])
                ]
                : [],
            "server",
            "NET_CORE",
            "netcoreapp3.1",
            "CSharp8",
            "inferred_high",
            "sdk_style",
            "ASP.NET Core WebApi",
            "Microsoft.AspNetCore.Authentication.JwtBearer 3.1.8",
            "none",
            "none",
            "NETCORE_WEBAPI",
            "controller_action",
            documentationLevel);

    private static CodeDocumentationRequest BuildNetFrameworkAuthRequest()
        => new(
            "code-doc-genietasks-auth",
            "local-genietasks-static-eval",
            @"C:\Projects\genietasks",
            "local",
            @"A1ProjectManagement\AuthController.cs",
            "A1ProjectManagement.AuthController.Authenticate(LoginDetails)",
            "public UserBack Authenticate(LoginDetails user) { string passMD5 = user.password.ConvertToMD5(); Member authUser = ...; return CreateBackUser(authUser); }",
            [
                new CodeDocumentationCallEdge("AuthController.Authenticate", "HelpAuthClass.ConvertToMD5", "extension_method_call"),
                new CodeDocumentationCallEdge("AuthController.Authenticate", "AuthController.CreateBackUser", "direct_method_call"),
                new CodeDocumentationCallEdge("UserBack.UserBack(Member)", "JwtManager.GenerateToken", "direct_method_call")
            ],
            [
                new CodeDocumentationCalleeSummary(
                    "HelpAuthClass.ConvertToMD5",
                    "Hashes the supplied password string with MD5 and returns a lowercase hexadecimal digest.",
                    ["AuthController.cs:114"]),
                new CodeDocumentationCalleeSummary(
                    "JwtManager.GenerateToken",
                    "Creates a JWT token with legacy claim fields using the static symmetric key and expiry window.",
                    ["JwtManager.cs:12"]),
                new CodeDocumentationCalleeSummary(
                    "AuthController.CreateBackUser",
                    "Wraps a Member in UserBack, which generates the token during construction.",
                    ["AuthController.cs:211"])
            ],
            "mixed",
            "NET_FRAMEWORK",
            "net46",
            "CSharp5_or_CSharp6_review_required",
            "inferred_review_required",
            "legacy_non_sdk",
            "WebApi2 plus ASMX",
            "System.Web.Http 5.2.3 / Breeze.WebApi2 1.6.5",
            "AngularJS",
            "1.4.8",
            "NETFX_WEBAPI",
            "auth_workflow",
            "D3");

    private static CodeDocumentationRequest BuildGeniesoftLifecycleRequest()
        => new(
            "code-doc-geniesoft-program-main",
            "local-geniesoft-template-lifecycle-eval",
            @"C:\Projects\geniesoftGameMVPServer",
            "local",
            @"geniesoftGameMVPServer\geniesoftGameMVPServer\Program.cs",
            "geniesoftGameMVPServer::Program.Main",
            "public static void Main(string[] args) { CreateHostBuilder(args).Build().Run(); }",
            [
                new CodeDocumentationCallEdge(
                    "geniesoftGameMVPServer::Program.Main",
                    "geniesoftGameMVPServer::Program.CreateHostBuilder",
                    "direct_method_call"),
                new CodeDocumentationCallEdge(
                    "geniesoftGameMVPServer::Program.CreateHostBuilder",
                    "geniesoftGameMVPServer::Startup.ConfigureServices",
                    "aspnetcore_host_lifecycle"),
                new CodeDocumentationCallEdge(
                    "geniesoftGameMVPServer::Program.CreateHostBuilder",
                    "geniesoftGameMVPServer::Startup.Configure",
                    "aspnetcore_host_lifecycle")
            ],
            [
                new CodeDocumentationCalleeSummary(
                    "geniesoftGameMVPServer::Program.CreateHostBuilder",
                    "Builds the ASP.NET Core generic host and wires Startup through ConfigureWebHostDefaults.",
                    ["Program.cs:19"]),
                new CodeDocumentationCalleeSummary(
                    "geniesoftGameMVPServer::Startup.ConfigureServices",
                    "Registers controller services for endpoint routing.",
                    ["Startup.cs:26"]),
                new CodeDocumentationCalleeSummary(
                    "geniesoftGameMVPServer::Startup.Configure",
                    "Builds the HTTP middleware pipeline with development exception handling, HTTPS redirection, routing, authorization, and controller endpoint mapping.",
                    ["Startup.cs:32"])
            ],
            "server",
            "NET_CORE",
            "netcoreapp3.1",
            "CSharp8",
            "csproj_exact",
            "sdk_style",
            "ASP.NET Core WebApi",
            "Microsoft.AspNetCore.App 3.1",
            "none",
            "none",
            "NETCORE31_TEMPLATE_WEBAPI",
            "framework_lifecycle_startup",
            "D4");
}
