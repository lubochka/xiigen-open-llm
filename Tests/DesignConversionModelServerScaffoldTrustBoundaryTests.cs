// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AgentInfrastructure;
using DesignConversionDomain;
using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerScaffoldTrustBoundaryTests
{
    private const string TenantA = "tenant:scaffold-boundary:alpha";
    private const string TenantB = "tenant:scaffold-boundary:beta";
    private const string ActorA = "scaffold-boundary-approver-alpha";
    private const string ActorB = "scaffold-boundary-approver-beta";

    [Fact]
    public async Task SubmitRoute_MarkerLossSyntheticPngCannotBecomeApprovalOrReadinessEvidence()
    {
        var api = CreateApi(
            new MarkerLossSyntheticPngRenderProvider(),
            new PassingPngComparisonProvider(),
            utcNow: () => DateTimeOffset.Parse("2026-07-10T08:00:00Z"));

        var submit = await api.SubmitAndEvaluateAsync(CreateRequest("marker-loss-synthetic"));

        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, submit.Value!.ContractStatus);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            submit.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        Assert.False(submit.Value.ProductReady);
        Assert.False(submit.Value.TrainingReadiness.ProductReady);
        Assert.False(submit.Value.ModelUploadReady);
        Assert.Equal("promotion_not_run", submit.Value.TrainingReadiness.PromotionStatus);
        Assert.Equal("promotion_not_run", submit.Value.BoundaryMetadata["promotion_status"]);

        var challenge = await api.IssueHumanApprovalChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                submit.Value.JobId,
                Actor(TenantA, ActorA)));

        Assert.False(challenge.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SYNTHETIC_RENDER_PROOF_REJECTED", challenge.ErrorCode);
    }

    [Fact]
    public async Task SubmitRoute_ContradictoryClientMarkersAreQuarantinedBeforeComparison()
    {
        var comparisonProvider = new PassingPngComparisonProvider();
        var api = CreateApi(
            new MarkerLossSyntheticPngRenderProvider(),
            comparisonProvider,
            utcNow: () => DateTimeOffset.Parse("2026-07-10T08:01:00Z"));
        var request = CreateRequest(
            "contradictory-client-marker",
            requestMetadata: new Dictionary<string, string>
            {
                ["product_ready"] = bool.TrueString,
                ["browser_e2e_verified"] = bool.TrueString,
                ["browser_e2e_verification_state"] = "client_claimed_verified",
                ["learned_inference_applied"] = bool.TrueString,
                ["local_promoted_model_id"] = "client-supplied-other-model",
                ["model_activation_tenant_id_ref"] = "tenant:other"
            },
            artifactMetadata: new Dictionary<string, string>
            {
                ["provider_kind"] = BrowserBackedDesignConversionRenderProvider.ProviderKind,
                ["render_artifact_kind"] = "browser_screenshot",
                ["visual_fidelity_claimed"] = bool.TrueString,
                ["browser_or_playwright_used"] = bool.TrueString,
                ["browser_screenshot_png_signature_verified"] = bool.TrueString,
                ["browser_screenshot_sha256_verified"] = bool.TrueString,
                ["browser_screenshot_nonblank_pixels_verified"] = bool.TrueString,
                ["hidden_determinism_boundary"] = "browser_backed_render_evidence",
                ["product_ready"] = bool.TrueString,
                ["promotion_ready"] = bool.TrueString
            });

        var submit = await api.SubmitAndEvaluateAsync(request);

        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, submit.Value!.ContractStatus);
        foreach (var comparisonRequest in comparisonProvider.Requests)
        {
            var expectedMetadata = comparisonRequest.Expected.Metadata;
            Assert.DoesNotContain("provider_kind", expectedMetadata.Keys);
            Assert.DoesNotContain("render_artifact_kind", expectedMetadata.Keys);
            Assert.DoesNotContain("browser_or_playwright_used", expectedMetadata.Keys);
            Assert.DoesNotContain("hidden_determinism_boundary", expectedMetadata.Keys);
            Assert.DoesNotContain("product_ready", expectedMetadata.Keys);
            Assert.Equal(bool.TrueString, expectedMetadata["caller_artifact_trust_metadata_quarantined"]);
        }
        Assert.False(submit.Value.ProductReady);
        Assert.False(submit.Value.LearnedInferenceApplied);
        Assert.False(submit.Value.BoundaryMetadata.ContainsKey("local_promoted_model_id"));
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "runtime_not_completed",
            submit.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
    }

    [Fact]
    public async Task SubmitRoute_TenantModelJobReplayClaimsDoNotCrossStatusOrApprovalBoundary()
    {
        var clock = new MutableClock(DateTimeOffset.Parse("2026-07-10T08:02:00Z"));
        var api = CreateApi(
            new MarkerLossSyntheticPngRenderProvider(),
            new PassingPngComparisonProvider(),
            utcNow: clock.UtcNow);

        var first = await api.SubmitAndEvaluateAsync(CreateRequest(
            "identity-replay-a",
            requestMetadata: new Dictionary<string, string>
            {
                ["thin_client_job_id"] = "thin-job-attacker-replay",
                ["local_promoted_model_id"] = "attacker-model",
                ["model_activation_tenant_id_ref"] = TenantB,
                ["runtime_tenant_id_ref"] = TenantA,
                ["product_ready"] = bool.TrueString
            }));
        clock.Advance(TimeSpan.FromSeconds(1));
        var second = await api.SubmitAndEvaluateAsync(CreateRequest("identity-replay-b"));

        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.NotEqual("thin-job-attacker-replay", first.Value!.JobId);
        Assert.DoesNotContain("attacker-model", string.Join("|", first.Value.BoundaryMetadata.Values), StringComparison.Ordinal);
        Assert.False(first.Value.ProductReady);

        var challenge = await api.IssueHumanApprovalChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                first.Value.JobId,
                Actor(TenantA, ActorA)));
        Assert.False(challenge.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SYNTHETIC_RENDER_PROOF_REJECTED", challenge.ErrorCode);

        var browserRoot = NewRoot("identity-replay-browser");
        var browserApi = CreateApi(
            new BrowserBackedDesignConversionRenderProvider(
                new LocalPngBrowserAdapter(browserRoot)),
            new ServerOwnedLocalPngComparisonProvider(browserRoot),
            utcNow: clock.UtcNow);
        var browserFirst = await browserApi.SubmitAndEvaluateAsync(CreateRequest(
            "identity-replay-browser-a",
            artifactRoot: browserRoot));
        var browserSecond = await browserApi.SubmitAndEvaluateAsync(CreateRequest(
            "identity-replay-browser-b",
            artifactRoot: browserRoot));
        Assert.True(browserFirst.IsSuccess, browserFirst.ErrorMessage);
        Assert.True(browserSecond.IsSuccess, browserSecond.ErrorMessage);

        var browserChallenge = await browserApi.IssueHumanApprovalChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                browserFirst.Value!.JobId,
                Actor(TenantA, ActorA)));
        Assert.True(browserChallenge.IsSuccess, browserChallenge.ErrorMessage);

        var wrongJob = await browserApi.ApproveHumanConversionAsync(
            new DesignConversionHumanApprovalDecisionRequest(
                browserChallenge.Value!.ChallengeId,
                browserSecond.Value!.JobId,
                Actor(TenantA, ActorA),
                browserChallenge.Value.RequiredViewportIds,
                "wrong-job-replay-op",
                "wrong job replay"));
        var wrongTenant = await browserApi.ApproveHumanConversionAsync(
            new DesignConversionHumanApprovalDecisionRequest(
                browserChallenge.Value.ChallengeId,
                browserChallenge.Value.JobId,
                Actor(TenantB, ActorB),
                browserChallenge.Value.RequiredViewportIds,
                "wrong-tenant-replay-op",
                "wrong tenant replay"));

        Assert.False(wrongJob.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_SNAPSHOT_STALE", wrongJob.ErrorCode);
        Assert.False(wrongTenant.IsSuccess);
        Assert.Equal("DESIGN_CONVERSION_APPROVAL_AUTHORIZATION_DENIED", wrongTenant.ErrorCode);
    }

    [Fact]
    public async Task SubmitHttpRoute_UnrecognizedCallerMetadataNeverCrossesPublicRuntimeStatus()
    {
        const string requestMarkerValue = "amber-widget-alpha";
        const string budgetMarkerValue = "blue-river-budget";
        var api = CreateApi(
            new MarkerLossSyntheticPngRenderProvider(),
            new PassingPngComparisonProvider(),
            utcNow: () => DateTimeOffset.Parse("2026-07-10T08:02:30Z"));
        var handler = new DesignConversionModelServerHttpHandler(api);
        var baseRequest = CreateRequest(
            "caller-metadata-egress",
            requestMetadata: new Dictionary<string, string>
            {
                ["friendly_marker"] = requestMarkerValue,
                ["caller_note"] = "ordinary-status-note"
            });
        var request = baseRequest with
        {
            Budget = baseRequest.Budget! with
            {
                Metadata = new Dictionary<string, string>
                {
                    ["spend_marker"] = budgetMarkerValue,
                    ["budget_note"] = "ordinary-budget-note"
                }
            }
        };
        var bodyJson = JsonSerializer.Serialize(
            request,
            DesignConversionModelServerHttpHandler.JsonOptions);

        var http = await handler.HandleAsync(new DesignConversionModelServerHttpRequest(
            "POST",
            DesignConversionModelServerHttpHandler.EndpointPath,
            bodyJson,
            "application/json"));

        Assert.Equal(200, http.StatusCode);
        var parsed = JsonSerializer.Deserialize<DesignConversionModelServerHttpEvaluateResponse>(
            http.BodyJson,
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.NotNull(parsed);
        AssertPublicRuntimeStatusCallerMetadataAbsent(
            parsed!.RuntimeStatus,
            requestMarkerValue,
            budgetMarkerValue);
        Assert.Equal(
            DesignConversionModelServerApi.ApiShape,
            parsed.RuntimeStatus.Provenance["api_shape"]);
        Assert.Equal(
            DesignConversionModelServerApi.RuntimeSurface,
            parsed.RuntimeStatus.Provenance["runtime_surface"]);

        var status = await api.GetJobResultAsync(parsed.JobId);

        Assert.True(status.IsSuccess, status.ErrorMessage);
        AssertPublicRuntimeStatusCallerMetadataAbsent(
            status.Value!.RuntimeStatus,
            requestMarkerValue,
            budgetMarkerValue);
        Assert.Equal(
            DesignConversionModelServerApi.ApiShape,
            status.Value.RuntimeStatus.Provenance["api_shape"]);
        Assert.Equal(
            DesignConversionModelServerApi.RuntimeSurface,
            status.Value.RuntimeStatus.Provenance["runtime_surface"]);
    }

    [Fact]
    public async Task SubmitRoute_BrowserBackedRenderCanApproveForUserWithoutReadinessPromotionClaims()
    {
        var root = NewRoot("browser-backed-legitimate");
        var api = CreateApi(
            new BrowserBackedDesignConversionRenderProvider(new LocalPngBrowserAdapter(root)),
            new ServerOwnedLocalPngComparisonProvider(root),
            utcNow: () => DateTimeOffset.Parse("2026-07-10T08:03:00Z"));

        var submit = await api.SubmitAndEvaluateAsync(CreateRequest(
            "browser-backed-legitimate",
            artifactRoot: root));

        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.NotReady, submit.Value!.ContractStatus);
        Assert.Equal(bool.TrueString, submit.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "not_verified_by_server_owned_browser_render_receipts",
            submit.Value.BoundaryMetadata["browser_e2e_verification_state"]);
        Assert.Equal(
            "server_owned_model_server_browser_render_receipts",
            submit.Value.BoundaryMetadata["browser_e2e_verification_authority"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_required_viewport_count"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_observed_viewport_count"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_verified_render_artifact_count"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_png_signature_verified_count"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_sha256_verified_count"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_nonblank_verified_count"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_dimension_verified_count"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_browser_provenance_count"]);
        Assert.Equal(BrowserBackedDesignConversionRenderProvider.ProviderKind, submit.Value.BoundaryMetadata["browser_e2e_provider_kind"]);
        Assert.Equal("browser_screenshot", submit.Value.BoundaryMetadata["browser_e2e_artifact_kind"]);
        Assert.Equal("runtime_not_completed", submit.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["runtime_ready_verified"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["host_readiness_verified"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["visual_fidelity_verified"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["persistence_verified"]);
        Assert.Equal(bool.TrueString, submit.Value.BoundaryMetadata["service_scaffold_browser_renderer_explicit_local_injection_proven"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["service_scaffold_browser_renderer_default_production_wired"]);
        Assert.False(submit.Value.ProductReady);
        Assert.False(submit.Value.ModelUploadReady);
        Assert.Equal("promotion_not_run", submit.Value.TrainingReadiness.PromotionStatus);

        var challenge = await api.IssueHumanApprovalChallengeAsync(
            new DesignConversionHumanApprovalChallengeIssueRequest(
                submit.Value.JobId,
                Actor(TenantA, ActorA)));
        Assert.True(challenge.IsSuccess, challenge.ErrorMessage);

        var approval = await api.ApproveHumanConversionAsync(
            new DesignConversionHumanApprovalDecisionRequest(
                challenge.Value!.ChallengeId,
                challenge.Value.JobId,
                Actor(TenantA, ActorA),
                challenge.Value.RequiredViewportIds,
                "browser-backed-approval-op",
                "human accepted browser-backed refs"));

        Assert.True(approval.IsSuccess, approval.ErrorMessage);
        Assert.True(approval.Value!.Record.AcceptedForUser);
        Assert.False(approval.Value.Record.ProductReady);
        Assert.False(approval.Value.Record.TrainedOrLearnedProvenance);
        Assert.False(approval.Value.Record.DpoEligible);
        Assert.False(approval.Value.Record.ModelWeightsPublished);

        var status = api.GetJobResult(submit.Value.JobId);
        Assert.True(status.IsSuccess, status.ErrorMessage);
        Assert.Equal(bool.FalseString, status.Value!.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal(
            "not_verified_by_server_owned_browser_render_receipts",
            status.Value.BoundaryMetadata["browser_e2e_verification_state"]);
        Assert.Equal("runtime_not_completed", status.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        Assert.False(status.Value.ProductReady);
    }

    private static void AssertPublicRuntimeStatusCallerMetadataAbsent(
        DesignConversionThinClientStatusResult runtimeStatus,
        string requestMarkerValue,
        string budgetMarkerValue)
    {
        var runtimeStatusJson = JsonSerializer.Serialize(
            runtimeStatus,
            DesignConversionModelServerHttpHandler.JsonOptions);
        Assert.DoesNotContain(requestMarkerValue, runtimeStatusJson, StringComparison.Ordinal);
        Assert.DoesNotContain(budgetMarkerValue, runtimeStatusJson, StringComparison.Ordinal);
        Assert.DoesNotContain("request_metadata:", runtimeStatusJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("budget_metadata:", runtimeStatusJson, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            runtimeStatus.Provenance.Keys,
            key => key.StartsWith("request_metadata:", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            runtimeStatus.Provenance.Keys,
            key => key.StartsWith("budget_metadata:", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            runtimeStatus.Job.Metadata.Keys,
            key => key.StartsWith("request_metadata:", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(
            runtimeStatus.Job.Metadata.Keys,
            key => key.StartsWith("budget_metadata:", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SubmitRoute_BrowserBackedRenderMissingRequiredViewportKeepsBrowserE2eFalse()
    {
        var root = NewRoot("browser-backed-missing-viewport");
        var api = CreateApi(
            new BrowserBackedDesignConversionRenderProvider(new LocalPngBrowserAdapter(root)),
            new PassingPngComparisonProvider(),
            utcNow: () => DateTimeOffset.Parse("2026-07-10T08:04:00Z"));
        var request = CreateRequest("browser-backed-missing-viewport");

        var submit = await api.SubmitAndEvaluateAsync(request with
        {
            Budget = request.Budget! with { MaxRenderQaRuns = 1 }
        });

        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        Assert.Equal(DesignConversionThinClientStatus.Quarantined, submit.Value!.ContractStatus);
        Assert.Equal(bool.TrueString, submit.Value.BoundaryMetadata["browser_render_executed"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["multi_viewport_complete"]);
        Assert.Equal(bool.FalseString, submit.Value.BoundaryMetadata["browser_e2e_verified"]);
        Assert.Equal("2", submit.Value.BoundaryMetadata["browser_e2e_required_viewport_count"]);
        Assert.Equal("1", submit.Value.BoundaryMetadata["browser_e2e_observed_viewport_count"]);
        Assert.Equal("1", submit.Value.BoundaryMetadata["browser_e2e_verified_render_artifact_count"]);
        Assert.Equal(
            "runtime_not_completed",
            submit.Value.BoundaryMetadata["browser_e2e_failure_reason"]);
        Assert.False(submit.Value.ProductReady);
    }

    [Fact]
    public async Task BrowserE2eStatusMatrix_OnlyCompletedServerOwnedRuntimeCanUseTrustedReceipts()
    {
        var root = NewRoot("browser-e2e-status-matrix");
        var capture = new CapturingEvaluator(new DesignConversionInternalServiceThinClientJobEvaluator(
            new DesignConversionInternalService(
                new BrowserBackedDesignConversionRenderProvider(new LocalPngBrowserAdapter(root)),
                new ServerOwnedLocalPngComparisonProvider(root),
                new DefaultLocalDesignConversionVisualJudge(),
                routingFlow: null)));
        var api = CreateApi(
            capture,
            utcNow: () => DateTimeOffset.Parse("2026-07-10T08:05:00Z"));

        var submit = await api.SubmitAndEvaluateAsync(CreateRequest(
            "browser-e2e-status-matrix",
            artifactRoot: root));

        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        Assert.NotNull(capture.Evaluation);
        var completed = BuildCompletedBrowserRuntimeContext(
            submit.Value!,
            capture.Evaluation!);
        var matrix = new[]
        {
            (DesignConversionThinClientStatus.NotReady, false),
            (DesignConversionThinClientStatus.Completed, true),
            (DesignConversionThinClientStatus.Quarantined, false),
            (DesignConversionThinClientStatus.Submitted, false),
            (DesignConversionThinClientStatus.Running, false),
            (DesignConversionThinClientStatus.Failed, false),
            (DesignConversionThinClientStatus.Cancelled, false)
        };
        foreach (var (status, expectedVerified) in matrix)
        {
            var runtimeStatus = completed.Status with
            {
                Job = completed.Status.Job with { Status = status },
                ContractStatus = status
            };
            var readiness = DesignConversionModelServerApi.EvaluateBrowserE2eReadinessForTesting(
                runtimeStatus,
                completed.Evaluation);
            Assert.Equal(expectedVerified.ToString(), readiness["verified"]);
            Assert.Equal(
                expectedVerified ? "none" : "runtime_not_completed",
                readiness["failure_reason"]);
            Assert.Equal(
                expectedVerified
                    ? "verified_by_server_owned_browser_render_receipts"
                    : "not_verified_by_server_owned_browser_render_receipts",
                readiness["state"]);
        }

        var noEvaluation = DesignConversionModelServerApi.EvaluateBrowserE2eReadinessForTesting(
            completed.Status,
            evaluation: null);
        Assert.Equal(bool.FalseString, noEvaluation["verified"]);
        Assert.Equal("not_evaluated_no_internal_evaluation_receipt", noEvaluation["failure_reason"]);
        Assert.Equal(
            "not_verified_by_server_owned_browser_render_receipts",
            noEvaluation["state"]);
    }

    [Theory]
    [InlineData(BrowserE2eEvidenceTamperMode.RequestBinding, "server_owned_browser_render_artifact_receipts_incomplete")]
    [InlineData(BrowserE2eEvidenceTamperMode.RunBinding, "server_owned_browser_render_artifact_receipts_incomplete")]
    [InlineData(BrowserE2eEvidenceTamperMode.ProviderBinding, "server_owned_browser_render_artifact_receipts_incomplete")]
    [InlineData(BrowserE2eEvidenceTamperMode.MissingReceipt, "server_owned_browser_render_artifact_receipts_incomplete")]
    [InlineData(BrowserE2eEvidenceTamperMode.DuplicateReceipt, "server_owned_browser_render_artifact_receipts_not_unique")]
    [InlineData(BrowserE2eEvidenceTamperMode.MissingCompletionEvidence, "runtime_not_completed")]
    [InlineData(BrowserE2eEvidenceTamperMode.TamperedCompletionEvidence, "runtime_not_completed")]
    public async Task BrowserE2eCompletedRuntime_RequestRunProviderAndCompletionEvidenceFailClosed(
        BrowserE2eEvidenceTamperMode mode,
        string expectedFailureReason)
    {
        var root = NewRoot($"browser-e2e-evidence-{mode}");
        var capture = new CapturingEvaluator(new DesignConversionInternalServiceThinClientJobEvaluator(
            new DesignConversionInternalService(
                new BrowserBackedDesignConversionRenderProvider(new LocalPngBrowserAdapter(root)),
                new ServerOwnedLocalPngComparisonProvider(root),
                new DefaultLocalDesignConversionVisualJudge(),
                routingFlow: null)));
        var api = CreateApi(
            capture,
            utcNow: () => DateTimeOffset.Parse("2026-07-10T08:06:00Z"));

        var submit = await api.SubmitAndEvaluateAsync(CreateRequest(
            $"browser-e2e-evidence-{mode}",
            artifactRoot: root));

        Assert.True(submit.IsSuccess, submit.ErrorMessage);
        Assert.NotNull(capture.Evaluation);
        var completed = BuildCompletedBrowserRuntimeContext(submit.Value!, capture.Evaluation!);
        var tampered = TamperCompletedBrowserRuntimeContext(completed, mode);

        var readiness = DesignConversionModelServerApi.EvaluateBrowserE2eReadinessForTesting(
            tampered.Status,
            tampered.Evaluation);

        Assert.Equal(bool.FalseString, readiness["verified"]);
        Assert.Equal(expectedFailureReason, readiness["failure_reason"]);
        Assert.Equal(
            "not_verified_by_server_owned_browser_render_receipts",
            readiness["state"]);
    }

    private static BrowserRuntimeContext BuildCompletedBrowserRuntimeContext(
        DesignConversionModelServerApiResult submit,
        DesignConversionInternalEvaluationResult evaluation)
    {
        var trace = evaluation.VisualQaBoundaryTrace;
        var acceptedScore = Math.Max(trace.TierPolicy.RequiredScore, 0.99);
        var completedEvaluation = evaluation with
        {
            VisualQaBoundaryTrace = trace with
            {
                TierPolicy = trace.TierPolicy with
                {
                    BestScore = acceptedScore,
                    StopReason = DesignConversionVisualQaBoundaryPolicy.QualityReached,
                    ShouldContinue = false,
                    AcceptanceScore = acceptedScore
                },
                VisualDecision = trace.VisualDecision with
                {
                    Decision = DesignConversionVisualQaBoundaryPolicy.DecisionAccepted,
                    QualityScore = acceptedScore,
                    QualityThresholdMet = true
                }
            }
        };
        var provenance = new Dictionary<string, string>(submit.RuntimeStatus.Provenance, StringComparer.Ordinal)
        {
            ["internal_evaluation_job_id"] = completedEvaluation.JobId,
            ["thin_client_terminal_status"] = DesignConversionThinClientStatus.Completed
        };
        var completedStatus = submit.RuntimeStatus with
        {
            Job = submit.RuntimeStatus.Job with { Status = DesignConversionThinClientStatus.Completed },
            ContractStatus = DesignConversionThinClientStatus.Completed,
            Provenance = provenance
        };
        var readiness = DesignConversionModelServerApi.EvaluateBrowserE2eReadinessForTesting(
            completedStatus,
            completedEvaluation);
        Assert.Equal(bool.TrueString, readiness["verified"]);
        Assert.Equal("none", readiness["failure_reason"]);
        return new BrowserRuntimeContext(completedStatus, completedEvaluation);
    }

    private static BrowserRuntimeContext TamperCompletedBrowserRuntimeContext(
        BrowserRuntimeContext context,
        BrowserE2eEvidenceTamperMode mode)
    {
        var status = context.Status;
        var evaluation = context.Evaluation;
        var renderResults = evaluation.RenderResults.ToArray();
        var first = renderResults[0];
        var receipt = first.BrowserRenderProofReceipt!;

        switch (mode)
        {
            case BrowserE2eEvidenceTamperMode.RequestBinding:
                renderResults[0] = first with
                {
                    BrowserRenderProofReceipt = receipt with { RequestId = "other-request" }
                };
                evaluation = evaluation with { RenderResults = renderResults };
                break;
            case BrowserE2eEvidenceTamperMode.RunBinding:
                renderResults[0] = first with
                {
                    Image = first.Image with
                    {
                        Metadata = ReplaceMetadata(first.Image.Metadata, "server_bound_job_id", "other-job")
                    }
                };
                evaluation = evaluation with { RenderResults = renderResults };
                break;
            case BrowserE2eEvidenceTamperMode.ProviderBinding:
                renderResults[0] = first with
                {
                    BrowserRenderProofReceipt = receipt with { IssuerKind = "untrusted-browser-provider" }
                };
                evaluation = evaluation with { RenderResults = renderResults };
                break;
            case BrowserE2eEvidenceTamperMode.MissingReceipt:
                renderResults[0] = first with { BrowserRenderProofReceipt = null };
                evaluation = evaluation with { RenderResults = renderResults };
                break;
            case BrowserE2eEvidenceTamperMode.DuplicateReceipt:
                evaluation = evaluation with
                {
                    RenderResults = evaluation.RenderResults.Concat([first]).ToArray()
                };
                break;
            case BrowserE2eEvidenceTamperMode.MissingCompletionEvidence:
                var missingProvenance = new Dictionary<string, string>(status.Provenance, StringComparer.Ordinal);
                missingProvenance.Remove("internal_evaluation_job_id");
                status = status with { Provenance = missingProvenance };
                break;
            case BrowserE2eEvidenceTamperMode.TamperedCompletionEvidence:
                evaluation = evaluation with
                {
                    VisualQaBoundaryTrace = evaluation.VisualQaBoundaryTrace with
                    {
                        TierPolicy = evaluation.VisualQaBoundaryTrace.TierPolicy with
                        {
                            StopReason = DesignConversionVisualQaBoundaryPolicy.ViewportOperationFailed
                        }
                    }
                };
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
        }

        return new BrowserRuntimeContext(status, evaluation);
    }

    private static IReadOnlyDictionary<string, string> ReplaceMetadata(
        IReadOnlyDictionary<string, string> source,
        string key,
        string value)
    {
        var result = new Dictionary<string, string>(source, StringComparer.Ordinal)
        {
            [key] = value
        };
        return result;
    }

    private static DesignConversionModelServerApi CreateApi(
        IDesignConversionRenderProvider renderProvider,
        IDesignConversionComparisonProvider comparisonProvider,
        Func<DateTimeOffset> utcNow)
    {
        var service = new DesignConversionInternalService(
            renderProvider,
            comparisonProvider,
            new DefaultLocalDesignConversionVisualJudge(),
            routingFlow: null);
        return CreateApi(
            new DesignConversionInternalServiceThinClientJobEvaluator(service),
            utcNow);
    }

    private static DesignConversionModelServerApi CreateApi(
        IDesignConversionThinClientJobEvaluator evaluator,
        Func<DateTimeOffset> utcNow)
    {
        return new DesignConversionModelServerApi(
            evaluator,
            utcNow,
            humanApprovalStore: new InMemoryDesignConversionHumanApprovalStore(),
            visualCostBudgetStore: new JsonlDesignConversionVisualCostBudgetStore(
                NewRoot("visual-cost-budget")));
    }

    private static DesignConversionModelServerApiRequest CreateRequest(
        string id,
        IReadOnlyDictionary<string, string>? requestMetadata = null,
        IReadOnlyDictionary<string, string>? artifactMetadata = null,
        string? artifactRoot = null)
    {
        return new DesignConversionModelServerApiRequest(
            RequestId: $"scaffold-boundary-{id}",
            SourceKind: DesignConversionSourceKind.Figma,
            DesignTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{id}:design-tree",
                Kind: "figma-json",
                Sha256: ShaRef($"{id}:design-tree"),
                Payload: "{\"type\":\"FRAME\",\"name\":\"Trust boundary\"}",
                Format: "figma-json"),
            OriginalTree: new DesignConversionModelServerPayloadRef(
                RefId: $"{id}:original-tree",
                Kind: "figma-json",
                Sha256: ShaRef($"{id}:original-tree"),
                Payload: "{\"type\":\"FRAME\",\"name\":\"Original\"}",
                Format: "figma-json"),
            Html: "<main class=\"screen\"><h1>Trust boundary</h1></main>",
            Css: ".screen{display:grid;color:#111827;background:#f8fafc;}",
            Screenshots:
            [
                Artifact(id, "desktop", 640, 480, artifactMetadata, artifactRoot),
                Artifact(id, "mobile", 390, 844, artifactMetadata, artifactRoot)
            ],
            DesignRenders: [],
            Budget: new DesignConversionModelServerBudget(
                UserId: "trust-boundary-user",
                Tier: "local",
                MaxImprovementCandidates: 1,
                MaxRenderQaRuns: 2,
                BudgetUsd: 0.10m),
            Metadata: requestMetadata);
    }

    private static DesignConversionModelServerArtifactRef Artifact(
        string id,
        string viewport,
        int width,
        int height,
        IReadOnlyDictionary<string, string>? artifactMetadata,
        string? artifactRoot)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["media_type"] = "image/png",
            ["content_type"] = "image/png",
            ["png_width"] = width.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["png_height"] = height.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["viewport_width"] = width.ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["viewport_height"] = height.ToString(System.Globalization.CultureInfo.InvariantCulture)
        };
        foreach (var pair in artifactMetadata ?? new Dictionary<string, string>())
            metadata[pair.Key] = pair.Value;

        var sha256 = string.IsNullOrWhiteSpace(artifactRoot)
            ? ShaRef($"{id}:expected:{viewport}")
            : WriteTargetPng(artifactRoot, id, viewport, width, height);
        return new DesignConversionModelServerArtifactRef(
            ArtifactId: $"{id}:expected:{viewport}",
            Kind: "screenshot",
            Width: width,
            Height: height,
            Uri: string.IsNullOrWhiteSpace(artifactRoot)
                ? $"artifact://design-conversion/scaffold-boundary/{id}/{viewport}.png"
                : sha256,
            Sha256: sha256,
            Metadata: metadata);
    }

    private static string WriteTargetPng(
        string artifactRoot,
        string id,
        string viewport,
        int width,
        int height)
    {
        var render = new DeterministicLocalDesignConversionPngRenderProvider(artifactRoot)
            .RenderAsync(new DesignConversionRenderRequest(
                RenderRequestId: $"target-{id}-{viewport}-png",
                RequestId: $"target-{id}",
                Html: $"<main class=\"screen\">{id}:{viewport}</main>",
                Css: "body{margin:0}.screen{background:#f8fafc;color:#111827}",
                Resolution: new RenderQaResolution($"target-{id}-{viewport}", width, height, 1.0),
                Metadata: new Dictionary<string, string>()))
            .GetAwaiter()
            .GetResult();
        if (!render.IsSuccess)
            throw new InvalidOperationException(render.ErrorMessage);

        return render.Value!.Image.Sha256;
    }

    private static DesignConversionHumanApprovalActor Actor(string tenantId, string actorId)
        => new(
            TenantId: tenantId,
            ActorId: actorId,
            Roles: [DesignConversionHumanApprovalService.RequiredApproverRole],
            Authenticated: true);

    private static string NewRoot(string name)
    {
        var path = Path.Combine(Path.GetTempPath(), "xiigen-scaffold-trust-boundary", name, Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ShaRef(string value)
        => "sha256:" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();

    private sealed class MutableClock(DateTimeOffset now)
    {
        private DateTimeOffset _now = now;

        public DateTimeOffset UtcNow() => _now;

        public void Advance(TimeSpan duration) => _now = _now.Add(duration);
    }

    private sealed class MarkerLossSyntheticPngRenderProvider : IDesignConversionRenderProvider
    {
        public Task<OperationResult<DesignConversionRenderResult>> RenderAsync(
            DesignConversionRenderRequest request,
            CancellationToken cancellationToken = default)
        {
            var image = new RenderQaImageRecord(
                ImageId: $"{request.RenderRequestId}:marker-loss-png",
                Label: "synthetic-png-without-scaffold-marker",
                Resolution: request.Resolution,
                Uri: $"artifact://design-conversion/scaffold-boundary/synthetic/{Uri.EscapeDataString(request.RenderRequestId)}.png",
                Sha256: ShaRef($"{request.RenderRequestId}:synthetic"),
                Metadata: new Dictionary<string, string>
                {
                    ["media_type"] = "image/png",
                    ["png_width"] = request.Resolution.Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["png_height"] = request.Resolution.Height.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["viewport_width"] = request.Resolution.Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["viewport_height"] = request.Resolution.Height.ToString(System.Globalization.CultureInfo.InvariantCulture)
                });

            return Task.FromResult(OperationResult<DesignConversionRenderResult>.Ok(new DesignConversionRenderResult(
                RenderResultId: $"{request.RenderRequestId}:marker-loss-result",
                RenderRequestId: request.RenderRequestId,
                Image: image,
                ElapsedMs: 1,
                Warnings: ["marker-loss-synthetic-render-provider"])));
        }
    }

    private sealed class LocalPngBrowserAdapter(string root) : IDesignConversionBrowserProcessAdapter
    {
        private readonly string _root = root;

        public async Task<OperationResult<DesignConversionBrowserCaptureResult>> CaptureScreenshotAsync(
            DesignConversionBrowserCaptureRequest request,
            CancellationToken cancellationToken = default)
        {
            var render = await new DeterministicLocalDesignConversionPngRenderProvider(_root)
                .RenderAsync(
                    new DesignConversionRenderRequest(
                        RenderRequestId: $"{request.CaptureRequestId}:browser-adapter-local-png",
                        RequestId: request.RequestId,
                        Html: request.DocumentHtml,
                        Css: "body{margin:0}.screen{background:#f8fafc;color:#111827}",
                        Resolution: request.Resolution,
                        Metadata: request.Metadata),
                    cancellationToken);
            if (!render.IsSuccess)
                return render.MapError<DesignConversionBrowserCaptureResult>();

            return OperationResult<DesignConversionBrowserCaptureResult>.Ok(new DesignConversionBrowserCaptureResult(
                ScreenshotUri: render.Value!.Image.Uri,
                ScreenshotSha256: render.Value.Image.Sha256["sha256:".Length..],
                ElapsedMs: 3,
                Metadata: new Dictionary<string, string>
                {
                    ["browser_adapter_kind"] = "local-png-browser-adapter",
                    ["browser_process_boundary"] = "test-adapter-supplied-png"
                },
                Warnings: []));
        }
    }

    private sealed class PassingPngComparisonProvider : IDesignConversionComparisonProvider
    {
        public List<DesignConversionQaComparisonRequest> Requests { get; } = [];

        public Task<OperationResult<DesignConversionQaComparisonArtifact>> CompareAsync(
            DesignConversionQaComparisonRequest request,
            CancellationToken cancellationToken = default)
        {
            Requests.Add(request);
            var verdict = new VisualVerdict(
                VerdictId: $"{request.ComparisonRequestId}:pass-verdict",
                Score: 0.96,
                Defects: [],
                EvidenceRefs: [$"comparison:{request.ComparisonRequestId}:passed"],
                Passed: true);

            var artifact = new DesignConversionQaComparisonArtifact(
                ArtifactId: $"{request.ComparisonRequestId}:comparison-artifact",
                ComparisonRequestId: request.ComparisonRequestId,
                Expected: request.Expected,
                Actual: request.Actual,
                Verdict: verdict,
                DiffImageUri: $"artifact://design-conversion/scaffold-boundary/diff/{Uri.EscapeDataString(request.ComparisonRequestId)}.png",
                DiffImageSha256: ShaRef($"{request.ComparisonRequestId}:diff"),
                Metrics: new Dictionary<string, double> { ["visual_distance"] = 0.04 },
                EvidenceRefs: [$"comparison-artifact:{request.ComparisonRequestId}"])
            {
                Metadata = new Dictionary<string, string>
                {
                    ["media_type"] = "image/png",
                    ["png_width"] = request.Actual.Resolution.Width.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["png_height"] = request.Actual.Resolution.Height.ToString(System.Globalization.CultureInfo.InvariantCulture),
                    ["provider_kind"] = "passing-png-comparison-provider"
                }
            };

            return Task.FromResult(OperationResult<DesignConversionQaComparisonArtifact>.Ok(artifact));
        }
    }

    private sealed class CapturingEvaluator(IDesignConversionThinClientJobEvaluator inner)
        : IDesignConversionThinClientJobEvaluator
    {
        private readonly IDesignConversionThinClientJobEvaluator _inner =
            inner ?? throw new ArgumentNullException(nameof(inner));

        public DesignConversionInternalEvaluationResult? Evaluation { get; private set; }

        public async Task<OperationResult<DesignConversionInternalEvaluationResult>> EvaluateAsync(
            DesignConversionInternalEvaluateRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _inner.EvaluateAsync(request, cancellationToken);
            if (result.IsSuccess)
                Evaluation = result.Value;
            return result;
        }
    }

    public enum BrowserE2eEvidenceTamperMode
    {
        RequestBinding,
        RunBinding,
        ProviderBinding,
        MissingReceipt,
        DuplicateReceipt,
        MissingCompletionEvidence,
        TamperedCompletionEvidence
    }

    private sealed record BrowserRuntimeContext(
        DesignConversionThinClientStatusResult Status,
        DesignConversionInternalEvaluationResult Evaluation);
}
