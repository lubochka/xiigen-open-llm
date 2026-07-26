// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerImprovementLoopTests
{
    private const string RouteAdmissionCurrentToken = "current-improvement-loop-route-token-0123456789";
    private const string RouteAdmissionPreviousToken = "previous-improvement-loop-route-token-0123456789";
    private const string RouteAdmissionTenantId = "tenant-improvement-loop-alpha";
    private static readonly DateTimeOffset RouteAdmissionNow = DateTimeOffset.UnixEpoch.AddHours(25);

    [Fact]
    public void CreateModelServerMinimumResponsiveMatrix_SingleDesktopSourceAppendsServerOwnedMobile()
    {
        var result = DesignConversionRenderResolutionFactory.CreateModelServerMinimumResponsiveMatrix(
        [
            new DesignRenderArtifact(
                ArtifactId: "n6-4-factory-desktop",
                Kind: "design-render",
                Width: 1440,
                Height: 900,
                Uri: "artifact://n6-4/factory-desktop.png",
                Sha256: "sha256:n6-4-factory-desktop",
                Metadata: new Dictionary<string, string>())
        ]);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var resolutions = result.Value!;
        Assert.Equal(
            ["source-1440x900", DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId],
            resolutions.Select(resolution => resolution.ResolutionId).ToArray());
        Assert.Equal([1440, 390], resolutions.Select(resolution => resolution.Width).ToArray());
        Assert.Equal([900, 844], resolutions.Select(resolution => resolution.Height).ToArray());
        Assert.All(resolutions, resolution => Assert.Equal(1.0, resolution.DeviceScaleFactor));
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RegisteredRoute_ReturnsRefsOnlySuccessEnvelope()
    {
        const string privateMarker = "PRIVATE_PASS129_SUCCESS_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.96, 0.01m)
                : JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("pass129-success-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-success-loop",
            InitialPackage: CreatePackage("pass129-success-request", privateMarker),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        Assert.Equal("pass129-success-request", envelope.ParsedRequestId);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("deadlineExceeded").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["socket_opened"]);
        Assert.Equal(2, renderProvider.Requests.Select(request => request.Metadata["job_id"]).Distinct(StringComparer.Ordinal).Count());
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MultiViewportEvaluationConsumesOneImprovementAttempt()
    {
        const string privateMarker = "PRIVATE_N8_2_ATTEMPT_COUNT_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromExternalScore(0.96, 0.01m)
                : JudgeScript.FromExternalScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-2-second-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n8-2-attempt-count-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "n8-2-local-loop",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90"
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-2-attempt-count-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());

        var attempts = root.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Equal(2, attempts.Length);
        var firstAttemptMetadata = attempts[0].GetProperty("publicMetadata");
        var secondAttemptMetadata = attempts[1].GetProperty("publicMetadata");
        Assert.Equal("1", firstAttemptMetadata.GetProperty("attempt_number").GetString());
        Assert.Equal("1", firstAttemptMetadata.GetProperty("policy_attempt_count").GetString());
        Assert.Equal("2", firstAttemptMetadata.GetProperty("render_result_count").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, firstAttemptMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal("2", secondAttemptMetadata.GetProperty("attempt_number").GetString());
        Assert.Equal("2", secondAttemptMetadata.GetProperty("policy_attempt_count").GetString());
        Assert.Equal("2", secondAttemptMetadata.GetProperty("render_result_count").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, secondAttemptMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal("0.02", firstAttemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0.02", secondAttemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0.02", firstAttemptMetadata.GetProperty("loop_spent_budget_usd").GetString());
        Assert.Equal("0.04", secondAttemptMetadata.GetProperty("loop_spent_budget_usd").GetString());
        Assert.Equal("0.04", root.GetProperty("boundaryMetadata").GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0.96", root.GetProperty("boundaryMetadata").GetProperty("final_remaining_budget_usd").GetString());

        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Single(strategy.Requests);
        Assert.Equal(1, strategy.Requests[0].CompletedAttemptNumber);
        Assert.Equal(0.02m, Assert.Single(strategy.Requests[0].AttemptHistory).EstimatedCostUsd);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_BudgetQualityThresholdSurvivesHandlerMappingAndControlsAcceptance()
    {
        const string privateMarker = "PRIVATE_N8_9_QUALITY_THRESHOLD_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.94 : 0.92);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.94, 0.01m)
                : JudgeScript.FromScore(0.92, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-9-threshold-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n8-9-threshold-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "n8-9-server-custom-threshold",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.93",
                    ["budget_private_marker"] = privateMarker,
                    ["budget_raw_payload"] = $"<div data-secret=\"{privateMarker}\"></div>"
                }),
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-9-threshold-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, initialPackage);
        Assert.DoesNotContain("\"quality_threshold\":\"0.10\"", envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        Assert.Equal("0.93", root.GetProperty("boundaryMetadata").GetProperty("final_required_score").GetString());

        var attempts = root.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Equal(2, attempts.Length);
        var firstAttemptMetadata = attempts[0].GetProperty("publicMetadata");
        var secondAttemptMetadata = attempts[1].GetProperty("publicMetadata");
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ContinueImprovement, firstAttemptMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal("0.93", firstAttemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal("0.92", firstAttemptMetadata.GetProperty("measured_quality_score").GetString());
        Assert.Equal(bool.FalseString, firstAttemptMetadata.GetProperty("visual_decision_quality_threshold_met").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, secondAttemptMetadata.GetProperty("stop_reason").GetString());
        Assert.Equal("0.93", secondAttemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal("0.94", secondAttemptMetadata.GetProperty("measured_quality_score").GetString());
        Assert.Equal(bool.TrueString, secondAttemptMetadata.GetProperty("visual_decision_quality_threshold_met").GetString());

        Assert.All(visualJudge.Requests, request =>
        {
            Assert.Equal("0.93", request.Metadata["quality_threshold"]);
            Assert.DoesNotContain(privateMarker, string.Join("|", request.Metadata.Values), StringComparison.Ordinal);
        });
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Single(strategy.Requests);
    }

    [Theory]
    [InlineData("NaN", "n8-9-invalid-threshold", "0.85")]
    [InlineData("2.00", "n8-9-invalid-threshold", "0.85")]
    [InlineData("-0.10", "n8-9-invalid-threshold", "0.85")]
    [InlineData("0.00", "n8-9-invalid-threshold", "0.85")]
    [InlineData("0.10", "pro", "0.9")]
    public async Task TryRunImprovementLoopAsync_InvalidOrLoweredQualityThresholdFallsBackToServerBoundary(
        string configuredThreshold,
        string tier,
        string expectedThreshold)
    {
        const string privateMarker = "PRIVATE_N8_9_INVALID_THRESHOLD_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            new ScriptedCandidateStrategy());
        var initialPackage = CreatePackage($"n8-9-invalid-threshold-{tier}-{configuredThreshold}", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: tier,
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = configuredThreshold,
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata = new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: $"n8-9-invalid-threshold-loop-{tier}-{configuredThreshold}",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["quality_threshold"] = "0.10",
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, initialPackage);
        Assert.DoesNotContain("\"quality_threshold\":\"0.10\"", envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.All(visualJudge.Requests, request => Assert.Equal(expectedThreshold, request.Metadata["quality_threshold"]));

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        Assert.Equal(
            expectedThreshold,
            document.RootElement.GetProperty("boundaryMetadata").GetProperty("final_required_score").GetString());
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CumulativeVisualCostStopsBeforeNextCandidateWouldExceedBudget()
    {
        const string privateMarker = "PRIVATE_N8_4_CUMULATIVE_COST_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromExternalScore(0.72, 0.04m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-4-should-not-be-created", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n8-4-cumulative-cost-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "n8-4-local-loop",
                MaxImprovementCandidates: 5,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.10m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90"
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-4-cumulative-cost-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == "IMPROVEMENT_LOOP_NEXT_ESTIMATED_COST_EXCEEDS_REMAINING_BUDGET");

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("0.08", attemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0.08", attemptMetadata.GetProperty("loop_spent_budget_usd").GetString());
        Assert.Equal("0.1", attemptMetadata.GetProperty("budget_usd").GetString());
        Assert.Equal("0.02", attemptMetadata.GetProperty("remaining_budget_usd").GetString());
        Assert.Equal("0.08", root.GetProperty("boundaryMetadata").GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0.02", root.GetProperty("boundaryMetadata").GetProperty("final_remaining_budget_usd").GetString());

        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MissingCoverageCostProjectionStopsBeforeSameCandidateRetry()
    {
        const string privateMarker = "PRIVATE_N8_4_COVERAGE_COST_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromExternalScore(0.96, 0.08m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-4-coverage-should-not-run", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var basePackage = CreatePackage("n8-4-coverage-cost-request", privateMarker);
        var initialPackage = basePackage with
        {
            Budget = basePackage.Budget! with
            {
                Tier = "n8-4-coverage-cost-loop",
                MaxImprovementCandidates = 5,
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.10m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-4-coverage-cost-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("renderManifestIncomplete").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == "IMPROVEMENT_LOOP_COVERAGE_RETRY_STOPPED:cost_budget");
        Assert.Contains(
            root.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == "improvement_loop:coverage_retry_stop_source=cost_budget");
        Assert.Contains(
            root.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == DesignConversionVisualQaBoundaryPolicy.CoverageRetryMarker);

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("0.08", attemptMetadata.GetProperty("estimated_cost_usd").GetString());
        Assert.Equal("0.08", attemptMetadata.GetProperty("loop_spent_budget_usd").GetString());
        Assert.Equal("0.02", attemptMetadata.GetProperty("remaining_budget_usd").GetString());
        Assert.Equal("cost_budget", attemptMetadata.GetProperty("coverage_retry_stop_source").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("coverage_retry_same_candidate").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
        Assert.Equal("0.08", root.GetProperty("boundaryMetadata").GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0.02", root.GetProperty("boundaryMetadata").GetProperty("final_remaining_budget_usd").GetString());

        Assert.Single(renderProvider.Requests);
        Assert.Single(comparisonProvider.Requests);
        Assert.Single(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CoverageRetryMayConsumeExactRemainingBudget()
    {
        const string privateMarker = "PRIVATE_N8_4_COVERAGE_EQUALITY_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromExternalScore(0.96, 0.08m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n8-4-coverage-equality-should-not-run", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var basePackage = CreatePackage("n8-4-coverage-equality-request", privateMarker);
        var initialPackage = basePackage with
        {
            Budget = basePackage.Budget! with
            {
                Tier = "n8-4-coverage-equality-loop",
                MaxImprovementCandidates = 5,
                MaxRenderQaRuns = 1,
                BudgetUsd = 0.16m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-4-coverage-equality-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("renderManifestIncomplete").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        Assert.Equal("0.16", root.GetProperty("boundaryMetadata").GetProperty("spent_budget_usd").GetString());
        Assert.Equal("0", root.GetProperty("boundaryMetadata").GetProperty("final_remaining_budget_usd").GetString());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == "IMPROVEMENT_LOOP_COVERAGE_RETRY_STOPPED:cost_budget");

        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_NullBudgetDefaultRunsBothSourceViewports()
    {
        const string privateMarker = "PRIVATE_N6_3_NULL_BUDGET_MULTI_VIEWPORT_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n6-3-null-budget-multi-viewport-request", privateMarker) with
        {
            Budget = null
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n6-3-null-budget-multi-viewport-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("qualityReached").GetBoolean());
        Assert.False(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        Assert.Equal(2, attempt.GetProperty("renderResultRefs").GetArrayLength());
        Assert.Equal(2, attempt.GetProperty("comparisonArtifactRefs").GetArrayLength());
        Assert.Equal(2, attempt.GetProperty("visualJudgeResultRefs").GetArrayLength());
        Assert.Equal(
            ["source-1440x900", "source-390x844"],
            renderProvider.Requests.Select(request => request.Resolution.ResolutionId).ToArray());
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_SingleLogicalViewportDefaultAddsServerOwnedComplementAndFailsClosed()
    {
        const string privateMarker = "PRIVATE_N6_4_SINGLE_VIEWPORT_EXPECTED_REF_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("n6-4-single-mobile-default-request", privateMarker) with
        {
            Screenshots =
            [
                CreateArtifact("n6-4-single-mobile-default-request:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders =
            [
                CreateArtifact("n6-4-single-mobile-default-request:design-mobile", "design-render", 390, 844)
            ],
            Budget = null
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n6-4-single-mobile-default-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.DoesNotContain("artifact://", envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain("local_path", envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
        foreach (var artifact in initialPackage.Screenshots!.Concat(initialPackage.DesignRenders!))
        {
            Assert.DoesNotContain(artifact.Uri, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(artifact.Sha256, envelope.ResponseBodyJson, StringComparison.Ordinal);
        }

        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.False(root.GetProperty("renderManifestIncomplete").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            root.GetProperty("warnings").EnumerateArray(),
            warning => warning.GetString() == DesignConversionInternalService.ViewportExpectedReferenceMissingStatus);

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        Assert.Equal(2, attempt.GetProperty("renderResultRefs").GetArrayLength());
        Assert.Single(attempt.GetProperty("comparisonArtifactRefs").EnumerateArray());
        Assert.Single(attempt.GetProperty("visualJudgeResultRefs").EnumerateArray());
        Assert.Contains(
            attempt.GetProperty("evidenceRefs").EnumerateArray(),
            evidence => evidence.GetString() == "improvement_loop:stop_reason=viewport_operation_failed");
        Assert.Equal("2", attempt.GetProperty("publicMetadata").GetProperty("render_result_count").GetString());
        Assert.Equal("1", attempt.GetProperty("publicMetadata").GetProperty("comparison_artifact_count").GetString());
        Assert.Equal("1", attempt.GetProperty("publicMetadata").GetProperty("visual_judge_result_count").GetString());

        Assert.Equal(
            ["source-390x844", DesignConversionRenderResolutionFactory.ServerOwnedDesktopResolutionId],
            renderProvider.Requests.Select(request => request.Resolution.ResolutionId).ToArray());
        Assert.Equal(
            [390, 1440],
            renderProvider.Requests.Select(request => request.Resolution.Width).ToArray());
        Assert.Equal(
            [844, 900],
            renderProvider.Requests.Select(request => request.Resolution.Height).ToArray());
        Assert.Equal(2, renderProvider.Requests.Count);
        var comparisonRequest = Assert.Single(comparisonProvider.Requests);
        var judgeRequest = Assert.Single(visualJudge.Requests);
        Assert.Equal("source-390x844", comparisonRequest.Actual.Resolution.ResolutionId);
        Assert.Equal("source-390x844", judgeRequest.RenderedImage.Resolution.ResolutionId);
        Assert.Empty(strategy.Requests);

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("product_ready").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("quality_reached").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("viewport_operation_failed").GetString());
        Assert.Equal("1", metadata.GetProperty("viewport_failure_count").GetString());
        Assert.Equal("expected_reference", metadata.GetProperty("viewport_failure_stage").GetString());
        Assert.Equal(DesignConversionInternalService.ExpectedRenderResolutionMissingCode, metadata.GetProperty("viewport_failure_error_code").GetString());
        Assert.Equal(DesignConversionInternalService.ViewportExpectedReferenceMissingStatus, metadata.GetProperty("viewport_failure_status").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("expected_reference_failure_bounded").GetString());
        Assert.Equal(DesignConversionInternalService.ExpectedRenderResolutionMissingCode, metadata.GetProperty("expected_reference_failure_code").GetString());
        Assert.Equal(DesignConversionInternalService.ViewportExpectedReferenceMissingStatus, metadata.GetProperty("expected_reference_failure_status").GetString());
        var metadataJson = metadata.GetRawText();
        Assert.DoesNotContain(privateMarker, metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Html!, metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Css!, metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain("artifact://", metadataJson, StringComparison.Ordinal);
        Assert.DoesNotContain("local_path", metadataJson, StringComparison.OrdinalIgnoreCase);
        foreach (var artifact in initialPackage.Screenshots!.Concat(initialPackage.DesignRenders!))
        {
            Assert.DoesNotContain(artifact.Uri, metadataJson, StringComparison.Ordinal);
            Assert.DoesNotContain(artifact.Sha256, metadataJson, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_ExplicitMaxRenderQaRunsOneKeepsCallerCap()
    {
        const string privateMarker = "PRIVATE_N6_3_EXPLICIT_ONE_VIEWPORT_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var basePackage = CreatePackage("n6-3-explicit-one-viewport-request", privateMarker);
        var initialPackage = basePackage with
        {
            Screenshots =
            [
                CreateArtifact("n6-3-explicit-one-viewport-request:source-desktop", "screenshot", 1440, 900)
            ],
            DesignRenders =
            [
                CreateArtifact("n6-3-explicit-one-viewport-request:design-desktop", "design-render", 1440, 900)
            ],
            Budget = basePackage.Budget! with
            {
                MaxRenderQaRuns = 1
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n6-3-explicit-one-viewport-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["render_manifest_incomplete"]);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.RenderManifestIncomplete, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("renderManifestIncomplete").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        Assert.Single(attempt.GetProperty("renderResultRefs").EnumerateArray());
        Assert.Equal(
            ["source-1440x900"],
            renderProvider.Requests.Select(request => request.Resolution.ResolutionId).ToArray());
        Assert.DoesNotContain(
            DesignConversionRenderResolutionFactory.ServerOwnedMobileResolutionId,
            renderProvider.Requests.Select(request => request.Resolution.ResolutionId));
        Assert.Single(comparisonProvider.Requests);
        Assert.Single(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_FreeTierCapStopsLoopAndExposesSafePolicyMetadata()
    {
        const string privateMarker = "PRIVATE_PASS_TIER_POLICY_API_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("pass-tier-policy-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage("pass-tier-policy-api-request", privateMarker) with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "free",
                MaxImprovementCandidates: 10,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass-tier-policy-api-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("free", attemptMetadata.GetProperty("tier").GetString());
        Assert.Equal("0.8", attemptMetadata.GetProperty("required_score").GetString());
        Assert.Equal("0.05", attemptMetadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal("1", attemptMetadata.GetProperty("effective_max_improvement_candidates").GetString());
        Assert.Equal("1", attemptMetadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("10", attemptMetadata.GetProperty("submitted_max_improvement_candidates").GetString());
        Assert.Equal("iteration_budget", attemptMetadata.GetProperty("budget_stop_source").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("tier_policy_budget_cap_applied").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("tier_policy_iteration_cap_applied").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("raw_design_tree_included").GetString());

        Assert.Empty(strategy.Requests);
        Assert.Equal("free", envelope.BoundaryMetadata["tier"]);
        Assert.Equal("0.8", envelope.BoundaryMetadata["final_required_score"]);
        Assert.Equal("0.05", envelope.BoundaryMetadata["effective_budget_usd"]);
        Assert.Equal("1", envelope.BoundaryMetadata["effective_max_improvement_candidates"]);
        Assert.Equal("1", envelope.BoundaryMetadata["submitted_budget_usd"]);
        Assert.Equal("10", envelope.BoundaryMetadata["submitted_max_improvement_candidates"]);
        Assert.Equal("iteration_budget", envelope.BoundaryMetadata["budget_stop_source"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["tier_policy_budget_cap_applied"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["tier_policy_iteration_cap_applied"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["provider_payload_included"]);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RuntimeImprovementStrategyProducesChangedSecondAttempt()
    {
        const string privateMarker = "PRIVATE_PASS_NON_COPY_LOOP_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.96, 0.01m)
                : JudgeScript.FromScore(0.72, 0.01m));
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            new DesignConversionRuntimeImprovementCandidateStrategy());
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass-non-copy-runtime-loop",
            InitialPackage: CreatePackage("pass-non-copy-runtime-request", privateMarker),
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["local_non_copy_candidate_strategy"] = bool.TrueString
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.Equal(2, root.GetProperty("attemptCount").GetInt32());
        var attempts = root.GetProperty("attempts").EnumerateArray().ToArray();
        Assert.Equal(2, attempts.Length);
        Assert.Equal(1, attempts[0].GetProperty("attemptNumber").GetInt32());
        Assert.Equal(2, attempts[1].GetProperty("attemptNumber").GetInt32());

        var firstAttemptRenderRequests = renderProvider.Requests
            .Where(request => !request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal))
            .ToArray();
        var secondAttemptRenderRequests = renderProvider.Requests
            .Where(request => request.Metadata["job_id"].Contains("loop-attempt:2", StringComparison.Ordinal))
            .ToArray();
        Assert.NotEmpty(firstAttemptRenderRequests);
        Assert.NotEmpty(secondAttemptRenderRequests);
        Assert.All(secondAttemptRenderRequests, request =>
        {
            Assert.NotEqual(firstAttemptRenderRequests[0].RequestId, request.RequestId);
            Assert.NotEqual(firstAttemptRenderRequests[0].Html, request.Html);
            Assert.NotEqual(firstAttemptRenderRequests[0].Css, request.Css);
            Assert.Contains("dcm-local-improved", request.Html, StringComparison.Ordinal);
            Assert.Contains("gap:12px", request.Css, StringComparison.Ordinal);
            Assert.Contains("padding:16px", request.Css, StringComparison.Ordinal);
        });
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_LocalArtifactDbFailureAfterSuccessfulLoopReturnsNonDurableResult()
    {
        const string privateMarker = "PRIVATE_N8_12_DB_FAILURE_SECRET";
        const string rawDbPath = "C:\\private\\artifact-db\\PRIVATE_N8_12_DB_FAILURE_SECRET\\conversion-results.jsonl";
        const string rawExceptionMessage = "raw DB append failed at C:\\private\\artifact-db\\PRIVATE_N8_12_DB_FAILURE_SECRET\\conversion-results.jsonl";
        var artifactDb = new FailingAppendArtifactDb(rawExceptionMessage, "SCRIPTED_PERSISTENCE_FAILURE");
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy, artifactDb);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "n8-12-db-failure-loop",
            InitialPackage: CreatePackage("n8-12-db-failure-request", privateMarker),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
        Assert.Equal("n8-12-db-failure-request", envelope.ParsedRequestId);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal(1, artifactDb.AppendAttempts);
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("qualityReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Equal("0.95", root.GetProperty("boundaryMetadata").GetProperty("final_measured_quality_score").GetString());
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["persistence_invoked"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbPersistenceFailedMarker]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbPersistenceFailureBoundedMarker]);
        Assert.Equal("non_durable", envelope.BoundaryMetadata["local_artifact_db_persistence_status"]);
        Assert.Equal("SCRIPTED_PERSISTENCE_FAILURE", envelope.BoundaryMetadata["local_artifact_db_persistence_error_code"]);

        Assert.DoesNotContain(rawExceptionMessage, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(rawDbPath, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_ConfiguredCompositionRootsAutowriteOnceAndNoConfigIsNonDurable()
    {
        const string privateMarker = "PRIVATE_N8_13_COMPOSITION_AUTOWRITE_SECRET";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "n8-13-composition-autowrite");
        Directory.CreateDirectory(root);
        try
        {
            var startupArtifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(root, "startup-improvement-only-db"));
            var startupAdapter = CreateStartupImprovementLoopAdapter(startupArtifactDb);
            var startupPackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "n8-13-startup-loop",
                InitialPackage: CreatePackage("n8-13-startup-request", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["n8_13_composition"] = "startup_improvement_only",
                    ["caller_private_marker"] = privateMarker
                });

            var startupEnvelope = await startupAdapter.TryRunImprovementLoopAsync(startupPackage);

            var startupStored = await AssertConfiguredAutowriteAsync(
                startupArtifactDb,
                startupEnvelope,
                startupPackage,
                Path.Combine(root, "startup-improvement-only-db"),
                privateMarker);

            var fullRouteArtifactDb = new JsonlDesignConversionLocalArtifactDb(Path.Combine(root, "full-local-route-db"));
            var fullRouteAdapter = CreateFullLocalRouteAdapter(fullRouteArtifactDb);
            var fullRoutePackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "n8-13-full-route-loop",
                InitialPackage: CreatePackage("n8-13-full-route-request", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["n8_13_composition"] = "full_local_route",
                    ["caller_private_marker"] = privateMarker
                });

            var fullRouteEnvelope = await fullRouteAdapter.TryRunImprovementLoopAsync(fullRoutePackage);

            var fullRouteStored = await AssertConfiguredAutowriteAsync(
                fullRouteArtifactDb,
                fullRouteEnvelope,
                fullRoutePackage,
                Path.Combine(root, "full-local-route-db"),
                privateMarker);

            AssertMatchingAutowriteMetadata(startupStored, fullRouteStored);

            var noConfigAdapter = CreateStartupImprovementLoopAdapter(artifactDb: null);
            var noConfigPackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "n8-13-no-config-loop",
                InitialPackage: CreatePackage("n8-13-no-config-request", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["n8_13_composition"] = "startup_no_config",
                    ["caller_private_marker"] = privateMarker
                });

            var noConfigEnvelope = await noConfigAdapter.TryRunImprovementLoopAsync(noConfigPackage);

            AssertStableImprovementLoopEnvelope(
                noConfigEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal("success", noConfigEnvelope.EnvelopeKind);
            Assert.Equal(DesignConversionThinClientStatus.Completed, noConfigEnvelope.ParsedContractStatus);
            Assert.Equal("not_configured_non_durable", noConfigEnvelope.BoundaryMetadata["local_artifact_db_persistence_status"]);
            Assert.Equal(bool.FalseString, noConfigEnvelope.BoundaryMetadata["persistence_invoked"]);
            Assert.DoesNotContain(
                JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker,
                noConfigEnvelope.ResponseBodyJson,
                StringComparison.Ordinal);
            AssertNoPrivateMarkerLeak(noConfigEnvelope.ResponseBodyJson, privateMarker, noConfigPackage.InitialPackage!);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }

        static DesignConversionModelServerLocalCallerAdapter CreateStartupImprovementLoopAdapter(
            IDesignConversionLocalArtifactDb? artifactDb)
        {
            var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal(
                comparisonProvider: new RecordingComparisonProvider(_ => 0.95),
                visualJudge: new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0m)));
            var strategy = new ScriptedCandidateStrategy();
            var registry = artifactDb is null
                ? DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
                    composition,
                    strategy)
                : DesignConversionModelServerAspNetHostStartup.CreateImprovementLoopRegistry(
                    composition,
                    strategy,
                    artifactDb,
                    () => RouteAdmissionNow);
            return new DesignConversionModelServerLocalCallerAdapter(registry);
        }

        static DesignConversionModelServerLocalCallerAdapter CreateFullLocalRouteAdapter(
            IDesignConversionLocalArtifactDb artifactDb)
        {
            var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal(
                comparisonProvider: new RecordingComparisonProvider(_ => 0.95),
                visualJudge: new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0m)));
            var registry = DesignConversionModelServerAspNetHostStartup.CreateLocalRouteRegistry(
                composition,
                new ScriptedCandidateStrategy(),
                artifactDb,
                () => RouteAdmissionNow);
            return new DesignConversionModelServerLocalCallerAdapter(registry);
        }

        static async Task<DesignConversionLocalArtifactDbRecord> AssertConfiguredAutowriteAsync(
            JsonlDesignConversionLocalArtifactDb artifactDb,
            DesignConversionModelServerLocalCallerAdapterResponse envelope,
            DesignConversionModelServerImprovementLoopLocalCallerPackage package,
            string artifactDbRoot,
            string privateMarker)
        {
            AssertStableImprovementLoopEnvelope(
                envelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal("success", envelope.EnvelopeKind);
            Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
            Assert.Equal(package.InitialPackage!.RequestId, envelope.ParsedRequestId);
            Assert.Contains(
                DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker,
                envelope.ResponseBodyJson,
                StringComparison.Ordinal);
            Assert.DoesNotContain(artifactDbRoot, envelope.ResponseBodyJson, StringComparison.OrdinalIgnoreCase);
            AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage);

            var allRecords = await artifactDb.ListRecordsAsync();
            Assert.True(allRecords.IsSuccess, allRecords.ErrorMessage);
            var stored = Assert.Single(allRecords.Value!);
            Assert.Equal(package.InitialPackage.RequestId, stored.RequestId);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.LocalArtifactDbKind, stored.DatabaseKind);
            Assert.Equal(nameof(DesignConversionImprovementLoopCoordinator), stored.Metadata["artifact_db_record_source"]);
            Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
            Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);
            Assert.Equal(bool.TrueString, stored.Metadata["improvement_loop_to_local_training_store"]);
            Assert.Equal("1", stored.Metadata["attempt_count"]);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, stored.Metadata["stop_reason"]);
            Assert.Equal(bool.FalseString, stored.Metadata["final_candidate_from_loop"]);
            Assert.Equal(bool.FalseString, stored.Metadata["raw_html_included"]);
            Assert.Equal(bool.FalseString, stored.Metadata["raw_css_included"]);
            Assert.Equal(bool.FalseString, stored.Metadata["raw_design_tree_included"]);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, stored.EvidenceRefs);

            var storedJson = JsonSerializer.Serialize(stored, DesignConversionCheckpointJsonOptions.Standard);
            Assert.DoesNotContain(privateMarker, storedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Html!, storedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Css!, storedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, storedJson, StringComparison.Ordinal);
            Assert.DoesNotContain(artifactDbRoot, storedJson, StringComparison.OrdinalIgnoreCase);
            return stored;
        }

        static void AssertMatchingAutowriteMetadata(
            DesignConversionLocalArtifactDbRecord left,
            DesignConversionLocalArtifactDbRecord right)
        {
            var keys = new[]
            {
                "artifact_db_record_source",
                DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker,
                DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker,
                "improvement_loop_to_local_training_store",
                "attempt_count",
                "stop_reason",
                "final_candidate_from_loop",
                "raw_html_included",
                "raw_css_included",
                "raw_design_tree_included",
                "training_candidate_only",
                "product_not_ready",
                "production_db_ready"
            };

            foreach (var key in keys)
                Assert.Equal(left.Metadata[key], right.Metadata[key]);
        }
    }

    [Fact]
    public async Task Pass163_SameScenarioPackageToLocalDpoIntakeStopsAtSourceDisjointGateWithoutPromotion()
    {
        const string privateMarker = "PRIVATE_PASS163_SAME_SCENARIO_SECRET";
        const string loopId = "pass163-same-scenario-loop";
        const string requestId = "pass163-same-scenario-request";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass163-same-scenario-chain");
        Directory.CreateDirectory(root);
        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
            var usefulnessApi = new DesignConversionDpoUsefulnessApi(artifactDb);
            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(_ => 0.95);
            var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.95, 0m));
            var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
                CreateCandidate("pass163-improved-candidate", privateMarker)));
            var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy, artifactDb);
            var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: loopId,
                InitialPackage: CreatePass163Package(requestId, privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["pass163_same_scenario_id"] = "pass163-local-chain",
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString
                });

            var envelope = await adapter.TryRunImprovementLoopAsync(package);

            AssertStableImprovementLoopEnvelope(
                envelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal("success", envelope.EnvelopeKind);
            Assert.Equal(DesignConversionThinClientStatus.Completed, envelope.ParsedContractStatus);
            Assert.Equal(requestId, envelope.ParsedRequestId);
            Assert.False(envelope.RawPrivateResponseLeakageDetected);
            Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);

            using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
            var responseRoot = document.RootElement;
            Assert.Equal(loopId, responseRoot.GetProperty("loopId").GetString());
            Assert.Equal(requestId, responseRoot.GetProperty("requestId").GetString());
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.QualityReached, responseRoot.GetProperty("stopReason").GetString());
            Assert.True(responseRoot.GetProperty("completed").GetBoolean());
            Assert.True(responseRoot.GetProperty("qualityReached").GetBoolean());
            Assert.Equal(1, responseRoot.GetProperty("attemptCount").GetInt32());
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_ai_visual_judge_executed"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["real_external_provider_executed"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["network_call_executed"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["deployment_verified"]);
            Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
            Assert.Equal("2", envelope.BoundaryMetadata["render_result_count"]);
            Assert.Equal("2", envelope.BoundaryMetadata["comparison_artifact_count"]);
            Assert.Equal("2", envelope.BoundaryMetadata["visual_judge_result_count"]);

            var attempt = Assert.Single(responseRoot.GetProperty("attempts").EnumerateArray());
            Assert.Equal(2, attempt.GetProperty("renderResultRefs").GetArrayLength());
            Assert.Equal(2, attempt.GetProperty("comparisonArtifactRefs").GetArrayLength());
            Assert.Equal(2, attempt.GetProperty("visualJudgeResultRefs").GetArrayLength());
            var attemptMetadata = attempt.GetProperty("publicMetadata");
            Assert.Equal("2", attemptMetadata.GetProperty("render_result_count").GetString());
            Assert.Equal("2", attemptMetadata.GetProperty("comparison_artifact_count").GetString());
            Assert.Equal("2", attemptMetadata.GetProperty("visual_judge_result_count").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("external_ai_visual_judge_executed").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("real_external_provider_executed").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("network_call_executed").GetString());
            Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("product_ready").GetString());

            var records = await artifactDb.ListRecordsAsync();
            Assert.True(records.IsSuccess, records.ErrorMessage);
            var stored = Assert.Single(records.Value!);
            Assert.Equal(DesignConversionSourceKind.Figma.ToString(), stored.SourceKind);
            Assert.Equal("pass163-local-no-provider", stored.Tier);
            Assert.Equal(package.InitialPackage.DesignTree.RefId, stored.SanitizedSourceRefs["source_manifest_ref"]);
            Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalArtifactDbAutowriteMarker]);
            Assert.Equal(bool.TrueString, stored.Metadata[DesignConversionImprovementLoopCoordinator.LocalTrainingStoreDpoSourceMarker]);
            Assert.Equal(bool.TrueString, stored.Metadata["product_not_ready"]);
            Assert.Contains(JsonlDesignConversionLocalArtifactDb.StorageTrainingArtifactDbMarker, stored.EvidenceRefs);
            Assert.Equal(2, stored.RenderArtifactRefs.Count);
            Assert.Equal(
                ["source-1440x900", "source-390x844"],
                stored.RenderArtifactRefs.Select(artifact => artifact.ViewportId).Order(StringComparer.Ordinal).ToArray());
            Assert.All(stored.RenderArtifactRefs, artifact =>
            {
                Assert.StartsWith("artifact://pass129/render/", artifact.UriRef, StringComparison.Ordinal);
                Assert.StartsWith("sha256:", artifact.Sha256, StringComparison.Ordinal);
                Assert.False(artifact.Metadata.ContainsKey("raw_html_included"));
                Assert.False(artifact.Metadata.ContainsKey("raw_css_included"));
            });
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, stored.VisualJudgeStatus.Status);
            Assert.False(stored.VisualJudgeStatus.ExternalCallExecuted);
            Assert.Equal(bool.TrueString, stored.VisualJudgeStatus.Metadata["product_not_ready"]);

            var dpoCandidate = Assert.Single(stored.DpoCandidates);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, dpoCandidate.TrainingRecord.Provenance.Metadata["review_status"]);
            Assert.Equal(bool.FalseString, dpoCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            Assert.Equal(bool.TrueString, dpoCandidate.TrainingRecord.Provenance.Metadata["source_disjoint_not_proven"]);
            Assert.Equal(package.InitialPackage.DesignTree.Sha256, dpoCandidate.TrainingRecord.Provenance.SourceManifestRef);
            Assert.True(dpoCandidate.TrainingCandidateOnly);
            Assert.True(dpoCandidate.ProductNotReady);

            var usefulnessBeforeApproval = await usefulnessApi.QueryAsync(
                new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
            Assert.True(usefulnessBeforeApproval.IsSuccess, usefulnessBeforeApproval.ErrorMessage);
            Assert.Equal(0, usefulnessBeforeApproval.Value!.TrainReadyCount);
            var rowBeforeApproval = Assert.Single(usefulnessBeforeApproval.Value.Rows);
            Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalPendingStatus, rowBeforeApproval.ApprovalStatus);
            Assert.False(rowBeforeApproval.TrainReady);
            Assert.True(rowBeforeApproval.ProductNotReady);
            Assert.False(rowBeforeApproval.ModelReadinessClaimed);

            var displayBeforeApproval = await artifactDb.QueryDisplayRowsAsync(
                new DesignConversionDpoDisplayQuery(IncludeBlocked: true));
            Assert.True(displayBeforeApproval.IsSuccess, displayBeforeApproval.ErrorMessage);
            var displayRowBeforeApproval = Assert.Single(displayBeforeApproval.Value!.Rows);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, displayRowBeforeApproval.BoundaryMetadata["review_status"]);
            Assert.Equal(bool.FalseString, displayRowBeforeApproval.BoundaryMetadata["approved_for_training"]);

            var readbackBeforeApproval = await usefulnessApi.QueryLocalArtifactReadbackAsync(
                new DesignConversionLocalArtifactReadbackApiRequest(JobId: stored.JobId));
            Assert.True(readbackBeforeApproval.IsSuccess, readbackBeforeApproval.ErrorMessage);
            Assert.Equal(1, readbackBeforeApproval.Value!.DpoCandidateCount);
            var readbackRow = Assert.Single(readbackBeforeApproval.Value.Rows);
            Assert.Equal(2, readbackRow.RenderArtifactRefs.Count);
            Assert.Equal(
                ["source-1440x900", "source-390x844"],
                readbackRow.RenderArtifactRefs.Select(artifact => artifact.ViewportId).Order(StringComparer.Ordinal).ToArray());
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker, readbackRow.VisualJudgeStatus);
            Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ConfiguredOrLocalLabelerBoundary, readbackRow.VisualJudgeProviderBoundary);
            Assert.False(readbackRow.ExternalVisualAiCallExecuted);
            Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalPendingStatus, readbackRow.DpoApprovalStatus);
            Assert.True(readbackRow.ProductNotReady);
            Assert.False(readbackRow.ProductionDpo);
            Assert.False(readbackRow.ModelReadinessClaimed);
            Assert.Equal(DesignConversionDpoUsefulnessApi.ProductNotReadyBoundary, readbackRow.ReadinessBoundary);

            var approval = await usefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync(
                stored.RecordId,
                dpoCandidate.CandidateId,
                "reviewer:pass163-local",
                "same-scenario package/API local chain review; approval may create intake but not bypass source-disjoint gate",
                DateTimeOffset.UnixEpoch.AddMinutes(163));
            Assert.True(approval.IsSuccess, approval.ErrorMessage);
            Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus, approval.Value!.ApprovalStatus);
            Assert.True(approval.Value.ApprovedForTraining);
            Assert.Equal(bool.FalseString, approval.Value.BoundaryMetadata["model_readiness_claimed"]);

            var usefulnessAfterApproval = await usefulnessApi.QueryAsync(
                new DesignConversionDpoUsefulnessApiRequest(IncludeBlocked: true));
            Assert.True(usefulnessAfterApproval.IsSuccess, usefulnessAfterApproval.ErrorMessage);
            Assert.Equal(1, usefulnessAfterApproval.Value!.TrainReadyCount);
            var rowAfterApproval = Assert.Single(usefulnessAfterApproval.Value.Rows);
            Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus, rowAfterApproval.ApprovalStatus);
            Assert.True(rowAfterApproval.TrainReady);
            Assert.True(rowAfterApproval.ProductNotReady);
            Assert.False(rowAfterApproval.ModelReadinessClaimed);

            var handoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: stored.JobId),
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(handoff.IsSuccess, handoff.ErrorMessage);
            var handoffRow = Assert.Single(handoff.Value!.Rows);
            Assert.Equal(1, handoff.Value.ApprovedCandidateCount);
            Assert.Equal(package.InitialPackage.DesignTree.Sha256, handoffRow.SourceManifestRef);
            Assert.True(handoffRow.TrainingHandoffOnly);

            var intake = await usefulnessApi.CreateTrainingIntakeAsync(
                handoff.Value,
                "architect:pass163-local",
                "Pass163 same-scenario retrain request from local artifact DB; source-disjoint gate must decide runner eligibility",
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(intake.IsSuccess, intake.ErrorMessage);
            Assert.Equal(1, intake.Value!.AcceptedCandidateCount);
            Assert.True(intake.Value.TrainingIntakeOnly);
            Assert.True(intake.Value.ModelUploadNotReady);
            Assert.True(intake.Value.ProductNotReady);

            var preflight = await artifactDb.RunTrainingIntakePreflightAsync(intake.Value.IntakeId);
            Assert.True(preflight.IsSuccess, preflight.ErrorMessage);
            Assert.False(preflight.Value!.MayStartTraining);
            Assert.Equal("proven", preflight.Value.SourceManifestStatus);
            Assert.Equal("blocked_same_source", preflight.Value.SourceDisjointStatus);
            Assert.False(preflight.Value.ModelUploadReady);
            Assert.Contains("source_disjoint_blocked_same_source", preflight.Value.BlockReasons);
            Assert.Equal("1", preflight.Value.BoundaryMetadata["source_manifest_candidate_count"]);
            Assert.Equal("0", preflight.Value.BoundaryMetadata["source_disjoint_train_source_count"]);
            Assert.Equal("1", preflight.Value.BoundaryMetadata["source_disjoint_heldout_source_count"]);

            var promotion = await usefulnessApi.RunTrainingIntakeAndPromoteLocalAsync(
                intake.Value.IntakeId,
                Path.Combine(root, "blocked-local-promotion"));
            Assert.True(promotion.IsSuccess, promotion.ErrorMessage);
            Assert.False(promotion.Value!.TrainingRun.MayStartTraining);
            Assert.False(promotion.Value.TrainingRun.TrainingRunnerInvoked);
            Assert.False(promotion.Value.TrainingRun.TrainingStarted);
            Assert.False(promotion.Value.TrainingRun.CheckpointMutated);
            Assert.False(promotion.Value.TrainingRun.WeightsProduced);
            Assert.False(promotion.Value.TrainingRun.ModelUploadReady);
            Assert.Equal("blocked_same_source", promotion.Value.TrainingRun.SourceDisjointStatus);
            Assert.Equal("blocked_before_runner_by_training_intake_preflight", promotion.Value.TrainingRun.TrainingExecutionBoundary);
            Assert.Contains("source_disjoint_blocked_same_source", promotion.Value.TrainingRun.BlockReasons);
            Assert.False(promotion.Value.LocalPromotionReady);
            Assert.False(promotion.Value.ExternalCloudUploadAttempted);
            Assert.Null(promotion.Value.Publication);
            Assert.Null(promotion.Value.PromotedLocalModel);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["local_promotion_ready"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["external_cloud_upload_attempted"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["external_cloud_upload_not_attempted"]);
            Assert.Equal(bool.FalseString, promotion.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["cloud_model_upload_not_attempted"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["model_server_upload_not_attempted_local_only"]);
            Assert.Equal(bool.TrueString, promotion.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_only"]);

            var persistedJson = await File.ReadAllTextAsync(Path.Combine(root, "conversion-results.jsonl"));
            var serializedUsefulness = JsonSerializer.Serialize(usefulnessAfterApproval.Value);
            var serializedReadback = JsonSerializer.Serialize(readbackBeforeApproval.Value);
            var serializedIntake = JsonSerializer.Serialize(intake.Value);
            var serializedPromotion = JsonSerializer.Serialize(promotion.Value);
            foreach (var payload in new[] { persistedJson, serializedUsefulness, serializedReadback, serializedIntake, serializedPromotion })
            {
                Assert.DoesNotContain(privateMarker, payload, StringComparison.Ordinal);
                Assert.DoesNotContain(package.InitialPackage.Html!, payload, StringComparison.Ordinal);
                Assert.DoesNotContain(package.InitialPackage.Css!, payload, StringComparison.Ordinal);
                Assert.DoesNotContain(package.InitialPackage.DesignTree.Payload!, payload, StringComparison.Ordinal);
                Assert.DoesNotContain("C:/Users/", payload, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("external-provider-payload", payload, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Pass164_SourceDisjointLoopCreatedRuntimeCandidatesPassTrainingIntakeBoundary()
    {
        const string privateMarker = "PRIVATE_PASS164_SOURCE_DISJOINT_SECRET";
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass164-source-disjoint-loop-training");
        Directory.CreateDirectory(root);
        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
            var usefulnessApi = new DesignConversionDpoUsefulnessApi(artifactDb);
            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
            var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0m));
            var adapter = CreateAdapter(
                renderProvider,
                comparisonProvider,
                visualJudge,
                new ScriptedCandidateStrategy(),
                artifactDb);

            var firstPackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "pass164-source-a-loop",
                InitialPackage: CreatePass164Package("pass164-source-a-request", "pass164-source-a", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["pass164_source_disjoint_probe"] = bool.TrueString,
                    ["source_family"] = "pass164-source-a",
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString
                });

            var firstEnvelope = await adapter.TryRunImprovementLoopAsync(firstPackage);
            AssertStableImprovementLoopEnvelope(
                firstEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal(bool.FalseString, firstEnvelope.BoundaryMetadata["external_provider_called"]);
            Assert.Equal(bool.FalseString, firstEnvelope.BoundaryMetadata["product_ready"]);

            var firstRecords = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(RequestId: "pass164-source-a-request"));
            Assert.True(firstRecords.IsSuccess, firstRecords.ErrorMessage);
            var firstStored = Assert.Single(firstRecords.Value!);
            var firstCandidate = Assert.Single(firstStored.DpoCandidates);
            Assert.Equal(
                firstPackage.InitialPackage!.DesignTree!.Sha256,
                firstCandidate.TrainingRecord.Provenance.SourceManifestRef);
            Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, firstCandidate.TrainingRecord.Provenance.Metadata["review_status"]);
            Assert.Equal(bool.FalseString, firstCandidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
            AssertRuntimeQualityCreatedDpoPair(firstCandidate.TrainingRecord);

            var firstApproval = await usefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync(
                firstStored.RecordId,
                firstCandidate.CandidateId,
                "reviewer:pass164-local",
                "Pass164 first runtime candidate approval; single-source intake must remain blocked",
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(firstApproval.IsSuccess, firstApproval.ErrorMessage);

            var singleSourceHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: firstStored.JobId),
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(singleSourceHandoff.IsSuccess, singleSourceHandoff.ErrorMessage);
            var singleSourceRow = Assert.Single(singleSourceHandoff.Value!.Rows);
            Assert.Equal(firstPackage.InitialPackage.DesignTree.Sha256, singleSourceRow.SourceManifestRef);
            Assert.True(singleSourceRow.TrainingHandoffOnly);

            var singleSourceIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                singleSourceHandoff.Value,
                "architect:pass164-local",
                "Pass164 negative control: one loop-created runtime source must not start training",
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(singleSourceIntake.IsSuccess, singleSourceIntake.ErrorMessage);
            var singleSourcePreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(singleSourceIntake.Value!.IntakeId);
            Assert.True(singleSourcePreflight.IsSuccess, singleSourcePreflight.ErrorMessage);
            Assert.False(singleSourcePreflight.Value!.MayStartTraining);
            Assert.Equal("proven", singleSourcePreflight.Value.SourceManifestStatus);
            Assert.Equal("blocked_same_source", singleSourcePreflight.Value.SourceDisjointStatus);
            Assert.Contains("source_disjoint_blocked_same_source", singleSourcePreflight.Value.BlockReasons);
            Assert.Equal("1", singleSourcePreflight.Value.BoundaryMetadata["source_manifest_candidate_count"]);
            Assert.Equal("0", singleSourcePreflight.Value.BoundaryMetadata["source_disjoint_train_source_count"]);
            Assert.Equal("1", singleSourcePreflight.Value.BoundaryMetadata["source_disjoint_heldout_source_count"]);
            Assert.Equal(bool.FalseString, singleSourcePreflight.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, singleSourcePreflight.Value.BoundaryMetadata["product_ready"]);

            var secondPackage = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
                LoopId: "pass164-source-b-loop",
                InitialPackage: CreatePass164Package("pass164-source-b-request", "pass164-source-b", privateMarker),
                MaxAttempts: 2,
                Metadata: new Dictionary<string, string>
                {
                    ["pass164_source_disjoint_probe"] = bool.TrueString,
                    ["source_family"] = "pass164-source-b",
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString
                });

            var secondEnvelope = await adapter.TryRunImprovementLoopAsync(secondPackage);
            AssertStableImprovementLoopEnvelope(
                secondEnvelope,
                statusCode: 200,
                registryInvokeSucceeded: true,
                runtimeInvoked: true);
            Assert.Equal(bool.FalseString, secondEnvelope.BoundaryMetadata["external_provider_called"]);
            Assert.Equal(bool.FalseString, secondEnvelope.BoundaryMetadata["product_ready"]);

            var secondRecords = await artifactDb.ListRecordsAsync(
                new DesignConversionLocalArtifactDbQuery(RequestId: "pass164-source-b-request"));
            Assert.True(secondRecords.IsSuccess, secondRecords.ErrorMessage);
            var secondStored = Assert.Single(secondRecords.Value!);
            var secondCandidate = Assert.Single(secondStored.DpoCandidates);
            Assert.Equal(
                secondPackage.InitialPackage!.DesignTree!.Sha256,
                secondCandidate.TrainingRecord.Provenance.SourceManifestRef);
            Assert.NotEqual(
                firstCandidate.TrainingRecord.Provenance.SourceManifestRef,
                secondCandidate.TrainingRecord.Provenance.SourceManifestRef);
            AssertRuntimeQualityCreatedDpoPair(secondCandidate.TrainingRecord);

            var secondApproval = await usefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync(
                secondStored.RecordId,
                secondCandidate.CandidateId,
                "reviewer:pass164-local",
                "Pass164 second runtime candidate approval; source-disjoint intake may reach runner boundary",
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(secondApproval.IsSuccess, secondApproval.ErrorMessage);

            var sourceDisjointHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(),
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(sourceDisjointHandoff.IsSuccess, sourceDisjointHandoff.ErrorMessage);
            Assert.Equal(2, sourceDisjointHandoff.Value!.ApprovedCandidateCount);
            Assert.Equal(2, sourceDisjointHandoff.Value.Rows.Count);
            Assert.Equal(
                2,
                sourceDisjointHandoff.Value.Rows
                    .Select(row => row.SourceManifestRef)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Equal(
                2,
                sourceDisjointHandoff.Value.Rows
                    .Select(row => row.SourceStructureHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.All(sourceDisjointHandoff.Value.Rows, row =>
            {
                Assert.True(row.TrainingHandoffOnly);
                Assert.Equal(bool.TrueString, row.BoundaryMetadata["runtime_created_dpo_candidate"]);
                Assert.Equal(bool.FalseString, row.BoundaryMetadata["production_dpo"]);
                Assert.Equal(bool.TrueString, row.BoundaryMetadata["product_not_ready"]);
                Assert.Equal(bool.FalseString, row.BoundaryMetadata["model_readiness_claimed"]);
            });

            var sourceDisjointIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                sourceDisjointHandoff.Value,
                "architect:pass164-local",
                "Pass164 source-disjoint runtime candidates from two source manifests may cross preflight only",
                DateTimeOffset.UnixEpoch.AddMinutes(165));
            Assert.True(sourceDisjointIntake.IsSuccess, sourceDisjointIntake.ErrorMessage);
            Assert.Equal(2, sourceDisjointIntake.Value!.AcceptedCandidateCount);
            Assert.Equal(
                2,
                sourceDisjointIntake.Value.CandidateRefs
                    .Select(candidate => candidate.SourceIdentityHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.True(sourceDisjointIntake.Value.ProductNotReady);
            Assert.Equal(bool.FalseString, sourceDisjointIntake.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal("proven", sourceDisjointIntake.Value.BoundaryMetadata["source_manifest_status"]);
            Assert.Equal("proven", sourceDisjointIntake.Value.BoundaryMetadata["source_disjoint_status"]);

            var sourceDisjointPreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(sourceDisjointIntake.Value.IntakeId);
            Assert.True(sourceDisjointPreflight.IsSuccess, sourceDisjointPreflight.ErrorMessage);
            Assert.True(sourceDisjointPreflight.Value!.MayStartTraining);
            Assert.Equal("proven", sourceDisjointPreflight.Value.SourceManifestStatus);
            Assert.Equal("proven", sourceDisjointPreflight.Value.SourceDisjointStatus);
            Assert.DoesNotContain("source_disjoint_blocked_same_source", sourceDisjointPreflight.Value.BlockReasons);
            Assert.Empty(sourceDisjointPreflight.Value.BlockReasons);
            Assert.Equal("2", sourceDisjointPreflight.Value.BoundaryMetadata["source_manifest_candidate_count"]);
            Assert.Equal("2", sourceDisjointPreflight.Value.BoundaryMetadata["source_manifest_hash_count"]);
            Assert.Equal("1", sourceDisjointPreflight.Value.BoundaryMetadata["source_disjoint_train_source_count"]);
            Assert.Equal("1", sourceDisjointPreflight.Value.BoundaryMetadata["source_disjoint_heldout_source_count"]);
            Assert.Equal("0", sourceDisjointPreflight.Value.BoundaryMetadata["source_disjoint_overlap_count"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["training_started"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["weights_produced"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["product_ready"]);

            var sourceDisjointRun = await usefulnessApi.RunTrainingIntakeAsync(sourceDisjointIntake.Value.IntakeId);
            var serializedRun = JsonSerializer.Serialize(sourceDisjointRun.Value);
            Assert.True(sourceDisjointRun.IsSuccess, sourceDisjointRun.ErrorMessage);
            Assert.True(sourceDisjointRun.Value!.MayStartTraining);
            Assert.True(sourceDisjointRun.Value.TrainingRunnerInvoked);
            Assert.Equal(2, sourceDisjointRun.Value.ResolvedTrainingRecordCount);
            Assert.Equal("proven", sourceDisjointRun.Value.SourceManifestStatus);
            Assert.Equal("proven", sourceDisjointRun.Value.SourceDisjointStatus);
            Assert.False(sourceDisjointRun.Value.TrainingStarted);
            Assert.False(sourceDisjointRun.Value.CheckpointMutated);
            Assert.False(sourceDisjointRun.Value.WeightsProduced);
            Assert.False(sourceDisjointRun.Value.ModelUploadReady);
            Assert.Equal("runner_invoked_count_or_source_manifest_gate_no_training_started", sourceDisjointRun.Value.TrainingExecutionBoundary);
            Assert.Equal("runner_invoked_training_blocked_dataset_count_gate", sourceDisjointRun.Value.TrainingExecutionStatus);
            Assert.Equal("dataset_count_gate_not_passed", sourceDisjointRun.Value.RunnerBlockedReason);
            var runnerEvidence = Assert.IsType<DesignConversionTrainingIntakeRunnerEvidence>(sourceDisjointRun.Value.RunnerEvidence);
            Assert.True(runnerEvidence.SourceDisjoint);
            Assert.Equal("proven", runnerEvidence.SourceManifestStatus);
            Assert.Equal("proven", runnerEvidence.SourceDisjointStatus);
            Assert.False(runnerEvidence.TrainingStarted);
            Assert.False(runnerEvidence.PromotionUploadAllowed);
            Assert.Equal(bool.FalseString, sourceDisjointRun.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal(bool.FalseString, sourceDisjointRun.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, sourceDisjointRun.Value.BoundaryMetadata["promotion_upload_attempted"]);
            Assert.DoesNotContain(privateMarker, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPackage.InitialPackage.Html!, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain(secondPackage.InitialPackage.Html!, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain(firstPackage.InitialPackage.DesignTree.Payload!, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain(secondPackage.InitialPackage.DesignTree.Payload!, serializedRun, StringComparison.Ordinal);
            Assert.DoesNotContain("external-provider-payload", serializedRun, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task Pass164R_LoopCreatedRuntimeCandidatesPassDefaultCountGatePromoteAndModelServerUsesCheckpoint()
    {
        const string privateMarker = "PRIVATE_PASS164R_COUNT_GATE_PROMOTION_SECRET";
        const int requiredRuntimeCandidateCount = 100;
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "pass164r-loop-training-promotion");
        Directory.CreateDirectory(root);
        try
        {
            var artifactDb = new JsonlDesignConversionLocalArtifactDb(root);
            var usefulnessApi = new DesignConversionDpoUsefulnessApi(artifactDb);
            var renderProvider = new RecordingRenderProvider();
            var comparisonProvider = new RecordingComparisonProvider(_ => 0.97);
            var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.97, 0m));
            var adapter = CreateAdapter(
                renderProvider,
                comparisonProvider,
                visualJudge,
                new ScriptedCandidateStrategy(),
                artifactDb);

            var first = await CreateApprovedPass164RRuntimeCandidateAsync(
                artifactDb,
                usefulnessApi,
                adapter,
                sourceOrdinal: 0,
                privateMarker);

            var singleSourceHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(JobId: first.Stored.JobId),
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(singleSourceHandoff.IsSuccess, singleSourceHandoff.ErrorMessage);
            Assert.Equal(1, singleSourceHandoff.Value!.ApprovedCandidateCount);

            var singleSourceIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                singleSourceHandoff.Value,
                "architect:pass164r-local",
                "Pass164R negative control: one loop-created runtime source must not start training",
                DateTimeOffset.UnixEpoch.AddMinutes(164));
            Assert.True(singleSourceIntake.IsSuccess, singleSourceIntake.ErrorMessage);

            var singleSourcePreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(singleSourceIntake.Value!.IntakeId);
            Assert.True(singleSourcePreflight.IsSuccess, singleSourcePreflight.ErrorMessage);
            Assert.False(singleSourcePreflight.Value!.MayStartTraining);
            Assert.Equal("blocked_same_source", singleSourcePreflight.Value.SourceDisjointStatus);
            Assert.Contains("source_disjoint_blocked_same_source", singleSourcePreflight.Value.BlockReasons);
            Assert.Equal(bool.FalseString, singleSourcePreflight.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, singleSourcePreflight.Value.BoundaryMetadata["product_ready"]);

            var traces = new List<Pass164RRuntimeCandidateTrace> { first };
            for (var sourceOrdinal = 1; sourceOrdinal < requiredRuntimeCandidateCount; sourceOrdinal++)
            {
                traces.Add(await CreateApprovedPass164RRuntimeCandidateAsync(
                    artifactDb,
                    usefulnessApi,
                    adapter,
                    sourceOrdinal,
                    privateMarker));
            }

            Assert.Equal(requiredRuntimeCandidateCount, traces.Count);
            Assert.Equal(
                requiredRuntimeCandidateCount,
                traces.Select(trace => trace.SourceManifestRef).Distinct(StringComparer.Ordinal).Count());
            Assert.Equal(
                requiredRuntimeCandidateCount,
                traces.Select(trace => trace.SourceStructurePayload).Distinct(StringComparer.Ordinal).Count());

            var sourceDisjointHandoff = await usefulnessApi.ExportApprovedRuntimeDpoCandidatesAsync(
                new DesignConversionLocalArtifactDbQuery(),
                DateTimeOffset.UnixEpoch.AddMinutes(166));
            Assert.True(sourceDisjointHandoff.IsSuccess, sourceDisjointHandoff.ErrorMessage);
            Assert.Equal(requiredRuntimeCandidateCount, sourceDisjointHandoff.Value!.ApprovedCandidateCount);
            Assert.Equal(requiredRuntimeCandidateCount, sourceDisjointHandoff.Value.Rows.Count);
            Assert.Equal(
                requiredRuntimeCandidateCount,
                sourceDisjointHandoff.Value.Rows
                    .Select(row => row.SourceManifestRef)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.Equal(
                requiredRuntimeCandidateCount,
                sourceDisjointHandoff.Value.Rows
                    .Select(row => row.SourceStructureHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.All(sourceDisjointHandoff.Value.Rows, row =>
            {
                Assert.True(row.TrainingHandoffOnly);
                Assert.Equal(bool.TrueString, row.BoundaryMetadata["runtime_created_dpo_candidate"]);
                Assert.Equal(bool.FalseString, row.BoundaryMetadata["production_dpo"]);
                Assert.Equal(bool.TrueString, row.BoundaryMetadata["product_not_ready"]);
                Assert.Equal(bool.FalseString, row.BoundaryMetadata["model_readiness_claimed"]);
            });

            var sourceDisjointIntake = await usefulnessApi.CreateTrainingIntakeAsync(
                sourceDisjointHandoff.Value,
                "architect:pass164r-local",
                "Pass164R loop-created source-disjoint runtime candidates pass existing default count gate and local promotion only",
                DateTimeOffset.UnixEpoch.AddMinutes(167));
            Assert.True(sourceDisjointIntake.IsSuccess, sourceDisjointIntake.ErrorMessage);
            Assert.Equal(requiredRuntimeCandidateCount, sourceDisjointIntake.Value!.AcceptedCandidateCount);
            Assert.Equal(
                requiredRuntimeCandidateCount,
                sourceDisjointIntake.Value.CandidateRefs
                    .Select(candidate => candidate.SourceIdentityHash)
                    .Distinct(StringComparer.Ordinal)
                    .Count());
            Assert.True(sourceDisjointIntake.Value.ProductNotReady);
            Assert.Equal(bool.FalseString, sourceDisjointIntake.Value.BoundaryMetadata["product_ready"]);
            Assert.Equal("proven", sourceDisjointIntake.Value.BoundaryMetadata["source_manifest_status"]);
            Assert.Equal("proven", sourceDisjointIntake.Value.BoundaryMetadata["source_disjoint_status"]);

            var sourceDisjointPreflight = await usefulnessApi.RunTrainingIntakePreflightAsync(sourceDisjointIntake.Value.IntakeId);
            Assert.True(sourceDisjointPreflight.IsSuccess, sourceDisjointPreflight.ErrorMessage);
            Assert.True(sourceDisjointPreflight.Value!.MayStartTraining);
            Assert.Equal("proven", sourceDisjointPreflight.Value.SourceManifestStatus);
            Assert.Equal("proven", sourceDisjointPreflight.Value.SourceDisjointStatus);
            Assert.Empty(sourceDisjointPreflight.Value.BlockReasons);
            Assert.Equal(requiredRuntimeCandidateCount.ToString(), sourceDisjointPreflight.Value.BoundaryMetadata["source_manifest_candidate_count"]);
            Assert.Equal(requiredRuntimeCandidateCount.ToString(), sourceDisjointPreflight.Value.BoundaryMetadata["source_manifest_hash_count"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["model_upload_ready"]);
            Assert.Equal(bool.FalseString, sourceDisjointPreflight.Value.BoundaryMetadata["product_ready"]);

            var baselineCheckpointPath = Path.Combine(root, "baseline-checkpoint.json");
            var baselineSave = await new DesignConversionMachine().SaveCheckpointAsync(baselineCheckpointPath);
            Assert.True(baselineSave.IsSuccess, baselineSave.ErrorMessage);
            var baselineSha = await ReadSha256Async(baselineCheckpointPath);

            var promoted = await usefulnessApi.RunTrainingIntakeAndPromoteLocalAsync(
                sourceDisjointIntake.Value.IntakeId,
                Path.Combine(root, "pass164r-ready-promotion"),
                Pass164RDefaultCountLocalPromotionOptions("pass164r-loop-created-default-count-promotion"));
            Assert.True(promoted.IsSuccess, promoted.ErrorMessage);
            Assert.True(promoted.Value!.TrainingRun.MayStartTraining);
            Assert.True(promoted.Value.TrainingRun.TrainingRunnerInvoked);
            Assert.True(promoted.Value.TrainingRun.LocalLifecycleHookAttempted);
            Assert.True(promoted.Value.TrainingRun.TrainingStarted);
            Assert.True(promoted.Value.TrainingRun.CheckpointMutated);
            Assert.True(promoted.Value.TrainingRun.CheckpointCandidateCreated);
            Assert.True(promoted.Value.TrainingRun.WeightsProduced);
            Assert.True(promoted.Value.TrainingRun.ModelUploadReady);
            Assert.Equal("proven", promoted.Value.TrainingRun.SourceManifestStatus);
            Assert.Equal("proven", promoted.Value.TrainingRun.SourceDisjointStatus);
            Assert.Equal("local_lifecycle_training_started_with_real_learned_weights", promoted.Value.TrainingRun.TrainingExecutionStatus);
            Assert.Equal("none", promoted.Value.TrainingRun.RunnerBlockedReason);
            Assert.Equal(DesignConversionMeasurementGateStatus.Passed, promoted.Value.TrainingRun.MeasurementGateStatus);
            Assert.Equal("PROMOTION_ALLOWED_BY_MEASUREMENT_GATE_AND_REAL_WEIGHTS", promoted.Value.TrainingRun.PromotionGateStatus);
            Assert.Equal("upload_ready_local_promotion_gate_passed_no_external_upload", promoted.Value.TrainingRun.UploadReadinessStatus);

            var runnerEvidence = Assert.IsType<DesignConversionTrainingIntakeRunnerEvidence>(promoted.Value.TrainingRun.RunnerEvidence);
            Assert.True(runnerEvidence.DatasetCountGatePassed);
            Assert.Equal(50, runnerEvidence.RequiredTrainRecords);
            Assert.Equal(50, runnerEvidence.RequiredHeldOutRecords);
            Assert.Equal(50, runnerEvidence.ActualTrainRecords);
            Assert.Equal(50, runnerEvidence.ActualHeldOutRecords);
            Assert.True(runnerEvidence.SourceDisjoint);
            Assert.True(runnerEvidence.CheckpointContainsRealTrainedWeights);
            Assert.Equal(DesignConversionMeasurementGateStatus.Passed, runnerEvidence.MeasurementGateStatus);
            Assert.True(runnerEvidence.PromotionUploadAllowed);

            Assert.True(promoted.Value.LocalPromotionReady);
            Assert.False(promoted.Value.ExternalCloudUploadAttempted);
            Assert.NotNull(promoted.Value.Publication);
            Assert.NotNull(promoted.Value.Publication!.UploadReceipt);
            Assert.StartsWith(
                DesignConversionPromotedLocalModelLoader.LocalOnlyPromotionRefPrefix,
                promoted.Value.Publication.Manifest.PromotionUploadRef,
                StringComparison.Ordinal);
            Assert.NotNull(promoted.Value.PromotedLocalModel);
            Assert.True(File.Exists(promoted.Value.PromotedCheckpointPath));
            Assert.NotEqual(baselineSha, promoted.Value.PromotedLocalModel!.CheckpointSha256);
            Assert.NotEqual("0.1.0", promoted.Value.PromotedLocalModel.Version);
            Assert.True(promoted.Value.PromotedLocalModel.LearnedWeightArtifactCount > 0);
            Assert.True(promoted.Value.PromotedLocalModel.LearnedWeightObservationCount > 0);
            Assert.True(promoted.Value.PromotedLocalModel.CheckpointFreshLoadVerified);
            Assert.True(promoted.Value.PromotedLocalModel.CheckpointContainsRealTrainedWeights);
            Assert.True(promoted.Value.PromotedLocalModel.LocalOnly);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["local_promotion_ready"]);
            Assert.Equal(bool.FalseString, promoted.Value.BoundaryMetadata["external_cloud_upload_attempted"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["external_cloud_upload_not_attempted"]);
            Assert.Equal(bool.FalseString, promoted.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["cloud_model_upload_not_attempted"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["model_server_upload_not_attempted_local_only"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_only"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["local_promotion_receipt_written"]);
            Assert.Equal("local_receipt_only", promoted.Value.BoundaryMetadata["promotion_receipt_kind"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["model_server_upload_not_attempted_local_receipt"]);
            Assert.Equal(bool.TrueString, promoted.Value.BoundaryMetadata["model_server_promotion_not_claimed_local_receipt"]);
            Assert.DoesNotContain("model_server_upload_attempted_local_only", promoted.Value.BoundaryMetadata.Keys);
            Assert.DoesNotContain("model_server_promotion_claimed_local_only", promoted.Value.BoundaryMetadata.Keys);

            var promotedLoad = await DesignConversionPromotedLocalModelLoader.LoadAsync(
                promoted.Value.Publication,
                promoted.Value.PromotedCheckpointPath);
            Assert.True(promotedLoad.IsSuccess, promotedLoad.ErrorMessage);

            var service = DesignConversionPromotedLocalModelLoader.CreateInternalService(
                promotedLoad.Value!,
                new RecordingRenderProvider(),
                new RecordingComparisonProvider(_ => 0.96),
                new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0m)));
            var modelServer = new DesignConversionModelServerApi(
                service,
                () => DateTimeOffset.Parse("2026-07-03T12:00:00Z"),
                trainingReadbackApi: usefulnessApi,
                promotedLocalModel: promoted.Value.PromotedLocalModel);

            var response = await modelServer.SubmitAndEvaluateAsync(CreatePass164RModelServerRequest("pass164r-promoted-modelserver-request"));
            Assert.True(response.IsSuccess, response.ErrorMessage);
            Assert.True(response.Value!.LearnedInferenceApplied);
            Assert.True(response.Value.ModelTrainingVerified);
            Assert.False(response.Value.ModelUploadReady);
            Assert.False(response.Value.DeploymentVerified);
            Assert.False(response.Value.ProductReady);
            Assert.Equal("promoted_local_checkpoint_loaded", response.Value.TrainingReadiness.Status);
            Assert.Equal("local_promoted_checkpoint_loaded", response.Value.TrainingReadiness.PromotionStatus);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, response.Value.PromotedLocalModel!.ModelId);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, response.Value.BoundaryMetadata["local_promoted_model_id"]);
            Assert.Equal(promoted.Value.PromotedLocalModel.Version, response.Value.BoundaryMetadata["local_promoted_model_version"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_model_promotion_verified"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_loaded"]);
            Assert.Equal(bool.TrueString, response.Value.BoundaryMetadata["local_promoted_checkpoint_real_weights"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["cloud_model_upload_attempted"]);
            Assert.Equal(bool.FalseString, response.Value.BoundaryMetadata["product_ready"]);
            Assert.Contains(DesignConversionMachine.LearnedInferenceAppliedEvidence, response.Value.EvidenceRefs);

            var status = modelServer.GetJobResult(response.Value.JobId);
            Assert.True(status.IsSuccess, status.ErrorMessage);
            Assert.True(status.Value!.LearnedInferenceApplied);
            Assert.Equal(promoted.Value.PromotedLocalModel.ModelId, status.Value.BoundaryMetadata["local_promoted_model_id"]);
            Assert.False(status.Value.DeploymentVerified);
            Assert.False(status.Value.ProductReady);

            var serializedPromotion = JsonSerializer.Serialize(promoted.Value);
            var serializedBoundary = JsonSerializer.Serialize(response.Value.BoundaryMetadata);
            foreach (var payload in new[] { serializedPromotion, serializedBoundary })
            {
                Assert.DoesNotContain(privateMarker, payload, StringComparison.Ordinal);
                Assert.DoesNotContain("<main", payload, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("C:/Users/", payload, StringComparison.OrdinalIgnoreCase);
                Assert.DoesNotContain("external-provider-payload", payload, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_ProviderRequired_ReturnsBoundedNonReadyEnvelopeWithoutExternalProviderClaim()
    {
        const string privateMarker = "PRIVATE_PASS129_PROVIDER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.40);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromBlockedProvider());
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-provider-required-loop",
            InitialPackage: CreatePackage("pass129-provider-required-request", privateMarker),
            MaxAttempts: 3);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.ProviderRequired, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.True(root.GetProperty("providerRequiredOrBlocked").GetBoolean());
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_ai_visual_judge_executed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_BlockingRenderDeadlineReturnsTypedDeadlineEnvelopeWithoutAttempt()
    {
        const string privateMarker = "PRIVATE_DEADLINE_MODEL_SERVER_SECRET";
        var renderProvider = new BlockingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            runDeadline: TimeSpan.FromMilliseconds(50));
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "modelserver-deadline-loop",
            InitialPackage: CreatePackage("modelserver-deadline-request", privateMarker),
            MaxAttempts: 3);

        var stopwatch = Stopwatch.StartNew();
        var envelope = await adapter.TryRunImprovementLoopAsync(package);
        stopwatch.Stop();

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage!.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.DesignTree!.Payload!, envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, root.GetProperty("stopReason").GetString());
        Assert.True(root.GetProperty("deadlineExceeded").GetBoolean());
        Assert.False(root.GetProperty("cancelled").GetBoolean());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("candidateStrategyFailed").GetBoolean());
        Assert.Equal(0, root.GetProperty("attemptCount").GetInt32());
        Assert.Empty(root.GetProperty("attempts").EnumerateArray());
        Assert.Contains(
            DesignConversionImprovementLoopCoordinator.DeadlineExceededMarker,
            root.GetProperty("evidenceRefs").EnumerateArray().Select(reference => reference.GetString()));
        Assert.Contains(
            "IMPROVEMENT_LOOP_DEADLINE_EXCEEDED_DURING_EVALUATION",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));

        Assert.Single(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.DeadlineExceeded, envelope.BoundaryMetadata["stop_reason"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["completed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cancelled"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["refs_or_hashes_only"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_html_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_css_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["raw_design_tree_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["provider_payload_included"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(2), $"deadline elapsed {stopwatch.Elapsed}");
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CandidateNoOp_PropagatesNoProgressStopAsBoundedNonReady()
    {
        const string privateMarker = "PRIVATE_NO_OP_CANDIDATE_SECRET";
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "candidate-no-op-loop",
            InitialPackage: CreatePackage("candidate-no-op-request", privateMarker),
            MaxAttempts: 3);
        var noOpCandidate = new DesignConversionImprovementCandidate(
            CandidateId: "candidate-no-op-same-html-css",
            RequestId: "candidate-no-op-next-request",
            Html: package.InitialPackage!.Html!,
            Css: package.InitialPackage.Css!,
            ChangeReasons: ["scripted no-op candidate"],
            EvidenceRefs: ["candidate-no-op:evidence"],
            Metadata: new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker
            });
        var strategy = new ScriptedCandidateStrategy(
            OperationResult<DesignConversionImprovementCandidate>.Ok(noOpCandidate));
        var adapter = CreateAdapter(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(_ => 0.72),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m)),
            strategy);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        Assert.Null(envelope.ParsedErrorCode);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Html!, envelope.ResponseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(package.InitialPackage.Css!, envelope.ResponseBodyJson, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.False(root.GetProperty("candidateStrategyFailed").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());
        Assert.Contains(
            "IMPROVEMENT_LOOP_CANDIDATE_VALIDATION_NO_PROGRESS:IMPROVEMENT_CANDIDATE_NO_OP",
            root.GetProperty("warnings").EnumerateArray().Select(warning => warning.GetString()));
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["candidate_strategy_failed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["completed"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["quality_reached"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(DesignConversionImprovementLoopCoordinator.NoProgressStop, envelope.BoundaryMetadata["stop_reason"]);
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_UnsupportedArtifactProofRefs_FailLocalValidationBeforeRuntime()
    {
        var adapter = CreateAdapter(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(_ => 0.72),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m)),
            new ScriptedCandidateStrategy());
        var initialPackage = CreatePackage(
            "pass129-unsupported-artifact-loop-request",
            "PRIVATE_PASS129_UNSUPPORTED_ARTIFACT_SECRET");
        initialPackage = initialPackage with
        {
            Screenshots =
            [
                initialPackage.Screenshots![0] with
                {
                    ArtifactId = "unsupported-improvement-loop-screenshot-proof",
                    Uri = "https://client.example.invalid/improvement-loop-screenshot-proof.png",
                    Sha256 = "sha256:unsupported-improvement-loop-screenshot-proof"
                }
            ],
            DesignRenders = []
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-unsupported-artifact-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal("DESIGN_CONVERSION_ARTIFACT_REF_PROOF_REQUIRED", envelope.ParsedErrorCode);
        Assert.Contains("refs-only", envelope.ParsedErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_OversizedInitialPackageFailsLocalValidationBeforeRuntime()
    {
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(
            renderProvider,
            comparisonProvider,
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m)),
            strategy);
        var initialPackage = CreatePackage(
            "pass129-oversized-payload-loop-request",
            "PRIVATE_PASS129_OVERSIZED_PAYLOAD_SECRET") with
        {
            DesignTree = new DesignConversionModelServerPayloadRef(
                RefId: "pass129-oversized-payload-loop-request:design-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass129-oversized-payload-loop-request:design-tree",
                Payload: new string('t', DesignConversionModelServerPayloadSizeGuard.MaxRawPayloadChars + 1),
                Format: "figma-json")
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-oversized-payload-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionModelServerPayloadSizeGuard.ErrorCode, envelope.ParsedErrorCode);
        Assert.Contains("designTree.payload", envelope.ParsedErrorMessage, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("")]
    public async Task TryRunImprovementLoopAsync_UnknownTierZeroMaxImprovementCandidatesFailsLocalValidationBeforeRegistry(
        string tier)
    {
        const string privateMarker = "PRIVATE_PASS_CANDIDATE_BUDGET_ADAPTER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "pass129-candidate-budget-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                Tier = tier,
                MaxImprovementCandidates = 0,
                BudgetUsd = 0m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-candidate-budget-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, envelope.ParsedErrorCode);
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public void DescribeImprovementLoopRoute_ValidationFailuresDocumentBudgetValidationRules()
    {
        var validationFailures = DesignConversionModelServerCallerContract
            .DescribeImprovementLoopRoute()
            .ValidationFailures;

        var negativeBudgetUsdFailure = Assert.Single(
            validationFailures,
            failure => failure.ErrorCode == DesignConversionInternalService.NegativeBudgetUsdErrorCode);
        Assert.Equal(400, negativeBudgetUsdFailure.StatusCode);
        Assert.Contains("negative", negativeBudgetUsdFailure.Trigger, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("budgetUsd", negativeBudgetUsdFailure.Trigger, StringComparison.Ordinal);

        var improvementCandidateBudgetFailure = Assert.Single(
            validationFailures,
            failure => failure.ErrorCode == DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode);
        Assert.Equal(400, improvementCandidateBudgetFailure.StatusCode);
        Assert.Contains("negative", improvementCandidateBudgetFailure.Trigger, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("maxImprovementCandidates", improvementCandidateBudgetFailure.Trigger, StringComparison.Ordinal);
        Assert.Contains("zero", improvementCandidateBudgetFailure.Trigger, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "server-owned entitlement tier",
            improvementCandidateBudgetFailure.Trigger,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_NegativeMaxImprovementCandidatesFailsLocalValidationBeforeRegistry()
    {
        const string privateMarker = "PRIVATE_PASS_NEGATIVE_CANDIDATE_BUDGET_ADAPTER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "pass129-negative-candidate-budget-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                MaxImprovementCandidates = -1
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-negative-candidate-budget-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, envelope.ParsedErrorCode);
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_NegativeBudgetUsdFailsLocalValidationBeforeRegistry()
    {
        const string privateMarker = "PRIVATE_PASS_NEGATIVE_BUDGET_ADAPTER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "pass129-negative-budget-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                BudgetUsd = -0.01m
            }
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-negative-budget-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(DesignConversionInternalService.NegativeBudgetUsdErrorCode, envelope.ParsedErrorCode);
        Assert.Equal(
            "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
            envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_FreeTierZeroBudgetAndMaxUsesEntitlementDefaultsThroughRegistry()
    {
        const string privateMarker = "PRIVATE_PASS_FREE_ZERO_ENTITLEMENT_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.70);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.70, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("pass-free-zero-entitlement-candidate", privateMarker)));
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var initialPackage = CreatePackage(
            "pass-free-zero-entitlement-adapter-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: " free ",
                MaxImprovementCandidates: 0,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0m,
                Metadata: new Dictionary<string, string>
                {
                    ["budget_private_marker"] = privateMarker
                })
        };
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass-free-zero-entitlement-adapter-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 3,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal("success", envelope.EnvelopeKind);
        Assert.Equal("bounded_non_ready", envelope.ParsedContractStatus);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        using var document = JsonDocument.Parse(envelope.ResponseBodyJson);
        var root = document.RootElement;
        Assert.Equal("bounded_non_ready", root.GetProperty("contractStatus").GetString());
        Assert.Equal(DesignConversionVisualQaBoundaryPolicy.BudgetCapReached, root.GetProperty("stopReason").GetString());
        Assert.False(root.GetProperty("completed").GetBoolean());
        Assert.False(root.GetProperty("qualityReached").GetBoolean());
        Assert.True(root.GetProperty("budgetCapReached").GetBoolean());
        Assert.Equal(1, root.GetProperty("attemptCount").GetInt32());

        var attempt = Assert.Single(root.GetProperty("attempts").EnumerateArray());
        var attemptMetadata = attempt.GetProperty("publicMetadata");
        Assert.Equal("free", attemptMetadata.GetProperty("tier").GetString());
        Assert.Equal("0", attemptMetadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("0", attemptMetadata.GetProperty("submitted_max_improvement_candidates").GetString());
        Assert.Equal("0.05", attemptMetadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal("1", attemptMetadata.GetProperty("effective_max_improvement_candidates").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("tier_policy_budget_entitlement_default_applied").GetString());
        Assert.Equal(bool.TrueString, attemptMetadata.GetProperty("tier_policy_iteration_entitlement_default_applied").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("tier_policy_budget_cap_applied").GetString());
        Assert.Equal(bool.FalseString, attemptMetadata.GetProperty("tier_policy_iteration_cap_applied").GetString());
        Assert.Equal("iteration_budget", attemptMetadata.GetProperty("budget_stop_source").GetString());

        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.TrueString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal("0", metadata.GetProperty("submitted_budget_usd").GetString());
        Assert.Equal("0", metadata.GetProperty("submitted_max_improvement_candidates").GetString());
        Assert.Equal("0.05", metadata.GetProperty("effective_budget_usd").GetString());
        Assert.Equal("1", metadata.GetProperty("effective_max_improvement_candidates").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("tier_policy_budget_entitlement_default_applied").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("tier_policy_iteration_entitlement_default_applied").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("tier_policy_budget_cap_applied").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("tier_policy_iteration_cap_applied").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Equal("0", envelope.BoundaryMetadata["submitted_budget_usd"]);
        Assert.Equal("0", envelope.BoundaryMetadata["submitted_max_improvement_candidates"]);
        Assert.Equal("0.05", envelope.BoundaryMetadata["effective_budget_usd"]);
        Assert.Equal("1", envelope.BoundaryMetadata["effective_max_improvement_candidates"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["tier_policy_budget_entitlement_default_applied"]);
        Assert.Equal(bool.TrueString, envelope.BoundaryMetadata["tier_policy_iteration_entitlement_default_applied"]);

        Assert.Single(renderProvider.Requests.Select(providerRequest => providerRequest.Metadata["job_id"]).Distinct(StringComparer.Ordinal));
        Assert.Equal(2, renderProvider.Requests.Count);
        Assert.Equal(2, comparisonProvider.Requests.Count);
        Assert.Equal(2, visualJudge.Requests.Count);
        Assert.Empty(strategy.Requests);
    }

    [Theory]
    [InlineData("enterprise")]
    [InlineData("")]
    public async Task HandleAsync_ZeroMaxImprovementCandidatesForUnknownOrBlankTierFailsBeforeRuntimeWhenAdapterBypassed(
        string tier)
    {
        const string privateMarker = "PRIVATE_PASS_UNKNOWN_ZERO_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var initialPackage = CreatePackage(
            "pass129-unknown-zero-handler-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                Tier = tier,
                MaxImprovementCandidates = 0,
                BudgetUsd = 0m
            }
        };
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass129-unknown-zero-handler-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await handler.HandleAsync(request);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, loopRequest.InitialPackage!);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            root.GetProperty("errorMessage").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_NegativeBudgetUsdFailsBeforeRuntimeWhenAdapterBypassed()
    {
        const string privateMarker = "PRIVATE_PASS_NEGATIVE_BUDGET_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var initialPackage = CreatePackage(
            "pass129-negative-budget-handler-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                Tier = "free",
                MaxImprovementCandidates = 1,
                BudgetUsd = -0.01m
            }
        };
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass129-negative-budget-handler-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await handler.HandleAsync(request);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, loopRequest.InitialPackage!);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionModelServerImprovementLoopHandler.NegativeBudgetUsdErrorCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(
            "DesignConversion improvement-loop budgetUsd must be greater than or equal to zero.",
            root.GetProperty("errorMessage").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_NegativeMaxImprovementCandidatesFailsBeforeRuntimeWhenAdapterBypassed()
    {
        const string privateMarker = "PRIVATE_PASS_CANDIDATE_BUDGET_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var initialPackage = CreatePackage(
            "pass129-candidate-budget-handler-request",
            privateMarker);
        initialPackage = initialPackage with
        {
            Budget = initialPackage.Budget! with
            {
                MaxImprovementCandidates = -1
            }
        };
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass129-candidate-budget-handler-loop",
            InitialPackage: initialPackage,
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await handler.HandleAsync(request);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, loopRequest.InitialPackage!);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(DesignConversionInternalService.ImprovementCandidateBudgetRequiredCode, root.GetProperty("errorCode").GetString());
        Assert.Equal(
            "DesignConversion improvement candidate budget must be greater than zero.",
            root.GetProperty("errorMessage").GetString());
        var metadata = root.GetProperty("boundaryMetadata");
        Assert.Equal(bool.FalseString, metadata.GetProperty("runtime_invoked").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_html_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_css_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("raw_design_tree_included").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("response_contains_raw_private_data").GetString());
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_MaxAttemptsAboveCoordinatorCapFailsLocalValidationBeforeRuntime()
    {
        const string privateMarker = "PRIVATE_PASS_MAX_ATTEMPTS_ADAPTER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var adapter = CreateAdapter(renderProvider, comparisonProvider, visualJudge, strategy);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-max-attempts-adapter-loop",
            InitialPackage: CreatePackage("pass129-max-attempts-adapter-request", privateMarker),
            MaxAttempts: DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts + 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 400,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode,
            envelope.ParsedErrorCode);
        Assert.Equal(ExpectedMaxAttemptsExceededErrorMessage(), envelope.ParsedErrorMessage);
        AssertNoPrivateMarkerLeak(envelope.ResponseBodyJson, privateMarker, package.InitialPackage!);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.Equal("local_improvement_loop_package_validation", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_MaxAttemptsAboveCoordinatorCapFailsBeforeRuntimeWhenAdapterBypassed()
    {
        const string privateMarker = "PRIVATE_PASS_MAX_ATTEMPTS_HANDLER_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.72);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        var handler = new DesignConversionModelServerImprovementLoopHandler(coordinator);
        var loopRequest = new DesignConversionModelServerImprovementLoopRequest(
            LoopId: "pass129-max-attempts-handler-loop",
            InitialPackage: CreatePackage("pass129-max-attempts-handler-request", privateMarker),
            MaxAttempts: DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts + 1,
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });
        var request = new DesignConversionModelServerHttpRequest(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: JsonSerializer.Serialize(loopRequest, DesignConversionModelServerHttpHandler.JsonOptions),
            ContentType: "application/json",
            Headers: new Dictionary<string, string>(StringComparer.Ordinal));

        var response = await handler.HandleAsync(request);

        Assert.Equal(400, response.StatusCode);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
        Assert.DoesNotContain(privateMarker, response.BodyJson, StringComparison.Ordinal);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, loopRequest.InitialPackage!);
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.MaxAttemptsExceededErrorCode,
            root.GetProperty("errorCode").GetString());
        Assert.Equal(ExpectedMaxAttemptsExceededErrorMessage(), root.GetProperty("errorMessage").GetString());
        Assert.Equal(bool.FalseString, root.GetProperty("boundaryMetadata").GetProperty("runtime_invoked").GetString());
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_EnabledAdmissionRejectsMissingAndInvalidAuthBeforeImprovementLoopRuntimeWithoutLeakage()
    {
        const string privateMarker = "PRIVATE_N25_4A1_AUTH_SECRET";
        const string idempotencyKey = "n25-4a1-auth-key";
        const string invalidToken = "wrong-improvement-loop-route-token-0123456789";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var routeAdmission = CreateEnabledImprovementLoopAdmission();
        var handler = CreateAdmissionBackedHandler(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            routeAdmission);
        var package = CreatePackage("n25-4a1-auth-request", privateMarker);
        var bodyJson = SerializeLoopRequest(
            "n25-4a1-auth-loop",
            package,
            maxAttempts: 1,
            privateMarker);

        var missingAuth = await handler.HandleAsync(ImprovementLoopHttpRequest(
            bodyJson,
            AdmissionHeaders(token: null, idempotencyKey: idempotencyKey)));
        var invalidAuth = await handler.HandleAsync(ImprovementLoopHttpRequest(
            bodyJson,
            AdmissionHeaders(token: invalidToken, idempotencyKey: idempotencyKey)));

        Assert.Equal(401, missingAuth.StatusCode);
        Assert.Equal(403, invalidAuth.StatusCode);
        AssertError(missingAuth, "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_REQUIRED");
        AssertError(invalidAuth, "DESIGN_CONVERSION_ROUTE_ADMISSION_TOKEN_INVALID");
        AssertRuntimeNotInvoked(missingAuth);
        AssertRuntimeNotInvoked(invalidAuth);
        AssertNoPrivateMarkerLeak(missingAuth.BodyJson, privateMarker, package);
        AssertNoPrivateMarkerLeak(invalidAuth.BodyJson, privateMarker, package);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            missingAuth,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            idempotencyKey,
            privateMarker,
            package.Html!,
            package.Css!,
            package.DesignTree!.Payload!,
            package.OriginalTree!.Payload!);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            invalidAuth,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            idempotencyKey,
            invalidToken,
            privateMarker,
            package.Html!,
            package.Css!,
            package.DesignTree!.Payload!,
            package.OriginalTree!.Payload!);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_EnabledAdmissionRejectsMissingIdempotencyKeyWithLoopSpecificDenialBeforeRuntime()
    {
        const string privateMarker = "PRIVATE_N25_4A1_MISSING_IDEMPOTENCY_SECRET";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var handler = CreateAdmissionBackedHandler(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            CreateEnabledImprovementLoopAdmission());
        var package = CreatePackage("n25-4a1-missing-idempotency-request", privateMarker);
        var bodyJson = SerializeLoopRequest(
            "n25-4a1-missing-idempotency-loop",
            package,
            maxAttempts: 1,
            privateMarker);

        var response = await handler.HandleAsync(ImprovementLoopHttpRequest(
            bodyJson,
            AdmissionHeaders(idempotencyKey: null)));

        Assert.Equal(400, response.StatusCode);
        AssertError(
            response,
            "DESIGN_CONVERSION_IDEMPOTENCY_KEY_REQUIRED",
            "DesignConversion improvement-loop endpoint requires an Idempotency-Key header.");
        AssertRuntimeNotInvoked(response);
        AssertNoPrivateMarkerLeak(response.BodyJson, privateMarker, package);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            response,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            privateMarker,
            package.Html!,
            package.Css!,
            package.DesignTree!.Payload!,
            package.OriginalTree!.Payload!);
        Assert.Empty(renderProvider.Requests);
        Assert.Empty(comparisonProvider.Requests);
        Assert.Empty(visualJudge.Requests);
        Assert.Empty(strategy.Requests);
    }

    [Fact]
    public async Task HandleAsync_EnabledAdmissionReplaysSameCanonicalImprovementLoopBodyOnceWithSafeRouteMetadata()
    {
        const string privateMarker = "PRIVATE_N25_4A1_REPLAY_SECRET";
        const string idempotencyKey = "n25-4a1-replay-key";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(request =>
            request.ComparisonRequestId.Contains("loop-attempt:2", StringComparison.Ordinal) ? 0.96 : 0.72);
        var visualJudge = new ScriptedVisualJudge(request =>
            request.JudgeRequestId.Contains("loop-attempt:2", StringComparison.Ordinal)
                ? JudgeScript.FromScore(0.96, 0.01m)
                : JudgeScript.FromScore(0.72, 0.01m));
        var strategy = new ScriptedCandidateStrategy(OperationResult<DesignConversionImprovementCandidate>.Ok(
            CreateCandidate("n25-4a1-replay-candidate", privateMarker)));
        var handler = CreateAdmissionBackedHandler(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            CreateEnabledImprovementLoopAdmission());
        var package = CreatePackage("n25-4a1-replay-request", privateMarker);
        var bodyJson = SerializeLoopRequest(
            "n25-4a1-replay-loop",
            package,
            maxAttempts: 2,
            privateMarker);
        var request = ImprovementLoopHttpRequest(
            bodyJson,
            AdmissionHeaders(idempotencyKey: idempotencyKey));

        var first = await handler.HandleAsync(request);
        var replay = await handler.HandleAsync(request);

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(200, replay.StatusCode);
        Assert.Equal(first.BodyJson, replay.BodyJson);
        Assert.Equal(4, renderProvider.Requests.Count);
        Assert.Equal(4, comparisonProvider.Requests.Count);
        Assert.Equal(4, visualJudge.Requests.Count);
        Assert.Single(strategy.Requests);
        AssertNoPrivateMarkerLeak(first.BodyJson, privateMarker, package);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            first,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            idempotencyKey,
            privateMarker,
            package.Html!,
            package.Css!,
            package.DesignTree!.Payload!,
            package.OriginalTree!.Payload!);

        var metadata = BoundaryMetadata(first);
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_authorized").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_token_accepted").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("idempotency_key_accepted").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("idempotency_route_scoped").GetString());
        Assert.Equal(bool.TrueString, metadata.GetProperty("route_admission_tenant_bound").GetString());
        Assert.Equal(
            DesignConversionModelActivationRegistry.BuildTenantIdRef(RouteAdmissionTenantId),
            metadata.GetProperty("route_admission_tenant_id_ref").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("route_admission_raw_tenant_id_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_payload_digest_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_route_method_path_exposed").GetString());
        Assert.Equal(bool.FalseString, metadata.GetProperty("idempotency_effective_scope_contains_raw_route").GetString());
        Assert.Equal("ref_only", metadata.GetProperty("route_admission_method").GetString());
        Assert.Equal("ref_only", metadata.GetProperty("route_admission_path").GetString());
        Assert.StartsWith("sha256:", metadata.GetProperty("idempotency_effective_scope_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("idempotency_route_scope_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("idempotency_route_method_ref").GetString(), StringComparison.Ordinal);
        Assert.StartsWith("sha256:", metadata.GetProperty("idempotency_route_path_ref").GetString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task HandleAsync_EnabledAdmissionConflictsSameKeyDifferentCanonicalLoopBodyWithoutSecondExecutionOrLeakage()
    {
        const string firstPrivateMarker = "PRIVATE_N25_4A1_CONFLICT_FIRST_SECRET";
        const string secondPrivateMarker = "PRIVATE_N25_4A1_CONFLICT_SECOND_SECRET";
        const string idempotencyKey = "n25-4a1-conflict-key";
        var renderProvider = new RecordingRenderProvider();
        var comparisonProvider = new RecordingComparisonProvider(_ => 0.96);
        var visualJudge = new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m));
        var strategy = new ScriptedCandidateStrategy();
        var handler = CreateAdmissionBackedHandler(
            renderProvider,
            comparisonProvider,
            visualJudge,
            strategy,
            CreateEnabledImprovementLoopAdmission());
        var firstPackage = CreatePackage("n25-4a1-conflict-first-request", firstPrivateMarker);
        var secondPackage = CreatePackage("n25-4a1-conflict-second-request", secondPrivateMarker);

        var first = await handler.HandleAsync(ImprovementLoopHttpRequest(
            SerializeLoopRequest("n25-4a1-conflict-loop", firstPackage, maxAttempts: 1, firstPrivateMarker),
            AdmissionHeaders(idempotencyKey: idempotencyKey)));
        var renderCountAfterFirst = renderProvider.Requests.Count;
        var comparisonCountAfterFirst = comparisonProvider.Requests.Count;
        var visualJudgeCountAfterFirst = visualJudge.Requests.Count;
        var strategyCountAfterFirst = strategy.Requests.Count;
        var conflict = await handler.HandleAsync(ImprovementLoopHttpRequest(
            SerializeLoopRequest("n25-4a1-conflict-loop-changed", secondPackage, maxAttempts: 1, secondPrivateMarker),
            AdmissionHeaders(idempotencyKey: idempotencyKey)));

        Assert.Equal(200, first.StatusCode);
        Assert.Equal(409, conflict.StatusCode);
        AssertError(conflict, "DESIGN_CONVERSION_IDEMPOTENCY_KEY_CONFLICT");
        Assert.Equal(renderCountAfterFirst, renderProvider.Requests.Count);
        Assert.Equal(comparisonCountAfterFirst, comparisonProvider.Requests.Count);
        Assert.Equal(visualJudgeCountAfterFirst, visualJudge.Requests.Count);
        Assert.Equal(strategyCountAfterFirst, strategy.Requests.Count);
        AssertNoPrivateMarkerLeak(conflict.BodyJson, firstPrivateMarker, firstPackage);
        AssertNoPrivateMarkerLeak(conflict.BodyJson, secondPrivateMarker, secondPackage);
        AssertResponseAndBoundaryMetadataDoNotLeak(
            conflict,
            RouteAdmissionCurrentToken,
            RouteAdmissionPreviousToken,
            RouteAdmissionTenantId,
            idempotencyKey,
            firstPrivateMarker,
            secondPrivateMarker,
            firstPackage.Html!,
            firstPackage.Css!,
            firstPackage.DesignTree!.Payload!,
            firstPackage.OriginalTree!.Payload!,
            secondPackage.Html!,
            secondPackage.Css!,
            secondPackage.DesignTree!.Payload!,
            secondPackage.OriginalTree!.Payload!);
    }

    [Fact]
    public void RegisterDesignConversionImprovementLoop_StoresExactRouteAdmissionForHandlerAndCompositionRegistrations()
    {
        var routeAdmission = CreateEnabledImprovementLoopAdmission();
        var directRegistry = new DesignConversionModelServerEndpointRegistry();
        var directHandler = CreateAdmissionBackedHandler(
            new RecordingRenderProvider(),
            new RecordingComparisonProvider(_ => 0.96),
            new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m)),
            new ScriptedCandidateStrategy(),
            routeAdmission);

        var directRegistered = directRegistry.RegisterDesignConversionImprovementLoop(directHandler);
        var directResolved = directRegistry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);

        Assert.True(directRegistered.IsSuccess, directRegistered.ErrorMessage);
        Assert.True(directResolved.IsSuccess, directResolved.ErrorMessage);
        Assert.Same(routeAdmission, directRegistered.Value!.RouteAdmission);
        Assert.Same(routeAdmission, directResolved.Value!.RouteAdmission);

        var compositionAdmission = CreateEnabledImprovementLoopAdmission(tenantId: "tenant-improvement-loop-composition");
        var composition = DesignConversionLocalServiceComposition.CreateDeterministicLocal(
            comparisonProvider: new RecordingComparisonProvider(_ => 0.96),
            visualJudge: new ScriptedVisualJudge(_ => JudgeScript.FromScore(0.96, 0.01m)));
        var compositionRegistry = new DesignConversionModelServerEndpointRegistry();
        var compositionRegistered = compositionRegistry.RegisterDesignConversionImprovementLoopFromComposition(
            composition,
            new ScriptedCandidateStrategy(),
            routeAdmission: compositionAdmission);
        var compositionResolved = compositionRegistry.Resolve(
            DesignConversionModelServerEndpointRegistry.EndpointMethod,
            DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);

        Assert.True(compositionRegistered.IsSuccess, compositionRegistered.ErrorMessage);
        Assert.True(compositionResolved.IsSuccess, compositionResolved.ErrorMessage);
        Assert.Same(compositionAdmission, compositionRegistered.Value!.RouteAdmission);
        Assert.Same(compositionAdmission, compositionResolved.Value!.RouteAdmission);

        var artifactAdmission = CreateEnabledImprovementLoopAdmission(tenantId: "tenant-improvement-loop-artifact");
        var artifactRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "n25-4a1-registry");
        Directory.CreateDirectory(artifactRoot);
        try
        {
            var artifactRegistry = new DesignConversionModelServerEndpointRegistry();
            var artifactRegistered = artifactRegistry.RegisterDesignConversionImprovementLoopFromComposition(
                composition,
                new ScriptedCandidateStrategy(),
                new JsonlDesignConversionLocalArtifactDb(artifactRoot),
                utcNow: () => RouteAdmissionNow,
                routeAdmission: artifactAdmission);
            var artifactResolved = artifactRegistry.Resolve(
                DesignConversionModelServerEndpointRegistry.EndpointMethod,
                DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath);

            Assert.True(artifactRegistered.IsSuccess, artifactRegistered.ErrorMessage);
            Assert.True(artifactResolved.IsSuccess, artifactResolved.ErrorMessage);
            Assert.Same(artifactAdmission, artifactRegistered.Value!.RouteAdmission);
            Assert.Same(artifactAdmission, artifactResolved.Value!.RouteAdmission);
        }
        finally
        {
            Directory.Delete(artifactRoot, recursive: true);
        }
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_RouteMissing_ReturnsStableRouteErrorEnvelope()
    {
        var registry = new DesignConversionModelServerEndpointRegistry();
        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-route-missing-loop",
            InitialPackage: CreatePackage("pass129-route-missing-request", "PRIVATE_PASS129_ROUTE_SECRET"),
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 404,
            registryInvokeSucceeded: false,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal("DESIGN_CONVERSION_ENDPOINT_ROUTE_NOT_FOUND", envelope.ParsedErrorCode);
        Assert.Equal("endpoint_registry", envelope.BoundaryMetadata["adapter_error_source"]);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, envelope.EndpointPath);
    }

    [Fact]
    public async Task TryRunImprovementLoopAsync_CoordinatorNotConfigured_ReturnsStableErrorEnvelope()
    {
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoop(
            new DesignConversionModelServerImprovementLoopHandler(coordinator: null));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        var adapter = new DesignConversionModelServerLocalCallerAdapter(registry);
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: "pass129-not-configured-loop",
            InitialPackage: CreatePackage("pass129-not-configured-request", "PRIVATE_PASS129_CONFIG_SECRET"),
            MaxAttempts: 2);

        var envelope = await adapter.TryRunImprovementLoopAsync(package);

        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 503,
            registryInvokeSucceeded: true,
            runtimeInvoked: false);
        Assert.Equal("error", envelope.EnvelopeKind);
        Assert.Equal(
            DesignConversionModelServerImprovementLoopHandler.CoordinatorNotConfiguredErrorCode,
            envelope.ParsedErrorCode);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["improvement_loop_coordinator_configured"]);
    }

    private static void AssertStableImprovementLoopEnvelope(
        DesignConversionModelServerLocalCallerAdapterResponse envelope,
        int statusCode,
        bool registryInvokeSucceeded,
        bool runtimeInvoked)
    {
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeSchemaVersion, envelope.EnvelopeSchemaVersion);
        Assert.Equal(DesignConversionModelServerCallerContract.LocalCallerResponseEnvelopeBoundary, envelope.EnvelopeBoundary);
        Assert.Equal(DesignConversionModelServerLocalCallerAdapter.AdapterBoundary, envelope.AdapterBoundary);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.EndpointMethod, envelope.EndpointMethod);
        Assert.Equal(DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath, envelope.EndpointPath);
        Assert.Equal(statusCode, envelope.StatusCode);
        Assert.Equal(registryInvokeSucceeded, envelope.RegistryInvokeSucceeded);
        Assert.Equal(runtimeInvoked, envelope.RuntimeInvoked);
        Assert.True(envelope.ParsedResponseJsonSucceeded);
        Assert.Null(envelope.ResponseJsonParseError);
        var improvementLoopRequestFields = DesignConversionModelServerCallerContract
            .DescribeImprovementLoopRoute()
            .RequestFields;
        var requiredImprovementLoopRequestFields = improvementLoopRequestFields
            .Where(field => field.Required)
            .Select(field => field.JsonName)
            .ToArray();
        Assert.Equal(requiredImprovementLoopRequestFields, envelope.RequiredRequestFields);
        Assert.Contains(improvementLoopRequestFields, field =>
            field is { JsonName: "loopId", Required: true });
        Assert.Contains(improvementLoopRequestFields, field =>
            field is { JsonName: "initialPackage", Required: true });
        Assert.False(envelope.RawHtmlIncluded);
        Assert.False(envelope.RawCssIncluded);
        Assert.False(envelope.RawDesignTreeIncluded);
        Assert.False(envelope.ResponseContainsRawPrivateData);
        Assert.False(envelope.RawPrivateResponseLeakageDetected);
        Assert.False(envelope.ExternalCallerRepoTouched);
        Assert.False(envelope.CrossRepoCallerVerified);
        Assert.False(envelope.PublicHttpEndpointVerified);
        Assert.Equal(registryInvokeSucceeded.ToString(), envelope.BoundaryMetadata["registry_invoke_succeeded"]);
        Assert.Equal(runtimeInvoked.ToString(), envelope.BoundaryMetadata["runtime_invoked"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_caller_repo_touched"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["cross_repo_caller_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["public_http_endpoint_verified"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["socket_opened"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["response_contains_raw_private_data"]);
    }

    private static void AssertNoPrivateMarkerLeak(
        string responseBodyJson,
        string privateMarker,
        DesignConversionModelServerLocalCallerPackage initialPackage)
    {
        Assert.DoesNotContain(privateMarker, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Html!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.Css!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.DesignTree!.Payload!, responseBodyJson, StringComparison.Ordinal);
        Assert.DoesNotContain(initialPackage.OriginalTree!.Payload!, responseBodyJson, StringComparison.Ordinal);
    }

    private static string ExpectedMaxAttemptsExceededErrorMessage()
        => $"DesignConversion improvement-loop max attempts must be less than or equal to {DesignConversionImprovementLoopCoordinator.MaxAllowedAttempts.ToString(System.Globalization.CultureInfo.InvariantCulture)}.";

    private static DesignConversionModelServerRouteAdmission CreateEnabledImprovementLoopAdmission(
        string? tenantId = RouteAdmissionTenantId)
        => DesignConversionModelServerRouteAdmission.Create(
            new DesignConversionModelServerRouteAdmissionOptions(
                enabled: true,
                currentToken: RouteAdmissionCurrentToken,
                previousToken: RouteAdmissionPreviousToken,
                previousTokenExpiresAtUtc: RouteAdmissionNow.AddHours(1),
                idempotencyCapacity: 16,
                idempotencyTtl: TimeSpan.FromMinutes(30),
                tenantId: tenantId),
            () => RouteAdmissionNow);

    private static DesignConversionModelServerImprovementLoopHandler CreateAdmissionBackedHandler(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge,
        IDesignConversionImprovementCandidateStrategy strategy,
        DesignConversionModelServerRouteAdmission routeAdmission)
    {
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(service, strategy);
        return new DesignConversionModelServerImprovementLoopHandler(
            coordinator,
            routeAdmission: routeAdmission);
    }

    private static string SerializeLoopRequest(
        string loopId,
        DesignConversionModelServerLocalCallerPackage package,
        int maxAttempts,
        string privateMarker)
        => JsonSerializer.Serialize(
            new DesignConversionModelServerImprovementLoopRequest(
                LoopId: loopId,
                InitialPackage: package,
                MaxAttempts: maxAttempts,
                Metadata: new Dictionary<string, string>
                {
                    ["caller_private_marker"] = privateMarker
                }),
            DesignConversionModelServerHttpHandler.JsonOptions);

    private static DesignConversionModelServerHttpRequest ImprovementLoopHttpRequest(
        string bodyJson,
        IReadOnlyDictionary<string, string>? headers)
        => new(
            Method: DesignConversionModelServerEndpointRegistry.EndpointMethod,
            Path: DesignConversionModelServerEndpointRegistry.ImprovementLoopEndpointPath,
            BodyJson: bodyJson,
            ContentType: "application/json",
            Headers: headers);

    private static IReadOnlyDictionary<string, string> AdmissionHeaders(
        string? token = RouteAdmissionCurrentToken,
        string? idempotencyKey = "n25-4a1-default-key")
    {
        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (token is not null)
            headers[DesignConversionModelServerRouteAdmission.AuthorizationHeaderName] = "Bearer " + token;
        if (idempotencyKey is not null)
            headers[DesignConversionModelServerRouteAdmission.IdempotencyHeaderName] = idempotencyKey;

        return headers;
    }

    private static void AssertError(
        DesignConversionModelServerHttpResponse response,
        string errorCode,
        string? errorMessage = null)
    {
        using var document = JsonDocument.Parse(response.BodyJson);
        var root = document.RootElement;
        Assert.Equal(errorCode, root.GetProperty("errorCode").GetString());
        if (errorMessage is not null)
            Assert.Equal(errorMessage, root.GetProperty("errorMessage").GetString());
    }

    private static void AssertRuntimeNotInvoked(DesignConversionModelServerHttpResponse response)
        => Assert.Equal(
            bool.FalseString,
            BoundaryMetadata(response).GetProperty("runtime_invoked").GetString());

    private static JsonElement BoundaryMetadata(DesignConversionModelServerHttpResponse response)
    {
        using var document = JsonDocument.Parse(response.BodyJson);
        return document.RootElement.GetProperty("boundaryMetadata").Clone();
    }

    private static void AssertResponseAndBoundaryMetadataDoNotLeak(
        DesignConversionModelServerHttpResponse response,
        params string[] forbidden)
    {
        var metadataJson = BoundaryMetadata(response).GetRawText();
        foreach (var value in forbidden.Where(value => !string.IsNullOrWhiteSpace(value)))
        {
            Assert.DoesNotContain(value, response.BodyJson, StringComparison.Ordinal);
            Assert.DoesNotContain(value, metadataJson, StringComparison.Ordinal);
        }
    }

    private static DesignConversionModelServerLocalCallerAdapter CreateAdapter(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        IDesignConversionVisualJudge visualJudge,
        IDesignConversionImprovementCandidateStrategy strategy,
        IDesignConversionLocalArtifactDb? artifactDb = null,
        TimeSpan? runDeadline = null)
    {
        var service = new DesignConversionInternalService(renderProvider, comparisonProvider, visualJudge);
        var coordinator = new DesignConversionImprovementLoopCoordinator(
            service,
            strategy,
            artifactDb,
            runDeadline: runDeadline);
        var registry = new DesignConversionModelServerEndpointRegistry();
        var registered = registry.RegisterDesignConversionImprovementLoop(
            new DesignConversionModelServerImprovementLoopHandler(coordinator));
        Assert.True(registered.IsSuccess, registered.ErrorMessage);
        return new DesignConversionModelServerLocalCallerAdapter(registry);
    }

    private static DesignConversionModelServerLocalCallerPackage CreatePackage(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:design-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"generated\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{requestId}:original-tree",
                Payload: $"{{\"secret\":\"{privateMarker}\",\"kind\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["private_marker"] = privateMarker
                }),
            Html: $"<main data-secret=\"{privateMarker}\">Pass129</main>",
            Css: $".pass129{{background-image:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 1440, 900),
                CreateArtifact($"{requestId}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:design-desktop", "design-render", 1440, 900),
                CreateArtifact($"{requestId}:design-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: $"user-{privateMarker}",
                Tier: "pass129-local-loop",
                MaxImprovementCandidates: 5,
                MaxRenderQaRuns: 2,
                BudgetUsd: 1.00m,
                Metadata: new Dictionary<string, string>
                {
                    ["quality_threshold"] = "0.90",
                    ["budget_private_marker"] = privateMarker
                }),
            Metadata: new Dictionary<string, string>
            {
                ["caller_private_marker"] = privateMarker
            });

    private static DesignConversionModelServerLocalCallerPackage CreatePass163Package(
        string requestId,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass163-same-scenario-design-tree",
                Payload: $"{{\"node\":\"frame\",\"secret\":\"{privateMarker}\",\"source\":\"pass163\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["source_kind"] = "figma",
                    ["local_only"] = bool.TrueString
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass163-same-scenario-original-tree",
                Payload: $"{{\"node\":\"frame\",\"secret\":\"{privateMarker}\",\"source\":\"original\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["source_kind"] = "figma",
                    ["local_only"] = bool.TrueString
                }),
            Html: $"<main class=\"pass163-card\" data-secret=\"{privateMarker}\">Pass163</main>",
            Css: $".pass163-card{{display:grid;gap:12px;background:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 1440, 900),
                CreateArtifact($"{requestId}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:target-desktop", "design-render", 1440, 900),
                CreateArtifact($"{requestId}:target-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "pass163-local-user",
                Tier: "pass163-local-no-provider",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>
                {
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString,
                    ["cloud_upload"] = bool.FalseString,
                    ["deployment_verified"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString
                }),
            Metadata: new Dictionary<string, string>
            {
                ["pass163_same_scenario_id"] = "pass163-local-chain",
                ["local_only"] = bool.TrueString,
                ["no_external_provider"] = bool.TrueString,
                ["cloud_upload"] = bool.FalseString,
                ["deployment_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });

    private static DesignConversionModelServerLocalCallerPackage CreatePass164Package(
        string requestId,
        string sourceSlug,
        string privateMarker)
        => new(
            RequestId: requestId,
            SourceKind: nameof(DesignConversionSourceKind.Figma),
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:design-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{sourceSlug}-design-tree",
                Payload: $"{{\"node\":\"frame\",\"source\":\"{sourceSlug}\",\"layout\":\"runtime-loop\",\"secret\":\"{privateMarker}\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["source_kind"] = "figma",
                    ["source_family"] = sourceSlug,
                    ["local_only"] = bool.TrueString
                }),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{requestId}:original-tree-ref",
                Kind: "figma-json",
                Sha256: $"sha256:{sourceSlug}-original-tree",
                Payload: $"{{\"node\":\"frame\",\"source\":\"{sourceSlug}-original\",\"layout\":\"runtime-loop\",\"secret\":\"{privateMarker}\"}}",
                Format: "figma-json",
                Metadata: new Dictionary<string, string>
                {
                    ["source_kind"] = "figma",
                    ["source_family"] = sourceSlug,
                    ["local_only"] = bool.TrueString
                }),
            Html: $"<main class=\"pass164-card-{sourceSlug}\" data-secret=\"{privateMarker}\">Pass164 {sourceSlug}</main>",
            Css: $".pass164-card-{sourceSlug}{{display:grid;gap:12px;background:url('{privateMarker}');}}",
            Screenshots:
            [
                CreateArtifact($"{sourceSlug}:source-desktop", "screenshot", 1440, 900),
                CreateArtifact($"{sourceSlug}:source-mobile", "screenshot", 390, 844)
            ],
            DesignRenders:
            [
                CreateArtifact($"{sourceSlug}:target-desktop", "design-render", 1440, 900),
                CreateArtifact($"{sourceSlug}:target-mobile", "design-render", 390, 844)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "pass164-local-user",
                Tier: "pass164-local-no-provider",
                MaxImprovementCandidates: 2,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.25m,
                Metadata: new Dictionary<string, string>
                {
                    ["source_family"] = sourceSlug,
                    ["local_only"] = bool.TrueString,
                    ["no_external_provider"] = bool.TrueString,
                    ["cloud_upload"] = bool.FalseString,
                    ["deployment_verified"] = bool.FalseString,
                    ["product_ready"] = bool.FalseString
                }),
            Metadata: new Dictionary<string, string>
            {
                ["pass164_source_disjoint_probe"] = bool.TrueString,
                ["source_family"] = sourceSlug,
                ["local_only"] = bool.TrueString,
                ["no_external_provider"] = bool.TrueString,
                ["cloud_upload"] = bool.FalseString,
                ["deployment_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });

    private static DesignConversionModelServerArtifactRef CreateArtifact(
        string id,
        string kind,
        int width,
        int height)
        => new(
            ArtifactId: id,
            Kind: kind,
            Width: width,
            Height: height,
            Uri: $"artifact://pass129/{id}.png",
            Sha256: $"sha256:{id}");

    private static async Task<Pass164RRuntimeCandidateTrace> CreateApprovedPass164RRuntimeCandidateAsync(
        JsonlDesignConversionLocalArtifactDb artifactDb,
        DesignConversionDpoUsefulnessApi usefulnessApi,
        DesignConversionModelServerLocalCallerAdapter adapter,
        int sourceOrdinal,
        string privateMarker)
    {
        var sourceSlug = $"pass164r-source-{sourceOrdinal:000}";
        var requestId = $"pass164r-runtime-request-{sourceOrdinal:000}";
        var package = new DesignConversionModelServerImprovementLoopLocalCallerPackage(
            LoopId: $"pass164r-runtime-loop-{sourceOrdinal:000}",
            InitialPackage: CreatePass164Package(requestId, sourceSlug, privateMarker),
            MaxAttempts: 2,
            Metadata: new Dictionary<string, string>
            {
                ["pass164r_count_gate_probe"] = bool.TrueString,
                ["source_family"] = sourceSlug,
                ["local_only"] = bool.TrueString,
                ["no_external_provider"] = bool.TrueString
            });

        var envelope = await adapter.TryRunImprovementLoopAsync(package);
        AssertStableImprovementLoopEnvelope(
            envelope,
            statusCode: 200,
            registryInvokeSucceeded: true,
            runtimeInvoked: true);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["external_provider_called"]);
        Assert.Equal(bool.FalseString, envelope.BoundaryMetadata["product_ready"]);
        Assert.DoesNotContain(privateMarker, envelope.ResponseBodyJson, StringComparison.Ordinal);

        var records = await artifactDb.ListRecordsAsync(
            new DesignConversionLocalArtifactDbQuery(RequestId: requestId));
        Assert.True(records.IsSuccess, records.ErrorMessage);
        var stored = Assert.Single(records.Value!);
        var candidate = Assert.Single(stored.DpoCandidates);
        Assert.Equal(package.InitialPackage!.DesignTree!.Sha256, candidate.TrainingRecord.Provenance.SourceManifestRef);
        Assert.Equal(JsonlDesignConversionLocalArtifactDb.RuntimeCandidateReviewPendingStatus, candidate.TrainingRecord.Provenance.Metadata["review_status"]);
        Assert.Equal(bool.FalseString, candidate.TrainingRecord.Provenance.Metadata["approved_for_training"]);
        AssertRuntimeQualityCreatedDpoPair(candidate.TrainingRecord);

        var approval = await usefulnessApi.ApproveRuntimeCreatedDpoCandidateAsync(
            stored.RecordId,
            candidate.CandidateId,
            "reviewer:pass164r-local",
            $"Pass164R runtime candidate approval for source ordinal {sourceOrdinal:000}; count gate must remain default 50/50",
            DateTimeOffset.UnixEpoch.AddMinutes(164).AddSeconds(sourceOrdinal));
        Assert.True(approval.IsSuccess, approval.ErrorMessage);
        Assert.True(approval.Value!.ApprovedForTraining);
        Assert.Equal(DesignConversionDpoUsefulnessApi.ApprovalApprovedStatus, approval.Value.ApprovalStatus);

        return new Pass164RRuntimeCandidateTrace(
            Stored: stored,
            CandidateId: candidate.CandidateId,
            SourceManifestRef: candidate.TrainingRecord.Provenance.SourceManifestRef,
            SourceStructurePayload: candidate.TrainingRecord.SourceStructurePayload);
    }

    private static DesignConversionTrainingPipelineOptions Pass164RDefaultCountLocalPromotionOptions(string runId)
        => new(
            RunId: runId,
            BatchSize: 25,
            DatasetOptions: new DesignConversionDpoDatasetOptions(
                DatasetId: runId,
                Preflight: DesignConversionDpoDatasetPreflight.LocalTrainingIntakeSourceManifestDisjointProof(
                    [$"{runId}:loop_created_runtime_source_disjoint_default_count_gate"])),
            MeasurementGateRequest: BuildPass164RPassingMeasurementGateRequest($"{runId}:measurement-pass"));

    private static DesignConversionMeasurementGateRequest BuildPass164RPassingMeasurementGateRequest(string gateId)
        => new(
            GateId: gateId,
            UnitId: DesignConversionDpoUnits.ImproverUnit,
            HeldOutMetrics: new DesignConversionHeldOutMeasurementMetrics(
                HeldOutRecordCount: 50,
                HeldOutQualityScore: 0.94,
                RuntimeMs: 320,
                CostUsd: 0.25m,
                PeakRamMb: 256,
                EvidenceRefs: [$"{gateId}:heldout:pass"]),
            IntegrityProof: new DesignConversionMeasurementIntegrityProof(
                SourceDisjoint: true,
                LeakageDetected: false,
                HiddenDeterminismDetected: false,
                AblationPassed: true,
                EvidenceRefs: [$"{gateId}:integrity:pass"]),
            Thresholds: new DesignConversionMeasurementGateThresholds(
                RequiredHeldOutQualityScore: 0.90,
                MaxRuntimeMs: 800,
                MaxCostUsd: 1.00m,
                MaxPeakRamMb: 512));

    private static DesignConversionModelServerApiRequest CreatePass164RModelServerRequest(string requestId)
        => new(
            RequestId: requestId,
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: "pass164r-modelserver-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass164r-modelserver-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"pass164r-safe\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: "pass164r-modelserver-original-tree-ref",
                Kind: "figma-json",
                Sha256: "sha256:pass164r-modelserver-original-tree",
                Payload: "{\"type\":\"FRAME\",\"name\":\"pass164r-original\"}",
                Format: "figma-json"),
            Html: "<main class=\"screen\"><h1>Pass164R</h1></main>",
            Css: ".screen{display:grid;gap:16px;}",
            Screenshots:
            [
                CreateArtifact($"{requestId}:source-desktop", "screenshot", 1440, 900)
            ],
            DesignRenders:
            [
                CreateArtifact($"{requestId}:target-desktop", "design-render", 1440, 900)
            ],
            Budget: new DesignConversionModelServerBudget(
                UserId: "pass164r-local-user",
                Tier: "pass164r-local-promoted",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 1,
                BudgetUsd: 0.25m),
            Metadata: new Dictionary<string, string>
            {
                ["local_only"] = bool.TrueString,
                ["no_external_provider"] = bool.TrueString,
                ["cloud_upload"] = bool.FalseString,
                ["deployment_verified"] = bool.FalseString,
                ["product_ready"] = bool.FalseString
            });

    private static async Task<string> ReadSha256Async(string path)
    {
        var bytes = await File.ReadAllBytesAsync(path);
        return Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    }

    private static DesignConversionImprovementCandidate CreateCandidate(
        string candidateId,
        string privateMarker)
        => new(
            CandidateId: candidateId,
            RequestId: $"{candidateId}-request",
            Html: $"<main data-secret=\"{privateMarker}\">Improved</main>",
            Css: $".improved-pass129{{color:'{privateMarker}';}}",
            ChangeReasons: ["scripted local improvement"],
            EvidenceRefs: [$"candidate:{candidateId}"],
            Metadata: new Dictionary<string, string>
            {
                ["private_marker"] = privateMarker
            });

    private static void AssertRuntimeQualityCreatedDpoPair(DesignConversionDpoTrainingRecord record)
    {
        Assert.Equal(DesignConversionDpoUnits.ImproverUnit, record.UnitId);
        Assert.True(record.Chosen.Accepted);
        Assert.False(record.Rejected.Accepted);
        Assert.NotEqual(record.Chosen.CandidateId, record.Rejected.CandidateId);
        Assert.True(record.Chosen.Verdict.Score > record.Rejected.Verdict.Score);
        Assert.True(record.UsefulnessScore > 0);
        Assert.Contains("runtime_created_dpo_candidate", record.Provenance.Metadata.Keys);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["runtime_created_dpo_candidate"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["review_required"]);
        Assert.Equal(bool.TrueString, record.Provenance.Metadata["approval_required"]);
        Assert.True(record.Rejected.EvidenceRefs.Count + record.Rejected.QaResults.Count > 0);
        Assert.False(record.Quarantine.IsQuarantined);
    }

    private sealed class RecordingRenderProvider : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:actual",
                Label: "actual-render",
                Resolution: request.Resolution,
                Uri: $"artifact://pass129/render/{request.RenderRequestId}.png",
                Sha256: $"sha256:{request.RenderRequestId}:actual",
                Metadata: request.Metadata);

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 1,
                Warnings: [])));
        }
    }

    private sealed class BlockingRenderProvider : IDesignConversionRenderProvider
    {
        public List<DesignConversionRenderRequest> Requests { get; } = [];

        public async Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return OperationResult<DesignConversionRenderResult>.Fail(
                "Blocking render provider unexpectedly completed.",
                "BLOCKING_RENDER_PROVIDER_COMPLETED");
        }
    }

    private sealed class RecordingComparisonProvider(Func<DesignConversionQaComparisonRequest, double> resolveScore)
        : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var score = resolveScore(request);
            var passed = score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:verdict",
                Score: score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.ComparisonRequestId}:defect",
                            Category: "spacing",
                            Severity: "minor",
                            Description: "Scripted comparison provider for Pass129 ModelServer improvement-loop tests: elements are too close and not enough padding.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [request.Expected.ImageId, request.Actual.ImageId])
                    ],
                EvidenceRefs: [$"comparison:{request.ComparisonRequestId}"],
                Passed: passed);

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://pass129/diff/{request.ComparisonRequestId}.png",
                DiffImageSha256: $"sha256:{request.ComparisonRequestId}:diff",
                Metrics: new Dictionary<string, double>
                {
                    ["visual_distance"] = 1.0 - score
                },
                EvidenceRefs: [$"comparison-artifact:{request.ComparisonRequestId}"])));
        }
    }

    private sealed class ScriptedVisualJudge(Func<DesignConversionVisualJudgeRequest, JudgeScript> resolve)
        : IDesignConversionVisualJudge
    {
        public List<DesignConversionVisualJudgeRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionVisualJudgeResult>> JudgeAsync(
            DesignConversionVisualJudgeRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var script = resolve(request);
            var passed = !script.Blocked && script.Score >= 0.90;
            var verdict = new VisualVerdict(
                VerdictId: $"{request.JudgeRequestId}:scripted-verdict",
                Score: script.Score,
                Defects: passed
                    ? []
                    :
                    [
                        new VisualDefect(
                            DefectId: $"{request.JudgeRequestId}:scripted-defect",
                            Category: script.Blocked ? "provider" : "quality",
                            Severity: script.Blocked ? "blocker" : "minor",
                            Description: "Scripted visual judge result for Pass129 ModelServer improvement-loop tests.",
                            X: 0,
                            Y: 0,
                            Width: 1,
                            Height: 1,
                            EvidenceImageIds: [request.ExpectedScreenshot.ImageId, request.RenderedImage.ImageId])
                    ],
                EvidenceRefs: [$"visual-judge:{request.JudgeRequestId}"],
                Passed: passed);
            var status = script.Blocked
                ? DesignConversionVisualQaBoundaryPolicy.ProviderRequired
                : "SCRIPTED_VISUAL_JUDGE_VERDICT_RECEIVED";
            var providerMetadata = new Dictionary<string, string>
            {
                [DesignConversionVisualQaBoundaryPolicy.TestProviderOnlyMarker] = (!script.Blocked && !script.ExternalCallExecuted).ToString(),
                ["external_ai_visual_judge_executed"] = script.ExternalCallExecuted.ToString()
            };
            var resultMetadata = new Dictionary<string, string>
            {
                ["external_ai_visual_judge_executed"] = script.ExternalCallExecuted.ToString()
            };
            if (script.ExternalCallExecuted && script.Cost.HasValue)
            {
                providerMetadata["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest;
                resultMetadata["cost_source"] = DesignConversionExternalVisualProviderConfigIntake.ServerOwnedCostSourceFixedPerRequest;
            }

            return Task.FromResult(OperationResult<DesignConversionVisualJudgeResult>.Ok(new DesignConversionVisualJudgeResult(
                JudgeResultId: $"{request.JudgeRequestId}:scripted-result",
                JudgeRequestId: request.JudgeRequestId,
                Status: status,
                IsBlocked: script.Blocked,
                Verdict: verdict,
                Provider: new DesignConversionVisualJudgeProviderMetadata(
                    ProviderName: script.Blocked ? "none" : "scripted-local-judge",
                    EndpointHost: "none",
                    ModelId: script.Blocked ? "none" : "scripted-local-v1",
                    ExternalCallExecuted: script.ExternalCallExecuted,
                    Metadata: providerMetadata),
                EstimatedCostUsd: script.Cost,
                TokenMetadata: new Dictionary<string, double>(),
                EvidenceRefs: verdict.EvidenceRefs,
                Metadata: resultMetadata)));
        }
    }

    private sealed record JudgeScript(double Score, decimal? Cost, bool Blocked, bool ExternalCallExecuted = false)
    {
        public static JudgeScript FromScore(double score, decimal? cost)
            => new(score, cost, Blocked: false);

        public static JudgeScript FromExternalScore(double score, decimal? cost)
            => new(score, cost, Blocked: false, ExternalCallExecuted: true);

        public static JudgeScript FromBlockedProvider()
            => new(0.0, 0m, Blocked: true);
    }

    private sealed class ScriptedCandidateStrategy(
        params OperationResult<DesignConversionImprovementCandidate>[] results)
        : IDesignConversionImprovementCandidateStrategy
    {
        private readonly Queue<OperationResult<DesignConversionImprovementCandidate>> _results = new(results);

        public List<DesignConversionCandidateUpdateRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionImprovementCandidate>> CreateNextCandidateAsync(
            DesignConversionCandidateUpdateRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            if (_results.Count == 0)
            {
                return Task.FromResult(OperationResult<DesignConversionImprovementCandidate>.Fail(
                    "No scripted candidate result was configured.",
                    "SCRIPTED_CANDIDATE_MISSING"));
            }

            return Task.FromResult(_results.Dequeue());
        }
    }

    private sealed class FailingAppendArtifactDb(
        string errorMessage,
        string errorCode,
        bool throwCanceled = false) : IDesignConversionLocalArtifactDb
    {
        public int AppendAttempts { get; private set; }

        public Task<OperationResult<DesignConversionLocalArtifactDbRecord>> AppendRecordAsync(
            DesignConversionLocalArtifactDbRecord record,
            CancellationToken ct = default)
        {
            AppendAttempts++;
            if (throwCanceled)
                throw new OperationCanceledException(ct);

            return Task.FromResult(OperationResult<DesignConversionLocalArtifactDbRecord>.Fail(errorMessage, errorCode));
        }

        public Task<OperationResult<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>> ListRecordsAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => UnsupportedAsync<IReadOnlyList<DesignConversionLocalArtifactDbRecord>>();

        public Task<OperationResult<IReadOnlyList<DesignConversionDpoTrainingRecord>>> QueryApprovedTrainingCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            CancellationToken ct = default)
            => UnsupportedAsync<IReadOnlyList<DesignConversionDpoTrainingRecord>>();

        public Task<OperationResult<DesignConversionApprovedDpoHandoffArtifact>> ExportApprovedRuntimeDpoCandidatesAsync(
            DesignConversionLocalArtifactDbQuery? query = null,
            DateTimeOffset? exportedAtUtc = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionApprovedDpoHandoffArtifact>();

        public Task<OperationResult<DesignConversionTrainingIntakeRecord>> CreateTrainingIntakeAsync(
            DesignConversionApprovedDpoHandoffArtifact handoff,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => UnsupportedAsync<DesignConversionTrainingIntakeRecord>();

        public Task<OperationResult<IReadOnlyList<DesignConversionTrainingIntakeRecord>>> ListTrainingIntakeRequestsAsync(
            CancellationToken ct = default)
            => UnsupportedAsync<IReadOnlyList<DesignConversionTrainingIntakeRecord>>();

        public Task<OperationResult<DesignConversionTrainingIntakePreflightResult>> RunTrainingIntakePreflightAsync(
            string intakeId,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionTrainingIntakePreflightResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeRunResult>> RunTrainingIntakeAsync(
            string intakeId,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default,
            IReadOnlyDictionary<string, string>? admissionBoundaryMetadata = null)
            => UnsupportedAsync<DesignConversionTrainingIntakeRunResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionResult>> RunTrainingIntakeAndPromoteLocalAsync(
            string intakeId,
            string promotionOutputDirectory,
            DesignConversionTrainingPipelineOptions? options = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionTrainingIntakeLocalPromotionResult>();

        public Task<OperationResult<DesignConversionTrainingIntakeLocalPromotionRollbackResult>> RollbackTrainingIntakeLocalPromotionAsync(
            string promotedLocalModelRef,
            string requestedByRef,
            string reason,
            DateTimeOffset? requestedAtUtc = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionTrainingIntakeLocalPromotionRollbackResult>();

        public Task<OperationResult<DesignConversionDpoCandidateApprovalResult>> ApproveRuntimeCreatedDpoCandidateAsync(
            string localArtifactRecordId,
            string candidateOrTrainingRecordId,
            string reviewerRef,
            string reason,
            DateTimeOffset? approvedAtUtc = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionDpoCandidateApprovalResult>();

        public Task<OperationResult<DesignConversionDpoDisplayResult>> QueryDisplayRowsAsync(
            DesignConversionDpoDisplayQuery? query = null,
            CancellationToken ct = default)
            => UnsupportedAsync<DesignConversionDpoDisplayResult>();

        private static Task<OperationResult<T>> UnsupportedAsync<T>()
            => Task.FromResult(OperationResult<T>.Fail(
                "Failing append artifact DB supports append only.",
                "FAILING_APPEND_ARTIFACT_DB_UNSUPPORTED"));
    }

    private sealed record Pass164RRuntimeCandidateTrace(
        DesignConversionLocalArtifactDbRecord Stored,
        string CandidateId,
        string SourceManifestRef,
        string SourceStructurePayload);
}
